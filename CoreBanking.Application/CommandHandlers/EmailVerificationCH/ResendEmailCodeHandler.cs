using CoreBanking.Application.Command.EmailConfirmationCommand;
using CoreBanking.Application.Common;
using MediatR;

namespace CoreBanking.Application.CommandHandlers.EmailVerificationCH
{
    public class ResendEmailCodeHandler : IRequestHandler<ResendEmailCodeCommand, Result>
    {
        private readonly IMediator _mediator;

        public ResendEmailCodeHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result> Handle(ResendEmailCodeCommand request, CancellationToken cancellationToken)
        {
            //  reuse the Send handler
            var result = await _mediator.Send(new SendEmailCodeCommand
            {
                Email = request.Email
            });

            return result;
        }
    }
}
