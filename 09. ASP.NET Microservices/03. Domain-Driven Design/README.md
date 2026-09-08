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

**Concepts**
- Business domain as the center of all design decisions
- Ubiquitous language bridging developer and domain expert vocabulary
- Strategic design for carving up a large domain
- Tactical design building blocks for modeling concepts
- Context map as the shared architectural blueprint

**Answer**

Domain-Driven Design (DDD) is an approach to software development that places the business domain — the real-world problem the software solves — at the center of all design decisions. Eric Evans introduced DDD in his 2003 book to fix a recurring problem: large codebases become unmaintainable because developers use their own technical vocabulary while business experts use another, so the code never truly reflects how the business works. DDD insists that developers and domain experts share a single, precise vocabulary called the ubiquitous language, ensuring that the code reads like the business concepts it models rather than a layer of technical abstractions that must be mentally translated. The approach is especially valuable for complex domains — order management, insurance underwriting, logistics — where business rules are intricate and frequently change; it is less justified for simple CRUD applications where the overhead outweighs the benefit. DDD is split into two halves: strategic design (how to carve up a large domain into manageable, coherent pieces) and tactical design (how to model the concepts inside each piece using building blocks like Entities, Value Objects, and Aggregates). The primary output of strategic DDD is a context map — a diagram of how different parts of the system relate and communicate — which serves as a shared architectural blueprint for both technical and business stakeholders.

---

## Q2. What is the difference between strategic DDD and tactical DDD?

**Concepts**
- Strategic DDD scoping the whole system and its boundaries
- Tactical DDD providing object-model building blocks within one context
- Context map and bounded contexts as strategic outputs
- Entities, Value Objects, and Aggregates as tactical outputs
- Strategic design preceding tactical design in sequence

**Answer**

Strategic DDD deals with the large-scale structure of the system — how to divide a complex domain into smaller, coherent areas and how those areas communicate. Tactical DDD provides a set of object-model building blocks used inside each area to accurately capture business rules in code. Strategic design is language-first: the key activity is agreeing on boundaries and vocabulary with business stakeholders before writing any code. Tactical design patterns only make sense inside a well-defined bounded context; applying tactical patterns without strategic clarity produces a tangled model that tries to serve too many purposes at once.

| Aspect | Strategic DDD | Tactical DDD |
|---|---|---|
| Scope | Whole system / organisation | Single bounded context |
| Concern | Boundaries and relationships | Object model internals |
| Output | Context map, bounded contexts | Entities, Value Objects, Aggregates, Domain Events |
| Who is involved | Architects, domain experts, developers | Developers modelling one domain |
| When applied | Early design and architecture | Implementation phase |

---

## Q3. What is a domain, a subdomain, and what are the three types of subdomains (core, supporting, generic)?

**Concepts**
- Domain as the entire subject area of the software
- Subdomain as a coherent area of business concern
- Core domain as the source of competitive advantage
- Supporting domain as a necessary but non-differentiating area
- Generic domain handled by commodity solutions

**Answer**

The domain is the entire subject area the software is built around — for example, "e-commerce" or "healthcare claims processing." Because a domain is too large to model uniformly, DDD breaks it into subdomains, each representing a coherent area of business concern. A core domain is where the business derives its competitive advantage — the area that must be modelled with the most care and investment, so for an e-commerce company the recommendation engine or dynamic pricing engine might be the core domain. A supporting domain enables the core domain but is not a competitive differentiator — for example, inventory management or notification delivery. It warrants custom code because off-the-shelf solutions do not fit exactly, but it does not need the same depth of modelling as the core. A generic domain solves a well-understood problem that has no strategic value in being unique — authentication, billing, or email sending — and is almost always best handled by buying or adopting an existing product rather than building from scratch. Recognizing subdomain types guides investment decisions: spend your best engineers on the core domain and use commodity solutions everywhere else.

---

## Q4. What is ubiquitous language and why is it important in DDD?

**Concepts**
- Shared vocabulary used consistently by all team members
- Ubiquitous language scoped to a single bounded context
- Code naming reflecting business terminology directly
- Active vocabulary negotiation between developers and domain experts
- Self-documenting code as the result of ubiquitous language

**Answer**

Ubiquitous language is a shared, precise vocabulary for a bounded context that is used consistently by every member of the team — developers, domain experts, product managers — in conversation, documentation, and in the code itself. The word "ubiquitous" means "found everywhere" — the same terms appear in class names, method names, tests, and user stories with no translation layer between them. When developers use different words than business experts — for example, calling a "policy" an "insurance contract" in code while the business calls it a "policy" — every conversation requires mental translation, and misunderstandings compound into bugs. Ubiquitous language is bounded to a context: the word "Account" can mean a bank account in the payments context and a user profile in the identity context, and both are correct within their own boundary. Building the ubiquitous language is an active collaboration: domain experts correct the model when a term does not match business reality, and developers push back when business terms are ambiguous, resulting in a richer shared understanding on both sides. The code becomes self-documenting when it uses the ubiquitous language — a method named `policy.Lapse()` is immediately understandable to an insurance domain expert without reading the implementation.

---

## Q5. What is a bounded context and why is it the cornerstone of strategic DDD?

**Concepts**
- Bounded context as an explicit semantic boundary for one domain model
- Single term having one meaning inside the boundary
- Bounded context versus microservice distinction
- Technical enforcement via separate codebase, schema, and API contracts
- Boundary size as the critical design decision

**Answer**

A bounded context is an explicit boundary within which a particular domain model applies and a particular ubiquitous language is used consistently. Inside the boundary, every term has one meaning; outside the boundary, the same word may mean something entirely different. Without explicit boundaries, a single model is forced to satisfy multiple conflicting purposes — a "Customer" that means a shopping cart owner in one part of the system and a billing account in another will accrue compromises until the model satisfies nobody cleanly. A bounded context is not the same as a microservice, although they often map onto each other; a bounded context is a conceptual boundary that can be implemented as a single service, multiple services, or even a module inside a monolith. The boundary is enforced technically — separate codebase or module, separate database schema, explicit API contract between contexts — so that changes inside one context cannot break another context's model. One of the most valuable activities in DDD is drawing the boundaries correctly: too broad and the model becomes incoherent; too narrow and you end up with trivial services that spend most of their time calling each other.

---

## Chapter 2 — Context Mapping & Integration Patterns

---

## Q6. What is context mapping in DDD and why does it matter?

