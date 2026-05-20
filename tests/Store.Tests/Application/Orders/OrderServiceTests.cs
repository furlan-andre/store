using FluentAssertions;
using Moq;
using Store.Application.Common.Results;
using Store.Application.Orders;
using Store.Domain.Orders;

namespace Store.Tests.Application.Orders;

public sealed class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderService = new OrderService(_orderRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSuccess_WhenOrderExists()
    {
        var order = new Order(1, "BRL", [new OrderItem(1, 2, 10m)]);
        _orderRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _orderService.GetByIdAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Should().BeEquivalentTo(new OrderResponse
        {
            Id = 0,
            CustomerId = 1,
            Status = OrderStatus.Placed,
            Total = 20m,
            Currency = "BRL",
            CreatedAt = order.CreatedAt,
            ConfirmedAt = null,
            CancelledAt = null,
            Items =
            [
                new OrderItemResponse
                {
                    Id = 0,
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 10m,
                    Subtotal = 20m
                }
            ]
        });
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        _orderRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _orderService.GetByIdAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "order.not_found",
            Message = "Order not found."
        });
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSuccessWithOrders_WhenOrdersExist()
    {
        var orders = new List<Order>
        {
            new(1, "BRL", [new OrderItem(1, 1, 10m)]),
            new(2, "USD", [new OrderItem(2, 2, 5m)])
        };
        _orderRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var result = await _orderService.GetAllAsync(CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Should().HaveCount(2);
        result.Value[0].Should().BeEquivalentTo(new OrderResponse
        {
            Id = 0,
            CustomerId = 1,
            Status = OrderStatus.Placed,
            Total = 10m,
            Currency = "BRL",
            CreatedAt = orders[0].CreatedAt,
            ConfirmedAt = null,
            CancelledAt = null,
            Items =
            [
                new OrderItemResponse
                {
                    Id = 0,
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 10m,
                    Subtotal = 10m
                }
            ]
        });
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSuccessWithEmptyList_WhenOrdersDoNotExist()
    {
        _orderRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _orderService.GetAllAsync(CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Should().BeEmpty();
    }
}
