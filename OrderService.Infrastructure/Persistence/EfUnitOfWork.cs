using Microsoft.EntityFrameworkCore.Storage;
using OrderService.Application.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence
{
    public sealed class EfUnitOfWork : IUnitOfWork
    {
        private readonly OrderDbContext _db;
        private IDbContextTransaction? _tx;

        public EfUnitOfWork(OrderDbContext db)
        {
            _db = db;
        }

        public async Task BeginAsync(CancellationToken ct)
        {
            _tx = await _db.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitAsync(CancellationToken ct)
        {
            await _db.SaveChangesAsync(ct);
            await _tx!.CommitAsync(ct);
        }

        public async Task RollbackAsync(CancellationToken ct)
        {
            await _tx!.RollbackAsync(ct);
        }
    }
}
