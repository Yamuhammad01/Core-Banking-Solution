using CoreBanking.Application.Command.TransactionPinCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.CommandHandlers.TransactionPinCH
{
    public class ResendPinResetHandler : IRequestHandler<ResendPinResetCommand, Result>
    {
        private readonly IMediator _mediator;

        public ResendPinResetHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result> Handle(ResendPinResetCommand request, CancellationToken cancellationToken)
        {
            //  reuse the Send handler
            var result = await _mediator.Send(new SendPinResetCommand
            {
                Email = request.Email
            });

            return result;
        }
    }
}
