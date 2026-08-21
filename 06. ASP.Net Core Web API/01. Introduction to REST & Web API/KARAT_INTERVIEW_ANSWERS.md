# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/01. Introduction to REST & Web API`

---

#### Q1. (R) Review this "RESTful" order API. QA reports duplicate charges when users refresh the browser after checkout. Which REST constraints are violated and what status codes should change?

**Answer:** Charging money via `GET` violates safe-method semantics and REST's uniform interface — browsers, proxies, and prefetchers may repeat the request, causing duplicate side effects. Checkout must be a non-safe verb (`POST`) with `201 Created` or `303 See Other`, never `GET` with `200 OK`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| REST / HTTP | `GET` performs state change (`Charge`) | Not safe; retries and refreshes re-execute payment |
| REST | Uses query string for mutation | Violates idempotent read expectations; logged in access logs |
| API contract | Returns `200 OK` for resource creation | Clients cannot distinguish create vs read; no `Location` header |
| Security | Sensitive `amount` in URL | Leaks PII/financial data via logs, referrer headers, browser history |

**Fix (priority order):**

1. Replace with `[HttpPost]` (or `POST /api/orders`) that creates an order — return `CreatedAtAction` with `201`.
2. Accept payment details in the request body, not query string.
3. Add idempotency key header (`Idempotency-Key`) for payment retries if the gateway supports it.

**Production takeaway:** Karat uses "GET checkout" to test whether you connect **HTTP method safety** to real money bugs — not whether you can recite REST acronym expansions.

---

#### Q2. (R) A legacy integration team exposes this controller and claims it is REST. Identify RPC-in-REST smells and how you would reshape routes and verbs without breaking existing clients immediately.

**Answer:** Every operation is a POST to a verb-named sub-path with RPC-style names — that is remote procedure call over HTTP, not resource-oriented REST. Migrate toward noun-based routes and correct HTTP verbs while keeping deprecated RPC routes behind a version or `[Obsolete]` shim during transition.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Verb in URL (`CreateOrder`, `CancelOrder`) | Non-standard contract; hard to cache and document in OpenAPI |
| HTTP | `GetOrderById` uses POST with body | Breaks caching, CDN rules, and HTTP semantics |
| REST | No stable resource identifiers in path | Clients cannot link to `/api/orders/{id}` |
| Maintainability | One POST pattern for all operations | Gateway auth, rate limits, and monitoring cannot differ by operation type |

**Fix (priority order):**

1. Introduce canonical routes: `POST /api/orders`, `GET /api/orders/{id}`, `DELETE /api/orders/{id}` (or PATCH for cancel state).
2. Keep old routes as thin wrappers calling the same service layer — mark deprecated in OpenAPI.
3. Publish migration guide with sunset date; add integration tests for both surfaces during overlap.

**Production takeaway:** Real migrations rarely flip every client overnight — show **resource modeling plus backward-compatible deprecation**, not purity lectures.

---

#### Q3. (R) Review error handling in this inventory API. Clients cannot distinguish "bad request" from "not found," and failed updates sometimes return HTTP 200 with `{ "success": false }`.

**Answer:** Wrapping failures in HTTP 200 with a boolean flag breaks REST clients, monitors, and retry logic — HTTP status codes exist precisely to signal outcome class. Return `400 Bad Request` for validation failures and `404 Not Found` when the resource id does not exist.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP semantics | Validation failure returns `200 OK` | Load balancers and APM treat request as success |
| HTTP semantics | Missing resource returns `200` with message | Clients must parse body; caches may store "errors" |
| API design | Ad-hoc `{ success, message }` envelope | Inconsistent with `ProblemDetails` and OpenAPI tooling |
| Observability | Alerts on 5xx/4xx rates miss business failures | Incidents hide in 200 traffic |

**Fix (priority order):**

1. Return `BadRequest(ProblemDetails)` or `ValidationProblem(ModelState)` for invalid quantity.
2. Return `NotFound()` when `Find` returns null.
3. Return `204 NoContent` or `200` with updated DTO on success — not a success flag wrapper.

**Production takeaway:** **Status code discipline** is part of REST maturity — envelope JSON does not replace HTTP semantics in production APIs.

---

#### Q4. (M) A mobile client retries `PUT /api/customers/42` after a timeout. The first request actually succeeded but the client never received the response. Explain why PUT is the right verb here and what idempotency guarantees the server should document.

**Answer:** PUT to a known URI is defined as idempotent — repeating the same full representation should converge the resource to the same state, so a retry after a network timeout should not create a second customer or duplicate side effects. Document that PUT replaces the resource at `{id}` and that repeated identical payloads yield the same final state (same etag/version).

- **Idempotency:** Multiple identical PUTs should not multiply records or increment counters unintentionally — server applies replace semantics on `/customers/42`.
- **Contrast with POST:** `POST /api/customers` creates a new resource each retry unless you add an idempotency key — wrong verb for "update known id."
- **Response on retry:** Second PUT may return `200 OK` or `204 NoContent` instead of `201` — both are valid; include `ETag` for concurrency control.
- **Not automatic:** If your handler increments `Revision` on every PUT regardless of body equality, document that behavior — true idempotency may require comparing payload hash.
- **Client guidance:** Safe to retry PUT on timeout; use `If-Match` etag to avoid lost updates when payload changed between attempts.

**Production takeaway:** Idempotency is a **contract promise** tied to verb + server behavior — Karat wants scenario reasoning, not "PUT is idempotent" in isolation.

