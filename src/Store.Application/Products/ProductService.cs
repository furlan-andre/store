using Store.Application.Common.Results;
using Store.Domain.Products;

namespace Store.Application.Products;

public sealed class ProductService(IProductRepository productRepository) : IProductService
{
    public async Task<Result<ProductResponse>> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return Result<ProductResponse>.NotFound(
                ResultError.Create("product.not_found", "Product not found."));
        }

        var response = MapToResponse(product);

        return Result<ProductResponse>.Success(response);
    }

    public async Task<Result<IReadOnlyList<ProductResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        var response = products
            .Select(MapToResponse)
            .ToList();

        return Result<IReadOnlyList<ProductResponse>>.Success(response);
    }

    private static ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            UnitPrice = product.UnitPrice,
            AvailableQuantity = product.AvailableQuantity
        };
    }
}
