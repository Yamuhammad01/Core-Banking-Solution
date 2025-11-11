using CoreBanking.Application.Common;
using MediatR;

namespace CoreBanking.Application.Command.PasswordResetCommand
{
    public class ResendPasswordResetCodeCommand : IRequest<Result>
    {
        public string Email { get; set; } = default!;
    }
}
