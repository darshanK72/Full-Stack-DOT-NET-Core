using Microsoft.AspNetCore.SignalR;
using RealTimeSignalR.Hubs;
using System.Threading.Tasks;

/*
 * FILE ROLE: Wraps IHubContext<NotificationHub> to push real-time notifications
 *            from application services, eliminating direct SignalR coupling in callers.
 *
 * SECTIONS IN THIS FILE:
 *   1. IHubContext<THub> injection — constructor and lifetime notes
 *   2. Sending to All, Group, and User from outside a hub
 */

namespace RealTimeSignalR.Services;

/*
 * SECTION 1: INJECTING IHUBCONTEXT<THUB>
 *
 * IHubContext<THub> is automatically registered as a singleton by AddSignalR().
 * It grants access to the hub's client connection manager without instantiating
 * the hub itself (hubs are transient; they have no persistent instance to reuse).
 *
 * Constructor injection pattern:
 *   The DI container injects the singleton IHubContext<NotificationHub> into
 *   NotificationService.  The service can be registered as any lifetime:
 *     - Scoped (per-request)  — safe; scoped holding singleton is allowed
 *     - Singleton             — equally safe; single hub context reference
 *     - Transient             — fine; context is shared via singleton
 *
 * Background service usage:
 *   Inject IHubContext<THub> directly into a BackgroundService constructor.
 *   Because IHubContext<THub> is a singleton, it is safe to hold it for the
 *   lifetime of a hosted service.  Scoped services (e.g. DbContext) inside a
 *   background service require IServiceScopeFactory instead.
 */
public sealed class NotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;  // singleton — safe to hold long-term

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /*
     * SECTION 2: SENDING FROM OUTSIDE A HUB
     *
     * IHubContext<THub>.Clients mirrors most of the Clients surface from inside a hub,
     * but without Caller or Others (no invoking connection exists outside the hub).
     *
     * Available targets on IHubContext<THub>.Clients:
     *   .All                          — every client connected to NotificationHub
     *   .Client(connectionId)         — one specific connection
     *   .Clients(IReadOnlyList)        — a list of connections
     *   .Group(groupName)             — all connections in a group
     *   .Groups(IReadOnlyList)         — multiple groups
     *   .User(userId)                 — all connections for one authenticated user
     *   .Users(IEnumerable)           — multiple users
     *   .AllExcept(connectionIds)     — everyone except specified connections
     *
     * All return IClientProxy (not IChatClient) because IHubContext<THub> is untyped.
     * Use SendAsync("MethodName", arg) — the method name must match the JavaScript
     * connection.on("ReceiveNotification", handler) registration.
     *
     * Typed IHubContext (advanced):
     *   IHubContext<NotificationHub, INotificationClient> gives typed Clients.
     *   Register: builder.Services.AddSignalR(); then inject the typed variant.
     *   Less commonly used; plain IHubContext<THub> covers most scenarios.
     */
    public async Task BroadcastAsync(string message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);          // push to all connections
    }

    public async Task SendToChannelAsync(string channel, string message)
    {
        await _hubContext.Clients.Group(channel).SendAsync(                               // push to named group only
            "ReceiveNotification", message);
    }

    public async Task SendToUserAsync(string userId, string message)
    {
        await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message); // push to one user's connections
    }
}
