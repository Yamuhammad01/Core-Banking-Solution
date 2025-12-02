using CoreBanking.Application.Command.EmailConfirmationCommand;
using CoreBanking.Application.Command.RegisterCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IMailServices;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Shared;
using CoreBanking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static CoreBanking.Application.Services.PaystackService;

/* namespace CoreBanking.Application.CommandHandlers.RegisterCH
{
    public class PaystackRegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IUnitOfWork _uow;
       // private readonly IVirtualAccountService _paystackService;
        private readonly IBankingDbContext _dbContext;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IEmailSenderr _emailSender;
        private readonly IMediator _mediator;

        public PaystackRegisterCommandHandler(
            UserManager<Customer> userManager,
            IUnitOfWork uow,
           // IVirtualAccountService paystackService,
            IBankingDbContext dbContext,
            IEmailTemplateService emailTemplateService,
            IEmailSenderr emailSender,
            IMediator mediator)
        {
            _userManager = userManager;
            _uow = uow;
           // _paystackService = paystackService;
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

                //  Call Paystack to create a dedicated virtual account
                var paystackResponse = await _paystackService.CreateDedicatedVirtualAccountAsync(new PaystackAccountRequest
                {
                    CustomerEmail = user.Email,
                    CustomerName = $"{user.FirstName} {user.LastName}",
                    CustomerId = user.Id
                });

                if (!paystackResponse.Success)
                    throw new Exception("Failed to create Paystack virtual account.");

                // 3. Store the Paystack-generated bank account
                var bankAccount = new BankAccount
                {
                    CustomerId = user.Id,
                    AccountNumber = paystackResponse.AccountNumber,
                    BankName = paystackResponse.BankName,
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
*/