# Interview Questions — Distributed Messaging — Interview Q&A
> 12 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is a message broker, and why do microservices use asynchronous messaging instead of direct HTTP calls between services?](#q1-what-is-a-message-broker-and-why-do-microservices-use-asynchronous-messaging-instead-of-direct-http-calls-between-services)
2. [Q2. What is the difference between at-most-once, at-least-once, and exactly-once delivery semantics in messaging?](#q2-what-is-the-difference-between-at-most-once-at-least-once-and-exactly-once-delivery-semantics-in-messaging)
3. [Q3. How do point-to-point queues differ from publish/subscribe topics, and when would you use each pattern?](#q3-how-do-point-to-point-queues-differ-from-publishsubscribe-topics-and-when-would-you-use-each-pattern)
4. [Q4. What are RabbitMQ exchanges, and how do direct, fanout, topic, and headers exchanges route messages differently?](#q4-what-are-rabbitmq-exchanges-and-how-do-direct-fanout-topic-and-headers-exchanges-route-messages-differently)
5. [Q5. How do consumer acknowledgements work in RabbitMQ, and what is a dead-letter queue used for?](#q5-how-do-consumer-acknowledgements-work-in-rabbitmq-and-what-is-a-dead-letter-queue-used-for)
6. [Q6. How does Azure Service Bus differ from RabbitMQ, and when would you choose one over the other?](#q6-how-does-azure-service-bus-differ-from-rabbitmq-and-when-would-you-choose-one-over-the-other)
7. [Q7. What are Kafka partitions and consumer groups, and how does Kafka achieve ordered and scalable message consumption?](#q7-what-are-kafka-partitions-and-consumer-groups-and-how-does-kafka-achieve-ordered-and-scalable-message-consumption)
8. [Q8. What is MassTransit, and what does it add over using a raw broker client library directly?](#q8-what-is-masstransit-and-what-does-it-add-over-using-a-raw-broker-client-library-directly)
9. [Q9. How do you implement consumer idempotency — what happens if the same message is delivered and processed more than once?](#q9-how-do-you-implement-consumer-idempotency-what-happens-if-the-same-message-is-delivered-and-processed-more-than-once)
10. [Q10. What is the competing consumers pattern, and what trade-offs does it introduce for message ordering?](#q10-what-is-the-competing-consumers-pattern-and-what-trade-offs-does-it-introduce-for-message-ordering)
11. [Q11. How do you evolve message schemas without breaking existing consumers — what compatibility strategies are available?](#q11-how-do-you-evolve-message-schemas-without-breaking-existing-consumers-what-compatibility-strategies-are-available)
12. [Q12. How does the transactional outbox pattern solve the dual-write problem between a database and a message broker?](#q12-how-does-the-transactional-outbox-pattern-solve-the-dual-write-problem-between-a-database-and-a-message-broker)

---

## Q1. What is a message broker, and why do microservices use asynchronous messaging instead of direct HTTP calls between services?

**Concepts**
- Message broker as temporal and spatial decoupler
- Load leveling — absorbing traffic spikes without overwhelming downstream services
- Automatic fault tolerance through broker-side message persistence
- Pub/sub fan-out to multiple consumers without producer awareness
- Temporal coupling as the core liability of synchronous HTTP

**Answer**

A message broker is middleware that stores messages from producers until consumers retrieve and process them, so neither side needs to be online at the same time or know the other's address. The primary reason microservices prefer this over synchronous HTTP is the elimination of temporal coupling: an HTTP call requires both parties to be healthy at the exact moment of the call, which means a consumer restart or deployment window directly causes producer failures. Because the broker acts as a durable intermediary, producer and consumer lifecycles are fully independent. Brokers also provide load leveling — when traffic spikes, messages queue up and consumers work through them at a safe pace rather than crashing the downstream service with a sudden flood of requests. If a consumer crashes mid-processing, messages remain safely in the broker and are re-delivered when it restarts, giving automatic fault tolerance that HTTP retries alone cannot provide. A single published message can also reach multiple independent services through pub/sub patterns, so the producer does not need to know how many downstream services care about the event.

---

## Q2. What is the difference between at-most-once, at-least-once, and exactly-once delivery semantics in messaging?

**Concepts**
- At-most-once — message loss over duplication
- At-least-once — duplicates handled by idempotent consumers
- Exactly-once — two-phase coordination across producer, broker, and consumer
- Kafka idempotent producers and transactional APIs approximating exactly-once
- Practical preference: at-least-once with consumer-side idempotency

**Answer**

Delivery semantics describe how many times a messaging system guarantees a message will be delivered. At-most-once means the broker sends the message once and does not retry on failure — simple and fast, but acceptable only where occasional loss is tolerable, such as high-volume telemetry. At-least-once means the broker retries until it receives a confirmation, which guarantees delivery but may cause the consumer to process the same message more than once after a crash or network failure, so the consumer must be idempotent. Exactly-once guarantees no duplicates and no losses, but it requires two-phase coordination between producer, broker, and consumer, which is expensive and difficult to implement correctly across distributed systems — Kafka provides idempotent producers and transactional APIs that approximate exactly-once, but only end-to-end when both the source and sink participate in the protocol. In practice, at-least-once with consumer-side idempotency is the most pragmatic choice because it provides strong durability without the coordination overhead, and designing a consumer to be idempotent is generally straightforward.

---

## Q3. How do point-to-point queues differ from publish/subscribe topics, and when would you use each pattern?

**Concepts**
- Point-to-point queue — each message consumed by exactly one receiver
- Pub/sub topic — every subscriber receives an independent copy
- Competing consumers for horizontal throughput scaling
- Fan-out to multiple independent services without producer awareness
- Kafka consumer groups replicating point-to-point behavior

**Answer**

In a point-to-point queue, multiple consumers compete for messages but each message is delivered to exactly one of them — the right model when the message represents a unit of work that only one worker should perform, such as processing a payment or generating an invoice. In publish/subscribe, a single published message is delivered to every subscriber independently so each receives its own copy — the right model when a state change needs to be observed by multiple independent services, such as an `OrderPlaced` event that billing, inventory, and email services each need to act on separately. The core question is whether the message represents work to be done once or an event that many parties need to know about. Azure Service Bus and RabbitMQ support both patterns natively. Kafka is fundamentally topic-based but replicates point-to-point behavior through consumer group assignment, where each partition is assigned to exactly one consumer in the group at a time.

---

## Q4. What are RabbitMQ exchanges, and how do direct, fanout, topic, and headers exchanges route messages differently?

**Concepts**
- RabbitMQ exchange as the routing layer between producers and queues
- Direct exchange — exact routing key match
- Fanout exchange — broadcasts to all bound queues regardless of key
- Topic exchange — wildcard pattern matching with `*` and `#`
- Headers exchange — routes on message header attributes

**Answer**

In RabbitMQ, producers never send messages directly to a queue — they publish to an exchange, which routes messages to one or more queues based on rules called bindings. The exchange type determines the routing algorithm, so choosing correctly is foundational to the topology. A direct exchange routes to every queue whose binding key exactly matches the message's routing key, which makes it the right choice for command routing where a specific worker type should receive the message. A fanout exchange ignores the routing key entirely and broadcasts the message to all bound queues, making it ideal for pub/sub scenarios where every subscriber needs the same event. A topic exchange uses pattern matching on the routing key with `*` (matching one word) and `#` (matching zero or more words), so a message with routing key `orders.europe.express` can match both an `orders.europe.*` queue and an `orders.#` queue simultaneously — enabling flexible multi-category routing that neither direct nor fanout can express. A headers exchange routes based on message header attributes rather than the routing key, which allows routing on multiple arbitrary properties but is rarely used because topic exchanges cover most real-world needs more simply.

---

## Q5. How do consumer acknowledgements work in RabbitMQ, and what is a dead-letter queue used for?

**Concepts**
- Unacknowledged message state until consumer sends ack
- Re-queuing on channel close before ack — at-least-once guarantee
- Negative acknowledgement (nack) with discard vs. re-queue choice
- Dead-letter exchange routing unprocessable messages to a DLQ
- Message TTL and max-length for robust retry and poison-message topology

**Answer**

When RabbitMQ delivers a message to a consumer, the message stays in an unacknowledged state until the consumer explicitly sends an acknowledgement back to the broker. If the consumer's channel closes before the ack arrives — because of a crash or unhandled exception — RabbitMQ re-queues the message and delivers it to the next available consumer, which is how at-least-once delivery is guaranteed without any producer-side retry logic. A negative acknowledgement (nack) lets a consumer explicitly signal failure, with a flag to either re-queue the message for another attempt or discard it. When a message is nacked with `requeue=false`, or when it exceeds a configured retry count, RabbitMQ routes it to the dead-letter exchange (DLX), which delivers it to a dead-letter queue (DLQ). The DLQ is a holding area for messages that could not be processed successfully — rather than silently dropping them, they are preserved for manual inspection, alerting, or offline reprocessing. Combining `x-message-ttl`, `x-max-length`, and DLX configuration on the source queue creates a retry and poison-message handling topology without any custom broker-side code.

---

## Q6. How does Azure Service Bus differ from RabbitMQ, and when would you choose one over the other?

**Concepts**
- Azure Service Bus as fully managed PaaS — zero operational overhead
- RabbitMQ as self-managed open-source broker with protocol flexibility
- Message sessions for per-key ordered FIFO delivery in Service Bus
- Native scheduled delivery and geo-disaster recovery in Service Bus premium tier
- RabbitMQ protocol support — AMQP 0-9-1, MQTT, STOMP

**Answer**

Azure Service Bus is a fully managed cloud broker provided as PaaS on Azure, while RabbitMQ is an open-source broker you deploy and manage yourself. Both support queues and pub/sub topics, but the operational model and feature set differ enough that the choice usually comes down to cloud strategy and which advanced capabilities the workload needs. Service Bus eliminates infrastructure management entirely and ships with features that RabbitMQ lacks natively: message sessions guarantee ordered FIFO delivery per session key such as a customer ID, native scheduled delivery allows sending a message now to be delivered at a future time, and built-in active/passive geo-disaster recovery is available on the premium tier. The maximum message size also differs — RabbitMQ supports up to 128 MB with streams, while Service Bus is limited to 256 KB on standard tiers and 100 MB on premium. I would choose RabbitMQ when the workload needs to run on-premises or across multiple clouds, requires MQTT for IoT or STOMP for legacy systems, or when vendor lock-in to Azure is a concern. I would choose Azure Service Bus when the workload already lives on Azure and zero operational overhead, per-customer ordered processing via sessions, or integrated portal-level dead-letter visibility matters.

---

## Q7. What are Kafka partitions and consumer groups, and how does Kafka achieve ordered and scalable message consumption?

**Concepts**
- Kafka topic partition as ordered, immutable, append-only log
- Consumer group — each partition assigned to exactly one consumer instance
- Partition key for per-entity ordering across a distributed system
- Offset commits replacing per-message acknowledgement
- Configurable message retention enabling consumer group replay

**Answer**

Kafka stores messages in topics, each split into one or more partitions. A partition is an ordered, immutable, append-only log — messages within a single partition are always read in exactly the order they were written, which is where Kafka's ordering guarantee comes from. A consumer group is a named set of consumers that collectively read all partitions of a topic, with each partition assigned to exactly one consumer in the group at any given time. This design gives both ordering and scalability: adding more instances to the consumer group causes Kafka to rebalance partition assignments, and parallel processing scales up to the number of partitions. Ordering is guaranteed only within a partition, so to preserve event order for a specific entity — such as all events for a given customer or order — I would use a partition key derived from that entity's ID, ensuring all its events land on the same partition. Kafka also retains messages for a configurable period regardless of whether they have been consumed, unlike traditional brokers that delete messages after acknowledgement, which allows consumer groups to replay history or reprocess from any offset after a failure. The acknowledgement mechanism is an offset commit rather than a per-message ack: a consumer periodically commits the highest offset it has successfully processed, and Kafka re-delivers from that offset on restart.

---

## Q8. What is MassTransit, and what does it add over using a raw broker client library directly?

**Concepts**
- MassTransit as broker-agnostic .NET service bus abstraction
- Convention-based topology creation from .NET type names
- Built-in retry, circuit breaker, and dead-letter consumer policies
- Saga state machine for durable long-running workflow orchestration
- Request/response over messaging with correlated response routing

**Answer**

MassTransit is an open-source .NET service bus abstraction that runs on top of RabbitMQ, Azure Service Bus, Amazon SQS, Kafka, and other transports. Rather than writing broker-specific publish and consume code with native client libraries, you write against MassTransit's uniform API, so switching brokers becomes a configuration change rather than a code rewrite. Beyond transport abstraction, MassTransit adds features you would otherwise build manually: built-in retry policies, circuit breakers, and dead-letter consumer configuration that apply consistently across all transports; convention-based message routing that automatically creates exchanges, queues, and bindings from .NET type names, so you rarely configure broker topology by hand; and a Saga state machine that lets you model long-running stateful workflows — such as an order fulfillment process spanning multiple services — with durable state persisted to a database. It also supports request/response over messaging natively, where a service publishes a request and awaits a correlated reply, giving RPC semantics without a synchronous HTTP call.

```csharp
// Consumer in .NET 10 with MassTransit
public class OrderPlacedConsumer : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var order = context.Message;
        await ProcessAsync(order.OrderId);
    }
}
```

---

## Q9. How do you implement consumer idempotency — what happens if the same message is delivered and processed more than once?

**Concepts**
- At-least-once delivery as the source of duplicate processing
- Processed-message log with unique ID as an idempotency check
- Natural idempotency via upserts and conditional state transitions
- Stable broker-assigned message IDs as idempotency keys
- Atomic check-and-insert with unique constraint against concurrent duplicates

**Answer**

At-least-once delivery means any message can legitimately arrive more than once — after a consumer crash before acknowledging, after a network timeout, or after a broker failover — so idempotency is a consumer-side responsibility that the broker does not handle. The simplest approach is a processed-message log: before doing any work, check a database table for the message's unique identifier, and if it already exists skip processing and return success. This is reliable but adds a database read per message. A more elegant approach is to make the business operation itself naturally idempotent by using upserts rather than plain inserts, or conditional updates that only apply when the record is in an expected state — for example, "only mark as shipped if status is currently `paid`." I would use the message's correlation ID or message ID as the idempotency key rather than generating my own, since both RabbitMQ and Azure Service Bus assign stable IDs so redelivered copies of the same original message share the same key. In distributed scenarios where two concurrent deliveries might both pass the lookup check before either commits, an atomic check-and-insert with a unique database constraint prevents the race condition.

---

## Q10. What is the competing consumers pattern, and what trade-offs does it introduce for message ordering?

**Concepts**
- Competing consumers — multiple instances share one queue, each message to one instance
- Global ordering loss across concurrent consumers
- Azure Service Bus sessions for serialized per-entity ordering at scale
- Kafka partition-per-key preserving per-entity ordering
- Commutative or idempotent operations as the prerequisite for ordering-free scaling

**Answer**

The competing consumers pattern runs multiple instances of the same consumer reading from a single queue, where each message is delivered to exactly one instance, enabling horizontal scaling by adding more consumer instances. The trade-off is that global message ordering across all consumers is no longer guaranteed, because messages are dispatched to whichever consumer is free — consumer A might receive message 3 while consumer B is still processing message 2, so the side effects of message 3 can appear in the system before those of message 2. Azure Service Bus sessions solve this for ordered workloads: a session key such as a customer ID locks all messages with that key to one consumer at a time, so per-entity ordering is preserved while the system still scales across different session keys in parallel. Kafka achieves the same effect through partition-per-key assignment, where all messages for a given key land on the same partition and are read by exactly one consumer in the group. When operations are commutative or idempotent — where processing order does not matter — the competing consumers pattern is the simplest and most effective way to increase throughput without ordering concerns.

---

## Q11. How do you evolve message schemas without breaking existing consumers — what compatibility strategies are available?

**Concepts**
- Backward and forward compatibility as the default requirement
- Adding optional fields with defaults as the safe change
- Deprecation in place rather than immediate deletion
- Schema registry enforcing compatibility rules at publish time
- Versioned message types for unavoidable breaking changes

**Answer**

Message schema evolution is the challenge of changing a message's structure while keeping consumers built against an older version still functional. Because producers and consumers are often deployed independently, schema changes must be backward- and forward-compatible by default — failing to manage this causes deserialization errors or silent data loss in production. The safest rule is to only add new fields and always make them optional or nullable with a sensible default, since older consumers ignore unknown fields if the deserializer is configured permissively and newer consumers handle the field's absence gracefully. I would never remove or rename a field that any deployed consumer depends on; instead I would deprecate it by leaving it in place and removing it only after all consumers have been updated and deployed. A schema registry such as Confluent Schema Registry for Kafka enforces compatibility rules at publish time and rejects schemas that would break registered consumers before the message enters the system. For breaking changes that cannot be avoided, I introduce a versioned message type such as `OrderPlacedV2` alongside the original, migrate consumers one by one, and decommission the V1 path only after all consumers are on V2.

---

## Q12. How does the transactional outbox pattern solve the dual-write problem between a database and a message broker?

**Concepts**
- Dual-write problem — database and broker cannot join one distributed transaction
- Outbox table written atomically with domain data in one local transaction
- Background relay worker polling and publishing with broker acknowledgement
- At-least-once delivery from the relay requiring idempotent consumers

**Answer**

The dual-write problem arises when code must atomically update a database and publish a message to a broker — two resources that cannot participate in the same transaction. If the database commit succeeds but the broker publish fails, the system is left inconsistent: data changed but no event was emitted, so downstream services never react. The transactional outbox pattern solves this by writing the intended message to an `OutboxMessages` table in the same local database transaction as the domain data, deferring actual broker delivery to a separate process — so either both the domain data and the outbox row are committed, or neither is. A background worker — typically a `BackgroundService` with a `PeriodicTimer` — polls the outbox for unprocessed rows, publishes each to the broker, and marks the row as processed only after the broker acknowledges receipt. Because publishing is decoupled from the HTTP request path, transient broker failures do not roll back business data; the worker retries on the next poll cycle. The pattern guarantees at-least-once delivery, so consumers must still be idempotent — a worker crash between publish and marking processed causes the same message to be republished on the next cycle.

```csharp
// Domain handler — single transaction covers both writes
await db.Orders.AddAsync(order);
await db.OutboxMessages.AddAsync(new OutboxMessage
{
    Payload = JsonSerializer.Serialize(new OrderPlaced(order.Id)),
    CreatedAt = DateTime.UtcNow
});
await db.SaveChangesAsync(); // atomic — either both succeed or both roll back
```

---

## Gotchas — Distributed Messaging (Interview Traps)

---

#### Gotcha 1. Assuming Messages Are Delivered in FIFO Order

**Concepts**
- At-least-once delivery independent of ordering guarantee
- Kafka partition ordering versus global topic ordering
- RabbitMQ queue ordering broken by concurrent consumers
- Per-entity ordering via partition key assignment

**Answer**

Message brokers that guarantee at-least-once delivery do not automatically guarantee global FIFO ordering — Kafka preserves ordering within a single partition, but messages with different partition keys (or no key) arrive at consumers in interleaved order. RabbitMQ queues are FIFO by default, but multiple concurrent consumers process messages in parallel, so an earlier message may be processed after a later one if the earlier consumer is slower. Code that assumes strict ordering — applying events to an entity in sequence — will corrupt state when events arrive out of order. The solution is to route all messages for a given entity (e.g., by `OrderId`) to the same partition so they are ordered within that entity's processing stream.

---

#### Gotcha 2. Consumer Group Rebalance Interrupting Processing

**Concepts**
- Kafka consumer group rebalance triggered by new consumer joining or leaving
- Partition reassignment pausing all consumers briefly
- Long processing time triggering session timeout and forced rebalance
- max.poll.interval.ms and heartbeat.interval.ms tuning

**Answer**

A Kafka consumer group rebalances — reassigning partitions among consumers — whenever a consumer joins, leaves, or fails to poll within `max.poll.interval.ms`. During a rebalance, all consumers in the group pause processing, which means message processing stops for the entire group until the rebalance completes. A slow consumer that takes longer than `max.poll.interval.ms` to process a batch is considered dead and triggers a rebalance that disrupts all healthy consumers. The fix is to keep processing fast, set `max.poll.interval.ms` generously for slow-processing consumers, or decouple message consumption from processing by handing messages to an in-process queue and returning the poll thread immediately.

---

#### Gotcha 3. Dead-Letter Queue Not Monitored or Alerting

**Concepts**
- DLQ accumulating silently without operational visibility
- Consumer bug routing all messages to DLQ undetected
- DLQ depth as a critical alert metric
- Replay workflow required to recover DLQ messages after fix

**Answer**

A dead-letter queue that grows silently is an invisible data loss event — if a consumer has a bug that throws on every message, all messages route to the DLQ and processing stops while the operational dashboard shows the consumer as "running." Every DLQ must have a CloudWatch Alarm, Azure Monitor alert, or Prometheus alert that fires on any non-zero queue depth, with an on-call runbook that describes how to diagnose the failure, fix the consumer, and replay the DLQ messages. Interviewers expect candidates to describe the full DLQ lifecycle: retry policy, DLQ routing, alerting, consumer fix, and replay — not just the happy path where DLQs exist but are never checked.

---

#### Gotcha 4. Non-Idempotent Consumer Under At-Least-Once Delivery

**Concepts**
- At-least-once delivery guaranteeing duplicate messages on retry or redelivery
- Duplicate charge, duplicate email, duplicate inventory decrement
- Processed-messages table with message ID as deduplication mechanism
- Conditional update as an alternative idempotency strategy

**Answer**

Any broker configured for at-least-once delivery will redeliver messages when a consumer crashes before acknowledging, when a network error prevents the acknowledgement from reaching the broker, or when the broker restarts. A consumer that charges a card, sends an email, or decrements inventory without a deduplication check will perform those operations multiple times for the same logical message. The standard deduplication pattern is an `OutboxProcessed` or `ProcessedMessages` table with the message ID as a unique key — before processing, check if the ID exists; if yes, acknowledge without processing; if no, process and insert the ID within the same database transaction as the business effect.

---

#### Gotcha 5. Infinite Retry Loop on a Poison Message

**Concepts**
- Poison message causing consumer to throw on every retry attempt
- Retry policy without a maximum attempt limit blocking the queue
- Dead-letter queue routing after exhausting retry count
- Circuit breaker on the consumer as an additional protection

**Answer**

A "poison message" is one that causes the consumer to throw an exception every time it is processed — a malformed payload, an unexpected null, or a downstream dependency that is permanently unavailable for that message. A retry policy without a maximum retry count (or with an extremely high count) will loop forever, blocking the consumer from processing any subsequent messages and consuming CPU in an exponential-backoff loop. Every retry policy must have a maximum attempt count (`RetryCount = 5`) after which the message is routed to the dead-letter queue and the consumer moves on. Without this, a single poison message can bring a consumer to a halt indefinitely.

---

#### Gotcha 6. Publishing Without Confirming Broker Acknowledgement

**Concepts**
- Fire-and-forget publish losing messages on broker crash
- Publisher confirms (RabbitMQ) and acks (Kafka producer acks=all)
- Outbox pattern as the application-level durability guarantee
- Message loss versus latency trade-off in acknowledgement modes

**Answer**

A producer that publishes a message and does not wait for the broker's acknowledgement (RabbitMQ `BasicPublish` without publisher confirms, Kafka producer with `acks=0`) operates in fire-and-forget mode — if the broker crashes or the network drops immediately after publish, the message is lost with no error visible to the producer. For durable messaging, RabbitMQ publisher confirms or Kafka `acks=all` (all ISR replicas acknowledge) must be enabled, at the cost of increased publish latency. The Transactional Outbox Pattern provides durability at the application level by writing to the database first and relaying to the broker separately, which is the correct approach when the publishing service also writes to a database.

---

#### Gotcha 7. Message Ordering Broken by Competing Consumers on RabbitMQ

**Concepts**
- Single queue with multiple concurrent consumers processing out of order
- Consumer A slower than Consumer B receiving an earlier message
- Exclusive consumer pattern for strict ordering
- Single-partition Kafka topic for strict-order use cases

**Answer**

A RabbitMQ queue with three concurrent consumers delivers messages to whichever consumer is free — if Consumer 1 receives message 1 but is slow, and Consumer 2 receives message 2 and finishes first, message 2 is processed before message 1 despite being enqueued later. For scenarios that require strict per-entity ordering — state machine transitions, sequential event sourcing — either use a single exclusive consumer (no parallelism, low throughput), route messages for the same entity to a dedicated queue, or use Kafka with a partition key so all messages for one entity land on the same partition processed by a single consumer.

---

#### Gotcha 8. Committing Kafka Offset Before Processing Completes

**Concepts**
- Early offset commit marking a message as processed before the business effect
- Process crash after commit losing the message permanently
- at-least-once semantics requiring commit after successful processing
- Manual commit versus auto-commit in Kafka consumer configuration

**Answer**

Kafka's `enable.auto.commit=true` (the default) commits the offset automatically on a background timer, independent of whether the business logic for that message has completed successfully. If the consumer crashes after the auto-commit but before the database write completes, the message is considered processed and will not be redelivered — the business effect is lost. For reliable processing, auto-commit must be disabled and the offset committed manually only after the business operation and its database transaction have succeeded. The ordering is: receive message → process business logic → commit to database → commit Kafka offset.

---

#### Gotcha 9. Schema-less Message Payloads Breaking Consumers on Field Rename

**Concepts**
- Loose JSON payload with no contract enforcement
- Field renamed or removed by producer breaking consumer silently
- Schema Registry enforcing backward compatibility at publish time
- Consumer-Driven Contract Testing as an alternative

**Answer**

Publishing JSON messages without a schema registry or contract test means a producer team can rename `customerId` to `customer_id` in a refactor, the change passes all producer unit tests, ships to production, and silently breaks every consumer that reads `customerId` — which now deserializes to null or throws, depending on the consumer's null handling. A schema registry (Confluent Schema Registry, Azure Schema Registry) enforces compatibility rules at publish time and rejects a new schema that removes or renames a field without a version bump. Without a registry, Consumer-Driven Contract Testing with Pact achieves the same protection by running the producer's CI against schemas the consumers have registered.

---

#### Gotcha 10. No Backpressure Handling on the Consumer Side

**Concepts**
- Consumer processing slower than producer publishing rate
- Queue depth growing unboundedly until broker runs out of memory
- Consumer-side throttling using prefetch count and processing concurrency
- Producer-side rate limiting when queue depth exceeds a threshold

**Answer**

A consumer that processes one message in 100 ms while the producer publishes 100 messages per second will fall indefinitely behind, growing the queue depth without bound until the broker runs out of disk space or memory and starts dropping messages or blocking producers. RabbitMQ's `basicQos` prefetch count limits how many unacknowledged messages the broker delivers to a consumer at once, acting as a natural backpressure mechanism — once the prefetch window is full, the broker stops delivering to that consumer. Kafka's `max.poll.records` limits how many records are fetched per poll cycle. Consumers must also horizontally scale by adding consumer instances to the consumer group when sustained throughput exceeds single-consumer capacity.

---
