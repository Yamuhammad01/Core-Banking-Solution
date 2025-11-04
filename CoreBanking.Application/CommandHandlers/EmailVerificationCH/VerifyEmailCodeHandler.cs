using CoreBanking.Application.Command.EmailConfirmationCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Security;
using CoreBanking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.CommandHandlers.EmailVerificationCH
{
    public class VerifyEmailCodeHandler : IRequestHandler<VerifyEmailCodeCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IBankingDbContext _dbContext;
        private readonly ILogger<VerifyEmailCodeHandler> _logger;
        private readonly ICodeHasher _codeHasher;

        public VerifyEmailCodeHandler(
            UserManager<Customer> userManager,
            IBankingDbContext db,
            ILogger<VerifyEmailCodeHandler> logger,
            ICodeHasher codeHasher)
        {
            _userManager = userManager;
            _dbContext = db;
            _logger = logger;
            _codeHasher = codeHasher;
        }

        public async Task<Result> Handle(VerifyEmailCodeCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new Exception("User not found.");

            // Get latest unused code for this user and purpose
            var record = await _dbContext.EmailConfirmations
                .Where(x => x.UserId == user.Id && x.Purpose == "EmailConfirmation" && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

           

            // check for confirmation codes from the db
            if (record == null)
                return Result.Failure("Confirmation code not found");

            if (record.IsUsed)
                return Result.Failure("Email already verified");

            // check if the code has expired   
            if (record.ExpiresAt < DateTime.UtcNow)
                return Result.Failure("Confirmation code expired. Please Request a new one");
       
            // Hash input using stored salt and compare
            var computedHash = _codeHasher.HashCode(request.Code, record.Salt);
            if (!_codeHasher.CryptographicEquals(computedHash, record.CodeHash))
                return Result.Failure("Invalid confirmation code.");
            // Mark as used
            record.IsUsed = true;

            // Confirm user email
            user.EmailConfirmed = true;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result.Failure("Failed to update user email status.");

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} confirmed email.", user.Id);
            return Result.Success("Email Verified Successfully");
        }
    }
}
