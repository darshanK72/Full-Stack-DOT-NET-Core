---
module: 02. Object Oriented Programming
difficulty: Hard
chapters: 06 Interfaces, 08 Events, 09 Notification Example
domain: Alerting
---

# Notification Hub

Build a **.NET 8 console application from scratch** routing alerts through interface-based senders and in-process events.

## Business context

Operations sends email and SMS alerts. Subscribers log deliveries for audit. Design mirrors publisher/subscriber with typed events and pluggable senders.

## Definitions

**Enum `AlertSeverity`:** `Info`, `Warning`, `Critical`

**Interface `INotificationSender`**

- `string ChannelName { get; }` — e.g. `"Email"`, `"Sms"`
- `bool Send(string recipient, string message, AlertSeverity severity)`

**`EmailSender : INotificationSender`**

- Validates recipient contains `@`; false if invalid
- `ChannelName` → `"Email"`

**`SmsSender : INotificationSender`**

- Validates recipient length 10–15 digits (digits only after trim)
- `ChannelName` → `"Sms"`

**Class `AlertPublishedEventArgs : EventArgs`**

- `Recipient`, `Message`, `Severity`, `ChannelName`, `Success` (bool)

**Class `NotificationHub`**

- Constructor accepts `IEnumerable<INotificationSender>` senders
- Event `AlertPublished` → `EventHandler<AlertPublishedEventArgs>`
- `bool Publish(string recipient, string message, AlertSeverity severity, string channelName)` — find sender by case-insensitive `ChannelName`; if none, return false
- On send attempt, raise `AlertPublished` with outcome (even when sender returns false)
- External code cannot invoke the event directly

**Class `AuditLog`**

- Subscribes to hub on construction
- Maintains `IReadOnlyList<string> Entries` — each entry `"[{Severity}] {ChannelName} -> {Recipient}: {Success}"`
- `Unsubscribe()` removes handler

## Demo Main

Wire hub with both senders, subscribe audit log, publish valid/invalid recipients on both channels, print entries, unsubscribe, publish again (no new entries).

## Constraints

- net8, explicit usings
- Use `event` keyword (not public delegate field)

## Non-goals

Real SMTP/SMS APIs, async

## Evaluation

[EVALUATION.md](EVALUATION.md)
