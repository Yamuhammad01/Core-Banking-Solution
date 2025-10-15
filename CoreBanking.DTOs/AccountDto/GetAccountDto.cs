using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.DTOs.AccountDto
{
    public class GetAccountDto
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = default!;
        public string AccountType { get; set; } = default!;
        public decimal Balance { get; set; }
        public string Status { get; set; } = default!;
        public string Currency {  get; set; } 
    }
}
