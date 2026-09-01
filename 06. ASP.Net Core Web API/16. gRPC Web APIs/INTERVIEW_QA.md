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

**Concepts**
- HTTP/2 multiplexing and binary framing
- Protocol Buffers for contract-first, strongly typed serialization
- .proto-generated client stubs and server base classes
- Grpc.AspNetCore with Kestrel as the .NET server

**Answer**

gRPC is a high-performance RPC framework using HTTP/2 and Protocol Buffers for contract-first, strongly typed service-to-service communication. Clients and servers generate stubs from `.proto` files, calling methods like local functions with binary serialization instead of JSON. It is built on HTTP/2 multiplexing, header compression, and bidirectional streaming support. The reason I choose gRPC for internal microservice communication is that it is fast, the compiler catches contract mismatches, and `.proto` contracts version independently of REST URL paths. The primary .NET support is via `Grpc.AspNetCore` with Kestrel as the server.

---

## Q2. What is the difference between gRPC and REST?

**Concepts**
- REST resource-oriented URLs vs gRPC RPC methods on a service
- Protobuf binary vs JSON text serialization
- HTTP caching and CDN advantage for REST
- gRPC streaming — server, client, and bidirectional

**Answer**

REST models resources with HTTP verbs, JSON payloads, and standard status codes on many URLs; gRPC models RPC methods on a service with Protobuf messages over HTTP/2 on typically one base path per service. A REST `GET /api/orders/1` returns JSON that clients infer shape from documentation or OpenAPI; a gRPC `GetOrder(OrderRequest)` returns a typed `OrderReply` that is compiler-checked on both sides. REST leverages HTTP caching and CDN naturally; gRPC requires grpc-web and different caching strategies for browser scenarios. gRPC supports streaming — server, client, and bidirectional — which REST traditionally handles separately via chunked HTTP, SSE, or WebSockets.

---

## Q3. What are Protocol Buffers?

**Concepts**
- Language-neutral binary serialization format
- Field numbers as wire identifiers — not field names
- proto3 as the current syntax for new services
- Smaller and faster than JSON for structured data

**Answer**

Protocol Buffers (protobuf) are Google's language-neutral serialization format defined in `.proto` files. Messages declare typed fields with numbered tags, and the compiler generates C# classes and serialization code that produces compact binary payloads on the wire. Field numbers identify wire data — names are not sent on the wire, which is why renaming a field is safe but changing a field's number or type is a breaking change. Protobuf is smaller and faster to serialize/deserialize than JSON for structured data, which is why gRPC's binary protocol significantly outperforms JSON HTTP APIs for high-throughput internal calls. `proto3` is the current syntax for new gRPC services in .NET.

---

## Q4. What is a `.proto` file?

**Concepts**
- .proto as language-neutral contract defining services and messages
- Grpc.Tools generating C# server base classes and client stubs
- Field number as permanent wire identifier
- Shared .proto enabling polyglot client generation

**Answer**

A `.proto` file defines gRPC services, RPC methods, request/response messages, and enums in a language-neutral contract. The .NET build integrates `Grpc.Tools` to generate C# server base classes and client stubs from the proto at compile time, so the generated code is always in sync with the contract. Messages specify fields as `type name = number;` — the numbers are permanent wire identifiers, not the field names. Shared `.proto` files enable polyglot clients — a Java mobile app and a .NET backend can both generate stubs from one contract. I include the proto in the project with `<Protobuf Include="Protos\order.proto" GrpcServices="Server" />` in the `.csproj`.

---

## Q5. What is gRPC-Web?

**Concepts**
- gRPC-Web as browser-compatible protocol variant
- AddGrpc().EnableGrpcWeb() and UseGrpcWeb() middleware
- CORS configuration requirement for cross-origin SPA clients
- Envoy or nginx translation as alternative deployment

**Answer**

gRPC-Web is a protocol variant that lets browser clients call gRPC services because browsers cannot use native gRPC's HTTP/2 trailing headers directly. ASP.NET Core enables it with `AddGrpc()` followed by `UseGrpcWeb()` middleware and `EnableGrpcWeb()` on mapped services. It often needs explicit CORS configuration for cross-origin browser apps. The alternative deployment pattern is to run an Envoy or nginx proxy that translates grpc-web to native gRPC server-side, which keeps the ASP.NET Core service unmodified. Unary calls are most common with gRPC-Web; browser-side streaming support is more limited than in server-to-server native gRPC.

---

## Q6. Why can't browsers use native gRPC directly?

