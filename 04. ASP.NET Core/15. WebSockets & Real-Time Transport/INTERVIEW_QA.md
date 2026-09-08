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

**Concepts**
- Full-duplex persistent connection after HTTP upgrade handshake
- `Connection: Upgrade` / `Upgrade: websocket` HTTP GET
- Server push without request-response cycle
- Long-lived TCP connection — server resources held for session duration

**Answer**

WebSockets provide a full-duplex, persistent connection between client and server after an initial HTTP upgrade handshake, allowing both sides to send messages at any time without the request-response overhead of standard HTTP. Regular HTTP is stateless and one request yields one response after which the connection may close, while WebSockets keep the TCP connection open for bidirectional framed messages. The upgrade begins as an HTTP GET with `Connection: Upgrade` and `Upgrade: websocket` headers — once accepted, the protocol switches from HTTP to the WebSocket framing protocol. WebSockets suit live dashboards, chat, gaming, and tick feeds where server push latency matters. They consume server resources for the connection duration, unlike short HTTP requests that release resources immediately after the response.

---

## Q2. How do you enable WebSockets in ASP.NET Core?

**Concepts**
- `app.UseWebSockets()` — adds upgrade detection middleware
- `context.WebSockets.IsWebSocketRequest` — guard before accept
- `AcceptWebSocketAsync` — switches protocol and returns `WebSocket`
- `WebSocketOptions.KeepAliveInterval` — server-side ping control

**Answer**

I call `app.UseWebSockets()` optionally with `WebSocketOptions` in the middleware pipeline and handle upgrade requests in an endpoint that checks `context.WebSockets.IsWebSocketRequest` before calling `AcceptWebSocketAsync`.

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

`UseWebSockets` adds middleware that detects upgrade requests and enables the WebSocket subsystem in Kestrel. Without this middleware, upgrade attempts fail or behave as normal HTTP requests. SignalR enables WebSockets internally when you call `AddSignalR()` and map hubs, so raw WebSocket use requires explicit middleware and handler code.

---

## Q3. Where must `UseWebSockets()` be placed in the middleware pipeline?

**Concepts**
- Placement before the upgrade-handling branch and terminal middleware
- Authentication must run before `AcceptWebSocketAsync`
- Proxy `Upgrade` and `Connection` header forwarding requirement

**Answer**

`UseWebSockets()` must be placed early in the pipeline — after exception handling and forwarded headers, but before the branch that handles the upgrade and before terminal middleware that would short-circuit the request. The reason authentication and authorization for the upgrade request must run before `AcceptWebSocketAsync` is that WebSocket messages after upgrade no longer pass through standard HTTP middleware on each frame, so identity must be established at upgrade time. If placed after a terminal middleware or missing entirely, upgrade requests return 404 or fail to switch protocols. Behind a reverse proxy, the proxy must forward `Upgrade` and `Connection` headers and path-base configuration must match the mapped WebSocket route. SignalR's `MapHub` still requires `UseWebSockets()` in the pipeline for the WebSocket transport.

---

## Q4. What happens during a WebSocket upgrade request?

**Concepts**
- `101 Switching Protocols` — server accepts upgrade
- `Sec-WebSocket-Key` / `Sec-WebSocket-Accept` handshake headers
- Auth and cookies apply before upgrade, not per frame
- Load balancer upgrade support and sticky sessions

**Answer**

The client sends an HTTP GET with `Connection: Upgrade`, `Upgrade: websocket`, `Sec-WebSocket-Key`, and related headers. The server validates the request, responds with `101 Switching Protocols` and a computed `Sec-WebSocket-Accept` value, and the connection becomes a WebSocket with framed bidirectional messaging. Until the server accepts, the request is normal HTTP — cookies, JWT bearer tokens, and authorization policies apply at this stage, which is why auth must be established before calling `AcceptWebSocketAsync`. After `AcceptWebSocketAsync`, further communication uses WebSocket frames via `ReceiveAsync` and `SendAsync` rather than HTTP request-response pairs. Failed upgrades return HTTP error status codes such as 400, 401, or 404 before any protocol switch occurs, and load balancers must support connection upgrade.

---

## Q5. What server resources are consumed by an open WebSocket connection?

**Concepts**
- TCP connection, `WebSocket` object, send/receive buffers per connection
- Unbounded static connection dictionaries — memory leak
- Slow clients accumulating outbound queues — backpressure needed
- Monitoring: active connection count, bytes in/out, process memory

**Answer**

Each open WebSocket holds a TCP connection, a `WebSocket` object, send and receive buffers, and any application-level registry state such as connection dictionaries and group memberships until the client closes or the server terminates the connection. Thousands of idle tabs multiply memory for buffers and tracking structures, so unbounded static dictionaries of connections are a common memory leak. Thread pool continuations from `ReceiveAsync` and `SendAsync` add CPU overhead under high message rates. Slow clients that cannot read fast enough accumulate outbound queues unless the server applies backpressure or drops them. I monitor active connection count, bytes in/out, and process memory and alert when connections grow without matching active user sessions.

