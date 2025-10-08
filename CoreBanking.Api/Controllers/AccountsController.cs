using CoreBanking.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CoreBanking.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CoreBanking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountsController(AccountService accountService)
        {
            _accountService = accountService;
        }

        private string GetUserId() =>
         User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /*[Authorize]
        [HttpPost("open")]
        [ProducesResponseType(typeof(GetAccountDto), 201)]
        public async Task<IActionResult> OpenAccount([FromBody] CreateAccountDto createaccounttdto)
        {
            var customerId = GetUserId();
            var account = await _accountService.OpenAccountAsync(customerId, createaccounttdto);
            return CreatedAtAction(nameof(GetAccountById), new { id = account.Id }, account);
        } */


        [Authorize]
        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            return Ok("Welcome User — only you can see this.");
        }

        [Authorize]
        [HttpGet("myaccount")]
        public async Task<IActionResult> GetMyAccounts()
        {
            var customerId = GetUserId();
            var accounts = await _accountService.GetAccountsAsync(customerId);
            return Ok(accounts);
        }
    }
}
