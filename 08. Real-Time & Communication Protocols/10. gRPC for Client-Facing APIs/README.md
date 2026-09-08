# gRPC for Client-Facing APIs — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — gRPC for Client-Facing APIs (Interview Traps)](#gotchas--grpc-for-client-facing-apis-interview-traps)

---

## Gotchas — gRPC for Client-Facing APIs (Interview Traps)

---

#### Gotcha 1. gRPC Requires HTTP/2 End-to-End — A Single HTTP/1.1 Hop Breaks the Connection

**Concepts**
- gRPC framing relies on HTTP/2 binary multiplexing and trailer frames for status codes
- HTTP/1.1 does not support trailers; gRPC status codes cannot be delivered over HTTP/1.1
- AWS ALB by default forwards to targets over HTTP/1.1, breaking gRPC even when the listener is HTTP/2
- gRPC requests fail with a protocol error, not a meaningful application error

**Answer**

gRPC encodes its status code and trailing metadata in HTTP/2 trailer frames — a feature that simply does not exist in HTTP/1.1. Any hop in the network path that downgrades the connection to HTTP/1.1 silently breaks gRPC. AWS Application Load Balancer is the most common trap: ALB supports HTTP/2 on its listener, but by default it forwards requests to backend targets over HTTP/1.1. The gRPC client receives a protocol-level error rather than a gRPC status code, making the failure difficult to diagnose. The fix is to use a Network Load Balancer (which passes TCP through transparently) or to enable HTTP/2 on the ALB target group. The same issue affects any Nginx configuration that uses the default HTTP/1.1 proxy connection to the upstream.

---

#### Gotcha 2. Browser Clients Cannot Use Native gRPC Without a grpc-web Proxy Layer

**Concepts**
- Browsers expose the Fetch API which does not support HTTP/2 trailer frames
- Native gRPC is blocked at the browser API boundary, not by a protocol restriction
- grpc-web remaps trailers into a body-encoded frame readable by browsers
- Envoy Proxy and ASP.NET Core UseGrpcWeb() are the two common grpc-web proxy options

**Answer**

Browser JavaScript cannot make native gRPC calls because the Fetch API and XHR do not expose HTTP/2 trailer frames, which is where gRPC encodes its status code and trailing metadata. The `grpc-web` protocol solves this by rewriting responses: trailers are embedded as a special framing byte sequence at the end of the HTTP/1.1 or HTTP/2 response body, where the browser can read them. ASP.NET Core supports grpc-web natively via the `Grpc.AspNetCore.Web` package: calling `app.UseGrpcWeb()` and marking individual services with `EnableGrpcWeb()` enables browser clients to call gRPC endpoints using the `@improbable-eng/grpc-web` or `grpc-web` npm client. Grpc-web does not support client streaming or bidirectional streaming — only unary and server-streaming calls are compatible, which limits some API patterns.

---

#### Gotcha 3. Removing or Renumbering a Protobuf Field Number Breaks Binary Compatibility

**Concepts**
- Protobuf identifies fields by their number, not their name
- A client compiled against field number 3 = "email" will read any future field 3 as email, regardless of rename
- Removing a field and reusing its number for a different field silently corrupts deserialisation
- Removed fields must be reserved using the reserved keyword to prevent number reuse

**Answer**

Protobuf serialisation uses field numbers, not names, for encoding. The name of a field in a `.proto` file is irrelevant to the binary format; only the number matters. If field number 3 was `string email = 3;` and is removed, then a new field `int32 age = 3;` is added, any client compiled against the old contract that receives a message with field 3 will interpret the age bytes as a UTF-8 string, producing garbage. The Proto specification requires removed field numbers (and names) to be marked as `reserved 3; reserved "email";` to prevent future reuse. Generated code enforces this at compile time. This is the most common backward-compatibility mistake in gRPC API evolution and is easily missed by developers who come from JSON REST APIs where removing a field is trivial.

---

#### Gotcha 4. Server Streaming RPCs Left Open Silently Consume Server Resources

