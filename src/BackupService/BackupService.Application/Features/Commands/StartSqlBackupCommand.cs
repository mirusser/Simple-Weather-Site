using BackupService.Application.Models.Results;
using BackupService.Application.Services;
using Common.Mediator;
using Common.Presentation.Http;
using Microsoft.Extensions.Logging;

namespace BackupService.Application.Features.Commands;

public class StartSqlBackupCommand : IRequest<Result<StartSqlBackupResult>>
{
    public string? BackupName { get; init; }
}

public sealed class StartSqlBackupHandler(
    IBackupJobRunner jobRunner,
    ILogger<StartSqlBackupHandler> logger) : IRequestHandler<StartSqlBackupCommand, Result<StartSqlBackupResult>>
{
    public async Task<Result<StartSqlBackupResult>> Handle(
        StartSqlBackupCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing: {Request}", nameof(StartSqlBackupCommand));

        var jobId = await jobRunner.StartSqlBackupAsync(request.BackupName, cancellationToken);

        logger.LogInformation("Finished: {Request} with JobId: {JobId}", nameof(StartSqlBackupCommand), jobId);

        return Result<StartSqlBackupResult>.Ok(new StartSqlBackupResult { JobId = jobId });
    }
}