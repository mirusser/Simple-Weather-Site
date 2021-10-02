namespace Models.Settings
{
    public class MailSettings
    {
        public string From { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string DefaultEmailReciever { get; set; } = null!;
    }
}