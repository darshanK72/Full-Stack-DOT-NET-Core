using System.Collections.Generic;
using XUnit.Models;
using XUnit.Services;

namespace XUnit.Tests.Fixtures;

/*
 * SECTION 9b: FIXTURE CLASS — SHARED SETUP DATA
 *
 * OrderCatalogFixture builds the pricing service and a standard sample order.
 * Used by IClassFixture (one instance per class) and ICollectionFixture
 * (one instance shared by every class in the "OrderCatalog" collection).
 *
 * CreateStandardOrder is static so [Fact] tests can reuse the same data without
 * a fixture when they only need the order shape.
 */
public sealed class OrderCatalogFixture
{
    public OrderCatalogFixture()
    {
        Service = new OrderPricingService();
        StandardOrder = CreateStandardOrder();
    }

    public OrderPricingService Service { get; }

    public Order StandardOrder { get; }

    public static Order CreateStandardOrder()
    {
        return new Order
        {
            OrderId = "ORD-TEST-001",
            Lines = new List<OrderLine>
            {
                new OrderLine
                {
                    Sku = "KB-001",
                    ProductName = "Mechanical Keyboard",
                    Quantity = 1,
                    UnitPrice = 79.99m,
                },
                new OrderLine
                {
                    Sku = "MS-010",
                    ProductName = "Wireless Mouse",
                    Quantity = 2,
                    UnitPrice = 29.50m,
                },
            },
        };
    }
}
