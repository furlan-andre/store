using Store.Domain.Orders;

namespace Store.Application.Orders;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken);
}
