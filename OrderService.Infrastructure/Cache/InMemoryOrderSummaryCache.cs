using OrderService.Application.DTO;
using OrderService.Application.Interface.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Cache
{
    public sealed class InMemoryOrderSummaryCache : IOrderSummaryCache
    {
        private static readonly Dictionary<Guid, OrderSummaryDTO> _cache = new();

        public Task<OrderSummaryDTO?> GetAsync(Guid orderId, CancellationToken ct)
        {
            _cache.TryGetValue(orderId, out var value);
            return Task.FromResult(value);
        }

        public Task InvalidateAsync(Guid orderId, CancellationToken ct)
        {
            _cache.Remove(orderId);
            return Task.CompletedTask;
        }

        public Task SetAsync(Guid orderId, OrderSummaryDTO summary, CancellationToken ct)
        {
            _cache[orderId] = summary;
            return Task.CompletedTask;
        }
    }
}
