using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interface
{
    public interface IIdempotencyRepository
    {
        Task<Guid?> FindOrderIdAsync(string key, CancellationToken ct); 
        Task AddAsync(IdempotencyRecord idempotency, CancellationToken ct);
    }
}
