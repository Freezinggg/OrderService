using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OrderService.Infrastructure.Configuration.Kafka;

namespace OrderService.API.WorkerService.Event
{
    public class OrderProjectionConsumerWorker : BackgroundService
    {
        private readonly ILogger<OrderProjectionConsumerWorker> _logger;
        private readonly KafkaOptions _kafkaConfig;

        public OrderProjectionConsumerWorker(
            ILogger<OrderProjectionConsumerWorker> logger,
            IOptions<KafkaOptions> kafkaConfig)
        {
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
                        Console.WriteLine("Before Poll");

                        var result = consumer.Consume(
                            TimeSpan.Zero);

                        Console.WriteLine("After Poll");

                        if (result is null)
                        {
                            Console.WriteLine("No message");
                        }
                        else
                        {
                            Console.WriteLine(
                                $"Message received: {result.Message.Value}");
                        }

                        await Task.Delay(
                            1000,
                            stoppingToken);
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
