using CoreBanking.Application.Command.PasswordResetCommand;
using CoreBanking.Application.Common;
using MediatR;

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
