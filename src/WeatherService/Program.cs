using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace WeatherService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateLogger();

            try
            {
                Log.Information($"WeatherService is starting (Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")})");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "WeatherService failed to start");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void CreateLogger()
        {
            //Read configuration from appSettings
            var config = new ConfigurationBuilder()
                .AddJsonFile(
                    path: $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json",
                    optional: false,
                    reloadOnChange: true)
                .Build();

            //Initialize Logger (Serilog)
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                .CreateLogger();
        }
    }
}