**Concepts**
- A server streaming RPC holds an open HTTP/2 stream until the server writes the final frame
- A client that navigates away without cancelling the call leaves the stream alive on the server
- The server's IAsyncEnumerable or IServerStreamWriter continues running until cancellation
- CancellationToken from the gRPC ServerCallContext must be observed in the streaming loop

**Answer**

In a server streaming RPC, the server writes a sequence of response messages until it calls `responseStream.WriteAsync` for the last time and the method returns. If the client disconnects — browser navigation, process kill, timeout — the underlying HTTP/2 stream is closed, and `ServerCallContext.CancellationToken` is signalled. A streaming method that does not observe this token continues executing the streaming loop, producing messages that are immediately dropped because the stream is closed. These orphaned tasks consume CPU and memory proportionally to their computational intensity. The streaming loop must pass `context.CancellationToken` to every `await` call — including `responseStream.WriteAsync`, database queries, and any `Task.Delay` — so the operation terminates cleanly when the client disconnects.

---

#### Gotcha 5. Deadline Propagation Must Be Explicit — Child Service Calls Do Not Inherit the Parent Deadline

**Concepts**
- A gRPC call carries a deadline that the server can check via ServerCallContext.Deadline
- Calling an internal gRPC service from within a gRPC handler does not automatically propagate the deadline
- The child call must be given a deadline equal to or shorter than the remaining parent deadline
- Without propagation, a parent with a 500 ms deadline can trigger child calls that run for seconds

**Answer**

gRPC deadlines allow the caller to specify the absolute time by which the call must complete. When server A handles a gRPC call and makes an outgoing gRPC call to server B as part of handling it, the deadline is not propagated automatically. The outgoing call to B uses its own default deadline (or none at all) unless the developer explicitly sets it. If the caller gave A a 500 ms deadline and A calls B with a 30-second timeout, A will time out and cancel its response to the caller after 500 ms, but B continues processing for up to 30 seconds, wasting resources on work whose result will never be used. The correct pattern is to propagate the deadline: `callOptions.WithDeadline(context.Deadline)` when building the child call options. The `Grpc.Net.ClientFactory` and gRPC interceptors can automate this propagation.

---

#### Gotcha 6. gRPC Status Codes Are Not a Simple 1:1 Mapping to HTTP Status Codes

**Concepts**
- gRPC defines its own 17 status codes (OK, NOT_FOUND, PERMISSION_DENIED, UNAVAILABLE, etc.)
- Mapping gRPC NOT_FOUND to HTTP 404 is reasonable; UNAVAILABLE does not cleanly map to HTTP 503
- grpc-web clients see HTTP 200 for all gRPC calls; the gRPC status is in the body trailer
- REST clients monitoring HTTP status codes will always see 200 for gRPC calls, missing real errors

**Answer**

gRPC has its own set of 17 status codes defined in the gRPC specification that do not have a one-to-one mapping to HTTP status codes. `NOT_FOUND` maps naturally to 404, but `UNAVAILABLE` (transient failure, safe to retry) is different from HTTP 503 Service Unavailable in semantics. `RESOURCE_EXHAUSTED` (quota exceeded) loosely maps to HTTP 429 but carries different retry guidance. Applications that build error handling logic by mapping gRPC status codes to HTTP codes must make deliberate, documented choices for each mapping. More importantly, monitoring infrastructure that alerts on HTTP 4xx/5xx counts will see zero errors for gRPC services, since all responses are HTTP 200 — the gRPC status is inside the response trailer. gRPC-aware monitoring must parse the `grpc-status` trailer value.

---

#### Gotcha 7. Server Reflection Must Be Enabled Separately for Tooling Like Postman and gRPCurl

**Concepts**
- gRPC server reflection allows clients to discover available services and methods at runtime
- Reflection is not enabled by default in Grpc.AspNetCore for security reasons
- Postman, gRPCurl, and Grpc.UI all require reflection or a .proto file to call gRPC endpoints
- Reflection should be disabled in production or restricted to authorised clients

**Answer**

