# C# Inheritance & Polymorphism — Interview Q&A

## Foundation Questions

---

## Q1. What is inheritance in C#, and what does a derived class automatically gain from its base?

**Concepts**
- IS-A relationship
- Single base class (single inheritance)
- All public and protected members inherited
- Private members not accessible but physically present
- `Object` as the ultimate base

**Answer**

Inheritance is a mechanism where a derived class automatically acquires all `public` and `protected` members — fields, properties, methods, events — from its base class without re-declaring them. This represents an IS-A relationship: a `Manager` IS-A `Employee`. C# supports single class inheritance (one direct base class per class) but allows multiple interface implementations. Private members of the base are still part of the object's memory layout but are inaccessible by name in the derived class — they exist to support the base's implementation. The ultimate root of every class hierarchy in C# is `System.Object`, which provides `GetType()`, `ToString()`, `Equals()`, and `GetHashCode()` to all types. Inheritance reduces duplication by placing shared logic and state in the base, but it creates tight coupling, so it should be used only when the IS-A relationship is genuinely true rather than for code reuse alone.

---

## Q2. What is the `base` keyword, and in what two distinct contexts is it used?

**Concepts**
- `base.Method()` — calling overridden base member
- `: base(args)` — invoking base constructor
- Available only in derived class context
- Cannot be used in static methods
- Prevents infinite recursion in override

**Answer**

The `base` keyword refers to the base class of the current object and is used in two distinct contexts. First, as `: base(args)` in a constructor initializer, it calls a specific constructor of the direct base class, ensuring base state is initialized before the derived constructor body runs. Second, as `base.Method()` inside a derived method, it calls the base class implementation of an overridden or hidden method. The most common use of the second form is in overrides: `public override Money CalculateNet() { base.ValidateNonNegative(BaseSalary); return BaseSalary + Bonus; }` — calling `base.ValidateNonNegative` runs the base validation without reimplementing it. If an override omits `base.Method()` when it should call it, shared behavior (validation, audit, logging) in the base is silently skipped. `base` cannot be used in static methods because there is no instance to qualify.

---

## Q3. What is the difference between method overriding (`override`) and method hiding (`new`)?

**Concepts**
- `virtual` + `override` — runtime (dynamic) dispatch via vtable
- `new` modifier — compile-time (static) dispatch hiding
- Reference type determines dispatch for hiding
- Object's runtime type determines dispatch for overriding
- CS0108 warning when hiding without `new`

**Answer**

Method overriding uses the `virtual`/`override` pair to enable runtime polymorphism: when you call a method through a base-class reference, the CLR dispatches to the most-derived override at runtime via the vtable. If `Employee.GetBadgeCode()` is `virtual` and `Manager` overrides it, then calling `GetBadgeCode()` on an `Employee` reference that points to a `Manager` instance invokes `Manager.GetBadgeCode()`. Method hiding with `new` is entirely different: dispatch is determined by the compile-time type of the reference. If `Manager` hides `GetBadgeCode()` with `new`, calling through an `Employee` reference invokes `Employee.GetBadgeCode()` — the manager's implementation is invisible through the base reference. This is why hiding produces wrong badge codes in production: iterating a `List<Employee>` calls the base method for every element, regardless of actual type.

---

## Q4. What is the `virtual` keyword, and what happens if you forget it?

**Concepts**
- Marks a method as overridable
- Enables vtable dispatch entry
- Default — non-virtual (static dispatch)
- Derived class can use `new` to hide without `virtual`
- Retroactively adding `virtual` is a source-compatible but binary-incompatible change

**Answer**

The `virtual` keyword marks a method as overridable, instructing the compiler to insert a vtable entry for dynamic dispatch. Without `virtual`, a method is non-virtual: calls are always dispatched statically to the compile-time type's method. If a base method is non-virtual and a derived class declares a method with the same signature, the compiler emits a CS0108 warning ("hides inherited member — use `new` modifier") and the derived method hides rather than overrides. The critical consequence is that polymorphic code — loops over base-type collections, parameter types — always calls the base implementation. Forgetting `virtual` on a method that should support override is a common source of bugs in inheritance hierarchies, particularly when the base class is designed as an abstraction but its author forgot to annotate polymorphic points. Adding `virtual` later is a source-compatible change but breaks binary compatibility because the vtable slot changes.

