# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/15. Real-Time UI with SignalR`

---

#### Q1. (R) Review this live auction hub. Bidding works in dev with one user; under load, bids from one user appear on another user's screen and `InvalidOperationException` surfaces in logs about scoped services.

```csharp
public class AuctionHub : Hub
{
    private readonly IAuctionService _auction; // registered Scoped
    private static readonly Dictionary<string, decimal> _highBids = new();
    // ...
}
```

**Answer:** The hub is constructed as a **transient per invocation** type while holding a **scoped** `IAuctionService` and **static shared state** — both violate SignalR lifetime rules and create cross-user data leaks under concurrency.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | Scoped service injected into Hub (transient) | `InvalidOperationException` or captive dependency — scoped resolved from singleton/root provider |
| State | `static Dictionary` for bids | All users share one in-memory map — User A's bid visible to User B |
| Concurrency | Non-thread-safe `Dictionary` | Corrupted state / exceptions under parallel bids |
| Scale-out | Static in-process cache | Instances disagree on high bid; no source of truth |

**Fix (priority order):**

1. Remove static `_highBids`; persist authoritative bid state in scoped/transient service backed by database or Redis with optimistic concurrency.
2. Inject `IAuctionService` via method injection or `IHubContext` + separate scoped service factory — or register hub dependencies carefully: hubs are **not** scoped per connection; use `IHubContext<T>` from controllers/services instead of storing state on the hub instance.
3. For read models during the auction, broadcast via `Clients.All` from the service after successful persist — do not cache authoritative prices in hub fields.

**Production takeaway:** Hubs should be thin transport layers — business state belongs in scoped services or external stores; static fields on hubs are a Karat favorite for cross-user bugs.

---

#### Q2. (P) You deploy four MVC instances behind an Azure Application Gateway. Users on instance 2 never receive chat messages sent from a controller on instance 4. Walk through **Redis backplane** registration for SignalR — what it synchronizes, what it does not, and one production misconfiguration that silently breaks fan-out.

**Answer:** Without a backplane, each instance only knows its own connection IDs — `IHubContext.Clients.Group("room").SendAsync` on instance 4 reaches only sockets connected to instance 4; Redis pub/sub propagates the message to all instances so each forwards to local connections.

- Register: `builder.Services.AddSignalR().AddStackExchangeRedis(configuration["Redis:Connection"], options => { options.Configuration.ChannelPrefix = "MyApp:SignalR:"; });`
- The backplane synchronizes **hub message fan-out** (groups, all, user targets) across servers — it does **not** replicate connection-local state, group membership lists, or custom static registries you built outside SignalR APIs.
- Each instance still maintains its own connection map; Redis carries the "send this payload to group X" instruction, not the WebSocket itself.
- **Silent failure:** Redis instances share the same `ChannelPrefix` across **different environments** (staging + prod) — cross-talk or swallowed messages; or Redis ACL/firewall blocks pub/sub while cache GET works — negotiate succeeds but cross-instance sends never arrive.
- Also verify all instances run the **same hub assembly version** and hub path (`/hubs/chat`) — mismatched routes break client reconnect to wrong shard.

**Production takeaway:** Backplane fixes **message routing across nodes**, not **state** — pair it with external storage for room lists, presence, or user-to-connection mapping if you need durability.

---

#### Q3. (D) Ops proposes **sticky sessions (session affinity)** on the load balancer instead of a SignalR backplane to save Redis cost. The app is a stock ticker dashboard with MVC Razor views and a SignalR hub. What works with sticky sessions alone, what fails on instance recycle or deploy, and when is affinity acceptable vs when is backplane or Azure SignalR mandatory?

**Answer:** Sticky sessions keep a user's WebSocket on one instance, so **server-originated messages from that same instance** (e.g., a timer on that node pushing ticks to its local connections) can work without a backplane — but any message originating on **another** instance (controller action, background job, message from another user on a different node) never reaches the socket.

| Scenario | Sticky sessions only | Backplane / Azure SignalR |
|---|---|---|
| Single instance | Works | Works |
| Broadcast from controller on instance B to user stuck on instance A | **Fails** | Works |
| Rolling deploy / instance drain | Connections drop; affinity cookie may land on cold node — groups/state lost | Reconnect + rejoin pattern; Azure SignalR handles connection migration |
| Background worker publishes price updates | Worker not on user's instance — **Fails** | Works |
| Cost/complexity | Lower infra | Redis cost or Azure SignalR billing |

- Affinity is acceptable for **low-traffic internal tools**, **single-instance**, or prototypes where all real-time events originate on the same node as the socket (rare in MVC + controller + worker setups).
- Mandatory backplane/Azure SignalR when: multiple instances **and** events originate outside the connection's host process (ship order from controller, Redis stream consumer, Hangfire job, user A chats to user B on different nodes).
- Sticky sessions **do not replace** reconnection state recovery — on reconnect the client may land on a different node after deploy.

