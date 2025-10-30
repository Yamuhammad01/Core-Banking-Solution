using CoreBanking.Application.Command.PasswordResetCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Security;
using CoreBanking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.CommandHandlers.PasswordResetCH
{
    public class VerifyPasswordResetCodeHandler : IRequestHandler<VerifyPasswordResetCodeCommand, Result>
    {
        private readonly IBankingDbContext _dbContext;
        private readonly UserManager<Customer> _userManager;
        private readonly ICodeHasher _codeHasher;

        public VerifyPasswordResetCodeHandler(IBankingDbContext dbContext,
            UserManager<Customer> userManager,
            ICodeHasher codeHasher)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _codeHasher = codeHasher;
        }

        public async Task<Result> Handle(VerifyPasswordResetCodeCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result.Failure("User not Found");

            var record = await _dbContext.EmailConfirmations
                .Where(x => x.Email == request.Email && x.Purpose == "PasswordReset" && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (record == null)
                return Result.Failure("No reset code found. Request a new one");

            // check if the code has expired   
            if (record.ExpiresAt < DateTime.UtcNow)
                return Result.Failure("Confirmation code expired. Please Request a new one");

            // Hash input using stored salt and compare
            var computedHash = _codeHasher.HashCode(request.Code, record.Salt);
            if (!_codeHasher.CryptographicEquals(computedHash, record.CodeHash))
                return Result.Failure("Invalid confirmation code.");

            // mark used
            record.IsUsed = true;
            await _dbContext.SaveChangesAsync(cancellationToken);

            // reset password
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!resetResult.Succeeded)
                return Result.Failure("Password reset failed");

            return Result.Success("Password reset successfully.");
        }
    }
}
