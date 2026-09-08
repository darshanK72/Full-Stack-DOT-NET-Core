# SignalR Fundamentals — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — SignalR Fundamentals (Interview Traps)](#gotchas--signalr-fundamentals-interview-traps)

---

## Gotchas — SignalR Fundamentals (Interview Traps)

---

#### Gotcha 1. Transport Negotiation Silently Falls Back to Long Polling Without Any Warning

**Concepts**
- SignalR negotiates WebSockets first, then SSE, then Long Polling
- A proxy that strips Upgrade headers causes silent fallback to Long Polling
- The fallback produces no error — the connection succeeds but performance degrades
- connection.transport?.name (JavaScript) or hub connection logs reveal the active transport

**Answer**

SignalR selects the best available transport by sending a preflight POST to the negotiate endpoint. If the response indicates WebSockets are available but the actual upgrade request fails — because a reverse proxy strips the `Upgrade` header — SignalR silently retries with SSE and then with Long Polling. The connection appears to succeed from both client and server perspectives, but all message round-trips carry the much higher latency of Long Polling. Developers running on Kestrel locally never hit this because there is no proxy, and the fallback only surfaces on staging or production environments behind Nginx, HAProxy, or a corporate gateway. Checking the active transport on the JavaScript client with `connection.transport?.name` immediately reveals whether the application is actually using WebSockets or has silently degraded.

---

#### Gotcha 2. Connection ID Is Ephemeral and Must Never Be Stored or Sent to Clients as an Identity

**Concepts**
- Context.ConnectionId is assigned per TCP connection and changes on every reconnect
- Storing a ConnectionId in a database or sending it to another client creates a stale reference
- After reconnect, the old ConnectionId no longer exists and any targeted send to it silently drops
- Context.UserIdentifier from JWT claims provides a stable cross-reconnect identity

**Answer**

`Context.ConnectionId` is a server-generated GUID that identifies one physical connection. It is assigned fresh on every new negotiate-and-upgrade cycle, including automatic reconnects. If you store a ConnectionId in a database as a routing key or broadcast it to other users for targeted messaging, that value becomes stale the moment the connection drops and reconnects. Sending to a stale ConnectionId with `Clients.Client(staleId).SendAsync(...)` silently drops the message — there is no exception. A stable identity for routing and addressing must come from the authenticated user's claims (`Context.UserIdentifier` when using JWT), not from the ephemeral connection ID.

---

#### Gotcha 3. Hub Lifetime Is Per-Invocation, Not Per-Connection

**Concepts**
- ASP.NET Core creates and disposes a hub instance for every method invocation
- State stored as hub instance fields is lost immediately after the method returns
- Connection-scoped state must use IHubContext, a distributed cache, or in-memory stores keyed by ConnectionId
- DI-injected services must be appropriate for per-request or singleton lifetime

**Answer**

ASP.NET Core creates a new hub instance for each incoming method call and disposes it immediately after the method returns. State stored in hub instance fields — `private int _callCount = 0; _callCount++` — is meaningless because the field is reset to its default on every invocation. Developers who come from SignalR for ASP.NET 4 (persistent connection model) or who expect hub objects to persist for the duration of a connection are frequently surprised when instance fields lose their values between calls. Connection-scoped state must live outside the hub: in a concurrent dictionary keyed by `Context.ConnectionId`, in the distributed cache, or in a database. The connection lifetime hooks `OnConnectedAsync` and `OnDisconnectedAsync` also run in separate transient hub instances.

---

#### Gotcha 4. skipNegotiation: true Forces WebSockets and Removes the Fallback Safety Net

**Concepts**
- skipNegotiation skips the preflight POST and attempts WebSocket upgrade directly
- If WebSockets are unavailable in the environment, the connection fails completely with no fallback
- Useful only when WebSocket availability is guaranteed in all client environments
- Reduces one HTTP round trip but eliminates transport resilience

**Answer**

The `skipNegotiation: true` option on the JavaScript `HubConnectionBuilder` bypasses the negotiate preflight request and directly attempts a WebSocket upgrade to the hub URL. This saves one HTTP round trip, which is meaningful for high-frequency reconnect scenarios. The cost is that there is no fallback: if the WebSocket upgrade fails — proxy strip, firewall block, browser restriction — the connection fails entirely with no automatic retry via SSE or Long Polling. For applications deployed in environments where WebSocket support is guaranteed end-to-end (controlled corporate intranet, modern mobile apps with no corporate proxy), `skipNegotiation` is a safe optimisation. For applications deployed to arbitrary internet clients or through corporate gateways, it is a reliability risk.

---

#### Gotcha 5. OnDisconnectedAsync Fires for Both Graceful Closes and Abrupt Network Drops

**Concepts**
- exception parameter is null for graceful closes and non-null for abrupt disconnects
- Both cases trigger the same cleanup path — the code must not assume graceful close
- State cleanup (group removal, active user tracking) must happen in OnDisconnectedAsync regardless
- Reconnect fires OnConnectedAsync with a new ConnectionId before OnDisconnectedAsync completes on the old one

**Answer**

`OnDisconnectedAsync(Exception? exception)` is called for every connection termination, whether the client explicitly closed the connection (exception is null) or the network dropped unexpectedly (exception carries the error). Applications that only clean up state when `exception == null` — assuming graceful closes are the only path that matters — accumulate stale entries whenever a client disconnects abruptly due to a network failure, browser crash, or device sleep. All connection-scoped state cleanup — removing from groups, updating presence indicators, cancelling background tasks — must happen unconditionally in `OnDisconnectedAsync`, regardless of whether the exception parameter is null. The reconnect sequence also creates an overlap: `OnConnectedAsync` for the new connection fires on a different thread while `OnDisconnectedAsync` for the old connection may still be running, so cleanup code must be thread-safe.

