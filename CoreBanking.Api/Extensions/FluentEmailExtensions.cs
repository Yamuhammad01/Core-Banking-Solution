using CoreBanking.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBanking.Api.Extensions
{
    public static class FluentEmailExtensions
    {
        public static IServiceCollection AddFluentEmailConfiguration(
            this IServiceCollection services, IConfiguration configuration)

        {
            
            var emailConfig = configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfiguration>() ?? new EmailConfiguration();

            Console.WriteLine(string.IsNullOrWhiteSpace(emailConfig.SmtpHost)
                ? "[Email] SMTP NOT configured — set EmailConfiguration__SmtpHost / __SmtpUser / __SmtpPassword."
                : $"[Email] SMTP configured: {emailConfig.SmtpHost}:{emailConfig.SmtpPort}, From={emailConfig.From}, Ssl={emailConfig.EnableSsl}");

            services.AddSingleton(emailConfig);
            return services;
        }
    }
}
