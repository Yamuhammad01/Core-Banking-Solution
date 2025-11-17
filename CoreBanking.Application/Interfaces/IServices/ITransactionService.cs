using CoreBanking.Application.Common;
using CoreBanking.DTOs.TransactionDto;

namespace CoreBanking.Application.Interfaces.IServices
{
    public interface ITransactionService
    {
        Task<Result> TransferFundsAsync(string userId, TransferRequestDto request);
        Task<Responses.ApiResponses> DepositAsync(string UserId, DepositRequestDto request);
        Task<Result> WithdrawAsync(string userId, WithdrawalRequestDto request);
    }
}
