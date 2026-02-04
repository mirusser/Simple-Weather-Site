using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Infrastructure.Consts;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Presentation.Http;
using Microsoft.Extensions.Options;
using WeatherSite.Logic.Clients.Models.Records;
using WeatherSite.Logic.Settings;

namespace WeatherSite.Logic.Clients;

public class IconClient(
    IHttpExecutor httpExecutor,
    IHttpRequestFactory  requestFactory,
    IOptions<ApiEndpoints> options,
    JsonSerializerOptions jsonSerializerOptions)
{
    private readonly ApiEndpoints apiEndpoints = options.Value;

    public async Task<Result<IconDto>> GetIconAsync(string icon, CancellationToken ct)
    {   
        var url = $"{apiEndpoints.IconServiceApiUrl}Get";

        var jsonBody = JsonSerializer.Serialize(
            new { icon },
            jsonSerializerOptions);

        using var request = requestFactory.Create(
            url,
            HttpMethod.Post.Method,
            jsonBody,
            null);
        
        using var res = await httpExecutor.SendAsync(
            HttpClientConsts.DefaultName, 
            PipelineNames.Default,
            request, 
            ct);

        return await HttpResult.ReadJsonAsResultAsync<IconDto>(res, ct);
    }
}