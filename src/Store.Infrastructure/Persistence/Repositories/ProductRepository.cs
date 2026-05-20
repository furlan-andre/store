using Microsoft.EntityFrameworkCore;
using Store.Application.Products;
using Store.Domain.Products;

namespace Store.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(StoreDbContext dbContext) : IProductRepository
{
    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        await dbContext.Products.AddAsync(product, cancellationToken);
    }

    public async Task<bool> DecreaseStockAsync(
        long productId,
        long quantity,
        CancellationToken cancellationToken)
    {
        var affectedRows = await dbContext.Products
            .Where(product =>
                product.Id == productId &&
                product.AvailableQuantity >= quantity)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    product => product.AvailableQuantity,
                    product => product.AvailableQuantity - quantity),
                cancellationToken);

        return affectedRows == 1;
    }

    public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .OrderBy(product => product.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
