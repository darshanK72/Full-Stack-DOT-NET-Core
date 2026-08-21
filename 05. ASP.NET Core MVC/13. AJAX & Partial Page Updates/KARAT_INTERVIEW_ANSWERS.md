# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/13. AJAX & Partial Page Updates`

---

#### Q1. (D) PartialView vs JsonResult — which approach for comments, optimistic UI, and SEO?

**Answer:** Use **server-rendered HTML for the initial page and partial refreshes** when markup is complex and you want Razor encoding and shared partials; use **JSON only when the client owns rendering** (rich interactivity, non-HTML targets, or multiple consumers). For this MVC app, standardize on `PartialView` for list refresh and a small JSON endpoint only if optimistic UI needs immediate local state before the server confirms.

| Concern | PartialView (HTML fragment) | JsonResult + client template |
|---|---|---|
| Initial SEO / no-JS | Works — full page renders same partial server-side | Poor — comments invisible without JS |
| XSS safety | Razor encodes by default (`@item.Text`) | Easy to slip if templates use `.innerHTML` |
| Optimistic UI | Awkward — wait for HTML round-trip | Natural — append JSON row, reconcile on response |
| Contract stability | HTML shape tied to DOM/CSS | Explicit DTO contract; version independently |
| Caching | Must set headers carefully (see Q7) | Easier to cache JSON with ETags |

- **Standardize:** `_Comments` partial for initial render + AJAX refresh; optional `POST` returning `{ id, html }` only if you need both optimistic row and server markup.
- **Avoid** building HTML strings in JavaScript for user-generated content — duplicates encoding rules.
- If the same data feeds a mobile app, add a JSON API endpoint; do not force the web partial to return JSON.

**Production takeaway:** Karat tests judgment — PartialView is not "old" and JsonResult is not "modern"; pick by who owns markup and whether SEO/no-JS matters.

---

#### Q2. (R) AJAX POST returns 400 — missing antiforgery and wrong content type.

```csharp
[HttpPost]
public IActionResult AddToCart(int productId, int quantity)
{
    _cart.Add(productId, quantity);
    return PartialView("_CartSummary", _cart.GetSummary());
}
```

**Answer:** The request fails antiforgery validation (400) because global antiforgery is enabled and the fetch sends JSON without a verification token; even with a token, `[FromBody]` JSON would not bind to simple action parameters without a wrapper model or `[FromBody]` complex type.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Antiforgery | No `RequestVerificationToken` header or form field | 400 Bad Request before action runs |
| Binding | JSON body vs simple parameters | `productId`/`quantity` stay 0 — silent wrong cart state if validation were bypassed |
| Content-Type | `application/json` on MVC form-style action | Model binder expects form fields or explicit `[FromBody]` model |

**Fix (priority order):**

1. Send antiforgery token: read from `<input name="__RequestVerificationToken">` and set header `RequestVerificationToken` (or include in form body).
2. Match binding: use `application/x-www-form-urlencoded` or `FormData` with `productId` and `quantity`, **or** introduce `AddToCartRequest` with `[FromBody]`.
3. Add `[ValidateAntiForgeryToken]` explicitly on the action for clarity; return `PartialView` only after validation passes.

```javascript
const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
fetch('/Cart/AddToCart', {
    method: 'POST',
    headers: { 'RequestVerificationToken': token },
    body: new URLSearchParams({ productId: 42, quantity: 1 })
});
```

**Production takeaway:** MVC AJAX POSTs fail loudly on missing antiforgery — always pair token with the binding shape the action expects.

---

#### Q3. (R) jQuery → fetch migration — credentials and cross-origin.

**Answer:** `fetch` defaults to `credentials: 'same-origin'` and does not send antiforgery cookies/headers the way jQuery's same-origin POST with serialized form did; cross-subdomain posts are cross-origin, so cookies and antiforgery tokens are not sent unless CORS and credentials are configured correctly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| CORS / cookies | `api.example.com` vs `www.example.com` | Session and antiforgery cookie not sent — 401/400 |
| fetch defaults | No explicit `credentials: 'include'` | Cross-site cookie auth breaks vs jQuery same-origin |
| Antiforgery | Token not in header when using bare `URLSearchParams` | Validation fails even when form had token field |
| Architecture | MVC partial action on separate host | Wrong split — partials should stay same-origin |

**Fix (priority order):**

