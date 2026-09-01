# Resilience & Circuit Breaker — Interview Q&A
> 32 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is resilience in the context of microservices, and why does it matter more than in a monolithic application?](#q1-what-is-resilience-in-the-context-of-microservices-and-why-does-it-matter-more-than-in-a-monolithic-application)
2. [Q2. What is a transient fault, and how does it differ from a permanent failure?](#q2-what-is-a-transient-fault-and-how-does-it-differ-from-a-permanent-failure)
3. [Q3. What is a cascade failure, and how do resilience patterns prevent it from propagating?](#q3-what-is-a-cascade-failure-and-how-do-resilience-patterns-prevent-it-from-propagating)
4. [Q4. What is the difference between fault tolerance (preventing impact) and fault recovery (restoring state)?](#q4-what-is-the-difference-between-fault-tolerance-preventing-impact-and-fault-recovery-restoring-state)
5. [Q5. What is the Circuit Breaker pattern, and what problem does it solve in service-to-service calls?](#q5-what-is-the-circuit-breaker-pattern-and-what-problem-does-it-solve-in-service-to-service-calls)
6. [Q6. Describe the three states of a Circuit Breaker — Closed, Open, and Half-Open — and the transitions between them.](#q6-describe-the-three-states-of-a-circuit-breaker-closed-open-and-half-open-and-the-transitions-between-them)
7. [Q7. What metrics does a Circuit Breaker use to decide when to trip open — failure count, failure rate, or something else?](#q7-what-metrics-does-a-circuit-breaker-use-to-decide-when-to-trip-open-failure-count-failure-rate-or-something-else)
8. [Q8. What is a sampling window (sliding window), and how does it affect circuit breaker sensitivity?](#q8-what-is-a-sampling-window-sliding-window-and-how-does-it-affect-circuit-breaker-sensitivity)
9. [Q9. What is the difference between a count-based and a time-based sliding window in a circuit breaker?](#q9-what-is-the-difference-between-a-count-based-and-a-time-based-sliding-window-in-a-circuit-breaker)
10. [Q10. What happens to requests when the circuit is Open, and what is a fallback response?](#q10-what-happens-to-requests-when-the-circuit-is-open-and-what-is-a-fallback-response)
11. [Q11. When is a retry safe to apply, and when can it make things worse?](#q11-when-is-a-retry-safe-to-apply-and-when-can-it-make-things-worse)
12. [Q12. What is exponential backoff, and why is it preferred over fixed-interval retries?](#q12-what-is-exponential-backoff-and-why-is-it-preferred-over-fixed-interval-retries)
13. [Q13. What is jitter in a retry strategy, and why does it help at scale?](#q13-what-is-jitter-in-a-retry-strategy-and-why-does-it-help-at-scale)
14. [Q14. What is the difference between a timeout and a cancellation token when making HTTP calls?](#q14-what-is-the-difference-between-a-timeout-and-a-cancellation-token-when-making-http-calls)
15. [Q15. How do you decide the right timeout value for a downstream service call?](#q15-how-do-you-decide-the-right-timeout-value-for-a-downstream-service-call)
16. [Q16. What is the Bulkhead pattern, and what problem does it solve in microservices?](#q16-what-is-the-bulkhead-pattern-and-what-problem-does-it-solve-in-microservices)
17. [Q17. What are the two main types of bulkhead isolation — thread-pool isolation and semaphore isolation — and how do they differ?](#q17-what-are-the-two-main-types-of-bulkhead-isolation-thread-pool-isolation-and-semaphore-isolation-and-how-do-they-differ)
18. [Q18. What is rate limiting, and how does it differ from throttling a downstream dependency?](#q18-what-is-rate-limiting-and-how-does-it-differ-from-throttling-a-downstream-dependency)
19. [Q19. How does the sliding-window rate limiter differ from the fixed-window and token-bucket algorithms?](#q19-how-does-the-sliding-window-rate-limiter-differ-from-the-fixed-window-and-token-bucket-algorithms)
20. [Q20. What is the Hedging strategy, and when is it appropriate to use it?](#q20-what-is-the-hedging-strategy-and-when-is-it-appropriate-to-use-it)
21. [Q21. What is a fallback policy, and how is it different from simply returning an error response?](#q21-what-is-a-fallback-policy-and-how-is-it-different-from-simply-returning-an-error-response)
22. [Q22. What is Polly, and what resilience policies does it provide?](#q22-what-is-polly-and-what-resilience-policies-does-it-provide)
23. [Q23. How do you compose multiple Polly policies together — what is a PolicyWrap or ResiliencePipeline?](#q23-how-do-you-compose-multiple-polly-policies-together-what-is-a-policywrap-or-resiliencepipeline)
24. [Q24. What is the difference between a reactive policy (fault handling) and a proactive policy (fault prevention) in Polly?](#q24-what-is-the-difference-between-a-reactive-policy-fault-handling-and-a-proactive-policy-fault-prevention-in-polly)
25. [Q25. How does Polly v8's ResiliencePipeline differ from the v7 Policy API?](#q25-how-does-polly-v8s-resiliencepipeline-differ-from-the-v7-policy-api)
26. [Q26. How do you register Polly resilience with IHttpClientFactory in .NET so it applies to all outgoing HTTP calls?](#q26-how-do-you-register-polly-resilience-with-ihttpclientfactory-in-net-so-it-applies-to-all-outgoing-http-calls)
27. [Q27. What is Microsoft.Extensions.Http.Resilience, and how does it differ from using Polly directly?](#q27-what-is-microsoftextensionshttpresilience-and-how-does-it-differ-from-using-polly-directly)
28. [Q28. What does `AddStandardResilienceHandler()` configure by default, and what can you customize?](#q28-what-does-addstandardresiliencehandler-configure-by-default-and-what-can-you-customize)
29. [Q29. How do you apply per-named-client resilience options using `AddResilienceHandler()`?](#q29-how-do-you-apply-per-named-client-resilience-options-using-addresiliencehandler)
30. [Q30. What are ASP.NET Core health checks, and how do they contribute to overall system resilience?](#q30-what-are-aspnet-core-health-checks-and-how-do-they-contribute-to-overall-system-resilience)
31. [Q31. How do Kubernetes liveness and readiness probes differ, and how do you configure an ASP.NET Core service to serve both?](#q31-how-do-kubernetes-liveness-and-readiness-probes-differ-and-how-do-you-configure-an-aspnet-core-service-to-serve-both)
32. [Q32. What role does a service mesh (such as Istio) or a load balancer play in resilience, and how does that differ from application-layer resilience?](#q32-what-role-does-a-service-mesh-such-as-istio-or-a-load-balancer-play-in-resilience-and-how-does-that-differ-from-application-layer-resilience)

---

## Q1. What is resilience in the context of microservices, and why does it matter more than in a monolithic application?

**Concepts**
- Resilience — continuing to operate correctly when one or more components fail
- Partial failure as the norm in distributed systems, not the exception
- Thread pool exhaustion from a slow dependency as the cascade trigger
- Combined availability math — many services each at 99.9% yields a low overall path availability

**Answer**

Resilience is a system's ability to continue operating correctly — possibly in a degraded mode — when one or more of its components fail. In a microservices architecture, a single user request typically crosses several network boundaries and calls multiple services, so the probability that at least one dependency is degraded at any moment is meaningfully higher than in a monolith where all code runs in the same process. A monolith either works or crashes as a unit, whereas in microservices a slow or failing downstream service can silently stall threads or exhaust connection pools in the caller, causing what appears to be unrelated failures upstream. Resilience engineering acknowledges that distributed systems fail partially — individual nodes, databases, and network paths fail independently — so the design goal is graceful degradation rather than binary availability. The cost of neglecting resilience is amplified by the number of inter-service calls: a fleet of fifty services each with 99.9% availability yields a combined path availability well below 95% without explicit resilience measures.

---

## Q2. What is a transient fault, and how does it differ from a permanent failure?

**Concepts**
- Transient fault — temporary error likely to resolve on its own within seconds
- Permanent failure — error that will not resolve by retrying
- HTTP status code classification — 429 and 503 transient, 400 and 401 permanent
- Polly default predicates classifying common transient HTTP status codes

**Answer**

A transient fault is a temporary error condition that is likely to resolve on its own within milliseconds to seconds, such as a momentary network blip, a connection timeout during a TCP handshake, or a brief HTTP 503 from a service that is restarting. A permanent failure — like a misconfigured URL, an HTTP 404 for a resource that does not exist, or an authentication error — will not resolve by retrying. Transient faults are the primary target of retry and circuit-breaker policies; retrying a permanent failure wastes resources and delays the caller's error response without any benefit. Distinguishing them in code requires inspecting the exception type or HTTP status code — HTTP 429 (Too Many Requests) and 503 are generally transient, while 400 (Bad Request) and 401 (Unauthorized) are not. Polly and `Microsoft.Extensions.Http.Resilience` both ship with default predicates that classify common HTTP status codes as transient, and these can be extended for service-specific error contracts.

---

## Q3. What is a cascade failure, and how do resilience patterns prevent it from propagating?

**Concepts**
- Cascade failure — slow downstream service exhausting upstream thread pools
- Circuit Breaker fast-failing calls to a known-failing service
- Bulkhead isolation bounding blast radius per downstream dependency
- Timeout releasing blocked threads before circuit breaker opens

**Answer**

A cascade failure occurs when the failure or slow-down of one service causes the services that depend on it to also fail, which in turn causes their callers to fail, propagating the outage upward through the call graph until a large portion of the system is unavailable. The classic trigger is a slow downstream service that holds threads or connections in the caller, eventually exhausting the caller's thread pool and rendering it unable to serve any request — including those that do not touch the faulty dependency. The Circuit Breaker pattern stops cascade failures by refusing to forward calls to a known-failing service rather than waiting for each one to time out, freeing threads immediately. Bulkhead isolation limits the blast radius by giving each downstream dependency its own bounded pool of threads or semaphore slots, so saturation against one service cannot consume resources meant for another. Timeout policies ensure that waiting threads are released even when the circuit breaker has not yet opened, bounding the time any single dependency can hold a resource.

---

## Q4. What is the difference between fault tolerance (preventing impact) and fault recovery (restoring state)?

**Concepts**
- Fault tolerance — absorbing a fault without propagating it to callers
- Fault recovery — detecting and restoring the system to full operation after the fault clears
- Circuit breaker, bulkhead, and fallback as fault-tolerance mechanisms
- Health checks, readiness probes, and Half-Open state as fault-recovery mechanisms

**Answer**

Fault tolerance means designing the system so that a fault in one component does not propagate to cause visible degradation in others — the system keeps working, possibly with reduced functionality, while the fault is present. Fault recovery is the complementary process of detecting the fault and restoring the system to its fully operational state after the fault clears. Circuit breakers, bulkheads, and fallbacks are fault-tolerance mechanisms — they absorb the impact of a failure without necessarily fixing the underlying cause. Health checks, readiness probes, self-healing restart policies in Kubernetes, and the Half-Open state of a circuit breaker are fault-recovery mechanisms — they detect that a service has recovered and restore it to normal traffic. In practice both must be designed together: tolerance limits the damage while recovery determines how quickly normal operation resumes.

---

## Chapter 2 — Circuit Breaker Pattern

---

## Q5. What is the Circuit Breaker pattern, and what problem does it solve in service-to-service calls?

**Concepts**
- Circuit Breaker — monitoring call failures and fast-failing once a threshold is crossed
- Thread pool exhaustion from accumulated timeout waits without a breaker
- Break duration giving the downstream service recovery time without incoming traffic
- Half-Open probe volume testing recovery without overwhelming a fragile service

**Answer**

The Circuit Breaker pattern wraps calls to a remote service and monitors the failure rate of those calls; when failures exceed a configured threshold, the circuit "trips open" and subsequent calls are immediately rejected without being forwarded to the remote service. This prevents a failing or slow downstream service from tying up threads in the caller through timeout waits and gives the downstream service time to recover without being bombarded by traffic. Without a circuit breaker, a service calling a dependency that takes 30 seconds to time out will accumulate new requests faster than old ones complete, eventually exhausting its thread pool and becoming unresponsive itself. The pattern is named after an electrical circuit breaker: just as a household breaker interrupts current to protect wiring from overload, the software circuit breaker interrupts requests to protect the calling service from resource exhaustion. After a configurable break duration, the circuit moves to Half-Open and allows a small probe volume through to test whether the downstream has recovered.

---

## Q6. Describe the three states of a Circuit Breaker — Closed, Open, and Half-Open — and the transitions between them.

**Concepts**
- Closed state — all calls forwarded, failures counted toward the threshold
- Open state — all calls rejected immediately without contacting the dependency
- Half-Open state — limited probe calls forwarded to test recovery
- Break duration as the recovery window for the downstream service

**Answer**

A circuit breaker operates as a state machine with three states that control whether calls are forwarded to the dependency. In the Closed state all calls pass through and failures are counted; when the failure metric crosses the threshold the circuit transitions to Open. In the Open state all calls are rejected immediately without contacting the dependency, freeing threads instantly rather than waiting for timeouts to expire. After a configured break duration the circuit transitions to Half-Open, where a limited number of probe calls are allowed through; if they succeed the circuit returns to Closed, and if they fail it returns to Open. The break duration in the Open state gives the downstream service breathing room to restart or recover without being flooded, which is why the break duration should be set long enough to be meaningful — too short and the circuit never gives the dependency enough time to stabilize. The Half-Open probe volume is intentionally small — typically one to five requests — to avoid overwhelming a service that may still be fragile.

---

## Q7. What metrics does a Circuit Breaker use to decide when to trip open — failure count, failure rate, or something else?

**Concepts**
- Failure rate within a sliding window rather than raw failure count
- Minimum call count requirement preventing cold-start false trips
- Slow-call rate threshold for detecting degradation before full failures appear
- Rate-based threshold as environment-agnostic across varying traffic volumes

**Answer**

Modern circuit breaker implementations evaluate the failure rate — the percentage of failed calls within a sliding window — rather than a raw failure count, because a raw count can be misleading: five failures out of five calls is very different from five failures out of five thousand. In Polly v8, the circuit breaker opens when the failure ratio such as 50% is exceeded within a minimum number of calls that must be observed in the sampling window — both thresholds must be met simultaneously to avoid tripping on a cold start with a single failure. Some implementations also consider slow calls — calls that complete but exceed a latency threshold — as a separate metric that can contribute to tripping the circuit, allowing it to open proactively when a dependency is degraded but not fully failing. Using rate rather than count makes the threshold environment-agnostic: the same policy works correctly whether the service is processing ten or ten thousand requests per second.

---

## Q8. What is a sampling window (sliding window), and how does it affect circuit breaker sensitivity?

**Concepts**
- Sampling window defining the scope for failure metric aggregation
- Narrow window — responsive to sudden bursts but prone to false trips
- Wide window — stable but slow to detect a real outage beginning
- Balancing sensitivity against specificity in window size selection

**Answer**

A sampling window defines the scope over which the circuit breaker aggregates failure metrics before deciding whether to trip. The window moves forward in time as requests arrive, so only recent calls contribute to the current failure rate rather than the full lifetime of the service. Without a window, a single failure at startup would permanently influence the circuit breaker's decision, which is not useful because historical failures are rarely predictive of current health. A narrower window makes the circuit breaker more responsive to sudden bursts of failures but also more prone to tripping on statistical noise from a small sample. A wide window such as 120 seconds smooths out brief spikes but may delay tripping when a real outage begins, causing more cascading damage before the circuit opens. Choosing window size requires balancing sensitivity — catching real outages quickly — against specificity — avoiding false trips on brief transient spikes.

---

## Q9. What is the difference between a count-based and a time-based sliding window in a circuit breaker?

**Concepts**
- Count-based window — fixed N most recent requests regardless of time
- Time-based window — all requests within the last T seconds, variable sample size
- Stale data risk in count-based window under low traffic
- Time-based window naturally expiring old samples under variable load

**Answer**

A count-based sliding window tracks the most recent N requests regardless of how long they took to arrive, so the window always contains exactly N samples. A time-based sliding window tracks all requests that arrived within the last T seconds, so the sample size varies with throughput. A count-based window can leave stale data in the window for a long time if request rate drops, meaning a past failure continues to count long after the dependency recovered — if only one request per minute arrives, the last hundred requests span over an hour. A time-based window naturally expires old samples, making it a better fit for services with variable or bursty load since the window content always reflects recent behavior regardless of how many requests came in. For a consistently high-traffic service, a count-based window reflects very recent behavior accurately since new requests replace old ones quickly. Polly v8 supports both types, and the right choice depends on the traffic pattern of the service being protected.

---

## Q10. What happens to requests when the circuit is Open, and what is a fallback response?

**Concepts**
- Fast-fail in Open state — `BrokenCircuitException` thrown in microseconds
- Fallback response as a predetermined safe substitute for the real response
- Thread pool protection as the reason fast-failing while Open matters
- Fallback registered separately in Polly, wrapping the circuit breaker

**Answer**

When the circuit is Open, the circuit breaker immediately throws an exception — typically `BrokenCircuitException` in Polly — or invokes a configured fallback without contacting the remote service at all, so the caller receives a response in microseconds rather than waiting for a timeout. Fast-failing while Open is crucial: it prevents threads from accumulating waiting for timeout durations, which is what leads to thread pool exhaustion and cascade failures. A fallback response is a predetermined safe answer returned in place of the real response — it might be a cached value, an empty collection, a default object, or a graceful degradation message that allows the user to continue. A fallback should be semantically meaningful — for example, a product recommendations service might return an empty list rather than crashing the checkout page, allowing the user to proceed without personalized suggestions. In Polly, fallbacks are registered separately from the circuit breaker and placed as the outermost layer of a `ResiliencePipeline` so that the broken-circuit exception is caught and the fallback value returned.

---

## Chapter 3 — Retry & Timeout Strategies

---

## Q11. When is a retry safe to apply, and when can it make things worse?

**Concepts**
- Idempotency as the prerequisite for safe retries
- Transient fault classification as the target for retry
- Retrying non-idempotent writes causing duplicate side effects
- Retrying during overload worsening the downstream situation

**Answer**

A retry is safe when the operation is idempotent — repeating it produces the same result as performing it once — and when the failure is classified as transient. Retrying a non-idempotent operation such as placing an order or charging a payment can cause duplicate side effects, while retrying a permanent failure such as a 400 Bad Request due to invalid input wastes resources without any chance of success. HTTP GET requests are generally idempotent and safe to retry; POST requests that mutate state are not, unless the server supports idempotency-key semantics. Retrying during an overload condition such as HTTP 429 or 503 can worsen the situation at the downstream service by increasing its incoming load exactly when it cannot handle more — which is why retry should always be paired with backoff. For write operations where retry is needed, the recommended approach is to make the write idempotent via an idempotency key so that the same request sent twice has the same net effect as sending it once.

---

## Q12. What is exponential backoff, and why is it preferred over fixed-interval retries?

**Concepts**
- Exponential backoff — wait time doubling between successive retry attempts
- Thundering herd from synchronized fixed-interval retries
- Maximum delay cap preventing unbounded wait times
- Increasing recovery time for downstream service between attempts

**Answer**

Exponential backoff is a retry delay strategy where the wait time between successive attempts grows exponentially — for example, one second, two seconds, four seconds, eight seconds — rather than being a fixed interval. Fixed-interval retries can create a synchronized thundering herd: if thousands of clients all fail at the same time and retry every five seconds, they all hit the recovering service at the same moments and may overwhelm it again before it can stabilize. Exponential backoff disperses retry attempts over a longer window of time, giving the downstream service progressively more time to recover between attempts, which is especially important when the cause of failure is temporary overload rather than a crash. A maximum delay cap is typically applied so that backoff does not grow so large that the caller waits forever — after the cap, subsequent retries use the capped delay rather than continuing to grow.

---

## Q13. What is jitter in a retry strategy, and why does it help at scale?

**Concepts**
- Jitter — random amount added to each retry delay
- Correlated retry spikes from synchronized failures despite exponential backoff
- Full jitter spreading load uniformly across the retry window
- AWS research validating full jitter as the best distribution strategy at scale

**Answer**

Jitter is a small random amount of time added to each retry delay so that different clients retrying at the same time do not produce synchronized spikes in load against the dependency. Even with exponential backoff, if all clients start a burst simultaneously they will all retry at intervals that are exponentially offset from the same origin, causing correlated spikes. Without jitter, a thousand clients that all fail at second zero and retry with backoff of one, two, four seconds will all produce load spikes at roughly second one, second two, and second four simultaneously. With full jitter, each client independently picks a random delay between zero and the backoff cap, so load is spread roughly uniformly across the entire retry window. AWS published influential research on jitter strategies — full jitter, equal jitter, decorrelated jitter — showing that full jitter produces the best load distribution at scale; Polly's default retry uses a similar approach.

---

## Q14. What is the difference between a timeout and a cancellation token when making HTTP calls?

**Concepts**
- Timeout — policy-level construct abandoning the operation after N seconds
- CancellationToken — .NET mechanism signaling an in-flight operation to stop
- `HttpClient.Timeout` creating an internal cancellation token on the configured duration
- Polly timeout recording timeout as a metric feeding the circuit breaker

**Answer**

A timeout is a policy-level construct that says "if this operation has not completed within N seconds, abandon it and treat it as a failure." A `CancellationToken` is a .NET mechanism that signals an in-flight operation to stop what it is doing; a timeout policy typically works by cancelling the operation's cancellation token after the configured duration, but cancellation tokens can also be triggered by other sources such as the user closing the browser or the server shutting down. `HttpClient` has a `Timeout` property that creates an internal `CancellationToken` that fires after the configured duration; Polly's timeout strategy wraps the call with its own cancellation token that fires independently. Using `CancellationToken` directly gives callers fine-grained control: ASP.NET Core automatically passes the request's cancellation token to action methods so that HTTP calls can be cancelled when the HTTP client disconnects. Polly's timeout strategy differs from `HttpClient.Timeout` in that it integrates with the resilience pipeline and records the timeout as a metric that feeds the circuit breaker, whereas `HttpClient.Timeout` fires independently of any Polly configuration.

---

## Q15. How do you decide the right timeout value for a downstream service call?

**Concepts**
- P95/P99 latency of successful calls as the baseline for timeout calculation
- 2–3× the P99 as a common rule of thumb
- End-to-end timeout budget propagation across a multi-hop call chain
- Reviewing timeouts when downstream SLAs or code paths change

**Answer**

The right timeout is derived from the service's actual performance data — specifically the high-percentile latency (P95 or P99) of successful calls under normal load — with a small safety margin added. Setting the timeout at the mean latency would cancel a significant portion of legitimate requests; setting it too high defeats the purpose of bounding wait time. A common rule of thumb is to set the timeout at two to three times the P99 latency measured in production under normal conditions, so that genuine slowdowns are distinguished from normal variance. End-to-end timeout budgets must be considered across the call chain: if the overall request timeout for the frontend is five seconds and the call chain passes through three services, each service's timeout must be set to leave time for subsequent hops — for example, 1.5 seconds each rather than five seconds each. Timeouts should be reviewed when the downstream service changes its SLA or when profiling reveals that a new code path is slower than historical baselines.

---

## Chapter 4 — Bulkhead & Rate Limiting

---

## Q16. What is the Bulkhead pattern, and what problem does it solve in microservices?

**Concepts**
- Bulkhead — bounded resource allocation per downstream dependency
- Thread pool saturation from one slow dependency starving others
- Blast radius limitation — failing one dependency cannot consume all resources
- Rejection vs. queuing for time-sensitive requests at bulkhead limit

**Answer**

The Bulkhead pattern limits the number of concurrent requests a service sends to a specific dependency, preventing a slow or failing dependency from consuming all of the caller's resources — threads, connections, or semaphore slots — and degrading calls to completely unrelated dependencies. The name comes from the watertight compartments in a ship's hull that prevent a breach in one section from flooding the whole vessel. Without bulkheads, a service that calls three dependencies A, B, and C over a shared thread pool can have that entire pool saturated by slow calls to dependency A, making the service unable to process requests that only touch B or C. Bulkheads enforce isolation by giving each dependency its own bounded resource allocation — for example, at most ten concurrent in-flight calls to service A and at most twenty to service B. When the bulkhead limit is reached, additional calls are either rejected immediately or queued up to a secondary limit; rejection is preferable for time-sensitive requests because queuing may just defer the failure.

---

## Q17. What are the two main types of bulkhead isolation — thread-pool isolation and semaphore isolation — and how do they differ?

**Concepts**
- Thread-pool isolation — dedicated thread pool per dependency, true resource separation
- Semaphore isolation — shared threads with a counting semaphore per dependency
- Async .NET code favoring semaphore isolation over thread-pool isolation
- Polly v8 implementing bulkhead via the `RateLimiter` abstraction

**Answer**

Thread-pool isolation assigns a dedicated pool of threads to each dependency, so calls to that dependency execute only on those threads and cannot borrow threads from other pools. Semaphore isolation uses a shared thread pool but limits the number of concurrent operations via a counting semaphore, rejecting new requests when the semaphore count is exhausted. Thread-pool isolation provides true resource separation — saturating the pool for one dependency is physically impossible to affect another — but carries higher overhead through context switches and the memory cost of additional thread stacks. Semaphore isolation is more lightweight since a semaphore is just a counter, but threads are still shared so saturation is partial rather than total. In .NET async code, the traditional Hystrix-style thread-pool isolation is less natural because async operations do not block a thread while awaiting, which means the concept of a "dedicated thread" does not map cleanly to async continuations — semaphore isolation with `SemaphoreSlim` or Polly's `RateLimiter`-backed bulkhead is the more idiomatic choice. Polly v8 implements bulkhead isolation via the `RateLimiter` abstraction, which supports both concurrency limits and queue depths.

---

## Q18. What is rate limiting, and how does it differ from throttling a downstream dependency?

**Concepts**
- Rate limiting — controlling inbound request throughput to protect the service itself
- Outbound throttling — limiting calls the service makes to a downstream dependency
- ASP.NET Core 7+ built-in rate limiting middleware for inbound protection
- Both inbound rate limiting and outbound bulkheading needed in a complete system

**Answer**

Rate limiting controls how many requests a service accepts from its callers — inbound — typically to protect itself from being overwhelmed. Throttling in the resilience context refers to slowing down or limiting outbound calls the service makes to a downstream dependency, which is effectively the Bulkhead pattern applied to throughput rather than concurrency. The two terms are sometimes used interchangeably, but the direction matters: rate limiting is self-defense — protecting yourself from callers — while throttling or bulkheading is courtesy — protecting a dependency from you. ASP.NET Core 7+ ships with built-in rate limiting middleware via `Microsoft.AspNetCore.RateLimiting` that enforces inbound rate limits at the HTTP pipeline level before controllers run. Rate limiting a downstream HTTP call is better implemented with Polly's `RateLimiter` strategy, which rejects or queues requests before they leave the process. Both are needed in a complete system: inbound rate limiting protects your service, and outbound concurrency or rate limits protect your dependencies.

---

## Q19. How does the sliding-window rate limiter differ from the fixed-window and token-bucket algorithms?

**Concepts**
- Fixed-window — counter resets at boundary, allowing burst of 2× limit at the seam
- Sliding-window — rolling measurement eliminating boundary bursts
- Token bucket — accumulated tokens enabling controlled bursts
- ASP.NET Core supporting all four: fixed, sliding, token bucket, concurrency

**Answer**

A fixed-window rate limiter allows a fixed number of requests per discrete time window such as 100 per minute, but resets its counter at the window boundary, which can allow a burst of 200 requests if 100 arrive at the end of one window and 100 more arrive at the start of the next. A sliding-window rate limiter tracks requests continuously so the count is always measured over the most recent window, eliminating the boundary burst since a new request is only allowed if the count of requests in the preceding full window plus the current partial window is below the limit. A token-bucket algorithm allows short bursts by accumulating tokens when the service is under-utilized and spending them during bursts up to a bucket capacity, which is useful for APIs where legitimate bursty traffic is expected such as batch uploads. A concurrency limiter simply caps parallel in-flight requests rather than rate, which makes it more of a bulkhead than a rate limiter. ASP.NET Core's `RateLimiterOptions` supports all four: `FixedWindowRateLimiter`, `SlidingWindowRateLimiter`, `TokenBucketRateLimiter`, and `ConcurrencyLimiter`. The right choice depends on whether you need to prevent bursty traffic — sliding window or concurrency — allow controlled bursts — token bucket — or keep implementation simple — fixed window.

---

## Chapter 5 — Hedging & Fallback

---

## Q20. What is the Hedging strategy, and when is it appropriate to use it?

**Concepts**
- Hedging — concurrent parallel requests accepting the first successful response
- Retry vs. hedging — hedging overlaps attempts proactively rather than waiting for failure
- Tail latency reduction as the use case
- Idempotency as the prerequisite for safe hedging

**Answer**

The Hedging strategy issues a secondary — and optionally further — parallel request to the same endpoint if the first request has not returned within a configured delay, accepting whichever response arrives first and cancelling the slower copy. Unlike retry, which waits for the first attempt to fail before sending the next attempt, hedging runs attempts concurrently to reduce tail latency — the extreme cases where the P99 latency is much higher than the median. Hedging is appropriate when a service has high variance in response time and latency is more important than minimizing load on the downstream, because hedging by definition sends more requests than retrying does. It is safe only when the downstream operation is idempotent: if both copies of the request succeed before the second is cancelled, the downstream must not process a duplicate side effect. Polly v8 introduced the `HedgingResilienceStrategy`, configurable with a hedging delay and a maximum hedge count; the strategy cancels the trailing requests as soon as the first successful response arrives.

---

## Q21. What is a fallback policy, and how is it different from simply returning an error response?

**Concepts**
- Fallback policy — substituting a safe predetermined response for a specific failure
- Graceful degradation — allowing users to continue with reduced functionality
- `FallbackResilienceStrategy<T>` with `ShouldHandle` predicate for selective activation
- Risk of transparent fallback masking data absence from downstream logic

**Answer**

A fallback policy intercepts a specific failure condition and substitutes a predetermined safe value or alternative action, so the caller receives a usable — if degraded — response rather than an exception or HTTP error. Returning an error response means propagating the failure upstream as an HTTP 500 or an exception; a fallback instead hides the failure from the caller by substituting something meaningful. A fallback is valuable when the downstream data is optional or can be approximated — for example, returning a cached version of a user's recommendation list rather than failing the entire page load. Fallbacks should be designed carefully: a fallback that returns an empty list silently may confuse users or downstream logic that expects data, so some scenarios benefit from a partial-failure indicator rather than a completely transparent substitution. In Polly v8, the `FallbackResilienceStrategy<T>` is configured with a `FallbackAction<T>` delegate and an optional `ShouldHandle` predicate so it only activates for specific exception types or result conditions rather than for every failure.

---

## Chapter 6 — Polly in .NET

---

## Q22. What is Polly, and what resilience policies does it provide?

**Concepts**
- Polly as the de facto .NET resilience and transient-fault-handling library
- Polly v7 — Retry, Circuit Breaker, Timeout, Bulkhead, Fallback, Cache, PolicyWrap
- Polly v8 — ResiliencePipeline replacing IAsyncPolicy, adding Hedging and RateLimiter
- Stateless reusable policies sharing circuit breaker state across all callers

**Answer**

Polly is an open-source .NET resilience and transient-fault-handling library that wraps calls in configurable policies (v7) or strategies (v8) to handle failures consistently across an application. It is the de facto standard resilience library for .NET and is integrated into `IHttpClientFactory` through `Microsoft.Extensions.Http.Polly` (v7) and `Microsoft.Extensions.Http.Resilience` (v8+). Polly v7 provides Retry, Circuit Breaker, Timeout, Bulkhead Isolation, Fallback, Cache, and PolicyWrap for composition. Polly v8 redesigned the API around `ResiliencePipeline` and individual strategies: Retry, Circuit Breaker, Timeout, Rate Limiter replacing Bulkhead, Fallback, and the new Hedging strategy — all built on `System.Threading.RateLimiting` and modern async patterns. A key feature of Polly is that policies are stateless and reusable: a single circuit breaker instance maintains state and tracks failure rate across all callers that share it, so the circuit can trip based on the aggregate failure pattern rather than per-caller.

---

## Q23. How do you compose multiple Polly policies together — what is a PolicyWrap or ResiliencePipeline?

**Concepts**
- Polly v7 `Policy.WrapAsync` composing outer-to-inner
- Polly v8 `ResiliencePipelineBuilder.AddX()` in declaration order
- Timeout innermost to bound each individual attempt, not the entire retry sequence
- Fallback outermost to catch all failures including `BrokenCircuitException`

**Answer**

In Polly v7, multiple policies are combined with `Policy.WrapAsync(outer, inner, ...)`, where the outermost policy in the list is the first to execute and the innermost surrounds the actual operation. In Polly v8, a `ResiliencePipelineBuilder` composes strategies in declaration order, where the first `AddX()` call corresponds to the outermost layer.

```csharp
// Polly v8 — strategies execute in declaration order (outer to inner)
var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddFallback(...)       // outermost — catches all failures
    .AddCircuitBreaker(...) // middle — trips on repeated failures
    .AddRetry(...)          // inner — retries transient faults
    .AddTimeout(...)        // innermost — bounds each individual attempt
    .Build();
```

Order matters significantly: Timeout must be the innermost strategy so it bounds each individual retry attempt rather than the entire multi-retry operation. Fallback should be outermost so it catches exceptions from all inner strategies including the `BrokenCircuitException` thrown when the circuit is Open. Circuit Breaker should sit inner of Fallback but outer of Retry so that failed retries count toward the circuit breaker's failure metric, and once the circuit opens no more retries are attempted.

---

## Q24. What is the difference between a reactive policy (fault handling) and a proactive policy (fault prevention) in Polly?

**Concepts**
- Reactive policy — activates after observing a failure in the operation's outcome
- Proactive policy — intervenes based on constraints before outcome is known
- Retry, Circuit Breaker, and Fallback as reactive strategies
- Timeout and Rate Limiter as proactive strategies
- Hedging as a hybrid — reacting to slowness by proactively sending a parallel request

**Answer**

A reactive policy handles a failure after it has already occurred — it activates when an exception is thrown or when a result value indicates failure such as an HTTP 5xx status code. A proactive policy prevents a failure from happening by enforcing a constraint before the outcome is known — timeout and rate limiter strategies are proactive because they intervene based on time elapsed or concurrency count, not on whether the operation ultimately failed. Retry, Circuit Breaker, and Fallback are reactive: they inspect the result of the operation and respond to failures already observed. Timeout and Rate Limiter are proactive: Timeout enforces a maximum wait regardless of outcome, and Rate Limiter rejects requests before they are sent when concurrency or throughput limits are exceeded. Hedging is a hybrid: it reacts to slow responses — a form of implicit failure — by proactively sending a parallel request before the first one fails, without waiting for a definitive failure signal.

---

## Q25. How does Polly v8's ResiliencePipeline differ from the v7 Policy API?

**Concepts**
- `ResiliencePipeline<T>` replacing `IAsyncPolicy<T>` as the core abstraction
- Fluent builder `AddX()` replacing `Policy.WrapAsync(outer, inner)`
- Built-in `System.Diagnostics.Metrics` telemetry replacing external listeners
- `AddHedging()` as new strategy only available in v8

**Answer**

Polly v8 introduced a completely redesigned API centered on `ResiliencePipeline<T>` and `ResiliencePipelineBuilder<T>`, replacing the v7 `IAsyncPolicy<T>` and `Policy.WrapAsync()` model. The v8 API is built on top of .NET's `System.Threading.RateLimiting` primitives, integrates with `System.Diagnostics.Metrics` for built-in telemetry, and replaces the Bulkhead strategy with a proper Rate Limiter strategy. The composition model changed from `Policy.WrapAsync(outer, inner)` to a fluent `ResiliencePipelineBuilder.AddX()` chain where declaration order defines the pipeline structure, which is more readable. Built-in telemetry is a significant improvement over v7: the v8 SDK emits metrics automatically to `System.Diagnostics.Metrics` and `DiagnosticSource` without requiring external listeners, so circuit breaker state, retry counts, and timeout events appear in your observability stack with no extra setup. Migration from v7 to v8 requires updating policy registration calls; the behavior and semantics are equivalent, but extension method names and namespaces changed. The v8 `ResiliencePipelineBuilder` also integrates directly with `IHttpClientBuilder` via `Microsoft.Extensions.Http.Resilience`, which did not exist for v7.

---

## Q26. How do you register Polly resilience with IHttpClientFactory in .NET so it applies to all outgoing HTTP calls?

**Concepts**
- Resilience handler as a `DelegatingHandler` in the `HttpClient` middleware chain
- Per-named-client independent resilience pipeline instances
- `AddStandardResilienceHandler()` attaching Microsoft's default pipeline
- Custom pipeline replacing standard via `AddResilienceHandler()`

**Answer**

Named or typed `HttpClient` instances registered through `IHttpClientFactory` can have resilience pipelines attached in `Program.cs` using extension methods from the `Microsoft.Extensions.Http.Resilience` package. The pipeline executes automatically for every request sent by that client, including redirects, without any per-call code.

```csharp
// Program.cs (.NET 8+)
builder.Services.AddHttpClient("PaymentsClient")
    .AddStandardResilienceHandler(); // attaches default pipeline
```

The resilience handler is added as an additional `DelegatingHandler` in the `HttpClient` middleware chain, sitting between the caller's code and the actual HTTP transport. Each named client has its own independent resilience pipeline instance, so the circuit breaker state for the Payments client is separate from the circuit breaker for the Orders client — they do not share failure counts. Custom pipelines can replace the standard one: `.AddResilienceHandler("custom", builder => { builder.AddRetry(...); builder.AddCircuitBreaker(...); })`.

---

## Chapter 7 — Microsoft.Extensions.Http.Resilience (.NET 8+)

---

## Q27. What is Microsoft.Extensions.Http.Resilience, and how does it differ from using Polly directly?

**Concepts**
- Higher-level package built on Polly v8 with opinionated defaults for HttpClient
- `AddStandardResilienceHandler()` providing well-tuned five-layer default pipeline
- Configuration via options validated at startup rather than inline Polly builder calls
- Built-in `System.Diagnostics.Metrics` telemetry without manual setup

**Answer**

`Microsoft.Extensions.Http.Resilience` is a Microsoft-authored NuGet package introduced with .NET 8 that provides opinionated, pre-configured resilience pipelines for `HttpClient` instances managed by `IHttpClientFactory`. It is built on top of Polly v8 internally but exposes a higher-level API that integrates with dependency injection, options validation, and telemetry without requiring the caller to configure individual Polly strategies. The key addition over raw Polly is `AddStandardResilienceHandler()`, which configures a well-tuned default pipeline — retry, circuit breaker, attempt timeout, total request timeout, and hedging for eligible methods — following Microsoft's best-practice defaults. It also provides `AddStandardHedgingHandler()`, which specializes the pipeline for hedging-optimized scenarios like read-heavy endpoints. Using this package means resilience configuration lives in options configurable via `appsettings.json`, is validated at startup, and emits metrics to `System.Diagnostics.Metrics` automatically — three concerns that would require manual setup when using raw Polly.

---

## Q28. What does `AddStandardResilienceHandler()` configure by default, and what can you customize?

**Concepts**
- Default pipeline layers: total request timeout, retry, circuit breaker, per-attempt timeout
- `HttpStandardResilienceOptions` exposing all defaults for override via options pattern
- Default retry predicate targeting `HttpRequestException` and 408, 429, 500–599
- Layer-level customization without rewriting the entire pipeline

**Answer**

`AddStandardResilienceHandler()` registers a resilience pipeline that includes, in outer-to-inner order: a total request timeout defaulting to 30 seconds, retry with exponential backoff and jitter defaulting to three retries, a circuit breaker defaulting to a 10% failure rate over a 30-second window with a 30-second break duration, and a per-attempt timeout defaulting to 10 seconds. These defaults follow Microsoft's guidance for typical HTTP microservice calls and are conservative enough to be safe to enable without tuning. All defaults are exposed through strongly typed options — `HttpStandardResilienceOptions` — that can be overridden in `appsettings.json` or via the options pattern in code, so teams do not have to rewrite the entire pipeline to change a single value. The retry predicate defaults to retrying on `HttpRequestException` and HTTP status codes 408, 429, and 500–599, which covers the most common transient HTTP failures. Individual layer options can be customized via the overload: `.AddStandardResilienceHandler(options => { options.Retry.MaxRetryAttempts = 5; options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(60); })`.

---

## Q29. How do you apply per-named-client resilience options using `AddResilienceHandler()`?

**Concepts**
- `AddResilienceHandler(name, configure)` for fully custom pipeline on a named client
- Pipeline name used for metrics labeling in observability dashboards
- No defaults applied — every needed strategy must be added explicitly
- Shared pipeline via `AddResiliencePipeline()` reused across multiple named clients

**Answer**

`AddResilienceHandler(name, configure)` lets you attach a fully custom resilience pipeline to a named `HttpClient`, where the `configure` callback receives a `ResiliencePipelineBuilder<HttpResponseMessage>` and you add strategies manually. This is appropriate when the default pipeline's structure does not match the target service's SLA or when you need a strategy the standard handler does not include.

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

The pipeline name — the first argument — is used for metrics labeling and diagnostics, so using a meaningful name helps trace which pipeline is affecting a given request in observability dashboards. Multiple named clients can share the same pipeline definition by extracting it into a reusable `ResiliencePipeline<HttpResponseMessage>` registered with `AddResiliencePipeline()` and referenced by name. Unlike `AddStandardResilienceHandler()`, `AddResilienceHandler()` applies no defaults — every strategy you need must be added explicitly.

---

## Chapter 8 — Health Checks & Self-Healing

---

## Q30. What are ASP.NET Core health checks, and how do they contribute to overall system resilience?

**Concepts**
- Health check endpoints reporting Healthy, Degraded, or Unhealthy status
- Pull-based mechanism polled by orchestrators, not pushed by the service
- Pre-built community packages for SQL Server, Redis, RabbitMQ, and other dependencies
- Orchestrator removing unhealthy instances from load balancing before users see failures

**Answer**

ASP.NET Core health checks are lightweight HTTP endpoints — typically `/health`, `/healthz/ready`, `/healthz/live` — that report whether the application and its dependencies are operating correctly. They contribute to resilience by giving orchestration platforms such as Kubernetes, load balancers, and service meshes the signal they need to remove an unhealthy instance from the load-balancing rotation before it serves failed requests to users.

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddUrlGroup(new Uri("https://api.dependency.com/health"), "dependency");

app.MapHealthChecks("/healthz/ready");
```

Health checks are a pull-based mechanism: the orchestrator periodically polls the endpoint rather than the service pushing a status update, which is simpler and survives network partitions gracefully. The `AspNetCore.HealthChecks.*` community packages provide ready-made checks for SQL Server, Redis, RabbitMQ, and many other dependencies so teams do not have to write probe logic from scratch. Health check results are categorized as Healthy, Degraded, or Unhealthy — returning Degraded allows the orchestrator to log a warning while keeping the instance in rotation, which is useful for non-critical dependency failures.

---

## Q31. How do Kubernetes liveness and readiness probes differ, and how do you configure an ASP.NET Core service to serve both?

**Concepts**
- Readiness probe failure — removes pod from endpoints without restart
- Liveness probe failure — triggers pod restart for unrecoverable conditions
- Same check for both as a common mistake causing restart loops
- Tag-based filtering mapping separate check sets to separate endpoints

**Answer**

A readiness probe tells Kubernetes whether the pod is ready to accept traffic — failing readiness removes the pod from the service's endpoints without restarting it, which is appropriate for temporary conditions like a downstream database being unavailable. A liveness probe tells Kubernetes whether the pod is still alive and functioning — failing liveness causes Kubernetes to restart the pod, which is appropriate for unrecoverable states like deadlocks or out-of-memory conditions. A common mistake is using the same health check for both probes: a pod that cannot reach its database should fail readiness — stop receiving traffic — but should not fail liveness, because Kubernetes would restart it in a loop even though the database is the problem, not the pod. In ASP.NET Core, liveness checks should verify that the process is responsive, while readiness checks verify that dependencies like databases, caches, and message brokers are reachable. Mapping separate endpoints is the recommended pattern: `app.MapHealthChecks("/healthz/live", liveOptions)` and `app.MapHealthChecks("/healthz/ready", readyOptions)` using tag-based filtering to include only the appropriate checks in each endpoint.

---

## Q32. What role does a service mesh (such as Istio) or a load balancer play in resilience, and how does that differ from application-layer resilience?

**Concepts**
- Infrastructure-layer resilience — retry and circuit breaking without application code changes
- Application-layer resilience — business-aware logic inspecting response content and error codes
- Infrastructure retry handling transport-level faults but not application-level error semantics
- Two layers as complementary rather than redundant

**Answer**

A service mesh or smart load balancer provides infrastructure-layer resilience — retry, timeout, circuit breaking, and load balancing — transparently to all services in the mesh without any application code changes. Application-layer resilience implemented with Polly or `Microsoft.Extensions.Http.Resilience` runs within the service process and can be aware of business logic, response content, and request context in ways that the infrastructure layer cannot. Infrastructure-layer retry is useful for basic transport-level transient faults such as TCP resets or brief DNS resolution failures and requires no deployment of new application code, but it cannot distinguish between a retryable application-level 500 and a non-retryable business-logic 500 — both look the same to the sidecar proxy. Application-layer resilience can inspect the response body, log structured correlation data, and decide whether a 500 is retryable based on error codes in the JSON payload — nuance that a generic mesh proxy cannot perform. The two layers are complementary: infrastructure retry handles transport-level failures and eliminates the need for redundant retry logic in every service, while application-layer circuit breaking, fallbacks, and hedging handle the richer failure semantics that only the application understands.

---
