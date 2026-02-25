using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Record
{
    public sealed record PendingOrderRecord(Guid id, DateTime createdAt);
}
