# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/02. Reflection & Attributes/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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

---

#### Q5. (P) An ASP.NET Core API discovers `[EntityTable]`-decorated export types by scanning `Assembly.GetExecutingAssembly().GetTypes()` at startup. After enabling `<PublishTrimmed>true</PublishTrimmed>` for a Native AOT experiment, several entity types vanish from the export manifest with no compile errors. Why does trimming break this pattern, and what production-safe alternatives exist?

---

#### Q6. (D) Your team ships a CSV export endpoint. One developer scans every request with `type.GetProperties()` and `GetCustomAttribute<ExportableAttribute>()` (same pattern as `ExportManifestBuilder` in this chapter). Another caches `PropertyInfo[]` and attribute metadata in a `ConcurrentDictionary<Type, ExportColumn[]>` built once at startup. Under 500 RPS with 40 exportable properties per row, which approach do you choose and why?

---

#### Q7. (M) A `PremiumProduct : Product` subclass is added for a loyalty tier. The ORM layer reads `[EntityTable]` from the base `Product` type to resolve table names. Export works for `Product` but `PremiumProduct` rows fail with "table not mapped." Given this attribute definition from the tutorial:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EntityTableAttribute : Attribute { /* TableName */ }
```

Why does inheritance behave this way, and what are two correct fixes at the call site or attribute definition?
