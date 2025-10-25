using CoreBanking.Application.Common;
using MediatR;
using Octokit.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Command.EmailConfirmationCommand
{
    public class SendEmailCodeCommand : IRequest<Result>
    {
        public string Email { get; set; }
    }
}
