using Store.Application.Common.Results;

namespace Store.Application.Customers;

public interface ICustomerService
{
    Task<Result<CustomerResponse>> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<CustomerResponse>>> GetAllAsync(CancellationToken cancellationToken);
}
