using XUnit.Tests.Fixtures;
using Xunit;

namespace XUnit.Tests.Fixtures;

/*
 * SECTION 9a: [CollectionDefinition] — DECLARE A NAMED COLLECTION + FIXTURE
 *
 * ICollectionFixture<T> on a [CollectionDefinition] class registers fixture T
 * for all test classes that opt into the same collection name via [Collection].
 *
 * The definition class is usually empty — it exists only to bind name + fixture type.
 */
[CollectionDefinition("OrderCatalog")]
public sealed class OrderCatalogCollection : ICollectionFixture<OrderCatalogFixture>
{
}
