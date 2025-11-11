using CoreBanking.Application.Common;
using MediatR;

namespace CoreBanking.Application.Command.TransactionPinCommand
{
    public class ResendPinResetCommand : IRequest<Result>
    {
        public string Email { get; set; } = default!;
    }
}
