# 09. Unit Testing — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. Unit Testing Basics](#01-unit-testing-basics)
  - [Q1. What is unit testing, and how does it differ from integration, component, and end-to-end testing?](#q1-what-is-unit-testing-and-how-does-it-differ-from-integration-component-and-end-to-end-testing)
  - [Q2. Explain the AAA pattern (Arrange, Act, Assert) and why order matters psychologically for readers.](#q2-explain-the-aaa-pattern-arrange-act-assert-and-why-order-matters-psychologically-for-readers)
  - [Q3. What makes a good unit test (FIRST / TRICE — fast, isolated, repeatable, self-validating, timely)?](#q3-what-makes-a-good-unit-test-first--trice--fast-isolated-repeatable-self-validating-timely)
  - [Q4. What is the System Under Test (SUT), and how do you identify its boundaries?](#q4-what-is-the-system-under-test-sut-and-how-do-you-identify-its-boundaries)
  - [Q5. What is test coverage, and why can 100% line coverage still miss important bugs?](#q5-what-is-test-coverage-and-why-can-100-line-coverage-still-miss-important-bugs)
  - [Q6. What is mutation testing, and how does it critique coverage metrics?](#q6-what-is-mutation-testing-and-how-does-it-critique-coverage-metrics)
  - [Q7. What is the difference between state-based and interaction-based testing?](#q7-what-is-the-difference-between-state-based-and-interaction-based-testing)
  - [Q8. What is a test fixture, and how is it different from a test case?](#q8-what-is-a-test-fixture-and-how-is-it-different-from-a-test-case)
  - [Q9. What are flaky tests, and what common causes (time, threading, shared state, external I/O)?](#q9-what-are-flaky-tests-and-what-common-causes-time-threading-shared-state-external-io)
  - [Q10. What is the test pyramid, and where do unit tests sit relative to integration tests?](#q10-what-is-the-test-pyramid-and-where-do-unit-tests-sit-relative-to-integration-tests)
  - [Q11. What is TDD (Red-Green-Refactor), and what benefits/challenges does it bring?](#q11-what-is-tdd-red-green-refactor-and-what-benefitschallenges-does-it-bring)
  - [Q12. When should a bug fix include a regression test?](#q12-when-should-a-bug-fix-include-a-regression-test)
  - [Q13. What is the difference between testing public behavior vs internal implementation?](#q13-what-is-the-difference-between-testing-public-behavior-vs-internal-implementation)
  - [Q14. How do deterministic tests handle `DateTime.Now`, `Guid.NewGuid()`, and randomness?](#q14-how-do-deterministic-tests-handle-datetimenow-guidnewguid-and-randomness)
  - [Q15. What is arrange duplication, and when is shared setup justified vs harmful?](#q15-what-is-arrange-duplication-and-when-is-shared-setup-justified-vs-harmful)

- [02. xUnit](#02-xunit)
  - [Q1. What is xUnit.net, and how does its philosophy differ from MSTest and NUnit?](#q1-what-is-xunitnet-and-how-does-its-philosophy-differ-from-mstest-and-nunit)
  - [Q2. Explain `[Fact]` vs `[Theory]` — when is parameterized testing appropriate?](#q2-explain-fact-vs-theory--when-is-parameterized-testing-appropriate)
  - [Q3. How do `[InlineData]`, `[MemberData]`, and `[ClassData]` supply theory inputs?](#q3-how-do-inlinedata-memberdata-and-classdata-supply-theory-inputs)
  - [Q4. How does xUnit create test class instances — per test or per class?](#q4-how-does-xunit-create-test-class-instances--per-test-or-per-class)
  - [Q5. What are `IClassFixture<T>` and `ICollectionFixture<T>`, and when use each?](#q5-what-are-iclassfixturet-and-icollectionfixturet-and-when-use-each)
  - [Q6. How do collection definitions (`[Collection("Name")]`) serialize tests that share expensive resources?](#q6-how-do-collection-definitions-collectionname-serialize-tests-that-share-expensive-resources)
  - [Q7. What is `IAsyncLifetime`, and how does it replace async setup/teardown patterns?](#q7-what-is-iasynclifetime-and-how-does-it-replace-async-setupteardown-patterns)
  - [Q8. How does xUnit handle parallel test execution by default, and how do you disable it?](#q8-how-does-xunit-handle-parallel-test-execution-by-default-and-how-do-you-disable-it)
  - [Q9. What is `ITestOutputHelper`, and how is it injected into tests?](#q9-what-is-itestoutputhelper-and-how-is-it-injected-into-tests)
  - [Q10. How do `[Trait("Category", "Slow")]` attributes help filter tests in CI?](#q10-how-do-traitcategory-slow-attributes-help-filter-tests-in-ci)
  - [Q11. How does constructor injection of dependencies work in xUnit test classes?](#q11-how-does-constructor-injection-of-dependencies-work-in-xunit-test-classes)
  - [Q12. What happens if a test constructor throws — how does xUnit report it?](#q12-what-happens-if-a-test-constructor-throws--how-does-xunit-report-it)
  - [Q13. How do you assert exceptions with `Assert.Throws<T>` vs `Assert.ThrowsAsync<T>`?](#q13-how-do-you-assert-exceptions-with-assertthrowst-vs-assertthrowsasynct)
  - [Q14. What is the difference between returning `Task` from a test vs `async void`?](#q14-what-is-the-difference-between-returning-task-from-a-test-vs-async-void)
  - [Q15. How do you run xUnit tests from CLI (`dotnet test`) and filter by fully qualified name?](#q15-how-do-you-run-xunit-tests-from-cli-dotnet-test-and-filter-by-fully-qualified-name)

- [03. MSTest](#03-mstest)
  - [Q1. What NuGet packages compose an MSTest project?](#q1-what-nuget-packages-compose-an-mstest-project-mstesttestframework-mstesttestadapter-microsoftnettestsdk)
  - [Q2. Explain `[TestClass]`, `[TestMethod]`, and how discovery finds tests.](#q2-explain-testclass-testmethod-and-how-discovery-finds-tests)
  - [Q3. What are `[DataTestMethod]` and `[DataRow]` equivalents to xUnit theories?](#q3-what-are-datatestmethod-and-datarow-equivalents-to-xunit-theories)
  - [Q4. What is `[TestInitialize]` / `[TestCleanup]` vs `[ClassInitialize]` / `[ClassCleanup]` vs `[AssemblyInitialize]` / `[AssemblyCleanup]`?](#q4-what-is-testinitialize--testcleanup-vs-classinitialize--classcleanup-vs-assemblyinitialize--assemblycleanup)
  - [Q5. Why must `[ClassInitialize]` and `[AssemblyInitialize]` be `static`?](#q5-why-must-classinitialize-and-assemblyinitialize-be-static)
  - [Q6. What is the `[TestContext]` property, and what runtime services does it expose?](#q6-what-is-the-testcontext-property-and-what-runtime-services-does-it-expose)
  - [Q7. How does MSTest instance lifecycle differ from xUnit's new-instance-per-test model?](#q7-how-does-mstest-instance-lifecycle-differ-from-xunits-new-instance-per-test-model)
  - [Q8. What are `[ExpectedException]` / `[ExpectedExceptionAttribute]` (legacy), and why is `Assert.ThrowsException` preferred?](#q8-what-are-expectedexception--expectedexceptionattribute-legacy-and-why-is-assertthrowsexception-preferred)
  - [Q9. What Assert helpers exist in MSTest (`Assert.AreEqual`, `Assert.IsTrue`, `Assert.ThrowsException`, `StringAssert`)?](#q9-what-assert-helpers-exist-in-mstest-assertareequal-assertistrue-assertthrowsexception-stringassert)
  - [Q10. How do you deploy test content files (`[DeploymentItem]`) — and what are modern alternatives?](#q10-how-do-you-deploy-test-content-files-deploymentitem--and-what-are-modern-alternatives)
  - [Q11. What is `[Ignore]` / `[TestCategory]`, and how do you filter categories in `dotnet test`?](#q11-what-is-ignore--testcategory-and-how-do-you-filter-categories-in-dotnet-test)
  - [Q12. How does MSTest parallelization work (`Parallelize` attribute at assembly/class level)?](#q12-how-does-mstest-parallelization-work-parallelize-attribute-at-assemblyclass-level)
  - [Q13. What is the difference between MSTest V1 and V2/V3 adapters in SDK-style projects?](#q13-what-is-the-difference-between-mstest-v1-and-v2v3-adapters-in-sdk-style-projects)
  - [Q14. When would teams choose MSTest over xUnit in greenfield .NET projects?](#q14-when-would-teams-choose-mstest-over-xunit-in-greenfield-net-projects)
  - [Q15. How do MSTest data sources (`[DynamicData]`) compare to xUnit `[MemberData]`?](#q15-how-do-mstest-data-sources-dynamicdata-compare-to-xunit-memberdata)

- [04. Mocking & Test Doubles](#04-mocking--test-doubles)
  - [Q1. Define the test double taxonomy — dummy, fake, stub, spy, mock.](#q1-define-the-test-double-taxonomy--dummy-fake-stub-spy-mock-what-distinguishes-each)
  - [Q2. What is a mock in the strict sense (interaction verification) vs informal "mock"?](#q2-what-is-a-mock-in-the-strict-sense-interaction-verification-vs-informal-mock-meaning-any-fake)
  - [Q3. What is a stub, and when do you configure return values without verifying calls?](#q3-what-is-a-stub-and-when-do-you-configure-return-values-without-verifying-calls)
  - [Q4. What is a spy, and how does it record interactions for later assertion?](#q4-what-is-a-spy-and-how-does-it-record-interactions-for-later-assertion)
  - [Q5. What is a fake (e.g., in-memory repository), and when is it preferable to mocks?](#q5-what-is-a-fake-eg-in-memory-repository-and-when-is-it-preferable-to-mocks)
  - [Q6. What is dependency injection's role in making code testable?](#q6-what-is-dependency-injections-role-in-making-code-testable)
  - [Q7. When should you use a mocking framework vs hand-written fakes?](#q7-when-should-you-use-a-mocking-framework-moq-nsubstitute-fakeiteasy-vs-hand-written-fakes)
  - [Q8. What is the difference between mocking an interface vs a concrete class?](#q8-what-is-the-difference-between-mocking-an-interface-vs-a-concrete-class)
  - [Q9. Why can't Moq intercept non-virtual methods on concrete classes?](#q9-why-cant-moq-intercept-non-virtual-methods-on-concrete-classes)
  - [Q10. What is `Mock<T>.Setup`, `Returns`, `Callback`, and `Verify` in Moq terms?](#q10-what-is-mocktsetup-returns-callback-and-verify-in-moq-terms)
  - [Q11. What is over-specification / brittle mocking?](#q11-what-is-over-specification--brittle-mocking-and-how-does-it-couple-tests-to-implementation)
  - [Q12. What is the difference between verifying behavior (`Verify`) vs asserting output state?](#q12-what-is-the-difference-between-verifying-behavior-verify-vs-asserting-output-state)
  - [Q13. How do you mock `async` methods returning `Task` / `Task<T>`?](#q13-how-do-you-mock-async-methods-returning-task--taskt)
  - [Q14. How do you substitute `HttpClient`, `ILogger<T>`, and `DateTime` abstractions in tests?](#q14-how-do-you-substitute-httpclient-iloggert-and-datetime-abstractions-in-tests)
  - [Q15. What is a seam, and how do partial wrappers or interfaces introduce testability?](#q15-what-is-a-seam-and-how-do-partial-wrappers-or-interfaces-introduce-testability)
  - [Q16. When is integration testing with real dependencies better than mocking everything?](#q16-when-is-integration-testing-with-real-dependencies-better-than-mocking-everything)
  - [Q17. What are anti-patterns: mocking concrete DB providers, verifying private collaborators, testing framework code?](#q17-what-are-anti-patterns-mocking-concrete-db-providers-verifying-private-collaborators-testing-framework-code)
  - [Q18. How do manual test doubles compare to framework mocks for readability?](#q18-how-do-manual-test-doubles-hand-rolled-stubs-compare-to-framework-mocks-for-readability)
  - [Q19. Testing private methods directly.](#q19-testing-private-methods-directly--usually-signals-a-design-problem-test-through-public-seams-or-extract-collaborators)
  - [Q20. Shared mutable static state.](#q20-shared-mutable-static-state--tests-pass-alone-but-fail-in-parallel-or-random-order-isolate-with-instance-state-or-collection-serialization)
  - [Q21. Mocking concrete classes with non-virtual members.](#q21-mocking-concrete-classes-with-non-virtual-members--framework-proxies-cannot-override-sealednon-virtual-methods-depend-on-interfaces)
  - [Q22. `async void` test methods.](#q22-async-void-test-methods--runners-may-not-observe-exceptions-always-return-task-from-async-tests)
  - [Q23. Over-verifying mock calls.](#q23-over-verifying-mock-calls--verify-on-every-internal-call-breaks-on-refactor-assert-outcomes-and-critical-interactions-only)
  - [Q24. Integration tests disguised as unit tests.](#q24-integration-tests-disguised-as-unit-tests--real-sqlfilenetwork-makes-tests-slow-and-flaky-name-and-folder-them-honestly)
  - [Q25. MSTest `[ClassInitialize]` sharing mutable state.](#q25-mstest-classinitialize-sharing-mutable-state--static-setup-mutated-by-one-test-leaks-into-others-unless-reset-in-testinitialize)
  - [Q26. xUnit class fixtures shared across unrelated tests.](#q26-xunit-class-fixtures-shared-across-unrelated-tests--fixture-lifetime-is-per-class-accidental-shared-state-causes-order-dependent-failures)
  - [Q27. Theory data referencing mutable objects.](#q27-theory-data-referencing-mutable-objects--shared-arraylist-mutated-in-one-run-corrupts-later-theory-cases)
  - [Q28. Not awaiting async assertions.](#q28-not-awaiting-async-assertions--assertthrowsasync-must-be-awaited-fire-and-forget-hides-failures)
  - [Q29. Assuming test execution order.](#q29-assuming-test-execution-order--xunit-and-parallel-mstest-do-not-guarantee-order-tests-must-be-independent)
  - [Q30. Confusing stub with mock.](#q30-confusing-stub-with-mock--stubs-set-up-responses-mocks-strict-sense-verify-interactions--mixing-terms-leads-to-wrong-test-design)

---

### 01. Unit Testing Basics

---

## Q1. What is unit testing, and how does it differ from integration, component, and end-to-end testing?

**Concepts**
- Single-unit isolation boundary
- Test double substitution for collaborators
- Test pyramid layer ratios
- Feedback loop speed by layer
- Failure diagnosis specificity

**Answer**

Unit testing exercises one class or function in complete isolation from its collaborators, which means every external dependency is replaced with a test double so that a failure points to a single cause. Since the scope is narrow, each test runs in milliseconds and pinpoints the exact method that broke. Integration tests span multiple real components — a service plus its real database, or two services communicating over HTTP — which makes them slower and occasionally flaky but proves that components wire together correctly. Component tests sit between those levels, exercising a cluster of cooperating classes against lightweight in-process fakes rather than a full external system. End-to-end tests drive the full deployed stack through a browser or API gateway, providing the highest confidence about real user flows but also the most maintenance burden and the slowest feedback. The test pyramid captures the ideal ratio: many cheap unit tests at the base, a moderate number of integration tests in the middle, and a small number of end-to-end tests at the top — because each higher layer costs exponentially more per test in time, infrastructure, and flakiness. When a unit test suite is healthy, a developer can tell within seconds which class introduced a regression, so the pyramid deliberately weights the cheapest, most diagnostic layer most heavily.

---

## Q2. Explain the AAA pattern (Arrange, Act, Assert) and why order matters psychologically for readers.

**Concepts**
- Arrange-Act-Assert phase separation
- Cognitive load for reviewers
- Single behavior per test
- Missing-assert detection
- Blank-line visual structure

**Answer**

AAA divides every test into three ordered phases. In Arrange I build all the prerequisites — the SUT, its injected doubles, and any input data. In Act I invoke exactly one behavior under test and capture its result. In Assert I verify the outcome against a fixed expected value. The order matters because readers scan top to bottom: when Arrange comes first they have the full context before the action, which maps to the cause-and-effect way humans reason about a scenario. Mixing phases — asserting in the middle of act, or re-arranging state after the first assert — forces the reader to hold multiple mental contexts simultaneously, which slows review and makes it easy to miss an assertion gap. A test with no Assert phase will still pass even if the SUT returns a completely wrong result, since "no exception thrown" is not a specification unless throwing is the only failure mode. Using blank lines or `// Arrange / Act / Assert` comments as section separators enforces the boundary visually, so a code reviewer can immediately spot a missing Assert section.

---

## Q3. What makes a good unit test (FIRST / TRICE — fast, isolated, repeatable, self-validating, timely)?

**Concepts**
- FIRST properties mnemonic
- Test isolation via doubles
- Repeatability across environments
- Self-validating assertion requirement
- Refactor-resistant public-surface testing

**Answer**

A good unit test satisfies the FIRST properties. Fast means it runs in milliseconds, not seconds, so the full suite finishes in a time that doesn't break a developer's flow. Isolated means each test has no dependency on the order or outcome of other tests, no shared mutable state, and no live external resources — every collaborator is replaced with a test double. Repeatable means the test produces the same pass/fail result on any machine, in any environment, at any time — which rules out `DateTime.Now`, random seeds, and fixed file paths. Self-validating means the test asserts a specific expected value and passes or fails without human inspection; a test that prints output and requires someone to read the log is not self-validating. Timely means the test is written alongside (or before) the production code it covers, not months later when the logic is hard to remember. Isolation is the hardest property to maintain because it requires replacing every collaborator that touches a clock, network, or database — but without it, a test that fails for environmental reasons wastes more time than it saves.

---

## Q4. What is the System Under Test (SUT), and how do you identify its boundaries?

**Concepts**
- SUT definition and scope
- Public constructor as entry point
- Dependency as collaborator outside boundary
- Constructor injection as seam indicator
- SUT boundary vs collaborator boundary

**Answer**

The SUT is the single class (or minimal cluster) I am directly testing in a given test — everything else is either an input value, an output to assert on, or a test double. I identify the SUT's boundary by its public constructor and public methods: those are the only entry points and exit points I should interact with. Any class the SUT receives through its constructor or method parameters is a dependency and lives outside the boundary, so it should be substituted with a test double to keep the test focused. A useful heuristic is constructor injection: if I have to pass a collaborator into the SUT's constructor, that collaborator is a seam where a test double can be inserted. Widening the SUT boundary to include a real database or HTTP service turns the test into an integration test — it slows down, becomes fragile, and when it fails the failure could originate from any layer. Keeping the boundary tight means a red test means "the code in this specific class is wrong," which is the core diagnostic value of unit testing.

---

## Q5. What is test coverage, and why can 100% line coverage still miss important bugs?

**Concepts**
- Line coverage metric
- Branch coverage distinction
- Coverage as execution metric not correctness metric
- Boundary gap problem
- False confidence from assertionless tests

**Answer**

Test coverage measures the fraction of production code lines (or branches, or paths) that were executed during a test run. 100% line coverage means every statement was reached at least once, but it says nothing about whether every interesting input value was exercised — because a single call to `GetLetterGrade(72)` might execute all five grade-band if-else branches while never testing the score-90 cutoff where the A/B boundary sits. Branch coverage is stricter (every true and false path through every condition must be hit) but still cannot detect a wrong threshold value since it counts paths, not the correctness of values on each path. The deeper problem is that coverage counts executions, not assertions — a test that calls a method and asserts nothing inflates coverage to 100% while protecting zero behavior. A QA-reported grading bug can ship through green tests if the only covered inputs are 72 and 95 (mid-range values) while the off-by-one boundaries at 90, 80, 70, and 60 were never tested. Healthy coverage requires both the metric and a discipline of asserting at boundary values rather than relying on arbitrary mid-range cases.

---

## Q6. What is mutation testing, and how does it critique coverage metrics?

**Concepts**
- Mutation operator definition
- Mutant survival as test weakness signal
- Mutation score metric
- Coverage vs assertion quality distinction
- Computational cost trade-off

**Answer**

Mutation testing automatically inserts tiny code changes — flipping `>=` to `>`, replacing `+` with `-`, negating a boolean return — and then re-runs the full test suite against each mutated version. If no test fails after a mutation, the mutant "survives," which means the suite does not assert anything that distinguishes the correct logic from the broken version. A high mutation score (fraction of mutants killed by failing tests) indicates that assertions are tightly coupled to logic; a low score exposes suites that execute code without pinning its meaning. This directly critiques line coverage: a test that calls `IsPassing(72)` and asserts the result can survive a `>=` to `>` mutation on the 60 threshold because the mid-range value of 72 produces the same result either way — only a test that asserts on 60 itself would kill that mutant. Mutation testing is computationally expensive since it recompiles and re-runs for each mutant, so teams typically apply it nightly or on high-risk modules such as pricing rules and validation logic, using it to prioritize where to add boundary assertions.

---

## Q7. What is the difference between state-based and interaction-based testing?

**Concepts**
- State verification on output values
- Interaction verification on mock calls
- Black-box vs white-box test coupling
- Refactor safety of state-based tests
- Appropriate use of Verify

**Answer**

State-based testing asserts on the observable outcome after acting on the SUT — the return value, a thrown exception, or changed state on an object I hold a reference to. Interaction-based testing asserts that the SUT called its collaborators in a specific way, using mock expectations or spy records to verify method invocations, arguments, and call counts. State-based tests are more resilient to refactoring because they treat the SUT as a black box: as long as the return value or resulting state is correct, the internal path doesn't matter, so I can replace a nested-if algorithm with a dictionary lookup without breaking a test. Interaction-based tests are necessary when a side effect — sending an email, publishing to a queue, writing an audit log — produces no observable state I can inspect within a unit test's scope, so the only way to assert that the action happened is to verify the call on a mock. The risk of over-relying on interaction verification is that tests couple to the current call graph, so a harmless internal refactor (merging two loops into one) fails CI even though the customer-visible behavior is unchanged. I prefer state-based assertions wherever possible and use `Verify` only for essential collaborations that cross a meaningful boundary.

---

## Q8. What is a test fixture, and how is it different from a test case?

**Concepts**
- Test fixture as shared precondition set
- Test case as individual scenario method
- Fixture lifetime vs test case lifetime
- Immutability requirement for shared fixtures
- xUnit IClassFixture lifetime model

**Answer**

A test fixture is the set of preconditions — objects, data, and configured dependencies — that must be in place before a test can execute. In xUnit, a fixture class (used via `IClassFixture<T>`) provides expensive shared setup such as a compiled service host or a seeded catalog, constructed once and shared across all test methods in a class. A test case is a single method decorated with `[Fact]` or a `[Theory]` row that exercises one specific scenario and asserts on one expected outcome. The distinction matters because they have different lifetimes: a fixture lives for the duration of all tests in a class (or collection), while a test case's local variables live only for that single invocation. Mutable state on a shared fixture leaks from one test case into another because the fixture is not reset between them, which is why fixtures should be immutable after construction — read-only seeded data is safe to share, but a mutable list on the fixture that one test appends to will corrupt subsequent tests that expect an empty list.

---

## Q9. What are flaky tests, and what common causes (time, threading, shared state, external I/O)?

**Concepts**
- Flaky test definition
- Non-deterministic execution sources
- Shared mutable static state
- Time-dependent assertion brittleness
- Race conditions in parallel test execution

**Answer**

A flaky test passes and fails on the same code without any code change — its outcome depends on some environmental variable that differs between runs. The most common causes are time-dependent assertions (using `DateTime.Now` or `Stopwatch` with tolerances that fail under CPU load), threading race conditions (parallel test classes sharing a static dictionary that one test writes while another reads), shared external I/O (real files written to a fixed path by one test while a concurrently-running test tries to open the same path), and implicit ordering assumptions (a test that succeeds only if another test has already seeded the database). Flaky tests erode trust in the suite because a red CI run might mean "real bug" or "flake" — both demand investigation, and the false alarms slowly teach developers to ignore red builds. The fix is always to eliminate the nondeterministic dependency: inject a clock abstraction, use `Path.GetTempFileName()` for unique paths, move mutable state into test-local variables, and place tests that share mutable infrastructure into a `[Collection]` that xUnit serializes rather than running in parallel.

---

## Q10. What is the test pyramid, and where do unit tests sit relative to integration tests?

**Concepts**
- Test pyramid layer structure
- Unit/integration/E2E ratio rationale
- Feedback speed by layer
- Flakiness and maintenance cost by layer
- Inverted pyramid anti-pattern

**Answer**

The test pyramid describes the ideal ratio of test types: many fast unit tests at the base, a moderate number of integration tests in the middle, and a small number of end-to-end tests at the top. Unit tests sit at the base because they are the cheapest to write, execute in milliseconds, produce no flakiness from external dependencies, and pinpoint failures to a single class. Integration tests occupy the middle because they confirm that components wire together correctly — touching a real database, HTTP client, or file system — which makes them slower and occasionally flaky, but they catch contract mismatches that unit tests with mocked collaborators cannot. End-to-end tests exercise the full deployed stack through a browser or API gateway, providing the highest confidence but the most maintenance burden and the slowest CI times. Inverting the pyramid — relying primarily on E2E tests — produces slow CI, frequent unrelated failures, and difficulty tracing which change broke which behavior. The pyramid's shape says to invest the most test effort in unit tests since they deliver the fastest, most actionable signal per minute of test time.

---

## Q11. What is TDD (Red-Green-Refactor), and what benefits/challenges does it bring?

**Concepts**
- Red-Green-Refactor cycle
- Test-first API design pressure
- Scope constraint from failing test
- Refactor safety net
- Over-specification risk in TDD

**Answer**

TDD is the practice of writing a failing test (Red) before writing the production code that makes it pass (Green), then improving the design without changing behavior (Refactor). The Red step forces me to think about how the code will be consumed before writing it, which naturally drives toward simpler APIs and injected dependencies — code that is hard to test in TDD tends to be hard to use in production too, so the discipline surfaces design problems early. The Green step constrains scope: I write only enough production code to pass the failing test, which prevents premature generalization. The Refactor step is safe because the passing test suite acts as a regression net. The main challenges are the learning curve of writing tests first for complex algorithms or UI code, the risk of over-specifying implementation details in tests (which makes refactoring painful instead of safe), and the time investment in maintaining a growing suite. Teams that skip the Refactor step end up with duplicated production code and a suite shaped around the original, unoptimized implementation.

---

## Q12. When should a bug fix include a regression test?

**Concepts**
- Regression test purpose
- Bug reproduction before fix
- Defect traceability
- Re-emergence prevention
- Test layer matching bug layer

**Answer**

A bug fix should always include a regression test that fails on the buggy code and passes after the fix. Without this test, the same bug can silently re-emerge when someone later modifies nearby code, since nothing in CI would catch it. Writing the test first also proves that I have correctly reproduced the root cause rather than fixing a surface symptom — if the test still passes before the fix, I am testing the wrong thing. The test documents the correct behavior at the specific input that triggered the failure, which is valuable for future contributors who would otherwise not know why an oddly specific boundary case exists. The only exception is a bug that occurs exclusively at an integration or end-to-end layer that unit tests cannot reach — in that case the regression test belongs at the appropriate layer. A comment referencing the ticket or issue tracker in the test method helps future developers understand the history without needing to trace git blame.

---

## Q13. What is the difference between testing public behavior vs internal implementation?

**Concepts**
- Public API contract as test target
- Private member access via reflection
- Refactor safety of black-box tests
- Implementation-coupled test fragility
- InternalsVisibleTo misuse

**Answer**

Testing public behavior means asserting on what a class promises through its public methods and properties — the observable contract — without caring how that contract is fulfilled internally. Testing internal implementation means asserting on private fields, internal arrays, or protected methods, typically through reflection or `InternalsVisibleTo`. Behavior-based tests survive refactoring because I can replace a nested-if algorithm with a dictionary lookup without breaking a single test — the tests only verify that `GetLetterGrade(90)` returns `"A"`. Implementation-based tests break the moment I rename a private field, reorder an internal array, or extract a private helper into a separate class, even though the observable behavior is unchanged, which means refactors that improve the code structure are blocked by failing tests that were never supposed to constrain that structure. The practical rule is that reaching for `typeof(MyClass).GetField("_bands", BindingFlags.NonPublic | BindingFlags.Instance)` in a test is a design signal — the behavior I want to verify should be expressed through the public interface, or extracted into a testable collaborator with its own public API.

---

## Q14. How do deterministic tests handle `DateTime.Now`, `Guid.NewGuid()`, and randomness?

**Concepts**
- ISystemClock injection pattern
- FakeClock deterministic substitute
- IGuidProvider abstraction
- TimeProvider in .NET 8+
- Ambient context vs constructor injection

**Answer**

`DateTime.Now`, `Guid.NewGuid()`, and random number generators are non-deterministic — they return different values on every call, so a test that asserts on their output without controlling them is either flaky or impossible to assert on precisely. I introduce testability by placing each source of non-determinism behind an interface: an `ISystemClock` with a `UtcNow` property that I inject into any class needing the current time, and in tests I pass a `FakeClock` fixed to a known instant. For GUIDs I inject a `Func<Guid>` or an `IGuidProvider` that my test supplies as a deterministic sequence of values. For randomness I inject `System.Random` or a custom interface seeded with a fixed value so the test's expected output is computed from the same seed. .NET 8 introduced `TimeProvider` and `FakeTimeProvider` (from `Microsoft.Extensions.TimeProvider.Testing`) as a first-class abstraction for this pattern, which means I no longer need to define my own clock interface for new code — the platform provides the seam. The goal in all cases is to push non-determinism behind an interface boundary so the SUT's logic is pure and its tests are stable across every environment and execution time.

---

## Q15. What is arrange duplication, and when is shared setup justified vs harmful?

**Concepts**
- Arrange duplication across test methods
- Shared constructor setup in xUnit
- TestInitialize reset in MSTest
- Immutable shared state safety
- Per-test inline Arrange for scenario clarity

**Answer**

Arrange duplication occurs when every test in a class repeats the same setup block — constructing the SUT, seeding the same dependency, building the same input object. Extracting this into a shared constructor (xUnit) or `[TestInitialize]` (MSTest) removes boilerplate and is safe when the shared state is immutable or completely reset before each test. The shared setup becomes harmful when it initializes mutable state that one test modifies and another test assumes is still in its original form, producing the classic "test B fails only when test A runs first" ordering bug. A second harm is hiding what makes each test case distinct: when the critical input that drives a scenario is buried in a shared setup method rather than visible in the test method itself, a reader cannot understand the test's purpose without reading two methods. The rule I follow is to extract only the objects that are identical across every test and truly read-only — the SUT creation, the static reference data — and keep the values that are the essence of each scenario as local variables inside each test method.

---

### 02. xUnit

---

## Q1. What is xUnit.net, and how does its philosophy differ from MSTest and NUnit?

**Concepts**
- Per-test class instantiation model
- Constructor injection as setup mechanism
- IDisposable as teardown mechanism
- Attribute minimalism philosophy
- Community-first design origin

**Answer**

xUnit creates a new instance of the test class for every `[Fact]` and every `[Theory]` row, which eliminates the entire category of ordering bugs caused by mutable fields persisting between tests. MSTest and NUnit (by default) reuse one instance per class and rely on `[TestInitialize]` or `[SetUp]` to reset state before each test — a reset that is easy to forget, get wrong, or accidentally make order-dependent. xUnit replaces lifecycle attributes with plain C# idioms: the constructor is setup, `IDisposable.Dispose` is teardown, and `IAsyncLifetime` covers async variants. This means test setup is just a constructor, and IDE tooling understands it as a constructor rather than as a magic hook. NUnit sits between the two — it creates one class instance per class by default but supports instance-per-test via `[FixtureLifeCycle]`. xUnit is the default for `dotnet new xunit` and the preferred framework for new .NET projects because its isolation model makes parallel execution safe without extra configuration, since parallel tests always run on independent class instances.

---

## Q2. Explain `[Fact]` vs `[Theory]` — when is parameterized testing appropriate?

**Concepts**
- Fact as single fixed scenario
- Theory as parameterized row expansion
- Data-driven coverage of input space
- Row isolation in test output
- When separate Facts are clearer than Theory

**Answer**

`[Fact]` marks a test with no parameters — one fixed scenario with its own hard-coded inputs, act, and assert. `[Theory]` marks a method that receives parameters from data attributes; each combination generates an independent test result in the runner's output with its own pass/fail status, so a failure in row 2 does not obscure the result of row 1. Parameterized testing is appropriate when the same logical assertion — "IsPassing returns the expected bool for this score" — must be verified across multiple inputs that differ only in their data values, not in the structure of the test. It is inappropriate when each case requires meaningfully different Arrange or Assert code, because forcing those differences into a single theory method produces complex conditional logic inside the test. The practical rule is: three or more cases sharing identical Arrange/Act structure that vary only in data values should be a `[Theory]`; cases that have distinct setup or distinct failure explanations should stay as named `[Fact]` methods whose names encode the scenario.

---

## Q3. How do `[InlineData]`, `[MemberData]`, and `[ClassData]` supply theory inputs?

**Concepts**
- InlineData constant literals at call site
- MemberData computed IEnumerable sequence
- ClassData encapsulated data class
- Fresh object requirement per row
- Mutable shared reference danger

**Answer**

`[InlineData]` supplies arguments as compile-time constant literals directly in the attribute, which makes the values immediately visible at the call site — it is ideal for simple types such as `int`, `string`, and `bool`. `[MemberData]` points to a public static property or method that returns `IEnumerable<object[]>`, which supports complex types and computed data, but since the property may be evaluated once and the reference cached, each row must supply a newly constructed object graph rather than a shared mutable instance. `[ClassData]` points to a class implementing `IEnumerable<object[]>`, allowing data generation logic to be encapsulated and reused across multiple test classes. The critical invariant for both `[MemberData]` and `[ClassData]` is immutability of the supplied data: a `static readonly List<OrderLine>` referenced from `[MemberData]` and then mutated inside the test body will corrupt subsequent rows and re-runs because all invocations share the same list instance, which is why each row's data source should construct a fresh collection.

---

## Q4. How does xUnit create test class instances — per test or per class?

**Concepts**
- Per-test instantiation guarantee
- Mutable field safety per instance
- Constructor as setup replacement
- Expensive setup in fixtures instead
- Contrast with MSTest per-class default

**Answer**

xUnit creates a new instance of the test class for every individual `[Fact]` and for every `[Theory]` row, so each test runs against a completely fresh object with no state carried over from any previous test. This means any field I initialize in the constructor is reset before each test method executes, making it safe to use mutable instance fields without a teardown step. The implication is that constructors should contain only lightweight, fast initialization — if setup requires expensive operations such as seeding a database or starting a test server, that work belongs in an `IClassFixture<T>` or `ICollectionFixture<T>` that is constructed once and shared. The per-test instantiation model is why xUnit tests are naturally isolated without lifecycle attributes: unlike MSTest (v1), where a single class instance persisted across all `[TestMethod]` calls and `[TestInitialize]` was required to reset fields, in xUnit the constructor IS the initialization and each test gets its own copy.

---

## Q5. What are `IClassFixture<T>` and `ICollectionFixture<T>`, and when use each?

**Concepts**
- IClassFixture one instance per test class
- ICollectionFixture one instance per collection
- Shared construction cost amortization
- Immutable fixture as safety rule
- Serial execution within a collection

**Answer**

`IClassFixture<T>` tells xUnit to construct one `T` instance before the first test in the class runs, inject it into every test constructor in that class, and dispose it (if `IDisposable`) after the last test in the class completes. I use it when multiple tests in one class share an expensive read-only setup — like compiling a service provider or preparing a catalog of test data — since it amortizes the construction cost across all tests in the class. `ICollectionFixture<T>` does the same but shares the fixture instance across all classes marked with the same `[Collection("Name")]` attribute, which additionally causes all those classes' tests to run serially rather than in parallel. I use `ICollectionFixture<T>` when two or more classes need one shared resource that cannot safely run concurrently — a test database, a shared socket, an in-memory server. The critical rule for both is that the fixture must be immutable after construction: xUnit does not reset or reconstruct the fixture between test method calls, so any mutable state written by one test leaks into subsequent tests.

---

## Q6. How do collection definitions (`[Collection("Name")]`) serialize tests that share expensive resources?

**Concepts**
- CollectionDefinition class role
- Collection attribute on test class
- Serial execution guarantee within collection
- Parallel execution preserved outside collection
- CI flake prevention for shared mutable resources

**Answer**

A `[CollectionDefinition("Name")]` class declares a named collection and optionally implements `ICollectionFixture<T>` to attach a shared fixture. Any test class tagged `[Collection("Name")]` becomes a member of that collection, which means xUnit runs all tests from all member classes serially — no two test methods from any member class run at the same time. This is the mechanism for coordinating access to shared mutable resources: a temp file at a fixed path, a global in-memory registry, a database schema. Without `[Collection]`, every class runs in parallel by default on a multi-core CI agent, so classes that touch static state race against each other and produce non-deterministic failures that pass locally on a single-threaded IDE runner. The correct response to CI-only flakes caused by parallelism is not to disable all parallelism globally — that penalizes every fast stateless test — but to group only the tests that share state into a named collection, preserving parallelism everywhere else.

---

## Q7. What is `IAsyncLifetime`, and how does it replace async setup/teardown patterns?

**Concepts**
- IAsyncLifetime interface contract
- InitializeAsync called after constructor
- DisposeAsync called after all class tests
- Async fixture setup without sync-over-async
- Deadlock avoidance in async contexts

**Answer**

`IAsyncLifetime` is an xUnit interface with two methods: `InitializeAsync`, called after the constructor and before any test runs, and `DisposeAsync`, called after the last test in the class or collection completes. I implement it on a test class or fixture class when setup or teardown requires awaitable operations — opening a database connection, seeding data through an async ORM, starting a `TestServer`, or running `await Task.Delay` as part of cleanup. Without `IAsyncLifetime`, async operations in setup would require calling `.GetAwaiter().GetResult()` or `.Wait()` in the constructor, which risks deadlocking in synchronization-context-aware runtimes and suppresses async stack traces. xUnit awaits `InitializeAsync` and `DisposeAsync` correctly, so the async call chain remains unbroken. For fixtures shared via `IClassFixture<T>`, the fixture class can implement `IAsyncLifetime` to perform async construction and teardown while the test class constructor stays synchronous — the two concerns are kept separate.

---

## Q8. How does xUnit handle parallel test execution by default, and how do you disable it?

**Concepts**
- Default assembly-level parallelism
- Collection-scoped serialization
- DisableTestParallelization assembly attribute
- MaxParallelThreads configuration
- dotnet test CLI flag override

**Answer**

By default, xUnit runs test classes from the same assembly in parallel — each class (which forms its own default collection) can run concurrently with other classes. Methods within a single class always run sequentially since each gets its own instance but class-level parallelism is controlled by collection membership. To disable all parallelism for an entire assembly, I add `[assembly: CollectionBehavior(DisableTestParallelization = true)]` to `AssemblyInfo.cs` or a test setup file, which makes every test in the assembly run serially. For finer control, `[assembly: CollectionBehavior(MaxParallelThreads = 2)]` limits concurrency to two threads. From the CLI, `dotnet test -- xunit.parallelizeTestCollections=false` achieves the same without modifying source. The practical recommendation is to keep parallelism enabled for stateless unit tests — they benefit from it — and use named `[Collection]` attributes only for the test classes that truly share mutable infrastructure, since global serialization slows every test run including the fast pure unit tests.

---

## Q9. What is `ITestOutputHelper`, and how is it injected into tests?

**Concepts**
- ITestOutputHelper constructor injection
- Per-test scoped output capture
- Console.WriteLine alternative in xUnit
- Diagnostic logging on test failure
- TRX and CI output visibility

**Answer**

`ITestOutputHelper` is xUnit's mechanism for writing diagnostic text associated with a specific test rather than the global console stream. I inject it through the test class constructor — xUnit resolves it automatically when the constructor declares a parameter of type `ITestOutputHelper`. I use `_output.WriteLine(...)` to log intermediate values during a test (`_output.WriteLine($"Stock after reserve: {stock["WIDGET-01"]}")`), which helps diagnose failures in CI without affecting test isolation or adding permanent logging to production code. Unlike `Console.WriteLine`, which merges output from all parallel tests into one interleaved stream and is often suppressed by test runners entirely, `ITestOutputHelper` output is scoped to its test and visible only when that test fails or when the runner is set to verbose mode. This makes it particularly valuable in integration tests that call real infrastructure: I can log the HTTP request URI and response body to understand a 500 error without leaving trace output in every passing test run.

---

## Q10. How do `[Trait("Category", "Slow")]` attributes help filter tests in CI?

**Concepts**
- Trait key-value metadata
- dotnet test --filter expression
- CI pipeline segmentation by category
- Fast feedback loop preservation
- Trait inheritance at class level

**Answer**

`[Trait("Category", "Slow")]` attaches key-value metadata to a test method or class, which `dotnet test` can use with the `--filter` flag to include or exclude tests selectively. Running `dotnet test --filter "Category!=Slow"` on every PR keeps the build under ten seconds by skipping expensive tests, while a nightly job runs `dotnet test --filter "Category=Slow"` to include the full suite. Other common trait values are `"Integration"`, `"Database"`, and `"External"` to segregate tests that require real infrastructure from pure unit tests. Applying traits at the class level with `[Trait("Category", "Integration")]` on the class declaration propagates to all methods in that class, which is more maintainable than annotating each method individually. Combining multiple traits in a filter uses standard boolean syntax: `dotnet test --filter "Category!=Slow&FullyQualifiedName~OrderService"` runs all non-slow tests in the `OrderService` namespace. The trait system allows a single test project to serve multiple CI pipeline stages without splitting test code into separate projects.

---

## Q11. How does constructor injection of dependencies work in xUnit test classes?

**Concepts**
- xUnit constructor injection surface
- IClassFixture and ICollectionFixture resolution
- ITestOutputHelper built-in resolution
- No general IoC container in xUnit
- Manual object graph construction

**Answer**

xUnit resolves constructor parameters for test classes from a small, fixed set of sources: any `IClassFixture<T>` or `ICollectionFixture<T>` the class declares, `ITestOutputHelper`, and `IMessageSink`. There is no general-purpose IoC container — I cannot register arbitrary services the way an ASP.NET Core test host does with `WebApplicationFactory.ConfigureTestServices`. For a class implementing `IClassFixture<MyFixture>`, xUnit constructs `MyFixture` once and injects it into every new test class instance so each test method runs with a fresh class instance sharing the same fixture. This means fixtures hold expensive shared setup while the test class constructor handles lightweight per-test initialization — creating the SUT from fixture data, for instance. For anything beyond this injection surface, I build the object graph manually in the constructor or create factory helpers that the test calls inline. The constraint is intentional: xUnit's design keeps test setup explicit and readable rather than hidden in a container configuration file.

---

## Q12. What happens if a test constructor throws — how does xUnit report it?

**Concepts**
- Constructor exception propagation
- Test failure vs runner error distinction
- Fixture construction failure impact
- All-tests-in-class failure cascade
- Guard assertion in fixture constructors

**Answer**

If a test class constructor throws, xUnit catches the exception and reports every test in that class as failed with the constructor exception's message and stack trace, without ever invoking any test method body. This means the failure shows up in CI as a test failure rather than a runner crash, so the build correctly goes red and the error is traceable. The failure is attributed to each test that would have run rather than to the runner process, which means in a class with ten tests, all ten show as failed with the same constructor exception. The most common scenario where this matters is a fixture whose constructor throws because a required environment variable or connection string is missing — when the fixture fails, every class using it fails with the same message. To make these failures discoverable and informative rather than cryptic NullReferenceExceptions, I add guard assertions at the top of fixture constructors: `if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("TEST_DB_CONNECTION not set in environment");`, which produces a clear error message pointing to the missing configuration.

---

## Q13. How do you assert exceptions with `Assert.Throws<T>` vs `Assert.ThrowsAsync<T>`?

**Concepts**
- Assert.Throws synchronous lambda wrapper
- Assert.ThrowsAsync awaitable wrapper
- Exception type matching
- Exception property inspection after catch
- Missing await producing false pass

**Answer**

`Assert.Throws<T>` wraps a synchronous lambda, executes it, and fails the test if no exception is thrown or if the exception type does not match `T` — it returns the caught exception so I can then assert on its `ParamName`, `Message`, or inner exception. `Assert.ThrowsAsync<T>` does the same for an async lambda returning `Task`; I must `await` it, otherwise the assertion expression completes immediately before the lambda finishes and the test passes vacuously regardless of whether the exception was thrown. The calling conventions are: `var ex = Assert.Throws<ArgumentException>(() => sut.Method())` and `var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.MethodAsync())`. After catching the exception, I prefer asserting on `ex.ParamName` (for `ArgumentException` subtypes) or a specific fragment of `ex.Message` rather than matching the full message string, because full message matching is brittle against wording changes, localization updates, and framework version differences — it breaks tests for reasons unrelated to the behavior under test.

---

## Q14. What is the difference between returning `Task` from a test vs `async void`?

**Concepts**
- async Task observable exception propagation
- async void fire-and-forget from caller perspective
- xUnit await behavior on Task return
- Unobserved exception swallowing
- Absolute rule for async test methods

**Answer**

An `async void` method that throws propagates the exception to the synchronization context rather than through a `Task`, because there is no `Task` object for the caller to observe. xUnit cannot await an `async void` test — from its perspective the method returns immediately (void), so xUnit treats it as complete before the async body has run. The exception is thrown on a background thread where it is either swallowed or crashes the process depending on the runtime's unhandled exception policy. The result is that the test appears to pass even though it threw, which is the worst possible failure mode in a test suite. Returning `Task` from a test method gives xUnit a handle to observe: it awaits the task, catches any exception propagated through it, and reports the test as failed with the correct message and stack trace. The rule is absolute: every async test method must return `Task` — never `async void`, which is reserved for event handlers where the framework signature is forced.

---

## Q15. How do you run xUnit tests from CLI (`dotnet test`) and filter by fully qualified name?

**Concepts**
- dotnet test command discovery
- --filter flag syntax
- FullyQualifiedName contains operator
- Trait filter syntax
- --no-build and --logger flags for CI

**Answer**

`dotnet test` discovers and runs all tests in a project or solution. To filter by class name I pass `--filter "FullyQualifiedName~GradeCalculatorTests"` where the `~` operator means "contains", so any test whose fully qualified name includes that string will run. For an exact match I use `FullyQualifiedName=Namespace.ClassName.MethodName`. To filter by trait I use `dotnet test --filter "Category=Slow"`. Logical combinations use `&` (and) and `|` (or): `dotnet test --filter "Category!=Slow&FullyQualifiedName~OrderService"` runs all non-slow tests whose name contains "OrderService". In CI pipelines I typically pair `dotnet test` with `--no-build` after a prior build step, `--logger "trx;LogFileName=results.trx"` for structured TRX output, and `--results-directory ./TestResults` to control artifact paths. The filter syntax is the VSTest filter DSL and is adapter-specific — xUnit supports `FullyQualifiedName`, `DisplayName`, `Trait`, and class name patterns, all of which work the same whether tests are run from the CLI, Visual Studio's Test Explorer, or a CI agent.

---

### 03. MSTest

---

## Q1. What NuGet packages compose an MSTest project (`MSTest.TestFramework`, `MSTest.TestAdapter`, `Microsoft.NET.Test.Sdk`)?

**Concepts**
- Microsoft.NET.Test.Sdk as runner host
- MSTest.TestAdapter for VSTest discovery
- MSTest.TestFramework for attribute library
- Package version compatibility requirement
- SDK-style project auto-inclusion

**Answer**

An MSTest project in an SDK-style `.csproj` requires three packages. `Microsoft.NET.Test.Sdk` is the test runner host that `dotnet test` bootstraps — without it the project is not recognized as a test project and the CLI produces no test output. `MSTest.TestAdapter` is the VSTest adapter that discovers `[TestClass]`-decorated types and executes them — without it the runner finds zero tests. `MSTest.TestFramework` is the attribute and assertion library that test code references: `[TestClass]`, `[TestMethod]`, `Assert.AreEqual`, etc. — without it the code does not compile. All three must be version-compatible, which is a common migration pitfall when upgrading from MSTest v1 to v2: leaving the old adapter in place while updating the framework causes discovery failures that are difficult to diagnose because the project builds but tests do not appear. In Visual Studio's MSTest v2/v3 template all three are added automatically, and `dotnet new mstest` does the same from the CLI.

---

## Q2. Explain `[TestClass]`, `[TestMethod]`, and how discovery finds tests.

**Concepts**
- TestClass as container marker
- TestMethod as test discovery marker
- Adapter reflection-based scanning
- Public non-static zero-parameter requirement
- Attribute as registration mechanism

**Answer**

`[TestClass]` marks a class as a container for test methods, and `[TestMethod]` marks each individual test method within it. The MSTest adapter scans compiled assemblies for public classes annotated with `[TestClass]` and then enumerates their public, non-static, zero-parameter methods annotated with `[TestMethod]`. Discovery happens through reflection at the adapter level: the adapter reads attributes, reports discovered tests to the VSTest platform, and the platform displays them in Test Explorer and executes them via `dotnet test`. A class missing `[TestClass]` will not be scanned even if it contains `[TestMethod]` methods, and a method missing `[TestMethod]` will not run even if its class has `[TestClass]`. This attribute-driven model means tests require no registration code — the annotation is the registration. The practical implication is that a merge conflict that accidentally removes `[TestClass]` from a file silently drops an entire class of tests with no build error and no runner warning.

---

## Q3. What are `[DataTestMethod]` and `[DataRow]` equivalents to xUnit theories?

**Concepts**
- DataTestMethod as Theory equivalent
- DataRow for compile-time constant rows
- Row expansion to separate test results
- Attribute argument type constraints
- DataRow vs DynamicData for complex types

**Answer**

`[DataTestMethod]` is MSTest's equivalent of xUnit's `[Theory]` — it marks a method that receives data from one or more `[DataRow]` attributes. Each `[DataRow(...)]` provides a set of arguments for one execution, and the runner expands these into separate test results so that a failure in one row does not hide results for other rows. The critical constraint is that `[DataRow]` arguments must be compile-time constants of supported types — `int`, `double`, `float`, `string`, `bool`, `char`, `byte`, `short`, `long`, `Type`, and `null` — but not `decimal`. A method annotated with `[TestMethod]` instead of `[DataTestMethod]` will not expand its `[DataRow]` attributes: either the build fails because the method has parameters (which `[TestMethod]` requires be absent) or the rows are silently ignored and only one result appears in Test Explorer. For money amounts I use `double` literals in `[DataRow]` and cast to `decimal` inside the test body. For complex types or computed data that exceed what compile-time constants allow, I use `[DynamicData]` pointing to a static method that returns `IEnumerable<object[]>`.

---

## Q4. What is `[TestInitialize]` / `[TestCleanup]` vs `[ClassInitialize]` / `[ClassCleanup]` vs `[AssemblyInitialize]` / `[AssemblyCleanup]`?

**Concepts**
- TestInitialize per-test instance lifecycle
- ClassInitialize per-class static lifecycle
- AssemblyInitialize per-assembly static lifecycle
- Static requirement for class and assembly hooks
- Immutability rule for shared static state

**Answer**

MSTest provides three levels of setup and teardown. `[TestInitialize]` runs before each test method and `[TestCleanup]` runs after — these are instance methods on the test class, so in MSTest v2 they run on a fresh instance per test, giving each test a clean slate for instance fields. `[ClassInitialize]` runs once before any test in the class starts and must be `static`, accepting a `TestContext` parameter; `[ClassCleanup]` runs once after all tests in the class complete, also `static`. `[AssemblyInitialize]` runs once for the entire test assembly before any class starts, is `static`, and receives a `TestContext`; `[AssemblyCleanup]` mirrors it. I use class- and assembly-level hooks for truly one-time expensive setup — creating a shared in-memory server, loading a large read-only dataset — and instance-level hooks for SUT creation that must be fresh per test. The invariant for class and assembly hooks is that the static data they initialize must be treated as immutable for the remainder of the run: any test that mutates shared static state leaves that state corrupted for subsequent tests since no reset occurs between test methods.

---

## Q5. Why must `[ClassInitialize]` and `[AssemblyInitialize]` be `static`?

**Concepts**
- Pre-instance invocation timing
- Static member accessibility before construction
- TestContext parameter injection
- Static backing field initialization pattern
- Null guard in ClassCleanup

**Answer**

`[ClassInitialize]` and `[AssemblyInitialize]` run before any instance of the test class has been created — they are invoked directly on the class type, not through an object reference. Since no instance exists at that point, the method cannot be an instance method; making it `static` is what allows the runtime to invoke it without constructing a `[TestClass]` object. This also reflects their purpose: they initialize class-level static state — `private static DbConnection _connection = null!;` — that all subsequent test instances in the class will share via the static field. The `[ClassInitialize]` method accepts a `TestContext` parameter injected by MSTest, which provides access to test settings, the run directory, and result writing. A common pitfall is declaring static backing fields as `null!` with a nullable suppressor and then failing to null them out or dispose them in `[ClassCleanup]`, which causes resource leaks in long-lived test host processes where multiple test runs execute in the same process without restarting.

---

## Q6. What is the `[TestContext]` property, and what runtime services does it expose?

**Concepts**
- TestContext automatic injection
- TestName for diagnostic logging
- CurrentTestOutcome in TestCleanup
- WriteLine for test-scoped output
- AddResultFile for artifact attachment

**Answer**

`[TestContext]` is a special property that MSTest automatically sets on a `[TestClass]` if the class declares a public property named `TestContext` of type `TestContext`. It exposes runtime metadata and services: `TestContext.TestName` gives the name of the currently executing test method, which I use in `[TestInitialize]` to tag log output with the test name. `TestContext.CurrentTestOutcome` is readable in `[TestCleanup]` to determine whether the test passed or failed, which allows conditional capture of expensive diagnostics — only take a screenshot on failure. `TestContext.WriteLine(message)` writes to the test's output stream, which appears in the TRX report alongside that test's result rather than in a global console dump. `TestContext.AddResultFile(path)` attaches a file to the test result, which is useful in UI test frameworks for attaching failure screenshots. In modern MSTest code, `TestContext` is primarily used for `TestName` logging, conditional cleanup, and attaching result files — the `DataRow` access pattern from legacy `[DataSource]` CSV tests is no longer relevant in SDK-style projects.

---

## Q7. How does MSTest instance lifecycle differ from xUnit's new-instance-per-test model?

**Concepts**
- MSTest v1 single shared instance
- MSTest v2 new instance per test
- TestInitialize as mandatory reset in v1
- xUnit constructor as equivalent setup
- Field mutation safety by framework version

**Answer**

In MSTest v1, a single `[TestClass]` instance was shared across all test methods in the class — any field mutated by one test remained changed when the next test ran, making `[TestInitialize]` essential for resetting fields to a known state before each test. MSTest v2 changed this to create a new instance per test method, which aligns with xUnit's model and makes instance fields initialized in the constructor or `[TestInitialize]` safe without explicit teardown. However, most real-world MSTest code was written under v1 assumptions and still relies on `[TestInitialize]` rather than constructors, since that is the documented MSTest pattern. xUnit has always created a new instance per test, which is why it uses constructors for setup instead of a lifecycle attribute — the constructor is the setup and there is no equivalent of `[TestInitialize]`. The practical difference when reviewing code is that in xUnit, mutable `this` fields are safe by design because each test gets its own instance; in legacy MSTest v1 code, fields must be reset in `[TestInitialize]` or they carry state from the previous test.

---

---

## Q8. What are `[ExpectedException]` / `[ExpectedExceptionAttribute]` (legacy), and why is `Assert.ThrowsException` preferred?

**Concepts**
- ExpectedException attribute on method
- Overly broad exception scope
- Assert.ThrowsException scoped lambda
- Exception property inspection after catch
- Legacy attribute removal in MSTest v2

**Answer**

`[ExpectedException(typeof(ArgumentException))]` placed on a `[TestMethod]` tells MSTest that the test passes if any `ArgumentException` is thrown anywhere in the method body. The critical flaw is scope: if the Arrange phase throws the expected exception — perhaps a constructor that validates its arguments — the test passes even though the Act phase was never reached and no behavior was tested. `Assert.ThrowsException<T>` fixes this by accepting a lambda that wraps exactly the one expression expected to throw, so only a throw from that specific call satisfies the assertion. It also returns the caught exception, which allows me to then assert on `ex.ParamName` or a message fragment rather than treating any throw as a pass. MSTest v2 removed first-class support for `[ExpectedException]` in favor of `Assert.ThrowsException<T>` and `Assert.ThrowsExceptionAsync<T>`, and the async variant must be awaited — an unawaited `ThrowsExceptionAsync` call returns a task that is never observed, causing the test to pass regardless of what happens inside the lambda.

---

## Q9. What Assert helpers exist in MSTest (`Assert.AreEqual`, `Assert.IsTrue`, `Assert.ThrowsException`, `StringAssert`)?

**Concepts**
- Assert.AreEqual value equality overloads
- Assert.IsTrue / IsFalse condition checks
- Assert.ThrowsException scoped exception check
- StringAssert pattern and substring methods
- CollectionAssert for collection comparisons

**Answer**

MSTest's `Assert` class provides the core verification methods. `Assert.AreEqual(expected, actual)` checks value equality and has overloads for `double` and `float` that accept a delta tolerance — the two-argument overload on floating-point types is almost always wrong since binary floating-point cannot represent most decimal fractions exactly. `Assert.IsTrue(condition)` and `Assert.IsFalse(condition)` verify boolean conditions; they accept an optional message parameter for CI output. `Assert.ThrowsException<T>` wraps a lambda and verifies the specific type thrown, returning the exception for further inspection. `Assert.IsNull` and `Assert.IsNotNull` check reference nullability. Beyond `Assert`, `StringAssert` provides `Contains`, `StartsWith`, `EndsWith`, and `Matches` (regex) for string-specific assertions that produce better failure messages than `Assert.IsTrue(str.Contains(...))` because they include the actual string value in the error output. `CollectionAssert.AreEqual` compares ordered sequences, `AreEquivalent` compares unordered sets — using the wrong one for ordered vs unordered scenarios is a common source of false passes.

---

## Q10. How do you deploy test content files (`[DeploymentItem]`) — and what are modern alternatives?

**Concepts**
- DeploymentItem attribute purpose
- Shadow copy deployment folder
- CopyToOutputDirectory project property
- Content file deployment without attribute
- TestContext.TestRunDirectory for file paths

**Answer**

`[DeploymentItem("TestData/sample.json")]` instructs MSTest to copy a file from the project directory into the test run's deployment folder before the test executes, so the test can reference the file by a relative path. This was necessary in older MSTest runners that executed tests from a shadow-copied directory where the project's output was not directly accessible. The modern alternative in SDK-style projects is to set the file's build action to `Content` with `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>` in the `.csproj`, which ensures the file lands in the same directory as the compiled assembly and can be referenced with `Path.Combine(AppContext.BaseDirectory, "TestData", "sample.json")`. This approach works for both MSTest and xUnit without any test-framework-specific attribute. `[DeploymentItem]` still works but couples the test to MSTest's shadow-copy mechanism, which is disabled by default in SDK-style projects — so most teams encounter it only when migrating legacy test projects from .NET Framework.

---

## Q11. What is `[Ignore]` / `[TestCategory]`, and how do you filter categories in `dotnet test`?

**Concepts**
- Ignore attribute for skipping tests
- TestCategory attribute for grouping
- dotnet test --filter TestCategory expression
- Skip justification requirement
- CI pipeline segmentation by category

**Answer**

`[Ignore("Reason")]` marks a test or class as skipped — the test appears in results as "Skipped" rather than "Passed" or "Failed", which is preferable to commenting the test out since the skip is visible in CI reports. The reason string is mandatory in MSTest v2 to prevent unexplained skips accumulating unnoticed. `[TestCategory("Integration")]` attaches a category string to a test or class, which `dotnet test` uses with `--filter "TestCategory=Integration"` to run only that category, or `--filter "TestCategory!=Slow"` to exclude it. Multiple categories can be applied to the same method. The filter expression syntax for MSTest categories is `TestCategory` (not `Category` as in xUnit's `[Trait("Category", ...)]`). In CI pipelines I typically add a fast gate that runs `dotnet test --filter "TestCategory!=Integration&TestCategory!=Slow"` on every PR and a full gate that runs without filter in a scheduled nightly build, ensuring developers get sub-minute feedback without losing coverage of slower tests.

---

## Q12. How does MSTest parallelization work (`Parallelize` attribute at assembly/class level)?

**Concepts**
- Parallelize assembly attribute
- Workers and scope configuration
- Class-level isolation requirement
- Static state hazard under parallelism
- Per-test instance creation in MSTest v2

**Answer**

MSTest parallelism is opt-in, unlike xUnit which parallelizes by default. Adding `[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.MethodLevel)]` to the assembly instructs MSTest to run up to four tests concurrently. `Scope = ExecutionScope.MethodLevel` runs individual test methods in parallel across all classes; `Scope = ExecutionScope.ClassLevel` runs test classes in parallel but keeps each class's methods sequential. Class-level scope is safer since it matches xUnit's default model — each class runs on one thread at a time, so instance fields don't race, but different classes may run simultaneously so static fields still need synchronization. Static fields set in `[ClassInitialize]` and mutated by any test are the primary hazard: once parallelism is enabled, a mutation in one class's test can race with a read in another class's test if both classes share the same static type. MSTest v2 creates a new instance per test method, so instance fields are already thread-safe under method-level parallelism — the remaining work is eliminating mutable static state.

---

## Q13. What is the difference between MSTest V1 and V2/V3 adapters in SDK-style projects?

**Concepts**
- MSTest V1 NuGet package identity
- MSTest V2 restructured package set
- V3 parallel execution improvements
- SDK-style project compatibility
- Migration pitfall from V1 adapter

**Answer**

MSTest V1 shipped as a single package tied to Visual Studio and the .NET Framework testing infrastructure, making it difficult to use on Linux CI or in SDK-style projects without workarounds. MSTest V2 restructured this into the three-package model (`Microsoft.NET.Test.Sdk`, `MSTest.TestAdapter`, `MSTest.TestFramework`) that runs on .NET Core and later, supports cross-platform CI, and adds new-instance-per-test behavior, `Assert.ThrowsException`, `[DynamicData]`, and a proper `TestContext` implementation. MSTest V3 (2023+) adds first-class parallelism improvements, `[Timeout]` at test level, and better async support, while keeping the same three-package dependency model. The common migration pitfall is leaving the V1 adapter (`Microsoft.VisualStudio.QualityTools.UnitTestFramework`) referenced while the framework packages are updated to V2 — the old adapter cannot discover V2-decorated classes, producing zero test results with no build error. The fix is to remove the V1 reference entirely and ensure all three V2 packages are present and version-compatible.

---

## Q14. When would teams choose MSTest over xUnit in greenfield .NET projects?

**Concepts**
- Enterprise tooling alignment
- Azure DevOps template compatibility
- Legacy suite extension cost
- Org-wide training investment
- Framework capability parity

**Answer**

For most greenfield .NET projects I standardize on xUnit since `dotnet new xunit` is the SDK default, community samples and OSS libraries target it first, and its per-test isolation model requires no special setup to enable. I choose MSTest instead in three situations: first, when the team is extending an existing MSTest suite — the cost of mixing frameworks in one solution (two adapters, two attribute vocabularies, diverging conventions) usually exceeds any benefit of switching for new projects alongside old ones. Second, when the organization's CI templates, Azure DevOps test reporting dashboards, or internal tooling are built around MSTest's attribute names and TRX format — both frameworks produce TRX output but the attribute names differ and institutional documentation matters for onboarding. Third, when Visual Studio-centric workflows and Test Explorer groupings are critical for non-developer testers who rely on the MSTest project template experience. Both frameworks have feature parity for discovery, lifecycle, parallelism, and `dotnet test` integration — the choice is a team maintainability decision, not a capability gap.

---

## Q15. How do MSTest data sources (`[DynamicData]`) compare to xUnit `[MemberData]`?

**Concepts**
- DynamicData pointing to static method or property
- MemberData equivalent pattern
- Row expansion to separate test results
- Return type IEnumerable of object arrays
- Display name customization

**Answer**

`[DynamicData(nameof(GetDiscountRows))]` points to a public static method or property on the same class (or another class via the second argument) that returns `IEnumerable<object[]>`, and MSTest expands each row into a separate test result — the direct equivalent of xUnit's `[MemberData]`. Both require the data source to return `IEnumerable<object[]>` (or in newer versions, `IEnumerable<object?[]>`), and both expand rows independently so a failure in one row does not suppress results for others. The key difference is display name control: MSTest `[DynamicData]` supports a `DynamicDataDisplayName` parameter pointing to a static method that formats the test name from the row arguments, whereas xUnit's `[MemberData]` relies on the framework's default `(arg1, arg2, ...)` formatting or custom `TheoryData<T>` types with display names. Both require fresh data per row — a static field returning the same mutable list across rows is an identical hazard in both frameworks. For large computed datasets or data loaded from files, `[DynamicData]` and `[MemberData]` are interchangeable in practice; the only time they diverge is display name formatting, which matters for test result readability in CI reports.

---

### 04. Mocking & Test Doubles

---

## Q1. Define the test double taxonomy — dummy, fake, stub, spy, mock; what distinguishes each?

**Concepts**
- Dummy as unused placeholder
- Stub as canned return-value supplier
- Fake as working in-memory implementation
- Spy as interaction recorder
- Mock as behavior-verified collaborator

**Answer**

The five test double types differ by what they contribute to a test. A dummy is an object passed to satisfy a parameter signature but never called — it carries no behavior and exists only because the SUT's constructor or method requires it. A stub provides canned return values for calls the SUT makes to a collaborator: `Setup(i => i.HasStock(...)).Returns(true)` — it controls the SUT's inputs without caring whether the method was called. A fake is a working but simplified implementation of the real dependency — an in-memory `Dictionary`-backed repository that actually stores and retrieves data without a database. A spy records how it was called so assertions can be made after the act: `FakeEmailSender.SentMessages` is a list I can inspect. A mock (in the strict sense) has pre-programmed call expectations built in before the act and verifies those expectations automatically at the end of the test — `Mock<T>.Verify(...)` is the xUnit/Moq manifestation. In practice "mock" is used loosely to mean any test double, which causes design confusion: I should pick the double type that matches what I need to observe — return values, accumulated state, or interaction counts — rather than defaulting to a mock framework for every collaborator.

---

## Q2. What is a mock in the strict sense (interaction verification) vs informal "mock" meaning any fake?

**Concepts**
- Strict mock definition from Meszaros
- Informal mock usage in everyday speech
- Verify as the distinguishing call
- Over-mocking risk from terminology confusion
- Behavior assertion vs interaction assertion trade-off

**Answer**

In Gerard Meszaros's taxonomy, a mock is specifically a test double that has call expectations pre-programmed before the act phase and verifies those expectations automatically — the defining characteristic is that verification is built into the double itself, not added as a separate `Assert` call. Moq's `Verify(...)` and NSubstitute's `Received()` implement this pattern. Informally, developers use "mock" to mean any object substituted for a real dependency, including stubs (which only supply return values) and fakes (which have working implementations). This terminology confusion leads to design mistakes: teams reach for `new Mock<T>()` for every collaborator, configure `Setup` chains for return values, and then add `Verify` calls for every method, producing over-specified tests that break on harmless refactors. The practical rule is that interaction verification (Verify/Received) is appropriate only for side effects that cannot be observed through return values or state — an email sent, a message published, an audit record written. For everything else, a stub or fake with a state assertion is more resilient to refactoring.

---

## Q3. What is a stub, and when do you configure return values without verifying calls?

**Concepts**
- Stub as input controller for SUT
- Returns configuration without Verify
- State-based assertion sufficiency
- Stub vs mock selection criteria
- Stub dictionary as simplest form

**Answer**

A stub configures return values for methods the SUT calls on its collaborator, giving the SUT the inputs it needs to follow a specific code path, without recording or verifying whether those calls happened. I use a stub when the test's assertion is on the SUT's output or resulting state — the fact that `HasStock` returned `true` is input, not outcome, so there is no reason to verify it was called. `Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true)` is pure stubbing: it controls a scenario, but I don't care how many times `HasStock` was called or in what order. Configuring without verifying keeps tests decoupled from implementation details — if `OrderService` adds a caching layer that calls `HasStock` fewer times, a stub-only test still passes because it never asserted on the call count. I switch from stub to mock (add `Verify`) only when the side effect itself is the claim under test — "the order confirmation email was sent exactly once to the buyer" is an interaction claim that requires verification since no return value or state captures it.

---

## Q4. What is a spy, and how does it record interactions for later assertion?

**Concepts**
- Spy as observable side-effect recorder
- SentMessages list as classic spy pattern
- Spy vs mock inspection timing
- Manual spy vs framework spy
- Post-act assertion on recorded interactions

**Answer**

A spy is a test double that records how it was called so I can inspect those records after the act phase with normal `Assert` calls. The canonical example is a `FakeEmailSender` that implements `IEmailSender` and appends each call to a `List<EmailMessage> SentMessages` — after `service.PlaceOrder(order)` I assert `Assert.Single(fake.SentMessages)` and `Assert.Equal("buyer@example.com", fake.SentMessages[0].To)`. The difference from a mock is timing and coupling: a spy's assertions are written as regular `Assert` statements in the Assert section (making them visible and skippable without breaking the test structure), while a mock's expectations are pre-programmed in Arrange and verified in a separate `Verify` call that is coupled to the mock framework's API. Moq's `Callback` feature turns a mock into a spy: `.Callback<string, int>((sku, qty) => capturedSkus.Add(sku))` records arguments for later assertion. Manual spy classes (like `FakeEmailSender`) are easier to read and reuse across tests, while framework-based spies (Moq Callback) are lighter for one-off captures in tests that are already Moq-centric.

---

## Q5. What is a fake (e.g., in-memory repository), and when is it preferable to mocks?

**Concepts**
- Fake as working simplified implementation
- In-memory dictionary repository pattern
- Fake reuse across multiple tests
- Fake vs mock for stateful behavior
- When fake reduces test complexity

**Answer**

A fake is a working but simplified implementation of the real dependency that runs entirely in memory — it actually stores and retrieves data, applies basic rules, and maintains state, but without the overhead of a real database, file system, or network. A `StubInventoryService` backed by a `Dictionary<string, int>` that deducts stock on `Reserve` is a fake: it behaves like the real inventory service for the scenarios it covers, which means I can assert on `stock["WIDGET-01"] == 8` after a successful order rather than asserting on mock call counts. Fakes are preferable to mocks when the scenario requires stateful behavior — when multiple methods interact (check stock, then reserve, then check again) and the outcome depends on state mutation between calls, a fake handles this naturally while a mock requires a complex sequence of `Setup` and `Callback` chains that essentially reimplements the fake inside Moq. Fakes are also preferable when the same collaborator behavior is needed across many tests — a shared `FakeEmailSender` class is simpler to maintain than repeated Moq `Setup` blocks. The downside of fakes is the maintenance cost of keeping them consistent with the real interface as it evolves.

---

## Q6. What is dependency injection's role in making code testable?

**Concepts**
- Constructor injection as seam creation
- Interface abstraction for substitution
- Hardcoded dependency as testability blocker
- IoC container wiring in production
- Manual wiring in tests

**Answer**

Dependency injection creates the seams where test doubles can be inserted. A class that constructs its own `SqlConnection`, `HttpClient`, or `EmailService` internally has no seam — there is no way to substitute a test double without modifying production code or using reflection hacks. By accepting dependencies through constructor parameters typed as interfaces, the class declares what it needs without specifying how it is provided, which means production code passes real implementations and test code passes doubles. The testability benefit is direct: `new OrderService(new Mock<IInventoryService>().Object, new FakeEmailSender())` is possible only because `OrderService` accepts `IInventoryService` and `IEmailSender` by interface rather than constructing `WarehouseInventoryService` and `SmtpEmailSender` itself. DI containers (like ASP.NET Core's built-in container) wire production implementations at startup; tests wire substitutes manually in the constructor call, which requires no container setup and runs in pure in-process memory. The design principle is that a class should express its dependencies explicitly rather than hiding them inside methods, since explicit dependencies are substitutable and hidden dependencies are not.

---

## Q7. When should you use a mocking framework (Moq, NSubstitute, FakeItEasy) vs hand-written fakes?

**Concepts**
- Mocking framework for interface with many methods
- Hand-written fake for stateful behavior reuse
- Framework overhead vs clarity trade-off
- Fake class maintenance cost
- Decision criteria based on test count

**Answer**

I use a mocking framework when I need a lightweight substitute for an interface that has many methods but the current test only exercises one or two of them — `new Mock<IInventoryService>()` gives me a no-op implementation of every method for free, and I configure only the methods the SUT calls in this scenario. I prefer hand-written fakes when: multiple tests share the same simplified stateful behavior (a `FakeEmailSender` used across twenty tests is simpler than twenty identical Moq setup blocks), when the fake's behavior involves actual state transitions that would require complex `Callback` chains in Moq, or when I want the fake to be self-documenting (a `FakeEmailSender.SentMessages` property is clearer than a Moq `Verify` expression). The trade-off is maintenance: a hand-written fake must be updated every time the interface changes, while a Moq mock adapts automatically to new interface methods (new methods become no-ops by default). For interfaces with three or fewer methods, hand-written fakes are almost always clearer; for interfaces with ten or more methods where I only care about one or two, a mocking framework wins on conciseness.

---

## Q8. What is the difference between mocking an interface vs a concrete class?

**Concepts**
- Interface mock via pure proxy generation
- Concrete class mock via virtual method subclassing
- Sealed class limitation
- Non-virtual method interception impossibility
- Interface dependency as best practice

**Answer**

Mocking frameworks like Moq generate proxy classes at runtime to intercept method calls. For an interface, the proxy simply implements every method and routes calls through the mock's interception pipeline — since interfaces have no implementation, every method is inherently overridable. For a concrete class, the proxy subclasses it and overrides only `virtual` or `abstract` methods; non-virtual and sealed members cannot be overridden, so calls to them bypass the mock and execute the real production code. This means mocking a concrete class that has non-virtual methods creates a hybrid: some calls are intercepted (virtual) and some run real code (non-virtual), which produces confusing and unpredictable test behavior. Sealed classes cannot be mocked at all since they cannot be subclassed. The practical conclusion is to depend on interfaces when testability matters — if a collaborator exists only as a concrete class without an interface, I either extract an interface, wrap it in an adapter that I can mock, or accept that the test must be an integration test using the real class.

---

## Q9. Why can't Moq intercept non-virtual methods on concrete classes?

**Concepts**
- Castle DynamicProxy subclassing mechanism
- Virtual dispatch requirement
- Non-virtual method direct dispatch
- Sealed class proxy impossibility
- Static method non-interceptability

**Answer**

Moq (and most .NET mocking frameworks) use Castle DynamicProxy to generate subclasses at runtime. A subclass can only override methods that the base class marked as `virtual` or `abstract` — the CLR's method dispatch table routes virtual calls through the most-derived override, which is where Moq's interception code lives. For a non-virtual method, the CLR dispatches the call directly to the base class implementation at the call site (resolved at compile time via JIT inlining), bypassing the virtual dispatch table entirely, so no subclass override or proxy can intercept it. The same applies to `static` methods (no instance dispatch at all) and `sealed` classes (cannot be subclassed). This is a CLR design constraint, not a Moq limitation — any framework that works by subclassing faces the same restriction. The only way to intercept non-virtual calls without modifying source is through IL weaving (like Fakes in Visual Studio Enterprise or TypeMock) or by introducing an interface layer in production code, which is the standard solution.

---

## Q10. What is `Mock<T>.Setup`, `Returns`, `Callback`, and `Verify` in Moq terms?

**Concepts**
- Setup expression as method matcher
- Returns for configuring return values
- Callback for side effects during call
- Verify for post-act interaction assertion
- Times constraint on expected call count

**Answer**

`Mock<T>.Setup(expression)` registers an expectation on how a method should be called, using a lambda expression that Moq analyzes to identify the method and match arguments. `Returns(value)` (or `Returns(() => value)`) chains onto `Setup` to specify the return value for matching calls — without it, value types return their default and reference types return null. `Callback<T1, T2>(action)` chains onto `Setup` to execute a side effect when the method is called, capturing arguments for later inspection: `.Callback<string, int>((sku, qty) => log.Add($"{sku}:{qty}"))`. `Verify(expression, times)` is called after the act to assert that the method was called the expected number of times with the expected arguments — `Times.Once`, `Times.Never`, `Times.AtLeastOnce`, `Times.Exactly(n)`. `Setup` without `Verify` is a stub; `Verify` without `Setup` is an interaction check on a no-op; combining `Setup` with `Verify` tests both the return value scenario and the call count. `VerifyNoOtherCalls()` asserts that no method was called except those explicitly verified, which is useful for strict anti-corruption boundaries but makes tests brittle to harmless internal changes.

---

## Q11. What is over-specification / brittle mocking, and how does it couple tests to implementation?

**Concepts**
- Over-specification definition
- Call order assertions as over-specification
- VerifyNoOtherCalls brittleness
- Redundant Verify on same method
- Refactor-safe test scope

**Answer**

Over-specification occurs when a test asserts on internal collaboration details that are not part of the observable behavior contract — verifying call counts, argument values, or method call order for operations whose effect is already captured by the return value or state assertion. A test that verifies `HasStock` was called exactly twice and `Reserve` was called in the order `[WIDGET, GADGET]` is coupled to the current loop structure of `OrderService.PlaceOrder`, so merging two loops into one (a valid performance refactor) breaks CI even though every customer-facing guarantee is preserved. `VerifyNoOtherCalls()` is the most common form: it makes every method on the mock an implicit "called zero times" expectation, so adding `OrderService` logging or caching breaks tests that were not testing logging or caching. The cost is maintenance friction that makes developers afraid to refactor — they know any refactor will require touching multiple test files even when behavior is unchanged. The corrective discipline is to verify only the essential collaborations: "email was sent once on success" and "reserve was never called on failure" are behavioral claims; "HasStock was called before Reserve in test iteration two" is an implementation detail.

---

## Q12. What is the difference between verifying behavior (`Verify`) vs asserting output state?

**Concepts**
- Verify as interaction claim
- State assertion on return value or object
- Black-box vs white-box test design
- Prefer state assertion for resilience
- When Verify is necessary vs redundant

**Answer**

`Verify` asserts that a specific method was called on a collaborator — it is an interaction claim about what the SUT did internally. State assertion checks the observable output of the SUT — its return value, a thrown exception, or the changed state of an object I have a reference to. State assertions are more resilient to refactoring because they treat the SUT as a black box: the test only cares what came out, not how it was produced. `Verify` is necessary when the side effect — sending an email, publishing a domain event, writing an audit log — produces no state I can observe in a unit test scope; the only evidence the action happened is the call itself. When both options are available, I prefer state assertion: if `PlaceOrder` returns an `OrderResult` containing the reserved quantities, asserting on those quantities proves both that stock was checked and that the correct amounts were reserved, without coupling to the `HasStock`/`Reserve` method names. I use `Verify` only for void side effects that cross a meaningful boundary and have no observable state equivalent within unit test scope.

---

## Q13. How do you mock `async` methods returning `Task` / `Task<T>`?

**Concepts**
- ReturnsAsync for Task return types
- Task.FromResult alternative
- ThrowsAsync for exception simulation
- Awaited mock method behavior
- Sync-over-async pitfall in Setup

**Answer**

Moq provides `ReturnsAsync(value)` for methods returning `Task<T>` — it is equivalent to `.Returns(Task.FromResult(value))` but shorter and reads more clearly. For methods returning plain `Task` (no result), `Returns(Task.CompletedTask)` configures a successful no-op. For methods returning `ValueTask<T>`, Moq 4.16+ supports `ReturnsAsync` directly. To simulate an async method throwing an exception, `ThrowsAsync<T>()` chains onto `Setup` and is equivalent to `.Returns(Task.FromException<ReturnType>(new T()))`. The key gotcha is using `Returns(value)` (synchronous) instead of `ReturnsAsync(value)` on an async method: `Returns` wraps the value in `Task.FromResult` in some Moq versions but in others returns the raw value, causing a cast exception at call time. Always use `ReturnsAsync` for `Task<T>` methods to avoid this ambiguity. NSubstitute uses `.Returns(x)` for both sync and async — it detects the return type and wraps automatically, which is less explicit but avoids the wrong-overload trap.

---

## Q14. How do you substitute `HttpClient`, `ILogger<T>`, and `DateTime` abstractions in tests?

**Concepts**
- HttpMessageHandler stub for HttpClient
- IHttpClientFactory injection pattern
- NullLogger and NullLoggerFactory for ILogger
- ISystemClock or TimeProvider for DateTime
- FakeTimeProvider in .NET 8+

**Answer**

`HttpClient` cannot be mocked directly because its core method `SendAsync` is non-virtual on the base class; instead I inject a custom `HttpMessageHandler` subclass that overrides `SendAsync` and returns canned `HttpResponseMessage` values, then wrap it with `new HttpClient(handler)`. For services registered via `IHttpClientFactory`, I configure a named client in the test's `IServiceCollection` with the stub handler. For `ILogger<T>`, most unit tests do not need to assert on log output — `new NullLogger<OrderService>()` or `NullLoggerFactory.Instance.CreateLogger<OrderService>()` satisfies the interface without any output. When I do need to assert that a warning or error was logged, I use `ILogger` as a `Mock<ILogger<T>>` and verify `Log(...)` calls, or use the `Microsoft.Extensions.Logging.Testing` `FakeLogger` from .NET 8+. For `DateTime`, I inject `TimeProvider` (from `System` in .NET 8) and use `TimeProvider.System` in production and `new FakeTimeProvider(DateTimeOffset.UtcNow)` in tests, which allows advancing the clock for time-dependent scenarios without injecting a custom interface.

---

## Q15. What is a seam, and how do partial wrappers or interfaces introduce testability?

**Concepts**
- Seam definition from Working Effectively with Legacy Code
- Constructor parameter as seam
- Interface extraction as seam creation
- Adapter wrapper around third-party class
- Partial class seam in legacy code

**Answer**

A seam is a point in code where behavior can be changed without modifying the code at that point — Michael Feathers's term for a substitution point. In object-oriented code, the most common seams are constructor parameters and interface abstractions: the class declares what it needs (the interface), and the caller provides the implementation (production or test double). When a dependency is a third-party concrete class with no interface, I create a seam by writing a thin adapter interface: `IFileSystem` wrapping `System.IO.File`, `IDateTimeProvider` wrapping `DateTime`, or `IHttpSender` wrapping `HttpClient`. My production code depends on the adapter interface and the adapter's implementation delegates to the real type; tests depend on the same interface and supply a fake or mock. For legacy code where the class is large and introducing an interface is risky, a partial class or a virtual method override (subclass-and-override) can serve as a seam without restructuring the whole design. The principle is that anywhere a behavior needs to be substitutable for testing, that substitution point must be exposed through some form of indirection — static methods, sealed classes, and `new` expressions inside methods are seam-blockers.

---

## Q16. When is integration testing with real dependencies better than mocking everything?

**Concepts**
- Mock fidelity gap
- Contract drift between mock and real implementation
- Integration test for wiring verification
- Database query behavior not capturable by mocks
- Test layer selection by risk type

**Answer**

Mocking everything produces a test suite that can silently diverge from reality: the mock's `Setup` encodes assumptions about the collaborator's behavior, and if those assumptions are wrong — because the real implementation handles edge cases differently, the database query returns different data ordering, or an HTTP API changed its error response shape — the mock-only tests pass while production fails. Integration testing with real dependencies is better when: the risk is in the interaction between components rather than in a single class's logic (ORM-generated SQL is often surprising compared to what a hand-written mock would return), when the collaborator is a third-party system whose behavior is opaque and only discoverable by running it, or when the test is verifying that the DI container wires components correctly and that the real serialization pipeline works end to end. The practical rule is that unit tests (with mocks) verify that my code does the right thing given certain inputs from its collaborators, while integration tests verify that my code and the real collaborators actually agree on what those inputs will be. Both layers are necessary — a suite with only mocks has fidelity risk; a suite with only integration tests has speed and flakiness risk.

---

## Q17. What are anti-patterns: mocking concrete DB providers, verifying private collaborators, testing framework code?

**Concepts**
- Concrete DbContext mock fragility
- Framework code as trust assumption
- Private method test through public surface
- Test maintenance cost from implementation coupling
- What belongs in unit vs integration tests

**Answer**

Mocking a concrete `DbContext` or `SqlConnection` is fragile because EF Core and database drivers are complex concrete types with internal state — any test that configures mock return values for `DbSet<T>.FindAsync` or `ExecuteReaderAsync` is reimplementing the ORM's query logic inside the test, which creates a fake database that behaves differently from the real one on joins, lazy loading, and transaction semantics. The right level for database tests is an in-memory provider (`UseInMemoryDatabase`) for basic CRUD or a real test database for complex queries. Verifying private collaborators — asserting on internal fields or methods through reflection — couples tests to implementation details that the class is entitled to change freely; any refactor that does not change public behavior should not break tests. Testing framework code (asserting that `IServiceCollection.AddSingleton` registers the correct type, that `Middleware` invokes `next()`, or that ASP.NET Core model binding parses a query string) is testing Microsoft's code, not mine — use integration tests or `WebApplicationFactory` for wiring verification, not unit tests that mock framework internals.

---

## Q18. How do manual test doubles (hand-rolled stubs) compare to framework mocks for readability?

**Concepts**
- Hand-rolled stub explicit behavior
- Framework mock concise but abstract
- Readability for test reviewer
- Maintenance on interface change
- Choosing by test count and collaboration complexity

**Answer**

A hand-rolled `FakeEmailSender` class with a `List<string> SentTo` field makes its behavior obvious at a glance — a reviewer reading the test sees `new FakeEmailSender()` and understands that it records sent messages without running any real email infrastructure. A Moq equivalent requires reading `new Mock<IEmailSender>()`, `Setup(...)`, `Verify(...)`, and understanding Moq's expression-tree DSL. For complex stateful behavior the hand-rolled fake wins on clarity since the fake's implementation IS the specification; for simple one-return-value stubs on large interfaces the framework mock wins on conciseness since it avoids creating a class file for a single scenario. The maintenance trade-off is the inverse: adding a method to `IEmailSender` requires updating every hand-rolled fake, while a Moq mock automatically gets a default implementation for new methods. Teams with many test files benefit from investing in a small library of well-named fake classes (a fake repository, a fake clock, a fake event bus) that are reused and understood by all contributors, rather than repeating inline `Setup` chains that encode the same assumptions in dozens of test files independently.

---

## Q19. Testing private methods directly — usually signals a design problem; test through public seams or extract collaborators.

**Concepts**
- Private method as implementation detail
- Public surface as test entry point
- InternalsVisibleTo misuse
- Reflection as last resort
- Extract-and-test as design improvement

**Answer**

Reaching for a private method in a test — through reflection, `InternalsVisibleTo`, or widening its access modifier — signals that the behavior being tested is not reachable through the class's public surface, which is either a test design problem or a production design problem. If the private logic is complex enough to warrant its own test, it is complex enough to warrant its own class with a public interface, at which point I can test it directly through that class's API. If the private method can be fully verified by testing the public methods that call it, no direct test is needed — the public test exercises the private code along the path. The only legitimate use of `InternalsVisibleTo` in tests is for testing internal (assembly-level) classes that are intentionally not public but are still part of the tested assembly's design — not for reaching into `private` members. Reflection-based access in tests is a maintenance hazard: a private field rename breaks the test with a runtime exception rather than a compile error, making the failure harder to trace.

---

## Q20. Shared mutable static state — tests pass alone but fail in parallel or random order; isolate with instance state or `[Collection]` serialization.

**Concepts**
- Static field as shared test state
- Parallel execution race condition
- Instance field isolation per test
- Collection serialization for unavoidable statics
- Order-independence requirement

**Answer**

A static field set in one test and read in another creates an implicit ordering dependency — the tests are not actually independent even though they appear so when run one at a time. On a multi-core CI agent running tests in parallel, two test classes may simultaneously write to and read from the same static field, producing non-deterministic results that are impossible to reproduce locally when using a sequential IDE runner. The fix hierarchy is: prefer instance state (initialized in the constructor or `[TestInitialize]`) so each test gets its own copy; if a static is truly unavoidable (a global registry, a static `HttpClient` pool), use `[Collection("SharedResource")]` in xUnit or `[assembly: Parallelize(Scope = ExecutionScope.ClassLevel)]` with careful sequencing in MSTest to serialize the affected tests. Testing for this failure mode requires running the suite with a high parallelism degree: `dotnet test -- xunit.maxParallelThreads=8` on a suite that normally passes is a quick way to surface hidden static state bugs before they reach CI.

---

## Q21. Mocking concrete classes with non-virtual members — framework proxies cannot override sealed/non-virtual methods; depend on interfaces.

**Concepts**
- DynamicProxy subclassing mechanism
- Virtual dispatch requirement for interception
- Sealed class mock impossibility
- Interface extraction as solution
- Adapter wrapper for third-party concrete classes

**Answer**

When I try to mock a concrete class with non-virtual methods, the mocking framework generates a subclass that can only override `virtual` members — calls to non-virtual methods bypass the proxy and execute the real production code, producing unexpected behavior that is difficult to diagnose because the mock appears to be configured correctly. Sealed classes cannot be subclassed at all, so `new Mock<HttpClient>()` throws at runtime. The solution is to extract an interface for the dependency: `IInventoryService` instead of `WarehouseInventoryService`, `IFileSystem` instead of `System.IO.File`. For third-party types I don't own (HttpClient, FileInfo), I write a thin adapter class that implements my interface and delegates to the real type — the adapter is never tested directly but its interface can be mocked freely. The broader principle is that testable code depends on abstractions, not concrete types, which aligns with both the Dependency Inversion Principle and the practical constraint that DynamicProxy-based mocking requires virtual dispatch.

---

## Q22. `async void` test methods — runners may not observe exceptions; always return `Task` from async tests.

**Concepts**
- async void fire-and-forget semantics
- Test runner Task observation requirement
- Exception on synchronization context
- Unobserved task exception swallowing
- ValueTask compatibility in xUnit

**Answer**

An `async void` method propagates exceptions to the synchronization context instead of through a returned `Task`, which means the calling code — the test runner — has no `Task` to observe and cannot catch the exception. xUnit schedules the method and receives void immediately, treating the test as complete before the async continuation runs; any subsequent exception either goes unobserved (silently swallowed) or crashes the process depending on the runtime's unhandled exception handler. Either outcome produces a false pass — the test result is green while the behavior under test failed. Returning `Task` gives the runner a handle to `await`, so the runner catches exceptions from the continuation and reports them correctly as test failures. The rule is absolute for all async test methods in all frameworks: `async Task`, never `async void`. In xUnit, `ValueTask` is also supported as a return type from test methods. For async `[TestInitialize]` in MSTest v2, MSTest supports returning `Task` from `[TestInitialize]` and `[TestCleanup]` methods — the same rule applies there.

---

## Q23. Over-verifying mock calls — `Verify` on every internal call breaks on refactor; assert outcomes and critical interactions only.

**Concepts**
- Over-verification definition
- Refactor-blocking test fragility
- VerifyNoOtherCalls brittleness
- Essential interaction identification
- Outcome-first assertion strategy

**Answer**

Over-verifying occurs when every method call on every mock is asserted — `HasStock` called twice, `Reserve` called twice, `SendConfirmation` called once, `VerifyNoOtherCalls()` on all mocks. This transcribes the current execution trace into the test, so the test fails on any refactor that changes how `OrderService` walks its collaborators internally, even when the customer-visible result is identical. `VerifyNoOtherCalls()` is the most aggressive form: it fails if the SUT adds a logging call, a metrics increment, or a cache lookup — none of which change behavior but all of which break this assertion. The corrective approach is to identify only the interactions that are part of the observable contract: "on insufficient stock, no email is sent" is a contract claim that requires `Verify(email.Send..., Times.Never)`; "HasStock was called in iteration order before Reserve" is an implementation detail that has no contract standing. For the happy path, asserting on `result.Success` and stock levels via a fake is almost always sufficient without any `Verify` calls.

---

## Q24. Integration tests disguised as unit tests — real SQL/file/network makes tests slow and flaky; name and folder them honestly.

**Concepts**
- Integration test mislabeled as unit test
- Flakiness source from real I/O
- Speed degradation in unit test suite
- Folder and naming convention for test layers
- [TestCategory] / [Trait] for segmentation

**Answer**

A test that opens a real database connection, writes to a real file at a fixed path, or makes a real HTTP request is an integration test regardless of which test class or project it lives in. When these tests are mixed into the unit test project and named like unit tests, they slow the suite (each test may take seconds instead of milliseconds), produce flaky failures caused by environment differences (missing database, network timeout, locked file), and make CI unreliable. The fix is to be honest about what the test exercises: name it with an `Integration` suffix or prefix, place it in an `IntegrationTests` project or folder, and tag it with `[Trait("Category", "Integration")]` (xUnit) or `[TestCategory("Integration")]` (MSTest) so `dotnet test --filter "Category!=Integration"` excludes it from the fast PR gate. This is not about avoiding integration tests — they are necessary — but about giving them their own CI gate with appropriate timeouts, environment prerequisites, and run frequency (often nightly rather than on every commit).

---

## Q25. MSTest `[ClassInitialize]` sharing mutable state — static setup mutated by one test leaks into others unless reset in `[TestInitialize]`.

**Concepts**
- ClassInitialize runs once per class
- Static field shared across all test instances
- TestInitialize for per-test reset
- IReadOnlyDictionary for enforced immutability
- ClassCleanup teardown scope

**Answer**

`[ClassInitialize]` runs exactly once before any test in the class executes and initializes static fields that persist for the entire class run. If one `[TestMethod]` mutates a static field — overriding a dictionary entry, appending to a list, setting a flag — that mutation is visible to every subsequent test in the class because the static field is not reset between tests. The `[ClassInitialize]` only runs once, not before each test. The corrective patterns are: declare the static data as `IReadOnlyDictionary` or a `readonly` array to make mutation a compile error; if a test genuinely needs a modified copy, create a local clone in `[TestInitialize]`; if multiple tests share a mutable resource that must be reset, move the reset into `[TestInitialize]` and pay the per-test initialization cost. `[ClassCleanup]` runs after all tests in the class complete (not after each one), so it is suitable for releasing external resources (connections, file handles) but not for isolating tests from each other.

---

## Q26. xUnit class fixtures shared across unrelated tests — fixture lifetime is per-class; accidental shared state causes order-dependent failures.

**Concepts**
- IClassFixture one instance per class lifetime
- Mutable fixture field leaking between tests
- Fixture designed for read-only shared data
- IDisposable not called between individual tests
- Local variable vs fixture field decision

**Answer**

`IClassFixture<T>` creates one `T` instance for the duration of the test class and injects it into every test method's constructor — it is not reconstructed or reset between tests. A mutable field on the fixture accumulates changes from each test in execution order, so `AddPromoLine_IncludesLineInWorkingSet` adds a line and `WorkingLines_StartsEmptyEachTest` then finds a non-empty list. `IDisposable.Dispose` on the fixture is called only after the last test in the class completes, not between individual tests, so it cannot serve as a per-test reset. The rule for fixture design is: fixtures hold read-only shared setup (a compiled service host, a seeded catalog of reference data); per-test mutable state belongs in local variables inside each test method. If a test truly needs a starting state derived from expensive fixture data, it should build that state locally from the fixture's immutable read-only reference rather than mutating the fixture and relying on cleanup.

---

## Q27. Theory data referencing mutable objects — shared array/list mutated in one run corrupts later theory cases.

**Concepts**
- Static MemberData list reference sharing
- Theory row mutation affecting subsequent rows
- Fresh object creation per row
- IReadOnlyList parameter type vs List implementation
- Re-run isolation requirement

**Answer**

When a `[MemberData]` property returns an `IEnumerable<object[]>` where one element is a shared static `List<OrderLine>`, all theory rows — and potentially re-runs of the same row — reference the same list instance. A test body that calls `lines.Add(...)` on that list grows it permanently for the lifetime of the test process, so the second run of the same row sees extra items and fails with an unexpected total. The `IReadOnlyList<T>` parameter type declaration in the test method signature does not prevent this — it only prevents the test from calling `Add` directly (requiring a cast), but if the underlying object is a mutable `List<T>` passed as the interface, the type system provides no protection. The fix is to return a freshly constructed list per row from the `MemberData` property: using a getter property (not a field) that constructs `new List<object[]> { new object[] { new List<OrderLine> { ... }, 60m } }` on every access, so each theory invocation receives an independent object graph that is safe to mutate.

---

## Q28. Not awaiting async assertions — `Assert.ThrowsAsync` must be awaited; fire-and-forget hides failures.

**Concepts**
- Unawaited Task discard
- ThrowsAsync returning Task of exception
- False pass from unobserved exception
- Compiler warning CS4014
- Async Assert pattern requirement

**Answer**

`Assert.ThrowsAsync<T>` returns a `Task<T>` representing the assertion — the assertion does not execute synchronously. If I write `Assert.ThrowsAsync<ArgumentException>(() => sut.MethodAsync())` without `await`, the test method proceeds immediately without observing the task's result, and since the task is discarded without being awaited the exception (or its absence) is never surfaced. The test appears to pass regardless of whether the exception was thrown. The compiler emits CS4014 ("this call is not awaited") but in older project configurations this is a warning rather than an error, so it is easy to miss in code review. The correct form is `var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.MethodAsync())` followed by assertions on `ex`. The same applies to NUnit's `Assert.ThrowsAsync` and any custom async assertion helper — any assertion method that returns `Task` must be awaited, and test methods that use `await` must return `Task` (not `async void`) to ensure the runner observes the complete execution including the awaited assertion.

---

## Q29. Assuming test execution order — xUnit and parallel MSTest do not guarantee order; tests must be independent.

**Concepts**
- Non-deterministic method ordering in xUnit
- Parallel class execution in xUnit
- Test independence requirement
- Alphabetical sorting as false ordering signal
- ITestCaseOrderer for deliberate ordering

**Answer**

xUnit deliberately randomizes the order of test methods within a class (in xUnit v2, via internal hashing; in xUnit v3, configurable) and runs test classes in parallel by default. MSTest with `[Parallelize(Scope = ExecutionScope.MethodLevel)]` also does not guarantee method order within a class. A test that seeds state and expects a later test to consume it will flake non-deterministically — passing when alphabetical ordering happens to run them in the right sequence and failing when it does not. The correct model is that every test must be a complete, self-contained scenario: Arrange builds all required preconditions from scratch, Act exercises exactly one behavior, Assert verifies the outcome. If test A seeds a database row that test B reads, both tests should seed the row independently, or the seeding should live in a fixture that both tests share immutably. xUnit provides `ITestCaseOrderer` for tests that genuinely require sequential execution (integration smoke tests, migration scripts), but this is a deliberate opt-in for scenarios where ordering is the specification — not a workaround for tests that accidentally share state.

---

## Q30. Confusing stub with mock — stubs set up responses; mocks (strict sense) verify interactions — mixing terms leads to wrong test design.

**Concepts**
- Stub as input controller without verification
- Mock as interaction verifier
- Terminology confusion causing design errors
- Wrong double type producing false test signal
- Meszaros taxonomy as shared vocabulary

**Answer**

The practical consequence of confusing stub with mock is adding `Verify` calls to every configured `Setup`, which couples tests to the internal call graph and produces over-specified suites that break on harmless refactors. A stub exists to return a value so the SUT can follow a code path — verifying that `HasStock` was called once adds no value when the test already asserts on `result.Success` and stock levels, because the state assertion already proves `HasStock` must have been consulted. A mock (in the strict sense) is appropriate when the side effect is the claim: "no confirmation email was sent on failure" cannot be proven by state assertion in a unit test — I must verify the call count on the email collaborator. Using mock terminology for stubs leads teams to routinely add `Verify` to every `Setup` as a matter of habit, inflating test maintenance cost for no additional coverage. Establishing a shared vocabulary — stub for return-value configuration, mock for interaction verification — and reviewing test files for unnecessary `Verify` calls on Setup-only doubles is a practical way to reduce brittleness across a codebase.


(D) Your team tests `OrderService.PlaceOrder` three different ways — `StubInventoryService`, `FakeEmailSender`, and `Mock<IEmailSender>` with `Verify`. For a new test that asserts "insufficient stock cancels the order and no email is sent," which double type do you pick for **inventory** and for **email**, and why? When would you swap the email side from fake to mock?

**Answer:** Use a **stub** (or minimal mock `Setup`) for inventory to force `HasStock` to return `false`, and a **fake** for email so you assert `SentMessages` is empty — the same pattern as `PlaceOrder_WhenStubReportsLowStock_FailsWithoutEmailOrReservation` in **OrderServiceManualDoubleTests.cs**. Swap email to a mock when you only need a spy (`Verify(..., Times.Never)`) and do not care about inspecting message contents — as in **OrderServiceMoqTests.cs** failure tests.

- **Inventory → stub:** The scenario needs a canned `false` from `HasStock("WIDGET-01", 5)` when stock is 1. A stub dictionary or one `Setup().Returns(false)` is enough; no need to verify `Reserve` was never called if stock state stays unchanged (stub) or you add a single `Verify(..., Times.Never)` on reserve.
- **Email → fake (default here):** `FakeEmailSender.SentMessages` gives a readable behavior assertion (`Assert.Empty`) without Moq ceremony. The chapter's **Program.cs** Section 5 demo uses this path.
- **Email → mock:** Prefer `Mock<IEmailSender>` + `Verify(..., Times.Never)` when the test file is already Moq-centric, when email is not the focus and you want consistent mock-only Arrange blocks, or when multiple void collaborators each need `Times.Never` in one Assert section.
- **Avoid mock for inventory in this scenario** unless you also need to prove `Reserve` was never invoked — the stub's unchanged dictionary already proves no reservation side effect.

Pick doubles by **what you need to observe** — return values (stub), accumulated state (fake), or interaction counts (mock/spy).

---

#### Q2. What is a mock in the strict sense (interaction verification) vs informal "mock" meaning any fake?

(R) A teammate refactors `OrderServiceMoqTests` to "prove every collaboration." Review the Arrange/Assert block:

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

Issues:
- Redundant `Verify` — specific SKU calls and `It.IsAny` with `Exactly(2)` assert the same guarantee twice; failure messages confuse reviewers.
- `VerifyNoOtherCalls()` on inventory breaks when `OrderService` adds logging, metrics, or a harmless extra query.
- Tests **how** `PlaceOrder` walks lines, not **what** it delivers — refactors that preserve behavior still break CI.
- No check on `OrderResult.Success` or `OrderId` — the test could pass if `PlaceOrder` returned a default struct after side effects.

Fix: assert `result.Success` and `result.OrderId` first, keep one essential Reserve + email `Verify`, delete duplicate `It.IsAny` verifies, drop `VerifyNoOtherCalls()` on the happy path.

---

#### Q3. What is a stub, and when do you configure return values without verifying calls?

(R) After a harmless refactor — merging the two `foreach` loops in `OrderService.PlaceOrder` into one pass — CI fails on this test (adapted from `PlaceOrder_WhenStockAvailable_ChecksStockBeforeEveryReserve`):

```csharp
[Fact]
public void PlaceOrder_ChecksAllStockBeforeAnyReserve()
{
    var inventoryMock = new Mock<IInventoryService>();
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

**Answer:** The test asserts **call order**, not business outcome — a brittle Callback sequence tied to the two-loop implementation. A single-loop version interleaves `HasStock` and `Reserve` per line (`HasStock WIDGET → Reserve WIDGET → HasStock GADGET → Reserve GADGET`) and is still correct, but `callLog` no longer matches the hard-coded array.

Replace the ordered `callLog` assertion with outcome checks: success result, stock decremented to correct values, one email in `FakeEmailSender`. For the all-or-nothing invariant, use `Verify(Reserve..., Times.Never)` on the failure path — that tests a business rule, not loop structure. Reserve `Callback` capture lists for verifying **what** was reserved, not **when** relative to unrelated calls.

---

#### Q4. What is a spy, and how does it record interactions for later assertion?

(P) Production `OrderService` will call a warehouse REST API through `HttpClient`. You must unit-test `WarehouseInventoryService : IInventoryService` without network I/O. Sketch the test seam — how do you substitute HTTP responses, and why is `new HttpClient()` inside the service a blocker?

**Answer:** Inject `HttpClient` (via constructor or `IHttpClientFactory`) and in tests pass an `HttpClient` backed by a custom `HttpMessageHandler` that returns canned `HttpResponseMessage` instances — no socket, no DNS, no sandbox API.

- Subclass `HttpMessageHandler`, override `SendAsync`, return `new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"available":10}""", Encoding.UTF8, "application/json") }`. Wrap with `new HttpClient(handler)`.
- Production uses `services.AddHttpClient<IInventoryService, WarehouseInventoryService>()`; tests construct `WarehouseInventoryService` with the stub handler directly.
- `new HttpClient()` inside the service blocks testing because you cannot swap the handler without reflection; each test would hit the real network or require a global static mock server.

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

#### Q5. What is a fake (e.g., in-memory repository), and when is it preferable to mocks?

(D) A pricing microservice client is injected into checkout. Your lead asks whether to test it with (A) a custom `HttpMessageHandler` stub in a unit test, (B) `WebApplicationFactory` hitting an in-memory test server, or (C) a contract test against a deployed sandbox. Map each option to a layer of the **test pyramid** for this repo's `OrderService` chapter. When is each the right default?

**Answer:** **A** is the wide base (fast unit tests of the HTTP client wrapper); **B** is the middle (integration — your API + DI + middleware + serialization); **C** is the narrow top (end-to-end or contract against real external systems).

| Option | Pyramid layer | What it proves | Default when |
|---|---|---|---|
| **A — `HttpMessageHandler` stub** | Unit (many) | Your code builds correct request and maps response | Every branch of client parsing, error handling, retries — milliseconds per test |
| **B — `WebApplicationFactory`** | Integration (some) | `OrderService` in ASP.NET pipeline, model binding, auth | Controller + DI wiring + JSON contract; still no real warehouse if inventory is mocked |
| **C — Sandbox contract test** | E2E / contract (few) | Real pricing API version, TLS, auth tokens, rate limits | Nightly or pre-release; not on every PR |

Do not use **C** to prove "insufficient stock skips email" — that logic belongs in unit tests with `FakeEmailSender` or `Times.Never`. Only **C** tests inverts the pyramid to an ice cream cone.

---

#### Q6. What is dependency injection's role in making code testable?

(R) A junior duplicates production stock logic inside Moq `Setup` chains so tests "stay realistic":

```csharp
var stock = new Dictionary<string, int> { ["WIDGET-01"] = 10, ["GADGET-02"] = 5 };
var inventoryMock = new Mock<IInventoryService>();
inventoryMock
    .Setup(i => i.HasStock(It.IsAny<string>(), It.IsAny<int>()))
    .Returns<string, int>((sku, qty) => stock.TryGetValue(sku, out int n) && n >= qty);
inventoryMock
    .Setup(i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()))
    .Callback<string, int>((sku, qty) => stock[sku] -= qty);
```

Compare this to `OrderServiceManualDoubleTests` using `StubInventoryService` + `FakeEmailSender`. What maintenance and design problems does the mock-heavy version introduce, and when is the manual double clearly better?

**Answer:** The mock version reimplements `StubInventoryService` inside Moq lambdas — same behavior, worse readability and no reuse. Issues: stock rules drift from `StubInventoryService.cs`, lambda Setup is harder to scan than `new StubInventoryService(stock)`, and the mock is used as a fake (stateful dictionary) when Moq shines for verification, not for reimplementing domain state.

Replace mock inventory with `StubInventoryService` + `FakeEmailSender` — identical assertions with fewer lines. Use mocks when the scenario is one boolean (`HasStock` false) or a spy (`Verify` email once). Fake/stub classes for stateful simplified behavior; mocks for interaction contracts.

---

#### Q7. When should you use a mocking framework (Moq, NSubstitute, FakeItEasy) vs hand-written fakes?

(R) Review this failure-path test for the second-line-out-of-stock scenario (`PlaceOrder_WhenSecondLineOutOfStock_DoesNotReserveFirstLine`):

```csharp
[Fact]
public void PlaceOrder_WhenSecondLineOutOfStock_DoesNotReserveFirstLine()
{
    var inventoryMock = new Mock<IInventoryService>();
    inventoryMock.Setup(i => i.HasStock("WIDGET-01", 2)).Returns(true);
    inventoryMock.Setup(i => i.HasStock("GADGET-02", 1)).Returns(false);

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

**Answer:** Moq records all invocations regardless of whether a `Setup` exists for void methods — `Times.Never` would catch an explicit `Reserve("WIDGET-01", 2)` call with matching arguments. The gap is brittleness to API reshaping: if `OrderService` is refactored to call a new `TryReserveAll()` batch method that replaces the individual `Reserve` calls, the per-SKU `Verify` passes vacuously because the old method signatures were never invoked — while the business-rule violation (reserving first-line stock when second-line is unavailable) persists. The stronger approach is to substitute `StubInventoryService` and assert on `stock["WIDGET-01"]` remaining at 10 after `PlaceOrder` — observable state survives API reshaping. Adding `Assert.Contains("GADGET-02", result.FailureReasons)` tightens what the failure result communicates. For the interaction verification that remains, prefer `Verify(i => i.Reserve(It.IsAny<string>(), It.IsAny<int>()), Times.Never)` over per-SKU expressions, since the business rule is "no reservation at all" not "no reservation of this specific SKU with this specific quantity."

---

#### Q8. What is the difference between mocking an interface vs a concrete class?

_Answer not found._

---

#### Q9. Why can't Moq intercept non-virtual methods on concrete classes?

_Answer not found._

---

#### Q10. What is `Mock<T>.Setup`, `Returns`, `Callback`, and `Verify` in Moq terms?

_Answer not found._

---

#### Q11. What is over-specification / brittle mocking, and how does it couple tests to implementation?

_Answer not found._

---

#### Q12. What is the difference between verifying behavior (`Verify`) vs asserting output state?

_Answer not found._

---

#### Q13. How do you mock `async` methods returning `Task` / `Task<T>`?

_Answer not found._

---

#### Q14. How do you substitute `HttpClient`, `ILogger<T>`, and `DateTime` abstractions in tests?

_Answer not found._

---

#### Q15. What is a seam, and how do partial wrappers or interfaces introduce testability?

_Answer not found._

---

#### Q16. When is integration testing with real dependencies better than mocking everything?

_Answer not found._

---

#### Q17. What are anti-patterns: mocking concrete DB providers, verifying private collaborators, testing framework code?

_Answer not found._

---

#### Q18. How do manual test doubles (hand-rolled stubs) compare to framework mocks for readability?

_Answer not found._

---

#### Q19. **Testing private methods directly** — Usually signals a design problem; test through public seams or extract collaborators.

_Answer not found._

---

#### Q20. **Shared mutable static state** — Tests pass alone but fail in parallel or random order; isolate with instance state or `[Collection]` serialization.

_Answer not found._

---

#### Q21. **Mocking concrete classes with non-virtual members** — Framework proxies cannot override sealed/non-virtual methods; depend on interfaces.

_Answer not found._

---

#### Q22. **`async void` test methods** — Runners may not observe exceptions; always return `Task` from async tests.

_Answer not found._

---

#### Q23. **Over-verifying mock calls** — `Verify` on every internal call breaks on refactor; assert outcomes and critical interactions only.

_Answer not found._

---

#### Q24. **Integration tests disguised as unit tests** — Real SQL/file/network makes tests slow and flaky; name and folder them honestly.

_Answer not found._

---

#### Q25. **MSTest `[ClassInitialize]` sharing mutable state** — Static setup mutated by one test leaks into others unless reset in `[TestInitialize]`.

_Answer not found._

---

#### Q26. **xUnit class fixtures shared across unrelated tests** — Fixture lifetime is per-class; accidental shared state causes order-dependent failures.

_Answer not found._

---

#### Q27. **Theory data referencing mutable objects** — Shared array/list mutated in one run corrupts later theory cases.

_Answer not found._

---

#### Q28. **Not awaiting async assertions** — `Assert.ThrowsAsync` must be awaited; fire-and-forget hides failures.

_Answer not found._

---

#### Q29. **Assuming test execution order** — xUnit and parallel MSTest do not guarantee order; tests must be independent.

_Answer not found._

---

#### Q30. **Confusing stub with mock** — Stubs set up responses; mocks (strict sense) verify interactions — mixing terms leads to wrong test design.

_Answer not found._
