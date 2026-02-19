using CitiesService.Application.Features.Listeners;
using Common.Shared.Jobs;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CitiesService.IntegrationTests.MassTransit;

/// <summary>
/// Validates MassTransit wiring at the message/consumer boundary using the in-memory test harness.
/// This gives confidence consumers can be resolved and invoked without requiring RabbitMQ.
/// </summary>
public class JobListenerMassTransitIntegrationTests
{
    [Fact]
    public async Task JobListener_Consumes_IJobMessage()
    {
        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddMassTransit(cfg =>
            {
                cfg.AddConsumer<JobListener>();
                cfg.UsingInMemory((ctx, busCfg) =>
                {
                    busCfg.ConfigureEndpoints(ctx);
                });
            })
            .BuildServiceProvider(true);

        var bus = provider.GetRequiredService<IBusControl>();
        var tracker = new ConsumeTracker();
        bus.ConnectConsumeObserver(tracker);

        await bus.StartAsync(CancellationToken.None);
        try
        {
            await bus.Publish<IJobMessage>(new TestJobMessage
            {
                JobName = "Example",
                ServiceName = "CitiesService"
            }, CancellationToken.None);

            var consumed = await tracker.WaitForConsume(TimeSpan.FromSeconds(5));
            Assert.True(consumed);
        }
        finally
        {
            await bus.StopAsync(CancellationToken.None);
        }
    }

    private sealed class TestJobMessage : IJobMessage
    {
        public string JobName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
    }

    private sealed class ConsumeTracker : IConsumeObserver
    {
        private readonly TaskCompletionSource<bool> consumed = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task PreConsume<T>(ConsumeContext<T> context) where T : class => Task.CompletedTask;

        public Task PostConsume<T>(ConsumeContext<T> context) where T : class
        {
            if (typeof(T) == typeof(IJobMessage) || context.Message is IJobMessage)
            {
                consumed.TrySetResult(true);
            }
            return Task.CompletedTask;
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
            => Task.CompletedTask;

        public async Task<bool> WaitForConsume(TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            try
            {
                return await consumed.Task.WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
