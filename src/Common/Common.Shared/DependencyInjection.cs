using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // in .NET 9 and later you could use: JsonSerializerOptions.Default
        //https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/configure-options?pivots=dotnet-8-0
        services.AddSingleton<JsonSerializerOptions>(_ => new(JsonSerializerDefaults.Web));

        return services;
    }
}