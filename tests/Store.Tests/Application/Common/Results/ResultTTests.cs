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

    [Theory]
    [InlineData(ResultStatus.Validation)]
    [InlineData(ResultStatus.NotFound)]
    [InlineData(ResultStatus.Conflict)]
    [InlineData(ResultStatus.BusinessRule)]
    public void FailureFactories_ShouldCreateTypedResultWithExpectedStatus(ResultStatus expectedStatus)
    {
        var error = ResultError.Create("error", "Error.");

        var result = expectedStatus switch
        {
            ResultStatus.Validation => Result<string>.Validation(error),
            ResultStatus.NotFound => Result<string>.NotFound(error),
            ResultStatus.Conflict => Result<string>.Conflict(error),
            ResultStatus.BusinessRule => Result<string>.BusinessRule(error),
            _ => throw new InvalidOperationException("Unsupported status.")
        };

        result.Status.Should().Be(expectedStatus);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }
}
