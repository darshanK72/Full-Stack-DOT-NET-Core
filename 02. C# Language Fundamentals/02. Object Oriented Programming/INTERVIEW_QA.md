# 02. Object Oriented Programming — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. Classes & Objects](#01-classes-objects)
  - [Q1. What is a class and what is an object in C#?](#q1-what-is-a-class-and-what-is-an-object-in-c)
  - [Q2. What is the difference between `struct` and `class` in C#?](#q2-what-is-the-difference-between-struct-and-class-in-c)
  - [Q3. What are the different principles of OOP supported in C#?](#q3-what-are-the-different-principles-of-oop-supported-in-c)
  - [Q4. What is a partial class in C#?](#q4-what-is-a-partial-class-in-c)
  - [Q5. Explain object initializers and collection initializers in C#.](#q5-explain-object-initializers-and-collection-initializers-in-c)
  - [Q6. What is the difference between shallow copy and deep copy in C#?](#q6-what-is-the-difference-between-shallow-copy-and-deep-copy-in-c)
  - [Q7. What is the difference between object identity and object equality?](#q7-what-is-the-difference-between-object-identity-and-object-equality)
  - [Q8. What is the difference between `IDisposable` and a finalizer (`~ClassName()`)?](#q8-what-is-the-difference-between-idisposable-and-a-finalizer-classname)
  - [Q9. What happens at runtime when you execute `new MyClass()`?](#q9-what-happens-at-runtime-when-you-execute-new-myclass)
  - [Q10. Where are class instances stored vs where are struct instances typically stored?](#q10-where-are-class-instances-stored-vs-where-are-struct-instances-typically-stored)
  - [Q11. What is the difference between a field, a property, and a method on a class?](#q11-what-is-the-difference-between-a-field-a-property-and-a-method-on-a-class)
  - [Q12. What is a static class vs an instance class — can you instantiate a static class?](#q12-what-is-a-static-class-vs-an-instance-class--can-you-instantiate-a-static-class)
  - [Q13. What is the `null` reference for reference types, and what is `default` for a struct vs a class?](#q13-what-is-the-null-reference-for-reference-types-and-what-is-default-for-a-struct-vs-a-class)
  - [Q14. What is object initializer syntax, and how does it interact with constructors?](#q14-what-is-object-initializer-syntax-and-how-does-it-interact-with-constructors)
  - [Q15. What is the difference between `ReferenceEquals`, `==`, and `Equals`?](#q15-what-is-the-difference-between-referenceequals--and-equals)
  - [Q16. When is a struct copied vs when is a reference copied when passed to a method?](#q16-when-is-a-struct-copied-vs-when-is-a-reference-copied-when-passed-to-a-method)
  - [Q17. What is the fragile base class problem at a high level?](#q17-what-is-the-fragile-base-class-problem-at-a-high-level)
  - [Q18. What is the difference between stack allocation and heap allocation for objects?](#q18-what-is-the-difference-between-stack-allocation-and-heap-allocation-for-objects)
  - [Q19. What does `GC.GetTotalMemory` measure, and why is it only a rough indicator?](#q19-what-does-gcgettotalmemory-measure-and-why-is-it-only-a-rough-indicator)
  - [Q20. What is the difference between an anemic class and a rich domain object?](#q20-what-is-the-difference-between-an-anemic-class-and-a-rich-domain-object)

- [02. Properties & Indexers](#02-properties-indexers)
  - [Q1. Explain properties and fields in C#.](#q1-explain-properties-and-fields-in-c)
  - [Q2. What are auto-implemented properties?](#q2-what-are-auto-implemented-properties)
  - [Q3. What are indexers in C#?](#q3-what-are-indexers-in-c)
  - [Q4. What is the difference between a `public` field and a `public` auto-property?](#q4-what-is-the-difference-between-a-public-field-and-a-public-auto-property)
  - [Q5. What are init-only properties (`get; init;`)?](#q5-what-are-init-only-properties-get-init)
  - [Q6. What is the difference between `{ get; private set; }` and a public getter with a private setter method?](#q6-what-is-the-difference-between--get-private-set--and-a-public-getter-with-a-private-setter-method)
  - [Q7. What are expression-bodied properties?](#q7-what-are-expression-bodied-properties)
  - [Q8. Can indexers be overloaded — what distinguishes overloads?](#q8-can-indexers-be-overloaded--what-distinguishes-overloads)
  - [Q9. What is the syntax for an indexer?](#q9-what-is-the-syntax-for-an-indexer)
  - [Q10. When should you use a full property with validation vs an auto-property?](#q10-when-should-you-use-a-full-property-with-validation-vs-an-auto-property)
  - [Q11. What is a computed/read-only property that derives its value from other members?](#q11-what-is-a-computedread-only-property-that-derives-its-value-from-other-members)
  - [Q12. What is the difference between `init` properties and constructor parameters for immutable objects?](#q12-what-is-the-difference-between-init-properties-and-constructor-parameters-for-immutable-objects)
  - [Q13. How do properties participate in object initializer syntax?](#q13-how-do-properties-participate-in-object-initializer-syntax)
  - [Q14. What is a preview-level understanding of `record` types and synthesized properties?](#q14-what-is-a-preview-level-understanding-of-record-types-and-synthesized-properties)
  - [Q15. Why might exposing a public `{ get; set; }` on a collection-typed property break encapsulation?](#q15-why-might-exposing-a-public--get-set--on-a-collection-typed-property-break-encapsulation)
  - [Q16. What is the difference between an indexer and a method named `GetByIndex`?](#q16-what-is-the-difference-between-an-indexer-and-a-method-named-getbyindex)
  - [Q17. Can interface types declare indexers, and how are they implemented?](#q17-can-interface-types-declare-indexers-and-how-are-they-implemented)
  - [Q18. What is the relationship between properties and data binding / serialization frameworks?](#q18-what-is-the-relationship-between-properties-and-data-binding--serialization-frameworks)

- [03. Constructors & Method Overloading](#03-constructors--method-overloading)
  - [Q1. Explain constructors and their types in C#.](#q1-explain-constructors-and-their-types-in-c)
  - [Q2. What is a destructor/finalizer in C#?](#q2-what-is-a-destructorfinalizer-in-c)
  - [Q3. Explain constructor chaining in C# (`: this(...)` vs `: base(...)`).](#q3-explain-constructor-chaining-in-c-this-vs-base)
  - [Q4. How can you call the base class constructor from a derived class?](#q4-how-can-you-call-the-base-class-constructor-from-a-derived-class)
  - [Q5. In what order do constructors and field initializers run in an inheritance chain?](#q5-in-what-order-do-constructors-and-field-initializers-run-in-an-inheritance-chain)
  - [Q6. Explain method overloading and method overriding in C#.](#q6-explain-method-overloading-and-method-overriding-in-c)
  - [Q7. What is a static constructor, and when does it run?](#q7-what-is-a-static-constructor-and-when-does-it-run)
  - [Q8. Can a struct have a parameterless constructor (C# 10+ rules vs earlier)?](#q8-can-a-struct-have-a-parameterless-constructor-c-10-rules-vs-earlier)
  - [Q9. What is the difference between a primary constructor (C# 12) and traditional constructors?](#q9-what-is-the-difference-between-a-primary-constructor-c-12-and-traditional-constructors)
  - [Q10. What happens if you do not define any constructor?](#q10-what-happens-if-you-do-not-define-any-constructor)
  - [Q11. Why might you mark a constructor `private`?](#q11-why-might-you-mark-a-constructor-private)
  - [Q12. What is constructor overloading, and how does `: this(...)` reduce duplication?](#q12-what-is-constructor-overloading-and-how-does-this-reduce-duplication)
  - [Q13. What is the exact initialization order: static constructor, field initializers, instance constructor, base constructor?](#q13-what-is-the-exact-initialization-order-static-constructor-field-initializers-instance-constructor-base-constructor)
  - [Q14. What is the difference between method overloading (compile-time) and method overriding (runtime polymorphism)?](#q14-what-is-the-difference-between-method-overloading-compile-time-and-method-overriding-runtime-polymorphism)
  - [Q15. When does the compiler fail to pick an overload due to ambiguity?](#q15-when-does-the-compiler-fail-to-pick-an-overload-due-to-ambiguity)
  - [Q16. Can constructors be inherited?](#q16-can-constructors-be-inherited)
  - [Q17. What validation belongs in a constructor vs a factory method?](#q17-what-validation-belongs-in-a-constructor-vs-a-factory-method)
  - [Q18. What is the difference between calling an overloaded instance method vs a static overloaded method?](#q18-what-is-the-difference-between-calling-an-overloaded-instance-method-vs-a-static-overloaded-method)

- [04. Static Members & Static Classes](#04-static-members--static-classes)
  - [Q1. Explain the `static` keyword in detail.](#q1-explain-the-static-keyword-in-detail)
  - [Q2. What is a static class in C#?](#q2-what-is-a-static-class-in-c)
  - [Q3. Why can you not override a `static` method?](#q3-why-can-you-not-override-a-static-method)
  - [Q4. What is the difference between a static class and the singleton pattern?](#q4-what-is-the-difference-between-a-static-class-and-the-singleton-pattern)
  - [Q5. What is a static field, and how is its lifetime different from an instance field?](#q5-what-is-a-static-field-and-how-is-its-lifetime-different-from-an-instance-field)
  - [Q6. What is a static property and static method — what is the `this` reference inside them?](#q6-what-is-a-static-property-and-static-method--what-is-the-this-reference-inside-them)
  - [Q7. Why can static methods not access instance members directly?](#q7-why-can-static-methods-not-access-instance-members-directly)
  - [Q8. When are static constructors executed, and how many times per AppDomain/process?](#q8-when-are-static-constructors-executed-and-how-many-times-per-appdomainprocess)
  - [Q9. What is the difference between `const` and `static readonly`?](#q9-what-is-the-difference-between-const-and-static-readonly)
  - [Q10. Can a static class implement interfaces?](#q10-can-a-static-class-implement-interfaces)
  - [Q11. What thread-safety concerns apply to mutable static fields?](#q11-what-thread-safety-concerns-apply-to-mutable-static-fields)
  - [Q12. Why is overusing static state a testing and maintainability problem?](#q12-why-is-overusing-static-state-a-testing-and-maintainability-problem)
  - [Q13. What is the difference between static nested classes and non-static nested classes?](#q13-what-is-the-difference-between-static-nested-classes-and-non-static-nested-classes)
  - [Q14. How do static members participate in inheritance — are they polymorphic?](#q14-how-do-static-members-participate-in-inheritance--are-they-polymorphic)

- [05. Inheritance & Polymorphism](#05-inheritance--polymorphism)
  - [Q1. Explain inheritance in detail in C#.](#q1-explain-inheritance-in-detail-in-c)
  - [Q2. Explain polymorphism in C# and how it can be achieved.](#q2-explain-polymorphism-in-c-and-how-it-can-be-achieved)
  - [Q3. What is the difference between compile-time and runtime polymorphism?](#q3-what-is-the-difference-between-compile-time-and-runtime-polymorphism)
  - [Q4. What is a sealed class in C#?](#q4-what-is-a-sealed-class-in-c)
  - [Q5. What is a virtual method in C#?](#q5-what-is-a-virtual-method-in-c)
  - [Q6. What is the difference between `this` and `base` keywords?](#q6-what-is-the-difference-between-this-and-base-keywords)
  - [Q7. What is operator overloading in C#?](#q7-what-is-operator-overloading-in-c)
  - [Q8. Explain the difference between `virtual`, `abstract`, and `override` keywords.](#q8-explain-the-difference-between-virtual-abstract-and-override-keywords)
  - [Q9. Explain the `new` keyword in the context of method hiding.](#q9-explain-the-new-keyword-in-the-context-of-method-hiding)
  - [Q10. Explain how C# handles multiple inheritance.](#q10-explain-how-c-handles-multiple-inheritance)
  - [Q11. Why does C# not support multiple inheritance of classes?](#q11-why-does-c-not-support-multiple-inheritance-of-classes)
  - [Q12. What is the fragile base class problem?](#q12-what-is-the-fragile-base-class-problem)
  - [Q13. Why is "favor composition over inheritance" a common guideline?](#q13-why-is-favor-composition-over-inheritance-a-common-guideline)
  - [Q14. What is runtime dispatch — how does the CLR resolve `override` calls through a base reference?](#q14-what-is-runtime-dispatch--how-does-the-clr-resolve-override-calls-through-a-base-reference)
  - [Q15. What is the difference between hiding with `new` and overriding with `override` when calling through a base-typed variable?](#q15-what-is-the-difference-between-hiding-with-new-and-overriding-with-override-when-calling-through-a-base-typed-variable)
  - [Q16. Can you inherit from a sealed class?](#q16-can-you-inherit-from-a-sealed-class)
  - [Q17. What is the difference between `is` type testing and casting in polymorphic code paths?](#q17-what-is-the-difference-between-is-type-testing-and-casting-in-polymorphic-code-paths)
  - [Q18. What is the Liskov Substitution Principle?](#q18-what-is-the-liskov-substitution-principle)
  - [Q19. When does `base.Method()` call the parent's implementation vs the current type's override?](#q19-when-does-basemethod-call-the-parents-implementation-vs-the-current-types-override)
  - [Q20. What is the difference between extending behavior with inheritance vs wrapping with composition?](#q20-what-is-the-difference-between-extending-behavior-with-inheritance-vs-wrapping-with-composition)

- [06. Abstract Classes & Interfaces](#06-abstract-classes--interfaces)
  - [Q1. Explain abstraction in detail in C#.](#q1-explain-abstraction-in-detail-in-c)
  - [Q2. What is the difference between abstraction and encapsulation?](#q2-what-is-the-difference-between-abstraction-and-encapsulation)
  - [Q3. What is the difference between abstraction and polymorphism?](#q3-what-is-the-difference-between-abstraction-and-polymorphism)
  - [Q4. What is the difference between an abstract class and an interface?](#q4-what-is-the-difference-between-an-abstract-class-and-an-interface)
  - [Q5. What is the difference between an abstract class and an interface before C# 8 vs after?](#q5-what-is-the-difference-between-an-abstract-class-and-an-interface-before-c-8-vs-after)
  - [Q6. Why do we need interfaces in C#?](#q6-why-do-we-need-interfaces-in-c)
  - [Q7. What is explicit interface implementation and when is it used?](#q7-what-is-explicit-interface-implementation-and-when-is-it-used)
  - [Q8. What are static abstract members in interfaces (C# 11)?](#q8-what-are-static-abstract-members-in-interfaces-c-11)
  - [Q9. Can an abstract class have concrete (non-abstract) methods?](#q9-can-an-abstract-class-have-concrete-non-abstract-methods)
  - [Q10. Can a class implement multiple interfaces?](#q10-can-a-class-implement-multiple-interfaces)
  - [Q11. When would you choose an abstract base class over an interface for shared implementation?](#q11-when-would-you-choose-an-abstract-base-class-over-an-interface-for-shared-implementation)
  - [Q12. What is the diamond problem, and how does C# address it?](#q12-what-is-the-diamond-problem-and-how-does-c-address-it)
  - [Q13. What is explicit interface implementation — why might `((IMyInterface)obj).Method()` work when `obj.Method()` does not?](#q13-what-is-explicit-interface-implementation--why-might-imyinterfaceobjmethod-work-when-objmethod-does-not)
  - [Q14. Can interfaces declare fields, constructors, or static concrete state?](#q14-can-interfaces-declare-fields-constructors-or-static-concrete-state)
  - [Q15. What is the difference between `IReadOnlyList<T>` as a parameter type and `List<T>`?](#q15-what-is-the-difference-between-ireadonlylistt-as-a-parameter-type-and-listt)
  - [Q16. When should API surface depend on interfaces vs abstract classes?](#q16-when-should-api-surface-depend-on-interfaces-vs-abstract-classes)

- [07. Encapsulation & Access Modifiers](#07-encapsulation--access-modifiers)
  - [Q1. Explain encapsulation in C# with examples.](#q1-explain-encapsulation-in-c-with-examples)
  - [Q2. What are the different access modifiers in C#?](#q2-what-are-the-different-access-modifiers-in-c)
  - [Q3. What is the difference between "information hiding" and "data hiding"?](#q3-what-is-the-difference-between-information-hiding-and-data-hiding)
  - [Q4. Why is exposing a mutable collection through a public getter an encapsulation break?](#q4-why-is-exposing-a-mutable-collection-through-a-public-getter-an-encapsulation-break)
  - [Q5. What is the difference between `protected internal` and `private protected`?](#q5-what-is-the-difference-between-protected-internal-and-private-protected)
  - [Q6. What does `internal` mean in the context of assemblies and `InternalsVisibleTo`?](#q6-what-does-internal-mean-in-the-context-of-assemblies-and-internalsvisibleto)
  - [Q7. What is the default access level for class members if you omit a modifier?](#q7-what-is-the-default-access-level-for-class-members-if-you-omit-a-modifier)
  - [Q8. How do access modifiers apply to nested types vs top-level types?](#q8-how-do-access-modifiers-apply-to-nested-types-vs-top-level-types)
  - [Q9. What is defensive copying when returning collections from properties?](#q9-what-is-defensive-copying-when-returning-collections-from-properties)
  - [Q10. What is the difference between encapsulation and immutability?](#q10-what-is-the-difference-between-encapsulation-and-immutability)
  - [Q11. Why are public fields discouraged in public APIs?](#q11-why-are-public-fields-discouraged-in-public-apis)
  - [Q12. How does `private protected` restrict visibility compared to `protected` alone?](#q12-how-does-private-protected-restrict-visibility-compared-to-protected-alone)
  - [Q13. What is a friend assembly pattern, and what are its trade-offs?](#q13-what-is-a-friend-assembly-pattern-and-what-are-its-trade-offs)
  - [Q14. How do property accessors use asymmetric access (`public get; private set;`)?](#q14-how-do-property-accessors-use-asymmetric-access-public-get-private-set)

- [08. Events](#08-events)
  - [Q1. Explain events in C# (including event handling and publisher-subscriber pattern).](#q1-explain-events-in-c-including-event-handling-and-publisher-subscriber-pattern)
  - [Q2. What is the difference between an `event` and a plain public delegate field?](#q2-what-is-the-difference-between-an-event-and-a-plain-public-delegate-field)
  - [Q3. Why should you unsubscribe from events, and what problem does this prevent?](#q3-why-should-you-unsubscribe-from-events-and-what-problem-does-this-prevent)
  - [Q4. What happens during multicast delegate invocation if one subscriber throws?](#q4-what-happens-during-multicast-delegate-invocation-if-one-subscriber-throws)
  - [Q5. What is the standard `EventHandler` / `EventHandler<TEventArgs>` pattern?](#q5-what-is-the-standard-eventhandler--eventhandlerteventargs-pattern)
  - [Q6. How do you raise an event safely?](#q6-how-do-you-raise-an-event-safely)
  - [Q7. What is the difference between custom delegate types and `EventHandler` for events?](#q7-what-is-the-difference-between-custom-delegate-types-and-eventhandler-for-events)
  - [Q8. Can interfaces declare events, and how are they implemented?](#q8-can-interfaces-declare-events-and-how-are-they-implemented)
  - [Q9. What memory-leak scenario arises when a long-lived publisher holds references to short-lived subscribers?](#q9-what-memory-leak-scenario-arises-when-a-long-lived-publisher-holds-references-to-short-lived-subscribers)
  - [Q10. What is the difference between events and the Observer pattern / IObservable?](#q10-what-is-the-difference-between-events-and-the-observer-pattern--iobservable)
  - [Q11. Can you assign to an event from outside the declaring class?](#q11-can-you-assign-to-an-event-from-outside-the-declaring-class)
  - [Q12. What is thread-safe event raising, and when is locking required?](#q12-what-is-thread-safe-event-raising-and-when-is-locking-required)

- [09. OOP Real-World Examples](#09-oop-real-world-examples)
  - [Q1. Explain the SOLID principles with concrete C# examples.](#q1-explain-the-solid-principles-with-concrete-c-examples)
  - [Q2. What is the Liskov Substitution Principle? Give a classic violation.](#q2-what-is-the-liskov-substitution-principle-give-a-classic-violation)
  - [Q3. What is Dependency Inversion, and how does constructor injection implement it?](#q3-what-is-dependency-inversion-and-how-does-constructor-injection-implement-it)
  - [Q4. What is the difference between Dependency Injection and the Service Locator pattern?](#q4-what-is-the-difference-between-dependency-injection-and-the-service-locator-pattern)
  - [Q5. What is the difference between "has-a" and "is-a" relationships?](#q5-what-is-the-difference-between-has-a-and-is-a-relationships)
  - [Q6. What is the anemic domain model anti-pattern?](#q6-what-is-the-anemic-domain-model-anti-pattern)
  - [Q7. What is the Open/Closed Principle, and how do interfaces support extension without modification?](#q7-what-is-the-openclosed-principle-and-how-do-interfaces-support-extension-without-modification)
  - [Q8. What is the Single Responsibility Principle?](#q8-what-is-the-single-responsibility-principle)
  - [Q9. What is the Interface Segregation Principle?](#q9-what-is-the-interface-segregation-principle)
  - [Q10. What is a factory method vs a simple constructor?](#q10-what-is-a-factory-method-vs-a-simple-constructor)
  - [Q11. What is the Strategy pattern, and how does it map to interfaces/delegates in C#?](#q11-what-is-the-strategy-pattern-and-how-does-it-map-to-interfacesdelegates-in-c)
  - [Q12. What is the Repository pattern at a high level, and why depend on abstractions?](#q12-what-is-the-repository-pattern-at-a-high-level-and-why-depend-on-abstractions)
  - [Q13. How does polymorphism simplify replacing implementations in tests?](#q13-how-does-polymorphism-simplify-replacing-implementations-in-tests)
  - [Q14. What is the difference between domain modeling with rich behavior vs CRUD-style service objects?](#q14-what-is-the-difference-between-domain-modeling-with-rich-behavior-vs-crud-style-service-objects)
  - [Q15. Virtual method from base constructor — what is the risk?](#q15-virtual-method-from-base-constructor--what-is-the-risk)
  - [Q16. Method hiding vs overriding — how does `new` vs `override` affect dispatch?](#q16-method-hiding-vs-overriding--how-does-new-vs-override-affect-dispatch)
  - [Q17. `Equals()` without `GetHashCode()` — what breaks?](#q17-equals-without-gethashcode--what-breaks)
  - [Q18. Mutable object as dictionary key — what is the runtime risk?](#q18-mutable-object-as-dictionary-key--what-is-the-runtime-risk)
  - [Q19. Struct boxing via interface — what happens to subsequent mutations?](#q19-struct-boxing-via-interface--what-happens-to-subsequent-mutations)
  - [Q20. `protected internal` vs `private protected` — what is the access difference?](#q20-protected-internal-vs-private-protected--what-is-the-access-difference)
  - [Q21. Type-checking anti-pattern — why are long `is` chains problematic?](#q21-type-checking-anti-pattern--why-are-long-is-chains-problematic)
  - [Q22. Memory leaks despite GC — what causes them in managed code?](#q22-memory-leaks-despite-gc--what-causes-them-in-managed-code)
  - [Q23. Exposing `List<T>` directly — why is this problematic?](#q23-exposing-listt-directly--why-is-this-problematic)
  - [Q24. `init` after construction — what is allowed and what is not?](#q24-init-after-construction--what-is-allowed-and-what-is-not)
  - [Q25. Static "singleton" vs DI singleton — what is the testability difference?](#q25-static-singleton-vs-di-singleton--what-is-the-testability-difference)
  - [Q26. Explicit interface hiding — how can two `GetName()` methods coexist?](#q26-explicit-interface-hiding--how-can-two-getname-methods-coexist)
  - [Q27. Finalizer timing — why can't you rely on `~ClassName()` for timely cleanup?](#q27-finalizer-timing--why-cant-you-rely-on-classname-for-timely-cleanup)
  - [Q28. Overriding `==` without consistent `Equals`/`GetHashCode` — what breaks?](#q28-overriding--without-consistent-equalsgethashcode--what-breaks)
  - [Q29. Default interface methods on structs — when does boxing occur?](#q29-default-interface-methods-on-structs--when-does-boxing-occur)

---

### 01. Classes & Objects

---

## Q1. What is a class and what is an object in C#?

**Concepts**
- class as a type definition and blueprint
- object as a runtime instance on the heap
- reference vs instance state distinction
- static members vs instance members
- many-objects-from-one-class relationship

**Answer**

A class is a type definition — a blueprint that describes the fields, properties, and methods every instance will have. An object is a concrete instance of that class created at runtime with `new`, which allocates memory on the managed heap and returns a reference stored in the variable. The class exists once in compiled metadata, while many independent objects can be created from it during execution, each holding its own copy of instance fields. Static members belong to the type itself rather than any particular object, so every instance observes the same static state and no instance is needed to access them.

---

## Q2. What is the difference between `struct` and `class` in C#?

**Concepts**
- value type vs reference type semantics
- copy-on-assignment behavior
- null default for class references vs zero-bits for structs
- inheritance restrictions on structs
- struct identity vs reference identity

**Answer**

A `struct` is a value type that copies its entire data on assignment, so two variables holding the same struct are independent after the assignment. A `class` is a reference type where the variable stores only a pointer, meaning two variables can reference the same object and mutations through one are visible through the other. Structs default to all-zero bits and cannot be null unless wrapped in `Nullable<T>`, while class variables default to `null` since no object is referenced. Structs support only interface inheritance and cannot serve as base types, which means they are unsuitable for polymorphic hierarchies. Use structs for small, short-lived, immutable data; use classes for entities with identity, shared mutable state, or inheritance.

---

## Q3. What are the different principles of OOP supported in C#?

**Concepts**
- encapsulation via access modifiers and properties
- abstraction via abstract classes and interfaces
- inheritance via single base class plus interfaces
- polymorphism via virtual dispatch and interface implementation
- composition as a modern complement to inheritance

**Answer**

C# supports the four classical OOP pillars. Encapsulation hides state behind access modifiers and controlled property accessors so the type enforces its own invariants. Abstraction exposes only essential operations through abstract classes or interfaces, hiding implementation details from callers. Inheritance lets a derived class extend a base class, reusing and specializing behavior while sharing the base contract. Polymorphism allows code written against a base type or interface reference to invoke the correct derived behavior at runtime through virtual dispatch.

---

## Q4. What is a partial class in C#?

**Concepts**
- partial keyword splitting one type across files
- compiler merging at compile time only
- use with designer-generated code
- partial methods for cross-file hooks
- same namespace and assembly requirement

**Answer**

A `partial class` splits one type's definition across multiple source files, and the compiler merges them into a single type at compile time. All parts must carry the `partial` modifier and share the same namespace, type name, and assembly. This pattern is common for tooling scenarios like WinForms and EF Core scaffolding, where one file holds auto-generated code that users should not edit and another holds custom logic. Partial methods let one part declare a method signature while another provides the implementation, with the compiler eliminating call sites when no implementation is supplied.

---

## Q5. Explain object initializers and collection initializers in C#.

**Concepts**
- object initializer running after constructor
- collection initializer desugaring to Add calls
- init-only properties in object initializers
- property assignment order in initializers
- use cases for DTOs and test data

**Answer**

Object initializers set public properties or fields immediately after a constructor completes using `{ Property = value }` syntax, which avoids needing a dedicated constructor overload for every combination of optional properties. Collection initializers add elements to collections that implement `Add` using `{ item1, item2 }` syntax, which desugars to repeated `Add` calls after the collection is constructed. The constructor still runs first and establishes required invariants; the initializer assignments follow in source-text order. Init-only properties (`init`) accept assignments during this initializer phase and become read-only once construction ends.

---

## Q6. What is the difference between shallow copy and deep copy in C#?

**Concepts**
- shallow copy duplicating top-level fields only
- deep copy recursively cloning all referenced objects
- MemberwiseClone producing shallow copy
- reference aliasing risk in shallow copies
- immutable nested objects making shallow copy safe

**Answer**

A shallow copy duplicates the top-level object and copies field values as-is, which means reference-type fields in the copy still point to the same nested objects as the original. A deep copy recursively clones every nested object so the entire object graph is independent, requiring custom logic, serialization-based cloning, or explicit recursive `Clone` implementations. `MemberwiseClone` on a class produces a shallow copy. Since two shallow-copied objects share nested reference types, mutating a nested collection through one copy is visible through the other, which makes shallow copy unsafe when inner state must be isolated. Immutable nested objects make shallow copy safe because inner state cannot change regardless of sharing.

---

## Q7. What is the difference between object identity and object equality?

**Concepts**
- identity as same heap address
- equality as equivalent value or custom logic
- ReferenceEquals for identity check
- Equals and == for equality check
- consistent override of Equals, GetHashCode, and ==

**Answer**

Identity means two references point to the exact same heap object, which `ReferenceEquals` tests. Equality means two objects compare as equivalent by value or custom logic even when they are distinct instances, which `Equals` and an overloaded `==` operator express. For classes, the default `Equals` uses reference identity unless overridden; for value types, it compares fields structurally by default. When you override `Equals` to express value equality, you must also override `GetHashCode` consistently so that equal objects produce the same hash, which dictionary and set operations rely on.

---

## Q8. What is the difference between `IDisposable` and a finalizer (`~ClassName()`)?

**Concepts**
- IDisposable for deterministic cleanup
- finalizer as a safety net for missed dispose
- GC.SuppressFinalize after successful dispose
- dispose pattern with protected virtual Dispose(bool)
- non-deterministic finalizer timing

**Answer**

`IDisposable.Dispose` releases resources deterministically when callers use `using` or call `Dispose` explicitly, which makes it the right mechanism for timely cleanup of file handles, database connections, or network sockets. A finalizer (`~ClassName()`) runs later and non-deterministically during garbage collection as a last-resort safety net for cases where `Dispose` was never called. Since finalizers add GC overhead and cannot be relied on for timely release, the standard pattern is to implement `IDisposable` and call `GC.SuppressFinalize(this)` after successful cleanup so the finalizer is skipped when dispose ran correctly.

---

## Q9. What happens at runtime when you execute `new MyClass()`?

**Concepts**
- heap allocation including object header and method table pointer
- field initialization before constructor body
- constructor chain execution order
- reference assignment after construction completes
- no instance exists until new returns

**Answer**

The CLR first allocates memory on the managed heap for the object header, method table pointer, and all instance fields. Field initializers then run in declaration order, setting fields to their explicit initial values before any constructor body executes. Next the constructor chain fires: if a `: base(...)` call is present, the base constructor runs first, then the derived constructor body. Finally the completed object's reference is returned and assigned to the target variable. No object reference is available during construction until `new` returns, which is why passing `this` out of a constructor can expose a partially-initialized object.

---

## Q10. Where are class instances stored vs where are struct instances typically stored?

**Concepts**
- class instances always on the managed heap
- struct locals typically on the stack or in registers
- structs boxed to heap when cast to object or interface
- closures capturing struct locals promote to heap
- large struct copy cost vs heap indirection

**Answer**

Class instances always live on the managed heap and variables hold references to them. Struct local variables typically reside on the stack or in registers because they are value types with bounded lifetimes, which eliminates GC pressure for short-lived computations. However, structs embedded as fields in a heap object, boxed to `object` or an interface, or captured in a lambda closure are promoted to the heap as part of those containers. Large struct locals still copy by value on assignment and parameter passing, so structs with many fields can be more expensive to pass than a single reference, which is why the `in` modifier exists for read-only pass-by-reference.

---

## Q11. What is the difference between a field, a property, and a method on a class?

**Concepts**
- field as a raw storage location
- property as a controlled accessor pair
- method as a behavioral operation
- properties enabling validation and versioning
- fields bypassing invariant enforcement

**Answer**

A field is a direct storage location on the object, accessible by name; it holds state but offers no interception on reads or writes. A property is a pair of get/set accessor methods that present field-like syntax to callers while letting the type validate, compute, or defer values on access. A method is an operation that performs behavior, takes explicit parameters, and may mutate state or return results. Public fields are discouraged in APIs because any future need to add validation requires a breaking change from field to property, whereas properties can evolve internally without changing the public call site.

---

## Q12. What is a static class vs an instance class — can you instantiate a static class?

**Concepts**
- static class sealed and non-instantiable
- static class containing only static members
- instance class supporting new and inheritance
- compile error on new with static class
- testability tradeoff of static vs injected instance

**Answer**

A static class is implicitly sealed, cannot be instantiated, and can contain only static members, making it a named container for related utility functions or extension methods. Attempting `new` on a static class is a compile error because no constructor exists to call. An instance class can mix static helpers and instance state, and clients create objects with `new` whose lifetimes the GC manages. Static classes are suitable for stateless utilities but are difficult to mock or replace in tests, which is why production services that need substitution are better modeled as instance classes registered in dependency injection.

---

## Q13. What is the `null` reference for reference types, and what is `default` for a struct vs a class?

**Concepts**
- null as absence of any object reference
- NullReferenceException on dereference of null
- default(T) for classes resolving to null
- default(T) for structs resolving to zero-initialized value
- nullable reference type annotations under #nullable enable

**Answer**

`null` for a reference type means the variable holds no object reference; dereferencing it throws `NullReferenceException`. `default(T)` for a class type evaluates to `null` since the default state is no object. `default(T)` for a struct evaluates to a zero-initialized value where all fields are their respective zero equivalents — `0`, `false`, `null` for reference-type fields, and so on — which is always a valid struct value. Under `#nullable enable`, the compiler warns when `null` is assigned to a non-nullable reference variable, helping catch null dereferences at compile time.

---

## Q14. What is object initializer syntax, and how does it interact with constructors?

**Concepts**
- constructor running before initializer assignments
- initializer executing after constructor returns
- init-only properties accepting assignments in initializer
- assignment order following source text
- constructor cannot observe initializer assignments

**Answer**

Object initializer syntax `new Foo(args) { Prop = value }` calls the specified constructor first, lets it complete and establish invariants, and then assigns the listed properties or fields in source-text order. The constructor body finishes before any initializer assignment runs, so the constructor cannot read values set in the initializer. Init-only properties accept assignments during this phase and become read-only once the object exits its construction context. This pattern is useful for optional properties that do not belong in the required constructor signature, keeping the constructor focused on the mandatory invariants.

---

## Q15. What is the difference between `ReferenceEquals`, `==`, and `Equals`?

**Concepts**
- ReferenceEquals always checking pointer identity
- == default behavior for classes vs structs
- Equals virtual method overridable per type
- risk of == and Equals diverging
- GetHashCode consistency requirement

**Answer**

`ReferenceEquals` always compares whether two references point to the same heap address and cannot be overridden. For classes that do not override equality, `==` and `Equals` also compare reference identity by default, so all three agree. When a class overrides `Equals` to express value equality but forgets to overload `==`, the two operators diverge — `==` still uses identity while `Equals` uses the custom logic — which confuses callers and breaks `Dictionary` and `HashSet` operations. The correct approach is to override `Equals`, `GetHashCode`, and `==` together so they remain consistent.

---

## Q16. When is a struct copied vs when is a reference copied when passed to a method?

**Concepts**
- struct parameter copying entire value by default
- reference parameter copying only the pointer
- ref and out enabling struct pass-by-reference
- in modifier for read-only struct pass-by-reference
- boxing when struct assigned to object or interface

**Answer**

When you pass a struct to a method, the entire struct value is copied into the parameter slot, so mutations to the parameter's fields affect only the copy and are invisible to the caller. When you pass a class instance, only the reference (pointer) is copied, so both caller and callee share the same heap object and mutations to fields are visible on both sides. Adding `ref` or `out` to a struct parameter passes it by reference, eliminating the copy and allowing the method to mutate the caller's variable. The `in` modifier passes a struct by read-only reference, avoiding the copy cost without allowing mutation.

---

## Q17. What is the fragile base class problem at a high level?

**Concepts**
- derived class coupling to base implementation details
- base change silently breaking subclasses
- virtual calls from constructors as acute risk
- sealed classes reducing fragility
- composition avoiding the coupling

**Answer**

The fragile base class problem occurs when a change to a base class — adding a virtual method, reordering constructor steps, or altering a shared field — unexpectedly breaks subclasses that relied on the previous behavior. Subclasses are tightly coupled to base implementation details they do not control, so the base class author cannot safely evolve the type without auditing all known derivations. Calling overridable virtual methods from a base constructor is particularly dangerous because the derived override runs before derived field initializers complete, leaving derived state at default values. Sealing classes, minimizing virtual surface, and preferring composition over inheritance all reduce this risk.

---

## Q18. What is the difference between stack allocation and heap allocation for objects?

**Concepts**
- stack allocation scoped to method invocation
- heap allocation managed by GC
- stackalloc for unmanaged buffer on stack
- Span<T> wrapping stackalloc safely
- escape restrictions preventing dangling stack pointers

**Answer**

Stack allocation ties memory to the current method invocation: it is automatically reclaimed when the method returns with no GC involvement. Heap allocation via `new` on a class produces an object whose lifetime extends until the GC determines it is unreachable, which can be longer or shorter than any single method call. `stackalloc` allocates a buffer on the stack and is typically wrapped in `Span<T>` for safe bounds-checked access; it is useful for high-performance temporary buffers because it generates no GC pressure. The language prevents returning references to stack-allocated memory that would outlive the method, catching most misuses at compile time.

---

## Q19. What does `GC.GetTotalMemory` measure, and why is it only a rough indicator?

**Concepts**
- approximate managed heap byte count
- optional forced collection before measurement
- exclusion of unmanaged and JIT memory
- post-GC state skewing measurements
- production profiling tools as the correct alternative

**Answer**

`GC.GetTotalMemory` returns an approximation of bytes currently allocated in the managed heaps as the GC understands them. Passing `true` triggers a collection before measuring, which returns a post-collection low-water mark that obscures normal working-set usage. The number excludes unmanaged allocations made through P/Invoke or `Marshal.AllocHGlobal`, JIT code size, and native memory from interop — so it never equals the process working set shown in Task Manager. For production memory analysis, use dotnet-counters, dotnet-trace, or a memory profiler that captures full heap snapshots rather than this single approximate value.

---

## Q20. What is the difference between an anemic class and a rich domain object?

**Concepts**
- anemic class as data bag with external behavior
- rich domain object encapsulating state and rules together
- invariant enforcement inside rich objects
- scattered logic risk with anemic models
- context-appropriate choice between the two

**Answer**

An anemic class exposes all state through public getters and setters while business rules live in separate service classes that operate on the data externally. A rich domain object encapsulates both state and the rules that govern valid transitions, enforcing invariants through methods and controlled property accessors so the object can never exist in an illegal state. Anemic models are straightforward for simple CRUD operations and mapping layers, but they scatter domain logic across many service classes, making the rules hard to find and easy to bypass. Rich models align better with encapsulation when the type has meaningful constraints — for example a `BankAccount` that refuses negative balances through its `TryWithdraw` method rather than a plain balance setter.

### 02. Properties & Indexers

---

## Q1. Explain properties and fields in C#.

**Concepts**
- field as raw storage variable
- property as get/set accessor pair
- compiler-generated backing field for auto-properties
- properties enabling validation and interception
- interface contracts using properties not public fields

**Answer**

Fields are variables declared directly on a type and hold state without any interception on reads or writes. Properties are members with `get` and `set` accessors that present field-like syntax to callers while letting the type add validation, compute derived values, raise change notifications, or enforce invariants on assignment. Auto-properties declare `{ get; set; }` and the compiler generates a hidden backing field automatically, making them syntactically equivalent to fields at the call site but with the extensibility of a property. Interfaces specify properties in their contracts because fields cannot be part of an interface, which means using properties from the start avoids a breaking binary change later when validation becomes necessary.

---

## Q2. What are auto-implemented properties?

**Concepts**
- compiler-generated backing field
- get-only via init or constructor-only patterns
- private set restricting external mutation
- upgrade path to full property without API change
- use for DTOs and simple state

**Answer**

Auto-implemented properties use `{ get; set; }` syntax and let the compiler generate a private hidden backing field and the corresponding accessor methods, removing the need to write a field manually. They are appropriate for DTOs and simple state where any valid value of the property type is acceptable and no side effects are needed on get or set. Restricting mutation to the owning class uses `{ get; private set; }`, and restricting to the construction phase uses `{ get; init; }`. When validation or change notification becomes necessary later, you can replace the auto-property with a full property backed by an explicit field without changing the public API surface.

---

## Q3. What are indexers in C#?

**Concepts**
- this keyword with bracket parameters
- collection-like bracket syntax on custom types
- get and set accessors with parameters
- overloading by parameter signature
- interface indexer declarations

**Answer**

Indexers let a class or struct expose element-access syntax `obj[key]` by declaring a member with the `this` keyword and parameters in brackets: `public T this[int index] { get { ... } set { ... } }`. They work like properties with parameters, so the get accessor returns a value for the given key and the set accessor assigns one. Indexers are natural for collection-like types such as custom dictionaries, matrices, or buffers where callers expect bracket notation. They can be overloaded by parameter type — `this[int index]` and `this[string key]` can coexist — and interfaces may declare indexer contracts that implementing classes fulfill.

---

## Q4. What is the difference between a `public` field and a `public` auto-property?

**Concepts**
- property compiled to methods in IL
- field exposing storage directly
- data binding and serialization framework conventions
- properties supporting virtual override
- future validation without breaking call sites

**Answer**

At the call site both look identical, but a property compiles to get/set methods in IL while a field compiles to a raw storage slot. Serialization frameworks like `System.Text.Json` and data-binding frameworks like WPF discover public properties by convention and may skip public fields unless explicitly configured, so switching from field to property later can change serialization behavior. Properties can be `virtual` and overridden in derived classes, which fields cannot. Changing a public field to a property later is a binary-incompatible change for compiled consumers, whereas a property can evolve its getter or setter implementation without breaking callers.

---

## Q5. What are init-only properties (`get; init;`)?

**Concepts**
- assignment allowed only during construction phase
- read-only after object initialization completes
- difference from get-only and from private set
- object initializer ergonomics with init
- records relying heavily on init properties

**Answer**

Init-only properties use `{ get; init; }` and allow assignment only during the construction phase — inside the constructor body or in an object initializer `new Foo { Prop = value }` — after which they become read-only. This differs from `{ get; private set; }`, where class methods can still mutate the property after construction, and from a plain get-only property, which only a constructor or field initializer can set. Init properties give immutable-by-default semantics with the ergonomics of object initializers, since callers can specify only the properties they care about without a large constructor signature. Records use init properties heavily for their positional members.

---

## Q6. What is the difference between `{ get; private set; }` and a public getter backed by a private setter method?

**Concepts**
- private set accessible from all instance methods
- named setter method documenting intent
- domain operation vs generic assignment
- private set for simple internal mutation
- method for operations with parameters or side effects

**Answer**

`{ get; private set; }` exposes a property where the setter is private, meaning any instance method of the declaring class can call it with plain assignment syntax. A dedicated method like `Promote()` or `ApplyDiscount(decimal rate)` restricts mutation to a named operation that documents its intent, can carry additional parameters, and can enforce domain-specific preconditions beyond a simple null check. The choice is about clarity: use `private set` when any instance method should freely assign the value; use a named method when mutation represents a specific business operation with its own invariants and semantics.

---

## Q7. What are expression-bodied properties?

**Concepts**
- arrow syntax for read-only computed property
- evaluated on every get access
- no backing storage unless manually cached
- suitable for simple derived values
- complex logic belongs in full property or method

**Answer**

Expression-bodied properties use `=>` to define a read-only property that returns the result of a single expression: `public string FullName => $"{First} {Last}";`. They are syntactic sugar for a get-only property with a one-line body, making derived or formatted values concise to write. Since there is no backing field, the expression is evaluated fresh on every read, which is fine for cheap derivations but wasteful for expensive computations that should be cached. When the logic involves more than a single expression, a full property body with explicit get accessor or a dedicated method is clearer and avoids hiding significant work inside what looks like a simple property access.

---

## Q8. Can indexers be overloaded — what distinguishes overloads?

**Concepts**
- overloading by parameter signature
- parameter type and count distinguishing overloads
- return type alone not distinguishing overloads
- multi-dimensional indexers with multiple parameters
- explicit interface indexers as a separate contract

**Answer**

Indexers overload by parameter signature just like methods — different parameter types, counts, or type combinations produce distinct overloads that share the `this[...]` name. For example, `this[int index]` and `this[string key]` are two separate indexers on the same type. Return type alone does not distinguish overloads, since the compiler cannot select based on how the return value is used. Multi-dimensional access uses multiple parameters: `this[int row, int col]`. A class can also implement an interface indexer explicitly, providing separate behavior when accessed through the interface reference versus the concrete class type.

---

## Q9. What is the syntax for an indexer?

**Concepts**
- this keyword with bracket parameter list
- access modifier and return type before this
- get and set accessor blocks with parameters
- read-only indexer omitting set
- no default parameter values on indexer parameters

**Answer**

An indexer is declared as `public ReturnType this[ParameterType name] { get { ... } set { ... } }` where `this` signals the indexer, the parameters appear in square brackets, and the accessors work like property accessors with parameters available inside them. The access modifier and return type precede `this`. A read-only indexer omits the set accessor. Indexer parameters cannot have default values, which distinguishes them from methods with optional parameters. The `value` keyword in the set accessor refers to the assigned value, exactly as in a property setter.

---

## Q10. When should you use a full property with validation vs an auto-property?

**Concepts**
- full property when invariants or side effects are needed
- auto-property when any valid value is acceptable
- transition from auto to full without API change
- validation throwing ArgumentOutOfRangeException
- INotifyPropertyChanged requiring full property

**Answer**

Use a full property with an explicit backing field when assignment must validate ranges, normalize input, raise property-changed notifications, or trigger lazy loading. Use an auto-property when any value of the declared type is acceptable and no side effects are needed on get or set. The practical approach is to start with an auto-property and upgrade to a full property only when a specific rule appears, since the API surface — the property name and accessibility — remains identical to callers before and after the change. INotifyPropertyChanged implementations always require full properties because the setter must raise the `PropertyChanged` event when the value actually changes.

---

## Q11. What is a computed/read-only property that derives its value from other members?

**Concepts**
- no backing field storing the derived value
- evaluated on each read
- expression-bodied syntax for simple cases
- caching for expensive computations
- avoiding side effects in getters

**Answer**

A computed property derives its value from other fields or properties each time it is read, expressing derived state like `public decimal Total => Quantity * UnitPrice` or `public bool IsAdult => Age >= 18`. Since there is no separate backing field, the property never goes stale relative to its sources. Getters should avoid side effects because callers expect property reads to be predictable and cheap; if the computation is expensive, cache the result in a private field and invalidate it when the source fields change. Expression-bodied syntax is natural for simple computed properties.

---

## Q12. What is the difference between `init` properties and constructor parameters for immutable objects?

**Concepts**
- constructor parameters enforcing required values at creation
- init properties enabling optional member object initializer ergonomics
- required modifier for mandatory init properties (C# 11)
- constructor validation in one place
- choosing based on number and optionality of members

**Answer**

Constructor parameters enforce required values through an explicit signature — callers must supply each argument, and the compiler enforces the parameter list at every call site. Init properties allow object initializer syntax where callers supply only the properties they care about, with unset properties defaulting to their type defaults unless marked `required` (C# 11). For small immutable types with a few mandatory values, constructor parameters are cleaner since the signature documents what is required. For types with many optional fields, init properties with `required` annotations scale better than constructors with many optional parameters. Records combine both through positional syntax that generates both a constructor and init properties.

---

## Q13. How do properties participate in object initializer syntax?

**Concepts**
- settable properties assigned after constructor
- init-only properties restricted to this phase
- get-only properties not assignable in initializer
- assignment order following source text
- constructor invariants established before initializer

**Answer**

Object initializers can assign any property that has a publicly accessible setter or is declared `init`. The constructor runs first and establishes the object's core invariants, then the listed property assignments execute in source-text order. Init-only properties accept assignment here and become read-only once the initializer block closes. Get-only properties without `init` cannot be assigned in an object initializer; only the constructor or a field initializer can set them. Collection-typed properties can be populated via nested collection initializers if the property returns a mutable collection and the property itself is readable.

---

## Q14. What is a preview-level understanding of `record` types and synthesized properties?

**Concepts**
- record synthesizing equality, ToString, and Clone
- positional parameters becoming init-only properties
- value equality by default instead of reference equality
- with expressions for non-destructive mutation
- record struct for value-type records

**Answer**

Records (C# 9+) are primarily reference types where the compiler synthesizes `Equals`, `GetHashCode`, `ToString`, and a `Clone` method based on their properties. A positional record `record Person(string Name, int Age)` generates init-only properties `Name` and `Age`, a primary constructor, and value equality that compares those properties rather than reference identity. Two record instances with the same property values are equal even though they are distinct objects. The `with` expression creates a copy with selective property changes: `person with { Name = "Ali" }` produces a new `Person` with `Age` from the original. Record structs (C# 10+) apply the same pattern to value types.

---

## Q15. Why might exposing a public `{ get; set; }` on a collection-typed property break encapsulation?

**Concepts**
- public setter allowing complete collection replacement
- callers mutating internal collection directly
- invariant bypass via Add and Remove
- IReadOnlyList<T> as safer return type
- defensive copy pattern

**Answer**

A public setter on a `List<T>` property lets callers replace the entire collection with one they control, bypassing any sorting, deduplication, or synchronization the owning type maintains. Even without a setter, returning the live `List<T>` reference through the getter lets callers call `Add`, `Remove`, or `Clear` directly, which the owning type cannot observe or prevent. The safer pattern is to expose an `IReadOnlyList<T>` or `IReadOnlyCollection<T>` through the public getter while keeping a private mutable backing list, or to return a defensive copy when the collection must be fully detached from internal state.

---

## Q16. What is the difference between an indexer and a method named `GetByIndex`?

**Concepts**
- indexer using bracket syntax integrated with language
- method using explicit call syntax
- indexers participating in collection initializer protocol
- methods extensible via extension method pattern
- API ergonomics as the primary consideration

**Answer**

An indexer enables `obj[key]` bracket syntax that integrates naturally with C# language features — collection initializers can target indexer setters, and LINQ's `IList<T>` contract uses indexers. A method named `GetByIndex` requires explicit call syntax `obj.GetByIndex(key)` and reads more like a named query, which clarifies intent for non-collection lookups where bracket syntax might mislead. Indexers cannot be extension members, while methods can be. The choice is about API ergonomics: collection-like types with natural element access benefit from indexers; types where the lookup is a named domain operation are clearer with methods.

---

## Q17. Can interface types declare indexers, and how are they implemented?

**Concepts**
- interface indexer declaring get and set requirements
- implementing class providing this[ ] accessors
- explicit interface implementation for name conflicts
- consumers using interface reference for bracket access
- same hiding patterns as explicit method implementation

**Answer**

Interfaces can declare indexers with `Type this[ParamType name] { get; set; }`, and implementing classes provide the `this[...]` accessors that fulfill the contract. A class can implement the indexer publicly, making `obj[key]` work regardless of reference type, or implement it explicitly as `ReturnType IMap.this[string key] { get => ... }`, which is accessible only through an interface-typed reference. Explicit implementation is useful when the class also has a public indexer with different behavior or a different return type than the interface demands, and callers must cast to the interface to reach the explicit version.

---

## Q18. What is the relationship between properties and data binding / serialization frameworks?

**Concepts**
- serializers discovering public properties by convention
- public setter required for deserialization in most frameworks
- JsonIgnore and similar attributes targeting properties
- init-only properties supported by modern serializers
- naming conventions aligning with model binding

**Answer**

Serialization frameworks like `System.Text.Json` and `XmlSerializer` discover public readable and writable properties by convention and ignore fields unless explicitly configured otherwise, so making fields public does not automatically include them in serialized output. Missing public setters can prevent deserialization unless the framework supports constructor-based or init-based initialization. Attributes like `[JsonIgnore]` and `[JsonPropertyName]` target properties to control the serialized shape. Init-only properties work with modern versions of `System.Text.Json` when constructor-based deserialization is enabled. Data-binding systems in WPF and ASP.NET model binding follow similar conventions, expecting public properties as the bindable surface.

### 03. Constructors & Method Overloading

---

## Q1. Explain constructors and their types in C# (default, parameterized, static, private).

**Concepts**
- constructor as special initialization method
- compiler-generated default constructor
- parameterized constructor for required arguments
- static constructor for type-level initialization
- private constructor for singleton and factory patterns

**Answer**

A constructor is a special method invoked when an object is created, responsible for initializing fields and enforcing invariants before the object is exposed to callers. If you define no constructor, the compiler generates a public parameterless default constructor that zero-initializes all instance fields. A parameterized constructor accepts arguments so required values are supplied at the call site, which prevents partially-initialized objects. A static constructor is declared without access modifiers and runs once before the first use of the type to initialize static members. A private constructor prevents direct instantiation from outside the class, which is the foundation of singleton and factory patterns where instance creation is controlled through a static method.

---

## Q2. What is a destructor/finalizer in C#?

**Concepts**
- finalizer as GC-scheduled safety net
- non-deterministic execution timing
- IDisposable as the preferred deterministic alternative
- GC.SuppressFinalize after successful dispose
- finalizer adding two-pass GC overhead

**Answer**

A finalizer, written as `~ClassName()`, is called by the garbage collector before reclaiming the object's memory, providing a last-resort opportunity to release unmanaged resources if `Dispose` was never called. Finalizers execute non-deterministically because the GC schedules them based on memory pressure, so you cannot rely on them for timely release of file handles, connections, or locks. Objects with finalizers require two GC passes to collect — one to detect unreachability and one to run the finalizer and then collect — which adds heap pressure. The correct pattern is `IDisposable` for deterministic cleanup, calling `GC.SuppressFinalize(this)` after `Dispose` succeeds so the finalizer is skipped when cleanup already ran.

---

## Q3. Explain constructor chaining in C# (`: this(...)` vs `: base(...)`).

**Concepts**
- this() chaining within the same class
- base() crossing to the parent class constructor
- chained constructor running before calling constructor body
- single validation path through canonical constructor
- convention of most-complete constructor doing the real work

**Answer**

Constructor chaining allows one constructor to call another to eliminate duplicated initialization logic. `: this(...)` delegates to another constructor in the same class, so simpler overloads can call the most-complete constructor that holds all validation. `: base(...)` forwards to a constructor in the base class, which is required when the base has no accessible parameterless constructor. The chained constructor runs entirely before the calling constructor's body begins, which means validation in the target constructor runs for every path. By convention the most-parameter constructor does the real work, and simpler overloads delegate to it with default argument values.

---

## Q4. How can you call the base class constructor from a derived class?

**Concepts**
- base() syntax in derived constructor declaration
- base constructor running before derived constructor body
- implicit call to parameterless base constructor
- compiler error when no accessible parameterless base exists
- base constructor initializing the base portion fully

**Answer**

You call the base class constructor by appending `: base(arguments)` to the derived constructor declaration, immediately after its parameter list. The base constructor runs first, before the derived constructor body, so the base portion of the object is fully initialized when derived initialization code executes. If you omit `: base(...)`, the compiler implicitly inserts a call to the base's parameterless constructor; if the base has no parameterless constructor, omitting `: base(...)` is a compile error that requires you to provide an explicit chain. This guarantees that every object in the hierarchy is initialized from root to leaf in construction order.

---

## Q5. In what order do constructors and field initializers run in an inheritance chain?

**Concepts**
- base field initializers running before base constructor body
- derived field initializers running before derived constructor body
- execution flowing root to leaf
- virtual method calls from base constructor reading uninitialized derived fields
- static constructors running once before any instance creation

**Answer**

For an instance creation, execution flows from the most-base type outward: base instance field initializers run first (in source order), then the base constructor body executes, then derived instance field initializers run, then the derived constructor body. This means the base constructor completes before any derived field initializer has run, which is why calling virtual methods from a base constructor is dangerous — the derived override executes while derived fields are still at their default zero values. Static constructors for each type in the chain run once before the first instance of that type is used, following the same root-to-leaf order among static constructors.

---

## Q6. Explain method overloading and method overriding in C#.

**Concepts**
- overloading as compile-time dispatch by parameter signature
- overriding as runtime dispatch through virtual table
- override requiring virtual or abstract on base method
- new keyword hiding rather than overriding
- both techniques orthogonal and independently applicable

**Answer**

Method overloading defines multiple methods with the same name but different parameter signatures in the same class; the compiler selects the best-matching overload at the call site based on argument types and counts, so the selection is entirely at compile time. Method overriding replaces a base class's `virtual` or `abstract` method in a derived class using `override`, and the CLR dispatches to the most-derived override at runtime when the method is called through a base-typed reference. Overloading is about convenience — one name for logically similar operations with different inputs. Overriding is about behavioral substitution — derived types specialized behavior transparently replacing the base's. A derived class can both overload its own methods and override inherited virtual ones independently.

---

## Q7. What is a static constructor, and when does it run?

**Concepts**
- static constructor running once per type per AppDomain
- triggered before first use of the type
- no access modifier and no parameters
- TypeInitializationException wrapping any thrown exception
- cannot be called explicitly

**Answer**

A static constructor is declared with only the `static` keyword and no access modifier or parameters, and the CLR guarantees it runs exactly once per AppDomain before the first instance of the type is created or the first static member is accessed, whichever happens first. It is used to initialize static fields that require complex setup beyond what a field initializer can express. The static constructor cannot be called directly — the runtime invokes it automatically. If it throws, the type is permanently poisoned for that AppDomain: all subsequent attempts to use the type throw `TypeInitializationException` wrapping the original exception.

---

## Q8. Can a struct have a parameterless constructor (C# 10+ rules vs earlier)?

**Concepts**
- pre-C# 10 struct parameterless constructor disallowed
- C# 10 allowing explicit parameterless struct constructor
- default(T) always zero-initializing regardless of constructor
- array element initialization bypassing constructor
- practical use for safe default values

**Answer**

Before C# 10, structs could not declare an explicit parameterless constructor; the runtime always provided one that zero-initializes all fields, and this was not overridable. Starting with C# 10, structs can declare their own parameterless constructor to produce a meaningful default value. However, `default(T)` always produces the zero-initialized form rather than calling the parameterless constructor, and arrays of structs initialize elements to zero without calling any constructor. This means `new Point()` calls the custom constructor but `default(Point)` and `new Point[10]` still produce zero-initialized structs, so relying on the parameterless constructor for a "safe default" requires callers to use `new` explicitly.

---

## Q9. What is the difference between a primary constructor (C# 12) and traditional constructors?

**Concepts**
- primary constructor parameters declared on the class declaration line
- parameters captured in scope throughout the type body
- no automatic field generation in classes (unlike records)
- traditional constructor body for validation
- primary constructors reducing boilerplate for DI scenarios

**Answer**

Primary constructors (C# 12 for classes and structs) declare constructor parameters directly in the type declaration — `class Service(ILogger logger)` — making those parameters available throughout the type body as if they were captured variables. Unlike records, primary constructor parameters in classes are not automatically converted to properties or fields; you must manually assign them to fields if persistence beyond construction is needed. Traditional constructors have an explicit body where validation logic, field assignments, and chaining can all occur, which makes invariant enforcement straightforward. Primary constructors reduce boilerplate for types that only need to capture dependencies, such as DI-injected services where each parameter simply becomes a private readonly field.

---

## Q10. What happens if you do not define any constructor?

**Concepts**
- compiler generating public parameterless constructor
- generated constructor zero-initializing all fields
- parameterized constructor suppressing generated default
- frameworks requiring parameterless constructor for instantiation
- explicit constructor for invariant enforcement

**Answer**

If you define no constructor at all, the compiler generates a public parameterless constructor that zero-initializes all instance fields, so callers can instantiate the type with `new Foo()`. If you define at least one constructor with parameters but no parameterless constructor, the compiler does not generate one, which means `new Foo()` is a compile error for callers. This matters for frameworks like `System.Text.Json` (in some configurations), EF Core, and XML serializers that require a parameterless constructor to materialize instances during deserialization or object-relational mapping.

---

## Q11. Why might you mark a constructor `private`?

**Concepts**
- private constructor preventing direct external instantiation
- factory method pattern returning instances through static methods
- singleton pattern controlling single instance creation
- subtype selection in factory methods
- async initialization requiring factory for constructor limitation

**Answer**

A private constructor prevents any code outside the class from calling `new` directly, which forces all instance creation through static factory methods that the class controls. The singleton pattern uses a private constructor combined with a static property to ensure only one instance exists. Factory methods benefit from private constructors because they can return subtypes, cache instances, validate complex preconditions, or perform asynchronous initialization — none of which is possible in a constructor directly. This pattern is also used when the creation name should be descriptive: `Order.CreateWithDiscount(...)` communicates more intent than `new Order(amount, true)`.

---

## Q12. What is constructor overloading, and how does `: this(...)` reduce duplication?

**Concepts**
- constructor overloading providing multiple initialization signatures
- this() delegation to the canonical constructor
- validation living in one constructor only
- simpler overloads supplying defaults
- avoiding inconsistency when rules change

**Answer**

Constructor overloading provides multiple constructors with different parameter signatures so callers can initialize an object in the most convenient way for their context. `: this(...)` chaining has the simpler overloads delegate to the most-complete constructor so that all validation and initialization logic exists in one place. When a business rule changes — for example, a minimum quantity requirement — updating the single canonical constructor automatically enforces it for every overload without duplicating the check. Without chaining, each overload that copies initialization logic independently risks drifting out of sync with the others.

---

## Q13. What is the exact initialization order: static constructor, field initializers, instance constructor body, base constructor?

**Concepts**
- static constructor running once before first type use
- instance field initializers running before constructor body
- base field initializers running before base constructor body
- full base initialization before derived body
- derivation chain executing root to leaf

**Answer**

When a type is first used, its static field initializers run in source order and then the static constructor body executes — this happens once per AppDomain. For instance creation the order is: derived instance field initializers run, then execution jumps up to the base chain: base instance field initializers run, then the base constructor body, and then control returns down the chain so the derived constructor body runs. The full sequence for `new Derived()` where `Derived : Base` is: Derived field initializers → Base field initializers → Base constructor body → Derived constructor body. Virtual methods called from the base constructor execute during that phase, which means derived field initialization (constructor body assignments) has not run yet at that point.

---

## Q14. What is the difference between method overloading (compile-time) and method overriding (runtime polymorphism)?

**Concepts**
- overloading resolved by compiler from static type
- overriding resolved at runtime from actual object type
- virtual dispatch through method table
- same method name different selection mechanisms
- overloading for convenience, overriding for substitution

**Answer**

Method overloading is resolved entirely at compile time: the compiler inspects the static types of the arguments and selects the best-matching overload, which is fixed in the compiled IL. Method overriding is resolved at runtime: when a `virtual` method is called on a base-typed reference, the CLR looks up the actual object's method table and invokes the most-derived override. Overloading is a convenience mechanism — one name for conceptually similar operations with different inputs. Overriding is a behavioral-substitution mechanism — derived types provide specialized implementations that callers using the base API receive automatically. Both can coexist: a virtual method can be overloaded across parameter signatures and overridden in derived classes simultaneously.

---

## Q15. When does the compiler fail to pick an overload due to ambiguity?

**Concepts**
- CS0121 ambiguous call error
- optional parameters creating overlapping arities
- params arrays matching fixed-parameter overloads
- named arguments as disambiguation at call site
- removing optional parameters as the structural fix

**Answer**

The compiler emits CS0121 when two or more overloads are equally applicable to a call site and neither is more specific than the other. The most common cause is a method with optional parameters whose reduced arity overlaps with another overload's required parameters, so an argument list that satisfies both overloads with equal specificity triggers the error. A `params` array overload can also conflict with a fixed-parameter overload when the argument count matches both. The clean fix is to ensure each overload has a unique required parameter signature so no call site is ambiguous; a temporary workaround is using named arguments at the call site to force the compiler to pick the intended overload.

---

## Q16. Can constructors be inherited?

**Concepts**
- constructors not inherited in C#
- derived class must define its own constructors
- implicit base() call when base has parameterless constructor
- required explicit base() when base has only parameterized constructors
- C# 13 primary constructor inheritance in limited scenarios

**Answer**

Constructors are not inherited in C#; each class must define its own constructors explicitly. When you create a derived class without any constructor and the base has a public parameterless constructor, the compiler generates a parameterless derived constructor that implicitly calls `: base()`. If the base has only parameterized constructors, the derived class must define at least one constructor that explicitly calls `: base(args)`, otherwise the code does not compile. This design means derived classes always have clear ownership of their initialization, since inheriting constructors would implicitly expose base initialization paths that the derived type might not want to support.

---

## Q17. What validation belongs in a constructor vs a factory method?

**Concepts**
- constructor for fundamental type invariants
- factory method for complex conditional or async creation
- factory returning different subtypes based on arguments
- constructor throwing for invalid fundamental state
- factory returning result type to avoid exception-based error flow

**Answer**

Constructor validation is appropriate for fundamental invariants — conditions without which the object simply cannot exist in a meaningful state, such as a non-null required dependency or a positive price. If the condition fails, throwing from the constructor correctly prevents the variable from being assigned. Factory methods are preferred when creation can fail in ways that should return a typed result rather than throw, when the method may return different subtypes based on arguments, when asynchronous initialization is required (since constructors cannot be `async`), or when complex preconditions span multiple values whose interaction only makes sense at a higher level. The guideline is: constructors for invariants, factories for policy.

---

## Q18. What is the difference between calling an overloaded instance method vs a static overloaded method?

**Concepts**
- identical compile-time overload resolution rules for both
- instance method requiring an object receiver
- static method requiring type name as receiver
- virtual instance overloads dispatching at runtime if virtual
- static overloads always resolved at compile time

**Answer**

Both instance and static overloaded methods follow the same compile-time overload resolution rules: the compiler selects the best match based on argument types, counts, and any applicable conversions. The practical difference is that instance method overloads have access to `this` and participate in virtual dispatch if declared `virtual`, so calling an overloaded virtual instance method through a base reference may dispatch to a derived override at runtime. Static overload resolution always uses compile-time types since there is no instance and no virtual dispatch. The call site syntax differs — instance methods need an object or `this`, static methods need the type name — but the selection mechanics are the same.

### 04. Static Members & Static Classes

---

## Q1. Explain the `static` keyword in detail.

**Concepts**
- static member belonging to the type not an instance
- shared state across all instances
- no this reference in static context
- static constructor initializing type-level state
- static class as sealed non-instantiable container

**Answer**

The `static` keyword declares a member as belonging to the type itself rather than to any particular instance, so static fields are shared across all instances and static methods have no `this` reference. A static field is allocated once when the type is loaded and lives for the duration of the AppDomain. A static method can only access other static members directly; it must receive an explicit instance parameter to work with any object state. A static constructor runs once before the first use of the type. A `static class` is implicitly sealed, cannot be instantiated, and can contain only static members, making it a named container for stateless utility functions or extension methods.

---

## Q2. What is a static class in C#?

**Concepts**
- implicitly sealed and non-instantiable
- only static members permitted
- compiler enforcing no instance constructors or fields
- use for stateless utility groupings and extension methods
- cannot implement interfaces or be used as base

**Answer**

A static class is implicitly sealed, cannot be instantiated with `new`, and can contain only static members. The compiler rejects any instance field, instance property, instance constructor, or inheritance relationship on a static class. Static classes are used to group logically related utility functions that operate purely on their parameters — math helpers, string extension methods, and configuration constants are typical examples. Because they hold no instance state, they behave as typed namespaces for methods. Extension methods must be declared in a static non-nested class, which is the most common reason to define one in application code.

---

## Q3. Why can you not override a `static` method?

**Concepts**
- static dispatch resolved at compile time by type name
- no virtual table for static members
- new hiding static method by compile-time type
- C# 11 static abstract in interfaces for generic polymorphism
- class static hierarchy remaining non-polymorphic

**Answer**

Static methods are resolved at compile time using the type name in the call rather than through virtual dispatch on an object's method table. Since there is no runtime instance involved, there is nothing to look up in a virtual table, and the concept of "override" — which means the most-derived implementation wins at runtime — does not apply. If a derived class declares a static method with the same name using `new`, it hides the base method by compile-time type, but a call through a base-typed reference still invokes the base version. C# 11 introduced `static abstract` and `static virtual` members in interfaces to enable a constrained form of polymorphism in generic contexts, but this does not extend to class hierarchies.

---

## Q4. What is the difference between a static class and the singleton pattern?

**Concepts**
- static class as a language-level type with no instances
- singleton as a design pattern limiting a class to one instance
- singleton instance being a real heap object
- singleton implementing interfaces and participating in DI
- testability advantage of singleton over static class

**Answer**

A static class is a language construct that can never be instantiated and exists purely as a container for type-level members. The singleton pattern is a design pattern where a normal class controls its instantiation to produce exactly one instance, typically exposed through a static property like `Instance`. The singleton's single object lives on the heap, can implement interfaces, and can be substituted with a subtype or mock in tests by injecting it through a constructor parameter. A static class provides none of these capabilities, since you cannot assign it to a variable, pass it to a method expecting an interface, or replace it with a test double.

---

## Q5. What is a static field, and how is its lifetime different from an instance field?

**Concepts**
- static field allocated once per type per AppDomain
- instance field allocated per object
- static field persisting across object lifetimes
- GC collecting objects but not static state
- mutable static field as shared global state risk

**Answer**

A static field is associated with the type rather than any instance; it is allocated once when the type is first loaded and lives for the entire duration of the AppDomain or process. An instance field is allocated as part of each object and lives until that object is garbage collected. Because static fields exist independently of individual object lifetimes, they persist across method calls and object creations, making them suitable for type-level counts or caches but dangerous for per-request or per-user mutable state in multi-threaded applications. Mutating a static field in a web application without synchronization is a race condition since multiple request threads share the same static storage.

---

## Q6. What is a static property and static method — what is the `this` reference inside them?

**Concepts**
- accessed through type name not an object
- no this reference available
- accessing only static members directly
- instance members requiring explicit parameter
- useful for lazy initialization of shared state

**Answer**

Static properties and methods are invoked through the type name rather than an object reference, so there is no `this` reference inside them. They can only access other static members directly; to work with instance members they must receive an explicit object as a parameter. A static property can implement lazy initialization of shared state — for example, returning a cached `Lazy<T>` value or a configuration setting loaded once. A static method is appropriate for pure computations that operate only on their arguments and type-level state, such as parsing helpers or factory-like `Create(args)` methods that allocate and return new instances.

---

## Q7. Why can static methods not access instance members directly?

**Concepts**
- static context lacking a this reference
- instance members stored per-object on the heap
- no object to read instance state from
- explicit instance parameter enabling access
- design enforcing stateless utility nature of static methods

**Answer**

Static methods lack a `this` reference because they are invoked on the type, not on any particular object. Instance fields and properties hold values per object on the heap, and without knowing which specific object to read, the runtime has no way to locate the correct data. To access instance members from a static method you must accept an explicit object parameter and access members through it. This restriction is a design feature: it enforces that static methods remain stateless with respect to any particular object, which makes them easy to reason about in isolation and safe to call from any context without worrying about object lifecycle.

---

## Q8. When are static constructors executed, and how many times per AppDomain/process?

**Concepts**
- executed exactly once per type per AppDomain
- triggered before first instance creation or first static member access
- cannot be called explicitly
- TypeInitializationException wrapping any failure
- type permanently unusable after static constructor throws

**Answer**

The CLR executes a type's static constructor exactly once per AppDomain, before the first instance of the type is created or the first static member is accessed, whichever comes first. The runtime serializes this execution so even concurrent first accesses result in the static constructor running only once. It cannot be invoked explicitly. If the static constructor throws an exception, the type is permanently poisoned for that AppDomain: every subsequent attempt to use the type throws `TypeInitializationException` wrapping the original exception, and there is no recovery without restarting the process. This makes avoiding I/O and external calls in static constructors important for production reliability.

---

## Q9. What is the difference between `const` and `static readonly`?

**Concepts**
- const inlined at compile time into consuming assemblies
- static readonly evaluated once at runtime
- const requiring recompile of consumers after change
- static readonly readable from environment or config at startup
- const limited to primitive and string types

**Answer**

`const` values are substituted by the compiler directly into every consuming assembly's IL at compile time, so they incur zero runtime cost but require recompiling all consumers when the value changes to pick up the new value. `static readonly` fields are assigned once — either from a field initializer or from the static constructor — and stored in the type's memory at runtime, so consuming assemblies read the current value without recompilation after redeployment. `const` is limited to types the compiler can embed inline (primitive numerics, `string`, `bool`, and `enum`), while `static readonly` can hold any type including complex objects. Use `const` for true universal constants that will never change; use `static readonly` for values that are logically constant but derived at startup.

---

## Q10. Can a static class implement interfaces?

**Concepts**
- static class cannot implement interfaces
- interfaces describing instance contracts
- no instance to assign to an interface variable
- workaround via normal sealed class with static members
- extension method host static class as a common exception

**Answer**

No — a static class cannot implement interfaces because an interface describes a contract for instances, and you cannot create an instance of a static class or assign it to a variable of interface type. The compiler rejects the attempt with an error. If you need a type that both exposes utility members statically and satisfies an interface contract, you must use a normal (non-static) class, potentially with a combination of static and instance members. Extension method container classes are static classes that cannot implement interfaces, which is why extension method dispatch is resolved at compile time rather than through interface polymorphism.

---

## Q11. What thread-safety concerns apply to mutable static fields?

**Concepts**
- shared across all threads with no automatic synchronization
- read-modify-write as a non-atomic race condition
- Interlocked for simple counters
- lock for compound multi-field operations
- mutable static in ASP.NET Core as a per-process global

**Answer**

Mutable static fields are shared across all threads in the AppDomain without any built-in synchronization, so any read-modify-write sequence is a potential race condition. Even an innocuous `count++` compiles to separate load, increment, and store instructions, meaning two threads can read the same value before either writes back and produce a lost update. For simple integer counters, `Interlocked.Increment` provides an atomic operation without locking. For compound operations spanning multiple fields, a `lock` or a `ReaderWriterLockSlim` is required. In ASP.NET Core, mutable static fields are especially dangerous because every HTTP request runs on a thread pool thread and all requests share the same static state.

---

## Q12. Why is overusing static state a testing and maintainability problem?

**Concepts**
- static state persisting across test runs in the same process
- test order dependency causing flaky failures
- no injection seam for fakes or mocks
- parallel test isolation impossible with shared static state
- global hidden coupling between unrelated code paths

**Answer**

Static state persists for the entire AppDomain lifetime, so mutations in one test affect subsequent tests unless explicitly reset, which makes test order matter and produces flaky failures. There is no injection seam: you cannot replace a static dependency with a fake without modifying the class under test, which means static callers are tightly coupled to specific static implementations. Running tests in parallel is unsafe when tests share mutable static state because race conditions produce unpredictable results. At the design level, static state creates hidden coupling — any code anywhere in the process can read or write it, making dependencies invisible and refactoring risky.

---

## Q13. What is the difference between static nested classes and non-static nested classes?

**Concepts**
- static nested class lacking reference to enclosing instance
- non-static nested class holding implicit reference to outer instance
- static nested class accessing only outer static members
- non-static nested class accessing all outer private members
- use cases for builder pattern and helper types

**Answer**

A static nested class declared inside an outer class does not hold a reference to any instance of the enclosing type; it can access only the outer class's static members. It is essentially a normal class that is logically scoped inside the outer type's namespace for organizational clarity. A non-static nested class holds an implicit reference to an enclosing instance and can access all instance members of the outer class, including private ones. Static nested classes are common for builder patterns, configuration options nested inside a service class, and helper types that logically belong to an outer type but need no instance reference. Non-static nested classes are uncommon in C# compared to Java and are typically used when the inner type must tightly couple to a specific outer instance's private state.

---

## Q14. How do static members participate in inheritance — are they polymorphic?

**Concepts**
- static members not polymorphic through virtual dispatch
- hiding with new resolving by compile-time type
- C# 11 static abstract and static virtual in interfaces
- class-level static hierarchy non-polymorphic
- generic constraints enabling static polymorphism via interfaces

**Answer**

Static members in a class hierarchy are not polymorphic. You cannot declare a static method `virtual` in a class or override it with `override` in a derived class. If a derived class defines a static method with the same name using `new`, it hides the base version by compile-time type, so a call through a base-type reference still reaches the base method. C# 11 introduced `static abstract` and `static virtual` members in interfaces, which allows a constrained form of polymorphism for statics when consumed through generic type parameters constrained to that interface. This enables patterns like `T.Parse(string)` in generic math, but it is distinct from class-based inheritance and requires the interface constraint at the call site.

### 05. Inheritance & Polymorphism

---

## Q1. Explain inheritance in detail in C#.

**Concepts**
- single base class inheritance plus multiple interface implementation
- derived class inheriting instance and static members with access limits
- virtual methods enabling override in subclasses
- constructor chaining ensuring base initialization
- sealed keyword stopping further derivation

**Answer**

Inheritance lets a derived class extend a base class, gaining all accessible instance and static members and adding or replacing behavior. C# supports single inheritance of classes — a class can have exactly one direct base class — but a class may implement any number of interfaces, which allows multiple behavioral contracts without the complexity of multiple implementation inheritance. Protected members are visible to derived classes, enabling controlled extension while hiding implementation from unrelated code. A derived class constructor must chain to a base constructor using `: base(...)`, ensuring the base portion of the object is initialized before the derived body runs. Sealing a class or sealing an individual override stops further derivation at that point, which documents a final implementation and allows the JIT to devirtualize calls.

---

## Q2. Explain polymorphism in C# and how it can be achieved.

**Concepts**
- runtime dispatch through virtual method table
- base-typed reference invoking derived override
- interface implementation as a form of polymorphism
- compile-time polymorphism via overloading
- pattern matching as an alternative for closed hierarchies

**Answer**

Polymorphism allows code written against an abstraction — a base class or interface reference — to receive behavior from the actual derived type at runtime. When a `virtual` method is called through a base-typed reference, the CLR uses the object's method table to dispatch to the most-derived override, so `Animal a = new Dog(); a.Speak()` calls `Dog.Speak` if `Speak` is declared virtual. Interfaces achieve polymorphism without a shared class hierarchy: any type implementing `IShape` can be substituted wherever `IShape` is expected. Method overloading is compile-time polymorphism since the compiler picks the overload from static argument types. Pattern matching with `switch` expressions extends polymorphic dispatch to closed type sets where virtual dispatch is impractical.

---

## Q3. What is the difference between compile-time (static) and runtime (dynamic) polymorphism?

**Concepts**
- compile-time selection from static type information
- runtime selection through virtual dispatch table
- overloading as compile-time polymorphism
- virtual override as runtime polymorphism
- dynamic keyword adding late binding beyond inheritance

**Answer**

Compile-time polymorphism is resolved when the compiler builds the IL: method overloading, operator overloading, and `new` method hiding are all decided from the static types visible at the call site. Runtime polymorphism is resolved when the program executes: virtual method dispatch looks up the actual object's method table and invokes the most-derived override, regardless of the compile-time type of the reference. The key difference is when the decision is made — compile-time means the behavior is fixed in IL, runtime means it depends on what object is actually there. The `dynamic` keyword adds a third form of late binding where member resolution is deferred entirely to runtime beyond what the type system normally expresses.

---

## Q4. What is a sealed class in C#?

**Concepts**
- sealed class preventing subclassing
- sealed override preventing further override in chain
- JIT devirtualization optimization opportunity
- invariant and security preservation motivation
- CS0509 error on deriving from sealed class

**Answer**

A `sealed` class cannot be used as a base class; any attempt to inherit from it produces compile error CS0509. `sealed` can also be applied to an individual `override` method, which allows the class itself to be subclassed while preventing that specific virtual method from being overridden further down the hierarchy. The CLR and JIT can devirtualize calls to sealed types in some scenarios, since the method table lookup can be eliminated when the final type is known. Many BCL types like `string` are sealed to preserve invariants and security assumptions that would break if subclasses could replace key methods.

---

## Q5. What is a virtual method in C#?

**Concepts**
- virtual providing overridable default implementation
- non-virtual default in C# unlike Java
- runtime dispatch for virtual calls
- abstract virtual requiring override in concrete subclasses
- calling virtual methods from constructors as a risk

**Answer**

A `virtual` method in a base class provides a default implementation that derived classes may replace using `override`, and calls to it through any base-typed reference dispatch to the most-derived override at runtime. Unlike Java, C# methods are non-virtual by default, so only explicitly `virtual` members participate in runtime dispatch. An `abstract` method is implicitly virtual but has no body and must be overridden in every non-abstract derived class. Calling a virtual method from a base constructor is risky because the override runs while derived field initializers have not yet executed, meaning the override sees default values for all derived fields.

---

## Q6. What is the difference between `this` and `base` keywords?

**Concepts**
- this as reference to current instance
- base accessing base class members hidden by derived declarations
- base() in constructor calling parent constructor
- base.Method() invoking parent implementation without virtual redispatch
- static context having no this

**Answer**

`this` refers to the current object instance and is used for disambiguation (when a parameter name shadows a field), for passing the current instance as an argument, or for chaining to another constructor with `: this(...)`. `base` accesses base class members when they have been hidden by a derived declaration, and `base.Method()` explicitly calls the base class's implementation of a virtual method — it bypasses the derived override for that specific call site without dynamic redispatch. `base(...)` in a constructor initializer calls the base class constructor. Neither `this` nor `base` is available in static members since there is no instance involved.

---

## Q7. What is operator overloading in C#?

**Concepts**
- public static methods with operator keyword
- which operators can be overloaded
- paired overload requirement for == and !=
- equality operator consistency with Equals and GetHashCode
- use for domain types with intuitive arithmetic semantics

**Answer**

Operator overloading defines `public static` methods with the `operator` keyword so expressions like `a + b` compile for user-defined types when the appropriate overload is defined. Most arithmetic, comparison, and bitwise operators can be overloaded; some like `&&` and `||` cannot be directly overloaded though the `true` and `false` unary operators unlock short-circuit behavior for custom types. When you overload `==` you must also overload `!=`, and you should override `Equals` and `GetHashCode` consistently — if equality operators disagree with `Equals`, dictionary and set operations produce wrong results. Operator overloading is appropriate for domain types with natural numeric or comparison semantics, such as monetary amounts, vectors, or duration types.

---

## Q8. Explain the difference between `virtual`, `abstract`, and `override` keywords.

**Concepts**
- virtual providing overridable implementation in concrete class
- abstract requiring override with no body in base
- abstract class non-instantiable
- override replacing virtual or abstract in derived class
- sealed override stopping the chain

**Answer**

`virtual` declares a method in a concrete base class with an implementation that derived classes may optionally replace. `abstract` declares a method with no body and requires every non-abstract derived class to provide an implementation; a class with any abstract method must itself be declared `abstract` and cannot be instantiated. `override` in a derived class replaces the inherited virtual or abstract slot, and the runtime dispatches to the most-derived override at runtime. A `sealed override` in a derived class replaces the slot and simultaneously prevents any further subclass from overriding it again. Abstract classes can mix abstract methods (which derived classes must implement) with concrete methods (which derived classes inherit as-is or optionally override if declared virtual).

---

## Q9. Explain the `new` keyword in the context of method hiding.

**Concepts**
- new hiding base method by compile-time type
- no virtual dispatch for hidden methods
- CS0108 warning when hiding without new
- override for polymorphism vs new for hiding
- explicit cast to derived type exposing hidden method

**Answer**

When a derived class declares a method with the same signature as a non-virtual base method (or intentionally hides a virtual one), the `new` modifier signals the hiding is deliberate and suppresses compiler warning CS0108. Unlike `override`, hiding does not replace the virtual slot — when the method is called through a base-typed reference, the base version is invoked because dispatch is based on the compile-time type of the reference, not the runtime type. Calling through a derived-typed reference or after an explicit cast reaches the hiding method. Hiding is the wrong tool when polymorphism is the goal; use `override` when you want callers using a base reference to automatically receive the derived behavior.

---

## Q10. Explain how C# handles multiple inheritance (using interfaces).

**Concepts**
- single class inheritance preventing multiple base classes
- multiple interface implementation allowed
- explicit interface implementation resolving name conflicts
- default interface methods in C# 8 for shared implementation
- diamond problem resolved through interface resolution rules

**Answer**

C# allows a class to inherit from exactly one base class but implement any number of interfaces, gaining multiple behavioral contracts without the diamond-problem complexity of multiple class inheritance. When two implemented interfaces define members with the same signature, the class must either provide a single implementation that satisfies both or use explicit interface implementation to provide separate implementations per interface. C# 8 introduced default interface methods, which allow interfaces to supply a default body for members; when two interfaces provide conflicting default implementations, the compiler requires the implementing class to explicitly disambiguate.

---

## Q11. Why does C# not support multiple inheritance of classes?

**Concepts**
- diamond problem with conflicting implementations
- ambiguous object layout and constructor chaining
- CLR object model designed around single inheritance
- interfaces as the multiple-behavior alternative
- composition providing reuse without MI complexity

**Answer**

Multiple class inheritance introduces the diamond problem: if two base classes both define a method, the derived class inherits two conflicting implementations and the runtime has no unambiguous way to resolve which to call. It also complicates object layout since the object must accommodate two full base class instances, and constructor chaining becomes ambiguous. C# designers chose single class inheritance plus interface implementation, which allows multiple behavioral contracts without implementation conflicts. Composition — building a type from contained objects implementing each interface — covers most reuse scenarios that might otherwise motivate multiple inheritance, without the ambiguity.

---

## Q12. What is the fragile base class problem?

**Concepts**
- derived class tight coupling to base implementation details
- base change silently breaking subclasses
- adding virtual methods to base breaking derived assumptions
- virtual calls from constructors as acute risk
- composition and sealing as mitigations

**Answer**

The fragile base class problem arises when a change to a base class — adding a new virtual method, altering constructor order, changing a protected field's semantics — unexpectedly breaks derived classes that depended on the previous behavior. Subclasses couple to implementation details they cannot control, so the base author cannot safely evolve the type without auditing every subclass. The problem is especially acute when a base constructor calls a virtual method: the derived override runs before derived field initializers complete, producing bugs that are hard to diagnose. Mitigations include sealing classes that should not be extended, minimizing the virtual surface area, documenting extension points explicitly, and preferring composition over inheritance.

---

## Q13. Why is "favor composition over inheritance" a common guideline?

**Concepts**
- composition building types from contained collaborators
- loose coupling to interface of inner object
- inheritance exposing derived to base implementation changes
- wrapper and decorator patterns using composition
- strategy pattern replacing subclassing with injected behavior

**Answer**

Composition builds a type by containing other objects and delegating behavior to them, coupling only to those objects' interfaces rather than their internal implementations. Inheritance couples a derived class tightly to its base — every change to the base can silently break subclasses, and deep hierarchies obscure where behavior originates. A composed collaborator can be swapped at construction time, while a base class is fixed at compile time. The Strategy pattern replaces inheritance-based behavioral variation with an injected interface, and the Decorator pattern stacks composed wrappers instead of creating subclass combinations. Inheritance is still appropriate for true is-a relationships with stable, well-understood base contracts, but composition is the first tool to reach for when reuse is the only motivation.

---

## Q14. What is runtime dispatch — how does the CLR resolve `override` calls through a base reference?

**Concepts**
- method table pointer in object header
- virtual slot replaced in derived type's method table
- callvirt IL instruction enforcing virtual lookup and null check
- static type of reference irrelevant for virtual dispatch
- JIT devirtualization for sealed or provably final types

**Answer**

Every object's header contains a pointer to its type's method table. For virtual methods, the table has a slot per virtual member; when a derived class overrides a method, its method table slot points to the derived implementation. The CLR's `callvirt` IL instruction reads the actual object's method table pointer at runtime and invokes whatever function is in the relevant slot, ignoring the static type of the reference. This means `Base b = new Derived(); b.Method()` — where `Method` is virtual — always calls `Derived.Method` since the object's table has the derived slot. The JIT can skip the table lookup (devirtualize) when it can prove the object's concrete type is known, for example when calling through a `sealed` type reference.

---

## Q15. What is the difference between hiding with `new` and overriding with `override` when calling through a base-typed variable?

**Concepts**
- override replacing virtual slot for runtime dispatch
- new hiding by compile-time type only
- base-typed reference reaching hidden method
- explicit cast to derived type exposing hidden method
- polymorphic design requiring override not new

**Answer**

With `override`, a call through a base-typed reference dispatches to the derived implementation because virtual dispatch uses the runtime type. With `new` hiding, the same call invokes the base implementation because hiding is purely a compile-time, static-type decision — the base's virtual slot is untouched, so the runtime still finds the base method. To invoke the hiding method, callers must hold a derived-typed reference or cast explicitly to the derived type. This is why `new` should not be used as a substitute for `override` in polymorphic designs: callers holding base references will never see the derived behavior.

---

## Q16. Can you inherit from a sealed class?

**Concepts**
- sealed class preventing any derivation
- CS0509 compile error on attempt
- sealed types in BCL including string and Enum
- sealed override allowing class subclassing while blocking method override
- composition and interfaces for extending sealed types

**Answer**

No — a sealed class cannot serve as a base class, and any attempt produces compile error CS0509. Many BCL types like `string`, `Enum`, and most value-type boxed forms are sealed to preserve invariants and prevent subclasses from altering behavior that the framework depends on. `sealed` on a specific `override` method is distinct from sealing the class: a class can still be subclassed while that particular method cannot be overridden further. When you need to extend behavior on a sealed type, the options are composition — wrapping an instance and forwarding calls — and interfaces, since the sealed type may already implement interfaces you can program against.

---

## Q17. What is the difference between `is` type testing and casting in polymorphic code paths?

**Concepts**
- is returning bool without throwing on mismatch
- cast throwing InvalidCastException on failure
- pattern variable binding with is
- excessive type tests as a code smell
- switch expression with type patterns scaling better

**Answer**

`is` tests compatibility and returns a boolean without throwing, and the pattern form `if (b is Dog d)` also binds a typed variable on success so no separate cast is needed. A direct cast `(Dog)b` throws `InvalidCastException` when the object is not the expected type. In polymorphic code, excessive `is` chains are a code smell because they re-implement dispatch logic that virtual methods would handle automatically — a long `if (x is Cat) ... else if (x is Dog)` sequence should usually be replaced with a virtual method. When a closed type set genuinely requires type-based dispatch without a common virtual method, `switch` expressions with type patterns are more maintainable than chains of `is` checks.

---

## Q18. What is the Liskov Substitution Principle?

**Concepts**
- derived objects usable wherever base expected
- preconditions not strengthened in derived
- postconditions not weakened in derived
- Square/Rectangle as classic violation
- LSP violations indicating wrong inheritance choice

**Answer**

The Liskov Substitution Principle states that objects of a derived class must be usable anywhere their base class is expected without breaking correctness. A derived type must honor the base contract: it cannot strengthen preconditions (require more of callers than the base required) or weaken postconditions (deliver less to callers than the base promised). The classic violation is `Square : Rectangle` where settable width and height are meaningful on `Rectangle` but break on `Square` because setting width must also change height, violating callers who set width and height independently. When a derived class breaks callers of the base, the inheritance relationship is wrong; composition or a shared interface better expresses the relationship.

---

## Q19. When does `base.Method()` call the parent's implementation vs the current type's override?

**Concepts**
- base.Method() bypassing virtual dispatch for that call
- invoking immediate base class implementation directly
- useful for extending rather than replacing base behavior
- distinct from calling through a base-typed reference
- constructor virtual call danger unrelated to base.Method()

**Answer**

`base.Method()` explicitly invokes the base class's implementation for that virtual method slot, bypassing any override in the current or further-derived class for that specific call. It is a non-virtual dispatch pinned to the immediate base in the inheritance chain. This is useful when a derived override wants to extend rather than completely replace the base behavior — calling `base.Method()` first runs the parent logic, then the derived body adds on top. It differs from calling through a base-typed reference: `base.Method()` in a derived method always targets the declared base; a base-typed reference still dispatches to the most-derived override at runtime.

---

## Q20. What is the difference between extending behavior with inheritance vs wrapping with composition?

**Concepts**
- inheritance extending via subtype that is-a base
- composition wrapping via has-a collaborator
- inheritance coupling to base implementation details
- composition coupling only to collaborator interface
- decorator pattern stacking composed behaviors

**Answer**

Inheritance extends behavior by creating a subtype that IS-A base, overriding virtual methods to specialize behavior while reusing the base's other members. Composition wraps an inner object that the outer type HAS-A, forwarding calls and optionally intercepting them without subclassing the inner type's implementation. Inheritance couples the derived class to base internals — changes to the base can silently affect all subclasses. Composition couples only to the interface of the inner collaborator, which can be swapped at construction time with a different implementation or a mock. The Decorator pattern uses composition to stack multiple behaviors by wrapping the same interface, which is more flexible than creating a subclass for every combination.

### 06. Abstract Classes & Interfaces

---

## Q1. Explain abstraction in detail in C#.

**Concepts**
- abstraction hiding implementation behind a simplified interface
- abstract class with partial implementation and required overrides
- interface as a pure contract with no instance state
- use for expressing essential operations without exposing how
- enabling polymorphism and dependency inversion

**Answer**

Abstraction means exposing only the essential operations of a concept while hiding the implementation details behind a controlled interface. In C#, abstraction is achieved through abstract classes and interfaces. An abstract class defines a partial implementation — some methods are concrete and shared, others are abstract and must be overridden in each non-abstract subclass. An interface defines a pure contract specifying what a type can do without any instance state or implementation (before C# 8) or with optional default implementations (C# 8+). Abstraction enables callers to work against the simplified interface without knowing or depending on any specific implementation, which makes code extensible and testable.

---

## Q2. What is the difference between abstraction and encapsulation?

**Concepts**
- abstraction hiding what a thing does from the outside
- encapsulation hiding how it does it internally
- abstraction operating at the API design level
- encapsulation operating at the implementation level
- both reducing coupling but at different layers

**Answer**

Abstraction is about the design of a public API — it presents a simplified view of what a type or operation does, hiding the complexity behind an interface or abstract class. Encapsulation is about the protection of internal state — it hides the data and implementation details within a type using access modifiers, ensuring the type enforces its own invariants. Abstraction says "here is what you can do with this thing"; encapsulation says "here is the state this thing owns and manages privately." A `BankAccount` interface is abstraction; the `private decimal _balance` backing field and the `TryWithdraw` method that enforces invariants is encapsulation. Both reduce coupling, but abstraction targets the consumer's view while encapsulation targets the implementation's integrity.

---

## Q3. What is the difference between abstraction and polymorphism?

**Concepts**
- abstraction defining the simplified interface
- polymorphism enabling multiple implementations of that interface
- abstraction at design time, polymorphism at runtime
- abstract class or interface enabling polymorphic dispatch
- both working together in object-oriented design

**Answer**

Abstraction defines the interface — the simplified contract that callers use. Polymorphism is what happens at runtime when that interface is invoked: different concrete implementations each provide their own behavior, and the caller receives the right behavior for the actual type in use. Abstraction is a design-time activity — you choose which operations to expose and which to hide. Polymorphism is the runtime mechanism that makes the abstraction pay off — calling `shape.Draw()` produces different output for `Circle`, `Square`, and `Triangle` without the caller knowing which is in use. Without abstraction there is no stable interface to be polymorphic over; without polymorphism the abstraction is just a naming choice with no runtime benefit.

---

## Q4. What is the difference between an abstract class and an interface?

**Concepts**
- abstract class allowing state, constructors, and concrete methods
- interface defining a contract without instance state (pre-C# 8)
- single inheritance for abstract classes vs multiple interfaces
- abstract class for shared implementation, interface for capability contract
- access modifiers on abstract class members vs public-only interface

**Answer**

An abstract class can have instance fields, constructors, access modifiers on members, and a mix of concrete and abstract methods, making it suitable when a group of related types shares partial implementation. An interface traditionally defines only method, property, event, and indexer signatures without any state or implementation, though C# 8 added default method bodies. A class can inherit only one abstract base class but implement many interfaces. Choose an abstract class when subtypes genuinely share fields, constructors, or non-trivial concrete methods; choose an interface when unrelated types need to satisfy a common capability contract without sharing implementation.

---

## Q5. What is the difference between an abstract class and an interface before C# 8 vs after (default interface methods)?

**Concepts**
- pre-C# 8 interface as pure signature contract with no bodies
- C# 8 default interface methods providing optional implementation
- binary compatibility for adding members to interfaces
- dispatch nuance for default vs overriding implementations
- target framework requirements for default interface methods

**Answer**

Before C# 8, an interface could only declare signatures — no method bodies, fields, or constructors — so any new interface member was a breaking change for all implementors. Abstract classes could always provide concrete methods, so they were the only way to share implementation in a hierarchy. C# 8 introduced default interface methods, which allow an interface to supply a body for a member so existing implementors are not required to add the method to remain compilable. However, default interface methods require a runtime that supports them (not `netstandard2.0`) and have dispatch nuances: calling the default through the interface invokes the default unless the implementor overrides it, but calling on the concrete class type invokes only what the class explicitly provides. Abstract classes remain preferable when sharing instance state or constructors, since interfaces still cannot have these.

---

## Q6. Why do we need interfaces in C#?

**Concepts**
- multiple interface implementation without multiple class inheritance
- programming to abstraction for testability
- decoupling callers from specific implementations
- enabling polymorphism across unrelated type hierarchies
- contract documentation for consumers

**Answer**

Interfaces provide a way for unrelated types to satisfy the same capability contract without sharing a common base class, which is essential since C# classes can only inherit from one base. They are the primary mechanism for dependency inversion: when a class depends on `IRepository` rather than `SqlRepository`, any implementation can be injected including test doubles, alternate databases, or in-memory fakes. Interfaces also enable polymorphism across unrelated hierarchies — `string`, `List<T>`, and `Array` all implement `IEnumerable<T>` even though they share no common class ancestor. This makes interfaces the preferred abstraction mechanism for API design, since they express capability without implying identity or shared state.

---

## Q7. What is explicit interface implementation and when is it used?

**Concepts**
- explicit implementation accessible only through interface reference
- resolving name conflicts between two interfaces with same member name
- hiding members from the public API of the class
- accessing explicit members via cast to interface type
- different behavior on class reference vs interface reference

**Answer**

Explicit interface implementation declares a method as `ReturnType IInterface.Method(...)` without an access modifier, making it accessible only when the object is held through the named interface reference, not through the class type directly. It is used when two implemented interfaces define members with the same signature but different intended semantics, allowing each to have a separate implementation. It also hides interface-mandated members from the class's public API when exposing them publicly would be confusing or misleading — for example, `IDisposable.Dispose` explicitly implemented when the class exposes a more descriptive `Close()` method publicly. Callers must cast to the interface to reach the explicit implementation: `((IMyInterface)obj).Method()`.

---

## Q8. What are static abstract members in interfaces (C# 11)?

**Concepts**
- static abstract enabling polymorphism over type-level operations
- generic constraints required for static abstract usage
- use for generic math and operator polymorphism
- no instance dispatch since no object is involved
- static virtual providing optional default implementation

**Answer**

C# 11 introduced `static abstract` (and `static virtual` with a default) members in interfaces, which enable a form of polymorphism where the "implementation" is selected based on a type parameter rather than an object instance. Because there is no instance, callers must use a generic type parameter constrained to the interface: `T.Parse(string)` where `T : IParsable<T>`. This enables generic math where `T.operator +(T, T)` works across any numeric type implementing `IAdditionOperators<T,T,T>`. Static abstract members cannot be called on a concrete type directly without a generic context; they exist specifically to allow type-safe generic algorithms that are polymorphic over type-level operations like parsing, construction, or arithmetic operators.

---

## Q9. Can an abstract class have concrete (non-abstract) methods?

**Concepts**
- abstract class mixing abstract and concrete methods
- concrete methods providing shared default behavior
- abstract methods requiring override in non-abstract subclasses
- virtual concrete methods optionally overridable
- abstract class as partial implementation pattern

**Answer**

Yes — an abstract class can and typically does contain concrete methods alongside abstract ones. Concrete methods in an abstract class provide shared behavior that all subclasses inherit without needing to override, which is the main reason to use an abstract class instead of an interface. Abstract methods declare obligations that each non-abstract subclass must fulfill with its own implementation. Concrete methods can also be declared `virtual`, making them optionally overridable, or non-virtual, meaning subclasses inherit them unchanged. This mixture — shared implementation for common concerns, abstract requirements for type-specific behavior — is the abstract class's core advantage over interfaces when a family of related types shares significant behavior.

---

## Q10. Can a class implement multiple interfaces — what about an interface inheriting another interface?

**Concepts**
- class implementing unlimited number of interfaces
- interface inheritance extending contract with additional members
- implementing class must satisfy all inherited interface members
- explicit implementation for conflicting signatures
- interface hierarchy for capability layering

**Answer**

A class can implement any number of interfaces simultaneously: `class Worker : IEmployable, IPayable, ISchedulable` satisfies all three contracts. An interface can itself inherit from one or more other interfaces, extending the contract — `interface IReadWriteStream : IReadStream, IWriteStream` requires implementors to provide all members from both parent interfaces as well. When two implemented interfaces have members with the same signature, the class either provides one implementation satisfying both or uses explicit interface implementation to give each interface a separate body. This system allows capability layering without multiple class inheritance and without the diamond problem, since interfaces carry no instance state.

---

## Q11. When would you choose an abstract base class over an interface for shared implementation?

**Concepts**
- shared instance fields requiring abstract class
- constructor-enforced invariants requiring abstract class
- access-modifier-protected helpers requiring abstract class
- unrelated types sharing only a contract favoring interface
- template method pattern as classic abstract class use case

**Answer**

Choose an abstract class when the related types genuinely share instance state — fields that each subtype inherits and that establish shared identity or configuration. Abstract classes can enforce invariants through constructors that all derived classes must call, can provide protected helper methods with implementation, and can define fields with access modifiers that derived classes read through controlled accessors. Interfaces cannot have instance fields or constructors, so they cannot provide any of this. The template method pattern is the clearest signal: when a base class defines an algorithm skeleton with `protected abstract` steps that subclasses fill in, an abstract class is the right tool. When the common factor is only "this type can do X" with no shared state or helpers, an interface expresses the contract more cleanly.

---

## Q12. What is the diamond problem, and how does C# address it for classes and interfaces?

**Concepts**
- diamond problem arising from two base classes sharing an ancestor
- ambiguous method resolution without explicit override
- C# preventing it for classes via single inheritance
- C# 8 interfaces requiring explicit disambiguation for default method conflicts
- most-specific override rule for interface default methods

**Answer**

The diamond problem occurs when a type inherits from two sources that both trace back to a common ancestor with the same method, leaving the compiler unable to determine which implementation to use. C# avoids it for classes entirely through single class inheritance — there is only one path up the hierarchy so no ambiguity is possible. For interfaces with default methods (C# 8+), the diamond can arise when a class implements two interfaces that each provide a default implementation of the same member tracing back to a common base interface. C# resolves this with the most-specific override rule: if one interface's default is explicitly more derived than another's, it wins. When both are equally specific, the compiler requires the implementing class to provide an explicit override that disambiguates, otherwise it is a compile error.

---

## Q13. What is explicit interface implementation — why might `((IMyInterface)obj).Method()` work when `obj.Method()` does not?

**Concepts**
- explicit member not in the class's public API
- accessible only through interface-typed reference
- class reference resolving to nothing or different method
- name collision resolution between interfaces
- hiding interface-mandated members from class surface

**Answer**

When a method is explicitly implemented as `void IMyInterface.Method()`, it has no access modifier and does not appear on the class's public member list. Calling `obj.Method()` on a variable typed as the class fails to compile because there is no public method named `Method` on the class. Accessing the method requires holding the object through the interface: `((IMyInterface)obj).Method()` succeeds because the explicit implementation is visible through the interface reference. This pattern is used to hide members that are required by an interface contract but are not appropriate to expose on the class's own API — for example, explicit `IEnumerator.Reset()` on enumerators that do not logically support reset — or to resolve naming conflicts when two implemented interfaces demand the same member name with different semantics.

---

## Q14. Can interfaces declare fields, constructors, or static concrete state?

**Concepts**
- interfaces having no instance fields ever
- no constructors in interfaces
- C# 8 adding static private fields as implementation detail for default methods
- C# 11 adding static abstract and static virtual members
- these additions not changing the no-instance-state rule

**Answer**

Interfaces have never been allowed to declare instance fields or instance constructors, and this restriction remains in all C# versions. C# 8 added default interface methods, and to support them the spec allows `private static` fields within interfaces as implementation details of default methods — but these are type-level, not instance-level. C# 11 added `static abstract` and `static virtual` members, which can include static properties. None of these additions permit instance state; an interface still cannot hold per-object data or define initialization logic. Constructors in interfaces would be meaningless since you cannot instantiate an interface directly.

---

## Q15. What is the difference between `IReadOnlyList<T>` as a parameter type and `List<T>`?

**Concepts**
- IReadOnlyList<T> accepting arrays, lists, and any read-only collection
- List<T> restricting callers to that specific concrete type
- read-only contract preventing mutation through the parameter
- callers passing any compatible implementation
- widening parameter type reducing coupling

**Answer**

Using `IReadOnlyList<T>` as a parameter type accepts any read-only indexed sequence — `T[]`, `List<T>`, `ImmutableList<T>`, or any custom implementation — because they all implement the interface. Using `List<T>` forces callers to provide exactly a `List<T>`, which excludes arrays and other sequences and creates an unnecessary coupling to a specific collection implementation. The read-only interface also signals intent: the method promises not to mutate the collection, which callers can rely on. Widening parameter types to the narrowest sufficient abstraction — preferring `IEnumerable<T>` when only iteration is needed, `IReadOnlyList<T>` when indexed access is needed, `IList<T>` when mutation is needed — is a key API design principle.

---

## Q16. When should API surface depend on interfaces vs abstract classes?

**Concepts**
- interfaces for public API boundaries consumed by unrelated callers
- abstract classes for family hierarchies with shared state
- interfaces enabling testability via mock injection
- abstract classes when callers are closely related subclasses
- mixing both for shared implementation plus broad contractual usage

**Answer**

Public API surface that will be consumed by unrelated callers — including test doubles — should depend on interfaces, because interfaces impose no implementation constraints and any type can satisfy them. Interfaces are the right tool when the contract is "you can do X" and the caller has no reason to care about what state or helpers an implementor uses internally. Abstract classes are appropriate when the API is the base of a closely related family — the abstract class establishes shared state, protected helpers, and partial implementation that all subclasses inherit, and callers are the subclasses themselves rather than unrelated consumers. In practice, the two are often combined: an abstract class `DocumentBase` provides shared implementation for the family, and `IExportable` provides a broad contract that any document type plus unrelated types can satisfy.

### 07. Encapsulation & Access Modifiers

---

## Q1. Explain encapsulation in C# with examples.

**Concepts**
- hiding internal state behind controlled accessors
- access modifiers restricting who can read or write state
- methods enforcing invariants on state changes
- private fields with public property accessors
- encapsulation preventing external invariant violation

**Answer**

Encapsulation bundles state and the rules governing it into a single type, hiding the internal details behind a controlled interface. A `BankAccount` class stores balance in a `private decimal _balance` field and exposes it read-only through `public decimal Balance => _balance`, while mutations go through `Deposit(decimal amount)` and `TryWithdraw(decimal amount, out string message)` which enforce business rules like no-negative-amounts and frozen-account checks. Without encapsulation, callers could assign `Balance` directly and bypass those rules, so the type cannot guarantee it is always in a valid state. Access modifiers — `private`, `protected`, `internal`, `public` — are the language mechanism; the discipline of routing all state changes through validated methods is the design principle.

---

## Q2. What are the different access modifiers in C#?

**Concepts**
- private accessible within declaring type only
- protected accessible to declaring type and derived classes
- internal accessible within the same assembly
- protected internal union of protected and internal
- private protected intersection of protected and same-assembly

**Answer**

C# provides five access modifiers. `private` restricts access to the declaring type only — no subclass, no assembly peer. `protected` allows access within the declaring type and any derived class, regardless of assembly. `internal` allows access to any code within the same assembly but nothing outside it. `protected internal` is the union: accessible to derived classes in any assembly OR to any code in the same assembly, whichever applies. `private protected` is the intersection: accessible only to derived classes that are also in the same assembly, making it the narrowest combined modifier. Types themselves at the top level can only be `public` or `internal`; nested types may use all five.

---

## Q3. What is the difference between "information hiding" and "data hiding"?

**Concepts**
- data hiding concealing raw fields behind property accessors
- information hiding concealing implementation details broadly
- information hiding applying to algorithms, dependencies, and structure
- data hiding as a subset of information hiding
- both reducing coupling and improving maintainability

**Answer**

Data hiding is the narrower concept: keeping raw fields private and exposing them only through property accessors so callers cannot directly manipulate storage. Information hiding is broader: concealing any implementation detail that callers need not know — including which algorithm is used, which dependencies are held, which data structures back a collection, and how internal state is organized. Data hiding is one tool for information hiding. A class that exposes its sorting algorithm name through a public property is hiding data (the field is private) but failing at information hiding (the algorithm choice is an implementation detail callers should not depend on). The goal of both is reducing the coupling between the type and its callers.

---

## Q4. Why is exposing a mutable collection through a public getter an encapsulation break?

**Concepts**
- caller getting direct reference to internal list
- Add and Remove bypassing the type's invariants
- Clear or Replace via setter bypassing ownership entirely
- IReadOnlyList<T> or defensive copy as solutions
- encapsulation requiring all mutations go through methods

**Answer**

When a property returns the live mutable collection that the type owns internally, callers can call `items.Add(...)`, `items.Remove(...)`, or `items.Clear()` directly and the owning type has no way to observe or validate those changes. This bypasses any sorting, deduplication, capacity limits, or notification logic the type manages through its own methods. If the property also has a public setter, callers can replace the entire collection with one they control, severing the type's ownership entirely. The fix is to expose only `IReadOnlyList<T>` or `IReadOnlyCollection<T>` through the getter, return a defensive copy if the caller needs to work with a detached list, and provide specific mutating methods like `AddItem(T item)` on the owning type.

---

## Q5. What is the difference between `protected internal` and `private protected`?

**Concepts**
- protected internal as a union: protected OR internal
- private protected as an intersection: protected AND same-assembly
- protected internal accessible to derived classes anywhere plus same-assembly peers
- private protected accessible only to derived classes in the same assembly
- private protected as the narrower of the two combined modifiers

**Answer**

`protected internal` is a union: the member is accessible to any derived class regardless of assembly, and also to any code in the same assembly regardless of whether it derives from the declaring type. `private protected` is an intersection: the member is accessible only to code that is both a derived class and in the same assembly. If an external assembly defines a derived class, it can access `protected internal` members (the `protected` side applies) but cannot access `private protected` members (the assembly requirement fails). `private protected` is the right choice when a member is an extension hook intended only for subclasses built as part of the same library, not for external subclasses.

---

## Q6. What does `internal` mean in the context of assemblies and `InternalsVisibleTo`?

**Concepts**
- internal visible only within the declaring assembly
- InternalsVisibleTo granting named assemblies access to internal members
- test assembly as the canonical friend
- strong-named assemblies requiring public key in InternalsVisibleTo
- InternalsVisibleTo as a compile-time visibility grant not a security boundary

**Answer**

`internal` restricts access to any code compiled into the same assembly — the `.dll` or `.exe` — and is invisible to any code in other assemblies. `InternalsVisibleTo` in an assembly's source grants one or more named friend assemblies compile-time access to that assembly's `internal` members, as if they were in the same assembly. The canonical use is test assemblies: `[assembly: InternalsVisibleTo("MyLib.Tests")]` lets unit tests call internal helpers without making them public on the NuGet surface. For strong-named assemblies, the friend's public key must be specified or the access grant silently fails. `InternalsVisibleTo` is a visibility mechanism, not a security boundary — it applies to compile-time access only and does not prevent reflection-based access at runtime.

---

## Q7. What is the default access level for class members if you omit a modifier?

**Concepts**
- class members defaulting to private
- top-level types defaulting to internal
- nested types defaulting to private
- interface members defaulting to public
- enum members always public

**Answer**

Class and struct members default to `private` when no access modifier is specified. Top-level types (non-nested) default to `internal`. Nested types default to `private`, the same as other class members. Interface members default to `public` because their purpose is to define a public contract — although C# 8 explicit access modifiers on default implementations may alter this for individual members. Enum members are always `public` regardless of the enum's own accessibility. Knowing the defaults prevents accidental over-exposure (forgetting a modifier on a member that should be private is safe) and accidental under-exposure (forgetting `public` on a top-level class makes it assembly-internal).

---

## Q8. How do access modifiers apply to nested types vs top-level types?

**Concepts**
- top-level types limited to public or internal
- nested types supporting all five access modifiers
- nested types defaulting to private
- private nested type visible only within the enclosing type
- protected nested type accessible to derived classes of outer type

**Answer**

Top-level types (not nested inside another type) can only be `public` or `internal` — the other modifiers are not meaningful without an enclosing scope. Nested types — declared inside a class, struct, or interface — can use all five access modifiers including `private` and `protected`. A `private` nested type is visible only within the enclosing type, making it a true implementation detail. A `protected` nested type is accessible to derived classes of the enclosing type. A `private protected` nested type is accessible to derived classes in the same assembly. This lets complex types encapsulate their helper or builder types as deeply as needed without exposing them in the public API.

---

## Q9. What is defensive copying when returning collections from properties?

**Concepts**
- returning a copy instead of the live collection reference
- preventing callers from mutating internal state through the copy
- ToList() or ToArray() as common copy mechanisms
- performance cost of copying on every access
- IReadOnlyList<T> as a lighter alternative that avoids copying

**Answer**

Defensive copying means the property getter returns a new collection object containing the same elements rather than returning a reference to the internal collection. Callers who add, remove, or clear elements on the returned copy do not affect the original because they have an independent list. The typical pattern is `return _items.ToList()` or `return _items.ToArray()`. The cost is a full allocation and copy on every get, which is acceptable for collections accessed infrequently but expensive for hot paths. A lighter alternative that avoids copying is returning `_items.AsReadOnly()` or typing the property as `IReadOnlyList<T>`, which prevents callers from calling mutating methods without the allocation overhead of a full copy.

---

## Q10. What is the difference between encapsulation and immutability?

**Concepts**
- encapsulation controlling who can change state through methods
- immutability preventing any change to state after construction
- encapsulation allowing controlled mutation through validated methods
- immutability eliminating mutation entirely
- immutability as a stronger constraint useful for concurrency and value semantics

**Answer**

Encapsulation controls access — it hides state behind methods that validate mutations, so the object can change but only in ways the type explicitly permits. Immutability prevents change entirely — once an object is constructed, its state cannot be modified by anyone including the owning type. A `BankAccount` is encapsulated but mutable: only `Deposit` and `TryWithdraw` can change the balance, and they enforce rules. An immutable `Money` record cannot change after creation; operations produce new values. Encapsulation is sufficient for most domain objects where state evolution is expected. Immutability simplifies concurrent access and reasoning about value semantics, since there are no race conditions on reads and no aliasing hazards.

---

## Q11. Why are public fields discouraged in public APIs even for simple DTOs?

**Concepts**
- public field exposing implementation storage directly
- no interception on read or write
- binary incompatibility when changing field to property
- serialization and data-binding frameworks preferring properties
- properties enabling future validation without API change

**Answer**

A public field exposes the storage mechanism directly, so any future need to add validation, raise change notifications, compute a derived value, or serialize with a different name requires changing the field to a property — which is a binary-incompatible change that recompiles consumers. Serialization frameworks and data-binding systems like WPF and ASP.NET model binding discover properties by convention and may skip fields. A public auto-property `{ get; set; }` looks identical to callers at the source level but compiles to get/set methods, giving the type room to add logic later without breaking anyone. Even for DTOs where no validation is expected today, the consistent practice of using properties avoids an API churn when requirements change.

---

## Q12. How does `private protected` restrict visibility compared to `protected` alone?

**Concepts**
- protected visible to all derived classes in any assembly
- private protected visible only to derived classes in the same assembly
- external subclasses unable to access private protected members
- use for same-library extension hooks
- narrowest possible combined visibility

**Answer**

`protected` alone makes a member accessible to any derived class, regardless of which assembly that class lives in. This means third-party libraries or plugin assemblies that subclass your type can read and write `protected` members, which widens your effective public surface unexpectedly. `private protected` adds the requirement that the accessing code must also reside in the same assembly. External derived classes — plugin projects, consumer subclasses — receive a compile error when they try to access `private protected` members. This is the right choice for extension hooks that are internal implementation details for your own team's subclasses but should not form part of the contract for external extenders.

---

## Q13. What is a friend assembly pattern, and what are its trade-offs?

**Concepts**
- InternalsVisibleTo granting named assembly access to internal members
- test assembly as the primary legitimate use case
- maintenance coupling between friend and provider assemblies
- overuse turning internal into effectively public
- strong-naming requirement for signed assemblies

**Answer**

The friend assembly pattern uses `[assembly: InternalsVisibleTo("FriendAssembly")]` to grant a named assembly access to `internal` types and members at compile time, without exposing them publicly in the NuGet or binary API surface. The main legitimate use is test assemblies: unit tests can exercise internal helpers, validators, and factories without making them `public`. The trade-off is that every friend assembly creates a maintenance coupling — internal refactors that rename or remove members break the friend. Overuse promotes `internal` to effectively "public but inconvenient," which defeats the purpose of encapsulation. For signed assemblies, the friend's public key must be included in the attribute, or access is silently denied.

---

## Q14. How do property accessors use asymmetric access (`public get; private set;`)?

**Concepts**
- asymmetric access combining different visibility on get and set
- public getter exposing value for reading
- private set restricting mutation to the owning class
- protected set for hierarchy-controlled mutation
- common pattern for domain objects with encapsulated state changes

**Answer**

Asymmetric access lets a property be readable by the public while being settable only from within the declaring class: `public decimal Balance { get; private set; }`. This exposes the value for observation without allowing external code to assign it directly. Any mutation must go through a method that the class controls, such as `Deposit` or `Withdraw`, which can enforce invariants before changing the backing value. `protected set` extends mutation rights to derived classes while still hiding the setter from external callers. This pattern is a practical middle ground between a fully read-only property (which requires a constructor or backing field assignment) and a fully public property (which exposes mutation without control).

### 08. Events

---

## Q1. Explain events in C# (including event handling and publisher-subscriber pattern).

**Concepts**
- event as encapsulated multicast delegate
- publisher raising the event without knowing subscribers
- subscriber registering handler with +=
- EventHandler and EventHandler<T> as standard delegate signatures
- loose coupling between publisher and zero or more subscribers

**Answer**

An event in C# is a member backed by a multicast delegate, restricted by the `event` keyword so outside code can only subscribe (`+=`) or unsubscribe (`-=`) but cannot raise the event or replace all handlers. The publisher-subscriber pattern has the publisher type declare the event and raise it when something meaningful happens — `BankAccount` raises `BalanceChanged` after a deposit — while subscriber objects attach handler methods with `+=` and remove them with `-=`. Because the publisher knows nothing about its subscribers and subscribers know nothing about each other, the pattern decouples these concerns: adding a new subscriber requires no change to the publisher. When the event is raised, every subscribed handler is invoked in subscription order through the multicast delegate chain.

---

## Q2. What is the difference between an `event` and a plain public delegate field?

**Concepts**
- event restricting outsiders to += and -= only
- public delegate field allowing external invoke and = assignment
- CS0070 error on external raise of event
- encapsulation protecting publisher's raise logic
- event as the correct tool for publish-subscribe APIs

**Answer**

A plain `public Action<string>? Completed` field lets any code outside the class invoke the delegate directly (`gateway.Completed("x")`) or replace all subscribers with `gateway.Completed = null`, which silently removes every registered handler without anyone's consent. The `event` keyword restricts outside code to `+=` and `-=` only; any attempt to invoke the event or assign to it from outside the declaring class is a compile error (CS0070). Only the declaring class can raise the event. This encapsulates the raise logic inside the publisher, prevents accidental clearing of the subscriber list, and prevents third-party code from firing fake notifications that could corrupt downstream state.

---

## Q3. Why should you unsubscribe from events, and what problem does this prevent?

**Concepts**
- publisher holding reference to subscriber via delegate
- GC unable to collect subscribers still referenced by event
- memory leak accumulating as subscribe-without-unsubscribe cycles
- -= in Dispose or equivalent cleanup method
- named handler method enabling matching unsubscribe

**Answer**

When an object subscribes to an event, the publisher's delegate list holds a reference to the subscriber's handler method (and through it, to the subscriber object itself if a lambda captures `this`). If the subscriber is logically done — a UI panel closed, a scoped service disposed — but never unsubscribes, the publisher's long-lived delegate list keeps the subscriber in memory indefinitely, preventing the GC from collecting it. This creates a leak that accumulates with every subscribe-without-unsubscribe cycle. The fix is to unsubscribe in a `Dispose` method or lifecycle cleanup hook using the same delegate reference used for `+=`. Using a named instance method rather than a lambda makes the matching `-=` straightforward.

---

## Q4. What happens during multicast delegate invocation if one subscriber throws?

**Concepts**
- multicast delegate invoking handlers in subscription order
- exception from one handler aborting remaining handlers
- remaining handlers after the throwing one not called
- GetInvocationList for exception-isolated invocation
- aggregate exception pattern for collecting all failures

**Answer**

When a multicast delegate is invoked, handlers are called in the order they were subscribed. If one handler throws an unhandled exception, the default invocation stops at that point and the remaining handlers in the chain are never called. This means a poorly-behaved subscriber can silently prevent other subscribers from receiving the event. To invoke all handlers and collect exceptions independently, use `GetInvocationList()` to iterate handlers one at a time in a try-catch loop, which lets you accumulate failures in an `AggregateException` and ensure every subscriber is attempted regardless of prior failures. This pattern is important in frameworks where all subscribers must receive notification even if some are buggy.

---

## Q5. What is the standard `EventHandler` / `EventHandler<TEventArgs>` pattern?

**Concepts**
- EventHandler<TEventArgs> as the BCL-standard delegate signature
- sender as object for the raising instance
- EventArgs subclass carrying event-specific data
- parameterless EventHandler for events with no payload
- convention enabling consistent event wiring across BCL and user code

**Answer**

The standard pattern uses `EventHandler<TEventArgs>` where the delegate signature is `void Handler(object? sender, TEventArgs e)`. The `sender` parameter provides the object that raised the event, and `e` is a custom `EventArgs` subclass carrying event-specific data — for example `BalanceChangedEventArgs` with `NewBalance` and `OldBalance` properties. For events with no meaningful payload, the non-generic `EventHandler` delegate (with `EventArgs.Empty`) is used. Following this convention ensures that any C# developer can immediately recognize the pattern, that tooling can correctly identify event handlers, and that framework infrastructure for event wiring recognizes and supports the signature.

---

## Q6. How do you raise an event safely?

**Concepts**
- null-conditional invoke handling no-subscribers case
- local copy preventing race between null-check and invoke
- protected virtual OnX method as the raise helper
- null check required because zero subscribers leaves delegate null
- thread-safe local copy for concurrent subscribe/unsubscribe

**Answer**

The idiomatic safe raise uses null-conditional invocation: `BalanceChanged?.Invoke(this, e)`. Since an event with no subscribers is represented as a `null` delegate, calling it directly would throw `NullReferenceException`. In multi-threaded scenarios where subscribers may unsubscribe concurrently, copy the delegate reference to a local variable before invoking it: `var handler = BalanceChanged; handler?.Invoke(this, e)`. The local copy captures the invocation list at that moment, so even if another thread unsubscribes the last handler between the copy and the invoke, the copy is non-null and the call is safe. The conventional structure wraps this in a `protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)` method so derived classes can override the raise behavior.

---

## Q7. What is the difference between custom delegate types and `EventHandler` for events?

**Concepts**
- EventHandler<T> as the BCL-recommended standard signature
- custom delegate types for non-standard parameter shapes
- EventHandler enforcing object sender and EventArgs e convention
- custom delegate allowing direct typed parameters
- interoperability and tooling recognition favoring EventHandler

**Answer**

`EventHandler<TEventArgs>` enforces the `(object? sender, TEventArgs e)` signature that matches BCL conventions, makes event handlers immediately recognizable, and ensures compatibility with framework infrastructure that expects this shape. A custom delegate type like `delegate void PriceChangedHandler(decimal oldPrice, decimal newPrice)` allows a different parameter shape — no sender, no EventArgs wrapping — which can be cleaner for simple domain-internal events but is less interoperable with generic tooling. The trade-off is ergonomics vs convention: use `EventHandler<T>` for events that may be consumed by general-purpose infrastructure or external callers; custom delegates are acceptable for tightly-scoped internal events where the strongly-typed parameters are clearer than boxing values into an `EventArgs` subclass.

---

## Q8. Can interfaces declare events, and how are they implemented?

**Concepts**
- interface declaring event with standard EventHandler pattern
- implementing class providing += and -= accessors
- explicit event implementation for name conflicts
- callers subscribing through interface reference
- same hiding patterns as explicit method implementation

**Answer**

Interfaces can declare events: `event EventHandler<T>? SomeEvent;`. Implementing classes provide the event either by declaring a matching `event` field (the compiler generates accessors) or by providing explicit `add { }` and `remove { }` accessors. When two implemented interfaces declare events with the same name, explicit event implementation — `event EventHandler IFirst.SomeEvent { add {...} remove {...} }` — provides separate implementations per interface. Callers holding an interface-typed reference subscribe and unsubscribe through that interface, and the dispatch reaches the class's implementation. The same encapsulation rules apply: only the implementing class can raise the event even when declared through an interface.

---

## Q9. What memory-leak scenario arises when a long-lived publisher holds references to short-lived subscribers?

**Concepts**
- publisher's delegate list retaining reference to subscriber
- GC unable to reclaim short-lived subscriber
- UI controls closing without unsubscribing from domain events
- scoped services subscribing to singleton events without cleanup
- weak event patterns and IDisposable unsubscribe as mitigations

**Answer**

When a long-lived publisher — a static service, a singleton, a domain aggregate with application lifetime — holds an event to which short-lived subscribers have attached, those subscribers cannot be garbage collected because the publisher's delegate list holds a strong reference to each handler (and through captured variables, to the subscriber object itself). A WPF panel that subscribes to `account.BalanceChanged` in its constructor and is later "closed" but never disposed remains in memory as long as the `BankAccount` exists. The fix is to unsubscribe in the subscriber's `Dispose` or `IAsyncDisposable` implementation. When explicit unsubscription is impractical, weak event patterns or a mediator that does not hold references between events can prevent the leak.

---

## Q10. What is the difference between events and the Observer pattern / IObservable?

**Concepts**
- C# events as a language-level multicast delegate mechanism
- Observer pattern as a design pattern with explicit Subject and Observer roles
- IObservable<T> as the reactive pull-push stream contract
- events having no built-in completion or error notification
- IObservable supporting OnNext, OnError, and OnCompleted

**Answer**

C# events are a language mechanism backed by multicast delegates; they fire notifications on demand but have no concept of completion, error propagation, or sequence termination. The Observer pattern is a broader design pattern that defines a `Subject` maintaining a list of `Observer` objects and notifying them of state changes — C# events are one implementation of this pattern. `IObservable<T>` from the Reactive Extensions model represents an asynchronous stream of values with three notification types: `OnNext(T)` for each value, `OnError(Exception)` for a terminal error, and `OnCompleted()` for stream termination. `IObservable<T>` also supports composition, filtering, and transformation via LINQ-style operators, which plain events do not. Use events for simple domain notifications; use `IObservable<T>` when stream composition, backpressure, or lifecycle semantics are needed.

---

## Q11. Can you assign to an event from outside the declaring class?

**Concepts**
- event restricting external code to += and -= only
- = assignment on event outside declaring class is CS0070
- only the declaring class can raise or replace the invocation list
- test code needing fresh instances rather than = null resets
- delegate field allowing = but breaking encapsulation

**Answer**

No — code outside the declaring class can only subscribe (`+=`) and unsubscribe (`-=`) from an event. Attempting to assign (`=`) or invoke the event from outside the declaring type is compile error CS0070. Only the class that declares the event can raise it or manipulate the underlying delegate directly. This restriction is what makes `event` different from a plain `public` delegate field: a public field allows external `= null` to wipe all subscribers, which `event` prevents. Test code that wants to start with a clean event state should use a fresh publisher instance rather than trying to null out the event from outside.

---

## Q12. What is thread-safe event raising, and when is locking required?

**Concepts**
- local delegate copy as the minimal thread-safe raise pattern
- concurrent subscribe/unsubscribe not corrupting the copy
- default add/remove accessors using Interlocked internally
- lock required for compound read-raise-decide sequences
- custom add/remove with explicit lock for stricter ordering

**Answer**

The minimal thread-safe raise pattern copies the delegate to a local variable before invoking: `var h = MyEvent; h?.Invoke(this, e)`. This is safe because delegate assignment in .NET is atomic — the local variable captures a complete invocation list snapshot — and even if another thread unsubscribes concurrently, the copy remains valid for the duration of the raise. The default compiler-generated `add` and `remove` accessors use `Interlocked.CompareExchange` internally, so subscribe and unsubscribe are themselves thread-safe. Explicit locking is needed only when the raise decision depends on state that must be consistent with the event invocation — for example, "raise only if count > 0" where another thread could change count between the check and the raise — in which case a `lock` wrapping both the check and the invoke is required.

### 09. OOP Real-World Examples

---

## Q1. Explain the SOLID principles with concrete C# examples.

**Concepts**
- Single Responsibility: one reason to change per class
- Open/Closed: open for extension, closed for modification
- Liskov Substitution: derived types substitutable for base
- Interface Segregation: small focused interfaces over fat ones
- Dependency Inversion: depend on abstractions not concretions

**Answer**

SOLID is five principles that guide maintainable OOP design. Single Responsibility means a class has one reason to change — `OrderValidator` validates, `OrderRepository` persists, not one class that does both. Open/Closed means extending behavior by adding new classes rather than editing existing ones — adding `PushNotificationSender : INotificationSender` extends the notification system without touching the `OrderFulfillmentService`. Liskov Substitution means derived types must be safely substitutable for their base — `Square : Rectangle` violates this when setting width independently breaks area invariants. Interface Segregation means clients should not depend on members they do not use — split a fat `IDocumentCapabilities` into `IExportable`, `IPrintable`, and `ISignable`. Dependency Inversion means high-level modules depend on abstractions — `OrderService` depends on `IPaymentProcessor`, not on `CardPaymentProcessor` directly.

---

## Q2. What is the Liskov Substitution Principle? Give a classic violation.

**Concepts**
- substitutability of derived for base without breaking behavior
- preconditions not strengthened in derived
- postconditions not weakened in derived
- Square/Rectangle as the canonical violation
- invariant preservation as the key test

**Answer**

The Liskov Substitution Principle states that every instance of a derived class must be usable in place of a base class instance without breaking the program's correctness. The classic violation is `Square : Rectangle`: `Rectangle` has independently settable `Width` and `Height`, and code that sets `Width = 5` and then asserts `Area == 5 * Height` is correct for a `Rectangle` but breaks for a `Square`, because setting `Width` on a square must also change `Height`. When existing code that works correctly with `Rectangle` fails with `Square`, the is-a inheritance relationship is wrong. A design that models the relationship as a `Shape` interface with an `Area` property, where both implement independently, avoids the violation entirely.

---

## Q3. What is Dependency Inversion, and how does constructor injection implement it?

**Concepts**
- high-level modules depending on abstractions not concretions
- low-level modules implementing abstractions
- constructor injection as the primary DI mechanism
- abstractions defined in terms of what callers need
- testability and replaceability as outcomes

**Answer**

Dependency Inversion says high-level policy modules should not depend on low-level implementation details; both should depend on abstractions. Rather than `OrderService` creating a `new SqlOrderRepository()` internally, it declares `private readonly IOrderRepository _repo` and receives an `IOrderRepository` through its constructor. The concrete `SqlOrderRepository : IOrderRepository` is resolved by the DI container at startup. Constructor injection makes dependencies explicit and visible in the constructor signature, which makes the class testable — you can pass a fake `InMemoryOrderRepository` in unit tests — and replaceable — swapping `SqlOrderRepository` for `CosmosOrderRepository` requires no change to `OrderService`. The abstraction is defined in terms of what `OrderService` needs, not what the database supports.

---

## Q4. What is the difference between Dependency Injection and the Service Locator pattern?

**Concepts**
- DI pushing dependencies in through constructor or parameters
- Service Locator pulling dependencies out from a global registry
- DI making dependencies explicit and visible
- Service Locator creating hidden dependencies and global coupling
- DI enabling testability without global state

**Answer**

Dependency Injection pushes dependencies into a class through its constructor (or method/property), making all dependencies explicit in the class signature. Service Locator has the class pull its own dependencies from a global registry: `var repo = ServiceLocator.Get<IOrderRepository>()`. With DI, you can see exactly what a class needs from its constructor; with Service Locator, dependencies are hidden inside the method body and the class couples itself to the locator globally. Testing with DI is straightforward — pass fakes through the constructor. Testing with Service Locator requires configuring the global registry before each test, creating ordering dependencies and potential test pollution. Service Locator is widely considered an anti-pattern for application code for these reasons.

---

## Q5. What is the difference between "has-a" and "is-a" relationships? When is inheritance the wrong choice?

**Concepts**
- is-a relationship justifying inheritance
- has-a relationship justifying composition
- inheritance implying substitutability by LSP
- reuse without substitutability favoring composition
- Stack extending List as a classic wrong inheritance

**Answer**

An is-a relationship means a derived type truly is a specialized form of the base and can be substituted for it — `Manager : Employee` is valid because a manager is an employee in every context that handles employees. A has-a relationship means a type contains another as a component — `OrderService` has an `IOrderRepository`, not is-a repository. Inheritance is wrong when the only motivation is code reuse without a true is-a relationship. The `Stack<T> : List<T>` design in older Java code is the canonical mistake: a stack is not a list and should not expose `Insert`, `RemoveAt`, or arbitrary indexing. The correct model is for `Stack<T>` to contain a `List<T>` internally and expose only `Push`, `Pop`, and `Peek`.

---

## Q6. What is the anemic domain model anti-pattern?

**Concepts**
- domain objects as pure data bags
- business logic in external service classes
- invariants unenforceable at the object level
- rich domain model as the alternative
- scattered and duplicated rules as the consequence

**Answer**

An anemic domain model has classes that are pure data bags — only public getters and setters — while all business logic lives in separate service classes that operate on those data bags procedurally. A `Customer` with public `Balance` and `IsActive` setters that a `CustomerService` reads and writes directly is anemic: there is no place in the `Customer` type to enforce that balance never goes negative or that inactive customers cannot accrue transactions. Rules scatter across service classes, duplicate across use cases, and are easy to bypass. A rich domain model encapsulates rules inside the domain objects — `Customer.TryWithdraw` enforces the balance invariant — so the object cannot reach an invalid state regardless of which service method is called.

---

## Q7. What is the Open/Closed Principle, and how do interfaces support extension without modification?

**Concepts**
- open for extension by adding new implementations
- closed for modification of existing stable code
- interface enabling new types without editing callers
- polymorphic dispatch routing to new behavior automatically
- strategy and decorator as patterns relying on OCP

**Answer**

The Open/Closed Principle says software entities should be open for extension but closed for modification — you should be able to add new behavior without editing existing, tested code. Interfaces enable this: when `OrderFulfillmentService` loops over `IEnumerable<INotificationSender>`, adding `PushNotificationSender : INotificationSender` extends the system without touching the service. If the service instead had a long `if (type == "email") ... else if (type == "sms")` chain, adding push would require editing and retesting that chain. The polymorphic dispatch driven by the interface routes calls to the new implementation automatically. The Strategy and Decorator patterns both rely on OCP: strategies are new implementations of an interface injected at runtime; decorators wrap existing implementations with new behavior without changing the wrapped class.

---

## Q8. What is the Single Responsibility Principle — how do you recognize a class that violates it?

**Concepts**
- one reason to change per class
- responsibility as a cohesive area of concern
- multiple unrelated concerns as a violation signal
- large number of dependencies as a code smell
- frequent changes for different reasons as runtime evidence

**Answer**

The Single Responsibility Principle says a class should have one reason to change — one area of concern that drives its evolution. A class that violates it will need to change when business rules for validation change, when the persistence mechanism changes, when the notification format changes, and when the logging format changes, all independently. A common signal is a constructor with many injected dependencies covering unrelated concerns: `OrderService(IValidator, IRepository, INotifier, ILogger, IInvoiceGenerator, IEmailService)` suggests the class is doing too much. The fix is to split responsibilities: `OrderValidator`, `OrderRepository`, `OrderNotificationService`, each with a focused constructor and a single reason to evolve.

---

## Q9. What is the Interface Segregation Principle — why are fat interfaces problematic?

**Concepts**
- clients not forced to depend on methods they do not use
- fat interface forcing stubs for unused members
- ISP violation producing unnecessary implementation coupling
- small focused interfaces as the remedy
- callers declaring minimal required contract

**Answer**

The Interface Segregation Principle says clients should not be forced to implement or depend on interface members they do not use. A fat interface like `IDocumentCapabilities` that bundles export, print, PDF rendering, signing, and naming forces every implementor to provide all of them — a `ReportDocument` that only exports must stub or throw on signing and printing. This creates unnecessary coupling between unrelated capabilities. Small focused interfaces — `IExportable`, `IPrintable`, `ISignable` — let each implementor satisfy only what applies, and each caller declare the narrowest contract it needs: `ExportService` accepts `IEnumerable<IExportable>` rather than the full capabilities type.

---

## Q10. What is a factory method vs a simple constructor — when do you introduce a factory?

**Concepts**
- constructor for fundamental invariant-guaranteed creation
- factory method for conditional or named creation
- factory returning different subtypes based on arguments
- factory enabling async initialization
- factory encapsulating validation in a named operation

**Answer**

A constructor is appropriate when creation is straightforward: supply required values, enforce basic invariants, and return a fully initialized object. A factory method is preferable when creation can fail in ways that should return a typed result rather than throw, when different arguments should yield different concrete subtypes (`Order.CreateRushOrder(...)` vs `Order.CreateStandardOrder(...)`), when the operation name carries domain meaning beyond "construct a T", or when initialization requires async work that constructors cannot express. Factory methods also allow private constructors so the only valid entry points are the named methods, which makes the creation API explicit and prevents callers from bypassing validation by choosing an inappropriate overload.

---

## Q11. What is the Strategy pattern, and how does it map to interfaces/delegates in C#?

**Concepts**
- Strategy encapsulating an algorithm behind an interface
- context holding a strategy reference not a concrete implementation
- swapping strategy at construction or runtime
- delegate as lightweight strategy for single-method behaviors
- dependency injection naturally delivering strategies

**Answer**

The Strategy pattern defines a family of algorithms, encapsulates each behind an interface, and makes them interchangeable so the context that uses them can vary the algorithm independently. In C#, the pattern maps directly to interfaces: `IDiscountStrategy` with `decimal Calculate(Order order)`, implemented by `PercentageDiscount`, `FixedAmountDiscount`, and `NoDiscount`. The `PricingService` holds an `IDiscountStrategy` injected through its constructor and calls it without knowing the concrete type. For single-method strategies, a `Func<Order, decimal>` delegate is a lightweight alternative that avoids defining a dedicated interface. DI containers naturally deliver strategies: registering `IDiscountStrategy` binds to the environment-appropriate implementation at startup.

---

## Q12. What is the Repository pattern at a high level, and why depend on abstractions?

**Concepts**
- repository mediating between domain and data storage
- interface hiding persistence mechanism from domain logic
- domain logic testable without database
- swappable implementations for different storage backends
- unit of work pairing with repository for transaction scope

**Answer**

The Repository pattern mediates between the domain model and the data storage layer, presenting a collection-like interface for querying and persisting domain objects: `ICustomerRepository` with `GetById(Guid id)`, `Add(Customer customer)`, and `Save()`. Domain logic depends on the interface, not on EF Core, Dapper, or any specific storage technology. This separation means domain logic can be unit-tested with an in-memory fake repository without a database, alternate storage backends (SQL, CosmosDB, file system) can be introduced by implementing the interface, and the domain layer does not need to change when the persistence technology changes. A Unit of Work pairs with repositories to coordinate multiple repositories in a single transaction boundary.

---

## Q13. How does polymorphism simplify replacing implementations in tests?

**Concepts**
- interface enabling mock or stub substitution
- test double replacing real implementation via constructor injection
- no network, disk, or database required in unit tests
- polymorphic dispatch routing to fake in tests
- behavior verification through mock assertions

**Answer**

When a class depends on an interface rather than a concrete type, any object implementing that interface can be injected — including test doubles. A unit test for `OrderService(IOrderRepository repo)` creates a `FakeOrderRepository : IOrderRepository` that stores orders in a `Dictionary` and injects it: `new OrderService(fakeRepo)`. The `OrderService` code calls `repo.Save(order)` polymorphically, and the fake implementation records the call without touching a database. This makes tests fast, isolated, and deterministic. Mock frameworks like Moq and NSubstitute generate these fakes automatically from interfaces. Without the interface — if `OrderService` directly used `SqlOrderRepository` — there is no injection seam and no way to avoid the database dependency in tests.

---

## Q14. What is the difference between domain modeling with rich behavior vs CRUD-style service objects?

**Concepts**
- rich domain object encapsulating state and rules together
- CRUD service operating on dumb data objects
- invariant enforcement location in each approach
- anemic model duplicating rules across service methods
- rich model keeping domain knowledge inside the domain

**Answer**

Rich domain modeling puts both state and the rules governing it inside the domain object: `Order.Confirm()` validates that the order has items, marks it confirmed, and raises an event — the `Order` class owns its lifecycle. CRUD-style service objects treat domain objects as passive data and perform all operations externally: `OrderService.ConfirmOrder(int orderId)` loads the order DTO, checks items, sets the status, saves it back. With CRUD, the rules scatter across service methods and are easy to bypass by calling the repository directly. With rich models, bypassing rules requires actively circumventing the domain object's methods. Rich modeling is preferable for complex domains with meaningful state transitions; CRUD is pragmatic for simple data entry applications where the domain is a thin wrapper over storage.

---

## Q15. Virtual method from base constructor — what is the risk?

**Concepts**
- derived override executing before derived field initializers run
- derived fields seeing default zero values during the virtual call
- base constructor unaware of derived initialization order
- NullReferenceException or logical errors from uninitialized state
- avoiding virtual calls in constructors as the safe rule

**Answer**

When a base class constructor calls a virtual method, the CLR dispatches to the most-derived override even though the derived object is still being initialized. Since derived instance field initializers and the derived constructor body have not yet run, the override sees all derived fields at their default zero values — `null` for reference types, `0` for numerics. This produces `NullReferenceException` or silent incorrect behavior when the override reads derived state it assumes has been initialized. The safe rule is to avoid calling virtual methods from constructors entirely; if base initialization must invoke logic that varies per subclass, use a separate virtual `Initialize()` method that callers invoke after construction, or pass the varying logic as a constructor parameter.

---

## Q16. Method hiding vs overriding — how does `new` vs `override` affect dispatch?

**Concepts**
- override replacing virtual slot for runtime-type-based dispatch
- new creating a separate method slot resolved by compile-time type
- base-typed reference reaching base method when new is used
- derived-typed reference reaching hiding method
- polymorphic designs requiring override not new

**Answer**

`override` replaces the virtual slot in the derived type's method table, so any call through any reference type — base or derived — dispatches to the derived implementation at runtime. `new` creates a separate method declaration that hides the base method for callers holding a derived-typed reference but leaves the virtual slot untouched, so callers holding a base-typed reference still reach the base method. The practical consequence: `Base b = new Derived(); b.M()` calls `Base.M` if `M` uses `new`, and calls `Derived.M` if `M` uses `override`. Mixing the two in a hierarchy breaks the expectation that polymorphic callers receive derived behavior, which is why `new` should not be used as a substitute for `override` in any design that relies on substitutability.

---

## Q17. `Equals()` without `GetHashCode()` — what breaks?

**Concepts**
- hash contract requiring equal objects to have equal hash codes
- Dictionary and HashSet using hash code for bucket placement
- object placed in bucket by original hash code
- mutation changing hash code breaking dictionary lookup
- required co-override of Equals and GetHashCode

**Answer**

The hash contract requires that if two objects are equal according to `Equals`, they must return the same value from `GetHashCode`. If you override `Equals` without also overriding `GetHashCode`, two equal objects may hash to different buckets in a `Dictionary` or `HashSet`, so the collection cannot find the key even though `Equals` would return true. Additionally, if you place a mutable object as a dictionary key and then mutate it in a way that changes what `GetHashCode` returns, the object is now in the wrong bucket and lookups fail silently — the entry appears lost even though it is still in the collection. Always override `Equals` and `GetHashCode` together so they remain consistent.

---

## Q18. Mutable object as dictionary key — what is the runtime risk?

**Concepts**
- dictionary placing key in bucket based on hash at insertion
- mutation changing hash code moving key to wrong logical bucket
- lookup after mutation failing to find the entry
- entry effectively lost without removal
- immutable types or stable hash codes as safe key designs

**Answer**

A `Dictionary<TKey, TValue>` hashes the key at insertion and places the entry in the bucket corresponding to that hash. If the key object is mutable and you change a field that affects `GetHashCode` after insertion, the stored hash no longer matches the bucket where the entry lives. A subsequent lookup hashes the mutated key, searches the wrong bucket, and finds nothing — the entry appears to have vanished even though it is still present in the dictionary. The entry cannot be retrieved or removed via normal means. The safe design is to use immutable types as dictionary keys — or at minimum types whose `GetHashCode` is based only on immutable identity fields that never change after construction.

---

## Q19. Struct boxing via interface — what happens to subsequent mutations?

**Concepts**
- boxing copying struct to a new heap allocation
- interface variable holding a reference to the boxed copy
- mutations through the interface variable affecting only the boxed copy
- original struct variable remaining unchanged
- interfaces not enabling in-place struct mutation

**Answer**

When a struct is assigned to an interface variable, the struct is boxed: a new heap allocation is created containing a copy of the struct's fields, and the interface variable holds a reference to that boxed copy. Subsequent mutations through the interface variable modify the boxed copy on the heap, not the original struct variable. The original struct remains unchanged because it and the boxed copy are independent values after the boxing operation. This surprises developers who expect interface-based operations to modify the original struct in place. If you need a struct's interface implementations to mutate state that the original variable sees, you cannot achieve that through an interface reference — you must hold the struct directly or use `ref` parameters.

---

## Q20. `protected internal` vs `private protected` — what is the access difference?

**Concepts**
- protected internal as union: protected OR same-assembly
- private protected as intersection: protected AND same-assembly
- external derived classes accessing protected internal
- external derived classes blocked from private protected
- narrowest appropriate modifier as the guiding principle

**Answer**

`protected internal` is a union modifier: a member is accessible to any derived class anywhere, and also to any code in the same assembly regardless of inheritance. An external assembly's subclass can access `protected internal` members because the `protected` part of the union applies. `private protected` is an intersection: a member is accessible only to code that is both a derived class and located in the same assembly. An external assembly's subclass cannot access `private protected` members because the assembly requirement is not met. Use `private protected` when a member is an extension point intended only for subclasses built as part of the same library; use `protected internal` only when external subclasses legitimately need the member as part of the public extension API.

---

## Q21. Type-checking anti-pattern — why are long `is` chains problematic?

**Concepts**
- is chains reimplementing virtual dispatch manually
- new type requiring editing all switch/if locations
- violating Open/Closed by requiring modification
- virtual method routing automatically to new type
- switch expression as a more maintainable alternative for closed sets

**Answer**

Long `if (animal is Dog) ... else if (animal is Cat) ... else if (animal is Bird)` chains defeat polymorphism because they re-implement the dispatch logic that a virtual `Speak()` method would handle automatically. Every time a new animal type is added, every such chain throughout the codebase must be found and updated, which violates the Open/Closed Principle and creates maintenance risk when any chain is missed. A virtual method routes to the new type's override automatically with no existing code changed. When a closed type set genuinely warrants type-based dispatch — for example, an AST node visitor where not all types have a shared virtual method — prefer a `switch` expression with exhaustive type patterns so the compiler warns when a new case is not handled.

---

## Q22. Memory leaks despite GC — what causes them in managed code?

**Concepts**
- GC collecting unreachable objects only
- long-lived roots retaining references to short-lived objects
- event handlers keeping subscribers alive through delegate references
- static caches accumulating entries indefinitely
- IDisposable unsubscription and cache eviction as fixes

**Answer**

The GC collects objects that are unreachable — no live root holds a reference chain to them. A managed memory leak occurs when a long-lived root unintentionally holds a reference to a short-lived object, keeping it reachable and uncollectable. The two classic sources are event handlers and static caches. A static singleton publisher that accumulates event subscriptions over time holds references to every subscriber ever attached, preventing their collection even after they are logically done. A `static Dictionary` used as a cache that grows without eviction holds all cached objects for the process lifetime. The fixes are unsubscribing from events in `Dispose`, using `WeakReference` for caches that should not prevent collection, and adding cache eviction or expiration policies.

---

## Q23. Exposing `List<T>` directly — why is this problematic?

**Concepts**
- direct List<T> reference enabling external Add and Remove
- encapsulation bypass without type's knowledge
- callers violating sorting, deduplication, or ordering invariants
- IReadOnlyList<T> exposing read-only view
- defensive copy or AsReadOnly as alternatives

**Answer**

Returning the internal `List<T>` through a public property gives callers a live reference to the type's private state. They can call `items.Add(...)`, `items.Remove(...)`, `items.Clear()`, or `items.Sort(...)` without the owning type observing or validating those changes, bypassing any invariants the type maintains — such as keeping the list sorted, preventing duplicates, or maintaining a maximum count. The owning type loses control of its own state. Exposing `IReadOnlyList<T>` prevents callers from calling mutating methods; returning `_items.ToList()` gives a detached copy that callers can modify freely without affecting internal state. The preferred pattern is to provide specific mutating methods — `AddItem(T item)` — that enforce invariants.

---

## Q24. `init` after construction — what is allowed and what is not?

**Concepts**
- init assignment allowed in constructor and object initializer
- init assignment blocked after the construction phase ends
- difference from private set which allows post-construction mutation
- compile error CS8852 on assigning init property outside construction
- confusing init and private set as a common mistake

**Answer**

Init-only properties (`{ get; init; }`) accept assignment during the construction phase only: inside the type's constructors and inside object initializer blocks at the call site. Once the object exits its construction context — that is, the `new` expression and any object initializer have completed — the property is read-only and any attempt to assign it produces compile error CS8852. `{ get; private set; }` differs in that any instance method of the declaring class can assign the property at any time after construction. The confusion arises because both look similar at the declaration site but have very different mutation semantics: `init` is for immutable-after-construction values; `private set` is for values the class needs to mutate over the object's lifetime.

---

## Q25. Static "singleton" vs DI singleton — what is the testability difference?

**Concepts**
- static singleton accessed via type name as global state
- DI singleton registered as single instance in the container
- DI singleton implementing an interface and injectable
- static singleton not replaceable in tests
- DI singleton swappable with mock or alternative in tests

**Answer**

A static singleton — `static Instance` property on a non-injectable class — is global state accessed by type name from anywhere in the code. There is no injection seam: tests cannot substitute a fake without modifying the class, and static state persists across tests causing pollution. A DI singleton is a single instance registered in the container as an interface: `services.AddSingleton<IMyService, MyService>()`. Callers receive it through constructor injection, making the dependency explicit. Tests can configure `services.AddSingleton<IMyService>(mockService)` to replace the implementation entirely. Both result in one instance per process, but the DI singleton preserves the injection seam that makes substitution possible without global state.

---

## Q26. Explicit interface hiding — how can two `GetName()` methods coexist?

**Concepts**
- public class method GetName() for display purposes
- explicit interface method INamedDocument.GetName() for system purposes
- caller holding class reference reaching public method
- caller holding interface reference reaching explicit method
- different implementations of the same name for different purposes

**Answer**

A class can simultaneously expose a public `GetName()` method and an explicit `INamedDocument.GetName()` implementation, and they can return different values. The public method is accessible through a class-typed reference: `invoice.GetName()` returns the display name `"Invoice: Q1-2025"`. The explicit implementation is accessible only through the interface: `((INamedDocument)invoice).GetName()` returns the file-safe slug `"invoice-q1-2025"`. This pattern is appropriate when the interface contract demands a value with different semantics than what the class naturally provides publicly — for example, a file-safe name vs a display label. Callers must be aware of which API they are using, since the two versions can diverge.

---

## Q27. Finalizer timing — why can't you rely on `~ClassName()` for timely cleanup?

**Concepts**
- finalizer running on GC thread non-deterministically
- no guaranteed order or timing relative to application events
- expensive resources held until next GC collection
- using and IDisposable for deterministic release
- GC.SuppressFinalize reducing overhead after Dispose

**Answer**

A finalizer runs when the GC collects the object, which happens at an indeterminate future point based on memory pressure rather than when the object logically goes out of scope. File handles, database connections, and network sockets held by a finalizer-only class may remain open for an arbitrary duration after the object is no longer in use, exhausting the operating system's resource limit. Two finalizers on different objects have no guaranteed execution order. The correct approach is `IDisposable` and `using` statements: `Dispose` is called deterministically when the `using` block exits. The finalizer should only exist as a safety net for callers who forget to call `Dispose`, and `GC.SuppressFinalize(this)` in `Dispose` skips the finalizer when cleanup already ran.

---

## Q28. Overriding `==` without consistent `Equals`/`GetHashCode` — what breaks?

**Concepts**
- == operator and Equals() potentially diverging
- Dictionary and HashSet using Equals and GetHashCode
- LINQ Distinct and GroupBy using Equals and GetHashCode
- inconsistent equality producing wrong results in collections
- required triple override: Equals, GetHashCode, and ==

**Answer**

If you override `==` to express value equality but do not also override `Equals` and `GetHashCode`, the three equality mechanisms disagree: `a == b` returns `true` but `a.Equals(b)` returns `false`, and the two objects may have different hash codes. LINQ operators like `Distinct`, `GroupBy`, and `Contains` use `Equals` and `GetHashCode`, not `==`, so they treat the objects as different even when `==` says they are equal. `Dictionary` and `HashSet` key lookup uses the same path, causing lookups to fail for objects that compare equal via `==`. The rule is to always override all three together and ensure they agree: equal objects must produce equal hash codes, and `==` must be consistent with `Equals`.

---

## Q29. Default interface methods on structs — when does boxing occur?

**Concepts**
- default interface method called on struct via interface reference
- boxing occurring when struct assigned to interface variable
- no boxing when struct overrides the default method
- constrained call instruction avoiding box in generic context
- performance sensitivity requiring awareness of interface call paths

**Answer**

When a struct implements an interface that has a default method, calling that default method depends on how the call is made. If the struct overrides the default method with its own implementation, calling it through an interface variable still boxes the struct (the interface reference is a heap pointer), but calling it through a generic constraint like `where T : IMyInterface` uses a constrained `callvirt` IL instruction that avoids boxing by calling the struct's override directly. If the struct does not override the default method and relies on the interface's default body, the call through an interface variable boxes the struct since the default method implementation belongs to the interface type, not the struct. For performance-critical code with high-frequency struct-through-interface calls, providing explicit struct overrides of all needed interface members avoids boxing on the constrained call path.
