using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderService.Application.Common;
using OrderService.Application.Interface.Repository;
using OrderService.Application.Outbox.Payloads;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Configuration.Kafka;
using StackExchange.Redis;

namespace OrderService.API.WorkerService.Event.Consumer.Projection
{
    public class OrderProjectionConsumerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderProjectionConsumerWorker> _logger;
        private readonly KafkaOptions _kafkaConfig;

        public OrderProjectionConsumerWorker(IServiceScopeFactory scopeFactory,
            ILogger<OrderProjectionConsumerWorker> logger,
            IOptions<KafkaOptions> kafkaConfig)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _kafkaConfig = kafkaConfig.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.WriteLine("Starting Consumer Worker");

                var config = new ConsumerConfig
                {
                    BootstrapServers = _kafkaConfig.BootstrapServers,
                    GroupId = _kafkaConfig.OrderProjectionGroup,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                using var consumer =
                    new ConsumerBuilder<Ignore, string>(config)
                        .Build();

                Console.WriteLine("Consumer Built");

                consumer.Subscribe(_kafkaConfig.OrderEventsTopic);

                Console.WriteLine($"Subscribed to {_kafkaConfig.OrderEventsTopic}");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        Console.WriteLine("Before Poll");
                        var result = consumer.Consume(TimeSpan.Zero);
                        Console.WriteLine("After Poll");

                        /*
                                POINT FOR LATER:
                                Might add constraint UNIQUE(OrderId) to prevent duplicate/TOCTOU,
                                so only one projection might exist here
                        */

                        if (result is null) Console.WriteLine("No message");
                        else
                        {
                            Console.WriteLine($"Message received: {result.Message.Value}");

                            var message = JsonConvert.DeserializeObject<EventMessage>(result.Message.Value);

                            if (message is null)
                            {
                                Console.WriteLine("Payload is null");
                                continue;
                            }
                            else
                            {
                                using var scope = _scopeFactory.CreateScope();
                                var _orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                                var _orderProjectionRepo = scope.ServiceProvider.GetRequiredService<IOrderProjectionRepository>();

                                if (Enum.TryParse<EventType>(message.EventType, out var eventType))
                                {
                                    switch (eventType)
                                    {
                                        //really want to make function for this for cleaner, but might pass too much parameter bcs scoped services
                                        case EventType.OrderCreated:
                                            {

                                                var orderCreatedPayload = JsonConvert.DeserializeObject<OrderCreatedPayload>(message.Payload);
                                                if (orderCreatedPayload is null)
                                                {
                                                    Console.WriteLine("Invalid payload");
                                                    break;
                                                }

                                                var orderProjection = await _orderProjectionRepo.GetByIdAsync(orderCreatedPayload.OrderId, stoppingToken);
                                                if (orderProjection is null)
                                                {
                                                    Console.WriteLine($"Creating projection {orderCreatedPayload.OrderId}");

                                                    //Create projection
                                                    OrderProjection projection = new OrderProjection(orderCreatedPayload.OrderId, orderCreatedPayload.Status);
                                                    await _orderProjectionRepo.AddAsync(projection, stoppingToken);
                                                }
                                                else Console.WriteLine($"Duplicate OrderCreated({orderCreatedPayload.OrderId}) ignored");

                                                break;
                                            }
                                        case EventType.OrderCompleted:
                                            {
                                                var orderCompletedPayload = JsonConvert.DeserializeObject<OrderCompletedPayload>(message.Payload);
                                                if (orderCompletedPayload is null)
                                                {
                                                    Console.WriteLine("Invalid payload");
                                                    break;
                                                }

                                                var orderProjection = await _orderProjectionRepo.GetByIdAsync(orderCompletedPayload.OrderId, stoppingToken);

                                                //Create projection if null, if not then updated
                                                if (orderProjection is null)
                                                {
                                                    Console.WriteLine($"Creating projection {orderCompletedPayload.OrderId}");

                                                    //Create projection
                                                    OrderProjection projection = new OrderProjection(orderCompletedPayload.OrderId, OrderStatus.Completed);
                                                    await _orderProjectionRepo.AddAsync(projection, stoppingToken);
                                                }
                                                else
                                                {
                                                    //Update projection
                                                    orderProjection.UpdateStatus(OrderStatus.Completed);
                                                    await _orderProjectionRepo.UpdateStatus(orderProjection, stoppingToken);
                                                }

                                                break;
                                            }
                                    }

                                    Console.WriteLine($"Committing offset {result.Offset}");

                                    consumer.Commit(result);

                                    Console.WriteLine($"Committed offset {result.Offset}");
                                }
                            }
                        }

                        // Prevent hot-looping.
                        // Consume(TimeSpan.Zero) returns immediately when no message exists.
                        // Without this delay the worker will spin aggressively
                        // and create excessive CPU/load and freeze docker.
                        await Task.Delay(1000, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Poll Error: {ex}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OrderProjectionConsumerWorker failed");
            }
        }
    }
}


