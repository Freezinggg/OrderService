using OrderService.Application.Interface.Repository;

namespace OrderService.API.WorkerService.Reconciliation
{
    public class OrderReconciliationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderReconciliationWorker> _logger;
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

        public OrderReconciliationWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<OrderReconciliationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var _orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                    var _orderProjectionRepo = scope.ServiceProvider.GetRequiredService<IOrderProjectionRepository>();

                    var orders = await _orderRepo.GetAllAsync(stoppingToken);
                    foreach (var order in orders)
                    {
                        try
                        {
                            var projection = await _orderProjectionRepo.GetByIdAsync(order.Id, stoppingToken);
                            if (projection is null)
                            {
                                //create new one here, based on latest order
                                await _orderProjectionRepo.AddAsync(new Domain.Entities.OrderProjection(order.Id, order.Status), stoppingToken);
                                _logger.LogInformation($"Repairing projection for [OrderId: {order.Id}]");
                            }
                            else
                            {
                                continue;
                            }
                        }
                        catch(Exception ex)
                        {
                            _logger.LogInformation(ex, $"Repairing projection for [OrderId: {order.Id}] failed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing Order Reconciliation.");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }
    }
}
