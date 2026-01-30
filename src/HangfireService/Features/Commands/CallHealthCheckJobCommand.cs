using System.Text.Json;
using Common.Contracts.HealthCheck;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Mediator;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MQModels.Email;
using MassTransit;
using Microsoft.Extensions.Options;
using HangfireService.Settings;

namespace HangfireService.Features.Commands;

public class CallHealthCheckJobCommand : IRequest<bool>
{
	public string JobName { get; set; } = null!;
	public string ServiceName { get; set; } = null!;
	public string Url { get; set; } = null!;
}

public class CallHealthCheckJobHandler(
	IHttpExecutor httpExecutor,
	IHttpRequestFactory requestFactory,
	IPublishEndpoint publishEndpoint,
	IOptions<MailSettings> options,
	JsonSerializerOptions jsonSerializerOptions) : IRequestHandler<CallHealthCheckJobCommand, bool>
{
	private readonly MailSettings mailSettings = options.Value;
	private const string ClientName = "default";
	
	public async Task<bool> Handle(CallHealthCheckJobCommand request, CancellationToken cancellationToken)
	{
		using var httpRequest = requestFactory.Create(
			request.Url,
			HttpMethod.Get.Method);

		var response = await httpExecutor.SendAsync(
			ClientName,
			PipelineNames.Health,
			httpRequest,
			cancellationToken);
		response.EnsureSuccessStatusCode();

		var content = await response.Content.ReadAsStringAsync(cancellationToken);
		var healthCheckResponse = JsonSerializer.Deserialize<HealthCheckResponse>(content, jsonSerializerOptions);

		if (healthCheckResponse?.Status == nameof(HealthStatus.Healthy))
		{
			SendEmail sendEmailModel = new()
			{
				Subject = "HealthCheck",
				Body = content
			};
			await publishEndpoint.Publish(sendEmailModel, cancellationToken);
		}

		return true;
	}
}
