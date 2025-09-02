using FluentValidation;
using Vitura.API.Services;

namespace Vitura.API.Validation;

public class OrderValidator : IOrderValidator
{
    private readonly IValidator<OrderQueryParams> _validator;

    public OrderValidator(IValidator<OrderQueryParams> validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public ValidationResult Validate(OrderQueryParams query)
    {
        var result = _validator.Validate(query);
        return result.ToCustomValidationResult();
    }

    public async Task<ValidationResult> ValidateAsync(OrderQueryParams query)
    {
        var result = await _validator.ValidateAsync(query);
        return result.ToCustomValidationResult();
    }
}