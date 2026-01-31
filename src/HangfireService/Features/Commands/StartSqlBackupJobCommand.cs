using System.Text.Json;
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

public sealed class StartSqlBackupJobCommand : IRequest<bool>
{
    public Dictionary<string, string>? Headers { get; set; }
    public string ClientName { get; set; } = "default";
}

public sealed class StartSqlBackupJobHandler(
    IHttpExecutor httpExecutor,
    IHttpRequestFactory requestFactory,
    IBackgroundJobClient backgroundJobs,
    JsonSerializerOptions jsonSerializerOptions,
    ILogger<StartSqlBackupJobHandler> logger,
    IOptions<BackupJobSettings> backupJobOptions)
    : IRequestHandler<StartSqlBackupJobCommand, bool>
{
    public async Task<bool> Handle(StartSqlBackupJobCommand request, CancellationToken cancellationToken)
    {
        var settings = backupJobOptions.Value;

        var startBody = JsonSerializer.Serialize(
            new { settings.BackupName },
            jsonSerializerOptions);

        using var startRequest = requestFactory.Create(
            settings.StartUrl,
            HttpMethod.Post.Method,
            startBody,
            request.Headers);

        var startResponse = await httpExecutor.SendAsync(
            request.ClientName,
            PipelineNames.Default,
            startRequest,
            cancellationToken);

        if (!startResponse.IsSuccessStatusCode)
        {
            var statusCode = (int)startResponse.StatusCode;
            logger.LogError(
                "StartSqlBackup failed. JobName={JobName}, Status={Status}",
                settings.JobName,
                statusCode);
            
            throw new InvalidOperationException($"StartSqlBackup failed with HTTP {statusCode}.");
        }

        var startResult = await HttpResult.ReadJsonAsResultAsync<StartSqlBackupResultDto>(
            startResponse,
            cancellationToken);

        if (!startResult.IsSuccess || startResult.Value is null || string.IsNullOrWhiteSpace(startResult.Value.JobId))
        {
            logger.LogError(
                "StartSqlBackup returned invalid payload. JobName={JobName}, Error={Error}",
                settings.JobName,
                startResult.Problem?.Message ?? "empty JobId");
            
            throw new InvalidOperationException(
                $"StartSqlBackup failed: {startResult.Problem?.Message ?? "empty JobId"}");
        }

        var jobId = startResult.Value.JobId;

        backgroundJobs.Schedule<HangfireMediatorExecutor>(
            x => x.Execute(new CheckSqlBackupStatusJobCommand
            {
                JobName = settings.JobName,
                StatusUrl = settings.StatusUrl,
                JobId = jobId,
                Attempt = 1,
                PollIntervalSeconds = settings.PollIntervalSeconds,
                MaxPollAttempts = settings.MaxPollAttempts,
                Headers = request.Headers,
                ClientName = request.ClientName
            }),
            TimeSpan.FromSeconds(settings.PollIntervalSeconds));

        return true;
    }

}
