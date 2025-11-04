using CoreBanking.Application.CommandHandlers;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Services;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs;
using CoreBanking.DTOs.AccountDto;
using CoreBanking.DTOs.TransactionDto;
using CoreBanking.Infrastructure.EmailServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using System.Security.Claims;
using System.Threading.Tasks;
namespace CoreBanking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly TransactionPinService _pinService;
        private readonly IEmailSenderr _emailSender;
        private readonly UserManager<Customer> _userManager;    
       

        public AccountsController(AccountService accountService, 
            TransactionPinService transactionPinService,
            IEmailSenderr emailService,
            UserManager<Customer> userManager)
        {
            _accountService = accountService;
            _pinService = transactionPinService;
            _emailSender = emailService;
            _userManager = userManager;
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
        public async Task<IActionResult> GetDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            { 
                return NotFound();
            }
            var customer = await _userManager.FindByIdAsync(userId);
           
            if (customer == null) 
            { 
                return NotFound("Customer not found");
            }
           
            return Ok($"Welcome {customer.FirstName} — only you can see this");
        }


        [Authorize]
        [HttpPost("set-pin")]
        public async Task<IActionResult> SetPin([FromBody] SetPinRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _pinService.SetTransactionPinAsync(userId, request);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpGet("my-account")]
        public async Task<IActionResult> GetMyAccounts()
        {
            var customerId = GetUserId();
            var accounts = await _accountService.GetAccountsAsync(customerId);

            var accountDtos = accounts.Select(t => new GetAccountDto
            {
                Id = t.Id,
                AccountNumber = t.AccountNumber,
                AccountType = t.AccountType,
                Balance = t.Balance,
                Currency = t.Currency,
                Status = t.Status
            }).ToList();
                return Ok(accountDtos);
        }
        // customer 
        [Authorize]
        [HttpGet("my-profile")]
        public async Task <IActionResult> Profile()
        {
            var customerId = GetUserId();
            var customer = await _accountService.GetCustomerInfoAsync(customerId);

            if (customer == null)
                return NotFound("Customer not found");

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

        [Authorize]
        [HttpGet("check-balance")]
        public async Task<IActionResult> CheckBalance()
        {
            var customerId = GetUserId();
            var accounts = await _accountService.GetAccountsAsync(customerId);

            var BalanceDtos = accounts.Select(t => new BalanceDto
            {
               
                Balance = t.Balance,
               
            }).ToList();
            return Ok(BalanceDtos);
        }
        public class BalanceDto
        {
           public decimal Balance { get; set; } 
        }
    }
}
