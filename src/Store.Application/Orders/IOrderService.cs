using Store.Application.Common.Results;
using Store.Application.Common.Pagination;

namespace Store.Application.Orders;

public interface IOrderService
{
    Task<Result<OrderResponse>> CancelAsync(long id, CancellationToken cancellationToken);
    Task<Result<OrderResponse>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<Result<OrderResponse>> ConfirmAsync(long id, CancellationToken cancellationToken);
    Task<Result<OrderResponse>> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<Result<PagedResponse<OrderResponse>>> GetAllAsync(ListOrdersRequest request, CancellationToken cancellationToken);
}
