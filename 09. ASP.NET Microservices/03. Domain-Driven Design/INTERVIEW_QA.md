# Domain-Driven Design — Interview Q&A
> 32 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Domain-Driven Design (DDD) and what problem does it solve?](#q1-what-is-domain-driven-design-ddd-and-what-problem-does-it-solve)
2. [Q2. What is the difference between strategic DDD and tactical DDD?](#q2-what-is-the-difference-between-strategic-ddd-and-tactical-ddd)
3. [Q3. What is a domain, a subdomain, and what are the three types of subdomains (core, supporting, generic)?](#q3-what-is-a-domain-a-subdomain-and-what-are-the-three-types-of-subdomains-core-supporting-generic)
4. [Q4. What is ubiquitous language and why is it important in DDD?](#q4-what-is-ubiquitous-language-and-why-is-it-important-in-ddd)
5. [Q5. What is a bounded context and why is it the cornerstone of strategic DDD?](#q5-what-is-a-bounded-context-and-why-is-it-the-cornerstone-of-strategic-ddd)
6. [Q6. What is context mapping in DDD and why does it matter?](#q6-what-is-context-mapping-in-ddd-and-why-does-it-matter)
7. [Q7. What are the most important context-mapping relationship patterns, and what does each one mean?](#q7-what-are-the-most-important-context-mapping-relationship-patterns-and-what-does-each-one-mean)
8. [Q8. What is an Anti-Corruption Layer (ACL) and when should you use one?](#q8-what-is-an-anti-corruption-layer-acl-and-when-should-you-use-one)
9. [Q9. What is an Open Host Service (OHS) and a Published Language, and how do they enable integration?](#q9-what-is-an-open-host-service-ohs-and-a-published-language-and-how-do-they-enable-integration)
10. [Q10. What is the difference between an upstream context and a downstream context?](#q10-what-is-the-difference-between-an-upstream-context-and-a-downstream-context)
11. [Q11. What is an Entity in DDD and how does it differ from a Value Object?](#q11-what-is-an-entity-in-ddd-and-how-does-it-differ-from-a-value-object)
12. [Q12. What is a Value Object? What makes it immutable, and when should you prefer it over an Entity?](#q12-what-is-a-value-object-what-makes-it-immutable-and-when-should-you-prefer-it-over-an-entity)
13. [Q13. How do you implement Entity equality and Value Object equality in C#?](#q13-how-do-you-implement-entity-equality-and-value-object-equality-in-c)
14. [Q14. What is identity in DDD — should you use database-generated IDs or domain-generated IDs, and why?](#q14-what-is-identity-in-ddd-should-you-use-database-generated-ids-or-domain-generated-ids-and-why)
15. [Q15. What is an Aggregate and what is the Aggregate Root?](#q15-what-is-an-aggregate-and-what-is-the-aggregate-root)
16. [Q16. What invariants do Aggregates enforce, and why must all modifications go through the Aggregate Root?](#q16-what-invariants-do-aggregates-enforce-and-why-must-all-modifications-go-through-the-aggregate-root)
17. [Q17. What is the one-transaction-one-aggregate rule, and what does it mean for eventual consistency?](#q17-what-is-the-one-transaction-one-aggregate-rule-and-what-does-it-mean-for-eventual-consistency)
18. [Q18. How do you decide the size and boundaries of an Aggregate? What are the signs of a God Aggregate?](#q18-how-do-you-decide-the-size-and-boundaries-of-an-aggregate-what-are-the-signs-of-a-god-aggregate)
19. [Q19. How should Aggregates reference each other — by object reference or by ID?](#q19-how-should-aggregates-reference-each-other-by-object-reference-or-by-id)
20. [Q20. What is a Domain Event in DDD and why is it important?](#q20-what-is-a-domain-event-in-ddd-and-why-is-it-important)
21. [Q21. What is the difference between a Domain Event and an Integration Event?](#q21-what-is-the-difference-between-a-domain-event-and-an-integration-event)
22. [Q22. How do you dispatch Domain Events in a way that keeps the domain model clean? What is the Outbox pattern?](#q22-how-do-you-dispatch-domain-events-in-a-way-that-keeps-the-domain-model-clean-what-is-the-outbox-pattern)
23. [Q23. What is a Domain Service and how does it differ from an Application Service?](#q23-what-is-a-domain-service-and-how-does-it-differ-from-an-application-service)
24. [Q24. What is a Repository in DDD and how does it differ from a data-access layer?](#q24-what-is-a-repository-in-ddd-and-how-does-it-differ-from-a-data-access-layer)
25. [Q25. What is a Factory in DDD, and when should you use one instead of a constructor?](#q25-what-is-a-factory-in-ddd-and-when-should-you-use-one-instead-of-a-constructor)
26. [Q26. What is an anemic domain model and why is it considered an anti-pattern?](#q26-what-is-an-anemic-domain-model-and-why-is-it-considered-an-anti-pattern)
27. [Q27. What are the standard layers in a DDD-based application, and what are the dependency rules between them?](#q27-what-are-the-standard-layers-in-a-ddd-based-application-and-what-are-the-dependency-rules-between-them)
28. [Q28. Why should the domain model have no knowledge of infrastructure concerns like databases or HTTP clients?](#q28-why-should-the-domain-model-have-no-knowledge-of-infrastructure-concerns-like-databases-or-http-clients)
29. [Q29. How do bounded contexts map to microservices, and what are the risks of mapping them one-to-one?](#q29-how-do-bounded-contexts-map-to-microservices-and-what-are-the-risks-of-mapping-them-one-to-one)
30. [Q30. How does DDD integrate with Command Query Responsibility Segregation (CQRS)?](#q30-how-does-ddd-integrate-with-command-query-responsibility-segregation-cqrs)
31. [Q31. What is the Saga pattern and how does it relate to DDD's aggregate boundaries and eventual consistency?](#q31-what-is-the-saga-pattern-and-how-does-it-relate-to-ddds-aggregate-boundaries-and-eventual-consistency)
32. [Q32. What are the most common mistakes developers make when applying DDD in practice?](#q32-what-are-the-most-common-mistakes-developers-make-when-applying-ddd-in-practice)

---

## Q1. What is Domain-Driven Design (DDD) and what problem does it solve?

What is Domain-Driven Design (DDD) and what problem does it solve?

**Answer:** Domain-Driven Design (DDD) is an approach to software development that places the business domain — the real-world problem the software solves — at the center of all design decisions. Eric Evans introduced DDD in his 2003 book to fix a recurring problem: large codebases become unmaintainable because developers use their own technical vocabulary while business experts use another, so the code never truly reflects how the business works.

- DDD insists that developers and domain experts share a single, precise vocabulary called the ubiquitous language, ensuring that the code reads like the business concepts it models rather than a layer of technical abstractions that must be mentally translated.
- The approach is especially valuable for complex domains — order management, insurance underwriting, logistics — where business rules are intricate and frequently change; it is less justified for simple CRUD applications where the overhead outweighs the benefit.
- DDD is split into two halves: strategic design (how to carve up a large domain into manageable, coherent pieces) and tactical design (how to model the concepts inside each piece using building blocks like Entities, Value Objects, and Aggregates).
- The primary output of strategic DDD is a context map — a diagram of how different parts of the system relate and communicate — which serves as a shared architectural blueprint for both technical and business stakeholders.

---

## Q2. What is the difference between strategic DDD and tactical DDD?

What is the difference between strategic DDD and tactical DDD?

**Answer:** Strategic DDD deals with the large-scale structure of the system — how to divide a complex domain into smaller, coherent areas and how those areas communicate. Tactical DDD provides a set of object-model building blocks used inside each area to accurately capture business rules in code.

| Aspect | Strategic DDD | Tactical DDD |
|---|---|---|
| Scope | Whole system / organisation | Single bounded context |
| Concern | Boundaries and relationships | Object model internals |
| Output | Context map, bounded contexts | Entities, Value Objects, Aggregates, Domain Events |
| Who is involved | Architects, domain experts, developers | Developers modelling one domain |
| When applied | Early design and architecture | Implementation phase |

- Strategic design is language-first: the key activity is agreeing on boundaries and vocabulary with business stakeholders before writing any code.
- Tactical design patterns only make sense inside a well-defined bounded context; applying tactical patterns without strategic clarity produces a tangled model that tries to serve too many purposes at once.

---

## Q3. What is a domain, a subdomain, and what are the three types of subdomains (core, supporting, generic)?

What is a domain, a subdomain, and what are the three types of subdomains?

**Answer:** The domain is the entire subject area the software is built around — for example, "e-commerce" or "healthcare claims processing." Because a domain is too large to model uniformly, DDD breaks it into subdomains, each representing a coherent area of business concern. Subdomains come in three types based on their strategic importance.

- A **core domain** is where the business derives its competitive advantage, the area that must be modelled with the most care and investment. For an e-commerce company the recommendation engine or the dynamic pricing engine might be the core domain.
- A **supporting domain** enables the core domain but is not a competitive differentiator — for example, inventory management or notification delivery. It warrants custom code because off-the-shelf solutions do not fit exactly, but it does not need the same depth of modelling as the core.
- A **generic domain** solves a well-understood problem that has no strategic value in being unique — authentication, billing, or email sending. Generic domains are almost always best handled by buying or adopting an existing product rather than building from scratch.
- Recognising subdomain types guides investment decisions: spend your best engineers on the core domain and use commodity solutions everywhere else.

---

## Q4. What is ubiquitous language and why is it important in DDD?

What is ubiquitous language and why is it important in DDD?

**Answer:** Ubiquitous language is a shared, precise vocabulary for a bounded context that is used consistently by every member of the team — developers, domain experts, product managers — in conversation, documentation, and in the code itself. The word "ubiquitous" means "found everywhere" — the same terms appear in class names, method names, tests, and user stories with no translation layer between them.

- When developers use different words than business experts — for example, calling a "policy" an "insurance contract" in code while the business calls it a "policy" — every conversation requires mental translation, and misunderstandings compound into bugs.
- Ubiquitous language is bounded to a context: the word "Account" can mean a bank account in the payments context and a user profile in the identity context, and both are correct within their own boundary.
- Building the ubiquitous language is an active collaboration: domain experts correct the model when a term does not match business reality, and developers push back when business terms are ambiguous, resulting in a richer shared understanding on both sides.
- The code becomes self-documenting when it uses the ubiquitous language — a method named `policy.Lapse()` is immediately understandable to an insurance domain expert without reading the implementation.

---

## Q5. What is a bounded context and why is it the cornerstone of strategic DDD?

What is a bounded context and why is it the cornerstone of strategic DDD?

**Answer:** A bounded context is an explicit boundary within which a particular domain model applies and a particular ubiquitous language is used consistently. Inside the boundary, every term has one meaning; outside the boundary, the same word may mean something entirely different. Bounded contexts are the primary tool for managing complexity in large systems.

- Without explicit boundaries, a single model is forced to satisfy multiple conflicting purposes — a "Customer" that means a shopping cart owner in one part of the system and a billing account in another will accrue compromises until the model satisfies nobody cleanly.
- A bounded context is not the same as a microservice, although they often map onto each other. A bounded context is a conceptual boundary that can be implemented as a single service, multiple services, or even a module inside a monolith.
- The boundary is enforced technically — separate code base or module, separate database schema, explicit API contract between contexts — so that changes inside one context cannot break another context's model.
- One of the most valuable activities in DDD is drawing the boundaries correctly: too broad and the model becomes incoherent; too narrow and you end up with trivial services that spend most of their time calling each other.

---

## Chapter 2 — Context Mapping & Integration Patterns

---

## Q6. What is context mapping in DDD and why does it matter?

What is context mapping in DDD and why does it matter?

**Answer:** Context mapping is the practice of identifying all bounded contexts in a system and documenting how they relate to and communicate with each other, producing a diagram called a context map. The context map exposes who owns what, who depends on whom, and how information flows across boundaries — making it the most important architectural artefact in a DDD project.

- Without a context map, team dependencies are invisible: one team unknowingly breaks another by changing a shared model, or teams duplicate effort because they did not know another context already solved the same problem.
- The context map records not just technical connections but also organisational relationships — which team is upstream (producer) and which is downstream (consumer), and what the power dynamic is between them, since those political realities shape what integration patterns are feasible.
- A context map is a living document that evolves as the architecture evolves; the act of drawing it forces teams to articulate and negotiate integration contracts they otherwise leave implicit.

---

## Q7. What are the most important context-mapping relationship patterns, and what does each one mean?

What are the most important context-mapping relationship patterns, and what does each one mean?

**Answer:** DDD defines a set of relationship patterns that characterise how two bounded contexts integrate. Each pattern captures not just the technical connection but also the team dynamic and level of trust between the two sides.

| Pattern | Description |
|---|---|
| Partnership | Two teams align closely and coordinate changes; both win or fail together. |
| Shared Kernel | Two contexts share a small, agreed-upon subset of the model; changes need mutual consent. |
| Customer-Supplier | Upstream team supplies a model that downstream team consumes; downstream has negotiating power. |
| Conformist | Downstream accepts the upstream model as-is with no say in its design. |
| Anti-Corruption Layer (ACL) | Downstream translates the upstream model into its own language to stay isolated from upstream changes. |
| Open Host Service (OHS) | Upstream publishes a well-defined protocol others can integrate with without special negotiation. |
| Published Language | Upstream defines a shared, public data format (e.g., JSON schema, Protobuf) that all consumers use. |
| Separate Ways | Contexts have no integration at all; each solves its own problem independently. |

- The Conformist and ACL patterns both occur when one team has no influence over the upstream model, but they differ in impact: Conformist is cheaper (no translation code) but bleeds upstream concepts into the downstream model, while ACL is more expensive but keeps the downstream model pure.
- Choosing the wrong pattern often leads to tight coupling that makes future changes painful; the context map makes these trade-offs explicit and debatable.

---

## Q8. What is an Anti-Corruption Layer (ACL) and when should you use one?

What is an Anti-Corruption Layer (ACL) and when should you use one?

**Answer:** An Anti-Corruption Layer is a translation layer that sits at the boundary of a downstream bounded context and converts data and concepts from an upstream context — or a legacy system — into the downstream context's own model. The name reflects its purpose: protecting the downstream model from being "corrupted" by alien concepts or legacy terminology that do not belong in its language.

- An ACL is most valuable when integrating with a legacy system whose model is messy, poorly documented, or uses different terminology — without the ACL, legacy concepts would leak throughout the downstream code and make it harder to understand and change.
- The ACL typically involves adapters, translators, or façades that map incoming DTOs (Data Transfer Objects) from the upstream model to domain objects in the downstream model, keeping the domain model free of cross-context dependencies.
- The cost of an ACL is the translation code itself, which must be maintained whenever the upstream interface changes; this cost is justified when the upstream model is volatile or conceptually incompatible with the downstream model.
- In microservices, an ACL is commonly implemented as a set of mapping classes in the subscribing service that convert events or API responses from another service into domain events or commands that the local domain understands.

---

## Q9. What is an Open Host Service (OHS) and a Published Language, and how do they enable integration?

What is an Open Host Service and a Published Language, and how do they enable integration?

**Answer:** An Open Host Service (OHS) is a pattern where an upstream context defines a clean, stable service interface — a published API — that any downstream context can integrate with without negotiating custom arrangements. A Published Language extends this by defining a precise, shared data format (such as a JSON schema, Avro schema, or Protocol Buffers definition) that all consumers use, making integration explicit and toolable.

- OHS shifts the integration burden to the upstream: instead of every consumer adapting to whatever the upstream produces, the upstream takes responsibility for offering a well-designed, backwards-compatible interface.
- Published Language is common in event-driven architectures where a service publishes events to a message bus; the event schema is the "published language" that consumers must understand, and schema registries enforce its versioning.
- Together, OHS and Published Language reduce the need for Anti-Corruption Layers in downstream contexts because the upstream model is already designed for consumption rather than being an arbitrary internal representation.

---

## Q10. What is the difference between an upstream context and a downstream context?

What is the difference between an upstream context and a downstream context?

**Answer:** In a context-mapping relationship, the upstream context produces or defines the model that flows to another, while the downstream context consumes or depends on that model. The upstream team has more influence: their decisions about how to model something directly affect what the downstream team must deal with, regardless of whether the downstream team agrees.

- Changes in an upstream context ripple downstream — a field renamed or removed in the upstream API breaks downstream consumers — which is why the direction of dependency matters for managing change and risk.
- The Customer-Supplier pattern addresses the power imbalance: the downstream team acts as a "customer" with requirements, and the upstream team acts as a "supplier" who incorporates those requirements into their roadmap, giving the downstream team some influence.
- In practice, "upstream" and "downstream" are not always technically obvious; they reflect ownership and change authority, and identifying them prevents silent assumptions about who is responsible for compatibility.

---

## Chapter 3 — Entities, Value Objects & Identity

---

## Q11. What is an Entity in DDD and how does it differ from a Value Object?

What is an Entity in DDD and how does it differ from a Value Object?

**Answer:** An Entity is a domain object that has a unique identity that persists through the lifetime of the object, even as its attributes change. Two Entities are the same if and only if they share the same identity — two `Order` objects with the same `OrderId` represent the same order regardless of any other attribute. A Value Object, by contrast, has no identity; two Value Objects are equal if all their attributes are equal.

| Aspect | Entity | Value Object |
|---|---|---|
| Identity | Has a unique ID | No identity; equality by value |
| Mutability | Usually mutable | Always immutable |
| Examples | Customer, Order, Product | Money, Address, DateRange |
| Lifetime | Long-lived, persisted | Short-lived, often replaced |

- The choice matters for design: Entities are tracked and managed over time, while Value Objects are replaced wholesale when they change — instead of mutating an `Address`, you assign a new `Address` instance.
- Many concepts that seem like Entities are better modelled as Value Objects; over-using Entities leads to complex lifecycle management and unnecessary identity tracking.

---

## Q12. What is a Value Object? What makes it immutable, and when should you prefer it over an Entity?

What is a Value Object? What makes it immutable, and when should you prefer it over an Entity?

**Answer:** A Value Object is a domain concept that is fully described by its attributes, has no identity of its own, and is always immutable — once created, its values never change. `Money(100, "USD")` and another `Money(100, "USD")` are interchangeable; there is no meaningful distinction between them beyond their values.

- Immutability is the key design property: a Value Object is never modified — instead, operations on it produce a new instance, similar to how `string` works in C#. This eliminates a whole class of bugs caused by shared mutable state.
- Prefer a Value Object when the concept is defined entirely by its measurements or attributes and you do not need to track it independently — postal codes, GPS coordinates, temperature ranges, and email addresses are natural Value Objects.
- Value Objects can encapsulate validation and business rules, preventing invalid states from existing — a `Money` Value Object can enforce that the amount is non-negative in its constructor, rather than having that check scattered across the codebase.
- In C# 10+, `record struct` or `record` types are ideal for Value Objects because they provide structural equality, immutability by convention, and concise syntax.

---

## Q13. How do you implement Entity equality and Value Object equality in C#?

How do you implement Entity equality and Value Object equality in C#?

**Answer:** Entities compare by identity: two Entity instances are equal if they have the same ID, regardless of their other properties. Value Objects compare structurally: two instances are equal if every attribute is equal. Getting these implementations right prevents subtle bugs where two objects that represent the same thing compare as different.

For a Value Object using a C# record:
```csharp
public record Money(decimal Amount, string Currency);
// Structural equality is built in — no extra code needed
```

For an Entity base class:
```csharp
public abstract class Entity<TId>
{
    public TId Id { get; protected set; }
    public override bool Equals(object obj) =>
        obj is Entity<TId> other && Id!.Equals(other.Id);
    public override int GetHashCode() => Id!.GetHashCode();
}
```

- Using `record` for Value Objects is the modern approach in .NET; the compiler generates `Equals`, `GetHashCode`, and `==` based on all properties, which is exactly the structural equality semantics needed.
- Entity equality must be based only on ID; never include mutable properties like name or status in `Equals`, because two Entities with the same ID but different states are still the same Entity.
- Transient Entities (not yet persisted, ID is default value) require special handling — two new Entities should not be considered equal simply because both have a default ID.

---

## Q14. What is identity in DDD — should you use database-generated IDs or domain-generated IDs, and why?

What is identity in DDD — should you use database-generated IDs or domain-generated IDs, and why?

**Answer:** Identity in DDD means the attribute that uniquely identifies an Entity and distinguishes it from all other instances of the same type over its lifetime. The question of who generates that identity — the domain or the database — has significant architectural implications for DDD.

- Database-generated identity (auto-increment integers, database sequences) means the Entity does not have an ID until it is persisted, which breaks the DDD principle that an Entity is a complete, valid object before it is saved — you cannot dispatch Domain Events referencing an Entity's ID before it hits the database.
- Domain-generated identity using GUIDs (Globally Unique Identifiers) or strongly-typed IDs (`OrderId`, `CustomerId`) solves this: the identity is assigned at object creation, so the Entity is fully formed and can participate in business logic, events, and unit tests without a database round-trip.
- Strongly typed IDs (wrapping a `Guid` in a value object like `OrderId`) prevent primitive obsession bugs where an `orderId` Guid is accidentally passed where a `customerId` Guid is expected — the compiler catches the mistake.
- Sequential GUIDs (using `Guid.CreateVersion7()` in .NET 9+) combine the benefits of domain-generated IDs with database index friendliness, avoiding the fragmentation that standard random GUIDs cause on clustered indexes.

---

## Chapter 4 — Aggregates & Invariants

---

## Q15. What is an Aggregate and what is the Aggregate Root?

What is an Aggregate and what is the Aggregate Root?

**Answer:** An Aggregate is a cluster of related Entities and Value Objects that are treated as a single unit for the purpose of data changes, with one Entity designated as the Aggregate Root. The Aggregate Root is the only entry point through which the outside world can interact with the cluster; no external object may hold a direct reference to an inner Entity.

- The Aggregate enforces all business invariants — consistency rules that must always hold — within its boundary. For example, an `Order` Aggregate ensures that the total of all `OrderLine` items always matches the stored total, and that an order in "Shipped" status cannot have items added to it.
- The Aggregate Root controls the lifecycle of all objects inside it: inner objects are created, modified, and deleted only through methods on the root, never by external code reaching in and manipulating them directly.
- A classic example: an `Order` is the Aggregate Root that contains a collection of `OrderLine` Entities and a `ShippingAddress` Value Object. External code calls `order.AddItem(product, quantity)` rather than manipulating the `OrderLines` collection directly.
- Aggregates define the transactional boundary: everything inside an Aggregate is saved or rolled back together in a single database transaction, while consistency across Aggregate boundaries is achieved through eventual consistency and Domain Events.

---

## Q16. What invariants do Aggregates enforce, and why must all modifications go through the Aggregate Root?

What invariants do Aggregates enforce, and why must all modifications go through the Aggregate Root?

**Answer:** An invariant is a business rule that must always be true — a consistency constraint that cannot be violated at any point in time. Aggregates enforce invariants by controlling all write access to their state; since every mutation goes through the Aggregate Root's methods, the root can check the invariant after every change and throw a domain exception if it would be violated.

- If external code could modify inner Entities directly, the Aggregate would have no way to intercept the change and verify the invariant — the invariant would need to be checked externally, scattered across application code where it is easily forgotten.
- Example: a `BankAccount` Aggregate has an invariant that the balance may not go below zero for a standard account. The method `account.Withdraw(amount)` checks the balance rule before updating state; if the check were skipped, any caller could directly reduce the balance field and break the invariant.
- DDD calls this the "tell, don't ask" principle — callers tell the Aggregate Root what they want to do, and the root decides whether it is allowed and how to do it, rather than callers reading state, making their own decisions, and writing back.
- In C#, this is enforced by making inner collection properties private or using read-only collections (`IReadOnlyList<OrderLine>`) exposed for reading, with all mutation going through root methods.

---

## Q17. What is the one-transaction-one-aggregate rule, and what does it mean for eventual consistency?

What is the one-transaction-one-aggregate rule, and what does it mean for eventual consistency?

**Answer:** The one-transaction-one-aggregate rule states that a single database transaction should modify only one Aggregate. If a business operation needs to change two Aggregates, those changes must happen in separate transactions, with the second change triggered by a Domain Event that the first Aggregate publishes. This rule enforces that the Aggregate boundary is also the consistency boundary.

- The rule exists because trying to update two Aggregates in one transaction means taking distributed locks across multiple rows or tables, reducing concurrency and creating coupling between Aggregates — if one fails, the other rolls back too, tying their lifecycles together.
- Updating two Aggregates in separate transactions means there is a period of time between the first and second transaction where the system is in an intermediate state — this is called eventual consistency, and the domain must be designed to tolerate it.
- Not all operations can tolerate eventual consistency; when strong consistency between two concepts is truly required, it usually means those concepts belong in the same Aggregate, not in separate ones — the rule also serves as a design signal about aggregate boundaries.
- The Outbox pattern is commonly used to guarantee that the Domain Event is reliably published even if the process crashes between transactions, ensuring the second Aggregate eventually receives the event.

---

## Q18. How do you decide the size and boundaries of an Aggregate? What are the signs of a God Aggregate?

How do you decide the size and boundaries of an Aggregate? What are the signs of a God Aggregate?

**Answer:** Aggregate boundaries should be as small as possible while still being able to enforce all business invariants. The guiding principle is to include in an Aggregate only the objects that must change together to maintain a consistency rule — everything else belongs outside the boundary.

- Start by asking: "Can this invariant be violated if I change A without also changing B?" If yes, A and B belong in the same Aggregate. If no, they can be separate Aggregates that reference each other by ID.
- A God Aggregate is one that has grown to include every loosely related concept — an `Order` that contains `Customer`, `Product`, `Inventory`, and `Payment` all in one cluster. It becomes the entire application in a single object, requires large transactions, and serialises all concurrent writes.
- Signs of a God Aggregate: the Aggregate has dozens of nested objects, loading it requires fetching hundreds of rows, multiple concurrent users always contend on the same instance, or it contains objects that rarely or never change together.
- The fix is to split along natural consistency boundaries and use Domain Events to propagate changes between the new smaller Aggregates — for example, `Order` references `CustomerId` (a value), not the full `Customer` Aggregate.

---

## Q19. How should Aggregates reference each other — by object reference or by ID?

How should Aggregates reference each other — by object reference or by ID?

**Answer:** Aggregates should reference other Aggregates by identity (an ID value) rather than by object reference. This enforces the transactional boundary — holding an object reference invites code to navigate into a foreign Aggregate and mutate it, blurring the boundary and enabling cross-aggregate consistency violations.

- If `Order` held a direct reference to the `Customer` object, it would be tempting (and possible) to call `order.Customer.ChangeName("New")` from outside the `Customer` Aggregate's boundary, bypassing any invariants the `Customer` enforces.
- Referencing by ID means: when you need data from another Aggregate during a use case, you load it separately through its own Repository, apply changes through its own root methods, and save it in its own transaction.
- In Entity Framework Core this is implemented by not including navigation properties to foreign Aggregate roots — the `Order` entity has a `CustomerId` property of type `Guid` or a strongly typed `CustomerId`, but no `public Customer Customer` navigation property.
- This also improves performance by preventing accidental eager loading of an entire Aggregate graph when only a subset is needed.

---

## Chapter 5 — Domain Events & Integration Events

---

## Q20. What is a Domain Event in DDD and why is it important?

What is a Domain Event in DDD and why is it important?

**Answer:** A Domain Event is an immutable record of something significant that happened within the domain — something a business expert would care about — expressed in the past tense, such as `OrderPlaced`, `PaymentReceived`, or `CustomerRegistered`. Domain Events are the mechanism by which parts of the domain react to changes without being directly coupled to each other.

- Domain Events make side-effects explicit: instead of an `Order` service directly calling an inventory service and a notification service after placing an order, the `Order` Aggregate publishes `OrderPlaced`, and other parts of the system subscribe to it and take their own actions independently.
- This decoupling allows each subscriber to evolve independently — adding a new consequence (send a confirmation email) means adding a new handler, not modifying the `Order` Aggregate or the `PlaceOrderCommandHandler`.
- Domain Events also serve as a design tool: if a domain expert says "when X happens, we need to do Y and Z," those side-effects are natural candidates for event handlers, and the phrasing itself suggests the event name.
- Within a single bounded context, Domain Events are often dispatched in-process using a mediator (MediatR in .NET). Across bounded contexts they typically become Integration Events transmitted over a message bus.

---

## Q21. What is the difference between a Domain Event and an Integration Event?

What is the difference between a Domain Event and an Integration Event?

**Answer:** A Domain Event is an in-process notification that something happened within a single bounded context; it uses the domain's own language and object model. An Integration Event is a message published to a message bus that crosses bounded context (or service) boundaries; it uses a serialisable, versionable contract that other services can consume without knowing about the originating domain model.

| Aspect | Domain Event | Integration Event |
|---|---|---|
| Scope | Inside one bounded context | Across bounded contexts or services |
| Transport | In-process (mediator, event dispatcher) | Message bus (RabbitMQ, Azure Service Bus, Kafka) |
| Language | Uses domain types (Entities, Value Objects) | Uses serialisable DTOs (JSON, Protobuf) |
| Versioning | Internal — no external consumers | Requires versioning strategy for backward compatibility |
| Examples | `OrderPlaced` (inside Order context) | `OrderConfirmedIntegrationEvent` (published to bus) |

- The mapping between the two is intentional: a Domain Event handler may translate a `OrderPlaced` domain event into an `OrderConfirmedIntegrationEvent` DTO and publish it to the bus, decoupling the internal model from the external contract.
- Integration Events must be designed with stability in mind — adding fields is safe, removing or renaming fields is a breaking change that requires coordination with all consumers.

---

## Q22. How do you dispatch Domain Events in a way that keeps the domain model clean? What is the Outbox pattern?

How do you dispatch Domain Events in a way that keeps the domain model clean? What is the Outbox pattern?

**Answer:** The cleanest approach is to have the Aggregate Root collect Domain Events during a business operation and store them on the object (not publish them directly), then dispatch them after the transaction completes — keeping the domain model free of infrastructure dependencies like message bus clients. The Outbox pattern solves the "what if the process crashes after saving but before publishing the event" problem.

- The Aggregate Root exposes a collection like `IReadOnlyList<IDomainEvent> DomainEvents` which accumulates events during operations. After `dbContext.SaveChangesAsync()` succeeds, the application layer iterates these events and dispatches them via a mediator or event dispatcher.
- The risk: if the application crashes between `SaveChanges` (database committed) and event dispatch (not yet published), the event is lost and downstream subscribers never react — this breaks eventual consistency.
- The Outbox pattern addresses this: Domain Events are written to an "outbox" table in the same database transaction as the Aggregate changes. A background worker (using Quartz.NET or a hosted service) polls the outbox table and publishes any unpublished events to the message bus, marking them as published. Even if the process crashes, the next restart picks up the unpublished events.
- This guarantees at-least-once delivery: the event will definitely be published at some point, so consumers must be idempotent — able to handle the same event arriving more than once without duplicating side-effects.

---

## Chapter 6 — Domain Services, Application Services & Repositories

---

## Q23. What is a Domain Service and how does it differ from an Application Service?

What is a Domain Service and how does it differ from an Application Service?

**Answer:** A Domain Service is an operation that belongs in the domain layer because it expresses a significant business rule or process, but it does not naturally belong to any single Entity or Value Object — typically because it operates on multiple Aggregates or requires domain logic that spans objects. An Application Service orchestrates the use case — it loads Aggregates, calls Domain Services and Aggregate methods, and then saves results — but it contains no business logic itself.

| Aspect | Domain Service | Application Service |
|---|---|---|
| Layer | Domain layer | Application layer |
| Contains | Business logic | Orchestration only |
| Knowledge of domain | Yes — uses domain types | Yes — calls domain types |
| Knowledge of infrastructure | No | Minimal (through interfaces) |
| Examples | `TransferFundsService` | `TransferFundsCommandHandler` |

- A rule of thumb: if deleting the class would remove business logic, it is a Domain Service. If deleting it would only remove orchestration steps, it is an Application Service.
- Application Services are the entry point called by controllers or message handlers; they handle cross-cutting concerns like transaction management, logging, and authorisation, delegating all real decisions to the domain.
- Overloading Application Services with business logic is a code smell called the "fat service" anti-pattern — it produces procedural code that is hard to test and reuse.

---

## Q24. What is a Repository in DDD and how does it differ from a data-access layer?

What is a Repository in DDD and how does it differ from a data-access layer?

**Answer:** A Repository in DDD is an abstraction that acts as an in-memory collection of Aggregate Roots — callers add, remove, and retrieve Aggregates from it using domain-meaningful queries, without knowing anything about the underlying storage mechanism. A data-access layer (DAL), by contrast, is typically a set of classes for executing queries and CRUD operations, often exposing the database structure rather than the domain model.

- The Repository abstraction keeps the domain layer clean: `IOrderRepository` is defined in the domain project and depends only on domain types; the concrete implementation using Entity Framework Core lives in the infrastructure layer.
- Repositories should operate at the Aggregate level — you load a complete Aggregate, change it, and save it back. They do not expose raw query methods like `GetByFirstName` or `GetWhere(x => x.Status == ...)` that would encourage bypassing the Aggregate boundary.
- A common misuse is creating a repository for every Entity inside an Aggregate — only Aggregate Roots should have repositories. `OrderLineRepository` in a model where `OrderLine` is inside the `Order` Aggregate is a sign that the boundaries are not respected.
- In read-heavy scenarios, DDD often recommends using a separate query model (CQRS) instead of going through the repository, since forcing all reads through a fully reconstructed Aggregate is unnecessarily expensive.

---

## Q25. What is a Factory in DDD, and when should you use one instead of a constructor?

What is a Factory in DDD, and when should you use one instead of a constructor?

**Answer:** A Factory in DDD is a method or class responsible for creating complex domain objects — particularly Aggregates — when the construction logic is complex enough to warrant isolation from the object itself. A Factory ensures that created objects are always in a valid initial state, which is especially important for Aggregates with multiple required dependencies or complex invariant checks at creation time.

- Use a Factory when constructing an Aggregate requires fetching data, making decisions based on business rules, or setting up a web of related objects — putting all that logic in a constructor makes the constructor hard to read, test, and evolve.
- Factory methods on the Aggregate Root itself (static `Create(...)` methods) are a common and readable DDD pattern in C#, communicating intent better than a constructor with many parameters.
- A dedicated Factory class makes sense when creation involves multiple steps, produces different subtypes based on input, or when the creation logic itself is a domain concept (like a `LoanApplicationFactory` that applies eligibility rules during construction).
- Factories are also important for reconstitution — recreating an Aggregate from stored data — which is typically handled by the ORM (Object-Relational Mapper) but sometimes requires a custom factory when the mapping is non-trivial.

---

## Q26. What is an anemic domain model and why is it considered an anti-pattern?

What is an anemic domain model and why is it considered an anti-pattern?

**Answer:** An anemic domain model is one where the domain objects (Entities, Aggregates) contain only data — public properties with getters and setters — while all business logic lives in separate service classes that read from and write to those objects. Martin Fowler coined the term as an anti-pattern because it reproduces a procedural programming style inside an object-oriented structure.

- The problem with the anemic model is that invariants cannot be enforced: if `Order` is just a bag of properties, any code anywhere in the application can set `order.Status = "Shipped"` without checking whether the order is paid, regardless of the business rule that says only paid orders can be shipped.
- Anemic models result in duplicated logic: if the same business rule needs to apply in multiple places, it must be copy-pasted into each service that touches the object, creating divergence over time.
- It is easy to slide into the anemic model with ORM-first development — developers model their classes to match database tables (lots of public setters for EF Core mapping) and then put logic in services, ending up with the anti-pattern without intending to.
- The fix is to move business logic back into the domain objects, making setters private or removing them, and exposing intent-revealing methods like `order.Ship()`, `order.Cancel(reason)`, and `account.Withdraw(amount)` that enforce the rules internally.

---

## Chapter 7 — Layered Architecture & Clean Architecture in DDD

---

## Q27. What are the standard layers in a DDD-based application, and what are the dependency rules between them?

What are the standard layers in a DDD-based application, and what are the dependency rules between them?

**Answer:** A DDD-based application is typically structured in four layers — Domain, Application, Infrastructure, and Presentation — with a strict dependency rule: outer layers depend on inner layers, and the Domain layer has no dependencies on any other layer.

| Layer | Responsibility | Depends on |
|---|---|---|
| Domain | Entities, Value Objects, Aggregates, Domain Events, Domain Services, Repository interfaces | Nothing (pure domain logic) |
| Application | Use case orchestration, Command/Query handlers, Application Services | Domain |
| Infrastructure | Repository implementations, DB context, message bus, external API clients | Domain, Application (through interfaces) |
| Presentation | Controllers, gRPC endpoints, message consumers, CLI | Application |

- The Domain layer is the most stable and the most important — it changes only when business rules change, never because a database or framework was swapped out.
- The Infrastructure layer implements the interfaces defined in the Domain layer (Repository interfaces, notification interfaces), following the Dependency Inversion Principle so that the domain never imports an ORM or messaging library.
- In Clean Architecture (Robert C. Martin's formalisation of this idea), the same principle is described as the "Dependency Rule": source code dependencies always point inward toward higher-level policies, and the domain is the innermost ring.

---

## Q28. Why should the domain model have no knowledge of infrastructure concerns like databases or HTTP clients?

Why should the domain model have no knowledge of infrastructure concerns like databases or HTTP clients?

**Answer:** The domain model must remain independent of infrastructure because infrastructure is a detail — it can change (switching from SQL Server to PostgreSQL, from REST to gRPC) without changing the business rules. If the domain model imports Entity Framework attributes or `HttpClient` directly, every database migration or API change forces changes to the domain layer, eroding its stability and testability.

- A domain model that imports `[Column]` or `[Table]` attributes from Entity Framework is coupled to the ORM: changing the mapping strategy requires modifying domain classes, violating the Single Responsibility Principle.
- Infrastructure dependencies make unit testing the domain hard because tests must set up database connections or HTTP servers rather than just instantiating a domain object and calling a method.
- The practical pattern is to define interfaces in the domain layer (like `IEmailSender`, `IOrderRepository`) and implement them in the infrastructure layer; the domain never sees the concrete implementation, only the contract.
- Keeping the domain model infrastructure-free also enables running it in completely different hosting contexts — the same domain can be used in an ASP.NET Core web service, a console app, or a test harness without modification.

---

## Chapter 8 — DDD with Microservices, CQRS & Sagas

---

## Q29. How do bounded contexts map to microservices, and what are the risks of mapping them one-to-one?

How do bounded contexts map to microservices, and what are the risks of mapping them one-to-one?

**Answer:** Bounded contexts and microservices are complementary concepts but not identical. A bounded context defines a semantic boundary around a coherent domain model; a microservice is a deployable unit with its own process and database. In an ideal architecture, each microservice owns exactly one bounded context, but the mapping does not have to be one-to-one.

- One bounded context per microservice is a sensible default because it ensures each service has a coherent, internally consistent model and owns its own data — the service boundary and the model boundary align.
- The risk of forcing a strict one-to-one mapping prematurely is creating microservices that are too small and too chatty: each service is a trivial CRUD endpoint, services must constantly call each other to complete any meaningful operation, and the overhead of distributed systems is paid without the benefits of true isolation.
- Some bounded contexts start as modules within a monolith and are extracted into separate services later when the team size, deployment frequency, or scaling requirements justify the operational overhead — starting as a "modular monolith" is a valid DDD-compatible approach.
- Multiple bounded contexts in one service is also valid when the contexts are small or closely related; the key constraint is that one service should never own the model of another service's bounded context.

---

## Q30. How does DDD integrate with Command Query Responsibility Segregation (CQRS)?

How does DDD integrate with Command Query Responsibility Segregation (CQRS)?

**Answer:** Command Query Responsibility Segregation (CQRS) separates write operations (Commands that change state) from read operations (Queries that return data). DDD and CQRS complement each other naturally: the Command side uses the full DDD stack (Aggregates, Domain Events, Repositories) to enforce business invariants on writes, while the Query side bypasses the domain model entirely and reads directly from optimised read models or database views.

- On the Command side, a `PlaceOrderCommand` handler loads the `Order` Aggregate from its Repository, calls `order.Place(items, shippingAddress)`, handles any Domain Events raised, and saves via the Repository — the entire DDD model is engaged.
- On the Query side, a `GetOrderSummaryQuery` handler skips the Repository and Aggregate entirely, querying a read-optimised view or a separate read database directly, returning a flat DTO (Data Transfer Object) — no domain objects are involved.
- This split solves a real problem: loading a fully constructed Aggregate with all its invariant-preserving complexity is wasteful for a simple "show the order list" query. Read models are simpler, faster, and optimised for the UI's needs.
- In event-sourced architectures, CQRS is nearly mandatory: events are the write model, and the read models are projections built by replaying those events — DDD Domain Events become the source of truth that feeds read projections.

---

## Q31. What is the Saga pattern and how does it relate to DDD's aggregate boundaries and eventual consistency?

What is the Saga pattern and how does it relate to DDD's aggregate boundaries and eventual consistency?

**Answer:** A Saga is a pattern for managing long-running business processes that span multiple Aggregates or bounded contexts, coordinating a sequence of steps where each step is a local transaction on one Aggregate. Because DDD's one-transaction-one-aggregate rule prevents multi-aggregate transactions, the Saga compensates for failures by running compensating transactions that undo previous steps.

- There are two Saga implementations: choreography-based Sagas where each service listens for events and decides its next action independently, and orchestration-based Sagas where a central Saga orchestrator sends commands to each participant and reacts to their replies.
- Choreography Sagas are simpler and more decoupled but harder to trace — understanding the full process flow requires reading each service's event handlers. Orchestration Sagas centralise the flow logic in one place, making it easier to monitor and debug but introducing a coordination bottleneck.
- Compensating transactions are the Saga's rollback mechanism: if `ConfirmInventory` succeeds but `ChargePayment` fails, the Saga sends `ReleaseInventory` to undo the reservation. Compensating transactions must be designed as domain operations — `inventory.ReleaseReservation(orderId)` — not as database rollbacks.
- Sagas require careful attention to idempotency: steps may be retried, so each participant must handle receiving the same command twice without duplicating side-effects, typically by tracking which correlation IDs have already been processed.

---

## Q32. What are the most common mistakes developers make when applying DDD in practice?

What are the most common mistakes developers make when applying DDD in practice?

**Answer:** DDD is widely misapplied because its tactical patterns look simple in isolation but require disciplined judgment to apply correctly. The most common mistakes involve applying the patterns mechanically without the underlying strategic thinking that gives them meaning.

- **Skipping strategic design and jumping straight to tactical patterns**: writing Entities, Value Objects, and Repositories without first defining bounded contexts and ubiquitous language produces tactically correct but strategically incoherent models that grow messy over time.
- **Building anemic domain models**: creating Entity classes with nothing but public getters and setters and putting all logic in service classes defeats the purpose of DDD — business rules are scattered rather than encapsulated.
- **Over-engineering simple domains**: applying full tactical DDD to a simple CRUD application adds complexity without business benefit; DDD is justified by domain complexity, not by architectural preference.
- **Making Aggregates too large**: including every related concept in one Aggregate to avoid eventual consistency creates contention, performance problems, and a God Object that is hard to change.
- **Using database-generated IDs and leaking ORM concerns into the domain**: auto-increment IDs prevent Domain Events from referencing the Entity's ID before it is saved; ORM attributes on domain classes couple the model to the persistence framework.
- **Ignoring the Ubiquitous Language after the initial design**: if code names drift from the business vocabulary over time, the domain model stops reflecting the real domain and the communication benefits of DDD are lost.

---
