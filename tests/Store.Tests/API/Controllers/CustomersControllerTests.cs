using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Controllers;
using Store.Application.Common.Results;
using Store.Application.Customers;

namespace Store.Tests.API.Controllers;

public sealed class CustomersControllerTests
{
    private readonly Mock<ICustomerService> _customerService = new();
    private readonly CustomersController _controller;

    public CustomersControllerTests()
    {
        _controller = new CustomersController(_customerService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenCustomersAreListed()
    {
        var customers = new List<CustomerResponse>
        {
            new()
            {
                Id = 1,
                Name = "Acme"
            }
        };
        _customerService
            .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<CustomerResponse>>.Success(customers));

        var actionResult = await _controller.GetAll(CancellationToken.None);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        
        okResult.Value.Should().BeEquivalentTo(customers);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenCustomerExists()
    {
        var customer = new CustomerResponse
        {
            Id = 1,
            Name = "Acme"
        };
        _customerService
            .Setup(service => service.GetByIdAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CustomerResponse>.Success(customer));

        var actionResult = await _controller.GetById(1, CancellationToken.None);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(customer);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        var error = ResultError.Create("customer.not_found", "Customer not found.");
        _customerService
            .Setup(service => service.GetByIdAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CustomerResponse>.NotFound(error));

        var actionResult = await _controller.GetById(1, CancellationToken.None);

        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }
}
