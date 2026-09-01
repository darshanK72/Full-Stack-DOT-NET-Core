# WebSockets & Real-Time Transport — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What are WebSockets, and how do they differ from regular HTTP requests?](#q1-what-are-websockets-and-how-do-they-differ-from-regular-http-requests)
2. [Q2. How do you enable WebSockets in ASP.NET Core?](#q2-how-do-you-enable-websockets-in-aspnet-core)
3. [Q3. Where must `UseWebSockets()` be placed in the middleware pipeline?](#q3-where-must-usewebsockets-be-placed-in-the-middleware-pipeline)
4. [Q4. What happens during a WebSocket upgrade request?](#q4-what-happens-during-a-websocket-upgrade-request)
5. [Q5. What server resources are consumed by an open WebSocket connection?](#q5-what-server-resources-are-consumed-by-an-open-websocket-connection)
6. [Q6. What is SignalR?](#q6-what-is-signalr)
7. [Q7. What is the difference between SignalR and raw WebSockets?](#q7-what-is-the-difference-between-signalr-and-raw-websockets)
8. [Q8. When would you choose SignalR over raw WebSockets?](#q8-when-would-you-choose-signalr-over-raw-websockets)
9. [Q9. What is a SignalR backplane, and why is it needed?](#q9-what-is-a-signalr-backplane-and-why-is-it-needed)
10. [Q10. How do you scale WebSocket/SignalR applications across multiple server instances?](#q10-how-do-you-scale-websocketsignalr-applications-across-multiple-server-instances)
11. [Q11. How is authentication handled for WebSocket connections?](#q11-how-is-authentication-handled-for-websocket-connections)
12. [Q12. What are WebSocket message size limits in ASP.NET Core/Kestrel?](#q12-what-are-websocket-message-size-limits-in-aspnet-corekestrel)
13. [Q13. What is WebSocket backpressure, and why does it matter for broadcasts?](#q13-what-is-websocket-backpressure-and-why-does-it-matter-for-broadcasts)
14. [Q14. How do you detect and clean up stale WebSocket connections?](#q14-how-do-you-detect-and-clean-up-stale-websocket-connections)
15. [Q15. What is the difference between WebSocket and Server-Sent Events (SSE)?](#q15-what-is-the-difference-between-websocket-and-server-sent-events-sse)
16. [Q16. What is long polling, and how does it compare to WebSockets?](#q16-what-is-long-polling-and-how-does-it-compare-to-websockets)
17. [Q17. What is a SignalR Hub?](#q17-what-is-a-signalr-hub)
18. [Q18. What security risks exist when clients self-identify via the first WebSocket message?](#q18-what-security-risks-exist-when-clients-self-identify-via-the-first-websocket-message)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What are WebSockets, and how do they differ from regular HTTP requests?

What are WebSockets, and how do they differ from regular HTTP requests?

**Answer:** WebSockets provide a full-duplex, persistent connection between client and server after an initial HTTP upgrade handshake, allowing both sides to send messages at any time without the request-response overhead of standard HTTP.

- Regular HTTP is stateless and typically one request yields one response, then the connection may close; WebSockets keep the TCP connection open for bidirectional framed messages.
- The upgrade begins as HTTP GET with `Connection: Upgrade` and `Upgrade: websocket` headers — once accepted, the protocol switches from HTTP to the WebSocket framing protocol.
- WebSockets suit live dashboards, chat, gaming, and tick feeds where server push latency matters.
- They consume server resources for the connection duration — unlike short HTTP requests that release resources immediately after the response.

---

## Q2. How do you enable WebSockets in ASP.NET Core?

How do you enable WebSockets in ASP.NET Core?

**Answer:** Call `app.UseWebSockets()` (optionally with `WebSocketOptions`) in the middleware pipeline and handle upgrade requests in an endpoint that checks `context.WebSockets.IsWebSocketRequest` before calling `AcceptWebSocketAsync`.

```csharp
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

app.Map("/ws", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }
    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    // receive/send loop
});
```

- `UseWebSockets` adds middleware that detects upgrade requests and enables the WebSocket subsystem in Kestrel.
- Without this middleware, upgrade attempts fail or behave as normal HTTP requests.
- SignalR enables WebSockets internally when you call `AddSignalR()` and map hubs — raw WebSocket use requires explicit middleware and handler code.

---

## Q3. Where must `UseWebSockets()` be placed in the middleware pipeline?

Where must `UseWebSockets()` be placed in the middleware pipeline?

**Answer:** Place `UseWebSockets()` early in the pipeline — after exception handling and forwarded headers, but before the branch that handles the upgrade and before terminal middleware that would short-circuit the request.

- Authentication and authorization for the upgrade request must run before `AcceptWebSocketAsync` because WebSocket messages after upgrade no longer pass through standard HTTP middleware on each frame.
- If placed after a terminal middleware or missing entirely, upgrade requests return 404 or fail to switch protocols.
- Behind a reverse proxy, ensure the proxy forwards `Upgrade` and `Connection` headers and that path-base configuration matches the mapped WebSocket route.
- SignalR's `MapHub` still requires `UseWebSockets()` (or equivalent) in the pipeline for the WebSocket transport.

---

## Q4. What happens during a WebSocket upgrade request?

What happens during a WebSocket upgrade request?

**Answer:** The client sends an HTTP GET with `Connection: Upgrade`, `Upgrade: websocket`, `Sec-WebSocket-Key`, and related headers; the server validates the request, responds with `101 Switching Protocols` and a computed `Sec-WebSocket-Accept` value, and the connection becomes a WebSocket with framed bidirectional messaging.

- Until the server accepts, the request is normal HTTP — cookies, JWT bearer tokens, and authorization policies apply at this stage.
- After `AcceptWebSocketAsync`, further communication uses WebSocket frames (`ReceiveAsync`/`SendAsync`), not HTTP request/response pairs.
- Failed upgrades return HTTP error status codes (400, 401, 404) before any protocol switch occurs.
- Load balancers must support connection upgrade and often require sticky sessions or shared backplane for subsequent message routing in multi-instance setups.

---

## Q5. What server resources are consumed by an open WebSocket connection?

What server resources are consumed by an open WebSocket connection?

**Answer:** Each open WebSocket holds a TCP connection, a `WebSocket` object, send/receive buffers, and any application-level registry state (connection dictionaries, group memberships) until the client closes or the server terminates the connection.

- Thousands of idle tabs multiply memory for buffers and tracking structures — unbounded static dictionaries of connections are a common memory leak.
- Thread pool continuations from `ReceiveAsync`/`SendAsync` add CPU overhead under high message rates.
- Slow clients that cannot read fast enough accumulate outbound queues unless the server applies backpressure or drops them.
- Monitor active connection count, bytes in/out, and process memory — alert when connections grow without matching active user sessions.

---

## Q6. What is SignalR?

What is SignalR?

**Answer:** SignalR is an ASP.NET Core library that provides a high-level real-time messaging abstraction over WebSockets, Server-Sent Events, and long polling, with hubs, connection IDs, groups, and automatic client reconnection support.

- Developers define hub classes with methods callable from clients and server push methods such as `Clients.Group("room").SendAsync(...)`.
- SignalR negotiates the best available transport — WebSockets when supported, falling back to SSE or long polling through firewalls and proxies.
- It integrates with authentication (`[Authorize]` on hubs), dependency injection, and scale-out backplanes (Redis, Azure Service Bus).
- SignalR is the default choice for ASP.NET Core real-time features unless you need a fully custom binary protocol.

---

## Q7. What is the difference between SignalR and raw WebSockets?

What is the difference between SignalR and raw WebSockets?

**Answer:** Raw WebSockets give you a low-level framed connection where you define message format, routing, reconnection, and scale-out yourself; SignalR provides hubs, groups, connection management, transport fallback, and built-in scale-out hooks on top of WebSockets or alternate transports.

| Aspect | Raw WebSocket | SignalR |
|---|---|---|
| Protocol | You define JSON/binary framing | Hub methods, JSON/MessagePack |
| Reconnection | Manual backoff and resubscribe | Client SDK auto-reconnect |
| Scale-out | Custom pub/sub + registry | Redis/Azure backplane built-in |
| Fallback transports | WebSocket only | WebSocket, SSE, long polling |
| Auth | Manual at upgrade time | `[Authorize]`, JWT on negotiate |

- Raw WebSockets fit custom protocols, non-.NET clients with strict wire formats, or minimal overhead binary streams.
- SignalR fits typical notify/broadcast scenarios (order status, chat, live dashboards) with faster team delivery.

---

## Q8. When would you choose SignalR over raw WebSockets?

When would you choose SignalR over raw WebSockets?

**Answer:** Choose SignalR when you need group broadcast, automatic transport fallback, client reconnection, and multi-instance scale-out without building that infrastructure yourself — typical business real-time notifications are the sweet spot.

- Order status updates to browser and mobile clients mapped to user groups (`Clients.User(id)`) are simpler with SignalR than maintaining per-connection dictionaries manually.
- Environments where WebSockets are blocked by proxies benefit from SignalR's SSE/long-polling fallback without separate client code paths.
- Teams without dedicated real-time protocol expertise ship faster with hub-based APIs and official JavaScript/.NET clients.
- Choose raw WebSockets when SignalR's overhead, negotiate handshake, or opinionated hub model does not fit (embedded devices, third-party binary protocols, extreme latency tuning).

---

## Q9. What is a SignalR backplane, and why is it needed?

What is a SignalR backplane, and why is it needed?

**Answer:** A SignalR backplane is a shared pub/sub message bus (Redis, Azure Service Bus, etc.) that synchronizes hub messages across all server instances so a broadcast from one node reaches clients connected to other nodes.

- Without a backplane, each instance only knows about its local connections — `Clients.All.SendAsync` on instance A does not reach sockets on instance B.
- Register with `AddSignalR().AddStackExchangeRedis(connectionString, options => ...)` or the Azure SignalR Service integration.
- Sticky sessions alone keep one client on one node but do not solve cross-instance fan-out when events originate on arbitrary nodes.
- Azure SignalR Service is a managed alternative that offloads connection management and scaling entirely from your web servers.

---

## Q10. How do you scale WebSocket/SignalR applications across multiple server instances?

How do you scale WebSocket/SignalR applications across multiple server instances?

**Answer:** Combine a SignalR backplane or Azure SignalR Service for message fan-out, enforce authentication at connection time, configure proxy WebSocket timeouts, and optionally use sticky sessions for connection affinity while relying on the backplane for cross-node broadcasts.

- Redis backplane: all instances subscribe to a channel prefix; hub messages publish once and every node delivers to its local connections.
- Raw WebSocket scale-out requires a custom connection registry in Redis and pub/sub routing — each instance subscribes and forwards to local sockets.
- Configure nginx/ALB idle timeouts longer than your heartbeat interval to prevent proxy-side disconnects.
- Load-test connection count and broadcast fan-out separately — 2,000 idle connections behave differently from broadcasting to 2,000 clients every second.

---

## Q11. How is authentication handled for WebSocket connections?

How is authentication handled for WebSocket connections?

**Answer:** Authentication occurs during the HTTP upgrade request before the protocol switches — validate JWT bearer tokens, cookies, or API keys in middleware or endpoint authorization, because individual WebSocket frames do not re-run the full HTTP auth pipeline.

- For JWT, browsers often pass the token as a query parameter or `Authorization` header on the upgrade GET because WebSocket API header support varies.
- SignalR's negotiate endpoint accepts `[Authorize]` and standard authentication handlers before establishing the transport.
- After upgrade, derive user identity from `context.User` claims established at upgrade — never trust a client-sent user ID in the first WebSocket message.
- Anonymous upgrades should be rejected explicitly (`401`) before `AcceptWebSocketAsync` for protected resources.

---

## Q12. What are WebSocket message size limits in ASP.NET Core/Kestrel?

What are WebSocket message size limits in ASP.NET Core/Kestrel?

**Answer:** Kestrel limits the initial HTTP upgrade request body via `MaxRequestBodySize`, but per-message limits for WebSocket frames require application-level enforcement because `ReceiveAsync` returns one frame chunk at a time and large logical messages span multiple frames.

- Default receive buffers are often 4 KB per call — reassemble with a `MemoryStream` until `EndOfMessage` is true, counting total bytes against a cap.
- SignalR exposes `MaximumReceiveMessageSize` in hub options; raw handlers must implement equivalent guards and close with status `1009` (Message Too Big) when exceeded.
- Without a reassembly cap, a malicious client sending infinite partial frames causes out-of-memory failures.
- Prefer HTTP upload endpoints for large blobs; use WebSockets for notifications and small control messages.

---

## Q13. What is WebSocket backpressure, and why does it matter for broadcasts?

What is WebSocket backpressure, and why does it matter for broadcasts?

**Answer:** Backpressure occurs when a producer sends messages faster than a slow consumer can read them, causing outbound queues to grow — in naive broadcast loops that `await SendAsync` sequentially to every client, one slow peer blocks delivery to all others (head-of-line blocking).

- Serialize the payload once and fan out with bounded parallelism or per-client outbound queues instead of awaiting every send inside one client's receive loop.
- Remove dead sockets from registries when `WebSocketState` is not `Open` or when sends throw — stale entries amplify blocking.
- SignalR handles much of this internally; raw WebSocket broadcast code needs explicit queue caps and drop policies for slow clients.
- Decouple inbound messages (publish to a channel/bus) from outbound fan-out so one client's read loop does not drive global broadcast timing.

---

## Q14. How do you detect and clean up stale WebSocket connections?

How do you detect and clean up stale WebSocket connections?

**Answer:** Use protocol-level keep-alives plus application heartbeats, enforce idle timeouts, and always remove connections from registries in a `finally` block when the receive loop exits or the token is cancelled.

- Set `WebSocketOptions.KeepAliveInterval` so Kestrel sends control frames — this helps but may not traverse all proxies without application-level pings.
- Send periodic heartbeat messages and close the connection if no response arrives within the configured interval.
- Link `CancellationToken` to `HttpContext.RequestAborted` and host shutdown so deploys do not leave ghost entries in static dictionaries.
- Cap connections per authenticated user at accept time to prevent one account from opening unbounded tabs and exhausting server memory.

---

## Q15. What is the difference between WebSocket and Server-Sent Events (SSE)?

What is the difference between WebSocket and Server-Sent Events (SSE)?

**Answer:** WebSockets are bidirectional — either side can send at any time — while Server-Sent Events (SSE) are a one-way HTTP-based stream from server to client over a long-lived `text/event-stream` response.

- SSE works over standard HTTP/1.1 or HTTP/2 without an upgrade handshake, traversing some proxies and firewalls more easily than WebSockets.
- SSE is suitable for live feeds, progress updates, and notifications that only need server push; client-to-server updates still use regular HTTP requests.
- WebSockets fit chat, collaborative editing, and gaming where low-latency client messages are frequent.
- SignalR can fall back to SSE automatically when WebSockets are unavailable, hiding transport details from application code.

---

## Q16. What is long polling, and how does it compare to WebSockets?

What is long polling, and how does it compare to WebSockets?

**Answer:** Long polling is a technique where the client sends repeated HTTP requests and the server holds each request open until new data arrives or a timeout occurs, then the client immediately opens another request — it simulates push over plain HTTP.

- It has higher latency and overhead than WebSockets because each message cycle may require new HTTP headers and connection setup.
- It works everywhere HTTP works, including restrictive proxies — SignalR uses it as a last-resort fallback transport.
- WebSockets maintain one persistent connection with lower per-message overhead after the upgrade.
- Long polling consumes server threads or async waits per waiting client — at scale it is less efficient than WebSockets or SSE for continuous streams.

---

## Q17. What is a SignalR Hub?

What is a SignalR Hub?

**Answer:** A SignalR Hub is a server-side class that defines methods clients can invoke and provides `Clients`, `Groups`, and `Context` properties for pushing messages to connected clients, groups, or specific connection IDs.

```csharp
public class OrderHub : Hub
{
    public async Task JoinOrderGroup(string orderId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");

    public async Task OrderStatusChanged(string orderId, string status) =>
        await Clients.Group($"order-{orderId}").SendAsync("StatusUpdated", status);
}
```

- Map with `app.MapHub<OrderHub>("/hubs/orders");` — clients connect via the SignalR JavaScript or .NET client SDK.
- Hub methods run in the context of a connected client; server-side code injects services via constructor DI.
- `[Authorize]` on the hub class or methods restricts who can connect and invoke operations.
- Hubs abstract connection lifetime — the framework tracks connection IDs and group membership across reconnections when designed with durable user identifiers.

---

## Q18. What security risks exist when clients self-identify via the first WebSocket message?

What security risks exist when clients self-identify via the first WebSocket message?

**Answer:** If the server accepts a client-supplied user ID or tenant ID in the first WebSocket frame instead of binding identity from authenticated claims on the upgrade request, any anonymous connection can impersonate another user and subscribe to their private channels.

- WebSocket auth must be established at HTTP upgrade time — trusting post-upgrade JSON payloads is equivalent to skipping authentication on REST endpoints.
- Attackers connect to `/ws/orders`, send `{ "userId": "victim-guid" }`, and receive events intended for the victim.
- Derive identity from `context.User.FindFirst(ClaimTypes.NameIdentifier)` after validating JWT or cookies during upgrade.
- Combine authenticated upgrades with authorization checks on group subscription — users should only join groups matching their claims.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Answer:** In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata — registering auth before routing breaks endpoint-aware authorization and policy resolution.

- The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints (`MapControllers` / `MapGet`).
- When auth runs before routing, the endpoint has not been selected yet and `[Authorize]` metadata on minimal routes or controllers may not apply correctly.
- Symptoms include anonymous access to protected endpoints or 401 responses without proper challenge behavior.
- Always verify middleware order in `Program.cs` during code review for new services.

---

#### Gotcha 2. Scoped service in a Singleton

**Answer:** Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`.

- The singleton holds one scoped instance forever instead of one per request — EF change trackers accumulate unrelated entities.
- Enable `ValidateScopes` in Development/staging to catch illegal scope combinations at startup.
- Fix by injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation.
- This applies equally to singleton services, hosted services, and cached delegates in Minimal APIs.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Answer:** Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected.

- `HttpClient` is disposable but not meant for per-use disposal — `using var client = new HttpClient()` in a singleton is an anti-pattern.
- `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly.
- Register named or typed clients: `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>();`
- Symptoms include `SocketException` and timeout errors only under production traffic, not in local testing.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Answer:** `IOptions<T>` captures configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled.

- `IOptionsSnapshot<T>` recalculates per request scope; `IOptionsMonitor<T>` supports change notifications via `OnChange`.
- Singleton services must use `IOptionsMonitor<T>` or read options inside scoped operations if they need live updates.
- Misconfiguration persists silently until process restart when `.Value` was cached at construction.
- See Chapter 05 for the full options lifetime comparison.

---

#### Gotcha 5. GET with `[FromBody]`

**Answer:** Using `[FromBody]` on GET action parameters or minimal API handlers is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies, so binding fails silently in production.

- Query strings and route values are the correct binding sources for GET requests.
- Complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys.
- Failures often appear only in specific browsers or CDN layers, not in Swagger "Try it out" during development.
- REST conventions expect GET to be safe and idempotent with parameters in the URL.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Answer:** ASP.NET Core 8 Web API serializes JSON with camelCase property names by default via `JsonNamingPolicy.CamelCase`, so incoming JSON with PascalCase keys (for example `"CustomerName"`) may not bind to `CustomerName` unless case-insensitive matching is enabled.

- Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values (empty string, zero).
- Prefer standardizing clients on camelCase and documenting the contract in OpenAPI.
- Optional mitigation: `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` — but explicit camelCase contracts are cleaner.
- Add validation attributes so silent binding failures become 400 responses instead of corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Answer:** Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown.

- Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis.
- Always use `throw;` when rethrowing after logging or cleanup in a catch block.
- Wrap in a new exception only when adding context: `throw new OrderProcessingException("...", ex)` to preserve `InnerException`.
- This trap appears in both application code and background worker error handlers.

---

#### Gotcha 8. Kestrel as the only production layer

**Answer:** Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching that production deployments typically require.

- Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front.
- TLS certificates are easier to manage at the proxy layer with automatic renewal.
- Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy.
- Containers often bind Kestrel to port 8080 internally while the ingress controller handles HTTPS externally.

---

#### Gotcha 9. `launchSettings.json` in production

**Answer:** Settings in `Properties/launchSettings.json` — including `applicationUrl`, environment variables, and launch profiles — apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts.

- Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings.
- Assuming `launchSettings.json` sets Production behavior leads to wrong environment or binding in deployed environments.
- The file is development ergonomics, not runtime configuration.
- Use `appsettings.Production.json` and host-level env vars for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Answer:** A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics.

- PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent.
- Marketing consent and feature flags are common domains where this bug causes compliance or logic errors.
- Create DTOs may use non-nullable bool when explicit values are always required on insert.
- Document nullable fields in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Answer:** Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs.

- Call `UseForwardedHeaders()` early, before middleware that reads scheme or host (HTTPS redirection, link generation, rate limiting by IP).
- Configure `ForwardedHeadersOptions` to trust only your reverse proxy network — trusting all proxies enables header spoofing.
- Headers include `X-Forwarded-For`, `X-Forwarded-Proto`, and `X-Forwarded-Host`.
- Local development without a proxy does not need this; production behind nginx/IIS/ALB does.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Answer:** Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default — placing secrets, `.env`, backup configs, or private keys there exposes them over HTTP.

- Only public assets (CSS, JS, images, public PDFs) belong in `wwwroot`.
- Sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers.
- Accidental copy of `appsettings.Production.json` into `wwwroot` is a critical security incident.
- Use build pipelines to verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Answer:** SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers.

- Map API routes (`MapControllers`, minimal API groups) before `MapFallbackToFile("index.html")`.
- Scope fallback to non-API paths or use conditional fallback that excludes `/api` prefixes.
- Symptoms include CORS errors masked as HTML responses and Swagger fetch failures in production SPA hosting.
- Order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Answer:** A singleton `BackgroundService` that injects scoped services (`DbContext`, repositories) directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration.

- Hosted services live for the application lifetime — scoped dependencies must not be constructor-injected.
- Inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes.
- Same rule applies to timers and `Task.Run` loops started from singletons.
- Enable `ValidateScopes` to catch this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Answer:** SignalR broadcasts from one server instance reach only clients connected to that instance — without a Redis or Azure Service Bus backplane (or Azure SignalR Service), users on different nodes never receive each other's real-time events.

- Sticky sessions keep one client on one node but do not route events raised on other nodes to that client.
- Register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application.
- Raw WebSocket apps need equivalent custom pub/sub — SignalR's backplane is the built-in solution.
- Test scale-out with at least two instances before launch, not single-node staging alone.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

## Gotchas — ASP.NET Core (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (M) A raw WebSocket endpoint returns 404 on upgrade in production but works locally. Review middleware order — where must `UseWebSockets()` sit relative to routing, authentication, and terminal middleware?

---

**Answer:**

**Answer:** `UseWebSockets()` must run before the branch that handles the upgrade (typically before `UseRouting`/`Map`), and authentication/authorization must execute on the upgrade request before `AcceptWebSocketAsync` — placing WebSocket handling after a terminal middleware or missing `UseWebSockets()` causes failed upgrades or anonymous connections.

- Call `app.UseWebSockets(new WebSocketOptions { ... })` early — after exception handling and forwarded headers, before endpoints that accept upgrades.
- The upgrade is an HTTP GET with `Connection: Upgrade` — JWT bearer, cookies, or API keys must be validated **before** switching protocols; middleware after a short-circuiting branch may never run.
- `Map("/ws", handler)` or minimal API `MapGet` with WebSocket check must be registered with routing active — 404 often means path mismatch behind path-base or missing Map branch.
- Behind nginx/ALB, ensure proxy passes `Upgrade` and `Connection` headers and idle timeouts exceed ping interval.
- Do not call `UseAuthorization()` only on controllers if WebSocket path bypasses endpoint metadata — apply auth in the WebSocket delegate or use `[Authorize]` on SignalR hubs.

**Production takeaway:** WebSocket failures are often middleware-order or proxy-header issues, not socket API bugs — trace the HTTP upgrade request first.

---

---

#### Q2. (P) A dashboard opens a WebSocket per browser tab and keeps it open for hours. What server-side resources are tied to connection lifetime, and how do you detect stale connections and prevent unbounded memory growth?

---

**Answer:**

**Answer:** Each open WebSocket holds a connection object, receive/send buffers, and any registered server-side state (hub groups, static dictionaries) until close — long-lived tabs multiply memory and threadpool continuations, so apps must heartbeat, enforce idle timeout, and cap connections per user.

- Resources: TCP connection, `WebSocket` instance, per-connection buffers, application registry entries (`ConcurrentDictionary`), and any queued outbound messages for slow clients.
- Detect stale: protocol-level **ping/pong** (WebSocket keep-alive) or application heartbeat messages; close with `WebSocketCloseStatus` if no pong within interval.
- Configure `WebSocketOptions.KeepAliveInterval` so Kestrel sends control frames — does not replace app-level heartbeats through proxies.
- Enforce max duration and idle timeout in handler loop; remove from registry in `finally` on disconnect.
- Cap tabs per user at connection time using authenticated identity — reject excess with 429 or close code.
- Monitor connection count, bytes in/out, and GC pressure — alert on growth without corresponding user sessions.

**Production takeaway:** Treat WebSocket connections like leased server resources, not free persistent HTTP — unbounded registries are memory leaks with a longer half-life.

---

---

#### Q3. (D) Product needs live order-status updates to web and mobile clients. Compare **SignalR** vs **raw WebSocket** for this scenario — protocol, reconnection, scale-out, and team velocity.

---

**Answer:**

**Answer:** SignalR is the default for ASP.NET Core fan-out with automatic negotiate/fallback, connection ids, groups, and Redis backplane support; raw WebSockets fit custom binary protocols or non-.NET clients when you will own reconnection, heartbeats, and multi-instance routing yourself.

| Factor | SignalR | Raw WebSocket |
|---|---|---|
| Protocol | Hub abstraction; JSON/MessagePack; SSE/long-polling fallback | You define message framing and errors |
| Reconnection | Client SDK reconnect + stateful connection id (with server design) | Manual backoff, resubscribe, replay cursors |
| Scale-out | Built-in backplane (Redis, Azure Service Bus) | Custom pub/sub + connection registry |
| Auth | `[Authorize]` on hubs; JWT via query/header on negotiate | Manual on HTTP upgrade only |
| Velocity | Faster delivery for typical CRUD + notify | Lower framework help; more code |

- Choose SignalR when broadcasting order events to groups (`user-{id}`) across browsers and mobile with minimal plumbing.
- Choose raw WebSocket for ultra-low overhead binary streams, third-party hardware, or strict non-SignalR contracts.
- Hybrid: SignalR for web dashboard, push notifications via APNS/FCM for mobile background — don't force one transport everywhere.

**Production takeaway:** Karat tests judgment — SignalR is not "heavier" in the wrong sense; it buys operational features raw sockets push to your team.

---

---

#### Q4. (P) You deploy three API instances behind a load balancer. WebSocket clients connected to instance A never receive events raised on instance B. Preview how a **SignalR backplane** (Redis/Azure Service Bus) solves this and what still breaks if you use raw WebSockets without shared state.

---

**Answer:**

**Answer:** SignalR backplane publishes hub messages to all instances so each server forwards to its local connections — without shared pub/sub, raw WebSocket registries are process-local and events on instance B never reach sockets on instance A.

- Register: `builder.Services.AddSignalR().AddStackExchangeRedis(connectionString, o => o.Configuration.ChannelPrefix = "orders:");`
- When code calls `Clients.Group("user-42").SendAsync(...)`, the message goes to Redis pub/sub; all nodes receive and deliver to local sockets in that group.
- Sticky sessions alone do not fix **cross-instance fan-out** — they only keep one client on one node; events originating elsewhere still miss subscribers.
- Raw WebSocket fix: external broker (Redis, NATS) + subscribe per instance; maintain `connectionId → serverId` in Redis; route publishes to correct node or broadcast to all nodes.
- Still breaks without: group membership sync, connection lifetime cleanup on crash, message ordering guarantees, and backpressure on slow consumers.
- Azure SignalR Service offloads connection management entirely — alternative to self-hosted backplane at scale.

**Production takeaway:** Scale-out real-time is a distributed systems problem — backplane or Azure SignalR is the ASP.NET answer; sticky cookies are insufficient.

---

---

#### Q5. (R) Review this WebSocket endpoint. Anonymous clients can connect and impersonate any user id sent in the first message. What is wrong?

```csharp
app.UseWebSockets();

app.Map("/ws/orders", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = new byte[4 * 1024];

    while (socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Text)
        {
            var msg = JsonSerializer.Deserialize<OrderSubscribe>(buffer.AsSpan(0, result.Count));
            _hub.Register(msg!.UserId, socket); // static registry
            await SendOpenOrdersAsync(socket, msg.UserId);
        }
    }
});
```

*(Authentication middleware is registered globally but JWT is only sent on the initial HTTP upgrade request.)*

---

**Answer:**

**Answer:** The handler trusts client-supplied `UserId` in the first JSON message instead of binding identity from authenticated claims on the upgrade request, registers sockets in a static dictionary without cleanup or authz, and uses `CancellationToken.None` — allowing impersonation and resource leaks.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | No auth check before `AcceptWebSocketAsync` | Anyone can open `/ws/orders` |
| Security | `UserId` from client payload, not `context.User` | Subscribe to any user's orders |
| Design | Static `_hub.Register` without disconnect cleanup | Memory leak; ghost subscriptions |
| Reliability | `CancellationToken.None` on receive | Cannot honor shutdown; hung connections |
| Scale-out | Static in-process registry | Connections on other instances invisible |

**Fix (priority order):**

1. Require authentication on upgrade: `[Authorize]` equivalent — validate JWT/cookie in middleware before accept; reject 401 if `context.User.Identity?.IsAuthenticated != true`.
2. Derive user id from `context.User.FindFirst(ClaimTypes.NameIdentifier)` — ignore client-sent user id or verify it matches claim.
3. Register/unregister in `try/finally`; remove on close; use `CancellationToken` linked to `HttpContext.RequestAborted`.
4. Replace static dictionary with SignalR groups or Redis-backed connection map for multi-instance.

**Production takeaway:** WebSocket auth happens at HTTP upgrade time — trusting the first message is equivalent to skipping login on REST.

---

---

#### Q6. (M) Clients send large JSON payloads over WebSocket. What limits does ASP.NET Core/Kestrel impose on request/upgrade body sizes and individual WebSocket frames, and how do you enforce application-level max message size safely?

---

**Answer:**

**Answer:** Kestrel limits HTTP upgrade request bodies via `MaxRequestBodySize`; individual `ReceiveAsync` calls return one frame chunk (often up to 4 KB unless buffer enlarged), so apps must reassemble multi-frame messages and enforce their own max assembled size to prevent OOM attacks.

- `serverOptions.Limits.MaxRequestBodySize` applies to the initial HTTP request — usually small for WebSocket upgrade; not a substitute for per-message limits.
- `ReceiveAsync` returns `EndOfMessage` — large logical messages span multiple frames; accumulate into `MemoryStream` until `EndOfMessage`, counting total bytes.
- Default client/server max message sizes in SignalR are configurable (`MaximumReceiveMessageSize`) — raw handlers need equivalent guard.
- Reject messages exceeding cap with close code `1009` (Message Too Big) and log client id.
- Prefer chunking/file upload via HTTP for large blobs; use WebSocket for notifications, not bulk transfer.

```csharp
const int MaxMessageBytes = 64 * 1024;
long total = 0;
using var ms = new MemoryStream();
WebSocketReceiveResult result;
do
{
    result = await socket.ReceiveAsync(buffer, ct);
    total += result.Count;
    if (total > MaxMessageBytes)
    {
        await socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Limit exceeded", ct);
        return;
    }
    ms.Write(buffer, 0, result.Count);
} while (!result.EndOfMessage);
```

**Production takeaway:** Without reassembly limits, a client sending infinite partial frames exhausts server memory — frame size ≠ message size.

---

---

#### Q7. (R) Review this broadcast loop inside a WebSocket handler. Under 2k connections, CPU spikes and slow clients block everyone. What are the problems?

```csharp
private static readonly ConcurrentDictionary<string, WebSocket> _clients = new();

while (socket.State == WebSocketState.Open)
{
    var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
    if (result.MessageType == WebSocketMessageType.Text)
    {
        var tick = JsonSerializer.Deserialize<PriceTick>(buffer.AsSpan(0, result.Count));
        foreach (var kv in _clients)
        {
            var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tick));
            await kv.Value.SendAsync(payload, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
```

*(Assume `_clients` adds each accepted socket on connect.)*

**Answer:**

**Answer:** The handler synchronously awaits send to every client inside one connection's receive loop, serializing broadcast through slow peers, re-serializing JSON per client on every tick, and using `CancellationToken.None` — causing head-of-line blocking and CPU waste.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Sequential `await SendAsync` to all clients in one receiver's loop | Slow client blocks broadcast for everyone |
| Performance | `JsonSerializer.Serialize(tick)` inside foreach | O(n) allocations per message |
| Design | Broadcast triggered by one socket's inbound message | Wrong coupling — should publish to hub/bus |
| Reliability | No check `WebSocketState.Open` before send | Throws or stalls on half-open sockets |
| Shutdown | `CancellationToken.None` | Cannot cancel blocked sends on deploy |

**Fix (priority order):**

1. Decouple: inbound message publishes to channel/bus; separate worker fans out — one client's read loop does not broadcast globally.
2. Serialize once: `ReadOnlyMemory<byte> payload = JsonSerializer.SerializeToUtf8Bytes(tick);` reuse for all sends.
3. Send concurrently with bounded parallelism (`Parallel.ForEachAsync` or per-client outbound queues) — slow clients drop or queue with cap.
4. Skip closed sockets; remove dead entries from `_clients` on failure.
5. Prefer SignalR `Clients.All.SendAsync` or backplane — battle-tested fan-out.

**Production takeaway:** Naive `foreach await SendAsync` on a static dictionary fails at thousands of connections — fan-out needs queueing, backpressure, and single serialization.

---
