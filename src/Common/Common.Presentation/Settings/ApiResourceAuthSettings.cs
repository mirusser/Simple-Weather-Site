namespace Common.Presentation.Settings;

public class ApiResourceAuthSettings
{
	public string? AuthorityUrl { get; set; }
	public string? Audience { get; set; }
	public List<RequiredClaim>? RequiredClaims { get; init; }

	public class RequiredClaim
	{
		public required string ClaimType { get; set; }
		public required string[] AllowedValues { get; set; }
	}
}