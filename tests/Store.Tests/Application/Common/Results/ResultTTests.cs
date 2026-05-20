using FluentAssertions;
using Store.Application.Common.Results;

namespace Store.Tests.Application.Common.Results;

public sealed class ResultTTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResultWithValue()
    {
        var result = Result<string>.Success("created");

        result.Status.Should().Be(ResultStatus.Success);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("created");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Value_ShouldThrowWhenResultFailed()
    {
        var result = Result<string>.NotFound(ResultError.Create("order.not_found", "Order not found."));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access the value of a failed result.");
    }

    [Fact]
    public void Validation_ShouldCreateTypedFailedResultWithValidationStatus()
    {
        var error = ResultError.Create("error", "Error.");

        var result = Result<string>.Validation(error);

        result.Status.Should().Be(ResultStatus.Validation);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void NotFound_ShouldCreateTypedFailedResultWithNotFoundStatus()
    {
        var error = ResultError.Create("error", "Error.");

        var result = Result<string>.NotFound(error);

        result.Status.Should().Be(ResultStatus.NotFound);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void Conflict_ShouldCreateTypedFailedResultWithConflictStatus()
    {
        var error = ResultError.Create("error", "Error.");

        var result = Result<string>.Conflict(error);

        result.Status.Should().Be(ResultStatus.Conflict);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void BusinessRule_ShouldCreateTypedFailedResultWithBusinessRuleStatus()
    {
        var error = ResultError.Create("error", "Error.");

        var result = Result<string>.BusinessRule(error);

        result.Status.Should().Be(ResultStatus.BusinessRule);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }
}
