using Microsoft.EntityFrameworkCore;
using Store.Application.Customers;
using Store.Domain.Customers;

namespace Store.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(StoreDbContext dbContext) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Customers
            .OrderBy(customer => customer.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
