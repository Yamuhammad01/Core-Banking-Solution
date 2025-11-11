using CoreBanking.Application.Common;
using MediatR;

namespace CoreBanking.Application.Command.PasswordResetCommand
{
    public class VerifyPasswordResetCodeCommand : IRequest<Result>
    {
        public string Email { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
