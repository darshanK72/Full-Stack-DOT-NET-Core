# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/04. Dictionary`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A hot-path SKU lookup uses `ContainsKey` followed by the indexer. Review this warehouse catalog access. What is inefficient, and how would you improve it?

```csharp
public Product? FindProduct(Dictionary<string, Product> catalog, string sku)
{
    if (catalog.ContainsKey(sku))
    {
        return catalog[sku];
    }
    return null;
}
```

---

#### Q2. (R) A team uses a custom class as the dictionary key and mutates it after insert. Lookups start failing intermittently in production. Review this catalog code:

```csharp
public sealed class SkuKey
{
    public string Code { get; set; } = string.Empty;

    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    public override bool Equals(object? obj) =>
        obj is SkuKey other && Code == other.Code;
}

var catalog = new Dictionary<SkuKey, Product>();
var key = new SkuKey { Code = "WH-1001" };
catalog[key] = new Product { Sku = "WH-1001", Name = "Steel bracket", UnitPrice = 12.50m };

// Later, a pricing job "normalizes" the key object in place:
key.Code = "WH-1001-NORM";

// Another request:
var lookupKey = new SkuKey { Code = "WH-1001" };
catalog.TryGetValue(lookupKey, out Product? found); // found is null — product "vanished"
```

What broke, and how should keys be designed for `Dictionary<TKey, TValue>`?

---

#### Q3. (P) An ASP.NET Core API caches product details in a shared `Dictionary<string, Product>` field on a singleton service. Under load tests, responses are wrong and the process occasionally throws `InvalidOperationException`. Review the cache:

```csharp
public sealed class ProductCatalogService
{
    private readonly Dictionary<string, Product> _cache = new();
    private readonly IProductRepository _repo;

    public Product GetBySku(string sku)
    {
        if (!_cache.ContainsKey(sku))
        {
            Product loaded = _repo.GetBySku(sku); // DB call
            _cache[sku] = loaded;
        }
        return _cache[sku];
    }
}
```

What fails in production under concurrent requests, and what type/pattern replaces this?

---

#### Q4. (R) A REST endpoint maps query parameters directly into dictionary lookups without null checks. Review the handler:

```csharp
[HttpGet("product")]
public IActionResult GetProduct([FromQuery] string? sku, [FromServices] Dictionary<string, Product> catalog)
{
    if (catalog.ContainsKey(sku))
    {
        return Ok(catalog[sku]);
    }
    return NotFound();
}
```

The client calls `/product` with no `sku` parameter. What exception is thrown and where, and how do you harden this lookup?

---

#### Q5. (D) A microservice adds a static in-memory cache so repeated HTTP fetches are fast. After two weeks in production, pods hit OOM kills even though traffic is steady. Review the cache:

```csharp
public static class RemoteAssetCache
{
    private static readonly Dictionary<string, byte[]> _cache = new();

    public static byte[] GetAsset(string url)
    {
        if (!_cache.TryGetValue(url, out byte[]? bytes))
        {
            bytes = Download(url); // can be megabytes per entry
            _cache[url] = bytes;
        }
        return bytes;
    }

    private static byte[] Download(string url) { /* HttpClient GET */ return Array.Empty<byte>(); }
}
```

What design problem does this introduce at scale, and what would you use instead of an unbounded `Dictionary`?

---

#### Q6. (P) A developer avoids `ConcurrentDictionary` and hand-rolls lazy initialization with `TryGetValue`. Under load, the expensive factory runs twice for the same key. Review:

```csharp
private readonly Dictionary<string, Product> _catalog = new();

public Product GetOrLoad(string sku)
{
    if (!_catalog.TryGetValue(sku, out Product? product))
    {
        product = _repo.LoadProduct(sku); // slow DB + mapping
        _catalog[sku] = product;
    }
    return product;
}
```

What race exists when multiple threads call `GetOrLoad` for the same missing SKU, and how does `ConcurrentDictionary.GetOrAdd` (or alternatives) fix it?
