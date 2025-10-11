using CoreBanking.DTOs.TransactionDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionResponseDto> DepositAsync(Guid adminId, DepositRequestDto request);
        Task<TransactionResponseDto> WithdrawAsync(Guid userId, WithdrawalRequestDto request);
    }
}
