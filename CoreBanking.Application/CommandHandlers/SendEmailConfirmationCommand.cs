using MediatR;
using Octokit.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.CommandHandlers
{
    public class SendEmailConfirmationCommand : IRequest<Unit> 
    {
        public string Email { get; set; }
    }
}