---

## Q6. What is SignalR?

**Concepts**
- SignalR — high-level real-time messaging over WebSocket, SSE, long polling
- Hub — server-side class with client-callable methods and push targets
- Transport negotiation — automatic fallback for restricted environments
- Redis or Azure Service Bus backplane for scale-out

**Answer**

SignalR is an ASP.NET Core library that provides a high-level real-time messaging abstraction over WebSockets, Server-Sent Events, and long polling, with hubs, connection IDs, groups, and automatic client reconnection support. Developers define hub classes with methods clients can invoke and server push methods such as `Clients.Group("room").SendAsync(...)`, and SignalR negotiates the best available transport — WebSockets when supported, falling back to SSE or long polling through firewalls and proxies. It integrates with authentication via `[Authorize]` on hubs, dependency injection, and scale-out backplanes such as Redis and Azure Service Bus. SignalR is the default choice for ASP.NET Core real-time features unless you need a fully custom binary protocol.

---

## Q7. What is the difference between SignalR and raw WebSockets?

**Concepts**
- Raw WebSocket — low-level framing, custom protocol, manual reconnection
- SignalR hubs — method invocation, groups, connection management
- Transport fallback — SSE and long polling absent in raw WebSocket
- Scale-out — Redis backplane built into SignalR, manual in raw WebSocket

**Answer**

Raw WebSockets give you a low-level framed connection where you define message format, routing, reconnection, and scale-out yourself, while SignalR provides hubs, groups, connection management, transport fallback, and built-in scale-out hooks on top of WebSockets or alternate transports.

| Aspect | Raw WebSocket | SignalR |
|---|---|---|
| Protocol | You define JSON/binary framing | Hub methods, JSON/MessagePack |
| Reconnection | Manual backoff and resubscribe | Client SDK auto-reconnect |
| Scale-out | Custom pub/sub + registry | Redis/Azure backplane built-in |
| Fallback transports | WebSocket only | WebSocket, SSE, long polling |
| Auth | Manual at upgrade time | `[Authorize]`, JWT on negotiate |

Raw WebSockets fit custom protocols, non-.NET clients with strict wire formats, or minimal overhead binary streams. SignalR fits typical notify and broadcast scenarios such as order status, chat, and live dashboards with faster team delivery.

---

## Q8. When would you choose SignalR over raw WebSockets?

**Concepts**
- Group broadcast — `Clients.User(id)` and `Clients.Group(...)`
- Transport fallback — SSE or long polling for proxy-restricted environments
- Client reconnection — auto-reconnect with stateful connection ID
- Raw WebSocket — custom binary protocol or non-SignalR wire format

**Answer**

I choose SignalR when I need group broadcast, automatic transport fallback, client reconnection, and multi-instance scale-out without building that infrastructure myself. Order status updates to browser and mobile clients mapped to user groups with `Clients.User(id)` are simpler with SignalR than maintaining per-connection dictionaries manually. Environments where WebSockets are blocked by proxies benefit from SignalR's SSE and long-polling fallback without separate client code paths, and teams without dedicated real-time protocol expertise ship faster with hub-based APIs and official JavaScript and .NET clients. I choose raw WebSockets when SignalR's overhead, negotiate handshake, or opinionated hub model does not fit — such as embedded devices, third-party binary protocols, or extreme latency tuning.

---

## Q9. What is a SignalR backplane, and why is it needed?

**Concepts**
- Backplane — shared pub/sub bus across all server instances
- `AddStackExchangeRedis` — Redis backplane registration
- Sticky sessions — per-client affinity, not cross-instance fan-out
- Azure SignalR Service — managed alternative to self-hosted backplane

**Answer**

A SignalR backplane is a shared pub/sub message bus such as Redis or Azure Service Bus that synchronizes hub messages across all server instances so a broadcast from one node reaches clients connected to other nodes. Without a backplane, each instance only knows about its local connections, so `Clients.All.SendAsync` on instance A does not reach sockets on instance B. I register it with `AddSignalR().AddStackExchangeRedis(connectionString, options => ...)` or the Azure SignalR Service integration. Sticky sessions alone keep one client on one node but do not solve cross-instance fan-out when events originate on arbitrary nodes. Azure SignalR Service is a managed alternative that offloads connection management and scaling entirely from your web servers.

---

## Q10. How do you scale WebSocket/SignalR applications across multiple server instances?

**Concepts**
- Redis backplane — all instances publish and subscribe to hub messages
- Raw WebSocket scale-out — custom broker and `connectionId → serverId` registry
- Proxy idle timeout — must exceed heartbeat interval
- Load testing connection count vs broadcast fan-out separately

**Answer**

