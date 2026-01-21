using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Common.Presentation;

public static class WebApplicationStartup
{
    extension(WebApplication app)
    {
        public async Task RunWithLoggerAsync()
        {
            var appName = ExtensionMethods.AssemblyExtensions.GetProjectName();
            var lifetime = app.Lifetime;

            lifetime.ApplicationStarted.Register(() =>
            {
                Log.Information(
                    "{Name} has started and is ready to accept requests (Environment: {Environment})",
                    appName,
                    app.Environment.EnvironmentName);
            });

            lifetime.ApplicationStopping.Register(()
                => Log.Information("{Name} is stopping", appName));

            lifetime.ApplicationStopped.Register(()
                => Log.Information("{Name} has stopped", appName));

            try
            {
                Log.Information(
                    "{Name} is starting (Environment: {Environment})",
                    appName,
                    app.Environment.EnvironmentName);

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "{Name} fatal exception during execution", appName);
                throw;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}