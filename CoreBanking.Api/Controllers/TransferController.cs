using CoreBanking.Application.Interfaces;
using CoreBanking.Application.Services;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoreBanking.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;
        private readonly ITransactionRepository _repo;
        private readonly AccountService _accountService;
        public TransferController(ITransferService transferService, ITransactionRepository transactionRepository, AccountService accountService) 
        {
            _transferService = transferService;
            _repo = transactionRepository;
            _accountService = accountService;
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
        [HttpGet("transactionhistory{accountId}")]
        public async Task<IActionResult> GetTransactions(Guid accountId)
        {
            var txns = await _repo.GetByAccountIdAsync(accountId);
            return Ok(txns);
        }

    }
}
