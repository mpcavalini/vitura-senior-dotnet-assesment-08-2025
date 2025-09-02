using System.Text.Json.Serialization;

namespace Vitura.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderStatus
    {
        Pending,
        Processing,
        Packed,
        Shipped,
        Delivered,
        Cancelled
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeliveryType
    {
        Standard,
        Express,
        ClickAndCollect
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PaymentMethod
    {
        Card,
        BPAY,
        HICAPS,
        Invoice
    }
}