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

        public DateTime FirstProjectedAt { get; private set; }
        public DateTime LastProjectedAt { get; private set; }

        private OrderProjection() { }

        public OrderProjection(
            Guid orderId,
            OrderStatus status)
        {
            OrderId = orderId;
            Status = status;

            FirstProjectedAt = DateTime.UtcNow;
            LastProjectedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(OrderStatus status)
        {
            Status = status;
            LastProjectedAt = DateTime.UtcNow;
        }
    }
}