**Production takeaway:** Karat tests whether you know affinity is a **connection routing** tool, not a **pub/sub** solution — ticker + MVC controller updates is the classic "works in demo, fails in prod" combo.

---

#### Q4. (R) Review hub authorization. Authenticated users on the MVC site can open `/Dashboard`, but the SignalR connection succeeds as anonymous and `Context.User.Identity.Name` is null in hub methods.

**Answer:** Cookie authentication applies to MVC pages but the SignalR negotiate/upgrade path does not automatically attach the auth cookie unless the client sends credentials, and the hub lacks `[Authorize]` so anonymous connections are allowed even when authentication middleware is registered.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Hub auth | No `[Authorize]` on `ChatHub` | Anonymous connections accepted — hub trusts client-supplied room names |
| Client | `withUrl` without credentials | Browser omits auth cookie on negotiate — `Context.User` empty |
| JWT/API | N/A for cookie MVC — but if bearer used, missing `accessTokenFactory` | Same anonymous symptom |
| Security | `SendMessage` uses null user name | Impersonation / blank sender in UI |

**Fix (priority order):**

1. Add `[Authorize]` to `ChatHub` (or policy-based `[Authorize(Policy = "ChatAccess")]`) — reject unauthenticated negotiate with 401.
2. Client: `.withUrl("/hubs/chat", { withCredentials: true })` for cookie auth on same origin; for cross-origin SPA use bearer + `accessTokenFactory`.
3. Ensure middleware order: `UseAuthentication()` before `MapHub`; for JWT, configure bearer events to read token from query on `/negotiate` when WebSocket cannot send headers.
4. Never trust client-provided identity strings — use `Context.UserIdentifier` or `Context.User.FindFirst(ClaimTypes.NameIdentifier)`.

**Production takeaway:** MVC cookie login ≠ SignalR authenticated — wire credentials explicitly and enforce at the hub class level.

---

#### Q5. (R) Review group membership wiring. Some users never receive room messages; logs show `OnConnectedAsync` completed but `Groups.AddToGroupAsync` ran after the client already called `JoinRoom`.

**Answer:** The client calls `connection.invoke("JoinRoom", ...)` **before** `connection.start()` completes — the invocation is dropped or races the connection handshake, so the user is not in the group when the first server messages arrive; duplicate join paths (`OnConnectedAsync` query string vs client `JoinRoom`) add timing ambiguity.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Client order | `invoke` before `start()` | JoinRoom never runs or fails silently — missed messages |
| Race | Server adds group in `OnConnectedAsync` while client also joins | Depends on query string presence — inconsistent membership |
| Idempotency | No ack to client that join succeeded | UI shows "connected" but user not in group |

**Fix (priority order):**

1. Client pattern:
   ```javascript
   await connection.start();
   await connection.invoke("JoinRoom", "support-42");
   ```
2. Prefer **one authoritative join path**: either pass `room` on connect (query string or headers) in `OnConnectedAsync`, **or** expose `JoinRoom` called after start — not both without coordination.
3. Hub returns ack: `public async Task<string> JoinRoom(string room) { await Groups.AddToGroupAsync(...); return room; }` — client awaits invoke before subscribing UI to "ready."
4. On reconnect, client must re-invoke join — groups are not persisted across connection IDs.

**Production takeaway:** SignalR groups are per **connection ID** — membership requires explicit (re)join after every successful `start()`; Karat embeds the classic `invoke` before `start` bug.

---

#### Q6. (P) After a brief network blip, the MVC dashboard reconnects automatically but the user's watchlist group membership and server-side "last seen price" state are gone until they refresh the page. Explain **what SignalR resets on reconnect**, what the server must rehydrate, and a production pattern to restore group membership without a full page reload.

**Answer:** Reconnect assigns a **new `ConnectionId`** — all prior group memberships and connection-scoped server dictionaries keyed by old ID are invalid; automatic reconnect restores the transport only, not application group/state.

- **Resets:** connection ID, group membership, hub instance fields tied to old ID, any custom `ConcurrentDictionary<connectionId, …>` you maintained.
- **Persists:** authenticated user identity (if credentials re-sent on reconnect), server-side durable data in DB/Redis keyed by **user id**, not connection id.
- **Server must rehydrate:** re-add to groups in `OnConnectedAsync` or client-driven `JoinWatchlist(symbols)` after `onreconnected`; reload last prices from cache/DB and push snapshot to caller via `Clients.Caller`.
- **Production pattern:**
  ```csharp
  public override async Task OnConnectedAsync()
  {
      var userId = Context.UserIdentifier;
      if (userId != null)
      {
          await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
          var snapshot = await _watchlist.GetSnapshotAsync(userId);
          await Clients.Caller.SendAsync("WatchlistSnapshot", snapshot);
      }
      await base.OnConnectedAsync();
  }
  ```
  Client: handle `connection.onreconnected` → `invoke("JoinWatchlist", symbols)` mirroring initial connect.
