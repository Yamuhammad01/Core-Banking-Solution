using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.DTOs.Events
{
    public class UserCreated
    {
        public string UserId { get; set; } 
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
    }
}