**Concepts**
- HTTP/2 trailing headers inaccessible via browser fetch/XHR
- Corporate proxies and HTTP/1.1 paths breaking gRPC
- grpc-web wrapping for browser-sendable format
- Mobile and server .NET clients using full HTTP/2

**Answer**

Native gRPC relies on HTTP/2 features — trailing headers for status, binary framing, and full duplex streaming — that browser `fetch` and `XMLHttpRequest` APIs do not expose completely. gRPC status and metadata arrive in HTTP/2 trailers that are inaccessible to standard browser HTTP APIs. Corporate proxies and HTTP/1.1-only network paths also break native gRPC from client-side JavaScript. The grpc-web protocol works around this by wrapping calls in forms browsers can send, with a gateway converting to native gRPC server-side. Mobile and server .NET clients use `Grpc.Net.Client` with full HTTP/2 support, so they do not need the grpc-web translation layer.

---

## Q7. What is a unary gRPC call?

**Concepts**
- One request message and one response message
- Analogous to REST request/response round trip
- Single HTTP/2 stream opened and closed per call
- Deadline and cancellation token scope for the entire call

**Answer**

A unary call is the simplest gRPC pattern — one client request message and one server response message, analogous to a REST request/response. Most CRUD-style RPC methods are unary: `GetOrder`, `CreateOrder`, `CancelOrder`. The client awaits `var reply = await client.GetOrderAsync(request)` and the call maps naturally to single database lookups or command operations. It uses one HTTP/2 stream opened and closed for the call, and deadlines and cancellation tokens apply to the entire round trip. Unary is the right starting point; move to streaming only when the use case requires it.

---

## Q8. What is server streaming in gRPC?

**Concepts**
- One client request triggering a stream of server response messages
- AsyncServerStreamingCall<T> for client-side reading
- proto definition with `stream` keyword on return type
- Cancellation token for mid-stream abort

**Answer**

Server streaming RPCs send one client request and a stream of multiple server response messages — useful for large result sets, live updates, or file chunks without loading everything into memory. I define it in `.proto` as `rpc ListOrders(OrderFilter) returns (stream OrderReply)`. The server calls `await responseStream.WriteAsync(order)` repeatedly until complete, and the client reads messages asynchronously from `AsyncServerStreamingCall`. This is more efficient than paginated REST when the client consumes data incrementally, since data flows as it becomes available rather than waiting for a full page. The client must handle backpressure and cancellation mid-stream via `CancellationToken` — ignoring cancellation wastes server resources when the client disconnects.

---

## Q9. What is `RpcException`?

**Concepts**
- RpcException carrying StatusCode and detail message
- Throw on server, catch on client for structured error handling
- InvalidArgument vs Internal distinction for client errors
- No stack trace or internal details in production status messages

**Answer**

`RpcException` is the gRPC-specific exception type carrying a `StatusCode` and detail message. Servers throw it to signal client errors; clients catch it to distinguish `NotFound`, `InvalidArgument`, and `DeadlineExceeded` from transport failures. I throw it as `throw new RpcException(new Status(StatusCode.NotFound, "Order not found"))` on the server and inspect `ex.StatusCode` and `ex.Trailers` on the client. The key practice is mapping business validation failures to `InvalidArgument` rather than the generic `Internal` code, so clients can take appropriate action. I never leak stack traces or internal implementation details in production status messages — the message is part of the public API contract.

---

## Q10. What are gRPC status codes?

**Concepts**
- Standardized status codes carried in HTTP/2 trailers
- OK, Cancelled, InvalidArgument, NotFound, AlreadyExists, etc.
- Client retry behavior on Unavailable and DeadlineExceeded
- google.rpc.Status extensions for structured error detail

**Answer**

gRPC status codes are standardized result indicators parallel to HTTP status codes but carried in HTTP/2 trailers — `OK`, `Cancelled`, `InvalidArgument`, `NotFound`, `AlreadyExists`, `PermissionDenied`, `Unauthenticated`, `ResourceExhausted`, `DeadlineExceeded`, `Unavailable`, and others. `OK` (0) means success; non-zero codes indicate failure at the RPC layer. I map domain errors consistently — duplicate create maps to `AlreadyExists`, auth failure maps to `Unauthenticated` vs `PermissionDenied` depending on whether the credential was absent or insufficient. Clients retry idempotent calls on `Unavailable` and `DeadlineExceeded` with exponential backoff, so the code I return directly influences retry behavior.

---

## Q11. What is a deadline in gRPC?

