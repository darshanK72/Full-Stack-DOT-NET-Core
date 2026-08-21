# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_ANSWERS.md).

> **Folder:** `05. ASP.NET Core/15. WebSockets & Real-Time Transport`

---

#### Q1. (M) A raw WebSocket endpoint returns 404 on upgrade in production but works locally. Review middleware order — where must `UseWebSockets()` sit relative to routing, authentication, and terminal middleware?

**Answer:** `UseWebSockets()` must run before the branch that handles the upgrade (typically before `UseRouting`/`Map`), and authentication/authorization must execute on the upgrade request before `AcceptWebSocketAsync` — placing WebSocket handling after a terminal middleware or missing `UseWebSockets()` causes failed upgrades or anonymous connections.

- Call `app.UseWebSockets(new WebSocketOptions { ... })` early — after exception handling and forwarded headers, before endpoints that accept upgrades.
- The upgrade is an HTTP GET with `Connection: Upgrade` — JWT bearer, cookies, or API keys must be validated **before** switching protocols; middleware after a short-circuiting branch may never run.
- `Map("/ws", handler)` or minimal API `MapGet` with WebSocket check must be registered with routing active — 404 often means path mismatch behind path-base or missing Map branch.
- Behind nginx/ALB, ensure proxy passes `Upgrade` and `Connection` headers and idle timeouts exceed ping interval.
- Do not call `UseAuthorization()` only on controllers if WebSocket path bypasses endpoint metadata — apply auth in the WebSocket delegate or use `[Authorize]` on SignalR hubs.

**Production takeaway:** WebSocket failures are often middleware-order or proxy-header issues, not socket API bugs — trace the HTTP upgrade request first.

---

#### Q2. (P) A dashboard opens a WebSocket per browser tab and keeps it open for hours. What server-side resources are tied to connection lifetime, and how do you detect stale connections and prevent unbounded memory growth?

**Answer:** Each open WebSocket holds a connection object, receive/send buffers, and any registered server-side state (hub groups, static dictionaries) until close — long-lived tabs multiply memory and threadpool continuations, so apps must heartbeat, enforce idle timeout, and cap connections per user.

- Resources: TCP connection, `WebSocket` instance, per-connection buffers, application registry entries (`ConcurrentDictionary`), and any queued outbound messages for slow clients.
- Detect stale: protocol-level **ping/pong** (WebSocket keep-alive) or application heartbeat messages; close with `WebSocketCloseStatus` if no pong within interval.
- Configure `WebSocketOptions.KeepAliveInterval` so Kestrel sends control frames — does not replace app-level heartbeats through proxies.
- Enforce max duration and idle timeout in handler loop; remove from registry in `finally` on disconnect.
- Cap tabs per user at connection time using authenticated identity — reject excess with 429 or close code.
- Monitor connection count, bytes in/out, and GC pressure — alert on growth without corresponding user sessions.

**Production takeaway:** Treat WebSocket connections like leased server resources, not free persistent HTTP — unbounded registries are memory leaks with a longer half-life.

---

#### Q3. (D) Product needs live order-status updates to web and mobile clients. Compare **SignalR** vs **raw WebSocket** for this scenario — protocol, reconnection, scale-out, and team velocity.

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

#### Q4. (P) You deploy three API instances behind a load balancer. WebSocket clients connected to instance A never receive events raised on instance B. Preview how a **SignalR backplane** (Redis/Azure Service Bus) solves this and what still breaks if you use raw WebSockets without shared state.

**Answer:** SignalR backplane publishes hub messages to all instances so each server forwards to its local connections — without shared pub/sub, raw WebSocket registries are process-local and events on instance B never reach sockets on instance A.

- Register: `builder.Services.AddSignalR().AddStackExchangeRedis(connectionString, o => o.Configuration.ChannelPrefix = "orders:");`
- When code calls `Clients.Group("user-42").SendAsync(...)`, the message goes to Redis pub/sub; all nodes receive and deliver to local sockets in that group.
- Sticky sessions alone do not fix **cross-instance fan-out** — they only keep one client on one node; events originating elsewhere still miss subscribers.
- Raw WebSocket fix: external broker (Redis, NATS) + subscribe per instance; maintain `connectionId → serverId` in Redis; route publishes to correct node or broadcast to all nodes.
- Still breaks without: group membership sync, connection lifetime cleanup on crash, message ordering guarantees, and backpressure on slow consumers.
- Azure SignalR Service offloads connection management entirely — alternative to self-hosted backplane at scale.

**Production takeaway:** Scale-out real-time is a distributed systems problem — backplane or Azure SignalR is the ASP.NET answer; sticky cookies are insufficient.

---

#### Q5. (R) Review this WebSocket endpoint. Anonymous clients can connect and impersonate any user id sent in the first message. What is wrong?

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

#### Q6. (M) Clients send large JSON payloads over WebSocket. What limits does ASP.NET Core/Kestrel impose on request/upgrade body sizes and individual WebSocket frames, and how do you enforce application-level max message size safely?

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

#### Q7. (R) Review this broadcast loop inside a WebSocket handler. Under 2k connections, CPU spikes and slow clients block everyone. What are the problems?

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
