using CoreBanking.Application.Common;
using MediatR;

namespace CoreBanking.Application.Command.EmailConfirmationCommand
{
    public class ResendEmailCodeCommand : IRequest<Result>
    {
        public string Email { get; set; }
    }
}