I combine a SignalR backplane or Azure SignalR Service for message fan-out, enforce authentication at connection time, configure proxy WebSocket timeouts, and optionally use sticky sessions for connection affinity while relying on the backplane for cross-node broadcasts. With a Redis backplane, all instances subscribe to a channel prefix — hub messages publish once and every node delivers to its local connections in that group. Raw WebSocket scale-out requires a custom connection registry in Redis and pub/sub routing so each instance subscribes and forwards to local sockets. I configure nginx or ALB idle timeouts longer than the heartbeat interval to prevent proxy-side disconnects. Load-testing should cover connection count and broadcast fan-out separately since 2,000 idle connections behave very differently from broadcasting to 2,000 clients every second.

---

## Q11. How is authentication handled for WebSocket connections?

**Concepts**
- Authentication at HTTP upgrade time — not per WebSocket frame
- JWT via query parameter or `Authorization` header on upgrade GET
- SignalR negotiate endpoint — `[Authorize]` before transport established
- `context.User` claims — derive identity from upgrade, never from first message

**Answer**

Authentication occurs during the HTTP upgrade request before the protocol switches — JWT bearer tokens, cookies, or API keys must be validated in middleware or endpoint authorization, because individual WebSocket frames do not re-run the full HTTP auth pipeline after the upgrade. For JWT, browsers often pass the token as a query parameter or `Authorization` header on the upgrade GET because the WebSocket API's header support varies across clients. SignalR's negotiate endpoint accepts `[Authorize]` and standard authentication handlers before establishing the transport. After upgrade, I derive user identity from `context.User` claims established at upgrade time — I never trust a client-sent user ID in the first WebSocket message, since that is equivalent to skipping authentication on a REST endpoint. Anonymous upgrades should be rejected explicitly with 401 before `AcceptWebSocketAsync` for protected resources.

---

## Q12. What are WebSocket message size limits in ASP.NET Core/Kestrel?

**Concepts**
- `MaxRequestBodySize` — HTTP upgrade body, not per-frame limit
- `ReceiveAsync` returns one frame chunk — reassembly needed for large messages
- `EndOfMessage` flag — signals last fragment of a logical message
- Close code `1009` — Message Too Big

**Answer**

Kestrel limits the initial HTTP upgrade request body via `MaxRequestBodySize`, but per-message limits for WebSocket frames require application-level enforcement because `ReceiveAsync` returns one frame chunk at a time and large logical messages span multiple frames. Default receive buffers are often 4 KB per call, so I reassemble with a `MemoryStream` until `EndOfMessage` is true and count total bytes against a cap. SignalR exposes `MaximumReceiveMessageSize` in hub options while raw handlers must implement equivalent guards and close with status `1009` (Message Too Big) when exceeded. Without a reassembly cap, a malicious client sending infinite partial frames causes out-of-memory failures. I prefer HTTP upload endpoints for large blobs and use WebSockets for notifications and small control messages.

---

## Q13. What is WebSocket backpressure, and why does it matter for broadcasts?

**Concepts**
- Head-of-line blocking — sequential `await SendAsync` to all clients
- Per-client outbound queues — decouple slow receivers from fast senders
- Bounded parallelism fan-out — prevents one slow client stalling broadcast
- Decouple inbound message from outbound broadcast timing

**Answer**

Backpressure occurs when a producer sends messages faster than a slow consumer can read them, causing outbound queues to grow. The specific problem in naive broadcast loops is that awaiting `SendAsync` sequentially to every client means one slow peer blocks delivery to all others — head-of-line blocking. The fix is to serialize the payload once and fan out with bounded parallelism or per-client outbound queues rather than awaiting every send inside one client's receive loop. I remove dead sockets from registries when `WebSocketState` is not `Open` or when sends throw, since stale entries amplify blocking. SignalR handles much of this internally, but raw WebSocket broadcast code needs explicit queue caps and drop policies for slow clients. Decoupling inbound messages by publishing to a channel or bus means one client's read loop does not drive global broadcast timing.

---

## Q14. How do you detect and clean up stale WebSocket connections?

**Concepts**
- `WebSocketOptions.KeepAliveInterval` — Kestrel ping/pong control frames
- Application-level heartbeat — detect unresponsive clients
- `finally` block — always remove from registry on disconnect
- Cap connections per user — prevent unbounded tab exhaustion

**Answer**

I use protocol-level keep-alives plus application heartbeats, enforce idle timeouts, and always remove connections from registries in a `finally` block when the receive loop exits or the token is cancelled. Setting `WebSocketOptions.KeepAliveInterval` causes Kestrel to send control frames, but this may not traverse all proxies without application-level pings. I send periodic heartbeat messages and close the connection if no response arrives within the configured interval. I link `CancellationToken` to `HttpContext.RequestAborted` and host shutdown so deploys do not leave ghost entries in static dictionaries. I cap connections per authenticated user at accept time to prevent one account from opening unbounded tabs and exhausting server memory.

---

