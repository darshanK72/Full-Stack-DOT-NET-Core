# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/04. Content Negotiation & Formatters`

---

#### Q1. (R) Review serialization for a dual-client API. The JavaScript web app binds correctly; a legacy integration test sends PascalCase JSON and `CustomerName` arrives null.

**Answer:** ASP.NET Core 8 Web API templates default `System.Text.Json` to **camelCase** property names for JSON — incoming PascalCase `CustomerName` does not bind to `CustomerName` unless case-insensitive matching is enabled or clients send camelCase. The web app works; legacy PascalCase clients silently get defaults.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Serialization | Default JSON naming is camelCase (`customerName`) | PascalCase payloads skip property binding |
| Validation | Empty name triggers manual BadRequest | False negatives for valid legacy clients |
| Contract | Undocumented breaking change from Newtonsoft defaults | Migration surprises |
| Testing | Only modern client covered in CI | Legacy partner fails in production |

**Fix (priority order):**

1. Prefer client fix: send `{ "customerName": "Acme", "creditLimit": 5000 }`.
2. Or configure: `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);` during migration.
3. Document breaking naming policy in API changelog — do not rely on silent defaults.

**Production takeaway:** Debrief question **"why CustomerName becomes customerName"** — outbound camelCase; inbound also expects camelCase unless configured otherwise.

---

#### Q2. (R) Review this export endpoint. A partner sends `Accept: application/xml` but always receives JSON with HTTP 200.

**Answer:** With only `System.Text.Json` input/output formatters registered, the framework cannot satisfy `application/xml` — it falls back to JSON (or default formatter) instead of negotiating XML. Register XML formatters or restrict the endpoint to JSON and return 406 for unsupported Accept types.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Content negotiation | No XML output formatter registered | Accept header ignored |
| Contract | Partner expects XML schema validation | Downstream parsing fails despite HTTP 200 |
| API honesty | Returns JSON without `406` | Client cannot detect mismatch via status |
| Legacy | Enterprise clients often require XML | Integration blocked |

**Fix (priority order):**

1. If XML required: `builder.Services.AddControllers().AddXmlSerializerFormatters();` (or DataContract serializers).
2. If JSON-only: `[Produces("application/json")]` and enable 406 on unsupported Accept via formatter configuration.
3. Add integration test with `Accept: application/xml` asserting content type and body.

**Production takeaway:** **Accept header is a request for a representation** — without a matching formatter, you must fail explicitly or register the formatter.

---

#### Q3. (R) Review content negotiation failure handling. QA expects HTTP 406 when an unsupported `Accept` header is sent; API returns JSON 200.

**Answer:** `[Produces("application/json")]` documents intent but does not alone return 406 — default formatter selection may still write JSON when no formatter matches the Accept header unless `ReturnHttpNotAcceptable` is enabled or no compatible formatter exists and fallback is disabled.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP semantics | Unsupported Accept should yield `406 Not Acceptable` | Clients assume PDF contract incorrectly |
| Configuration | Default may fall back to first formatter | Silent wrong content type |
| Testing | QA scenario valid for strict APIs | Compliance failures |
| Documentation | OpenAPI lists only JSON — PDF not advertised | Still should not return fake PDF |

**Fix (priority order):**

1. Configure MVC: `options.ReturnHttpNotAcceptable = true;` on formatter options (or ensure no fallback formatter handles pdf).
2. Keep `[Produces("application/json")]` accurate — remove unsupported types from `[Produces]`.
3. Return `StatusCode(406)` explicitly for known unsupported types if custom logic required.

**Production takeaway:** **406 is the honest response** when you cannot produce any acceptable representation — better than wrong-format 200.

---

#### Q4. (P) Explain how `System.Text.Json` camelCase naming is configured in ASP.NET Core 8 Web APIs, and why `[JsonPropertyName("customer_name")]` on one property affects the whole contract story.

**Answer:** Web SDK sets camelCase via `JsonNamingPolicy.CamelCase` in `AddJsonOptions`; individual `[JsonPropertyName]` overrides create exceptions that clients must special-case — fine for external schema locks, dangerous when sprinkled inconsistently.

- **Default:** `builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);` — often already applied by template.
- **Outbound:** `CustomerName` serializes as `customerName` in JSON responses.
- **Inbound:** Deserialization expects camelCase keys matching policy unless `PropertyNameCaseInsensitive = true`.
- **`[JsonPropertyName("customer_name")]`:** Forces snake_case for one property — document in OpenAPI; mixed policies confuse code generators.
- **Newtonsoft:** If `AddNewtonsoftJson()` used, separate camelCase settings apply — do not mix serializers on same app without team agreement.

**Production takeaway:** Naming policy is a **published contract** — debrief camelCase question is about defaults plus migration, not memorizing attribute names.

