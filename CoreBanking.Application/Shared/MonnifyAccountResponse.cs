using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Shared
{
    public class MonnifyAccountResponse
    {
        public bool Success { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string ErrorMessage { get;  set; }
    }
    

}
