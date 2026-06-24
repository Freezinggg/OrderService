using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Outbox.Payloads
{
    public record OrderCompletedPayload
    {
        public Guid OrderId { get; init; }
        public OrderStatus Status { get; init; }
        public OrderCompletedPayload(Guid orderId, OrderStatus status)
        {
            OrderId = orderId;
            Status = status;
        }
    }
}
