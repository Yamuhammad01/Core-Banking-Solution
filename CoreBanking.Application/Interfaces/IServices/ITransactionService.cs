using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Application.Responses;
using CoreBanking.DTOs.TransactionDto;
using Octokit.Internal;
using Refit;

namespace CoreBanking.Application.Interfaces.IServices
{
    public interface ITransactionService
    {
        Task<TransactionResponseDto> TransferFundsAsync(string userId, TransferRequestDto request);
        Task<TransactionResponseDto> DepositAsync(string UserId, DepositRequestDto request);
        Task<Responses.ApiResponses> WithdrawAsync(string userId, WithdrawalRequestDto request);
    }
}
