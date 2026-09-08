# WebSockets in ASP.NET Core — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — WebSockets in ASP.NET Core (Interview Traps)](#gotchas--websockets-in-aspnet-core-interview-traps)

---

## Gotchas — WebSockets in ASP.NET Core (Interview Traps)

---

#### Gotcha 1. app.UseWebSockets() Must Be Registered Before the Route Handler That Accepts the Upgrade

**Concepts**
- WebSocket upgrade requires the UseWebSockets middleware to intercept the HTTP request first
- Without it, context.WebSockets.IsWebSocketRequest returns false for all requests
- Middleware order in ASP.NET Core is strictly sequential
- A missing UseWebSockets() call produces a 400 Bad Request silently

**Answer**

`app.UseWebSockets()` installs the middleware that watches for HTTP Upgrade requests and marks `HttpContext.WebSockets.IsWebSocketRequest` as true when the upgrade headers are present. If UseWebSockets() is not registered, or is registered after the endpoint middleware that checks IsWebSocketRequest, the check always returns false and the application falls through to returning a 400 or 404 without any meaningful error. This is a pure ordering bug and produces no exception on the server — the developer sees a failed upgrade and has to trace back through the pipeline to spot the missing call.

---

#### Gotcha 2. WebSocket Upgrade Fails Behind a Reverse Proxy That Strips the Upgrade Header

**Concepts**
- HTTP Upgrade requires the Upgrade: websocket and Connection: Upgrade headers to pass through the proxy
- Nginx requires proxy_http_version 1.1 and explicit Upgrade/Connection header pass-through
- AWS ALB supports WebSocket upgrades natively but requires a target-group idle-timeout increase
- A stripped Upgrade header causes a 400 or 101 that immediately closes

**Answer**

WebSocket upgrades begin with a regular HTTP/1.1 request carrying `Upgrade: websocket` and `Connection: Upgrade`. Many reverse proxies — Nginx in its default configuration, for example — use HTTP/1.0 for backend connections and strip hop-by-hop headers including Upgrade and Connection before forwarding. The client never receives the 101 Switching Protocols response and the upgrade silently fails. The fix in Nginx is to add `proxy_http_version 1.1;`, `proxy_set_header Upgrade $http_upgrade;`, and `proxy_set_header Connection "upgrade";` to the relevant location block. AWS ALB supports WebSocket upgrades natively, but its default idle timeout of 60 seconds closes idle WebSocket connections, causing client reconnects that masquerade as application bugs.

---

#### Gotcha 3. Raw WebSockets Do Not Automatically Send Ping Frames — Applications Must Implement Keepalive

**Concepts**
- TCP keepalive operates at OS level but fires only after minutes of inactivity by default
- WebSocket ping/pong frames are application-level heartbeats
- Most load balancers close TCP connections idle for more than 60–120 seconds
- Without keepalive, clients receive an ungraceful disconnect that looks like a server crash

**Answer**

The WebSocket protocol defines ping and pong frames for application-level keepalive, but the raw `WebSocket` class in ASP.NET Core does not send pings automatically. Without periodic pings, a connection that is open but idle will be silently closed by load balancers or middleboxes after their idle timeout fires — typically 60 to 120 seconds. The client receives an abrupt TCP FIN rather than a WebSocket close frame, making the disconnect look like a server crash rather than an expected timeout. The fix is to run a background loop on the server that calls `webSocket.SendAsync(new ArraySegment<byte>(Array.Empty<byte>()), WebSocketMessageType.Text, true, ct)` or uses the ping frame type on a schedule shorter than the load balancer's idle timeout.

---

#### Gotcha 4. The WebSocket Close Handshake Must Be Completed by Both Sides

**Concepts**
- A graceful WebSocket close requires each side to send a Close frame and receive one back
- Closing without waiting for the remote Close frame leaves the connection in a half-closed state
- WebSocketReceiveResult.MessageType == WebSocketMessageType.Close signals the remote close
- Aborting instead of closing leaves ports in CLOSE_WAIT on the server

**Answer**

A correct WebSocket close requires a two-way handshake: one side sends a Close frame, the other side sends a Close frame back, and then both sides close the underlying TCP connection. In ASP.NET Core, after receiving a message with `MessageType == WebSocketMessageType.Close`, the application must call `webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, ..., ct)` to complete the handshake. Failing to respond to the Close frame leaves the server's TCP socket in a half-open state, slowly accumulating CLOSE_WAIT sockets that consume file descriptors. Conversely, calling `CloseAsync` without first draining pending receive messages can deadlock if the remote side is simultaneously sending data while waiting for the close acknowledgement.

---

#### Gotcha 5. WebSocket.SendAsync Is Not Thread-Safe — Concurrent Sends Throw an Exception

**Concepts**
- Only one outstanding SendAsync or ReceiveAsync call per WebSocket is allowed at a time
- Concurrent SendAsync calls from multiple threads throw InvalidOperationException
- A Channel<T> or SemaphoreSlim is the standard serialisation pattern
- This also applies to ReceiveAsync — do not call it from more than one task simultaneously

**Answer**

The `WebSocket` class in .NET has a strict one-operation-at-a-time constraint: only one `SendAsync` and one `ReceiveAsync` call may be outstanding simultaneously. Calling `SendAsync` from two threads concurrently throws `InvalidOperationException: There is already one outstanding 'SendAsync' call for this WebSocket instance`. This is easy to trigger in notification scenarios where multiple independent producers want to push messages to the same connection. The standard fix is to route all outgoing messages through a `Channel<ReadOnlyMemory<byte>>`: producers write to the channel and a dedicated consumer task drains it with a single sequential loop of `SendAsync` calls, ensuring only one send is in flight at any time.