**Concepts**
- Deadline as absolute time budget propagated to server
- CallOptions(deadline) on client, ServerCallContext.Deadline on server
- DeadlineExceeded status when budget expires
- Chaining deadlines across microservice hops

**Answer**

A deadline specifies the absolute time by which an RPC must complete — propagated from client to server so work stops when the budget expires. The client sets `CallOptions(deadline: DateTime.UtcNow.AddSeconds(5))` and the server observes `ServerCallContext.Deadline`. When exceeded, the call ends with status `DeadlineExceeded`. The critical practice is passing `context.CancellationToken` to EF and downstream calls so the server actually honors the deadline rather than continuing to work after the client has given up. I chain deadlines through microservice calls — each hop should use the remaining budget rather than starting a fresh full timeout, since otherwise total call time can far exceed what the original caller expected.

---

## Q12. How does cancellation work in gRPC?

**Concepts**
- CancellationToken propagating from client disconnect or deadline
- ServerCallContext.CancellationToken passed to EF and downstream calls
- Ignoring cancellation wasting SQL and thread pool resources
- Cooperative cancellation to StatusCode.Cancelled mapping

**Answer**

gRPC cancellation propagates a `CancellationToken` from client to server over HTTP/2 when the client cancels or the deadline passes. Server methods must pass `ServerCallContext.CancellationToken` to long-running EF queries, HTTP calls, and loops so work stops promptly when the client disconnects. The reason this matters is that ignoring cancellation wastes SQL connection pool slots and thread pool threads after the client has already received a timeout — under load this causes cascading resource exhaustion. I pass the token to `FirstOrDefaultAsync(..., context.CancellationToken)`, to outbound `HttpClient` calls, and to any `await foreach` loop that processes streamed data.

---

## Q13. What is backward compatibility in Protocol Buffers?

**Concepts**
- Field number permanence — never change wire type of existing number
- Renaming fields is safe, changing type or reusing number is not
- reserved keyword for removed field numbers
- Mobile client lag requiring multi-version contract support

**Answer**

Protobuf wire compatibility requires never changing the wire type or number of existing fields and never reusing field numbers for different semantics. New fields are added with new numbers; old clients ignore unknown fields by design; new clients use default values for missing old fields. Renaming a field in `.proto` is safe because only tag numbers matter on the wire. Changing field 2 from `double` to `string` breaks old clients — I add a new field number instead. I mark deprecated fields with `deprecated = true` and reserve removed numbers with `reserved` so they cannot be accidentally reused. Mobile apps lag server deploys by weeks, which is why I treat `.proto` changes as multi-version contracts rather than assuming all clients upgrade simultaneously.

---

## Q14. What is `ServerCallContext`?

**Concepts**
- ServerCallContext as the gRPC equivalent of HttpContext
- Request metadata (headers) and response trailers access
- Deadline and CancellationToken on every service method
- context.User populated after authentication middleware

**Answer**

`ServerCallContext` is passed to every gRPC service method, providing request metadata (headers), response trailers, peer identity, deadline, and `CancellationToken`. It is the gRPC equivalent of `HttpContext` for RPC handlers. I read client metadata with `context.RequestHeaders.GetValue("correlation-id")`, write trailers with `context.ResponseTrailers.Add("processed-by", "orders-service")`, and access `context.User` after authentication middleware maps credentials. The most important field for production reliability is `context.CancellationToken` — I pass it to every async I/O call in the service method so the work stops cleanly when the deadline expires or the client cancels.

---

## Q15. What is the difference between gRPC and JSON HTTP APIs?

**Concepts**
- JSON HTTP — human-readable, universal browser and partner support
- gRPC — binary Protobuf, strongly typed, built-in streaming
- OpenAPI documentation advantage for JSON HTTP APIs
- REST at the edge, gRPC internally pattern

**Answer**

JSON HTTP APIs serialize text payloads negotiated via `Content-Type`, discovered through OpenAPI, and consumed universally including browsers. gRPC uses binary Protobuf over HTTP/2 with generated stubs — faster and stricter but less visible without specialized tools. JSON APIs suit public partners and SPAs where human readability, curl debugging, and Swagger documentation are valuable; gRPC suits service-to-service communication on shared `.proto` contracts where performance and type safety are the priority. ASP.NET Core 8 hosts both in one application — the common pattern is to expose REST at the edge for external consumers and gRPC internally between services.

---

## Q16. What is `AddGrpc` used for?

