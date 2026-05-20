namespace Store.Application.Common.Pagination;

public sealed record PagedResponse<T>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public long TotalItems { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<T> Items { get; init; } = [];
}
