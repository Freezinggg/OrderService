using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderService.Application.Common;
using OrderService.Application.Interface.Repository;
using OrderService.Application.Outbox.Payloads;
using OrderService.Application.Services;
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
                        var result = consumer.Consume(TimeSpan.Zero);

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
                                var projectionService = scope.ServiceProvider.GetRequiredService<OrderProjectionService>();

                                bool handleResult = await projectionService.HandleAsync(message, stoppingToken);
                                if (handleResult) consumer.Commit(result);
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