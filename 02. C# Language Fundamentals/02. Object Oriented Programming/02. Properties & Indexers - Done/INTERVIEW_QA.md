# C# Properties & Indexers — Interview Q&A


## Table of Contents

1. [Q1. What is a property in C# and how does it differ from a public field?](#q1-what-is-a-property-in-c-and-how-does-it-differ-from-a-public-field)
2. [Q2. What is an auto-implemented property and when should you use it?](#q2-what-is-an-auto-implemented-property-and-when-should-you-use-it)
3. [Q3. What is a full property with an explicit backing field, and when is it required?](#q3-what-is-a-full-property-with-an-explicit-backing-field-and-when-is-it-required)
4. [Q4. What are expression-bodied properties and what constraints apply to them?](#q4-what-are-expression-bodied-properties-and-what-constraints-apply-to-them)
5. [Q5. What is a read-only property and how does it differ from a `readonly` field?](#q5-what-is-a-read-only-property-and-how-does-it-differ-from-a-readonly-field)
6. [Q6. What is an init-only setter (`init`) in C# 9+ and how does it differ from a read-only property?](#q6-what-is-an-init-only-setter-init-in-c-9-and-how-does-it-differ-from-a-read-only-property)
7. [Q7. What is a private setter and when should you prefer it over `init`?](#q7-what-is-a-private-setter-and-when-should-you-prefer-it-over-init)
8. [Q8. What access modifier constraints apply to property accessors in C#?](#q8-what-access-modifier-constraints-apply-to-property-accessors-in-c)
9. [Q9. What are `required` properties (C# 11) and how do they differ from init-only setters?](#q9-what-are-required-properties-c-11-and-how-do-they-differ-from-init-only-setters)
10. [Q10. What is a static property and what limitations does it have?](#q10-what-is-a-static-property-and-what-limitations-does-it-have)
11. [Q11. What are abstract and virtual properties, and how does property inheritance work in C#?](#q11-what-are-abstract-and-virtual-properties-and-how-does-property-inheritance-work-in-c)
12. [Q12. What is an indexer in C# and how is it declared?](#q12-what-is-an-indexer-in-c-and-how-is-it-declared)
13. [Q13. How do you implement a multi-parameter indexer in C#?](#q13-how-do-you-implement-a-multi-parameter-indexer-in-c)
14. [Q14. How are indexers declared in interfaces, and what does an implementing class need to provide?](#q14-how-are-indexers-declared-in-interfaces-and-what-does-an-implementing-class-need-to-provide)
15. [Q15. How does the property pattern in C# switch expressions work, and what role do property names play?](#q15-how-does-the-property-pattern-in-c-switch-expressions-work-and-what-role-do-property-names-play)
16. [Q16. Why is it a mistake to produce side effects inside a property getter, and what bugs does it cause in practice?](#q16-why-is-it-a-mistake-to-produce-side-effects-inside-a-property-getter-and-what-bugs-does-it-cause-in-practice)
17. [Q17. Why does changing a property from full to auto-implementation silently break validation, and how do you catch this in code review?](#q17-why-does-changing-a-property-from-full-to-auto-implementation-silently-break-validation-and-how-do-you-catch-this-in-code-review)
18. [Q18. What is the defensive-copy gotcha with struct properties, and when does it silently mutate the wrong copy?](#q18-what-is-the-defensive-copy-gotcha-with-struct-properties-and-when-does-it-silently-mutate-the-wrong-copy)
19. [Q19. Can you give an indexer the same parameter type as a property name — and what naming conflict does that create in IL?](#q19-can-you-give-an-indexer-the-same-parameter-type-as-a-property-name-and-what-naming-conflict-does-that-create-in-il)
20. [Q20. What happens when you use `init` with `required` together, and what edge case breaks consumer code?](#q20-what-happens-when-you-use-init-with-required-together-and-what-edge-case-breaks-consumer-code)
21. [Q21. Why can an indexer not be `static`, and what is the practical implication of this restriction?](#q21-why-can-an-indexer-not-be-static-and-what-is-the-practical-implication-of-this-restriction)
22. [Q22. A `BookShelf` indexer passes QA with small data, but production reports `NullReferenceException` on `shelf[2]` when the shelf has capacity 10 but only 2 books. Review the code, identify the defect, and fix it.](#q22-a-bookshelf-indexer-passes-qa-with-small-data-but-production-reports-nullreferenceexception-on-shelf2-when-the-shelf-has-capacity-10-but-only-2-books-review-the-code-identify-the-defect-and-fix-it)
23. [Q23. You are designing an immutable API response DTO in .NET 10. Which property patterns do you choose, and why does that choice affect serialization, testing, and refactoring safety?](#q23-you-are-designing-an-immutable-api-response-dto-in-net-10-which-property-patterns-do-you-choose-and-why-does-that-choice-affect-serialization-testing-and-refactoring-safety)
24. [Q24. A ViewModel needs to implement `INotifyPropertyChanged` so the UI updates when properties change. How do properties make this possible, and what are the performance tradeoffs of different implementation patterns?](#q24-a-viewmodel-needs-to-implement-inotifypropertychanged-so-the-ui-updates-when-properties-change-how-do-properties-make-this-possible-and-what-are-the-performance-tradeoffs-of-different-implementation-patterns)
25. [Q25. A library module exposes its internal book list through a property returning `List<Book>`. A code review flags this as a design risk. Explain the risks, show the fix, and describe when `IReadOnlyList<T>` vs `IEnumerable<T>` is the better return type.](#q25-a-library-module-exposes-its-internal-book-list-through-a-property-returning-listbook-a-code-review-flags-this-as-a-design-risk-explain-the-risks-show-the-fix-and-describe-when-ireadonlylistt-vs-ienumerablet-is-the-better-return-type)
26. [Q26. You need a `Matrix<T>` class that supports grid access via a two-parameter indexer with bounds checking and a `Fill` method. Write the core class, identify the edge cases in the indexer, and explain how the design changes if `T` must be a numeric type.](#q26-you-need-a-matrixt-class-that-supports-grid-access-via-a-two-parameter-indexer-with-bounds-checking-and-a-fill-method-write-the-core-class-identify-the-edge-cases-in-the-indexer-and-explain-how-the-design-changes-if-t-must-be-a-numeric-type)
27. [Q27. A team debates whether to use `required` properties with object initializers or constructor parameters for a domain aggregate root. Walk through the tradeoffs and give a concrete recommendation for a `CustomerOrder` entity.](#q27-a-team-debates-whether-to-use-required-properties-with-object-initializers-or-constructor-parameters-for-a-domain-aggregate-root-walk-through-the-tradeoffs-and-give-a-concrete-recommendation-for-a-customerorder-entity)

---
## Foundation Questions

---

## Q1. What is a property in C# and how does it differ from a public field?

**Concepts**
- Accessor-backed member vs raw storage slot
- `get` and `set` accessor methods
- Encapsulation and validation point
- Interface and reflection compatibility
- Binary compatibility across assemblies

**Answer**

A property looks like a field at the call site — callers use dot notation to read and write — but under the hood it compiles to a pair of methods: `get_PropertyName` and `set_PropertyName`. This indirection is what makes properties the cornerstone of encapsulation in C#. A public field exposes raw storage directly: any caller can read or write any value with no interception, and you cannot add validation later without breaking binary compatibility across compiled assemblies. A property gives you a controlled gateway: the `set` accessor can throw `ArgumentException` for invalid input, trim whitespace, raise a `PropertyChanged` event, or trigger lazy initialization, all without changing the call site. Properties can also be declared in interfaces, whereas fields cannot, which matters for dependency injection and mocking. Finally, many framework features — data binding, serialization, reflection-based tooling — use property conventions exclusively and will silently skip public fields. As a rule of thumb: use a public field only for constants (`const`) or static read-only sentinels; use a property for any value that belongs to an object's observable state.

---

## Q2. What is an auto-implemented property and when should you use it?

**Concepts**
- Compiler-generated hidden backing field
- Concise declaration syntax
- Default initializer at declaration
- Appropriate use cases
- Limitation: no custom accessor logic

**Answer**

An auto-implemented property — `public string Title { get; set; }` — instructs the compiler to generate a hidden private backing field and wire the `get`/`set` accessors to it. You never see or name that field in source code. This is the right choice when the property needs no validation, transformation, or side effects: simple data-transfer objects, configuration models, and POCO entities typically qualify. You can provide a default value inline, as in `public string Name { get; set; } = string.Empty;`, which initializes the hidden field before any constructor runs. The limitation is precisely its simplicity: the moment you need to validate, trim, or raise a notification on assignment, you must promote it to a full property with an explicit backing field, because you cannot reach the hidden field from user code. In .NET 10 projects, auto-properties remain the dominant form for DTOs and record-like classes; for domain entities with invariants, a full property is the correct starting shape even if no logic exists yet — it makes the invariant location obvious to future maintainers.

---

## Q3. What is a full property with an explicit backing field, and when is it required?

**Concepts**
- Private backing field naming convention (`_camelCase`)
- `get` accessor reading the field
- `set` accessor with guard clauses
- `value` keyword in setter context
- Preserving state on validation failure

**Answer**

A full property declares a private field — by convention `_camelCase` — and implements the `get` and `set` accessors explicitly. The `get` accessor returns the field's current value; the `set` accessor receives the incoming value through the implicit `value` keyword, validates it, and only writes to the field if the value is acceptable. Crucially, the backing field must **not** be updated before validation passes: if you throw an exception at the end of the setter, the field has already been mutated. The correct pattern assigns only after all guards complete, or throws early before any assignment. Full properties are required whenever an assignment must be intercepted — trimming whitespace from an ISBN, clamping a percentage to 0–100, rejecting null, or raising `INotifyPropertyChanged`. They are also the right choice when the property's value must be derived from multiple fields in a non-trivial way that warrants storing intermediate state. In performance-sensitive paths, a full property with an expression-bodied getter (`get => _field;`) compiles identically to the auto-property form while still allowing you to add a validating setter later without restructuring the class.

---

## Q4. What are expression-bodied properties and what constraints apply to them?

**Concepts**
- `=>` lambda-body syntax for members
- Read-only computed value, no backing storage
- Equivalent to `get { return ...; }`
- Expression-bodied full getter vs entire property
- Cannot combine `=>` property body with explicit `set`

**Answer**

An expression-bodied property uses `=>` in place of a `{ get { return ...; } }` block: `public string DisplayLabel => $"{Title} [{Isbn}]";`. This is syntactic sugar for a read-only property whose getter is a single return expression — the compiler emits exactly the same IL as the long form. Because the `=>` syntax defines the entire property as a getter, you cannot add a `set` accessor to it; that would require switching to full-property syntax. Expression-bodied properties carry no backing storage of their own: every read evaluates the expression fresh, which is ideal for computed labels, formatted strings, or cheap derived values. They are not appropriate when the computation is expensive or has observable side effects, because callers may read the property multiple times (data-binding loops, grid renders) and expect idempotent results. In .NET 10 code, expression-bodied properties combine naturally with expression-bodied methods to create very compact, readable types. The one subtlety: you can have an expression-bodied `get` accessor inside a full-property block (`get => _field;`) while still declaring a separate `set`, which gives you a concise getter alongside a validating setter.

---

## Q5. What is a read-only property and how does it differ from a `readonly` field?

**Concepts**
- Get-only property syntax (`{ get; }`)
- Constructor-only assignment window
- `readonly` field vs read-only property semantics
- Interface and polymorphism compatibility
- Immutability guarantee strength

**Answer**

A read-only property — `public string CatalogId { get; }` — exposes a `get` accessor with no `set`, meaning external callers cannot assign after construction. The value must be set either in the constructor body or (for auto-properties) in a field initializer; no other location in the class can write it. This is distinct from a `readonly` field: a `readonly` field is a storage location whose constraint is enforced at the IL level, cannot be overridden in a subclass, and is always an instance member. A read-only property, by contrast, is a method pair — it can be `virtual` or `abstract`, it can be declared in an interface, and a derived class can override it. Both provide a similar immutability guarantee to callers (the value is stable after construction), but properties do so through accessor logic rather than field-level constraints. In practice, prefer a read-only property for any value you want to expose as part of a type's public contract; use a `readonly` field only for internal implementation details where no accessor overhead is warranted, such as in tight loops or when you need the field to participate in `ref` semantics.

---

## Q6. What is an init-only setter (`init`) in C# 9+ and how does it differ from a read-only property?

**Concepts**
- `init` accessor keyword (C# 9)
- Object initializer assignment window
- Immutability after construction
- Difference from `{ get; }` (constructor vs initializer)
- `with` expression on records and init properties

**Answer**

The `init` accessor, introduced in C# 9, allows a property to be set either inside a constructor or inside an object initializer, but never afterwards. The declaration `public DateTime AddedOn { get; init; }` means a caller can write `new Book("CAT-001", "Clean Code") { AddedOn = DateTime.UtcNow }` but cannot write `book.AddedOn = newDate` on a variable that already exists. This differs from a plain read-only property (`{ get; }`) in one important way: a read-only property can only be assigned inside the class's own constructor, whereas an `init` property can be assigned from **outside** the class during object initialization. That makes `init` the preferred pattern for immutable DTOs and domain objects that need to participate in object-initializer syntax — a common pattern with dependency injection, deserialization, and factory methods. In C# 9+ records, `with` expressions create a shallow copy with specified `init` properties overridden: `var updated = original with { AddedOn = newDate };`. The key behavioral guarantee is the same as read-only after construction: once the object's initializer scope closes, the property is permanently locked.

---

## Q7. What is a private setter and when should you prefer it over `init`?

**Concepts**
- `{ get; private set; }` syntax
- Asymmetric accessor access modifiers
- Mutable only from inside the class
- Appropriate for status/state fields
- Contrast with `init` (post-construction mutation)

**Answer**

A private setter — `public string LastUpdatedBy { get; private set; }` — makes a property publicly readable but restricts the `set` accessor to code inside the declaring class (or its nested types). External callers see an effectively read-only value; internal methods can update it through normal assignment. This is the right pattern for status fields and audit trails that change over the lifetime of an object but only through controlled operations: `public void Rename(string newTitle, string updatedBy) { Title = newTitle; LastUpdatedBy = updatedBy; }`. The difference from `init` is that `init` locks the value permanently after construction while `private set` allows ongoing mutation — the mutation is simply restricted to the class's own logic. Choosing between them is a question of invariant lifetime: if a property should never change after the object is created, use `init`; if the object is mutable but only the class itself should drive changes, use `private set`. Protected setters (`protected set`) extend the same pattern down an inheritance hierarchy, allowing derived classes to update the property while still blocking external callers.

---

## Q8. What access modifier constraints apply to property accessors in C#?

**Concepts**
- Accessor access modifier rules
- More restrictive than the property itself
- Only one accessor may have a modifier
- `private`, `protected`, `internal`, `protected internal`
- Interface property accessor restrictions

**Answer**

C# allows at most one accessor of a property to carry an access modifier, and that modifier must be **more restrictive** than the property's own access level. For example, `public string Status { get; private set; }` is legal because `private` is more restrictive than `public`; writing `private string Status { get; public set; }` is a compile error because `public` is less restrictive than `private`. The full hierarchy from least to most restrictive is: `public`, `protected internal`, `internal`, `protected`, `private protected`, `private`. You can apply any level to the setter (or getter) provided it falls below the property's level. You cannot independently modify both accessors — that would require two separate modifiers on the same property, which the language forbids. In interfaces, property accessors cannot carry access modifiers at all: the interface declares only `{ get; }`, `{ set; }`, or `{ get; set; }`, and the implementing class provides the logic. Abstract properties in base classes follow the same rule — the modifier on the property itself determines visibility; the accessors inherit it.

---

## Q9. What are `required` properties (C# 11) and how do they differ from init-only setters?

**Concepts**
- `required` modifier keyword (C# 11)
- Compiler-enforced initialization at call site
- Works with both `set` and `init` accessors
- `SetsRequiredMembers` attribute for constructors
- Complement to init-only, not a replacement

**Answer**

The `required` modifier, introduced in C# 11, instructs the compiler to enforce that a property is explicitly set in every object initializer that creates the type. Declaring `public required string Name { get; init; }` means any `new Person { }` call that omits `Name` is a compile-time error — the compiler does not silently use a default value. This is stronger than `init` alone, which merely restricts *when* the property can be set, not whether it *must* be. The `required` modifier can combine with either `init` or `set`: `required init` means "must be set during object initialization, then locked"; `required set` means "must be set but can be changed later." When a constructor handles initialization internally, it can be decorated with `[SetsRequiredMembers]` to suppress the compiler's enforcement — the attribute signals that the constructor guarantees all required members are assigned. The practical pattern is to use `required init` for value-object DTOs that travel over API boundaries, replacing the verbose all-parameters constructor with a clean object-initializer style while retaining compile-time safety. Unlike constructor parameters, `required` properties self-document which fields are mandatory at the property declaration itself, and they work naturally with object-initializer syntax that serializers and mappers rely on.

---

## Q10. What is a static property and what limitations does it have?

**Concepts**
- Belongs to the type, not an instance
- Shared mutable state risk
- Thread-safety considerations
- Cannot be virtual or abstract
- Common patterns: singleton, configuration, counter

**Answer**

A static property belongs to the type itself rather than any instance, so all callers share a single value. It is declared with the `static` keyword: `public static int Count { get; private set; }`. The most common legitimate uses are singleton accessors (`public static MyService Instance => _instance;`), type-level configuration defaults, and counters that track object creation across all instances. The primary risk of static mutable properties is shared mutable state: in a multi-threaded application, concurrent reads and writes without synchronization produce race conditions. If a static property setter modifies state, it typically requires a `lock` or an `Interlocked` operation. Static properties have one important constraint: they cannot be `virtual`, `abstract`, or `override`, because those keywords are instance-polymorphism mechanisms — there is no instance dispatch table for a static member. In interfaces starting with C# 11, `static abstract` members are supported for numeric generic constraints, but a static property there is a different concept used for operator-generic programming, not for general data access. For general design, minimize static mutable state; prefer static read-only properties backed by immutable values or lazy initializers.

---

## Q11. What are abstract and virtual properties, and how does property inheritance work in C#?

**Concepts**
- `virtual` — overridable with default implementation
- `abstract` — no implementation, forces override in derived class
- `override` keyword in derived class
- `sealed override` to stop further overriding
- Accessor symmetry requirement in overrides

**Answer**

A `virtual` property provides a base-class implementation that derived classes may replace with `override`. An `abstract` property declares only the accessor signature with no body, requiring any non-abstract derived class to provide the implementation. Both follow the same dispatch rules as virtual methods: the runtime calls the most-derived override based on the object's actual type. A key constraint governs accessor pairing: if the base class declares `{ get; set; }`, an override cannot remove the setter — it must provide both accessors or seal one with `sealed override`. Conversely, an abstract property with only `{ get; }` cannot gain a setter in the override. The practical implication is that you must design the accessor surface of an abstract property carefully, because it becomes a contract that all subclasses must honor. A common pattern in frameworks is an abstract base that declares `public abstract string DisplayName { get; }`, then subclasses compute a type-specific label. `sealed override` on a property prevents further subclasses from re-overriding it, providing a guarantee that the implementation is stable for that branch of the hierarchy. Interfaces declaring properties work similarly: `string Name { get; }` in an interface forces the implementing class to supply at minimum a getter.

---

## Q12. What is an indexer in C# and how is it declared?

**Concepts**
- `this[parameters]` syntax
- Instance member only (no `static`)
- `get` and `set` accessors same as properties
- Parameter type determines overload identity
- Bracket-notation call site

**Answer**

An indexer is a special member that allows instances of a class to be accessed using bracket notation, just like arrays. It is declared with the `this` keyword followed by a parameter list: `public Book this[int index] { get { ... } set { ... } }`. At the call site, `shelf[0]` invokes the getter and `shelf[0] = book` invokes the setter — the compiler translates these into calls to `get_Item` and `set_Item`, the standard IL method names for indexers. Unlike properties, which are identified by a name, indexers are identified by their parameter signature, enabling overloading: a class can have both `this[int index]` and `this[string key]` simultaneously. Indexers are always instance members — they cannot be `static`, because their purpose is to provide parameterized access to a specific object's internal state. The parameter type need not be an integer; any type is allowed, making indexers suitable for dictionary-style wrappers, configuration stores, or any collection that needs to surface both positional and key-based access through the same type. A read-only indexer omits the `set` accessor, and an expression-bodied indexer (`public int this[int i] => _data[i];`) condenses a get-only indexer into a single line.

---

## Q13. How do you implement a multi-parameter indexer in C#?

**Concepts**
- Multiple parameters in `this[T1 a, T2 b]`
- Comma-separated call site syntax
- Row/column matrix pattern
- Bounds validation on each parameter
- Similarity to two-dimensional array access

**Answer**

A multi-parameter indexer simply lists additional parameters in the `this[]` declaration, separated by commas: `public double this[int row, int col] { get => _data[row, col]; set => _data[row, col] = value; }`. At the call site, callers write `matrix[1, 2] = 5.5;` — the compiler passes both arguments to the indexer accessors. The parameter count and types together form the indexer's overload signature. This pattern is most natural for two-dimensional data structures: matrices, spreadsheet cells, game boards, or any grid where position is described by two independent coordinates. Bounds validation must cover every parameter independently — an argument check for the first index alone would allow an in-range row with an out-of-range column to slip through and corrupt memory or throw an unhelpful `IndexOutOfRangeException` from inside the internal array. In .NET 10, the `System.Numerics.Matrix4x4` and similar primitives use two-parameter patterns internally. When using a multi-dimensional backing array, remember that `int[,]` and `int[][]` (jagged) have different semantics; the indexer should mirror the backing structure's layout to avoid confusion.

---

## Q14. How are indexers declared in interfaces, and what does an implementing class need to provide?

**Concepts**
- Interface indexer declaration syntax
- No body in the interface
- Class provides the accessors
- Explicit interface implementation for conflicts
- Readonly vs read-write in interface contract

**Answer**

An interface can declare an indexer with the standard `this[]` syntax but without accessor bodies, just as it declares properties without implementations: `T this[int index] { get; }` or `T this[string key] { get; set; }`. An implementing class must provide all declared accessors at a minimum — it may add a setter if the interface only declares a getter, provided the extra accessor is not part of the interface contract. When two interfaces declare indexers with the same parameter type but different intended semantics, explicit interface implementation resolves the conflict: `Book IReadableShelf.this[int index] => ...; void IWritableShelf.this[int index] = ...`. Callers must cast the object to the specific interface to reach the explicitly implemented indexer. One practical reason to declare indexers on an interface is enabling mock frameworks and test doubles to intercept indexed access without coupling to a concrete collection type. In a large system, expressing `ILookup<TKey, TResult>` with an indexer on the interface lets you swap in a read-through cache, a database-backed store, or an in-memory stub without changing callers.

---

## Q15. How does the property pattern in C# switch expressions work, and what role do property names play?

**Concepts**
- Property pattern syntax `{ PropertyName: pattern }`
- Deconstruction vs property pattern
- Nested property patterns
- `and`, `or`, `not` combinators
- Pattern matching without `is` null check

**Answer**

The property pattern, available since C# 8 and extended in C# 9 and 10, allows a switch expression arm to match an object by testing the values of its properties directly. The syntax is `obj switch { { PropertyName: literalOrPattern } => result }`. For example, `book switch { { PageCount: > 500 } => "long", { PageCount: <= 100 } => "short", _ => "medium" }` evaluates without any explicit type cast, relying on the named properties being publicly readable. The runtime checks `null` implicitly — a null reference never matches a property pattern, so no explicit null guard is needed. Nested property patterns reach into object graphs: `{ Author: { Country: "UK" } }` matches a book whose `Author.Country` is "UK". Combinators compose multiple property tests: `{ IsAvailable: true, PageCount: > 0 }` requires both to be true. This feature is tightly coupled to property accessibility: only properties (and fields) visible to the pattern context participate. Computed properties (`=> expr`) work just as well as stored ones — the runtime calls the getter. In domain-model code, property patterns eliminate large chains of `if/else` comparisons and make branching logic readable at a glance.

---

## Gotcha Questions

---

## Q16. Why is it a mistake to produce side effects inside a property getter, and what bugs does it cause in practice?

**Concepts**
- Caller expectation of idempotency
- Data-binding repeated reads
- Serialization and debugging reads
- Hidden counter/timestamp mutation
- `[DebuggerBrowsable]` implicit reads

**Answer**

A property getter is expected to be a pure read: callers, frameworks, and tooling assume that reading `obj.SomeProperty` twice in a row yields the same value without changing any observable state. Violating this creates a category of subtle, hard-to-reproduce bugs. Data-binding systems such as WinForms, WPF, and Blazor can read a bound property dozens of times per render cycle — if the getter increments a `ViewCount` or updates a `LastRendered` timestamp on every read, those values will be wildly inflated. Debugger watch windows and IDE data-tip popups call getters automatically when you hover over a variable; if the getter has a side effect, simply pausing on a breakpoint mutates state. JSON serializers enumerate all readable properties during serialization — a getter that modifies internal state makes serialization a destructive operation. The fix is to separate concerns: the getter returns a value, while mutation happens in a method with an imperative name (`IncrementViewCount()`, `RecordRender()`). If you must cache or lazily initialize inside a getter, use `Lazy<T>` or a pattern where the backing field is set exactly once and all subsequent reads are pure returns. The only sanctioned exception is lazy initialization behind a thread-safe check — and even then, the observable value of the property is stable from the caller's perspective.

---

## Q17. Why does changing a property from full to auto-implementation silently break validation, and how do you catch this in code review?

**Concepts**
- Auto-property removes custom setter logic
- Validation silently disappears
- No compile-time warning
- Data integrity regression at runtime
- Code review checklist for property changes

**Answer**

When a developer changes `public string Isbn { get => _isbn; set { if (...) throw ...; _isbn = value.Trim(); } }` to `public string Isbn { get; set; }` to "clean up" the code, the compiler accepts both forms with no warning — but every guard and transformation in the original setter is silently discarded. From that point forward, any caller can assign whitespace, null, or malformed data directly to the property, and the change will persist to the database or downstream system without error. This regression is especially dangerous in deserialization paths: JSON or XML parsers bypass constructors and set properties directly, so a prior import guard in the setter is the only line of defence. To catch this in code review, look for any property change from full to auto form and ask: does the original setter contain any `if`, `throw`, assignment to a related field, or event invocation? If yes, the refactoring discards behaviour, not boilerplate. The rule of thumb is: a full property with a body is documentation of an invariant. Auto-property is appropriate only when there is genuinely nothing to enforce. Tests that exercise boundary values (empty string, whitespace, negative numbers) will catch the regression in CI, which is why boundary-value coverage on domain properties is important.

---

## Q18. What is the defensive-copy gotcha with struct properties, and when does it silently mutate the wrong copy?

**Concepts**
- Value-type copy semantics
- Property getter returning a struct value
- Mutating methods called on the copy
- `readonly struct` compiler enforcement
- `ref` return properties as a workaround

**Answer**

When a property returns a value type (struct), the getter returns a copy of the value, not a reference to the original storage. If you call a mutating method on that copy, the mutation applies to the transient copy and is immediately discarded — the original field is unchanged, and the bug leaves no trace. For example, if `public Point Location { get; set; }` holds a `System.Drawing.Point`, then `obj.Location.Offset(10, 10);` compiles cleanly but silently no-ops: the getter returns a copy of `Point`, `Offset` mutates that copy, the copy is thrown away, and `obj.Location` is unchanged. The correct fix is to read, modify, and reassign: `obj.Location = new Point(obj.Location.X + 10, obj.Location.Y + 10);`. Declaring a struct as `readonly struct` causes the compiler to emit a defensive copy whenever a non-readonly method is called on it through a readonly reference, but only warns if you enable the appropriate analyzer. In C# 10+ with `ref` return properties, you can return a managed reference to the backing field, allowing in-place mutation — but this is an advanced technique reserved for high-performance scenarios with careful ownership semantics. The practical rule: never call mutating methods on a property result unless the property is known to return a reference type.

---

## Q19. Can you give an indexer the same parameter type as a property name — and what naming conflict does that create in IL?

**Concepts**
- `IndexerName` attribute (`System.Runtime.CompilerServices`)
- Default IL name: `Item` / `get_Item` / `set_Item`
- Conflict with a property named `Item`
- Compiler error when names collide
- Renaming indexers for COM interop

**Answer**

The C# compiler translates every indexer into an IL property named `Item` by default, with accessor methods `get_Item` and `set_Item`. This means if a class simultaneously declares an indexer and a regular property named `Item`, the compiler raises an error: the two would compile to the same IL member name. The fix is to apply `[System.Runtime.CompilerServices.IndexerName("Element")]` (or any other name) to the indexer, which instructs the compiler to use that string for the generated IL property name instead of `Item`. The C# bracket syntax is unaffected — callers still write `obj[key]` — but the underlying IL method is now `get_Element`. This attribute also matters for COM interop, where the default indexer name must match a specific expected name in the COM type library. In most purely managed code this conflict is rare because naming a property `Item` is itself unusual, but it surfaces in library code that wraps existing COM or native interfaces. Knowing the underlying IL name is also useful when consuming an indexer via reflection: `GetProperty("Item")` or `GetProperty("Element")` depending on whether the attribute was applied.

---

## Q20. What happens when you use `init` with `required` together, and what edge case breaks consumer code?

**Concepts**
- `required init` combination legality
- Constructor with `[SetsRequiredMembers]` bypass
- Derived class required member inheritance
- Object initializer scope rules
- Deserialization frameworks and required members

**Answer**

`required` and `init` can and commonly should appear together: `public required string Name { get; init; }` means the consumer must supply `Name` in an object initializer and cannot change it afterwards. The edge case that breaks consumer code arises when a class also provides a parameterized constructor decorated with `[SetsRequiredMembers]`. In that case, code calling the constructor does not need an object initializer at all — the attribute suppresses the compiler check. But if that constructor exists alongside object-initializer usage, a caller who forgets `[SetsRequiredMembers]` on a factory or copy constructor will discover the compiler now silently enforces all required members again for that path. A related edge case: derived classes inherit required members from base classes. If the base declares `public required string Id { get; init; }` and the derived class adds a constructor that calls `base(...)`, the derived constructor must also be decorated with `[SetsRequiredMembers]` or the required member check resurfaces. Deserialization frameworks (System.Text.Json in .NET 7+) have built-in awareness of `required` members and will throw `JsonException` if the JSON payload omits a required property — which is the correct runtime enforcement, but it means changing a property from optional to `required` is a **breaking deserialization change** that requires a versioning strategy.

---

## Q21. Why can an indexer not be `static`, and what is the practical implication of this restriction?

**Concepts**
- Indexer tied to instance state
- Static context has no `this` receiver
- Workaround: static dictionary property
- Contrast with static properties
- Design signal for collection ownership

**Answer**

An indexer uses the `this` keyword as its identifier, which represents the current instance. A static context has no instance — there is no `this` — so the language specification prohibits `static` indexers outright; the compiler rejects the combination. The practical implication is that any type that needs indexed access must be instantiated, even if it wraps a shared or global data structure. If you want type-level keyed access, the correct pattern is a static property that returns a collection type: `public static IReadOnlyDictionary<string, Config> Defaults => _defaults;`, where callers then write `Config.Defaults["timeout"]`. This is slightly more verbose than `Config["timeout"]` would be, but it is explicit about ownership: the dictionary object is a first-class value, and bracket access belongs to the dictionary, not to the `Config` type. The restriction also serves as a design signal — if you find yourself wanting a static indexer, the underlying data probably belongs in a named static collection property, a static factory method, or a dedicated registry class. In performance-critical cache scenarios, `System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>` provides the required thread-safe keyed access through its own instance indexer.

---

## Real-World Scenarios

---

## Q22. A `BookShelf` indexer passes QA with small data, but production reports `NullReferenceException` on `shelf[2]` when the shelf has capacity 10 but only 2 books. Review the code, identify the defect, and fix it.

**Concepts**
- Bounds check against `_count` vs `_slots.Length`
- Allocated but unoccupied array slots
- `NullReferenceException` from empty slots
- `ArgumentOutOfRangeException` semantics
- Defensive indexer pattern

**Answer**

The defect is that the indexer validates the index against `_slots.Length` (the allocated array size) instead of `_count` (the number of actually added books). An index of 2 with `_slots.Length` equal to 10 passes the bounds check, but `_slots[2]` is `null` because only slots 0 and 1 have been filled. The caller receives a `null` reference silently, and the `NullReferenceException` surfaces later when they access a property on the returned object — far from the real defect site.

```csharp
// DEFECTIVE implementation
public class BookShelf
{
    private readonly Book[] _slots;
    private int _count;

    public BookShelf(int capacity) => _slots = new Book[capacity];
    public void Add(Book book) => _slots[_count++] = book;
    public int Count => _count;

    public Book this[int index]
    {
        get
        {
            if (index < 0 || index >= _slots.Length)  // BUG: should be _count
                throw new ArgumentOutOfRangeException(nameof(index));

            return _slots[index]; // returns null for unfilled slots
        }
    }
}
```

| Category | Problem | Impact |
|---|---|---|
| Logic | Guard uses `_slots.Length` instead of `_count` | Null returned for valid-seeming indexes |
| Correctness | Caller gets null with no exception | NullReferenceException far from source |
| Readability | `_count` field exists but is ignored in guard | Misleading code — `Count` is redundant |

**Fix priority:**
1. Change `index >= _slots.Length` to `index >= _count` in the guard — eliminates null returns.
2. Add null-guard to `Add` to prevent `_slots[_count++] = null` from poisoning later reads.
3. Add overflow guard to `Add` so inserting beyond capacity throws `InvalidOperationException` before corrupting `_count`.

---

## Q23. You are designing an immutable API response DTO in .NET 10. Which property patterns do you choose, and why does that choice affect serialization, testing, and refactoring safety?

**Concepts**
- `required init` for mandatory immutable fields
- `init`-only for optional fields with known defaults
- `System.Text.Json` awareness of required members
- Object initializer syntax vs constructor parameters
- Refactoring safety when fields are added

**Answer**

For an immutable API response DTO, the idiomatic .NET 10 pattern combines `required init` properties for fields that must always be present and plain `init` properties (with defaults) for optional fields. This approach produces a class that can be instantiated through object-initializer syntax — which serializers, mappers like AutoMapper, and test helpers all understand — while still enforcing mandatory presence at compile time and locking values after construction.

A concrete shape looks like this: `public required string OrderId { get; init; }` for the mandatory identifier, `public required string Status { get; init; }` for a non-nullable status code, and `public string? CancellationReason { get; init; }` for an optional nullable field. System.Text.Json in .NET 7+ and later respects `required` at deserialization time and throws `JsonException` when a required property is absent in the incoming JSON, providing server-side validation without a separate validation layer.

The refactoring safety advantage over an all-constructor approach is significant: adding a new `required` property to the DTO immediately surfaces all call sites that omit it as compile errors, whereas adding a constructor parameter requires updating every invocation location. In tests, the object-initializer pattern allows partial construction — you can omit optional properties and rely on their default values — keeping tests focused on the properties they care about. The tradeoff is that `required` makes the type a breaking change to add non-nullable members to: any consumer compiled against the old contract must be recompiled. Use `required init` for stable core fields and `init` with defaults for extensible optional fields.

---

## Q24. A ViewModel needs to implement `INotifyPropertyChanged` so the UI updates when properties change. How do properties make this possible, and what are the performance tradeoffs of different implementation patterns?

**Concepts**
- `INotifyPropertyChanged` interface
- `PropertyChanged` event invocation in setter
- `CallerMemberName` attribute for property name
- `SetField` helper pattern
- Reducing allocations with `EqualityComparer<T>`

**Answer**

`INotifyPropertyChanged` works by having every writable property's `set` accessor raise a `PropertyChanged` event with the property's name as a string. The UI framework subscribes to this event and re-renders bindings that reference the named property. The naive pattern is: in each setter, compare the new value to the backing field, update the field, then call `PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MyProperty)));`. The `nameof` operator prevents typos that would silently break bindings.

The repeating boilerplate is commonly extracted into a `SetField` helper:

```csharp
protected bool SetField<T>(ref T field, T value,
    [CallerMemberName] string propertyName = "")
{
    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
    field = value;
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    return true;
}

public string Title
{
    get => _title;
    set => SetField(ref _title, value);
}
```

The `[CallerMemberName]` attribute fills in the property name automatically at compile time, eliminating both the string literal and the `nameof` call. The `EqualityComparer<T>.Default.Equals` guard suppresses the event when the value has not actually changed, which is crucial for performance: without it, every assignment fires a re-render even for identical values, causing layout thrashing in large grids. The `bool` return value from `SetField` enables computed properties that depend on the changed field to raise their own `PropertyChanged` in sequence: `if (SetField(ref _price, value)) OnPropertyChanged(nameof(Total));`. This chain-reaction pattern keeps derived property notifications accurate without duplicating logic.

---

## Q25. A library module exposes its internal book list through a property returning `List<Book>`. A code review flags this as a design risk. Explain the risks, show the fix, and describe when `IReadOnlyList<T>` vs `IEnumerable<T>` is the better return type.

**Concepts**
- Returning a mutable collection reference
- Encapsulation breach via `Clear()`, `Add()`, `Remove()`
- `AsReadOnly()` and `IReadOnlyList<T>`
- `IEnumerable<T>` for lazy/streaming scenarios
- Defensive copy vs read-only view

**Answer**

Returning `List<Book>` from a property hands the caller a live reference to the object's private collection. Any caller can call `Clear()`, `Add(new Book(...))`, or `Remove(book)` directly — all without going through any validation logic in `AddBook()` or `RemoveBook()`. The class's internal invariants (no empty ISBNs, maximum shelf size, duplicate detection) are completely bypassed. Worse, because the caller holds a reference to the same list, any later mutations from inside the class are immediately visible to the caller's snapshot variable, and vice versa — shared mutable state across boundaries.

The fix at minimum is to return `IReadOnlyList<Book>`:

```csharp
public class LibrarySection
{
    private readonly List<Book> _books = new();

    public IReadOnlyList<Book> Books => _books.AsReadOnly();

    public void AddBook(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        if (string.IsNullOrWhiteSpace(book.Isbn))
            throw new ArgumentException("ISBN required.", nameof(book));
        _books.Add(book);
    }
}
```

`_books.AsReadOnly()` returns a `ReadOnlyCollection<T>` wrapper that prevents mutation without copying data. The interface type `IReadOnlyList<Book>` on the property declaration signals the contract to callers explicitly. Choose `IReadOnlyList<T>` when callers need random access by index or need `Count`. Choose `IEnumerable<T>` when callers only need to iterate and you want the flexibility to swap the internal storage to a different collection type or introduce deferred evaluation. Avoid returning `IEnumerable<T>` if callers will enumerate multiple times and the underlying query is expensive — in that case, materialize to a list before returning. Never return `List<T>` directly from a public API unless the class explicitly documents that callers own the returned collection.

---

## Q26. You need a `Matrix<T>` class that supports grid access via a two-parameter indexer with bounds checking and a `Fill` method. Write the core class, identify the edge cases in the indexer, and explain how the design changes if `T` must be a numeric type.

**Concepts**
- Multi-parameter indexer declaration
- Row/column bounds validation
- Generic type constraint (`INumber<T>` in .NET 7+)
- `ArgumentOutOfRangeException` per-parameter
- `Fill` vs element-wise mutation patterns

**Answer**

A `Matrix<T>` stores data in a one-dimensional array for cache locality, maps `[row, col]` to `row * _cols + col`, and validates both dimensions independently in the indexer.

```csharp
public class Matrix<T>
{
    private readonly T[] _data;
    private readonly int _rows;
    private readonly int _cols;

    public int Rows => _rows;
    public int Cols => _cols;

    public Matrix(int rows, int cols)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cols);
        _rows = rows; _cols = cols;
        _data = new T[rows * cols];
    }

    public T this[int row, int col]
    {
        get
        {
            ValidateBounds(row, col);
            return _data[row * _cols + col];
        }
        set
        {
            ValidateBounds(row, col);
            _data[row * _cols + col] = value;
        }
    }

    public void Fill(T value)
        => Array.Fill(_data, value);

    private void ValidateBounds(int row, int col)
    {
        if ((uint)row >= (uint)_rows)
            throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)col >= (uint)_cols)
            throw new ArgumentOutOfRangeException(nameof(col));
    }
}
```

The cast to `uint` before comparison (`(uint)row >= (uint)_rows`) simultaneously rejects negative values and out-of-range positive values in a single comparison — a standard JIT-friendly bounds-check pattern. Edge cases to handle: zero-sized dimensions (rejected in constructor), negative row or column (caught by uint cast), and integer overflow when rows × cols exceeds `int.MaxValue` (use `checked` arithmetic in the constructor for very large grids). If `T` must support arithmetic (addition, multiplication for matrix math), constrain it with `where T : INumber<T>` available since .NET 7 — this unlocks generic math operators (`+`, `*`) through the `System.Numerics.INumber<T>` interface without requiring separate method overloads per numeric type. For value types specifically, `T[]` avoids the boxing overhead that `object[]` would impose, making the flat-array layout both type-safe and allocation-efficient.

---

## Q27. A team debates whether to use `required` properties with object initializers or constructor parameters for a domain aggregate root. Walk through the tradeoffs and give a concrete recommendation for a `CustomerOrder` entity.

**Concepts**
- Constructor parameter: compile-time, positional, refactoring-visible
- `required` property: call-site explicit, flexible ordering, serializer-friendly
- Invariant enforcement location
- Domain-driven design aggregate construction pattern
- Factory method as a hybrid approach

**Answer**

Both approaches enforce that mandatory data is present when an object is created, but they differ in how they communicate that requirement and how well they integrate with surrounding tooling. Constructor parameters are positional: adding a new mandatory parameter immediately breaks all call sites, the compiler reports every missing argument, and the parameter list serves as documentation of what the aggregate needs to exist. The setter runs inside the constructor body, so invariant checks run once in a controlled location. The downside is that constructors with many parameters are harder to read and maintain, and they resist object-initializer-style test setup.

`required` properties with `init` accessors invert this: each required field is named at the call site (`new CustomerOrder { CustomerId = id, Items = items }`), making the intent clear even with many fields. They integrate naturally with System.Text.Json deserialization and test builders. The downside is that the invariant check must live in each `init` setter, or in a separate validation method called after construction — there is no single "all fields present and valid" moment comparable to the end of a constructor body.

The recommended pattern for a domain aggregate root is a **private constructor with a public static factory method**, combining both approaches: the constructor validates invariants in one place, and the factory method provides a named, readable creation entry point:

```csharp
public sealed class CustomerOrder
{
    public string OrderId { get; }
    public string CustomerId { get; }
    public IReadOnlyList<OrderItem> Items { get; }
    public DateTimeOffset PlacedAt { get; }

    private CustomerOrder(string orderId, string customerId,
        IReadOnlyList<OrderItem> items, DateTimeOffset placedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
            throw new ArgumentException("Order must have at least one item.");
        OrderId = orderId;
        CustomerId = customerId;
        Items = items;
        PlacedAt = placedAt;
    }

    public static CustomerOrder Create(
        string customerId, IReadOnlyList<OrderItem> items)
        => new(Guid.NewGuid().ToString("N"), customerId,
               items, DateTimeOffset.UtcNow);
}
```

Use `required init` for DTOs crossing API boundaries where serialization flexibility matters. Use a private constructor with a factory for domain aggregates where invariant correctness at creation time is non-negotiable and the type should never be constructed by a deserializer directly.