**Concepts**
- AddGrpc() registering server services and Kestrel configuration
- Interceptors for logging, auth, and exception translation
- EnableGrpcWeb() for browser client support
- Shared DI container with AddControllers()

**Answer**

`AddGrpc()` registers gRPC server services, interceptors, and Kestrel configuration needed to host gRPC endpoints in ASP.NET Core. I call it in `Program.cs` as `builder.Services.AddGrpc()` and chain `.AddServiceOptions<T>()` for interceptors, compression, and message size limits. I add `.EnableGrpcWeb()` when browser clients use grpc-web, and I register interceptors globally for cross-cutting concerns like logging, auth enforcement, and exception-to-`RpcException` translation. `AddGrpc()` works alongside `AddControllers()` — REST and gRPC share the same DI container and auth configuration, which means the same JWT Bearer or API key setup applies to both.

---

## Q17. What is `MapGrpcService`?

**Concepts**
- MapGrpcService<TService>() routing RPCs to the service implementation
- Generated base class from .proto defining method overrides
- EnableGrpcWeb() and RequireCors() on the endpoint
- RequireHost and TLS policy on individual services

**Answer**

`MapGrpcService<TService>()` maps a concrete gRPC service implementation to its RPC methods on the Kestrel endpoint routing table. I call it as `app.MapGrpcService<OrderServiceImpl>()` after `app.Build()`. Each service class inherits from the generated `OrderService.OrderServiceBase`, so the routing table knows which methods to dispatch. For grpc-web I chain `.EnableGrpcWeb().RequireCors("spa")` directly on the mapped service endpoint. I can also chain `RequireHost`, TLS policies, and authorization policies on individual service endpoints, which gives me fine-grained control over which services are exposed on which ports and with which security requirements.

---

## Q18. When should you choose gRPC over REST?

**Concepts**
- gRPC for internal microservice communication with shared contracts
- REST for public APIs, browser clients, and partner integrations
- HTTP caching and CDN advantage for REST at the edge
- gRPC-Web and proxy operational cost at browser boundary

**Answer**

I choose gRPC for internal microservice communication where both ends share generated contracts, need low latency, high throughput, streaming, or strict typing — especially .NET-to-.NET or polyglot backends behind a service mesh. I choose REST for public APIs, browser clients, partner integrations, and scenarios requiring HTTP caching and human-readable debugging. For a fintech order processing system, gRPC between inventory, payment, and fulfillment services on HTTP/2 with deadlines makes sense; REST is the right surface for the mobile app and third-party partner APIs documented with OpenAPI. The hybrid pattern — gRPC inside the cluster, REST or BFF at the API gateway for external consumers — is the most common at scale, since it uses each protocol where it has a clear advantage.

---

## Gotchas — ASP.NET Core Web API (Interview Traps)

---

#### Gotcha 1. POST returning 200 instead of 201

**Concepts**
- HTTP 201 Created with Location header for resource creation
- CreatedAtAction and CreatedAtRoute response helpers
- REST client reliance on status codes and Location header

**Answer**

A successful resource creation must return HTTP 201 Created because that status communicates where the new resource lives via the `Location` header — returning 200 omits that contract and breaks REST clients that rely on status codes. I use `CreatedAtAction`, `CreatedAtRoute`, or `Created` to return 201 with a `Location` header pointing at the new resource URL. OpenAPI-generated SDKs and standard HTTP client libraries inspect the status code, so returning 200 hides the resource URL from them silently.

---

#### Gotcha 2. GET that mutates state

**Concepts**
- HTTP safe and idempotent method semantics
- Browser prefetch and CDN cache replay risk
- GET read-only contract enforcement

**Answer**

GET must be safe and idempotent — performing deletes or updates inside a GET handler violates HTTP semantics. Browsers, CDNs, and link-preview crawlers may invoke GET URLs without user intent, so side effects run unintentionally. Cached GET responses can replay destructive operations. State changes belong on POST, PUT, PATCH, or DELETE.

---

#### Gotcha 3. `{ success: false }` with HTTP 200

**Concepts**
- HTTP status codes driving retry logic and APM alerting
- ProblemDetails and ValidationProblemDetails for failures
- Envelope error pattern anti-pattern

**Answer**

Business failures must map to appropriate 4xx or 5xx status codes because that is what drives client retry logic, API gateway routing, and APM alerting. A 200 with `{ success: false }` forces every client to parse the body before knowing whether the call worked. I return `ProblemDetails` with 400 for validation failures and 404, 409, or 422 for domain errors.

---

#### Gotcha 4. Returning EF entities from API actions

