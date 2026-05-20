using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Domain.Products;

namespace Store.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);

        builder.HasIndex(product => product.Name)
            .HasDatabaseName("IX_products_Name");

        builder.Property(product => product.Id)
            .ValueGeneratedOnAdd();

        builder.Property(product => product.Name)
            .HasColumnType($"varchar({Product.NameMaxLength})")
            .HasMaxLength(Product.NameMaxLength)
            .IsRequired();

        builder.Property(product => product.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(product => product.AvailableQuantity)
            .IsRequired();
    }
}
