using Common.Mediator;
using MassTransit;
using Common.Shared.Jobs;

namespace HangfireService.Features.Commands;

public class RunJobCommand : IRequest<bool>
{
	public string JobName { get; set; } = null!;
	public string ServiceName { get; set; } = null!;
}

public class RunJobHandler(IPublishEndpoint publishEndpoint) : IRequestHandler<RunJobCommand, bool>
{
	public async Task<bool> Handle(RunJobCommand request, CancellationToken cancellationToken)
	{
		await publishEndpoint.Publish<IJobMessage>(
			new { JobName = request.JobName, ServiceName = request.ServiceName },
			cancellationToken);
		
		return true;
	}
}