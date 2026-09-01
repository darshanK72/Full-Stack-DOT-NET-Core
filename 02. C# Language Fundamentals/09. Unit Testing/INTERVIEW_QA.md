# Unit Testing — Interview Q&A Index

This index covers unit testing in .NET 10, from foundational concepts through framework-specific tooling and mocking strategies. Browse each subfolder for focused Q&A, or use the cross-cutting questions below for topics that span multiple areas.

---

## Table of Contents

| Subfolder | Focus |
|---|---|
| [01. Unit Testing Basics](01.%20Unit%20Testing%20Basics/INTERVIEW_QA.md) | AAA pattern, test naming, assertions, test organization |
| [02. xUnit](02.%20xUnit/INTERVIEW_QA.md) | `[Fact]`, `[Theory]`, `IClassFixture`, constructor lifecycle |
| [03. MSTest](03.%20MSTest/INTERVIEW_QA.md) | `[TestClass]`, `[TestInitialize]`, `[DataTestMethod]`, lifecycle attributes |
| [04. Mocking & Test Doubles](04.%20Mocking%20%26%20Test%20Doubles/INTERVIEW_QA.md) | Moq, NSubstitute, stubs, fakes, spies, verification |

---

## Cross-Cutting Questions

---

## CQ1. How do you choose between xUnit and MSTest for a new .NET 10 project?

**Concepts**
- Framework defaults — xUnit is the ASP.NET Core / SDK template default
- Lifecycle model — xUnit constructor/`IDisposable` vs MSTest `[TestInitialize]`/`[TestCleanup]`
- Shared state — `IClassFixture<T>` (xUnit) vs `[ClassInitialize]` (MSTest)
- Parallel execution — xUnit runs test classes in parallel by default; MSTest requires opt-in
- Ecosystem fit — MSTest is preferred in enterprises with Azure DevOps and Visual Studio-centric workflows

**Answer**

For a greenfield .NET 10 project, xUnit is the natural default: it ships as the test framework in `dotnet new` templates for ASP.NET Core, aligns with the open-source .NET ecosystem, and enforces good isolation by design. Each test class is instantiated fresh per test, which makes shared-state bugs obvious early. Parallel execution is on by default and scoped at the collection level via `[Collection]`, giving fine-grained control without extra configuration.

MSTest remains a strong choice in enterprise contexts. Its `[TestInitialize]` and `[ClassInitialize]` lifecycle hooks are familiar to teams coming from Visual Studio's built-in test runner, and Azure DevOps result reporting integrates natively with MSTest's TRX format. MSTest v3 (current for .NET 10) has closed most of the gaps — it now supports `[DataTestMethod]` with `[DynamicData]` that rivals xUnit's `[Theory]`, and parallel execution is available via `[Parallelize]`.

The decisive factor is usually team convention. If the project will live in an Azure DevOps pipeline with VSTest and the team is already fluent in MSTest, stay there. If you are starting from scratch, or the project is open-source, xUnit's cleaner isolation model and richer community documentation make it the better starting point for .NET 10.

---

## CQ2. How does test isolation strategy interact with mocking setup across xUnit and MSTest?

**Concepts**
- xUnit constructor injection — mock is created and passed per test, no shared state
- MSTest `[TestInitialize]` — mock recreated in a method called before each test
- Mock field lifetime — risk of cross-test contamination if not reset
- `MockBehavior.Strict` vs `MockBehavior.Loose` — isolation tightness
- `AutoMock` / `AutoFixture` — generating fresh mocks automatically

**Answer**

xUnit and MSTest differ in where you initialize mocks, and that difference has real consequences for isolation. In xUnit, the test class constructor runs once per test method, so declaring `var mockRepo = new Mock<IRepository>()` in the constructor guarantees a fresh mock for every test. There is no shared instance and no reset call needed. In MSTest, the equivalent is a field initialized inside `[TestInitialize]`; if you initialize the mock at field-declaration time instead, the same instance is reused, and `Verifiable` setups or captured call counts can bleed across tests.

The safest pattern in both frameworks is to create mocks in the per-test setup point — constructor for xUnit, `[TestInitialize]` for MSTest — and pass them into the system under test via constructor injection. This directly mirrors the interface-based design principle: the SUT depends on an abstraction, the test supplies the mock at construction, and the mock's behavior is configured inside the Arrange step of AAA. Using `MockBehavior.Strict` tightens isolation further by failing immediately on any unexpected call, which catches setup drift between tests. For complex setups, `AutoFixture.AutoMoq` can generate fresh mock graphs automatically, reducing boilerplate while maintaining per-test isolation across either framework.

---

## CQ3. When should you use unit tests with mocks versus integration tests without mocks?

