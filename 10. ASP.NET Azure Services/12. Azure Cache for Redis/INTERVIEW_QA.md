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

What is Azure Cache for Redis, and why would an ASP.NET Core application use it instead of keeping data in the application process?

**Answer:** Azure Cache for Redis is a fully managed in-memory data store based on open-source Redis, hosted in Azure and reachable over the network from any application instance. An ASP.NET Core app uses it when multiple servers, containers, or regions need to share the same cached data, when you want cache entries to survive an individual process restart, or when you need Redis-specific features such as pub/sub, atomic counters, or distributed locks that a local in-process cache cannot provide.

- In-process memory lives inside one application process, so each scaled-out instance maintains its own isolated copy and evicts everything when that process recycles during a deploy or crash.
- A distributed cache sits outside the web tier, so every instance reads and writes the same key space and you can warm or invalidate data once for the whole fleet.
- Latency is higher than `IMemoryCache` (a network round trip versus a heap lookup), but the trade-off is consistency across nodes and a dedicated memory pool that does not compete with your application's garbage-collected heap.
- Typical wins include faster repeated database reads, reduced load on downstream APIs, shared session state, and coordination primitives for multi-instance scenarios.

---

## Q2. How does Azure Cache for Redis relate to open-source Redis, and what does Microsoft manage for you in the managed service?

How does Azure Cache for Redis relate to open-source Redis, and what does Microsoft manage for you in the managed service?

**Answer:** Azure Cache for Redis runs the Redis server engine (open-source Redis or Redis Enterprise on higher tiers) inside Azure's infrastructure, exposing the standard Redis protocol and commands that client libraries such as StackExchange.Redis already speak. Microsoft handles provisioning, patching, monitoring hooks, replication, and backup options so your team consumes Redis as a platform service rather than operating virtual machines and Redis clusters yourself.

- Your application code, connection strings, and StackExchange.Redis calls look the same as against a self-hosted Redis instance — you connect with a hostname, port, and access key (or Microsoft Entra ID token on supported configurations).
- Microsoft manages the underlying compute, operating system updates, Redis minor version upgrades within supported ranges, and built-in metrics in Azure Monitor.
- You still design key naming, expiration policies, serialization, and cache invalidation logic — the managed service does not replace application-level caching strategy.
- Higher tiers add Azure-specific capabilities such as data persistence options, virtual network injection, geo-replication, and Redis Enterprise modules on Enterprise SKUs.

---

## Q3. What problems does a distributed cache solve that an in-process cache (`IMemoryCache`) cannot?

What problems does a distributed cache solve that an in-process cache (`IMemoryCache`) cannot?

**Answer:** A distributed cache gives every application instance access to the same shared key space over the network, which an in-process `IMemoryCache` cannot do because its entries exist only inside one process's memory. That shared store solves cache coherence across a web farm, lets cache survive individual process restarts, and provides a central place for cross-cutting data such as session tokens, rate-limit counters, or pub/sub channels.

- When you scale ASP.NET Core to three pods behind a load balancer, `IMemoryCache` on pod A does not know what pod B cached — a user hitting pod B gets a cache miss even if pod A just loaded the same product record.
- Process-local cache is wiped on deploy, crash, or memory pressure within that process; Redis keeps entries until they expire or you delete them, independent of any single app instance lifecycle.
- Redis exposes atomic operations, lists, sorted sets, and pub/sub that enable distributed locking, leaderboards, and SignalR backplanes — patterns that go beyond simple object caching in `IMemoryCache`.
- The cost is network latency and operational dependency on Redis availability, so many teams use both: hot per-request data in `IMemoryCache` and shared cross-instance data in Redis.

---

## Q4. Compare `IMemoryCache` and `IDistributedCache` — when would you use each, and can you use both together?

Compare `IMemoryCache` and `IDistributedCache` — when would you use each, and can you use both together?

**Answer:** `IMemoryCache` stores objects in the current application's memory heap with the lowest possible latency, while `IDistributedCache` stores byte arrays in an external store such as Azure Cache for Redis that all instances can reach. You use `IMemoryCache` for single-instance or request-scoped hot data and `IDistributedCache` when multiple servers must see the same entries; using both together in a two-level cache is a common production pattern.

