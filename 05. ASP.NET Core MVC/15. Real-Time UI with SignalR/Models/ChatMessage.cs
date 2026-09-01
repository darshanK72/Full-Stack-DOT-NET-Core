using System;

/*
 * FILE ROLE: Data-transfer object (DTO) representing a single chat message.
 *            Passed between hub methods and serialized by SignalR's JSON protocol.
 *
 * SECTIONS IN THIS FILE:
 *   1. ChatMessage — message DTO with serialization notes
 */

namespace RealTimeSignalR.Models;

/*
 * SECTION 1: CHATMESSAGE — MESSAGE DTO
 *
 * SignalR serializes hub method arguments and return values with
 * System.Text.Json by default.  The serialized payload travels over
 * the wire to every targeted JavaScript client.
 *
 * Design choices:
 *   init-only properties  — immutable after construction; no accidental mutation
 *   string.Empty defaults — Nullable:enable; avoids CS8618 (non-nullable field
 *                           must contain non-null value on exit from constructor)
 *
 * JSON protocol naming (default camelCase):
 *   The JavaScript client receives: { user: "...", message: "...", sentAt: "..." }
 *   Property names use camelCase because AddSignalR() defaults to
 *   JsonNamingPolicy.CamelCase for hub payloads.
 *
 * Keeping DTOs separate from Hubs:
 *   - Hub class focuses on connection/routing logic
 *   - DTO defines shape; can be reused across multiple hubs or API controllers
 *   - Easier to test DTO serialization independently
 */
public sealed class ChatMessage
{
    public string User { get; init; } = string.Empty;       // sender display name
    public string Message { get; init; } = string.Empty;    // message body text
    public DateTime SentAt { get; init; } = DateTime.UtcNow; // UTC creation timestamp
}