---

#### Q5. (R) Review custom formatter registration. CSV downloads work locally but return empty bodies in staging — logs show formatter selected but model type mismatch.

**Answer:** `CanWriteType` returning `true` for everything hides that `context.Object` is not `IEnumerable<OrderDto>` — when the action returns `OkObjectResult` wrapping a different type, the invalid cast throws or writes empty output. Formatters must declare supported types and handle `ObjectResult` value types correctly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Formatter | `CanWriteType => true` too broad | Wrong formatter selected for non-CSV responses |
| Runtime | Unsafe cast `(IEnumerable<OrderDto>)context.Object!` | InvalidCastException or empty body in staging data edge cases |
| Design | Formatter assumes collection; action may return wrapper | Local smoke test with full dataset missed null path |
| Content negotiation | CSV formatter may run for JSON Accept | Corrupt JSON endpoints |

**Fix (priority order):**

1. `CanWriteType` return `typeof(IEnumerable<OrderDto>).IsAssignableFrom(type);`
2. In `WriteResponseBodyAsync`, pattern-match `context.Object` and handle null/empty safely.
3. Pin endpoint with `[Produces("text/csv")]` and verify Accept header in tests.

**Production takeaway:** Custom formatters must be **type-safe and narrow** — `CanWriteType true` is a production incident waiting to happen.

---

#### Q6. (M) A bank middleware still requires SOAP/XML for one endpoint while the rest of the platform is JSON. Where do input and output formatters sit in the pipeline relative to model binding, and what does `[Consumes("application/xml")]` change?

**Answer:** Input formatters deserialize the request body into action parameters before the action runs — model binding selects an input formatter based on Content-Type; output formatters run during result execution based on Accept and `[Produces]`. `[Consumes("application/xml")]` restricts which actions match XML Content-Type on input.

- **Input path:** Request → routing → model binding chooses input formatter by `Content-Type` → parameter object constructed → action executes.
- **Output path:** Action returns `IActionResult` → result executor asks output formatters (JSON, XML, CSV) using content negotiation.
- **`[Consumes("application/xml")]`:** Action selection filter — JSON POST to that action may return 415 Unsupported Media Type.
- **Legacy XML:** Register `AddXmlSerializerFormatters()` or custom Xml input formatter for that controller only.
- **Isolation:** Consider separate minimal controller or YARP route to XML adapter — do not force global XML for one client.

**Production takeaway:** Formatters are **pluggable serialization layers** at binding/result time — not middleware in the classic sense, but part of the MVC filter pipeline around the action.

---

#### Q7. (D) Leadership wants to drop XML support to reduce maintenance. One state-government client still posts `application/xml` to `POST /api/permits`. How do you decide retire vs adapter vs gateway translation?

**Answer:** Weigh client count, contract SLA, and translation cost — often an edge XML-to-JSON gateway or dedicated adapter service preserves the modern JSON core while honoring a single legacy consumer until contractual sunset.

- **Retire XML:** Acceptable when contract allows, client can migrate, and timeline is agreed — return `415` with migration guide.
- **Adapter service:** Small sidecar converts XML ↔ DTO ↔ JSON — keeps main API JSON-only and testable.
- **Gateway translation:** APIM/nginx script transforms payload — ops owns schema mapping; watch latency and error surfaces.
- **In-app XML:** `AddXmlSerializerFormatters` on one controller — lowest infra moving parts but spreads legacy into codebase.
- **Decision inputs:** Revenue/regulatory risk, test coverage for XML schemas, team skill, deprecation clause in partner agreement.

**Production takeaway:** Production APIs **sunset representations deliberately** — not by deleting formatters on a Friday without a migration path.

---

#### Q8. (R) Review `[Produces]` and `[ProducesResponseType]` usage. Swagger advertises XML and JSON responses; production returns JSON only and clients cache wrong content type.

**Answer:** Declaring `[Produces("application/json", "application/xml")]` without registering XML formatters misdocuments the API — Swagger shows XML clients cannot actually receive, and caches may store JSON under Accept XML expectations.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Documentation | `[Produces("application/xml")]` without formatter | False OpenAPI contract |
| Runtime | Always JSON body | Client XML parsers fail |
| Caching | `Vary: Accept` may be missing | Shared cache serves wrong representation |
| Compliance | Advertised formats must be honored or removed | Audit finding |

**Fix (priority order):**

1. Remove `application/xml` from `[Produces]` unless formatters registered and tested.
2. If XML needed, add `AddXmlSerializerFormatters()` and integration tests for both types.
3. Add `Response.Headers.Vary` for Accept when multiple representations are real.

**Production takeaway:** **`[Produces]` must match registered formatters** — OpenAPI lies become partner outages and cache poisoning.
