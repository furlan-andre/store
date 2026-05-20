using FluentAssertions;
using Moq;
using Store.Application.Common.Pagination;
using Store.Application.Common.Persistence;
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
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUnitOfWorkTransaction> _transaction = new();
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transaction.Object);

        _orderService = new OrderService(
            _orderRepository.Object,
            _customerRepository.Object,
            _productRepository.Object,
            _unitOfWork.Object,
            new CreateOrderRequestValidator(),
            new ListOrdersRequestValidator());
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnSuccessWithoutReturningStock_WhenOrderIsPlaced()
    {
        SetupOrderForUpdate(CreateOrder());

        var result = await _orderService.CancelAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Status.Should().Be(OrderStatus.Canceled);
        result.Value.CancelledAt.Should().NotBeNull();
        
        _productRepository.Verify(
            repository => repository.IncreaseStockAsync(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        
        _orderRepository.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        
        _transaction.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnSuccessAndReturnStock_WhenOrderIsConfirmed()
    {
        var order = CreateOrder();
        order.Confirm();
        SetupOrderForUpdate(order);

        var result = await _orderService.CancelAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Status.Should().Be(OrderStatus.Canceled);
        
        _productRepository.Verify(
            repository => repository.IncreaseStockAsync(1, 2, It.IsAny<CancellationToken>()),
            Times.Once);
        
        _orderRepository.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        
        _transaction.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        SetupOrderForUpdate(null);

        var result = await _orderService.CancelAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "order.not_found",
            Message = "Order not found."
        });
        
        _transaction.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnSuccessWithoutReturningStock_WhenOrderIsAlreadyCanceled()
    {
        var order = CreateOrder();
        order.Cancel();
        SetupOrderForUpdate(order);

        var result = await _orderService.CancelAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Status.Should().Be(OrderStatus.Canceled);
        
        _productRepository.Verify(
            repository => repository.IncreaseStockAsync(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        
        _orderRepository.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        
        _transaction.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
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
    public async Task ConfirmAsync_ShouldReturnSuccess_WhenOrderIsPlaced()
    {
        var order = CreateOrder();
        SetupOrderForUpdate(order);
        SetupStockDecreaseSucceeds();

        var result = await _orderService.ConfirmAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Status.Should().Be(OrderStatus.Confirmed);
        result.Value.ConfirmedAt.Should().NotBeNull();
        
        _productRepository.Verify(
            repository => repository.DecreaseStockAsync(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _orderRepository.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        
        _transaction.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmAsync_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        SetupOrderForUpdate(null);

        var result = await _orderService.ConfirmAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "order.not_found",
            Message = "Order not found."
        });
        
        _transaction.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmAsync_ShouldReturnSuccessWithoutDecreasingStock_WhenOrderIsAlreadyConfirmed()
    {
        var order = CreateOrder();
        order.Confirm();
        SetupOrderForUpdate(order);

        var result = await _orderService.ConfirmAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Status.Should().Be(OrderStatus.Confirmed);
        
        _productRepository.Verify(
            repository => repository.DecreaseStockAsync(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        
        _transaction.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmAsync_ShouldReturnBusinessRule_WhenOrderIsCanceled()
    {
        var order = CreateOrder();
        order.Cancel();
        SetupOrderForUpdate(order);

        var result = await _orderService.ConfirmAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.BusinessRule);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "order.canceled",
            Message = "Canceled order cannot be confirmed."
        });
        
        _transaction.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmAsync_ShouldReturnConflict_WhenStockDecreaseFails()
    {
        SetupOrderForUpdate(CreateOrder());
        SetupStockDecreaseFails();

        var result = await _orderService.ConfirmAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Conflict);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "product.stock.conflict",
            Message = "Product stock could not be decreased."
        });
        
        _orderRepository.Verify(
            repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        
        _transaction.Verify(
            transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
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
        var request = CreateListOrdersRequest();
        var orders = new List<Order>
        {
            new(1, "BRL", [new OrderItem(1, 1, 10m)]),
            new(2, "USD", [new OrderItem(2, 2, 5m)])
        };
        _orderRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ListOrdersRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePagedOrders(orders));

        var result = await _orderService.GetAllAsync(request, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalItems.Should().Be(2);
        result.Value.TotalPages.Should().Be(1);
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items[0].Should().BeEquivalentTo(new OrderResponse
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
    public async Task GetAllAsync_ShouldSendFiltersToRepository_WhenFiltersAreProvided()
    {
        var createdFrom = DateTimeOffset.UtcNow.AddDays(-7);
        var createdTo = DateTimeOffset.UtcNow;
        var cancelledFrom = DateTimeOffset.UtcNow.AddDays(-3);
        var cancelledTo = DateTimeOffset.UtcNow.AddDays(-1);
        var request = CreateListOrdersRequest() with
        {
            CustomerId = 10,
            Status = OrderStatus.Canceled,
            CreatedFrom = createdFrom,
            CreatedTo = createdTo,
            CancelledFrom = cancelledFrom,
            CancelledTo = cancelledTo
        };

        _orderRepository
            .Setup(repository => repository.GetAllAsync(
                It.Is<ListOrdersRequest>(repositoryRequest =>
                    repositoryRequest.Page == request.Page &&
                    repositoryRequest.PageSize == request.PageSize &&
                    repositoryRequest.CustomerId == request.CustomerId &&
                    repositoryRequest.Status == request.Status &&
                    repositoryRequest.CreatedFrom == request.CreatedFrom &&
                    repositoryRequest.CreatedTo == request.CreatedTo &&
                    repositoryRequest.CancelledFrom == request.CancelledFrom &&
                    repositoryRequest.CancelledTo == request.CancelledTo),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePagedOrders([]));

        var result = await _orderService.GetAllAsync(request, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        _orderRepository.Verify(
            repository => repository.GetAllAsync(
                It.IsAny<ListOrdersRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSuccessWithEmptyList_WhenOrdersDoNotExist()
    {
        var request = CreateListOrdersRequest();
        _orderRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ListOrdersRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePagedOrders([]));

        var result = await _orderService.GetAllAsync(request, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnValidation_WhenPageIsInvalid()
    {
        var request = CreateListOrdersRequest() with { Page = 0 };

        var result = await _orderService.GetAllAsync(request, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Validation);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "orders.page.invalid",
            Message = "Page must be greater than zero."
        });
        
        _orderRepository.Verify(
            repository => repository.GetAllAsync(
                It.IsAny<ListOrdersRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnValidation_WhenPageSizeExceedsLimit()
    {
        var request = CreateListOrdersRequest() with { PageSize = 101 };

        var result = await _orderService.GetAllAsync(request, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Validation);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "orders.page_size.max",
            Message = "Page size cannot exceed 100."
        });
        
        _orderRepository.Verify(
            repository => repository.GetAllAsync(
                It.IsAny<ListOrdersRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
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

    private static Order CreateOrder()
    {
        return new Order(1, "BRL", [new OrderItem(1, 2, 10m)]);
    }

    private static ListOrdersRequest CreateListOrdersRequest()
    {
        return new ListOrdersRequest
        {
            Page = 1,
            PageSize = 20
        };
    }

    private static PagedResponse<Order> CreatePagedOrders(IReadOnlyList<Order> orders)
    {
        return new PagedResponse<Order>
        {
            Page = 1,
            PageSize = 20,
            TotalItems = orders.Count,
            TotalPages = orders.Count == 0 ? 0 : 1,
            Items = orders
        };
    }

    private void SetupOrderForUpdate(Order? order)
    {
        _orderRepository
            .Setup(repository => repository.GetByIdForUpdateAsync(
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
    }

    private void SetupStockDecreaseSucceeds()
    {
        _productRepository
            .Setup(repository => repository.DecreaseStockAsync(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupStockDecreaseFails()
    {
        _productRepository
            .Setup(repository => repository.DecreaseStockAsync(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
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
