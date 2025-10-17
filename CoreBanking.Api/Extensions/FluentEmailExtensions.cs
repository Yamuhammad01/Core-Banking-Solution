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
                .Get<EmailConfiguration>();

            services.AddSingleton(emailConfig);
            return services;
        }
    }
}