**Concepts**
- Context map as a diagram of all bounded contexts and their relationships
- Upstream producer versus downstream consumer identification
- Team dependency visibility
- Context map as a living architectural artefact
- Organisational power dynamics captured alongside technical connections

**Answer**

Context mapping is the practice of identifying all bounded contexts in a system and documenting how they relate to and communicate with each other, producing a diagram called a context map. The context map exposes who owns what, who depends on whom, and how information flows across boundaries — making it the most important architectural artefact in a DDD project. Without a context map, team dependencies are invisible: one team unknowingly breaks another by changing a shared model, or teams duplicate effort because they did not know another context already solved the same problem. The context map records not just technical connections but also organisational relationships — which team is upstream (producer) and which is downstream (consumer), and what the power dynamic is between them, since those political realities shape what integration patterns are feasible. A context map is a living document that evolves as the architecture evolves; the act of drawing it forces teams to articulate and negotiate integration contracts they otherwise leave implicit.

---

## Q7. What are the most important context-mapping relationship patterns, and what does each one mean?

**Concepts**
- Partnership as mutual alignment between co-evolving teams
- Shared Kernel as a co-owned model subset
- Customer-Supplier as negotiated downstream influence
- Conformist versus Anti-Corruption Layer as upstream adoption trade-off
- Open Host Service and Published Language for stable upstream APIs

**Answer**

DDD defines a set of relationship patterns that characterise how two bounded contexts integrate. Each pattern captures not just the technical connection but also the team dynamic and level of trust between the two sides. The Conformist and ACL patterns both occur when one team has no influence over the upstream model, but they differ in impact: Conformist is cheaper (no translation code) but bleeds upstream concepts into the downstream model, while ACL is more expensive but keeps the downstream model pure. Choosing the wrong pattern often leads to tight coupling that makes future changes painful; the context map makes these trade-offs explicit and debatable.

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

---

## Q8. What is an Anti-Corruption Layer (ACL) and when should you use one?

**Concepts**
- ACL as a translation layer protecting the downstream model
- Adapter and translator classes at the context boundary
- Legacy system integration as the primary ACL use case
- ACL maintenance cost versus downstream model purity
- Microservice event subscription ACL implementation

**Answer**

An Anti-Corruption Layer is a translation layer that sits at the boundary of a downstream bounded context and converts data and concepts from an upstream context — or a legacy system — into the downstream context's own model. The name reflects its purpose: protecting the downstream model from being "corrupted" by alien concepts or legacy terminology that do not belong in its language. An ACL is most valuable when integrating with a legacy system whose model is messy, poorly documented, or uses different terminology — without the ACL, legacy concepts would leak throughout the downstream code and make it harder to understand and change. The ACL typically involves adapters, translators, or facades that map incoming DTOs from the upstream model to domain objects in the downstream model, keeping the domain model free of cross-context dependencies. The cost of an ACL is the translation code itself, which must be maintained whenever the upstream interface changes; this cost is justified when the upstream model is volatile or conceptually incompatible with the downstream model. In microservices, an ACL is commonly implemented as a set of mapping classes in the subscribing service that convert events or API responses from another service into domain events or commands that the local domain understands.

---

## Q9. What is an Open Host Service (OHS) and a Published Language, and how do they enable integration?

**Concepts**
- Open Host Service shifting integration burden to the upstream
- Published Language as a versioned shared data format contract
- Schema registry enforcing Published Language versioning
- OHS reducing downstream ACL need
- Backward-compatible schema evolution in Published Language

**Answer**

An Open Host Service (OHS) is a pattern where an upstream context defines a clean, stable service interface — a published API — that any downstream context can integrate with without negotiating custom arrangements. A Published Language extends this by defining a precise, shared data format such as a JSON schema, Avro schema, or Protocol Buffers definition that all consumers use, making integration explicit and toolable. OHS shifts the integration burden to the upstream: instead of every consumer adapting to whatever the upstream produces, the upstream takes responsibility for offering a well-designed, backwards-compatible interface. Published Language is common in event-driven architectures where a service publishes events to a message bus; the event schema is the "published language" that consumers must understand, and schema registries enforce its versioning. Together, OHS and Published Language reduce the need for Anti-Corruption Layers in downstream contexts because the upstream model is already designed for consumption rather than being an arbitrary internal representation.

---

## Q10. What is the difference between an upstream context and a downstream context?

**Concepts**
- Upstream as the producer whose model flows to others
- Downstream as the consumer dependent on the upstream model
- Upstream change ripple effect on downstream consumers
- Customer-Supplier pattern addressing the power imbalance
- Change authority versus technical dependency direction

**Answer**

In a context-mapping relationship, the upstream context produces or defines the model that flows to another, while the downstream context consumes or depends on that model. The upstream team has more influence: their decisions about how to model something directly affect what the downstream team must deal with, regardless of whether the downstream team agrees. Changes in an upstream context ripple downstream — a field renamed or removed in the upstream API breaks downstream consumers — which is why the direction of dependency matters for managing change and risk. The Customer-Supplier pattern addresses the power imbalance: the downstream team acts as a "customer" with requirements, and the upstream team acts as a "supplier" who incorporates those requirements into their roadmap, giving the downstream team some influence. In practice, "upstream" and "downstream" are not always technically obvious; they reflect ownership and change authority, and identifying them prevents silent assumptions about who is responsible for compatibility.

---

## Chapter 3 — Entities, Value Objects & Identity

---

## Q11. What is an Entity in DDD and how does it differ from a Value Object?

**Concepts**
- Entity identity persisting through attribute changes
- Value Object equality determined by all attribute values
- Entity mutable lifecycle versus Value Object immutability
- Identity tracking overhead versus Value Object simplicity
- Over-using Entities leading to unnecessary lifecycle management

**Answer**

An Entity is a domain object that has a unique identity that persists through the lifetime of the object, even as its attributes change. Two Entities are the same if and only if they share the same identity — two `Order` objects with the same `OrderId` represent the same order regardless of any other attribute. A Value Object, by contrast, has no identity; two Value Objects are equal if all their attributes are equal. The choice matters for design: Entities are tracked and managed over time, while Value Objects are replaced wholesale when they change — instead of mutating an `Address`, you assign a new `Address` instance. Many concepts that seem like Entities are better modelled as Value Objects; over-using Entities leads to complex lifecycle management and unnecessary identity tracking.

