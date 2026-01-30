using Common.Mediator;
using Hangfire;
using HangfireService.Models;

namespace HangfireService.Features.Commands;

public class RegisterJobCommand : IRequest<bool>
{
	public string JobName { get; set; } = null!;
	public string ServiceName { get; set; } = null!;
	public string CronExpression { get; set; } = null!;
	public JobType JobType { get; set; }
	public string? Url { get; set; }
	public string? HttpMethod { get; set; }
	public string? BodyJson { get; set; }
	public Dictionary<string, string>? Headers { get; set; }
}

public class RegisterJobHandler(
	IRecurringJobManager recurringJobManager,
	IMediator mediator,
	ILogger<RegisterJobHandler> logger) : IRequestHandler<RegisterJobCommand, bool>
{
	public Task<bool> Handle(RegisterJobCommand request, CancellationToken cancellationToken)
	{
		var jobCommand = GetJobCommand(
			request.JobType,
			request.JobName,
			request.ServiceName,
			request.Url,
			request.HttpMethod,
			request.BodyJson,
			request.Headers);

		recurringJobManager.AddOrUpdate(
			request.JobName,
			() => mediator.SendAsync(jobCommand, cancellationToken),
			request.CronExpression);

		logger.LogInformation(
			"Added (or updated) job; name:{Name}, service:{ServiceName}, url: {Url}",
			request.JobName,
			request.ServiceName,
			request.Url);
		
		return Task.FromResult(true);
	}

	private IRequest<bool> GetJobCommand(
		JobType jobType,
		string jobName,
		string serviceName,
		string? url,
		string? httpMethod,
		string? bodyJson,
		Dictionary<string, string>? headers) 
		=> jobType switch
		{
			JobType.RunJob => new RunJobCommand() { JobName = jobName, ServiceName = serviceName },
			JobType.CallHealthCheck => new CallHealthCheckJobCommand()
				{ JobName = jobName, ServiceName = serviceName, Url = url ?? "" },
			JobType.CallEndpointHttpJob => new CallEndpointHttpJobCommand()
			{
				JobName = jobName,
				ServiceName = serviceName,
				Url = url ?? "",
				HttpMethod = httpMethod ?? "POST",
				BodyJson = bodyJson,
				Headers = headers
			},
			_ => throw new ArgumentException($"Wrong argument: {nameof(JobType)}, value: {jobType}"),
		};
}
