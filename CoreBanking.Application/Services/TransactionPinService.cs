﻿using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Responses;
using CoreBanking.Application.Security;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs.TransactionDto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace CoreBanking.Application.Services
{
    public class TransactionPinService : ITransactionPinService
    {
        private readonly UserManager<Customer> _userManager;
        private readonly IPasswordHasher<Customer> _pinHasher;
        private readonly ICodeHasher _codeHasher;
        private readonly IPinValidationService _pinValidator;
        private readonly IBankingDbContext _dbContext;
        private CancellationToken cancellationToken;

        public TransactionPinService(UserManager<Customer> userManager,
            IPasswordHasher<Customer> pinHasher,
            ICodeHasher codeHasher,
            IPinValidationService pinValidationService,
            IBankingDbContext dbContext)
        {
            _userManager = userManager;
            _pinHasher = pinHasher;
            _codeHasher = codeHasher;
            _pinValidator = pinValidationService;
            _dbContext = dbContext;
        }

        public async Task<Responses.ApiResponses> SetTransactionPinAsync(string userId, SetPinRequestDto request)
        {
            if (request.Pin != request.ConfirmPin)
                return new Responses.ApiResponses(false, "PINs do not match.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new Responses.ApiResponses(false, "User not found");

            var result = _pinValidator.ValidateAndHashNewPin(request.Pin);
            if (!result.Success)
                return result;

            if (result.Data == null)
                return new ApiResponses(false, "Unexpected error: no data returned from pin validator.");

            dynamic data = result.Data;
            user.TransactionPin = data.hashedPin;
            user.PinSalt = data.Salt;

            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Responses.ApiResponses(true, "Transaction PIN set successfully");
        }
    }
}
