# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/04. Mocking & Test Doubles`

---

#### Q1. (D) Your team tests `OrderService.PlaceOrder` three different ways — `StubInventoryService`, `FakeEmailSender`, and `Mock<IEmailSender>` with `Verify`. For a new test that asserts "insufficient stock cancels the order and no email is sent," which double type do you pick for **inventory** and for **email**, and why? When would you swap the email side from fake to mock?

**Answer:** Use a **stub** (or minimal mock `Setup`) for inventory to force `HasStock` to return `false`, and a **fake** for email so you assert `SentMessages` is empty — the same pattern as `PlaceOrder_WhenStubReportsLowStock_FailsWithoutEmailOrReservation` in **OrderServiceManualDoubleTests.cs**. Swap email to a mock when you only need a spy (`Verify(..., Times.Never)`) and do not care about inspecting message contents — as in **OrderServiceMoqTests.cs** failure tests.

- **Inventory → stub:** The scenario needs a canned `false` from `HasStock("WIDGET-01", 5)` when stock is 1. A stub dictionary or one `Setup().Returns(false)` is enough; no need to verify `Reserve` was never called if stock state stays unchanged (stub) or you add a single `Verify(..., Times.Never)` on reserve.
- **Email → fake (default here):** `FakeEmailSender.SentMessages` gives a readable behavior assertion (`Assert.Empty`) without Moq ceremony. The chapter's **Program.cs** Section 5 demo uses this path.
- **Email → mock:** Prefer `Mock<IEmailSender>` + `Verify(..., Times.Never)` when the test file is already Moq-centric, when email is not the focus and you want consistent mock-only Arrange blocks, or when multiple void collaborators each need `Times.Never` in one Assert section.
- **Avoid mock for inventory in this scenario** unless you also need to prove `Reserve` was never invoked — the stub's unchanged dictionary already proves no reservation side effect.
- **Production takeaway:** Pick doubles by **what you need to observe** — return values (stub), accumulated state (fake), or interaction counts (mock/spy). See **Program.cs** Section 2 taxonomy table.

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

**Answer:** The test duplicates the same guarantees multiple times and locks the SUT to today's call graph instead of observable outcomes — classic **over-mocking** and **over-verification**. It will fail on innocent refactors (single-pass loop, batch reserve API) even when customers still get confirmed orders.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Redundant `Verify` — specific SKU calls **and** `It.IsAny` with `Exactly(2)` | Same assertion twice; failure messages confuse reviewers |
| Maintainability | `VerifyNoOtherCalls()` on inventory | Breaks when `OrderService` adds logging, metrics, or a harmless extra query |
| Design | Tests **how** `PlaceOrder` walks lines, not **what** it delivers | Refactors that preserve behavior still break CI |
| Missing assertion | No check on `OrderResult.Success` or `OrderId` | Could pass if `PlaceOrder` returned default struct after side effects |

**Fix (priority order):**

1. Assert **behavior first:** `Assert.True(result.Success)` and `Assert.Equal(1001, result.OrderId)` — missing from the snippet.
2. Keep **one** essential interaction block: per-line `Reserve` once + email once **or** switch to **StubInventoryService** + **FakeEmailSender** and assert stock decrements and `SentMessages.Single()` — see **OrderServiceManualDoubleTests.cs**.
3. Delete duplicate `It.IsAny` + `Exactly(2)` verifies when specific SKU verifies already cover the contract.
4. Drop `VerifyNoOtherCalls()` unless you are testing a strict anti-corruption boundary; prefer targeted `Times.Never` on failure paths only.
5. Do **not** verify every `HasStock` on the happy path unless the requirement is explicitly "check-before-reserve" — that belongs in a dedicated test or integration test, not stacked on every success test.

**Production takeaway:** Moq `Verify` is for **essential collaborations** (email sent once, reserve never on failure) — not a full execution transcript. See **Program.cs** Section 8 pitfalls table.

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

The refactored `OrderService` still returns success, still reserves both lines, and still sends one email — but this test fails. Diagnose the failure mode and recommend a behavior-focused replacement.

