using OrderService.Application.Record;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interface.Repository
{
    public interface IOutboxEventRepository
    {
        Task MarkOutboxEventProcessed(Guid outboxId, CancellationToken ct);
        Task AddAsync(OutboxEvent outboxEvent, CancellationToken ct);
        Task<List<OutboxEventRecord>> ClaimOutboxEventsAsync(int batchSize, CancellationToken ct);
    }
}
