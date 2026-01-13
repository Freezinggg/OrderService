using OrderService.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities
{
    public sealed class Order
    {
        public Guid Id { get; }
        public Guid CustomerId { get; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime ExpiredAt { get; }
        public string IdempotencyKey { get; }

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
        private readonly List<OrderItem> _items;

        private Order()
        {
            _items = new List<OrderItem>();
        }

        public Order(
            Guid id,
            Guid customerId,
            IEnumerable<OrderItem> items,
            DateTime createdAt,
            DateTime expiredAt,
            string idempotencyKey)
        {
            if (id == Guid.Empty)
                throw new DomainException("OrderId cannot be empty.");

            if (customerId == Guid.Empty)
                throw new DomainException("Customer cannot be empty.");

            if (items == null || !items.Any())
                throw new DomainException("Order must contain at least one item.");

            if (string.IsNullOrWhiteSpace(idempotencyKey))
                throw new DomainException("IdempotencyKey is required.");

            Id = id;
            CustomerId = customerId;
            CreatedAt = createdAt;
            ExpiredAt = expiredAt;
            IdempotencyKey = idempotencyKey;

            Status = OrderStatus.Active;
            _items = new List<OrderItem>(items);
        }
    }


    public enum OrderStatus : int
    {
        Active = 1,
        Canceled = 2,
        Expired = 3
    }
}
