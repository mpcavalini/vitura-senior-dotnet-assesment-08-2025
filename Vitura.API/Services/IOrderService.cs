namespace Vitura.API.Services;

using Vitura.API.DTOs;

public interface IOrderService 
{
    Task<PagedResponse<OrderResponseDto>> GetOrdersAsync(
        OrderQueryParams query, 
        CancellationToken cancellationToken
    );
}

public record OrderQueryParams(
    string? PharmacyId = null,
    string[]? Statuses = null, 
    DateTime? From = null,
    DateTime? To = null,
    string Sort = "createdAt",
    string Direction = "desc", 
    int Page = 1,
    int PageSize = 20
);
