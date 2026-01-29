using BackupService.Application.Models;
using BackupService.Application.Settings;
using Common.Infrastructure.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackupService.Application.Services;

public sealed class SqlBackupService(
    IOptions<BackupSettings> backupOptions,
    IOptions<ConnectionStrings> connectionOptions,
    ILogger<SqlBackupService> logger) : ISqlBackupService
{
    private readonly BackupSettings settings = backupOptions.Value;
    private readonly ConnectionStrings connectionStrings = connectionOptions.Value;

    public async Task<SqlBackupResult> CreateBackupAsync(
        string? backupName = null,
        CancellationToken cancellationToken = default)
    {
        var connectionString = connectionStrings.DefaultConnection
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        var databaseName = GetDatabaseName(connectionString, settings.DatabaseName);
        var backupDirectory = ResolveBackupDirectory(settings.BackupDirectory);
        var filePrefix = SanitizeFileNamePart(backupName ?? settings.FilePrefix ?? databaseName);

        if (!settings.SkipLocalDirectoryCreation)
        {
            try
            {
                Directory.CreateDirectory(backupDirectory);
            }
            catch (UnauthorizedAccessException)
            {
                // The backup path may live on the SQL Server host/container filesystem.
                // In that case this service can't create it locally, so we just rely on
                // the directory already existing inside SQL Server.
                logger.LogWarning(
                    "Skipping local directory creation for backup path: {BackupDirectory}",
                    backupDirectory);
            }
        }

        var startedAt = DateTimeOffset.UtcNow;
        var fileName = $"{filePrefix}_{startedAt:yyyyMMdd_HHmmss}.bak";
        var backupFilePath = Path.Combine(backupDirectory, fileName);

        var sql = BuildBackupCommand(databaseName, backupFilePath, settings);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = settings.CommandTimeoutSeconds
        };

        await command.ExecuteNonQueryAsync(cancellationToken);

        var completedAt = DateTimeOffset.UtcNow;
        var sizeBytes = await TryGetBackupSizeFromSqlAsync(
            connection,
            databaseName,
            backupFilePath,
            cancellationToken);

        CleanupOldBackups(backupDirectory, filePrefix, settings.RetentionDays);

        logger.LogInformation(
            "SQL backup completed. Database={Database}, File={File}",
            databaseName,
            backupFilePath);

        return new SqlBackupResult
        {
            DatabaseName = databaseName,
            BackupFilePath = backupFilePath,
            StartedAtUtc = startedAt,
            CompletedAtUtc = completedAt,
            SizeBytes = sizeBytes
        };
    }

    private static string GetDatabaseName(string connectionString, string? configuredName)
    {
        if (!string.IsNullOrWhiteSpace(configuredName))
        {
            return configuredName.Trim();
        }

        var builder = new SqlConnectionStringBuilder(connectionString);
        
        if (!string.IsNullOrWhiteSpace(builder.InitialCatalog))
        {
            return builder.InitialCatalog;
        }

        throw new InvalidOperationException("DatabaseName is not configured and not found in the connection string.");
    }

    private static string ResolveBackupDirectory(string? backupDirectory)
    {
        if (!string.IsNullOrWhiteSpace(backupDirectory))
        {
            return backupDirectory;
        }

        return Path.Combine(AppContext.BaseDirectory, "backups");
    }

    // Alternative: SMO (SQL Server Management Objects) offers a typed API for backups,
    // but it still issues the same BACKUP DATABASE T-SQL under the hood.
    // In a containerized setup SMO adds heavy dependencies and versioning friction
    // without real functional gain for this simple, single-shot backup.
    // Plain T-SQL keeps the service lightweight and portable while remaining the canonical SQL Server backup mechanism.
    private static string BuildBackupCommand(string databaseName, string filePath, BackupSettings settings)
    {
        var escapedPath = filePath.Replace("'", "''");
        var options = new List<string> { "INIT" };

        if (settings.UseCopyOnly)
        {
            options.Add("COPY_ONLY");
        }

        if (settings.UseCompression)
        {
            options.Add("COMPRESSION");
        }

        return $"BACKUP DATABASE [{databaseName}] TO DISK = N'{escapedPath}' WITH {string.Join(", ", options)}";
    }

    private static async Task<long?> TryGetBackupSizeFromSqlAsync(
        SqlConnection connection,
        string databaseName,
        string backupFilePath,
        CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
                               SELECT TOP 1 b.backup_size
                               FROM msdb.dbo.backupset b
                               INNER JOIN msdb.dbo.backupmediafamily m
                                   ON b.media_set_id = m.media_set_id
                               WHERE b.database_name = @DatabaseName
                                 AND m.physical_device_name = @BackupFilePath
                               ORDER BY b.backup_finish_date DESC;
                               """;

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@DatabaseName", databaseName);
            command.Parameters.AddWithValue("@BackupFilePath", backupFilePath);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is null || result is DBNull ? null : Convert.ToInt64(result);
        }
        catch
        {
            return null;
        }
    }

    private static void CleanupOldBackups(string directory, string filePrefix, int retentionDays)
    {
        if (retentionDays <= 0)
        {
            return;
        }

        var cutoff =  DateTimeOffset.UtcNow.AddDays(-retentionDays);
        var pattern = $"{filePrefix}_*.bak";

        foreach (var file in Directory.EnumerateFiles(directory, pattern))
        {
            try
            {
                var info = new FileInfo(file);
                if (info.Exists && info.CreationTimeUtc < cutoff)
                {
                    info.Delete();
                }
            }
            catch
            {
                // best-effort cleanup
            }
        }
    }

    private static string SanitizeFileNamePart(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new char[value.Length];

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            sanitized[i] = Array.IndexOf(invalidChars, c) >= 0 ? '_' : c;
        }

        return new string(sanitized);
    }
}
