using System.Text.Json;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Mediator;
using Common.Presentation.Http;
using HangfireService.Models;
using Hangfire;
using HangfireService.Features.Jobs;

namespace HangfireService.Features.Commands;

public sealed class CheckSqlBackupStatusJobCommand : IRequest<bool>
{
	public string JobName { get; set; } = null!;
	public string StatusUrl { get; set; } = null!;
	public string JobId { get; set; } = null!;
    public int Attempt { get; set; }
    public int PollIntervalSeconds { get; set; } = 60;
    public int MaxPollAttempts { get; set; } = 60;
    public Dictionary<string, string>? Headers { get; set; }
    public string ClientName { get; set; } = "default";
}

public sealed class CheckSqlBackupStatusJobHandler(
    IHttpExecutor httpExecutor,
    IHttpRequestFactory requestFactory,
    IBackgroundJobClient backgroundJobs,
    ILogger<CheckSqlBackupStatusJobHandler> logger)
    : IRequestHandler<CheckSqlBackupStatusJobCommand, bool>
{
    public async Task<bool> Handle(CheckSqlBackupStatusJobCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Checking SQL backup status. JobId={JobId}, Attempt={Attempt}/{MaxAttempts}",
            request.JobId,
            request.Attempt,
            request.MaxPollAttempts);

        var statusBody = JsonSerializer.Serialize(new { jobId = request.JobId });

        using var statusRequest = requestFactory.Create(
            request.StatusUrl,
            HttpMethod.Post.Method,
            statusBody,
            request.Headers);

        var statusResponse = await httpExecutor.SendAsync(
            request.ClientName,
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
            return RescheduleOrFail(request, backgroundJobs);
        }

        var status = statusResult.Value?.Status;
        if (status is null)
        {
            logger.LogWarning(
                "Status check returned empty status. JobId={JobId}, Attempt={Attempt}",
                request.JobId,
                request.Attempt);
            return RescheduleOrFail(request, backgroundJobs);
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
        return RescheduleOrFail(request, backgroundJobs);
    }

    private static bool RescheduleOrFail(CheckSqlBackupStatusJobCommand request, IBackgroundJobClient backgroundJobs)
    {
        if (request.Attempt >= request.MaxPollAttempts)
        {
            throw new TimeoutException(
                $"Backup status did not complete within {request.MaxPollAttempts} attempts.");
        }

        backgroundJobs.Schedule<HangfireMediatorExecutor>(
            x => x.Execute(new CheckSqlBackupStatusJobCommand
            {
                JobName = request.JobName,
                StatusUrl = request.StatusUrl,
                JobId = request.JobId,
                Attempt = request.Attempt + 1,
                PollIntervalSeconds = request.PollIntervalSeconds,
                MaxPollAttempts = request.MaxPollAttempts,
                Headers = request.Headers,
                ClientName = request.ClientName
            }),
            TimeSpan.FromSeconds(request.PollIntervalSeconds));

        return true;
    }
}
