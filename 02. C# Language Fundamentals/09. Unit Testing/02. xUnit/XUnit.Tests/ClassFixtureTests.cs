using XUnit.Tests.Fixtures;
using Xunit;

namespace XUnit.Tests;

/*
 * SECTION 8: IClassFixture<T> — ONE SHARED INSTANCE PER TEST CLASS
 *
 * Expensive or reusable setup (seed data, HTTP client, catalog) can be created
 * once per test class:
 *
 *   public class ClassFixtureTests : IClassFixture<OrderCatalogFixture>
 *   {
 *       public ClassFixtureTests(OrderCatalogFixture fixture) { ... }
 *   }
 *
 * xUnit constructs OrderCatalogFixture once and injects the same instance into
 * every test in ClassFixtureTests. A different test class gets its own fixture.
 *
 * Tests in the same class run sequentially — but avoid mutating shared state
 * unless the test explicitly documents order dependence.
 */
public sealed class ClassFixtureTests : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;

    public ClassFixtureTests(OrderCatalogFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Fixture_ProvidesSharedStandardOrder()
    {
        decimal subtotal = _fixture.Service.CalculateSubtotal(_fixture.StandardOrder.Lines);

        Assert.Equal(138.99m, subtotal);
    }

    [Fact]
    public void Fixture_ProvidesSameServiceInstanceForAllTestsInClass()
    {
        Assert.Same(_fixture.Service, _fixture.Service);
    }
}
