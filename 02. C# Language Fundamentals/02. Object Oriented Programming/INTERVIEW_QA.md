# Object-Oriented Programming — Cross-Topic Index

Cross-cutting questions that require reasoning across multiple OOP topics simultaneously; individual subfolder files cover each topic in depth.

## Table of Contents

| # | Topic | File |
|---|-------|------|
| 01 | Classes & Objects | [INTERVIEW_QA.md](01.%20Classes%20%26%20Objects%20-%20Done/INTERVIEW_QA.md) |
| 02 | Properties & Indexers | [INTERVIEW_QA.md](02.%20Properties%20%26%20Indexers%20-%20Done/INTERVIEW_QA.md) |
| 03 | Constructors & Method Overloading | [INTERVIEW_QA.md](03.%20Constructors%20%26%20Method%20Overloading/INTERVIEW_QA.md) |
| 04 | Static Members & Static Classes | [INTERVIEW_QA.md](04.%20Static%20Members%20%26%20Static%20Classes/INTERVIEW_QA.md) |
| 05 | Inheritance & Polymorphism | [INTERVIEW_QA.md](05.%20Inheritance%20%26%20%20Polymorphism/INTERVIEW_QA.md) |
| 06 | Abstract Classes & Interfaces | [INTERVIEW_QA.md](06.%20Abstract%20Classes%20%26%20Interfaces/INTERVIEW_QA.md) |
| 07 | Encapsulation & Access Modifiers | [INTERVIEW_QA.md](07.%20Encapsulation%20%26%20Access%20Modifiers/INTERVIEW_QA.md) |
| 08 | Events | [INTERVIEW_QA.md](08.%20Events/INTERVIEW_QA.md) |
| 09 | OOP Real-World Examples | [INTERVIEW_QA.md](09.%20OOP%20Real-World%20Examples/INTERVIEW_QA.md) |

---

## CQ1. How do the five SOLID principles depend on each other, and where do they conflict?

**Concepts**
- Single Responsibility Principle — one reason to change
- Open/Closed Principle — extension without modification
- Liskov Substitution Principle — subtype behavioural contracts
- Interface Segregation Principle — narrow, role-specific interfaces
- Dependency Inversion Principle — depend on abstractions

**Answer**

The SOLID principles are mutually reinforcing but not free of tension. SRP pushes you to split classes into focused units, which naturally produces the small abstractions that ISP and DIP require — once a class has a single job it is easy to describe its contract in a narrow interface. OCP builds on that: stable abstractions are the extension points through which new behaviour is added without touching existing code, so violating SRP (by packing multiple concerns into one class) tends to violate OCP at the same time because any change to either concern forces a recompile. LSP constrains how you use inheritance: a subclass that overrides a method to throw `NotSupportedException` breaks every caller that reasonably expected base-class behaviour, turning polymorphism into a liability. That failure usually signals you should have used interface composition instead, which brings you back to ISP. The practical conflict is between SRP and DRY — aggressively splitting responsibilities can scatter related logic, making the codebase harder to follow. The resolution in .NET 10 practice is to let default interface members absorb shared behaviour rather than duplicating it or collapsing two roles into one class. The DIP sits at the top of the chain: if all collaborators are injected through abstractions, each of the other four principles becomes easier to enforce because no component hard-wires a concrete dependency that would resist change.

---

## CQ2. When should you choose an abstract class over an interface, given that interfaces now support default members in C# (.NET 10)?

**Concepts**
- Abstract class — shared state, constructor chain, protected members
- Interface default members — behavioural sharing without inheritance
- Liskov substitution and single-inheritance constraint
- Template Method pattern vs Strategy pattern
- Versioning and binary compatibility

**Answer**

The practical rule is: choose an abstract class when the shared implementation is inseparable from shared state, and choose an interface when you want to describe a capability a type can acquire independently of its class hierarchy. An abstract class can hold fields, enforce a constructor chain, and expose protected helpers — useful when every subclass needs identical backing data (for example, a `DbContext`-derived class that owns the connection string and change tracker). An interface, even with default members, cannot hold instance fields or call constructors, so it cannot guarantee that state. Default interface members in C# 8+ shifted a large portion of the "reuse" argument toward interfaces: you can now add new members to a published interface without breaking existing implementors, which removes the main versioning reason to choose an abstract class. Where the single-inheritance constraint bites hardest is in domain hierarchies that cross organisational lines — `OrderService` cannot inherit from both `AuditableBase` and `CachingBase`, but it can implement `IAuditable` and `ICacheable` and pick up default implementations from each. The remaining stronghold of abstract classes is the Template Method pattern, where a fixed algorithm skeleton calls abstract hooks that subclasses fill in; that structure requires the class to own the orchestration logic in a way interfaces cannot express. In .NET 10 library design the recommendation is to prefer interfaces for public API contracts and reserve abstract classes for internal framework scaffolding where shared state is genuinely needed.