---

## Q5. What is the Liskov Substitution Principle, and what C# features help enforce it?

**Concepts**
- Behavioral subtyping
- Derived type fully substitutable for base
- Preconditions, postconditions, invariants
- Throwing from an override breaks LSP
- `sealed`, pattern matching, abstract methods as design guards

**Answer**

The Liskov Substitution Principle (LSP) states that objects of a derived type must be usable wherever objects of the base type are expected, without callers needing to know the difference and without breaking program correctness. A derived class must not strengthen preconditions, weaken postconditions, or violate invariants established by the base. The classic violation is an `InternEmployee` that overrides `CalculateNet()` with `throw new InvalidOperationException()` — callers that invoke `CalculateNet()` polymorphically through an `Employee` reference now get an unexpected exception for some inputs. LSP violations are often a sign that the inheritance relationship is incorrect: `InternEmployee IS-A Employee` may not hold if interns cannot participate in standard payroll. C# helps enforce LSP through `abstract` methods (forcing derived classes to implement behavior rather than inherit a default), `sealed` (preventing unsound further specialization), and by designing base class contracts in XML documentation.

---

## Q6. What is the `sealed` keyword on a class and on a method, and when do you use each?

**Concepts**
- `sealed class` — prevents inheritance
- `sealed override` — stops further overriding of a specific method
- JIT optimization for devirtualization
- Explicit design intent communication
- Cannot seal a non-virtual or non-override method

**Answer**

When applied to a class, `sealed` prevents any other class from inheriting from it. This is appropriate for value-like types where subclassing would break invariants, security-sensitive types, or any type where the designer wants to guarantee no further specialization. `string`, `int`, and most BCL value types are sealed. When applied to a method in a derived class — `public sealed override GetBadgeCode()` — `sealed` prevents further derived classes from overriding that specific method while still permitting inheritance of the class itself. This is used when an override reaches the final intended behavior and further overriding would be unsound. The JIT compiler can also devirtualize calls to sealed methods, eliminating the vtable lookup overhead. You cannot apply `sealed` to a method that is not already an `override`, because only virtual methods participate in the vtable.

---

## Q7. What is the `is` operator and pattern matching, and how do they interact with inheritance?

**Concepts**
- Type test returning `bool`
- Pattern variable binding (`is Type t`)
- `switch` expression with type patterns
- Null-safe — `null is T` always false
- Alternative to `as` + null check

**Answer**

The `is` operator tests whether an object is compatible with a given type at runtime, returning `bool`. In its extended form, it also declares and binds a typed variable: `if (employee is Manager mgr) { ... }`. The bound variable `mgr` is only in scope within the branch where the test succeeded and is guaranteed non-null. Pattern matching in `switch` expressions extends this to discriminated dispatch: `employee switch { Manager m => m.CalculateNet() + m.TeamBonus, ContractEmployee c => c.CalculateNet(), _ => employee.CalculateNet() }`. While patterns are powerful, using explicit type checks in a polymorphic hierarchy is usually a code smell — it should be replaced by a virtual method that each type implements appropriately. Pattern matching is appropriate when the dispatch logic cannot be placed on the type itself, such as in infrastructure or serialization code that must handle types it does not own.

---

## Q8. What is upcasting and downcasting, and what are the runtime risks of each?

**Concepts**
- Upcasting — implicit, always safe
- Downcasting — explicit cast, can throw `InvalidCastException`
- `as` operator — returns null on failure instead of throwing
- `is` check before cast pattern
- Pattern matching as modern replacement

**Answer**

