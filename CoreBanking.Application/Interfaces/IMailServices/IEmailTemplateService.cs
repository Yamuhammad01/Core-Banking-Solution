using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Interfaces.IMailServices
{
    public interface IEmailTemplateService
    {
        Task<string> GetWelcomeTemplateAsync(
            string firstName,
            string lastName,
            string accountNumber,
            string currency);

        Task<string> GetTransactionTemplateAsync(
            string firstName,
            string lastName,
            string transactionType,
            string? senderFullName,
            decimal amount,
            string accountNumber,
            string reference,
            decimal balance,
            DateTime date);
    }
}
