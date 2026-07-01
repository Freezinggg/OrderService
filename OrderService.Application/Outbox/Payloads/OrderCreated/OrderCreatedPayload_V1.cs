using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Outbox.Payloads.OrderCreated
{
    public record OrderCreatedPayload_V1
    {
        public Guid OrderId { get; init; }
        public OrderStatus Status { get; init; }
        public OrderCreatedPayload_V1(Guid orderId, OrderStatus status)
        {
            OrderId = orderId;
            Status = status;
        }
    }
}
