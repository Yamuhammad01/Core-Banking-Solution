using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Application;
using CoreBanking.Application.Interfaces;
using CoreBanking.Domain.Entities;
using CoreBanking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.Infrastructure.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly CoreBankingDbContext _dbContext;

        public AccountRepository(CoreBankingDbContext context)
        {
            _dbContext = context;
        }

        public async Task<BankAccount> CreateAsync(BankAccount account)
        {
            _dbContext.BankAccounts.Add(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }

        public async Task<IEnumerable<BankAccount>> GetByCustomerIdAsync(string customerId)
        {
            return await _dbContext.BankAccounts
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<BankAccount?> GetByUserIdAsync(string userId)
        {
            return await _dbContext.BankAccounts
                .Include(a => a.Customers) // include AspNetUsers
                .FirstOrDefaultAsync(a => a.CustomerId == userId);
        }

        public async Task<Customer?> GetUserIdAsync(string userId)
        {
            return await _dbContext.Customers
                .FirstOrDefaultAsync(a => a.Id == userId);
        }

        public async Task<BankAccount?> GetByIdAsync(Guid id)
        {
            return await _dbContext.BankAccounts.FindAsync(id);
        }

        public async Task UpdateAsync(BankAccount account)
        {
            _dbContext.BankAccounts.Update(account);
           // await _dbContext.SaveChangesAsync();
        }

        public async Task<BankAccount?> GetByAccountNumberAsync(string accountNumber) =>
          await _dbContext.BankAccounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        public async Task<BankAccount?> GetByAccountNumberAndUserIdAsync(string UserId, string accountNumber) =>
                      await _dbContext.BankAccounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.CustomerId == UserId);

    }

}

