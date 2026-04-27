using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Infrastructure.Database;

public sealed class PostgreSqlSeedLockProvider(ApplicationDbContext context) : ISeedLockProvider
{
    public async Task<ISeedLockLease?> TryAcquireAsync(
        string resource,
        CancellationToken cancellationToken = default)
    {
        var conn = context.Database.GetDbConnection();
        await context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT pg_try_advisory_lock(hashtext(@resource));";
            cmd.CommandType = System.Data.CommandType.Text;

            var p = cmd.CreateParameter();
            p.ParameterName = "resource";
            p.Value = resource;
            cmd.Parameters.Add(p);

            var resultObj = await cmd.ExecuteScalarAsync(cancellationToken);
            if (resultObj is true)
            {
                return new PostgreSqlSeedLockLease(context, resource);
            }

            await context.Database.CloseConnectionAsync();
            return null;
        }
        catch
        {
            await context.Database.CloseConnectionAsync();
            throw;
        }
    }

    private sealed class PostgreSqlSeedLockLease(
        ApplicationDbContext context,
        string resource) : ISeedLockLease
    {
        private bool disposed;

        public async ValueTask DisposeAsync()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            try
            {
                var conn = context.Database.GetDbConnection();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT pg_advisory_unlock(hashtext(@resource));";
                cmd.CommandType = System.Data.CommandType.Text;

                var p = cmd.CreateParameter();
                p.ParameterName = "resource";
                p.Value = resource;
                cmd.Parameters.Add(p);

                await cmd.ExecuteScalarAsync();
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }
    }
}
