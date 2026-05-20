namespace Store.Application.Common.Results;

public class Result
{
    protected Result(ResultStatus status, IReadOnlyCollection<ResultError> errors)
    {
        Status = status;
        Errors = errors;
    }

    public ResultStatus Status { get; }
    public IReadOnlyCollection<ResultError> Errors { get; }
    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result Success()
    {
        return new Result(ResultStatus.Success, Array.Empty<ResultError>());
    }

    public static Result Validation(params ResultError[] errors)
    {
        return Failure(ResultStatus.Validation, errors);
    }

    public static Result NotFound(params ResultError[] errors)
    {
        return Failure(ResultStatus.NotFound, errors);
    }

    public static Result Conflict(params ResultError[] errors)
    {
        return Failure(ResultStatus.Conflict, errors);
    }

    public static Result BusinessRule(params ResultError[] errors)
    {
        return Failure(ResultStatus.BusinessRule, errors);
    }

    protected static IReadOnlyCollection<ResultError> NormalizeErrors(ResultError[] errors)
    {
        return errors.Length == 0 ? Array.Empty<ResultError>() : errors;
    }

    private static Result Failure(ResultStatus status, ResultError[] errors)
    {
        return new Result(status, NormalizeErrors(errors));
    }
}
