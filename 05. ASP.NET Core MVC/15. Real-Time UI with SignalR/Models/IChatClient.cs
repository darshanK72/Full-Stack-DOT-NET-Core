using System.Threading.Tasks;

/*
 * FILE ROLE: Defines the typed hub client interface used by Hub<IChatClient>.
 *            Replaces magic-string SendAsync calls with compiler-verified method calls.
 *
 * SECTIONS IN THIS FILE:
 *   1. IChatClient — typed client contract for ChatHub
 */

namespace RealTimeSignalR.Models;

/*
 * SECTION 1: TYPED HUB CLIENT INTERFACE (IChatClient)
 *
 * Hub<T> requires T to be an interface where every method returns Task.
 * Task<TResult> is NOT allowed — server-to-client pushes are fire-and-forget;
 * SignalR does not support a return value from client to server on a push call.
 *
 * Naming convention: I{HubName}Client  → IChatClient, INotificationClient
 *
 * Contrast with untyped SendAsync:
 *   Untyped:  await Clients.All.SendAsync("ReceiveMessage", user, msg);
 *               - method name is a string literal — typos compile silently
 *               - parameter count/type mismatch is a runtime error
 *
 *   Typed:    await Clients.All.ReceiveMessage(user, msg);
 *               - compiler checks method name, count, and argument types
 *               - refactoring renames propagate automatically
 *
 * The JavaScript client registers handlers matching these method names
 * (case-insensitive on the server side):
 *   connection.on("ReceiveMessage",       (user, message) => { ... });
 *   connection.on("ReceiveSystemMessage", (msg)           => { ... });
 *   connection.on("UserJoined",           (user)          => { ... });
 *   connection.on("UserLeft",             (user)          => { ... });
 */
public interface IChatClient
{
    Task ReceiveMessage(string user, string message);   // server pushes a chat message to client(s)
    Task ReceiveSystemMessage(string message);          // server pushes a system/status notification
    Task UserJoined(string user);                       // broadcast: a new connection was established
    Task UserLeft(string user);                         // broadcast: a connection was closed
}
