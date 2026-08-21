# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/03. MSTest`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A `[TestClass]` has two `[TestInitialize]` methods. One test passes in Visual Studio, another fails on CI — and swapping test names changes which one fails. Review:

```csharp
[TestClass]
public class DiscountSetupOrderTests
{
    private DiscountCalculator _calculator = null!;
    private decimal _seededTotal;

    [TestInitialize]
    public void SeedFromTierDiscount()
    {
        _seededTotal = _calculator.ApplyTierDiscount(100.00m, CustomerTier.Silver);
    }

    [TestInitialize]
    public void CreateCalculator()
    {
        _calculator = new DiscountCalculator();
    }

    [TestMethod]
    public void SeededTotal_IsNinety_WhenSilverOnOneHundred()
    {
        Assert.AreEqual(90.00m, _seededTotal);
    }
}
```

What lifecycle rule did the author violate, and how do you fix it?

---

#### Q2. (R) A developer ports money tests from `DiscountCalculatorDataTests.cs` and the project no longer builds. Review:

```csharp
[DataTestMethod]
[DataRow(100.00m, 0.10m, 90.00m)]
[DataRow(50.00m, 0.20m, 40.00m)]
public void ApplyPercentDiscount_ReturnsExpectedTotal(
    decimal orderTotal,
    decimal discountRate,
    decimal expectedTotal)
{
    var calculator = new DiscountCalculator();
    decimal result = calculator.ApplyPercentDiscount(orderTotal, discountRate);
    Assert.AreEqual(expectedTotal, result);
}
```

What is wrong with this parameterized test, and what pattern does this chapter use instead?

---

#### Q3. (R) A `[DataTestMethod]` is tagged `[TestMethod]` by mistake during a merge. Test Explorer shows one test; the team assumes all discount rows are covered. Review:

```csharp
[TestClass]
public class DiscountRowCoverageTests
{
    private DiscountCalculator _calculator = null!;

    [TestInitialize]
    public void SetUp() => _calculator = new DiscountCalculator();

    [TestMethod]
    [DataRow(100.00, 0.10, 90.00)]
    [DataRow(50.00, 0.20, 40.00)]
    [DataRow(0.00, 0.05, 0.00)]
    public void ApplyPercentDiscount_ReturnsExpectedTotal(
        double orderTotal, double discountRate, double expectedTotal)
    {
        decimal result = _calculator.ApplyPercentDiscount(
            (decimal)orderTotal, (decimal)discountRate);
        Assert.AreEqual((decimal)expectedTotal, result);
    }
}
```

What breaks at compile time or discovery time, and why is `[DataTestMethod]` required here?

---

#### Q4. (R) A pricing test passes locally but fails on a parallel CI agent. Review the class-level setup and two tests:

```csharp
[TestClass]
public class TierRateTableTests
{
    private static Dictionary<CustomerTier, decimal> _tierRateTable = null!;
    private DiscountCalculator _calculator = null!;

    [ClassInitialize]
    public static void InitClass(TestContext context)
    {
        _tierRateTable = new Dictionary<CustomerTier, decimal>
        {
            [CustomerTier.Standard] = 0.05m,
            [CustomerTier.Silver] = 0.10m,
            [CustomerTier.Gold] = 0.15m,
        };
    }

    [TestInitialize]
    public void SetUp() => _calculator = new DiscountCalculator();

    [TestMethod]
    public void OverrideGoldRate_ForPromoScenario()
    {
        _tierRateTable[CustomerTier.Gold] = 0.25m;
        Assert.AreEqual(0.25m, _tierRateTable[CustomerTier.Gold]);
    }

    [TestMethod]
    public void GoldRate_MatchesClassInitializeDefault()
    {
        Assert.AreEqual(0.15m, _tierRateTable[CustomerTier.Gold]);
    }
}
```

What does `[ClassInitialize]` actually share across tests, and why is the second test flaky?

---

#### Q5. (R) A teammate adds a "quick sanity" test for computed discount rates using `float`. It fails in CI with mismatched expected vs actual. Review:

```csharp
[TestMethod]
public void ApplyPercentDiscount_TenPercentOnHundred_ReturnsNinetyAsFloat()
{
    var calculator = new DiscountCalculator();
    float orderTotal = 100.0f;
    float rate = 0.10f;

    float discounted = (float)calculator.ApplyPercentDiscount(
        (decimal)orderTotal, (decimal)rate);

    Assert.AreEqual(90.0f, discounted);
}

[TestMethod]
public void FloatSum_IsPointThree()
{
    double sum = 0.1 + 0.2;
    Assert.AreEqual(0.3, sum);
}
```

What is wrong with exact equality on floating-point types in MSTest, and how does `Assert.AreEqual` help?

---

#### Q6. (D) Your team starts a greenfield .NET 8 microservice. `dotnet new xunit` is the SDK default; Visual Studio still offers an MSTest template. Half the org maintains legacy MSTest suites from .NET Framework migrations. For **new** unit-test projects on this service, which framework would you standardize on and why? Cover discovery, lifecycle hooks, parallelism defaults, and when staying on MSTest is the pragmatic choice.

---
