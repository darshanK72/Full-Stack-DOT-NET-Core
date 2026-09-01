using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

/*
 * FILE ROLE: A plain (untyped) Hub that serves as the target endpoint for
 *            IHubContext<NotificationHub> injection in services and controllers.
 *            Demonstrates the IHubContext<THub> pattern overview.
 *
 * SECTIONS IN THIS FILE:
 *   1. NotificationHub — untyped Hub as an IHubContext<THub> target
 *   2. IHubContext<THub> overview — pushing from outside the hub
 */

namespace RealTimeSignalR.Hubs;

/*
 * SECTION 1: NOTIFICATIONHUB — UNTYPED HUB
 *
 * This hub intentionally uses the plain Hub base class (not Hub<T>) to
 * demonstrate the untyped SendAsync pattern and contrast it with ChatHub.
 *
 * Untyped SendAsync:
 *   await Clients.All.SendAsync("ReceiveNotification", message);
 *   The method name "ReceiveNotification" is matched on the JavaScript side:
 *     connection.on("ReceiveNotification", (msg) => { ... });
 *
 * The hub defines Subscribe/Unsubscribe methods so clients can opt in to
 * named channels (groups) and receive targeted pushes.
 * The actual notification sends come from outside the hub via IHubContext
 * — see Services/NotificationService.cs and Controllers/NotificationsController.cs.
 */

/*
 * SECTION 2: IHUBCONTEXT<THUB> OVERVIEW — PUSHING FROM OUTSIDE THE HUB
 *
 * Inside a Hub method, Clients and Groups are available as properties.
 * Outside a hub — in a controller, Razor Page, service, or background job —
 * you cannot instantiate a Hub directly.  Inject IHubContext<THub> instead:
 *
 *   IHubContext<NotificationHub> context
 *     context.Clients.All                    → push to all connected clients
 *     context.Clients.Client(connectionId)   → push to one specific connection
 *     context.Clients.Group(groupName)       → push to a named group
 *     context.Clients.User(userId)           → push to an authenticated user
 *
 * IHubContext<THub> does NOT expose:
 *   - Groups.AddToGroupAsync   (group management requires a live Hub instance)
 *   - Clients.Caller / Others  (no concept of "caller" outside a hub)
 *   - Context                  (no HubCallerContext outside a hub)
 *
 * IHubContext<THub> is registered as a singleton by AddSignalR().
 * Inject it into scoped services or transient controllers without concern —
 * a scoped consumer holding a singleton reference is safe in ASP.NET Core.
 *
 * Full implementation:
 *   Services/NotificationService.cs      — wraps IHubContext, used from services
 *   Controllers/NotificationsController.cs — triggers push from an HTTP POST
 */
public sealed class NotificationHub : Hub
{
    // Subscribe the calling connection to a named channel (backed by a SignalR group)
    public async Task Subscribe(string channel)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, channel);   // join group named after the channel
        await Clients.Caller.SendAsync("Subscribed", channel);         // confirm subscription to caller
    }

    // Unsubscribe the calling connection from a named channel
    public async Task Unsubscribe(string channel)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, channel);   // leave the channel group
        await Clients.Caller.SendAsync("Unsubscribed", channel);            // confirm to caller
    }
}
