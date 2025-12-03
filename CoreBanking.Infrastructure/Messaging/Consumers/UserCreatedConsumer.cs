using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IMailServices;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.DTOs.Events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Infrastructure.Messaging.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreated>
    {
        private readonly IEmailSenderr _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;

        public UserCreatedConsumer(IEmailSenderr emailSender, IEmailTemplateService emailTemplateService)
        {
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
        }

        public async Task Consume(ConsumeContext<UserCreated> context)
        {
            var message = context.Message;

            var emailBody = await _emailTemplateService.GetWelcomeTemplateAsync(
                message.FirstName, message.LastName, message.AccountNumber, message.Currency
            );

            var email = new Message(
                new string[] { message.Email },
                "Welcome to CoreBanking",
                emailBody
            );

            try
            {
                await _emailSender.SendEmailAsync(email);
            }
            catch (Exception ex)
            {
                // log failure for retry
                Console.WriteLine($"Failed to send to {message.Email} : {ex.Message}");

                // Throw exception to trigger MassTransit retry
                throw;
            }
        }
    }
}
