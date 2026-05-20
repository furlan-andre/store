using Store.Domain.Orders;
using Store.Application.Common.Pagination;

namespace Store.Application.Orders;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<Order?> GetByIdForUpdateAsync(long id, CancellationToken cancellationToken);
    Task<PagedResponse<Order>> GetAllAsync(ListOrdersRequest request, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