## Q15. What is the difference between WebSocket and Server-Sent Events (SSE)?

**Concepts**
- WebSocket — full-duplex, both sides send at any time
- SSE — one-way server push over `text/event-stream` HTTP response
- SSE works over HTTP/1.1 or HTTP/2 without upgrade handshake
- SignalR SSE fallback — transparent to application code

**Answer**

WebSockets are bidirectional — either side can send at any time — while Server-Sent Events are a one-way HTTP-based stream from server to client over a long-lived `text/event-stream` response. SSE works over standard HTTP/1.1 or HTTP/2 without an upgrade handshake, so it traverses some proxies and firewalls more easily than WebSockets. SSE is suitable for live feeds, progress updates, and notifications that only need server push, since client-to-server updates still use regular HTTP requests. WebSockets fit chat, collaborative editing, and gaming where low-latency client messages are frequent. SignalR can fall back to SSE automatically when WebSockets are unavailable, hiding transport details from application code.

---

## Q16. What is long polling, and how does it compare to WebSockets?

**Concepts**
- Long polling — client holds request open until data arrives, then reconnects
- Higher per-message overhead than WebSocket after upgrade
- SignalR last-resort fallback — works through restrictive proxies
- Async wait per client — less efficient than WebSocket at scale

**Answer**

Long polling is a technique where the client sends repeated HTTP requests and the server holds each request open until new data arrives or a timeout occurs, then the client immediately opens another request, simulating push over plain HTTP. It has higher latency and overhead than WebSockets because each message cycle may require new HTTP headers and connection setup. Long polling works everywhere HTTP works including restrictive proxies, which is why SignalR uses it as a last-resort fallback transport. WebSockets maintain one persistent connection with lower per-message overhead after the upgrade. Long polling consumes server threads or async waits per waiting client so at scale it is less efficient than WebSockets or SSE for continuous streams.

---

## Q17. What is a SignalR Hub?

**Concepts**
- `Hub` base class — `Clients`, `Groups`, `Context` properties
- `Groups.AddToGroupAsync` — group membership management
- `MapHub<T>` — endpoint registration and URL mapping
- `[Authorize]` on hub or methods — connection and invocation security

**Answer**

A SignalR Hub is a server-side class that defines methods clients can invoke and provides `Clients`, `Groups`, and `Context` properties for pushing messages to connected clients, groups, or specific connection IDs.

```csharp
public class OrderHub : Hub
{
    public async Task JoinOrderGroup(string orderId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");

    public async Task OrderStatusChanged(string orderId, string status) =>
        await Clients.Group($"order-{orderId}").SendAsync("StatusUpdated", status);
}
```

I map hubs with `app.MapHub<OrderHub>("/hubs/orders")` so clients connect via the SignalR JavaScript or .NET client SDK. Hub methods run in the context of a connected client, and the framework resolves services via constructor DI. `[Authorize]` on the hub class or methods restricts who can connect and invoke operations. Hubs abstract connection lifetime — the framework tracks connection IDs and group membership across reconnections when designed with durable user identifiers.

---

## Q18. What security risks exist when clients self-identify via the first WebSocket message?

**Concepts**
- WebSocket auth at upgrade time — not post-upgrade JSON payload
- Client-supplied user ID — impersonation of any other user
- `context.User.FindFirst(ClaimTypes.NameIdentifier)` — authoritative identity
- Authorization on group subscription — verify claims match group

**Answer**

If the server accepts a client-supplied user ID or tenant ID in the first WebSocket frame instead of binding identity from authenticated claims on the upgrade request, any anonymous connection can impersonate another user and subscribe to their private channels. WebSocket auth must be established at HTTP upgrade time — trusting post-upgrade JSON payloads is equivalent to skipping authentication on REST endpoints. An attacker connects to `/ws/orders`, sends `{ "userId": "victim-guid" }`, and receives events intended for the victim. The fix is to derive identity from `context.User.FindFirst(ClaimTypes.NameIdentifier)` after validating JWT or cookies during upgrade. I also combine authenticated upgrades with authorization checks on group subscription so users can only join groups matching their claims.

---

## Gotchas — WebSockets & Real-Time Transport (Interview Traps)

---

#### Gotcha 1. WebSocket upgrade must happen before any response body is written

**Concepts**
- HTTP upgrade handshake requiring a 101 Switching Protocols response
- `HttpContext.WebSockets.AcceptWebSocketAsync()` sending the upgrade response
- `InvalidOperationException` when response body has already started
- WebSocket upgrade as the point of no return for the HTTP response

**Answer**

A WebSocket connection begins with an HTTP Upgrade handshake — the client sends an HTTP GET with `Upgrade: websocket` headers, and the server responds with 101 Switching Protocols. `context.WebSockets.AcceptWebSocketAsync()` sends this 101 response, transitioning the connection to WebSocket protocol. If any response body bytes have been written before this call, the 101 response cannot be sent and `AcceptWebSocketAsync` throws `InvalidOperationException`. Middleware must check `context.WebSockets.IsWebSocketRequest` before executing any other response-writing logic and call `AcceptWebSocketAsync` as the first response action. Placing the WebSocket upgrade inside a try-catch and falling through to normal response writing on failure also prevents the upgrade.

