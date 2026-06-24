using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OrderService.Application.Common;
using OrderService.Application.Handler.CancelOrder;
using OrderService.Application.Interface;
using OrderService.Application.Interface.Repository;
using OrderService.Application.Outbox.Payloads;
using OrderService.Domain.Entities;
using OrderService.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OrderService.Domain.Exception.DomainException;

namespace OrderService.Application.Handler.CompleteOrder
{
    public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, Result<bool>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IOrderRepository _orderRepository;
        private readonly IOutboxEventRepository _outboxRepository;

        public CompleteOrderCommandHandler(IUnitOfWork uow, IOrderRepository orderRepository, IOutboxEventRepository outboxRepository)
        {
            _uow = uow;
            _orderRepository = orderRepository;
            _outboxRepository = outboxRepository;
        }

        public async Task<Result<bool>> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (order == null) return Result<bool>.NotFound("Order doesn't exist.");

            await _uow.BeginAsync(cancellationToken);
            try
            {
                order.Complete();

                //Outbox
                var payload = JsonConvert.SerializeObject(new OrderCompletedPayload(order.Id, OrderStatus.Completed));
                OutboxEvent outboxEvent = new(Guid.NewGuid(), EventType.OrderCompleted, payload);
                await _outboxRepository.AddAsync(outboxEvent, cancellationToken);

                await _uow.CommitAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (DomainException ex)
            {
                var result = new Result<bool>();

                //This is domain exception, which is to check INVARIANT
                await _uow.RollbackAsync(cancellationToken);

                switch (ex.Category)
                {
                    case FailureCategory.Invariant:
                        result = Result<bool>.Invalid(ex.Message);
                        break;
                    case FailureCategory.Policy or FailureCategory.State:
                        result = Result<bool>.Fail(ex.Message);
                        break;
                    default:
                        result = Result<bool>.Error("Unhandled domain exception.");
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(cancellationToken);
                return Result<bool>.ServiceUnavailable("Service unavailable");
            }
        }
    }
}
