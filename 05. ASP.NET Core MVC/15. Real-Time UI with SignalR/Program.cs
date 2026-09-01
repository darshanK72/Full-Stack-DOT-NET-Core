/*
 * TOPIC: Real-Time UI with SignalR
 *
 * WHY IT MATTERS:
 *   HTTP is request/response: the client asks, the server answers — one round trip.
 *   SignalR breaks that model.  The SERVER pushes data to every connected browser
 *   the moment something happens: a chat message arrives, a sensor reading changes,
 *   a trade executes, a collaborator edits a document.  No polling, no page reload.
 *
 *   SignalR abstracts the transport layer — it picks the best available mechanism
 *   (WebSockets, SSE, Long Polling) and falls back automatically.  You write one hub
 *   class; SignalR handles connection management, serialization, reconnection, and
 *   multi-server scaling.
 *
 * WHAT YOU WILL LEARN:
 *    1. Transport negotiation: WebSockets → Server-Sent Events → Long Polling
 *    2. Hub<T> typed hubs vs the plain Hub base class
 *    3. Hub lifecycle: OnConnectedAsync / OnDisconnectedAsync
 *    4. Server-to-client targets: Clients.All, Caller, Others, Group, User
 *    5. Group management: AddToGroupAsync / RemoveFromGroupAsync
 *    6. Strongly typed hubs via IChatClient interface
 *    7. JavaScript client: HubConnectionBuilder, .on(), .invoke(), .send()
 *    8. IHubContext<THub> — push from controllers and background services
 *    9. Authentication with SignalR (query-string JWT token for WebSockets)
 *   10. Scale-out: Redis backplane (PREVIEW)
 *
 * CHAPTER MAP:
 *    1. Transport negotiation overview    → Program.cs     SECTION 1
 *    2. Message DTO                       → Models/ChatMessage.cs
 *    3. Typed hub client interface        → Models/IChatClient.cs
 *    4. Chat hub (typed, groups, lifecycle) → Hubs/ChatHub.cs
 *    5. Notification hub                  → Hubs/NotificationHub.cs
 *    6. Notification service (IHubContext) → Services/NotificationService.cs
 *    7. Home controller (serves chat view) → Controllers/HomeController.cs
 *    8. Notifications controller (HTTP push) → Controllers/NotificationsController.cs
 *    9. JavaScript client                 → Views/Home/Index.cshtml
 *   10. AddSignalR + MapHub               → Program.cs     SECTION 2
 *   11. Authentication (query-string token) → Program.cs   SECTION 3
 *   12. Scale-out: Redis backplane        → Program.cs     SECTION 4
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RealTimeSignalR.Hubs;
using RealTimeSignalR.Services;

/*
 * SECTION 1: SIGNALR TRANSPORT NEGOTIATION
 *
 * SignalR always starts with an HTTP POST to /hub-url/negotiate.
 * The server returns a list of transports it supports and a connection token.
 * The client then selects the best available transport in this priority order:
 *
 * ┌─────────────────────┬──────────────────────────────────────────────────────┐
 * │ Transport           │ Description                                          │
 * ├─────────────────────┼──────────────────────────────────────────────────────┤
 * │ WebSockets          │ Full-duplex TCP connection.  Both sides can send at  │
 * │ (preferred)         │ any time.  Requires HTTP Upgrade or HTTP/2.          │
 * │                     │ Lowest latency; smallest per-message overhead.       │
 * ├─────────────────────┼──────────────────────────────────────────────────────┤
 * │ Server-Sent Events  │ One-way stream: server → client only.                │
 * │ (SSE)               │ Client sends via separate HTTP requests.             │
 * │                     │ Automatic browser reconnect via EventSource.         │
 * │                     │ Fallback when WebSockets are blocked by a proxy.     │
 * ├─────────────────────┼──────────────────────────────────────────────────────┤
 * │ Long Polling        │ Client sends HTTP GET; server holds it open until    │
 * │ (universal fallback)│ data arrives or a timeout fires, then responds.      │
 * │                     │ Client immediately re-polls.                         │
 * │                     │ Works everywhere HTTP works — even on old proxies.   │
 * └─────────────────────┴──────────────────────────────────────────────────────┘
 *
 * Forcing a specific transport (skips negotiation):
 *   .withUrl("/chat-hub", { transport: signalR.HttpTransportType.WebSockets })
 *
 * Disabling specific transports:
 *   .withUrl("/chat-hub", {
 *       transport: signalR.HttpTransportType.WebSockets |
 *                  signalR.HttpTransportType.ServerSentEvents
 *   })
 *
 * skipNegotiation: true  — valid ONLY when forcing WebSockets; saves one HTTP round trip.
 *   .withUrl("/chat-hub", { skipNegotiation: true,
 *                           transport: signalR.HttpTransportType.WebSockets })
 */

