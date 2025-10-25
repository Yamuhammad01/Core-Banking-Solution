using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Security
{
    public interface ICodeHasher
    {
        string HashCode(string code, string salt);
        string Generate6DigitCode();
        bool CryptographicEquals(string a, string b);
    }
}
