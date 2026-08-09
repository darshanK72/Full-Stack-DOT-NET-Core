using XUnit.Tests.Fixtures;
using Xunit;

namespace XUnit.Tests;

/*
 * SECTION 9d: SECOND CLASS IN SAME COLLECTION
 *
 * CollectionFixtureTestsB joins the "OrderCatalog" collection defined in
 * Fixtures/OrderCatalogCollection.cs. xUnit injects the same OrderCatalogFixture
 * instance that CollectionFixtureTestsA receives — not a separate copy per class.
 */
[Collection("OrderCatalog")]
public sealed class CollectionFixtureTestsB
{
    private readonly OrderCatalogFixture _fixture;

    public CollectionFixtureTestsB(OrderCatalogFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void SharedCatalog_BulkEligibility_FromClassB()
    {
        bool eligible = _fixture.Service.IsEligibleForBulkDiscount(_fixture.StandardOrder.Lines);

        Assert.True(eligible);
    }

    [Fact]
    public void SharedCatalog_SameOrderId_AsClassA()
    {
        Assert.Equal("ORD-TEST-001", _fixture.StandardOrder.OrderId);
    }
}
