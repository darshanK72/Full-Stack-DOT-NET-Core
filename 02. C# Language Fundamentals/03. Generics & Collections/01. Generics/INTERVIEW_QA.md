# C# Generics — Interview Q&A

> Topics: generic classes, generic methods, generic interfaces, type constraints, variance (covariance/contravariance), open vs closed generic types, type inference, generic collections vs non-generic, reflection with generics, performance benefits of generics

---

## Foundation Questions

---

## Q1. What are generics in C# and why were they introduced?

**Concepts**
- Parameterized type definitions
- Compile-time type safety
- Code reuse without duplication
- Elimination of boxing/unboxing overhead
- Replacement for `object`-based containers

**Answer**

Generics allow you to define classes, methods, interfaces, and delegates with a type placeholder that the caller fills in at compile time. Before generics (pre-.NET 2.0), the only way to write truly reusable data structures was to store and accept `object`, which meant losing type safety and incurring boxing overhead every time a value type was stored or retrieved.

The compiler enforces the type argument at the call site, so a `List<int>` will simply refuse to accept a `string` — you get a build error rather than a runtime `InvalidCastException`. At the JIT level, generics are not mere templates that get erased; value-type specializations generate entirely separate native code, so `List<int>` never boxes its elements. Reference types share a single JIT body but still benefit from the type-safe API surface.

The practical outcome is that generics let a single implementation of a stack, queue, or repository serve many types without copy-pasting and without sacrificing performance or correctness. That is why they are the foundation of the entire .NET collections library (`List<T>`, `Dictionary<TKey,TValue>`, etc.) and are used pervasively in frameworks like ASP.NET Core, EF Core, and MediatR.

---

## Q2. How do you declare a generic class and a generic method?

**Concepts**
- Type parameter syntax (`<T>`)
- Generic class declaration
- Generic method declaration
- Standalone vs class-scoped type parameters
- Multiple type parameters

**Answer**

A generic class is declared by appending angle-bracket type parameters after the class name: `public class Stack<T> { ... }`. Any member inside that class can use `T` as if it were a concrete type. A generic method is declared by placing the type parameter list between the method name and its parameter list: `public T Max<T>(T a, T b) where T : IComparable<T> { ... }`. A generic method does not need to live inside a generic class; it can sit on a non-generic class and introduce its own type parameters.

When a generic class itself is also generic, the method can use the class-level parameter, its own method-level parameter, or both simultaneously. Multiple type parameters are separated by commas: `class Pair<TFirst, TSecond>`. By convention, a single type parameter uses `T`, additional ones use descriptive names like `TKey`, `TValue`, `TResult`.