**Answer:** The test asserts **call order**, not business outcome — a **brittle Setup/Callback sequence** tied to the two-loop implementation in **OrderService.cs**. A single-loop version might interleave `HasStock` and `Reserve` per line (`HasStock WIDGET → Reserve WIDGET → HasStock GADGET → Reserve GADGET`) and still be correct, but `callLog` no longer matches the hard-coded array.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Asserts internal orchestration sequence | False failures on equivalent refactor |
| Threading | `List<string>` mutated from mock callbacks | Safe in single-threaded unit test, but pattern encourages fragile global ordering |
| Maintainability | Two `Setup` registrations on `HasStock` | Second Setup overrides behavior; easy to misconfigure when extending |

**Fix (priority order):**

1. **Delete** the ordered `callLog` assertion for the happy path.
2. Replace with outcome checks from **OrderServiceManualDoubleTests.cs**: success result, stock decremented to 8 and 4, one email in `FakeEmailSender`.
3. For the **all-or-nothing** invariant ("second line fails → first line never reserved"), keep **OrderServiceMoqTests** `PlaceOrder_WhenSecondLineOutOfStock_DoesNotReserveFirstLine` style `Verify(Reserve..., Times.Never)` — that tests a **business rule**, not loop structure.
4. If check-then-reserve must be guaranteed at integration level, test with a **fake inventory** that throws if `Reserve` is called while a flag `CheckPhaseComplete` is false — still behavior, still refactor-tolerant within the rule.
5. Reserve `Callback` capture lists (as in `PlaceOrder_WhenStockAvailable_CallbackCapturesReservedSkus`) for **what** was reserved, not **when** relative to unrelated calls.

**Production takeaway:** **Program.cs** Section 8 — "Verify internal call order" is listed as a pitfall; prefer "order fails, email never sent" over "HasStock before Reserve on line 23."

---

#### Q4. (P) Production `OrderService` will call a warehouse REST API through `HttpClient`. You must unit-test `WarehouseInventoryService : IInventoryService` without network I/O. Sketch the test seam — how do you substitute HTTP responses, and why is `new HttpClient()` inside the service a blocker?

**Answer:** Inject `HttpClient` (via constructor or `IHttpClientFactory`) and in tests pass an `HttpClient` backed by a **custom `HttpMessageHandler`** that returns canned `HttpResponseMessage` instances — no socket, no DNS, no sandbox API.

- **Handler stub:** Subclass `HttpMessageHandler`, override `SendAsync`, return `new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"available":10}""", Encoding.UTF8, "application/json") }`. Wrap with `new HttpClient(handler)`.
- **Register in DI:** Production uses `services.AddHttpClient<IInventoryService, WarehouseInventoryService>()`; tests construct `WarehouseInventoryService` with the stub handler directly — same seam as **OrderService** constructor injection of **IInventoryService**.
- **`new HttpClient()` inside the service blocks testing** because you cannot swap the handler without reflection; each test would hit the real network or require a global static mock server.
- **Also avoid** `HttpClient` as singleton with wrong DNS lifetime — use factory; handler substitution stays per-test.
- **Map to chapter:** `HasStock`/`Reserve` become HTTP GET/PATCH; unit test parses handler response into bool / verifies request URI and JSON body via `handler.LastRequest` or Moq on a delegating handler.
- **Production takeaway:** External API testing at unit scope = **fake transport**, not fake internet. Integration tests (fewer) may use `WebApplicationFactory` or wiremock; unit tests stay at handler level.

```csharp
var handler = new StubHttpMessageHandler(_ =>
    new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(new { sku = "WIDGET-01", qty = 10 }),
    });
var client = new HttpClient(handler) { BaseAddress = new Uri("https://warehouse.test/") };
var inventory = new WarehouseInventoryService(client);
```

---

#### Q5. (D) A pricing microservice client is injected into checkout. Your lead asks whether to test it with (A) a custom `HttpMessageHandler` stub in a unit test, (B) `WebApplicationFactory` hitting an in-memory test server, or (C) a contract test against a deployed sandbox. Map each option to a layer of the **test pyramid** for this repo's `OrderService` chapter. When is each the right default?

**Answer:** **A** is the wide base (fast unit tests of `WarehouseInventoryService` / HTTP client wrapper); **B** is the middle (integration — your API + DI + middleware + serialization); **C** is the narrow top (end-to-end or contract against real external systems).

