using CoreBanking.Domain.Enums;
using CoreBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.DTOs.TransactionDto;
using Microsoft.AspNetCore.Identity;
using Refit;
using CoreBanking.Application.Common;
using CoreBanking.Application.Responses;
using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Application.Interfaces.IServices;
using Microsoft.Win32;
using Octokit;
using System.Security.Principal;
using CoreBanking.Application.Security;

namespace CoreBanking.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly ITransactionRepository _txRepo;
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher<Customer> _pinHasher;
        private readonly UserManager<Customer> _userManager;
        private readonly ITransactionPinService _pinService;
        private readonly ITransactionEmailService _transactionEmailService;
        private readonly IPinValidationService _pinValidator;
        private readonly ICodeHasher _codeHasher;

        public TransactionService(IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IUnitOfWork uow, IPasswordHasher<Customer> pinHasher,
            UserManager<Customer> userManager,
            ITransactionPinService pinService, 
            ITransactionEmailService transactionEmailService,
            IPinValidationService pinValidationService,
            ICodeHasher codeHasher)
        {
            _accountRepo = accountRepository;
            _txRepo = transactionRepository;
            _uow = uow;
            _pinHasher = pinHasher;
            _userManager = userManager;
            _pinService = pinService;
            _transactionEmailService = transactionEmailService;
            _pinValidator = pinValidationService;
            _codeHasher = codeHasher;
        }
        public async Task<Result> TransferFundsAsync(string userId, TransferRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure("User not found");

            var verify = _pinValidator.VerifyExistingPin(request.TransactionPin, user.TransactionPin, user.PinSalt);
            if (!verify.Succeeded)
                return verify;

            var source = await _accountRepo.GetByUserIdAsync(userId);
            var destination = await _accountRepo.GetByAccountNumberAsync(request.AccountNumber);
            //var customer = await _accountRepo.GetUserIdAsync(userId);

            if (source == null || destination == null)
                return Result.Failure("Invalid account number");

            //check if sender account is frozen 
            if (source.Customers.IsFrozen)
                return Result.Failure("Transaction failed. Your account is frozen.");
            //check if the receiver account is frozen 
            if (destination.Customers.IsFrozen)
                return Result.Failure("Can't send funds to this user.The user account is frozen");

            if (source == destination)
                return Result.Failure("Cannot do self transfer");

            if (source.Balance < request.Amount)
                return Result.Failure("Insufficient balance");

            source.Balance -= request.Amount;
            destination.Balance += request.Amount;

            await _accountRepo.UpdateAsync(source);
            await _accountRepo.UpdateAsync(destination);

            var reference = Guid.NewGuid().ToString("N");

            var newcustomer = await _accountRepo.GetUserIdAsync(userId);

            await _txRepo.AddAsync(new Transactions
            {
                BankAccountId = source.Id,
                UserId = source.CustomerId,
                Amount = request.Amount,
                Type = TransactionType.Debit,
                Description = request.Narration,
                Reference = reference
            });

            await _txRepo.AddAsync(new Transactions
            {
                BankAccountId = destination.Id,
                UserId = destination.CustomerId,
                Amount = request.Amount,
                Type = TransactionType.Credit,
                Description = request.Narration,
                Reference = reference
            });

            var sender = await _accountRepo.GetUserByAccountIdAsync(source.Id);
            var receiver = await _accountRepo.GetUserByAccountIdAsync(destination.Id);

            // Send a Debit Email Alert to the sender  
            await _transactionEmailService.SendTransactionEmailAsync(
                email: sender.Email,
                firstName: sender.FirstName,
                lastName: sender.LastName,
                transactionType: "Debit",
                amount: request.Amount,
                accountNumber: source.AccountNumber,
                reference: reference,
                balance: source.Balance,
                date: DateTime.UtcNow,
                senderFullName: null
            );

            // Send a Credit Email Alert to the receiver with the sender's full name
            await _transactionEmailService.SendTransactionEmailAsync(
                email: receiver.Email,
                firstName: receiver.FirstName,
                lastName: receiver.LastName,
                transactionType: "Credit",
                amount: request.Amount,
                accountNumber: destination.AccountNumber,
                reference: reference,
                balance: destination.Balance,
                date: DateTime.UtcNow,
                senderFullName: $"{sender.FirstName} {sender.LastName}"
            );

            return Result.Success("Transfer successful.", new
            {
                Reference = reference
            });
        }

        public async Task<Responses.ApiResponses> AdminDepositAsync(string adminId, DepositRequestDto request)
        {
            if (request.Amount <= 0)
                return new() { Success = false, Message = "Amount must be greater than zero." };

            var account = await _accountRepo.GetByAccountNumberAsync(request.AccountNumber);
            if (account == null)
                return new() { Success = false, Message = "Invalid Account Number." };

            var reference = Guid.NewGuid().ToString("N");

            try
            {
                await _uow.BeginTransactionAsync();

                account.Balance += request.Amount;
                await _accountRepo.UpdateAsync(account);

                var tx = new Transactions
                {
                    Id = Guid.NewGuid(),
                    BankAccountId = account.Id,
                    Amount = request.Amount,
                    UserId = account.CustomerId,
                    Type = TransactionType.Deposit,
                    Reference = reference,
                    Description = request.Narration ?? "Admin deposit",
 
                    CreatedAt = DateTime.UtcNow
                };


                var receiver = await _accountRepo.GetUserByAccountIdAsync(account.Id);
                //  Send a Credit Email Alert to the receiver
                await _transactionEmailService.SendTransactionEmailAsync(
                     email: receiver.Email,
                     firstName: receiver.FirstName,
                     lastName: receiver.LastName,
                     transactionType: "Credit",
                     amount: request.Amount,
                     accountNumber: account.AccountNumber,
                     reference: reference,
                     balance: account.Balance,
                     date: DateTime.UtcNow
                );

                await _txRepo.AddAsync(tx);
                await _uow.CommitAsync();

                return new Responses.ApiResponses
                {
                    Success = true,
                    Message = "Deposit successful.",
                    Reference = reference,
                    NewBalance = account.Balance
                };
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return new Responses.ApiResponses
                {
                    Success = false,
                    Message = $"Deposit failed: {ex.Message}"
                };
            }
        }

        public async Task<Responses.ApiResponses> DepositAsync(string userId, DepositRequestDto request)
        {
            if (request.Amount <= 0) 
                return new() { Success = false, Message = "Amount must be > 0" };

            var account = await _accountRepo.GetByAccountNumberAndUserIdAsync(userId, request.AccountNumber);
          
            if (account == null) 
                return new() { Success = false, Message = "Account not found." };

            var reference = Guid.NewGuid().ToString("N");
            
            try
            {
                await _uow.BeginTransactionAsync();

                account.Balance += request.Amount;
                await _accountRepo.UpdateAsync(account);

                var tx = new Transactions
                {
                    Id = Guid.NewGuid(),
                    BankAccountId = account.Id,
                    Amount = request.Amount,
                    UserId = account.CustomerId,
                    Type = TransactionType.Deposit,
                    Reference = reference,
                    Description = request.Narration,
                    //PerformedByAdmin = adminId.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                await _txRepo.AddAsync(tx);
                await _uow.CommitAsync();

                return new Responses.ApiResponses { Success = true, Message = "Deposit successful.", Reference = reference, NewBalance = account.Balance };
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return new Responses.ApiResponses { Success = false, Message = $"Deposit failed: {ex.Message}" };
            }
        }
        // withdrawal service
        public async Task<Result> WithdrawAsync(string userId, WithdrawalRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Failure("User not found");

            if (user.TransactionPin == null)
                return Result.Failure("User transaction pin not found");

            var verify = _pinValidator.VerifyExistingPin(request.TransactionPin, user.TransactionPin, user.PinSalt);
            if (!verify.Succeeded)
                return verify;

            //check if the account is frozen 
            if (user.IsFrozen)
                return Result.Failure("Transaction failed. Your account is frozen.");

            if (string.IsNullOrEmpty(user.TransactionPin) || string.IsNullOrEmpty(user.PinSalt))
                return Result.Failure("User Pin not found");

            if (request.Amount <= 0)
                return Result.Failure("Amount must be greater than 0");

            var account = await _accountRepo.GetByUserIdAsync(userId);
            var customer = await _accountRepo.GetUserIdAsync(userId);
            if (account == null)
                return Result.Failure("Source account not found.");

            if (account.Balance < request.Amount)
                return Result.Failure("Insufficient funds.");

            var reference = Guid.NewGuid().ToString("N");

            try
            {
                await _uow.BeginTransactionAsync();

                account.Balance -= request.Amount;
                await _accountRepo.UpdateAsync(account);

                var tx = new Transactions
                {
                    Id = Guid.NewGuid(),
                    BankAccountId = account.Id,
                    Amount = request.Amount,
                    Type = TransactionType.Withdraw,
                    Reference = reference,
                    Description = request.Narration,
                    UserId = account.CustomerId,
                    CreatedAt = DateTime.UtcNow
                };

                await _txRepo.AddAsync(tx);
                await _uow.CommitAsync();

                // Send Email Alert 
                await _transactionEmailService.SendTransactionEmailAsync(
                    email: customer.Email,
                    firstName: customer.FirstName,
                    lastName: customer.LastName,
                    transactionType: "Withdrawal",
                    amount: request.Amount,
                    accountNumber: account.AccountNumber,
                    reference: reference,
                    balance: account.Balance,
                    date: DateTime.UtcNow
                );

                return new Result(true, "Withdrawal successful", new
                {
                    Reference = reference,
                    NewBalance = account.Balance
                });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Result.Failure($"Withdrawal failed {ex.Message}");
            }
        }

    }
}
