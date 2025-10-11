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
    public class TransferService : ITransferService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly ITransactionRepository _txRepo;
        public TransferService(IAccountRepository accountRepository, ITransactionRepository transactionRepository) 
        { 
            _accountRepo = accountRepository;
            _txRepo = transactionRepository;
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
    }
}
