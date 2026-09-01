/*
 * TOPIC: WebSockets & Real-Time Transport
 *
 * WHY IT MATTERS:
 *   HTTP is a request/response protocol — the client always initiates, the server
 *   always replies, and the connection closes. Modern applications (chat, dashboards,
 *   collaborative editing, live gaming) need the SERVER to push data to the CLIENT
 *   at any time without the client asking first. Three transport strategies solve this:
 *
 *     WebSockets  — full-duplex, persistent TCP tunnel over HTTP upgrade
 *     SSE         — server-push only, plain HTTP/1.1 streaming (EventSource API)
 *     Long Polling — client re-polls immediately after each response (legacy fallback)
 *
 *   SignalR wraps all three transports behind a unified Hub abstraction and picks
 *   the best one automatically.
 *
 * WHAT YOU WILL LEARN:
 *   1. WebSocket protocol: handshake, frames, opcodes, close handshake
 *   2. UseWebSockets middleware and WebSocketOptions (keepalive / ping-pong)
 *   3. HttpContext.WebSockets.AcceptWebSocketAsync — HTTP→WS upgrade
 *   4. ReceiveAsync / SendAsync loop, WebSocketMessageType, framing & fragmentation
 *   5. Graceful close: CloseAsync vs CloseOutputAsync, WebSocketCloseStatus codes
 *   6. WebSocketConnectionManager — multi-connection tracking and broadcast
 *   7. Server-Sent Events (SSE) with text/event-stream
 *   8. WebSocket vs SSE vs Long Polling comparison
 *   9. SignalR Hub PREVIEW: Hub class, hub methods, client invocation, groups
 *  10. IConnectionMultiplexer scale-out PREVIEW
 *
 * CHAPTER MAP (open files in this order):
 *   1. ChatMessage DTO        → Models/ChatMessage.cs
 *   2. ConnectionInfo DTO     → Models/ConnectionInfo.cs
 *   3. Connection registry    → WebSockets/WebSocketConnectionManager.cs
 *   4. Connection handler     → WebSockets/WebSocketHandler.cs
 *   5. SSE handler            → Sse/SseHandler.cs
 *   6. SignalR Hub PREVIEW    → Hubs/ChatHub.cs
 *   7. Startup wiring         → Program.cs (this file, below)
 */

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using WebSocketsRealTime.Hubs;
using WebSocketsRealTime.Sse;
using WebSocketsRealTime.WebSockets;

/*
 * SECTION 1: WEBSOCKET PROTOCOL OVERVIEW
 *
 * The WebSocket protocol (RFC 6455) upgrades an HTTP/1.1 connection to a persistent,
 * full-duplex TCP tunnel. Once upgraded, either side can send frames at any time.
 *
 * Handshake (HTTP upgrade):
 *   Client sends:  GET /ws HTTP/1.1
 *                  Upgrade: websocket
 *                  Connection: Upgrade
 *                  Sec-WebSocket-Key: <base64 nonce>
 *
 *   Server replies: HTTP/1.1 101 Switching Protocols
 *                   Upgrade: websocket
 *                   Sec-WebSocket-Accept: <SHA-1 of key + GUID, base64>
 *
 *   After 101, the TCP connection is "owned" by the WebSocket protocol — HTTP is gone.
 *
 * Frame structure (simplified):
 *   Bit 0   : FIN — 1 if this is the last frame of the message (EndOfMessage)
 *   Bits 1–3: RSV1-3 — reserved for extensions (0 unless negotiated)
 *   Bits 4–7: opcode
 *     0x0 Continuation  — continuation frame for a fragmented message
 *     0x1 Text          — UTF-8 payload
 *     0x2 Binary        — raw bytes
 *     0x8 Close         — close-handshake frame (carries status code + reason)
 *     0x9 Ping          — keepalive probe
 *     0xA Pong          — keepalive reply to Ping
 *   Bit 8   : MASK — must be 1 from client, 0 from server
 *   Bits 9–15/23: payload length (7-bit inline, 16-bit, or 64-bit extended)
 *   Masking key (4 bytes, client only)
 *   Payload
 *
 * Ping / Pong keepalive:
 *   UseWebSockets sets KeepAliveInterval (default 2 min). The server sends Ping frames
 *   automatically. The client must respond with Pong. If no Pong arrives within the
 *   interval the server closes the connection. This detects silent TCP drops
 *   (NAT timeout, proxy idle cutoff, device sleep). .NET's WebSocket handles Ping/Pong
 *   transparently — you never see them in ReceiveAsync.
 */

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// register WebSocketConnectionManager as singleton — shared across all requests
builder.Services.AddSingleton<WebSocketConnectionManager>();

