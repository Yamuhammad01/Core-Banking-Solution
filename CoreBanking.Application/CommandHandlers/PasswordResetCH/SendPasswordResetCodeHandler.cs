using CoreBanking.Application.Command.PasswordResetCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Security;
using CoreBanking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.CommandHandlers.PasswordResetCH
{
    public class SendPasswordResetCodeHandler : IRequestHandler<SendPasswordResetCodeCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IBankingDbContext _dbContext;
        private readonly IEmailSenderr _emailSender;
        private readonly ICodeHasher _codeHasher;
        private readonly ILogger<SendPasswordResetCodeHandler> _logger;

        public SendPasswordResetCodeHandler(UserManager<Customer> userManager,
            IBankingDbContext dbContext,
            IEmailSenderr emailSender,
            ICodeHasher codeHasher,
            ILogger<SendPasswordResetCodeHandler> logger)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _emailSender = emailSender;
            _codeHasher = codeHasher;
            _logger = logger;
        }

        public async Task<Result> Handle(SendPasswordResetCodeCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result.Failure("User not Found");

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

            var html = $@"
            <p>Hi {user.UserName},</p>
            <p>Your password reset code is <strong>{code}</strong>.</p>
            <p>This code expires in 10 minutes. If you did not request this, ignore this email.</p>";

            var message = new Message(
               new string[] { user.Email! },          // recipients
                 "Password Reset Code",                  // subject
               html                                   // body/content
            );
            await _emailSender.SendEmailAsync(message);

            _logger.LogInformation("Password reset code generated for user {UserId}", user.Id);
            return Result.Success("Password reset code sent successfully");
        }
    }
}
