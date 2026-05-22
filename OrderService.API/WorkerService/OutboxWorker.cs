using OrderService.Application.Interface;

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
                    var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                    var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxEventRepository>();

                    var claimedOutboxEvents = await outboxRepo.ClaimOutboxEventsAsync(10, stoppingToken);

                    foreach (var outboxEvent in claimedOutboxEvents)
                    {
                        try
                        {
                            //TODO:
                            //do async work here, change order to completion?

                            await Task.Delay(3000, stoppingToken);
                            await outboxRepo.MarkOutboxEventProcessed(outboxEvent.Id, stoppingToken);
                        }
                        catch
                        {

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
