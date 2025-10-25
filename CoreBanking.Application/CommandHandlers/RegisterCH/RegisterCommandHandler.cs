using CoreBanking.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Domain.Entities;
using Microsoft.Win32;
using Microsoft.EntityFrameworkCore;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Interfaces.IMailServices;
using CoreBanking.Application.Command.RegisterCommand;
using CoreBanking.Application.Command.EmailConfirmationCommand;

namespace CoreBanking.Application.CommandHandlers.RegisterCH
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IMediator _mediator;
        private readonly IBankingDbContext _dbContext;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IEmailSenderr _emailSender;

        public RegisterCommandHandler(UserManager<Customer> userManager,
            IMediator mediator,
            IBankingDbContext bankingDbContext,
            IEmailTemplateService emailTemplateService,
            IEmailSenderr emailSender)
        {
            _userManager = userManager;
            _mediator = mediator;
            _dbContext = bankingDbContext;
            _emailTemplateService = emailTemplateService;
            _emailSender = emailSender;
        }

        public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            //check if password doesnt match
            if (request.Password != request.ConfirmPassword)
                return Result.Failure("Passwords do not match.");

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return Result.Failure("User with this email already exists");

            var user = new Customer
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                return Result.Failure(string.Join(", ", createResult.Errors.Select(e => e.Description)));
            // Create default BankAccount after user registration
            var bankAccount = new BankAccount
            {
                CustomerId = user.Id,
                AccountNumber = await GenerateUniqueAccountNumberAsync(),
                Balance = 0m, // default balance
                AccountType = "Savings",
                Currency = "NGN",
                Status = "Active"
            };

            _dbContext.BankAccounts.Add(bankAccount);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var emailBody = await _emailTemplateService.GetWelcomeTemplateAsync(
             user.FirstName, user.LastName, bankAccount.AccountNumber, bankAccount.Currency); // get the placeholder to use in the template

            var message = new Message(
                new string[] { request.Email },
                "Welcome to CoreBanking",
                emailBody
            );


            await _emailSender.SendEmailAsync(message);

            await _mediator.Send(new SendEmailCodeCommand { Email = user.Email }, cancellationToken);

            return Result.Success("Registration successful! Please check your email for the confirmation code.");
        }

        private async Task<string> GenerateUniqueAccountNumberAsync()
        {
            const string bankCode = "811";
            string accountNumber;
            bool exists;

            do
            {
                // Generate 7 random digits
                var random = new Random();
                var randomDigits = random.Next(0, 9999999).ToString("D7"); // pad with zeros
                accountNumber = bankCode + randomDigits;

                // Ensure it's unique b4 creating new account
                exists = await _dbContext.BankAccounts
                    .AnyAsync(b => b.AccountNumber == accountNumber);

            } while (exists);

            return accountNumber;
        }
    }
}
