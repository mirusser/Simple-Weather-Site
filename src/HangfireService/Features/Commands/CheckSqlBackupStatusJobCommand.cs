using System.Text.Json;
using Common.Infrastructure.Consts;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Mediator;
using Common.Presentation.Http;
using HangfireService.Models;
using Hangfire;
using HangfireService.Features.Jobs;
using HangfireService.Settings;
using Microsoft.Extensions.Options;

namespace HangfireService.Features.Commands;

public sealed class CheckSqlBackupStatusJobCommand : IRequest<bool>
{
    public string JobId { get; set; } = null!;
    public int Attempt { get; set; }
}

public sealed class CheckSqlBackupStatusJobHandler(
    IHttpExecutor httpExecutor,
    IHttpRequestFactory requestFactory,
    IBackgroundJobClient backgroundJobs,
    IOptions<BackupJobSettings> backupJobOptions,
    ILogger<CheckSqlBackupStatusJobHandler> logger)
    : IRequestHandler<CheckSqlBackupStatusJobCommand, bool>
{
    private readonly BackupJobSettings settings = backupJobOptions.Value;

    public async Task<bool> Handle(CheckSqlBackupStatusJobCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Checking SQL backup status. JobId={JobId}, Attempt={Attempt}/{MaxAttempts}",
            request.JobId,
            request.Attempt,
            settings.MaxPollAttempts);

        var statusBody = JsonSerializer.Serialize(new { jobId = request.JobId });

        using var statusRequest = requestFactory.Create(
            settings.StatusUrl,
            HttpMethod.Post.Method,
            statusBody,
            null);

        var statusResponse = await httpExecutor.SendAsync(
            HttpClientConsts.DefaultName,
            PipelineNames.Default,
            statusRequest,
            cancellationToken);

        var statusResult = await HttpResult.ReadJsonAsResultAsync<GetSqlBackupStatusResultDto>(
            statusResponse,
            cancellationToken);

        if (!statusResult.IsSuccess)
        {
            logger.LogWarning(
                "Status check failed. JobId={JobId}, Attempt={Attempt}, Error={Error}",
                request.JobId,
                request.Attempt,
                statusResult.Problem?.Message);
            // Rely on resilience pipeline + reschedule until attempts are exhausted.
            return RescheduleOrFail(request);
        }

        var status = statusResult.Value?.Status;
        if (status is null)
        {
            logger.LogWarning(
                "Status check returned empty status. JobId={JobId}, Attempt={Attempt}",
                request.JobId,
                request.Attempt);
            return RescheduleOrFail(request);
        }

        if (status.State == BackupJobState.Succeeded)
        {
            logger.LogInformation(
                "SQL backup succeeded. JobId={JobId}",
                request.JobId);
            return true;
        }

        if (status.State == BackupJobState.Failed)
        {
            logger.LogError(
                "SQL backup failed. JobId={JobId}, Error={Error}",
                request.JobId,
                status.Error);
            throw new InvalidOperationException(
                $"Backup failed: {status.Error ?? "Unknown error"}");
        }

        logger.LogInformation(
            "SQL backup still in progress. JobId={JobId}, State={State}",
            request.JobId,
            status.State);
        
        return RescheduleOrFail(request);
    }

    private bool RescheduleOrFail(CheckSqlBackupStatusJobCommand request)
    {
        if (request.Attempt >= settings.MaxPollAttempts)
        {
            throw new TimeoutException(
                $"Backup status did not complete within {settings.MaxPollAttempts} attempts.");
        }

        backgroundJobs.Schedule<HangfireMediatorExecutor>(
            x => x.ExecuteNamed(
                settings.JobCheckStatusName, 
                new CheckSqlBackupStatusJobCommand
                {
                    JobId = request.JobId,
                    Attempt = request.Attempt + 1
                }),
            TimeSpan.FromSeconds(settings.PollIntervalSeconds));

        return true;
    }
}