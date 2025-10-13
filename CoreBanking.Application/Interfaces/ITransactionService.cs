using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.DTOs.TransactionDto;

namespace CoreBanking.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<TransferResponseDto> TransferFundsAsync(string userId, TransferRequestDto request);
        Task<TransactionResponseDto> DepositAsync(string UserId, DepositRequestDto request);
        Task<TransactionResponseDto> WithdrawAsync(string userId, WithdrawalRequestDto request);
    }
}