//using Confluent.Kafka;
//using Microsoft.Extensions.Options;
//using Newtonsoft.Json;
//using OrderService.Application.Interface.Repository;
//using OrderService.Application.Outbox.Payloads;
//using OrderService.Domain.Entities;
//using OrderService.Infrastructure.Configuration.Kafka;
//using StackExchange.Redis;

//namespace OrderService.API.WorkerService.Event
//{
//    public class OrderProjectionConsumerWorker : BackgroundService
//    {
//        private readonly IServiceScopeFactory _scopeFactory;
//        private readonly ILogger<OrderProjectionConsumerWorker> _logger;
//        private readonly KafkaOptions _kafkaConfig;

//        public OrderProjectionConsumerWorker(IServiceScopeFactory scopeFactory,
//            ILogger<OrderProjectionConsumerWorker> logger,
//            IOptions<KafkaOptions> kafkaConfig)
//        {
//            _scopeFactory = scopeFactory;
//            _logger = logger;
//            _kafkaConfig = kafkaConfig.Value;
//        }

//        protected override async Task ExecuteAsync(
//            CancellationToken stoppingToken)
//        {
//            try
//            {
//                Console.WriteLine("Starting Consumer Worker");

//                var config = new ConsumerConfig
//                {
//                    BootstrapServers = _kafkaConfig.BootstrapServers,
//                    GroupId = _kafkaConfig.OrderProjectionGroup,
//                    AutoOffsetReset = AutoOffsetReset.Earliest,
//                    EnableAutoCommit = false
//                };

//                using var consumer =
//                    new ConsumerBuilder<Ignore, string>(config)
//                        .Build();

//                Console.WriteLine("Consumer Built");

//                consumer.Subscribe(
//                    _kafkaConfig.OrderEventsTopic);

//                Console.WriteLine(
//                    $"Subscribed to {_kafkaConfig.OrderEventsTopic}");

//                while (!stoppingToken.IsCancellationRequested)
//                {
//                    try
//                    {
//                        using var scope = _scopeFactory.CreateScope();
//                        var _orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
//                        var _orderProjectionRepo = scope.ServiceProvider.GetRequiredService<IOrderProjectionRepository>();

//                        Console.WriteLine("Before Poll");
//                        var result = consumer.Consume(TimeSpan.Zero);
//                        Console.WriteLine("After Poll");

//                        if (result is null) Console.WriteLine("No message");
//                        else
//                        {
//                            Console.WriteLine($"Message received: {result.Message.Value}");

//                            //Handle order projection here
//                            var payload = JsonConvert.DeserializeObject<OrderCreatedPayload>(result.Message.Value);
//                            if (payload is null)
//                            {
//                                Console.WriteLine("Payload is null");
//                                continue;
//                            }
//                            else
//                            {
//                                /*
//                                 POINT FOR LATER:
//                                    Might add constraint UNIQUE(OrderId) to prevent duplicate/TOCTOU,
//                                    so only one projection might exist here
//                                 */
//                                var orderProjection = await _orderProjectionRepo.GetByIdAsync(payload.OrderId, stoppingToken);
//                                if (orderProjection is null)
//                                {
//                                    Console.WriteLine($"Creating projection {payload.OrderId}");

//                                    //create
//                                    OrderProjection projection = new OrderProjection(payload.OrderId, payload.Status);
//                                    await _orderProjectionRepo.AddAsync(projection, stoppingToken);
//                                }
//                                else Console.WriteLine($"Duplicate OrderCreated({payload.OrderId}) ignored");


//                                Console.WriteLine($"Committing offset {result.Offset}");

//                                consumer.Commit(result);

//                                Console.WriteLine($"Committed offset {result.Offset}");
//                            }
//                        }

//                        await Task.Delay(1000, stoppingToken);
//                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine(
//                            $"Poll Error: {ex}");

//                        throw;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(
//                    ex,
//                    "OrderProjectionConsumerWorker failed");
//            }
//        }
//    }
//}