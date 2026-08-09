using System;
using System.Linq;
using WarehouseOperations;
using WarehousePickCatalog.Tests.Fixtures;
using Xunit;

namespace WarehousePickCatalog.Tests;

public sealed class PickListClassFixtureTests : IClassFixture<SkuCatalogFixture>
{
    private readonly SkuCatalogFixture _fixture;

    public PickListClassFixtureTests(SkuCatalogFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void BuildPickPath_ReturnsZonesForKnownSkus()
    {
        // TODO: PickListBuilder with _fixture.Catalog; request 2 known SKUs; assert zone count
        throw new NotImplementedException();
    }
}
