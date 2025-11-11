using CoreBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.Application.Interfaces.IServices
{
    public interface IBankingDbContext
    {
        DbSet<EmailConfirmation> EmailConfirmations { get; }
        DbSet<BankAccount> BankAccounts { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        //Task SaveChangesAsync();
    }
}