| | `IMemoryCache` | `IDistributedCache` (Redis) |
|---|---|---|
| Scope | One process | All processes sharing the Redis instance |
| Latency | Microseconds (in-heap) | Milliseconds (network + serialization) |
| Serialization | Stores live .NET objects | Stores `byte[]`; caller serializes |
| Survives deploy | No | Yes (until TTL or explicit delete) |
| Best for | Config snapshots, per-node memoization | Shared sessions, cross-node product catalog, rate limits |

- A two-level pattern checks `IMemoryCache` first, then Redis on miss, then the database — populating both on the way back to amortize network cost across many requests on the same node.
- Register `AddMemoryCache()` and `AddStackExchangeRedisCache()` independently; they resolve to different interfaces and do not conflict.
- Choose `IMemoryCache` alone only when you run a single instance or stale per-node copies are acceptable; choose Redis when correctness across instances matters more than raw speed.

---

## Q5. What are the most common caching use cases for Azure Cache for Redis in ASP.NET Core (API responses, reference data, rate limiting, and others)?

What are the most common caching use cases for Azure Cache for Redis in ASP.NET Core (API responses, reference data, rate limiting, and others)?

**Answer:** Azure Cache for Redis most often accelerates read-heavy workloads by storing expensive query results, reference data, and computed API responses so repeated requests avoid hitting SQL or external services. Teams also use it for cross-instance coordination — session state, rate limiting, distributed locks, and SignalR backplane messaging — where shared mutable state must be fast and atomic.

- **Reference and lookup data** — Countries, product categories, or permission maps that change infrequently but are read on nearly every request; a short time-to-live (TTL) with explicit invalidation on admin updates works well.
- **Database query results** — Paginated lists, dashboard aggregates, or entity graphs loaded by Entity Framework Core where the same key is requested many times per second.
- **API response caching** — Serialized JSON for public catalog endpoints, keyed by route plus query parameters, with TTL aligned to business tolerance for staleness.
- **Rate limiting and throttling** — Atomic increment per client IP or API key using Redis counters with expiration windows.
- **Session and TempData** — ASP.NET Core session middleware can persist session payloads to Redis so users stay logged in regardless of which instance serves the next request.
- **Pub/sub and real-time** — SignalR scale-out uses Redis to broadcast hub messages across all web nodes without sticky sessions.

---

## Chapter 2 — Caching Patterns

---

## Q6. What is the cache-aside (lazy loading) pattern, and walk through the read and write flow?

What is the cache-aside (lazy loading) pattern, and walk through the read and write flow?

**Answer:** Cache-aside means the application code owns the cache explicitly: on a read it checks the cache first and loads from the authoritative store only on a miss, then writes the result into the cache; on a write it updates the database first and then deletes or updates the cache entry. The cache is never the system of record — it is a performance layer the application manages around the real data source.

**Read flow:**

1. Application builds a cache key (for example `product:42`).
2. It calls `IDistributedCache.GetAsync(key)` — on a hit, deserialize and return.
3. On a miss, query the database or API for the data.
4. Serialize the result and call `SetAsync` with an appropriate expiration.
5. Return the data to the caller.

**Write flow:**

1. Persist the change to the database (the source of truth).
2. Remove the cache key (`RemoveAsync`) or overwrite it with the new value — deletion is safer when updates are partial or complex.
3. The next read repopulates the cache lazily.

- This is the default pattern with `IDistributedCache` because the interface provides no automatic database integration.
- Stale data appears if you update the database but forget to invalidate the cache — always pair writes with explicit invalidation or short TTL as a safety net.
- See Q11 for invalidation strategies when multiple keys depend on one entity.

---

## Q7. What is write-through caching, and when is it preferable to cache-aside?

What is write-through caching, and when is it preferable to cache-aside?

**Answer:** Write-through caching updates the cache and the authoritative store together on every write, typically inside a single coordinating layer, so the cache always holds the latest committed value after a write completes. It is preferable when read-after-write consistency matters and you can afford the extra write latency on every update, because readers never see stale cached data after a successful write.

- On a write, the coordinating code saves to the database and simultaneously `SetAsync` the new value in Redis before returning success to the caller.
- On a read, the application always consults the cache first and almost always hits, because writes kept the cache synchronized.
- Compared to cache-aside, write-through adds latency to every write (two destinations) but eliminates the "updated DB, forgot to invalidate cache" failure mode for that key.
- Use it for high-read, moderate-write reference data where stale reads are unacceptable and write volume is manageable; heavy write workloads may prefer cache-aside with invalidation or write-behind.
- `IDistributedCache` does not implement write-through automatically — you encode the dual-write sequence in your repository or service layer.

