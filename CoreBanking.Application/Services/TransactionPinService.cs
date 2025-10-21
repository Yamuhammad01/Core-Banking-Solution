using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Responses;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs.TransactionDto;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Services
{
    public class TransactionPinService : ITransactionPinService
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IPasswordHasher<Customer> _pinHasher;

        public TransactionPinService(UserManager<Customer> userManager, IPasswordHasher<Customer> pinHasher)
        {
            _userManager = userManager;
            _pinHasher = pinHasher;
        }

        public async Task<ApiResponses> SetTransactionPinAsync(string userId, SetPinRequestDto request)
        {
            // pin validation 
            if (request.Pin != request.ConfirmPin)
                return new ApiResponses(false, "PINs do not match.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new ApiResponses(false, "User not found");

            //check of user input an empty pin
            if (string.IsNullOrEmpty(request.Pin))
                return new ApiResponses(false, "Please input your transaction pin");

            //check if pin is exactly 4 digits
            if (request.Pin.Length != 4)
            {
                return new ApiResponses(false, "Transaction PIN must be 4 digits");
            }

            // check if the pin contain a character
            if (!request.Pin.All(char.IsDigit))
            {
                return new ApiResponses(false, "Transaction PIN must contain only numbers");
            }

            //check if a pin already exist
            if (!string.IsNullOrEmpty(user.TransactionPin))
                return new ApiResponses(false, "Transaction PIN already exists");

            //Hash the new pin
            user.TransactionPin = _pinHasher.HashPassword(user, request.Pin);
            await _userManager.UpdateAsync(user);

            return new ApiResponses(true, "Transaction PIN set successfully");
        }

        public async Task<bool> VerifyTransactionPinAsync(string userId, string pin)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TransactionPin))
                return false;

            var result = _pinHasher.VerifyHashedPassword(user, user.TransactionPin, pin);
            return result == PasswordVerificationResult.Success;
        }
    }
}
