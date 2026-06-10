using Newtonsoft.Json;
using OrderService.Application.Interface.Repository;
using OrderService.Application.Outbox.Payloads;
using OrderService.Domain.Entities;

namespace OrderService.API.WorkerService
{
    public sealed class OutboxWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxWorker> _logger;
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);


        public OutboxWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation("OrderCompletionWorker started.");
            
            Console.WriteLine($"Worker {Guid.NewGuid()} started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    //initialize repo
                    var _orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                    var _outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();
                    var _orderProjectionRepo = scope.ServiceProvider.GetRequiredService<IOrderProjectionRepository>();

                    var claimedOutboxEvents = await _outboxRepo.ClaimOutboxEventsAsync(10, stoppingToken);

                    foreach (var outboxEvent in claimedOutboxEvents)
                    {
                        try
                        {
                            await Task.Delay(3000, stoppingToken);
                            await _outboxRepo.MarkOutboxEventProcessed(outboxEvent.Id, stoppingToken);
                            //throw new Exception("Projection update failed");


                            //TODO: do async work here

                            #region Async continuation projection
                            //Projection async work
                            var payload = JsonConvert.DeserializeObject<OrderCreatedPayload>(outboxEvent.Payload);
                            if(payload is null)
                            {
                                _logger.LogInformation("Payload is null");
                                continue;
                            }
                            else
                            {
                                var orderProjection = await _orderProjectionRepo.GetByIdAsync(payload.OrderId, stoppingToken);
                                Order order = await _orderRepo.GetByIdAsync(payload.OrderId, stoppingToken);

                                if (order is not null)
                                {
                                    //create projection order here?
                                    if (orderProjection is null)
                                    {
                                        //null, create projection
                                        OrderProjection projection = new(payload.OrderId, order.Status);
                                        await _orderProjectionRepo.AddAsync(projection, stoppingToken); //do i need aawit? bcs i dont need the result right
                                    }
                                    else
                                    {
                                        //not null, update
                                        orderProjection.UpdateStatus(order.Status);
                                        await _orderProjectionRepo.UpdateStatus(orderProjection, stoppingToken); //do i need here also awit? bcs i dont need the result right
                                    }
                                }
                                else
                                {
                                    _logger.LogInformation($"Potential order missing for [OutboxId: {outboxEvent.Id}, OrderId {payload.OrderId}].");
                                }
                            }
                            #endregion
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError(ex, $"Failed processing outbox event [OutboxId : {outboxEvent.Id}].");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker loop failed.");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }
    }
}
