using CoreBanking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Infrastructure.Services
{
    public class AccountNumberGenerator
    {
        private readonly CoreBankingDbContext _context;
        private readonly Random _random = new();
        public AccountNumberGenerator(CoreBankingDbContext context) 
        {
            _context = context;
        }

        private async Task<string> GenerateUniqueAccountNumberAsync()
        {
            const string bankCode = "811";
            string accountNumber;
            bool exists;

            do
            {
                // Generate 7 random digits
                var random = new Random();
                var randomDigits = random.Next(0, 9999999).ToString("D7"); // pad with zeros
                accountNumber = bankCode + randomDigits;

                // Ensure it's unique b4 creating new account
                exists = await _context.BankAccounts
                    .AnyAsync(b => b.AccountNumber == accountNumber);

            } while (exists);

            return accountNumber;
        }

    }
}
