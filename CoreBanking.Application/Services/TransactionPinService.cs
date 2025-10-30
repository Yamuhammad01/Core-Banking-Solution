using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Responses;
using CoreBanking.Application.Security;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs.TransactionDto;
using Microsoft.AspNetCore.Identity;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace CoreBanking.Application.Services
{
    public class TransactionPinService : ITransactionPinService
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IPasswordHasher<Customer> _pinHasher;
        private readonly ICodeHasher _codeHasher;
        private readonly IPinValidationService _pinValidator;

        public TransactionPinService(UserManager<Customer> userManager,
            IPasswordHasher<Customer> pinHasher,
            ICodeHasher codeHasher,
            IPinValidationService pinValidationService)
        {
            _userManager = userManager;
            _pinHasher = pinHasher;
            _codeHasher = codeHasher;
            _pinValidator = pinValidationService;
        }

        public async Task<Responses.ApiResponses> SetTransactionPinAsync(string userId, SetPinRequestDto request)
        {
            if (request.Pin != request.ConfirmPin)
                return new Responses.ApiResponses(false, "PINs do not match.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new Responses.ApiResponses(false, "User not found");

            var pinCheck = _pinValidator.ValidatePin(request.Pin, user.TransactionPin);
            if (!pinCheck.Success)
                return pinCheck;

            // Generate salt and hash
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);
            var codeHash = _codeHasher.HashCode(request.Pin, salt);

            //Hash the new pin
            user.TransactionPin = codeHash;
            user.PinSalt = salt;
           // user.TransactionPin = _pinHasher.HashPassword(user, request.Pin);
            await _userManager.UpdateAsync(user);

            return new Responses.ApiResponses(true, "Transaction PIN set successfully");
        }
    }
}
