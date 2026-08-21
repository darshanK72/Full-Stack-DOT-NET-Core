# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/02. Reflection & Attributes/`

---

#### Q1. (R) A warehouse API loads pricing plugins from a separate assembly at runtime. It works on a developer machine but `LoadPlugin` always returns null in staging. Review the loader:

```csharp
public sealed class PluginLoader
{
    public object? LoadPlugin(string typeName)
    {
        Type? pluginType = Type.GetType(typeName); // e.g. "Acme.Pricing.VolumeDiscountPlugin"
        if (pluginType is null)
        {
            return null;
        }

        return Activator.CreateInstance(pluginType);
    }
}

// Startup config (appsettings):
// "Plugins:PricingType": "Acme.Pricing.VolumeDiscountPlugin"
```

What fails cross-assembly, and how do you fix type resolution for production plugin loading?

**Answer:** `Type.GetType(string)` resolves types in the **calling assembly** and mscorlib by default — not arbitrary referenced or dynamically loaded assemblies. A namespace-qualified name without an assembly qualifier returns null when the plugin lives in `Acme.Pricing.dll`, which looks like "works locally" only if everything is inlined in one project.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Type.GetType` without assembly-qualified name | Returns null for types in other assemblies — plugin silently skipped |
| Design | No assembly load step before type lookup | Plugin DLL never loaded into the AppDomain |
| Correctness | `Activator.CreateInstance` on resolved type may still fail | Parameterless ctor required; wrong ctor throws `MissingMethodException` |

**Fix (priority order):**

1. Load the plugin assembly explicitly: `Assembly.LoadFrom(path)` or `AssemblyLoadContext.LoadFromAssemblyPath`, then `assembly.GetType(typeName)`.
2. Or pass an assembly-qualified name in config: `"Acme.Pricing.VolumeDiscountPlugin, Acme.Pricing, Version=…, Culture=neutral, PublicKeyToken=…"`.
3. Fail fast when resolution returns null — log config value and searched assemblies instead of returning null quietly.
4. Validate plugin types implement a known interface (`IPricingPlugin`) before `CreateInstance`; guard null and ctor requirements.

**Production takeaway:** Cross-assembly `Type.GetType` is a classic "works in monolith, fails when split" trap — same lesson as **Program.cs** Section 6b: assembly-qualified names are required for external types.

---

#### Q2. (R) A metadata-driven audit interceptor invokes controller actions and logs failures, but operators only see `TargetInvocationException` in Splunk — never the real fault. Review the handler:

```csharp
public sealed class AuditInterceptor
{
    public object? InvokeWithAudit(object target, MethodInfo method, object?[] args)
    {
        AuditActionAttribute? audit = method.GetCustomAttribute<AuditActionAttribute>();
        _logger.LogInformation("Before {Action}", audit?.Action ?? method.Name);