1. **Prefer same-origin** partial URLs (`/Cart/Add` on `www`) — partial HTML updates should not cross origins.
2. If API subdomain is required: configure CORS with `AllowCredentials`, echo specific origin (not `*`), and set `fetch(..., { credentials: 'include' })`.
3. Copy `__RequestVerificationToken` into `RequestVerificationToken` header on every mutating fetch.
4. Keep `Content-Type` implicit for `URLSearchParams` (browser sets `application/x-www-form-urlencoded`) — do not force JSON unless action expects it.

**Production takeaway:** jQuery `$.ajax` on the same page masked missing explicit CORS/credentials — fetch exposes cross-origin and antiforgery gaps immediately.

---

#### Q4. (R) Stored XSS in autocomplete partial via `Html.Raw`.

```csharp
<li data-term="@hit.Term">@Html.Raw(hit.HighlightedHtml)</li>
```

**Answer:** `HighlightedHtml` injects the raw user search term inside markup without encoding — Razor's `@hit.Term` in the attribute is encoded, but `Html.Raw` bypasses encoding in the body, enabling script injection when results are loaded into the DOM via `innerHTML`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| XSS | `Html.Raw` on user-influenced highlight HTML | Arbitrary script execution in victim browsers |
| AJAX surface | Partial loaded via `innerHTML` | Browser parses and executes injected markup |
| Data attribute | `data-term` encoded but body is not | Attack lives in list item content, not attribute |

**Fix (priority order):**

1. Encode first, highlight second: HTML-encode `hit.Term`, then wrap encoded text with `<mark>` server-side — never wrap raw user input.
2. Remove `Html.Raw`; use `<mark>@hit.Term</mark>` (Razor auto-encodes) or a safe helper that only adds allowed tags.
3. Prefer returning structured JSON (`term`, `displayText`) and build DOM with `textContent` if client renders.
4. Add Content-Security-Policy `script-src` to limit blast radius — defense in depth, not a substitute for encoding.

**Production takeaway:** Partial views do not sanitize by magic — any HTML fragment inserted via AJAX is an XSS surface if `Html.Raw` touches user data.

---

#### Q5. (R) AJAX filter — empty model from binding source mismatch.

```csharp
[HttpPost]
public IActionResult Filter([FromBody] ProductFilter filter)
```

**Answer:** `[FromBody]` expects JSON (typically `application/json`), but the client sends `URLSearchParams` (`application/x-www-form-urlencoded`) without a JSON serializer — the body does not deserialize into `ProductFilter`, leaving null/default properties.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding source | `[FromBody]` vs form-urlencoded body | Empty filter — grid shows all products or none |
| Content-Type | Missing `application/json` header | Input formatter skips JSON deserializer |
| MVC convention | Partial actions often use form POST | Mismatch with Web API-style `[FromBody]` habit |

**Fix (priority order):**

1. Remove `[FromBody]` and accept form binding: `Filter(ProductFilter filter)` with `URLSearchParams` or `FormData` body.
2. Or send JSON explicitly: `headers: { 'Content-Type': 'application/json' }`, `body: JSON.stringify({ category: 'books', minPrice: 10 })`.
3. On validation failure, return partial with empty state message — not silent empty grid.
4. Add `[ValidateAntiForgeryToken]` for POST filter if it mutates server state.

**Production takeaway:** "Model binding broken on AJAX" is often `[FromBody]` vs form encoding — see Model Binding module for source rules.

---

#### Q6. (R) 500 error page HTML injected into partial container.

**Answer:** The client treats every 200/500 response as partial HTML and assigns it with `innerHTML`; when `_comments.GetPage` throws, exception middleware returns the full `/Home/Error` view (layout + nav), which gets nested inside `#comments-panel`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Error handling | Global HTML exception handler for all requests | AJAX receives full page, not fragment |
| Client script | No `response.ok` check | 500 HTML injected into widget |
| UX | Broken layout inside panel | Users see duplicate chrome; JS hooks break |
| Observability | Exception swallowed in UI | Harder to detect from client alone |

**Fix (priority order):**

1. Client: check status — `if (!response.ok) { showToast('Could not load comments'); return; }`.
2. Server: detect AJAX — `if (Request.Headers.Accept.Contains("application/json"))` or custom header `X-Requested-With: XMLHttpRequest` / `HX-Request` (HTMX); return 500 JSON `{ error: "..." }` or 204 + problem body.
3. Use `IExceptionHandler` or filter that returns `ProblemDetails` for API/AJAX paths and HTML for full navigation.
4. Consider `[ResponseCache(NoStore = true)]` on partial actions so error pages are not cached.

