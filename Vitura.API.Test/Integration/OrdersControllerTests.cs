using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Vitura.API.DTOs;
using Vitura.Models.Enums;
using Xunit;

public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public OrdersControllerTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task GetOrders_Default_Parameters_Returns_Success()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/orders");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<OrderResponseDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrders_Multiple_Filters_Combined_Success()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/orders?pharmacyId=ph001&statuses=Shipped&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<OrderResponseDto>>();
        result!.Items.Should().OnlyContain(x => x.PharmacyId == "ph001" && x.Status == OrderStatus.Shipped.ToString());
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetOrders_Cancelled_Token_Throws_TaskCancelled()
    {
        var client = _factory.CreateClient();
        using var cts = new CancellationTokenSource(0);
        await Assert.ThrowsAsync<TaskCanceledException>(async () => {
            await client.GetAsync("/api/orders", cts.Token);
        });
    }

    [Fact]
    public async Task GetOrders_Empty_Result_Returns_Valid_Response()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/orders?pharmacyId=NOTFOUND");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<OrderResponseDto>>();
        result!.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }
}
