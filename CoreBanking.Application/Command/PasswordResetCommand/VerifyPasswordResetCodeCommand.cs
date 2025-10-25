using CoreBanking.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Command.PasswordResetCommand
{
    public class VerifyPasswordResetCodeCommand : IRequest<Result>
    {
        public string Email { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
