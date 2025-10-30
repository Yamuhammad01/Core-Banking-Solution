using CoreBanking.Application.Command.TransactionPinCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Security;
using CoreBanking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks; 

namespace CoreBanking.Application.CommandHandlers.TransactionPinCH
{
    public class VerifyPinResetHandler : IRequestHandler<VerifyPinResetCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IBankingDbContext _dbContext;
        private readonly ICodeHasher _codeHasher;

        public VerifyPinResetHandler(
            UserManager<Customer> userManager,
            IBankingDbContext dbContext,
            ICodeHasher codeHasher)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _codeHasher = codeHasher;
        }

        public async Task<Result> Handle(VerifyPinResetCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result.Failure("User not found");

            // check if the code is in the db
            var record = await _dbContext.EmailConfirmations
                .Where(x => x.Email == user.Email && x.Purpose == "TransactionPinReset" && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (record == null)
               return Result.Failure("Reset Code not found. Please request a new one");
            //check if the code has expired
                if (record.ExpiresAt < DateTime.UtcNow)
                    return Result.Failure("Reset code has expired");
           
            // Verify code
            var computedHash = _codeHasher.HashCode(request.Code, record.Salt);
            if (!_codeHasher.CryptographicEquals(computedHash, record.CodeHash))
                return Result.Failure("Invalid Reset Code");

            if (string.IsNullOrEmpty(record.Salt) || string.IsNullOrEmpty(record.CodeHash))
                return Result.Failure("Invalid reset record. Please request a new one");

            // Mark code as used
            record.IsUsed = true;
            _dbContext.EmailConfirmations.Update(record);

            //Generate new salt for PIN
             var newSaltBytes = RandomNumberGenerator.GetBytes(16);
            var newSalt = Convert.ToBase64String(newSaltBytes);

           // Hash new PIN using  CodeHasher (HMAC + salt)
            var newPinHash = _codeHasher.HashCode(request.NewTransactionPin, newSalt);

            // Update the users Transaction PIN
            user.TransactionPin = newPinHash;
            user.PinSalt = newSalt;

            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success("Transaction PIN successfully reset.");
        }
    }
}
