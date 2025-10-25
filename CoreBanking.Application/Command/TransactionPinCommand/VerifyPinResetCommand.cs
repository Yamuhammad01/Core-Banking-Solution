using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
