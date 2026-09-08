# Choosing a Protocol — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — Choosing a Protocol (Interview Traps)](#gotchas--choosing-a-protocol-interview-traps)

---

## Gotchas — Choosing a Protocol (Interview Traps)

---

#### Gotcha 1. Choosing WebSockets When SSE Is Sufficient for One-Way Server Push

**Concepts**
- WebSockets provide full-duplex communication; SSE provides one-way server-to-client push
- If the client only needs to receive events and never sends data on the same connection, SSE is simpler
- WebSockets require more infrastructure support (upgrade headers, proxy configuration)
- SSE reconnects automatically; WebSockets require manual reconnect logic

**Answer**

WebSockets are bidirectional by design and are the correct choice when the client needs to send data to the server over the same connection — chat messages, collaborative edits, game inputs. For read-only server push (stock price tickers, notification feeds, live log streams) where the client only consumes events and sends commands via separate HTTP requests, SSE provides the same delivery capability with far less infrastructure complexity. SSE reconnects automatically via the browser's `EventSource` API, works over standard HTTP/1.1 and HTTP/2, and requires no special proxy configuration beyond disabling response buffering. Choosing WebSockets for a one-way push scenario adds unneeded complexity: WebSocket upgrade proxy requirements, manual reconnect logic, and a framing protocol that provides no benefit when only one direction carries data.

---

#### Gotcha 2. Selecting gRPC for Browser Clients Without Accounting for grpc-web

**Concepts**
- Native gRPC requires HTTP/2 trailer access that browsers do not expose through Fetch/XHR
- Browser gRPC requires the grpc-web wrapper protocol and a compatible client library
- grpc-web does not support client streaming or bidirectional streaming
- A team that chooses gRPC for a browser-facing API must plan the grpc-web layer from day one

**Answer**

gRPC is an excellent choice for server-to-server communication but creates an additional layer of complexity for browser clients. The Fetch API and XHR do not expose HTTP/2 trailer frames, which gRPC uses to carry its status code. Browser clients must use the `grpc-web` protocol, which wraps trailers in a body-encoded frame, and a corresponding npm client library. Additionally, `grpc-web` supports only unary and server-streaming call types — client streaming and bidirectional streaming are not available to browser clients. An API team that selects gRPC for a client-facing API intending to serve web browsers must plan the grpc-web proxy (Envoy or `UseGrpcWeb()` in ASP.NET Core) from the start and accept the streaming limitations. Discovering this constraint after the contracts are written requires either limiting the API design or adding an HTTP REST facade.

---

#### Gotcha 3. Assuming REST Is Stateless Rules Out Long-Lived Connections Without Thinking Through the Trade-Off

**Concepts**
- REST is stateless in the HTTP sense: each request carries all the information needed to process it
- Real-time features (notifications, live updates) require long-lived connections that REST alone cannot provide
- Adding WebSockets or SignalR to a REST API is additive, not contradictory
- The stateless vs stateful distinction becomes a conversation about infrastructure, not API design philosophy

**Answer**

REST's statelessness means each HTTP request must carry all necessary information — authentication, context, parameters — without relying on server-side session state. This is a constraint on individual requests, not a prohibition on persistent connections. Many production architectures combine a REST API for resource management with WebSockets or SignalR for real-time event delivery. The REST principle is not violated by this combination — the REST endpoints remain stateless, and the WebSocket connection is a separate channel for event streaming. Teams that interpret REST's statelessness as "no long-lived connections allowed" and attempt to implement real-time features through high-frequency polling are solving an architectural problem with the wrong tool. The correct framing is: use REST for request-response interactions and add a push channel for real-time delivery.

---

#### Gotcha 4. Optimising for Latency vs Throughput Without Identifying Which Constraint Matters

**Concepts**
- Latency: time from event to delivery (WebSockets and gRPC streaming minimise per-message delay)
- Throughput: messages per second through the system (binary protocols and batching maximise this)
- SSE has higher per-message overhead than WebSockets but adequate throughput for most notification use cases
- Selecting a protocol based on throughput capability when the real constraint is latency optimises the wrong metric

