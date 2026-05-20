namespace Store.Application.Orders;

public sealed record CreateOrderRequest
{
    public long CustomerId { get; init; }
    public string Currency { get; init; } = string.Empty;
    public IReadOnlyList<CreateOrderItemRequest> Items { get; init; } = [];
}
