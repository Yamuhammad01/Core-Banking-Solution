using CoreBanking.Application.Shared;
using CoreBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Interfaces.IServices
{
    public interface IMonnifyService
    {
        Task<MonnifyAccountResponse> CreateDedicatedVirtualAccountAsync(MonnifyAccountRequest request);
    }

}
