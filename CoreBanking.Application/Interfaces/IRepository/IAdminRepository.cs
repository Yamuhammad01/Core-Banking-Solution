using CoreBanking.DTOs.AccountDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Interfaces.IRepository
{
    public interface IAdminRepository
    {
        Task<List<ProfileDto>> GetAllCustomersAsync();
    }
}
