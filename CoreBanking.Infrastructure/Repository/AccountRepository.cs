using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Domain.Entities;
using CoreBanking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.Infrastructure.Repository
{
    public class AccountRepository(CoreBankingDbContext context) : IAccountRepository
    {
        public async Task<BankAccount> CreateAsync(BankAccount account)
        {
            context.BankAccounts.Add(account);
            await context.SaveChangesAsync();
            return account;
        }

        //  Fetch the user associated with a specific account
        public async Task<Customer> GetUserByAccountIdAsync(Guid accountId)
        {
            var account = await context.BankAccounts
                .Include(a => a.Customers)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            return account?.Customers;
        }

        public async Task<IEnumerable<BankAccount>> GetByCustomerIdAsync(string customerId)
        {
            return await context.BankAccounts
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<BankAccount?> GetByUserIdAsync(string userId)
        {
            return await context.BankAccounts
                .Include(a => a.Customers) // include AspNetUsers
                .FirstOrDefaultAsync(a => a.CustomerId == userId);
        }

        public async Task<Customer?> GetUserIdAsync(string userId)
        {
            return await context.Customers
                .FirstOrDefaultAsync(a => a.Id == userId);
        }

        //return customer information from 2 tables
        public async Task<Customer?> GetCustomerInfoAsync(string userId)
        {
            return await context.Customers
                .Include(c => c.BankAccount)
                .FirstOrDefaultAsync(c => c.Id == userId);
        }
        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await context.Customers
                .Include(c => c.BankAccount)
              .FirstOrDefaultAsync(c => c.Email == email);
        }
        //retrive all list of customers
        public async Task<List<Customer>> GetAllCustomerDetailsAsync()
        {
            return await context.Customers
                .Include(u => u.BankAccount)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<BankAccount?> GetByIdAsync(Guid id)
        {
            return await context.BankAccounts.FindAsync(id);
        }

        public async Task UpdateAsync(BankAccount account)
        {
            context.BankAccounts.Update(account);
           // await _dbContext.SaveChangesAsync();
        }

        public async Task<BankAccount?> GetByAccountNumberAsync(string accountNumber) =>
          await context.BankAccounts
             .Include(c => c.Customers)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        public async Task<BankAccount?> GetByAccountNumberAndUserIdAsync(string UserId, string accountNumber) =>
                      await context.BankAccounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.CustomerId == UserId);
        public async Task UpdateCustomerInfoAsync(Customer customer)
        {
            context.Customers.Update(customer);
            await context.SaveChangesAsync();
        }
        public async Task DeleteCustomer(Customer customer)
        {
            context.Customers.Remove(customer);
            await context.SaveChangesAsync();
        }

    }


    public record CreatePin
    {
        public int Pin {  get; set; }
    }
}

