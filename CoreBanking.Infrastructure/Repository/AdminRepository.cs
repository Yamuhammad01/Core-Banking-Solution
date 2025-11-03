using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.DTOs.AccountDto;
using CoreBanking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
    }
}