| Option | Pyramid layer | What it proves | Default when |
|---|---|---|---|
| **A — `HttpMessageHandler` stub** | Unit (many) | Your code builds correct request and maps response to `HasStock`/`Reserve` | Every branch of client parsing, error handling, retries — milliseconds per test |
| **B — `WebApplicationFactory`** | Integration (some) | `OrderService` registered in ASP.NET pipeline, model binding, auth, `PlaceOrder` endpoint returns 200/409 | Controller + DI wiring + JSON contract; still no real warehouse if inventory is mocked in test `ConfigureServices` |
| **C — Sandbox contract test** | E2E / contract (few) | Real pricing API version, TLS, auth tokens, rate limits | Nightly or pre-release; not on every keystroke |

- **This chapter's focus:** **OrderService** + doubles = layer **A** mindset — isolate SUT, substitute **IInventoryService** (stub/fake/mock), run `dotnet test` in milliseconds (**Program.cs** Section 1).
- **Do not** use **C** to prove "insufficient stock skips email" — that logic belongs in unit tests with **FakeEmailSender** or Moq `Times.Never`.
- **B** fits when `PlaceOrder` becomes an HTTP POST and you need to verify ProblemDetails on 400 — factory spins Kestrel in-process; handler stubs still replace outbound warehouse calls inside the test server.
- **Anti-pattern:** Only **C** tests — slow, flaky, cannot cover every stock edge case; pyramid inverts to ice cream cone.
- **Production takeaway:** Handler stubs test **your** HTTP adapter; factory tests **your** app's wiring; sandbox tests **their** API still matches your assumptions.

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

