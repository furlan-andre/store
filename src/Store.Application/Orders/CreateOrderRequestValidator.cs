using FluentValidation;

namespace Store.Application.Orders;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(request => request.CustomerId)
            .GreaterThan(0)
            .WithErrorCode("order.customer_id.invalid")
            .WithMessage("Customer id must be greater than zero.");

        RuleFor(request => request.Currency)
            .NotEmpty()
            .WithErrorCode("order.currency.required")
            .WithMessage("Currency is required.");

        RuleFor(request => request.Items)
            .NotEmpty()
            .WithErrorCode("order.items.required")
            .WithMessage("Order must have at least one item.");

        RuleForEach(request => request.Items)
            .ChildRules(item =>
            {
                item.RuleFor(orderItem => orderItem.ProductId)
                    .GreaterThan(0)
                    .WithErrorCode("order_item.product_id.invalid")
                    .WithMessage("Product id must be greater than zero.");

                item.RuleFor(orderItem => orderItem.Quantity)
                    .GreaterThan(0)
                    .WithErrorCode("order_item.quantity.invalid")
                    .WithMessage("Quantity must be greater than zero.");
            });
    }
}
