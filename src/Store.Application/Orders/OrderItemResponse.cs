namespace Store.Application.Orders;

public sealed record OrderItemResponse
{
    public long Id { get; init; }
    public long ProductId { get; init; }
    public long Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Subtotal { get; init; }
}