---

## Q8. What is write-behind (write-back) caching, and what consistency risks does it introduce?

What is write-behind (write-back) caching, and what consistency risks does it introduce?

**Answer:** Write-behind (write-back) caching acknowledges writes to the cache immediately and persists them to the authoritative database asynchronously in a batch or background flush, optimizing write throughput at the cost of durability and consistency guarantees. If the cache node fails before the flush completes, un persisted writes are lost, and readers on other instances may see data that has not yet reached the database.

- The application writes only to Redis on the request path and returns quickly; a background worker or timed flush pushes accumulated changes to SQL.
- This suits write-heavy telemetry, analytics counters, or non-critical preference updates where losing a few seconds of data on catastrophic cache failure is acceptable.
- Risk: another service reading directly from the database sees lagging data; another cache instance without the write also sees stale values unless all writes go through the same Redis key space.
- Risk: ordering and conflict resolution become hard when multiple keys map to related rows — most business transaction systems in ASP.NET Core avoid write-behind for authoritative entity state.
- Azure Cache for Redis is sometimes used with custom write-behind logic, but cache-aside or write-through remains the default for typical CRUD APIs.

---

## Q9. What is cache stampede (thundering herd), and how do you prevent it in a .NET application?

What is cache stampede (thundering herd), and how do you prevent it in a .NET application?

**Answer:** A cache stampede occurs when a popular cache entry expires (or is deleted) and many concurrent requests miss at once, each triggering an expensive reload of the same underlying data and overwhelming the database. Prevention strategies ensure only one caller rebuilds the entry while others wait or serve slightly stale data.

