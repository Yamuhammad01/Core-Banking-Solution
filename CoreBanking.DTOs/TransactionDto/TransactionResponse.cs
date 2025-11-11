namespace CoreBanking.DTOs.TransactionDto
{
    public class TransactionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!; 
        public string? Reference { get; set; } // TransactionResponse
        public decimal? NewBalance { get; set; }
    }
}
