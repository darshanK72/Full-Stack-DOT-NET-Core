# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/04. Dictionary`

---

#### Q1. (R) A hot-path SKU lookup uses `ContainsKey` followed by the indexer. Review this warehouse catalog access. What is inefficient, and how would you improve it?

**Answer:** The code performs two hash lookups — one in `ContainsKey` and one in the indexer — when a single `TryGetValue` call can retrieve the value in one pass. Under hot paths or large catalogs, the extra lookup adds avoidable cost and is harder to read than the idiomatic pattern.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Double hash lookup per hit | Unnecessary CPU on high-frequency SKU resolution |
| Idiomatic C# | `ContainsKey` + indexer is a legacy pattern | Easy to miss in code review; signals unfamiliarity with BCL APIs |
| Concurrency | Two separate reads on a shared dictionary (minor) | Theoretically inconsistent if another thread mutates between calls |

**Fix (priority order):**

1. Replace the pair with `TryGetValue` and branch on its `bool` result.
2. Return the `out` variable directly when found; avoid a second access.
3. Prefer `CollectionsMarshal.GetValueRefOrNullRef` only in measured hot paths — `TryGetValue` is the default fix.

```csharp
public Product? FindProduct(Dictionary<string, Product> catalog, string sku)
{
    return catalog.TryGetValue(sku, out Product? product) ? product : null;
}
```

**Production takeaway:** Karat flags this as a micro-optimization with readability upside — it signals you know standard library APIs, not just that dictionaries exist. See **Program.cs** Section 5 — `TryGetValue` preferred over `ContainsKey` + indexer.

---

#### Q2. (R) A team uses a custom class as the dictionary key and mutates it after insert. Lookups start failing intermittently in production. Review this catalog code:

**Answer:** `Dictionary` stores entries by the key's hash code at insert time. Mutating `key.Code` after insert leaves the entry in the wrong bucket — lookups with a new `SkuKey { Code = "WH-1001" }` hash to a different slot, so the product appears missing even though it is still in the table under a stale hash.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable key changed after `catalog[key] = …` | `TryGetValue` / indexer miss; "orphaned" entries |
| Hash contract | `GetHashCode`/`Equals` must be stable while key is in the map | Violated when `Code` property mutates |
| Design | Reference-type key with public setter | Silent data loss in catalog lookups |

**Fix (priority order):**

1. Make key types **immutable** after construction — `init` or `readonly` properties, or use `record`/`readonly record struct` for value semantics.
2. Never mutate a key object that is already in the dictionary; remove, create a new key, and re-insert if the identifier changes.
3. For string SKUs, prefer `Dictionary<string, Product>` — `string` is immutable and already implements the hash contract correctly.
4. If you must wrap identifiers, use `readonly record struct SkuKey(string Code)` or a sealed class with no setters.

```csharp
public readonly record struct SkuKey(string Code);

var catalog = new Dictionary<SkuKey, Product>();
catalog[new SkuKey("WH-1001")] = product;
// To change SKU: remove old entry, insert with new SkuKey — do not mutate in place
```

**Production takeaway:** Hash-table collections assume keys do not change while inserted — Karat uses this to test the hash contract beyond "override GetHashCode." See **Program.cs** Section 1 — custom keys must override both methods and stay immutable after insert.

---

#### Q3. (P) An ASP.NET Core API caches product details in a shared `Dictionary<string, Product>` field on a singleton service. Under load tests, responses are wrong and the process occasionally throws `InvalidOperationException`. Review the cache:

**Answer:** `Dictionary<TKey, TValue>` is not thread-safe. Concurrent reads and writes from multiple HTTP requests corrupt internal buckets, throw during enumeration, and allow two threads to both miss the cache and write different `Product` instances for the same SKU — undefined behavior under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Unsynchronized `_cache` mutations | `InvalidOperationException`, torn internal state |
| Correctness | Check-then-add without locking | Duplicate DB loads; possible inconsistent cached values |
| Lifetime | Singleton holds one shared dictionary for all requests | Every request shares the same unsynchronized structure |
| Performance | `ContainsKey` + indexer (two lookups) | Extra cost on every cache access |

**Fix (priority order):**

1. Replace with `ConcurrentDictionary<string, Product>` and use `GetOrAdd` or `TryGetValue` for reads.
2. If you must keep `Dictionary`, guard all access with a single lock (`lock (_cache) { … }`) — simpler but lower throughput than `ConcurrentDictionary`.
3. Register the cache as **scoped** only when it is per-request scratch data — not for a cross-request product catalog; singleton + concurrent collection is the usual pattern for shared read-mostly caches.
4. Consider `IMemoryCache` with size limits and expiration instead of a raw unbounded map (see Q5).

```csharp
private readonly ConcurrentDictionary<string, Product> _cache = new();

public Product GetBySku(string sku) =>
    _cache.GetOrAdd(sku, s => _repo.GetBySku(s));
```

**Production takeaway:** Passing local load tests with one thread hides dictionary thread-safety gaps — Karat expects you to name `ConcurrentDictionary` or explicit locking for shared mutable maps. See foundation **Dictionary** gotcha — not thread-safe for concurrent read/write.

---

#### Q4. (R) A REST endpoint maps query parameters directly into dictionary lookups without null checks. Review the handler:

