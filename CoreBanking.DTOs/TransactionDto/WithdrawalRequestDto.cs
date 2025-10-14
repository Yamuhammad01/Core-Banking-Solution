using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.DTOs.TransactionDto
{
    public class WithdrawalRequestDto
    {
        public decimal Amount { get; set; }
        public string? Narration { get; set; }
        public string TransactionPin { get; set; } = string.Empty; 
    }
}
