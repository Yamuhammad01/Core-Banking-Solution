using CoreBanking.Application.Common;
using MediatR;

namespace CoreBanking.Application.Command.PasswordResetCommand
{
    public class ChangePasswordCommand : IRequest<Result>
    {
        public string Email { get; set; } = default!;
        public string OldPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}
