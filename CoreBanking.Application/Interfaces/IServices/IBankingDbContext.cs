using CoreBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