```javascript
const r = await fetch(`/Comments/MoreComments?page=${page}`);
if (!r.ok) { showError(); return; }
document.getElementById('comments-panel').innerHTML = await r.text();
```

**Production takeaway:** Partial-update apps need **two error shapes** — HTML for full page, compact JSON or minimal fragment for AJAX.

---

#### Q7. (P) Stale partial badges behind CloudFront — cache headers.

**Answer:** `PartialView` responses without explicit cache policy inherit defaults that intermediaries may cache (especially if URLs are stable GET endpoints), serving stale stock HTML from CloudFront while the JSON API or DB already shows sold out.

- Set **`[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]`** on dynamic partial actions (`StockBadge`, cart summary).
- Alternatively **`Cache-Control: private, no-store`** via `Response.Headers` for user-specific fragments.
- Do **not** apply site-wide static-file cache rules to MVC partial routes — separate path patterns in CDN (`/_content/*` long cache vs `/Products/StockBadge/*` no cache).
- If caching is desired for anonymous fragments, use **`[ResponseCache(Duration = 60, VaryByQueryKeys = "sku")]`** plus short TTL and cache bust query when inventory changes.
- Add **`Vary: Cookie`** when output differs by auth — otherwise shared cache serves one user's cart to another.

**Production takeaway:** Partials are HTTP responses — CDN defaults treat GET HTML like static assets unless you forbid store.

---

#### Q8. (R) Duplicate DOM ids after loading multiple partials.

**Answer:** Each partial repeats `id="edit-row"` and `id="quick-edit-form"`; HTML requires unique ids document-wide. After injection, `getElementById` and the single delegated listener wired at page load target only the first match — other panels' buttons appear dead.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DOM | Duplicate `id` attributes | Invalid HTML; unpredictable `querySelector` results |
| Events | Listener bound once at load | Dynamically loaded forms never wired |
| Partials | Shared `_EditButton` without scoped ids | Each AJAX load duplicates same ids |

**Fix (priority order):**

1. Replace ids with classes inside partials: `.quick-edit-form`, `.edit-row-btn`.
2. Use **event delegation** on stable parent: `document.getElementById('panel-orders').addEventListener('click', e => { if (e.target.matches('.edit-row-btn')) ... })` — re-bind per panel container after each fetch.
3. Pass scoped suffix in ViewData: `id="edit-row-@Model.OrderId"` when id is required for labels.
4. After `innerHTML` update, call a small `initPanel(container)` to attach unobtrusive validation/AJAX parsers (see Q9).

**Production takeaway:** Partial updates multiply markup — ids must be unique or replaced with class + delegation.

---

#### Q9. (P) Unobtrusive AJAX — validation stops working after modal refresh.

**Answer:** `jquery.validate.unobtrusive` parses the DOM once at page load; when AJAX replaces the form HTML, new `data-val-*` attributes are present but validators are not re-parsed, so client-side validation silently stops until full reload.

- After injecting partial HTML, call **`$.validator.unobtrusive.parse('#modal-form-container')`** (or parse the new form element).
- If using **`jquery.unobtrusive-ajax`**, ensure `data-ajax-update` target is replaced and parse runs in the **`ajaxComplete`** / success callback before showing the modal again.
- Re-run **`$.unobtrusive.ajax.parse`** if custom `data-ajax-*` attributes were added dynamically.
- Server-side: still check `ModelState.IsValid` — client validation is UX only.
- When closing modal, **destroy** old validator instance if re-parsing same node id causes duplicate handlers — replace container `innerHTML` then parse once.

**Production takeaway:** Any partial swap that includes validated forms must re-parse unobtrusive adapters — same rule as Client-Side Validation chapter.

---

#### Q10. (D) Unobtrusive AJAX + jQuery vs HTMX for admin grid.

**Answer:** HTMX (or disciplined `fetch` + swap) reduces boilerplate by declaring behavior in HTML attributes and keeping rendering on the server; jQuery unobtrusive AJAX is mature on MVC but couples you to jQuery lifecycle and manual parse/rebind steps.

