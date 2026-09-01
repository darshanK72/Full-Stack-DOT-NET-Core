# Azure API Management — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [What is Azure API Management (APIM), and what problem does it solve in a microse…](#q1)
2. [What is the APIM gateway, and how does it sit between clients and backend servic…](#q2)
3. [What are the main components of an Azure API Management instance (gateway, manag…](#q3)
4. [What is the difference between the Consumption, Developer, Basic, Standard, and …](#q4)
5. [What is a self-hosted gateway in Azure API Management, and when would you use it…](#q5)
6. [What is the difference between an API, a product, and a subscription in Azure AP…](#q6)
7. [How does the `Ocp-Apim-Subscription-Key` header work, and where is it validated …](#q7)
8. [What is the purpose of API revisions and versions in APIM, and when would you us…](#q8)
9. [How do you expose multiple backend services (for example Azure App Service and A…](#q9)
10. [What are APIM policies, and at what scopes can they be applied (global, product,…](#q10)
11. [Explain the inbound, backend, outbound, and on-error policy pipeline sections an…](#q11)
12. [What is a policy fragment in Azure API Management, and how does it differ from i…](#q12)
13. [What is the `context` object in APIM policy expressions, and what information do…](#q13)
14. [How does the `<send-request>` policy work, and what is a common use case for it …](#q14)
15. [How do you rewrite URLs or transform request and response bodies using APIM poli…](#q15)
16. [What is the `<choose>` policy, and when would you use conditional logic in the g…](#q16)
17. [How does the `<validate-jwt>` policy validate bearer tokens, and what OpenID Con…](#q17)
18. [What is the difference between validating a JWT at APIM versus validating it in …](#q18)
19. [How can APIM integrate with Microsoft Entra ID (Azure AD) or Azure AD B2C for OA…](#q19)
20. [How would you implement custom authentication logic (such as SOAP UsernameToken …](#q20)
21. [What is mutual TLS (mTLS) in Azure API Management, and when is it used?](#q21)
22. [What is the difference between `<rate-limit>` and `<quota>` policies in APIM?](#q22)
23. [How do rate-limit counters work across APIM instances, and what stores them on t…](#q23)
24. [How can you implement subscription-based or IP-based throttling in APIM?](#q24)
25. [What are the `<cache-lookup>` and `<cache-store>` policies used for?](#q25)
26. [What is the APIM developer portal, and what can external developers do with it?](#q26)
27. [How do you import an OpenAPI (Swagger) specification into Azure API Management?](#q27)
28. [Gotcha: What common mistake do teams make when they put business logic in APIM p…](#q28)

---

## Q1. What is Azure API Management (APIM), and what problem does it solve in a microservices or multi-API architecture?

What is Azure API Management (APIM), and what problem does it solve in a microservices or multi-API architecture?

**Answer:** Azure API Management (APIM) is a fully managed gateway and lifecycle platform that sits in front of your backend APIs and gives you one consistent front door for authentication, throttling, routing, documentation, and observability instead of duplicating that logic in every service.

- In a microservices layout, clients would otherwise need to know many URLs, keys, and auth schemes; APIM publishes a single base URL and hides whether the backend is App Service, Azure Functions, or an on-premises server.
- It decouples how APIs are consumed from how they are implemented, so you can refactor, version, or move backends without changing every client application.
- Teams use APIM to enforce organization-wide standards — subscription keys, rate limits, JWT validation, and request logging — in one place rather than in each ASP.NET Core project.

---

## Q2. What is the APIM gateway, and how does it sit between clients and backend services?

What is the APIM gateway, and how does it sit between clients and backend services?

**Answer:** The APIM gateway is the runtime proxy that terminates incoming HTTP(S) requests from clients, runs configured policies, and forwards transformed requests to the configured backend URL.

- A client always calls the APIM endpoint (for example `https://myapi.azure-api.net/orders`), never the internal App Service hostname directly, which keeps backend addresses private.
- On each request the gateway executes the inbound policy section first (auth, rate limits, header injection), then the backend section (routing, retries), then outbound policies (response shaping, CORS headers) before returning the response.
- The gateway is stateless at the application level; durable counters for quotas and some cache entries are stored in Azure-managed backing services depending on tier.

---

## Q3. What are the main components of an Azure API Management instance (gateway, management plane, developer portal)?

What are the main components of an Azure API Management instance (gateway, management plane, developer portal)?

**Answer:** An APIM instance has three cooperating parts: the gateway handles live traffic, the management plane is where administrators define APIs and policies, and the developer portal is the self-service site where consumers discover APIs and manage subscription keys.

- The **management plane** (Azure portal, REST API, ARM/Bicep, or Azure DevOps pipelines) is where you import OpenAPI specs, attach backends, write policies, and publish products — it does not serve customer API traffic.
- The **gateway** is the only component that must be on the request path; it scales with your tier and optionally runs as self-hosted nodes inside your network on Premium.
- The **developer portal** is a customizable website (built-in or fully replaced) where partners sign up, read documentation, and retrieve primary/secondary subscription keys for the products they subscribe to.

---

## Q4. What is the difference between the Consumption, Developer, Basic, Standard, and Premium APIM service tiers?

What is the difference between the Consumption, Developer, Basic, Standard, and Premium APIM service tiers?

**Answer:** APIM tiers trade off cost, scale, and enterprise features: Consumption is serverless pay-per-call, Developer is a low-cost sandbox, Basic and Standard add dedicated capacity and SLAs for production, and Premium adds multi-region deployment, virtual network injection, and self-hosted gateways.

| Tier | Typical use | Notable limits / features |
|---|---|---|
| Consumption | Sporadic or bursty APIs | No fixed capacity units; per-million-call billing; no VNet |
| Developer | Learning and non-production | Same feature surface as Standard at low price; no SLA |
| Basic | Small production APIs | Fixed scale units; custom domain; limited multi-region |
| Standard | Production with moderate scale | Higher unit limits; SLA; optional zone redundancy |
| Premium | Enterprise, hybrid, multi-region | VNet integration, self-hosted gateway, higher throughput |

Choose Consumption when traffic is unpredictable and integration depth is moderate; choose Premium when APIs must run inside a private network or be deployed to multiple Azure regions for latency and availability.

---

## Q5. What is a self-hosted gateway in Azure API Management, and when would you use it?

What is a self-hosted gateway in Azure API Management, and when would you use it?

**Answer:** A self-hosted gateway is a containerized APIM gateway you deploy on-premises or in your own Kubernetes cluster while still managing APIs and policies from the Azure-hosted management plane.

- It lets hybrid scenarios keep traffic inside a corporate network — for example an on-premises ERP API — while central policy and developer portal remain in Azure.
- Self-hosted gateways are available on the Premium tier and register to your APIM service; they pull configuration from the management plane and report telemetry back.
- Use them when regulatory, latency, or network-segmentation requirements prevent sending internal API traffic through the public Azure gateway endpoint.

---

## Chapter 2: APIs, Products & Subscriptions

---

## Q6. What is the difference between an API, a product, and a subscription in Azure API Management?

What is the difference between an API, a product, and a subscription in Azure API Management?

**Answer:** An **API** is a published surface (operations, policies, backend URL) for one backend service; a **product** is a bundle of one or more APIs offered to consumers under shared terms; and a **subscription** is a consumer's enrollment in a product that grants a unique subscription key and optional rate limits.

- You might define an `Orders API` and a `Inventory API`, then group both into a `Partner Portal` product so external partners get one key for everything they need.
- Products control visibility (open, protected, or private) and can carry product-level policies such as a shared quota across all APIs in the bundle.
- Each subscription belongs to one product and one user (or application); APIM validates the subscription key before the request reaches API-level policies.

---

## Q7. How does the `Ocp-Apim-Subscription-Key` header work, and where is it validated in the request pipeline?

How does the `Ocp-Apim-Subscription-Key` header work, and where is it validated in the request pipeline?

**Answer:** The `Ocp-Apim-Subscription-Key` header carries the primary or secondary key issued when a developer subscribes to a product, and APIM validates it during inbound processing — typically via the built-in `<check-header>` or subscription validation — before forwarding the call to the backend.

- Clients can also pass the key as a query parameter named `subscription-key`, though headers are preferred because query strings appear in logs and browser history more easily.
- Each subscription has two keys so operators can rotate one while the other remains active, reducing downtime during key rollover.
- Subscription validation is separate from OAuth or JWT checks: the key identifies *which consumer* is calling, while JWT validation identifies *which user or client application* is authenticated.

---

## Q8. What is the purpose of API revisions and versions in APIM, and when would you use each?

What is the purpose of API revisions and versions in APIM, and when would you use each?

**Answer:** **Revisions** are draft or staging snapshots of the same API that share one URL until you make a revision current, while **versions** are distinct numbered or labeled releases (for example `v1`, `v2`) that clients call through separate URL paths or query parameters.

- Use a revision when you want to test policy or documentation changes safely — you edit revision `2`, validate it, then set it current without changing the public version label.
- Use versions when you must support breaking contract changes simultaneously: `/orders/v1` keeps legacy clients working while `/orders/v2` exposes a new schema.
- Revisions help internal rollout; versions help external backward compatibility — they solve different lifecycle problems and can be combined on the same API.

---

## Q9. How do you expose multiple backend services (for example Azure App Service and Azure Functions) through a single APIM instance?

How do you expose multiple backend services (for example Azure App Service and Azure Functions) through a single APIM instance?

**Answer:** You import or manually create a separate API in APIM for each backend, point each API's backend service URL to the correct App Service or Functions hostname, and optionally group them under one product so clients use a single APIM base URL and subscription key.

- Each API maintains its own operations, policies, and diagnostics even though they share one APIM gateway hostname (`https://contoso.azure-api.net`).
- Path-based routing is natural: `/inventory/*` maps to one backend URL and `/billing/*` to another, or you use separate API URL suffixes (`/inventory`, `/billing`).
- Shared cross-cutting policies (JWT validation, correlation ID injection) can live at global or product scope so you do not duplicate them on every API.

---

## Chapter 3: Policies & the Request Pipeline

---

## Q10. What are APIM policies, and at what scopes can they be applied (global, product, API, operation)?

What are APIM policies, and at what scopes can they be applied (global, product, API, operation)?

**Answer:** APIM policies are XML-defined rules executed by the gateway on every matching request or response, and they can be attached at four scopes — global (all APIs), product, API, or individual operation — with narrower scopes overriding broader ones where conflicts exist.

- Global policies suit organization-wide requirements such as adding a `X-Request-Id` header or blocking certain IP ranges.
- Product policies enforce terms for a bundle, such as a quota shared across all APIs in a partner product.
- API and operation policies handle service-specific routing, caching, or payload transforms close to the endpoint that needs them.

Policy execution order within a section follows scope from global → product → API → operation, so more specific rules can refine or extend shared behavior.

---

## Q11. Explain the inbound, backend, outbound, and on-error policy pipeline sections and what runs in each.

Explain the inbound, backend, outbound, and on-error policy pipeline sections and what runs in each.

**Answer:** APIM divides policy execution into four sequential sections: **inbound** runs before the backend call, **backend** controls forwarding and retries, **outbound** runs on the response before it reaches the client, and **on-error** runs when an unhandled failure occurs in any prior section.

1. **Inbound** — Authentication, rate limiting, URL rewriting, request body validation, and header injection happen here; a `<return-response>` in inbound short-circuits the pipeline and never calls the backend.
2. **Backend** — `<forward-request>`, timeout tuning, and `<set-backend-service>` select which backend URL receives the proxied call; this section is skipped if inbound already returned a response.
3. **Outbound** — Response header manipulation, masking internal error details, CORS headers, and payload transformation run on the backend response before the client sees it.
4. **On-error** — Custom error bodies, logging, and fallback responses run when APIM or a policy throws; without on-error policies clients may see generic gateway errors.

Thinking in this order helps you place logic correctly: reject bad auth in inbound, not in outbound.

---

## Q12. What is a policy fragment in Azure API Management, and how does it differ from inline policies?

What is a policy fragment in Azure API Management, and how does it differ from inline policies?

**Answer:** A policy fragment is a reusable block of policy XML stored centrally and included with `<include-fragment>` from global, API, or operation policies, whereas inline policies are written directly in each policy document.

- Fragments reduce duplication when many APIs share the same authentication or logging block — you maintain one fragment and reference it from multiple APIs.
- When a fragment is included, its XML is expanded in place at runtime; variables set before the include are visible inside the fragment, but you must define variables before use (as noted in policy authoring guidance).
- Teams often extract complex flows — such as SOAP UsernameToken parsing plus downstream token exchange — into fragments to keep operation-level policies readable and testable in isolation.

---

## Q13. What is the `context` object in APIM policy expressions, and what information does it expose?

What is the `context` object in APIM policy expressions, and what information does it expose?

**Answer:** The `context` object is the runtime state available inside C#-style policy expressions (`@(...)`), exposing the current request, response, API metadata, user subscription, and named variables set by earlier policy steps.

- `context.Request` provides headers, body, URL, and method; `context.Response` is available in outbound and on-error sections after the backend responds.
- `context.Api`, `context.Operation`, and `context.Product` identify which surface handled the call, which is useful for logging and conditional policies.
- `context.Variables` is a dictionary you populate with `<set-variable>` — for example storing a parsed JWT payload or roles list — and read later in the same pipeline with `context.Variables.GetValueOrDefault<T>("name")`.

Without `context`, policies could not branch on header values, API name, or intermediate computation results.

---

## Q14. How does the `<send-request>` policy work, and what is a common use case for it in authentication flows?

How does the `<send-request>` policy work, and what is a common use case for it in authentication flows?

**Answer:** The `<send-request>` policy makes an HTTP call from the gateway to an external URL during policy execution, stores the response in a named variable, and lets subsequent policy steps read status codes and bodies — commonly used to exchange credentials for tokens or call a custom auth validation API.

- Attributes such as `mode="new"`, `timeout`, and `response-variable-name` control whether the call blocks the pipeline and where the `IResponse` result is stored.
- In a legacy SOAP basic-auth scenario, the gateway might `<send-request>` to a token endpoint with `client_credentials`, then call a validation API with the returned bearer token before allowing the original request through.
- Because `<send-request>` adds latency on the critical path, keep timeouts tight, cache tokens when possible, and fail fast with `<return-response>` when validation returns 401 or 500.

---

## Q15. How do you rewrite URLs or transform request and response bodies using APIM policies?

How do you rewrite URLs or transform request and response bodies using APIM policies?

**Answer:** URL changes use `<rewrite-uri>` or `<set-query-parameter>` in the inbound section, while body transforms use `<set-body>` with policy expressions or the `context.Request.Body.As<T>()` / `context.Response.Body.As<T>()` helpers to read and replace JSON, XML, or plain text.

- `<rewrite-uri template="/v2/{orderId}" />` maps a public path to a different backend path without exposing internal routing conventions.
- Reading the body with `preserveContent: true` keeps the original stream available for later policies and the backend; omitting preservation consumes the body stream.
- Outbound `<set-body>` can strip internal fields from JSON responses or convert XML to JSON so external clients see a stable public contract even when the backend schema changes.

---

## Q16. What is the `<choose>` policy, and when would you use conditional logic in the gateway?

What is the `<choose>` policy, and when would you use conditional logic in the gateway?

**Answer:** The `<choose>` policy evaluates one or more `<when condition="@(...)">` expressions and executes the first matching branch, with an optional `<otherwise>` fallback — it is APIM's equivalent of if/else for routing different auth methods or response formats.

- Use it when the same API must accept multiple authentication styles — for example SOAP UsernameToken in the body versus a Bearer JWT in the `Authorization` header — and each style needs a different validation path.
- Conditions are Boolean expressions over `context`, so you can branch on header presence, HTTP method, product name, or variables set earlier in the pipeline.
- Heavy business rules with many branches belong in the backend service; `<choose>` is best for gateway concerns such as auth mode selection, content-type handling, and environment-specific routing.

---

## Chapter 4: Security — OAuth, JWT & Authentication

---

## Q17. How does the `<validate-jwt>` policy validate bearer tokens, and what OpenID Connect metadata does it use?

How does the `<validate-jwt>` policy validate bearer tokens, and what OpenID Connect metadata does it use?

**Answer:** The `<validate-jwt>` policy parses the JWT from a configured header or query parameter, fetches signing keys from an OpenID Connect discovery document, and verifies signature, issuer, audience, expiration, and optional claims before the request proceeds — returning 401 or 403 on failure.

- The `<openid-config url="..." />` element points to the well-known metadata URL (for example `https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration`) so APIM retrieves current signing keys automatically.
- You list acceptable `<audiences>` and `<issuers>` explicitly; tokens meant for a different API or tenant are rejected even if cryptographically valid.
- Attributes like `require-expiration-time="true"` and `require-scheme="Bearer"` enforce baseline OAuth 2.0 bearer token rules at the edge.

```xml
<validate-jwt header-name="Authorization" require-scheme="Bearer">
  <openid-config url="https://login.example.com/.well-known/openid-configuration" />
  <audiences><audience>api://my-app</audience></audiences>
</validate-jwt>
```

---

## Q18. What is the difference between validating a JWT at APIM versus validating it in the backend ASP.NET Core API?

What is the difference between validating a JWT at APIM versus validating it in the backend ASP.NET Core API?

**Answer:** Validating at APIM rejects unauthorized traffic before it reaches your servers, reducing load and hiding backend URLs, while validating in ASP.NET Core with `AddJwtBearer` gives the API full access to claims for authorization handlers but consumes compute on every request including junk traffic.

| Concern | Validate at APIM | Validate in ASP.NET Core |
|---|---|---|
| Traffic filtering | Blocks invalid tokens at the edge | Backend must process each request |
| Claim-based authorization | Limited to policy expressions | Rich `[Authorize]`, policies, and handlers |
| Key rotation | APIM refreshes from OpenID metadata | App must reload metadata or restart |
| Defense in depth | Single layer if backend is public | Both layers if APIM and app validate |

Many production setups validate at APIM for coarse access control and still configure JWT bearer authentication in the API for fine-grained role and scope checks — duplicate validation is acceptable when backends can also be reached through other paths.

---

## Q19. How can APIM integrate with Microsoft Entra ID (Azure AD) or Azure AD B2C for OAuth 2.0?

How can APIM integrate with Microsoft Entra ID (Azure AD) or Azure AD B2C for OAuth 2.0?

**Answer:** APIM can protect APIs with `<validate-jwt>` against tokens issued by Microsoft Entra ID or Azure AD B2C by pointing OpenID configuration at the tenant's discovery endpoint and listing the app registration's Application ID URI as an allowed audience.

- For Entra ID protected APIs, the audience is typically `api://{client-id}` or the application ID URI configured on the exposed API app registration.
- For Azure AD B2C, the OpenID URL includes the B2C policy segment (for example `.../B2C_1_signupsignin/v2.0/.well-known/openid-configuration`) because each user flow publishes separate metadata.
- The developer portal can also be configured as an OAuth 2.0 client so partners sign in with Entra ID and receive subscription keys, aligning portal identity with corporate directories.

---

## Q20. How would you implement custom authentication logic (such as SOAP UsernameToken or legacy basic auth) in APIM policies?

How would you implement custom authentication logic (such as SOAP UsernameToken or legacy basic auth) in APIM policies?

**Answer:** Custom auth in APIM combines `<set-variable>` expressions that parse the request body or headers, `<choose>` branches for each auth style, `<send-request>` calls to a token or validation service, and `<return-response>` with SOAP faults or HTTP 401 when validation fails — all in the inbound section before `<forward-request>`.

- For SOAP UsernameToken, policies read the XML body with `context.Request.Body.As<XElement>(preserveContent: true)`, extract username and password nodes via XPath or LINQ to XML, and reject the call if either is empty.
- After extracting credentials, the gateway can obtain an access token via client credentials and POST the username/password to an internal validation API, then store returned roles in `context.Variables` for later `<check-header>` or backend routing decisions.
- JWT-based clients on the same API take a separate `<when>` branch that runs `<validate-jwt>` instead of SOAP parsing, so one APIM operation supports legacy and modern clients.

This pattern appears in enterprise B2C bridges where APIM modernizes a public SOAP contract without rewriting the legacy backend immediately.

---

## Q21. What is mutual TLS (mTLS) in Azure API Management, and when is it used?

What is mutual TLS (mTLS) in Azure API Management, and when is it used?

**Answer:** Mutual TLS (mTLS) requires the client to present an X.509 certificate during the TLS handshake in addition to the server certificate, and APIM can enforce client certificate validation at the gateway or present its own certificate to backends that require it.

- Client certificate authentication suits machine-to-machine integrations where subscription keys or passwords are considered too weak or hard to rotate at scale.
- On Premium with VNet integration, you configure trusted client CAs and optional certificate pinning so only partners with issued certs can call the API.
- Backend mTLS is the reverse: APIM presents a client cert when forwarding to the backend so the upstream service knows the call originated from the gateway.

Use mTLS when regulatory or contractual requirements demand certificate-based identity beyond bearer tokens or API keys.

---

## Chapter 5: Rate Limiting, Quotas & Caching

---

## Q22. What is the difference between `<rate-limit>` and `<quota>` policies in APIM?

What is the difference between `<rate-limit>` and `<quota>` policies in APIM?

**Answer:** `<rate-limit>` caps how many calls are allowed in a short rolling window (for example 100 calls per minute) to prevent bursts, while `<quota>` caps total usage over a longer period (for example 10,000 calls per month) to enforce commercial plan limits.

- Rate limits protect backend stability from traffic spikes; when exceeded, APIM returns HTTP 429 with a `Retry-After` hint depending on policy configuration.
- Quotas align with subscription or product tiers — a free product might allow 1,000 calls per month and a paid product 1 million — and reset on a defined calendar or rolling interval.
- Both policies can be scoped by subscription key, IP address, or other dimensions using policy variants such as `<rate-limit-by-key>`.

Apply rate limits for technical protection and quotas for business entitlements; many products use both together.

---

## Q23. How do rate-limit counters work across APIM instances, and what stores them on the Premium tier?

How do rate-limit counters work across APIM instances, and what stores them on the Premium tier?

**Answer:** APIM maintains distributed counters for rate limits and quotas so all gateway instances in a region share the same counts; on Premium tier with multiple units or regions, counters synchronize through Azure-managed storage backing the service so a client cannot bypass limits by hitting different nodes.

- On lower tiers, counters are scoped to the service's deployed units within a region; Premium adds multi-region gateway deployment with consistent policy enforcement when configured.
- Counter keys derive from the policy dimension — typically subscription ID or client IP — so two different subscriptions each get independent buckets.
- Because counters are external to your backend, moving rate limiting to APIM gives consistent enforcement even when several App Service instances scale out behind the gateway.

---

## Q24. How can you implement subscription-based or IP-based throttling in APIM?

How can you implement subscription-based or IP-based throttling in APIM?

**Answer:** Subscription-based throttling uses `<rate-limit-by-key>` or product-level limits keyed on `{subscriptionId}`, while IP-based throttling keys on `{clientIpAddress}` or a forwarded header you trust, applied in inbound policies before the backend call.

- Subscription throttling ties naturally to APIM's subscription model — each product tier gets different `<rate-limit-by-key calls="..." renewal-period="..." />` values.
- IP throttling protects public endpoints from abuse when callers do not present a subscription key, or adds a second dimension alongside subscription limits.
- When clients pass through upstream proxies, configure `<set-variable>` to read `X-Forwarded-For` carefully and only trust it from known proxy IPs to avoid spoofing.

Combine both when partners authenticate with keys but anonymous health-check endpoints still need per-IP caps.

---

## Q25. What are the `<cache-lookup>` and `<cache-store>` policies used for?

What are the `<cache-lookup>` and `<cache-store>` policies used for?

**Answer:** The `<cache-lookup>` and `<cache-store>` pair enables response caching inside APIM: lookup checks whether a cached response exists for a computed cache key, skips the backend on a hit, and store saves the backend response for future requests with a defined time-to-live.

- Cache keys should include values that distinguish responses — URL path, query string, relevant headers — so one user does not receive another user's data.
- Caching only safe, idempotent GET responses is standard practice; marking personalized or authorized data as cacheable requires including subscription ID or user claims in the key.
- Internal APIM cache reduces latency and backend load for read-heavy public APIs; it is not a replacement for Redis or CDN caching at the edge when global distribution is required.

---

## Chapter 6: Developer Portal & Backend Services

---

## Q26. What is the APIM developer portal, and what can external developers do with it?

What is the APIM developer portal, and what can external developers do with it?

**Answer:** The APIM developer portal is a customizable self-service website where API consumers browse product catalogs, read interactive documentation, register accounts, subscribe to products, and retrieve subscription keys without opening a ticket to the platform team.

- Developers see operation descriptions, try-it-out consoles, and code samples generated from the imported OpenAPI definition.
- Subscription workflows can require administrator approval for protected products or be automatic for open products.
- The portal can be themed, extended with custom pages, or replaced entirely while still using APIM's subscription and identity backend.

It separates **API publishing** (what your team exposes) from **API consumption** (how partners onboard themselves).

---

## Q27. How do you import an OpenAPI (Swagger) specification into Azure API Management?

How do you import an OpenAPI (Swagger) specification into Azure API Management?

**Answer:** You import an OpenAPI JSON or YAML document through the Azure portal (**APIs → Add API → OpenAPI**), the APIM REST API, ARM/Bicep, or CI/CD pipelines, and APIM generates operations, parameters, and models from the spec while letting you set the backend service URL and apply policies afterward.

- During import you specify the API URL suffix, backend base URL (for example your ASP.NET Core App Service hostname), and optional version/revision settings.
- Import creates operation stubs aligned with HTTP methods and paths in the spec; you still attach authentication, rate limits, and rewrites in policy after import.
- Continuous deployment pipelines often store the OpenAPI file in source control and redeploy to APIM on each release so documentation and the live gateway stay synchronized with the ASP.NET Core project.

---

#### Gotcha 28. What common mistake do teams make when they put business logic in APIM policies instead of backend services?

**Answer:** Teams sometimes implement multi-step business workflows, database lookups, or domain rules entirely in APIM policy XML because it is convenient at the gateway, but that logic becomes hard to test, version, debug, and reuse compared with code in the ASP.NET Core backend.

- APIM policies excel at cross-cutting gateway concerns — auth, throttling, routing, header injection, and simple transforms — not at maintaining order totals, inventory checks, or saga orchestration.
- Complex C# expressions embedded in XML lack unit tests, pull requests, and static analysis that normal `.cs` projects provide, and policy errors surface only at runtime in production traffic.
- When requirements change frequently, redeploying backend code through standard CI/CD is safer than editing a thousand-line policy fragment that multiple APIs include.

Keep APIM thin: validate and protect at the edge, execute business rules in the API service where domain experts and developers already work.

---

## Q28. Gotcha: What common mistake do teams make when they put business logic in APIM policies instead of backend services?

_Answer not found._

---
