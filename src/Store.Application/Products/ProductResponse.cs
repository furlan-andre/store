namespace Store.Application.Products;

public sealed record ProductResponse
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public long AvailableQuantity { get; init; }
}
