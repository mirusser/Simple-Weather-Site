using BackupService.Application.Models.Results;
using BackupService.Application.Services;
using Common.Mediator;
using Common.Presentation.Http;
using Microsoft.Extensions.Logging;

namespace BackupService.Application.Features.Queries;

public class GetSqlBackupStatusQuery : IRequest<Result<GetSqlBackupStatusResult>>
{
    public required string JobId { get; init; }
}

public class GetSqlBackupStatusQueryHandler(
    IBackupJobRunner jobRunner,
    ILogger<GetSqlBackupStatusQueryHandler> logger) : IRequestHandler<GetSqlBackupStatusQuery, Result<GetSqlBackupStatusResult>>
{
    public Task<Result<GetSqlBackupStatusResult>> Handle(
        GetSqlBackupStatusQuery query,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing: {Query} with {JobId}", nameof(GetSqlBackupStatusQuery), query.JobId);
        
        var status = jobRunner.GetStatus(query.JobId);

        if (status is null)
        {
            logger.LogWarning("Status for Job: {JobId} not found", query.JobId);
            
            return Task.FromResult(
                Result<GetSqlBackupStatusResult>.Fail(
                    Problems.NotFound($"Could not find job: {query.JobId}")));    
        }
        
        logger.LogInformation("Job {JobId}: {Status}", query.JobId,  status);
        
        return Task.FromResult(
            Result<GetSqlBackupStatusResult>.Ok(
                new GetSqlBackupStatusResult { Status = status }));
    }
}