| Aspect | Entity | Value Object |
|---|---|---|
| Identity | Has a unique ID | No identity; equality by value |
| Mutability | Usually mutable | Always immutable |
| Examples | Customer, Order, Product | Money, Address, DateRange |
| Lifetime | Long-lived, persisted | Short-lived, often replaced |

---

## Q12. What is a Value Object? What makes it immutable, and when should you prefer it over an Entity?

**Concepts**
- Value Object fully described by its attribute values
- Immutability eliminating shared-mutable-state bugs
- C# record or record struct as the natural implementation
- Validation and business rules encapsulated in the Value Object constructor
- Measurement and descriptor concepts as natural Value Object candidates

**Answer**

A Value Object is a domain concept that is fully described by its attributes, has no identity of its own, and is always immutable — once created, its values never change. `Money(100, "USD")` and another `Money(100, "USD")` are interchangeable; there is no meaningful distinction between them beyond their values. Immutability is the key design property: a Value Object is never modified — instead, operations on it produce a new instance, similar to how `string` works in C#. This eliminates a whole class of bugs caused by shared mutable state. I prefer a Value Object when the concept is defined entirely by its measurements or attributes and there is no need to track it independently — postal codes, GPS coordinates, temperature ranges, and email addresses are natural Value Objects. Value Objects can encapsulate validation and business rules, preventing invalid states from existing — a `Money` Value Object can enforce that the amount is non-negative in its constructor, rather than having that check scattered across the codebase. In C# 10+, `record struct` or `record` types are ideal for Value Objects because they provide structural equality, immutability by convention, and concise syntax.

---

## Q13. How do you implement Entity equality and Value Object equality in C#?

**Concepts**
- Entity equality based solely on ID, ignoring mutable properties
- Value Object structural equality via C# record type
- Entity base class implementing Equals and GetHashCode from ID
- Transient Entity handling when ID is the default value
- Compiler-generated equality for records versus manual override for Entities

**Answer**

Entities compare by identity: two Entity instances are equal if they have the same ID, regardless of their other properties. Value Objects compare structurally: two instances are equal if every attribute is equal. Getting these implementations right prevents subtle bugs where two objects that represent the same thing compare as different.

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

Using `record` for Value Objects is the modern approach in .NET; the compiler generates `Equals`, `GetHashCode`, and `==` based on all properties, which is exactly the structural equality semantics needed. Entity equality must be based only on ID — never include mutable properties like name or status in `Equals`, because two Entities with the same ID but different states are still the same Entity. Transient Entities (not yet persisted, ID is default value) require special handling — two new Entities should not be considered equal simply because both have a default ID.

---

## Q14. What is identity in DDD — should you use database-generated IDs or domain-generated IDs, and why?

**Concepts**
- Domain-generated GUID identity enabling pre-persistence completeness
- Database-generated auto-increment breaking pre-persistence Domain Events
- Strongly typed ID wrappers preventing primitive obsession bugs
- Sequential GUIDs combining domain generation with index friendliness
- Entity completeness before the database as a DDD principle

**Answer**

Identity in DDD means the attribute that uniquely identifies an Entity and distinguishes it from all other instances of the same type over its lifetime. Database-generated identity such as auto-increment integers or database sequences means the Entity does not have an ID until it is persisted, which breaks the DDD principle that an Entity is a complete, valid object before it is saved — you cannot dispatch Domain Events referencing an Entity's ID before it hits the database. Domain-generated identity using GUIDs or strongly typed IDs such as `OrderId` and `CustomerId` solves this: the identity is assigned at object creation, so the Entity is fully formed and can participate in business logic, events, and unit tests without a database round-trip. Strongly typed IDs wrapping a `Guid` prevent primitive obsession bugs where an `orderId` Guid is accidentally passed where a `customerId` Guid is expected — the compiler catches the mistake. Sequential GUIDs using `Guid.CreateVersion7()` in .NET 9+ combine the benefits of domain-generated IDs with database index friendliness, avoiding the fragmentation that standard random GUIDs cause on clustered indexes.

---

## Chapter 4 — Aggregates & Invariants

---

## Q15. What is an Aggregate and what is the Aggregate Root?

**Concepts**
- Aggregate as a consistency cluster of Entities and Value Objects
- Aggregate Root as the sole external entry point
- Aggregate enforcing all business invariants within its boundary
- Aggregate Root controlling the lifecycle of inner objects
- Aggregate as the transaction boundary for persistence

**Answer**

An Aggregate is a cluster of related Entities and Value Objects that are treated as a single unit for the purpose of data changes, with one Entity designated as the Aggregate Root. The Aggregate Root is the only entry point through which the outside world can interact with the cluster; no external object may hold a direct reference to an inner Entity. The Aggregate enforces all business invariants — consistency rules that must always hold — within its boundary, so for an `Order` Aggregate containing `OrderLine` entities, the `Order` Aggregate Root ensures that the total of all `OrderLine` items always matches the stored total and that an order in "Shipped" status cannot have items added to it. The Aggregate Root controls the lifecycle of all objects inside it: inner objects are created, modified, and deleted only through methods on the root, so external code calls `order.AddItem(product, quantity)` rather than manipulating the `OrderLines` collection directly. Aggregates define the transactional boundary: everything inside an Aggregate is saved or rolled back together in a single database transaction, while consistency across Aggregate boundaries is achieved through eventual consistency and Domain Events.

---

## Q16. What invariants do Aggregates enforce, and why must all modifications go through the Aggregate Root?

**Concepts**
- Invariant as a business consistency rule that must always hold
- Aggregate Root as the sole write interceptor enforcing invariants
- Tell-don't-ask principle via Aggregate Root methods
- IReadOnlyList exposure for inner collections
- Scattered invariant enforcement as the cost of bypassing the root

**Answer**

An invariant is a business rule that must always be true — a consistency constraint that cannot be violated at any point in time. Aggregates enforce invariants by controlling all write access to their state; since every mutation goes through the Aggregate Root's methods, the root can check the invariant after every change and throw a domain exception if it would be violated. If external code could modify inner Entities directly, the Aggregate would have no way to intercept the change and verify the invariant — the invariant would need to be checked externally, scattered across application code where it is easily forgotten. A `BankAccount` Aggregate has an invariant that the balance may not go below zero for a standard account, so the method `account.Withdraw(amount)` checks the balance rule before updating state; if the check were skipped, any caller could directly reduce the balance field and break the invariant. DDD calls this the "tell, don't ask" principle — callers tell the Aggregate Root what they want to do, and the root decides whether it is allowed and how to do it, rather than callers reading state, making their own decisions, and writing back. In C#, this is enforced by making inner collection properties private or using read-only collections such as `IReadOnlyList<OrderLine>` exposed for reading, with all mutation going through root methods.

