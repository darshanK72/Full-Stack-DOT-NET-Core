/*
 * FILE ROLE: ChatHub — SignalR Hub PREVIEW. Demonstrates the Hub base class,
 *            hub methods (callable from clients), client invocation, and groups.
 *            Full SignalR depth (transports, backplane, IHubContext, typed hubs,
 *            authentication) belongs in a dedicated SignalR chapter.
 *
 * SECTIONS IN THIS FILE:
 *   1. SignalR overview — what it adds over raw WebSockets
 *   2. Hub class — lifetime, Context, Clients, Groups
 *   3. SendMessage — basic hub method (server receives, broadcasts to all)
 *   4. Groups — JoinGroup / SendToGroup pattern
 *
 * COVERED IN DETAIL LATER → SignalR (dedicated topic folder)
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace WebSocketsRealTime.Hubs;

/*
 * SECTION 1: SIGNALR OVERVIEW
 *
 * SignalR is an ASP.NET Core abstraction over real-time transports. It selects the
 * best available transport automatically:
 *
 *   1. WebSockets (preferred — full-duplex, lowest latency)
 *   2. Server-Sent Events (fallback if WebSockets blocked)
 *   3. Long Polling (final fallback — works through all proxies)
 *
 * What SignalR adds over raw WebSockets:
 *   - Hub protocol: JSON or MessagePack RPC (call named methods by string)
 *   - Automatic reconnect with configurable retry policy
 *   - Groups: server-side logical channels (Clients.Group("room1"))
 *   - User identity integration (Clients.User("userId"))
 *   - IHubContext<T>: inject into services/controllers to push outside the hub
 *   - Scale-out via Redis, Azure Service Bus, or SQL backplane
 *
 * Wiring (Program.cs):
 *   builder.Services.AddSignalR();       // registers SignalR services
 *   app.MapHub<ChatHub>("/chathub");     // exposes the hub at this URL
 *
 * Client (JavaScript):
 *   const conn = new signalR.HubConnectionBuilder()
 *       .withUrl("/chathub").build();
 *   await conn.start();
 *   conn.on("ReceiveMessage", (user, msg) => console.log(user, msg));
 *   await conn.invoke("SendMessage", "Alice", "Hello!");
 */

/*
 * SECTION 2: Hub CLASS — LIFETIME AND BUILT-IN PROPERTIES
 *
 * A Hub is instantiated once per client invocation (not per connection).
 * Do NOT store state in hub fields — use external services or Groups instead.
 *
 * Built-in properties (all on the base Hub class):
 *
 *   Context         — IHubCallerContext
 *     .ConnectionId — unique string id for this WebSocket connection
 *     .User         — ClaimsPrincipal (if authenticated)
 *     .Items        — per-connection Dictionary<object, object?> for state
 *
 *   Clients         — IHubCallerClients
 *     .All          — all connected clients
 *     .Caller       — the client that invoked this method
 *     .Others       — all except the caller
 *     .Client(id)   — specific connection by id
 *     .Group(name)  — all clients in a named group
 *
 *   Groups          — IGroupManager
 *     .AddToGroupAsync(connectionId, groupName)
 *     .RemoveFromGroupAsync(connectionId, groupName)
 *
 * Override OnConnectedAsync / OnDisconnectedAsync to run code on connect/disconnect.
 */
public sealed class ChatHub : Hub
{
    /*
     * SECTION 3: SendMessage — HUB METHOD
     *
     * A hub method is a public method on the Hub class. Clients invoke it by name
     * over the SignalR JSON protocol:
     *
     *   Client → Server: { "type": 1, "target": "SendMessage", "arguments": ["Alice","Hi"] }
     *   Server → All:    Clients.All.SendAsync("ReceiveMessage", user, message)
     *
     * "ReceiveMessage" is the client-side event name. Clients register a handler:
     *   conn.on("ReceiveMessage", (user, message) => { ... });
     *
     * Hub methods are async Task — always await hub calls to propagate exceptions
     * and complete the round-trip properly.
     */
    public async Task SendMessage(string user, string message)
    {
        // broadcast to all connected clients including the caller
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    /*
     * SECTION 4: GROUPS — logical channels within a hub
     *
     * Groups let you broadcast to a named subset of clients without managing
     * your own connection-id lists. Groups are ephemeral — they do not persist
     * across server restarts or scale-out nodes (use Redis backplane for that).
     *
     * Pattern:
     *   1. Client calls JoinGroup("room1")
     *   2. Server adds Context.ConnectionId to group "room1"
     *   3. Any hub method can then send to Clients.Group("room1")
     *   4. Client calls LeaveGroup or disconnects — group membership is auto-cleaned on disconnect
     */
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName); // add caller to group
        // notify all group members that someone joined
        await Clients.Group(groupName).SendAsync("GroupJoined", Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("GroupLeft", Context.ConnectionId, groupName);
    }

    public async Task SendToGroup(string groupName, string user, string message)
    {
        // send only to clients in the named group (excludes the caller if not in group)
        await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
    }

    // OnConnectedAsync / OnDisconnectedAsync lifecycle hooks
    public override async Task OnConnectedAsync()
    {
        System.Console.WriteLine($"[SignalR] Connected: {Context.ConnectionId}");
        await base.OnConnectedAsync(); // always call base
    }

    public override async Task OnDisconnectedAsync(System.Exception? exception)
    {
        System.Console.WriteLine($"[SignalR] Disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception); // always call base
    }
}
