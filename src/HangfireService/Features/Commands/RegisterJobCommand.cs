using Hangfire;
using HangfireService.Models;
using MediatR;

namespace HangfireService.Features.Commands;

public class RegisterJobCommand : IRequest
{
	public string JobName { get; set; } = null!;
	public string ServiceName { get; set; } = null!;
	public string CronExpression { get; set; } = null!;
	public JobType JobType { get; set; }
	public string? Url { get; set; }
}

public class RegisterJobHandler(
	IRecurringJobManager recurringJobManager,
	IMediator mediator,
	ILogger<RegisterJobHandler> logger) : IRequestHandler<RegisterJobCommand>
{
	public Task Handle(RegisterJobCommand request, CancellationToken cancellationToken)
	{
		var jobCommand = GetJobCommand(
			request.JobType,
			request.JobName,
			request.ServiceName,
			request.Url);

		recurringJobManager.AddOrUpdate(
			request.JobName,
			() => mediator.Send(jobCommand, cancellationToken),
			request.CronExpression);

		logger.LogInformation(
			"Added (or updated) job; name:{Name}, service:{ServiceName}, url: {Url}",
			request.JobName,
			request.ServiceName,
			request.Url);
		return Task.CompletedTask;
	}

	private IRequest GetJobCommand(JobType jobType, string jobName, string serviceName, string? url)
		=> jobType switch
		{
			JobType.RunJob => new RunJobCommand() { JobName = jobName, ServiceName = serviceName },
			JobType.CallEndpointJob => new CallEndpointJobCommand() { JobName = jobName, ServiceName = serviceName, Url = url ?? "" },
			JobType.CallHealthCheck => new CallHealthCheckJobCommand() { JobName = jobName, ServiceName = serviceName, Url = url ?? "" },
			_ => throw new ArgumentException($"Wrong argument: {nameof(JobType)}, value: {jobType}"),
		};
}