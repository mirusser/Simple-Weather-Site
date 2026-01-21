using Common.ExtensionMethods;
using Common.Infrastructure.Managers.Contracts;
using Common.Presentation.Settings;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.Presentation.Http;

public class BearerTokenHandler(
    HttpClient httpClient,
    ICacheManager cache,
    ILogger<BearerTokenHandler> logger,
    IOptions<ApiConsumerAuthSettings> options) : DelegatingHandler
{
    private readonly HttpClient httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ApiConsumerAuthSettings settings = options.Value;
    private readonly string applicationName = AssemblyExtensions.GetProjectName();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var getTokenResult = await TryGetTokenFromCacheAsync(cancellationToken);

        if (getTokenResult.IsSuccess
            && getTokenResult.CachedToken?.IsError == false
            && getTokenResult.CachedToken?.AccessToken is not null)
        {
            request.SetBearerToken(getTokenResult.CachedToken.AccessToken);
        }
        else
        {
            logger.LogError(
                "{Error}: {ErrorDescription}",
                getTokenResult.CachedToken?.Error,
                getTokenResult.CachedToken?.ErrorDescription);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<(bool IsSuccess, CachedToken? CachedToken)> TryGetTokenFromCacheAsync(
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{applicationName}-access-token";

        (bool isSuccess, CachedToken? cachedToken) = await cache
            .TryGetValueAsync<CachedToken?>(cacheKey, cancellationToken);

        if (isSuccess && cachedToken is not null)
        {
            return (isSuccess, cachedToken);
        }

        (isSuccess, var tokenResponse) = await TryGetTokenAsync(cancellationToken);

        cachedToken = new CachedToken(
            tokenResponse?.AccessToken,
            tokenResponse?.IsError,
            tokenResponse?.ExpiresIn,
            tokenResponse?.RefreshToken,
            tokenResponse?.Error,
            tokenResponse?.ErrorDescription);

        if (isSuccess)
        {
            await cache.SetAsync(cacheKey, cachedToken, cancellationToken);
        }

        return (isSuccess, cachedToken);
    }

    private async Task<(bool IsSuccess, TokenResponse? TokenResponse)> TryGetTokenAsync(
        CancellationToken cancellationToken = default)
    {
        TokenResponse? tokenResponse = null;

        DiscoveryDocumentRequest discoveryRequest = new() { Address = settings.AuthorityUrl };
        discoveryRequest.Policy.RequireHttps = false; // TODO: change for prod

        var discoveryDocument = await httpClient
            .GetDiscoveryDocumentAsync(discoveryRequest, cancellationToken: cancellationToken);

        if (!ValidateDiscoveryDocument())
        {
            return (false, tokenResponse);
        }

        tokenResponse = await httpClient
            .RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryDocument.TokenEndpoint,
                ClientId = settings.ClientId,
                ClientSecret = settings.ClientSecret,
                Scope = settings.Scope,
            }, cancellationToken: cancellationToken);

        if (tokenResponse.IsError)
        {
            logger.LogError("{Error}", tokenResponse.Error);
            return (false, tokenResponse);
        }

        return (true, tokenResponse);

        bool ValidateDiscoveryDocument()
        {
            var result = true;

            if (discoveryDocument.IsError)
            {
                result = false;
                logger.LogError("{Error}", discoveryDocument.Error);
            }

            if (discoveryDocument.Issuer != settings.AuthorityUrl)
            {
                result = false;
                logger.LogError("Issuer: {Issuer} different than authority url: {Authority}", discoveryDocument.Issuer,
                    settings.AuthorityUrl);
            }

            if (string.IsNullOrEmpty(discoveryDocument.TokenEndpoint))
            {
                result = false;
                logger.LogError($"{nameof(discoveryDocument.TokenEndpoint)} is null or empty");
            }

            return result;
        }
    }
}

public sealed record CachedToken(
    string? AccessToken,
    bool? IsError,
    int? ExpiresIn,
    string? RefreshToken,
    string? Error,
    string? ErrorDescription
);