---

#### Gotcha 6. Receiving a Partial Message Is Normal — Applications Must Reassemble Multi-Frame Messages

**Concepts**
- WebSocket.ReceiveAsync fills the provided buffer and returns EndOfMessage = false for partial reads
- A single logical message may be split across multiple physical WebSocket frames
- Not looping until EndOfMessage = true causes truncated message handling
- The buffer must be grown or the loop must append into a MemoryStream until the message is complete

**Answer**

`WebSocket.ReceiveAsync` fills whatever buffer you provide and returns a `WebSocketReceiveResult` with `EndOfMessage = false` if the message was larger than the buffer. A single logical text or binary message may span multiple frames. An application that reads once and processes the partial buffer will silently handle truncated messages. The correct receive loop continues calling `ReceiveAsync` and accumulates bytes until `result.EndOfMessage == true`. For variable-length messages a `MemoryStream` or a `PipeWriter` is the practical accumulation strategy. Setting a large buffer and hoping messages always fit is fragile; the maximum message size is unlimited by the protocol and controlled only by the sender.

---

#### Gotcha 7. WebSocket Subprotocol Must Be Negotiated Explicitly If the Client Requests One

**Concepts**
- Client requests a subprotocol via Sec-WebSocket-Protocol header
- Server must select one of the offered subprotocols in the AcceptWebSocketAsync call
- If the server does not respond with a subprotocol the client requested, it closes the connection
- SignalR uses its own hub protocol subprotocol; raw WebSocket endpoints ignore it by default

**Answer**

Clients that expect a specific WebSocket subprotocol — such as `graphql-transport-ws` or `mqtt` — include a `Sec-WebSocket-Protocol` header in the upgrade request. The server must respond by selecting one of the offered values in `AcceptWebSocketAsync(subprotocol)`. If the server calls `AcceptWebSocketAsync()` with no subprotocol but the client requires one, the client may immediately close the connection after the 101 response because the protocol negotiation failed. This is invisible at the HTTP level since the 101 succeeds, but the WebSocket closes within milliseconds. The trap is that in testing with a generic WebSocket client that does not enforce subprotocol matching, the connection works fine — the mismatch only surfaces against strict clients.

---

#### Gotcha 8. wss:// Is Required in Production — ws:// Over HTTPS Causes Mixed-Content Errors

**Concepts**
- wss:// is WebSocket over TLS; ws:// is WebSocket over plain TCP
- Browsers block ws:// connections from HTTPS pages as mixed content
- A hard-coded ws:// URL in the JavaScript client causes silent connection failures in production
- The connection URL scheme must match the page's own scheme (http→ws, https→wss)

**Answer**

Browsers enforce mixed-content rules for WebSocket connections: a page served over HTTPS cannot open a `ws://` (unencrypted) WebSocket connection. Attempting to do so is silently blocked by the browser — the connection attempt never reaches the server and the browser console shows a mixed-content error. Development environments typically run on HTTP, making `ws://` work fine locally, but production deployments behind TLS break the same code. The fix is to derive the WebSocket URL scheme dynamically from `window.location.protocol`: `const scheme = window.location.protocol === "https:" ? "wss" : "ws"`. Hard-coding `ws://` in the client is one of the most common reasons WebSocket connections work perfectly in development and fail entirely in production.

---

#### Gotcha 9. IIS WebSocket Support Requires the WebSocket Protocol Feature to Be Enabled

**Concepts**
- IIS requires the WebSocket Protocol Windows feature to be installed
- Without it, IIS returns 500 for WebSocket upgrade requests
- IIS Express also requires enabling WebSockets in the applicationhost.config
- Kestrel supports WebSockets natively without any additional configuration

**Answer**

When hosting ASP.NET Core on IIS or IIS Express rather than Kestrel, WebSocket support requires the "WebSocket Protocol" Windows feature to be installed via Server Manager or DISM. Without it, IIS returns an HTTP 500 error for WebSocket upgrade requests, which manifests as a failed upgrade with a confusing server-side exception rather than the expected 101. On developer machines running IIS Express, WebSocket support is disabled by default and must be enabled in `applicationhost.config` by setting `webSocket enabled="true"`. Kestrel, by contrast, supports WebSockets natively with only `app.UseWebSockets()`. The IIS requirement is frequently forgotten because all development uses Kestrel and the IIS path is only exercised on the staging or production server.

---

#### Gotcha 10. Disconnecting Clients Are Not Detected Immediately Without Receiving or Pinging

**Concepts**
- TCP FIN is not always delivered when a client disconnects abruptly (process kill, network drop)
- The server only detects a dead connection when it next tries to send or receive
- A server that only writes and never reads may keep a ghost connection alive indefinitely
- CancellationToken on ReceiveAsync fires when the connection is aborted by ASP.NET Core

**Answer**

When a WebSocket client disconnects abruptly — process killed, Wi-Fi dropped, laptop lid closed — the TCP FIN may not be delivered to the server. The server's socket appears alive and write operations succeed at the kernel level, buffering data that is never acknowledged. The server only discovers the dead connection when it calls `ReceiveAsync` (which returns a Close frame or throws) or when `SendAsync` eventually fails after the OS exhausts its retransmit window. A server that exclusively pushes data and never reads will never detect that the client is gone, leaking connection resources indefinitely. The robust pattern is always to run a receive loop concurrently with the send loop, and to pass a `CancellationToken` linked to `HttpContext.RequestAborted` so that ASP.NET Core's own connection tracking can signal when the underlying HTTP connection is torn down.

---
