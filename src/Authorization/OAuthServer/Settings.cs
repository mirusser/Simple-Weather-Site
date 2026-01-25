namespace OAuthServer;

public class Settings
{
	public List<Client>? Clients { get; set; }
	public List<ApiResource>? ApiResources { get; set; }
	public List<ApiScope>? Scopes { get; set; }

	public class Client
	{
		public string? ClientId { get; set; }
		public string? ClientSecret { get; set; }
		public List<string>? AllowedScopes { get; set; }
	}

	public class ApiResource
	{
		public string? Name { get; set; }
		public string? DisplayName { get; set; }
		public List<string>? Scopes { get; set; }
		public bool IsEnabled { get; set; }
	}

	public class ApiScope
	{
		public string? Name { get; set; }
		public string? DisplayName { get; set; }
	}
}