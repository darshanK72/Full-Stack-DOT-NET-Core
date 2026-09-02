# 08. Real-Time & Communication Protocols — Cross-Topic Interview Q&A
> Back to [Full Stack .NET Core](../README.md)

## Subfolders

| # | Topic | Summary |
|---|-------|---------|
| 01 | [HTTP Deep Dive](01.%20HTTP%20Deep%20Dive/INTERVIEW_QA.md) | HTTP/1.1 vs HTTP/2 vs HTTP/3, headers, status codes, caching, and connection lifecycle |
| 02 | [WebSockets in ASP.NET Core](02.%20WebSockets%20in%20ASP.NET%20Core/INTERVIEW_QA.md) | Full-duplex TCP upgrade, middleware wiring, and raw WebSocket message handling |
| 03 | [Server-Sent Events %28SSE%29](03.%20Server-Sent%20Events%20%28SSE%29/INTERVIEW_QA.md) | One-way server push over HTTP/1.1, event stream format, and browser reconnect behaviour |
| 04 | [Long Polling %26 Transport Fallbacks](04.%20Long%20Polling%20%26%20Transport%20Fallbacks/INTERVIEW_QA.md) | Hanging GET pattern, timeout tuning, and why it remains the last-resort transport |
| 05 | [SignalR Fundamentals](05.%20SignalR%20Fundamentals/INTERVIEW_QA.md) | Hub abstraction, connection lifecycle, and automatic transport negotiation |
| 06 | [SignalR Hubs, Groups %26 Clients](06.%20SignalR%20Hubs%2C%20Groups%20%26%20Clients/INTERVIEW_QA.md) | Hub methods, group management, strongly-typed hubs, and targeted invocations |
| 07 | [SignalR with Authentication](07.%20SignalR%20with%20Authentication/INTERVIEW_QA.md) | JWT on the WebSocket upgrade, query-string token delivery, and [Authorize] on hubs |
| 08 | [WebRTC Signaling with ASP.NET Core](08.%20WebRTC%20Signaling%20with%20ASP.NET%20Core/INTERVIEW_QA.md) | SDP offer/answer exchange, ICE candidate relay, and STUN/TURN coordination |
| 09 | [GraphQL with HotChocolate](09.%20GraphQL%20with%20HotChocolate/INTERVIEW_QA.md) | Schema-first vs code-first, queries/mutations/subscriptions, and DataLoader |
| 10 | [gRPC for Client-Facing APIs](10.%20gRPC%20for%20Client-Facing%20APIs/INTERVIEW_QA.md) | Protobuf contracts, streaming RPCs, HTTP/2 requirements, and grpc-web |
| 11 | [OData APIs](11.%20OData%20APIs/INTERVIEW_QA.md) | $filter/$select/$expand query options, EDM metadata, and EF Core integration |
| 12 | [Choosing a Protocol](12.%20Choosing%20a%20Protocol/INTERVIEW_QA.md) | Decision matrix across REST, GraphQL, gRPC, WebSockets, SSE, and SignalR |

---

