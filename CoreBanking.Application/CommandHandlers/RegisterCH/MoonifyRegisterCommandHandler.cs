using CoreBanking.Application.Command.EmailConfirmationCommand;
using CoreBanking.Application.Command.RegisterCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IMailServices;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Shared;
using CoreBanking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.CommandHandlers.RegisterCH
{
    public class MoonifyRegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IUnitOfWork _uow;
        private readonly IMonnifyService _monnifyService;
        private readonly IBankingDbContext _dbContext;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IEmailSenderr _emailSender;
        private readonly IMediator _mediator;

        public MoonifyRegisterCommandHandler(
            UserManager<Customer> userManager,
            IUnitOfWork uow,
            IMonnifyService monnifyService,
            IBankingDbContext dbContext,
            IEmailTemplateService emailTemplateService,
            IEmailSenderr emailSender,
            IMediator mediator)
        {
            _userManager = userManager;
            _uow = uow;
            _monnifyService = monnifyService;
            _dbContext = dbContext;
            _emailTemplateService = emailTemplateService;
            _emailSender = emailSender;
            _mediator = mediator;
        }

        public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (request.Password != request.ConfirmPassword)
                return Result.Failure("Passwords do not match.");

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return Result.Failure("User with this email already exists");

            await _uow.BeginTransactionAsync();
            Customer user = null;

            try
            {
                // 1. Create user in Identity
                user = new Customer
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber
                };

                var identityResult = await _userManager.CreateAsync(user, request.Password);
                if (!identityResult.Succeeded)
                    return Result.Failure(string.Join(", ", identityResult.Errors.Select(e => e.Description)));

                // 2. Call Monnify immediately and receive account details immediately
                var monnifyResponse = await _monnifyService.CreateDedicatedVirtualAccountAsync(new MonnifyAccountRequest
                {
                   CustomerEmail = user.Email,
                    CustomerName = $"{user.FirstName} {user.LastName}",
                    CustomerId = user.Id.ToString(),
                    
                });

                if (!monnifyResponse.Success)
                    throw new Exception("Failed to create Monnify virtual account.");

                // 3. Store the Monnify-generated bank account
                var bankAccount = new BankAccount
                {
                    CustomerId = user.Id,
                    AccountNumber = monnifyResponse.AccountNumber,
                    BankName = monnifyResponse.BankName,
                    Balance = 0m,
                    AccountType = "Savings",
                    Currency = "NGN",
                    Status = "Active"
                };

                _dbContext.BankAccounts.Add(bankAccount);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // 4. Send welcome email
                var emailBody = await _emailTemplateService.GetWelcomeTemplateAsync(
                    user.FirstName,
                    user.LastName,
                    bankAccount.AccountNumber,
                    bankAccount.Currency);

                await _emailSender.SendEmailAsync(
                    new Message(new[] { user.Email }, "Welcome to CoreBanking", emailBody)
                );

                // 5. Send confirmation code
                await _mediator.Send(new SendEmailCodeCommand { Email = user.Email }, cancellationToken);

                // 6. Commit
                await _uow.CommitAsync();

                return Result.Success("Registration successful! Your virtual account has been created.");
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();

                if (user != null)
                    await _userManager.DeleteAsync(user);

                return Result.Failure($"Registration failed: {ex.Message}");
            }
        }
    }


}