**Concepts**
- Navigation property N+1 during serialization
- Circular reference serializer loop risk
- DTO decoupling from database schema

**Answer**

EF Core entities expose navigation properties and circular references that are not designed for public contracts. Lazy-loaded navigations trigger database queries per row during serialization, and circular references cause serializer loops. I always serialize DTOs with explicit shapes so the API contract is decoupled from the database schema.

---

#### Gotcha 5. PascalCase JSON with default camelCase policy

**Concepts**
- System.Text.Json camelCase default in ASP.NET Core 8
- Silent binding failure on case mismatch
- JsonPropertyName attribute and PropertyNamingPolicy override

**Answer**

ASP.NET Core 8 defaults to camelCase JSON via `System.Text.Json`, so PascalCase property names from some clients bind as missing, causing silent data loss on POST and PUT. I align expectations using `[JsonPropertyName("PropertyName")]` or a custom `PropertyNamingPolicy`. The failure is insidious because the server returns 201 or 204 with no error while the data is silently incomplete.

---

#### Gotcha 6. GET with `[FromBody]`

**Concepts**
- GET body stripping by proxies and HTTP clients
- [FromQuery] for simple filters
- OpenAPI and browser fetch GET body restrictions

**Answer**

Many HTTP clients, proxies, and caches ignore or strip GET request bodies, so filters sent as JSON in a GET request fail silently. I use query strings with `[FromQuery]` for simple filters, or POST to a dedicated search endpoint for complex filter objects. OpenAPI tools and browser `fetch` also discourage GET bodies, making the pattern fragile in production.

---

#### Gotcha 7. CORS as server security

**Concepts**
- CORS as browser-only enforcement mechanism
- curl and server-to-server bypass of CORS
- Authentication and authorization as real API security

**Answer**

CORS is enforced only by browsers — it does not stop curl, Postman, or server-to-server calls. CORS headers tell a browser whether JavaScript on one origin may read a cross-origin response; they authenticate nothing. I register `AddCors` and `UseCors` for browser SPA access, and enforce JWT, cookies, or API keys separately as the actual security mechanism.

---

#### Gotcha 8. `AllowAnyOrigin` with credentials

**Concepts**
- Access-Control-Allow-Origin wildcard and credentials incompatibility
- WithOrigins explicit list requirement for credentialed requests
- AllowCredentials requirement for cookies and Authorization headers

**Answer**

Browsers reject `Access-Control-Allow-Origin: *` when the request sends cookies or authorization headers, so `AllowAnyOrigin()` and `AllowCredentials()` cannot be combined. I specify every trusted frontend origin explicitly with `WithOrigins` and pair that with `AllowCredentials()`. Credentialed cross-origin calls require both a matching explicit origin and `Access-Control-Allow-Credentials: true`.

---

#### Gotcha 9. Swagger UI exposed in Production

**Concepts**
- OpenAPI schema reconnaissance risk
- Environment-gated Swagger UI registration
- Production API surface disclosure

**Answer**

Public Swagger UI discloses the full API surface to anyone who finds the endpoint. I wrap `MapSwagger` and `UseSwaggerUI` with an environment check so they only serve in Development or Staging, and gate production access behind authentication middleware when internal tooling requires it.

---

#### Gotcha 10. Missing `[ApiController]` on some controllers

**Concepts**
- [ApiController] enabling automatic model-state 400 responses
- [FromBody] inference for complex types
- Inconsistent error contracts from mixed controller conventions

**Answer**

Without `[ApiController]`, automatic 400 `ValidationProblemDetails` responses and binding source inference differ from controllers that do have it. A mix of attributed and non-attributed controllers produces inconsistent error contracts. I apply `[ApiController]` at the controller or assembly level so every endpoint shares the same API conventions.

---

#### Gotcha 11. Blocking on `.Result` in async actions

**Concepts**
- Sync-over-async thread-pool starvation
- SynchronizationContext deadlock under ASP.NET Core
- async Task<IActionResult> propagation through service layer

**Answer**

Blocking on `.Result` or `.Wait()` ties up Kestrel request threads while I/O completes, reducing throughput under concurrent load. Deadlocks occur when the blocked thread holds a synchronization context the async continuation needs. I mark controller actions `async Task<IActionResult>` and propagate `await` through the entire service layer to EF Core and `HttpClient` calls.

---

#### Gotcha 12. Liveness probe includes SQL check

**Concepts**
- Liveness vs readiness probe semantics in Kubernetes
- Unnecessary pod restart from database-down liveness failure
- /health/live lightweight self-check vs /health/ready dependency check