**Answer:** When `sku` is omitted, it is `null`. `Dictionary<string, T>.ContainsKey(null)` throws `ArgumentNullException` before the `NotFound()` branch runs — the API returns 500 instead of 400/404. The same rule applies to `Add`, the indexer, and `TryGetValue` with a null reference-type key.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Null key passed to `ContainsKey` | `ArgumentNullException` — 500 to client |
| API contract | Missing query param not validated | Wrong status code; noisy error logs |
| Input hygiene | Nullable `string? sku` used as dictionary key without guard | Any null path hits the same exception |

**Fix (priority order):**

1. Validate input first: `if (string.IsNullOrWhiteSpace(sku)) return BadRequest("sku is required");`
2. Use `TryGetValue` for the lookup after validation — one lookup, no exception on missing key.
3. Do not inject `Dictionary<string, Product>` directly into controllers — use a scoped service that owns catalog access and validation.
4. Return `NotFound()` only after a validated, non-null key misses the catalog.

```csharp
[HttpGet("product")]
public IActionResult GetProduct([FromQuery] string? sku, [FromServices] IProductCatalog catalog)
{
    if (string.IsNullOrWhiteSpace(sku))
        return BadRequest("sku is required");

    return catalog.TryGetProduct(sku, out Product? product)
        ? Ok(product)
        : NotFound();
}
```

**Production takeaway:** Null keys are rejected at the API boundary of `Dictionary<string, …>` — Karat tests whether you validate before touching the collection. See **Program.cs** Section 7c — null key on `Add` throws `ArgumentNullException`.

---

#### Q5. (D) A microservice adds a static in-memory cache so repeated HTTP fetches are fast. After two weeks in production, pods hit OOM kills even though traffic is steady. Review the cache:

**Answer:** A plain `Dictionary` with no eviction policy grows without bound — every distinct URL adds a full byte array that is never removed. Static lifetime means the cache survives for the process lifetime and is shared across all requests, so memory only increases as URL diversity grows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory | Unbounded key space (`url` strings) + large values (`byte[]`) | OOM kills; GC pressure |
| Lifecycle | `static` cache never cleared | Memory not reclaimed until process restart |
| Operations | No TTL, size cap, or LRU | Cannot reason about worst-case footprint |
| Scale-out | Per-pod static cache | Duplicate memory across replicas; no shared invalidation |

**Fix (priority order):**

1. Replace with `IMemoryCache` (or `MemoryCache`) configured with `SizeLimit`, `CompactionPercentage`, and per-entry `Size` + `AbsoluteExpiration` / `SlidingExpiration`.
2. For distributed deployments, use `IDistributedCache` (Redis) with explicit TTL instead of unbounded in-process storage.
3. If a raw dictionary is unavoidable, implement LRU with a max entry count and max total bytes — evict oldest when limits are hit.
4. Remove `static` — inject a singleton `IMemoryCache` via DI so tests can substitute and options can be configured per environment.

```csharp
// Program.cs — services
builder.Services.AddMemoryCache(o =>
{
    o.SizeLimit = 10_000; // abstract "size units", set per entry
});

// Usage
_cache.GetOrCreate(url, entry =>
{
    entry.Size = 1;
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
    return Download(url);
});
```

**Production takeaway:** `Dictionary` is a map, not a cache policy — Karat distinguishes "fast lookup" from "safe caching." Unbounded in-memory maps are a common postmortem root cause. See **Program.cs** WHY IT MATTERS — catalogs and caches appear everywhere; size and eviction are production requirements.

---

#### Q6. (P) A developer avoids `ConcurrentDictionary` and hand-rolls lazy initialization with `TryGetValue`. Under load, the expensive factory runs twice for the same key. Review:

**Answer:** Between `TryGetValue` returning false and `_catalog[sku] = product`, another thread can pass the same check and also call `_repo.LoadProduct(sku)` — classic check-then-act race. Both threads may insert; the last write wins, but you paid for duplicate DB work and may briefly expose inconsistent state.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Non-atomic check-then-add on `Dictionary` | Duplicate expensive loads under parallel requests |
| Correctness | Two writers without synchronization | Undefined behavior on `Dictionary` itself (see Q3) |
| Cost | Idempotent DB read assumed | Thundering herd on cold keys at startup or cache flush |

**Fix (priority order):**

1. Use `ConcurrentDictionary.GetOrAdd` so factory execution for a given key is coordinated by the collection.
2. If the factory is very expensive, wrap with `Lazy<Product>` per key or use `GetOrAdd` with a factory that returns `Lazy<Product>` and then `.Value` once — avoids duplicate work when factory cost dominates.
3. For single-threaded or scoped usage, plain `TryGetValue` + assign is fine — the bug is specifically shared mutable state under concurrency.
4. Add metrics on cache misses and factory duration to detect duplicate load spikes in production.

```csharp
private readonly ConcurrentDictionary<string, Product> _catalog = new();

public Product GetOrLoad(string sku) =>
    _catalog.GetOrAdd(sku, s => _repo.LoadProduct(s));

// Optional: defer heavy work until first read
private readonly ConcurrentDictionary<string, Lazy<Product>> _catalog = new();

public Product GetOrLoad(string sku) =>
    _catalog.GetOrAdd(sku, s => new Lazy<Product>(() => _repo.LoadProduct(s))).Value;
```

**Production takeaway:** `GetOrAdd` is the production pattern for "compute once per key" in concurrent caches — Karat tests whether you recognize check-then-add as a race, not whether you memorized the method name. Pair with Q3: thread-safe type **and** atomic get-or-create semantics.

---
