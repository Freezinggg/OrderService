using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence
{
    public class OrderDbContextFactory
     : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();

            // DESIGN-TIME connection string
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=order;Username=postgres;Password=password");

            return new OrderDbContext(optionsBuilder.Options);
        }
    }
}
