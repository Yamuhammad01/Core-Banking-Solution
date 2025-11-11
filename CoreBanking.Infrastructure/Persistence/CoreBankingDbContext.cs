using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CoreBanking.Domain.Entities;
using CoreBanking.Application.Interfaces.IServices;

namespace CoreBanking.Infrastructure.Persistence
{
    public class CoreBankingDbContext : IdentityDbContext<Customer>, IBankingDbContext
    {
        public CoreBankingDbContext(DbContextOptions<CoreBankingDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Transactions> Transactions { get; set; }
        public DbSet<EmailConfirmation> EmailConfirmations { get; set; } = default!;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(cancellationToken);

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // One-to-one mapping between Customer and BankAccount
            builder.Entity<Customer>()
                .HasOne(c => c.BankAccount)
                .WithOne(b => b.Customers)
                .HasForeignKey<BankAccount>(b => b.CustomerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Transactions>()
           .HasOne(al => al.BankAccounts)
           .WithMany(s => s.Transactions)
           .HasForeignKey(al => al.BankAccountId); 

            builder.Entity<Transactions>()
         .HasOne(al => al.Customers)
         .WithMany(s => s.Transactions)
         .HasForeignKey(al => al.UserId);

        }
      

    }
}
