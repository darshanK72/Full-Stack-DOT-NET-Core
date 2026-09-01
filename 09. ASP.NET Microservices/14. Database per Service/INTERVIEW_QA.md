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

What is the Database per Service pattern, and why is it considered a foundational principle of microservices architecture?

**Answer:** The Database per Service pattern means each microservice owns and exclusively controls its own data store — no other service is allowed to connect to it directly. This boundary enforces true service autonomy: a service can change its schema, swap its database engine, or be deployed independently without coordinating with other teams. It is considered foundational because without data isolation, microservices remain coupled at the data layer even if their APIs are decoupled.

- A service's database is treated as a private implementation detail, exposed only through the service's API, not through a shared connection string.
- Independent data ownership lets teams choose the most appropriate storage technology for their domain rather than being constrained by a single central database.
- The pattern forces teams to design explicit contracts (events and APIs) for sharing data, which makes cross-service dependencies visible and manageable.
- Without this isolation, a schema change in one service's table can silently break another service's SQL query — exactly the tight coupling microservices aim to eliminate.

---

## Q2. What is the Shared Database anti-pattern in microservices, and why does it undermine service autonomy?

What is the Shared Database anti-pattern in microservices, and why does it undermine service autonomy?

**Answer:** The Shared Database anti-pattern occurs when two or more microservices read from or write to the same database schema, whether intentionally or because a monolith was split into services without splitting its data layer. This means any schema change — adding a column, renaming a table, removing an index — must be coordinated across every team that touches that database, defeating the independence microservices are meant to provide.

- Services that share a database develop hidden dependencies: Service A's query may depend on a table structure that Service B owns and wants to change.
- Shared database access makes it impossible to replace one service's database engine (for example, moving from SQL Server to Redis) without affecting all other services connected to the same instance.
- Because any service can write to any table, business rule enforcement becomes fragmented — two services may apply inconsistent logic to the same data.
- Teams lose the ability to deploy services independently because a database migration must be backward-compatible with every consumer simultaneously.

---

## Q3. What are the main benefits and trade-offs of giving every microservice its own dedicated database?

What are the main benefits and trade-offs of giving every microservice its own dedicated database?

**Answer:** The primary benefits of Database per Service are loose coupling, independent deployability, and the freedom to choose the right data technology per domain. The main cost is that data that once lived in a single query must now be assembled from multiple service APIs, and keeping data consistent across services requires explicit coordination mechanisms like sagas and the outbox pattern.

| Benefit | Trade-off |
|---|---|
| Schema changes are local and safe | Cross-service queries require API composition |
| Each service can choose its own DB engine | Data duplication is sometimes necessary |
| Services deploy and scale independently | Distributed consistency is more complex |
| Failure in one DB does not cascade | More databases to operate and monitor |

The pattern is the right default for teams working on genuinely independent domains; it becomes overhead if services are too fine-grained and constantly need each other's data.

---

## Q4. What is polyglot persistence, and how does the Database per Service pattern enable it?

What is polyglot persistence, and how does the Database per Service pattern enable it?

**Answer:** Polyglot persistence means using different types of databases for different parts of a system — for example, a relational database for transactional order data, a document store for product catalogs, and a graph database for recommendation engines. The Database per Service pattern enables this directly because each service's data store is encapsulated behind its API, so different services can use entirely different storage engines without any other service needing to know.

- A Catalog service might use MongoDB to store flexible product schemas, while an Orders service uses PostgreSQL for ACID (Atomicity, Consistency, Isolation, Durability) transactions.
- A Search service can mirror data into Elasticsearch optimized for full-text queries, consuming events from the source-of-truth services.
- With a shared database, polyglot persistence is impossible because all services must use the same engine and schema.
- Polyglot persistence requires teams to be intentional about which technology fits each domain's read/write patterns and query requirements.

---

## Chapter 2: Isolation Strategies

---

## Q5. What are the three common levels of database isolation used in microservices (separate server, separate schema, shared schema with prefixed tables), and when is each appropriate?

What are the three common levels of database isolation used in microservices (separate server, separate schema, shared schema with prefixed tables), and when is each appropriate?

**Answer:** Database isolation in microservices sits on a spectrum from the strongest guarantee (completely separate database server) to the weakest (tables in a shared schema distinguished only by a naming prefix). The right level depends on the team's stage of adoption, budget, and the actual independence required between services.