Tools like Postman, gRPCurl, and Grpc.UI discover a gRPC server's available services, methods, and message shapes through the gRPC Server Reflection API. This API is not enabled by default in ASP.NET Core's `Grpc.AspNetCore` because it exposes the full API schema — including internal services — to any client that can reach the endpoint. Developers who set up a gRPC service and try to call it from Postman receive a reflection error and wonder why the endpoint is unreachable. The fix is to add the `Grpc.AspNetCore.Server.Reflection` package and call `app.MapGrpcReflectionService()` in development environments. In production, reflection should either be disabled entirely or protected by authentication to prevent schema discovery by unauthorised clients.

---

#### Gotcha 8. MaxReceiveMessageSize Default of 4 MB Rejects Large Protobuf Payloads Without a Clear Error

**Concepts**
- GrpcServiceOptions.MaxReceiveMessageSize defaults to 4 MB per message
- Messages exceeding the limit are rejected with status RESOURCE_EXHAUSTED
- The default protects against unintended large payloads but surprises developers sending bulk data
- For large data transfer, server streaming or chunking the payload is preferred over raising the limit

**Answer**

ASP.NET Core's gRPC server enforces a maximum incoming message size of 4 MB by default. A client that sends a large Protobuf-serialised request — a bulk data import, a large file embedded in a request field — receives a `RESOURCE_EXHAUSTED` gRPC status with a "Received message exceeds the maximum configured message size" error. The limit can be raised with `AddGrpc(options => options.MaxReceiveMessageSize = 100 * 1024 * 1024)`, but raising it without a bounded cap is a denial-of-service risk. The better architecture for large data is to use a server streaming RPC or a client streaming RPC that sends the data in chunks, keeping each individual message well within the default size. Embedding large binary blobs directly in Protobuf messages is generally poor design.

---

#### Gotcha 9. Unary RPC Is Not Appropriate for Large Response Sets That Should Be Streamed

**Concepts**
- A unary RPC buffers the entire response in memory before sending it to the client
- Returning a list of 100,000 items as a repeated field in a single unary response causes memory pressure
- Server streaming RPC yields items incrementally with backpressure from the HTTP/2 flow control
- Choosing unary vs streaming affects both server memory and client time-to-first-byte

**Answer**

A unary gRPC RPC collects the entire response — including any `repeated` fields — into a single Protobuf message and sends it as one HTTP/2 DATA frame sequence. For large result sets, this means the server must hold all items in memory before sending the first byte to the client. A response with 100,000 items serialised to a 50 MB Protobuf message causes 50 MB of server memory allocation per concurrent request and forces the client to wait for all 100,000 items before processing the first one. A server streaming RPC writes items to the `IServerStreamWriter` incrementally, and HTTP/2 flow control provides backpressure — the server only advances as fast as the client can consume. APIs that return variable-length or potentially large result sets should use server streaming to provide both better memory efficiency and lower time-to-first-byte.

---

#### Gotcha 10. gRPC Interceptors Are Separate from ASP.NET Core Middleware — One Does Not Replace the Other

**Concepts**
- ASP.NET Core middleware runs in the HTTP pipeline before the gRPC handler
- gRPC interceptors run inside the gRPC framework layer, after HTTP middleware but before hub method execution
- Authentication and HTTPS termination belong in middleware; logging and error handling can use either
- Applying the wrong abstraction leads to concerns executing at the wrong layer

**Answer**

ASP.NET Core hosts gRPC services on top of the HTTP pipeline. Middleware such as `UseAuthentication()`, `UseAuthorization()`, and HTTPS termination runs before the gRPC handler receives the call. gRPC interceptors — registered with `AddGrpc().AddInterceptor<T>()` — execute inside the gRPC framework after middleware has run and the request has been matched to a service method. An interceptor that attempts to read `HttpContext.User` before `UseAuthentication()` has run will find an unauthenticated principal because it is operating after middleware but its results depend on middleware having run first. Authentication must remain in middleware. Logging and error normalisation can be placed in either interceptors or middleware; interceptors have the advantage of access to gRPC-specific context (method name, request/response message types), while middleware has broader HTTP context visibility.

---
