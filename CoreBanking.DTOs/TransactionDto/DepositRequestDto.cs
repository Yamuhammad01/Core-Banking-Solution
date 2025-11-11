namespace CoreBanking.DTOs.TransactionDto
{
    public class DepositRequestDto
    {
        public string AccountNumber { get; set; } = null!; 
        public decimal Amount { get; set; }
        public string? Narration { get; set; }
    }
}
