using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interface.Repository;
using OrderService.Application.Record;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence.Repository
{
    public class OutboxRepository : IOutboxEventRepository
    {
        private readonly OrderDbContext _db;

        public OutboxRepository(OrderDbContext db)
        {
            _db = db;
        }

        public Task AddAsync(OutboxEvent outboxEvent, CancellationToken ct)
        {
            _db.OutboxEvents.AddAsync(outboxEvent);
            return Task.CompletedTask;
        }

        public async Task<List<OutboxEventRecord>> ClaimOutboxEventsAsync(int batchSize, CancellationToken ct)
        {
            var claimableEvents = await _db.OutboxEvents
                .FromSqlInterpolated($@"
                        SELECT ""Id"", ""EventType"", ""Payload"", ""EventVersion"", ""CreatedAt""
                        FROM ""OutboxEvents""
                        WHERE ""ProcessedAt"" IS NULL
                        FOR UPDATE SKIP LOCKED
                        LIMIT {batchSize}")
                .Select(o => new OutboxEventRecord(o.Id, o.EventType, o.Payload, o.EventVersion, o.CreatedAt))
                .ToListAsync(ct);

            return claimableEvents;
        }

        public async Task MarkOutboxEventPublished(Guid outboxId, CancellationToken ct)
        {
            try
            {
                var outboxEvent = await _db.OutboxEvents.FindAsync(outboxId);
                outboxEvent.MarkAsProcessed();
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //log here
            }
        }

        public async Task<List<OutboxEventRecord>> GetAllAsync(CancellationToken ct)
        {
            return await _db.OutboxEvents
                .OrderBy(o => o.CreatedAt)
                .Select(o => new OutboxEventRecord(
                    o.Id,
                    o.EventType,
                    o.Payload,
                    o.EventVersion,
                    o.CreatedAt))
                .ToListAsync(ct);
        }
    }
}
