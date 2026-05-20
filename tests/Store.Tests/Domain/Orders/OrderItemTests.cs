using FluentAssertions;
using Store.Domain.Orders;

namespace Store.Tests.Domain.Orders;

public sealed class OrderItemTests
{
    [Fact]
    public void Constructor_ShouldCreateOrderItem_WhenDataIsValid()
    {
        var item = new OrderItem(1, 2, 10.50m);

        item.ProductId.Should().Be(1);
        item.Quantity.Should().Be(2);
        item.UnitPrice.Should().Be(10.50m);
        item.Subtotal.Should().Be(21.00m);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProductIdIsZero()
    {
        var act = () => new OrderItem(0, 1, 10m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Product id must be greater than zero.*")
            .And.ParamName.Should().Be("productId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenQuantityIsZero()
    {
        var act = () => new OrderItem(1, 0, 10m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Quantity must be greater than zero.*")
            .And.ParamName.Should().Be("quantity");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitPriceIsNegative()
    {
        var act = () => new OrderItem(1, 1, -0.01m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Unit price cannot be negative.*")
            .And.ParamName.Should().Be("unitPrice");
    }
}
