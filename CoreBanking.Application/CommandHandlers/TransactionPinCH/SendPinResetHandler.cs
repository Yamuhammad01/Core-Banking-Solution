using CoreBanking.Application.Command.TransactionPinCommand;
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

namespace CoreBanking.Application.CommandHandlers.TransactionPinCH
{
    public class SendPinResetHandler : IRequestHandler<SendPinResetCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IBankingDbContext _dbContext;
        private readonly IEmailSenderr _emailSender;
        private readonly ICodeHasher _codeHasher;

        public SendPinResetHandler(
            UserManager<Customer> userManager,
            IBankingDbContext dbContext,
            IEmailSenderr emailSender,
            ICodeHasher codeHasher)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _emailSender = emailSender;
            _codeHasher = codeHasher;
        }

        public async Task<Result> Handle(SendPinResetCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result.Failure("User not found");

            // Generate 6-digit code
            var code = _codeHasher.Generate6DigitCode();

            // Remove any old unused code for this purpose
            var oldCodes = await _dbContext.EmailConfirmations
                .Where(x => x.UserId == user.Id && x.Purpose == "TransactionPinReset" && !x.IsUsed)
                .ToListAsync(cancellationToken);
            _dbContext.EmailConfirmations.RemoveRange(oldCodes);

            // Generate salt and hash
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);
            var codeHash = _codeHasher.HashCode(code, salt);

            // Create new record
            var record = new EmailConfirmation
            {
                UserId = user.Id,
                Email = user.Email!,
                CodeHash = codeHash,
                Salt = salt,
                Purpose = "TransactionPinReset",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            _dbContext.EmailConfirmations.Add(record);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Send email
            var message = new Message(
                new[] { user.Email },
                "Transaction PIN Reset Code",
                $"Your transaction PIN reset code is: {code}"
            );

            await _emailSender.SendEmailAsync(message);
            return Result.Success("A 6-digit reset code has been sent to your email");

        }
    }
}
