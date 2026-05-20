using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Controllers;
using Store.Application.Common.Results;
using Store.Application.Orders;
using Store.Domain.Orders;

namespace Store.Tests.API.Controllers;

public sealed class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderService = new();
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _controller = new OrdersController(_orderService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenOrdersAreListed()
    {
        var order = CreateOrderResponse();
        var orders = new List<OrderResponse> { order };

        _orderService
            .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<OrderResponse>>.Success(orders));

        var actionResult = await _controller.GetAll(CancellationToken.None);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(orders);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenOrderExists()
    {
        var order = CreateOrderResponse();

        _orderService
            .Setup(service => service.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Success(order));

        var actionResult = await _controller.GetById(1, CancellationToken.None);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        var error = ResultError.Create("order.not_found", "Order not found.");
        _orderService
            .Setup(service => service.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.NotFound(error));

        var actionResult = await _controller.GetById(1, CancellationToken.None);

        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }

    private static OrderResponse CreateOrderResponse()
    {
        return new OrderResponse
        {
            Id = 1,
            CustomerId = 1,
            Status = OrderStatus.Placed,
            Total = 10m,
            Currency = "BRL",
            CreatedAt = DateTimeOffset.UtcNow,
            Items =
            [
                new OrderItemResponse
                {
                    Id = 1,
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 10m,
                    Subtotal = 10m
                }
            ]
        };
    }
}
