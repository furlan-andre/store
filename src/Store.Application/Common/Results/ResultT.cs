namespace Store.Application.Common.Results;

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(ResultStatus.Success, Array.Empty<ResultError>())
    {
        _value = value;
    }

    private Result(ResultStatus status, IReadOnlyCollection<ResultError> errors) : base(status, errors)
    {
    }

    public T Value
    {
        get
        {
            if (!IsSuccess)
            {
                throw new InvalidOperationException("Cannot access the value of a failed result.");
            }

            return _value!;
        }
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public new static Result<T> Validation(params ResultError[] errors)
    {
        return Failure(ResultStatus.Validation, errors);
    }

    public new static Result<T> NotFound(params ResultError[] errors)
    {
        return Failure(ResultStatus.NotFound, errors);
    }

    public new static Result<T> Conflict(params ResultError[] errors)
    {
        return Failure(ResultStatus.Conflict, errors);
    }

    public new static Result<T> BusinessRule(params ResultError[] errors)
    {
        return Failure(ResultStatus.BusinessRule, errors);
    }

    private static Result<T> Failure(ResultStatus status, ResultError[] errors)
    {
        return new Result<T>(status, NormalizeErrors(errors));
    }
}
