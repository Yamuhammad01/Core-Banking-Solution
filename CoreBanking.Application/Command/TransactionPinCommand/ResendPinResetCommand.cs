using CoreBanking.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Command.TransactionPinCommand
{
    public class ResendPinResetCommand : IRequest<Result>
    {
        public string Email { get; set; } = default!;
    }
}
