using CoreBanking.Infrastructure.Configuration;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CoreBanking.Infrastructure.Messaging.Consumers
{
    public static class MassTransitConfig
    {
        public static void AddMassTransitServices(this IServiceCollection services, RabbitMqSettings settings)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<UserCreatedConsumer>();

                if (settings.IsBrokerConfigured)
                {
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        if (!string.IsNullOrWhiteSpace(settings.Url))
                        {
                            // Full connection string, e.g. amqps://user:pass@host/vhost (CloudAMQP)
                            cfg.Host(new Uri(settings.Url));
                        }
                        else
                        {
                            // amqp://host:5672 (or amqps://host:5671 when ssl is enabled)
                            cfg.Host(new Uri(settings.BuildEndpointUri()), h =>
                            {
                                h.Username(settings.User);
                                h.Password(settings.Password);
                            });
                        }

                        cfg.ReceiveEndpoint("user-created-dlq", e => { });

                        cfg.ReceiveEndpoint("user-created-queue", e =>
                        {
                            e.ConfigureConsumer<UserCreatedConsumer>(context);

                            // Retry policy: 30 attempts, 10s apart
                            e.UseMessageRetry(r => r.Interval(30, TimeSpan.FromSeconds(10)));
                        });
                    });
                }
                else
                {
                    // No broker configured (e.g. Render without RabbitMQ env vars):
                    // keep the bus and IPublishEndpoint resolvable with the in-memory transport
                    // so no connection is ever attempted against localhost.
                    x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
                }
            });
        }
    }
}
