using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Pagination;
using Store.Application.Orders;
using Store.Domain.Orders;

namespace Store.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(StoreDbContext dbContext) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task<Order?> GetByIdForUpdateAsync(long id, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task<PagedResponse<Order>> GetAllAsync(
        ListOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Orders
            .AsNoTracking()
            .AsQueryable();

        if (request.CustomerId.HasValue)
        {
            query = query.Where(order => order.CustomerId == request.CustomerId);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(order => order.Status == request.Status);
        }

        if (request.CreatedFrom.HasValue)
        {
            query = query.Where(order => order.CreatedAt >= request.CreatedFrom);
        }

        if (request.CreatedTo.HasValue)
        {
            query = query.Where(order => order.CreatedAt <= request.CreatedTo);
        }

        if (request.CancelledFrom.HasValue)
        {
            query = query.Where(order => order.CancelledAt >= request.CancelledFrom);
        }

        if (request.CancelledTo.HasValue)
        {
            query = query.Where(order => order.CancelledAt <= request.CancelledTo);
        }

        var totalItems = await query.LongCountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var items = await query
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Order>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = items
        };
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
