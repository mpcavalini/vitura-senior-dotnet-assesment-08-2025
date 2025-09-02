using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using Vitura.API.Models;
using Vitura.Models.Enums;

namespace Vitura.API.Services;

public class OrderDataLoader : IHostedService
{
    public static ConcurrentBag<Order> Orders { get; private set; }
    private readonly ILogger<OrderDataLoader> _logger;
    private readonly IHostEnvironment _env;

    static OrderDataLoader()
    {
        var contentRoot = AppContext.BaseDirectory;
        var filePath = Path.Combine(contentRoot, "sample-orders.json");
        try
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var orders = JsonSerializer.Deserialize<Order[]>(json, options);
                Orders = orders != null ? new ConcurrentBag<Order>(orders) : new ConcurrentBag<Order>(GetFallbackOrders());
            }
            else
            {
                Orders = new ConcurrentBag<Order>(GetFallbackOrders());
            }
        }
        catch
        {
            Orders = new ConcurrentBag<Order>(GetFallbackOrders());
        }
    }

    public OrderDataLoader(ILogger<OrderDataLoader> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;

        _logger.LogInformation("OrderDataLoader initialized with {OrderCount} orders", Orders.Count);
        foreach (var order in Orders.Take(3))
        {
            _logger.LogInformation("Sample order: {OrderId} - {PharmacyId} - {Status}",
                order.Id, order.PharmacyId, order.Status);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static IEnumerable<Order> GetFallbackOrders() => new[]
    {
        new Order(Guid.NewGuid(), "ph001", OrderStatus.Shipped, DateTime.UtcNow.AddDays(-1), 12000, 2, PaymentMethod.Card, DeliveryType.Standard, "First order"),
        new Order(Guid.NewGuid(), "ph002", OrderStatus.Pending, DateTime.UtcNow.AddDays(-2), 34000, 5, PaymentMethod.HICAPS, DeliveryType.ClickAndCollect, "Second order"),
        new Order(Guid.NewGuid(), "ph003", OrderStatus.Processing, DateTime.UtcNow.AddDays(-3), 56000, 3, PaymentMethod.BPAY, DeliveryType.Express, "Third order")
    };
}