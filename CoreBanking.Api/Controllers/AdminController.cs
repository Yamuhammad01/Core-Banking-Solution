using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Services;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs.AccountDto;
using CoreBanking.DTOs.TransactionDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoreBanking.Api.Controllers
{
  //  [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly TransactionService _transactionService;
        private readonly AdminService _adminService;
        public AdminController(AccountService accountService,
            TransactionService transactionService,
            AdminService adminService) 
        { 
            _accountService = accountService;
            _transactionService = transactionService;
            _adminService = adminService;
        }

        // get all customers 
        [HttpGet("get-all-customers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _adminService.GetAllCustomersAsync(); 

            if (customers == null || !customers.Any())
                return NotFound("No customers found.");

            return Ok(customers);
        }

        // get all frozen accounts 
        // get all customers 
        [HttpGet("get-all-frozen-accounts")]
        public async Task<IActionResult> GetAllFrozenAccountAsync()
        {
            var customers = await _adminService.GetAllFrozenAccounts();

            if (customers == null || !customers.Any())
                return NotFound("No any frozen account found");

            return Ok(customers);
        }

        //get a customer by email
        [HttpGet("get-customerinfo-by-email")]
        public async Task<IActionResult> GetAccountByEmail(string email)
        {
            var customer = await _accountService.GetCustomerByEmailAsync(email);

            if (customer == null)
                return NotFound("Customer not found");

            if (customer.BankAccount == null)
                return NotFound("Customer has no linked bank account.");

            var customerInfo = new ProfileDto
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                AccountNumber = customer.BankAccount.AccountNumber,
                AccountBalance = customer.BankAccount.Balance,
                IsActive = customer.IsActive
            };
            return Ok(customerInfo);
            
        }
        //get total number of customers
        [HttpGet("total-customers")]
        public async Task<IActionResult> GetCustomerCount()
        {
            var totalCustomers = await _adminService.TotalCustomersAsync();
            return Ok(new { TotalCustomers = totalCustomers });
        }

        //get total number of active customers
        [HttpGet("total-active-customers")]
        public async Task<IActionResult> GetTotalActiveCustomerCountAsync()
        {
            var activeCustomers = await _adminService.TotalActiveCustomers();
            return Ok(new { TotalActiveCustomers = activeCustomers });
        }

        //get total number of inactive customers
        [HttpGet("total-inactive-customers")]
        public async Task<IActionResult> GetTotalInActiveCustomerCountAsync()
        {
            var inActiveCustomers = await _adminService.TotalInActiveCustomers();
            return Ok(new { TotalInActiveCustomers = inActiveCustomers });
        }
        // Deposit - only Admin can deposit
        [HttpPost("deposit")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DepositAsync([FromBody] DepositRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid or missing token.");

            if (string.IsNullOrEmpty(dto.AccountNumber))
                return BadRequest("Account number is required for admin deposits.");

            var result = await _transactionService.AdminDepositAsync(userId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update-account")]
        public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateProfileDto request)
        {

            var success = await _accountService.UpdateCustomerProfileAsync(request);
            return Ok(new { message = success });
        }

        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteProfileAsync(string email)
        {

            var success = await _accountService.DeleteCustomerProfileAsync(email);
            return Ok(new { message = success });
        }

        [HttpPost("freeze-account")]
        public async Task<IActionResult> FreezeAccountAsync(string email)
        {
            var result = await _adminService.FreezeAccountAsync(email);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }

        [HttpPost("unfreeze-account")]
        public async Task<IActionResult> UnfreezeAccountAsync(string email)
        {
            var result = await _adminService.UnfreezeAccountAsync(email);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }

        //deactivate account 
        [HttpPost("deactivate-account")]
        public async Task<IActionResult> DeactivateAccountAsync(string email)
        {
            var result = await _adminService.DeactivateAccountAsync(email);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }

        [HttpPost("reactivate-account")]
        public async Task<IActionResult> ReactivateAccountAsync(string email)
        {
            var result = await _adminService.ReactivateAccountAsync(email);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }
    }

}
   
