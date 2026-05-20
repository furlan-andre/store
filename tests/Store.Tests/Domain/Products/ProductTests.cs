using FluentAssertions;
using Store.Domain.Products;

namespace Store.Tests.Domain.Products;

public sealed class ProductTests
{
    [Fact]
    public void Constructor_ShouldCreateProduct_WhenDataIsValid()
    {
        var product = new Product("Notebook", 3500.50m, 10);

        product.Name.Should().Be("Notebook");
        product.UnitPrice.Should().Be(3500.50m);
        product.AvailableQuantity.Should().Be(10);
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        var product = new Product("  Notebook  ", 3500.50m, 10);

        product.Name.Should().Be("Notebook");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsEmpty()
    {
        var act = () => new Product(string.Empty, 10m, 1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsWhitespace()
    {
        var act = () => new Product("   ", 10m, 1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameExceedsMaxLength()
    {
        var name = new string('A', Product.NameMaxLength + 1);

        var act = () => new Product(name, 10m, 1);

        act.Should().Throw<ArgumentException>()
            .WithMessage($"Name cannot exceed {Product.NameMaxLength} characters.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenUnitPriceIsNegative()
    {
        var act = () => new Product("Notebook", -0.01m, 1);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Unit price cannot be negative.*")
            .And.ParamName.Should().Be("unitPrice");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAvailableQuantityIsNegative()
    {
        var act = () => new Product("Notebook", 10m, -1);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Available quantity cannot be negative.*")
            .And.ParamName.Should().Be("availableQuantity");
    }

}
