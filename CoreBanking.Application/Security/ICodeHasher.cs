namespace CoreBanking.Application.Security
{
    public interface ICodeHasher
    {
        string HashCode(string code, string salt);
        string Generate6DigitCode();
        bool CryptographicEquals(string a, string b);
    }
}
