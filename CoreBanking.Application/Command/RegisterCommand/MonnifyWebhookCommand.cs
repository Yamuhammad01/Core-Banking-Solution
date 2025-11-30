using CoreBanking.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreBanking.Application.Command.RegisterCommand
{
    public class MonnifyWebhookCommand : IRequest<Result>
    {
        public JsonElement Payload { get; }

        public MonnifyWebhookCommand(JsonElement payload)
        {
            Payload = payload;
        }
    }
}
