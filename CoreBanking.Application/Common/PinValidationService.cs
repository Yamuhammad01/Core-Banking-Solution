using CoreBanking.Application.Responses;
using CoreBanking.Application.Security;
using CoreBanking.Domain.Entities;
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
        ApiResponses ValidateAndHashNewPin(string pin);
        ApiResponses VerifyExistingPin(string inputPin, string storedHash, string storedSalt);//for transaction operation 
        ApiResponses ChangePin(string oldPin, string newPin, string storedHash, string storedSalt); 
    }

    public class PinValidationService : IPinValidationService
    {
        private readonly ICodeHasher _codeHasher;
        public PinValidationService(ICodeHasher codeHasher)
        {
            _codeHasher = codeHasher;
        }

        public ApiResponses ValidateAndHashNewPin(string pin)
        {
            if (string.IsNullOrEmpty(pin))
                return new ApiResponses(false, "Please input your transaction PIN");

            if (pin.Length != 4)
                return new ApiResponses(false, "Transaction PIN must be 4 digits");

            if (!pin.All(char.IsDigit))
                return new ApiResponses(false, "Transaction PIN must contain only numbers");

            // Generate new salt
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);

            // Hash the PIN with the generated salt
            var hashedPin = _codeHasher.HashCode(pin, salt);

            return new ApiResponses(true, "PIN generated successfully");
        }
       // Verify PIN for transactions operations
        public ApiResponses VerifyExistingPin(string inputPin, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                return new ApiResponses(false, "User PIN not found");

            if (string.IsNullOrEmpty(inputPin))
                return new ApiResponses(false, "Please input your transaction PIN");

            var computedHash = _codeHasher.HashCode(inputPin, storedSalt);

            if (!_codeHasher.CryptographicEquals(computedHash, storedHash))
                return new ApiResponses(false, "Invalid Transaction PIN");

            return new ApiResponses(true, "PIN verification successful");
        }

        public ApiResponses ChangePin(string oldPin, string newPin, string storedHash, string storedSalt)
        {
            // Step 1: Verify old PIN
            var verifyResult = VerifyExistingPin(oldPin, storedHash, storedSalt);
            if (!verifyResult.Success)
                return new ApiResponses(false, "Old Transaction PIN is incorrect");

            // Step 2: Validate and hash new PIN
            var newPinResult = ValidateAndHashNewPin(newPin);
            if (!newPinResult.Success)
                return newPinResult;

            return new ApiResponses(true, "Transaction PIN changed successfully");
        }
    }
}
