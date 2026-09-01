/*
 * FILE ROLE: ChatMessage DTO — represents a single chat event transported over WebSocket
 *            or SignalR. Used by WebSocketHandler to log received frames and by the
 *            broadcast helper to carry message metadata.
 *
 * SECTIONS IN THIS FILE:
 *   1. MessageKind enum  — distinguishes Text, Binary, and System (join/leave) frames
 *   2. ChatMessage class — the data-transfer object for one chat event
 */

using System;

namespace WebSocketsRealTime.Models;

/*
 * SECTION 1: MessageKind ENUM
 *
 * WebSocket frames carry a MessageType flag in the frame header:
 *
 *   WebSocketMessageType.Text   → UTF-8 encoded payload (this demo)
 *   WebSocketMessageType.Binary → raw bytes (images, files, custom protocols)
 *   WebSocketMessageType.Close  → close-handshake frame (not a data message)
 *
 * MessageKind maps those wire-level types to an application-level concept.
 * "System" covers server-generated notifications (user joined / left) that are
 * not sent by a client — they have no WebSocketMessageType counterpart.
 */
public enum MessageKind
{
    Text,    // ordinary chat text — UTF-8 string payload
    Binary,  // binary frame — image, file, or protocol-specific bytes
    System   // server-generated notification (join, leave, ping echo)
}

/*
 * SECTION 2: ChatMessage CLASS
 *
 * Immutable DTO: all properties use init-only setters so a message cannot be
 * mutated after construction. This mirrors how events travel in real systems —
 * past events are facts, not editable state.
 *
 * Why init-only (C# 9+)?
 *   record-like immutability without making ChatMessage a record, which would
 *   add value equality semantics we do not need here.
 *
 * SentAt is set to UTC at construction time. Always store timestamps in UTC;
 * convert to local time only at the UI layer.
 */
public sealed class ChatMessage
{
    public string ConnectionId { get; init; } = string.Empty; // who sent it
    public string Text        { get; init; } = string.Empty; // the payload text
    public DateTime SentAt    { get; init; } = DateTime.UtcNow; // UTC timestamp
    public MessageKind Kind   { get; init; } = MessageKind.Text; // frame category

    /// <summary>Returns a human-readable log line for console output.</summary>
    public override string ToString() =>
        $"[{SentAt:HH:mm:ss}] [{Kind}] {ConnectionId}: {Text}";
}
