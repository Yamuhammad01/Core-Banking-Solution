using CoreBanking.Application.Command.TransactionPinCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Security;
using CoreBanking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.CommandHandlers.TransactionPinCH
{
    public class ChangePinHandler : IRequestHandler<ChangePinCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;
        private readonly ICodeHasher _codeHasher;
       // private readonly IEmailSenderr _emailSenderr;
        public ChangePinHandler(UserManager<Customer> userManager, ICodeHasher codeHasher)
        {
            _userManager = userManager;
            _codeHasher = codeHasher;
        }

        public async Task<Result> Handle(ChangePinCommand request, CancellationToken cancellationToken)
        {
            //check if pin doesnt match
            if (request.NewPin != request.ConfirmPin)
                return Result.Failure("Pins do not match");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result.Failure("User not found");

            // Verify old pin
            var computedHash = _codeHasher.HashCode(request.OldPin, user.PinSalt);   
            if (!_codeHasher.CryptographicEquals(computedHash, user.TransactionPin))
                return Result.Failure("Invalid old PIN");

            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);

            //hash a new pin and generate salt
           
            var newHash = _codeHasher.HashCode(request.NewPin, salt);

            user.TransactionPin = newHash;
            user.PinSalt = salt;

            var result = await _userManager.UpdateAsync(user);  
            if (!result.Succeeded)
                return Result.Failure("Failed to change Pin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
           // await _emailService.SendSecurityAlertAsync(user.Email, "Your transaction PIN was changed.");
            return Result.Success("Pin changed successfully");
        }
    }
}