using CoreBanking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Domain.Entities;
using CoreBanking.Application.Interfaces;
using System.Security.Principal;

namespace CoreBanking.Infrastructure.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly CoreBankingDbContext _dbContext;
        public TransactionRepository(CoreBankingDbContext coreBankingDbContext) 
        {
            _dbContext = coreBankingDbContext;
        }

        public async Task AddAsync(Transactions transaction)
        {
            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
        }

       
        public async Task<IEnumerable<Transactions>> GetByAccountIdAsync(Guid accountId) =>
            await _dbContext.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
    }
}

