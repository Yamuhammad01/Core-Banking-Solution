namespace CoreBanking.DTOs.TransactionDto
{
    public class WithdrawalRequestDto
    {
        public decimal Amount { get; set; }
        public string? Narration { get; set; }
        public string TransactionPin { get; set; } = string.Empty; 
    }
}