| Level | Description | When to use |
|---|---|---|
| Separate database server | Each service gets its own database instance | Production environments; sensitive data; true team autonomy |
| Separate schema / database on shared server | Same server, different logical database or schema | Shared hosting, lower cost; still prevents cross-service joins |
| Shared schema, prefixed tables | Tables like `orders_*` and `catalog_*` in the same schema | Early-stage migration from monolith; lowest operational cost |

- Separate servers provide the strongest isolation because a networking outage or misconfiguration cannot allow cross-service SQL access.
- Separate schemas still prevent accidental JOIN queries across service boundaries while sharing infrastructure costs.
- Prefixed tables are a tactical first step when splitting a monolith but should be treated as temporary — they still allow accidental coupling.

---

## Q6. What is the difference between logical isolation (separate schemas/users) and physical isolation (separate database servers), and what does each guarantee?

What is the difference between logical isolation (separate schemas/users) and physical isolation (separate database servers), and what does each guarantee?

**Answer:** Logical isolation uses database-level constructs — separate schemas, separate user accounts with restricted permissions, or separate databases on the same server — to prevent one service from accessing another's data. Physical isolation runs each service against a completely separate database server instance, providing both access control and resource isolation (CPU, memory, disk I/O) so one service cannot starve another.

- Logical isolation enforces the boundary at the permission level: Service B's credentials simply do not grant SELECT on Service A's schema, so accidental cross-service queries fail at runtime.
- Physical isolation goes further: a database outage or runaway query for one service cannot degrade another service's database performance.
- Physical isolation is more expensive in infrastructure cost but is the correct choice for services with very different load profiles or for regulated data that must reside on separate infrastructure.
- Most production microservices architectures combine both: separate servers per team or domain cluster, with schema-level permissions as a defense-in-depth measure.

---

## Q7. How do you choose between a relational database and a NoSQL store for a specific microservice?

How do you choose between a relational database and a NoSQL store for a specific microservice?

**Answer:** The choice depends on the service's data structure, query patterns, consistency requirements, and write volume. Relational databases excel at structured data with complex relationships, multi-entity transactions, and ad-hoc queries; NoSQL stores excel at flexible schemas, high write throughput, hierarchical documents, or key-value lookups.

- Choose a relational database (SQL Server, PostgreSQL) when the service enforces complex business rules, requires multi-row ACID transactions, or has well-known, stable schemas.
- Choose a document store (MongoDB, Cosmos DB) when the service's entities have variable shape, are naturally hierarchical, and are usually read and written as a whole document rather than via joins.
- Choose a key-value or cache store (Redis) when the service primarily needs low-latency lookups of a single entity by identifier — for example, session data or rate-limit counters.
- Choose a search engine (Elasticsearch) for a read-optimized service that needs full-text search, faceted filtering, or relevance ranking, typically fed by events from the authoritative write service.

---

## Chapter 3: Cross-Service Data Access

---

## Q8. Why can microservices not share database tables or issue cross-database JOIN queries, and what is the consequence of allowing it?

Why can microservices not share database tables or issue cross-database JOIN queries, and what is the consequence of allowing it?

**Answer:** Cross-database joins couple services at the storage layer, meaning Service A's query logic depends on Service B's schema structure. Any change Service B makes to its schema — renaming a column, splitting a table, changing a data type — can silently break Service A's query without any API contract violation being caught by tests or CI pipelines.

- When a JOIN spans two services' databases, the deployment of those two services is no longer independent: one service's schema migration must be coordinated with the other service's query.
- Cross-database joins also lock both services into the same database engine and server, eliminating polyglot persistence and independent scaling.
- Allowing such joins is usually a sign that the service boundary is wrong: if two services frequently need each other's data via joins, they may belong in the same service.
- The correct replacement is to expose data through APIs or events, which creates an explicit, versioned contract instead of a hidden structural dependency.

---

## Q9. What is the API Composition pattern, and how does it replace cross-service database queries?

What is the API Composition pattern, and how does it replace cross-service database queries?

**Answer:** The API Composition pattern assembles a response that spans multiple services by calling each relevant service's API and merging the results in memory, either in an API Gateway or in a dedicated aggregator service. It replaces the database JOIN with a sequence (or parallel set) of HTTP or gRPC calls to the owning services.

