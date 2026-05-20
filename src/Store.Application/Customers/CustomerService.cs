using Store.Application.Common.Results;
using Store.Domain.Customers;

namespace Store.Application.Customers;

public sealed class CustomerService(ICustomerRepository customerRepository) : ICustomerService
{
    public async Task<Result<CustomerResponse>> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
        {
            return Result<CustomerResponse>.NotFound(
                ResultError.Create("customer.not_found", "Customer not found."));
        }

        var result = MapToResponse(customer); 
        
        return Result<CustomerResponse>.Success(result);
    }

    public async Task<Result<IReadOnlyList<CustomerResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var customers = await customerRepository.GetAllAsync(cancellationToken);

        var response = customers
            .Select(MapToResponse)
            .ToList();

        return Result<IReadOnlyList<CustomerResponse>>.Success(response);
    }

    private static CustomerResponse MapToResponse(Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            Name = customer.Name
        };
    }
}
