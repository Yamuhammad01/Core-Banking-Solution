using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Services;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs.TransactionDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CoreBanking.Domain.Enums;
using CoreBanking.Infrastructure.EmailServices;
using CoreBanking.Infrastructure.EmailServices;
using Microsoft.Win32;
using Octokit;

namespace CoreBanking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transferService;
        private readonly ITransactionRepository _repo;
        private readonly AccountService _accountService;
        private readonly TransactionService _transactionService;
        private readonly EmailTemplateService _emailTemplateService;
        public TransactionController(ITransactionService transferService, ITransactionRepository transactionRepository, AccountService accountService, TransactionService transactionService, EmailTemplateService emailTemplateService)
        {
            _transferService = transferService;
            _repo = transactionRepository;
            _accountService = accountService;
            _transactionService = transactionService;
            _emailTemplateService = emailTemplateService;
        }


        /// Transfer funds between accounts.
        [Authorize]
        [HttpPost("transfer-funds")]
        [ProducesResponseType(typeof(TransactionResponse), 200)]
        public async Task<IActionResult> Transfer([FromBody] TransferRequestDto request)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid or missing token.");

            var result = await _transferService.TransferFundsAsync(userId, request);
            return Ok(result);
        }

        // get the transaction history for the user with the id
        [Authorize]
        [HttpGet("transaction-history")]
        public async Task<IActionResult> GetTransactions()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid or missing token.");

            var txns = await _repo.GetByAccountIdAsync(userId);

            if (txns == null || !txns.Any())
                return NotFound("You dont have a transaction history yet");

            var transactionDtos = txns.Select(t => new TransactionHistoryDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Reference = t.Reference,
                Description = t.Description
            }).ToList();

          
            return Ok(transactionDtos);
        }

       /* [HttpPost("deposit")]
        [Authorize]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid or missing token.");

            var result = await _transactionService.DepositAsync(userId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        } */


        [HttpPost("withdraw")]
        [Authorize] // any authenticated user
        public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid or missing token.");

            var result = await _transactionService.WithdrawAsync(userId, dto);
            return result.Succeeded ? Ok(result) : BadRequest(result);
        }

    }
}