/*
 * SECTION 2: ADDSTGINALR + MAPHUB — REGISTERING SIGNALR IN THE PIPELINE
 *
 * AddSignalR() registers:
 *   - IHubContext<THub>          — injectable singleton for external pushes
 *   - Hub<T> activation          — DI-aware hub instantiation per invocation
 *   - Connection manager         — tracks active connections by ID and user
 *   - Message serialization      — System.Text.Json by default
 *
 * Customizing the JSON protocol:
 *   builder.Services.AddSignalR()
 *     .AddJsonProtocol(options => {
 *         options.PayloadSerializerOptions.PropertyNamingPolicy = null; // PascalCase
 *     });
 *
 * MapHub<THub>(path) wires the hub to a URL and creates the /negotiate endpoint.
 *   Convention: lowercase kebab-case paths (/chat-hub, /notifications)
 *   The path must match exactly the JavaScript client's .withUrl(path).
 *
 * SignalR is built into the ASP.NET Core SDK for net8.0.
 * No <PackageReference Include="Microsoft.AspNetCore.SignalR" /> is required.
 *
 * Hub endpoint options (applied via MapHub fluent chain):
 *   app.MapHub<ChatHub>("/chat-hub")
 *      .RequireAuthorization()           // restrict to authenticated users
 *      .WithStatefulReconnect();         // .NET 8 stateful reconnect (preview)
 */
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();                          // register all SignalR services + IHubContext<T>
builder.Services.AddScoped<NotificationService>();      // wrapper around IHubContext<NotificationHub>

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapDefaultControllerRoute();                        // GET / → HomeController.Index → chat page
app.MapHub<ChatHub>("/chat-hub");                       // typed hub: chat messages, groups
app.MapHub<NotificationHub>("/notifications");          // plain hub: server-push notifications

/*
 * SECTION 3: AUTHENTICATION WITH SIGNALR — QUERY-STRING TOKEN
 *
 * Problem: WebSocket and SSE upgrade requests cannot carry custom HTTP headers.
 * JwtBearer middleware reads the token from the Authorization header by default,
 * which WebSocket clients cannot set.
 *
 * Solution: pass the JWT in the query string (?access_token=<jwt>) and
 * extract it in the OnMessageReceived event before the bearer middleware runs.
 *
 * JavaScript client — accessTokenFactory:
 *   new HubConnectionBuilder()
 *     .withUrl("/chat-hub", {
 *         accessTokenFactory: () => localStorage.getItem("jwt") ?? ""
 *     })
 *     .build();
 *   The SignalR JS client automatically appends ?access_token=<token>
 *   to WebSocket and SSE requests when accessTokenFactory is provided.
 *
 * Server-side setup (add Microsoft.AspNetCore.Authentication.JwtBearer):
 *
 *   builder.Services
 *     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 *     .AddJwtBearer(options =>
 *     {
 *         options.Events = new JwtBearerEvents
 *         {
 *             OnMessageReceived = context =>
 *             {
 *                 var token = context.Request.Query["access_token"];
 *                 var path  = context.HttpContext.Request.Path;
 *                 if (!string.IsNullOrEmpty(token) &&
 *                     path.StartsWithSegments("/chat-hub"))
 *                 {
 *                     context.Token = token;   // hand the token to the JwtBearer middleware
 *                 }
 *                 return Task.CompletedTask;
 *             }
 *         };
 *         // also configure TokenValidationParameters for issuer, audience, key...
 *     });
 *
 * Inside the hub after authentication:
 *   Context.UserIdentifier  → NameIdentifier claim value (e.g. "user-123")
 *   Context.User?.FindFirst(ClaimTypes.Email)?.Value  → any other claim
 *
 * Restricting a hub to authenticated users only:
 *   app.MapHub<ChatHub>("/chat-hub").RequireAuthorization();
 *   Or decorate the hub class: [Authorize]
 */

