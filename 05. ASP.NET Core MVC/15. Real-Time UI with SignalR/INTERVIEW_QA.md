# Real-Time UI with SignalR — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is SignalR and how does it relate to ASP.NET Core MVC?](#q1-what-is-signalr-and-how-does-it-relate-to-aspnet-core-mvc)
2. [Q2. What is a SignalR Hub?](#q2-what-is-a-signalr-hub)
3. [Q3. What is the difference between a Hub and an MVC controller?](#q3-what-is-the-difference-between-a-hub-and-an-mvc-controller)
4. [Q4. How do you map a Hub endpoint in `Program.cs`?](#q4-how-do-you-map-a-hub-endpoint-in-programcs)
5. [Q5. What is `IHubContext` and why is it used from MVC controllers?](#q5-what-is-ihubcontext-and-why-is-it-used-from-mvc-controllers)
6. [Q6. Why should you not inject a Hub directly into a controller?](#q6-why-should-you-not-inject-a-hub-directly-into-a-controller)
7. [Q7. What is a SignalR backplane (e.g., Redis) and when is it needed?](#q7-what-is-a-signalr-backplane-eg-redis-and-when-is-it-needed)
8. [Q8. What is the difference between sticky sessions and a SignalR backplane?](#q8-what-is-the-difference-between-sticky-sessions-and-a-signalr-backplane)
9. [Q9. How does cookie authentication apply to SignalR connections?](#q9-how-does-cookie-authentication-apply-to-signalr-connections)
10. [Q10. What are SignalR Groups and how do clients join them?](#q10-what-are-signalr-groups-and-how-do-clients-join-them)
11. [Q11. What happens to group membership on SignalR reconnect?](#q11-what-happens-to-group-membership-on-signalr-reconnect)
12. [Q12. What is the negotiate step in SignalR?](#q12-what-is-the-negotiate-step-in-signalr)
13. [Q13. What CORS settings are required for cross-origin SignalR connections?](#q13-what-cors-settings-are-required-for-cross-origin-signalr-connections)
14. [Q14. What is Azure SignalR Service and when would you use it?](#q14-what-is-azure-signalr-service-and-when-would-you-use-it)
15. [Q15. What is `HubException` and how should hub errors be returned to clients?](#q15-what-is-hubexception-and-how-should-hub-errors-be-returned-to-clients)
16. [Q16. How do you invoke hub methods from JavaScript in a Razor view?](#q16-how-do-you-invoke-hub-methods-from-javascript-in-a-razor-view)
17. [Q17. What are `Clients.All`, `Clients.Caller`, and `Clients.Others`?](#q17-what-are-clientsall-clientscaller-and-clientsothers)
18. [Q18. How does `PathBase` affect SignalR hub URLs behind a reverse proxy?](#q18-how-does-pathbase-affect-signalr-hub-urls-behind-a-reverse-proxy)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is SignalR and how does it relate to ASP.NET Core MVC?

**Concepts**
- Server-to-client push over WebSockets, SSE, or long polling
- Hub mapped alongside MVC routes in `Program.cs`
- `IHubContext<THub>` as the bridge from MVC controllers to connected clients
- Shared host, authentication middleware, and DI container

**Answer**

SignalR is a real-time communication library for ASP.NET Core that enables server-to-client push, selecting WebSockets, Server-Sent Events, or long polling based on what the environment supports. In MVC apps it complements traditional request/response pages with live notifications, chat, dashboards, and progress updates. MVC renders the initial Razor page and the JavaScript client connects to a SignalR hub for ongoing updates, so the two work together rather than competing. Hubs share the same host, authentication middleware, and DI container as the rest of the MVC application.

---

## Q2. What is a SignalR Hub?

**Concepts**
- `Hub` or `Hub<T>` base class providing `Clients`, `Groups`, and `Context`
- Hub methods invoked from JavaScript via `connection.invoke`
- Server pushing to clients via `Clients.All.SendAsync`
- Transient per-invocation lifetime
- Business logic belonging in injected services, not the hub

**Answer**

A Hub is a high-level pipeline class that defines methods callable from clients and uses `Clients`, `Groups`, and `Context` to push messages to connected browsers. Hub methods are invoked from JavaScript via `connection.invoke("MethodName", args)`, and server code calls `await Clients.All.SendAsync("EventName", data)` to push to connections. Hub instances are transient per invocation, not scoped per connection, so I keep hubs thin transport layers with business logic in injected services.

---

## Q3. What is the difference between a Hub and an MVC controller?

**Concepts**
- Controller per HTTP request vs Hub over long-lived persistent connection
- Controller returning `IActionResult`; Hub returning `Task` and pushing via `Clients`
- `IHubContext<THub>` for broadcasting from controllers to hub clients

**Answer**

Controllers handle HTTP request/response cycles and return action results. Hubs maintain persistent bidirectional connections and push messages asynchronously outside the HTTP request lifecycle. Controllers are activated per HTTP request; hubs handle multiple invocations over a long-lived connection. I use controllers for page rendering and form POSTs, and hubs for real-time events to already-connected clients. When a controller action needs to broadcast to hub clients, I use `IHubContext<THub>` — I never inject the hub class directly into a controller.

---

## Q4. How do you map a Hub endpoint in `Program.cs`?

**Concepts**
- `builder.Services.AddSignalR()` registering hub services
- `app.MapHub<THub>("path")` after routing and auth middleware
- Dedicated `/hubs/` prefix convention
- Scale-out via `.AddStackExchangeRedis` or `.AddAzureSignalR` on `AddSignalR()`

**Answer**

I register SignalR in services and map the hub endpoint after routing and authentication middleware are configured.

```csharp
builder.Services.AddSignalR();

var app = builder.Build();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/hubs/chat");
app.MapControllerRoute(/* ... */);
app.Run();
```

The hub path is independent of MVC conventional routes, so I use a dedicated prefix like `/hubs/`. Authentication middleware must run before `MapHub` when hubs require `[Authorize]`. For scale-out I chain `.AddStackExchangeRedis(...)` or `.AddAzureSignalR(...)` on `AddSignalR()`, and I configure `UsePathBase` and forwarded headers before hub mapping for subpath deployments.

---

## Q5. What is `IHubContext` and why is it used from MVC controllers?

**Concepts**
- `IHubContext<THub>` as singleton service for sending from outside hub context
- Same `Clients`, `Groups`, and `User` targeting APIs as inside a hub
- Backplane enabling cross-instance message delivery
- Controllers and background services as common callers

**Answer**

`IHubContext<THub>` is a singleton service registered by SignalR that allows any application code — MVC controllers, background services, or middleware — to send messages to connected clients without being inside a hub method. I inject `IHubContext<ChatHub>` into a controller and call `await _hubContext.Clients.All.SendAsync("OrderShipped", id)`. It provides the same `Clients`, `Groups`, and `User` targeting APIs as inside a hub. With a Redis backplane or Azure SignalR, messages reach connections on all server instances, since the `IHubContext` singleton participates in the same backplane.

---

## Q6. Why should you not inject a Hub directly into a controller?

**Concepts**
- Hub not registered in DI; instantiated by SignalR per invocation with connection context
- DI-resolved hub instance lacking active connection state
- `IHubContext<THub>` as correct abstraction for out-of-hub sends
- Business notifications flowing controller → service → `IHubContext` → clients

**Answer**

Hub classes are not registered in DI for direct injection — they are instantiated by SignalR per invocation with connection context. Injecting a concrete `Hub` into a controller fails activation, produces wrong lifetimes, or gives an instance without active connection state, so `Clients` and other connection-specific members are not usable. `IHubContext<THub>` is the correct abstraction for sending from controllers and services. I never register `services.AddSingleton<MyHub>()` to work around this, since the result is still an instance with no live connection.

---

## Q7. What is a SignalR backplane (e.g., Redis) and when is it needed?

**Concepts**
- Pub/sub propagating hub messages across all server instances
- Required when multiple instances and events originate on any node
- `AddStackExchangeRedis` registration with `ChannelPrefix`
- Backplane synchronizing message fan-out, not connection state or group membership

**Answer**

A backplane uses pub/sub — typically Redis — to propagate SignalR messages across all server instances so a send originating on one node reaches clients connected to other nodes. It is required when running multiple ASP.NET Core instances and events originate on any instance — controller actions, background jobs, or one user triggering a message to another. I register it via `builder.Services.AddSignalR().AddStackExchangeRedis(connectionString, options => { options.Configuration.ChannelPrefix = "MyApp:"; })`. The backplane synchronizes message fan-out only — it does not replicate connection state, group membership, or custom in-memory registries, since each instance still maintains its own connection map.

---

## Q8. What is the difference between sticky sessions and a SignalR backplane?

**Concepts**
- Sticky sessions routing a user's WebSocket to the same server instance
- Backplane broadcasting messages to all instances regardless of connection location
- Sticky sessions failing when events originate on a different instance
- Rolling deploy or reconnect potentially changing affinity

**Answer**

Sticky sessions route a user's WebSocket to the same server instance for the connection lifetime. A backplane broadcasts messages to all instances regardless of which node holds the connection. Sticky sessions alone work only when all events originate on the same instance as the socket — which is rare in MVC apps with controller-triggered broadcasts. A controller on instance B sending to a user connected to instance A produces no delivery without a backplane, since instance B has no socket for that user. On rolling deploy or reconnect, affinity may change anyway, so group membership must be re-established either way.

---

## Q9. How does cookie authentication apply to SignalR connections?

**Concepts**
- Auth cookie included on negotiate request for same-origin connections
- `withCredentials: true` required for cross-origin cookie auth
- `[Authorize]` on hub enforcing authentication at connection time
- JWT bearer using `accessTokenFactory` since WebSockets cannot send Authorization headers
- Middleware order: `UseAuthentication()` before `MapHub`

**Answer**

Cookie authentication used by MVC applies to SignalR when the client sends credentials on the negotiate request and the hub is protected with `[Authorize]`. The auth cookie must be included via same-origin requests or `withCredentials: true` on cross-origin connections with proper CORS. Without `[Authorize]` on the hub, connections succeed as anonymous even if the user is logged into MVC pages — the middleware runs but the hub does not reject unauthenticated connections. For JWT bearer auth I use `accessTokenFactory` on the client because WebSockets cannot always send `Authorization` headers, and `UseAuthentication()` must run before `MapHub` in the pipeline so `Context.User` reflects the authenticated principal inside hub methods.

---

## Q10. What are SignalR Groups and how do clients join them?

**Concepts**
- Named connection collections for targeted broadcasts
- `Groups.AddToGroupAsync(ConnectionId, groupName)` adding connections
- Client calling a hub method after `connection.start()` to join
- Group membership per `ConnectionId` — lost on reconnect

**Answer**

Groups are named collections of connections used to target broadcasts to subsets of clients — chat rooms, document collaborators, or per-tenant channels. Server code adds connections with `await Groups.AddToGroupAsync(Context.ConnectionId, groupName)`, typically in `OnConnectedAsync` or in a dedicated hub method the client invokes. Clients join by calling a hub method after `connection.start()` completes — for example `await connection.invoke("JoinRoom", "support-42")`. Broadcasts reach the group via `await Clients.Group("support-42").SendAsync("ReceiveMessage", message)`. Group membership is per connection ID, so clients must explicitly rejoin after every reconnect since a new connection ID is assigned.

---

## Q11. What happens to group membership on SignalR reconnect?

**Concepts**
- Reconnect assigning new `ConnectionId` invalidating prior group memberships
- Automatic reconnect restoring transport only, not application group state
- `connection.onreconnected` as hook for client-side rejoin
- Durable state keyed by user ID in database or Redis, not connection ID

**Answer**

Reconnect assigns a new `ConnectionId` and all previous group memberships are lost since groups are keyed by connection ID. Automatic reconnect restores the transport only — not application group state or connection-scoped dictionaries. The client must re-add itself to groups, typically via `connection.onreconnected(() => connection.invoke("JoinRoom", roomName))`. I key durable state by user ID in database or Redis, not by connection ID, and push a snapshot to `Clients.Caller` after rejoin so the UI recovers missed updates rather than showing a stale state.

---

## Q12. What is the negotiate step in SignalR?

**Concepts**
- Initial HTTP request discovering available transports
- Authentication occurring at this HTTP stage before WebSocket upgrade
- CORS and credentials required at negotiate for cross-origin connections
- Wrong hub URL (missing PathBase) producing 404 at negotiate

**Answer**

Negotiate is the initial HTTP request the SignalR client sends to the hub URL to discover available transports — WebSockets, SSE, long polling — obtain a connection token, and establish authentication before upgrading the connection. This occurs before the WebSocket upgrade, which means CORS and authentication must succeed at this HTTP stage, not later. Cross-origin negotiate requires CORS with credentials when using cookie authentication. A wrong hub URL — missing `PathBase`, wrong subpath — produces 404 at negotiate, so the connection never starts even if a WebSocket to the same host would work.

---

## Q13. What CORS settings are required for cross-origin SignalR connections?

**Concepts**
- Exact SPA origin with `.AllowCredentials()` — wildcard origins incompatible with credentials
- `app.UseCors` before `MapHub` in pipeline
- Client `withCredentials: true` for cookie auth
- JWT bearer using `accessTokenFactory` instead of credentials flag

**Answer**

Cross-origin SignalR requires a CORS policy specifying the exact SPA origin with `.AllowCredentials()` — wildcard origins cannot be used with credentials and browsers will reject the response.

```csharp
builder.Services.AddCors(o => o.AddPolicy("Spa", p =>
    p.WithOrigins("https://app.example.com")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()));
```

I call `app.UseCors("Spa")` before `MapHub` so the policy applies to negotiate and WebSocket paths. For cookie auth the client uses `{ withCredentials: true }` on `.withUrl(...)`; for JWT bearer I use `accessTokenFactory` instead because Postman bypasses CORS — browser console errors are the correct diagnostic source.

---

## Q14. What is Azure SignalR Service and when would you use it?

**Concepts**
- Managed service hosting WebSocket connections separately from app instances
- `AddAzureSignalR(connectionString)` replacing self-hosted connections
- `IHubContext<T>` usage unchanged in controllers
- Sticky sessions not required; connection ownership moves to Azure
- Trade-off: managed scaling vs vendor coupling and SKU limits

**Answer**

Azure SignalR Service is a managed service that hosts and scales WebSocket connections separately from MVC application instances. The app runs hub logic while Azure handles connection memory and fan-out at scale. I register it via `builder.Services.AddSignalR().AddAzureSignalR(connectionString)` and `MapHub` calls remain unchanged. I use it when concurrent connections exceed what app servers can hold, or when multi-region deployment is needed. `IHubContext<T>` usage in controllers is unchanged — the SDK forwards messages through the service. Sticky sessions are not required since connection ownership moves to Azure, but the trade-off is vendor coupling, SKU connection limits, and upstream endpoint configuration.

---

## Q15. What is `HubException` and how should hub errors be returned to clients?

**Concepts**
- `HubException` sending a structured error to the calling client
- Unhandled generic exceptions potentially terminating the connection
- `Clients.Caller.SendAsync` for recoverable business failures without throwing
- `IHubFilter` for global exception mapping and logging
- Never exposing stack traces or internal details to clients

**Answer**

`HubException` is a SignalR-specific exception that sends an error payload to the calling client for recoverable business failures without necessarily terminating the entire connection. Unhandled generic exceptions in hub methods can close the connection, which is the wrong behavior for validation errors. For recoverable errors I prefer `await Clients.Caller.SendAsync("OperationFailed", new { code, message })` and return without throwing, so the connection stays open. For errors the client should display I use `throw new HubException("User-friendly message")`. Global exception mapping and correlation logging go in an `IHubFilter`. I never expose stack traces or internal details to clients — I log server-side only.

---

## Q16. How do you invoke hub methods from JavaScript in a Razor view?

**Concepts**
- `@microsoft/signalr` client library loaded before the script block
- `HubConnectionBuilder` with `withUrl` and `withAutomaticReconnect`
- `connection.start()` must complete before any `invoke` call
- `Url.Content("~/hubs/...")` generating path-base-aware URL
- `onreconnected` handler for group rejoin and UI state refresh

**Answer**

I include the `@microsoft/signalr` client library, build a connection to the hub URL, start it, then invoke server methods and register client-side handlers for server pushes.

```html
<script src="~/lib/microsoft/signalr/dist/browser/signalr.min.js"></script>
<script>
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("@Url.Content("~/hubs/comments")", { withCredentials: true })
        .withAutomaticReconnect()
        .build();

    connection.on("NewComment", (comment) => { /* update DOM */ });

    connection.start()
        .then(() => connection.invoke("JoinRoom", roomId))
        .catch(err => console.error(err));
</script>
```

I always await `connection.start()` before any `invoke` call, since invoking before the connection is established silently fails. I generate the hub URL with `Url.Content` so path base is respected in subpath deployments, and I handle `onreconnected` to rejoin groups and refresh UI state after reconnect.

---

## Q17. What are `Clients.All`, `Clients.Caller`, and `Clients.Others`?

**Concepts**
- `Clients.All` — every connected client on the hub
- `Clients.Caller` — only the invoking connection
- `Clients.Others` — all connections except the caller
- Additional targeting: `Clients.User`, `Clients.Group`, `Clients.Client`

**Answer**

These are built-in client targeting collections on `IHubCallerClients` for addressing connected clients without manually tracking connection IDs. `Clients.All` reaches every connected client on the hub and is subject to backplane fan-out across instances. `Clients.Caller` targets only the connection that invoked the current hub method, which is useful for returning results or errors to the requesting client. `Clients.Others` reaches all connections except the caller, which is the right choice when broadcasting a chat message so the sender does not receive their own echo. For finer targeting I use `Clients.User(userId)`, `Clients.Group(groupName)`, and `Clients.Client(connectionId)`.

---

## Q18. How does `PathBase` affect SignalR hub URLs behind a reverse proxy?

**Concepts**
- `UsePathBase` stripping prefix so app routes without subpath
- Hard-coded root-relative hub URLs ignoring path base
- `Url.Content("~/hubs/...")` generating path-base-aware URLs
- `UseForwardedHeaders` required for correct scheme and host in absolute URLs

**Answer**

When the app is deployed under a subpath like `/apps/mvc`, `UsePathBase` must be configured and hub URLs must include that prefix. Hard-coded `/hubs/chat` ignores the path base, so negotiate returns 404 in staging while working locally at the site root. I generate hub URLs server-side with `@Url.Content("~/hubs/chat")` so the path base is applied automatically. I configure `app.UseForwardedHeaders()` and `UsePathBase("/apps/mvc")` before routing and `MapHub`, and the reverse proxy must forward WebSocket upgrades and negotiate requests to the correct path. Misconfigured forwarded headers also break cookie auth and HTTPS scheme detection.

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

#### Gotcha 1. Business logic in Razor views

**Concepts**
- Views as presentation-only layer
- Business rules in Razor bypassing unit tests
- Authorization belonging to filters and policies
- Service layer as owner of calculations and decisions

**Answer**

Placing pricing, discount, or business rules in `.cshtml` files means that logic cannot be unit tested, often duplicates the service layer, and diverges from API or batch behavior over time. Views should only render data the controller or ViewModel already computed. Authorization belongs in filters, policies, or controller checks before the view executes. I keep Razor limited to presentation formatting — any calculation or decision that affects correctness lives in services.

---

#### Gotcha 2. EF entities passed directly to views

**Concepts**
- Navigation property lazy-load triggering unexpected queries during rendering
- Over-posting via mass assignment on POST action binding
- Dedicated ViewModels as UI-contract decoupling layer
- Controller or mapping service as entity-to-ViewModel boundary

**Answer**

Binding and displaying EF Core entities exposes navigation properties that trigger unexpected lazy queries during rendering and enables mass assignment of properties users should not control on POST. The fix is a dedicated ViewModel with only the fields the view needs, mapped in the controller or a mapping service before passing to the view or reading from the form.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Concepts**
- `application/x-www-form-urlencoded` vs JSON `Content-Type` mismatch
- `[FromBody]` using JSON input formatter, leaving model at defaults on mismatch
- Silent binding failure producing no exception

**Answer**

Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`, not JSON. `[FromBody]` uses the JSON input formatter and leaves the model at default values when the content type doesn't match, so the action runs with empty or zero fields without any error. I remove `[FromBody]` for conventional form POSTs and let model binding read form fields.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Concepts**
- Client validation bypassable by any HTTP client
- `ModelState.IsValid` as mandatory server enforcement gate
- Missing server check as a security defect

**Answer**

Client-side validation is bypassable — attackers POST directly without running browser scripts. Server-side validation is mandatory before any persist, redirect, or side effect. I always gate POST actions with `if (!ModelState.IsValid) return View(model);`. Remote validation and unobtrusive rules are not security boundaries.

---

#### Gotcha 5. `return View()` after successful POST

**Concepts**
- Duplicate submission on browser refresh of a POST response
- Post-Redirect-Get (PRG) pattern separating mutation from display
- `TempData` for flash messages across redirect

**Answer**

Returning the same view after a successful POST means the browser resubmits the POST body when the user refreshes. The fix is Post-Redirect-Get: `return RedirectToAction(nameof(Index))` after successful create or update, separating the mutation from the display. Flash success messages go via `TempData` on the redirect target.

---

#### Gotcha 6. `ModelState` after redirect

**Concepts**
- `ModelState` as request-scoped, not surviving `RedirectToAction`
- Return `View(model)` on failure, redirect only on success pattern

**Answer**

`ModelState` is request-scoped and does not survive `RedirectToAction` — validation errors are lost unless I redisplay the form without redirecting on failure. The standard pattern is redirect only on success; on validation failure return `View(model)` with errors displayed inline.

---

#### Gotcha 7. TempData read twice in layout and view

**Concepts**
- `TempData` consumed on first read by default
- `Peek()` reading without consuming
- `Keep()` retaining after first read for a second consumer

**Answer**

`TempData` is consumed on the first read by default, so if the layout reads a flash message, the view sees nothing. I use `TempData.Peek("Message")` in the layout to read without consuming, or call `TempData.Keep("Message")` after the layout reads so the view can also read it. The simpler approach is a single consumption point — either the layout or a dedicated partial, not both.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Concepts**
- `[Area("AreaName")]` required for area route discovery
- Area routing registered separately with `{area:exists}` constraint

**Answer**

Controllers in `Areas/Admin/Controllers` without `[Area("Admin")]` are not discovered by the areas route and return 404 or match the wrong conventional route. Every area controller must declare `[Area("AreaName")]` matching its folder, and area routing is registered separately in `Program.cs` with the `{area:exists}` constraint.

---

#### Gotcha 9. Link generation without `asp-area`

**Concepts**
- Tag helpers defaulting to current area context
- `asp-area` required for cross-area links
- `Url.Action` requiring `area` in route values

**Answer**

Tag helpers default to the current area context when generating URLs, so links from a root view to an area controller need explicit `asp-area="Admin"` or they generate URLs without the area segment. Cross-area links require both `asp-area` and `asp-controller`. The same rule applies to `Url.Action` — I pass `new { area = "Admin" }` in route values.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Concepts**
- Unchecked checkbox posting nothing, model binding defaulting to `false`
- `[Required]` never failing since `false` is a valid value
- `bool?` with `[Required]` requiring explicit selection
- Hidden-field pattern for deliberate `false` posting

**Answer**

An unchecked checkbox posts nothing, so model binding sets a non-nullable `bool` to `false`. `[Required]` never fails because `false` is a valid non-null value. I use `bool?` with `[Required]` when I need an explicit true selection for consent checkboxes, or the hidden-field pattern where a hidden input posts `false` and the checkbox posts `true`.

---

#### Gotcha 11. Collection binding with gap indices

**Concepts**
- Contiguous zero-based index requirement for collection model binder
- Client-side reindexing after row deletion

**Answer**

Deleting a row from a dynamic form leaving indices like `Lines[0]` and `Lines[2]` breaks model binder alignment — index 1 is missing and subsequent items may bind incorrectly or truncate. I reindex client-side after row deletion so indices are contiguous starting at zero.

---

#### Gotcha 12. `@Html.Raw` with user content

**Concepts**
- Razor auto-encoding preventing XSS by default
- `@Html.Raw` bypassing encoding for attacker-supplied content
- Server-side sanitization before any raw rendering

**Answer**

Default Razor encoding prevents XSS by HTML-encoding output, so `@Html.Raw(Model.UserComment)` with unsanitized user content renders attacker-supplied script. I use `@Model.UserComment` for auto-encoding, or sanitize server-side with a trusted HTML sanitizer library before any raw rendering.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Concepts**
- Form tag helpers emitting antiforgery tokens automatically
- `fetch`/jQuery AJAX requiring manual token inclusion
- `[AutoValidateAntiforgeryToken]` rejecting missing tokens before action runs

**Answer**

Form tag helpers emit antiforgery tokens automatically, but `fetch` and jQuery AJAX must manually send the `RequestVerificationToken` header or `__RequestVerificationToken` form field — otherwise POSTs fail with 400 antiforgery errors. I read the hidden field value from the page and include it on every mutating AJAX request.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Concepts**
- Hub not registered in DI; instantiated by SignalR per invocation with connection context
- `IHubContext<THub>` as correct singleton proxy
- Hub requiring active connection context that DI-resolved instance lacks

**Answer**

Hubs are not registered in DI for direct injection into controllers — they are instantiated by SignalR per invocation with connection context. Injecting a concrete `Hub` fails activation or produces an instance without active connection state. I inject `IHubContext<THub>` instead, which is registered as a singleton proxy by `AddSignalR()`.

---

#### Gotcha 15. SignalR scale-out without backplane

**Concepts**
- Sticky sessions routing connections but not cross-instance messages
- Redis backplane as pub/sub fan-out across all instances
- Group membership and connection IDs local to each instance

**Answer**

Sticky sessions alone do not fan-out events across server instances — they route connections to the same node but do not relay messages. A controller on instance A calling `IHubContext.Clients.User(id).SendAsync` misses users connected to instance B without a backplane. Multi-node deployments need a Redis backplane via `AddStackExchangeRedis` or Azure SignalR Service so messages sent from any instance reach clients on all instances.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) Review this live auction hub. Bidding works in dev with one user; under load, bids from one user appear on another user's screen and `InvalidOperationException` surfaces in logs about scoped services.

```csharp
public class AuctionHub : Hub
{
    private readonly IAuctionService _auction; // registered Scoped
    private static readonly Dictionary<string, decimal> _highBids = new();

    public AuctionHub(IAuctionService auction) => _auction = auction;

    public async Task PlaceBid(string lotId, decimal amount)
    {
        _highBids[lotId] = amount;
        await _auction.RecordBidAsync(lotId, amount, Context.UserIdentifier);
        await Clients.Others.SendAsync("BidPlaced", lotId, amount);
    }
}
```

`Program.cs`: `builder.Services.AddScoped<IAuctionService, AuctionService>();` and `app.MapHub<AuctionHub>("/hubs/auction");`

---

**Answer**

The hub is constructed as a transient per invocation while holding a scoped `IAuctionService` — injecting a scoped service into a transient type resolved outside a request scope causes an `InvalidOperationException` or a captive dependency where the scoped instance lives longer than intended. The static `Dictionary<string, decimal>` is a second problem: it is shared across all hub invocations across all users, so concurrent writes corrupt state and User A's bid becomes visible as User B's, since the dictionary is not thread-safe and is keyed without isolation. The fix is to remove the static dictionary entirely and persist authoritative bid state in the scoped service backed by a database or Redis with optimistic concurrency. The `IAuctionService` should be resolved through a scoped factory or the hub's DI-managed scope rather than direct constructor injection, and after a successful persist the service broadcasts via `IHubContext` from outside the hub rather than inside. Hubs should be thin transport layers — authoritative bid state does not belong on hub fields.

---

#### Q2. (P) You deploy four MVC instances behind an Azure Application Gateway. Users on instance 2 never receive chat messages sent from a controller on instance 4. Walk through **Redis backplane** registration for SignalR — what it synchronizes, what it does not, and one production misconfiguration that silently breaks fan-out.

---

**Answer**

Without a backplane, each instance only knows its own connection IDs — `IHubContext.Clients.Group("room").SendAsync` on instance 4 reaches only sockets connected to instance 4. Redis pub/sub propagates the message to all instances so each forwards to its local connections. I register it via `builder.Services.AddSignalR().AddStackExchangeRedis(configuration["Redis:Connection"], options => { options.Configuration.ChannelPrefix = "MyApp:SignalR:"; })`. The backplane synchronizes hub message fan-out across groups, all-clients, and user targets — it does not replicate connection-local state, group membership lists, or custom static registries I built outside SignalR APIs. Each instance still maintains its own connection map; Redis carries the instruction, not the WebSocket. The most common silent production failure is sharing the same `ChannelPrefix` across staging and production environments running on the same Redis cluster — cross-talk means staging events reach production connections or messages are swallowed, neither of which produces an exception. Alternatively, a Redis ACL or firewall that blocks pub/sub while allowing cache GET commands lets negotiate succeed but cross-instance sends never arrive.

---

#### Q3. (D) Ops proposes **sticky sessions (session affinity)** on the load balancer instead of a SignalR backplane to save Redis cost. The app is a stock ticker dashboard with MVC Razor views and a SignalR hub. What works with sticky sessions alone, what fails on instance recycle or deploy, and when is affinity acceptable vs when is backplane or Azure SignalR mandatory?

---

**Answer**

Sticky sessions keep a user's WebSocket on one instance, so server-originated messages from that same instance — for example, a timer on that node pushing price ticks to its local connections — can work without a backplane. Any message originating on another instance fails to reach the socket, since the sending instance has no socket for that user. In a stock ticker dashboard where a background worker or MVC controller on any instance publishes price updates, that worker is frequently not on the same instance as the connected client, so delivery fails silently. On rolling deploy or instance drain the affinity cookie may land the reconnecting client on a different node, group memberships are gone, and the client must rejoin regardless. Affinity is acceptable for single-instance deployments, low-traffic internal tools, or prototypes where all real-time events genuinely originate on the same node as every socket. A backplane or Azure SignalR becomes mandatory when multiple instances are running and events originate outside the connection's host process — which covers any controller-triggered broadcast, Redis stream consumer, Hangfire job, or one user messaging another on a different node. Sticky sessions are a connection routing tool, not a pub/sub solution.

---

#### Q4. (R) Review hub authorization. Authenticated users on the MVC site can open `/Dashboard`, but the SignalR connection succeeds as anonymous and `Context.User.Identity.Name` is null in hub methods.

```csharp
// ChatHub.cs — no attributes on class
public class ChatHub : Hub
{
    public async Task SendMessage(string room, string text)
    {
        var user = Context.User.Identity!.Name;
        await Clients.Group(room).SendAsync("ReceiveMessage", user, text);
    }
}

// Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
builder.Services.AddSignalR();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/hubs/chat");
app.MapControllerRoute(/* ... */);
```

---

**Answer**

The hub has no `[Authorize]` attribute, so anonymous connections are accepted even though authentication middleware is registered — the middleware runs but the hub does not reject unauthenticated callers. A second problem is that the client likely connects without sending the auth cookie, because the default `withUrl` call does not include `{ withCredentials: true }` for same-origin cookie auth. Both issues must be fixed: I add `[Authorize]` to `ChatHub` so the negotiate request is rejected with 401 for unauthenticated users, and the client connects with `{ withCredentials: true }`. For cross-origin SPA or JWT bearer scenarios I use `accessTokenFactory` instead of credentials, and configure the JWT bearer events to read the token from the query string on `/negotiate` since WebSockets cannot always send `Authorization` headers. Inside hub methods I use `Context.UserIdentifier` or `Context.User.FindFirst(ClaimTypes.NameIdentifier)` rather than trusting client-supplied identity strings.

---

#### Q5. (R) Review group membership wiring. Some users never receive room messages; logs show `OnConnectedAsync` completed but `Groups.AddToGroupAsync` ran after the client already called `JoinRoom`.

```csharp
public class RoomHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var room = Context.GetHttpContext()?.Request.Query["room"].ToString();
        if (!string.IsNullOrEmpty(room))
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
        await base.OnConnectedAsync();
    }

    public Task JoinRoom(string room) =>
        Groups.AddToGroupAsync(Context.ConnectionId, room);
}
```

```javascript
// client.js — runs immediately after connection.start()
connection.on("ReceiveMessage", (user, msg) => render(user, msg));
connection.invoke("JoinRoom", "support-42");
connection.start();
```

---

**Answer**

The client calls `connection.invoke("JoinRoom", ...)` before `connection.start()` completes — the invocation is dropped or races the connection handshake, so the user is not in the group when the first server messages arrive. The fix is to await `start()` before any invoke:

```javascript
await connection.start();
await connection.invoke("JoinRoom", "support-42");
```

A second issue is that two join paths exist — `OnConnectedAsync` checks the query string and `JoinRoom` handles an explicit client call — creating timing ambiguity and potential double-adds. I prefer one authoritative join path: either pass `room` on connect via query string and handle it in `OnConnectedAsync`, or expose `JoinRoom` called after start, not both without coordination. Having the hub return an ack from `JoinRoom` — `public async Task<string> JoinRoom(string room)` — lets the client await the invoke result before showing the "connected" state to the user. On reconnect, the client must re-invoke join since groups are not persisted across connection IDs.

---

#### Q6. (P) After a brief network blip, the MVC dashboard reconnects automatically but the user's watchlist group membership and server-side "last seen price" state are gone until they refresh the page. Explain **what SignalR resets on reconnect**, what the server must rehydrate, and a production pattern to restore group membership without a full page reload.

---

**Answer**

Reconnect assigns a new `ConnectionId` — all prior group memberships and connection-scoped server dictionaries keyed by the old ID are invalid. Automatic reconnect restores the transport only, not application group state. Anything keyed by connection ID resets: group membership, hub instance fields tied to the old ID, any custom `ConcurrentDictionary<connectionId, …>` maintained outside SignalR. Durable data persists if it is keyed by user ID in database or Redis rather than connection ID. The server must rehydrate by re-adding the connection to groups in `OnConnectedAsync` and pushing a snapshot of last-known state to the caller:

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

The client handles `connection.onreconnected` and invokes `JoinWatchlist(symbols)` mirroring the initial connect flow. I enable `WithAutomaticReconnect()` with a backoff policy and show a "Reconnecting..." UI state until the snapshot arrives so the user is not confused by stale prices.

---

#### Q7. (R) Review broadcasting from an MVC controller after an admin action. The notification never reaches connected browsers; no exception is thrown.

```csharp
public class OrdersController : Controller
{
    private readonly OrderHub _hub; // concrete hub injected

    public OrdersController(OrderHub hub) => _hub = hub;

    [HttpPost]
    public async Task<IActionResult> Ship(int id)
    {
        await _orders.MarkShippedAsync(id);
        await _hub.Clients.All.SendAsync("OrderShipped", id);
        return RedirectToAction("Index");
    }
}

public class OrderHub : Hub { }
```

---

**Answer**

Controllers must not inject concrete `Hub` types — hubs are not registered in DI for direct resolution, and the `Clients` property on a hub instance is only valid during an active SignalR invocation, not when the hub is resolved as a DI dependency. Even if DI does not fail activation outright, `_hub.Clients` is null or has no connection context, so `SendAsync` silently sends to nobody. The correct pattern is `IHubContext<OrderHub>`, which is registered as a singleton proxy by `AddSignalR()` and is specifically designed for sending from outside hub context. I inject `IHubContext<OrderHub> _hubContext` into the controller and call `await _hubContext.Clients.All.SendAsync("OrderShipped", id)`. For multi-instance deployments I add a Redis backplane so the controller on any node reaches connections on all nodes. The hub class stays as `public class OrderHub : Hub { }` with no DI registration needed.

---

#### Q8. (R) A separate SPA on `https://app.example.com` calls your MVC site's SignalR hub at `https://api.example.com/hubs/notifications`. Browser console shows CORS error on the negotiate request; direct WebSocket works in Postman.

```csharp
builder.Services.AddCors(o => o.AddPolicy("Spa", p =>
    p.WithOrigins("https://app.example.com")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();
app.UseCors("Spa");
app.MapHub<NotificationHub>("/hubs/notifications");
```

*(SPA client uses `withUrl("https://api.example.com/hubs/notifications")` without credentials.)*

---

**Answer**

The CORS policy is missing `.AllowCredentials()`, which is required when the browser sends cookies or authorization headers on the negotiate request. Without it the browser blocks the preflight or strips the auth, causing the connection to fail at negotiate. Postman bypasses CORS entirely, which is why it works there. For cookie auth I add `.AllowCredentials()` to the policy and the client connects with `{ withCredentials: true }`. Wildcard `AllowAnyOrigin()` cannot be combined with `AllowCredentials()` — the origin must be listed explicitly. For JWT bearer I use `accessTokenFactory` on the client instead of the credentials flag, since the token travels in a query parameter on the negotiate request. `app.UseCors("Spa")` must be called before `MapHub` so the policy applies to negotiate and the WebSocket upgrade paths.

---

#### Q9. (P) Traffic grows to 50k concurrent connections across regions. Team evaluates **Azure SignalR Service** vs self-hosted hubs with Redis backplane on the MVC app. Compare connection ownership, deployment model (`AddAzureSignalR`), sticky-session requirements, and one scenario where Azure SignalR simplifies ops but changes how you broadcast from controllers.

---

**Answer**

Azure SignalR Service hosts and scales connections in the managed service while the MVC app runs hub logic as a worker — connection memory and WebSocket handling move off the app servers. With self-hosted plus Redis, the app processes own all sockets and Redis carries the fan-out instruction. Registration is `AddSignalR().AddAzureSignalR(connectionString)` and `MapHub` calls are unchanged. Sticky sessions are not required with Azure SignalR since connection ownership moves to the service, whereas self-hosted without backplane needs them and self-hosted with Redis makes them optional. `IHubContext<T>` usage in controllers is unchanged — the SDK forwards messages from the app to the service and then to clients. The key ops difference is that 50k connections no longer consume app server memory or threads, so app servers scale on CPU and business logic independently of socket count. The behavior change is that connection limits and message quotas become Azure SKU concerns, diagnostic logging splits between application insights and the Azure portal, and local development may need the Azure SignalR emulator or a fallback to self-hosted. The trade-off is vendor coupling and upstream endpoint configuration requirements against a significant reduction in infrastructure complexity at scale.

---

#### Q10. (R) Review hub method error handling. Clients receive a generic connection drop; server logs show unhandled exceptions inside hub methods.

```csharp
public class TradeHub : Hub
{
    public async Task SubmitOrder(OrderDto order)
    {
        try
        {
            var result = await _trading.ExecuteAsync(order);
            await Clients.Caller.SendAsync("OrderAccepted", result);
        }
        catch (ValidationException ex)
        {
            throw new Exception(ex.Message); // rethrow to client
        }
        catch (InsufficientFundsException)
        {
            await Clients.Caller.SendAsync("OrderRejected", "Insufficient funds");
        }
    }
}
```

---

**Answer**

Throwing a raw `Exception` in a hub method is the problem. Unhandled exceptions in hub methods terminate the connection by default in SignalR, so the validation error closes the entire socket rather than returning a structured error to the caller. `throw new Exception(ex.Message)` also leaks the exception message text to the client, potentially exposing internal rule details. The `InsufficientFundsException` branch is handled correctly — sending to caller and returning without throwing keeps the connection alive. I apply the same pattern to validation errors: `await Clients.Caller.SendAsync("OrderRejected", new { code = "VALIDATION", message = "..." }); return;`. If the intent is to use an exception-based flow, `throw new HubException("Invalid order")` sends an error to the caller through SignalR's error channel rather than terminating the connection. For consistent error mapping across all hub methods I implement `IHubFilter` with a `try/catch` around `next(context)` that maps known exception types to structured `HubException` payloads and logs server-side with a correlation ID. I never expose stack traces or internal details to clients.

---

#### Q11. (R) Review MVC Razor view and client wiring for a live comment feed. Connection fails in staging (HTTPS, subpath deploy) but works locally on HTTP.

```html
<!-- Views/Comments/Index.cshtml -->
@section Scripts {
    <script src="~/lib/microsoft/signalr/dist/browser/signalr.min.js"></script>
    <script>
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/comments")
            .build();

        connection.on("NewComment", (c) => appendComment(c));
        connection.start();

        document.getElementById("postBtn").onclick = () =>
            connection.invoke("PostComment", document.getElementById("text").value);
    </script>
}
```

*(App deployed at `https://intranet.corp/apps/comments/` behind a reverse proxy; `PathBase` is configured in `Program.cs`; antiforgery required on form posts elsewhere on the site.)*

---

**Answer**

The hard-coded root-relative URL `/hubs/comments` ignores `PathBase` (`/apps/comments`) set by the reverse proxy, so the negotiate request hits the wrong path in staging and returns 404, while locally at the site root the same URL works. The fix is to generate the hub URL server-side so path base is applied: `const hubUrl = "@Url.Content("~/hubs/comments")";`. A second issue is that `connection.start()` is not awaited before `connection.invoke("PostComment", ...)` in the click handler — on slow networks or reconnect this silently fails because invoke runs before the connection is established. I restructure to `connection.start().then(() => setupPostButton(connection))` so the button handler is only wired up after the connection is ready. I also add `app.UseForwardedHeaders()` and `app.UsePathBase("/apps/comments")` before routing in `Program.cs` so the reverse proxy's path and scheme are correctly reflected in hub URL generation and cookie auth.

---
