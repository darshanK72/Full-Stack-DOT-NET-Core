# 09. Real-Time & Communication Protocols

Beyond **REST** — hands-on ASP.NET Core projects for **WebSockets**, **SignalR**, **GraphQL**, **gRPC**, **WebRTC signaling**, and related transport choices. Read after core Web API fundamentals; several topics assume **08. ASP.NET Core Identity** for authenticated real-time connections.

## Prerequisites

- **[05. ASP.NET Core](../05.%20ASP.NET%20Core/)** — middleware, DI, routing, hosting
- **[07. ASP.Net Core Web API](../07.%20ASP.Net%20Core%20Web%20API/)** — REST controllers, Swagger, CORS
- **[08. ASP.NET Core Identity](../08.%20ASP.NET%20Core%20Identity/)** — recommended before SignalR auth topics

## Related topics elsewhere

| Topic | Also covered in |
|-------|-----------------|
| WebSockets (transport layer) | **05** ch.15 — pipeline-level introduction |
| GraphQL (API style) | **07** ch.15 — Web API integration angle |
| gRPC (public/client API) | **07** ch.16 — gRPC Web APIs |
| Real-time Razor UI | **06** ch.15 — SignalR with MVC views |
| gRPC (service-to-service) | **10** ch.13 — microservices internal communication |
| MQTT / IoT messaging | **11** Azure Services — IoT Hub & cloud messaging |

## Topics (work in order)

| # | Topic | Focus |
|---|--------|--------|
| 01 | [HTTP Deep Dive](./01.%20HTTP%20Deep%20Dive/) | Methods, headers, status codes, HTTP/2 basics |
| 02 | [WebSockets in ASP.NET Core](./02.%20WebSockets%20in%20ASP.NET%20Core/) | Raw WebSocket middleware & handlers |
| 03 | [Server-Sent Events (SSE)](./03.%20Server-Sent%20Events%20(SSE)/) | One-way server → client streaming |
| 04 | [Long Polling & Transport Fallbacks](./04.%20Long%20Polling%20%26%20Transport%20Fallbacks/) | When WebSockets are unavailable |
| 05 | [SignalR Fundamentals](./05.%20SignalR%20Fundamentals/) | Connections, hubs, negotiation |
| 06 | [SignalR Hubs, Groups & Clients](./06.%20SignalR%20Hubs%2C%20Groups%20%26%20Clients/) | Broadcast, groups, strongly typed hubs |
| 07 | [SignalR with Authentication](./07.%20SignalR%20with%20Authentication/) | JWT/cookie auth for hub connections |
| 08 | [WebRTC Signaling with ASP.NET Core](./08.%20WebRTC%20Signaling%20with%20ASP.NET%20Core/) | Signaling server (often via SignalR) |
| 09 | [GraphQL with HotChocolate](./09.%20GraphQL%20with%20HotChocolate/) | Schema, queries, mutations, subscriptions |
| 10 | [gRPC for Client-Facing APIs](./10.%20gRPC%20for%20Client-Facing%20APIs/) | Public gRPC / gRPC-Web endpoints |
| 11 | [OData APIs](./11.%20OData%20APIs/) | Queryable REST with `$filter`, `$select` |
| 12 | [Choosing a Protocol](./12.%20Choosing%20a%20Protocol/) | REST vs GraphQL vs SignalR vs gRPC decision guide |

## Next module

**[10. ASP.NET Microservices](../10.%20ASP.NET%20Microservices/)** — service-to-service patterns including internal gRPC, messaging, and resilience.
