using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OrderService.Application.Common;
using OrderService.Application.Interface;
using OrderService.Domain.Entities;
using OrderService.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static OrderService.Domain.Exception.DomainException;

namespace OrderService.Application.Handler.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
    {
        private readonly IIdempotencyRepository _idempotencyRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _uow;
        private readonly IOrderMetric _metric;

        private static bool IsIdempotencyConflict(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx)
            {
                return pgEx.SqlState == PostgresErrorCodes.UniqueViolation;
            }

            return false;
        }

        public CreateOrderCommandHandler(IIdempotencyRepository idempotencyRepository, IOrderRepository orderRepository, IUnitOfWork uow, IOrderMetric metric)
        {
            _idempotencyRepository = idempotencyRepository;
            _orderRepository = orderRepository;
            _uow = uow;
            _metric = metric;
        }

        public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            Result<Guid> result = new();

            await _uow.BeginAsync(cancellationToken);
            try
            {
                DateTime currentDateTime = DateTime.UtcNow;
                Guid orderId = Guid.NewGuid();
                var items = request.Items
                    .Select(i => new OrderItem(Guid.NewGuid(), orderId, i.Code, i.Quantity))
                    .ToList().AsReadOnly(); //Make it secure, cannot be mutated
                Order order = new(
                    orderId,
                    request.CustomerId,
                    items,
                    currentDateTime,
                    currentDateTime.AddMinutes(30),
                    request.IdempotencyKey
                    );

                IdempotencyRecord idempotencyRecord = new(Guid.NewGuid(), request.IdempotencyKey, order.Id, currentDateTime);

                //persist inside atomic boundary. IDEMPOTENCY first, then ORDER. ALWAYS.
                //Because when the order entered first, then idempoten then when retries -> duplicate order. breaks the point of idempotency.

                //PERSIST INTENT FIRST
                await _idempotencyRepository.AddAsync(idempotencyRecord, cancellationToken);

                //For testing purpose
                //await Task.Delay(2000, cancellationToken);

                //PERSIST OUTCOME THEN
                await _orderRepository.AddAsync(order, cancellationToken);

                await _uow.CommitAsync(cancellationToken);

                //Please remove this after testing
                //Environment.FailFast("Simulated crash after commit");

                result = Result<Guid>.Success(order.Id);
            }
            catch (DbUpdateException ex) when (IsIdempotencyConflict(ex))
            {
                // Another request won the race, so we check the idempoten (this will catch exception if theres exist same key inside the db, thats why the expcetion is dbexception)
                var existingOrderId =
                    await _idempotencyRepository.FindOrderIdAsync(request.IdempotencyKey, cancellationToken);

                await _uow.RollbackAsync(cancellationToken);
                result = Result<Guid>.Success(existingOrderId!.Value);
            }
            catch (DomainException ex)
            {
                //This is domain exception, which is to check INVARIANT
                await _uow.RollbackAsync(cancellationToken);

                switch (ex.Category)
                {
                    case FailureCategory.Invariant:
                        result = Result<Guid>.Invalid(ex.Message);
                        break;
                    case FailureCategory.Policy or FailureCategory.State:
                        result = Result<Guid>.Fail(ex.Message);
                        break;
                    default:
                        result = Result<Guid>.Error("Unhandled domain exception.");
                        break;
                }
            }
            catch
            {
                //Catch in general, system crash etc
                await _uow.RollbackAsync(cancellationToken);
                result = Result<Guid>.ServiceUnavailable("Service unavailable");
            }
            finally
            {
                stopwatch.Stop();

                //Add metrics, result will depend on Status of result
                _metric.RecordCreateOrder(
                    result.Status,
                    stopwatch.ElapsedMilliseconds
                );
            }

            return result;
        }
    }
}