- **Single-flight (request coalescing)** — Use a per-key lock (`SemaphoreSlim`, Redis `SET NX` lock, or HybridCache's built-in coordination) so the first miss acquires the lock, reloads data, repopulates the cache, and releases; concurrent misses await the same task instead of each querying SQL.
- **Probabilistic early expiration** — Refresh hot keys slightly before hard TTL expiry so the entry is rarely absent at the same instant for all clients.
- **Stale-while-revalidate** — Return the expired cached value immediately while one background task refreshes it, trading brief staleness for stable database load.
- **Jitter on TTL** — Add random seconds to expiration so thousands of keys created in the same deployment wave do not expire in the same second.
- In .NET 9+, `HybridCache` combines L1/L2 caching with stampede protection; with raw `IDistributedCache`, implement single-flight manually around your cache-aside helper.

---

## Q10. When should you avoid caching data, and what types of data are poor cache candidates?

When should you avoid caching data, and what types of data are poor cache candidates?

**Answer:** Avoid caching when data must always reflect the latest authoritative state, when entries are unique per request and will never be read again, or when the cost of serialization and network round trips exceeds the cost of fetching the source directly. Poor candidates include highly volatile financial balances, one-off search results, large binary blobs that exceed Redis value limits, and sensitive secrets that should not sit in a shared memory store.

- **Strong consistency requirements** — Real-time inventory for high-contention purchases or fraud checks where a stale cache causes overselling or incorrect authorization decisions.
- **Low reuse** — Report exports keyed by arbitrary user filters where each key is accessed once; caching adds overhead without hit rate benefit.
- **Large payloads** — Multi-megabyte files or graph responses; Redis works best with small values (guidance: keep entries well under 1 MB; Azure documents a maximum value size of 512 MB but large values hurt performance).
- **Personal sensitive data** — Unless encrypted and TTL-bound, caching PII in a shared Redis instance expands the compliance surface.
- **Write-heavy counters with exact accuracy** — Prefer direct database updates or careful use of Redis atomic ops with explicit durability requirements understood.
- When in doubt, measure hit rate and latency improvement; a cache with under 30% hits may not justify complexity.

---

## Q11. How do you handle cache invalidation when the authoritative source (database or API) changes?

How do you handle cache invalidation when the authoritative source (database or API) changes?

**Answer:** Cache invalidation means removing or updating cache entries when the underlying data changes so readers never rely on stale values indefinitely. The application that performs the write is responsible for invalidating affected keys — there is no automatic link between Entity Framework Core saves and Redis unless you build it.

- **Key-per-entity deletion** — After `SaveChanges`, call `RemoveAsync` for predictable keys such as `product:{id}`; simple and precise when the mapping is one-to-one.
- **Tag or prefix invalidation** — Maintain a set of keys related to a category (for example all keys tagged `category:5`) and delete the set when the category changes; Redis sets or a custom index support this.
- **Pub/sub broadcast** — On update, publish an invalidation message; all app instances subscribe and drop matching entries from local `IMemoryCache` and/or Redis.
- **TTL as safety net** — Even with explicit invalidation, set reasonable expiration so orphaned keys self-heal after missed invalidation bugs.
- **Versioned keys** — Include a version number in the key (`product:42:v3`) and bump the version on schema or bulk refresh instead of hunting every old key.
- Invalidation granularity is a design choice: coarse invalidation (flush entire prefix) is simpler but causes more reload traffic; fine-grained is precise but easy to miss a dependent key.

---

## Chapter 3 — ASP.NET Core Distributed Cache Integration

---

## Q12. How do you register Azure Cache for Redis as `IDistributedCache` in ASP.NET Core, and where does the connection string come from?

How do you register Azure Cache for Redis as `IDistributedCache` in ASP.NET Core, and where does the connection string come from?

**Answer:** Install the `Microsoft.Extensions.Caching.StackExchangeRedis` package, add the connection string to configuration (often from Azure Portal or Azure Key Vault), and call `AddStackExchangeRedisCache` in `Program.cs` so the container resolves `IDistributedCache` backed by your Azure instance. The connection string typically uses the format `yourcache.redis.cache.windows.net:6380,password=...,ssl=True,abortConnect=False`.

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApp:"; // optional key prefix
});
```

- Create the Azure Cache for Redis resource in the portal, copy the primary connection string from **Access keys**, and store it in `appsettings.json`, environment variables, or Key Vault — never commit secrets to source control.
- `InstanceName` prefixes every key the adapter writes, which helps when multiple applications share one Redis instance for cost savings.
- `abortConnect=False` lets the app start even if Redis is temporarily unreachable; operations throw at call time instead of failing startup — pair with health checks for readiness.
- Inject `IDistributedCache` into services or minimal API handlers; the abstraction keeps unit tests on an in-memory `MemoryDistributedCache` substitute.

---

## Q13. What is the difference between `AddStackExchangeRedisCache` and using `ConnectionMultiplexer` from StackExchange.Redis directly?

What is the difference between `AddStackExchangeRedisCache` and using `ConnectionMultiplexer` from StackExchange.Redis directly?

**Answer:** `AddStackExchangeRedisCache` registers an `IDistributedCache` adapter that stores serialized byte arrays under string keys using a simplified API, while `ConnectionMultiplexer` is the low-level StackExchange.Redis client that exposes the full Redis command surface — hashes, lists, pub/sub, Lua scripts, and atomic operations. Use `IDistributedCache` for standard get/set/remove caching; register `ConnectionMultiplexer` as a singleton when you need Redis features beyond key-value strings.

| | `AddStackExchangeRedisCache` | `ConnectionMultiplexer` |
|---|---|---|
| Interface | `IDistributedCache` | `IConnectionMultiplexer` / `IDatabase` |
| API surface | Get, Set, Remove, async variants | Full Redis commands |
| Serialization | Your code serializes to `byte[]` | You choose per operation |
| Use case | Typical cache-aside | Locks, counters, pub/sub, complex structures |

- Both can share one connection string; register `ConnectionMultiplexer.Connect(...)` as singleton separately if you need both in the same app.
- `IDistributedCache` is easier to mock and swap for `MemoryDistributedCache` in tests; direct multiplexer calls couple tests to Redis or require wrapping interfaces.
- SignalR's Redis backplane uses its own StackExchange.Redis integration, not `IDistributedCache`.

---

## Q14. How does `IDistributedCache` serialize values, and what constraints does that place on the objects you store?

How does `IDistributedCache` serialize values, and what constraints does that place on the objects you store?

**Answer:** `IDistributedCache` stores only `byte[]` — it does not serialize .NET objects for you. Your application must convert objects to bytes before `SetAsync` and deserialize after `GetAsync`, which means you choose the format (JSON, MessagePack, or Protocol Buffers) and handle version compatibility yourself.

- `SetAsync(key, bytes, options)` and `GetAsync(key)` operate on raw bytes; there is no generic `Set<T>` on the interface.
- JSON via `System.Text.Json` is the most common choice in modern ASP.NET Core because it is human-readable in Redis CLI debugging and aligns with API payloads.
- Cached types should be data transfer objects (DTOs) — not `DbContext`, `HttpClient`, or types holding unmanaged resources — because you are persisting a snapshot, not live object graphs.
- Schema changes break deserialization unless you version keys or use tolerant JSON options; deploy new key prefixes when changing shape.
- `MemoryDistributedCache` (used in development or tests) follows the same byte contract, so serialization helpers transfer between providers.

```csharp
var json = JsonSerializer.Serialize(product);
await cache.SetAsync(key, Encoding.UTF8.GetBytes(json), options);
```

---

## Q15. How do you configure absolute and sliding expiration with `DistributedCacheEntryOptions`?

How do you configure absolute and sliding expiration with `DistributedCacheEntryOptions`?

**Answer:** Pass `DistributedCacheEntryOptions` to `SetAsync` to control how long an entry lives in Redis: absolute expiration sets a fixed deadline from the moment of write, sliding expiration resets the countdown on every read, and you can combine both so activity extends lifetime up to a hard maximum.

```csharp
var options = new DistributedCacheEntryOptions()
    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

