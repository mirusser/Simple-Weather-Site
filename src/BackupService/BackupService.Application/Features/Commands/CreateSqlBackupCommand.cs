using BackupService.Application.Models;
using BackupService.Application.Services;
using Common.Mediator;
using Common.Presentation.Http;
using Microsoft.Extensions.Logging;

namespace BackupService.Application.Features.Commands;

public sealed class CreateSqlBackupCommand : IRequest<Result<SqlBackupResult>>
{
    public string? BackupName { get; set; }
}

public sealed class CreateSqlBackupHandler(
    ISqlBackupService backupService,
    ILogger<CreateSqlBackupHandler> logger) : IRequestHandler<CreateSqlBackupCommand, Result<SqlBackupResult>>
{
    public async Task<Result<SqlBackupResult>> Handle(
        CreateSqlBackupCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await backupService.CreateBackupAsync(
                request.BackupName,
                cancellationToken);

            return Result<SqlBackupResult>.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SQL backup failed.");
            return Result<SqlBackupResult>.Fail(
                Problems.ServiceUnavailable("SQL backup failed."));
        }
    }
}
