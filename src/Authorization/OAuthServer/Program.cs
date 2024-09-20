using Duende.IdentityServer.Models;
using OAuthServer;

var builder = WebApplication.CreateBuilder(args);
{
	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen();

	Settings settings = new();
	builder.Configuration
		.GetSection(nameof(Settings))
		.Bind(settings);

	var clients = settings.Clients?.Select(c => new Client()
	{
		ClientId = c.ClientId,
		AllowedGrantTypes = GrantTypes.ClientCredentials,
		ClientSecrets =
			{
				new Secret(c.ClientSecret.Sha256())
			},
		AllowedScopes = c.AllowedScopes,
		AlwaysSendClientClaims = true
	});

	var apiScopes = settings.Scopes.Select(s => new ApiScope(s.Name, s.DisplayName));

	var apiResources = settings.ApiResources.Select(r => new ApiResource(r.Name, r.DisplayName)
	{
		Scopes = r.Scopes,
		Enabled = r.IsEnabled,
	});

	builder.Services.AddIdentityServer(options =>
	{
		// IdentityServer options configuration, if needed
	})
	.AddInMemoryApiScopes(apiScopes)
	.AddInMemoryClients(clients)
	.AddInMemoryApiResources(apiResources);
}

var app = builder.Build();
{
	app.UseSwagger();
	app.UseSwaggerUI();

	if (app.Environment.IsProduction())
	{
		app.UseHttpsRedirection();
	}

	app.UseIdentityServer();

	app
	.MapGet("/", () => "IdentityServer is running...")
	.WithOpenApi();
}

app.Run();