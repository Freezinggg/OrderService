using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities
{
    public sealed class OrderProjection
    {
        public Guid OrderId { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime ProjectedAt { get; private set; }

        private OrderProjection() { }

        public OrderProjection(
            Guid orderId,
            OrderStatus status)
        {
            OrderId = orderId;
            Status = status;
            ProjectedAt = DateTime.UtcNow;
        }
    }
}
