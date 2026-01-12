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
        private static readonly List<Order> _orders = new();

        public Task AddAsync(Order order, CancellationToken ct)
        {
            _orders.Add(order);
            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult(_orders.FirstOrDefault(x => x.Id == id));
        }
    }
}
