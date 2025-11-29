using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.DTOs.AccountDto
{
    public record CustomerCreatedMessage(
     string FirstName,
     string LastName,
     string Email,
     string Password,
     string ConfirmPassword,
     string PhoneNumber 

   );
}
