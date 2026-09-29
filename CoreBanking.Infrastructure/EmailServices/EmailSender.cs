using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using SecureSocketOptions = MailKit.Security.SecureSocketOptions;

namespace CoreBanking.Application.Services
{
    public class EmailSender : IEmailSenderr
    {
        private readonly EmailConfiguration _emailConfig;
        public EmailSender(EmailConfiguration emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmailAsync(Message message)
        {
            if (string.IsNullOrWhiteSpace(_emailConfig.SmtpHost))
                throw new InvalidOperationException(
                    "EmailConfiguration:SmtpHost is not configured. Set EmailConfiguration__SmtpHost (e.g. smtp-relay.brevo.com).");

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(_emailConfig.From));
            foreach (var recipient in message.To)
                mimeMessage.To.Add(MailboxAddress.Parse(recipient.Address));
            mimeMessage.Subject = message.Subject;
            mimeMessage.Body = new BodyBuilder { HtmlBody = message.Content }.ToMessageBody();

            using var smtpClient = new SmtpClient();
            var socketOptions = _emailConfig.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await smtpClient.ConnectAsync(_emailConfig.SmtpHost, _emailConfig.SmtpPort, socketOptions);

            if (!string.IsNullOrWhiteSpace(_emailConfig.SmtpUser))
                await smtpClient.AuthenticateAsync(_emailConfig.SmtpUser, _emailConfig.SmtpPassword);

            await smtpClient.SendAsync(mimeMessage);
            await smtpClient.DisconnectAsync(true);
        }
    }
}
