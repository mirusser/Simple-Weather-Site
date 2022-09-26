using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Common.Presentation;

public static class WebApplicationStartup
{
    public static void Run(WebApplication? app)
    {
        CreateLogger();

        var executingAssemblyName = Assembly
            .GetEntryAssembly()
            .GetName()
            .Name;

        try
        {
            Log.Information($"{executingAssemblyName} is starting (Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")})");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, $"{executingAssemblyName} failed to start");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void CreateLogger()
    {
        var environmentName = Environment
            .GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        //Read configuration from appSettings
        var config = new ConfigurationBuilder()
            .AddJsonFile(
                path: $"appsettings.{environmentName}.json",
                optional: false,
                reloadOnChange: true)
            .Build();

        //Initialize Logger (Serilog)
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.WithProperty(
                "Environment",
                environmentName)
            .CreateLogger();
    }
}