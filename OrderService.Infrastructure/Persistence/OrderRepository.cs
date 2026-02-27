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
            var now = DateTime.UtcNow;
            var leaseUntil = now.AddSeconds(30);
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            // Select rows with lock so no other workers can claim that work
            //where status of order is pending
            //  or (status = processing and processinguntil < now (this one means the job is update to processing, but might crash midway))
            var claimableOrders = await _db.Orders
                .FromSqlInterpolated($@"
                        SELECT ""Id"", ""CreatedAt""
                        FROM ""Orders""
                        WHERE ""Status"" = {(int)OrderStatus.Pending}
                           OR (""Status"" = {(int)OrderStatus.Processing} AND ""ProcessingUntil"" < {now})
                        FOR UPDATE SKIP LOCKED
                        LIMIT {batchSize}")
                .Select(o => new { o.Id, o.CreatedAt })
                .ToListAsync(ct);

            if (claimableOrders.Count == 0)
            {
                await tx.CommitAsync(ct);
                return new List<PendingOrderRecord>();
            }

            var claimedOrders = new List<PendingOrderRecord>();

            //Mark as Processing immediately and update OWNERSHIP by updating executionId
            foreach (var order in claimableOrders)
            {
                Guid execId = Guid.NewGuid();

                var rows = await _db.Orders
                .Where(o => o.Id == order.Id &&
                    (
                        o.Status == OrderStatus.Pending ||
                        (o.Status == OrderStatus.Processing && o.ProcessingUntil < now)
                    ))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.Status, OrderStatus.Processing)
                    .SetProperty(o => o.ProcessedAt, now)
                    .SetProperty(o => o.ProcessingUntil, leaseUntil)
                    .SetProperty(o => o.ExecutionId, execId)
                    , ct);

                //if rows > 0 means success
                if (rows > 0) claimedOrders.Add(new PendingOrderRecord(order.Id, order.CreatedAt, execId));
            }


            await tx.CommitAsync(ct);

            return claimedOrders;
        }

        public async Task<bool> TryCancelAsync(Guid id, CancellationToken ct)
        {
            var rows = await _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE ""Orders""
                SET ""Status"" = {(int)OrderStatus.Cancelled}
                WHERE ""Id"" = {id}
                  AND ""Status"" = {(int)OrderStatus.Processing}");

            return rows == 1;
        }

        public async Task<bool> TryCompleteAsync(Guid id, Guid executionId, DateTime now, CancellationToken ct)
        {
            var rows = await _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Orders""
            SET ""Status"" = {(int)OrderStatus.Completed},
                ""ProcessingUntil"" = NULL,
                ""ExecutionId"" =  NULL
            WHERE ""Id"" = {id}
              AND ""Status"" = {(int)OrderStatus.Processing}
              AND ""ExecutionId"" = {executionId}
              AND ""ExpiredAt"" > {now}");

            return rows == 1;
        }

        public async Task<int> ExpireProcessingOrderAsync(DateTime now, CancellationToken ct)
        {
            //LINQ order > Get order where status is processing & processedat != null (not pending) & processedat < threshold (expired, with 5 minutes threshold/grace period)
            //After above query executed, get the list of the orders, then update all the orders to expired
            var rows = await _db.Orders
                .Where(o => (o.Status == OrderStatus.Processing || o.Status == OrderStatus.Pending) &&
                            o.ExpiredAt < now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(o => o.Status, OrderStatus.Expired)
                    .SetProperty(o => o.ExecutionId, (Guid?)null)
                    .SetProperty(o => o.ProcessingUntil, (DateTime?)null),
                    ct);

            return rows;
        }

        public async Task<int> GetProcessingOrderCountAsync(CancellationToken ct)
        {
            return await _db.Orders.Where(x => x.Status == OrderStatus.Processing).CountAsync();
        }
    }
}
