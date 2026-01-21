namespace EmailService.Domain.Common.Errors;

public static partial class ErrorCodes
{
    // Mail-specific
    public const string MailInvalidRecipient = "mail_invalid_recipient";
    public const string MailAuthenticationFailed = "mail_auth_failed";
    public const string MailServerUnavailable = "mail_server_unavailable";
    public const string MailProtocolError = "mail_protocol_error";
}