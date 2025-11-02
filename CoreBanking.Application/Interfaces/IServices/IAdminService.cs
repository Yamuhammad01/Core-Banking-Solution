using CoreBanking.Application.Common;
using CoreBanking.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Interfaces.IServices
{
    public interface IAdminService
    {

        Task<Result> FreezeAccountAsync(string email);
        Task<Result> UnfreezeAccountAsync(string email);
        Task<Result> DeactivateAccountAsync(string email);
        Task<Result> ReactivateAccountAsync(string email);

    }
}
