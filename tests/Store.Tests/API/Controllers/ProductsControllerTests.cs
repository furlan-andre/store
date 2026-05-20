using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Controllers;
using Store.Application.Common.Results;
using Store.Application.Products;

namespace Store.Tests.API.Controllers;

public sealed class ProductsControllerTests
{
    private readonly Mock<IProductService> _productService = new();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new ProductsController(_productService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenProductsAreListed()
    {
        var products = new List<ProductResponse>
        {
            new()
            {
                Id = 1,
                Name = "Keyboard",
                UnitPrice = 150m,
                AvailableQuantity = 5
            }
        };
        
        _productService
            .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProductResponse>>.Success(products));

        var actionResult = await _controller.GetAll(CancellationToken.None);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        
        okResult.Value.Should().BeEquivalentTo(products);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenProductExists()
    {
        var product = new ProductResponse
        {
            Id = 1,
            Name = "Keyboard",
            UnitPrice = 150m,
            AvailableQuantity = 5
        };
        
        _productService
            .Setup(service => service.GetByIdAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductResponse>.Success(product));

        var actionResult = await _controller.GetById(1, CancellationToken.None);

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        
        okResult.Value.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var error = ResultError.Create("product.not_found", "Product not found.");
        
        _productService
            .Setup(service => service.GetByIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductResponse>.NotFound(error));

        var actionResult = await _controller.GetById(1, CancellationToken.None);

        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }
}
