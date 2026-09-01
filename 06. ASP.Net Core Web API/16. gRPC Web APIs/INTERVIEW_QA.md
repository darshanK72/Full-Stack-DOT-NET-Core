# gRPC Web APIs — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is gRPC?](#q1-what-is-grpc)
2. [Q2. What is the difference between gRPC and REST?](#q2-what-is-the-difference-between-grpc-and-rest)
3. [Q3. What are Protocol Buffers?](#q3-what-are-protocol-buffers)
4. [Q4. What is a `.proto` file?](#q4-what-is-a-proto-file)
5. [Q5. What is gRPC-Web?](#q5-what-is-grpc-web)
6. [Q6. Why can't browsers use native gRPC directly?](#q6-why-cant-browsers-use-native-grpc-directly)
7. [Q7. What is a unary gRPC call?](#q7-what-is-a-unary-grpc-call)
8. [Q8. What is server streaming in gRPC?](#q8-what-is-server-streaming-in-grpc)
9. [Q9. What is `RpcException`?](#q9-what-is-rpcexception)
10. [Q10. What are gRPC status codes?](#q10-what-are-grpc-status-codes)
11. [Q11. What is a deadline in gRPC?](#q11-what-is-a-deadline-in-grpc)
12. [Q12. How does cancellation work in gRPC?](#q12-how-does-cancellation-work-in-grpc)
13. [Q13. What is backward compatibility in Protocol Buffers?](#q13-what-is-backward-compatibility-in-protocol-buffers)
14. [Q14. What is `ServerCallContext`?](#q14-what-is-servercallcontext)
15. [Q15. What is the difference between gRPC and JSON HTTP APIs?](#q15-what-is-the-difference-between-grpc-and-json-http-apis)
16. [Q16. What is `AddGrpc` used for?](#q16-what-is-addgrpc-used-for)
17. [Q17. What is `MapGrpcService`?](#q17-what-is-mapgrpcservice)
18. [Q18. When should you choose gRPC over REST?](#q18-when-should-you-choose-grpc-over-rest)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is gRPC?

What is gRPC?

**Answer:** gRPC is a high-performance RPC framework using HTTP/2 and Protocol Buffers for contract-first, strongly typed service-to-service communication. Clients and servers generate stubs from `.proto` files, calling methods like local functions with binary serialization instead of JSON.

- Built on HTTP/2 multiplexing, header compression, and bidirectional streaming support.
- First-class in .NET via `Grpc.AspNetCore` with Kestrel as the server.
- Ideal for low-latency internal microservice calls on .NET, Go, Java, and other supported languages.
- Uses `.proto` contracts versioned independently of REST URL paths.

---

## Q2. What is the difference between gRPC and REST?

What is the difference between gRPC and REST?

**Answer:** REST models resources with HTTP verbs, JSON payloads, and standard status codes on many URLs; gRPC models RPC methods on a service with Protobuf messages over HTTP/2 on typically one base path per service. REST is human-readable and browser-friendly; gRPC is binary, contract-strict, and optimized for service meshes.

- REST: `GET /api/orders/1` returns JSON; clients infer shape from documentation or OpenAPI.
- gRPC: `GetOrder(OrderRequest)` returns typed `OrderReply` — compiler-checked on both sides.
- REST leverages HTTP caching and CDN; gRPC requires grpc-web and different caching strategies for browsers.
- gRPC supports streaming (server, client, bidirectional); REST traditionally uses chunked HTTP or SSE/WebSockets separately.

---

## Q3. What are Protocol Buffers?

What are Protocol Buffers?

**Answer:** Protocol Buffers (protobuf) are Google's language-neutral serialization format defined in `.proto` files. Messages declare typed fields with numbered tags; the compiler generates C# classes and serialization code that produces compact binary payloads on the wire.

- Smaller and faster to serialize/deserialize than JSON for structured data.
- Field numbers identify wire data — names are not sent on the wire.
- `proto3` is the current syntax for new gRPC services in .NET.
- Backward compatibility depends on field number rules, not C# property names.

---

## Q4. What is a `.proto` file?

What is a `.proto` file?

**Answer:** A `.proto` file defines gRPC services, RPC methods, request/response messages, and enums in a language-neutral contract. The .NET build integrates `Grpc.Tools` to generate C# server base classes and client stubs from the proto at compile time.

- Declares `service OrderService { rpc GetOrder(OrderRequest) returns (OrderReply); }`.
- Messages specify fields as `type name = number;` — numbers are permanent wire identifiers.
- Shared `.proto` files enable polyglot clients (Java mobile app, .NET backend) from one contract.
- Place protos in the project with `<Protobuf Include="Protos\order.proto" GrpcServices="Server" />` in the `.csproj`.

---

## Q5. What is gRPC-Web?

What is gRPC-Web?

**Answer:** gRPC-Web is a protocol variant that lets browser clients call gRPC services over HTTP/1.1 or HTTP/2 with JSON-like framing adapters, because browsers cannot use native gRPC's HTTP/2 trailing headers directly. ASP.NET Core enables it with `AddGrpc().EnableGrpcWeb()` and `UseGrpcWeb()` middleware.

- Required for SPA/browser consumers without a native gRPC stack.
- Often deployed behind Envoy or nginx that translates grpc-web to native gRPC.
- Needs explicit CORS configuration for cross-origin browser apps.
- Unary calls are most common; streaming support in browsers is more limited than server-to-server gRPC.

---

## Q6. Why can't browsers use native gRPC directly?

Why can't browsers use native gRPC directly?

**Answer:** Native gRPC relies on HTTP/2 features — trailing headers for status, binary framing, and full duplex streaming — that browser `fetch` and `XMLHttpRequest` APIs do not expose completely. Browsers lack a first-class gRPC client without grpc-web translation or a proxy.

- gRPC status and metadata arrive in HTTP/2 trailers inaccessible to standard browser HTTP APIs.
- Corporate proxies and HTTP/1.1-only paths break native gRPC from client-side JavaScript.
- grpc-web wraps calls in forms browsers can send; a gateway converts to native gRPC server-side.
- Mobile and server .NET clients use `Grpc.Net.Client` with full HTTP/2 support — no grpc-web needed.

---

## Q7. What is a unary gRPC call?

What is a unary gRPC call?

**Answer:** A unary call is the simplest gRPC pattern — one client request message and one server response message, analogous to a REST request/response. Most CRUD-style RPC methods are unary: `GetOrder`, `CreateOrder`, `CancelOrder`.

- Client awaits `var reply = await client.GetOrderAsync(request);`.
- Maps naturally to single database lookups or command operations.
- Uses one HTTP/2 stream opened and closed for the call.
- Deadlines and cancellation tokens apply to the entire round trip.

---

## Q8. What is server streaming in gRPC?

What is server streaming in gRPC?

**Answer:** Server streaming RPCs send one client request and a stream of multiple server response messages — useful for large result sets, live updates, or file chunks without loading everything into memory. The client reads messages asynchronously from `AsyncServerStreamingCall`.

- Defined in `.proto` as `rpc ListOrders(OrderFilter) returns (stream OrderReply);`.
- Server calls `await responseStream.WriteAsync(order)` repeatedly until complete.
- More efficient than paginated REST when the client consumes data incrementally.
- Client must handle backpressure and cancellation mid-stream via `CancellationToken`.

---

## Q9. What is `RpcException`?

What is `RpcException`?

**Answer:** `RpcException` is the gRPC-specific exception type carrying a `StatusCode` (similar to gRPC status codes) and detail message. Servers throw it to signal client errors; clients catch it to distinguish `NotFound`, `InvalidArgument`, and `DeadlineExceeded` from transport failures.

- Throw: `throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));`.
- Clients inspect `ex.StatusCode` and `ex.Trailers` for structured error metadata.
- Map business validation failures to `InvalidArgument` rather than generic `Internal` for clearer client handling.
- Do not leak stack traces or internal details in production status messages.

---

## Q10. What are gRPC status codes?

What are gRPC status codes?

**Answer:** gRPC status codes are standardized result indicators parallel to HTTP status codes but carried in HTTP/2 trailers — `OK`, `Cancelled`, `InvalidArgument`, `NotFound`, `AlreadyExists`, `PermissionDenied`, `Unauthenticated`, `ResourceExhausted`, `FailedPrecondition`, `Aborted`, `OutOfRange`, `Unimplemented`, `Internal`, `Unavailable`, `DeadlineExceeded`, and others.

- `OK` (0) means success; non-zero codes indicate failure at the RPC layer.
- Map domain errors consistently — e.g., duplicate create → `AlreadyExists`, auth failure → `Unauthenticated` vs `PermissionDenied`.
- Clients retry idempotent calls on `Unavailable` and `DeadlineExceeded` with backoff.
- Status details can include `google.rpc.Status` protobuf extensions for structured error info.

---

## Q11. What is a deadline in gRPC?

What is a deadline in gRPC?

**Answer:** A deadline specifies the absolute time by which an RPC must complete — propagated from client to server so work stops when the budget expires. Clients set `CallOptions(deadline: DateTime.UtcNow.AddSeconds(5))`; servers observe `ServerCallContext.Deadline`.

- Prevents hung calls from tying up threads and database connections indefinitely.
- When exceeded, the call ends with status `DeadlineExceeded`.
- Server code must pass `context.CancellationToken` to EF and downstream calls to honor the deadline.
- Chain deadlines through microservice calls — each hop should use the remaining budget, not a fresh full timeout.

---

## Q12. How does cancellation work in gRPC?

How does cancellation work in gRPC?

**Answer:** gRPC cancellation propagates a `CancellationToken` from client to server over HTTP/2 when the client cancels or the deadline passes. Server methods should pass `ServerCallContext.CancellationToken` to long-running EF queries, HTTP calls, and loops so work stops promptly.

- Client: `cts.Cancel()` or dispose the call disposes the underlying stream.
- Server: `await context.CancellationToken.ThrowIfCancellationRequested()` or pass token to `FirstOrDefaultAsync(..., token)`.
- Ignoring cancellation wastes SQL and thread pool resources after the client already disconnected.
- Map cooperative cancellation to `StatusCode.Cancelled` when appropriate.

---

## Q13. What is backward compatibility in Protocol Buffers?

What is backward compatibility in Protocol Buffers?

**Answer:** Protobuf wire compatibility requires never changing the wire type or number of existing fields and never reusing field numbers for different semantics. New fields are added with new numbers; old clients ignore unknown fields; new clients use default values for missing old fields.

- Renaming a field in `.proto` is safe — only tag numbers matter on the wire.
- Changing field 2 from `double` to `string` breaks old clients — use a new field number instead.
- Mark deprecated fields with `deprecated = true` and reserve removed numbers with `reserved`.
- Mobile apps lag server deploys — treat `.proto` changes as multi-version contracts.

---

## Q14. What is `ServerCallContext`?

What is `ServerCallContext`?

**Answer:** `ServerCallContext` is passed to every gRPC service method, providing request metadata (headers), response trailers, peer identity, deadline, and `CancellationToken`. It is the gRPC equivalent of `HttpContext` for RPC handlers.

- Read client metadata: `context.RequestHeaders.GetValue("correlation-id")`.
- Write trailers: `context.ResponseTrailers.Add("processed-by", "orders-service")`.
- Access `context.User` after authentication middleware maps credentials.
- Use `context.CancellationToken` for all async I/O in the service method.

---

## Q15. What is the difference between gRPC and JSON HTTP APIs?

What is the difference between gRPC and JSON HTTP APIs?

**Answer:** JSON HTTP APIs (ASP.NET Core controllers or Minimal APIs) serialize text payloads negotiated via `Content-Type`, discovered through OpenAPI, and consumed universally including browsers. gRPC uses binary Protobuf over HTTP/2 with generated stubs — faster and stricter but less visible without specialized tools.

- JSON: human-readable, easy debugging with curl, broad client support, Swagger documentation.
- gRPC: smaller payloads, strongly typed contracts, built-in streaming, better performance for internal calls.
- JSON APIs suit public partners and SPAs; gRPC suits service-to-service on shared `.proto` contracts.
- ASP.NET Core 8 hosts both in one application — expose REST at the edge, gRPC internally.

---

## Q16. What is `AddGrpc` used for?

What is `AddGrpc` used for?

**Answer:** `AddGrpc()` registers gRPC server services, interceptors, and Kestrel configuration needed to host gRPC endpoints in ASP.NET Core 8. Chain `.AddServiceOptions<T>()` for interceptors, compression, and message size limits.

- Called in `Program.cs`: `builder.Services.AddGrpc();`.
- Add `.EnableGrpcWeb()` when browser clients use grpc-web.
- Register interceptors for logging, auth, and exception translation globally or per service.
- Works alongside `AddControllers()` — REST and gRPC share the same DI container and auth configuration.

---

## Q17. What is `MapGrpcService`?

What is `MapGrpcService`?

**Answer:** `MapGrpcService<TService>()` maps a concrete gRPC service implementation to its RPC methods on the Kestrel endpoint routing table. The generated base class from `.proto` defines overrides the application implements.

- `app.MapGrpcService<OrderServiceImpl>();` after `app.Build()`.
- Combine with `RequireHost`, TLS, and authorization policies on the endpoint.
- For grpc-web: `app.MapGrpcService<OrderServiceImpl>().EnableGrpcWeb().RequireCors("spa");`.
- Each service class inherits from the generated `OrderService.OrderServiceBase`.

---

## Q18. When should you choose gRPC over REST?

When should you choose gRPC over REST?

**Answer:** Choose gRPC for internal microservice communication where both ends share generated contracts, need low latency, high throughput, streaming, or strict typing — especially .NET-to-.NET or polyglot backends behind a service mesh. Choose REST for public APIs, browser clients, partner integrations, and scenarios requiring HTTP caching and human-readable debugging.

- gRPC: order processing between inventory, payment, and fulfillment services on HTTP/2 with deadlines.
- REST: mobile app and third-party partner APIs documented with OpenAPI and consumed via standard HTTP.
- Hybrid: gRPC inside the cluster; REST or BFF at the API gateway for external consumers.
- Evaluate operational cost — grpc-web, proxies, and protobuf governance add complexity REST avoids at the edge.

---

---

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

#### Gotcha 1. POST returning 200 instead of 201

**Answer:** A successful resource creation with POST should return HTTP 201 Created and tell the client where the new resource lives — returning 200 OK omits that contract and breaks REST clients that rely on status codes and the Location header.

- Use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to return 201 with a Location header pointing at the new resource URL.
- Include the created representation or a minimal payload in the response body when clients need immediate data without a follow-up GET.
- Returning 200 for create operations hides the new resource URL from standard HTTP client libraries and OpenAPI-generated SDKs.

---

#### Gotcha 2. GET that mutates state

**Answer:** GET must be safe and idempotent — performing deletes or updates on GET violates HTTP semantics, breaks caching proxies, and creates security holes when URLs are prefetched, logged, or opened in email clients.

- Browsers, CDNs, and link-preview crawlers may invoke GET URLs without user intent, so side effects run unintentionally.
- Cached GET responses can replay destructive operations or stale mutations across clients.
- Use POST, PUT, PATCH, or DELETE for state changes and keep GET read-only.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Answer:** Business failures must map to appropriate 4xx or 5xx status codes — a 200 response with an error flag forces every client to parse the body instead of using standard HTTP semantics, retries, and monitoring.

- Return `ValidationProblemDetails` or `ProblemDetails` with 400 for validation failures and 404, 409, or 422 for domain errors.
- HTTP status codes drive client retry logic, API gateways, and APM alerting; a 200 masks failures in dashboards.
- Envelope patterns like `{ success: false }` require custom handling in every consumer and break OpenAPI contract expectations.

---

#### Gotcha 4. Returning EF entities from API actions

**Answer:** EF Core entities expose navigation properties, shadow fields, and circular references that are not meant for public contracts — serialize DTOs with explicit shapes and never leak database schema to clients.

- Lazy-loaded navigations trigger N+1 queries during serialization and can pull entire object graphs into the response.
- Circular references between entities cause JSON serializer loops or require fragile reference-handling settings.
- DTOs decouple the API contract from schema migrations and let you expose only the fields clients need.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Answer:** ASP.NET Core 8 defaults to camelCase JSON via `System.Text.Json` — PascalCase property names from some clients bind as missing properties, leaving model properties at default values and causing silent data loss on POST and PUT.

- `[JsonPropertyName("PropertyName")]` or a custom `PropertyNamingPolicy` aligns server expectations with legacy client payloads.
- Enable `PropertyNameCaseInsensitive = true` in `AddControllers().AddJsonOptions(...)` when you must accept mixed casing.
- Silent binding failures produce 201/204 success responses with partially saved data and no validation error.

---

#### Gotcha 6. GET with `[FromBody]`

**Answer:** Many HTTP clients, proxies, and caches ignore or strip GET request bodies — filters sent as JSON in GET requests fail silently or never reach the action in ASP.NET Core 8 Web API.

- Model binding for `[FromBody]` on GET is not reliably supported across the HTTP ecosystem.
- Use query strings with `[FromQuery]` for simple filters or POST to a dedicated search endpoint for complex filter objects.
- OpenAPI tools and browser fetch also discourage or block GET bodies, making the pattern fragile in production.

---

#### Gotcha 7. CORS as server security

**Answer:** CORS is enforced by browsers only — it does not stop curl, Postman, server-to-server calls, or direct API requests; authentication and authorization still protect the API.

- CORS headers tell a browser whether JavaScript on one origin may read a cross-origin response; they do not authenticate callers.
- A public API without auth remains fully accessible to any non-browser client regardless of CORS policy.
- Register `AddCors` and `UseCors` for browser SPA access, and enforce JWT, cookies, or API keys separately for real security.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Answer:** Browsers reject `Access-Control-Allow-Origin: *` when the request sends cookies or authorization headers — you must specify explicit origins with `WithOrigins` and call `AllowCredentials`.

- `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined; ASP.NET Core will not emit a valid CORS response for credentialed requests.
- List every trusted frontend origin explicitly, including local dev URLs and production domains.
- Credentialed cross-origin calls require both matching origins and `Access-Control-Allow-Credentials: true`.

---

#### Gotcha 9. Swagger UI exposed in Production

**Answer:** Public Swagger UI discloses the full API surface, schemas, and try-it-out access — gate it behind authentication or disable it outside Development and Staging in ASP.NET Core 8.

- `MapSwagger` and `UseSwaggerUI` in `Program.cs` should be wrapped in environment checks or authorization middleware.
- Exposed OpenAPI documents reveal internal endpoints, field names, and enum values useful for reconnaissance.
- Production APIs typically serve OpenAPI only to authenticated developers or internal tooling, not the public internet.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Answer:** Without `[ApiController]`, automatic 400 `ValidationProblemDetails`, binding source inference, and attribute routing behaviors differ — mixed controllers in the same Web API produce inconsistent error contracts.

- `[ApiController]` enables automatic model-state validation responses and `[FromBody]` inference for complex types.
- Controllers missing the attribute may return 200 with invalid models or require manual `ModelState` checks.
- Apply `[ApiController]` at the controller or assembly level so every endpoint shares the same API conventions.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Answer:** Blocking on `.Result` or `.Wait()` in async API actions causes thread-pool starvation and deadlocks under load — always `await` async service and database calls in ASP.NET Core 8.

- Sync-over-async ties up request threads while I/O completes, reducing throughput on Kestrel under concurrent load.
- Deadlocks occur when the blocked thread holds a synchronization context the continuation needs to resume.
- Mark controller actions `async Task<IActionResult>` and propagate `await` through the service layer to EF Core and HTTP clients.

---

#### Gotcha 12. Liveness probe includes SQL check

**Answer:** If the liveness probe fails when SQL is down, Kubernetes restarts pods that cannot fix the dependency — put SQL, Redis, and external service checks on readiness only.

- Liveness answers whether the process should be killed and restarted; a down database is not healed by restarting the app.
- Readiness removes the pod from the load balancer until dependencies recover without unnecessary restarts.
- Map `/health/live` to a lightweight self-check and `/health/ready` to `AddDbContextCheck` or custom dependency tags.

---

#### Gotcha 13. N+1 queries in list endpoints

**Answer:** Returning entities with lazy-loaded navigation properties triggers one SQL query per row — use projection with `Select`, explicit `Include`, or DTO mapping to fetch list data in a bounded number of queries.

- Serializing a list of `Order` entities with `Customer` navigation can execute 1 + N queries under default lazy loading.
- Project directly to DTOs in LINQ so EF Core generates a single query with only the columns needed.
- For graphs that must be included, use `Include`/`ThenInclude` or split queries deliberately rather than relying on lazy load during JSON output.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Answer:** Concurrent inserts and deletes between offset pages cause duplicate or skipped rows — use keyset or cursor pagination ordered by a stable, indexed key for large datasets in Web API list endpoints.

- `Skip((page - 1) * pageSize).Take(pageSize)` shifts the window when rows are added or removed between requests.
- Keyset pagination uses `WHERE id > @lastId ORDER BY id LIMIT @pageSize` with the last seen key from the previous response.
- Offset pagination remains acceptable for small, mostly static tables; expose cursor tokens in link headers or response metadata for high-churn data.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Answer:** Field resolvers in HotChocolate or other GraphQL servers that query the database per parent row explode SQL under load — batch related loads with DataLoader or resolve joins at the root query.

- A list of 100 authors each resolving `books` individually executes 101 queries instead of one batched query.
- Register DataLoader services in DI so concurrent field resolutions within a request are grouped into single round-trips.
- Eager-load or project at the root query when the client always requests nested fields together.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Answer:** Native gRPC uses HTTP/2 binary framing that browsers do not expose to JavaScript — browser clients need gRPC-Web middleware plus CORS configuration in ASP.NET Core 8.

- Standard `@grpc/grpc-js` in Node or .NET clients works server-to-server; Blazor WASM and SPA browsers require the gRPC-Web protocol.
- Add `AddGrpcWeb()` and `EnableGrpcWeb()` on mapped gRPC services to translate between gRPC-Web and native gRPC.
- Configure CORS for the browser origin alongside gRPC-Web, since cross-origin browser calls still enforce CORS on preflight and response headers.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (D) A fintech team must expose an **order status** API to (a) a React SPA in the browser, (b) an internal .NET microservice, and (c) a partner's legacy HTTP JSON client. They propose gRPC for all three. What would you recommend per consumer, and why?

---

**Answer:**

**Answer:** **gRPC vs REST choice** depends on client capabilities, not server preference alone. Internal .NET service-to-service is gRPC's sweet spot; browsers and legacy JSON partners need different surfaces.

- **(a) React SPA:** **REST or BFF + JSON** (or **gRPC-Web** with Envoy/nginx translation if team accepts binary framing, CORS, and limited browser tooling). Native gRPC from browser is not standard — grpc-web adds complexity. Prefer REST/JSON for public browser APIs unless latency and contract rigor justify grpc-web infrastructure.
- **(b) Internal .NET microservice:** **gRPC** — HTTP/2, strong contracts via `.proto`, streaming, deadlines, efficient serialization. Use mTLS or internal auth between services.
- **(c) Partner legacy JSON:** **REST/JSON** with OpenAPI — partners cannot regenerate stubs from your `.proto`; forcing gRPC blocks integration. Version HTTP API separately.

**Production takeaway:** Use gRPC **inside** the mesh; expose **REST at the edge** for browsers and external HTTP clients unless you operate grpc-web end-to-end deliberately.

---

---

#### Q2. (R) Review this `.proto` change shipped as a "backward compatible" release. Old mobile clients crash on deserialize; server logs `InvalidProtocolBufferException`.

```protobuf
// v1
message TransferRequest {
  int32 account_id = 1;
  double amount = 2;
}

// v2 — "just renamed for clarity"
message TransferRequest {
  int32 account_id = 1;
  string currency_code = 2;  // was amount — type changed
  double amount = 3;
}
```

Deploy strategy was blue/green with no client coordination.

---

**Answer:**

**Answer:** Protobuf wire compatibility requires **never reusing field numbers** and **never changing wire types** of existing numbers. Renaming is fine; moving `amount` from field 2 to 3 while inserting `string` at field 2 **breaks** old clients that still send/read field 2 as `double`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Versioning | Changed field 2 type `double` → `string` | Old clients deserialize garbage / throw |
| Field numbers | Reassigned semantics on same tag | Wire format incompatible — not fixable with blue/green |
| Process | No client coordination | Mobile apps lag server deploys by weeks |

**Fix (priority order):**

1. **Add** new fields with **new numbers** only — keep field 2 as `double amount` (deprecated), add `string currency_code = 4`, `double amount_v2 = 3` if needed — follow protobuf reserved/deprecated annotations.
2. Use `reserved 2;` only after all clients migrated — never recycle numbers.
3. Server accepts both shapes during transition; clients regenerate from same `.proto` version.
4. Document **protobuf versioning** in API governance — breaking wire = new package/service name if necessary (`v2.OrderService`).

**Production takeaway:** **Protobuf versioning** is about **field numbers and wire types**, not C# property names — incompatible `.proto` changes are not saved by deployment strategy alone.

---

---

#### Q3. (R) Review this gRPC service method. Callers report hung requests when downstream SQL is slow; cancellation from the client never stops the query.

```csharp
public class OrderService : Order.OrderBase
{
    private readonly AppDbContext _db;

    public override async Task<OrderReply> GetOrder(
        OrderRequest request,
        ServerCallContext context)
    {
        var order = await _db.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId);

        if (order is null)
            throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));

        return Map(order);
    }
}
```

Client sets `deadline: DateTime.UtcNow.AddSeconds(5)`; EF query uses default `CancellationToken.None`.

---

**Answer:**

**Answer:** gRPC **deadlines** and client cancellation propagate through `ServerCallContext.CancellationToken` — EF must receive that token or SQL continues after the client disconnects, wasting DB resources and holding threads.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cancellation | `FirstOrDefaultAsync()` uses default token | Client deadline ignored; query runs to completion |
| Resource | Hung SQL under load | Connection pool exhaustion |
| gRPC contract | Deadline exceeded not returned promptly | Clients see timeout instead of `DeadlineExceeded` |

**Fix (priority order):**

1. Pass token: `FirstOrDefaultAsync(o => o.Id == request.OrderId, context.CancellationToken)`.
2. Map `OperationCanceledException` to `RpcException(StatusCode.DeadlineExceeded)` when `context.CancellationToken.IsCancellationRequested`.
3. Configure command timeout aligned with max deadline budget.
4. Client: set `CallOptions(deadline: DateTime.UtcNow.AddSeconds(5))` on stub calls.

**Production takeaway:** **Deadline/cancellation** must flow **HTTP/gRPC → EF → SQL** — default `CancellationToken.None` in data layer defeats gRPC's main reliability feature.

---

---

#### Q4. (R) Review `Program.cs` for a gRPC API consumed from a browser via **gRPC-Web**. Preflight succeeds but unary calls return 415; Chrome shows `application/grpc-web-text` rejected.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<GreeterService>();
app.Run();
```

No CORS, no `UseGrpcWeb()`, Kestrel listens HTTPS only on port 5001. Frontend uses `@grpc/grpc-web` against `https://api.example.com`.

---

**Answer:**

**Answer:** Browser clients speak **gRPC-Web**, not native gRPC over HTTP/2 cleartext/h2 the same way server-to-server clients do. Kestrel must enable **gRPC-Web middleware**, CORS for the SPA origin, and often dual endpoint configuration (HTTP/1.1 for grpc-web).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Middleware | Missing `UseGrpcWeb()` / `EnableGrpcWeb()` on endpoint | Server rejects grpc-web content types (415) |
| CORS | No policy for browser origin | Preflight or response blocked |
| Protocol | Only HTTPS h2 without grpc-web bridge | Browser cannot speak native gRPC |

**Fix (priority order):**

1. `app.UseGrpcWeb();` and `app.MapGrpcService<GreeterService>().EnableGrpcWeb().RequireCors("Spa");`
2. Configure CORS: allow origin, `POST`, headers `grpc-timeout`, `x-grpc-web`, expose grpc-status.
3. Terminate TLS at gateway (Envoy, YARP) with grpc-web translation if not on Kestrel directly.
4. Confirm client uses `@grpc/grpc-web` against grpc-web enabled URL, not raw `GrpcChannel` ( .NET client ) from browser.

**Production takeaway:** **grpc-web browser** path requires explicit server (or proxy) support — `AddGrpc()` alone serves native gRPC clients only.

---

---

#### Q5. (R) Review this exception handling in a gRPC interceptor and the matching REST global handler. Support tickets show clients receive `StatusCode.Unknown` with message `"Object reference not set to an instance of an object."`

```csharp
public class ExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(ex.Message));
        }
    }
}
```

`Status` constructor used without explicit `StatusCode`; validation failures throw `ArgumentException` with user-supplied text.

---

**Answer:**

**Answer:** Wrapping all exceptions in `RpcException` with **`Status` defaulting to Unknown** and **raw `ex.Message`** leaks implementation details and loses HTTP-equivalent semantics clients need for retries. Validation errors must map to `InvalidArgument`; missing resources to `NotFound` — never expose NullReference text.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Error mapping | `new Status(ex.Message)` without code | Clients get Unknown — no retry/idempotency hints |
| Security | Internal exception messages in response | Information disclosure |
| Validation | `ArgumentException` not mapped | Same as 500 for client input errors |

**Fix (priority order):**

1. Map known exceptions: `ArgumentException` → `InvalidArgument`, `KeyNotFoundException` → `NotFound`, auth → `PermissionDenied`.
2. Log full exception server-side; return generic message in `RpcException` for unhandled faults.
3. Use `throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));` explicitly in service (as in Q3 snippet for not found).
4. BFF translating to REST: map gRPC codes to ProblemDetails `type`/`title`/`status` — hide raw SQL/null messages.

**Production takeaway:** **Error mapping** for gRPC is **`StatusCode` + safe message** — parallel to Problem Details for REST, not `throw ex` string passthrough.

---

---

#### Q6. (P) Map common failure scenarios to **gRPC status codes** vs **HTTP Problem Details** for a BFF that exposes REST externally and calls gRPC internally: not found, invalid argument, deadline exceeded, permission denied, upstream unavailable. What must the BFF translate, and what should never leak to the browser?

---

**Answer:**

**Answer:** The BFF is the **trust boundary** — translate gRPC `StatusCode` to HTTP status and ProblemDetails; never forward internal messages or stack traces.

| Scenario | gRPC status | REST (BFF) | Browser body |
|---|---|---|---|
| Not found | `NotFound` | 404 | ProblemDetails — generic "resource not found" |
| Bad input | `InvalidArgument` | 400 | Validation problem extensions with field errors |
| Timeout | `DeadlineExceeded` | 504 Gateway Timeout | "Request timed out" — retry-safe hint |
| Authz | `PermissionDenied` | 403 | No internal policy names |
| Upstream down | `Unavailable` | 503 | Retry-After if known |

- **Translate:** code, safe title, correlation id — log gRPC trailing metadata server-side.
- **Never leak:** NullReference messages, SQL errors, internal service names, raw `RpcException` detail from downstream microservices.
- **Idempotency:** map `AlreadyExists` → 409; `FailedPrecondition` → 412 or 409 per API guide.

**Production takeaway:** gRPC status codes are for **machine clients**; browser-facing REST needs **ProblemDetails** with sanitized narrative.

---

---

#### Q7. (R) Review this **server streaming** RPC. Memory grows unbounded during bulk exports; the client disconnects but the service keeps reading SQL for ten minutes.

```csharp
public override async Task StreamReports(
    ReportRequest request,
    IServerStreamWriter<ReportChunk> responseStream,
    ServerCallContext context)
{
    await foreach (var row in _db.ReportRows
        .Where(r => r.TenantId == request.TenantId)
        .AsAsyncEnumerable())
    {
        var chunk = Map(row);
        await responseStream.WriteAsync(chunk);
    }
}
```

No `context.CancellationToken` passed to EF; no batching; chunks include full row payloads (~50 KB each).

---

**Answer:**

**Answer:** Server streaming still requires **cooperative cancellation** and **bounded work** — passing no cancellation token to EF means the service ignores client disconnect; `await foreach` without batching can buffer huge result sets if the producer outpaces the consumer.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cancellation | No `context.CancellationToken` on EF async enumerable | Server continues after client abort |
| Memory | Unbounded stream of 50 KB chunks | LOH pressure; OOM on large tenants |
| Streaming pitfall | One row → one message without flow control | Backpressure ignored — internal queue grows |

**Fix (priority order):**

1. Pass `context.CancellationToken` to every `WriteAsync` and EF operation — exit loop on cancellation.
2. Batch rows into smaller chunks; paginate SQL with keyset, not one giant `AsAsyncEnumerable` on unfiltered table.
3. Configure channel flow control / max outbound buffer; consider `IServerStreamWriter` write timeout.
4. Map cancellation to graceful stream end, not silent hang.

**Production takeaway:** **Streaming RPC pitfalls** — cancellation and backpressure are not automatic; client disconnect must stop server work.

---

---

#### Q8. (M) Explain **deadline propagation** from a REST gateway through a gRPC client to a downstream gRPC service. What breaks if the gateway sets a 30-second HTTP timeout but the gRPC client uses `CallOptions` without `deadline`, and how do you wire `CancellationToken` from ASP.NET Core into `GrpcChannel` calls?

---

**Answer:**

**Answer:** Deadlines are a **budget** subtracted across hop boundaries. If the REST gateway accepts `HttpContext.RequestAborted` (30s client timeout) but the gRPC outbound call has **no deadline**, the downstream service may run minutes while the gateway already returned 504 — orphan work, inconsistent state, and resource leaks.

- Gateway receives HTTP request with 30s client timeout → `HttpContext.RequestAborted` fires at cancel.
- Create gRPC call: `var deadline = DateTime.UtcNow.AddSeconds(RemainingBudget()); var callOptions = new CallOptions(deadline: deadline, cancellationToken: httpContext.RequestAborted);`
- Pass same linked token into EF in downstream service via `ServerCallContext.CancellationToken`.
- **Breaks without deadline:** downstream runs unbounded; gateway times out first; user sees failure but order may still commit.
- **GrpcChannel:** use `channel.CreateCallInvoker().AsyncUnaryCall(..., callOptions)` or extension methods accepting `CallOptions`; in ASP.NET Core minimal/controller, link `RequestAborted` with `CancellationTokenSource.CreateLinkedTokenSource`.

```csharp
var cts = CancellationTokenSource.CreateLinkedTokenSource(
    httpContext.RequestAborted,
    new CancellationTokenSource(TimeSpan.FromSeconds(25)).Token);

await client.GetOrderAsync(request,
    new CallOptions(cancellationToken: cts.Token,
                    deadline: DateTime.UtcNow.AddSeconds(25)));
```

**Production takeaway:** **Deadline propagation** is end-to-end — each hop subtracts overhead; always bind outbound gRPC to the incoming HTTP cancellation and a computed deadline budget.

---

---