await cache.SetAsync(key, bytes, options);
```

- **Absolute expiration** — Entry dies at `UtcNow + 30 minutes` regardless of read frequency; good for data with a known freshness window (daily exchange rates).
- **Sliding expiration** — Each `GetAsync` extends life by another 10 minutes; ideal for session-like data accessed repeatedly but abandoned keys eventually drop off.
- **Combined** — Sliding keeps hot keys warm, absolute caps maximum staleness even under constant access.
- Redis implements TTL internally; when both are set, the adapter applies the stricter effective policy per Microsoft's implementation — verify behavior for your package version in docs if precise semantics matter.
- No expiration means the key persists until manual removal or memory eviction under Redis `maxmemory` policy — always set TTL intentionally in production.

---

## Q16. How does distributed caching differ from ASP.NET Core output caching and response caching middleware?

How does distributed caching differ from ASP.NET Core output caching and response caching middleware?

**Answer:** Distributed caching (`IDistributedCache`) is a general-purpose key-value store your application code reads and writes explicitly, while output caching (`.NET 7+`) and response caching middleware cache entire HTTP responses at the framework level based on headers, routes, and policies. They solve different layers of the stack and can complement each other when configured carefully.

| | `IDistributedCache` | Output caching | Response caching middleware |
|---|---|---|---|
| Granularity | Arbitrary keys you define | Full HTTP response by policy | Full HTTP response by cache headers |
| Control | Manual in code | Attributes / policies | `[ResponseCache]`, headers |
| Storage | Redis, SQL, memory | In-memory or Redis (with config) | In-memory on the server |
| Typical use | Database query results | GET API endpoints | Static-like GET with headers |

- Output caching stores status code, headers, and body after the pipeline runs — you decorate endpoints with `[OutputCache]` or policies with tags and durations.
- Response caching middleware honors `Cache-Control` and `ETag` headers and caches on the handling instance unless extended — less flexible than output caching in modern apps.
- Use `IDistributedCache` when only part of the work is expensive (a database call inside the handler); use output caching when the entire GET response is identical for many clients.
- Authenticated personalized responses usually should not use full-response caching unless policies vary by user or role explicitly.

---

## Chapter 4 — StackExchange.Redis

---

## Q17. What is `ConnectionMultiplexer` in StackExchange.Redis, and why should you register it as a singleton?

What is `ConnectionMultiplexer` in StackExchange.Redis, and why should you register it as a singleton?

**Answer:** `ConnectionMultiplexer` is the long-lived client object in StackExchange.Redis that manages a shared connection pool, multiplexes many concurrent operations over a small number of physical connections to Redis, and handles automatic reconnects. You register one instance as a singleton for the application lifetime because creating a multiplexer per request is expensive and defeats connection reuse.

```csharp
builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));