**Answer**

If the liveness probe fails when SQL is down, Kubernetes restarts the pod unnecessarily — restarting cannot fix a database outage. Liveness answers whether the process is healthy; readiness answers whether the pod should receive traffic. I put SQL and external service checks on the readiness probe only, mapping `/health/live` to a lightweight self-check and `/health/ready` to `AddDbContextCheck`.

---

#### Gotcha 13. N+1 queries in list endpoints

**Concepts**
- Lazy-loaded navigation property per-row SQL query
- LINQ projection to DTO in a single query
- Include/ThenInclude for explicit eager loading

**Answer**

Returning entities with lazy-loaded navigation properties triggers one SQL query per row. I fix this by projecting directly to DTOs in LINQ so EF Core generates a single query, or by using `Include`/`ThenInclude` for graphs that must be loaded together. Serialization must never drive database queries.

---

#### Gotcha 14. Unstable pagination with Skip/Take

**Concepts**
- Offset pagination instability under concurrent writes
- Keyset pagination with stable indexed key
- Cursor token exposure in response metadata

**Answer**

`Skip((page - 1) * pageSize).Take(pageSize)` shifts the window when rows are inserted or deleted between page requests, causing duplicates or gaps. Keyset pagination uses `WHERE id > @lastId ORDER BY id LIMIT @pageSize` with the last seen key, which is stable under concurrent writes. I expose cursor tokens in response metadata for high-churn data.

---

#### Gotcha 15. GraphQL N+1 without DataLoader

**Concepts**
- Field resolver per-parent database query explosion
- DataLoader batching into single IN clause
- Eager loading at root query as alternative

**Answer**

Field resolvers in HotChocolate that query the database per parent row explode into N+1 SQL calls — 100 authors resolving `books` individually fires 101 queries. The fix is DataLoader: a batch loader collects author IDs and issues a single `WHERE AuthorId IN (...)` query. When the client always requests nested fields together, eager loading at the root is also valid.

---

#### Gotcha 16. gRPC in browser without gRPC-Web

**Concepts**
- Native gRPC HTTP/2 trailing headers inaccessible to browsers
- gRPC-Web middleware translation requirement
- CORS configuration alongside gRPC-Web

**Answer**

Native gRPC uses HTTP/2 binary framing and trailing headers that browser `fetch` and `XMLHttpRequest` APIs do not expose. Blazor WASM and SPA browsers require the gRPC-Web protocol — I add `AddGrpcWeb()` and call `EnableGrpcWeb()` on mapped services. I also configure CORS for the browser origin, since cross-origin browser calls still enforce CORS on preflight and response headers.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (D) A fintech team must expose an **order status** API to (a) a React SPA in the browser, (b) an internal .NET microservice, and (c) a partner's legacy HTTP JSON client. They propose gRPC for all three. What would you recommend per consumer, and why?

**Concepts**
- Browser grpc-web complexity vs REST simplicity for SPA
- gRPC sweet spot for internal .NET microservice communication
- REST/JSON required for partner legacy HTTP integration
- Use gRPC inside the mesh, REST at the edge pattern

**Answer**

gRPC is not the right choice for all three consumers because each has different capabilities and constraints. For the React SPA, I would use REST or JSON over HTTP — native gRPC is not available in browsers, grpc-web requires proxy infrastructure or ASP.NET Core middleware, adds CORS complexity, and offers no meaningful advantage for a browser client that needs human-debuggable HTTP calls. For the internal .NET microservice, gRPC is the ideal choice — HTTP/2, strongly typed contracts via `.proto`, streaming support, deadlines, and efficient binary serialization are all valuable for service-to-service communication on a shared infrastructure. For the partner's legacy HTTP JSON client, REST with OpenAPI is the only practical option — the partner cannot regenerate stubs from a `.proto` file, and forcing gRPC blocks integration and creates an ongoing support burden. The right architecture is to expose REST at the edge for the SPA and partner, and gRPC internally for the microservice, which is exactly the pattern gRPC's designers intended.

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

**Concepts**
- Field number wire type permanence — type change on same number is breaking
- Renaming is safe, type change is not
- reserved keyword preventing number reuse after migration
- Mobile client lag requiring multi-version contract support

**Answer**