Upcasting converts a derived-type reference to a base-type reference. It is always implicit and always safe because a derived object is guaranteed to have all base members. Downcasting goes the other way — converting a base-type reference to a derived type. It is explicit (requires a cast operator or `as`) and can fail at runtime if the actual object is not the target type. A plain cast `(Manager)employee` throws `InvalidCastException` if `employee` is a `ContractEmployee`. The `as` operator attempts the cast and returns `null` on failure rather than throwing: `var mgr = employee as Manager; if (mgr != null) { ... }`. The modern idiomatic replacement for the `as` + null check pattern is `if (employee is Manager mgr) { ... }`, which is null-safe (null fails the test) and scopes `mgr` to the success branch. Frequent downcasting in a codebase is a strong signal that the inheritance design needs virtual methods to eliminate the type checks.

---

## Q9. What is covariance and contravariance in the context of inheritance and generics?

**Concepts**
- Covariance — `out` type parameter, producer positions
- Contravariance — `in` type parameter, consumer positions
- `IEnumerable<out T>` as canonical covariant example
- `IComparer<in T>` as canonical contravariant example
- Arrays are covariant but unsafely so

**Answer**

Covariance and contravariance describe how subtype relationships between types carry over to generic types. With covariance (`out T`), if `Manager` inherits from `Employee`, then `IEnumerable<Manager>` is implicitly convertible to `IEnumerable<Employee>`. The `out` keyword on a type parameter restricts the type to appear only in output (return) positions, which makes this safe. With contravariance (`in T`), an `IComparer<Employee>` is implicitly assignable where an `IComparer<Manager>` is expected, because a comparer that works on all employees certainly works on managers. The `in` keyword restricts the type to input (parameter) positions. Arrays in C# are covariant without compile-time restrictions: a `Manager[]` can be assigned to `Employee[]`, but writing a non-`Manager` element into it throws `ArrayTypeMismatchException` at runtime — a known design flaw. Generic covariance/contravariance via `in`/`out` is type-safe because the compiler enforces the position restrictions.

---

## Q10. How does method dispatch work in C# — what is a vtable?

**Concepts**
- Virtual method table per type
- Slot per virtual method
- Derived class override replaces slot
- `callvirt` IL instruction for virtual dispatch
- `call` IL instruction for non-virtual dispatch

**Answer**

Every class with virtual methods has a vtable — a per-type array of function pointers, one slot per virtual method. Each instance holds a hidden pointer to its type's vtable. When the runtime encounters a `callvirt` IL instruction (generated for calls on reference types), it follows the vtable pointer from the instance, looks up the slot for the specific method, and calls the function pointer stored there. If a derived class overrides a virtual method, it replaces the corresponding vtable slot with a pointer to its own implementation. Callers reading through the vtable therefore always invoke the most derived implementation, regardless of the declared type of the reference — this is dynamic dispatch. Non-virtual method calls use `call`, which hardcodes the target method at compile time, skipping the vtable lookup. The performance difference between `call` and `callvirt` is small in modern JITs (which devirtualize many calls), but the behavioral difference is the entire foundation of polymorphism.

---

## Q11. What is the fragile base class problem, and how does it manifest in C#?

**Concepts**
- Adding/changing a virtual method in base breaks derived classes
- Derived classes depend on implementation details of base
- `new` virtual in base with same name as hidden member in derived
- Binary compatibility broken when adding a virtual override
- Composition-over-inheritance as mitigation

**Answer**

The fragile base class problem occurs when a change to a base class unexpectedly breaks derived classes that were correct before the change. In C#, adding a new virtual method to a base class can silently shadow a same-named method in a derived class — the derived class's method was previously hiding (with `new`) a non-virtual base method, but after the base adds `virtual` on a new method with the same name, the derived method becomes an unintended override or the dispatch semantics change. A subtler case: if the base changes the implementation of a virtual method that some derived class's override calls via `base.Method()`, the derived override may now have incorrect behavior. Each derived class is implicitly coupled to the base's implementation strategy. The standard mitigation is to prefer composition over inheritance for behavior reuse, keep inheritance hierarchies shallow, seal types aggressively when subclassing is not intended, and treat base class APIs as contracts that cannot change semantics once published.

---

## Q12. How does constructor chaining work in a multi-level inheritance chain?

