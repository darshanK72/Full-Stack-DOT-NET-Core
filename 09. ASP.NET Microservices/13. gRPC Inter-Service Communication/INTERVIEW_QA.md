# Interview Questions — gRPC Inter-Service Communication — Interview Q&A
> 25 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. Why is gRPC typically preferred over REST for synchronous service-to-service calls inside a microservices system, and what are the trade-offs of that choice?](#q1-why-is-grpc-typically-preferred-over-rest-for-synchronous-service-to-service-calls-inside-a-microservices-system-and-what-are-the-trade-offs-of-that-choice)
2. [Q2. What is HTTP/2 multiplexing, and why does it matter for gRPC inter-service communication under high concurrency?](#q2-what-is-http2-multiplexing-and-why-does-it-matter-for-grpc-inter-service-communication-under-high-concurrency)
3. [Q3. When should you choose an asynchronous message broker over a synchronous gRPC call between two microservices?](#q3-when-should-you-choose-an-asynchronous-message-broker-over-a-synchronous-grpc-call-between-two-microservices)
4. [Q4. What is the GrpcClientFactory pattern in ASP.NET Core, and why is it preferred over constructing `GrpcChannel` directly inside each service?](#q4-what-is-the-grpcclientfactory-pattern-in-aspnet-core-and-why-is-it-preferred-over-constructing-grpcchannel-directly-inside-each-service)
5. [Q5. How do you register a typed gRPC client with `AddGrpcClient<T>`, configure its base address, and inject it via dependency injection?](#q5-how-do-you-register-a-typed-grpc-client-with-addgrpcclientt-configure-its-base-address-and-inject-it-via-dependency-injection)
6. [Q6. How do you attach a client-side interceptor to a gRPC client registered via GrpcClientFactory, and what cross-cutting concerns does it typically handle?](#q6-how-do-you-attach-a-client-side-interceptor-to-a-grpc-client-registered-via-grpcclientfactory-and-what-cross-cutting-concerns-does-it-typically-handle)
7. [Q7. How do you configure `HttpClient` and channel options — connection lifetime, keepalive pings, and HTTP version — for a GrpcClientFactory-managed channel?](#q7-how-do-you-configure-httpclient-and-channel-options-connection-lifetime-keepalive-pings-and-http-version-for-a-grpcclientfactory-managed-channel)
8. [Q8. How should `.proto` files be shared across multiple microservices — what are the options and the trade-offs of each approach?](#q8-how-should-proto-files-be-shared-across-multiple-microservices-what-are-the-options-and-the-trade-offs-of-each-approach)
9. [Q9. What is the difference between a backward-compatible and a breaking proto schema change in a live microservices environment, and how do teams manage rolling upgrades safely?](#q9-what-is-the-difference-between-a-backward-compatible-and-a-breaking-proto-schema-change-in-a-live-microservices-environment-and-how-do-teams-manage-rolling-upgrades-safely)
10. [Q10. What is the purpose of the `csharp_namespace` and `package` options in a `.proto` file, and what problems arise if they are omitted or mismatched across services?](#q10-what-is-the-purpose-of-the-csharp_namespace-and-package-options-in-a-proto-file-and-what-problems-arise-if-they-are-omitted-or-mismatched-across-services)
11. [Q11. What is mutual TLS (mTLS), and why is it the recommended authentication mechanism for gRPC service-to-service communication?](#q11-what-is-mutual-tls-mtls-and-why-is-it-the-recommended-authentication-mechanism-for-grpc-service-to-service-communication)
12. [Q12. How do you propagate a caller's JWT bearer token from an inbound HTTP request into an outgoing gRPC call using call metadata?](#q12-how-do-you-propagate-a-callers-jwt-bearer-token-from-an-inbound-http-request-into-an-outgoing-grpc-call-using-call-metadata)
13. [Q13. What is the difference between channel credentials and call credentials in gRPC, and when would you use each?](#q13-what-is-the-difference-between-channel-credentials-and-call-credentials-in-grpc-and-when-would-you-use-each)
14. [Q14. How do you authorize gRPC service methods in ASP.NET Core using `[Authorize]` attributes and policy-based authorization?](#q14-how-do-you-authorize-grpc-service-methods-in-aspnet-core-using-authorize-attributes-and-policy-based-authorization)
15. [Q15. How do you configure retry policies for a gRPC client in .NET using the built-in gRPC retry support, and how does it differ from wrapping calls with Polly?](#q15-how-do-you-configure-retry-policies-for-a-grpc-client-in-net-using-the-built-in-grpc-retry-support-and-how-does-it-differ-from-wrapping-calls-with-polly)
16. [Q16. What is hedging in gRPC, how does it differ from retry, and what conditions make it unsafe to use?](#q16-what-is-hedging-in-grpc-how-does-it-differ-from-retry-and-what-conditions-make-it-unsafe-to-use)
17. [Q17. How does a client deadline interact with a retry policy in a multi-hop service call, and what happens when the deadline expires during a retry sequence?](#q17-how-does-a-client-deadline-interact-with-a-retry-policy-in-a-multi-hop-service-call-and-what-happens-when-the-deadline-expires-during-a-retry-sequence)
18. [Q18. Why does the default Kubernetes `ClusterIP` service not distribute gRPC traffic evenly across pods, and what is the root cause?](#q18-why-does-the-default-kubernetes-clusterip-service-not-distribute-grpc-traffic-evenly-across-pods-and-what-is-the-root-cause)
19. [Q19. What is a headless Kubernetes service, and how does it enable client-side gRPC load balancing?](#q19-what-is-a-headless-kubernetes-service-and-how-does-it-enable-client-side-grpc-load-balancing)
20. [Q20. How do you configure a .NET gRPC client to use DNS-based service discovery with round-robin load balancing across resolved pod addresses?](#q20-how-do-you-configure-a-net-grpc-client-to-use-dns-based-service-discovery-with-round-robin-load-balancing-across-resolved-pod-addresses)
21. [Q21. What is the standard gRPC health checking protocol, how do you implement it in an ASP.NET Core service, and how does Kubernetes use it?](#q21-what-is-the-standard-grpc-health-checking-protocol-how-do-you-implement-it-in-an-aspnet-core-service-and-how-does-kubernetes-use-it)
22. [Q22. How does OpenTelemetry instrument gRPC calls in .NET, and what trace attributes does it attach to client and server spans?](#q22-how-does-opentelemetry-instrument-grpc-calls-in-net-and-what-trace-attributes-does-it-attach-to-client-and-server-spans)
23. [Q23. How do you implement a server-side gRPC interceptor to emit structured log entries for every request and response, and how does it differ from ASP.NET Core middleware?](#q23-how-do-you-implement-a-server-side-grpc-interceptor-to-emit-structured-log-entries-for-every-request-and-response-and-how-does-it-differ-from-aspnet-core-middleware)
24. [Q24. What Kestrel and environment configuration changes are required when running a gRPC service in a Docker container without TLS (plain HTTP/2)?](#q24-what-kestrel-and-environment-configuration-changes-are-required-when-running-a-grpc-service-in-a-docker-container-without-tls-plain-http2)
25. [Q25. What is the `grpc` probe type in Kubernetes 1.24+, and how does it differ from a plain HTTP liveness probe for a gRPC-only service?](#q25-what-is-the-grpc-probe-type-in-kubernetes-124-and-how-does-it-differ-from-a-plain-http-liveness-probe-for-a-grpc-only-service)

---

## Q1. Why is gRPC typically preferred over REST for synchronous service-to-service calls inside a microservices system, and what are the trade-offs of that choice?

**Concepts**
- Protobuf binary encoding — 3–10× smaller and faster than equivalent JSON
- HTTP/2 multiplexing eliminating per-request connection overhead
- Strongly typed generated stubs catching contract mismatches at compile time
- Trade-offs: HTTP/2 dependency, binary payload debugging difficulty, smaller tooling ecosystem

**Answer**

gRPC is preferred for inter-service communication because it uses HTTP/2 as its transport and Protocol Buffers for serialization, yielding significantly lower latency and payload size compared to JSON over HTTP/1.1. Protobuf binary encoding is typically three to ten times smaller than equivalent JSON and faster to serialize and deserialize, which matters when Service A calls Service B thousands of times per second. HTTP/2 multiplexing allows many concurrent RPC calls to share a single TCP connection, eliminating the per-request connection overhead and head-of-line blocking present in HTTP/1.1. The strongly typed generated client and server stubs remove an entire category of integration errors — a field type change that breaks a consumer is caught when the shared `.proto` file is updated and the project fails to compile. The trade-offs are real: gRPC requires HTTP/2 end-to-end, which is problematic with some proxies and load balancers; debugging binary payloads is harder than inspecting JSON; and the tooling ecosystem is smaller than REST's. For external-facing APIs or third-party consumers I would use REST or GraphQL; gRPC belongs in the internal service mesh where both ends are controlled.

---

## Q2. What is HTTP/2 multiplexing, and why does it matter for gRPC inter-service communication under high concurrency?

**Concepts**
- HTTP/2 multiplexing — multiple concurrent streams over a single TCP connection
- Eliminating per-request TCP handshake and TLS negotiation cost
- `GrpcClientFactory` managing long-lived channel reuse automatically
- TCP-level head-of-line blocking remaining — eliminated only by HTTP/3 over QUIC

**Answer**

HTTP/2 multiplexing means multiple concurrent requests and responses can be interleaved over a single TCP connection as independent logical streams, each with its own stream identifier, rather than requiring a separate connection per request. In HTTP/1.1, a client wanting 100 concurrent requests to a service typically needs 100 TCP connections — 100 three-way handshakes and 100 sets of TLS negotiation. With HTTP/2, one TCP connection carries all 100 concurrent streams; the kernel and network stack handle one set of handshake and congestion-control state, reducing connection setup cost to near zero. gRPC channels are long-lived HTTP/2 connections managed by the `GrpcChannel`, and `GrpcClientFactory` handles reuse automatically, amortizing connection setup cost across all RPCs made through the channel. Head-of-line blocking at the HTTP layer is eliminated — a slow stream does not delay other streams — though TCP-level head-of-line blocking remains, which HTTP/3 over QUIC addresses, but gRPC does not yet use HTTP/3 in mainstream .NET. For a service making 5,000 gRPC calls per second, the difference between one shared channel and 5,000 per-request channels is the difference between a stable system and resource exhaustion.

---

## Q3. When should you choose an asynchronous message broker over a synchronous gRPC call between two microservices?

**Concepts**
- Message broker for fire-and-forget operations where no immediate result is needed
- Broker for durable delivery guarantees when the downstream may be temporarily unavailable
- gRPC for request-response operations requiring an immediate result
- High fan-out scenarios fitting a broker — broadcasting to N consumers avoids N serial gRPC calls

**Answer**

A message broker is the right choice when the calling service does not need an immediate result, when the receiving service may be temporarily unavailable, or when the operation's scope spans multiple services that must each react independently. Synchronous gRPC couples the caller's availability and latency directly to the downstream service's availability and latency, which becomes a liability for any non-trivial chain of dependencies. I would choose a message broker when the operation is fire-and-forget — publishing an `OrderPlaced` event that three other services consume on their own schedule — or when I need durable delivery guarantees: if the downstream service restarts mid-processing, the message stays in the queue and is retried, whereas a failed gRPC call requires the caller to implement its own retry logic. I would choose gRPC when I need an immediate response — checking current product stock before confirming a cart, or validating a payment token that must succeed before the HTTP response is sent to the user. High fan-out scenarios almost always fit a broker better than gRPC, because broadcasting a synchronous call to N services means N serial or N concurrent calls whose failures all need handling. In practice a microservices system uses both: gRPC for low-latency request-response calls where the caller waits, and a message broker for events that decouple producers from consumers.

---

## Chapter 2: GrpcClientFactory & Channel Management

---

## Q4. What is the GrpcClientFactory pattern in ASP.NET Core, and why is it preferred over constructing `GrpcChannel` directly inside each service?

**Concepts**
- GrpcClientFactory integrating `IHttpClientFactory` infrastructure with gRPC client creation
- Handler pooling and rotation respecting DNS TTLs without stale connections
- Centralized cross-cutting configuration — interceptors, credentials, base address
- DI integration injecting generated client types directly into application classes

**Answer**

`GrpcClientFactory` is the integration between the ASP.NET Core `IHttpClientFactory` infrastructure and gRPC client creation, provided by the `Grpc.Net.ClientFactory` NuGet package. It manages the lifecycle of the underlying `HttpClient` and `GrpcChannel` instances so that channels are reused across requests, connection lifetimes are rotated safely, and cross-cutting configuration is applied once at registration rather than scattered across call sites. Constructing a `GrpcChannel` directly in a service class means every instantiation of that class creates a new channel — and therefore a new HTTP/2 connection — which defeats multiplexing and causes socket exhaustion under load. `GrpcClientFactory` pools and reuses the underlying `HttpMessageHandler`, rotating it periodically — defaulting to two minutes — to respect DNS TTLs without leaving stale connections open indefinitely. Configuration is centralized: you register interceptors, credentials, base addresses, and retry policies once in `Program.cs`, and every injected client instance inherits them automatically. The factory integrates with ASP.NET Core's DI container, so you inject `OrderService.OrderServiceClient` directly into your application class — the factory handles instantiation, channel reuse, and disposal.

---

## Q5. How do you register a typed gRPC client with `AddGrpcClient<T>`, configure its base address, and inject it via dependency injection?

**Concepts**
- `AddGrpcClient<TClient>()` registering the generated client class with the DI container
- `Address` property set to the downstream service's URI
- Constructor injection of the generated client type in application classes
- Plain HTTP/2 via `AppContext.SetSwitch` for development or service mesh environments

**Answer**

You call `builder.Services.AddGrpcClient<TClient>()` in `Program.cs`, passing a configuration delegate that sets the base address of the target service. The factory then makes `TClient` available for injection anywhere in the application without the caller knowing anything about channels or HTTP/2.

```csharp
builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
{
    o.Address = new Uri("https://order-service:5001");
});
```

The generic type argument is the generated client class from your `.proto` file — for example `OrderService.OrderServiceClient`. The `Address` property sets the base URI of the downstream service; in Kubernetes you would use the DNS name such as `https://order-svc.orders.svc.cluster.local`. Once registered, you inject the client by declaring it as a constructor parameter: `public MyHandler(OrderService.OrderServiceClient client)`. The factory creates the client instance with the shared channel each time the handler is instantiated. For services without TLS in development or in a service mesh that handles mTLS at the sidecar level, you configure the channel to use plain HTTP/2 by calling `AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true)` and using an `http://` address.

---

## Q6. How do you attach a client-side interceptor to a gRPC client registered via GrpcClientFactory, and what cross-cutting concerns does it typically handle?

**Concepts**
- `.AddInterceptor<T>()` chained onto `AddGrpcClient` registration
- Interceptor overriding `AsyncUnaryCall` and streaming variants
- JWT propagation from `IHttpContextAccessor` as the most common use
- Deadline injection ensuring all outbound calls have a bounded timeout

**Answer**

You chain `.AddInterceptor<T>()` onto the `AddGrpcClient<TClient>()` registration in `Program.cs`. The interceptor is a class that inherits from `Grpc.Core.Interceptors.Interceptor` and overrides the handler methods to inject behavior before and after each RPC.

```csharp
builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
    {
        o.Address = new Uri("https://order-service:5001");
    })
    .AddInterceptor<AuthHeaderInterceptor>()
    .AddInterceptor<LoggingInterceptor>();
```

A common use is JWT propagation: the interceptor reads the current user's bearer token from `IHttpContextAccessor` and appends it to the outgoing gRPC call's metadata as an `Authorization` entry, so the downstream service can authenticate the original user. Logging interceptors record the method name, request size, deadline, and response status for every outbound RPC, giving structured telemetry without modifying individual call sites. Deadline-injection interceptors attach a default deadline to any call that does not already have one, preventing unbounded calls against a slow downstream service. Interceptors are resolved from the DI container per-request or per scope depending on registration lifetime, so they can take scoped dependencies like `IHttpContextAccessor`.

---

## Q7. How do you configure `HttpClient` and channel options — connection lifetime, keepalive pings, and HTTP version — for a GrpcClientFactory-managed channel?

**Concepts**
- `ConfigurePrimaryHttpMessageHandler()` controlling `SocketsHttpHandler` settings
- `KeepAlivePingDelay` preventing NAT and load balancer silent TCP connection drops
- `PooledConnectionIdleTimeout` set shorter than upstream load balancer idle timeout
- `EnableMultipleHttp2Connections` for additional connections when stream limit is reached

**Answer**

You chain `.ConfigurePrimaryHttpMessageHandler()` or `.ConfigureChannel()` onto the `AddGrpcClient` registration. `ConfigurePrimaryHttpMessageHandler` controls the underlying `SocketsHttpHandler`, where you set keepalive pings and connection lifetime. `ConfigureChannel` provides access to `GrpcChannelOptions`, where you control message size limits and credentials.

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

`PooledConnectionIdleTimeout` controls how long an idle HTTP/2 connection stays in the pool before being closed; set it shorter than any upstream load balancer's idle timeout to avoid receiving RST_STREAM frames on what the client believes is a valid connection. `KeepAlivePingDelay` tells the client to send a PING frame every N seconds on idle connections, which prevents NAT tables and load balancers from silently dropping the TCP connection; `KeepAlivePingTimeout` is how long to wait for a PONG before treating the connection as dead. `EnableMultipleHttp2Connections` allows the factory to open additional HTTP/2 connections to the same endpoint when the current connection is saturated with streams — gRPC's default maximum is 100 concurrent streams per connection. For plain HTTP/2 with no TLS, you must also set `AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true)` before building the app.

---

## Chapter 3: Proto Contract Sharing

---

## Q8. How should `.proto` files be shared across multiple microservices — what are the options and the trade-offs of each approach?

**Concepts**
- Copy-per-repo as simple but drift-prone
- Shared NuGet package as the most robust option for separate repositories
- Git submodule or subtree for a single source of truth without a package feed
- Monorepo shared project guaranteed consistent through the build graph

**Answer**

There are three main approaches to sharing `.proto` files across microservices. Copying files into each service's repository is simple and requires no tooling, but copies go out of sync easily and the mismatch may not be caught until runtime. Sharing via a private NuGet package containing the `.proto` files and the generated C# stubs gives each consumer an explicit version to pin and an upgrade path — publishing a new version of `MyCompany.Contracts.Orders` signals that consumers need to update. This is the most robust approach for teams with separate repositories, though it requires a package feed and publishing discipline. A git submodule or subtree creates a single source of truth without a feed but adds git complexity, and teams must remember to update the submodule when the proto definition changes. In a monorepo, a shared `Directory.Build.props` can reference a common `Protos/` folder, and all services regenerate stubs from the same source on every build, guaranteeing consistency through the build graph. Whichever approach you choose, treat `.proto` files with the same discipline as a public API: version them, maintain a changelog, and communicate breaking changes ahead of deployment.

---

## Q9. What is the difference between a backward-compatible and a breaking proto schema change in a live microservices environment, and how do teams manage rolling upgrades safely?

**Concepts**
- Backward-compatible changes — adding new fields, renaming fields (names not on wire)
- Breaking changes — reusing a field number with a different type, changing wire type
- Deploy server first — new servers still accept old requests with absent new fields
- `reserved` keyword preventing accidental field number reuse

**Answer**

A backward-compatible proto change is one that does not alter the wire encoding of existing fields, so old clients can communicate with new servers and vice versa without modification. A breaking change alters wire encoding, removes a required field in proto2, or reuses a field number with a different type, causing deserialization failures when an old client talks to a new server. Backward-compatible changes include adding a new field with a new field number, renaming a field — field names are not on the wire, only numbers are — marking a field as `reserved`, and adding a new enum value. Breaking changes include deleting a field number and reassigning it to a new type, changing a field's wire type such as from `int32` to `string`, and adding `oneof` around an existing field. Rolling upgrades work by deploying the new server first — which still accepts old requests because the new field is simply absent from old messages — then upgrading clients one by one. Old clients send messages without the new field; new servers read zero or empty for that field, which is the protobuf default. I would use the `reserved` keyword when retiring a field to prevent accidental reuse of the field number — `reserved 3; reserved "old_name";` — which turns accidental reuse into a compile error.

---

## Q10. What is the purpose of the `csharp_namespace` and `package` options in a `.proto` file, and what problems arise if they are omitted or mismatched across services?

**Concepts**
- `package` option for logical namespace and symbol disambiguation
- `csharp_namespace` overriding generated C# namespace to follow PascalCase conventions
- Absent `package` causing global namespace collisions in multi-proto codebases
- Mismatched `csharp_namespace` across copied files causing wire-compatible but C#-incompatible types

**Answer**

The `package` option defines the logical namespace of the proto messages and is used by the protobuf runtime for reflection, symbol resolution, and disambiguation when two proto files define messages with the same name. The `csharp_namespace` option overrides the C# namespace of the generated code so it can follow your project's naming conventions independently of the proto package name. Without `package`, two proto files that both define a message named `OrderRequest` produce ambiguous symbol resolution at the protobuf level, and the generated C# code may end up in the global namespace, causing compile-time collisions. Without `csharp_namespace`, the generated C# class is placed in a namespace derived from the `package` value, which typically uses lowercase dot-separated names like `orders.v1` rather than the PascalCase conventions of C# like `MyApp.Orders.V1`. If two services include the same `.proto` file but with different `csharp_namespace` values — for example because they copied the file and edited it separately — the generated types are technically compatible at the wire level but incompatible at the C# type level: you cannot assign one service's generated `OrderRequest` to the other's without mapping. The best practice is to set `package` to a stable versioned identifier like `mycompany.orders.v1` and `csharp_namespace` to your C# project's namespace like `MyCompany.Orders.V1`.

---

## Chapter 4: Security — Authentication & Authorization

---

## Q11. What is mutual TLS (mTLS), and why is it the recommended authentication mechanism for gRPC service-to-service communication?

**Concepts**
- mTLS — both client and server present X.509 certificates proving identity before data exchange
- Service identity at the transport layer without credential headers
- Service mesh sidecar proxies handling mTLS transparently with no application code changes
- mTLS complementary to JWT — mTLS proves which service, JWT proves which user

**Answer**

Mutual Transport Layer Security is a TLS handshake in which both the client and the server present and validate X.509 certificates, proving their identities to each other before any data is exchanged. In a standard one-way TLS connection only the server presents a certificate; in mTLS the client also presents one, which is validated by the server against a trusted certificate authority. mTLS solves the service identity problem at the transport layer: before a single byte of gRPC payload is exchanged, both parties have cryptographically proven who they are, without needing to pass credentials in headers. In Kubernetes, service mesh solutions such as Istio, Linkerd, and Consul Connect inject sidecar proxies that handle mTLS transparently at the network layer, so application code requires no changes — all gRPC connections between pods are automatically mutually authenticated and encrypted. Without mTLS or an equivalent identity mechanism, a rogue service that discovers the internal DNS name of another service can call it freely since there is no network-level authentication stopping it. For services where a service mesh is not available, you can configure Kestrel with `ClientCertificateMode.RequireCertificate` and validate the client certificate in a middleware or interceptor. mTLS is complementary to JWT-based authorization: mTLS proves which service is calling, while JWTs propagate the end-user's identity for resource-level authorization decisions inside the service.

---

## Q12. How do you propagate a caller's JWT bearer token from an inbound HTTP request into an outgoing gRPC call using call metadata?

**Concepts**
- `IHttpContextAccessor` reading the bearer token from the inbound HTTP context
- gRPC `Metadata` entry with key `Authorization` carrying the token to the downstream service
- Client interceptor as the correct place for token propagation — applies to all calls
- Never logging raw token values — treat tokens as secrets in traces and logs

**Answer**

In a service that receives an HTTP request with an `Authorization: Bearer <token>` header and then makes a downstream gRPC call, you extract the token from `IHttpContextAccessor` and attach it to the gRPC call's metadata so the downstream service can authenticate the original user. This is typically done in a client-side interceptor registered on the gRPC client.

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

You register `IHttpContextAccessor` with `builder.Services.AddHttpContextAccessor()` so the interceptor can access the ambient HTTP context from the incoming request. The token is added as a gRPC metadata entry with the key `Authorization` and the value `Bearer <token>`; the downstream gRPC service's ASP.NET Core pipeline reads this as a standard `Authorization` header and validates it with the configured JWT bearer middleware. Never log the raw token value — log only whether a token was present or absent, since tokens are credentials and must be treated as secrets in logs and traces.

---

## Q13. What is the difference between channel credentials and call credentials in gRPC, and when would you use each?

**Concepts**
- Channel credentials — TLS configuration established once for all calls on the channel
- Call credentials — per-RPC metadata varying call to call on the same channel
- mTLS as channel-level identity for service-to-service authentication
- JWT as call-level identity for end-user authorization per request

**Answer**

Channel credentials secure the entire channel — they are established once during the TLS handshake and apply to all calls made over that channel. Call credentials are per-RPC metadata — most commonly an `Authorization` header — attached to individual calls and can carry different values for different calls on the same channel. Channel credentials include TLS certificate configuration; in .NET's managed gRPC, TLS is configured at the `SocketsHttpHandler` and `GrpcChannel` level. Call credentials in the C-core model are a way to attach a token-fetching function to every call without touching the channel configuration; in .NET's managed gRPC, the equivalent is attaching metadata in a client interceptor or in `CallOptions.Headers` on each individual call. I would use channel-level credentials for service identity via mTLS — the certificate does not change call to call — and call-level credentials for user identity via JWT — the token is different for each end-user request that flows through the service. Combining both is the standard pattern: mTLS at the channel level proves the calling service, JWT at the call level proves the end user, and the downstream service can enforce both simultaneously.

---

## Q14. How do you authorize gRPC service methods in ASP.NET Core using `[Authorize]` attributes and policy-based authorization?

**Concepts**
- `[Authorize]` on gRPC service class or individual method — same as for controllers
- `UseAuthentication()` and `UseAuthorization()` required before `MapGrpcService<T>()`
- `ClaimsPrincipal` populated by JWT bearer middleware from the gRPC call's `HttpContext`
- `StatusCode.Unauthenticated` and `StatusCode.PermissionDenied` returned on failure

**Answer**

ASP.NET Core's standard authorization middleware works with gRPC services out of the box because gRPC services are part of the same request pipeline. You apply `[Authorize]` at the class or method level on your service implementation, and you configure policies in `Program.cs` just as you would for a controller.

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

You must add `app.UseAuthentication()` and `app.UseAuthorization()` to the middleware pipeline before `app.MapGrpcService<T>()`, just as with controllers. The `[Authorize]` attribute reads claims from the `ClaimsPrincipal` attached to the gRPC call's `HttpContext` — if the call carries a valid JWT, the bearer middleware populates the principal and authorization proceeds normally. Unauthorized calls receive a `StatusCode.Unauthenticated` or `StatusCode.PermissionDenied` status code, which the client sees as an `RpcException`. For service-to-service scenarios, a custom policy can inspect a specific claim such as a `service_name` claim in a client certificate or a service-level JWT scope rather than relying solely on user identity.

---

## Chapter 5: Resiliency — Retry, Hedging & Deadlines

---

## Q15. How do you configure retry policies for a gRPC client in .NET using the built-in gRPC retry support, and how does it differ from wrapping calls with Polly?

**Concepts**
- `ServiceConfig.MethodConfigs.RetryPolicy` as the gRPC-native retry mechanism
- gRPC built-in retry operating inside the transport layer before calls surface to application code
- Polly wrapping at the application layer — more flexible but gRPC-unaware
- Only listing safe status codes in `RetryableStatusCodes` — never `INTERNAL`

**Answer**

.NET's gRPC client supports a built-in retry mechanism configured via `GrpcChannelOptions.ServiceConfig`, which follows the gRPC retry specification and operates inside the gRPC transport layer before the call is returned to application code.

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

gRPC built-in retry is aware of deadlines, the stream state machine, and hedging; it can retry transparent calls — those where no response bytes have been received yet — automatically. Polly wraps at the application layer: each retry creates a new `CallOptions` object and makes a fresh call. Polly is more flexible since you can implement circuit breakers, bulkhead isolation, and fallbacks, but it is gRPC-unaware and will retry non-retryable status codes unless you write the predicate yourself. The built-in retry only retries on status codes you explicitly list in `RetryableStatusCodes` — `UNAVAILABLE` is the safest default. I would never add `INTERNAL` to that list because internal errors often indicate a server-side bug, and retrying does not fix bugs.

---

## Q16. What is hedging in gRPC, how does it differ from retry, and what conditions make it unsafe to use?

**Concepts**
- Hedging — concurrent parallel requests accepting the first successful response
- Retry waiting for failure; hedging overlapping attempts proactively
- `HedgingPolicy` with `MaxAttempts` and optional `HedgingDelay` in `ServiceConfig`
- Idempotency as the non-negotiable prerequisite for safe hedging

**Answer**

Hedging sends multiple copies of the same RPC to the server simultaneously — or with a short delay between them — and returns the first successful response, cancelling the remaining in-flight calls. Retry waits for a call to fail before sending the next attempt; hedging overlaps attempts proactively, trading additional server load and duplicate processing for reduced tail latency. Hedging is configured via `HedgingPolicy` in `ServiceConfig`, specifying the `MaxAttempts` and an optional `HedgingDelay` — the interval between successive hedged requests. Hedging is only safe for idempotent operations: reading data, checking a status, or any operation that can safely execute multiple times without side effects. Hedging a payment charge or a database insert will result in duplicate charges or duplicate rows because both copies of the request may succeed before the second is cancelled. Even for read operations, hedging increases backend load proportionally to `MaxAttempts`, so it should only be used where tail latency reduction is critical such as on low-percentile SLA requirements for hot read paths. The gRPC specification distinguishes transparent retries — safe, no response received — from retried calls requiring explicit configuration and hedging; only transparent retries are done automatically without any policy configuration.

---

## Q17. How does a client deadline interact with a retry policy in a multi-hop service call, and what happens when the deadline expires during a retry sequence?

**Concepts**
- gRPC deadline as an absolute timestamp propagating across the call chain
- Remaining deadline budget smaller than next backoff — retry cancelled, `DEADLINE_EXCEEDED` returned
- Deadline propagation passing `ServerCallContext.Deadline` to outbound calls
- Fixed offset from `DateTime.UtcNow` as the wrong pattern for downstream calls

**Answer**

A gRPC deadline is an absolute timestamp that propagates with the call; it represents the time by which the entire distributed operation must complete, not just a single hop. When a retry policy is active and the remaining deadline budget is smaller than the next backoff interval, the gRPC runtime cancels the retry sequence and returns `DEADLINE_EXCEEDED` to the caller rather than waiting for the backoff to elapse. If Service A sets a three-second deadline and calls Service B, and the first attempt fails after two seconds with `UNAVAILABLE`, there is only one second left before the deadline. If the configured `InitialBackoff` is two seconds, the retry is never attempted — the call returns `DEADLINE_EXCEEDED` immediately, which is the correct behavior since the upstream caller has already received or is about to receive a timeout error. Deadline propagation across services means that when Service A calls Service B and Service B calls Service C, each downstream call should carry the remaining time budget of the original deadline rather than creating a fresh timeout. The implementation is to read `ServerCallContext.Deadline` on the inbound call and construct outbound `CallOptions` with `deadline: inboundContext.Deadline` rather than `deadline: DateTime.UtcNow.AddSeconds(N)`, which would extend the total allowed time beyond what the original caller expects.

---

## Chapter 6: Load Balancing & Service Discovery

---

## Q18. Why does the default Kubernetes `ClusterIP` service not distribute gRPC traffic evenly across pods, and what is the root cause?

**Concepts**
- `ClusterIP` load balancing at TCP connection level via `iptables` — one decision per connection
- gRPC long-lived HTTP/2 connection carrying all RPCs on one TCP connection
- One pod receiving all traffic from one client pod after the first connection is established
- Layer-7 proxy or client-side load balancing needed to distribute HTTP/2 streams

**Answer**

A Kubernetes `ClusterIP` service load balances at the TCP connection level using `iptables` rules: each new TCP connection is directed to a pod chosen by round-robin or random selection. Since gRPC runs over HTTP/2, a single long-lived TCP connection carries all RPC calls as multiplexed streams. Once the gRPC client opens that connection to one pod, all subsequent RPCs go to the same pod — no new TCP connections mean no new load balancing decisions. In REST over HTTP/1.1, each request typically opens a new connection, so `iptables` round-robin distributes traffic relatively evenly over time. In gRPC, the `GrpcClientFactory` deliberately reuses a single channel — and its underlying TCP connection — for efficiency, which is correct behavior for HTTP/2 but means one pod handles all traffic from one client pod while other pods sit idle. This is not a Kubernetes bug — it is the correct consequence of using long-lived HTTP/2 connections with a TCP-level load balancer. The solution requires moving load balancing to the application layer — the gRPC client itself using DNS-based client-side load balancing — or to a Layer-7 proxy such as Envoy or Linkerd that understands HTTP/2 streams.

---

## Q19. What is a headless Kubernetes service, and how does it enable client-side gRPC load balancing?

**Concepts**
- Headless service — `clusterIP: None` returning A records per pod instead of a virtual IP
- DNS query returning multiple A records enabling the client to connect to individual pods
- gRPC DNS resolver distributing calls across all returned IPs via round-robin
- DNS refresh interval as the limitation — new pods not used immediately after scaling

**Answer**

A headless Kubernetes service is defined with `clusterIP: None`, which instructs Kubernetes not to allocate a virtual IP address for the service. Instead, a DNS query for the service returns A records for each individual pod endpoint IP, allowing the client to see and connect to individual pods directly. With a normal `ClusterIP` service, `nslookup order-svc` returns one IP — the virtual IP — and all connections go through `iptables` to one pod. With a headless service, `nslookup order-svc` returns multiple A records, one per ready pod. The gRPC client can be configured with a `DnsResolverFactory` that resolves the headless service name and distributes calls across all returned IPs using round-robin, so each pod gets its own HTTP/2 connection and the client balances new RPCs across those connections. The limitation is that the client must update its endpoint list when pods scale up or down; the gRPC DNS resolver periodically re-resolves the name, but the refresh interval — typically 30 seconds — means new pods are not used immediately after scaling events.

---

## Q20. How do you configure a .NET gRPC client to use DNS-based service discovery with round-robin load balancing across resolved pod addresses?

**Concepts**
- `dns:///` URI scheme activating the gRPC DNS resolver
- `RoundRobinConfig` in `ServiceConfig.LoadBalancingConfigs`
- `PickFirstConfig` as the default always selecting the first address
- Service mesh handling L7 load balancing transparently when available

**Answer**

You configure the `GrpcChannel` with a `dns:///` URI scheme and a `RoundRobinConfig` in the `ServiceConfig`. The channel resolves the DNS name at startup and periodically thereafter, then distributes RPCs across all returned endpoints using round-robin selection.

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

The `dns:///` URI scheme activates the DNS resolver — the triple slash is required since `dns:` is the scheme and `order-svc:5001` is the headless Kubernetes service name. `RoundRobinConfig` in `ServiceConfig.LoadBalancingConfigs` tells the channel to use round-robin balancing across all resolved addresses; the default `PickFirstConfig` always uses the first address, which is equivalent to no load balancing. When integrated with `GrpcClientFactory`, you configure the channel via `ConfigureChannel()` or by providing a `GrpcChannelOptions` factory in `AddGrpcClient`. Service meshes like Istio or Linkerd handle this at the network layer without any client configuration — in a mesh you can use a standard `ClusterIP` service and the sidecar proxy handles L7 load balancing, so client-side load balancing is only needed when a service mesh is not available.

---

## Chapter 7: Health Checks & Observability

---

## Q21. What is the standard gRPC health checking protocol, how do you implement it in an ASP.NET Core service, and how does Kubernetes use it?

**Concepts**
- Standard `grpc.health.v1.Health` service defined in `grpc/health/v1/health.proto`
- `GrpcHealthChecksPublisher` bridging ASP.NET Core `IHealthCheckService` to gRPC health
- Kubernetes 1.24+ `grpc` probe type calling `Health/Check` natively
- Minimal service surface — no additional HTTP health endpoint needed

**Answer**

The gRPC health checking protocol is a standardized gRPC service defined in `grpc/health/v1/health.proto`. It exposes a `Check` unary RPC and a `Watch` server-streaming RPC. The `Check` method accepts a service name — or empty string for overall health — and returns `SERVING`, `NOT_SERVING`, or `SERVICE_UNKNOWN`. In ASP.NET Core, you add the `Grpc.HealthCheck` NuGet package and register the service: `builder.Services.AddGrpcHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy())`, then map it with `app.MapGrpcHealthChecksService()`. The `GrpcHealthChecksPublisher` bridges ASP.NET Core's built-in `IHealthCheckService` with the gRPC health protocol, so existing `IHealthCheck` implementations — database connectivity checks, external dependency checks — automatically report their status through the gRPC health endpoint. In Kubernetes 1.24+, you configure a `grpc` liveness or readiness probe pointing to the service's port; the kubelet calls the `grpc.health.v1.Health/Check` RPC directly and considers the pod healthy only when the response is `SERVING`. Using gRPC health probes avoids the need to expose a separate HTTP health endpoint, keeping the service's surface area minimal.

---

## Q22. How does OpenTelemetry instrument gRPC calls in .NET, and what trace attributes does it attach to client and server spans?

**Concepts**
- `OpenTelemetry.Instrumentation.GrpcNetClient` for client spans
- `AddAspNetCoreInstrumentation()` covering gRPC server spans
- `rpc.system`, `rpc.service`, `rpc.method`, `rpc.grpc.status_code` as standard attributes
- `traceparent` propagated in gRPC metadata automatically

**Answer**

OpenTelemetry for .NET instruments gRPC calls automatically when you add `OpenTelemetry.Instrumentation.GrpcNetClient` for client spans and `OpenTelemetry.Instrumentation.AspNetCore` for server spans. Each outbound RPC and each inbound RPC handler creates a span that participates in the distributed trace, propagating the `traceparent` W3C header through gRPC metadata. Client spans carry: `rpc.system = "grpc"`, `rpc.service` with the proto service name, `rpc.method` with the proto method name, `rpc.grpc.status_code` with the numeric gRPC status code on completion, and `net.peer.name` and `net.peer.port`. Server spans carry the same `rpc.*` attributes plus `net.host.name` and `net.host.port`. The trace context is propagated in gRPC metadata as `traceparent` and `tracestate` headers; the OTel ASP.NET Core instrumentation reads these on the server side and creates child spans automatically — no manual context propagation code is needed. To enable: call `tracerProviderBuilder.AddGrpcClientInstrumentation()` in the OTel setup; on the server, `AddAspNetCoreInstrumentation()` covers both HTTP and gRPC handlers. Without OTel, debugging a failed cross-service gRPC call requires grepping logs from multiple services by timestamp; with OTel you search by `trace_id` and see the full call chain as a single waterfall view in Jaeger, Zipkin, or Azure Monitor.

---

## Q23. How do you implement a server-side gRPC interceptor to emit structured log entries for every request and response, and how does it differ from ASP.NET Core middleware?

**Concepts**
- Server interceptor operating on typed gRPC request and response objects
- ASP.NET Core middleware operating on raw `HttpContext` bytes and headers
- `ServerCallContext` providing method name, deadline, peer address, and metadata
- Register with `builder.Services.AddGrpc(o => o.Interceptors.Add<T>())`

**Answer**

A gRPC server interceptor inherits from `Grpc.Core.Interceptors.Interceptor` and overrides the handler methods to inject behavior before and after each RPC. Unlike ASP.NET Core middleware, which operates on `HttpContext` and sees only raw HTTP request and response bytes, an interceptor operates on the typed gRPC request and response objects and has access to `ServerCallContext` including the method name, deadline, peer address, and metadata.

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

Register it at startup with `builder.Services.AddGrpc(o => o.Interceptors.Add<LoggingInterceptor>())`. Interceptors operate after the gRPC framing layer decodes the message but before the service method is invoked, giving you access to strongly typed request objects rather than raw bytes. ASP.NET Core middleware can also intercept gRPC calls at the HTTP level — it sees headers and status codes, not proto message content. The right split is to use middleware for HTTP-level concerns such as rate limiting and correlation ID injection, and interceptors for gRPC-semantic concerns such as logging method names and mapping exceptions to status codes.

---

## Chapter 8: gRPC in Containers & Kubernetes

---

## Q24. What Kestrel and environment configuration changes are required when running a gRPC service in a Docker container without TLS (plain HTTP/2)?

**Concepts**
- `Http2UnencryptedSupport` AppContext switch lifting runtime refusal of H2C
- `HttpProtocols.Http2` on the Kestrel endpoint for a gRPC-only port
- `http://` client address for plain HTTP/2 connections
- Service mesh sidecar handling mTLS between pods while the app uses plain H2C internally

**Answer**

By default, Kestrel negotiates HTTP/2 only over TLS using ALPN. To run plain HTTP/2 — also called H2C — inside a Docker container or Kubernetes pod where TLS is handled by the service mesh or ingress, you must explicitly configure Kestrel to accept HTTP/2 without TLS and enable the corresponding .NET switch.

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

`Http2UnencryptedSupport` must be set to `true` before the app starts; it lifts the .NET runtime's default refusal to speak H2C, which exists to prevent accidental cleartext use in non-containerized environments. Setting `HttpProtocols.Http2` on the Kestrel endpoint tells Kestrel not to negotiate HTTP/1.1 on that port — the port speaks only HTTP/2, which is what gRPC clients expect. In Docker Compose or a Kubernetes pod spec, expose port 5001 and configure the gRPC client's address as `http://` rather than `https://`. In a service mesh, the sidecar proxy intercepts all traffic and upgrades connections to mTLS transparently; the application communicates in plain H2C to the sidecar on `localhost`, which is secure because the traffic never leaves the pod boundary unencrypted.

---

## Q25. What is the `grpc` probe type in Kubernetes 1.24+, and how does it differ from a plain HTTP liveness probe for a gRPC-only service?

**Concepts**
- `grpc` probe type — kubelet calling `grpc.health.v1.Health/Check` natively since 1.24
- Eliminating the need for `grpc_health_probe` binary in the container image
- `service` field mapping to `HealthCheckRequest.service` in the proto message
- Requires the service to implement the standard `grpc.health.v1.Health` protocol

**Answer**

The `grpc` probe type, introduced in Kubernetes 1.24 as a stable feature, allows the kubelet to perform a native gRPC `Health/Check` RPC directly against the pod's gRPC port, without requiring an additional HTTP health endpoint or a sidecar `grpc_health_probe` binary. The kubelet connects to the specified port and calls the standard `grpc.health.v1.Health/Check` method; a `SERVING` response means healthy, and anything else or a connection error means unhealthy.

```yaml
livenessProbe:
  grpc:
    port: 5001
    service: ""        # empty string checks overall service health
  initialDelaySeconds: 5
  periodSeconds: 10
```

Before Kubernetes 1.24, the only native probe types were `httpGet`, `tcpSocket`, and `exec`. To health-check a gRPC-only service, teams either added an HTTP health endpoint — mixing protocols — ran `grpc_health_probe` as an `exec` probe requiring the binary in the container image, or used a TCP probe that only checks if the port is open rather than if the service is functioning. The `grpc` probe type eliminates all three workarounds. The `service` field maps to the `service` field in the `HealthCheckRequest` proto message — an empty string requests the overall health of the server, while a service name like `"grpc.orders.OrderService"` can return the health of that specific named service if the implementation registers per-service health status. The probe requires that the gRPC service implement the standard health checking protocol, which in ASP.NET Core is provided by the `Grpc.HealthCheck` package registered via `AddGrpcHealthChecks()`.

---
