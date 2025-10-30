using CoreBanking.Application.Responses;
using CoreBanking.Application.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Common
{
    public interface IPinValidationService
    {
        ApiResponses ValidatePin(string pin, string? existingPin = null, string? salt = null);
    }

    public class PinValidationService : IPinValidationService
    {
        private readonly ICodeHasher _codeHasher;
        public PinValidationService(ICodeHasher codeHasher)
        {
            _codeHasher = codeHasher;
        }

        public ApiResponses ValidatePin(string pin, string? existingPin = null, string? salt = null)
        {
            if (string.IsNullOrEmpty(pin))
                return new ApiResponses(false, "Please input your transaction PIN");

            if (pin.Length != 4)
                return new ApiResponses(false, "Transaction PIN must be 4 digits");

            if (!pin.All(char.IsDigit))
                return new ApiResponses(false, "Transaction PIN must contain only numbers");
            //seeting 
            if (string.IsNullOrEmpty(existingPin))
                return new ApiResponses(true, "PIN set successfully");

            // Generate salt and hash
          //  var saltBytes = RandomNumberGenerator.GetBytes(16);
            //var salt = Convert.ToBase64String(saltBytes);
            //var codeHash = _codeHasher.HashCode(request.Pin, salt);

            // verify pin 
            if (!string.IsNullOrEmpty(salt))
            {
                var hashedPin = _codeHasher.HashCode(pin, salt);
                if (hashedPin != existingPin)
                    return new ApiResponses(false, "Invalid transaction PIN");
            }

            return new ApiResponses(true, "PIN validation successful");
        }
    }
}
