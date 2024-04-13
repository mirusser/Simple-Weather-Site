using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Common.Presentation;

public static class WebApplicationStartup
{
	public static async Task RunWithLoggerAsync(this WebApplication app)
	{
		InitializeLogger(app);

		var executingAssemblyName = GetExecutingAssemblyName();

		try
		{
			Log.Information(
				"{Name} is starting (Environment: {Environment})",
				executingAssemblyName,
				app.Environment.EnvironmentName);

			await app.RunAsync();
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, "{Name} failed to start", executingAssemblyName);
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}

	private static void InitializeLogger(WebApplication app)
	{
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(app.Configuration)
			.Enrich.WithProperty("Environment", app.Environment.EnvironmentName)
			.CreateLogger();
	}

	private static string GetExecutingAssemblyName()
		=> Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown Application";
}