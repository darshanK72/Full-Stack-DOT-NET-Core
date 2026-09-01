# 09. Unit Testing — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. Unit Testing Basics](#01-unit-testing-basics)
  - [Q1. What is unit testing, and how does it differ from integration, component, and end-to-end testing?](#q1-what-is-unit-testing-and-how-does-it-differ-from-integration-component-and-end-to-end-testing)
  - [Q2. Explain the AAA pattern (Arrange, Act, Assert) and why order matters psychologically for readers.](#q2-explain-the-aaa-pattern-arrange-act-assert-and-why-order-matters-psychologically-for-readers)
  - [Q3. What makes a good unit test (FIRST / TRICE — fast, isolated, repeatable, self-validating, timely)?](#q3-what-makes-a-good-unit-test-first-trice-fast-isolated-repeatable-self-validating-timely)
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
  - [Q2. Explain `[Fact]` vs `[Theory]` — when is parameterized testing appropriate?](#q2-explain-fact-vs-theory-when-is-parameterized-testing-appropriate)
  - [Q3. How do `[InlineData]`, `[MemberData]`, and `[ClassData]` supply theory inputs?](#q3-how-do-inlinedata-memberdata-and-classdata-supply-theory-inputs)
  - [Q4. How does xUnit create test class instances — per test or per class?](#q4-how-does-xunit-create-test-class-instances-per-test-or-per-class)
  - [Q5. What are `IClassFixture<T>` and `ICollectionFixture<T>`, and when use each?](#q5-what-are-iclassfixturet-and-icollectionfixturet-and-when-use-each)
  - [Q6. How do collection definitions (`[Collection("Name")]`) serialize tests that share expensive resources?](#q6-how-do-collection-definitions-collectionname-serialize-tests-that-share-expensive-resources)
  - [Q7. What is `IAsyncLifetime`, and how does it replace async setup/teardown patterns?](#q7-what-is-iasynclifetime-and-how-does-it-replace-async-setupteardown-patterns)
  - [Q8. How does xUnit handle parallel test execution by default, and how do you disable it?](#q8-how-does-xunit-handle-parallel-test-execution-by-default-and-how-do-you-disable-it)
  - [Q9. What is `ITestOutputHelper`, and how is it injected into tests?](#q9-what-is-itestoutputhelper-and-how-is-it-injected-into-tests)
  - [Q10. How do `[Trait("Category", "Slow")]` attributes help filter tests in CI?](#q10-how-do-traitcategory-slow-attributes-help-filter-tests-in-ci)
  - [Q11. How does constructor injection of dependencies work in xUnit test classes?](#q11-how-does-constructor-injection-of-dependencies-work-in-xunit-test-classes)
  - [Q12. What happens if a test constructor throws — how does xUnit report it?](#q12-what-happens-if-a-test-constructor-throws-how-does-xunit-report-it)
  - [Q13. How do you assert exceptions with `Assert.Throws<T>` vs `Assert.ThrowsAsync<T>`?](#q13-how-do-you-assert-exceptions-with-assertthrowst-vs-assertthrowsasynct)
  - [Q14. What is the difference between returning `Task` from a test vs `async void`?](#q14-what-is-the-difference-between-returning-task-from-a-test-vs-async-void)
  - [Q15. How do you run xUnit tests from CLI (`dotnet test`) and filter by fully qualified name?](#q15-how-do-you-run-xunit-tests-from-cli-dotnet-test-and-filter-by-fully-qualified-name)

- [03. MSTest](#03-mstest)
  - [Q1. What NuGet packages compose an MSTest project (`MSTest.TestFramework`, `MSTest.TestAdapter`, `Microsoft.NET.Test.Sdk`)?](#q1-what-nuget-packages-compose-an-mstest-project-mstesttestframework-mstesttestadapter-microsoftnettestsdk)
  - [Q2. Explain `[TestClass]`, `[TestMethod]`, and how discovery finds tests.](#q2-explain-testclass-testmethod-and-how-discovery-finds-tests)
  - [Q3. What are `[DataTestMethod]` and `[DataRow]` equivalents to xUnit theories?](#q3-what-are-datatestmethod-and-datarow-equivalents-to-xunit-theories)
  - [Q4. What is `[TestInitialize]` / `[TestCleanup]` vs `[ClassInitialize]` / `[ClassCleanup]` vs `[AssemblyInitialize]` / `[AssemblyCleanup]`?](#q4-what-is-testinitialize-testcleanup-vs-classinitialize-classcleanup-vs-assemblyinitialize-assemblycleanup)
  - [Q5. Why must `[ClassInitialize]` and `[AssemblyInitialize]` be `static`?](#q5-why-must-classinitialize-and-assemblyinitialize-be-static)
  - [Q6. What is the `[TestContext]` property, and what runtime services does it expose?](#q6-what-is-the-testcontext-property-and-what-runtime-services-does-it-expose)
  - [Q7. How does MSTest instance lifecycle differ from xUnit's new-instance-per-test model?](#q7-how-does-mstest-instance-lifecycle-differ-from-xunits-new-instance-per-test-model)
  - [Q8. What are `[ExpectedException]` / `[ExpectedExceptionAttribute]` (legacy), and why is `Assert.ThrowsException` preferred?](#q8-what-are-expectedexception-expectedexceptionattribute-legacy-and-why-is-assertthrowsexception-preferred)
  - [Q9. What Assert helpers exist in MSTest (`Assert.AreEqual`, `Assert.IsTrue`, `Assert.ThrowsException`, `StringAssert`)?](#q9-what-assert-helpers-exist-in-mstest-assertareequal-assertistrue-assertthrowsexception-stringassert)
  - [Q10. How do you deploy test content files (`[DeploymentItem]`) — and what are modern alternatives?](#q10-how-do-you-deploy-test-content-files-deploymentitem-and-what-are-modern-alternatives)
  - [Q11. What is `[Ignore]` / `[TestCategory]`, and how do you filter categories in `dotnet test`?](#q11-what-is-ignore-testcategory-and-how-do-you-filter-categories-in-dotnet-test)
  - [Q12. How does MSTest parallelization work (`Parallelize` attribute at assembly/class level)?](#q12-how-does-mstest-parallelization-work-parallelize-attribute-at-assemblyclass-level)
  - [Q13. What is the difference between MSTest V1 and V2/V3 adapters in SDK-style projects?](#q13-what-is-the-difference-between-mstest-v1-and-v2v3-adapters-in-sdk-style-projects)
  - [Q14. When would teams choose MSTest over xUnit in greenfield .NET projects?](#q14-when-would-teams-choose-mstest-over-xunit-in-greenfield-net-projects)
  - [Q15. How do MSTest data sources (`[DynamicData]`) compare to xUnit `[MemberData]`?](#q15-how-do-mstest-data-sources-dynamicdata-compare-to-xunit-memberdata)

- [04. Mocking & Test Doubles](#04-mocking-test-doubles)
  - [Q1. Define the test double taxonomy — dummy, fake, stub, spy, mock; what distinguishes each?](#q1-define-the-test-double-taxonomy-dummy-fake-stub-spy-mock-what-distinguishes-each)
  - [Q2. What is a mock in the strict sense (interaction verification) vs informal "mock" meaning any fake?](#q2-what-is-a-mock-in-the-strict-sense-interaction-verification-vs-informal-mock-meaning-any-fake)
  - [Q3. What is a stub, and when do you configure return values without verifying calls?](#q3-what-is-a-stub-and-when-do-you-configure-return-values-without-verifying-calls)
  - [Q4. What is a spy, and how does it record interactions for later assertion?](#q4-what-is-a-spy-and-how-does-it-record-interactions-for-later-assertion)
  - [Q5. What is a fake (e.g., in-memory repository), and when is it preferable to mocks?](#q5-what-is-a-fake-eg-in-memory-repository-and-when-is-it-preferable-to-mocks)
  - [Q6. What is dependency injection's role in making code testable?](#q6-what-is-dependency-injections-role-in-making-code-testable)
  - [Q7. When should you use a mocking framework (Moq, NSubstitute, FakeItEasy) vs hand-written fakes?](#q7-when-should-you-use-a-mocking-framework-moq-nsubstitute-fakeiteasy-vs-hand-written-fakes)
  - [Q8. What is the difference between mocking an interface vs a concrete class?](#q8-what-is-the-difference-between-mocking-an-interface-vs-a-concrete-class)
  - [Q9. Why can't Moq intercept non-virtual methods on concrete classes?](#q9-why-cant-moq-intercept-non-virtual-methods-on-concrete-classes)
  - [Q10. What is `Mock<T>.Setup`, `Returns`, `Callback`, and `Verify` in Moq terms?](#q10-what-is-mocktsetup-returns-callback-and-verify-in-moq-terms)
  - [Q11. What is over-specification / brittle mocking, and how does it couple tests to implementation?](#q11-what-is-over-specification-brittle-mocking-and-how-does-it-couple-tests-to-implementation)
  - [Q12. What is the difference between verifying behavior (`Verify`) vs asserting output state?](#q12-what-is-the-difference-between-verifying-behavior-verify-vs-asserting-output-state)
  - [Q13. How do you mock `async` methods returning `Task` / `Task<T>`?](#q13-how-do-you-mock-async-methods-returning-task-taskt)
  - [Q14. How do you substitute `HttpClient`, `ILogger<T>`, and `DateTime` abstractions in tests?](#q14-how-do-you-substitute-httpclient-iloggert-and-datetime-abstractions-in-tests)
  - [Q15. What is a seam, and how do partial wrappers or interfaces introduce testability?](#q15-what-is-a-seam-and-how-do-partial-wrappers-or-interfaces-introduce-testability)
  - [Q16. When is integration testing with real dependencies better than mocking everything?](#q16-when-is-integration-testing-with-real-dependencies-better-than-mocking-everything)
  - [Q17. What are anti-patterns: mocking concrete DB providers, verifying private collaborators, testing framework code?](#q17-what-are-anti-patterns-mocking-concrete-db-providers-verifying-private-collaborators-testing-framework-code)
  - [Q18. How do manual test doubles (hand-rolled stubs) compare to framework mocks for readability?](#q18-how-do-manual-test-doubles-hand-rolled-stubs-compare-to-framework-mocks-for-readability)
  - [Q19. **Testing private methods directly** — Usually signals a design problem; test through public seams or extract collaborators.](#q19-testing-private-methods-directly-usually-signals-a-design-problem-test-through-public-seams-or-extract-collaborators)
  - [Q20. **Shared mutable static state** — Tests pass alone but fail in parallel or random order; isolate with instance state or `[Collection]` serialization.](#q20-shared-mutable-static-state-tests-pass-alone-but-fail-in-parallel-or-random-order-isolate-with-instance-state-or-collection-serialization)
  - [Q21. **Mocking concrete classes with non-virtual members** — Framework proxies cannot override sealed/non-virtual methods; depend on interfaces.](#q21-mocking-concrete-classes-with-non-virtual-members-framework-proxies-cannot-override-sealednon-virtual-methods-depend-on-interfaces)
  - [Q22. **`async void` test methods** — Runners may not observe exceptions; always return `Task` from async tests.](#q22-async-void-test-methods-runners-may-not-observe-exceptions-always-return-task-from-async-tests)
  - [Q23. **Over-verifying mock calls** — `Verify` on every internal call breaks on refactor; assert outcomes and critical interactions only.](#q23-over-verifying-mock-calls-verify-on-every-internal-call-breaks-on-refactor-assert-outcomes-and-critical-interactions-only)
  - [Q24. **Integration tests disguised as unit tests** — Real SQL/file/network makes tests slow and flaky; name and folder them honestly.](#q24-integration-tests-disguised-as-unit-tests-real-sqlfilenetwork-makes-tests-slow-and-flaky-name-and-folder-them-honestly)
  - [Q25. **MSTest `[ClassInitialize]` sharing mutable state** — Static setup mutated by one test leaks into others unless reset in `[TestInitialize]`.](#q25-mstest-classinitialize-sharing-mutable-state-static-setup-mutated-by-one-test-leaks-into-others-unless-reset-in-testinitialize)
  - [Q26. **xUnit class fixtures shared across unrelated tests** — Fixture lifetime is per-class; accidental shared state causes order-dependent failures.](#q26-xunit-class-fixtures-shared-across-unrelated-tests-fixture-lifetime-is-per-class-accidental-shared-state-causes-order-dependent-failures)
  - [Q27. **Theory data referencing mutable objects** — Shared array/list mutated in one run corrupts later theory cases.](#q27-theory-data-referencing-mutable-objects-shared-arraylist-mutated-in-one-run-corrupts-later-theory-cases)
  - [Q28. **Not awaiting async assertions** — `Assert.ThrowsAsync` must be awaited; fire-and-forget hides failures.](#q28-not-awaiting-async-assertions-assertthrowsasync-must-be-awaited-fire-and-forget-hides-failures)
  - [Q29. **Assuming test execution order** — xUnit and parallel MSTest do not guarantee order; tests must be independent.](#q29-assuming-test-execution-order-xunit-and-parallel-mstest-do-not-guarantee-order-tests-must-be-independent)
  - [Q30. **Confusing stub with mock** — Stubs set up responses; mocks (strict sense) verify interactions — mixing terms leads to wrong test design.](#q30-confusing-stub-with-mock-stubs-set-up-responses-mocks-strict-sense-verify-interactions-mixing-terms-leads-to-wrong-test-design)
  - [Q1. (R) A teammate adds coverage for `GradeCalculator` after reading this chapter's AAA section. The test passes in CI but does not protect behavior. Review:](#q1-r-a-teammate-adds-coverage-for-gradecalculator-after-reading-this-chapters-aaa-section-the-test-passes-in-ci-but-does-not-protect-behavior-review)
  - [Q2. (R) A PR adds "boundary" tests for `IsPassing`. Review:](#q2-r-a-pr-adds-boundary-tests-for-ispassing-review)
  - [Q3. (R) After a refactor, `GradeCalculator` keeps the same public API but replaces nested `if` checks with a private `GradeBand[]` lookup. This test now fails and blocks merge:](#q3-r-after-a-refactor-gradecalculator-keeps-the-same-public-api-but-replaces-nested-if-checks-with-a-private-gradeband-lookup-this-test-now-fails-and-blocks-merge)
  - [Q4. (R) A developer wants tests to "document business rules" and adds:](#q4-r-a-developer-wants-tests-to-document-business-rules-and-adds)
  - [Q5. (R) QA reports that a grading bug shipped despite green tests. The suite includes:](#q5-r-qa-reports-that-a-grading-bug-shipped-despite-green-tests-the-suite-includes)
  - [Q6. (D) Two approaches are proposed for the failing grade at 59 / passing at 60 boundary used in this chapter's `Program.cs` and `GradeCalculatorTests.cs`:](#q6-d-two-approaches-are-proposed-for-the-failing-grade-at-59-passing-at-60-boundary-used-in-this-chapters-programcs-and-gradecalculatortestscs)

- [02. xUnit](#02-xunit-1)

- [02. xUnit](#02-xunit-2)
  - [Q1. (R) A teammate refactors `DiscountTheoryTests` into separate methods "for clarity." CI passes locally but a new `[Fact]` fails intermittently on the build agent. Review:](#q1-r-a-teammate-refactors-discounttheorytests-into-separate-methods-for-clarity-ci-passes-locally-but-a-new-fact-fails-intermittently-on-the-build-agent-review)
  - [Q2. (R) A `[Theory]` backed by `[MemberData]` passes in Visual Studio's Test Explorer but fails unpredictably in `dotnet test` on CI. Review the data source and test:](#q2-r-a-theory-backed-by-memberdata-passes-in-visual-studios-test-explorer-but-fails-unpredictably-in-dotnet-test-on-ci-review-the-data-source-and-test)
  - [Q3. (R) `OrderCatalogFixture` is extended for integration-style tests. Two methods in the same class pass locally but one fails when run with the full suite. Review:](#q3-r-ordercatalogfixture-is-extended-for-integration-style-tests-two-methods-in-the-same-class-pass-locally-but-one-fails-when-run-with-the-full-suite-review)
  - [Q4. (R) Two test classes should share one catalog database seed via `ICollectionFixture<OrderCatalogFixture>`, but tests flake only on multi-core CI agents. Review:](#q4-r-two-test-classes-should-share-one-catalog-database-seed-via-icollectionfixtureordercatalogfixture-but-tests-flake-only-on-multi-core-ci-agents-review)
  - [Q5. (M) A developer asks: "We have three test classes using `IClassFixture<OrderCatalogFixture>`. How many `OrderCatalogFixture` instances does xUnit create for one `dotnet test` run, and when does the fixture constructor run?" They also add `IDisposable` to the fixture to reset state. Explain xUnit's lifecycle for class vs collection fixtures and whether `IDisposable` on the fixture replaces proper test isolation.](#q5-m-a-developer-asks-we-have-three-test-classes-using-iclassfixtureordercatalogfixture-how-many-ordercatalogfixture-instances-does-xunit-create-for-one-dotnet-test-run-and-when-does-the-fixture-constructor-run-they-also-add-idisposable-to-the-fixture-to-reset-state-explain-xunits-lifecycle-for-class-vs-collection-fixtures-and-whether-idisposable-on-the-fixture-replaces-proper-test-isolation)
  - [Q6. (P) Your pipeline runs `dotnet test` on a 4-core Linux agent with no extra flags. Over time, unrelated test classes start failing together — one writes a temp file by fixed name, another reads a static `ConcurrentDictionary`, a third assumes an empty in-memory registry. Locally (often single-threaded or ReSharper's sequential runner) everything passes. What xUnit parallelism rules explain this, and what is your prioritized strategy for CI isolation without disabling all parallelism?](#q6-p-your-pipeline-runs-dotnet-test-on-a-4-core-linux-agent-with-no-extra-flags-over-time-unrelated-test-classes-start-failing-together-one-writes-a-temp-file-by-fixed-name-another-reads-a-static-concurrentdictionary-a-third-assumes-an-empty-in-memory-registry-locally-often-single-threaded-or-resharpers-sequential-runner-everything-passes-what-xunit-parallelism-rules-explain-this-and-what-is-your-prioritized-strategy-for-ci-isolation-without-disabling-all-parallelism)

- [03. MSTest](#03-mstest-1)

- [03. MSTest](#03-mstest-2)
  - [Q1. (R) A `[TestClass]` has two `[TestInitialize]` methods. One test passes in Visual Studio, another fails on CI — and swapping test names changes which one fails. Review:](#q1-r-a-testclass-has-two-testinitialize-methods-one-test-passes-in-visual-studio-another-fails-on-ci-and-swapping-test-names-changes-which-one-fails-review)
  - [Q2. (R) A developer ports money tests from `DiscountCalculatorDataTests.cs` and the project no longer builds. Review:](#q2-r-a-developer-ports-money-tests-from-discountcalculatordatatestscs-and-the-project-no-longer-builds-review)
  - [Q3. (R) A `[DataTestMethod]` is tagged `[TestMethod]` by mistake during a merge. Test Explorer shows one test; the team assumes all discount rows are covered. Review:](#q3-r-a-datatestmethod-is-tagged-testmethod-by-mistake-during-a-merge-test-explorer-shows-one-test-the-team-assumes-all-discount-rows-are-covered-review)
  - [Q4. (R) A pricing test passes locally but fails on a parallel CI agent. Review the class-level setup and two tests:](#q4-r-a-pricing-test-passes-locally-but-fails-on-a-parallel-ci-agent-review-the-class-level-setup-and-two-tests)
  - [Q5. (R) A teammate adds a "quick sanity" test for computed discount rates using `float`. It fails in CI with mismatched expected vs actual. Review:](#q5-r-a-teammate-adds-a-quick-sanity-test-for-computed-discount-rates-using-float-it-fails-in-ci-with-mismatched-expected-vs-actual-review)
  - [Q6. (D) Your team starts a greenfield .NET 8 microservice. `dotnet new xunit` is the SDK default; Visual Studio still offers an MSTest template. Half the org maintains legacy MSTest suites from .NET Framework migrations. For **new** unit-test projects on this service, which framework would you standardize on and why? Cover discovery, lifecycle hooks, parallelism defaults, and when staying on MSTest is the pragmatic choice.](#q6-d-your-team-starts-a-greenfield-net-8-microservice-dotnet-new-xunit-is-the-sdk-default-visual-studio-still-offers-an-mstest-template-half-the-org-maintains-legacy-mstest-suites-from-net-framework-migrations-for-new-unit-test-projects-on-this-service-which-framework-would-you-standardize-on-and-why-cover-discovery-lifecycle-hooks-parallelism-defaults-and-when-staying-on-mstest-is-the-pragmatic-choice)

- [04. Mocking & Test Doubles](#04-mocking-test-doubles-1)

- [04. Mocking & Test Doubles](#04-mocking-test-doubles-2)
  - [Q1. (D) Your team tests `OrderService.PlaceOrder` three different ways — `StubInventoryService`, `FakeEmailSender`, and `Mock<IEmailSender>` with `Verify`. For a new test that asserts "insufficient stock cancels the order and no email is sent," which double type do you pick for **inventory** and for **email**, and why? When would you swap the email side from fake to mock?](#q1-d-your-team-tests-orderserviceplaceorder-three-different-ways-stubinventoryservice-fakeemailsender-and-mockiemailsender-with-verify-for-a-new-test-that-asserts-insufficient-stock-cancels-the-order-and-no-email-is-sent-which-double-type-do-you-pick-for-inventory-and-for-email-and-why-when-would-you-swap-the-email-side-from-fake-to-mock)
  - [Q2. (R) A teammate refactors `OrderServiceMoqTests` to "prove every collaboration." Review the Arrange/Assert block:](#q2-r-a-teammate-refactors-orderservicemoqtests-to-prove-every-collaboration-review-the-arrangeassert-block)
  - [Q3. (R) After a harmless refactor — merging the two `foreach` loops in `OrderService.PlaceOrder` into one pass — CI fails on this test (adapted from `PlaceOrder_WhenStockAvailable_ChecksStockBeforeEveryReserve`):](#q3-r-after-a-harmless-refactor-merging-the-two-foreach-loops-in-orderserviceplaceorder-into-one-pass-ci-fails-on-this-test-adapted-from-placeorder_whenstockavailable_checksstockbeforeeveryreserve)
  - [Q4. (P) Production `OrderService` will call a warehouse REST API through `HttpClient`. You must unit-test `WarehouseInventoryService : IInventoryService` without network I/O. Sketch the test seam — how do you substitute HTTP responses, and why is `new HttpClient()` inside the service a blocker?](#q4-p-production-orderservice-will-call-a-warehouse-rest-api-through-httpclient-you-must-unit-test-warehouseinventoryservice-iinventoryservice-without-network-io-sketch-the-test-seam-how-do-you-substitute-http-responses-and-why-is-new-httpclient-inside-the-service-a-blocker)
  - [Q5. (D) A pricing microservice client is injected into checkout. Your lead asks whether to test it with (A) a custom `HttpMessageHandler` stub in a unit test, (B) `WebApplicationFactory` hitting an in-memory test server, or (C) a contract test against a deployed sandbox. Map each option to a layer of the **test pyramid** for this repo's `OrderService` chapter. When is each the right default?](#q5-d-a-pricing-microservice-client-is-injected-into-checkout-your-lead-asks-whether-to-test-it-with-a-a-custom-httpmessagehandler-stub-in-a-unit-test-b-webapplicationfactory-hitting-an-in-memory-test-server-or-c-a-contract-test-against-a-deployed-sandbox-map-each-option-to-a-layer-of-the-test-pyramid-for-this-repos-orderservice-chapter-when-is-each-the-right-default)
  - [Q6. (R) A junior duplicates production stock logic inside Moq `Setup` chains so tests "stay realistic":](#q6-r-a-junior-duplicates-production-stock-logic-inside-moq-setup-chains-so-tests-stay-realistic)
  - [Q7. (R) Review this failure-path test for the second-line-out-of-stock scenario (`PlaceOrder_WhenSecondLineOutOfStock_DoesNotReserveFirstLine`):](#q7-r-review-this-failure-path-test-for-the-second-line-out-of-stock-scenario-placeorder_whensecondlineoutofstock_doesnotreservefirstline)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

### 01. Unit Testing Basics

#### Q1. What is unit testing, and how does it differ from integration, component, and end-to-end testing?

(R) A teammate adds coverage for `GradeCalculator` after reading this chapter's AAA section. The test passes in CI but does not protect behavior. Review:

```csharp
[Fact]
public void GetLetterGrade_ValidInput_DoesNotThrow()
{
    GradeCalculator calculator = new GradeCalculator();

    calculator.GetLetterGrade(85);
}
```

What is wrong, and how would you rewrite it using the chapter's naming and AAA pattern?

**Answer:** The test has no assertion — it only checks that the call does not throw, so any return value (including `"F"` for 85) still passes. Unit tests must assert observable outcomes, not merely "completed without exception."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Missing Assert phase | False green — wrong grades ship undetected |
| AAA | Act without Assert | Violates Section 3 pattern; test documents nothing |
| Naming | Name claims "DoesNotThrow" but never verifies grade | Misleading failure signal when renamed later |

**Fix (priority order):**

1. Assert the **behavior** — expected letter for a known input — using `Assert.Equal`.
2. Rename to `GetLetterGrade_Score85_ReturnsB` (MethodName_Scenario_ExpectedResult from Section 4).
3. Keep Arrange–Act–Assert comments or blank lines so review catches missing asserts.

```csharp
[Fact]
public void GetLetterGrade_Score85_ReturnsB()
{
    // Arrange
    GradeCalculator calculator = new GradeCalculator();

    // Act
    string grade = calculator.GetLetterGrade(85);

    // Assert
    Assert.Equal("B", grade);
}
```

**Production takeaway:** "No throw" is not a specification unless throwing is the only failure mode you care about — Karat uses this to catch tests that inflate coverage metrics without encoding decisions. See **GradeCalculatorTests.cs** and **Program.cs** Section 9 for correct AAA examples.

---

#### Q2. Explain the AAA pattern (Arrange, Act, Assert) and why order matters psychologically for readers.

(R) A PR adds "boundary" tests for `IsPassing`. Review:

```csharp
[Fact]
public void IsPassing_BoundaryTests()
{
    var calc = new GradeCalculator();

    Assert.False(calc.IsPassing(59));
    bool atThreshold = calc.IsPassing(60);
    Assert.True(atThreshold);
    calc.IsPassing(100);
}
```

Identify AAA violations and any assertion gaps. What would you change before approving the PR?

**Answer:** The method mixes multiple scenarios in one test, interleaves acts and asserts, and ends with an Act (`IsPassing(100)`) that has no assertion — so a regression at the maximum valid score would not fail this test.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| AAA | Multiple Act/Assert cycles in one `[Fact]` | Unclear which scenario failed; harder to maintain |
| Correctness | `IsPassing(100)` never asserted | Upper boundary untested despite being invoked |
| Naming | Generic `BoundaryTests` | Failing CI line does not identify 59 vs 60 vs 100 |
| Design | One test owns three behaviors | Violates "one logical behavior per test" from Section 3 |

**Fix (priority order):**

1. Split into focused tests — mirror **GradeCalculatorTests.cs**: `IsPassing_Score59_ReturnsFalse`, `IsPassing_Score60_ReturnsTrue`.
2. Add `IsPassing_Score100_ReturnsTrue` (or fold max boundary into a dedicated test).
3. Each test: one Arrange, one Act, one Assert (or one exception assert).
4. Optionally use `[Theory]` + `[InlineData]` for table-driven boundaries (preview in Section 8 — full **xUnit** chapter).

**Production takeaway:** Bundled boundary tests look thorough but hide gaps — Karat tests whether you read AAA structure, not just boundary vocabulary. See **Program.cs** Sections 3–4.

---

#### Q3. What makes a good unit test (FIRST / TRICE — fast, isolated, repeatable, self-validating, timely)?

(R) After a refactor, `GradeCalculator` keeps the same public API but replaces nested `if` checks with a private `GradeBand[]` lookup. This test now fails and blocks merge:

```csharp
[Fact]
public void GetLetterGrade_InternalBandCount_IsFive()
{
    var field = typeof(GradeCalculator).GetField(
        "_bands",
        BindingFlags.NonPublic | BindingFlags.Instance);

    var bands = (Array)field!.GetValue(new GradeCalculator())!;

    Assert.Equal(5, bands.Length);
}
```

What category of testing mistake is this, and how should passing thresholds be verified instead?

**Answer:** This is **implementation testing** — it couples the suite to private fields and internal structure, so harmless refactors break CI even when public behavior is unchanged. Tests should target **observable behavior** through the public API (`GetLetterGrade`, `IsPassing`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Reflection on `_bands` | Breaks on rename, reorder, or algorithm change |
| Maintainability | Tests private layout, not contract | Blocks refactors that improve code without changing behavior |
| Correctness | Band count ≠ correct mapping | Could pass with wrong thresholds inside five bands |

**Fix (priority order):**

1. Delete the reflection test; do not widen visibility (`internal` + `InternalsVisibleTo`) unless there is a strong seam reason.
2. Assert **outputs at boundaries** — e.g. 90→`"A"`, 89→`"B"`, 59→not passing, 60→passing — as in **GradeCalculator.cs** and manual tests in **Program.cs** Section 9.
3. For invalid input, keep `Assert.Throws<ArgumentOutOfRangeException>` on public methods (see **GradeCalculatorValidationTests**).
4. If logic grows complex, extract a testable public abstraction (e.g. `IGradeScale`) rather than testing privates.

**Production takeaway:** Karat distinguishes "tests broke" from "behavior broke" — implementation-coupled tests create merge friction without catching user-visible bugs. See Section 5: unit tests exercise one class through its public surface.

---

#### Q4. What is the System Under Test (SUT), and how do you identify its boundaries?

(R) A developer wants tests to "document business rules" and adds:

```csharp
private const int PassingThreshold = 60;

[Fact]
public void IsPassing_ReimplementsThresholdLogic()
{
    int score = 72;
    bool expected = score >= PassingThreshold;

    bool actual = new GradeCalculator().IsPassing(score);

    Assert.Equal(expected, actual);
}
```

Why does this test provide little value and risk false confidence? Show a better test for the same rule.

**Answer:** The test **duplicates production logic** in the test body (`score >= PassingThreshold`), so both sides can be wrong the same way and the assertion still passes. It verifies the copy matches the copy, not that `GradeCalculator` matches the business rule.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Expected value computed with same rule as SUT | Bug in threshold (e.g. `>= 65`) may pass if test constant matches |
| Magic numbers | `72` is arbitrary mid-range | Does not pin the actual decision boundary at 60 |
| Documentation | Test name hides which rule is encoded | Reviewers cannot see boundary intent |

**Fix (priority order):**

1. Replace with **fixed expected outcomes** at meaningful inputs — not derived from the same formula.
2. Test **both sides of the boundary** (59 false, 60 true) as in this chapter's **GradeCalculatorTests.cs**.
3. If the threshold is a business constant, define it once in production code and test boundaries against known cases; do not recompute `expected` from that constant in the test unless testing a pure helper you own separately.

```csharp
[Fact]
public void IsPassing_Score59_ReturnsFalse()
{
    GradeCalculator calculator = new GradeCalculator();
    bool passing = calculator.IsPassing(59);
    Assert.False(passing);
}

[Fact]
public void IsPassing_Score60_ReturnsTrue()
{
    GradeCalculator calculator = new GradeCalculator();
    bool passing = calculator.IsPassing(60);
    Assert.True(passing);
}
```

**Production takeaway:** Tests should be independent oracles — Karat uses duplicated logic in tests to check whether you understand false confidence vs boundary specification.

---

#### Q5. What is test coverage, and why can 100% line coverage still miss important bugs?

(R) QA reports that a grading bug shipped despite green tests. The suite includes:

```csharp
[Fact]
public void GetLetterGrade_Score72_ReturnsC()
{
    Assert.Equal("C", new GradeCalculator().GetLetterGrade(72));
}

[Fact]
public void GetLetterGrade_Score95_ReturnsA()
{
    Assert.Equal("A", new GradeCalculator().GetLetterGrade(95));
}

[Fact]
public void GetLetterGrade_InvalidScore_ThrowsWithExactMessage()
{
    var ex = Assert.Throws<ArgumentOutOfRangeException>(
        () => new GradeCalculator().GetLetterGrade(101));

    Assert.Equal("Score must be between 0 and 100.", ex.Message);
}
```

Which tests are brittle, which gaps remain for `GetLetterGrade`, and what would you add or change?

**Answer:** The exception test is **brittle** against full `Message` strings; the happy-path tests use **unexplained magic numbers** and leave **boundary gaps** (e.g. 89 vs 90, 69 vs 70, 0, 100, negative scores) that would catch off-by-one threshold bugs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Brittleness | Exact `ex.Message` match | Fails on wording/localization/refactor though throw type and param still correct |
| Coverage gap | Only 72 and 95 tested | Wrong B/C or A/B cutoff ships undetected |
| Magic numbers | 72, 95 without comment | Readers cannot tell boundary vs mid-range intent |
| Naming | Inline one-liners skip AAA structure | Harder review for missing cases |

**Fix (priority order):**

1. Assert **exception type** and optionally `paramName` / inner details — not full message text unless message is a stable contract.

```csharp
var ex = Assert.Throws<ArgumentOutOfRangeException>(
    () => new GradeCalculator().GetLetterGrade(101));
Assert.Equal("score", ex.ParamName);
```

2. Add boundary pairs: 90/89 (`A`/`B`), 80/79, 70/69, 60/59 (`D`/`F`) — one act/assert each or `[Theory]` rows.
3. Add 0 and 100 edge cases; keep negative and >100 in **GradeCalculatorValidationTests** style.
4. Prefer names like `GetLetterGrade_Score90_ReturnsA`; use `[InlineData(90, "A")]` with column names in comments if using Theory later.

**Production takeaway:** Green tests with sparse boundaries and stringly exception asserts are a common post-release surprise — Karat tests prioritization of **behavior at edges** over happy-path literals. See **GradeCalculator.cs** thresholds and **Program.cs** Section 9 manual boundary cases.

---

#### Q6. What is mutation testing, and how does it critique coverage metrics?

(D) Two approaches are proposed for the failing grade at 59 / passing at 60 boundary used in this chapter's `Program.cs` and `GradeCalculatorTests.cs`:

**A — one test per boundary side (chapter style):**

```csharp
[Fact] public void IsPassing_Score59_ReturnsFalse() { … Assert.False(calc.IsPassing(59)); }
[Fact] public void IsPassing_Score60_ReturnsTrue()  { … Assert.True(calc.IsPassing(60)); }
```

**B — single parameterized test:**

```csharp
[Theory]
[InlineData(59, false)]
[InlineData(60, true)]
public void IsPassing_AtPassingThreshold_ReturnsExpected(int score, bool expected)
{
    Assert.Equal(expected, new GradeCalculator().IsPassing(score));
}
```

Which approach better supports maintainability and failure diagnosis in a large team, and what naming or data choices still matter?

**Answer:** For two critical boundaries, **Approach A** (chapter style) gives the clearest failure names in CI and matches this folder's teaching convention; **Approach B** scales better when many `(score, expected)` rows exist, but only if each row represents an independent specification — not a duplicate of production logic.

**Approach A — prefer when:**

- Boundaries are few and business-critical (59/60 passing rule in **GradeCalculator.cs**).
- Test Explorer / `dotnet test` output should read like specs: `IsPassing_Score59_ReturnsFalse` fails without opening the method.
- New contributors learn AAA from explicit Arrange/Act/Assert blocks in **GradeCalculatorTests.cs**.

**Approach B — prefer when:**

- Dozens of grade rows would duplicate boilerplate; `[Theory]` + `[InlineData]` or `[MemberData]` reduces noise (full pattern in chapter **02. xUnit**).
- Data is clearly labeled — e.g. `[InlineData(59, false)] // one below passing threshold` — so magic numbers stay documented.
- Team agrees on one theory name: `IsPassing_AtPassingThreshold_ReturnsExpected` is acceptable; avoid vague `BoundaryTests`.

**Shared rules (both approaches):**

- Test **behavior** (`IsPassing` result), not how `score >= 60` is implemented internally.
- Always include **both sides** of the boundary; never only the passing case.
- One logical assert target per test method (A naturally; B requires each row to be a distinct case).

**Production takeaway:** The trade-off is **failure signal clarity** vs **DRY at scale** — Karat expects you to justify choice for team size and data volume, not declare one pattern universally superior. This chapter intentionally uses Approach A in **Program.cs** Section 9 and **GradeCalculatorTests.cs** before introducing `[Theory]` in the next chapter.

---

#### Q7. What is the difference between state-based and interaction-based testing?

_Answer not found._

---

#### Q8. What is a test fixture, and how is it different from a test case?

_Answer not found._

---

#### Q9. What are flaky tests, and what common causes (time, threading, shared state, external I/O)?

_Answer not found._

---

#### Q10. What is the test pyramid, and where do unit tests sit relative to integration tests?

_Answer not found._

---

#### Q11. What is TDD (Red-Green-Refactor), and what benefits/challenges does it bring?

_Answer not found._

---

#### Q12. When should a bug fix include a regression test?

_Answer not found._

---

#### Q13. What is the difference between testing public behavior vs internal implementation?

_Answer not found._

---

#### Q14. How do deterministic tests handle `DateTime.Now`, `Guid.NewGuid()`, and randomness?

_Answer not found._

---

#### Q15. What is arrange duplication, and when is shared setup justified vs harmful?

_Answer not found._

---

### 02. xUnit

#### Q1. What is xUnit.net, and how does its philosophy differ from MSTest and NUnit?

(R) A teammate refactors `DiscountTheoryTests` into separate methods "for clarity." CI passes locally but a new `[Fact]` fails intermittently on the build agent. Review:

```csharp
public sealed class DiscountRefactorTests
{
    private readonly OrderPricingService _pricing = new();
    private decimal _lastDiscountedAmount;

    [Theory]
    [InlineData(100, 10, 90)]
    [InlineData(200, 25, 150)]
    public void ApplyDiscount_ReturnsExpected(
        decimal subtotal, decimal discountPercent, decimal expected)
    {
        _lastDiscountedAmount = _pricing.ApplyDiscount(subtotal, discountPercent);
        Assert.Equal(expected, _lastDiscountedAmount);
    }

    [Fact]
    public void ApplyDiscount_LastRun_Leaves150AfterBulkRow()
    {
        Assert.Equal(150m, _lastDiscountedAmount);
    }
}
```

What is wrong with mixing `[Fact]` and `[Theory]` here, and how should discount tables be tested instead?

**Answer:** The `[Fact]` reads instance field state left behind by whichever `[Theory]` row ran last in the same class — that is an order-dependent coupling, not an isolated scenario. `[Theory]` is for parameterized rows with self-contained assertions; `[Fact]` is for one fixed scenario with no parameters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Test design | `[Fact]` depends on side effect of `[Theory]` rows | Pass/fail varies with theory row order or whether the fact runs alone |
| Misuse of attributes | `[Theory]` used but follow-up scenario not expressed as another data row | Hidden coupling — looks like two tests, behaves like one ordered script |
| Isolation | Mutable `_lastDiscountedAmount` shared across methods in one class | Methods in a class run sequentially but still share instance state |

**Fix (priority order):**

1. Delete the dependent `[Fact]` — if "150 after bulk row" matters, add an explicit `[InlineData(200, 25, 150)]` row or a dedicated `[Fact]` that **arranges its own** inputs.
2. Keep each theory row fully self-contained: arrange, act, assert inside the method — no "last result" fields.
3. Use `[Fact]` only when a single AAA path is enough (see **FactTests.cs**); use `[Theory]` + `[InlineData]` / `[MemberData]` for tables (see **DiscountTheoryTests.cs**).
4. If two scenarios must share expensive setup, use a fixture for **read-only** shared data — not for passing values between tests.

**Production takeaway:** Karat uses `[Fact]` vs `[Theory]` to test whether you know parameterized tests are not a loop you can observe from the next test method. Discount tables belong in `[InlineData]`, not in cross-test field memory.

---

#### Q2. Explain `[Fact]` vs `[Theory]` — when is parameterized testing appropriate?

(R) A `[Theory]` backed by `[MemberData]` passes in Visual Studio's Test Explorer but fails unpredictably in `dotnet test` on CI. Review the data source and test:

```csharp
private static readonly List<OrderLine> SharedLines =
[
    new() { Sku = "A", Quantity = 2, UnitPrice = 25m },
    new() { Sku = "B", Quantity = 1, UnitPrice = 10m },
];

public static IEnumerable<object[]> SubtotalRows =>
    new List<object[]> { new object[] { SharedLines, 60m } };

[Theory]
[MemberData(nameof(SubtotalRows))]
public void CalculateSubtotal_MemberDataRow(
    IReadOnlyList<OrderLine> lines, decimal expected)
{
    lines.Add(new OrderLine { Sku = "EXTRA", Quantity = 1, UnitPrice = 99m });
    decimal subtotal = _pricing.CalculateSubtotal(lines);
    Assert.Equal(expected, subtotal);
}
```

Identify the misuse of `[Theory]` / `[MemberData]` and why the failure is order-dependent.

**Answer:** The theory mutates `SharedLines`, a static list reused by every row and potentially across test runs — the first execution grows the list, so later runs see the wrong line count and subtotal. `[MemberData]` should supply **fresh, immutable** inputs per row, not shared mutable collections the test body edits.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `lines.Add(...)` mutates shared static data | Expected 60m fails after list already contains `EXTRA` |
| Theory misuse | Single shared `List<OrderLine>` referenced from member data | Re-runs, parallel theories, or future extra rows all stomp the same list |
| Design | `IReadOnlyList<T>` parameter does not imply immutability | API suggests read-only; implementation is a mutable `List<T>` |

**Fix (priority order):**

1. Remove mutation from the test — pass lines that already include every SKU under test, or copy before act: `var copy = lines.ToList();`.
2. Return **new** lists per row from `MemberData` (pattern in **DiscountTheoryTests.cs** `SubtotalMemberData`).
3. Prefer immutable fixtures or `IReadOnlyList` backed by arrays/`List.AsReadOnly()` for shared catalogs.
4. Add a second theory row with empty lines to prove rows do not leak state into each other.

```csharp
public static IEnumerable<object[]> SubtotalRows =>
    new List<object[]>
    {
        new object[]
        {
            new List<OrderLine>
            {
                new() { Sku = "A", Quantity = 2, UnitPrice = 25m },
                new() { Sku = "B", Quantity = 1, UnitPrice = 10m },
            },
            60m,
        },
    };
```

**Production takeaway:** `[MemberData]` is for computed tables, not shared scratch buffers — treat each row like a pure function input. See **Program.cs** QUICK REFERENCE — `[MemberData]` for complex inputs.

---

#### Q3. How do `[InlineData]`, `[MemberData]`, and `[ClassData]` supply theory inputs?

(R) `OrderCatalogFixture` is extended for integration-style tests. Two methods in the same class pass locally but one fails when run with the full suite. Review:

```csharp
public sealed class OrderCatalogFixture
{
    public OrderPricingService Service { get; } = new();
    public Order StandardOrder { get; } = OrderCatalogFixture.CreateStandardOrder();
    public List<OrderLine> WorkingLines { get; } = new();
}

public sealed class MutableFixtureTests : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;

    public MutableFixtureTests(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void AddPromoLine_IncludesLineInWorkingSet()
    {
        _fixture.WorkingLines.Add(
            new OrderLine { Sku = "PROMO", Quantity = 1, UnitPrice = 5m });
        Assert.Single(_fixture.WorkingLines);
    }

    [Fact]
    public void WorkingLines_StartsEmptyEachTest()
    {
        Assert.Empty(_fixture.WorkingLines);
    }
}
```

What does `IClassFixture<T>` guarantee about instance lifetime, and why does the second test fail?

**Answer:** xUnit creates **one** `OrderCatalogFixture` per test class and injects the same instance into every test method constructor in that class — it does **not** reset mutable state between tests. After `AddPromoLine` runs, `WorkingLines` still holds the promo line when `WorkingLines_StartsEmptyEachTest` runs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Fixture lifetime | `IClassFixture<T>` = one shared instance per class | Mutable lists on the fixture leak across tests |
| False isolation assumption | Treating fixture like per-test `[SetUp]` | Second test fails depending on method order within the class |
| Design | Expensive **read-only** seed mixed with per-test scratch state | Same object used for two incompatible purposes |

**Fix (priority order):**

1. Keep fixtures **immutable** after construction — match **OrderCatalogFixture.cs** (`Service`, `StandardOrder` only).
2. Put per-test mutable state in local variables or a fresh `List<OrderLine>` inside each test method.
3. If reset is unavoidable, implement `IDisposable` on the fixture and clear `WorkingLines` in `Dispose()` — xUnit calls it after all tests in the class finish (not between each test).
4. For per-test reset, use a constructor on the test class or `IAsyncLifetime` per test — not shared mutable fixture fields.

**Production takeaway:** `IClassFixture<T>` shares setup cost, not test isolation. ClassFixtureTests in this chapter shares a **read-only** catalog — Karat checks whether you would add mutable `WorkingLines` and expect a clean slate.

---

#### Q4. How does xUnit create test class instances — per test or per class?

(R) Two test classes should share one catalog database seed via `ICollectionFixture<OrderCatalogFixture>`, but tests flake only on multi-core CI agents. Review:

```csharp
[CollectionDefinition("OrderCatalog")]
public sealed class OrderCatalogCollection : ICollectionFixture<OrderCatalogFixture> { }

[Collection("OrderCatalog")]
public sealed class CatalogTestsA : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;
    public CatalogTestsA(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void A_MarksCatalogLoaded() => _fixture.StandardOrder.OrderId = "LOADED";
}

// Missing [Collection("OrderCatalog")] — "just uses the fixture type"
public sealed class CatalogTestsB : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;
    public CatalogTestsB(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void B_ExpectsDefaultOrderId()
    {
        Assert.Equal("ORD-TEST-001", _fixture.StandardOrder.OrderId);
    }
}
```

What parallelization and fixture-lifetime mistakes stack here, and how do you fix them?

**Answer:** `CatalogTestsB` is **not** in the `"OrderCatalog"` collection, so xUnit gives it a **separate** `OrderCatalogFixture` and may run it **in parallel** with collection members — while `CatalogTestsA` mutates shared `StandardOrder`, creating cross-class interference and nondeterministic order IDs. Combining `[Collection]` with redundant `IClassFixture<T>` on the same class also confuses which fixture instance applies.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Collection membership | `CatalogTestsB` missing `[Collection("OrderCatalog")]` | Different fixture instance; parallel with collection tests |
| Parallelism | Non-collection classes run in parallel by default | **ParallelExecutionTests.cs** — unrelated classes use separate threads |
| Shared mutation | `StandardOrder.OrderId = "LOADED"` on collection-shared object | Other tests in the collection see mutated order |
| Fixture wiring | `IClassFixture<OrderCatalogFixture>` **and** `[Collection]` for same type | Extra instance per class unless only collection fixture is used |

**Fix (priority order):**

1. Add `[Collection("OrderCatalog")]` to **every** class that must share the seed and serialize together — see **CollectionFixtureTestsA.cs** / **CollectionFixtureTestsB.cs**.
2. Inject `OrderCatalogFixture` via collection fixture only; drop redundant `IClassFixture<OrderCatalogFixture>` on collection classes unless you intentionally want a second instance.
3. Stop mutating shared fixture objects — treat `StandardOrder` as read-only; clone before changes.
4. If global mutation is required, keep all writers/readers in one collection and document serial execution — do not rely on "we only have two classes."

**Production takeaway:** `[CollectionDefinition]` + `[Collection]` is xUnit's lock for shared expensive resources **and** parallel scheduling. Missing `[Collection]` is a classic CI-only flake when cores > 1. See **OrderCatalogCollection.cs** and **Program.cs** Section 7–8.

---

#### Q5. What are `IClassFixture<T>` and `ICollectionFixture<T>`, and when use each?

(M) A developer asks: "We have three test classes using `IClassFixture<OrderCatalogFixture>`. How many `OrderCatalogFixture` instances does xUnit create for one `dotnet test` run, and when does the fixture constructor run?" They also add `IDisposable` to the fixture to reset state. Explain xUnit's lifecycle for class vs collection fixtures and whether `IDisposable` on the fixture replaces proper test isolation.

**Answer:** Three unrelated classes each implementing `IClassFixture<OrderCatalogFixture>` get **three separate instances** — one per class, constructed before the first test in that class runs and disposed (if `IDisposable`) after the last test in that class finishes. Classes in the same `[Collection("OrderCatalog")]` share **one** collection fixture instance for the whole collection, and tests in that collection do not run in parallel with each other.

- **`IClassFixture<T>`:** One `T` per test class; ctor runs once per class; same reference injected into every test method ctor on that class; tests in the class run **sequentially**.
- **`ICollectionFixture<T>`:** One `T` shared by all classes with the same `[Collection("Name")]`; ctor runs once for the collection; disposed after all tests in all member classes complete.
- **`IDisposable` on fixture:** Called after the **class** (class fixture) or **collection** (collection fixture) completes — **not** between individual tests. Clearing mutable state in `Dispose()` does not help test B if test A mutated the fixture mid-class.
- **Proper isolation:** Immutable fixture data, per-test local state, or `IAsyncLifetime.InitializeAsync` per test class instance — not "reset in Dispose only."
- **Contrast with this chapter:** `ClassFixtureTests` gets its own fixture; `CollectionFixtureTestsA/B` share one — compare subtotals and order IDs across files to prove instance boundaries.

**Production takeaway:** Know instance counts when diagnosing "why did my seed run twice" or "why do these classes see different catalogs." `IDisposable` is for teardown of external resources (files, connections), not a substitute for tests that do not mutate shared state.

---

#### Q6. How do collection definitions (`[Collection("Name")]`) serialize tests that share expensive resources?

(P) Your pipeline runs `dotnet test` on a 4-core Linux agent with no extra flags. Over time, unrelated test classes start failing together — one writes a temp file by fixed name, another reads a static `ConcurrentDictionary`, a third assumes an empty in-memory registry. Locally (often single-threaded or ReSharper's sequential runner) everything passes. What xUnit parallelism rules explain this, and what is your prioritized strategy for CI isolation without disabling all parallelism?

**Answer:** xUnit runs **different test classes in parallel** by default while methods inside the same class run sequentially — so static fields, fixed temp paths, and process-wide singletons race across classes on a 4-core agent. Locally, sequential IDE runners or fewer cores hide the overlap.

- **Default rules:** Different classes → parallel; same class → serial; same `[Collection]` → serial with shared collection fixture (**ParallelExecutionTests.cs**, **Program.cs** QUICK REFERENCE).
- **Why CI differs:** Agents use full CPU; `--parallel` / default xUnit worker count > 1; no guaranteed class order; reruns and TRX merge do not reset static process state.
- **Prioritized strategy:**
  1. **Fix tests first** — remove static mutable state, use `Path.GetTempFileName()` or `Guid` suffixes, inject fakes instead of static registries.
  2. **Group offenders** into a named `[Collection("SerialIntegration")]` so they share one synchronization boundary and optional shared fixture.
  3. **Run truly integration-heavy** projects in a separate CI job with `-- xUnit.parallelizeTestCollections=false` or `[assembly: CollectionBehavior(DisableTestParallelization = true)]` only for that assembly — not the whole solution.
  4. **Keep unit tests parallel** — stateless `[Fact]` / `[Theory]` tests on `OrderPricingService`-style pure logic scale well and match **FactTests.cs** / **DiscountTheoryTests.cs** patterns.
  5. **Detect flakes** — repeat `dotnet test --repeat 50` on PR for known parallel-sensitive assemblies; fail on nondeterminism before merge.
- **Avoid:** Global disable of parallelism for every test project because one legacy suite uses static state — that punishes fast unit tests and lengthens every pipeline.

**Production takeaway:** Test isolation in CI is an xUnit scheduling problem plus code hygiene. Prefer collection-scoped serialization over killing parallelism solution-wide — aligns with **Program.cs** Section 10 and production pipelines that rely on multi-core `dotnet test`.

---

---

#### Q7. What is `IAsyncLifetime`, and how does it replace async setup/teardown patterns?

_Answer not found._

---

#### Q8. How does xUnit handle parallel test execution by default, and how do you disable it?

_Answer not found._

---

#### Q9. What is `ITestOutputHelper`, and how is it injected into tests?

_Answer not found._

---

#### Q10. How do `[Trait("Category", "Slow")]` attributes help filter tests in CI?

_Answer not found._

---

#### Q11. How does constructor injection of dependencies work in xUnit test classes?

_Answer not found._

---

#### Q12. What happens if a test constructor throws — how does xUnit report it?

_Answer not found._

---

#### Q13. How do you assert exceptions with `Assert.Throws<T>` vs `Assert.ThrowsAsync<T>`?

_Answer not found._

---

#### Q14. What is the difference between returning `Task` from a test vs `async void`?

_Answer not found._

---

#### Q15. How do you run xUnit tests from CLI (`dotnet test`) and filter by fully qualified name?

_Answer not found._

---

### 03. MSTest

#### Q1. What NuGet packages compose an MSTest project (`MSTest.TestFramework`, `MSTest.TestAdapter`, `Microsoft.NET.Test.Sdk`)?

(R) A `[TestClass]` has two `[TestInitialize]` methods. One test passes in Visual Studio, another fails on CI — and swapping test names changes which one fails. Review:

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

**Answer:** MSTest runs every `[TestInitialize]` method before each test, but **does not guarantee order** between those methods — `SeedFromTierDiscount` can run before `CreateCalculator`, leaving `_calculator` null and causing `NullReferenceException` or wrong state depending on discovery order.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifecycle | Two `[TestInitialize]` methods with hidden dependency | Non-deterministic setup — pass/fail varies by runner ordering |
| Design | `_calculator` used before guaranteed initialization | Intermittent CI failures when test discovery order changes |
| Maintainability | Split setup across unordered hooks | Next refactor breaks tests silently |

**Fix (priority order):**

1. Merge setup into **one** `[TestInitialize]` with explicit top-to-bottom order — create SUT first, then seed derived state (matches `DiscountCalculatorCoreTests.cs` in this chapter).
2. Never assume one `[TestInitialize]` runs before another; if phases are required, call private helpers from a single hook in the order you need.
3. Keep each `[TestMethod]` independent — no reliance on another test having run first.
4. Run `dotnet test` on CI (not only Test Explorer) to catch ordering assumptions early.

```csharp
[TestInitialize]
public void SetUp()
{
    _calculator = new DiscountCalculator();
    _seededTotal = _calculator.ApplyTierDiscount(100.00m, CustomerTier.Silver);
}
```

**Production takeaway:** `[TestInitialize]` is "before each test," not "ordered pipeline stages" — Karat uses multi-hook classes to test whether you know MSTest lifecycle is per-test but unordered across hooks. See **Program.cs** Section 5 and `DiscountCalculatorCoreTests.cs`.

---

#### Q2. Explain `[TestClass]`, `[TestMethod]`, and how discovery finds tests.

(R) A developer ports money tests from `DiscountCalculatorDataTests.cs` and the project no longer builds. Review:

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

**Answer:** `[DataRow]` attribute arguments must be compile-time constants of types MSTest supports — **`decimal` literals are not valid `[DataRow]` arguments**, so the build fails even though `decimal` works fine in normal C# code and in the SUT.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `[DataRow(100.00m, …)]` — decimal not accepted | CS1503 / attribute argument errors — tests never run |
| Domain | Money calculations need `decimal` in production code | Forcing `double` in rows risks precision loss if cast is forgotten |
| Pattern drift | Ignores chapter convention in `DiscountCalculatorDataTests.cs` | Team copies broken snippet into CI |

**Fix (priority order):**

1. Keep `[DataRow]` values as **`double` literals** (or other supported types like `int`, `string`, `enum`).
2. Cast to `decimal` **inside** the test body before calling `ApplyPercentDiscount` — exactly as this chapter does.
3. Assert with `decimal` via `Assert.AreEqual((decimal)expectedTotal, result)` for money.
4. For large or computed tables, use `[DynamicData]` from a static method returning `IEnumerable<object[]>` when `[DataRow]` constants are too limiting.

```csharp
[DataTestMethod]
[DataRow(100.00, 0.10, 90.00)]
public void ApplyPercentDiscount_ReturnsExpectedTotal(
    double orderTotal, double discountRate, double expectedTotal)
{
    decimal result = new DiscountCalculator().ApplyPercentDiscount(
        (decimal)orderTotal, (decimal)discountRate);
    Assert.AreEqual((decimal)expectedTotal, result);
}
```

**Production takeaway:** Parameterized MSTest tests carry a compile-time type contract separate from your domain model — always cast at the boundary. See **Program.cs** Section 6 pitfall and `DiscountCalculatorDataTests.cs`.

---

#### Q3. What are `[DataTestMethod]` and `[DataRow]` equivalents to xUnit theories?

(R) A `[DataTestMethod]` is tagged `[TestMethod]` by mistake during a merge. Test Explorer shows one test; the team assumes all discount rows are covered. Review:

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
    { /* ... */ }
}
```

What breaks at compile time or discovery time, and why is `[DataTestMethod]` required here?

**Answer:** `[TestMethod]` methods **must have no parameters** — a parameterized signature with `[DataRow]` requires `[DataTestMethod]`, otherwise the compiler rejects the method or the adapter will not expand rows into separate test cases.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile / discovery | `[TestMethod]` + parameters + `[DataRow]` mismatch | Build error or single undiscoverable method — not three row cases |
| Coverage illusion | One green test in Explorer | Two discount scenarios never executed — regression slips to prod |
| Merge hygiene | Attribute regression during conflict resolution | Silent loss of data-driven coverage |

**Fix (priority order):**

1. Replace `[TestMethod]` with **`[DataTestMethod]`** on any method that takes `[DataRow]` arguments.
2. Verify Test Explorer / `dotnet test` lists **one result per row** (e.g., `…(100,0.1,90)`, `…(50,0.2,40)`).
3. Add CI coverage gates or `--filter` spot checks on critical `[DataTestMethod]` classes after merges.
4. Prefer explicit naming so row failures identify inputs in the log.

**Production takeaway:** `[DataRow]` without `[DataTestMethod]` is a common merge accident — Karat checks whether you know MSTest distinguishes single-case `[TestMethod]` from parameterized `[DataTestMethod]`. See **Program.cs** Sections 3 and 6.

---

#### Q4. What is `[TestInitialize]` / `[TestCleanup]` vs `[ClassInitialize]` / `[ClassCleanup]` vs `[AssemblyInitialize]` / `[AssemblyCleanup]`?

(R) A pricing test passes locally but fails on a parallel CI agent. Review the class-level setup and two tests:

```csharp
[TestClass]
public class TierRateTableTests
{
    private static Dictionary<CustomerTier, decimal> _tierRateTable = null!;
    // ... ClassInitialize loads dictionary ...
    // OverrideGoldRate_ForPromoScenario mutates _tierRateTable[Gold] = 0.25m
    // GoldRate_MatchesClassInitializeDefault expects 0.15m
}
```

What does `[ClassInitialize]` actually share across tests, and why is the second test flaky?

**Answer:** `[ClassInitialize]` runs **once per `[TestClass]`** and the static `_tierRateTable` is **shared by every test in that class** — mutating it in one `[TestMethod]` leaks state into others, and parallel runners make execution order non-deterministic so the "default rate" test flakes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable static dictionary written in one test, read in another | Order-dependent pass/fail — classic flake on parallel CI |
| Lifecycle misuse | `[ClassInitialize]` treated as "read-only reference data" but not enforced | Promo override permanently changes shared table for the class run |
| Parallelism | MSTest can run tests concurrently within an assembly (V3+ / `[Parallelize]`) | Shared mutable statics race across threads |

**Fix (priority order):**

1. Treat `[ClassInitialize]` data as **immutable** — `IReadOnlyDictionary<CustomerTier, decimal>` or copy-on-read, never mutate in tests (pattern in `DiscountCalculatorLifecycleTests.cs`).
2. If a test needs a modified table, **clone** the dictionary in `[TestInitialize]` or build local expected values without touching static state.
3. Reset statics in `[ClassCleanup]` only for teardown of external resources — not as a substitute for per-test isolation.
4. For expensive shared **services**, prefer instance fields set in `[TestInitialize]` with a fresh SUT; reserve class-level hooks for truly read-only, expensive setup.

```csharp
[ClassInitialize]
public static void InitClass(TestContext context)
{
    _tierRateTable = new ReadOnlyDictionary<CustomerTier, decimal>(
        new Dictionary<CustomerTier, decimal> { /* rates */ });
}

// In test: decimal expectedRate = _tierRateTable[tier]; — read only
```

**Production takeaway:** `[ClassInitialize]` is one-time shared setup, not a per-test reset — same judgment as xUnit `IClassFixture` mutation traps (Chapter 02). See **Program.cs** Section 8 and `DiscountCalculatorLifecycleTests.cs`.

---

#### Q5. Why must `[ClassInitialize]` and `[AssemblyInitialize]` be `static`?

(R) A teammate adds a "quick sanity" test for computed discount rates using `float`. It fails in CI with mismatched expected vs actual. Review:

```csharp
[TestMethod]
public void ApplyPercentDiscount_TenPercentOnHundred_ReturnsNinetyAsFloat()
{
    float discounted = (float)calculator.ApplyPercentDiscount(100.0m, 0.10m);
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

**Answer:** Binary floating-point (`float`/`double`) cannot represent many decimal fractions exactly — exact `Assert.AreEqual` without tolerance fails even when the math is "correct" at business precision (e.g., `0.1 + 0.2 != 0.3`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Exact equality on `float`/`double` | False failures — tests red while production code is fine |
| Domain | Money path cast to `float` for assertion | Loses decimal precision the SUT was designed for |
| API misuse | Two-parameter `AreEqual` on floats | Ignores MSTest's delta overload meant for this scenario |

**Fix (priority order):**

1. For **money**, stay on `decimal` end-to-end — `Assert.AreEqual(90.00m, result)` as in `DiscountCalculatorAssertTests.cs`.
2. When asserting `float`/`double` is unavoidable, use the **delta overload**: `Assert.AreEqual(90.0f, discounted, 0.001f)` or appropriate epsilon.
3. Delete or rewrite "sanity" tests that cast pricing results to `float` — they test the wrong type.
4. Document team standard: financial asserts use `decimal`; floating-point asserts always specify tolerance.

```csharp
Assert.AreEqual(0.3, sum, 1e-10);           // double with delta
Assert.AreEqual(90.0f, discounted, 0.01f);  // float with delta — prefer decimal for money
```

**Production takeaway:** `Assert.AreEqual` has value-type and reference-type paths — for `double`/`float` the three-argument delta form is the production-safe pattern. See **Program.cs** Section 7.

---

#### Q6. What is the `[TestContext]` property, and what runtime services does it expose?

(D) Your team starts a greenfield .NET 8 microservice. `dotnet new xunit` is the SDK default; Visual Studio still offers an MSTest template. Half the org maintains legacy MSTest suites from .NET Framework migrations. For **new** unit-test projects on this service, which framework would you standardize on and why? Cover discovery, lifecycle hooks, parallelism defaults, and when staying on MSTest is the pragmatic choice.

**Answer:** For most greenfield .NET 8 services, **standardize on xUnit** — SDK templates, community samples, and OSS libraries target it first — while **keeping MSTest** where enterprise templates, existing suites, or Visual Studio-centric workflows dominate.

- **Greenfield default → xUnit:** `dotnet new xunit` is zero-friction; `[Fact]` / `[Theory]` + `[InlineData]` map cleanly from MSTest once attribute names are known (see **Program.cs** Section 9 table). Constructor + `IDisposable` replace `[TestInitialize]` / `[TestCleanup]` with explicit C# lifecycle.
- **Discovery & CLI:** Both run under `Microsoft.NET.Test.Sdk` and `dotnet test`; filter syntax differs slightly but CI integration is equivalent. Pick one framework per solution/service to avoid duplicate adapter packages and mixed conventions.
- **Parallelism:** xUnit parallelizes by default across collections — good for speed, requires immutable fixtures (Chapter 02). MSTest historically ran more sequentially per assembly; V3+ improves parallelism but teams must opt in with `[Parallelize]` and avoid mutable statics from `[ClassInitialize]`.
- **Lifecycle mapping:** `[ClassInitialize]` / `[ClassCleanup]` ↔ `IClassFixture<T>` / `IDisposable`; `[AssemblyInitialize]` ↔ custom assembly fixtures or `AssemblyFixture` patterns — xUnit's fixture model is more composable for large suites.
- **When MSTest is pragmatic:** Large legacy MSTest bank being extended incrementally; org mandate / Azure DevOps templates; mixed-skill teams already trained on `[TestClass]` / Test Explorer groupings; internal libraries with MSTest examples only.
- **Migration strategy:** New microservice → xUnit; shared test utilities abstract SUT setup so framework attributes stay thin; do not run MSTest and xUnit test projects with conflicting static state in one CI job without understanding parallel scope.

**Production takeaway:** Framework choice is a team maintainability decision, not a capability gap — both run on `dotnet test`. Prefer one convention per repo boundary; know the attribute map in **Program.cs** QUICK REFERENCE when reading either style in code review.

---

---

#### Q7. How does MSTest instance lifecycle differ from xUnit's new-instance-per-test model?

_Answer not found._

---

#### Q8. What are `[ExpectedException]` / `[ExpectedExceptionAttribute]` (legacy), and why is `Assert.ThrowsException` preferred?

_Answer not found._

---

#### Q9. What Assert helpers exist in MSTest (`Assert.AreEqual`, `Assert.IsTrue`, `Assert.ThrowsException`, `StringAssert`)?

_Answer not found._

---

#### Q10. How do you deploy test content files (`[DeploymentItem]`) — and what are modern alternatives?

_Answer not found._

---

#### Q11. What is `[Ignore]` / `[TestCategory]`, and how do you filter categories in `dotnet test`?

_Answer not found._

---

#### Q12. How does MSTest parallelization work (`Parallelize` attribute at assembly/class level)?

_Answer not found._

---

#### Q13. What is the difference between MSTest V1 and V2/V3 adapters in SDK-style projects?

_Answer not found._

---

#### Q14. When would teams choose MSTest over xUnit in greenfield .NET projects?

_Answer not found._

---

#### Q15. How do MSTest data sources (`[DynamicData]`) compare to xUnit `[MemberData]`?

_Answer not found._

---

### 04. Mocking & Test Doubles

#### Q1. Define the test double taxonomy — dummy, fake, stub, spy, mock; what distinguishes each?

(D) Your team tests `OrderService.PlaceOrder` three different ways — `StubInventoryService`, `FakeEmailSender`, and `Mock<IEmailSender>` with `Verify`. For a new test that asserts "insufficient stock cancels the order and no email is sent," which double type do you pick for **inventory** and for **email**, and why? When would you swap the email side from fake to mock?

**Answer:** Use a **stub** (or minimal mock `Setup`) for inventory to force `HasStock` to return `false`, and a **fake** for email so you assert `SentMessages` is empty — the same pattern as `PlaceOrder_WhenStubReportsLowStock_FailsWithoutEmailOrReservation` in **OrderServiceManualDoubleTests.cs**. Swap email to a mock when you only need a spy (`Verify(..., Times.Never)`) and do not care about inspecting message contents — as in **OrderServiceMoqTests.cs** failure tests.

- **Inventory → stub:** The scenario needs a canned `false` from `HasStock("WIDGET-01", 5)` when stock is 1. A stub dictionary or one `Setup().Returns(false)` is enough; no need to verify `Reserve` was never called if stock state stays unchanged (stub) or you add a single `Verify(..., Times.Never)` on reserve.
- **Email → fake (default here):** `FakeEmailSender.SentMessages` gives a readable behavior assertion (`Assert.Empty`) without Moq ceremony. The chapter's **Program.cs** Section 5 demo uses this path.
- **Email → mock:** Prefer `Mock<IEmailSender>` + `Verify(..., Times.Never)` when the test file is already Moq-centric, when email is not the focus and you want consistent mock-only Arrange blocks, or when multiple void collaborators each need `Times.Never` in one Assert section.
- **Avoid mock for inventory in this scenario** unless you also need to prove `Reserve` was never invoked — the stub's unchanged dictionary already proves no reservation side effect.
- **Production takeaway:** Pick doubles by **what you need to observe** — return values (stub), accumulated state (fake), or interaction counts (mock/spy). See **Program.cs** Section 2 taxonomy table.

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

#### Q3. What is a stub, and when do you configure return values without verifying calls?

(R) After a harmless refactor — merging the two `foreach` loops in `OrderService.PlaceOrder` into one pass — CI fails on this test (adapted from `PlaceOrder_WhenStockAvailable_ChecksStockBeforeEveryReserve`):

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

#### Q4. What is a spy, and how does it record interactions for later assertion?

(P) Production `OrderService` will call a warehouse REST API through `HttpClient`. You must unit-test `WarehouseInventoryService : IInventoryService` without network I/O. Sketch the test seam — how do you substitute HTTP responses, and why is `new HttpClient()` inside the service a blocker?

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

#### Q5. What is a fake (e.g., in-memory repository), and when is it preferable to mocks?

(D) A pricing microservice client is injected into checkout. Your lead asks whether to test it with (A) a custom `HttpMessageHandler` stub in a unit test, (B) `WebApplicationFactory` hitting an in-memory test server, or (C) a contract test against a deployed sandbox. Map each option to a layer of the **test pyramid** for this repo's `OrderService` chapter. When is each the right default?

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

#### Q6. What is dependency injection's role in making code testable?

(R) A junior duplicates production stock logic inside Moq `Setup` chains so tests "stay realistic":

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

#### Q7. When should you use a mocking framework (Moq, NSubstitute, FakeItEasy) vs hand-written fakes?

(R) Review this failure-path test for the second-line-out-of-stock scenario (`PlaceOrder_WhenSecondLineOutOfStock_DoesNotReserveFirstLine`):

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

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A teammate adds coverage for `GradeCalculator` after reading this chapter's AAA section. The test passes in CI but does not protect behavior. Review:

```csharp
[Fact]
public void GetLetterGrade_ValidInput_DoesNotThrow()
{
    GradeCalculator calculator = new GradeCalculator();

    calculator.GetLetterGrade(85);
}
```

What is wrong, and how would you rewrite it using the chapter's naming and AAA pattern?

---

**Answer:**

```csharp
[Fact]
public void GetLetterGrade_ValidInput_DoesNotThrow()
{
    GradeCalculator calculator = new GradeCalculator();

    calculator.GetLetterGrade(85);
}
```

What is wrong, and how would you rewrite it using the chapter's naming and AAA pattern?

**Answer:** The test has no assertion — it only checks that the call does not throw, so any return value (including `"F"` for 85) still passes. Unit tests must assert observable outcomes, not merely "completed without exception."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Missing Assert phase | False green — wrong grades ship undetected |
| AAA | Act without Assert | Violates Section 3 pattern; test documents nothing |
| Naming | Name claims "DoesNotThrow" but never verifies grade | Misleading failure signal when renamed later |

**Fix (priority order):**

1. Assert the **behavior** — expected letter for a known input — using `Assert.Equal`.
2. Rename to `GetLetterGrade_Score85_ReturnsB` (MethodName_Scenario_ExpectedResult from Section 4).
3. Keep Arrange–Act–Assert comments or blank lines so review catches missing asserts.

```csharp
[Fact]
public void GetLetterGrade_Score85_ReturnsB()
{
    // Arrange
    GradeCalculator calculator = new GradeCalculator();

    // Act
    string grade = calculator.GetLetterGrade(85);

    // Assert
    Assert.Equal("B", grade);
}
```

**Production takeaway:** "No throw" is not a specification unless throwing is the only failure mode you care about — Karat uses this to catch tests that inflate coverage metrics without encoding decisions. See **GradeCalculatorTests.cs** and **Program.cs** Section 9 for correct AAA examples.

---

---

#### Q2. (R) A PR adds "boundary" tests for `IsPassing`. Review:

```csharp
[Fact]
public void IsPassing_BoundaryTests()
{
    var calc = new GradeCalculator();

    Assert.False(calc.IsPassing(59));
    bool atThreshold = calc.IsPassing(60);
    Assert.True(atThreshold);
    calc.IsPassing(100);
}
```

Identify AAA violations and any assertion gaps. What would you change before approving the PR?

---

**Answer:**

```csharp
[Fact]
public void IsPassing_BoundaryTests()
{
    var calc = new GradeCalculator();

    Assert.False(calc.IsPassing(59));
    bool atThreshold = calc.IsPassing(60);
    Assert.True(atThreshold);
    calc.IsPassing(100);
}
```

Identify AAA violations and any assertion gaps. What would you change before approving the PR?

**Answer:** The method mixes multiple scenarios in one test, interleaves acts and asserts, and ends with an Act (`IsPassing(100)`) that has no assertion — so a regression at the maximum valid score would not fail this test.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| AAA | Multiple Act/Assert cycles in one `[Fact]` | Unclear which scenario failed; harder to maintain |
| Correctness | `IsPassing(100)` never asserted | Upper boundary untested despite being invoked |
| Naming | Generic `BoundaryTests` | Failing CI line does not identify 59 vs 60 vs 100 |
| Design | One test owns three behaviors | Violates "one logical behavior per test" from Section 3 |

**Fix (priority order):**

1. Split into focused tests — mirror **GradeCalculatorTests.cs**: `IsPassing_Score59_ReturnsFalse`, `IsPassing_Score60_ReturnsTrue`.
2. Add `IsPassing_Score100_ReturnsTrue` (or fold max boundary into a dedicated test).
3. Each test: one Arrange, one Act, one Assert (or one exception assert).
4. Optionally use `[Theory]` + `[InlineData]` for table-driven boundaries (preview in Section 8 — full **xUnit** chapter).

**Production takeaway:** Bundled boundary tests look thorough but hide gaps — Karat tests whether you read AAA structure, not just boundary vocabulary. See **Program.cs** Sections 3–4.

---

---

#### Q3. (R) After a refactor, `GradeCalculator` keeps the same public API but replaces nested `if` checks with a private `GradeBand[]` lookup. This test now fails and blocks merge:

```csharp
[Fact]
public void GetLetterGrade_InternalBandCount_IsFive()
{
    var field = typeof(GradeCalculator).GetField(
        "_bands",
        BindingFlags.NonPublic | BindingFlags.Instance);

    var bands = (Array)field!.GetValue(new GradeCalculator())!;

    Assert.Equal(5, bands.Length);
}
```

What category of testing mistake is this, and how should passing thresholds be verified instead?

---

**Answer:**

```csharp
[Fact]
public void GetLetterGrade_InternalBandCount_IsFive()
{
    var field = typeof(GradeCalculator).GetField(
        "_bands",
        BindingFlags.NonPublic | BindingFlags.Instance);

    var bands = (Array)field!.GetValue(new GradeCalculator())!;

    Assert.Equal(5, bands.Length);
}
```

What category of testing mistake is this, and how should passing thresholds be verified instead?

**Answer:** This is **implementation testing** — it couples the suite to private fields and internal structure, so harmless refactors break CI even when public behavior is unchanged. Tests should target **observable behavior** through the public API (`GetLetterGrade`, `IsPassing`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Reflection on `_bands` | Breaks on rename, reorder, or algorithm change |
| Maintainability | Tests private layout, not contract | Blocks refactors that improve code without changing behavior |
| Correctness | Band count ≠ correct mapping | Could pass with wrong thresholds inside five bands |

**Fix (priority order):**

1. Delete the reflection test; do not widen visibility (`internal` + `InternalsVisibleTo`) unless there is a strong seam reason.
2. Assert **outputs at boundaries** — e.g. 90→`"A"`, 89→`"B"`, 59→not passing, 60→passing — as in **GradeCalculator.cs** and manual tests in **Program.cs** Section 9.
3. For invalid input, keep `Assert.Throws<ArgumentOutOfRangeException>` on public methods (see **GradeCalculatorValidationTests**).
4. If logic grows complex, extract a testable public abstraction (e.g. `IGradeScale`) rather than testing privates.

**Production takeaway:** Karat distinguishes "tests broke" from "behavior broke" — implementation-coupled tests create merge friction without catching user-visible bugs. See Section 5: unit tests exercise one class through its public surface.

---

---

#### Q4. (R) A developer wants tests to "document business rules" and adds:

```csharp
private const int PassingThreshold = 60;

[Fact]
public void IsPassing_ReimplementsThresholdLogic()
{
    int score = 72;
    bool expected = score >= PassingThreshold;

    bool actual = new GradeCalculator().IsPassing(score);

    Assert.Equal(expected, actual);
}
```

Why does this test provide little value and risk false confidence? Show a better test for the same rule.

---

**Answer:**

```csharp
private const int PassingThreshold = 60;

[Fact]
public void IsPassing_ReimplementsThresholdLogic()
{
    int score = 72;
    bool expected = score >= PassingThreshold;

    bool actual = new GradeCalculator().IsPassing(score);

    Assert.Equal(expected, actual);
}
```

Why does this test provide little value and risk false confidence? Show a better test for the same rule.

**Answer:** The test **duplicates production logic** in the test body (`score >= PassingThreshold`), so both sides can be wrong the same way and the assertion still passes. It verifies the copy matches the copy, not that `GradeCalculator` matches the business rule.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Expected value computed with same rule as SUT | Bug in threshold (e.g. `>= 65`) may pass if test constant matches |
| Magic numbers | `72` is arbitrary mid-range | Does not pin the actual decision boundary at 60 |
| Documentation | Test name hides which rule is encoded | Reviewers cannot see boundary intent |

**Fix (priority order):**

1. Replace with **fixed expected outcomes** at meaningful inputs — not derived from the same formula.
2. Test **both sides of the boundary** (59 false, 60 true) as in this chapter's **GradeCalculatorTests.cs**.
3. If the threshold is a business constant, define it once in production code and test boundaries against known cases; do not recompute `expected` from that constant in the test unless testing a pure helper you own separately.

```csharp
[Fact]
public void IsPassing_Score59_ReturnsFalse()
{
    GradeCalculator calculator = new GradeCalculator();
    bool passing = calculator.IsPassing(59);
    Assert.False(passing);
}

[Fact]
public void IsPassing_Score60_ReturnsTrue()
{
    GradeCalculator calculator = new GradeCalculator();
    bool passing = calculator.IsPassing(60);
    Assert.True(passing);
}
```

**Production takeaway:** Tests should be independent oracles — Karat uses duplicated logic in tests to check whether you understand false confidence vs boundary specification.

---

---

#### Q5. (R) QA reports that a grading bug shipped despite green tests. The suite includes:

```csharp
[Fact]
public void GetLetterGrade_Score72_ReturnsC()
{
    Assert.Equal("C", new GradeCalculator().GetLetterGrade(72));
}

[Fact]
public void GetLetterGrade_Score95_ReturnsA()
{
    Assert.Equal("A", new GradeCalculator().GetLetterGrade(95));
}

[Fact]
public void GetLetterGrade_InvalidScore_ThrowsWithExactMessage()
{
    var ex = Assert.Throws<ArgumentOutOfRangeException>(
        () => new GradeCalculator().GetLetterGrade(101));

    Assert.Equal("Score must be between 0 and 100.", ex.Message);
}
```

Which tests are brittle, which gaps remain for `GetLetterGrade`, and what would you add or change?

---

**Answer:**

```csharp
[Fact]
public void GetLetterGrade_Score72_ReturnsC()
{
    Assert.Equal("C", new GradeCalculator().GetLetterGrade(72));
}

[Fact]
public void GetLetterGrade_Score95_ReturnsA()
{
    Assert.Equal("A", new GradeCalculator().GetLetterGrade(95));
}

[Fact]
public void GetLetterGrade_InvalidScore_ThrowsWithExactMessage()
{
    var ex = Assert.Throws<ArgumentOutOfRangeException>(
        () => new GradeCalculator().GetLetterGrade(101));

    Assert.Equal("Score must be between 0 and 100.", ex.Message);
}
```

Which tests are brittle, which gaps remain for `GetLetterGrade`, and what would you add or change?

**Answer:** The exception test is **brittle** against full `Message` strings; the happy-path tests use **unexplained magic numbers** and leave **boundary gaps** (e.g. 89 vs 90, 69 vs 70, 0, 100, negative scores) that would catch off-by-one threshold bugs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Brittleness | Exact `ex.Message` match | Fails on wording/localization/refactor though throw type and param still correct |
| Coverage gap | Only 72 and 95 tested | Wrong B/C or A/B cutoff ships undetected |
| Magic numbers | 72, 95 without comment | Readers cannot tell boundary vs mid-range intent |
| Naming | Inline one-liners skip AAA structure | Harder review for missing cases |

**Fix (priority order):**

1. Assert **exception type** and optionally `paramName` / inner details — not full message text unless message is a stable contract.

```csharp
var ex = Assert.Throws<ArgumentOutOfRangeException>(
    () => new GradeCalculator().GetLetterGrade(101));
Assert.Equal("score", ex.ParamName);
```

2. Add boundary pairs: 90/89 (`A`/`B`), 80/79, 70/69, 60/59 (`D`/`F`) — one act/assert each or `[Theory]` rows.
3. Add 0 and 100 edge cases; keep negative and >100 in **GradeCalculatorValidationTests** style.
4. Prefer names like `GetLetterGrade_Score90_ReturnsA`; use `[InlineData(90, "A")]` with column names in comments if using Theory later.

**Production takeaway:** Green tests with sparse boundaries and stringly exception asserts are a common post-release surprise — Karat tests prioritization of **behavior at edges** over happy-path literals. See **GradeCalculator.cs** thresholds and **Program.cs** Section 9 manual boundary cases.

---

---

#### Q6. (D) Two approaches are proposed for the failing grade at 59 / passing at 60 boundary used in this chapter's `Program.cs` and `GradeCalculatorTests.cs`:

**A — one test per boundary side (chapter style):**

```csharp
[Fact] public void IsPassing_Score59_ReturnsFalse() { … Assert.False(calc.IsPassing(59)); }
[Fact] public void IsPassing_Score60_ReturnsTrue()  { … Assert.True(calc.IsPassing(60)); }
```

**B — single parameterized test:**

```csharp
[Theory]
[InlineData(59, false)]
[InlineData(60, true)]
public void IsPassing_AtPassingThreshold_ReturnsExpected(int score, bool expected)
{
    Assert.Equal(expected, new GradeCalculator().IsPassing(score));
}
```

Which approach better supports maintainability and failure diagnosis in a large team, and what naming or data choices still matter?

---

### 02. xUnit

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/02. xUnit`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**A — one test per boundary side (chapter style):**

```csharp
[Fact] public void IsPassing_Score59_ReturnsFalse() { … Assert.False(calc.IsPassing(59)); }
[Fact] public void IsPassing_Score60_ReturnsTrue()  { … Assert.True(calc.IsPassing(60)); }
```

**B — single parameterized test:**

```csharp
[Theory]
[InlineData(59, false)]
[InlineData(60, true)]
public void IsPassing_AtPassingThreshold_ReturnsExpected(int score, bool expected)
{
    Assert.Equal(expected, new GradeCalculator().IsPassing(score));
}
```

Which approach better supports maintainability and failure diagnosis in a large team, and what naming or data choices still matter?

**Answer:** For two critical boundaries, **Approach A** (chapter style) gives the clearest failure names in CI and matches this folder's teaching convention; **Approach B** scales better when many `(score, expected)` rows exist, but only if each row represents an independent specification — not a duplicate of production logic.

**Approach A — prefer when:**

- Boundaries are few and business-critical (59/60 passing rule in **GradeCalculator.cs**).
- Test Explorer / `dotnet test` output should read like specs: `IsPassing_Score59_ReturnsFalse` fails without opening the method.
- New contributors learn AAA from explicit Arrange/Act/Assert blocks in **GradeCalculatorTests.cs**.

**Approach B — prefer when:**

- Dozens of grade rows would duplicate boilerplate; `[Theory]` + `[InlineData]` or `[MemberData]` reduces noise (full pattern in chapter **02. xUnit**).
- Data is clearly labeled — e.g. `[InlineData(59, false)] // one below passing threshold` — so magic numbers stay documented.
- Team agrees on one theory name: `IsPassing_AtPassingThreshold_ReturnsExpected` is acceptable; avoid vague `BoundaryTests`.

**Shared rules (both approaches):**

- Test **behavior** (`IsPassing` result), not how `score >= 60` is implemented internally.
- Always include **both sides** of the boundary; never only the passing case.
- One logical assert target per test method (A naturally; B requires each row to be a distinct case).

**Production takeaway:** The trade-off is **failure signal clarity** vs **DRY at scale** — Karat expects you to justify choice for team size and data volume, not declare one pattern universally superior. This chapter intentionally uses Approach A in **Program.cs** Section 9 and **GradeCalculatorTests.cs** before introducing `[Theory]` in the next chapter.

---

### 02. xUnit

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/02. xUnit`

---

---

#### Q1. (R) A teammate refactors `DiscountTheoryTests` into separate methods "for clarity." CI passes locally but a new `[Fact]` fails intermittently on the build agent. Review:

```csharp
public sealed class DiscountRefactorTests
{
    private readonly OrderPricingService _pricing = new();
    private decimal _lastDiscountedAmount;

    [Theory]
    [InlineData(100, 10, 90)]
    [InlineData(200, 25, 150)]
    public void ApplyDiscount_ReturnsExpected(
        decimal subtotal, decimal discountPercent, decimal expected)
    {
        _lastDiscountedAmount = _pricing.ApplyDiscount(subtotal, discountPercent);
        Assert.Equal(expected, _lastDiscountedAmount);
    }

    [Fact]
    public void ApplyDiscount_LastRun_Leaves150AfterBulkRow()
    {
        Assert.Equal(150m, _lastDiscountedAmount);
    }
}
```

What is wrong with mixing `[Fact]` and `[Theory]` here, and how should discount tables be tested instead?

---

**Answer:**

```csharp
public sealed class DiscountRefactorTests
{
    private readonly OrderPricingService _pricing = new();
    private decimal _lastDiscountedAmount;

    [Theory]
    [InlineData(100, 10, 90)]
    [InlineData(200, 25, 150)]
    public void ApplyDiscount_ReturnsExpected(
        decimal subtotal, decimal discountPercent, decimal expected)
    {
        _lastDiscountedAmount = _pricing.ApplyDiscount(subtotal, discountPercent);
        Assert.Equal(expected, _lastDiscountedAmount);
    }

    [Fact]
    public void ApplyDiscount_LastRun_Leaves150AfterBulkRow()
    {
        Assert.Equal(150m, _lastDiscountedAmount);
    }
}
```

What is wrong with mixing `[Fact]` and `[Theory]` here, and how should discount tables be tested instead?

**Answer:** The `[Fact]` reads instance field state left behind by whichever `[Theory]` row ran last in the same class — that is an order-dependent coupling, not an isolated scenario. `[Theory]` is for parameterized rows with self-contained assertions; `[Fact]` is for one fixed scenario with no parameters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Test design | `[Fact]` depends on side effect of `[Theory]` rows | Pass/fail varies with theory row order or whether the fact runs alone |
| Misuse of attributes | `[Theory]` used but follow-up scenario not expressed as another data row | Hidden coupling — looks like two tests, behaves like one ordered script |
| Isolation | Mutable `_lastDiscountedAmount` shared across methods in one class | Methods in a class run sequentially but still share instance state |

**Fix (priority order):**

1. Delete the dependent `[Fact]` — if "150 after bulk row" matters, add an explicit `[InlineData(200, 25, 150)]` row or a dedicated `[Fact]` that **arranges its own** inputs.
2. Keep each theory row fully self-contained: arrange, act, assert inside the method — no "last result" fields.
3. Use `[Fact]` only when a single AAA path is enough (see **FactTests.cs**); use `[Theory]` + `[InlineData]` / `[MemberData]` for tables (see **DiscountTheoryTests.cs**).
4. If two scenarios must share expensive setup, use a fixture for **read-only** shared data — not for passing values between tests.

**Production takeaway:** Karat uses `[Fact]` vs `[Theory]` to test whether you know parameterized tests are not a loop you can observe from the next test method. Discount tables belong in `[InlineData]`, not in cross-test field memory.

---

---

#### Q2. (R) A `[Theory]` backed by `[MemberData]` passes in Visual Studio's Test Explorer but fails unpredictably in `dotnet test` on CI. Review the data source and test:

```csharp
private static readonly List<OrderLine> SharedLines =
[
    new() { Sku = "A", Quantity = 2, UnitPrice = 25m },
    new() { Sku = "B", Quantity = 1, UnitPrice = 10m },
];

public static IEnumerable<object[]> SubtotalRows =>
    new List<object[]> { new object[] { SharedLines, 60m } };

[Theory]
[MemberData(nameof(SubtotalRows))]
public void CalculateSubtotal_MemberDataRow(
    IReadOnlyList<OrderLine> lines, decimal expected)
{
    lines.Add(new OrderLine { Sku = "EXTRA", Quantity = 1, UnitPrice = 99m });
    decimal subtotal = _pricing.CalculateSubtotal(lines);
    Assert.Equal(expected, subtotal);
}
```

Identify the misuse of `[Theory]` / `[MemberData]` and why the failure is order-dependent.

---

**Answer:**

```csharp
private static readonly List<OrderLine> SharedLines =
[
    new() { Sku = "A", Quantity = 2, UnitPrice = 25m },
    new() { Sku = "B", Quantity = 1, UnitPrice = 10m },
];

public static IEnumerable<object[]> SubtotalRows =>
    new List<object[]> { new object[] { SharedLines, 60m } };

[Theory]
[MemberData(nameof(SubtotalRows))]
public void CalculateSubtotal_MemberDataRow(
    IReadOnlyList<OrderLine> lines, decimal expected)
{
    lines.Add(new OrderLine { Sku = "EXTRA", Quantity = 1, UnitPrice = 99m });
    decimal subtotal = _pricing.CalculateSubtotal(lines);
    Assert.Equal(expected, subtotal);
}
```

Identify the misuse of `[Theory]` / `[MemberData]` and why the failure is order-dependent.

**Answer:** The theory mutates `SharedLines`, a static list reused by every row and potentially across test runs — the first execution grows the list, so later runs see the wrong line count and subtotal. `[MemberData]` should supply **fresh, immutable** inputs per row, not shared mutable collections the test body edits.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `lines.Add(...)` mutates shared static data | Expected 60m fails after list already contains `EXTRA` |
| Theory misuse | Single shared `List<OrderLine>` referenced from member data | Re-runs, parallel theories, or future extra rows all stomp the same list |
| Design | `IReadOnlyList<T>` parameter does not imply immutability | API suggests read-only; implementation is a mutable `List<T>` |

**Fix (priority order):**

1. Remove mutation from the test — pass lines that already include every SKU under test, or copy before act: `var copy = lines.ToList();`.
2. Return **new** lists per row from `MemberData` (pattern in **DiscountTheoryTests.cs** `SubtotalMemberData`).
3. Prefer immutable fixtures or `IReadOnlyList` backed by arrays/`List.AsReadOnly()` for shared catalogs.
4. Add a second theory row with empty lines to prove rows do not leak state into each other.

```csharp
public static IEnumerable<object[]> SubtotalRows =>
    new List<object[]>
    {
        new object[]
        {
            new List<OrderLine>
            {
                new() { Sku = "A", Quantity = 2, UnitPrice = 25m },
                new() { Sku = "B", Quantity = 1, UnitPrice = 10m },
            },
            60m,
        },
    };
```

**Production takeaway:** `[MemberData]` is for computed tables, not shared scratch buffers — treat each row like a pure function input. See **Program.cs** QUICK REFERENCE — `[MemberData]` for complex inputs.

---

---

#### Q3. (R) `OrderCatalogFixture` is extended for integration-style tests. Two methods in the same class pass locally but one fails when run with the full suite. Review:

```csharp
public sealed class OrderCatalogFixture
{
    public OrderPricingService Service { get; } = new();
    public Order StandardOrder { get; } = OrderCatalogFixture.CreateStandardOrder();
    public List<OrderLine> WorkingLines { get; } = new();
}

public sealed class MutableFixtureTests : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;

    public MutableFixtureTests(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void AddPromoLine_IncludesLineInWorkingSet()
    {
        _fixture.WorkingLines.Add(
            new OrderLine { Sku = "PROMO", Quantity = 1, UnitPrice = 5m });
        Assert.Single(_fixture.WorkingLines);
    }

    [Fact]
    public void WorkingLines_StartsEmptyEachTest()
    {
        Assert.Empty(_fixture.WorkingLines);
    }
}
```

What does `IClassFixture<T>` guarantee about instance lifetime, and why does the second test fail?

---

**Answer:**

```csharp
public sealed class OrderCatalogFixture
{
    public OrderPricingService Service { get; } = new();
    public Order StandardOrder { get; } = OrderCatalogFixture.CreateStandardOrder();
    public List<OrderLine> WorkingLines { get; } = new();
}

public sealed class MutableFixtureTests : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;

    public MutableFixtureTests(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void AddPromoLine_IncludesLineInWorkingSet()
    {
        _fixture.WorkingLines.Add(
            new OrderLine { Sku = "PROMO", Quantity = 1, UnitPrice = 5m });
        Assert.Single(_fixture.WorkingLines);
    }

    [Fact]
    public void WorkingLines_StartsEmptyEachTest()
    {
        Assert.Empty(_fixture.WorkingLines);
    }
}
```

What does `IClassFixture<T>` guarantee about instance lifetime, and why does the second test fail?

**Answer:** xUnit creates **one** `OrderCatalogFixture` per test class and injects the same instance into every test method constructor in that class — it does **not** reset mutable state between tests. After `AddPromoLine` runs, `WorkingLines` still holds the promo line when `WorkingLines_StartsEmptyEachTest` runs.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Fixture lifetime | `IClassFixture<T>` = one shared instance per class | Mutable lists on the fixture leak across tests |
| False isolation assumption | Treating fixture like per-test `[SetUp]` | Second test fails depending on method order within the class |
| Design | Expensive **read-only** seed mixed with per-test scratch state | Same object used for two incompatible purposes |

**Fix (priority order):**

1. Keep fixtures **immutable** after construction — match **OrderCatalogFixture.cs** (`Service`, `StandardOrder` only).
2. Put per-test mutable state in local variables or a fresh `List<OrderLine>` inside each test method.
3. If reset is unavoidable, implement `IDisposable` on the fixture and clear `WorkingLines` in `Dispose()` — xUnit calls it after all tests in the class finish (not between each test).
4. For per-test reset, use a constructor on the test class or `IAsyncLifetime` per test — not shared mutable fixture fields.

**Production takeaway:** `IClassFixture<T>` shares setup cost, not test isolation. ClassFixtureTests in this chapter shares a **read-only** catalog — Karat checks whether you would add mutable `WorkingLines` and expect a clean slate.

---

---

#### Q4. (R) Two test classes should share one catalog database seed via `ICollectionFixture<OrderCatalogFixture>`, but tests flake only on multi-core CI agents. Review:

```csharp
[CollectionDefinition("OrderCatalog")]
public sealed class OrderCatalogCollection : ICollectionFixture<OrderCatalogFixture> { }

[Collection("OrderCatalog")]
public sealed class CatalogTestsA : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;
    public CatalogTestsA(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void A_MarksCatalogLoaded() => _fixture.StandardOrder.OrderId = "LOADED";
}

// Missing [Collection("OrderCatalog")] — "just uses the fixture type"
public sealed class CatalogTestsB : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;
    public CatalogTestsB(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void B_ExpectsDefaultOrderId()
    {
        Assert.Equal("ORD-TEST-001", _fixture.StandardOrder.OrderId);
    }
}
```

What parallelization and fixture-lifetime mistakes stack here, and how do you fix them?

---

**Answer:**

```csharp
[CollectionDefinition("OrderCatalog")]
public sealed class OrderCatalogCollection : ICollectionFixture<OrderCatalogFixture> { }

[Collection("OrderCatalog")]
public sealed class CatalogTestsA : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;
    public CatalogTestsA(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void A_MarksCatalogLoaded() => _fixture.StandardOrder.OrderId = "LOADED";
}

// Missing [Collection("OrderCatalog")] — "just uses the fixture type"
public sealed class CatalogTestsB : IClassFixture<OrderCatalogFixture>
{
    private readonly OrderCatalogFixture _fixture;
    public CatalogTestsB(OrderCatalogFixture fixture) => _fixture = fixture;

    [Fact]
    public void B_ExpectsDefaultOrderId()
    {
        Assert.Equal("ORD-TEST-001", _fixture.StandardOrder.OrderId);
    }
}
```

What parallelization and fixture-lifetime mistakes stack here, and how do you fix them?

**Answer:** `CatalogTestsB` is **not** in the `"OrderCatalog"` collection, so xUnit gives it a **separate** `OrderCatalogFixture` and may run it **in parallel** with collection members — while `CatalogTestsA` mutates shared `StandardOrder`, creating cross-class interference and nondeterministic order IDs. Combining `[Collection]` with redundant `IClassFixture<T>` on the same class also confuses which fixture instance applies.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Collection membership | `CatalogTestsB` missing `[Collection("OrderCatalog")]` | Different fixture instance; parallel with collection tests |
| Parallelism | Non-collection classes run in parallel by default | **ParallelExecutionTests.cs** — unrelated classes use separate threads |
| Shared mutation | `StandardOrder.OrderId = "LOADED"` on collection-shared object | Other tests in the collection see mutated order |
| Fixture wiring | `IClassFixture<OrderCatalogFixture>` **and** `[Collection]` for same type | Extra instance per class unless only collection fixture is used |

**Fix (priority order):**

1. Add `[Collection("OrderCatalog")]` to **every** class that must share the seed and serialize together — see **CollectionFixtureTestsA.cs** / **CollectionFixtureTestsB.cs**.
2. Inject `OrderCatalogFixture` via collection fixture only; drop redundant `IClassFixture<OrderCatalogFixture>` on collection classes unless you intentionally want a second instance.
3. Stop mutating shared fixture objects — treat `StandardOrder` as read-only; clone before changes.
4. If global mutation is required, keep all writers/readers in one collection and document serial execution — do not rely on "we only have two classes."

**Production takeaway:** `[CollectionDefinition]` + `[Collection]` is xUnit's lock for shared expensive resources **and** parallel scheduling. Missing `[Collection]` is a classic CI-only flake when cores > 1. See **OrderCatalogCollection.cs** and **Program.cs** Section 7–8.

---

---

#### Q5. (M) A developer asks: "We have three test classes using `IClassFixture<OrderCatalogFixture>`. How many `OrderCatalogFixture` instances does xUnit create for one `dotnet test` run, and when does the fixture constructor run?" They also add `IDisposable` to the fixture to reset state. Explain xUnit's lifecycle for class vs collection fixtures and whether `IDisposable` on the fixture replaces proper test isolation.

---

**Answer:**

**Answer:** Three unrelated classes each implementing `IClassFixture<OrderCatalogFixture>` get **three separate instances** — one per class, constructed before the first test in that class runs and disposed (if `IDisposable`) after the last test in that class finishes. Classes in the same `[Collection("OrderCatalog")]` share **one** collection fixture instance for the whole collection, and tests in that collection do not run in parallel with each other.

- **`IClassFixture<T>`:** One `T` per test class; ctor runs once per class; same reference injected into every test method ctor on that class; tests in the class run **sequentially**.
- **`ICollectionFixture<T>`:** One `T` shared by all classes with the same `[Collection("Name")]`; ctor runs once for the collection; disposed after all tests in all member classes complete.
- **`IDisposable` on fixture:** Called after the **class** (class fixture) or **collection** (collection fixture) completes — **not** between individual tests. Clearing mutable state in `Dispose()` does not help test B if test A mutated the fixture mid-class.
- **Proper isolation:** Immutable fixture data, per-test local state, or `IAsyncLifetime.InitializeAsync` per test class instance — not "reset in Dispose only."
- **Contrast with this chapter:** `ClassFixtureTests` gets its own fixture; `CollectionFixtureTestsA/B` share one — compare subtotals and order IDs across files to prove instance boundaries.

**Production takeaway:** Know instance counts when diagnosing "why did my seed run twice" or "why do these classes see different catalogs." `IDisposable` is for teardown of external resources (files, connections), not a substitute for tests that do not mutate shared state.

---

---

#### Q6. (P) Your pipeline runs `dotnet test` on a 4-core Linux agent with no extra flags. Over time, unrelated test classes start failing together — one writes a temp file by fixed name, another reads a static `ConcurrentDictionary`, a third assumes an empty in-memory registry. Locally (often single-threaded or ReSharper's sequential runner) everything passes. What xUnit parallelism rules explain this, and what is your prioritized strategy for CI isolation without disabling all parallelism?

---

---

### 03. MSTest

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/03. MSTest`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** xUnit runs **different test classes in parallel** by default while methods inside the same class run sequentially — so static fields, fixed temp paths, and process-wide singletons race across classes on a 4-core agent. Locally, sequential IDE runners or fewer cores hide the overlap.

- **Default rules:** Different classes → parallel; same class → serial; same `[Collection]` → serial with shared collection fixture (**ParallelExecutionTests.cs**, **Program.cs** QUICK REFERENCE).
- **Why CI differs:** Agents use full CPU; `--parallel` / default xUnit worker count > 1; no guaranteed class order; reruns and TRX merge do not reset static process state.
- **Prioritized strategy:**
  1. **Fix tests first** — remove static mutable state, use `Path.GetTempFileName()` or `Guid` suffixes, inject fakes instead of static registries.
  2. **Group offenders** into a named `[Collection("SerialIntegration")]` so they share one synchronization boundary and optional shared fixture.
  3. **Run truly integration-heavy** projects in a separate CI job with `-- xUnit.parallelizeTestCollections=false` or `[assembly: CollectionBehavior(DisableTestParallelization = true)]` only for that assembly — not the whole solution.
  4. **Keep unit tests parallel** — stateless `[Fact]` / `[Theory]` tests on `OrderPricingService`-style pure logic scale well and match **FactTests.cs** / **DiscountTheoryTests.cs** patterns.
  5. **Detect flakes** — repeat `dotnet test --repeat 50` on PR for known parallel-sensitive assemblies; fail on nondeterminism before merge.
- **Avoid:** Global disable of parallelism for every test project because one legacy suite uses static state — that punishes fast unit tests and lengthens every pipeline.

**Production takeaway:** Test isolation in CI is an xUnit scheduling problem plus code hygiene. Prefer collection-scoped serialization over killing parallelism solution-wide — aligns with **Program.cs** Section 10 and production pipelines that rely on multi-core `dotnet test`.

---

---

### 03. MSTest

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/03. MSTest`

---

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

**Answer:**

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

**Answer:** MSTest runs every `[TestInitialize]` method before each test, but **does not guarantee order** between those methods — `SeedFromTierDiscount` can run before `CreateCalculator`, leaving `_calculator` null and causing `NullReferenceException` or wrong state depending on discovery order.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifecycle | Two `[TestInitialize]` methods with hidden dependency | Non-deterministic setup — pass/fail varies by runner ordering |
| Design | `_calculator` used before guaranteed initialization | Intermittent CI failures when test discovery order changes |
| Maintainability | Split setup across unordered hooks | Next refactor breaks tests silently |

**Fix (priority order):**

1. Merge setup into **one** `[TestInitialize]` with explicit top-to-bottom order — create SUT first, then seed derived state (matches `DiscountCalculatorCoreTests.cs` in this chapter).
2. Never assume one `[TestInitialize]` runs before another; if phases are required, call private helpers from a single hook in the order you need.
3. Keep each `[TestMethod]` independent — no reliance on another test having run first.
4. Run `dotnet test` on CI (not only Test Explorer) to catch ordering assumptions early.

```csharp
[TestInitialize]
public void SetUp()
{
    _calculator = new DiscountCalculator();
    _seededTotal = _calculator.ApplyTierDiscount(100.00m, CustomerTier.Silver);
}
```

**Production takeaway:** `[TestInitialize]` is "before each test," not "ordered pipeline stages" — Karat uses multi-hook classes to test whether you know MSTest lifecycle is per-test but unordered across hooks. See **Program.cs** Section 5 and `DiscountCalculatorCoreTests.cs`.

---

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

**Answer:**

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

**Answer:** `[DataRow]` attribute arguments must be compile-time constants of types MSTest supports — **`decimal` literals are not valid `[DataRow]` arguments**, so the build fails even though `decimal` works fine in normal C# code and in the SUT.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `[DataRow(100.00m, …)]` — decimal not accepted | CS1503 / attribute argument errors — tests never run |
| Domain | Money calculations need `decimal` in production code | Forcing `double` in rows risks precision loss if cast is forgotten |
| Pattern drift | Ignores chapter convention in `DiscountCalculatorDataTests.cs` | Team copies broken snippet into CI |

**Fix (priority order):**

1. Keep `[DataRow]` values as **`double` literals** (or other supported types like `int`, `string`, `enum`).
2. Cast to `decimal` **inside** the test body before calling `ApplyPercentDiscount` — exactly as this chapter does.
3. Assert with `decimal` via `Assert.AreEqual((decimal)expectedTotal, result)` for money.
4. For large or computed tables, use `[DynamicData]` from a static method returning `IEnumerable<object[]>` when `[DataRow]` constants are too limiting.

```csharp
[DataTestMethod]
[DataRow(100.00, 0.10, 90.00)]
public void ApplyPercentDiscount_ReturnsExpectedTotal(
    double orderTotal, double discountRate, double expectedTotal)
{
    decimal result = new DiscountCalculator().ApplyPercentDiscount(
        (decimal)orderTotal, (decimal)discountRate);
    Assert.AreEqual((decimal)expectedTotal, result);
}
```

**Production takeaway:** Parameterized MSTest tests carry a compile-time type contract separate from your domain model — always cast at the boundary. See **Program.cs** Section 6 pitfall and `DiscountCalculatorDataTests.cs`.

---

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

**Answer:**

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
    { /* ... */ }
}
```

What breaks at compile time or discovery time, and why is `[DataTestMethod]` required here?

**Answer:** `[TestMethod]` methods **must have no parameters** — a parameterized signature with `[DataRow]` requires `[DataTestMethod]`, otherwise the compiler rejects the method or the adapter will not expand rows into separate test cases.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile / discovery | `[TestMethod]` + parameters + `[DataRow]` mismatch | Build error or single undiscoverable method — not three row cases |
| Coverage illusion | One green test in Explorer | Two discount scenarios never executed — regression slips to prod |
| Merge hygiene | Attribute regression during conflict resolution | Silent loss of data-driven coverage |

**Fix (priority order):**

1. Replace `[TestMethod]` with **`[DataTestMethod]`** on any method that takes `[DataRow]` arguments.
2. Verify Test Explorer / `dotnet test` lists **one result per row** (e.g., `…(100,0.1,90)`, `…(50,0.2,40)`).
3. Add CI coverage gates or `--filter` spot checks on critical `[DataTestMethod]` classes after merges.
4. Prefer explicit naming so row failures identify inputs in the log.

**Production takeaway:** `[DataRow]` without `[DataTestMethod]` is a common merge accident — Karat checks whether you know MSTest distinguishes single-case `[TestMethod]` from parameterized `[DataTestMethod]`. See **Program.cs** Sections 3 and 6.

---

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

**Answer:**

```csharp
[TestClass]
public class TierRateTableTests
{
    private static Dictionary<CustomerTier, decimal> _tierRateTable = null!;
    // ... ClassInitialize loads dictionary ...
    // OverrideGoldRate_ForPromoScenario mutates _tierRateTable[Gold] = 0.25m
    // GoldRate_MatchesClassInitializeDefault expects 0.15m
}
```

What does `[ClassInitialize]` actually share across tests, and why is the second test flaky?

**Answer:** `[ClassInitialize]` runs **once per `[TestClass]`** and the static `_tierRateTable` is **shared by every test in that class** — mutating it in one `[TestMethod]` leaks state into others, and parallel runners make execution order non-deterministic so the "default rate" test flakes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable static dictionary written in one test, read in another | Order-dependent pass/fail — classic flake on parallel CI |
| Lifecycle misuse | `[ClassInitialize]` treated as "read-only reference data" but not enforced | Promo override permanently changes shared table for the class run |
| Parallelism | MSTest can run tests concurrently within an assembly (V3+ / `[Parallelize]`) | Shared mutable statics race across threads |

**Fix (priority order):**

1. Treat `[ClassInitialize]` data as **immutable** — `IReadOnlyDictionary<CustomerTier, decimal>` or copy-on-read, never mutate in tests (pattern in `DiscountCalculatorLifecycleTests.cs`).
2. If a test needs a modified table, **clone** the dictionary in `[TestInitialize]` or build local expected values without touching static state.
3. Reset statics in `[ClassCleanup]` only for teardown of external resources — not as a substitute for per-test isolation.
4. For expensive shared **services**, prefer instance fields set in `[TestInitialize]` with a fresh SUT; reserve class-level hooks for truly read-only, expensive setup.

```csharp
[ClassInitialize]
public static void InitClass(TestContext context)
{
    _tierRateTable = new ReadOnlyDictionary<CustomerTier, decimal>(
        new Dictionary<CustomerTier, decimal> { /* rates */ });
}

// In test: decimal expectedRate = _tierRateTable[tier]; — read only
```

**Production takeaway:** `[ClassInitialize]` is one-time shared setup, not a per-test reset — same judgment as xUnit `IClassFixture` mutation traps (Chapter 02). See **Program.cs** Section 8 and `DiscountCalculatorLifecycleTests.cs`.

---

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

**Answer:**

```csharp
[TestMethod]
public void ApplyPercentDiscount_TenPercentOnHundred_ReturnsNinetyAsFloat()
{
    float discounted = (float)calculator.ApplyPercentDiscount(100.0m, 0.10m);
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

**Answer:** Binary floating-point (`float`/`double`) cannot represent many decimal fractions exactly — exact `Assert.AreEqual` without tolerance fails even when the math is "correct" at business precision (e.g., `0.1 + 0.2 != 0.3`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Exact equality on `float`/`double` | False failures — tests red while production code is fine |
| Domain | Money path cast to `float` for assertion | Loses decimal precision the SUT was designed for |
| API misuse | Two-parameter `AreEqual` on floats | Ignores MSTest's delta overload meant for this scenario |

**Fix (priority order):**

1. For **money**, stay on `decimal` end-to-end — `Assert.AreEqual(90.00m, result)` as in `DiscountCalculatorAssertTests.cs`.
2. When asserting `float`/`double` is unavoidable, use the **delta overload**: `Assert.AreEqual(90.0f, discounted, 0.001f)` or appropriate epsilon.
3. Delete or rewrite "sanity" tests that cast pricing results to `float` — they test the wrong type.
4. Document team standard: financial asserts use `decimal`; floating-point asserts always specify tolerance.

```csharp
Assert.AreEqual(0.3, sum, 1e-10);           // double with delta
Assert.AreEqual(90.0f, discounted, 0.01f);  // float with delta — prefer decimal for money
```

**Production takeaway:** `Assert.AreEqual` has value-type and reference-type paths — for `double`/`float` the three-argument delta form is the production-safe pattern. See **Program.cs** Section 7.

---

---

#### Q6. (D) Your team starts a greenfield .NET 8 microservice. `dotnet new xunit` is the SDK default; Visual Studio still offers an MSTest template. Half the org maintains legacy MSTest suites from .NET Framework migrations. For **new** unit-test projects on this service, which framework would you standardize on and why? Cover discovery, lifecycle hooks, parallelism defaults, and when staying on MSTest is the pragmatic choice.

---

---

### 04. Mocking & Test Doubles

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/04. Mocking & Test Doubles`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** For most greenfield .NET 8 services, **standardize on xUnit** — SDK templates, community samples, and OSS libraries target it first — while **keeping MSTest** where enterprise templates, existing suites, or Visual Studio-centric workflows dominate.

- **Greenfield default → xUnit:** `dotnet new xunit` is zero-friction; `[Fact]` / `[Theory]` + `[InlineData]` map cleanly from MSTest once attribute names are known (see **Program.cs** Section 9 table). Constructor + `IDisposable` replace `[TestInitialize]` / `[TestCleanup]` with explicit C# lifecycle.
- **Discovery & CLI:** Both run under `Microsoft.NET.Test.Sdk` and `dotnet test`; filter syntax differs slightly but CI integration is equivalent. Pick one framework per solution/service to avoid duplicate adapter packages and mixed conventions.
- **Parallelism:** xUnit parallelizes by default across collections — good for speed, requires immutable fixtures (Chapter 02). MSTest historically ran more sequentially per assembly; V3+ improves parallelism but teams must opt in with `[Parallelize]` and avoid mutable statics from `[ClassInitialize]`.
- **Lifecycle mapping:** `[ClassInitialize]` / `[ClassCleanup]` ↔ `IClassFixture<T>` / `IDisposable`; `[AssemblyInitialize]` ↔ custom assembly fixtures or `AssemblyFixture` patterns — xUnit's fixture model is more composable for large suites.
- **When MSTest is pragmatic:** Large legacy MSTest bank being extended incrementally; org mandate / Azure DevOps templates; mixed-skill teams already trained on `[TestClass]` / Test Explorer groupings; internal libraries with MSTest examples only.
- **Migration strategy:** New microservice → xUnit; shared test utilities abstract SUT setup so framework attributes stay thin; do not run MSTest and xUnit test projects with conflicting static state in one CI job without understanding parallel scope.

**Production takeaway:** Framework choice is a team maintainability decision, not a capability gap — both run on `dotnet test`. Prefer one convention per repo boundary; know the attribute map in **Program.cs** QUICK REFERENCE when reading either style in code review.

---

---

### 04. Mocking & Test Doubles

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/09. Unit Testing/04. Mocking & Test Doubles`

---

---

#### Q1. (D) Your team tests `OrderService.PlaceOrder` three different ways — `StubInventoryService`, `FakeEmailSender`, and `Mock<IEmailSender>` with `Verify`. For a new test that asserts "insufficient stock cancels the order and no email is sent," which double type do you pick for **inventory** and for **email**, and why? When would you swap the email side from fake to mock?

---

**Answer:**

**Answer:** Use a **stub** (or minimal mock `Setup`) for inventory to force `HasStock` to return `false`, and a **fake** for email so you assert `SentMessages` is empty — the same pattern as `PlaceOrder_WhenStubReportsLowStock_FailsWithoutEmailOrReservation` in **OrderServiceManualDoubleTests.cs**. Swap email to a mock when you only need a spy (`Verify(..., Times.Never)`) and do not care about inspecting message contents — as in **OrderServiceMoqTests.cs** failure tests.

- **Inventory → stub:** The scenario needs a canned `false` from `HasStock("WIDGET-01", 5)` when stock is 1. A stub dictionary or one `Setup().Returns(false)` is enough; no need to verify `Reserve` was never called if stock state stays unchanged (stub) or you add a single `Verify(..., Times.Never)` on reserve.
- **Email → fake (default here):** `FakeEmailSender.SentMessages` gives a readable behavior assertion (`Assert.Empty`) without Moq ceremony. The chapter's **Program.cs** Section 5 demo uses this path.
- **Email → mock:** Prefer `Mock<IEmailSender>` + `Verify(..., Times.Never)` when the test file is already Moq-centric, when email is not the focus and you want consistent mock-only Arrange blocks, or when multiple void collaborators each need `Times.Never` in one Assert section.
- **Avoid mock for inventory in this scenario** unless you also need to prove `Reserve` was never invoked — the stub's unchanged dictionary already proves no reservation side effect.
- **Production takeaway:** Pick doubles by **what you need to observe** — return values (stub), accumulated state (fake), or interaction counts (mock/spy). See **Program.cs** Section 2 taxonomy table.

---

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

**Answer:**

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

**Answer:**

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

---

#### Q4. (P) Production `OrderService` will call a warehouse REST API through `HttpClient`. You must unit-test `WarehouseInventoryService : IInventoryService` without network I/O. Sketch the test seam — how do you substitute HTTP responses, and why is `new HttpClient()` inside the service a blocker?

---

**Answer:**

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

---

#### Q5. (D) A pricing microservice client is injected into checkout. Your lead asks whether to test it with (A) a custom `HttpMessageHandler` stub in a unit test, (B) `WebApplicationFactory` hitting an in-memory test server, or (C) a contract test against a deployed sandbox. Map each option to a layer of the **test pyramid** for this repo's `OrderService` chapter. When is each the right default?

---

**Answer:**

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

**Answer:**

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

**Answer:**

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

---
