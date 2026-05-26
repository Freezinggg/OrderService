using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interface.Repository;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence.Repository
{
    public class OrderProjectionRepository : IOrderProjectionRepository
    {
        private readonly OrderDbContext _db;

        public OrderProjectionRepository(OrderDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(OrderProjection orderProjection, CancellationToken ct)
        {
            await _db.OrderProjections.AddAsync(orderProjection);
            await _db.SaveChangesAsync(ct);
        }

        public Task<OrderProjection?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return _db.OrderProjections.FirstOrDefaultAsync(x => x.OrderId == id, ct);
        }

        public async Task UpdateStatus(OrderProjection orderProjection, CancellationToken ct)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
