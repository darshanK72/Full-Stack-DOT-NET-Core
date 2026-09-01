# Azure Event Grid — Interview Q&A
> 27 questions · Back to [README](../README.md)

## Table of Contents
1. [What is Azure Event Grid, and what problem does it solve in event-driven archite…](#q1)
2. [How does Azure Event Grid differ from Azure Service Bus, Azure Event Hubs, and A…](#q2)
3. [What is the publish/subscribe model in Event Grid, and how does it decouple even…](#q3)
4. [What types of event sources can Azure Event Grid integrate with out of the box?](#q4)
5. [What is the difference between push-based and pull-based event delivery, and whi…](#q5)
6. [What is an Event Grid topic, and how does it differ between a custom topic, a sy…](#q6)
7. [What is an Event Grid domain, and when would you use a domain instead of multipl…](#q7)
8. [What is an Event Grid subscription, and what endpoint types can it deliver event…](#q8)
9. [How do subscription filters work in Event Grid — subject prefix/suffix, event ty…](#q9)
10. [What is a dead-letter destination for Event Grid, and when should you configure …](#q10)
11. [What is the default Event Grid event schema, and what are the key properties in …](#q11)
12. [What is CloudEvents, and how does Event Grid support the CloudEvents 1.0 schema?](#q12)
13. [How do you publish custom application events to an Event Grid custom topic from …](#q13)
14. [What is the difference between `eventType`, `subject`, and `dataVersion` in an E…](#q14)
15. [How do you evolve event schemas without breaking existing Event Grid subscribers…](#q15)
16. [How does Event Grid deliver events to webhook subscribers, and what HTTP respons…](#q16)
17. [What is Event Grid's retry policy when delivery to a subscriber fails?](#q17)
18. [What is the difference between at-least-once delivery in Event Grid and exactly-…](#q18)
19. [How do you implement idempotent event handlers for Event Grid webhooks in ASP.NE…](#q19)
20. [What causes events to be dead-lettered in Event Grid, and how do you replay them…](#q20)
21. [What is the difference between a webhook endpoint and an Azure Function Event Gr…](#q21)
22. [How do you secure an Event Grid webhook endpoint in ASP.NET Core?](#q22)
23. [How does Event Grid validate a webhook subscription during creation (the validat…](#q23)
24. [How do you handle Event Grid subscription validation in an ASP.NET Core API?](#q24)
25. [How do you use the `Azure.Messaging.EventGrid` SDK to publish events from a .NET…](#q25)
26. [What is Event Grid's role in Azure resource change notifications (system events)…](#q26)
27. [What monitoring and troubleshooting practices help diagnose failed Event Grid de…](#q27)

---

## Q1. What is Azure Event Grid, and what problem does it solve in event-driven architecture?

What is Azure Event Grid, and what problem does it solve in event-driven architecture?

**Answer:** Azure Event Grid is a fully managed publish/subscribe event routing service that connects event producers to consumers without those parties knowing about each other. It solves the integration problem in event-driven architecture by letting you react to state changes — in your own application or in Azure resources — through a central router instead of point-to-point HTTP callbacks or polling.

- Publishers emit events to a topic or domain endpoint; Event Grid matches each event against subscription filters and pushes copies only to subscribers that care about that event type or subject.
- Because delivery, retry, and fan-out are handled by the service, your application code stays focused on business logic rather than building a custom notification dispatcher.
- Event Grid fits reactive workflows such as "when a blob is uploaded, start a thumbnail function" or "when an order is placed, notify billing and inventory" where loose coupling and near-real-time notification matter more than guaranteed ordering across all consumers.

---

## Q2. How does Azure Event Grid differ from Azure Service Bus, Azure Event Hubs, and Azure Queue Storage?

How does Azure Event Grid differ from Azure Service Bus, Azure Event Hubs, and Azure Queue Storage?

**Answer:** Event Grid is an event router optimized for discrete notifications and fan-out, while Service Bus is a general-purpose message broker with queues and topics, Event Hubs is a high-throughput event ingestion stream for telemetry, and Queue Storage is a simple durable queue for work items.

| | Event Grid | Service Bus | Event Hubs | Queue Storage |
|---|---|---|---|---|
| Primary model | Push-based pub/sub routing | Queues and topics with brokered messages | Partitioned event stream | Single queue per storage account |
| Typical payload | Small event notification (often a pointer or delta) | Commands or integration messages of varying size | High-volume telemetry / log streams | Job or task messages |
| Consumer style | Event Grid pushes to subscriber endpoints | Consumers pull (peek-lock or receive) | Consumers read partitions with offsets | Workers dequeue messages |
| Ordering | No global ordering guarantee across subscribers | Sessions can enforce order per key | Ordered within a partition | Best-effort FIFO |
| Best fit | React to Azure or app events with many independent handlers | Reliable async messaging between services | Analytics, IoT ingestion at scale | Simple background job queues |

Choose Event Grid when you need lightweight, many-to-many notification routing; choose Service Bus or Event Hubs when you need durable messaging semantics, larger payloads, or stream processing at very high volume.

---

## Q3. What is the publish/subscribe model in Event Grid, and how does it decouple event publishers from subscribers?

What is the publish/subscribe model in Event Grid, and how does it decouple event publishers from subscribers?

**Answer:** In Event Grid's publish/subscribe model, a publisher sends an event to a topic without knowing which applications will react, and each subscriber registers interest through a subscription filter so Event Grid delivers only matching events. The publisher and subscriber are decoupled in both identity and timing — neither needs a direct URL or contract with the other.

- The publisher only needs the topic endpoint and permission to publish; it does not maintain a list of downstream services or call them synchronously.
- Subscribers declare what they want via filters on event type, subject, or advanced JSON properties, and can be added or removed without redeploying the publisher.
- Multiple subscribers can receive the same event independently, which supports parallel reactions (send email, update cache, start workflow) from one domain event without the origin service orchestrating each step.

---

## Q4. What types of event sources can Azure Event Grid integrate with out of the box?

What types of event sources can Azure Event Grid integrate with out of the box?

**Answer:** Event Grid integrates natively with many Azure services as system event sources and also accepts custom events from your own applications through custom topics and domains.

- **System topics** emit resource change events from Azure services such as Storage (blob created/deleted), Key Vault (secret near expiry), App Configuration (setting changed), Resource Groups, and Subscriptions without you operating a separate messaging infrastructure.
- **Custom topics** and **domains** let your .NET microservices, Logic Apps, or third-party systems publish application-level domain events (for example `Order.Placed`) to endpoints you control.
- **Partner topics** and **namespace topics** extend the same routing model to software-as-a-service (SaaS) partners and newer Event Grid namespace scenarios where events from external providers enter your Azure subscription.
- Event subscriptions can target Azure Functions, Logic Apps, webhooks, Service Bus queues or topics, Event Hubs, hybrid connections, and other supported handler types, so the same event fabric connects Azure-native and custom code.

---

## Q5. What is the difference between push-based and pull-based event delivery, and which model does Event Grid use?

What is the difference between push-based and pull-based event delivery, and which model does Event Grid use?

**Answer:** Push-based delivery means the messaging service actively sends each event to the subscriber's endpoint as soon as it is routed, while pull-based delivery means the consumer polls or reads from a buffer when it is ready. Event Grid uses push-based delivery — it POSTs events to webhooks or invokes integrated handlers directly.

- With push, subscribers do not run a polling loop or manage cursor offsets; they expose an HTTP endpoint (or use a native Azure trigger) and respond when Event Grid delivers.
- Push reduces latency for reactive scenarios because the handler runs immediately after the event is published, which suits notifications and workflow triggers better than batch polling.
- The trade-off is that the subscriber must be available and respond within Event Grid's timeout window; failed deliveries enter retry and optional dead-lettering rather than sitting in a queue the consumer reads later (though you can configure a Service Bus queue as the subscription endpoint for a pull-style downstream).

---

## Chapter 2 — Topics, Domains & Subscriptions

---

## Q6. What is an Event Grid topic, and how does it differ between a custom topic, a system topic, and a partner topic?

What is an Event Grid topic, and how does it differ between a custom topic, a system topic, and a partner topic?

**Answer:** An Event Grid topic is the publish endpoint and routing boundary where events are ingested before Event Grid evaluates subscriptions; the three topic kinds differ in who publishes events and what they represent.

- **Custom topics** are application-owned endpoints you create for your own domain events. Your .NET service publishes to the topic URL using an access key, shared access signature (SAS), or Microsoft Entra ID (formerly Azure AD) token, and you define subscriptions for each reacting service.
- **System topics** are managed automatically for an Azure resource provider (for example a Storage account). Azure publishes resource lifecycle events to the system topic; you only create subscriptions — you do not publish to it from application code.
- **Partner topics** receive events from an authorized external partner (SaaS) into your subscription, with registration and activation steps so only approved partners can send events on your behalf.

All three use the same subscription and filter mechanics; the difference is the event source and who controls publishing.

---

## Q7. What is an Event Grid domain, and when would you use a domain instead of multiple separate custom topics?

What is an Event Grid domain, and when would you use a domain instead of multiple separate custom topics?

**Answer:** An Event Grid domain is a management container for up to thousands of related custom topics under one security and billing boundary, with a single publish credential model at the domain level and per-topic routing inside it.

- Use a domain when a multi-tenant SaaS platform or large solution needs many isolated topics (one per tenant or bounded context) but you want centralized administration, shared access policies, and consistent monitoring instead of provisioning hundreds of standalone custom topics.
- Each topic inside the domain still has its own endpoint and subscriptions, so tenants or teams remain logically separated while operations teams manage one domain resource.
- For a small application with one or two event streams, a single custom topic is simpler; domains pay off when topic count, tenant isolation, or uniform governance requirements grow.

---

## Q8. What is an Event Grid subscription, and what endpoint types can it deliver events to?

What is an Event Grid subscription, and what endpoint types can it deliver events to?

**Answer:** An Event Grid subscription binds a topic (or system topic scope) to one or more destination handlers and optional filters, defining which events are delivered and where they go.

- Supported **endpoint types** include HTTPS webhooks, Azure Functions (Event Grid trigger), Azure Logic Apps, Azure Automation webhooks, Azure Service Bus queues or topics, Azure Event Hubs, Azure Functions (as a resource destination), hybrid connections, and partner destinations depending on scenario and API version.
- Each subscription can specify filters so the same topic feeds different handlers — for example one subscription for `Order.Placed` to a billing Function and another for `Order.Shipped` to a notification webhook.
- Subscriptions also carry delivery schema choice (Event Grid schema vs CloudEvents), retry policy, dead-letter configuration, and optional expiration time for temporary integrations.

---

## Q9. How do subscription filters work in Event Grid — subject prefix/suffix, event types, and advanced filters?

How do subscription filters work in Event Grid — subject prefix/suffix, event types, and advanced filters?

**Answer:** Event Grid subscriptions use filters so only events matching declared criteria are delivered to that subscriber, reducing noise and letting multiple handlers share one topic safely.

- **Included event types** restrict delivery to a list such as `Microsoft.Storage.BlobCreated` for system events or `Order.Placed` for custom events; omitted types are not sent to that subscription.
- **Subject begins with** and **subject ends with** match the `subject` property prefix or suffix — useful when subjects encode hierarchy like `/orders/12345/lines/1` and a handler only cares about `/orders/`.
- **Advanced filters** evaluate JSON fields in the event envelope or `data` payload with operators (string contains, number greater than, boolean equals, and others) and support AND/OR logic for fine-grained routing without creating many separate topics.
- If no filter is configured, the subscription receives all events published to that topic scope, which is appropriate only when a single handler processes every event.

---

## Q10. What is a dead-letter destination for Event Grid, and when should you configure one?

What is a dead-letter destination for Event Grid, and when should you configure one?

**Answer:** A dead-letter destination is an Azure Storage blob container where Event Grid stores events that could not be delivered to the subscriber endpoint after retries or that failed with non-retriable errors, so you can inspect, fix, and replay them instead of losing the notification silently.

- Configure dead-lettering on production subscriptions whenever business-critical reactions (payments, provisioning, compliance audit) depend on event delivery — the default is to drop undeliverable events if no dead-letter container is set.
- Events are dead-lettered when maximum delivery attempts or event time-to-live is exceeded, or immediately on certain HTTP status codes such as 400 Bad Request or 413 Payload Too Large that indicate retry will not succeed.
- Operations teams pull dead-lettered JSON blobs from storage, diagnose handler bugs or misconfiguration, redeploy fixes, and optionally republish or manually process the stored events.

---

## Chapter 3 — Event Schemas & CloudEvents

---

## Q11. What is the default Event Grid event schema, and what are the key properties in every event envelope?

What is the default Event Grid event schema, and what are the key properties in every event envelope?

**Answer:** The default Event Grid schema wraps each notification in a JSON envelope with standard metadata fields separate from your business payload in `data`, so routing and filtering work consistently across Azure and custom events.

- **`id`** — unique identifier for this event occurrence; use it for idempotency checks in handlers.
- **`topic`** — resource path of the topic that received the event; identifies the publishing scope.
- **`subject`** — path-like string describing the entity the event refers to (for example blob URL or `/orders/12345`).
- **`eventType`** — string naming what happened (for example `Microsoft.Storage.BlobCreated` or `Order.Placed`).
- **`eventTime`** — UTC timestamp when the event occurred.
- **`data`** — publisher-specific payload (JSON object); size and shape are defined by the event type.
- **`dataVersion`** — schema version of the `data` payload so consumers can deserialize the correct shape.
- **`metadataVersion`** — version of the envelope metadata schema itself.

Webhooks often receive an array of events in one HTTP POST when batching is enabled, so handlers should iterate the array rather than assume a single object.

---

## Q12. What is CloudEvents, and how does Event Grid support the CloudEvents 1.0 schema?

What is CloudEvents, and how does Event Grid support the CloudEvents 1.0 schema?

**Answer:** CloudEvents is a vendor-neutral specification (CNCF standard) for describing event metadata in a consistent way across platforms, and Event Grid can deliver events using the CloudEvents 1.0 schema instead of the native Event Grid envelope.

- When you set a subscription's delivery schema to CloudEvents, Event Grid maps its fields to CloudEvents attributes such as `id`, `source`, `type`, `time`, and `data`, which improves portability if the same handler consumes events from multiple cloud providers or on-premises systems.
- Publishing to custom topics can also use CloudEvents format so upstream producers already emitting CloudEvents do not need a custom adapter layer.
- The underlying routing, filtering, and retry behavior is identical; only the on-the-wire JSON shape changes, so choose CloudEvents when interoperability is a requirement and Event Grid schema when integrating primarily with Azure-native tooling and documentation examples.

---

## Q13. How do you publish custom application events to an Event Grid custom topic from a .NET application?

How do you publish custom application events to an Event Grid custom topic from a .NET application?

**Answer:** From .NET you use the `Azure.Messaging.EventGrid` client library to construct `EventGridEvent` instances (or CloudEvents) and send them to the custom topic's HTTPS endpoint, authenticating with a topic key, SAS token, or Microsoft Entra credential.

- Register the NuGet package `Azure.Messaging.EventGrid`, create an `EventGridPublisherClient` with the topic endpoint URI and credential, and call `SendEventAsync` or `SendEventsAsync` with one or more events.
- Each event needs at minimum a distinct `id`, a meaningful `subject`, an `eventType` your subscribers filter on, and a serializable `data` object representing the domain occurrence.
- Publishing is fire-and-forget from the application's perspective — Event Grid accepts the batch and takes responsibility for fan-out, retry, and dead-lettering to subscribers; your service should still handle publish failures (network, auth) and optionally use the outbox pattern if the event must align with a database commit (see distributed messaging modules).

```csharp
var client = new EventGridPublisherClient(
    new Uri(topicEndpoint),
    new AzureKeyCredential(topicKey));

await client.SendEventAsync(new EventGridEvent(
    subject: "/orders/1001",
    eventType: "Order.Placed",
    dataVersion: "1.0",
    data: new { OrderId = 1001, Total = 49.99m }));
```

---

## Q14. What is the difference between `eventType`, `subject`, and `dataVersion` in an Event Grid event?

What is the difference between `eventType`, `subject`, and `dataVersion` in an Event Grid event?

**Answer:** These three fields serve different roles: `eventType` names what happened, `subject` identifies which entity it happened to, and `dataVersion` declares which shape of the payload in `data` the publisher used.

- **`eventType`** is the primary routing key for subscriptions — handlers subscribe to types like `Order.Placed` or `Microsoft.Storage.BlobCreated` rather than inspecting payload details first.
- **`subject`** provides a hierarchical locator for the affected resource (order ID, blob path, user ID) and supports prefix/suffix filters so one handler processes all events under `/orders/` while another handles `/inventory/`.
- **`dataVersion`** lets you evolve the JSON inside `data` — when you add fields or restructure, increment the version so new subscribers deserialize v2 while old handlers can remain on v1 filters until migrated.

Confusing `eventType` with `subject` leads to overly broad subscriptions or filters that miss events; treat type as verb/category and subject as noun/instance path.

---

## Q15. How do you evolve event schemas without breaking existing Event Grid subscribers?

How do you evolve event schemas without breaking existing Event Grid subscribers?

**Answer:** Treat event contracts as versioned interfaces: prefer additive changes, use `dataVersion` to signal breaking layout changes, and filter subscriptions so consumers opt into new types or versions rather than silently receiving incompatible payloads.

- **Additive evolution** — add optional JSON properties to `data` with defaults implied in handler code; existing subscribers ignore unknown fields and keep working.
- **Breaking evolution** — publish under a new `eventType` (for example `Order.Placed.v2`) or bump `dataVersion` and create new subscriptions pointed at updated handlers while old subscriptions drain on the previous version.
- **Dual publishing** during migration — temporarily emit both v1 and v2 events or run parallel subscriptions until all consumers upgrade.
- Document the contract (JSON schema or OpenAPI-style description) and coordinate with teams owning downstream Functions and webhooks, since Event Grid does not enforce schema validation at publish time the way some schema registries do for Kafka.

---

## Chapter 4 — Delivery, Retry & Reliability

---

## Q16. How does Event Grid deliver events to webhook subscribers, and what HTTP response must the endpoint return?

How does Event Grid deliver events to webhook subscribers, and what HTTP response must the endpoint return?

**Answer:** Event Grid delivers events by sending an HTTP POST with a JSON body (one event or an array) to the subscriber URL, and the endpoint must respond with a 2xx status code within the service timeout so Event Grid marks the delivery successful.

- The default timeout is 30 seconds for the webhook to return a response after receiving the POST; slow handlers should acknowledge quickly and process asynchronously if needed, though you must still return success only after you accept responsibility for the event or have safely queued it.
- Any non-2xx response or timeout causes Event Grid to schedule a retry according to the subscription retry policy rather than dropping the event immediately.
- Handlers should parse the body as an array of events, return 400 only when the payload is permanently invalid (which can trigger immediate dead-lettering), and avoid returning 5xx for business validation failures that will fail again on retry unless you intend to dead-letter after exhaustion.

---

## Q17. What is Event Grid's retry policy when delivery to a subscriber fails?

What is Event Grid's retry policy when delivery to a subscriber fails?

**Answer:** When delivery fails with a retriable error or timeout, Event Grid retries with exponential backoff until either the configured maximum delivery attempts or event time-to-live is reached, whichever limit is hit first.

- The default schedule uses increasing delays (approximately 30 seconds, 1 minute, 5 minutes, and then continued retries up to the policy limits) on a best-effort basis; exact intervals are documented by Microsoft and may vary slightly by service tier.
- You can customize **max delivery attempts** (1–30, default 30) and **event time-to-live** in minutes (1–1440, default 1440) per subscription on the Additional features settings or via Azure CLI, REST, or infrastructure-as-code.
- Non-retriable responses such as 400 Bad Request or 413 Request Entity Too Large skip further retries and move toward dead-lettering immediately because repeating the same request will not succeed.
- If both TTL and attempt limits are set, the first limit reached stops delivery — for example a short TTL may dead-letter before all attempt slots are used.

---

## Q18. What is the difference between at-least-once delivery in Event Grid and exactly-once event processing?

What is the difference between at-least-once delivery in Event Grid and exactly-once event processing?

**Answer:** Event Grid guarantees at-least-once delivery to subscribers, meaning an event may arrive more than once if a handler returns success slowly, crashes after processing, or a network failure causes ambiguity — it does not guarantee exactly-once end-to-end processing.

- **At-least-once (transport)** — Event Grid may redeliver the same `id` until it receives a successful acknowledgment or exhausts retries.
- **Exactly-once processing (application goal)** — your handler must be **idempotent**: processing the same event `id` twice produces the same final state as processing it once, typically by storing processed IDs or using natural keys in the database.
- Exactly-once semantics across publish, route, and process require patterns such as idempotent consumers, deduplication stores, or transactional outbox — Event Grid alone does not deduplicate on behalf of subscribers.

See Q19 for implementing idempotency in ASP.NET Core handlers.

---

## Q19. How do you implement idempotent event handlers for Event Grid webhooks in ASP.NET Core?

How do you implement idempotent event handlers for Event Grid webhooks in ASP.NET Core?

**Answer:** An idempotent Event Grid handler records each event's `id` (or a business deduplication key from `data`) before or atomically with side effects, so duplicate deliveries become no-ops instead of double charges or duplicate records.

- On receipt, check a durable store (SQL table, Redis, or Azure Table Storage) for the event `id`; if present, return 200 OK immediately without re-running business logic.
- If absent, perform the side effect and insert the `id` in the same transaction as the business write when possible, so a crash between processing and acknowledgment does not leave inconsistent state.
- Use the event `id` from the envelope, not only HTTP request correlation, because retries resend the same event payload with the same `id`.
- Return 200 only after the event is safely recorded or processed; returning success before persistence causes Event Grid to stop retrying while your handler may not have finished work.

```csharp
if (await _processedEvents.ExistsAsync(evt.Id))
    return Ok();

await _orderService.PlaceOrderAsync(evt.Data);
await _processedEvents.MarkAsync(evt.Id);
return Ok();
```

---

## Q20. What causes events to be dead-lettered in Event Grid, and how do you replay them?

What causes events to be dead-lettered in Event Grid, and how do you replay them?

**Answer:** Events are dead-lettered when Event Grid cannot deliver them successfully within the subscription retry limits, when event time-to-live expires, or when the endpoint returns immediate non-retriable HTTP errors — and replay means reading those blobs from the dead-letter container and republishing or manually reprocessing them.

- **Causes** — exhausted max attempts, exceeded TTL, HTTP 400/413 responses, or missing/incorrect dead-letter configuration leading to drop instead of storage when limits hit (always configure a container for critical paths).
- **Inspection** — dead-lettered events appear as JSON blobs in the configured Storage container with metadata about failure reason and delivery attempts; use Storage Explorer, Azure Portal, or a script to list and download them.
- **Replay** — fix the root cause (handler bug, auth, filter mismatch), then either republish the `data` to the topic using the Event Grid publisher client or invoke the handler directly with the stored payload; there is no automatic replay button — operations owns the re-drive process.
- Prevent recurring dead-letter volume with monitoring alerts on dead-letter blob count and Application Insights logging inside webhook handlers.

---

## Chapter 5 — Webhooks, Azure Functions & .NET Integration

---

## Q21. What is the difference between a webhook endpoint and an Azure Function Event Grid trigger?

What is the difference between a webhook endpoint and an Azure Function Event Grid trigger?

**Answer:** Both receive Event Grid events, but a webhook is a general HTTPS endpoint you implement (ASP.NET Core API, third-party URL) while an Azure Function Event Grid trigger is a managed binding that invokes your function code with deserialized events and integrated scaling.

| | Webhook (ASP.NET Core) | Azure Function trigger |
|---|---|---|
| Hosting | App Service, container, any HTTPS server | Azure Functions runtime |
| Validation handshake | You implement subscription validation response | Handled by the Functions runtime for supported versions |
| Scaling | Scales with your app plan / replicas | Consumption or premium plan auto-scale per invocation |
| Control | Full middleware, auth pipeline, custom routing | Simpler function body; less HTTP ceremony |
| Best for | Existing APIs, strict enterprise HTTP policies | Lightweight reactive handlers, rapid prototyping |

Choose webhooks when the handler lives in an existing ASP.NET Core service or you need full control over the HTTP pipeline; choose Functions when you want minimal boilerplate and serverless scaling for isolated event reactions.

---

## Q22. How do you secure an Event Grid webhook endpoint in ASP.NET Core?

How do you secure an Event Grid webhook endpoint in ASP.NET Core?

**Answer:** Secure Event Grid webhooks by validating that requests genuinely originate from Event Grid, restricting network access where possible, and treating the endpoint as an authenticated integration point rather than a public anonymous API.

- During subscription creation, Event Grid sends a **SubscriptionValidation** event; your API must echo the `validationCode` in the response body so only someone who can receive that handshake completes registration (this also proves endpoint ownership at setup time).
- For ongoing delivery, configure **Microsoft Entra ID (Azure AD) authentication** on the webhook so Event Grid presents a bearer token your API validates, or use a **shared secret** model with a query parameter or header known to both sides when Entra is not available.
- Restrict ingress with **IP filtering** or private endpoints where your architecture allows, enable HTTPS only, and avoid exposing unnecessary routes on the same host.
- Apply authorization policies in ASP.NET Core (`[Authorize]`) on the Event Grid route and log validation failures separately from business processing errors.

---

## Q23. How does Event Grid validate a webhook subscription during creation (the validation handshake)?

How does Event Grid validate a webhook subscription during creation (the validation handshake)?

**Answer:** When you create an event subscription pointing at an HTTPS webhook, Event Grid immediately POSTs a special `Microsoft.EventGrid.SubscriptionValidationEvent` to that URL and expects a JSON response containing the `validationCode` from the event data, proving you control the endpoint before any real events flow.

- The validation request looks like a normal event batch but `eventType` is `Microsoft.EventGrid.SubscriptionValidationEvent` and `data` includes `validationCode` and optionally `validationUrl` for manual validation scenarios.
- Your endpoint must respond with HTTP 200 and body `{ "validationResponse": "<validationCode>" }` synchronously during subscription creation; until this succeeds, the subscription stays in a provisioning or failed state.
- If automatic validation fails (firewall, wrong URL, handler treats validation as unknown event type), you can complete validation manually via the `validationUrl` link in the event data from a browser or HTTP client.
- Handlers must branch on validation events before business logic — treating validation as an error returns failed provisioning and blocks delivery.

---

## Q24. How do you handle Event Grid subscription validation in an ASP.NET Core API?

How do you handle Event Grid subscription validation in an ASP.NET Core API?

**Answer:** In ASP.NET Core, deserialize the incoming event array, detect `Microsoft.EventGrid.SubscriptionValidationEvent`, and return `Ok(new { validationResponse = validationCode })` without running business handlers — typically in a dedicated minimal API route or controller action.

- Use `System.Text.Json` or the `Azure.Messaging.EventGrid` models to parse the batch; the validation event is always the first concern when the subscription is new.
- Example flow: read POST body → foreach event, if type is SubscriptionValidationEvent, extract `data.validationCode` → return 200 with `validationResponse` immediately.
- After validation succeeds, the same endpoint receives normal events and should route by `eventType` to application services.
- For local development, use ngrok or Azure Relay so Event Grid can reach localhost during handshake testing; without a public HTTPS URL, subscription creation fails validation.

```csharp
app.MapPost("/eventgrid", async (HttpRequest request) =>
{
    var events = await JsonSerializer.DeserializeAsync<EventGridEvent[]>(request.Body);
    var validation = events?.FirstOrDefault(e =>
        e.EventType == "Microsoft.EventGrid.SubscriptionValidationEvent");
    if (validation?.Data is JsonElement data &&
        data.TryGetProperty("validationCode", out var code))
        return Results.Ok(new { validationResponse = code.GetString() });

    foreach (var evt in events ?? [])
        await _handler.HandleAsync(evt);

    return Results.Ok();
});
```

---

## Q25. How do you use the `Azure.Messaging.EventGrid` SDK to publish events from a .NET service?

How do you use the `Azure.Messaging.EventGrid` SDK to publish events from a .NET service?

**Answer:** The `Azure.Messaging.EventGrid` package provides `EventGridPublisherClient` for sending events to a custom topic or domain topic endpoint, with credential options matching how you secure the topic in Azure.

- Add the NuGet package, configure the topic HTTPS endpoint from Azure Portal or configuration, and choose `AzureKeyCredential` (access key), `AzureSasCredential`, or `DefaultAzureCredential` for Microsoft Entra ID in production.
- Build `EventGridEvent` objects with unique `id`, `subject`, `eventType`, `dataVersion`, and serializable `data`; batch with `SendEventsAsync` for efficiency.
- Handle `RequestFailedException` for throttling or auth errors — publishing is synchronous over HTTP to Event Grid's ingress, distinct from subscriber delivery retry.
- In ASP.NET Core, register the client as a singleton via `AddSingleton<EventGridPublisherClient>` and inject it into application services that raise domain events after successful commands.

See Q13 for a minimal publish example; prefer managed identity over embedded access keys in deployed environments.

---

## Chapter 6 — Advanced & Operational

---

## Q26. What is Event Grid's role in Azure resource change notifications (system events)?

What is Event Grid's role in Azure resource change notifications (system events)?

**Answer:** Event Grid system topics turn Azure resource lifecycle and data-plane operations into push notifications so your automation reacts to infrastructure and data changes without polling the Resource Manager or Storage APIs.

- When you enable a system topic on a resource (for example Storage, Key Vault, App Service, Azure Subscription), Azure automatically publishes well-defined event types such as blob creation, secret near expiry, or resource write success to that system topic.
- You create **event subscriptions** on the system topic to route those events to Functions, Logic Apps, or webhooks — typical uses include triggering backup workflows, cache invalidation, security auditing, and infrastructure drift remediation.
- System events use Microsoft's published schemas documented per resource provider; `eventType` and `data` shapes are stable and versioned by Microsoft rather than by your application team.
- This model is central to **event-driven automation** in Azure: reactive DevOps and application logic wired to platform signals instead of scheduled scans.

---

## Q27. What monitoring and troubleshooting practices help diagnose failed Event Grid deliveries in production?

What monitoring and troubleshooting practices help diagnose failed Event Grid deliveries in production?

**Answer:** Production Event Grid troubleshooting combines subscription metrics, delivery failure logs, dead-letter blob inspection, and correlated application logging in the subscriber so you can see whether failure happened at routing, transport, or handler logic.

- In Azure Portal, open the topic or domain **Metrics** for publish success, delivery success/failure counts, and dead-lettered events; set alerts when failure rate or dead-letter count exceeds baseline.
- Enable **diagnostic settings** to send Event Grid operational logs to Log Analytics, Storage, or Event Hub for querying failed deliveries, latency, and subscription provisioning errors.
- Inspect the **dead-letter container** for undelivered event JSON and HTTP response details recorded by Event Grid when dead-lettering is enabled.
- In the subscriber (ASP.NET Core or Function), log event `id`, `eventType`, and `subject` with correlation IDs; compare with Event Grid logs to distinguish timeout, 401/403 auth misconfiguration, validation mishandling, and unhandled exceptions.
- Reproduce with **Cloud Shell** or REST API to republish a test event after fixes, and verify subscription filter rules if handlers never receive expected types — misconfigured filters look like delivery failures but are silent drops at routing time.

---
