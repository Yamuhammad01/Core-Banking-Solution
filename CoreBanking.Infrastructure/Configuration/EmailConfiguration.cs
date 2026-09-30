namespace CoreBanking.Infrastructure.Configuration
{
    public class EmailConfiguration
    {
        public string From { get; set; } = string.Empty;

        // Brevo transactional API (HTTPS/443). Used instead of SMTP because Render's
        // free tier blocks outbound SMTP ports 25/465/587 
        
        public string ApiKey { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = "https://api.brevo.com/v3/smtp/email";

        // SMTP settings (legacy; kept for local tooling — EmailSender no longer uses them)
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }
}
