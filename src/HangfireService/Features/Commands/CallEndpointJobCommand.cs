using Common.Mediator;
using HangfireService.Clients.Contracts;

namespace HangfireService.Features.Commands;

public class CallEndpointJobCommand : IRequest<bool>
{
    public string JobName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string Url { get; set; } = null!;
}

public class CallEndpointJobHandler(IHangfireHttpClient callEndpointClient)
    : IRequestHandler<CallEndpointJobCommand, bool>
{
    // TODO: implement handling the response
    public async Task<bool> Handle(CallEndpointJobCommand request, CancellationToken cancellationToken)
    {
        var response = await callEndpointClient.GetMethodAsync(request.Url, cancellationToken);

        return true;
    }
}