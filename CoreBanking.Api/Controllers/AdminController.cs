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
        //get a customer by email
        [HttpGet("getcustomerinfo")]
        public async Task<IActionResult> GetAccountById(string email)
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
                AccountNumber = customer.BankAccount.AccountNumber

            };
            return Ok(customerInfo);
            
        }

        [HttpPatch("updatestatus{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAccountStatusDto updateaccountstatus)
        {
            await _accountService.UpdateStatusAsync(id, updateaccountstatus.Status);
            return NoContent();
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

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateProfileDto request)
        {

            var success = await _accountService.UpdateCustomerProfileAsync(request);
            return Ok(new { message = success });
        }

        [HttpDelete("delete-profile")]
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
   
