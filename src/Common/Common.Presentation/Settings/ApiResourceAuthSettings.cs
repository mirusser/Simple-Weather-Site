namespace Common.Presentation.Settings;

public class ApiResourceAuthSettings
{
	public string? AuthorityUrl { get; set; }
	public string? Audience { get; set; }
	public List<RequiredClaim>? RequiredClaims { get; set; }

	public class RequiredClaim
	{
		public string? ClaimType { get; set; }
		public string[]? AllowedValues { get; set; }
	}
}