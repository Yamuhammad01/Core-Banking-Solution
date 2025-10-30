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
        Result ValidateAndHashNewPin(string pin);
        Result VerifyExistingPin(string inputPin, string storedHash, string storedSalt); //for transaction operation 
        Result ChangePin(string oldPin, string newPin, string storedHash, string storedSalt);
    }

    public class PinValidationService : IPinValidationService
    {
        private readonly ICodeHasher _codeHasher;
        public PinValidationService(ICodeHasher codeHasher)
        {
            _codeHasher = codeHasher;
        }

        public Result ValidateAndHashNewPin(string pin)
        {
            if (string.IsNullOrEmpty(pin))
                return Result.Failure("Please input your transaction PIN");

            if (pin.Length != 4)
                return Result.Failure("Transaction PIN must be 4 digits");

            if (!pin.All(char.IsDigit))
                return Result.Failure("Transaction PIN must contain only numbers");

            // Generate new salt
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);

            // Hash the PIN with the generated salt
            var hashedPin = _codeHasher.HashCode(pin, salt);

            var data = new PinHashResult
            {
                HashedPin = hashedPin,
                Salt = salt
            };

            return Result.Success("PIN generated successfully", data);
        }

        // Verify PIN for transactions operations
        public Result VerifyExistingPin(string inputPin, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                return Result.Failure("User PIN not found");

            if (string.IsNullOrEmpty(inputPin))
                return Result.Failure("Please input your transaction PIN");

            var computedHash = _codeHasher.HashCode(inputPin, storedSalt);

            if (!_codeHasher.CryptographicEquals(computedHash, storedHash))
                return Result.Failure("Invalid Transaction PIN");

            return Result.Success("PIN verification successful");
        }

        public Result ChangePin(string oldPin, string newPin, string storedHash, string storedSalt)
        {
            // Step 1: Verify old PIN
            var verifyResult = VerifyExistingPin(oldPin, storedHash, storedSalt);
            if (!verifyResult.Succeeded)
                return Result.Failure("Old Transaction PIN is incorrect");

            // Step 2: Validate and hash new PIN
            var newPinResult = ValidateAndHashNewPin(newPin);
            if (!newPinResult.Succeeded)
                return newPinResult;

            return Result.Success("Transaction PIN changed successfully");
        }

        public class PinHashResult
        {
            public string HashedPin { get; set; }
            public string Salt { get; set; }
        }
    }
}