**Answer:** The mock version reimplements **StubInventoryService** inside Moq lambdas — same behavior, worse readability and no reuse. Manual doubles in **TestDoubles/** are the chapter's intended home for working in-memory logic; mocks should stay thin (`Returns(true)` / `Times.Never`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Maintainability | Business logic duplicated in test Setup | Stock rules drift from **StubInventoryService.cs** |
| Readability | Lambda `Setup` harder to scan than `new StubInventoryService(stock)` | Reviewers miss failure scenarios |
| Design | Mock used as **fake** (stateful dictionary) | Moq shines for verification, not reimplementing domain |
| Missing coverage | Email side still mock with no assertion | Test proves stock math, not confirmation sent |

**Fix (priority order):**

1. Replace mock inventory with **`StubInventoryService`** + **`FakeEmailSender`** — identical assertions, see **OrderServiceManualDoubleTests.cs** `PlaceOrder_WithStubAndFake_SucceedsReservesStockAndSendsEmail`.
2. Use mocks when scenario is **one boolean** (`HasStock` false) or **spy** (`Verify` email once) — **OrderServiceMoqTests.cs** failure and `Throws` tests.
3. Extract shared stock dictionary to test helper only if many tests need it — not into every mock Setup.
4. Add `Assert.Single(email.SentMessages)` when using fake — complete behavior check.

**Production takeaway:** **Fake/stub classes** for stateful simplified behavior; **mocks** for interaction contracts. Duplicating fake logic inside Moq is an over-mocking smell — see **Program.cs** Section 8 "Duplicate production logic in Setup."

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

**Answer:** Moq's default for void methods is a **no-op** — `Reserve` does nothing and **`Times.Never` still passes** even if `Reserve` **was called**, because Verify only counts invocations on the mock recorder, and calls did happen but were silently ignored. Wait - actually if Reserve WAS called, Verify Times.Never would FAIL because the mock tracks invocations regardless of Setup.

Let me reconsider: If Reserve is called, Moq mock DOES record the invocation even without Setup. So Verify Times.Never WOULD fail if Reserve was called.

Actually Moq tracks all calls on the mock. Setup is only needed for return values or throws. For void methods, default behavior is no-op but calls ARE recorded. So if someone reserves first line optimistically, Verify(Reserve WIDGET, Times.Never) would FAIL.

So the test WOULD catch optimistic reserve of first line... unless they only reserve on second line failure after partial reserve - then Reserve WIDGET would be called and Verify would fail.

Hmm, the question says "why does this test not catch that regression?" - maybe the scenario is different. If they change to reserve first line optimistically BEFORE checking second:
- HasStock WIDGET true -> Reserve WIDGET called
- HasStock GADGET false -> fail
- Verify Reserve WIDGET Times.Never would FAIL

So the test WOULD catch it. Let me think of another angle...

Maybe the issue is: if they use a **stub with real state** instead of mock, the stub would show stock decremented - but with pure mock without callback, you don't see side effects on a dictionary.

Actually the better angle for "doesn't catch regression":
- If Reserve is called on mock without Setup, Verify Times.Never still works and catches it
- The weakness might be: using mock without state - you verify Never but don't prove **all-or-nothing** with observable state
- Alternative regression: change to batch API `ReserveAll()` - the specific Verify for individual SKUs might not match new API while behavior is wrong

Or: if someone changes failure to happen AFTER reserving first line in production with real DB, the mock test passes Never on Reserve - but if the bug calls Reserve, Verify fails. Good.

Actually re-read the question - "Missing Setup for Reserve — default void mock behavior" - the pedagogical point might be:
1. Times.Never on mock DOES work for detecting calls
2. BUT the test doesn't assert on **stub state** - if you switched to StubInventoryService, Assert stock unchanged is stronger behavioral proof
3. Or: Verify on mock without strict mock behavior - if Reserve throws in production stub but mock swallows, different issue

Another angle: **Strict mock** - if using Strict(), any unexpected Reserve would throw during Act, not just at Verify.

I'll also mention: the test doesn't assert FailureReason contains GADGET-02, doesn't use stub to prove stock unchanged - complementary weaknesses.

For optimistic reserve scenario - Moq Verify Times.Never WOULD catch Reserve("WIDGET-01", 2) being called. Unless the refactor uses different quantities or batches.

Let me think of a case where it DOESN'T catch:
- If OrderService is changed to call `Reserve` through a different overload or wrapper not verified
- If they use `It.IsAny` reserve internally with different signature
- If the regression is **email sent before failure** - test has Times.Never on email - would catch

Maybe the intended answer is about **stub vs mock**: with mock-only, you don't observe that stock dictionary is unchanged - you only verify method wasn't called. If someone bypasses IInventoryService and writes directly... no that's silly.

From Gerard Meszaros / common interview angle: **Verify Times.Never proves the method wasn't invoked on the mock, but doesn't prove transactional rollback** in integration. For unit test with mock, it should catch Reserve calls.

I'll re-read OrderService - two phase check then reserve. If changed to interleaved reserve, Reserve WIDGET would be called before GADGET check fails - Verify Never on WIDGET would fail. Good.

Perhaps the trick is: **`Times.Never` with `It.IsAny` is the right assertion** but per-SKU Never is redundant; the real gap is **no assertion that result.FailureReason mentions the failing SKU** and **no stub state check**.

For "doesn't catch optimistic reserve" - I'll pivot: if developer changes to **reserve first line inside HasStock callback** or uses a **fire-and-forget** path, or changes to **`Reserve` with quantity 0** which might not match Verify expression `Reserve("WIDGET-01", 2)` exactly - Verify is expression-tree exact match!

That's it! If optimistic reserve calls `Reserve("WIDGET-01", 2)` it fails. But if bug reserves with **`Reserve("WIDGET-01", 1)`** wrong qty or uses batch id, Verify for exact args misses it while still mutating inventory in production.

Or: **`Verify` only checks mock proxy** — if `OrderService` gets a **real StubInventoryService** in production bug scenario, unit test with mock doesn't run production code path... we're testing OrderService with mock so Reserve calls are on mock.

I'll write answer focusing on:
1. Mock void default = no-op, calls still recorded — Verify Never DOES catch explicit Reserve calls with matching expression
2. Gap: exact-expression Verify misses equivalent harmful calls (different qty, batch method)
3. Strengthen with **StubInventoryService** + assert stock unchanged (8 not 8... starts at 10, if reserve 2 would be 8) — behavioral proof without call order
4. Add FailureReason assertion
5. Optional `MockBehavior.Strict` on inventory mock to fail fast on unexpected Reserve during Act

Actually re-read again - "why does this test not catch that regression?" 

For Moq: calling Reserve without setup - the invocation IS tracked. Verify Never WILL fail.

Unless - the question author thinks Never doesn't work without Setup? That's a common misconception I should clarify: **Verify works; the gap is state-based proof and exact matcher brittleness**.

I'll also mention: if optimistic reservation happens via **same Reserve calls**, test catches it. The **stronger** improvement is stub/fake for observable state (stock still 10) which survives API reshaping better than coupling to Reserve method still existing.

If OrderService refactored to single `TryPlaceOrder` on inventory interface, old Verify on Reserve becomes obsolete and passes vacuously if Reserve removed from interface entirely while bug persists in new method - **over-coupling to current method names**.

Good answer angle!


StrReplace