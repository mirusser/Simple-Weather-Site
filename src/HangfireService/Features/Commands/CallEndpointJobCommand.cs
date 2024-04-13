using HangfireService.Clients.Contracts;
using MediatR;

namespace HangfireService.Features.Commands;

public class CallEndpointJobCommand : IRequest
{
	public string JobName { get; set; } = null!;
	public string ServiceName { get; set; } = null!;
	public string Url { get; set; } = null!;
}

public class CallEndpointJobHandler(IHangfireHttpClient callEndpointClient) : IRequestHandler<CallEndpointJobCommand>
{
	// TODO: implement handling the response
	public async Task Handle(CallEndpointJobCommand request, CancellationToken cancellationToken)
	{
		var response = await callEndpointClient.GetMethodAsync(request.Url, cancellationToken);
	}
}