---

#### Gotcha 6. Large Messages Are Rejected by Default Due to MaximumReceiveMessageSize Limit

**Concepts**
- Default MaximumReceiveMessageSize is 32 KB per incoming hub message
- Messages exceeding the limit cause a connection abort with a logged error
- The limit must be raised explicitly in AddSignalR().AddHubOptions<T>()
- Large binary payloads should use dedicated HTTP endpoints, not SignalR hub messages

**Answer**

SignalR imposes a default maximum message size of 32 KB per incoming hub invocation to protect against denial-of-service via oversized messages. Sending a JSON payload or binary blob larger than this limit causes the server to abort the connection with an error. This catches developers off-guard when hub methods start receiving larger inputs — file contents, bulk data — that were small in initial testing but grew as usage increased. The limit can be raised with `AddSignalR().AddHubOptions<MyHub>(options => options.MaximumReceiveMessageSize = 512 * 1024)`. However, raising the limit without considering the implications is a mistake; for truly large payloads the correct architectural pattern is to use a dedicated HTTP multipart upload endpoint and then notify connected clients via SignalR that the upload is ready.

---

#### Gotcha 7. MessagePack Protocol Requires All Hub Method Parameters to Be Serialisable Without Object References

**Concepts**
- MessagePack serialises by index/key, not by name like JSON
- Interface types and abstract classes are not serialisable with default MessagePack settings
- Adding a new field without a key attribute shifts indices and breaks binary compatibility
- MessagePack contracts must be versioned carefully in long-lived systems

**Answer**

The optional MessagePack hub protocol offers smaller payload size and faster serialisation than JSON but imposes stricter type constraints. MessagePack serialises fields and properties by position index unless `[MessagePackObject]` and `[Key(n)]` attributes are used. Adding a new property to a hub method parameter type without a key attribute shifts all subsequent property indices, silently corrupting deserialization for clients that have not been updated. Interface or abstract types cannot be serialised without custom formatters. Teams that switch from JSON to MessagePack for performance frequently discover these contract rigidity issues the first time they update a shared type. MessagePack contracts must be treated like Protobuf contracts — append-only, with explicit key assignments — to maintain backward compatibility.

---

#### Gotcha 8. SignalR Requires the Negotiate Endpoint to Be Accessible — JWT Auth on the Whole Path Blocks It

**Concepts**
- The negotiate POST is the first request in the SignalR connection flow
- Adding [Authorize] to the hub or its route also protects the negotiate endpoint
- Unauthenticated clients receive a 401 on negotiate before they can send authentication credentials
- The client must obtain a token before calling HubConnectionBuilder, not during negotiation

**Answer**

When a hub is decorated with `[Authorize]`, the negotiate endpoint at `hub-path/negotiate` is protected by the same policy. An unauthenticated client that attempts to connect receives a 401 on the negotiate POST before it has a chance to supply a token. This is correct behavior — unauthenticated clients should be rejected — but it catches developers who expect to be able to authenticate during the negotiate handshake. The correct pattern is for the client to authenticate separately first (exchange credentials for a JWT with a normal REST endpoint), then pass the token via `accessTokenFactory` in `HubConnectionBuilder` before calling `connection.start()`. The token is then attached to the negotiate POST and the WebSocket upgrade, allowing both to succeed against the protected hub.

---

#### Gotcha 9. Hub Methods Cannot Return IAsyncEnumerable Unless the Client Uses the Streaming API

**Concepts**
- Hub methods returning IAsyncEnumerable<T> activate server-to-client streaming
- The JavaScript client must call connection.stream() not connection.invoke() for streaming methods
- Using invoke() on a streaming method causes the entire stream to be buffered before delivery
- Streaming methods cannot return a single value; the entire response is a sequence

**Answer**

A hub method returning `IAsyncEnumerable<T>` activates SignalR's server-to-client streaming protocol: the server pushes items to the client incrementally as the async enumerator yields them. The JavaScript client must call `connection.stream("MethodName", ...args)` and subscribe to the returned observable to receive items. If the developer calls `connection.invoke("MethodName", ...args)` instead, the entire sequence is buffered on the server into an array before being sent as a single response, defeating the purpose of streaming and potentially causing memory pressure for large sequences. Additionally, streaming methods cannot return both a stream and a completion value — the entire response is the stream. The client must handle stream cancellation via the `CancellationToken` parameter to stop the server enumeration early.

---

#### Gotcha 10. A Hub Connection That Is Never Started Accumulates Registered Event Handlers Without Delivering Events

**Concepts**
- Calling hubConnection.on("EventName", handler) registers a handler but does not start the connection
- hubConnection.start() must be awaited before any events or invocations can flow
- Errors on start() must be caught; unhandled promise rejections on start() are silent in some environments
- The connection must also be explicitly stopped on component unmount to prevent memory leaks

**Answer**

Calling `connection.on("ReceiveMessage", handler)` on a `HubConnection` instance only registers a local event listener. The connection is not live until `connection.start()` is called and its returned Promise resolves. Code that registers handlers and then invokes methods before `await connection.start()` will throw because the underlying transport is not established. Equally common is failing to `await connection.start()` and catching its error — if the server is unavailable or authentication fails, `start()` rejects and unhandled rejection may be swallowed silently in some environments. In component-based UI frameworks, the connection must also be explicitly stopped on component teardown with `connection.stop()` to remove all handlers and close the transport, otherwise the connection and its event listeners continue running after the component is gone, causing memory leaks and phantom event deliveries.

---
