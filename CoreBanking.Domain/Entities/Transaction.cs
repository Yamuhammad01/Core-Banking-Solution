using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CoreBanking.Domain.Enums;

namespace CoreBanking.Domain.Entities
{
    public class Transactions
    {
        public Guid Id { get; set; }
        public string Reference { get; set; } = Guid.NewGuid().ToString("N");
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid BankAccountId { get; set; } //PK in BankAccount entity
        public BankAccount BankAccounts { get; set; }
        public string UserId { get; set; } //PK in BankAccount entity
        public Customer Customers { get; set; }
    }
}
