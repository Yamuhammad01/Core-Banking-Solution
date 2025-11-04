using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.DTOs.AccountDto;
using CoreBanking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Infrastructure.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly CoreBankingDbContext _dbContext;
        public AdminRepository(CoreBankingDbContext dbContext) 
        { 
            _dbContext = dbContext;
        }

        public async Task<List<ProfileDto>> GetAllCustomersAsync()
        {
            var customers = await _dbContext.Users
                .Include(u => u.BankAccount)
                 .Where(u => u.BankAccount != null) // exclude admin (admin doesnt have a bank account)
                .Select(u => new ProfileDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    AccountNumber = u.BankAccount.AccountNumber,
                    AccountBalance = u.BankAccount.Balance,
                    IsActive = u.IsActive
                })
                .ToListAsync();
          

            return customers;
        }
        //get frozen accounts 
        public async Task<List<FrozenAccountDto>> GetAllFrozenAccountAsync()
        {
            var customers = await _dbContext.Users
                .Include(u => u.BankAccount)
                 .Where(u => u.BankAccount != null) // exclude admin (admin doesnt have a bank account)
                 .Where(u => u.IsFrozen == true)
                .Select(u => new FrozenAccountDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    AccountNumber = u.BankAccount.AccountNumber,
                    AccountBalance = u.BankAccount.Balance,
                    IsFrozen = u.IsFrozen
                })
                .ToListAsync();


            return customers;
        }
        // get total numbers of customers
        public async Task<int> GetCustomerCountAsync()
        {
            return await _dbContext.Customers
                .CountAsync();
        }
        //get total number of active customers
        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _dbContext.Customers.CountAsync(u => u.IsActive == true);
        }

        public async Task<int> GetInactiveUsersCountAsync()
        {
            return await _dbContext.Customers.CountAsync(u => u.IsActive == false);
        }
    }
}
