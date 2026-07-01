using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Common
{
    public class EventMessage
    {
        public Guid EventId { get; set; } //Outbox ID
        public string EventType { get; set; } = default!;
        public string Payload { get; set; } = default!;
        public int EventVersion { get; set; } = default;
        public DateTime OccurredAtUtc { get; set; } //When bussiness fact happened
    }
}
