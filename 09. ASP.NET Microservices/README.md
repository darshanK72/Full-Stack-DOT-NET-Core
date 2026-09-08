# 09. ASP.NET Microservices

Clean Architecture, CQRS, DDD, event-driven, API Gateway, Docker, Kubernetes, messaging, Saga, resilience.

## Topics

| # | Topic | Questions | Q&A File |
|---|-------|-----------|----------|
| 01 | Clean Architecture | 30 | [README.md](./01.%20Clean%20Architecture/README.md) |
| 02 | Interview Questions — CQRS Pattern | 24 | [README.md](./02.%20CQRS%20Pattern/README.md) |
| 03 | Domain-Driven Design | 32 | [README.md](./03.%20Domain-Driven%20Design/README.md) |
| 04 | Interview Questions — Event-Driven Architecture | 23 | [README.md](./04.%20Event-Driven%20Architecture/README.md) |
| 05 | Interview Questions — API Gateway Pattern | 30 | [README.md](./05.%20API%20Gateway%20Pattern/README.md) |
| 06 | Service Discovery & Configuration | 20 | [README.md](./06.%20Service%20Discovery%20%26%20Configuration/README.md) |
| 07 | Interview Questions — Docker & Containerization | 18 | [README.md](./07.%20Docker%20%26%20Containerization/README.md) |
| 08 | Kubernetes Orchestration | 29 | [README.md](./08.%20Kubernetes%20Orchestration/README.md) |
| 09 | Interview Questions — Distributed Messaging | 12 | [README.md](./09.%20Distributed%20Messaging/README.md) |
| 10 | Saga Pattern | 22 | [README.md](./10.%20Saga%20Pattern/README.md) |
| 11 | Resilience & Circuit Breaker | 32 | [README.md](./11.%20Resilience%20%26%20Circuit%20Breaker/README.md) |
| 12 | Observability & Distributed Tracing | 36 | [README.md](./12.%20Observability%20%26%20Distributed%20Tracing/README.md) |
| 13 | Interview Questions — gRPC Inter-Service Communication | 25 | [README.md](./13.%20gRPC%20Inter-Service%20Communication/README.md) |
| 14 | Interview Questions — Database per Service | 25 | [README.md](./14.%20Database%20per%20Service/README.md) |

---

> Each Q&A file has a **Table of Contents** at the top linking to every question.
> Files that include scenario-based Karat questions have a **Scenario-Based Questions** section at the bottom.

---

## Cross-Topic Interview Q&A

## About This File

Each subfolder above addresses one topic in depth. This file asks the questions that cannot be
answered by looking at a single topic in isolation — questions that deliberately span two or more
of those areas. The six questions below reflect the kinds of design conversations that come up in
senior and staff-level interviews: "You've described CQRS and event-driven architecture — how do
they actually connect across service boundaries?" or "You mentioned sagas and circuit breakers in
the same sentence — how do they interact?"

Every answer references .NET 10 idioms and tooling. Concepts bullets name the precise ideas an
interviewer expects to hear; the Answer paragraph is the flowing, conversational explanation that
shows you understand not just the mechanism but the production trade-offs.

**Topics spanned by each question**

| # | Primary | Secondary | Tertiary |
|---|---------|-----------|---------|
| CQ1 | CQRS (02) | Event-Driven Architecture (04) | Database per Service (14) |
| CQ2 | Saga Pattern (10) | Distributed Messaging (09) | Resilience (11) |
| CQ3 | API Gateway (05) | Service Discovery (06) | Kubernetes (08) |
| CQ4 | Clean Architecture (01) | DDD (03) | CQRS (02) |
| CQ5 | Observability (12) | gRPC (13) | Resilience (11) |
| CQ6 | Docker (07) | Kubernetes (08) | Configuration (06) |
| CQ7 | Event-Driven Architecture (04) | CQRS (02) | — |
| CQ8 | API Gateway (05) | Auth | — |
| CQ9 | Event-Driven Architecture (04) | Distributed Messaging (09) | — |
| CQ10 | DDD (03) | Database per Service (14) | — |
| CQ11 | Docker (07) | Kubernetes (08) | Resilience (11) |
| CQ12 | Resilience (11) | Service Discovery/Mesh (06/08) | — |
| CQ13 | Observability (12) | gRPC (13) | Event-Driven Architecture (04) |
| CQ14 | Kubernetes (08) | Configuration (06) | Database per Service (14) |
| CQ15 | Saga Pattern (10) | Event-Driven Architecture (04) | — |
| CQ16 | Clean Architecture (01) | DDD (03) | Testing |
| CQ17 | API Gateway (05) | Service Discovery (06) | Kubernetes (08) |
| CQ18 | Event-Driven Architecture (04) | CQRS (02) | Database per Service (14) |

---

## Table of Contents

