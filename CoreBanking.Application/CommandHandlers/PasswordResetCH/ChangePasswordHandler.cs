using CoreBanking.Application.Command.PasswordResetCommand;
using CoreBanking.Application.Common;
using CoreBanking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.CommandHandlers.PasswordResetCH
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly UserManager<Customer> _userManager;

        public ChangePasswordHandler(UserManager<Customer> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            //check if passwords doesnt match
            if (request.NewPassword != request.ConfirmPassword)
                return Result.Failure("Passwords do not match");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result.Failure("User not found");

            var checkOldPassword = await _userManager.CheckPasswordAsync(user, request.OldPassword);
            if (!checkOldPassword)
                return Result.Failure("Old password is not correct");

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
                return Result.Failure("Failed to change password: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            return Result.Failure("Passwords changed successfully");
        }
    }
}