- Enable `WithAutomaticReconnect()` with backoff; show "Reconnecting…" UI until snapshot received.

**Production takeaway:** Treat connection ID as ephemeral — key durable real-time state by user/room in storage and replay on every connect/reconnect.

---

#### Q7. (R) Review broadcasting from an MVC controller after an admin action. The notification never reaches connected browsers; no exception is thrown.

```csharp
public class OrdersController : Controller
{
    private readonly OrderHub _hub; // concrete hub injected
    // ...
    await _hub.Clients.All.SendAsync("OrderShipped", id);
}
```

**Answer:** Controllers must not inject concrete `Hub` types — hubs are not registered in DI for direct resolution; use **`IHubContext<OrderHub>`**, which is registered as a singleton proxy for sending messages outside hub method context.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI | `OrderHub` not registered / wrong lifetime | Null reference, failed activation, or never resolved — silent skip in some setups |
| Pattern | Direct hub injection anti-pattern | `Clients` unavailable — hub instance is not the active connection context |
| Scale-out | Even if hacked, instance-local hub | Misses connections on other nodes without `IHubContext` + backplane |

**Fix (priority order):**

1. Inject `IHubContext<OrderHub> _hubContext` into controller or `OrderNotificationService`.
2. `await _hubContext.Clients.All.SendAsync("OrderShipped", id);` — or target groups/users: `Clients.Group($"order-{id}")`.
3. Register `builder.Services.AddSignalR()` and keep hub as `public class OrderHub : Hub { }` — no `services.AddSingleton<OrderHub>()`.
4. Multi-instance: add Redis backplane or Azure SignalR so controller on any node reaches all connections.

**Production takeaway:** `IHubContext<T>` is the bridge from MVC controller actions to connected clients — Karat pairs this with missing backplane for full production story.

---

#### Q8. (R) A separate SPA on `https://app.example.com` calls your MVC site's SignalR hub at `https://api.example.com/hubs/notifications`. Browser console shows CORS error on the negotiate request; direct WebSocket works in Postman.

**Answer:** SignalR cross-origin requires CORS policy that allows the SPA origin **with credentials** when using cookies, and the client must opt in — `AllowAnyMethod` without `AllowCredentials` plus missing client credentials breaks negotiate preflight or strips auth.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| CORS | No `.AllowCredentials()` | Browser blocks credentialed negotiate — connection fails |
| Client | `withUrl` without `{ withCredentials: true }` | Cookie auth not sent — may fail auth or CORS |
| Wildcard | Cannot use `AllowAnyOrigin()` with credentials | Spec violation — browser rejects |
| Headers | SignalR needs allowed headers on preflight | Custom headers blocked if not allowed |

**Fix (priority order):**

1. Server:
   ```csharp
   p.WithOrigins("https://app.example.com")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials();
   ```
2. Client: `.withUrl("https://api.example.com/hubs/notifications", { withCredentials: true })` for cookies; for JWT use bearer + `accessTokenFactory` (no credentials flag).
3. Call `app.UseCors("Spa")` before `MapHub`; ensure negotiate and WebSocket paths share the policy.
4. Postman bypasses CORS — browser failures are not reproduced there.

**Production takeaway:** SignalR negotiate is HTTP-first — CORS must pass before WebSocket upgrade; cross-origin MVC API + SPA is a common Karat hosting combo.

---

#### Q9. (P) Traffic grows to 50k concurrent connections across regions. Team evaluates **Azure SignalR Service** vs self-hosted hubs with Redis backplane on the MVC app. Compare connection ownership, deployment model (`AddAzureSignalR`), sticky-session requirements, and one scenario where Azure SignalR simplifies ops but changes how you broadcast from controllers.

**Answer:** Azure SignalR Service **hosts and scales connections** in the managed service while your MVC app runs hub **logic** as a serverless worker — you offload connection memory and WebSocket handling from app nodes.

| Factor | Self-hosted + Redis backplane | Azure SignalR Service |
|---|---|---|
| Connection ownership | Your Kestrel processes | Azure service front door |
| Scale | Scale app instances + Redis | Scale connection units independently |
| Sticky sessions | Required without backplane; optional with backplane | **Not required** — service routes to app for hub methods |
| Registration | `AddSignalR().AddStackExchangeRedis(...)` | `AddSignalR().AddAzureSignalR(connectionString)` + `app.MapHub` unchanged |
| Controller broadcast | `IHubContext<T>` + backplane | Same `IHubContext<T>` — SDK forwards to Azure service |

