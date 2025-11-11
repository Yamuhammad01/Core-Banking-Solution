using CoreBanking.DTOs.AccountDto;

namespace CoreBanking.Application.Interfaces.IRepository
{
    public interface IAdminRepository
    {
        Task<List<ProfileDto>> GetAllCustomersAsync();
       // Task<byte[]> ExportStaffListToExcelAsync();
        Task<int> GetCustomerCountAsync();
        Task<int> GetActiveUsersCountAsync();
        Task<int> GetInactiveUsersCountAsync();
        Task<List<FrozenAccountDto>> GetAllFrozenAccountAsync();
    }
}
