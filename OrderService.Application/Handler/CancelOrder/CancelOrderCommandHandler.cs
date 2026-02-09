using MediatR;
using OrderService.Application.Common;
using OrderService.Application.Interface;
using OrderService.Application.Interface.Cache;
using OrderService.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OrderService.Domain.Exception.DomainException;

namespace OrderService.Application.Handler.CancelOrder
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<bool>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderSummaryCache _cache;

        public CancelOrderCommandHandler(IUnitOfWork uow, IOrderRepository orderRepository, IOrderSummaryCache cache)
        {
            _uow = uow;
            _orderRepository = orderRepository;
            _cache = cache;
        }

        public async Task<Result<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (order == null) return Result<bool>.NotFound("Order doesn't exist.");

            await _uow.BeginAsync(cancellationToken);
            try
            {
                order.Cancel();

                await _uow.CommitAsync(cancellationToken);
                await _cache.InvalidateAsync(request.Id, cancellationToken);

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
            catch
            {
                //Catch in general, system crash etc
                await _uow.RollbackAsync(cancellationToken);
                return Result<bool>.ServiceUnavailable("Service unavailable");
            }
        }
    }
}
