using CoreBanking.Application.Common;
using MediatR;

namespace CoreBanking.Application.Command.EmailConfirmationCommand
{
    public class SendEmailCodeCommand : IRequest<Result>
    {
        public string Email { get; set; }
    }
}