**Concepts**
- Base constructor runs before derived constructor
- Implicit `: base()` when no explicit base call and base has parameterless constructor
- CS7036 when base has no parameterless constructor and `base(...)` is omitted
- Execution order: A → B → C for hierarchy A → B → C
- No access to derived fields during base constructor

**Answer**

When a derived class constructor executes, the base class constructor always runs first — the derived constructor body cannot begin until the base is fully initialized. If the base class has a parameterless constructor, the compiler inserts an implicit `: base()` call if you write no explicit `: base(...)`. If the base has only parameterized constructors and the derived omits `: base(...)`, the compiler reports CS7036. In a three-level hierarchy `Person → Employee → Manager`, creating a `Manager` runs `Person`'s constructor, then `Employee`'s, then `Manager`'s. This ordering means each level can safely read members initialized by levels above it. The risk is calling virtual methods from within a constructor: the vtable dispatches to the most derived override even though the derived class's constructor has not run yet, so the override may read uninitialized derived fields.

---

## Q13. What is method hiding with the `new` modifier, and why is it a code smell?

**Concepts**
- `new` modifier on member with same name as base member
- Static dispatch — resolved by reference type at compile time
- CS0108 warning without `new`
- Breaks polymorphic collections
- Design smell: usually a sign the base should have used `virtual`

**Answer**

Method hiding with `new` declares a member that shares a name with a base class member but is dispatched statically by the compile-time type of the reference. If `Manager.GetBadgeCode()` is declared with `new`, then `((Employee)mgr).GetBadgeCode()` calls the base implementation while `mgr.GetBadgeCode()` calls the derived implementation. Any code that works with a collection of `Employee` references — typical polymorphic loops — silently calls the base. This is almost never what you want and is a sign that the base class method should have been declared `virtual`. Legitimate uses of `new` are rare and narrow: for example, hiding a property to change its return type to a more specific derived type (`new List<Order> Items { get; }` hiding `IEnumerable<Order> Items`), or when you intentionally need different behavior depending on the static reference type. In practice, if you find yourself reaching for `new` to "override" behavior, add `virtual` to the base method or reconsider the inheritance hierarchy.

---

## Gotcha Questions

---

## Q14. A payroll loop produces `"EMP"` badge codes for all employees, including managers. The method returns `employee.GetBadgeCode()`. Why, and how do you fix it?

**Concepts**
- `GetBadgeCode()` not declared `virtual` on base
- `new` in derived class creates hiding, not overriding
- Static dispatch via `Employee` reference type
- Fix: add `virtual` to base method, `override` in derived
- Polymorphic dispatch via vtable after fix

**Answer**

The method `GetBadgeCode()` on `Employee` is not declared `virtual`. Derived classes (`Manager`, `ContractEmployee`) declare `GetBadgeCode()` with `new`, which hides the base method rather than overriding it. When the payroll loop iterates an `Employee[]` and calls `employee.GetBadgeCode()`, the compiler generates a `callvirt` instruction using the `Employee` type's vtable slot — which has no override because hiding methods do not participate in vtable overriding. The call always resolves to `Employee.GetBadgeCode()` returning `"EMP"`. The fix is to add `virtual` to `Employee.GetBadgeCode()` and change `new` to `override` in every derived class. After this change, the vtable slot for `GetBadgeCode` in `Manager`'s vtable points to `Manager.GetBadgeCode()`, and the polymorphic loop returns the correct badge code for each derived type without any type checks.

---

## Q15. A new `InternEmployee : Employee` overrides `CalculateNet()` to throw `InvalidOperationException`. Why does this violate LSP, and what should be done instead?

**Concepts**
- LSP — substitutability requirement
- Override that throws breaks caller expectations
- Payroll loop has no type-specific handling
- Wrong inheritance relationship (interns are not payroll employees)
- Segregated interface or exclusion from the payroll hierarchy

**Answer**

