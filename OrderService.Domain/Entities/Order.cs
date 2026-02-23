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
                throw new InvariantViolationException("OrderId cannot be empty.");

            if (customerId == Guid.Empty)
                throw new InvariantViolationException("Customer cannot be empty.");

            if (items == null || !items.Any())
                throw new InvariantViolationException("Order must contain at least one item.");

            if (string.IsNullOrWhiteSpace(idempotencyKey))
                throw new InvariantViolationException("IdempotencyKey is required.");

            Id = id;
            CustomerId = customerId;
            CreatedAt = createdAt;
            ExpiredAt = expiredAt;
            IdempotencyKey = idempotencyKey;

            Status = OrderStatus.Pending;
            _items = new List<OrderItem>(items);
        }

        public void Cancel()
        {
            EnsureNotTerminal();
            Status = OrderStatus.Cancelled;
        }

        public void Expire()
        {
            EnsureNotTerminal();
            Status = OrderStatus.Expired;
        }

        public void EnsureNotTerminal()
        {
            if (Status == OrderStatus.Cancelled || Status == OrderStatus.Expired || Status == OrderStatus.Completed)
                throw new InvalidStateTransitionException($"Order {Id} is in terminal state: {Status}");
        }
    }


    public enum OrderStatus : int
    {
        Pending = 1, //row/order exist, completion not yet executed
        Completed = 2, //async finished, business invariant met, terminal
        Cancelled = 3, //terminal, by client
        Expired = 4 //time-based invalidation, terminal
    }
}
