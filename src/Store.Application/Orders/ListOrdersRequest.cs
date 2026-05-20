using Store.Domain.Orders;

namespace Store.Application.Orders;

public sealed record ListOrdersRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public long? CustomerId { get; init; }
    public OrderStatus? Status { get; init; }
    public DateTimeOffset? CreatedFrom { get; init; }
    public DateTimeOffset? CreatedTo { get; init; }
    public DateTimeOffset? CancelledFrom { get; init; }
    public DateTimeOffset? CancelledTo { get; init; }
}
