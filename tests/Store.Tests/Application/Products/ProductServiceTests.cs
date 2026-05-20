using FluentAssertions;
using Moq;
using Store.Application.Common.Results;
using Store.Application.Products;
using Store.Domain.Products;

namespace Store.Tests.Application.Products;

public sealed class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productService = new ProductService(_productRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSuccessWithProduct_WhenProductExists()
    {
        var product = new Product("Notebook", 3500.50m, 10);
        _productRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _productService.GetByIdAsync(It.IsAny<long>(), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.Success);
        result.Value.Should().BeEquivalentTo(new ProductResponse
        {
            Id = 0,
            Name = "Notebook",
            UnitPrice = 3500.50m,
            AvailableQuantity = 10
        });
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var result = await _productService.GetByIdAsync(It.IsAny<long>(), CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Code = "product.not_found",
            Message = "Product not found."
        });
    }
}
