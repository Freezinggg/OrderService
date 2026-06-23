using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderService.Application.Interface.Repository;
using OrderService.Application.Outbox.Payloads;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Configuration.Kafka;
using StackExchange.Redis;

namespace OrderService.API.WorkerService.Event
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

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
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

                consumer.Subscribe(
                    _kafkaConfig.OrderEventsTopic);

                Console.WriteLine(
                    $"Subscribed to {_kafkaConfig.OrderEventsTopic}");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var _orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                        var _orderProjectionRepo = scope.ServiceProvider.GetRequiredService<IOrderProjectionRepository>();

                        Console.WriteLine("Before Poll");
                        var result = consumer.Consume(TimeSpan.Zero);
                        Console.WriteLine("After Poll");

                        if (result is null) Console.WriteLine("No message");
                        else
                        {
                            Console.WriteLine($"Message received: {result.Message.Value}");

                            //Handle order projection here
                            var payload = JsonConvert.DeserializeObject<OrderCreatedPayload>(result.Message.Value);
                            if (payload is null)
                            {
                                Console.WriteLine("Payload is null");
                                continue;
                            }
                            else
                            {
                                /*
                                 POINT FOR LATER:
                                    Might add constraint UNIQUE(OrderId) to prevent duplicate/TOCTOU,
                                    so only one projection might exist here
                                 */
                                var orderProjection = await _orderProjectionRepo.GetByIdAsync(payload.OrderId, stoppingToken);
                                if (orderProjection is null)
                                {
                                    Console.WriteLine($"Creating projection {payload.OrderId}");

                                    //create
                                    OrderProjection projection = new OrderProjection(payload.OrderId, payload.Status);
                                    await _orderProjectionRepo.AddAsync(projection, stoppingToken);
                                }
                                else Console.WriteLine($"Duplicate OrderCreated({payload.OrderId}) ignored");


                                Console.WriteLine($"Committing offset {result.Offset}");

                                consumer.Commit(result);

                                Console.WriteLine($"Committed offset {result.Offset}");
                            }
                        }

                        await Task.Delay(1000, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Poll Error: {ex}");

                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "OrderProjectionConsumerWorker failed");
            }
        }
    }
}
