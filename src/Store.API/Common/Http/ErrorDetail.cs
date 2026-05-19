namespace Store.API.Common.Http;

public sealed record ErrorDetail
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
