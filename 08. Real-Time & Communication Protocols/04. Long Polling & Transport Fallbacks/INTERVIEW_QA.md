# Long Polling & Transport Fallbacks — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — Long Polling & Transport Fallbacks (Interview Traps)](#gotchas--long-polling--transport-fallbacks-interview-traps)

---

## Gotchas — Long Polling & Transport Fallbacks (Interview Traps)

---

#### Gotcha 1. Load Balancer Idle Timeout Must Be Longer Than the Server's Long-Poll Hold Time

**Concepts**
- Long polling holds an HTTP request open until data is available or a timeout fires
- Load balancers close TCP connections idle for longer than their configured idle timeout
- If the server holds a request for 90 s and the LB idle timeout is 60 s, the LB silently closes it
- The server's max hold time must always be shorter than every hop's idle timeout

**Answer**

A long-poll handler holds the client's HTTP request open until the server has data or a configured timeout expires. If the request is held open longer than the idle timeout of any upstream device — load balancer, reverse proxy, or corporate HTTP proxy — that device silently closes the TCP connection. The client receives an abrupt disconnect rather than a completed response, and the server may not detect the disconnect immediately, holding the request context open and leaking memory. The server's maximum hold time must be set shorter than the smallest idle timeout in the path. AWS ALB defaults to 60 seconds; many corporate proxies use 30 seconds. Setting the server's long-poll timeout to 25–45 seconds leaves margin for all realistic hop configurations.

---

#### Gotcha 2. Duplicate Message Delivery Occurs When the Client Retries Before Receiving the Response

**Concepts**
- Client retries after a timeout before the server's response has arrived
- Server delivers the message to the original request; client opens a new request and receives it again
- Sequence numbers or message IDs sent by the client prevent processing duplicates
- At-least-once delivery is the default; at-exactly-once requires client-side deduplication

**Answer**

In long polling, the client opens a request and waits. If the network is slow, the client may apply its own timeout and open a second request before the first response arrives. The server sends the message on the original connection; the client processes it when the first request resolves. The client then also processes the same message when the second connection receives it on the next push cycle, depending on implementation. To prevent duplicate processing, the server must include a sequence number or unique message ID in each response, and the client must track the last processed ID and discard messages with IDs it has already seen. Failing to implement idempotent message handling leads to double-processing of events — a subtle data integrity issue that only surfaces under poor network conditions.

---

#### Gotcha 3. The Server Must Honour CancellationToken to Release Resources When the Client Disconnects

**Concepts**
- When a client closes the browser tab, HttpContext.RequestAborted is signalled
- A long-poll handler awaiting a channel or semaphore must observe the cancellation token
- Without cancellation, the handler continues waiting until its own timeout, wasting server resources
- Accumulated orphaned long-poll tasks create memory pressure at scale

**Answer**

When a long-polling client disconnects — tab closed, navigation away, process killed — ASP.NET Core signals `HttpContext.RequestAborted`. A long-poll handler that is blocked on `await channel.Reader.WaitToReadAsync()` or `await Task.Delay(timeout)` without passing this token will continue to wait for its configured hold time even though the client is gone. Under high load, each orphaned handler holds an `HttpContext`, a channel reader, and potentially database or cache resources. The fix is to pass `HttpContext.RequestAborted` (or a token that links to it) to every await in the long-poll handler, so that the task is cancelled immediately when the client disconnects and all associated resources are released.

---

#### Gotcha 4. Messages Emitted Between Two Consecutive Poll Requests Are Silently Lost

**Concepts**
- After the server responds to a poll, there is a gap before the client opens the next request
- Any message emitted during this gap is never delivered unless the server queues it
- A server-side queue per client with a configurable max depth is required for reliable delivery
- Long polling is inherently at-risk-of-gap delivery without explicit buffering

**Answer**

Long polling creates a structural gap between the moment the server sends a response and the moment the client opens the next poll request. Any event that fires during this gap is emitted with no active request to receive it. Without a server-side per-client message queue, those events are silently dropped. The client receives a clean response for every poll but may miss significant state changes that occurred between polls. A correct implementation queues all events for each client identifier, returns all queued events on the next poll request, and clears them from the queue only after the client acknowledges receipt. Implementing this correctly at scale requires a durable store (Redis, database) rather than in-process memory.

---

#### Gotcha 5. Long Polling Under Load Generates More HTTP Overhead Than an Equivalent WebSocket Connection

**Concepts**
- Each long-poll cycle involves a full HTTP request-response pair including headers
- On HTTP/1.1 without HPACK, all headers are re-sent on every request
- 10,000 concurrent long-poll clients generate 10,000 active HttpContext objects and task continuations
- WebSocket connections amortise the handshake cost over the full connection lifetime

**Answer**

Every long-poll cycle is a complete HTTP transaction: request headers, response headers, and body, all over a connection that may need a new TCP handshake if the previous connection was closed. At ten thousand concurrent clients each polling every 30 seconds, the server processes approximately 333 requests per second of overhead traffic carrying no application data. Each pending long-poll request holds a live ASP.NET Core `HttpContext`, middleware state, and task continuation in memory. WebSocket connections amortise the entire HTTP overhead into a single upgrade handshake and then exchange only two-to-fourteen bytes of framing per message. Long polling is a last-resort fallback for environments where WebSocket upgrades cannot pass; it is not an equally performant alternative.

