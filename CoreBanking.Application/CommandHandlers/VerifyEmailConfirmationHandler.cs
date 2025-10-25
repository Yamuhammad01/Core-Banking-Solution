using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
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

namespace CoreBanking.Application.CommandHandlers
{
    public class VerifyEmailConfirmationHandler : IRequestHandler<VerifyEmailConfirmationCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IBankingDbContext _dbContext;
        private readonly ILogger<VerifyEmailConfirmationHandler> _logger;

        public VerifyEmailConfirmationHandler(
            UserManager<Customer> userManager,
            IBankingDbContext db,
            ILogger<VerifyEmailConfirmationHandler> logger)
        {
            _userManager = userManager;
            _dbContext = db;
            _logger = logger;
        }

        public async Task<Result> Handle(VerifyEmailConfirmationCommand request, CancellationToken cancellationToken)
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
               
            // check if the code has expired   
            if (record.ExpiresAt < DateTime.UtcNow)
                return Result.Failure("Confirmation code expired. Please Request a new one");

            // Hash input using stored salt and compare
            var computedHash = HashCode(request.Code, record.Salt);
            if (!CryptographicEquals(computedHash, record.CodeHash))
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

        private static string HashCode(string code, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using var hmac = new HMACSHA256(saltBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(hash);
        }

        // Time-constant comparison to avoid timing attacks
        private static bool CryptographicEquals(string a, string b)
        {
            var aBytes = Convert.FromBase64String(a);
            var bBytes = Convert.FromBase64String(b);
            return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
        }
    }
}
