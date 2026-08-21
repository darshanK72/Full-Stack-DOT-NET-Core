# Karat — Interview Questions

> **Folder:** `07. ASP.Net Core Web API/16. gRPC Web APIs`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)  
> **Cross-ref:** [07. ASP.Net Core Web API/01. Introduction to REST & Web API](../01.%20Introduction%20to%20REST%20&%20Web%20API/.gitkeep) (REST semantics — contrast with RPC model)

---

#### Q1. (D) A fintech team must expose an **order status** API to (a) a React SPA in the browser, (b) an internal .NET microservice, and (c) a partner's legacy HTTP JSON client. They propose gRPC for all three. What would you recommend per consumer, and why?

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

#### Q6. (P) Map common failure scenarios to **gRPC status codes** vs **HTTP Problem Details** for a BFF that exposes REST externally and calls gRPC internally: not found, invalid argument, deadline exceeded, permission denied, upstream unavailable. What must the BFF translate, and what should never leak to the browser?

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

#### Q8. (M) Explain **deadline propagation** from a REST gateway through a gRPC client to a downstream gRPC service. What breaks if the gateway sets a 30-second HTTP timeout but the gRPC client uses `CallOptions` without `deadline`, and how do you wire `CancellationToken` from ASP.NET Core into `GrpcChannel` calls?

---
