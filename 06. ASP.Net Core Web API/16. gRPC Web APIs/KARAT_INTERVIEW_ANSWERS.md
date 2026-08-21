# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/16. gRPC Web APIs`

---

#### Q1. (D) A fintech team must expose an **order status** API to (a) a React SPA in the browser, (b) an internal .NET microservice, and (c) a partner's legacy HTTP JSON client. They propose gRPC for all three. What would you recommend per consumer, and why?

**Answer:** **gRPC vs REST choice** depends on client capabilities, not server preference alone. Internal .NET service-to-service is gRPC's sweet spot; browsers and legacy JSON partners need different surfaces.

- **(a) React SPA:** **REST or BFF + JSON** (or **gRPC-Web** with Envoy/nginx translation if team accepts binary framing, CORS, and limited browser tooling). Native gRPC from browser is not standard — grpc-web adds complexity. Prefer REST/JSON for public browser APIs unless latency and contract rigor justify grpc-web infrastructure.
- **(b) Internal .NET microservice:** **gRPC** — HTTP/2, strong contracts via `.proto`, streaming, deadlines, efficient serialization. Use mTLS or internal auth between services.
- **(c) Partner legacy JSON:** **REST/JSON** with OpenAPI — partners cannot regenerate stubs from your `.proto`; forcing gRPC blocks integration. Version HTTP API separately.

**Production takeaway:** Use gRPC **inside** the mesh; expose **REST at the edge** for browsers and external HTTP clients unless you operate grpc-web end-to-end deliberately.

---

#### Q2. (R) Review this `.proto` change shipped as a "backward compatible" release. Old mobile clients crash on deserialize; server logs `InvalidProtocolBufferException`.

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

#### Q3. (R) Review this gRPC service method. Callers report hung requests when downstream SQL is slow; cancellation from the client never stops the query.

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

#### Q4. (R) Review `Program.cs` for a gRPC API consumed from a browser via **gRPC-Web**. Preflight succeeds but unary calls return 415; Chrome shows `application/grpc-web-text` rejected.

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

#### Q5. (R) Review this exception handling in a gRPC interceptor and the matching REST global handler. Support tickets show clients receive `StatusCode.Unknown` with message `"Object reference not set to an instance of an object."`

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

#### Q6. (P) Map common failure scenarios to **gRPC status codes** vs **HTTP Problem Details** for a BFF that exposes REST externally and calls gRPC internally: not found, invalid argument, deadline exceeded, permission denied, upstream unavailable. What must the BFF translate, and what should never leak to the browser?

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

#### Q7. (R) Review this **server streaming** RPC. Memory grows unbounded during bulk exports; the client disconnects but the service keeps reading SQL for ten minutes.

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

#### Q8. (M) Explain **deadline propagation** from a REST gateway through a gRPC client to a downstream gRPC service. What breaks if the gateway sets a 30-second HTTP timeout but the gRPC client uses `CallOptions` without `deadline`, and how do you wire `CancellationToken` from ASP.NET Core into `GrpcChannel` calls?

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
