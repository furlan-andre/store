namespace Store.Application.Common.Results;

public enum ResultStatus
{
    Success = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    BusinessRule = 4
}
