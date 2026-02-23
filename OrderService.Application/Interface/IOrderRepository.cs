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

        Task<List<Guid>> GetActiveOrderIdsAsync(CancellationToken ct);
        Task<bool> TryCompleteAsync(Guid id, DateTime now, CancellationToken ct);
        Task<bool> TryCancelAsync(Guid id, CancellationToken ct);
    }
}
