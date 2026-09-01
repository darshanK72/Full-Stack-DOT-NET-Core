# 02. Object Oriented Programming — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. Classes & Objects](#01-classes-objects)
  - [Q1. What is a class and what is an object in C#?](#01-classes-objects-q1)
  - [Q2. What is the difference between `struct` and `class` in C#?](#01-classes-objects-q2)
  - [Q3. What are the different principles of OOP supported in C#?](#01-classes-objects-q3)
  - [Q4. What is a partial class in C#?](#01-classes-objects-q4)
  - [Q5. Explain object initializers and collection initializers in C…](#01-classes-objects-q5)
  - [Q6. What is the difference between shallow copy and deep copy in…](#01-classes-objects-q6)
  - [Q7. What is the difference between object identity and object eq…](#01-classes-objects-q7)
  - [Q8. What is the difference between `IDisposable` and a finalizer…](#01-classes-objects-q8)
  - [Q9. What happens at runtime when you execute `new MyClass()` — a…](#01-classes-objects-q9)
  - [Q10. Where are class instances stored vs where are struct instanc…](#01-classes-objects-q10)
  - [Q11. What is the difference between a field, a property, and a me…](#01-classes-objects-q11)
  - [Q12. What is a static class vs an instance class — can you instan…](#01-classes-objects-q12)
  - [Q13. What is the `null` reference for reference types, and what i…](#01-classes-objects-q13)
  - [Q14. What is object initializer syntax, and how does it interact …](#01-classes-objects-q14)
  - [Q15. What is the difference between `ReferenceEquals`, `==`, and …](#01-classes-objects-q15)
  - [Q16. When is a struct copied vs when is a reference copied when p…](#01-classes-objects-q16)
  - [Q17. What is the fragile base class problem at a high level?](#01-classes-objects-q17)
  - [Q18. What is the difference between stack allocation (`stackalloc…](#01-classes-objects-q18)
  - [Q19. What does `GC.GetTotalMemory` measure, and why is it only a …](#01-classes-objects-q19)
  - [Q20. What is the difference between an anemic class (data-only) a…](#01-classes-objects-q20)

- [02. Properties & Indexers](#02-properties-indexers)
  - [Q1. Explain properties and fields in C#.](#02-properties-indexers-q1)
  - [Q2. What are auto-implemented properties?](#02-properties-indexers-q2)
  - [Q3. What are indexers in C#?](#02-properties-indexers-q3)
  - [Q4. What is the difference between a `public` field and a `publi…](#02-properties-indexers-q4)
  - [Q5. What are init-only properties (`get; init;`), and how do the…](#02-properties-indexers-q5)
  - [Q6. What is the difference between `{ get; private set; }` and a…](#02-properties-indexers-q6)
  - [Q7. What are expression-bodied properties (`public string Label …](#02-properties-indexers-q7)
  - [Q8. Can indexers be overloaded — what distinguishes overloads?](#02-properties-indexers-q8)
  - [Q9. What is the syntax for an indexer (`this[int index]`, `this[…](#02-properties-indexers-q9)
  - [Q10. When should you use a full property with validation vs an au…](#02-properties-indexers-q10)
  - [Q11. What is a computed/read-only property that derives its value…](#02-properties-indexers-q11)
  - [Q12. What is the difference between `init` properties and constru…](#02-properties-indexers-q12)
  - [Q13. How do properties participate in object initializer syntax?](#02-properties-indexers-q13)
  - [Q14. What is a preview-level understanding of `record` types and …](#02-properties-indexers-q14)
  - [Q15. Why might exposing a public `{ get; set; }` on a collection-…](#02-properties-indexers-q15)
  - [Q16. What is the difference between an indexer and a method named…](#02-properties-indexers-q16)
  - [Q17. Can interface types declare indexers, and how are they imple…](#02-properties-indexers-q17)
  - [Q18. What is the relationship between properties and data binding…](#02-properties-indexers-q18)

- [03. Constructors & Method Overloading](#03-constructors-method-overloading)
  - [Q1. Explain constructors and their types in C# (default, paramet…](#03-constructors-method-overloading-q1)
  - [Q2. What is a destructor/finalizer in C#?](#03-constructors-method-overloading-q2)
  - [Q3. Explain constructor chaining in C# (`: this(...)` vs `: base…](#03-constructors-method-overloading-q3)
  - [Q4. How can you call the base class constructor from a derived c…](#03-constructors-method-overloading-q4)
  - [Q5. In what order do constructors and field initializers run in …](#03-constructors-method-overloading-q5)
  - [Q6. Explain method overloading and method overriding in C#.](#03-constructors-method-overloading-q6)
  - [Q7. What is a static constructor, and when does it run?](#03-constructors-method-overloading-q7)
  - [Q8. Can a struct have a parameterless constructor (C# 10+ rules …](#03-constructors-method-overloading-q8)
  - [Q9. What is the difference between a primary constructor (C# 12 …](#03-constructors-method-overloading-q9)
  - [Q10. What happens if you do not define any constructor — what def…](#03-constructors-method-overloading-q10)
  - [Q11. Why might you mark a constructor `private` (singleton, facto…](#03-constructors-method-overloading-q11)
  - [Q12. What is constructor overloading, and how does `: this(...)` …](#03-constructors-method-overloading-q12)
  - [Q13. What is the exact order: static constructor, instance field …](#03-constructors-method-overloading-q13)
  - [Q14. What is the difference between method overloading (compile-t…](#03-constructors-method-overloading-q14)
  - [Q15. When does the compiler fail to pick an overload due to ambig…](#03-constructors-method-overloading-q15)
  - [Q16. Can constructors be inherited — how does a derived class get…](#03-constructors-method-overloading-q16)
  - [Q17. What validation belongs in a constructor vs a factory method…](#03-constructors-method-overloading-q17)
  - [Q18. What is the difference between calling an overloaded instanc…](#03-constructors-method-overloading-q18)

- [04. Static Members & Static Classes](#04-static-members-static-classes)
  - [Q1. Explain the `static` keyword in detail.](#04-static-members-static-classes-q1)
  - [Q2. What is a static class in C#?](#04-static-members-static-classes-q2)
  - [Q3. Why can you not override a `static` method?](#04-static-members-static-classes-q3)
  - [Q4. What is the difference between a static class and the single…](#04-static-members-static-classes-q4)
  - [Q5. What is a static field, and how is lifetime different from a…](#04-static-members-static-classes-q5)
  - [Q6. What is a static property and static method — what is the `t…](#04-static-members-static-classes-q6)
  - [Q7. Why can static methods not access instance members directly?](#04-static-members-static-classes-q7)
  - [Q8. When are static constructors executed, and how many times pe…](#04-static-members-static-classes-q8)
  - [Q9. What is the difference between `const` (implicitly static) a…](#04-static-members-static-classes-q9)
  - [Q10. Can a static class implement interfaces?](#04-static-members-static-classes-q10)
  - [Q11. What thread-safety concerns apply to mutable static fields?](#04-static-members-static-classes-q11)
  - [Q12. Why is overusing static state a testing and maintainability …](#04-static-members-static-classes-q12)
  - [Q13. What is the difference between static nested classes and non…](#04-static-members-static-classes-q13)
  - [Q14. How do static members participate in inheritance — are they …](#04-static-members-static-classes-q14)

- [05. Inheritance & Polymorphism](#05-inheritance-polymorphism)
  - [Q1. Explain inheritance in detail in C#.](#05-inheritance-polymorphism-q1)
  - [Q2. Explain polymorphism in C# and how it can be achieved.](#05-inheritance-polymorphism-q2)
  - [Q3. What is the difference between compile-time (static) and run…](#05-inheritance-polymorphism-q3)
  - [Q4. What is a sealed class in C#?](#05-inheritance-polymorphism-q4)
  - [Q5. What is a virtual method in C#?](#05-inheritance-polymorphism-q5)
  - [Q6. What is the difference between `this` and `base` keywords?](#05-inheritance-polymorphism-q6)
  - [Q7. What is operator overloading in C#?](#05-inheritance-polymorphism-q7)
  - [Q8. Explain the difference between `virtual`, `abstract`, and `o…](#05-inheritance-polymorphism-q8)
  - [Q9. Explain the `new` keyword in the context of method hiding.](#05-inheritance-polymorphism-q9)
  - [Q10. Explain how C# handles multiple inheritance (using interface…](#05-inheritance-polymorphism-q10)
  - [Q11. Why does C# not support multiple inheritance of classes?](#05-inheritance-polymorphism-q11)
  - [Q12. What is the fragile base class problem?](#05-inheritance-polymorphism-q12)
  - [Q13. Why is "favor composition over inheritance" a common guideli…](#05-inheritance-polymorphism-q13)
  - [Q14. What is runtime dispatch — how does the CLR resolve `overrid…](#05-inheritance-polymorphism-q14)
  - [Q15. What is the difference between hiding with `new` and overrid…](#05-inheritance-polymorphism-q15)
  - [Q16. Can you inherit from a sealed class?](#05-inheritance-polymorphism-q16)
  - [Q17. What is the difference between `is` type testing and casting…](#05-inheritance-polymorphism-q17)
  - [Q18. What is the Liskov Substitution Principle in one sentence, a…](#05-inheritance-polymorphism-q18)
  - [Q19. When does `base.Method()` call the parent's implementation v…](#05-inheritance-polymorphism-q19)
  - [Q20. What is the difference between extending behavior with inher…](#05-inheritance-polymorphism-q20)

- [06. Abstract Classes & Interfaces](#06-abstract-classes-interfaces)
  - [Q1. Explain abstraction in detail in C#.](#06-abstract-classes-interfaces-q1)
  - [Q2. What is the difference between abstraction and encapsulation…](#06-abstract-classes-interfaces-q2)
  - [Q3. What is the difference between abstraction and polymorphism?](#06-abstract-classes-interfaces-q3)
  - [Q4. What is the difference between an abstract class and an inte…](#06-abstract-classes-interfaces-q4)
  - [Q5. What is the difference between an abstract class and an inte…](#06-abstract-classes-interfaces-q5)
  - [Q6. Why do we need interfaces in C#?](#06-abstract-classes-interfaces-q6)
  - [Q7. What is explicit interface implementation and when is it use…](#06-abstract-classes-interfaces-q7)
  - [Q8. What are static abstract members in interfaces (C# 11)?](#06-abstract-classes-interfaces-q8)
  - [Q9. Can an abstract class have concrete (non-abstract) methods?](#06-abstract-classes-interfaces-q9)
  - [Q10. Can a class implement multiple interfaces — what about an in…](#06-abstract-classes-interfaces-q10)
  - [Q11. When would you choose an abstract base class over an interfa…](#06-abstract-classes-interfaces-q11)
  - [Q12. What is the diamond problem, and how does C# avoid it for cl…](#06-abstract-classes-interfaces-q12)
  - [Q13. What is explicit interface implementation — why might `((IMy…](#06-abstract-classes-interfaces-q13)
  - [Q14. Can interfaces declare fields, constructors, or static concr…](#06-abstract-classes-interfaces-q14)
  - [Q15. What is the difference between `IReadOnlyList<T>` as a param…](#06-abstract-classes-interfaces-q15)
  - [Q16. When should API surface depend on interfaces vs abstract cla…](#06-abstract-classes-interfaces-q16)

- [07. Encapsulation & Access Modifiers](#07-encapsulation-access-modifiers)
  - [Q1. Explain encapsulation in C# with examples.](#07-encapsulation-access-modifiers-q1)
  - [Q2. What are the different access modifiers in C#? (`private`, `…](#07-encapsulation-access-modifiers-q2)
  - [Q3. What is the difference between "information hiding" and "dat…](#07-encapsulation-access-modifiers-q3)
  - [Q4. Why is exposing a mutable collection through a public getter…](#07-encapsulation-access-modifiers-q4)
  - [Q5. What is the difference between `protected internal` and `pri…](#07-encapsulation-access-modifiers-q5)
  - [Q6. What does `internal` mean in the context of assemblies and `…](#07-encapsulation-access-modifiers-q6)
  - [Q7. What is the default access level for class members if you om…](#07-encapsulation-access-modifiers-q7)
  - [Q8. How do access modifiers apply to nested types vs top-level t…](#07-encapsulation-access-modifiers-q8)
  - [Q9. What is defensive copying when returning collections from pr…](#07-encapsulation-access-modifiers-q9)
  - [Q10. What is the difference between encapsulation and immutabilit…](#07-encapsulation-access-modifiers-q10)
  - [Q11. Why are public fields discouraged in public APIs even for si…](#07-encapsulation-access-modifiers-q11)
  - [Q12. How does `private protected` restrict visibility compared to…](#07-encapsulation-access-modifiers-q12)
  - [Q13. What is a friend assembly pattern, and what are its trade-of…](#07-encapsulation-access-modifiers-q13)
  - [Q14. How do property accessors use asymmetric access (`public get…](#07-encapsulation-access-modifiers-q14)

- [08. Events](#08-events)
  - [Q1. Explain events in C# (including event handling and publisher…](#08-events-q1)
  - [Q2. What is the difference between an `event` and a plain public…](#08-events-q2)
  - [Q3. Why should you unsubscribe from events, and what problem doe…](#08-events-q3)
  - [Q4. What happens during multicast delegate invocation if one sub…](#08-events-q4)
  - [Q5. What is the standard `EventHandler` / `EventHandler<TEventAr…](#08-events-q5)
  - [Q6. How do you raise an event safely (null-check, `?.Invoke`, lo…](#08-events-q6)
  - [Q7. What is the difference between custom delegate types and `Ev…](#08-events-q7)
  - [Q8. Can interfaces declare events, and how are they implemented?](#08-events-q8)
  - [Q9. What memory-leak scenario arises when a long-lived publisher…](#08-events-q9)
  - [Q10. What is the difference between events and the Observer patte…](#08-events-q10)
  - [Q11. Can you assign to an event from outside the declaring class …](#08-events-q11)
  - [Q12. What is thread-safe event raising, and when is locking requi…](#08-events-q12)

- [09. OOP Real-World Examples](#09-oop-real-world-examples)
  - [Q1. Explain the SOLID principles with concrete C# examples.](#09-oop-real-world-examples-q1)
  - [Q2. What is the Liskov Substitution Principle? Give a classic vi…](#09-oop-real-world-examples-q2)
  - [Q3. What is Dependency Inversion, and how does constructor injec…](#09-oop-real-world-examples-q3)
  - [Q4. What is the difference between Dependency Injection and the …](#09-oop-real-world-examples-q4)
  - [Q5. What is the difference between "has-a" and "is-a" relationsh…](#09-oop-real-world-examples-q5)
  - [Q6. What is the anemic domain model anti-pattern?](#09-oop-real-world-examples-q6)
  - [Q7. What is the Open/Closed Principle, and how do interfaces sup…](#09-oop-real-world-examples-q7)
  - [Q8. What is the Single Responsibility Principle — how do you rec…](#09-oop-real-world-examples-q8)
  - [Q9. What is the Interface Segregation Principle — why are fat in…](#09-oop-real-world-examples-q9)
  - [Q10. What is a factory method vs a simple constructor — when do y…](#09-oop-real-world-examples-q10)
  - [Q11. What is the Strategy pattern, and how does it map to interfa…](#09-oop-real-world-examples-q11)
  - [Q12. What is the Repository pattern at a high level, and why depe…](#09-oop-real-world-examples-q12)
  - [Q13. How does polymorphism simplify replacing implementations in …](#09-oop-real-world-examples-q13)
  - [Q14. What is the difference between domain modeling with rich beh…](#09-oop-real-world-examples-q14)
  - [Q15. **Virtual method from base constructor** — Calling an overri…](#09-oop-real-world-examples-q15)
  - [Q16. **Method hiding vs overriding** — `new` hides by compile-tim…](#09-oop-real-world-examples-q16)
  - [Q17. **`Equals()` without `GetHashCode()`** — Breaks the hash con…](#09-oop-real-world-examples-q17)
  - [Q18. **Mutable object as dictionary key** — Changing a key after …](#09-oop-real-world-examples-q18)
  - [Q19. **Struct boxing via interface** — Assigning a struct to an i…](#09-oop-real-world-examples-q19)
  - [Q20. **`protected internal` vs `private protected`** — `protected…](#09-oop-real-world-examples-q20)
  - [Q21. **Type-checking anti-pattern** — Long `if (animal is Dog)` c…](#09-oop-real-world-examples-q21)
  - [Q22. **Memory leaks despite GC** — Event handlers and static cach…](#09-oop-real-world-examples-q22)
  - [Q23. **Exposing `List<T>` directly** — Callers can mutate interna…](#09-oop-real-world-examples-q23)
  - [Q24. **`init` after construction** — Init-only properties can be …](#09-oop-real-world-examples-q24)
  - [Q25. **Static "singleton" vs DI singleton** — A static class is h…](#09-oop-real-world-examples-q25)
  - [Q26. **Explicit interface hiding** — Public class method and expl…](#09-oop-real-world-examples-q26)
  - [Q27. **Finalizer timing** — `~ClassName()` runs non-deterministic…](#09-oop-real-world-examples-q27)
  - [Q28. **Overriding `==` without consistent `Equals`/`GetHashCode`*…](#09-oop-real-world-examples-q28)
  - [Q29. **Default interface methods on structs** — Calling a default…](#09-oop-real-world-examples-q29)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

### 01. Classes & Objects

#### Q1. What is a class and what is an object in C#? {#01-classes-objects-q1}

What is a class and what is an object in C#?

**Answer:** A class is a type definition—a blueprint describing fields, properties, and methods that instances will have. An object is a concrete instance of that class created at runtime with `new`, holding its own copy of instance state while sharing method implementations from the type.

- The class exists once in metadata; many objects can be instantiated from it during execution.
- Reference-type objects live on the heap; the variable stores a reference to the object.
- Static members belong to the type; instance members belong to each object—see Q12.
- Tutorial terminology: `Student` is the class; `new Student(...)` produces an object.

---

#### Q2. What is the difference between `struct` and `class` in C#? {#01-classes-objects-q2}

What is the difference between `struct` and `class` in C#?

**Answer:** `struct` is a value type copied on assignment and defaulting to zeroed fields without null (unless nullable), while `class` is a reference type identified by reference, defaulting to null, supporting inheritance and full polymorphism.

| | `struct` | `class` |
|---|---|---|
| Kind | Value type | Reference type |
| Inheritance | Interfaces only | Single base class + interfaces |
| Default | Zero bits | `null` |
| Identity | Copied; no reference identity | Reference equality by default |

Choose structs for small immutable data; classes for identity, shared mutable state, and inheritance hierarchies.

---

#### Q3. What are the different principles of OOP supported in C#? {#01-classes-objects-q3}

What are the different principles of OOP supported in C#?

**Answer:** C# supports encapsulation (hide state, expose controlled APIs), abstraction (essential model without implementation detail), inheritance (reuse and extend types), and polymorphism (one interface, many behaviors via virtual methods and interfaces).

- Encapsulation uses access modifiers and properties—Module 02 chapter 07.
- Abstraction uses abstract classes and interfaces—chapter 06.
- Inheritance and polymorphism—chapter 05.
- C# also emphasizes composition patterns alongside classical OOP in modern API design.

---

#### Q4. What is a partial class in C#? {#01-classes-objects-q4}

What is a partial class in C#?

**Answer:** A `partial class` splits one class definition across multiple source files, merged by the compiler into a single type. It supports designer-generated code separation (WinForms, EF) and large team workflows without one giant file.

- All parts must use the `partial` modifier and the same namespace and class name.
- Partial methods (with restrictions) allow one part to declare and another to implement.
- Cannot split across assemblies—partial is a compile-time source organization feature only.
- One partial file can hold generated code users should not edit manually.

---

#### Q5. Explain object initializers and collection initializers in C#. {#01-classes-objects-q5}

Explain object initializers and collection initializers in C#.

**Answer:** Object initializers set public fields or properties immediately after construction with `{ Property = value }` syntax without requiring a dedicated constructor overload. Collection initializers add elements to collections implementing `Add` with `{ item1, item2 }` or `{ ["key"] = value }` for indexers.

- Object initializers call constructor first, then assign listed members in source order.
- Collection initializers desugar to repeated `Add` calls on the new collection instance.
- Init-only properties (`init`) work in object initializers until construction completes—chapter 02.
- Initializers improve readability for DTO construction and test data setup.

---

#### Q6. What is the difference between shallow copy and deep copy in C#? {#01-classes-objects-q6}

What is the difference between shallow copy and deep copy in C#?

**Answer:** Shallow copy duplicates the top-level object and copies field values as-is—for reference fields, both copies point to the same nested objects. Deep copy recursively clones nested objects so the clone graph is independent, requiring custom logic or serialization.

- `MemberwiseClone` on classes is protected shallow copy; structs copy by value shallowly for contained references.
- Arrays clone shallowly for elements that are reference types.
- Immutable nested objects make shallow copy safe when inner state cannot change.
- See Arrays Q11 for array copy semantics.

---

#### Q7. What is the difference between object identity and object equality? {#01-classes-objects-q7}

What is the difference between object identity and object equality?

**Answer:** Identity means two references denote the same heap object (`ReferenceEquals` true). Equality means two objects compare as equivalent by value or custom logic (`Equals`, overloaded `==`) even when they are distinct instances.

- Default class equality is reference identity unless overridden.
- Value types compare by value bitwise/default equality unless overridden.
- Equal but non-identical strings illustrate content vs reference—Module 01 Strings Q16.
- Consistent `Equals`, `GetHashCode`, and `==` matter for collections—Gotcha 3 Module 02.

---

#### Q8. What is the difference between `IDisposable` and a finalizer (`~ClassName()`)? {#01-classes-objects-q8}

What is the difference between `IDisposable` and a finalizer (`~ClassName()`)?

**Answer:** `IDisposable.Dispose` releases resources deterministically when callers use `using` or explicit dispose. A finalizer runs later during garbage collection as a safety net for missed dispose calls, not for timely cleanup of scarce resources like file handles.

- Implement dispose pattern: public `Dispose()` calling protected virtual `Dispose(bool disposing)`.
- Suppress finalizer after successful dispose with `GC.SuppressFinalize`.
- Finalizers add GC overhead and non-deterministic timing—Gotcha 13 Module 02.
- Unmanaged resources belong in dispose; managed references usually need only nulling in dispose when holding events or caches.

---

#### Q9. What happens at runtime when you execute `new MyClass()` — allocation, constructor, and reference assignment? {#01-classes-objects-q9}

What happens at runtime when you execute `new MyClass()` — allocation, constructor, and reference assignment?

**Answer:** The runtime allocates memory for the object (heap for classes), initializes fields to defaults or field initializers, runs instance constructors (base then derived chain), and returns a reference assigned to the variable.

1. **Allocate** — CLR allocates object header, method table pointer, and field storage aligned for the type.
2. **Initialize fields** — Field initializers and default values run before constructor body in defined order (chapter 03).
3. **Construct** — Constructor chain executes `: base(...)` then `: this(...)` rules per inheritance.
4. **Assign** — Reference is stored in the target variable or returned to caller.

No object exists for instance methods until `new` completes successfully.

---

#### Q10. Where are class instances stored vs where are struct instances typically stored when local variables? {#01-classes-objects-q10}

Where are class instances stored vs where are struct instances typically stored when local variables?

**Answer:** Class instances always live on the managed heap; local variables hold references. Struct locals typically reside on the stack or in registers, but structs embedded in heap objects or boxed to `object` live on the heap as part of those containers.

- Escape analysis may allocate struct locals on heap when referenced from closures surviving the method.
- Large struct locals still copy by value on assignment—performance consideration for `in` parameters.
- See Module 01 Q13 for value vs reference storage teaching model.
- `stackalloc` and `Span` scenarios use stack memory for buffers with safety rules.

---

#### Q11. What is the difference between a field, a property, and a method on a class? {#01-classes-objects-q11}

What is the difference between a field, a property, and a method on a class?

**Answer:** Fields are data storage locations; properties are accessors (often with get/set) presenting controlled access to state; methods are operations that perform behavior, optionally mutating state or computing results without necessarily exposing storage.

- Public fields expose implementation directly—discouraged in public APIs (chapter 07).
- Properties can validate, compute, or defer loading while keeping field-like syntax at call sites.
- Methods express actions (`CalculateTotal`, `Save`) with arbitrary parameters and return types.
- Auto-properties blur field/property line syntactically but still generate hidden backing fields.

---

#### Q12. What is a static class vs an instance class — can you instantiate a static class? {#01-classes-objects-q12}

What is a static class vs an instance class — can you instantiate a static class?

**Answer:** A static class is sealed, cannot be instantiated, and contains only static members—it acts as a container for shared utilities. Instance classes create objects with `new` and may mix instance and static members.

- Attempting `new` on a static class is a compile error.
- Static classes cannot implement interfaces (C# rules)—chapter 04.
- Instance classes can have static helpers (`InstanceCount`) alongside instance state.
- Prefer instance services with dependency injection over static classes for testability—Gotcha 11 Module 02.

---

#### Q13. What is the `null` reference for reference types, and what is `default` for a struct vs a class? {#01-classes-objects-q13}

What is the `null` reference for reference types, and what is `default` for a struct vs a class?

**Answer:** Reference type variables default to `null`, meaning no object is referenced. Struct `default` is all-zero value with no null unless `Nullable<T>`. Class `default` in generics is null.

- Dereferencing null throws `NullReferenceException`.
- Nullable reference type annotations warn when null assigned to non-nullable references under `#nullable enable`.
- `default(Customer)` for a class is null; `default(Point)` for struct is (0,0) coordinates.
- Always initialize reference fields in constructors for non-nullable intent.

---

#### Q14. What is object initializer syntax, and how does it interact with constructors? {#01-classes-objects-q14}

What is object initializer syntax, and how does it interact with constructors?

**Answer:** Object initializer syntax runs immediately after the selected constructor completes, assigning listed properties or fields in textual order. Constructor establishes invariants; initializer sets additional optional surface properties.

- You must invoke an accessible constructor—either parameterless or matched overload before `{ ... }`.
- Init-only properties accept assignments only during this construction phase in modern C#.
- Constructor cannot see initializer assignments; initializer runs after constructor body returns to caller chain.
- See Q5 and Properties chapter Q13.

---

#### Q15. What is the difference between `ReferenceEquals`, `==`, and `Equals` for classes that do not override equality? {#01-classes-objects-q15}

What is the difference between `ReferenceEquals`, `==`, and `Equals` for classes that do not override equality?

**Answer:** Without overrides, `ReferenceEquals` and `==` (unless overloaded) compare reference identity, and `Equals` on `Object` also uses reference equality by default. Overloads may diverge if `==` is customized without matching `Equals`.

- Structs use value equality defaults; classes use reference identity defaults.
- Gotcha 14 Module 02 warns when `==` is overridden without consistent `GetHashCode`.
- For domain equality, override `Equals`, `GetHashCode`, and optionally `==` together.
- See Module 01 Operators Q3 and Q14.

---

#### Q16. When is a struct copied vs when is a reference copied when passed to a method? {#01-classes-objects-q16}

When is a struct copied vs when is a reference copied when passed to a method?

**Answer:** Struct parameters copy the entire struct value into the parameter slot unless modified by `ref`, `out`, or `in`. Reference type parameters copy the reference value, aliasing the same object without copying the object itself.

- Mutating struct parameter fields mutates the copy only unless `ref`.
- Mutating object fields through reference parameter affects caller's object.
- Large structs use `in` for efficient readonly passing—Methods Q12 Module 01.
- Boxing copies struct to heap when passed as `object` or interface—Gotcha 5 Module 02.

---

#### Q17. What is the fragile base class problem at a high level? {#01-classes-objects-q17}

What is the fragile base class problem at a high level?

**Answer:** The fragile base class problem occurs when a base class change (new virtual method, altered constructor sequence) breaks derived classes that relied on previous behavior, because subclasses are tightly coupled to base implementation details they do not control.

- Adding virtual calls in base constructor to overridable methods is especially dangerous—Gotcha 1 Module 02.
- Favor composition, sealed defaults, or careful virtual design to minimize surprise in derivatives.
- See Inheritance chapter Q12 for expanded discussion.
- Versioning public base classes in libraries requires extreme caution.

---

#### Q18. What is the difference between stack allocation (`stackalloc`, local structs) and heap allocation for objects? {#01-classes-objects-q18}

What is the difference between stack allocation (`stackalloc`, local structs) and heap allocation for objects?

**Answer:** `stackalloc` and local struct variables use stack or register storage scoped to the method invocation (with escape restrictions), while `new` on classes allocates on the heap with lifetime managed by garbage collection until unreachable.

- Stack memory is reclaimed when the method returns automatically—no GC.
- Heap objects survive until no references remain; finalizers run non-deterministically if present.
- `stackalloc` into `Span<T>` is idiomatic for temporary buffers in modern C#.
- Do not return references to stack memory that outlives the method—language rules prevent most cases.

---

#### Q19. What does `GC.GetTotalMemory` measure, and why is it only a rough indicator? {#01-classes-objects-q19}

What does `GC.GetTotalMemory` measure, and why is it only a rough indicator?

**Answer:** `GC.GetTotalMemory` returns an approximate number of bytes the garbage collector believes are allocated in managed heaps after optionally forcing a collection, useful for coarse diagnostics—not precise accounting of process working set or native memory.

- Passing `true` triggers collection before measure, skewing results toward post-GC state.
- Does not include unmanaged allocations, stack, or JIT code size.
- Production monitoring uses profilers and `dotnet-counters`, not ad hoc `GetTotalMemory` alone.
- Teaches that GC heap size differs from task manager process memory.

---

#### Q20. What is the difference between an anemic class (data-only) and a rich domain object? {#01-classes-objects-q20}

What is the difference between an anemic class (data-only) and a rich domain object?

**Answer:** An anemic class exposes data through getters and setters while behavior lives in external services, whereas a rich domain object encapsulates business rules and invariants alongside its data, enforcing valid states through methods and properties.

- Anemic models simplify CRUD and mapping layers but scatter domain logic across procedural code.
- Rich models align with encapsulation and reduce invalid state combinations if designed well.
- Neither is always wrong—reporting DTOs are intentionally anemic; core domain may be rich.
- See OOP Real-World Examples chapter Q6 on anemic domain anti-pattern.

---

### 02. Properties & Indexers

#### Q1. Explain properties and fields in C#. {#02-properties-indexers-q1}

Explain properties and fields in C#.

**Answer:** Fields are variables declared directly on a type; properties are members with accessors that read or write backing state through methods disguised as field-like syntax. Properties enable validation, computed values, and versioning without changing public call sites.

- Auto-properties compile to hidden backing fields with trivial get/set.
- Fields cannot intercept assignment; properties can enforce invariants on set.
- Interface contracts use properties, not public fields, for consistency.
- See Classes Q11 for roles relative to methods.

---

#### Q2. What are auto-implemented properties? {#02-properties-indexers-q2}

What are auto-implemented properties?

**Answer:** Auto-implemented properties declare `{ get; set; }` without manual backing field code; the compiler generates a private hidden field and accessor methods automatically.

- Useful for DTOs and simple state when no validation is needed yet.
- Can use `{ get; private set; }` for restricted mutation from outside the type.
- Init-only `{ get; init; }` restricts assignment to construction phase—Q5.
- Upgrade to full property with backing field when validation becomes necessary—Q10.

---

#### Q3. What are indexers in C#? {#02-properties-indexers-q3}

What are indexers in C#?

**Answer:** Indexers are properties that accept parameters in square brackets (`this[int index]`, `this[string key]`) allowing instance syntax like `collection[i]` on custom types, implemented as get/set methods with parameters.

- Syntax mirrors arrays but defined on classes or structs implementing dictionaries, buffers, or matrices.
- Can overload on parameter types—Q8.
- Interfaces may declare indexers implemented explicitly or publicly—Q17.
- Distinct from methods named `GetByIndex` primarily by call-site syntax—Q16.

---

#### Q4. What is the difference between a `public` field and a `public` auto-property — if they behave similarly, why prefer properties? {#02-properties-indexers-q4}

What is the difference between a `public` field and a `public` auto-property — if they behave similarly, why prefer properties?

**Answer:** At runtime both expose get/set-like access, but properties are methods in IL metadata, allowing future validation, computed backing, versioning, and data-binding conventions without breaking binary compatibility as easily as changing public fields.

- Reflection and serializers often treat properties as the public surface for serialization.
- Fields cannot be virtual; properties can be overridden with custom logic in derived classes.
- Public fields cannot intercept assignment for invariant checks without refactoring all call sites to methods.
- Encapsulation chapter expands API design rationale—chapter 07 Q11.

---

#### Q5. What are init-only properties (`get; init;`), and how do they differ from get-only and `{ get; set; }`? {#02-properties-indexers-q5}

What are init-only properties (`get; init;`), and how do they differ from get-only and `{ get; set; }`?

**Answer:** Init-only properties allow assignment only during object construction—constructor body or object initializer—then become read-only afterward. Get-only properties without init may be set only in constructor or expression-bodied; `{ get; set; }` allows mutation any time.

- Init supports immutable object models with object initializer ergonomics.
- `{ get; private set; }` allows class methods to mutate after construction; init does not.
- Records use init properties heavily for positional semantics—Q14 preview.
- Gotcha 10 Module 02 contrasts init vs private set confusion.

---

#### Q6. What is the difference between `{ get; private set; }` and a property with only a public getter backed by a private setter method? {#02-properties-indexers-q6}

What is the difference between `{ get; private set; }` and a property with only a public getter backed by a private setter method?

**Answer:** `{ get; private set; }` exposes a property whose setter is callable from any member of the declaring type. A public getter with private `SetName()` method restricts mutation to explicit methods, documenting which operations change state.

- Auto-property private set is concise for simple internal mutation from any instance method.
- Dedicated setter methods name the intent (`Promote()`, `Deactivate()`) and can carry parameters beyond single value assignment.
- Both hide public mutation; choose based on clarity of domain operations vs generic property set.
- Init-only properties differ from both—see Q5.

---

#### Q7. What are expression-bodied properties (`public string Label => $"{Title}";`)? {#02-properties-indexers-q7}

What are expression-bodied properties (`public string Label => $"{Title}";`)?

**Answer:** Expression-bodied properties use `=>` to define read-only properties computing a single expression without a braced get accessor block, reducing noise for derived values like formatted labels or boolean flags from other members.

- Must be read-only unless using `{ get => field; set => field = value; }` form for accessors in newer C#.
- Evaluated on each get access unless caching added in backing logic elsewhere.
- Keep expressions simple; complex logic belongs in methods or full property bodies.
- See Methods Q3 Module 01 for expression-bodied members generally.

---

#### Q8. Can indexers be overloaded — what distinguishes overloads? {#02-properties-indexers-q8}

Can indexers be overloaded — what distinguishes overloads?

**Answer:** Indexers overload by parameter signature—different parameter types, counts, or modifier combinations (`int` vs `string` key)—while sharing the `this[...]` name. Return types alone do not distinguish indexer overloads.

- Multi-dimensional indexers use multiple parameters: `this[int row, int col]`.
- Explicit interface indexers can implement interface indexer separately from public class indexer.
- Overloads must differ in parameter lists like methods.
- Compiler selects overload based on argument types at call site.

---

#### Q9. What is the syntax for an indexer (`this[int index]`, `this[string key]`)? {#02-properties-indexers-q9}

What is the syntax for an indexer (`this[int index]`, `this[string key]`)?

**Answer:** Indexers declare `public Type this[ParameterList] { get; set; }` where `this` keyword marks the indexer, parameters appear in brackets, and get/set accessors behave like property accessors with parameters.

- Parameter types define key or coordinate semantics (`string key`, `int index`).
- Read-only indexers omit set accessor.
- Default parameter values are not allowed on indexer parameters.
- Collection initializer syntax on custom types requires public `Add` or accessible indexer set.

---

#### Q10. When should you use a full property with validation vs an auto-property? {#02-properties-indexers-q10}

When should you use a full property with validation vs an auto-property?

**Answer:** Use a full property with explicit backing field when assignment must validate ranges, normalize input, raise change notifications, or lazy-load expensive data. Use auto-properties when any valid value of the type is acceptable and no side effects are needed on get/set.

- Transition from auto to full property without changing public API surface beyond behavior.
- Throw `ArgumentOutOfRangeException` in set for invalid domain values.
- INotifyPropertyChanged implementations require full properties to invoke events on change.
- YAGNI: start auto, upgrade when rules appear—avoid premature validation boilerplate.

---

#### Q11. What is a computed/read-only property that derives its value from other members? {#02-properties-indexers-q11}

What is a computed/read-only property that derives its value from other members?

**Answer:** A read-only property calculates its return value from other fields or properties each time it is read, expressing derived state like `FullName => $"{First} {Last}"` or `IsAdult => Age >= 18` without storing redundant fields.

- Avoid side effects in getters; keep them predictable for debugging and binding.
- Cache in private field if computation is expensive and invalidation is manageable.
- Computed properties should not create inconsistent mutable state separate from source fields.
- Expression-bodied syntax common for simple computed properties—Q7.

---

#### Q12. What is the difference between `init` properties and constructor parameters for immutable objects? {#02-properties-indexers-q12}

What is the difference between `init` properties and constructor parameters for immutable objects?

**Answer:** Constructor parameters enforce required values at creation with explicit signature; init properties allow object initializer syntax and optional members while still preventing post-construction mutation. Records combine both with positional syntax.

- Constructors validate in one place; multiple constructor overloads may duplicate validation without `: this()`.
- Init properties suit many optional immutable fields with initializer ergonomics.
- Required members (C# 11+) annotate mandatory init properties compile-time.
- Choose constructor-only for small immutable types; init + initializer for many optional fields.

---

#### Q13. How do properties participate in object initializer syntax? {#02-properties-indexers-q13}

How do properties participate in object initializer syntax?

**Answer:** Object initializers assign to settable properties and fields after the constructor runs: `new Customer { Name = "Ada", Id = 1 }`. Init-only properties accept assignments there; get-only properties without init cannot be set in initializer.

- Order of initializer assignments follows source text; dependencies between properties should not assume order unless documented.
- Collection initializers target properties returning mutable collections or indexers.
- Constructor still establishes required invariants before initializer assignments execute.
- See Classes Q5 and Q14.

---

#### Q14. What is a preview-level understanding of `record` types and synthesized properties? {#02-properties-indexers-q14}

What is a preview-level understanding of `record` types and synthesized properties?

**Answer:** Records (C# 9+) are reference types (or struct records) with compiler-synthesized equality, `ToString`, and clone members, often using primary constructor parameters that become init or get-only properties for concise immutable data carriers.

- `record Person(string Name, int Age);` creates positional properties `Name` and `Age`.
- Value equality by default compares property values, not reference identity.
- `with` expressions create copies with selective property changes.
- Records suit DTOs and domain events; behavior-rich entities may remain classes.

---

#### Q15. Why might exposing a public `{ get; set; }` on a collection-typed property break encapsulation? {#02-properties-indexers-q15}

Why might exposing a public `{ get; set; }` on a collection-typed property break encapsulation?

**Answer:** Callers can replace or mutate the internal collection without going through your type's methods, bypassing invariants like duplicate prevention, sorting, or synchronization—Gotcha 9 Module 02.

- Exposing `List<T>` allows `obj.Items.Clear()` from outside without your knowledge.
- Prefer `IReadOnlyList<T>` public get with private mutable backing list, or defensive copies on get.
- See Encapsulation chapter Q4 and Q9.
- Initialize collection properties to empty instances to avoid null reference on add.

---

#### Q16. What is the difference between an indexer and a method named `GetByIndex`? {#02-properties-indexers-q16}

What is the difference between an indexer and a method named `GetByIndex`?

**Answer:** Indexers use `obj[key]` syntax integrated with language indexing semantics and collection initializers; methods use `obj.GetByIndex(key)` explicit call syntax without participating in indexer language features.

- Indexers feel natural for collection-like types; methods clarify intent for non-collection lookups.
- Indexers cannot be extension members; methods can be extensions in static classes.
- Performance is equivalent; choice is API ergonomics and framework conventions (` IList<T>` uses indexer).
- Overloading rules differ slightly in discoverability for tooling.

---

#### Q17. Can interface types declare indexers, and how are they implemented? {#02-properties-indexers-q17}

Can interface types declare indexers, and how are they implemented?

**Answer:** Interfaces may declare indexers with get/set requirements; implementing classes provide `this[...]` accessors matching the contract, either publicly or through explicit interface implementation when name clashes occur.

- `interface IMap { string this[string key] { get; set; } }`
- Explicit implementation: `string IMap.this[string key] { get => ...; set => ...; }`
- Consumers typed as interface use indexer syntax through interface reference.
- Same explicit implementation hiding patterns as methods—Gotcha 12 Module 02.

---

#### Q18. What is the relationship between properties and data binding / serialization frameworks? {#02-properties-indexers-q18}

What is the relationship between properties and data binding / serialization frameworks?

**Answer:** Data-binding (WPF, ASP.NET model binding) and serializers (System.Text.Json, XmlSerializer) typically discover public readable/writable properties by convention, ignoring fields unless configured otherwise.

- Missing public setters affects deserialization and two-way binding unless custom converters exist.
- `[JsonIgnore]` and similar attributes target properties to control serialization shape.
- Init-only properties work with serializers that support immutable object patterns in modern versions.
- Naming conventions (`Id`, `Name`) align with model binding from query strings and JSON bodies.

---

### 03. Constructors & Method Overloading

#### Q1. Explain constructors and their types in C# (default, parameterized, static, private). {#03-constructors-method-overloading-q1}

(R) A teammate refactors `OrderLine` to chain constructors like the chapter's `Product` type. QA reports invalid lines in production — empty SKU and zero quantity slip through. Review the ctors. What went wrong, and how do you fix it?

**Answer:** The single-parameter constructor does not chain to the validated `(string, int)` ctor — it duplicates initialization logic without guards, so callers using `new OrderLine("")` or `new OrderLine(null)` get empty SKUs that never hit the validation in the three-parameter constructor.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | `OrderLine(string sku)` bypasses `: this(sku, 1)` | Invalid SKU values reach production data |
| Correctness | `sku?.Trim() ?? string.Empty` masks null instead of rejecting | Silent bad state instead of fail-fast at creation |
| Maintainability | Validation duplicated in intent but only implemented once | Future ctors can repeat the same bypass mistake |

**Fix (priority order):**

1. Chain the one-parameter ctor: `public OrderLine(string sku) : this(sku, 1) { }` — single validation path.
2. Keep all invariant checks in the **most complete** ctor (here, `(string sku, int quantity)`), matching the chapter's `Product` pattern in **Program.cs** Sections 1c and 1b.
3. Remove defensive null-coalescing to empty string in convenience ctors — let the validated ctor throw `ArgumentException`.
4. Add unit tests per ctor overload to assert invalid SKU/quantity throws before any repository write.

```csharp
public OrderLine(string sku)
    : this(sku, 1)
{
}
```

**Production takeaway:** Constructor chaining only enforces invariants when **every** ctor path reaches the guarded ctor — a common Karat trap after "helpful" shortcut ctors are added without `: this(...)`.

---

#### Q2. What is a destructor/finalizer in C#? {#03-constructors-method-overloading-q2}

(R) A .NET 8 service adopts a **primary constructor** for a warehouse DTO. Unit tests expecting `ArgumentException` on bad input fail with `NullReferenceException` instead. Review the type. What is the initialization order problem, and how would you enforce invariants?

**Answer:** Field initializers on the primary-constructor type run **before** the instance constructor body block, so `sku.Trim()` executes while `sku` is still null — throwing `NullReferenceException` instead of the intended `ArgumentException` from the guard block below.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Sku = sku.Trim()` before validation block | Wrong exception type; callers/tests cannot rely on contract |
| Correctness | Invariants assumed to run "first" in `{ }` body | Primary ctor initialization order differs from mental model |
| API contract | Mixed primary params + property initializers | Hard to see which line can throw what |

**Fix (priority order):**

1. Validate **before** any use of parameters — either in the constructor body as the first statements with manual assignment to properties, or via a static factory `StockReceipt.Create(...)` that validates then calls a private ctor.
2. Do not call instance methods (`Trim`) on parameters in field/property initializers when null is invalid.
3. Prefer explicit parameterized ctor + chaining for domain types with strict invariants; use primary constructors for simple immutable carriers where validation is minimal or delegated to a factory.
4. Align tests to assert the final exception type after fix (`ArgumentException` for null/whitespace SKU).

```csharp
public sealed class StockReceipt
{
    public string Sku { get; }
    public decimal UnitCost { get; }
    public int Quantity { get; }

    public StockReceipt(string sku, decimal unitCost, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (unitCost < 0m)
            throw new ArgumentOutOfRangeException(nameof(unitCost));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        Sku = sku.Trim();
        UnitCost = unitCost;
        Quantity = quantity;
    }
}
```

**Production takeaway:** Primary constructors do not replace the chapter rule — **enforce invariants at creation** — but the execution order is initializer expressions first, then body; Karat tests whether you know where validation must live.

---

#### Q3. Explain constructor chaining in C# (`: this(...)` vs `: base(...)`). {#03-constructors-method-overloading-q3}

(R) After adding a convenience overload to `LineItemCalculator`-style pricing helpers, `dotnet build` fails with **CS0121** ("The call is ambiguous"). Which overloads conflict, and how do you resolve the call site or signatures?

**Answer:** The call `LineTotal(3, 2.49m, 0.10m)` matches both the three-parameter overload (with optional `discountRate`) and the four-parameter overload equally well — the third argument `0.10m` can bind to either `discountRate` or the third positional parameter before `taxRate`, so the compiler cannot pick a unique best match.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Optional parameter on overload A overlaps arity with overload B | CS0121 — build blocked |
| Design | Two overloads differ only by trailing optional vs required extension | Call sites with three decimals are ambiguous |
| Maintainability | Mixing optional params and extra overloads (chapter Section 9 warning) | Every new decimal argument risks new ambiguity |

**Fix (priority order):**

1. **Preferred:** Remove the optional from the three-parameter signature — use two explicit overloads (`qty, price` and `qty, price, discount`) plus a separate `WithTax(...)` method, mirroring **Program.cs** `Price(int, decimal)` vs `Price(int, decimal, decimal)`.
2. At the call site, disambiguate with a **named argument**: `LineTotal(3, 2.49m, discountRate: 0.10m)` if you must keep the optional temporarily.
3. Avoid `params` + optional + overlapping arity in the same method group — chapter Section 15 / CS0121.
4. Add a compiler-focused unit test project or analyzer rule comment so overlapping optionals are caught in review.

**Production takeaway:** Overload resolution is compile-time — ambiguous APIs never ship — but Karat uses this to test whether you can diagnose **optional parameters colliding with additional overloads**, not just recall the CS0121 code.

---

#### Q4. How can you call the base class constructor from a derived class? {#03-constructors-method-overloading-q4}

(M) A junior dev models discounted inventory items by inheriting from `Product` (chapter pattern). The project does not compile. Diagnose **`: this(...)` vs `: base(...)`** mistakes and state the correct ctor initialization order.

**Answer:** Attempt A lists **both** `: base(...)` and `: this(...)` on one constructor header — only one initializer is allowed (CS2506). Attempt B omits `: base(...)` while `Product` has no parameterless ctor, so the compiler cannot construct the base part (CS7036). Derived ctors must eventually reach the parent through `: base(...)`; `: this(...)` only delegates to another ctor in the **same** derived type. Initialization order: static base → static derived → instance base → instance derived → base ctor body → derived ctor body.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `: base(...)` and `: this(...)` on Attempt A | CS2506 — only one constructor initializer permitted |
| Compile | Attempt B missing `: base(name, unitPrice)` | CS7036 — no accessible parameterless base ctor |
| Design | Treating `: this(...)` as a way to call the parent | Parent state never constructed; validation in `Product` skipped |

**Fix (priority order):**

1. Split Attempt A into **either** sibling delegation **or** base forwarding — not both on one header:
   `public DiscountedProduct(string name, decimal unitPrice, decimal discountRate) : this(name, unitPrice, discountRate, applyMinimum: true) { }`
2. Fix Attempt B: `public DiscountedProduct(..., bool applyMinimum) : base(name, unitPrice) { DiscountRate = discountRate; }`.
3. Ensure every `: this(...)` chain in the derived class terminates at a ctor that calls `: base(...)` so `Product` validation runs — matching chapter **Program.cs** Section 3 preview.
4. Document ctor order in review: base static → derived static → base instance → derived instance → base ctor → derived ctor.

**Production takeaway:** `: this(...)` chains within a type; `: base(...)` crosses inheritance — Karat stacks this with CS7036 when the base parameterless ctor disappears after adding a parameterized base ctor.

---

#### Q5. In what order do constructors and field initializers run in an inheritance chain? {#03-constructors-method-overloading-q5}

(P) An ASP.NET Core API maps inbound JSON to a **`required`** init-only request type before calling domain ctors. A client omits `Name` but the payload still deserializes and reaches `new Product(...)`. What happened at compile time vs runtime, and how do you align API contracts with constructor validation?

**Answer:** `required` is enforced at **object creation** for object initializers and `new()` expressions at compile time, but **System.Text.Json** (and Newtonsoft) can still materialize instances without required members unless you enable required-member deserialization validation — so `Name` may default to `null` at runtime, and `Product`'s ctor then throws or mis-validates depending on null checks.

- **Compile time:** `new CreateProductRequest { UnitPrice = 8.99m }` without `Name` fails to compile — `required` works for in-code construction.
- **Runtime (JSON):** Deserializer may not enforce `required` unless configured (`JsonSerializerOptions` / `[JsonRequired]` / validation attributes / manual guard in minimal APIs).
- **Domain layer:** `Product(string name, decimal unitPrice)` should still validate — last line of defense — but the API should return **400 ProblemDetails**, not a 500 from an unhandled `ArgumentException`.
- **Alignment:** Use `[Required]` + FluentValidation or ASP.NET model validation, enable required property support for STJ in .NET 7+, map to domain via factory that throws typed validation exceptions converted to 400.

```csharp
// Minimal API guard example:
if (string.IsNullOrWhiteSpace(body.Name))
    return Results.ValidationProblem(new Dictionary<string, string[]>
    {
        [nameof(body.Name)] = ["Name is required."]
    });
```

**Production takeaway:** Required members and constructor validation solve different layers — DTO `required` for developer mistakes, ctor invariants for domain truth, API validation for external clients — Karat tests stacking all three.

---

#### Q6. Explain method overloading and method overriding in C#. {#03-constructors-method-overloading-q6}

(D) A warehouse microservice registers services in DI but still constructs dependencies manually inside ctors. Review startup and `InventorySyncService`. What breaks in tests, lifetimes, and startup, and what pattern replaces it?

**Answer:** The service graph mixes DI registration with static singleton access and throws inside `ProductCatalog`'s ctor during container build — startup fails (or the host never becomes healthy), tests cannot substitute a fake registry, and two lifetimes (DI singleton vs static `Instance`) fight for the same responsibility.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Startup | `new Product("Seed SKU", -1.00m)` in `ProductCatalog` ctor | `ArgumentOutOfRangeException` during `BuildServiceProvider` — app won't start |
| DI | `InventorySyncService` uses `InventoryRegistry.Instance` | Bypasses container; cannot mock `IInventoryRegistry` in tests |
| Lifetime | Static singleton + `AddSingleton<>` duplicate ownership | Hidden global state; unclear thread-safety and test isolation |
| Design | Chapter singleton (`InventoryRegistry`) copied into production service | Violates "prefer DI" note in **Program.cs** Section 2 |

**Fix (priority order):**

1. **Constructor injection:** `public InventorySyncService(IInventoryRegistry registry)` — no parameterless ctor grabbing statics.
2. Register an abstraction: `builder.Services.AddSingleton<IInventoryRegistry, InventoryRegistry>()` with a **public or internal** ctor (or factory delegate) — retire `Instance` for app code; keep private ctor only if factory registration is used.
3. Move seed data out of the ctor — use `IHostedService`, explicit `SeedAsync`, or configuration-driven load so invalid catalog data surfaces as a controlled startup error with logging, not ctor throw during DI resolution.
4. Let `Product`'s validated ctor throw for bad **runtime** input; seed paths must pass valid arguments or use a dedicated test factory.
5. Integration tests build `WebApplicationFactory` with replaced `IInventoryRegistry` fake — only possible when ctors demand interfaces.

```csharp
builder.Services.AddSingleton<IInventoryRegistry, InventoryRegistry>();
builder.Services.AddSingleton<IInventorySyncService, InventorySyncService>();

public sealed class InventorySyncService : IInventorySyncService
{
    private readonly IInventoryRegistry _registry;

    public InventorySyncService(IInventoryRegistry registry)
    {
        _registry = registry;
    }

    public void Sync(Product product) => _registry.Register(product);
}
```

**Production takeaway:** Object creation belongs in the composition root — ctors enforce invariants for **their** parameters, not for bootstrapping entire graphs via `new` and static `Instance`; Karat links chapter singleton intro to real ASP.NET Core registration mistakes.

---

---

#### Q7. What is a static constructor, and when does it run? {#03-constructors-method-overloading-q7}

_Answer not found._

---

#### Q8. Can a struct have a parameterless constructor (C# 10+ rules vs earlier)? {#03-constructors-method-overloading-q8}

_Answer not found._

---

#### Q9. What is the difference between a primary constructor (C# 12 on classes/records) and traditional constructors? {#03-constructors-method-overloading-q9}

_Answer not found._

---

#### Q10. What happens if you do not define any constructor — what default constructor is provided? {#03-constructors-method-overloading-q10}

_Answer not found._

---

#### Q11. Why might you mark a constructor `private` (singleton, factory patterns)? {#03-constructors-method-overloading-q11}

_Answer not found._

---

#### Q12. What is constructor overloading, and how does `: this(...)` reduce duplication? {#03-constructors-method-overloading-q12}

_Answer not found._

---

#### Q13. What is the exact order: static constructor, instance field initializers, instance constructor body, base constructor? {#03-constructors-method-overloading-q13}

_Answer not found._

---

#### Q14. What is the difference between method overloading (compile-time) and method overriding (runtime polymorphism)? {#03-constructors-method-overloading-q14}

_Answer not found._

---

#### Q15. When does the compiler fail to pick an overload due to ambiguity involving optional parameters and `params`? {#03-constructors-method-overloading-q15}

_Answer not found._

---

#### Q16. Can constructors be inherited — how does a derived class get a base constructor? {#03-constructors-method-overloading-q16}

_Answer not found._

---

#### Q17. What validation belongs in a constructor vs a factory method? {#03-constructors-method-overloading-q17}

_Answer not found._

---

#### Q18. What is the difference between calling an overloaded instance method vs a static overloaded method? {#03-constructors-method-overloading-q18}

_Answer not found._

---

### 04. Static Members & Static Classes

#### Q1. Explain the `static` keyword in detail. {#04-static-members-static-classes-q1}

(R) An ASP.NET Core API caches the "current user's cart" in a static field so every controller can read it without DI. Under load, users report seeing each other's items. Review the code — what is wrong and how do you fix it?

**Answer:** A static `_items` list is one shared object for the entire application domain — every request overwrites and reads the same cart, so concurrent users bleed data across threads and requests; this is the classic mutable-static-state failure mode in web apps.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable static `_items` holds per-user data | User A sees User B's cart under concurrency |
| Architecture | Static holder bypasses request scope and DI | Hidden global state; untestable without static resets |
| Scale-out | In-memory static state is per process | Sticky sessions won't help — race is on one instance |
| Thread safety | `List<T>` mutated without synchronization | Corrupted list / exceptions under parallel requests |

**Fix (priority order):**

1. Remove `CartContext` static mutable storage — register a **scoped** `ICartService` (or store cart keyed by user id in Redis/SQL).
2. Pass `HttpContext.User` identity into the service; never store "current user" in static fields.
3. If caching shared **read-only** reference data, use `IMemoryCache` or `IOptions<T>` with immutable snapshots — not a static `List` rewritten per request.
4. Add integration tests with parallel HTTP clients to catch cross-user leakage.

**Production takeaway:** Static members are fine for type-level constants and pure helpers (`BankAccount.IsValidRoutingNumber`) — not for request-scoped or user-scoped state. See **Program.cs** Section 1 — "mutable static fields are shared global state."

---

#### Q2. What is a static class in C#? {#04-static-members-static-classes-q2}

(M) A teammate adds runtime config loading to `AppSettings` and reports intermittent `TypeInitializationException` on first request. Review the static initialization — what ordering traps exist, and how would you make startup deterministic?

**Answer:** Static field initializers run in declaration order before the static constructor body, but circular reads between static fields or throwing initializers can fail type initialization once and poison the type for the AppDomain — the fix is to defer I/O to explicit startup (`IConfiguration`) instead of fragile static ctor chains.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Init order | `MaxLoginAttempts = LoadMaxAttempts()` runs before `static AppSettings()` body | `LoadMaxAttempts` uses `ConfigPath` — OK here, but reordering fields breaks silently |
| Runtime | `LoadMaxAttempts` throws on missing/malformed file | `TypeInitializationException` — type unusable until app restart |
| Design | Static ctor performs I/O and logging | Failures happen on first touch, not at controlled startup |
| Web hosting | First request triggers type load | Lazy failure in prod instead of fail-fast at `WebApplication` boot |

**Fix (priority order):**

1. Move config loading to ASP.NET Core **options pattern** — `builder.Services.Configure<LoginOptions>(configuration.GetSection("Login"))` — validate at startup with `ValidateOnStart`.
2. If static readonly is required, keep static fields **simple** (env var only); load file-backed values in `Program.cs` after `builder.Configuration` exists.
3. Avoid static initializers that depend on each other's side effects; document declaration order or use explicit static ctor assignment only.
4. Never swallow exceptions in static constructors — they wrap inner failures in `TypeInitializationException` and hide root cause in logs.

```csharp
// Prefer at startup, not in static type init:
builder.Services.AddOptions<LoginOptions>()
    .Bind(configuration.GetSection("Login"))
    .Validate(o => o.MaxAttempts > 0, "MaxAttempts required")
    .ValidateOnStart();
```

**Production takeaway:** Static constructors run once per type load (**Program.cs** Section 6 preview) — treat them like hidden startup code. Production apps load config through `IConfiguration`, not static field chains that throw on first access.

---

#### Q3. Why can you not override a `static` method? {#04-static-members-static-classes-q3}

(R) Production logging uses the tutorial's `AuditLogger` singleton instead of `ILogger`. Tests pass locally but CI flakes and log counts are wrong under concurrent requests. Review the pattern — what's broken and what replaces it?

**Answer:** Hand-rolled singletons expose untestable global mutable state (`_entryCount++` is not thread-safe) and fight ASP.NET Core's built-in logging pipeline — register `ILogger<T>` and scoped/transient services instead of `AuditLogger.Instance`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Non-interlocked `_entryCount++` | Lost updates / wrong counts under parallel requests |
| Testability | Static `Instance` and private ctor | Tests share global counter; order-dependent flakes |
| DI misuse | `AddSingleton(AuditLogger.Instance)` registers pre-built object | Bypasses container ownership; can't substitute fakes easily |
| Observability | `Console.WriteLine` instead of `ILogger` | No levels, filters, structured fields, or centralized sinks |

**Fix (priority order):**

1. Delete the singleton — inject `ILogger<CheckoutController>` (or an application service) via constructor DI.
2. If audit is a domain concern, define `IAuditService` registered **scoped** or **singleton** only when the implementation is **stateless**; persist counts to storage if needed.
3. Use `Interlocked.Increment` only for cheap diagnostics — not as a substitute for proper logging/metrics (`IMeterFactory`, Application Insights).
4. In tests, use `WebApplicationFactory` with logging providers or mock `ILogger<T>` — no static reset hacks.

```csharp
public class CheckoutController : ControllerBase
{
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(ILogger<CheckoutController> logger) => _logger = logger;

    public IActionResult Checkout()
    {
        _logger.LogInformation("Checkout completed for {UserId}", UserId);
        return Ok();
    }
}
```

**Production takeaway:** **Program.cs** Section 10 previews singleton for learning — production prefers DI singletons (container-managed, interface-based) over static `Instance` accessors. See foundation **Constructors** chapter for thread-safe lazy init when a true single instance is required.

---

#### Q4. What is the difference between a static class and the singleton pattern? {#04-static-members-static-classes-q4}

(R) A developer refactors `TaxHelper` to support per-region tax profiles and adds instance state. Build fails. Review the changes — what rules did they violate, and what structure should replace a static class here?

**Answer:** Static classes cannot have instance members or instance constructors — the compiler rejects instance fields and `TaxHelper(decimal)` on a `static class`; once you need per-object state, convert to an ordinary instance class (often injected via DI) and keep only pure functions static if needed.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Instance field + ctor on `static class` | CS0708 / CS0710 — build blocked |
| Design | Mixed static utility + instance profile in one type | Violates static class purpose (stateless helper group) |
| API | Callers would need `new TaxHelper(...)` | CS0712 — cannot instantiate static class even without other errors |

**Fix (priority order):**

1. Replace `static class TaxHelper` with a normal sealed class, e.g. `ITaxCalculator` / `TaxCalculator`, taking `regionRate` via constructor or options.
2. Register `ITaxCalculator` as scoped or singleton in DI depending on whether rate is per-request or app-wide config.
3. Keep stateless math as `public static decimal CalculateSalesTax(...)` on a separate `TaxMath` static class **or** private static method on the instance class — match **Program.cs** Section 7 (static class = no instance state).
4. Do not inherit from `TaxHelper` — static classes are implicitly sealed; use composition and interfaces instead.

```csharp
public interface ITaxCalculator
{
    decimal CalculateForRegion(decimal amount);
}

public sealed class TaxCalculator : ITaxCalculator
{
    private readonly decimal _regionRate;
    public TaxCalculator(IOptions<TaxOptions> options) => _regionRate = options.Value.Rate;
    public decimal CalculateForRegion(decimal amount) =>
        Math.Round(amount * _regionRate, 2, MidpointRounding.AwayFromZero);
}
```

**Production takeaway:** Static classes (`TaxHelper`, `AppSettings` helpers) are for stateless utilities — the moment you need `this`, use an instance type. Karat tests whether you know CS0712/CS0709 rules from **Program.cs** Section 7, not just memorize `static`.

---

#### Q5. What is a static field, and how is lifetime different from an instance field? {#04-static-members-static-classes-q5}

(R) `BankAccount` account numbers duplicate in production after traffic increases. The team uses the tutorial counter as-is. Review the static field usage — what race exists and how do you fix it without abandoning a shared sequence?

**Answer:** `_nextAccountNumber++` is not atomic — two threads can read the same value before either writes back, producing duplicate `AccountNumber` values; use `Interlocked.Increment` for in-process sequences or a database/ID service for authoritative numbering.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Read-modify-write on `_nextAccountNumber++` | Duplicate account numbers under parallel ctor calls |
| Correctness | Assumes single-threaded console demo semantics | Web API creates many `BankAccount` objects concurrently |
| Scale-out | Static counter is per process | Two pods can still collide — DB sequence or distributed ID required |

**Fix (priority order):**

1. **In-process fix:** assign with `Interlocked.Increment(ref _nextAccountNumber)` (or `Interlocked.Add`) inside the constructor.
2. **Production fix:** generate account numbers from SQL `IDENTITY`/sequence, UUID, or Snowflake-style ID service — static fields don't survive multi-instance deployments.
3. Mark `_nextAccountNumber` `private static` and never expose mutability via public static setters.
4. Add stress test spawning parallel account creation tasks asserting unique numbers.

```csharp
public BankAccount(string ownerName, decimal openingDeposit)
{
    AccountNumber = Interlocked.Increment(ref _nextAccountNumber);
    OwnerName = ownerName;
    Balance = openingDeposit;
}
```

**Production takeaway:** **Program.cs** Section 1 warns that mutable static fields race unless synchronized — the tutorial's counter is correct for demos, not for concurrent web registration endpoints.

---

#### Q6. What is a static property and static method — what is the `this` reference inside them? {#04-static-members-static-classes-q6}

(D) Your API team debates three approaches for shared, read-mostly configuration: `public const` literals, `static readonly` loaded at type init, and mutable `public static` properties set from middleware. Which would you allow in a multi-instance ASP.NET Core deployment, and which would you ban? Why?

**Answer:** Allow `const` for true compile-time literals and immutable `static readonly` only when the value is identical on every instance and never changes after type init; ban mutable `public static` properties for app configuration — use `IOptions<T>` / `IConfiguration` so each pod reads consistent, reloadable, testable settings without global writes from middleware.

**Allow — `const` (e.g., `MaxLoginAttempts = 3`):**

- Fixed at compile time; zero runtime cost; safe to share everywhere.
- Trade-off: changing value requires recompile of all assemblies that inline it (**Program.cs** Section 8 — const metadata inlining).

**Allow with caution — `static readonly` set once at type init (e.g., `EnvironmentName` from env var):**

- OK for process-wide, immutable facts loaded before requests (deployment stamp, machine name).
- Must not read per-request data; env var is fixed for process lifetime.
- Prefer `IOptions<T>` for anything that might reload or differ by environment file.

**Ban — mutable `public static` properties (e.g., `BankAccount.BankName { get; set; }` set from middleware):**

- Creates hidden global state writable from anywhere; race-prone under concurrent requests.
- Multi-instance: each pod has its own static copy — "global" settings drift if one instance mutates.
- Breaks unit tests (order-dependent mutations) and violates DI/test seams.

**Production pattern:** `builder.Services.Configure<BankOptions>(configuration.GetSection("Bank"))` inject `IOptionsSnapshot<BankOptions>` where refresh matters. Keep static classes for pure functions only (`IsValidRoutingNumber`).

**Production takeaway:** **Program.cs** contrasts `const`, `static readonly`, and mutable static properties — in web apps, configuration flows through the options/configuration stack, not static setters touched during the HTTP pipeline.

---

---

#### Q7. Why can static methods not access instance members directly? {#04-static-members-static-classes-q7}

_Answer not found._

---

#### Q8. When are static constructors executed, and how many times per AppDomain/process? {#04-static-members-static-classes-q8}

_Answer not found._

---

#### Q9. What is the difference between `const` (implicitly static) and `static readonly`? {#04-static-members-static-classes-q9}

_Answer not found._

---

#### Q10. Can a static class implement interfaces? {#04-static-members-static-classes-q10}

_Answer not found._

---

#### Q11. What thread-safety concerns apply to mutable static fields? {#04-static-members-static-classes-q11}

_Answer not found._

---

#### Q12. Why is overusing static state a testing and maintainability problem? {#04-static-members-static-classes-q12}

_Answer not found._

---

#### Q13. What is the difference between static nested classes and non-static nested classes? {#04-static-members-static-classes-q13}

_Answer not found._

---

#### Q14. How do static members participate in inheritance — are they polymorphic? {#04-static-members-static-classes-q14}

_Answer not found._

---

### 05. Inheritance & Polymorphism

#### Q1. Explain inheritance in detail in C#. {#05-inheritance-polymorphism-q1}

Explain inheritance in detail in C#.

**Answer:** Inheritance lets a derived class extend a base class, inheriting instance and static members (with accessibility limits) and adding or replacing behavior. C# supports single inheritance of classes plus multiple interface implementation.

- Base class can define virtual methods for override; sealed base stops further derivation.
- Protected members visible to derived classes enable extension while hiding from unrelated code.
- Constructor chaining ensures base initialization—Constructors chapter.
- Favor composition when inheritance only reuses implementation without true is-a relationship—Q13.

---

#### Q2. Explain polymorphism in C# and how it can be achieved. {#05-inheritance-polymorphism-q2}

Explain polymorphism in C# and how it can be achieved.

**Answer:** Polymorphism allows code to operate on abstractions (base class or interface references) while runtime behavior comes from the actual derived type. C# achieves it via virtual method overriding, interface implementation, and implicit interface dispatch.

- `Animal a = new Dog(); a.Speak()` calls `Dog.Speak` if `Speak` is virtual/override.
- Interfaces enable polymorphism without shared base class implementation.
- Pattern matching and switch on type complement but do not replace virtual design for open hierarchies.
- Gotcha 7 warns against long type-check chains defeating polymorphism.

---

#### Q3. What is the difference between compile-time (static) and runtime (dynamic) polymorphism? {#05-inheritance-polymorphism-q3}

What is the difference between compile-time (static) and runtime (dynamic) polymorphism?

**Answer:** Compile-time polymorphism includes method overloading and `new` method hiding—resolved from static type at compile time. Runtime polymorphism uses `virtual`/`override` dispatch based on actual object type, and interface calls through implementing instances.

- Overload: chosen by argument types at compile time.
- Override: `Base b = new Derived(); b.V()` calls Derived at runtime.
- `dynamic` keyword adds runtime binding for member resolution beyond inheritance—Module 01 Q25.
- Default interface methods dispatch with rules for struct boxing—Gotcha 15 Module 02.

---

#### Q4. What is a sealed class in C#? {#05-inheritance-polymorphism-q4}

What is a sealed class in C#?

**Answer:** A `sealed class` cannot be inherited; `sealed override` on a method prevents further overrides in derived classes. Sealing documents final implementation and enables runtime devirtualization optimizations in some cases.

- `string` and many BCL types are sealed for security and invariant preservation.
- Seal classes when extension via inheritance would break invariants (security, correctness).
- Cannot derive from sealed class—Q16.
- Sealing is optional design choice vs `virtual` extensibility trade-off.

---

#### Q5. What is a virtual method in C#? {#05-inheritance-polymorphism-q5}

What is a virtual method in C#?

**Answer:** A `virtual` method in a base class provides a default implementation that derived classes may replace with `override`, enabling runtime dispatch to the most derived override through a base-typed reference.

- Without `virtual`, methods are non-virtual by default in C# (unlike Java instance methods).
- Virtual methods participate in inheritance chains; `abstract` virtual requires override in derived non-abstract class.
- Calling virtual methods from constructor sees derived overrides before derived initialization completes—Gotcha 1.
- Performance: JIT can devirtualize sealed or known types in optimized tiers.

---

#### Q6. What is the difference between `this` and `base` keywords? {#05-inheritance-polymorphism-q6}

What is the difference between `this` and `base` keywords?

**Answer:** `this` refers to the current instance for disambiguation and passing self; `base` accesses base class members hidden by derived declarations, especially calling `base.Method()` to run parent implementation before or after derived logic.

- `base` in constructor initializer calls base constructor—Constructors Q4.
- `base.Method()` invokes base virtual method even when derived overrides—does not dynamically dispatch to derived when explicitly qualified with `base`.
- `this()` chains constructors on same class.
- Static context has no `this`; `base` only in instance members of derived class.

---

#### Q7. What is operator overloading in C#? {#05-inheritance-polymorphism-q7}

What is operator overloading in C#?

**Answer:** Operator overloading defines static methods with `operator` keyword so expressions like `a + b` compile for user-defined types when overloads exist, subject to language rules on which operators are overloadable.

- Must declare public static overloads; some operators require paired overloads (`==` and `!=`).
- Cannot overload `&&` `||` as user operators though `true`/`false` unary operators enable short-circuit patterns for custom types in limited scenarios.
- Use sparingly for domain types (vectors, money) with intuitive semantics.
- Inconsistent equality operators break collections—Gotcha 14 Module 02.

---

#### Q8. Explain the difference between `virtual`, `abstract`, and `override` keywords. {#05-inheritance-polymorphism-q8}

Explain the difference between `virtual`, `abstract`, and `override` keywords.

**Answer:** `virtual` provides overridable default implementation in concrete base class. `abstract` on class or method requires derived non-abstract class to implement (no body on abstract method). `override` replaces inherited virtual or abstract method in derived class.

| Keyword | On class | On method |
|---|---|---|
| `virtual` | — | Optional override with default body |
| `abstract` | Class cannot instantiate | No body; must override in derived |
| `override` | — | Replaces base virtual/abstract |

Abstract class can mix concrete and abstract methods—chapter 06 Q9.

---

#### Q9. Explain the `new` keyword in the context of method hiding. {#05-inheritance-polymorphism-q9}

Explain the `new` keyword in the context of method hiding.

**Answer:** `new` on a derived method hides a base method with the same signature without overriding; dispatch when calling through base-typed reference uses base method unless static type of reference is derived.

- Does not participate in runtime polymorphism like `override`.
- Warning CS0108 if hiding without `new` keyword—compiler suggests `new`.
- Gotcha 2: mixing hide and override breaks expectations when calling through base type.
- Use `override` when polymorphism intended; `new` when base API should remain unchanged for base references.

---

#### Q10. Explain how C# handles multiple inheritance (using interfaces). {#05-inheritance-polymorphism-q10}

Explain how C# handles multiple inheritance (using interfaces).

**Answer:** C# allows a class to inherit one base class at most but implement multiple interfaces, gaining polymorphic contracts from each interface without merging implementation from multiple class hierarchies.

- `class Worker : Person, IEmployable, IPayable` inherits Person once and implements both interfaces.
- Interface methods implemented explicitly or publicly on the class.
- Default interface methods (C# 8+) supply shared implementation on interfaces without class base duplication.
- Diamond problem for classes avoided; interfaces with default methods have resolution rules—chapter 06 Q12.

---

#### Q11. Why does C# not support multiple inheritance of classes? {#05-inheritance-polymorphism-q11}

Why does C# not support multiple inheritance of classes?

**Answer:** Multiple class inheritance complicates object layout, virtual dispatch, constructor chaining, and the diamond problem where two base classes provide conflicting implementations of the same method. C# chose single inheritance plus interfaces for clarity and predictable memory layout.

- COM and CLR object model simplified with single inheritance chain.
- Composition and interfaces cover most multiple reuse scenarios without MI complexity.
- Languages with MI require complex resolution rules C# designers avoided.
- See Q13 favor composition guideline.

---

#### Q12. What is the fragile base class problem? {#05-inheritance-polymorphism-q12}

What is the fragile base class problem?

**Answer:** Derived classes depend on base class implementation details; changes in base (new virtual methods, altered sequence) break subclasses unexpectedly. Virtual calls from base constructors exacerbate the issue—Gotcha 1.

- Mitigate by sealing classes, minimizing virtual surface, documenting extension points, using composition.
- Framework authors treat unsealed public classes as extensibility contracts with versioning cost.
- See Classes Q17 high-level summary.
- Unit tests on derived classes may fail when base library updates silently change behavior.

---

#### Q13. Why is "favor composition over inheritance" a common guideline? {#05-inheritance-polymorphism-q13}

Why is "favor composition over inheritance" a common guideline?

**Answer:** Composition builds types by containing helper objects and delegating behavior, avoiding tight coupling to base class implementation and fragile override chains. Inheritance exposes derived classes to base changes and deep hierarchy maintenance costs.

- Wrapper pattern (`class LoggingRepository : IRepository` delegating to inner repo) swaps behavior without subclassing concrete base.
- Inheritance suits true is-a polymorphic relationships with stable abstractions.
- Deep inheritance trees obscure where behavior originates.
- Strategy pattern uses composition with interfaces—OOP Examples Q11.

---

#### Q14. What is runtime dispatch — how does the CLR resolve `override` calls through a base reference? {#05-inheritance-polymorphism-q14}

What is runtime dispatch — how does the CLR resolve `override` calls through a base reference?

**Answer:** For virtual calls, the CLR uses the method table of the actual object type at runtime to locate the most derived override, ignoring the compile-time static type of the reference for instance virtual methods.

- Each object header points to method table with slot for virtual methods; override replaces slot in derived table layout.
- Non-virtual calls bind to compile-time type method directly without virtual indirection.
- `callvirt` IL instruction enforces virtual dispatch and null check on instance.
- Sealed override enables devirtualization optimization when JIT proves final type.

---

#### Q15. What is the difference between hiding with `new` and overriding with `override` when calling through a base-typed variable? {#05-inheritance-polymorphism-q15}

What is the difference between hiding with `new` and overriding with `override` when calling through a base-typed variable?

**Answer:** With `override`, `Base b = new Derived(); b.M()` calls `Derived.M` at runtime. With `new` hiding, same call invokes `Base.M` because dispatch uses static type of reference `Base` for non-virtual hidden method.

- Gotcha 2 is core interview trap.
- Explicit cast to `Derived` calls hidden method on derived: `((Derived)b).M()`.
- Polymorphic designs should use `virtual`/`override`, not `new`.
- Interface implementation always uses runtime type of implementing object for interface calls.

---

#### Q16. Can you inherit from a sealed class? {#05-inheritance-polymorphism-q16}

Can you inherit from a sealed class?

**Answer:** No—sealed classes cannot serve as base classes; attempting to derive produces compile error CS0509.

- Sealed types include `string`, `Enum`, and many BCL security-sensitive classes.
- Seal when extension would violate invariants or security assumptions.
- Use interfaces or composition to extend behavior of sealed types.
- `sealed override` on method stops further override but class itself may still be subclassed unless class is sealed.

---

#### Q17. What is the difference between `is` type testing and casting in polymorphic code paths? {#05-inheritance-polymorphism-q17}

What is the difference between `is` type testing and casting in polymorphic code paths?

**Answer:** `is` checks compatibility and supports patterns without throwing; casting `(Derived)b` throws `InvalidCastException` on failure. In polymorphic code, prefer `is` patterns or virtual methods over repeated casts.

- `if (b is Dog d)` assigns typed variable on success.
- Cast required when you know type after guard or for value types unboxing.
- Excessive type tests suggest missing virtual abstraction—Gotcha 7.
- Switch expressions on type patterns scale better than cast chains.

---

#### Q18. What is the Liskov Substitution Principle in one sentence, and how does it relate to inheritance? {#05-inheritance-polymorphism-q18}

What is the Liskov Substitution Principle in one sentence, and how does it relate to inheritance?

**Answer:** Liskov Substitution Principle states that objects of a derived class must be usable anywhere their base class is expected without breaking correctness—derived types must honor the base contract, not strengthen preconditions or weaken postconditions.

- Violation example: `Square`/`Rectangle` with settable width/height breaking area invariants—OOP Examples Q2.
- Inheritance implies substitutability; if derived breaks callers of base, inheritance was wrong model.
- Prefer interfaces defining minimal contracts derived types can reliably fulfill.
- Related to polymorphism safety in tests using mocks substituting real implementations.

---

#### Q19. When does `base.Method()` call the parent's implementation vs the current type's override? {#05-inheritance-polymorphism-q19}

When does `base.Method()` call the parent's implementation vs the current type's override?

**Answer:** `base.Method()` explicitly invokes the base class's method implementation for that virtual method, bypassing the derived override for that call site even though the object is derived. Normal virtual call without `base` uses most derived override.

- Useful when derived override extends rather than replaces base behavior (call base first).
- `base` qualified calls are non-virtual dispatch to immediate base implementation in the inheritance chain step.
- Differs from calling through base-typed reference with hidden `new` methods—Q15.
- Constructor cannot call overridable virtual methods safely before derived init—Gotcha 1.

---

#### Q20. What is the difference between extending behavior with inheritance vs wrapping with composition? {#05-inheritance-polymorphism-q20}

What is the difference between extending behavior with inheritance vs wrapping with composition?

**Answer:** Inheritance extends by substituting a subtype that IS-A base, overriding virtual methods for changed behavior. Composition wraps an inner object HAS-A collaborator, forwarding calls and optionally intercepting without subclassing the inner type.

- Inheritance couples to base implementation; composition couples to interface of inner object swappable at runtime.
- Decorator pattern uses composition to stack behaviors.
- Inheritance depth increases fragile base risk; composition localizes changes.
- OOP Examples chapter SOLID guidance reinforces composition for extension—Q5–Q7.

---

### 06. Abstract Classes & Interfaces

#### Q1. Explain abstraction in detail in C#. {#06-abstract-classes-interfaces-q1}

(D) Your team is adding a `SpreadsheetDocument` to the document archive. It shares `Title` and `CreatedOn` with invoices and reports, but also needs optional CSV export and a separate audit trail that other document types may never use. A junior dev proposes making everything an interface:

```csharp
public interface ISpreadsheetDocument
{
    string Title { get; }
    DateTime CreatedOn { get; }
    string RenderContent();
    string Export(string format);
    void WriteAuditEntry(string action);
}
```

How would you model this using abstract classes and interfaces (as in this chapter), and why?

**Answer:** Keep the **IS-A** document hierarchy on an abstract `Document` base for shared state and rendering contract, then add **CAN-DO** interfaces only for optional capabilities — `IExportable` for export, a narrow `IAuditable` (or similar) for audit — instead of one fat document interface.

- **Abstract `Document`:** `Title`, `CreatedOn`, protected constructor, abstract `DocumentKind` and `RenderContent()`, plus concrete `GetSummary()` — matches **Program.cs** Sections 1 and 3; `SpreadsheetDocument : Document` reuses helpers without duplicating fields.
- **`IExportable`:** Export is a cross-cutting capability; invoices, reports, and spreadsheets can implement it without forcing audit on types that do not need it.
- **`IAuditable` (small interface):** Only types that write audit entries implement `WriteAuditEntry`; reports that never audit are not forced to stub empty methods.
- **Why not one interface:** Duplicates state across unrelated "documents," blocks multiple inheritance of implementation, and violates Interface Segregation — consumers that only export must know about audit members.
- **Both together:** `class SpreadsheetDocument : Document, IExportable, IAuditable` — single class hierarchy, multiple optional behaviors, same pattern as `InvoiceDocument : Document, IExportable, IPrintable, INamedDocument`.

**Production takeaway:** Abstract class for shared identity and partial implementation; interfaces for capabilities that cut across hierarchies. See this chapter's **Document** + **IExportable** split and foundation **Abstract class vs interface** table.

---

#### Q2. What is the difference between abstraction and encapsulation? {#06-abstract-classes-interfaces-q2}

(R) A storage service saves file names for archived documents. After deployment, some invoices overwrite each other on disk. Review:

```csharp
public sealed class InvoiceStorageService
{
    public string ResolveFileName(InvoiceDocument invoice)
    {
        // Human-readable label for UI and logs
        return invoice.GetName();
    }

    public void Save(InvoiceDocument invoice, Stream content)
    {
        string path = Path.Combine(_root, ResolveFileName(invoice));
        using var file = File.Create(path);
        content.CopyTo(file);
    }
}
```

`InvoiceDocument` implements `INamedDocument` with explicit `string INamedDocument.GetName()` returning a file-safe name, and a public `GetName()` returning `"Invoice: " + Title`. What is wrong, and how do you fix it?

**Answer:** The storage service calls the **public** `GetName()` (display label with spaces and punctuation), not the **explicit** `INamedDocument.GetName()` (file-safe slug) — two invoices with the same title collide on disk because paths are not unique or filesystem-safe.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `ResolveFileName` uses public `GetName()` instead of `INamedDocument` contract | Duplicate paths; overwrites; invalid characters on some OSes |
| API surface | Explicit implementation is invisible on concrete type | Callers assume one `GetName()` — easy to pick the wrong one |
| Design | Storage depends on concrete `InvoiceDocument` | Harder to test; wrong abstraction for "file naming" capability |

**Fix (priority order):**

1. Resolve names through the interface: `((INamedDocument)invoice).GetName()` or accept `INamedDocument` / `IFileNaming` in `ResolveFileName`.
2. Add uniqueness: append document id or hash if titles can repeat — explicit slug alone may still collide.
3. Rename public method to `GetDisplayName()` if both names must coexist on the type — reduces accidental misuse.
4. Unit-test storage with two invoices sharing a title; assert distinct file paths.

```csharp
public string ResolveFileName(INamedDocument named)
{
    return named.GetName(); // explicit implementation invoked via interface
}
```

**Production takeaway:** Explicit interface implementation exists precisely when the public API and contract differ — services must depend on the **interface variable**, as **Program.cs** Section 5 demonstrates with `namedContract.GetName()` vs `invoice.GetName()`.

---

#### Q3. What is the difference between abstraction and polymorphism? {#06-abstract-classes-interfaces-q3}

(R) A PR introduces a "kitchen sink" capability interface for the export pipeline. Review:

```csharp
public interface IDocumentCapabilities
{
    string Export(string format);
    string Print();
    string GetName();
    byte[] RenderPdf();
    void SendToPrinter(string queueName);
    string SignWithCertificate(string thumbprint);
}

public class ExportOrchestrator
{
    public void RunBatch(IEnumerable<IDocumentCapabilities> items, string format)
    {
        foreach (var item in items)
        {
            _logger.LogInformation(item.Export(format));
        }
    }
}
```

Only invoices need signing; reports only export. What design problems do you see, and how would you refactor?

**Answer:** `IDocumentCapabilities` is a **fat interface** that violates the **Interface Segregation Principle** — every implementer must stub or throw for unrelated members, and callers cannot express minimal dependencies.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design (ISP) | One interface bundles export, print, PDF, signing, naming | `ReportDocument` forced to implement `SignWithCertificate` with `NotSupportedException` |
| Maintainability | New capability added to interface breaks all implementers | Package version churn; empty stubs multiply |
| Testing | Fakes must implement six methods to test export-only orchestrator | Bloated test doubles; brittle mocks |
| API clarity | `ExportOrchestrator` only needs `Export` but depends on mega-contract | Misleading type bounds; hides true requirements |

**Fix (priority order):**

1. Split into focused interfaces — `IExportable`, `IPrintable`, `ISignable`, `INamedDocument` — matching this chapter's pattern.
2. Change orchestrator signature to `IEnumerable<IExportable>` (as **ExportService.ExportAll** does).
3. Compose at call site: pass types that implement multiple interfaces; use pattern matching or separate services for signing/printing steps.
4. If a facade is needed for DI registration, use a small adapter per document type — not a monolithic interface.

**Production takeaway:** Prefer several small interfaces over one "capabilities" blob — callers depend on what they use, implementers only provide what they support. See **Program.cs** Section 4 (`InvoiceDocument` implements three interfaces, not one fat type).

---

#### Q4. What is the difference between an abstract class and an interface? {#06-abstract-classes-interfaces-q4}

(M) The team ships a NuGet package with `IExportable` consumed by ten internal services. To add optional metadata without breaking implementers, they add a C# 8 default method:

```csharp
public interface IExportable
{
    string Export(string format);

    string ExportWithMetadata(string format)
    {
        return Export(format) + " | exported=" + DateTime.UtcNow.ToString("O");
    }
}
```

An older service still targets `netstandard2.0` and references the updated package. A newer ASP.NET Core service on `net8.0` overrides `ExportWithMetadata` in one document type. What breaks or surprises you in build, runtime, and testing — and what would you document for consumers?

**Answer:** Default interface methods require **C# 8+** and a runtime that supports them — `netstandard2.0` consumers may **fail to compile** or cannot override defaults the same way; even on modern runtimes, dispatch through the interface vs concrete type can surprise callers who expect polymorphic override behavior.

- **Build / TFM:** Default interface members are not available on older language/runtime combinations targeting pre-C#-8 projects — the package bump may block the legacy service until it retargets or the new member is moved to an extension method or separate `IExportableV2`.
- **Binary compatibility:** Adding a default method is often safer than adding a **required** abstract member (which breaks all implementers), but implementers on C# 8+ can override — document which types customize metadata vs inherit default.
- **Dispatch nuance:** Calling `ExportWithMetadata` on `IExportable` uses the most specific override on the implementing type; calling on concrete class without override uses default — tests must use the same reference type production uses.
- **Testing:** Fakes implementing `IExportable` inherit the default unless they override — unit tests may accidentally assert timestamp behavior from the default implementation instead of domain logic.
- **Alternative for wide compatibility:** Extension method `ExportWithMetadata(this IExportable e, ...)` or compositional wrapper — works on `netstandard2.0` without DIM.

**Production takeaway:** Default interface methods help evolve shared contracts with optional behavior (**Program.cs** Section 7 preview), but package authors must treat TFMs, override rules, and test doubles as part of the public API — not every consumer upgrades language version with the package.

---

#### Q5. What is the difference between an abstract class and an interface before C# 8 vs after (default interface methods)? {#06-abstract-classes-interfaces-q5}

(R) Unit tests for `DocumentProcessor` are slow and require real PDF files on disk because production code was wired to concrete types. Review:

```csharp
public sealed class DocumentProcessor
{
    private readonly PdfRenderer _renderer = new PdfRenderer(); // reads templates from disk

    public string BuildBatchSummary(IReadOnlyList<InvoiceDocument> documents)
    {
        var builder = new StringBuilder();
        foreach (var doc in documents)
        {
            builder.AppendLine(doc.GetSummary());
            builder.AppendLine(_renderer.Render(doc)); // not on Document base
        }
        return builder.ToString();
    }
}
```

The chapter's `DocumentProcessor` accepts `IReadOnlyList<Document>` and `ExportService` accepts `IEnumerable<IExportable>`. What is wrong here, and how would you introduce test seams?

**Answer:** The processor **news up** a concrete `PdfRenderer`, accepts only `InvoiceDocument`, and mixes summary building with PDF rendering — no injection point, so tests hit disk and cannot substitute a fake.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Testability | `new PdfRenderer()` inside the class | Tests require filesystem templates; slow, flaky CI |
| Abstraction | Parameter is `InvoiceDocument` not `Document` | Cannot reuse batch logic for reports; breaks polymorphism |
| SRP / design | Summary builder coupled to PDF rendering | Changing render strategy forces retesting batch orchestration |
| DI | Hidden dependency | ASP.NET Core cannot register or swap renderer per environment |

**Fix (priority order):**

1. Extract rendering behind an interface — `IDocumentRenderer` or reuse `IExportable` / a narrow `IRenderable` with `string Render()` — inject via constructor.
2. Accept abstractions on the base type: `IReadOnlyList<Document>` for polymorphic summaries (**Program.cs** Section 6).
3. Register `PdfRenderer` in DI for production; register `FakeRenderer` in tests returning fixed strings.
4. Keep orchestration thin — `BuildBatchSummary` calls `document.GetSummary()` and `document.RenderContent()` on the abstract base where possible; PDF-specific work lives in export/render services.

```csharp
public sealed class DocumentProcessor
{
    private readonly IDocumentRenderer _renderer;

    public DocumentProcessor(IDocumentRenderer renderer) => _renderer = renderer;

    public string BuildBatchSummary(IReadOnlyList<Document> documents)
    {
        var builder = new StringBuilder();
        foreach (var doc in documents)
        {
            builder.AppendLine(doc.GetSummary());
            builder.AppendLine(_renderer.Render(doc));
        }
        return builder.ToString();
    }
}
```

**Production takeaway:** Interfaces are test seams — depend on abstractions, inject implementations. The chapter's static `DocumentProcessor` / `ExportService` illustrate the **dependency direction**; production services add constructor injection and fakes for fast tests.

---

#### Q6. Why do we need interfaces in C#? {#06-abstract-classes-interfaces-q6}

(D) Code review: two approaches for a payment-notification feature.

**Option A — one abstract base:**

```csharp
public abstract class NotifierBase
{
    public abstract void Send(string recipient, string message);
    protected void LogAttempt(string recipient) { /* shared */ }
}
```

**Option B — interface only:**

```csharp
public interface INotificationSender
{
    void Send(string recipient, string message);
}
```

Some notifiers are `EmailNotifier : NotifierBase`; others are `SmsNotifier : INotificationSender` with no shared base. When do you pick abstract class, interface, or both — and what is your decision rule for this codebase?

**Answer:** Use an **abstract base** when notifiers truly share state or concrete helpers (logging, retry policy, template loading); use an **interface** when the only contract is "can send" across unrelated types; use **both** when shared infrastructure belongs in a base but multiple channels must also be substitutable in DI and tests.

**Decision rule (aligned with this chapter):**

| Signal | Choose |
|---|---|
| Shared fields, protected helpers, single IS-A hierarchy | Abstract class (`NotifierBase`) |
| Unrelated types (email, SMS, webhook) must be swappable | `INotificationSender` interface |
| Shared logging/retry **and** need multiple inheritance of behavior | Base class for shared code + `INotificationSender` implemented by base or subclasses |
| Only some notifiers support attachments/signing | Separate small interfaces — do not bloated base |

**For this codebase:**

- **`INotificationSender`** for DI registration, controllers, and unit tests — same role as `IExportable` in **ExportService**.
- **`NotifierBase`** only if most channels share `LogAttempt`, correlation id, or configuration — avoid forcing SMS through an email-centric hierarchy.
- **`SmsNotifier : INotificationSender`** without base is valid when there is nothing to share — do not invent an abstract class for one method.

**Production takeaway:** Abstract class answers "what are they in common?" Interface answers "what can they do for me?" The chapter's **Document** + **IExportable** combination is the template — base for identity, interfaces for pluggable capabilities and test doubles.

---

---

#### Q7. What is explicit interface implementation and when is it used? {#06-abstract-classes-interfaces-q7}

_Answer not found._

---

#### Q8. What are static abstract members in interfaces (C# 11)? {#06-abstract-classes-interfaces-q8}

_Answer not found._

---

#### Q9. Can an abstract class have concrete (non-abstract) methods? {#06-abstract-classes-interfaces-q9}

_Answer not found._

---

#### Q10. Can a class implement multiple interfaces — what about an interface inheriting another interface? {#06-abstract-classes-interfaces-q10}

_Answer not found._

---

#### Q11. When would you choose an abstract base class over an interface for shared implementation? {#06-abstract-classes-interfaces-q11}

_Answer not found._

---

#### Q12. What is the diamond problem, and how does C# avoid it for classes but address it for interfaces with default methods? {#06-abstract-classes-interfaces-q12}

_Answer not found._

---

#### Q13. What is explicit interface implementation — why might `((IMyInterface)obj).Method()` work when `obj.Method()` does not? {#06-abstract-classes-interfaces-q13}

_Answer not found._

---

#### Q14. Can interfaces declare fields, constructors, or static concrete state (pre- and post-C# 8)? {#06-abstract-classes-interfaces-q14}

_Answer not found._

---

#### Q15. What is the difference between `IReadOnlyList<T>` as a parameter type and `List<T>` for abstraction? {#06-abstract-classes-interfaces-q15}

_Answer not found._

---

#### Q16. When should API surface depend on interfaces vs abstract classes? {#06-abstract-classes-interfaces-q16}

_Answer not found._

---

### 07. Encapsulation & Access Modifiers

#### Q1. Explain encapsulation in C# with examples. {#07-encapsulation-access-modifiers-q1}

(R) A junior developer "simplifies" the chapter's `BankAccount` for a payments microservice. QA reports negative balances in production. Review the change — what broke the invariant, and how do you fix it?

**Answer:** Exposing `Balance` as a public setter lets any caller bypass `TryWithdraw` and `Deposit` rules — the type no longer owns its invariant, so external code and concurrent writers can corrupt state even though the methods still look correct.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Public `{ get; set; }` on domain state | Callers assign `Balance` directly — negative balances, skipped validation |
| Concurrency | Read-modify-write on public property | Two threads can interleave `Balance +=` / `-=` without synchronization |
| Encapsulation | Methods enforce rules; property ignores them | `TryWithdraw` checks become advisory — support scripts and mappers bypass them |
| API contract | Mutable balance contradicts audit expectations | Ledger reconciliation finds amounts that never passed `Deposit`/`TryWithdraw` |

**Fix (priority order):**

1. Restore a **private** backing field (`private decimal _balance`) — no public setter.
2. Expose balance read-only: `public decimal Balance => _balance;` or keep `GetBalance()` — callers observe, they do not mutate.
3. Route all changes through methods that enforce invariants (positive deposits, sufficient funds, audit logging).
4. If external systems must post adjustments, add an explicit `ApplyAdjustment(decimal amount, string reason, IAuthorizationContext ctx)` that validates authorization — never a bare setter.
5. For concurrent updates, guard mutations with a lock, database transaction, or optimistic concurrency token — encapsulation alone does not fix races.

```csharp
private decimal _balance;

public decimal Balance => _balance;

public bool TryWithdraw(decimal amount, out string message)
{
    // sole path to decrease _balance
}
```

**Production takeaway:** Auto-properties feel idiomatic in C#, but a public setter on invariant-bearing state is a field in disguise — Karat uses this to test whether you protect rules at the type boundary, not only inside "happy path" methods. See **Program.cs** Section 1 — `BankAccount` keeps `_balance` private.

---

#### Q2. What are the different access modifiers in C#? (`private`, `protected`, `internal`, `protected internal`, `private protected`) {#07-encapsulation-access-modifiers-q2}

(R) A shared library ships both a public façade and internal implementation types. A consuming team references the NuGet package and complains they cannot unit-test ledger entries. Review the library surface:

**Answer:** The library leaked an `internal` type through public method signatures — `CreateLedger` and `PostEntry` expose `InternalLedger` on the public API, which is a compile error for external consumers and breaks the intended assembly boundary even if it compiled.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `public` methods return/accept `internal` type | CS0051/CS0052 — inconsistent accessibility; package may not build |
| API surface | Internal implementation type is part of public contract | Callers depend on types you intended to hide — versioning nightmare |
| Encapsulation | Façade pattern inverted — gateway exposes guts | `List<string> Entries` on internal type becomes reachable if accessibility bug is fixed |
| Testing | Consumers cannot construct `InternalLedger` | Tests forced to go through static gateway — brittle, no seam for fakes |

**Fix (priority order):**

1. Keep `InternalLedger` **internal**; never appear in public signatures.
2. Introduce a **public** abstraction: `public interface ILedger { string PostEntry(string description); }` implemented internally, or return `string`/`LedgerEntryId` DTOs only.
3. `LedgerGateway.PostEntry(string description)` creates the internal ledger internally — matches **Program.cs** Section 3 pattern.
4. For testability inside the library, use `InternalsVisibleTo` for test assembly **or** expose `ILedger` with an internal default implementation registered via DI.
5. Return immutable snapshots (`IReadOnlyList<LedgerEntry>`) rather than live `List<T>` references.

```csharp
public static class LedgerGateway
{
    public static string PostEntry(string description)
    {
        var ledger = new InternalLedger();
        return ledger.Record(description);
    }
}
```

**Production takeaway:** `internal` types belong behind public façades — leaking them in signatures is worse than making everything public because it fails at compile time and signals unclear API design. See **Program.cs** Section 3 — `LedgerGateway` hides `InternalLedger`.

---

#### Q3. What is the difference between "information hiding" and "data hiding"? {#07-encapsulation-access-modifiers-q3}

(P) Two assemblies in the same solution — `Billing.Core` (library) and `Billing.Tests` — need test access to `internal` pricing helpers without exposing them on the public NuGet surface. A developer adds `InternalsVisibleTo` to the csproj. What does it grant, what risks does it introduce, and what guardrails apply?

**Answer:** `InternalsVisibleTo` lets named friend assemblies access `internal` types and members at compile time — it widens visibility from "same assembly" to "same assembly plus declared friends," without changing `public` NuGet consumers' view if friends are test or first-party tooling projects only.

- **What it grants:** Friend assemblies can reference `internal` classes, methods, and constructors — tests can call pricing helpers, factories, and validators directly without making them `public`.
- **Strong-name caveat:** Signed assemblies require `InternalsVisibleTo` to include the friend's public key (`Include="Tests, PublicKey=..."`) — mismatched keys silently fail to grant access.
- **Risks:** Every friend is a **maintenance coupling** — internal refactors break friend code; overuse turns `internal` into "public but inconvenient." Shipping `InternalsVisibleTo` to production friends (other product teams, plugins) expands your semver surface — `internal` changes become breaking for them.
- **Security:** Friends can invoke internal code paths — do not use IVT to bypass auth; it is a compile-time visibility tool, not a security boundary.
- **Guardrails:** Limit friends to `*.Tests` and build-time tooling; document in ARCHITECTURE.md; prefer `public` interfaces for legitimate extension points; never friend untrusted third-party assemblies; audit IVT entries in code review like public API changes.

**Production takeaway:** Friend assemblies are the idiomatic way to test `internal` implementation without polluting NuGet — Karat tests whether you distinguish visibility for **consumers** vs **collaborators**. See **Program.cs** Quick Reference — InternalsVisibleTo cross-ref to .NET Framework Architecture module.

---

#### Q4. Why is exposing a mutable collection through a public getter an encapsulation break? {#07-encapsulation-access-modifiers-q4}

(R) A domain hierarchy models employee compensation. A subclass "optimizes" payroll by writing directly to protected state. Review:

**Answer:** `protected` fields expose implementation details to every derived class — `CommissionEmployee` can mutate `_baseSalary` and `_auditTrail` without going through `ApplyRaise`, breaking payroll invariants and audit integrity that the base class thought it owned.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encapsulation | `protected` fields instead of controlled hooks | Subclasses bypass validation, logging, and business rules |
| Invariant | Direct `_baseSalary` assignment | Raises applied without approval workflow; minimum wage rules skipped |
| Audit | `_auditTrail.Clear()` | HR/compliance reports lose history — protected mutable collection |
| Design | Base class cannot enforce postconditions on derived behavior | Liskov violations — base assumes audit trail is append-only |

**Fix (priority order):**

1. Make fields **private**; stop exposing raw state to subclasses.
2. Expose controlled extension points: `protected void SetBaseSalary(decimal value, string reason)` that validates and appends audit entries — or make `ApplyRaise` `sealed`/`non-virtual` and use template method with protected abstract hooks that cannot touch salary directly.
3. Return **read-only** audit view: `public IReadOnlyList<string> AuditTrail => _auditTrail.AsReadOnly();` — never expose mutable `List<T>` as `protected`.
4. If derived types need guaranteed minimum, model it as separate state (`_guaranteedMinimum`) combined in a computed `EffectiveBaseSalary` — not by overwriting `_baseSalary`.
5. Consider `private protected` only for same-assembly inheritance helpers — not as a substitute for private fields.

```csharp
private decimal _baseSalary;
private readonly List<string> _auditTrail = new();

protected void AdjustBaseSalary(decimal newSalary, string reason)
{
    if (newSalary < 0) throw new ArgumentOutOfRangeException(nameof(newSalary));
    _baseSalary = newSalary;
    _auditTrail.Add(reason);
}
```

**Production takeaway:** `protected` is not "private but inheritance-friendly" for invariant-bearing data — it is a public API for every future subclass. See **Program.cs** Section 2 — `protected` visibility and Section 1 — behavior enforces invariants, not exposed fields.

---

#### Q5. What is the difference between `protected internal` and `private protected`? {#07-encapsulation-access-modifiers-q5}

(D) Your team designs an immutable `MemberProfile` DTO for cross-service messaging. Proposal A (init-only + `List<string>`) vs Proposal B (factory + `IReadOnlyList`). Which do you ship, and what breaks if callers treat Proposal A as immutable?

**Answer:** Ship Proposal B (or Proposal A only after deep-defensive copying and a read-only exposed collection) — init-only scalars are immutable after construction, but a mutable `List<string>` referenced by the DTO remains live, so "immutable" messages can change under downstream caches and serializers.

**Proposal A — hidden mutability:**

- `Roles` is init-settable once, but the **list contents** mutate forever — any holder can `dto.Roles.Add("Admin")` without reconstructing the DTO.
- Passing the DTO through a message bus, in-memory queue, or ORM session shares one list instance — Service B mutates roles; Service C sees the change — violates message immutability expectations.
- JSON deserializers often populate concrete `List<T>` — deserialization is fine, but post-deserialization mutation breaks contract assumptions.

**Proposal B — true read-only surface:**

- Factory validates inputs (non-empty email, trimmed strings — mirrors **Program.cs** `MemberProfile.Email` setter rules).
- Constructor copies roles into private `List<string>` or array; expose `IReadOnlyList<string>`.
- Callers cannot widen privileges after send — safe for retries, caching, parallel consumers.

**Pragmatic middle ground if staying with init:**

```csharp
public IReadOnlyList<string> Roles { get; init; }

// In factory/constructor:
Roles = roles.ToList().AsReadOnly();
```

**Production takeaway:** C# `init` immutability is shallow — reference-type properties still leak mutable innards. Karat tests whether you design DTOs like **Program.cs** Section 4 — `MemberId` get-only, `RegisteredOn` init-only, `LoginCount` private set — with attention to collection defense. See foundation **Properties** — init vs mutable backing stores.

---

#### Q6. What does `internal` mean in the context of assemblies and `InternalsVisibleTo`? {#07-encapsulation-access-modifiers-q6}

(M) A plugin assembly references your core HR assembly and defines `PayrollProcessor : Employee`. Developers expect to read `InternalCounter` on a base instance from the plugin, but the build fails with CS0122. Explain visibility for `InternalCounter`, `ProtectedInternalCounter`, and `PrivateProtectedCounter` from a derived class in another assembly, and which modifier fits same-assembly first-party plugins.

**Answer:** From a derived class in **another assembly**, only `protected internal` members are accessible on `this` — plain `internal` is assembly-scoped (CS0122 from outside), and `private protected` requires both derivation **and** same assembly (narrowest intersection).

| Member | Modifier | Derived in other assembly |
|---|---|---|
| `InternalCounter` | `internal` | **No** — visible only inside HR assembly; unrelated same-assembly peers can access, but derived plugin code cannot |
| `ProtectedInternalCounter` | `protected internal` (union) | **Yes** — union: accessible if derived **or** same assembly; cross-assembly derivation satisfies the `protected` side |
| `PrivateProtectedCounter` | `private protected` (intersection) | **No** — must be derived **and** same assembly; plugin in `Plugins.Payroll.dll` fails CS0122 |

- **`internal` on base instance from derived code:** Even in the same assembly, `VisibilityDerived.SummarizeFromDerived` in **Program.cs** deliberately omits `InternalCounter` — `internal` is not inherited as a subclass privilege; it is assembly membership. Derived types do not get special access to `internal` members on arbitrary base instances unless they are in the same assembly (and even then, access is through the instance in same assembly — the chapter notes derived code in same asm still follows the visibility table).
- **First-party plugins compiled into the same assembly:** Use `private protected` when the hook must never leak to external extenders — same-assembly subclasses only. Use `protected internal` only when third-party plugins in other assemblies legitimately need the hook (widest combo — use sparingly).
- **Cross-assembly plugin extensibility:** Prefer `protected` methods with controlled behavior over exposing protected fields; keep counters private and expose `protected virtual OnPayrollProcessed()` template hooks.

**Production takeaway:** Combined modifiers are easy to misread — `protected internal` is a **union** (wider), `private protected` is an **intersection** (narrower). Karat embeds the chapter's **Program.cs** Section 2 visibility table in a cross-assembly plugin scenario — the fix is choosing the narrowest modifier that matches your trust boundary, not defaulting to `public`.

---

---

#### Q7. What is the default access level for class members if you omit an modifier? {#07-encapsulation-access-modifiers-q7}

_Answer not found._

---

#### Q8. How do access modifiers apply to nested types vs top-level types? {#07-encapsulation-access-modifiers-q8}

_Answer not found._

---

#### Q9. What is defensive copying when returning collections from properties? {#07-encapsulation-access-modifiers-q9}

_Answer not found._

---

#### Q10. What is the difference between encapsulation and immutability? {#07-encapsulation-access-modifiers-q10}

_Answer not found._

---

#### Q11. Why are public fields discouraged in public APIs even for simple DTOs in some codebases? {#07-encapsulation-access-modifiers-q11}

_Answer not found._

---

#### Q12. How does `private protected` restrict visibility compared to `protected` alone? {#07-encapsulation-access-modifiers-q12}

_Answer not found._

---

#### Q13. What is a friend assembly pattern, and what are its trade-offs? {#07-encapsulation-access-modifiers-q13}

_Answer not found._

---

#### Q14. How do property accessors use asymmetric access (`public get; private set;`)? {#07-encapsulation-access-modifiers-q14}

_Answer not found._

---

### 08. Events

#### Q1. Explain events in C# (including event handling and publisher-subscriber pattern). {#08-events-q1}

(R) A WPF-style desktop app keeps growing in memory after users open and close account detail panels. Review this wiring. What keeps `AccountDetailPanel` instances alive, and how do you fix it?

**Answer:** The panel subscribes to `_account.BalanceChanged` with a lambda but never unsubscribes in `Dispose`, so the long-lived `BankAccount` publisher holds a delegate that captures `this` — the closed panel cannot be collected even after it is removed from the UI.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | `+=` in constructor, no `-=` in `Dispose` | Publisher retains subscriber → memory leak |
| Handler target | Lambda captures `this` (the panel instance) | GC cannot reclaim disposed UI objects |
| Design | Shared singleton/static `BankAccount` outlives every panel | Leak accumulates on each navigation open/close |

**Fix (priority order):**

1. Unsubscribe in `Dispose` (or `IAsyncDisposable`) — store the handler in a field if you used a lambda so `-=` matches the same delegate instance.
2. Prefer a named instance method handler when possible: `_account.BalanceChanged += OnBalanceChanged;` and `-= OnBalanceChanged` in `Dispose`.
3. If the publisher outlives all subscribers, consider weak-event patterns or a mediator (`IMediator`, `Channel<T>`) for UI refresh instead of direct domain events.
4. Profile with a memory dump — look for `AccountDetailPanel` instances retained via `BankAccount` → multicast delegate chain.

```csharp
private readonly EventHandler<BalanceChangedEventArgs> _balanceHandler;

public AccountDetailPanel(BankAccount account)
{
    _account = account;
    _balanceHandler = (_, e) => RefreshBalanceLabel(e.NewBalance);
    _account.BalanceChanged += _balanceHandler;
}

public void Dispose()
{
    _account.BalanceChanged -= _balanceHandler;
}
```

**Production takeaway:** Events create implicit references from publisher to subscriber — Karat tests whether you treat `-=` as mandatory cleanup, not optional. See **Program.cs** Section 6 — subscribe/unsubscribe and **Section 4c** — publisher outlives handlers.

---

#### Q2. What is the difference between an `event` and a plain public delegate field? {#08-events-q2}

(R) After a refactor, balance notifications crash when no UI is subscribed. Review the publisher change:

```csharp
public class BankAccount
{
    public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;

    protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
    {
        if (BalanceChanged != null)
        {
            BalanceChanged(this, e);  // was: BalanceChanged?.Invoke(this, e);
        }
    }
}
```

What breaks at runtime, and what is the idiomatic raise pattern in modern C#?

**Answer:** The null check and invoke are not atomic — another thread can unsubscribe between the `!= null` test and the call, leaving `BalanceChanged` null and throwing `NullReferenceException`. The idiomatic fix is null-conditional invoke: `BalanceChanged?.Invoke(this, e)`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Split null-check + direct invoke | Rare NRE when last handler unsubscribes during raise |
| Style | Verbose `if (BalanceChanged != null)` | Easy to regress during refactor away from `?.` |
| Threading | Non-atomic check-then-invoke | Same race as Q4; worse under concurrent UI/service threads |

**Fix (priority order):**

1. Restore null-conditional invoke inside `OnBalanceChanged`: `BalanceChanged?.Invoke(this, e);`
2. For multi-threaded publishers, copy to a local before invoke (see Q4): `var handler = BalanceChanged; handler?.Invoke(this, e);`
3. Keep raise logic centralized in `OnBalanceChanged` so derived classes override one hook — matches **Program.cs** Section 4c.
4. Add a unit test that unsubscribes a handler from inside another handler — reproduces the race without UI.

**Production takeaway:** Forgetting `?.` is a classic production footgun — zero subscribers is normal, not exceptional. See **Program.cs** QUICK REFERENCE — "Forgetting ?. before Invoke → NullReferenceException."

---

#### Q3. Why should you unsubscribe from events, and what problem does this prevent? {#08-events-q3}

(R) A teammate exposes a notification hook as a public delegate field "for flexibility." Review usage from another assembly:

```csharp
public class PaymentGateway
{
    public Action<string>? PaymentCompleted;  // public field, not event
}

// Consumer startup:
gateway.PaymentCompleted += msg => _audit.Log(msg);

// Later, a test helper "resets" listeners before each test:
gateway.PaymentCompleted = null;

// Malicious or buggy caller in another module:
gateway.PaymentCompleted?.Invoke("Fake payment — ship order");
```

What production risks does this design create compared to `public event Action<string>? PaymentCompleted`?

**Answer:** A public delegate field lets any caller invoke the callback chain or assign `null`, wiping every subscriber without their knowledge — breaking audit trails, tests, and domain integrity. The `event` keyword restricts outsiders to `+=` / `-=` only; only `PaymentGateway` may raise from inside the type (CS0070 blocks external `Invoke`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security / integrity | External `Invoke` fakes domain events | Downstream systems act on spoofed "payment completed" |
| Encapsulation | `= null` clears entire multicast chain | Silent loss of audit/logging handlers after test reset or bug |
| API contract | Callers cannot distinguish publisher vs subscriber responsibilities | Violates publisher/subscriber roles from **Program.cs** Section 1 |
| Compile-time safety | No CS0070 guard on external raise | Fake notifications ship to production undetected |

**Fix (priority order):**

1. Change to `public event Action<string>? PaymentCompleted;` and raise only from an internal `Publish(string message)` method.
2. Replace test `= null` with explicit `-=` per registered handler, or create a fresh gateway instance per test.
3. For cross-assembly extensibility, prefer interfaces + DI (`INotificationPublisher`) over exposed delegate fields.
4. Code-review rule: flag `public Action`/`Func` fields on domain types — require `event` or method-based hooks.

```csharp
public class PaymentGateway
{
    public event Action<string>? PaymentCompleted;

    public void CompletePayment(string receiptId)
    {
        // real gateway work...
        PaymentCompleted?.Invoke(receiptId);
    }
}
```

**Production takeaway:** **Program.cs** Section 5 — `UnsafeNotifier` vs `SafeNotifier` — same lesson at enterprise scale: events protect who may raise and who may clear subscribers.

---

#### Q4. What happens during multicast delegate invocation if one subscriber throws? {#08-events-q4}

(P) A background `BankAccount` service raises `BalanceChanged` from worker threads while the UI thread subscribes handlers. A developer uses only null-conditional invoke inside `OnBalanceChanged`:

```csharp
protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
{
    BalanceChanged?.Invoke(this, e);
}
```

Under concurrent subscribe/unsubscribe, handlers are occasionally skipped or you see rare `NullReferenceException` in older .NET code paths. Explain the race and show the thread-safe raise pattern from this chapter.

**Answer:** `BalanceChanged?.Invoke` still reads the event field twice conceptually — between load and invoke another thread can `-=` the last handler and set the backing delegate to null, so some handlers never run or an older pattern throws. Copy the delegate reference to a local variable, then null-conditional invoke the copy so the invocation list is fixed for that raise.

- **Race:** Thread A loads non-null delegate → Thread B unsubscribes last handler (field becomes null) → Thread A invokes — skipped notification or NRE with explicit null-check code.
- **Thread-safe pattern (from this chapter):**

```csharp
protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
{
    EventHandler<BalanceChangedEventArgs>? handler = BalanceChanged;
    handler?.Invoke(this, e);
}
```

- **Why it works:** The local `handler` captures the multicast delegate snapshot at raise time; subsequent `+=`/`-=` on the event do not affect that snapshot.
- **Stronger option:** Custom `add`/`remove` accessors with a lock if subscribe/unsubscribe must be synchronized with raise — **Program.cs** Section 4d; default compiler accessors are usually enough once you copy locally.
- **UI note:** Even with a safe raise, handlers that touch UI controls must marshal to the UI thread (`Dispatcher`, `SynchronizationContext`) — thread-safe raise does not make handler bodies thread-safe.

**Production takeaway:** Null-conditional invoke fixes "no subscribers"; local copy fixes "subscribers changed mid-raise" — Karat stacks both. See **BankAccount.OnBalanceChanged** in **Program.cs** lines 215–219.

---

#### Q5. What is the standard `EventHandler` / `EventHandler<TEventArgs>` pattern? {#08-events-q5}

(P) An ASP.NET Core API registers a **Singleton** `OrderStateTracker` that exposes `event EventHandler<OrderPlacedEventArgs>? OrderPlaced`. Scoped services subscribe in their constructors to push SignalR updates. After a few thousand requests, memory climbs and old connections still receive events. What is wrong with this wiring, and what pattern replaces in-process events for web apps?

**Answer:** A singleton publisher lives for the app lifetime, but each scoped `OrderNotificationService` subscribes in its constructor and never unsubscribes — every request adds another handler to the same event, retaining disposed scopes, `IHubContext` captures, and stale SignalR targets until the process recycles.

- **DI lifetime mismatch:** Singleton event source + scoped subscriber constructor subscription = unbounded handler list growth per HTTP request.
- **Memory:** Each handler closes over `hub` and possibly request state — GC cannot collect completed requests still referenced by the delegate chain.
- **Correctness:** Old handlers fire on new orders — clients see duplicate or ghost notifications from recycled connection ids.

**Fix (priority order):**

1. **Do not** subscribe in scoped service constructors to singleton events without matching `-=` in `Dispose`/`IAsyncDisposable` — hard to get right in ASP.NET.
2. Prefer **`IOptions` + `IHostedService`**, a **singleton** broadcaster with explicit connection mapping, or **`IHubContext` injected into a singleton** that tracks groups — not per-request event handlers.
3. For domain decoupling in ASP.NET Core, use **`IMediator` (MediatR)**, **`Channel<T>`**, or **message bus** (Azure Service Bus, RabbitMQ) scoped to the unit of work — not classic C# events across DI lifetimes.
4. If events are required (e.g., `DbContext.SaveChanges` interceptors), keep subscriber lifetime **≤ publisher lifetime** and unsubscribe when scope ends.

```csharp
// Better: scoped handler invoked explicitly from application service, no singleton event
public sealed class OrderApplicationService
{
    private readonly IHubContext<OrderHub> _hub;
    public async Task PlaceOrderAsync(Order order, CancellationToken ct)
    {
        // persist order...
        await _hub.Clients.Group(order.CustomerId).SendAsync("orderPlaced", order.Id, ct);
    }
}
```

**Production takeaway:** C# events assume you manage lifetimes manually — ASP.NET DI scopes do not auto-unsubscribe. Karat links **Events** to **DI lifetimes**: singleton + scoped event wiring is a production leak. Preview: **Program.cs** Section 6 — multi-handler wiring moves to ch.09 with service registration.

---

#### Q6. How do you raise an event safely (null-check, `?.Invoke`, local copy pattern)? {#08-events-q6}

(D) Your team debates three ways to notify downstream code when `BankAccount` balance changes: (A) `public event EventHandler<T>`, (B) `public Action<T>?` callback field, (C) `INotificationService` injected and called directly from `Deposit`/`TryWithdraw`. When would you choose each in a production ASP.NET Core domain layer, and what is the unsubscribe/lifetime rule of thumb?

**Answer:** In ASP.NET Core domain services, prefer **(C) injected abstractions** for application boundaries; use **(A) events** for in-process, same-lifetime object graphs (UI controls, short-lived aggregates with explicit cleanup); avoid **(B) public delegate fields** in production domain code except internal test doubles.

| Option | When to use | Lifetime rule |
|---|---|---|
| **(A) `event`** | Same-assembly domain objects, UI binding, aggregates where subscribers share publisher lifetime | Every `+=` needs matching `-=` when subscriber dies first; publisher must outlive or use weak patterns |
| **(B) `Action` field** | Rare — single callback slot, prototype code, serializer-friendly delegates you control entirely | Same as (A), plus anyone can `= null` or invoke — not for public APIs |
| **(C) `INotificationService` / MediatR** | ASP.NET Core services, cross-layer notifications, testability, multiple implementations | DI scope owns lifetime — no manual unsubscribe; singleton must not capture scoped services |

**Production guidance:**

- **Domain layer in API:** `BankAccount` should not expose public events to the web stack — call `INotificationService.PublishBalanceChanged(...)` from application services after persistence so lifetimes follow the request scope.
- **Console/UI tools:** Events match **Program.cs** tutorial — `BankAccount` + handlers in `Main` with clear subscribe/unsubscribe demo.
- **Testing:** (C) is easiest to mock; (A) requires raising events or attaching test handlers with cleanup; (B) invites test code that clears production handlers with `= null`.
- **Rule of thumb:** If the subscriber has a **shorter lifetime than the publisher**, you must unsubscribe — or do not use events. If lifetimes are managed by DI, use interfaces instead of events.

**Production takeaway:** Events excel at decoupling within one process and one lifetime story; ASP.NET Core's scoped/singleton graph breaks that assumption — Karat expects you to pick the mechanism by **who raises, who listens, and who outlives whom**, not syntax preference alone.

---

#### Q7. What is the difference between custom delegate types and `EventHandler` for events? {#08-events-q7}

_Answer not found._

---

#### Q8. Can interfaces declare events, and how are they implemented? {#08-events-q8}

_Answer not found._

---

#### Q9. What memory-leak scenario arises when a long-lived publisher holds references to short-lived subscribers? {#08-events-q9}

_Answer not found._

---

#### Q10. What is the difference between events and the Observer pattern / IObservable? {#08-events-q10}

_Answer not found._

---

#### Q11. Can you assign to an event from outside the declaring class (`event += handler` vs `event = handler`)? {#08-events-q11}

_Answer not found._

---

#### Q12. What is thread-safe event raising, and when is locking required? {#08-events-q12}

_Answer not found._

---

### 09. OOP Real-World Examples

#### Q1. Explain the SOLID principles with concrete C# examples. {#09-oop-real-world-examples-q1}

(R) A team ports the chapter's order-fulfillment payment flow into a service class. Support sees duplicate debits and failed rollbacks after card declines. Review:

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

What is wrong across encapsulation, abstraction, and correctness — and how would you fix it in priority order?

**Answer:** The service debits the wallet before any gateway succeeds, then infers payment outcome from a formatted string and silently retries a second gateway — so a declined card can still leave the customer charged twice or in an inconsistent ledger state. It also hard-codes concrete processors instead of depending on the chapter's `PaymentProcessor` abstraction.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Withdraw **before** confirmed charge; fallback charges a **second** gateway after partial success | Duplicate debits, reconciliation nightmares, support tickets |
| Encapsulation | Ignores `TryWithdraw` result (`out _` discarded); balance check + withdraw not atomic with payment | Race conditions; withdraw can fail while flow continues |
| Abstraction / DIP | `new CardPaymentProcessor()` / `new WalletPaymentProcessor()` inside method | Cannot swap gateways, mock in tests, or extend without editing this class (OCP) |
| Design | Parses `"declined"` from human-readable `ProcessOrderPayment` string | Fragile coupling to message text; breaks localization or logging changes |
| Domain | No idempotency on `orderRef` | Retries double-charge the same order |

**Fix (priority order):**

1. **Stop debiting before payment succeeds** — call `PaymentProcessor.TryCharge` (or gateway API) first; only `TryWithdraw` / ledger debit after confirmed charge, inside one transactional boundary (DB transaction or saga with compensating action).
2. **Inject `PaymentProcessor` (or strategy per payment method)** — caller or factory selects one processor per order; do not sequentially hammer two gateways on one decline string match.
3. **Use structured results** — return `bool` / result type from charge APIs, not `Contains("declined")` on formatted strings.
4. **Respect `TryWithdraw` outcome** and frozen-account rules from chapter `BankAccount` — propagate `errorMessage`; never ignore `out` parameters.
5. Add **idempotency key** on `orderRef` so retries are safe.

```csharp
public sealed class OrderPaymentService
{
    private readonly PaymentProcessor _processor;

    public OrderPaymentService(PaymentProcessor processor) => _processor = processor;

    public bool Run(BankAccount wallet, decimal total, string orderRef, out string message)
    {
        if (!_processor.TryCharge(total, orderRef))
        {
            message = _processor.ProcessOrderPayment(total, orderRef);
            return false;
        }

        if (!wallet.TryWithdraw(total, out message))
        {
            // Compensating refund/charge reversal on gateway
            return false;
        }

        message = _processor.ProcessOrderPayment(total, orderRef);
        return true;
    }
}
```

**Production takeaway:** The chapter separates **encapsulated ledger rules** (`BankAccount`) from **hidden gateway logic** (`PaymentProcessor`) — Karat stacks them to see if you preserve invariants when wiring a "real" service. See **Program.cs** Sections 2–3 — `TryWithdraw` + `ProcessOrderPayment`.

---

#### Q2. What is the Liskov Substitution Principle? Give a classic violation (e.g., `Square`/`Rectangle`). {#09-oop-real-world-examples-q2}

(R) A logistics API quotes delivery cost from the chapter's `Vehicle` fleet. After adding `Motorcycle` to the fleet, quotes are wrong and every new vehicle type requires editing this method. Review:

```csharp
public static decimal QuoteDelivery(Vehicle vehicle, decimal distanceKm, decimal ratePerKm)
{
    if (vehicle is Car)
        return distanceKm * ratePerKm;

    if (vehicle is Truck truck)
        return distanceKm * ratePerKm * (1.0m + truck.PayloadTons * 0.05m);

    // Fallback for anything else (Motorcycle, future types)
    return distanceKm * ratePerKm * 2.0m;
}
```

What design problems do you see, and how does the chapter's polymorphism model replace this?

**Answer:** The method re-implements pricing with type tests and a punitive default multiplier, so `Motorcycle` quotes are wrong and every new `Vehicle` subtype forces another branch — exactly what polymorphic `EstimateDeliveryCostKm` on the chapter's hierarchy avoids.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Polymorphism | Ignores `Vehicle.EstimateDeliveryCostKm` override on `Truck` | Duplicated / divergent pricing logic; truck payload formula may drift from domain |
| OCP | Central `is` / `if` chain | New vehicle types require editing shared utility — merge conflicts, missed cases |
| LSP / correctness | `2.0m` fallback for unknown types | Motorcycles overcharged; silent wrong quotes in production |
| Maintainability | `Car` branch duplicates base `ratePerKm * 1.0m` | Two places to change base rate logic |

**Fix (priority order):**

1. Replace the method body with **`vehicle.EstimateDeliveryCostKm(ratePerKm) * distanceKm`** — one line using runtime dispatch.
2. Override `EstimateDeliveryCostKm` on subtypes that differ (`Truck` already does); leave `Car` / `Motorcycle` on base behavior or add precise overrides.
3. Delete the fallback multiplier — if a new type needs special pricing, add a derived class override instead of editing a god-method.
4. Accept `Vehicle` (or `IReadOnlyList<Vehicle>`) in fleet APIs so callers never downcast for pricing.

**Production takeaway:** Chapter Section 4–5 shows **virtual override + base reference** so fleet loops stay branch-free — Karat uses logistics quoting to test whether you reach for `is` checks after learning polymorphism. See **Program.cs** — `deliveryVehicle.EstimateDeliveryCostKm(ratePerKm)`.

---

#### Q3. What is Dependency Inversion, and how does constructor injection implement it? {#09-oop-real-world-examples-q3}

(R) A PR consolidates payment, delivery, labels, notifications, and invoicing into one coordinator for "simplicity." Review:

```csharp
public sealed class OrderFulfillmentHub
{
    public BankAccount CustomerWallet { get; set; } = new("ACC-DEFAULT", 0m);

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

Identify stacked OOP and SOLID issues. What would you split, inject, or abstract first?

**Answer:** One class owns mutable shared wallet state, hard-coded collaborators, duplicated shape math, and a string-concatenated API response — violating SRP and DIP while bypassing the chapter's interface and polymorphism seams.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| SRP | Payment, delivery, labeling, notify, invoice in one method | Untestable blob; any change risks regressions everywhere |
| Encapsulation | Public `CustomerWallet` setter + default account | Any caller can swap or corrupt shared wallet; multi-tenant bleed |
| DIP / abstraction | `new CardPaymentProcessor`, `new EmailNotificationSender`, inline `Circle` math | No injection; cannot add SMS/Push or swap truck without editing hub |
| Polymorphism | Recomputes circle area instead of `circle.Area` / `Shape.Draw()` | Duplicated domain logic; breaks when label rules change |
| Correctness | Withdraw + charge ordering (same as Q1) | Financial inconsistency |
| API design | Returns opaque concatenated string | Callers cannot compose invoice PDF, audit log, or HTTP 201 body cleanly |

**Fix (priority order):**

1. **Extract orchestrator** that accepts dependencies — `PaymentProcessor`, `Vehicle` (or fleet service), `IReadOnlyList<Shape>`, `IEnumerable<INotificationSender>`, `Document` factory — constructor injection.
2. **Remove mutable shared `BankAccount` property** — pass per-order wallet/account id into `Fulfill`; load scoped instance per request.
3. **Use chapter contracts** — `NotifyCustomer(senders, …)` pattern from **Program.cs**; `RenderLabels` / `SumAreas` for shapes; `invoice.Render()` as sole document output, map to DTO separately.
4. Split **domain services** — `OrderPaymentService`, `DeliveryQuoteService`, `NotificationService` — orchestrator coordinates; each unit-tested.
5. Return a **structured result** (payment status, delivery cost, notification receipts, invoice text) — not one mega-string.

**Production takeaway:** The chapter's `Main` intentionally orchestrates for learning — production code inverts that into injected abstractions. Karat capstone tests whether you recognize demo-style composition vs shippable boundaries.

---

#### Q4. What is the difference between Dependency Injection and the Service Locator pattern? {#09-oop-real-world-examples-q4}

(D) Product wants **push notifications** and a shared **retry-with-backoff** helper for all channels. Two proposals land in code review:

**Option A — extend abstract base:**

```csharp
public abstract class NotificationSenderBase
{
    protected void Retry(Action sendAttempt) { /* shared retry */ }
    public abstract string Send(string recipient, string message);
}

public class PushNotificationSender : NotificationSenderBase { /* ... */ }
```

**Option B — keep chapter interface + optional helper:**

```csharp
public interface INotificationSender
{
    string ChannelName { get; }
    string Send(string recipient, string message);
}

public static class NotificationRetry
{
    public static string SendWithRetry(INotificationSender sender, string recipient, string message) { /* ... */ }
}
```

Email and SMS already implement `INotificationSender` with no common base. Which direction fits this chapter's fulfillment model, and when would you combine both?

**Answer:** Prefer **Option B** — keep `INotificationSender` and add `PushNotificationSender : INotificationSender`, with retry as a cross-cutting helper or decorator — because email and SMS are unrelated types united only by a contract, matching Section 6. Introduce an abstract base only when several channels share substantial state or template steps, not for one shared utility method.

- **Why not Option A alone:** Forcing `EmailNotificationSender` and `SmsNotificationSender` onto a new base class reshapes existing types, introduces fragile inheritance where a interface sufficed, and violates **ISP** if the base accumulates channel-specific hooks (push tokens, SMS truncation).
- **Option B alignment:** Chapter `NotifyCustomer` already loops `INotificationSender[]` — push slots in without changing orchestration; retry wraps any sender.
- **When to combine both:** If push and SMS later share **significant** infrastructure (shared rate limiter state, correlation id field, template rendering), extract a small `NotificationSenderBase` **in addition to** the interface for those two — or use a **decorator** `RetryingNotificationSender : INotificationSender` that wraps any implementer.
- **Events vs direct Send:** Audit/logging can stay on `OrderFulfillmentCoordinator.OrderCompleted` (Section 9 preview) — do not push audit into the notification hierarchy.
- **Testing:** Interface + decorator/helper lets you mock `INotificationSender` and assert retry policy independently.

**Production takeaway:** Chapter rule — **interface when unrelated types share a capability; abstract class when subtypes share fields + template logic** (`Document` vs `INotificationSender`). Karat asks you to apply that rule under feature pressure, not pick inheritance by default.

---

#### Q5. What is the difference between "has-a" and "is-a" relationships? When is inheritance the wrong choice? {#09-oop-real-world-examples-q5}

(P) An ASP.NET Core team registers the chapter's fulfillment types in `Program.cs` for a checkout API:

```csharp
builder.Services.AddSingleton<BankAccount>();
builder.Services.AddSingleton<OrderFulfillmentCoordinator>();
builder.Services.AddTransient<CardPaymentProcessor>();
builder.Services.AddTransient<PaymentProcessor>(sp => sp.GetRequiredService<CardPaymentProcessor>());
builder.Services.AddSingleton<INotificationSender, EmailNotificationSender>();
```

Under concurrent requests, balances mix between customers and notification behavior looks "sticky." Explain what breaks at the DI lifetime layer and how you would register these abstractions for production.

**Answer:** `BankAccount` and a single `INotificationSender` registered as **singletons** share one instance for all HTTP requests, so every customer's checkout mutates the same balance and notification channel — a functional bug that only appears under concurrent load.

- **`BankAccount` singleton:** Domain objects with mutable balance must be **scoped per request** (or loaded per customer from persistence), never singleton — same rule as cart state in web apps. Opening an account belongs in a repository + scoped unit of work, not a shared DI instance.
- **`INotificationSender` singleton:** If the implementer holds per-send state, connection, or throttling counters, those leak across users. Prefer **transient** senders or **stateless singleton** that only wraps an `HttpClient` from `IHttpClientFactory`.
- **`OrderFulfillmentCoordinator` singleton:** Acceptable only if it is **stateless** and raises events without storing subscriber lists incorrectly — but event handlers that capture scoped services from singleton are a captive dependency smell; usually register coordinator **scoped**.
- **`PaymentProcessor` transient mapping:** Fine for stateless gateways; register **multiple implementations** via factory or keyed services (`IPaymentProcessorFactory`) when checkout picks card vs wallet per order — not a single `PaymentProcessor` → card binding.
- **Production pattern:** Scoped `OrderFulfillmentService` orchestrator; transient/scoped processors; `IEnumerable<INotificationSender>` or separate sends via factory; **never** singleton mutable domain entities.

```csharp
builder.Services.AddScoped<OrderFulfillmentCoordinator>();
builder.Services.AddTransient<CardPaymentProcessor>();
builder.Services.AddTransient<WalletPaymentProcessor>();
builder.Services.AddTransient<INotificationSender, EmailNotificationSender>();
builder.Services.AddTransient<INotificationSender, SmsNotificationSender>();
// BankAccount: resolve from scoped service using customer id — not AddSingleton<BankAccount>()
```

**Production takeaway:** Chapter types teach OOP shape; ASP.NET DI teaches **which instance lives how long**. Karat capstone connects `BankAccount` encapsulation to **scoped vs singleton** — see foundation DI lifetime gotchas when moving console demo to API.

---

#### Q6. What is the anemic domain model anti-pattern? {#09-oop-real-world-examples-q6}

(R) A developer splits `BankAccount` into partial files (as in this chapter) but adds a "fast path" for internal ops. Frozen accounts still accept money in staging. Review both fragments:

```csharp
// BankAccount.Core.cs
public partial class BankAccount
{
    private decimal _balance;

    public decimal Balance => _balance;

    public void Deposit(decimal amount)
    {
        if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount));
        _balance += amount;
    }
}

// BankAccount.Ops.cs
public partial class BankAccount
{
    public bool IsActive { get; set; } = true;

    public void CreditOpsAdjustment(decimal amount)
    {
        // Skips ValidateForTransaction — ops-only
        _balance += amount;
    }

    private void ValidateForTransaction()
    {
        if (!IsActive) throw new InvalidOperationException("Account is frozen.");
    }
}
```

`TryWithdraw` still calls `ValidateForTransaction`, but `Deposit` no longer does. What failed across encapsulation and invariants, and how do you fix it?

**Answer:** Partial classes merge into one type, but splitting files does not split invariants — `Deposit` and `CreditOpsAdjustment` now mutate `_balance` without the freeze check, while `TryWithdraw` still enforces it, so callers can credit frozen accounts and `IsActive` is publicly settable, breaking encapsulation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encapsulation | `IsActive` public setter | Any consumer can unfreeze/freeze accounts; bypasses `Freeze()` intent from chapter |
| Invariant | `Deposit` dropped `ValidateForTransaction()` | Frozen accounts accept deposits — staging bug matches production fraud/ops risk |
| Design | `CreditOpsAdjustment` writes `_balance` directly | Second mutation path; ops and customer deposits diverge in rules |
| partial class misuse | Team assumed file boundary = security boundary | Partial only splits compilation units, not access control |

**Fix (priority order):**

1. Restore **`ValidateForTransaction()` at the start of every public mutator** — `Deposit`, and any ops path that should respect freeze (or explicitly document and gate ops behind internal/admin API).
2. Change **`IsActive` to `{ get; private set; }`** — only `Freeze()` (and controlled `Reactivate()` if needed) mutate lifecycle.
3. Route **all balance changes** through private helpers, e.g. `ApplyCredit(decimal amount, bool bypassFreeze = false)` used only from trusted internal assembly with `InternalsVisibleTo` — not a public `CreditOpsAdjustment`.
4. Add tests: deposit/withdraw on frozen account must fail consistently across partial files.

```csharp
public void Deposit(decimal amount)
{
    ValidateForTransaction();
    if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount));
    _balance += amount;
}

public bool IsActive { get; private set; } = true;
```

**Production takeaway:** Chapter Section 2c–2d uses **partial** for team file layout — Karat checks you know both fragments share one invariant surface. See **Program.cs** — `TryDepositOnFrozenAccount` after `Freeze()`.

---

#### Q7. What is the Open/Closed Principle, and how do interfaces support extension without modification? {#09-oop-real-world-examples-q7}

(D) You inherit a monolithic fulfillment codebase that mirrors this chapter's demo `Main` — one method creates every object, mutates wallet state, picks a truck by array index, renders shapes, sends notifications, and prints the invoice. The team has one sprint to improve production readiness without a full rewrite.

What refactor order would you choose (encapsulation fixes, introduce interfaces, extract services, events/DI), and what would you **defer**? Tie your answer to the chapter's types (`BankAccount`, `PaymentProcessor`, `Vehicle`, `Shape`, `INotificationSender`, `Document`, `OrderFulfillmentCoordinator`).

**Answer:** First stop financial and state corruption (wallet + payment ordering + singleton/scoped mistakes), then introduce constructor-injected abstractions for payment and notifications, then extract read-only polymorphic helpers for fleet/shapes/documents — defer full event-driven architecture and extension-method polish until core seams are testable.

**Sprint 1 priority (do now):**

1. **Encapsulation / correctness (`BankAccount`, payment flow):** Ensure all debits go through `TryWithdraw`; fix withdraw-before-charge ordering; no public wallet mutation; per-customer account resolution — highest business risk.
2. **DIP entry points (`PaymentProcessor`, `INotificationSender`):** Extract an `OrderFulfillmentService` that accepts `PaymentProcessor` + `IEnumerable<INotificationSender>` — mirrors chapter `NotifyCustomer` and `ProcessOrderPayment` without rewriting domain types.
3. **Polymorphism cleanup (`Vehicle`, `Shape`):** Replace index/`is` checks with `EstimateDeliveryCostKm` and `Shape.Area`/`Draw()` helpers already in **Program.cs** — low risk, high clarity win.
4. **Document output (`Document`):** Keep `Render()` template method; return invoice string from service, not `Console.WriteLine` in orchestrator — enables API responses.
5. **DI lifetimes (when moving to ASP.NET):** Scoped orchestrator; never singleton `BankAccount`.

**Defer (explicitly):**

- **Full event-driven redesign** (`OrderFulfillmentCoordinator` audit via events) until core flow is unit-tested — events are valuable but add indirection early.
- **New subtypes** (extra shapes, vehicle types) — OCP is already satisfied once polymorphic calls exist.
- **Extension methods** (`ToDisplayLabel`) — cosmetic; no production risk.
- **Partial class splits** — organizational only; no runtime benefit until team scale demands it.
- **Sealed/further inheritance tuning** on `Motorcycle` — design hygiene, not sprint-critical.

**Production takeaway:** Capstone chapter integrates pillars in one narrative — Karat asks for **prioritized** hardening: protect invariants first, inject swappable collaborators second, unify polymorphic dispatch third, polish decoupling (events) last. That mirrors how you would evolve the chapter demo `Main` into a shippable checkout pipeline without a big-bang rewrite.

---

#### Q8. What is the Single Responsibility Principle — how do you recognize a class that violates it? {#09-oop-real-world-examples-q8}

_Answer not found._

---

#### Q9. What is the Interface Segregation Principle — why are fat interfaces problematic? {#09-oop-real-world-examples-q9}

_Answer not found._

---

#### Q10. What is a factory method vs a simple constructor — when do you introduce a factory? {#09-oop-real-world-examples-q10}

_Answer not found._

---

#### Q11. What is the Strategy pattern, and how does it map to interfaces/delegates in C#? {#09-oop-real-world-examples-q11}

_Answer not found._

---

#### Q12. What is the Repository pattern at a high level, and why depend on abstractions? {#09-oop-real-world-examples-q12}

_Answer not found._

---

#### Q13. How does polymorphism simplify replacing implementations in tests (mock/stub scenarios)? {#09-oop-real-world-examples-q13}

_Answer not found._

---

#### Q14. What is the difference between domain modeling with rich behavior vs CRUD-style service objects? {#09-oop-real-world-examples-q14}

_Answer not found._

---

#### Q15. **Virtual method from base constructor** — Calling an overridden virtual method from a base constructor runs before derived field initializers complete; overridden code sees default values. {#09-oop-real-world-examples-q15}

_Answer not found._

---

#### Q16. **Method hiding vs overriding** — `new` hides by compile-time type; `override` dispatches by runtime type. Mixing them breaks expected polymorphism. {#09-oop-real-world-examples-q16}

_Answer not found._

---

#### Q17. **`Equals()` without `GetHashCode()`** — Breaks the hash contract; objects can exist in a `Dictionary`/`HashSet` but not be found again after mutation. {#09-oop-real-world-examples-q17}

_Answer not found._

---

#### Q18. **Mutable object as dictionary key** — Changing a key after insertion causes "lost" entries at runtime. {#09-oop-real-world-examples-q18}

_Answer not found._

---

#### Q19. **Struct boxing via interface** — Assigning a struct to an interface type boxes; subsequent struct mutations don't affect the boxed copy. {#09-oop-real-world-examples-q19}

_Answer not found._

---

#### Q20. **`protected internal` vs `private protected`** — `protected internal` = protected OR internal; `private protected` = protected AND internal (same assembly only). {#09-oop-real-world-examples-q20}

_Answer not found._

---

#### Q21. **Type-checking anti-pattern** — Long `if (animal is Dog)` chains defeat polymorphism; prefer virtual methods or pattern matching on a common abstraction. {#09-oop-real-world-examples-q21}

_Answer not found._

---

#### Q22. **Memory leaks despite GC** — Event handlers and static caches holding references to short-lived objects are the classic managed leak. {#09-oop-real-world-examples-q22}

_Answer not found._

---

#### Q23. **Exposing `List<T>` directly** — Callers can mutate internal state without invariant checks; return `IReadOnlyList<T>` or defensive copies. {#09-oop-real-world-examples-q23}

_Answer not found._

---

#### Q24. **`init` after construction** — Init-only properties can be set in object initializers and constructors but not arbitrary code afterward; confusing with `{ get; private set; }`. {#09-oop-real-world-examples-q24}

_Answer not found._

---

#### Q25. **Static "singleton" vs DI singleton** — A static class is hard to test and replace; instance singletons registered in DI are still mockable if designed carefully. {#09-oop-real-world-examples-q25}

_Answer not found._

---

#### Q26. **Explicit interface hiding** — Public class method and explicit interface method can coexist with different behavior; callers must know which API they use. {#09-oop-real-world-examples-q26}

_Answer not found._

---

#### Q27. **Finalizer timing** — `~ClassName()` runs non-deterministically; do not rely on it for timely resource release — use `Dispose`. {#09-oop-real-world-examples-q27}

_Answer not found._

---

#### Q28. **Overriding `==` without consistent `Equals`/`GetHashCode`** — Custom equality operators that disagree with `Equals` break collections and LINQ. {#09-oop-real-world-examples-q28}

_Answer not found._

---

#### Q29. **Default interface methods on structs** — Calling a default interface method on a struct may box the struct depending on how it is invoked. {#09-oop-real-world-examples-q29}

_Answer not found._

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A loan portal caches `Customer` instances in memory between requests. After one user edits a profile, another user sees the same name and loan amount. Review:

```csharp
public class CustomerCache
{
    private readonly Dictionary<int, Customer> _cache = new();

    public Customer GetOrCreate(int id)
    {
        if (!_cache.ContainsKey(id))
            _cache[id] = new Customer(); // Id assigned by parameterless ctor
        return _cache[id];
    }

    public void UpdateLoan(int id, Customer updated)
    {
        var existing = GetOrCreate(id);
        existing.Name = updated.Name;
        existing.LoanAmount = updated.LoanAmount;
        existing.RateOfInterest = updated.RateOfInterest;
        existing.DurationOfLoan = updated.DurationOfLoan;
    }
}
```

```csharp
// Request A
var draft = cache.GetOrCreate(7);
draft.Name = "Meera Shah";
draft.LoanAmount = 250_000;

// Request B — same id, minutes later
var profile = cache.GetOrCreate(7);
Console.WriteLine(profile.Name); // prints Meera Shah
```

What is wrong with how objects are shared, and how would you fix it?

---

**Answer:**

```csharp
public class CustomerCache
{
    private readonly Dictionary<int, Customer> _cache = new();

    public Customer GetOrCreate(int id)
    {
        if (!_cache.ContainsKey(id))
            _cache[id] = new Customer(); // Id assigned by parameterless ctor
        return _cache[id];
    }

    public void UpdateLoan(int id, Customer updated)
    {
        var existing = GetOrCreate(id);
        existing.Name = updated.Name;
        existing.LoanAmount = updated.LoanAmount;
        existing.RateOfInterest = updated.RateOfInterest;
        existing.DurationOfLoan = updated.DurationOfLoan;
    }
}
```

```csharp
// Request A
var draft = cache.GetOrCreate(7);
draft.Name = "Meera Shah";
draft.LoanAmount = 250_000;

// Request B — same id, minutes later
var profile = cache.GetOrCreate(7);
Console.WriteLine(profile.Name); // prints Meera Shah
```

What is wrong with how objects are shared, and how would you fix it?

**Answer:** `Customer` is a reference type — `GetOrCreate` returns the same heap instance for a given id, and mutating fields through one variable changes the single shared object every caller sees. The cache conflates **identity** (one live object per id) with **session draft state** that should be isolated per request.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Reference semantics | `_cache[id]` stores one `Customer` reference; all callers mutate the same instance | Cross-request data bleed — user B sees user A's in-progress edits |
| Design | In-memory singleton cache of mutable domain objects without copy-on-read/write | Violates tenant/session isolation; hard to reason about in multi-user apps |
| Lifetime | `Customer` uses mutable public fields (chapter style) | Any holder of the reference can change state — no encapsulation boundary |
| Correctness | `GetOrCreate` assigns new `Customer()` but key is `id` while `Customer.Id` comes from static `CustomerCount` | Id/key mismatch risk if cache key ≠ `Customer.Id` |

**Fix (priority order):**

1. **Do not cache mutable domain entities** as shared writeable graphs — cache immutable DTOs/snapshots, or store ids and load fresh per request from a database.
2. If caching is required, return **copies** on read (`MemberwiseClone` only as a stopgap; prefer explicit DTO mapping) and treat cache entries as read-only.
3. Replace public fields with properties and encapsulate updates behind methods that validate invariants (see chapter **02. Properties and Indexers**).
4. Scope draft state to the **request** (scoped DI service), not a process-wide dictionary keyed by user id without version checks.
5. Use `TryGetValue` instead of `ContainsKey` + indexer for clarity and single lookup.

**Production takeaway:** Reference assignment copies the pointer, not the object — the chapter's `enrolled = student1` demo is intentional; in production, shared mutable caches cause the same surprise at scale. See **Program.cs** Section 5 — reference semantics.

---

---

#### Q2. (R) A student lookup API throws `NullReferenceException` in production when a roll number is missing. Review the service:

```csharp
public Student? FindByRoll(int rollNumber, List<Student> roster)
{
    return roster.FirstOrDefault(s => s.RollNumber == rollNumber);
}

public string BuildReportLine(int rollNumber, List<Student> roster)
{
    Student student = FindByRoll(rollNumber, roster);
    return $"{student.StudentName} — Roll {student.RollNumber}, Age {student.Age}";
}
```

The domain model uses public fields (as in this chapter's `Student` class). What breaks, and what would you change?

---

**Answer:**

```csharp
public Student? FindByRoll(int rollNumber, List<Student> roster)
{
    return roster.FirstOrDefault(s => s.RollNumber == rollNumber);
}

public string BuildReportLine(int rollNumber, List<Student> roster)
{
    Student student = FindByRoll(rollNumber, roster);
    return $"{student.StudentName} — Roll {student.RollNumber}, Age {student.Age}";
}
```

The domain model uses public fields (as in this chapter's `Student` class). What breaks, and what would you change?

**Answer:** `FindByRoll` correctly returns `null` when no match exists, but `BuildReportLine` assigns that result to a non-nullable `Student` and dereferences fields — producing `NullReferenceException` instead of a controlled "not found" response.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Null reference | Missing guard after `FirstOrDefault` | Runtime crash on unknown roll number |
| Nullable flow | Return type `Student?` but consumer treats as always present | Compiler warnings ignored; NRE in prod |
| API contract | No distinction between "invalid input" and "missing entity" | Callers cannot return 404/problem details |
| Domain model | Public fields allow `StudentName` to remain unset/null despite ctor defaults | Weaker invariants when objects constructed outside parameterized ctor paths |

**Fix (priority order):**

1. Guard before dereference: `if (student is null) return "Unknown roll"…` or throw `KeyNotFoundException` / return `Result<string>` — match API layer (404 + `ProblemDetails`).
2. Annotate honestly: `Student? student = FindByRoll(...)` and enable nullable reference types project-wide (`<Nullable>enable</Nullable>`).
3. Prefer **factory/constructor paths** that establish required fields (`StudentName`, `RollNumber`) so valid instances cannot be half-initialized.
4. Move reporting to a method that accepts `Student` only after null check, or use null-conditional: `student?.StudentName ?? "(unknown)"` for display-only paths.
5. Long term: replace public fields with properties and validation (chapter **07. Encapsulation**).

**Production takeaway:** Nullable reference types express intent — `Student?` means "may be absent"; production services must branch before field access. See **Program.cs** Section 6 — null references and `?.` / `??`.

---

---

#### Q3. (R) After `Student` gained only a parameterized constructor (`Student(string studentName, int rollNumber)`), a teammate adds a factory method. `dotnet build` fails. Review:

```csharp
public static Student CreateFromImport(ImportRow row)
{
    return new Student
    {
        StudentName = row.Name,
        RollNumber = row.Roll,
        Percentage = row.Score,
        Address = row.Address ?? string.Empty
    };
}
```

What conflicted with the class design, and how do you fix it without weakening invariants?

---

**Answer:**

```csharp
public static Student CreateFromImport(ImportRow row)
{
    return new Student
    {
        StudentName = row.Name,
        RollNumber = row.Roll,
        Percentage = row.Score,
        Address = row.Address ?? string.Empty
    };
}
```

What conflicted with the class design, and how do you fix it without weakening invariants?

**Answer:** Object initializer syntax requires a **parameterless constructor** (or an accessible ctor chain). Once `Student(string, int)` was added, the compiler stopped synthesizing a default ctor — so `new Student { … }` does not compile (CS7036 / no accessible parameterless constructor).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Object initializer without parameterless ctor | Build blocked after ctor change |
| Invariants | Initializer sets `RollNumber`/`StudentName` **after** construction — bypasses ctor validation | Duplicate initialization paths; null names possible if ctor rules added later |
| Design | Two construction stories (ctor vs initializer) for the same type | Team confusion about which fields are required at birth |
| Data integrity | `Percentage`/`Address` set outside ctor while identity fields expected in ctor | Import rows can create inconsistent students |

**Fix (priority order):**

1. **Preferred:** Call the parameterized ctor, then set remaining fields via `AssignDetails` or a dedicated import method:

```csharp
public static Student CreateFromImport(ImportRow row)
{
    var student = new Student(row.Name, row.Roll);
    student.AssignDetails(row.DateOfBirth, row.Age, row.Score, row.Address ?? string.Empty);
    return student;
}
```

2. If object initializers are required, add an explicit parameterless ctor **only** with clear rules (often `private` + static factory) — avoid public parameterless ctors that leave identity unset.
3. Centralize validation in one place (ctor or static factory), not split across initializer + methods.
4. Add unit tests that import rows missing required columns fail fast at construction time.

**Production takeaway:** `new T()` and `new T { … }` are not interchangeable — initializers still run a ctor first. See **Program.cs** Section 3 — adding any ctor removes the compiler-generated default.

---

---

#### Q4. (D) Your team models loans with the chapter's `Customer` class — public fields plus `CalculateTotalInterest()` on the instance. A new developer moves all interest math into a static `LoanCalculator` and leaves `Customer` as a data bag. Review both approaches. Which would you standardize on for a production lending module, and why?

---

**Answer:**

**Answer:** Prefer a **rich domain model** where `Customer` (or a renamed `LoanAccount`) owns `CalculateTotalInterest()` and enforces loan rules, supplemented by application services for orchestration — not an **anemic** `Customer` with public fields and all behavior in static helpers.

**Rich domain (chapter style, evolved):**

- Behavior lives with data: `CalculateTotalInterest()` reads `LoanAmount`, `RateOfInterest`, `DurationOfLoan` from the instance — matches **Program.cs** Section 3.
- Easier to test one object: construct `Customer`, set fields, assert interest without static glue.
- Natural path to encapsulation: replace public fields with properties, add validation ("rate must be > 0") inside the type.

**Anemic model (static `LoanCalculator`):**

- Acceptable for **pure functions** over DTOs (reporting, batch ETL) or when entities are persistence shapes only (some CRUD APIs).
- Risk: every caller must remember to invoke the calculator; invariants scatter across services; duplicate formulas drift.

**Production standard:**

- **Core lending domain:** rich entities/value objects + domain services for multi-entity rules (e.g., cross-account limits).
- **API/integration layer:** map entities to DTOs; do not expose public mutable fields.
- **Static calculators:** only for stateless policy tables or shared math with no instance context.

**Production takeaway:** Karat tests whether you recognize anemic vs rich trade-offs — tutorials use public fields for clarity; production moves behavior inward and encapsulates state. See **Program.cs** — `Customer.CalculateTotalInterest()` vs field-only `Student` with `AssignDetails`.

---

---

#### Q5. (M) A scheduling feature stores each student's date of birth and a "next review date." A bug report says review dates never update on the student record. Review:

```csharp
public void ScheduleReview(Student student, DateTime reviewDate)
{
    DateTime scheduled = student.DateOfBirth;
    scheduled = reviewDate; // developer intended to persist review on student
}

public void Demo()
{
    var s = new Student("Darshan K.", 101);
    s.AssignDetails(new DateTime(2000, 12, 7), 15, 78.52, "Malegaon");
    ScheduleReview(s, new DateTime(2026, 9, 1));
    Console.WriteLine(s.DateOfBirth); // still 2000-12-07
}
```

Explain the behavior using **reference vs value** semantics. What would you change?

---

**Answer:**

```csharp
public void ScheduleReview(Student student, DateTime reviewDate)
{
    DateTime scheduled = student.DateOfBirth;
    scheduled = reviewDate; // developer intended to persist review on student
}

public void Demo()
{
    var s = new Student("Darshan K.", 101);
    s.AssignDetails(new DateTime(2000, 12, 7), 15, 78.52, "Malegaon");
    ScheduleReview(s, new DateTime(2026, 9, 1));
    Console.WriteLine(s.DateOfBirth); // still 2000-12-07
}
```

Explain the behavior using **reference vs value** semantics. What would you change?

**Answer:** `Student` is a **reference type** — the parameter `student` points at the heap object and could be mutated through it. `DateTime` is a **value type** — `scheduled = student.DateOfBirth` copies the date value into a local; reassigning `scheduled` only changes the local copy, not `student.DateOfBirth`. The developer confused assigning a new value to a local struct with updating instance state.

**Mechanism:**

| Type | Assignment | Effect in snippet |
|---|---|---|
| `Student` (class) | Passed by reference | Mutations like `student.RollNumber = x` would persist |
| `DateTime` (struct) | Copied by value | `scheduled = reviewDate` does not write back to `student` |

**Fix:**

1. Add a field/property on `Student` (e.g., `NextReviewDate`) and assign directly: `student.NextReviewDate = reviewDate;`.
2. Or, if overloading `DateOfBirth` was intentional, assign to the instance field: `student.DateOfBirth = reviewDate;` (usually wrong semantically — separate fields are clearer).
3. For value-type updates that must stick, always mutate through the owning object, not a detached local copy — same lesson as `birthDateCopy = birthDateCopy.AddYears(1)` not changing `student1.DateOfBirth` in **Program.cs** Section 11.

**Production takeaway:** Reference variables alias one object; value types copy on assignment — production bugs often mix the two when developers expect struct locals to mirror writes. See **Program.cs** Sections 5 and 11 — reference semantics vs struct copy independence.

---

---

#### Q6. (R) An enrollment module aliases student records for audit trails. Roll numbers change unexpectedly in downstream reports. Review:

```csharp
public class EnrollmentService
{
    public void RegisterAuditCopy(Student liveEnrollment, List<Student> auditTrail)
    {
        Student auditEntry = liveEnrollment; // snapshot for compliance
        auditTrail.Add(auditEntry);
    }

    public void CorrectRollNumber(Student liveEnrollment, int correctedRoll)
    {
        liveEnrollment.RollNumber = correctedRoll;
    }
}
```

```csharp
var student = new Student("Priya Nair", 102);
service.RegisterAuditCopy(student, auditTrail);
service.CorrectRollNumber(student, 1102);
// auditTrail[0].RollNumber is now 1102 — not the original 102
```

What went wrong with object identity, and how would you fix the audit trail?

---

### 02. Properties & Indexers - Done

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/02. Properties & Indexers - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public class EnrollmentService
{
    public void RegisterAuditCopy(Student liveEnrollment, List<Student> auditTrail)
    {
        Student auditEntry = liveEnrollment; // snapshot for compliance
        auditTrail.Add(auditEntry);
    }

    public void CorrectRollNumber(Student liveEnrollment, int correctedRoll)
    {
        liveEnrollment.RollNumber = correctedRoll;
    }
}
```

```csharp
var student = new Student("Priya Nair", 102);
service.RegisterAuditCopy(student, auditTrail);
service.CorrectRollNumber(student, 1102);
// auditTrail[0].RollNumber is now 1102 — not the original 102
```

What went wrong with object identity, and how would you fix the audit trail?

**Answer:** `Student auditEntry = liveEnrollment` copies the **reference**, not a snapshot — `auditTrail` and `liveEnrollment` denote the same instance. `ReferenceEquals(auditEntry, liveEnrollment)` is true, so correcting the live record mutates the "audit" entry too. Compliance expects **value snapshots** or immutable records, not shared aliases.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Reference sharing | Audit list stores pointers to live objects | Historical reports rewrite when live data changes |
| Identity vs equality | No distinction between "same student over time" and "point-in-time copy" | Audit trail legally/operationally invalid |
| Design | Mutable `Student` with public fields | Any holder can mutate shared state unintentionally |
| Correctness | Comment says "snapshot" but code aliases | Reviewers miss bug without `ReferenceEquals` mental model |

**Fix (priority order):**

1. Store **immutable audit DTOs** or value snapshots at registration time:

```csharp
auditTrail.Add(new StudentAuditRecord(
    liveEnrollment.StudentName,
    liveEnrollment.RollNumber,
    capturedAt: DateTime.UtcNow));
```

2. If full `Student` copies are required, implement explicit `Clone()` / mapping to a new `Student` instance — never add the same reference twice.
3. Prefer append-only audit logs (events) keyed by enrollment id, not mutable object graphs in a `List<Student>`.
4. For live corrections, mutate only the authoritative record; audits remain frozen records.
5. Use `ReferenceEquals` in tests to assert audit entries are **not** the same instance as live enrollment.

**Production takeaway:** `ReferenceEquals` and `==` on classes compare identity by default — "copy" in business language usually means new instance or immutable record, not `=`. See **Program.cs** Section 5 — `enrolled = student1` shares one object; Section 5b — separate `new Student(...)` for independent instances.

---

### 02. Properties & Indexers - Done

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/02. Properties & Indexers - Done`

---

---

#### Q1. (R) A catalog service persists book records. A junior dev refactors `Isbn` to an auto-property "for consistency." Review the change — what breaks in production, and how should `Isbn` be implemented?

```csharp
public class Book
{
    public string CatalogId { get; }

    // Refactored from full property with validation
    public string Isbn { get; set; } = string.Empty;

    public Book(string catalogId, string title, string isbn)
    {
        CatalogId = catalogId;
        Title = title;
        Isbn = isbn;
    }

    public string Title { get; set; } = string.Empty;
}
```

```csharp
// Called from import pipeline after JSON deserialization
var book = new Book("CAT-001", "Clean Code", "978-0132350884");
book.Isbn = "   ";                    // whitespace-only "update"
await repository.SaveAsync(book);      // persists invalid ISBN
```

---

**Answer:**

**Answer:** Auto-implemented properties cannot enforce invariants — whitespace-only or untrimmed ISBNs pass straight through to persistence, corrupting catalog data and breaking keyed lookups that assume normalized values.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No validation on `Isbn` setter | `"   "` and empty strings persist; ISBN indexers return inconsistent results |
| Data integrity | No trimming/normalization | `" 978-0132350884 "` and `"978-0132350884"` may be treated as different keys |
| Design | Validation moved out of the type | Import pipeline, API controllers, and EF must duplicate rules — easy to miss one path |
| Encapsulation | Public `{ get; set; }` on invariant field | Any caller can bypass domain rules the chapter's `Book.Isbn` full property was meant to centralize |

**Fix (priority order):**

1. Restore a **full property** with a private backing field — validate in the setter (reject null/whitespace, trim before store), matching this chapter's `Book.Isbn` pattern.
2. Keep constructor assignment routed through the setter (`Isbn = isbn;`) so construction and later updates share one code path.
3. Add unit tests for invalid ISBN assignment (`ArgumentException`) and trim behavior.
4. Leave simple pass-through data (e.g., `Title`) as auto-properties — apply full properties only where invariants exist.

```csharp
private string _isbn = string.Empty;

public string Isbn
{
    get => _isbn;
    set
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ISBN is required.", nameof(value));
        _isbn = value.Trim();
    }
}
```

**Production takeaway:** Auto-properties are for dumb data; the moment a field has validation, normalization, or authorization, use a backing field. See **Program.cs** Section 2b — full property on `Isbn`.

---

---

#### Q2. (R) An API team models catalog metadata with init-only properties. After code review, a developer adds a "sync" method. What is wrong, and what pattern should they use instead?

```csharp
public class CatalogEntry
{
    public string CatalogId { get; init; } = string.Empty;
    public DateTime AddedOn { get; init; }
    public string Title { get; set; } = string.Empty;
}

public class CatalogSyncService
{
    public void ApplyRemoteTimestamp(CatalogEntry entry, DateTime remoteAddedOn)
    {
        // Remote source has the authoritative AddedOn — update local copy
        entry.AddedOn = remoteAddedOn;
    }
}
```

---

**Answer:**

**Answer:** `init` accessors only allow assignment during object construction or an object initializer — assigning `AddedOn` after the object exists is a compile-time error (`CS8852`), and that restriction is intentional for create-once metadata.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `entry.AddedOn = remoteAddedOn` outside init context | Build fails — method as written cannot ship |
| Design | Treating init-only props like mutable `{ get; set; }` | Confusion about which fields are immutable audit metadata vs editable display data |
| Correctness (if forced) | Reflection or serialization tricks to mutate init props | Breaks immutability guarantees; audit trail timestamps become untrustworthy |
| API contract | Mixed mutability on one DTO | Callers cannot tell `AddedOn` is fixed at creation without reading every accessor |

**Fix (priority order):**

1. **Do not** mutate init-only properties after construction — if remote sync needs a new timestamp, create a **new** `CatalogEntry` (record/copy pattern) or use a dedicated mutable field (`LastSyncedOn { get; private set; }`) for operational updates.
2. Keep true creation metadata (`AddedOn`, `CatalogId`) as `{ get; init; }` or `{ get; }` set only in the constructor.
3. Use `{ get; set; }` only for fields that legitimately change (`Title`, status flags).
4. For EF/API deserialization that must hydrate init props, rely on constructor + init in one creation flow — not post-hoc setter methods.

```csharp
public CatalogEntry WithAddedOn(DateTime addedOn) =>
    new() { CatalogId = CatalogId, Title = Title, AddedOn = addedOn };
```

**Production takeaway:** `init` is stricter than `{ get; set; }` for "set once at birth" data — production models should separate immutable audit fields from mutable operational fields. See **Program.cs** Section 2d — `AddedOn { get; init; }`.

---

---

#### Q3. (R) A dashboard reads `DisplayLabel` on every row render. A teammate adds "helpful" logic inside the expression-bodied getter. Review — what problems does this introduce?

```csharp
public class Book
{
    public string Title { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int ViewCount { get; private set; }

    public string DisplayLabel
    {
        get
        {
            ViewCount++;
            LastRendered = DateTime.UtcNow;
            return $"{Title} [{Isbn}]";
        }
    }

    public DateTime LastRendered { get; private set; }
}
```

```csharp
// Grid binds to DisplayLabel — 500 rows × 3 re-renders per second
foreach (var book in books)
    row.Cells["Label"].Text = book.DisplayLabel;
```

---

**Answer:**

**Answer:** Expression-bodied and block-bodied getters must be **pure reads** — incrementing `ViewCount` and updating `LastRendered` on every property access turns an innocent label lookup into hidden mutation that breaks caching, threading, and test expectations.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Getter mutates object state | `ViewCount` grows on every UI re-bind, not on actual user views — metrics lie |
| Performance | Side effects on hot path (500 rows × 3/sec) | Unnecessary writes; defeats memoization; harder to optimize |
| Surprise / API | Property looks like a field read | Callers expect idempotent `book.DisplayLabel` — logging/analytics code may read it in loops |
| Threading | Non-atomic read + two writes in getter | Concurrent grid refresh can race on `ViewCount` / `LastRendered` without locks |
| Testing | Asserting label text changes internal counters | Tests become order-dependent; "read property" tests mutate state |

**Fix (priority order):**

1. Make `DisplayLabel` a **pure computed property**: `public string DisplayLabel => $"{Title} [{Isbn}]";` — no storage writes in the getter.
2. Move view tracking to an explicit method: `RecordView()` or an application/analytics service called once per actual view event.
3. If expensive formatting is needed, use explicit caching with a known invalidation point (when `Title`/`Isbn` change), not on every get.
4. Code-review rule: **getters do not have side effects** — same input state, same output, no hidden I/O.

**Production takeaway:** Expression-bodied `=>` properties are syntactic sugar for `get` only — they do not imply "cheap," but they must not mutate. Side effects belong in methods or event handlers. See **Program.cs** Section 2f — `DisplayLabel` as read-only computed value.

---

---

#### Q4. (R) A `BookShelf` indexer passes QA with small test data, but production reports `NullReferenceException` and "empty slot" bugs. Review the indexer — what's wrong with bounds checking?

```csharp
public class BookShelf
{
    private readonly Book[] _slots;
    private int _count;

    public BookShelf(int capacity) => _slots = new Book[capacity];

    public void Add(Book book) => _slots[_count++] = book;

    public Book this[int index]
    {
        get
        {
            if (index < 0 || index >= _slots.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _slots[index];   // may return default/null for unused slots
        }
    }

    public int Count => _count;
}
```

```csharp
var shelf = new BookShelf(10);
shelf.Add(bookA);
shelf.Add(bookB);
var third = shelf[2];   // no exception — caller gets null
```

---

**Answer:**

**Answer:** The indexer validates against `_slots.Length` (capacity) instead of `_count` (occupied slots), so indices in the "empty tail" of the array are legal but return `null` — callers expecting a `Book` hit `NullReferenceException` downstream.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Bounds check uses `Capacity`, not `Count` | `shelf[2]` succeeds after two adds — returns `default(Book)` (null reference) |
| API contract | Indexer implies "slot i of books on shelf" | Callers cannot distinguish "out of range" from "empty reserved slot" |
| Consistency | `Add` stops at capacity; indexer allows reading unused indices | Off-by-one between logical collection size and array size |
| Defensive coding | Downstream null dereference instead of clear exception | Harder to diagnose in prod logs than `ArgumentOutOfRangeException` |

**Fix (priority order):**

1. Bound against **`_count`**, not `_slots.Length`: `if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException(nameof(index));`
2. Match this chapter's `BookShelf` int indexer — positional access only over populated slots.
3. If "raw array slot" access is needed internally, keep it private; public indexer represents logical contents.
4. Add tests: after `Add` twice, index `2` must throw; index `0` and `1` return books.

```csharp
public Book this[int index]
{
    get
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _slots[index];
    }
}
```

**Production takeaway:** Indexers should enforce the same logical bounds as `Count` — array capacity is an implementation detail. See **Program.cs** Section 3a — int indexer checks `index >= _count`.

---

---

#### Q5. (R) A library module exposes the internal book list through a property so callers can "query and filter easily." Review the API surface — what can go wrong?

```csharp
public class LibrarySection
{
    private readonly List<Book> _books = new();

    public List<Book> Books => _books;

    public void AddBook(Book book) => _books.Add(book);
}
```

```csharp
var section = library.GetSection("Fiction");
section.Books.Clear();                          // bypasses AddBook / validation
section.Books.Add(new Book("", "Hack", "bad")); // no ISBN rules enforced
var snapshot = section.Books;                   // same list reference — mutates later
```

---

**Answer:**

**Answer:** Returning the live `List<Book>` breaks encapsulation — callers can clear, reorder, or inject invalid books without going through `AddBook`, and holding a reference to `Books` sees every later internal mutation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encapsulation | Exposes mutable `_books` reference | `section.Books.Clear()` empties internal state without validation or events |
| Invariant bypass | Direct `Add` skips ISBN/page-count rules on `Book` | Invalid domain objects enter the collection |
| Aliasing | `snapshot = section.Books` shares reference | Code thinks it captured a point-in-time list; later adds/removes corrupt the "snapshot" |
| Evolution | Cannot swap backing store (array, immutable list) later | Public API locked to `List<Book>` forever |
| Thread safety | Unsynchronized shared list | Concurrent read during internal modification → `InvalidOperationException` or torn state |

**Fix (priority order):**

1. Expose **`IReadOnlyList<Book>`** (or `IEnumerable<Book>`) via a defensive copy or read-only wrapper: `public IReadOnlyList<Book> Books => _books.AsReadOnly();` or `return _books.ToList()` when callers need isolation.
2. Keep all mutations through controlled methods: `AddBook`, `RemoveBook`, `ClearSection` — enforce validation and raise change notifications if needed.
3. For LINQ-friendly querying without mutation, expose `Books.AsReadOnly()` or methods like `FindByIsbn(string)`.
4. Never return `List<T>` from a public property unless the type is explicitly a builder/mutable DTO documented as such.

```csharp
public IReadOnlyList<Book> Books => _books.AsReadOnly();
```

**Production takeaway:** Properties that expose collections should expose **views or copies**, not the backing collection — same principle as `BookShelf` hiding `_slots` behind indexers and `Count`. See foundation encapsulation — prefer controlled access over public fields/lists.

---

---

#### Q6. (D) You inherit a domain model mixing auto-properties, init-only metadata, expression-bodied labels, and a collection property. A PR proposes fixing all five categories above in one sprint. How do you prioritize encapsulation fixes before a catalog migration goes live?

Topics on the table: ISBN validation (Q1), init-only `AddedOn` misuse (Q2), side-effect getters (Q3), indexer bounds vs `Count` (Q4), and returning `IReadOnlyList<Book>` vs `List<Book>` (Q5). What do you fix first, what can wait, and why?

---

---

### 03. Constructors & Method Overloading

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/03. Constructors & Method Overloading`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Fix **data-corruption and silent-failure paths first** (ISBN validation, indexer bounds, mutable collection exposure), then **compile/design violations** (init misuse), then **observability/side-effect getters** — ship validation and bounds before migration writes bad rows into the new store.

**Priority order:**

| Priority | Fix | Why first / can wait |
|---|---|---|
| **P0 — before migration** | ISBN full property (Q1) | Invalid keys written during import are expensive to backfill; breaks ISBN indexer lookups immediately |
| **P0 — before migration** | Indexer bounds vs `_count` (Q4) | Silent nulls cause NREs in batch jobs — migration scripts often iterate by index |
| **P0 — before migration** | Stop exposing `List<Book>` (Q5) | Prevents callers from corrupting in-memory catalog during parallel migration tooling |
| **P1 — same release** | Init-only discipline (Q2) | Compile blocker if present; clarify immutable audit fields before API publishes contracts |
| **P2 — next iteration** | Pure getters / remove side effects (Q3) | Wrong metrics and perf, but rarely corrupts persisted data; fix before enabling analytics dashboards |
| **P3 — hardening** | Tests + API review checklist | Property validation tests, indexer edge cases, read-only collection contract tests |

**Trade-offs:**

- A big-bang refactor delays migration — **surgical P0 fixes** on hot types (`Book`, `BookShelf`, `LibrarySection`) unblock data move with minimal surface change.
- Auto-properties on low-risk display fields (`Title`) can stay — don't gold-plate every property in the same PR.
- Document team rules: invariants → full property; create-once → `init`; computed → pure getter; collections → `IReadOnlyList` or indexer.

**Production takeaway:** Encapsulation fixes rank by **what bad data or silent nulls cost in production**, not by line count — Karat tests prioritization, not just pattern recognition. Aligns with **Program.cs** property/indexer patterns in Sections 2–3.

---

---

### 03. Constructors & Method Overloading

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/03. Constructors & Method Overloading`

---

---

#### Q1. (R) A teammate refactors `OrderLine` to chain constructors like the chapter's `Product` type. QA reports invalid lines in production — empty SKU and zero quantity slip through. Review the ctors. What went wrong, and how do you fix it?

```csharp
public sealed class OrderLine
{
    public string Sku { get; }
    public int Quantity { get; }

    public OrderLine()
        : this("MISC", 1)
    {
    }

    public OrderLine(string sku)
    {
        Sku = sku?.Trim() ?? string.Empty;
        Quantity = 1;
    }

    public OrderLine(string sku, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        Sku = sku.Trim();
        Quantity = quantity;
    }
}
```

---

**Answer:**

**Answer:** The single-parameter constructor does not chain to the validated `(string, int)` ctor — it duplicates initialization logic without guards, so callers using `new OrderLine("")` or `new OrderLine(null)` get empty SKUs that never hit the validation in the three-parameter constructor.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | `OrderLine(string sku)` bypasses `: this(sku, 1)` | Invalid SKU values reach production data |
| Correctness | `sku?.Trim() ?? string.Empty` masks null instead of rejecting | Silent bad state instead of fail-fast at creation |
| Maintainability | Validation duplicated in intent but only implemented once | Future ctors can repeat the same bypass mistake |

**Fix (priority order):**

1. Chain the one-parameter ctor: `public OrderLine(string sku) : this(sku, 1) { }` — single validation path.
2. Keep all invariant checks in the **most complete** ctor (here, `(string sku, int quantity)`), matching the chapter's `Product` pattern in **Program.cs** Sections 1c and 1b.
3. Remove defensive null-coalescing to empty string in convenience ctors — let the validated ctor throw `ArgumentException`.
4. Add unit tests per ctor overload to assert invalid SKU/quantity throws before any repository write.

```csharp
public OrderLine(string sku)
    : this(sku, 1)
{
}
```

**Production takeaway:** Constructor chaining only enforces invariants when **every** ctor path reaches the guarded ctor — a common Karat trap after "helpful" shortcut ctors are added without `: this(...)`.

---

---

#### Q2. (R) A .NET 8 service adopts a **primary constructor** for a warehouse DTO. Unit tests expecting `ArgumentException` on bad input fail with `NullReferenceException` instead. Review the type. What is the initialization order problem, and how would you enforce invariants?

```csharp
public sealed class StockReceipt(string sku, decimal unitCost, int quantity)
{
    public string Sku { get; } = sku.Trim();
    public decimal UnitCost { get; } = unitCost;
    public int Quantity { get; } = quantity;

    // Intended guard — runs after field initializers above
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (unitCost < 0m)
            throw new ArgumentOutOfRangeException(nameof(unitCost));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));
    }
}
```

---

**Answer:**

**Answer:** Field initializers on the primary-constructor type run **before** the instance constructor body block, so `sku.Trim()` executes while `sku` is still null — throwing `NullReferenceException` instead of the intended `ArgumentException` from the guard block below.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Sku = sku.Trim()` before validation block | Wrong exception type; callers/tests cannot rely on contract |
| Correctness | Invariants assumed to run "first" in `{ }` body | Primary ctor initialization order differs from mental model |
| API contract | Mixed primary params + property initializers | Hard to see which line can throw what |

**Fix (priority order):**

1. Validate **before** any use of parameters — either in the constructor body as the first statements with manual assignment to properties, or via a static factory `StockReceipt.Create(...)` that validates then calls a private ctor.
2. Do not call instance methods (`Trim`) on parameters in field/property initializers when null is invalid.
3. Prefer explicit parameterized ctor + chaining for domain types with strict invariants; use primary constructors for simple immutable carriers where validation is minimal or delegated to a factory.
4. Align tests to assert the final exception type after fix (`ArgumentException` for null/whitespace SKU).

```csharp
public sealed class StockReceipt
{
    public string Sku { get; }
    public decimal UnitCost { get; }
    public int Quantity { get; }

    public StockReceipt(string sku, decimal unitCost, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));
        if (unitCost < 0m)
            throw new ArgumentOutOfRangeException(nameof(unitCost));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        Sku = sku.Trim();
        UnitCost = unitCost;
        Quantity = quantity;
    }
}
```

**Production takeaway:** Primary constructors do not replace the chapter rule — **enforce invariants at creation** — but the execution order is initializer expressions first, then body; Karat tests whether you know where validation must live.

---

---

#### Q3. (R) After adding a convenience overload to `LineItemCalculator`-style pricing helpers, `dotnet build` fails with **CS0121** ("The call is ambiguous"). Which overloads conflict, and how do you resolve the call site or signatures?

```csharp
public static class PricingHelper
{
    public static decimal LineTotal(int qty, decimal unitPrice, decimal discountRate = 0m)
    {
        decimal gross = qty * unitPrice;
        return gross - (gross * discountRate);
    }

    public static decimal LineTotal(int qty, decimal unitPrice, decimal discountRate, decimal taxRate)
    {
        decimal net = LineTotal(qty, unitPrice, discountRate);
        return net + (net * taxRate);
    }
}

// Call site in OrderService:
decimal total = PricingHelper.LineTotal(3, 2.49m, 0.10m);
```

---

**Answer:**

**Answer:** The call `LineTotal(3, 2.49m, 0.10m)` matches both the three-parameter overload (with optional `discountRate`) and the four-parameter overload equally well — the third argument `0.10m` can bind to either `discountRate` or the third positional parameter before `taxRate`, so the compiler cannot pick a unique best match.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Optional parameter on overload A overlaps arity with overload B | CS0121 — build blocked |
| Design | Two overloads differ only by trailing optional vs required extension | Call sites with three decimals are ambiguous |
| Maintainability | Mixing optional params and extra overloads (chapter Section 9 warning) | Every new decimal argument risks new ambiguity |

**Fix (priority order):**

1. **Preferred:** Remove the optional from the three-parameter signature — use two explicit overloads (`qty, price` and `qty, price, discount`) plus a separate `WithTax(...)` method, mirroring **Program.cs** `Price(int, decimal)` vs `Price(int, decimal, decimal)`.
2. At the call site, disambiguate with a **named argument**: `LineTotal(3, 2.49m, discountRate: 0.10m)` if you must keep the optional temporarily.
3. Avoid `params` + optional + overlapping arity in the same method group — chapter Section 15 / CS0121.
4. Add a compiler-focused unit test project or analyzer rule comment so overlapping optionals are caught in review.

**Production takeaway:** Overload resolution is compile-time — ambiguous APIs never ship — but Karat uses this to test whether you can diagnose **optional parameters colliding with additional overloads**, not just recall the CS0121 code.

---

---

#### Q4. (M) A junior dev models discounted inventory items by inheriting from `Product` (chapter pattern). `dotnet build` reports **CS2506** and **CS7036**. Diagnose **`: this(...)` vs `: base(...)`** mistakes and state the correct ctor initialization order.

```csharp
public class Product
{
    public Product(string name, decimal unitPrice) { /* validates */ }
    // No parameterless constructor — adding one removed compiler default.
}

public class DiscountedProduct : Product
{
    public decimal DiscountRate { get; }

    // Attempt A — build error CS2506
    public DiscountedProduct(string name, decimal unitPrice, decimal discountRate)
        : base(name, unitPrice)
        : this(name, unitPrice, discountRate, applyMinimum: true)
    {
        DiscountRate = discountRate;
    }

    // Attempt B — would be CS7036 without : base(...)
    public DiscountedProduct(string name, decimal unitPrice, decimal discountRate, bool applyMinimum)
    {
        DiscountRate = discountRate;
    }

    public DiscountedProduct(string name)
        : base(name, 0m)
    {
        DiscountRate = 0.10m;
    }
}
```

---

**Answer:**

_Answer not found._

---

#### Q5. (P) An ASP.NET Core API maps inbound JSON to a **`required`** init-only request type before calling domain ctors. A client omits `Name` but the payload still deserializes and reaches `new Product(...)`. What happened at compile time vs runtime, and how do you align API contracts with constructor validation?

```csharp
public sealed class CreateProductRequest
{
    public required string Name { get; init; }
    public required decimal UnitPrice { get; init; }
}

public static class ProductFactory
{
    public static Product FromRequest(CreateProductRequest request)
    {
        return new Product(request.Name, request.UnitPrice);
    }
}

// Controller (simplified):
[HttpPost]
public IActionResult Create([FromBody] CreateProductRequest body)
{
    var product = ProductFactory.FromRequest(body);
    return Ok(product.Describe());
}
```

Client POST body:

```json
{ "unitPrice": 8.99 }
```

---

**Answer:**

**Answer:** `required` is enforced at **object creation** for object initializers and `new()` expressions at compile time, but **System.Text.Json** (and Newtonsoft) can still materialize instances without required members unless you enable required-member deserialization validation — so `Name` may default to `null` at runtime, and `Product`'s ctor then throws or mis-validates depending on null checks.

- **Compile time:** `new CreateProductRequest { UnitPrice = 8.99m }` without `Name` fails to compile — `required` works for in-code construction.
- **Runtime (JSON):** Deserializer may not enforce `required` unless configured (`JsonSerializerOptions` / `[JsonRequired]` / validation attributes / manual guard in minimal APIs).
- **Domain layer:** `Product(string name, decimal unitPrice)` should still validate — last line of defense — but the API should return **400 ProblemDetails**, not a 500 from an unhandled `ArgumentException`.
- **Alignment:** Use `[Required]` + FluentValidation or ASP.NET model validation, enable required property support for STJ in .NET 7+, map to domain via factory that throws typed validation exceptions converted to 400.

```csharp
// Minimal API guard example:
if (string.IsNullOrWhiteSpace(body.Name))
    return Results.ValidationProblem(new Dictionary<string, string[]>
    {
        [nameof(body.Name)] = ["Name is required."]
    });
```

**Production takeaway:** Required members and constructor validation solve different layers — DTO `required` for developer mistakes, ctor invariants for domain truth, API validation for external clients — Karat tests stacking all three.

---

---

#### Q6. (D) A warehouse microservice registers services in DI but still constructs dependencies manually inside ctors. Review startup and `InventorySyncService`. What breaks in tests, lifetimes, and startup, and what pattern replaces it?

```csharp
// Program.cs
builder.Services.AddSingleton<IInventorySyncService, InventorySyncService>();
builder.Services.AddSingleton<IProductCatalog, ProductCatalog>();

public sealed class InventoryRegistry  // legacy singleton from tutorial
{
    private static readonly InventoryRegistry Shared = new();
    private InventoryRegistry() { }
    public static InventoryRegistry Instance => Shared;
    public void Register(Product p) { /* ... */ }
}

public sealed class ProductCatalog : IProductCatalog
{
    private readonly List<Product> _products = new();

    public ProductCatalog()
    {
        _products.Add(new Product("Seed SKU", -1.00m)); // negative price
    }
}

public sealed class InventorySyncService : IInventorySyncService
{
    private readonly InventoryRegistry _registry;

    public InventorySyncService()
    {
        _registry = InventoryRegistry.Instance;
    }

    public void Sync(Product product) => _registry.Register(product);
}
```

What would you change in registration, ctor signatures, and object creation?

---

---

### 04. Static Members & Static Classes

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/04. Static Members & Static Classes`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** The service graph mixes DI registration with static singleton access and throws inside `ProductCatalog`'s ctor during container build — startup fails (or the host never becomes healthy), tests cannot substitute a fake registry, and two lifetimes (DI singleton vs static `Instance`) fight for the same responsibility.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Startup | `new Product("Seed SKU", -1.00m)` in `ProductCatalog` ctor | `ArgumentOutOfRangeException` during `BuildServiceProvider` — app won't start |
| DI | `InventorySyncService` uses `InventoryRegistry.Instance` | Bypasses container; cannot mock `IInventoryRegistry` in tests |
| Lifetime | Static singleton + `AddSingleton<>` duplicate ownership | Hidden global state; unclear thread-safety and test isolation |
| Design | Chapter singleton (`InventoryRegistry`) copied into production service | Violates "prefer DI" note in **Program.cs** Section 2 |

**Fix (priority order):**

1. **Constructor injection:** `public InventorySyncService(IInventoryRegistry registry)` — no parameterless ctor grabbing statics.
2. Register an abstraction: `builder.Services.AddSingleton<IInventoryRegistry, InventoryRegistry>()` with a **public or internal** ctor (or factory delegate) — retire `Instance` for app code; keep private ctor only if factory registration is used.
3. Move seed data out of the ctor — use `IHostedService`, explicit `SeedAsync`, or configuration-driven load so invalid catalog data surfaces as a controlled startup error with logging, not ctor throw during DI resolution.
4. Let `Product`'s validated ctor throw for bad **runtime** input; seed paths must pass valid arguments or use a dedicated test factory.
5. Integration tests build `WebApplicationFactory` with replaced `IInventoryRegistry` fake — only possible when ctors demand interfaces.

```csharp
builder.Services.AddSingleton<IInventoryRegistry, InventoryRegistry>();
builder.Services.AddSingleton<IInventorySyncService, InventorySyncService>();

public sealed class InventorySyncService : IInventorySyncService
{
    private readonly IInventoryRegistry _registry;

    public InventorySyncService(IInventoryRegistry registry)
    {
        _registry = registry;
    }

    public void Sync(Product product) => _registry.Register(product);
}
```

**Production takeaway:** Object creation belongs in the composition root — ctors enforce invariants for **their** parameters, not for bootstrapping entire graphs via `new` and static `Instance`; Karat links chapter singleton intro to real ASP.NET Core registration mistakes.

---

---

### 04. Static Members & Static Classes

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/04. Static Members & Static Classes`

---

---

#### Q1. (R) An ASP.NET Core API caches the "current user's cart" in a static field so every controller can read it without DI. Under load, users report seeing each other's items. Review the code — what is wrong and how do you fix it?

```csharp
public static class CartContext
{
    private static List<CartItem> _items = new();

    public static void SetCart(List<CartItem> items) => _items = items;

    public static decimal GetTotal() => _items.Sum(i => i.Price * i.Quantity);
}

public class CheckoutController : ControllerBase
{
    [HttpPost("checkout")]
    public IActionResult Checkout([FromBody] List<CartItem> cart)
    {
        CartContext.SetCart(cart);
        var total = CartContext.GetTotal();
        return Ok(new { total });
    }
}
```

---

**Answer:**

**Answer:** A static `_items` list is one shared object for the entire application domain — every request overwrites and reads the same cart, so concurrent users bleed data across threads and requests; this is the classic mutable-static-state failure mode in web apps.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable static `_items` holds per-user data | User A sees User B's cart under concurrency |
| Architecture | Static holder bypasses request scope and DI | Hidden global state; untestable without static resets |
| Scale-out | In-memory static state is per process | Sticky sessions won't help — race is on one instance |
| Thread safety | `List<T>` mutated without synchronization | Corrupted list / exceptions under parallel requests |

**Fix (priority order):**

1. Remove `CartContext` static mutable storage — register a **scoped** `ICartService` (or store cart keyed by user id in Redis/SQL).
2. Pass `HttpContext.User` identity into the service; never store "current user" in static fields.
3. If caching shared **read-only** reference data, use `IMemoryCache` or `IOptions<T>` with immutable snapshots — not a static `List` rewritten per request.
4. Add integration tests with parallel HTTP clients to catch cross-user leakage.

**Production takeaway:** Static members are fine for type-level constants and pure helpers (`BankAccount.IsValidRoutingNumber`) — not for request-scoped or user-scoped state. See **Program.cs** Section 1 — "mutable static fields are shared global state."

---

---

#### Q2. (M) A teammate adds runtime config loading to `AppSettings` and reports intermittent `TypeInitializationException` on first request. Review the static initialization — what ordering traps exist, and how would you make startup deterministic?

```csharp
public static class AppSettings
{
    public static readonly string EnvironmentName =
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

    public static readonly int MaxLoginAttempts = LoadMaxAttempts();

    private static readonly string ConfigPath =
        Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    static AppSettings()
    {
        Console.WriteLine($"Loading settings from {ConfigPath} for {EnvironmentName}");
    }

    private static int LoadMaxAttempts()
    {
        // reads ConfigPath from disk — throws if file missing
        return int.Parse(File.ReadAllText(ConfigPath).Trim());
    }
}
```

---

**Answer:**

**Answer:** Static field initializers run in declaration order before the static constructor body, but circular reads between static fields or throwing initializers can fail type initialization once and poison the type for the AppDomain — the fix is to defer I/O to explicit startup (`IConfiguration`) instead of fragile static ctor chains.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Init order | `MaxLoginAttempts = LoadMaxAttempts()` runs before `static AppSettings()` body | `LoadMaxAttempts` uses `ConfigPath` — OK here, but reordering fields breaks silently |
| Runtime | `LoadMaxAttempts` throws on missing/malformed file | `TypeInitializationException` — type unusable until app restart |
| Design | Static ctor performs I/O and logging | Failures happen on first touch, not at controlled startup |
| Web hosting | First request triggers type load | Lazy failure in prod instead of fail-fast at `WebApplication` boot |

**Fix (priority order):**

1. Move config loading to ASP.NET Core **options pattern** — `builder.Services.Configure<LoginOptions>(configuration.GetSection("Login"))` — validate at startup with `ValidateOnStart`.
2. If static readonly is required, keep static fields **simple** (env var only); load file-backed values in `Program.cs` after `builder.Configuration` exists.
3. Avoid static initializers that depend on each other's side effects; document declaration order or use explicit static ctor assignment only.
4. Never swallow exceptions in static constructors — they wrap inner failures in `TypeInitializationException` and hide root cause in logs.

```csharp
// Prefer at startup, not in static type init:
builder.Services.AddOptions<LoginOptions>()
    .Bind(configuration.GetSection("Login"))
    .Validate(o => o.MaxAttempts > 0, "MaxAttempts required")
    .ValidateOnStart();
```

**Production takeaway:** Static constructors run once per type load (**Program.cs** Section 6 preview) — treat them like hidden startup code. Production apps load config through `IConfiguration`, not static field chains that throw on first access.

---

---

#### Q3. (R) Production logging uses the tutorial's `AuditLogger` singleton instead of `ILogger`. Tests pass locally but CI flakes and log counts are wrong under concurrent requests. Review the pattern — what's broken and what replaces it?

```csharp
public sealed class AuditLogger
{
    private static readonly AuditLogger InstanceField = new AuditLogger();
    private int _entryCount;

    private AuditLogger() { }

    public static AuditLogger Instance => InstanceField;

    public void Record(string message)
    {
        _entryCount++;
        Console.WriteLine($"[{_entryCount}] {message}");
    }
}

// Startup.cs / Program.cs
builder.Services.AddSingleton(AuditLogger.Instance);
```

---

**Answer:**

**Answer:** Hand-rolled singletons expose untestable global mutable state (`_entryCount++` is not thread-safe) and fight ASP.NET Core's built-in logging pipeline — register `ILogger<T>` and scoped/transient services instead of `AuditLogger.Instance`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Non-interlocked `_entryCount++` | Lost updates / wrong counts under parallel requests |
| Testability | Static `Instance` and private ctor | Tests share global counter; order-dependent flakes |
| DI misuse | `AddSingleton(AuditLogger.Instance)` registers pre-built object | Bypasses container ownership; can't substitute fakes easily |
| Observability | `Console.WriteLine` instead of `ILogger` | No levels, filters, structured fields, or centralized sinks |

**Fix (priority order):**

1. Delete the singleton — inject `ILogger<CheckoutController>` (or an application service) via constructor DI.
2. If audit is a domain concern, define `IAuditService` registered **scoped** or **singleton** only when the implementation is **stateless**; persist counts to storage if needed.
3. Use `Interlocked.Increment` only for cheap diagnostics — not as a substitute for proper logging/metrics (`IMeterFactory`, Application Insights).
4. In tests, use `WebApplicationFactory` with logging providers or mock `ILogger<T>` — no static reset hacks.

```csharp
public class CheckoutController : ControllerBase
{
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(ILogger<CheckoutController> logger) => _logger = logger;

    public IActionResult Checkout()
    {
        _logger.LogInformation("Checkout completed for {UserId}", UserId);
        return Ok();
    }
}
```

**Production takeaway:** **Program.cs** Section 10 previews singleton for learning — production prefers DI singletons (container-managed, interface-based) over static `Instance` accessors. See foundation **Constructors** chapter for thread-safe lazy init when a true single instance is required.

---

---

#### Q4. (R) A developer refactors `TaxHelper` to support per-region tax profiles and adds instance state. Build fails. Review the changes — what rules did they violate, and what structure should replace a static class here?

```csharp
public static class TaxHelper
{
    public const decimal DefaultRate = 0.0825m;
    private decimal _regionRate;  // set from constructor

    public TaxHelper(decimal regionRate) => _regionRate = regionRate;

    public static decimal CalculateSalesTax(decimal amount, decimal rate)
        => Math.Round(amount * rate, 2, MidpointRounding.AwayFromZero);

    public decimal CalculateForRegion(decimal amount)
        => CalculateSalesTax(amount, _regionRate);
}
```

---

**Answer:**

**Answer:** Static classes cannot have instance members or instance constructors — the compiler rejects instance fields and `TaxHelper(decimal)` on a `static class`; once you need per-object state, convert to an ordinary instance class (often injected via DI) and keep only pure functions static if needed.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Instance field + ctor on `static class` | CS0708 / CS0710 — build blocked |
| Design | Mixed static utility + instance profile in one type | Violates static class purpose (stateless helper group) |
| API | Callers would need `new TaxHelper(...)` | CS0712 — cannot instantiate static class even without other errors |

**Fix (priority order):**

1. Replace `static class TaxHelper` with a normal sealed class, e.g. `ITaxCalculator` / `TaxCalculator`, taking `regionRate` via constructor or options.
2. Register `ITaxCalculator` as scoped or singleton in DI depending on whether rate is per-request or app-wide config.
3. Keep stateless math as `public static decimal CalculateSalesTax(...)` on a separate `TaxMath` static class **or** private static method on the instance class — match **Program.cs** Section 7 (static class = no instance state).
4. Do not inherit from `TaxHelper` — static classes are implicitly sealed; use composition and interfaces instead.

```csharp
public interface ITaxCalculator
{
    decimal CalculateForRegion(decimal amount);
}

public sealed class TaxCalculator : ITaxCalculator
{
    private readonly decimal _regionRate;
    public TaxCalculator(IOptions<TaxOptions> options) => _regionRate = options.Value.Rate;
    public decimal CalculateForRegion(decimal amount) =>
        Math.Round(amount * _regionRate, 2, MidpointRounding.AwayFromZero);
}
```

**Production takeaway:** Static classes (`TaxHelper`, `AppSettings` helpers) are for stateless utilities — the moment you need `this`, use an instance type. Karat tests whether you know CS0712/CS0709 rules from **Program.cs** Section 7, not just memorize `static`.

---

---

#### Q5. (R) `BankAccount` account numbers duplicate in production after traffic increases. The team uses the tutorial counter as-is. Review the static field usage — what race exists and how do you fix it without abandoning a shared sequence?

```csharp
public class BankAccount
{
    private static int _nextAccountNumber = 1000;

    public int AccountNumber { get; }

    public BankAccount(string ownerName, decimal openingDeposit)
    {
        AccountNumber = _nextAccountNumber++;  // called from many threads
        OwnerName = ownerName;
        Balance = openingDeposit;
    }

    // ...
}
```

---

**Answer:**

**Answer:** `_nextAccountNumber++` is not atomic — two threads can read the same value before either writes back, producing duplicate `AccountNumber` values; use `Interlocked.Increment` for in-process sequences or a database/ID service for authoritative numbering.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Read-modify-write on `_nextAccountNumber++` | Duplicate account numbers under parallel ctor calls |
| Correctness | Assumes single-threaded console demo semantics | Web API creates many `BankAccount` objects concurrently |
| Scale-out | Static counter is per process | Two pods can still collide — DB sequence or distributed ID required |

**Fix (priority order):**

1. **In-process fix:** assign with `Interlocked.Increment(ref _nextAccountNumber)` (or `Interlocked.Add`) inside the constructor.
2. **Production fix:** generate account numbers from SQL `IDENTITY`/sequence, UUID, or Snowflake-style ID service — static fields don't survive multi-instance deployments.
3. Mark `_nextAccountNumber` `private static` and never expose mutability via public static setters.
4. Add stress test spawning parallel account creation tasks asserting unique numbers.

```csharp
public BankAccount(string ownerName, decimal openingDeposit)
{
    AccountNumber = Interlocked.Increment(ref _nextAccountNumber);
    OwnerName = ownerName;
    Balance = openingDeposit;
}
```

**Production takeaway:** **Program.cs** Section 1 warns that mutable static fields race unless synchronized — the tutorial's counter is correct for demos, not for concurrent web registration endpoints.

---

---

#### Q6. (D) Your API team debates three approaches for shared, read-mostly configuration: `public const` literals, `static readonly` loaded at type init, and mutable `public static` properties set from middleware. Which would you allow in a multi-instance ASP.NET Core deployment, and which would you ban? Why?

---

---

### 05. Inheritance &  Polymorphism

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/05. Inheritance &  Polymorphism`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Allow `const` for true compile-time literals and immutable `static readonly` only when the value is identical on every instance and never changes after type init; ban mutable `public static` properties for app configuration — use `IOptions<T>` / `IConfiguration` so each pod reads consistent, reloadable, testable settings without global writes from middleware.

**Allow — `const` (e.g., `MaxLoginAttempts = 3`):**

- Fixed at compile time; zero runtime cost; safe to share everywhere.
- Trade-off: changing value requires recompile of all assemblies that inline it (**Program.cs** Section 8 — const metadata inlining).

**Allow with caution — `static readonly` set once at type init (e.g., `EnvironmentName` from env var):**

- OK for process-wide, immutable facts loaded before requests (deployment stamp, machine name).
- Must not read per-request data; env var is fixed for process lifetime.
- Prefer `IOptions<T>` for anything that might reload or differ by environment file.

**Ban — mutable `public static` properties (e.g., `BankAccount.BankName { get; set; }` set from middleware):**

- Creates hidden global state writable from anywhere; race-prone under concurrent requests.
- Multi-instance: each pod has its own static copy — "global" settings drift if one instance mutates.
- Breaks unit tests (order-dependent mutations) and violates DI/test seams.

**Production pattern:** `builder.Services.Configure<BankOptions>(configuration.GetSection("Bank"))` inject `IOptionsSnapshot<BankOptions>` where refresh matters. Keep static classes for pure functions only (`IsValidRoutingNumber`).

**Production takeaway:** **Program.cs** contrasts `const`, `static readonly`, and mutable static properties — in web apps, configuration flows through the options/configuration stack, not static setters touched during the HTTP pipeline.

---

---

### 05. Inheritance &  Polymorphism

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/05. Inheritance &  Polymorphism`

---

---

#### Q1. (R) Badge printing in production shows `"EMP"` for every staff member, including managers and contractors. Review this excerpt from the payroll service (pattern matches this chapter's `GetBadgeThroughEmployeeReference`). What is wrong, and how do you fix it?

```csharp
public abstract class Employee
{
    public string GetBadgeCode() => "EMP";          // not virtual
}

public class ContractEmployee : Employee
{
    public new string GetBadgeCode() => "CTR";
}

public class Manager : PermanentEmployee
{
    public new string GetBadgeCode() => "MGR";
}

public static string PrintBadge(Employee employee) => employee.GetBadgeCode();

// Called from HR export loop over Employee[] payrollStaff
```

---

**Answer:**

**Answer:** `GetBadgeCode` is hidden with `new` in derived types but invoked through an `Employee` reference — binding is static (compile-time), so the base implementation always runs and every badge prints `"EMP"`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Polymorphism | `new` hides; no `virtual`/`override` | Base reference calls `Employee.GetBadgeCode()` — wrong badge for all derived instances |
| API contract | Polymorphic loop uses `Employee` (see chapter `ProcessPayroll` / badge helper) | HR export and access-control integrations show incorrect codes |
| Maintainability | Looks like overriding; behaves like hiding | Future devs add more `new` methods and repeat the bug |

**Fix (priority order):**

1. Make the base member polymorphic: `public virtual string GetBadgeCode() => "EMP";` and `public override string GetBadgeCode()` in derived types.
2. If badge text is not truly polymorphic, do not call it through `Employee` — accept `ContractEmployee`/`Manager` or introduce a strategy/interface (`IBadgeSource`) resolved at the call site.
3. Add a unit test that asserts badge text when the static type is `Employee` but the runtime type is `Manager` — catches hiding regressions.
4. Enable or heed compiler warning CS0114 ("hides inherited member") and treat `new` on instance methods as a code-review flag.

**Production takeaway:** Method hiding is the chapter's intentional trap — Karat tests whether you distinguish reference-type binding from virtual dispatch. See **Program.cs** Section 6b and `GetBadgeThroughEmployeeReference`.

---

---

#### Q2. (R) After adding `InternEmployee` to the payroll hierarchy, `ProcessPayroll` sometimes throws and totals are wrong. Review the new type and the unchanged payroll loop. What design rule did this violate, and what is the prioritized fix?

```csharp
public class InternEmployee : Employee
{
    public InternEmployee(/* ... */) : base(/* ... */) { }

    public override Money CalculateNet()
    {
        throw new InvalidOperationException("Interns are stipend-only; use StipendService");
    }
}

public static Money ProcessPayroll(IReadOnlyList<Employee> staff)
{
    Money total = new(0m);
    foreach (Employee employee in staff)
        total += employee.CalculateNet();   // no type checks — polymorphic sum
    return total;
}

// InternEmployee instances are stored in List<Employee> alongside permanent staff
```

---

**Answer:**

**Answer:** Substituting `InternEmployee` anywhere `Employee` is expected breaks callers that assume `CalculateNet()` always succeeds and returns a payroll amount — a Liskov Substitution Principle (LSP) violation, not a bug in the loop.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| LSP | Derived type throws where base contract implies a `Money` net pay | `ProcessPayroll` crashes when interns appear in `List<Employee>` |
| Design | Forced IS-A (`Intern : Employee`) for a role with incompatible pay semantics | Every consumer must special-case or try/catch |
| Extensibility | Polymorphic collection pattern (chapter Section 9a) assumes substitutability | Adding one subtype breaks aggregation without compile-time warning |

**Fix (priority order):**

1. Do not model interns as `Employee` if they cannot honor the payroll contract — use a separate type or composition (`PayrollParticipant` interface with `TryCalculateNet` / separate `StipendService`).
2. If they must share a collection, define an explicit contract on the base: document whether `CalculateNet()` may throw; prefer `Money?` or a result type over exceptions for expected branches.
3. Never fix this only with `if (employee is InternEmployee) continue` inside `ProcessPayroll` — that reintroduces switch-on-type and defeats the chapter's polymorphic design.
4. Add integration test: `ProcessPayroll` over mixed staff including the new role.

**Production takeaway:** LSP is not academic — any code that iterates `Employee[]` and calls `CalculateNet()` (as in **Program.cs** `ProcessPayroll`) trusts substitutability. Throwing overrides break that trust silently until runtime.

---

---

#### Q3. (R) A developer adds `Director : Manager` but the project fails to compile. Review the constructors. What is wrong with the chain, and what runs (in order) when `new Director(...)` succeeds?

```csharp
public class Person
{
    public Person(int id, string fullName) { /* sets Id, FullName */ }
}

public class Employee : Person
{
    public Employee(int id, string fullName, Department dept, Money salary)
        : base(id, fullName) { /* ... */ }
}

public class Manager : PermanentEmployee
{
    public Manager(int id, string name, Department dept, Money salary,
        Money perks, Money pf, Money teamBonus)
        : base(id, name, dept, salary, perks, pf) { /* ... */ }
}

public class Director : Manager
{
    public Director(int id, string name, Department dept, Money salary,
        Money perks, Money pf, Money teamBonus, Money boardFee)
    {
        BoardFee = boardFee;   // CS7036 — no suitable base constructor
    }

    public Money BoardFee { get; }
}
```

---

**Answer:**

**Answer:** The derived constructor must forward to an accessible base constructor with `: base(...)`; without it the compiler looks for a parameterless `Manager` constructor, finds none, and reports CS7036.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Missing `: base(id, name, dept, salary, perks, pf, teamBonus)` on `Director` | CS7036 — build blocked |
| Initialization | `BoardFee` assigned before base chain completes (if forced via workaround) | Illegal in C# — base constructors always run first |
| Hierarchy | Multilevel chain `Person → … → Manager → Director` requires each level to pass args upward | Easy to drop one salary/perk parameter when extending |

**Fix (priority order):**

1. Add explicit base forward:

```csharp
public Director(int id, string name, Department dept, Money salary,
    Money perks, Money pf, Money teamBonus, Money boardFee)
    : base(id, name, dept, salary, perks, pf, teamBonus)
{
    BoardFee = boardFee;
}
```

2. When `new Director(...)` runs successfully, constructors execute **base-first, outer-last**: `Person` → `Employee` → `PermanentEmployee` → `Manager` → `Director` body.
3. Reuse `: base(...)` in overrides like `CalculateNet()` when extending parent logic — same chaining idea as **Program.cs** `Manager.CalculateNet()` calling `base.CalculateNet()`.

**Production takeaway:** Constructor order is deterministic and non-negotiable — Karat uses multilevel payroll types to test CS7036 and whether you can narrate the chain. See **Program.cs** Sections 4 and 8.

---

---

#### Q4. (R) A refactor adds validation to the base payroll method. Contract net pay drops unexpectedly for some employees. Review the change. What broke, and how do you fix it without duplicating validation in every derived class?

```csharp
public class Employee
{
    public virtual Money CalculateNet()
    {
        ValidateNonNegative(BaseSalary);
        return BaseSalary;
    }

    protected void ValidateNonNegative(Money amount)
    {
        if (amount.Amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
    }
}

public class ContractEmployee : Employee
{
    public override Money CalculateNet()
    {
        // author assumed base still ran — only added bonus locally
        return BaseSalary + ContractBonus;
    }
}
```

---

**Answer:**

**Answer:** Derived `CalculateNet` overrides replaced the base implementation entirely, so `ValidateNonNegative` in `Employee.CalculateNet()` no longer runs — a classic **fragile base class** problem when subclasses do not call `base.CalculateNet()`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Fragile base | Base gained behavior; derived overrides bypass it | Negative or inconsistent nets slip through for contract staff |
| Correctness | `ContractEmployee` omits validation on `BaseSalary` and `ContractBonus` | Payroll audit failures; possible overpayment |
| Maintainability | Every future override must remember hidden base rules | Each new employee type is a regression vector |

**Fix (priority order):**

1. Call base from override: `return base.CalculateNet() + ContractBonus;` (after ensuring bonus validation — either in base hook or local check).
2. Prefer **Template Method**: base defines `public Money CalculateNet() { Validate...; return CalculateNetCore(); }` with `protected abstract/virtual Money CalculateNetCore()` — derived types cannot skip validation.
3. Move cross-cutting rules to non-virtual helpers invoked from a sealed `CalculateNet()` on the base if the rule must never be skipped.
4. Add tests for each derived type asserting validation runs (negative salary throws).

**Production takeaway:** Adding logic to a base `virtual` method silently breaks subclasses that fully override — the fix is structural (template method / sealed orchestrator), not "remember to call base." Mirrors **Manager** reusing permanent math via `base.CalculateNet()` in **Program.cs**.

---

---

#### Q5. (P) A teammate replaces the polymorphic payroll loop with explicit type checks "for clarity." New `ContractEmployee` rows are added to the database but never appear in the exported total. Review the method. What failed at runtime, and what pattern from this chapter should drive payroll aggregation instead?

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

---

**Answer:**

**Answer:** The `is PermanentEmployee` / `is Manager` ladder omits `ContractEmployee` (and any future sibling), so those instances contribute zero to `total` — silent underpayment, not a compile error.

- **Root cause:** Switching on concrete types duplicates dispatch the virtual table already provides; every new `Employee` subtype requires editing `ProcessPayroll`.
- **Manager branch is redundant noise:** `Manager` is a `PermanentEmployee` — if both were handled, order would matter; as written, neither contract nor many permanents may be counted correctly depending on edits.
- **Correct pattern:** Single polymorphic loop over `Employee` (or `IReadOnlyList<Employee>`) calling `employee.CalculateNet()` with `virtual`/`override` — exactly as **Program.cs** `ProcessPayroll` demonstrates in Section 9a.
- **Open/closed goal:** Add `ContractEmployee`, `PermanentEmployee`, `Manager` without changing the aggregator — new behavior lives in overrides.
- **If discrimination is truly required:** use visitor/double-dispatch or separate pipelines — not a partial `if/else` chain on siblings.

**Production takeaway:** Polymorphic collections only pay off when behavior stays on the type (`CalculateNet`, `RoleLabel`). Partial type switches fail open in finance code — totals look plausible but omit whole populations.

---

---

#### Q6. (D) Product wants `Employee` to inherit from a shared `AuditableEntity` base that already inherits `EntityBase`, while payroll still needs `Person → Employee → PermanentEmployee → Manager`. The team also proposes `Employee : Department` so every employee "is a department" for reporting. What breaks in C#, and where do LSP and fragile-base-class risks show up even if it compiles?

---

---

### 06. Abstract Classes & Interfaces

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/06. Abstract Classes & Interfaces`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** C# allows only one direct base class — you cannot chain `Person` and `AuditableEntity : EntityBase` on `Employee` without merging into one lineage or using interfaces; `Employee : Department` is a HAS-A relationship mis modeled as IS-A and invites LSP and fragile-hierarchy problems.

**Single inheritance (CS1721):**

- `class Employee : Person, AuditableEntity` does not compile — pick one base and move cross-cutting concerns to interfaces (`IAuditable`) or compose an `AuditableEntity` field.
- Deepening `Person → EntityBase → AuditableEntity → Employee → …` couples payroll to persistence auditing — changes to `AuditableEntity` (soft-delete flags, ORM hooks) ripple through every override (**fragile base class**).

**`Employee : Department` (HAS-A as IS-A):**

- Violates the chapter's composition rule: employees **have** departments (`HomeDepartment` field), they are not departments.
- LSP: code expecting a `Department` (code, name, org chart) receives an `Employee` — calling `Department` APIs on "employees" fails or returns nonsense.
- Reporting switches that pass `Department` into headcount APIs break when instances are actually people.

**Safer design:**

- Keep `Employee HAS-A Department`; share auditing via interface implementation on `Employee` or a small injected service.
- Cap hierarchy depth — prefer `Manager` bonuses over `Director : SeniorManager : …` solely for field reuse; extract shared payroll math into composable services when reuse is not IS-A.

**Production takeaway:** Inheritance is for substitutable IS-A contracts; auditing, persistence, and org structure are usually interfaces or composition. Karat ties **Program.cs** IS-A vs HAS-A (Section 2) to real hierarchy mistakes that compile only until you merge unrelated bases.

---

---

### 06. Abstract Classes & Interfaces

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/06. Abstract Classes & Interfaces`

---

---

#### Q1. (D) Your team is adding a `SpreadsheetDocument` to the document archive. It shares `Title` and `CreatedOn` with invoices and reports, but also needs optional CSV export and a separate audit trail that other document types may never use. A junior dev proposes making everything an interface:

```csharp
public interface ISpreadsheetDocument
{
    string Title { get; }
    DateTime CreatedOn { get; }
    string RenderContent();
    string Export(string format);
    void WriteAuditEntry(string action);
}
```

How would you model this using abstract classes and interfaces (as in this chapter), and why?

---

**Answer:**

```csharp
public interface ISpreadsheetDocument
{
    string Title { get; }
    DateTime CreatedOn { get; }
    string RenderContent();
    string Export(string format);
    void WriteAuditEntry(string action);
}
```

How would you model this using abstract classes and interfaces (as in this chapter), and why?

**Answer:** Keep the **IS-A** document hierarchy on an abstract `Document` base for shared state and rendering contract, then add **CAN-DO** interfaces only for optional capabilities — `IExportable` for export, a narrow `IAuditable` (or similar) for audit — instead of one fat document interface.

- **Abstract `Document`:** `Title`, `CreatedOn`, protected constructor, abstract `DocumentKind` and `RenderContent()`, plus concrete `GetSummary()` — matches **Program.cs** Sections 1 and 3; `SpreadsheetDocument : Document` reuses helpers without duplicating fields.
- **`IExportable`:** Export is a cross-cutting capability; invoices, reports, and spreadsheets can implement it without forcing audit on types that do not need it.
- **`IAuditable` (small interface):** Only types that write audit entries implement `WriteAuditEntry`; reports that never audit are not forced to stub empty methods.
- **Why not one interface:** Duplicates state across unrelated "documents," blocks multiple inheritance of implementation, and violates Interface Segregation — consumers that only export must know about audit members.
- **Both together:** `class SpreadsheetDocument : Document, IExportable, IAuditable` — single class hierarchy, multiple optional behaviors, same pattern as `InvoiceDocument : Document, IExportable, IPrintable, INamedDocument`.

**Production takeaway:** Abstract class for shared identity and partial implementation; interfaces for capabilities that cut across hierarchies. See this chapter's **Document** + **IExportable** split and foundation **Abstract class vs interface** table.

---

---

#### Q2. (R) A storage service saves file names for archived documents. After deployment, some invoices overwrite each other on disk. Review:

```csharp
public sealed class InvoiceStorageService
{
    public string ResolveFileName(InvoiceDocument invoice)
    {
        // Human-readable label for UI and logs
        return invoice.GetName();
    }

    public void Save(InvoiceDocument invoice, Stream content)
    {
        string path = Path.Combine(_root, ResolveFileName(invoice));
        using var file = File.Create(path);
        content.CopyTo(file);
    }
}
```

`InvoiceDocument` implements `INamedDocument` with explicit `string INamedDocument.GetName()` returning a file-safe name, and a public `GetName()` returning `"Invoice: " + Title`. What is wrong, and how do you fix it?

---

**Answer:**

```csharp
public sealed class InvoiceStorageService
{
    public string ResolveFileName(InvoiceDocument invoice)
    {
        // Human-readable label for UI and logs
        return invoice.GetName();
    }

    public void Save(InvoiceDocument invoice, Stream content)
    {
        string path = Path.Combine(_root, ResolveFileName(invoice));
        using var file = File.Create(path);
        content.CopyTo(file);
    }
}
```

`InvoiceDocument` implements `INamedDocument` with explicit `string INamedDocument.GetName()` returning a file-safe name, and a public `GetName()` returning `"Invoice: " + Title`. What is wrong, and how do you fix it?

**Answer:** The storage service calls the **public** `GetName()` (display label with spaces and punctuation), not the **explicit** `INamedDocument.GetName()` (file-safe slug) — two invoices with the same title collide on disk because paths are not unique or filesystem-safe.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `ResolveFileName` uses public `GetName()` instead of `INamedDocument` contract | Duplicate paths; overwrites; invalid characters on some OSes |
| API surface | Explicit implementation is invisible on concrete type | Callers assume one `GetName()` — easy to pick the wrong one |
| Design | Storage depends on concrete `InvoiceDocument` | Harder to test; wrong abstraction for "file naming" capability |

**Fix (priority order):**

1. Resolve names through the interface: `((INamedDocument)invoice).GetName()` or accept `INamedDocument` / `IFileNaming` in `ResolveFileName`.
2. Add uniqueness: append document id or hash if titles can repeat — explicit slug alone may still collide.
3. Rename public method to `GetDisplayName()` if both names must coexist on the type — reduces accidental misuse.
4. Unit-test storage with two invoices sharing a title; assert distinct file paths.

```csharp
public string ResolveFileName(INamedDocument named)
{
    return named.GetName(); // explicit implementation invoked via interface
}
```

**Production takeaway:** Explicit interface implementation exists precisely when the public API and contract differ — services must depend on the **interface variable**, as **Program.cs** Section 5 demonstrates with `namedContract.GetName()` vs `invoice.GetName()`.

---

---

#### Q3. (R) A PR introduces a "kitchen sink" capability interface for the export pipeline. Review:

```csharp
public interface IDocumentCapabilities
{
    string Export(string format);
    string Print();
    string GetName();
    byte[] RenderPdf();
    void SendToPrinter(string queueName);
    string SignWithCertificate(string thumbprint);
}

public class ExportOrchestrator
{
    public void RunBatch(IEnumerable<IDocumentCapabilities> items, string format)
    {
        foreach (var item in items)
        {
            _logger.LogInformation(item.Export(format));
        }
    }
}
```

Only invoices need signing; reports only export. What design problems do you see, and how would you refactor?

---

**Answer:**

```csharp
public interface IDocumentCapabilities
{
    string Export(string format);
    string Print();
    string GetName();
    byte[] RenderPdf();
    void SendToPrinter(string queueName);
    string SignWithCertificate(string thumbprint);
}

public class ExportOrchestrator
{
    public void RunBatch(IEnumerable<IDocumentCapabilities> items, string format)
    {
        foreach (var item in items)
        {
            _logger.LogInformation(item.Export(format));
        }
    }
}
```

Only invoices need signing; reports only export. What design problems do you see, and how would you refactor?

**Answer:** `IDocumentCapabilities` is a **fat interface** that violates the **Interface Segregation Principle** — every implementer must stub or throw for unrelated members, and callers cannot express minimal dependencies.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design (ISP) | One interface bundles export, print, PDF, signing, naming | `ReportDocument` forced to implement `SignWithCertificate` with `NotSupportedException` |
| Maintainability | New capability added to interface breaks all implementers | Package version churn; empty stubs multiply |
| Testing | Fakes must implement six methods to test export-only orchestrator | Bloated test doubles; brittle mocks |
| API clarity | `ExportOrchestrator` only needs `Export` but depends on mega-contract | Misleading type bounds; hides true requirements |

**Fix (priority order):**

1. Split into focused interfaces — `IExportable`, `IPrintable`, `ISignable`, `INamedDocument` — matching this chapter's pattern.
2. Change orchestrator signature to `IEnumerable<IExportable>` (as **ExportService.ExportAll** does).
3. Compose at call site: pass types that implement multiple interfaces; use pattern matching or separate services for signing/printing steps.
4. If a facade is needed for DI registration, use a small adapter per document type — not a monolithic interface.

**Production takeaway:** Prefer several small interfaces over one "capabilities" blob — callers depend on what they use, implementers only provide what they support. See **Program.cs** Section 4 (`InvoiceDocument` implements three interfaces, not one fat type).

---

---

#### Q4. (M) The team ships a NuGet package with `IExportable` consumed by ten internal services. To add optional metadata without breaking implementers, they add a C# 8 default method:

```csharp
public interface IExportable
{
    string Export(string format);

    string ExportWithMetadata(string format)
    {
        return Export(format) + " | exported=" + DateTime.UtcNow.ToString("O");
    }
}
```

An older service still targets `netstandard2.0` and references the updated package. A newer ASP.NET Core service on `net8.0` overrides `ExportWithMetadata` in one document type. What breaks or surprises you in build, runtime, and testing — and what would you document for consumers?

---

**Answer:**

```csharp
public interface IExportable
{
    string Export(string format);

    string ExportWithMetadata(string format)
    {
        return Export(format) + " | exported=" + DateTime.UtcNow.ToString("O");
    }
}
```

An older service still targets `netstandard2.0` and references the updated package. A newer ASP.NET Core service on `net8.0` overrides `ExportWithMetadata` in one document type. What breaks or surprises you in build, runtime, and testing — and what would you document for consumers?

**Answer:** Default interface methods require **C# 8+** and a runtime that supports them — `netstandard2.0` consumers may **fail to compile** or cannot override defaults the same way; even on modern runtimes, dispatch through the interface vs concrete type can surprise callers who expect polymorphic override behavior.

- **Build / TFM:** Default interface members are not available on older language/runtime combinations targeting pre-C#-8 projects — the package bump may block the legacy service until it retargets or the new member is moved to an extension method or separate `IExportableV2`.
- **Binary compatibility:** Adding a default method is often safer than adding a **required** abstract member (which breaks all implementers), but implementers on C# 8+ can override — document which types customize metadata vs inherit default.
- **Dispatch nuance:** Calling `ExportWithMetadata` on `IExportable` uses the most specific override on the implementing type; calling on concrete class without override uses default — tests must use the same reference type production uses.
- **Testing:** Fakes implementing `IExportable` inherit the default unless they override — unit tests may accidentally assert timestamp behavior from the default implementation instead of domain logic.
- **Alternative for wide compatibility:** Extension method `ExportWithMetadata(this IExportable e, ...)` or compositional wrapper — works on `netstandard2.0` without DIM.

**Production takeaway:** Default interface methods help evolve shared contracts with optional behavior (**Program.cs** Section 7 preview), but package authors must treat TFMs, override rules, and test doubles as part of the public API — not every consumer upgrades language version with the package.

---

---

#### Q5. (R) Unit tests for `DocumentProcessor` are slow and require real PDF files on disk because production code was wired to concrete types. Review:

```csharp
public sealed class DocumentProcessor
{
    private readonly PdfRenderer _renderer = new PdfRenderer(); // reads templates from disk

    public string BuildBatchSummary(IReadOnlyList<InvoiceDocument> documents)
    {
        var builder = new StringBuilder();
        foreach (var doc in documents)
        {
            builder.AppendLine(doc.GetSummary());
            builder.AppendLine(_renderer.Render(doc)); // not on Document base
        }
        return builder.ToString();
    }
}
```

The chapter's `DocumentProcessor` accepts `IReadOnlyList<Document>` and `ExportService` accepts `IEnumerable<IExportable>`. What is wrong here, and how would you introduce test seams?

---

**Answer:**

```csharp
public sealed class DocumentProcessor
{
    private readonly PdfRenderer _renderer = new PdfRenderer(); // reads templates from disk

    public string BuildBatchSummary(IReadOnlyList<InvoiceDocument> documents)
    {
        var builder = new StringBuilder();
        foreach (var doc in documents)
        {
            builder.AppendLine(doc.GetSummary());
            builder.AppendLine(_renderer.Render(doc)); // not on Document base
        }
        return builder.ToString();
    }
}
```

The chapter's `DocumentProcessor` accepts `IReadOnlyList<Document>` and `ExportService` accepts `IEnumerable<IExportable>`. What is wrong here, and how would you introduce test seams?

**Answer:** The processor **news up** a concrete `PdfRenderer`, accepts only `InvoiceDocument`, and mixes summary building with PDF rendering — no injection point, so tests hit disk and cannot substitute a fake.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Testability | `new PdfRenderer()` inside the class | Tests require filesystem templates; slow, flaky CI |
| Abstraction | Parameter is `InvoiceDocument` not `Document` | Cannot reuse batch logic for reports; breaks polymorphism |
| SRP / design | Summary builder coupled to PDF rendering | Changing render strategy forces retesting batch orchestration |
| DI | Hidden dependency | ASP.NET Core cannot register or swap renderer per environment |

**Fix (priority order):**

1. Extract rendering behind an interface — `IDocumentRenderer` or reuse `IExportable` / a narrow `IRenderable` with `string Render()` — inject via constructor.
2. Accept abstractions on the base type: `IReadOnlyList<Document>` for polymorphic summaries (**Program.cs** Section 6).
3. Register `PdfRenderer` in DI for production; register `FakeRenderer` in tests returning fixed strings.
4. Keep orchestration thin — `BuildBatchSummary` calls `document.GetSummary()` and `document.RenderContent()` on the abstract base where possible; PDF-specific work lives in export/render services.

```csharp
public sealed class DocumentProcessor
{
    private readonly IDocumentRenderer _renderer;

    public DocumentProcessor(IDocumentRenderer renderer) => _renderer = renderer;

    public string BuildBatchSummary(IReadOnlyList<Document> documents)
    {
        var builder = new StringBuilder();
        foreach (var doc in documents)
        {
            builder.AppendLine(doc.GetSummary());
            builder.AppendLine(_renderer.Render(doc));
        }
        return builder.ToString();
    }
}
```

**Production takeaway:** Interfaces are test seams — depend on abstractions, inject implementations. The chapter's static `DocumentProcessor` / `ExportService` illustrate the **dependency direction**; production services add constructor injection and fakes for fast tests.

---

---

#### Q6. (D) Code review: two approaches for a payment-notification feature.

**Option A — one abstract base:**

```csharp
public abstract class NotifierBase
{
    public abstract void Send(string recipient, string message);
    protected void LogAttempt(string recipient) { /* shared */ }
}
```

**Option B — interface only:**

```csharp
public interface INotificationSender
{
    void Send(string recipient, string message);
}
```

Some notifiers are `EmailNotifier : NotifierBase`; others are `SmsNotifier : INotificationSender` with no shared base. When do you pick abstract class, interface, or both — and what is your decision rule for this codebase?

---

---

### 07. Encapsulation & Access Modifiers

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/07. Encapsulation & Access Modifiers`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Option A — one abstract base:**

```csharp
public abstract class NotifierBase
{
    public abstract void Send(string recipient, string message);
    protected void LogAttempt(string recipient) { /* shared */ }
}
```

**Option B — interface only:**

```csharp
public interface INotificationSender
{
    void Send(string recipient, string message);
}
```

Some notifiers are `EmailNotifier : NotifierBase`; others are `SmsNotifier : INotificationSender` with no shared base. When do you pick abstract class, interface, or both — and what is your decision rule for this codebase?

**Answer:** Use an **abstract base** when notifiers truly share state or concrete helpers (logging, retry policy, template loading); use an **interface** when the only contract is "can send" across unrelated types; use **both** when shared infrastructure belongs in a base but multiple channels must also be substitutable in DI and tests.

**Decision rule (aligned with this chapter):**

| Signal | Choose |
|---|---|
| Shared fields, protected helpers, single IS-A hierarchy | Abstract class (`NotifierBase`) |
| Unrelated types (email, SMS, webhook) must be swappable | `INotificationSender` interface |
| Shared logging/retry **and** need multiple inheritance of behavior | Base class for shared code + `INotificationSender` implemented by base or subclasses |
| Only some notifiers support attachments/signing | Separate small interfaces — do not bloated base |

**For this codebase:**

- **`INotificationSender`** for DI registration, controllers, and unit tests — same role as `IExportable` in **ExportService**.
- **`NotifierBase`** only if most channels share `LogAttempt`, correlation id, or configuration — avoid forcing SMS through an email-centric hierarchy.
- **`SmsNotifier : INotificationSender`** without base is valid when there is nothing to share — do not invent an abstract class for one method.

**Production takeaway:** Abstract class answers "what are they in common?" Interface answers "what can they do for me?" The chapter's **Document** + **IExportable** combination is the template — base for identity, interfaces for pluggable capabilities and test doubles.

---

---

### 07. Encapsulation & Access Modifiers

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/07. Encapsulation & Access Modifiers`

---

---

#### Q1. (R) A junior developer "simplifies" the chapter's `BankAccount` for a payments microservice. QA reports negative balances in production. Review the change — what broke the invariant, and how do you fix it?

```csharp
public class BankAccount
{
    public decimal Balance { get; set; }
    public string AccountNumber { get; }

    public BankAccount(string accountNumber, decimal openingDeposit)
    {
        AccountNumber = accountNumber;
        Balance = openingDeposit;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Balance += amount;
    }

    public bool TryWithdraw(decimal amount, out string message)
    {
        if (amount <= 0) { message = "Amount must be positive."; return false; }
        if (amount > Balance) { message = "Insufficient funds."; return false; }
        Balance -= amount;
        message = "OK";
        return true;
    }
}

// Elsewhere in the API layer:
account.Balance = -10_000m;   // "adjustment" from a support script
account.Balance += 999m;      // race between two threads — no lock
```

---

**Answer:**

**Answer:** Exposing `Balance` as a public setter lets any caller bypass `TryWithdraw` and `Deposit` rules — the type no longer owns its invariant, so external code and concurrent writers can corrupt state even though the methods still look correct.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Public `{ get; set; }` on domain state | Callers assign `Balance` directly — negative balances, skipped validation |
| Concurrency | Read-modify-write on public property | Two threads can interleave `Balance +=` / `-=` without synchronization |
| Encapsulation | Methods enforce rules; property ignores them | `TryWithdraw` checks become advisory — support scripts and mappers bypass them |
| API contract | Mutable balance contradicts audit expectations | Ledger reconciliation finds amounts that never passed `Deposit`/`TryWithdraw` |

**Fix (priority order):**

1. Restore a **private** backing field (`private decimal _balance`) — no public setter.
2. Expose balance read-only: `public decimal Balance => _balance;` or keep `GetBalance()` — callers observe, they do not mutate.
3. Route all changes through methods that enforce invariants (positive deposits, sufficient funds, audit logging).
4. If external systems must post adjustments, add an explicit `ApplyAdjustment(decimal amount, string reason, IAuthorizationContext ctx)` that validates authorization — never a bare setter.
5. For concurrent updates, guard mutations with a lock, database transaction, or optimistic concurrency token — encapsulation alone does not fix races.

```csharp
private decimal _balance;

public decimal Balance => _balance;

public bool TryWithdraw(decimal amount, out string message)
{
    // sole path to decrease _balance
}
```

**Production takeaway:** Auto-properties feel idiomatic in C#, but a public setter on invariant-bearing state is a field in disguise — Karat uses this to test whether you protect rules at the type boundary, not only inside "happy path" methods. See **Program.cs** Section 1 — `BankAccount` keeps `_balance` private.

---

---

#### Q2. (R) A shared library ships both a public façade and internal implementation types. A consuming team references the NuGet package and complains they cannot unit-test ledger entries. Review the library surface:

```csharp
// Payments.Core.dll
internal class InternalLedger
{
    public List<string> Entries { get; } = new();
    public void Record(string description) => Entries.Add(description);
}

public class LedgerGateway
{
    public static InternalLedger CreateLedger() => new InternalLedger();

    public static string PostEntry(InternalLedger ledger, string description)
    {
        ledger.Record(description);
        return ledger.Entries[^1];
    }
}
```

What is wrong with this public API shape, and how would you redesign the assembly boundary?

---

**Answer:**

**Answer:** The library leaked an `internal` type through public method signatures — `CreateLedger` and `PostEntry` expose `InternalLedger` on the public API, which is a compile error for external consumers and breaks the intended assembly boundary even if it compiled.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `public` methods return/accept `internal` type | CS0051/CS0052 — inconsistent accessibility; package may not build |
| API surface | Internal implementation type is part of public contract | Callers depend on types you intended to hide — versioning nightmare |
| Encapsulation | Façade pattern inverted — gateway exposes guts | `List<string> Entries` on internal type becomes reachable if accessibility bug is fixed |
| Testing | Consumers cannot construct `InternalLedger` | Tests forced to go through static gateway — brittle, no seam for fakes |

**Fix (priority order):**

1. Keep `InternalLedger` **internal**; never appear in public signatures.
2. Introduce a **public** abstraction: `public interface ILedger { string PostEntry(string description); }` implemented internally, or return `string`/`LedgerEntryId` DTOs only.
3. `LedgerGateway.PostEntry(string description)` creates the internal ledger internally — matches **Program.cs** Section 3 pattern.
4. For testability inside the library, use `InternalsVisibleTo` for test assembly **or** expose `ILedger` with an internal default implementation registered via DI.
5. Return immutable snapshots (`IReadOnlyList<LedgerEntry>`) rather than live `List<T>` references.

```csharp
public static class LedgerGateway
{
    public static string PostEntry(string description)
    {
        var ledger = new InternalLedger();
        return ledger.Record(description);
    }
}
```

**Production takeaway:** `internal` types belong behind public façades — leaking them in signatures is worse than making everything public because it fails at compile time and signals unclear API design. See **Program.cs** Section 3 — `LedgerGateway` hides `InternalLedger`.

---

---

#### Q3. (P) Two assemblies in the same solution — `Billing.Core` (library) and `Billing.Tests` — need test access to `internal` pricing helpers without exposing them on the public NuGet surface. A developer adds this to `Billing.Core.csproj`:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="Billing.Tests" />
</ItemGroup>
```

What does `InternalsVisibleTo` actually grant, what risks does it introduce if misused, and what guardrails would you apply before adding friend assemblies in a production codebase?

---

**Answer:**

_Answer not found._

---

#### Q4. (R) A domain hierarchy models employee compensation. A subclass "optimizes" payroll by writing directly to protected state. Review:

```csharp
public abstract class Employee
{
    protected decimal _baseSalary;
    protected List<string> _auditTrail = new();

    protected Employee(decimal baseSalary)
    {
        _baseSalary = baseSalary;
        _auditTrail.Add($"Hired at {_baseSalary:C}");
    }

    public decimal GetBaseSalary() => _baseSalary;

    public virtual void ApplyRaise(decimal percent)
    {
        if (percent <= 0) throw new ArgumentOutOfRangeException(nameof(percent));
        _baseSalary *= (1 + percent / 100m);
        _auditTrail.Add($"Raise {percent}% applied");
    }
}

public class CommissionEmployee : Employee
{
    public CommissionEmployee(decimal baseSalary) : base(baseSalary) { }

    public void SetGuaranteedMinimum(decimal minimum)
    {
        _baseSalary = minimum;           // bypasses ApplyRaise validation/audit
        _auditTrail.Clear();             // hides history from HR reports
    }
}
```

What encapsulation failure does `protected` enable here, and how would you protect invariants for derived types?

---

**Answer:**

**Answer:** `protected` fields expose implementation details to every derived class — `CommissionEmployee` can mutate `_baseSalary` and `_auditTrail` without going through `ApplyRaise`, breaking payroll invariants and audit integrity that the base class thought it owned.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encapsulation | `protected` fields instead of controlled hooks | Subclasses bypass validation, logging, and business rules |
| Invariant | Direct `_baseSalary` assignment | Raises applied without approval workflow; minimum wage rules skipped |
| Audit | `_auditTrail.Clear()` | HR/compliance reports lose history — protected mutable collection |
| Design | Base class cannot enforce postconditions on derived behavior | Liskov violations — base assumes audit trail is append-only |

**Fix (priority order):**

1. Make fields **private**; stop exposing raw state to subclasses.
2. Expose controlled extension points: `protected void SetBaseSalary(decimal value, string reason)` that validates and appends audit entries — or make `ApplyRaise` `sealed`/`non-virtual` and use template method with protected abstract hooks that cannot touch salary directly.
3. Return **read-only** audit view: `public IReadOnlyList<string> AuditTrail => _auditTrail.AsReadOnly();` — never expose mutable `List<T>` as `protected`.
4. If derived types need guaranteed minimum, model it as separate state (`_guaranteedMinimum`) combined in a computed `EffectiveBaseSalary` — not by overwriting `_baseSalary`.
5. Consider `private protected` only for same-assembly inheritance helpers — not as a substitute for private fields.

```csharp
private decimal _baseSalary;
private readonly List<string> _auditTrail = new();

protected void AdjustBaseSalary(decimal newSalary, string reason)
{
    if (newSalary < 0) throw new ArgumentOutOfRangeException(nameof(newSalary));
    _baseSalary = newSalary;
    _auditTrail.Add(reason);
}
```

**Production takeaway:** `protected` is not "private but inheritance-friendly" for invariant-bearing data — it is a public API for every future subclass. See **Program.cs** Section 2 — `protected` visibility and Section 1 — behavior enforces invariants, not exposed fields.

---

---

#### Q5. (D) Your team designs an immutable `MemberProfile` DTO for cross-service messaging (similar to this chapter's `MemberProfile`). Two proposals:

**Proposal A — all init-only, mutable collection inside:**

```csharp
public sealed class MemberProfileDto
{
    public string MemberId { get; init; }
    public string Email { get; init; }
    public List<string> Roles { get; init; } = new();
}
```

**Proposal B — private ctor + factory + read-only surface:**

```csharp
public sealed class MemberProfileDto
{
    public string MemberId { get; }
    public string Email { get; }
    public IReadOnlyList<string> Roles { get; }

    private MemberProfileDto(string memberId, string email, IReadOnlyList<string> roles) { ... }

    public static MemberProfileDto Create(string memberId, string email, IEnumerable<string> roles) { ... }
}
```

Which approach would you ship for a message contract shared between three services, and why? What breaks if callers treat Proposal A as immutable?

---

**Answer:**

_Answer not found._

---

#### Q6. (M) A plugin assembly (`Plugins.Payroll`) references your core HR assembly and defines `PayrollProcessor : Employee`. Developers expect to read `InternalCounter` on a base instance from the plugin, but the build fails with CS0122. Given this base class from the chapter:

```csharp
public class VisibilityBase
{
    internal int InternalCounter = 3;
    protected internal int ProtectedInternalCounter = 4;
    private protected int PrivateProtectedCounter = 5;
}
```

Explain why each of the three counters behaves differently from a **derived class in another assembly**, and which modifier you would choose for a hook intended only for first-party plugins compiled into the same assembly as the base.

---

---

### 08. Events

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/08. Events`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

_Answer not found._

---

#### Q1. (R) A WPF-style desktop app keeps growing in memory after users open and close account detail panels. Review this wiring. What keeps `AccountDetailPanel` instances alive, and how do you fix it?

```csharp
public sealed class AccountDetailPanel : IDisposable
{
    private readonly BankAccount _account;

    public AccountDetailPanel(BankAccount account)
    {
        _account = account;
        _account.BalanceChanged += (_, e) =>
            RefreshBalanceLabel(e.NewBalance);
    }

    public void Dispose() { /* panel removed from UI */ }

    private void RefreshBalanceLabel(decimal balance) { /* update UI */ }
}

// Caller creates panels on navigation:
var panel = new AccountDetailPanel(sharedAccount);
// ... user navigates away; panel.Dispose() called but memory does not drop
```

---

**Answer:**

**Answer:** The panel subscribes to `_account.BalanceChanged` with a lambda but never unsubscribes in `Dispose`, so the long-lived `BankAccount` publisher holds a delegate that captures `this` — the closed panel cannot be collected even after it is removed from the UI.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | `+=` in constructor, no `-=` in `Dispose` | Publisher retains subscriber → memory leak |
| Handler target | Lambda captures `this` (the panel instance) | GC cannot reclaim disposed UI objects |
| Design | Shared singleton/static `BankAccount` outlives every panel | Leak accumulates on each navigation open/close |

**Fix (priority order):**

1. Unsubscribe in `Dispose` (or `IAsyncDisposable`) — store the handler in a field if you used a lambda so `-=` matches the same delegate instance.
2. Prefer a named instance method handler when possible: `_account.BalanceChanged += OnBalanceChanged;` and `-= OnBalanceChanged` in `Dispose`.
3. If the publisher outlives all subscribers, consider weak-event patterns or a mediator (`IMediator`, `Channel<T>`) for UI refresh instead of direct domain events.
4. Profile with a memory dump — look for `AccountDetailPanel` instances retained via `BankAccount` → multicast delegate chain.

```csharp
private readonly EventHandler<BalanceChangedEventArgs> _balanceHandler;

public AccountDetailPanel(BankAccount account)
{
    _account = account;
    _balanceHandler = (_, e) => RefreshBalanceLabel(e.NewBalance);
    _account.BalanceChanged += _balanceHandler;
}

public void Dispose()
{
    _account.BalanceChanged -= _balanceHandler;
}
```

**Production takeaway:** Events create implicit references from publisher to subscriber — Karat tests whether you treat `-=` as mandatory cleanup, not optional. See **Program.cs** Section 6 — subscribe/unsubscribe and **Section 4c** — publisher outlives handlers.

---

---

#### Q2. (R) After a refactor, balance notifications crash when no UI is subscribed. Review the publisher change:

```csharp
public class BankAccount
{
    public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;

    protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
    {
        if (BalanceChanged != null)
        {
            BalanceChanged(this, e);  // was: BalanceChanged?.Invoke(this, e);
        }
    }
}
```

What breaks at runtime, and what is the idiomatic raise pattern in modern C#?

---

**Answer:**

```csharp
public class BankAccount
{
    public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;

    protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
    {
        if (BalanceChanged != null)
        {
            BalanceChanged(this, e);  // was: BalanceChanged?.Invoke(this, e);
        }
    }
}
```

What breaks at runtime, and what is the idiomatic raise pattern in modern C#?

**Answer:** The null check and invoke are not atomic — another thread can unsubscribe between the `!= null` test and the call, leaving `BalanceChanged` null and throwing `NullReferenceException`. The idiomatic fix is null-conditional invoke: `BalanceChanged?.Invoke(this, e)`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Split null-check + direct invoke | Rare NRE when last handler unsubscribes during raise |
| Style | Verbose `if (BalanceChanged != null)` | Easy to regress during refactor away from `?.` |
| Threading | Non-atomic check-then-invoke | Same race as Q4; worse under concurrent UI/service threads |

**Fix (priority order):**

1. Restore null-conditional invoke inside `OnBalanceChanged`: `BalanceChanged?.Invoke(this, e);`
2. For multi-threaded publishers, copy to a local before invoke (see Q4): `var handler = BalanceChanged; handler?.Invoke(this, e);`
3. Keep raise logic centralized in `OnBalanceChanged` so derived classes override one hook — matches **Program.cs** Section 4c.
4. Add a unit test that unsubscribes a handler from inside another handler — reproduces the race without UI.

**Production takeaway:** Forgetting `?.` is a classic production footgun — zero subscribers is normal, not exceptional. See **Program.cs** QUICK REFERENCE — "Forgetting ?. before Invoke → NullReferenceException."

---

---

#### Q3. (R) A teammate exposes a notification hook as a public delegate field "for flexibility." Review usage from another assembly:

```csharp
public class PaymentGateway
{
    public Action<string>? PaymentCompleted;  // public field, not event
}

// Consumer startup:
gateway.PaymentCompleted += msg => _audit.Log(msg);

// Later, a test helper "resets" listeners before each test:
gateway.PaymentCompleted = null;

// Malicious or buggy caller in another module:
gateway.PaymentCompleted?.Invoke("Fake payment — ship order");
```

What production risks does this design create compared to `public event Action<string>? PaymentCompleted`?

---

**Answer:**

```csharp
public class PaymentGateway
{
    public Action<string>? PaymentCompleted;  // public field, not event
}

// Consumer startup:
gateway.PaymentCompleted += msg => _audit.Log(msg);

// Later, a test helper "resets" listeners before each test:
gateway.PaymentCompleted = null;

// Malicious or buggy caller in another module:
gateway.PaymentCompleted?.Invoke("Fake payment — ship order");
```

What production risks does this design create compared to `public event Action<string>? PaymentCompleted`?

**Answer:** A public delegate field lets any caller invoke the callback chain or assign `null`, wiping every subscriber without their knowledge — breaking audit trails, tests, and domain integrity. The `event` keyword restricts outsiders to `+=` / `-=` only; only `PaymentGateway` may raise from inside the type (CS0070 blocks external `Invoke`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security / integrity | External `Invoke` fakes domain events | Downstream systems act on spoofed "payment completed" |
| Encapsulation | `= null` clears entire multicast chain | Silent loss of audit/logging handlers after test reset or bug |
| API contract | Callers cannot distinguish publisher vs subscriber responsibilities | Violates publisher/subscriber roles from **Program.cs** Section 1 |
| Compile-time safety | No CS0070 guard on external raise | Fake notifications ship to production undetected |

**Fix (priority order):**

1. Change to `public event Action<string>? PaymentCompleted;` and raise only from an internal `Publish(string message)` method.
2. Replace test `= null` with explicit `-=` per registered handler, or create a fresh gateway instance per test.
3. For cross-assembly extensibility, prefer interfaces + DI (`INotificationPublisher`) over exposed delegate fields.
4. Code-review rule: flag `public Action`/`Func` fields on domain types — require `event` or method-based hooks.

```csharp
public class PaymentGateway
{
    public event Action<string>? PaymentCompleted;

    public void CompletePayment(string receiptId)
    {
        // real gateway work...
        PaymentCompleted?.Invoke(receiptId);
    }
}
```

**Production takeaway:** **Program.cs** Section 5 — `UnsafeNotifier` vs `SafeNotifier` — same lesson at enterprise scale: events protect who may raise and who may clear subscribers.

---

---

#### Q4. (P) A background `BankAccount` service raises `BalanceChanged` from worker threads while the UI thread subscribes handlers. A developer uses only null-conditional invoke inside `OnBalanceChanged`:

```csharp
protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
{
    BalanceChanged?.Invoke(this, e);
}
```

Under concurrent subscribe/unsubscribe, handlers are occasionally skipped or you see rare `NullReferenceException` in older .NET code paths. Explain the race and show the thread-safe raise pattern from this chapter.

---

**Answer:**

```csharp
protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
{
    BalanceChanged?.Invoke(this, e);
}
```

Under concurrent subscribe/unsubscribe, handlers are occasionally skipped or you see rare `NullReferenceException` in older .NET code paths. Explain the race and show the thread-safe raise pattern from this chapter.

**Answer:** `BalanceChanged?.Invoke` still reads the event field twice conceptually — between load and invoke another thread can `-=` the last handler and set the backing delegate to null, so some handlers never run or an older pattern throws. Copy the delegate reference to a local variable, then null-conditional invoke the copy so the invocation list is fixed for that raise.

- **Race:** Thread A loads non-null delegate → Thread B unsubscribes last handler (field becomes null) → Thread A invokes — skipped notification or NRE with explicit null-check code.
- **Thread-safe pattern (from this chapter):**

```csharp
protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
{
    EventHandler<BalanceChangedEventArgs>? handler = BalanceChanged;
    handler?.Invoke(this, e);
}
```

- **Why it works:** The local `handler` captures the multicast delegate snapshot at raise time; subsequent `+=`/`-=` on the event do not affect that snapshot.
- **Stronger option:** Custom `add`/`remove` accessors with a lock if subscribe/unsubscribe must be synchronized with raise — **Program.cs** Section 4d; default compiler accessors are usually enough once you copy locally.
- **UI note:** Even with a safe raise, handlers that touch UI controls must marshal to the UI thread (`Dispatcher`, `SynchronizationContext`) — thread-safe raise does not make handler bodies thread-safe.

**Production takeaway:** Null-conditional invoke fixes "no subscribers"; local copy fixes "subscribers changed mid-raise" — Karat stacks both. See **BankAccount.OnBalanceChanged** in **Program.cs** lines 215–219.

---

---

#### Q5. (P) An ASP.NET Core API registers a **Singleton** `OrderStateTracker` that exposes `event EventHandler<OrderPlacedEventArgs>? OrderPlaced`. Scoped services subscribe in their constructors to push SignalR updates. After a few thousand requests, memory climbs and old connections still receive events. What is wrong with this wiring, and what pattern replaces in-process events for web apps?

```csharp
builder.Services.AddSingleton<OrderStateTracker>();
builder.Services.AddScoped<OrderNotificationService>();

public sealed class OrderNotificationService
{
    public OrderNotificationService(OrderStateTracker tracker, IHubContext<OrderHub> hub)
    {
        tracker.OrderPlaced += async (_, e) =>
            await hub.Clients.All.SendAsync("orderPlaced", e.OrderId);
    }
}
```

---

**Answer:**

**Answer:** A singleton publisher lives for the app lifetime, but each scoped `OrderNotificationService` subscribes in its constructor and never unsubscribes — every request adds another handler to the same event, retaining disposed scopes, `IHubContext` captures, and stale SignalR targets until the process recycles.

- **DI lifetime mismatch:** Singleton event source + scoped subscriber constructor subscription = unbounded handler list growth per HTTP request.
- **Memory:** Each handler closes over `hub` and possibly request state — GC cannot collect completed requests still referenced by the delegate chain.
- **Correctness:** Old handlers fire on new orders — clients see duplicate or ghost notifications from recycled connection ids.

**Fix (priority order):**

1. **Do not** subscribe in scoped service constructors to singleton events without matching `-=` in `Dispose`/`IAsyncDisposable` — hard to get right in ASP.NET.
2. Prefer **`IOptions` + `IHostedService`**, a **singleton** broadcaster with explicit connection mapping, or **`IHubContext` injected into a singleton** that tracks groups — not per-request event handlers.
3. For domain decoupling in ASP.NET Core, use **`IMediator` (MediatR)**, **`Channel<T>`**, or **message bus** (Azure Service Bus, RabbitMQ) scoped to the unit of work — not classic C# events across DI lifetimes.
4. If events are required (e.g., `DbContext.SaveChanges` interceptors), keep subscriber lifetime **≤ publisher lifetime** and unsubscribe when scope ends.

```csharp
// Better: scoped handler invoked explicitly from application service, no singleton event
public sealed class OrderApplicationService
{
    private readonly IHubContext<OrderHub> _hub;
    public async Task PlaceOrderAsync(Order order, CancellationToken ct)
    {
        // persist order...
        await _hub.Clients.Group(order.CustomerId).SendAsync("orderPlaced", order.Id, ct);
    }
}
```

**Production takeaway:** C# events assume you manage lifetimes manually — ASP.NET DI scopes do not auto-unsubscribe. Karat links **Events** to **DI lifetimes**: singleton + scoped event wiring is a production leak. Preview: **Program.cs** Section 6 — multi-handler wiring moves to ch.09 with service registration.

---

---

#### Q6. (D) Your team debates three ways to notify downstream code when `BankAccount` balance changes: (A) `public event EventHandler<T>`, (B) `public Action<T>?` callback field, (C) `INotificationService` injected and called directly from `Deposit`/`TryWithdraw`. When would you choose each in a production ASP.NET Core domain layer, and what is the unsubscribe/lifetime rule of thumb?

---

### 09. OOP Real-World Examples

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/09. OOP Real-World Examples`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** In ASP.NET Core domain services, prefer **(C) injected abstractions** for application boundaries; use **(A) events** for in-process, same-lifetime object graphs (UI controls, short-lived aggregates with explicit cleanup); avoid **(B) public delegate fields** in production domain code except internal test doubles.

| Option | When to use | Lifetime rule |
|---|---|---|
| **(A) `event`** | Same-assembly domain objects, UI binding, aggregates where subscribers share publisher lifetime | Every `+=` needs matching `-=` when subscriber dies first; publisher must outlive or use weak patterns |
| **(B) `Action` field** | Rare — single callback slot, prototype code, serializer-friendly delegates you control entirely | Same as (A), plus anyone can `= null` or invoke — not for public APIs |
| **(C) `INotificationService` / MediatR** | ASP.NET Core services, cross-layer notifications, testability, multiple implementations | DI scope owns lifetime — no manual unsubscribe; singleton must not capture scoped services |

**Production guidance:**

- **Domain layer in API:** `BankAccount` should not expose public events to the web stack — call `INotificationService.PublishBalanceChanged(...)` from application services after persistence so lifetimes follow the request scope.
- **Console/UI tools:** Events match **Program.cs** tutorial — `BankAccount` + handlers in `Main` with clear subscribe/unsubscribe demo.
- **Testing:** (C) is easiest to mock; (A) requires raising events or attaching test handlers with cleanup; (B) invites test code that clears production handlers with `= null`.
- **Rule of thumb:** If the subscriber has a **shorter lifetime than the publisher**, you must unsubscribe — or do not use events. If lifetimes are managed by DI, use interfaces instead of events.

**Production takeaway:** Events excel at decoupling within one process and one lifetime story; ASP.NET Core's scoped/singleton graph breaks that assumption — Karat expects you to pick the mechanism by **who raises, who listens, and who outlives whom**, not syntax preference alone.

---

### 09. OOP Real-World Examples

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/09. OOP Real-World Examples`

---

---

#### Q1. (R) A team ports the chapter's order-fulfillment payment flow into a service class. Support sees duplicate debits and failed rollbacks after card declines. Review:

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

What is wrong across encapsulation, abstraction, and correctness — and how would you fix it in priority order?

---

**Answer:**

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

What is wrong across encapsulation, abstraction, and correctness — and how would you fix it in priority order?

**Answer:** The service debits the wallet before any gateway succeeds, then infers payment outcome from a formatted string and silently retries a second gateway — so a declined card can still leave the customer charged twice or in an inconsistent ledger state. It also hard-codes concrete processors instead of depending on the chapter's `PaymentProcessor` abstraction.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Withdraw **before** confirmed charge; fallback charges a **second** gateway after partial success | Duplicate debits, reconciliation nightmares, support tickets |
| Encapsulation | Ignores `TryWithdraw` result (`out _` discarded); balance check + withdraw not atomic with payment | Race conditions; withdraw can fail while flow continues |
| Abstraction / DIP | `new CardPaymentProcessor()` / `new WalletPaymentProcessor()` inside method | Cannot swap gateways, mock in tests, or extend without editing this class (OCP) |
| Design | Parses `"declined"` from human-readable `ProcessOrderPayment` string | Fragile coupling to message text; breaks localization or logging changes |
| Domain | No idempotency on `orderRef` | Retries double-charge the same order |

**Fix (priority order):**

1. **Stop debiting before payment succeeds** — call `PaymentProcessor.TryCharge` (or gateway API) first; only `TryWithdraw` / ledger debit after confirmed charge, inside one transactional boundary (DB transaction or saga with compensating action).
2. **Inject `PaymentProcessor` (or strategy per payment method)** — caller or factory selects one processor per order; do not sequentially hammer two gateways on one decline string match.
3. **Use structured results** — return `bool` / result type from charge APIs, not `Contains("declined")` on formatted strings.
4. **Respect `TryWithdraw` outcome** and frozen-account rules from chapter `BankAccount` — propagate `errorMessage`; never ignore `out` parameters.
5. Add **idempotency key** on `orderRef` so retries are safe.

```csharp
public sealed class OrderPaymentService
{
    private readonly PaymentProcessor _processor;

    public OrderPaymentService(PaymentProcessor processor) => _processor = processor;

    public bool Run(BankAccount wallet, decimal total, string orderRef, out string message)
    {
        if (!_processor.TryCharge(total, orderRef))
        {
            message = _processor.ProcessOrderPayment(total, orderRef);
            return false;
        }

        if (!wallet.TryWithdraw(total, out message))
        {
            // Compensating refund/charge reversal on gateway
            return false;
        }

        message = _processor.ProcessOrderPayment(total, orderRef);
        return true;
    }
}
```

**Production takeaway:** The chapter separates **encapsulated ledger rules** (`BankAccount`) from **hidden gateway logic** (`PaymentProcessor`) — Karat stacks them to see if you preserve invariants when wiring a "real" service. See **Program.cs** Sections 2–3 — `TryWithdraw` + `ProcessOrderPayment`.

---

---

#### Q2. (R) A logistics API quotes delivery cost from the chapter's `Vehicle` fleet. After adding `Motorcycle` to the fleet, quotes are wrong and every new vehicle type requires editing this method. Review:

```csharp
public static decimal QuoteDelivery(Vehicle vehicle, decimal distanceKm, decimal ratePerKm)
{
    if (vehicle is Car)
        return distanceKm * ratePerKm;

    if (vehicle is Truck truck)
        return distanceKm * ratePerKm * (1.0m + truck.PayloadTons * 0.05m);

    // Fallback for anything else (Motorcycle, future types)
    return distanceKm * ratePerKm * 2.0m;
}
```

What design problems do you see, and how does the chapter's polymorphism model replace this?

---

**Answer:**

```csharp
public static decimal QuoteDelivery(Vehicle vehicle, decimal distanceKm, decimal ratePerKm)
{
    if (vehicle is Car)
        return distanceKm * ratePerKm;

    if (vehicle is Truck truck)
        return distanceKm * ratePerKm * (1.0m + truck.PayloadTons * 0.05m);

    // Fallback for anything else (Motorcycle, future types)
    return distanceKm * ratePerKm * 2.0m;
}
```

What design problems do you see, and how does the chapter's polymorphism model replace this?

**Answer:** The method re-implements pricing with type tests and a punitive default multiplier, so `Motorcycle` quotes are wrong and every new `Vehicle` subtype forces another branch — exactly what polymorphic `EstimateDeliveryCostKm` on the chapter's hierarchy avoids.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Polymorphism | Ignores `Vehicle.EstimateDeliveryCostKm` override on `Truck` | Duplicated / divergent pricing logic; truck payload formula may drift from domain |
| OCP | Central `is` / `if` chain | New vehicle types require editing shared utility — merge conflicts, missed cases |
| LSP / correctness | `2.0m` fallback for unknown types | Motorcycles overcharged; silent wrong quotes in production |
| Maintainability | `Car` branch duplicates base `ratePerKm * 1.0m` | Two places to change base rate logic |

**Fix (priority order):**

1. Replace the method body with **`vehicle.EstimateDeliveryCostKm(ratePerKm) * distanceKm`** — one line using runtime dispatch.
2. Override `EstimateDeliveryCostKm` on subtypes that differ (`Truck` already does); leave `Car` / `Motorcycle` on base behavior or add precise overrides.
3. Delete the fallback multiplier — if a new type needs special pricing, add a derived class override instead of editing a god-method.
4. Accept `Vehicle` (or `IReadOnlyList<Vehicle>`) in fleet APIs so callers never downcast for pricing.

**Production takeaway:** Chapter Section 4–5 shows **virtual override + base reference** so fleet loops stay branch-free — Karat uses logistics quoting to test whether you reach for `is` checks after learning polymorphism. See **Program.cs** — `deliveryVehicle.EstimateDeliveryCostKm(ratePerKm)`.

---

---

#### Q3. (R) A PR consolidates payment, delivery, labels, notifications, and invoicing into one coordinator for "simplicity." Review:

```csharp
public sealed class OrderFulfillmentHub
{
    public BankAccount CustomerWallet { get; set; } = new("ACC-DEFAULT", 0m);

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

Identify stacked OOP and SOLID issues. What would you split, inject, or abstract first?

---

**Answer:**

```csharp
public sealed class OrderFulfillmentHub
{
    public BankAccount CustomerWallet { get; set; } = new("ACC-DEFAULT", 0m);

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

Identify stacked OOP and SOLID issues. What would you split, inject, or abstract first?

**Answer:** One class owns mutable shared wallet state, hard-coded collaborators, duplicated shape math, and a string-concatenated API response — violating SRP and DIP while bypassing the chapter's interface and polymorphism seams.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| SRP | Payment, delivery, labeling, notify, invoice in one method | Untestable blob; any change risks regressions everywhere |
| Encapsulation | Public `CustomerWallet` setter + default account | Any caller can swap or corrupt shared wallet; multi-tenant bleed |
| DIP / abstraction | `new CardPaymentProcessor`, `new EmailNotificationSender`, inline `Circle` math | No injection; cannot add SMS/Push or swap truck without editing hub |
| Polymorphism | Recomputes circle area instead of `circle.Area` / `Shape.Draw()` | Duplicated domain logic; breaks when label rules change |
| Correctness | Withdraw + charge ordering (same as Q1) | Financial inconsistency |
| API design | Returns opaque concatenated string | Callers cannot compose invoice PDF, audit log, or HTTP 201 body cleanly |

**Fix (priority order):**

1. **Extract orchestrator** that accepts dependencies — `PaymentProcessor`, `Vehicle` (or fleet service), `IReadOnlyList<Shape>`, `IEnumerable<INotificationSender>`, `Document` factory — constructor injection.
2. **Remove mutable shared `BankAccount` property** — pass per-order wallet/account id into `Fulfill`; load scoped instance per request.
3. **Use chapter contracts** — `NotifyCustomer(senders, …)` pattern from **Program.cs**; `RenderLabels` / `SumAreas` for shapes; `invoice.Render()` as sole document output, map to DTO separately.
4. Split **domain services** — `OrderPaymentService`, `DeliveryQuoteService`, `NotificationService` — orchestrator coordinates; each unit-tested.
5. Return a **structured result** (payment status, delivery cost, notification receipts, invoice text) — not one mega-string.

**Production takeaway:** The chapter's `Main` intentionally orchestrates for learning — production code inverts that into injected abstractions. Karat capstone tests whether you recognize demo-style composition vs shippable boundaries.

---

---

#### Q4. (D) Product wants **push notifications** and a shared **retry-with-backoff** helper for all channels. Two proposals land in code review:

**Option A — extend abstract base:**

```csharp
public abstract class NotificationSenderBase
{
    protected void Retry(Action sendAttempt) { /* shared retry */ }
    public abstract string Send(string recipient, string message);
}

public class PushNotificationSender : NotificationSenderBase { /* ... */ }
```

**Option B — keep chapter interface + optional helper:**

```csharp
public interface INotificationSender
{
    string ChannelName { get; }
    string Send(string recipient, string message);
}

public static class NotificationRetry
{
    public static string SendWithRetry(INotificationSender sender, string recipient, string message) { /* ... */ }
}
```

Email and SMS already implement `INotificationSender` with no common base. Which direction fits this chapter's fulfillment model, and when would you combine both?

---

**Answer:**

**Option A — extend abstract base:**

```csharp
public abstract class NotificationSenderBase
{
    protected void Retry(Action sendAttempt) { /* shared retry */ }
    public abstract string Send(string recipient, string message);
}

public class PushNotificationSender : NotificationSenderBase { /* ... */ }
```

**Option B — keep chapter interface + optional helper:**

```csharp
public interface INotificationSender
{
    string ChannelName { get; }
    string Send(string recipient, string message);
}

public static class NotificationRetry
{
    public static string SendWithRetry(INotificationSender sender, string recipient, string message) { /* ... */ }
}
```

Email and SMS already implement `INotificationSender` with no common base. Which direction fits this chapter's fulfillment model, and when would you combine both?

**Answer:** Prefer **Option B** — keep `INotificationSender` and add `PushNotificationSender : INotificationSender`, with retry as a cross-cutting helper or decorator — because email and SMS are unrelated types united only by a contract, matching Section 6. Introduce an abstract base only when several channels share substantial state or template steps, not for one shared utility method.

- **Why not Option A alone:** Forcing `EmailNotificationSender` and `SmsNotificationSender` onto a new base class reshapes existing types, introduces fragile inheritance where a interface sufficed, and violates **ISP** if the base accumulates channel-specific hooks (push tokens, SMS truncation).
- **Option B alignment:** Chapter `NotifyCustomer` already loops `INotificationSender[]` — push slots in without changing orchestration; retry wraps any sender.
- **When to combine both:** If push and SMS later share **significant** infrastructure (shared rate limiter state, correlation id field, template rendering), extract a small `NotificationSenderBase` **in addition to** the interface for those two — or use a **decorator** `RetryingNotificationSender : INotificationSender` that wraps any implementer.
- **Events vs direct Send:** Audit/logging can stay on `OrderFulfillmentCoordinator.OrderCompleted` (Section 9 preview) — do not push audit into the notification hierarchy.
- **Testing:** Interface + decorator/helper lets you mock `INotificationSender` and assert retry policy independently.

**Production takeaway:** Chapter rule — **interface when unrelated types share a capability; abstract class when subtypes share fields + template logic** (`Document` vs `INotificationSender`). Karat asks you to apply that rule under feature pressure, not pick inheritance by default.

---

---

#### Q5. (P) An ASP.NET Core team registers the chapter's fulfillment types in `Program.cs` for a checkout API:

```csharp
builder.Services.AddSingleton<BankAccount>();
builder.Services.AddSingleton<OrderFulfillmentCoordinator>();
builder.Services.AddTransient<CardPaymentProcessor>();
builder.Services.AddTransient<PaymentProcessor>(sp => sp.GetRequiredService<CardPaymentProcessor>());
builder.Services.AddSingleton<INotificationSender, EmailNotificationSender>();
```

Under concurrent requests, balances mix between customers and notification behavior looks "sticky." Explain what breaks at the DI lifetime layer and how you would register these abstractions for production.

---

**Answer:**

```csharp
builder.Services.AddSingleton<BankAccount>();
builder.Services.AddSingleton<OrderFulfillmentCoordinator>();
builder.Services.AddTransient<CardPaymentProcessor>();
builder.Services.AddTransient<PaymentProcessor>(sp => sp.GetRequiredService<CardPaymentProcessor>());
builder.Services.AddSingleton<INotificationSender, EmailNotificationSender>();
```

Under concurrent requests, balances mix between customers and notification behavior looks "sticky." Explain what breaks at the DI lifetime layer and how you would register these abstractions for production.

**Answer:** `BankAccount` and a single `INotificationSender` registered as **singletons** share one instance for all HTTP requests, so every customer's checkout mutates the same balance and notification channel — a functional bug that only appears under concurrent load.

- **`BankAccount` singleton:** Domain objects with mutable balance must be **scoped per request** (or loaded per customer from persistence), never singleton — same rule as cart state in web apps. Opening an account belongs in a repository + scoped unit of work, not a shared DI instance.
- **`INotificationSender` singleton:** If the implementer holds per-send state, connection, or throttling counters, those leak across users. Prefer **transient** senders or **stateless singleton** that only wraps an `HttpClient` from `IHttpClientFactory`.
- **`OrderFulfillmentCoordinator` singleton:** Acceptable only if it is **stateless** and raises events without storing subscriber lists incorrectly — but event handlers that capture scoped services from singleton are a captive dependency smell; usually register coordinator **scoped**.
- **`PaymentProcessor` transient mapping:** Fine for stateless gateways; register **multiple implementations** via factory or keyed services (`IPaymentProcessorFactory`) when checkout picks card vs wallet per order — not a single `PaymentProcessor` → card binding.
- **Production pattern:** Scoped `OrderFulfillmentService` orchestrator; transient/scoped processors; `IEnumerable<INotificationSender>` or separate sends via factory; **never** singleton mutable domain entities.

```csharp
builder.Services.AddScoped<OrderFulfillmentCoordinator>();
builder.Services.AddTransient<CardPaymentProcessor>();
builder.Services.AddTransient<WalletPaymentProcessor>();
builder.Services.AddTransient<INotificationSender, EmailNotificationSender>();
builder.Services.AddTransient<INotificationSender, SmsNotificationSender>();
// BankAccount: resolve from scoped service using customer id — not AddSingleton<BankAccount>()
```

**Production takeaway:** Chapter types teach OOP shape; ASP.NET DI teaches **which instance lives how long**. Karat capstone connects `BankAccount` encapsulation to **scoped vs singleton** — see foundation DI lifetime gotchas when moving console demo to API.

---

---

#### Q6. (R) A developer splits `BankAccount` into partial files (as in this chapter) but adds a "fast path" for internal ops. Frozen accounts still accept money in staging. Review both fragments:

```csharp
// BankAccount.Core.cs
public partial class BankAccount
{
    private decimal _balance;

    public decimal Balance => _balance;

    public void Deposit(decimal amount)
    {
        if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount));
        _balance += amount;
    }
}

// BankAccount.Ops.cs
public partial class BankAccount
{
    public bool IsActive { get; set; } = true;

    public void CreditOpsAdjustment(decimal amount)
    {
        // Skips ValidateForTransaction — ops-only
        _balance += amount;
    }

    private void ValidateForTransaction()
    {
        if (!IsActive) throw new InvalidOperationException("Account is frozen.");
    }
}
```

`TryWithdraw` still calls `ValidateForTransaction`, but `Deposit` no longer does. What failed across encapsulation and invariants, and how do you fix it?

---

**Answer:**

```csharp
// BankAccount.Core.cs
public partial class BankAccount
{
    private decimal _balance;

    public decimal Balance => _balance;

    public void Deposit(decimal amount)
    {
        if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount));
        _balance += amount;
    }
}

// BankAccount.Ops.cs
public partial class BankAccount
{
    public bool IsActive { get; set; } = true;

    public void CreditOpsAdjustment(decimal amount)
    {
        // Skips ValidateForTransaction — ops-only
        _balance += amount;
    }

    private void ValidateForTransaction()
    {
        if (!IsActive) throw new InvalidOperationException("Account is frozen.");
    }
}
```

`TryWithdraw` still calls `ValidateForTransaction`, but `Deposit` no longer does. What failed across encapsulation and invariants, and how do you fix it?

**Answer:** Partial classes merge into one type, but splitting files does not split invariants — `Deposit` and `CreditOpsAdjustment` now mutate `_balance` without the freeze check, while `TryWithdraw` still enforces it, so callers can credit frozen accounts and `IsActive` is publicly settable, breaking encapsulation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encapsulation | `IsActive` public setter | Any consumer can unfreeze/freeze accounts; bypasses `Freeze()` intent from chapter |
| Invariant | `Deposit` dropped `ValidateForTransaction()` | Frozen accounts accept deposits — staging bug matches production fraud/ops risk |
| Design | `CreditOpsAdjustment` writes `_balance` directly | Second mutation path; ops and customer deposits diverge in rules |
| partial class misuse | Team assumed file boundary = security boundary | Partial only splits compilation units, not access control |

**Fix (priority order):**

1. Restore **`ValidateForTransaction()` at the start of every public mutator** — `Deposit`, and any ops path that should respect freeze (or explicitly document and gate ops behind internal/admin API).
2. Change **`IsActive` to `{ get; private set; }`** — only `Freeze()` (and controlled `Reactivate()` if needed) mutate lifecycle.
3. Route **all balance changes** through private helpers, e.g. `ApplyCredit(decimal amount, bool bypassFreeze = false)` used only from trusted internal assembly with `InternalsVisibleTo` — not a public `CreditOpsAdjustment`.
4. Add tests: deposit/withdraw on frozen account must fail consistently across partial files.

```csharp
public void Deposit(decimal amount)
{
    ValidateForTransaction();
    if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount));
    _balance += amount;
}

public bool IsActive { get; private set; } = true;
```

**Production takeaway:** Chapter Section 2c–2d uses **partial** for team file layout — Karat checks you know both fragments share one invariant surface. See **Program.cs** — `TryDepositOnFrozenAccount` after `Freeze()`.

---

---

#### Q7. (D) You inherit a monolithic fulfillment codebase that mirrors this chapter's demo `Main` — one method creates every object, mutates wallet state, picks a truck by array index, renders shapes, sends notifications, and prints the invoice. The team has one sprint to improve production readiness without a full rewrite.

What refactor order would you choose (encapsulation fixes, introduce interfaces, extract services, events/DI), and what would you **defer**? Tie your answer to the chapter's types (`BankAccount`, `PaymentProcessor`, `Vehicle`, `Shape`, `INotificationSender`, `Document`, `OrderFulfillmentCoordinator`).

---

**Answer:**

What refactor order would you choose (encapsulation fixes, introduce interfaces, extract services, events/DI), and what would you **defer**? Tie your answer to the chapter's types (`BankAccount`, `PaymentProcessor`, `Vehicle`, `Shape`, `INotificationSender`, `Document`, `OrderFulfillmentCoordinator`).

**Answer:** First stop financial and state corruption (wallet + payment ordering + singleton/scoped mistakes), then introduce constructor-injected abstractions for payment and notifications, then extract read-only polymorphic helpers for fleet/shapes/documents — defer full event-driven architecture and extension-method polish until core seams are testable.

**Sprint 1 priority (do now):**

1. **Encapsulation / correctness (`BankAccount`, payment flow):** Ensure all debits go through `TryWithdraw`; fix withdraw-before-charge ordering; no public wallet mutation; per-customer account resolution — highest business risk.
2. **DIP entry points (`PaymentProcessor`, `INotificationSender`):** Extract an `OrderFulfillmentService` that accepts `PaymentProcessor` + `IEnumerable<INotificationSender>` — mirrors chapter `NotifyCustomer` and `ProcessOrderPayment` without rewriting domain types.
3. **Polymorphism cleanup (`Vehicle`, `Shape`):** Replace index/`is` checks with `EstimateDeliveryCostKm` and `Shape.Area`/`Draw()` helpers already in **Program.cs** — low risk, high clarity win.
4. **Document output (`Document`):** Keep `Render()` template method; return invoice string from service, not `Console.WriteLine` in orchestrator — enables API responses.
5. **DI lifetimes (when moving to ASP.NET):** Scoped orchestrator; never singleton `BankAccount`.

**Defer (explicitly):**

- **Full event-driven redesign** (`OrderFulfillmentCoordinator` audit via events) until core flow is unit-tested — events are valuable but add indirection early.
- **New subtypes** (extra shapes, vehicle types) — OCP is already satisfied once polymorphic calls exist.
- **Extension methods** (`ToDisplayLabel`) — cosmetic; no production risk.
- **Partial class splits** — organizational only; no runtime benefit until team scale demands it.
- **Sealed/further inheritance tuning** on `Motorcycle` — design hygiene, not sprint-critical.

**Production takeaway:** Capstone chapter integrates pillars in one narrative — Karat asks for **prioritized** hardening: protect invariants first, inject swappable collaborators second, unify polymorphic dispatch third, polish decoupling (events) last. That mirrors how you would evolve the chapter demo `Main` into a shippable checkout pipeline without a big-bang rewrite.

---

---
