using CoreBanking.Application;
using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Domain.Entities;
using CoreBanking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

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

        //  Fetch the user associated with a specific account
        public async Task<Customer> GetUserByAccountIdAsync(Guid accountId)
        {
            var account = await _dbContext.BankAccounts
                .Include(a => a.Customers)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            return account?.Customers;
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

        //return customer information from 2 tables
        public async Task<Customer?> GetCustomerInfoAsync(string userId)
        {
            return await _dbContext.Customers
                .Include(c => c.BankAccount)
                .FirstOrDefaultAsync(c => c.Id == userId);
        }
        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _dbContext.Customers
                .Include(c => c.BankAccount)
              .FirstOrDefaultAsync(c => c.Email == email);
        }
        //retrive all list of customers
        public async Task<List<Customer>> GetAllCustomerDetailsAsync()
        {
            return await _dbContext.Customers
                .Include(u => u.BankAccount)
                .AsNoTracking()
                .ToListAsync();
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
          await _dbContext.BankAccounts
             .Include(c => c.Customers)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        public async Task<BankAccount?> GetByAccountNumberAndUserIdAsync(string UserId, string accountNumber) =>
                      await _dbContext.BankAccounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.CustomerId == UserId);
        public async Task UpdateCustomerInfoAsync(Customer customer)
        {
            _dbContext.Customers.Update(customer);
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteCustomer(Customer customer)
        {
            _dbContext.Customers.Remove(customer);
            await _dbContext.SaveChangesAsync();
        }

    }


    public record CreatePin
    {
        public int Pin {  get; set; }
    }
}

