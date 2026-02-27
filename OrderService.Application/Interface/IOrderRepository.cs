using OrderService.Application.Record;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interface
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order, CancellationToken ct);
        Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);

        Task<List<PendingOrderRecord>> ClaimPendingOrderAsync(int batchSize, CancellationToken ct);
        Task<int> ExpireProcessingOrderAsync(DateTime now, CancellationToken ct);
        Task<bool> TryCompleteAsync(Guid id, Guid executionId, DateTime now, CancellationToken ct);
        Task<bool> TryCancelAsync(Guid id, CancellationToken ct);
        Task<int> GetProcessingOrderCountAsync(CancellationToken ct);
    }
}
