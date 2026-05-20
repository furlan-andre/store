using FluentAssertions;
using Moq;
using Store.Application.Common.Results;
using Store.Application.Customers;
using Store.Application.Orders;
using Store.Application.Products;
using Store.Domain.Customers;
using Store.Domain.Orders;
using Store.Domain.Products;

namespace Store.Tests.Application.Orders;

public sealed class OrderServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderService = new OrderService(
            _orderRepository.Object,
            _customerRepository.Object,
            _productRepository.Object,
            new CreateOrderRequestValidator());
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        var request = CreateOrderRequest();
        SetupCustomerExists();
        SetupProductExists();

        var result = await _orderService.CreateAsync(request, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Should().BeEquivalentTo(new OrderResponse
        {
            Id = 0,
            CustomerId = 1,
            Status = OrderStatus.Placed,
            Total = 300m,
            Currency = "BRL",
            CreatedAt = result.Value.CreatedAt,
            ConfirmedAt = null,
            CancelledAt = null,
            Items =
            [
                new OrderItemResponse
                {
                    Id = 0,
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 150m,
                    Subtotal = 300m
                }
            ]
        });
        
        _orderRepository.Verify(
            repository => repository.AddAsync(
                It.Is<Order>(order =>
                    order.CustomerId == 1 &&
                    order.Status == OrderStatus.Placed &&
                    order.Total == 300m &&
                    order.Items.Single().ProductId == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnValidation_WhenItemsAreEmpty()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Currency = "BRL",
            Items = []
        };

        var result = await _orderService.CreateAsync(request, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Validation);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "order.items.required",
            Message = "Order must have at least one item."
        });
        
        _orderRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnValidation_WhenItemQuantityIsZero()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = 1,
            Currency = "BRL",
            Items =
            [
                new CreateOrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 0
                }
            ]
        };

        var result = await _orderService.CreateAsync(request, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Validation);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "order_item.quantity.invalid",
            Message = "Quantity must be greater than zero."
        });
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        SetupCustomerDoesNotExist();

        var result = await _orderService.CreateAsync(CreateOrderRequest(), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "customer.not_found",
            Message = "Customer not found."
        });
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        SetupCustomerExists();
        SetupProductDoesNotExist();

        var result = await _orderService.CreateAsync(CreateOrderRequest(), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "product.not_found",
            Message = "Product not found."
        });
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnBusinessRule_WhenStockIsInsufficient()
    {
        SetupCustomerExists();
        SetupProductExists(availableQuantity: 1);

        var result = await _orderService.CreateAsync(CreateOrderRequest(), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.BusinessRule);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "product.stock.insufficient",
            Message = "Product stock is insufficient."
        });
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
                    Quantity = 2
                }
            ]
        };
    }

    private void SetupCustomerExists()
    {
        _customerRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer("Acme"));
    }

    private void SetupCustomerDoesNotExist()
    {
        _customerRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);
    }

    private void SetupProductExists(long availableQuantity = 10)
    {
        _productRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product("Keyboard", 150m, availableQuantity));
    }

    private void SetupProductDoesNotExist()
    {
        _productRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);
    }
}