// inject IDatabase per operation
var db = multiplexer.GetDatabase();
await db.StringGetAsync("product:42");
```

- The library is designed around one multiplexer per Redis cluster or standalone server per process — thread-safe and meant to be shared across all requests.
- `GetDatabase()` returns a lightweight `IDatabase` facade (default database 0); call it per operation rather than caching `IDatabase` as singleton if you need different database numbers.
- On connection blips, the multiplexer retries and buffers short outages; `abortConnect=False` in the connection string allows startup before Redis is reachable.
- Dispose the multiplexer on application shutdown (`IHostApplicationLifetime`) to close connections cleanly.

---

## Q18. Beyond simple string key-value pairs, what Redis data structures does StackExchange.Redis expose, and when would you use hashes, sorted sets, or lists?

Beyond simple string key-value pairs, what Redis data structures does StackExchange.Redis expose, and when would you use hashes, sorted sets, or lists?

**Answer:** StackExchange.Redis maps Redis's native types to API methods on `IDatabase`: strings for simple values, hashes for object fields, lists for queues, sets for unique collections, and sorted sets for ranked data — each with atomic server-side operations. Use the structure that matches access patterns instead of serializing everything as JSON strings when partial reads or atomic updates matter.

- **Strings** — `StringGet` / `StringSet` for cached JSON blobs, counters (`StringIncrement`), and distributed lock tokens; simplest cache-aside storage.
- **Hashes** — `HashSet` / `HashGetAll` store field-value maps (for example user profile fields) so you can update one field without rewriting the entire object.
- **Lists** — `ListLeftPush` / `ListRightPop` implement queues and recent-history buffers with FIFO ordering.
- **Sets** — `SetAdd` / `SetMembers` track unique tags, online user IDs, or invalidation tag membership without duplicates.
- **Sorted sets** — `SortedSetAdd` with scores supports leaderboards, priority queues, and time-window rate limits ordered by timestamp score.
- Complex structures reduce serialization overhead and enable atomic increments, but increase code complexity — many ASP.NET Core apps stay with string JSON until a specific pattern requires more.

---

## Q19. How do you implement a distributed lock with Redis, and what pitfalls exist with naive lock implementations?

How do you implement a distributed lock with Redis, and what pitfalls exist with naive lock implementations?

**Answer:** A distributed lock uses Redis to coordinate so only one process across all instances performs a critical section at a time, typically with `SET key unique-token NX EX seconds` so the key is set only if absent and auto-expires to prevent deadlocks. Naive implementations fail if you use `GET` then `SET` without atomicity, if you release another process's lock, or if work outlives the TTL and two workers both believe they hold the lock.

- **Acquire** — `bool acquired = await db.StringSetAsync(lockKey, lockToken, expiry: TimeSpan.FromSeconds(30), when: When.NotExists);`
- **Release** — Use a Lua script that deletes the key only if the value still equals your unique token, so you never remove a lock re acquired by another instance after yours expired.
- **Extend** — Long-running work may renew TTL with a watchdog loop if the same owner still holds the token.
- Pitfall: no `NX` — two clients both see an empty key and both proceed.
- Pitfall: fixed TTL too short — job runs past expiry and a second worker enters; use Redlock algorithm or dedicated libraries (Medallion.Threading.Redis) for higher correctness requirements.
- Redis locks suit cache rebuild single-flight or short cron coordination — not a full replacement for database transactional locks on financial invariants.

---

## Q20. What is Redis pub/sub, and how is it used in .NET (for example SignalR backplane or event fan-out)?

What is Redis pub/sub, and how is it used in .NET (for example SignalR backplane or event fan-out)?

**Answer:** Redis pub/sub is a fire-and-forget messaging channel where publishers send messages to a named channel and all subscribers currently connected receive a copy, with no persistence for offline consumers. In .NET, SignalR uses it as a backplane so hub messages sent on one web instance reach clients connected to other instances, and custom services use `ISubscriber` for lightweight event fan-out.

- Publish: `await subscriber.PublishAsync(RedisChannel.Literal("orders"), payload);`
- Subscribe: `subscriber.Subscribe(RedisChannel.Literal("orders"), (channel, message) => { ... });`
- SignalR registers with `AddStackExchangeRedis` and a `ChannelPrefix` so only your app's messages share the bus.
- Pub/sub does not store messages — subscribers offline at publish time miss the event; use Azure Service Bus or Redis Streams when you need durable messaging.
- Suitable for cache invalidation broadcasts, live notifications, and cross-node SignalR — not for guaranteed delivery workflows.

---

## Q21. How should a production ASP.NET Core app handle Redis connection failures, timeouts, and transient errors?

How should a production ASP.NET Core app handle Redis connection failures, timeouts, and transient errors?

**Answer:** Treat Redis as a best-effort acceleration layer unless you explicitly design for hard dependency: catch timeouts on cache operations, fall back to the authoritative data source on failure, and expose health checks so orchestrators know when the app is degraded. Configure resilient connections at startup and log failures without letting cache errors break core business paths.

- Use `abortConnect=False` so the app starts without Redis; wrap `GetAsync`/`SetAsync` in try/catch and on failure proceed as a cache miss to the database.
- Register a health check (`AddRedis` from `AspNetCore.HealthChecks.Redis`) on readiness endpoints — return Degraded instead of Unhealthy if you can operate without cache.
- Set reasonable timeouts in the connection string (`connectTimeout`, `syncTimeout`) so hung Redis does not block request threads indefinitely.
- Monitor Azure metrics: connected clients, server load, cache hits/misses (application-level), evicted keys, and errors.
- For critical coordination (locks, idempotency), explicit failure handling is required — do not silently swallow errors that could cause duplicate processing.
- Circuit breaker patterns (Polly) around Redis calls prevent hammering a failing instance and speed recovery to fallback paths.

---

## Chapter 5 — TTL, Tiers, Session State & Operations

---

## Q22. What is TTL (time-to-live) in Redis, and how do you choose expiration values for different data types?

What is TTL (time-to-live) in Redis, and how do you choose expiration values for different data types?

**Answer:** TTL is the automatic expiration Redis applies to a key after a set duration, expressed in seconds (or milliseconds) via `EXPIRE` or through `SetAsync` options in .NET. Choosing TTL balances freshness against hit rate: volatile data gets short TTL, stable reference data gets longer TTL, and session data aligns with authentication timeout policy.

- **Hot reference data** (product catalog) — 5–60 minutes with explicit invalidation on admin updates; longer if change frequency is low and brief staleness is acceptable.
- **Computed aggregates** (dashboard totals) — 1–5 minutes or rebuild on a schedule; match business tolerance stated in service-level objectives.
- **User session** — Match cookie timeout and security policy (20–60 minutes sliding is common).
- **Rate-limit windows** — Exact window length (for example 60 seconds for 100 requests per minute).
- **Anti-stampede jitter** — Add random offset (±10%) to mass-created keys so they do not expire simultaneously.
- No TTL is appropriate only for true configuration keys maintained by deliberate updates — monitor memory when keys never expire.

---

## Q23. What are the Azure Cache for Redis tiers (Basic, Standard, Premium, and Enterprise), and when would you choose each?

What are the Azure Cache for Redis tiers (Basic, Standard, Premium, and Enterprise), and when would you choose each?

**Answer:** Azure Cache for Redis tiers trade off high availability, throughput, persistence, networking, and cost: Basic is a single node for development, Standard adds replication, Premium adds clustering and advanced features, and Enterprise adds Redis Enterprise modules and very large memory footprints. Choose based on production requirements for uptime, data size, and compliance — not peak development convenience.

| Tier | Nodes | Replication | Typical use |
|---|---|---|---|
| Basic | 1 | None | Dev/test only — no SLA for production |
| Standard | 2 (primary + replica) | Yes | Production apps needing failover |
| Premium | 2+ with optional cluster | Yes + clustering | Large memory, VNet, persistence, geo-replication |
| Enterprise | Redis Enterprise | Advanced HA | Modules (RedisJSON, RediSearch), extreme scale |

- Basic has no service-level agreement and no replication — a node restart loses in-memory data and causes downtime.
- Standard provides automatic failover when the primary fails — the minimum for production web apps using session or shared cache.
- Premium supports shard clustering for larger datasets, data persistence (RDB/AOF options), deployment into a virtual network, and geo-replication for disaster recovery.
- Enterprise targets large enterprises needing Redis Enterprise modules or multi-terabyte scenarios — higher cost, specialized features.

---

## Q24. What high-availability and scaling options does Azure Cache for Redis offer (replication, clustering, geo-replication)?

What high-availability and scaling options does Azure Cache for Redis offer (replication, clustering, geo-replication)?

**Answer:** Standard tier and above replicate data to a secondary node with automatic failover, Premium adds Redis cluster sharding to scale memory and throughput horizontally, and Premium geo-replication links two regional caches for disaster recovery with manual failover between regions. Vertical scaling changes the cache size tier; clustering partitions keys across shards on Premium.

- **Replication (Standard+)** — Primary handles traffic; replica stays synchronized; Azure promotes replica on primary failure with minimal downtime.
- **Clustering (Premium)** — Data partitioned across up to 10 shards; client libraries like StackExchange.Redis discover topology automatically when clustering is enabled.
- **Geo-replication (Premium)** — Secondary region holds a linked cache; useful for read preference or DR drills — not transparent active-active for all write patterns without application design.
- **Zone redundancy** — Deploy across availability zones within a region for resiliency against datacenter failures (supported configurations per SKU).
- Scale operations may cause brief connection blips — use resilient multiplexer settings and retry logic during maintenance windows.

---

## Q25. How do you configure ASP.NET Core session state to use Redis as the distributed session store?

How do you configure ASP.NET Core session state to use Redis as the distributed session store?

**Answer:** Install `Microsoft.Extensions.Caching.StackExchangeRedis`, register distributed cache with the Redis connection string, then call `AddSession` and `AddStackExchangeRedisCache` (or `AddDistributedRedisCache` in older templates) and middleware `UseSession` so session payloads serialize to Redis instead of an in-memory cookie store. The session cookie on the client holds only the session ID — the bulk data lives in Redis.

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

// after routing, before endpoints
app.UseSession();
```

