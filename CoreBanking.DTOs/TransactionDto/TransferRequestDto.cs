using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CoreBanking.DTOs.TransactionDto
{
    public class TransferRequestDto
    {

        public string AccountNumber { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Narration { get; set; } = string.Empty;
        public string TransactionPin {  get; set; } = string.Empty ;
    }
}
