using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Domain.Entities
{
    public class BankAccount
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = default!;
        public string AccountType { get; set; } = default!;
        public decimal Balance { get; set; } = 0;
        public string Currency { get; set; } = "NGN";
        public string Status { get; set; } = "PendingApproval";

        public string CustomerId { get; set; } = default!; // Identity UserId (FK)
        public Customer Customers { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Transactions> Transactions { get; set; } = new List<Transactions>();
    }
}
