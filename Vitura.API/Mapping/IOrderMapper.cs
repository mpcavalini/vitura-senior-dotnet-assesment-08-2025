using Vitura.API.DTOs;
using Vitura.API.Models;

namespace Vitura.API.Mapping;

public interface IOrderMapper
{
    OrderResponseDto ToDto(Order order, bool needsReview);
    PagedResponse<OrderResponseDto> ToPagedResponse(OrderResponseDto[] items, int page, int pageSize, int total);
}