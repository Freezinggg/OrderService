using OrderService.Application.Interface;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private static readonly List<IdempotencyRecord> _records = new();
        public Task AddAsync(IdempotencyRecord idempotency, CancellationToken ct)
        {
            _records.Add(idempotency);
            return Task.CompletedTask;
        }

        public Task<Guid?> FindOrderIdAsync(string key, CancellationToken ct)
        {
            var record = _records.FirstOrDefault(x => x.Key == key);
            return Task.FromResult(record?.OrderId);
        }
    }
}
