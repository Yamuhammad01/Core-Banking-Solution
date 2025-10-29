using CoreBanking.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
