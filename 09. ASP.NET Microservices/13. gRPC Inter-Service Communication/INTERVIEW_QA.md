# Interview Questions — gRPC Inter-Service Communication — Interview Q&A
> 25 questions · Back to [README](../README.md)

## Table of Contents
1. [Why is gRPC typically preferred over REST for synchronous service-to-service cal…](#q1)
2. [What is HTTP/2 multiplexing, and why does it matter for gRPC inter-service commu…](#q2)
3. [When should you choose an asynchronous message broker over a synchronous gRPC ca…](#q3)
4. [What is the GrpcClientFactory pattern in ASP.NET Core, and why is it preferred o…](#q4)
5. [How do you register a typed gRPC client with `AddGrpcClient<T>`, configure its b…](#q5)
6. [How do you attach a client-side interceptor to a gRPC client registered via Grpc…](#q6)
7. [How do you configure `HttpClient` and channel options — connection lifetime, kee…](#q7)
8. [How should `.proto` files be shared across multiple microservices — what are the…](#q8)
9. [What is the difference between a backward-compatible and a breaking proto schema…](#q9)
10. [What is the purpose of the `csharp_namespace` and `package` options in a `.proto…](#q10)
11. [What is mutual TLS (mTLS), and why is it the recommended authentication mechanis…](#q11)
12. [How do you propagate a caller's JWT bearer token from an inbound HTTP request in…](#q12)
13. [What is the difference between channel credentials and call credentials in gRPC,…](#q13)
14. [How do you authorize gRPC service methods in ASP.NET Core using `[Authorize]` at…](#q14)
15. [How do you configure retry policies for a gRPC client in .NET using the built-in…](#q15)
16. [What is hedging in gRPC, how does it differ from retry, and what conditions make…](#q16)
17. [How does a client deadline interact with a retry policy in a multi-hop service c…](#q17)
18. [Why does the default Kubernetes `ClusterIP` service not distribute gRPC traffic …](#q18)
19. [What is a headless Kubernetes service, and how does it enable client-side gRPC l…](#q19)
20. [How do you configure a .NET gRPC client to use DNS-based service discovery with …](#q20)
21. [What is the standard gRPC health checking protocol, how do you implement it in a…](#q21)
22. [How does OpenTelemetry instrument gRPC calls in .NET, and what trace attributes …](#q22)
23. [How do you implement a server-side gRPC interceptor to emit structured log entri…](#q23)
24. [What Kestrel and environment configuration changes are required when running a g…](#q24)
25. [What is the `grpc` probe type in Kubernetes 1.24+, and how does it differ from a…](#q25)

---

## Q1. Why is gRPC typically preferred over REST for synchronous service-to-service calls inside a microservices system, and what are the trade-offs of that choice?

Why is gRPC typically preferred over REST for synchronous service-to-service calls inside a microservices system, and what are the trade-offs of that choice?

**Answer:** gRPC (Google Remote Procedure Call) is preferred for inter-service communication because it uses HTTP/2 as its transport and Protocol Buffers (protobuf) for serialization, yielding significantly lower latency and payload size compared to JSON over HTTP/1.1. The contract-first approach using `.proto` files also enforces a strongly typed API between services, catching mismatches at compile time rather than at runtime.

- Protobuf binary encoding is typically 3–10× smaller than equivalent JSON and faster to serialize and deserialize, which matters when Service A calls Service B thousands of times per second.
- HTTP/2 multiplexing allows many concurrent RPC calls to share a single TCP connection, eliminating the per-request connection overhead and head-of-line blocking present in HTTP/1.1.
- The strongly typed generated client and server stubs remove an entire category of integration errors — a field type change that breaks a consumer is caught when the shared `.proto` file is updated and the project fails to compile.
- The trade-offs are real: gRPC requires HTTP/2 end-to-end (problematic with some proxies and load balancers), debugging binary payloads is harder than inspecting JSON, and the tooling ecosystem is smaller than REST's.
- For external-facing APIs or third-party consumers, REST or GraphQL is usually the better choice; reserve gRPC for the internal service mesh where you control both ends.

---

## Q2. What is HTTP/2 multiplexing, and why does it matter for gRPC inter-service communication under high concurrency?

What is HTTP/2 multiplexing, and why does it matter for gRPC inter-service communication under high concurrency?

**Answer:** HTTP/2 multiplexing means multiple concurrent requests and responses can be interleaved over a single TCP connection as independent logical streams, each with its own stream identifier, rather than requiring a separate connection per request. This directly addresses the connection-per-request overhead that limits REST over HTTP/1.1 under high concurrency.

- In HTTP/1.1, a client that wants 100 concurrent requests to a service typically needs 100 TCP connections (or serializes requests per connection), which means 100 three-way handshakes and 100 sets of TLS negotiation.
- With HTTP/2, one TCP connection carries all 100 concurrent streams. The kernel and the network stack handle one set of handshake and congestion-control state, reducing connection setup cost to near zero.
- gRPC channels are long-lived HTTP/2 connections managed by the `GrpcChannel`. When you reuse the channel (which `GrpcClientFactory` handles automatically), you amortize connection setup cost across all RPCs made through that channel.
- Head-of-line blocking at the HTTP layer is eliminated — a slow stream does not delay other streams. (Note: TCP-level head-of-line blocking remains, which HTTP/3 over QUIC addresses, but gRPC does not yet use HTTP/3 in mainstream .NET.)
- For a service making 5,000 gRPC calls per second, the difference between one shared channel and 5,000 per-request channels is the difference between a stable system and resource exhaustion.

---

## Q3. When should you choose an asynchronous message broker over a synchronous gRPC call between two microservices?

When should you choose an asynchronous message broker over a synchronous gRPC call between two microservices?

**Answer:** A message broker (such as RabbitMQ, Azure Service Bus, or Kafka) is the right choice when the calling service does not need an immediate result, when the receiving service may be temporarily unavailable, or when the operation's scope spans multiple services that must each react independently. Synchronous gRPC couples the caller's availability and latency directly to the downstream service's availability and latency, which becomes a liability for any non-trivial chain of dependencies.

- Choose a message broker when the operation is fire-and-forget — for example, publishing an `OrderPlaced` event that three other services (inventory, billing, notifications) each consume on their own schedule.
- Choose a message broker when you need durable delivery guarantees: if the downstream service restarts mid-processing, the message stays in the queue and is retried, whereas a failed gRPC call requires the caller to implement its own retry logic.
- Choose gRPC when you need an immediate response — for example, checking current product stock before confirming a cart, or validating a payment token that must succeed before the HTTP response is sent to the user.
- High fan-out (one event, many consumers) almost always fits a broker better than gRPC, because broadcasting a call to N services synchronously means N serial or N concurrent calls, all of whose failures you must handle.
- In practice, a microservices system uses both: gRPC for low-latency, request-response calls where the caller waits, and a message broker for events that decouple producers from consumers.

---

## Chapter 2: GrpcClientFactory & Channel Management

---

## Q4. What is the GrpcClientFactory pattern in ASP.NET Core, and why is it preferred over constructing `GrpcChannel` directly inside each service?

What is the GrpcClientFactory pattern in ASP.NET Core, and why is it preferred over constructing `GrpcChannel` directly inside each service?

**Answer:** `GrpcClientFactory` is the integration between the ASP.NET Core `IHttpClientFactory` infrastructure and gRPC client creation, provided by the `Grpc.Net.ClientFactory` NuGet package. It manages the lifecycle of the underlying `HttpClient` and `GrpcChannel` instances so that channels are reused across requests, connection lifetimes are rotated safely, and cross-cutting configuration (interceptors, headers, timeouts) is applied once at registration rather than scattered across call sites.

- Constructing a `GrpcChannel` directly in a service class means every instantiation of that class creates a new channel — and therefore a new HTTP/2 connection — which defeats multiplexing and causes socket exhaustion under load.
- `GrpcClientFactory` pools and reuses the underlying `HttpMessageHandler` (the actual TCP connection), rotating the handler periodically (defaulting to 2 minutes) to respect DNS TTLs without leaving stale connections open indefinitely.
- Configuration is centralized: you register interceptors, credentials, base addresses, and retry policies once in `Program.cs`, and every injected client instance inherits them automatically.
- The factory integrates with ASP.NET Core's built-in dependency injection, so you inject `OrderService.OrderServiceClient` directly into your application class — the factory handles instantiation, channel reuse, and disposal.

---

## Q5. How do you register a typed gRPC client with `AddGrpcClient<T>`, configure its base address, and inject it via dependency injection?

How do you register a typed gRPC client with `AddGrpcClient<T>`, configure its base address, and inject it via dependency injection?

**Answer:** You call `builder.Services.AddGrpcClient<TClient>()` in `Program.cs`, passing a configuration delegate that sets the base address of the target service. The factory then makes `TClient` available for injection anywhere in the application without the caller knowing anything about channels or HTTP/2.

```csharp
builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
{
    o.Address = new Uri("https://order-service:5001");
});
```

- The generic type argument is the generated client class from your `.proto` file — for example `OrderService.OrderServiceClient`.
- The `Address` property sets the base URI of the downstream service. In Kubernetes you would use the DNS name of the service, such as `https://order-svc.orders.svc.cluster.local`.
- Once registered, you inject the client by declaring it as a constructor parameter: `public MyHandler(OrderService.OrderServiceClient client)`. The factory creates the client instance with the shared channel each time the handler is instantiated.
- For services without TLS in development or in a service mesh that handles mTLS at the sidecar level, you can configure the channel to use plain HTTP/2 by calling `AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true)` and using an `http://` address.

---

## Q6. How do you attach a client-side interceptor to a gRPC client registered via GrpcClientFactory, and what cross-cutting concerns does it typically handle?

How do you attach a client-side interceptor to a gRPC client registered via GrpcClientFactory, and what cross-cutting concerns does it typically handle?

**Answer:** You chain `.AddInterceptor<T>()` onto the `AddGrpcClient<TClient>()` registration in `Program.cs`. The interceptor is a class that inherits from `Grpc.Core.Interceptors.Interceptor` and overrides the `BlockingUnaryCall`, `AsyncUnaryCall`, `AsyncServerStreamingCall`, and related methods to inject behavior before and after each RPC.

```csharp
builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
    {
        o.Address = new Uri("https://order-service:5001");
    })
    .AddInterceptor<AuthHeaderInterceptor>()
    .AddInterceptor<LoggingInterceptor>();
```

- A common use is JWT propagation: the interceptor reads the current user's bearer token from `IHttpContextAccessor` and appends it to the outgoing gRPC call as an `Authorization` metadata entry, so the downstream service receives the caller's identity.
- Logging interceptors record the method name, request size, deadline, and response status for every outbound RPC, giving you structured telemetry without modifying individual call sites.
- Deadline-injection interceptors attach a default deadline to any call that does not already have one, preventing unbounded calls against a slow downstream service.
- Interceptors are resolved from the DI container per-request (or per scope, depending on registration), so they can take scoped dependencies like `IHttpContextAccessor`.

---

## Q7. How do you configure `HttpClient` and channel options — connection lifetime, keepalive pings, and HTTP version — for a GrpcClientFactory-managed channel?

How do you configure `HttpClient` and channel options — connection lifetime, keepalive pings, and HTTP version — for a GrpcClientFactory-managed channel?

**Answer:** You chain `.ConfigurePrimaryHttpMessageHandler()` or `.ConfigureChannel()` onto the `AddGrpcClient` registration. `ConfigurePrimaryHttpMessageHandler` controls the underlying `SocketsHttpHandler`, where you set keepalive pings and connection lifetime. `ConfigureChannel` provides access to `GrpcChannelOptions`, where you control message size limits and credentials.

```csharp
builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
    o.Address = new Uri("https://order-service:5001"))
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout     = TimeSpan.FromMinutes(5),
        KeepAlivePingDelay              = TimeSpan.FromSeconds(60),
        KeepAlivePingTimeout            = TimeSpan.FromSeconds(30),
        EnableMultipleHttp2Connections  = true,
    });
```

- `PooledConnectionIdleTimeout` controls how long an idle HTTP/2 connection stays in the pool before being closed and replaced; set it shorter than any upstream load balancer's idle timeout to avoid receiving RST_STREAM frames.
- `KeepAlivePingDelay` tells the client to send a PING frame every N seconds on idle connections, which prevents NAT tables and load balancers from silently dropping the TCP connection; `KeepAlivePingTimeout` is how long to wait for a PONG before treating the connection as dead.
- `EnableMultipleHttp2Connections` allows the factory to open additional HTTP/2 connections to the same endpoint when the current connection is saturated with streams (gRPC's default max is 100 concurrent streams per connection).
- For plain HTTP/2 (no TLS, common in Kubernetes with a service mesh), you must also set `AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true)` before building the app.

---

## Chapter 3: Proto Contract Sharing

---

## Q8. How should `.proto` files be shared across multiple microservices — what are the options and the trade-offs of each approach?

How should `.proto` files be shared across multiple microservices — what are the options and the trade-offs of each approach?

**Answer:** There are three main approaches to sharing `.proto` files across microservices: copying files into each service's repository, sharing via a private NuGet package containing only the `.proto` files and generated stubs, or using a git submodule or subtree to synchronize a shared directory. The right choice depends on team size, change frequency, and whether services are in the same repository.

| Approach | Pros | Cons |
|---|---|---|
| Copy into each repo | Simple, no tooling needed | Drift — copies go out of sync easily |
| Shared NuGet package (proto + generated stubs) | Versioned, explicit upgrade path | Publishing overhead; requires a package feed |
| Git submodule / subtree | Single source of truth, no feed needed | Git submodule complexity; teams must remember to update |
| Shared project in a monorepo | Same build graph, guaranteed consistency | Only works if all services live in one repo |

- The NuGet package approach is the most robust for teams with separate repositories: you publish a package like `MyCompany.Contracts.Orders` containing the `.proto` file and the generated C# stubs, then consumers pin a version and upgrade deliberately.
- In a monorepo, a shared `Directory.Build.props` can reference a common `Protos/` folder, and all services regenerate stubs from the same source on every build.
- Whichever approach you choose, treat `.proto` files with the same discipline as a public API: version them, maintain a changelog, and communicate breaking changes ahead of deployment.

---

## Q9. What is the difference between a backward-compatible and a breaking proto schema change in a live microservices environment, and how do teams manage rolling upgrades safely?

What is the difference between a backward-compatible and a breaking proto schema change in a live microservices environment, and how do teams manage rolling upgrades safely?

**Answer:** A backward-compatible proto change is one that does not alter the wire encoding of existing fields, so old clients can communicate with new servers and vice versa without modification. A breaking change alters wire encoding, removes a required field (in proto2), or reuses a field number with a different type, causing deserialization failures when a old client talks to a new server.

- Backward-compatible changes include: adding a new field with a new field number, renaming a field (field names are not on the wire — only numbers are), marking a field as `reserved`, and adding a new enum value.
- Breaking changes include: deleting a field number and reassigning it to a new type, changing a field's wire type (e.g., from `int32` to `string`), and adding `oneof` around an existing field.
- Rolling upgrades work by deploying the new server first (which still accepts old requests because the new field is simply absent), then upgrading clients. Old clients send messages without the new field; new servers read zero/empty for that field, which is the protobuf default.
- Use the `reserved` keyword when retiring a field to prevent accidental reuse of the field number: `reserved 3; reserved "old_name";`. This turns accidental reuse into a compile error.

---

## Q10. What is the purpose of the `csharp_namespace` and `package` options in a `.proto` file, and what problems arise if they are omitted or mismatched across services?

What is the purpose of the `csharp_namespace` and `package` options in a `.proto` file, and what problems arise if they are omitted or mismatched across services?

**Answer:** The `package` option defines the logical namespace of the proto messages and is used by the protobuf runtime for reflection, symbol resolution, and disambiguation when two proto files define messages with the same name. The `csharp_namespace` option overrides the C# namespace of the generated code so it can follow your project's naming conventions independently of the proto package name.

- Without `package`, two proto files that both define a message named `OrderRequest` will produce ambiguous symbol resolution at the protobuf level, and in larger systems the generated C# code may end up in the global namespace, causing compile-time collisions.
- Without `csharp_namespace`, the generated C# class is placed in a namespace derived from the `package` value, which typically uses lowercase dot-separated names (e.g., `orders.v1`) rather than the PascalCase conventions of C# (e.g., `MyApp.Orders.V1`).
- If two services include the same `.proto` file but with different `csharp_namespace` values (for example because they copied the file and edited it), the generated types are technically compatible at the wire level but incompatible at the C# type level — you cannot assign one service's generated `OrderRequest` to the other's.
- Best practice: set `package` to a stable, versioned identifier like `mycompany.orders.v1` and set `csharp_namespace` to your C# project's namespace like `MyCompany.Orders.V1`.

---

## Chapter 4: Security — Authentication & Authorization

---

## Q11. What is mutual TLS (mTLS), and why is it the recommended authentication mechanism for gRPC service-to-service communication?

What is mutual TLS (mTLS), and why is it the recommended authentication mechanism for gRPC service-to-service communication?

**Answer:** Mutual Transport Layer Security (mTLS) is a TLS handshake in which both the client and the server present and validate X.509 certificates, proving their identities to each other before any data is exchanged. In a standard one-way TLS connection, only the server presents a certificate; in mTLS, the client also presents one, which is validated by the server against a trusted certificate authority (CA).

- mTLS solves the service identity problem at the transport layer: before a single byte of gRPC payload is exchanged, both parties have cryptographically proven who they are, without needing to pass credentials in headers.
- In Kubernetes, service mesh solutions (Istio, Linkerd, Consul Connect) inject sidecar proxies that handle mTLS transparently at the network layer, so application code requires no changes — all gRPC connections between pods are automatically mutually authenticated and encrypted.
- Without mTLS or an equivalent identity mechanism, a rogue service that discovers the internal DNS name of another service can call it freely — there is no network-level authentication stopping it.
- For services where a service mesh is not available, you can configure Kestrel with a `ClientCertificateMode.RequireCertificate` option and validate the client certificate against a known CA in a middleware or an interceptor.
- mTLS is complementary to JWT-based authorization: mTLS proves which service is calling, while JWTs propagate the end-user's identity for resource-level authorization decisions inside the service.

---

## Q12. How do you propagate a caller's JWT bearer token from an inbound HTTP request into an outgoing gRPC call using call metadata?

How do you propagate a caller's JWT bearer token from an inbound HTTP request into an outgoing gRPC call using call metadata?

**Answer:** In a service that receives an HTTP request with an `Authorization: Bearer <token>` header and then makes a downstream gRPC call, you extract the token from `IHttpContextAccessor` and attach it to the gRPC call's metadata so the downstream service can authenticate the original user. This is typically done in a client-side interceptor registered on the gRPC client.

```csharp
public class AuthHeaderInterceptor : Interceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AuthHeaderInterceptor(IHttpContextAccessor a) => _httpContextAccessor = a;

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var token = _httpContextAccessor.HttpContext?
            .Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(token))
        {
            var headers = context.Options.Headers ?? new Metadata();
            headers.Add("Authorization", token);
            context = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method, context.Host,
                context.Options.WithHeaders(headers));
        }
        return continuation(request, context);
    }
}
```

- You register `IHttpContextAccessor` with `builder.Services.AddHttpContextAccessor()` so the interceptor can access the ambient HTTP context from the incoming request.
- The token is added as a gRPC metadata entry with the key `Authorization` and the value `Bearer <token>`. The downstream gRPC service's ASP.NET Core pipeline reads this as a standard `Authorization` header and validates it with the configured JWT bearer middleware.
- Never log the raw token value; log only that a token was present or absent. Tokens are credentials and must be treated as secrets in logs and traces.

---

## Q13. What is the difference between channel credentials and call credentials in gRPC, and when would you use each?

What is the difference between channel credentials and call credentials in gRPC, and when would you use each?

**Answer:** Channel credentials secure the entire channel — they are established once during the TLS handshake and apply to all calls made over that channel. Call credentials are per-RPC metadata (most commonly an `Authorization` header) that are attached to individual calls and can carry different values for different calls made on the same channel.

- Channel credentials include TLS certificate configuration and, in the gRPC C-core library, `SslCredentials`. In .NET's managed gRPC, TLS is configured on the `SocketsHttpHandler` and `GrpcChannel` level — effectively the same layer.
- Call credentials (in the C-core model, `CallCredentials`) are a way to attach a token-fetching function to every call without touching the channel configuration. In .NET's managed gRPC, the equivalent is attaching metadata in a client interceptor or in `CallOptions.Headers` on each individual call.
- Use channel-level authentication for service identity (mTLS — the certificate does not change call to call). Use call-level credentials for user identity (JWT — the token is different for each end-user request that flows through the service).
- Combining both is the standard pattern: mTLS at the channel level proves the calling service, JWT at the call level proves the end user. The downstream service can enforce both — "this is a legitimate internal service calling on behalf of this authenticated user."

---

## Q14. How do you authorize gRPC service methods in ASP.NET Core using `[Authorize]` attributes and policy-based authorization?

How do you authorize gRPC service methods in ASP.NET Core using `[Authorize]` attributes and policy-based authorization?

**Answer:** ASP.NET Core's standard authorization middleware works with gRPC services out of the box because gRPC services are part of the same request pipeline. You apply `[Authorize]` at the class or method level on your service implementation, and you configure policies in `Program.cs` just as you would for a controller.

```csharp
[Authorize(Policy = "RequireInternalService")]
public class OrderServiceImpl : OrderService.OrderServiceBase
{
    [AllowAnonymous]
    public override Task<PingReply> Ping(PingRequest r, ServerCallContext ctx)
        => Task.FromResult(new PingReply { Ok = true });

    public override async Task<OrderReply> GetOrder(
        OrderRequest request, ServerCallContext context)
    { /* only reached by authorized callers */ }
}
```

- You must add `app.UseAuthentication()` and `app.UseAuthorization()` to the middleware pipeline before `app.MapGrpcService<T>()`, just as with controllers.
- The `[Authorize]` attribute reads claims from the `ClaimsPrincipal` attached to the gRPC call's `HttpContext`. If the call carries a valid JWT, the bearer middleware populates the principal and authorization proceeds normally.
- Unauthorized calls receive a `StatusCode.Unauthenticated` or `StatusCode.PermissionDenied` status code, which the client sees as an `RpcException`.
- For service-to-service scenarios, a custom policy can inspect a specific claim (such as a `service_name` claim in a client certificate or a service-level JWT scope) rather than relying solely on user identity.

---

## Chapter 5: Resiliency — Retry, Hedging & Deadlines

---

## Q15. How do you configure retry policies for a gRPC client in .NET using the built-in gRPC retry support, and how does it differ from wrapping calls with Polly?

How do you configure retry policies for a gRPC client in .NET using the built-in gRPC retry support, and how does it differ from wrapping calls with Polly?

**Answer:** .NET's gRPC client supports a built-in retry mechanism configured via `GrpcChannelOptions.ServiceConfig`, which follows the gRPC retry specification and operates inside the gRPC transport layer before the call is returned to application code. This is different from wrapping calls in a Polly policy, where each retry is a full new call from the application layer.

```csharp
var serviceConfig = new ServiceConfig
{
    MethodConfigs = { new MethodConfig
    {
        Names = { MethodName.Default },
        RetryPolicy = new RetryPolicy
        {
            MaxAttempts = 4,
            InitialBackoff = TimeSpan.FromSeconds(1),
            MaxBackoff = TimeSpan.FromSeconds(5),
            BackoffMultiplier = 1.5,
            RetryableStatusCodes = { StatusCode.Unavailable }
        }
    }}
};
var channel = GrpcChannel.ForAddress("https://order-service",
    new GrpcChannelOptions { ServiceConfig = serviceConfig });
```

- gRPC built-in retry is aware of hedging, deadlines, and the gRPC call lifecycle; it integrates with the stream state machine and can retry transparent calls (those where no response bytes have been received yet) automatically.
- Polly wraps at the application layer: each retry creates a new `CallOptions` object and makes a fresh call. Polly is more flexible (you can implement circuit breakers, bulkhead isolation, and fallbacks), but it is gRPC-unaware and will retry even non-retryable status codes unless you write the predicate yourself.
- The built-in retry only retries on status codes you explicitly list in `RetryableStatusCodes`. `UNAVAILABLE` is the safest default. Never add `INTERNAL` to that list — internal errors often indicate a bug on the server, and retrying does not fix bugs.

---

## Q16. What is hedging in gRPC, how does it differ from retry, and what conditions make it unsafe to use?

What is hedging in gRPC, how does it differ from retry, and what conditions make it unsafe to use?

**Answer:** Hedging sends multiple copies of the same RPC to the server simultaneously (or with a short delay between them) and returns the first successful response, cancelling the remaining in-flight calls. Retry waits for a call to fail before sending the next attempt; hedging overlaps attempts proactively, trading additional server load and duplicate processing for reduced tail latency.

- Hedging is configured via `HedgingPolicy` in `ServiceConfig`, specifying the `MaxAttempts` and an optional `HedgingDelay` — the interval between successive hedged requests.
- Hedging is only safe for idempotent operations: reading data, checking a status, or any operation that can safely execute multiple times and produce no side effects. Hedging a payment charge or a database insert will result in duplicate charges or duplicate rows.
- Even for read operations, hedging increases backend load proportionally to `MaxAttempts`, so it should only be used where tail latency reduction is critical (e.g., low-percentile SLA requirements on hot read paths).
- The gRPC specification distinguishes transparent retries (safe, no response received) from retried retries (requires explicit configuration) and hedging; only transparent retries are done automatically without any policy configuration.

---

## Q17. How does a client deadline interact with a retry policy in a multi-hop service call, and what happens when the deadline expires during a retry sequence?

How does a client deadline interact with a retry policy in a multi-hop service call, and what happens when the deadline expires during a retry sequence?

**Answer:** A gRPC deadline is an absolute timestamp that propagates with the call; it represents the time by which the entire distributed operation must complete, not just a single hop. When a retry policy is active and the remaining deadline budget is smaller than the next backoff interval, the gRPC runtime cancels the retry sequence and returns `DEADLINE_EXCEEDED` to the caller rather than waiting for the backoff to elapse.

- If Service A sets a 3-second deadline and calls Service B, and the first attempt fails after 2 seconds with `UNAVAILABLE`, there is only 1 second left before the deadline. If the configured `InitialBackoff` is 2 seconds, the retry is never attempted — the call returns `DEADLINE_EXCEEDED` immediately.
- This is the correct behavior: retrying after the deadline is pointless because the upstream caller (Service A's own caller) has already received or is about to receive a timeout error.
- Deadline propagation across services means that when Service A calls Service B and Service B calls Service C, each downstream call should carry the remaining time budget of the original deadline — not create a fresh timeout. This prevents a slow chain from burning wall-clock time that has already expired from the perspective of the ultimate caller.
- Implement deadline propagation by reading `ServerCallContext.Deadline` on the inbound call and constructing outbound `CallOptions` with `deadline: inboundContext.Deadline` (not a fixed offset from `DateTime.UtcNow`).

---

## Chapter 6: Load Balancing & Service Discovery

---

## Q18. Why does the default Kubernetes `ClusterIP` service not distribute gRPC traffic evenly across pods, and what is the root cause?

Why does the default Kubernetes `ClusterIP` service not distribute gRPC traffic evenly across pods, and what is the root cause?

**Answer:** A Kubernetes `ClusterIP` service load balances at the TCP connection level using `iptables` rules: each new TCP connection is directed to a pod chosen by round-robin (or random) selection. Since gRPC runs over HTTP/2, a single long-lived TCP connection carries all RPC calls as multiplexed streams. Once the gRPC client opens that connection to one pod, all subsequent RPCs in the process go to the same pod — no new TCP connections, no new load balancing decisions.

- In REST over HTTP/1.1, each request typically opens a new connection (or connection pool entries cycle), so `iptables` round-robin distributes traffic relatively evenly over time.
- In gRPC, the `GrpcClientFactory` deliberately reuses a single channel (and its underlying TCP connection) for efficiency. This is correct behavior for HTTP/2, but it means one pod handles all traffic from one client pod, while other pods sit idle.
- This is not a Kubernetes bug — it is the correct consequence of using long-lived HTTP/2 connections with a TCP-level load balancer. The solution requires moving load balancing to the application layer (the gRPC client itself) or to a Layer-7 proxy (like Envoy or Linkerd) that understands HTTP/2 streams.

---

## Q19. What is a headless Kubernetes service, and how does it enable client-side gRPC load balancing?

What is a headless Kubernetes service, and how does it enable client-side gRPC load balancing?

**Answer:** A headless Kubernetes service is defined with `clusterIP: None`, which instructs Kubernetes not to allocate a virtual IP address for the service. Instead, a DNS query for the service returns A records for each individual pod endpoint IP, allowing the client to see and connect to individual pods directly.

- With a normal `ClusterIP` service, `nslookup order-svc` returns one IP (the virtual IP), and all connections go through `iptables` to one pod. With a headless service, `nslookup order-svc` returns multiple A records — one per ready pod.
- The gRPC client can be configured with a `DnsResolverFactory` that resolves the headless service name and distributes calls across all returned IPs using round-robin. Each pod gets its own HTTP/2 connection, and the client balances new RPCs across those connections.
- Headless services also enable Kubernetes DNS to return accurate round-trip information about pod addresses, which other service-discovery mechanisms (such as Consul or Eureka) build on.
- The limitation is that the client must update its endpoint list when pods scale up or down; the gRPC DNS resolver periodically re-resolves the name, but the refresh interval (typically 30 seconds) means new pods are not used immediately after scaling.

---

## Q20. How do you configure a .NET gRPC client to use DNS-based service discovery with round-robin load balancing across resolved pod addresses?

How do you configure a .NET gRPC client to use DNS-based service discovery with round-robin load balancing across resolved pod addresses?

**Answer:** You configure the `GrpcChannel` with a `DnsResolverFactory` and a `RoundRobinBalancerFactory` from the `Grpc.Net.Client.Balancing` namespace. The channel resolves the DNS name at startup and periodically thereafter, then distributes RPCs across all returned endpoints using round-robin selection.

```csharp
var channel = GrpcChannel.ForAddress("dns:///order-svc:5001",
    new GrpcChannelOptions
    {
        ServiceConfig = new ServiceConfig
        {
            LoadBalancingConfigs = { new RoundRobinConfig() }
        }
    });
```

- The `dns:///` URI scheme activates the DNS resolver. The triple slash is required — `dns:` is the scheme, the host is `order-svc:5001` (for a headless Kubernetes service).
- `RoundRobinConfig` in the `ServiceConfig.LoadBalancingConfigs` tells the channel to use round-robin balancing across all resolved addresses. Other options include `PickFirstConfig` (the default, which always uses the first address — equivalent to no load balancing).
- When integrated with `GrpcClientFactory`, you configure the channel via `ConfigureChannel()` or by providing a `GrpcChannelOptions` factory in `AddGrpcClient`.
- Service meshes like Istio or Linkerd handle this at the network layer without any client configuration — in a mesh, you can use a standard `ClusterIP` service and the sidecar proxy handles L7 load balancing. Use client-side load balancing only when a service mesh is not available.

---

## Chapter 7: Health Checks & Observability

---

## Q21. What is the standard gRPC health checking protocol, how do you implement it in an ASP.NET Core service, and how does Kubernetes use it?

What is the standard gRPC health checking protocol, how do you implement it in an ASP.NET Core service, and how does Kubernetes use it?

**Answer:** The gRPC health checking protocol is a standardized gRPC service defined in `grpc/health/v1/health.proto`. It exposes a `Check` unary RPC and a `Watch` server-streaming RPC. The `Check` method accepts a service name (or empty string for overall health) and returns `SERVING`, `NOT_SERVING`, or `SERVICE_UNKNOWN`. Kubernetes uses this protocol via its `grpc` probe type to determine pod readiness and liveness.

- In ASP.NET Core, add the `Grpc.HealthCheck` NuGet package and register the service: `builder.Services.AddGrpcHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy())`. Map it with `app.MapGrpcHealthChecksService()`.
- The `GrpcHealthChecksPublisher` bridges ASP.NET Core's built-in `IHealthCheckService` with the gRPC health protocol, so your existing `IHealthCheck` implementations (database connectivity, external dependency checks) automatically report their status through the gRPC health endpoint.
- In Kubernetes, configure a `grpc` liveness or readiness probe pointing to the service's port. Kubernetes 1.24+ calls the `grpc.health.v1.Health/Check` RPC using the `grpc_health_probe` binary (bundled in newer versions) and considers the pod healthy only when the response is `SERVING`.
- Using gRPC health probes avoids the need to expose a separate HTTP health endpoint, keeping the service's surface area minimal.

---

## Q22. How does OpenTelemetry instrument gRPC calls in .NET, and what trace attributes does it attach to client and server spans?

How does OpenTelemetry instrument gRPC calls in .NET, and what trace attributes does it attach to client and server spans?

**Answer:** OpenTelemetry (OTel) for .NET instruments gRPC calls automatically when you add the `OpenTelemetry.Instrumentation.GrpcNetClient` package for client spans and `OpenTelemetry.Instrumentation.AspNetCore` for server spans. Each outbound RPC and each inbound RPC handler creates a span that participates in the distributed trace, propagating the `traceparent` W3C header through gRPC metadata.

- Client spans carry the attributes: `rpc.system = "grpc"`, `rpc.service` (the proto service name), `rpc.method` (the proto method name), `rpc.grpc.status_code` (the numeric gRPC status code on completion), and `net.peer.name` / `net.peer.port`.
- Server spans carry the same `rpc.*` attributes plus `net.host.name` / `net.host.port` and the HTTP/2 attributes from the underlying request.
- The trace context is propagated in gRPC metadata as the `traceparent` and `tracestate` headers. The OTel ASP.NET Core instrumentation reads these on the server side and creates child spans automatically — no manual context propagation code is needed.
- To enable: call `tracerProviderBuilder.AddGrpcClientInstrumentation()` in the OTel setup. On the server, `AddAspNetCoreInstrumentation()` covers both HTTP and gRPC handlers.
- Without OTel, debugging a failed cross-service gRPC call requires grepping logs from multiple services by timestamp — with OTel, you search by `trace_id` and see the full call chain in a single waterfall view in Jaeger, Zipkin, or Azure Monitor.

---

## Q23. How do you implement a server-side gRPC interceptor to emit structured log entries for every request and response, and how does it differ from ASP.NET Core middleware?

How do you implement a server-side gRPC interceptor to emit structured log entries for every request and response, and how does it differ from ASP.NET Core middleware?

**Answer:** A gRPC server interceptor inherits from `Grpc.Core.Interceptors.Interceptor` and overrides the handler methods (`UnaryServerHandler`, `ServerStreamingServerHandler`, etc.). Unlike ASP.NET Core middleware, which operates on `HttpContext` and sees only raw HTTP request and response bytes, an interceptor operates on the typed gRPC request and response objects and has access to `ServerCallContext` including the method name, deadline, peer address, and metadata.

```csharp
public class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;
    public LoggingInterceptor(ILogger<LoggingInterceptor> l) => _logger = l;

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext ctx,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("gRPC {Method} from {Peer}", ctx.Method, ctx.Peer);
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await continuation(request, ctx);
            _logger.LogInformation("{Method} OK in {Ms}ms", ctx.Method, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed", ctx.Method);
            throw;
        }
    }
}
```

- Register it at startup with `builder.Services.AddGrpc(o => o.Interceptors.Add<LoggingInterceptor>())`.
- Interceptors operate after the gRPC framing layer decodes the message but before the service method is invoked, giving you access to strongly typed request objects rather than raw bytes.
- ASP.NET Core middleware can also intercept gRPC calls but at the HTTP level — it sees headers and status codes, not proto message content. Use middleware for HTTP-level concerns (rate limiting, correlation ID injection) and interceptors for gRPC-semantic concerns (logging method names, mapping exceptions to status codes).

---

## Chapter 8: gRPC in Containers & Kubernetes

---

## Q24. What Kestrel and environment configuration changes are required when running a gRPC service in a Docker container without TLS (plain HTTP/2)?

What Kestrel and environment configuration changes are required when running a gRPC service in a Docker container without TLS (plain HTTP/2)?

**Answer:** By default, Kestrel negotiates HTTP/2 only over TLS using ALPN (Application-Layer Protocol Negotiation). To run plain HTTP/2 (also called H2C — HTTP/2 Cleartext) inside a Docker container or Kubernetes pod (where TLS is handled by the service mesh or ingress), you must explicitly configure Kestrel to accept HTTP/2 without TLS and enable the corresponding .NET switch.

```csharp
// Program.cs
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});
```

- `Http2UnencryptedSupport` must be set to `true` before the app starts; it lifts the .NET runtime's default refusal to speak H2C (which exists to prevent accidental cleartext use in non-containerized environments).
- Setting `HttpProtocols.Http2` on the Kestrel endpoint tells Kestrel not to negotiate HTTP/1.1 on that port — the port speaks only HTTP/2, which is what gRPC clients expect.
- In Docker Compose or a Kubernetes pod spec, expose port 5001 and configure the gRPC client's address as `http://` (not `https://`), for example `http://order-svc:5001`.
- In a service mesh (Istio, Linkerd), the sidecar proxy intercepts all traffic and upgrades connections to mTLS transparently; the application itself communicates in plain H2C to the sidecar on `localhost`, which is secure because the traffic never leaves the pod boundary unencrypted.

---

## Q25. What is the `grpc` probe type in Kubernetes 1.24+, and how does it differ from a plain HTTP liveness probe for a gRPC-only service?

What is the `grpc` probe type in Kubernetes 1.24+, and how does it differ from a plain HTTP liveness probe for a gRPC-only service?

**Answer:** The `grpc` probe type, introduced in Kubernetes 1.24 as a stable feature, allows the kubelet to perform a native gRPC `Health/Check` RPC directly against the pod's gRPC port, without requiring an additional HTTP health endpoint or a sidecar `grpc_health_probe` binary. The kubelet connects to the specified port and calls the standard `grpc.health.v1.Health/Check` method; a `SERVING` response means healthy, anything else (or a connection error) means unhealthy.

```yaml
livenessProbe:
  grpc:
    port: 5001
    service: ""        # empty string checks overall service health
  initialDelaySeconds: 5
  periodSeconds: 10
```

- Before Kubernetes 1.24, the only native probe types were `httpGet`, `tcpSocket`, and `exec`. To health-check a gRPC-only service, teams either added an HTTP health endpoint (mixing protocols), ran `grpc_health_probe` as an `exec` probe (requiring the binary in the container image), or used a TCP probe (which only checks if the port is open, not if the service is functioning).
- The `grpc` probe type eliminates all three workarounds. The kubelet speaks gRPC directly, so you do not need to embed `grpc_health_probe` in your Docker image or expose an HTTP port.
- The `service` field maps to the `service` field in the `HealthCheckRequest` proto message. An empty string requests the overall health of the server. A service name like `"grpc.orders.OrderService"` can return the health of that specific named service if the service implementation registers per-service health status.
- The probe requires that the gRPC service implement the standard health checking protocol (`grpc.health.v1.Health`), which in ASP.NET Core is provided by the `Grpc.HealthCheck` package registered via `AddGrpcHealthChecks()`.

---