- **Deployment:** Set connection string mode (Default/Serverless); configure upstream endpoints so Azure SignalR can call your app's `/negotiate` and hub endpoints (often via Azure Functions or app service with public URL).
- **Ops win:** No WebSocket connection count on app servers — MVC instances scale on CPU/business logic, not 50k sockets each.
- **Behavior change:** Connection limits and message quotas become **Azure SKU** concerns; local dev may use emulator or fall back to self-hosted; diagnostic logging splits between app and Azure portal.
- **Controller scenario:** `IHubContext<OrderHub>.Clients.User(userId).SendAsync(...)` still works — message flows app → Azure SignalR → client, but you must ensure hub methods are reachable from the service (network, auth, and `[Authorize]` on negotiate).

**Production takeaway:** Azure SignalR trades infrastructure complexity for vendor coupling — broadcasting from MVC controllers looks the same via `IHubContext`, but connection lifecycle moves off your boxes.

---

#### Q10. (R) Review hub method error handling. Clients receive a generic connection drop; server logs show unhandled exceptions inside hub methods.

```csharp
catch (ValidationException ex)
{
    throw new Exception(ex.Message); // rethrow to client
}
```

**Answer:** Unhandled exceptions in hub methods **terminate the connection** by default — throwing raw `Exception` exposes internal messages and gives the client no structured error channel; use `HubException`, caller-targeted messages, or `IHubFilter` for consistent error shapes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Error contract | `throw new Exception(...)` | Connection closed — client sees generic disconnect, not validation detail |
| Security | Exception message to client | Information leakage (stack paths, internal rules) |
| UX | No distinction validation vs fatal | User must reconnect for recoverable validation errors |
| Observability | Generic disconnect | Hard to correlate client failure with server log |

**Fix (priority order):**

1. Recoverable errors: `await Clients.Caller.SendAsync("OrderRejected", new { code = "VALIDATION", message = "..." }); return;` — do not throw.
2. Or `throw new HubException("Invalid order quantity");` — SignalR sends error to caller **without** necessarily killing entire connection (prefer explicit caller message for business rules).
3. Global handling: implement `IHubFilter` with `try/catch` around `next(context)` — map known exceptions to `HubException` or ProblemDetails-shaped payloads.
4. Log server-side with correlation ID; never `throw ex` — use `throw;` or wrap in `HubException` without inner details.
5. Client: `connection.onclose` / failed invoke handlers display user-friendly text; retry only when appropriate.

**Production takeaway:** Hub methods are not MVC actions — unhandled exceptions default to **closing the circuit**; design explicit error messages to the caller for business failures.

---

#### Q11. (R) Review MVC Razor view and client wiring for a live comment feed. Connection fails in staging (HTTPS, subpath deploy) but works locally on HTTP.

```html
.withUrl("/hubs/comments")
connection.start();
connection.invoke("PostComment", ...);
```

**Answer:** Hard-coded root-relative hub URL ignores **`PathBase`** (`/apps/comments`) behind the reverse proxy — negotiate hits the wrong path; missing `await connection.start()` before invoke causes the same class of failure as group join races; antiforgery is unrelated to SignalR unless you POST via MVC, but mixed HTTP/HTTPS breaks cookies.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| PathBase | `/hubs/comments` ignores subpath deploy | 404 negotiate in staging — works locally at site root |
| Client lifecycle | `start()` not awaited before invoke | PostComment fails on slow networks |
| HTTPS | Reverse proxy terminates TLS | Wrong scheme if forwarded headers not configured — cookie/auth issues |
| Script | SignalR script path/version | Missing `@microsoft/signalr` or wrong libman path — reference error |

**Fix (priority order):**

1. Resolve hub URL from server:
   ```html
   <script>
       const hubUrl = "@Url.Content("~/hubs/comments")";
       const connection = new signalR.HubConnectionBuilder()
           .withUrl(hubUrl, { withCredentials: true })
           .withAutomaticReconnect()
           .build();
       connection.on("NewComment", appendComment);
       connection.start().then(() => setupPostButton(connection));
   </script>
   ```
2. `Program.cs`: `app.UseForwardedHeaders()` + `app.UsePathBase("/apps/comments")` before routing — hub mapped after path base.
3. Use LibMan/npm consistent version of `@microsoft/signalr` matching server package.
4. For MVC form + SignalR hybrid, keep antiforgery on form POSTs; hub methods validate `[Authorize]` and input — do not bypass server validation in `PostComment`.

**Production takeaway:** MVC subpath and reverse-proxy deployments break root-relative SignalR URLs — generate the hub path with `Url.Content` and await `start()` before any `invoke`.
