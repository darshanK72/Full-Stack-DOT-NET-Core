# C# Static Members & Static Classes — Interview Q&A


## Table of Contents

1. [Q1. What is a static member in C#, and how does it differ from an instance member?](#q1-what-is-a-static-member-in-c-and-how-does-it-differ-from-an-instance-member)
2. [Q2. What is a static class, and what restrictions does the C# compiler enforce on it?](#q2-what-is-a-static-class-and-what-restrictions-does-the-c-compiler-enforce-on-it)
3. [Q3. What is a static constructor, and when is it guaranteed to run?](#q3-what-is-a-static-constructor-and-when-is-it-guaranteed-to-run)
4. [Q4. What is the difference between `static readonly` and `const`?](#q4-what-is-the-difference-between-static-readonly-and-const)
5. [Q5. What thread-safety problems arise from mutable static fields, and how do you address them?](#q5-what-thread-safety-problems-arise-from-mutable-static-fields-and-how-do-you-address-them)
6. [Q6. What are extension methods, and why must they live in static classes?](#q6-what-are-extension-methods-and-why-must-they-live-in-static-classes)
7. [Q7. What is the Singleton design pattern, and how does a static class differ from a Singleton?](#q7-what-is-the-singleton-design-pattern-and-how-does-a-static-class-differ-from-a-singleton)
8. [Q8. Can static members access instance members, and vice versa?](#q8-can-static-members-access-instance-members-and-vice-versa)
9. [Q9. What happens to static state when multiple threads access a static counter concurrently without synchronization?](#q9-what-happens-to-static-state-when-multiple-threads-access-a-static-counter-concurrently-without-synchronization)
10. [Q10. What is `static readonly object _lock = new()` used for, and when is `lock` insufficient?](#q10-what-is-static-readonly-object-lock-new-used-for-and-when-is-lock-insufficient)
11. [Q11. What is `ThreadLocal<T>`, and when should it be used instead of a static field?](#q11-what-is-threadlocalt-and-when-should-it-be-used-instead-of-a-static-field)
12. [Q12. How does a static class in C# handle the Singleton concern differently from a manual Singleton pattern?](#q12-how-does-a-static-class-in-c-handle-the-singleton-concern-differently-from-a-manual-singleton-pattern)
13. [Q13. A static field in an ASP.NET Core controller stores the "current user's cart" to avoid DI. What specific failure mode will occur under concurrent requests?](#q13-a-static-field-in-an-aspnet-core-controller-stores-the-current-users-cart-to-avoid-di-what-specific-failure-mode-will-occur-under-concurrent-requests)
14. [Q14. A static constructor reads a config file. What happens if the file is missing on the first deployment, and how can it be recovered without restarting?](#q14-a-static-constructor-reads-a-config-file-what-happens-if-the-file-is-missing-on-the-first-deployment-and-how-can-it-be-recovered-without-restarting)
15. [Q15. Why is `public static class TaxHelper` with a constructor and instance field a compile error?](#q15-why-is-public-static-class-taxhelper-with-a-constructor-and-instance-field-a-compile-error)
16. [Q16. Why does registering `AuditLogger.Instance` as a singleton in DI and using a non-thread-safe `_entryCount++` field produce wrong log counts under load?](#q16-why-does-registering-auditloggerinstance-as-a-singleton-in-di-and-using-a-non-thread-safe-entrycount-field-produce-wrong-log-counts-under-load)
17. [Q17. An ASP.NET Core API uses a static list to cache all products. Under moderate load, `InvalidOperationException: Collection was modified` appears. Diagnose and fix.](#q17-an-aspnet-core-api-uses-a-static-list-to-cache-all-products-under-moderate-load-invalidoperationexception-collection-was-modified-appears-diagnose-and-fix)
18. [Q18. A static counter generates bank account numbers in production. Under high concurrency, duplicate account numbers appear. How do you fix it with `Interlocked`?](#q18-a-static-counter-generates-bank-account-numbers-in-production-under-high-concurrency-duplicate-account-numbers-appear-how-do-you-fix-it-with-interlocked)
19. [Q19. A developer adds mutable `public static` properties for per-request config in a multi-instance ASP.NET Core deployment. What breaks?](#q19-a-developer-adds-mutable-public-static-properties-for-per-request-config-in-a-multi-instance-aspnet-core-deployment-what-breaks)

---
## Foundation Questions

---

## Q1. What is a static member in C#, and how does it differ from an instance member?

**Concepts**
- Belongs to the type, not any instance
- Single shared copy per `AppDomain`
- Accessed via type name, not object reference
- Instance members require an object; static do not
- Memory allocation at type load time

**Answer**

A static member belongs to the type itself rather than to any particular instance. There is exactly one copy of a static field for the entire application domain, regardless of how many instances of the class exist. You access static members using the type name — `Counter.Total`, not `myCounter.Total`. Instance members, by contrast, have a separate copy for each object; they exist only after `new` creates the instance and are GC-collected when the instance is no longer referenced. The practical consequence is that static state is inherently global and shared, which makes it useful for counters, configuration constants, and utility methods that have no meaningful per-object state, but dangerous in multithreaded environments where concurrent access without synchronization causes data races.

---

## Q2. What is a static class, and what restrictions does the C# compiler enforce on it?

**Concepts**
- Cannot be instantiated (`new` is a compile error)
- Cannot be inherited
- All members must be static
- Implicit `abstract` and `sealed`
- Useful for stateless utility/extension method containers

**Answer**

A static class is declared with the `static` modifier and serves as a container for members that have no instance state. The compiler enforces three non-negotiable rules: you cannot use `new` to create an instance, you cannot inherit from a static class, and every member of the class must itself be static. Internally the compiler marks static classes as both `abstract` and `sealed`, which is why instantiation and inheritance are forbidden. The canonical use cases are pure utility helpers (e.g., `Math`, `Path`, `File`) and extension method containers. Because static classes cannot be instantiated, they have no instance constructor, though they may have a static constructor. The restriction that all members must be static is enforced at compile time; attempting to add an instance field or method produces an error immediately rather than surprising behavior at runtime.

---

## Q3. What is a static constructor, and when is it guaranteed to run?

**Concepts**
- Type initializer (`.cctor`)
- Runs before first instance creation or first static member access
- Runs exactly once per `AppDomain`
- Thread-safe by CLR guarantee
- No access modifier or parameters allowed

**Answer**

A static constructor (type initializer) is a special method declared as `static MyClass()` with no parameters or access modifier. The CLR guarantees it runs exactly once per `AppDomain`, before the first instance of the type is created or any static member is accessed, whichever comes first. The CLR also guarantees thread safety for the static constructor — if multiple threads simultaneously trigger type initialization, only one runs the constructor while others block until it completes. This guarantee makes static constructors suitable for one-time initialization of complex static state, such as loading a static lookup table from embedded resources. The main risk is that any unhandled exception escaping a static constructor wraps in `TypeInitializationException` and permanently faults the type for that `AppDomain`'s lifetime.

---

## Q4. What is the difference between `static readonly` and `const`?

**Concepts**
- `const` — compile-time constant, baked into call sites
- `static readonly` — runtime constant, evaluated once
- `const` requires primitive or string types
- `static readonly` allows reference types and complex expressions
- Binary versioning difference

**Answer**

`const` declares a compile-time constant whose value is inlined into every call site's IL at compile time. Because of this, `const` supports only primitive types (`int`, `bool`, `decimal`, `string`, etc.) and its value must be a literal expression. `static readonly` is initialized once at runtime — either at field declaration or in the static constructor — and is read from memory at each call. This distinction has a significant versioning implication: if you change a `const` value in a library, consuming assemblies see the old value until they are recompiled, because the old value is baked into their IL. A `static readonly` change takes effect immediately at runtime because callers always read from the field. The practical guideline is: use `const` only for truly invariant values (mathematical constants, sentinel strings) and `static readonly` for everything that could logically change between releases.

---

## Q5. What thread-safety problems arise from mutable static fields, and how do you address them?

**Concepts**
- Shared mutable state across all threads
- Read-modify-write not atomic
- `Interlocked` for atomic operations
- `lock` for multi-field consistency
- `ThreadLocal<T>` for per-thread isolation

**Answer**

A mutable static field is effectively global state shared by every thread in the process. A `_nextAccountNumber++` operation reads the value, adds one, and writes back — three non-atomic steps. Two threads executing simultaneously can both read the same value, both add one, and both write the same result, so you get two increments but the counter only advances by one. For simple numeric counters, `Interlocked.Increment(ref _next)` provides an atomic read-modify-write. For compound operations involving multiple fields that must change together consistently, use `lock` with a dedicated `static readonly object _sync = new()`. For state that must be different per thread — such as a per-request correlation ID — `ThreadLocal<T>` gives each thread its own independent copy without synchronization overhead. In ASP.NET Core, mutable static state is particularly dangerous because the thread pool reuses threads across requests, meaning `ThreadLocal` values from a previous request can leak into subsequent ones unless explicitly reset.

---

## Q6. What are extension methods, and why must they live in static classes?

**Concepts**
- `this` parameter prefix on first argument
- Syntactic sugar — compiles to static call
- Must be in a non-generic static class
- Appear on intellisense as instance methods
- Cannot override existing instance methods

**Answer**

Extension methods allow you to add new methods to an existing type without modifying its source code, creating a subclass, or using a wrapper. They are declared as `public static` methods in a `public static` non-generic class, with the first parameter prefixed by `this`: `public static bool IsEmpty(this string s)`. The compiler translates `myString.IsEmpty()` into the static call `StringExtensions.IsEmpty(myString)` at the call site; there is no runtime magic. Extension methods must be in static classes because they are always static calls — there is no instance state in the method itself. They appear in IntelliSense on the extended type as if they were instance members, which is their UX benefit. Extension methods cannot override existing instance methods: if an instance method and an extension method have the same signature, the instance method always wins. Extension methods are the foundation of LINQ — all `Where`, `Select`, `OrderBy` etc. are extension methods on `IEnumerable<T>`.

---

## Q7. What is the Singleton design pattern, and how does a static class differ from a Singleton?

**Concepts**
- Singleton — single-instance guarantee with controlled access
- Static class — no instance at all, just members
- Singleton can implement interfaces (static class cannot)
- Singleton lazy initialization
- DI-managed singleton vs manual singleton

**Answer**

A Singleton ensures at most one instance of a class exists per application lifetime and provides a global access point. A static class, by contrast, has no instance at all — it cannot implement interfaces, cannot be passed as a reference, and cannot be mocked in tests. Singletons are implemented with a private constructor and a static property returning the single instance, often using `Lazy<T>` for thread-safe lazy initialization: `private static readonly Lazy<Service> _instance = new(() => new Service())`. The critical practical advantage of the Singleton pattern over static classes is testability: a Singleton can implement an interface (e.g., `ICache`), allowing DI to substitute a mock in tests. Static classes cannot implement interfaces. In modern ASP.NET Core, the recommended approach is to register the class as a singleton in the DI container (`builder.Services.AddSingleton<ICache, Cache>()`) rather than using the static Singleton pattern, because the DI container handles the single-instance guarantee while preserving interface-based abstraction.

---

## Q8. Can static members access instance members, and vice versa?

**Concepts**
- Static methods have no implicit `this`
- Instance methods can access static members via type name
- Static method cannot use `this` or access instance fields directly
- Must pass an instance reference explicitly
- Common compiler error CS0120

**Answer**

Static methods have no `this` reference — they are not associated with any instance — so they cannot directly access instance fields or call instance methods. Attempting to do so produces CS0120 ("An object reference is required for the non-static field, method, or property"). However, instance methods can freely access static members because the static member belongs to the type and is always available without a reference. A static method that needs to work with instance data must receive an instance as a parameter. This asymmetry is fundamental: instance members operate on a specific object's state, while static members express behavior or state that is independent of any particular object. A common design smell is a static utility class that takes an instance of a type as a parameter to do work that logically belongs on the instance — a sign that the behavior should be an instance method instead.

---

## Q9. What happens to static state when multiple threads access a static counter concurrently without synchronization?

**Concepts**
- Non-atomic increment (`++`)
- Race condition (TOCTOU)
- Lost update problem
- `Interlocked.Increment` as fix
- Memory visibility / cache coherency

**Answer**

The `++` operator on a static integer is not atomic. It decompiles to three operations: load the current value from memory, add one, store the result back. Under concurrent access, two threads can interleave these steps: both read `1000`, both compute `1001`, and both write `1001`. Net effect: two increments happened but the counter advanced by only one. Over thousands of requests, the counter drifts significantly below the expected value, and in the case of account numbers, this produces duplicates. `Interlocked.Increment(ref _next)` performs the read-modify-write atomically using CPU-level locked instructions (LOCK XADD on x86), preventing any other thread from interleaving. Beyond correctness, there is also a memory visibility concern on multi-core systems: without a memory barrier, each core may read a stale cached copy of the field. `Interlocked` operations include implicit full memory barriers, ensuring all threads see the updated value.

---

## Q10. What is `static readonly object _lock = new()` used for, and when is `lock` insufficient?

**Concepts**
- `lock` for mutual exclusion
- `lock` object as a private, dedicated monitor
- Avoids deadlocks from locking on `this` or public objects
- `lock` is blocking — affects throughput
- `lock` insufficient for async code (`SemaphoreSlim`)

**Answer**

`lock(obj) { }` acquires an exclusive monitor lock on `obj`, blocking other threads until the lock is released. Using a private `static readonly object _lock = new()` as the lock target is the recommended pattern because it prevents external code from acquiring the same lock, which could cause deadlocks. Locking on `this` is discouraged because callers can lock on your instance from outside and interfere. Locking on a public or accessible object is similarly risky. The limitation of `lock` is that it is a synchronous blocking construct — it cannot be used across `await` points in async methods. For async code, `SemaphoreSlim(1, 1)` with `await sem.WaitAsync()` and `sem.Release()` in a finally block provides the equivalent non-blocking mutual exclusion. For read-heavy scenarios, `ReaderWriterLockSlim` allows multiple concurrent readers and exclusive writers, improving throughput over a plain lock.

---

## Q11. What is `ThreadLocal<T>`, and when should it be used instead of a static field?

**Concepts**
- Per-thread isolated storage
- Each thread gets its own value
- Initialized lazily per thread
- Dispose required to release thread-affine slots
- Leaks in thread-pool scenarios if not cleaned up

**Answer**

`ThreadLocal<T>` provides a separate value of type `T` for each thread that accesses it. Unlike a plain static field where all threads share the same value, `ThreadLocal<T>` initializes a distinct instance per thread on first access, using an optional factory lambda. This is ideal for per-request context (like a correlation ID), per-thread caches, or thread-unsafe objects (like `Random` pre-.NET 6) where you want one instance per thread to avoid synchronization. In thread-pool environments like ASP.NET Core, you must be careful: worker threads are reused across requests, so a `ThreadLocal<T>` value set during one request persists on that thread and may be read by the next request handled on the same thread. Always clear or re-initialize `ThreadLocal<T>` values at the start of a new logical operation. `ThreadLocal<T>` implements `IDisposable` and should be disposed when the holding class is no longer needed to release the CLR's thread-static slot entries.

---

## Q12. How does a static class in C# handle the Singleton concern differently from a manual Singleton pattern?

**Concepts**
- Static class cannot implement interfaces
- Static class cannot be lazy without explicit initialization
- Manual Singleton can be interface-backed and DI-friendly
- Static class lifetime is the AppDomain
- `Lazy<T>` for thread-safe lazy Singleton

**Answer**

A static class and a manual Singleton both provide a single globally-accessible set of data and behavior, but they have fundamentally different capabilities. A static class cannot implement any interface, cannot be assigned to a variable, and cannot be passed as an argument of its own type — it has no type identity beyond `Type`. A Singleton class is a normal reference type that happens to limit instantiation; it can implement `ICache` or `ILogger`, can be registered in a DI container, and can be replaced by a test double in unit tests. Thread-safe lazy initialization with a Singleton uses `Lazy<T>`: `private static readonly Lazy<MyService> _instance = new(() => new MyService())`, where the `Lazy` constructor guarantees single execution. In production codebases, the explicit `AddSingleton<T>()` DI registration is preferred over the manual Singleton pattern because the container handles lazy creation, lifetime, and disposal while keeping the type loosely coupled via its interface.

---

## Gotchas — Static Members & Static Classes (Interview Traps)

---

#### Gotcha 1. Static fields are shared across all threads — concurrent mutation without synchronization corrupts state

**Concepts**
- Static field has one copy per AppDomain
- All threads access the same location
- Non-atomic operations cause races
- `lock` or `Interlocked` for thread safety
- No per-request isolation

**Answer**

A static field exists once in the process. When multiple threads read and write it concurrently without synchronization, the result is a data race — each thread can overwrite another's write or read a partially-written state. For counters, use `Interlocked.Increment`; for collections, use `ConcurrentDictionary`; for request-scoped data, use DI scoped services instead of static fields.

---

#### Gotcha 2. Static constructor faults permanently — TypeInitializationException on every subsequent access if it throws

**Concepts**
- Static ctor runs once per type per AppDomain
- Exception marks type as permanently faulted
- `TypeInitializationException` wraps the original
- No retry possible — restart the process
- `Lazy<T>` with `PublicationOnly` allows retries

**Answer**

If a static constructor throws, the CLR marks the type as failed and wraps the exception in `TypeInitializationException`. Every later attempt to use any member of that type throws the same exception — there is no retry path without restarting the process. Move fallible I/O out of static constructors and into `Lazy<T>` or explicit `Initialize()` methods.

---

#### Gotcha 3. A static class cannot have instance constructors or instance fields — CS0710

**Concepts**
- Static classes: all members must be static
- No instance can be created
- CS0710 for instance members
- Instance state belongs in a regular class or record
- Utility classes vs service classes

**Answer**

The compiler enforces that every member of a `static class` is itself static. Adding an instance field or a non-static constructor causes CS0710. If per-instance state is needed, remove `static` from the class declaration and register it as a DI service. Reserve `static class` for pure stateless utility methods.

---

#### Gotcha 4. Static constructors have no access modifier — adding one is a compile error

**Concepts**
- Access modifier not allowed on static ctors
- CLR-controlled invocation only
- CS0515 if access modifier present
- Private is implied but not writable
- Contrast with instance constructors

**Answer**

Static constructors are invoked by the CLR, never directly by user code, so specifying an access modifier such as `public` or `private` on them is a CS0515 compile error. The static constructor is implicitly private to the CLR's type system — you write `static MyClass() { }` with no modifier.

---

#### Gotcha 5. static readonly field vs const — constants are embedded at call site, readonly fields are resolved at runtime

**Concepts**
- `const` value embedded in caller's IL
- Changing `const` requires recompiling callers
- `static readonly` resolved at runtime from the defining assembly
- Breaking vs non-breaking versioning difference
- `const` only for values stable across versions

**Answer**

A `const` value is baked into the caller's compiled IL; if the library changes the constant and only the library DLL is replaced, existing callers continue using the old value until recompiled. A `static readonly` field is read from the library at runtime, so callers always see the current value. Use `const` only for values guaranteed never to change (mathematical constants, fixed protocol codes), and prefer `static readonly` for everything else in a library.

---

#### Gotcha 6. Extension methods are static but dispatched as instance methods — null check is not automatic

**Concepts**
- Extension method receives `this` as first parameter
- `null` can be passed as `this`
- No NullReferenceException until the body accesses the value
- Guard `this` parameter explicitly
- Behaviour differs from instance method dispatch

**Answer**

An extension method `public static string Truncate(this string s, int max)` can be called as `nullString.Truncate(10)` without immediately throwing. The `null` is passed as the `s` parameter. If the body does not check for null before using `s`, you get a `NullReferenceException` inside the extension method, not at the call site, making the stack trace confusing. Always guard the `this` parameter: `if (s is null) return string.Empty;`.

---

#### Gotcha 7. Static methods on generic types share the static member only within the same closed type

**Concepts**
- `Cache<string>._count` is separate from `Cache<int>._count`
- One static field per closed generic type
- Surprising when expecting a shared counter
- Intentional per-type caches are a valid pattern
- Must use a non-generic class for truly shared static state

**Answer**

A static field in `Cache<T>` exists separately for each closed type: `Cache<string>._count` and `Cache<int>._count` are different fields. A counter intended to track all cache accesses across all `T` values will silently under-count because each closed type has its own copy. Place shared-across-all-T state in a non-generic companion class.

---

#### Gotcha 8. Singleton pattern via static field is hard to test — DI registered singleton is preferred

**Concepts**
- Static singleton cannot be replaced in tests
- DI singleton is scoped to the container
- Container lifetime control vs static lifetime
- `AddSingleton<T>()` for DI-managed singletons
- Static singleton survives test runs unless reset

**Answer**

A static singleton (`static readonly Lazy<T> _instance`) is a process-global object that cannot be swapped for a test double without reflection hacks. A DI-registered singleton (`services.AddSingleton<IService, Service>()`) is scoped to the container's lifetime, and tests can build their own container with a mock registered as the singleton. Always prefer DI singletons over static singletons for testability.

---

#### Gotcha 9. Static imports (using static) can shadow local names silently

**Concepts**
- `using static System.Math` brings `Abs`, `Sqrt`, etc. into scope
- Name collision with local method resolved by proximity
- Harder to trace which type owns the method
- Use sparingly — especially in large files

**Answer**

`using static System.Math` lets you write `Sqrt(x)` instead of `Math.Sqrt(x)`. If the same file also defines a local method or imports another static class with a method named `Sqrt`, the compiler resolves the nearest scope, which may not be the one intended. The ambiguity is a compile error only when both are equally accessible; otherwise a method silently shadows another. Limit `using static` to small, focused files where all imported names are immediately obvious.

---

#### Gotcha 10. Calling a static method through an instance reference compiles but is misleading — the instance is ignored

**Concepts**
- `obj.StaticMethod()` compiles
- Instance is not passed; only the type is used
- Resharper and Roslyn warnings flag this
- Misleads readers about dependency
- Use `ClassName.StaticMethod()` for clarity

**Answer**

C# allows calling a static method through an instance reference (`obj.StaticMethod()`), but the compiler discards `obj` entirely and emits a call to the static method on the type. Any null-check or side effect you expect from dereferencing `obj` does not occur. Code analysis tools (Roslyn, ReSharper) warn about this pattern because it misleads readers into thinking there is an instance dependency. Always use `ClassName.StaticMethod()` for static calls.

---

## Real-World Scenarios

---

## Q17. An ASP.NET Core API uses a static list to cache all products. Under moderate load, `InvalidOperationException: Collection was modified` appears. Diagnose and fix.

**Concepts**
- `List<T>` is not thread-safe
- Concurrent read and write during enumeration
- `ConcurrentBag<T>` vs `ImmutableList<T>` vs lock
- Snapshot pattern for safe enumeration
- Cache invalidation strategy

**Answer**

`List<T>` is explicitly documented as not thread-safe for concurrent reads and writes. When one thread iterates the list (in `foreach`) while another thread adds or removes an item, the iterator's internal version counter detects the modification and throws `InvalidOperationException: Collection was modified; enumeration operation may not execute`. With multiple concurrent requests, this happens unpredictably. The minimal fix is to protect all reads and writes with a `lock`, though this serializes all cache access and can become a throughput bottleneck. For a read-heavy product cache, `ImmutableList<T>` allows lock-free reads — writes create a new list and atomically swap the reference using `Interlocked.Exchange`. A more production-appropriate approach is `MemoryCache` from `Microsoft.Extensions.Caching.Memory`, registered as a singleton in DI, which is thread-safe by design, supports sliding and absolute expiration, and does not require manual synchronization.

---

## Q18. A static counter generates bank account numbers in production. Under high concurrency, duplicate account numbers appear. How do you fix it with `Interlocked`?

**Concepts**
- Non-atomic `_nextAccountNumber++`
- `Interlocked.Increment` for atomic read-modify-write
- `volatile` keyword vs `Interlocked`
- Sequence gaps under distributed deployments
- Database-generated sequence as the authoritative fix

**Answer**

The statement `AccountNumber = _nextAccountNumber++` is not atomic. It reads the current value, adds one, and writes back — three steps where two threads can interleave and both get the same account number. The single-process fix is `AccountNumber = Interlocked.Increment(ref _nextAccountNumber)`, which performs the read-add-write atomically using a CPU locked instruction. `volatile` alone is insufficient — it provides visibility (ensures no stale cached copy) but not atomicity of the multi-step increment. However, this in-process solution still breaks in any multi-instance deployment (Kubernetes pods, load-balanced servers) because each process has its own copy of `_nextAccountNumber`. The production-grade fix for a distributed system is to use a database sequence (`CREATE SEQUENCE account_seq` in SQL Server or PostgreSQL) or a distributed counter service (Redis `INCR`). The static counter approach is acceptable only for local demos or single-process utilities where uniqueness within a single process lifetime is sufficient.

---

## Q19. A developer adds mutable `public static` properties for per-request config in a multi-instance ASP.NET Core deployment. What breaks?

**Concepts**
- Static properties are process-scoped, not request-scoped
- Cross-request state contamination
- Multiple pod instances each have isolated static state
- Configuration should come from DI-scoped services or `IOptions<T>`
- `const` and `static readonly` are safe read-only; mutable statics are not

**Answer**

Mutable `public static` properties in ASP.NET Core are process-global. When request A sets `AppSettings.CurrentRegion = "EU"` and request B simultaneously runs and reads `AppSettings.CurrentRegion`, it may get "EU" even though B is a US request — request-level isolation is completely broken. On a single instance, this produces intermittent and hard-to-reproduce bugs. In a multi-instance deployment (load-balanced web farm, Kubernetes), each pod has its own copy of the static property, meaning setting it on pod 1 does not propagate to pod 2. Users hitting different pods see inconsistent behavior. The correct approach for request-scoped configuration is `IOptions<RegionConfig>` injected into scoped services, or `HttpContext.Items` for per-request data that does not belong in DI. `const` and `static readonly` are safe for truly immutable configuration because they cannot be mutated after initialization. Only `const` literals and immutable `static readonly` values are acceptable in multi-instance environments without explicit cross-process synchronization.
