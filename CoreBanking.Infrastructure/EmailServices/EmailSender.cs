using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.DTOs.AccountDto;
using CoreBanking.Infrastructure.Configuration;
using CoreBanking.Infrastructure.Services;
using FluentEmail.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using CoreBanking.Infrastructure.EmailServices;
using SendGrid.Helpers.Mail;
using SendGrid;
using CoreBanking.Application.Common;

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
            var client = new SendGridClient(_emailConfig.SendGridApiKey);
            var from = new EmailAddress(_emailConfig.From, "CoreBanking");
            var subject = message.Subject;

            var toEmails = message.To.Select(x => new EmailAddress(x.Address, x.Name)).ToList();

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(
                from,
                toEmails,
                subject,
                plainTextContent: "Your email client does not support HTML.",
                htmlContent: message.Content
            );

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid failed: {response.StatusCode} - {error}");
            }
        }
        
    }

}