---

## Q17. What is the one-transaction-one-aggregate rule, and what does it mean for eventual consistency?

**Concepts**
- One transaction scoping to one Aggregate boundary
- Domain Events coordinating cross-aggregate consistency
- Eventual consistency as the consequence of separate transactions
- Intermediate state tolerance as a domain design requirement
- Outbox pattern ensuring reliable Domain Event delivery

**Answer**

The one-transaction-one-aggregate rule states that a single database transaction should modify only one Aggregate. If a business operation needs to change two Aggregates, those changes must happen in separate transactions, with the second change triggered by a Domain Event that the first Aggregate publishes. The rule exists because trying to update two Aggregates in one transaction means taking distributed locks across multiple rows or tables, reducing concurrency and creating coupling between Aggregates — if one fails, the other rolls back too, tying their lifecycles together. Updating two Aggregates in separate transactions means there is a period of time between the first and second transaction where the system is in an intermediate state — this is called eventual consistency, and the domain must be designed to tolerate it. Not all operations can tolerate eventual consistency; when strong consistency between two concepts is truly required, it usually means those concepts belong in the same Aggregate, not in separate ones — the rule also serves as a design signal about aggregate boundaries. The Outbox pattern is commonly used to guarantee that the Domain Event is reliably published even if the process crashes between transactions, ensuring the second Aggregate eventually receives the event.

---

## Q18. How do you decide the size and boundaries of an Aggregate? What are the signs of a God Aggregate?

**Concepts**
- Minimum boundary enclosing all invariant-related objects
- Co-mutation test for determining Aggregate membership
- God Aggregate as an oversized cluster causing contention
- Domain Events replacing direct references for cross-aggregate coordination
- Aggregate reference by ID as a boundary enforcement technique

**Answer**

Aggregate boundaries should be as small as possible while still being able to enforce all business invariants. The guiding principle is to include in an Aggregate only the objects that must change together to maintain a consistency rule — everything else belongs outside the boundary. I start by asking: "Can this invariant be violated if I change A without also changing B?" If yes, A and B belong in the same Aggregate. If no, they can be separate Aggregates that reference each other by ID. A God Aggregate is one that has grown to include every loosely related concept — an `Order` that contains `Customer`, `Product`, `Inventory`, and `Payment` all in one cluster. It becomes the entire application in a single object, requires large transactions, and serializes all concurrent writes. Signs of a God Aggregate include dozens of nested objects, loading it requiring hundreds of rows, multiple concurrent users always contending on the same instance, or objects that rarely or never change together. The fix is to split along natural consistency boundaries and use Domain Events to propagate changes between the new smaller Aggregates — for example, `Order` references `CustomerId` as a value, not the full `Customer` Aggregate.

---

## Q19. How should Aggregates reference each other — by object reference or by ID?

**Concepts**
- ID-based cross-aggregate references enforcing transaction boundaries
- Object reference enabling accidental cross-boundary mutation
- Separate Repository load for each referenced Aggregate
- EF Core navigation property omission for cross-aggregate references
- Performance improvement from preventing accidental eager loading

**Answer**

Aggregates should reference other Aggregates by identity (an ID value) rather than by object reference. This enforces the transactional boundary — holding an object reference invites code to navigate into a foreign Aggregate and mutate it, blurring the boundary and enabling cross-aggregate consistency violations. If `Order` held a direct reference to the `Customer` object, it would be tempting and possible to call `order.Customer.ChangeName("New")` from outside the `Customer` Aggregate's boundary, bypassing any invariants the `Customer` enforces. Referencing by ID means: when you need data from another Aggregate during a use case, you load it separately through its own Repository, apply changes through its own root methods, and save it in its own transaction. In Entity Framework Core this is implemented by not including navigation properties to foreign Aggregate roots — the `Order` entity has a `CustomerId` property of type `Guid` or a strongly typed `CustomerId`, but no `public Customer Customer` navigation property. This also improves performance by preventing accidental eager loading of an entire Aggregate graph when only a subset is needed.

---

## Chapter 5 — Domain Events & Integration Events

---

## Q20. What is a Domain Event in DDD and why is it important?

**Concepts**
- Domain Event as an immutable past-tense record of a business fact
- Domain Event enabling downstream decoupling without direct service calls
- Domain Event as a design tool revealing side-effect relationships
- In-process mediator dispatch within a single bounded context
- Domain Events becoming Integration Events across bounded contexts

**Answer**

A Domain Event is an immutable record of something significant that happened within the domain — something a business expert would care about — expressed in the past tense, such as `OrderPlaced`, `PaymentReceived`, or `CustomerRegistered`. Domain Events are the mechanism by which parts of the domain react to changes without being directly coupled to each other, because instead of an `Order` service directly calling an inventory service and a notification service after placing an order, the `Order` Aggregate publishes `OrderPlaced` and other parts of the system subscribe to it and take their own actions independently. This decoupling allows each subscriber to evolve independently — adding a new consequence such as sending a confirmation email means adding a new handler, not modifying the `Order` Aggregate or the `PlaceOrderCommandHandler`. Domain Events also serve as a design tool: if a domain expert says "when X happens, we need to do Y and Z," those side-effects are natural candidates for event handlers, and the phrasing itself suggests the event name. Within a single bounded context, Domain Events are often dispatched in-process using a mediator like MediatR in .NET. Across bounded contexts they typically become Integration Events transmitted over a message bus.

---

## Q21. What is the difference between a Domain Event and an Integration Event?

**Concepts**
- Domain Event as an in-process intra-context notification
- Integration Event as a serialisable inter-context message bus message
- Domain type usage in Domain Events versus DTO usage in Integration Events
- Integration Event versioning strategy for backward compatibility
- Domain Event to Integration Event translation at the context boundary

**Answer**

A Domain Event is an in-process notification that something happened within a single bounded context; it uses the domain's own language and object model. An Integration Event is a message published to a message bus that crosses bounded context or service boundaries; it uses a serialisable, versionable contract that other services can consume without knowing about the originating domain model. The mapping between the two is intentional: a Domain Event handler may translate an `OrderPlaced` domain event into an `OrderConfirmedIntegrationEvent` DTO and publish it to the bus, decoupling the internal model from the external contract. Integration Events must be designed with stability in mind — adding fields is safe, removing or renaming fields is a breaking change that requires coordination with all consumers.

