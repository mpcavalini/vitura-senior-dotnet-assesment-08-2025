using System.Text.Json.Serialization;

namespace Vitura.API.DTOs;

public record OrderResponseDto(
    [property: JsonPropertyName("id")]
    Guid Id,

    [property: JsonPropertyName("pharmacyId")]
    string PharmacyId,

    [property: JsonPropertyName("status")]
    string Status,

    [property: JsonPropertyName("createdAt")]
    DateTime CreatedAt,

    [property: JsonPropertyName("totalCents")]
    int TotalCents,

    [property: JsonPropertyName("itemCount")]
    int ItemCount,

    [property: JsonPropertyName("paymentMethod")]
    string PaymentMethod,

    [property: JsonPropertyName("deliveryType")]
    string DeliveryType,

    [property: JsonPropertyName("notes")]
    string Notes,

    [property: JsonPropertyName("needsReview")]
    bool? NeedsReview = null // Only present if true
);

public record PagedResponse<T>(
    [property: JsonPropertyName("items")]
    T[] Items,

    [property: JsonPropertyName("page")]
    int Page,

    [property: JsonPropertyName("pageSize")]
    int PageSize,

    [property: JsonPropertyName("total")]
    int Total
);
