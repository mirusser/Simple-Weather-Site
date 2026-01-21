using System.Threading.Tasks;
using Common.Shared.Jobs;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CitiesService.Application.Features.Listeners;

public class JobListener(ILogger<JobListener> logger) : IConsumer<IJobMessage>
{
	public async Task Consume(ConsumeContext<IJobMessage> context)
	{
		if (context?.Message is not null)
		{
			//check using 'ServiceName' and 'JobName' if it's your job to execute
			//then execute said job, if not just do nothing

			logger.LogInformation("Listener1: Received event {EventName}, job name: {JobName}, service name: {ServiceName}", nameof(IJobMessage), context.Message.JobName, context.Message.ServiceName);
		}
		else
		{
			logger.LogWarning("Listener1: Received event {EventName} is null.", nameof(IJobMessage));
		}

		await Task.CompletedTask;
	}
}