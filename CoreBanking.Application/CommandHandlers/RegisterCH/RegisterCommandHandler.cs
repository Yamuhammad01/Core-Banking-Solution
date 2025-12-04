using CoreBanking.Application.Command.EmailConfirmationCommand;
using CoreBanking.Application.Command.RegisterCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IMailServices;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs.Events;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.CommandHandlers.RegisterCH
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IMediator _mediator;
        private readonly IBankingDbContext _dbContext;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IEmailSenderr _emailSender;
        private readonly IUnitOfWork _uow;
        private readonly IPublishEndpoint _publishEndpoint;

        public RegisterCommandHandler(UserManager<Customer> userManager,
            IMediator mediator,
            IBankingDbContext bankingDbContext,
            IEmailTemplateService emailTemplateService,
            IEmailSenderr emailSender,
            IUnitOfWork unitOfWork,
            IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _mediator = mediator;
            _dbContext = bankingDbContext;
            _emailTemplateService = emailTemplateService;
            _emailSender = emailSender;
            _uow = unitOfWork;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            //check if password doesnt match
            if (request.Password != request.ConfirmPassword)
                return Result.Failure("Passwords do not match.");

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return Result.Failure("User with this email already exists");


            await _uow.BeginTransactionAsync();
            try
            {
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
                // await _dbContext.SaveChangesAsync(cancellationToken);

                // Publish UserCreated event
                await _publishEndpoint.Publish(new UserCreated
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AccountNumber = bankAccount.AccountNumber,
                    Currency = bankAccount.Currency
                }, context =>
                {
                    context.MessageId = Guid.NewGuid();
                });

                // Send confirmation code (only if email was sent successfully)
                 await _mediator.Send(new SendEmailCodeCommand { Email = user.Email }, cancellationToken);

                //commit everything 
                await _uow.CommitAsync();

                return Result.Success("Registration successful! Please check your email (spam) for the confirmation code.");
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                return Result.Failure($"Registration failed, Check your internet connection please");
            }
          
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
