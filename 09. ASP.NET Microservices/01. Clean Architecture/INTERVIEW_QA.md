# Clean Architecture — Interview Q&A
> 30 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Clean Architecture, and what problem does it solve compared to a traditional layered (N-Tier) architecture?](#q1-what-is-clean-architecture-and-what-problem-does-it-solve-compared-to-a-traditional-layered-n-tier-architecture)
2. [Q2. What is the Dependency Rule in Clean Architecture, and why is it the single most important constraint?](#q2-what-is-the-dependency-rule-in-clean-architecture-and-why-is-it-the-single-most-important-constraint)
3. [Q3. How does Clean Architecture relate to Hexagonal Architecture (Ports & Adapters) and Onion Architecture? Are they different names for the same thing?](#q3-how-does-clean-architecture-relate-to-hexagonal-architecture-ports-adapters-and-onion-architecture-are-they-different-names-for-the-same-thing)
4. [Q4. What are the four concentric layers in Clean Architecture, and what is the primary responsibility of each?](#q4-what-are-the-four-concentric-layers-in-clean-architecture-and-what-is-the-primary-responsibility-of-each)
5. [Q5. Why does Clean Architecture make applications more testable, and which layer benefits the most?](#q5-why-does-clean-architecture-make-applications-more-testable-and-which-layer-benefits-the-most)
6. [Q6. What belongs in the Domain layer, and what must never appear there?](#q6-what-belongs-in-the-domain-layer-and-what-must-never-appear-there)
7. [Q7. What is a Domain Entity in Clean Architecture, and how is it different from a database entity or ORM model?](#q7-what-is-a-domain-entity-in-clean-architecture-and-how-is-it-different-from-a-database-entity-or-orm-model)
8. [Q8. What is a Value Object, and when should you model something as a Value Object rather than an Entity?](#q8-what-is-a-value-object-and-when-should-you-model-something-as-a-value-object-rather-than-an-entity)
9. [Q9. What are Domain Events, and why are they raised in the Domain layer rather than in the Application or Infrastructure layer?](#q9-what-are-domain-events-and-why-are-they-raised-in-the-domain-layer-rather-than-in-the-application-or-infrastructure-layer)
10. [Q10. What is an Aggregate and an Aggregate Root? What rule governs how external code interacts with an Aggregate?](#q10-what-is-an-aggregate-and-an-aggregate-root-what-rule-governs-how-external-code-interacts-with-an-aggregate)
11. [Q11. What is the difference between a Domain Service and an Application Service?](#q11-what-is-the-difference-between-a-domain-service-and-an-application-service)
12. [Q12. What belongs in the Application layer, and what dependencies is it allowed to take?](#q12-what-belongs-in-the-application-layer-and-what-dependencies-is-it-allowed-to-take)
13. [Q13. What is a Use Case (Interactor) in Clean Architecture, and what is its single responsibility?](#q13-what-is-a-use-case-interactor-in-clean-architecture-and-what-is-its-single-responsibility)
14. [Q14. What is the role of interfaces defined in the Application layer (such as repository interfaces and external service contracts)?](#q14-what-is-the-role-of-interfaces-defined-in-the-application-layer-such-as-repository-interfaces-and-external-service-contracts)
15. [Q15. How does CQRS (Command Query Responsibility Segregation) fit into the Application layer of Clean Architecture?](#q15-how-does-cqrs-command-query-responsibility-segregation-fit-into-the-application-layer-of-clean-architecture)
16. [Q16. What is the Mediator pattern, and why is MediatR commonly paired with Clean Architecture in .NET?](#q16-what-is-the-mediator-pattern-and-why-is-mediatr-commonly-paired-with-clean-architecture-in-net)
17. [Q17. What is an Application Service, and how does it differ from a Domain Service?](#q17-what-is-an-application-service-and-how-does-it-differ-from-a-domain-service)
18. [Q18. What belongs in the Infrastructure layer, and why does it sit at the outermost ring?](#q18-what-belongs-in-the-infrastructure-layer-and-why-does-it-sit-at-the-outermost-ring)
19. [Q19. What is the Repository pattern, and how does it enforce the Dependency Rule in Clean Architecture?](#q19-what-is-the-repository-pattern-and-how-does-it-enforce-the-dependency-rule-in-clean-architecture)
20. [Q20. How do you implement and register an Infrastructure-layer repository so that the Application layer stays unaware of EF Core?](#q20-how-do-you-implement-and-register-an-infrastructure-layer-repository-so-that-the-application-layer-stays-unaware-of-ef-core)
21. [Q21. What is the difference between the Repository pattern and the Unit of Work pattern, and how do they relate to each other?](#q21-what-is-the-difference-between-the-repository-pattern-and-the-unit-of-work-pattern-and-how-do-they-relate-to-each-other)
22. [Q22. What is the role of the Presentation layer in Clean Architecture, and what framework code is allowed there?](#q22-what-is-the-role-of-the-presentation-layer-in-clean-architecture-and-what-framework-code-is-allowed-there)
23. [Q23. What are DTOs (Data Transfer Objects) and Mappers, and in which layer do they belong?](#q23-what-are-dtos-data-transfer-objects-and-mappers-and-in-which-layer-do-they-belong)
24. [Q24. What is the difference between a Domain Model and a ViewModel or DTO? Why must they not be the same class?](#q24-what-is-the-difference-between-a-domain-model-and-a-viewmodel-or-dto-why-must-they-not-be-the-same-class)
25. [Q25. How do you structure a Clean Architecture solution in .NET — which projects do you create, and what are the allowed project references?](#q25-how-do-you-structure-a-clean-architecture-solution-in-net-which-projects-do-you-create-and-what-are-the-allowed-project-references)
26. [Q26. How do you wire up dependency injection (DI) in a Clean Architecture .NET solution without leaking infrastructure details into the outer layers?](#q26-how-do-you-wire-up-dependency-injection-di-in-a-clean-architecture-net-solution-without-leaking-infrastructure-details-into-the-outer-layers)
27. [Q27. What is the role of the `DependencyInjection.cs` (or `ServiceCollectionExtensions`) class that each layer typically exposes?](#q27-what-is-the-role-of-the-dependencyinjectioncs-or-servicecollectionextensions-class-that-each-layer-typically-exposes)
28. [Q28. What are the main drawbacks or costs of adopting Clean Architecture for a small or medium project?](#q28-what-are-the-main-drawbacks-or-costs-of-adopting-clean-architecture-for-a-small-or-medium-project)
29. [Q29. What is "Anemic Domain Model" anti-pattern, and how does it indicate that Clean Architecture is not being applied correctly in the Domain layer?](#q29-what-is-anemic-domain-model-anti-pattern-and-how-does-it-indicate-that-clean-architecture-is-not-being-applied-correctly-in-the-domain-layer)
30. [Q30. When should you NOT use Clean Architecture, and what simpler alternatives exist for small services?](#q30-when-should-you-not-use-clean-architecture-and-what-simpler-alternatives-exist-for-small-services)

---

## Q1. What is Clean Architecture, and what problem does it solve compared to a traditional layered (N-Tier) architecture?

**Concepts**
- Framework and infrastructure coupling in N-Tier architecture
- Concentric layer model with inward-only dependency direction
- Dependency inversion between business rules and infrastructure
- In-memory testability of Domain and Application layers
- Database and framework independence of the business core

**Answer**

Clean Architecture, introduced by Robert C. Martin, organizes code into concentric layers where business rules live at the innermost ring and all external concerns — databases, frameworks, UIs — live at the outside edge. The central problem it solves is framework coupling: in a traditional N-Tier architecture each layer depends on the one below it, which means the business logic layer often knows about `SqlConnection`, `DbContext`, or specific ORM entities — details that have nothing to do with business rules. Clean Architecture inverts this so the Domain and Application layers define interfaces and outer layers implement them, meaning the business logic has zero knowledge of EF Core, SQL Server, or ASP.NET Core. Since the Domain and Application layers depend only on abstractions, the entire business core can be tested in memory with no database, no HTTP stack, and no third-party framework — only plain C# classes. Traditional N-Tier still works well for simple CRUD applications, but it becomes painful when you need to swap databases, add a new UI such as a CLI alongside an API, or write unit tests without spinning up infrastructure.

---

## Q2. What is the Dependency Rule in Clean Architecture, and why is it the single most important constraint?

**Concepts**
- Inward-only compile-time dependency direction
- Interface-based inversion between Application and Infrastructure
- .NET project reference graph as enforcement mechanism
- Business logic isolation from framework details
- Untestability as the cost of a violated Dependency Rule

**Answer**

The Dependency Rule states that source code dependencies can only point inward — toward higher-level policy — and never outward toward lower-level details. This means nothing in the Domain layer can reference anything in Application, Infrastructure, or Presentation, and nothing in Application can reference Infrastructure or Presentation directly. A dependency here means any compile-time reference: a `using` directive, an inheritance relationship, a constructor parameter type, or a generic type argument — if inner code names an outer type, the rule is violated. The rule is enforced by having inner layers define abstractions and outer layers provide implementations, so the Application layer declares `IOrderRepository` while the Infrastructure layer implements `SqlOrderRepository`, but the Application project never references the Infrastructure project. Breaking the Dependency Rule typically means business logic now couples to a framework detail, which makes it untestable in isolation and forces you to rebuild or redeploy more than necessary when infrastructure changes. In .NET terms the rule maps directly to `.csproj` project references: `Domain` has no project references, `Application` references only `Domain`, `Infrastructure` references `Application` and `Domain`, and `Presentation` references `Application` plus optionally `Infrastructure` for DI wiring only.

---

## Q3. How does Clean Architecture relate to Hexagonal Architecture (Ports & Adapters) and Onion Architecture? Are they different names for the same thing?

**Concepts**
- Shared core isolation principle across all three architectures
- Hexagonal Architecture ports and adapters metaphor
- Onion Architecture explicit layer naming convention
- Clean Architecture distinct Use Case layer
- Convergent project structure across all three approaches

**Answer**

Clean Architecture, Hexagonal Architecture (by Alistair Cockburn), and Onion Architecture (by Jeffrey Palermo) all express the same core idea — isolate business logic from infrastructure by inverting dependencies — but they use different metaphors and place slightly different emphasis on naming and structure. They are not identical, but they are highly compatible and can be seen as variations on the same principle.

Hexagonal Architecture focuses on the "port" metaphor: a port is an interface the application defines, and an adapter is anything that connects to that port — a REST controller, a message queue consumer, or a test double. Onion Architecture explicitly names Domain Model, Domain Services, Application Services, and Infrastructure, which is a closer match to how most .NET developers structure projects. Clean Architecture synthesizes both and adds a distinct "Use Case" (Interactor) layer separate from the Domain, making application-specific orchestration logic explicit. In practice, a .NET project described as Clean Architecture will almost certainly also satisfy the principles of Hexagonal and Onion — they all result in the same project structure.

| Concept | Hexagonal (Ports & Adapters) | Onion Architecture | Clean Architecture |
|---|---|---|---|
| Core abstraction | Application core surrounded by ports | Innermost domain, outer rings | Innermost entities, concentric rings |
| Terminology for boundaries | Ports (interfaces) and Adapters (implementations) | Layers (Domain, Application, Infrastructure) | Layers (Entities, Use Cases, Interface Adapters, Frameworks) |
| Entry points | Driving adapters (HTTP, CLI, tests) | Application Services | Controllers, Presenters |
| Dependency direction | Outside → core | Outer rings → inner rings | Outer rings → inner rings |

---

## Q4. What are the four concentric layers in Clean Architecture, and what is the primary responsibility of each?

**Concepts**
- Entities (Domain) layer as the home of enterprise-wide rules
- Use Cases (Application) layer for application-specific orchestration
- Interface Adapters layer for format translation
- Frameworks and Drivers as the outermost ring
- Inward-only dependency across all four rings

**Answer**

Clean Architecture defines four rings from innermost to outermost: Entities (Domain), Use Cases (Application), Interface Adapters, and Frameworks and Drivers. Each ring may only depend on rings inside it, never on rings outside.

The Entities layer contains enterprise-wide business rules — Domain Entities, Aggregates, Value Objects, and Domain Events — and changes only when fundamental business rules change, not when a database is swapped or a UI is redesigned. The Use Cases layer contains application-specific business rules as Use Case classes (also called Interactors or Application Services) that orchestrate Domain objects to fulfill one specific task; this layer also defines interfaces for any external dependency it needs. The Interface Adapters layer — Infrastructure plus Presentation — converts data between the formats Use Cases expect and the formats required by external systems, so controllers transform HTTP requests into Use Case input models and repository implementations translate Domain objects to database rows. The outermost Frameworks and Drivers ring contains the actual tools — ASP.NET Core, Entity Framework Core, SQL Server, Redis, Serilog — which are plugged into the Interface Adapters ring so the business logic never touches them directly.

---

## Q5. Why does Clean Architecture make applications more testable, and which layer benefits the most?

**Concepts**
- Abstraction-only dependencies in Domain and Application layers
- Test double substitution for infrastructure interfaces
- Application layer as the primary unit test target
- Infrastructure confinement of untestable code
- N-Tier coupling as the source of hard-to-test business logic

**Answer**

Clean Architecture makes applications more testable because the business logic — Domain and Application layers — depends only on abstractions, never on concrete infrastructure like databases, HTTP clients, or file systems. Any external dependency is represented by an interface that can be replaced with a test double without changing any production code, which means the Application layer benefits the most: Use Case classes receive repository interfaces and external service interfaces via constructor injection, so a unit test can pass in an `InMemoryOrderRepository` or a mock without running any SQL or making any HTTP calls. The Domain layer is even simpler to test since Domain Entities and Value Objects are pure C# classes with no interfaces or dependencies at all — a test constructs them directly and exercises their methods. Infrastructure code such as EF Core repositories and HTTP clients requires integration tests because it inherently talks to external systems, but Clean Architecture confines this untestable code to the outermost ring, minimizing how much of the codebase requires a running database. By contrast, in a tightly coupled N-Tier application where business logic calls `new SqlConnection(...)` directly, any unit test must either mock `SqlConnection` (fragile) or spin up a real database (slow), making the test suite expensive and brittle.

---

## Chapter 02. Domain Layer

---

## Q6. What belongs in the Domain layer, and what must never appear there?

**Concepts**
- Domain Entities, Aggregates, Value Objects, and Domain Events
- Repository and service interfaces expressing domain needs
- Third-party framework reference prohibition
- Data annotation prohibition on domain classes
- Infrastructure-independence as the domain completeness test

**Answer**

The Domain layer contains everything that expresses the core business rules — Entities, Aggregates, Value Objects, Domain Events, Domain Services, and repository or service interfaces that express what the domain needs rather than how it is fulfilled. Nothing that depends on any external system, framework, or infrastructure detail is allowed here, because adding such a reference forces every unit test of business logic to depend on it as well. Data annotations from `System.ComponentModel.DataAnnotations` such as `[Required]` or `[MaxLength]` are also forbidden on Domain Entities since they are UI and validation framework concerns, not domain rules — invariants should be enforced in the entity's constructor or factory methods instead. A simple completeness test: if you could take the Domain project, drop it into a completely different application type — CLI, different database, different web framework — and it still compiles and makes sense, the domain is clean.

---

## Q7. What is a Domain Entity in Clean Architecture, and how is it different from a database entity or ORM model?

**Concepts**
- Identity-based equality versus attribute-based equality
- Invariant enforcement in Domain Entity methods
- ORM model as a schema-driven property bag
- Separate Domain Entity and EF Core persistence model classes
- EF Core Fluent API mapping isolated to the Infrastructure layer

**Answer**

A Domain Entity is a class that represents a business concept with a unique, persistent identity and encapsulates the business rules and invariants governing its state. A database entity or ORM model is a class whose sole purpose is to map rows from a database table to C# properties — it carries no business logic and its shape is driven by the schema, not by business meaning. Domain Entities enforce their own invariants, so an `Order` entity might refuse to add a line item if the order is already shipped, throwing a domain exception rather than allowing the caller to create an invalid state. Domain Entities are identified by identity (an `OrderId` value), which means two `Order` objects with the same identity represent the same order even if some properties differ. ORM models are often decorated with framework-specific attributes such as `[Table]` and `[Column]` that couple them to EF Core, whereas Domain Entities should have none of these — EF Core's Fluent API in the Infrastructure layer handles the mapping separately. In practice you often maintain two separate classes: a Domain Entity used throughout the Application and Domain layers, and an EF Core entity (sometimes called a "persistence model") that the Repository in the Infrastructure layer maps to and from.

---

## Q8. What is a Value Object, and when should you model something as a Value Object rather than an Entity?

**Concepts**
- Structural equality determined by attribute values
- Immutability as the defining property of Value Objects
- C# record type as the natural implementation
- Lifecycle-free concepts as natural Value Object candidates
- Unnecessary identity tracking as an Entity anti-pattern

**Answer**

A Value Object is an immutable object whose identity is determined entirely by the values of its attributes, not by a unique identifier, so two Value Objects with the same attribute values are equal and interchangeable. I model something as a Value Object when it has no meaningful standalone lifecycle and no need for unique identity across the system — examples include `Money(amount: 100, currency: "USD")`, `Address`, `DateRange`, `EmailAddress`, and `Coordinates`, since these concepts are fully described by their data. Value Objects are typically `record` types in modern C#, which automatically implement structural equality, so two `Money` records are equal if `Amount` and `Currency` match. Because Value Objects are immutable they are inherently thread-safe and free of the lifecycle management complexities that come with mutable Entities — when a modified version is needed, a new instance is created rather than mutating the existing one. A common mistake is modeling `Address` as an Entity by giving it an `AddressId` when the business has no need to track addresses as independent objects with their own lifecycle, since they only ever exist as part of a `Customer` or `Order`.

---

## Q9. What are Domain Events, and why are they raised in the Domain layer rather than in the Application or Infrastructure layer?

**Concepts**
- Domain Event as an immutable record of a business fact
- Aggregate Root as the collector of Domain Events
- Post-transaction dispatch by the Application layer
- Downstream effect decoupling via event handlers
- DomainEntity base class event collection pattern

**Answer**

Domain Events are lightweight immutable objects that record that something meaningful happened within the Domain — for example, `OrderPlacedEvent`, `PaymentFailedEvent`, or `InventoryReservedEvent`. They are raised inside Domain Entities or Aggregates at the moment the business action completes because the domain is the authoritative source of business fact, which means raising events there ensures the event is tightly coupled to the business rule that caused it rather than to an infrastructure concern or application orchestration step. If `Order.Place()` raises `OrderPlacedEvent`, you cannot accidentally forget to raise it from a different code path. The Application layer dispatches the collected Domain Events after the Use Case completes and the transaction commits, typically through a mediator or an event dispatcher — the Domain itself never dispatches, it only collects events. A typical pattern is to have Domain Entities inherit from a base class `DomainEntity` that exposes `AddDomainEvent(IDomainEvent)` and `GetDomainEvents()`, and after the Unit of Work saves changes the Application layer reads these events and publishes them to handlers. Domain Events decouple downstream effects — sending a confirmation email, updating a read model, triggering a saga — from the core business rule, so each handler is independently testable and can be added without modifying the domain entity.

---

## Q10. What is an Aggregate and an Aggregate Root? What rule governs how external code interacts with an Aggregate?

**Concepts**
- Aggregate as a consistency unit of Entities and Value Objects
- Aggregate Root as the sole external access point
- Repository-only persistence of Aggregate Roots
- Aggregate boundary as the transaction boundary
- Small aggregate size as a concurrency discipline

**Answer**

An Aggregate is a cluster of related Domain Entities and Value Objects treated as a single unit of consistency — all changes inside the cluster must satisfy the business invariants of the whole group, and they are persisted together in one transaction. The Aggregate Root is the single Entity within the cluster that external code is allowed to reference; all access to other entities within the aggregate must go through the root, so for an `Order` aggregate containing `OrderLine` entities, only `Order` exposes methods like `AddLine()` and `RemoveLastLine()` — an `OrderLine` cannot be retrieved and mutated directly from outside. Repositories only load and save Aggregate Roots, which means you never have an `IOrderLineRepository`, only an `IOrderRepository` that loads the whole `Order` with its lines. Aggregates define the transaction boundary so everything inside one aggregate is saved in one atomic operation, and if two aggregates need to change together they each change independently and are coordinated with Domain Events for eventual consistency rather than a cross-aggregate transaction. Keeping aggregates small is a key design discipline, since a large aggregate that contains dozens of child entities creates contention — every operation on any part of the aggregate locks the whole thing.

---

## Q11. What is the difference between a Domain Service and an Application Service?

**Concepts**
- Domain Service as multi-object business logic in the Domain layer
- Application Service as workflow orchestration in the Application layer
- Domain Service reusability across multiple use cases
- Infrastructure isolation in Domain Services
- Leaked domain logic in Application Services as a code smell

**Answer**

A Domain Service contains business logic that is part of the domain model but does not naturally belong to any single Entity or Value Object — it operates on multiple domain objects and expresses a business rule that spans them. An Application Service (Use Case) is not business logic itself; it orchestrates the flow of a use case by calling repositories, domain objects, and domain services in the right order to fulfill one application scenario without containing any business decisions of its own. A Domain Service lives in the Domain layer, has no dependency on infrastructure, and expresses a rule like "transfer funds between two accounts" — an operation that needs both the source and destination `BankAccount` entities but belongs to neither exclusively. An Application Service lives in the Application layer and handles concerns like loading the account from the repository, calling the domain service, raising events, saving changes, and returning a result DTO. The distinction matters because domain rules buried inside Application Services cannot be reused and are harder to test in isolation — if a pricing rule exists in the Application layer, two different use cases that price orders would duplicate it.

| Aspect | Domain Service | Application Service |
|---|---|---|
| Layer | Domain | Application |
| Contains | Business logic spanning multiple domain objects | Orchestration and workflow |
| Knowledge of infrastructure | None | Via interfaces only |
| Example | `TransferService.Transfer(from, to, amount)` | `TransferFundsUseCase.ExecuteAsync(command)` |

---

## Chapter 03. Application Layer

---

## Q12. What belongs in the Application layer, and what dependencies is it allowed to take?

**Concepts**
- Use Cases and Command/Query objects as core Application content
- Application-defined interfaces for infrastructure needs
- Domain-only project reference for the Application layer
- ASP.NET Core type prohibition in the Application layer
- Compilability without Infrastructure as the completeness test

**Answer**

The Application layer contains the application-specific business rules expressed as Use Cases (Interactors or Application Services), plus the interfaces for all external capabilities the application needs. It is allowed to depend only on the Domain layer — it can reference Domain types directly — and it must express all infrastructure needs as interfaces defined within itself, because referencing any concrete infrastructure class would violate the Dependency Rule. Use Case classes, Command and Query objects for CQRS, interface definitions for repositories and external services such as `IOrderRepository`, `IEmailSender`, and `IPaymentGateway`, input/output DTO models, and validators for those DTOs all belong here. No reference to EF Core, `HttpClient`, SQL, messaging brokers, or any concrete infrastructure class belongs here. ASP.NET Core types like `HttpContext`, `IActionResult`, or routing attributes must also stay out since the Application layer should be equally callable from an HTTP endpoint, a CLI, or a message queue consumer. A good test: if you delete the Infrastructure and Presentation projects, the Application layer should still compile because all its external dependencies are represented only by C# interfaces it defines itself.

---

## Q13. What is a Use Case (Interactor) in Clean Architecture, and what is its single responsibility?

**Concepts**
- Single application operation per Use Case class
- Strongly typed Command or Query input object
- Strongly typed output DTO
- Domain object encapsulation from callers
- Use Case as the primary unit test target

**Answer**

A Use Case, also called an Interactor, is a class in the Application layer that encapsulates exactly one application-specific operation — for example, "Place an Order", "Cancel a Shipment", or "Generate Monthly Invoice". Its single responsibility is to orchestrate domain objects and infrastructure interfaces to carry out that one operation and produce one result, meaning it takes a strongly typed input model (a Command or Query object), performs its work by calling repositories and domain logic, and returns a strongly typed output model (a result DTO or simple success/failure). The Use Case never exposes raw domain objects to callers, so the Application layer controls what details cross its boundary. Without this layer, presentation code like controllers would orchestrate domain logic directly, which quickly becomes untestable and tangled with HTTP concerns. A Use Case is the ideal unit test target: inject mocked repositories and service interfaces, call the Use Case, and assert on the result — no database, no HTTP, no file system required. Each Use Case should remain small and focused; if one is growing to handle five different scenarios with complex branching, it is likely doing too much and should be split.

---

## Q14. What is the role of interfaces defined in the Application layer (such as repository interfaces and external service contracts)?

**Concepts**
- Dependency Inversion contracts for infrastructure needs
- Infrastructure project dependency on Application interfaces
- Application compilation without Infrastructure
- In-memory test implementations via interface substitution
- Storage technology replacement via new interface implementation

**Answer**

Interfaces defined in the Application layer — such as `IOrderRepository`, `IEmailSender`, or `IPaymentGateway` — are the Dependency Inversion contracts that let the Application layer declare what it needs without naming who provides it. By defining these interfaces in the Application layer, the dependency direction is inverted: the Infrastructure layer depends on the Application layer to implement the interfaces, not the other way around, which is the central mechanism that makes the Dependency Rule possible. Infrastructure implementations like `SqlOrderRepository : IOrderRepository` live in the Infrastructure project, which references the Application project, so the Application project never references the Infrastructure project and can be compiled and tested without EF Core. When a new storage technology is needed — switching from SQL Server to MongoDB — you create a new implementation of `IOrderRepository` in Infrastructure and swap it in the DI registration, and the Application layer changes nothing. This pattern also makes it easy to write fast in-memory implementations of repository interfaces for unit and integration tests, eliminating the need for a running database in most of the test suite.

---

## Q15. How does CQRS (Command Query Responsibility Segregation) fit into the Application layer of Clean Architecture?

**Concepts**
- Command and Query objects as Application layer Use Case inputs
- Command handler as a write-side Use Case
- Query handler bypassing the Domain layer for read optimization
- Read model versus full domain model for queries
- Complementary roles of CQRS structure and Clean Architecture layers

**Answer**

CQRS separates operations that change state (Commands) from operations that read state (Queries), and it fits naturally into the Application layer because each Command or Query becomes a Use Case input object and each handler becomes a Use Case class — the boundary between Presentation and Application flows through these typed objects. A Command such as `PlaceOrderCommand` carries all the data needed to execute a state-changing operation; its handler validates, applies domain logic, and persists the result, returning either nothing or a minimal result like the new entity's ID. A Query such as `GetOrderSummaryQuery` carries filter parameters and its handler returns a read-optimized DTO directly from the database — it may bypass the Domain layer entirely and read denormalized data straight from a view or projection table. Clean Architecture provides the layer structure and dependency rules while CQRS provides the message-based communication style between Presentation and Application layers, so each use case remains isolated and independently testable. The Command/Query split also enables separate optimization: the write side uses rich Domain Entities with invariant enforcement, while the read side uses lightweight flat query models optimized for display without dragging the full domain model through every read operation.

---

## Q16. What is the Mediator pattern, and why is MediatR commonly paired with Clean Architecture in .NET?

**Concepts**
- Mediator as a central message dispatcher decoupling sender from handler
- MediatR as the dominant .NET Mediator library
- IPipelineBehavior for cross-cutting concerns
- One-file-per-use-case structure
- Runtime handler resolution through DI

**Answer**

The Mediator pattern routes messages — Commands, Queries, Notifications — from a sender to a registered handler through a central broker, so the sender and handler are not directly coupled to each other. MediatR is the dominant .NET library implementing this pattern, and it is commonly paired with Clean Architecture because it provides a convenient, low-ceremony way to dispatch Commands and Queries from controllers to Application-layer handlers without controllers needing to inject specific handler types. With MediatR, a controller injects `IMediator`, calls `await _mediator.Send(new PlaceOrderCommand(...))`, and the mediator resolves the matching `IRequestHandler<PlaceOrderCommand, OrderId>` from the DI container and invokes it — the controller never references the handler class directly. MediatR also supports `IPipelineBehavior<TRequest, TResponse>` — a pipeline similar to ASP.NET Core middleware but for the Application layer — so cross-cutting concerns like validation, logging, caching, and transaction management can be implemented as behaviors and applied to all commands without modifying any handler. The combination of MediatR, Clean Architecture, and CQRS results in individual handler files that are small, focused, and easy to locate, meaning adding a new feature means adding new Command/Query/Handler files without touching existing code. MediatR is not required by Clean Architecture — the same pattern can be implemented with explicit interfaces — but it eliminates a lot of boilerplate.

---

## Q17. What is an Application Service, and how does it differ from a Domain Service?

**Concepts**
- Application Service as a use-case orchestrator
- Domain Service as cross-object business logic in the Domain layer
- Infrastructure knowledge as the key distinction
- Fat Application Service as a code smell
- Separate unit test strategies for Domain versus Application Services

**Answer**

An Application Service coordinates the steps of one application scenario — it loads entities from repositories, calls domain methods or domain services, handles transactional boundaries, and maps results to output DTOs. A Domain Service encapsulates a pure business rule that spans multiple domain objects, lives in the Domain layer, and knows nothing about repositories, transactions, or DTOs. Application Services are orchestrators: they know the sequence of steps needed to fulfill a use case, but the actual business decisions happen in Domain Entities and Domain Services — an Application Service that contains `if/else` logic deciding how to price an order is a symptom of leaked domain logic. Domain Services are called by Application Services and are purely about business logic, so `TransferFundsService.Transfer(source, destination, amount)` applies the business rules of a transfer and raises a `FundsTransferredEvent`, but it does not know how to load accounts from a database — that is the Application Service's job. Both are injectable via DI, but their test strategies differ: Domain Services are tested with pure unit tests since they take domain objects as parameters and need no mocks, while Application Services are tested with mocked repositories and service interfaces injected into their constructors.

---

## Chapter 04. Infrastructure Layer

---

## Q18. What belongs in the Infrastructure layer, and why does it sit at the outermost ring?

**Concepts**
- Concrete implementations of Application and Domain interfaces
- Infrastructure volatility as the reason for the outermost position
- Business logic prohibition in Infrastructure
- Infrastructure DI extension methods as the composition point
- Full layer swappability without touching Application or Domain

**Answer**

The Infrastructure layer contains concrete implementations of all interfaces defined in the Application and Domain layers — database repositories, external API clients, message queue publishers, email senders, file system adapters, and caching providers. It sits at the outermost ring because it is the most volatile part of the system: frameworks, databases, and external services change more often than business rules, and the architecture ensures those changes stay contained here. EF Core `DbContext`, repository implementations like `SqlOrderRepository`, migrations, HTTP clients wrapped in typed service implementations, Serilog sinks configuration, email client wrappers, cloud storage adapters, and background job registrations all belong here. Infrastructure code must not contain business logic — if an Infrastructure class starts making business decisions, those decisions need to move to the Application or Domain layer, since Infrastructure code should be a thin adapter that translates external formats to domain formats and vice versa. Because Infrastructure depends on Application and Domain rather than the reverse, you can swap EF Core for Dapper or SQL Server for MongoDB by rewriting only the Infrastructure project without changing Application or Domain. Infrastructure also includes the DI registration code — `IServiceCollection` extension methods that wire up concrete implementations to their interfaces — which is the one place where the concrete types are named explicitly.

---

## Q19. What is the Repository pattern, and how does it enforce the Dependency Rule in Clean Architecture?

**Concepts**
- Collection-like abstraction over Aggregate Root persistence
- Interface in Application layer, implementation in Infrastructure
- Inward-pointing dependency arrow via the Repository interface
- Domain Entity return rather than ORM model
- Test double substitution at runtime via DI

**Answer**

The Repository pattern provides an abstraction over the data access layer that exposes a collection-like interface for retrieving and persisting domain objects, hiding all storage implementation details from the Application and Domain layers. It enforces the Dependency Rule by placing the interface in the Application (or Domain) layer while placing the implementation in the Infrastructure layer, so the dependency arrow points inward rather than outward. The Application layer defines `IOrderRepository` with methods like `GetByIdAsync(OrderId id)`, `AddAsync(Order order)`, and `SaveChangesAsync()` — the Application layer knows about this interface but not about EF Core, SQL, or any database. The Infrastructure layer provides `SqlOrderRepository : IOrderRepository`, which internally uses `OrderDbContext` (EF Core), and this project depends on the Application project to implement its interface, satisfying the Dependency Rule. At runtime the DI container resolves `IOrderRepository` to `SqlOrderRepository`; at test time it resolves to `InMemoryOrderRepository` or a Moq mock, and the Application layer's Use Cases are unaffected. Repositories should return Domain Entities rather than ORM models, so the repository implementation is responsible for the mapping between EF Core's persistence model and the Domain Entity, keeping that translation concern inside Infrastructure.

---

## Q20. How do you implement and register an Infrastructure-layer repository so that the Application layer stays unaware of EF Core?

**Concepts**
- Infrastructure-to-Application project reference direction
- Domain Entity to EF Core entity mapping inside the repository
- AddInfrastructure IServiceCollection extension method
- EF Core type name confinement to Infrastructure
- Single-line initialization in Program.cs

**Answer**

I implement the repository in the Infrastructure project by creating a class that implements the Application-layer interface using EF Core internally, then register that implementation via an extension method on `IServiceCollection` that lives in the Infrastructure project. The Infrastructure project references both the Application project for the interface and the EF Core NuGet packages — the Application project references neither the Infrastructure project nor EF Core. The repository implementation maps between EF Core entity classes and Domain Entity classes, typically using AutoMapper, a hand-written static mapper, or a dedicated mapping service, since Domain Entities are what the Use Cases receive while EF Core entities are what the `DbContext` manages. The `AddInfrastructure(this IServiceCollection services, IConfiguration config)` extension method registers `services.AddScoped<IOrderRepository, SqlOrderRepository>()`, `services.AddDbContext<AppDbContext>(...)`, and any other infrastructure services. The API project calls `builder.Services.AddInfrastructure(builder.Configuration)` — one line, with no EF Core names visible in the API project — which means a developer working on a Use Case never needs to know whether the repository uses EF Core, Dapper, or an in-memory store.

---

## Q21. What is the difference between the Repository pattern and the Unit of Work pattern, and how do they relate to each other?

**Concepts**
- Repository managing single Aggregate load and save
- Unit of Work tracking cross-repository changes as one transaction
- EF Core DbContext as a built-in Unit of Work implementation
- Deferred commit for cross-repository consistency
- IUnitOfWork interface exposed by the Application layer

**Answer**

The Repository pattern abstracts access to a single type of Aggregate — it manages loading and saving individual aggregates in isolation. The Unit of Work pattern tracks all changes made during a business operation across multiple repositories and commits or rolls them back as a single atomic transaction. They are complementary: repositories manage what objects to load and save, and the Unit of Work manages when to commit everything together. A repository without a Unit of Work commits each save immediately and independently, which means two repositories saving two aggregates could have one succeed and the other fail, leaving data inconsistent. The Unit of Work defers the actual database write until all domain changes are complete, then commits everything in one transaction — EF Core's `DbContext` is itself an implementation of the Unit of Work pattern, since `SaveChangesAsync()` commits all tracked changes atomically. In Clean Architecture, the Application layer interface often exposes `IUnitOfWork` (or `SaveChangesAsync` on a shared interface) separately from the repository interfaces; repository calls are made during the Use Case, and a final `await _unitOfWork.CommitAsync()` persists all changes at once. A common practical choice in .NET is to have the EF Core `DbContext` serve as both the repository via `DbSet<T>` and the Unit of Work via `SaveChangesAsync`, with thin repository wrapper classes delegating to it — clean separation at the interface level, shared implementation in practice.

---

## Chapter 05. Presentation Layer & Entry Points

---

## Q22. What is the role of the Presentation layer in Clean Architecture, and what framework code is allowed there?

**Concepts**
- Presentation layer as the entry point and external format translator
- Thin controllers dispatching Commands and Queries
- Multiple Presentation layers over a shared Application layer
- HTTP-specific concerns confined to Presentation
- DI reference to Infrastructure only in the host project

**Answer**

The Presentation layer is the entry point of the application — it receives external input such as HTTP requests, CLI arguments, or message queue events, translates that input into Application-layer Commands or Queries, dispatches them, and translates the result back into an external format like a JSON response or acknowledgement. All framework-specific code — ASP.NET Core, controllers, SignalR hubs, Minimal API route registrations — belongs here. The Presentation layer references the Application layer to dispatch Commands/Queries and, in most .NET projects, references the Infrastructure layer only in the host entry-point project for DI wiring, not in controllers themselves. Controllers and Minimal API endpoints should be thin: parse and validate the incoming request into a Command/Query object, call `mediator.Send()`, and map the result to an HTTP response — business logic in controllers is a design smell. Multiple Presentation layers can sit on top of the same Application layer simultaneously, so an ASP.NET Core Web API, a Blazor Server frontend, and a background Worker Service can all send Commands to the same Application layer handlers from different Presentation adapters. HTTP-specific concerns — route definitions, `[Authorize]` attributes, response caching headers, content negotiation, `ProblemDetails` error mapping — all live here and must not appear in Application or Domain.

---

## Q23. What are DTOs (Data Transfer Objects) and Mappers, and in which layer do they belong?

**Concepts**
- DTO as a behavior-free property bag for cross-layer data transfer
- Application-layer DTOs at the Use Case boundary
- Presentation-layer response models shaped for the consumer
- Mapper placement matching the translating layer
- Direct Domain Entity HTTP serialization as an anti-pattern

**Answer**

Data Transfer Objects (DTOs) are simple property-bag classes that carry data across layer or process boundaries without any business logic. Mappers are the code that converts between a Domain Entity and a DTO, and both belong at the boundary where the translation occurs — DTOs used at the Application boundary belong in the Application layer, and DTOs used in HTTP responses (ViewModels) belong in the Presentation layer. Application-layer DTOs are the input and output models of Use Cases: `PlaceOrderCommand` carries the data needed to place an order while `OrderSummaryDto` carries the data the Use Case returns, and these are distinct from the Domain Entity and from the HTTP response model. Presentation-layer response models are shaped for the consumer — a mobile API response may include computed fields or exclude internal IDs — so the Presentation layer maps from the Application output DTO to the response model. Mappers such as AutoMapper profiles, Mapster configurations, or hand-written extension methods belong in the layer that performs the mapping: Application-layer mappers live in Application, Presentation mappers live in Presentation, and Infrastructure mappers that translate between Domain Entities and EF Core entities live in Infrastructure. Using Domain Entities directly as HTTP response bodies is a common anti-pattern since it couples the HTTP contract to the internal domain model shape, meaning any domain refactoring changes the public API.

---

## Q24. What is the difference between a Domain Model and a ViewModel or DTO? Why must they not be the same class?

**Concepts**
- Domain Model as a behavior-carrying invariant-enforcing object
- ViewModel or DTO as a display-shaped property bag
- Diverging change axes for domain rules versus UI requirements
- Sensitive field exposure risk from direct entity serialization
- Deserialization-friendly DTO versus invariant-enforcing constructor

**Answer**

A Domain Model is a rich object that encapsulates business rules, enforces invariants, and represents a concept in the problem domain. A ViewModel or DTO is a flat, dumb property-bag class shaped for a specific consumer — a UI screen or an API endpoint — with no behavior and no invariants. They must not be the same class because their lifecycles, shapes, and concerns are governed by completely different forces: Domain Models change when business rules change, while ViewModels change when UI requirements change such as adding a new form field or combining a computed property. Domain Models may contain sensitive fields — internal audit data, cost calculations — that should never be exposed in an API response, so returning a Domain Entity directly risks accidentally serializing fields that should be private. Domain Models also enforce invariants via constructors and methods where the constructor may throw if data is invalid, whereas DTOs need to be freely constructable for deserialization with public setters and parameterless constructors because JSON deserializers create them from incoming data before validation is applied. A practical mapping chain: HTTP request JSON is deserialized to a Command/DTO, the Use Case loads the Domain Entity and applies changes, saves, maps the Domain Entity to an output DTO, and the DTO is serialized to the JSON response — each object in the chain has the right shape and concerns for its step.

---

## Chapter 06. Project Structure & .NET Implementation

---

## Q25. How do you structure a Clean Architecture solution in .NET — which projects do you create, and what are the allowed project references?

**Concepts**
- Four-project solution matching the four layers
- Domain class library with no project references
- Application class library referencing only Domain
- Infrastructure class library referencing Application and Domain
- API project as the composition root referencing all layers

**Answer**

A standard Clean Architecture .NET solution contains four projects corresponding to the four layers: `Domain` (class library), `Application` (class library), `Infrastructure` (class library), and `Presentation` or `API` (ASP.NET Core project). The solution host — typically the `API` project — is the only project that references all others, and the project reference graph is strictly unidirectional inward.

`YourApp.Domain` is a class library with no project references, containing Entities, Value Objects, Aggregates, Domain Events, and base classes. `YourApp.Application` references only `Domain` and contains Use Cases, Command/Query/Handler classes, application-level interfaces, validators, and output DTOs. `YourApp.Infrastructure` references `Application` and `Domain` and contains the EF Core `DbContext`, repository implementations, external service adapters, DI extension methods, and migrations. `YourApp.API` references `Application`, `Infrastructure` for DI wiring, and optionally `Domain` for error types, containing Controllers or Minimal API endpoints, middleware, filters, and `Program.cs`.

```
Solution
├── Domain         (no references)
├── Application    (→ Domain)
├── Infrastructure (→ Application, Domain)
└── API            (→ Application, Infrastructure, Domain)
```

A test project `YourApp.Tests` references only `Application` and `Domain` for unit tests, plus `Infrastructure` for integration tests.

---

## Q26. How do you wire up dependency injection (DI) in a Clean Architecture .NET solution without leaking infrastructure details into the outer layers?

**Concepts**
- Per-layer IServiceCollection extension methods
- AddApplication and AddInfrastructure as the naming convention
- Infrastructure type name confinement inside AddInfrastructure
- Program.cs as the single composition call site
- Full infrastructure swappability via one method change

**Answer**

Each layer exposes one static extension method on `IServiceCollection` — typically named `AddDomain`, `AddApplication`, and `AddInfrastructure` — that registers only that layer's services. The host project's `Program.cs` calls these methods in sequence, remaining unaware of which concrete classes each layer registers internally. `AddApplication(this IServiceCollection services)` in the Application layer registers MediatR, FluentValidation, pipeline behaviors, and any application-layer services, with no infrastructure types visible there. `AddInfrastructure(this IServiceCollection services, IConfiguration config)` in the Infrastructure layer registers `DbContext`, repository implementations, `HttpClient` factories, caching, and any other external adapters — this is the only place EF Core type names appear explicitly. `Program.cs` in the API project calls `builder.Services.AddApplication().AddInfrastructure(builder.Configuration)` and needs no knowledge of `SqlOrderRepository` or `AppDbContext`, since those details are hidden behind the extension method. This approach means you can swap an entire infrastructure layer implementation by changing only the `AddInfrastructure` method body and the classes it registers, with nothing in `Program.cs`, Application, or Domain changing.

---

## Q27. What is the role of the `DependencyInjection.cs` (or `ServiceCollectionExtensions`) class that each layer typically exposes?

**Concepts**
- Single-call layer initialization API for the host project
- Application DI registering MediatR and pipeline behaviors
- Infrastructure DI registering DbContext and repository implementations
- Facade pattern applied to DI wiring complexity
- Community convention from Clean Architecture templates

**Answer**

The `DependencyInjection.cs` file in each layer is a static class containing a single public extension method on `IServiceCollection` that encapsulates all DI registrations for that layer. Its role is to provide a clean, single-call API for the host project to initialize each layer without the host needing to know the names or lifetimes of any individual service within the layer. The Application layer's `DependencyInjection.cs` calls `services.AddMediatR(...)`, `services.AddValidatorsFromAssembly(...)`, `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))`, and similar registrations — all internal Application concerns. The Infrastructure layer's `DependencyInjection.cs` calls `services.AddDbContext<AppDbContext>(...)`, `services.AddScoped<IOrderRepository, SqlOrderRepository>()`, `services.AddHttpClient<IPaymentGateway, StripePaymentGateway>()`, and so on. The naming convention is popularized by templates like `dotnet new cleanarch` and Jason Taylor's Clean Architecture template rather than being a framework requirement — any static class with an extension method works. This pattern is an application of the Facade design pattern: it presents a simplified interface to the complexity of the layer's internal wiring, preventing the host from becoming a long list of individual `services.AddScoped<I..., Impl...>()` calls.

---

## Chapter 07. Trade-offs & Common Pitfalls

---

## Q28. What are the main drawbacks or costs of adopting Clean Architecture for a small or medium project?

**Concepts**
- Structural overhead of multiple projects, interfaces, and mappers
- Increased ceremony for simple CRUD features
- Multi-step indirection cost in debugging
- Mapping overhead between domain, application, and presentation models
- Payoff conditions that justify the overhead

**Answer**

Clean Architecture adds structural overhead — more projects, more interfaces, more mapping code, and more indirection — that pays off in large, long-lived systems but can be wasteful for small CRUD applications or microservices with a limited, well-defined scope. Even a simple "create customer" use case requires a Command class, a Handler class, a repository interface, a repository implementation, a mapping step, and a response DTO — whereas the same feature in a Minimal API with EF Core could be three lines of code. Translating between Domain Entities, Application DTOs, Infrastructure persistence models, and Presentation ViewModels creates multiple mapping layers that must be maintained and kept in sync as models evolve. Debugging requires tracing through controller → mediator → handler → repository interface → repository implementation → EF Core instead of a direct call chain, which is harder to follow for developers not familiar with the pattern. Clean Architecture is worth the overhead when the codebase will be maintained for years, the team is large enough that independent testability of layers matters, the domain is complex enough to benefit from a rich domain model, or the storage or presentation technology is expected to change.

---

## Q29. What is "Anemic Domain Model" anti-pattern, and how does it indicate that Clean Architecture is not being applied correctly in the Domain layer?

**Concepts**
- Anemic Domain Model as a property-bag entity with no behavior
- Business logic in Application Services instead of Domain Entities
- ORM-first design as a common cause of the anti-pattern
- Private setters and meaningful methods as the fix
- EF Core Fluent API with private backing fields

**Answer**

An Anemic Domain Model is a domain layer in which Entity classes contain only properties (getters and setters) and no business logic — all the logic lives in Application Services or Domain Services instead. It indicates that Clean Architecture is not working correctly because the domain layer, which should be the home of business rules, has been hollowed out into a data-transfer container that provides none of the intended benefits of encapsulation and invariant enforcement. In a healthy domain model, an `Order` entity has methods like `Place()`, `Cancel()`, and `AddLine(product, quantity, price)` that enforce business rules internally, so callers cannot put an `Order` in an invalid state. In an anemic domain model, `Order` has public setters on every property and an `OrderService` in the Application layer manually coordinates field assignments — any caller can set `Status = "Shipped"` without checking whether the order was ever placed. The anemic pattern often emerges when developers treat domain classes as ORM entities and let the ORM drive the design, since EF Core requires public setters (or constructors) which tempts developers to make all fields publicly settable. The fix is to make setters private, remove parameterless constructors from domain entities, and add meaningful business methods, then use EF Core's Fluent API with `HasField` and value converters to map private backing fields so the ORM and the domain model each keep their integrity.

---

## Q30. When should you NOT use Clean Architecture, and what simpler alternatives exist for small services?

**Concepts**
- Clean Architecture overhead versus domain complexity mismatch
- Vertical Slice Architecture as a feature-organized alternative
- Minimal API with flat structure for simple data services
- Modular Monolith with per-module Clean Architecture
- Architecture complexity proportional to domain complexity

**Answer**

I should not use Clean Architecture when the service is small, the domain is simple, the team is small, and the expected lifespan is short — the structural overhead will slow the team down without delivering meaningful benefits. For CRUD microservices with minimal business logic, Vertical Slice Architecture or a simple Feature Folder approach inside a single project often delivers better developer velocity and clarity. Vertical Slice Architecture, popularized by Jimmy Bogard, organizes code by feature rather than by layer — each feature folder contains its own handler, DTO, validator, and data access code in one place with no Domain or Application layer abstraction — which scales well for services with many small, independent features and little cross-feature domain logic. A Minimal API with EF Core and a flat structure is appropriate for simple data services: one `Program.cs`, a few route handlers, a `DbContext`, and FluentValidation, with no interfaces, no mediators, and no mapping. A Modular Monolith with Clean Architecture per module can be a middle ground: each bounded context gets its own Domain, Application, and Infrastructure, but they all run in one process and share a single API host, avoiding the distributed systems overhead of microservices while still providing Clean Architecture's testability within each module. The decision rule: if a Use Case in the Application layer contains only one repository call and no domain logic, Clean Architecture is adding ceremony without value for that feature — the architecture complexity should match the domain complexity.

---

## Gotchas — Clean Architecture (Interview Traps)

---

#### Gotcha 1. Domain Layer Depending on Infrastructure

**Concepts**
- Dependency Rule violated when Domain references outer layers
- EF Core DbContext or NuGet packages leaked into Domain project
- Circular dependency breaking the compile-time enforcement
- Domain project's `.csproj` as the audit target

**Answer**

The most common and most damaging Clean Architecture mistake is adding an EF Core, Newtonsoft.Json, or any infrastructure NuGet package to the Domain project. The Domain layer must have zero dependencies on frameworks or persistence libraries — once `Microsoft.EntityFrameworkCore` appears in `Domain.csproj`, the Dependency Rule is broken and every stated benefit of Clean Architecture (framework independence, in-memory testability) disappears. The fix is to check the Domain project has no project references and no NuGet packages beyond base class library helpers, and to move any ORM-related attributes or interfaces to the Infrastructure project.

---

#### Gotcha 2. Anemic Domain Model — All Logic Lives in Application Services

**Concepts**
- Entity as a property bag with only getters and setters
- Business rules scattered across Application Service methods
- ORM-first design as the root cause
- Private setters and behavioral methods as the cure

**Answer**

An anemic domain model means Entity classes contain only public properties with no business methods, so all logic ends up in Application Services — this is a procedural design wearing Clean Architecture clothing. Interviewers look for whether you recognise that a `Customer` entity should have methods like `Activate()`, `ChangeEmail(string newEmail)`, and `Deactivate()` with invariant guards inside them, not a `CustomerService` that directly sets `customer.IsActive = true` from outside. The usual cause is starting from an EF Core entity and leaving setters public for the ORM, then never adding behavior — the fix is private setters with EF Core's Fluent API `HasField` configuration and explicit domain methods that enforce all business rules before mutating state.

---

#### Gotcha 3. Over-Engineering a Simple CRUD Service

**Concepts**
- Complexity cost versus domain complexity mismatch
- Ceremony without benefit in simple services
- Vertical Slice Architecture as the alternative
- One-line handlers as the smell indicator

**Answer**

Applying Clean Architecture to a microservice that is essentially a thin CRUD wrapper over a database table produces four projects, five mapping steps, and dozens of interfaces for what amounts to three lines of EF Core code — the overhead is real and the benefit is zero. The signal that the pattern is being misapplied is when every command handler looks like `await _repo.Save(entity); return Unit.Value;` with no domain logic whatsoever. Interviewers expect you to know when to reach for Vertical Slice Architecture, Minimal API with EF Core, or even a simple CRUD controller — architecture complexity must be proportional to domain complexity.

---

#### Gotcha 4. Repository Interface Defined in the Infrastructure Layer

**Concepts**
- Interface ownership belonging to the consumer, not the implementer
- Infrastructure project referencing Application instead of the reverse
- Dependency inversion versus dependency injection confusion
- Application layer defining the contract it needs

**Answer**

A common mistake is defining `IOrderRepository` inside the Infrastructure project alongside its `SqlOrderRepository` implementation, then referencing Infrastructure from Application to use the interface — this inverts the intended direction and reintroduces the coupling Clean Architecture is designed to remove. The Dependency Rule requires that the interface belongs to the Application layer (the layer that depends on the contract), not the Infrastructure layer (the implementer). The Infrastructure project references Application to implement Application's interfaces; Application references nothing in Infrastructure. Placing the interface in Infrastructure and having Application reference Infrastructure for it defeats the pattern entirely.

---

#### Gotcha 5. Returning `IQueryable<T>` from a Repository

**Concepts**
- IQueryable leaking EF Core execution context outside the repository
- LINQ expressions evaluated in the wrong layer
- Test doubles unable to reproduce EF Core query translation
- Materialised collections as the correct return type

**Answer**

Returning `IQueryable<T>` from a repository interface leaks the EF Core execution context into the Application layer — any LINQ expression chained by the caller is translated and executed against the database, which means the Application layer is silently coupled to the EF Core query provider. When you mock the repository in a unit test with `IEnumerable<T>`, LINQ-to-Objects behaves differently from LINQ-to-SQL, so tests pass but production queries fail or vice versa. The contract of the repository interface should return fully materialised collections or single objects so the query is completely executed within the Infrastructure layer and the Application layer sees only plain in-memory objects.

---

#### Gotcha 6. Domain Entity Used Directly as HTTP Response

**Concepts**
- Sensitive field exposure from direct entity serialization
- Domain model shape change propagating to API contract
- Circular reference and infinite recursion in JSON serializer
- Dedicated output DTO at each boundary

**Answer**

Returning a Domain Entity directly from a controller exposes every property on the entity to the API consumer, including internal audit fields, cost calculations, or navigation properties that should remain private, and it couples the public API contract to the internal domain model shape so any domain refactoring changes the wire format. Additionally, domain entities with navigation properties cause JSON serializers to follow object graphs and produce either circular reference exceptions or unintentionally deep JSON trees. The correct approach is to map the Domain Entity to a dedicated output DTO in the Application layer and return that DTO from the handler, keeping the HTTP contract independent of the domain model.

---

#### Gotcha 7. Infrastructure Types Registered Directly in `Program.cs`

**Concepts**
- Composition root leaking infrastructure type names
- DI wiring responsibility belonging to AddInfrastructure extension
- Testability broken when infrastructure types are hard-coded in the host
- Per-layer ServiceCollection extension methods as the pattern

**Answer**

When `Program.cs` contains lines like `services.AddScoped<SqlOrderRepository>()` or `services.AddDbContext<AppDbContext>(...)`, the host project has taken on knowledge of internal infrastructure types — knowledge that should be hidden inside an `AddInfrastructure(IServiceCollection, IConfiguration)` extension method in the Infrastructure project. Registering infrastructure types directly in `Program.cs` means you cannot swap implementations without editing the host, and integration test hosts must replicate the same registration list manually. The per-layer extension method pattern (each layer exposes one `AddXxx` method) keeps `Program.cs` at three lines and makes the infrastructure swappable by changing only the extension method body.

---

#### Gotcha 8. Application Layer Referencing Infrastructure for Concrete Types

**Concepts**
- Application project adding a project reference to Infrastructure
- Concrete EF Core types visible in Use Case handlers
- Circular dependency risk as the compile-time symptom
- Interface abstraction as the only allowed reference direction

**Answer**

Adding a project reference from Application to Infrastructure is a red flag that the Dependency Rule has been violated — handlers start calling `_dbContext.SaveChangesAsync()` directly instead of going through the `IUnitOfWork` abstraction, or they new up `EmailSender` directly instead of injecting `IEmailSender`. Once Application references Infrastructure, the entire inward-only dependency direction is destroyed and the domain core is no longer infrastructure-independent. The allowed reference graph is `Infrastructure → Application → Domain`; Application must only know about interfaces it defines itself, and Infrastructure implements those interfaces.

---

#### Gotcha 9. Sharing Domain Entities Across Bounded Contexts

**Concepts**
- Bounded context isolation preventing cross-context coupling
- Shared kernel versus shared entity distinction
- One change propagating breakage to multiple contexts
- Anti-Corruption Layer for cross-context communication

**Answer**

Using the same Domain Entity class in two different bounded contexts — for example, a `Customer` entity shared between an Order context and a Billing context — creates tight coupling that defeats the purpose of bounded context separation. Changes to `Customer` required by the Order context will break the Billing context, and the single class accumulates fields from both contexts until it serves neither well. Each bounded context must own its own model of the concept: the Billing context has its `BillingCustomer` with billing-specific fields, and the Order context has its `OrderingCustomer` with order-specific fields; an Anti-Corruption Layer or an integration event translates between them when they need to communicate.

---

#### Gotcha 10. Unit of Work Scope Mismatched With Business Transaction

**Concepts**
- Single business operation spanning multiple repositories
- SaveChangesAsync called in each repository instead of once
- Unit of Work coordinating a single transaction across repositories
- Partial save leaving data in an inconsistent state

**Answer**

When multiple repositories each call `_dbContext.SaveChangesAsync()` independently, a business operation that modifies two aggregates — creating an order and decrementing inventory — can partially succeed: the order is saved but the inventory update fails in a separate call, leaving the database in an inconsistent state. The Unit of Work pattern addresses this by giving a single `IUnitOfWork.CommitAsync()` call that saves all changes accumulated across multiple repositories in one database transaction. A common mistake in Clean Architecture is treating the repository as responsible for saving — the repository handles loading and staging changes, and the Application layer calls `unitOfWork.CommitAsync()` once at the end of the Use Case to commit the entire business operation atomically.

---
