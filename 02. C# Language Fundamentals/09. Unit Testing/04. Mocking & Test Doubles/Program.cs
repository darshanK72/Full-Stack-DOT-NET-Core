/*
 * TOPIC: Test doubles — substitutes for real dependencies so unit tests stay fast,
 *        deterministic, and isolated. Stubs return canned data; fakes provide
 *        working simplified behavior; mocks (Moq) combine stubbed returns with
 *        interaction verification (spy behavior).
 *
 * WHY IT MATTERS:
 *   OrderService must not require a live warehouse database or SMTP server to prove
 *   that "insufficient stock cancels the order." Constructor injection of
 *   IInventoryService and IEmailSender creates a seam — swap production infrastructure
 *   for controlled doubles in tests. Moq generates proxy implementations at runtime so
 *   you can Setup return values, simulate Throws, capture Callback side effects, and
 *   Verify collaborators were called correctly.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Why isolate dependencies — interfaces, constructor injection, fast tests
 *   2.  Test double taxonomy — stub, fake, mock, spy (conceptual)
 *   3.  Manual doubles — TestDoubles/StubInventoryService, FakeEmailSender
 *   4.  OrderService orchestration — two-phase stock check then reserve + email
 *   5.  Moq — Mock<T>, Setup, Returns, Verify, Times.Once/Never, It.IsAny<T>()
 *   6.  Moq advanced — Throws, Callback for side-effect capture
 *   7.  Pitfalls — over-mocking, asserting implementation instead of behavior
 */

using System;
using System.Collections.Generic;
using MockingAndTestDoubles.Models;
using MockingAndTestDoubles.Services;
using MockingAndTestDoubles.TestDoubles;

