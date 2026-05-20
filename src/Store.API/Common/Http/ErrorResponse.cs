namespace Store.API.Common.Http;

public sealed record ErrorResponse
{
    public IReadOnlyCollection<ErrorDetail> Errors { get; init; } = Array.Empty<ErrorDetail>();
}