---

#### Gotcha 2. Not reading from the WebSocket receive loop causes the sender to block

**Concepts**
- WebSocket protocol flow control — sender blocks when receive buffer is full
- `ReceiveAsync` loop required even for send-only server scenarios
- Receive loop detecting graceful close from the client
- Deadlock pattern: both endpoints waiting for the other to read

**Answer**

WebSocket connections are bidirectional, and the underlying TCP flow control means a sender blocks when the receive buffer on the other side is full. A server that sends messages but never reads from the WebSocket — because it only pushes data to clients — causes its clients to eventually block trying to send to the server, and vice versa. Even for a push-only server, a receive loop is required: `WebSocket.ReceiveAsync(buffer, token)` must be called continuously to drain any client-sent messages (including close frames) and to detect when the client disconnects. Failing to run a receive loop prevents clean close detection and causes the connection to appear active after the client has disconnected.

---

#### Gotcha 3. Closing a WebSocket requires `CloseAsync` before disposal — abrupt close sends RST instead of close frame

**Concepts**
- WebSocket close handshake: `CloseAsync` sending `Close` frame, waiting for response
- `CloseOutputAsync` sending close frame without waiting for acknowledgement
- `Abort()` for immediate termination without graceful handshake
- Resource leak when `WebSocket.Dispose()` is called without closing first

**Answer**

A WebSocket connection should be closed with `socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", token)` before disposing the socket. This sends a WebSocket `Close` frame and waits for the peer to acknowledge it, completing the graceful close handshake defined by RFC 6455. Calling `Dispose()` directly or letting the `WebSocket` go out of scope without a close sends a TCP RST, which the client sees as an unexpected disconnection rather than a clean close and may trigger reconnect logic or error handling on the client side. Use `CloseOutputAsync` when you want to initiate close without blocking on the acknowledgement, and `Abort()` only when the connection is in an error state and clean close is not possible.

---

#### Gotcha 4. WebSocket authentication must be established at HTTP upgrade time — post-upgrade frame auth is insecure

**Concepts**
- JWT or cookie authentication checked during HTTP upgrade request
- `context.User` populated from the upgrade request's auth headers
- Post-upgrade frames accepting user-supplied identity bypassing auth
- Combining `UseAuthentication` in the pipeline with WebSocket endpoint protection

**Answer**

WebSocket authentication must be enforced during the HTTP Upgrade request — the standard HTTP authentication mechanisms (JWT Bearer, cookies) apply to the upgrade's HTTP request and populate `context.User` before `AcceptWebSocketAsync` is called. If the server instead accepts an unauthenticated WebSocket upgrade and then trusts a JSON `{ "userId": "..." }` frame sent post-upgrade to identify the user, any anonymous connection can impersonate any user. Validate `context.User.Identity.IsAuthenticated` and extract the user identity from `context.User` claims after `UseAuthentication` has run. Apply `[Authorize]` to controller actions or `.RequireAuthorization()` to Minimal API endpoints that perform the WebSocket upgrade to ensure only authenticated clients can connect.

---

#### Gotcha 5. Binary vs text WebSocket frames — frame type mismatch causes deserialization failures

**Concepts**
- `WebSocketMessageType.Text` vs `WebSocketMessageType.Binary` frame types
- Client and server must agree on frame type for each message
- `Text` frames expected to be valid UTF-8; `Binary` frames are arbitrary bytes
- Checking `WebSocketReceiveResult.MessageType` before interpreting payload

**Answer**

WebSocket messages have a type: `Text` (UTF-8 string) or `Binary` (raw bytes). When the client sends `Text` frames and the server reads them as `Binary`, the bytes are correct but the server treats them as opaque binary data rather than strings. When the client sends `Binary` and the server expects `Text`, JSON deserialization fails because binary-encoded data is not valid UTF-8. Always check `result.MessageType` in the receive loop before interpreting the payload. JavaScript WebSocket clients send strings as `Text` frames by default and `ArrayBuffer` or `Blob` as `Binary`. Standardize on one type in the protocol contract and enforce it by closing the connection with an appropriate close status when the wrong message type is received.

---

#### Gotcha 6. WebSocket connections hold memory and OS sockets — unbounded connections cause resource exhaustion

**Concepts**
- Each WebSocket connection consuming memory for receive/send buffers
- No default connection limit in ASP.NET Core for WebSocket connections
- `MaxConcurrentUpgradedConnections` Kestrel limit for WebSocket connections
- Connection pool management and connection tracking for active WebSocket sessions

**Answer**