| Aspect | Domain Event | Integration Event |
|---|---|---|
| Scope | Inside one bounded context | Across bounded contexts or services |
| Transport | In-process (mediator, event dispatcher) | Message bus (RabbitMQ, Azure Service Bus, Kafka) |
| Language | Uses domain types (Entities, Value Objects) | Uses serialisable DTOs (JSON, Protobuf) |
| Versioning | Internal — no external consumers | Requires versioning strategy for backward compatibility |
| Examples | `OrderPlaced` (inside Order context) | `OrderConfirmedIntegrationEvent` (published to bus) |

---

## Q22. How do you dispatch Domain Events in a way that keeps the domain model clean? What is the Outbox pattern?

**Concepts**
- Aggregate Root collecting Domain Events without dispatching them
- Application layer dispatching events after transaction commit
- At-least-once delivery guarantee via the Outbox pattern
- Outbox table written in the same transaction as Aggregate changes
- Consumer idempotency required by at-least-once delivery

**Answer**

The cleanest approach is to have the Aggregate Root collect Domain Events during a business operation and store them on the object rather than publishing them directly, then dispatch them after the transaction completes — keeping the domain model free of infrastructure dependencies like message bus clients. The Aggregate Root exposes a collection like `IReadOnlyList<IDomainEvent> DomainEvents` which accumulates events during operations. After `dbContext.SaveChangesAsync()` succeeds, the application layer iterates these events and dispatches them via a mediator or event dispatcher. The risk: if the application crashes between `SaveChanges` (database committed) and event dispatch (not yet published), the event is lost and downstream subscribers never react — this breaks eventual consistency. The Outbox pattern addresses this: Domain Events are written to an "outbox" table in the same database transaction as the Aggregate changes. A background worker polls the outbox table and publishes any unpublished events to the message bus, marking them as published — even if the process crashes, the next restart picks up the unpublished events. This guarantees at-least-once delivery, which means consumers must be idempotent — able to handle the same event arriving more than once without duplicating side-effects.

---

## Chapter 6 — Domain Services, Application Services & Repositories

---

## Q23. What is a Domain Service and how does it differ from an Application Service?

**Concepts**
- Domain Service as cross-object business logic in the Domain layer
- Application Service as use-case orchestration in the Application layer
- Domain Service tested with pure unit tests and no mocks
- Fat service anti-pattern from business logic in Application Services
- Domain Service reuse across multiple Application Services

**Answer**

A Domain Service is an operation that belongs in the domain layer because it expresses a significant business rule or process, but it does not naturally belong to any single Entity or Value Object — typically because it operates on multiple Aggregates or requires domain logic that spans objects. An Application Service orchestrates the use case — it loads Aggregates, calls Domain Services and Aggregate methods, and then saves results — but it contains no business logic itself. A rule of thumb: if deleting the class would remove business logic, it is a Domain Service. If deleting it would only remove orchestration steps, it is an Application Service. Application Services are the entry point called by controllers or message handlers; they handle cross-cutting concerns like transaction management, logging, and authorisation, delegating all real decisions to the domain. Overloading Application Services with business logic is a code smell called the "fat service" anti-pattern — it produces procedural code that is hard to test and reuse.

| Aspect | Domain Service | Application Service |
|---|---|---|
| Layer | Domain layer | Application layer |
| Contains | Business logic | Orchestration only |
| Knowledge of domain | Yes — uses domain types | Yes — calls domain types |
| Knowledge of infrastructure | No | Minimal (through interfaces) |
| Examples | `TransferFundsService` | `TransferFundsCommandHandler` |

---

## Q24. What is a Repository in DDD and how does it differ from a data-access layer?

**Concepts**
- Repository as an in-memory Aggregate collection abstraction
- Interface in Domain layer, implementation in Infrastructure
- Aggregate-level load and save versus row-level operations
- Repository per Aggregate Root rule
- CQRS read model as the alternative for read-heavy scenarios

**Answer**

A Repository in DDD is an abstraction that acts as an in-memory collection of Aggregate Roots — callers add, remove, and retrieve Aggregates from it using domain-meaningful queries, without knowing anything about the underlying storage mechanism. A data-access layer (DAL), by contrast, is typically a set of classes for executing queries and CRUD operations, often exposing the database structure rather than the domain model. The Repository abstraction keeps the domain layer clean: `IOrderRepository` is defined in the domain project and depends only on domain types; the concrete implementation using Entity Framework Core lives in the infrastructure layer. Repositories should operate at the Aggregate level — you load a complete Aggregate, change it, and save it back; they do not expose raw query methods like `GetByFirstName` or `GetWhere(x => x.Status == ...)` that would encourage bypassing the Aggregate boundary. A common misuse is creating a repository for every Entity inside an Aggregate — only Aggregate Roots should have repositories. `OrderLineRepository` in a model where `OrderLine` is inside the `Order` Aggregate is a sign that the boundaries are not respected. In read-heavy scenarios, DDD often recommends using a separate query model (CQRS) instead of going through the repository, since forcing all reads through a fully reconstructed Aggregate is unnecessarily expensive.

---

## Q25. What is a Factory in DDD, and when should you use one instead of a constructor?

**Concepts**
- Factory isolating complex construction logic from the object itself
- Static factory method on the Aggregate Root as the common pattern
- Dedicated Factory class for multi-step or multi-subtype creation
- Factory for reconstitution from stored data
- Constructor complexity as the trigger for a Factory

**Answer**

A Factory in DDD is a method or class responsible for creating complex domain objects — particularly Aggregates — when the construction logic is complex enough to warrant isolation from the object itself. A Factory ensures that created objects are always in a valid initial state, which is especially important for Aggregates with multiple required dependencies or complex invariant checks at creation time. I use a Factory when constructing an Aggregate requires fetching data, making decisions based on business rules, or setting up a web of related objects — putting all that logic in a constructor makes the constructor hard to read, test, and evolve. Factory methods on the Aggregate Root itself such as static `Create(...)` methods are a common and readable DDD pattern in C#, communicating intent better than a constructor with many parameters. A dedicated Factory class makes sense when creation involves multiple steps, produces different subtypes based on input, or when the creation logic itself is a domain concept such as a `LoanApplicationFactory` that applies eligibility rules during construction. Factories are also important for reconstitution — recreating an Aggregate from stored data — which is typically handled by the ORM but sometimes requires a custom factory when the mapping is non-trivial.

