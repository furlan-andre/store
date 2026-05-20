using Store.Application.Common.Results;

namespace Store.Application.Orders;

public interface IOrderService
{
    Task<Result<OrderResponse>> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<OrderResponse>>> GetAllAsync(CancellationToken cancellationToken);
}
