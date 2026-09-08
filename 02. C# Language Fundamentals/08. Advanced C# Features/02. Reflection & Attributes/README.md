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

## Gotchas — Reflection & Attributes (Interview Traps)

---

#### Gotcha 1. Reflection is slow — cache `MethodInfo`/`PropertyInfo` objects; avoid per-call `GetMethod`

**Concepts**
- `GetMethod` / `GetProperty` perform metadata lookup on every call
- caching in `static readonly` fields or `ConcurrentDictionary<Type, PropertyInfo[]>`
- `MethodInfo.Invoke` overhead vs compiled delegate
- source generators as the zero-overhead compile-time alternative

**Answer**

Calling `type.GetMethod("Calculate")` searches the type's metadata table on every invocation. In a serializer or ORM that processes thousands of objects per request, this per-call lookup produces measurable latency and significant allocations. The fix is to resolve `MethodInfo` and `PropertyInfo` objects once — during startup or on first use per type — and store them in a `static readonly` dictionary keyed by type. For even better performance, compile the `MethodInfo` to a delegate using `CreateDelegate` or build an expression tree and call `.Compile()`, reducing repeated invocation to a virtual dispatch. Source generators eliminate the runtime reflection cost entirely by generating strongly typed accessor code at compile time.

---

#### Gotcha 2. `Type.GetMethod` with overloads — must specify binding flags and parameter types; ambiguous match exception

**Concepts**
- `AmbiguousMatchException` when multiple overloads exist
- `Type.GetMethod(string, Type[])` overload for precise resolution
- `BindingFlags.Public | BindingFlags.Instance` defaults
- `BindingFlags.NonPublic` required for private methods

**Answer**

`Type.GetMethod("Process")` throws `AmbiguousMatchException` when the type has more than one public instance method named `Process` with different parameter lists. The fix is to use the overload that accepts a `Type[]` parameter array: `type.GetMethod("Process", new[] { typeof(string), typeof(int) })`. Additionally, `GetMethod` by default searches only public instance methods; to find a static or non-public method you must pass `BindingFlags` explicitly — `BindingFlags.NonPublic | BindingFlags.Instance` for a private instance method. Omitting the flags when looking for a private method returns `null` silently, which is then typically used as `null.Invoke(...)` causing a `NullReferenceException` rather than a helpful message about why the method was not found.

---

#### Gotcha 3. `Attribute.GetCustomAttribute` vs `MemberInfo.GetCustomAttributes` — single vs multiple

**Concepts**
- `Attribute.GetCustomAttribute` returns one `Attribute` or throws if multiple exist
- `MemberInfo.GetCustomAttributes(typeof(T), inherit)` returns an array
- `[AttributeUsage(AllowMultiple = true)]` allows multiple instances on one target
- `IsDefined` for fast existence check without allocating the attribute instance

**Answer**

`Attribute.GetCustomAttribute(memberInfo, typeof(ValidateAttribute))` retrieves the single attribute of that type applied to the member. If `AllowMultiple = true` and two instances of the attribute are applied, this method throws `AmbiguousMatchException`. When an attribute allows multiple applications, always use `Attribute.GetCustomAttributes` (plural) or `memberInfo.GetCustomAttributes(typeof(ValidateAttribute), inherit: true)`, which returns an array. Using the singular form on a multi-occurrence attribute is a runtime exception that only surfaces when someone actually applies the attribute twice — a subtle late-discovery bug. If you only need to know whether the attribute exists at all, `Attribute.IsDefined` performs the check without instantiating the attribute and is the most efficient option.

---

#### Gotcha 4. `AttributeUsage(AttributeTargets.Method, AllowMultiple = false)` — applying twice throws at compile time

**Concepts**
- `AllowMultiple = false` (default) prevents duplicate application
- compile-time error on duplicate attribute
- `AllowMultiple = true` required for multi-application scenarios
- `AttributeTargets` controls which element types can be annotated

**Answer**

When `[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]` is set on an attribute (or when `AllowMultiple` is omitted, since `false` is the default), applying the same attribute twice to the same method produces a compile-time error: `Duplicate 'MyAttribute' attribute`. This is the intended safety mechanism — the compiler prevents accidental duplication before the code reaches runtime. To support legitimate multiple applications (e.g., a `[RequiredRole("Admin"), RequiredRole("Manager")]` security attribute), the attribute must explicitly declare `AllowMultiple = true`. A common design mistake is forgetting to set `AllowMultiple = true` on attributes that are logically intended to stack, discovering the restriction only when a consumer attempts to apply them multiple times.

---

#### Gotcha 5. Private member access via reflection — requires `BindingFlags.NonPublic | BindingFlags.Instance`

**Concepts**
- `GetField` / `GetMethod` without flags returns only public members
- `BindingFlags.NonPublic | BindingFlags.Instance` for private/protected members
- `BindingFlags.Static` required for private static members
- accessing privates via reflection is a test design smell

**Answer**

Reflection methods like `GetField`, `GetMethod`, and `GetProperty` default to returning only public members. A call to `type.GetField("_cache")` returns `null` for a private backing field — not an exception. Code that then calls `field.SetValue(instance, value)` on the `null` reference throws `NullReferenceException`, making the null field lookup the real bug rather than a missing access. The correct flags are `BindingFlags.NonPublic | BindingFlags.Instance` for private instance members and `BindingFlags.NonPublic | BindingFlags.Static` for private static members. In tests, accessing private members via reflection is generally a design smell — test through the public API instead. Legitimate uses include framework-level code (serializers, ORMs) that must operate on the full object graph regardless of access modifiers.

