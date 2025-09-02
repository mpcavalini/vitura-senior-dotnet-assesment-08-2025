using Vitura.API.Models;

namespace Vitura.API.Services;

public interface IOrderRepository {
    IEnumerable<Order> GetAll();
}

public class InMemoryOrderRepository : IOrderRepository {
    private readonly ILogger<InMemoryOrderRepository> _logger;

    public InMemoryOrderRepository(ILogger<InMemoryOrderRepository> logger)
    {
        _logger = logger;
    }

    public IEnumerable<Order> GetAll()
    {
        var orders = OrderDataLoader.Orders.ToArray();
        _logger.LogInformation("Repository returning {OrderCount} orders", orders.Length);
        return orders;
    }
}
