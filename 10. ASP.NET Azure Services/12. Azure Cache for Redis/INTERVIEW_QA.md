# Azure Cache for Redis — Interview Q&A
> 27 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Azure Cache for Redis, and why would an ASP.NET Core application use it instead of keeping data in the application process?](#q1-what-is-azure-cache-for-redis-and-why-would-an-aspnet-core-application-use-it-instead-of-keeping-data-in-the-application-process)
2. [Q2. How does Azure Cache for Redis relate to open-source Redis, and what does Microsoft manage for you in the managed service?](#q2-how-does-azure-cache-for-redis-relate-to-open-source-redis-and-what-does-microsoft-manage-for-you-in-the-managed-service)
3. [Q3. What problems does a distributed cache solve that an in-process cache (`IMemoryCache`) cannot?](#q3-what-problems-does-a-distributed-cache-solve-that-an-in-process-cache-imemorycache-cannot)
4. [Q4. Compare `IMemoryCache` and `IDistributedCache` — when would you use each, and can you use both together?](#q4-compare-imemorycache-and-idistributedcache-when-would-you-use-each-and-can-you-use-both-together)
5. [Q5. What are the most common caching use cases for Azure Cache for Redis in ASP.NET Core (API responses, reference data, rate limiting, and others)?](#q5-what-are-the-most-common-caching-use-cases-for-azure-cache-for-redis-in-aspnet-core-api-responses-reference-data-rate-limiting-and-others)
6. [Q6. What is the cache-aside (lazy loading) pattern, and walk through the read and write flow?](#q6-what-is-the-cache-aside-lazy-loading-pattern-and-walk-through-the-read-and-write-flow)
7. [Q7. What is write-through caching, and when is it preferable to cache-aside?](#q7-what-is-write-through-caching-and-when-is-it-preferable-to-cache-aside)
8. [Q8. What is write-behind (write-back) caching, and what consistency risks does it introduce?](#q8-what-is-write-behind-write-back-caching-and-what-consistency-risks-does-it-introduce)
9. [Q9. What is cache stampede (thundering herd), and how do you prevent it in a .NET application?](#q9-what-is-cache-stampede-thundering-herd-and-how-do-you-prevent-it-in-a-net-application)
10. [Q10. When should you avoid caching data, and what types of data are poor cache candidates?](#q10-when-should-you-avoid-caching-data-and-what-types-of-data-are-poor-cache-candidates)
11. [Q11. How do you handle cache invalidation when the authoritative source (database or API) changes?](#q11-how-do-you-handle-cache-invalidation-when-the-authoritative-source-database-or-api-changes)
12. [Q12. How do you register Azure Cache for Redis as `IDistributedCache` in ASP.NET Core, and where does the connection string come from?](#q12-how-do-you-register-azure-cache-for-redis-as-idistributedcache-in-aspnet-core-and-where-does-the-connection-string-come-from)
13. [Q13. What is the difference between `AddStackExchangeRedisCache` and using `ConnectionMultiplexer` from StackExchange.Redis directly?](#q13-what-is-the-difference-between-addstackexchangerediscache-and-using-connectionmultiplexer-from-stackexchangeredis-directly)
14. [Q14. How does `IDistributedCache` serialize values, and what constraints does that place on the objects you store?](#q14-how-does-idistributedcache-serialize-values-and-what-constraints-does-that-place-on-the-objects-you-store)
15. [Q15. How do you configure absolute and sliding expiration with `DistributedCacheEntryOptions`?](#q15-how-do-you-configure-absolute-and-sliding-expiration-with-distributedcacheentryoptions)
16. [Q16. How does distributed caching differ from ASP.NET Core output caching and response caching middleware?](#q16-how-does-distributed-caching-differ-from-aspnet-core-output-caching-and-response-caching-middleware)
17. [Q17. What is `ConnectionMultiplexer` in StackExchange.Redis, and why should you register it as a singleton?](#q17-what-is-connectionmultiplexer-in-stackexchangeredis-and-why-should-you-register-it-as-a-singleton)
18. [Q18. Beyond simple string key-value pairs, what Redis data structures does StackExchange.Redis expose, and when would you use hashes, sorted sets, or lists?](#q18-beyond-simple-string-key-value-pairs-what-redis-data-structures-does-stackexchangeredis-expose-and-when-would-you-use-hashes-sorted-sets-or-lists)
19. [Q19. How do you implement a distributed lock with Redis, and what pitfalls exist with naive lock implementations?](#q19-how-do-you-implement-a-distributed-lock-with-redis-and-what-pitfalls-exist-with-naive-lock-implementations)
20. [Q20. What is Redis pub/sub, and how is it used in .NET (for example SignalR backplane or event fan-out)?](#q20-what-is-redis-pubsub-and-how-is-it-used-in-net-for-example-signalr-backplane-or-event-fan-out)
21. [Q21. How should a production ASP.NET Core app handle Redis connection failures, timeouts, and transient errors?](#q21-how-should-a-production-aspnet-core-app-handle-redis-connection-failures-timeouts-and-transient-errors)
22. [Q22. What is TTL (time-to-live) in Redis, and how do you choose expiration values for different data types?](#q22-what-is-ttl-time-to-live-in-redis-and-how-do-you-choose-expiration-values-for-different-data-types)
23. [Q23. What are the Azure Cache for Redis tiers (Basic, Standard, Premium, and Enterprise), and when would you choose each?](#q23-what-are-the-azure-cache-for-redis-tiers-basic-standard-premium-and-enterprise-and-when-would-you-choose-each)
24. [Q24. What high-availability and scaling options does Azure Cache for Redis offer (replication, clustering, geo-replication)?](#q24-what-high-availability-and-scaling-options-does-azure-cache-for-redis-offer-replication-clustering-geo-replication)
25. [Q25. How do you configure ASP.NET Core session state to use Redis as the distributed session store?](#q25-how-do-you-configure-aspnet-core-session-state-to-use-redis-as-the-distributed-session-store)
26. [Q26. What security features does Azure Cache for Redis provide (TLS, access keys, firewall, Microsoft Entra ID authentication)?](#q26-what-security-features-does-azure-cache-for-redis-provide-tls-access-keys-firewall-microsoft-entra-id-authentication)
27. [Q27. What monitoring, alerting, and troubleshooting practices apply to Azure Cache for Redis in production?](#q27-what-monitoring-alerting-and-troubleshooting-practices-apply-to-azure-cache-for-redis-in-production)

---

## Q1. What is Azure Cache for Redis, and why would an ASP.NET Core application use it instead of keeping data in the application process?

**Concepts**
- Fully managed in-memory data store based on open-source Redis
- Shared key space across all scaled-out instances
- Cache entries survive individual process restarts
- Network round-trip latency vs in-process heap lookup
- Distributed coordination — locks, counters, pub/sub

**Answer**

Azure Cache for Redis is a fully managed in-memory data store based on open-source Redis, hosted in Azure and reachable over the network from any application instance. An ASP.NET Core app uses it when multiple servers, containers, or regions need to share the same cached data, when you want cache entries to survive an individual process restart, or when you need Redis-specific features such as pub/sub, atomic counters, or distributed locks that a local in-process cache cannot provide. In-process memory lives inside one application process, so each scaled-out instance maintains its own isolated copy and evicts everything when that process recycles during a deploy or crash, whereas a distributed cache sits outside the web tier so every instance reads and writes the same key space. Latency is higher than `IMemoryCache` — a network round trip versus a heap lookup — but the trade-off is consistency across nodes and a dedicated memory pool that does not compete with your application's garbage-collected heap. Typical wins include faster repeated database reads, reduced load on downstream APIs, shared session state, and coordination primitives for multi-instance scenarios.

---

## Q2. How does Azure Cache for Redis relate to open-source Redis, and what does Microsoft manage for you in the managed service?

**Concepts**
- Managed Redis engine — standard Redis protocol and commands
- Microsoft-managed provisioning, patching, replication, and backup
- Application code identical to self-hosted Redis
- Higher tiers — VNet injection, geo-replication, Redis Enterprise modules
- Caching strategy still the developer's responsibility

**Answer**

Azure Cache for Redis runs the Redis server engine inside Azure's infrastructure, exposing the standard Redis protocol and commands that client libraries such as StackExchange.Redis already speak. Microsoft handles provisioning, patching, monitoring hooks, replication, and backup options so your team consumes Redis as a platform service rather than operating virtual machines and Redis clusters yourself. Your application code, connection strings, and StackExchange.Redis calls look the same as against a self-hosted Redis instance — you connect with a hostname, port, and access key or Microsoft Entra ID token. Microsoft manages the underlying compute, operating system updates, Redis minor version upgrades within supported ranges, and built-in metrics in Azure Monitor. You still design key naming, expiration policies, serialization, and cache invalidation logic since the managed service does not replace application-level caching strategy. Higher tiers add Azure-specific capabilities such as data persistence options, virtual network injection, geo-replication, and Redis Enterprise modules on Enterprise SKUs.

---

## Q3. What problems does a distributed cache solve that an in-process cache (`IMemoryCache`) cannot?

**Concepts**
- Cache coherence across multiple web farm instances
- Entries persist across process restarts and deployments
- Shared atomic operations — counters, locks, pub/sub
- Two-level cache pattern — L1 in-process, L2 Redis
- Operational dependency trade-off — network latency and Redis availability

**Answer**

A distributed cache gives every application instance access to the same shared key space over the network, which an in-process `IMemoryCache` cannot do because its entries exist only inside one process's memory. When you scale ASP.NET Core to three pods behind a load balancer, `IMemoryCache` on pod A does not know what pod B cached — a user hitting pod B gets a cache miss even if pod A just loaded the same product record. Process-local cache is wiped on deploy, crash, or memory pressure within that process; Redis keeps entries until they expire or you delete them, independent of any single app instance lifecycle. Redis also exposes atomic operations, lists, sorted sets, and pub/sub that enable distributed locking, leaderboards, and SignalR backplanes — patterns that go beyond simple object caching in `IMemoryCache`. The cost is network latency and operational dependency on Redis availability, so many teams use both: hot per-request data in `IMemoryCache` and shared cross-instance data in Redis.

---

## Q4. Compare `IMemoryCache` and `IDistributedCache` — when would you use each, and can you use both together?

**Concepts**
- `IMemoryCache` — in-process heap storage, microsecond latency, stores live .NET objects
- `IDistributedCache` — external byte-array store, millisecond latency, manual serialization
- Two-level cache pattern — L1 memory, L2 Redis, L3 database
- `MemoryDistributedCache` — in-memory `IDistributedCache` substitute for tests
- Registration — `AddMemoryCache()` and `AddStackExchangeRedisCache()` coexist

**Answer**

`IMemoryCache` stores objects in the current application's memory heap with the lowest possible latency, while `IDistributedCache` stores byte arrays in an external store such as Azure Cache for Redis that all instances can reach. Use `IMemoryCache` for single-instance or request-scoped hot data and `IDistributedCache` when multiple servers must see the same entries. Using both together in a two-level cache is a common production pattern — it checks `IMemoryCache` first, then Redis on miss, then the database, populating both on the way back to amortize network cost across many requests on the same node. Register `AddMemoryCache()` and `AddStackExchangeRedisCache()` independently; they resolve to different interfaces and do not conflict. Choose `IMemoryCache` alone only when you run a single instance or stale per-node copies are acceptable; choose Redis when correctness across instances matters more than raw speed.

| | `IMemoryCache` | `IDistributedCache` (Redis) |
|---|---|---|
| Scope | One process | All processes sharing the Redis instance |
| Latency | Microseconds (in-heap) | Milliseconds (network + serialization) |
| Serialization | Stores live .NET objects | Stores `byte[]`; caller serializes |
| Survives deploy | No | Yes (until TTL or explicit delete) |
| Best for | Config snapshots, per-node memoization | Shared sessions, cross-node catalogs, rate limits |

---

## Q5. What are the most common caching use cases for Azure Cache for Redis in ASP.NET Core (API responses, reference data, rate limiting, and others)?

**Concepts**
- Reference and lookup data — infrequently changed, read on every request
- Database query result caching — paginated lists, EF Core aggregates
- API response caching — serialized JSON keyed by route and query
- Rate limiting — atomic Redis counters with expiration windows
- Session state and pub/sub SignalR backplane

**Answer**

Azure Cache for Redis most often accelerates read-heavy workloads by storing expensive query results, reference data, and computed API responses so repeated requests avoid hitting SQL or external services. Teams also use it for cross-instance coordination — session state, rate limiting, distributed locks, and SignalR backplane messaging — where shared mutable state must be fast and atomic. Reference and lookup data such as countries, product categories, or permission maps changes infrequently but is read on nearly every request; a short TTL with explicit invalidation on admin updates works well. Database query results — paginated lists, dashboard aggregates, or entity graphs loaded by Entity Framework Core — benefit significantly when the same key is requested many times per second. API response caching stores serialized JSON for public catalog endpoints keyed by route plus query parameters, with TTL aligned to business tolerance for staleness. Rate limiting uses atomic increment per client IP or API key using Redis counters with expiration windows, and SignalR scale-out uses Redis to broadcast hub messages across all web nodes without sticky sessions.

---

## Q6. What is the cache-aside (lazy loading) pattern, and walk through the read and write flow?

**Concepts**
- Cache-aside — application owns cache explicitly, not the database
- Read flow — check cache, miss loads from source, populate cache
- Write flow — persist to database first, then delete or update cache entry
- Cache as performance layer — not the system of record
- Invalidation on write — deletion safer than partial update

**Answer**

Cache-aside means the application code owns the cache explicitly: on a read it checks the cache first and loads from the authoritative store only on a miss, then writes the result into the cache; on a write it updates the database first and then deletes or updates the cache entry. The cache is never the system of record — it is a performance layer the application manages around the real data source. On a read, the application builds a cache key such as `product:42`, calls `IDistributedCache.GetAsync(key)`, and on a hit deserializes and returns; on a miss it queries the database or API, serializes the result, calls `SetAsync` with an appropriate expiration, and returns the data to the caller. On a write, the application persists the change to the database first, then removes the cache key with `RemoveAsync` or overwrites it — deletion is safer when updates are partial or complex. Stale data appears if you update the database but forget to invalidate the cache, so always pair writes with explicit invalidation or a short TTL as a safety net. This is the default pattern with `IDistributedCache` because the interface provides no automatic database integration.

---

## Q7. What is write-through caching, and when is it preferable to cache-aside?

**Concepts**
- Write-through — update cache and database together on every write
- Read-after-write consistency — readers always see the latest committed value
- Extra write latency — two destinations per write operation
- Eliminates "forgot to invalidate" failure mode for that key
- Heavy write workloads — may prefer cache-aside with invalidation

**Answer**

Write-through caching updates the cache and the authoritative store together on every write, typically inside a single coordinating layer, so the cache always holds the latest committed value after a write completes. It is preferable when read-after-write consistency matters and you can afford the extra write latency on every update, because readers never see stale cached data after a successful write. On a write, the coordinating code saves to the database and simultaneously calls `SetAsync` with the new value in Redis before returning success to the caller; on a read, the application almost always hits the cache because writes kept it synchronized. Compared to cache-aside, write-through adds latency to every write since two destinations must succeed, but it eliminates the "updated DB, forgot to invalidate cache" failure mode for that key. Use it for high-read, moderate-write reference data where stale reads are unacceptable and write volume is manageable; `IDistributedCache` does not implement write-through automatically — you encode the dual-write sequence in your repository or service layer.

---

## Q8. What is write-behind (write-back) caching, and what consistency risks does it introduce?

**Concepts**
- Write-behind — acknowledge to cache immediately, persist to database asynchronously
- Write throughput optimization — background flush reduces request latency
- Durability risk — unpersisted writes lost on cache node failure
- Stale reads — other services reading database see lagging data
- Use cases — telemetry counters, non-critical preferences; not for entity state

**Answer**

Write-behind (write-back) caching acknowledges writes to the cache immediately and persists them to the authoritative database asynchronously in a batch or background flush, optimizing write throughput at the cost of durability and consistency guarantees. If the cache node fails before the flush completes, unpersisted writes are lost, and readers on other instances may see data that has not yet reached the database. The application writes only to Redis on the request path and returns quickly; a background worker or timed flush pushes accumulated changes to SQL. This suits write-heavy telemetry, analytics counters, or non-critical preference updates where losing a few seconds of data on catastrophic cache failure is acceptable. Another service reading directly from the database will see lagging data, and ordering and conflict resolution become hard when multiple keys map to related rows — which is why most business transaction systems in ASP.NET Core avoid write-behind for authoritative entity state, preferring cache-aside or write-through instead.

---

## Q9. What is cache stampede (thundering herd), and how do you prevent it in a .NET application?

**Concepts**
- Cache stampede — popular entry expires, many concurrent misses reload simultaneously
- Single-flight (request coalescing) — one caller reloads, others await same task
- Probabilistic early expiration — refresh before hard TTL expiry
- Stale-while-revalidate — return expired value while background refresh runs
- TTL jitter — random offset prevents mass simultaneous expiry

**Answer**

A cache stampede occurs when a popular cache entry expires (or is deleted) and many concurrent requests miss at once, each triggering an expensive reload of the same underlying data and overwhelming the database. Prevention strategies ensure only one caller rebuilds the entry while others wait or serve slightly stale data. Single-flight coalescing uses a per-key lock — `SemaphoreSlim`, Redis `SET NX` lock, or HybridCache's built-in coordination — so the first miss acquires the lock, reloads data, repopulates the cache, and releases; concurrent misses await the same task instead of each querying SQL. Probabilistic early expiration refreshes hot keys slightly before hard TTL expiry so the entry is rarely absent at the same instant for all clients. Stale-while-revalidate returns the expired cached value immediately while one background task refreshes it, trading brief staleness for stable database load. Adding random seconds (jitter) to expiration prevents thousands of keys created in the same deployment wave from expiring in the same second. In .NET 9+, `HybridCache` combines L1/L2 caching with stampede protection; with raw `IDistributedCache`, implement single-flight manually around your cache-aside helper.

---

## Q10. When should you avoid caching data, and what types of data are poor cache candidates?

**Concepts**
- Strong consistency requirements — real-time financial or authorization decisions
- Low reuse — one-off query results never requested again
- Large payloads — multi-megabyte values hurt Redis performance
- PII in shared cache — expanded compliance surface
- Hit rate below ~30% — caching adds overhead without benefit

**Answer**

Avoid caching when data must always reflect the latest authoritative state, when entries are unique per request and will never be read again, or when the cost of serialization and network round trips exceeds the cost of fetching the source directly. Poor candidates include highly volatile financial balances, one-off search results, large binary blobs that exceed Redis value limits, and sensitive secrets that should not sit in a shared memory store. Real-time inventory for high-contention purchases or fraud checks requires strong consistency since a stale cache causes overselling or incorrect authorization decisions. Report exports keyed by arbitrary user filters are accessed once, so caching adds overhead without hit rate benefit. Multi-megabyte files or graph responses should be stored in Azure Blob Storage with a reference URL rather than in Redis, since large values hurt performance across the board. Personal sensitive data expands the compliance surface unless encrypted and TTL-bound. When in doubt, measure hit rate and latency improvement; a cache with under 30% hits may not justify the added complexity.

---

## Q11. How do you handle cache invalidation when the authoritative source (database or API) changes?

**Concepts**
- Key-per-entity deletion — `RemoveAsync` after `SaveChanges` for predictable keys
- Tag or prefix invalidation — set of related keys deleted together
- Pub/sub broadcast — cross-instance `IMemoryCache` invalidation signal
- TTL as safety net — self-healing for missed invalidation
- Versioned keys — bump version on schema or bulk refresh

**Answer**

Cache invalidation means removing or updating cache entries when the underlying data changes so readers never rely on stale values indefinitely. The application that performs the write is responsible for invalidating affected keys — there is no automatic link between Entity Framework Core saves and Redis unless you build it. Key-per-entity deletion calls `RemoveAsync` after a save for predictable keys such as `product:{id}`; it is simple and precise when the mapping is one-to-one. Tag or prefix invalidation maintains a set of keys related to a category and deletes the set when the category changes; Redis sets or a custom index support this. Pub/sub broadcast publishes an invalidation message on update; all app instances subscribe and drop matching entries from local `IMemoryCache` and/or Redis. Always set a reasonable TTL even with explicit invalidation so orphaned keys self-heal after missed invalidation bugs. Versioned keys include a version number in the key such as `product:42:v3` and bump the version on schema or bulk refresh instead of hunting every old key. Invalidation granularity is a design choice — coarse invalidation is simpler but causes more reload traffic; fine-grained is precise but easy to miss a dependent key.

---

## Q12. How do you register Azure Cache for Redis as `IDistributedCache` in ASP.NET Core, and where does the connection string come from?

**Concepts**
- `Microsoft.Extensions.Caching.StackExchangeRedis` NuGet package
- `AddStackExchangeRedisCache` in `Program.cs` — wires `IDistributedCache`
- Connection string format — hostname, port, SSL, password, abortConnect
- `InstanceName` — key prefix for multi-app Redis sharing
- `abortConnect=False` — app starts even if Redis is temporarily unreachable

**Answer**

Install the `Microsoft.Extensions.Caching.StackExchangeRedis` package, add the connection string to configuration from Azure Portal or Azure Key Vault, and call `AddStackExchangeRedisCache` in `Program.cs` so the container resolves `IDistributedCache` backed by your Azure instance. Create the Azure Cache for Redis resource in the portal, copy the primary connection string from Access keys, and store it in `appsettings.json`, environment variables, or Key Vault — never commit secrets to source control. The `InstanceName` prefix helps when multiple applications share one Redis instance for cost savings. `abortConnect=False` lets the app start even if Redis is temporarily unreachable; operations throw at call time instead of failing startup — pair with health checks for readiness. Inject `IDistributedCache` into services or minimal API handlers; the abstraction keeps unit tests on an in-memory `MemoryDistributedCache` substitute.

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApp:"; // optional key prefix
});
```

---

## Q13. What is the difference between `AddStackExchangeRedisCache` and using `ConnectionMultiplexer` from StackExchange.Redis directly?

**Concepts**
- `AddStackExchangeRedisCache` — registers `IDistributedCache` adapter for get/set/remove
- `ConnectionMultiplexer` — exposes full Redis command surface (hashes, lists, pub/sub)
- Interface difference — `IDistributedCache` vs `IConnectionMultiplexer` / `IDatabase`
- Mockability — `IDistributedCache` easier to swap in tests
- Coexistence — register both independently when both are needed

**Answer**

`AddStackExchangeRedisCache` registers an `IDistributedCache` adapter that stores serialized byte arrays under string keys using a simplified API, while `ConnectionMultiplexer` is the low-level StackExchange.Redis client that exposes the full Redis command surface — hashes, lists, pub/sub, Lua scripts, and atomic operations. Use `IDistributedCache` for standard get/set/remove caching; register `ConnectionMultiplexer` as a singleton when you need Redis features beyond key-value strings. Both can share one connection string; register `ConnectionMultiplexer.Connect(...)` as singleton separately if you need both in the same app. `IDistributedCache` is easier to mock and swap for `MemoryDistributedCache` in tests since direct multiplexer calls couple tests to Redis or require wrapping interfaces. SignalR's Redis backplane uses its own StackExchange.Redis integration, not `IDistributedCache`.

| | `AddStackExchangeRedisCache` | `ConnectionMultiplexer` |
|---|---|---|
| Interface | `IDistributedCache` | `IConnectionMultiplexer` / `IDatabase` |
| API surface | Get, Set, Remove, async variants | Full Redis commands |
| Use case | Typical cache-aside | Locks, counters, pub/sub, complex structures |

---

## Q14. How does `IDistributedCache` serialize values, and what constraints does that place on the objects you store?

**Concepts**
- `IDistributedCache` stores only `byte[]` — no automatic serialization
- JSON via `System.Text.Json` — most common choice for human-readable values
- DTO constraint — no `DbContext`, `HttpClient`, or unmanaged resource types
- Schema change risk — bump key prefix when cached type shape changes
- `MemoryDistributedCache` follows same byte contract for test portability

**Answer**

`IDistributedCache` stores only `byte[]` — it does not serialize .NET objects for you, so your application must convert objects to bytes before `SetAsync` and deserialize after `GetAsync`, which means you choose the format (JSON, MessagePack, or Protocol Buffers) and handle version compatibility yourself. `SetAsync(key, bytes, options)` and `GetAsync(key)` operate on raw bytes; there is no generic `Set<T>` on the interface. JSON via `System.Text.Json` is the most common choice in modern ASP.NET Core because it is human-readable in Redis CLI debugging and aligns with API payloads. Cached types should be data transfer objects — not `DbContext`, `HttpClient`, or types holding unmanaged resources — because you are persisting a snapshot, not live object graphs. Schema changes break deserialization unless you version keys or use tolerant JSON options; deploy new key prefixes when changing shape. `MemoryDistributedCache` used in development or tests follows the same byte contract, so serialization helpers transfer between providers.

```csharp
var json = JsonSerializer.Serialize(product);
await cache.SetAsync(key, Encoding.UTF8.GetBytes(json), options);
```

---

## Q15. How do you configure absolute and sliding expiration with `DistributedCacheEntryOptions`?

**Concepts**
- Absolute expiration — fixed deadline from write time regardless of access
- Sliding expiration — countdown resets on each read
- Combined — sliding keeps hot keys warm, absolute caps maximum staleness
- No expiration — only for deliberately managed configuration keys
- Redis TTL — enforced internally; verify behavior for precise combined semantics

**Answer**

Pass `DistributedCacheEntryOptions` to `SetAsync` to control how long an entry lives in Redis: absolute expiration sets a fixed deadline from the moment of write, sliding expiration resets the countdown on every read, and you can combine both so activity extends lifetime up to a hard maximum. Absolute expiration means the entry dies at `UtcNow + configured duration` regardless of read frequency, which is good for data with a known freshness window such as daily exchange rates. Sliding expiration extends life by another interval on each `GetAsync`, making it ideal for session-like data accessed repeatedly while abandoned keys eventually drop off. When combined, sliding keeps hot keys warm while absolute caps maximum staleness even under constant access. No expiration is appropriate only for true configuration keys maintained by deliberate updates — always set TTL intentionally in production to avoid filling Redis memory.

```csharp
var options = new DistributedCacheEntryOptions()
    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

await cache.SetAsync(key, bytes, options);
```

---

## Q16. How does distributed caching differ from ASP.NET Core output caching and response caching middleware?

**Concepts**
- `IDistributedCache` — general-purpose key-value store, manual code control
- Output caching (.NET 7+) — full HTTP response cached by framework policy
- Response caching middleware — honors `Cache-Control` and `ETag` headers
- Granularity difference — partial expensive work vs full response
- Personalized authenticated responses — avoid full-response caching without vary policy

**Answer**

Distributed caching via `IDistributedCache` is a general-purpose key-value store your application code reads and writes explicitly, while output caching and response caching middleware cache entire HTTP responses at the framework level based on headers, routes, and policies. They solve different layers of the stack and can complement each other when configured carefully. Output caching stores status code, headers, and body after the pipeline runs — you decorate endpoints with `[OutputCache]` or policies with tags and durations. Response caching middleware honors `Cache-Control` and `ETag` headers and caches on the handling instance unless extended, which is less flexible than output caching in modern apps. Use `IDistributedCache` when only part of the work is expensive such as a database call inside the handler; use output caching when the entire GET response is identical for many clients. Authenticated personalized responses usually should not use full-response caching unless policies vary by user or role explicitly.

| | `IDistributedCache` | Output caching | Response caching |
|---|---|---|---|
| Granularity | Arbitrary keys you define | Full HTTP response | Full HTTP response |
| Control | Manual in code | Attributes / policies | `[ResponseCache]`, headers |
| Storage | Redis, SQL, memory | In-memory or Redis | In-memory on the server |

---

## Q17. What is `ConnectionMultiplexer` in StackExchange.Redis, and why should you register it as a singleton?

**Concepts**
- `ConnectionMultiplexer` — long-lived client managing shared connection pool
- Singleton per Redis server — thread-safe, designed for sharing across requests
- `GetDatabase()` — lightweight `IDatabase` facade, call per operation
- `abortConnect=False` — allows startup before Redis is reachable
- Dispose on application shutdown via `IHostApplicationLifetime`

**Answer**

`ConnectionMultiplexer` is the long-lived client object in StackExchange.Redis that manages a shared connection pool, multiplexes many concurrent operations over a small number of physical connections to Redis, and handles automatic reconnects. You register one instance as a singleton for the application lifetime because creating a multiplexer per request is expensive and defeats connection reuse; the library is designed around one multiplexer per Redis cluster or standalone server per process — thread-safe and meant to be shared across all requests. `GetDatabase()` returns a lightweight `IDatabase` facade; call it per operation rather than caching `IDatabase` as singleton if you need different database numbers. On connection blips, the multiplexer retries and buffers short outages; `abortConnect=False` in the connection string allows startup before Redis is reachable. Dispose the multiplexer on application shutdown via `IHostApplicationLifetime` to close connections cleanly.

```csharp
builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));

// inject IDatabase per operation
var db = multiplexer.GetDatabase();
await db.StringGetAsync("product:42");
```

---

## Q18. Beyond simple string key-value pairs, what Redis data structures does StackExchange.Redis expose, and when would you use hashes, sorted sets, or lists?

**Concepts**
- Strings — cached JSON blobs, counters, distributed lock tokens
- Hashes — field-value maps for partial field updates without full rewrite
- Lists — FIFO queues and recent-history buffers
- Sets — unique tag membership and invalidation indexes
- Sorted sets — leaderboards, priority queues, time-window rate limits

**Answer**

StackExchange.Redis maps Redis's native types to API methods on `IDatabase` — strings for simple values, hashes for object fields, lists for queues, sets for unique collections, and sorted sets for ranked data — each with atomic server-side operations. Use the structure that matches access patterns instead of serializing everything as JSON strings when partial reads or atomic updates matter. Strings via `StringGet` / `StringSet` suit cached JSON blobs, counters via `StringIncrement`, and distributed lock tokens; they are the simplest cache-aside storage. Hashes via `HashSet` / `HashGetAll` store field-value maps such as user profile fields so you can update one field without rewriting the entire object. Lists via `ListLeftPush` / `ListRightPop` implement queues and recent-history buffers with FIFO ordering. Sets via `SetAdd` / `SetMembers` track unique tags, online user IDs, or invalidation tag membership without duplicates. Sorted sets via `SortedSetAdd` with scores support leaderboards, priority queues, and time-window rate limits ordered by timestamp score. Complex structures reduce serialization overhead and enable atomic increments, but increase code complexity — many ASP.NET Core apps stay with string JSON until a specific pattern requires more.

---

## Q19. How do you implement a distributed lock with Redis, and what pitfalls exist with naive lock implementations?

**Concepts**
- `SET key token NX EX seconds` — atomic acquire if absent
- Unique lock token — prevents releasing another instance's lock
- Lua script for atomic release — delete only if value matches token
- Watchdog extension — renew TTL for long-running work
- Redlock algorithm or Medallion.Threading.Redis for higher correctness

**Answer**

A distributed lock uses Redis to coordinate so only one process across all instances performs a critical section at a time, typically with `SET key unique-token NX EX seconds` so the key is set only if absent and auto-expires to prevent deadlocks. The naive pitfall of using `GET` then `SET` without atomicity allows two clients to both see an empty key and both proceed, since the check-then-set is not atomic. Release must use a Lua script that deletes the key only if the value still equals your unique token, preventing you from removing a lock reacquired by another instance after yours expired. Long-running work may renew the TTL with a watchdog loop if the same owner still holds the token, since a fixed TTL that expires before work completes allows a second worker to enter. Redis locks suit cache rebuild single-flight or short cron coordination — not a full replacement for database transactional locks on financial invariants where the Redlock algorithm or a dedicated library such as Medallion.Threading.Redis provides higher correctness.

---

## Q20. What is Redis pub/sub, and how is it used in .NET (for example SignalR backplane or event fan-out)?

**Concepts**
- Redis pub/sub — fire-and-forget channel messaging, no persistence
- `ISubscriber.PublishAsync` and `Subscribe` — StackExchange.Redis API
- SignalR Redis backplane — broadcasts hub messages across web instances
- Cache invalidation broadcast — cross-node `IMemoryCache` eviction signal
- No durability — offline subscribers miss messages; use Service Bus or Redis Streams for reliable delivery

**Answer**

Redis pub/sub is a fire-and-forget messaging channel where publishers send messages to a named channel and all subscribers currently connected receive a copy, with no persistence for offline consumers. In .NET, SignalR uses it as a backplane so hub messages sent on one web instance reach clients connected to other instances, and custom services use `ISubscriber` for lightweight event fan-out. Pub/sub does not store messages — subscribers offline at publish time miss the event — so use Azure Service Bus or Redis Streams when you need durable messaging. Pub/sub is suitable for cache invalidation broadcasts, live notifications, and cross-node SignalR fan-out where missed messages on reconnect are tolerable.

```csharp
// publish
await subscriber.PublishAsync(RedisChannel.Literal("orders"), payload);

// subscribe
subscriber.Subscribe(RedisChannel.Literal("orders"), (channel, message) => { ... });
```

SignalR registers with `AddStackExchangeRedis` and a `ChannelPrefix` so only your app's messages share the bus.

---

## Q21. How should a production ASP.NET Core app handle Redis connection failures, timeouts, and transient errors?

**Concepts**
- Graceful degradation — Redis as best-effort acceleration, fall back to database on failure
- `abortConnect=False` — app starts without Redis; operations fail at call time
- Health check — return Degraded instead of Unhealthy when operable without cache
- `connectTimeout` and `syncTimeout` in connection string — bound hung Redis
- Circuit breaker with Polly — prevent hammering a failing Redis instance

**Answer**

Treat Redis as a best-effort acceleration layer unless you explicitly design for hard dependency: catch timeouts on cache operations, fall back to the authoritative data source on failure, and expose health checks so orchestrators know when the app is degraded. Use `abortConnect=False` so the app starts without Redis; wrap `GetAsync`/`SetAsync` in try/catch and on failure proceed as a cache miss to the database. Register a health check — `AddRedis` from `AspNetCore.HealthChecks.Redis` — on readiness endpoints, returning Degraded instead of Unhealthy if you can operate without cache. Set reasonable timeouts in the connection string via `connectTimeout` and `syncTimeout` so hung Redis does not block request threads indefinitely. For critical coordination such as locks or idempotency, explicit failure handling is required — do not silently swallow errors that could cause duplicate processing. Circuit breaker patterns via Polly around Redis calls prevent hammering a failing instance and speed recovery to fallback paths.

---

## Q22. What is TTL (time-to-live) in Redis, and how do you choose expiration values for different data types?

**Concepts**
- TTL — automatic key expiration after configured seconds or milliseconds
- Freshness vs hit-rate trade-off — volatile data short TTL, stable data longer TTL
- Session TTL — aligns with authentication cookie timeout policy
- Rate-limit window TTL — matches the window duration exactly
- TTL jitter — random offset to prevent mass simultaneous expiry

**Answer**

TTL is the automatic expiration Redis applies to a key after a set duration. Choosing TTL balances freshness against hit rate: volatile data gets short TTL, stable reference data gets longer TTL, and session data aligns with authentication timeout policy. Hot reference data such as product catalogs typically uses 5–60 minutes with explicit invalidation on admin updates; longer if change frequency is low and brief staleness is acceptable. Computed aggregates such as dashboard totals suit 1–5 minutes or a scheduled rebuild, matched to business tolerance stated in service-level objectives. User session TTL should match cookie timeout and security policy — 20–60 minutes sliding is common. Rate-limit windows use the exact window length such as 60 seconds for 100 requests per minute. Add random offset (±10%) to mass-created keys so they do not expire simultaneously. No TTL is appropriate only for true configuration keys maintained by deliberate updates — monitor memory when keys never expire.

---

## Q23. What are the Azure Cache for Redis tiers (Basic, Standard, Premium, and Enterprise), and when would you choose each?

**Concepts**
- Basic — single node, no SLA, dev/test only
- Standard — two nodes with replication and failover, minimum for production
- Premium — clustering, VNet injection, data persistence, geo-replication
- Enterprise — Redis Enterprise modules, extreme scale, advanced HA
- Never use Basic in production — no replication, no SLA

**Answer**

Azure Cache for Redis tiers trade off high availability, throughput, persistence, networking, and cost. Basic is a single node for development with no SLA for production — a node restart loses in-memory data and causes downtime. Standard provides automatic failover when the primary fails and is the minimum for production web apps using session or shared cache. Premium supports shard clustering for larger datasets, data persistence (RDB/AOF options), deployment into a virtual network, and geo-replication for disaster recovery. Enterprise targets large enterprises needing Redis Enterprise modules or multi-terabyte scenarios at higher cost with specialized features. Choose Standard as the baseline for any production ASP.NET Core app; move to Premium when memory requirements exceed Standard limits, when you need VNet isolation, or when disaster recovery across regions is a requirement.

| Tier | Nodes | Replication | Typical use |
|---|---|---|---|
| Basic | 1 | None | Dev/test only |
| Standard | 2 (primary + replica) | Yes | Production apps needing failover |
| Premium | 2+ with optional cluster | Yes + clustering | Large memory, VNet, persistence, geo-replication |
| Enterprise | Redis Enterprise | Advanced HA | Modules, extreme scale |

---

## Q24. What high-availability and scaling options does Azure Cache for Redis offer (replication, clustering, geo-replication)?

**Concepts**
- Replication (Standard+) — primary and replica with automatic failover
- Clustering (Premium) — up to 10 shards, keys partitioned across shards
- Geo-replication (Premium) — secondary region for disaster recovery
- Zone redundancy — availability zone spread for datacenter resilience
- Scale operations — brief connection blips, multiplexer reconnect logic needed

**Answer**

Standard tier and above replicate data to a secondary node with automatic failover, Premium adds Redis cluster sharding to scale memory and throughput horizontally, and Premium geo-replication links two regional caches for disaster recovery with manual failover between regions. Replication keeps a primary handling traffic while the replica stays synchronized; Azure promotes the replica on primary failure with minimal downtime. Clustering partitions data across up to 10 shards; client libraries like StackExchange.Redis discover topology automatically when clustering is enabled. Geo-replication with a secondary region is useful for read preference or DR drills — it is not transparent active-active for all write patterns without application design. Zone redundancy deploys across availability zones within a region for resiliency against datacenter failures on supported configurations per SKU. Scale operations may cause brief connection blips, so use resilient multiplexer settings and retry logic during maintenance windows.

---

## Q25. How do you configure ASP.NET Core session state to use Redis as the distributed session store?

**Concepts**
- Session ID in cookie, bulk payload in Redis
- `AddStackExchangeRedisCache` + `AddSession` + `UseSession` middleware
- `InstanceName` prefix — isolates session keys from other cache keys
- All instances must share same Redis config and prefix
- Session removes sticky sessions requirement on load balancer

**Answer**

Install `Microsoft.Extensions.Caching.StackExchangeRedis`, register distributed cache with the Redis connection string, then call `AddSession` and `UseSession` middleware so session payloads serialize to Redis instead of an in-memory store. The session cookie on the client holds only the session ID — the bulk data lives in Redis. All instances must share the same Redis configuration and `InstanceName` prefix or sessions appear empty after load balancing routes to another node, which removes the need for sticky sessions on the load balancer. Use `HttpContext.Session.SetString` / `SetInt32` for simple values; complex objects require manual serialization to bytes or JSON strings. Protect the connection string and enable TLS since session data may contain sensitive user context.

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApp:Session:";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

app.UseSession();
```

---

## Q26. What security features does Azure Cache for Redis provide (TLS, access keys, firewall, Microsoft Entra ID authentication)?

**Concepts**
- TLS on port 6380 — encrypts data in transit
- Primary and secondary access keys — rotation without dual downtime
- Firewall rules — IP allowlist for public endpoint
- Private endpoint / VNet injection (Premium) — no public IP
- Microsoft Entra ID authentication — eliminates shared secrets with managed identity

**Answer**

Azure Cache for Redis encrypts data in transit with TLS on port 6380, authenticates clients with access keys or Microsoft Entra ID on supported tiers, and restricts network reach with firewall rules and private endpoints inside a virtual network. Connect with `ssl=True` in the connection string; Azure rejects plain-text connections for current caches. Primary and secondary keys allow rotation without dual downtime — regenerate secondary, deploy apps, then regenerate primary. Firewall rules allow only Azure App Service outbound IPs or specific corporate ranges with default deny for public endpoints. Premium VNet injection means Redis has no public IP; traffic stays on the Azure backbone from your app subnet. Microsoft Entra authentication eliminates shared secrets in connection strings where supported; apps acquire tokens via managed identity, which is the preferred zero-secret rotation approach for Azure-hosted ASP.NET Core apps. Redis is not encrypted at rest on all SKUs identically — check current Azure documentation for your tier; sensitive payloads can be encrypted application-side before `SetAsync` if the tier does not meet compliance requirements.

---

## Q27. What monitoring, alerting, and troubleshooting practices apply to Azure Cache for Redis in production?

**Concepts**
- Azure Monitor metrics — connected clients, memory, evictions, operations/sec
- Application-level instrumentation — cache hit/miss rates and serialization size
- Alert thresholds — memory above 80%, sustained evictions, connection drops
- Common issues — large values, `KEYS *` in production, missing TTL, wrong prefix
- Load testing failover — validate multiplexer reconnect before production dependence

**Answer**

Monitor server health through Azure Monitor metrics and diagnostics, track application-level hit and miss rates in your own logging, alert on connection errors and memory pressure, and troubleshoot latency by correlating Redis slowlog with expensive commands. Azure metrics to watch include `connectedclients`, `cachehits` / `cachemisses`, `usedmemory`, `evictedkeys`, `operationsPerSecond`, `serverLoad`, and replication lag. At the application layer, log cache operation duration, fallback-to-database counts, and serialization size; use Application Insights dependencies for Redis calls when using direct StackExchange.Redis. Set alerts when memory exceeds 80%, when evictions are sustained above zero, when connection count drops during steady traffic, or when health check failures appear on `/health/ready`. Common issues include large values causing high latency, `KEYS *` blocking the server in production (use `SCAN` instead), missing TTL filling memory, a wrong `InstanceName` prefix causing apparent misses, and firewall blocking App Service outbound IPs after a scale change. Validate failover behavior on Standard/Premium during a planned maintenance window before production dependence to ensure multiplexer reconnect logic is active during infrastructure changes.

---
