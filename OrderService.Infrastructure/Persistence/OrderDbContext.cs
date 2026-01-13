using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Order>(entity =>
                {
                    entity.HasKey(x => x.Id);

                    entity.Property(x => x.CustomerId).IsRequired();
                    entity.Property(x => x.Status).IsRequired();
                    entity.Property(x => x.CreatedAt).IsRequired();
                    entity.Property(x => x.ExpiredAt).IsRequired();
                    entity.Property(x => x.IdempotencyKey).IsRequired();

                    entity.Ignore(x => x.Items);

                    entity.HasMany<OrderItem>("_items")
                          .WithOne()
                          .HasForeignKey(x => x.OrderId)
                          .OnDelete(DeleteBehavior.Cascade);

                    entity.Navigation("_items")
                          .UsePropertyAccessMode(PropertyAccessMode.Field);
                })
                .Entity<OrderItem>(entity =>
                {
                    entity.HasKey(x => x.Id);
                    entity.Property(x => x.Code).IsRequired();
                    entity.Property(x => x.Quantity).IsRequired();
                })
                .Entity<IdempotencyRecord>(entity =>
                {
                    entity.HasKey(x => x.Id);

                    entity.Property(x => x.Key).IsRequired();
                    entity.Property(x => x.OrderId).IsRequired();
                    entity.Property(x => x.CreatedAt).IsRequired();

                    entity.HasIndex(x => x.Key)
                          .IsUnique();

                });
        }
    }
}
