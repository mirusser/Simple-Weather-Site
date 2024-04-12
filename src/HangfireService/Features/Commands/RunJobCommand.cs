using MassTransit;
using MediatR;
using Common.Shared.Jobs;

namespace HangfireService.Features.Commands;

public class RunJobCommand : IRequest
{
	public string JobName { get; set; } = null!;
	public string ServiceName { get; set; } = null!;
}

public class RunJobHandler(IPublishEndpoint publishEndpoint) : IRequestHandler<RunJobCommand>
{
	public async Task Handle(RunJobCommand request, CancellationToken cancellationToken)
	{
		await publishEndpoint.Publish<IJobMessage>(
			new { JobName = request.JobName, ServiceName = request.ServiceName },
			cancellationToken);
	}
}