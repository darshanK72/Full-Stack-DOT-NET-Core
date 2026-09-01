using Microsoft.AspNetCore.SignalR;
using RealTimeSignalR.Models;
using System;
using System.Threading.Tasks;

/*
 * FILE ROLE: Core SignalR hub teaching file.  Covers the Hub<T> typed pattern,
 *            hub lifecycle callbacks, all server-to-client call targets,
 *            group management, and strongly-typed client calls.
 *
 * SECTIONS IN THIS FILE:
 *   1. Hub<T> — typed hub declaration vs untyped Hub base class
 *   2. Hub lifecycle: OnConnectedAsync / OnDisconnectedAsync
 *   3. Client-to-server hub methods (client invokes server)
 *   4. Server-to-client call targets: All, Caller, Others, Group, User
 *   5. Group management: AddToGroupAsync / RemoveFromGroupAsync
 *   6. Per-user targeting with Clients.User (requires authentication)
 */

namespace RealTimeSignalR.Hubs;

/*
 * SECTION 1: HUB<T> — STRONGLY TYPED HUB
 *
 * Hub is the untyped base class.  Server pushes messages via:
 *   await Clients.All.SendAsync("ReceiveMessage", user, msg);
 *   The method name is a magic string — typos compile but fail at runtime.
 *
 * Hub<T> is the typed base class.  Declare an interface T (IChatClient)
 * with one method per server-to-client event.  The server calls:
 *   await Clients.All.ReceiveMessage(user, msg);
 *   The compiler verifies method name, argument count, and types.
 *
 *   Untyped:  public class ChatHub : Hub
 *   Typed:    public class ChatHub : Hub<IChatClient>   ← this file
 *
 * Interface contract → Models/IChatClient.cs
 *
 * Hub instance lifetime — TRANSIENT:
 *   A new ChatHub instance is created for each client method invocation and
 *   disposed immediately after.  Never store mutable state in Hub fields.
 *   Use injected services, Groups (server-side memory), or a cache instead.
 *
 * Dependency injection in Hubs:
 *   Inject services via the constructor exactly as you would in a controller:
 *     public ChatHub(ILogger<ChatHub> logger) { _logger = logger; }
 *   AddSignalR() registers hubs with the DI container.
 */
public sealed class ChatHub : Hub<IChatClient>
{
    /*
     * SECTION 2: HUB LIFECYCLE — OnConnectedAsync / OnDisconnectedAsync
     *
     * OnConnectedAsync
     *   Called after the transport handshake completes and the connection is live.
     *   Use it to: greet the caller, add them to a default group, or load state.
     *
     *   Context.ConnectionId    — unique GUID string for this connection
     *   Context.UserIdentifier  — authenticated user ID (null if anonymous);
     *                             resolved from the NameIdentifier claim by default
     *   Context.User            — ClaimsPrincipal; inspect individual claims here
     *
     * OnDisconnectedAsync(Exception? exception)
     *   Called when the connection closes — gracefully or due to an error.
     *   exception is null for a clean disconnect (client called connection.stop()).
     *   exception is non-null for a transport error or unexpected drop.
     *
     *   All group memberships for this connection are automatically cleared by the
     *   framework after OnDisconnectedAsync returns — no manual cleanup needed.
     *
     * Always call base.OnConnectedAsync() and base.OnDisconnectedAsync() to allow
     * the framework to perform its own cleanup and fire any registered filters.
     */
    public override async Task OnConnectedAsync()
    {
        string connectionId = Context.ConnectionId;                   // unique GUID per connection
        string user = Context.UserIdentifier ?? "anonymous";          // null if unauthenticated

        await Clients.Caller.ReceiveSystemMessage(                    // greet only the new caller
            $"Connected. User: {user} | ConnectionId: {connectionId}");

        await Clients.Others.UserJoined(user);                        // notify all other connections

        await base.OnConnectedAsync();                                // framework lifecycle hook
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string user = Context.UserIdentifier ?? "anonymous";

        // exception is null → clean disconnect; non-null → transport error
        string reason = exception is null ? "disconnected cleanly" : $"error: {exception.Message}";
        await Clients.Others.UserLeft($"{user} ({reason})");          // notify remaining connections

        await base.OnDisconnectedAsync(exception);                    // framework cleanup
    }

