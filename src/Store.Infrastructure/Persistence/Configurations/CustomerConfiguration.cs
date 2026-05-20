using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Domain.Customers;

namespace Store.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(customer => customer.Id);

        builder.HasIndex(customer => customer.Name)
            .HasDatabaseName("IX_customers_Name");

        builder.Property(customer => customer.Id)
            .ValueGeneratedOnAdd();

        builder.Property(customer => customer.Name)
            .HasColumnType($"varchar({Customer.NameMaxLength})")
            .HasMaxLength(Customer.NameMaxLength)
            .IsRequired();
    }
}
