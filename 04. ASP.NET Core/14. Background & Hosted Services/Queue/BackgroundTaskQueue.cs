/*
 * FILE ROLE: Implements a Channel<T>-based background task queue used in the
 *            producer/consumer pattern. The producer (e.g. an API endpoint)
 *            calls EnqueueAsync; the consumer (QueueProcessorService) calls DequeueAsync.
 * SECTIONS IN THIS FILE:
 *   5a. Channel<T> — in-process async producer/consumer queue
 *   5b. BoundedChannel options — backpressure and overflow strategies
 */

using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BackgroundHostedServices.Models;

namespace BackgroundHostedServices.Queue;

/*
 * SECTION 5a: Channel<T> — IN-PROCESS ASYNC PRODUCER/CONSUMER QUEUE
 *
 * System.Threading.Channels provides a high-performance, fully async queue
 * primitive for in-process producer/consumer pipelines. It replaces the older
 * pattern of ConcurrentQueue<T> + SemaphoreSlim + polling.
 *
 *   Channel.CreateUnbounded<T>()       — grows without limit; no backpressure
 *   Channel.CreateBounded<T>(options)  — fixed capacity; configurable on overflow
 *
 * Channel<T> is split into two sides (allows separate access control):
 *   ChannelWriter<T>.WriteAsync(item, ct)  — producer writes; awaits space if bounded
 *   ChannelReader<T>.ReadAsync(ct)         — consumer reads; awaits until item available
 *   ChannelReader<T>.WaitToReadAsync(ct)   — peek: returns true when data arrives
 *   ChannelWriter<T>.Complete()            — signals no more items; reader drains cleanly
 *
 * Why Channel<T> over BlockingCollection<T>?
 *   - Fully async — no thread blocking under the hood
 *   - Backpressure support via bounded channels (WriteAsync awaits when full)
 *   - Structured completion: Complete() / TryComplete() lets the reader drain
 *     all remaining items and then WaitToReadAsync returns false
 *   - Works with IAsyncEnumerable<T> via ChannelReader.ReadAllAsync(ct)
 *
 * SECTION 5b: BoundedChannel OPTIONS — BACKPRESSURE AND OVERFLOW STRATEGIES
 *
 * BoundedChannelOptions lets you tune capacity and overflow behavior:
 *
 *   FullMode (BoundedChannelFullMode enum):
 *   ┌──────────────┬────────────────────────────────────────────────────────────┐
 *   │ Wait         │ WriteAsync awaits until space is available (backpressure)   │
 *   │ DropWrite    │ WriteAsync returns immediately; new item is silently dropped │
 *   │ DropNewest   │ The item just written is discarded (no space taken)         │
 *   │ DropOldest   │ The oldest unread item is removed to make room              │
 *   └──────────────┴────────────────────────────────────────────────────────────┘
 *
 *   Choose Wait for correctness (no data loss — critical jobs, commands).
 *   Choose DropNewest/DropOldest for telemetry or metrics (freshness > completeness).
 *
 *   SingleReader = true  — hint: only one consumer; enables lock-free optimization
 *   SingleWriter = false — many producers can write concurrently
 *   AllowSynchronousContinuations = false — continuations always on the thread pool,
 *       not inline on the writer's thread (prevents unexpected call stack depth)
 */
public interface IBackgroundTaskQueue
{
    // Enqueue a work item. Awaits if the bounded channel is full (backpressure).
    ValueTask EnqueueAsync(WorkItem item, CancellationToken ct = default);

    // Dequeue the next work item. Awaits asynchronously until one is available.
    ValueTask<WorkItem> DequeueAsync(CancellationToken ct);
}

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<WorkItem> _channel;

    public BackgroundTaskQueue(int capacity = 100)
    {
        BoundedChannelOptions options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait, // backpressure — callers await space
            SingleReader = true,                     // one consumer: QueueProcessorService
            SingleWriter = false,                    // many producers: API endpoints, events
            AllowSynchronousContinuations = false    // always resume on thread pool
        };
        _channel = Channel.CreateBounded<WorkItem>(options); // typed bounded channel
    }

    public ValueTask EnqueueAsync(WorkItem item, CancellationToken ct = default)
    {
        // WriteAsync is a ValueTask — no Task allocation on the fast path (space available).
        return _channel.Writer.WriteAsync(item, ct);
    }

    public ValueTask<WorkItem> DequeueAsync(CancellationToken ct)
    {
        // ReadAsync awaits asynchronously until an item is enqueued or ct is cancelled.
        return _channel.Reader.ReadAsync(ct);
    }
}
