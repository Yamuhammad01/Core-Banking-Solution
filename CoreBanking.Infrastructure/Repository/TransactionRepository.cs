using CoreBanking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Domain.Entities;
using System.Security.Principal;
using CoreBanking.Application.Interfaces.IRepository;

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

       
        public async Task<IEnumerable<Transactions>> GetByAccountIdAsync(string UserId) =>
            await _dbContext.Transactions
                .Where(t => t.UserId == UserId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
    }
}

