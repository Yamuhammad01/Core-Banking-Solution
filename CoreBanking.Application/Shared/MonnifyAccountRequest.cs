using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Shared
{
    public class MonnifyAccountRequest
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;
    }
}
