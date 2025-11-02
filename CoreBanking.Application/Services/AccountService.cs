using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs.AccountDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Services
{
    public class AccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }


        public async Task<BankAccount> OpenAccountAsync(string customerId, CreateAccountDto request)
        {
            var account = new BankAccount
            {
                AccountNumber = GenerateAccountNumber(),
                AccountType = request.AccountType,
                Currency = request.Currency,
                Balance = request.InitialDeposit,
                CustomerId = customerId
            };

            return await _accountRepository.CreateAsync(account);
        }

        public async Task<IEnumerable<BankAccount>> GetAccountsAsync(string customerId)
            => await _accountRepository.GetByCustomerIdAsync(customerId);

        public async Task<BankAccount?> GetByIdAsync(Guid id)
            => await _accountRepository.GetByIdAsync(id);

        public async Task UpdateStatusAsync(Guid id, string status)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null)
                throw new Exception("Account not found");

            account.Status = status;
            await _accountRepository.UpdateAsync(account);
        }
        public async Task<Customer?> GetCustomerInfoAsync(string userId)
        {
            return await _accountRepository.GetCustomerInfoAsync(userId);
        }
        // update customer information
        public async Task<Result> UpdateCustomerProfileAsync(UpdateProfileDto request)
        {
            var customer = await _accountRepository.GetCustomerByEmailAsync(request.Email);

            if (customer == null)
                return Result.Failure("Customer not found");

            // Update customer info
            customer.FirstName = request.FirstName ?? customer.FirstName;
            customer.LastName = request.LastName ?? customer.LastName;
            customer.Email = request.Email ?? customer.Email;
            customer.PhoneNumber = request.PhoneNumber ?? customer.PhoneNumber;

            await _accountRepository.UpdateCustomerInfoAsync(customer);
            return Result.Success("Customer updated successfully");
        }

        // get a customer by email 
        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _accountRepository.GetCustomerByEmailAsync(email);
        }

        public async Task<Result> DeleteCustomerProfileAsync(string email)
        {
            var customer = await _accountRepository.GetCustomerByEmailAsync(email);

            if (customer == null)
                return Result.Failure("Customer not found");

            await _accountRepository.DeleteCustomer(customer);
            return Result.Success("Customer deleted successfully");
        }

        private static string GenerateAccountNumber()
        {
            var random = new Random();
            return random.Next(1000000000, int.MaxValue).ToString().Substring(0, 10);
        }
    }

}

