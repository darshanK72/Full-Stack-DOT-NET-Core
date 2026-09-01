/*
 * FILE ROLE: WebSocketConnectionManager — singleton registry of all active WebSocket
 *            connections. Enables broadcast (send to all clients), per-connection
 *            lookup, and clean removal on disconnect. The broadcast chat example
 *            wires through this class.
 *
 * SECTIONS IN THIS FILE:
 *   1. Storage — ConcurrentDictionary for thread-safe multi-connection tracking
 *   2. AddSocket / RemoveSocket — connection lifecycle helpers
 *   3. BroadcastAsync — fan-out send to all open sockets
 *   4. GetInfo — return a ConnectionInfo snapshot without exposing raw WebSocket
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketsRealTime.Models;

namespace WebSocketsRealTime.WebSockets;

/*
 * SECTION 1: STORAGE — ConcurrentDictionary
 *
 * WebSocket connections are inherently concurrent: many clients connect at the
 * same time and each is served by its own async pipeline. A plain Dictionary is
 * not thread-safe — concurrent Add/Remove from different async continuations
 * causes data corruption.
 *
 * ConcurrentDictionary<TKey, TValue> (System.Collections.Concurrent):
 *   - Lock-striped internally; reads are usually lock-free
 *   - TryAdd, TryRemove, TryGetValue — all atomic
 *   - foreach over a ConcurrentDictionary takes a snapshot-like view; safe during iteration
 *
 * For production scale-out (multiple server nodes) you would replace this
 * in-process dictionary with a Redis-backed IConnectionMultiplexer.
 * COVERED IN DETAIL LATER → SignalR Scale-Out / Redis Backplane topic
 */
public sealed class WebSocketConnectionManager
{
    // keyed by GUID connection id; value is the live WebSocket
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

    /*
     * SECTION 2: AddSocket / RemoveSocket
     *
     * AddSocket assigns a new GUID as the connection id, stores the socket, and
     * returns the id so the caller can associate it with the HTTP context or a user.
     *
     * RemoveSocket is called from the finally block of the receive loop so the slot
     * is always freed whether the connection closed gracefully or was aborted.
     */
    public string AddSocket(WebSocket socket)
    {
        string id = Guid.NewGuid().ToString(); // unique id per connection
        _sockets.TryAdd(id, socket);           // atomic; safe under concurrent accepts
        return id;
    }

    public void RemoveSocket(string id)
    {
        _sockets.TryRemove(id, out _); // discard the value; we only need the side-effect
    }

    public int Count => _sockets.Count; // number of open slots (some may be Closed/Aborted)

    /*
     * SECTION 3: BroadcastAsync — fan-out send
     *
     * Walk every registered socket and send the message if the socket is still Open.
     * Skip the sender's own id (excludeId) so they do not echo to themselves.
     *
     * SendAsync parameters:
     *   buffer          — ArraySegment<byte> wrapping the UTF-8 encoded message
     *   messageType     — WebSocketMessageType.Text (UTF-8) or Binary
     *   endOfMessage    — true = this is the final (or only) frame of the message
     *                     false = more frames follow (fragmentation)
     *   cancellationToken
     *
     * Fragmentation (endOfMessage = false):
     *   Large payloads can be split across multiple frames.  The receiver reassembles
     *   them by looping ReceiveAsync until result.EndOfMessage == true.
     *   For chat messages we always send complete single-frame messages (endOfMessage = true).
     *
     * Thread safety: each WebSocket.SendAsync must be called by one sender at a time.
     * For simplicity this demo sends sequentially. Production code uses a per-connection
     * Channel<T> to queue outbound messages and drain them from a single writer task.
     */
    public async Task BroadcastAsync(
        string message,
        string? excludeId = null,
        CancellationToken ct = default)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);             // encode once
        ArraySegment<byte> segment = new ArraySegment<byte>(bytes); // zero-copy wrapper

        foreach (KeyValuePair<string, WebSocket> pair in _sockets)
        {
            if (pair.Key == excludeId) continue;                     // skip sender
            if (pair.Value.State != WebSocketState.Open) continue;   // skip closed/aborted

            await pair.Value.SendAsync(
                segment,
                WebSocketMessageType.Text,
                endOfMessage: true,  // single-frame message
                ct);
        }
    }

    /*
     * SECTION 4: GetInfo — ConnectionInfo snapshot
     *
     * Returns a ConnectionInfo value object describing the current state of a
     * connection, without exposing the raw WebSocket to the caller.
     *
     * Returns null if the id is not registered (already removed).
     */
    public ConnectionInfo? GetInfo(string id)
    {
        if (!_sockets.TryGetValue(id, out WebSocket? socket))
            return null;

        return new ConnectionInfo
        {
            Id    = id,
            State = socket.State,       // live state snapshot
            ConnectedAt = DateTime.UtcNow // approximation — real code stores the connect time
        };
    }
}
