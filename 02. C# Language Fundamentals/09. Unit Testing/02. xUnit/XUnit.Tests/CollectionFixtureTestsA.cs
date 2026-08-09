using XUnit.Tests.Fixtures;
using Xunit;

namespace XUnit.Tests;

/*
 * SECTION 9c: ICollectionFixture — SHARED ACROSS MULTIPLE TEST CLASSES
 *
 * When several test classes must share one expensive resource, assign them to
 * the same collection:
 *
 *   [Collection("OrderCatalog")]
 *   public class CollectionFixtureTestsA { ... }
 *
 *   [Collection("OrderCatalog")]
 *   public class CollectionFixtureTestsB { ... }
 *
 * All classes in "OrderCatalog" share one OrderCatalogFixture instance.
 * Tests in the collection do not run in parallel with each other (see §10).
 *
 * CollectionFixtureTestsA and CollectionFixtureTestsB prove two classes receive
 * the same fixture — compare with ClassFixtureTests which gets its own instance.
 */
[Collection("OrderCatalog")]
public sealed class CollectionFixtureTestsA
{
    private readonly OrderCatalogFixture _fixture;

    public CollectionFixtureTestsA(OrderCatalogFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void SharedCatalog_StandardOrderSubtotal_FromClassA()
    {
        decimal subtotal = _fixture.Service.CalculateSubtotal(_fixture.StandardOrder.Lines);

        Assert.Equal(138.99m, subtotal);
    }
}
