using CoreBanking.Infrastructure.Persistence;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit.EntityFrameworkCoreIntegration;
using System.Data;

namespace CoreBanking.Infrastructure.Messaging.Consumers
{
    public static class MassTransitConfig
    {
        public static void AddMassTransitServices(this IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<UserCreatedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    cfg.ReceiveEndpoint("user-created-dlq", e => { });

                    cfg.ReceiveEndpoint("user-created-queue", e =>
                    {
                        // configure consumer
                        e.ConfigureConsumer<UserCreatedConsumer>(context);

                        // ensure messages are persisted 
                        //e.Durable = true;

                        // Retry policy: retry 30 times, 15s interval
                        e.UseMessageRetry(r => r.Interval(30, TimeSpan.FromSeconds(10)));

                        cfg.UseDelayedMessageScheduler();

                    
                        // 
                        /*   e.UseEntityFrameworkOutbox<CoreBankingDbContext>(o =>
                            {
                                o.QueryDelay = TimeSpan.FromSeconds(1);
                                o.OutboxEntityType = typeof(OutboxState);
                                o.UseSqlServer();
                            });


                            //e.UseInMemoryOutbox();
                            e.DiscardFaultedMessages();



                            // Configure dead-letter queue automatically
                            //e.BindDeadLetterQueue("user-created-queue");

                        });

                        cfg.UseEntityFrameworkOutbox<CoreBankingDbContext>(o =>
                        {
                            o.QueryDelay = TimeSpan.FromSeconds(1);
                            o.OutboxEntityType = typeof(OutboxState);   // use your custom entity
                            o.UseSqlServer();
                        });




                        // standard retry policy 
                        /* cfg.UseMessageRetry(r =>
                        {
                            // Layer 1: quick retries
                            r.Intervals(
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(3),
                                TimeSpan.FromSeconds(9)
                            );

                            // Layer 2: delayed retries
                            r.Intervals(
                                TimeSpan.FromMinutes(1),
                                TimeSpan.FromMinutes(3),
                                TimeSpan.FromMinutes(5),
                                TimeSpan.FromMinutes(8),
                                TimeSpan.FromMinutes(12),
                                TimeSpan.FromMinutes(15)
                            );
                        }); */


                    });
                });

                //services.AddMassTransitHostedService();
            });
        }
    }
}