- An API Gateway or Backend-for-Frontend (BFF) calls Service A for order data and Service B for customer data in parallel, then merges both results before returning a combined response to the client.
- The pattern works well when the number of services to call is small and each call returns a bounded result set — it avoids the N+1 problem by batching IDs in one call per service.
- The main trade-off is latency: a composed response takes as long as the slowest downstream service call, whereas a single JOIN is often faster on small data sets.
- For large-scale read requirements (hundreds of thousands of records across services), API composition is too slow, and a dedicated read model built from events is the better solution.

---

## Q10. What is a CQRS read model (materialized view) that spans multiple services, and how is it kept up to date?

What is a CQRS read model (materialized view) that spans multiple services, and how is it kept up to date?

**Answer:** A CQRS (Command Query Responsibility Segregation) read model is a pre-computed, denormalized data store optimized for a specific query — for example, a single "order summary" document that combines data from the Orders, Catalog, and Customer services. It is maintained by a dedicated read-side service (or a projection worker) that subscribes to integration events from each source service and updates its own private database whenever the source data changes.

- When Service A publishes an `OrderPlaced` event and Service B publishes a `ProductPriceChanged` event, the read-side projection handler listens to both and updates the combined read model accordingly.
- The read model database can be a completely different technology — for example, Elasticsearch — optimized for the specific query the UI needs.
- Because the read model is rebuilt from events, it can always be rebuilt from scratch by replaying the event log, which makes schema migrations on the read side simpler.
- The read model is eventually consistent: there is a propagation lag between a write in a source service and the read model reflecting that change.

---

## Q11. What is data duplication in the context of microservices, and why is controlled duplication sometimes the correct design choice?

What is data duplication in the context of microservices, and why is controlled duplication sometimes the correct design choice?

**Answer:** Data duplication means a microservice stores a copy of data that is also owned by another service — for example, the Orders service storing the customer's name at the time of order placement rather than calling the Customer service every time an order is fetched. This controlled redundancy trades storage space for reduced inter-service coupling and improved read performance.

- Storing a snapshot of data at the time of a transaction is often the correct semantic: an order placed in 2023 should show the customer name from 2023, not the customer's current name if they changed it in 2025.
- Duplicating slowly changing reference data (country names, currency codes) into a service's own database avoids a runtime dependency on the authoritative service for every read.
- The owning service notifies subscribers via events when the canonical data changes; each subscriber updates its local copy on receipt of the event.
- Duplication must be deliberate: the copy is acknowledged as a projection of the truth, not the truth itself, and the ownership of the canonical version remains in one service.

---

## Chapter 4: Distributed Data Consistency

---

## Q12. Why are distributed (two-phase commit) transactions impractical across microservice databases, and what replaces them?

Why are distributed (two-phase commit) transactions impractical across microservice databases, and what replaces them?

**Answer:** Two-phase commit (2PC) requires all participating database servers to hold locks and vote on whether to commit or roll back as a coordinated unit. In a distributed microservices environment this creates tight coupling between services, introduces a single point of failure at the coordinator, and degrades availability because all participants must be reachable before any can proceed. The correct replacement is the Saga pattern, which breaks a distributed transaction into a sequence of local transactions coordinated through events or an orchestrator.

- 2PC does not work across heterogeneous databases (for example, one service on PostgreSQL and another on MongoDB) because not all engines implement the XA protocol.
- A failure partway through 2PC leaves the system blocked waiting for the coordinator to recover, reducing availability — which violates the high-availability goals of microservices.
- The Saga pattern accepts that each step commits locally; if a later step fails, compensating transactions undo the earlier steps' effects.
- Compensating transactions must be explicitly designed for every failure path, which makes sagas more complex to reason about but far more resilient in practice.

---

## Q13. What is the Saga pattern, and how does it maintain data consistency across services that each own their own database?

What is the Saga pattern, and how does it maintain data consistency across services that each own their own database?

**Answer:** The Saga pattern manages a long-running business transaction that spans multiple microservices by breaking it into a series of local transactions, each of which updates one service's database and then triggers the next step via an event or command. If any step fails, the saga executes compensating transactions in reverse order to undo the completed steps and leave the system in a consistent state.

