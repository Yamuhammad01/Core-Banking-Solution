namespace CoreBanking.Infrastructure.Configuration
{
    public class EmailConfiguration
    {
        public string From { get; set; } = string.Empty;
        public string SendGridApiKey { get; set; } = string.Empty;
    }
}