This is a breaking wire-format change disguised as a rename. Protobuf wire compatibility requires never changing the wire type of an existing field number — field 2 was `double amount` in v1, and v2 reassigns field 2 to `string currency_code`. Old mobile clients still send field 2 as a `double`, but the v2 server now expects field 2 to be a `string`, causing deserialization failures. The field rename is irrelevant because names are not transmitted on the wire — only tag numbers are. Blue/green deployment does not help here because the client and server are exchanging incompatible binary formats, not incompatible HTTP paths. The correct fix is to add `string currency_code = 4` as a completely new field with a new number, keep `double amount = 2` (marked `deprecated = true`) for old client compatibility, and never reuse field 2 for a different type. Once all mobile clients have migrated, I use `reserved 2;` to prevent accidental reuse of that number. Any `.proto` change that modifies a field's wire type requires a new message type or new field number — deployment strategy cannot save it.

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

**Concepts**
- ServerCallContext.CancellationToken passed to EF queries
- Deadline/cancellation propagation from gRPC to data layer
- Connection pool exhaustion from ignored cancellation under load
- OperationCanceledException mapped to DeadlineExceeded

**Answer**

The client sets a 5-second deadline, which propagates through gRPC as a cancellation signal, but `FirstOrDefaultAsync` uses `CancellationToken.None` so EF ignores the cancellation entirely. When SQL is slow the query runs to completion regardless of the deadline, which means the client receives `DeadlineExceeded` after 5 seconds but the server continues consuming a database connection for the full query duration. Under load this causes connection pool exhaustion — many completed-from-the-client's-perspective requests still holding SQL connections on the server. The fix is a single change: `FirstOrDefaultAsync(o => o.Id == request.OrderId, context.CancellationToken)`. I also add `OperationCanceledException` handling in the method body to map it to `StatusCode.DeadlineExceeded` when `context.CancellationToken.IsCancellationRequested`, and I configure a SQL command timeout aligned with the maximum deadline budget as a backstop for cases where the cancellation token path is not sufficient.

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

**Concepts**
- UseGrpcWeb() middleware required before MapGrpcService
- EnableGrpcWeb() on the endpoint accepting grpc-web content type
- CORS policy including grpc-web headers and exposed grpc-status
- AddGrpc() alone serving native gRPC only

**Answer**

`AddGrpc()` registers native gRPC support — it handles `application/grpc` but not `application/grpc-web` or `application/grpc-web-text`. The 415 Unsupported Media Type is the server rejecting the grpc-web content type because the grpc-web translation middleware is missing. The preflight succeeds only because CORS is not yet configured to reject it, but the actual request fails at content-type negotiation. I would add `app.UseGrpcWeb()` before `app.MapGrpcService<GreeterService>()` and chain `.EnableGrpcWeb()` on the mapped service. I would also add a CORS policy that allows the SPA origin, the `POST` method, and the gRPC-relevant headers (`grpc-timeout`, `x-grpc-web`, `content-type`) and exposes `grpc-status` and `grpc-message` in the response. For production I would also evaluate running Envoy or YARP in front of the service as the grpc-web translation layer, which keeps the ASP.NET Core service serving native gRPC and delegates browser compatibility to the proxy.

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

**Concepts**
- Status constructor defaulting to Unknown without explicit StatusCode
- Exception message leaking internal implementation details
- ArgumentException → InvalidArgument, KeyNotFoundException → NotFound mapping
- BFF translating gRPC StatusCode to ProblemDetails for REST consumers

**Answer**

`new Status(ex.Message)` uses the `Status(string detail)` overload which defaults `StatusCode` to `Unknown` — so every error the service throws returns `Unknown` to the client, losing the semantic meaning that would tell clients whether to retry or not. The `ex.Message` for a `NullReferenceException` is the raw .NET exception message, which is an information disclosure vulnerability in production since it reveals implementation details. I would fix this by mapping known exception types to appropriate status codes: `ArgumentException` and `ValidationException` map to `InvalidArgument`, `KeyNotFoundException` maps to `NotFound`, authorization failures map to `PermissionDenied`. For all other exceptions I log the full exception server-side and return a generic message in `RpcException` — `throw new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred"))`. For the BFF layer translating gRPC to REST, I map each `StatusCode` to a `ProblemDetails` response: `NotFound` → 404, `InvalidArgument` → 400, `PermissionDenied` → 403, never forwarding raw `RpcException` messages to browser clients.

---

#### Q6. (P) Map common failure scenarios to **gRPC status codes** vs **HTTP Problem Details** for a BFF that exposes REST externally and calls gRPC internally: not found, invalid argument, deadline exceeded, permission denied, upstream unavailable. What must the BFF translate, and what should never leak to the browser?

