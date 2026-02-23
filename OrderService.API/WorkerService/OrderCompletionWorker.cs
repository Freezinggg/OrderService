using OrderService.Application.Interface;

namespace OrderService.API.WorkerService
{
    public sealed class OrderCompletionWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderCompletionWorker> _logger;
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);


        public OrderCompletionWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<OrderCompletionWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderCompletionWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                    var now = DateTime.UtcNow;
                    var activeOrders = await repo.GetActiveOrderIdsAsync(stoppingToken);

                    foreach (var id in activeOrders)
                    {
                        //await Task.Delay(5000, stoppingToken); // artificial delay
                        await repo.TryCompleteAsync(id, now, stoppingToken);
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