**Answer**

Latency is the time it takes one message to travel from sender to receiver; throughput is the number of messages the system can process per unit time. Different protocols optimise for different constraints. WebSocket and gRPC streaming minimise per-message overhead and deliver low latency. HTTP/2 with batching and compression maximises throughput for large data transfers. SSE introduces a small per-event serialisation overhead but is perfectly adequate for notification use cases where dozens of events per second per client is the upper bound. A team that selects gRPC streaming over SSE because gRPC achieves higher theoretical throughput for a notification feed that delivers three events per minute has chosen a protocol based on an irrelevant metric. The selection criterion must match the actual operational constraint — latency SLA for trading applications, throughput for bulk data feeds, simplicity for simple notification services.

---

#### Gotcha 5. Choosing a Binary Protocol Without Verifying That All Client Environments Support It

**Concepts**
- Protobuf and MessagePack are binary; JSON is text — text protocols are universally supported
- Binary protocols are not directly readable in browser developer tools without special plugins
- Some corporate web filtering proxies inspect and block non-HTTP binary payloads
- Client ecosystem support (mobile SDK, third-party integrations) may only offer JSON parsing

**Answer**

Binary protocols like Protobuf (gRPC) and MessagePack (SignalR optional) reduce payload size and serialisation overhead compared to JSON, which matters for high-throughput internal service communication. However, binary payloads are not human-readable, require dedicated tooling (gRPCurl, Protobuf decoder plugins) for debugging, and may be blocked by corporate web filters that only recognise HTTP text payloads. Third-party integrations, legacy mobile clients, and community-built API consumers typically support JSON and not binary protocols. Adopting a binary protocol for a public or partner-facing API limits the ecosystem of clients that can easily integrate. The decision should weigh the performance benefit of binary serialisation against the tooling friction, ecosystem breadth, and debuggability requirements — favouring binary for internal high-performance calls and JSON for any interface consumed by external parties.

---

#### Gotcha 6. Using Long Polling as a Default Transport Instead of Treating It as a Last-Resort Fallback

**Concepts**
- Long polling simulates push by making each HTTP response trigger a new client request
- Per-message latency is at minimum one network round trip; WebSockets achieve sub-millisecond delivery
- Long polling generates more HTTP overhead: a full request-response cycle per message
- SignalR uses long polling only when WebSockets and SSE are both unavailable

**Answer**

Long polling emerged as a workaround for environments that blocked WebSocket upgrades — enterprise firewalls, older proxies, Internet Explorer. It is the last fallback in SignalR's transport negotiation precisely because its performance characteristics are materially worse than WebSockets or SSE. A team that chooses long polling as its default real-time transport because "it's simpler to implement" accepts higher per-message latency (at least one round-trip time), higher server resource consumption (each pending request holds an HttpContext and task continuation), and more complex message queuing requirements (server must buffer events emitted during the gap between poll completions). SignalR's automatic fallback to long polling exists for environments where no better option is available — it is not an endorsement of long polling as an acceptable primary transport.

---

#### Gotcha 7. Treating SignalR and Raw WebSockets as Equivalent Choices

**Concepts**
- SignalR adds hub protocol, named-method dispatch, automatic reconnect, and group management
- Raw WebSockets provide a byte channel with no naming, routing, or reconnect infrastructure
- SignalR's negotiate round trip adds one HTTP request to connection setup
- Raw WebSockets are appropriate for interoperability with non-SignalR peers or binary subprotocols

**Answer**

SignalR and raw WebSockets both use WebSocket transport at the TCP layer, but they operate at different levels of abstraction. SignalR adds a hub protocol (JSON or MessagePack) that provides named-method invocation, return values, automatic client reconnect with configurable retry, group management, and transport fallback. Raw WebSockets provide a bidirectional byte channel with no application-layer structure — the application must implement all framing, routing, and reconnect logic manually. Choosing raw WebSockets when building a hub-style application means reimplementing what SignalR provides. Choosing SignalR when the application must interoperate with a non-SignalR peer (an IoT device speaking its own binary subprotocol, a STOMP broker, an MQTT endpoint) imposes SignalR's framing on a peer that does not understand it. The decision is about whether the application needs the hub abstraction layer or requires protocol interoperability.

