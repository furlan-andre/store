using FluentAssertions;
using Store.Domain.Orders;

namespace Store.Tests.Domain.Orders;

public sealed class OrderTests
{
    [Fact]
    public void Constructor_ShouldCreateOrder_WhenDataIsValid()
    {
        var items = new[]
        {
            new OrderItem(1, 2, 10m),
            new OrderItem(2, 1, 5.50m)
        };

        var order = new Order(1, "BRL", items);

        order.CustomerId.Should().Be(1);
        order.Currency.Should().Be("BRL");
        order.Status.Should().Be(OrderStatus.Placed);
        order.Total.Should().Be(25.50m);
        order.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        order.ConfirmedAt.Should().BeNull();
        order.CancelledAt.Should().BeNull();
        order.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void Constructor_ShouldTrimCurrency()
    {
        var order = new Order(1, "  BRL  ", [new OrderItem(1, 1, 10m)]);

        order.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCustomerIdIsZero()
    {
        var act = () => new Order(0, "BRL", [new OrderItem(1, 1, 10m)]);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Customer id must be greater than zero.*")
            .And.ParamName.Should().Be("customerId");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCurrencyIsEmpty()
    {
        var act = () => new Order(1, string.Empty, [new OrderItem(1, 1, 10m)]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Currency is required.*")
            .And.ParamName.Should().Be("currency");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCurrencyIsWhitespace()
    {
        var act = () => new Order(1, "   ", [new OrderItem(1, 1, 10m)]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Currency is required.*")
            .And.ParamName.Should().Be("currency");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenItemsIsEmpty()
    {
        var act = () => new Order(1, "BRL", []);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Order must have at least one item.*")
            .And.ParamName.Should().Be("items");
    }
}