---

#### Gotcha 6. Invoking a method via `MethodInfo.Invoke` boxes value type arguments

**Concepts**
- `MethodInfo.Invoke(obj, object[] args)` parameter array accepts `object`
- value types are boxed into the `object[]` array before the call
- boxing/unboxing allocation on every invocation
- compiled delegate via `CreateDelegate` avoids boxing

**Answer**

`MethodInfo.Invoke` accepts arguments as `object[]`, which requires boxing every value type — `int`, `bool`, `decimal`, `struct` — into heap-allocated objects before the call. On a hot path that invokes methods on thousands of structs per second, this produces significant GC pressure. The return value is also boxed if the method returns a value type. To eliminate boxing, compile the `MethodInfo` into a typed delegate: `(Func<int, int>)methodInfo.CreateDelegate(typeof(Func<int, int>), target)` bypasses the `object[]` indirection entirely and executes at near-native speed. For generic scenarios, expression tree compilation (`Expression.Lambda(...).Compile()`) achieves the same result with a typed `Func<T, TResult>` delegate.

---

#### Gotcha 7. `typeof(T)` vs `obj.GetType()` — compile-time type vs runtime type

**Concepts**
- `typeof(T)` resolves at compile time to the declared type
- `obj.GetType()` resolves at runtime to the actual concrete type
- polymorphic dispatch difference: base vs derived type metadata
- sealed classes / value types: `typeof(T) == obj.GetType()` always true

**Answer**

`typeof(Animal)` gives the `Type` object for `Animal` as known at compile time — if `T` is `Animal` in a generic method, `typeof(T)` is `Animal` even when the actual object passed is a `Dog`. `obj.GetType()` returns the runtime type: for a `Dog` instance, `obj.GetType()` is `typeof(Dog)`. When a serializer uses `typeof(T)` to discover properties, it finds only those declared on `Animal`; when it uses `obj.GetType()`, it finds all properties including those added in `Dog`. Using the wrong one causes silent data loss — derived-type properties are ignored. The correct choice depends on intent: serializing the full concrete object requires `obj.GetType()`; generic constraints that should operate on the declared contract use `typeof(T)`.

---

#### Gotcha 8. Attributes are instantiated at reflection access time, not at compile time

**Concepts**
- attribute constructor runs when `GetCustomAttribute` is called, not at application time
- mutable attribute state: changes to fields after `GetCustomAttribute` are per-instance
- attribute with side effects in constructor: executed on every reflection call
- caching attribute instances avoids repeated construction

**Answer**

An attribute's constructor does not run when the `[MyAttribute(...)]` syntax is compiled into the assembly. It runs each time `GetCustomAttribute` or `GetCustomAttributes` is called — every call creates a new attribute instance. An attribute that opens a file, reads configuration, or has expensive initialization in its constructor will pay that cost on every reflection call. If the attribute maintains mutable state (a writable property set by the consumer), each `GetCustomAttribute` call returns a fresh instance so mutations on one instance do not affect another. To avoid repeated construction, cache the attribute instance in a static `ConcurrentDictionary<MemberInfo, MyAttribute>` keyed by the member.

---

#### Gotcha 9. `dynamic` uses reflection internally — boxing/unboxing + DLR overhead

**Concepts**
- `dynamic` invokes the DLR (Dynamic Language Runtime) on each member access
- call sites are cached after first dispatch, but first-call overhead is significant
- boxing of value types through `object` binder
- `dynamic` in a tight loop is measurably slower than a typed call

**Answer**

When you write `dynamic obj = GetObject(); obj.Process()`, the C# compiler emits a DLR call site that performs late-bound member resolution on the first invocation. The DLR caches the resolved method and binding rule, so repeated calls on the same runtime type are faster than the first — but even cached DLR dispatch is significantly slower than a direct virtual call or interface call. Value types accessed through `dynamic` are boxed and unboxed on every access. In hot paths — inner loops, high-throughput message processing — `dynamic` can be 10-100x slower than a typed dispatch. Prefer interfaces or `MethodInfo`-compiled delegates when performance matters, and reserve `dynamic` for genuinely schema-free scenarios such as interop with COM, `ExpandoObject`, or scripting engines.

---

#### Gotcha 10. Source generators as the compile-time alternative to runtime reflection

**Concepts**
- source generators run during compilation, emit C# code, no runtime cost
- `IIncrementalGenerator` API in .NET 6+
- AOT-compatible: no `Assembly.GetTypes()` or `MethodInfo.Invoke` at runtime
- `System.Text.Json`'s `[JsonSerializable]` and `Regex`'s `[GeneratedRegex]` as built-in examples

**Answer**

Source generators execute as part of the compilation pipeline and emit additional C# source files that are compiled alongside user code. The generated code contains direct property accessors, switch dispatches, or lookup tables — no runtime reflection required. This makes source-generated code fully compatible with Native AOT, which cannot JIT-compile reflection-based code at deployment time. The `[JsonSerializable(typeof(MyDto))]` attribute on a partial `JsonSerializerContext` class causes the `System.Text.Json` source generator to emit typed serializer code that is faster than the reflection-based path. Similarly, `[GeneratedRegex(@"\d+")]` generates a compiled state machine at build time. When writing custom infrastructure code (mappers, validators, serializers), implementing a source generator is the modern alternative that achieves zero runtime overhead while supporting AOT deployment.

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
