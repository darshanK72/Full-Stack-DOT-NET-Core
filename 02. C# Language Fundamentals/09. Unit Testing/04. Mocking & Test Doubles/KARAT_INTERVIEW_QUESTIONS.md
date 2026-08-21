# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/04. Mocking & Test Doubles`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (D) Your team tests `OrderService.PlaceOrder` three different ways — `StubInventoryService`, `FakeEmailSender`, and `Mock<IEmailSender>` with `Verify`. For a new test that asserts "insufficient stock cancels the order and no email is sent," which double type do you pick for **inventory** and for **email**, and why? When would you swap the email side from fake to mock?

---

#### Q2. (R) A teammate refactors `OrderServiceMoqTests` to "prove every collaboration." Review the Arrange/Assert block:

```csharp
[Fact]
public void PlaceOrder_WhenStockAvailable_ProvesFullOrchestration()
{
    var inventoryMock = new Mock<IInventoryService>();
    inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true);
    inventoryMock.Setup(i => i.HasStock("GADGET-02", 1)).Returns(true);
    inventoryMock.Setup(i => i.Reserve("WIDGET-01", 2));
    inventoryMock.Setup(i => i.Reserve("GADGET-02", 1));

    var emailMock = new Mock<IEmailSender>();
    var service = new OrderService(inventoryMock.Object, emailMock.Object);

    service.PlaceOrder(CreateSampleOrder());

    inventoryMock.Verify(i => i.HasStock("WIDGET-01", 2), Times.Once);
    inventoryMock.Verify(i => i.HasStock("GADGET-02", 1), Times.Once);
    inventoryMock.Verify(i => i.Reserve("WIDGET-01", 2), Times.Once);
    inventoryMock.Verify(i => i.Reserve("GADGET-02", 1), Times.Once);
    inventoryMock.Verify(i => i.HasStock(It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(2));
    inventoryMock.Verify(i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(2));
    emailMock.Verify(e => e.SendOrderConfirmation("buyer@example.com", 1001, 49.99m), Times.Once);
    emailMock.VerifyNoOtherCalls();
    inventoryMock.VerifyNoOtherCalls();
}
```

What is wrong with this test as a unit test of `OrderService`, and what would you keep vs delete?

---

#### Q3. (R) After a harmless refactor — merging the two `foreach` loops in `OrderService.PlaceOrder` into one pass — CI fails on this test (adapted from `PlaceOrder_WhenStockAvailable_ChecksStockBeforeEveryReserve`):

```csharp
[Fact]
public void PlaceOrder_ChecksAllStockBeforeAnyReserve()
{
    var inventoryMock = new Mock<IInventoryService>();
    inventoryMock.Setup(i => i.HasStock(It.IsAny<string>(), It.IsAny<int>())).Returns(true);

    var callLog = new List<string>();
    inventoryMock
        .Setup(i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()))
        .Callback<string, int>((sku, qty) => callLog.Add($"Reserve:{sku}"));

    inventoryMock
        .Setup(i => i.HasStock(It.IsAny<string>(), It.IsAny<int>()))
        .Callback<string, int>((sku, qty) => callLog.Add($"HasStock:{sku}"))
        .Returns(true);

    var emailMock = new Mock<IEmailSender>();
    var service = new OrderService(inventoryMock.Object, emailMock.Object);

    service.PlaceOrder(CreateSampleOrder());

    Assert.Equal(
        new[] { "HasStock:WIDGET-01", "HasStock:GADGET-02", "Reserve:WIDGET-01", "Reserve:GADGET-02" },
        callLog);
}
```

The refactored `PlaceOrder` still returns success, still reserves both lines, and still sends one email — but this test fails. Diagnose the failure mode and recommend a behavior-focused replacement.

---

#### Q4. (P) Production `OrderService` will call a warehouse REST API through `HttpClient`. You must unit-test `WarehouseInventoryService : IInventoryService` without network I/O. Sketch the test seam — how do you substitute HTTP responses, and why is `new HttpClient()` inside the service a blocker?

---

#### Q5. (D) A pricing microservice client is injected into checkout. Your lead asks whether to test it with (A) a custom `HttpMessageHandler` stub in a unit test, (B) `WebApplicationFactory` hitting an in-memory test server, or (C) a contract test against a deployed sandbox. Map each option to a layer of the **test pyramid** for this repo's `OrderService` chapter. When is each the right default?

---

#### Q6. (R) A junior duplicates production stock logic inside Moq `Setup` chains so tests "stay realistic":

```csharp
var stock = new Dictionary<string, int> { ["WIDGET-01"] = 10, ["GADGET-02"] = 5 };

var inventoryMock = new Mock<IInventoryService>();
inventoryMock
    .Setup(i => i.HasStock(It.IsAny<string>(), It.IsAny<int>()))
    .Returns<string, int>((sku, qty) =>
        stock.TryGetValue(sku, out int n) && n >= qty);
inventoryMock
    .Setup(i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()))
    .Callback<string, int>((sku, qty) => stock[sku] -= qty);

var emailMock = new Mock<IEmailSender>();
var service = new OrderService(inventoryMock.Object, emailMock.Object);

OrderResult result = service.PlaceOrder(CreateSampleOrder());

Assert.True(result.Success);
Assert.Equal(8, stock["WIDGET-01"]);
```

Compare this to `OrderServiceManualDoubleTests` using `StubInventoryService` + `FakeEmailSender`. What maintenance and design problems does the mock-heavy version introduce, and when is the manual double clearly better?

---

#### Q7. (R) Review this failure-path test for the second-line-out-of-stock scenario (`PlaceOrder_WhenSecondLineOutOfStock_DoesNotReserveFirstLine`):

```csharp
[Fact]
public void PlaceOrder_WhenSecondLineOutOfStock_DoesNotReserveFirstLine()
{
    var inventoryMock = new Mock<IInventoryService>();
    inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true);
    inventoryMock.Setup(i => i.HasStock("GADGET-02", 1)).Returns(false);
    // Missing Setup for Reserve — default void mock behavior

    var emailMock = new Mock<IEmailSender>();
    var service = new OrderService(inventoryMock.Object, emailMock.Object);

    OrderResult result = service.PlaceOrder(CreateSampleOrder());

    Assert.False(result.Success);
    inventoryMock.Verify(i => i.Reserve("WIDGET-01", 2), Times.Never);
    inventoryMock.Verify(i => i.Reserve("GADGET-02", 1), Times.Never);
    emailMock.Verify(
        e => e.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()),
        Times.Never);
}
```

The test passes today. What happens if someone changes `OrderService` to reserve the first line optimistically before checking the second — and why does this test not catch that regression? How would you strengthen the assertion without reintroducing brittle call-order checks?