**Concepts**
- BFF as trust boundary between gRPC internal and REST external
- StatusCode to HTTP status mapping
- ProblemDetails sanitized message vs raw RpcException detail
- Correlation ID logging for internal detail without browser exposure

**Answer**

The BFF is the trust boundary — it receives typed `RpcException` from downstream services and must translate both the status code and the message before sending anything to the browser. The mapping I use:

| Scenario | gRPC status | BFF HTTP status | Browser body |
|---|---|---|---|
| Not found | `NotFound` | 404 | ProblemDetails — "Resource not found" |
| Bad input | `InvalidArgument` | 400 | ValidationProblemDetails with field errors |
| Timeout | `DeadlineExceeded` | 504 Gateway Timeout | "Request timed out" |
| Authz | `PermissionDenied` | 403 | No internal policy names |
| Upstream down | `Unavailable` | 503 | Retry-After if known |

The BFF must translate the status code to HTTP semantics and sanitize the message — replace `ex.Status.Detail` with a safe, user-facing description and log the original detail plus correlation ID server-side for debugging. What must never leak to the browser: raw exception messages, SQL error details, internal service names, downstream stack traces, or `RpcException` detail strings from microservices that include implementation details. For `AlreadyExists` I return 409; for `FailedPrecondition` I return 412 or 409 depending on API semantics. The correlation ID travels as a response header so support engineers can trace the full call chain without the browser seeing internal topology.

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

**Concepts**
- CancellationToken required on EF async enumerable for client disconnect
- Unbounded stream memory pressure from 50 KB chunks
- Keyset-paginated SQL batching for large exports
- IServerStreamWriter backpressure and write timeout

**Answer**

There are three problems here. First, the `await foreach` passes no cancellation token to EF, so when the client disconnects or the deadline expires the server continues reading rows from SQL, holding the database connection and consuming memory for the full duration of the export — potentially ten minutes. Second, streaming 50 KB chunks without any batching or pagination means the SQL cursor stays open for the entire export, holding a connection pool slot. Third, if the server produces chunks faster than the network can deliver them to the client, the internal write queue grows unbounded. I would fix this by passing `context.CancellationToken` to both the `WriteAsync` call and the EF async enumerable: `.AsAsyncEnumerable(context.CancellationToken)`. I would replace the unbounded query with keyset-paginated SQL batches — load N rows at a time using `WHERE Id > @lastId`, write them, then advance the cursor — so no single SQL query holds a long-lived connection. I would also check `context.CancellationToken.IsCancellationRequested` in the loop and exit gracefully rather than continuing to write to a closed stream.

---

#### Q8. (M) Explain **deadline propagation** from a REST gateway through a gRPC client to a downstream gRPC service. What breaks if the gateway sets a 30-second HTTP timeout but the gRPC client uses `CallOptions` without `deadline`, and how do you wire `CancellationToken` from ASP.NET Core into `GrpcChannel` calls?

**Concepts**
- Deadline as budget subtracted across hop boundaries
- HttpContext.RequestAborted as the HTTP-layer cancellation signal
- CallOptions(deadline, cancellationToken) wiring gRPC to HTTP cancellation
- Orphan work and inconsistent state from missing deadline propagation

**Answer**

Deadlines are a budget meant to be subtracted at each hop. If the REST gateway accepts a 30-second HTTP client timeout — surfaced in ASP.NET Core as `HttpContext.RequestAborted` — but the outbound gRPC call has no `deadline` in `CallOptions`, the downstream service may run for minutes after the gateway has already returned 504 to the browser. The result is orphan work: the order may commit on the downstream service while the gateway already reported failure, creating an inconsistent state that is hard to reconcile. I wire the deadline explicitly by computing the remaining budget from the gateway's timeout and linking the HTTP cancellation token:

```csharp
var cts = CancellationTokenSource.CreateLinkedTokenSource(
    httpContext.RequestAborted,
    new CancellationTokenSource(TimeSpan.FromSeconds(25)).Token);

await client.GetOrderAsync(request,
    new CallOptions(cancellationToken: cts.Token,
                    deadline: DateTime.UtcNow.AddSeconds(25)));
```

This linked token fires when either the HTTP client disconnects or the 25-second budget expires, whichever comes first. On the downstream gRPC service, `ServerCallContext.CancellationToken` fires when the client cancels, and I pass it to EF and any further downstream calls so the cancellation propagates all the way to the database. Each microservice hop subtracts a small coordination overhead from the remaining budget rather than starting a fresh full timeout.
