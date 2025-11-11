using CoreBanking.Domain.Entities;

namespace CoreBanking.Application.Interfaces.IRepository
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transactions transaction);
        public Task<IEnumerable<Transactions>> GetByAccountIdAsync(string UserId);

    }
}
