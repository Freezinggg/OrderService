using OrderService.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities
{
    public sealed class OrderItem
    {
        public Guid Id { get; }
        public Guid OrderId { get; }

        public string Code { get; set; }
        public int Quantity { get; set; }

        private OrderItem() { }
        public OrderItem(Guid id, Guid orderId, string code, int quantity)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Code must not be empty.");
            if (quantity <= 0) throw new DomainException("Quantity must be at leastt 1");

            Id = id;
            OrderId = orderId;
            Code = code;
            Quantity = quantity;
        }
    }
}
