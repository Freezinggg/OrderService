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
            //_logger.LogInformation("OrderCompletionWorker started.");

            Console.WriteLine($"Worker {Guid.NewGuid()} started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                    var claimedOrders = await repo.ClaimPendingOrderAsync(10, stoppingToken);
                    _orderMetric.SetPendingCount(claimedOrders.Count);

                    foreach (var order in claimedOrders)
                    {
                        Console.WriteLine($"Claimed {order.Id} with ExecId {order.ExecutionId} at {DateTime.Now}");
                        await Task.Delay(20000);

                        var now = DateTime.UtcNow;
                        var success = await repo.TryCompleteAsync(order.Id, order.ExecutionId, now, stoppingToken);
                        if (success)
                        {
                            var ageSeconds = (now - order.CreatedAt).TotalSeconds;
                            _orderMetric.RecordAsyncPendingAge(ageSeconds);
                            _orderMetric.RecordCompleted();
                            Console.WriteLine($"Completion success: {success} with exec id {order.ExecutionId} at {DateTime.Now}");
                        }
                        else
                        {
                            Console.WriteLine($"Completion failed: {success} with exec id {order.ExecutionId} at {DateTime.Now}");
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
