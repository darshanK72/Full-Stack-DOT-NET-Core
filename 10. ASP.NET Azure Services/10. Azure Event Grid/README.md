# Azure Event Grid — Interview Q&A
> 27 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Azure Event Grid, and what problem does it solve in event-driven architecture?](#q1-what-is-azure-event-grid-and-what-problem-does-it-solve-in-event-driven-architecture)
2. [Q2. How does Azure Event Grid differ from Azure Service Bus, Azure Event Hubs, and Azure Queue Storage?](#q2-how-does-azure-event-grid-differ-from-azure-service-bus-azure-event-hubs-and-azure-queue-storage)
3. [Q3. What is the publish/subscribe model in Event Grid, and how does it decouple event publishers from subscribers?](#q3-what-is-the-publishsubscribe-model-in-event-grid-and-how-does-it-decouple-event-publishers-from-subscribers)
4. [Q4. What types of event sources can Azure Event Grid integrate with out of the box?](#q4-what-types-of-event-sources-can-azure-event-grid-integrate-with-out-of-the-box)
5. [Q5. What is the difference between push-based and pull-based event delivery, and which model does Event Grid use?](#q5-what-is-the-difference-between-push-based-and-pull-based-event-delivery-and-which-model-does-event-grid-use)
6. [Q6. What is an Event Grid topic, and how does it differ between a custom topic, a system topic, and a partner topic?](#q6-what-is-an-event-grid-topic-and-how-does-it-differ-between-a-custom-topic-a-system-topic-and-a-partner-topic)
7. [Q7. What is an Event Grid domain, and when would you use a domain instead of multiple separate custom topics?](#q7-what-is-an-event-grid-domain-and-when-would-you-use-a-domain-instead-of-multiple-separate-custom-topics)
8. [Q8. What is an Event Grid subscription, and what endpoint types can it deliver events to?](#q8-what-is-an-event-grid-subscription-and-what-endpoint-types-can-it-deliver-events-to)
9. [Q9. How do subscription filters work in Event Grid — subject prefix/suffix, event types, and advanced filters?](#q9-how-do-subscription-filters-work-in-event-grid-subject-prefixsuffix-event-types-and-advanced-filters)
10. [Q10. What is a dead-letter destination for Event Grid, and when should you configure one?](#q10-what-is-a-dead-letter-destination-for-event-grid-and-when-should-you-configure-one)
11. [Q11. What is the default Event Grid event schema, and what are the key properties in every event envelope?](#q11-what-is-the-default-event-grid-event-schema-and-what-are-the-key-properties-in-every-event-envelope)
12. [Q12. What is CloudEvents, and how does Event Grid support the CloudEvents 1.0 schema?](#q12-what-is-cloudevents-and-how-does-event-grid-support-the-cloudevents-10-schema)
13. [Q13. How do you publish custom application events to an Event Grid custom topic from a .NET application?](#q13-how-do-you-publish-custom-application-events-to-an-event-grid-custom-topic-from-a-net-application)
14. [Q14. What is the difference between `eventType`, `subject`, and `dataVersion` in an Event Grid event?](#q14-what-is-the-difference-between-eventtype-subject-and-dataversion-in-an-event-grid-event)
15. [Q15. How do you evolve event schemas without breaking existing Event Grid subscribers?](#q15-how-do-you-evolve-event-schemas-without-breaking-existing-event-grid-subscribers)
16. [Q16. How does Event Grid deliver events to webhook subscribers, and what HTTP response must the endpoint return?](#q16-how-does-event-grid-deliver-events-to-webhook-subscribers-and-what-http-response-must-the-endpoint-return)
17. [Q17. What is Event Grid's retry policy when delivery to a subscriber fails?](#q17-what-is-event-grids-retry-policy-when-delivery-to-a-subscriber-fails)
18. [Q18. What is the difference between at-least-once delivery in Event Grid and exactly-once event processing?](#q18-what-is-the-difference-between-at-least-once-delivery-in-event-grid-and-exactly-once-event-processing)
19. [Q19. How do you implement idempotent event handlers for Event Grid webhooks in ASP.NET Core?](#q19-how-do-you-implement-idempotent-event-handlers-for-event-grid-webhooks-in-aspnet-core)
20. [Q20. What causes events to be dead-lettered in Event Grid, and how do you replay them?](#q20-what-causes-events-to-be-dead-lettered-in-event-grid-and-how-do-you-replay-them)
21. [Q21. What is the difference between a webhook endpoint and an Azure Function Event Grid trigger?](#q21-what-is-the-difference-between-a-webhook-endpoint-and-an-azure-function-event-grid-trigger)
22. [Q22. How do you secure an Event Grid webhook endpoint in ASP.NET Core?](#q22-how-do-you-secure-an-event-grid-webhook-endpoint-in-aspnet-core)
23. [Q23. How does Event Grid validate a webhook subscription during creation (the validation handshake)?](#q23-how-does-event-grid-validate-a-webhook-subscription-during-creation-the-validation-handshake)
24. [Q24. How do you handle Event Grid subscription validation in an ASP.NET Core API?](#q24-how-do-you-handle-event-grid-subscription-validation-in-an-aspnet-core-api)
25. [Q25. How do you use the `Azure.Messaging.EventGrid` SDK to publish events from a .NET service?](#q25-how-do-you-use-the-azuremessagingeventgrid-sdk-to-publish-events-from-a-net-service)
26. [Q26. What is Event Grid's role in Azure resource change notifications (system events)?](#q26-what-is-event-grids-role-in-azure-resource-change-notifications-system-events)
27. [Q27. What monitoring and troubleshooting practices help diagnose failed Event Grid deliveries in production?](#q27-what-monitoring-and-troubleshooting-practices-help-diagnose-failed-event-grid-deliveries-in-production)

---

## Q1. What is Azure Event Grid, and what problem does it solve in event-driven architecture?

**Concepts**
- Fully managed publish/subscribe event routing service
- Central router vs point-to-point HTTP callbacks or polling
- Publisher/subscriber decoupling in identity and timing
- Reactive workflows for near-real-time loose coupling

**Answer**

Azure Event Grid is a fully managed publish/subscribe event routing service that connects event producers to consumers without those parties knowing about each other. It solves the integration problem in event-driven architecture by letting me react to state changes — in my own application or in Azure resources — through a central router instead of point-to-point HTTP callbacks or polling. Publishers emit events to a topic or domain endpoint; Event Grid matches each event against subscription filters and pushes copies only to subscribers that care about that event type or subject. Because delivery, retry, and fan-out are handled by the service, my application code stays focused on business logic rather than building a custom notification dispatcher. Event Grid fits reactive workflows such as "when a blob is uploaded, start a thumbnail function" or "when an order is placed, notify billing and inventory" where loose coupling and near-real-time notification matter more than guaranteed ordering across all consumers.

---

## Q2. How does Azure Event Grid differ from Azure Service Bus, Azure Event Hubs, and Azure Queue Storage?

**Concepts**
- Event Grid — push-based pub/sub routing for discrete notifications
- Service Bus — brokered messaging with queues and topics
- Event Hubs — partitioned high-throughput event streaming
- Queue Storage — simple durable FIFO job queue
- Consumer model: push vs pull vs partition offset vs dequeue

**Answer**

Event Grid is an event router optimized for discrete notifications and fan-out, while Service Bus is a general-purpose message broker with queues and topics, Event Hubs is a high-throughput event ingestion stream for telemetry, and Queue Storage is a simple durable queue for work items. Event Grid uses push-based delivery — it POSTs events to subscriber endpoints directly. Service Bus consumers pull messages using peek-lock or receive, giving them acknowledgement control. Event Hubs consumers read partitions by offset, which supports replay of a retained event log. Queue Storage workers dequeue messages for background job processing. I choose Event Grid when I need lightweight, many-to-many notification routing; I choose Service Bus or Event Hubs when I need durable messaging semantics, larger payloads, or stream processing at very high volume.

---

## Q3. What is the publish/subscribe model in Event Grid, and how does it decouple event publishers from subscribers?

**Concepts**
- Publisher emits to topic without knowing subscribers
- Subscriber registers interest via subscription filter
- Adding/removing subscribers without redeploying publisher
- Parallel independent reactions from one domain event

**Answer**

In Event Grid's publish/subscribe model, a publisher sends an event to a topic without knowing which applications will react, and each subscriber registers interest through a subscription filter so Event Grid delivers only matching events. The publisher only needs the topic endpoint and permission to publish; it does not maintain a list of downstream services or call them synchronously, which decouples them in both identity and timing. Subscribers declare what they want via filters on event type, subject, or advanced JSON properties, and can be added or removed without redeploying the publisher. Multiple subscribers can receive the same event independently, which supports parallel reactions — send email, update cache, start workflow — from one domain event without the origin service orchestrating each step.

---

## Q4. What types of event sources can Azure Event Grid integrate with out of the box?

**Concepts**
- System topics — Azure resource lifecycle events (Storage, Key Vault, etc.)
- Custom topics and domains — application-defined domain events
- Partner topics — SaaS provider events into your subscription
- Supported handler types — Functions, Logic Apps, webhooks, Service Bus, Event Hubs

**Answer**

Event Grid integrates natively with many Azure services as system event sources and also accepts custom events from my own applications through custom topics and domains. System topics emit resource change events from Azure services such as Storage (blob created/deleted), Key Vault (secret near expiry), App Configuration (setting changed), Resource Groups, and Subscriptions without me operating a separate messaging infrastructure. Custom topics and domains let my .NET microservices, Logic Apps, or third-party systems publish application-level domain events such as `Order.Placed` to endpoints I control. Partner topics extend the same routing model to SaaS providers whose events enter my Azure subscription through registration and activation steps. Event subscriptions can target Azure Functions, Logic Apps, webhooks, Service Bus queues or topics, Event Hubs, hybrid connections, and other supported handler types, so the same event fabric connects Azure-native and custom code.

---

## Q5. What is the difference between push-based and pull-based event delivery, and which model does Event Grid use?

**Concepts**
- Push — service POSTs to subscriber endpoint immediately
- Pull — consumer polls or reads from a buffer when ready
- Event Grid uses push delivery
- Trade-off: low latency vs subscriber availability requirement

**Answer**

Push-based delivery means the messaging service actively sends each event to the subscriber's endpoint as soon as it is routed, while pull-based delivery means the consumer polls or reads from a buffer when it is ready. Event Grid uses push-based delivery — it POSTs events to webhooks or invokes integrated handlers directly. With push, subscribers do not run a polling loop or manage cursor offsets; they expose an HTTP endpoint or use a native Azure trigger and respond when Event Grid delivers. Push reduces latency for reactive scenarios because the handler runs immediately after the event is published, which suits notifications and workflow triggers better than batch polling. The trade-off is that the subscriber must be available and respond within Event Grid's timeout window; failed deliveries enter retry and optional dead-lettering rather than sitting in a queue the consumer reads later.

---

## Q6. What is an Event Grid topic, and how does it differ between a custom topic, a system topic, and a partner topic?

**Concepts**
- Topic — publish endpoint and routing boundary for events
- Custom topic — application-owned, publisher controls events
- System topic — Azure resource provider emits, I only subscribe
- Partner topic — SaaS provider events via registration

**Answer**

An Event Grid topic is the publish endpoint and routing boundary where events are ingested before Event Grid evaluates subscriptions; the three topic kinds differ in who publishes events and what they represent. Custom topics are application-owned endpoints I create for my own domain events, where my .NET service publishes to the topic URL using an access key, SAS token, or Microsoft Entra ID token, and I define subscriptions for each reacting service. System topics are managed automatically for an Azure resource provider such as a Storage account — Azure publishes resource lifecycle events to the system topic, and I only create subscriptions since I do not publish to it from application code. Partner topics receive events from an authorized external SaaS partner into my subscription, with registration and activation steps so only approved partners can send events on my behalf. All three use the same subscription and filter mechanics; the difference is the event source and who controls publishing.

---

## Q7. What is an Event Grid domain, and when would you use a domain instead of multiple separate custom topics?

**Concepts**
- Event Grid domain — management container for thousands of related topics
- Single security and billing boundary with per-topic routing
- Centralized administration vs hundreds of standalone topics
- Tenant isolation within a domain via per-topic endpoints

**Answer**

An Event Grid domain is a management container for up to thousands of related custom topics under one security and billing boundary, with a single publish credential model at the domain level and per-topic routing inside it. I use a domain when a multi-tenant SaaS platform or large solution needs many isolated topics — one per tenant or bounded context — but I want centralized administration, shared access policies, and consistent monitoring instead of provisioning hundreds of standalone custom topics. Each topic inside the domain still has its own endpoint and subscriptions, so tenants or teams remain logically separated while operations teams manage one domain resource. For a small application with one or two event streams, a single custom topic is simpler; domains pay off when topic count, tenant isolation, or uniform governance requirements grow.

---

## Q8. What is an Event Grid subscription, and what endpoint types can it deliver events to?

**Concepts**
- Subscription — binding between a topic and one destination with filters
- Supported endpoint types — webhooks, Functions, Service Bus, Event Hubs
- Per-subscription delivery schema, retry policy, dead-letter, and expiry
- Filter isolation between subscribers on the same topic

**Answer**

An Event Grid subscription binds a topic or system topic scope to one destination handler with optional filters, defining which events are delivered and where they go. Supported endpoint types include HTTPS webhooks, Azure Functions (Event Grid trigger), Azure Logic Apps, Azure Automation webhooks, Azure Service Bus queues or topics, Azure Event Hubs, and other supported destinations depending on scenario and API version. Each subscription can specify filters so the same topic feeds different handlers — for example one subscription for `Order.Placed` to a billing Function and another for `Order.Shipped` to a notification webhook. Subscriptions also carry delivery schema choice (Event Grid schema vs CloudEvents), retry policy, dead-letter configuration, and optional expiration time for temporary integrations.

---

## Q9. How do subscription filters work in Event Grid — subject prefix/suffix, event types, and advanced filters?

**Concepts**
- Included event types — whitelist of eventType strings
- Subject begins with / ends with — hierarchical path filtering
- Advanced filters — JSON field comparisons with AND/OR logic
- No filter on a subscription receives all events from that scope

**Answer**

Event Grid subscriptions use filters so only events matching declared criteria are delivered to that subscriber, reducing noise and letting multiple handlers share one topic safely. Included event types restrict delivery to a list such as `Microsoft.Storage.BlobCreated` for system events or `Order.Placed` for custom events; omitted types are not sent to that subscription. Subject begins with and subject ends with match the `subject` property prefix or suffix — useful when subjects encode hierarchy like `/orders/12345/lines/1` and a handler only cares about `/orders/`. Advanced filters evaluate JSON fields in the event envelope or `data` payload with operators (string contains, number greater than, boolean equals, and others) and support AND/OR logic for fine-grained routing without creating many separate topics. If no filter is configured, the subscription receives all events published to that topic scope, which is appropriate only when a single handler processes every event.

---

## Q10. What is a dead-letter destination for Event Grid, and when should you configure one?

**Concepts**
- Dead-letter — blob container for undeliverable events
- Configured per subscription, not on the topic
- Events dropped silently without dead-letter configuration
- Inspection, fix, and manual replay as the recovery workflow

**Answer**

A dead-letter destination is an Azure Storage blob container where Event Grid stores events that could not be delivered to the subscriber endpoint after retries or that failed with non-retriable errors, so I can inspect, fix, and replay them instead of losing the notification silently. I configure dead-lettering on production subscriptions whenever business-critical reactions such as payments, provisioning, or compliance audit depend on event delivery — the default is to drop undeliverable events if no dead-letter container is set. Events are dead-lettered when maximum delivery attempts or event time-to-live is exceeded, or immediately on certain HTTP status codes such as 400 Bad Request or 413 Payload Too Large that indicate retry will not succeed. Operations teams pull dead-lettered JSON blobs from storage, diagnose handler bugs or misconfiguration, redeploy fixes, and optionally republish or manually process the stored events.

---

## Q11. What is the default Event Grid event schema, and what are the key properties in every event envelope?

**Concepts**
- id — unique event identifier for idempotency checks
- eventType — primary routing key for subscription filters
- subject — hierarchical entity path supporting prefix/suffix filters
- data — publisher-specific payload
- dataVersion — schema version of the data payload

**Answer**

The default Event Grid schema wraps each notification in a JSON envelope with standard metadata fields separate from my business payload in `data`, so routing and filtering work consistently across Azure and custom events. The `id` is a unique identifier for this event occurrence, which I use for idempotency checks in handlers. The `topic` is the resource path of the topic that received the event, identifying the publishing scope. The `subject` is a path-like string describing the entity the event refers to such as a blob URL or `/orders/12345`. The `eventType` names what happened such as `Microsoft.Storage.BlobCreated` or `Order.Placed` and is the primary routing key for subscriptions. The `eventTime` is the UTC timestamp when the event occurred. The `data` object is the publisher-specific payload whose size and shape are defined by the event type. The `dataVersion` is the schema version of the `data` payload so consumers can deserialize the correct shape. Webhooks often receive an array of events in one HTTP POST when batching is enabled, so handlers should iterate the array rather than assume a single object.

---

## Q12. What is CloudEvents, and how does Event Grid support the CloudEvents 1.0 schema?

**Concepts**
- CloudEvents — CNCF vendor-neutral event metadata specification
- Event Grid delivery schema choice per subscription
- CloudEvents attribute mapping — id, source, type, time, data
- Portability across cloud providers vs Azure-native schema

**Answer**

CloudEvents is a vendor-neutral specification (CNCF standard) for describing event metadata in a consistent way across platforms, and Event Grid can deliver events using the CloudEvents 1.0 schema instead of the native Event Grid envelope. When I set a subscription's delivery schema to CloudEvents, Event Grid maps its fields to CloudEvents attributes such as `id`, `source`, `type`, `time`, and `data`, which improves portability if the same handler consumes events from multiple cloud providers or on-premises systems. Publishing to custom topics can also use CloudEvents format so upstream producers already emitting CloudEvents do not need a custom adapter layer. The underlying routing, filtering, and retry behavior is identical; only the on-the-wire JSON shape changes, so I choose CloudEvents when interoperability is a requirement and Event Grid schema when integrating primarily with Azure-native tooling.

---

## Q13. How do you publish custom application events to an Event Grid custom topic from a .NET application?

**Concepts**
- EventGridPublisherClient from Azure.Messaging.EventGrid package
- AzureKeyCredential, AzureSasCredential, or DefaultAzureCredential
- Required event fields — id, subject, eventType, dataVersion, data
- Fire-and-forget from the publisher; Event Grid owns fan-out and retry

**Answer**

From .NET I use the `Azure.Messaging.EventGrid` client library to construct `EventGridEvent` instances and send them to the custom topic's HTTPS endpoint, authenticating with a topic key, SAS token, or Microsoft Entra credential. I register the NuGet package `Azure.Messaging.EventGrid`, create an `EventGridPublisherClient` with the topic endpoint URI and credential, and call `SendEventAsync` or `SendEventsAsync` with one or more events. Each event needs at minimum a distinct `id`, a meaningful `subject`, an `eventType` my subscribers filter on, and a serializable `data` object representing the domain occurrence. Publishing is fire-and-forget from the application's perspective — Event Grid accepts the batch and takes responsibility for fan-out, retry, and dead-lettering to subscribers — so my service should still handle publish failures such as network or auth errors and optionally use the outbox pattern if the event must align with a database commit.

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

**Concepts**
- eventType — verb/category, primary routing key for filters
- subject — noun/instance path, supports prefix/suffix subscription filters
- dataVersion — schema version of the data payload for consumer evolution

**Answer**

These three fields serve different roles: `eventType` names what happened, `subject` identifies which entity it happened to, and `dataVersion` declares which shape of the payload the publisher used. `eventType` is the primary routing key for subscriptions — handlers subscribe to types like `Order.Placed` or `Microsoft.Storage.BlobCreated` rather than inspecting payload details first. `subject` provides a hierarchical locator for the affected resource such as an order ID, blob path, or user ID, and it supports prefix/suffix filters so one handler processes all events under `/orders/` while another handles `/inventory/`. `dataVersion` lets me evolve the JSON inside `data` — when I add fields or restructure, I increment the version so new subscribers deserialize the updated shape while old handlers can remain on v1 filters until migrated. Confusing `eventType` with `subject` leads to overly broad subscriptions or filters that miss events; I treat type as verb/category and subject as noun/instance path.

---

## Q15. How do you evolve event schemas without breaking existing Event Grid subscribers?

**Concepts**
- Additive changes — add optional fields, existing consumers ignore them
- Breaking changes — new eventType or bumped dataVersion
- Dual publishing during migration to run old and new in parallel
- Event Grid does not enforce schema validation at publish time

**Answer**

I treat event contracts as versioned interfaces and prefer additive changes — adding optional JSON properties to `data` with defaults implied in handler code — since existing subscribers ignore unknown fields and keep working. For breaking changes such as restructuring the payload or changing required fields, I publish under a new `eventType` such as `Order.Placed.v2` or bump `dataVersion` and create new subscriptions pointed at updated handlers while old subscriptions drain on the previous version. During migration I temporarily emit both v1 and v2 events or run parallel subscriptions until all consumers upgrade, which allows a zero-downtime transition. Event Grid does not enforce schema validation at publish time the way some schema registries do for Kafka, so I document the contract as a JSON schema and coordinate with teams owning downstream Functions and webhooks before making breaking changes.

---

## Q16. How does Event Grid deliver events to webhook subscribers, and what HTTP response must the endpoint return?

**Concepts**
- HTTP POST with JSON event array to the subscriber URL
- 2xx response required within the 30-second timeout
- Acknowledge quickly, process asynchronously if needed
- 400 Bad Request triggers immediate dead-lettering, not retry

**Answer**

Event Grid delivers events by sending an HTTP POST with a JSON body to the subscriber URL, and the endpoint must respond with a 2xx status code within the service timeout so Event Grid marks the delivery successful. The default timeout is 30 seconds for the webhook to return a response after receiving the POST; slow handlers should acknowledge quickly and process asynchronously if needed, though I must return success only after I accept responsibility for the event or have safely queued it. Any non-2xx response or timeout causes Event Grid to schedule a retry according to the subscription retry policy rather than dropping the event immediately. Handlers should parse the body as an array of events, return 400 only when the payload is permanently invalid — which can trigger immediate dead-lettering — and avoid returning 5xx for business validation failures that will fail again on retry unless I intend to dead-letter after exhaustion.

---

## Q17. What is Event Grid's retry policy when delivery to a subscriber fails?

**Concepts**
- Exponential backoff retries on retriable errors and timeouts
- Configurable max delivery attempts (1–30) and event TTL (1–1440 minutes)
- Non-retriable HTTP codes — 400, 413 — skip retries immediately
- First limit reached (TTL or attempts) stops delivery

**Answer**

When delivery fails with a retriable error or timeout, Event Grid retries with exponential backoff until either the configured maximum delivery attempts or event time-to-live is reached, whichever limit is hit first. The default schedule uses increasing delays — approximately 30 seconds, 1 minute, 5 minutes, and then continued retries — up to the policy limits. I can customize max delivery attempts (1–30, default 30) and event time-to-live in minutes (1–1440, default 1440) per subscription via the Azure portal, Azure CLI, REST, or infrastructure-as-code. Non-retriable responses such as 400 Bad Request or 413 Request Entity Too Large skip further retries and move toward dead-lettering immediately because repeating the same request will not succeed. If both TTL and attempt limits are set, the first limit reached stops delivery — for example a short TTL may dead-letter before all attempt slots are used.

---

## Q18. What is the difference between at-least-once delivery in Event Grid and exactly-once event processing?

**Concepts**
- At-least-once — Event Grid may redeliver the same event id
- Idempotent handler — processing same id twice yields same result
- Exactly-once processing requires deduplication store or natural idempotency
- Event Grid does not deduplicate on behalf of subscribers

**Answer**

Event Grid guarantees at-least-once delivery to subscribers, meaning an event may arrive more than once if a handler returns success slowly, crashes after processing, or a network failure causes ambiguity — it does not guarantee exactly-once end-to-end processing. At-least-once at the transport layer means Event Grid may redeliver the same `id` until it receives a successful acknowledgement or exhausts retries. Exactly-once processing is an application goal: my handler must be idempotent, meaning processing the same event `id` twice produces the same final state as processing it once, typically by storing processed IDs or using natural keys in the database. Exactly-once semantics across publish, route, and process require patterns such as idempotent consumers, deduplication stores, or transactional outbox — Event Grid alone does not deduplicate on behalf of subscribers.

---

## Q19. How do you implement idempotent event handlers for Event Grid webhooks in ASP.NET Core?

**Concepts**
- Deduplication store keyed on event id
- Check-before-process and mark-after-process pattern
- Return 200 immediately for already-processed events
- Atomic write of side effect and idempotency record

**Answer**

An idempotent Event Grid handler records each event's `id` before or atomically with side effects, so duplicate deliveries become no-ops instead of double charges or duplicate records. On receipt, I check a durable store — SQL table, Redis, or Azure Table Storage — for the event `id`; if present, I return 200 OK immediately without re-running business logic. If absent, I perform the side effect and insert the `id` in the same transaction as the business write when possible, so a crash between processing and acknowledgement does not leave inconsistent state. I use the event `id` from the envelope, not only HTTP request correlation, because retries resend the same event payload with the same `id`. I return 200 only after the event is safely recorded or processed; returning success before persistence causes Event Grid to stop retrying while my handler may not have finished work.

```csharp
if (await _processedEvents.ExistsAsync(evt.Id))
    return Ok();

await _orderService.PlaceOrderAsync(evt.Data);
await _processedEvents.MarkAsync(evt.Id);
return Ok();
```

---

## Q20. What causes events to be dead-lettered in Event Grid, and how do you replay them?

**Concepts**
- Causes — exhausted retries, TTL exceeded, non-retriable HTTP errors
- Dead-letter blobs in Storage with failure metadata
- Replay — republish data to topic or invoke handler directly
- No automatic replay button; operations owns re-drive

**Answer**

Events are dead-lettered when Event Grid cannot deliver them successfully within the subscription retry limits, when event time-to-live expires, or when the endpoint returns immediate non-retriable HTTP errors. I inspect dead-lettered events as JSON blobs in the configured Storage container, where each blob includes metadata about the failure reason and delivery attempts. To replay, I fix the root cause such as a handler bug, auth misconfiguration, or filter mismatch, then either republish the `data` to the topic using the Event Grid publisher client or invoke the handler directly with the stored payload. There is no automatic replay button — operations owns the re-drive process. I prevent recurring dead-letter volume with monitoring alerts on dead-letter blob count and Application Insights logging inside webhook handlers.

---

## Q21. What is the difference between a webhook endpoint and an Azure Function Event Grid trigger?

**Concepts**
- Webhook — general HTTPS endpoint I implement (ASP.NET Core, etc.)
- Azure Function trigger — managed binding with automatic scaling
- Subscription validation handshake — manual in ASP.NET Core, auto in Functions
- Control vs simplicity trade-off

**Answer**

Both receive Event Grid events, but a webhook is a general HTTPS endpoint I implement in ASP.NET Core or any other server while an Azure Function Event Grid trigger is a managed binding that invokes my function code with deserialized events and integrated scaling. With a webhook I have full control over the HTTP middleware pipeline, auth scheme, and routing, and I must implement the subscription validation handshake myself. With an Azure Function trigger the Functions runtime handles the validation handshake for supported versions, auto-scales on Consumption or Premium plan per invocation, and requires minimal HTTP ceremony. I choose webhooks when the handler lives in an existing ASP.NET Core service or I need full control over the HTTP pipeline; I choose Functions when I want minimal boilerplate and serverless scaling for isolated event reactions.

---

## Q22. How do you secure an Event Grid webhook endpoint in ASP.NET Core?

**Concepts**
- SubscriptionValidation event handshake proves endpoint ownership
- Entra ID bearer token or shared secret for ongoing delivery
- IP filtering and HTTPS-only for network restriction
- [Authorize] policy on the Event Grid route

**Answer**

I secure Event Grid webhooks by validating that requests genuinely originate from Event Grid, restricting network access where possible, and treating the endpoint as an authenticated integration point rather than a public anonymous API. During subscription creation, Event Grid sends a SubscriptionValidation event; my API must echo the `validationCode` in the response body so only someone who can receive that handshake completes registration. For ongoing delivery I configure Microsoft Entra ID authentication on the webhook so Event Grid presents a bearer token my API validates, or I use a shared secret model with a query parameter or header known to both sides when Entra is not available. I restrict ingress with IP filtering or private endpoints where my architecture allows, enable HTTPS only, and apply authorization policies with `[Authorize]` on the Event Grid route.

---

## Q23. How does Event Grid validate a webhook subscription during creation (the validation handshake)?

**Concepts**
- SubscriptionValidationEvent POST to the webhook URL on subscription creation
- validationCode in data, validationResponse in the reply
- HTTP 200 with validationResponse JSON required synchronously
- validationUrl for manual validation if automatic fails

**Answer**

When I create an event subscription pointing at an HTTPS webhook, Event Grid immediately POSTs a special `Microsoft.EventGrid.SubscriptionValidationEvent` to that URL and expects a JSON response containing the `validationCode` from the event data, proving I control the endpoint before any real events flow. The validation request looks like a normal event batch but `eventType` is `Microsoft.EventGrid.SubscriptionValidationEvent` and `data` includes `validationCode` and optionally `validationUrl` for manual validation scenarios. My endpoint must respond with HTTP 200 and body `{ "validationResponse": "<validationCode>" }` synchronously during subscription creation; until this succeeds, the subscription stays in a failed state. If automatic validation fails due to firewall or wrong URL, I can complete validation manually via the `validationUrl` link from a browser or HTTP client. Handlers must branch on validation events before business logic — treating validation as an unknown event type returns failed provisioning and blocks delivery.

---

## Q24. How do you handle Event Grid subscription validation in an ASP.NET Core API?

**Concepts**
- Deserialize event array, detect SubscriptionValidationEvent by type
- Return Ok with validationResponse before running business handlers
- ngrok or Azure Relay for local development handshake testing
- Same endpoint handles validation and normal events

**Answer**

In ASP.NET Core I deserialize the incoming event array, detect `Microsoft.EventGrid.SubscriptionValidationEvent`, and return `Ok(new { validationResponse = validationCode })` without running business handlers — typically in a dedicated minimal API route or controller action. I use `System.Text.Json` or the `Azure.Messaging.EventGrid` models to parse the batch; the validation event is always the first concern when the subscription is new. After validation succeeds, the same endpoint receives normal events and routes by `eventType` to application services. For local development I use ngrok or Azure Relay so Event Grid can reach localhost during handshake testing, since without a public HTTPS URL subscription creation fails validation.

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

**Concepts**
- EventGridPublisherClient with topic endpoint and credential
- AzureKeyCredential for dev/test, DefaultAzureCredential for production
- SendEventsAsync for batching efficiency
- RequestFailedException handling for throttling and auth errors

**Answer**

The `Azure.Messaging.EventGrid` package provides `EventGridPublisherClient` for sending events to a custom topic or domain topic endpoint with credential options matching how I secure the topic in Azure. I add the NuGet package, configure the topic HTTPS endpoint from configuration, and choose `AzureKeyCredential` for development or `DefaultAzureCredential` for managed identity in production. I build `EventGridEvent` objects with unique `id`, `subject`, `eventType`, `dataVersion`, and serializable `data`, and use `SendEventsAsync` to batch multiple events for efficiency. I handle `RequestFailedException` for throttling or auth errors since publishing is synchronous over HTTP to Event Grid's ingress. In ASP.NET Core I register the client as a singleton via `AddSingleton<EventGridPublisherClient>` and inject it into application services that raise domain events after successful commands, preferring managed identity over embedded access keys in deployed environments.

---

## Q26. What is Event Grid's role in Azure resource change notifications (system events)?

**Concepts**
- System topics turning Azure resource lifecycle into push notifications
- Supported sources — Storage, Key Vault, App Service, Azure Subscription
- Event subscriptions route to Functions, Logic Apps, or webhooks
- Event-driven automation vs scheduled polling

**Answer**

Event Grid system topics turn Azure resource lifecycle and data-plane operations into push notifications so my automation reacts to infrastructure and data changes without polling the Resource Manager or Storage APIs. When I enable a system topic on a resource such as Storage, Key Vault, App Service, or an Azure Subscription, Azure automatically publishes well-defined event types such as blob creation, secret near expiry, or resource write success to that system topic. I create event subscriptions on the system topic to route those events to Functions, Logic Apps, or webhooks — typical uses include triggering backup workflows, cache invalidation, security auditing, and infrastructure drift remediation. System events use Microsoft's published schemas documented per resource provider, where `eventType` and `data` shapes are stable and versioned by Microsoft. This model is central to event-driven automation in Azure: reactive DevOps and application logic wired to platform signals instead of scheduled scans.

---

## Q27. What monitoring and troubleshooting practices help diagnose failed Event Grid deliveries in production?

**Concepts**
- Topic Metrics for publish success, delivery failure, and dead-lettered counts
- Diagnostic settings to Log Analytics for query-based investigation
- Dead-letter blob inspection for failure metadata
- Correlating subscriber logs with Event Grid logs by event id
- Filter misconfiguration as silent drops vs actual delivery failures

**Answer**

Production Event Grid troubleshooting combines subscription metrics, delivery failure logs, dead-letter blob inspection, and correlated application logging in the subscriber so I can see whether failure happened at routing, transport, or handler logic. In the Azure Portal I open the topic or domain Metrics for publish success, delivery success/failure counts, and dead-lettered events, and I set alerts when failure rate or dead-letter count exceeds baseline. I enable diagnostic settings to send Event Grid operational logs to Log Analytics for querying failed deliveries, latency, and subscription provisioning errors. I inspect the dead-letter container for undelivered event JSON and HTTP response details recorded by Event Grid when dead-lettering is enabled. In the subscriber I log event `id`, `eventType`, and `subject` with correlation IDs and compare with Event Grid logs to distinguish timeout, 401/403 auth misconfiguration, validation mishandling, and unhandled exceptions. I reproduce with Cloud Shell or REST API to republish a test event after fixes, and I verify subscription filter rules if handlers never receive expected types since misconfigured filters look like delivery failures but are silent drops at routing time.

---

## Gotchas — Azure Event Grid (Interview Traps)

---

#### Gotcha 1. Dead-lettering is not enabled by default — failed event deliveries are silently dropped after retry exhaustion

**Concepts**
- Event Grid retries delivery for up to 24 hours using exponential backoff
- After retry exhaustion, events are permanently lost unless a dead-letter destination is configured
- Dead-letter destination requires a storage account with a container and a SAS URI
- Monitoring retry metrics is the only way to detect lost events without dead-lettering

**Answer**

Azure Event Grid does not dead-letter failed events automatically. Without a dead-letter destination configured on the event subscription, events that exhaust the retry policy (24 hours of retries by default) are permanently discarded with no notification. Configuring dead-lettering requires adding a storage account container as the destination and providing a SAS URI with write permission to the container. Systems that rely on guaranteed event processing must configure dead-lettering and monitor the dead-letter container; otherwise a subscriber outage during the 24-hour retry window causes silent data loss with no recovery path.

---

#### Gotcha 2. Event Grid uses push delivery — a subscriber that is down misses events if the retry window expires

**Concepts**
- Event Grid pushes events to HTTP webhook endpoints; it does not queue for pull
- A subscriber down for more than 24 hours loses all events delivered during that outage
- Service Bus provides guaranteed pull-based delivery without time-based expiry
- Event Grid → Service Bus integration routes events to a queue for durable pull consumption

**Answer**

Unlike Service Bus, which stores messages indefinitely until a consumer retrieves them, Event Grid pushes events to registered webhook endpoints and retries for a maximum of 24 hours. A subscriber (HTTP endpoint) that is unavailable for more than 24 hours will lose all events delivered during that outage, even after it comes back online. For workloads requiring guaranteed delivery independent of subscriber availability windows, the correct pattern is to route Event Grid events to a Service Bus queue (using an Event Grid → Service Bus topic subscription), turning push-based Event Grid delivery into pull-based durable consumption.

---

#### Gotcha 3. Event Grid schema and CloudEvents schema are not interchangeable — the schema choice per topic is permanent

**Concepts**
- Event Grid supports two schemas: Event Grid schema (proprietary) and CloudEvents 1.0
- The schema is chosen per topic at creation and cannot be changed after creation
- Publishers and subscribers must use the same schema type
- CloudEvents is the recommended standard for new topics due to its portability

**Answer**

When you create an Event Grid topic, you choose either the proprietary Event Grid schema or the CloudEvents 1.0 standard schema. This choice is permanent; you cannot migrate an existing topic between schemas without recreating it and migrating all subscriptions. Publishers that produce Event Grid schema events cannot send to a CloudEvents topic without reformatting the event payload. For organizations with multiple teams publishing to shared topics, using different schemas across topics creates friction when consumers need to handle events from multiple sources.

---

#### Gotcha 4. Dead-letter SAS token expiry silently stops dead-lettering without error

**Concepts**
- The dead-letter destination is configured with a SAS URI that includes an expiry
- When the SAS URI expires, Event Grid cannot write to the dead-letter container
- Expired SAS on the dead-letter destination causes silently lost events (they are dropped instead of dead-lettered)
- Rotating or using a managed identity-based connection for dead-lettering prevents this

**Answer**

The dead-letter destination for an Event Grid subscription is a storage container referenced via a SAS URI. When that SAS token expires, Event Grid loses write access to the container and falls back to dropping failed events instead of dead-lettering them. There is no alert or error message when this happens; the dead-letter container simply stops receiving new files. Teams often set the SAS expiry once during provisioning and forget about it, discovering months later that all events during a subscriber outage were silently dropped. Using a system-assigned managed identity for the Event Grid subscription instead of a SAS URI removes the expiry concern.

---

#### Gotcha 5. Event Grid has no message lock — a 200 response after a crash causes silent event loss

**Concepts**
- Event Grid acks an event delivery the moment the subscriber endpoint returns 200
- If the application crashes or rolls back after returning 200, the event is not redelivered
- Service Bus message locking provides at-least-once semantics with redelivery on crash
- Idempotent consumers and transactional outbox patterns address this gap

**Answer**

Event Grid considers an event successfully delivered as soon as the subscriber endpoint returns HTTP 200–204. If the application processes the event, flushes the HTTP response buffer returning 200, and then crashes before committing the side effect (for example a database write), Event Grid treats the delivery as successful and does not redeliver. This is fundamentally different from Service Bus, where the message lock must be explicitly completed after processing and a crash before completion causes automatic redelivery. Event Grid provides at-least-once delivery in the sense that retries occur on non-2xx responses, but it cannot protect against successes that precede crashes.

---

#### Gotcha 6. Webhook subscriber endpoints must handle the validation handshake — endpoints that ignore it are rejected

**Concepts**
- Event Grid sends a `SubscriptionValidationEvent` to new webhook endpoints before delivering events
- The endpoint must respond with the `validationCode` from the payload within 30 seconds
- An HTTPS endpoint that returns 404 or ignores the validation event cannot be registered
- Azure Functions and Logic Apps handle validation automatically; custom APIs must implement it

**Answer**

When you create an Event Grid webhook subscription, Event Grid immediately sends a `SubscriptionValidationEvent` to the subscriber endpoint. The endpoint must respond with HTTP 200 and a JSON body containing `{"validationResponse": "<validationCode>"}` from the event payload within 30 seconds, or the subscription creation fails. A custom ASP.NET Core API that does not have this validation handling implemented returns 404 or an unrecognized response, causing the subscription to be rejected. Azure Functions HTTP triggers and Azure Logic Apps handle this validation automatically, but any manually written webhook endpoint requires explicit handling of the `Microsoft.EventGrid.SubscriptionValidationEvent` event type.

---

#### Gotcha 7. Event filters support only prefix/suffix and exact string matching — complex routing requires downstream logic

**Concepts**
- Event Grid subscription filters can match on event type, subject prefix, and subject suffix
- Advanced filters support comparison operators on event data properties but not regex
- Complex routing rules (contains, regex, multi-field logic) cannot be expressed in filters
- Azure Functions or Logic Apps are used as filter proxies for complex routing

**Answer**

Azure Event Grid subscription filters support exact match on event type, subject prefix, subject suffix, and advanced filters using comparison operators (equals, ends with, begins with, contains, etc.) on specific event data fields. However, regex matching, conditional multi-field logic, and nested property filtering are not supported. When routing logic requires more expressiveness — for example routing based on whether an event body contains a specific string pattern or on the combination of multiple property values — a subscriber Azure Function acts as a routing proxy: it receives all events, applies the complex logic, and forward-publishes to downstream systems or discards the event.

---

#### Gotcha 8. System topics for Blob Storage can only be created once per storage account per region — duplicate creation fails

**Concepts**
- A system topic links one Azure resource (e.g., storage account) to Event Grid
- Only one system topic per storage account per subscription is allowed
- Creating a second system topic for the same storage account returns a conflict error
- Multiple subscriptions can be added to the same system topic to route to multiple consumers

**Answer**

Azure Event Grid system topics for a given source resource (such as a storage account) can only be created once per Azure subscription and region. If a system topic already exists for a storage account and a pipeline or ARM template tries to create a second one, the operation fails with a conflict error. This often happens when Bicep or Terraform templates are not idempotent — they try to recreate the system topic on every deployment. The solution is to deploy the system topic with a `if not exists` guard in the template, or to reference the existing system topic name and add new subscriptions to it rather than recreating the topic.

---

#### Gotcha 9. Maximum event size is 1 MB — an oversized event in a batch causes the entire batch to be rejected

**Concepts**
- Individual events must not exceed 1 MB including headers and metadata
- A batch that contains even one event exceeding 1 MB is rejected entirely
- The rejection is HTTP 413 (Payload Too Large) and no events in the batch are delivered
- Large payloads should use the claim-check pattern (store payload in Blob, send reference)

**Answer**

Azure Event Grid enforces a 1 MB maximum size per event. When publishing a batch of events, if any single event in the batch exceeds 1 MB, the entire batch is rejected with HTTP 413 and none of the events — including the valid smaller ones — are delivered. This is a silent all-or-nothing failure for callers who do not inspect the response code. The claim-check pattern addresses this: the large payload is written to Azure Blob Storage and the Event Grid event contains only the blob URL and metadata, keeping the event size well under the limit.

---

#### Gotcha 10. Webhook subscribers must respond within 30 seconds — synchronous long processing causes 408 and retry flood

**Concepts**
- Event Grid expects HTTP 200–204 within 30 seconds of delivering an event
- A slow synchronous handler that processes inline causes a 408 timeout
- Event Grid retries the delivery on timeout, causing duplicate processing
- The correct pattern is to acknowledge immediately and process asynchronously via a queue

**Answer**

Event Grid expects the subscriber endpoint to return a 2xx HTTP response within 30 seconds of receiving the event. An ASP.NET Core action handler that performs synchronous long-running work — database writes, external API calls, file processing — inline before returning will time out after 30 seconds, causing Event Grid to treat the delivery as failed and schedule a retry. This produces a flood of duplicate deliveries as Event Grid continuously retries the slow endpoint. The correct pattern is to respond immediately with HTTP 202 Accepted, enqueue the work item to Azure Service Bus or an in-process background queue, and process asynchronously outside the request handler.

---
