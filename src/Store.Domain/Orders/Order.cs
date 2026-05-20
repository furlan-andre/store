namespace Store.Domain.Orders;

public sealed class Order
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Total { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    private readonly List<OrderItem> _items = [];
    
    private Order() { }

    public Order(long customerId, string currency, IEnumerable<OrderItem> items)
    {
        ValidateCustomerId(customerId);
        ValidateCurrency(currency);

        var orderItems = items.ToList();
        ValidateItems(orderItems);

        CustomerId = customerId;
        Currency = currency.Trim();
        Status = OrderStatus.Placed;
        CreatedAt = DateTimeOffset.UtcNow;
        _items.AddRange(orderItems);
        Total = CalculateTotal();
    }

    private decimal CalculateTotal()
    {
        return _items.Sum(item => item.Subtotal);
    }

    private static void ValidateCustomerId(long customerId)
    {
        if (customerId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(customerId), "Customer id must be greater than zero.");
        }
    }

    private static void ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }
    }

    private static void ValidateItems(IReadOnlyCollection<OrderItem> items)
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("Order must have at least one item.", nameof(items));
        }
    }
}
