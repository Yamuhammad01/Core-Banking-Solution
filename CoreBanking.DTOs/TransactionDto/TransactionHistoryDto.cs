namespace CoreBanking.DTOs.TransactionDto
{
    public class TransactionHistoryDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
