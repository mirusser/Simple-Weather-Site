using System;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CitiesService.Infrastructure.Database;

public sealed class SqlServerSeedLockProvider(ApplicationDbContext context) : ISeedLockProvider
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
            cmd.CommandText =
                """
                DECLARE @result int;
                EXEC @result = sp_getapplock
                    @Resource = @resource,
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Session',
                    @LockTimeout = 0;
                SELECT @result;
                """;
            cmd.CommandType = System.Data.CommandType.Text;

            var p = cmd.CreateParameter();
            p.ParameterName = "@resource";
            p.Value = resource;
            cmd.Parameters.Add(p);

            var resultObj = await cmd.ExecuteScalarAsync(cancellationToken);
            var result = Convert.ToInt32(resultObj);

            if (result >= 0)
            {
                return new SqlServerSeedLockLease(context, resource);
            }

            context.Database.CloseConnection();
            return null;
        }
        catch
        {
            context.Database.CloseConnection();
            throw;
        }
    }

    private sealed class SqlServerSeedLockLease(
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
                cmd.CommandText =
                    """
                    EXEC sp_releaseapplock
                        @Resource = @resource,
                        @LockOwner = 'Session';
                    """;
                cmd.CommandType = System.Data.CommandType.Text;

                var p = cmd.CreateParameter();
                p.ParameterName = "@resource";
                p.Value = resource;
                cmd.Parameters.Add(p);

                await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                context.Database.CloseConnection();
            }
        }
    }
}
