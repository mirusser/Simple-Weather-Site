using Common.Presentation.Settings;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.Presentation.Http;

public class BearerTokenHandler(
	ILogger<BearerTokenHandler> logger,
	HttpClient httpClient,
	IOptions<ApiConsumerAuthSettings> options) : DelegatingHandler
{
	private readonly HttpClient httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
	private readonly ApiConsumerAuthSettings settings = options.Value;

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		var token = await GetTokenAsync(cancellationToken);

		if (!token.IsError && token.AccessToken is not null)
		{
			request.SetBearerToken(token.AccessToken);
		}
		else
		{
			logger.LogError("{Error}: {ErrorType} {ErrorDescription}", token.Error, token.ErrorType, token.ErrorDescription);
		}

		return await base.SendAsync(request, cancellationToken);
	}

	//TODO: caching
	private async Task<TokenResponse> GetTokenAsync(CancellationToken cancellationToken = default)
	{
		var discoveryDocument = await httpClient
			.GetDiscoveryDocumentAsync(settings.AuthorityUrl, cancellationToken: cancellationToken);

		if (discoveryDocument.IsError)
		{
			logger.LogError("{Error}", discoveryDocument.Error);
		}

		var tokenResponse = await httpClient
			.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
			{
				Address = discoveryDocument.TokenEndpoint,
				ClientId = settings.ClientId,
				ClientSecret = settings.ClientSecret,
				Scope = settings.Scope
			}, cancellationToken: cancellationToken);

		if (tokenResponse.IsError)
		{
			logger.LogError("{Error}", tokenResponse.Error);
		}

		return tokenResponse;
	}
}