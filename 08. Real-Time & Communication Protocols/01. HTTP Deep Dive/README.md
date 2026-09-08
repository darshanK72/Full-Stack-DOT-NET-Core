# HTTP Deep Dive — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — HTTP Deep Dive (Interview Traps)](#gotchas--http-deep-dive-interview-traps)

---

## Gotchas — HTTP Deep Dive (Interview Traps)

---

#### Gotcha 1. HTTP/2 Does Not Eliminate Head-of-Line Blocking at the TCP Layer

**Concepts**
- HTTP/2 multiplexes multiple streams over one TCP connection
- A single lost TCP segment stalls all streams until the OS retransmits it
- This is TCP-level HOL blocking, distinct from HTTP/1.1 request-level HOL blocking
- HTTP/3 (QUIC) solves TCP HOL blocking with per-stream reliable delivery over UDP

**Answer**

HTTP/2 solves the HTTP/1.1 problem of one slow response blocking the next request on the same connection, but it does not solve TCP head-of-line blocking. When a TCP segment carrying frames for stream 3 is lost, the OS must retransmit it before delivering any subsequent segment — even segments for streams 1, 2, and 4 that arrived in order. All multiplexed HTTP/2 streams on that connection stall waiting for the retransmission. This matters most on high-latency or lossy networks. HTTP/3 replaces TCP with QUIC, which tracks reliability independently per stream, so a lost datagram stalls only its own stream and leaves all other concurrent streams unaffected.

---

#### Gotcha 2. HTTP/1.1 Keep-Alive Reuses Connections Sequentially, Not in Parallel

**Concepts**
- Browsers open 6–8 TCP connections per origin to work around HTTP/1.1 serialisation
- Keep-Alive prevents the TCP handshake cost but still serialises requests per connection
- A slow response on one connection blocks the next request queued behind it
- HTTP/2 eliminates the per-host connection limit with true stream multiplexing

**Answer**

HTTP/1.1 persistent connections reduce handshake overhead by reusing a TCP connection for multiple sequential requests, but each connection still handles only one active request-response pair at a time. Browsers work around this by opening up to six parallel connections per origin. When a page makes twenty parallel sub-requests, fourteen queue behind the first six. HTTP/2 sidesteps this entirely by multiplexing all streams over a single connection with no in-connection serialisation. The practical trap is disabling Keep-Alive during debugging and then measuring performance that looks far worse than production, or the opposite — seeing HTTP/1.1 perform well in test because the concurrent request count is below the per-host limit and the queuing effect never surfaces.

---

#### Gotcha 3. Chunked Transfer-Encoding and Content-Length Cannot Both Appear in the Same Response

**Concepts**
- Content-Length declares the exact body size upfront before any bytes are written
- Transfer-Encoding: chunked streams the body in variable-length segments without a prior size
- Sending both is a protocol violation per RFC 7230; clients must reject or ignore one
- ASP.NET Core sets one automatically; manually setting both causes undefined client behavior

**Answer**

Content-Length and Transfer-Encoding: chunked are mutually exclusive. Content-Length tells the client exactly how many bytes to expect; chunked encoding tells the client to read chunks until a zero-length terminator arrives. Including both in the same response is a protocol violation. ASP.NET Core sets Transfer-Encoding: chunked automatically when you write to the response without first assigning `Response.ContentLength`, as happens with `Response.BodyWriter` or streaming middleware. A common trap is setting `Response.ContentLength` explicitly and then writing more or fewer bytes than declared — clients either hang waiting for the promised bytes or silently discard the surplus, producing corrupted or incomplete responses.

---

#### Gotcha 4. A 301 Redirect Is Cached Permanently by the Browser and Cannot Be Recalled

**Concepts**
- 301 Moved Permanently is cached indefinitely by browsers and CDNs without an Expires header
- 302 Found is not cached by default and is safe for reversible redirects
- Once a user's browser caches a 301, the server cannot correct it without clearing the cache
- 307 and 308 preserve the original HTTP method; 301 and 302 allow POST-to-GET downgrade

**Answer**

A 301 redirect is cached permanently by browsers with no expiry. If you redirect `/old-path` to `/new-path` with a 301 and later need to change the destination, users who visited before the change will replay the stale redirect forever — their browser never reaches the server to discover the update. This cannot be corrected server-side; the only mitigation is instructing users to clear their cache, which is rarely practical. For any redirect that might change, use 302 or 307 instead. The method-preservation distinction is also frequently missed: 301 and 302 permit the client to downgrade a POST to a GET on the redirected request, while 307 (Temporary) and 308 (Permanent) guarantee the original method is preserved — critical when redirecting form submission endpoints.

---

#### Gotcha 5. Cache-Control max-age Takes Precedence Over Expires When Both Headers Are Present

**Concepts**
- Expires is an HTTP/1.0 header specifying an absolute expiry date
- Cache-Control max-age is HTTP/1.1 and overrides Expires in compliant clients
- no-cache means revalidate before use; no-store means do not store at all
- A sensitive endpoint returning only no-cache may still be stored in browser history

**Answer**

When both `Cache-Control: max-age=3600` and `Expires: <date>` appear in a response, HTTP/1.1 caches ignore Expires and use max-age. Changing one without updating the other leaves inconsistent values that confuse HTTP/1.0-only intermediaries. The more dangerous confusion is between `no-cache` and `no-store`: `no-cache` means "store it but always revalidate with the origin before serving," while `no-store` means "do not store this response anywhere." A sensitive endpoint that returns only `Cache-Control: no-cache` may still appear in browser history or be retained in shared proxy caches; `no-store` is required to prevent any storage of the response content.

