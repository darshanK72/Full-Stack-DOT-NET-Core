using Xunit;

namespace XUnit.Tests;

/*
 * SECTION 10: PARALLEL EXECUTION DEFAULTS
 *
 * xUnit parallelizes at the test-class level by default:
 *
 *   | Scope                         | Default behavior              |
 *   |-------------------------------|-------------------------------|
 *   | Methods in the same class     | Sequential                    |
 *   | Methods in different classes  | Parallel (separate threads)   |
 *   | Classes in same [Collection]  | Sequential within collection  |
 *
 * Implications:
 *   • Stateless tests in unrelated classes scale on multi-core CI agents.
 *   • Shared mutable static state across classes can cause flaky failures —
 *     group those tests into a [Collection] or disable parallelization.
 *   • ICollectionFixture already defines a synchronization boundary.
 *
 * To disable all parallelism (rare):
 *   [assembly: CollectionBehavior(DisableTestParallelization = true)]
 *
 * Prefer collection-scoped isolation over turning off parallelism globally.
 *
 * These tests are documentation-style — they pass regardless of thread timing.
 * ParallelExecutionCollectionTests sits in "OrderCatalog" to show collection serialization.
 */
public sealed class ParallelExecutionTests
{
    [Fact]
    public void UnrelatedTestClass_CanRunInParallelWithOtherClasses()
    {
        Assert.True(true);
    }

    [Fact]
    public void MethodsInSameClass_RunSequentially_NotInParallelWithEachOther()
    {
        Assert.True(true);
    }
}

[Collection("OrderCatalog")]
public sealed class ParallelExecutionCollectionTests
{
    [Fact]
    public void CollectionTests_DoNotRunInParallelWithOtherCollectionMembers()
    {
        Assert.True(true);
    }
}
