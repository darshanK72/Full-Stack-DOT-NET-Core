# Mocking & Test Doubles — Interview Q&A


## Table of Contents

1. [Q1. What are the five categories in the test doubles taxonomy and when do you use each?](#q1-what-are-the-five-categories-in-the-test-doubles-taxonomy-and-when-do-you-use-each)
2. [Q2. What is the critical distinction between a Stub and a Mock?](#q2-what-is-the-critical-distinction-between-a-stub-and-a-mock)
3. [Q3. How does a Fake differ from a Stub?](#q3-how-does-a-fake-differ-from-a-stub)
4. [Q4. How do you create a Moq mock and obtain the substitute object the SUT consumes?](#q4-how-do-you-create-a-moq-mock-and-obtain-the-substitute-object-the-sut-consumes)
5. [Q5. How does Setup().Returns() configure a canned response for a dependency method?](#q5-how-does-setupreturns-configure-a-canned-response-for-a-dependency-method)
6. [Q6. How does Moq's Verify() work and what does it assert?](#q6-how-does-moqs-verify-work-and-what-does-it-assert)
7. [Q7. What Times options does Moq provide and when do you use each?](#q7-what-times-options-does-moq-provide-and-when-do-you-use-each)
8. [Q8. What is the difference between Loose and Strict mocks in Moq?](#q8-what-is-the-difference-between-loose-and-strict-mocks-in-moq)
9. [Q9. What is MockBehavior.Strict and what are its practical tradeoffs?](#q9-what-is-mockbehaviorstrict-and-what-are-its-practical-tradeoffs)
10. [Q10. Why does interface-based design enable mocking, and why can Moq not mock non-virtual methods on concrete classes?](#q10-why-does-interface-based-design-enable-mocking-and-why-can-moq-not-mock-non-virtual-methods-on-concrete-classes)
11. [Q11. Why is constructor injection the primary DI pattern for testability?](#q11-why-is-constructor-injection-the-primary-di-pattern-for-testability)
12. [Q12. What does It.IsAny<T>() do in Moq argument matching?](#q12-what-does-itisanyt-do-in-moq-argument-matching)
13. [Q13. How does It.Is<T>(predicate) differ from It.IsAny<T>(), and what is It.IsIn()?](#q13-how-does-itistpredicate-differ-from-itisanyt-and-what-is-itisin)
14. [Q14. How do you simulate exceptions thrown by a dependency using Moq?](#q14-how-do-you-simulate-exceptions-thrown-by-a-dependency-using-moq)
15. [Q15. How does Callback() work in Moq and when should you use it?](#q15-how-does-callback-work-in-moq-and-when-should-you-use-it)
16. [Q16. What does NSubstitute offer and how does its syntax compare to Moq?](#q16-what-does-nsubstitute-offer-and-how-does-its-syntax-compare-to-moq)
17. [Q17. Why does Moq silently return default values for unstubbed calls, and how can this hide production bugs?](#q17-why-does-moq-silently-return-default-values-for-unstubbed-calls-and-how-can-this-hide-production-bugs)
18. [Q18. What is over-specification in tests and why does it create fragile tests?](#q18-what-is-over-specification-in-tests-and-why-does-it-create-fragile-tests)
19. [Q19. What happens when you call Verify() on a method that was never set up, and when is that a valid pattern?](#q19-what-happens-when-you-call-verify-on-a-method-that-was-never-set-up-and-when-is-that-a-valid-pattern)
20. [Q20. Why can you not mix literal arguments and It.* matchers in the same Setup() or Verify() call?](#q20-why-can-you-not-mix-literal-arguments-and-it-matchers-in-the-same-setup-or-verify-call)
21. [Q21. What happens when you use Returns() instead of ReturnsAsync() for an async dependency method?](#q21-what-happens-when-you-use-returns-instead-of-returnsasync-for-an-async-dependency-method)
22. [Q22. How do you write a complete Moq test for OrderService.PlaceOrder() that enforces the all-or-nothing business rule?](#q22-how-do-you-write-a-complete-moq-test-for-orderserviceplaceorder-that-enforces-the-all-or-nothing-business-rule)
23. [Q23. Review the following test. What defects does it contain?](#q23-review-the-following-test-what-defects-does-it-contain)
24. [Q24. When should you prefer manual test doubles (StubInventoryService, FakeEmailSender) over Moq?](#q24-when-should-you-prefer-manual-test-doubles-stubinventoryservice-fakeemailsender-over-moq)
25. [Q25. How do you verify the order in which Reserve() was called for multiple SKUs using Moq?](#q25-how-do-you-verify-the-order-in-which-reserve-was-called-for-multiple-skus-using-moq)
26. [Q26. When should you NOT mock a collaborator, and what types of objects are poor candidates for mocking?](#q26-when-should-you-not-mock-a-collaborator-and-what-types-of-objects-are-poor-candidates-for-mocking)
27. [Q27. How do you migrate an OrderService Moq test suite to NSubstitute while preserving behavioral coverage?](#q27-how-do-you-migrate-an-orderservice-moq-test-suite-to-nsubstitute-while-preserving-behavioral-coverage)

---
## Foundation Questions

---

## Q1. What are the five categories in the test doubles taxonomy and when do you use each?

**Concepts**
- Dummy, Stub, Spy, Mock, Fake
- Gerard Meszaros classification
- state verification vs behavior verification
- usage context per type
- test double selection criteria

**Answer**

Gerard Meszaros's taxonomy defines five test double categories, each serving a different verification purpose. A **Dummy** is an object passed to satisfy a required parameter but never exercised — passing `null` or a no-op `IEmailSender` when testing pure stock logic that never touches email. A **Stub** provides canned answers to queries so the SUT can reach a specific code path; `StubInventoryService` returning `true` for `HasStock` feeds the SUT a controlled input without tracking whether the method was called. A **Spy** is a stub that also records its calls; you inspect the recording after execution and write the assertions yourself. A **Mock** is a pre-programmed object with embedded expectations; it knows in advance which calls must occur and fails the test automatically if they do not — Moq's `Verify()` brings a mock into this mode. A **Fake** has a real, working but simplified implementation: `FakeEmailSender` capturing sent messages in a `SentMessages` list, or an in-memory database replacing SQL Server, are canonical fakes. The distinction matters because mixing categories — asserting on a stub — produces tests that couple to implementation details you never intended to specify and that break on refactoring even when behavior is unchanged.

---

## Q2. What is the critical distinction between a Stub and a Mock?

**Concepts**
- state verification vs behavior verification
- stub answers queries
- mock verifies commands
- test intent
- over-specification risk

**Answer**

The distinction lives in verification style, not in syntax. A stub exists to supply inputs to the SUT so the test can reach a specific code path; after the SUT executes you verify the SUT's output or resulting state. A mock exists to assert that the SUT sent the right messages — method calls — to its collaborators. In the `OrderService` context, configuring `HasStock` to return `true` is stub behavior: you are controlling input, not specifying that `HasStock` must be called. Calling `inventoryMock.Verify(i => i.Reserve("WIDGET-01", 2), Times.Once)` is mock behavior: you are asserting a command was issued to the collaborator. Conflating the two leads to over-specified tests. If you verify every `HasStock` call in addition to the `Reserve` call, you tie the test to the internal query pattern, and the test breaks whenever the stock-check loop is refactored — even when observable behavior is unchanged. The rule of thumb: stub queries (methods that return values and have no side effects), mock commands (methods that cause observable side effects). Stubs answer "what does the world look like to the SUT?"; mocks answer "what did the SUT do to the world?".

---

## Q3. How does a Fake differ from a Stub?

**Concepts**
- fake vs stub
- working business logic in fake
- in-memory replacement
- stub as hard-coded answer
- FakeEmailSender example

**Answer**

A stub is a minimal double that returns hard-coded answers with no internal logic; it exists purely to feed controlled values into the SUT. A fake is a simplified but genuinely functional implementation that replaces a full infrastructure component while remaining fast and dependency-free. The `StubInventoryService` in this codebase actually crosses into fake territory: it is backed by a `Dictionary<string, int>`, its `HasStock` method performs a real comparison (`available >= quantity`), and its `Reserve` method decrements the count. A pure stub would hard-code `return true` regardless of the dictionary contents. A canonical fake is an `InMemoryRepository<T>` that satisfies the full `IRepository<T>` interface using `List<T>` storage, supporting Add, Get, Delete, and simple queries without touching a database. `FakeEmailSender` is a fake: it captures sent messages in a `SentMessages` list and lets tests assert `email.SentMessages.Count == 1`, which is state verification on the fake's own observable state. The distinction matters for test maintenance: fakes can be shared across many tests and grow alongside the interface; stubs are typically test-local and single-purpose. The risk with fakes is that their own correctness becomes load-bearing — a bug in `FakeEmailSender` can mask real issues — so fakes sometimes warrant their own unit tests.

---

## Q4. How do you create a Moq mock and obtain the substitute object the SUT consumes?

**Concepts**
- Mock<T> wrapper
- .Object property
- Castle DynamicProxy
- interface dependency
- mock instantiation pattern

**Answer**

Moq separates the control plane from the substitute object. `Mock<T>` is the wrapper you interact with for setup and verification; `.Object` is the runtime `T` implementation you pass to the SUT. The standard pattern is:

```csharp
Mock<IInventoryService> inventoryMock = new Mock<IInventoryService>();
Mock<IEmailSender> emailMock = new Mock<IEmailSender>();

var sut = new OrderService(inventoryMock.Object, emailMock.Object);
```

`inventoryMock` provides `Setup()` and `Verify()`. `inventoryMock.Object` is the `IInventoryService` instance the `OrderService` constructor receives. Because `OrderService` depends on `IInventoryService` — not on a concrete class — Moq can generate a proxy at runtime using Castle DynamicProxy. This proxy intercepts every method call, checks whether a matching setup exists, and returns the configured value. If no setup matches and the mock is Loose (the default), it returns `default(T)` — `false` for `bool`, `null` for reference types, `0` for numeric types. Keeping `Mock<T>` and `.Object` distinct matters: accidentally passing the `Mock<T>` wrapper itself to a constructor that expects `IInventoryService` results in a compile error, which is an accidental guard that prevents one category of test-authoring mistake.

---

## Q5. How does Setup().Returns() configure a canned response for a dependency method?

**Concepts**
- Setup() expression tree matching
- Returns() value factory
- argument-specific responses
- last-match-wins ordering
- ReturnsAsync for async methods

**Answer**

`Setup()` takes a lambda expression tree that Moq inspects at runtime to identify which method and which argument pattern to intercept. `Returns()` specifies what value the proxy hands back when that pattern matches:

```csharp
inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true);
inventoryMock.Setup(i => i.HasStock("GADGET-05", 1)).Returns(false);
```

When `OrderService` calls `inventory.HasStock("WIDGET-01", 2)`, Moq finds the first setup and returns `true`; the call with `"GADGET-05"` returns `false`. Moq evaluates setups in reverse registration order so the last matching setup wins when patterns overlap. For `async` methods returning `Task<bool>`, you must use `ReturnsAsync(true)` instead of `Returns(true)`; using `Returns(true)` on an async method compiles but causes a `NullReferenceException` or `InvalidCastException` when the SUT awaits, because `bool` is not assignable to `Task<bool>`. For setups that should return different values on successive calls, `SetupSequence()` lets you chain multiple return values in order. Arguments in the lambda must either all be literals or all use `It.*` matchers within a single setup — mixing the two causes an `InvalidOperationException` at runtime.

---

## Q6. How does Moq's Verify() work and what does it assert?

**Concepts**
- Verify() assertion mechanism
- call log inspection
- Times parameter
- MockException on failure
- VerifyAll and VerifyNoOtherCalls

**Answer**

Moq records every intercepted method call on the proxy, regardless of whether a setup exists. After the SUT has executed, `Verify()` inspects that call log and throws `MockException` if the recorded calls do not satisfy the specified `Times` constraint. The syntax mirrors `Setup()` — you pass an expression tree identifying the method and argument pattern:

```csharp
inventoryMock.Verify(i => i.Reserve("WIDGET-01", 2), Times.Once);
inventoryMock.Verify(i => i.Reserve("GADGET-05", 1), Times.Never);
```

The first line asserts the proxy was called exactly once with those specific arguments. The second asserts it was never called with those arguments. `Verify()` uses the same argument-matching system as `Setup()`, so `It.IsAny<string>()` in a Verify expression matches any call regardless of the string value. `VerifyAll()` checks every setup created with `.Verifiable()` was invoked at least once. `VerifyNoOtherCalls()` asserts that no calls were made beyond those already verified, effectively turning a Loose mock into a near-Strict assertion phase without paying the Strict brittleness cost during setup. For `OrderService`, verifying both `Reserve` calls and checking `Times.Never` when stock is insufficient are the two most critical assertions because together they define the all-or-nothing contract.

---

## Q7. What Times options does Moq provide and when do you use each?

**Concepts**
- Times.Once
- Times.Never
- Times.AtLeastOnce
- Times.Exactly(n)
- Times.Between(min, max, RangeKind)

**Answer**

Moq's `Times` struct provides factory methods covering every cardinality scenario. `Times.Once` asserts the method was called exactly once and is the right choice for commands that should have a single effect — reserving stock for a given SKU in a single-line order. `Times.Never` asserts zero calls occurred and is essential for negative tests, such as confirming that `Reserve` was never invoked when any line fails the stock check. `Times.AtLeastOnce` is appropriate when the SUT may call a logging or notification dependency one or more times without a fixed count mattering for correctness. `Times.Exactly(n)` generalizes `Once` to any integer, useful when an order with three lines should produce exactly three `Reserve` calls. `Times.AtMost(n)` provides an upper bound useful for throttling and circuit-breaker scenarios. `Times.Between(min, max, RangeKind.Inclusive)` covers range constraints for polling or retry logic. In the `OrderService` context the most common pair is `Times.Once` for the happy-path reservation per SKU and `Times.Never` for the partial-failure guard, because the business rule mandates that no SKU is reserved unless every SKU passes the stock check. Using `Times.Exactly(order.Lines.Count)` in a parameterized test is a clean way to assert proportional call counts without hard-coding a number.

---

## Q8. What is the difference between Loose and Strict mocks in Moq?

**Concepts**
- MockBehavior.Loose default
- MockBehavior.Strict
- default return values on unstubbed calls
- exception on unexpected call
- tradeoffs between modes

**Answer**

Moq's default behavior is Loose (`MockBehavior.Loose`). Under Loose behavior, any method call that does not match a `Setup()` returns `default(T)` for its return type: `false` for `bool`, `null` for reference types, `0` for numeric types, and a completed `Task` for void-async methods. This is convenient during early test development because you only configure the calls relevant to the current assertion, but it can mask bugs — if the SUT calls a dependency method you forgot to set up, the mock silently returns a default instead of flagging the gap. Strict behavior (`MockBehavior.Strict`) throws `MockException` immediately when any call is made without a matching setup, making every unexpected call an explicit test failure. To create a strict mock: `new Mock<IInventoryService>(MockBehavior.Strict)`. The tradeoff is brittleness: adding any new dependency call to the SUT — even a benign diagnostic call — breaks every strict test covering that code path, requiring setup updates across the suite. The most pragmatic approach is Loose mocks with targeted `Verify()` assertions: you retain flexibility while still confirming the interactions that matter. Reserve Strict for dependencies with irreversible side effects — payment processing, outbound emails, audit logs — where any unexpected call in a test represents a real safety concern.

---

## Q9. What is MockBehavior.Strict and what are its practical tradeoffs?

**Concepts**
- MockBehavior.Strict instantiation
- whitelist of allowed calls
- unexpected call exception timing
- safety for side-effecting dependencies
- refactoring cost

**Answer**

`MockBehavior.Strict` turns the mock into a whitelist: only calls with a registered `Setup()` are permitted; any other invocation throws `MockException` before the SUT returns. This makes every dependency call an explicit declaration visible in the test:

```csharp
var inventoryMock = new Mock<IInventoryService>(MockBehavior.Strict);
inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true);
inventoryMock.Setup(i => i.HasStock("GADGET-05", 1)).Returns(true);
inventoryMock.Setup(i => i.Reserve("WIDGET-01", 2));
inventoryMock.Setup(i => i.Reserve("GADGET-05", 1));
```

If `OrderService` calls any method not in that list, the test fails loudly. The advantages are improved contract visibility and catching accidental calls to dangerous dependencies. The disadvantages are significant: every SUT refactoring that adds even one new dependency call requires updating every Strict test covering that path, creating high coupling between tests and implementation internals. A balanced alternative is Loose mocks for query methods — which return values and rarely cause irreversible effects — and Strict behavior (or `VerifyNoOtherCalls()`) only for commands like email sending or payment processing. In the `OrderService` scenario, `IEmailSender` is a stronger candidate for strict-like treatment than `IInventoryService`, because sending a duplicate confirmation email to a customer is more harmful than making a redundant stock query.

---

## Q10. Why does interface-based design enable mocking, and why can Moq not mock non-virtual methods on concrete classes?

**Concepts**
- interface as seam
- Castle DynamicProxy subclassing
- virtual method requirement
- concrete class limitation
- design for testability

**Answer**

Moq generates test doubles at runtime using Castle DynamicProxy, which creates a subclass of the target type and overrides its methods. For an interface, every member is implicitly abstract, so the proxy can override them all freely. For a concrete class, the proxy can only override members declared `virtual` or `abstract` — non-virtual methods are sealed at the IL level and cannot be intercepted by any subclass. If `OrderService` depended on `InventoryService` (the concrete class) rather than `IInventoryService`, Moq could not intercept `HasStock` or `Reserve` unless those methods were explicitly marked `virtual`. This is not a Moq limitation specifically — it is a constraint of the CLR's inheritance model. The deeper design lesson is that dependencies should be expressed as abstractions so that any collaborator — production code, test double, or alternative implementation — can satisfy the contract. Constructor injection pairs with interface-based design to give tests full control: because `OrderService` receives its dependencies through the constructor, the test decides which implementation to inject. Classes that instantiate their own dependencies with `new InventoryService()` inside the constructor eliminate this seam entirely and make the dependency untestable without modifying production code or using a heavier tool like Microsoft Fakes.

---

## Q11. Why is constructor injection the primary DI pattern for testability?

**Concepts**
- constructor injection
- required dependency declaration
- seam creation
- property injection weakness
- service locator anti-pattern

**Answer**

Constructor injection expresses dependencies as required constructor parameters, which means the SUT cannot be instantiated without them — the compiler enforces that every test must supply collaborators. This gives the test full control over which implementations the SUT receives. In `OrderService`:

```csharp
public OrderService(IInventoryService inventory, IEmailSender email)
{
    _inventory = inventory;
    _email = email;
}
```

A test simply passes mocks: `new OrderService(inventoryMock.Object, emailMock.Object)`. Alternative patterns weaken this control. Property injection allows the SUT to set a production default (`_email = new SmtpEmailSender()`) and lets tests optionally override it — but a test that forgets the override may invoke the real SMTP sender, producing nondeterministic results. Method injection, where a dependency is passed per call, is appropriate for context objects such as `CancellationToken` but is awkward for stable collaborators that the SUT uses throughout its lifetime. Service locator is the worst choice for testability: the SUT calls a global registry at runtime, making interception impossible without replacing the registry itself. Constructor injection also makes the dependency graph visible in the constructor signature, which aids both testing and architectural review — you can instantly see everything a class depends on without reading its implementation.

---

## Q12. What does It.IsAny<T>() do in Moq argument matching?

**Concepts**
- wildcard argument matcher
- It.IsAny<T>() behavior
- Setup and Verify usage
- thread-local matcher queue
- consistent matcher rule

**Answer**

`It.IsAny<T>()` is a Moq argument matcher that matches any value of type `T`, including `null` for reference types. When placed in a `Setup()` expression it causes the setup to match any call to that method regardless of what argument is passed for that position:

```csharp
inventoryMock
    .Setup(i => i.HasStock(It.IsAny<string>(), It.IsAny<int>()))
    .Returns(true);
```

This single setup returns `true` for any SKU and any quantity, useful when you want to focus the test on behavior that occurs after the stock check rather than on which SKU was checked. In `Verify()`, `It.IsAny<T>()` counts any matching call regardless of arguments toward the `Times` constraint:

```csharp
inventoryMock.Verify(
    i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()),
    Times.Never);
```

This asserts that no reservation occurred for any SKU at any quantity — the correct assertion for the all-or-nothing failure path. The important constraint is that within a single `Setup()` or `Verify()` call you cannot mix literal values with `It.*` matchers. If one argument uses `It.IsAny<string>()`, all other arguments must also use matchers; mixing causes an `InvalidOperationException` at runtime. Wrap literals in `It.Is<T>(v => v == literal)` when mixing is necessary, though using matchers for all arguments is the simpler choice when argument values are not load-bearing for the assertion.

---

## Q13. How does It.Is<T>(predicate) differ from It.IsAny<T>(), and what is It.IsIn()?

**Concepts**
- It.Is<T>(predicate) custom matching
- It.IsIn() collection membership
- It.IsNotNull<T>() convenience matcher
- Verify with predicate
- precision vs flexibility

**Answer**

`It.Is<T>(predicate)` accepts a lambda that receives the actual runtime argument and returns `bool`, enabling arbitrarily complex matching without binding to a specific literal. To match any quantity greater than zero:

```csharp
inventoryMock
    .Setup(i => i.HasStock(It.IsAny<string>(), It.Is<int>(q => q > 0)))
    .Returns(true);
```

This is more expressive than `It.IsAny<int>()` (too permissive) or a literal (too specific). `It.IsIn(collection)` matches when the argument is contained in a provided set:

```csharp
inventoryMock
    .Setup(i => i.HasStock(It.IsIn("WIDGET-01", "GADGET-05"), It.IsAny<int>()))
    .Returns(true);
```

Particularly useful when testing a loop that should call the dependency for a fixed set of SKUs. `It.IsNotNull<T>()` is a convenience shorthand for `It.Is<T>(v => v != null)`. In `Verify()`, `It.Is<T>()` allows asserting that only calls satisfying the predicate were made — for example, confirming the SUT never passed a non-positive quantity to `Reserve`:

```csharp
inventoryMock.Verify(
    i => i.Reserve(It.IsAny<string>(), It.Is<int>(q => q <= 0)),
    Times.Never);
```

Choosing between the three: use `It.IsAny` when the argument is irrelevant to the assertion, `It.IsIn` when enumerating a fixed set of valid inputs, and `It.Is` for range, structural, or string-pattern checks.

---

## Q14. How do you simulate exceptions thrown by a dependency using Moq?

**Concepts**
- Throws() setup
- exception propagation testing
- ThrowsAsync for async methods
- SetupSequence for transient failures
- unhappy path verification

**Answer**

`Throws()` chains onto a `Setup()` in place of `Returns()` and causes the proxy to throw the specified exception whenever the matching call occurs:

```csharp
inventoryMock
    .Setup(i => i.Reserve("WIDGET-01", 2))
    .Throws(new InvalidOperationException("Warehouse offline"));
```

When `OrderService.PlaceOrder()` calls `Reserve`, the proxy throws `InvalidOperationException`. The test then asserts that either the SUT re-throws, wraps it in a domain exception, or handles it gracefully. Using xUnit's `Assert.Throws<InvalidOperationException>` confirms propagation:

```csharp
Assert.Throws<InvalidOperationException>(() => sut.PlaceOrder(order));
```

If the SUT should swallow the exception and send a failure notification instead, the test asserts no exception escapes and then verifies the email mock was called:

```csharp
sut.PlaceOrder(order); // must not throw
emailMock.Verify(e => e.Send(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
```

`ThrowsAsync<T>()` is the async counterpart for `Task`-returning methods. `SetupSequence()` lets you return a value on the first call and throw on the second, simulating transient failures useful for retry-logic tests. Exception simulation through `Throws()` is more predictable than relying on real infrastructure raising exceptions, because it decouples the test from external state and produces deterministic results.

---

## Q15. How does Callback() work in Moq and when should you use it?

**Concepts**
- Callback() argument capture
- side-effect simulation
- call sequence recording
- Callback chained before Returns
- list accumulation pattern

**Answer**

`Callback()` registers a delegate that Moq invokes when a matching call occurs, giving you access to the actual runtime arguments. The generic form `Callback<TArg1, TArg2>(...)` takes a delegate whose parameter types match the intercepted method signature:

```csharp
var reservedSkus = new List<string>();

inventoryMock
    .Setup(i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()))
    .Callback<string, int>((sku, quantity) => reservedSkus.Add($"{sku}:{quantity}"));
```

After `PlaceOrder()` runs, `reservedSkus` contains every `(sku, quantity)` pair passed to `Reserve` in call order, enabling assertions like `Assert.Equal(new[] { "WIDGET-01:2", "GADGET-05:1" }, reservedSkus)`. `Callback()` can precede `Returns()` in a chain — `.Callback(...).Returns(true)` — executing the capture and then returning the configured value. This is the standard approach for methods that both have side effects and return a value. A common misuse is letting `Callback()` replace what `Verify()` should handle: if you only need to confirm that `Reserve` was called once with specific arguments, `Verify()` is clearer. Reserve `Callback()` for cases where you need to capture arguments for downstream assertions, verify relative call ordering across multiple methods, or simulate stateful mutations on the mock's internal tracking data.

---

## Q16. What does NSubstitute offer and how does its syntax compare to Moq?

**Concepts**
- Substitute.For<T>() vs new Mock<T>().Object
- .Returns() call-chain syntax
- Received(n) and DidNotReceive()
- Arg.Any<T>() matcher
- Received.InOrder() for ordering

**Answer**

NSubstitute is a mocking library that favors a more fluent, less bracket-heavy syntax. Instead of `new Mock<T>()` plus `.Object`, NSubstitute produces the substitute directly:

```csharp
IInventoryService inventory = Substitute.For<IInventoryService>();
IEmailSender email = Substitute.For<IEmailSender>();
var sut = new OrderService(inventory, email);
```

Setup chains directly onto what looks like the real call:

```csharp
inventory.HasStock("WIDGET-01", 2).Returns(true);
inventory.HasStock("GADGET-05", 1).Returns(false);
```

Verification uses `Received()` inserted before the method expression:

```csharp
inventory.Received(1).Reserve("WIDGET-01", 2);
inventory.DidNotReceive().Reserve("GADGET-05", 1);
```

NSubstitute uses `Arg.Any<T>()` and `Arg.Is<T>(predicate)` as equivalents to Moq's `It.IsAny<T>()` and `It.Is<T>()`. For ordered interaction verification, `Received.InOrder(() => { inventory.Reserve("WIDGET-01", 2); inventory.Reserve("GADGET-05", 1); })` asserts the two calls happened in sequence — something Moq needs `Callback()` to replicate. Exception simulation uses `.Throws(new InvalidOperationException(...))` chained on the call expression. The choice between Moq and NSubstitute is primarily team style: Moq's lambda expression setup is IDE-refactoring friendly (renaming a method updates every Setup), while NSubstitute's direct-call style is more immediately readable. Both libraries work identically on .NET 10.

---

## Gotcha Questions

---

## Q17. Why does Moq silently return default values for unstubbed calls, and how can this hide production bugs?

**Concepts**
- Loose mock default behavior
- false positive tests
- unstubbed method returning false
- MockBehavior.Loose pitfall
- VerifyNoOtherCalls as safeguard

**Answer**

Moq's Loose mode returns `default(T)` for any method call without a matching setup: `false` for `bool`, `null` for objects, `0` for integers, and a completed `Task` for void-async methods. This is convenient but dangerous. Consider a test where the developer forgets to set up `HasStock` for the second order line:

```csharp
inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true);
// Missing: HasStock("GADGET-05", 1) — returns false by default
```

The SUT's all-or-nothing check sees `false` for `GADGET-05` and aborts the reservation. If the test only checks that `Reserve` was not called, it may still pass — but for the wrong reason. The test is accidentally testing the failure path when it intended to test the success path. Worse, if a code path proceeds on a `null` return without a null check, a missing setup can mask a `NullReferenceException` that only surfaces in production. The fix is to be explicit about every setup relevant to the test path. An intermediate safeguard is `inventoryMock.VerifyNoOtherCalls()` at the end of tests where you want to enumerate all expected calls exhaustively — this fails the test if the SUT made any call not previously verified, catching gaps without the full rigidity of `MockBehavior.Strict` during setup.

---

## Q18. What is over-specification in tests and why does it create fragile tests?

**Concepts**
- over-specification
- coupling to implementation details
- refactoring breakage
- testing behavior vs mechanism
- assertion granularity

**Answer**

Over-specification is the practice of asserting on interactions that are implementation details rather than observable contract requirements. For `OrderService`, the contract is: when all lines have stock, reserve all of them and send a confirmation; when any line lacks stock, reserve nothing. The implementation may check stock sequentially, in parallel, or grouped by warehouse — those are internal concerns. An over-specified test adds `Verify` assertions on the exact order of `HasStock` calls, asserts that `HasStock` was called exactly twice (hard-coding the number of lines), or checks that `HasStock` was called before `Reserve`. These assertions break when a developer refactors the iteration strategy, even though the observable behavior is unchanged. The result is a test suite that resists change: every refactoring requires updating tests that should never have been affected, which trains the team to distrust the suite and skip it before merging. The guideline is to verify outcomes — state changes and commands issued to external boundaries — rather than the sequence of internal decisions. In the `OrderService` scenario the right assertions are that `Reserve` was called for every in-stock SKU and never for any out-of-stock SKU, not the precise number of times `HasStock` was queried or the order those queries occurred.

---

## Q19. What happens when you call Verify() on a method that was never set up, and when is that a valid pattern?

**Concepts**
- Verify without Setup
- call log independence from setup
- command verification pattern
- void-return method testing
- Loose mock call recording

**Answer**

In Moq, `Verify()` checks the call log independently of whether a `Setup()` was registered. Because Loose mocks record every intercepted call — including calls with no matching setup — you can call `Verify()` on a method that was never configured. This is a valid and common pattern when you are testing a command method (void return or no meaningful return value) and do not need to control its output, only confirm it was invoked. For example, `IEmailSender.Send()` likely returns `void`. No `Setup()` is needed, but the call should still be verified:

```csharp
// No Setup for emailMock.Send()
sut.PlaceOrder(order);
emailMock.Verify(e => e.Send(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
```

Moq records the call to `Send` on the Loose proxy and `Verify()` finds it in the log. The gotcha runs in the opposite direction: calling `Verify(..., Times.Once)` when the method was never called rightly fails — but confusing "not set up" with "not called" is a common authoring mistake. Remember that `Setup()` controls what the mock returns; the call log is maintained regardless of setup. A missing setup on a void method (under Loose mode) does not prevent the method from being recorded — it simply means the proxy does nothing when called, which for void methods is exactly the right default.

---

## Q20. Why can you not mix literal arguments and It.* matchers in the same Setup() or Verify() call?

**Concepts**
- argument matcher mixing constraint
- thread-local matcher queue
- InvalidOperationException at runtime
- It.Is<T> workaround
- consistent matching mode rule

**Answer**

Moq's argument matching is implemented through a thread-local queue of `IArgumentMatcher` instances. When the expression tree in `Setup(i => i.HasStock("WIDGET-01", It.IsAny<int>()))` is compiled and evaluated to capture matchers, the literal `"WIDGET-01"` does not go through the matcher queue — it is captured directly as a value — while `It.IsAny<int>()` enqueues a wildcard matcher. At the point Moq reads the setup expression it finds one queued matcher and one literal, which it cannot reconcile consistently, resulting in `InvalidOperationException` or a matcher-out-of-order error depending on the Moq version. The fix is to wrap the literal in an `It.Is<T>` predicate so both arguments go through the matcher queue:

```csharp
inventoryMock.Setup(i => i.HasStock(
    It.Is<string>(s => s == "WIDGET-01"),
    It.IsAny<int>()
)).Returns(true);
```

Alternatively, when both argument values are fixed, use two literals and no matchers at all — Moq handles all-literal setups through equality comparison. The mixing restriction applies identically to `Verify()`. Learning this rule early prevents a class of runtime errors that produce confusing messages not obviously related to argument matching.

---

## Q21. What happens when you use Returns() instead of ReturnsAsync() for an async dependency method?

**Concepts**
- Returns() vs ReturnsAsync()
- Task<T> wrapping requirement
- NullReferenceException on await
- Returns(Task.CompletedTask) for void async
- async setup correctness

**Answer**

When a mocked interface method returns `Task<bool>`, the setup must return a `Task<bool>`, not a raw `bool`. `ReturnsAsync(true)` creates `Task.FromResult(true)` and configures the mock correctly. `Returns(true)` attempts to use `bool` as the return value where `Task<bool>` is expected; it compiles because `Returns()` accepts `object`, but at interception time Moq casts the stored value to the method's declared return type. Since `bool` is not assignable to `Task<bool>`, the result is a `NullReferenceException` or `InvalidCastException` when the SUT awaits. The correct patterns:

```csharp
// For Task<bool>
inventoryMock.Setup(i => i.HasStockAsync("WIDGET-01", 2)).ReturnsAsync(true);

// For Task (void async)
emailMock.Setup(e => e.SendAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

// For exception on async method
inventoryMock
    .Setup(i => i.ReserveAsync("WIDGET-01", 2))
    .ThrowsAsync(new InvalidOperationException("Warehouse offline"));
```

For `ValueTask<bool>`, use `Returns(new ValueTask<bool>(true))` in older Moq versions; recent Moq releases allow `ReturnsAsync` on `ValueTask` as well. The simplest rule to internalize: check the interface method's return type signature before writing the setup. If it starts with `Task` or `ValueTask`, use the `Async` variants.

---

## Real-World Scenario Questions

---

## Q22. How do you write a complete Moq test for OrderService.PlaceOrder() that enforces the all-or-nothing business rule?

**Concepts**
- all-or-nothing reservation
- multi-setup coordination
- Times.Once per SKU
- Times.Never on failure path
- Arrange / Act / Assert structure

**Answer**

The all-or-nothing rule requires two distinct test cases: one where every line has stock (all reserves happen) and one where at least one line is out of stock (no reserves happen). The happy-path test:

```csharp
[Fact]
public void PlaceOrder_AllLinesInStock_ReservesEveryLineAndSendsEmail()
{
    var inventoryMock = new Mock<IInventoryService>();
    var emailMock = new Mock<IEmailSender>();
    var sut = new OrderService(inventoryMock.Object, emailMock.Object);

    inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true);
    inventoryMock.Setup(i => i.HasStock("GADGET-05", 1)).Returns(true);

    var order = new Order(new[]
    {
        new OrderLine("WIDGET-01", 2),
        new OrderLine("GADGET-05", 1)
    });

    sut.PlaceOrder(order);

    inventoryMock.Verify(i => i.Reserve("WIDGET-01", 2), Times.Once);
    inventoryMock.Verify(i => i.Reserve("GADGET-05", 1), Times.Once);
    emailMock.Verify(e => e.Send(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
}
```

The failure-path test replaces the second `HasStock` setup with `Returns(false)` and verifies `Times.Never` for any `Reserve` call — not just the out-of-stock SKU but all SKUs, because the atomicity rule forbids partial reservation:

```csharp
[Fact]
public void PlaceOrder_AnyLineOutOfStock_ReservesNoLines()
{
    inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true);
    inventoryMock.Setup(i => i.HasStock("GADGET-05", 1)).Returns(false);

    sut.PlaceOrder(order);

    inventoryMock.Verify(
        i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()),
        Times.Never);
}
```

Using `It.IsAny` in the failure assertion is intentional: regardless of which SKU, zero reserves must occur. This pair of tests constitutes a behavioral contract — any implementation of `PlaceOrder` that violates the all-or-nothing rule breaks at least one of them.

---

## Q23. Review the following test. What defects does it contain?

```csharp
[Fact]
public void PlaceOrder_OneLineOutOfStock_DoesNotReserveOutOfStockItem()
{
    var inventoryMock = new Mock<IInventoryService>();
    var emailMock = new Mock<IEmailSender>();
    var sut = new OrderService(inventoryMock.Object, emailMock.Object);

    inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true);
    inventoryMock.Setup(i => i.HasStock("GADGET-05", 1)).Returns(false);

    var order = new Order(new[]
    {
        new OrderLine("WIDGET-01", 2),
        new OrderLine("GADGET-05", 1)
    });

    sut.PlaceOrder(order);

    inventoryMock.Verify(
        i => i.Reserve("GADGET-05", 1), Times.Never);
}
```

**Concepts**
- incomplete assertion
- all-or-nothing enforcement gap
- Times.Never on only one SKU
- missing notification verification
- misleading test name

**Answer**

This test contains three distinct defects.

| Category | Problem | Impact |
|---|---|---|
| Incomplete Assertion | Only verifies GADGET-05 was not reserved; no assertion on WIDGET-01 | A broken implementation that reserves WIDGET-01 anyway passes this test, violating the all-or-nothing rule entirely undetected |
| Missing Behavior Check | No verification of the failure notification path on `emailMock` | The notification side-effect is unspecified, allowing silent regressions in customer communication |
| Misleading Test Name | Name says "DoesNotReserveOutOfStockItem" implying partial reservation is acceptable | Developers reading this test conclude that reserving in-stock items while skipping out-of-stock ones is the intended contract |

**Fix priority:**

1. Replace the single-SKU `Times.Never` with `inventoryMock.Verify(i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()), Times.Never)` to assert that no line — including WIDGET-01 — was reserved. This is the critical fix because it is the only assertion that enforces atomicity.
2. Add `emailMock.Verify(e => e.Send(It.IsAny<string>(), It.IsAny<string>()), Times.Once)` if the business rule requires a failure notification, or `Times.Never` if the SUT should silently abort. Either assertion makes the notification contract explicit rather than unspecified.
3. Rename the test to `PlaceOrder_AnyLineOutOfStock_ReservesNoLines` to accurately communicate that the invariant is atomicity, not "out-of-stock items are individually skipped."

---

## Q24. When should you prefer manual test doubles (StubInventoryService, FakeEmailSender) over Moq?

**Concepts**
- manual double vs mocking library
- reuse across test classes
- FakeEmailSender state inspection
- Moq setup complexity threshold
- library-free doubles

**Answer**

Manual test doubles are the right choice in three situations. First, when the double needs business logic that is nontrivial to express with `Setup()` chains — `StubInventoryService` with dictionary-backed stock management and a `Reserve` method that mutates counts is easier to reason about than a Moq mock that must update a captured variable through `Callback()`. Second, when the same double will be reused across many test classes: a shared `FakeEmailSender` that accumulates `SentMessages` can be asserted against by any test, whereas Moq requires each test to configure and verify independently. Third, when the team includes developers unfamiliar with Moq's API, because a plain C# class is universally readable without framework knowledge. `FakeEmailSender` checking `email.SentMessages.Count == 1` is state verification — appropriate when the double has observable state worth inspecting. The `StubInventoryService` allowing `Assert.Equal(8, stub.Stock["WIDGET-01"])` after a reservation is a post-reservation state check that Moq cannot express directly. The tradeoff is maintenance cost: manual doubles require authoring and updating additional files as the interface evolves. Use Moq when interaction verification (which method was called, with which arguments, how many times) is the primary concern; use manual doubles when the double itself has observable state that tests need to inspect, or when setup complexity would make the test harder to read than a custom class.

---

## Q25. How do you verify the order in which Reserve() was called for multiple SKUs using Moq?

**Concepts**
- call order verification
- Callback() for sequencing
- list accumulation pattern
- MockSequence in Moq
- NSubstitute Received.InOrder()

**Answer**

Moq does not natively assert call sequence through `Verify()` — `Times` counts occurrences, not order. The recommended approach is `Callback()` to record calls and then assert on the resulting list:

```csharp
var callOrder = new List<string>();

inventoryMock
    .Setup(i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()))
    .Callback<string, int>((sku, _) => callOrder.Add(sku));

inventoryMock
    .Setup(i => i.HasStock(It.IsAny<string>(), It.IsAny<int>()))
    .Returns(true);

sut.PlaceOrder(order);

Assert.Equal(new[] { "WIDGET-01", "GADGET-05" }, callOrder);
```

`MockSequence` in Moq 4.x provides an alternative: setup calls against a sequence object and Moq throws if they fire out of declared order:

```csharp
var seq = new MockSequence();
inventoryMock.InSequence(seq).Setup(i => i.Reserve("WIDGET-01", 2));
inventoryMock.InSequence(seq).Setup(i => i.Reserve("GADGET-05", 1));
```

In NSubstitute the equivalent is `Received.InOrder(() => { inventory.Reserve("WIDGET-01", 2); inventory.Reserve("GADGET-05", 1); })`. Before committing to order verification, evaluate whether order is part of the contract or an implementation detail. If `OrderService` may parallelize reservations in a future version, an order assertion will break even though behavior is correct. Prefer asserting that each SKU was reserved exactly once, without order, unless the business rule explicitly requires sequential processing.

---

## Q26. When should you NOT mock a collaborator, and what types of objects are poor candidates for mocking?

**Concepts**
- value objects
- pure functions
- simple DTOs
- over-mocking
- test behavior not internals

**Answer**

Mocking is valuable when a collaborator is an external dependency, has side effects, is slow, or is nondeterministic. It adds overhead and complexity when the collaborator has none of those properties. Value objects — types like `Money`, `Address`, or `OrderLine` — are pure data containers; constructing one directly in the test is simpler and clearer than mocking it. Pure static utility methods such as `PriceCalculator.Compute(lines)` have no state, no side effects, and deterministic output; mocking them hides the actual calculation from the assertion and makes the test less informative. Simple DTOs like `Order` and `OrderLine` should always be constructed directly. Framework types like `ILogger<T>` are sometimes mocked to assert diagnostic output, but using `NullLogger<T>.Instance` is usually sufficient and avoids fragile assertions on log message strings that can change for non-behavioral reasons. The principle is: mock the boundary, not the interior. `IInventoryService` and `IEmailSender` represent the process boundary — external I/O and infrastructure — making them correct mocking candidates. An internal helper class like a `StockValidator` that contains pure allocation logic and is always created by `OrderService` is an implementation detail; it should be tested through `OrderService`, not mocked out. Over-mocking produces tests that assert on internal wiring rather than behavior, leave real logic un-exercised, and give false confidence in coverage metrics while resisting refactoring.

---

## Q27. How do you migrate an OrderService Moq test suite to NSubstitute while preserving behavioral coverage?

**Concepts**
- NSubstitute migration mapping
- Substitute.For<T>() vs new Mock<T>().Object
- Received(n) vs Verify Times.Once
- DidNotReceive() vs Times.Never
- Arg.Any<T>() vs It.IsAny<T>()

**Answer**

The migration is largely mechanical but requires attention to three translation points. First, substitute creation: replace `new Mock<IInventoryService>()` with `Substitute.For<IInventoryService>()`. The result is the substitute itself, not a wrapper — there is no `.Object`; pass the substitute directly to `OrderService`'s constructor. Second, setup translation: Moq's `mock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true)` becomes `inventory.HasStock("WIDGET-01", 2).Returns(true)` — the configure call resembles the real call, which improves readability but relies on NSubstitute's interception happening during the `Returns` chain. Third, verification translation: `inventoryMock.Verify(i => i.Reserve("WIDGET-01", 2), Times.Once)` becomes `inventory.Received(1).Reserve("WIDGET-01", 2)`, and `Times.Never` becomes `inventory.DidNotReceive().Reserve(...)`. Argument matchers map directly: `It.IsAny<string>()` becomes `Arg.Any<string>()`, and `It.Is<string>(s => s.StartsWith("W"))` becomes `Arg.Is<string>(s => s.StartsWith("W"))`. Exception simulation: `.Throws<T>()` chains identically. `MockBehavior.Strict` has no direct NSubstitute equivalent; the closest approach is combining `Received()` checks with `inventory.ReceivedCalls()` inspection. After translating every test, run the full suite before committing the migration to confirm all tests still pass — NSubstitute's interception model is equivalent to Moq's under .NET 10, so failures indicate translation errors rather than library capability gaps.
