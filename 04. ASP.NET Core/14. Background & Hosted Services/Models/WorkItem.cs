/*
 * FILE ROLE: Defines the WorkItem record — the unit of data placed onto the
 *            background task queue and consumed by QueueProcessorService.
 * SECTIONS IN THIS FILE:
 *   1. WorkItem record — immutable work payload for Channel<T> queue
 */

using System;

namespace BackgroundHostedServices.Models;

/*
 * SECTION 1: WORKITEM — UNIT OF WORK FOR THE BACKGROUND QUEUE
 *
 * A record is ideal for queue payloads: immutable, value-equality, concise.
 *
 * Fields:
 *   Id       — unique identifier assigned by the producer (e.g. an API endpoint)
 *   Payload  — the work description; in production this would be a typed command DTO
 *   Enqueued — timestamp when the item was placed on the queue (audit/latency tracking)
 *
 * Why a record?
 *   - Positional properties are init-only — immutable after construction
 *   - Value equality: two WorkItems with identical data compare as equal
 *   - Compact syntax; compiler generates ToString(), Equals(), GetHashCode()
 *
 * Design note: keep work items small — they live in memory on the Channel<T>.
 * For large payloads, store a reference (a database row ID, a blob URL, etc.)
 * and load the full data inside the consumer.
 */
public sealed record WorkItem(Guid Id, string Payload, DateTimeOffset Enqueued);
