using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.DTOs.TransactionDto;

namespace CoreBanking.Application.Interfaces
{
    public interface ITransferService
    {
        Task<TransferResponseDto> TransferFundsAsync(string userId, TransferRequestDto request);
    }
}
