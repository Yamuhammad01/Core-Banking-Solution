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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace CoreBanking.Application.CommandHandlers.PasswordResetCH
{
    public class ResendPasswordResetCodeHandler : IRequestHandler<ResendPasswordResetCodeCommand, Result>
    {
        private readonly IBankingDbContext _dbContext;
        private readonly UserManager<Customer> _userManager;
        private readonly IEmailSenderr _emailSender;
        private readonly ICodeHasher _codeHasher;

        public ResendPasswordResetCodeHandler(IBankingDbContext dbContext,
            UserManager<Customer> userManager,
            IEmailSenderr emailSender,
            ICodeHasher codeHasher)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _emailSender = emailSender;
            _codeHasher = codeHasher;
        }

        public async Task<Result> Handle(ResendPasswordResetCodeCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result.Failure("User not found");



            // invalidate old codes
            var oldCodes = await _dbContext.EmailConfirmations
                .Where(x => x.Email == request.Email && x.Purpose == "PasswordReset" && !x.IsUsed)
                .ToListAsync(cancellationToken);
            foreach (var codee in oldCodes) codee.IsUsed = true;

            // Generate secure 6-digit code
            var code = _codeHasher.Generate6DigitCode();

            // Generate salt and hash
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);
            var codeHash = _codeHasher.HashCode(code, salt);

            var confirmation = new EmailConfirmation
            {
                UserId = user.Id,
                Email = user.Email!,
                CodeHash = codeHash,
                Salt = salt,
                Purpose = "PasswordReset",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            _dbContext.EmailConfirmations.Add(confirmation);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _emailSender.SendEmailAsync(new Message(
                new[] { user.Email! },
                "New Password Reset Code",
                $"Your new password reset code is: {code}. It expires in 10 minutes."
            ));

            return Result.Success("A new password reset code has been sent to your email");
        }
    }
}
