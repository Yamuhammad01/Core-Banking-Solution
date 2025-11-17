using CoreBanking.Application.Common;

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
