# Interview Questions — API Gateway Pattern — Interview Q&A
> 30 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is an API Gateway, and what problem does it solve in a microservices architecture?](#q1-what-is-an-api-gateway-and-what-problem-does-it-solve-in-a-microservices-architecture)
2. [Q2. How does an API Gateway differ from a traditional reverse proxy or load balancer?](#q2-how-does-an-api-gateway-differ-from-a-traditional-reverse-proxy-or-load-balancer)
3. [Q3. What are the core responsibilities an API Gateway typically handles?](#q3-what-are-the-core-responsibilities-an-api-gateway-typically-handles)
4. [Q4. What is the Backend for Frontend (BFF) pattern, and how does it extend the API Gateway concept?](#q4-what-is-the-backend-for-frontend-bff-pattern-and-how-does-it-extend-the-api-gateway-concept)
5. [Q5. What are the main trade-offs of introducing an API Gateway in a microservices system?](#q5-what-are-the-main-trade-offs-of-introducing-an-api-gateway-in-a-microservices-system)
6. [Q6. How does an API Gateway route requests to downstream services, and what routing strategies are commonly supported?](#q6-how-does-an-api-gateway-route-requests-to-downstream-services-and-what-routing-strategies-are-commonly-supported)
7. [Q7. What is request aggregation in an API Gateway, and when should the gateway aggregate rather than the client?](#q7-what-is-request-aggregation-in-an-api-gateway-and-when-should-the-gateway-aggregate-rather-than-the-client)
8. [Q8. What is the difference between synchronous parallel aggregation and sequential aggregation at the gateway?](#q8-what-is-the-difference-between-synchronous-parallel-aggregation-and-sequential-aggregation-at-the-gateway)
9. [Q9. How does an API Gateway perform protocol translation, such as from HTTP/REST to gRPC?](#q9-how-does-an-api-gateway-perform-protocol-translation-such-as-from-httprest-to-grpc)
10. [Q10. How does an API Gateway centralize authentication and authorization for downstream microservices?](#q10-how-does-an-api-gateway-centralize-authentication-and-authorization-for-downstream-microservices)
11. [Q11. What is rate limiting at the API Gateway, what algorithms are used, and why does it matter for microservices?](#q11-what-is-rate-limiting-at-the-api-gateway-what-algorithms-are-used-and-why-does-it-matter-for-microservices)
12. [Q12. How does an API Gateway handle SSL/TLS termination, and what are the security implications?](#q12-how-does-an-api-gateway-handle-ssltls-termination-and-what-are-the-security-implications)
13. [Q13. What is request and response transformation, and why might a gateway need to modify payloads in flight?](#q13-what-is-request-and-response-transformation-and-why-might-a-gateway-need-to-modify-payloads-in-flight)
14. [Q14. How does an API Gateway contribute to observability — logging, metrics, and distributed tracing?](#q14-how-does-an-api-gateway-contribute-to-observability-logging-metrics-and-distributed-tracing)
15. [Q15. What is Ocelot, and how does it work as an API Gateway in an ASP.NET Core application?](#q15-what-is-ocelot-and-how-does-it-work-as-an-api-gateway-in-an-aspnet-core-application)
16. [Q16. How do you configure routes in Ocelot, and what does a basic ocelot.json configuration look like?](#q16-how-do-you-configure-routes-in-ocelot-and-what-does-a-basic-ocelotjson-configuration-look-like)
17. [Q17. How does Ocelot integrate with service discovery (e.g., Consul or Kubernetes), and what is the benefit?](#q17-how-does-ocelot-integrate-with-service-discovery-eg-consul-or-kubernetes-and-what-is-the-benefit)
18. [Q18. What are Ocelot's built-in middleware capabilities, and how do you extend them with custom logic?](#q18-what-are-ocelots-built-in-middleware-capabilities-and-how-do-you-extend-them-with-custom-logic)
19. [Q19. What are the known limitations of Ocelot in high-throughput or large-scale production environments?](#q19-what-are-the-known-limitations-of-ocelot-in-high-throughput-or-large-scale-production-environments)
20. [Q20. What is YARP, how does it differ from Ocelot, and when would you choose it for a .NET gateway?](#q20-what-is-yarp-how-does-it-differ-from-ocelot-and-when-would-you-choose-it-for-a-net-gateway)
21. [Q21. How do you configure YARP routes and clusters in an ASP.NET Core application?](#q21-how-do-you-configure-yarp-routes-and-clusters-in-an-aspnet-core-application)
22. [Q22. How does YARP support load balancing, and what load balancing policies does it provide?](#q22-how-does-yarp-support-load-balancing-and-what-load-balancing-policies-does-it-provide)
23. [Q23. How do you implement request transformation or custom middleware with YARP?](#q23-how-do-you-implement-request-transformation-or-custom-middleware-with-yarp)
24. [Q24. What resilience patterns should an API Gateway implement, and how do Polly and gateway-level circuit breakers complement each other?](#q24-what-resilience-patterns-should-an-api-gateway-implement-and-how-do-polly-and-gateway-level-circuit-breakers-complement-each-other)
25. [Q25. What security threats does placing an API Gateway in front of microservices help mitigate?](#q25-what-security-threats-does-placing-an-api-gateway-in-front-of-microservices-help-mitigate)
26. [Q26. How do you prevent the API Gateway itself from becoming a bottleneck or single point of failure in a distributed system?](#q26-how-do-you-prevent-the-api-gateway-itself-from-becoming-a-bottleneck-or-single-point-of-failure-in-a-distributed-system)
27. [Q27. When is a dedicated API Gateway unnecessary, and when does it become essential?](#q27-when-is-a-dedicated-api-gateway-unnecessary-and-when-does-it-become-essential)
28. [Q28. What is the "smart endpoints, dumb pipes" philosophy in microservices, and does an API Gateway contradict it?](#q28-what-is-the-smart-endpoints-dumb-pipes-philosophy-in-microservices-and-does-an-api-gateway-contradict-it)
29. [Q29. How do you manage API Gateway configuration drift as the number of routes and services grows?](#q29-how-do-you-manage-api-gateway-configuration-drift-as-the-number-of-routes-and-services-grows)
30. [Q30. What are the differences between a self-hosted gateway (Ocelot, YARP) and a managed gateway service (Azure API Management, AWS API Gateway)?](#q30-what-are-the-differences-between-a-self-hosted-gateway-ocelot-yarp-and-a-managed-gateway-service-azure-api-management-aws-api-gateway)

---

## Q1. What is an API Gateway, and what problem does it solve in a microservices architecture?

**Concepts**
- Single entry point vs fragmented multi-service client exposure
- Cross-cutting concern duplication across individual services
- Request fan-out aggregation at the edge
- Protocol translation between client and internal protocols

**Answer**

An API Gateway is a single entry point that sits between external clients and a collection of backend microservices, accepting all inbound requests and routing them to the appropriate service. Without a gateway, clients must know the address of every service, handle different authentication schemes per service, make multiple round trips to assemble a single view, and update when service topology changes. The gateway centralises all of that, presenting a unified API surface to clients regardless of how many services exist behind it. Without a gateway, clients are directly coupled to service topology — if a service is split, renamed, or relocated, every client that calls it breaks. Cross-cutting concerns such as authentication, rate limiting, and logging would otherwise be duplicated in every microservice; the gateway implements them once and applies them consistently. It can also fan out a single client request to several downstream services in parallel, merge the results, and return one response, which reduces round trips on constrained clients such as mobile apps.

---

## Q2. How does an API Gateway differ from a traditional reverse proxy or load balancer?

**Concepts**
- Application-layer logic vs network-layer operation
- Authentication and authorization as gateway-exclusive concern
- Request aggregation across multiple services
- Protocol translation from external to internal format

**Answer**

A traditional reverse proxy or load balancer operates at the network or transport level — it performs TLS termination, distributes traffic across backend instances, and routes by host or path, but has no understanding of application-layer concepts. An API Gateway does all of that and adds application-layer logic: authentication and authorisation, rate limiting, request and response transformation, API versioning, response caching, and request aggregation across multiple services. A reverse proxy is infrastructure; an API Gateway is application infrastructure — the distinction matters when choosing where to place authentication or transformation logic.

| Feature | Reverse Proxy / Load Balancer | API Gateway |
|---|---|---|
| TLS termination | Yes | Yes |
| Load balancing | Yes | Yes |
| Path-based routing | Basic | Rich, with transforms |
| Authentication / Authorisation | No | Yes |
| Rate limiting | No | Yes |
| Request aggregation | No | Yes |
| Protocol translation | No | Yes |
| Caching | Sometimes (simple) | Yes, configurable per route |

---

## Q3. What are the core responsibilities an API Gateway typically handles?

**Concepts**
- Path-based request routing as baseline capability
- Token validation at the edge before any downstream execution
- Rate limiting and throttling for traffic control
- Request and response transformation for contract independence
- Unified observability across all services at one chokepoint

**Answer**

An API Gateway centralises the concerns that would otherwise be implemented and maintained independently in every microservice. Routing is the baseline: the gateway matches incoming requests by path, HTTP method, host header, or custom header and forwards them to the correct downstream service. Authentication and authorisation follow — the gateway validates tokens, whether JWT or API keys, once for every request before the downstream service ever executes. Rate limiting and throttling cap the number of requests a client can make per time window, protecting backend services from traffic spikes and abusive callers. Request and response transformation — rewriting URLs, injecting or stripping headers, and reshaping payloads — allows the external API contract to evolve independently of how internal services are structured. Observability rounds out the core responsibilities: the gateway is a single place to emit structured access logs, latency metrics, and distributed trace spans for every inbound request across all services.

---

## Q4. What is the Backend for Frontend (BFF) pattern, and how does it extend the API Gateway concept?

**Concepts**
- Client-type-specific API surface per BFF instance
- Response shaping for different bandwidth and data-shape needs
- Frontend team ownership of its own BFF
- Deployment complexity trade-off vs single generic gateway

**Answer**

The Backend for Frontend (BFF) pattern creates a dedicated API Gateway for each distinct type of client — one for the web app, one for the mobile app, one for third-party API consumers. A generic gateway gives all clients the same API surface, but different clients have very different needs: a mobile client wants compact, low-bandwidth responses, while a desktop dashboard may need richer aggregated data. The BFF allows each client's API to evolve independently without those changes affecting other consumers. A generic gateway risks over-fetching — returning fields mobile clients discard — or under-fetching — requiring multiple calls where one shaped call would do. Each BFF is typically owned by the team building the corresponding frontend, aligning ownership and allowing rapid iteration without cross-team coordination. The BFF still sits in front of downstream services, so cross-cutting concerns are handled at the BFF layer rather than duplicated in services. The trade-off is additional deployment complexity: running N BFFs rather than one generic gateway, each needing its own maintenance and scaling.

---

## Q5. What are the main trade-offs of introducing an API Gateway in a microservices system?

**Concepts**
- Additional network hop latency on every external request
- Centralised configuration fragility affecting all services simultaneously
- Logic creep into infrastructure layer creating a hidden monolith
- Unnecessary overhead for purely internal service-to-service traffic

**Answer**

An API Gateway simplifies clients and centralises cross-cutting concerns, but it also introduces a new component that must be highly available, carefully maintained, and sized for peak traffic. Every external request now passes through an additional hop — well-implemented gateways add single-digit milliseconds, but this compounds with downstream service latency for every user. Configuration fragility is a real concern: gateway routing rules are a centralised artefact, so an incorrect route change silently breaks a service for all clients, which is harder to isolate than a change inside a single service. Teams sometimes push business logic into gateway-level transformations for convenience, creating a hidden monolith in the infrastructure layer that is hard to test and version. Exposing an API Gateway on the internal network for service-to-service calls adds overhead with no benefit over direct service calls or an internal service mesh, so the gateway should sit at the external edge only.

---

## Q6. How does an API Gateway route requests to downstream services, and what routing strategies are commonly supported?

**Concepts**
- Path-based routing as primary discriminator
- Host-based routing for multi-tenant or multi-environment deployments
- Header-based canary and A/B routing without downstream code changes
- Weighted traffic splitting for gradual rollouts

**Answer**

An API Gateway routes requests by evaluating matching rules against attributes of each incoming request — URL path, HTTP method, host header, query parameters, or custom headers — and forwarding to the configured upstream service when a rule matches. Route evaluation is ordered: the gateway tests rules top-to-bottom and applies the first matching rule. Path-based routing is the most common strategy: `/api/orders/**` routes to the order service and `/api/customers/**` to the customer service. Host-based routing distinguishes between subdomains such as `api.example.com` versus `internal.example.com` to reach different backends, useful in multi-tenant or multi-environment deployments. Header-based routing routes on the value of a custom header such as `X-API-Version` or `X-Client-Type`, enabling canary deployments or A/B testing at the gateway layer without touching downstream code. Weighted routing splits traffic across two versions of a service — for example 90% to stable and 10% to a canary — allowing gradual rollouts with no client-side changes.

---

## Q7. What is request aggregation in an API Gateway, and when should the gateway aggregate rather than the client?

**Concepts**
- Parallel fan-out to reduce client round trips
- Mobile client latency reduction on high-latency connections
- Structural vs domain-logic aggregation boundary
- Partial response as degraded aggregation strategy

**Answer**

Request aggregation means the gateway calls multiple downstream services on behalf of the client, combines their results, and returns a single response — eliminating the need for the client to make and manage multiple parallel requests. The gateway should aggregate when the client is constrained, such as mobile on a high-latency network, when the combination logic is stable, or when backend credentials should not leave the server. Mobile clients benefit most: one gateway call rather than five reduces both perceived latency, since backend calls run in parallel, and data transfer. Aggregation at the gateway is appropriate when the combination logic is simple and unlikely to change — if the logic is complex or domain-specific, a dedicated aggregation microservice or BFF is more maintainable. The gateway should not aggregate when the combination requires business-rule decisions, since pushing domain logic into infrastructure makes it invisible and hard to version. A practical boundary: if the aggregation is purely structural — merge these two JSON objects — it belongs in the gateway. If it requires conditional logic based on field values, it belongs in a service.

---

## Q8. What is the difference between synchronous parallel aggregation and sequential aggregation at the gateway?

**Concepts**
- Parallel aggregation — total latency equals slowest upstream
- Sequential aggregation — total latency equals sum of all upstream calls
- Data dependency as driver for sequential ordering
- Partial response with timeout for slow parallel upstreams

**Answer**

In parallel aggregation, the gateway fires all downstream requests simultaneously and waits for all responses before assembling the combined payload — total latency equals the slowest service call. In sequential aggregation, each downstream call depends on the result of the previous one, so calls execute one after another and total latency is the sum of all call times. Parallel aggregation is appropriate when downstream services are independent, such as fetching product details, customer reviews, and inventory status simultaneously to build a product page. Sequential aggregation is unavoidable when there are data dependencies between calls — for example, the gateway must first resolve a customer identity before it can fetch that customer's orders using the returned customer ID. For parallel calls where one upstream is consistently slow, consider a timeout with a partial response: return what completed and fill in the missing section with a default or empty value rather than blocking the entire aggregated response. In practice, most real aggregations are hybrid: parallel where services are independent and sequential only where strict data dependencies exist.

---

## Q9. How does an API Gateway perform protocol translation, such as from HTTP/REST to gRPC?

**Concepts**
- HTTP/JSON to gRPC/Protobuf deserialisation and re-serialisation
- Protobuf schema requirement at the gateway build
- YARP custom middleware for HTTP-to-gRPC translation
- Serialisation overhead vs gRPC internal network efficiency gain

**Answer**

Protocol translation at the gateway means accepting an external request in one protocol — HTTP/1.1 with a JSON body — and forwarding it to a backend service in a different protocol — gRPC over HTTP/2 with Protobuf encoding. The gateway deserialises the incoming format, re-serialises to the target format, invokes the upstream, and reverses the process on the response path. This is valuable when internal services communicate via gRPC for efficiency and strong typing but external clients, especially browsers, can only use HTTP/JSON without a gRPC-Web proxy. The gateway must know the Protobuf schema to serialise incoming JSON into the correct Protobuf message type, which usually means the schema is checked into the gateway's build. In .NET, YARP with custom middleware can implement this translation; Ocelot does not support HTTP-to-gRPC translation natively and requires custom extensions. The latency overhead of serialisation and deserialisation is typically small — single-digit milliseconds — and is usually outweighed by the performance gains from using gRPC on the internal network.

---

## Q10. How does an API Gateway centralize authentication and authorization for downstream microservices?

**Concepts**
- JWT validation at the edge before downstream execution
- Claims forwarding as trusted internal headers
- Coarse-grained vs fine-grained authorization boundary
- API key resolution and downstream identity passing

**Answer**

The API Gateway validates authentication credentials — JWTs, API keys, or OAuth 2.0 access tokens — before any request reaches a downstream service, rejecting unauthorised requests with a 401 or 403 response at the edge. JWT validation at the gateway checks the token signature, expiry, issuer, and audience; valid claims such as user ID, roles, and scopes are forwarded downstream as headers like `X-User-Id` or `X-User-Role` that services trust without re-validating. Downstream services only accept traffic from within the cluster network, so they trust the gateway's headers by network boundary rather than needing their own authentication middleware. Authorisation at the gateway handles coarse-grained decisions such as "does this token have the `orders:read` scope?"; fine-grained authorisation — "does user 42 own order 99?" — belongs in the service where the business data lives. API key management is similarly centralised: the gateway validates the key, resolves the associated tenant and rate-limit tier, and passes a resolved identity header downstream without exposing the raw key to services.

---

## Q11. What is rate limiting at the API Gateway, what algorithms are used, and why does it matter for microservices?

**Concepts**
- Token bucket algorithm for bursty-but-bounded traffic
- Fixed window vs sliding window vs leaky bucket trade-offs
- Per-dimension limiting — IP, user, API key, tenant
- Ocelot per-route config and ASP.NET Core RateLimiter integration

**Answer**

Rate limiting at the API Gateway enforces a cap on how many requests a client or group of clients can make within a time window, protecting downstream microservices from traffic spikes, abusive clients, and denial-of-service conditions. Without gateway-level rate limiting, every individual service would need its own rate limiting logic, and a misbehaving client could overwhelm one service while bypassing limits on others. Token bucket is the most common algorithm in production because it allows short bursts while enforcing an average rate, matching real client traffic patterns. Rate limits are applied per dimension: per IP address, per authenticated user, per API key, or per tenant — the dimension depends on whether the limit is meant to be fair per consumer or protective per service. In Ocelot, rate limiting is configured per route in `ocelot.json`. In YARP, it integrates with ASP.NET Core's built-in `RateLimiter` middleware introduced in .NET 7.

| Algorithm | How it works | Best for |
|---|---|---|
| Fixed window | Counts requests per discrete time window | Simple per-client quotas |
| Sliding window | Rolling count to prevent burst at window boundary | Smoother enforcement |
| Token bucket | Tokens accumulate; each request consumes one | Bursty but bounded traffic |
| Leaky bucket | Requests drain at constant rate | Constant output throughput |

---

## Q12. How does an API Gateway handle SSL/TLS termination, and what are the security implications?

**Concepts**
- TLS termination and internal HTTP forwarding
- Zero-trust mutual TLS (mTLS) for internal re-encryption
- Centralised certificate management at gateway
- Kestrel TLS in YARP gateway host

**Answer**

TLS termination means the API Gateway decrypts incoming HTTPS connections from external clients and forwards the request to downstream services over plain HTTP inside the internal network, offloading cryptographic work and certificate management from every individual service to one centralised point. The connection between the gateway and downstream services is unencrypted by default, which is acceptable when services run inside a trusted, isolated network boundary such as a Kubernetes pod network with network policies. In a zero-trust architecture — where internal network traffic is not implicitly trusted — the gateway terminates the external TLS session and re-encrypts the forwarded request using mutual TLS toward each downstream service. Certificate management is centralised: the gateway renews and deploys certificates; services do not each need their own certificates or certificate renewal logic. In .NET, Kestrel in the gateway host handles TLS termination; in YARP, the upstream destination URL uses `http://` for internal services while the YARP listener binds to HTTPS for external traffic.

---

## Q13. What is request and response transformation, and why might a gateway need to modify payloads in flight?

**Concepts**
- Header injection for correlation ID and resolved identity
- URL rewriting for external-internal contract separation
- Response filtering to strip internal fields from clients
- YARP and Ocelot declarative transform configuration

**Answer**

Request transformation means the gateway alters the incoming request — changing headers, rewriting URLs, adding query parameters, or modifying the body — before forwarding it downstream. Response transformation means the gateway alters the downstream response — adding headers, stripping sensitive fields, or reformatting JSON — before returning it to the client. Both allow the external API contract to evolve independently of how internal services are structured. Header injection adds `X-Forwarded-For`, `X-Correlation-Id`, or resolved identity headers that services expect but external clients do not provide, without modifying each service. URL rewriting maps an external path such as `/api/v2/orders` to an internal path of `/orders` with no version prefix, allowing the internal URL structure to change without breaking external clients. Response filtering strips internal fields — server version headers, internal service names in error messages — before they reach external clients, reducing information exposure. In YARP, transformations are declared in route configuration or applied in code; in Ocelot, they are configured per route in `ocelot.json` with `UpstreamHeaderTransform` and `DownstreamHeaderTransform` sections.

---

## Q14. How does an API Gateway contribute to observability — logging, metrics, and distributed tracing?

**Concepts**
- Unified structured access log across all microservices
- W3C TraceContext propagation at the gateway chokepoint
- Per-route SLI metrics — request rate, error rate, latency percentiles
- OpenTelemetry integration in YARP and Ocelot

**Answer**

The API Gateway is the single chokepoint through which all external traffic passes, making it the ideal place to emit consistent structured logs, latency metrics, and distributed trace spans covering every request before it reaches any service. Without gateway-level observability, each service logs requests independently, making it difficult to reconstruct a client's full journey across multiple services. Structured access logs record HTTP method, path, client IP, response status code, and request duration for every request — a unified audit log no single service could produce alone. Distributed trace propagation at the gateway generates or forwards W3C TraceContext headers, linking the external client request to every downstream span so the full call tree is visible in tools like Jaeger, Zipkin, or Azure Application Insights. Per-route metrics — request counts, error rates (4xx/5xx), and latency percentiles (p50/p95/p99) — give an SLI view for each upstream service without instrumenting those services separately. In .NET, integrating OpenTelemetry at the YARP or Ocelot layer exports all gateway telemetry to any OpenTelemetry-compatible backend without requiring changes to downstream services.

---

## Q15. What is Ocelot, and how does it work as an API Gateway in an ASP.NET Core application?

**Concepts**
- Ocelot as embedded ASP.NET Core middleware library
- IHttpClientFactory-based request forwarding
- ocelot.json hot-reload without application restart
- Built-in JWT auth, rate limiting, aggregation, and load balancing

**Answer**

Ocelot is an open-source .NET library that transforms an ASP.NET Core application into an API Gateway by embedding a routing and forwarding pipeline inside the standard ASP.NET Core middleware stack. It reads a configuration file (`ocelot.json`) that defines route mappings, matches incoming requests, optionally transforms them, forwards them to upstream services using `HttpClient`, and returns the response. Ocelot is a library rather than a standalone binary — you create a normal ASP.NET Core application, call `AddOcelot()` and `UseOcelot()`, and it runs inside that process alongside any other middleware. It uses `IHttpClientFactory` internally to forward requests, inheriting .NET's connection pooling, timeout configuration, and Polly resilience policy integration. Route configuration is hot-reloadable: Ocelot watches for changes to `ocelot.json` and applies them without restarting, which allows runtime route updates in environments where restarting is costly. Built-in features include JWT authentication, claim-based authorisation, rate limiting, request aggregation, header transformation, load balancing across multiple downstream hosts, and response caching.

---

## Q16. How do you configure routes in Ocelot, and what does a basic ocelot.json configuration look like?

**Concepts**
- UpstreamPathTemplate to DownstreamPathTemplate mapping
- `{everything}` wildcard capture for sub-path forwarding
- DownstreamHostAndPorts for built-in round-robin load balancing
- Route evaluation order — specific patterns before wildcard patterns

**Answer**

Ocelot routes are defined in `ocelot.json` as an array of route objects. Each object maps an `UpstreamPathTemplate` — the path pattern the client sends — to a `DownstreamPathTemplate` and host. The gateway evaluates routes in order and forwards the first match.

```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/api/orders/{everything}",
      "UpstreamHttpMethod": ["GET", "POST"],
      "DownstreamPathTemplate": "/orders/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "order-service", "Port": 80 }]
    }
  ],
  "GlobalConfiguration": { "BaseUrl": "https://api.example.com" }
}
```

The `{everything}` placeholder captures the remainder of the path and carries it verbatim into `DownstreamPathTemplate`, allowing wildcard routing without enumerating every sub-path. `DownstreamHostAndPorts` accepts multiple entries, enabling built-in round-robin load balancing across instances of the same service without an external load balancer. Authentication is added per-route with an `AuthenticationOptions` object specifying the authentication scheme name registered in ASP.NET Core DI. Multiple routes can point to the same downstream service, and Ocelot evaluates them in declaration order — more specific patterns should appear before wildcard patterns.

---

## Q17. How does Ocelot integrate with service discovery (e.g., Consul or Kubernetes), and what is the benefit?

**Concepts**
- Dynamic address resolution vs static DownstreamHostAndPorts
- Consul health-check-filtered instance discovery
- Kubernetes DNS as Ocelot downstream host
- YARP as alternative for live Kubernetes endpoint updates

**Answer**

Ocelot integrates with service discovery tools such as Consul or Eureka by dynamically resolving downstream service addresses at request time rather than hard-coding them in `ocelot.json`. When a service instance registers or deregisters with the discovery tool, Ocelot's routing automatically reflects the change without any configuration file update. With Consul integration via the `Ocelot.Provider.Consul` package, Ocelot queries Consul's catalog to resolve the set of healthy instances for a service name, and Consul health checks ensure Ocelot only forwards traffic to instances that are currently passing their health check. In Kubernetes, DNS-based service discovery is sufficient: Ocelot uses the Kubernetes service DNS name such as `order-service.default.svc.cluster.local` as the downstream host, and Kubernetes's `kube-proxy` handles instance-level load balancing transparently. The benefit is elastic routing — as pods scale up or down or are replaced by rolling deployments, no gateway configuration changes are needed. Ocelot does not natively watch the Kubernetes API for real-time endpoint changes; for Kubernetes-native discovery with live endpoint updates, YARP with its Kubernetes configuration provider or a dedicated ingress controller is more appropriate.

---

## Q18. What are Ocelot's built-in middleware capabilities, and how do you extend them with custom logic?

**Concepts**
- ASP.NET Core middleware before UseOcelot() for gateway-wide concerns
- Per-route DelegatingHandler for upstream-specific request interception
- Rate limiting and caching as configuration-only built-in features
- DelegatingHandler as the primary custom extension point

**Answer**

Ocelot's pipeline includes built-in middleware for rate limiting, JWT authentication, claim-based authorisation, request header transformation, response caching, and load balancing — each configurable per route in `ocelot.json`. For logic not covered by built-in features, you extend Ocelot via ASP.NET Core middleware placed before `UseOcelot()` or via per-route `DelegatingHandler` instances that intercept forwarded `HttpClient` calls. For gateway-wide concerns such as injecting correlation IDs or audit logging, you add a standard ASP.NET Core middleware component before `UseOcelot()` in the pipeline — it runs for every inbound request before Ocelot handles it. For per-route concerns such as adding an internal auth header to a specific upstream or retrying one service with a custom backoff, you register a custom `DelegatingHandler` with DI and reference it in the route's `HttpHandlerOptions.DelegatingHandlers` list in `ocelot.json`. The handler receives the outbound `HttpRequestMessage` and the upstream `HttpResponseMessage`. Rate limiting and caching are configuration-only — no custom code is required to enable them; they are toggled and tuned in `ocelot.json`.

---

## Q19. What are the known limitations of Ocelot in high-throughput or large-scale production environments?

**Concepts**
- Middleware-based throughput ceiling vs YARP low-level I/O pipeline
- Configuration reload stall under traffic with large route sets
- WebSocket proxying as second-class concern requiring workarounds
- Ocelot maintenance mode since mid-2020s

**Answer**

Ocelot is built as an ASP.NET Core middleware library around `HttpClient`, which means its forwarding throughput is limited by the overhead of that middleware pipeline. YARP, by contrast, is purpose-built as a high-performance reverse proxy using ASP.NET Core's low-level I/O pipeline and outperforms Ocelot significantly at high request rates. Ocelot reloads its entire in-memory route configuration when `ocelot.json` changes, so for configurations with hundreds of routes this reload can cause a brief processing stall and may surface race conditions under traffic. WebSocket proxying is not first-class in Ocelot — it requires additional configuration and community-contributed workarounds, whereas YARP supports WebSocket proxying natively. Ocelot's request aggregation capability is also basic: it can fan out to multiple services and merge response bodies, but complex aggregation requiring field selection from multiple JSON responses requires writing a custom aggregation class. Ocelot's community activity and release cadence have slowed significantly since the mid-2020s; the project is largely in maintenance mode, so new .NET projects building an API Gateway should evaluate YARP first and fall back to Ocelot only if its built-in features would meaningfully reduce boilerplate.

---

## Q20. What is YARP, how does it differ from Ocelot, and when would you choose it for a .NET gateway?

**Concepts**
- Low-level I/O pipeline performance vs middleware-based throughput
- Native WebSocket and gRPC proxying
- IProxyConfigProvider for dynamic configuration loading
- Microsoft-backed active development vs Ocelot maintenance mode

**Answer**

YARP (Yet Another Reverse Proxy) is a Microsoft-developed, open-source reverse proxy library for ASP.NET Core built on top of the same low-level I/O infrastructure as Kestrel. Unlike Ocelot, which bundles application-level gateway features into its middleware, YARP focuses on high-performance proxying and delegates application-layer concerns to standard ASP.NET Core middleware and policies. Choose YARP when performance, WebSocket, or gRPC proxying is required, or when you want to compose gateway logic from standard, testable ASP.NET Core middleware. Choose Ocelot when its built-in rate limiting and aggregation would significantly reduce boilerplate in a moderate-throughput environment.

| Feature | Ocelot | YARP |
|---|---|---|
| Performance | Middleware-based; moderate throughput | High-performance; near-raw Kestrel throughput |
| WebSocket support | Limited, requires workarounds | First-class, native |
| gRPC proxying | Not supported natively | Supported |
| Configuration | JSON file (`ocelot.json`) | JSON config or programmatic `IProxyConfigProvider` |
| Rate limiting | Built-in, per-route | Uses ASP.NET Core rate limiting middleware |
| Maintenance | Community, slowing | Microsoft-backed, actively developed |

---

## Q21. How do you configure YARP routes and clusters in an ASP.NET Core application?

**Concepts**
- Route-to-cluster two-level configuration model
- `{**remainder}` catch-all path syntax
- IProxyConfigProvider for dynamic route and cluster loading
- services.AddReverseProxy() and app.MapReverseProxy() wiring

**Answer**

YARP uses a two-level configuration model: a Route defines request matching rules and points to a Cluster; a Cluster defines the pool of backend service addresses and the load balancing policy to use across them. Both levels are defined in `appsettings.json` under the `ReverseProxy` section, or programmatically via `IProxyConfigProvider`.

```json
"ReverseProxy": {
  "Routes": {
    "orders-route": {
      "ClusterId": "orders-cluster",
      "Match": { "Path": "/api/orders/{**remainder}" }
    }
  },
  "Clusters": {
    "orders-cluster": {
      "Destinations": {
        "orders-1": { "Address": "http://order-service:8080/" }
      }
    }
  }
}
```

Routes are keyed by name, reference a `ClusterId`, and use ASP.NET Core route patterns including catch-all `{**remainder}` syntax. One cluster can serve multiple routes. Clusters hold one or more named destinations — YARP load balances across them using the configured policy. YARP resolves configuration dynamically: implementing `IProxyConfigProvider` allows routes and clusters to be loaded from a database, Consul, or the Kubernetes API and refreshed without restarting the application. `services.AddReverseProxy().LoadFromConfig(...)` and `app.MapReverseProxy()` wire YARP into ASP.NET Core DI and the middleware pipeline.

---

## Q22. How does YARP support load balancing, and what load balancing policies does it provide?

**Concepts**
- LeastRequests for variable-response-time service instances
- Passive health checking — remove on consecutive failures, restore after cooldown
- Active health checking via periodic HTTP probes
- ILoadBalancingPolicy for custom consistent hashing

**Answer**

YARP distributes requests across a cluster's destinations using a configurable load balancing policy set per cluster. The policy determines which destination receives each incoming request, and YARP also supports passive and active health checks that automatically remove unhealthy destinations from the rotation. `LeastRequests` is best for services with variable response times because it naturally drains traffic away from slow or overloaded instances. Custom load balancing policies implement `ILoadBalancingPolicy`, enabling use cases such as consistent hashing for routing a given user ID to the same instance for session affinity. Passive health checking removes a destination from rotation after consecutive failures and restores it after a configured recovery period, without any external health-check infrastructure. Active health checking — periodic HTTP probes to a `/healthz` endpoint on each destination — is also supported and provides faster detection of unhealthy instances.

| Policy | Behaviour |
|---|---|
| `RoundRobin` | Rotates evenly across all healthy destinations |
| `Random` | Picks a destination at random on each request |
| `LeastRequests` | Routes to the destination with fewest in-flight requests |
| `PowerOfTwoChoices` | Picks two random destinations and sends to the one with fewer requests |
| `FirstAlphabetical` | Always picks the first destination by name |

---

## Q23. How do you implement request transformation or custom middleware with YARP?

**Concepts**
- MapReverseProxy pipeline middleware for per-request request interception
- Declarative transforms in appsettings for common header and path changes
- Response header removal declarations in route config
- Body buffering cost for request body transforms

**Answer**

YARP provides a declarative transform system for common modifications — path prefix stripping, header injection, query string changes — configured in `appsettings.json` or code. For logic that cannot be expressed as a built-in transform, YARP participates fully in the ASP.NET Core middleware pipeline, so you add middleware before or after `MapReverseProxy()`, or inject custom code directly into the per-request proxy pipeline.

```csharp
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use(async (context, next) =>
    {
        context.Request.Headers["X-Correlation-Id"] = Guid.NewGuid().ToString();
        await next();
    });
});
```

The middleware lambda runs for every proxied request, making it ideal for injecting correlation IDs, audit logging, or modifying a specific header before the request leaves the gateway. Built-in transform declarations in config handle the common cases without code: forwarding `X-Forwarded-For` and `X-Forwarded-Host`, stripping path prefixes, and adding or removing response headers. YARP streams request bodies by default without buffering; transforming the request body requires enabling buffering for that route, which has a memory cost for large payloads.

---

## Q24. What resilience patterns should an API Gateway implement, and how do Polly and gateway-level circuit breakers complement each other?

**Concepts**
- Circuit breaker — open on consecutive failures, half-open on cooldown probe
- Retry limited to idempotent operations to prevent duplicate side effects
- Per-route timeout to prevent connection pool exhaustion
- Polly DelegatingHandler in Ocelot vs IHttpClientFactory in YARP

**Answer**

An API Gateway should implement circuit breaking, retries with exponential backoff, and per-route timeouts to protect clients from downstream service failures. Polly is a .NET resilience library that applies these policies to the `HttpClient` calls the gateway makes to upstream services, integrating with both Ocelot's `DelegatingHandler` extension point and YARP's `IHttpClientFactory` registration. The circuit breaker opens after a configured number of consecutive failures to a downstream service, so the gateway returns 503 immediately for subsequent requests to that service rather than continuing to forward and accumulate latency; after a cooldown period, the circuit half-opens and allows a probe request through. Retries with exponential backoff address transient failures such as network blips or service restarts without the client being aware — retries should only apply to idempotent operations (GET, PUT, DELETE) to avoid creating duplicate side effects on POST. Every downstream call must also have an explicit per-route timeout, because without it a slow upstream service can exhaust the gateway's connection pool and degrade all routes, not just the slow one. In Ocelot, Polly policies attach via `DelegatingHandler` registered on the per-route `HttpClient`; in YARP, Polly integrates through `IHttpClientFactory` using `.AddPolicyHandler()` on the named HTTP client per cluster.

---

## Q25. What security threats does placing an API Gateway in front of microservices help mitigate?

**Concepts**
- Credential theft reduction through raw-token-at-edge validation
- DDoS and API abuse prevention via gateway-level rate limiting
- Service enumeration prevention through hidden internal addresses
- Oversized and malformed request rejection before downstream processing

**Answer**

An API Gateway creates a controlled perimeter through which all external traffic must pass, allowing authentication, input validation, and rate limiting to run once before any downstream service executes. Credential theft exposure is reduced: the gateway validates raw tokens and passes resolved identity headers downstream, so individual services never receive or store raw client credentials. DDoS and API abuse threats are addressed by rate limiting and IP throttling at the gateway, which sheds malicious traffic before it reaches any service — coordinating this across independently exposed services would be impractical. Service enumeration is prevented because only the gateway's address is publicly reachable, so an attacker cannot discover internal service names, ports, framework versions, or error messages from services not intended to be public. The gateway can also reject structurally invalid requests — malformed JSON, oversized payloads, requests missing required headers — before they reach services that might be more vulnerable to such inputs.

---

## Q26. How do you prevent the API Gateway itself from becoming a bottleneck or single point of failure in a distributed system?

**Concepts**
- Stateless gateway replicas behind a load balancer
- Redis for cross-instance rate limiting counters
- HPA and readiness probe in Kubernetes for elastic scaling
- /healthz endpoint for load balancer health-based routing

**Answer**

The API Gateway must be deployed as multiple stateless replicas behind a load balancer so that any single instance can handle any request. Configuration state — routes, policies — is loaded from a shared external source such as configuration files in Git, Consul, or environment variables rather than stored in one instance's memory. In Kubernetes, a `Deployment` with a `HorizontalPodAutoscaler` and a `Service` of type `LoadBalancer` or `ClusterIP` covers this automatically. Statelessness is non-negotiable: JWT validation is stateless by design, but rate limiting counters require a shared external store such as Redis if multiple instances must enforce a combined per-client limit rather than per-instance limits. The gateway is on the critical path for every external request, so it must be provisioned for peak traffic plus headroom. A `/healthz` endpoint allows the load balancer or Kubernetes readiness probe to remove an unhealthy gateway instance from rotation quickly, before clients experience sustained failures.

---

## Q27. When is a dedicated API Gateway unnecessary, and when does it become essential?

**Concepts**
- Simple reverse proxy sufficiency for one-to-three internal-only services
- Client diversity as the threshold trigger for a gateway
- Centralised audit log as regulatory gateway justification
- Cross-cutting concern duplication as cost-effectiveness signal

**Answer**

A dedicated API Gateway is unnecessary when a system has only a small number of services consumed by internal callers under the same security boundary, or when a simple reverse proxy covers the routing requirements without needing application-layer logic. It becomes essential when external clients need a unified API surface across multiple services, or when cross-cutting concerns would otherwise need to be duplicated in every service. Systems with one to three services and a single client type can route traffic with a simple reverse proxy — adding a gateway introduces operational complexity without commensurate benefit. When client diversity increases — web app, mobile app, and third-party API consumers each with different data needs — a gateway or BFF centralises the adaptation that would otherwise live in every service or every client. Regulatory or compliance requirements such as centralised audit logging, PCI-DSS enforcement, or mandatory request signing are far simpler to satisfy at a single gateway than distributed across services. A practical signal: if your team is implementing the same authentication middleware independently in three or more services, a gateway has already become cost-effective.

---

## Q28. What is the "smart endpoints, dumb pipes" philosophy in microservices, and does an API Gateway contradict it?

**Concepts**
- Business logic in services vs infrastructure-only in pipes
- ESB anti-pattern failure from accumulated middleware logic
- Structural aggregation vs domain-rule aggregation boundary
- Gateway compliance test — infrastructure concern or business logic

**Answer**

"Smart endpoints, dumb pipes" is the principle from Sam Newman's *Building Microservices* that business logic belongs in the services and the communication infrastructure should be minimal and generic. This directly opposes the Enterprise Service Bus pattern where routing, orchestration, and transformation logic accumulate in the messaging middleware, making it a hidden bottleneck of business logic. An API Gateway is a pipe — and as long as it performs only infrastructure-layer concerns such as routing, authentication, rate limiting, and logging, it complies with the "dumb pipes" philosophy. The gateway does not decide what the data means, only where to send it. The gateway contradicts the philosophy when it begins implementing business logic: orchestrating multi-step workflows, applying domain-specific transformation rules, or making decisions that belong in a bounded context. Response aggregation for client convenience sits in a grey zone — it is structural composition rather than domain logic, and is widely accepted in BFF implementations. The key test is whether removing the aggregation from the gateway would break a business process, which means it belongs in a service, or merely require more client calls, which is acceptable in the gateway. The lesson from ESB failures is not "never centralise infrastructure" but "never let infrastructure encode business rules."

---

## Q29. How do you manage API Gateway configuration drift as the number of routes and services grows?

**Concepts**
- Gateway config as version-controlled code with pull request review
- OpenAPI-based route validation in CI pipeline
- Kubernetes co-located ingress manifests for co-deployment
- Dynamic IProxyConfigProvider for service instance address currency

**Answer**

Gateway configuration drift occurs when the route definitions in the gateway diverge from the actual API contracts of the services they point to — for example, a service changes its path prefix and the gateway still routes to the old one. The remedy is to treat gateway configuration as code in version control, validate it in a CI pipeline, and where possible generate it from each service's own API specification. Storing `ocelot.json`, YARP `appsettings.json`, or ingress manifests in a Git repository with pull request review required creates an audit trail and prevents ad-hoc runtime edits that nobody else knows about. For Kubernetes deployments, Ingress or Gateway API manifests live alongside the service's Kubernetes manifests in the same repository, so routing and service configuration are co-located and deployed together. Generating gateway routes from OpenAPI specifications — validating that gateway route paths match the spec in a build pipeline step — catches drift before a deployment reaches production. Dynamic configuration via service discovery eliminates drift for service instance addresses; path-level route rules still need explicit versioned management, but at least instance addresses are always current.

---

## Q30. What are the differences between a self-hosted gateway (Ocelot, YARP) and a managed gateway service (Azure API Management, AWS API Gateway)?

**Concepts**
- Operational burden vs customisation freedom trade-off
- Self-hosted low latency vs managed PaaS higher latency
- Azure APIM developer portal and subscription management
- Azure APIM self-hosted gateway component for hybrid deployments

**Answer**

A self-hosted gateway runs inside your own infrastructure — a container you build, deploy, and manage — giving full control over performance, customisation, data residency, and cost model. A managed gateway service is operated by a cloud provider: you configure it through a portal or declarative API, and the provider handles availability, scaling, certificate renewal, and patching. For internal microservices running in a Kubernetes cluster, a self-hosted YARP instance or a Kubernetes-native ingress controller is lower latency, lower cost, and simpler to operate than routing internal traffic through a managed cloud service. Azure API Management (APIM) adds a self-service developer portal for API consumers, subscription and usage-tier management, built-in API analytics, and an inbound/outbound XML policy engine — making it the right choice for public APIs with multiple external, third-party consumers. Azure APIM offers a hybrid option: a self-hosted gateway component that runs inside your cluster but is managed and configured through the APIM portal, providing managed tooling with data-locality control.

| Dimension | Self-Hosted (Ocelot / YARP) | Managed (Azure APIM / AWS API Gateway) |
|---|---|---|
| Operational burden | High — you manage deployment, scaling, upgrades | Low — provider managed |
| Customisation | Unlimited — write any .NET code | Limited to policy engine or extension points |
| Cost | Infrastructure cost only | Per-request or per-unit pricing |
| Latency | Low — co-located with services | Higher — call traverses cloud service boundary |
| Feature set | Build what you need | Rich built-in: developer portal, analytics, subscriptions |
| Data sovereignty | Full control | Data may leave the region depending on configuration |

---

## Gotchas — API Gateway Pattern (Interview Traps)

---

#### Gotcha 1. The Gateway Becomes a Distributed Monolith

**Concepts**
- Business logic accumulating in gateway configuration
- All services forced to deploy in lockstep with gateway changes
- Gateway as a pure routing and cross-cutting concerns layer
- Backend-for-Frontend pattern distributing consumer-specific logic

**Answer**

An API gateway that starts hosting business logic — data transformation, orchestration rules, business validation — becomes a distributed monolith: every service change requires a gateway update, a central team owns a deployment bottleneck, and the gateway becomes the hardest single component to change in the system. The gateway should only perform cross-cutting infrastructure concerns: authentication, rate limiting, SSL termination, routing, and logging — nothing that requires knowledge of business rules. When consumer-specific aggregation or transformation is genuinely needed, a Backend-for-Frontend (BFF) service owned by the consuming team is the correct pattern, keeping the gateway free of business logic.

---

#### Gotcha 2. Fan-Out Latency Not Bounded With a Timeout

**Concepts**
- Aggregation endpoint calling multiple upstream services in parallel
- Slowest service determining the response latency without a timeout
- Task.WhenAll without cancellation token causing indefinite wait
- Circuit breaker and timeout on each upstream call

**Answer**

A gateway aggregation endpoint that calls three upstream services in parallel with `Task.WhenAll` will block until all three complete — if one service has a 30-second timeout or is hanging, the gateway holds the client connection open for 30 seconds on every request, exhausting connection pool capacity. Each upstream call must carry its own cancellation token with a short deadline, and the aggregation must decide whether to return partial results or fail fast when any upstream times out. Interviewers probe this by asking what happens to the gateway when one of five upstream services becomes slow — the expected answer includes per-call timeouts, partial result handling, and circuit breaker state.

---

#### Gotcha 3. Rate Limiting Not Scoped to Client Identity

**Concepts**
- Global rate limit throttling all clients equally
- One misbehaving client consuming all available capacity
- Per-client rate limit keyed by API key, IP, or authenticated user
- Rate limit header communication to clients

**Answer**

A global rate limit that allows 1000 requests per second across all clients means a single script or misbehaving API consumer can exhaust the entire quota and throttle legitimate users to zero. Rate limits must be scoped to a client identity — API key, authenticated user ID, or IP address — so each client has its own independent quota. Gateways like YARP and Azure APIM support per-key rate limiting; for custom implementations in ASP.NET Core, the `Microsoft.AspNetCore.RateLimiting` middleware supports keyed policies via `GetHttpContext().User` or request headers. The gateway should also return `Retry-After` and `X-RateLimit-*` headers so clients can self-regulate.

---

#### Gotcha 4. Circuit Breaker Placed Only at the Gateway

**Concepts**
- Gateway circuit breaker protecting the gateway, not services
- Service-to-service calls bypassing the gateway
- Circuit breaker needed at each service client, not just the entry point
- Polly circuit breaker per HttpClient in each microservice

**Answer**

Placing a circuit breaker only in the API gateway protects clients from direct gateway failures, but service-to-service calls that bypass the gateway — internal gRPC calls, message-triggered service invocations — have no protection at all. When Service A calls Service B internally and Service B is failing, the circuit breaker at the gateway does nothing to prevent A from exhausting its thread pool waiting for B. Each service must have its own circuit breaker on every `HttpClient` or gRPC channel that calls a downstream dependency, implemented with Polly `CircuitBreakerPolicy` or `CircuitBreakerAsync`. The gateway's circuit breaker is an additional layer for client-facing traffic, not a substitute for service-level resilience.

---

#### Gotcha 5. SSL Termination at Gateway Without Re-Encryption to Backends

**Concepts**
- mTLS between gateway and backend services
- Plain HTTP on the internal network as a security gap
- Zero-trust network model requiring encryption everywhere
- Service mesh providing transparent mutual TLS

**Answer**

Terminating TLS at the gateway and forwarding requests to backend services over plain HTTP on the internal network creates an unencrypted communication path that is vulnerable to lateral movement attacks if an attacker gains access to the internal network segment — a zero-trust security model requires encryption everywhere, not just at the perimeter. For sensitive workloads, the gateway should re-encrypt traffic to backends using mTLS (mutual TLS), which also authenticates both ends of the connection. A service mesh like Istio or Linkerd handles mTLS transparently at the infrastructure layer without requiring application code changes, making zero-trust achievable without gateway-specific configuration for each backend.

---

#### Gotcha 6. Authentication at the Gateway Without Authorization at Each Service

**Concepts**
- Authentication verifying identity versus authorization checking permissions
- Authenticated request assumed authorised by backend service
- Confused deputy attack via gateway identity forwarding
- Each service validating claims it cares about

**Answer**

Validating JWT tokens at the gateway and forwarding requests to backend services as "authenticated" does not mean those services should trust the request to perform any operation — a valid token from a standard user could reach an admin endpoint if the backend does not check the `role` or `scope` claim. Each service must perform its own authorization check on the claims forwarded in the request header; the gateway performs authentication (is this a valid token from our issuer?) while each service performs authorization (does this token have the `orders:write` scope needed for this endpoint?). Delegating all authorization to the gateway creates a confused-deputy vulnerability.

---

#### Gotcha 7. No Health Check Integration in the Gateway Routing Table

**Concepts**
- Gateway routing to unhealthy service instances
- Health check response determining routing eligibility
- Active versus passive health checks
- Circuit breaker opening on repeated health check failures

**Answer**

An API gateway that does not integrate service health checks will continue routing requests to a backend instance that has failed, returning 500s or timeouts to clients even though healthy replicas exist. Active health checks — the gateway periodically probing a `/health` endpoint on each backend — allow it to remove failing instances from the routing pool within seconds of detection. Passive health checks mark an instance unhealthy based on consecutive error responses. YARP supports both active and passive health checks with configurable thresholds; without this, the gateway acts as a dumb router and amplifies backend failures into user-visible errors instead of routing around them.

---

#### Gotcha 8. Caching Responses for Non-Idempotent or User-Specific Endpoints

**Concepts**
- Response cache hit returning another user's data
- POST or PATCH response cached and replayed incorrectly
- Cache key must include user identity for personalised responses
- Cache-Control directives as the authoritative source

**Answer**

Enabling response caching at the gateway for endpoints that return user-specific data — account details, order history — without including the user identity in the cache key will return one user's data to a different user who makes the same request with a different token. Non-idempotent methods (POST, PATCH, DELETE) must never be cached since replaying a cached POST response can cause the client to believe it successfully submitted data it never actually sent. Cache keys must include every dimension that affects the response: user ID, locale, requested resource version, and any relevant query parameters.

---

#### Gotcha 9. Gateway Performing Service Discovery Lookups on Every Request

**Concepts**
- DNS or service registry lookup on every inbound request adding latency
- Cached address table with TTL-based refresh
- Stale address causing routing to a deregistered instance
- Periodic refresh versus per-request lookup trade-off

**Answer**

Resolving the backend service address from Consul or Kubernetes DNS on every inbound request adds a synchronous network round trip to every request's latency — under high load this compounds and can add tens of milliseconds per request. The gateway should maintain a cached address table, refreshed periodically on a background timer (typically every few seconds to a minute), and use health-check-confirmed addresses from the cache. The trade-off is staleness: a too-long TTL means the gateway routes to a deregistered instance after a deployment; a too-short TTL means frequent discovery lookups. Combining periodic refresh with passive health checks that immediately remove failing addresses balances freshness and performance.

---

#### Gotcha 10. Missing Correlation ID Injection at the Gateway

**Concepts**
- Correlation ID generated at the gateway entry point
- X-Correlation-ID header forwarded to all downstream services
- Downstream logs without correlation ID being un-traceable
- Gateway as the authoritative source of the trace root

**Answer**

Without a correlation ID injected at the gateway, every downstream service generates its own unrelated request IDs, making it impossible to correlate log entries across services for a single user-facing request. The API gateway is the ideal injection point: if the incoming request carries an `X-Correlation-ID` header (from a client that generated one), forward it; if not, generate a new GUID and inject it into all forwarded requests. Every downstream service must read this header and include its value in all structured log entries using Serilog's `LogContext` or OpenTelemetry's baggage propagation. A request that fails deep in Service D becomes trivially diagnosable when every log entry from gateway to database carries the same correlation ID.

---
