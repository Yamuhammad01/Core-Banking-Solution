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
namespace CoreBanking.Application.CommandHandlers
{

    public class SendEmailConfirmationHandler : IRequestHandler<SendEmailConfirmationCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IBankingDbContext _db;
        private readonly IEmailSenderr _emailService;
        private readonly ILogger<SendEmailConfirmationHandler> _logger;

        public SendEmailConfirmationHandler(
            UserManager<Customer> userManager,
            IBankingDbContext db,
            IEmailSenderr emailService,
            ILogger<SendEmailConfirmationHandler> logger)
        {
            _userManager = userManager;
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Result> Handle(SendEmailConfirmationCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result.Failure("User not Found");
                //throw new Exception("User not found.");

            // Remove old unused codes for the same purpose (optional)
            var old = _db.EmailConfirmations
                .Where(x => x.UserId == user.Id && x.Purpose == "EmailConfirmation" && !x.IsUsed);
            _db.EmailConfirmations.RemoveRange(old);

            // Generate secure 6-digit code
            var code = Generate6DigitCode();

            // Generate salt and hash
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);
            var codeHash = HashCode(code, salt);

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
             .Where(e => e.Email == request.Email && !e.IsUsed)
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

        private static string Generate6DigitCode()
        {
            //Generate a secure random number between 0 and 999999
            var val = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return val.ToString("D6"); // zero-padded to 6 digits
        }

        private static string HashCode(string code, string salt)
        {
            // Used HMACSHA256 with salt for hashing
            var saltBytes = Convert.FromBase64String(salt);
            using var hmac = new HMACSHA256(saltBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(hash);
        }

       
    }
}
