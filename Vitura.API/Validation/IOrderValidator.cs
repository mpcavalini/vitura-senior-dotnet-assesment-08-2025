using FluentValidation.Results;
using Vitura.API.Services;

namespace Vitura.API.Validation;

public record ValidationResult(bool IsValid, string[] Errors);

public interface IOrderValidator
{
    ValidationResult Validate(OrderQueryParams query);
    Task<ValidationResult> ValidateAsync(OrderQueryParams query);
}

public static class ValidationResultExtensions
{
    public static ValidationResult ToCustomValidationResult(this FluentValidation.Results.ValidationResult fluentResult)
    {
        return new ValidationResult(
            fluentResult.IsValid,
            fluentResult.Errors.Select(e => e.ErrorMessage).ToArray()
        );
    }
}