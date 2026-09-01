/*
 * FILE ROLE: WebSocketHandler — manages the full lifecycle of a single WebSocket
 *            connection: accept, receive loop with fragmentation awareness, graceful
 *            close handshake, and fan-out broadcast via WebSocketConnectionManager.
 *
 * SECTIONS IN THIS FILE:
 *   1. HandleAsync — entry point; registers connection, runs loop, cleans up
 *   2. ReceiveLoopAsync — ReceiveAsync / SendAsync loop, framing, MessageType handling
 *   3. Graceful close handshake — CloseAsync vs CloseOutputAsync
 */

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebSocketsRealTime.Models;

namespace WebSocketsRealTime.WebSockets;

/*
 * SECTION 1: HandleAsync — CONNECTION ENTRY POINT
 *
 * Called from Program.cs after the HTTP connection has been upgraded to WebSocket.
 * The upgrade is performed by HttpContext.WebSockets.AcceptWebSocketAsync() before
 * this method is invoked — by the time HandleAsync runs, the WebSocket is Open.
 *
 * Lifecycle:
 *   1. Register the socket with WebSocketConnectionManager (assigns a GUID id)
 *   2. Run the receive loop (blocks until connection closes)
 *   3. Unregister in a finally block — runs even on exception or abort
 *
 * context.RequestAborted is a CancellationToken that fires when:
 *   - the client disconnects abruptly (TCP RST)
 *   - the server is shutting down (IHostApplicationLifetime.ApplicationStopping)
 * Pass it through the entire call chain so tasks cancel promptly.
 */
public sealed class WebSocketHandler
{
    private readonly WebSocketConnectionManager _manager; // injected singleton

    public WebSocketHandler(WebSocketConnectionManager manager)
    {
        _manager = manager;
    }

    public async Task HandleAsync(HttpContext context, WebSocket webSocket)
    {
        string connectionId = _manager.AddSocket(webSocket);  // register; get GUID
        CancellationToken ct = context.RequestAborted;        // fires on disconnect / shutdown

        Console.WriteLine($"[WS] Connected: {connectionId[..8]}…");

        // announce join to all other clients
        await _manager.BroadcastAsync(
            $"[system] {connectionId[..8]}… joined. Total: {_manager.Count}",
            excludeId: connectionId,
            ct);

        try
        {
            await ReceiveLoopAsync(webSocket, connectionId, ct);
        }
        finally
        {
            // always unregister — even on exception, abort, or forced close
            _manager.RemoveSocket(connectionId);
            Console.WriteLine($"[WS] Disconnected: {connectionId[..8]}…");
        }
    }

    /*
     * SECTION 2: ReceiveLoopAsync — RECEIVE / SEND LOOP
     *
     * ReceiveAsync blocks until a full (or partial) frame arrives. It writes into
     * the caller-supplied buffer and returns a WebSocketReceiveResult:
     *
     *   result.MessageType   — Text | Binary | Close
     *   result.Count         — bytes written into the buffer this call
     *   result.EndOfMessage  — true if this is the last frame of the message
     *                          false if more continuation frames follow (fragmentation)
     *
     * Fragmentation (large messages):
     *   A single logical WebSocket message may be delivered across multiple frames.
     *   The sender sets endOfMessage=false on all frames except the last.
     *   The receiver MUST loop ReceiveAsync until result.EndOfMessage == true,
     *   accumulating bytes (e.g. into a MemoryStream or StringBuilder) before
     *   treating the payload as complete.
     *
     *   This demo uses a 4 KB buffer. For chat it is always sufficient in one frame.
     *   For production binary streaming: use a MemoryStream accumulator loop.
     *
     * WebSocketMessageType values:
     *   Text  (1) — payload is valid UTF-8; decode with Encoding.UTF8.GetString
     *   Binary(2) — raw bytes; application-defined meaning
     *   Close (8) — peer sent a close frame; initiate close handshake (see Section 3)
     */
    private async Task ReceiveLoopAsync(
        WebSocket webSocket,
        string connectionId,
        CancellationToken ct)
    {
        byte[] buffer = new byte[4096]; // 4 KB receive buffer — reused each iteration

        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), ct);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                // SECTION 3 — graceful close handled inline below
                await CloseGracefullyAsync(webSocket, ct);
                break;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                // decode only the bytes actually written (result.Count, not buffer.Length)
                string text = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // create a ChatMessage for structured logging
                ChatMessage msg = new ChatMessage
                {
                    ConnectionId = connectionId,
                    Text         = text,
                    Kind         = MessageKind.Text
                };
                Console.WriteLine(msg.ToString()); // [HH:mm:ss] [Text] <id>: <text>

                // broadcast to all other connected clients
                await _manager.BroadcastAsync(
                    $"{connectionId[..8]}…: {text}",
                    excludeId: connectionId,
                    ct);
            }
            // Binary frames: log and ignore in this demo
            else if (result.MessageType == WebSocketMessageType.Binary)
            {
                ChatMessage msg = new ChatMessage
                {
                    ConnectionId = connectionId,
                    Text         = $"<binary {result.Count} bytes>",
                    Kind         = MessageKind.Binary
                };
                Console.WriteLine(msg.ToString());
            }
        }
    }

    /*
     * SECTION 3: GRACEFUL CLOSE HANDSHAKE
     *
     * WebSocket close is a two-step handshake defined in RFC 6455:
     *
     *   1. First mover sends a Close frame (with optional status code + reason)
     *   2. The other peer echoes a Close frame
     *   3. TCP connection is then torn down
     *
     * CloseAsync (two-step, recommended):
     *   Sends a Close frame AND waits for the peer's echo before returning.
     *   Use when YOU initiate the close.
     *
     * CloseOutputAsync (one-step):
     *   Sends a Close frame but does NOT wait for the echo.
     *   Use when the PEER initiated the close (already received their Close frame);
     *   you just need to echo back.
     *
     * WebSocketCloseStatus codes (RFC 6455 §7.4):
     *   NormalClosure      (1000) — clean intentional close
     *   EndpointUnavailable(1001) — server going away (restart, deploy)
     *   ProtocolError      (1002) — protocol violation
     *   InvalidMessageType (1003) — received unsupported data type
     *   PolicyViolation    (1008) — application-level policy (auth, rate-limit)
     *   MessageTooBig      (1009) — payload exceeded maximum allowed size
     *   InternalServerError(1011) — unexpected server error
     */
    private static async Task CloseGracefullyAsync(WebSocket webSocket, CancellationToken ct)
    {
        // Peer sent Close first — echo back with CloseOutputAsync (one step)
        await webSocket.CloseOutputAsync(
            WebSocketCloseStatus.NormalClosure,
            "Acknowledged",
            ct);

        Console.WriteLine($"[WS] Close handshake complete. Final state: {webSocket.State}");
    }
}
