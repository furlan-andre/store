using FluentAssertions;
using Moq;
using Store.Application.Common.Results;
using Store.Application.Customers;
using Store.Domain.Customers;

namespace Store.Tests.Application.Customers;

public sealed class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _customerService = new CustomerService(_customerRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSuccess_WhenCustomerExists()
    {
        var customer = new Customer("Acme");
        
        _customerRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _customerService.GetByIdAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Should().BeEquivalentTo(new CustomerResponse
        {
            Id = 0,
            Name = "Acme"
        });
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        _customerRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var result = await _customerService.GetByIdAsync(1, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "customer.not_found",
            Message = "Customer not found."
        });
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSuccessWithCustomers_WhenCustomersExist()
    {
        var customers = new List<Customer>
        {
            new("Acme"),
            new("Contoso")
        };
        _customerRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        var result = await _customerService.GetAllAsync(CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Should().BeEquivalentTo([
            new CustomerResponse
            {
                Id = 0,
                Name = "Acme"
            },
            new CustomerResponse
            {
                Id = 0,
                Name = "Contoso"
            }
        ]);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSuccessWithEmptyList_WhenCustomersDoNotExist()
    {
        _customerRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _customerService.GetAllAsync(CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Should().BeEmpty();
    }
}
