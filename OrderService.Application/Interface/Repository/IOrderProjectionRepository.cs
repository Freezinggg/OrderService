using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interface.Repository
{
    public interface IOrderProjectionRepository
    {
        Task<OrderProjection?> GetByIdAsync(Guid id, CancellationToken ct);
        Task AddAsync(OrderProjection orderProjection, CancellationToken ct);
        Task UpdateStatus(OrderProjection orderProjection, CancellationToken ct);
        Task DeleteAllAsync(CancellationToken ct);
    }
}