---

#### Gotcha 8. Choosing OData for High-Throughput Fixed-Shape APIs Where Simple REST Is Better

**Concepts**
- OData's query processing overhead is non-trivial: URL parsing, LINQ expression composition, metadata loading
- Fixed-shape REST endpoints have predictable performance because the query plan is known at design time
- OData's query flexibility creates a large security surface requiring explicit configuration
- Reporting and BI use cases justify OData's cost; high-throughput transactional APIs do not

**Answer**

OData's flexibility comes with overhead: every request parses and validates the URL query options, constructs a LINQ expression tree that is then translated to SQL, and processes the EDM to validate property names and navigation paths. For high-throughput transactional APIs where every response has a fixed shape — return an order by ID, list the user's recent transactions — this overhead is wasted. A REST endpoint with a fixed SQL query translates directly to a prepared statement and executes with minimal overhead. OData earns its cost when clients genuinely need ad-hoc query capability — dynamic filtering, cross-entity joins, variable projections — such as reporting dashboards and Excel connectivity scenarios. Using OData for a payment processing API that returns fixed-shape responses at tens of thousands of requests per second adds processing overhead and a broad query security surface without any corresponding benefit.

---

#### Gotcha 9. Conflating GraphQL Subscriptions with REST Webhooks — Different Delivery Guarantees

**Concepts**
- GraphQL subscriptions maintain a persistent WebSocket connection; the server pushes events on it
- REST webhooks make outbound HTTP POST requests from the server to the client's registered URL
- Subscriptions require the client to maintain a live connection; webhooks work across disconnections
- Webhooks are stateless from the server perspective; subscriptions are stateful (connection must be alive)

**Answer**

GraphQL subscriptions and REST webhooks both deliver real-time events from server to client, but their delivery models are fundamentally different. A GraphQL subscription requires the client to maintain a live WebSocket connection; events are delivered only while the connection is open. If the client is offline, events are lost (unless the server implements a queue with replay). REST webhooks invert the direction: the server makes an HTTP POST to a URL the client pre-registered. The client receives events even after a restart because the delivery is pull-based from the server's perspective — it retries failed POST calls. Webhooks are appropriate for server-to-server integrations where the receiving service may restart, scale, or be temporarily unavailable. GraphQL subscriptions are appropriate for browser clients that are online for the duration of a user session. Mixing up these models leads to either dropped events (using subscriptions where the client may disconnect) or unnecessary infrastructure complexity (using webhooks where a live browser connection would suffice).

---

#### Gotcha 10. Mixing Multiple Protocols in the Same Application Without a Consistent Authentication Strategy

**Concepts**
- REST endpoints use Authorization: Bearer header; SignalR must use query string for WebSocket transport
- gRPC uses HTTP/2 Metadata headers; GraphQL subscriptions use WebSocket connection-init payload
- A single JWT token issuer and validation configuration must serve all these mechanisms
- Inconsistent token delivery paths create different security exposure profiles

**Answer**

A full-stack application commonly uses multiple protocols simultaneously: REST for resource management, SignalR for real-time notifications, gRPC for internal service calls, and GraphQL for flexible client queries. Each protocol delivers authentication tokens differently — REST via `Authorization: Bearer`, SignalR via query string `?access_token=`, gRPC via HTTP/2 metadata headers, and GraphQL WebSocket subscriptions via a `ConnectionInit` payload. Without a deliberate authentication strategy that accounts for these differences, developers apply different JWT configurations to each protocol, use different token lifetimes, or miss securing one protocol entirely. The recommended approach is a single `AddJwtBearer()` configuration shared by all protocol handlers, with explicit per-protocol token extraction hooks where needed (SignalR's `OnMessageReceived` for query-string tokens, HotChocolate's `ConnectionInit` handler for WebSocket payload tokens). Security reviews must cover all protocol surfaces, not just the REST endpoints.

---
