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
using CoreBanking.Application.Common;
using CoreBanking.Application.Responses;
using Octokit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using CoreBanking.Application.Security;
using CoreBanking.Application.Command.EmailConfirmationCommand;
namespace CoreBanking.Application.CommandHandlers.EmailVerificationCH
{

    public class SendEmailCodeHandler : IRequestHandler<SendEmailCodeCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IBankingDbContext _db;
        private readonly IEmailSenderr _emailService;
        private readonly ILogger<SendEmailCodeHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICodeHasher _codeHasher;

        public SendEmailCodeHandler(
            UserManager<Customer> userManager,
            IBankingDbContext db,
            IEmailSenderr emailService,
            ILogger<SendEmailCodeHandler> logger,
            IHttpContextAccessor httpContextAccessor,
            ICodeHasher codeHasher)
        {
            _userManager = userManager;
            _db = db;
            _emailService = emailService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _codeHasher = codeHasher;
        }

        public async Task<Result> Handle(SendEmailCodeCommand request, CancellationToken cancellationToken)
        {
            //find a user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result.Failure("User not found.");

            if (user.EmailConfirmed)
                return Result.Failure("Email already confirmed");

            // Remove old unused codes for the same purpose (optional)
            var old = _db.EmailConfirmations
                .Where(x => x.UserId == user.Id && x.Purpose == "EmailConfirmation" && !x.IsUsed);
            _db.EmailConfirmations.RemoveRange(old);

            // Generate secure 6-digit code
            var code = _codeHasher.Generate6DigitCode();

            // Generate salt and hash
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);
            var codeHash = _codeHasher.HashCode(code, salt);

            var record = new EmailConfirmation
            {
                UserId = user.Id,
                Email = user.Email!,
                CodeHash = codeHash,
                Salt = salt,
                Purpose = "EmailConfirmation",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };
            var existing = await _db.EmailConfirmations
             .Where(e => e.Email == user.Email && !e.IsUsed)
            .ToListAsync();
            _db.EmailConfirmations.RemoveRange(existing);

            _db.EmailConfirmations.Add(record);
            await _db.SaveChangesAsync(cancellationToken);

            // Build email content (use template service in real app)
            var html = $@"
            <p>Hi {user.UserName},</p>
            <p>Your email confirmation code is <strong>{code}</strong>.</p>
            <p>This code expires in 10 minutes. If you did not request this, ignore this email.</p>";

            var message = new Message(
               new string[] { user.Email! },          // recipients
                 "Confirmation Email",                  // subject
               html                                   // body/content
            );

            await _emailService.SendEmailAsync(message);

            _logger.LogInformation("Email confirmation code generated for user {UserId}", user.Id);
            return Result.Success("Email Confirmation sent successfully");
        }
    }
}