Each WebSocket connection consumes a TCP socket, memory for I/O buffers, and a thread pool task for the receive loop. With no connection limit, a long-running application accepting connections from mobile clients (which frequently disconnect without sending a close frame) can accumulate thousands of zombie connections. Set `KestrelServerOptions.Limits.MaxConcurrentUpgradedConnections` to a reasonable bound. Track active connections in a `ConcurrentDictionary<string, WebSocket>` or similar structure to support broadcasting, timeout stale connections, and enforce per-user connection limits. Monitor `dotnet counters` for `Microsoft.AspNetCore.Http.Connections` if using SignalR, or track custom metrics for raw WebSocket connection counts.

---

#### Gotcha 7. Receive buffer must be large enough for a single message — fragmented messages require looping

**Concepts**
- `WebSocket.ReceiveAsync(buffer, token)` reading one fragment, not a complete message
- `WebSocketReceiveResult.EndOfMessage` indicating last fragment of a message
- Small buffer causing multiple receive calls needed to assemble one message
- Memory accumulation when building complete messages from fragments

**Answer**

`WebSocket.ReceiveAsync(buffer, token)` fills the buffer with up to one fragment of data and returns a `WebSocketReceiveResult`. If `result.EndOfMessage` is false, the message continues in subsequent fragments — additional `ReceiveAsync` calls are needed to reassemble the complete message. A common bug is treating the first fragment as the complete message, which works in testing with small payloads but fails silently when a client sends a message larger than the buffer size. Use a `MemoryStream` or `ArrayBufferWriter<byte>` to accumulate fragments, calling `ReceiveAsync` in a loop until `result.EndOfMessage` is true, then process the complete assembled message.

---

#### Gotcha 8. DI scope for WebSocket handlers — no automatic scope per connection, unlike HTTP requests

**Concepts**
- HTTP request scope created automatically by ASP.NET Core for each request
- WebSocket connection lifetime extending beyond the upgrade HTTP request
- Scoped services resolved at upgrade time becoming long-lived
- Manual scope creation per WebSocket connection via `IServiceScopeFactory`

**Answer**

During the HTTP Upgrade handshake, ASP.NET Core creates a request scope that is associated with the upgrade HTTP request. Once the upgrade succeeds and the WebSocket is opened, the HTTP request technically completes but the connection lives on. Scoped services resolved from `HttpContext.RequestServices` during upgrade are valid for the duration of the request scope, which may be disposed when the upgrade request lifecycle ends depending on the hosting model. For connection-lifetime services (tracking connection state, per-connection repositories), create an explicit scope with `IServiceScopeFactory` after the upgrade and use it for the WebSocket handler's lifetime, disposing it when the WebSocket closes.

---

#### Gotcha 9. SignalR without a backplane on multiple instances — hub broadcasts only reach locally connected clients

**Concepts**
- SignalR hub in-memory connection tracking per server instance
- `Clients.All.SendAsync` reaching only clients on the same instance
- Redis backplane via `AddSignalR().AddStackExchangeRedis()`
- Azure SignalR Service as a managed backplane alternative

**Answer**

SignalR's hub connection state is stored in memory on each server instance. A hub broadcast (`Clients.All.SendAsync`, `Clients.Group(name).SendAsync`) only reaches clients connected to the same instance — clients on other instances behind the load balancer never receive it. Sticky sessions (keeping each client on the same pod) prevent reconnection issues but do not route broadcasts across instances. The fix is a Redis backplane: `builder.Services.AddSignalR().AddStackExchangeRedis(connectionString)` installs a Redis pub/sub channel shared by all instances so broadcasts fan out globally. Azure SignalR Service manages the backplane as a managed service, removing the need to operate Redis. Test multi-instance scenarios before launch — single-instance staging hides this problem completely.

---

#### Gotcha 10. Server-Sent Events (SSE) and `application/json` clients — `text/event-stream` content type is required

**Concepts**
- SSE requiring `Content-Type: text/event-stream` response header
- `data:` prefixed lines in SSE protocol format
- Keep-alive headers to prevent proxy timeout disconnection
- SSE unidirectional — client cannot push data to server over SSE

**Answer**

Server-Sent Events require the response `Content-Type` to be `text/event-stream` and the response to follow the SSE protocol format: `data: {payload}\n\n` for events, optional `id:` and `event:` fields, and comment lines starting with `:` for keep-alive. Setting `Content-Type: application/json` breaks the client's `EventSource` API, which only handles `text/event-stream`. Proxies and load balancers with short idle timeouts close SSE connections that receive no data — send `:keepalive\n\n` comment lines every 15-30 seconds to prevent proxy-side timeouts. SSE is unidirectional: the client can only receive events from the server, not send data over the SSE connection. Use WebSockets when bidirectional communication is needed.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (M) A raw WebSocket endpoint returns 404 on upgrade in production but works locally. Review middleware order — where must `UseWebSockets()` sit relative to routing, authentication, and terminal middleware?

