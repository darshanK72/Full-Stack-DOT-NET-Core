# Interview Questions — CQRS Pattern — Interview Q&A
> 24 questions · Back to [README](../README.md)

## Table of Contents
1. [What is CQRS (Command Query Responsibility Segregation), and what problem does i…](#q1)
2. [How does CQRS differ from a traditional CRUD (Create, Read, Update, Delete) arch…](#q2)
3. [What is Bertrand Meyer's original CQS (Command-Query Separation) principle, and …](#q3)
4. [What is the Mediator pattern, and why does it complement CQRS in .NET applicatio…](#q4)
5. [What is a command in CQRS, how does it differ from a query, and what naming conv…](#q5)
6. [What should a command handler do, what should it return, and what responsibiliti…](#q6)
7. [How do you validate a command before the handler executes, and what is the diffe…](#q7)
8. [What is the difference between a synchronous command that returns a result and a…](#q8)
9. [What is a query in CQRS, and why should a query handler never modify application…](#q9)
10. [What is a read model (also called a projection), and why is it often a different…](#q10)
11. [When is it appropriate to query the write database directly versus maintaining a…](#q11)
12. [How do you wire up MediatR in an ASP.NET Core application, and what is the role …](#q12)
13. [What is a pipeline behavior (`IPipelineBehavior<TRequest, TResponse>`) in Mediat…](#q13)
14. [What is the difference between MediatR's `IRequest` (request/response) and `INot…](#q14)
15. [What are the risks of overusing MediatR in a codebase, and when is it unnecessar…](#q15)
16. [When should the read and write databases be physically separated, and what infra…](#q16)
17. [What is eventual consistency in a CQRS system, and how do you handle the lag bet…](#q17)
18. [What is a projection in Event-Sourced CQRS, and how does it differ from a simple…](#q18)
19. [What is Event Sourcing, and why is it frequently paired with CQRS?](#q19)
20. [What is the difference between an event store and a traditional relational datab…](#q20)
21. [What is a snapshot in Event Sourcing, and why is it needed?](#q21)
22. [What makes CQRS command and query handlers easier to unit test than a traditiona…](#q22)
23. [When should you NOT use CQRS, and what signals indicate that a simpler CRUD desi…](#q23)
24. [How do you avoid "handler explosion" — the proliferation of tiny single-use hand…](#q24)

---

## Q1. What is CQRS (Command Query Responsibility Segregation), and what problem does it solve in a distributed system?

What is CQRS (Command Query Responsibility Segregation), and what problem does it solve in a distributed system?

**Answer:** CQRS (Command Query Responsibility Segregation) is an architectural pattern that separates the part of the system that changes state (commands) from the part that reads state (queries) into two distinct models with independent code paths. In a traditional design, the same service and data model serve both writes and reads, forcing both to share the same performance and structural constraints. CQRS solves this by letting each side evolve, scale, and be optimized independently.

- Commands carry intent to change state — "place this order", "deactivate this account" — and go through a write model that enforces domain rules and invariants.
- Queries retrieve data optimized for display — often denormalized, pre-joined, or flattened — without the overhead of loading and validating a full domain object.
- In distributed systems, read traffic commonly dwarfs write traffic by an order of magnitude; separating the two allows the read side to be scaled horizontally, cached aggressively, or served from a different data store without affecting write throughput.
- The pattern also makes the system's intentions explicit in code: a `PlaceOrderCommand` tells you something is about to change, while a `GetOrderSummaryQuery` signals a safe, read-only operation.

---

## Q2. How does CQRS differ from a traditional CRUD (Create, Read, Update, Delete) architecture, and what are the trade-offs of adopting it?

How does CQRS differ from a traditional CRUD (Create, Read, Update, Delete) architecture, and what are the trade-offs of adopting it?

**Answer:** In a traditional CRUD (Create, Read, Update, Delete) architecture, a single service layer and a single data model handle all four operations, with controllers calling generic repository or service methods. CQRS replaces that unified model with two separate stacks — a command stack that writes and a query stack that reads — each with its own models, handlers, and optionally its own data store.

| Dimension | CRUD | CQRS |
|---|---|---|
| Data model | One model for reads and writes | Separate write model (domain) and read model (projection) |
| Code organization | Service with Save/Get/Delete methods | Distinct command handlers and query handlers |
| Query flexibility | Limited by the domain model's shape | Read model can be arbitrarily denormalized |
| Complexity | Low — straightforward CRUD is easy to understand | Higher — two paths to maintain, possible eventual consistency |
| Scaling | Both reads and writes scale together | Each side scales independently |

- The main cost of CQRS is that two code paths must be kept in sync; a business rule change may need to touch both a command handler and the read model that reflects it.
- The benefit is proportional to system complexity: a simple admin tool with low traffic gains almost nothing from CQRS, while a high-throughput order processing system gains significantly in flexibility and scalability.

---

## Q3. What is Bertrand Meyer's original CQS (Command-Query Separation) principle, and how does CQRS extend it to the architectural level?

What is Bertrand Meyer's original CQS (Command-Query Separation) principle, and how does CQRS extend it to the architectural level?

**Answer:** Bertrand Meyer's CQS (Command-Query Separation) principle states that every method should either be a command that changes state or a query that returns a value — never both. The canonical violation is a `Pop()` method on a stack that both removes and returns the top element simultaneously. CQRS takes this object-level principle and elevates it to the application-architecture level, separating the write side and read side into entirely distinct subsystems rather than just method signatures.

- At the method level, CQS means: if a method returns a value, it must not mutate state; if it mutates state, it must return void.
- At the architecture level, CQRS means: the command pathway (controllers → command handlers → domain → write database) is physically separate from the query pathway (controllers → query handlers → read database / projection).
- CQRS adds the concept of distinct models — the write model is a rich domain object enforcing invariants, while the read model is a simple DTO (Data Transfer Object) optimized for display — which CQS at the method level does not address.
- One practical consequence: in a CQRS system, a command handler typically returns either void or a minimal acknowledgement (such as the new entity's ID), rather than returning the fully updated resource the way a CRUD endpoint might.

---

## Q4. What is the Mediator pattern, and why does it complement CQRS in .NET applications?

What is the Mediator pattern, and why does it complement CQRS in .NET applications?

**Answer:** The Mediator pattern introduces an intermediary object that coordinates communication between components, so that components send messages to the mediator rather than invoking each other directly, reducing direct coupling. In .NET, MediatR is the dominant library implementing this pattern; a controller sends a command or query object to MediatR, which dispatches it to the registered handler without the controller knowing anything about the handler's implementation. This indirect dispatch is a natural fit for CQRS because each command and query becomes a self-contained message routed to exactly one handler.

- Without a mediator, CQRS handlers would need to be injected directly into controllers, creating a coupling between the HTTP layer and individual business operations — a controller might need five or six injected handler dependencies.
- With MediatR, a controller injects only `IMediator` and sends any message through it; the framework locates the correct handler at runtime via the registered type mapping.
- The mediator also enables a processing pipeline: behaviors (middleware-like objects) wrap every handler dispatch, allowing cross-cutting concerns like logging, validation, and transaction management to run transparently around every command and query.
- The trade-off is indirection: you lose compile-time navigation from a call site to its handler, making it harder to trace execution flow without IDE support or naming conventions.

---

## Chapter 2 — Commands and Command Handlers

---

## Q5. What is a command in CQRS, how does it differ from a query, and what naming conventions are typically used?

What is a command in CQRS, how does it differ from a query, and what naming conventions are typically used?

**Answer:** A command is a message that expresses an intent to change the system's state — it carries all data required to perform one specific action and is named in the imperative to reflect that intent. A query, by contrast, is a message that requests data without any side effects; the system's state must be identical before and after a query executes. This distinction makes each operation's intent visible at the type level rather than buried inside a generic service method.

- Commands use imperative verb-noun naming: `PlaceOrderCommand`, `CancelSubscriptionCommand`, `UpdateCustomerAddressCommand`. Queries use noun or noun-phrase naming: `GetOrderByIdQuery`, `ListActiveCustomersQuery`.
- A command is expected to succeed or fail with an explicit result (usually just the ID of the created resource or a unit/void acknowledgement), while a query returns the requested data.
- Commands are routed to exactly one handler that owns the responsibility for that state change; queries may also target a single handler but can more freely cross-read multiple data sources.
- Because a command carries intent, it forms a natural audit trail: you can log every `PlaceOrderCommand` as a user action, replay commands to rebuild state, or queue them for asynchronous processing.

---

## Q6. What should a command handler do, what should it return, and what responsibilities should it NOT have?

What should a command handler do, what should it return, and what responsibilities should it NOT have?

**Answer:** A command handler has one responsibility: accept a command object, apply the domain logic required to fulfill that intent, persist the resulting state change, and return a minimal acknowledgement. It should orchestrate the domain — load the aggregate, call domain methods, save the result — but it should not contain the domain rules themselves, and it should not format or shape data for the caller.

- A handler should return either `Task` (void) when no data is needed by the caller, or `Task<Guid>` / `Task<int>` when only the new entity's identifier is needed — not a full DTO of the saved entity, because reading the result is the read side's responsibility.
- Responsibilities that belong elsewhere: input validation (pipeline behavior or FluentValidation), authorization checks (policy or another behavior), data formatting and mapping (query handlers and read models), and cross-cutting concerns like logging and retry logic (pipeline behaviors).
- A handler that also loads data for the response, formats view models, and performs business validation all in one method is a sign the CQRS separation has broken down and the handler has become a mini-service.

```csharp
public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    public async Task<Guid> Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Place(cmd.CustomerId, cmd.Items); // domain method
        await _repo.AddAsync(order, ct);
        return order.Id;
    }
}
```

---

## Q7. How do you validate a command before the handler executes, and what is the difference between input validation and domain validation?

How do you validate a command before the handler executes, and what is the difference between input validation and domain validation?

**Answer:** In a MediatR-based CQRS system, input validation is best placed in a pipeline behavior that runs before the handler, using a library like FluentValidation to check that the command's data is structurally valid before any domain logic runs. Domain validation — checking business rules that require knowledge of persisted state, such as "an order cannot be placed if the customer's account is suspended" — belongs inside the domain itself and runs inside the handler.

- Input validation checks things the command object alone can answer: required fields present, string lengths within bounds, numeric values in range, valid enum members. It does not need a database call.
- Domain validation checks invariants that require loading domain objects: "does this product still exist?", "does the customer have sufficient credit?". This runs inside the handler after loading the aggregate.
- Separating the two means validation failures return fast (before any I/O) for structural errors, while business rule violations are surfaced as domain exceptions or result objects after the handler loads state.

```csharp
public class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.Items).NotEmpty().WithMessage("Order must have at least one item.");
    }
}
```

The pipeline behavior catches validation failures and throws `ValidationException` before `PlaceOrderHandler.Handle` is ever called, keeping the handler free of structural validation noise.

---

## Q8. What is the difference between a synchronous command that returns a result and a fire-and-forget command, and when do you use each?

What is the difference between a synchronous command that returns a result and a fire-and-forget command, and when do you use each?

**Answer:** A synchronous command executes within the current request and returns a result (such as a new entity ID or a success/failure indicator) before the HTTP response is sent; the caller waits for the operation to complete. A fire-and-forget command publishes the intent to a queue or message bus and returns immediately, with the actual processing happening asynchronously in a separate worker — the caller gets only an acknowledgement that the message was accepted, not that it was processed.

- Synchronous commands are appropriate when the caller needs the result to continue — for example, creating a new order and redirecting to its detail page requires the order ID before the response can be sent.
- Fire-and-forget commands are appropriate when processing is long-running (sending emails, generating reports, syncing with third-party systems) or when the system must remain responsive under high load without blocking HTTP threads.
- Fire-and-forget introduces eventual consistency between the client's perceived state and the actual system state; the client must be designed to poll for completion, receive a webhook callback, or display a "processing" status.
- A hybrid approach — process synchronously for the critical path (e.g., reserve inventory) and fire-and-forget for secondary effects (e.g., send confirmation email) — is common in production systems.

---

## Chapter 3 — Queries and Query Handlers

---

## Q9. What is a query in CQRS, and why should a query handler never modify application state?

What is a query in CQRS, and why should a query handler never modify application state?

**Answer:** A query in CQRS is a message that requests a specific piece of data from the system; it is purely read-only and must leave all application state exactly as it was before the query executed. Enforcing this immutability guarantee means that queries can be executed any number of times without side effects, cached freely, retried on failure, and run concurrently without coordination — none of which is true if a query might mutate something as a side effect.

- If a "query" also updates a "last-accessed" timestamp or increments a view counter, it is no longer a safe read-only operation; those side effects should instead be raised as a separate command or domain event handled independently.
- The safety guarantee allows the read side to be optimized without worrying about write consistency: a query handler can read from a read-only replica, an in-memory cache, or a separate denormalized read store.
- In code, this means a query handler should use read-only data access — `AsNoTracking()` in Entity Framework Core, read-only transaction isolation, or a direct Dapper query — rather than loading tracked domain objects that could be accidentally saved.

---

## Q10. What is a read model (also called a projection), and why is it often a different shape from the write model?

What is a read model (also called a projection), and why is it often a different shape from the write model?

**Answer:** A read model (or projection) is a data structure optimized specifically for the needs of a particular view or use case, typically denormalized, pre-joined, and shaped for the UI that will display it. The write model (the domain aggregate) is designed around enforcing business invariants and clustering related data for consistency during a state change, which usually means normalized entities with relationships. These two purposes pull the data model in opposite directions, which is why they should be separate.

- The write model for an order might be an `Order` aggregate containing `OrderLine` child entities with foreign keys to `Product` and `Customer` — normalized for relational integrity and domain rule enforcement.
- The read model for an order list page might be a flat `OrderSummaryDto` with customer name, item count, and total already computed — a single object the UI can render without joins or extra queries.
- Maintaining a separate read model means queries run faster (no ORM joins, no lazy loading, no change tracking overhead) and are isolated from changes to the write model's structure.
- Read models can be rebuilt at any time by replaying commands or events over the write store — they are always derivable from the source of truth, so they can be changed, versioned, or added without migrating existing data.

---

## Q11. When is it appropriate to query the write database directly versus maintaining a separate read store?

When is it appropriate to query the write database directly versus maintaining a separate read store?

**Answer:** Querying the write database directly is appropriate when the read requirements are simple, the data volume is low, and the query's shape is close to what the domain model already represents; a separate read store becomes worth the operational overhead when queries are complex, performance-sensitive, or require a fundamentally different data shape or technology. Most small-to-medium CQRS systems start by querying the write database with `AsNoTracking()` and only introduce a separate read store when a specific bottleneck justifies it.

- Direct querying of the write database with no-tracking Entity Framework Core queries or Dapper is straightforward, requires no synchronization infrastructure, and keeps the system consistent — there is no eventual consistency lag.
- A separate read store (Redis for key-value lookups, Elasticsearch for full-text search, a materialized view in a reporting database) enables specialized query capabilities and can be scaled independently of the write database.
- The threshold for introducing a separate read store is typically: the write database is becoming a read bottleneck under load, the query requires features unavailable in the write database (full-text, geospatial, graph traversal), or the read model needs to aggregate across multiple bounded contexts.
- Introducing a separate read store also introduces the need to keep it synchronized with the write side, usually via domain events or change-data capture — this is operational complexity that must be weighed against the query performance benefit.

---

## Chapter 4 — MediatR in ASP.NET Core

---

## Q12. How do you wire up MediatR in an ASP.NET Core application, and what is the role of `IRequest<T>` and `IRequestHandler<TRequest, TResponse>`?

How do you wire up MediatR in an ASP.NET Core application, and what is the role of `IRequest<T>` and `IRequestHandler<TRequest, TResponse>`?

**Answer:** MediatR is wired up by calling `builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly))` in `Program.cs`, which scans the specified assembly for all classes implementing `IRequestHandler<TRequest, TResponse>` and registers them with the DI container. `IRequest<T>` is the marker interface a command or query object implements to declare what type of result it produces; `IRequestHandler<TRequest, TResponse>` is the interface a handler class implements to process that specific request type.

```csharp
// Command (write side)
public record PlaceOrderCommand(Guid CustomerId, List<OrderItem> Items) : IRequest<Guid>;

// Handler
public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    public async Task<Guid> Handle(PlaceOrderCommand cmd, CancellationToken ct) { ... }
}

// Controller usage
var orderId = await _mediator.Send(new PlaceOrderCommand(customerId, items));
```

- The pairing of `IRequest<T>` and `IRequestHandler<TRequest, TResponse>` creates a compile-time contract: the handler's response type must match the request's declared result type, so type mismatches fail at compile time rather than at runtime.
- MediatR resolves the correct handler at runtime through the DI container, so adding a new command-handler pair requires only implementing the interfaces — no registration code beyond the initial `AddMediatR` scan.
- Commands that return nothing use `IRequest` (without a type parameter, equivalent to `IRequest<Unit>`) and `IRequestHandler<TRequest>` (the void variant).

---

## Q13. What is a pipeline behavior (`IPipelineBehavior<TRequest, TResponse>`) in MediatR, and name three common behaviors used in production?

What is a pipeline behavior (`IPipelineBehavior<TRequest, TResponse>`) in MediatR, and name three common behaviors used in production?

**Answer:** A pipeline behavior in MediatR is a middleware-like class that wraps every handler dispatch for matching request types, allowing code to run before and after the handler without modifying the handler itself; it implements `IPipelineBehavior<TRequest, TResponse>` and calls `next()` to invoke the next behavior or the handler in the chain. This is the CQRS equivalent of ASP.NET Core middleware — it is how cross-cutting concerns are applied uniformly across all commands and queries without scattering the logic across every handler.

- **Validation behavior** — runs FluentValidation validators registered in DI for the incoming request type and throws `ValidationException` if any rule fails, so handlers receive only structurally valid commands.
- **Logging behavior** — logs the request type, execution time, and any exceptions around every `Handle` call, providing uniform observability across all commands and queries without a single line of logging code in the handlers themselves.
- **Transaction behavior** — wraps command handlers (and only command handlers, filtered by a marker interface or generic constraint) in a database transaction, committing on success and rolling back on exception, so handlers do not need to manage transaction boundaries explicitly.
- Other common behaviors include: performance monitoring (alerting on slow handlers), retry logic for transient failures, and authorization checks that verify the current user has permission to execute a specific command type.

---

## Q14. What is the difference between MediatR's `IRequest` (request/response) and `INotification` (publish/subscribe), and when do you use each?

What is the difference between MediatR's `IRequest` (request/response) and `INotification` (publish/subscribe), and when do you use each?

**Answer:** `IRequest` implements a one-to-one request/response pattern where exactly one handler processes the message and returns a result, while `INotification` implements a one-to-many publish/subscribe pattern where any number of handlers can respond to the same event without any of them returning a result. In CQRS terms, commands and queries use `IRequest` because they target a specific handler with a known responsibility; domain events and integration events use `INotification` because multiple downstream parts of the system may need to react to the same thing.

- Use `IRequest` for commands ("place this order") and queries ("get this order") — one handler owns the operation, the caller expects a result or acknowledgement.
- Use `INotification` for domain events ("order was placed") — the handler for placing the order publishes the event, and separate handlers update the read model, send a confirmation email, and notify the warehouse, each independently.
- `INotification` handlers run sequentially by default in MediatR; for true parallel or isolated fan-out, you may need a proper message bus (RabbitMQ, Azure Service Bus) rather than MediatR's in-process notification.
- Mixing the two appropriately keeps handlers focused: the command handler does one thing (place the order), then publishes an event, and the downstream concerns stay in separate notification handlers rather than accumulating inside the command handler.

---

## Q15. What are the risks of overusing MediatR in a codebase, and when is it unnecessary complexity?

What are the risks of overusing MediatR in a codebase, and when is it unnecessary complexity?

**Answer:** MediatR adds a layer of indirection that makes tracing code harder: you cannot navigate from a controller call to its handler with a simple "go to definition" because the dispatch goes through a runtime type lookup. When applied to every operation in a small or simple application, this indirection multiplies without providing meaningful benefit — the codebase becomes a proliferation of command and query objects for operations that could be simple method calls on a well-structured service.

- MediatR is unnecessary when the application is a simple CRUD API with no complex domain logic, few cross-cutting concerns, and no need for the pipeline behavior mechanism — a direct service layer is simpler and fully navigable.
- A signal of overuse is a handler with a single line that calls directly through to a repository with no domain logic, validation, or behavior — the command/handler wrapping adds ceremony for no benefit.
- The pipeline behavior mechanism is MediatR's main value proposition; if your project has no shared cross-cutting concerns to centralize, you lose most of the reason to adopt it.
- A useful heuristic: if adding CQRS means your team spends more time navigating files and maintaining mapping objects than they save on extensibility, the pattern is adding overhead that the project's complexity does not justify.

---

## Chapter 5 — Read/Write Model Separation and Eventual Consistency

---

## Q16. When should the read and write databases be physically separated, and what infrastructure does that require?

When should the read and write databases be physically separated, and what infrastructure does that require?

**Answer:** The read and write databases should be physically separated when the read and write workloads have conflicting performance requirements — for example, when long-running analytical queries on the read side would lock rows needed by the write side, or when the query model needs a technology the write database does not support (full-text search, document storage, in-memory caching). Physical separation is a significant operational commitment and is only justified when a specific, measurable bottleneck demands it.

- Physical separation requires a synchronization mechanism: domain events published by the write side trigger read-model update handlers that project the new state into the read store, or change-data capture (CDC) from the write database's transaction log feeds the read store.
- The read store does not need to be a relational database — it could be Redis for key-value lookups, Elasticsearch for search, a SQL read replica for complex joins, or a pre-materialized Azure Cosmos DB container.
- Teams often adopt a progressive approach: start with a single database queried via `AsNoTracking()`, add a SQL read replica when query load grows, and only introduce a separate read-store technology when the replica's data model is insufficient.
- Monitoring is critical: the synchronization lag between write and read stores must be measured and alerted on; a stuck event handler can cause the read store to silently drift far behind the write store.

---

## Q17. What is eventual consistency in a CQRS system, and how do you handle the lag between a command completing and the read model being updated?

What is eventual consistency in a CQRS system, and how do you handle the lag between a command completing and the read model being updated?

**Answer:** Eventual consistency in a CQRS system means that after a command completes and its domain event is published, the read model reflects the new state only after the event handler has processed the event and updated the read store — there is a window (typically milliseconds to seconds) during which the read model is stale. This is not a bug but a deliberate design trade-off; the system prioritizes write availability and read performance over synchronous consistency.

- The most common user-visible consequence: a user submits a form, the command succeeds, they are redirected to a list page, and the new item is not yet there. The fix is either to optimistically add the item to the UI from local state before the read model updates, or to poll the read model with a brief retry.
- Commands can return the ID of the newly created resource, and the client can immediately `GET /api/orders/{newId}` — if the read model is not yet updated, the server returns 202 Accepted or a polling URL indicating the resource is being prepared.
- In high-stakes scenarios (financial systems), the command handler can query the write database directly for the result of its own transaction using the resource's ID, bypassing the read store for that one call.
- Designing UIs to embrace eventual consistency — showing "order submitted, processing…" states and using WebSockets or Server-Sent Events to push the final state — produces a better user experience than pretending the lag does not exist.

---

## Q18. What is a projection in Event-Sourced CQRS, and how does it differ from a simple query?

What is a projection in Event-Sourced CQRS, and how does it differ from a simple query?

**Answer:** A projection in Event-Sourced CQRS is a process that listens to a stream of domain events and builds or maintains a read model by applying each event in order; it is an active subscriber that transforms the event stream into a queryable structure, not a passive data retrieval step. A simple query, by contrast, reads data that already exists in a database — it does not transform or build anything, it only retrieves.

- A `PlaceOrderProjection` listens for `OrderPlacedEvent`, `OrderShippedEvent`, and `OrderCancelledEvent`, updating an `OrderSummary` read table with each event so that queries against that table are fast and pre-computed.
- Projections can be rebuilt from scratch at any time by replaying all events from the event store — this makes them resilient to schema changes: if you need a new field in the read model, add it to the projection logic and replay.
- A projection runs asynchronously from the command that produced the event; this is the source of eventual consistency — the read model reflects events up to the most recently processed one, which lags behind the current moment.
- Multiple projections can consume the same event stream independently, each maintaining a different read model optimized for a different query or user interface without any coupling between them.

---

## Chapter 6 — CQRS with Event Sourcing

---

## Q19. What is Event Sourcing, and why is it frequently paired with CQRS?

What is Event Sourcing, and why is it frequently paired with CQRS?

**Answer:** Event Sourcing is a persistence strategy in which the system stores every state change as an immutable event in an append-only log, rather than storing only the current state of an entity; the current state of any aggregate is derived by replaying its events from the beginning of the log. CQRS and Event Sourcing are frequently paired because CQRS's command side naturally produces state-changing operations that can be recorded as events, and the event stream is the ideal source from which read model projections are built for the query side.

- Without Event Sourcing, CQRS typically stores final state in the write database and publishes events as a side effect; with Event Sourcing, events are the primary storage — the write database is the event log itself.
- The combination gives a complete audit history for free: every change to every aggregate is recorded as a named, timestamped, immutable event — no separate audit table or trigger is needed.
- Event Sourcing adds significant complexity: aggregates must be reconstituted by replaying events, event schemas must be versioned carefully, and the event store must be append-only and durable.
- The pairing is not mandatory: CQRS works without Event Sourcing (the most common production configuration), and Event Sourcing can be used without CQRS — though the two genuinely complement each other's strengths.

---

## Q20. What is the difference between an event store and a traditional relational database, and what does that mean for reads and writes?

What is the difference between an event store and a traditional relational database, and what does that mean for reads and writes?

**Answer:** A traditional relational database stores the current state of entities in rows that are updated in-place — reading the database shows you what is true right now, and the history is lost unless you add audit tables or triggers. An event store is an append-only log of immutable events — no row is ever updated or deleted, and reading the database shows you every state transition in sequence; to know the current state you replay events or read a projection.

| Dimension | Relational Database (current state) | Event Store |
|---|---|---|
| Write operation | INSERT or UPDATE the current state row | APPEND a new event to the stream |
| Read for current state | SELECT the row directly | Replay events or query a projection |
| History available | Only if audit tables are added | Always, by design |
| Schema changes | Require migrations on existing data | Add new event types; old events are immutable |
| Query flexibility | Full SQL expressiveness | Events are opaque; reads go through projections |

- Writes to an event store are always fast appends with no locking contention on existing rows; writes to a relational store may involve row-level locks, index updates, and constraint checks.
- Reads from an event store for current state require either replaying events (slow for long-lived aggregates without snapshots) or querying a projection (fast, but eventually consistent).

---

## Q21. What is a snapshot in Event Sourcing, and why is it needed?

What is a snapshot in Event Sourcing, and why is it needed?

**Answer:** A snapshot in Event Sourcing is a point-in-time copy of an aggregate's current state, stored alongside the event stream so that the aggregate can be reconstituted by loading the most recent snapshot and replaying only the events that occurred after it, rather than replaying the entire event history from the beginning. Snapshots become necessary when an aggregate accumulates enough events that replaying all of them on every command becomes too slow for the system's latency requirements.

- Without snapshots, an `Order` aggregate that has had 10,000 state-change events (common for long-running, high-throughput entities) would require loading and replaying all 10,000 events to process a single new command — an unacceptable latency in most systems.
- A snapshot is typically taken every N events (e.g., every 100 events) or when the event count exceeds a threshold; the system stores the snapshot separately and records which event version it represents.
- Snapshots are a performance optimization, not a correctness requirement — the system must still be correct without them; they can be deleted and regenerated at any time by replaying the full event stream.
- The snapshot format must be versioned separately from the event format, since the aggregate's state shape may change as the domain evolves, requiring snapshot migration strategies similar to database schema migrations.

---

## Chapter 7 — Testing and Design Trade-offs

---

## Q22. What makes CQRS command and query handlers easier to unit test than a traditional thick service layer?

What makes CQRS command and query handlers easier to unit test than a traditional thick service layer?

**Answer:** CQRS handlers are easier to unit test because each handler has a single, narrow responsibility with a clearly defined input (the command or query object) and a clearly defined output (the result type), and all dependencies are explicitly declared in the constructor — there is no inherited state from a base class, no static method calls, and no shared mutable context. A thick service layer often accumulates many methods with overlapping dependencies, making it difficult to instantiate for a single test case without satisfying all the service's unrelated dependencies.

- A command handler for `PlaceOrderCommand` depends only on `IOrderRepository` and `IDomainEventPublisher`; a test can mock exactly those two dependencies and test the handler in complete isolation.
- Because commands and queries are plain C# record or class objects, constructing test inputs is straightforward and requires no HTTP context, model binders, or controller infrastructure.
- The pipeline behavior separation means validation logic, transaction handling, and logging are tested independently of the handler — a `ValidationBehavior` test only needs a validator and a mock `next` delegate, not an entire handler.
- The explicit separation of read and write paths means query handler tests can verify that `AsNoTracking()` is used and no state change occurs, while command handler tests can verify that the correct aggregate method was called and the repository's `SaveAsync` was invoked.

---

## Q23. When should you NOT use CQRS, and what signals indicate that a simpler CRUD design is more appropriate?

When should you NOT use CQRS, and what signals indicate that a simpler CRUD design is more appropriate?

**Answer:** CQRS should not be used when the application's domain is simple, the read and write shapes are nearly identical, and the team size and codebase scale do not justify the additional abstractions; applying CQRS to a straightforward CRUD system multiplies the number of files, indirections, and concepts without providing any of the scalability or domain modeling benefits that make it valuable. The pattern is a solution to specific problems — complex domain logic, divergent read/write performance requirements, team boundaries around write vs. read side — and it creates unnecessary complexity when those problems are absent.

- A strong signal that CQRS is overkill: every command handler consists of a single call to a repository save method with no domain logic, and every query handler is a direct database read with no transformation — the pattern is providing ceremony without benefit.
- Another signal: the read model is exactly the same shape as the domain model, so there is no value in maintaining two separate structures.
- Small teams and internal tools where simplicity and development speed matter more than long-term scalability are better served by a straightforward layered architecture with services and repositories.
- CQRS is most valuable when: the domain is rich and complex, different teams own the read and write sides, query and write throughput differ by orders of magnitude, or the system needs an event-driven audit trail — these are the conditions the pattern was designed to address.

---

## Q24. How do you avoid "handler explosion" — the proliferation of tiny single-use handlers that fragment business logic across the codebase?

How do you avoid "handler explosion" — the proliferation of tiny single-use handlers that fragment business logic across the codebase?

**Answer:** Handler explosion occurs when developers apply CQRS mechanically by creating a new command/query pair for every operation without considering whether operations belong together, resulting in dozens of nearly identical handlers that are harder to understand than the service layer they replaced. The remedy is to design commands and queries around meaningful business operations rather than technical operations, and to let handlers be as rich as the use case demands.

- Commands should represent a complete business operation, not a field update: `UpdateCustomerAddressCommand` is one command that updates all address fields atomically, not five separate commands for street, city, state, zip, and country.
- If two operations always happen together as part of one business transaction, they likely belong in a single command handler — splitting them into two separate commands that must be called in sequence by the controller leaks orchestration logic into the HTTP layer.
- Shared logic between handlers belongs in domain objects (aggregates, value objects, domain services), not duplicated across handlers — a handler is an orchestrator, not a place for reusable logic.
- A practical guideline: the number of handlers should grow roughly in proportion to the number of meaningful user-facing actions in the system, not in proportion to the number of database columns or entity properties.

---
