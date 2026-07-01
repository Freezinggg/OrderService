using Newtonsoft.Json;
using OrderService.Application.Common;
using OrderService.Application.Interface.Repository;
using OrderService.Application.Outbox.Payloads;
using OrderService.Application.Outbox.Payloads.OrderCreated;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Services
{
    public class OrderProjectionService
    {
        private readonly IOrderProjectionRepository _orderProjectionRepo;

        public OrderProjectionService(IOrderProjectionRepository orderProjectionRepo)
        {
            _orderProjectionRepo = orderProjectionRepo;
        }

        public async Task<bool> HandleAsync(EventMessage ev, CancellationToken cancellationToken)
        {
            bool result = false;
            try
            {
                if (Enum.TryParse<EventType>(ev.EventType, out var eventType))
                {
                    switch (eventType)
                    {
                        case EventType.OrderCreated:
                            result = await HandleOrderCreated(ev, cancellationToken);
                            break;
                        case EventType.OrderCompleted:
                            result = await HandleOrderCompleted(ev.Payload, cancellationToken);
                            break;
                        
                    }
                }
                else
                {
                    Console.WriteLine("Invalid event type.");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Order Projection Service handle error. {ex.ToString()}");
                return false;
            }

            return result;
        }

        private async Task<bool> HandleOrderCreated(EventMessage ev, CancellationToken cancellationToken)
        {
            try
            {
                switch (ev.EventVersion)
                {
                    case 1:
                        return await HandleOrderCreated_V1(ev.Payload, cancellationToken);
                    case 2:
                        return await HandleOrderCreated_V2(ev.Payload, cancellationToken);
                    default:
                        throw new NotSupportedException("Versioning mismatch");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Order Projection Service handle order create error. {ex.ToString()}");
                return false;
            }            
        }

        private async Task<bool> HandleOrderCreated_V1(string message, CancellationToken cancellationToken)
        {
            Console.WriteLine("Executing HandleOrderCreated_V1..");
            try
            {
                var payload = JsonConvert.DeserializeObject<OrderCreatedPayload_V1>(message);
                if (payload is null)
                {
                    Console.WriteLine("Invalid payload");
                    return false;
                }

                var projection = await _orderProjectionRepo.GetByIdAsync(payload.OrderId, cancellationToken);
                if (projection is null)
                {
                    Console.WriteLine($"Creating projection {payload.OrderId}");

                    //Create projection
                    await _orderProjectionRepo.AddAsync(
                        new OrderProjection(payload.OrderId, payload.Status) //could force status = create here, or the payload itself
                        , cancellationToken);
                }
                else
                {
                    Console.WriteLine($"Duplicate OrderCreated({payload.OrderId}) ignored");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Order Projection Service handle order create error. {ex.ToString()}");
                return false;
            }
        }

        private async Task<bool> HandleOrderCreated_V2(string message, CancellationToken cancellationToken)
        {
            Console.WriteLine("Executing HandleOrderCreated_V2..");
            try
            {
                var payload = JsonConvert.DeserializeObject<OrderCreatedPayload_V2>(message);
                if (payload is null)
                {
                    Console.WriteLine("Invalid payload");
                    return false;
                }

                var projection = await _orderProjectionRepo.GetByIdAsync(payload.OrderId, cancellationToken);
                if (projection is null)
                {
                    Console.WriteLine($"Creating projection {payload.OrderId}");

                    //Create projection
                    await _orderProjectionRepo.AddAsync(
                        new OrderProjection(payload.OrderId, payload.Status) //could force status = create here, or the payload itself
                        , cancellationToken);
                }
                else
                {
                    Console.WriteLine($"Duplicate OrderCreated({payload.OrderId}) ignored");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Order Projection Service handle order create error. {ex.ToString()}");
                return false;
            }
        }


        private async Task<bool> HandleOrderCompleted(string message, CancellationToken cancellationToken)
        {
            try
            {
                var payload = JsonConvert.DeserializeObject<OrderCompletedPayload>(message);
                if (payload is null)
                {
                    Console.WriteLine("Invalid payload");
                    return false;
                }

                var projection = await _orderProjectionRepo.GetByIdAsync(payload.OrderId, cancellationToken);
                if (projection is null)
                {
                    Console.WriteLine($"Creating projection {payload.OrderId}");

                    //Create projection
                    await _orderProjectionRepo.AddAsync(
                        new OrderProjection(payload.OrderId, payload.Status)
                        , cancellationToken);
                }
                else
                {
                    projection.UpdateStatus(OrderStatus.Completed);
                    await _orderProjectionRepo.UpdateStatus(projection, cancellationToken);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Order Projection Service handle order completed error. {ex.ToString()}");
                return false;
            }
        }
    }
}
