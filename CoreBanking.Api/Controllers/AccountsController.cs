using CoreBanking.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CoreBanking.DTOs;
using Microsoft.AspNetCore.Authorization;
using CoreBanking.DTOs.TransactionDto;
using CoreBanking.DTOs.AccountDto;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Infrastructure.EmailServices;
using CoreBanking.Application.CommandHandlers;
using MediatR;
using CoreBanking.Application.Common;
namespace CoreBanking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly TransactionPinService _pinService;
        private readonly IEmailSenderr _emailSender;
        private readonly IMediator _mediator;

        public AccountsController(AccountService accountService, TransactionPinService transactionPinService, IEmailSenderr emailService, IMediator mediator)
        {
            _accountService = accountService;
            _pinService = transactionPinService;
            _emailSender = emailService;
            _mediator = mediator;
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

        [HttpGet("send-email")]
        public async Task <IActionResult> SendEmail()
        {
            var message = new Message(
                new string[] { "idrismuhd418@gmail.com" },
                "Test email",
                "This is the content from our email."
            );

            await _emailSender.SendEmailAsync(message);

            return Ok("Email has been sent successfully!");
        }

        [Authorize]
        [HttpPost("set-pin")]
        public async Task<IActionResult> SetPin([FromBody] SetPinRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _pinService.SetTransactionPinAsync(userId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpPost("send-email-code")]
        public async Task<IActionResult> SendEmailCode([FromBody] SendEmailConfirmationCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { message = "Confirmation code sent" });
        }

        [HttpPost("verify-email-code")]
        public async Task<IActionResult> VerifyEmailCode([FromBody] VerifyEmailConfirmationCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { message = "Email confirmed successfully" });
        }

        [Authorize]
        [HttpGet("myaccount")]
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
    }
}
