using System.Text.Json;
using HangfireService.Clients.Contracts;
using MediatR;
using Common.Contracts.HealthCheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MQModels.Email;
using MassTransit;
using Microsoft.Extensions.Options;
using HangfireService.Settings;

namespace HangfireService.Features.Commands;

public class CallHealthCheckJobCommand : IRequest
{
	public string JobName { get; set; } = null!;
	public string ServiceName { get; set; } = null!;
	public string Url { get; set; } = null!;
}

public class CallHealthCheckJobHandler(
	IHangfireHttpClient hangfireHttpClient,
	IPublishEndpoint publishEndpoint,
	IOptions<MailSettings> options,
	JsonSerializerOptions jsonSerializerOptions) : IRequestHandler<CallHealthCheckJobCommand>
{
	private readonly MailSettings mailSettings = options.Value;
	public async Task Handle(CallHealthCheckJobCommand request, CancellationToken cancellationToken)
	{
		var response = await hangfireHttpClient.GetMethodAsync(request.Url, cancellationToken);
		response.EnsureSuccessStatusCode();

		var content = await response.Content.ReadAsStringAsync(cancellationToken);
		var healthCheckResponse = JsonSerializer.Deserialize<HealthCheckResponse>(content, jsonSerializerOptions);

		if (healthCheckResponse?.Status == HealthStatus.Healthy.ToString())
		{
			SendEmail sendEmailModel = new()
			{
				Subject = "HealthCheck",
				Body = content
			};
			await publishEndpoint.Publish(sendEmailModel, cancellationToken);
		}
	}
}