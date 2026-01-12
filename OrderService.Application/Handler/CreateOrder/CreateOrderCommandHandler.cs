using MediatR;
using OrderService.Application.Common;
using OrderService.Application.Interface;
using OrderService.Domain.Entities;
using OrderService.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Handler.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
    {
        private readonly IIdempotencyRepository _idempotencyRepository;
        private readonly IOrderRepository _orderRepository;

        public CreateOrderCommandHandler(IIdempotencyRepository idempotencyRepository, IOrderRepository orderRepository)
        {
            _idempotencyRepository = idempotencyRepository;
            _orderRepository = orderRepository;
        }

        public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // TODO: begin transaction (DB enforced)
            try
            {
                //check idempotency store
                Guid? existingOrderId = await _idempotencyRepository.FindOrderIdAsync(request.IdempotencyKey, cancellationToken);
                if (existingOrderId != null) return Result<Guid>.Success(existingOrderId.Value);

                DateTime currentDateTime = DateTime.UtcNow;
                var items = request.Items
                    .Select(i => new OrderItem(i.Code, i.Quantity))
                    .ToList().AsReadOnly(); //Make it secure, cannot be mutated
                Order order = new(
                    Guid.NewGuid(),
                    request.CustomerId,
                    items,
                    currentDateTime,
                    currentDateTime.AddMinutes(30),
                    request.IdempotencyKey
                    );

                IdempotencyRecord idempotencyRecord = new(Guid.NewGuid(), request.IdempotencyKey, order.Id, currentDateTime);

                //persist inside atomic boundary
                await _orderRepository.AddAsync(order, cancellationToken);
                await _idempotencyRepository.AddAsync(idempotencyRecord, cancellationToken);

                // TODO: commit

                return Result<Guid>.Success(order.Id);
            }
            catch (DomainException ex)
            {
                return Result<Guid>.Invalid(ex.Message);
            }
        }
    }
}