**Concepts**
- Testing pyramid — unit (wide base), integration, E2E (narrow peak)
- Mock boundary — replace infrastructure (DB, HTTP, queue), never domain logic
- Integration test scope — real `DbContext`, real HTTP via `WebApplicationFactory<T>`
- Test speed and feedback loop — unit tests milliseconds, integration tests seconds
- Confidence vs isolation tradeoff

**Answer**

The testing pyramid gives the rule of thumb: write many fast unit tests at the base, fewer slower integration tests in the middle, and minimal end-to-end tests at the top. Unit tests with mocks are the right tool when you want to verify a single class's logic in complete isolation — the business rule, the calculation, the branching condition — without the latency or fragility of real infrastructure. Mocks let you simulate every edge case (network timeout, empty result set, exception from a dependency) deterministically and cheaply.

Integration tests without mocks earn their place when you need confidence that components wire together correctly: an EF Core query that must produce the right SQL, a repository that maps columns to domain objects, or an ASP.NET Core pipeline that must apply middleware in the correct order. For .NET 10 projects, `WebApplicationFactory<TEntryPoint>` and an in-memory or Testcontainers-backed `DbContext` give a real application host at acceptable speed. The key discipline is not to mock things just because you can. Mocking an `HttpClient` call in a unit test is appropriate; mocking `DbContext` in a test that is really checking an EF query is not — that should be an integration test with a real database connection. Use mocks to isolate logic; use integration tests to verify integration.

---

## CQ4. How do Moq/NSubstitute and interface-based design combine with the AAA pattern?

**Concepts**
- Dependency inversion — program to interfaces, inject via constructor
- AAA structure — Arrange (configure mock), Act (call SUT), Assert (verify outcome or interaction)
- `Setup` / `Returns` (Moq) vs `Returns` / `Throws` (NSubstitute) in Arrange
- `Verify` / `Received` in Assert — interaction assertions
- Single-responsibility of a test — one concept per Arrange/Act/Assert block

**Answer**

Interface-based design and AAA are natural partners for mocking. Because the system under test depends on an interface rather than a concrete type, the test can substitute a `Mock<IInterface>` (Moq) or an `Substitute.For<IInterface>()` (NSubstitute) at construction time. The Arrange step configures what the mock returns — `mockService.Setup(s => s.GetData(42)).Returns(fakeData)` in Moq, or `mockService.GetData(42).Returns(fakeData)` in NSubstitute. The Act step calls exactly one method on the SUT, invoking the real logic while the mock handles any outbound calls. The Assert step either checks the return value or state change on the SUT, or verifies an interaction on the mock — `mockRepo.Verify(r => r.Save(It.IsAny<Order>()), Times.Once)` in Moq, or `mockRepo.Received(1).Save(Arg.Any<Order>())` in NSubstitute.

The discipline to maintain is single-concept per test: one Arrange configuration, one Act invocation, one logical assertion. If you find yourself configuring four different mock setups to cover multiple paths in one test, that is a signal to split. Interaction assertions (Verify / Received) should appear only when the call itself is the behaviour being tested — for example confirming an audit log was written. When the observable outcome is a return value or a state change, assert on that directly and leave the mock's internal call log alone.

---

## CQ5. How does the test double taxonomy (stub/mock/fake/spy/dummy) map to xUnit + Moq patterns?

**Concepts**
- Dummy — satisfies a parameter signature, never called
- Stub — returns canned data, no verification
- Mock — pre-programmed expectations verified after the act
- Spy — records calls, verified after the act (subset of mock)
- Fake — working implementation with shortcuts (e.g., in-memory repository)
- Moq as a unified tool — can act as stub, mock, or spy depending on usage

**Answer**

Gerard Meszaros's taxonomy maps cleanly onto everyday xUnit and Moq usage. A **dummy** is a `null` argument or `new Mock<ILogger>().Object` passed to a constructor just to satisfy the signature when the test never exercises that dependency. A **stub** is a `Mock<T>` with only `Setup(...).Returns(...)` configured and no `Verify` call — you only care about what comes back, not whether the dependency was called. A **mock** adds `Verify` (or `VerifyAll`) after the Act step, turning the object into a pre-programmed expectation that the test will assert against. A **spy** is the same thing at a finer grain — `mockLogger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once)` checks that a specific interaction happened without asserting the full contract. A **fake** is a hand-written class with real logic but simplified infrastructure — an `InMemoryRepository` that stores items in a `Dictionary<int, T>` is the canonical example; Moq cannot generate these, so they live as test-project classes.

In practice, a single `Mock<T>` object in Moq can play the stub role in one test and the mock role in another depending on whether you call `Verify`. Keeping the role clear within a test makes intent obvious: if a test configures `Setup` but never calls `Verify`, it is using the object as a stub; when `Verify` appears, it is a mock assertion. Using NSubstitute, the same distinction holds — `Returns` alone is stub behaviour; `Received()` switches the object's role to a mock in that assertion context.
