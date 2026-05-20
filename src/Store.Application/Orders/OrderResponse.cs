using Store.Domain.Orders;

namespace Store.Application.Orders;

public sealed record OrderResponse
{
    public long Id { get; init; }
    public long CustomerId { get; init; }
    public OrderStatus Status { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }
    public IReadOnlyList<OrderItemResponse> Items { get; init; } = [];
}