namespace MockingAndTestDoubles;

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * SECTION 1: PROJECT LAYOUT — PRODUCTION CODE VS TEST PROJECT
         *
         *   MockingAndTestDoubles.csproj     — runnable demo + types under test
         *   Services/                        — SUT (OrderService) + interfaces
         *   Models/                          — Order, OrderLine, OrderResult
         *   TestDoubles/                     — hand-written stub and fake
         *   MockingAndTestDoubles.Tests/     — xUnit + Moq (dotnet test)
         *
         * Chapters 01–03 cover AAA and xUnit/MSTest basics. This chapter assumes
         * Arrange → Act → Assert and focuses on replacing dependencies.
         *
         * Run tests from 09. Unit Testing/:
         *   dotnet test "04. Mocking & Test Doubles/MockingAndTestDoubles.Tests"
         */

        Console.WriteLine("=== Mocking and Test Doubles — Order placement demo ===\n");

        /*
         * SECTION 2: TEST DOUBLE TAXONOMY
         *
         * A test double is any object standing in for a real dependency.
         *
         *   Kind  | Role                              | Example in this folder
         *   ------|-----------------------------------|------------------------------------------
         *   Stub  | Returns canned answers            | StubInventoryService (fixed stock map)
         *   Fake  | Working simplified implementation | FakeEmailSender (in-memory SentMessages)
         *   Mock  | Framework-generated double        | Moq Mock<IEmailSender> in .Tests
         *   Spy   | Records calls for later assertion | Moq Verify (mocks act as stub + spy)
         *
         * Decision guide:
         *   • Need fixed inputs only?              → stub (or mock.Setup().Returns())
         *   • Need lightweight real behavior?      → fake
         *   • Need to assert method was called?    → mock + Verify (spy behavior)
         *
         * Manual double tests → OrderServiceManualDoubleTests.cs
         * Moq tests           → OrderServiceMoqTests.cs
         */

        RunSuccessfulOrderWithStubAndFake();
        RunFailedOrderWhenStockIsLow();

        /*
         * SECTION 3: WHY ISOLATE DEPENDENCIES — INTERFACES AND DI
         *
         * OrderService receives IInventoryService and IEmailSender through its
         * constructor. Production wiring (Startup, DI container) might register
         * SqlInventoryService and SmtpEmailSender; tests pass doubles instead.
         *
         * Benefits:
         *   • Tests run in milliseconds — no network or database
         *   • Deterministic — you control HasStock return values
         *   • Focus — failure localizes to OrderService logic, not infrastructure
         *
         * Moq creates proxies for interfaces (or classes with virtual members).
         * See Services/IInventoryService.cs and Services/IEmailSender.cs.
         */

        Console.WriteLine("\n--- Dependency seams (interfaces + constructor injection) ---");
        Console.WriteLine("  IInventoryService → HasStock, Reserve");
        Console.WriteLine("  IEmailSender      → SendOrderConfirmation");
        Console.WriteLine("  OrderService      → PlaceOrder orchestrates both");

        /*
         * SECTION 7: MOQ ESSENTIALS (RUNNABLE IN TEST PROJECT)
         *
         * Typical Arrange-Act-Assert with Moq:
         *
         *   var inventoryMock = new Mock<IInventoryService>();
         *   inventoryMock
         *       .Setup(i => i.HasStock("WIDGET-01", 2))
         *       .Returns(true);
         *
         *   var emailMock = new Mock<IEmailSender>();
         *   var service = new OrderService(inventoryMock.Object, emailMock.Object);
         *
         *   OrderResult result = service.PlaceOrder(order);
         *
         *   Assert.True(result.Success);
         *   emailMock.Verify(
         *       e => e.SendOrderConfirmation("buyer@example.com", 1001, 49.99m),
         *       Times.Once);
         *
         *   | API              | Purpose
         *   |------------------|--------------------------------------------------
         *   | Mock<T>()        | Create proxy implementing T
         *   | .Setup(expr)     | Intercept a method call pattern
         *   | .Returns(value)  | Stub return value (bool, etc.)
         *   | .Throws(ex)      | Simulate dependency failure
         *   | .Callback(...)   | Run code when matched call happens (capture args)
         *   | .Object          | Pass the double into the SUT constructor
         *   | .Verify(expr, n) | Assert interaction count (Times.Once, Never, …)
         *   | It.IsAny<T>()    | Match any argument of type T
         *
         * File: MockingAndTestDoubles.Tests/OrderServiceMoqTests.cs
         */

        Console.WriteLine("\n--- Moq patterns (Setup, Verify, Throws, Callback) ---");
        Console.WriteLine("  Open MockingAndTestDoubles.Tests/OrderServiceMoqTests.cs");
        Console.WriteLine("  Run: dotnet test MockingAndTestDoubles.Tests");

        /*
         * SECTION 8: PITFALLS — OVER-MOCKING AND IMPLEMENTATION TESTS
         *
         *   Pitfall                         | Better approach
         *   --------------------------------|--------------------------------------------
         *   Mock every private helper       | Test public behavior (PlaceOrder result + key interactions)
         *   Verify internal call order      | Assert outcome and essential collaborations only
         *   Duplicate production logic in Setup | Keep Setup minimal — only what the SUT reads
         *   Mock concrete classes without seam | Prefer interfaces for external dependencies
         *
         * Test behavior: "when stock is low, order fails and email is never sent."
         * Not implementation: "HasStock is called before Reserve on line 23."
         * The second test breaks when you refactor loop structure without changing behavior.
         */

        Console.WriteLine("\n--- Pitfalls ---");
        Console.WriteLine("  Prefer behavior assertions over brittle call-order checks");
        Console.WriteLine("  Use fakes when inspecting state is simpler than Verify");
        Console.WriteLine("  Use stubs/mocks when you need precise return values or Never/Once");
    }

    /*
     * SECTION 4: DEMO — SUCCESS PATH WITH STUB + FAKE
     *
     * StubInventoryService answers HasStock from a dictionary you own.
     * FakeEmailSender collects confirmations — inspect SentMessages after PlaceOrder.
     * No Moq required when state inspection is enough.
     */
    private static void RunSuccessfulOrderWithStubAndFake()
    {
        var stock = new Dictionary<string, int>
        {
            ["WIDGET-01"] = 10,
            ["GADGET-02"] = 5,
        };

        IInventoryService inventory = new StubInventoryService(stock);
        FakeEmailSender email = new FakeEmailSender();
        OrderService service = new OrderService(inventory, email);

        Order order = new Order
        {
            OrderId = 1001,
            CustomerEmail = "buyer@example.com",
            Lines =
            [
                new OrderLine { Sku = "WIDGET-01", Quantity = 2 },
                new OrderLine { Sku = "GADGET-02", Quantity = 1 },
            ],
            Total = 49.99m,
        };

        OrderResult result = service.PlaceOrder(order);

        Console.WriteLine("--- Successful order (stub inventory + fake email) ---");
        Console.WriteLine($"  Success: {result.Success}, OrderId: {result.OrderId}");
        Console.WriteLine($"  Remaining WIDGET-01 stock: {stock["WIDGET-01"]}");
        Console.WriteLine($"  Emails sent: {email.SentMessages.Count}");
        if (email.SentMessages.Count > 0)
        {
            (string to, int id, decimal total) = email.SentMessages[0];
            Console.WriteLine($"  Confirmation → {to}, order {id}, total {total:C}");
        }
    }

    /*
     * SECTION 5: DEMO — FAILURE PATH WHEN STUB REPORTS LOW STOCK
     *
     * Same SUT and fake; stub dictionary has only 1 unit but order wants 5.
     * PlaceOrder returns Failed — fake SentMessages stays empty (no email side effect).
     * Moq tests express the same guarantee with Verify(..., Times.Never).
     */
    private static void RunFailedOrderWhenStockIsLow()
    {
        var stock = new Dictionary<string, int> { ["WIDGET-01"] = 1 };

        IInventoryService inventory = new StubInventoryService(stock);
        FakeEmailSender email = new FakeEmailSender();
        OrderService service = new OrderService(inventory, email);

        Order order = new Order
        {
            OrderId = 1002,
            CustomerEmail = "buyer@example.com",
            Lines = [new OrderLine { Sku = "WIDGET-01", Quantity = 5 }],
            Total = 25.00m,
        };

        OrderResult result = service.PlaceOrder(order);

        Console.WriteLine("\n--- Failed order (insufficient stock) ---");
        Console.WriteLine($"  Success: {result.Success}");
        Console.WriteLine($"  Reason: {result.FailureReason}");
        Console.WriteLine($"  Emails sent: {email.SentMessages.Count}");
        Console.WriteLine($"  WIDGET-01 stock unchanged: {stock["WIDGET-01"]}");
    }
}

