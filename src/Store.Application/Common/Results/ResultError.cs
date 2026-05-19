namespace Store.Application.Common.Results;

public sealed record ResultError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static ResultError Create(string code, string message)
    {
        return new ResultError
        {
            Code = code,
            Message = message
        };
    }
}
