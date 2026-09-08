# 09. Loading Related Data — Interview Q&A
> Back to [Entity Framework Core Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Q59. What is the difference between eager loading, lazy loading, and explicit loading?](#q59-what-is-the-difference-between-eager-loading-lazy-loading-and-explicit-loading)
- [Q60. How do you use `Include` and `ThenInclude` for eager loading?](#q60-how-do-you-use-include-and-theninclude-for-eager-loading)
- [Q61. What is the N+1 problem in the context of loading related data?](#q61-what-is-the-n1-problem-in-the-context-of-loading-related-data)
- [Q62. Why should you avoid lazy loading in ASP.NET Core applications?](#q62-why-should-you-avoid-lazy-loading-in-aspnet-core-applications)
- [Q63. What is a cartesian explosion when including multiple collections?](#q63-what-is-a-cartesian-explosion-when-including-multiple-collections)
- [Q64. What is `AsSplitQuery`, and when should you use it?](#q64-what-is-assplitquery-and-when-should-you-use-it)
- [Q65. How do you choose between eager loading, explicit loading, and projection?](#q65-how-do-you-choose-between-eager-loading-explicit-loading-and-projection)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 09. Loading Related Data

---

## Q59. What is the difference between eager loading, lazy loading, and explicit loading?

**Concepts**
- Include for up-front eager loading
- lazy loading proxy with disposal risk
- explicit Load on demand
- predictable vs implicit query count trade-off

**Answer**

Eager loading fetches related data in the same query (or coordinated queries) up front using `Include`. Lazy loading fetches related data automatically when a navigation property is first accessed. Explicit loading runs a separate query on demand via `Entry(...).Collection(...).Load()` or `Reference(...).Load()`. Eager: `context.Orders.Include(o => o.Lines)` — related data available immediately, no extra queries on access; Lazy: requires proxies or lazy-loading proxies package; triggers SQL on first navigation access — risky after context disposal. Explicit: start with a stub entity or partial load, then call `Load()` when you know you need the related data. Eager and explicit give predictable query counts; lazy makes query count depend on code paths and property access order.

---

## Q60. How do you use `Include` and `ThenInclude` for eager loading?

**Concepts**
- single-level Include for direct navigation
- ThenInclude for deeper navigation paths
- multiple collection branches via repeated Include
- filtered Include in EF Core 5+

**Answer**

Chain `Include` on the root `DbSet` query to load related entities, and use `ThenInclude` to load deeper levels along a navigation path. EF Core translates includes into SQL JOINs or split queries depending on configuration. One level: `context.Orders.Include(o => o.Customer)`; Deeper: `context.Orders.Include(o => o.Lines).ThenInclude(l => l.Product)`. Multiple branches: repeat `Include` from the root — `Include(o => o.Customer).Include(o => o.Lines)`. Filtered includes (EF Core 5+): `.Include(o => o.Lines.Where(l => l.Active))` loads only matching related rows.

---

## Q61. What is the N+1 problem in the context of loading related data?

**Concepts**
- lazy loading and missing Include as root causes
- per-parent additional query multiplication
- APM and logging-based detection
- Include and projection as fixes

**Answer**

When listing parent entities without loading related navigations, each access to a child collection or reference in a loop fires a separate SQL query — one initial query plus N per-row queries. This is the relational-data manifestation of the N+1 anti-pattern. Occurs with lazy loading enabled or when developers forget `Include` on list endpoints; A 50-row list with one navigation access per row becomes 51 database round-trips per HTTP request. Detect via EF Core command logging or APM tools showing repeated identical queries with different keys. Resolve with `Include`, `AsSplitQuery`, projection, or batch explicit loading before the loop.

---

## Q62. Why should you avoid lazy loading in ASP.NET Core applications?

**Concepts**
- disposed DbContext and serializer trigger
- implicit query count on navigation access
- explicit Include for predictable contract
- desktop vs web context lifetime difference

**Answer**

Lazy loading triggers database queries during navigation property access, which often happens during JSON serialization or view rendering after the request-scoped `DbContext` is disposed or without the developer's explicit awareness. ASP.NET Core's stateless request model makes implicit loading unpredictable and expensive. Serializers accessing navigations cause "Cannot access a disposed context" or hidden N+1 query storms; Query count becomes data-dependent — endpoints perform differently based on which properties the client touches. Explicit `Include` or projection makes API contracts and performance predictable and testable. Lazy loading suits long-lived desktop contexts with UI-driven access patterns, not short HTTP request scopes.

---

## Q63. What is a cartesian explosion when including multiple collections?

**Concepts**
- JOIN row multiplication from multiple collections
- EF Core fix-up deduplication in memory
- network and memory cost of inflated rowset
- AsSplitQuery as primary mitigation

**Answer**

Cartesian explosion happens when a single SQL query JOINs two or more collection navigations, producing a rowset whose size is roughly the product of collection cardinalities — many duplicate parent rows over the wire before EF Core deduplicates in memory. Example: `Include(o => o.Lines).Include(o => o.Shipments)` on orders with 20 lines and 10 shipments can emit ~200 rows per order in SQL; Network and memory costs spike even though the final object graph has far fewer unique entities. EF Core fix-up reconstructs parents correctly, but the damage is already done at the SQL transport layer. Mitigate with `AsSplitQuery()`, separate queries, or projection to DTOs that avoid multi-collection joins.

---

## Q64. What is `AsSplitQuery`, and when should you use it?

**Concepts**
- separate SELECT per collection Include
- cartesian explosion avoidance
- slightly more round-trips than single JOIN
- global QuerySplittingBehavior setting

**Answer**

`AsSplitQuery()` tells EF Core to load included related data using multiple SQL queries instead of one large JOIN, avoiding cartesian explosion when including multiple collection navigations. Each include path gets its own SELECT while EF Core still assembles the object graph. Use when eager-loading two or more `Include` collection branches on the same root entity; Slightly more round-trips than a single join but far less data transferred when collections are large. Can be set globally via `UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)` or per query with `.AsSplitQuery()`. Single-reference includes (many-to-one) rarely need split queries — the problem is primarily multi-collection joins.

---

## Q65. How do you choose between eager loading, explicit loading, and projection?

**Concepts**
- Include for known up-front graphs
- explicit Load for conditional related data
- projection for read-only flat shapes
- lazy loading exclusion in web APIs

**Answer**

Choose based on how much related data you know you need at query time and whether the operation is read-only or tracked. Eager loading suits known graphs up front; explicit loading suits conditional related data; projection suits read-only API responses that need flat or partial shapes. Eager (`Include`): list/detail endpoints where the response always needs the same related entities; Explicit (`Load`): related data needed only in some branches — load when a flag or business rule triggers the need. Projection (`Select` to DTO): read-only APIs that never update entities — smallest payload, no tracking, no N+1. Avoid lazy loading in web APIs; combine split queries when eager-loading multiple collections.

---

## Gotchas — Loading Related Data (Interview Traps)

---

#### Gotcha 1. Lazy loading enabled — hidden N+1 per navigation property access in loop

**Concepts**
- lazy loading proxy fires SQL on every navigation property access
- `UseLazyLoadingProxies()` + `virtual` properties enable lazy loading
- N parent entities → N separate queries for each child navigation
- EF Core 3+ requires explicit opt-in for lazy loading
- `LogTo(Console.WriteLine)` reveals hidden lazy queries in development

**Answer**

With lazy loading enabled, accessing a navigation property on a tracked entity that is not yet loaded fires a hidden SQL query. In a loop over 100 orders, `order.Customer.Name` fires 100 separate `SELECT` queries — one per order — for a total of 101 database roundtrips. This is invisible in source code unless EF Core command logging is active. Disable lazy loading by default and use `Include` for known navigations. If lazy loading is necessary, always profile query counts with `LogTo` in development before shipping to production.

---

#### Gotcha 2. Missing `ThenInclude` on nested navigation — inner navigation is null

**Concepts**
- `Include(o => o.Lines)` loads direct child navigation only
- `ThenInclude(l => l.Product)` required for grandchild navigation
- `o.Lines.First().Product` is null without `ThenInclude`
- chain as many `ThenInclude` levels as needed
- verify loaded graph with logging before relying on deeply nested properties

**Answer**

`Include(o => o.Lines)` loads the `Lines` collection of each `Order`, but `Lines[0].Product` remains null — loading a child's child requires `.ThenInclude(l => l.Product)` chained after `Include`. Each navigation level requires its own `Include` or `ThenInclude` call. Accessing a deeply nested navigation that was not included does not throw — the property is simply null, producing silent bugs. Always verify the full graph with `ToQueryString()` or logging, and add every `ThenInclude` level required by the operation.

---

#### Gotcha 3. Cartesian explosion from multiple collection `Include` paths

**Concepts**
- two collection `Include` paths produce Cartesian JOIN
- 10 OrderLines × 5 Tags = 50 rows per order in result set
- EF Core deduplicates in memory but network already transmitted inflated rows
- `AsSplitQuery()` uses separate SELECT per collection — no Cartesian product
- split query trade-off: no atomic snapshot, extra round-trips

**Answer**

When two collection navigation properties are both eager-loaded in one query (`Include(o => o.Lines).Include(o => o.Tags)`), SQL Server returns a Cartesian JOIN — each line is repeated for every tag, multiplying the row count by the product of the collection sizes. EF Core deduplicates parent entities during fix-up, but the inflated rowset was already transmitted over the network. Use `AsSplitQuery()` to fetch each collection in a separate SQL query, reducing network overhead at the cost of the queries no longer being a single atomic snapshot.

---

#### Gotcha 4. Lazy loading after `DbContext` disposal — serializer triggers hidden queries post-scope

**Concepts**
- lazy loading proxy accesses navigation when property touched after context disposal
- JSON serializer touches all public properties including navigations
- `ObjectDisposedException` thrown mid-serialization
- `Include` required navigations before context scope ends
- DTO projection eliminates navigation property serialization entirely

**Answer**

If an entity with unloaded navigation properties is returned from a method where the `DbContext` has already been disposed, the JSON serializer touches the navigation property and triggers a lazy load — but the context is gone, causing `ObjectDisposedException`. This is intermittent because it depends on when the serializer accesses the property relative to context disposal. Load all required navigations with `Include` inside the request handler, or project to a DTO that contains no navigation properties, eliminating the problem entirely.

---

#### Gotcha 5. Explicit loading (`LoadAsync`) called after context disposed — `ObjectDisposedException`

**Concepts**
- `context.Entry(entity).Collection(e => e.Lines).LoadAsync()` for explicit loading
- explicit load must be called while context is still alive
- calling outside `using` scope after context disposal throws
- explicit loading appropriate only for conditional, branch-specific loading
- eager loading with `Include` as the simpler alternative

**Answer**

Explicit loading via `context.Entry(entity).Collection(e => e.Lines).LoadAsync()` must be called while the `DbContext` is still open and in scope. Calling it after the context is disposed throws `ObjectDisposedException`. Explicit loading is appropriate for conditional scenarios where only some requests need the related data, but it requires the context to remain alive for the full duration of the conditional load path. For simpler patterns, use `Include` in the initial query — loading navigation properties that are not needed for some requests is a minor efficiency cost compared to the lifetime management complexity of explicit loading.

---

#### Gotcha 6. `AsSplitQuery` no longer provides atomic snapshot — race condition in high-concurrency writes

**Concepts**
- `AsSplitQuery` issues separate SELECT statements per collection
- concurrent write between the first and second SELECT changes data
- first SELECT returns entity state A; second SELECT returns state B
- single-query `Include` provides one consistent snapshot
- `AsSplitQuery` appropriate for read-heavy, low-write scenarios

**Answer**

`AsSplitQuery()` issues separate SQL queries for each collection navigation — the parent entities are loaded in the first query, and each collection is loaded in a subsequent query. If a concurrent write occurs between these queries, the split result may be inconsistent: the parent row reflects state before the write while the child collection reflects state after (or vice versa). For write-heavy entities where snapshot consistency is critical (financial operations, audit trails), use the default single-query approach with `Include` and accept the potential Cartesian join overhead.

---

#### Gotcha 7. Filtered `Include` (EF Core 5+) predicate excludes needed related rows silently

**Concepts**
- `Include(o => o.Lines.Where(l => !l.IsCancelled))` as filtered include
- predicate excludes matching child rows — not an error, just missing data
- navigation collection appears to have fewer items than expected
- missing rows cause business logic to produce wrong totals or decisions
- always assert filtered include result in tests with controlled data

**Answer**

Filtered `Include` (EF Core 5+) allows `Include(o => o.Lines.Where(l => l.IsActive))` to load only matching child rows. If the predicate is wrong or too restrictive, the loaded collection silently excludes rows the calling code expects to be present — navigation properties have fewer items than actually exist in the database. Business logic that totals order amounts from the loaded collection produces incorrect results without any error. Always write integration tests with controlled seed data asserting that filtered collections return exactly the expected row count.

---

#### Gotcha 8. Lazy loading proxies require `virtual` navigation properties — missing `virtual` silently disables proxy

**Concepts**
- `UseLazyLoadingProxies()` generates runtime proxy subclasses
- proxy intercepts `virtual` property access to trigger lazy load
- non-`virtual` navigation property is not proxied — always null
- sealed entity class cannot be subclassed — proxy creation fails
- proxy requirement: class not sealed, constructor not private, properties `virtual`

**Answer**

`UseLazyLoadingProxies()` generates a proxy subclass at runtime that overrides navigation property getters to trigger lazy loads. If a navigation property is not marked `virtual`, the proxy cannot override it and the property is never loaded — it remains null. If the entity class is `sealed`, proxy generation fails entirely at startup. For lazy loading to function, every navigation property that should be lazily loaded must be `virtual`, the class must not be sealed, and the constructor must be accessible to the proxy generator.

---

#### Gotcha 9. Loading a large graph with `Include` when only a summary is needed

**Concepts**
- `Include(o => o.Lines).ThenInclude(l => l.Product)` loads full entities
- loading 50 columns per entity when only 3 are needed wastes bandwidth
- projection to DTO with only required columns as the efficient alternative
- `AsNoTracking()` omitted on full entity load adds change tracking overhead
- rule: if response does not update the entities, project, don't `Include`

**Answer**

Using `Include`/`ThenInclude` to load full entity graphs when the response only needs a few fields from each entity fetches far more data than required. Loading `Order` with full `Lines` and `Products` to display an order summary card (id, date, total, line count) transmits dozens of unused columns per row. Use `Select` projection to a DTO with only the needed fields: `context.Orders.Select(o => new OrderSummaryDto { Id = o.Id, Total = o.Lines.Sum(l => l.Price), LineCount = o.Lines.Count })`. This also eliminates change-tracking overhead since projections are never tracked.

---

#### Gotcha 10. Collection navigation not initialized before `Add()` call — `NullReferenceException`

**Concepts**
- reference navigation default is null when entity loaded without `Include`
- collection navigation EF-initialized as empty collection on entity materialization
- hand-constructed `new Order()` — collection navigation is null until initialized
- `order.Lines.Add(line)` on null `Lines` throws `NullReferenceException`
- initialize collection in entity constructor or property initializer

**Answer**

When you construct a new `Order` entity in code (not loaded from database), the collection navigation property `Lines` is null unless initialized in the constructor or via a property initializer. Calling `order.Lines.Add(new OrderLine())` on a null collection throws `NullReferenceException`. Initialize collection navigation properties in the entity class: `public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();`. EF Core initializes collections on materialized entities, but code that constructs entities with `new` must handle initialization explicitly.

---

## Scenario-Based Questions (Karat Format)

---

## Q144. (R) A list endpoint only needs order id, date, and customer name, but the repository loads the full graph before mapping. Review the method — what is the performance problem, and how do you fix it while keeping a single SQL round-trip?

```csharp
public async Task<IReadOnlyList<OrderListItemDto>> GetRecentOrdersAsync(CancellationToken ct)
{
    using ShopDbContext context = CreateContext();
    List<Order> orders = await context.Orders
        .AsNoTracking()
        .Include(o => o.Customer)
        .Include(o => o.Lines)
            .ThenInclude(l => l.Product)
        .OrderByDescending(o => o.OrderDate)
        .Take(50)
        .ToListAsync(ct);

    return orders
        .Select(o => new OrderListItemDto(o.OrderId, o.OrderDate, o.Customer!.Name))
        .ToList();
}
```

**Concepts**
- full graph load for list view needing two columns
- Select projection for DTO directly in query
- single SQL round-trip with projection
- change tracker exclusion on read-only response

**Answer**

The query over-fetches — it JOINs and materializes every `OrderLine` and `Product` row for 50 orders even though the DTO only needs scalar order fields and the customer name, wasting SQL I/O, network bandwidth, and heap memory before the mapping discards the graph.

1. Replace eager Include with a **server-side projection** — EF translates `Select` to SQL that returns only required columns in one command:

```csharp
return await context.Orders
    .AsNoTracking()
    .OrderByDescending(o => o.OrderDate)
    .Take(50)
    .Select(o => new OrderListItemDto(o.OrderId, o.OrderDate, o.Customer!.Name))
    .ToListAsync(ct);
```

2. If you must stay on entities temporarily, drop `.Include(o => o.Lines).ThenInclude(l => l.Product)` — keep only `.Include(o => o.Customer)` when customer name is needed and lines are not.
3. Add query logging (`LogTo` or `QueryDiagnostics` from this chapter) in staging to compare row counts before/after.

---

## Q145. (R) After eager-loading changes, the order-detail page shows line quantities but every `ProductName` is null. Review the query — what is wrong with the Include chain, and what SQL shape do you expect after the fix?

```csharp
public Order? GetOrderDetail(int orderId)
{
    using ShopDbContext context = CreateContext();
    return context.Orders
        .AsNoTracking()
        .Include(o => o.Customer)
        .Include(o => o.Lines)
        .FirstOrDefault(o => o.OrderId == orderId);
}
```

**Concepts**
- ThenInclude missing from second-level navigation
- null ProductName in order-detail response
- correct Include chain shape
- SQL JOIN shape verification

**Answer**

The query includes `Order.Lines` but never continues the chain with `ThenInclude(l => l.Product)`, so line entities materialize without their product navigation — `line.Product` stays null even though `Quantity` comes from the included line rows.

1. Extend the chain to match **OrderLoadingService.GetOrdersWithEagerLoading**:

```csharp
return context.Orders
    .AsNoTracking()
    .Include(o => o.Customer)
    .Include(o => o.Lines)
        .ThenInclude(l => l.Product)
    .FirstOrDefault(o => o.OrderId == orderId);
```

2. Each new `Include` from the root starts a **new branch** — after `.Include(o => o.Customer)`, the next `.Include(o => o.Lines)` is another root branch, and `.ThenInclude(l => l.Product)` attaches to the **Lines** branch, not Customer.
3. Expect SQL with JOINs from `Orders` → `OrderLines` → `Products` (plus `Customers`), or two statements if split query is enabled.

---

## Q146. (M) A reporting job loads customers with all their orders and every order line. Under load, SQL row counts explode and memory spikes, but the team insists "it's one query so it must be efficient." Review the query — explain the cartesian product, and when you would use `AsSplitQuery()`.

```csharp
public IList<Customer> GetCustomersWithOrderHistory()
{
    using ShopDbContext context = CreateContext();
    return context.Customers
        .AsNoTracking()
        .Include(c => c.Orders)
            .ThenInclude(o => o.Lines)
                .ThenInclude(l => l.Product)
        .OrderBy(c => c.CustomerId)
        .ToList();
}
```

**Concepts**
- cartesian product from nested customer-order-line Include
- single query seeming efficient but producing row explosion
- AsSplitQuery timing and trade-off
- memory spike under multi-collection load

**Answer**

A single SQL statement with JOINs across two collections (`Customer.Orders` and each order's `Lines`) produces a cartesian product — each customer row repeats for every combination of order × line in the result set, so EF must de-duplicate in memory and the database ships far more rows than entities returned. - Mechanism: One `Include` on a collection plus `ThenInclude` into another collection multiplies rows in the flat JOIN result. Ten orders with five lines each can yield ~50 joined rows per customer before deduplication. - Symptoms: Slow reports, high SQL `logical reads`, memory spikes, "one query" that transfers megabytes — worsens as order history grows. - `AsSplitQuery()`: EF issues multiple SQL commands (e.g., customers, then orders, then lines) that avoid the wide JOIN cartesian product while still building the same graph — usually 2–4 round-trips vs one giant result. - When to use: Multiple collection navigations or deep Include trees on reporting/read-only paths where a single JOIN would explode — as noted in OrderLoadingService.cs Section 4 preview. - Trade-off: Split queries add round-trips; on latency-sensitive small graphs a single JOIN may be fine — profile with command logging. Production takeaway: "One query" ≠ "one efficient query" — cartesian explosion is a classic EF production incident; split queries or projection are the fix. See QueryDiagnostics in this chapter to compare command count vs row volume.

---

## Q147. (R) A developer refactors N+1 line loading to "explicit loading" but production still shows hundreds of SQL commands per request. Review the method — identify every issue and prioritize fixes.

```csharp
public IList<OrderHeaderDto> GetOrderHeaders(IReadOnlyList<int> orderIds)
{
    using ShopDbContext context = CreateContext();
    List<Order> orders = context.Orders
        .AsNoTracking()
        .Where(o => orderIds.Contains(o.OrderId))
        .ToList();

    foreach (Order order in orders)
    {
        context.Entry(order).Reference(o => o.Customer).Load();
        context.Entry(order).Collection(o => o.Lines).Load();
    }

    return orders.Select(o => new OrderHeaderDto(
        o.OrderId,
        o.Customer!.Name,
        o.Lines.Count)).ToList();
}
```

**Concepts**
- explicit loading inside loop repeating N queries
- per-item Collection.LoadAsync call
- batch load alternative for multiple parents
- N+1 from explicit loading misapplication

**Answer**

Explicit loading never runs correctly here — `AsNoTracking()` detaches entities so `Entry(order).Load()` cannot attach and load navigations, and even if tracked, calling `Load()` twice per order in a loop reproduces an N+1 (1 + 2×N commands) instead of a batched Include or projection.

1. For bulk headers, use one query with Include or projection — not per-row Load:

```csharp
return await context.Orders
    .AsNoTracking()
    .Where(o => orderIds.Contains(o.OrderId))
    .Select(o => new OrderHeaderDto(o.OrderId, o.Customer!.Name, o.Lines.Count))
    .ToListAsync();
```

2. If explicit loading is required (conditional navigations after business logic), remove `AsNoTracking()` on the root query so the entity is **tracked**, and batch where possible — still prefer Include when you always need Customer + Lines.
3. Follow **OrderLoadingService.GetOrderWithExplicitLoading** — single tracked root, `Reference().Load()` and `Collection().Query().Include(...).Load()` when the context is open.

---

## Q148. (R) A fulfillment API should return only orders that have at least one line with `Quantity >= 2`, and each order should expose only those qualifying lines. The developer uses filtered Include but QA reports wrong orders in the response. Review the code — what is misunderstood about filtered Include, and how do you fix the business rule?

```csharp
public IList<Order> GetBulkShipCandidates(int minQuantity)
{
    using ShopDbContext context = CreateContext();
    return context.Orders
        .AsNoTracking()
        .Include(o => o.Lines.Where(l => l.Quantity >= minQuantity))
        .OrderBy(o => o.OrderId)
        .ToList();
}
```

**Concepts**
- filtered Include not filtering the outer set
- Where predicate on Include applies only to included children
- outer Where for order qualification
- two-filter distinction in filtered Include

**Answer**

Filtered `Include` only filters which child rows populate the collection — it does not filter the root `Order` entities, so orders with no qualifying lines still appear with an empty `Lines` collection unless you add a root `Where`.

1. Filter the root entity when the business rule applies to **which orders** are returned:

```csharp
return context.Orders
    .AsNoTracking()
    .Where(o => o.Lines.Any(l => l.Quantity >= minQuantity))
    .Include(o => o.Lines.Where(l => l.Quantity >= minQuantity))
    .OrderBy(o => o.OrderId)
    .ToList();
```

2. Keep filtered Include to avoid loading non-qualifying lines into the graph — both predicates should align on the same rule.
3. Document that filtered Include (EF Core 5+) translates to SQL on the collection subquery, not client-side filtering after a full load — see **OrderLoadingService** Section 8.

---

## Q149. (D) An order API has three read paths: (A) list view — id/date/customer name only; (B) detail view — customer + lines + product names; (C) background job — load customer only when an order fails validation. For each path, choose **projection**, **Include/ThenInclude**, or **explicit Load** — and name one production failure mode if you pick the wrong strategy.

**Concepts**
- list view vs detail view vs background job loading strategy
- projection vs Include vs explicit Load selection
- production failure mode per wrong strategy
- loading strategy decision matrix

**Answer**

Match loading strategy to how much of the graph you dereference — project flat list data, Include the full detail graph in one or split queries, and explicit Load only for conditional navigations on a tracked entity. | Path | Strategy | Why | Wrong choice → production failure | |---|---|---|---| | (A) List view | Projection (`Select` to DTO) | Only three scalars; no need for entity graph or change tracking | Include + ThenInclude → over-fetch (Q1): memory and SQL bloat on every list request | | (B) Detail view | Include / ThenInclude (optionally `AsSplitQuery` if cartesian risk) | Always need Customer, Lines, Product — same as GetOrdersWithEagerLoading | Projection-only without planning → multiple round-trips or N+1 if UI walks navigations later | | (C) Validation job | Explicit Load on tracked root after initial query | Customer needed only for failed orders — avoid loading customer for every order up front | Include Customer on bulk query → wasted JOINs when 95% pass validation; Load in loop without tracking → null Customer (Q4) | - (A) Keep `AsNoTracking`; never Include lines on list endpoints. - (B) Chain `.Include(o => o.Customer).Include(o => o.Lines).ThenInclude(l => l.Product)`; add `AsSplitQuery()` if metrics show cartesian row explosion (Q3). - (C) Query orders tracked (no `AsNoTracking`), run validation, then `Entry(order).Reference(o => o.Customer).Load()` only in the failure branch — context must stay open until Load completes. Production takeaway: The three EF loading modes from this chapter — eager, explicit, projection — are not interchangeable; Karat expects you to tie each HTTP/job shape to a strategy and name the failure mode (over-fetch, N+1, cartesian, null navigations) when you mismatch.
