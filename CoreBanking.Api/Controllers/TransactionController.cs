using CoreBanking.Application.Interfaces;
using CoreBanking.Application.Services;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs.TransactionDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public TransactionController(ITransactionService transferService, ITransactionRepository transactionRepository, AccountService accountService, TransactionService transactionService)
        {
            _transferService = transferService;
            _repo = transactionRepository;
            _accountService = accountService;
            _transactionService = transactionService;
        }


        /// Transfer funds between accounts.
        [Authorize]
        [HttpPost("transfer/funds")]
        [ProducesResponseType(typeof(TransferResponseDto), 200)]
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
        [HttpGet("transactionhistory")]
        public async Task<IActionResult> GetTransactions()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid or missing token.");

            var txns = await _repo.GetByAccountIdAsync(userId);
            return Ok(txns);
        }

        [HttpPost("deposit")]
        //[Authorize(Roles = "Admin")]
        [Authorize]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid or missing token.");

            var result = await _transactionService.DepositAsync(userId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpPost("withdraw")]
        [Authorize] // any authenticated user
        public async Task<IActionResult> Withdraw([FromBody] WithdrawalRequestDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid or missing token.");

            var result = await _transactionService.WithdrawAsync(userId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

    }
}
