# Resilience & Circuit Breaker — Interview Q&A
> 32 questions · Back to [README](../README.md)

## Table of Contents
1. [What is resilience in the context of microservices, and why does it matter more …](#q1)
2. [What is a transient fault, and how does it differ from a permanent failure?](#q2)
3. [What is a cascade failure, and how do resilience patterns prevent it from propag…](#q3)
4. [What is the difference between fault tolerance (preventing impact) and fault rec…](#q4)
5. [What is the Circuit Breaker pattern, and what problem does it solve in service-t…](#q5)
6. [Describe the three states of a Circuit Breaker — Closed, Open, and Half-Open — a…](#q6)
7. [What metrics does a Circuit Breaker use to decide when to trip open — failure co…](#q7)
8. [What is a sampling window (sliding window), and how does it affect circuit break…](#q8)
9. [What is the difference between a count-based and a time-based sliding window in …](#q9)
10. [What happens to requests when the circuit is Open, and what is a fallback respon…](#q10)
11. [When is a retry safe to apply, and when can it make things worse?](#q11)
12. [What is exponential backoff, and why is it preferred over fixed-interval retries…](#q12)
13. [What is jitter in a retry strategy, and why does it help at scale?](#q13)
14. [What is the difference between a timeout and a cancellation token when making HT…](#q14)
15. [How do you decide the right timeout value for a downstream service call?](#q15)
16. [What is the Bulkhead pattern, and what problem does it solve in microservices?](#q16)
17. [What are the two main types of bulkhead isolation — thread-pool isolation and se…](#q17)
18. [What is rate limiting, and how does it differ from throttling a downstream depen…](#q18)
19. [How does the sliding-window rate limiter differ from the fixed-window and token-…](#q19)
20. [What is the Hedging strategy, and when is it appropriate to use it?](#q20)
21. [What is a fallback policy, and how is it different from simply returning an erro…](#q21)
22. [What is Polly, and what resilience policies does it provide?](#q22)
23. [How do you compose multiple Polly policies together — what is a PolicyWrap or Re…](#q23)
24. [What is the difference between a reactive policy (fault handling) and a proactiv…](#q24)
25. [How does Polly v8's ResiliencePipeline differ from the v7 Policy API?](#q25)
26. [How do you register Polly resilience with IHttpClientFactory in .NET so it appli…](#q26)
27. [What is Microsoft.Extensions.Http.Resilience, and how does it differ from using …](#q27)
28. [What does `AddStandardResilienceHandler()` configure by default, and what can yo…](#q28)
29. [How do you apply per-named-client resilience options using `AddResilienceHandler…](#q29)
30. [What are ASP.NET Core health checks, and how do they contribute to overall syste…](#q30)
31. [How do Kubernetes liveness and readiness probes differ, and how do you configure…](#q31)
32. [What role does a service mesh (such as Istio) or a load balancer play in resilie…](#q32)

---

## Q1. What is resilience in the context of microservices, and why does it matter more than in a monolithic application?

What is resilience in the context of microservices, and why does it matter more than in a monolithic application?

**Answer:** Resilience is a system's ability to continue operating correctly — possibly in a degraded mode — when one or more of its components fail. In a microservices architecture, a single user request typically crosses several network boundaries and calls multiple services, so the probability that at least one dependency is degraded at any moment is meaningfully higher than in a monolith where all code runs in the same process.

- A monolith either works or crashes as a unit, whereas in microservices a slow or failing downstream service can silently stall threads or exhaust connection pools in the caller, causing what appears to be unrelated failures upstream.
- Resilience engineering acknowledges that distributed systems fail partially — individual nodes, databases, and network paths can fail independently — so the design goal is graceful degradation rather than binary availability.
- The cost of neglecting resilience is amplified by the number of inter-service calls; a fleet of fifty services each with 99.9 % availability yields a combined path availability well below 95 % without explicit resilience measures.

---

## Q2. What is a transient fault, and how does it differ from a permanent failure?

What is a transient fault, and how does it differ from a permanent failure?

**Answer:** A transient fault is a temporary error condition that is likely to resolve on its own within milliseconds to seconds, such as a momentary network blip, a connection timeout during a TCP handshake, or a brief HTTP 503 from a service that is restarting. A permanent failure — like a misconfigured URL, an HTTP 404 for a resource that does not exist, or an authentication error — will not resolve by retrying.

- Transient faults are the primary target of retry and circuit-breaker policies; retrying a permanent failure wastes resources and delays the caller's error response without any benefit.
- Distinguishing them in code requires inspecting the exception type or HTTP status code — for example, HTTP 429 (Too Many Requests) and 503 are generally transient, while 400 (Bad Request) and 401 (Unauthorized) are not.
- Polly and Microsoft.Extensions.Http.Resilience both ship with default predicates that classify common HTTP status codes as transient, and these can be extended for service-specific error contracts.

---

## Q3. What is a cascade failure, and how do resilience patterns prevent it from propagating?

What is a cascade failure, and how do resilience patterns prevent it from propagating?

**Answer:** A cascade failure occurs when the failure or slow-down of one service causes the services that depend on it to also fail, which in turn causes their callers to fail, propagating the outage upward through the call graph until a large portion of the system is unavailable. The classic trigger is a slow downstream service that holds threads or connections in the caller, which eventually exhausts the caller's thread pool and renders it unable to serve any request — including those that do not touch the faulty dependency.

- The Circuit Breaker pattern stops cascade failures by refusing to forward calls to a known-failing service rather than waiting for each one to time out, freeing threads immediately.
- Bulkhead isolation limits the blast radius by giving each downstream dependency its own bounded pool of threads or semaphore slots, so saturation against one service cannot consume resources meant for another.
- Timeout policies ensure that waiting threads are released even when the circuit breaker has not yet opened, bounding the time any single dependency can hold a resource.

---

## Q4. What is the difference between fault tolerance (preventing impact) and fault recovery (restoring state)?

What is the difference between fault tolerance (preventing impact) and fault recovery (restoring state)?

**Answer:** Fault tolerance means designing the system so that a fault in one component does not propagate to cause visible degradation in others — the system keeps working, possibly with reduced functionality, while the fault is present. Fault recovery is the complementary process of detecting the fault and restoring the system to its fully operational state after the fault clears.

- Circuit breakers, bulkheads, and fallbacks are fault-tolerance mechanisms — they absorb the impact of a failure without necessarily fixing the underlying cause.
- Health checks, readiness probes, self-healing restart policies in Kubernetes, and the Half-Open state of a circuit breaker are fault-recovery mechanisms — they detect that a service has recovered and restore it to normal traffic.
- In practice both must be designed together: tolerance limits the damage while recovery determines how quickly normal operation resumes.

---

## Chapter 2 — Circuit Breaker Pattern

---

## Q5. What is the Circuit Breaker pattern, and what problem does it solve in service-to-service calls?

What is the Circuit Breaker pattern, and what problem does it solve in service-to-service calls?

**Answer:** The Circuit Breaker pattern wraps calls to a remote service and monitors the failure rate of those calls; when failures exceed a configured threshold, the circuit "trips open" and subsequent calls are immediately rejected (or redirected to a fallback) without being forwarded to the remote service. This prevents a failing or slow downstream service from tying up threads in the caller through timeout waits, and gives the downstream service time to recover without being bombarded by traffic.

- The pattern is named after an electrical circuit breaker: just as a household breaker interrupts current to protect wiring from overload, the software circuit breaker interrupts requests to protect the calling service from resource exhaustion.
- Without a circuit breaker, a service calling a dependency that takes 30 seconds to time out will accumulate new requests faster than old ones complete, eventually exhausting its thread pool and becoming unresponsive itself.
- After a configurable break duration, the circuit moves to Half-Open and allows a small probe volume through to test whether the downstream has recovered.

---

## Q6. Describe the three states of a Circuit Breaker — Closed, Open, and Half-Open — and the transitions between them.

Describe the three states of a Circuit Breaker — Closed, Open, and Half-Open — and the transitions between them.

**Answer:** A circuit breaker operates as a state machine with three states that control whether calls are forwarded to the dependency. In the Closed state all calls pass through and failures are counted; when the failure metric crosses the threshold the circuit transitions to Open. In the Open state all calls are rejected immediately without contacting the dependency. After a configured break duration the circuit transitions to Half-Open, where a limited number of probe calls are allowed through; if they succeed the circuit returns to Closed, and if they fail it returns to Open.

| State | Behavior | Transition trigger |
|---|---|---|
| **Closed** | All calls forwarded normally | Failure rate/count exceeds threshold → Open |
| **Open** | All calls rejected immediately (fast-fail) | Break duration elapses → Half-Open |
| **Half-Open** | Limited probe calls forwarded | Probes succeed → Closed; probes fail → Open |

- The break duration in the Open state gives the downstream service breathing room to restart or recover without being flooded.
- The Half-Open probe volume is intentionally small — typically 1 to 5 requests — to avoid overwhelming a service that is still fragile.

---

## Q7. What metrics does a Circuit Breaker use to decide when to trip open — failure count, failure rate, or something else?

What metrics does a Circuit Breaker use to decide when to trip open — failure count, failure rate, or something else?

**Answer:** Modern circuit breaker implementations typically evaluate the failure rate (percentage of failed calls within a sliding window) rather than a raw failure count, because a raw count can be misleading — 5 failures out of 5 calls is very different from 5 failures out of 5 000. Additionally, some implementations also consider slow calls (calls that complete but exceed a latency threshold) as a separate metric that can contribute to tripping the circuit.

- In Polly v8, the circuit breaker opens when the failure ratio (e.g., 50 %) is exceeded within a minimum number of calls that must be observed in the sampling window — both thresholds must be met simultaneously to avoid tripping on a cold start with a single failure.
- Slow-call rate thresholds allow the breaker to open proactively when a dependency is degraded but not fully failing — for example, when responses take 10 seconds instead of the normal 200 ms.
- Using rate rather than count makes the threshold environment-agnostic: the same policy works correctly whether the service is processing 10 or 10 000 requests per second.

---

## Q8. What is a sampling window (sliding window), and how does it affect circuit breaker sensitivity?

What is a sampling window (sliding window), and how does it affect circuit breaker sensitivity?

**Answer:** A sampling window defines the scope over which the circuit breaker aggregates failure metrics before deciding whether to trip. The window moves forward in time as requests arrive, so only recent calls contribute to the current failure rate rather than the full lifetime of the service. A narrower window makes the circuit breaker more responsive to sudden bursts of failures but also more prone to tripping on statistical noise from a small sample.

- Without a window, a single failure at startup would permanently influence the circuit breaker's decision, which is not useful because historical failures are rarely predictive of current health.
- A wide window (e.g., 120 seconds) smooths out brief spikes but may delay tripping when a real outage begins, causing more cascading damage before the circuit opens.
- Choosing window size requires balancing sensitivity (catch real outages quickly) against specificity (avoid false trips on brief transient spikes).

---

## Q9. What is the difference between a count-based and a time-based sliding window in a circuit breaker?

What is the difference between a count-based and a time-based sliding window in a circuit breaker?

**Answer:** A count-based sliding window tracks the most recent N requests, regardless of how long they took to arrive, so the window always contains exactly N samples. A time-based sliding window tracks all requests that arrived within the last T seconds, so the sample size varies with throughput. Both windows calculate the failure rate from their samples, but they respond differently under varying load.

| Dimension | Count-based | Time-based |
|---|---|---|
| **Window boundary** | Last N requests | Last T seconds |
| **Sample size** | Fixed (N) | Variable (depends on request rate) |
| **Low-traffic behavior** | Slow to refresh — old samples persist | Refreshes by time even at low traffic |
| **High-traffic behavior** | Reflects very recent requests accurately | May aggregate thousands of samples |
| **Polly v8 default** | Supported | Supported — both are available |

- A count-based window can leave stale data in the window for a long time if request rate drops, meaning a past failure continues to count long after the dependency recovered.
- A time-based window naturally expires old samples, making it a better fit for services with variable or bursty load.

---

## Q10. What happens to requests when the circuit is Open, and what is a fallback response?

What happens to requests when the circuit is Open, and what is a fallback response?

**Answer:** When the circuit is Open, the circuit breaker immediately throws an exception (typically `BrokenCircuitException` in Polly) or invokes a configured fallback without contacting the remote service at all, so the caller receives a response in microseconds rather than waiting for a timeout. A fallback response is a predetermined safe answer returned in place of the real response — it might be a cached value, an empty collection, a default object, or a graceful degradation message.

- Fast-failing while Open is crucial: it prevents threads from accumulating waiting for timeout durations (e.g., 30 seconds each), which is what leads to thread pool exhaustion and cascade failures.
- A fallback should be semantically meaningful — for example, a product recommendations service might return an empty list rather than crashing the checkout page, allowing the user to proceed without personalized suggestions.
- Fallbacks are registered separately from the circuit breaker in Polly; together they form a common `ResiliencePipeline` where the fallback wraps the circuit breaker so that the broken-circuit exception is caught and the fallback value returned.

---

## Chapter 3 — Retry & Timeout Strategies

---

## Q11. When is a retry safe to apply, and when can it make things worse?

When is a retry safe to apply, and when can it make things worse?

**Answer:** A retry is safe when the operation is idempotent — meaning repeating it produces the same result as performing it once — and when the failure is classified as transient. Retrying a non-idempotent operation (such as placing an order or charging a payment) can cause duplicate side effects, while retrying a permanent failure (such as a 400 Bad Request due to invalid input) wastes resources without any chance of success.

- HTTP GET requests are generally idempotent and safe to retry; POST requests that mutate state are not, unless the server has idempotency-key support.
- Retrying during an overload condition (HTTP 429 or 503) can worsen the situation at the downstream service by increasing its incoming load exactly when it cannot handle more — this is why retry should always be paired with backoff.
- For write operations where retry is needed, the recommended approach is to make writes idempotent via an idempotency key so that the same request sent twice has the same net effect as sending it once.

---

## Q12. What is exponential backoff, and why is it preferred over fixed-interval retries?

What is exponential backoff, and why is it preferred over fixed-interval retries?

**Answer:** Exponential backoff is a retry delay strategy where the wait time between successive attempts grows exponentially — for example, 1 second, 2 seconds, 4 seconds, 8 seconds — rather than being a fixed interval. This reduces the load placed on a recovering downstream service and prevents multiple callers from hammering it simultaneously at regular intervals.

- Fixed-interval retries can create a synchronized thundering herd: if thousands of clients all retry every 5 seconds, they all hit the recovering service at the same moments and may overwhelm it again before it can stabilize.
- Exponential backoff disperses retry attempts over a longer window of time, giving the downstream service progressively more time to recover between attempts.
- A maximum delay cap (e.g., 30 seconds) is typically applied so that backoff does not grow so large that the caller waits forever; after the cap, subsequent retries use the capped delay.

---

## Q13. What is jitter in a retry strategy, and why does it help at scale?

What is jitter in a retry strategy, and why does it help at scale?

**Answer:** Jitter is a small random amount of time added to each retry delay, so that different clients retrying at the same time do not produce synchronized spikes in load against the dependency. Even with exponential backoff, if all clients start a burst simultaneously they will all retry at intervals that are exponentially offset from the same origin, causing correlated spikes; jitter breaks that correlation.

- Without jitter, 1 000 clients that all fail at second 0 and retry with backoff of 1, 2, 4 seconds will all produce load spikes at roughly second 1, second 2, and second 4 simultaneously.
- With full jitter, each client independently picks a random delay between 0 and the backoff cap, so load is spread roughly uniformly across the entire retry window.
- AWS published influential research on jitter strategies (full jitter, equal jitter, decorrelated jitter) showing that full jitter produces the best load distribution at scale; Polly's default retry uses a similar approach.

---

## Q14. What is the difference between a timeout and a cancellation token when making HTTP calls?

What is the difference between a timeout and a cancellation token when making HTTP calls?

**Answer:** A timeout is a policy-level construct that says "if this operation has not completed within N seconds, abandon it and treat it as a failure." A `CancellationToken` is a .NET mechanism that signals an in-flight operation to stop what it is doing; a timeout policy typically works by cancelling the operation's cancellation token after the configured duration, but cancellation tokens can also be triggered by other sources such as the user closing the browser or the server shutting down.

- `HttpClient` has a `Timeout` property that creates an internal `CancellationToken` that fires after the configured duration; Polly's timeout strategy wraps the call with its own cancellation token that fires independently.
- Using `CancellationToken` directly gives callers fine-grained control: the ASP.NET Core framework automatically passes the request's cancellation token to action methods so that HTTP calls can be cancelled when the HTTP client disconnects.
- Polly's timeout strategy differs from `HttpClient.Timeout` in that it integrates with the resilience pipeline and records the timeout as a metric that feeds the circuit breaker, whereas `HttpClient.Timeout` fires independently.

---

## Q15. How do you decide the right timeout value for a downstream service call?

How do you decide the right timeout value for a downstream service call?

**Answer:** The right timeout is derived from the service's actual performance data — specifically the high-percentile latency (P95 or P99) of successful calls under normal load — with a small safety margin added. Setting the timeout at the mean latency would cancel a significant portion of legitimate requests; setting it too high defeats the purpose of bounding wait time.

- A common rule of thumb is to set the timeout at 2–3 times the P99 latency measured in production under normal conditions, so that genuine slowdowns are distinguished from normal variance.
- End-to-end timeout budgets must be considered: if the overall request timeout for the frontend is 5 seconds and the call chain passes through three services, each service's timeout must be set to leave time for subsequent hops (e.g., 1.5 seconds each rather than 5 seconds each).
- Timeouts should be reviewed when the downstream service changes its SLA or when profiling reveals that a new code path is slower than historical baselines.

---

## Chapter 4 — Bulkhead & Rate Limiting

---

## Q16. What is the Bulkhead pattern, and what problem does it solve in microservices?

What is the Bulkhead pattern, and what problem does it solve in microservices?

**Answer:** The Bulkhead pattern limits the number of concurrent requests a service sends to a specific dependency, preventing a slow or failing dependency from consuming all of the caller's resources (threads, connections, or semaphore slots) and degrading calls to completely unrelated dependencies. The name comes from the watertight compartments (bulkheads) in a ship's hull that prevent a breach in one section from flooding the whole vessel.

- Without bulkheads, a service that calls three dependencies (A, B, C) over a shared thread pool can have that entire pool saturated by slow calls to dependency A, making the service unable to process requests that only touch B or C.
- Bulkheads enforce isolation by giving each dependency its own bounded resource allocation — for example, at most 10 concurrent in-flight calls to service A and at most 20 to service B.
- When the bulkhead limit is reached, additional calls are either rejected immediately or queued up to a secondary limit; rejection is preferable for time-sensitive requests because queuing may just defer the failure.

---

## Q17. What are the two main types of bulkhead isolation — thread-pool isolation and semaphore isolation — and how do they differ?

What are the two main types of bulkhead isolation — thread-pool isolation and semaphore isolation — and how do they differ?

**Answer:** Thread-pool isolation assigns a dedicated pool of threads to each dependency, so calls to that dependency execute only on those threads and cannot borrow threads from other pools. Semaphore isolation uses a shared thread pool but limits the number of concurrent operations via a counting semaphore, rejecting new requests when the semaphore count is exhausted.

| Dimension | Thread-pool isolation | Semaphore isolation |
|---|---|---|
| **Resource** | Dedicated thread pool per dependency | Shared threads, separate semaphore per dependency |
| **Overhead** | Higher — context switches, memory for threads | Lower — semaphore is a lightweight counter |
| **True isolation** | Yes — saturated dependency cannot borrow threads | Partial — threads are shared but count is bounded |
| **Suitable for** | Truly independent workloads, Netflix Hystrix model | Lightweight isolation in async .NET pipelines |

- In .NET async code, the traditional Hystrix-style thread-pool isolation is less natural because async operations do not block a thread while awaiting — semaphore isolation with `SemaphoreSlim` or Polly's `RateLimiter`-backed bulkhead is the more idiomatic choice.
- Polly v8 implements bulkhead isolation via the `RateLimiter` abstraction, which supports both concurrency limits and queue depths.

---

## Q18. What is rate limiting, and how does it differ from throttling a downstream dependency?

What is rate limiting, and how does it differ from throttling a downstream dependency?

**Answer:** Rate limiting controls how many requests a service accepts from its callers (inbound), typically to protect the service from being overwhelmed. Throttling in the resilience context refers to slowing down or limiting outbound calls the service makes to a downstream dependency, which is effectively the Bulkhead pattern applied to throughput rather than concurrency. The two terms are sometimes used interchangeably, but the direction matters: rate limiting is self-defense (protecting yourself from callers), while throttling/bulkhead is courtesy (protecting a dependency from you).

- ASP.NET Core 7+ ships with built-in rate limiting middleware (`Microsoft.AspNetCore.RateLimiting`) that enforces inbound rate limits at the HTTP pipeline level before controllers run.
- Rate limiting a downstream HTTP call (outbound) is better implemented with Polly's `RateLimiter` strategy, which rejects or queues requests before they leave the process.
- Both are needed in a complete system: inbound rate limiting protects your service, and outbound concurrency/rate limits protect your dependencies.

---

## Q19. How does the sliding-window rate limiter differ from the fixed-window and token-bucket algorithms?

How does the sliding-window rate limiter differ from the fixed-window and token-bucket algorithms?

**Answer:** A fixed-window rate limiter allows a fixed number of requests per discrete time window (e.g., 100 per minute) but resets its counter at the window boundary, which can allow a burst of 200 requests if 100 arrive at the end of one window and 100 more arrive at the start of the next. A sliding-window rate limiter tracks requests continuously so the count is always measured over the most recent window, eliminating the boundary burst. A token-bucket algorithm allows short bursts by accumulating tokens when the service is under-utilized and spending them during bursts, up to a bucket capacity.

| Algorithm | Burst handling | Implementation complexity | Use case |
|---|---|---|---|
| **Fixed window** | Allows boundary bursts | Simple | Low-traffic APIs where occasional bursts are acceptable |
| **Sliding window** | No boundary bursts | Moderate | APIs that need strict, consistent rate enforcement |
| **Token bucket** | Controlled bursts allowed | Moderate | APIs where short bursts are legitimate (e.g., batch uploads) |
| **Concurrency limiter** | Limits parallel calls, not rate | Simple | Protecting a downstream with a concurrency cap |

- ASP.NET Core's `RateLimiterOptions` supports all four: `FixedWindowRateLimiter`, `SlidingWindowRateLimiter`, `TokenBucketRateLimiter`, and `ConcurrencyLimiter`.
- The right choice depends on whether you want to prevent bursty traffic (sliding window or concurrency), allow controlled bursts (token bucket), or keep implementation simple (fixed window).

---

## Chapter 5 — Hedging & Fallback

---

## Q20. What is the Hedging strategy, and when is it appropriate to use it?

What is the Hedging strategy, and when is it appropriate to use it?

**Answer:** The Hedging strategy issues a secondary (and optionally further) parallel request to the same endpoint if the first request has not returned within a configured delay, accepting whichever response arrives first and cancelling the slower copy. Unlike retry (which waits for the first attempt to fail before trying again), hedging runs attempts concurrently to reduce tail latency — the extreme cases where the P99 latency is much higher than the median.

- Hedging is appropriate when a service has high variance in response time and latency is more important than minimizing load on the downstream (because hedging by definition sends more requests than retrying does).
- It is safe only when the downstream operation is idempotent: if both copies of the request succeed before the second is cancelled, the downstream must not process a duplicate side effect.
- Polly v8 introduced the `HedgingResilienceStrategy`, configurable with a hedging delay and a maximum hedge count; the strategy cancels the trailing requests as soon as the first successful response arrives.

---

## Q21. What is a fallback policy, and how is it different from simply returning an error response?

What is a fallback policy, and how is it different from simply returning an error response?

**Answer:** A fallback policy intercepts a specific failure condition and substitutes a predetermined safe value or alternative action, so the caller receives a usable (if degraded) response rather than an exception or HTTP error. Returning an error response means propagating the failure upstream as an HTTP 500 or an exception; a fallback instead hides the failure from the caller by substituting something meaningful.

- A fallback is valuable when the downstream data is optional or can be approximated — for example, returning a cached version of a user's recommendation list rather than failing the entire page load.
- Fallbacks should be designed carefully: a fallback that returns an empty list silently may confuse users or downstream logic that expects data; some scenarios benefit from a partial-failure indicator rather than a completely transparent substitution.
- In Polly v8, the `FallbackResilienceStrategy<T>` is configured with a `FallbackAction<T>` delegate and optional `ShouldHandle` predicate so it only activates for specific exception types or result conditions.

---

## Chapter 6 — Polly in .NET

---

## Q22. What is Polly, and what resilience policies does it provide?

What is Polly, and what resilience policies does it provide?

**Answer:** Polly is an open-source .NET resilience and transient-fault-handling library that wraps calls in configurable policies (v7) or strategies (v8) to handle failures consistently across an application. It is the de facto standard resilience library for .NET and is integrated into `IHttpClientFactory` through `Microsoft.Extensions.Http.Polly` (v7) and `Microsoft.Extensions.Http.Resilience` (v8+).

- Polly v7 provides: Retry, Circuit Breaker, Timeout, Bulkhead Isolation, Fallback, Cache, and PolicyWrap (composition).
- Polly v8 redesigned the API around `ResiliencePipeline` and individual strategies: Retry, Circuit Breaker, Timeout, Rate Limiter (replacing Bulkhead), Fallback, and Hedging — all built on `System.Threading.RateLimiting` and modern async patterns.
- A key feature of Polly is that policies are stateless and reusable: a single circuit breaker instance maintains state and tracks failure rate across all callers that use it, so the circuit can trip based on the aggregate failure pattern rather than per-caller.

---

## Q23. How do you compose multiple Polly policies together — what is a PolicyWrap or ResiliencePipeline?

How do you compose multiple Polly policies together — what is a PolicyWrap or ResiliencePipeline?

**Answer:** In Polly v7, multiple policies are combined with `Policy.WrapAsync(outer, inner, ...)`, where the outermost policy in the list is the first to execute and the innermost surrounds the actual operation. In Polly v8 (and newer Microsoft.Extensions.Http.Resilience), a `ResiliencePipelineBuilder` composes strategies in declaration order, where the first `AddX()` call corresponds to the outermost layer.

```csharp
// Polly v8 — strategies execute in declaration order (outer to inner)
var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddFallback(...)       // outermost — catches all failures
    .AddCircuitBreaker(...) // middle — trips on repeated failures
    .AddRetry(...)          // inner — retries transient faults
    .AddTimeout(...)        // innermost — bounds each individual attempt
    .Build();
```

- Order matters: Timeout must be inner so it bounds each individual retry attempt; placing it outer would bound the entire multi-retry operation instead.
- Fallback should be outermost so it catches exceptions from all inner strategies, including the `BrokenCircuitException` thrown when the circuit is Open.
- Circuit Breaker should be inner of Fallback but outer of Retry so that failed retries count toward the circuit breaker's failure metric, and once the circuit opens no more retries are attempted.

---

## Q24. What is the difference between a reactive policy (fault handling) and a proactive policy (fault prevention) in Polly?

What is the difference between a reactive policy (fault handling) and a proactive policy (fault prevention) in Polly?

**Answer:** A reactive policy handles a failure after it has already occurred — it activates when an exception is thrown or when a result value indicates failure (such as an HTTP 5xx status code). A proactive policy prevents a failure from happening by enforcing a constraint before the outcome is known — timeout and rate limiter strategies are proactive because they intervene based on time elapsed or concurrency count, not on whether the operation ultimately failed.

- Retry, Circuit Breaker, and Fallback are reactive: they inspect the result of the operation and respond to failures already observed.
- Timeout and Rate Limiter (Bulkhead in v7) are proactive: Timeout enforces a maximum wait regardless of outcome, and Rate Limiter rejects requests before they are sent when concurrency or throughput limits are exceeded.
- Hedging is a hybrid: it reacts to slow responses (a form of implicit failure) by proactively sending a parallel request before the first one fails, without waiting for a definitive failure signal.

---

## Q25. How does Polly v8's ResiliencePipeline differ from the v7 Policy API?

How does Polly v8's ResiliencePipeline differ from the v7 Policy API?

**Answer:** Polly v8 introduced a completely redesigned API centered on `ResiliencePipeline<T>` and `ResiliencePipelineBuilder<T>`, replacing the v7 `IAsyncPolicy<T>` and `Policy.WrapAsync()` model. The v8 API is built on top of .NET's `System.Threading.RateLimiting` primitives, integrates with `System.Diagnostics.Metrics` for built-in telemetry, and replaces the Bulkhead strategy with a proper Rate Limiter strategy.

| Dimension | Polly v7 | Polly v8 |
|---|---|---|
| **Core abstraction** | `IAsyncPolicy<T>` | `ResiliencePipeline<T>` |
| **Composition** | `Policy.WrapAsync(outer, inner)` | `ResiliencePipelineBuilder.AddX()` (fluent) |
| **Bulkhead** | `BulkheadAsync()` | `AddRateLimiter()` using `System.Threading.RateLimiting` |
| **Telemetry** | External listeners only | Built-in `System.Diagnostics.Metrics` + `DiagnosticSource` |
| **Hedging** | Not available | `AddHedging()` — new in v8 |
| **Non-generic pipeline** | Not available | `ResiliencePipeline` (non-generic) |

- Migration from v7 to v8 requires updating policy registration calls; behavior and semantics are equivalent, but extension method names and namespaces changed.
- The v8 `ResiliencePipelineBuilder` supports `AddResilienceHandler()` directly in the `IHttpClientBuilder` via Microsoft.Extensions.Http.Resilience, which did not exist for v7.

---

## Q26. How do you register Polly resilience with IHttpClientFactory in .NET so it applies to all outgoing HTTP calls?

How do you register Polly resilience with IHttpClientFactory in .NET so it applies to all outgoing HTTP calls?

**Answer:** Named or typed `HttpClient` instances registered through `IHttpClientFactory` can have resilience pipelines attached in `Program.cs` using extension methods from the `Microsoft.Extensions.Http.Resilience` package. The pipeline executes automatically for every request sent by that client, including redirects, without any per-call code.

```csharp
// Program.cs (.NET 8+)
builder.Services.AddHttpClient("PaymentsClient")
    .AddStandardResilienceHandler(); // attaches default pipeline
```

- The resilience handler is added as an additional `DelegatingHandler` in the `HttpClient` middleware chain, so it sits between the caller's code and the actual HTTP transport.
- Each named client has its own independent resilience pipeline instance, so the circuit breaker state for the Payments client is separate from the circuit breaker for the Orders client — they do not share failure counts.
- Custom pipelines can replace the standard one: `.AddResilienceHandler("custom", builder => { builder.AddRetry(...); builder.AddCircuitBreaker(...); })`.

---

## Chapter 7 — Microsoft.Extensions.Http.Resilience (.NET 8+)

---

## Q27. What is Microsoft.Extensions.Http.Resilience, and how does it differ from using Polly directly?

What is Microsoft.Extensions.Http.Resilience, and how does it differ from using Polly directly?

**Answer:** `Microsoft.Extensions.Http.Resilience` is a Microsoft-authored NuGet package (introduced with .NET 8) that provides opinionated, pre-configured resilience pipelines for `HttpClient` instances managed by `IHttpClientFactory`. It is built on top of Polly v8 internally but exposes a higher-level API that integrates with dependency injection, options validation, and telemetry without requiring the caller to configure individual Polly strategies.

- The key addition over raw Polly is `AddStandardResilienceHandler()`, which configures a well-tuned default pipeline (retry, circuit breaker, attempt timeout, total request timeout, and hedging for eligible methods) following Microsoft's best-practice defaults.
- It also provides `AddStandardHedgingHandler()`, which specializes the pipeline for hedging-optimized scenarios like read-heavy endpoints.
- Using this package means resilience configuration lives in options (configurable via `appsettings.json`), is validated at startup, and emits metrics to `System.Diagnostics.Metrics` automatically — three concerns that would require manual setup when using raw Polly.

---

## Q28. What does `AddStandardResilienceHandler()` configure by default, and what can you customize?

What does `AddStandardResilienceHandler()` configure by default, and what can you customize?

**Answer:** `AddStandardResilienceHandler()` registers a resilience pipeline that includes, in outer-to-inner order: total request timeout (default 30 s), retry with exponential backoff and jitter (default 3 retries), circuit breaker (default 10 % failure rate over 30 s window, 30 s break duration), per-attempt timeout (default 10 s). These defaults follow Microsoft's guidance for typical HTTP microservice calls and are conservative enough to be safe to enable without tuning.

- All defaults are exposed through strongly typed options — `HttpStandardResilienceOptions` — that can be overridden in `appsettings.json` or via the options pattern in code, so teams do not have to rewrite the entire pipeline to change a single value.
- The retry predicate defaults to retrying on `HttpRequestException` and HTTP status codes 408, 429, and 500–599, which covers the most common transient HTTP failures.
- You can customize individual layer options through the overload: `.AddStandardResilienceHandler(options => { options.Retry.MaxRetryAttempts = 5; options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(60); })`.

---

## Q29. How do you apply per-named-client resilience options using `AddResilienceHandler()`?

How do you apply per-named-client resilience options using `AddResilienceHandler()`?

**Answer:** `AddResilienceHandler(name, configure)` lets you attach a fully custom resilience pipeline to a named `HttpClient`, where the `configure` callback receives a `ResiliencePipelineBuilder<HttpResponseMessage>` and you add strategies manually. This is appropriate when the default pipeline's structure does not match the target service's SLA, or when you need a strategy the standard handler does not include.

```csharp
builder.Services.AddHttpClient("InventoryClient")
    .AddResilienceHandler("inventory-pipeline", pipelineBuilder =>
    {
        pipelineBuilder
            .AddTimeout(TimeSpan.FromSeconds(5))
            .AddRetry(new HttpRetryStrategyOptions { MaxRetryAttempts = 2 })
            .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions());
    });
```

- The pipeline name (first argument) is used for metrics labeling and diagnostics, so using a meaningful name helps trace which pipeline is affecting a given request in observability dashboards.
- Multiple named clients can share the same pipeline definition by extracting it into a reusable `ResiliencePipeline<HttpResponseMessage>` registered with `AddResiliencePipeline()` and referenced by name.
- Unlike `AddStandardResilienceHandler()`, `AddResilienceHandler()` does not apply any defaults — every strategy you need must be added explicitly.

---

## Chapter 8 — Health Checks & Self-Healing

---

## Q30. What are ASP.NET Core health checks, and how do they contribute to overall system resilience?

What are ASP.NET Core health checks, and how do they contribute to overall system resilience?

**Answer:** ASP.NET Core health checks are lightweight HTTP endpoints (typically `/health`, `/healthz/ready`, `/healthz/live`) that report whether the application and its dependencies are operating correctly. They contribute to resilience by giving orchestration platforms (Kubernetes, load balancers, service meshes) the signal they need to remove an unhealthy instance from the load-balancing rotation before it serves failed requests to users.

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddUrlGroup(new Uri("https://api.dependency.com/health"), "dependency");

app.MapHealthChecks("/healthz/ready");
```

- Health checks are a pull-based mechanism: the orchestrator periodically polls the endpoint rather than the service pushing a status update, which is simpler and survives network partitions gracefully.
- The `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` and `AspNetCore.HealthChecks.*` community packages provide ready-made checks for SQL Server, Redis, RabbitMQ, and many other dependencies so teams do not have to write probe logic from scratch.
- Health check results are categorized as Healthy, Degraded, or Unhealthy; returning Degraded allows the orchestrator to log a warning while keeping the instance in rotation, which is useful for non-critical dependency failures.

---

## Q31. How do Kubernetes liveness and readiness probes differ, and how do you configure an ASP.NET Core service to serve both?

How do Kubernetes liveness and readiness probes differ, and how do you configure an ASP.NET Core service to serve both?

**Answer:** A readiness probe tells Kubernetes whether the pod is ready to accept traffic — failing readiness removes the pod from the service's endpoints without restarting it, which is appropriate for temporary conditions like a downstream database being unavailable. A liveness probe tells Kubernetes whether the pod is still alive and functioning — failing liveness causes Kubernetes to restart the pod, which is appropriate for unrecoverable states like deadlocks or out-of-memory conditions.

- A common mistake is using the same health check for both probes: a pod that cannot reach its database should fail readiness (stop receiving traffic) but should not fail liveness (Kubernetes would restart it in a loop even though the database is the problem, not the pod).
- In ASP.NET Core, liveness checks should verify that the process is responsive (a trivial always-healthy check or a thread-deadlock detector), while readiness checks verify that dependencies like databases, caches, and message brokers are reachable.
- Mapping separate endpoints is the recommended pattern: `app.MapHealthChecks("/healthz/live", liveOptions)` and `app.MapHealthChecks("/healthz/ready", readyOptions)` using tag-based filtering to include only the appropriate checks in each endpoint.

---

## Q32. What role does a service mesh (such as Istio) or a load balancer play in resilience, and how does that differ from application-layer resilience?

What role does a service mesh (such as Istio) or a load balancer play in resilience, and how does that differ from application-layer resilience?

**Answer:** A service mesh or smart load balancer provides infrastructure-layer resilience — retry, timeout, circuit breaking, and load balancing — transparently to all services in the mesh without any application code changes. Application-layer resilience (Polly, Microsoft.Extensions.Http.Resilience) is implemented within the service process and can be aware of business logic, response content, and request context in ways that the infrastructure layer cannot.

- Infrastructure-layer retry is useful for basic transport-level transient faults (TCP resets, brief DNS resolution failures) and requires no deployment of new application code, but it cannot distinguish between a retryable application-level 500 and a non-retryable business-logic 500 — both look the same to the sidecar proxy.
- Application-layer resilience can inspect the response body, log structured correlation data, and decide whether a 500 is retryable based on error codes in the JSON payload — nuance that a generic mesh proxy cannot perform.
- The two layers are complementary: infrastructure retry handles transport-level failures and eliminates the need for redundant retry logic in every service, while application-layer circuit breaking, fallbacks, and hedging handle the richer failure semantics that only the application understands.

---
