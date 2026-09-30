using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Infrastructure.Configuration;
using System.Text;
using System.Text.Json;

namespace CoreBanking.Application.Services
{
    /// <summary>
    /// Sends transactional email through Brevo's HTTPS API (port 443).
    /// SMTP cannot be used on Render's free tier: outbound connections to
    /// SMTP ports 25/465/587 are blocked there (Render changelog, Sep 2025),
    /// so this service uses Brevo's HTTPS API instead. SMTP is still supported for local development.
    /// </summary>
    public class EmailSender : IEmailSenderr
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly HttpClient _httpClient;

        public EmailSender(EmailConfiguration emailConfig, HttpClient httpClient)
        {
            _emailConfig = emailConfig;
            _httpClient = httpClient;
        }

        public async Task SendEmailAsync(Message message)
        {
            if (string.IsNullOrWhiteSpace(_emailConfig.ApiKey))
                throw new InvalidOperationException(
                    "EmailConfiguration:ApiKey is not configured. Set EmailConfiguration__ApiKey " +
                    "(Brevo -> Settings -> SMTP & API -> API keys, value starts with \"xkeysib-\"). " +
                    "SMTP keys (\"xsmtpsib-\") do NOT work with this API.");

            if (string.IsNullOrWhiteSpace(_emailConfig.From))
                throw new InvalidOperationException(
                    "EmailConfiguration:From is not configured. Set EmailConfiguration__From to a verified Brevo sender.");

            var payload = new
            {
                sender = new { email = _emailConfig.From },
                to = message.To.Select(a => new { email = a.Address, name = a.Name }).ToArray(),
                subject = message.Subject,
                htmlContent = message.Content
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _emailConfig.ApiUrl);
            request.Headers.TryAddWithoutValidation("api-key", _emailConfig.ApiKey);
            request.Headers.TryAddWithoutValidation("accept", "application/json");
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Brevo API returned {(int)response.StatusCode} ({response.ReasonPhrase}) for {_emailConfig.ApiUrl}: {body}");
        }
    }
}
