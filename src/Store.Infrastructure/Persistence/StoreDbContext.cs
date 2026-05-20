using Microsoft.EntityFrameworkCore;
using Store.Domain.Customers;
using Store.Domain.Orders;
using Store.Domain.Products;
using Store.Infrastructure.Persistence.Configurations;

namespace Store.Infrastructure.Persistence;

public sealed class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());

        RemoveOrderItemProductIdIndex(modelBuilder);
    }

    private static void RemoveOrderItemProductIdIndex(ModelBuilder modelBuilder)
    {
        var orderItemEntity = modelBuilder.Entity<OrderItem>().Metadata;
        var productIdProperty = orderItemEntity.FindProperty(nameof(OrderItem.ProductId));
        if (productIdProperty is null)
        {
            return;
        }

        var productIdIndex = orderItemEntity.FindIndex([productIdProperty]);
        if (productIdIndex is not null)
        {
            orderItemEntity.RemoveIndex(productIdIndex);
        }
    }
}