## Table of Contents
- [CQ1. How does SignalR abstract WebSockets, SSE, and Long Polling — and when does it fall back?](#cq1-how-does-signalr-abstract-websockets-sse-and-long-polling--and-when-does-it-fall-back)
- [CQ2. Why must SignalR pass JWT tokens in the query string, and what are the security implications?](#cq2-why-must-signalr-pass-jwt-tokens-in-the-query-string-and-what-are-the-security-implications)
- [CQ3. Why does gRPC require HTTP/2, and how does that affect proxy and browser deployments?](#cq3-why-does-grpc-require-http2-and-how-does-that-affect-proxy-and-browser-deployments)
- [CQ4. Why do SignalR groups break under scale-out, and how does a Redis backplane fix it?](#cq4-why-do-signalr-groups-break-under-scale-out-and-how-does-a-redis-backplane-fix-it)
- [CQ5. When should you choose GraphQL over REST, and what is the N+1 problem in resolvers?](#cq5-when-should-you-choose-graphql-over-rest-and-what-is-the-n1-problem-in-resolvers)
- [CQ6. Why does WebRTC still need a signaling server, and how does ASP.NET Core provide one?](#cq6-why-does-webrtc-still-need-a-signaling-server-and-how-does-aspnet-core-provide-one)
- [CQ7. How does HTTP/2 server push differ architecturally from SSE, and why is SSE still widely used?](#cq7-how-does-http2-server-push-differ-architecturally-from-sse-and-why-is-sse-still-widely-used)
- [CQ8. What does SignalR add over raw WebSockets, and when should you use raw WebSockets instead?](#cq8-what-does-signalr-add-over-raw-websockets-and-when-should-you-use-raw-websockets-instead)
- [CQ9. How do SSE and long polling differ in HTTP mechanics, and how do load balancers treat each?](#cq9-how-do-sse-and-long-polling-differ-in-http-mechanics-and-how-do-load-balancers-treat-each)
- [CQ10. When should you choose gRPC vs OData vs REST for a client-facing API?](#cq10-when-should-you-choose-grpc-vs-odata-vs-rest-for-a-client-facing-api)
- [CQ11. How does HotChocolate deliver GraphQL subscriptions, and how does this interact with SignalR auth?](#cq11-how-does-hotchocolate-deliver-graphql-subscriptions-and-how-does-this-interact-with-signalr-auth)
- [CQ12. What happens to SignalR group membership when a client reconnects, and how should applications handle it?](#cq12-what-happens-to-signalr-group-membership-when-a-client-reconnects-and-how-should-applications-handle-it)
- [CQ13. How does HTTP/3 (QUIC) benefit gRPC, and what does ASP.NET Core Kestrel support in .NET 10?](#cq13-how-does-http3-quic-benefit-grpc-and-what-does-aspnet-core-kestrel-support-in-net-10)
- [CQ14. Once WebRTC ICE negotiation succeeds, what role does the signaling server play?](#cq14-once-webrtc-ice-negotiation-succeeds-what-role-does-the-signaling-server-play)
- [CQ15. How do you secure OData endpoints in ASP.NET Core, and what is the $expand authorization risk?](#cq15-how-do-you-secure-odata-endpoints-in-aspnet-core-and-what-is-the-expand-authorization-risk)
- [CQ16. What is the server resource cost of SignalR long polling vs WebSocket connections?](#cq16-what-is-the-server-resource-cost-of-signalr-long-polling-vs-websocket-connections)

---

## CQ1. How does SignalR abstract WebSockets, SSE, and Long Polling — and when does it fall back?

**Concepts**
- SignalR negotiates the best available transport on every new connection
- Transport preference order: WebSockets > Server-Sent Events > Long Polling
- Negotiation is driven by a preflight POST to `/hub-path/negotiate`
- Firewalls, reverse proxies, and corporate HTTP proxies commonly block WebSocket upgrades
- `skipNegotiation: true` (client-side) forces WebSockets and bypasses fallback entirely

**Answer**
When a SignalR client connects it first sends a POST to the negotiate endpoint; the server returns a list of supported transports and the client picks the best one it can use. WebSockets are tried first because they are full-duplex and maintain a single TCP connection. If the WebSocket upgrade is blocked — typically by a corporate proxy that only understands HTTP/1.1 or by a load balancer that strips the `Upgrade` header — SignalR falls back to Server-Sent Events, which is a one-way server-push stream over plain HTTP. If SSE is also unavailable (Internet Explorer, certain network policies), it falls back to long polling, where the client sends a hanging GET that the server holds open until it has data or a timeout fires. The common gotcha is assuming that because WebSockets work in development they will work in production; a classic symptom is a hub that works on localhost but shows 500 ms round-trip latency in staging because it silently degraded to long polling through a proxy. Checking the transport in use via `connection.transport?.name` (JavaScript) or hub connection logs surfaces this quickly.

---

## CQ2. Why must SignalR pass JWT tokens in the query string, and what are the security implications?

**Concepts**
- Browser WebSocket API has no mechanism to set custom request headers
- `Authorization: Bearer` header cannot be attached to a WebSocket upgrade request from a browser
- SignalR's built-in `AccessTokenProvider` appends the token as `?access_token=...`
- Query strings are recorded verbatim in web server access logs, CDN logs, and proxy logs
- HTTPS encryption protects the token in transit but not from log storage

**Answer**
HTTP Bearer authentication works because the browser can set the `Authorization` header on every fetch or XHR request. The WebSocket API, however, only accepts the URL and a subprotocol array — there is no overload that accepts custom headers. When SignalR negotiates a WebSocket transport the initial upgrade request must carry the token, so the only cross-browser mechanism is the query string. SignalR's JavaScript client uses the `accessTokenFactory` callback for exactly this: it appends `?access_token=<token>` to the upgrade URL. The security implication is that access logs on the server, any intermediate proxy, and any CDN edge node will record the full URL including the token value. A short-lived token (under 5 minutes) significantly reduces the blast radius of a log exposure, but it does not eliminate it. In environments with strict compliance requirements the mitigation is either to use a cookie-based auth flow (the negotiate POST sets a cookie the upgrade request then carries automatically) or to rotate tokens aggressively and ensure log access is tightly controlled. This is in direct contrast to standard HTTP API calls where the `Authorization` header is in the request body of TLS records and never appears in access logs.

---

## CQ3. Why does gRPC require HTTP/2, and how does that affect proxy and browser deployments?

**Concepts**
- gRPC framing relies on HTTP/2 binary multiplexing and trailer headers
- HTTP/1.1 does not support trailers; gRPC status codes are delivered in a trailer frame
- Many load balancers (AWS ALB classic, Nginx default config) terminate or downgrade HTTP/2
- `grpc-web` is a spec that wraps gRPC frames in HTTP/1.1 for browser and proxy compatibility
- .NET 10 ships `Grpc.AspNetCore` with `EnableGrpcWeb()` middleware

**Answer**
gRPC encodes its status code and metadata at the end of a response using HTTP/2 trailer frames, a feature that simply does not exist in HTTP/1.1. Every gRPC call therefore requires an HTTP/2 connection end-to-end — from client to server with no HTTP/1.1 hop in between. This creates a production gotcha with AWS Application Load Balancer: ALB supports HTTP/2 on the listener but by default forwards requests to targets over HTTP/1.1. A gRPC service sitting behind ALB in this default configuration will fail with protocol errors at the gRPC framing layer even though the TLS handshake succeeds. The fix is to use Network Load Balancer (which passes TCP transparently) or enable HTTP/2 on the ALB target group. Browsers add another layer: the Fetch API and XHR both support HTTP/2, but browsers block raw HTTP/2 cleartext (h2c) and the browser has no way to read HTTP/2 trailer frames through the standard APIs, so gRPC in a browser requires `grpc-web`. The `grpc-web` spec remaps trailers into a special framing byte at the end of the HTTP/1.1 response body, which the grpc-web client library decodes. In ASP.NET Core, calling `app.UseGrpcWeb()` and marking individual services with `EnableGrpcWeb()` enables this without changing the Protobuf contracts.

---

## CQ4. Why do SignalR groups break under scale-out, and how does a Redis backplane fix it?

**Concepts**
- Each ASP.NET Core process maintains its own in-memory group-to-connection map
- A client connected to Server A holds a connection context only on Server A
- A group message sent from Server B has no knowledge of connections on Server A
- Redis backplane (`Microsoft.AspNetCore.SignalR.StackExchangeRedis`) uses pub/sub to fan out
- Sticky sessions are an alternative but create uneven load and single points of failure

**Answer**
When you call `Clients.Group("room-42").SendAsync(...)` on a hub, SignalR looks up all connection IDs that belong to that group in the current process's in-memory dictionary and pushes the message to each one. In a single-server deployment this works fine. With two or more servers behind a load balancer — a common autoscale scenario — each server holds only the connections that happen to be routed to it. A user connected to Server A is invisible to Server B's group map, so a message sent on Server B simply never reaches that user. The Redis backplane solves this by turning each server into a pub/sub publisher and subscriber. When any server sends to a group it publishes the serialised message to a Redis channel named for that group; every server subscribed to that channel picks it up and delivers it to any local connections in that group. The net effect is that the group membership is still in-process, but message delivery is fanned out via Redis. The key production implication is that the backplane must be configured before any connections are accepted — adding it to a running cluster causes a brief window where some messages are lost. An alternative is sticky sessions (consistent hashing or ALB sticky cookies), which routes a client back to the same server on reconnect; this avoids the backplane cost but breaks if a server restarts or is replaced during an autoscale event.

---

## CQ5. When should you choose GraphQL over REST, and what is the N+1 problem in resolvers?

**Concepts**
- REST over-fetches when a response includes fields the client does not need
- REST under-fetches when assembling a view requires multiple sequential round trips
- GraphQL lets the client specify the exact shape of data in a single request
- N+1 problem: fetching a list of N items then issuing one database query per item to load a related entity
- DataLoader batches and deduplicates child queries into a single bulk fetch

**Answer**
REST is the right default for simple, stable, resource-oriented APIs where clients have predictable and consistent data needs. GraphQL earns its complexity when clients are diverse — mobile apps needing minimal payloads, desktop UIs needing rich aggregations — and when the backend team would otherwise maintain multiple REST endpoints or BFF (Backend for Frontend) layers to serve each client's exact shape. The trade-off is schema maintenance, tooling overhead, and the N+1 problem. In a GraphQL resolver tree, the `author` field resolver on a `Post` type might look correct in isolation: fetch the author by ID. But when the root query returns 50 posts, that resolver runs 50 times, issuing 50 separate `SELECT * FROM Authors WHERE Id = ?` queries — the N+1 pattern that also plagues naive ORM usage. HotChocolate's DataLoader addresses this by collecting all the author ID requests accumulated during one execution tick and issuing a single `SELECT * FROM Authors WHERE Id IN (...)` for the batch, then distributing the results back to each resolver. This is structurally equivalent to using `Include()` in Entity Framework for eager loading, but operates at the resolver graph level. The rule of thumb is: if your GraphQL resolvers hit the database without DataLoader you almost certainly have an N+1 problem.

---

## CQ6. Why does WebRTC still need a signaling server, and how does ASP.NET Core provide one?

**Concepts**
- WebRTC establishes peer-to-peer media streams, but peers must first exchange connection metadata out-of-band
- SDP (Session Description Protocol) describes codec capabilities and network addresses
- ICE candidates are potential network paths (host, server-reflexive via STUN, relay via TURN)
- Signaling is not standardised by WebRTC — any bidirectional channel works
- ASP.NET Core WebSockets or SignalR hubs are the most common signaling channel choices

**Answer**
WebRTC's peer-to-peer design means that once the connection is established, audio and video frames travel directly between browsers without touching your server. The bootstrapping problem is that each peer does not know the other's IP address, port, or supported codecs before the connection exists. They must exchange this information through an existing channel they both already trust — the signaling server. The signaling flow starts with one peer creating an SDP offer (a text document listing its supported codecs and candidate network addresses) and sending it to the other peer via the signaling channel. The second peer responds with an SDP answer. Simultaneously, each peer's ICE agent discovers candidate addresses — its local IP, its public IP via a STUN server, and a relay address via a TURN server if direct traversal fails — and sends each candidate to the other peer through the same signaling channel as they are discovered. Only after this exchange do the peers attempt direct UDP (or TCP fallback) connections to each other. ASP.NET Core fills the signaling role naturally: a SignalR hub can relay `offer`, `answer`, and `icecandidate` messages between two clients identified by their connection ID or a room code. The WebRTC spec deliberately leaves signaling unspecified so that any transport works; in practice SignalR is convenient because the same connection used for signaling can also carry application-level events (chat, participant roster updates) alongside the WebRTC bootstrapping messages.

---

## CQ7. How does HTTP/2 server push differ architecturally from SSE, and why is SSE still widely used?

**Concepts**
- HTTP/2 server push sends resources proactively via PUSH_PROMISE before the client requests them
- SSE is client-initiated: the browser opens a `GET` with `Accept: text/event-stream` and the server holds the response open
- HTTP/2 push is a page-load performance optimisation; SSE is an application-level event stream
- Chrome removed HTTP/2 push support in 2022 after studies showed minimal real-world benefit
- SSE works over both HTTP/1.1 and HTTP/2 connections; the browser's `EventSource` API handles automatic reconnect

**Answer**
HTTP/2 server push and SSE both deliver bytes from server to client without an explicit per-resource request, but their intent and mechanics are completely different. HTTP/2 push is a performance hint: when a browser requests `index.html`, the server immediately sends a PUSH_PROMISE frame for `app.js` and `styles.css`, pre-loading them into the browser cache before the HTML parser even encounters the `<script>` tag. The server makes a unilateral prediction about what the client will need. SSE is a deliberate subscription: the client sends a normal `GET` with `Accept: text/event-stream`, and the server responds with an open-ended chunked response, writing `data:` frames whenever new application events occur. The connection stays alive for as long as the subscription is active. Despite both enabling server-to-client data flow, SSE remains the more practical tool for streaming application events. Chrome dropped HTTP/2 push in 2022 because studies showed it rarely improved performance — the server frequently pushed resources the browser had already cached. SSE, by contrast, remains well-supported across all modern browsers. In ASP.NET Core with .NET 10, an SSE endpoint sets `Response.ContentType = "text/event-stream"` and `Response.Headers.CacheControl = "no-cache"`, then writes and flushes formatted event frames. SSE is also carried efficiently over HTTP/2 connections — each subscription occupies one multiplexed HTTP/2 stream, sharing the same underlying TCP connection with other requests.

---

## CQ8. What does SignalR add over raw WebSockets, and when should you use raw WebSockets instead?

**Concepts**
- SignalR wraps WebSockets with a hub protocol (JSON or MessagePack) for named-method dispatch
- Provides automatic client reconnect with a configurable retry policy
- Manages connection lifecycle via `OnConnectedAsync` and `OnDisconnectedAsync` hooks
- Supports strongly-typed hub clients through a generic `Hub<T>` base class
- Raw WebSockets are preferable when interoperating with non-SignalR peers or when binary framing control is required

**Answer**
A raw WebSocket gives you a bidirectional byte-or-text channel — you send frames and you receive frames. Mapping a frame to an action is entirely your responsibility. SignalR builds a complete RPC-style invocation layer on top of that channel using a hub protocol. When the JavaScript client calls `connection.invoke("SendMessage", roomId, text)`, SignalR serialises the method name and arguments into a JSON envelope (or MessagePack with `AddMessagePackProtocol()`), sends it over the WebSocket, and the hub's `SendMessage(string roomId, string text)` method is invoked with the deserialised arguments. The return value travels back through the same envelope. Beyond the protocol layer, SignalR adds automatic reconnect: when the WebSocket drops, the client retries according to a configurable schedule and fires your `onreconnected` callback when it succeeds. Strongly-typed hub clients (`Hub<IHubClient>`) let you call `Clients.All.ReceiveMessage(msg)` with compile-time checking rather than the stringly-typed `SendAsync("ReceiveMessage", msg)`. Raw WebSockets are the right choice when you need to interoperate with a peer that speaks its own subprotocol (a binary telemetry device, an external broker), when you need complete control over frame boundaries and binary layout, or when SignalR's negotiate round-trip and protocol framing add measurable overhead — for example in a high-frequency market-data feed where messages are fixed-format binary structs and every allocation matters.

---

## CQ9. How do SSE and long polling differ in HTTP mechanics, and how do load balancers treat each?

**Concepts**
- SSE: a single HTTP response is held open indefinitely; the server writes event frames as data arrives
- Long polling: each request completes with a response; the client immediately issues the next request
- Buffering proxies (Nginx default, IIS ARR) accumulate the SSE response body and never forward frames to the client
- `proxy_buffering off` (Nginx) or `X-Accel-Buffering: no` response header disables SSE buffering
- Long polling requests must complete before the load balancer's idle-connection timeout fires (typically 60–120 s)

**Answer**
SSE uses a single HTTP response that the server never closes. The server sets `Content-Type: text/event-stream` and writes individual event lines (`data: ...\n\n`) as application data arrives, flushing the response buffer after each. The TCP (or HTTP/2 stream) stays open for the lifetime of the subscription. Long polling mimics a stream by stretching the normal HTTP request-response cycle: the client issues a `GET`, the server holds the request open until it has data or a timeout fires, returns the response, and the client immediately issues the next `GET`. The two patterns look similar to the application but behave very differently under infrastructure. A buffering reverse proxy — Nginx with its default `proxy_buffering on`, or IIS Application Request Routing — accumulates the upstream response body until the upstream closes the connection. For SSE this means event frames are queued on the proxy and never reach the browser until the stream ends, which defeats the purpose entirely. The fix is `proxy_buffering off` in Nginx or including `X-Accel-Buffering: no` in the response headers so that Nginx honours the per-response override. Long polling is less affected by response buffering since each poll completes normally, but it is highly sensitive to idle-connection timeouts: if the server holds a request open for 90 seconds and the load balancer's idle timeout is 60 seconds, the proxy silently drops the connection. The server's configured long-poll timeout must always be shorter than every hop's idle timeout to prevent silent request loss.

---

## CQ10. When should you choose gRPC vs OData vs REST for a client-facing API?

**Concepts**
- REST: resource-oriented, HTTP verbs on stable URLs, fixed server-defined response shapes, widest client support
- OData: extends REST with `$filter`, `$select`, `$expand`, `$orderby` — client drives the query shape at runtime
- gRPC: typed Protobuf contracts, binary serialisation over HTTP/2, ideal for service-to-service; requires grpc-web for browsers
- Unrestricted OData `$expand` can traverse entity relationships and expose data the designer did not intend to share
- Decision axis: fixed contract vs dynamic query vs high-throughput typed RPC

**Answer**
REST is the sensible default for public HTTP APIs: it is understood by every HTTP client, maps naturally to CRUD operations on named resources, and delivers predictable, cacheable responses. Its limitation is fixed response shapes — the server decides what fields come back. OData extends REST by embedding a query language directly in the URL (`GET /api/products?$filter=price lt 50&$select=name,price&$expand=category`), letting clients drive filtering, projection, and entity-graph traversal at runtime. This is valuable when clients have heterogeneous query needs — reporting dashboards, Excel add-ins, admin UIs — and when maintaining a dedicated endpoint for every combination of filters would be impractical. The cost is a tightly coupled entity data model (EDM) and the security risk that an unconstrained `$expand` can walk foreign-key relationships and return entities the caller should not see. gRPC occupies a different position: it is optimised for typed, high-throughput communication between services that are both under your control. Protobuf contracts are defined in `.proto` files, generated code is strongly typed in both client and server, and binary serialisation is significantly more compact than JSON. Browser clients require a grpc-web proxy layer (Envoy or ASP.NET Core `UseGrpcWeb()`). The decision rule: choose REST for stable, cacheable HTTP APIs consumed by diverse clients; choose OData when clients legitimately need ad-hoc query capability over entity sets and you can restrict `$expand` depth and scope; choose gRPC for internal service-to-service calls where performance and contract rigidity matter more than broad client compatibility.

---

## CQ11. How does HotChocolate deliver GraphQL subscriptions, and how does this interact with SignalR auth in the same application?

**Concepts**
- HotChocolate supports subscriptions over WebSockets (`graphql-transport-ws` subprotocol) and SSE
- Application-level `ConnectionInit` auth check occurs in the first WebSocket message frame, not in the HTTP upgrade headers
- Both HotChocolate and SignalR pass through the standard ASP.NET Core `UseAuthentication()`/`UseAuthorization()` pipeline
- Route isolation is critical — HotChocolate maps to `/graphql`, SignalR hubs map to their own paths
- HotChocolate's `ConnectionInit` payload allows token delivery without the query-string exposure that SignalR requires

**Answer**
HotChocolate supports two subscription delivery modes in .NET 10: WebSockets using the `graphql-transport-ws` subprotocol, and SSE using a chunked `text/event-stream` response. In either case the connection enters the standard ASP.NET Core middleware pipeline, so `UseAuthentication()` and `UseAuthorization()` apply before HotChocolate handles the request. For WebSocket subscriptions, HotChocolate performs an application-level authentication step during the `ConnectionInit` message — the first frame the client sends after the HTTP upgrade succeeds. This allows the JWT to be passed as a payload field inside the WebSocket frame rather than in the URL query string, which is a small but meaningful security improvement over SignalR's forced `?access_token=` approach. When SignalR hubs and HotChocolate subscriptions coexist in the same application, they share the same `AddAuthentication()`/`AddJwtBearer()` registration, so both validate tokens against the same issuer and signing key. The key configuration concern is routing: SignalR hubs are registered at their own paths (`MapHub<ChatHub>("/hub")`), and HotChocolate at `/graphql`. Path collisions cause one to shadow the other. A common integration gotcha is omitting `AddHttpContextAccessor()` from the service registration — HotChocolate needs `IHttpContextAccessor` to read `HttpContext.User` inside resolver authorisation checks, and without it `[Authorize]` on a subscription type silently passes all connections rather than rejecting unauthenticated ones.

---

## CQ12. What happens to SignalR group membership when a client reconnects, and how should applications handle it?

**Concepts**
- A reconnected SignalR client receives a brand-new connection ID; the previous ID is permanently discarded
- `OnDisconnectedAsync` fires for the lost connection; `OnConnectedAsync` fires for the new one
- In-memory group membership is keyed by connection ID and is lost when the old connection is released
- Group membership must be stored in a persistent store (database, Redis) keyed by a stable user identity
- `Context.UserIdentifier` provides a stable identifier across reconnects when JWT authentication is in use

**Answer**
SignalR's transport layer is connection-oriented, and each connection carries a server-assigned connection ID that is unique and ephemeral. When a client's network drops and the SignalR JavaScript client automatically reconnects, it goes through a full new negotiate-and-upgrade cycle and receives a fresh connection ID. From the server's perspective, the old connection was lost (firing `OnDisconnectedAsync`) and a completely new connection arrived (firing `OnConnectedAsync`). Any group memberships that were associated with the old connection ID — stored in SignalR's in-process `IGroupManager` dictionary — are gone. The new connection ID is not in any group. An application that calls `Groups.AddToGroupAsync(Context.ConnectionId, "room-42")` only inside the hub method that handles an explicit join request will silently drop the user from all groups whenever a network blip occurs. The correct pattern stores group membership in a persistent store keyed by a stable identity, not by connection ID. With JWT authentication, `Context.UserIdentifier` resolves to the `sub` or `nameidentifier` claim and remains stable across reconnects. In `OnConnectedAsync`, the hub queries the store for all groups the authenticated user belongs to and calls `Groups.AddToGroupAsync` for each. The JavaScript client's `onreconnected` callback is the right place to trigger any UI refresh that signals the user the connection was briefly lost, because by the time that callback fires the server has already completed `OnConnectedAsync` and re-added the user to all their groups.

---

## CQ13. How does HTTP/3 (QUIC) benefit gRPC, and what does ASP.NET Core Kestrel support in .NET 10?

**Concepts**
- HTTP/3 replaces TCP with QUIC — a UDP-based transport with per-stream reliable delivery
- QUIC eliminates TCP head-of-line blocking: a lost packet stalls only its own stream, not all concurrent streams
- QUIC integrates TLS 1.3 into the handshake — 1-RTT connection establishment (0-RTT on resumption)
- QUIC connection migration allows IP address changes without a new handshake
- Kestrel in .NET 10 enables HTTP/3 via `HttpProtocols.Http1AndHttp2AndHttp3`; TLS is mandatory

**Answer**
gRPC already multiplexes multiple RPC calls over one HTTP/2 connection, eliminating the per-request TCP overhead of HTTP/1.1. The remaining bottleneck is TCP itself: if a single TCP segment is lost, all streams on that connection stall while the OS waits for retransmission — TCP head-of-line blocking. HTTP/3 addresses this by replacing TCP with QUIC, a UDP-based transport that implements reliable ordered delivery independently per stream. A dropped UDP datagram pauses only the one QUIC stream it belongs to; all other concurrent gRPC calls continue unaffected. For a gRPC service handling dozens of simultaneous streaming RPCs, this is a meaningful latency improvement on networks with occasional packet loss. QUIC also embeds TLS 1.3 directly into its handshake, reducing new connection establishment to one round trip (and zero with 0-RTT session resumption for returning clients). Connection migration is another benefit for mobile clients: when a device switches from Wi-Fi to cellular and its IP address changes, QUIC can migrate the existing connection to the new path without a new handshake, keeping long-lived streaming RPCs alive across the transition. In ASP.NET Core with .NET 10, HTTP/3 is enabled on Kestrel by setting `ListenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3` and providing a valid TLS certificate — HTTP/3 cleartext (h3c) is not supported. Kestrel advertises `Alt-Svc: h3` in response headers, and the .NET 10 gRPC client (`Grpc.Net.Client`) negotiates HTTP/3 automatically when the server advertises it and the host platform supports QUIC.

---

## CQ14. Once WebRTC ICE negotiation succeeds, what role does the signaling server play?

**Concepts**
- ICE selects a working direct or TURN-relayed UDP/TCP path between peers; DTLS-SRTP secures the media channel
- After ICE completes, media frames bypass the signaling server entirely — no media traffic touches it
- The signaling connection stays open for application-level events: mute, participant roster, screen-share, call end
- WebSockets (via SignalR) are the preferred signaling transport because they are bidirectional and remain open cheaply
- SSE can carry server-to-client signaling but requires separate POST requests for client-to-server messages

**Answer**
WebRTC's peer-to-peer design means the signaling server is load-critical only during the offer/answer SDP exchange and ICE candidate relay phase. The moment ICE selects a working candidate pair and the DTLS handshake completes, audio and video frames flow directly between the two peers' UDP (or TCP-fallback) sockets. The signaling server is completely out of the media path and receives zero media traffic regardless of how long the call lasts. The signaling server does not become useless after ICE succeeds, however. Application-level control events still need a channel: a participant clicking mute must notify the remote peer to update its track state; screen-share start, participant join and leave, recording triggers, and call termination all require a message channel between the application layer and the peers. A WebSocket connection via SignalR is the natural choice for this role because it is bidirectional, stays alive cheaply as an idle TCP connection after ICE completes, and can carry both the WebRTC bootstrapping messages during setup (offer, answer, ICE candidates) and the application control events during the live call without any protocol change. SSE is technically capable of delivering the server-to-client half of the signaling — relaying the remote SDP answer and ICE candidates — but the client-to-server direction (uploading the local offer and candidates) would require separate HTTP POST requests, making SSE an awkward fit for a protocol that is inherently bidirectional. The practical design guideline is to keep the SignalR hub connection open for the full call duration as the application control plane, and let the WebRTC `RTCPeerConnection` manage all media independently.

---

## CQ15. How do you secure OData endpoints in ASP.NET Core, and what is the $expand authorization risk?

**Concepts**
- `[Authorize]` on an OData controller restricts unauthenticated access via the standard ASP.NET Core middleware
- OData's `$expand` follows navigation properties and can return deeply nested related entities in a single response
- Row-level filtering must be applied to the IQueryable before OData query composition executes
- `ODataValidationSettings` with `AllowedQueryOptions` and `MaxExpansionDepth` restrict client query capabilities
- Unrestricted `$expand` on public endpoints risks traversing entity relationships the designer never intended to expose

**Answer**
Applying `[Authorize]` at the controller or action level in an ASP.NET Core OData service works identically to standard MVC — the authorization middleware validates the policy before the OData pipeline processes the URL query options. The harder security problem lies in what an authenticated user can do after passing that check. OData's `$expand` instructs the server to include related entities inline: `GET /api/orders?$expand=customer($expand=paymentMethods)` can, in a single request, return order records, the associated customer records, and each customer's stored payment methods — all resolved by Entity Framework navigations from a single IQueryable. If the IQueryable is simply `_context.Orders` with no ownership filter, an authenticated user who should only see their own orders can use `$expand` to walk the entity graph and read data that belongs to other tenants or users. The mitigation requires two layers. First, apply row-level filtering before handing the IQueryable to the OData pipeline: `return _context.Orders.Where(o => o.UserId == currentUserId)`. This ensures that `$expand` can only traverse relationships reachable from the already-filtered root set. Second, restrict query options using `ODataValidationSettings` in the controller action or via a global `EnableQueryAttribute` configuration: set `AllowedQueryOptions` to exclude `$expand` entirely for endpoints where it is not needed, or set `MaxExpansionDepth = 1` to prevent multi-level traversal. For internet-facing OData services, auditing the entity data model for navigation properties that should not be traversable by clients — and removing those navigations from the EDM if they are not needed — is the most robust long-term control.

---

## CQ16. What is the server resource cost of SignalR long polling vs WebSocket connections?

**Concepts**
- An idle WebSocket connection holds only a socket handle and the hub's connection context — no active request
- A pending long-poll request holds a `TaskCompletionSource`, a live ASP.NET Core request context, and a response buffer
- Async long polling does not block OS threads but does accumulate live Task continuations and middleware state in memory
- 10,000 concurrent long-poll connections generate significantly more heap pressure than 10,000 idle WebSocket connections
- Long polling also generates higher network and CPU overhead — a full HTTP request-response pair per poll cycle

**Answer**
ASP.NET Core's async I/O model means that a long-poll handler written with `await channel.Reader.WaitToReadAsync(cancellationToken)` does not block an OS thread — the thread is returned to the pool while the continuation waits for data. However, each pending long-poll request still holds a live ASP.NET Core `HttpContext`, a `TaskCompletionSource` or channel reader continuation, a response stream buffer, and the state of every middleware that ran before the handler. At 10,000 concurrent long-poll connections, that is 10,000 live request contexts allocated on the heap, 10,000 pending continuations, and 10,000 open response streams — measurable GC pressure and memory usage compared with 10,000 idle WebSocket connections, where the server holds only a socket handle and the hub's lean connection context object. Long polling also generates significantly more CPU and network traffic: each poll cycle involves a complete HTTP request-response pair including headers. On HTTP/1.1, headers are re-transmitted in full with every poll; even on HTTP/2 with HPACK compression, the overhead is non-trivial at high connection counts. WebSockets amortise the HTTP upgrade cost over the entire connection lifetime and then exchange minimal two-to-ten-byte framing overhead per message. The practical operational guidance is to ensure that production load balancers and reverse proxies are configured to pass WebSocket upgrades, and to treat long polling as a genuine last resort for environments where the WebSocket upgrade cannot pass — not as an acceptable equivalent. The performance difference is most visible under load with many concurrent idle connections, which is the normal state for notification-style real-time features.

---
