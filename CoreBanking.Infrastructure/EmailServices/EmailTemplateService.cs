using System;
using System.IO;
using System.Threading.Tasks;

namespace CoreBanking.Infrastructure.EmailServices
{
    public class EmailTemplateService
    {
        private readonly string _contentRootPath;

        public EmailTemplateService(string contentRootPath)
        {
            _contentRootPath = contentRootPath;
        }

        public async Task<string> GetWelcomeTemplateAsync(string firstName, string lastName, string accountNumber, string currency)
        {
            var path = Path.Combine(
                 Directory.GetParent(_contentRootPath)!.FullName,
                "CoreBanking.Infrastructure",
                "EmailTemplates",
                "WelcomeTemplate.html"
            );

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
    }
}