---

## Q26. What is an anemic domain model and why is it considered an anti-pattern?

**Concepts**
- Anemic domain model as property-bag Entities with no business methods
- Invariant unenforceability as the core problem
- Duplicated business logic across multiple service classes
- ORM-first design as a common cause
- Private setters and intent-revealing methods as the fix

**Answer**

An anemic domain model is one where the domain objects — Entities, Aggregates — contain only data with public properties and getters and setters, while all business logic lives in separate service classes that read from and write to those objects. Martin Fowler coined the term as an anti-pattern because it reproduces a procedural programming style inside an object-oriented structure. The problem with the anemic model is that invariants cannot be enforced: if `Order` is just a bag of properties, any code anywhere in the application can set `order.Status = "Shipped"` without checking whether the order is paid, regardless of the business rule that says only paid orders can be shipped. Anemic models result in duplicated logic: if the same business rule needs to apply in multiple places, it must be copy-pasted into each service that touches the object, creating divergence over time. It is easy to slide into the anemic model with ORM-first development — developers model their classes to match database tables with lots of public setters for EF Core mapping and then put logic in services, ending up with the anti-pattern without intending to. The fix is to move business logic back into the domain objects, making setters private or removing them, and exposing intent-revealing methods like `order.Ship()`, `order.Cancel(reason)`, and `account.Withdraw(amount)` that enforce the rules internally.

---

## Chapter 7 — Layered Architecture & Clean Architecture in DDD

---

## Q27. What are the standard layers in a DDD-based application, and what are the dependency rules between them?

**Concepts**
- Domain layer with no dependencies as the most stable layer
- Application layer depending only on Domain
- Infrastructure layer implementing Domain and Application interfaces
- Presentation layer as the entry point calling Application
- Dependency Inversion Principle mapping to project references

**Answer**

