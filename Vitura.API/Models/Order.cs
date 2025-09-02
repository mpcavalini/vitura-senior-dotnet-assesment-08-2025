using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Vitura.Models.Enums;

namespace Vitura.API.Models;

public record Order(
    [property: JsonPropertyName("id")]
    [property: Required]
    Guid Id,

    [property: JsonPropertyName("pharmacyId")]
    [property: Required]
    [property: RegularExpression(@"^ph\d{3}$", ErrorMessage = "PharmacyId must be in format 'phXXX' where X is a digit")]
    string PharmacyId,

    [property: JsonPropertyName("status")]
    [property: Required]
    OrderStatus  Status,

    [property: JsonPropertyName("createdAt")]
    [property: Required]
    DateTime CreatedAt,

    [property: JsonPropertyName("totalCents")]
    [property: Range(0, int.MaxValue, ErrorMessage = "TotalCents must be a non-negative number")]
    int TotalCents,

    [property: JsonPropertyName("itemCount")]
    [property: Range(1, int.MaxValue, ErrorMessage = "ItemCount must be at least 1")]
    int ItemCount,

    [property: JsonPropertyName("paymentMethod")]
    [property: Required]
    PaymentMethod  PaymentMethod,

    [property: JsonPropertyName("deliveryType")]
    [property: Required]
    DeliveryType  DeliveryType,

    [property: JsonPropertyName("notes")]
    string Notes
);