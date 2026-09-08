# Interview Questions — Database per Service — Interview Q&A
> 25 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is the Database per Service pattern, and why is it considered a foundational principle of microservices architecture?](#q1-what-is-the-database-per-service-pattern-and-why-is-it-considered-a-foundational-principle-of-microservices-architecture)
2. [Q2. What is the Shared Database anti-pattern in microservices, and why does it undermine service autonomy?](#q2-what-is-the-shared-database-anti-pattern-in-microservices-and-why-does-it-undermine-service-autonomy)
3. [Q3. What are the main benefits and trade-offs of giving every microservice its own dedicated database?](#q3-what-are-the-main-benefits-and-trade-offs-of-giving-every-microservice-its-own-dedicated-database)
4. [Q4. What is polyglot persistence, and how does the Database per Service pattern enable it?](#q4-what-is-polyglot-persistence-and-how-does-the-database-per-service-pattern-enable-it)
5. [Q5. What are the three common levels of database isolation used in microservices (separate server, separate schema, shared schema with prefixed tables), and when is each appropriate?](#q5-what-are-the-three-common-levels-of-database-isolation-used-in-microservices-separate-server-separate-schema-shared-schema-with-prefixed-tables-and-when-is-each-appropriate)
6. [Q6. What is the difference between logical isolation (separate schemas/users) and physical isolation (separate database servers), and what does each guarantee?](#q6-what-is-the-difference-between-logical-isolation-separate-schemasusers-and-physical-isolation-separate-database-servers-and-what-does-each-guarantee)
7. [Q7. How do you choose between a relational database and a NoSQL store for a specific microservice?](#q7-how-do-you-choose-between-a-relational-database-and-a-nosql-store-for-a-specific-microservice)
8. [Q8. Why can microservices not share database tables or issue cross-database JOIN queries, and what is the consequence of allowing it?](#q8-why-can-microservices-not-share-database-tables-or-issue-cross-database-join-queries-and-what-is-the-consequence-of-allowing-it)
9. [Q9. What is the API Composition pattern, and how does it replace cross-service database queries?](#q9-what-is-the-api-composition-pattern-and-how-does-it-replace-cross-service-database-queries)
10. [Q10. What is a CQRS read model (materialized view) that spans multiple services, and how is it kept up to date?](#q10-what-is-a-cqrs-read-model-materialized-view-that-spans-multiple-services-and-how-is-it-kept-up-to-date)
11. [Q11. What is data duplication in the context of microservices, and why is controlled duplication sometimes the correct design choice?](#q11-what-is-data-duplication-in-the-context-of-microservices-and-why-is-controlled-duplication-sometimes-the-correct-design-choice)
12. [Q12. Why are distributed (two-phase commit) transactions impractical across microservice databases, and what replaces them?](#q12-why-are-distributed-two-phase-commit-transactions-impractical-across-microservice-databases-and-what-replaces-them)
13. [Q13. What is the Saga pattern, and how does it maintain data consistency across services that each own their own database?](#q13-what-is-the-saga-pattern-and-how-does-it-maintain-data-consistency-across-services-that-each-own-their-own-database)
14. [Q14. What is the Outbox Pattern, and why is it needed when a service must both save to its own database and publish an event?](#q14-what-is-the-outbox-pattern-and-why-is-it-needed-when-a-service-must-both-save-to-its-own-database-and-publish-an-event)
15. [Q15. What is eventual consistency, and how do you design a user experience that tolerates it?](#q15-what-is-eventual-consistency-and-how-do-you-design-a-user-experience-that-tolerates-it)
16. [Q16. How do you manage Entity Framework Core migrations in a microservice that owns its own database, especially in a containerized environment?](#q16-how-do-you-manage-entity-framework-core-migrations-in-a-microservice-that-owns-its-own-database-especially-in-a-containerized-environment)
17. [Q17. What is a backward-compatible schema change, and why is it critical when services deploy independently?](#q17-what-is-a-backward-compatible-schema-change-and-why-is-it-critical-when-services-deploy-independently)
18. [Q18. How do you handle reference data (lookup tables such as countries or currencies) that multiple services need?](#q18-how-do-you-handle-reference-data-lookup-tables-such-as-countries-or-currencies-that-multiple-services-need)
19. [Q19. How do you configure separate EF Core DbContexts for multiple microservices in the same solution, each pointing to a different connection string?](#q19-how-do-you-configure-separate-ef-core-dbcontexts-for-multiple-microservices-in-the-same-solution-each-pointing-to-a-different-connection-string)
20. [Q20. How do you seed initial data and run database migrations automatically on service startup without causing deployment race conditions?](#q20-how-do-you-seed-initial-data-and-run-database-migrations-automatically-on-service-startup-without-causing-deployment-race-conditions)
21. [Q21. What connection string management strategies are recommended for microservices deployed in Docker or Kubernetes?](#q21-what-connection-string-management-strategies-are-recommended-for-microservices-deployed-in-docker-or-kubernetes)
22. [Q22. What is the Repository pattern in the context of a microservice, and how does it help keep the database implementation detail hidden from domain logic?](#q22-what-is-the-repository-pattern-in-the-context-of-a-microservice-and-how-does-it-help-keep-the-database-implementation-detail-hidden-from-domain-logic)
23. [Q23. How do you handle reporting and analytics queries that need data from multiple microservice databases?](#q23-how-do-you-handle-reporting-and-analytics-queries-that-need-data-from-multiple-microservice-databases)
24. [Q24. What are the operational costs of the Database per Service pattern, and how do teams manage them at scale?](#q24-what-are-the-operational-costs-of-the-database-per-service-pattern-and-how-do-teams-manage-them-at-scale)
25. [Q25. When is it acceptable to start with a shared database and split later, and what signals indicate it is time to split?](#q25-when-is-it-acceptable-to-start-with-a-shared-database-and-split-later-and-what-signals-indicate-it-is-time-to-split)

---

## Q1. What is the Database per Service pattern, and why is it considered a foundational principle of microservices architecture?

**Concepts**
- Database per Service — exclusive data store ownership
- Storage-layer coupling surviving API-layer decoupling
- Schema change as a hidden cross-service breaking change
- Service API as the sole data contract

**Answer**

The Database per Service pattern means each microservice exclusively owns and controls its own data store, and no other service is allowed to connect to it directly. It is foundational because services remain coupled at the storage layer even when their APIs appear decoupled — if Service A queries Service B's schema directly, any column rename or table split in B silently breaks A without any API contract violation surfacing in tests or CI. Since the service's database is a private implementation detail accessible only through its API, the owning team can change their schema, swap database engines, or deploy independently without coordinating with other teams. Without data isolation, microservices are decoupled at the API layer while still tightly coupled where it matters most.

---

## Q2. What is the Shared Database anti-pattern in microservices, and why does it undermine service autonomy?

**Concepts**
- Shared Database anti-pattern — hidden inter-service schema dependencies
- Schema migration coordination overhead
- Polyglot persistence blocked by shared engine
- Business rule fragmentation across shared tables

**Answer**

The Shared Database anti-pattern occurs when two or more microservices read from or write to the same database schema, whether by design or because a monolith was split into services without splitting its data layer. Any schema change — renaming a column, splitting a table, removing an index — must be coordinated across every team that accesses that database, which defeats the independent deployability microservices are meant to provide. Services that share a schema develop hidden dependencies: Service A's query may rely on a table structure that Service B owns and wants to evolve. Beyond coordination, shared access makes it impossible to replace one service's database engine without affecting all other services on the same instance, and business rule enforcement becomes fragmented because two services may apply inconsistent logic to the same rows.

---

## Q3. What are the main benefits and trade-offs of giving every microservice its own dedicated database?

**Concepts**
- Loose coupling and independent schema evolution
- API Composition replacing cross-service JOIN
- Saga and Outbox patterns for distributed consistency
- Operational cost scaling with number of services

**Answer**

The primary benefits of Database per Service are loose coupling, independent deployability, and the freedom to choose the right storage technology per domain. The main cost is that data that once lived in a single query must now be assembled from multiple service APIs, and keeping data consistent across services requires explicit coordination mechanisms. Schema changes are local and safe to deploy, each service can choose its own database engine, and failure in one database does not cascade to other services. Against those gains, cross-service reads require API composition, controlled data duplication is sometimes necessary, distributed consistency demands sagas and the outbox pattern, and the number of databases to operate and monitor grows linearly with service count. The pattern is the right default for teams working on genuinely independent domains; it becomes overhead when services are too fine-grained and constantly need each other's data.

---

## Q4. What is polyglot persistence, and how does the Database per Service pattern enable it?

**Concepts**
- Polyglot persistence — matching storage engine to domain access patterns
- API encapsulation as the enabler of engine diversity
- Specialized storage engines — document, relational, search, graph
- Shared database preventing engine freedom

**Answer**

Polyglot persistence means using different types of databases for different parts of a system — for example, a relational database for transactional order data, a document store for product catalogs, and a graph database for recommendation engines. The Database per Service pattern enables this directly because each service's data store is encapsulated behind its API, so different services can use entirely different storage engines without any other service needing to know. A Catalog service might use MongoDB to store flexible product schemas while an Orders service uses PostgreSQL for ACID transactions, and a Search service can mirror data into Elasticsearch optimized for full-text queries by consuming events from the source-of-truth services. With a shared database, polyglot persistence is impossible because all services must use the same engine, which is why data isolation is the prerequisite for letting teams choose the best fit for their domain.

---

## Q5. What are the three common levels of database isolation used in microservices (separate server, separate schema, shared schema with prefixed tables), and when is each appropriate?

**Concepts**
- Separate database server — maximum isolation and resource independence
- Separate schema or logical database — cost sharing with access boundary
- Shared schema with prefixed tables — migration stepping stone
- Isolation strength vs infrastructure cost trade-off

**Answer**

Database isolation in microservices sits on a spectrum. At the strongest end, each service runs on a completely separate database server instance — the right choice for production environments with sensitive data or teams that need true operational independence, since a networking outage or misconfiguration on one server cannot allow cross-service SQL access. In the middle, separate schemas or logical databases on the same server still prevent accidental JOIN queries across service boundaries while sharing infrastructure costs, which suits shared hosting or lower-budget environments that still need access control. At the weakest end, tables distinguished only by a naming prefix like `orders_*` and `catalog_*` share a schema — a tactical first step when splitting a monolith that should be treated as temporary since it still allows accidental coupling. The right level depends on the team's stage of adoption, compliance requirements, and the actual independence the domain demands.

---

## Q6. What is the difference between logical isolation (separate schemas/users) and physical isolation (separate database servers), and what does each guarantee?

**Concepts**
- Logical isolation — permission-level access control
- Physical isolation — resource contention and performance separation
- Defense-in-depth combining both layers

**Answer**

Logical isolation uses database-level constructs — separate schemas, separate user accounts with restricted permissions, or separate databases on the same server — to prevent one service from accessing another's data. Service B's credentials simply do not grant SELECT on Service A's schema, so accidental cross-service queries fail at runtime. Physical isolation goes further by running each service against a completely separate database server instance, which provides both access control and resource isolation so a database outage or runaway query for one service cannot degrade another service's database performance. Physical isolation is more expensive in infrastructure cost but is the correct choice for services with very different load profiles or for regulated data that must reside on separate infrastructure. Most production microservices architectures combine both: separate servers per team or domain cluster, with schema-level permissions as a defense-in-depth measure.

---

## Q7. How do you choose between a relational database and a NoSQL store for a specific microservice?

**Concepts**
- Relational databases — ACID transactions and complex stable schemas
- Document stores — flexible hierarchical schemas read as whole documents
- Key-value and cache stores — low-latency single-entity lookup
- Search engines — full-text and faceted query optimization

**Answer**

The choice depends on the service's data structure, query patterns, consistency requirements, and write volume. I choose a relational database like SQL Server or PostgreSQL when the service enforces complex business rules, requires multi-row ACID transactions, or has well-known stable schemas with joins across related entities. I choose a document store like MongoDB or Cosmos DB when entities have variable shape, are naturally hierarchical, and are usually read and written as a whole document rather than joined — since the document model avoids the impedance mismatch of mapping nested objects to normalized rows. A key-value or cache store like Redis fits when the service primarily needs low-latency lookups by a single identifier, such as session data or rate-limit counters. A search engine like Elasticsearch belongs to a read-optimized service that needs full-text search or relevance ranking, typically fed by events from the authoritative write service, because its inverted-index structure excels at queries that relational databases handle poorly.

---

## Q8. Why can microservices not share database tables or issue cross-database JOIN queries, and what is the consequence of allowing it?

**Concepts**
- Cross-database JOIN — structural schema coupling between services
- Deployment coordination forced by shared query dependencies
- Polyglot persistence blocked by cross-server joins
- Wrong service boundary signaled by frequent cross-joins

**Answer**

Cross-database joins couple services at the storage layer, meaning Service A's query logic depends on Service B's schema structure. Any change Service B makes to its schema — renaming a column, splitting a table, changing a data type — can silently break Service A's query without any API contract violation being caught by tests or CI pipelines, since the dependency is structural and invisible to the consumer's build. When a JOIN spans two services' databases, deployment of those two services is no longer independent: one service's schema migration must be coordinated with the other service's query. Cross-database joins also lock both services into the same database engine and server, eliminating polyglot persistence and independent scaling. If two services frequently need each other's data via joins, that is usually a signal that the service boundary is wrong — they may belong in the same service. The correct replacement is to expose data through APIs or events, which creates an explicit versioned contract instead of a hidden structural dependency.

---

## Q9. What is the API Composition pattern, and how does it replace cross-service database queries?

**Concepts**
- API Composition — in-memory merge of parallel service API calls
- Backend-for-Frontend and aggregator as composition hosts
- N+1 mitigation through ID batching per service
- Latency ceiling of the slowest downstream call

**Answer**

The API Composition pattern assembles a response that spans multiple services by calling each relevant service's API and merging the results in memory, either in an API Gateway or a dedicated aggregator service, replacing what would have been a single JOIN with a set of HTTP or gRPC calls to the owning services. An aggregator calls Service A for order data and Service B for customer data in parallel, then merges both results before returning a combined response, so the database boundary is never crossed. The pattern works well when the number of services to call is small and each returns a bounded result set — you avoid the N+1 problem by batching IDs in one call per service rather than calling per row. The main trade-off is latency: a composed response takes as long as the slowest downstream service call, whereas a single JOIN on small data sets is often faster, which means that for large-scale read requirements spanning hundreds of thousands of records, a dedicated event-driven read model is the better solution.

---

## Q10. What is a CQRS read model (materialized view) that spans multiple services, and how is it kept up to date?

**Concepts**
- CQRS read model — pre-computed denormalized projection for a specific query
- Event-sourced projection worker subscribing to multiple services
- Specialized read-side storage — Elasticsearch, Redis, or similar
- Eventual consistency — propagation lag between write and read model

**Answer**

A CQRS read model is a pre-computed, denormalized data store optimized for a specific query — for example, a single order summary document combining data from the Orders, Catalog, and Customer services. It is maintained by a dedicated projection worker that subscribes to integration events from each source service and updates its own private database whenever source data changes. When Service A publishes an `OrderPlaced` event and Service B publishes a `ProductPriceChanged` event, the read-side handler listens to both and updates the combined read model accordingly, so the UI can query one fast, optimized store rather than composing data from multiple APIs at request time. The read model database can be a completely different technology — such as Elasticsearch — optimized for the specific query the UI needs, and because it is rebuilt from events, it can always be reconstructed by replaying the event log, which makes read-side schema migrations simpler. The trade-off is that the read model is eventually consistent: there is a propagation lag between a write in a source service and the read model reflecting that change.

---

## Q11. What is data duplication in the context of microservices, and why is controlled duplication sometimes the correct design choice?

**Concepts**
- Controlled data duplication — snapshot semantics at transaction time
- Canonical data ownership in one authoritative service
- Event-driven subscriber updates on canonical change
- Duplication vs runtime coupling trade-off

**Answer**

Data duplication means a microservice stores a copy of data that is also owned by another service — for example, the Orders service storing the customer's name at the time of order placement rather than calling the Customer service every time an order is fetched. This controlled redundancy trades storage space for reduced inter-service coupling and improved read performance. Storing a snapshot at the time of a transaction is often the correct semantic: an order placed in 2023 should show the customer name from 2023, not the customer's current name if they changed it in 2025. The owning service notifies subscribers via events when the canonical data changes, and each subscriber updates its local copy on receipt of the event. The key discipline is that the copy must be acknowledged as a projection of the truth rather than the truth itself — ownership of the canonical version remains in one service, and the duplication is deliberate rather than accidental sharing.

---

## Q12. Why are distributed (two-phase commit) transactions impractical across microservice databases, and what replaces them?

**Concepts**
- Two-phase commit — coordinator lock and availability coupling
- XA protocol limitation across heterogeneous databases
- Saga pattern as the practical replacement
- Compensating transactions for failure-path rollback

**Answer**

Two-phase commit requires all participating database servers to hold locks and vote on whether to commit or roll back as a coordinated unit, which creates tight coupling between services and introduces a single point of failure at the coordinator. In a distributed microservices environment this degrades availability because all participants must be reachable before any can proceed — the exact opposite of the high-availability goal microservices are designed to achieve. Beyond availability, 2PC does not work across heterogeneous databases since not all engines implement the XA protocol, so a PostgreSQL and a MongoDB can never participate in the same distributed transaction. The correct replacement is the Saga pattern, which breaks a distributed transaction into a sequence of local transactions coordinated through events or an orchestrator, accepting that each step commits locally and that if a later step fails, compensating transactions undo the earlier steps. Compensating transactions must be explicitly designed for every failure path, which makes sagas more complex to reason about but far more resilient in practice.

---

## Q13. What is the Saga pattern, and how does it maintain data consistency across services that each own their own database?

**Concepts**
- Saga — sequence of local transactions across service boundaries
- Choreography vs orchestration implementation styles
- Compensating transactions reversing completed steps
- Idempotency requirement on compensating operations

**Answer**

The Saga pattern manages a long-running business transaction spanning multiple microservices by breaking it into a series of local transactions, each of which updates one service's database and then triggers the next step via an event or command. If any step fails, the saga executes compensating transactions in reverse order to undo the completed steps and leave the system in a consistent state. There are two implementation styles: choreography, where each service publishes an event that triggers the next service's local transaction with no central controller, and orchestration, where a central saga orchestrator sends commands to each service and tracks progress. Choreography is simpler to build but harder to debug because the transaction flow is implicit across event handlers in multiple services, while orchestration makes the flow explicit and easy to visualize but introduces an orchestrator that becomes a single point of coordination complexity. Compensating transactions must be idempotent so that if the same compensation is applied twice due to a retry, the result is identical to a single application.

---

## Q14. What is the Outbox Pattern, and why is it needed when a service must both save to its own database and publish an event?

**Concepts**
- Dual-write problem — database and broker as independent failure domains
- Outbox table — atomic write within a single local transaction
- At-least-once delivery from the relay process
- CDC as a relay mechanism alternative to polling

**Answer**

The Outbox Pattern solves the dual-write problem: if a service saves a record to its database and then publishes an event to a message broker in two separate operations, either can fail independently, leaving the system in an inconsistent state. The solution is to write both the domain record and the outbox event to the same database in a single local transaction, then have a separate background relay process publish the outbox events to the broker. Because both the business record and the outbox row are written in one transaction, either both succeed or both roll back — so a crash between the database write and the broker publish is no longer possible. The relay process (or a Change Data Capture tool like Debezium) polls or streams the outbox table and marks each event as sent after publishing, which guarantees at-least-once delivery: if the relay crashes after publishing but before marking the event as sent, it will publish again on restart, so consumers must handle duplicates idempotently.

---

## Q15. What is eventual consistency, and how do you design a user experience that tolerates it?

**Concepts**
- Eventual consistency — convergence window after a committed write
- Optimistic UI update masking propagation delay
- Strongly consistent read via the authoritative write-side service
- UX patterns acknowledging asynchronous processing

**Answer**

Eventual consistency means that after a write is committed to one service's database, other services that hold derived or copied data will reflect that change at some point in the future — typically milliseconds to seconds — but not immediately, since the system converges only after all events have propagated. A user who submits an order and immediately requests the orders list may not yet see their new order if the read model is updated asynchronously, because the propagation window exists even if it is brief. One UI design technique is optimistic updates: render the expected state on the client immediately after a successful write response, without waiting for the read model to confirm, so the user sees their change instantly. Systems can also acknowledge to the user that "your order is being processed" rather than "your order is confirmed" until the downstream read model catches up. Critical flows like payment confirmation should not rely on eventually consistent reads at all — they should call the authoritative write-side service directly to get a strongly consistent answer.

---

## Q16. How do you manage Entity Framework Core migrations in a microservice that owns its own database, especially in a containerized environment?

**Concepts**
- Per-service migration history — independent project and context isolation
- MigrateAsync concurrency safety via __EFMigrationsHistory locking
- Init container pattern — schema applied before service container starts
- DDL privilege separation — migration account vs runtime credentials

**Answer**

Each microservice maintains its own EF Core migration history in its own project, completely independent of other services, with migration files generated by `dotnet ef migrations add` scoped to that service's `DbContext`. In a containerized environment, the recommended approach is to apply migrations in a separate step before starting the service — either as a Kubernetes init container that runs `dotnet ef database update` and exits before the main container starts, or as a CI/CD pipeline stage — so the database is always at the correct schema version before the application begins accepting requests. When that is impractical, `DbContext.Database.MigrateAsync()` at startup is acceptable because EF Core's migration history table uses a database lock, so only one instance applies each migration while others wait and proceed once the lock is released. Migration scripts should always be backward-compatible with the previous service version during rolling deployments, since adding a nullable column is safe while dropping a column the old version reads is not, and the production database user for the running service should have read/write privileges but not the DDL rights that migrations require.

---

## Q17. What is a backward-compatible schema change, and why is it critical when services deploy independently?

**Concepts**
- Rolling deployment — old and new service versions coexisting on the same schema
- Backward-compatible vs breaking DDL changes
- Expand-and-contract migration phasing
- Zero-downtime deployment dependency on migration safety

**Answer**

A backward-compatible schema change is a migration that can be applied while the previous version of the service is still running — adding a nullable column, adding an index, or adding a new table. It is critical because rolling deployments mean the old and new versions of a service may run simultaneously against the same database for a period, and a breaking migration like dropping or renaming a column will crash the old running instances that still reference those structures. Adding a NOT NULL column with no default is a breaking change because old instances do not supply that column's value when inserting rows. The safe sequence for renaming a column is to add the new column, deploy a version that writes to both columns, backfill the new column, then drop the old column only in a later deployment after the old version is fully retired. This expand-and-contract approach takes more releases but ensures zero-downtime for services with live traffic, which is why EF Core migration files should be reviewed with the same rigor as code changes.

---

## Q18. How do you handle reference data (lookup tables such as countries or currencies) that multiple services need?

**Concepts**
- Reference Data service as canonical authoritative owner
- Static embedding for stable data avoiding runtime dependency
- Event-driven replication for moderately changing reference data
- Runtime coupling risk of synchronous per-request lookup

**Answer**

Reference data shared across services should be owned by one authoritative service and distributed to consumers through a mechanism chosen by how often the data changes. For truly stable data like ISO country codes or currency symbols, the simplest and most resilient approach is to embed it as a static seed file in each service's own database at startup, which avoids a runtime dependency on a central service entirely. For moderately changing reference data, the owning service publishes change events and each consumer subscribes to update its local copy, maintaining eventual consistency without blocking on a synchronous call. Requiring a synchronous call to a reference data service on every request creates a latency and availability dependency that undermines service resilience — if the reference data service is slow or unavailable, every dependent service degrades. When a service stores a copy, it should also store the identifier from the authoritative source so that updates can be applied by ID when an event arrives.

---

## Q19. How do you configure separate EF Core DbContexts for multiple microservices in the same solution, each pointing to a different connection string?

**Concepts**
- Per-service DbContext — private entity model per project
- AddDbContext registration with service-specific connection strings
- Polyglot persistence via different EF Core providers
- Connection string sourcing from environment or secrets management

**Answer**

In a solution containing multiple microservice projects, each project defines its own `DbContext` subclass and registers it in its own `Program.cs` using `AddDbContext<TContext>()` with the connection string read from that service's configuration. There is no shared `DbContext` across projects — each microservice treats its context and its database as a private implementation detail, which means no entity class should be shared between two services' contexts.

```csharp
// In OrdersService/Program.cs
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));

// In CatalogService/Program.cs
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDb")));
```

Using different database providers (SQL Server for one, PostgreSQL for another) demonstrates polyglot persistence and is fully supported since each context registers its own provider independently. Connection strings should be sourced from environment variables or secrets management such as Azure Key Vault rather than hard-coded in `appsettings.json`, since connection strings are sensitive configuration that changes per environment.

---

## Q20. How do you seed initial data and run database migrations automatically on service startup without causing deployment race conditions?

**Concepts**
- MigrateAsync concurrency safety via __EFMigrationsHistory lock
- Init container pre-deployment migration pattern
- HasData seeding vs idempotent seeder class with existence check
- DDL privilege isolation between migration and runtime credentials

**Answer**

The safest approach is to apply migrations in a separate step before starting the service — either as a Kubernetes init container, a Docker Compose `depends_on` condition, or a CI/CD pipeline stage — so the schema is ready before the application begins accepting requests. When a separate migration step is impractical, `MigrateAsync()` at startup is safe to call from multiple concurrently starting instances because EF Core's migration history table uses a database lock, meaning only one instance applies each migration while others wait and then proceed once the lock releases. Data seeding should use `HasData()` in `OnModelCreating` for static reference data or a separate seeder class invoked after `MigrateAsync()` that checks for existence before inserting, so re-running the seeder on restart does not duplicate rows. The migration account should have DDL privileges needed to create and alter tables, while the runtime application account should have only read/write data permissions, since granting DDL rights to a long-lived connection pool increases the blast radius of a SQL injection vulnerability.

---

## Q21. What connection string management strategies are recommended for microservices deployed in Docker or Kubernetes?

**Concepts**
- Environment variable injection — Docker Compose and Kubernetes
- Kubernetes Secret vs ConfigMap for sensitive vs non-sensitive configuration
- Azure Key Vault / HashiCorp Vault IConfiguration integration
- Credential rotation without redeployment via connection pool reset

**Answer**

In containerized microservices, connection strings must never be hard-coded in configuration files checked into source control because those files travel with every container image and pull request. The standard approach is to inject them at runtime via environment variables — in Docker Compose through the service's `environment:` block, and in Kubernetes via Secret objects referenced as environment variables in the Pod spec. Kubernetes Secrets should be used for connection strings rather than ConfigMaps, since ConfigMaps are not encrypted at rest on most cluster configurations. For production-grade secret management, Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault integrates with the .NET `IConfiguration` pipeline via provider libraries, so the application reads secrets through the same `builder.Configuration["Key"]` pattern used locally. Credentials can be rotated without redeploying services by storing only the secret name in configuration and having the application re-read the secret value on each connection pool reset.

---

## Q22. What is the Repository pattern in the context of a microservice, and how does it help keep the database implementation detail hidden from domain logic?

**Concepts**
- Repository interface — abstraction hiding EF Core from domain and application layers
- Clean Architecture layering — interface in domain, implementation in infrastructure
- Unit testability via in-memory or mock repository
- IQueryable leakage defeating the EF Core abstraction

**Answer**

The Repository pattern defines a collection-like interface for accessing domain entities — for example, `IOrderRepository` with methods like `GetByIdAsync` and `SaveAsync` — and places the EF Core implementation behind that interface in the infrastructure layer. Domain logic and application services depend only on the interface, so the underlying database, ORM, or query mechanism can be swapped or upgraded without modifying the domain layer, which is the key principle of Clean Architecture separation. Unit tests for domain logic can inject a mock or in-memory implementation of the repository interface without needing a real database connection, which dramatically speeds up the test suite. The repository also ensures that all database access for an aggregate root goes through one place, making it straightforward to add cross-cutting concerns like caching, query logging, or retry policies. The one trap to avoid is making the repository a thin wrapper around `DbSet<T>` that leaks EF Core types such as `IQueryable<T>` to callers — that defeats the abstraction by coupling the caller to EF Core's query model.

---

## Q23. How do you handle reporting and analytics queries that need data from multiple microservice databases?

**Concepts**
- Event-driven data warehouse pipeline — domain events into analytical store
- CDC (Change Data Capture) for non-invasive database streaming
- Read-optimized reporting service with denormalized tables
- Protecting operational databases from heavy analytical query load

**Answer**

Reporting and analytics across multiple services are handled by a dedicated read layer separate from the operational microservices, since running aggregation queries directly against transactional databases degrades the performance of live services. The most common approach is an event-driven pipeline: services publish domain events to a message broker, and a consumer writes them to a data warehouse optimized for aggregation queries such as Azure Synapse, Snowflake, or BigQuery. An alternative is Change Data Capture tooling like Debezium, which streams changes from each service's database at the transaction log level without requiring code changes to the source service, though it requires access to the source database's replication stream. The reporting layer maintains its own eventually consistent copy and does not call into the operational services' APIs for bulk data, which protects transactional performance and means analytical workloads can scale independently from the write path.

---

## Q24. What are the operational costs of the Database per Service pattern, and how do teams manage them at scale?

**Concepts**
- Infrastructure as Code for consistent, automated database provisioning
- Centralized observability aggregating per-service database metrics
- Service granularity signaled by database count proliferation
- Per-service backup and restore runbooks

**Answer**

The main operational costs are the number of databases to provision, monitor, back up, and patch — which grows linearly with the number of services — and the complexity of tracing data inconsistencies across multiple stores when something goes wrong. Teams manage these costs through infrastructure automation, centralized observability, and careful service boundary design. Database infrastructure is typically provisioned via Infrastructure as Code using Terraform or Bicep so each new service gets a consistently configured, monitored database without manual setup. Centralized database monitoring through Azure Monitor, Datadog, or Prometheus with database exporters aggregates metrics from all service databases into one dashboard, reducing the burden of many separate instances. Backup and restore policies should be codified and tested as part of each service's runbook rather than assumed to be in place. The number of separate databases is also a forcing function for service granularity: if a team finds itself operating dozens of small databases for trivial services, that is a signal that some services should be merged.

---

## Q25. When is it acceptable to start with a shared database and split later, and what signals indicate it is time to split?

**Concepts**
- Shared database as pragmatic start for unclear domain boundaries
- Schema migration coordination overhead as the primary split signal
- Separate schemas as an intermediate isolation step
- DDD Bounded Context guiding service and database boundary decisions

**Answer**

Starting with a shared database is a pragmatic choice for early-stage products where the domain boundaries are not yet well understood, since splitting prematurely locks you into the wrong boundaries and creates expensive data migrations. The right time to split is when teams working on different parts of the schema are regularly stepping on each other's migrations, or when one service's database load is noticeably affecting another service's performance. If two teams frequently need to coordinate schema migrations because their tables are in the same database, that coordination cost is a clear signal to split. When a service becomes a scaling bottleneck and you want to move it to a different database instance for resource isolation, splitting the data is necessary to do so. A useful intermediate step is to separate schemas or user accounts on the same server first — this enforces access boundaries and removes accidental cross-service joins while deferring the infrastructure cost of separate servers. Domain-Driven Design Bounded Contexts are the conceptual guide: each Bounded Context maps naturally to one service and one database, so if two schemas feel like they belong to the same Bounded Context, they probably should not be split yet.

---

## Gotchas — Database per Service (Interview Traps)

---

#### Gotcha 1. Cross-Service Join Implemented as API Call N+1

**Concepts**
- Report requiring data from three services making three API calls per row
- N+1 HTTP calls per page of results instead of one database join
- Data denormalisation into read-model projections solving the join
- API composition gateway aggregating in parallel, not sequentially

**Answer**

A reporting query that joins orders, customers, and products — data owned by three separate services — cannot use a database join and must fetch each service's data separately. A naive implementation fetches 100 orders, then makes 100 HTTP calls to CustomerService for each customer, and 100 calls to ProductService for each product — 300 HTTP round trips instead of one SQL join. The solution is to denormalise the data needed for reporting into a read-model projection that each service populates via domain events, so the read model has all fields in one queryable table. For ad-hoc aggregation, an API composition layer fetches all required data in parallel with `Task.WhenAll` rather than sequentially, but this still cannot beat a database join for complex filtering and sorting.

---

#### Gotcha 2. Eventual Consistency Confusing the UI

**Concepts**
- UI showing stale data after a command because the read model hasn't updated
- Customer seeing the order they just placed as not existing
- "Read your own writes" pattern returning command result immediately
- Optimistic UI update while projection catches up

**Answer**

When a user places an order and immediately navigates to the order list, the read-model projection that backs the list may not yet have processed the `OrderPlacedEvent` — the user sees a list that does not include the order they just created. Eventual consistency is inherent when the write and read databases are separate and updated asynchronously. The standard mitigation is to include the created resource in the command response body: the `POST /orders` response returns the full `OrderDto` directly from the write side so the UI can display it immediately without querying the eventually-consistent read model. This "return what you wrote" approach eliminates the visible inconsistency for the most common case.

---

#### Gotcha 3. No Data Ownership Rule Leading to Duplication Without Authority

**Concepts**
- Same entity (Customer) stored by multiple services with no owner
- Updates to the authoritative source not propagated to copies
- One service as the System of Record per entity type
- Integration events propagating changes from owner to consumers

**Answer**

When both OrderService and ShippingService store customer address data and there is no defined rule for which service owns the authoritative copy, an address update in CustomerService (the logical owner) may not be propagated to either service — both end up with stale addresses. The rule is: one service is the System of Record for each piece of data; every other service that needs the data holds a read-only copy populated via integration events. CustomerService owns `Customer.Address` and publishes `CustomerAddressChangedEvent` when it changes; OrderService and ShippingService consume this event and update their local copies. Data duplication is acceptable and expected; undefined ownership is the defect.

---

#### Gotcha 4. Using a Shared Database With Per-Schema Ownership and Calling It "Database per Service"

**Concepts**
- Schema-per-service still sharing database engine, credentials, and connection pool
- Cross-schema foreign key still possible via SQL
- Schema isolation as an intermediate step, not the pattern
- True isolation requiring separate database instances or clusters

**Answer**

Placing each service's tables in a separate schema on the same database instance provides name isolation and prevents accidental joins — but it is not Database per Service. All schemas share the same database engine, the same connection pool, the same compute resources, and the same credentials if not managed carefully. A DBA can still write `SELECT * FROM orders.OrderLines JOIN inventory.Products ON ...` bypassing the inter-service API boundary entirely. Database per Service requires separate database instances (separate servers, or separate managed database services) so that one service's query load, migrations, and outages cannot affect another service. Schema separation is a valid intermediate step when splitting a shared database incrementally, not the end state.

---

#### Gotcha 5. Schema Migration Not Coordinated With Rolling Deployment

**Concepts**
- Destructive migration running while old service version still active
- Old code breaking when a column is renamed or removed
- Expand-Contract migration pattern for zero-downtime schema changes
- Feature flag controlling code path switch after migration completes

**Answer**

A schema migration that renames column `customer_id` to `customerId` runs against the database while old service instances in the rolling update are still reading `customer_id` — those old instances immediately start failing with "column not found" errors before the new instances have taken over. The Expand-Contract pattern prevents this: first deploy an Additive migration that adds the new column `customerId` alongside the old one (Expand); then deploy new code that writes to both columns; then remove reads from the old column; then deploy a second migration that drops the old column (Contract). This three-phase approach ensures old and new service versions can coexist during the deployment window without schema conflicts.

---

#### Gotcha 6. Reporting Query Spanning All Services With No Read Model

**Concepts**
- Business report requiring aggregation across 5 services
- API composition too slow and fragile for complex queries
- Dedicated reporting database aggregated from all services
- CQRS read model or data warehouse as the solution

**Answer**

A quarterly business report that combines data from OrderService, CustomerService, ProductService, InventoryService, and PaymentService cannot be built efficiently by making API calls and joining results in application code — filtering, sorting, aggregating, and paginating across millions of records requires database-level operations that are impossible via API composition. The solution is a dedicated reporting service or data warehouse that subscribes to integration events from all source services and builds a denormalised reporting schema optimised for query patterns. The reporting database is a read-only secondary that can use any query technology (SQL analytics, OLAP cube, search index) without affecting the operational service databases.

---

#### Gotcha 7. Service Splitting Along Technical Lines Instead of Domain Boundaries

**Concepts**
- Splitting by technology (all reads in one service, all writes in another)
- Tight business coupling remaining despite the split
- DDD Bounded Context as the split criterion
- Chatty inter-service calls revealing a misdrawn boundary

**Answer**

Splitting a service along technical lines — one service handles database reads and another handles writes, or one service per entity type (OrderService, OrderLineService, OrderStatusService) — creates technically separate services that are still tightly coupled in business terms. The OrderLineService calling OrderService on every operation means the services need to be deployed together, tested together, and changed together, which provides none of the organisational independence that Database per Service is designed to achieve. Service and database boundaries must follow domain boundaries (Bounded Contexts): an entire order concept — lines, status, payment — belongs in one OrderManagement service if they must always be consistent and are managed by the same team.

---

#### Gotcha 8. Cross-Service Transaction Using a Shared Database Transaction

**Concepts**
- Cross-service database transaction creating tight deployment coupling
- Distributed transaction failing when one service restarts
- Saga pattern replacing distributed transactions
- Local transaction per service as the unit of atomicity

**Answer**

Two services sharing a database connection and participating in a single `BEGIN TRANSACTION / COMMIT` achieve atomicity at the cost of tight coupling — both services must use the same database type, both must be available simultaneously, and neither can be deployed independently. When one service restarts mid-transaction, the transaction is rolled back and the other service's operation is lost without compensation. The correct pattern is to treat each service's local database transaction as the unit of atomicity and use the Saga pattern to coordinate multi-service workflows with compensating transactions — each service commits its own transaction and publishes an event, and the Saga handles failures by triggering compensation.

---

#### Gotcha 9. Integration Tests Using All Services' Live Databases

**Concepts**
- Integration test modifying shared production or staging database
- Test data polluting other services' data or being affected by it
- Per-test database isolation using Docker Compose test environments
- TestContainers providing ephemeral isolated databases per test run

**Answer**

An integration test that writes to the live staging database of OrderService and then calls CustomerService — which reads from its own staging database — introduces cross-service test data dependencies, test pollution, and flakiness caused by other tests or developers modifying the shared data simultaneously. Each service's integration tests must use an isolated database instance — TestContainers spins up a Docker container with a fresh database for each test run, and Docker Compose provides a complete isolated environment for cross-service integration tests with no shared state between runs. Integration tests should never touch shared staging or production databases.

---

#### Gotcha 10. Saga Not Used When a Multi-Service Business Operation Requires Consistency

**Concepts**
- Multi-service write failing halfway and leaving data inconsistent
- No compensation mechanism to roll back committed steps
- Saga as the required pattern for multi-service consistency
- Manual cleanup in support tickets as the alternative anti-pattern

**Answer**

An order placement flow that calls PaymentService, InventoryService, and ShippingService in sequence with no Saga — using direct HTTP calls without compensation — will leave the system inconsistent if ShippingService fails after PaymentService has already charged the card and InventoryService has reserved the stock. With no automated compensation, the support team manually cancels the payment via the payment gateway and releases the inventory reservation — operational work that the Saga should perform automatically. Any business operation that modifies data in more than one service must use the Saga pattern or be redesigned so all data lives in the same service. The lack of a Saga is not a minor omission; it is a missing reliability guarantee for the entire business operation.

---
