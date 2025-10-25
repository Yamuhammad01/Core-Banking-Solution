using CoreBanking.Application.Interfaces.IMailServices;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreBanking.Infrastructure.EmailServices
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly string _contentRootPath;

        public EmailTemplateService(string contentRootPath)
        {
            _contentRootPath = contentRootPath;
        }

        public async Task<string> GetWelcomeTemplateAsync(string firstName, string lastName, string accountNumber, string currency)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "WelcomeTemplate.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Email template not found at {path}");

            var template = await File.ReadAllTextAsync(path);

            template = template.Replace("{{FirstName}}", firstName)
                               .Replace("{{LastName}}", lastName)
                               .Replace("{{AccountNumber}}", accountNumber)
                               .Replace("{{Currency}}", currency)
                               .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

            return template;
        }


        public async Task<string> GetTransactionTemplateAsync(
                 string firstName,
                 string lastName,
                 string transactionType,
                 string? senderFullName,
                 decimal amount,
               string accountNumber,
               string reference,
               decimal balance,
               DateTime date)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", "TransactionAlert.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Email template not found at {path}");

            var template = await File.ReadAllTextAsync(path);
            string transactionColor = transactionType.Equals("credit", StringComparison.OrdinalIgnoreCase)
                 ? "background-color:#28a745;"
                 : "background-color:#dc3545;";

            // If sender is null or empty, remove the entire <tr> that contains {{SenderSection}}
            if (string.IsNullOrEmpty(senderFullName))
            {
                // Remove the <tr> block containing t1he Sender row completely
                template = Regex.Replace(
                    template,
                    @"<tr[^>]*>\s*<td[^>]*>Sender:<\/td>\s*<td[^>]*>{{SenderSection}}<\/td>\s*<\/tr>",
                    string.Empty,
                    RegexOptions.IgnoreCase
                );
            }


            template = template.Replace("{{FirstName}}", firstName)
                                .Replace("{{LastName}}", lastName)
                               .Replace("{{TransactionType}}", transactionType)
                               .Replace("{{Amount}}", amount.ToString("N2"))
                               .Replace("{{AccountNumber}}", accountNumber)
                               .Replace("{{Reference}}", reference)
                               .Replace("{{Balance}}", balance.ToString("N2"))
                               .Replace("{{Date}}", date.ToString("dd MMM yyyy, hh:mm tt"))
                               .Replace("{{Year}}", DateTime.UtcNow.Year.ToString())
                               .Replace("{{TransactionColor}}", transactionColor)
                               .Replace("{{SenderSection}}", senderFullName ?? string.Empty);

            return template;
        }

    }
}
