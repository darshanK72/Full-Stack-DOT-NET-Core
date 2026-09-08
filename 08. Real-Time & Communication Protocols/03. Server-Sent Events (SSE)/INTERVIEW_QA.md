# Server-Sent Events (SSE) — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — Server-Sent Events (SSE) (Interview Traps)](#gotchas--server-sent-events-sse-interview-traps)

---

## Gotchas — Server-Sent Events (SSE) (Interview Traps)

---

#### Gotcha 1. Response Buffering on Nginx or IIS Swallows SSE Frames Before They Reach the Client

**Concepts**
- Nginx proxy_buffering on (default) accumulates the upstream response body before forwarding
- SSE events are never delivered to the client until the server closes the stream, which defeats the purpose
- proxy_buffering off or X-Accel-Buffering: no disables response buffering for SSE endpoints
- IIS ARR also buffers responses and requires responseBufferLimit="0" in the web.config

**Answer**

SSE works by holding an HTTP response open and writing event frames incrementally, flushing after each one. A buffering reverse proxy accumulates the upstream response body before forwarding it to the client, which means event frames sit in the proxy's buffer indefinitely — never reaching the browser until the server finally closes the stream. This completely defeats the purpose of SSE. The fix on Nginx is to add `proxy_buffering off;` to the SSE location block, or to include the `X-Accel-Buffering: no` response header from the server, which Nginx honours as a per-response override. On IIS with Application Request Routing, buffering must be disabled with `responseBufferLimit="0"` in the `<webFarms>` configuration. This is the most common reason SSE works in direct Kestrel tests but fails silently behind any reverse proxy.

---

#### Gotcha 2. The EventSource API Reconnects Automatically — Sending last-event-id Is Not Optional

**Concepts**
- EventSource reconnects after a configurable retry interval (default ~3 seconds) when the connection drops
- The browser automatically sends Last-Event-ID header on reconnect if the server issued id: fields
- Without id: fields in events, the client loses its position and may miss events after reconnect
- The server must replay missed events based on Last-Event-ID from a durable event log

**Answer**

The browser's `EventSource` API reconnects automatically when the SSE connection drops. On each reconnect it includes a `Last-Event-ID` header containing the ID of the last event it received — but only if the server included `id:` fields in its event frames. Without `id:` fields, the browser has nothing to send back, and after each reconnect it receives only events that occur after the new connection is established, silently dropping any events that fired during the disconnection window. The server must assign monotonically increasing IDs to events, store them in a durable log, and replay all events with IDs greater than `Last-Event-ID` at the start of each new connection. This is the SSE equivalent of at-least-once delivery and is non-trivial to implement correctly under concurrent writers.

---

#### Gotcha 3. SSE Is Unidirectional — Clients Cannot Send Data Back Over the Same Connection

**Concepts**
- SSE is a one-way server-to-client stream; there is no mechanism to send client-to-server messages on it
- Client-to-server communication requires separate HTTP POST requests
- This asymmetry makes SSE unsuitable for interactive or bidirectional protocols
- WebSockets or SignalR are the correct choice when bidirectional communication is needed

**Answer**

SSE sends data from server to client only. The browser's `EventSource` API has no method to send data back to the server. Any client-to-server communication must happen through entirely separate HTTP requests — typically `fetch` or `XMLHttpRequest` POST calls. This is architecturally clean for read-only feeds (stock prices, live logs, progress updates) but becomes awkward for interactive scenarios where the user's actions need to influence the stream content. Developers who pick SSE because it is simpler than WebSockets sometimes discover mid-project that they need bidirectional communication and have to retrofit a separate POST channel or migrate to WebSockets. Establishing the correct choice at design time avoids this.

---

#### Gotcha 4. Gzip Compression Must Be Disabled for SSE Endpoints to Allow Incremental Flushing

**Concepts**
- Gzip and Brotli compression require the full response body before producing output
- A streaming SSE endpoint can never produce a complete body, so compressed SSE never flushes
- UseResponseCompression() in ASP.NET Core must exclude text/event-stream MIME type
- Alternatively, set no-transform in Cache-Control to signal that intermediaries must not compress

**Answer**

Response compression middleware — both in ASP.NET Core via `UseResponseCompression()` and in intermediate proxies — buffers the entire response body to compress it before forwarding. An SSE stream has no end until the client disconnects, so a compressor that waits for the full body will hold all event frames indefinitely and never deliver them. The fix is to exclude `text/event-stream` from the list of compressible MIME types in the ASP.NET Core compression options, and to ensure any upstream Nginx or CDN compression rules also exclude that content type. Including `Cache-Control: no-transform` in the SSE response header tells downstream proxies and CDNs not to apply content transformations including compression.

---

#### Gotcha 5. SSE over HTTP/1.1 Consumes One of the Browser's Six Per-Origin Connections

**Concepts**
- HTTP/1.1 browsers allow 6 simultaneous TCP connections per origin
- Each open SSE stream occupies one connection for its entire lifetime
- Multiple SSE subscriptions from the same page starve other HTTP/1.1 requests
- HTTP/2 multiplexing eliminates this problem by hosting SSE streams as separate streams on one connection

**Answer**

An open SSE connection is a long-lived HTTP/1.1 response that keeps a TCP connection occupied indefinitely. Browsers limit connections to six per origin on HTTP/1.1. A page that opens two or three SSE subscriptions simultaneously — one for chat, one for notifications, one for live data — leaves only three or fewer connections for regular API calls, image loads, and asset fetches, degrading overall page performance. This is a well-known SSE limitation on HTTP/1.1 that led some teams to abandon SSE in favour of WebSockets. Over HTTP/2, each SSE stream occupies one multiplexed HTTP/2 stream on a single TCP connection, making the problem disappear entirely. Ensuring that both the server and any reverse proxy support HTTP/2 end-to-end is therefore important for SSE-heavy applications.

