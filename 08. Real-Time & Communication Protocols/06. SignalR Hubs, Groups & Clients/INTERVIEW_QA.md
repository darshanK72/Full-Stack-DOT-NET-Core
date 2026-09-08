# SignalR Hubs, Groups & Clients — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — SignalR Hubs, Groups & Clients (Interview Traps)](#gotchas--signalr-hubs-groups--clients-interview-traps)

---

## Gotchas — SignalR Hubs, Groups & Clients (Interview Traps)

---

#### Gotcha 1. Group Membership Is Not Persisted Across Client Reconnects

**Concepts**
- Group membership is stored in an in-process dictionary keyed by ConnectionId
- A reconnected client receives a new ConnectionId and is no longer in any group
- OnConnectedAsync must re-add the client to all their groups from a durable store
- Groups.AddToGroupAsync only adds the current ConnectionId; previous IDs are irrelevant

**Answer**

SignalR groups are in-memory mappings from a group name to a set of connection IDs. When a client reconnects — after a network drop or a browser tab restore — it goes through a full new negotiate cycle and receives a fresh connection ID. The previous connection ID, along with all its group memberships, is purged from memory. The reconnected client is in no groups until the application explicitly re-adds it. The correct pattern is to store group membership in a durable store (database, Redis) keyed by the user's stable identity, and in `OnConnectedAsync` to query that store and call `Groups.AddToGroupAsync(Context.ConnectionId, groupName)` for each group the user belongs to. Applications that add clients to groups only in a dedicated hub method (e.g. "join room") will silently drop users from rooms on every reconnect.

---

#### Gotcha 2. Clients.All Includes the Calling Client — Use Clients.Others to Exclude It

**Concepts**
- Clients.All sends to every connected client including the one that triggered the hub method
- Clients.Others sends to all connected clients except the caller
- Clients.AllExcept(IReadOnlyList<string> connectionIds) excludes a specific set of connections
- Sending an echo back to the caller via Clients.All causes the client to receive its own message twice

**Answer**

`Clients.All.SendAsync(method, payload)` delivers the message to every connected client, including the connection that invoked the hub method. In chat or collaborative editing scenarios, the calling client often already applied the action locally (optimistic update) and should not receive the event again from the server. Using `Clients.All` in this case causes the caller to process its own message twice, leading to duplicated entries in the UI. `Clients.Others` is the correct choice when the server broadcasts a change triggered by a specific client and that client should not receive the echo. `Clients.AllExcept(connectionIds)` is available for finer-grained exclusion. This distinction is conceptually simple but frequently missed by developers reading SignalR examples that use `Clients.All` for brevity without explaining the caller-echo implication.

---

#### Gotcha 3. IHubContext Must Be Used to Send Messages from Outside the Hub — Injecting the Hub Class Itself Does Not Work

**Concepts**
- Hub instances are transient and created only during method invocations; they cannot be retained
- IHubContext<THub> is the correct DI service for sending hub messages from background services
- IHubContext<THub, TClient> for strongly-typed hubs provides compile-time safety on the client interface
- Injecting IHubContext into the hub constructor itself creates a circular reference

**Answer**

ASP.NET Core creates hub instances on demand for method invocations and disposes them immediately. A background service that tries to hold a reference to a hub instance or inject the hub class via DI will receive null or a stale object that is no longer connected to any clients. The correct approach is to inject `IHubContext<MyHub>` into the background service's constructor via DI. `IHubContext` provides `Clients` and `Groups` properties that operate on live connections. For strongly-typed hubs (those inheriting `Hub<TClient>`), use `IHubContext<MyHub, TClient>` to get compile-time-checked client method calls. Injecting `IHubContext` into the hub's own constructor creates a circular dependency and throws a DI resolution error at startup.

---

#### Gotcha 4. OnDisconnectedAsync Must Explicitly Remove the Client from All Groups

**Concepts**
- SignalR does not automatically remove a disconnected connection from groups
- Group membership entries for closed connections remain until the server is restarted
- Stale entries do not cause errors but accumulate memory and can interfere with Clients.Group calls
- OnDisconnectedAsync is the correct location for group cleanup

**Answer**

When a client disconnects, SignalR fires `OnDisconnectedAsync` but does not automatically remove the connection from any groups it was in. The stale connection ID remains in the group's membership set. While `Clients.Group(groupName).SendAsync(...)` will skip dead connections silently, the stale entries accumulate over time and consume memory. If groups are persisted across restarts in an external store, stale entries in the store can cause confusing behavior when group-membership queries return connection IDs that no longer exist. The correct pattern is to call `Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName)` for every group the client was in, inside `OnDisconnectedAsync`. If group membership is stored in an external store, that record must also be cleaned up in the same method.

---

#### Gotcha 5. Strongly-Typed Hub Client Interface Methods Must Match the Exact JavaScript invocation Names

**Concepts**
- Hub<TClient> binds the C# interface method names to JavaScript client-side event names
- Method names are case-sensitive by default in the JavaScript client
- A mismatch between C# interface method name and JavaScript connection.on() name silently drops messages
- Using PascalCase on the server and camelCase on the client causes the mismatch

**Answer**

When using `Hub<IMyHubClient>`, the C# interface defines the method names the server will invoke on clients. The JavaScript client registers handlers using `connection.on("MethodName", handler)`. The method name used in `connection.on()` must exactly match the C# interface method name as serialised by the hub protocol — by default, the JSON hub protocol lowercases the first letter (camelCase), so C# `ReceiveMessage` becomes `"receiveMessage"` on the wire. If the JavaScript client registers `connection.on("ReceiveMessage", ...)` with an uppercase R and the server sends `"receiveMessage"`, the handler never fires and no error is raised. Always verify the exact wire-format method name in the browser's network WebSocket frame inspector when handlers appear to not be receiving messages.

