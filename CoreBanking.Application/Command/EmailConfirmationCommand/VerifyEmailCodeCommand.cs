using CoreBanking.Application.Common;
using MediatR;

namespace CoreBanking.Application.Command.EmailConfirmationCommand
{
    public class VerifyEmailCodeCommand : IRequest<Result>
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
