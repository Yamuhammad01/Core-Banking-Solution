using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Domain.Entities
{
    public class InboxState
    {
        public Guid MessageId { get; set; }
        public string? ConsumerType { get; set; }
        public DateTime? Received { get; set; }
        public DateTime? Delivered { get; set; }
        public int DeliveryCount { get; set; }
        public bool Consumed { get; set; }
    }
}
