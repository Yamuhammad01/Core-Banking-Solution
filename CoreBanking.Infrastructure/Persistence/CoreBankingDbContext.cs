﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CoreBanking.Domain.Entities;
using System.Reflection.Emit;

namespace CoreBanking.Infrastructure.Persistence
{
    public class CoreBankingDbContext : IdentityDbContext<Customer>
    {
        public CoreBankingDbContext(DbContextOptions<CoreBankingDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Transactions> Transactions { get; set; }
        

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
