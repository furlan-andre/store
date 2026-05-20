using Store.Application.Common.Results;

namespace Store.Application.Orders;

public interface IOrderService
{
    Task<Result<OrderResponse>> CancelAsync(long id, CancellationToken cancellationToken);
    Task<Result<OrderResponse>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<Result<OrderResponse>> ConfirmAsync(long id, CancellationToken cancellationToken);
    Task<Result<OrderResponse>> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<OrderResponse>>> GetAllAsync(CancellationToken cancellationToken);
}
