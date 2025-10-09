using CoreBanking.Application.Services;
using CoreBanking.DTOs.AccountDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreBanking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AccountService _accountService;
        public AdminController(AccountService accountService) 
        { 
            _accountService = accountService;
        }
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
    }
}
