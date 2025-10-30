using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.DTOs.TransactionDto
{
    public class ApiResponses
    {
        public string TransactionReference { get; set; } = default!;
        public string Message { get; set; } = default!;
        public bool Success { get; set; }
    }
}
