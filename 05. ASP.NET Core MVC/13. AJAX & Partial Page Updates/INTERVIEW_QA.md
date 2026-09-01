# AJAX & Partial Page Updates — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 13. AJAX & Partial Page Updates](#chapter-13-ajax-partial-page-updates)
  - [Q1. What is a partial page update in ASP.NET Core MVC?](#chapter-13-ajax-partial-page-updates-q1)
  - [Q2. What is `PartialView()` and what does it return?](#chapter-13-ajax-partial-page-updates-q2)
  - [Q3. What is the difference between returning `PartialView` and `…](#chapter-13-ajax-partial-page-updates-q3)
  - [Q4. How does model binding differ for AJAX POST with `FormData` …](#chapter-13-ajax-partial-page-updates-q4)
  - [Q5. How do you include an antiforgery token in a `fetch`/AJAX re…](#chapter-13-ajax-partial-page-updates-q5)
  - [Q6. What is `RequestVerificationToken` and how is it validated o…](#chapter-13-ajax-partial-page-updates-q6)
  - [Q7. What is the difference between `[ValidateAntiForgeryToken]` …](#chapter-13-ajax-partial-page-updates-q7)
  - [Q8. What is unobtrusive AJAX (`data-ajax="true"`)?](#chapter-13-ajax-partial-page-updates-q8)
  - [Q9. Why should AJAX partial endpoints check response status befo…](#chapter-13-ajax-partial-page-updates-q9)
  - [Q10. What `Cache-Control` headers should dynamic partial views us…](#chapter-13-ajax-partial-page-updates-q10)
  - [Q11. What XSS risks exist when injecting server-rendered HTML via…](#chapter-13-ajax-partial-page-updates-q11)
  - [Q12. Why do duplicate HTML `id` attributes break AJAX-loaded part…](#chapter-13-ajax-partial-page-updates-q12)
  - [Q13. What is the difference between `Html.PartialAsync` returned …](#chapter-13-ajax-partial-page-updates-q13)
  - [Q14. How do you handle validation errors in AJAX form submissions…](#chapter-13-ajax-partial-page-updates-q14)
  - [Q15. What happens when `UseExceptionHandler` returns a full error…](#chapter-13-ajax-partial-page-updates-q15)
  - [Q16. What is `[FromBody]` vs form-urlencoded binding for AJAX fil…](#chapter-13-ajax-partial-page-updates-q16)
  - [Q17. What is the difference between jQuery unobtrusive AJAX and `…](#chapter-13-ajax-partial-page-updates-q17)
  - [Q18. How do you design separate actions for full-page POST vs AJA…](#chapter-13-ajax-partial-page-updates-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 13. AJAX & Partial Page Updates

### Q1. What is a partial page update in ASP.NET Core MVC? {#chapter-13-ajax-partial-page-updates-q1}

What is a partial page update in ASP.NET Core MVC?

**Answer:** A partial page update refreshes only a fragment of the current page instead of performing a full browser navigation. The server returns a Razor partial view (HTML fragment) or JSON, and client-side JavaScript swaps that content into a target DOM container.

- Typical use cases include infinite scroll, inline edit panels, cart summaries, and filter grids without reloading the layout.
- The initial page still renders server-side for SEO and no-JavaScript fallback; AJAX enhances the same partial used on first load.
- ASP.NET Core 8 MVC supports this through `PartialView()`, unobtrusive AJAX, `fetch`, HTMX, or similar client swap patterns.
- Partial updates keep layout, navigation, and global scripts stable while only the widget region changes.

---

### Q2. What is `PartialView()` and what does it return? {#chapter-13-ajax-partial-page-updates-q2}

What is `PartialView()` and what does it return?

**Answer:** `PartialView()` is a controller helper that returns a `PartialViewResult` rendering a named Razor view without a layout. The HTTP response body is an HTML fragment suitable for injection into an existing page.

- Overloads accept a view name and optional model: `return PartialView("_CartSummary", model);`.
- Unlike `View()`, partial views do not apply `_ViewStart` layout by default — they render markup only.
- The result still runs through the MVC filter pipeline, model binding, and validation like any action result.
- Partial views should be strongly typed and live under `Views/Shared` or controller-specific `Views` folders.

---

### Q3. What is the difference between returning `PartialView` and `Json` from an AJAX action? {#chapter-13-ajax-partial-page-updates-q3}

What is the difference between returning `PartialView` and `Json` from an AJAX action?

**Answer:** `PartialView` returns server-rendered HTML that Razor encodes by default; `Json()` returns serialized data for the client to render. Choose based on who owns markup generation and whether you need shared partials across full-page and AJAX paths.

- `PartialView` reuses existing Razor partials, preserves encoding rules, and works well when markup is complex or must match server-rendered output.
- `Json` suits optimistic UI, mobile/API consumers, or rich client frameworks that build DOM from DTOs.
- HTML fragments tie the contract to DOM structure; JSON ties it to a stable DTO schema.
- Many MVC apps standardize on `PartialView` for list refresh and reserve JSON for non-HTML clients or lightweight state updates.

---

### Q4. How does model binding differ for AJAX POST with `FormData` vs JSON? {#chapter-13-ajax-partial-page-updates-q4}

How does model binding differ for AJAX POST with `FormData` vs JSON?

**Answer:** `FormData` and `application/x-www-form-urlencoded` bodies bind through the form value provider to action parameters and complex types without `[FromBody]`. JSON requires `Content-Type: application/json` and `[FromBody]` on a complex type for the JSON input formatter to deserialize.

- Simple parameters (`int id`) bind from form fields by name; JSON simple parameters do not bind unless wrapped in a model class with `[FromBody]`.
- `FormData` supports file uploads (`IFormFile`); JSON does not carry multipart files without a separate upload endpoint.
- Mixing `[FromBody]` on an action with a form-encoded AJAX POST leaves the model empty or default — a common silent bug.
- Match the client payload shape to the action signature explicitly in ASP.NET Core 8 MVC.

---

### Q5. How do you include an antiforgery token in a `fetch`/AJAX request? {#chapter-13-ajax-partial-page-updates-q5}

How do you include an antiforgery token in a `fetch`/AJAX request?

**Answer:** Read the hidden field `__RequestVerificationToken` rendered by the form tag helper or `@Html.AntiForgeryToken()` and send it as the `RequestVerificationToken` header or as a form field in the POST body. The antiforgery cookie is sent automatically on same-origin requests.

```javascript
const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
await fetch('/Cart/Add', {
    method: 'POST',
    headers: { 'RequestVerificationToken': token },
    body: new URLSearchParams({ productId: '42', quantity: '1' })
});
```

- For `FormData`, append `__RequestVerificationToken` alongside other fields.
- Cross-origin requests require CORS with credentials and cannot use wildcard origins when cookies are involved.
- Missing or mismatched tokens produce **400 Bad Request** before the action executes.
- `[AutoValidateAntiforgeryToken]` on the controller validates all unsafe HTTP methods automatically.

---

### Q6. What is `RequestVerificationToken` and how is it validated on AJAX POSTs? {#chapter-13-ajax-partial-page-updates-q6}

What is `RequestVerificationToken` and how is it validated on AJAX POSTs?

**Answer:** `RequestVerificationToken` is the HTTP header name ASP.NET Core antiforgery accepts as an alternative to the hidden form field `__RequestVerificationToken`. The antiforgery filter compares the submitted token with the token stored in the antiforgery cookie.

- Validation runs as a filter before the action method — mismatch or absence yields 400, not 401.
- The cookie-and-field pair prevents cross-site request forgery for cookie-authenticated MVC forms.
- Header name is case-insensitive; the value must match the rendered hidden field value.
- Bearer-authenticated API endpoints typically skip antiforgery; cookie-auth MVC partial endpoints must not.

---

### Q7. What is the difference between `[ValidateAntiForgeryToken]` and `[AutoValidateAntiforgeryToken]` for AJAX? {#chapter-13-ajax-partial-page-updates-q7}

What is the difference between `[ValidateAntiForgeryToken]` and `[AutoValidateAntiforgeryToken]` for AJAX?

**Answer:** `[ValidateAntiForgeryToken]` applies antiforgery validation to a single action. `[AutoValidateAntiforgeryToken]` applies it to all unsafe HTTP methods (POST, PUT, PATCH, DELETE) on the decorated controller or action scope.

- Both use the same token mechanism — cookie plus field or `RequestVerificationToken` header.
- `[AutoValidateAntiforgeryToken]` at the controller level is the preferred default for MVC apps with many POST actions.
- AJAX POSTs fail identically under either attribute when the token is missing.
- `[IgnoreAntiforgeryToken]` opts a specific action out — use only for webhooks or explicitly authenticated API endpoints.

---

### Q8. What is unobtrusive AJAX (`data-ajax="true"`)? {#chapter-13-ajax-partial-page-updates-q8}

What is unobtrusive AJAX (`data-ajax="true"`)?

**Answer:** Unobtrusive AJAX is a jQuery-based library (`jquery.unobtrusive-ajax`) that intercepts form and link submissions marked with `data-ajax="true"` and performs asynchronous requests, swapping the response into a target element without full page reload.

- Attributes such as `data-ajax-url`, `data-ajax-update`, `data-ajax-method`, and `data-ajax-confirm` declare behavior in HTML.
- The server still returns `PartialView` or other action results; the library handles the HTTP call and DOM update.
- It integrates with MVC form tag helpers and antiforgery tokens when forms are rendered server-side.
- After dynamic HTML injection, unobtrusive validation and AJAX parsers may need to be re-run on the new DOM subtree.

---

### Q9. Why should AJAX partial endpoints check response status before injecting HTML? {#chapter-13-ajax-partial-page-updates-q9}

Why should AJAX partial endpoints check response status before injecting HTML?

**Answer:** Without checking `response.ok` or the HTTP status code, error responses — including full error pages from exception middleware — get injected into the partial container via `innerHTML`, breaking layout and hiding failures from users.

- A 500 from `UseExceptionHandler` may return a full `/Home/Error` page with layout and navigation nested inside a widget.
- Client code should branch on status: show a toast or inline error for non-2xx responses instead of blindly assigning `response.text()`.
- Server-side, detect AJAX requests via `Accept` header, `X-Requested-With`, or a custom header and return compact JSON or minimal error fragments.
- Always validate success before parsing HTML or updating the DOM.

---

### Q10. What `Cache-Control` headers should dynamic partial views use? {#chapter-13-ajax-partial-page-updates-q10}

What `Cache-Control` headers should dynamic partial views use?

**Answer:** User-specific or frequently changing partials should use `Cache-Control: private, no-store` or `[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]` to prevent browsers and CDNs from serving stale HTML.

- Anonymous fragments that can be cached briefly may use short TTL with `VaryByQueryKeys` for cache keys tied to parameters.
- Add `Vary: Cookie` when output differs by authentication state — otherwise shared caches may serve one user's cart to another.
- Do not apply static-file CDN cache rules to MVC partial GET routes by default.
- Inventory badges, cart summaries, and personalized widgets are typical no-store candidates.

---

### Q11. What XSS risks exist when injecting server-rendered HTML via `innerHTML`? {#chapter-13-ajax-partial-page-updates-q11}

What XSS risks exist when injecting server-rendered HTML via `innerHTML`?

**Answer:** Assigning server HTML to `innerHTML` causes the browser to parse and execute any script or event handlers embedded in that markup. If the partial used `@Html.Raw` on user content or unsanitized data, attacker-controlled script runs in the victim's session.

- Razor's default `@` encoding is safe; `Html.Raw` on user-influenced strings bypasses that protection.
- AJAX partials are not safer than full pages — any injected HTML is an XSS surface.
- Prefer structured JSON plus `textContent` for user-generated text, or encode first and highlight second server-side.
- Content-Security-Policy is defense in depth, not a substitute for proper encoding.

---

### Q12. Why do duplicate HTML `id` attributes break AJAX-loaded partials? {#chapter-13-ajax-partial-page-updates-q12}

Why do duplicate HTML `id` attributes break AJAX-loaded partials?

**Answer:** HTML requires unique `id` values document-wide. Loading the same partial twice duplicates ids such as `edit-form`, so `getElementById`, label associations, and one-time event listeners target only the first match.

- Replace ids with classes inside partials and use event delegation on a stable parent container.
- When ids are required for accessibility, scope them with a suffix: `id="edit-row-@Model.Id"`.
- After each `innerHTML` swap, call an `initPanel(container)` to rebind unobtrusive validation and AJAX parsers.
- Duplicate ids produce invalid HTML and unpredictable JavaScript behavior across multiple AJAX-loaded panels.

---

### Q13. What is the difference between `Html.PartialAsync` returned from an action vs a full `View`? {#chapter-13-ajax-partial-page-updates-q13}

What is the difference between `Html.PartialAsync` returned from an action vs a full `View`?

**Answer:** `Html.PartialAsync` in a view synchronously renders a partial into the current page during server-side composition. `PartialView()` from a controller action returns a standalone HTTP response containing only the partial HTML for AJAX or direct requests.

- `PartialAsync` is for embedding fragments while building a full page response.
- `PartialView()` as an action result is the AJAX endpoint contract — no layout, fragment only.
- Both render the same `.cshtml` file but differ in HTTP context: inline composition vs separate request/response.
- AJAX clients call the action URL and inject the returned fragment; full pages call `PartialAsync` during initial render.

---

### Q14. How do you handle validation errors in AJAX form submissions? {#chapter-13-ajax-partial-page-updates-q14}

How do you handle validation errors in AJAX form submissions?

**Answer:** On the server, check `ModelState.IsValid` and return the form partial with validation messages when invalid, or return 400 with `ValidationProblemDetails` for API-style clients. On the client, replace the form container with the returned partial and re-parse unobtrusive validation.

- Return `PartialView("_Form", model)` with 200 or 400 depending on your client convention — consistency matters more than the specific status code.
- Ensure the partial includes `asp-validation-for` spans and optionally `asp-validation-summary`.
- After injecting the updated form, call `$.validator.unobtrusive.parse('#form-container')` to reattach client rules.
- Server-side validation remains mandatory; client validation is UX only.

---

### Q15. What happens when `UseExceptionHandler` returns a full error page to a partial AJAX request? {#chapter-13-ajax-partial-page-updates-q15}

What happens when `UseExceptionHandler` returns a full error page to a partial AJAX request?

**Answer:** The global exception handler renders the configured error view — often a full page with layout — and the AJAX client injects that entire page HTML into a widget container, producing nested navigation, broken styling, and no user-visible error message.

- The HTTP status may be 500 while the body looks like a normal HTML page.
- Partial-update apps need separate error handling for AJAX: return JSON `{ error: "..." }`, ProblemDetails, or a minimal error partial.
- Client scripts must check `response.ok` before assigning response text to `innerHTML`.
- Use `IExceptionHandler` or exception filters that branch on request type for dual error shapes.

---

### Q16. What is `[FromBody]` vs form-urlencoded binding for AJAX filter endpoints? {#chapter-13-ajax-partial-page-updates-q16}

What is `[FromBody]` vs form-urlencoded binding for AJAX filter endpoints?

**Answer:** `[FromBody]` expects JSON deserialized by the JSON input formatter. Form-urlencoded or `URLSearchParams` bodies bind through the form value provider when `[FromBody]` is omitted.

- Sending `URLSearchParams` to an action with `[FromBody] ProductFilter filter` leaves the model null or default — the grid shows wrong results silently.
- Fix by removing `[FromBody]` for form-style AJAX filters, or send JSON with `Content-Type: application/json` and `JSON.stringify`.
- MVC partial actions commonly use form encoding to mirror standard HTML form binding.
- Align Content-Type, binding source, and action signature explicitly.

---

### Q17. What is the difference between jQuery unobtrusive AJAX and `fetch` + `innerHTML`? {#chapter-13-ajax-partial-page-updates-q17}

What is the difference between jQuery unobtrusive AJAX and `fetch` + `innerHTML`?

**Answer:** jQuery unobtrusive AJAX declares behavior via `data-ajax-*` attributes and handles submission, antiforgery, and DOM swap automatically. `fetch` + `innerHTML` requires explicit token headers, status checks, error handling, and re-parsing of unobtrusive scripts after each swap.

- jQuery unobtrusive couples you to jQuery lifecycle and manual re-parse after dynamic updates.
- `fetch` is native, lighter, and pairs well with modern patterns or HTMX-style swaps.
- Both approaches typically receive the same `PartialView` HTML from the server.
- HTMX (`hx-post`, `hx-target`, `hx-swap`) offers declarative swaps without jQuery while keeping server-rendered partials.

---

### Q18. How do you design separate actions for full-page POST vs AJAX POST? {#chapter-13-ajax-partial-page-updates-q18}

How do you design separate actions for full-page POST vs AJAX POST?

**Answer:** Either split into distinct actions (`Create` for full page, `CreatePartial` for AJAX) or branch inside one action using an AJAX detection header such as `X-Requested-With: XMLHttpRequest` or `Accept: text/html` vs `application/json`.

- Full-page success should use Post-Redirect-Get (`RedirectToAction`) to prevent duplicate submission on refresh.
- Full-page validation failure returns `View(model)` with layout; AJAX failure returns `PartialView("_Form", model)`.
- AJAX success returns a row partial or updated widget; full-page success redirects to a list or details page.
- Sharing one action without branching causes browsers to navigate to bare HTML fragments or duplicate rows on refresh.

---

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

#### Gotcha 1. Business logic in Razor views

**Answer:** Placing pricing, discount, authorization, or business rules in `.cshtml` files bypasses unit tests, duplicates service-layer logic, and makes behavior hard to change consistently.

- Views should render data the controller or ViewModel already prepared.
- Authorization belongs in filters, policies, or controller/service checks before the view executes.
- Calculations in Razor cannot be tested independently and often diverge from API or batch logic.
- Keep Razor limited to presentation formatting — not business decisions.

---

#### Gotcha 2. EF entities passed directly to views

**Answer:** Binding and displaying EF Core entities exposes navigation properties, causes over-posting on POST, and couples the UI to the database schema.

- Lazy-loaded navigations can trigger unexpected queries during rendering.
- Mass assignment can update properties the user should not control (e.g., `IsAdmin`).
- Use dedicated ViewModels with only the fields the view needs.
- Map between entities and ViewModels in the controller or a mapping service.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Answer:** Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`, not JSON. `[FromBody]` uses the JSON input formatter and leaves the model empty while the action runs with default values.

- Remove `[FromBody]` for conventional form POSTs and let model binding read form fields.
- Use `[FromBody]` only when the client sends JSON with the correct Content-Type.
- Silent binding failure is a common source of "my POST action receives null model" bugs.
- AJAX forms using `FormData` follow the same form binding rules as full-page forms.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Answer:** Client-side validation is bypassable — attackers POST directly without browser scripts. Server-side validation is mandatory before any persist, redirect, or side effect.

- Always gate POST actions with `if (!ModelState.IsValid) return View(model);` or equivalent.
- Client validation improves UX for legitimate users only.
- Remote validation and unobtrusive rules are not security boundaries.
- Treat missing server validation as a security defect regardless of client script presence.

---

#### Gotcha 5. `return View()` after successful POST

**Answer:** Returning the same view after a successful POST causes duplicate submission when the user refreshes the page — the browser resubmits the POST body.

- Use Post-Redirect-Get: `return RedirectToAction(nameof(Index))` after successful create/update.
- PRG separates the mutation (POST) from the display (GET).
- Flash success messages via TempData on the redirect target.
- AJAX partial POSTs have a similar concern — disable submit during request or use idempotent server logic.

---

#### Gotcha 6. `ModelState` after redirect

**Answer:** `ModelState` is request-scoped and does not survive `RedirectToAction`. Validation errors are lost unless rehydrated through TempData, a second validation pass on GET, or by redisplaying the form without redirect on failure only.

- Common pattern: redirect only on success; on validation failure return `View(model)` with errors inline.
- To survive redirect on failure, serialize errors to TempData or use PRG with a form-specific error cache.
- Do not assume errors automatically follow the user after redirect.
- AJAX partial forms avoid redirect and can return the form partial with `ModelState` errors directly.

---

#### Gotcha 7. TempData read twice in layout and view

**Answer:** TempData is consumed on first read by default. If the layout reads a flash message, the view sees nothing unless you use `Peek()` or `Keep()`.

- Use `TempData.Peek("Message")` in the layout to read without consuming.
- Or call `TempData.Keep("Message")` after the layout read so the view can read it too.
- Prefer a single consumption point — typically the layout or a dedicated partial, not both.
- Cookie-based TempData has size limits; avoid storing large payloads.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Answer:** Controllers in `Areas/Admin/Controllers` without `[Area("Admin")]` are not discovered by the areas route and return 404 or match the wrong conventional route.

- Every area controller must declare `[Area("AreaName")]` matching its folder.
- Area routing is registered separately in `Program.cs` with the `{area:exists}` constraint.
- Without the attribute, MVC treats the controller as a root controller.
- Verify area registration order — specific area routes before catch-all default routes.

---

#### Gotcha 9. Link generation without `asp-area`

**Answer:** Tag Helpers default to the current area context when generating URLs. Links from a root view to an area controller need explicit `asp-area="Admin"` or they generate URLs without the area segment.

- From within an area, omitting `asp-area` keeps links inside the current area — sometimes incorrectly.
- Cross-area links require both `asp-area` and `asp-controller` (and `asp-action`).
- Wrong URLs produce 404 or hit unintended controllers.
- Same rule applies to `Url.Action` — pass `new { area = "Admin" }` in route values.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Answer:** A missing unchecked checkbox posts nothing and model binding sets a non-nullable `bool` to `false`. `[Required]` never fails because `false` is a valid value — not null or empty.

- Use `bool?` with `[Required]` to require an explicit true selection for consent checkboxes.
- Or use the hidden-field pattern: hidden input `false` plus checkbox `true` so unchecked still posts `false` deliberately.
- Server-side, verify explicit consent with a dedicated check rather than relying on `[Required]` alone.
- This applies to both full-page forms and AJAX form posts.

---

#### Gotcha 11. Collection binding with gap indices

**Answer:** Deleting a row from a dynamic form leaving indices such as `Lines[0]` and `Lines[2]` breaks model binder alignment — index 1 is missing and subsequent items may bind incorrectly or truncate.

- Reindex client-side after row deletion so indices are contiguous starting at zero.
- Or implement a custom `IModelBinder` that tolerates non-contiguous indices.
- Partial views rendering collection editors must maintain consistent index naming.
- Test add/delete row scenarios explicitly in complex form POSTs.

---

#### Gotcha 12. `@Html.Raw` with user content

**Answer:** Default Razor encoding prevents XSS by HTML-encoding output. `@Html.Raw(Model.UserComment)` renders attacker-supplied script if the content is not sanitized server-side.

- Encode first, then apply safe formatting — never wrap raw user input in HTML.
- AJAX-loaded partials injected via `innerHTML` execute injected script the same as full pages.
- Prefer `@Model.UserComment` (auto-encoded) or sanitize with a trusted HTML sanitizer library.
- Content-Security-Policy limits blast radius but does not replace encoding.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Answer:** Form tag helpers emit antiforgery tokens automatically, but `fetch` and jQuery AJAX must manually send `RequestVerificationToken` header or `__RequestVerificationToken` form field or POSTs fail with 400 antiforgery errors.

- Read the hidden field value from the page and include it on every mutating AJAX request.
- Same-origin requests send the antiforgery cookie automatically.
- `[AutoValidateAntiforgeryToken]` on the controller validates all unsafe methods — missing tokens fail before the action runs.
- Do not disable antiforgery on MVC cookie-auth endpoints to "fix" AJAX — add the token instead.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Answer:** Hubs are not registered in DI for direct injection into controllers. Use `IHubContext<THub>` to broadcast messages from controllers, services, or background jobs.

- Injecting a concrete `Hub` fails activation or produces an instance without connection context.
- `IHubContext<T>` is a singleton proxy registered by `AddSignalR()`.
- Pair with Redis backplane or Azure SignalR for multi-instance fan-out.
- Keep hubs thin; business logic stays in scoped or transient services.

---

#### Gotcha 15. SignalR scale-out without backplane

**Answer:** Sticky sessions alone do not fan-out events across server instances. Multi-node deployments need a Redis backplane or Azure SignalR Service so messages sent from any instance reach clients on all instances.

- Controller on instance A calling `IHubContext.Clients.User(id).SendAsync` misses users connected to instance B without a backplane.
- Sticky sessions route connections but do not route cross-instance messages.
- Group membership and connection IDs are local to each instance.
- Register `AddStackExchangeRedis` or `AddAzureSignalR` when scaling beyond a single node.

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

## Gotchas — ASP.NET Core MVC (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (D) A product page loads a comment list via AJAX. One teammate returns `PartialView("_Comments", comments)`; another returns `JsonResult(comments)` and builds HTML in JavaScript. The page also needs optimistic UI updates and SEO on the initial load. Which approach fits each concern, and what would you standardize on for this MVC app?

---

**Answer:**

_Answer not found._

---

#### Q2. (R) Review this AJAX POST action and client script. The form submits via `fetch` but the server always returns 400 with no useful body.

```csharp
[HttpPost]
public IActionResult AddToCart(int productId, int quantity)
{
    _cart.Add(productId, quantity);
    return PartialView("_CartSummary", _cart.GetSummary());
}
```

```javascript
fetch('/Cart/AddToCart', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ productId: 42, quantity: 1 })
});
```

`Program.cs` has global antiforgery enabled; the view has no token in the fetch call.

---

**Answer:**

_Answer not found._

---

#### Q3. (R) Review this partial-update script migrated from jQuery to native `fetch`. Add-to-cart works in Chrome locally but fails in production (cross-origin API subdomain) and antiforgery validation fails intermittently.

```javascript
// Before (worked):
// $.ajax({ url: '/Cart/Add', type: 'POST', data: $('#cartForm').serialize() });

fetch('/Cart/Add', {
    method: 'POST',
    body: new URLSearchParams(new FormData(document.getElementById('cartForm')))
});
```

No `credentials`, no `RequestVerificationToken` header, and the form lives on `www.example.com` while the action URL targets `api.example.com`.

---

**Answer:**

_Answer not found._

---

#### Q4. (R) Review this search autocomplete partial. QA reports stored XSS when a user searches for `<script>alert(1)</script>` and the suggestion list renders in the DOM.

```csharp
[HttpGet]
public IActionResult Suggest(string term)
{
    var hits = _search.Find(term);
    return PartialView("_Suggestions", hits);
}
```

```html
<!-- _Suggestions.cshtml -->
@foreach (var hit in Model)
{
    <li data-term="@hit.Term">@Html.Raw(hit.HighlightedHtml)</li>
}
```

`HighlightedHtml` is built server-side by wrapping the user's `term` in `<mark>` tags without encoding.

---

**Answer:**

_Answer not found._

---

#### Q5. (R) Review this AJAX filter endpoint. The grid partial never updates — `Model` is always empty defaults even though the network tab shows a JSON body.

```csharp
[HttpPost]
public IActionResult Filter([FromBody] ProductFilter filter)
{
    var rows = _catalog.Query(filter);
    return PartialView("_ProductGrid", rows);
}

public class ProductFilter
{
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
}
```

```javascript
const params = new URLSearchParams({ category: 'books', minPrice: '10' });
fetch('/Products/Filter', { method: 'POST', body: params });
```

---

**Answer:**

_Answer not found._

---

#### Q6. (R) Review this "load more comments" feature. When the database throws, users see the full site layout (nav, footer, error styling) injected inside `#comments-panel`.

```csharp
[HttpGet]
public IActionResult MoreComments(int page)
{
    var pageResult = _comments.GetPage(page); // throws SqlException under load
    return PartialView("_CommentList", pageResult);
}
```

```javascript
fetch(`/Comments/MoreComments?page=${page}`)
    .then(r => r.text())
    .then(html => document.getElementById('comments-panel').innerHTML = html);
```

No status check; `Program.cs` uses `UseExceptionHandler("/Home/Error")` for HTML error pages.

---

**Answer:**

_Answer not found._

---

#### Q7. (P) A `_StockBadge` partial is fetched via AJAX on every product hover. After deploy behind CloudFront, some users see stale "In stock" badges for sold-out items. The action returns `PartialView` with `Cache-Control` unset. What headers and action patterns fix this without disabling caching entirely for static assets?

---

**Answer:**

_Answer not found._

---

#### Q8. (R) Review this dashboard that loads three partials into placeholders. After refresh, "Edit" buttons only work on the first panel; duplicate-id warnings appear in dev tools.

```html
<div id="panel-orders" data-url="/Dashboard/OrdersPartial"></div>
<div id="panel-shipping" data-url="/Dashboard/ShippingPartial"></div>
```

```csharp
// _OrdersPartial.cshtml and _ShippingPartial.cshtml both contain:
<button id="edit-row" type="button">Edit</button>
<form id="quick-edit-form">...</form>
```

```javascript
document.querySelectorAll('[data-url]').forEach(el => {
    fetch(el.dataset.url).then(r => r.text()).then(html => el.innerHTML = html);
});
document.getElementById('quick-edit-form').addEventListener('submit', ...); // wired once on full page load
```

---

**Answer:**

_Answer not found._

---

#### Q9. (P) A team uses **unobtrusive AJAX** (`data-ajax="true"`) on a modal form with client-side validation (`jquery.validate.unobtrusive`). After a successful AJAX POST, the modal closes and reopens with a fresh empty form — but validation errors no longer appear until a full page reload. What causes this and how do you fix it in an MVC partial-update flow?

---

**Answer:**

_Answer not found._

---

#### Q10. (D) Compare **ASP.NET MVC unobtrusive AJAX + jQuery** vs **HTMX** (or `fetch` + `innerHTML`) for a CRUD admin grid with inline edit, delete confirm, and server-rendered row partials. What breaks or gets simpler when you switch, and when would you not choose HTMX?

---

**Answer:**

_Answer not found._

---

#### Q11. (M) Walk through what happens on an AJAX POST that returns `PartialView` when `[ValidateAntiForgeryToken]` is present: where the token is validated, what status code the client sees without a token, and how `[AutoValidateAntiforgeryToken]` on the controller differs from `[IgnoreAntiforgeryToken]` on one action.

---

**Answer:**

_Answer not found._

---

#### Q12. (R) Review this hybrid endpoint used by both a full-page form POST and an AJAX refresh. AJAX callers receive JSON; browser form posts should redirect — but both paths now return HTML fragments and double-submit creates duplicate rows.

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

Full-page form: `<form asp-action="CreateOrder" method="post">` with no `data-ajax`.  
AJAX: `fetch` posts `FormData` and replaces `#order-table tbody` with the response HTML.

**Answer:**

_Answer not found._

---
