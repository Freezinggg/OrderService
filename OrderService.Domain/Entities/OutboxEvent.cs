using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities
{
    public sealed class OutboxEvent
    {
        public Guid Id { get; }
        public EventType EventType { get; private set; }
        public string Payload { get; }
        public DateTime CreatedAt { get; }
        public DateTime? ProcessedAt { get; private set; }

        private OutboxEvent() { }

        public OutboxEvent(Guid id, EventType eventType, string payload)
        {
            Id = id;
            EventType = eventType;
            Payload = payload;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsProcessed()
        {
            ProcessedAt = DateTime.UtcNow;
        }
    }

    public enum EventType : int
    {
        OrderCreated = 0,
        OrderCompleted = 1,
    }
}
