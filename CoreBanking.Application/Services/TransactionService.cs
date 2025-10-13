using CoreBanking.Application.Interfaces;
using CoreBanking.Domain.Enums;
using CoreBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.DTOs.TransactionDto;

namespace CoreBanking.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly ITransactionRepository _txRepo;
        private readonly IUnitOfWork _uow;
        public TransactionService(IAccountRepository accountRepository, ITransactionRepository transactionRepository, IUnitOfWork uow)
        {
            _accountRepo = accountRepository;
            _txRepo = transactionRepository;
            _uow = uow;
        }

        public async Task<TransferResponseDto> TransferFundsAsync(string userId, TransferRequestDto request)
        {
            var source = await _accountRepo.GetByUserIdAsync(userId);
            var destination = await _accountRepo.GetByAccountNumberAsync(request.AccountNumber);

            if (source == null || destination == null)
                return new() { Success = false, Message = "Invalid account number" };

            if (source ==  destination)
                return new() { Success = false, Message = "Cannot do self transfer" };

            if (source.Balance < request.Amount)
                return new() { Success = false, Message = "Insufficient balance" };

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

            return new()
            {
                Success = true,
                TransactionReference = reference,
                Message = "Transfer successful."
            };
        }

        public async Task<TransactionResponseDto> AdminDepositAsync(string adminId, DepositRequestDto request)
        {
            if (request.Amount <= 0)
                return new() { Success = false, Message = "Amount must be greater than zero." };

            var account = await _accountRepo.GetByAccountNumberAsync(request.AccountNumber);
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
                    Description = request.Description ?? "Admin deposit",
                   // PerformedByAdmin = adminId,
                    CreatedAt = DateTime.UtcNow
                };

                await _txRepo.AddAsync(tx);
                await _uow.CommitAsync();

                return new TransactionResponseDto
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
                return new TransactionResponseDto
                {
                    Success = false,
                    Message = $"Deposit failed: {ex.Message}"
                };
            }
        }




        public async Task<TransactionResponseDto> DepositAsync(string userId, DepositRequestDto request)
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
                    Description = request.Description,
                    //PerformedByAdmin = adminId.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                await _txRepo.AddAsync(tx);
                await _uow.CommitAsync();

                return new TransactionResponseDto { Success = true, Message = "Deposit successful.", Reference = reference, NewBalance = account.Balance };
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return new TransactionResponseDto { Success = false, Message = $"Deposit failed: {ex.Message}" };
            }
        }

        public async Task<TransactionResponseDto> WithdrawAsync(string userId, WithdrawalRequestDto request)
        {
            if (request.Amount <= 0) 
                return new() { Success = false, Message = "Amount must be > 0" };

            var account = await _accountRepo.GetByUserIdAsync(userId);
            if (account == null) 
                return new() { Success = false, Message = "Source account not found." };

            if (account.Balance < request.Amount) 
                return new() { Success = false, Message = "Insufficient funds." };

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
                    Description = request.Description,
                    UserId = account.CustomerId,
                   // PerformedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _txRepo.AddAsync(tx);
                await _uow.CommitAsync();

                return new TransactionResponseDto { Success = true, Message = "Withdrawal successful.", Reference = reference, NewBalance = account.Balance };
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return new TransactionResponseDto { Success = false, Message = $"Withdrawal failed: {ex.Message}" };
            }
        }

    }
}
