namespace Store.Domain.Products;

public sealed class Product
{
    public const int NameMaxLength = 150;
    
    public long Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public long AvailableQuantity { get; private set; }
 
    private Product() { }
    
    public Product(string name, decimal unitPrice, long availableQuantity)
    {
        ValidateName(name);
        ValidateUnitPrice(unitPrice);
        ValidateAvailableQuantity(availableQuantity);

        Name = name.Trim();
        UnitPrice = unitPrice;
        AvailableQuantity = availableQuantity;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        if (name.Trim().Length > NameMaxLength)
        {
            throw new ArgumentException($"Name cannot exceed {NameMaxLength} characters.", nameof(name));
        }
    }

    private static void ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        }
    }

    private static void ValidateAvailableQuantity(long availableQuantity)
    {
        if (availableQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableQuantity), "Available quantity cannot be negative.");
        }
    }
    
    
    public void IncreaseAvailableQuantity(long quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        AvailableQuantity += quantity;
    }

    public void DecreaseAvailableQuantity(long quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (quantity > AvailableQuantity)
        {
            throw new InvalidOperationException("Available quantity cannot be negative.");
        }

        AvailableQuantity -= quantity;
    }
}
