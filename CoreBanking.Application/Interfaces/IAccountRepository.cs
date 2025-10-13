using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Domain.Entities;

namespace CoreBanking.Application.Interfaces
{
    public interface IAccountRepository
    {
        Task<BankAccount> CreateAsync(BankAccount account);
        Task<BankAccount?> GetByAccountNumberAsync(string accountNumber);

        Task<BankAccount?> GetByAccountNumberAndUserIdAsync(string UserId, string accountNumber);

        Task<IEnumerable<BankAccount>> GetByCustomerIdAsync(string customerId);

        Task<BankAccount?> GetByUserIdAsync(string userId);

        Task<Customer?> GetUserIdAsync(string userId);

        Task<BankAccount?> GetByIdAsync(Guid id);
        Task UpdateAsync(BankAccount account);
    }
}