// register SignalR services (PREVIEW — see Hubs/ChatHub.cs)
builder.Services.AddSignalR();

WebApplication app = builder.Build();

/*
 * SECTION 2: UseWebSockets MIDDLEWARE
 *
 * app.UseWebSockets() MUST be called before any endpoint that handles WebSocket
 * requests. It:
 *   - Checks each incoming request for the Upgrade: websocket header
 *   - Adds HttpContext.WebSockets (WebSocketManager) so endpoints can call
 *     IsWebSocketRequest and AcceptWebSocketAsync
 *   - Starts the ping/pong keepalive timer (KeepAliveInterval)
 *
 * Without UseWebSockets:
 *   HttpContext.WebSockets.IsWebSocketRequest always returns false
 *   HttpContext.WebSockets.AcceptWebSocketAsync throws InvalidOperationException
 *
 * WebSocketOptions:
 *   KeepAliveInterval  — how often to send Ping frames (default: 2 min)
 *   AllowedOrigins     — restrict origins for same-origin policy enforcement
 *                        (browser sends Origin header; server validates here)
 *
 * Middleware order matters: UseWebSockets → UseRouting → MapGet/MapHub
 */
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30) // ping every 30 s (demo; default is 2 min)
});

/*
 * SECTION 3: WEBSOCKET ENDPOINT — AcceptWebSocketAsync
 *
 * HttpContext.WebSockets.IsWebSocketRequest
 *   Returns true when the request carries valid WebSocket upgrade headers.
 *   Always check before calling AcceptWebSocketAsync — calling it on a plain HTTP
 *   request throws InvalidOperationException.
 *
 * HttpContext.WebSockets.AcceptWebSocketAsync()
 *   Completes the HTTP 101 handshake and returns an open System.Net.WebSockets.WebSocket.
 *   Optional overload: AcceptWebSocketAsync(subProtocol) — negotiates a sub-protocol
 *   (e.g. "graphql-ws", "mqtt") advertised by the client in Sec-WebSocket-Protocol.
 *
 * After AcceptWebSocketAsync returns, the HTTP request pipeline is no longer active.
 * The WebSocket handler takes ownership of the connection until it is closed.
 *
 * Broadcast chat pattern:
 *   WebSocketHandler registers the socket with WebSocketConnectionManager (singleton),
 *   loops ReceiveAsync, and for each message calls BroadcastAsync to fan out to all
 *   other connected clients. See WebSockets/WebSocketHandler.cs for full detail.
 */
app.MapGet("/ws", async (HttpContext context) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest; // reject plain HTTP
        return;
    }

    // complete the HTTP→WebSocket upgrade; returns an Open WebSocket
    System.Net.WebSockets.WebSocket ws =
        await context.WebSockets.AcceptWebSocketAsync();

    // retrieve the singleton connection manager from DI
    WebSocketConnectionManager manager =
        context.RequestServices.GetRequiredService<WebSocketConnectionManager>();

    WebSocketHandler handler = new WebSocketHandler(manager);
    await handler.HandleAsync(context, ws); // blocks until connection closes
});

/*
 * SECTION 4: SERVER-SENT EVENTS ENDPOINT
 *
 * SSE needs no special middleware. It is a plain GET response that:
 *   - sets Content-Type: text/event-stream
 *   - keeps the response body open
 *   - writes formatted event lines and flushes after each one
 *
 * The browser's built-in EventSource API:
 *   - automatically reconnects after disconnect
 *   - sends Last-Event-ID header on reconnect so the server can resume
 *
 * See Sse/SseHandler.cs for full protocol explanation and event format.
 */