---

## CQ3. How do encapsulation, inheritance, and polymorphism interact when you make a single design decision — say, exposing a property as `virtual`?

**Concepts**
- Encapsulation — hiding implementation behind a controlled surface
- Virtual dispatch — runtime resolution of overridden members
- Fragile base class problem
- Template Method vs public virtual surface
- Liskov substitution and invariant preservation

**Answer**

Marking a property `virtual` threads through all three pillars simultaneously. From an encapsulation perspective, a virtual member is a deliberate hole in the encapsulation boundary: you are advertising that subclasses may replace the behaviour, which means the base class can no longer reason about the property's return value being stable. If `Order.TotalAmount` is virtual and a subclass overrides it to apply a discount, any base-class method that calls `TotalAmount` internally — perhaps in `IsAffordable()` — now executes subclass logic it did not anticipate. This is the fragile base class problem, and it arises precisely because inheritance breaks encapsulation when virtual surfaces are too broad. Polymorphism then amplifies the issue: once callers hold a reference typed as `Order`, they invoke whichever override is bound at runtime, so the design of the property must satisfy LSP across every possible subclass or callers will receive unexpected values. The standard mitigation is the Template Method pattern: keep the public surface non-virtual (or sealed), expose a protected `virtual` hook with a narrow contract, and let the base method call the hook. This preserves encapsulation — the public API is stable — while allowing inheritance to participate in a controlled way. The design decision of whether to make a member virtual is therefore not just a polymorphism choice; it is simultaneously a statement about what invariants the base class is willing to give up and what behavioural contract every subclass must honour.

---

## CQ4. Describe a late-binding scenario where choosing between `virtual`/`override`, an interface, and a delegate each produces meaningfully different runtime behaviour.

**Concepts**
- Virtual dispatch — vtable-based late binding
- Interface dispatch — type-based method table lookup
- Delegate — first-class function pointer, multicast support
- Events as encapsulated multicast delegates
- Closed vs open generic variance

**Answer**

Consider a notification system. If you model it with a `virtual` method on a `Notifier` base class, late binding is resolved through the vtable: the exact override called depends on the concrete type placed in a `Notifier` reference, but only one implementation runs and the class hierarchy is fixed at compile time — adding a new channel requires a new subclass. If you model it with an `INotifier` interface, the dispatch is still single-target per reference but the implementor is free to be anything — a class, a struct (boxed), or a source-generated proxy — and you gain the ability to combine notification strategies through composition rather than inheritance, including generic variance (`INotifier<out T>`). The most flexible option is a delegate or event: the subscriber list is built at runtime, any number of handlers can be attached, and each handler can come from an entirely different class hierarchy. Events add the encapsulation layer that prevents external code from clearing or replacing the invocation list. The practical difference surfaces when requirements change: switching channels with the virtual approach means changing the object graph (substituting a subclass); switching with an interface means swapping the injected implementation; switching with an event means attaching a new handler — zero modification to existing code. The delegate path is the only one where the decision about how many targets to invoke is deferred entirely to runtime assembly of the invocation list, rather than being embedded in class structure.

---

## CQ5. How does the Static Members topic interact with encapsulation and object identity, and when does static state become an anti-pattern?

**Concepts**
- Static vs instance lifetime and scope
- Encapsulation of shared state
- Thread safety and race conditions
- Testability and dependency isolation
- Singleton pattern vs static class

**Answer**

Static members live for the duration of the AppDomain and are shared across every instance, which means they break the object identity contract that encapsulation is built on. When a class stores mutable data in a static field, every instance observes the same value — there is no per-object encapsulation boundary. This is acceptable for genuinely application-wide constants or utility functions (`Math.Sqrt`, `string.IsNullOrEmpty`) but becomes an anti-pattern the moment the static field holds mutable state: any test that changes the static value leaks state into the next test, any concurrent thread that writes the field must coordinate through a lock, and injecting a different value for a different execution context is impossible without reflection. The Singleton pattern is the canary: a `static Instance` property with a private constructor achieves a controlled single instance, but it is just as testable-hostile as a static field unless the singleton implements an interface and is injected through DI. The .NET 10 recommended alternative is to register the class with a DI container as a singleton scope — the container owns the single instance, tests can substitute a different implementation, and no static mutable state crosses test boundaries. Static classes are legitimate when the behaviour is purely functional (no side effects, no external dependencies), which is why extension methods are defined in static classes: they extend an existing type's surface without coupling to any specific instance or container.
