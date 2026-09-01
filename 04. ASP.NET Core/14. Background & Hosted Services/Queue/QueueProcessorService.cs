/*
 * FILE ROLE: Consumer BackgroundService that drains the BackgroundTaskQueue.
 *            Demonstrates the complete Channel<T> consumer pattern and best-practice
 *            exception handling that keeps the processor alive despite bad work items.
 * SECTIONS IN THIS FILE:
 *   6a. Queue consumer BackgroundService — draining Channel<T> in ExecuteAsync
 *   6b. Exception handling in hosted services — per-item isolation
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using BackgroundHostedServices.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundHostedServices.Queue;

/*
 * SECTION 6a: QUEUE CONSUMER BackgroundService
 *
 * The consumer calls DequeueAsync in a tight loop:
 *   - When the queue is empty, DequeueAsync awaits asynchronously (no busy-wait)
 *   - When an item arrives, the await completes and processing begins
 *   - When stoppingToken is cancelled, DequeueAsync throws OperationCanceledException
 *
 * Shutdown sequence:
 *   1. Host triggers stoppingToken (SIGTERM / Ctrl+C / StopApplication).
 *   2. DequeueAsync throws OperationCanceledException.
 *   3. The outer catch breaks the loop — in-flight item is logged as cancelled.
 *   4. ExecuteAsync returns — BackgroundService's StopAsync completes.
 *
 * Graceful drain (zero item loss on shutdown):
 *   After the break, check ChannelReader.TryRead() in a loop to process items
 *   that were already enqueued before the shutdown signal. This requires exposing
 *   the reader or using a separate draining path — suitable for critical job queues.
 *
 * SECTION 6b: EXCEPTION HANDLING IN HOSTED SERVICES
 *
 * Per-item exception isolation is essential in long-running consumers:
 *
 *   WRONG — a single bad item crashes the entire consumer:
 *     await ProcessItemAsync(item, ct);
 *     // Unhandled exception propagates to ExecuteAsync → host stops (.NET 6+)
 *
 *   CORRECT — isolate exceptions per item:
 *     try   { await ProcessItemAsync(item, ct); }
 *     catch (OperationCanceledException) { break; }  // shutdown signal — exit loop
 *     catch (Exception ex)               { _logger.LogError(ex, ...); } // log, continue
 *
 * Decision guide for caught exceptions:
 *   ┌───────────────────────────────┬───────────────────────────────────────────────┐
 *   │ Exception type                │ Recommended action                            │
 *   ├───────────────────────────────┼───────────────────────────────────────────────┤
 *   │ OperationCanceledException    │ Break / re-throw — always a shutdown signal   │
 *   │ Transient (timeout, network)  │ Log + retry with exponential back-off         │
 *   │ Poison message (always fails) │ Log + move to dead-letter; never retry        │
 *   │ Unrecoverable infrastructure  │ Re-throw — let StopHost take effect (net6+)   │
 *   └───────────────────────────────┴───────────────────────────────────────────────┘
 *
 * HostOptions.BackgroundServiceExceptionBehavior reminder:
 *   Default in .NET 6+: StopHost — unhandled ExecuteAsync exception stops the app.
 *   Set to Ignore ONLY if you handle every exception path inside ExecuteAsync.
 */
public sealed class QueueProcessorService : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<QueueProcessorService> _logger;

    public QueueProcessorService(
        IBackgroundTaskQueue queue,
        ILogger<QueueProcessorService> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QueueProcessorService started — waiting for work items.");

        while (!stoppingToken.IsCancellationRequested)
        {
            WorkItem item;

            try
            {
                // Awaits asynchronously until an item is available or ct is cancelled.
                item = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // stoppingToken was cancelled — host is shutting down, exit the loop.
                break;
            }

            try
            {
                await ProcessItemAsync(item, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Shutdown fired during processing — log and exit cleanly.
                _logger.LogWarning("Work item {Id} cancelled mid-processing (shutdown).",
                    item.Id);
                break;
            }
            catch (Exception ex)
            {
                // Bad item — log the error and continue processing the next item.
                // Do NOT re-throw here unless the error is unrecoverable infrastructure.
                _logger.LogError(ex, "Failed to process work item {Id} ({Payload}).",
                    item.Id, item.Payload);
            }
        }

        _logger.LogInformation("QueueProcessorService stopped.");
    }

    private async Task ProcessItemAsync(WorkItem item, CancellationToken ct)
    {
        _logger.LogInformation(
            "Processing work item {Id} enqueued at {Enqueued}: {Payload}.",
            item.Id, item.Enqueued, item.Payload);

        await Task.Delay(150, ct); // simulate async processing (DB write, API call, etc.)

        _logger.LogInformation("Work item {Id} processed successfully.", item.Id);
    }
}
