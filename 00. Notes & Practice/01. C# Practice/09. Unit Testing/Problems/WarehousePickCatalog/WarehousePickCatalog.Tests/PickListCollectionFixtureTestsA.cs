using System;
using WarehousePickCatalog.Tests.Fixtures;
using Xunit;

namespace WarehousePickCatalog.Tests;

[Collection("SkuCatalog")]
public sealed class PickListCollectionFixtureTestsA
{
    private readonly SkuCatalogFixture _fixture;

    public PickListCollectionFixtureTestsA(SkuCatalogFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void SharedCatalog_HasExpectedCount_FromCollectionFixture()
    {
        // TODO: Assert.Equal(3, _fixture.Catalog.Count) or your seed count
        throw new NotImplementedException();
    }
}

[Collection("SkuCatalog")]
public sealed class PickListCollectionFixtureTestsB
{
    private readonly SkuCatalogFixture _fixture;

    public PickListCollectionFixtureTestsB(SkuCatalogFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void SharedCatalog_SameInstanceAsOtherCollectionClass()
    {
        // TODO: Assert same Count as TestsA (proves shared ICollectionFixture)
        throw new NotImplementedException();
    }
}
