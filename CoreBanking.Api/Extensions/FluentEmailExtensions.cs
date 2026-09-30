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

            Console.WriteLine(string.IsNullOrWhiteSpace(emailConfig.ApiKey)
                ? "[Email] Brevo API NOT configured — set EmailConfiguration__ApiKey (xkeysib-...)."
                : $"[Email] Brevo API configured: {emailConfig.ApiUrl}, From={emailConfig.From}");

            services.AddSingleton(emailConfig);
            return services;
        }
    }
}