---

#### Gotcha 6. An Inconsistent ETag Generator Defeats All Conditional GET Caching

**Concepts**
- ETag is a server-generated version token that clients send back in If-None-Match
- Last-Modified is a date-based alternative; If-None-Match takes precedence when both are sent
- A 304 Not Modified response must not include a body
- ETags computed from volatile state (timestamps, process IDs) invalidate unnecessarily

**Answer**

Conditional GET allows a client to revalidate a cached resource without downloading the full body again. The server issues an ETag or Last-Modified header; the client includes If-None-Match or If-Modified-Since on subsequent requests. If the resource is unchanged, the server returns 304 Not Modified with no body, saving bandwidth. The trap is on the server side: if ETag generation depends on volatile state — a timestamp that changes on each process restart, or a hash that includes transient metadata — clients receive 200 with a full body even when the data has not changed, negating all caching benefit. Additionally, a 304 response must contain no message body; including one causes some clients to hang waiting for data that will never arrive.

---

#### Gotcha 7. CORS Preflight Fails When OPTIONS Is Not Handled by the Endpoint

**Concepts**
- Non-simple cross-origin requests trigger an automatic OPTIONS preflight request
- The browser shows a CORS error and never sends the actual request if preflight fails
- Access-Control-Allow-Origin: * is rejected by the browser when credentials are included
- UseCors() middleware must be registered before routing and endpoint middleware

**Answer**

CORS preflight is an OPTIONS request the browser sends automatically before any non-simple cross-origin request. The server must respond with `Access-Control-Allow-Origin`, `Access-Control-Allow-Methods`, and `Access-Control-Allow-Headers` matching the incoming request. A common trap is adding CORS headers to the GET or POST handler but not handling OPTIONS — the preflight fails and the developer sees only a CORS error in the browser console with no trace of the actual request, because the browser never sent it. In ASP.NET Core, `app.UseCors()` must appear before `app.UseRouting()` and endpoint middleware in the pipeline. Using `Access-Control-Allow-Origin: *` on an endpoint that also sets `AllowCredentials()` is rejected outright by browsers; a specific origin must be named when credentials are permitted.

---

#### Gotcha 8. A Server That Ignores Range Requests and Returns 200 Breaks Media Seeking

**Concepts**
- Range: bytes=0-1023 requests a specific byte slice of a resource
- Server must respond with 206 Partial Content and Content-Range header
- Returning 200 with the full body signals that range requests are not supported
- Accept-Ranges: bytes in response headers advertises range-request capability

**Answer**

HTTP range requests let clients resume interrupted downloads or seek within media files by specifying a byte range. The client sends `Range: bytes=500000-999999` and expects `206 Partial Content` with `Content-Range: bytes 500000-999999/5000000` and only the requested bytes. A server that ignores the Range header and returns 200 with the full body forces media players and download managers to restart from the beginning on every seek operation. In ASP.NET Core, `UseStaticFiles()` handles range requests correctly. Custom file endpoints must implement range handling explicitly or return a `PhysicalFileResult`, which handles it automatically. Returning 200 when 206 is expected also signals to clients that the server does not support ranges, permanently disabling seek functionality for that resource.

---

#### Gotcha 9. HTTP/3 Requires TLS — Cleartext QUIC Is Not Supported by Any Browser

**Concepts**
- HTTP/3 runs over QUIC, which integrates TLS 1.3 directly into the handshake
- No browser supports unencrypted QUIC (h3c); a valid certificate is always required
- QUIC uses UDP port 443; corporate firewalls blocking UDP 443 cause silent fallback to TCP
- Kestrel advertises HTTP/3 availability via Alt-Svc response header, not via configuration push

**Answer**

HTTP/3 binds TLS 1.3 directly into the QUIC handshake — there is no unencrypted HTTP/3 mode available in any browser. This means HTTP/3 requires a valid TLS certificate even in local development. In ASP.NET Core with .NET 10, HTTP/3 is enabled by setting `HttpProtocols.Http1AndHttp2AndHttp3` on Kestrel's listen options alongside a TLS certificate; Kestrel then includes `Alt-Svc: h3=":443"` in responses so that clients attempt QUIC on the next request. The practical trap is that corporate firewalls frequently block UDP port 443, causing clients to silently fall back to TCP without any error — HTTP/3 must be treated as a progressive enhancement rather than a guaranteed transport that you can rely on or test against in controlled environments only.

---

#### Gotcha 10. HTTP Request Smuggling Exploits Disagreement Between Proxy and Backend on Request Boundaries

**Concepts**
- CL.TE: proxy uses Content-Length, backend uses Transfer-Encoding
- TE.CL: proxy uses Transfer-Encoding, backend uses Content-Length
- Attacker crafts a request whose body prefix is interpreted as the start of the next request by the backend
- Enables authentication bypass, cache poisoning, and request hijacking against other users

**Answer**

HTTP request smuggling occurs when a frontend proxy and a backend server parse conflicting `Content-Length` and `Transfer-Encoding` headers differently, causing them to disagree on where one request ends and the next begins. An attacker crafts a request whose body, as seen by the backend, is a partial HTTP request — effectively injecting a poisoned prefix into the next legitimate user's request pipeline. This can bypass authentication, poison shared HTTP caches, or expose other users' responses. The mitigation is to ensure that Kestrel and any upstream proxy reject or normalise requests with ambiguous framing headers, keep proxy and application on the same HTTP version, and configure proxies to strip Transfer-Encoding headers before forwarding. Running Kestrel directly without a frontend proxy eliminates the parsing disagreement entirely for simple deployments.

---
