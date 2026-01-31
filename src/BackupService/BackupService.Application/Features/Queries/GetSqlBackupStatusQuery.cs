using BackupService.Application.Models.Results;
using BackupService.Application.Services;
using Common.Mediator;
using Common.Presentation.Http;

namespace BackupService.Application.Features.Queries;

public class GetSqlBackupStatusQuery : IRequest<Result<GetSqlBackupStatusResult>>
{
    public string JobId { get; init; }
}

public class GetSqlBackupStatusQueryHandler(
    IBackupJobRunner jobRunner) : IRequestHandler<GetSqlBackupStatusQuery, Result<GetSqlBackupStatusResult>>
{
    public Task<Result<GetSqlBackupStatusResult>> Handle(
        GetSqlBackupStatusQuery query,
        CancellationToken cancellationToken)
    {
        var status = jobRunner.GetStatus(query.JobId);

        if (status is null)
        {
            return Task.FromResult(
                Result<GetSqlBackupStatusResult>.Fail(
                    Problems.NotFound($"Could not find job: {query.JobId}")));    
        }
        
        return Task.FromResult(
            Result<GetSqlBackupStatusResult>.Ok(
                new GetSqlBackupStatusResult { Status = status }));
    }
}