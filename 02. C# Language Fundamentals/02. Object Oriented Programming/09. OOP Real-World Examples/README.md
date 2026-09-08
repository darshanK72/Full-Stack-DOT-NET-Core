# C# OOP Real-World Examples — Interview Q&A


## Table of Contents

1. [Q1. What is the difference between an entity and a value object in domain modeling, and how does C# support each?](#q1-what-is-the-difference-between-an-entity-and-a-value-object-in-domain-modeling-and-how-does-c-support-each)
2. [Q2. What is the Strategy pattern, and how does it use interfaces in C#?](#q2-what-is-the-strategy-pattern-and-how-does-it-use-interfaces-in-c)
3. [Q3. What is the Repository pattern, and why is an interface critical to its value?](#q3-what-is-the-repository-pattern-and-why-is-an-interface-critical-to-its-value)
4. [Q4. What is the Factory Method pattern, and when do you use it over direct `new` construction?](#q4-what-is-the-factory-method-pattern-and-when-do-you-use-it-over-direct-new-construction)
5. [Q5. What is the Observer pattern, and how does C# events implement it?](#q5-what-is-the-observer-pattern-and-how-does-c-events-implement-it)
6. [Q6. What does "composition over inheritance" mean, and when does it apply?](#q6-what-does-composition-over-inheritance-mean-and-when-does-it-apply)
7. [Q7. What are the five SOLID principles, and which one is most commonly violated in C#?](#q7-what-are-the-five-solid-principles-and-which-one-is-most-commonly-violated-in-c)
8. [Q8. What is the `IDisposable` pattern's role in OOP, and when should a domain class implement it?](#q8-what-is-the-idisposable-patterns-role-in-oop-and-when-should-a-domain-class-implement-it)
9. [Q9. What is the Decorator pattern, and how does it complement interface-based design?](#q9-what-is-the-decorator-pattern-and-how-does-it-complement-interface-based-design)
10. [Q10. What is the Template Method pattern, and how does it differ from the Strategy pattern?](#q10-what-is-the-template-method-pattern-and-how-does-it-differ-from-the-strategy-pattern)
11. [Q11. How does the Dependency Inversion Principle change how you design class interactions?](#q11-how-does-the-dependency-inversion-principle-change-how-you-design-class-interactions)
12. [Q12. What distinguishes a "rich domain model" from an "anemic domain model", and why does it matter in C#?](#q12-what-distinguishes-a-rich-domain-model-from-an-anemic-domain-model-and-why-does-it-matter-in-c)
13. [Q13. `OrderPaymentService` withdraws from a wallet, then calls `CardPaymentProcessor.ProcessOrderPayment()`. If the card is declined, the wallet is refunded. What OOP and correctness problems exist?](#q13-orderpaymentservice-withdraws-from-a-wallet-then-calls-cardpaymentprocessorprocessorderpayment-if-the-card-is-declined-the-wallet-is-refunded-what-oop-and-correctness-problems-exist)
14. [Q14. A logistics API uses an explicit type-switch to quote delivery cost by vehicle type. New vehicle types require editing the method. What OOP design replaces this?](#q14-a-logistics-api-uses-an-explicit-type-switch-to-quote-delivery-cost-by-vehicle-type-new-vehicle-types-require-editing-the-method-what-oop-design-replaces-this)
15. [Q15. `OrderFulfillmentHub.Fulfill` does everything: payment, delivery, labeling, notification, and invoicing. Identify the SOLID violations and describe the refactor order.](#q15-orderfulfillmenthubfulfill-does-everything-payment-delivery-labeling-notification-and-invoicing-identify-the-solid-violations-and-describe-the-refactor-order)
16. [Q16. DI lifetimes for fulfillment types are wrong: `BankAccount` registered as singleton, processors as transient, `INotificationSender` as singleton. What breaks at runtime?](#q16-di-lifetimes-for-fulfillment-types-are-wrong-bankaccount-registered-as-singleton-processors-as-transient-inotificationsender-as-singleton-what-breaks-at-runtime)
17. [Q17. A monolithic fulfillment `Main` method creates every object, mutates wallet state, and prints the invoice. You have one sprint. What is your refactor priority order?](#q17-a-monolithic-fulfillment-main-method-creates-every-object-mutates-wallet-state-and-prints-the-invoice-you-have-one-sprint-what-is-your-refactor-priority-order)

---
## Foundation Questions

---

## Q1. What is the difference between an entity and a value object in domain modeling, and how does C# support each?

**Concepts**
- Entity: identity-based equality, mutable lifecycle
- Value object: structural (value) equality, immutable
- `record` for value objects in C# 9+
- `class` with `Id` property for entities
- Aggregate roots and consistency boundaries

**Answer**

An entity is an object whose identity is defined by a unique identifier — two `Order` objects with the same `OrderId` represent the same real-world order regardless of their other attributes. Entities are typically mutable: their state changes over time (order status, shipping address) while their identity remains fixed. A value object is defined entirely by its data — two `Money` objects with the same amount and currency are interchangeable, and neither has a meaningful identity. Value objects should be immutable: changing a money amount means creating a new `Money` rather than modifying the existing one. In C# 9+, `record` types provide built-in structural equality and non-destructive mutation (`with`), making them ideal for value objects. Entities are best modeled as `class` types with a private-set or init-only `Id` property. Aggregate roots are entities that own a consistency boundary: all mutations to the aggregate (entity + related objects) go through the root, which enforces invariants.

---

## Q2. What is the Strategy pattern, and how does it use interfaces in C#?

**Concepts**
- Encapsulating interchangeable algorithms behind an interface
- Runtime selection of strategy
- Open/Closed Principle — new strategies without modifying consumer
- Dependency injection of strategy via constructor
- Avoids conditional logic (if/switch) in consuming code

**Answer**

The Strategy pattern defines a family of algorithms, encapsulates each behind a common interface, and makes them interchangeable. In C#, this means declaring `IPaymentProcessor` with a `string ProcessOrderPayment(decimal total, string orderRef)` method and providing `CardPaymentProcessor` and `WalletPaymentProcessor` as separate implementations. The consuming class accepts an `IPaymentProcessor` via constructor injection and calls it without knowing which specific implementation it received. Adding a new payment method means writing a new class implementing the interface — the consumer never changes. This is the Open/Closed Principle: open for extension (new strategies), closed for modification (consumer stays unchanged). Strategy is also trivially testable: inject a mock `IPaymentProcessor` in unit tests to assert that the consumer calls the processor with the correct arguments without hitting real payment infrastructure.

---

## Q3. What is the Repository pattern, and why is an interface critical to its value?

**Concepts**
- Abstracts data access behind a collection-like interface
- `IOrderRepository` decouples domain from persistence technology
- In-memory implementation for testing
- Single responsibility — domain logic does not contain SQL
- Enables switching storage backends without changing domain

**Answer**

The Repository pattern abstracts the mechanism for retrieving and persisting domain objects behind a collection-like interface: `IOrderRepository` with `GetById(OrderId)`, `Add(Order)`, `Remove(Order)`, and `FindByCustomer(CustomerId)`. The domain layer depends only on `IOrderRepository`; it never references `SqlConnection`, EF Core `DbContext`, or any persistence infrastructure. An `EfOrderRepository` implementing the interface handles the actual SQL. For testing, an `InMemoryOrderRepository` holds a `Dictionary<OrderId, Order>` and provides instant, database-free tests of all domain logic. The interface is what makes this valuable: without it, the domain is tightly coupled to the storage technology, making tests slow and brittle. The repository is registered in DI: `builder.Services.AddScoped<IOrderRepository, EfOrderRepository>()`. In tests, a test-specific registration swaps in the in-memory version.

---

## Q4. What is the Factory Method pattern, and when do you use it over direct `new` construction?

**Concepts**
- Encapsulates object creation in a method/class
- Returns an interface or abstract type, not a concrete type
- Callers decouple from the specific concrete type
- Enables parameterized, validated, or conditional creation
- `Create(...)` static factory method vs constructor

**Answer**

The Factory Method pattern moves object creation responsibility into a dedicated method, hiding which concrete type is constructed and what initialization it requires. A static factory like `PaymentProcessor.Create(PaymentMethod method)` returns `IPaymentProcessor` and conditionally instantiates `CardPaymentProcessor` or `WalletPaymentProcessor` based on the method enum. Callers receive an interface reference and never know the concrete type, so switching the implementation requires only changing the factory. Factory methods also enable richer creation semantics than constructors: they can return `null` (try-create pattern), return a cached instance, apply validation before constructing, or throw a domain-specific exception (`PaymentMethodNotSupportedException`) instead of a generic `ArgumentException`. Direct `new` is fine for simple types with no polymorphism concerns; factory methods are appropriate when the concrete type is a decision, when creation is complex or can fail, or when the caller should depend on an abstraction.

---

## Q5. What is the Observer pattern, and how does C# events implement it?

**Concepts**
- Subject (publisher) notifies observers (subscribers) of state changes
- Loose coupling — subject does not know observer types
- `event` keyword as the built-in Observer mechanism
- Subscribe via `+=`, unsubscribe via `-=`
- Alternative: `IObservable<T>` / `IObserver<T>` for richer semantics

**Answer**

The Observer pattern defines a one-to-many dependency: when a subject changes state, all registered observers are automatically notified. C# events implement this pattern directly. `BankAccount.BalanceChanged` is the subject's notification point; `AccountDetailPanel` subscribes with `_account.BalanceChanged += OnBalanceChanged`. The `BankAccount` knows nothing about `AccountDetailPanel` or any other observer — it raises the event and the delegate invocation list notifies everyone. Adding a new observer requires no modification to `BankAccount`, satisfying OCP. Unsubscribing in `Dispose()` keeps the memory safe. For more complex reactive scenarios — filtering events, merging streams, throttling high-frequency notifications — `IObservable<T>` and Rx.NET provide operator pipelines over the same conceptual pattern. In domain-driven design, domain events (separate from C# events) follow the same Observer concept but are typically dispatched through `MediatR` or a message bus for durability and cross-service notification.

---

## Q6. What does "composition over inheritance" mean, and when does it apply?

**Concepts**
- Favor HAS-A (fields) over IS-A (base class)
- Compose behavior from injected dependencies
- More flexible than deep inheritance hierarchies
- Follows Dependency Inversion Principle
- Avoids fragile base class problem

**Answer**

"Composition over inheritance" means building complex behavior by combining objects (fields/properties of different types) rather than through a rigid inheritance chain. When `OrderFulfillmentCoordinator` needs to process payments, deliver items, and send notifications, it HAS-A `IPaymentProcessor`, a `IDeliveryService`, and an `INotificationSender` rather than inheriting from `PaymentBase` or `DeliveryBase`. Each concern is a separately injectable, independently testable component. This is more flexible than inheritance because the combination can be changed at runtime (pass a different `IPaymentProcessor` for different order types) and at configuration time (DI registration). It avoids the fragile base class problem because `OrderFulfillmentCoordinator` does not depend on any implementation details of its collaborators. The guideline is: use inheritance when the derived class genuinely IS a specialized version of the base with no possibility of the same behavior being needed across unrelated hierarchies; use composition when you are assembling capabilities.

---

## Q7. What are the five SOLID principles, and which one is most commonly violated in C#?

**Concepts**
- SRP: Single Responsibility Principle
- OCP: Open/Closed Principle
- LSP: Liskov Substitution Principle
- ISP: Interface Segregation Principle
- DIP: Dependency Inversion Principle
- SRP and OCP most commonly violated in practice

**Answer**

SOLID is an acronym for five design principles that guide maintainable OOP design. Single Responsibility Principle (SRP): each class has exactly one reason to change. Open/Closed Principle (OCP): classes are open for extension but closed for modification. Liskov Substitution Principle (LSP): subtypes must be substitutable for their base types. Interface Segregation Principle (ISP): interfaces should be narrow and role-specific rather than fat. Dependency Inversion Principle (DIP): high-level modules depend on abstractions, not concrete implementations. In practice, SRP is the most commonly violated — classes grow organically to include payment, validation, logging, and notification in one class. OCP is violated whenever a switch-on-type pattern requires modifying a method when new types are added. DIP is addressed by ASP.NET Core's DI container, which encourages programming to interfaces. Remembering that SOLID is a guideline for managing change — each principle targets a specific class of maintenance problem — helps prioritize which violations to fix in a given codebase.

---

## Q8. What is the `IDisposable` pattern's role in OOP, and when should a domain class implement it?

**Concepts**
- Deterministic resource release
- `using` statement guarantees cleanup
- Domain class should implement when it owns unmanaged or heavyweight resources
- Wrapper classes over streams, connections, handles
- `IAsyncDisposable` for async cleanup

**Answer**

`IDisposable` signals that a class owns resources that should be deterministically released rather than waiting for garbage collection. In domain modeling, most entities should not implement `IDisposable` — their state is pure data without resources. Implement `IDisposable` when the class wraps an external resource: a database connection, file handle, HTTP client, timer, or subscription to an external event. In OOP terms, the `Dispose()` method is the partner to the constructor — the constructor acquires resources, `Dispose()` releases them. Using `using` or `using var` makes this symmetric at the call site: the resource lives exactly as long as the block. For composite objects that own multiple disposable components, implement the full dispose pattern — `protected virtual Dispose(bool disposing)` called from `Dispose()` with `GC.SuppressFinalize(this)`. In .NET 10, `IAsyncDisposable` with `await using` handles resources that require async operations to release (e.g., flushing a buffer, draining a queue, closing an async network connection).

---

## Q9. What is the Decorator pattern, and how does it complement interface-based design?

**Concepts**
- Wraps an interface implementation with added behavior
- Same interface as the wrapped type
- Transparent to consumers
- Alternative to subclassing for cross-cutting concerns
- Enables runtime composition of behaviors

**Answer**

The Decorator pattern wraps an existing object implementing an interface with a new object that also implements the same interface, adding behavior before or after delegating to the wrapped object. For example, `LoggingPaymentProcessor` implements `IPaymentProcessor`, holds an `IPaymentProcessor _inner`, logs the call, delegates to `_inner.ProcessOrderPayment(...)`, logs the result, and returns it. Consumers receive an `IPaymentProcessor` and never know whether it is the real processor or a logging wrapper. Multiple decorators can be chained — logging wraps retry wraps the real processor — without modifying any individual component. This is composition of cross-cutting concerns (logging, retry, caching, authorization) without inheritance or modifying the core implementation. In ASP.NET Core DI, decorators can be registered using `builder.Services.Decorate<IPaymentProcessor, LoggingPaymentProcessor>()` via Scrutor or built-in service descriptor manipulation. Decorators are cleaner than subclassing for cross-cutting concerns because they do not require the base class to be designed for extension.

---

## Q10. What is the Template Method pattern, and how does it differ from the Strategy pattern?

**Concepts**
- Template Method: algorithm skeleton in base, steps in derived classes
- Strategy: swappable algorithm via interface
- Template Method uses inheritance; Strategy uses composition
- Template Method steps are `protected virtual`; Strategy algorithm is injected
- Both address OCP but at different scopes

**Answer**

The Template Method pattern defines an algorithm's skeleton in a base class method, delegating specific steps to `protected virtual` or `abstract` methods that derived classes implement. For example, `Document.Render()` could be a non-virtual template method that calls `virtual GetHeader()`, `abstract RenderContent()`, and `virtual GetFooter()` in sequence. Derived classes override only the steps they customize; the overall flow is fixed by the base. Strategy externalizes the entire algorithm as an injected object: the consumer holds an `IRenderer` and delegates the whole rendering operation. The key difference is inheritance vs composition. Template Method couples the algorithm to the class hierarchy — you need a new subclass for each variation of a step. Strategy allows runtime substitution of the entire algorithm without subclassing. Use Template Method when the overall structure is stable and variations are at well-defined points within an established class hierarchy. Use Strategy when algorithms are interchangeable, need runtime selection, or exist across unrelated types.

---

## Q11. How does the Dependency Inversion Principle change how you design class interactions?

**Concepts**
- High-level modules depend on abstractions (interfaces)
- Low-level modules implement those abstractions
- Both depend on the abstraction, not on each other
- Abstractions defined by the high-level module's needs
- Constructor injection as the primary DI mechanism

**Answer**

Without Dependency Inversion, high-level business logic directly depends on low-level implementations: `OrderService` instantiates `SqlOrderRepository` and `SmtpEmailSender`. Changing the database or email provider requires modifying `OrderService`. DIP reverses this: `OrderService` is written against `IOrderRepository` and `IEmailSender` interfaces; the low-level classes implement those interfaces. The high-level module defines the interfaces it needs — making the abstraction the product of the business need, not the technology. The concrete implementations then implement those interfaces. In ASP.NET Core, constructor injection enforces this at runtime: if `OrderService` requires `IOrderRepository`, the DI container resolves and injects the registered implementation. The result is that high-level logic can be fully tested with mock implementations and that low-level technology can be swapped without touching any business logic file. This is the practical payoff of DIP in production codebases.

---

## Q12. What distinguishes a "rich domain model" from an "anemic domain model", and why does it matter in C#?

**Concepts**
- Anemic: public setters, logic in service classes
- Rich: behavior co-located with state, private fields
- Domain invariants enforced by the entity
- DDD alignment — entities protect their own consistency
- Testability: test entity behavior without service layer

**Answer**

An anemic domain model has entities that are plain data bags — public getters and setters everywhere, no methods that enforce business rules. All logic lives in service classes outside the entity. `OrderService.ApplyDiscount(order, percent)` reads and sets `order.DiscountRate` directly. A rich domain model places behavior on the entity: `order.ApplyDiscount(percent)` validates the percent, updates the internal state, records the change in a domain event, and enforces invariants — all in one place. The practical consequences of anemia: business rules are scattered across services and duplicated, invariants are only as consistent as the discipline of every developer who writes service code, and testing requires constructing entire service objects rather than testing the entity in isolation. Rich models are easier to test (construct the entity, call its methods, assert state), more self-documenting (the entity's public methods tell you what it can do), and more robust (invariants cannot be bypassed). The cost is slightly more complex entity design, but the investment pays off quickly as the model grows.

---

## Gotchas — OOP Real-World Design (Interview Traps)

---

#### Gotcha 1. Concrete class dependencies created with new are untestable — inject interfaces instead

**Concepts**
- `new ConcreteClass()` inside a method hard-wires dependencies
- No seam for test doubles
- DIP: depend on abstractions, not concretions
- Constructor injection via interface
- Real infrastructure hits in tests

**Answer**

When a method calls `new CardPaymentProcessor()`, it creates a hard dependency on the concrete class. Tests cannot substitute a fake without hitting real infrastructure. Inject `IPaymentProcessor` via the constructor instead. The DIP says high-level modules should not depend on low-level modules — both should depend on abstractions.

---

#### Gotcha 2. Type-switch on domain objects is an OCP violation — replace with polymorphic method on the base class

**Concepts**
- `is TypeA` / `is TypeB` checks in a switch
- Adding new types requires editing the switch
- OCP: closed for modification, open for extension
- Abstract or virtual method on base class
- Each type owns its own behavior

**Answer**

An explicit type-switch that routes behavior by `vehicle is Car`, `vehicle is Truck`, etc. must be edited every time a new type is introduced, violating the Open/Closed Principle. Add an abstract or virtual method to the base class so each type encapsulates its own logic. The switch becomes a single polymorphic call with no editing needed for new types.

---

#### Gotcha 3. God class with all responsibilities violates SRP — extract each responsibility into its own service

**Concepts**
- SRP: one reason to change per class
- God class accumulates all responsibilities
- Extract service classes with narrow contracts
- Inject extracted services via constructor
- Each step independently testable

**Answer**

A class that handles payment, delivery, labeling, notification, and invoicing has five reasons to change — one for each responsibility. The refactor order is: introduce interfaces for each responsibility, inject them via the constructor, then extract each into its own class registered in DI. Each extracted service can be tested in isolation.

---

#### Gotcha 4. TOCTOU race — checking state before acting is not atomic with the action

**Concepts**
- Time-of-check to time-of-use (TOCTOU)
- Balance may change between check and withdrawal
- Use return value of the mutating call instead
- Atomic operations via `TryWithdraw` pattern
- Two-phase operations need compensation or reservation

**Answer**

Reading `wallet.Balance < total` and then calling `TryWithdraw` is a TOCTOU race — the balance can change between the two calls. Instead, use the return value of `TryWithdraw` itself: if it returns false, the funds were insufficient at the time of the attempt. The check and the action must be the same atomic operation.

---

#### Gotcha 5. Non-atomic rollback leaves state permanently corrupted on exception between withdraw and refund

**Concepts**
- Withdraw then charge — exception between them debits wallet with no charge
- Not atomic: no transaction wraps both
- Saga / compensating transaction pattern
- Two-phase: reserve then confirm
- Do not withdraw until charge succeeds

**Answer**

If the charge call throws after a successful wallet withdrawal, the wallet is debited but no charge was made. The rollback (`wallet.Deposit`) is also never reached. Use a two-phase pattern: reserve funds, attempt the charge, then confirm the reservation on success or release it on failure. A compensating transaction service logs both operations so failed charges can be reconciled.

---

#### Gotcha 6. Encapsulation breach — reading internal state from outside the owning class bypasses invariants

**Concepts**
- `wallet.Balance` read from outside BankAccount
- External reader can make decisions based on stale state
- Encapsulate in the owning object
- Tell, don't ask: give the object the command, let it decide
- Returns result instead of checking precondition externally

**Answer**

Reading `wallet.Balance` from an external service is an encapsulation breach — the caller makes decisions based on internal state that may be stale or inconsistently read. Apply "tell, don't ask": give `BankAccount` a `TryWithdraw` method that encapsulates the check-and-withdraw atomically and returns whether it succeeded. The caller acts on the result, not on pre-read state.

---

#### Gotcha 7. Ignoring return values from Try-pattern methods assumes success silently

**Concepts**
- `TryWithdraw(total, out _)` return value discarded
- Method may return false (insufficient funds)
- Code continues as if success regardless
- Always check return values for Try-pattern methods
- Prefer `bool TryX(...)` over void with exceptions for control flow

**Answer**

Calling `wallet.TryWithdraw(total, out _)` and ignoring the `bool` return means the code proceeds even if the withdrawal failed. The downstream logic (charging a card, generating an invoice) runs on the assumption that the withdrawal succeeded when it may not have. Always check and act on the return value of any `Try`-pattern method.

---

#### Gotcha 8. Hard-coded fallback values produce silently wrong results for unhandled cases

**Concepts**
- `return distanceKm * ratePerKm * 2.0m` for unknown type
- No exception, no log, wrong number returned
- Fail-fast is preferable: throw for unrecognized types
- Exhaustive pattern matching in C# 8+
- Abstract method eliminates the fallback entirely

**Answer**

A fallback constant like `2.0m` for unknown vehicle types silently produces a plausible-looking but wrong quote. The caller has no indication anything went wrong. Prefer either an `abstract` method (which forces all types to provide an implementation) or a `throw new NotSupportedException($"Unhandled vehicle type: {vehicle.GetType().Name}")` so unrecognized types are detected immediately.

---

#### Gotcha 9. Static factory method pattern hides the constructor but does not enforce interface injection

**Concepts**
- Static factory can return different concrete types
- Still uses concrete type internally unless designed for DI
- Useful for controlled construction with validation
- Cannot be replaced in tests without DI
- Named factory + interface + DI is the testable pattern

**Answer**

A static factory method like `Payment.Create(amount)` hides object construction behind a readable name and can enforce creation rules, but it still returns a concrete class by value. Tests that call the factory get the real implementation. For testability, register a factory interface (`IPaymentFactory`) in DI and inject it — tests can substitute a fake factory that returns mock payment objects.

---

#### Gotcha 10. Law of Demeter violation — method chains across object graphs create brittle coupling

**Concepts**
- `order.Customer.Address.City` chains three objects
- Changes anywhere in chain break the caller
- Encapsulate the query in the owning object
- Introduce a property or method at the right level
- Each object responsible for its own traversal

**Answer**

`order.Customer.Address.City` means the calling code knows the internal structure three levels deep. If `Customer` later wraps `Address` differently, or `Address` changes how `City` is stored, the caller breaks. The Law of Demeter says an object should only talk to its immediate collaborators. Encapsulate the navigation: `order.GetDeliveryCity()` delegates to its collaborators without exposing the chain.

---

## Q16. DI lifetimes for fulfillment types are wrong: `BankAccount` registered as singleton, processors as transient, `INotificationSender` as singleton. What breaks at runtime?

**Concepts**
- Singleton `BankAccount` shared across all requests
- Per-request state must be scoped or transient
- Captive dependency — transient injected into singleton
- `CardPaymentProcessor` transient but resolved new per `ServiceProvider` call
- Notification sender singleton leaks request-specific context

**Answer**

`BankAccount` registered as a singleton means every HTTP request shares the same account object, with the same balance. Concurrent requests withdraw from and deposit to this single shared account, producing race conditions and cross-customer balance contamination. `CardPaymentProcessor` registered as transient creates a new instance per DI resolution, but if it is resolved inside a singleton, the same processor instance is used across requests (the singleton holds its own resolved copy from construction time). `INotificationSender` as singleton is safe only if `EmailNotificationSender` is stateless; if it captures request-specific state (authentication context, correlation ID), it leaks that state across requests. The correct lifetime assignments: `BankAccount` should be scoped per request (or better: loaded from a repository per request, not registered as a service directly). `CardPaymentProcessor` as transient is correct if it is stateless. `INotificationSender` as singleton is correct if the implementation is stateless; otherwise scope it. Register `IOrderRepository` as scoped to ensure each request loads its own data from the database.

---

## Q17. A monolithic fulfillment `Main` method creates every object, mutates wallet state, and prints the invoice. You have one sprint. What is your refactor priority order?

**Concepts**
- Encapsulation fixes first — stop invariant violations
- Interfaces for testability seams
- Extract services for SRP
- Events/DI for extensibility
- Defer infrastructure concerns

**Answer**

With one sprint and a monolithic `Main`-style entrypoint, the priority order is strictly risk-driven, prioritizing correctness first and extensibility second. Sprint one: (1) Encapsulation — make `BankAccount.Balance` private-set and route all changes through `Deposit`/`TryWithdraw`. This stops the most dangerous invariant violations immediately with minimal code change. (2) Introduce interfaces — extract `IPaymentProcessor`, `INotificationSender`, and `IDeliveryService`. Replace the three hard-coded `new` calls in `OrderFulfillmentCoordinator` with constructor-injected parameters. This makes tests possible for the coordinator without changing any other code. (3) Wire up ASP.NET Core DI — register the interfaces in `Program.cs` with appropriate lifetimes; the coordinator becomes a DI service. (4) Defer: events for `BalanceChanged`, changing `Vehicle.EstimateDeliveryCostKm` to abstract, refactoring `Document` hierarchy, and distributed tracing should wait for sprint two. Each step in the priority list delivers immediate, measurable value and does not require finishing the entire refactor to be useful. The goal is to leave the codebase better than you found it at each checkin, not to complete a rewrite in one pass.
