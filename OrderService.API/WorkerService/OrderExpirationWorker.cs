using OrderService.Application.Interface;

namespace OrderService.API.WorkerService
{
    public sealed class OrderExpirationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOrderMetric _orderMetric;
        private readonly ILogger<OrderExpirationWorker> _logger;
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);

        public OrderExpirationWorker(IServiceScopeFactory scopeFactory, IOrderMetric orderMetric, ILogger<OrderExpirationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _orderMetric = orderMetric;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                    var processingCount = await repo.GetProcessingOrderCountAsync(stoppingToken);
                    _orderMetric.SetProcessingCurrent(processingCount);

                    var expiredRows = await repo.ExpireProcessingOrderAsync(DateTime.UtcNow, stoppingToken);

                    if (expiredRows > 0) _orderMetric.RecordProcessingExpired(expiredRows);
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