LSP requires that a derived class can fully substitute for its base in any context that works with the base type. `ProcessPayroll` iterates `IReadOnlyList<Employee>` and calls `CalculateNet()` on every element, expecting it to return a valid `Money` result. `InternEmployee.CalculateNet()` throws instead, breaking that expectation for callers who never anticipated the exception. This is a classic LSP violation: the base class's contract is that `CalculateNet()` returns a value; the derived class strengthens the precondition by adding an implicit "not an intern" requirement. The root cause is a wrong IS-A relationship — interns are not payroll employees in the standard sense. The fix depends on the domain: either exclude interns from the `Employee` hierarchy and handle them in `StipendService` separately, or introduce a segregated interface `IPayrollCalculable` that only types supporting standard payroll calculation implement, and filter the payroll loop to `IPayrollCalculable` items. This keeps the inheritance hierarchy honest.

---

## Q16. Why does a `ContractEmployee` override of `CalculateNet()` that omits `base.CalculateNet()` silently drop validation?

**Concepts**
- `override` replaces base implementation — does not auto-call base
- `base.Method()` call is explicit and optional
- Shared validation/audit logic in base bypassed
- Template Method Pattern as alternative
- `abstract` base method forces full ownership

**Answer**

When `ContractEmployee.CalculateNet()` is declared `override` and its body returns `BaseSalary + ContractBonus` without calling `base.CalculateNet()`, it completely replaces the base implementation. Any validation, audit logging, or invariant-checking code inside the base `CalculateNet()` is silently skipped. Callers and unit tests may not notice because the method still returns a value and does not throw — the missing validation is invisible. The fix depends on design intent. If validation must always run, use the Template Method Pattern: the base class provides a non-virtual `CalculateNet()` that performs validation and then calls an abstract (or virtual) `CalculateNetCore()` that derived classes override to provide their specific computation. This guarantees validation runs for all types without relying on each derived class to remember to call `base`. Alternatively, mark the base `CalculateNet()` `abstract`, forcing derived classes to own the complete calculation and include validation themselves — appropriate when each type's pay structure is entirely different.

---

## Q17. Can a C# class inherit from two base classes? How is the "diamond problem" handled?

**Concepts**
- Single class inheritance only
- Multiple interface implementation
- No diamond problem with interfaces (compile-time disambiguation)
- Default interface methods can create diamond scenarios
- `ExplicitInterfaceImplementation` as resolution

**Answer**

C# does not allow a class to inherit from more than one base class — this is a fundamental language design decision that eliminates the diamond problem at the class level. If `Director` needed to inherit from both `Employee` and `BoardMember` classes, C# forbids it. Multiple interface implementation is the mechanism for expressing multiple capabilities: `class Director : Employee, IBoardMember`. Since interfaces (before C# 8) had no implementation, there was no ambiguity about which implementation to inherit. C# 8 introduced default interface methods, which can create a limited form of the diamond problem: if `IAlpha` and `IBeta` both provide a default implementation of `Print()`, and `class Foo : IAlpha, IBeta` does not override it, the compiler reports an ambiguity error. The resolution is for `Foo` to explicitly implement `Print()`, or use explicit interface implementation (`void IAlpha.Print()`) to disambiguate.

---

## Real-World Scenarios

---

## Q18. A payroll loop incorrectly uses explicit type checks instead of polymorphism. Diagnose the problem and show the corrected design.

**Concepts**
- Type-switching anti-pattern in polymorphic code
- Open/Closed Principle violation
- Polymorphic method replaces switch
- New types silently unhandled
- Fragile maintenance burden

```csharp
public static Money ProcessPayroll(IEnumerable<Employee> staff)
{
    Money total = new(0m);
    foreach (Employee employee in staff)
    {
        if (employee is PermanentEmployee permanent)
            total += permanent.CalculateNet();
        else if (employee is Manager manager)
            total += manager.CalculateNet();
        // ContractEmployee and future types not handled
    }
    return total;
}
```

| Category | Problem | Impact |
|---|---|---|
| Missing branch | `ContractEmployee` not handled | Contract employees silently excluded from payroll total |
| OCP violation | Adding a new employee type requires editing this method | Risk of forgotten case in future changes |
| Redundant cast | `CalculateNet()` is a virtual method on `Employee` — no cast needed | Unnecessary complexity |
| Type order bug | `Manager : PermanentEmployee` — `Manager` branch never reached if `PermanentEmployee` checked first | Managers may be processed incorrectly |