- `HttpContext.Session.SetString` / `SetInt32` store serializable values; complex objects require manual serialization to bytes or JSON strings.
- All instances must share the same Redis configuration and `InstanceName` prefix or sessions appear empty after load balancing routes to another node.
- Session in Redis removes the need for sticky sessions on the load balancer for session affinity.
- Protect the connection string and enable TLS; session data may contain sensitive user context.

---

## Q26. What security features does Azure Cache for Redis provide (TLS, access keys, firewall, Microsoft Entra ID authentication)?

What security features does Azure Cache for Redis provide (TLS, access keys, firewall, Microsoft Entra ID authentication)?

**Answer:** Azure Cache for Redis encrypts data in transit with TLS (port 6380), authenticates clients with access keys or Microsoft Entra ID (Azure Active Directory) on supported tiers, and restricts network reach with firewall rules and private endpoints inside a virtual network. Production deployments disable non-TLS ports, rotate keys through Key Vault, and limit IP ranges or VNet integration to application subnets only.

- **TLS** — Connect with `ssl=True` in the connection string; Azure rejects plain-text on 6380 for current caches.
- **Access keys** — Primary and secondary keys allow rotation without dual downtime — regenerate secondary, deploy apps, then regenerate primary.
- **Firewall** — Allow only Azure App Service outbound IPs or specific corporate ranges; default deny for public endpoints.
- **Private Endpoint / VNet injection (Premium)** — Redis has no public IP; traffic stays on the Azure backbone from your app subnet.
- **Microsoft Entra authentication** — Eliminates shared secrets in connection strings where supported; apps acquire tokens via managed identity — preferred for zero-secret rotation in Azure-hosted ASP.NET Core apps.
- Redis is not encrypted at rest on all SKUs the same way — check current Azure documentation for your tier; sensitive payloads can be encrypted application-side before `SetAsync`.

