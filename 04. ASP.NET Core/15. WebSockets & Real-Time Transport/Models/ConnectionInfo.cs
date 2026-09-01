/*
 * FILE ROLE: ConnectionInfo — snapshot of metadata for one active WebSocket connection.
 *            Returned by WebSocketConnectionManager.GetInfo() to give callers a
 *            read-only view of connection state without exposing the raw WebSocket.
 *
 * SECTIONS IN THIS FILE:
 *   1. ConnectionInfo class — connection metadata snapshot
 */

using System;
using System.Net.WebSockets;

namespace WebSocketsRealTime.Models;

/*
 * SECTION 1: ConnectionInfo CLASS
 *
 * Purpose: decouple "what do I know about this connection?" from the live
 * System.Net.WebSockets.WebSocket object. Callers that only need to display
 * or log connection details should not hold a reference to the raw socket.
 *
 * WebSocketState values (System.Net.WebSockets.WebSocketState):
 *
 *   None          (0) — not yet connected
 *   Connecting    (1) — handshake in progress
 *   Open          (2) — connection established, can send/receive
 *   CloseSent     (3) — server sent Close frame, waiting for client echo
 *   CloseReceived (4) — client sent Close frame, server should echo
 *   Closed        (5) — close handshake complete
 *   Aborted       (6) — transport-level error; socket unusable
 *
 * Before calling SendAsync or ReceiveAsync, always check State == Open.
 * Sending on a non-Open socket throws WebSocketException.
 */
public sealed class ConnectionInfo
{
    public string Id              { get; init; } = string.Empty;      // GUID assigned at connect
    public WebSocketState State   { get; set; }  = WebSocketState.None; // mutable — updated on state change
    public DateTime ConnectedAt   { get; init; } = DateTime.UtcNow;  // UTC connect timestamp
    public string? RemoteIpAddress { get; init; }                     // client IP, null if unavailable

    /// <summary>Age of the connection from connect time to now.</summary>
    public TimeSpan Age => DateTime.UtcNow - ConnectedAt;

    public override string ToString() =>
        $"Id={Id[..8]}… State={State} Age={Age.TotalSeconds:F0}s IP={RemoteIpAddress ?? "?"}";
}
