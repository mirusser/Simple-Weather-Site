
// request
// handler
// mediator
// request => mediator => handler => response

using Common.Mediator;
using Common.Mediator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var serviceProvider = new ServiceCollection()
    .AddLogging(b => b.AddConsole())
    //.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
    .AddMediator(typeof(Program).Assembly)
    .BuildServiceProvider();

PrintToConsoleRequest request = new() { Text = "Hello from Mediator"};

var mediator = serviceProvider.GetRequiredService<IMediator>();

await mediator.SendAsync(request);

public class PrintToConsoleRequest : IRequest<bool>
{
    public string Text { get; init; }
}

public class PrintToConsoleHandler : IRequestHandler<PrintToConsoleRequest, bool>
{
    public Task<bool> HandleAsync(PrintToConsoleRequest request)
    {
        Console.WriteLine(request.Text);
        
        return Task.FromResult(true);
    }

    public Task<bool> Handle(PrintToConsoleRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine(request.Text);
        
        return Task.FromResult(true);
    }
}