---

#### Gotcha 6. Long Polling Requests Look Identical to Slow Normal HTTP Requests to Monitoring Tools

**Concepts**
- APM tools (Application Insights, Datadog) flag long-held requests as slow or erroring
- P95/P99 latency metrics are inflated by intentional long-poll holds
- Long-poll endpoints should be excluded from normal latency SLO dashboards
- Separate metrics tracking poll hold time vs. queue wait time are needed

**Answer**

Application Performance Monitoring tools measure request duration. A long-poll request that is held for 30 seconds before responding registers as a 30-second response time. This inflates P95 and P99 latency metrics, triggers slow-request alerts, and creates noisy dashboards that obscure real performance problems. Teams that are unaware their monitoring is capturing long-poll endpoints alongside regular API endpoints may spend hours investigating a "latency regression" that is actually a correctly functioning long-poll implementation. Long-poll endpoints should be tagged and excluded from standard latency SLOs, and separate metrics should track the hold time (how long the server waited before pushing data) versus the actual queue processing time.

---

#### Gotcha 7. IIS Request Queue Depth Is Exhausted When Long-Poll Connections Fill All Worker Threads

**Concepts**
- Classic IIS with ASP.NET on the integrated pipeline allocates a thread per active request
- Async long polling on ASP.NET Core does not block threads, but IIS connection limits still apply
- applicationPool maxConcurrentRequestsPerCPU limits parallel requests regardless of async usage
- Kestrel is preferred for long-polling workloads because it handles connections with asynchronous I/O

**Answer**

ASP.NET Core's async model means that a long-poll handler waiting on `await channel.Reader.WaitToReadAsync()` does not block a thread — the thread returns to the pool while the continuation is parked. However, IIS imposes its own connection and request queue limits via `applicationPool maxConcurrentRequestsPerCPU` and the HTTP.sys queue. If the queue depth is smaller than the number of concurrent long-poll clients, IIS returns 503 Service Unavailable to incoming connections. Kestrel, which handles connections with pure asynchronous I/O on a small number of threads, handles large numbers of concurrent long-poll connections far more efficiently. Deploying ASP.NET Core long-poll applications behind IIS without tuning the request queue depth is a common cause of 503 errors that only appear at moderate to high connection counts.

---

#### Gotcha 8. Long Polling Is a Fallback of Last Resort, Not an Acceptable Default Transport

**Concepts**
- SignalR uses long polling only when WebSockets and SSE are both unavailable
- Treating long polling as equivalent to WebSockets in capability is incorrect
- Long polling introduces round-trip latency equal to at least one network RTT per message
- A half-duplex long-poll channel cannot stream data; each response carries a discrete payload

**Answer**

Long polling is the final fallback in SignalR's transport negotiation chain, used only when WebSocket upgrades and SSE are both blocked by the network environment. Its per-message latency is at minimum one round-trip time plus server hold time, compared to sub-millisecond delivery on an open WebSocket. Its half-duplex nature means the client cannot receive the next message until it has sent the next request, which bounds throughput to roughly one message per RTT. Teams that discover their SignalR application is using long polling in production — typically because a proxy strips WebSocket upgrade headers — often mistake this for a code bug rather than a transport degradation. The correct response is to fix the proxy configuration so WebSocket upgrades pass through. Long polling is a safety net, not a production transport strategy.

---

#### Gotcha 9. A Race Condition Between the Server Response and the Client's Next Request Can Lose Events

**Concepts**
- Server sends response A; client processes it and immediately sends request B
- Server emits event C before request B arrives; C has no pending request to attach to
- Without server-side queuing, C is dropped
- This race is more likely on low-latency connections where the client cycles fast

**Answer**

Long polling creates a window between the server completing one response and the client submitting the next request. On low-latency connections this window is very short — single-digit milliseconds — but events can still arrive during it. A naive implementation that delivers events only to an active request and discards them if no request is pending will drop events at a frequency proportional to the product of the event rate and the round-trip time. This race is invisible in most test conditions because test clients cycle quickly and event rates are low, but it surfaces in production under high event rates or degraded networks. Robust long polling requires a per-client event buffer on the server so that events emitted during the gap are held and delivered on the next incoming request.

---

#### Gotcha 10. Polling Interval Tuning Is a Balance Between Latency and Server Load

**Concepts**
- Short hold times reduce message latency but increase request rate and server load
- Long hold times reduce server load but increase maximum message delivery latency
- Adaptive hold times (release immediately on data, hold up to max on silence) give the best trade-off
- Client-initiated poll frequency must be coordinated with server-side timeout configuration

**Answer**

Long polling with a very short server hold time degrades into regular polling: the server holds the request for one second, returns with nothing, and the client immediately re-polls. This has nearly the same overhead as polling without the long-polling benefit of immediate delivery when data is available. A very long hold time reduces server request rate but means that a message emitted shortly after a poll response is received will wait up to the full hold duration before the client picks it up. The optimal strategy is adaptive: the server releases the request immediately when data is available and holds it up to a configured maximum (e.g. 30 seconds) when there is nothing to send. This gives near-immediate delivery when events occur frequently and reduces overhead to one request per 30 seconds during idle periods.

---
