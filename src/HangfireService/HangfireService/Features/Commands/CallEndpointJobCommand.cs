using HangfireService.Clients.Contracts;
using MediatR;

namespace HangfireService.Features.Commands;

public class CallEndpointJobCommand : IRequest
{
	public string JobName { get; set; } = null!;
	public string ServiceName { get; set; } = null!;
	public string Url { get; set; } = null!;
}

public class CallEndpointJobHandler(ICallEndpointClient callEndpointClient) : IRequestHandler<CallEndpointJobCommand>
{
	public async Task Handle(CallEndpointJobCommand request, CancellationToken cancellationToken)
	{
		await callEndpointClient.GetMethodAsync(request.Url, cancellationToken);
	}
}