/*
 * SECTION 4: SCALE-OUT — REDIS BACKPLANE (PREVIEW)
 *
 * Problem: with multiple load-balanced SignalR server instances, a message
 * published by instance A is delivered only to clients connected to instance A.
 * Clients connected to instances B and C miss it.
 *
 * Solution: a shared backplane.  All instances subscribe to the same Pub/Sub
 * channel; any published message is forwarded to every instance, which then
 * delivers it to its local connections.
 *
 * Redis backplane setup (add Microsoft.AspNetCore.SignalR.StackExchangeRedis):
 *
 *   builder.Services.AddSignalR()
 *     .AddStackExchangeRedis("localhost:6379", options =>
 *     {
 *         options.Configuration.ChannelPrefix =
 *             RedisChannel.Literal("SignalRDemo");
 *     });
 *
 * Other scale-out options:
 *   Azure SignalR Service    — fully managed PaaS; handles all scale automatically;
 *                              free tier available; recommended for Azure deployments
 *   Azure Service Bus        — alternative message backplane for Azure-hosted apps
 *
 * Backplane limitations:
 *   Groups.AddToGroupAsync must still run on a hub instance (cannot be done via
 *   IHubContext) — group membership state is local per instance until the next
 *   Redis sync.  Use sticky sessions or stateful reconnect for group-sensitive apps.
 *
 * COVERED IN DETAIL LATER → Azure SignalR Service and distributed scale-out.
 */

app.Run();

/*
 * QUICK REFERENCE — SIGNALR SERVER API
 * ─────────────────────────────────────────────────────────────────────────────
 * Registration
 *   builder.Services.AddSignalR()              register SignalR services
 *   app.MapHub<THub>("/path")                  expose hub at /path
 *
 * Hub base classes
 *   Hub                     untyped; SendAsync("MethodName", args)
 *   Hub<T>                  typed; T methods called directly on Clients.*
 *
 * Hub lifecycle
 *   OnConnectedAsync()                 connection established; override + call base
 *   OnDisconnectedAsync(Exception?)    connection closed; exception null = clean
 *
 * Context properties (inside Hub)
 *   Context.ConnectionId               unique GUID string for this connection
 *   Context.UserIdentifier             NameIdentifier claim; null if anonymous
 *   Context.User                       ClaimsPrincipal
 *
 * Server-to-client targets (Hub<T>) — all return T
 *   Clients.All                        every connected client
 *   Clients.Caller                     the invoking connection only
 *   Clients.Others                     all except the caller
 *   Clients.Client(id)                 one connection by GUID
 *   Clients.Group(name)                all in a named group
 *   Clients.OthersInGroup(name)        group members except the caller
 *   Clients.User(userId)               all connections for one auth'd user
 *   Clients.AllExcept(ids)             all except a list of IDs
 *
 * Group management (inside Hub only)
 *   Groups.AddToGroupAsync(connectionId, groupName)
 *   Groups.RemoveFromGroupAsync(connectionId, groupName)
 *
 * IHubContext<THub> (inject outside hub)
 *   _ctx.Clients.All / .Group / .User / .Client   no Caller or Others
 *   _ctx.Clients.All.SendAsync("MethodName", arg)  untyped push
 *
 * JavaScript client (@microsoft/signalr)
 *   const conn = new signalR.HubConnectionBuilder()
 *     .withUrl("/chat-hub")
 *     .withAutomaticReconnect()
 *     .build();
 *   conn.on("ReceiveMessage", (user, msg) => { ... });   register handler
 *   await conn.start();                                   open connection
 *   await conn.invoke("SendMessage", user, msg);          call hub method (awaits)
 *   conn.send("SendMessage", user, msg);                  fire-and-forget call
 *   await conn.stop();                                    close connection
 *   conn.state                                            "Disconnected" | "Connecting" | "Connected" | "Reconnecting"
 *
 * Authentication (query-string token)
 *   JS: .withUrl("/chat-hub", { accessTokenFactory: () => getJwt() })
 *   Server: OnMessageReceived event → context.Token = query["access_token"]
 *
 * Scale-out
 *   .AddStackExchangeRedis(connectionString)   Redis backplane
 *   Azure SignalR Service                      fully managed PaaS alternative
 * ─────────────────────────────────────────────────────────────────────────────
 */
