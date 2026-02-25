using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interface;
using OrderService.Application.Record;
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

        public async Task<List<PendingOrderRecord>> ClaimPendingOrderAsync(int batchSize, CancellationToken ct)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            // Select rows with lock so no other workers can claim that work
            var pendingOrders = await _db.Orders
                .FromSqlInterpolated($@"
                    SELECT ""Id"", ""CreatedAt""
                    FROM ""Orders""
                    WHERE ""Status"" = {(int)OrderStatus.Pending}
                    FOR UPDATE SKIP LOCKED
                    LIMIT {batchSize}")
                .Select(o => new PendingOrderRecord(o.Id, o.CreatedAt))
                .ToListAsync(ct);

            if (pendingOrders.Count == 0)
            {
                await tx.CommitAsync(ct);
                return pendingOrders;
            }

            //Mark as Processing immediately
            var ids = pendingOrders.Select(x => x.id).ToList();
            await _db.Orders
                .Where(o => ids.Contains(o.Id) && o.Status == OrderStatus.Pending)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.Status, OrderStatus.Processing)
                    .SetProperty(o => o.ProcessedAt, DateTime.UtcNow),
                    ct);

            await tx.CommitAsync(ct);

            return pendingOrders;
        }

        public async Task<bool> TryCancelAsync(Guid id,  CancellationToken ct)
        {
            var rows = await _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Orders""
                SET ""Status"" = {(int)OrderStatus.Cancelled}
                WHERE ""Id"" = {id}
                  AND ""Status"" = {(int)OrderStatus.Processing}");

            return rows == 1;
        }

        public async Task<bool> TryCompleteAsync(Guid id, DateTime now, CancellationToken ct)
        {
            var rows = await _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Orders""
            SET ""Status"" = {(int)OrderStatus.Completed}
            WHERE ""Id"" = {id}
              AND ""Status"" = {(int)OrderStatus.Processing}
              AND ""ExpiredAt"" > {now}");

            return rows == 1;
        }

        public async Task<int> ExpireProcessingOrderAsync(DateTime threshold, CancellationToken ct)
        {
            //LINQ order > Get order where status is processing & processedat != null (not pending) & processedat < threshold (expired, with 5 minutes threshold/grace period)
            //After above query executed, get the list of the orders, then update all the orders to expired
            return await _db.Orders
                .Where(o => o.Status == OrderStatus.Processing &&
                            o.ProcessedAt != null &&
                            o.ProcessedAt < threshold)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.Status, OrderStatus.Expired),
                    ct);
        }
    }
}