---

## Q27. What monitoring, alerting, and troubleshooting practices apply to Azure Cache for Redis in production?

What monitoring, alerting, and troubleshooting practices apply to Azure Cache for Redis in production?

**Answer:** Monitor server health through Azure Monitor metrics and diagnostics, track application-level hit and miss rates in your own logging, alert on connection errors and memory pressure, and troubleshoot latency by correlating Redis slowlog with expensive commands. Treat sudden miss-rate spikes and `used_memory` approaching `maxmemory` as early warnings before evictions or timeouts degrade the app.

- **Azure metrics** — `connectedclients`, `cachehits` / `cachemisses` (Redis internal), `usedmemory`, `evictedkeys`, `operationsPerSecond`, `serverLoad`, replication lag.
- **Application instrumentation** — Log cache operation duration, fallback-to-database counts, and serialization size; use Application Insights dependencies for Redis calls when using direct StackExchange.Redis.
- **Alerts** — Memory above 80%, evictions > 0 sustained, connection count drops during steady traffic, health check failures on `/health/ready`.
- **Common issues** — Large values causing high latency; `KEYS *` in production blocking the server (use `SCAN`); missing TTL filling memory; wrong `InstanceName` prefix causing apparent misses; firewall blocking App Service outbound IPs after scale change.
- **Load testing** — Validate failover behavior on Standard/Premium during a planned maintenance window before production dependence.
- Azure Portal **Reboot** and **Scale** operations disconnect clients momentarily — ensure multiplexer reconnect logic is active during infrastructure changes.

---