        try
        {
            return method.Invoke(target, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit invoke failed for {Method}", method.Name);
            throw;
        }
    }
}
```

What is wrong with exception handling here, and what should be logged and rethrown?

**Answer:** Exceptions thrown **inside** the invoked method are wrapped in `TargetInvocationException`. Logging and rethrowing the wrapper hides the real fault (`InvalidOperationException`, `ArgumentException`, etc.) from operators and any upstream handler that keys off exception type.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Logs outer `TargetInvocationException` only | Splunk shows wrapper message/stack — root cause buried in `.InnerException` |
| Observability | `throw;` rethrows wrapper unchanged | Global exception middleware may classify wrong HTTP status |
| Maintainability | No unwrap before log/rethrow | On-call cannot triage from alert text alone |

**Fix (priority order):**

1. Catch `TargetInvocationException` specifically and unwrap: `var inner = tie.InnerException ?? tie;`
2. Log `inner` (message + stack), not the wrapper — include `method.Name` and audit action metadata.
3. Rethrow `inner` with `ExceptionDispatchInfo.Capture(inner).Throw()` if you must preserve original stack without `throw ex` corruption.
4. Guard `method` for null before `Invoke` — `GetMethod` typos cause `NullReferenceException` before any wrapper is involved.

```csharp
catch (TargetInvocationException tie)
{
    Exception actual = tie.InnerException ?? tie;
    _logger.LogError(actual, "Audit invoke failed for {Method}", method.Name);
    ExceptionDispatchInfo.Capture(actual).Throw();
}
```

**Production takeaway:** `MethodInfo.Invoke` always wraps callee faults — production code must unwrap before logging and API error mapping. See **Program.cs** Section 6e and QUICK REFERENCE — TargetInvocationException.

---

#### Q3. (R) After a rename refactor from `CalculateLineTotal` to `CalculateOrderTotal`, order totals silently become zero in production. Review the pricing service:

```csharp
public decimal GetLineTotal(Product product, int quantity)
{
    Type type = product.GetType();
    MethodInfo? method = type.GetMethod("CalculateLineTotal"); // string name — not nameof

    object? result = method.Invoke(product, new object[] { quantity });
    return result is decimal total ? total : 0m;
}
```

Identify the stacked problems (compile-time, runtime, maintainability) and prioritize fixes.

**Answer:** The method was renamed but the string literal was not updated — `GetMethod` returns null, and the code invokes without a null check, which throws at runtime (or would if the fallback `0m` masked a null invoke in a sloppier variant). Stringly-typed reflection bypasses the compiler's rename refactor.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Maintainability | Hard-coded `"CalculateLineTotal"` | Rename refactor does not update call sites — silent wrong behavior or NRE |
| Runtime | No null guard on `method` before `Invoke` | `NullReferenceException` in production when name drifts |
| API contract | `GetMethod(name)` without parameter types | Ambiguous if overloads exist — wrong overload or null |
| Design | Reflection for a known compile-time type | Unnecessary indirection vs direct `product.CalculateOrderTotal(quantity)` |

**Fix (priority order):**

1. Replace string with `nameof(Product.CalculateOrderTotal)` and pass parameter types: `GetMethod(nameof(...), new[] { typeof(int) })`.
2. Guard null and fail loudly (throw `InvalidOperationException`) instead of returning `0m` — silent zero corrupts billing.
3. Prefer direct call when type is known at compile time; reserve reflection for plugin/unknown-type scenarios.
4. Add a unit test that asserts invoke succeeds after renames — or eliminate reflection on this path entirely.

**Production takeaway:** Reflection + magic strings defeats IDE refactor — Karat stacks rename drift with missing null guards. See **Program.cs** Section 6e pitfall table and Section 6i — "Rename refactor breaks silently."

---

#### Q4. (R) A margin-report job reads private cost fields from `CostRecord`-like DTOs but always gets null and skips rows. Review the extractor:

```csharp
public decimal? ReadCostBasis(object record)
{
    Type type = record.GetType();
    FieldInfo? field = type.GetField("_costBasis"); // default binding — public only

    if (field is null)
    {
        return null;
    }

    return (decimal?)field.GetValue(record);
}
```

What binding mistake causes this, and what flags are required?

**Answer:** `GetField` without `BindingFlags` uses default binding, which returns **public** instance/static members only. `_costBasis` is a private instance field — lookup returns null, the method exits early, and rows are skipped.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Default binding omits non-public members | Private field never found — report data missing |
| Correctness | `(decimal?)field.GetValue(record)` on boxed value | Unboxing via nullable cast can throw if type mismatches |
| Design | Reading private fields from outside the type | Breaks encapsulation — prefer a public/internal accessor for reporting |

**Fix (priority order):**

1. Pass explicit flags matching **Program.cs** Section 6d: `BindingFlags.Instance | BindingFlags.NonPublic` (add `Public` if you want either).
2. Guard null and log type name + field name when lookup fails — distinguish "wrong type" from "wrong flags."
3. Prefer exposing `CostBasis` via an internal property or `IOffsetCostReadable` interface for the report job instead of reaching into private fields.
4. Cache `FieldInfo` per `Type` in a static readonly dictionary if this runs in a batch loop.

```csharp
const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
FieldInfo? field = type.GetField("_costBasis", flags);
```

**Production takeaway:** Default reflection binding is the first thing to check when "member not found" — same specimen as `CostRecord` in **Program.cs** Section 3 and 6d.

---

#### Q5. (P) An ASP.NET Core API discovers `[EntityTable]`-decorated export types by scanning `Assembly.GetExecutingAssembly().GetTypes()` at startup. After enabling `<PublishTrimmed>true</PublishTrimmed>` for a Native AOT experiment, several entity types vanish from the export manifest with no compile errors. Why does trimming break this pattern, and what production-safe alternatives exist?

**Answer:** The trimmer removes types and members it cannot prove are used at compile time. Startup reflection that scans assemblies and reads custom attributes is invisible to static analysis — entity types with no direct references are linked out, so `GetTypes()` returns a smaller set and attribute-driven discovery silently drops models.

- **Why no compile error:** Trimming is a link-time optimization; reflection targets are not required call sites the compiler tracks.
- **Mitigations:** Annotate roots with `[DynamicallyAccessedMembers]` / `DynamicallyAccessedMemberTypes` on APIs that accept `Type`; use a trimmer descriptor file (`TrimmerRootAssembly` / `TrimmerRootDescriptor`) to preserve entity assemblies; register known export types explicitly in DI instead of full-assembly scan.
- **Long-term:** Replace scan-all-reflection with **source generators** that emit export manifests or EF-style mappings at compile time — same metadata (`[EntityTable]`, `[Exportable]`), no runtime graph walk.
- **Handle `ReflectionTypeLoadException`:** When dependencies are trimmed or missing, `GetTypes()` can throw — catch and log `LoaderExceptions` (see **Program.cs** Section 6b).

**Production takeaway:** Attribute-driven discovery copied from EF/ASP.NET patterns fails under Native AOT and aggressive publish trimming unless you root types or generate code — preview in **Program.cs** Section 6i AOT note.

---

#### Q6. (D) Your team ships a CSV export endpoint. One developer scans every request with `type.GetProperties()` and `GetCustomAttribute<ExportableAttribute>()` (same pattern as `ExportManifestBuilder` in this chapter). Another caches `PropertyInfo[]` and attribute metadata in a `ConcurrentDictionary<Type, ExportColumn[]>` built once at startup. Under 500 RPS with 40 exportable properties per row, which approach do you choose and why?

**Answer:** Cache metadata at startup (or first use per `Type`) and only call `GetValue` per instance per request — reflection on `Type` and `MemberInfo` is orders of magnitude more expensive than reading pre-resolved columns from a cached `ExportColumn[]`.

- **Per-request scan:** 500 × 40 property walks × attribute lookups allocates and hits internal reflection caches repeatedly — CPU spikes, GC pressure, latency tail grows under load.
- **Cached manifest:** Startup (or lazy) build mirrors **ExportManifestBuilder** logic once per exportable type; hot path is `foreach (col in manifest) col.Getter(instance)` — optionally compile delegates with `CreateGetter` for value types to avoid boxing.
- **Thread safety:** `ConcurrentDictionary<Type, ExportColumn[]>` is safe for lazy initialization; manifest is immutable after build — no lock on read path.
- **Invalidation:** If types are loaded dynamically (plugins), register manifest on plugin load; static domain models rarely need refresh.
- **Trade-off:** Cached approach uses slightly more memory for delegate/metadata tables — acceptable vs per-request CPU at 500 RPS.

**Production takeaway:** Metadata-driven export is a framework pattern — scan once, read many — not scan per row. See **Program.cs** Section 5 and Section 6i — "Uncached reflection in hot paths."

---

#### Q7. (M) A `PremiumProduct : Product` subclass is added for a loyalty tier. The ORM layer reads `[EntityTable]` from the base `Product` type to resolve table names. Export works for `Product` but `PremiumProduct` rows fail with "table not mapped." Given this attribute definition from the tutorial:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EntityTableAttribute : Attribute { /* TableName */ }
```

Why does inheritance behave this way, and what are two correct fixes at the call site or attribute definition?

**Answer:** `Inherited = false` on `EntityTableAttribute` means the attribute is stored only on `Product` — it is **not** visible when you call `typeof(PremiumProduct).GetCustomAttribute<EntityTableAttribute>()`. The ORM resolves the runtime type of each instance, sees no attribute on the subclass, and reports "table not mapped."

- **Why designed this way:** Table-per-type hierarchies often need different tables per concrete class — inheriting `[EntityTable("Products")]` onto every subclass would be wrong when `PremiumProduct` maps to `PremiumProducts`.
- **Fix 1 (call site):** Walk the inheritance chain: check `type.GetCustomAttribute<EntityTableAttribute>(inherit: true)` is insufficient when `Inherited = false`; instead loop `type = type.BaseType` until you find the attribute, or map `PremiumProduct` explicitly in a type registry.
- **Fix 2 (attribute):** If all subclasses share one table, set `Inherited = true` on `EntityTableAttribute` and apply only on the base — then `GetCustomAttribute` on derived types returns the base metadata (verify this matches your schema).
- **Fix 3 (explicit):** Add `[EntityTable("PremiumProducts")]` on `PremiumProduct` — correct when the subclass has its own table regardless of inheritance flags.

**Production takeaway:** Attribute inheritance is opt-in via `AttributeUsage.Inherited` — framework code must not assume subclass metadata mirrors the base. Contrast with `[DisplayLabel]` in **Program.cs** Section 1 (`Inherited = true`) vs `[EntityTable]` (`Inherited = false`).
