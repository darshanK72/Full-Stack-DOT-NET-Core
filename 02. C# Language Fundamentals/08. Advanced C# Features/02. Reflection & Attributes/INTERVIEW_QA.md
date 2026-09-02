# Reflection & Attributes — Interview Q&A


## Table of Contents

1. [Q1. What is reflection in .NET and what can you do with it?](#q1-what-is-reflection-in-net-and-what-can-you-do-with-it)
2. [Q2. How do you get the Type of an object at runtime, and what are the three ways to obtain it?](#q2-how-do-you-get-the-type-of-an-object-at-runtime-and-what-are-the-three-ways-to-obtain-it)
3. [Q3. What is a custom attribute in C# and how do you define one?](#q3-what-is-a-custom-attribute-in-c-and-how-do-you-define-one)
4. [Q4. What does [AttributeUsage] control and what does Inherited = false mean?](#q4-what-does-attributeusage-control-and-what-does-inherited-false-mean)
5. [Q5. How do you invoke a method dynamically using MethodInfo.Invoke?](#q5-how-do-you-invoke-a-method-dynamically-using-methodinfoinvoke)
6. [Q6. What BindingFlags are needed to access private or non-public members?](#q6-what-bindingflags-are-needed-to-access-private-or-non-public-members)
7. [Q7. Why is reflection slow and how do you mitigate the cost in hot paths?](#q7-why-is-reflection-slow-and-how-do-you-mitigate-the-cost-in-hot-paths)
8. [Q8. What is Activator.CreateInstance and what are its limitations?](#q8-what-is-activatorcreateinstance-and-what-are-its-limitations)
9. [Q9. How do you use Type.GetType for cross-assembly plugin loading?](#q9-how-do-you-use-typegettype-for-cross-assembly-plugin-loading)
10. [Q10. What happens to reflection-based attribute scanning when Native AOT or PublishTrimmed is enabled?](#q10-what-happens-to-reflection-based-attribute-scanning-when-native-aot-or-publishtrimmed-is-enabled)
11. [Q11. What is the difference between GetCustomAttribute and IsDefined?](#q11-what-is-the-difference-between-getcustomattribute-and-isdefined)
12. [Q12. How do expression trees provide faster dynamic dispatch than raw MethodInfo.Invoke?](#q12-how-do-expression-trees-provide-faster-dynamic-dispatch-than-raw-methodinfoinvoke)
13. [Q13. Why does MethodInfo.Invoke wrap exceptions in TargetInvocationException?](#q13-why-does-methodinfoinvoke-wrap-exceptions-in-targetinvocationexception)
14. [Q14. Why does string-based method lookup via GetMethod break silently after a rename refactor?](#q14-why-does-string-based-method-lookup-via-getmethod-break-silently-after-a-rename-refactor)
15. [Q15. What happens when Inherited = false on an attribute and a subclass is checked?](#q15-what-happens-when-inherited-false-on-an-attribute-and-a-subclass-is-checked)
16. [Q16. Why is scanning GetExecutingAssembly().GetTypes() fragile in large solutions?](#q16-why-is-scanning-getexecutingassemblygettypes-fragile-in-large-solutions)
17. [Q17. (Code Review) A warehouse API loads pricing plugins from a separate assembly at runtime. It always returns null in staging. Review the loader.](#q17-code-review-a-warehouse-api-loads-pricing-plugins-from-a-separate-assembly-at-runtime-it-always-returns-null-in-staging-review-the-loader)
18. [Q18. A margin-report job reads private cost fields from DTOs but always gets null. What is the binding mistake?](#q18-a-margin-report-job-reads-private-cost-fields-from-dtos-but-always-gets-null-what-is-the-binding-mistake)
19. [Q19. An API discovers [EntityTable]-decorated types at startup, but they vanish after enabling PublishTrimmed. Why, and what are production-safe alternatives?](#q19-an-api-discovers-entitytable-decorated-types-at-startup-but-they-vanish-after-enabling-publishtrimmed-why-and-what-are-production-safe-alternatives)

---
> **Module:** 02. C# Language Fundamentals › 08. Advanced C# Features › 02. Reflection & Attributes  
> **Stack:** .NET 10 · System.Reflection · Custom Attributes · Source Generators

---

## Foundation Questions

---

## Q1. What is reflection in .NET and what can you do with it?

**Concepts**
- runtime type inspection
- Type, MethodInfo, PropertyInfo, FieldInfo
- Assembly and module metadata
- late binding and dynamic invocation
- performance cost of reflection

**Answer**

Reflection is the ability of .NET code to inspect and manipulate the metadata of assemblies, types, and members at runtime. The entry point is `Type` — obtained via `typeof(T)`, `obj.GetType()`, or `Type.GetType("Namespace.TypeName, Assembly")` — which exposes the type's name, base class, interfaces, constructors, methods, properties, fields, and custom attributes. From a `MethodInfo` you can call `Invoke(target, args)` to execute a method on an arbitrary object without knowing its type at compile time; from a `PropertyInfo` you can call `GetValue` and `SetValue` for dynamic property access. Reflection underpins many framework features: ASP.NET Core discovers controllers and action methods by scanning assemblies; ORMs build column mappings by reading property names and custom attributes; serializers enumerate properties to convert objects to JSON or XML. The primary cost is that reflection bypasses the JIT's optimization pipeline, involves boxing for value types, and requires security permission checks. For hot paths, cached reflection (storing `MethodInfo` in a dictionary keyed by type) or compiled expression trees that wrap reflective calls significantly reduce the per-call overhead.

---

## Q2. How do you get the Type of an object at runtime, and what are the three ways to obtain it?

**Concepts**
- typeof(T) compile-time operator
- obj.GetType() runtime instance method
- Type.GetType("AssemblyQualifiedName") string-based lookup
- cross-assembly resolution
- null safety

**Answer**

There are three ways to obtain a `Type` reference. The `typeof(T)` operator is evaluated at compile time and returns the `Type` for the named type directly — it requires the type to be known at compile time and is the most efficient option. The instance method `obj.GetType()` returns the runtime type of an object, which may differ from the declared type when dealing with polymorphism — calling it on a `List<string>` variable returns `System.Collections.Generic.List\`1[System.String]`. The static method `Type.GetType("Namespace.TypeName")` parses a type name string at runtime; without an assembly qualifier it searches only the calling assembly and `mscorlib`, which is why plugin loaders that use this form often return `null` in staging environments where the target assembly is not the executing assembly. The full form required for cross-assembly resolution is `Type.GetType("Namespace.TypeName, AssemblyName, Version=…, Culture=…, PublicKeyToken=…")`, or more practically, load the assembly first with `Assembly.LoadFrom(path)` and then call `assembly.GetType("Namespace.TypeName")`.

---

## Q3. What is a custom attribute in C# and how do you define one?

**Concepts**
- Attribute base class
- [AttributeUsage] to constrain targets
- AllowMultiple and Inherited parameters
- positional vs named attribute parameters
- GetCustomAttribute<T> retrieval

**Answer**

A custom attribute is a class that inherits from `System.Attribute`. It allows you to attach declarative metadata to types, methods, properties, parameters, assemblies, and other program elements, and then query that metadata at runtime via reflection or at compile time via source generators or Roslyn analyzers. You define one by creating a class with the `Attribute` suffix (conventional but not required), inheriting from `Attribute`, and applying `[AttributeUsage]` to constrain where the attribute may be placed. Constructor parameters become positional arguments in the attribute syntax; public settable properties become named arguments. For example: `[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]`. At runtime, `member.GetCustomAttribute<MyAttribute>()` returns the attribute instance if present, or `null` if absent. `AllowMultiple = false` prevents stacking the same attribute twice on one target; `Inherited = true` means that subclasses of an attributed class also carry the attribute when queried via `MemberInfo.GetCustomAttributes(inherit: true)`.

---

## Q4. What does [AttributeUsage] control and what does Inherited = false mean?

**Concepts**
- AttributeTargets enum flags
- AllowMultiple behavior
- Inherited = true vs false
- GetCustomAttributes(inherit: bool) overload
- IsDefined vs GetCustomAttribute

**Answer**

`[AttributeUsage(target, AllowMultiple, Inherited)]` controls three things. `AttributeTargets` is a flags enum that specifies valid attachment points — `Class`, `Method`, `Property`, `Parameter`, `Assembly`, and so on; placing an attribute on an invalid target is a compile error. `AllowMultiple = true` permits the same attribute to appear multiple times on the same element, with each instance retrieved as a separate object in `GetCustomAttributes`; the default is `false`. `Inherited` controls whether subclasses automatically inherit the attribute from a base class or interface implementation. When `Inherited = false`, a subclass does not see the attribute unless it reapplies it explicitly, even when `GetCustomAttributes(inherit: true)` is called. This is relevant for ORM table-name attributes: if `EntityTableAttribute` is defined with `Inherited = false`, applying it to a `Product` base class will not cause `PremiumProduct : Product` to map to the same table automatically — the ORM will not find the attribute on the subclass, which is usually a bug unless the design intention is that subclasses must explicitly opt in.

---

## Q5. How do you invoke a method dynamically using MethodInfo.Invoke?

**Concepts**
- MethodInfo from Type.GetMethod
- BindingFlags for non-public access
- Invoke(target, parameters) signature
- TargetInvocationException wrapping
- boxing of value-type arguments

**Answer**

`Type.GetMethod("MethodName")` by default searches public instance methods on the type. It returns a `MethodInfo` or `null` if not found. Calling `methodInfo.Invoke(targetInstance, new object[] { arg1, arg2 })` executes the method and returns the result boxed as `object`. For static methods, pass `null` as the target. The critical gotcha is that any exception thrown by the invoked method is wrapped in a `TargetInvocationException`; the real fault is in `ex.InnerException`, which is what you should log and (if rethrowing) propagate. Calling code that logs `TargetInvocationException` directly hides the root cause in production. For non-public methods, pass `BindingFlags.NonPublic | BindingFlags.Instance` to `GetMethod`; the default `BindingFlags` only surfaces public instance members. Value-type arguments are automatically boxed into the `object[]` parameter array, which adds allocation overhead on hot paths.

---

## Q6. What BindingFlags are needed to access private or non-public members?

**Concepts**
- BindingFlags.NonPublic
- BindingFlags.Instance vs Static
- default binding (public instance only)
- BindingFlags combinations
- encapsulation bypass and test concerns

**Answer**

`Type.GetField`, `Type.GetProperty`, and `Type.GetMethod` without explicit `BindingFlags` use a default of `BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static`, which means they only find public members. To access `private`, `internal`, or `protected` members you must pass `BindingFlags.NonPublic` explicitly. The full combination for private instance members is `BindingFlags.NonPublic | BindingFlags.Instance`; for private static members it is `BindingFlags.NonPublic | BindingFlags.Static`. Omitting `BindingFlags.Instance` or `BindingFlags.Static` causes `GetField` to return `null` even when the correct member name is provided, which is the most common source of "always gets null" bugs in reflection-based extractors. Accessing private members via reflection breaks encapsulation and is difficult to maintain through refactors — it is acceptable in test infrastructure and internal framework code but should be avoided in production application logic where a proper public API can be added instead.

---

## Q7. Why is reflection slow and how do you mitigate the cost in hot paths?

**Concepts**
- IL interpretation overhead
- boxing of value types
- security permission checks
- compiled expression tree caching
- ConcurrentDictionary<Type, PropertyInfo[]> cache pattern

**Answer**

Reflection is slow for several compounding reasons. `GetMethod`, `GetProperty`, and `GetField` scan the type's metadata tables each time they are called; there is no automatic caching at the call site. `MethodInfo.Invoke` bypasses the JIT's inlining and optimization passes, invokes the method through an interpreted thunk, and boxes every value-type argument and the return value. On top of this, the CLR performs access checks on every invocation. For a path called once at startup this is irrelevant, but for a CSV exporter invoked on 40 properties per row at 500 RPS the cumulative cost is measurable. The primary mitigation is to compute `PropertyInfo[]` once per type and store it in a `ConcurrentDictionary<Type, PropertyInfo[]>` built at startup. For even higher throughput, compile an expression tree that wraps the reflective `GetValue` call into a typed `Func<T, object>` delegate: the lambda is compiled to IL once and thereafter executes at near-native speed. Source generators (C# 9+ Roslyn) eliminate the reflection cost entirely by generating the mapping code at compile time.

---

## Q8. What is Activator.CreateInstance and what are its limitations?

**Concepts**
- dynamic type instantiation
- parameterless vs parameterized constructor
- Activator.CreateInstance<T> generic form
- constructor matching via GetConstructor
- performance cost vs compiled factory

**Answer**

`Activator.CreateInstance(type)` invokes the parameterless constructor of the given `Type` and returns the new instance as `object`. The generic form `Activator.CreateInstance<T>()` returns `T` directly and is slightly more efficient because it avoids a cast. When a parameterless constructor does not exist, the call throws `MissingMethodException`. To invoke a parameterized constructor, either pass an `object[]` of arguments to `Activator.CreateInstance(type, args)` — which searches for a matching constructor signature — or use `ConstructorInfo.Invoke(args)` for finer control. The main limitation is performance: every call goes through reflection rather than through a direct `new` allocation, which is typically 10–100x slower. For plugin loaders that instantiate the same type repeatedly, the pattern is to locate the constructor once, compile it into a `Func<T>` expression, cache that delegate, and use the delegate for all subsequent instantiations. `Activator.CreateInstance` is appropriate for plugin loading, test fixtures, and DI containers; avoid it in request-handling hot paths.

---

## Q9. How do you use Type.GetType for cross-assembly plugin loading?

**Concepts**
- assembly-qualified type name
- Assembly.LoadFrom / LoadContext
- AssemblyLoadContext isolation
- type resolution order
- staging vs development environment differences

**Answer**

`Type.GetType("TypeName")` without an assembly qualifier searches only the calling assembly and the core library. In a plugin loader this almost always returns `null` in staging because the plugin assembly is a separate DLL not loaded into the executing assembly's context. The correct approach for cross-assembly loading is to load the plugin assembly explicitly — via `AssemblyLoadContext.Default.LoadFromAssemblyPath(pluginDllPath)` for shared-context loading, or via a custom `AssemblyLoadContext` for isolation — and then call `assembly.GetType("Namespace.TypeName")` on the loaded assembly object. This guarantees the type is found regardless of how the runtime resolves assembly names. In .NET 6+ the preferred pattern for isolated plugins is a derived `AssemblyLoadContext` that loads the plugin DLL and its dependencies into a separate context, preventing version conflicts with the host application. Never pass user-controlled type names directly to `Type.GetType` or `Activator.CreateInstance` without validating against an allowlist — this is a deserialization-style gadget vulnerability.

---

## Q10. What happens to reflection-based attribute scanning when Native AOT or PublishTrimmed is enabled?

**Concepts**
- IL linker / trimmer removing unreferenced code
- reflection-rooted types via DynamicDependency
- RootDescriptor XML for trimming
- source generators as trimming-safe alternative
- [RequiresUnreferencedCode] warning

**Answer**

The .NET IL trimmer removes types, methods, and metadata that it cannot prove are reachable from the application's entry point. Reflection-based attribute scanning at runtime uses `Assembly.GetExecutingAssembly().GetTypes()` and `GetCustomAttribute<T>()`, which are opaque to the static analysis the trimmer performs — the trimmer cannot know which types will be accessed at runtime, so it may remove them. The result is that entity types decorated with `[EntityTable]` vanish from the export manifest with no compile-time error; the bug only manifests at runtime. The official mitigations are: annotate the scanning code with `[RequiresUnreferencedCode("Uses reflection")]` to produce a trimming warning for downstream callers; use `[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MyType))]` to root specific types into the trimmed output; or add a `TrimmerRootDescriptor.xml` file that lists all entity types. The production-safe long-term solution is source generators (Roslyn `IIncrementalGenerator`) which emit the attribute-scanning code at compile time, require no runtime reflection, and are fully compatible with Native AOT and trimming.

---

## Foundation Questions (continued)

---

## Q11. What is the difference between GetCustomAttribute and IsDefined?

**Concepts**
- IsDefined for existence check (no instance allocation)
- GetCustomAttribute for reading attribute values
- performance difference
- inherit parameter
- AttributeTargets.All scanning

**Answer**

`IsDefined(memberInfo, typeof(MyAttribute))` checks whether an attribute of a given type exists on a member without constructing the attribute instance. It is cheaper than `GetCustomAttribute` for the common case where you only need a yes/no answer — for example, checking whether a type is marked as exportable before adding it to a manifest. `GetCustomAttribute<T>(member)` constructs the attribute instance and returns it, giving you access to the property values set on the attribute. If you need both the check and the values, call `GetCustomAttribute` directly and check for `null`; calling `IsDefined` followed by `GetCustomAttribute` doubles the reflection work. Both methods accept an `inherit` boolean parameter that controls whether attributes on base classes and interfaces are considered. The subtle difference is that `IsDefined` with `inherit = true` traverses the inheritance chain, which means it can return `true` even if the attribute is not on the immediate declaring type.

---

## Q12. How do expression trees provide faster dynamic dispatch than raw MethodInfo.Invoke?

**Concepts**
- Expression.Lambda compilation to IL delegate
- one-time compilation cost amortized over many calls
- typed Func<T, object> vs object[] boxing in Invoke
- cached delegate pattern
- when to prefer compiled expressions vs source generators

**Answer**

A compiled expression tree generates actual IL code that the JIT can optimize, unlike `MethodInfo.Invoke` which executes through an interpreted thunk. The pattern is: build an `Expression<Func<T, object>>` tree that calls the target property getter or method, call `.Compile()` on it once to get a `Func<T, object>` delegate, store that delegate in a `ConcurrentDictionary<(Type, string), Func<object, object>>`, and call the delegate directly on each subsequent use. The delegate executes at near-native speed — effectively the same as a direct virtual dispatch — while the construction overhead is paid only once per unique type-member combination, typically at application startup. This approach is appropriate for generic ORMs, serializers, and validation frameworks that must work with any type. Source generators are preferable when the set of types is known at compile time, because they produce strongly typed code with zero runtime overhead, no startup compilation cost, and full Native AOT compatibility.

---

## Gotchas

---

## Q13. Why does MethodInfo.Invoke wrap exceptions in TargetInvocationException?

**Concepts**
- TargetInvocationException.InnerException
- logging the wrong exception
- ExceptionDispatchInfo.Capture / Throw for rethrow
- unwrapping pattern
- Splunk / telemetry showing surface exception

**Answer**

When `MethodInfo.Invoke` calls a method that throws, the CLR catches the exception and wraps it inside a `TargetInvocationException` before surfacing it to the caller. The stack trace and message of the original exception are preserved in `ex.InnerException`. Code that logs `TargetInvocationException` directly shows only "Exception has been thrown by the target of an invocation" in monitoring tools — the root cause is hidden one level down and requires manual drill-down to diagnose. The correct pattern is to catch `TargetInvocationException`, log `ex.InnerException` (or the full exception including inner), and rethrow the inner exception to preserve the original type for catch clauses upstream. To rethrow while preserving the stack trace, use `ExceptionDispatchInfo.Capture(ex.InnerException!).Throw()` rather than a bare `throw ex.InnerException` which resets the stack trace to the current location.

---

## Q14. Why does string-based method lookup via GetMethod break silently after a rename refactor?

**Concepts**
- string literals not refactor-safe
- nameof operator as compile-time guard
- null return from GetMethod after rename
- silent zero/null result propagation
- test coverage gap

**Answer**

`type.GetMethod("CalculateLineTotal")` is a raw string that the compiler treats as opaque — renaming the method in C# does not update the string, so after a refactor the method is simply not found. `GetMethod` returns `null`, which propagates silently when the caller does `result is decimal total ? total : 0m` — the fallback zero is returned as the line total with no exception, no log entry, and no test failure unless the test explicitly asserts on a non-zero value. The fix has two parts: replace the string with `nameof(T.MethodName)` so that a rename causes a compile error; and add a null guard on the `MethodInfo` that throws an explicit, descriptive exception rather than silently defaulting. Beyond the immediate fix, the broader design question is whether dynamic method lookup via reflection is the right approach at all — if the set of methods is known at compile time, a virtual dispatch or interface call is safer, faster, and refactor-proof.

---

## Q15. What happens when Inherited = false on an attribute and a subclass is checked?

**Concepts**
- Inherited = false skips base-class attribute
- GetCustomAttributes(inherit: false) vs (true)
- ORM / mapper base-class attribute not visible on derived type
- fix: change Inherited = true or re-apply attribute
- IsDefined behavior with inheritance

**Answer**

When `[AttributeUsage(…, Inherited = false)]` is set, calling `GetCustomAttributes(typeof(EntityTableAttribute), inherit: true)` on a subclass returns an empty array — the attribute on the base class is invisible. The CLR's `IsDefined` method behaves the same way. This is the source of "table not mapped" errors when an ORM reads the table-name attribute from a base `Product` class but a `PremiumProduct` subclass is not explicitly annotated. There are two correct fixes: change `Inherited = false` to `Inherited = true` on the attribute definition if all subclasses should inherit the mapping automatically; or have each subclass re-apply the attribute explicitly if each needs its own distinct mapping. The right choice depends on whether the inheritance chain represents the same entity (one table, inheritance is fine) or distinct entities (each subclass maps to a different table, explicit attributes per class). `Inherited = false` is appropriate for attributes that have no sensible meaning on a subclass — for example, an attribute that marks a class as a specific serialization root should not silently propagate to derived types that might have different serialization shapes.

---

## Q16. Why is scanning GetExecutingAssembly().GetTypes() fragile in large solutions?

**Concepts**
- GetExecutingAssembly scope (only one assembly)
- AppDomain.CurrentDomain.GetAssemblies() for loaded assemblies
- late-loaded assemblies missed
- performance of type scanning at startup
- Span of time between scan and first use

**Answer**

`Assembly.GetExecutingAssembly().GetTypes()` scans only the assembly containing the code that calls it. In a multi-project solution where entity types, controllers, or plugin types are defined in separate class library projects, those types are in different assemblies and will not appear in the scan even if those assemblies are loaded. The broader alternative `AppDomain.CurrentDomain.GetAssemblies()` returns all assemblies that have been loaded into the default AppDomain at the time of the call — but assemblies loaded lazily (on first type access) may not yet appear. The practical solution for framework-level scanning (ORM entity discovery, attribute-based route registration) is to require explicit registration via a builder API or a marker interface, supplemented by optional convention-based scanning of explicitly listed assemblies rather than relying on `GetExecutingAssembly`. Source generators are the compile-time alternative that completely eliminates runtime scanning.

---

## Real-World Scenarios

---

## Q17. (Code Review) A warehouse API loads pricing plugins from a separate assembly at runtime. It always returns null in staging. Review the loader.

```csharp
public sealed class PluginLoader
{
    public object? LoadPlugin(string typeName)
    {
        Type? pluginType = Type.GetType(typeName);
        if (pluginType is null)
        {
            return null;
        }
        return Activator.CreateInstance(pluginType);
    }
}
// Config: "Plugins:PricingType": "Acme.Pricing.VolumeDiscountPlugin"
```

**Concepts**
- Type.GetType without assembly qualifier
- cross-assembly resolution failure
- Assembly.LoadFrom / AssemblyLoadContext
- assembly-qualified name format
- staging vs developer machine assembly load order

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Type.GetType("Acme.Pricing.VolumeDiscountPlugin")` only searches executing assembly — returns null for types in external DLLs | All plugins silently fail to load in staging |
| Security | User-supplied type name passed directly to Activator.CreateInstance | Arbitrary type instantiation if input is not validated |
| Correctness | No null check or error on CreateInstance failure | Exceptions propagate uncaught |

**Fix priority:**
1. Load the plugin assembly explicitly: `Assembly.LoadFrom(pluginDllPath).GetType(typeName)`.
2. Validate `typeName` against an allowlist of known plugin type names before resolution.
3. Add a null guard and descriptive exception when `pluginType` is null: `throw new InvalidOperationException($"Plugin type '{typeName}' not found in assembly.")`.
4. Consider an `AssemblyLoadContext` for version-isolated plugin loading in .NET 10.

---

## Q18. A margin-report job reads private cost fields from DTOs but always gets null. What is the binding mistake?

```csharp
public decimal? ReadCostBasis(object record)
{
    Type type = record.GetType();
    FieldInfo? field = type.GetField("_costBasis"); // default binding
    if (field is null) return null;
    return (decimal?)field.GetValue(record);
}
```

**Concepts**
- BindingFlags.NonPublic | BindingFlags.Instance required for private fields
- default GetField searches public fields only
- silent null propagation vs explicit error
- encapsulation bypass justification

| Category | Problem | Impact |
|---|---|---|
| Correctness | Default BindingFlags does not search private fields — `_costBasis` is never found | Always returns null, every row skipped |
| Maintainability | Accessing private fields by string breaks on rename | Silent data loss after any refactor |

**Fix priority:**
1. Pass `BindingFlags.NonPublic | BindingFlags.Instance` to `GetField`.
2. Add a null guard that throws a descriptive exception rather than silently returning null.
3. Evaluate whether a public property or interface method on the DTO is a better design than private field reflection.

---

## Q19. An API discovers [EntityTable]-decorated types at startup, but they vanish after enabling PublishTrimmed. Why, and what are production-safe alternatives?

**Concepts**
- IL trimmer removing unreferenced types
- reflection invisible to static analysis
- DynamicDependency attribute for explicit rooting
- [RequiresUnreferencedCode] annotation
- source generators as trimming-safe replacement

**Answer**

The IL trimmer performs static reachability analysis from the program's entry point. Types decorated with `[EntityTable]` that are not referenced by name anywhere in the compiled call graph appear unreachable to the trimmer — it removes them along with their metadata. At runtime, `Assembly.GetExecutingAssembly().GetTypes()` returns a smaller set than expected, and the entity types are not found even though they were present before trimming. The trimmer produces warnings tagged `IL2026`/`IL2075` during publish, which developers often suppress without understanding the impact. The correct mitigations in priority order are: annotate the scanning method with `[RequiresUnreferencedCode("Scans for EntityTable attributes")]` to surface the risk as a build warning; root the entity types via `[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MyEntity))]` on the scanner; add a `TrimmerRootDescriptor.xml` listing all entity assemblies; or replace the runtime scan entirely with a source generator that emits a compile-time `IEnumerable<Type>` of all `[EntityTable]`-decorated types. The source generator approach is the only one that is both trimming-safe and Native AOT compatible.
