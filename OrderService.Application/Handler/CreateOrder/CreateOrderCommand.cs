using MediatR;
using OrderService.Application.Common;
using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Handler.CreateOrder
{
    public class CreateOrderCommand : IRequest<Result<Guid>>
    {
        //Set was not set to public so no bypassing invariant
        public Guid Id { get; }
        public Guid CustomerId { get; }
        public string IdempotencyKey { get; }
        public IReadOnlyCollection<OrderItemDTO> Items { get; }

        public CreateOrderCommand(
            Guid customerId,
            string idempotencyKey,
        IReadOnlyCollection<OrderItemDTO> items)
        {
            CustomerId = customerId;
            IdempotencyKey = idempotencyKey;
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }
    }
}
