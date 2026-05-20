using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Controllers;
using Store.Application.Common.Pagination;
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
    public async Task Cancel_ShouldReturnOk_WhenOrderIsCanceled()
    {
        var order = CreateOrderResponse();
        _orderService
            .Setup(service => service.CancelAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Success(order));

        var actionResult = await _controller.Cancel(1, CancellationToken.None);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task Cancel_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        var error = ResultError.Create("order.not_found", "Order not found.");
        _orderService
            .Setup(service => service.CancelAsync(
                It.IsAny<long>(),  It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.NotFound(error));

        var actionResult = await _controller.Cancel(1, CancellationToken.None);

        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenOrderIsCreated()
    {
        var request = CreateOrderRequest();
        var order = CreateOrderResponse();
        
        _orderService
            .Setup(service => service.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Success(order));

        var actionResult = await _controller.Create(request, CancellationToken.None);

        var createdResult = actionResult.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be($"/orders/{order.Id}");
        createdResult.Value.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        var request = CreateOrderRequest();
        var error = ResultError.Create("order.items.required", "Order must have at least one item.");
        
        _orderService
            .Setup(service => service.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Validation(error));

        var actionResult = await _controller.Create(request, CancellationToken.None);

        actionResult.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnNotFound_WhenDependencyDoesNotExist()
    {
        var request = CreateOrderRequest();
        var error = ResultError.Create("product.not_found", "Product not found.");
        
        _orderService
            .Setup(service => service.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.NotFound(error));

        var actionResult = await _controller.Create(request, CancellationToken.None);

        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnUnprocessableEntity_WhenBusinessRuleFails()
    {
        var request = CreateOrderRequest();
        var error = ResultError.Create("product.stock.insufficient", "Product stock is insufficient.");
        
        _orderService
            .Setup(service => service.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.BusinessRule(error));

        var actionResult = await _controller.Create(request, CancellationToken.None);

        actionResult.Should().BeOfType<UnprocessableEntityObjectResult>();
    }

    [Fact]
    public async Task Confirm_ShouldReturnOk_WhenOrderIsConfirmed()
    {
        var order = CreateOrderResponse();
        _orderService
            .Setup(service => service.ConfirmAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Success(order));

        var actionResult = await _controller.Confirm(1, CancellationToken.None);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task Confirm_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        var error = ResultError.Create("order.not_found", "Order not found.");
        _orderService
            .Setup(service => service.ConfirmAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.NotFound(error));

        var actionResult = await _controller.Confirm(1, CancellationToken.None);

        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Confirm_ShouldReturnUnprocessableEntity_WhenOrderIsCanceled()
    {
        var error = ResultError.Create("order.canceled", "Canceled order cannot be confirmed.");
        _orderService
            .Setup(service => service.ConfirmAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.BusinessRule(error));

        var actionResult = await _controller.Confirm(1, CancellationToken.None);

        actionResult.Should().BeOfType<UnprocessableEntityObjectResult>();
    }

    [Fact]
    public async Task Confirm_ShouldReturnConflict_WhenStockCannotBeDecreased()
    {
        var error = ResultError.Create("product.stock.conflict", "Product stock could not be decreased.");
        _orderService
            .Setup(service => service.ConfirmAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponse>.Conflict(error));

        var actionResult = await _controller.Confirm(1, CancellationToken.None);

        actionResult.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenOrdersAreListed()
    {
        var request = CreateListOrdersRequest();
        var order = CreateOrderResponse();
        var orders = CreatePagedOrderResponse(order);

        _orderService
            .Setup(service => service.GetAllAsync(
                It.IsAny<ListOrdersRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResponse<OrderResponse>>.Success(orders));

        var actionResult = await _controller.GetAll(request, CancellationToken.None);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(orders);
    }

    [Fact]
    public async Task GetAll_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        var request = CreateListOrdersRequest() with { Page = 0 };
        var error = ResultError.Create("orders.page.invalid", "Page must be greater than zero.");

        _orderService
            .Setup(service => service.GetAllAsync(
                It.IsAny<ListOrdersRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResponse<OrderResponse>>.Validation(error));

        var actionResult = await _controller.GetAll(request, CancellationToken.None);

        actionResult.Should().BeOfType<BadRequestObjectResult>();
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

    private static CreateOrderRequest CreateOrderRequest()
    {
        return new CreateOrderRequest
        {
            CustomerId = 1,
            Currency = "BRL",
            Items =
            [
                new CreateOrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 1
                }
            ]
        };
    }

    private static ListOrdersRequest CreateListOrdersRequest()
    {
        return new ListOrdersRequest
        {
            Page = 1,
            PageSize = 20
        };
    }

    private static PagedResponse<OrderResponse> CreatePagedOrderResponse(OrderResponse order)
    {
        return new PagedResponse<OrderResponse>
        {
            Page = 1,
            PageSize = 20,
            TotalItems = 1,
            TotalPages = 1,
            Items = [order]
        };
    }
}
