using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderService.Application.Interface.Repository;
using OrderService.Infrastructure.Configuration.Kafka;

namespace OrderService.API.WorkerService.Event
{
    public class EventPublishWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventPublishWorker> _logger;
        private readonly KafkaOptions _kafkaConfig;

        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);
        private readonly ProducerConfig config;
        private readonly IProducer<Null, string> producer;

        public EventPublishWorker(IServiceScopeFactory scopeFactory
            ,ILogger<EventPublishWorker> logger
            ,IOptions<KafkaOptions> kafkaConfig)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _kafkaConfig = kafkaConfig.Value;

            config = new ProducerConfig
            {
                BootstrapServers = _kafkaConfig.BootstrapServers
            };

            producer = new ProducerBuilder<Null, string>(config).Build();
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"Worker {Guid.NewGuid()} started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"Polling at [{DateTime.UtcNow}..]");
                    using var scope = _scopeFactory.CreateScope();
                    var _outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();

                    //Claim Outbox
                    var claimedOutboxEvents = await _outboxRepo.ClaimOutboxEventsAsync(10, stoppingToken);
                    Console.WriteLine($"Claimed outbox event count : [{claimedOutboxEvents.Count}]");

                    //Loop through the claimed outbox
                    foreach (var outboxEvent in claimedOutboxEvents)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(outboxEvent.Payload))
                            {
                                _logger.LogError($"Paylod empty [Outbox ID : {outboxEvent.Id}]");
                                continue;
                            }
                            var result = await producer.ProduceAsync(_kafkaConfig.OrderEventsTopic, new Message<Null, string> { Value = outboxEvent.Payload });
                            Console.WriteLine(JsonConvert.SerializeObject(result));

                            await _outboxRepo.MarkOutboxEventPublished(outboxEvent.Id, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing [Outbox ID : {outboxEvent.Id}]");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error executing [Event Publish Worker]");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }
    }
}
