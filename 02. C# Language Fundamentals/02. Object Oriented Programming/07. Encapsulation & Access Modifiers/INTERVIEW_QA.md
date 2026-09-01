# C# Encapsulation & Access Modifiers — Interview Q&A

## Foundation Questions

---

## Q1. What is encapsulation, and why is it a core principle of OOP?

**Concepts**
- Bundling data and behavior in one unit
- Hiding implementation details
- Exposing only a controlled interface
- Invariant enforcement at class boundary
- Reducing coupling between components

**Answer**

Encapsulation bundles an object's state (fields) and the behavior that operates on that state (methods) inside a single class, while restricting direct access to the internal state from outside. The purpose is twofold: it protects invariants (the rules the object must always satisfy — for example, a bank account balance must never be negative) by routing all state changes through controlled methods, and it decouples the internal representation from the public interface, allowing the implementation to change without breaking callers. When `Balance` is a public settable field, any code in the application can write `account.Balance = -10000m`, bypassing all business rules. Making `Balance` a read-only property backed by a private field and exposing only `Deposit()` and `TryWithdraw()` ensures that every balance change goes through validated paths. Encapsulation is therefore the foundation on which the correctness guarantees of a domain model rest.

---

## Q2. What are the six access modifiers in C#, and what visibility does each grant?

**Concepts**
- `public` — visible everywhere
- `private` — visible only within the declaring type
- `protected` — visible within the type and derived types
- `internal` — visible within the same assembly
- `protected internal` — visible in same assembly OR derived types
- `private protected` — visible in same assembly AND derived types

**Answer**

C# provides six access modifiers with progressively expanding visibility. `private` is the most restrictive — accessible only inside the declaring class or struct body. `private protected` widens slightly to allow access within derived classes that are also in the same assembly. `protected` allows derived classes in any assembly to access the member. `internal` grants access to all code within the same compiled assembly, regardless of inheritance. `protected internal` is the union of `protected` and `internal` — accessible in the same assembly or in derived classes anywhere. `public` is unrestricted. The default access when no modifier is specified is `private` for class members and `internal` for top-level types. Choosing the most restrictive access that satisfies the design need is the guiding principle: it minimizes the surface area over which invariants must be defended and makes the public API easier to reason about.

---

## Q3. What is the default access modifier for class members and top-level types if none is specified?