---

#### Gotcha 6. SSE Event Frames Without a Trailing Double Newline Are Silently Ignored by the Browser

**Concepts**
- SSE frame format: "data: payload\n\n" — a double newline signals the end of one event
- A single newline after data continues the multi-line value; only double newline dispatches the event
- Forgetting the second newline causes the browser to buffer frames without ever firing the message event
- The colon prefix (": comment") creates a comment that keeps the connection alive without dispatching events

**Answer**

SSE events are delimited by blank lines: each event is one or more `data:`, `id:`, `event:`, or `retry:` lines followed by a blank line (two consecutive newlines). A single newline after `data: payload` does not dispatch the event — it continues the current event's data field. Only the blank line (the second newline) triggers the browser to dispatch the `message` event. An SSE endpoint that writes `data: {json}\n` and flushes but never writes the second `\n` will accumulate all events silently in the browser's parser without ever firing any event listeners. This is invisible in the browser's network inspector since the bytes are delivered; the error appears only in the application when event listeners never fire.

---

#### Gotcha 7. The retry: Field Controls Reconnect Interval But Does Not Guarantee Delivery During Downtime

**Concepts**
- retry: N sets the reconnect delay in milliseconds that the browser respects
- A short retry interval increases reconnect frequency but does not prevent missed events
- Durable event replay via Last-Event-ID is the only mechanism for guaranteed delivery
- Setting retry too short (under 1000 ms) floods the server during instability

**Answer**

The `retry:` field in an SSE event frame tells the browser how many milliseconds to wait before attempting to reconnect after a connection drop. Setting `retry: 1000` causes the browser to reconnect after one second, which is faster than the default three-second delay. However, a shorter retry interval does not prevent missed events — if five events were emitted during the one-second gap, they are still lost unless the server replays them based on `Last-Event-ID`. Setting retry too short during a server restart storm also amplifies the reconnect load: if a server serving ten thousand SSE clients restarts and all clients reconnect within one second, the server faces an immediate thundering-herd of ten thousand simultaneous connection attempts. A randomised exponential backoff strategy on the client, or a longer minimum retry delay, mitigates this.

---

#### Gotcha 8. Not Setting Cache-Control: no-cache Causes Proxies to Cache the SSE Stream

**Concepts**
- SSE endpoints must include Cache-Control: no-cache to prevent proxy caching
- A cached SSE response is served as a static response to subsequent clients — no live events
- Varnish, Squid, and CDN edge nodes may cache text/event-stream responses without explicit directives
- ASP.NET Core does not set no-cache automatically for SSE endpoints

**Answer**

An SSE response is an ongoing HTTP response, not a static document, but a caching proxy that lacks explicit directives may attempt to cache it. If the proxy caches the SSE response, subsequent clients receive a replayed copy of old events rather than a live stream — a silent failure that is very difficult to diagnose because the network tab shows events arriving normally. The SSE response must include `Cache-Control: no-cache` (and ideally `Cache-Control: no-store`) to prevent any intermediary from caching the stream. ASP.NET Core does not add these headers automatically for SSE endpoints; they must be set explicitly with `Response.Headers.CacheControl = "no-cache"` or through response middleware before the first flush.

---

#### Gotcha 9. Named Events Require addEventListener, Not the onmessage Handler

**Concepts**
- SSE events without an "event:" field are dispatched as "message" events
- Events with "event: mytype" are dispatched under the "mytype" event name
- EventSource.onmessage only handles unnamed events; named events are silently ignored by it
- EventSource.addEventListener("mytype", handler) is required for named events

**Answer**

SSE supports named events via the `event:` field: `event: orderUpdate\ndata: {json}\n\n` dispatches an event named `orderUpdate`. The `EventSource.onmessage` shorthand handler only fires for events without an `event:` field — it receives events named `message`. Named events are silently ignored by the `onmessage` handler, with no console warning. An application that sends `event: orderUpdate` frames and registers `source.onmessage = handler` will receive zero callbacks, and the developer may spend a long time diagnosing why the connection shows frames arriving in the network tab but no application-level events fire. The fix is `source.addEventListener("orderUpdate", handler)` for each distinct event type, or to drop the `event:` field from all frames and handle them uniformly as `message` events.

---

#### Gotcha 10. ASP.NET Core Must Flush the Response After Each SSE Frame — Writing Alone Is Not Enough

**Concepts**
- Response.WriteAsync writes bytes to the output buffer; flushing sends them to the client
- Without explicit flush, frames accumulate in the buffer and are delivered in bursts
- Response.Body.FlushAsync() or PipeWriter.FlushAsync() flushes the buffer immediately
- IIS buffering must also be disabled, or flush calls have no effect through the proxy

**Answer**

In ASP.NET Core, writing to the response body with `Response.WriteAsync("data: ...\n\n")` places bytes in a kernel or application buffer; it does not guarantee that bytes reach the client immediately. Without an explicit flush after each event frame, the framework may batch multiple writes and deliver them together when the buffer fills or on a timer, causing SSE clients to receive events in delayed bursts rather than immediately when emitted. The fix is to call `await Response.Body.FlushAsync()` or `await Response.BodyWriter.FlushAsync()` after each event write. Even with explicit flushing, if the upstream Nginx or IIS proxy is buffering responses (see Gotcha 1), flush calls have no observable effect on the client — both problems must be solved together for SSE to work correctly end-to-end.

---