| Aspect | jQuery unobtrusive AJAX | HTMX / modern swap |
|---|---|---|
| Declarative triggers | `data-ajax="true"` on forms/links | `hx-post`, `hx-target`, `hx-swap` |
| Partial swap | Manual `data-ajax-update` + callbacks | Built-in swap strategies (`innerHTML`, `outerHTML`, `beforeend`) |
| Validation re-parse | Manual `unobtrusive.parse` | Same — still needed for jQuery validate unless server-only validation |
| Delete confirm | `data-ajax-confirm` | `hx-confirm` attribute |
| Dependencies | jQuery + Microsoft.UnobtrusiveAjax | HTMX script (~10kb), no jQuery required |
| Testing | Harder — jQuery DOM side effects | Easier — assert HTML fragments; optional `hx-headers` for antiforgery |
| When not HTMX | Heavy client state (drag-drop grid, WebSocket tiles) | Prefer JSON + component framework (React/Vue island) |

- **HTMX simplifies:** inline edit row swap, load-more, tab panels — server returns row partial, `hx-target="closest tr"` replaces one row.
- **Still need:** antiforgery via `hx-headers='js:{"RequestVerificationToken": getToken()}'`, unique targets, error handling (`hx-target="#error"` + 422 partial).
- **Avoid HTMX when:** offline-first SPA, complex client-only state, or non-HTML consumers — use API + front-end framework.

**Production takeaway:** HTMX is not a different backend — it is a cleaner contract for the same `PartialView` responses MVC already renders.

---

#### Q11. (M) AJAX POST with `[ValidateAntiForgeryToken]` — validation flow.

**Answer:** Antiforgery runs as a filter before the action: the server compares the cookie token with the form field `__RequestVerificationToken` or header `RequestVerificationToken`; mismatch or absence yields **400 Bad Request** (not 401) and the action never executes.

- **Token generation:** Rendered via `@Html.AntiForgeryToken()` or `<form asp-antiforgery="true">` — pairs cookie + hidden field.
- **AJAX POST:** Must send the same pair — typically header `RequestVerificationToken: <hidden field value>` plus cookie sent automatically on same-origin requests.
- **`[AutoValidateAntiforgeryToken]`** (controller/class): validates all unsafe HTTP methods (POST, PUT, PATCH, DELETE) automatically — preferred over per-action attributes in MVC apps.
- **`[IgnoreAntiforgeryToken]`:** opts one action out (e.g. webhook) — use sparingly; document threat model.
- **PartialView return:** After token passes, action runs normally; antiforgery does not affect response shape.
- **API + MVC mix:** JSON API controllers often use `[IgnoreAntiforgeryToken]` with bearer auth — do not copy that pattern to cookie-auth MVC partial endpoints.

**Production takeaway:** 400 on AJAX POST with empty body is often antiforgery — check token header before debugging business logic.

---

#### Q12. (R) Hybrid endpoint — wrong response shape for full page vs AJAX.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult CreateOrder(CreateOrderVm model)
{
    if (!ModelState.IsValid)
        return PartialView("_OrderForm", model);

    var order = _orders.Create(model);
    return PartialView("_OrderRow", order);
}
```

**Answer:** Full-page form posts expect **redirect-after-POST (PRG)** or at least a full view on validation failure; returning row partials to a navigating browser shows bare HTML or replaces the entire page context wrongly. AJAX and full-page share one action with no `Request.IsAjaxRequest()` branching, so double-submit from slow networks creates duplicate orders.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Response contract | Partial for both AJAX and full POST | Browser navigates to raw `<tr>` fragment or broken page |
| Validation UX | Invalid full-page post gets partial form only | No layout, CSS, or navigation |
| Idempotency | No redirect after success | Refresh resubmits POST — duplicate rows |
| Separation | Single action serves two clients | Hard to evolve JSON vs HTML independently |

**Fix (priority order):**

1. Split actions: `CreateOrder` (full page → `return View()` / `RedirectToAction`) and `CreateOrderPartial` (AJAX → `PartialView`).
2. Or branch on AJAX header: invalid → `PartialView` for AJAX, `View(model)` for full page; success → `PartialView("_OrderRow")` vs `RedirectToAction("Index")`.
3. Implement **PRG** for full-page: `RedirectToAction` after successful create; flash validation errors with `TempData` on failure redirect if needed.
4. AJAX: disable submit button during request; return 409 or idempotent token if duplicate submit is a business concern.

```csharp
if (!ModelState.IsValid)
    return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
        ? PartialView("_OrderForm", model)
        : View(model);

var order = _orders.Create(model);
return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
    ? PartialView("_OrderRow", order)
    : RedirectToAction(nameof(Index));
```

**Production takeaway:** One action cannot silently serve both navigation and fragment contracts — explicit detection or separate endpoints prevents duplicate submits and broken full-page UX.
