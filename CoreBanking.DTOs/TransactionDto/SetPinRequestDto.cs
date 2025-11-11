namespace CoreBanking.DTOs.TransactionDto
{
    public class SetPinRequestDto
    {
        public string Pin { get; set; } = string.Empty;
        public string ConfirmPin { get; set; } = string.Empty;
    }
}
