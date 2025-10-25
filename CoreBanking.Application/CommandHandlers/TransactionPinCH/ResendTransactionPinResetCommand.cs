using CoreBanking.Application.Command;
using CoreBanking.Application.Common;
using CoreBanking.Application.Interfaces.IServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.CommandHandlers
{
    public class ResendTransactionPinResetCommandHandler : IRequest<ResendTransactionPinResetCommand, Result>
    {
        private readonly IBankingDbContext bankingDbContext;
        public ResendTransactionPinResetCommandHandler(IBankingDbContext bankingDbContext)
        {

        }

    }
}
