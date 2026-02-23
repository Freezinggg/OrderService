using Microsoft.Data.SqlClient;
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
            return _db.Orders.Include("_items").FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Guid>> GetActiveOrderIdsAsync(CancellationToken ct)
        {
            return await _db.Orders.Where(x => x.Status == OrderStatus.Pending).Select(x => x.Id).ToListAsync(ct);
        }

        public async Task<bool> TryCancelAsync(Guid id,  CancellationToken ct)
        {
            var rows = await _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Orders""
                SET ""Status"" = {(int)OrderStatus.Cancelled}
                WHERE ""Id"" = {id}
                  AND ""Status"" = {(int)OrderStatus.Pending}");

            return rows == 1;
        }

        public async Task<bool> TryCompleteAsync(Guid id, DateTime now, CancellationToken ct)
        {
            var rows = await _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Orders""
            SET ""Status"" = {(int)OrderStatus.Completed}
            WHERE ""Id"" = {id}
              AND ""Status"" = {(int)OrderStatus.Pending}
              AND ""ExpiredAt"" > {now}");

            return rows == 1;
        }
    }
}