/*
 * QUICK REFERENCE — TEST DOUBLES AND MOQ
 *
 * Double | Use when
 * -------|----------------------------------------------------------------
 * Stub   | Dependency must return fixed values; minimal logic
 * Fake   | Need working in-memory behavior you inspect afterward
 * Mock   | Need Setup return values AND Verify interaction counts/args
 * Spy    | Record calls — Moq Verify on a mock fills this role
 *
 * Moq essentials:
 *   new Mock<IInventoryService>()
 *   mock.Setup(m => m.HasStock("A", 1)).Returns(true);
 *   mock.Setup(m => m.Reserve("A", 1)).Throws(new InvalidOperationException());
 *   mock.Setup(m => m.Reserve(It.IsAny<string>(), It.IsAny<int>()))
 *       .Callback<string, int>((sku, qty) => { ... });  // capture args in closure
 *   mock.Object                                    // pass to SUT
 *   mock.Verify(m => m.Reserve("A", 1), Times.Once);
 *   mock.Verify(m => m.Reserve(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
 *
 * Times: Once, Never, Exactly(n), AtLeastOnce, AtMost(n)
 *
 * Test projects:
 *   OrderServiceManualDoubleTests.cs — StubInventoryService + FakeEmailSender
 *   OrderServiceMoqTests.cs          — Mock<T>, Setup, Verify, Throws, Callback
 */
