using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Domain.Entities;
using CoreBanking.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Infrastructure.Repository
{
    public class EmailVerificationRepository : GenericRepository<EmailVerification>, IEmailVerificationRepository
    {
        private readonly CoreBankingDbContext _dbContext;

        public EmailVerificationRepository(CoreBankingDbContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<EmailVerification?> GetLatestByUserAsync(Guid userId)
        {
            return await _dbContext.EmailVerifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
