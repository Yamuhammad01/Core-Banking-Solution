namespace CoreBanking.Infrastructure.Configuration
{
    public class EmailConfiguration
    {
        public string From { get; set; } = string.Empty;

        // SMTP (Brevo free-forever plan; also works for Mailjet/Gmail)
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }
}
