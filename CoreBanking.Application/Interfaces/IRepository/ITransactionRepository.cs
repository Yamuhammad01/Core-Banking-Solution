using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs.TransactionDto;
using Refit;

namespace CoreBanking.Application.Interfaces.IRepository
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transactions transaction);
        public Task<IEnumerable<Transactions>> GetByAccountIdAsync(string UserId);

    }
}
