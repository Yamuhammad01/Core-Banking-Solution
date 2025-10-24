using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Infrastructure.EmailServices;
using Microsoft.Extensions.Logging;
using NETCore.MailKit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Application.Common;
namespace CoreBanking.Infrastructure.Services
{
    public class TransactionEmailService : ITransactionEmailService
    {
        private readonly EmailTemplateService _emailTemplateService;
        private readonly IEmailSenderr _emailSender;
        private readonly ILogger<TransactionEmailService> _logger;

        public TransactionEmailService(
            EmailTemplateService emailTemplateService,
            IEmailSenderr emailSender,
            ILogger<TransactionEmailService> logger)
        {
            _emailTemplateService = emailTemplateService;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task SendTransactionEmailAsync(
            string email,
            string firstName,
            string lastName,
            string transactionType,
            decimal amount,
            string accountNumber,
            string reference,
            decimal balance,
            DateTime date,
            string? senderFullName)
        {
            try
            {
                var emailBody = await _emailTemplateService.GetTransactionTemplateAsync(
                    firstName,
                    lastName, 
                    transactionType,
                    senderFullName,
                    amount,
                    accountNumber,
                    reference,
                    balance,
                    date
                );

                var message = new Message(
                    new[] { email },
                    $"{transactionType} Alert - ₦{amount:N2}",
                    emailBody
                );

                await _emailSender.SendEmailAsync(message);
                _logger.LogInformation($"Transaction email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send transaction email: {ex.Message}");
            }
        }
    }
}
