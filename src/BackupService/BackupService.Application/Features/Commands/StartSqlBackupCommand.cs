using BackupService.Application.Models.Results;
using BackupService.Application.Services;
using Common.Mediator;
using Common.Presentation.Http;
using Microsoft.Extensions.Logging;

namespace BackupService.Application.Features.Commands;

public class StartSqlBackupCommand : IRequest<Result<StartSqlBackupResult>>
{
    public string? BackupName { get; set; }
}

public sealed class StartSqlBackupHandler(
    IBackupJobRunner jobRunner,
    ILogger<StartSqlBackupHandler> logger) : IRequestHandler<StartSqlBackupCommand, Result<StartSqlBackupResult>>
{
    public async Task<Result<StartSqlBackupResult>> Handle(
        StartSqlBackupCommand request, 
        CancellationToken cancellationToken)
    {
        var jobId = await jobRunner.StartSqlBackupAsync(request.BackupName, cancellationToken);
        
        return Result<StartSqlBackupResult>.Ok(new StartSqlBackupResult{ JobId = jobId});
    }
}