using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Domain.Entities
{
    public class OutboxState
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? MessageId { get; set; }
        public string? CorrelationId { get; set; }
        public string? ConversationId { get; set; }
        public string? InitiatorId { get; set; }
        public string? SourceAddress { get; set; }
        public string? DestinationAddress { get; set; }
        public string? ResponseAddress { get; set; }
        public string? FaultAddress { get; set; }
        public string? RequestId { get; set; }
        public string? ContentType { get; set; }
        public byte[]? Body { get; set; }
        public string? Headers { get; set; }
        public DateTime? SentTime { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
        public bool Processed { get; set; } = false;
    }
}
