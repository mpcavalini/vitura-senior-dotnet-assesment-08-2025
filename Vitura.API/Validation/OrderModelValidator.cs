using FluentValidation;
using Vitura.API.Models;

namespace Vitura.API.Validation;

public class OrderModelValidator : AbstractValidator<Order>
{
    public OrderModelValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.PharmacyId)
            .NotEmpty()
            .WithMessage("Pharmacy ID is required")
            .Matches(@"^ph\d{3}$")
            .WithMessage("Pharmacy ID must be in format 'phXXX' where X is a digit");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid OrderStatus");

        RuleFor(x => x.CreatedAt)
            .NotEmpty()
            .WithMessage("Created date is required")
            .Must(date => date.Year >= 1900 && date.Year <= DateTime.UtcNow.Year + 1)
            .WithMessage("Created date must be a reasonable date");

        RuleFor(x => x.TotalCents)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total cents must be non-negative");

        RuleFor(x => x.ItemCount)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Item count must be at least 1");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Payment method must be a valid PaymentMethod");

        RuleFor(x => x.DeliveryType)
            .IsInEnum()
            .WithMessage("Delivery type must be a valid DeliveryType");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters");
    }
}