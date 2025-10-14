using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CoreBankingDbContext _dbContext;
        private IDbContextTransaction? _tx;
        public UnitOfWork(CoreBankingDbContext db) 
        { 
            _dbContext = db; 
        }

        public async Task BeginTransactionAsync() 
        { 
            _tx = await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        { 
            if (_tx != null) 
            { 
                await _dbContext.SaveChangesAsync(); 
                await _tx.CommitAsync(); _tx.Dispose(); _tx = null; 
            } 
        }
        public async Task RollbackAsync() 
        { 
            if (_tx != null) 
            { 
                await _tx.RollbackAsync(); _tx.Dispose(); _tx = null; 
            }
        }
        public async Task<int> SaveChangesAsync() => await _dbContext.SaveChangesAsync();
    }
}
