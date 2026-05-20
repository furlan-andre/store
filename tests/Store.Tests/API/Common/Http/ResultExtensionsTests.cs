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

    [Fact]
    public void ToActionResult_ShouldReturnBadRequest_WhenResultIsValidation()
    {
        var error = ResultError.Create("error", "Error.");
        var result = Result.Validation(error);

        var actionResult = result.ToActionResult();

        var objectResult = actionResult.Should().BeOfType<BadRequestObjectResult>().Subject;
        objectResult.Value.Should().BeEquivalentTo(CreateErrorResponse(error));
    }

    [Fact]
    public void ToActionResult_ShouldReturnNotFound_WhenResultIsNotFound()
    {
        var error = ResultError.Create("error", "Error.");
        var result = Result.NotFound(error);

        var actionResult = result.ToActionResult();

        var objectResult = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
        objectResult.Value.Should().BeEquivalentTo(CreateErrorResponse(error));
    }

    [Fact]
    public void ToActionResult_ShouldReturnConflict_WhenResultIsConflict()
    {
        var error = ResultError.Create("error", "Error.");
        var result = Result.Conflict(error);

        var actionResult = result.ToActionResult();

        var objectResult = actionResult.Should().BeOfType<ConflictObjectResult>().Subject;
        objectResult.Value.Should().BeEquivalentTo(CreateErrorResponse(error));
    }

    [Fact]
    public void ToActionResult_ShouldReturnUnprocessableEntity_WhenResultIsBusinessRule()
    {
        var error = ResultError.Create("error", "Error.");
        var result = Result.BusinessRule(error);

        var actionResult = result.ToActionResult();

        var objectResult = actionResult.Should().BeOfType<UnprocessableEntityObjectResult>().Subject;
        objectResult.Value.Should().BeEquivalentTo(CreateErrorResponse(error));
    }

    [Fact]
    public void ToActionResult_ShouldMapTypedFailureToHttpStatusCode()
    {
        var error = ResultError.Create("product.not_found", "Product not found.");
        var result = Result<string>.NotFound(error);

        var actionResult = result.ToActionResult();

        var objectResult = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
        objectResult.Value.Should().BeEquivalentTo(CreateErrorResponse(error));
    }

    private static ErrorResponse CreateErrorResponse(ResultError error)
    {
        return new ErrorResponse
        {
            Errors =
            [
                new ErrorDetail
                {
                    Code = error.Code,
                    Message = error.Message
                }
            ]
        };
    }
}
