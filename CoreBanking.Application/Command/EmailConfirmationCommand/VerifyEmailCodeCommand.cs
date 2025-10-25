using CoreBanking.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Command.EmailConfirmationCommand
{
    public class VerifyEmailCodeCommand : IRequest<Result>
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}
