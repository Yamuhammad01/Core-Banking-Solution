using MediatR;
using CoreBanking.Application.Common;

namespace CoreBanking.Application.Command.TransactionPinCommand
{
    public class VerifyPinResetCommand : IRequest<Result>
    {
        public string Email { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string NewTransactionPin { get; set; } = default!;
    }
}