---

#### Q5. (R) Review this search endpoint from a security audit. What REST and HTTP semantics are wrong, and what breaks under caching proxies?

**Answer:** `GET /delete` performs destructive work with a safe verb, and putting secrets in query strings violates transport and caching expectations. Both endpoints will misbehave behind shared caches and violate REST safe-method rules.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| REST / HTTP | Delete via GET | Crawlers, prefetch, CDN cache may trigger deletion |
| Security | `apiKey` in query string | Keys appear in logs, analytics, browser history |
| Caching | GET responses may be cached | Search results stale; destructive GET catastrophic if cached path wrong |
| API design | Action name in path for mutation | RPC pattern; not addressable as `/api/items/{id}` |

**Fix (priority order):**

1. Change delete to `[HttpDelete("{id:int}")]` or POST to a sub-resource only if DELETE is blocked (document exception).
2. Move API key to header (`Authorization` or `X-Api-Key`).
3. Set `Cache-Control: no-store` on sensitive GETs; never mutate on GET.

**Production takeaway:** REST constraint violations become **security and cache incidents** in production — not style guide nitpicks.

---

#### Q6. (D) Product asks for full HATEOAS on every list response (`_links.self`, `_links.next`, etc.). When is hypermedia worth the contract cost in a B2B JSON API, and when is a stable OpenAPI document plus explicit pagination query params enough?

**Answer:** HATEOAS pays off when clients must discover available actions dynamically (long-lived public APIs, evolving workflows) or when you want server-driven state transitions; for most internal B2B integrations with code-generated clients, OpenAPI plus stable pagination parameters is simpler and equally production-ready.

- **Favor HATEOAS** when many heterogeneous clients consume the same API without coordinated releases, or when legal/compliance requires server-controlled "allowed next steps."
- **Skip full HATEOAS** when all consumers are your mobile/web apps with simultaneous deploys — links duplicate what OpenAPI already documents.
- **Pragmatic middle:** Include `Link` header or minimal `next`/`prev` URLs for pagination without full HAL/JSON-LD on every entity.
- **Cost:** Hypermedia complicates caching, testing fixtures, and versioning — every new link relation becomes a breaking-change surface.
- **Karat angle:** Saying "HATEOAS is optional in REST" is correct — **Richardson maturity level 3** is aspirational, not a gate for shipping.

**Production takeaway:** Judgment beats ideology — choose hypermedia when **discovery reduces coupling**, not because a checklist says level 3.

---

#### Q7. (P) You are introducing `/api/v2/customers` while v1 stays live for six months. Compare URL path versioning, `X-Api-Version` header, and `Accept: application/vnd.company.customers.v2+json` — which fits gateway routing, mobile apps, and breaking DTO changes?

**Answer:** URL path versioning (`/api/v2/customers`) is easiest for gateways, logs, and mobile hard-coded base paths; header or media-type versioning keeps URLs clean but complicates caching, browser testing, and CDN rules — pick one org-wide and enforce via ASP.NET Core API versioning middleware.

- **URL path:** Route rules in nginx/YARP are trivial; Swagger can expose `/v1` and `/v2` groups; breaking renames are obvious to partners.
- **Header (`X-Api-Version`):** Same URL for all versions — good for additive changes; bad when proxies strip unknown headers or caches key only on URL.
- **Media type (Accept vendor MIME):** Fine-grained content negotiation; heavy for mobile teams and Postman users; aligns with strict REST purists.
- **Breaking DTO changes:** Prefer new version with mapped adapters — never silently change v1 field types.
- **ASP.NET Core:** Use `Asp.Versioning.Mvc` with `[ApiVersion("2.0")]` and dual-map controllers or versioned Swagger docs.

**Production takeaway:** Versioning is an **ops + client lifecycle** decision — preview chapter sets up that you will implement, not just name three strategies.

---

#### Q8. (R) Review this mixed-style API surface from a fintech partner integration. Prioritize the highest-risk contract issues for production.

**Answer:** Route template stacking is inconsistent (double `api` prefix, missing segments), DELETE-with-body is poorly supported by clients and proxies, and verb-in-path RPC remains — the worst immediate risk is unreachable or ambiguous routes after `[controller]` expansion combined with absolute paths on actions.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Routing | `[Route("api/transfers/sendMoney")]` on action ignores class `[Route("api/[controller]")]` convention | Duplicate or unexpected URLs; OpenAPI shows wrong paths |
| Routing | `[HttpDelete("transfers")]` may resolve to `/api/Transfers/transfers` | Partner docs diverge from actual matcher |
| HTTP | DELETE with body | Some clients omit body; intermediaries may strip it |
| Design | `sendMoney` verb in path | Non-REST; cannot apply standard idempotency keys on resource |
| Consistency | Mixed absolute and relative route attributes | Link generation and `CreatedAtAction` break |

**Fix (priority order):**

1. Normalize class route: `[Route("api/[controller]")]`; actions use `[HttpPost]`, `[HttpGet("{id}/status")]`, `[HttpDelete("{id}")]` only.
2. Replace DELETE body with `DELETE /api/transfers/{id}` or POST `/api/transfers/{id}/cancellations` with idempotency key.
3. Regenerate OpenAPI and contract tests against partner sandbox before go-live.

**Production takeaway:** **Route hygiene** is REST/Web API basics — messy templates cause production outages before anyone debates HATEOAS.
