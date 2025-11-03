using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Responses;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs.AccountDto;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IAccountRepository _accountRepository;
        private readonly IAdminRepository _adminRepository;
        public AdminService(UserManager<Customer> userManager, 
            IAccountRepository accountRepository,
            IAdminRepository adminRepository)
        {
            _userManager = userManager;
            _accountRepository = accountRepository;
            _adminRepository = adminRepository;
        }

        public async Task<Result> FreezeAccountAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result.Failure("User not found");

            if (user.IsFrozen)
                return Result.Failure("Account is already frozen");

            user.IsFrozen = true;
            user.FrozenDate = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return Result.Success("Account has been frozen successfully");
        }

        public async Task<Result> UnfreezeAccountAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result.Failure("User not found");

            if (!user.IsFrozen)
                return Result.Failure("Account is already active");

            user.IsFrozen = false;
            user.FrozenDate = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return Result.Failure("Account has been reactivated successfully");
        }

        //deacitvate an account 
        public async Task<Result> DeactivateAccountAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result.Failure("User not found");

            if (!user.IsActive)
                return Result.Failure("Customer account has been deactivated already");

            user.IsActive = false;
   
            await _userManager.UpdateAsync(user);

            return Result.Success("Account has been deactivated successfully");
        }

        // Reactivate an account 
        public async Task<Result> ReactivateAccountAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result.Failure("User not found");

            if (user.IsActive)
                return Result.Failure("Account already active");

            user.IsActive = true;

            await _userManager.UpdateAsync(user);

            return Result.Success("Congrats, Your account has been reactivated successfully");
        }

        // get all customers 
        public async Task<List<ProfileDto>> GetAllCustomersAsync()
        {
            return await _adminRepository.GetAllCustomersAsync();
        }

    }

}

