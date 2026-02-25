using OrderService.Application.Interface;
using OrderService.Domain.Entities;

namespace OrderService.API.WorkerService
{
    public sealed class OrderCompletionWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOrderMetric _orderMetric;
        private readonly ILogger<OrderCompletionWorker> _logger;
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);


        public OrderCompletionWorker(
            IServiceScopeFactory scopeFactory,
            IOrderMetric orderMetric,
            ILogger<OrderCompletionWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _orderMetric = orderMetric;
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
                    var pendingOrders = await repo.ClaimPendingOrderAsync(10, stoppingToken);
                    _orderMetric.SetPendingCount(pendingOrders.Count);

                    foreach (var order in pendingOrders)
                    {
                        var success = await repo.TryCompleteAsync(order.id, now, stoppingToken);
                        if (success)
                        {
                            var ageSeconds = (now - order.createdAt).TotalSeconds;
                            _orderMetric.RecordAsyncPendingAge(ageSeconds);
                            _orderMetric.RecordCompleted();
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
