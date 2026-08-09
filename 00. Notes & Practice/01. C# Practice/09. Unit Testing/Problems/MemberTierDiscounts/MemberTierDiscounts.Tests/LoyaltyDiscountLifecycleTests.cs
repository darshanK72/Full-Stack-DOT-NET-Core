using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MemberTierDiscounts.Tests;

[TestClass]
public class LoyaltyDiscountLifecycleTests
{
    public static int TestsRunCount { get; private set; }

    [ClassInitialize]
    public static void ClassSetup(TestContext context)
    {
        // TODO: increment TestsRunCount (starts at 0)
        throw new NotImplementedException();
    }

    [TestMethod]
    public void ClassInitialize_RanBeforeTests()
    {
        // TODO: Assert.IsTrue(TestsRunCount >= 1)
        throw new NotImplementedException();
    }
}