A DDD-based application is typically structured in four layers — Domain, Application, Infrastructure, and Presentation — with a strict dependency rule: outer layers depend on inner layers, and the Domain layer has no dependencies on any other layer. The Domain layer is the most stable and the most important — it changes only when business rules change, never because a database or framework was swapped out. The Infrastructure layer implements the interfaces defined in the Domain layer — Repository interfaces, notification interfaces — following the Dependency Inversion Principle so that the domain never imports an ORM or messaging library. In Clean Architecture (Robert C. Martin's formalization of this idea), the same principle is described as the "Dependency Rule": source code dependencies always point inward toward higher-level policies, and the domain is the innermost ring.

| Layer | Responsibility | Depends on |
|---|---|---|
| Domain | Entities, Value Objects, Aggregates, Domain Events, Domain Services, Repository interfaces | Nothing (pure domain logic) |
| Application | Use case orchestration, Command/Query handlers, Application Services | Domain |
| Infrastructure | Repository implementations, DB context, message bus, external API clients | Domain, Application (through interfaces) |
| Presentation | Controllers, gRPC endpoints, message consumers, CLI | Application |

---

## Q28. Why should the domain model have no knowledge of infrastructure concerns like databases or HTTP clients?

**Concepts**
- Infrastructure as a detail that changes independently of business rules
- ORM attribute coupling as a Single Responsibility violation
- Infrastructure-free domain enabling pure unit tests
- Interface-based abstraction for infrastructure dependencies
- Domain portability across hosting contexts

**Answer**

The domain model must remain independent of infrastructure because infrastructure is a detail — it can change (switching from SQL Server to PostgreSQL, from REST to gRPC) without changing the business rules. If the domain model imports Entity Framework attributes or `HttpClient` directly, every database migration or API change forces changes to the domain layer, eroding its stability and testability. A domain model that imports `[Column]` or `[Table]` attributes from Entity Framework is coupled to the ORM: changing the mapping strategy requires modifying domain classes, violating the Single Responsibility Principle. Infrastructure dependencies make unit testing the domain hard because tests must set up database connections or HTTP servers rather than just instantiating a domain object and calling a method. The practical pattern is to define interfaces in the domain layer such as `IEmailSender` and `IOrderRepository` and implement them in the infrastructure layer; the domain never sees the concrete implementation, only the contract. Keeping the domain model infrastructure-free also enables running it in completely different hosting contexts — the same domain can be used in an ASP.NET Core web service, a console app, or a test harness without modification.

---

## Chapter 8 — DDD with Microservices, CQRS & Sagas

---

## Q29. How do bounded contexts map to microservices, and what are the risks of mapping them one-to-one?

**Concepts**
- One bounded context per microservice as a sensible default
- Premature microservice extraction creating chatty trivial services
- Modular Monolith as a valid intermediate step
- Multiple bounded contexts in one service when contexts are small
- One service owning one bounded context as the non-negotiable rule

**Answer**

Bounded contexts and microservices are complementary concepts but not identical. A bounded context defines a semantic boundary around a coherent domain model; a microservice is a deployable unit with its own process and database. In an ideal architecture, each microservice owns exactly one bounded context, but the mapping does not have to be one-to-one. One bounded context per microservice is a sensible default because it ensures each service has a coherent, internally consistent model and owns its own data — the service boundary and the model boundary align. The risk of forcing a strict one-to-one mapping prematurely is creating microservices that are too small and too chatty: each service is a trivial CRUD endpoint, services must constantly call each other to complete any meaningful operation, and the overhead of distributed systems is paid without the benefits of true isolation. Some bounded contexts start as modules within a monolith and are extracted into separate services later when team size, deployment frequency, or scaling requirements justify the operational overhead — starting as a "modular monolith" is a valid DDD-compatible approach. Multiple bounded contexts in one service is also valid when the contexts are small or closely related; the key constraint is that one service should never own the model of another service's bounded context.

---

## Q30. How does DDD integrate with Command Query Responsibility Segregation (CQRS)?

**Concepts**
- DDD command side using full Aggregate and Domain Event stack
- CQRS query side bypassing domain model for read-optimized DTOs
- DDD Domain Events as the source of CQRS read-side projections
- Aggregate invariant enforcement on writes versus flat DTO on reads
- Event sourcing making CQRS nearly mandatory

**Answer**

CQRS separates write operations (Commands that change state) from read operations (Queries that return data). DDD and CQRS complement each other naturally: the Command side uses the full DDD stack — Aggregates, Domain Events, Repositories — to enforce business invariants on writes, while the Query side bypasses the domain model entirely and reads directly from optimised read models or database views. On the Command side, a `PlaceOrderCommand` handler loads the `Order` Aggregate from its Repository, calls `order.Place(items, shippingAddress)`, handles any Domain Events raised, and saves via the Repository — the entire DDD model is engaged. On the Query side, a `GetOrderSummaryQuery` handler skips the Repository and Aggregate entirely, querying a read-optimised view or a separate read database directly and returning a flat DTO — no domain objects are involved. This split solves a real problem: loading a fully constructed Aggregate with all its invariant-preserving complexity is wasteful for a simple "show the order list" query, since read models are simpler, faster, and optimised for the UI's needs. In event-sourced architectures, CQRS is nearly mandatory: events are the write model, and the read models are projections built by replaying those events — DDD Domain Events become the source of truth that feeds read projections.

---

## Q31. What is the Saga pattern and how does it relate to DDD's aggregate boundaries and eventual consistency?

**Concepts**
- Saga as a long-running process spanning multiple Aggregates
- Choreography-based versus orchestration-based Saga
- Compensating transactions as the Saga rollback mechanism
- Saga correlation ID for tracking process state
- Consumer idempotency required by Saga retry semantics

**Answer**

A Saga is a pattern for managing long-running business processes that span multiple Aggregates or bounded contexts, coordinating a sequence of steps where each step is a local transaction on one Aggregate. Because DDD's one-transaction-one-aggregate rule prevents multi-aggregate transactions, the Saga compensates for failures by running compensating transactions that undo previous steps. There are two Saga implementations: choreography-based Sagas where each service listens for events and decides its next action independently, and orchestration-based Sagas where a central Saga orchestrator sends commands to each participant and reacts to their replies. Choreography Sagas are simpler and more decoupled but harder to trace — understanding the full process flow requires reading each service's event handlers. Orchestration Sagas centralise the flow logic in one place, making it easier to monitor and debug but introducing a coordination bottleneck. Compensating transactions are the Saga's rollback mechanism: if `ConfirmInventory` succeeds but `ChargePayment` fails, the Saga sends `ReleaseInventory` to undo the reservation. Compensating transactions must be designed as domain operations — `inventory.ReleaseReservation(orderId)` — not as database rollbacks. Sagas require careful attention to idempotency: steps may be retried, so each participant must handle receiving the same command twice without duplicating side-effects, typically by tracking which correlation IDs have already been processed.

---

## Q32. What are the most common mistakes developers make when applying DDD in practice?

**Concepts**
- Skipping strategic design and jumping straight to tactical patterns
- Anemic domain model from ORM-first development
- Over-engineering simple domains with full tactical DDD
- God Aggregates from insufficient boundary discipline
- Ubiquitous language drift eroding communication benefits

**Answer**

DDD is widely misapplied because its tactical patterns look simple in isolation but require disciplined judgment to apply correctly. The most common mistakes involve applying the patterns mechanically without the underlying strategic thinking that gives them meaning.

Skipping strategic design and jumping straight to tactical patterns produces tactically correct but strategically incoherent models — writing Entities, Value Objects, and Repositories without first defining bounded contexts and ubiquitous language means the model grows messy over time. Building anemic domain models by creating Entity classes with nothing but public getters and setters and putting all logic in service classes defeats the purpose of DDD, since business rules are scattered rather than encapsulated. Over-engineering simple domains by applying full tactical DDD to a straightforward CRUD application adds complexity without business benefit — DDD is justified by domain complexity, not by architectural preference. Making Aggregates too large by including every related concept in one Aggregate to avoid eventual consistency creates contention, performance problems, and a God Object that is hard to change. Using database-generated IDs and leaking ORM concerns into the domain prevents Domain Events from referencing the Entity's ID before it is saved, and ORM attributes on domain classes couple the model to the persistence framework. Ignoring the Ubiquitous Language after the initial design is also common — if code names drift from the business vocabulary over time, the domain model stops reflecting the real domain and the communication benefits of DDD are lost.

---

## Gotchas — Domain-Driven Design (Interview Traps)

---

#### Gotcha 1. Aggregate Boundary Drawn Too Large

**Concepts**
- Aggregate as a consistency boundary, not a grouping of related entities
- Over-inclusion causing database contention and large transaction scope
- One Aggregate per request as the design heuristic
- Small aggregates communicating via domain events

**Answer**

A common DDD mistake is drawing aggregate boundaries around everything that belongs together semantically — for example, making `Order` contain `Customer`, `Product`, `Inventory`, and `Payment` in one aggregate — which creates a giant object that must be locked and loaded in its entirety for every operation. The aggregate boundary should be defined by the consistency requirement: what must be transactionally consistent together? An `Order` needs to be consistent within its own line items, but `Customer` and `Inventory` have their own independent invariants and belong in separate aggregates communicating via domain events. Large aggregates cause performance problems, concurrency conflicts, and God-Object complexity.

---

#### Gotcha 2. Value Object Compared by Identity Instead of Structural Equality

**Concepts**
- Value Object equality based on all property values
- Entity equality based on identity (ID)
- Missing Equals and GetHashCode override on Value Object
- Two Money(10, "USD") instances must be equal

**Answer**

A Value Object class that does not override `Equals` and `GetHashCode` uses reference equality by default, so two `Money(10, "USD")` instances are not equal even though they represent the same value — which breaks collection membership checks, deduplication, and domain invariant comparisons that depend on value equality. In C# the canonical solution is to make Value Objects `record` types (which auto-generate structural equality) or to implement `IEquatable<T>` with a complete `Equals` override that compares all constituent properties. Interviewers test this by asking "how would you implement a `Money` or `Address` value object?" and expect to see equality by value, immutability, and no identity field.

---

#### Gotcha 3. Repository Defined Per Entity Instead of Per Aggregate Root

**Concepts**
- Aggregate Root as the only gateway to the aggregate
- Bypassing the root by loading child entities directly
- Repository interface for every entity breaking encapsulation
- Invariants enforced at the root becoming unreachable

**Answer**

Creating `IOrderLineRepository`, `IOrderHeaderRepository`, and `IOrderRepository` as separate repositories for parts of the same aggregate allows callers to load and modify `OrderLine` records directly without going through the `Order` aggregate root — any invariant the root enforces (minimum one line, total quantity limit) is bypassed. The rule is one repository per aggregate root: `IOrderRepository` loads and saves the entire `Order` aggregate, including its `OrderLines` collection, and nothing reaches inside the aggregate except through the root's public methods. DDD repositories encapsulate persistence of the entire consistency boundary, not individual tables.

---

#### Gotcha 4. Domain Events Published After the Database Transaction Commits

**Concepts**
- In-process domain event dispatch before transaction commit
- Integration event published after commit via outbox
- Lost event when process crashes between commit and publish
- Transactional outbox pattern as the reliable solution

**Answer**

Publishing domain events directly to an in-memory dispatcher after `SaveChangesAsync()` commits means a process crash between the commit and the publish loses the event permanently — the aggregate state changed but no downstream subscriber was notified. The reliable approach is the Transactional Outbox Pattern: the domain events are written to an `OutboxMessages` table within the same database transaction as the aggregate changes, and a separate background worker reads uncommitted outbox records and publishes them to the message broker, marking them processed. This guarantees at-least-once delivery because the outbox record survives a crash and the worker retries on restart.

---

#### Gotcha 5. Using Database-Generated IDs in Domain Events Before Save

**Concepts**
- Database auto-increment ID unknown until after INSERT
- Domain Event referencing the entity ID at creation time
- Client-generated GUID allowing ID to exist before persistence
- Domain Event raised inside the constructor carrying a valid ID

**Answer**

If an `Order` aggregate uses a database-generated integer identity and raises an `OrderCreatedEvent` in its constructor, the event carries `Id = 0` because the database has not yet assigned an ID — any subscriber that tries to load the order by that ID gets nothing. The DDD solution is to use client-generated GUIDs (`Guid.NewGuid()`) as entity identifiers, assigned in the constructor before any domain events are raised, so the ID is a stable value from the moment the aggregate is created regardless of when it is persisted. This is one of the reasons DDD practitioners favour GUIDs over database sequences.

---

#### Gotcha 6. Sharing a Domain Entity Across Multiple Bounded Contexts

**Concepts**
- Bounded context as a semantic isolation boundary
- Shared entity accumulating contradictory properties from both contexts
- Change in one context breaking the other
- Anti-Corruption Layer translating across context boundaries

**Answer**

Placing the same `Customer` class in a shared library and using it in both the `Ordering` bounded context and the `Billing` bounded context creates tight coupling — when Billing needs to add `TaxId` to `Customer`, it modifies the shared class and must retest Ordering; when Ordering adds `PreferredDeliveryWindow`, it touches the shared class that Billing also owns. Each bounded context must have its own model of `Customer` with only the properties relevant to that context; when contexts need to communicate, they translate via domain events or an Anti-Corruption Layer, never by sharing a single class. This is the "Shared Kernel" boundary in DDD — only things explicitly agreed upon by both teams may be shared.

---

#### Gotcha 7. Ubiquitous Language Drifting From the Code Over Time

**Concepts**
- Ubiquitous Language as the shared vocabulary between domain experts and code
- Code divergence from business terms causing misalignment
- Method names using technical jargon instead of business language
- Living Glossary as a mitigation practice

**Answer**

DDD's ubiquitous language is not a one-time design artefact — it must stay alive in the code throughout the project lifecycle. When class names like `OrderProcessor`, method names like `DoOrderThing()`, and variable names like `obj` replace the business terms `OrderFulfillment`, `Confirm()`, and `shipment`, domain experts and developers can no longer read the code together and the model stops being useful for communication. The discipline is to rename code immediately whenever business language evolves, hold periodic code reviews with domain experts, and maintain a living glossary in the repository. Interviewers ask "how do you keep the ubiquitous language consistent?" and expect practices beyond just the initial modelling session.

---

#### Gotcha 8. ORM Attributes Placed on Domain Entities

**Concepts**
- Domain entity polluted with persistence concerns
- [Column], [Table], [Key] attributes coupling Domain to EF Core
- EF Core Fluent API as the clean alternative
- Infrastructure layer owning mapping configuration

**Answer**

Adding `[Table("orders")]`, `[Column("customer_id")]`, or `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` attributes directly on a Domain Entity couples the domain model to EF Core — the Domain project must reference `Microsoft.EntityFrameworkCore` to compile, which violates the Dependency Rule. The clean alternative is to use EF Core's Fluent API in an `EntityTypeConfiguration<T>` class inside the Infrastructure project, which maps domain properties to database columns with no attributes on the domain class itself. This keeps the Domain project free of any ORM dependency and allows the domain model to evolve independently of the persistence schema.

---

#### Gotcha 9. Domain Service Used as a Catch-All for Business Logic

**Concepts**
- Domain Service for operations involving multiple aggregates
- Business logic that belongs inside an aggregate method placed in Domain Service
- Anemic aggregate as the result of over-using Domain Services
- Domain Service as the last resort, not the first choice

**Answer**

Domain Services are intended for business operations that genuinely span multiple aggregates or require input from external services to enforce a domain rule — for example, a uniqueness check that queries the repository to ensure no duplicate order exists. When developers place logic that belongs inside an aggregate (price calculation, status transitions) into a Domain Service, the aggregate becomes anemic and the Domain Service becomes a procedural script. The rule of thumb is: if the logic only requires the state of one aggregate, it belongs as a method on that aggregate; if it requires coordinating two aggregates or calling a repository for a cross-entity constraint, a Domain Service is appropriate.

---

#### Gotcha 10. Bounded Context Without an Anti-Corruption Layer

**Concepts**
- Legacy system imposing its own model on the new context
- ACL translating external concepts into local ubiquitous language
- Conformist pattern as an alternative with known trade-offs
- Bubble context protection via adapters and translators

**Answer**

When integrating with a legacy CRM or an external payment provider, blindly mapping their data structures and terminology into your bounded context lets the external model pollute your domain — field names like `Cust_Ref_No`, statuses like `STAT_3`, and date formats from the legacy system appear in domain entities and ubiquitous language. An Anti-Corruption Layer (ACL) sits between your context and the external system and translates their concepts into your ubiquitous language: their `Cust_Ref_No` becomes your `CustomerId`, their `STAT_3` becomes your `OrderStatus.Cancelled`. Without an ACL, changes in the external system cascade directly into your domain model, and your codebase becomes infected with external terminology that domain experts cannot understand.

---
