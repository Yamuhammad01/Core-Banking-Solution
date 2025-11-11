using CoreBanking.Application.Common;
using MediatR;

namespace CoreBanking.Application.Command.TransactionPinCommand
{
    public class ChangePinCommand : IRequest<Result>
    {
        public string Email { get; set; } = default!;
        public string OldPin { get; set; } = default!;
        public string NewPin { get; set; } = default!;
        public string ConfirmPin { get; set; } = default!;
    }
}
