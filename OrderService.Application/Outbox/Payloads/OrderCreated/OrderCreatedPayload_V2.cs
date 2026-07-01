using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Outbox.Payloads.OrderCreated
{
    public class OrderCreatedPayload_V2
    {
        public Guid OrderId { get; init; }
        public OrderStatus Status { get; init; }
        public string Priority { get; set; } = default!;
        public OrderCreatedPayload_V2(Guid orderId, OrderStatus status, string priority)
        {
            OrderId = orderId;
            Status = status;
            Priority = priority;
        }
    }
}
