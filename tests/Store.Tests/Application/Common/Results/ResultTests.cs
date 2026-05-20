using FluentAssertions;
using Store.Application.Common.Results;

namespace Store.Tests.Application.Common.Results;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResultWithoutErrors()
    {
        var result = Result.Success();

        result.Status.Should().Be(ResultStatus.Success);
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validation_ShouldCreateFailedResultWithErrors()
    {
        var error = ResultError.Create("customer.name.required", "Customer name is required.");

        var result = Result.Validation(error);

        result.Status.Should().Be(ResultStatus.Validation);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void NotFound_ShouldCreateFailedResultWithNotFoundStatus()
    {
        var error = ResultError.Create("error", "Error.");

        var result = Result.NotFound(error);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void Conflict_ShouldCreateFailedResultWithConflictStatus()
    {
        var error = ResultError.Create("error", "Error.");

        var result = Result.Conflict(error);

        result.Status.Should().Be(ResultStatus.Conflict);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void BusinessRule_ShouldCreateFailedResultWithBusinessRuleStatus()
    {
        var error = ResultError.Create("error", "Error.");

        var result = Result.BusinessRule(error);

        result.Status.Should().Be(ResultStatus.BusinessRule);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }
}
