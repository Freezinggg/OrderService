using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Record
{
    public sealed record OutboxEventRecord(Guid Id, EventType EventType, string Payload, DateTime CreatedAt);
}