---

#### Gotcha 6. Clients.User() Requires the IUserIdProvider to Return a Consistent Non-Null Value

**Concepts**
- Clients.User(userId) delivers a message to all connections associated with that user identifier
- SignalR uses IUserIdProvider to map a ClaimsPrincipal to a user ID string
- Default implementation returns the NameIdentifier claim value; missing claim returns null
- Null or inconsistent user IDs cause Clients.User() calls to silently drop messages

**Answer**

`Clients.User(userId)` routes messages to all connections that belong to the user identified by `userId`. SignalR resolves the user identifier for each connection through `IUserIdProvider`, which by default extracts the `ClaimTypes.NameIdentifier` claim from the connection's `ClaimsPrincipal`. If that claim is absent from the JWT — for example because the token was generated with `sub` instead of `nameidentifier`, or the claim was mapped differently in the authentication configuration — `GetUserId()` returns null and the connection is not indexed under any user identifier. `Clients.User(userId)` calls with a valid user ID then produce no delivery and no error. The fix is to verify the JWT claim structure matches the identifier the application uses, or to implement a custom `IUserIdProvider` that extracts the correct claim.

---

#### Gotcha 7. Sending to a Non-Existent ConnectionId or GroupName Silently Drops the Message

**Concepts**
- Clients.Client(connectionId).SendAsync() with a stale or non-existent ConnectionId does nothing
- Clients.Group(groupName).SendAsync() with an empty or non-existent group delivers to nobody
- No exception is thrown and no return value indicates delivery failure
- SignalR follows a fire-and-forget delivery model; delivery acknowledgment requires application-level protocol

**Answer**

SignalR's client proxy methods are fire-and-forget at the framework level. Calling `Clients.Client("some-stale-id").SendAsync("Event", payload)` when that connection no longer exists returns a completed task immediately — no exception, no indication that zero clients received the message. Similarly, `Clients.Group("empty-group").SendAsync(...)` silently delivers nothing. Applications that need delivery confirmation — "did the user actually receive this message?" — must implement an application-level acknowledgment: the client sends a hub invocation back to the server confirming receipt, and the server tracks which messages have been acknowledged. Without this, the application can only know it attempted delivery, not whether it succeeded.

---

#### Gotcha 8. Hub Groups Are Not Shared Across Server Instances Without a Backplane

**Concepts**
- Groups.AddToGroupAsync stores membership only in the current process's memory
- In a multi-server deployment, a client added to a group on Server A is invisible to Server B
- Messages sent to that group on Server B are not delivered to the client on Server A
- Redis backplane (StackExchangeRedis) or Azure SignalR Service solve cross-server group fan-out

**Answer**

In a single-server deployment, `Groups.AddToGroupAsync` adds the connection to an in-memory dictionary and group messages are delivered correctly. Behind a load balancer with two or more servers, each server maintains its own independent group-to-connection map. A client connected to Server A is a member of group "room-42" only on Server A. When a hub method on Server B sends to group "room-42", Server B's map shows no connections in that group and delivers nothing. The Redis backplane solves this by using Redis pub/sub to fan out group messages to all servers, each of which then delivers to its local connections in the group. Without the backplane, group messaging silently breaks the moment a second server instance is added.

---

#### Gotcha 9. Hub Method Parameter Binding Fails Silently for Complex Types with Missing Properties

**Concepts**
- SignalR deserialises hub method parameters from the JSON hub protocol message
- Missing required properties in the incoming JSON are set to their default values, not rejected
- An optional property that becomes required in a new version silently receives null or zero
- Validation attributes on hub method parameters are not enforced by the framework automatically

**Answer**

When a JavaScript client invokes a hub method with a JSON payload, SignalR deserialises the JSON into the C# method's parameter types. If the incoming JSON is missing a property that the C# type declares as non-nullable — for example, a new required field added to the type after the client was last deployed — the deserialisation silently assigns the default value (null for reference types, 0 for integers) rather than rejecting the invocation. The hub method then processes partially-populated objects and may produce incorrect results or null-reference exceptions that appear unrelated to the root cause. Hub methods should explicitly validate their parameters and return a meaningful error to the caller when required fields are absent. Using `[Required]` and `Validator.TryValidateObject` inside the hub method provides this defence since SignalR does not call model validators automatically.

---

#### Gotcha 10. Calling hub.Clients Inside OnConnectedAsync Before the Base Method Returns May Miss the Connection

**Concepts**
- OnConnectedAsync must call await base.OnConnectedAsync() before performing any client operations
- The connection is not fully registered until the base method completes
- Sending to Clients.Caller inside OnConnectedAsync before the base call may fail silently
- Adding to groups before base.OnConnectedAsync completes can cause the group add to be overwritten

**Answer**

`OnConnectedAsync` is called when a new connection is established, but the connection is not fully registered in SignalR's internal tracking until `base.OnConnectedAsync()` has been awaited. Sending to `Clients.Caller` or adding to groups before this point can fail silently because the connection has not yet been registered with the transport layer. The correct pattern is to always call `await base.OnConnectedAsync()` as the first operation in a custom `OnConnectedAsync` override, then perform all group additions and initial messages afterward. Omitting the base call entirely also prevents the connection from being registered at all, causing the hub to function incorrectly for that client's entire session.

---
