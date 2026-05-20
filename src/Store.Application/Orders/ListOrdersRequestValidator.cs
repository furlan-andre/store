using FluentValidation;

namespace Store.Application.Orders;

public sealed class ListOrdersRequestValidator : AbstractValidator<ListOrdersRequest>
{
    public ListOrdersRequestValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThan(0)
            .WithErrorCode("orders.page.invalid")
            .WithMessage("Page must be greater than zero.");

        RuleFor(request => request.PageSize)
            .GreaterThan(0)
            .WithErrorCode("orders.page_size.invalid")
            .WithMessage("Page size must be greater than zero.")
            .LessThanOrEqualTo(100)
            .WithErrorCode("orders.page_size.max")
            .WithMessage("Page size cannot exceed 100.");

        RuleFor(request => request)
            .Must(request => !request.CreatedFrom.HasValue ||
                             !request.CreatedTo.HasValue ||
                             request.CreatedFrom <= request.CreatedTo)
            .WithErrorCode("orders.created_range.invalid")
            .WithMessage("Created from cannot be greater than created to.");

        RuleFor(request => request)
            .Must(request => !request.CancelledFrom.HasValue ||
                             !request.CancelledTo.HasValue ||
                             request.CancelledFrom <= request.CancelledTo)
            .WithErrorCode("orders.cancelled_range.invalid")
            .WithMessage("Cancelled from cannot be greater than cancelled to.");
    }
}
