# SignalR with Authentication — Interview Q&A
> 0 questions · Back to [README](../README.md)

## Table of Contents
- [Gotchas — SignalR with Authentication (Interview Traps)](#gotchas--signalr-with-authentication-interview-traps)

---

## Gotchas — SignalR with Authentication (Interview Traps)

---

#### Gotcha 1. JWT Must Travel in the Query String for WebSocket Transports — Authorization Header Is Not Available

**Concepts**
- Browser WebSocket API does not support custom request headers
- Tokens cannot be sent via Authorization: Bearer for WebSocket upgrade requests
- SignalR's accessTokenFactory appends the token as ?access_token= in the URL
- The query string is visible in server access logs, CDN edge logs, and browser history

**Answer**

Standard REST API authentication works by setting `Authorization: Bearer <token>` in every request header, which never appears in access logs. The browser's WebSocket API only accepts a URL and an optional subprotocol array — there is no header parameter. SignalR's JavaScript client uses the `accessTokenFactory` callback to append the JWT as `?access_token=<token>` to the WebSocket upgrade URL and the negotiate POST. The token is encrypted in transit over TLS, but the full URL including the token value is recorded in server access logs, reverse-proxy logs, and CDN edge logs. Short-lived tokens (TTL under five minutes) minimise the window of exposure if a log is compromised. Alternatively, cookie-based authentication avoids the query-string exposure because cookies are sent in HTTP headers automatically during both the negotiate POST and the WebSocket upgrade.

---

#### Gotcha 2. A JWT Can Expire Mid-Connection — the Hub Does Not Automatically Disconnect or Refresh

**Concepts**
- JWT expiry is only checked at connection time (negotiate + WebSocket upgrade)
- A long-lived SignalR connection established with a valid token remains open after the token expires
- The expired token is not re-validated on each hub invocation by default
- Applications must implement token refresh via connection.stop() / restart or using accessTokenFactory with refresh logic

**Answer**

Token validation in SignalR occurs at connection time: when the client sends the negotiate POST and when the WebSocket upgrade fires, ASP.NET Core validates the JWT's expiry, signature, and claims. Once the connection is established, subsequent hub invocations do not re-validate the token — the connection's `ClaimsPrincipal` is captured at connect time and reused. A connection that was established with a 30-minute token and stays open for two hours continues to work for the full two hours even though the token expired 90 minutes ago. For security-sensitive applications this is a risk: a revoked or expired token should not grant indefinite access. The mitigation is to implement a token-refresh loop on the client that calls `connection.stop()` and `connection.start()` before the token expires, supplying a freshly obtained token via `accessTokenFactory`.

---

#### Gotcha 3. [Authorize] on the Hub Class Does Not Apply Per-Invocation — It Applies at Connection Time

**Concepts**
- [Authorize] on the hub class is enforced at the negotiate endpoint and WebSocket upgrade
- Hub method invocations on an already-connected client are not individually re-authorized
- A user whose permissions change after connection is established retains their original context
- Fine-grained per-method authorization requires checking Context.User inside each method

**Answer**

Placing `[Authorize]` on a hub class causes SignalR to validate the connection's identity during the negotiate handshake and WebSocket upgrade. Once connected, subsequent hub method invocations do not re-run the authorization check — the connection's established `ClaimsPrincipal` is used for all calls. If a user's role is revoked after they connect, they continue to successfully invoke hub methods that check for that role via `[Authorize(Roles = "Admin")]` on the hub class for the entire duration of the connection. To enforce authorization at per-invocation granularity, each hub method must call `Context.User.IsInRole("Admin")` or check policies directly, or use per-method `[Authorize]` attributes — which in SignalR also only apply at the hub class level unless a custom IHubFilter is implemented to check authorization on each invocation.

---

#### Gotcha 4. Middleware Order Must Be: UseAuthentication Before UseAuthorization Before MapHub

**Concepts**
- UseAuthentication populates HttpContext.User from the incoming token
- UseAuthorization evaluates the policies against HttpContext.User
- MapHub registers the endpoint and its authorization requirements
- Out-of-order middleware means Context.User is null or anonymous when the hub runs

**Answer**

ASP.NET Core's middleware pipeline is order-sensitive. `UseAuthentication()` must run before `UseAuthorization()`, and both must run before the endpoint middleware that maps the hub. If `UseAuthentication()` is placed after `MapHub()`, the `HttpContext.User` is never populated from the token before the hub's authorization check runs, causing all authenticated requests to appear anonymous and `[Authorize]` to reject them with 401. A subtle variant is calling `UseAuthorization()` before `UseAuthentication()` — authentication never runs, `HttpContext.User` has no claims, and all authorization checks fail silently. The correct order in `Configure` (pre-minimal API) or `app.Use...` (minimal API) is: `UseRouting` → `UseAuthentication` → `UseAuthorization` → `MapHub`.

---

#### Gotcha 5. CORS Configuration Must Explicitly Allow the SignalR Negotiate and WebSocket Upgrade Paths

**Concepts**
- The SignalR negotiate POST is a cross-origin request if the client and server have different origins
- CORS policy must allow credentials to pass the Authorization header during negotiate
- AllowAnyOrigin() and AllowCredentials() cannot be used together — a specific origin must be named
- Missing CORS configuration on the negotiate endpoint blocks the entire SignalR connection

**Answer**

When a web application at `https://app.example.com` connects to a SignalR hub at `https://api.example.com/hub`, the negotiate POST and WebSocket upgrade are cross-origin requests. The server's CORS policy must allow the client's origin with credentials permitted, because the negotiate POST may carry cookies or the access token. `AllowAnyOrigin()` cannot be combined with `AllowCredentials()` — browsers reject this combination (CORS specification prohibits wildcard origins with credentials). The policy must name the specific allowed origin: `WithOrigins("https://app.example.com").AllowAnyHeader().AllowAnyMethod().AllowCredentials()`. The CORS policy must also be applied before `MapHub()` via `RequireCors` on the endpoint or via `UseCors()` middleware positioned before `UseRouting()`.

---

#### Gotcha 6. Cookie Authentication Avoids the Query-String Token Exposure but Requires SameSite Configuration

**Concepts**
- Cookies are sent automatically with the WebSocket upgrade request by the browser
- Cookie auth for SignalR avoids the ?access_token= query string exposure
- SameSite=None; Secure must be set for cross-origin SignalR connections that use cookies
- SameSite=Strict blocks cookies on cross-origin requests, preventing the authenticate upgrade

**Answer**

Cookie authentication is a viable alternative to JWT query-string delivery for SignalR. When the user is already authenticated via a cookie (set by a login endpoint or the same-origin application), the browser includes the cookie in both the negotiate POST and the WebSocket upgrade automatically — no `accessTokenFactory` configuration is needed. This avoids the access-log token exposure completely. The constraint is the `SameSite` cookie attribute: `SameSite=Strict` prevents the cookie from being sent on cross-origin requests, which breaks SignalR when the hub is on a different subdomain or domain from the page. `SameSite=None; Secure` allows cross-origin cookies but requires the HTTPS (Secure) flag and is blocked in some privacy-focused browser configurations. Same-origin deployments where the hub and the web page share a domain are the simplest configuration for cookie auth.

---

#### Gotcha 7. IUserIdProvider Returning Null Causes Clients.User() to Silently Miss All Connections

**Concepts**
- IUserIdProvider.GetUserId(HubConnectionContext context) maps a connection to a stable user ID
- Default implementation returns the NameIdentifier claim; absent claim returns null
- A connection with a null user ID is not indexed under any user identity
- Clients.User("userId") delivers to zero connections when GetUserId() returned null for those connections

**Answer**

`Clients.User(userId)` relies on `IUserIdProvider` to have indexed each connection under a consistent user identifier at connect time. The default `DefaultUserIdProvider` calls `context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value`. If the JWT uses `sub` instead of `nameidentifier` (common in OpenID Connect tokens where `sub` is the standard user identifier claim), `FindFirst(ClaimTypes.NameIdentifier)` returns null, `GetUserId` returns null, and the connection is stored without a user identifier. Subsequent `Clients.User("user-123")` calls find no connections and deliver nothing — silently. The fix is to implement a custom `IUserIdProvider` that reads the correct claim for the token format in use, and register it with `services.AddSingleton<IUserIdProvider, MyUserIdProvider>()`.

---

#### Gotcha 8. Authorize Attribute Does Not Prevent Unauthenticated Clients from Accessing the Negotiate Endpoint

**Concepts**
- The negotiate endpoint is a separate HTTP endpoint at hub-path/negotiate
- [Authorize] on the hub class protects the hub endpoint but the negotiate route must also be protected
- An unauthenticated negotiate call returns a connectionToken that cannot be used without a valid token on upgrade
- Exposing negotiate without authentication enables connection-token harvesting

**Answer**

The negotiate endpoint returns a connection token that the client uses for the subsequent WebSocket upgrade. When `[Authorize]` is on the hub class, ASP.NET Core protects both the negotiate path and the hub endpoint. However, custom routing configurations or middleware ordering errors can leave the negotiate endpoint exposed without authentication while the hub endpoint is protected. An attacker that can call the negotiate endpoint without a token receives a connection token; they cannot use that token without a valid JWT on the WebSocket upgrade, but the token exposure itself is a minor information disclosure. The correct configuration ensures that `UseAuthentication()` and `UseAuthorization()` run before `MapHub()` and that no route bypasses the authorization middleware for the negotiate path.

---

#### Gotcha 9. Hub Authorize Policies Are Not Re-Evaluated on Automatic Reconnect

**Concepts**
- On reconnect, SignalR reconnects to the same hub path and re-runs the negotiate + upgrade
- If accessTokenFactory provides a cached (possibly expired) token, the reconnect uses the stale token
- The policy check runs against the new connection's token; an expired token causes a 401 on reconnect
- accessTokenFactory must fetch a fresh token, not cache the initial one

**Answer**

When SignalR automatically reconnects after a connection drop, it initiates a new negotiate POST and WebSocket upgrade — the full connection establishment sequence runs again. The `accessTokenFactory` callback is called to provide the token for the new connection. If the factory simply returns the token that was captured at initial connection time (a common pattern when the factory is an arrow function closing over a variable), it may return an expired token on the reconnect attempt. The server validates the token on the new negotiate POST and rejects it with 401 if it has expired. The reconnect fails and the client enters a retry loop that never succeeds. The `accessTokenFactory` must dynamically retrieve a fresh token — calling a token refresh endpoint or reading from a token store that automatically refreshes — rather than closing over a static variable.

---

#### Gotcha 10. Token Validation Parameters Must Match Between REST API Endpoints and SignalR Hubs

**Concepts**
- If the REST API and the SignalR hub share the same AddAuthentication configuration, they use the same validation parameters
- If they use separate authentication schemes, the hub must be explicitly configured with the correct scheme
- A mismatched ValidAudience or ValidIssuer causes the hub to reject tokens that the REST API accepts
- The [Authorize(AuthenticationSchemes = "Bearer")] attribute on the hub overrides the default scheme

**Answer**

In applications where a REST API and a SignalR hub coexist, both use JWT Bearer authentication but may be configured with different `ValidAudience` or `ValidIssuer` parameters — for example, if the hub was added later and registered a separate Bearer scheme with slightly different settings. A JWT that is valid for the REST API routes is rejected on the hub negotiate endpoint with 401, confusing developers who verify the token is valid using the REST API and cannot understand why the same token fails on SignalR. The hub's `[Authorize]` attribute can specify `AuthenticationSchemes` to target the correct registered scheme, and both scheme configurations must agree on the same issuer, audience, and signing key. Using a single shared `AddJwtBearer()` registration for both REST and SignalR endpoints is the simplest way to avoid this mismatch.

---
