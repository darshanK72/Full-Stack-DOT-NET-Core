/*
 * TOPIC: The xUnit test framework — [Fact], [Theory], data sources, fixtures,
 *        diagnostic output, Assert helpers, and parallel execution defaults.
 *
 * WHY IT MATTERS:
 *   xUnit is the default test framework for modern .NET (ASP.NET Core templates,
 *   open-source libraries, CI pipelines). Mastering its attributes, fixture
 *   lifetimes, and parallelism rules lets you write fast, maintainable tests
 *   and debug failures without guessing why setup ran twice or tests interfered.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Testing-module layout — Program.cs entry, Services/Models SUT, XUnit.Tests/
 *   2.  [Fact] — single-scenario tests and common Assert helpers
 *   3.  [Theory] + [InlineData] — inline parameterized rows
 *   4.  [MemberData] — complex or computed test inputs
 *   5.  ITestOutputHelper — per-test diagnostic output
 *   6.  IClassFixture<T> — one shared instance per test class
 *   7.  ICollectionFixture<T> + [CollectionDefinition] — shared across classes
 *   8.  Parallel execution defaults — class-level parallelism in xUnit
 *   9.  Preview: MSTest and mocking (detailed in later chapters)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using XUnit.Models;
using XUnit.Services;

namespace XUnit;

public class Program
{
    /*
     * SECTION 4: DEMONSTRATION — Main orchestrates the SUT demo
     *
     * Main runs production code once so you can see expected numbers before
     * opening XUnit.Tests/. Each xUnit concept is taught in the matching test
     * file — read Program.cs for the map, then open the test file for code.
     *
     * Run tests from 09. Unit Testing/:
     *   dotnet test "02. xUnit/XUnit.Tests/XUnit.Tests.csproj"
     */
    public static void Main(string[] args)
    {
        Order sampleOrder = CreateSampleOrder();
        var pricing = new OrderPricingService();

        decimal subtotal = pricing.CalculateSubtotal(sampleOrder.Lines);
        const decimal discountPercent = 10m;
        const decimal taxRatePercent = 8m;
        decimal afterDiscount = pricing.ApplyDiscount(subtotal, discountPercent);
        decimal tax = pricing.CalculateTax(afterDiscount, taxRatePercent);
        decimal grandTotal = pricing.CalculateGrandTotal(sampleOrder, discountPercent, taxRatePercent);

        Console.WriteLine("=== Order pricing demo (SUT) ===");
        Console.WriteLine($"Order ID:     {sampleOrder.OrderId}");
        Console.WriteLine($"Line count:   {sampleOrder.Lines.Count}");
        Console.WriteLine($"Subtotal:     {FormatMoney(subtotal)}");
        Console.WriteLine($"After {discountPercent}% discount: {FormatMoney(afterDiscount)}");
        Console.WriteLine($"Tax ({taxRatePercent}%):           {FormatMoney(tax)}");
        Console.WriteLine($"Grand total:  {FormatMoney(grandTotal)}");
        Console.WriteLine($"Bulk eligible (>= $100): {pricing.IsEligibleForBulkDiscount(sampleOrder.Lines)}");
        Console.WriteLine();
        Console.WriteLine("=== xUnit test files (read in this order) ===");
        Console.WriteLine("  FactTests.cs              — [Fact], Assert.Equal/True/False/Contains/Throws/InRange");
        Console.WriteLine("  DiscountTheoryTests.cs    — [Theory], [InlineData], [MemberData]");
        Console.WriteLine("  OutputHelperTests.cs      — ITestOutputHelper");
        Console.WriteLine("  ClassFixtureTests.cs      — IClassFixture<T>");
        Console.WriteLine("  CollectionFixtureTestsA/B — ICollectionFixture + [Collection]");
        Console.WriteLine("  ParallelExecutionTests.cs — class-level parallelism notes");
        Console.WriteLine();
        Console.WriteLine("Run: dotnet test \"02. xUnit/XUnit.Tests/XUnit.Tests.csproj\"");
    }

    private static Order CreateSampleOrder()
    {
        return new Order
        {
            OrderId = "ORD-1001",
            Lines =
            [
                new OrderLine
                {
                    Sku = "KB-001",
                    ProductName = "Mechanical Keyboard",
                    Quantity = 1,
                    UnitPrice = 79.99m,
                },
                new OrderLine
                {
                    Sku = "MS-010",
                    ProductName = "Wireless Mouse",
                    Quantity = 2,
                    UnitPrice = 29.50m,
                },
            ],
        };
    }

    private static string FormatMoney(decimal amount)
    {
        return amount.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
    }
}

/*
 * QUICK REFERENCE — xUnit (this chapter)
 *
 *   [Fact]                              Single test; no parameters
 *   [Theory] + [InlineData(...)]        Parameterized rows with literals
 *   [Theory] + [MemberData(nameof(...))] Complex inputs from static property/method
 *   ITestOutputHelper                     Constructor injection; WriteLine per test
 *   IClassFixture<T>                      One T per test class
 *   [CollectionDefinition("Name")]        Declares collection + ICollectionFixture<T>
 *   [Collection("Name")]                  Assigns class to collection (shared fixture)
 *
 *   Assert.Equal / True / False / Contains / Throws<T> / InRange
 *
 *   Parallelism: different test classes run in parallel by default; methods in
 *   the same class run sequentially; classes in the same [Collection] serialize.
 *
 *   MSTest ([TestMethod], [TestClass])  → 03. MSTest
 *   Moq, fakes, stubs                   → 04. Mocking & Test Doubles
 *
 *   dotnet test --logger "console;verbosity=detailed"   Show ITestOutputHelper output
 */
