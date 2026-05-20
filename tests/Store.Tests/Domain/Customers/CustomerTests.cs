using FluentAssertions;
using Store.Domain.Customers;

namespace Store.Tests.Domain.Customers;

public sealed class CustomerTests
{
    [Fact]
    public void Constructor_ShouldCreateCustomer_WhenNameIsValid()
    {
        var customer = new Customer("Acme");

        customer.Name.Should().Be("Acme");
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        var customer = new Customer("  Acme  ");

        customer.Name.Should().Be("Acme");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsEmpty()
    {
        var act = () => new Customer(string.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameIsWhitespace()
    {
        var act = () => new Customer("   ");

        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required.*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNameExceedsMaxLength()
    {
        var name = new string('A', Customer.NameMaxLength + 1);

        var act = () => new Customer(name);

        act.Should().Throw<ArgumentException>()
            .WithMessage($"Name cannot exceed {Customer.NameMaxLength} characters.*")
            .And.ParamName.Should().Be("name");
    }
}
