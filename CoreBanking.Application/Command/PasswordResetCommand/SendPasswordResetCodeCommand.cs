using CoreBanking.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Command.PasswordResetCommand
{

    public class SendPasswordResetCodeCommand : IRequest<Result>
    {
        public string Email { get; set; } = default!;
    }
}
