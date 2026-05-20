using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Domain.Orders;
using Store.Domain.Products;

namespace Store.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(item => item.Id);

        builder.HasIndex(item => item.OrderId)
            .HasDatabaseName("IX_order_items_OrderId");

        builder.Property(item => item.Id)
            .ValueGeneratedOnAdd();

        builder.Property(item => item.OrderId)
            .IsRequired();

        builder.Property(item => item.ProductId)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Ignore(item => item.Subtotal);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .HasConstraintName("FK_order_items_products_ProductId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