**Fix priority list**
1. Remove all type-check branches; call `employee.CalculateNet()` directly on the base reference.
2. Mark `Employee.CalculateNet()` as `abstract` to force every derived class to implement it.
3. Add a test that creates a `List<Employee>` containing all subtypes and verifies the total.

**Answer**

The root issue is using explicit type checks in code that should be polymorphic. Every `Employee` subtype should implement `CalculateNet()`, and the loop should call it through the base reference without knowing the specific type. The type-switch pattern also violates the Open/Closed Principle: adding `DirectorEmployee` or `PartTimeEmployee` requires editing `ProcessPayroll` and easy to forget a case. After declaring `CalculateNet()` as `abstract` on `Employee`, the compiler ensures every concrete derived class provides an implementation. The loop becomes `foreach (Employee employee in staff) total += employee.CalculateNet()` — one line, no type checks, no missing cases. There is also a subtle ordering bug: `Manager` inherits from `PermanentEmployee`, so the `employee is PermanentEmployee` check matches managers first, meaning the `employee is Manager` branch is unreachable. Polymorphism eliminates this class of bug entirely by letting the vtable route each instance to the correct implementation.

---

## Q19. `Director` fails to compile with CS7036 because its constructor does not call `base(...)`. Describe the fix and the resulting execution order.

**Concepts**
- CS7036 — no suitable base constructor
- Implicit `: base()` only works if base has a parameterless constructor
- Explicit `: base(args)` required otherwise
- Constructor execution order in four-level chain
- Each level initializes its own state after base completes

```csharp
public class Director : Manager
{
    public Director(int id, string name, Department dept, Money salary,
        Money perks, Money pf, Money teamBonus, Money boardFee)
    {
        BoardFee = boardFee;  // CS7036 — no : base(...) call
    }
    public Money BoardFee { get; }
}
```

**Answer**

`Manager` has only a parameterized constructor with seven arguments. The C# compiler cannot insert an implicit `: base()` because there is no parameterless base constructor, so it reports CS7036. The fix is to add `: base(id, name, dept, salary, perks, pf, teamBonus)` to the `Director` constructor. Once added, the construction of `new Director(...)` executes in this strict order: `Person(int, string)` runs first (initializing `Id` and `FullName`), then `Employee(int, string, Department, Money)` runs, then `PermanentEmployee(...)`, then `Manager(...)`, and finally `Director`'s body runs, setting `BoardFee`. Each level can safely read state set by levels above it. If any level calls a virtual method, the vtable resolves to `Director`'s override even though `Director`'s body has not yet run, which is why virtual calls in constructors are dangerous — `Director.BoardFee` would be unset at that point.

---

## Q20. A new employee type inherits from both a shared `AuditableEntity` base and needs to be in the `Person → Employee → PermanentEmployee → Manager` chain. C# forbids multiple inheritance. How would you solve this?

**Concepts**
- Single inheritance limitation
- Interface extraction for `IAuditable`
- Mixin via extension methods or default interface methods
- Composition — `Employee` holds an `AuditLog` object
- Interface + shared abstract base as a compromise

**Answer**

C# allows only one base class, so inheriting from both `AuditableEntity` and `Employee` is forbidden. The design solution depends on what `AuditableEntity` provides. If it provides only behavior that can be expressed as an interface contract, extract `IAuditable` with a default implementation (C# 8+ default interface methods) and have `Employee` implement `IAuditable`. If it provides shared state (an `AuditLog` field), the cleanest approach is composition: add an `AuditLog` property to the `Employee` base class that all employees share, possibly initialized to a `NullAuditLog` by default. An alternative is to use extension methods on `IAuditable` to provide shared behavior without a base class. The proposal to make `Employee` inherit from `Department` (`Employee : Department`) is an IS-A violation — an employee is not a department — and would break LSP regardless of whether C# allowed it. Always question whether an inheritance proposal reflects a genuine IS-A relationship before attempting to model it.
