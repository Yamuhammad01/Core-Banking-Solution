using CoreBanking.Application.Command.TransactionPinCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace CoreBanking.Application.CommandHandlers.TransactionPinCH
{
    public class VerifyPinResetHandler : IRequestHandler<VerifyPinResetCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IBankingDbContext _dbContext;

        public VerifyPinResetHandler(
            UserManager<Customer> userManager,
            IBankingDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<Result> Handle(VerifyPinResetCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result.Failure("User not found");

            // check if the code is in the db
            var record = await _dbContext.EmailConfirmations
                .Where(x => x.UserId == user.Id && x.Purpose == "TransactionPinReset" && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (record == null)
                Result.Failure("Reset Code not found. Please request a new one");

            //check if the code has expired
            if (record.ExpiresAt < DateTime.UtcNow)
                return Result.Failure("Reset code has expired");
            
            // Check if the code matches
            if (record.CodeHash != request.Code)
                return Result.Failure("Invalid reset code");

            // Mark code as used
            record.IsUsed = true;
            _dbContext.EmailConfirmations.Update(record);

            // Update the users Transaction PIN
            user.TransactionPin = BCrypt.Net.BCrypt.HashPassword(request.NewTransactionPin);
            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success("Transaction PIN successfully reset.");
        }
    }
}
