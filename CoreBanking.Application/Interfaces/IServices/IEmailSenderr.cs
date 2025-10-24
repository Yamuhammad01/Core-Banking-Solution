using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.DTOs.AccountDto;
using CoreBanking.Application.Common;


namespace CoreBanking.Application.Interfaces.IServices
{
    public interface IEmailSenderr
    {
        Task SendEmailAsync(Message message);
    }

}
