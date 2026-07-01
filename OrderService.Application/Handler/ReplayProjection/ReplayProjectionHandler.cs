using OrderService.Application.Common;
using OrderService.Application.Interface.Repository;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Handler.ReplayProjection
{
    public class ReplayProjectionHandler
    {
        private readonly IOutboxEventRepository _outboxRepository;
        private readonly OrderProjectionService _orderProjectionService;
        private readonly IOrderProjectionRepository _orderProjectionRepository;

        public ReplayProjectionHandler(IOutboxEventRepository outboxRepository, OrderProjectionService orderProjectionService, IOrderProjectionRepository orderProjectionRepository)
        {
            _outboxRepository = outboxRepository;
            _orderProjectionService = orderProjectionService;
            _orderProjectionRepository = orderProjectionRepository;
        }

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _orderProjectionRepository.DeleteAllAsync(cancellationToken);

                var outboxEvents = await _outboxRepository.GetAllAsync(cancellationToken);
                Console.WriteLine($"Total outbox events {outboxEvents.Count}");
                foreach (var outbox in outboxEvents)
                {
                    await Task.Delay(3000, cancellationToken);
                    Console.WriteLine($"Processing outbox {outbox.Id}");
                    try
                    {
                        if (string.IsNullOrEmpty(outbox.Payload))
                        {
                            Console.WriteLine($"Paylod empty [Outbox ID : {outbox.Id}]");
                            continue;
                        }

                        var eventMsg = new EventMessage()
                        {
                            EventId = outbox.Id,
                            EventType = outbox.EventType.ToString(),
                            EventVersion = outbox.EventVersion,
                            Payload = outbox.Payload,
                            OccurredAtUtc = outbox.CreatedAt
                        };

                        bool handleResult = await _orderProjectionService.HandleAsync(eventMsg, cancellationToken);
                        if (handleResult) Console.WriteLine($"Handle order success for [Outbox ID : {outbox.Id}]");
                        else Console.WriteLine($"Handle order fail for [Outbox ID : {outbox.Id}]");
                    }
                    catch
                    {
                        Console.WriteLine($"Error handling outbox. [Outbox ID: {outbox.Id}]");
                    }   
                }
            }
            catch
            {
                Console.WriteLine($"Error handling projection replay.");
            }
        }
    }
}
