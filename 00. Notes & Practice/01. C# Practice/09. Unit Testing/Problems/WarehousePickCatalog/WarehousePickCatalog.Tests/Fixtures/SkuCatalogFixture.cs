using System;
using System.Collections.Generic;
using WarehouseOperations;
using Xunit;

namespace WarehousePickCatalog.Tests.Fixtures;

/*
 * Shared catalog seeded once — used by IClassFixture and ICollectionFixture.
 */
public sealed class SkuCatalogFixture
{
    public SkuCatalog Catalog { get; } = new SkuCatalog();

    public SkuCatalogFixture()
    {
        // TODO: call SeedCatalog()
        throw new NotImplementedException();
    }

    private void SeedCatalog()
    {
        // TODO: Catalog.Seed with >= 3 SkuItem entries (distinct PickZone values)
        throw new NotImplementedException();
    }
}

/*
 * Collection definition — classes with [Collection("SkuCatalog")] share one fixture instance.
 */
[CollectionDefinition("SkuCatalog")]
public sealed class SkuCatalogCollection : ICollectionFixture<SkuCatalogFixture>
{
}
