/*
 * FILE ROLE: SseHandler — demonstrates Server-Sent Events (SSE) using the
 *            text/event-stream content type. SSE is a unidirectional push protocol
 *            over plain HTTP — no WebSocket upgrade required.
 *
 * SECTIONS IN THIS FILE:
 *   1. SSE protocol overview — event format, content type, headers
 *   2. HandleAsync — response setup and event-push loop
 *   3. WebSocket vs SSE vs Long Polling comparison (in comments)
 */

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebSocketsRealTime.Sse;

/*
 * SECTION 1: SERVER-SENT EVENTS (SSE) PROTOCOL
 *
 * SSE (EventSource API / text/event-stream) is a W3C standard for server-push
 * over HTTP/1.1 or HTTP/2. It is simpler than WebSockets for use-cases where
 * the server pushes data to the browser but the client does not need to send data
 * back (dashboards, live feeds, notifications).
 *
 * Content-Type: text/event-stream
 *
 * Event format (one event = one or more field lines terminated by blank line):
 *
 *   data: <payload>\n\n          ← simplest form: data only
 *
 *   id: <event-id>\n             ← optional: browser tracks Last-Event-ID for reconnect
 *   event: <event-type>\n        ← optional: named event (browser: source.addEventListener)
 *   data: <payload>\n
 *   retry: 3000\n                ← optional: reconnect interval (ms) hint to client
 *   \n                           ← blank line = end of event
 *
 * Browser side (JavaScript):
 *   const source = new EventSource("/sse");
 *   source.onmessage = e => console.log(e.data);           // unnamed events
 *   source.addEventListener("tick", e => console.log(e.data)); // named events
 *
 * SSE vs WebSocket vs Long Polling:
 *
 *   Feature              WebSocket              SSE                   Long Polling
 *   ─────────────────    ─────────────────────  ────────────────────  ──────────────────
 *   Direction            Bidirectional          Server→Client only    Server→Client only
 *   Protocol             WS / WSS (upgrade)     HTTP/1.1 or HTTP/2    Plain HTTP
 *   Browser support      All modern             All modern (no IE)    All browsers
 *   Automatic reconnect  No (app code)          Yes (built-in)        App code
 *   Multiplexing (H/2)   No (own connection)    Yes (shared H/2)      No
 *   Use case             Chat, gaming, collab   Dashboards, feeds     Legacy / firewall-safe
 *   Server complexity    Higher (upgrade + loop) Low (plain response) Low
 *
 * Key ASP.NET Core difference:
 *   SSE needs no special middleware (no UseWebSockets). It is a plain HTTP GET
 *   response with the right Content-Type and a long-lived open body.
 */
public sealed class SseHandler
{
    /*
     * SECTION 2: HandleAsync — RESPONSE SETUP AND EVENT LOOP
     *
     * Steps:
     *   1. Set Content-Type: text/event-stream
     *   2. Disable response buffering (Cache-Control + X-Accel-Buffering)
     *   3. Loop: write a formatted SSE event, flush, await delay, repeat
     *   4. Catch OperationCanceledException when client disconnects — normal exit
     *
     * Response.Body.FlushAsync() is critical: ASP.NET Core buffers response writes
     * by default. Without an explicit flush the browser receives nothing until the
     * buffer fills or the response ends. SSE requires events to be delivered
     * immediately — always flush after each event.
     *
     * context.RequestAborted fires when the client closes the EventSource or navigates
     * away. Passing it to WriteAsync, FlushAsync, and Task.Delay ensures the server-side
     * loop exits promptly instead of wasting resources on a dead connection.
     */
    public async Task HandleAsync(HttpContext context)
    {
        // --- response headers ---
        context.Response.ContentType = "text/event-stream";
        context.Response.Headers["Cache-Control"] = "no-cache";       // no proxy caching
        context.Response.Headers["X-Accel-Buffering"] = "no";         // disable nginx buffering

        System.IO.Stream body = context.Response.Body;
        System.Threading.CancellationToken ct = context.RequestAborted;

        try
        {
            for (int i = 1; ; i++)
            {
                // Format: named event "tick" with a data line, terminated by blank line
                string eventText =
                    $"id: {i}\n" +
                    $"event: tick\n" +
                    $"data: {{\"seq\":{i},\"time\":\"{DateTime.UtcNow:O}\"}}\n\n";

                byte[] bytes = Encoding.UTF8.GetBytes(eventText);

                await body.WriteAsync(bytes, ct);   // write event bytes
                await body.FlushAsync(ct);           // push to client immediately

                await Task.Delay(2000, ct);          // 2-second interval between events
            }
        }
        catch (OperationCanceledException)
        {
            // client disconnected (navigated away, closed EventSource) — normal exit
            Console.WriteLine("[SSE] Client disconnected.");
        }
    }
}
