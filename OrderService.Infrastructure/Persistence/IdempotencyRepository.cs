using Microsoft.EntityFrameworkCore;
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
        private readonly OrderDbContext _db;
        public IdempotencyRepository(OrderDbContext db)
        {
            _db = db;
        }

        public Task AddAsync(IdempotencyRecord idempotency, CancellationToken ct)
        {
            _db.IdempotencyRecords.Add(idempotency);
            return Task.CompletedTask;
        }

        public Task<Guid?> FindOrderIdAsync(string key, CancellationToken ct)
        {
            var record = _db.IdempotencyRecords.FirstOrDefault(x => x.Key == key);
            return Task.FromResult(record?.OrderId);
        }
    }
}
