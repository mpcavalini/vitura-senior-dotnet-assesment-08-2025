using FluentAssertions;
using Vitura.API.Mapping;
using Vitura.API.Models;
using Vitura.API.DTOs;
using Xunit;
using Vitura.API.Tests.Fixtures;

public class OrderMapperTests
{
    [Fact]
    public void ToDto_MapsOrderCorrectly()
    {
        // Arrange
        var order = TestDataBuilder.GetSampleOrders().First();
        var mapper = new OrderMapper();
        // Act
        var dto = mapper.ToDto(order, true);
        // Assert
        dto.Id.Should().Be(order.Id);
        dto.NeedsReview.Should().BeTrue();
    }

    [Fact]
    public void ToPagedResponse_MapsCorrectly()
    {
        // Arrange
        var orders = TestDataBuilder.GetSampleOrders();

        var dtos = orders.Select(o => new OrderResponseDto(
           o.Id,
           o.PharmacyId,
           o.Status.ToString(),
           o.CreatedAt,
           o.TotalCents,
           o.ItemCount,
           o.PaymentMethod.ToString(),
           o.DeliveryType.ToString(),
           o.Notes,
           false)).ToArray();

        var mapper = new OrderMapper();
        // Act
        var response = mapper.ToPagedResponse(dtos, 1, 5, 5);
        // Assert
        response.Items.Should().HaveCount(5);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(5);
        response.Total.Should().Be(5);
    }
}
