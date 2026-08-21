# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/01. Generics`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A teammate adds a generic repository helper for warehouse stock rows. `dotnet build` fails. Review the constraint stack — what is wrong, and how do you fix it?

```csharp
public static class StockRepository
{
    public static T LoadOrCreate<T>(string sku) where T : struct, StockEntry, new()
    {
        if (_cache.TryGetValue(sku, out T existing))
        {
            return existing;
        }

        T created = new T { Sku = sku };
        _cache[sku] = created;
        return created;
    }

    private static readonly Dictionary<string, StockEntry> _cache = new();
}
```

---

#### Q2. (R) A developer "fixes" a method that accepts any payload list by widening to `List<object>`. Review the assignment and call site:

```csharp
public static void AuditSkus(List<object> allSkus)
{
    foreach (object sku in allSkus)
    {
        Console.WriteLine(sku);
    }
}

List<string> warehouseSkus = new() { "WH-4412", "WH-9901" };
AuditSkus(warehouseSkus); // CS1503 — cannot convert List<string> to List<object>

// Developer tries IEnumerable instead:
IEnumerable<object> widened = warehouseSkus;
foreach (object item in widened)
{
    var mutable = (List<object>)widened; // attempted cast at runtime
    mutable.Add(42);
}
```

What fails at compile time vs runtime, and what is the safe pattern for read-only aggregation?

---

#### Q3. (R) An API endpoint helper should return the larger of two comparable stock metrics without boxing value types. Review the call chain:

```csharp
public static T MaxOf<T>(T left, T right) where T : IComparable<T>
{
    return left.CompareTo(right) >= 0 ? left : right;
}

decimal priceA = 19.99m;
int unitsB = 120;
var winner = MaxOf(priceA, unitsB); // CS0411 — type arguments cannot be inferred

// After "fix" — explicit type args:
var forced = MaxOf<decimal>(priceA, unitsB); // CS1503 — int not convertible to decimal
```

What broke inference, why does the explicit fix still fail, and how would you design this helper for production?

---

#### Q4. (M) A hot inventory path stores millions of pallet counts per hour. One service uses `List<object>` "for flexibility"; another uses `List<int>`. Review the read loop:

```csharp
List<object> legacyCounts = new();
for (int i = 0; i < 1_000_000; i++)
{
    legacyCounts.Add(i); // boxed int on every Add
}

int legacySum = 0;
foreach (object boxed in legacyCounts)
{
    legacySum += (int)boxed; // unbox per iteration
}

List<int> genericCounts = new(capacity: 1_000_000);
for (int i = 0; i < 1_000_000; i++)
{
    genericCounts.Add(i); // no boxing
}

int genericSum = genericCounts.Sum();
```

Under JIT/AOT, what does the runtime do differently for `List<int>` vs `List<object>`, and when would you still accept the legacy shape?

---

#### Q5. (R) A factory method should default-construct inventory DTOs for an import pipeline. Review:

```csharp
public sealed record ImportedLine(string Sku, int Units);

public static class ImportFactory
{
    public static T CreateRow<T>() where T : new()
    {
        return new T();
    }
}

// Startup:
var row = ImportFactory.CreateRow<ImportedLine>();
row.Sku = "WH-4412";

// Alternate path — value-type wrapper:
public readonly struct PalletTag
{
    public PalletTag(int zoneId) => ZoneId = zoneId;
    public int ZoneId { get; }
}

var tag = ImportFactory.CreateRow<PalletTag>();
```

What compiles, what fails, and how do you constrain factories correctly for records vs structs?

---

#### Q6. (R) A library author exposes typed domain exceptions via generics "so callers can catch exactly what they need." Review:

```csharp
public static class StockGuard
{
    public static void EnsurePositive<TException>(int units)
        where TException : Exception, new()
    {
        if (units <= 0)
        {
            throw new TException();
        }
    }

    public static void Reserve(string sku, int units)
    {
        EnsurePositive<ArgumentOutOfRangeException>(units);

        if (!IsKnownSku(sku))
        {
            EnsurePositive<InvalidOperationException>(0); // reuses generic throw
        }
    }
}

// Consumer:
try
{
    StockGuard.Reserve("WH-0000", -5);
}
catch (ArgumentOutOfRangeException ex)
{
    _logger.LogWarning(ex, "Bad quantity for {Sku}", sku);
}
catch (InvalidOperationException ex)
{
    _logger.LogError(ex, "Unknown SKU workflow failure");
}
```

What is wrong with generic exception throwing, what breaks observability and API contracts, and what pattern replaces it?

---
