using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Responses;
using CoreBanking.Domain.Entities;
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
        public AdminService(UserManager<Customer> userManager)
        {
            _userManager = userManager;
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
    }
}
