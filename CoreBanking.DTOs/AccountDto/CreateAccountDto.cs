using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.DTOs.AccountDto
{
    public class CreateAccountDto
    {
        public string AccountType { get; set; } = default!;
        public string Currency { get; set; } = "NGN";
        public decimal InitialDeposit { get; set; } = 0;
    }
}
