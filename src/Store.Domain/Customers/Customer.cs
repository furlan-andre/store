namespace Store.Domain.Customers;

public sealed class Customer
{
    public const int NameMaxLength = 150;

    public long Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Customer() { }

    public Customer(string name)
    {
        ValidateName(name);

        Name = name.Trim();
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
}
