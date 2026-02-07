using OrderService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interface.Cache
{
    public interface IOrderSummaryCache
    {
        Task<OrderSummaryDTO?> GetAsync(Guid orderId, CancellationToken ct);
        Task SetAsync(Guid orderId, OrderSummaryDTO summary, CancellationToken ct);
        Task InvalidateAsync(Guid orderId, CancellationToken ct);
    }
}
