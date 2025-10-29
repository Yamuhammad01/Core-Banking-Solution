using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Domain.Entities;
using Octokit;

namespace CoreBanking.Application.Interfaces.IRepository
{
    public interface IAccountRepository
    {
        Task<BankAccount> CreateAsync(BankAccount account);
        Task<BankAccount?> GetByAccountNumberAsync(string accountNumber);

        Task<BankAccount?> GetByAccountNumberAndUserIdAsync(string UserId, string accountNumber);

        Task<IEnumerable<BankAccount>> GetByCustomerIdAsync(string customerId);

        Task<BankAccount?> GetByUserIdAsync(string userId);

        Task<Customer?> GetUserIdAsync(string userId);

        Task<Customer> GetUserByAccountIdAsync(Guid accountId);

        Task<BankAccount?> GetByIdAsync(Guid id);
        Task UpdateAsync(BankAccount account);
        Task<Customer?> GetCustomerInfoAsync(string userId);
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task DeleteCustomer (Customer customer);
        Task UpdateCustomerInfoAsync(Customer customer);
    }
}
