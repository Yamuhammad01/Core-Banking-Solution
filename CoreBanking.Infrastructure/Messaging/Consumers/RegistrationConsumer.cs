using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Npgsql;
using Dapper;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBanking.Infrastructure.Messaging.Consumer
{
    public class RegistrationConsumer : BackgroundService
    {
        private readonly IConfiguration _config;

        public RegistrationConsumer(IConfiguration config)
        {
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
               //DispatchConsumersAsync = true  //  REQUIRED for async consumers
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "registration.queue",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<CustomerCreatedMessage>(json);

                try
                {
                    var connString = _config.GetConnectionString("DefaultConnection");

                    using var db = new NpgsqlConnection(connString);

                    // Upsert: insert if not exists, do nothing if email exists
                    var sql = @"
                        INSERT INTO customers(first_name, last_name, email, password_hash, phone_number)
                         VALUES (@FirstName, @LastName, @Email, @PasswordHash, @PhoneNumber)
                          ON CONFLICT (email) DO NOTHING;
                   ";

                    var result = await db.ExecuteAsync(sql, new
                    {
                        message.FirstName,
                        message.LastName,
                        message.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(message.Password),
                        message.PhoneNumber
                    });

                    if (result > 0)
                    {
                        Console.WriteLine($"[x] Customer {message.Email} inserted successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"[!] Customer {message.Email} already exists. Skipping insert.");
                    }

                    // Always ACK to prevent RabbitMQ retry
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to process {message.Email}: {ex.Message}");
                    // NACK with requeue = true to retry later
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync(
                queue: "registration.queue",
                autoAck: false,
                consumer: consumer
            );

            // Keep service alive
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }

    public record CustomerCreatedMessage(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string ConfirmPassword,
        string PhoneNumber
    );
}
