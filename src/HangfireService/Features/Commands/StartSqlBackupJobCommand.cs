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

public sealed class StartSqlBackupJobCommand : IRequest<bool>;

public sealed class StartSqlBackupJobHandler(
    IHttpExecutor httpExecutor,
    IHttpRequestFactory requestFactory,
    IBackgroundJobClient backgroundJobs,
    JsonSerializerOptions jsonSerializerOptions,
    ILogger<StartSqlBackupJobHandler> logger,
    IOptions<BackupJobSettings> backupJobOptions,
    IHangfireJobContextAccessor jobContextAccessor)
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
            null);

        var startResponse = await httpExecutor.SendAsync(
            HttpClientConsts.DefaultName,
            PipelineNames.Default,
            startRequest,
            cancellationToken);

        var responseBody = await startResponse.Content.ReadAsStringAsync(cancellationToken);
        SetJobParameter(jobContextAccessor, "startResponse", responseBody);
        logger.LogInformation("StartSqlBackup response: {Response}", Truncate(responseBody));

        if (!startResponse.IsSuccessStatusCode)
        {
            var statusCode = (int)startResponse.StatusCode;
            logger.LogError(
                "StartSqlBackup failed. JobName={JobName}, Status={Status}",
                settings.JobStartName,
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
                settings.JobStartName,
                startResult.Problem?.Message ?? "empty JobId");

            throw new InvalidOperationException(
                $"StartSqlBackup failed: {startResult.Problem?.Message ?? "empty JobId"}");
        }

        var jobId = startResult.Value.JobId;

        backgroundJobs.Schedule<HangfireMediatorExecutor>(
            x => x.ExecuteNamed(
                settings.JobCheckStatusName,
                new CheckSqlBackupStatusJobCommand
                {
                    JobId = jobId,
                    Attempt = 1
                }),
            TimeSpan.FromSeconds(settings.PollIntervalSeconds));

        return true;
    }

    private static void SetJobParameter(
        IHangfireJobContextAccessor accessor,
        string key,
        string? value)
    {
        accessor.Context?.SetJobParameter(key, Truncate(value));
    }

    private static string? Truncate(string? value, int maxLength = 4000)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }
}