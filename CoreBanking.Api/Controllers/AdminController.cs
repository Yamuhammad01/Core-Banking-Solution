using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces;
using CoreBanking.Application.Services;
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
        public AdminController(AccountService accountService, TransactionService transactionService) 
        { 
            _accountService = accountService;
            _transactionService = transactionService;
        }
        //get a customer by id 
        [HttpGet("getcustomer{id}")]
        public async Task<IActionResult> GetAccountById(Guid id)
        {
            var account = await _accountService.GetByIdAsync(id);
            if (account == null) return NotFound();
            return Ok(account);
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
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {

            var success = await _accountService.UpdateCustomerProfileAsync(request);
            return Ok(new { message = success });
        }


    }
}