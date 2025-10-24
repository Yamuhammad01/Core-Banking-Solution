using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Domain.Entities
{
    public class EmailConfirmation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = default!;           // FK to AspNetUsers.Id
        public string Email { get; set; } = default!;      // user email (for easy lookup)
        public string CodeHash { get; set; } = default!;         // hashed OTP
        public string Salt { get; set; } = default!;             // salt used for hashing
        public string Purpose { get; set; } = "EmailConfirmation";
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
    }
}
