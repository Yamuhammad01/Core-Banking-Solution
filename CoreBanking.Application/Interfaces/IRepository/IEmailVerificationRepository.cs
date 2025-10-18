using CoreBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Interfaces.IRepository
{
    public interface IEmailVerificationRepository 
    {
        Task<EmailVerification?> GetLatestByUserAsync(string userId);
    }
}
