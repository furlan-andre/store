namespace Store.Domain.Orders;

public sealed class OrderItem
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public long ProductId { get; private set; }
    public long Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal => UnitPrice * Quantity;
    
    private OrderItem() { }

    public OrderItem(long productId, long quantity, decimal unitPrice)
    {
        ValidateProductId(productId);
        ValidateQuantity(quantity);
        ValidateUnitPrice(unitPrice);

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    private static void ValidateProductId(long productId)
    {
        if (productId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(productId), "Product id must be greater than zero.");
        }
    }

    private static void ValidateQuantity(long quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }
    }

    private static void ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        }
    }
}
