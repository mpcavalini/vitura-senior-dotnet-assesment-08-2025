using Vitura.API.DTOs;
using Vitura.API.Models;

namespace Vitura.API.Mapping;

public class OrderMapper : IOrderMapper
{
    public OrderResponseDto ToDto(Order order, bool needsReview) =>
    new(order.Id
        , order.PharmacyId
        , order.Status.ToString()
        , order.CreatedAt
        , order.TotalCents
        , order.ItemCount
        , order.PaymentMethod.ToString()
        , order.DeliveryType.ToString()
        , order.Notes
        , needsReview ? true : null);

    public PagedResponse<OrderResponseDto> ToPagedResponse(OrderResponseDto[] items, int page, int pageSize, int total) =>
        new(items, page, pageSize, total);
}