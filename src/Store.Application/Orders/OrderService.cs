using Store.Application.Common.Results;
using Store.Domain.Orders;

namespace Store.Application.Orders;

public sealed class OrderService(IOrderRepository orderRepository) : IOrderService
{
    public async Task<Result<OrderResponse>> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return Result<OrderResponse>.NotFound(
                ResultError.Create("order.not_found", "Order not found."));
        }

        var result = MapToResponse(order);
        
        return Result<OrderResponse>.Success(result);
    }

    public async Task<Result<IReadOnlyList<OrderResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);

        var response = orders
            .Select(MapToResponse)
            .ToList();

        return Result<IReadOnlyList<OrderResponse>>.Success(response);
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