**Concepts**
- `UseWebSockets` placement — before upgrade-handling branch
- Auth must run before `AcceptWebSocketAsync` — upgrade is the last HTTP moment
- Proxy `Upgrade` and `Connection` header forwarding
- Path mismatch behind path-base or `Map` branch

**Answer**

`UseWebSockets()` must run before the branch that handles the upgrade — after exception handling and forwarded headers, before endpoints that accept upgrades. The upgrade is an HTTP GET with `Connection: Upgrade`, so JWT bearer, cookies, or API keys must be validated before switching protocols since middleware after a short-circuiting branch may never run. A 404 in production but not locally usually means a path mismatch: the URL is behind a path-base, a `Map("/ws", ...)` branch is not registered under routing, or `UseWebSockets` is missing entirely so the handler never gets to check `IsWebSocketRequest`. Behind nginx or an ALB, the proxy must pass `Upgrade` and `Connection` headers and idle timeouts must exceed the ping interval. I do not rely on `UseAuthorization()` only on controllers if the WebSocket path bypasses endpoint metadata — auth must be applied in the WebSocket delegate directly or via `[Authorize]` on SignalR hubs.

---

#### Q2. (P) A dashboard opens a WebSocket per browser tab and keeps it open for hours. What server-side resources are tied to connection lifetime, and how do you detect stale connections and prevent unbounded memory growth?

**Concepts**
- Per-connection resources: TCP, buffers, registry entries, outbound queues
- `WebSocketOptions.KeepAliveInterval` — Kestrel control frames
- Application heartbeat — close on pong timeout
- Cap connections per user — prevent tab exhaustion

**Answer**

Each open WebSocket holds a TCP connection, a `WebSocket` instance, per-connection send and receive buffers, application registry entries in `ConcurrentDictionary`, and any queued outbound messages for slow clients. Thousands of long-lived tabs multiply memory and threadpool continuations since each waiting `ReceiveAsync` holds a continuation.

I detect stale connections in two ways: Kestrel sends control frames via `WebSocketOptions.KeepAliveInterval`, and I also send application-level heartbeat messages and close with `WebSocketCloseStatus` if no pong arrives within an interval, since proxy-transparent pings are not guaranteed. I enforce a maximum connection duration and idle timeout in the handler loop, and always remove from the registry in a `finally` block on disconnect so deploys do not leave ghost entries. I cap connections per authenticated user at accept time using identity from `context.User`, rejecting excess with 429 or a close code. I monitor connection count, bytes in/out, and GC pressure and alert when growth is not correlated with active user sessions.

---

#### Q3. (D) Product needs live order-status updates to web and mobile clients. Compare **SignalR** vs **raw WebSocket** for this scenario — protocol, reconnection, scale-out, and team velocity.

**Concepts**
- SignalR — hubs, groups, transport fallback, Redis backplane built-in
- Raw WebSocket — custom framing, manual reconnection, manual registry
- Hybrid: SignalR for web, push notifications for mobile background

**Answer**

SignalR is the default for this scenario because it provides hub abstraction, `Clients.User(id)` and group broadcast, automatic JSON or MessagePack framing, client SDK reconnection, and a Redis or Azure Service Bus backplane without custom code. Raw WebSockets require you to define message framing and errors, implement manual backoff and resubscribe on reconnect, build a custom pub/sub and connection registry for scale-out, and handle auth manually at upgrade time only.

| Factor | SignalR | Raw WebSocket |
|---|---|---|
| Protocol | Hub methods; SSE/long-polling fallback | You define framing |
| Reconnection | Client SDK auto-reconnect | Manual backoff, resubscribe |
| Scale-out | Redis/Azure backplane built-in | Custom pub/sub + registry |
| Velocity | Faster for typical notify/broadcast | More code, more risk |

I choose SignalR for the web dashboard because broadcasting order events to groups across browsers is the exact scenario it was built for. For mobile background updates, I prefer APNS and FCM push notifications rather than a persistent socket, since mobile OS kill sockets when the app is backgrounded. I would not force raw WebSocket just to avoid SignalR's negotiate overhead — the operational features it buys are worth more than the marginal cost.

---

#### Q4. (P) You deploy three API instances behind a load balancer. WebSocket clients connected to instance A never receive events raised on instance B. Describe how a **SignalR backplane** (Redis/Azure Service Bus) solves this and what still breaks if you use raw WebSockets without shared state.

**Concepts**
- Redis backplane — publish once, all nodes deliver to local sockets
- Sticky sessions — per-client affinity only, not cross-instance event routing
- Raw WebSocket without shared state — process-local registry misses remote sockets
- `connectionId → serverId` Redis mapping for raw WebSocket routing

**Answer**

SignalR solves this by publishing hub messages to a Redis channel — all instances subscribe to the channel prefix, and when code calls `Clients.Group("user-42").SendAsync(...)`, the message goes to Redis pub/sub and every node delivers it to local sockets in that group. I register it with `builder.Services.AddSignalR().AddStackExchangeRedis(connectionString, o => o.Configuration.ChannelPrefix = "orders:")`.