The distinction matters for type inference: the compiler infers method-level type parameters from argument types at the call site, whereas class-level parameters must be supplied explicitly when constructing the object (though C# 9+ target-typed `new()` and collection expressions reduce that ceremony).

---

## Q3. What are type constraints, and what kinds does C# support?

**Concepts**
- `where T : class` (reference type)
- `where T : struct` (non-nullable value type)
- `where T : new()` (parameterless constructor)
- `where T : SomeBaseClass` (base class constraint)
- `where T : ISomeInterface` (interface constraint)
- `where T : notnull` (.NET 5+)
- Multiple constraints on one parameter

**Answer**

Type constraints restrict what concrete types are allowed as arguments for a type parameter. Without constraints, the only operations available on `T` inside the generic body are those defined on `object` — you cannot call `T`-specific methods, compare values, or instantiate the type.

`where T : class` guarantees that `T` is a reference type, enabling null assignments and reference-equality checks. `where T : struct` guarantees a non-nullable value type, enabling `Nullable<T>` scenarios and avoiding null checks entirely. `where T : new()` requires a public parameterless constructor, allowing `new T()` inside the body — useful for factory patterns. A base-class constraint like `where T : Animal` ensures every `T` has the members of `Animal`, enabling polymorphic calls. An interface constraint like `where T : IComparable<T>` exposes comparison methods without knowing the concrete type.

Multiple constraints on one parameter are written as a comma-separated list after the colon: `where T : class, IRepository, new()`. If both a base class and interfaces are specified, the base class must come first. The `notnull` constraint (C# 8 / .NET 5+) integrates with nullable reference type analysis to prevent passing a nullable reference or value as `T`. Understanding constraints is critical because they expand what you can do with `T` while keeping the generic body fully verifiable by the compiler.

---

## Q4. What is the difference between an open generic type and a closed generic type?

**Concepts**
- Open generic type (unbound type parameters)
- Closed generic type (all parameters supplied)
- Runtime representation (`typeof`)
- `IsGenericTypeDefinition` flag
- JIT specialization timing

**Answer**

An open generic type is a type whose type parameters have not yet been substituted with concrete types. `List<T>` by itself is an open generic type — it exists in metadata but cannot be instantiated directly. A closed generic type is the result of supplying all type arguments: `List<int>`, `List<string>`, and `List<MyClass>` are all separate closed types derived from the same open definition.

At runtime in .NET, both forms have `Type` representations. `typeof(List<>)` returns the open generic type definition; its `IsGenericTypeDefinition` property returns `true`. `typeof(List<int>)` returns a closed type whose `IsGenericTypeDefinition` is `false` and whose `GetGenericArguments()` returns `[typeof(int)]`.

The JIT defers code generation until a closed type is first used. For value-type arguments, the JIT generates a specialized native code body (so `List<int>` and `List<double>` have different machine code). For reference-type arguments, a single shared code body is reused because pointers are the same size regardless of the referent type.

This distinction matters in reflection-driven scenarios: you cannot call `Activator.CreateInstance` on an open type — you must first call `openType.MakeGenericType(typeof(int))` to produce a closed type, and then instantiate it. Dependency injection containers like Microsoft.Extensions.DependencyInjection use exactly this mechanism to register and resolve open generic services such as `IRepository<>`.

---

## Q5. How does type inference work for generic methods?

**Concepts**
- Compiler-inferred type arguments
- Inference from method argument types
- Return-type inference limitations
- Explicit type argument override
- Ambiguity and failure cases

**Answer**

Type inference allows you to call a generic method without spelling out the type arguments when the compiler can deduce them from the method's argument list. Given `public T Identity<T>(T value)`, a call like `Identity(42)` infers `T = int` because the literal `42` has type `int`, and the parameter is typed `T`. You may still write `Identity<int>(42)` explicitly, but it is redundant.

The compiler performs inference only from the types of supplied arguments, not from the expected return type. If the inference would be ambiguous — for instance, when two arguments each constrain `T` to different types — the compiler reports an error rather than picking one silently. In such cases you supply the type argument explicitly.

Inference extends to lambda parameters in LINQ. When you write `list.Select(x => x.Length)`, the compiler infers the delegate's input and output types from the list's element type and the lambda body, threading them through the generic parameters of `Select<TSource, TResult>`. This is why LINQ queries read fluidly without angle brackets.

Type inference does not cross method boundaries or flow backward from the call's surrounding context, so it cannot infer a return-only parameter. If a method returns `Func<T>` and takes no `T`-typed argument, the caller must supply the type explicitly.

---

## Q6. What is `default(T)` and when would you use it?

**Concepts**
- Zero-value for value types
- `null` for reference types
- Generic code without knowing T's kind
- `default` literal (C# 7.1+)
- Initialization patterns in generic containers

**Answer**

`default(T)` produces the default value of whatever type `T` resolves to at runtime. For value types it returns the zero-initialized form: `0` for numeric types, `false` for `bool`, and an all-zero struct. For reference types and nullable value types it returns `null`. Because generic code does not know at write time whether `T` will be a struct or a class, `default(T)` is the only safe way to express "give me a valid, harmless starting value of type `T`."

A common use case is implementing a generic stack, queue, or pool where you need to clear out a slot after popping an element to prevent the collection from inadvertently holding object references that would block garbage collection:

```csharp
public T Pop()
{
    T value = _items[--_size];
    _items[_size] = default;   // release reference for GC
    return value;
}
```

In C# 7.1 the `default` literal was introduced, dropping the repetition of the type name when the type can be inferred from context: you can write `T result = default;` instead of `T result = default(T);`. The two forms are semantically identical; the shorter form is preferred in modern code. Understanding `default(T)` is also essential when writing generic algorithms that need a sentinel "no result" value without mandating that `T` be nullable.

---

## Q7. How do generic interfaces work, and what is variance?

**Concepts**
- Generic interface declaration
- Covariance (`out` keyword)
- Contravariance (`in` keyword)
- Variance only on reference types
- `IEnumerable<out T>` and `IComparer<in T>` examples

**Answer**

A generic interface declares type parameters just like a generic class: `interface IRepository<T>`. Implementing classes supply a concrete type: `class OrderRepo : IRepository<Order>`. This lets code program against the abstraction without caring about the concrete entity.

Variance extends generic interfaces to allow safe implicit conversions in an inheritance hierarchy. Covariance, declared with `out T`, means the interface produces `T` values but never consumes them. Because every `Cat` is an `Animal`, an `IEnumerable<Cat>` is safely assignable to `IEnumerable<Animal>` — reading a `Cat` from the sequence and treating it as an `Animal` is always valid. Contravariance, declared with `in T`, means the interface only consumes `T`. An `IComparer<Animal>` can compare any two animals, so it can safely stand in for an `IComparer<Cat>` — if you need a comparer for cats, a comparer that handles all animals is more capable, not less.

The invariant case (no `in`/`out`) — which is the default — means the type parameter appears in both input and output positions, so no implicit conversion is safe. `IList<Cat>` is not assignable to `IList<Animal>` because the list's `Add` method would allow inserting a `Dog`.

Variance applies only to generic interfaces and generic delegates, and only when the type argument is a reference type. Value type arguments always produce invariant behaviour regardless of the annotation.

---

## Q8. What is covariance and contravariance on generic delegates?

**Concepts**
- Delegate covariance (`Func<out TResult>`)
- Delegate contravariance (`Action<in T>`)
- Built-in `Func<T>` and `Action<T>` variance
- Method group conversions
- Practical assignability examples

**Answer**

Generic delegates in .NET support the same variance annotations as interfaces. The built-in `Func<TResult>` delegate is covariant in its return type: a `Func<Cat>` can be assigned to a `Func<Animal>` because every call to the delegate will return a `Cat`, which is an `Animal`. Conversely, `Action<T>` is contravariant in its parameter: an `Action<Animal>` can be assigned to an `Action<Cat>` because the handler accepts any animal, and a `Cat` is certainly an animal.

These annotations are built into the framework's delegate definitions. `Func<in T, out TResult>` is contravariant in the input and covariant in the output — matching the Liskov substitution principle precisely: you can replace a function with one that accepts a more general input and returns a more specific output without breaking callers.

In practical terms, variance on delegates enables clean composition in LINQ and reactive pipelines. You can pass a `Func<DerivedEntity>` wherever a `Func<BaseEntity>` is expected without an explicit cast. Method group conversions also benefit: a method `Animal GetAnimal()` matches a `Func<Cat>` target if covariance is in play, though this exact case requires the method signature to return `Cat` or a subtype. Understanding delegate variance prevents puzzling compile errors when wiring event handlers and LINQ projections across type hierarchies.

---

## Q9. How do generic collections differ from non-generic collections like `ArrayList` and `Hashtable`?

**Concepts**
- Type safety at compile time
- Boxing/unboxing elimination for value types
- `ArrayList` vs `List<T>`
- `Hashtable` vs `Dictionary<TKey,TValue>`
- Runtime cast failures in non-generic collections

**Answer**

Non-generic collections such as `ArrayList` and `Hashtable` store and return `object` references. Any value type placed into them undergoes boxing — the value is wrapped in a heap-allocated object — and must be explicitly cast and unboxed on retrieval. Both of these operations have measurable overhead, and the cast is not verified until runtime, so a stray incorrect element produces an `InvalidCastException` at an unpredictable moment, often far from where the mistake was made.

Generic collections eliminate both problems. `List<int>` stores raw `int` values in an internal `int[]` array; no boxing ever occurs. The type parameter is enforced at compile time, so inserting a `string` into a `List<int>` is a build error rather than a runtime surprise. `Dictionary<TKey,TValue>` provides the same benefits for key-value storage: keys and values are typed, no object casts are needed, and hash operations on structs avoid boxing.

The performance difference is most pronounced with value types at high throughput. Benchmarks consistently show that tight loops over a `List<int>` are significantly faster than the equivalent `ArrayList` code because they avoid heap allocations for each element. The non-generic collections exist for backward compatibility with pre-.NET 2.0 code; all new code should use their generic counterparts. The `System.Collections.Generic` namespace contains the canonical set: `List<T>`, `Dictionary<TKey,TValue>`, `HashSet<T>`, `Queue<T>`, `Stack<T>`, `SortedDictionary<TKey,TValue>`, and more.

---

## Q10. How do static fields behave in generic classes?

**Concepts**
- Per-closed-type static field instances
- Type specialization at the JIT level
- Shared state per type argument
- Practical use in caching type-specific data
- Common misconception about shared statics

**Answer**

Static fields in a generic class are not shared across all instantiations of that class. Each closed generic type gets its own independent set of static fields. `Cache<int>.Count` and `Cache<string>.Count` are entirely separate variables; incrementing one does not affect the other.

This happens because `Cache<int>` and `Cache<string>` are genuinely distinct types at runtime — they have separate `Type` objects, separate vtables, and separate static storage. The JIT treats them as unrelated except for their common open-type origin.

This behaviour can be exploited deliberately for type-keyed caching. A pattern like the following avoids dictionary lookups entirely:

```csharp
static class TypeCache<T>
{
    public static readonly string Name = typeof(T).Name;
}
```

`TypeCache<int>.Name` is computed and stored once for `int`, and a separate computation is stored once for `string`. The field read is a direct memory access with no hashing or locking.

The trap is writing code that expects statics to be shared across all `T` variants — for example, a counter intended to track total instances across all uses of the generic class. Such a counter must live on a separate non-generic class. Misunderstanding this behaviour leads to subtle concurrency bugs and confusing state isolation issues that are very hard to diagnose once the application is under load.

---

## Q11. How does reflection interact with generic types?

**Concepts**
- `typeof(List<>)` open type definition
- `Type.MakeGenericType`
- `IsGenericType`, `IsGenericTypeDefinition`
- `GetGenericArguments`
- `MethodInfo.MakeGenericMethod`

**Answer**

The reflection API exposes generics through two distinct `Type` representations. `typeof(List<>)` returns the open generic type definition; its `IsGenericTypeDefinition` is `true` and it cannot be instantiated. `typeof(List<int>)` returns a closed type with `IsGenericType == true`, `IsGenericTypeDefinition == false`, and `GetGenericArguments()` returning `[typeof(int)]`.

To manufacture a closed type at runtime from an open one, call `openType.MakeGenericType(typeof(int))`. The returned `Type` is fully usable: you can pass it to `Activator.CreateInstance`, register it in a DI container, or call `GetMethod` on it. This is how ASP.NET Core's DI container resolves open generic registrations — when you register `typeof(IRepository<>)` mapped to `typeof(EfRepository<>)`, the container calls `MakeGenericType` with the requested entity type each time a service is resolved.

For generic methods, `MethodInfo.MakeGenericMethod(typeof(int))` closes the method's type parameter at runtime, enabling dynamic invocation of methods like `JsonSerializer.Deserialize<T>` with a type only known at runtime.

Reflection on generics is slower than direct calls but unavoidable in framework code. Source generators (.NET 5+) and `TypedResults` APIs exist partly to push such reflection to compile time, but understanding the runtime model remains essential for debugging serializers, ORMs, and plugin systems that rely on type discovery.

---

## Q12. What performance advantages do generics provide for value types?

**Concepts**
- Boxing allocation on the heap
- Unboxing and cast overhead
- JIT value-type specialization
- GC pressure reduction
- Benchmark impact in tight loops

**Answer**

The most significant performance advantage of generics over `object`-based containers is the elimination of boxing. When a value type such as `int`, `double`, or a custom `struct` is stored in a variable of type `object`, the runtime allocates a small heap object to wrap the value. Every such allocation adds pressure to the garbage collector, and every retrieval requires both a null check and a type-verified uncast. In throughput-sensitive code — sorting a large array, summing a financial ledger, or processing a sensor stream — this overhead accumulates to a measurable cost.

With a generic container, the JIT generates a specialized version of the compiled method body for each distinct value-type argument. `List<int>` internally maintains an `int[]` array; elements are stored and read as bare `int` values on the stack and in CPU registers, with no heap allocation beyond the array itself. The GC sees fewer objects, pause times shrink, and memory locality improves because elements are packed contiguously.

Microbenchmarks using BenchmarkDotNet consistently show that summing a `List<int>` is several times faster than the equivalent `ArrayList` loop, with the gap widening as the collection grows. For reference types, the benefit is mainly type safety rather than raw speed, since references are the same size regardless of the referent type and share a single JIT body. But for value types — especially hot-path number crunching, span-based processing, or high-frequency struct collections — choosing a generic container is an architectural decision with direct runtime consequences.

---

## Q13. What is a generic delegate and how do `Func<T>` and `Action<T>` fit in?

**Concepts**
- Generic delegate declaration
- `Func<TResult>` and its overloads
- `Action<T>` and its overloads
- `Predicate<T>`
- Replacing custom delegate types

**Answer**

A generic delegate is a delegate type parameterized over one or more types. Before the BCL provided `Func` and `Action`, every project declared its own `delegate void Callback<T>(T item)` family of types, leading to fragmentation and compatibility issues. The framework's built-in generic delegates eliminate that need.

`Action<T>` represents a void-returning method that takes one argument of type `T`. Its overloads go up to sixteen parameters: `Action<T1, T2, ..., T16>`. `Func<TResult>` represents a method that returns `TResult` and takes no arguments; `Func<T, TResult>` takes one argument; and so on up to `Func<T1, ..., T16, TResult>` where the last type argument is always the return type. `Predicate<T>` is a specialized delegate equivalent to `Func<T, bool>`, used in methods like `List<T>.FindAll`.

Using these standard delegates keeps API surfaces consistent. LINQ is built almost entirely on `Func` overloads, so lambda expressions wire up naturally without explicit delegate conversions. The variance annotations on `Func` and `Action` (contravariant inputs, covariant return) discussed in Q8 further increase their composability. You should introduce a custom named delegate only when you need the name to carry semantic meaning that the generic form cannot express — for example, `EventHandler<TEventArgs>` adds the `(sender, args)` semantic that a bare `Action<object, TEventArgs>` would not convey.

---

## Q14. Can you use generics with interfaces that have multiple type parameters?

**Concepts**
- Multi-parameter generic interfaces
- Explicit interface implementation with generics
- Disambiguation of conflicting implementations
- `IConverter<TFrom, TTo>` pattern
- Constraint interactions across parameters

**Answer**

Yes. Generic interfaces can declare as many type parameters as needed, each with its own independent constraints. A converter abstraction is a classic example: `interface IConverter<TFrom, TTo> { TTo Convert(TFrom input); }`. An implementing class can be generic itself (`class JsonConverter<T> : IConverter<string, T>`) or fully concrete (`class UtcConverter : IConverter<DateTime, DateTimeOffset>`).

When a class implements the same generic interface multiple times with different type arguments — for instance, `class MultiConverter : IConverter<string, int>, IConverter<string, double>` — the two implementations must be provided as explicit interface implementations to disambiguate. Calling `((IConverter<string, int>)instance).Convert(...)` routes to the first; calling the double variant routes to the second.

Constraints on multiple parameters can reference each other. `interface IMap<TKey, TValue> where TKey : IEquatable<TKey>` constrains only `TKey` while leaving `TValue` unconstrained. Both constraints can also appear: `where TKey : notnull where TValue : class, new()`. The compiler validates each constraint independently.

Multi-parameter generic interfaces are central to patterns like CQRS, where a handler interface `IRequestHandler<TRequest, TResponse>` separates the command/query type from its result type, allowing each combination to be registered and resolved independently by the DI container.

---

## Q15. What happens when you define a generic class that inherits from another generic class?

**Concepts**
- Generic inheritance
- Forwarding type parameters
- Partially closed base classes
- Method override with generic types
- `new()` and covariant return types

**Answer**

A generic class can inherit from another generic class in several ways. It can forward its own type parameter to the base: `class SpecialList<T> : List<T>`. It can partially close the base, fixing one parameter: `class StringDictionary<TValue> : Dictionary<string, TValue>`. Or it can fully close the base with concrete types: `class IntList : List<int>`, which is no longer generic itself.

When a derived class overrides a virtual method defined in the generic base, the override must respect the signature established by the closed type. In `IntList : List<int>`, overriding `Add` means declaring `override void Add(int item)` — the type parameter is already resolved to `int`.

Forwarding constraints from the derived class to the base is handled automatically; the base class's constraints are inherited along with its definition. If the base requires `where T : struct`, the derived class cannot relax that by omitting the constraint — the compiler enforces it.

Covariant return types (C# 9+) interact with generics carefully: you can return a more derived type in an override, but the generic parameter itself is fixed by the base's declaration and cannot be made more specific through covariance. Understanding generic inheritance is essential when building layered abstraction stacks — such as an `EfRepository<T>` extending `BaseRepository<T>` extending `IRepository<T>` — because each layer's type parameter management determines what code can be shared and what must be overridden.

---

## Q16. What is the `where T : unmanaged` constraint and when is it useful?

**Concepts**
- Unmanaged type definition
- Pointer and `stackalloc` operations
- `Span<T>` and unsafe interop
- Performance-critical code patterns
- Difference from `where T : struct`

**Answer**

The `unmanaged` constraint (C# 7.3+) restricts `T` to types that contain no managed references: primitive numeric types, `bool`, `char`, enums, pointers, and structs whose fields are all recursively unmanaged. It is stricter than `where T : struct`, which allows structs containing reference-type fields.

The primary benefit is that `unmanaged` types have a fixed, known memory layout with no GC roots, enabling the use of pointer arithmetic, `stackalloc`, `sizeof(T)`, and pinning inside the generic method body:

```csharp
public static unsafe T ReadBigEndian<T>(ReadOnlySpan<byte> buffer)
    where T : unmanaged
{
    T value;
    MemoryMarshal.Read(buffer, out value);
    return value;
}
```

This pattern appears in network protocol parsers, binary serializers, and native interop layers where you want a single generic implementation for `int`, `float`, `Guid`, and custom fixed-layout structs without writing one method per type.

`Span<T>` itself uses `unmanaged`-like restrictions internally; methods like `MemoryMarshal.Cast<TFrom, TTo>` require unmanaged types. Understanding this constraint matters when writing high-performance, allocation-free code that bridges managed C# with native APIs or binary wire formats. Without it, you would need either separate overloads or reflection-based marshalling, both of which are slower.

---

## Q17. How do generic type parameters interact with nullable reference types (NRTs) in .NET 10?

**Concepts**
- `T?` in a generic context
- `where T : notnull` constraint
- Nullable annotations on unconstrained parameters
- `[MaybeNull]` and `[NotNull]` attributes
- NRT flow analysis limitations with generics

**Answer**

Nullable reference type analysis and generics intersect in subtle ways that the compiler must navigate carefully. When `T` is unconstrained, it might be a reference type (where `T?` means nullable reference) or a value type (where `T?` means `Nullable<T>`). To write `T?` in a generic body and have it mean "possibly null" for both kinds, you need either two separate constrained overloads or the `[MaybeNull]` attribute.

The `where T : notnull` constraint tells the compiler that `T` is always non-null, enabling you to annotate return types and parameters with the understanding that a `T` is never null. Violating this — returning `null` from a method with `T` return type when `notnull` is in effect — produces a nullable warning.

For unconstrained `T` where you need to express a possibly-absent result, the `[MaybeNull]` return attribute is the escape hatch: `[return: MaybeNull] public T GetOrDefault<T>(string key)`. The attribute instructs the NRT analyzer to treat the return value as potentially null even though `T` itself carries no explicit `?`.

In .NET 10, the BCL is fully annotated. `Dictionary<TKey,TValue>.TryGetValue` uses `[MaybeNullWhen(false)]` on its out parameter to convey that the value is non-null when the method returns `true`. Reading these annotations in IDE tooltips and understanding how they flow through your own generic code is essential for writing warning-free, null-safe APIs that consume modern .NET libraries correctly.

---

## Q18. How do generics support the repository pattern, and what does a generic base repository look like?

**Concepts**
- Generic repository abstraction
- `IRepository<T>` interface
- Base class code reuse
- EF Core `DbSet<T>` integration
- CRUD operation generalization

**Answer**

The repository pattern isolates data access logic from business logic. Generics make it possible to define a single interface and base implementation that covers all entities, with each concrete repository inheriting the common CRUD operations rather than re-implementing them.

A typical interface: `interface IRepository<T> where T : class`. A generic EF Core base implementation closes over `DbContext` and delegates to `DbSet<T>` for queries, inserts, updates, and deletes. Concrete repositories like `OrderRepository : EfRepository<Order>` inherit all common operations and only add query methods specific to `Order`.

The pattern works naturally with dependency injection. You register the open generic: `services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))`. ASP.NET Core's DI container calls `MakeGenericType` at resolution time to produce the specific closed type needed. This means every entity type gets a properly typed repository without any registration boilerplate per entity.

Generic constraints protect the pattern: `where T : class` prevents value types (which EF Core cannot track); adding `where T : BaseEntity` ensures every entity has an `Id` property that the `GetById` method can reference. The combination of generics, constraints, and open generic DI registration is one of the most impactful places in a .NET codebase where mastery of generics directly reduces boilerplate and maintenance burden.

---

## Gotchas

---

## Q19. Why is `List<Animal>` not assignable to `List<Cat>` even though `Cat` derives from `Animal`?

**Concepts**
- Invariance of generic classes
- Type safety violation scenario
- Contrast with covariant interfaces
- Array covariance vs generic invariance
- `IEnumerable<T>` as the safe workaround

**Answer**

`List<T>` is an invariant generic type, meaning `List<Cat>` and `List<Animal>` are treated as completely unrelated types even though `Cat : Animal`. The reason is that `List<T>` both produces and consumes `T`: it has `Add(T item)` (consumer) and `T this[int index]` (producer). If `List<Cat>` were assignable to `List<Animal>`, the following code would compile but violate type safety:

```csharp
List<Cat> cats = new List<Cat>();
List<Animal> animals = cats; // hypothetically allowed
animals.Add(new Dog());       // Dog is an Animal — compiles, but breaks cats!
Cat c = cats[0];              // runtime exception or memory corruption
```

This is why generic classes are invariant by default. The `out`/`in` variance annotations are only available on interfaces and delegates — and only when the type parameter appears exclusively in output or input positions respectively.

The safe substitution when you only need to read elements is to use the covariant interface: `IEnumerable<Cat>` is assignable to `IEnumerable<Animal>` because `IEnumerable<out T>` is declared covariant and only exposes a read-forward enumerator. Arrays in .NET actually do allow covariant assignment (`Cat[] = new Cat[]` can be assigned to `Animal[]`), but this is a legacy design decision that the runtime compensates for with an array element type check on every write — an approach that trades performance and safety for historical compatibility. Generics made the correct invariant choice.

---

## Q20. Why does `static T _instance` in a generic class create a separate field per type argument?

**Concepts**
- Per-closed-type static storage
- Common singleton anti-pattern in generics
- Intentional type-keyed cache pattern
- Thread safety implications
- Comparison with non-generic static fields

**Answer**

Every closed generic type gets its own copy of all static members. If you write a generic class with a `static T _instance` intending to share one instance across all callers, you will be surprised to find that `MyService<int>._instance` and `MyService<string>._instance` are independent variables. Initializing one does not affect the other; both start as `default(T)`.

This trips up developers who try to implement the Singleton pattern inside a generic class:

```csharp
public class Singleton<T> where T : new()
{
    private static T _instance = new T();  // separate per T!
    public static T Instance => _instance;
}
```

`Singleton<int>.Instance` and `Singleton<string>.Instance` are distinct objects stored in different static fields. This is actually correct behaviour if your intent was a type-keyed registry, but it breaks the classic single-instance guarantee. The fix is to move the singleton responsibility to a non-generic holder, or to use a `ConcurrentDictionary<Type, object>` keyed by `typeof(T)` if the type-keyed behavior is desired but must be accessed from a single non-generic entry point.

The deeper point is that understanding static field lifecycle in generics is essential for designing thread-safe initializers, avoiding memory leaks in plugin-loaded generic assemblies, and correctly reasoning about test isolation when statics accumulate state per type argument during a test run.

---

## Q21. What goes wrong when you call a generic method via reflection but forget to call `MakeGenericMethod`?

**Concepts**
- Open method invocation failure
- `InvalidOperationException` from reflection
- `MakeGenericMethod` requirement
- Runtime type argument supply
- Contrast with compile-time invocation

**Answer**

When you obtain a `MethodInfo` for a generic method via `typeof(SomeClass).GetMethod("Process")`, you get the open method definition — a `MethodInfo` whose `IsGenericMethodDefinition` is `true`. Calling `methodInfo.Invoke(target, args)` on this open definition throws an `InvalidOperationException` with the message "Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true."

The fix is to first close the method by calling `methodInfo.MakeGenericMethod(typeof(int))`, which returns a new `MethodInfo` representing the specific instantiation `Process<int>`. Only then can `Invoke` succeed.

This mistake is common when dynamically invoking CQRS handlers or deserializers: you discover the handler method at runtime, but forget that the method is generic and needs to be closed with the actual message type before invocation. The error message is clear once you know what to look for, but it can be confusing the first time because the method name and parameters all look correct in the debugger.

A related gotcha: if you cache the closed `MethodInfo` for performance, cache it keyed by the type argument (e.g., in a `ConcurrentDictionary<Type, MethodInfo>`) rather than caching the open definition and re-closing it each call. The `MakeGenericMethod` call itself allocates a new object on each invocation, so in hot paths the cached-closed-method approach avoids repeated allocation.

---

## Q22. Why can you not use a value type as a type argument when the constraint is `where T : class`?

**Concepts**
- Reference type constraint semantics
- Value type classification
- Nullable structs vs reference types
- Compile-time enforcement
- Interaction with nullable reference types

**Answer**

The `where T : class` constraint mandates that any type argument must be a reference type — a class, interface, delegate, or array. Value types (`int`, `double`, `bool`, and any struct) are excluded unconditionally. The compiler enforces this at the call site: `MyGeneric<int>` where the class declares `where T : class` is a build error, not a runtime error.

The motivation is that certain operations are only valid for reference types. Null assignment (`T x = null`) is legal only if `T` is guaranteed to be a reference type. Reference-equality checks (`Object.ReferenceEquals`) and interface implementations that rely on null sentinels all require this guarantee.

A subtle confusion arises with `string`: because `string` is a reference type, `where T : class` accepts it. But `string?` in a nullable-enabled context is still a reference type (nullable annotation, not `Nullable<T>`), so it satisfies the constraint as well. Conversely, `int?` is `Nullable<int>`, which is a struct, and it therefore fails the `class` constraint despite superficially looking "nullable."

If you need to write a generic method that works with both structs and classes but needs null-return capability, use an unconstrained `T` with `[MaybeNull]` on the return, or split into two overloads — one constrained to `class` returning `T?` directly, one constrained to `struct` returning `T?` as `Nullable<T>`. .NET 10's BCL uses both approaches in different APIs.

---

## Q23. What is the variance pitfall when assigning `IEnumerable<DerivedType>` to `IEnumerable<BaseType>` and then mutating through it?

**Concepts**
- Covariance read-only guarantee
- No mutation through covariant interface
- `IList<T>` invariance
- Covariance as "producer" contract
- Practical misunderstanding in team codebases

**Answer**

`IEnumerable<T>` is covariant: `IEnumerable<Cat>` is assignable to `IEnumerable<Animal>`. This is safe because `IEnumerable<T>` only exposes a `GetEnumerator` that lets you read elements; there is no `Add` or mutation method. The covariance promise is that you will only ever get `T` values out of the interface, never put them in.

The pitfall occurs when developers assume that because the assignment compiled, they are free to cast back to a mutable type and modify the collection. For example:

```csharp
IEnumerable<Animal> animals = new List<Cat> { new Cat() };
((List<Cat>)animals).Add(new Cat()); // fine — same underlying type
((IList<Animal>)animals).Add(new Dog()); // InvalidCastException at runtime
```

The second cast to `IList<Animal>` compiles because `List<Cat>` might implement `IList<Animal>` in the developer's mental model, but it does not — `IList<T>` is invariant, so `List<Cat>` does not implement `IList<Animal>`. The `InvalidCastException` arrives at runtime.

The lesson is that covariant assignment is a read contract. If downstream code needs to add to or replace elements, pass a type that explicitly supports it, such as `IList<T>` constrained to the exact element type. Accepting `IEnumerable<T>` in a method signature is a clear signal that the method will only read, which both enforces good API design and enables callers to pass arrays, LINQ sequences, and other read-only sources.

---

## Real-World Scenarios

---

## Q24. You are reviewing a generic repository in an EF Core project. Identify the issues.

```csharp
public class Repository<T> where T : class
{
    private readonly DbContext _context;

    public Repository(DbContext context) => _context = context;

    public List<T> GetAll() => _context.Set<T>().ToList();

    public T GetById(int id) => _context.Set<T>().Find(id);

    public void Save(T entity)
    {
        _context.Set<T>().Add(entity);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var entity = GetById(id);
        _context.Set<T>().Remove(entity);
        _context.SaveChanges();
    }

    public List<T> Search(string term)
    {
        return _context.Set<T>()
            .Where(e => e.ToString().Contains(term))
            .ToList();
    }
}
```

**Concepts**
- Unit of work / `SaveChanges` granularity
- `Find` vs `FirstOrDefaultAsync`
- Null safety on `GetById` result
- `ToString()` as a query predicate
- Missing async API surface

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Delete` calls `GetById` which may return `null` (EF `Find` returns null if not found), then passes `null` to `Remove` — throws `ArgumentNullException` | Runtime crash on missing id |
| Performance | `GetAll().ToList()` fetches every row into memory with no pagination or projection | Out-of-memory on large tables; kills DB server |
| Query Translation | `e.ToString().Contains(term)` cannot be translated to SQL; EF Core loads all rows to memory then filters client-side | Full table scan, O(n) memory allocation |
| API Design | All methods are synchronous; EF Core is designed for async I/O | Thread pool starvation under load in ASP.NET Core |
| Encapsulation | `SaveChanges` called inside every individual operation; impossible to batch multiple repository calls in one transaction | Data inconsistency; unit of work pattern violated |

**Fix Priority**

1. Add null check (or `FindAsync` + `NotFoundException`) in `Delete` to prevent the immediate `ArgumentNullException`.
2. Convert `GetAll`, `GetById`, `Save`, and `Delete` to `async` / `await` returning `Task<T>`.
3. Remove `SaveChanges` from individual methods; expose a `CommitAsync()` method or rely on the outer Unit of Work (`IUnitOfWork`) to call it.
4. Replace `Search`'s `ToString` predicate with a proper expression parameter: `Search(Expression<Func<T, bool>> predicate)`, delegating filtering to EF Core and pushing the query to the database.
5. Add pagination parameters (`int skip, int take`) to `GetAll` to cap result set size.

---

## Q25. A teammate registers a CQRS handler with open generics but requests fail silently. How do you diagnose and fix it?

**Concepts**
- Open generic DI registration
- `typeof(IRequestHandler<,>)` pattern
- MediatR or custom dispatcher mechanics
- `MakeGenericType` at resolution time
- Assembly scanning vs manual registration

**Answer**

The symptom — requests dispatched through a mediator that return nothing or throw a "no handler found" exception — usually traces to one of three misconfigured open generic registrations in the DI container.

The first issue is registering the closed type instead of the open type. Writing `services.AddScoped<IRequestHandler<CreateOrder, OrderDto>, CreateOrderHandler>()` works only for that specific command. When the dispatcher resolves `IRequestHandler<CancelOrder, Unit>`, the container has no mapping. The fix is to use assembly scanning: `services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))`, which discovers and registers every `IRequestHandler<,>` implementation automatically.

The second issue occurs when a custom dispatcher uses reflection to locate the handler. After retrieving the open `IRequestHandler<,>` type from the container, the developer forgets to call `MakeGenericType` before resolution. The container receives an unbound open type and either throws or returns null:

```csharp
// Wrong — passing open generic to GetService
var handlerType = typeof(IRequestHandler<,>);
var handler = _provider.GetService(handlerType); // returns null

// Correct — close the type first
var closedType = typeof(IRequestHandler<,>)
    .MakeGenericType(requestType, responseType);
var handler = _provider.GetRequiredService(closedType);
```

The third issue is a missing service registration for the concrete handler class itself. Some minimal DI setups require explicit registration of the implementation even when the interface is registered as open generic.

Diagnosis steps: enable DI validation (`ValidateOnBuild = true` in the host builder), add logging inside the dispatcher to print the resolved type, and write a test that resolves each handler type from the container explicitly during startup.

---

## Q26. You are designing a generic `Result<T>` type for operation outcomes. Discuss the design decisions.

**Concepts**
- Discriminated union simulation in C#
- `implicit operator` for ergonomics
- `where T : notnull` constraint choice
- Covariant result interface
- Pattern matching integration

**Answer**

A `Result<T>` type wraps either a successful value of type `T` or an error, preventing callers from ignoring failure paths (unlike exceptions, which unwind silently). The core design decision is the representation: store a `bool IsSuccess`, a `T Value`, and an `Error` in the same struct, or use a sealed class hierarchy.

The struct approach avoids heap allocation but requires `default(T)` for the value field in the failure case, which means `T` should be constrained to `notnull` to prevent ambiguity between "null success value" and "failure." Alternatively, use a `bool` flag and guarantee that `Value` is only read when `IsSuccess` is true, enforcing this with a guard throw.

Implicit operators improve ergonomics: `public static implicit operator Result<T>(T value) => Result<T>.Success(value)` lets methods `return entity;` directly. Similarly, `public static implicit operator Result<T>(Error error) => Result<T>.Failure(error)` lets `return Errors.NotFound;` work without construction ceremony.

Pattern matching (C# 8+ `switch` expressions) integrates naturally with a `Deconstruct` method: `var (ok, value, error) = result;`. This makes chaining results in application service layers readable.

For interfaces, declare `IResult<out T>` as covariant so a `Result<DerivedEntity>` can be assigned to `IResult<BaseEntity>` in read-only contexts. This is especially useful in pipeline patterns where upstream handlers return specific types but downstream consumers only care about the base.

In .NET 10, `Result<T>` interacts cleanly with nullable reference types when constrained to `notnull`: the `Value` property can be typed `T` (non-nullable), and the compiler prevents reading it in the failure branch when proper guard flow is in place.

---

## Q27. Your serializer must deserialize JSON into a generic type only known at runtime. How do you use reflection with generics to accomplish this?

**Concepts**
- `JsonSerializer.Deserialize<T>` generic method
- `MethodInfo.MakeGenericMethod` for runtime T
- `Type.GetMethod` overload resolution
- Caching closed `MethodInfo` in a dictionary
- Performance tradeoffs vs source-generated serialization

**Answer**

`System.Text.Json.JsonSerializer.Deserialize<T>(string json)` is a generic method. When the target type `T` is only known at runtime (e.g., retrieved from a database configuration table or a plugin manifest), you cannot use it directly with angle brackets. The reflection path is:

```csharp
private static readonly ConcurrentDictionary<Type, MethodInfo> _methodCache = new();

public static object DeserializeAny(string json, Type targetType)
{
    var method = _methodCache.GetOrAdd(targetType, t =>
    {
        var openMethod = typeof(JsonSerializer)
            .GetMethod(nameof(JsonSerializer.Deserialize),
                       new[] { typeof(string), typeof(JsonSerializerOptions) });
        return openMethod!.MakeGenericMethod(t);
    });

    return method.Invoke(null, new object?[] { json, null })!;
}
```

There are several gotchas here. `JsonSerializer` has multiple `Deserialize` overloads, so `GetMethod` by name alone is ambiguous; you must supply parameter types to `GetMethod` to select the correct overload. The open `MethodInfo` retrieved by `GetMethod` has `IsGenericMethodDefinition == true`; calling `MakeGenericMethod(targetType)` on it produces the closed `MethodInfo` that `Invoke` can actually call.

Caching in `ConcurrentDictionary<Type, MethodInfo>` is essential because `MakeGenericMethod` allocates a new `MethodInfo` object each invocation. Without the cache, a high-throughput endpoint that deserializes many messages will produce significant GC pressure.

For production code, prefer `JsonSerializer.Deserialize(json, targetType)` — the non-generic overload that accepts a `Type` directly — which avoids reflection entirely and is significantly faster. The reflection approach is appropriate when you are wrapping a third-party library whose only entry point is a generic method with no non-generic equivalent.

---

## Q28. A high-frequency trading system processes market ticks stored in a generic ring buffer. The buffer performs poorly with value types. Diagnose and fix.

```csharp
public class RingBuffer<T>
{
    private readonly object[] _buffer;
    private int _head, _tail;

    public RingBuffer(int capacity)
    {
        _buffer = new object[capacity];
    }

    public void Write(T item) => _buffer[_tail++ % _buffer.Length] = item;

    public T Read() => (T)_buffer[_head++ % _buffer.Length];

    public bool IsEmpty => _head == _tail;
}
```

**Concepts**
- Boxing on `object[]` storage
- Generic array `T[]` as replacement
- Lock-free ring buffer design
- GC pressure in tight loops
- `Volatile.Read`/`Volatile.Write` for thread safety

| Category | Problem | Impact |
|---|---|---|
| Performance | `object[]` storage boxes every value type write and unboxes every read | Continuous heap allocations, GC pauses in microsecond-latency code |
| Thread Safety | `_head` and `_tail` are not accessed with any memory barrier; concurrent read/write produces torn indices | Data corruption, stale reads under multi-threaded use |
| Correctness | Overwrite detection absent: when the buffer is full, `Write` silently overwrites unread data | Silent data loss for slow consumers |
| Correctness | Integer overflow: `_head` and `_tail` are plain `int`; after 2 billion operations they wrap negative and the modulo produces incorrect indices | Buffer corruption in long-running systems |

**Fix Priority**

1. Replace `object[]` with `T[]` so value types are stored directly without boxing: `private readonly T[] _buffer;`. This is the highest-impact single change.
2. Wrap `_head` and `_tail` as `volatile int` fields and use `Interlocked.Increment` (or `Volatile.Read`/`Volatile.Write`) for safe multi-producer/multi-consumer access.
3. Add full-buffer detection: before writing, check `(_tail - _head) >= _buffer.Length` and either throw, return false, or block depending on the required back-pressure semantics.
4. Use `long` (or `uint` with intentional wrapping arithmetic) for the indices to handle the overflow case in a 24/7 system.
5. Consider `System.Threading.Channels.Channel<T>` or `System.Collections.Concurrent.ConcurrentQueue<T>` before hand-rolling a ring buffer; both are generic, allocation-aware, and heavily battle-tested.
