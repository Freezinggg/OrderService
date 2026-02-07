using OrderService.Domain.Entities;
using OrderService.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTO
{
    public sealed class OrderSummaryDTO
    {
        public Guid Id { get; }
        public Guid CustomerId { get; }
        public OrderStatus Status { get; }
        public DateTime CreatedAt { get; }
        public DateTime ExpiredAt { get; }
        public IReadOnlyCollection<OrderItemDTO> Items { get; }

        public OrderSummaryDTO(
            Guid id,
            Guid customerId,
            OrderStatus status,
            DateTime createdAt,
            DateTime expiredAt,
            IReadOnlyCollection<OrderItemDTO> items)
        {
            Id = id;
            CustomerId = customerId;
            Status = status;
            CreatedAt = createdAt;
            ExpiredAt = expiredAt;
            Items = items;
        }
    }

}
