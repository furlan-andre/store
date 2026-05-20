namespace Store.Application.Orders;

public sealed record CreateOrderItemRequest
{
    public long ProductId { get; init; }
    public long Quantity { get; init; }
}
