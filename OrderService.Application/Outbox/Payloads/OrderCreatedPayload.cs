using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Outbox.Payloads
{
    public class OrderCreatedPayload
    {
        public Guid OrderId { get; }
        public OrderCreatedPayload(Guid orderId) => OrderId = orderId;
    }
}
