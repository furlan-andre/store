using Store.Application.Common.Results;

namespace Store.Application.Products;

public interface IProductService
{
    Task<Result<ProductResponse>> GetByIdAsync(long id, CancellationToken cancellationToken);
}
