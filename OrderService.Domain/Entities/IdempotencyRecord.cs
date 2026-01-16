using OrderService.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entities
{
    public sealed class IdempotencyRecord
    {
        //No set because we dont want invariant bypass, only by Constructor the invariant can be passed.
        public Guid Id { get; }
        public string Key { get; }
        public Guid OrderId { get; }
        public DateTime CreatedAt { get; }

        public IdempotencyRecord(Guid id, string key, Guid orderId, DateTime createdAt)
        {
            if (id == Guid.Empty)
                throw new InvariantViolationException("Idempotency record id cannot be empty.");

            if (string.IsNullOrWhiteSpace(key))
                throw new InvariantViolationException("Idempotency key cannot be empty.");

            if (orderId == Guid.Empty)
                throw new InvariantViolationException("OrderId cannot be empty.");

            Id = id;
            Key = key;
            OrderId = orderId;
            CreatedAt = createdAt;
        }
    }
}
