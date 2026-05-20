using Microsoft.AspNetCore.Mvc;
using Store.Application.Common.Results;

namespace Store.API.Common.Http;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        return result.Status switch
        {
            ResultStatus.Success => new NoContentResult(),
            ResultStatus.Validation => new BadRequestObjectResult(ToErrorResponse(result)),
            ResultStatus.NotFound => new NotFoundObjectResult(ToErrorResponse(result)),
            ResultStatus.Conflict => new ConflictObjectResult(ToErrorResponse(result)),
            ResultStatus.BusinessRule => new UnprocessableEntityObjectResult(ToErrorResponse(result)),
            _ => new ObjectResult(ToErrorResponse(result)) { StatusCode = StatusCodes.Status500InternalServerError }
        };
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Success => new OkObjectResult(result.Value),
            _ => ((Result)result).ToActionResult()
        };
    }

    public static IActionResult ToCreatedActionResult<T>(
        this Result<T> result,
        string? location = null)
    {
        return result.Status switch
        {
            ResultStatus.Success => new CreatedResult(location, result.Value),
            _ => result.ToActionResult()
        };
    }

    private static ErrorResponse ToErrorResponse(Result result)
    {
        return new ErrorResponse
        {
            Errors = result.Errors
                .Select(error => new ErrorDetail
                {
                    Code = error.Code,
                    Message = error.Message
                })
                .ToArray()
        };
    }
}
