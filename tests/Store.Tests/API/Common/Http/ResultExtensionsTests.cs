using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.API.Common.Http;
using Store.Application.Common.Results;

namespace Store.Tests.API.Common.Http;

public sealed class ResultExtensionsTests
{
    [Fact]
    public void ToActionResult_ShouldReturnNoContent_WhenResultIsSuccessWithoutValue()
    {
        var result = Result.Success();

        var actionResult = result.ToActionResult();

        actionResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_ShouldReturnOk_WhenTypedResultIsSuccess()
    {
        var result = Result<string>.Success("created");

        var actionResult = result.ToActionResult();

        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be("created");
    }

    [Fact]
    public void ToCreatedActionResult_ShouldReturnCreated_WhenTypedResultIsSuccess()
    {
        var result = Result<long>.Success(10);

        var actionResult = result.ToCreatedActionResult("/orders/10");

        var createdResult = actionResult.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be("/orders/10");
        createdResult.Value.Should().Be(10);
    }

    [Theory]
    [InlineData(ResultStatus.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ResultStatus.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ResultStatus.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ResultStatus.BusinessRule, StatusCodes.Status422UnprocessableEntity)]
    public void ToActionResult_ShouldMapFailureStatusToHttpStatusCode(ResultStatus resultStatus, int expectedStatusCode)
    {
        var error = ResultError.Create("error", "Error.");
        var result = CreateFailureResult(resultStatus, error);

        var actionResult = result.ToActionResult();

        var objectResult = actionResult.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(expectedStatusCode);
        objectResult.Value.Should().BeEquivalentTo(new ErrorResponse
        {
            Errors =
            [
                new ErrorDetail
                {
                    Code = error.Code,
                    Message = error.Message
                }
            ]
        });
    }

    [Fact]
    public void ToActionResult_ShouldMapTypedFailureToHttpStatusCode()
    {
        var error = ResultError.Create("product.not_found", "Product not found.");
        var result = Result<string>.NotFound(error);

        var actionResult = result.ToActionResult();

        var objectResult = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
        objectResult.Value.Should().BeEquivalentTo(new ErrorResponse
        {
            Errors =
            [
                new ErrorDetail
                {
                    Code = error.Code,
                    Message = error.Message
                }
            ]
        });
    }

    private static Result CreateFailureResult(ResultStatus status, ResultError error)
    {
        return status switch
        {
            ResultStatus.Validation => Result.Validation(error),
            ResultStatus.NotFound => Result.NotFound(error),
            ResultStatus.Conflict => Result.Conflict(error),
            ResultStatus.BusinessRule => Result.BusinessRule(error),
            _ => throw new InvalidOperationException("Unsupported status.")
        };
    }
}
