namespace HangfireService.Settings;

public class MailSettings
{
	public string To { get; set; } = null!;
	public string? From { get; set; }
	public bool SendWhenUnhealthyIsEnabled { get; set; }
}
