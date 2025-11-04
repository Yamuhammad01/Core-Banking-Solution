using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Responses;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs.AccountDto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using OfficeOpenXml;
using System.ComponentModel;
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

        // get all frozen accounts 
        public async Task<List<FrozenAccountDto>> GetAllFrozenAccounts()
        {
            return await _adminRepository.GetAllFrozenAccountAsync();
        }

        public async Task <int> TotalCustomersAsync()
        {
            return await _adminRepository.GetCustomerCountAsync();

        }
        public async Task<int> TotalActiveCustomers()
        {
            return await _adminRepository.GetActiveUsersCountAsync();

        }
        //  get total list of active customers
        public async Task<int> TotalInActiveCustomers()
        {
            return await _adminRepository.GetInactiveUsersCountAsync();

        }

        // export customer list into excel sheet
       /* public async Task<byte[]> ExportStaffListToExcelAsync()
        {
            var customerList = await _adminRepository.GetAllCustomersAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Staff List");

            // Header Row
            worksheet.Cells[1, 1].Value = "First Name";
            worksheet.Cells[1, 2].Value = "Last Name";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "Phone Number";
            worksheet.Cells[1, 5].Value = "Account Number";

            int row = 2;
            foreach (var customer in customerList)
            {
                worksheet.Cells[row, 1].Value = customer.FirstName;
                worksheet.Cells[row, 2].Value = customer.LastName;
                worksheet.Cells[row, 3].Value = customer.Email;
                worksheet.Cells[row, 4].Value = customer.PhoneNumber;
                worksheet.Cells[row, 5].Value = customer.AccountNumber;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            return await package.GetAsByteArrayAsync();
        }
       */

    }

}

