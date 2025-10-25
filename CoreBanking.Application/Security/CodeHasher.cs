using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Security
{
    public class CodeHasher : ICodeHasher
    {
        public string HashCode(string code, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using var hmac = new HMACSHA256(saltBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(hash);
        }

        public bool CryptographicEquals(string a, string b)
        {
            var aBytes = Convert.FromBase64String(a);
            var bBytes = Convert.FromBase64String(b);
            return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
        }

        public string Generate6DigitCode()
        {
            //Generate a secure random number between 0 and 999999
            var val = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return val.ToString("D6"); // zero-padded to 6 digits
        }
    }
}