    /*
     * SECTION 3: CLIENT-TO-SERVER HUB METHODS
     *
     * Any public method on a Hub<T> subclass is callable by connected clients.
     * Method names are case-insensitive on the server side by convention.
     *
     * JavaScript: two ways to call a hub method
     *
     *   invoke (async, awaits server Task, captures return value):
     *     await connection.invoke("SendMessage", userName, text);
     *
     *   send (fire-and-forget, does not wait for the server Task):
     *     connection.send("SendMessage", userName, text);
     *
     * Returning values from hub methods:
     *   public async Task<string> Echo(string text) { return text; }
     *   const result = await connection.invoke("Echo", "hello");
     *   Only invoke() captures the return value — send() discards it.
     *
     * Streaming (for large or continuous data) uses IAsyncEnumerable<T>
     * or ChannelReader<T> — covered in advanced SignalR topics.
     */
    public async Task SendMessage(string user, string message)
    {
        var chatMessage = new ChatMessage           // build the serializable DTO
        {
            User = user,
            Message = message,
            SentAt = DateTime.UtcNow
        };

        // Clients.Others — push to everyone EXCEPT the sender's connection
        await Clients.Others.ReceiveMessage(chatMessage.User, chatMessage.Message);

        // Clients.Caller — push back only to the sender (echo / confirmation)
        await Clients.Caller.ReceiveMessage("(you)", chatMessage.Message);

        // NOTE: Clients.All would include the sender:
        //   await Clients.All.ReceiveMessage(chatMessage.User, chatMessage.Message);
    }

    /*
     * SECTION 4: SERVER-TO-CLIENT CALL TARGETS
     *
     * The Clients property (IHubCallerClients<IChatClient>) exposes targeting options:
     *
     *   Clients.All                         all connected clients on this hub
     *   Clients.Caller                      only the connection that invoked this method
     *   Clients.Others                      all connections EXCEPT the caller
     *   Clients.Client(connectionId)        one specific connection by its GUID
     *   Clients.Clients(IReadOnlyList)      a list of specific connection IDs
     *   Clients.Group(groupName)            all connections in a named group
     *   Clients.Groups(IReadOnlyList)       all connections across multiple groups
     *   Clients.OthersInGroup(groupName)    group members EXCEPT the caller
     *   Clients.User(userId)                all connections for one authenticated user
     *   Clients.Users(IEnumerable)          all connections for multiple user IDs
     *   Clients.AllExcept(connectionIds)    all clients EXCEPT a list of connection IDs
     *
     * In Hub<IChatClient>, each of these returns IChatClient — the compiler checks
     * method names and argument types at build time.
     *
     * In a plain Hub (untyped), each returns IClientProxy, and you call
     * .SendAsync("MethodName", arg1, arg2) with runtime-only checking.
     */
    public async Task BroadcastAnnouncement(string message)
    {
        await Clients.All.ReceiveSystemMessage(message);              // push to every connection
    }

    /*
     * SECTION 5: GROUP MANAGEMENT
     *
     * Groups are named sets of connection IDs maintained in server memory.
     * One connection can belong to any number of groups simultaneously.
     * Groups are NOT persisted — they live in the process and reset on restart.
     * In a scaled-out (multi-server) deployment, use a backplane (Redis) so that
     * group membership is visible across all server instances.
     *
     * Groups.AddToGroupAsync(connectionId, groupName)
     *   Adds the connection to the group.  Creates the group if it does not exist.
     *   Can only be called inside a Hub method (not from IHubContext).
     *
     * Groups.RemoveFromGroupAsync(connectionId, groupName)
     *   Removes the connection from the group.  No-op if not a member.
     *
     * Framework cleanup: when a connection disconnects, the framework removes it
     * from all groups automatically — OnDisconnectedAsync does not need to.
     *
     * Clients.Group(groupName) targets all current members of the group.
     * Clients.OthersInGroup(groupName) excludes the calling connection.
     */
    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);     // add caller to group
        await Clients.Group(roomName).ReceiveSystemMessage(               // notify all room members
            $"{Context.UserIdentifier ?? "A user"} joined room '{roomName}'");
    }

    public async Task LeaveRoom(string roomName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);    // remove from group
        await Clients.Group(roomName).ReceiveSystemMessage(                   // notify remaining members
            $"{Context.UserIdentifier ?? "A user"} left room '{roomName}'");
    }

    public async Task SendToRoom(string roomName, string message)
    {
        string sender = Context.UserIdentifier ?? "anonymous";
        await Clients.Group(roomName).ReceiveMessage(sender, message);    // push only to group members
    }

    /*
     * SECTION 6: PER-USER TARGETING — Clients.User (REQUIRES AUTHENTICATION)
     *
     * Clients.User(userId) targets ALL connections belonging to one authenticated
     * user.  A single user can have multiple simultaneous connections (multiple
     * browser tabs, mobile + desktop) — User() reaches all of them at once.
     *
     * Context.UserIdentifier is resolved from the ClaimTypes.NameIdentifier claim.
     * To use a different claim (e.g. email), implement IUserIdProvider:
     *   services.AddSingleton<IUserIdProvider, EmailBasedUserIdProvider>();
     *
     * If the hub is anonymous (no authentication configured), UserIdentifier is null
     * and Clients.User("anyId") sends to nobody — it does not throw an exception.
     *
     * For authentication setup (query-string JWT token) see Program.cs SECTION 3.
     */
    public async Task SendPrivateMessage(string targetUserId, string message)
    {
        string sender = Context.UserIdentifier ?? "anonymous";

        await Clients.User(targetUserId).ReceiveMessage(              // push to all connections of target user
            sender, $"(private) {message}");

        await Clients.Caller.ReceiveMessage(                          // echo back to sender
            "(you → private)", message);
    }
}
