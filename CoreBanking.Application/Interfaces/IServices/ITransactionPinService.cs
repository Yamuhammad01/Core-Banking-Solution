using CoreBanking.Application.Common;
using CoreBanking.Application.Responses;
using CoreBanking.DTOs.TransactionDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Interfaces.IServices
{
    public interface ITransactionPinService
    {
        Task<Result> SetTransactionPinAsync(string userId, SetPinRequestDto request);
       
    }
}