app.MapGet("/sse", async (HttpContext context) =>
{
    SseHandler sseHandler = new SseHandler();
    await sseHandler.HandleAsync(context);
});

/*
 * SECTION 5: SIGNALR HUB ENDPOINT (PREVIEW)
 *
 * app.MapHub<THub>(pattern) registers the hub at the given URL path.
 * SignalR negotiates the transport (WebSocket → SSE → Long Polling) automatically.
 *
 * Client connects at: ws://host/chathub (WebSocket transport)
 *                 or: http://host/chathub (SSE / Long Polling fallback)
 *
 * IConnectionMultiplexer scale-out (PREVIEW):
 *   In a single-server deployment, WebSocketConnectionManager (in-process
 *   ConcurrentDictionary) works fine. On multiple server nodes, a client connected
 *   to Node A cannot receive a message broadcast from Node B.
 *
 *   Solution: use a shared backplane (Redis, Azure Service Bus, SQL) so all nodes
 *   share the same message bus. In SignalR:
 *     builder.Services.AddSignalR().AddStackExchangeRedis("localhost:6379");
 *
 *   For raw WebSockets (no SignalR): use StackExchange.Redis IConnectionMultiplexer,
 *   subscribe to a Redis Pub/Sub channel, and forward each published message to all
 *   local sockets. Each node publishes to Redis; Redis fans out to all subscribers.
 *
 *   COVERED IN DETAIL LATER → SignalR Scale-Out & Redis Backplane (dedicated folder)
 *
 * See Hubs/ChatHub.cs for Hub class, hub methods, and groups.
 */
app.MapHub<ChatHub>("/chathub");

app.Run();

/*
 * QUICK REFERENCE — WebSockets & Real-Time Transport
 * ════════════════════════════════════════════════════
 *
 * Middleware registration:
 *   app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(30) });
 *
 * Accept upgrade:
 *   bool isWs = context.WebSockets.IsWebSocketRequest;
 *   WebSocket ws = await context.WebSockets.AcceptWebSocketAsync();
 *
 * Receive loop:
 *   byte[] buf = new byte[4096];
 *   WebSocketReceiveResult r = await ws.ReceiveAsync(new ArraySegment<byte>(buf), ct);
 *   r.MessageType   → Text | Binary | Close
 *   r.Count         → bytes written into buf this call
 *   r.EndOfMessage  → false = more frames follow (fragmented message)
 *
 * Send:
 *   await ws.SendAsync(new ArraySegment<byte>(bytes),
 *       WebSocketMessageType.Text, endOfMessage: true, ct);
 *
 * Close handshake:
 *   await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "reason", ct);     // initiate
 *   await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "ack", ct);  // echo back
 *
 * WebSocketState: None | Connecting | Open | CloseSent | CloseReceived | Closed | Aborted
 *
 * SSE response:
 *   context.Response.ContentType = "text/event-stream";
 *   context.Response.Headers["Cache-Control"] = "no-cache";
 *   await body.WriteAsync(Encoding.UTF8.GetBytes("data: hello\n\n"), ct);
 *   await body.FlushAsync(ct);
 *
 * SSE event format:
 *   id: <id>\n event: <name>\n data: <payload>\n\n
 *
 * SignalR (PREVIEW):
 *   builder.Services.AddSignalR();
 *   app.MapHub<MyHub>("/hub");
 *   // Hub method: public async Task Send(string msg) => await Clients.All.SendAsync("Recv", msg);
 *   // Groups:     await Groups.AddToGroupAsync(Context.ConnectionId, "room1");
 *                  await Clients.Group("room1").SendAsync("Recv", msg);
 *
 * Transport comparison:
 *   WebSocket   — full-duplex, low-latency, requires WS protocol support
 *   SSE         — server→client only, auto-reconnect, plain HTTP, HTTP/2 multiplexed
 *   Long Polling — universal fallback, higher latency, works through all proxies
 */
