namespace CoreBanking.Application.Interfaces.IServices
{
    public interface ITransactionEmailService
    {
        Task SendTransactionEmailAsync(
            string email,
            string firstName,
            string lastName,
            string transactionType,
            decimal amount,
            string accountNumber,
            string reference,
            decimal balance,
            DateTime date,
            string senderFullName = null);
    }
}
