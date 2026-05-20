using Store.Domain.Customers;

namespace Store.Application.Customers;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken);
}
