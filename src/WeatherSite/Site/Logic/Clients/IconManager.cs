using System.Net.Http;
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

public class IconManager(
    IHttpExecutor httpExecutor,
    IHttpRequestFactory  requestFactory,
    IOptions<ApiEndpoints> options)
{
    private readonly ApiEndpoints apiEndpoints = options.Value;

    public async Task<Result<IconDto>> GetIconAsync(string icon, CancellationToken ct)
    {   
        var url = $"{apiEndpoints.IconServiceApiUrl}Get";
        
        using var request = requestFactory.Create(
            url,
            new { icon },
            HttpMethod.Post.Method);
        
        using var res = await httpExecutor.SendAsync(
            HttpClientConsts.DefaultName, 
            PipelineNames.Default,
            request, 
            ct);

        return await HttpResult.ReadJsonAsResultAsync<IconDto>(res, ct);
    }
}