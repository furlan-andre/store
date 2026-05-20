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

    [Fact]
    public void Confirm_ShouldSetStatusToConfirmed_WhenOrderIsPlaced()
    {
        var order = CreateOrder();

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_ShouldSetConfirmedAt_WhenOrderIsPlaced()
    {
        var order = CreateOrder();

        order.Confirm();

        order.ConfirmedAt.Should().NotBeNull();
        order.ConfirmedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Confirm_ShouldKeepConfirmedAt_WhenOrderIsAlreadyConfirmed()
    {
        var order = CreateOrder();
        order.Confirm();
        var confirmedAt = order.ConfirmedAt;

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().Be(confirmedAt);
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenOrderIsCanceled()
    {
        var order = CreateOrder();
        order.Cancel();

        var act = order.Confirm;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Canceled order cannot be confirmed.");
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCanceled()
    {
        var order = CreateOrder();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Canceled);
    }

    [Fact]
    public void Cancel_ShouldSetCancelledAt()
    {
        var order = CreateOrder();

        order.Cancel();

        order.CancelledAt.Should().NotBeNull();
        order.CancelledAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Cancel_ShouldKeepCancelledAt_WhenOrderIsAlreadyCanceled()
    {
        var order = CreateOrder();
        order.Cancel();
        var cancelledAt = order.CancelledAt;

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Canceled);
        order.CancelledAt.Should().Be(cancelledAt);
    }

    private static Order CreateOrder()
    {
        return new Order(1, "BRL", [new OrderItem(1, 1, 10m)]);
    }
}
