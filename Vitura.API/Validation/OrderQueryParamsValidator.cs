using FluentValidation;
using Vitura.API.Services;
using Vitura.Models.Enums;

public class OrderQueryParamsValidator : AbstractValidator<OrderQueryParams>
{
    private static readonly string[] ValidSorts = ["createdat", "totalcents"];
    private static readonly string[] ValidDirections = ["asc", "desc"];

    public OrderQueryParamsValidator()
    {
        RuleFor(x => x.PharmacyId)
            .NotEmpty()
            .WithMessage("Pharmacy ID cannot be empty")
            .Length(3, 50)
            .WithMessage("Pharmacy ID must be between 3 and 50 characters")
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Pharmacy ID must contain only alphanumeric characters, hyphens, and underscores")
            .When(x => !string.IsNullOrEmpty(x.PharmacyId));

        RuleFor(x => x.Statuses)
            .Must(BeValidStatuses)
            .WithMessage($"Status must be one of: {string.Join(", ", Enum.GetNames<OrderStatus>())}")
            .When(x => x.Statuses != null);

        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("From date must be less than or equal to To date")
            .When(x => x.From.HasValue && x.To.HasValue);

        RuleFor(x => x.To)
            .Must(date => BeReasonableDate(date!.Value))
            .WithMessage("To date must be a reasonable date")
            .When(x => x.To.HasValue);

        RuleFor(x => x.From)
            .Must(date => BeReasonableDate(date!.Value))
            .WithMessage("From date must be a reasonable date")
            .When(x => x.From.HasValue);

        RuleFor(x => x.Sort)
            .Must(sort => ValidSorts.Contains(sort.ToLower()))
            .WithMessage($"Sort must be one of: {string.Join(", ", ValidSorts)}");

        RuleFor(x => x.Direction)
            .Must(direction => ValidDirections.Contains(direction.ToLower()))
            .WithMessage($"Direction must be one of: {string.Join(", ", ValidDirections)}");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");
    }

    private static bool BeValidStatuses(string[]? statuses)
    {
        if (statuses == null) return true;

        return statuses.All(status =>
            Enum.TryParse<OrderStatus>(status, ignoreCase: true, out _));
    }

    private static bool BeReasonableDate(DateTime date)
    {
        return date.Year >= 1900 && date.Year <= DateTime.UtcNow.Year + 10;
    }
}