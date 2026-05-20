using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Domain.Customers;
using Store.Domain.Orders;

namespace Store.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(order => order.Id);

        builder.HasIndex(order => order.CustomerId)
            .HasDatabaseName("IX_orders_CustomerId");

        builder.HasIndex(order => order.Status)
            .HasDatabaseName("IX_orders_Status");

        builder.HasIndex(order => order.CreatedAt)
            .HasDatabaseName("IX_orders_CreatedAt");

        builder.HasIndex(order => order.CancelledAt)
            .HasDatabaseName("IX_orders_CancelledAt");

        builder.Property(order => order.Id)
            .ValueGeneratedOnAdd();

        builder.Property(order => order.CustomerId)
            .IsRequired();

        builder.Property(order => order.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(order => order.Total)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(order => order.Currency)
            .HasColumnType("varchar(10)")
            .IsRequired();

        builder.Property(order => order.CreatedAt)
            .IsRequired();

        builder.Property(order => order.ConfirmedAt);

        builder.Property(order => order.CancelledAt);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
