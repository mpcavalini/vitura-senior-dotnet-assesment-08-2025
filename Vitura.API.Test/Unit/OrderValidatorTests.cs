using FluentAssertions;
using Vitura.API.Services;
using Vitura.API.Validation;
using Xunit;

public class OrderValidatorTests
{
    private readonly OrderValidator _validator;

    public OrderValidatorTests()
    {
        // Create the FluentValidation validator that OrderValidator depends on
        var fluentValidator = new OrderQueryParamsValidator();
        _validator = new OrderValidator(fluentValidator);
    }

    [Theory]
    [InlineData("INVALID!@#", null)]
    [InlineData(null, new[] { "badstatus" })]
    public void Validate_InvalidInputs_ReturnsErrors(string pharmacyId, string[] statuses)
    {
        // Arrange
        var query = new OrderQueryParams(PharmacyId: pharmacyId, Statuses: statuses);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_ValidInputs_ReturnsValid()
    {
        // Arrange
        var query = new OrderQueryParams(PharmacyId: "ph001", Statuses: new[] { "shipped" });

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}