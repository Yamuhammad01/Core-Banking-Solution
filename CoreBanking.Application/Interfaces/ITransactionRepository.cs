using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Domain.Entities;

namespace CoreBanking.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transactions transaction);
        public Task<IEnumerable<Transactions>> GetByAccountIdAsync(string UserId);
    }
}
