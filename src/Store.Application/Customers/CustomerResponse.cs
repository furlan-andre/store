namespace Store.Application.Customers;

public sealed record CustomerResponse
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