Sticky sessions alone do not fix cross-instance fan-out — they only keep one client on one node, but events originating on instance B still never reach sockets on instance A. Without a backplane, raw WebSocket registries are process-local, so instance B's event simply has no way to reach instance A's sockets. The raw WebSocket fix requires an external broker such as Redis or NATS, per-instance subscription, a `connectionId → serverId` mapping in Redis, and routing each publish to the correct node or broadcasting to all nodes. Even then, group membership sync, connection lifetime cleanup on crash, message ordering guarantees, and backpressure on slow consumers remain problems you must solve yourself. Azure SignalR Service offloads all of this entirely as a managed alternative.

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

**Concepts**
- No auth check before `AcceptWebSocketAsync` — anonymous connections accepted
- Client-supplied `UserId` — impersonation of any user
- Static registry without `finally` cleanup — memory leak and ghost subscriptions
- `CancellationToken.None` — cannot honor shutdown

**Answer**

The handler has four problems. It accepts any connection before checking authentication — no JWT, cookie, or API key is validated before `AcceptWebSocketAsync`, so any anonymous client can open the socket. It trusts `msg.UserId` from the first JSON message rather than reading identity from `context.User`, which means a client can subscribe to any user's orders by sending an arbitrary GUID. The static `_hub.Register` call has no corresponding cleanup — when the socket closes, the entry remains in the registry indefinitely, causing a memory leak and stale subscriptions. `CancellationToken.None` on `ReceiveAsync` prevents the server from honoring graceful shutdown since the call cannot be cancelled.

The fixes in priority order: validate JWT or cookie before `AcceptWebSocketAsync` and reject with 401 if `context.User.Identity?.IsAuthenticated != true`; derive the user ID from `context.User.FindFirst(ClaimTypes.NameIdentifier)` and ignore or verify the client-sent value against the claim; register and unregister in a `try/finally` block using a `CancellationToken` linked to `HttpContext.RequestAborted`; replace the static dictionary with SignalR groups or a Redis-backed connection map for multi-instance correctness.

---

#### Q6. (M) Clients send large JSON payloads over WebSocket. What limits does ASP.NET Core/Kestrel impose on request/upgrade body sizes and individual WebSocket frames, and how do you enforce application-level max message size safely?

**Concepts**
- `MaxRequestBodySize` — HTTP upgrade body only, not per-frame
- `ReceiveAsync` frame chunks — reassembly needed for logical messages
- `EndOfMessage` flag — signals complete logical message
- Close code `1009` — Message Too Big response

**Answer**

Kestrel limits HTTP upgrade request bodies via `MaxRequestBodySize`, but that applies to the initial HTTP request and is not a substitute for per-message limits since individual `ReceiveAsync` calls return one frame chunk at a time. Large logical messages span multiple frames, so I accumulate into a `MemoryStream` until `EndOfMessage` is true while counting total bytes against a cap, then reject with close code `1009` (Message Too Big) and log the client ID if exceeded.

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

SignalR exposes `MaximumReceiveMessageSize` in hub options as an equivalent guard. I prefer HTTP upload endpoints for large blobs and use WebSockets for notifications and control messages, since frame size does not equal message size and an attacker sending infinite partial frames will exhaust server memory without a reassembly cap.

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

**Concepts**
- Sequential `await SendAsync` in receive loop — head-of-line blocking
- `JsonSerializer.Serialize` inside `foreach` — O(n) allocations per tick
- Broadcast triggered by one socket's inbound message — wrong coupling
- `CancellationToken.None` — blocks shutdown

**Answer**

The handler has five problems. Sequential `await SendAsync` inside one connection's receive loop means a slow client's blocked send stalls delivery to every other client — one unresponsive tab blocks 1,999 others. `JsonSerializer.Serialize(tick)` inside the foreach re-serializes the same object for each client on every tick, producing O(n) allocations under 2,000 connections. Broadcast is triggered by inbound messages on one socket rather than through a shared hub or channel, creating wrong coupling where one client's read rate drives global broadcast timing. Dead sockets are never removed so sends throw or stall on half-open connections. `CancellationToken.None` means blocked sends cannot be cancelled during shutdown.

The fixes in priority order: decouple inbound messages from fan-out — publish to a `Channel<byte[]>` or internal bus; a separate worker fans out so no client's receive loop drives broadcast. Serialize once with `JsonSerializer.SerializeToUtf8Bytes(tick)` and reuse the `ReadOnlyMemory<byte>` for all sends. Send concurrently with bounded parallelism via `Parallel.ForEachAsync` or per-client outbound queues with drop policies for slow clients. Skip and remove closed sockets on failure. Consider SignalR `Clients.All.SendAsync` or a backplane, which handles fan-out and is battle-tested at this scale.
