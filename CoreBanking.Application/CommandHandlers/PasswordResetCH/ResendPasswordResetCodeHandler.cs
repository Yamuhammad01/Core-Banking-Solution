using CoreBanking.Application.Command.PasswordResetCommand;
using CoreBanking.Application.Command.TransactionPinCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Security;
using CoreBanking.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace CoreBanking.Application.CommandHandlers.PasswordResetCH
{
    public class ResendPasswordResetCodeHandler : IRequestHandler<ResendPasswordResetCodeCommand, Result>
    {

        private readonly IMediator _mediator;

        public ResendPasswordResetCodeHandler(IMediator mediator)
        {
                 _mediator = mediator;
        }

        public async Task<Result> Handle(ResendPasswordResetCodeCommand request, CancellationToken cancellationToken)
        {
            //  reuse the Send handler
            var result = await _mediator.Send(new SendPasswordResetCodeCommand
            {
                Email = request.Email
            });

            return result;
        }
    }
}