- [CQ1. How do CQRS write-side domain events become integration events that enable eventual consistency across service boundaries with Database-per-Service?](#cq1-how-do-cqrs-write-side-domain-events-become-integration-events-that-enable-eventual-consistency-across-service-boundaries-with-database-per-service)
- [CQ2. How does a saga orchestrate a multi-step distributed transaction over a message bus, and how do circuit breakers interact with saga retry and compensation logic?](#cq2-how-does-a-saga-orchestrate-a-multi-step-distributed-transaction-over-a-message-bus-and-how-do-circuit-breakers-interact-with-saga-retry-and-compensation-logic)
- [CQ3. How does Kubernetes-native service discovery change the role of an API Gateway, and what production pitfall arises when health checks do not propagate through the gateway?](#cq3-how-does-kubernetes-native-service-discovery-change-the-role-of-an-api-gateway-and-what-production-pitfall-arises-when-health-checks-do-not-propagate-through-the-gateway)
- [CQ4. How do DDD aggregates map to Clean Architecture's domain layer, and why should CQRS command handlers return only IDs or result DTOs rather than domain entities?](#cq4-how-do-ddd-aggregates-map-to-clean-architectures-domain-layer-and-why-should-cqrs-command-handlers-return-only-ids-or-result-dtos-rather-than-domain-entities)
- [CQ5. How do distributed traces propagate across gRPC calls, and what is the production pitfall when a circuit breaker opens inside an active trace span?](#cq5-how-do-distributed-traces-propagate-across-grpc-calls-and-what-is-the-production-pitfall-when-a-circuit-breaker-opens-inside-an-active-trace-span)
- [CQ6. What is the progression from docker-compose environment variables to Kubernetes ConfigMaps and Secrets, and how does ASP.NET Core's configuration provider chain unify them?](#cq6-what-is-the-progression-from-docker-compose-environment-variables-to-kubernetes-configmaps-and-secrets-and-how-does-aspnet-cores-configuration-provider-chain-unify-them)
- [CQ7. How does event sourcing replace row-based persistence on the CQRS write side, and how does event upcasting handle schema evolution in a long-lived event store?](#cq7-how-does-event-sourcing-replace-row-based-persistence-on-the-cqrs-write-side-and-how-does-event-upcasting-handle-schema-evolution-in-a-long-lived-event-store)
- [CQ8. Where should JWT tokens be validated in a microservices system, and what is the audience-mismatch gotcha when the gateway forwards the raw Authorization header?](#cq8-where-should-jwt-tokens-be-validated-in-a-microservices-system-and-what-is-the-audience-mismatch-gotcha-when-the-gateway-forwards-the-raw-authorization-header)
- [CQ9. What is the dual-write problem, and how does the transactional outbox pattern with a CDC-based relay solve it more reliably than a polling loop?](#cq9-what-is-the-dual-write-problem-and-how-does-the-transactional-outbox-pattern-with-a-cdc-based-relay-solve-it-more-reliably-than-a-polling-loop)
- [CQ10. How do DDD bounded context boundaries determine service and database ownership, and how do you serve cross-context queries without joining across schemas?](#cq10-how-do-ddd-bounded-context-boundaries-determine-service-and-database-ownership-and-how-do-you-serve-cross-context-queries-without-joining-across-schemas)
- [CQ11. How do Kubernetes liveness and readiness probes interact with a Polly circuit breaker, and what health check design prevents a service with an open circuit from appearing healthy?](#cq11-how-do-kubernetes-liveness-and-readiness-probes-interact-with-a-polly-circuit-breaker-and-what-health-check-design-prevents-a-service-with-an-open-circuit-from-appearing-healthy)
- [CQ12. When should retry and circuit-breaker policies live in application code versus a service mesh, and how do you prevent a retry storm when both layers are active?](#cq12-when-should-retry-and-circuit-breaker-policies-live-in-application-code-versus-a-service-mesh-and-how-do-you-prevent-a-retry-storm-when-both-layers-are-active)
- [CQ13. How does W3C TraceContext propagate a single user request's trace across synchronous gRPC calls and asynchronous message flows?](#cq13-how-does-w3c-tracecontext-propagate-a-single-user-requests-trace-across-synchronous-grpc-calls-and-asynchronous-message-flows)
- [CQ14. How does Kubernetes Secret injection enforce database-per-service isolation, and what is the critical gotcha of baking a connection string into a Docker image layer?](#cq14-how-does-kubernetes-secret-injection-enforce-database-per-service-isolation-and-what-is-the-critical-gotcha-of-baking-a-connection-string-into-a-docker-image-layer)
- [CQ15. What is the difference between compensating and undo transactions in saga rollback, and what happens when a compensating event arrives before the forward event on an unordered broker?](#cq15-what-is-the-difference-between-compensating-and-undo-transactions-in-saga-rollback-and-what-happens-when-a-compensating-event-arrives-before-the-forward-event-on-an-unordered-broker)
- [CQ16. How does Clean Architecture's dependency inversion make the domain layer testable in isolation, and what is the role of in-memory infrastructure fakes in application-layer tests?](#cq16-how-does-clean-architectures-dependency-inversion-make-the-domain-layer-testable-in-isolation-and-what-is-the-role-of-in-memory-infrastructure-fakes-in-application-layer-tests)
- [CQ17. Why does gRPC load balancing fail silently behind a Kubernetes ClusterIP Service, and how do headless Services or an API gateway solve it?](#cq17-why-does-grpc-load-balancing-fail-silently-behind-a-kubernetes-clusterip-service-and-how-do-headless-services-or-an-api-gateway-solve-it)
- [CQ18. How do you handle client reads that arrive during the lag between a write command completing and the CQRS read model being updated, and what is the 'read your own writes' pattern?](#cq18-how-do-you-handle-client-reads-that-arrive-during-the-lag-between-a-write-command-completing-and-the-cqrs-read-model-being-updated-and-what-is-the-read-your-own-writes-pattern)

---

## CQ1. How do CQRS write-side domain events become integration events that enable eventual consistency across service boundaries with Database-per-Service?

**Concepts**
- Domain events raised inside an aggregate are internal to the bounded context
- Integration events cross service boundaries and must be schema-stable versioned contracts
- The outbox pattern atomically persists the write-model change and the pending integration event
- A background relay publishes outbox rows to the broker after the transaction commits
- Each consuming service projects the integration event into its own independent read store
- Write-model schema changes must not silently break published event contracts (the coupling gotcha)

**Answer**

When a CQRS command handler drives an aggregate to completion, the aggregate raises domain events —
for example `OrderPlaced` — as in-memory notifications. Those events are internal to the service
and can carry rich domain types freely. To cross a service boundary they must be translated into
integration events: flat, versioned, schema-stable DTOs that external consumers depend on.

The recommended bridge is the outbox pattern: inside the same database transaction that persists
the aggregate state, the application layer also writes a row to an `OutboxMessages` table. A
background hosted service polls that table and publishes each row to the broker exactly once,
guaranteeing at-least-once delivery without a two-phase commit. Downstream services — each owning
their own database — consume the integration event and update their local read projections.

The critical production gotcha is contract coupling. Because external consumers bind to the event
schema, any structural rename in the write model that leaks into the integration event silently
breaks consumers. The correct discipline is to map domain events to explicitly versioned integration
event records in the application layer, version the contract via a namespace or a `$schema`
property, and produce a new event type for breaking changes rather than mutating the existing one.
In .NET 10 this is typically handled by a dedicated `IntegrationEventMapper` in the application
layer, keeping the domain model free to evolve without touching the published contract.

---

## CQ2. How does a saga orchestrate a multi-step distributed transaction over a message bus, and how do circuit breakers interact with saga retry and compensation logic?

**Concepts**
- A saga replaces 2PC with a sequence of local transactions and compensating transactions
- Orchestration-based sagas use an explicit state machine (e.g., MassTransit `MassTransitStateMachine`)
- Each saga step publishes a command and awaits a success or failure event over the bus
- Compensating transactions undo already-completed steps in reverse order on failure
- Message handlers must be idempotent to tolerate at-least-once delivery from the broker
- A circuit breaker throws `BrokenCircuitException` before a saga step publishes, causing a timeout cascade

**Answer**

In an orchestration-based saga a central state machine sends a command to each downstream service
over the message bus and waits for a success or failure response event. MassTransit's
`MassTransitStateMachine<TSagaState>` models this as explicit states — `Pending`,
`InventoryReserved`, `PaymentCharged`, `Completed`, `Compensating` — with transitions driven by
incoming events. When a step fails, the saga transitions to `Compensating` and issues compensating
commands in reverse order. Because the bus provides at-least-once delivery, every step handler must
be idempotent: it checks whether the operation was already applied (using a correlation ID stored in
the local database) before executing again.

The interaction with circuit breakers is subtle and frequently misunderstood. A Polly
`CircuitBreakerStrategy` wrapping an outbound HTTP call inside a saga step will throw
`BrokenCircuitException` before the step publishes its success event to the bus. Without explicit
handling, the saga times out waiting for that event and triggers the full compensation sequence.
This is technically correct behavior, but compensation steps themselves may also encounter the same
open circuit, causing them to fail in turn — leaving the saga in a partially compensated state.
The recommended fix is to treat `BrokenCircuitException` as a retryable fault at the MassTransit
consumer level rather than a definitive failure: the message is redelivered with exponential
back-off (configured via `UseMessageRetry`) so the saga waits until the circuit closes before
deciding whether to compensate. This prevents spurious compensation of transient infrastructure
outages while preserving the saga's ability to compensate genuine business failures.

---

## CQ3. How does Kubernetes-native service discovery change the role of an API Gateway, and what production pitfall arises when health checks do not propagate through the gateway?

**Concepts**
- Kubernetes kube-dns and ClusterIP Services replace client-side service registries (Consul, Eureka)
- A stable DNS name per Service (e.g., `order-service.default.svc.cluster.local`) handles load balancing
- An API Gateway still provides auth offloading, rate limiting, routing, and protocol translation
- YARP in .NET 10 can watch the Kubernetes endpoints API for live upstream membership updates
- K8s readiness probes remove unhealthy pods from the Service endpoints slice before the gateway sees them
- A stale gateway upstream pool diverges from K8s endpoints, causing silent 502 errors for external callers

**Answer**

In a bare-metal or docker-compose setup, client-side service discovery requires a registry that
each client queries to find healthy instances. Kubernetes replaces this with kube-dns and ClusterIP
Services: a stable DNS name resolves to a virtual IP backed by kube-proxy, which load-balances
across all ready pods automatically. Client-side discovery libraries such as Consul clients become
unnecessary for intra-cluster traffic; the platform handles it transparently.

An API Gateway (YARP, Ocelot, or a managed product) still earns its place for concerns that
Kubernetes does not address: authentication offloading, rate limiting per consumer, request
aggregation across multiple upstream services, external TLS termination, and protocol translation
(HTTP/1.1 to HTTP/2 or gRPC). The gateway operates at the edge where the platform's built-in
load balancing is not visible to external callers.

The production pitfall is health-check propagation. K8s readiness probes remove a failing pod from
the Service's endpoints slice within seconds. But the gateway maintains its own upstream pool,
typically populated from a static config file or a slow-polling discovery mechanism. If the gateway
caches route targets, it continues to forward requests to pods that K8s has already excluded,
producing connection-refused or 502 errors that appear in the gateway's logs with no clear cause.
The fix in YARP is to configure an `ActiveHealthCheckPolicy` or wire a Kubernetes endpoint watcher
so the gateway's cluster membership stays synchronized with the endpoints API in real time rather
than relying on periodic polls or static configuration.

---

## CQ4. How do DDD aggregates map to Clean Architecture's domain layer, and why should CQRS command handlers return only IDs or result DTOs rather than domain entities?

**Concepts**
- Clean Architecture's domain layer contains aggregates, value objects, domain events, and domain services
- Command handlers in the application layer orchestrate domain objects but own no business rules themselves
- Returning a domain entity from a command crosses the read/write boundary and leaks the write model
- Write-side aggregates are shaped for invariant enforcement, not for display or API serialization
- Result records (`PlaceOrderResult(Guid OrderId)`) decouple callers from aggregate internals
- Reads use separate query handlers that bypass the domain layer and project from the read store directly

**Answer**

In Clean Architecture the innermost domain layer contains the DDD building blocks: aggregate roots,
entities, value objects, domain events, and domain services. An `Order` aggregate enforces its own
invariants — a line item cannot be added after the order is confirmed — and raises domain events as
part of its state transitions. The aggregate knows nothing about HTTP, databases, or message
brokers; it is a pure in-memory object graph protected by private setters and factory methods.

Command handlers in the application layer are orchestrators: they load the aggregate via a
repository interface, call domain methods, save the updated aggregate, and dispatch raised domain
events. The application layer owns no business rules of its own; it merely sequences the work.

A common and consequential mistake is having a command handler return the full aggregate root or an
EF Core entity. This leaks the write model: the caller holds a domain object shaped for enforcement
rather than display, and any change to the aggregate's internal structure immediately breaks
callers. The cascade gets worse when the aggregate is serialized in an API response — the HTTP
contract is now implicitly tied to domain internals.

The correct pattern is to return a lightweight result record — `PlaceOrderResult(Guid OrderId)` or
simply `Unit` if no data is needed. Reads are handled by entirely separate query handlers that
bypass the domain layer and project directly from the read store or a denormalized view. This
discipline preserves the read/write separation that CQRS promises and keeps the domain layer free
to evolve without worrying about breaking read-side consumers. In .NET 10 the idiomatic approach
is returning `record PlaceOrderResult(Guid OrderId)` from the command handler, then using a
separate `GetOrderQuery` backed by Dapper or EF Core projections for all read paths.

---

## CQ5. How do distributed traces propagate across gRPC calls, and what is the production pitfall when a circuit breaker opens inside an active trace span?

**Concepts**
- W3C TraceContext (`traceparent`, `tracestate`) headers carry trace and span IDs across service calls
- gRPC metadata headers function identically to HTTP headers for propagation purposes
- `OpenTelemetry.Instrumentation.GrpcNetClient` in .NET 10 injects and extracts trace context automatically
- A circuit breaker open throws `BrokenCircuitException` before a child span is created for the downstream call
- The parent span records only the local exception, hiding the upstream service's actual failure reason
- OpenTelemetry baggage carries a correlation ID end-to-end even when the call graph is broken by a circuit open

**Answer**

OpenTelemetry propagates distributed traces using W3C TraceContext: the `traceparent` header
carries the trace ID, parent span ID, and sampling flag; `tracestate` carries vendor-specific
metadata. In gRPC these values travel as metadata entries rather than HTTP headers, but the
propagation mechanism is identical. The `OpenTelemetry.Instrumentation.GrpcNetClient` package in
.NET 10 automatically injects the current activity's trace context into outgoing gRPC metadata and
extracts it on the server side, creating a parent-child span relationship across service
boundaries. Both sides record duration, status codes, and exceptions within their respective spans,
giving observability tooling (Jaeger, Zipkin, Azure Monitor) a connected trace tree.

The production pitfall arises when a Polly circuit breaker is open at the moment a gRPC call is
attempted. The circuit breaker throws `BrokenCircuitException` before the gRPC channel even sends
the request. Because no outbound call was made, the instrumentation library never creates a child
span for the downstream service. The parent span records only the local `BrokenCircuitException`,
making the trace tree look like an isolated local failure with no upstream context. An on-call
engineer sees a broken span with a generic error and cannot tell whether the upstream service is
actually down, degraded, or whether the circuit was tripped by a completely unrelated earlier
failure.

The two-part mitigation is: first, add a span event to the current activity when the circuit opens
(`Activity.Current?.AddEvent(new("CircuitBreakerOpen", tags: ...))`) so the circuit state is
visible in the trace; second, use OpenTelemetry baggage to propagate a correlation ID end-to-end.
Baggage survives a circuit open because it is attached to the ambient activity context rather than
the outbound request — the upstream service's own traces can then be correlated manually even when
the call graph is structurally broken.

---

## CQ6. What is the progression from docker-compose environment variables to Kubernetes ConfigMaps and Secrets, and how does ASP.NET Core's configuration provider chain unify them?

**Concepts**
- docker-compose `environment:` keys set process env vars read directly by `AddEnvironmentVariables()`
- Secrets in compose files or `docker inspect` output are visible in plain text — acceptable locally only
- Kubernetes ConfigMaps hold non-sensitive key-value pairs as env vars or volume-mounted files
- Kubernetes Secrets hold sensitive values projected the same way but stored separately, RBAC-controlled
- `AddKeyPerFile(path)` reads a directory of files where each filename is a config key — matches K8s Secret mounts
- The recommended chain: `appsettings.json` → env vars → K8s Secret volume → Azure Key Vault (last wins)

**Answer**

In docker-compose, configuration is straightforward: the `environment:` block in `docker-compose.yml`
sets process environment variables, and `builder.Configuration.AddEnvironmentVariables()` in
ASP.NET Core picks them up automatically. Secrets passed this way are visible in the compose file,
in the repository history if accidentally committed, and in `docker inspect` output — acceptable
for local development but a security liability in any shared or production environment.

Kubernetes formalizes the distinction between configuration and secrets. ConfigMaps hold
non-sensitive key-value pairs such as feature flags, base URLs, and connection string templates;
they can be projected as environment variables or as volume-mounted files in the pod. Secrets hold
sensitive values such as passwords, API keys, and certificates; they are stored separately from
ConfigMaps, can be encrypted at rest with a KMS provider, and access is governed by Kubernetes
RBAC and service account bindings. Both are projected to the pod in the same way — as env vars
or as a directory of files — so the consuming application sees a uniform interface.

ASP.NET Core's `IConfiguration` provider chain exploits this uniformity. The recommended layering
for .NET 10 microservices is: `appsettings.json` supplies compile-time defaults; environment
variables override per-environment settings; a Kubernetes Secret mounted as a directory
(e.g., `/run/secrets`) is read by `AddKeyPerFile("/run/secrets", optional: true)`, where each
filename becomes a config key and its text content becomes the value; Azure Key Vault adds a final
authoritative layer for cross-environment and cross-cluster secrets via
`AddAzureKeyVault(vaultUri, new DefaultAzureCredential())`. Because `IConfiguration` resolves the
last-registered provider that declares a key, this chain gives a clean override hierarchy with no
code changes across environments. The critical rule is that secrets must never be embedded in
container images: baking a password into an image turns every registry push into a credential leak
and makes rotation require a full image rebuild.

---

## CQ7. How does event sourcing replace row-based persistence on the CQRS write side, and how does event upcasting handle schema evolution in a long-lived event store?

**Concepts**
- Event sourcing stores an immutable sequence of domain events rather than a mutable current-state row
- An aggregate is rebuilt by replaying its event stream; snapshots cap replay cost for long-lived streams
- The CQRS write side appends events; the read side projects them into denormalized view models
- EventStoreDB or a SQL `Events` table with `StreamId`, `Version`, and `Payload` columns serves as the store
- Schema evolution gotcha: adding a required field to an old event type breaks deserialization of historical events
- Event upcasting converts old payloads to the current schema at read time, preserving the immutable history

**Answer**

When event sourcing is used as the persistence model for the CQRS write side, an `Order` aggregate
is never stored as a single updatable row. Instead, every state change — `OrderPlaced`,
`ItemAdded`, `OrderConfirmed` — is appended as an immutable record to the aggregate's event
stream. To reconstruct the aggregate the application replays all events in stream order, calling
`Apply(event)` on each. For long-lived aggregates, periodic snapshots capture a point-in-time
state so replay begins from the snapshot rather than the beginning of the stream, keeping load
time bounded.

The read side maintains its own projection by subscribing to the same event stream. A background
projection handler reads new events and updates a denormalized SQL or document view, cleanly
separating write-model complexity from query performance.

The production gotcha is schema evolution. Because the event store is append-only, old events
cannot be migrated in place. If a new required property is added to `OrderPlaced`, replaying
events written a year earlier will fail deserialization or produce incomplete aggregates. The
solution is event upcasting: a chain of converter functions transforms old event payloads to the
latest schema at read time, allowing the store to remain immutable while the application model
evolves. In .NET 10 this is typically implemented as a registered `IEventUpcast<TOld, TCurrent>`
pipeline that runs during deserialization before the aggregate's `Apply` method is invoked. The
discipline required is that a new required field must never be added without supplying a default
value in the upcast; optional-first, then required-after-full-replay-is-confirmed is the safe
migration sequence.

---

## CQ8. Where should JWT tokens be validated in a microservices system, and what is the audience-mismatch gotcha when the gateway forwards the raw Authorization header?

**Concepts**
- Centralized validation at the gateway offloads JWT parsing and JWKS key rotation from every downstream service
- Services behind the gateway can accept a simplified trusted internal header (`X-User-Id`, `X-User-Claims`)
- The BFF (Backend-for-Frontend) pattern creates a dedicated gateway per client type, tailoring auth flows per consumer
- The `aud` claim ties a token to a specific resource server; a token issued for `orders-api` should fail at `inventory-api`
- Forwarding the raw `Authorization` header lets a token pass audience validation at a service it was not issued for
- Defense-in-depth: validate signature and expiry at the gateway; validate `aud` at each downstream service too

**Answer**

JWT validation can be centralized at the gateway, distributed across each service, or both — each
choice has trade-offs. Centralizing at the gateway simplifies downstream services: they no longer
need to fetch a JWKS endpoint or parse claims. Instead the gateway validates the token, extracts
user identity, and forwards trusted internal headers such as `X-User-Id` or a base64-encoded
`X-User-Claims` JSON object. This works well when the gateway is the sole external entry point and
internal network isolation prevents pods from being reached directly.

The BFF (Backend-for-Frontend) pattern extends the gateway concept by creating a dedicated facade
per client type — a mobile BFF, a web BFF — each performing auth and aggregation tailored to its
consumer's specific needs and permission scope.

The audience-mismatch gotcha arises when the gateway forwards the raw
`Authorization: Bearer <token>` header without stripping or replacing it. A token issued for
`orders-api` carries `aud: orders-api`. If `inventory-service` also accepts raw JWTs and omits
audience validation, it will process a token never intended for it. A compromised or poorly
written service could exploit this to act on behalf of the caller in a context the caller was
never authorized for. The defense-in-depth fix is to validate both signature and audience at every
layer: the gateway validates signature and expiry for fast rejection; each service also validates
that `aud` matches its own identifier. In .NET 10 this is configured per service with
`AddAuthentication().AddJwtBearer(o => o.TokenValidationParameters.ValidAudience = "inventory-api")`.
Services that accept the simplified internal header disable `AddJwtBearer` entirely and instead
validate the trusted header using middleware, relying on network policy to ensure only the gateway
can set it.

---

## CQ9. What is the dual-write problem, and how does the transactional outbox pattern with a CDC-based relay solve it more reliably than a polling loop?

**Concepts**
- Dual-write: persisting to a DB and publishing to a broker are two operations with no shared transaction
- A crash between the two leaves state committed but the event unpublished, or vice versa
- The outbox pattern writes the event as a row in the same DB transaction as the domain change — atomically
- A polling relay reads unprocessed outbox rows and publishes them; marks them published to complete the cycle
- CDC (Change Data Capture) reads the database replication log instead of polling, giving near-real-time delivery
- At-least-once delivery requires consumer idempotency; the outbox `MessageId` is the deduplication key

**Answer**

Every event-driven microservice eventually confronts the dual-write problem: a command handler must
both persist a state change to the database and publish an event to the message broker. If the
application commits the database transaction and then crashes before publishing, the event is
silently lost. If it publishes first and then crashes before committing, the state change is
absent while downstream services believe it succeeded. No two-phase commit practically spans a
relational database and a message broker in a microservices architecture.

The outbox pattern eliminates the gap by writing the event payload as a row in an `OutboxMessages`
table inside the same database transaction as the domain state change. Both the domain row and the
outbox row commit atomically or roll back together — the dual-write problem disappears at the cost
of adding a relay process.

A polling relay reads unprocessed outbox rows at a configurable interval, publishes each to the
broker, and marks rows as published. This works but introduces polling latency and additional query
load on the database. A CDC-based relay — Debezium watching the Postgres write-ahead log, or SQL
Server change tracking — reacts to the replication log the moment a row is written, giving
sub-second latency with minimal database overhead because it reads the existing replication stream
rather than issuing new queries. In .NET 10 applications built with MassTransit, the
`UseInboxOutbox()` extension wires both sides automatically with EF Core as the outbox store. The
relay side is handled by the MassTransit transport (RabbitMQ, Azure Service Bus). Regardless of
relay mechanism, consumers must be idempotent because the relay guarantees at-least-once, not
exactly-once, delivery — the outbox `MessageId` column is the natural deduplication key.

---

## CQ10. How do DDD bounded context boundaries determine service and database ownership, and how do you serve cross-context queries without joining across schemas?

**Concepts**
- A bounded context is the linguistic boundary within which a domain model and ubiquitous language are consistent
- One service per bounded context enforces model isolation; one database per service enforces data ownership
- Two services sharing a schema are coupled at the data layer — a schema change in one can break the other silently
- Anti-corruption layer (ACL) translates between context models at integration event boundaries
- Cross-context queries: API composition (aggregating results from multiple service calls) vs CQRS read model (pre-joined projection)
- API composition is consistent but couples services at query time; CQRS read models are eventually consistent but decoupled

**Answer**

Domain-Driven Design defines a bounded context as the linguistic boundary within which a domain
model is internally consistent. `Order` in the Order context is a confirmed customer purchase with
financial line items; `Order` in the Fulfillment context is a physical package to be picked from a
warehouse. Sharing the same database table for both meanings creates silent coupling: both teams
must coordinate every schema change because a column added for the Order context may violate an
assumption the Fulfillment service depends on.

Microservice architecture operationalizes the bounded context by giving each service its own
database schema. Neither service can issue SQL that crosses the boundary — there is no foreign key
from the Order table to the Fulfillment table, because they do not share a database.

The practical difficulty is cross-context queries. A dashboard showing a customer's order history
alongside shipment status cannot use a SQL join. Two patterns address this. API composition has a
query handler or BFF call both services and merge the results in memory — it is consistent and
straightforward, but both services must be available simultaneously and latency adds up for
fan-out queries. A CQRS read model materializes a pre-joined view by consuming integration events
from both contexts and projecting them into a dedicated denormalized store owned by a third
component. The read model is eventually consistent but fully decoupled — it serves queries even
when one upstream service is down. The anti-corruption layer applies at the integration event
boundary in both patterns: a mapping step translates the upstream context's vocabulary into the
consuming context's own model, preventing domain concept leakage across the linguistic boundary.

---

## CQ11. How do Kubernetes liveness and readiness probes interact with a Polly circuit breaker, and what health check design prevents a service with an open circuit from appearing healthy?

**Concepts**
- Readiness probe failing removes the pod from the Service endpoint slice — no new traffic is routed to it
- Liveness probe failing restarts the pod — used to recover from deadlocks or unrecoverable process states
- A Polly circuit breaker can be open (failing all outbound calls fast) while the pod is still up and responding to probes
- The pod appears healthy to K8s but fails every user request that hits the broken dependency path
- A custom `IHealthCheck` that reads Polly pipeline circuit state bridges the two layers
- Tying an open critical-dependency circuit to a `Unhealthy` readiness result removes the pod from rotation correctly

**Answer**

Kubernetes probes operate at the infrastructure level: a failing readiness probe causes kube-proxy
to stop routing traffic to that pod within seconds; a failing liveness probe restarts it. Polly
circuit breakers operate at the application level: when the circuit is open, calls to a downstream
dependency fail fast without making a network request. The two mechanisms can contradict each
other. A pod with an open circuit breaker around the payment service will still pass a readiness
probe that only verifies the HTTP server is listening. From Kubernetes's perspective the pod is
healthy and it continues to receive traffic; from the user's perspective every request that
requires the payment service fails with a `BrokenCircuitException`.

The correct design connects the two layers. ASP.NET Core's `IHealthChecksBuilder.AddCheck<T>()`
allows a custom health check to inspect the state of registered Polly resilience pipelines. When
the circuit for a dependency the pod cannot function without is `Open`, the health check returns
`HealthCheckResult.Unhealthy()`, causing the readiness probe endpoint to return a non-200 status
and K8s to remove the pod from the endpoint slice. Non-critical circuits — a recommendation
engine the service degrades gracefully without — should return only `Degraded`, which can be
configured to not fail the readiness endpoint and instead surface as an alerting signal.

In .NET 10 this is wired via `AddResiliencePipeline("payment", builder => ...)` on the
`IServiceCollection`, combined with a custom health check that resolves
`IResiliencePipelineProvider<string>` and reads the circuit state through the pipeline's
`CircuitBreakerStrategyOptions.OnOpened` callback or a shared `CircuitBreakerStateStore`.

---

## CQ12. When should retry and circuit-breaker policies live in application code versus a service mesh, and how do you prevent a retry storm when both layers are active?

**Concepts**
- Service mesh (Istio, Linkerd) handles L4/L7 resilience at the sidecar proxy — transparent to application code
- Polly handles resilience inside the process — aware of business exceptions, typed HTTP errors, and domain circuit state
- Double retry: mesh retries N times and Polly retries M times for the same logical request — effective retries = N × M
- Retry storms: multiplicative retry amplification during a partial outage can overwhelm the already-failing service
- Recommendation: mesh handles transparent network retries; Polly handles business-logic-aware circuit breaking
- Mitigation: disable Polly retries on paths the mesh already retries, or configure non-overlapping retry budgets

**Answer**

A service mesh such as Istio or Linkerd deploys a sidecar proxy alongside every pod. The proxy
intercepts all inbound and outbound traffic and can apply retry, timeout, circuit breaking, and
mutual TLS entirely transparently — the application has no awareness of these policies. Polly, by
contrast, lives inside the application process and can act on business-level information: retrying
only on a specific HTTP 409 conflict code, opening a circuit when a payment SLA threshold is
crossed, or isolating a slow third-party API behind a bulkhead.

Running both simultaneously without coordination creates a retry storm risk. If Istio retries three
times on a 503 and Polly also retries three times on `HttpRequestException`, a single failed
request generates up to nine actual network calls to the upstream service. During a partial outage
where the upstream is intermittently available, this amplification can cause a thundering-herd
collapse that prevents the service from recovering.

The practical guidance is to assign each concern to the layer best suited for it and document the
split explicitly. Let the mesh handle transparent network-level retries for transient TCP errors
and connection resets; use Polly for concerns the sidecar cannot see — domain exception types,
circuit state tied to business SLAs, bulkheads around slow dependencies. Concretely: set Polly's
retry count to zero on HTTP paths the mesh already retries, or configure Polly's retry with a
jittered delay long enough that it does not overlap with the mesh's retry window. In .NET 10
applications, a note in the service's resilience configuration file should record which retry layer
is active per upstream, because a service migration from one mesh to another silently inherits the
new mesh's retry policy and invalidates the Polly settings.

---

## CQ13. How does W3C TraceContext propagate a single user request's trace across synchronous gRPC calls and asynchronous message flows?

**Concepts**
- W3C TraceContext: `traceparent` header encodes trace ID, parent span ID, and sampling flag
- gRPC metadata carries `traceparent` identically to HTTP headers; `GrpcNetClient` instrumentation injects/extracts it
- Async message flows require serializing the current activity context into message headers before publishing
- The consumer extracts trace context from message headers and creates a child span linked to the producer's span
- Trace links (not parent-child hierarchy) represent async causality where producer-to-consumer latency is unbounded
- A single user request can produce a trace tree spanning N services, with async leaf spans closing much later

**Answer**

W3C TraceContext propagates distributed traces by placing a `traceparent` header on every outbound
call. The header encodes the trace ID (shared across all related spans), the current span ID
(which becomes the parent of the next span), and a sampling flag. When a synchronous gRPC call
leaves a .NET 10 service, `OpenTelemetry.Instrumentation.GrpcNetClient` automatically injects
`traceparent` as a gRPC metadata entry. The receiving service's gRPC server instrumentation
extracts it, creates a child span, and records its own duration and status. The result in Jaeger
or Azure Monitor Application Insights is a connected parent-child tree showing the complete
synchronous call path across service boundaries.

Asynchronous message flows require explicit propagation. Before publishing a message to RabbitMQ
or Azure Service Bus, the application serializes the ambient `Activity` context into message
headers using `Propagators.DefaultTextMapPropagator.Inject`. The consumer extracts the context on
receipt, creates a new root span, and attaches a trace link pointing to the producer's span rather
than making it a direct child. The link relationship represents causal ordering without implying
synchronous nesting — the correct model when producer-to-consumer latency is unbounded and the
consumer span may open seconds or minutes after the producer span closed.

A realistic order-processing trace might: enter through the API gateway (span 1), reach the Order
service via HTTP (span 2), call Inventory via gRPC (span 3), publish `OrderPlaced` to the broker
(span 4), and asynchronously trigger the Notification service (span 5, linked to span 4). All
five spans share one trace ID and appear as a connected tree with a linked async branch in the
observability backend, enabling end-to-end latency attribution for a single user action even
across synchronous and asynchronous hops.

---

## CQ14. How does Kubernetes Secret injection enforce database-per-service isolation, and what is the critical gotcha of baking a connection string into a Docker image layer?

**Concepts**
- Each service has its own named K8s Secret (`orders-db-secret`, `inventory-db-secret`) scoped to that service
- RBAC binds the pod's service account to a Role that grants `get` only on the service's own Secret
- `env.valueFrom.secretKeyRef` in the pod spec injects the connection string as a process environment variable
- Network policy can further restrict TCP egress from a pod to only its own database's ClusterIP
- Hardcoding gotcha: a connection string in a `Dockerfile` `ENV` instruction is visible in `docker history --no-trunc` and every derived image
- Startup validation with `AddOptions<T>().ValidateOnStart()` fails the readiness probe early if the connection string is missing

**Answer**

The database-per-service pattern works only when isolation is enforced mechanically, not merely by
convention. In Kubernetes the first enforcement layer is naming: each service has a distinct
Secret — `orders-db-secret`, `inventory-db-secret` — and the pod spec references its own Secret
exclusively through `env.valueFrom.secretKeyRef`. A Kubernetes RBAC Role bound to the pod's
service account grants `get` permission only on that specific Secret, so even if the pod were
compromised, the service account cannot read a sibling service's credentials.

Network policy adds a second layer: a `NetworkPolicy` resource can restrict TCP egress from the
Orders deployment to only the Orders database's ClusterIP on port 5432, preventing the pod from
reaching another service's database at the network level even if it somehow acquired the wrong
connection string.

The hardcoding gotcha is subtle but serious. A connection string placed in a `Dockerfile` `ENV`
instruction is baked into a named image layer and is visible in `docker history --no-trunc`
output, in every registry push of the image, and in every derived image that `FROM`s it. Once in
the build cache it is effectively readable by anyone with pull access to the registry, which in
many organizations is a broad group. The correct pattern is never to set
`ConnectionStrings__DefaultConnection` in the Dockerfile; inject it exclusively at runtime via
K8s Secret to pod environment variable. In .NET 10 the application validates the connection string
is present and well-formed during startup through `services.AddOptions<DatabaseOptions>()
.BindConfiguration("ConnectionStrings").ValidateDataAnnotations().ValidateOnStart()`, causing the
pod to fail its readiness probe immediately rather than start silently with a broken or empty
configuration.

---

## CQ15. What is the difference between compensating and undo transactions in saga rollback, and what happens when a compensating event arrives before the forward event on an unordered broker?

**Concepts**
- Compensating transaction: a new forward action that semantically reverses a completed step (e.g., refund a charge)
- Undo transaction: a literal database state revert — only safe within a single service's own data boundary
- Compensating transactions are necessary because other services may have already observed the intermediate state
- Idempotency requirement: at-least-once delivery may re-deliver compensating events; applying them twice must be safe
- Ordering gotcha: a broker without partition-level ordering guarantees can deliver `OrderCancelled` before `OrderReserved`
- Guard pattern: check whether the forward step was completed before applying compensation; requeue if not yet received

**Answer**

In a saga, a compensating transaction is not the mathematical inverse of the original operation —
it is a new forward action that semantically undoes the effect. Releasing a reserved inventory
item is a compensating transaction for `ReserveInventory`; it creates or updates a record
forward, rather than deleting the row that `ReserveInventory` inserted. This distinction matters
because state may have changed between the forward step and its compensation: the inventory
manager may have already observed the reserved state and made further decisions based on it, so a
literal SQL rollback would corrupt the audit trail and contradict observations other parts of the
system have already acted upon.

An undo transaction — literally reverting database state — is only safe within a single service's
own data boundary where no other service can have observed the intermediate state. Across service
boundaries, compensating transactions are the only sound model.

Because message brokers provide at-least-once delivery, compensating event handlers must be
idempotent. A `ReleaseInventoryReserved` handler that receives the same event twice must produce
the same outcome both times, typically by recording a `CompensationApplied` flag keyed on the
saga correlation ID before executing.

The ordering gotcha is more dangerous: a broker that does not guarantee strict partition-level
ordering can deliver the `OrderCancelled` compensating trigger to the Inventory service before
`OrderReserved` has been processed. The inventory service has no reservation to release. Ignoring
the compensating event creates a leaked reservation; processing it against a missing record may
corrupt state. The guard pattern handles this: if the correlation ID is unknown, the handler
requeues the compensating message with a short delay rather than ignoring or applying it
blindly. In MassTransit this is expressed as `await context.Defer(TimeSpan.FromSeconds(5))` when
the precondition record is absent, giving the forward event time to arrive and be processed first.

---

## CQ16. How does Clean Architecture's dependency inversion make the domain layer testable in isolation, and what is the role of in-memory infrastructure fakes in application-layer tests?

**Concepts**
- Domain layer has zero references to infrastructure: no EF Core, no HTTP clients, no message brokers
- Aggregate behavior is tested with plain xUnit tests — no test container, no mocking framework required
- Application layer depends on interfaces (`IOrderRepository`, `IEventPublisher`) defined in the domain or application layer
- In-memory fakes implement those interfaces in memory without a real database or broker
- Application-layer integration tests drive command handlers end-to-end using fakes for all infrastructure
- Only tests verifying EF Core queries, real broker serialization, or network behavior require a test container

**Answer**

Clean Architecture's dependency rule states that source code dependencies point only inward: the
domain layer references nothing from outside itself. An `Order` aggregate, its value objects,
domain events, and domain services carry zero references to EF Core, HTTP clients, or message
brokers. This makes domain-layer testing entirely infrastructure-free: instantiate the aggregate,
call a method, assert the raised domain events and resulting state — no mocking framework, no
container, no async infrastructure. These tests run in microseconds and provide complete coverage
of business rules.

The application layer introduces a seam at interfaces. `IOrderRepository`, `IEventPublisher`, and
`IUnitOfWork` are declared in the application or domain layer; their real implementations live in
the infrastructure layer and depend on EF Core, RabbitMQ, or Azure Service Bus. For
application-layer integration tests — verifying that a command handler loads the aggregate
correctly, invokes the right domain methods, saves the result, and publishes the expected event —
an in-memory fake for each interface is sufficient. A `FakeOrderRepository` backed by a
`Dictionary<Guid, Order>` and a `FakeEventPublisher` that accumulates published events in a list
let the test drive the full command handler orchestration without any running infrastructure.

Only tests that must verify EF Core query translation, real broker message serialization, or
actual network round-trips need a test container. The split is deliberate: the bulk of behavioral
coverage lives in fast, isolated domain and application-layer tests that a developer runs on every
save; infrastructure integration tests are narrower, slower, and far fewer in number. In .NET 10
the idiomatic approach is to register `FakeOrderRepository : IOrderRepository` in a test
`WebApplicationFactory<Program>` using `builder.ConfigureServices(services => services
.AddSingleton<IOrderRepository, FakeOrderRepository>())` with `UseEnvironment("Testing")`.

---

## CQ17. Why does gRPC load balancing fail silently behind a Kubernetes ClusterIP Service, and how do headless Services or an API gateway solve it?

**Concepts**
- ClusterIP load balancing is L4 (connection-level) — kube-proxy selects a pod IP per TCP connection, not per request
- HTTP/2 (used by gRPC) multiplexes all calls over a single long-lived TCP connection — L4 picks one pod and stays there
- Result: a gRPC client behind a ClusterIP sends 100% of traffic to one pod; others sit idle
- Headless Service (`clusterIP: None`) returns per-pod A records in DNS — enables client-side round-robin per call
- YARP or a gRPC-aware API gateway performs L7 load balancing, distributing individual gRPC method invocations
- `GrpcChannelOptions.ServiceConfig` with a round-robin policy configures the .NET gRPC client for per-call balancing

**Answer**

Kubernetes ClusterIP Services perform load balancing at L4: kube-proxy intercepts new TCP
connections to the virtual IP and selects a backing pod IP using iptables or IPVS rules. For
HTTP/1.1 this distributes traffic effectively because each request typically opens a new
connection. gRPC uses HTTP/2, which multiplexes all calls over a single, persistent TCP
connection. Once a gRPC client opens a connection to the ClusterIP, every subsequent call travels
over that same connection to the same pod. The L4 balancer has no visibility into individual gRPC
method invocations, so it cannot distribute them across replicas.

The symptom in production is a silently skewed load distribution after a deployment: one pod
handles all gRPC traffic while others sit idle. Memory and CPU utilization diverge, the overloaded
pod may be restarted by a liveness probe, and the root cause is not immediately visible in
standard metrics.

Two solutions exist. A Kubernetes headless Service (`clusterIP: None`) instructs kube-dns to
return a list of individual pod A records rather than a single virtual IP. A .NET 10 gRPC client
configured with a round-robin service config policy — set via
`GrpcChannelOptions { ServiceConfig = new ServiceConfig { LoadBalancingConfigs = { new RoundRobinConfig() } } }`
— queries DNS on each call, gets the full pod list, and distributes invocations across them.
Alternatively, a YARP-based API gateway placed in front of the gRPC tier performs true L7 routing:
it maintains a connection pool to each pod and assigns individual gRPC calls to different upstream
connections. The gateway approach is preferred when the callers are external, when retries and
authentication are required at the gRPC boundary, or when the team wants to avoid coupling gRPC
clients to Kubernetes DNS semantics.

---

## CQ18. How do you handle client reads that arrive during the lag between a write command completing and the CQRS read model being updated, and what is the 'read your own writes' pattern?

**Concepts**
- CQRS read-side projections update asynchronously — there is a window after a write where the read model is stale
- A client querying immediately after a command may receive pre-command (stale) data or a 404
- "Read your own writes" (RYOW): the issuing client must see the effect of its own writes without delay
- Simplest fix: return the created or updated resource directly in the command response — bypasses the read model
- Position token: the write side returns an event position; the read endpoint waits until the projection reaches that position
- `Channel<long>` or `SemaphoreSlim` in the projection handler signals waiting read queries when the projection advances

**Answer**

In a CQRS architecture, write commands update the event store or write database; the read-side
projection handler processes integration events asynchronously and refreshes the denormalized
view. The lag between a command committing and the projection catching up ranges from milliseconds
in a healthy system to seconds under load or broker backpressure. A client that fires
`POST /orders` followed immediately by `GET /orders/{id}` may receive a 404 or stale data from
the read model — a confusing result when the user just watched the Create button succeed.

The "read your own writes" pattern ensures the issuing client sees its own write immediately. The
simplest implementation is to include the created or updated resource in the command response
body: the HTTP 201 response carries the new order's data directly, so the first read comes from
the command handler's return value, not the query endpoint. The client displays this result
immediately and only refreshes from the query endpoint on subsequent calls. No synchronization
infrastructure is required.

For cases where the client must use the query endpoint — filtered lists, aggregations, or
cross-resource views — a projection position token provides a synchronization mechanism. The write
side returns the event stream position or logical timestamp in a response header
(`X-Event-Position: 1042`). The query endpoint accepts an optional `minPosition` query parameter;
if the projection has not yet advanced past that position, the handler waits up to a configurable
deadline (e.g., 500 ms) using a `Channel<long>` notification published by the projection worker
each time it processes a new event. If the projection catches up within the deadline the response
is returned normally; if not, the endpoint returns `202 Accepted` with a `Retry-After: 1`
header, telling the client when to retry. This keeps the query path non-blocking for the common
case while providing a deterministic answer for time-sensitive reads without coupling the
read and write pipelines through a shared database lock.

---

## Gotchas — ASP.NET Microservices (Interview Traps)

---

#### Gotcha 1. Service Boundaries Drawn Too Fine-Grained

**Concepts**
- Nano-service with only one or two endpoints per service
- Network call overhead dominating business logic execution time
- Chatty inter-service communication from over-decomposition
- Sam Newman's rule: start coarser, split when pain is felt

**Answer**

Decomposing a system into dozens of nano-services — one service per database table or one service per entity type — creates overwhelming operational complexity with no corresponding business benefit. Every inter-service call adds network latency, serialisation overhead, a new failure point, and a deployment coordination burden. A service that makes 10 synchronous calls to other services to handle one user request has 10 times the failure surface and cumulative latency of a well-designed monolith. The practical guidance is to start with coarser boundaries aligned to business capabilities (OrderManagement, CustomerProfile, ProductCatalogue) and split only when a specific scaling bottleneck, team ownership conflict, or technology isolation requirement justifies the operational cost.

---

#### Gotcha 2. Distributed Monolith — Separate Deployment, Tight Coupling

**Concepts**
- Services sharing a database and deployed independently
- Change in one service requiring simultaneous deployment of others
- Tight runtime coupling despite separate executables
- True independence requiring autonomous data ownership

**Answer**

A distributed monolith deploys services as separate processes but maintains tight coupling through a shared database, shared library versions that must stay in sync, or synchronous HTTP dependencies that require all services to be available simultaneously for any one to function. The hallmarks are: you can't deploy Service A without also deploying Service B, Service A fails if Service B is down, and both services write to the same database tables. Microservices are not defined by deployment topology — they are defined by autonomous data ownership, independent deployability, and bounded-context alignment. A system of 20 separately deployed services sharing one database is still a monolith with distributed failure modes.

---

#### Gotcha 3. Synchronous Call Chain Across Many Services

**Concepts**
- Request traversing 6 services synchronously before responding
- Cumulative latency and failure probability compounding per hop
- Availability product: 0.999^6 = 99.4% system availability
- Async messaging or Saga pattern for multi-service workflows

**Answer**

A user-facing request that synchronously calls Service A → B → C → D → E → F before returning a response has compounded latency (sum of all service round trips) and compounded failure probability (if each service has 99.9% uptime, the chain has 99.4% uptime — one additional 9 of downtime). Under this model, adding one more service to the chain reduces overall availability and increases median response time. Long synchronous chains indicate that the business capability should be consolidated into fewer services, or that the workflow should be redesigned as an asynchronous event-driven flow where services react to events rather than being called in sequence.

---

#### Gotcha 4. No Contract Testing Between Microservices

**Concepts**
- Provider changing an API field without knowledge of consumer expectations
- Integration test environment required to catch breaking changes
- Consumer-Driven Contract Testing with Pact
- Contract test running in CI before deployment

**Answer**

When Service A consumes Service B's REST or gRPC API, a breaking change in Service B — renamed field, removed endpoint, changed response shape — breaks Service A silently in production unless caught by integration tests that require both services to be running simultaneously in a shared environment. Consumer-Driven Contract Testing (Pact) solves this: Service A publishes a "contract" describing what it expects from Service B, and Service B's CI runs the contract test against its own codebase independently, without needing Service A to be running. A contract violation fails Service B's CI build before the breaking change is deployed, catching the incompatibility at the cheapest possible moment.

---

#### Gotcha 5. Not Planning for Partial Failure in Inter-Service Calls

**Concepts**
- Happy-path code assuming all downstream calls succeed
- Missing timeout, retry, and fallback for every outgoing call
- Polly resilience pipeline on every HttpClient and gRPC channel
- Degraded response preferred over total failure for non-critical data

**Answer**

Microservice code that calls downstream services without timeout, retry, or fallback handling assumes the network is reliable and every dependency is always available — an assumption that fails in production within hours of the first deployment. Every HTTP client and gRPC channel must have a timeout (preventing indefinite blocking), a retry with jitter (for transient failures), and a circuit breaker (for sustained failures). For non-critical downstream data — product recommendations, personalisation — a fallback that returns a cached or default response is preferable to propagating the failure to the user. Code without these patterns is incomplete by design, not by oversight.

---

#### Gotcha 6. Shared Library Coupling Services to Each Other

**Concepts**
- Common.Contracts NuGet package shared across all services
- Change to the shared package requiring all services to recompile and redeploy
- Shared kernel for truly stable concepts only
- API versioning and code generation as alternatives

**Answer**

Creating a `Common.Contracts` or `SharedModels` NuGet package containing DTOs, event classes, and interface definitions that all services reference re-introduces the coupling that microservices are designed to eliminate — a change to one DTO in the shared package requires all services that reference it to be rebuilt, retested, and redeployed simultaneously. Each service should own its own contracts; for cross-service event or API contracts, generate client code from OpenAPI or proto schemas (contract-first) or use Consumer-Driven Contract Testing so that compatibility is verified without a shared binary dependency. Shared packages are appropriate only for genuinely stable cross-cutting utilities (logging wrappers, telemetry configuration) that change independently of business contracts.

---

#### Gotcha 7. No Service Ownership — Every Team Deploys Every Service

**Concepts**
- Multiple teams making changes to the same service
- No clear accountability for service health or on-call responsibility
- Conway's Law: system structure mirrors communication structure
- One team per service as the ownership model

**Answer**

Without clear ownership, every team modifies any service it needs to change, no team is accountable for a service's availability during on-call incidents, and architectural decisions are made inconsistently across the service. Conway's Law predicts this outcome: a system built by multiple teams without clear service ownership will reflect the communication patterns of those teams — tangled dependencies and overlapping responsibilities. Microservices require that each service has exactly one owning team that is responsible for its design, deployments, SLA, and on-call rotation. Other teams consume the service via its public API and raise requests for changes to the owning team — they do not merge code directly into another team's service repository.

---

#### Gotcha 8. API Versioning Ignored Until a Breaking Change Forces a Crisis

**Concepts**
- Breaking change deployed to v1 breaking all existing clients
- Semantic versioning and URL path versioning for REST
- Proto package versioning for gRPC
- Deprecation period before removing old version

**Answer**

Without an API versioning strategy, the first breaking change — a renamed field, a removed endpoint, a changed status code meaning — requires either accepting broken clients or deploying new and old versions simultaneously with no planned migration path. The versioning strategy must be decided before the first public API is deployed: URL path versioning (`/v1/orders`, `/v2/orders`) is visible and cache-friendly for REST; header-based versioning is cleaner but less cache-friendly; proto package versioning (`myservice.v1`, `myservice.v2`) is the standard for gRPC. Every breaking change requires a new version, the old version must be maintained for a deprecation period with advance notice to clients, and the retirement date must be communicated and enforced.

---

#### Gotcha 9. Polyglot Persistence Without Data Sovereignty Planning

**Concepts**
- Each service choosing its own database technology independently
- Reporting spanning multiple database technologies with no aggregation layer
- Operational skills required for each database technology in production
- Technology choice driven by access pattern fit, not novelty

**Answer**

"Polyglot persistence" — each service choosing the database most suited to its data model — is a genuine benefit of Database per Service, but adopted without discipline it produces a fleet where one team operates SQL Server, another MongoDB, another Redis, another Elasticsearch, and another Cassandra — all in production simultaneously, requiring expertise and operational tooling for all five. The practical guidance is to select a small approved set of database technologies (e.g., PostgreSQL as the default, Redis for caching, Elasticsearch for full-text search) and justify deviation from the set with concrete access pattern requirements, not novelty. Reporting and compliance that spans all services also requires a data aggregation strategy from the start.

---

#### Gotcha 10. Skipping Load Testing and Assuming Microservices Auto-Scale

**Concepts**
- Horizontal pod autoscaler requiring load testing to set correct thresholds
- Downstream services becoming bottlenecks when only one service is scaled
- Network and database connection limits revealed only under realistic load
- Load testing identifying the weakest link in the call chain

**Answer**

Assuming that a microservices architecture with Kubernetes auto-scaling will handle any load automatically without load testing is a common and expensive mistake. The HPA scales pods based on CPU or custom metrics, but if the auto-scaling thresholds are wrong (too high or too low), the pods either scale too late (users experience latency spikes) or scale too aggressively (costs balloon). Load testing reveals the actual bottleneck: often it is not the scaled service but a downstream dependency — the database connection limit, a third-party API rate limit, or a service three hops downstream that is not configured with sufficient replicas. Load testing under realistic traffic patterns is a prerequisite for setting correct resource requests, limits, HPA thresholds, and circuit breaker configurations.

---
