namespace CoreBanking.DTOs.TransactionDto
{
    public class TransferRequestDto
    {

        public string AccountNumber { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Narration { get; set; } = string.Empty;
        public string TransactionPin {  get; set; } = string.Empty ;
    }
}
