using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.DTOs.AccountDto;
using CoreBanking.Infrastructure.EmailServices;

namespace CoreBanking.Application.Interfaces.IServices
{
    public interface IEmailSenderr
    {
        void SendEmail(Message message);
        Task SendEmailAsync(Message message);
    }
}
