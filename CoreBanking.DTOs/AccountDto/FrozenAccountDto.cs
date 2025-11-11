namespace CoreBanking.DTOs.AccountDto
{
    public class FrozenAccountDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal AccountBalance { get; set; }
        public bool IsFrozen { get; set; } 
    }
}