**Concepts**
- Class members default to `private`
- Top-level types (class, struct, enum) default to `internal`
- Interface members default to `public` (before C# 8)
- Namespace members (classes) default to `internal`
- Explicit is better than implicit for clarity

**Answer**

For members declared inside a class or struct (fields, methods, properties, nested types), the default access is `private` when no modifier is written. This is a sensible default: newly added members are unexposed until you explicitly widen access. For top-level type declarations (a class, struct, interface, or enum not nested inside another type), the default is `internal`, making the type visible only within its assembly. Interface members prior to C# 8 defaulted to `public abstract`. Relying on implicit defaults is not recommended in production code; explicit modifiers communicate intent and prevent accidental public exposure when a member is added during a refactor and the developer forgets to add a modifier. Code analysis rules (CA1051, SA1400) can be enabled to enforce explicit access modifiers on all members.

---

## Q4. How do properties enforce encapsulation better than public fields?

**Concepts**
- Property adds get/set logic layer
- Validation on assignment via set accessor
- Lazy computation in get accessor
- Binary compatibility — changing field to property is breaking; property to property is not
- Change notification (INotifyPropertyChanged) possible only with property

**Answer**

A public field exposes raw memory with no ability to intercept reads or writes. A property provides two code hooks — the `get` accessor and the `set` accessor — where you can add validation, transformation, change notification, or lazy initialization without changing the caller's syntax. Making `Balance` a public `decimal` field means any code can set it to any value. Making it a property with a `private set` or `init` forces all writes through methods that enforce the non-negative invariant. Properties also allow future implementation changes without breaking API consumers: replacing a simple computed property expression with a complex database-backed lookup is invisible to callers as long as the signature stays the same. Changing a public field to a property is a binary-incompatible change (the IL for field access is `ldfld`/`stfld`, while property access is `callvirt`) — an important reason to never expose fields publicly in library APIs.

---

## Q5. What is the `internal` modifier, and how does it support assembly boundary encapsulation?

**Concepts**
- Visible within the same compiled assembly only
- Hides implementation details from consumers of a NuGet package
- Multiple classes cooperating without exposing to external callers
- `InternalsVisibleTo` as the controlled exception
- Assembly as the natural encapsulation unit

**Answer**

The `internal` modifier restricts visibility to the assembly boundary — all code within the same `.dll` or `.exe` can access the member, but code in other assemblies cannot. This is the correct tool for implementation types that support the library's public API without being part of it. For example, a NuGet package may expose `IOrderRepository` and `OrderService` as `public` while keeping `SqlOrderQuery`, `OrderMapper`, and their unit-of-work types as `internal`. Consumers of the package see a clean public surface and cannot depend on the internal types, giving the library team freedom to refactor internals without a breaking change. Multiple `internal` types in the same assembly can collaborate freely, effectively treating the assembly as an encapsulation unit broader than a class but narrower than "the whole application."

---

## Q6. What is `InternalsVisibleTo`, and what are its risks?

**Concepts**
- Grants a named assembly access to `internal` members
- Added to `AssemblyInfo.cs` or csproj `<ItemGroup>`
- Primary use: test project accessing internal implementation
- Risks: broadening internal surface, coupling tests to implementation
- Strong-name signing requirement for strongly-named assemblies

**Answer**

`InternalsVisibleTo` is an assembly-level attribute that grants a specific, named assembly access to all `internal` members: `[assembly: InternalsVisibleTo("Billing.Tests")]`. Its primary legitimate use is allowing a unit-test project to test `internal` classes without making them `public`. The risks are: (1) it permanently widens the internal surface — other developers can add more `internal` types knowing the test assembly sees them; (2) it couples tests to implementation details, making tests fragile when internal structure changes; (3) in strongly-named assemblies, the friend assembly must also be strongly named or the attribute must include the public key, which adds configuration overhead. The guardrails to apply before adding a friend assembly are: confirm that the class truly cannot be tested through its public API; limit `InternalsVisibleTo` to test projects only, never to other production assemblies; and review whether the internal type should be made `public` with a documented contract or redesigned to be testable through a public seam.

---

## Q7. What is the `readonly` keyword on a field, and how does it differ from a property with only a `get` accessor?

**Concepts**
- `readonly` field: assigned only in declaration or constructor
- Read-only property: backed by logic, no `set` accessor
- `readonly` field is more performant (no method call)
- Read-only property can have computed or lazy-loaded value
- Both enforce immutability post-construction

**Answer**

A `readonly` field is a field that can only be assigned in its declaration or within a constructor of the declaring class. After the constructor completes, any attempt to assign the field — even within other methods of the same class — is a compile error. A property with only a `get` accessor (either explicit `{ get; }` or expression-bodied `=> expression`) is immutable from the caller's perspective but may compute its value each time it is read. The `readonly` field is typically more performant because the value is read directly from memory rather than through a method call and any associated logic. For public API design, a read-only property is usually preferred over a `public readonly` field because it maintains binary compatibility if the implementation later needs to change from a stored value to a computed value. For private or protected internal state that will always be a simple stored value, `readonly` fields are idiomatic.

---

## Q8. How does `protected` access in a base class create an encapsulation risk for derived classes?

**Concepts**
- `protected` fields accessible in derived classes
- Derived class can bypass base invariants directly
- Breaking encapsulation across the inheritance boundary
- Prefer `protected` methods over `protected` fields
- Template Method Pattern as structured protected access

**Answer**

`protected` fields are accessible to any derived class in any assembly, which means a derived class can modify the field value directly, bypassing any validation or audit logic in the base class's methods. If `Employee._baseSalary` is `protected`, a `CommissionEmployee` can set `_baseSalary = minimum` directly, bypassing the `ApplyRaise` method that records the audit trail and enforces non-negative constraints. The derived class has effectively broken the base class's invariants from the outside. The safer design is to make fields `private` and expose `protected` virtual or abstract methods that provide structured access — the Template Method Pattern. For example, `protected virtual void OnSalaryChanging(decimal newSalary)` gives derived classes a hook to react to salary changes through a controlled interface, without direct field access. The rule of thumb: `protected` fields should be treated with the same caution as `public` fields, because the derived class surface is as broad as the entire class hierarchy.

---

## Q9. What are immutable DTOs, and why are they preferred for cross-service messaging?

**Concepts**
- Immutable: state cannot change after construction
- `init`-only setters (C# 9)
- Private constructor + factory for full control
- Thread-safe by design (no mutation = no race)
- `with` expression for non-destructive mutation

**Answer**

An immutable DTO is one where all state is set at construction and cannot be modified afterward. This is the safest contract for messages exchanged between services: a message received should represent a fixed snapshot, not a mutable object that downstream handlers might accidentally modify. Immutability is also thread-safe by design — if no code can write to the object, concurrent reads are always safe. In C# 9+, init-only setters (`{ get; init; }`) allow object-initializer syntax at construction but block any post-construction writes. Records provide built-in immutability with `with` expressions for creating modified copies: `var updated = original with { Status = "Shipped" }`. For strict invariant enforcement, a private constructor with a factory method is the strongest approach: `MemberProfileDto.Create(id, email, roles)` validates arguments and constructs the object, and no other path exists to create an invalid instance. This is preferable to `init`-only with a mutable `List<T>` field, which is only "superficially immutable" — the list reference is fixed but its contents remain mutable.

---

## Q10. What is the difference between `protected internal` and `private protected`?

**Concepts**
- `protected internal` — same assembly OR derived type (union)
- `private protected` — same assembly AND derived type (intersection)
- `protected internal` is wider than either alone
- `private protected` is narrower than `protected`
- Use case: limiting extension hooks to first-party derived types

**Answer**

`protected internal` is the union of `internal` and `protected`: a member is accessible to any code in the same assembly and to any derived class in any assembly. This is appropriate for members that serve both internal collaborators within the assembly and subclass authors in external code. `private protected` is the intersection: accessible only within derived classes that are also in the same assembly. It is strictly narrower than `protected` and strictly narrower than `internal`. This modifier is useful when you want to provide a hook for derived classes while ensuring those derived classes must be compiled into the same assembly — effectively preventing third-party inheritance from accessing internal scaffolding. A practical example: `private protected virtual void OnInternalStateChanged()` can be overridden by first-party implementations in the same assembly but is completely invisible to external extension points, preventing unintended use or subclassing from consumers of a NuGet package.

---

## Q11. How does exposing a `List<T>` property break encapsulation, and how do you fix it?

**Concepts**
- Reference to mutable collection leaks through public API
- External code can add/remove without class's knowledge
- `IReadOnlyList<T>` or `IReadOnlyCollection<T>` as public type
- Return a defensive copy or wrap in `AsReadOnly()`
- Encapsulation invariant: class controls all mutations

**Answer**

Returning a `List<T>` as a public property exposes the internal mutable collection by reference. Any caller receiving the list can call `Add`, `Remove`, `Clear`, or `Sort` on it, mutating the class's internal state without going through any of the class's methods. Business rules enforced in methods like `AddRole(string role)` can be bypassed entirely. The minimal fix is to change the property type to `IReadOnlyList<T>` or `IReadOnlyCollection<T>` and return `_roles.AsReadOnly()`, which wraps the list in a read-only facade without copying. For stronger immutability, return an `ImmutableList<T>` or return a defensive copy (`_roles.ToList()`) when callers absolutely need their own mutable copy. The property's backing field remains a `List<T>` or `HashSet<T>` for efficient internal mutation. The class continues to control all structural changes, maintaining invariants such as "no duplicate roles" or "roles list cannot be empty."

---

## Q12. What is the "anemic domain model" anti-pattern, and how does encapsulation fix it?

**Concepts**
- Anemic domain model: public setters, logic in service layer
- Business rules scattered outside the entity
- Encapsulation moves logic into the entity
- Rich domain model: behavior co-located with state
- Validation at construction and state-change methods

**Answer**

An anemic domain model is one where domain entities are plain data containers with public getters and setters and no behavior — all business logic lives in service classes outside the entity. For example, `Employee` has `public decimal BaseSalary { get; set; }` and a separate `SalaryService` that applies raises, validates amounts, and writes audit entries. This is the opposite of encapsulation: the entity's state is fully public, invariants are enforced inconsistently (each service must remember to validate), and the rules for changing salary are spread across multiple files. The fix is to move behavior into the entity: make `_baseSalary` private, expose `ApplyRaise(decimal percent)` that validates the percent, updates the field, and appends to the audit trail, and provide a read-only `BaseSalary` property. The entity becomes the single authority for its own state changes, making business rules impossible to bypass and impossible to forget to apply.

---

## Gotcha Questions

---

## Q13. A `BankAccount` has `public decimal Balance { get; set; }`. A support script sets `account.Balance = -10000m`. Why is this an encapsulation failure, and what is the minimal fix?

**Concepts**
- Public `set` exposes raw state mutation
- Bypasses business rule (non-negative balance)
- Fix: `private set` or init-only, expose controlled methods
- Invariant enforcement at boundary
- Thread safety secondary issue

**Answer**

`public decimal Balance { get; set; }` allows any code in the application to write any value to `Balance`, including negative amounts that violate the domain rule that a bank balance must always be non-negative. This is an encapsulation failure: the invariant is asserted nowhere because there is no gatekeeper. The minimal fix is `public decimal Balance { get; private set; }` combined with explicit mutation methods `Deposit(decimal)` and `TryWithdraw(decimal)` that validate their arguments before updating `_balance`. For full immutability from external code, `{ get; init; }` prevents post-construction writes except through object initializers. The support script that previously did `account.Balance = -10000m` must now use the proper method or be replaced with an audited `OverrideBalance(decimal)` method that requires elevated authorization and logs the change. Every path through which `Balance` can change is now explicit, validated, and auditable.

---

## Q14. An `internal class InternalLedger` is returned from a `public static` method. Why does this break the assembly boundary?

**Concepts**
- Returning an `internal` type from a `public` method exposes it via reflection
- Consumers cannot declare the type in their code but can use it at runtime
- API design flaw — public surface should use only public types
- CS0051 compiler error in stricter scenarios
- Fix: expose through a public interface or redesign the return type

**Answer**

A `public static CreateLedger()` returning `InternalLedger` creates a contradiction: the method is publicly visible, so any consuming assembly can call it and receive the object at runtime, but they cannot name the type in their own source code. Depending on whether the compiler catches this (CS0051 is raised when a public method's return type is less accessible), the consuming assembly receives an anonymous-typed reference. They can pass it to other methods in your library that accept it, but they cannot declare a variable of that type, use it with `is`, or reflect on it predictably. This destroys the value of `internal` as an access barrier and makes the public API opaque and unusable. The fix is to return a public interface (`ILedger`) or abstract class that `InternalLedger` implements, keeping the concrete type internal while the abstraction is public. The public surface then communicates the intended contract cleanly.

---

## Q15. A `CommissionEmployee` subclass directly mutates `protected decimal _baseSalary` and clears `protected List<string> _auditTrail`. What invariants are broken, and how do you prevent this?

**Concepts**
- Protected field accessible from derived class bypasses base methods
- `ApplyRaise` validation and audit logging not called
- History erased — silent data loss
- Fix: make fields `private`, expose `protected virtual` methods
- Template Method Pattern for structured extension

**Answer**

`CommissionEmployee.SetGuaranteedMinimum(decimal minimum)` assigns `_baseSalary = minimum` directly, bypassing `ApplyRaise`'s validation (which requires positive percent) and its audit trail entry. More critically, it calls `_auditTrail.Clear()`, erasing all historical records of the employee's salary changes. From an HR compliance perspective, this is catastrophic — the history that must be retained for regulatory reporting is gone. Since `_baseSalary` and `_auditTrail` are `protected`, the compiler does not prevent this access. The fix is to make both fields `private` in `Employee` and add a `protected virtual void SetBaseSalaryDirect(decimal newSalary, string reason)` method that validates the value, appends an audit entry with the reason, and updates the private field. `CommissionEmployee` calls this protected method, which enforces the invariants consistently, and the audit trail is always updated. The private field cannot be bypassed, and the protected method is the only structured access point for derived types.

---

## Real-World Scenarios

---

## Q16. Two assemblies need test access to `internal` pricing helpers. A developer adds `InternalsVisibleTo` to the .csproj. What risks exist, and what guardrails apply?

**Concepts**
- InternalsVisibleTo grants all internal access to friend assembly
- Risk of over-testing implementation details
- Strong-name signing requirement
- Coupling test code to internal structure
- Alternative: test through public API

**Answer**

`InternalsVisibleTo` grants the named test assembly access to every `internal` class and member in the library — there is no granular control over which internals are shared. This creates the risk that tests write directly against internal implementation details (`InternalPriceCalculator.CalculateRaw()`) that are subject to change without notice, making tests brittle. If the library team refactors `InternalPriceCalculator` into two separate classes, the tests break even though the public behavior is unchanged. The guardrails are: (1) use `InternalsVisibleTo` only for test projects, never for production assemblies; (2) prefer testing through the public API where possible — if the pricing behavior is observable through a public endpoint, test it there; (3) if internal testing is genuinely needed, use the attribute but document which internals are legitimately tested; (4) in strongly-named assemblies, specify the public key to prevent unauthorized assemblies from claiming the friend relationship. Revisit `InternalsVisibleTo` usages in code review to ensure internal tests are testing behavior, not implementation structure.

---

## Q17. Design an immutable `MemberProfileDto` for cross-service messaging. Compare init-only with mutable `List<T>` vs private constructor with factory method.

**Concepts**
- `init`-only properties prevent post-construction assignment
- Mutable collection inside init-only DTO is a false immutability
- Private constructor + factory validates and wraps in `IReadOnlyList<T>`
- `with` expression for non-destructive record copies
- Thread safety and serialization compatibility

**Answer**

Proposal A (`MemberProfileDto` with `List<string> Roles { get; init; }`) is only superficially immutable. While the `Roles` reference itself cannot be replaced after construction, the list's contents remain fully mutable: any handler receiving the DTO can call `dto.Roles.Add("admin")` and silently modify the state that other services also reference. This is particularly dangerous when a DTO is shared across threads or queued message handlers. Proposal B uses `IReadOnlyList<string> Roles { get; }` with a private constructor and a `static Create(...)` factory that copies the input enumerable into an `ImmutableList<T>` or wraps it with `AsReadOnly()`. This guarantees no mutation path exists after creation. For JSON deserialization compatibility, add `[JsonConstructor]` on the private constructor in .NET 10 or use a custom `JsonConverter`. The recommendation for cross-service message contracts is Proposal B: the stronger immutability guarantee, validation in the factory method (non-null ID, valid email format, at least one role), and the `IReadOnlyList<T>` public surface communicate the contract's invariants clearly to every service that receives the message.
