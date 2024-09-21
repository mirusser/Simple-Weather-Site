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
		var getTokenResult = await TryGetTokenAsync(cancellationToken);

		if (getTokenResult.IsSuccess
			&& getTokenResult.TokenResponse?.IsError == false
			&& getTokenResult.TokenResponse?.AccessToken is not null)
		{
			request.SetBearerToken(getTokenResult.TokenResponse.AccessToken);
		}
		else
		{
			logger.LogError(
				"{Error}: {ErrorType} {ErrorDescription}",
				getTokenResult.TokenResponse?.Error,
				getTokenResult.TokenResponse?.ErrorType,
				getTokenResult.TokenResponse?.ErrorDescription);
		}

		return await base.SendAsync(request, cancellationToken);
	}

	//TODO: caching
	private async Task<(bool IsSuccess, TokenResponse? TokenResponse)> TryGetTokenAsync(CancellationToken cancellationToken = default)
	{
		TokenResponse? tokenResponse = null;
		var discoveryDocument = await httpClient
			.GetDiscoveryDocumentAsync(settings.AuthorityUrl, cancellationToken: cancellationToken);

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
				Scope = settings.Scope
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
				logger.LogError("Issuer: {Issuer} different than authority url: {Authority}", discoveryDocument.Issuer, settings.AuthorityUrl);
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