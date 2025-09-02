using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Vitura.API.DTOs;
using Vitura.API.Mapping;
using Vitura.API.Models;
using Vitura.API.Services;
using Vitura.API.Validation;
using Vitura.Models.Enums;
using Xunit;

namespace Vitura.API.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockRepo;
    private readonly Mock<IOrderMapper> _mockMapper;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly IConfiguration _config;

    public OrderServiceTests()
    {
        _mockRepo = new Mock<IOrderRepository>();
        _mockMapper = new Mock<IOrderMapper>();
        _mockLogger = new Mock<ILogger<OrderService>>();

        var configData = new Dictionary<string, string>
        {
            {"Review:DailyOrderThresholdCents", "50000"}
        };
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
    }

    [Fact]
    public async Task GetOrdersAsync_ShouldApplyNeedsReviewFlagCorrectly()
    {
        // Arrange
        var orders = new[]
        {
            new Order(Guid.NewGuid(), "ph001", OrderStatus.Pending, DateTime.UtcNow, 40000, 1, PaymentMethod.Card, DeliveryType.Standard, "Below threshold"),
            new Order(Guid.NewGuid(), "ph002",OrderStatus.Pending, DateTime.UtcNow, 60000, 1, PaymentMethod.Card, DeliveryType.Standard, "Above threshold")
        };

        _mockRepo.Setup(r => r.GetAll()).Returns(orders);

        // Configure mapper to return the needsReview value we pass to it
        _mockMapper.Setup(m => m.ToDto(It.IsAny<Order>(), false))
            .Returns((Order o, bool nr) => new OrderResponseDto(o.Id, o.PharmacyId, o.Status.ToString(), o.CreatedAt, o.TotalCents, o.ItemCount, o.PaymentMethod.ToString(), o.DeliveryType.ToString(), o.Notes, null));

        _mockMapper.Setup(m => m.ToDto(It.IsAny<Order>(), true))
            .Returns((Order o, bool nr) => new OrderResponseDto(o.Id, o.PharmacyId, o.Status.ToString(), o.CreatedAt, o.TotalCents, o.ItemCount, o.PaymentMethod.ToString(), o.DeliveryType.ToString(), o.Notes, true));

        _mockMapper.Setup(m => m.ToPagedResponse(It.IsAny<OrderResponseDto[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns((OrderResponseDto[] items, int page, int pageSize, int total) => new PagedResponse<OrderResponseDto>(items, page, pageSize, total));

        var service = new OrderService(_config, _mockRepo.Object, _mockMapper.Object, _mockLogger.Object);
        var query = new OrderQueryParams();

        // Act
        var result = await service.GetOrdersAsync(query, CancellationToken.None);

        // Assert
        _mockMapper.Verify(m => m.ToDto(orders[0], false), Times.Once); // Below threshold
        _mockMapper.Verify(m => m.ToDto(orders[1], true), Times.Once);  // Above threshold
    }

    [Fact]
    public async Task GetOrdersAsync_ShouldReturnStablePaginationResults()
    {
        // Arrange
        var orders = Enumerable.Range(1, 25)
            .Select(i => new Order(
                Guid.NewGuid(),
                $"ph{i:D3}",
                OrderStatus.Pending,
                DateTime.UtcNow.AddDays(-i),
                i * 1000,
                1,
                PaymentMethod.Card,
                DeliveryType.Standard,
                $"Order {i}"))
            .ToArray();

        _mockRepo.Setup(r => r.GetAll()).Returns(orders);

        _mockMapper.Setup(m => m.ToDto(It.IsAny<Order>(), It.IsAny<bool>()))
            .Returns((Order o, bool nr) => new OrderResponseDto(o.Id, o.PharmacyId, o.Status.ToString(), o.CreatedAt, o.TotalCents, o.ItemCount, o.PaymentMethod.ToString(), o.DeliveryType.ToString(), o.Notes, nr ? true : null));

        _mockMapper.Setup(m => m.ToPagedResponse(It.IsAny<OrderResponseDto[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns((OrderResponseDto[] items, int page, int pageSize, int total) => new PagedResponse<OrderResponseDto>(items, page, pageSize, total));

        var service = new OrderService(_config, _mockRepo.Object, _mockMapper.Object, _mockLogger.Object);
        var query = new OrderQueryParams(Page: 2, PageSize: 10);

        // Act - Call multiple times
        var result1 = await service.GetOrdersAsync(query, CancellationToken.None);
        var result2 = await service.GetOrdersAsync(query, CancellationToken.None);

        // Assert - Results should be identical
        Assert.Equal(result1.Items.Length, result2.Items.Length);
        Assert.Equal(result1.Page, result2.Page);
        Assert.Equal(result1.Total, result2.Total);

        // Verify same order IDs in same sequence
        for (int i = 0; i < result1.Items.Length; i++)
        {
            Assert.Equal(result1.Items[i].Id, result2.Items[i].Id);
        }
    }
}

public class OrderQueryParamsValidatorTests
{
    private readonly OrderQueryParamsValidator _validator;

    public OrderQueryParamsValidatorTests()
    {
        _validator = new OrderQueryParamsValidator();
    }

    [Theory]
    [InlineData("ph001", true)]
    [InlineData("PH001", true)]
    [InlineData("pharmacy-001", true)]
    [InlineData("", false)]
    [InlineData("ph", false)]
    [InlineData("toolongpharmacyidthatexceedsfiftycharacterslimit", false)]
    public void PharmacyId_ShouldValidateCorrectly(string pharmacyId, bool expectedValid)
    {
        // Arrange
        var query = new OrderQueryParams(PharmacyId: pharmacyId);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData(new[] { "pending" }, true)]
    [InlineData(new[] { "pending", "completed" }, true)]
    [InlineData(new[] { "PENDING" }, true)] // Case insensitive
    [InlineData(new[] { "invalid" }, false)]
    [InlineData(new[] { "pending", "invalid" }, false)]
    public void Statuses_ShouldValidateCorrectly(string[] statuses, bool expectedValid)
    {
        // Arrange
        var query = new OrderQueryParams(Statuses: statuses);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData(1, 20, true)]
    [InlineData(1, 100, true)]
    [InlineData(0, 20, false)]
    [InlineData(1, 101, false)]
    public void Pagination_ShouldValidateCorrectly(int page, int pageSize, bool expectedValid)
    {
        // Arrange
        var query = new OrderQueryParams(Page: page, PageSize: pageSize);

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void DateRange_FromGreaterThanTo_ShouldBeInvalid()
    {
        // Arrange
        var query = new OrderQueryParams(
            From: DateTime.UtcNow,
            To: DateTime.UtcNow.AddDays(-1)
        );

        // Act
        var result = _validator.Validate(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("From date must be less than or equal to To date", result.Errors.Select(e => e.ErrorMessage));
    }
}