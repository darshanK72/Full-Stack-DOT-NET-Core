# Azure API Management — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Azure API Management (APIM), and what problem does it solve in a microservices or multi-API architecture?](#q1-what-is-azure-api-management-apim-and-what-problem-does-it-solve-in-a-microservices-or-multi-api-architecture)
2. [Q2. What is the APIM gateway, and how does it sit between clients and backend services?](#q2-what-is-the-apim-gateway-and-how-does-it-sit-between-clients-and-backend-services)
3. [Q3. What are the main components of an Azure API Management instance (gateway, management plane, developer portal)?](#q3-what-are-the-main-components-of-an-azure-api-management-instance-gateway-management-plane-developer-portal)
4. [Q4. What is the difference between the Consumption, Developer, Basic, Standard, and Premium APIM service tiers?](#q4-what-is-the-difference-between-the-consumption-developer-basic-standard-and-premium-apim-service-tiers)
5. [Q5. What is a self-hosted gateway in Azure API Management, and when would you use it?](#q5-what-is-a-self-hosted-gateway-in-azure-api-management-and-when-would-you-use-it)
6. [Q6. What is the difference between an API, a product, and a subscription in Azure API Management?](#q6-what-is-the-difference-between-an-api-a-product-and-a-subscription-in-azure-api-management)
7. [Q7. How does the `Ocp-Apim-Subscription-Key` header work, and where is it validated in the request pipeline?](#q7-how-does-the-ocp-apim-subscription-key-header-work-and-where-is-it-validated-in-the-request-pipeline)
8. [Q8. What is the purpose of API revisions and versions in APIM, and when would you use each?](#q8-what-is-the-purpose-of-api-revisions-and-versions-in-apim-and-when-would-you-use-each)
9. [Q9. How do you expose multiple backend services (for example Azure App Service and Azure Functions) through a single APIM instance?](#q9-how-do-you-expose-multiple-backend-services-for-example-azure-app-service-and-azure-functions-through-a-single-apim-instance)
10. [Q10. What are APIM policies, and at what scopes can they be applied (global, product, API, operation)?](#q10-what-are-apim-policies-and-at-what-scopes-can-they-be-applied-global-product-api-operation)
11. [Q11. Explain the inbound, backend, outbound, and on-error policy pipeline sections and what runs in each.](#q11-explain-the-inbound-backend-outbound-and-on-error-policy-pipeline-sections-and-what-runs-in-each)
12. [Q12. What is a policy fragment in Azure API Management, and how does it differ from inline policies?](#q12-what-is-a-policy-fragment-in-azure-api-management-and-how-does-it-differ-from-inline-policies)
13. [Q13. What is the `context` object in APIM policy expressions, and what information does it expose?](#q13-what-is-the-context-object-in-apim-policy-expressions-and-what-information-does-it-expose)
14. [Q14. How does the `<send-request>` policy work, and what is a common use case for it in authentication flows?](#q14-how-does-the-send-request-policy-work-and-what-is-a-common-use-case-for-it-in-authentication-flows)
15. [Q15. How do you rewrite URLs or transform request and response bodies using APIM policies?](#q15-how-do-you-rewrite-urls-or-transform-request-and-response-bodies-using-apim-policies)
16. [Q16. What is the `<choose>` policy, and when would you use conditional logic in the gateway?](#q16-what-is-the-choose-policy-and-when-would-you-use-conditional-logic-in-the-gateway)
17. [Q17. How does the `<validate-jwt>` policy validate bearer tokens, and what OpenID Connect metadata does it use?](#q17-how-does-the-validate-jwt-policy-validate-bearer-tokens-and-what-openid-connect-metadata-does-it-use)
18. [Q18. What is the difference between validating a JWT at APIM versus validating it in the backend ASP.NET Core API?](#q18-what-is-the-difference-between-validating-a-jwt-at-apim-versus-validating-it-in-the-backend-aspnet-core-api)
19. [Q19. How can APIM integrate with Microsoft Entra ID (Azure AD) or Azure AD B2C for OAuth 2.0?](#q19-how-can-apim-integrate-with-microsoft-entra-id-azure-ad-or-azure-ad-b2c-for-oauth-20)
20. [Q20. How would you implement custom authentication logic (such as SOAP UsernameToken or legacy basic auth) in APIM policies?](#q20-how-would-you-implement-custom-authentication-logic-such-as-soap-usernametoken-or-legacy-basic-auth-in-apim-policies)
21. [Q21. What is mutual TLS (mTLS) in Azure API Management, and when is it used?](#q21-what-is-mutual-tls-mtls-in-azure-api-management-and-when-is-it-used)
22. [Q22. What is the difference between `<rate-limit>` and `<quota>` policies in APIM?](#q22-what-is-the-difference-between-rate-limit-and-quota-policies-in-apim)
23. [Q23. How do rate-limit counters work across APIM instances, and what stores them on the Premium tier?](#q23-how-do-rate-limit-counters-work-across-apim-instances-and-what-stores-them-on-the-premium-tier)
24. [Q24. How can you implement subscription-based or IP-based throttling in APIM?](#q24-how-can-you-implement-subscription-based-or-ip-based-throttling-in-apim)
25. [Q25. What are the `<cache-lookup>` and `<cache-store>` policies used for?](#q25-what-are-the-cache-lookup-and-cache-store-policies-used-for)
26. [Q26. What is the APIM developer portal, and what can external developers do with it?](#q26-what-is-the-apim-developer-portal-and-what-can-external-developers-do-with-it)
27. [Q27. How do you import an OpenAPI (Swagger) specification into Azure API Management?](#q27-how-do-you-import-an-openapi-swagger-specification-into-azure-api-management)
28. [Q28. What common mistake do teams make when they put business logic in APIM policies instead of backend services?](#q28-what-common-mistake-do-teams-make-when-they-put-business-logic-in-apim-policies-instead-of-backend-services)

---

## Q1. What is Azure API Management (APIM), and what problem does it solve in a microservices or multi-API architecture?

**Concepts**
- Single front door for authentication, throttling, routing, and observability
- Decoupling API consumption from implementation
- Consistent organization-wide standards across services
- Hiding backend URLs and implementation details from clients

**Answer**

Azure API Management is a fully managed gateway and lifecycle platform that sits in front of my backend APIs and gives me one consistent front door for authentication, throttling, routing, documentation, and observability instead of duplicating that logic in every service. In a microservices layout, clients would otherwise need to know many URLs, keys, and auth schemes; APIM publishes a single base URL and hides whether the backend is App Service, Azure Functions, or an on-premises server. It decouples how APIs are consumed from how they are implemented, so I can refactor, version, or move backends without changing every client application. Teams use APIM to enforce organization-wide standards — subscription keys, rate limits, JWT validation, and request logging — in one place rather than in each ASP.NET Core project.

---

## Q2. What is the APIM gateway, and how does it sit between clients and backend services?

**Concepts**
- APIM gateway as runtime proxy — terminates and re-originates requests
- Inbound → backend → outbound policy execution order
- Backend addresses kept private from clients
- Stateless at application level; durable counters in managed backing services

**Answer**

The APIM gateway is the runtime proxy that terminates incoming HTTP(S) requests from clients, runs configured policies, and forwards transformed requests to the configured backend URL. A client always calls the APIM endpoint such as `https://myapi.azure-api.net/orders` and never the internal App Service hostname directly, which keeps backend addresses private. On each request the gateway executes the inbound policy section first for auth, rate limits, and header injection, then the backend section for routing and retries, then outbound policies for response shaping and CORS headers before returning the response to the client. The gateway is stateless at the application level; durable counters for quotas and some cache entries are stored in Azure-managed backing services depending on tier.

---

## Q3. What are the main components of an Azure API Management instance (gateway, management plane, developer portal)?

**Concepts**
- Gateway — live traffic proxy, the only on-request-path component
- Management plane — API and policy definition, not customer traffic
- Developer portal — self-service consumer onboarding and documentation
- Self-hosted gateway on Premium for on-premises traffic

**Answer**

An APIM instance has three cooperating parts: the gateway handles live traffic, the management plane is where administrators define APIs and policies, and the developer portal is the self-service site where consumers discover APIs and manage subscription keys. The management plane — accessed through the Azure portal, REST API, ARM/Bicep, or Azure DevOps pipelines — is where I import OpenAPI specs, attach backends, write policies, and publish products; it does not serve customer API traffic. The gateway is the only component on the request path; it scales with the tier and optionally runs as self-hosted nodes inside a private network on Premium. The developer portal is a customizable website where partners sign up, read documentation, and retrieve subscription keys for the products they subscribe to.

---

## Q4. What is the difference between the Consumption, Developer, Basic, Standard, and Premium APIM service tiers?

**Concepts**
- Consumption — serverless pay-per-call, no VNet
- Developer — low-cost sandbox, no SLA
- Basic/Standard — dedicated capacity, custom domain, production SLA
- Premium — multi-region, VNet injection, self-hosted gateway

**Answer**

APIM tiers trade off cost, scale, and enterprise features. Consumption is serverless pay-per-call with no fixed capacity units and no VNet support, suited for sporadic or bursty APIs. Developer is a low-cost sandbox with the same feature surface as Standard but without an SLA, used for learning and non-production. Basic and Standard add dedicated capacity units, custom domains, and production SLAs for APIs serving real traffic. Premium adds multi-region deployment, virtual network injection, self-hosted gateways, and higher throughput, making it the right choice when APIs must run inside a private network or be deployed to multiple Azure regions for latency and availability. I choose Consumption when traffic is unpredictable and integration depth is moderate, and Premium when APIs must stay inside a corporate network or global distribution is required.

---

## Q5. What is a self-hosted gateway in Azure API Management, and when would you use it?

**Concepts**
- Containerized APIM gateway deployed on-premises or in Kubernetes
- Management plane remains in Azure; configuration pulled down
- Hybrid scenarios — internal APIs not reachable through public Azure endpoint
- Premium tier only

**Answer**

A self-hosted gateway is a containerized APIM gateway I deploy on-premises or in my own Kubernetes cluster while still managing APIs and policies from the Azure-hosted management plane. It lets hybrid scenarios keep traffic inside a corporate network — for example an on-premises ERP API — while central policy and developer portal remain in Azure. Self-hosted gateways are available on the Premium tier and register to my APIM service; they pull configuration from the management plane and report telemetry back. I use them when regulatory, latency, or network-segmentation requirements prevent sending internal API traffic through the public Azure gateway endpoint.

---

## Q6. What is the difference between an API, a product, and a subscription in Azure API Management?

**Concepts**
- API — published surface for one backend service
- Product — bundle of APIs under shared terms and access controls
- Subscription — consumer enrollment granting a unique key
- Product-level policies applying across bundled APIs

**Answer**

An API is a published surface with operations, policies, and a backend URL for one backend service. A product is a bundle of one or more APIs offered to consumers under shared terms — I might define an Orders API and an Inventory API, then group both into a Partner Portal product so external partners get one key for everything they need. A subscription is a consumer's enrollment in a product that grants a unique primary and secondary subscription key and optional rate limits. Products control visibility (open, protected, or private) and can carry product-level policies such as a shared quota across all APIs in the bundle. Each subscription belongs to one product and one user or application, and APIM validates the subscription key before the request reaches API-level policies.

---

## Q7. How does the `Ocp-Apim-Subscription-Key` header work, and where is it validated in the request pipeline?

**Concepts**
- Subscription key identifying which consumer is calling
- Validation during inbound processing before forwarding to backend
- Query parameter alternative — subscription-key
- Primary and secondary keys for zero-downtime rotation
- Subscription key vs JWT — consumer identity vs user identity

**Answer**

The `Ocp-Apim-Subscription-Key` header carries the primary or secondary key issued when a developer subscribes to a product, and APIM validates it during inbound processing before forwarding the call to the backend. Clients can also pass the key as a query parameter named `subscription-key`, though headers are preferred because query strings appear in logs and browser history more easily. Each subscription has two keys so operators can rotate one while the other remains active, reducing downtime during key rollover. Subscription validation is separate from OAuth or JWT checks: the key identifies which consumer is calling, while JWT validation identifies which user or client application is authenticated — I often use both layers together.

---

## Q8. What is the purpose of API revisions and versions in APIM, and when would you use each?

**Concepts**
- Revisions — draft snapshots sharing one URL for internal rollout
- Versions — distinct numbered or path-labeled releases for breaking changes
- Making a revision current vs deploying a new version
- Both can be combined on the same API

**Answer**

Revisions are draft or staging snapshots of the same API that share one URL until I make a revision current, while versions are distinct numbered or labeled releases such as `v1` and `v2` that clients call through separate URL paths or query parameters. I use a revision when I want to test policy or documentation changes safely — I edit revision 2, validate it, then set it current without changing the public version label. I use versions when I must support breaking contract changes simultaneously: `/orders/v1` keeps legacy clients working while `/orders/v2` exposes a new schema. Revisions help internal rollout; versions help external backward compatibility — they solve different lifecycle problems and can be combined on the same API.

---

## Q9. How do you expose multiple backend services (for example Azure App Service and Azure Functions) through a single APIM instance?

**Concepts**
- Separate API per backend with its own operations and policies
- Path-based routing via API URL suffixes
- Shared APIM base URL across all backends
- Cross-cutting policies at global or product scope

**Answer**

I import or manually create a separate API in APIM for each backend, point each API's backend service URL to the correct App Service or Functions hostname, and optionally group them under one product so clients use a single APIM base URL and subscription key. Each API maintains its own operations, policies, and diagnostics even though they share one APIM gateway hostname. Path-based routing is natural: `/inventory/*` maps to one backend URL and `/billing/*` to another, or I use separate API URL suffixes. Shared cross-cutting policies such as JWT validation and correlation ID injection live at global or product scope so I do not duplicate them on every API.

---

## Q10. What are APIM policies, and at what scopes can they be applied (global, product, API, operation)?

**Concepts**
- XML policy rules executed on every matching request or response
- Four scopes — global, product, API, operation
- Narrower scopes override or extend broader ones
- Policy execution order: global → product → API → operation

**Answer**

APIM policies are XML-defined rules executed by the gateway on every matching request or response, attached at four scopes — global (all APIs), product, API, or individual operation — with narrower scopes overriding broader ones where conflicts exist. Global policies suit organization-wide requirements such as adding a correlation header or blocking certain IP ranges. Product policies enforce terms for a bundle, such as a quota shared across all APIs in a partner product. API and operation policies handle service-specific routing, caching, or payload transforms close to the endpoint that needs them. Policy execution order within a section follows global → product → API → operation so more specific rules can refine or extend shared behavior.

---

## Q11. Explain the inbound, backend, outbound, and on-error policy pipeline sections and what runs in each.

**Concepts**
- Inbound — auth, rate limiting, URL rewriting, request validation
- Backend — forward-request, timeout, set-backend-service
- Outbound — response shaping, CORS, masking internal details
- On-error — custom error bodies, fallback responses
- return-response in inbound short-circuits the backend call

**Answer**

APIM divides policy execution into four sequential sections. Inbound runs before the backend call and is where I place authentication, rate limiting, URL rewriting, request body validation, and header injection; a `<return-response>` in inbound short-circuits the pipeline and never calls the backend. Backend controls forwarding and retries — `<forward-request>`, timeout tuning, and `<set-backend-service>` select which backend URL receives the proxied call, and this section is skipped if inbound already returned a response. Outbound runs on the response before it reaches the client and is where I do response header manipulation, mask internal error details, add CORS headers, and transform payloads. On-error runs when an unhandled failure occurs in any prior section, where I return custom error bodies, log failures, and supply fallback responses — without on-error policies clients may see generic gateway errors. Thinking in this order helps me place logic correctly: reject bad auth in inbound, not in outbound.

---

## Q12. What is a policy fragment in Azure API Management, and how does it differ from inline policies?

**Concepts**
- Policy fragment — reusable XML block stored centrally
- include-fragment for referencing from multiple APIs
- Variables visible to the included fragment from calling context
- Extracting complex flows for maintainability

**Answer**

A policy fragment is a reusable block of policy XML stored centrally and included with `<include-fragment>` from global, API, or operation policies, whereas inline policies are written directly in each policy document. Fragments reduce duplication when many APIs share the same authentication or logging block — I maintain one fragment and reference it from multiple APIs. When a fragment is included, its XML is expanded in place at runtime; variables set before the include are visible inside the fragment, so I define variables before use. Teams often extract complex flows — such as SOAP UsernameToken parsing plus downstream token exchange — into fragments to keep operation-level policies readable and testable in isolation.

---

## Q13. What is the `context` object in APIM policy expressions, and what information does it expose?

**Concepts**
- context.Request — headers, body, URL, method
- context.Response — available in outbound and on-error
- context.Api, context.Operation, context.Product — routing metadata
- context.Variables — named values set with set-variable, read with GetValueOrDefault

**Answer**

The `context` object is the runtime state available inside C#-style policy expressions and exposes the current request, response, API metadata, user subscription, and named variables set by earlier policy steps. `context.Request` provides headers, body, URL, and method; `context.Response` is available in outbound and on-error sections after the backend responds. `context.Api`, `context.Operation`, and `context.Product` identify which surface handled the call, which is useful for logging and conditional policies. `context.Variables` is a dictionary I populate with `<set-variable>` — for example storing a parsed JWT payload or roles list — and read later in the same pipeline with `context.Variables.GetValueOrDefault<T>("name")`. Without `context`, policies could not branch on header values, API name, or intermediate computation results.

---

## Q14. How does the `<send-request>` policy work, and what is a common use case for it in authentication flows?

**Concepts**
- send-request makes an HTTP call from the gateway mid-pipeline
- response-variable-name stores the IResponse for downstream steps
- Token exchange — client credentials call followed by validation
- Tight timeouts and fast-fail with return-response on auth failure

**Answer**

The `<send-request>` policy makes an HTTP call from the gateway to an external URL during policy execution, stores the response in a named variable, and lets subsequent policy steps read status codes and bodies. I set attributes such as `mode="new"`, `timeout`, and `response-variable-name` to control whether the call blocks the pipeline and where the `IResponse` result is stored. In a legacy SOAP basic-auth scenario, the gateway might `<send-request>` to a token endpoint with `client_credentials`, then call a validation API with the returned bearer token before allowing the original request through. Because `<send-request>` adds latency on the critical path, I keep timeouts tight, cache tokens when possible, and fail fast with `<return-response>` when validation returns 401 or 500.

---

## Q15. How do you rewrite URLs or transform request and response bodies using APIM policies?

**Concepts**
- rewrite-uri and set-query-parameter for URL changes in inbound
- set-body with context.Request.Body.As<T> for request transforms
- context.Response.Body.As<T> for outbound body transforms
- preserveContent: true to keep body stream available downstream

**Answer**

URL changes use `<rewrite-uri>` or `<set-query-parameter>` in the inbound section, while body transforms use `<set-body>` with policy expressions or the `context.Request.Body.As<T>()` and `context.Response.Body.As<T>()` helpers to read and replace JSON, XML, or plain text. `<rewrite-uri template="/v2/{orderId}" />` maps a public path to a different backend path without exposing internal routing conventions. Reading the body with `preserveContent: true` keeps the original stream available for later policies and the backend; omitting preservation consumes the body stream, which prevents the backend from reading it. Outbound `<set-body>` can strip internal fields from JSON responses or convert XML to JSON so external clients see a stable public contract even when the backend schema changes.

---

## Q16. What is the `<choose>` policy, and when would you use conditional logic in the gateway?

**Concepts**
- choose with when/otherwise — APIM if/else for routing logic
- Boolean expressions over context for branching
- Multiple auth styles — SOAP UsernameToken vs Bearer JWT
- Business rules belong in the backend, not in the gateway

**Answer**

The `<choose>` policy evaluates one or more `<when condition="@(...)">` expressions and executes the first matching branch with an optional `<otherwise>` fallback — it is APIM's equivalent of if/else for routing different auth methods or response formats. I use it when the same API must accept multiple authentication styles — for example SOAP UsernameToken in the body versus a Bearer JWT in the `Authorization` header — and each style needs a different validation path. Conditions are Boolean expressions over `context`, so I can branch on header presence, HTTP method, product name, or variables set earlier in the pipeline. Heavy business rules with many branches belong in the backend service; `<choose>` is best for gateway concerns such as auth mode selection, content-type handling, and environment-specific routing.

---

## Q17. How does the `<validate-jwt>` policy validate bearer tokens, and what OpenID Connect metadata does it use?

**Concepts**
- openid-config URL — fetches current signing keys automatically
- Audience and issuer validation against declared lists
- require-expiration-time and require-scheme enforcement
- 401 or 403 returned on validation failure

**Answer**

The `<validate-jwt>` policy parses the JWT from a configured header or query parameter, fetches signing keys from an OpenID Connect discovery document, and verifies signature, issuer, audience, expiration, and optional claims before the request proceeds — returning 401 or 403 on failure. The `<openid-config url="..." />` element points to the well-known metadata URL such as `https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration` so APIM retrieves current signing keys automatically and handles key rotation without redeployment. I list acceptable audiences and issuers explicitly; tokens meant for a different API or tenant are rejected even if cryptographically valid. Attributes like `require-expiration-time="true"` and `require-scheme="Bearer"` enforce baseline OAuth 2.0 bearer token rules at the edge.

```xml
<validate-jwt header-name="Authorization" require-scheme="Bearer">
  <openid-config url="https://login.example.com/.well-known/openid-configuration" />
  <audiences><audience>api://my-app</audience></audiences>
</validate-jwt>
```

---

## Q18. What is the difference between validating a JWT at APIM versus validating it in the backend ASP.NET Core API?

**Concepts**
- APIM validation — blocks invalid tokens at the edge, reduces backend load
- ASP.NET Core AddJwtBearer — rich claim-based authorization handlers
- Key rotation handled by OpenID metadata in both locations
- Defense in depth when both layers validate

**Answer**

Validating at APIM rejects unauthorized traffic before it reaches my servers, reducing load and hiding backend URLs, while validating in ASP.NET Core with `AddJwtBearer` gives the API full access to claims for authorization handlers but consumes compute on every request including junk traffic. APIM validation is coarse-grained and fast — it checks token validity but cannot easily enforce fine-grained scope or role requirements using C# policies. ASP.NET Core validation through `[Authorize]`, policies, and handlers can check `scope`, `roles`, `oid`, and custom claims with full .NET logic. Many production setups validate at APIM for baseline access control and still configure JWT bearer authentication in the API for fine-grained checks — duplicate validation is acceptable when backends can also be reached through other paths, since relying solely on APIM would create a single security perimeter.

---

## Q19. How can APIM integrate with Microsoft Entra ID (Azure AD) or Azure AD B2C for OAuth 2.0?

**Concepts**
- validate-jwt with Entra ID tenant discovery endpoint
- Audience — Application ID URI of the API registration
- Azure AD B2C — user flow-specific discovery URL
- Developer portal as OAuth 2.0 client for Entra ID sign-in

**Answer**

APIM can protect APIs with `<validate-jwt>` against tokens issued by Microsoft Entra ID or Azure AD B2C by pointing the OpenID configuration at the tenant's discovery endpoint and listing the app registration's Application ID URI as an allowed audience. For Entra ID protected APIs, the audience is typically `api://{client-id}` or the application ID URI configured on the exposed API app registration. For Azure AD B2C, the OpenID URL includes the B2C policy segment such as `.../B2C_1_signupsignin/v2.0/.well-known/openid-configuration` because each user flow publishes separate metadata. The developer portal can also be configured as an OAuth 2.0 client so partners sign in with Entra ID and receive subscription keys, aligning portal identity with corporate directories.

---

## Q20. How would you implement custom authentication logic (such as SOAP UsernameToken or legacy basic auth) in APIM policies?

**Concepts**
- set-variable to extract credentials from XML body or headers
- choose branches for each auth style in the inbound section
- send-request to call token or validation service
- return-response with SOAP fault or HTTP 401 on failure
- JWT and legacy SOAP clients handled by separate when branches

**Answer**

Custom auth in APIM combines `<set-variable>` expressions that parse the request body or headers, `<choose>` branches for each auth style, `<send-request>` calls to a token or validation service, and `<return-response>` with SOAP faults or HTTP 401 when validation fails — all in the inbound section before `<forward-request>`. For SOAP UsernameToken, policies read the XML body with `context.Request.Body.As<XElement>(preserveContent: true)`, extract username and password nodes via XPath or LINQ to XML, and reject the call if either is empty. After extracting credentials, the gateway can obtain an access token via client credentials and POST the username/password to an internal validation API, then store returned roles in `context.Variables` for later routing decisions. JWT-based clients on the same API take a separate `<when>` branch that runs `<validate-jwt>` instead of SOAP parsing, so one APIM operation supports legacy and modern clients simultaneously.

---

## Q21. What is mutual TLS (mTLS) in Azure API Management, and when is it used?

**Concepts**
- Client certificate during TLS handshake proves client identity
- APIM enforcing client certificate validation at the gateway
- Backend mTLS — APIM presents cert when calling upstream services
- Certificate-based identity stronger than subscription keys

**Answer**

Mutual TLS requires the client to present an X.509 certificate during the TLS handshake in addition to the server certificate, and APIM can enforce client certificate validation at the gateway or present its own certificate to backends that require it. Client certificate authentication suits machine-to-machine integrations where subscription keys or passwords are considered too weak or hard to rotate at scale. On Premium with VNet integration, I configure trusted client CAs and optional certificate pinning so only partners with issued certs can call the API. Backend mTLS is the reverse: APIM presents a client cert when forwarding to the backend so the upstream service knows the call originated from the gateway. I use mTLS when regulatory or contractual requirements demand certificate-based identity beyond bearer tokens or API keys.

---

## Q22. What is the difference between `<rate-limit>` and `<quota>` policies in APIM?

**Concepts**
- rate-limit — short rolling window cap to prevent bursts, HTTP 429
- quota — longer-period total usage cap for commercial plan tiers
- rate-limit-by-key and quota-by-key for per-subscription dimensions
- Technical protection vs business entitlement enforcement

**Answer**

`<rate-limit>` caps how many calls are allowed in a short rolling window such as 100 calls per minute to prevent bursts, while `<quota>` caps total usage over a longer period such as 10,000 calls per month to enforce commercial plan limits. Rate limits protect backend stability from traffic spikes; when exceeded, APIM returns HTTP 429 with a `Retry-After` hint. Quotas align with subscription or product tiers — a free product might allow 1,000 calls per month and a paid product 1 million — and reset on a defined calendar or rolling interval. Both policies can be scoped by subscription key, IP address, or other dimensions using variants such as `<rate-limit-by-key>`. I apply rate limits for technical protection and quotas for business entitlements; many products use both together.

---

## Q23. How do rate-limit counters work across APIM instances, and what stores them on the Premium tier?

**Concepts**
- Distributed counters shared across all gateway instances in a region
- Premium multi-region consistent enforcement
- Counter keys derived from subscription ID or client IP
- Backend throughput unaffected since enforcement is at the gateway

**Answer**

APIM maintains distributed counters for rate limits and quotas so all gateway instances in a region share the same counts, which means a client cannot bypass limits by hitting different nodes. On Premium tier with multiple units or regions, counters synchronize through Azure-managed storage backing the service so enforcement is consistent across scale units. On lower tiers, counters are scoped to the service's deployed units within a region; Premium adds multi-region gateway deployment with consistent policy enforcement when configured. Counter keys derive from the policy dimension — typically subscription ID or client IP — so two different subscriptions each get independent buckets. Because counters are external to my backend, moving rate limiting to APIM gives consistent enforcement even when several App Service instances scale out behind the gateway.

---

## Q24. How can you implement subscription-based or IP-based throttling in APIM?

**Concepts**
- rate-limit-by-key with subscriptionId dimension
- IP-based key using clientIpAddress or X-Forwarded-For
- Trusting forwarded headers only from known proxy IPs
- Combining subscription and IP dimensions for layered protection

**Answer**

Subscription-based throttling uses `<rate-limit-by-key>` keyed on `{subscriptionId}`, while IP-based throttling keys on `{clientIpAddress}` or a forwarded header I trust, applied in inbound policies before the backend call. Subscription throttling ties naturally to APIM's subscription model — each product tier gets different `calls` and `renewal-period` values on `<rate-limit-by-key>`. IP throttling protects public endpoints from abuse when callers do not present a subscription key, or adds a second dimension alongside subscription limits. When clients pass through upstream proxies, I configure `<set-variable>` to read `X-Forwarded-For` carefully and only trust it from known proxy IPs to avoid spoofing. I combine both when partners authenticate with keys but anonymous health-check endpoints still need per-IP caps.

---

## Q25. What are the `<cache-lookup>` and `<cache-store>` policies used for?

**Concepts**
- cache-lookup checks for cached response by computed cache key
- cache-store saves backend response with defined TTL
- Cache key must include values that distinguish responses
- Internal APIM cache vs Redis or CDN for global distribution

**Answer**

The `<cache-lookup>` and `<cache-store>` pair enables response caching inside APIM: lookup checks whether a cached response exists for a computed cache key and skips the backend on a hit, and store saves the backend response for future requests with a defined time-to-live. Cache keys should include values that distinguish responses — URL path, query string, relevant headers — so one user does not receive another user's data. Caching only safe, idempotent GET responses is standard practice; marking personalized or authorized data as cacheable requires including subscription ID or user claims in the key. Internal APIM cache reduces latency and backend load for read-heavy public APIs; it is not a replacement for Redis or CDN caching at the edge when global distribution is required.

---

## Q26. What is the APIM developer portal, and what can external developers do with it?

**Concepts**
- Self-service portal for API consumer onboarding
- Interactive documentation and try-it-out console
- Subscription key retrieval without operator involvement
- Customizable with themes, custom pages, or full replacement

**Answer**

The APIM developer portal is a customizable self-service website where API consumers browse product catalogs, read interactive documentation, register accounts, subscribe to products, and retrieve subscription keys without opening a ticket to the platform team. Developers see operation descriptions, try-it-out consoles, and code samples generated from imported OpenAPI definitions. Subscription workflows can require administrator approval for protected products or be automatic for open products. The portal can be themed, extended with custom pages, or replaced entirely while still using APIM's subscription and identity backend. It separates API publishing — what my team exposes — from API consumption — how partners onboard themselves.

---

## Q27. How do you import an OpenAPI (Swagger) specification into Azure API Management?

**Concepts**
- Import via Azure portal, REST API, ARM/Bicep, or CI/CD pipeline
- Backend service URL set at import time or updated afterward
- Operations generated from HTTP methods and paths in the spec
- Policies attached after import; spec does not carry auth or throttling

**Answer**

I import an OpenAPI JSON or YAML document through the Azure portal under APIs → Add API → OpenAPI, the APIM REST API, ARM/Bicep, or CI/CD pipelines, and APIM generates operations, parameters, and models from the spec while letting me set the backend service URL and apply policies afterward. During import I specify the API URL suffix, backend base URL such as my ASP.NET Core App Service hostname, and optional version or revision settings. Import creates operation stubs aligned with HTTP methods and paths in the spec; I still attach authentication, rate limits, and rewrites in policy after import. Continuous deployment pipelines often store the OpenAPI file in source control and redeploy to APIM on each release so documentation and the live gateway stay synchronized with the ASP.NET Core project.

---

## Q28. What common mistake do teams make when they put business logic in APIM policies instead of backend services?

**Concepts**
- APIM policies suited for gateway concerns, not domain logic
- Policy XML lacks unit tests, static analysis, and pull-request culture
- Policy errors surface only at runtime in production traffic
- Backend code redeployment safer and more maintainable

**Answer**

Teams sometimes implement multi-step business workflows, database lookups, or domain rules entirely in APIM policy XML because it is convenient at the gateway, but that logic becomes hard to test, version, debug, and reuse compared with code in the ASP.NET Core backend. APIM policies excel at cross-cutting gateway concerns — auth, throttling, routing, header injection, and simple transforms — not at maintaining order totals, inventory checks, or saga orchestration. Complex C# expressions embedded in XML lack unit tests, pull requests, and static analysis that normal `.cs` projects provide, and policy errors surface only at runtime in production traffic. When requirements change frequently, redeploying backend code through standard CI/CD is safer than editing a policy fragment that multiple APIs include. I keep APIM thin: validate and protect at the edge, and execute business rules in the API service where domain experts and developers already work.

---

## Gotchas — Azure API Management (Interview Traps)

---

#### Gotcha 1. Backend URL hardcoded in set-backend-service policy bypasses the backend pool — version changes require policy edits

**Concepts**
- `set-backend-service` in inbound policy overrides the API's configured backend URL
- Backend entities (Backend resource in APIM) enable named, versioned backend URLs
- Hardcoded URL in policy creates a deployment coupling between policy XML and backend versions
- Policy expressions can use named values to parameterize URLs without backend entities

**Answer**

A `set-backend-service` policy element with a literal URL (`base-url="https://myapi.azurewebsites.net"`) bypasses APIM's backend configuration entirely. When the backend URL changes — for example after migrating to a new App Service or introducing a new version — the policy XML must be edited manually in the portal or through ARM/Bicep. The APIM Backend resource (`<backend-id>`) solves this by letting you define a named backend URL once and reference it from multiple policies, so a URL change only requires updating the backend entity rather than every policy that references the URL.

---

#### Gotcha 2. Subscription key passed as a query string parameter is logged by proxies and appears in browser history

**Concepts**
- APIM accepts subscription keys via `Ocp-Apim-Subscription-Key` header or `subscription-key` query parameter
- Query parameter keys appear in server access logs, CDN logs, and browser history
- Header-based key transmission avoids query string logging exposure
- Disable query string key transmission in the APIM product settings to enforce header-only

**Answer**

Azure APIM accepts subscription keys in both the `Ocp-Apim-Subscription-Key` request header and the `subscription-key` query string parameter. Clients who send the key as a query parameter expose it in server-side access logs, reverse proxy logs, CDN access logs, browser address bar history, and referrer headers of subsequent requests. This is an accidental secret exposure that many developers miss because the default APIM test console uses the query parameter for convenience. The fix is to enforce header-only transmission by disabling the query string option in the APIM product or API settings.

---

#### Gotcha 3. validate-jwt policy does not validate the audience claim by default — tokens from unrelated apps pass

**Concepts**
- JWT validation in APIM checks signature and expiry by default
- `<required-claims>` block is needed to enforce `aud` claim values
- A token issued for Application A is valid against Application B's APIM policy without explicit audience validation
- Audience must match the API's app registration application ID URI

**Answer**

The APIM `validate-jwt` policy validates the token's signature against the issuer's signing keys and checks that the token has not expired, but it does not enforce the `aud` (audience) claim unless you add a `<required-claims>` block that explicitly requires the expected audience value. Without audience validation, an access token issued for a different application in the same Entra ID tenant passes APIM validation and reaches the backend. Every `validate-jwt` policy in production must include `<required-claims><claim name="aud" match="any"><value>{expected-app-id-uri}</value></claim></required-claims>` to prevent token cross-use.

---

#### Gotcha 4. Caching policy without tenant-discriminating vary-by returns one tenant's response to another

**Concepts**
- APIM caching uses the URL and any configured `vary-by-header` or `vary-by-query-parameter` as the cache key
- Multi-tenant APIs must include tenant identifier (`tid` claim, Authorization header, or tenant path segment) in the cache key
- A cache hit for tenant A's cached response is served to tenant B if the key is identical
- The `vary-by-header` element adds headers to the cache key for per-caller differentiation

**Answer**

The APIM `cache-lookup` policy stores responses keyed by URL path. For a multi-tenant API where the tenant context is carried in the JWT `tid` claim or the `Authorization` header, the default URL-only cache key will serve one tenant's cached response to a different tenant when both make the same request path. The `cache-lookup` policy's `vary-by-header` element must include `Authorization` or an explicit `X-Tenant-Id` header in the cache key to ensure each tenant receives their own cached response. Failure to include tenant discriminators in the cache key is a data isolation security vulnerability.

---

#### Gotcha 5. Inbound policy order matters — JWT validation placed after rate-limit or transformation policies leaks rate limit budget to unauthenticated callers

**Concepts**
- APIM evaluates inbound policy elements top-to-bottom
- `rate-limit` before `validate-jwt` counts unauthenticated requests against the quota
- Security validation should appear first in the inbound block to fail fast
- Policy scopes (global, product, API, operation) apply in order; later scopes can override earlier ones

**Answer**

APIM policy elements in the `<inbound>` section execute in the order they are written. If a `rate-limit` policy appears before `validate-jwt`, unauthenticated requests are counted against the rate limit before being rejected, allowing an attacker to exhaust the rate limit budget with token-less requests while legitimate callers are throttled. Authentication and authorization policies (`validate-jwt`, `check-header`) should be the first policies in the inbound block so requests that will ultimately be rejected never consume rate limit, transformation, or logging budget.

---

#### Gotcha 6. Named values are not secret by default — plain named values are visible to any user with APIM portal access

**Concepts**
- APIM Named Values can be plain or secret type
- Plain named values are visible in cleartext in the portal and ARM templates
- Secret named values are masked in the portal but stored in APIM's internal store, not Key Vault
- Key Vault-backed named values require the APIM managed identity to have Key Vault Secrets User role

**Answer**

APIM Named Values are frequently used to parameterize policy expressions (API keys for backends, JWT signing secrets). Plain named values are stored as cleartext and visible to any portal user with APIM Contributor access, making them unsuitable for storing secrets. Marking a named value as "secret" in the portal masks its value in the UI but stores it in APIM's own encrypted store, not Azure Key Vault. For production secrets, named values should be backed by Key Vault secrets, requiring the APIM service's managed identity to have the Key Vault Secrets User role and the Key Vault's soft-delete enabled, or the reference will fail silently.

---

#### Gotcha 7. APIM does not retry failed backend calls by default — a slow or 5xx backend returns immediately to the caller

**Concepts**
- APIM forwards backend errors to the caller without retry unless a `retry` policy is configured
- `retry` policy with `condition="@(context.Response.StatusCode >= 500)"` adds backend resilience
- Retry on the same backend instance does not help if the issue is instance-level; `set-backend-service` rotation is needed
- Circuit-breaker pattern requires policy conditions and a custom on-error policy

**Answer**

Azure APIM acts as a transparent proxy by default; it forwards the backend's 502, 503, or 504 response directly to the caller without retrying. Adding a `retry` policy element to the inbound section adds resilience for transient backend failures. However, retrying against the same backend URL when the backend instance itself is down is futile; true failover requires a backend pool with multiple backends and a `set-backend-service` policy that selects a healthy backend. Without explicit retry and circuit-breaker policies, APIM provides no protection against transient backend degradation.

---

#### Gotcha 8. Self-hosted gateway loses connection to APIM control plane if outbound HTTPS is blocked — it processes no traffic

**Concepts**
- Self-hosted gateway polls the APIM control plane for configuration updates over HTTPS
- Firewall rules that block outbound 443 to APIM's management endpoint disconnect the gateway
- A disconnected self-hosted gateway can still process traffic using cached configuration for a time
- After cache expiry, the gateway starts rejecting requests

**Answer**

The APIM self-hosted gateway maintains a persistent HTTPS connection to the APIM control plane to receive configuration, certificates, and policy updates. If an on-premises or private-network firewall blocks outbound port 443 to the APIM management endpoint (`*.azure-api.net`), the gateway silently disconnects. For a configured timeout period, the gateway continues processing requests using its cached configuration, after which it begins rejecting requests with errors. This is a common failure mode when deploying self-hosted gateways in corporate networks with strict egress filtering where Azure hostnames are not in the allowlist.

---

#### Gotcha 9. APIM latency adds 20-100 ms per call — routing every microservice internal call through APIM is an anti-pattern

**Concepts**
- APIM is a gateway for external-facing APIs, not an internal service mesh
- Each APIM request adds network round-trip, policy evaluation, and logging overhead
- Internal microservice-to-microservice calls should use direct service URLs or a service mesh
- Over-routing causes latency multiplication in request chains

**Answer**

Azure API Management is optimized for managing external API traffic: authentication, throttling, transformation, and monitoring at the API boundary. Every request that flows through APIM incurs additional network latency (20–100 ms depending on tier and region), policy evaluation time, and logging overhead. Architectures that route every internal microservice-to-microservice call through APIM multiply this latency across each hop in a request chain. APIM belongs at the edge of the system, exposing APIs to external consumers; internal service-to-service communication should use direct service URLs, Azure Service Bus for async calls, or a service mesh like Dapr.

---

#### Gotcha 10. APIM API versioning by URL path requires explicit product and subscription assignment for each version — missing assignment returns 401

**Concepts**
- APIM API versions (`/v1/`, `/v2/`) are separate API entities in the portal
- Products and subscriptions control which consumers can call each API version
- A new API version that is not added to a product is inaccessible even with a valid subscription key
- Version sets link versioned APIs but do not inherit product assignments

**Answer**

When you add a new version of an API in APIM using URL path versioning (`/v1/` and `/v2/`), the new version is a separate API entity. Products — which gate access via subscription keys — must explicitly include each version of the API. A new `v2` API that is published but not added to any product returns 401 Unauthorized to all callers even when they supply a valid subscription key, because the key is scoped to a product that does not include `v2`. Developers who test with the APIM subscription master key (which bypasses product restrictions) during development miss this gap and discover it when customers' product-scoped keys fail in production.

---
