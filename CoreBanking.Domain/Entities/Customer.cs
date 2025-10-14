﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Domain.Entities
{
    public class Customer : IdentityUser
    {
       
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? TransactionPinHash { get; set; } // hashed PIN
        public bool HasSetPin => !string.IsNullOrEmpty(TransactionPinHash);
        public BankAccount BankAccount { get; set; }

        public ICollection<Transactions> Transactions { get; set; } = new List<Transactions>();
    }
}
