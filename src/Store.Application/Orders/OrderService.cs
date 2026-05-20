using FluentValidation;
using Store.Application.Common.Persistence;
using Store.Application.Common.Results;
using Store.Application.Customers;
using Store.Application.Products;
using Store.Domain.Orders;
using Store.Domain.Products;

namespace Store.Application.Orders;

public sealed class OrderService(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IValidator<CreateOrderRequest> createOrderRequestValidator) : IOrderService
{
    public async Task<Result<OrderResponse>> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var requestValidationResult = await ValidateCreateRequestAsync(request, cancellationToken);
        if (!requestValidationResult.IsSuccess)
        {
            return Result<OrderResponse>.Validation(requestValidationResult.Errors.ToArray());
        }

        var customerValidationResult = await ValidateCustomerExistsAsync(request.CustomerId, cancellationToken);
        if (!customerValidationResult.IsSuccess)
        {
            return Result<OrderResponse>.NotFound(customerValidationResult.Errors.ToArray());
        }

        var productsResult = await GetRequiredProductsAsync(request.Items, cancellationToken);
        if (!productsResult.IsSuccess)
        {
            return Result<OrderResponse>.NotFound(productsResult.Errors.ToArray());
        }

        var stockValidationResult = ValidateStockAvailability(request.Items, productsResult.Value);
        if (!stockValidationResult.IsSuccess)
        {
            return Result<OrderResponse>.BusinessRule(stockValidationResult.Errors.ToArray());
        }

        var orderItems = CreateOrderItems(request.Items, productsResult.Value);
        var order = new Order(request.CustomerId, request.Currency, orderItems);

        await orderRepository.AddAsync(order, cancellationToken);

        return Result<OrderResponse>.Success(MapToResponse(order));
    }

    public async Task<Result<OrderResponse>> ConfirmAsync(long id, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var order = await orderRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (order is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<OrderResponse>.NotFound(
                ResultError.Create("order.not_found", "Order not found."));
        }

        if (order.Status == OrderStatus.Canceled)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<OrderResponse>.BusinessRule(
                ResultError.Create("order.canceled", "Canceled order cannot be confirmed."));
        }
        
        if (order.Status == OrderStatus.Confirmed)
        {
            await transaction.CommitAsync(cancellationToken);
            return Result<OrderResponse>.Success(MapToResponse(order));
        }

        foreach (var item in order.Items)
        {
            var stockDecreased = await productRepository.DecreaseStockAsync(
                item.ProductId,
                item.Quantity,
                cancellationToken);
        
            if (!stockDecreased)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<OrderResponse>.Conflict(
                    ResultError.Create("product.stock.conflict", "Product stock could not be decreased."));
            }
        }
        
        order.Confirm();
        await orderRepository.SaveChangesAsync(cancellationToken);
        
        await transaction.CommitAsync(cancellationToken);

        return Result<OrderResponse>.Success(MapToResponse(order));
    }

    public async Task<Result<OrderResponse>> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return Result<OrderResponse>.NotFound(
                ResultError.Create("order.not_found", "Order not found."));
        }

        return Result<OrderResponse>.Success(MapToResponse(order));
    }

    public async Task<Result<IReadOnlyList<OrderResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);

        var response = orders
            .Select(MapToResponse)
            .ToList();

        return Result<IReadOnlyList<OrderResponse>>.Success(response);
    }

    private async Task<Result> ValidateCreateRequestAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await createOrderRequestValidator.ValidateAsync(request, cancellationToken);

        return validation.IsValid
            ? Result.Success()
            : Result.Validation(
                validation.Errors
                    .Select(error => ResultError.Create(error.ErrorCode, error.ErrorMessage))
                    .ToArray());
    }

    private async Task<Result> ValidateCustomerExistsAsync(long customerId, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);

        return customer is null
            ? Result.NotFound(ResultError.Create("customer.not_found", "Customer not found."))
            : Result.Success();
    }

    private async Task<Result<Dictionary<long, Product>>> GetRequiredProductsAsync(
        IReadOnlyList<CreateOrderItemRequest> items,
        CancellationToken cancellationToken)
    {
        var products = new Dictionary<long, Product>();

        foreach (var productId in items.Select(item => item.ProductId).Distinct())
        {
            var product = await productRepository.GetByIdAsync(productId, cancellationToken);
            if (product is null)
            {
                return Result<Dictionary<long, Product>>.NotFound(
                    ResultError.Create("product.not_found", "Product not found."));
            }

            products.Add(productId, product);
        }

        return Result<Dictionary<long, Product>>.Success(products);
    }

    private static Result ValidateStockAvailability(
        IReadOnlyList<CreateOrderItemRequest> items,
        IReadOnlyDictionary<long, Product> products)
    {
        foreach (var itemGroup in items.GroupBy(item => item.ProductId))
        {
            var requestedQuantity = itemGroup.Sum(item => item.Quantity);
            var product = products[itemGroup.Key];

            if (requestedQuantity > product.AvailableQuantity)
            {
                return Result.BusinessRule(
                    ResultError.Create("product.stock.insufficient", "Product stock is insufficient."));
            }
        }

        return Result.Success();
    }

    private static IReadOnlyList<OrderItem> CreateOrderItems(
        IReadOnlyList<CreateOrderItemRequest> items,
        IReadOnlyDictionary<long, Product> products)
    {
        return items
            .Select(item => new OrderItem(
                item.ProductId,
                item.Quantity,
                products[item.ProductId].UnitPrice))
            .ToList();
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            Total = order.Total,
            Currency = order.Currency,
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt,
            CancelledAt = order.CancelledAt,
            Items = order.Items
                .Select(MapToResponse)
                .ToList()
        };
    }

    private static OrderItemResponse MapToResponse(OrderItem item)
    {
        return new OrderItemResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            Subtotal = item.Subtotal
        };
    }
}