- There are two implementation styles: choreography (each service publishes an event that triggers the next service's local transaction — no central controller) and orchestration (a central saga orchestrator sends commands to each service and tracks progress).
- Choreography is simpler to build but harder to debug because the transaction flow is implicit across event handlers in multiple services.
- Orchestration makes the transaction flow explicit and easy to visualize but introduces an orchestrator service that becomes a single point of coordination complexity.
- Compensating transactions must be idempotent: if the same compensation is applied twice (due to a retry), the result must be the same as if it were applied once.

---

## Q14. What is the Outbox Pattern, and why is it needed when a service must both save to its own database and publish an event?

What is the Outbox Pattern, and why is it needed when a service must both save to its own database and publish an event?

**Answer:** The Outbox Pattern solves the dual-write problem: if a service saves a record to its database and then tries to publish an event to a message broker in two separate operations, either can fail independently, leaving the system in an inconsistent state. The solution is to write both the domain record and the outbox event to the same database in a single local transaction, then have a separate background process relay the outbox events to the broker.

- Without the outbox pattern, a crash between the database write and the broker publish leaves the record saved but the event never sent — downstream services never learn about the change.
- The outbox table stores the serialized event as a row in the same database. Because both the business record and the outbox row are written in one transaction, either both succeed or both roll back.
- A relay process (or a Change Data Capture tool like Debezium) polls or streams the outbox table and publishes each unsent event to the message broker, marking it as sent afterward.
- This pattern guarantees at-least-once delivery: if the relay crashes after publishing but before marking the event as sent, it will publish again on restart, so consumers must handle duplicates idempotently.

---

## Q15. What is eventual consistency, and how do you design a user experience that tolerates it?

What is eventual consistency, and how do you design a user experience that tolerates it?

**Answer:** Eventual consistency means that after a write is committed to one service's database, other services that hold derived or copied data will reflect that change at some point in the future — but not immediately. The system will converge to a consistent state once all events have propagated, but there is a window where different parts of the system show different values.

- A user who submits an order and immediately requests the orders list may not yet see their order if the read model is updated asynchronously — the window is typically milliseconds to seconds, but it exists.
- One UI design technique is optimistic updates: render the expected state on the client immediately after a successful write response, without waiting for the read model to confirm.
- Systems can acknowledge to the user that "your order is being processed" rather than "your order is confirmed" until the downstream read model catches up.
- Critical flows (for example, payment confirmation) should not rely on eventually consistent reads; they should call the authoritative write-side service directly to get a strongly consistent answer.

---

## Chapter 5: Schema Management and Evolution

---

## Q16. How do you manage Entity Framework Core migrations in a microservice that owns its own database, especially in a containerized environment?

How do you manage Entity Framework Core migrations in a microservice that owns its own database, especially in a containerized environment?

**Answer:** Each microservice maintains its own EF Core migration history in its project, completely independent of other services. In a containerized environment, the recommended approach is to apply migrations automatically on startup using `DbContext.Database.MigrateAsync()`, or as a dedicated init container that runs before the service container starts, so the database is always at the correct schema version before the application begins accepting requests.

- Each service's migration files live in its own project and are generated with `dotnet ef migrations add` scoped to that service's `DbContext`.
- Running `MigrateAsync()` at startup is the simplest approach for small teams; it serializes startup until migrations complete and requires that the application's database user has DDL permissions.
- For larger teams, a separate migration-runner init container or a CI/CD pipeline step applies migrations before deploying the new service version, keeping the application's runtime credentials read/write but not DDL-privileged.
- Migration scripts should be backward-compatible with the previous service version during a rolling deployment window — adding a nullable column is safe; dropping a column that the old version reads is not.

---

## Q17. What is a backward-compatible schema change, and why is it critical when services deploy independently?

What is a backward-compatible schema change, and why is it critical when services deploy independently?

**Answer:** A backward-compatible schema change is a database migration that can be applied while the previous version of the service is still running — for example, adding a nullable column, adding an index, or adding a new table. It is critical in microservices because rolling deployments mean the old and new versions of a service may run simultaneously against the same database for a period, and a breaking migration (such as dropping or renaming a column) will crash the old running instances.

- Adding a NOT NULL column with no default is a breaking change: old instances do not know to supply that column's value when they insert rows.
- The safe sequence for renaming a column is: add the new column, deploy a version that writes both columns, backfill the new column, drop the old column in a later deployment after the old version is retired.
- This phased migration approach ("expand and contract") takes more releases but ensures zero-downtime deployments for services with live traffic.
- EF Core migration files should be reviewed as carefully as code changes because a bad migration can cause an irrecoverable production outage if the rollback path requires restoring a column that already has data.

---

## Q18. How do you handle reference data (lookup tables such as countries or currencies) that multiple services need?

How do you handle reference data (lookup tables such as countries or currencies) that multiple services need?

**Answer:** Reference data shared across services should be owned by one authoritative service (for example, a Reference Data service) and distributed to consumers through one of three mechanisms: synchronous API calls at runtime, events published when the data changes (allowing services to maintain local copies), or a static configuration file embedded in each service's deployment for truly static data. The choice depends on how often the data changes and how tightly services can tolerate coupling to a runtime dependency.

- For rarely changing data (country codes, currency ISO codes), embed a static file or seed data in each service's own database at startup — avoiding a runtime dependency on a central service.
- For moderately changing reference data, the owning service publishes change events; consumers subscribe and update their local copy, maintaining eventual consistency.
- Avoid requiring a synchronous call to a reference data service on every request — this creates a latency and availability dependency that undermines service resilience.
- When a service stores a copy of reference data, it should also store the identifier used by the authoritative source so that updates can be applied by ID.

---

## Chapter 6: Implementation in .NET and ASP.NET Core

---

## Q19. How do you configure separate EF Core DbContexts for multiple microservices in the same solution, each pointing to a different connection string?

How do you configure separate EF Core DbContexts for multiple microservices in the same solution, each pointing to a different connection string?

**Answer:** In a solution containing multiple microservice projects, each project defines its own `DbContext` subclass and registers it in its own `Program.cs` using `builder.Services.AddDbContext<TContext>()` with a connection string read from that service's configuration. There is no shared `DbContext` across projects — each microservice treats its context and its database as a private implementation detail.

```csharp
// In OrdersService/Program.cs
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersDb")));

// In CatalogService/Program.cs
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDb")));
```

- Each `DbContext` defines only the entities that belong to that service; no entity class is shared between two services' contexts.
- Connection strings are sourced from environment variables or secrets management (Azure Key Vault, Kubernetes Secrets) rather than hard-coded in `appsettings.json`.
- Using different database providers (SQL Server for one, PostgreSQL for another) demonstrates polyglot persistence and is fully supported because each context registers its own provider.

---

## Q20. How do you seed initial data and run database migrations automatically on service startup without causing deployment race conditions?

How do you seed initial data and run database migrations automatically on service startup without causing deployment race conditions?

**Answer:** The safest approach is to apply migrations in a separate step before starting the service — either as a Kubernetes init container, a Docker Compose `depends_on` condition, or a CI/CD pipeline stage. When that is impractical, `MigrateAsync()` at startup is acceptable if the service is designed so that only one instance runs migrations (achieved by database-level locking that EF Core's migration history table already provides via `__EFMigrationsHistory`).

- EF Core's `MigrateAsync()` is safe to call concurrently from multiple instances because the migration history table uses a database lock — only one instance will apply each migration; others will wait and then proceed once the lock is released.
- Data seeding should use `HasData()` in `OnModelCreating` for static reference data, or a separate seeder class invoked after `MigrateAsync()` that checks for existence before inserting.
- Init containers are the cleaner pattern for Kubernetes: a container image that only runs `dotnet ef database update` exits cleanly before the main service container starts, ensuring the schema is ready.
- Never run migrations with a production service account that has DDL privileges at runtime; use a separate migration account with elevated rights only for the migration step.

---

## Q21. What connection string management strategies are recommended for microservices deployed in Docker or Kubernetes?

What connection string management strategies are recommended for microservices deployed in Docker or Kubernetes?

**Answer:** In containerized microservices, connection strings must never be hard-coded in configuration files checked into source control. The standard approach is to inject them at runtime via environment variables in Docker or via Kubernetes Secrets mounted as environment variables or volume files, with the application reading them through the .NET configuration system.

- In Docker Compose, set the connection string as an environment variable in the service's `environment:` block, overriding whatever is in `appsettings.json`.
- In Kubernetes, store the connection string (or its password component) in a Secret object and reference it as an environment variable in the Pod spec — never in a ConfigMap, which is not encrypted at rest.
- For production-grade secret management, use Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault integrated with the .NET `IConfiguration` pipeline via a provider library.
- Rotate database credentials without redeploying services by storing only the secret name in configuration and having the application re-read the secret value on each connection pool reset.

---

## Q22. What is the Repository pattern in the context of a microservice, and how does it help keep the database implementation detail hidden from domain logic?

What is the Repository pattern in the context of a microservice, and how does it help keep the database implementation detail hidden from domain logic?

**Answer:** The Repository pattern defines a collection-like interface for accessing domain entities — for example, `IOrderRepository` with methods like `GetByIdAsync` and `SaveAsync` — and places the EF Core implementation behind that interface. Domain logic and application services depend only on the interface, so the underlying database, ORM, or query mechanism can be changed without modifying the domain layer.

- The interface lives in the domain or application layer; the EF Core implementation lives in the infrastructure layer, following Clean Architecture layering.
- Unit tests for domain logic can inject a mock or in-memory implementation of the repository interface without needing a real database connection.
- The repository ensures that all database access for an aggregate root goes through one place, making it easy to add cross-cutting concerns like caching, query logging, or retry policies.
- Avoid making the repository a thin wrapper around `DbSet<T>` that leaks EF Core types (like `IQueryable<T>`) into the caller — this defeats the abstraction and couples the caller to EF Core.

---

## Chapter 7: Design Trade-offs and Operational Concerns

---

## Q23. How do you handle reporting and analytics queries that need data from multiple microservice databases?

How do you handle reporting and analytics queries that need data from multiple microservice databases?

**Answer:** Reporting and analytics across multiple services are typically handled by a dedicated read layer that is separate from the operational microservices. The standard approaches are an event-driven data warehouse (services publish events that are consumed into a central analytics store), a read-optimized reporting service that subscribes to all relevant events and maintains denormalized tables, or Change Data Capture (CDC) tooling like Debezium that streams database changes from each service into a central analytical database.

- An event-driven pipeline publishes domain events to a message broker; a consumer writes them to a data warehouse (for example, Azure Synapse, Snowflake, BigQuery) optimized for aggregation queries.
- The reporting layer is an independent read model and does not call into the operational services' APIs for bulk data — it maintains its own eventually consistent copy.
- This separation protects operational databases from expensive analytical queries that would degrade the transactional performance of the live services.
- CDC tooling captures changes at the database transaction log level, which is non-invasive (no code changes to the source service) but requires access to the source database's replication stream.

---

## Q24. What are the operational costs of the Database per Service pattern, and how do teams manage them at scale?

What are the operational costs of the Database per Service pattern, and how do teams manage them at scale?

**Answer:** The main operational costs are the number of databases to provision, monitor, back up, and patch — which grows linearly with the number of services — and the complexity of tracing data inconsistencies across multiple stores when something goes wrong. Teams manage these costs through infrastructure automation, centralized observability, and careful service boundary design that avoids proliferating services unnecessarily.

- Database infrastructure is typically provisioned via Infrastructure as Code (Terraform, Bicep) so that each new service gets a consistently configured, monitored database without manual setup.
- Centralized database monitoring (Azure Monitor, Datadog, Prometheus with database exporters) aggregates metrics from all service databases into one dashboard, reducing the operational burden of many separate instances.
- Backup and restore policies should be codified and tested as part of each service's runbook, not assumed to be in place.
- The number of separate databases is a forcing function for service granularity: if a team finds itself operating dozens of small databases for trivial services, it is a signal that some services should be merged.

---

## Q25. When is it acceptable to start with a shared database and split later, and what signals indicate it is time to split?

When is it acceptable to start with a shared database and split later, and what signals indicate it is time to split?

**Answer:** Starting with a shared database is a pragmatic choice for early-stage products where the domain boundaries are not yet well understood — splitting prematurely locks you into the wrong boundaries and creates expensive data migrations. The right time to split is when teams working on different parts of the schema are regularly stepping on each other's migrations, or when one service's database load is noticeably affecting another service's performance.

- If two teams frequently need to coordinate schema migrations because their tables are in the same database, that coordination cost is a clear signal to split.
- When a service becomes a scaling bottleneck and you want to move it to a different database instance for resource isolation, splitting the data is necessary to do so.
- A useful intermediate step is to separate schemas or user accounts on the same server first — this enforces access boundaries and removes accidental cross-service joins while deferring the infrastructure cost of separate servers.
- Domain-Driven Design (DDD) Bounded Contexts are the conceptual guide: each Bounded Context maps naturally to one service and one database; if two schemas feel like they belong to the same Bounded Context, they probably should not be split yet.

---
