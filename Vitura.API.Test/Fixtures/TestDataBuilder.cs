using Vitura.API.Models;
using Vitura.Models.Enums;

namespace Vitura.API.Tests.Fixtures;

public static class TestDataBuilder
{
    public static Order[] GetSampleOrders() => new[] {
        new Order(
            Id: new Guid("11111111-1111-1111-1111-111111111111"),
            PharmacyId: "ph001",
            Status: OrderStatus.Shipped,
            CreatedAt: new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            TotalCents: 75000,
            ItemCount: 3,
            PaymentMethod: PaymentMethod.HICAPS,
            DeliveryType: DeliveryType.ClickAndCollect,
            Notes: "Test order 1"
        ),
        new Order(
            Id: new Guid("22222222-2222-2222-2222-222222222222"),
            PharmacyId: "phM002",
            Status:OrderStatus.Shipped,
            CreatedAt: new DateTime(2024, 1, 16, 11, 0, 0, DateTimeKind.Utc),
            TotalCents: 5000,
            ItemCount: 1,
            PaymentMethod: PaymentMethod.Card,
            DeliveryType: DeliveryType.Standard,
            Notes: "Test order 2"
        ),
        new Order(
            Id: new Guid("33333333-3333-3333-3333-333333333333"),
            PharmacyId: "ph003",
            Status: OrderStatus.Pending,
            CreatedAt: new DateTime(2024, 1, 17, 12, 0, 0, DateTimeKind.Utc),
            TotalCents: 10000,
            ItemCount: 2,
            PaymentMethod: PaymentMethod.HICAPS,
            DeliveryType: DeliveryType.Express,
            Notes: "Test order 3"
        ),
        new Order(
            Id: new Guid("44444444-4444-4444-4444-444444444444"),
            PharmacyId: "ph001",
            Status: OrderStatus.Processing,
            CreatedAt: new DateTime(2024, 1, 18, 13, 0, 0, DateTimeKind.Utc),
            TotalCents: 90000,
            ItemCount: 4,
            PaymentMethod: PaymentMethod.Card,
            DeliveryType: DeliveryType.ClickAndCollect,
            Notes: "Test order 4"
        ),
        new Order(
            Id: new Guid("55555555-5555-5555-5555-555555555555"),
            PharmacyId: "ph002",
            Status: OrderStatus.Shipped,
            CreatedAt: new DateTime(2024, 1, 19, 14, 0, 0, DateTimeKind.Utc),
            TotalCents: 20000,
            ItemCount: 2,
            PaymentMethod:PaymentMethod.HICAPS,
            DeliveryType:DeliveryType.Standard,
            Notes: "Test order 5"
        )
    };

    public static Order HighValueOrder => new(
        Id: Guid.NewGuid(),
        PharmacyId: "ph999",
        Status: OrderStatus.Delivered,
        CreatedAt: DateTime.UtcNow,
        TotalCents: 150000,
        ItemCount: 10,
        PaymentMethod: PaymentMethod.Invoice,
        DeliveryType: DeliveryType.Standard,
        Notes: "High value order"
    );
}
