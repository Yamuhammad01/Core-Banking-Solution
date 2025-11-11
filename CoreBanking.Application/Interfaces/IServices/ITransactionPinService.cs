using CoreBanking.Application.Common;
using CoreBanking.DTOs.TransactionDto;

namespace CoreBanking.Application.Interfaces.IServices
{
    public interface ITransactionPinService
    {
        Task<Result> SetTransactionPinAsync(string userId, SetPinRequestDto request);
       
    }
}
