# Azure Cosmos DB — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Azure Cosmos DB, and what core problems does it solve for cloud-native applications?](#q1-what-is-azure-cosmos-db-and-what-core-problems-does-it-solve-for-cloud-native-applications)
2. [Q2. When would you choose Azure Cosmos DB over Azure SQL Database or another relational database?](#q2-when-would-you-choose-azure-cosmos-db-over-azure-sql-database-or-another-relational-database)
3. [Q3. What is the global distribution model in Azure Cosmos DB, and how do read and write regions work?](#q3-what-is-the-global-distribution-model-in-azure-cosmos-db-and-how-do-read-and-write-regions-work)
4. [Q4. What availability and latency guarantees does Azure Cosmos DB provide through its service-level agreement (SLA)?](#q4-what-availability-and-latency-guarantees-does-azure-cosmos-db-provide-through-its-service-level-agreement-sla)
5. [Q5. What are the five Azure Cosmos DB API choices, and which one is considered the native API?](#q5-what-are-the-five-azure-cosmos-db-api-choices-and-which-one-is-considered-the-native-api)
6. [Q6. What are the core resource hierarchy levels in Azure Cosmos DB (account, database, container, item)?](#q6-what-are-the-core-resource-hierarchy-levels-in-azure-cosmos-db-account-database-container-item)
7. [Q7. What is a partition key in Azure Cosmos DB, and why is choosing it correctly critical?](#q7-what-is-a-partition-key-in-azure-cosmos-db-and-why-is-choosing-it-correctly-critical)
8. [Q8. What is the difference between a logical partition and a physical partition in Cosmos DB?](#q8-what-is-the-difference-between-a-logical-partition-and-a-physical-partition-in-cosmos-db)
9. [Q9. What is a hot partition in Cosmos DB, and how do you detect and mitigate it?](#q9-what-is-a-hot-partition-in-cosmos-db-and-how-do-you-detect-and-mitigate-it)
10. [Q10. What is a synthetic partition key, and when would you use one?](#q10-what-is-a-synthetic-partition-key-and-when-would-you-use-one)
11. [Q11. What are the five consistency levels in Azure Cosmos DB, and how do they trade off consistency versus latency?](#q11-what-are-the-five-consistency-levels-in-azure-cosmos-db-and-how-do-they-trade-off-consistency-versus-latency)
12. [Q12. Which consistency level is the default for Azure Cosmos DB accounts, and what does it guarantee?](#q12-which-consistency-level-is-the-default-for-azure-cosmos-db-accounts-and-what-does-it-guarantee)
13. [Q13. What is a Request Unit (RU/s) in Azure Cosmos DB, and what operations consume RUs?](#q13-what-is-a-request-unit-rus-in-azure-cosmos-db-and-what-operations-consume-rus)
14. [Q14. What is the difference between provisioned throughput and serverless capacity in Cosmos DB?](#q14-what-is-the-difference-between-provisioned-throughput-and-serverless-capacity-in-cosmos-db)
15. [Q15. What is autoscale throughput in Azure Cosmos DB, and when is it preferable to manual provisioning?](#q15-what-is-autoscale-throughput-in-azure-cosmos-db-and-when-is-it-preferable-to-manual-provisioning)
16. [Q16. What is the Cosmos DB SQL API, and how does its query language relate to standard SQL?](#q16-what-is-the-cosmos-db-sql-api-and-how-does-its-query-language-relate-to-standard-sql)
17. [Q17. How does the SQL API store documents, and what are the size limits for items and properties?](#q17-how-does-the-sql-api-store-documents-and-what-are-the-size-limits-for-items-and-properties)
18. [Q18. What is the default indexing policy in a Cosmos DB container, and how can over-indexing affect RU cost?](#q18-what-is-the-default-indexing-policy-in-a-cosmos-db-container-and-how-can-over-indexing-affect-ru-cost)
19. [Q19. What is the difference between a partition-key scoped query and a cross-partition query, and why does it matter for performance?](#q19-what-is-the-difference-between-a-partition-key-scoped-query-and-a-cross-partition-query-and-why-does-it-matter-for-performance)
20. [Q20. How do change feed and time-to-live (TTL) work in Cosmos DB containers?](#q20-how-do-change-feed-and-time-to-live-ttl-work-in-cosmos-db-containers)
21. [Q21. How do you create and register a `CosmosClient` in an ASP.NET Core application, and why should it be a singleton?](#q21-how-do-you-create-and-register-a-cosmosclient-in-an-aspnet-core-application-and-why-should-it-be-a-singleton)
22. [Q22. What is the difference between `CreateItemAsync`, `UpsertItemAsync`, and `ReplaceItemAsync` in the .NET SDK?](#q22-what-is-the-difference-between-createitemasync-upsertitemasync-and-replaceitemasync-in-the-net-sdk)
23. [Q23. How do you perform a point read versus a SQL query using the Azure Cosmos DB .NET SDK?](#q23-how-do-you-perform-a-point-read-versus-a-sql-query-using-the-azure-cosmos-db-net-sdk)
24. [Q24. How does optimistic concurrency work with `_etag` in the Cosmos DB .NET SDK?](#q24-how-does-optimistic-concurrency-work-with-_etag-in-the-cosmos-db-net-sdk)
25. [Q25. How should you handle transient errors and rate limiting (HTTP 429) in Cosmos DB .NET SDK code?](#q25-how-should-you-handle-transient-errors-and-rate-limiting-http-429-in-cosmos-db-net-sdk-code)
26. [Q26. What is the Entity Framework Core provider for Azure Cosmos DB, and what relational features does it not support?](#q26-what-is-the-entity-framework-core-provider-for-azure-cosmos-db-and-what-relational-features-does-it-not-support)
27. [Q27. How do you authenticate an ASP.NET Core application to Cosmos DB in production (connection string versus managed identity)?](#q27-how-do-you-authenticate-an-aspnet-core-application-to-cosmos-db-in-production-connection-string-versus-managed-identity)
28. [Q28. Gotcha: Why is treating Azure Cosmos DB like a drop-in replacement for SQL Server a common mistake?](#q28-gotcha-why-is-treating-azure-cosmos-db-like-a-drop-in-replacement-for-sql-server-a-common-mistake)

---

## Q1. What is Azure Cosmos DB, and what core problems does it solve for cloud-native applications?

**Concepts**
- Globally distributed multi-model NoSQL database service
- Single-digit millisecond reads and writes at any scale
- Fully managed — replication, patching, failover, storage scaling
- Tunable consistency levels — strong to eventual
- Request Units (RU/s) — unified throughput currency

**Answer**

Azure Cosmos DB is a globally distributed, multi-model NoSQL database service that stores schema-flexible JSON documents with single-digit millisecond reads and writes at any scale. It solves the problem of building applications that must serve users worldwide with predictable low latency, elastic throughput, and high availability without teams operating their own database clusters across regions. It is fully managed, so Microsoft handles replication, patching, failover, and storage scaling, which means application teams focus on data modeling and query design rather than shard management or replica promotion scripts. It offers tunable consistency levels from strong to eventual so architects can choose how tightly all regions must agree on a read versus how fast a read can return. Throughput is measured in Request Units per second, giving a single currency for reads, writes, and queries instead of sizing separate compute and storage tiers. Multiple API surfaces — SQL (Core), MongoDB, Cassandra, Gremlin, and Table — let teams migrate or integrate with familiar protocols while Cosmos DB runs one distributed engine underneath.

---

## Q2. When would you choose Azure Cosmos DB over Azure SQL Database or another relational database?

**Concepts**
- Cosmos DB fit — horizontal scale, flexible schema, global distribution
- Azure SQL fit — complex joins, foreign keys, multi-table transactions
- Access pattern alignment — single-partition reads vs join-heavy queries
- Cost model difference — RU/s and storage vs compute tier and storage
- Multi-master writes vs geo-replication trade-off

**Answer**

Choose Azure Cosmos DB when your workload needs horizontal scale across regions, flexible document schemas, predictable single-digit millisecond latency at global scale, or a NoSQL data model that does not map cleanly to normalized relational tables. Choose Azure SQL Database when you need rich relational features — complex joins, foreign keys, transactional constraints across many tables, and mature reporting tools — on a single-region or modest multi-region relational model. Cosmos DB fits high-volume IoT ingestion, user profile stores, product catalogs with varying attributes, session state, and globally replicated read-heavy APIs where each request typically touches one partition key. Azure SQL Database fits order-processing systems with strict referential integrity, financial ledgers requiring multi-table transactions, and applications that rely heavily on T-SQL stored procedures or Entity Framework Core relational mappings with joins. Cosmos DB global distribution with automatic failover is a first-class feature, since multi-master write patterns and schema-less evolution are more natural there, while Azure SQL may be cheaper when data volume is moderate and access patterns are join-heavy on a single region because Cosmos DB bills primarily by provisioned RU/s and storage rather than by compute tier.

---

## Q3. What is the global distribution model in Azure Cosmos DB, and how do read and write regions work?

**Concepts**
- Multi-region replication — automatic data copy across Azure regions
- Single-write-region vs multi-region write (multi-master) mode
- Last-writer-wins conflict resolution with `_ts` timestamp
- Automatic failover — read region promotion to write region
- Preferred regions in CosmosClient — lowest-latency routing

**Answer**

Azure Cosmos DB lets you associate one or more Azure regions with an account, replicating data automatically so clients can read from a nearby region with low latency. In single-write-region mode, all writes go to the write region and replicate asynchronously to read regions, which serve local reads without cross-region round trips for every request. In multi-region write mode, every configured region accepts writes for its local partition replicas, and Cosmos DB uses last-writer-wins conflict resolution with a built-in `_ts` timestamp when the same item is updated in two regions concurrently. Automatic failover can promote a former read region to write region if the primary region becomes unavailable, which supports high availability without manual DNS or cluster reconfiguration. Application code should use the region nearest users when creating the `CosmosClient` or configure preferred regions so the SDK routes reads to the lowest-latency endpoint.

---

## Q4. What availability and latency guarantees does Azure Cosmos DB provide through its service-level agreement (SLA)?

**Concepts**
- 99.99% availability for single-region accounts
- 99.999% availability for multi-region accounts with multiple regions
- Read latency SLA — under 10 ms at P99 for same-region reads
- Indexed query latency SLA — under 10 ms at P99 under documented conditions
- Provisioned RU/s availability — sustained throttling is a configuration issue

**Answer**

Microsoft publishes a financially backed SLA for Azure Cosmos DB that covers availability, latency, and consistency when the account is configured according to documented requirements. A single-region account with sufficient throughput is guaranteed at least 99.99% availability, and multi-region accounts can reach 99.999% availability when configured with multiple regions. Read latency at the 99th percentile is SLA-backed at under 10 milliseconds for items up to 1 KB when reading from the same region with session or stronger consistency in the SQL API, and indexed queries carry a matching latency SLA under the same documented conditions. The throughput SLA ensures provisioned RU/s is available for operations; sustained throttling from under-provisioning is a configuration issue rather than an SLA breach. SLA credits apply only when Microsoft fails to meet published metrics, which means teams must still right-size RU/s, choose appropriate consistency, and model partition keys correctly to meet their own application latency targets.

---

## Q5. What are the five Azure Cosmos DB API choices, and which one is considered the native API?

**Concepts**
- SQL (Core) API — native API, receives new features first
- MongoDB API — wire-protocol compatibility for migration
- Cassandra API — CQL-compatible wide-column model
- Gremlin API — graph traversal for relationship analytics
- Table API — Azure Table Storage upgrade path

**Answer**

Azure Cosmos DB exposes five APIs: SQL API (also called Core or Native API), MongoDB API, Cassandra API, Gremlin API, and Table API. The SQL API is the native API because it speaks directly to Cosmos DB's internal document engine and receives new platform features first. All APIs share the same global distribution, SLA backbone, and Request Unit billing model, but SDK features, query capabilities, and tooling differ by API surface. The .NET `Microsoft.Azure.Cosmos` SDK and the Entity Framework Core Cosmos provider target the SQL API exclusively. Choosing MongoDB or Cassandra API is often a migration convenience; greenfield ASP.NET Core services typically use the SQL API for the richest SDK integration and clearest Cosmos-specific features such as change feed.

| API | Wire protocol / model | Typical migration source |
|---|---|---|
| SQL (Core) | JSON documents + SQL-like queries | Greenfield .NET apps, Azure SDK |
| MongoDB | MongoDB wire protocol | Existing MongoDB workloads |
| Cassandra | CQL (Cassandra Query Language) | Wide-column Cassandra apps |
| Gremlin | Graph traversal language | Graph analytics, relationships |
| Table | Azure Table storage REST | Legacy Table storage apps |

---

## Q6. What are the core resource hierarchy levels in Azure Cosmos DB (account, database, container, item)?

**Concepts**
- Account — billing, regional placement, API type boundary
- Database — namespace grouping containers, optional shared throughput
- Container — unit of scale, partition key, indexing policy, TTL
- Item — JSON document identified by `id` plus partition key value
- 2 MB maximum item size

**Answer**

An Azure Cosmos DB account is the top-level billing and regional boundary; it contains one or more databases, each database contains containers, and each container holds items. Throughput (RU/s) and indexing policies are set at the container level in most configurations. The account holds the endpoint URI, consistency default, regional placement, and API type; you create one account per API since you cannot mix SQL and MongoDB APIs in the same account. A database is a namespace grouping containers; it does not serve requests directly but organizes resources and can participate in shared throughput offers. The container is the unit of scale and billing for provisioned RU/s; each container has a partition key definition, indexing policy, and optional TTL setting. An item is a single JSON document (SQL API) identified by `id` plus partition key value, with a maximum size of 2 MB including all properties.

---

## Q7. What is a partition key in Azure Cosmos DB, and why is choosing it correctly critical?

**Concepts**
- Partition key — property whose value determines item placement
- High cardinality requirement — many distinct values for even distribution
- Immutability — cannot change partition key after container creation
- Item address — `id` + partition key value must be unique in container
- Poor partition key — hot spots, throttling regardless of total RU/s

**Answer**

A partition key is a property (or set of properties) whose value determines which logical partition stores and serves an item. Cosmos DB uses it to colocate related data so point reads and writes by that key are efficient and can scale out by spreading logical partitions across physical storage and compute. Every container requires a partition key at creation time and it cannot be changed later without migrating data to a new container, which makes the initial choice critical. The combination of item `id` and partition key value must be unique within a container and forms the item's address for direct lookups. A good partition key has high cardinality — many distinct values — even distribution of requests and storage, and alignment with the most common query filter in the application. A poor partition key such as a boolean flag or a single tenant id for a massive multitenant system without suffixes creates hot partitions that throttle at the partition's RU/s ceiling regardless of total container throughput.

---

## Q8. What is the difference between a logical partition and a physical partition in Cosmos DB?

**Concepts**
- Logical partition — all items sharing one partition key value
- Physical partition — backend storage and compute unit hosting many logical partitions
- 20 GB logical partition size limit
- Automatic physical partition split and merge
- Throughput divided across physical partitions

**Answer**

A logical partition is the set of items that share the same partition key value; Cosmos DB guarantees that all items with one partition key value live together and are served by the same replica set. A physical partition is the underlying storage and compute unit on the backend that hosts one or more logical partitions as data grows. Logical partitions can grow up to 20 GB of storage; if a single partition key value approaches that limit, you must redesign the key — for example add a suffix — because Cosmos DB will reject writes that exceed it. When logical partitions accumulate, Cosmos DB splits or merges physical partitions automatically; application developers do not manage shard maps manually. Throughput provisioned on a container is divided across physical partitions, but a hot logical partition can still consume a disproportionate share and trigger HTTP 429 throttling on that key range. Point operations specifying both `id` and partition key route to exactly one logical partition, which is the cheapest and fastest access pattern.

---

## Q9. What is a hot partition in Cosmos DB, and how do you detect and mitigate it?

**Concepts**
- Hot partition — one partition key value saturates its RU/s share
- HTTP 429 concentrated on specific keys as symptom
- Normalized RU consumption metrics in Azure Monitor
- Partition key redesign — compound or synthetic key for load spreading
- Increasing total RU/s alone does not fix per-partition ceiling

**Answer**

A hot partition occurs when one partition key value receives far more reads, writes, or storage than others, causing that logical partition to saturate its share of Request Units while the rest of the container remains underutilized, and it is one of the most common production performance problems in Cosmos DB. Symptoms include HTTP 429 errors concentrated on specific keys, high normalized RU consumption metrics for a narrow key range in Azure Monitor, and uneven storage growth per partition key value. Mitigation starts with partition key redesign: split high-traffic tenants using a compound or synthetic key such as `tenantId + shardId`, or hash a skewed identifier so load spreads across many values. For write-heavy streams where key skew is inherent — for example a single global counter — offload increments to a different pattern such as Azure Functions aggregation or event hub buffering instead of hammering one document. Increasing total container RU/s alone does not fix a hot partition if traffic remains concentrated on one key, since the partition-level ceiling still applies.

---

## Q10. What is a synthetic partition key, and when would you use one?

**Concepts**
- Synthetic partition key — value computed at write time from multiple fields
- Hash or concatenation to improve distribution over natural keys
- Trade-off — better scaling vs more complex query logic
- Must be persisted on the document for reads and deletes
- Use case — low-cardinality natural key, telemetry partition skew, 20 GB limit risk

**Answer**

A synthetic partition key is a value computed at write time — often by concatenating or hashing business fields — to improve distribution when no single natural property provides both high cardinality and query alignment. It lets teams spread load while still supporting predictable access patterns when combined with the stored original fields. For example, a multitenant app where `tenantId` alone would hot-spot large tenants might use `partitionKey = tenantId + "-" + (userId.GetHashCode() % 20)` so each tenant's data spans multiple logical partitions. Synthetic keys trade easier scaling for query complexity, since lookups that need an entire tenant's data may require parallel queries across known suffix ranges or a secondary indexing pattern. The synthetic value must be persisted on the document — or derivable from indexed fields — so the application can supply it on every read, replace, and delete operation. Use synthetic keys when telemetry shows partition skew, when a natural key has low cardinality such as country code only, or when item count per key risks approaching the 20 GB logical partition limit.

---

## Q11. What are the five consistency levels in Azure Cosmos DB, and how do they trade off consistency versus latency?

**Concepts**
- Strong — global quorum read, highest latency, single-write-region only
- Bounded staleness — lag bounded by K versions or T time
- Session (default) — read-your-writes within a client session
- Consistent prefix — no out-of-order reads, may lag
- Eventual — lowest latency, no ordering guarantee

**Answer**

Azure Cosmos DB offers five tunable consistency levels that control how soon reads reflect writes across regions and replicas: Strong, Bounded Staleness, Session, Consistent Prefix, and Eventual. Stronger levels add coordination cost and latency; weaker levels allow faster reads with broader windows of stale or unordered data. Strong consistency is available only in single-write-region accounts since multi-master accounts cannot offer global strong reads across all write regions. Session consistency is the default and fits most ASP.NET Core web APIs when each user request carries a session token from prior responses. Weaker levels improve read throughput and reduce cross-region round trips for globally distributed read-heavy dashboards where slightly stale data is acceptable.

| Level | Summary | Typical latency impact |
|---|---|---|
| Strong | Reads always return the latest committed write globally | Highest — requires quorum read |
| Bounded staleness | Reads lag behind writes by at most K versions or T time | Moderate |
| Session | Consistent within a single client session (default) | Low for same-session reads |
| Consistent prefix | Reads never see out-of-order writes; may lag | Lower |
| Eventual | No ordering guarantee; lowest coordination | Lowest |

---

## Q12. Which consistency level is the default for Azure Cosmos DB accounts, and what does it guarantee?

**Concepts**
- Session consistency — default level
- Read-your-writes guarantee within a logical session
- Session token — SDK attaches automatically to responses
- Per-request override with `ItemRequestOptions.ConsistencyLevel`
- Bounded staleness and strong for regulatory or financial ordering requirements

**Answer**

Session is the default consistency level for Azure Cosmos DB accounts. It guarantees read-your-writes, monotonic reads, and monotonic writes within a logical session — meaning a client that writes an item and then reads it with the same session token will see its own update, though other clients may briefly see older values. The .NET SDK attaches session tokens to responses automatically; subsequent reads in the same `CosmosClient` instance or when tokens are forwarded between services preserve session guarantees. Session consistency does not guarantee that two different clients see the same value at the same instant unless they share session context or a stronger level is configured on the request. You can override consistency per request in the SDK via `ItemRequestOptions.ConsistencyLevel` or lower the account default for read-heavy workloads after validating business tolerance for staleness. Bounded staleness and strong are stricter choices when regulatory or financial rules require provable global ordering beyond a single user's session.

---

## Q13. What is a Request Unit (RU/s) in Azure Cosmos DB, and what operations consume RUs?

**Concepts**
- Request Unit (RU) — normalized cost of reading 1 KB by id and partition key
- Point reads cheapest; cross-partition queries most expensive
- Write RU cost — proportional to document size and indexed properties
- Indexing policy impact — excluding paths reduces write RU charge
- Throttling at HTTP 429 when RU/s budget is exceeded

**Answer**

A Request Unit (RU) is the normalized currency for every database operation in Cosmos DB — one RU represents the cost of reading a 1 KB item by id and partition key with session consistency in the same region. Provisioned throughput defines how many RUs the container can consume per second before throttling. Point reads by `id` and partition key are the cheapest operations; cross-partition SQL queries, large document writes, and queries without selective filters consume many more RUs because they scan more indexed data or fan out to all partitions. Indexing every property increases write RU cost because each indexed path is updated on insert or replace; excluding unused paths in the indexing policy reduces write charges. Stored procedures, triggers, and user-defined functions run inside the container and consume RUs from the same budget as client-initiated operations.

---

## Q14. What is the difference between provisioned throughput and serverless capacity in Cosmos DB?

**Concepts**
- Provisioned (manual) — fixed RU/s per hour regardless of usage
- Provisioned (autoscale) — pay for peak RU/s used per hour
- Serverless — per-request RU consumption, no minimum reservation
- Serverless limitations — throughput ceiling, storage limits per container
- Database-level vs container-level provisioned offer

**Answer**

Provisioned throughput reserves a fixed or autoscale range of RU/s for a container or database, billing continuously whether you use it or not but delivering predictable performance for steady workloads. Serverless mode bills only for RUs consumed per operation with no minimum RU/s reservation, which suits intermittent or development workloads with unpredictable traffic. Serverless accounts have limits — maximum storage per container, no multi-region write in all configurations, and a throughput ceiling per container — that make it unsuitable for high sustained load. Provisioned throughput can be shared across containers in the same database (database-level offer) or dedicated per container (container-level offer). Switching between serverless and provisioned requires account-level planning; they are not toggled per request.

| Mode | Billing | Best for |
|---|---|---|
| Provisioned (manual) | Fixed RU/s per hour | Stable production traffic, cost predictability at scale |
| Provisioned (autoscale) | Pay for peak RU/s used in each hour | Variable but recurring load |
| Serverless | Per-request RU consumption | Dev/test, sporadic APIs, prototypes |

---

## Q15. What is autoscale throughput in Azure Cosmos DB, and when is it preferable to manual provisioning?

**Concepts**
- Autoscale — automatic RU/s scaling between 10% minimum and configured maximum
- Billing per peak RU/s in each hour
- Preferred for variable traffic with non-zero baseline
- Manual fixed provisioning — better when utilization stays above ~70%
- Autoscale maximum still throttles if spikes exceed it

**Answer**

Autoscale throughput lets a container scale RU/s automatically between a configured minimum (10% of maximum) and maximum based on actual usage, billing for the highest RU/s level reached in each hour. It removes the need to manually raise and lower provisioned throughput as daily or weekly traffic patterns shift. Prefer autoscale when traffic varies predictably — business hours peaks, batch imports, seasonal events — but baseline load is non-zero and you want to avoid overnight over-provisioning. Manual fixed provisioning may cost less when utilization is consistently above 70% of a steady RU/s tier and you rarely need headroom spikes. The autoscale maximum defines the throttle ceiling; if spikes exceed it, HTTP 429 still occurs until you raise the max or optimize queries, so monitor `AutoscaleMaxThroughput` and normalized RU consumption in Azure Monitor to right-size the max bound without paying for unused headroom.

---

## Q16. What is the Cosmos DB SQL API, and how does its query language relate to standard SQL?

**Concepts**
- SQL API stores JSON documents — schema-less within a container
- SQL-like query language — SELECT, FROM, WHERE with dot-notation for properties
- `JOIN` on embedded arrays — not cross-container joins
- Cross-partition aggregations — higher RU cost than scoped filters
- `.GetItemQueryIterator` and EF Core LINQ translation

**Answer**

The Cosmos DB SQL API stores JSON documents and exposes a SQL-like query language designed for hierarchical, schema-less data rather than relational tables. It uses familiar `SELECT`, `FROM`, and `WHERE` clauses but navigates document properties with dot notation and supports constructs such as `ARRAY` functions and `JOIN` on embedded arrays. There is no `JOIN` across containers; relationships are modeled by embedding, denormalizing, or performing multiple point reads from application code. Aggregations — `COUNT`, `SUM`, `AVG`, `GROUP BY` — are supported but cross-partition aggregations consume more RUs and take longer than partition-scoped filters. The same language powers the Azure Portal query explorer, the .NET SDK's `GetItemQueryIterator`, and EF Core Cosmos LINQ translations with limitations.

---

## Q17. How does the SQL API store documents, and what are the size limits for items and properties?

**Concepts**
- JSON document with system properties — `id`, `_etag`, `_ts`, `_self`
- `id` — string unique within the logical partition
- `_etag` — system concurrency token for optimistic replace
- `_ts` — Unix epoch timestamp used in TTL and conflict resolution
- 2 MB maximum item size — large binaries belong in Blob Storage

**Answer**

The SQL API stores each item as a JSON document with system properties — `id`, `_etag`, `_ts`, `_self`, and the partition key field — plus application-defined properties. Items are schema-flexible within a container; different documents can have different property sets, but every item must not exceed 2 MB total size. The `id` is a string identifier unique within the logical partition; combined with partition key it is globally unique in the container. The `_etag` is a system-generated concurrency token used for optimistic replace and delete operations. The `_ts` is a Unix epoch timestamp updated on each write and used in TTL expiration and conflict resolution. Very large binary payloads should be stored in Azure Blob Storage with a reference URL in the document rather than inlined base64, both to stay under size limits and to reduce RU charges on reads.

---

## Q18. What is the default indexing policy in a Cosmos DB container, and how can over-indexing affect RU cost?

**Concepts**
- Default policy — index all properties (`/*`) with consistent mode
- Excluded paths — reduce write RU cost for unqueried properties
- Composite indexes — required for multi-property `ORDER BY` queries
- Spatial indexes — geospatial queries on GeoJSON points
- Index transformation on large containers — consumes RUs during policy change

**Answer**

By default, Cosmos DB automatically indexes every property path in every document with `"indexingMode": "consistent"` and `"includedPaths": [{ "path": "/*" }]`, which makes ad hoc queries fast without schema planning but increases write RU cost because each indexed path is updated on every insert or replace. Excluded paths remove properties from the index — useful for large blob metadata, audit payloads, or fields never queried — and lower write RU consumption. Composite indexes can be added for multi-property `ORDER BY` queries; without them, certain sort queries fail or fall back to expensive in-memory sorts. Spatial indexes support geospatial queries on GeoJSON points and add overhead only when those paths are included. Review the indexing policy during design rather than after production launch, since reducing indexed paths on a large container triggers an index transformation that consumes RUs and time.

---

## Q19. What is the difference between a partition-key scoped query and a cross-partition query, and why does it matter for performance?

**Concepts**
- Partition-scoped query — equality filter on partition key, routes to one or few partitions
- Cross-partition query — no partition key filter, fan-out to every physical partition
- Point read — `id` + partition key, typically 1 RU for a small document
- RU cost proportional to partitions scanned
- Change feed and materialized views for analytics requiring full scans

**Answer**

A partition-key scoped query includes an equality filter on the partition key, so Cosmos DB routes the query to one or a few logical partitions. A cross-partition query lacks that filter and must fan out to every physical partition, merging results and charging RUs proportional to partitions scanned. Point reads — `ReadItemAsync` with `id` and partition key — are the most efficient access, typically 1 RU for a small document. A partition-scoped SQL query such as `SELECT * FROM c WHERE c.tenantId = 'abc'` when `tenantId` is the partition key consumes RUs only within that tenant's partition. Cross-partition queries like `SELECT * FROM c WHERE c.status = 'Active'` without partition key equality can work but scale poorly and become expensive as container partition count grows. Design containers and queries so hot paths always supply the partition key; use change feed or materialized views for analytics that inherently need full scans.

---

## Q20. How do change feed and time-to-live (TTL) work in Cosmos DB containers?

**Concepts**
- Change feed — ordered incremental log of inserts and updates
- `ChangeFeedProcessor` with lease container for distributed processing
- TTL — automatic item deletion after configured seconds since `_ts`
- TTL deletion — asynchronous, no explicit delete RU charge
- Change feed and TTL delete events — design for idempotent handling

**Answer**

Change feed is an ordered, incremental log of inserts and updates in a container that downstream processors can read to build projections, sync search indexes, or trigger Azure Functions. TTL is a container or item-level expiration policy that automatically deletes documents after a specified number of seconds since last modification via `_ts`. Change feed does not emit delete events for TTL expirations in all configurations the same way as explicit deletes, so consumers should design idempotent handling and periodic reconciliation if exact delete tracking matters. TTL is enabled on the container with a default seconds value; individual items can override with a `ttl` property set to a positive integer or `-1` to opt out of expiration. TTL deletion is asynchronous and not instant; expired items disappear without consuming explicit delete RU charges from application code. Change feed processors in .NET use the `ChangeFeedProcessor` or `ChangeFeedEstimator` APIs with a lease container to distribute partition processing across worker instances.

---

## Q21. How do you create and register a `CosmosClient` in an ASP.NET Core application, and why should it be a singleton?

**Concepts**
- Singleton `CosmosClient` — shared connection pools and routing metadata cache
- `DefaultAzureCredential` for managed identity authentication
- `CosmosClientOptions` — preferred regions, connection mode, serializer
- Expensive construction — do not create per HTTP request
- `IAsyncDisposable` — host disposes on application stop

**Answer**

Create one `CosmosClient` instance for the application lifetime using the account endpoint and key or token credential, and register it as a singleton in dependency injection. The client is thread-safe, maintains connection pools and cached routing metadata, and is expensive to construct repeatedly, so creating a new `CosmosClient` per HTTP request increases latency, exhausts sockets, and prevents session token reuse.

```csharp
builder.Services.AddSingleton(_ =>
    new CosmosClient(endpoint, new DefaultAzureCredential()));
```

Register repositories or services that take `CosmosClient` as a constructor dependency as scoped or singleton depending on whether they hold per-request state; the client itself remains singleton. Use `CosmosClientOptions` to set application name, preferred regions, connection mode (direct versus gateway), and custom serializers for camelCase JSON alignment with ASP.NET Core defaults. During shutdown, `CosmosClient` implements `IAsyncDisposable`; the host disposes singletons on application stop.

---

## Q22. What is the difference between `CreateItemAsync`, `UpsertItemAsync`, and `ReplaceItemAsync` in the .NET SDK?

**Concepts**
- `CreateItemAsync` — insert only, fails if `id` + partition key already exists
- `ReplaceItemAsync` — full overwrite of existing item, fails if absent
- `UpsertItemAsync` — insert or replace, convenient but masks accidental overwrites
- `PatchItemAsync` — partial update, lower RU cost than full replace
- `IfMatchEtag` in `ItemRequestOptions` — optimistic concurrency on replace

**Answer**

`CreateItemAsync` inserts a new item and fails if an item with the same `id` and partition key already exists, which makes it the right choice when duplicate creation is a business error you want to detect explicitly. `ReplaceItemAsync` overwrites an existing item entirely and fails if the item does not exist; partial patch updates use `PatchItemAsync` in newer SDK versions to send only changed properties and reduce RU cost. `UpsertItemAsync` inserts when absent or replaces when present, making it convenient but masking accidental overwrites, so use it deliberately because it silently updates existing records. Pass `IfMatchEtag` in `ItemRequestOptions` on replace to enforce optimistic concurrency; the call fails with HTTP 412 if another writer updated the item first.

---

## Q23. How do you perform a point read versus a SQL query using the Azure Cosmos DB .NET SDK?

**Concepts**
- `ReadItemAsync<T>` — cheapest access, requires both `id` and partition key
- `QueryDefinition` with parameterized SQL — avoids injection, supports filtering
- `GetItemQueryIterator<T>` — paginated streaming with continuation tokens
- `QueryRequestOptions.PartitionKey` — scopes query to one partition
- `MaxItemCount` and `MaxConcurrency` — pagination and parallel partition control

**Answer**

A point read calls `Container.ReadItemAsync<T>(id, partitionKey)` and returns one document with minimal RU cost; a SQL query builds a `QueryDefinition` and iterates `GetItemQueryIterator<T>` to stream matching documents, optionally with partition key scope to limit fan-out. Point reads need both `id` and partition key; supplying only `id` is not supported. Query iterators handle pagination automatically via continuation tokens; always drain or dispose iterators to avoid leaking resources. Use `QueryRequestOptions.MaxItemCount` to page large result sets and `MaxConcurrency` to tune parallel partition queries for cross-partition scans.

```csharp
var item = await container.ReadItemAsync<Order>(orderId, new PartitionKey(tenantId));
var query = new QueryDefinition("SELECT * FROM c WHERE c.status = @s").WithParameter("@s", "Shipped");
using var iterator = container.GetItemQueryIterator<Order>(query,
    requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId) });
```

---

## Q24. How does optimistic concurrency work with `_etag` in the Cosmos DB .NET SDK?

**Concepts**
- `_etag` — system property updated on every item write
- `IfMatchEtag` in `ItemRequestOptions` — conditional replace or delete
- HTTP 412 Precondition Failed — returned when etag no longer matches
- Last-writer-wins default when no etag check is supplied
- `PatchItemAsync` etag support for high-contention partial updates

**Answer**

Every item carries a system `_etag` value that changes whenever the item is updated. When replacing or deleting, you can supply the etag from your last read in `IfMatchEtag`; Cosmos DB applies the write only if the stored etag still matches, otherwise it returns HTTP 412 Precondition Failed. Read the etag from `ItemResponse<T>.ETag` after a point read or query, store it in your domain model or concurrency token field, and pass it on update. Without an etag check, last-writer-wins applies silently — two concurrent updates can lose one writer's changes without error. `PatchItemAsync` also supports etag preconditions for partial updates in high-contention scenarios such as inventory decrements. This pattern mirrors EF Core concurrency tokens but uses Cosmos DB's native HTTP semantics instead of rowversion columns.

---

## Q25. How should you handle transient errors and rate limiting (HTTP 429) in Cosmos DB .NET SDK code?

**Concepts**
- Built-in SDK retry policy for HTTP 429 — honors `x-ms-retry-after-ms` header
- `MaxRetryAttemptsOnRateLimitedRequests` and `MaxRetryWaitTimeOnRateLimitedRequests`
- HTTP 429 as capacity signal — under-provisioned or hot partition
- `RequestCharge` on responses — RU consumption per operation
- Non-transient errors — 400, 404, 412 require domain-specific handling

**Answer**

The .NET SDK includes built-in retry policies for transient failures and throttling, honoring the `x-ms-retry-after-ms` header on HTTP 429 responses by waiting and retrying with exponential backoff. Application code should still treat sustained throttling as a capacity or hot-partition problem rather than relying only on retries. Configure `CosmosClientOptions.MaxRetryAttemptsOnRateLimitedRequests` and `MaxRetryWaitTimeOnRateLimitedRequests` to bound retry behavior under load tests. Log 429 responses with activity id and request charge — via `RequestCharge` on responses — to distinguish under-provisioned RU/s from skewed partition keys. For write-heavy bursts, use the bulk execution support or partition workloads temporally rather than infinite retry loops that delay user responses. Non-transient errors — 400 bad request, 404 not found, 412 precondition failed — should not be retried blindly; handle them with domain-specific logic.

---

## Q26. What is the Entity Framework Core provider for Azure Cosmos DB, and what relational features does it not support?

**Concepts**
- `Microsoft.EntityFrameworkCore.Cosmos` — LINQ to Cosmos SQL translation
- No cross-entity joins — relationships as embedded documents or reference ids
- No foreign key enforcement or relational migrations
- `EnsureCreated` vs EF migrations — different semantics for Cosmos
- Drop to native SDK for change feed, bulk APIs, advanced indexing

**Answer**

EF Core includes a Cosmos DB provider that maps entity classes to JSON documents in a container using LINQ queries translated to Cosmos SQL where possible. It accelerates CRUD-centric apps but does not implement full relational semantics — many SQL Server patterns simply do not apply. Unsupported or limited features include true cross-entity joins, foreign key enforcement, migrations that alter schemas across related tables, raw T-SQL, and some transaction scopes spanning multiple partition keys. Relationships are modeled as embedded documents or reference ids; `Include` may generate separate queries rather than SQL joins. `EnsureCreated` and EF migrations exist but differ from relational providers, so review generated container and indexing configuration carefully. Drop to the `Microsoft.Azure.Cosmos` SDK directly when you need change feed, fine-grained RU control, bulk APIs, or advanced indexing policies.

---

## Q27. How do you authenticate an ASP.NET Core application to Cosmos DB in production (connection string versus managed identity)?

**Concepts**
- Managed identity with `DefaultAzureCredential` — preferred for Azure-hosted apps
- Cosmos DB Built-in Data Contributor RBAC role assignment
- Connection strings — full account access, rotation risk, Key Vault storage
- `DefaultAzureCredential` — resolves to managed identity in Azure, developer credentials locally
- Diagnostic settings for data plane access auditing

**Answer**

Production ASP.NET Core apps on Azure should prefer Microsoft Entra ID authentication with managed identity instead of embedding primary account keys in connection strings. The `CosmosClient` accepts a `TokenCredential` such as `DefaultAzureCredential`, which resolves to the App Service or Azure Functions managed identity in Azure and to local developer credentials during development. Assign the app's managed identity the Cosmos DB Built-in Data Contributor role — or a custom role — on the account or specific database scope in Azure RBAC. Connection strings with account keys remain valid for local prototyping but grant full account access and leak easily through configuration dumps; when keys are unavoidable in legacy systems, store them in Azure Key Vault and inject via configuration providers rather than `appsettings.json` in source control. Enable diagnostic settings on the Cosmos account to audit data plane access regardless of authentication method.

---

## Q28. Gotcha: Why is treating Azure Cosmos DB like a drop-in replacement for SQL Server a common mistake?

**Concepts**
- Partition-key-addressed document store — not row-addressed relational table
- Request Unit billing — cross-partition scans are expensive
- Relational normalization — must become embedding or denormalization
- Multi-document ACID transactions — limited to single partition key value
- Migration as data model redesign — not a connection string swap

**Answer**

Teams often assume Cosmos DB behaves like SQL Server with horizontal scale, but it is a partition-key-addressed document store with Request Unit billing and limited cross-partition transactional guarantees. Porting normalized schemas, ad hoc join queries, and identity-column patterns without redesign leads to high RU bills, throttling, and data modeling friction. Relational normalization across many tables with foreign keys must become embedding, denormalization, or application-level composition — there is no cheap equivalent to multi-table joins inside Cosmos DB. Every query that omits the partition key may fan out to all partitions; a SQL Server table scan on a moderate table is not comparable in cost or latency at Cosmos scale. Multi-document ACID transactions exist within a single partition key value but not globally across arbitrary documents the way a SQL Server transaction spans rows in many tables. Success requires upfront partition key design, indexing policy tuning, and consistency level choice — treat migration as a data model project, not a connection string swap.

---
