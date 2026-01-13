using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interface;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _db;
        public OrderRepository(OrderDbContext db)
        {
            _db = db;
        }

        public Task AddAsync(Order order, CancellationToken ct)
        {
            _db.Orders.AddAsync(order);
            //await _db.SaveChangesAsync(); dont put savechangesasync here, will make atomicity gone because it will be partial commit
            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return _db.Orders.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
