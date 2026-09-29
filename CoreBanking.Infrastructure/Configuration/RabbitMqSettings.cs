using RabbitMQ.Client;
using System;

namespace CoreBanking.Infrastructure.Configuration
{
    /// <summary>
    /// RabbitMQ broker settings, bound from the "RabbitMq" configuration section.
    /// Local dev: appsettings.json. Render: environment variables, e.g.
    ///   RabbitMq__Url=amqps://user:pass@host/vhost
    ///   RabbitMq__Host / RabbitMq__Port / RabbitMq__User / RabbitMq__Password / RabbitMq__VirtualHost
    ///   RabbitMq__Enabled=false disables broker integration (the API still starts).
    /// </summary>
    public class RabbitMqSettings
    {
        public bool Enabled { get; set; } = true;

        /// <summary>Full AMQP URI (amqp:// or amqps://). Takes precedence over Host/Port/User/Password.</summary>
        public string Url { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 5672;
        public bool UseSsl { get; set; }
        public string User { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";

        public string Exchange { get; set; } = "corebank.exchange";
        public string Queue { get; set; } = "registration.queue";
        public string RoutingKey { get; set; } = "registration.create";
        public ushort PrefetchCount { get; set; } = 10;
        public bool DurableQueue { get; set; } = true;

        /// <summary>
        /// True only when a broker address was actually supplied, so nothing silently falls back
        /// to localhost inside an isolated container (e.g. Render).
        /// </summary>
        public bool IsBrokerConfigured =>
            Enabled && (!string.IsNullOrWhiteSpace(Url) || !string.IsNullOrWhiteSpace(Host));

        public bool IsSslEnabled =>
            UseSsl
            || Port == 5671
            || (!string.IsNullOrWhiteSpace(Url) && Url.StartsWith("amqps", StringComparison.OrdinalIgnoreCase));

        /// <summary>Host/port/vhost URI without credentials (MassTransit sets credentials separately).</summary>
        public string BuildEndpointUri()
        {
            var scheme = IsSslEnabled ? "amqps" : "amqp";
            var path = string.IsNullOrWhiteSpace(VirtualHost) || VirtualHost == "/"
                ? string.Empty
                : "/" + Uri.EscapeDataString(VirtualHost);

            return $"{scheme}://{Host}:{Port}{path}";
        }

        /// <summary>Shared factory used by the registration consumer and the publish endpoint.</summary>
        public ConnectionFactory CreateConnectionFactory()
        {
            if (!string.IsNullOrWhiteSpace(Url))
            {
                // The URL carries scheme, host, port, credentials and vhost; "amqps" also enables TLS.
                return new ConnectionFactory
                {
                    Uri = new Uri(Url),
                    AutomaticRecoveryEnabled = true
                };
            }

            return new ConnectionFactory
            {
                HostName = Host,
                Port = Port,
                UserName = User,
                Password = Password,
                VirtualHost = string.IsNullOrWhiteSpace(VirtualHost) ? "/" : VirtualHost,
                AutomaticRecoveryEnabled = true,
                Ssl = new SslOption
                {
                    Enabled = IsSslEnabled,
                    ServerName = Host
                }
            };
        }
    }
}
