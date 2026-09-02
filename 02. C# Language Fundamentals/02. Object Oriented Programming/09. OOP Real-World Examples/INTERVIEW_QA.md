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

## Gotcha Questions

---

## Q13. `OrderPaymentService` withdraws from a wallet, then calls `CardPaymentProcessor.ProcessOrderPayment()`. If the card is declined, the wallet is refunded. What OOP and correctness problems exist?

**Concepts**
- Encapsulation failure — accessing balance state directly
- Concrete class dependencies instead of interfaces
- Non-atomic rollback pattern (race between withdraw and refund)
- Hard-coded fallback to wallet processor
- SRP violation — service does too much

```csharp
public sealed class OrderPaymentService
{
    public string Run(BankAccount wallet, decimal total, string orderRef)
    {
        var card = new CardPaymentProcessor();
        var walletGw = new WalletPaymentProcessor();

        if (wallet.Balance < total)
            return "Insufficient funds";

        wallet.TryWithdraw(total, out _);

        string result = card.ProcessOrderPayment(total, orderRef);
        if (result.Contains("declined", StringComparison.OrdinalIgnoreCase))
        {
            wallet.Deposit(total);
            result = walletGw.ProcessOrderPayment(total, "WLT-" + orderRef);
        }
        return result;
    }
}
```

| Category | Problem | Impact |
|---|---|---|
| Concrete dependency | `new CardPaymentProcessor()` — not injectable or mockable | Tests hit real payment gateway |
| Encapsulation breach | `wallet.Balance < total` reads internal state before calling `TryWithdraw` | TOCTOU race on balance |
| Non-atomic rollback | Withdraw succeeds; card call may fail before `Deposit` runs | Balance permanently reduced if exception occurs between the two |
| Ignored return value | `TryWithdraw` out `_` discarded | Assumes success; balance may not have been deducted |
| SRP violation | Payment selection, execution, and fallback all in one method | Method will grow unbounded as payment types increase |

**Fix priority list**
1. Replace `new CardPaymentProcessor()` / `new WalletPaymentProcessor()` with injected `IPaymentProcessor` implementations.
2. Replace TOCTOU balance check with a single `TryWithdraw` call and use its return value.
3. Wrap withdraw + charge in a transaction scope or compensating transaction — do not withdraw until the charge succeeds, or use a two-phase commit pattern.
4. Extract payment fallback selection into a `IPaymentFallbackStrategy`.

**Answer**

Three independent problems compound here. First, `new CardPaymentProcessor()` creates a hard dependency on the concrete class, making tests impossible without hitting real payment infrastructure. Inject `IPaymentProcessor` via the constructor. Second, reading `wallet.Balance` before calling `TryWithdraw` is a TOCTOU race: balance changes between the check and the withdraw are invisible. Use the return value of `TryWithdraw` to learn whether the withdrawal succeeded. Third, the rollback pattern is not atomic: if an exception is thrown between `wallet.TryWithdraw(total, out _)` and `wallet.Deposit(total)` (for example, a network timeout during the card call), the customer's wallet is permanently debited with no refund. The correct pattern is to not withdraw from the wallet until the payment gateway confirms — or use a saga/compensating transaction approach where the withdrawal is held as a reservation until confirmed. The method also violates SRP by combining payment method selection, execution, and fallback logic.

---

## Q14. A logistics API uses an explicit type-switch to quote delivery cost by vehicle type. New vehicle types require editing the method. What OOP design replaces this?

**Concepts**
- Open/Closed Principle violation
- Polymorphic method on the base class
- Each vehicle calculates its own cost
- Abstract method or virtual override
- Extensible without modifying the dispatch method

```csharp
public static decimal QuoteDelivery(Vehicle vehicle, decimal distanceKm, decimal ratePerKm)
{
    if (vehicle is Car)
        return distanceKm * ratePerKm;
    if (vehicle is Truck truck)
        return distanceKm * ratePerKm * (1.0m + truck.PayloadTons * 0.05m);
    return distanceKm * ratePerKm * 2.0m;  // unknown fallback
}
```

**Answer**

The method is a classic OCP violation: every new vehicle type requires editing `QuoteDelivery`, and the fallback constant `2.0m` silently produces wrong quotes for unhandled types. The correct design adds an abstract or virtual method to `Vehicle`: `public abstract decimal EstimateDeliveryCostKm(decimal ratePerKm)`. Each vehicle implements the formula that makes sense for its type — `Car` returns `distanceKm * ratePerKm`, `Truck` applies the payload surcharge, `Motorcycle` applies a different rate. The `QuoteDelivery` method becomes `return vehicle.EstimateDeliveryCostKm(ratePerKm) * distanceKm` — one line, no type checks, no fallback, no editing required when new vehicles are added. The compiler enforces that every new concrete `Vehicle` subclass implements the abstract method, eliminating the silent-omission fallback. This is the OCP closed-for-modification / open-for-extension payoff: adding `Drone` as a new vehicle type requires only writing `Drone : Vehicle` with its own `EstimateDeliveryCostKm` — no existing code is touched.

---

## Q15. `OrderFulfillmentHub.Fulfill` does everything: payment, delivery, labeling, notification, and invoicing. Identify the SOLID violations and describe the refactor order.

**Concepts**
- SRP — five responsibilities in one class
- DIP — `new Truck(...)`, `new Circle(...)` hard-coded
- OCP — adding a delivery type requires editing `Fulfill`
- God class anti-pattern
- Service extraction and constructor injection order

```csharp
public sealed class OrderFulfillmentHub
{
    public string Fulfill(string customer, string orderRef, decimal total)
    {
        CustomerWallet.TryWithdraw(total, out _);
        var card = new CardPaymentProcessor();
        card.ProcessOrderPayment(total, orderRef);
        var truck = new Truck("Tata", "LPT", 2021, 3.5m);
        decimal cost = truck.EstimateDeliveryCostKm(2.4m) * 12.5m;
        var circle = new Circle(3.5);
        string label = $"Label area={Math.PI * circle.Radius * circle.Radius:0.##}";
        var email = new EmailNotificationSender();
        email.Send(customer, $"Order {orderRef} for {total:C}");
        var invoice = new InvoiceDocument("INV-1", DateTime.UtcNow, customer, total);
        return invoice.Render() + $" | delivery={cost:C} | {label}";
    }
}
```

**Answer**

`OrderFulfillmentHub.Fulfill` violates SRP by handling payment processing, delivery cost calculation, label geometry, email notification, and invoice generation — five separate responsibilities, each a reason to change the class. It violates DIP by instantiating concrete classes with `new` instead of using injected interfaces. It violates OCP because adding a new vehicle type for delivery requires editing this method. The refactor order for a single sprint: (1) First, introduce interfaces for the types with the most churn or test pain — `IPaymentProcessor`, `IDeliveryService`, `INotificationSender`, `IInvoiceService` — and inject them via constructor. This immediately enables test doubles. (2) Extract each responsibility into its own service class registered in DI. (3) Replace hard-coded geometry with a `ILabelGenerator` that encapsulates shape-based label creation. (4) Defer: infrastructure concerns like transactionality, retry, and distributed tracing can be addressed in a follow-up sprint once the seams are clear. Each step can be validated by a test that was previously impossible to write without real infrastructure.

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
