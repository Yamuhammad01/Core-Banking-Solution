using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Application.Common;
using CoreBanking.Application.Responses;
using CoreBanking.DTOs.TransactionDto;
using Octokit.Internal;
using Refit;

namespace CoreBanking.Application.Interfaces.IServices
{
    public interface ITransactionService
    {
        Task<Result> TransferFundsAsync(string userId, TransferRequestDto request);
        Task<Responses.ApiResponses> DepositAsync(string UserId, DepositRequestDto request);
        Task<Result> WithdrawAsync(string userId, WithdrawalRequestDto request);
    }
}
