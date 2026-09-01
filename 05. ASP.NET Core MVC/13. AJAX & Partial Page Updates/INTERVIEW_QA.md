# AJAX & Partial Page Updates — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is a partial page update in ASP.NET Core MVC?](#q1-what-is-a-partial-page-update-in-aspnet-core-mvc)
2. [Q2. What is `PartialView()` and what does it return?](#q2-what-is-partialview-and-what-does-it-return)
3. [Q3. What is the difference between returning `PartialView` and `Json` from an AJAX action?](#q3-what-is-the-difference-between-returning-partialview-and-json-from-an-ajax-action)
4. [Q4. How does model binding differ for AJAX POST with `FormData` vs JSON?](#q4-how-does-model-binding-differ-for-ajax-post-with-formdata-vs-json)
5. [Q5. How do you include an antiforgery token in a `fetch`/AJAX request?](#q5-how-do-you-include-an-antiforgery-token-in-a-fetchajax-request)
6. [Q6. What is `RequestVerificationToken` and how is it validated on AJAX POSTs?](#q6-what-is-requestverificationtoken-and-how-is-it-validated-on-ajax-posts)
7. [Q7. What is the difference between `[ValidateAntiForgeryToken]` and `[AutoValidateAntiforgeryToken]` for AJAX?](#q7-what-is-the-difference-between-validateantiforgerytoken-and-autovalidateantiforgerytoken-for-ajax)
8. [Q8. What is unobtrusive AJAX (`data-ajax="true"`)?](#q8-what-is-unobtrusive-ajax-data-ajaxtrue)
9. [Q9. Why should AJAX partial endpoints check response status before injecting HTML?](#q9-why-should-ajax-partial-endpoints-check-response-status-before-injecting-html)
10. [Q10. What `Cache-Control` headers should dynamic partial views use?](#q10-what-cache-control-headers-should-dynamic-partial-views-use)
11. [Q11. What XSS risks exist when injecting server-rendered HTML via `innerHTML`?](#q11-what-xss-risks-exist-when-injecting-server-rendered-html-via-innerhtml)
12. [Q12. Why do duplicate HTML `id` attributes break AJAX-loaded partials?](#q12-why-do-duplicate-html-id-attributes-break-ajax-loaded-partials)
13. [Q13. What is the difference between `Html.PartialAsync` returned from an action vs a full `View`?](#q13-what-is-the-difference-between-htmlpartialasync-returned-from-an-action-vs-a-full-view)
14. [Q14. How do you handle validation errors in AJAX form submissions?](#q14-how-do-you-handle-validation-errors-in-ajax-form-submissions)
15. [Q15. What happens when `UseExceptionHandler` returns a full error page to a partial AJAX request?](#q15-what-happens-when-useexceptionhandler-returns-a-full-error-page-to-a-partial-ajax-request)
16. [Q16. What is `[FromBody]` vs form-urlencoded binding for AJAX filter endpoints?](#q16-what-is-frombody-vs-form-urlencoded-binding-for-ajax-filter-endpoints)
17. [Q17. What is the difference between jQuery unobtrusive AJAX and `fetch` + `innerHTML`?](#q17-what-is-the-difference-between-jquery-unobtrusive-ajax-and-fetch-innerhtml)
18. [Q18. How do you design separate actions for full-page POST vs AJAX POST?](#q18-how-do-you-design-separate-actions-for-full-page-post-vs-ajax-post)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is a partial page update in ASP.NET Core MVC?

**Concepts**
- Partial page update — server returns HTML fragment or JSON, JavaScript swaps a DOM container
- Initial server-side render for SEO and no-JavaScript fallback
- `PartialView()`, unobtrusive AJAX, `fetch`, and HTMX as delivery mechanisms
- Layout, navigation, and global scripts remain stable across partial swaps

**Answer**

A partial page update refreshes only a fragment of the current page instead of performing a full browser navigation — the server returns a Razor partial view (HTML fragment) or JSON, and client-side JavaScript replaces a target DOM container with that content. The initial page still renders server-side for SEO and no-JavaScript fallback; AJAX enhances the same partial used on first load. ASP.NET Core MVC supports this through `PartialView()`, unobtrusive AJAX (`jquery.unobtrusive-ajax`), native `fetch`, or HTMX. Layout, navigation, and global scripts remain in place while only the widget region updates, since the browser does not navigate and the outer HTML structure does not reload.

---

## Q2. What is `PartialView()` and what does it return?

**Concepts**
- `PartialViewResult` — HTML fragment rendered without `_ViewStart` layout
- `return PartialView("_CartSummary", model)` overloads with view name and optional model
- MVC filter pipeline still running for partial view actions
- Partial view location convention — `Views/Shared` or controller-specific `Views` folders

**Answer**

`PartialView()` is a controller helper that returns a `PartialViewResult` which renders a named Razor view without applying `_ViewStart` or a layout — the HTTP response body is an HTML fragment suitable for injection into an existing page. Overloads accept a view name and optional model: `return PartialView("_CartSummary", model)`. Unlike `View()`, the rendered output has no layout wrapper, so it is a fragment of markup rather than a complete page. The result still runs through the MVC filter pipeline — model binding, action filters, and result filters execute normally. Partial views live under `Views/Shared/` for cross-controller partials or the controller-specific `Views/<Controller>/` folder.

---

## Q3. What is the difference between returning `PartialView` and `Json` from an AJAX action?

**Concepts**
- `PartialView` — server-rendered HTML with Razor encoding; reuses existing view logic
- `Json` — serialized DTO for client-side rendering; stable schema contract
- HTML fragment coupling to DOM structure vs JSON coupling to DTO shape
- `PartialView` preferred for shared partials used on both initial load and AJAX updates

**Answer**

`PartialView` returns server-rendered HTML that Razor encodes by default, reusing existing partial logic and encoding rules; `Json()` returns serialized data for the client to render using its own DOM manipulation. The key trade-off is who owns markup generation — `PartialView` keeps rendering server-side and works well when the same partial is used on initial load and AJAX refresh. `Json` suits optimistic UI updates, mobile API consumers, or rich client frameworks that construct DOM from DTOs. HTML fragments tie the contract to DOM structure and Razor syntax; JSON ties it to a stable DTO schema that can be consumed by multiple client types. Many MVC apps standardize on `PartialView` for list and widget refresh, reserving `Json` for non-HTML clients or lightweight state change signals.

---

## Q4. How does model binding differ for AJAX POST with `FormData` vs JSON?

**Concepts**
- `FormData` and `application/x-www-form-urlencoded` binding through the form value provider
- `[FromBody]` routing to the JSON input formatter — requires `Content-Type: application/json`
- Silent empty-model bug when `[FromBody]` receives form-encoded data
- `IFormFile` supported by `FormData` only, not JSON bodies

**Answer**

`FormData` and `application/x-www-form-urlencoded` bodies bind through the form value provider to action parameters and complex types without requiring `[FromBody]` — simple parameters bind by name, and nested types bind from dot-notation keys. JSON requires `Content-Type: application/json` and `[FromBody]` on a complex type for the JSON input formatter to deserialize. Mixing `[FromBody]` on an action with a form-encoded AJAX POST is a common silent failure: the JSON formatter finds no matching content and the model receives default values while the action runs normally. `FormData` also supports `IFormFile` for multipart uploads; JSON bodies cannot carry file content without a separate upload mechanism. The client payload shape and the action signature must be aligned explicitly.

---

## Q5. How do you include an antiforgery token in a `fetch`/AJAX request?

**Concepts**
- `__RequestVerificationToken` hidden field emitted by form tag helpers and `@Html.AntiForgeryToken()`
- `RequestVerificationToken` HTTP request header as the AJAX alternative to the hidden field
- Antiforgery cookie sent automatically on same-origin requests
- Missing or mismatched token yielding 400 before the action executes

**Answer**

Read the hidden field `__RequestVerificationToken` rendered by the form tag helper or `@Html.AntiForgeryToken()` and send its value as the `RequestVerificationToken` header on the `fetch` call — the antiforgery cookie is sent automatically by the browser on same-origin requests, so the server can validate the cookie-and-token pair:

```javascript
const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
await fetch('/Cart/Add', {
    method: 'POST',
    headers: { 'RequestVerificationToken': token },
    body: new URLSearchParams({ productId: '42', quantity: '1' })
});
```

For `FormData`, append `__RequestVerificationToken` alongside other fields instead of using a header. Cross-origin requests require CORS with credentials and cannot use wildcard origins. A missing or mismatched token produces 400 Bad Request before the action method executes.

---

## Q6. What is `RequestVerificationToken` and how is it validated on AJAX POSTs?

**Concepts**
- `RequestVerificationToken` HTTP header as the alternative to the hidden form field
- Antiforgery filter comparing the submitted token against the antiforgery cookie value
- 400 status on mismatch — not 401, since it is an authorization filter rejecting before the action
- Cookie-authenticated MVC endpoints requiring antiforgery; bearer-token API endpoints typically skipping it

**Answer**

`RequestVerificationToken` is the HTTP request header name that ASP.NET Core antiforgery accepts as an alternative to the `__RequestVerificationToken` hidden form field. The antiforgery authorization filter compares the submitted token (from the header or form field) with the token embedded in the antiforgery cookie — both must match for the request to proceed. Validation runs before the action method, so a mismatch or absent token yields 400 Bad Request — not 401 — because it is handled in the authorization filter stage. The header name is case-insensitive. Bearer-authenticated API endpoints typically do not use antiforgery since CSRF is only relevant for cookie-based browser sessions; cookie-auth MVC partial endpoints must not skip it.

---

## Q7. What is the difference between `[ValidateAntiForgeryToken]` and `[AutoValidateAntiforgeryToken]` for AJAX?

**Concepts**
- `[ValidateAntiForgeryToken]` applied to a single action
- `[AutoValidateAntiforgeryToken]` applied to all unsafe methods on the controller
- Same token mechanism — cookie plus field or `RequestVerificationToken` header
- `[IgnoreAntiforgeryToken]` opting one action out of a controller-level `AutoValidate`

**Answer**

`[ValidateAntiForgeryToken]` applies antiforgery validation to a single decorated action, while `[AutoValidateAntiforgeryToken]` applies it to all unsafe HTTP methods (POST, PUT, PATCH, DELETE) on the decorated controller or globally. Both use the same mechanism — cookie plus the `RequestVerificationToken` header or `__RequestVerificationToken` form field. `[AutoValidateAntiforgeryToken]` at the controller level is the preferred default for MVC apps with many POST actions, because it avoids the risk of forgetting the attribute on new actions. AJAX POSTs fail identically under either attribute when the token is missing. `[IgnoreAntiforgeryToken]` opts a specific action out — use only for webhooks, HMAC-authenticated callbacks, or explicitly bearer-token-authenticated API endpoints.

---

## Q8. What is unobtrusive AJAX (`data-ajax="true"`)?

**Concepts**
- `jquery.unobtrusive-ajax` intercepting form and link submissions via `data-ajax="true"` attributes
- Declarative behavior via `data-ajax-url`, `data-ajax-update`, `data-ajax-method` attributes
- Server returning `PartialView` or other action results — library handles HTTP call and DOM swap
- Re-parsing unobtrusive validation and AJAX parsers required after dynamic HTML injection

**Answer**

Unobtrusive AJAX is a jQuery-based library (`jquery.unobtrusive-ajax`) that intercepts form and link submissions marked with `data-ajax="true"` and performs asynchronous requests, swapping the response into a target element without a full page reload. Behavior is declared via HTML attributes: `data-ajax-url`, `data-ajax-update`, `data-ajax-method`, `data-ajax-confirm`. The server still returns a `PartialView` or other action result; the library handles the HTTP call, checks success callbacks, and swaps the response into the specified DOM container. It integrates with MVC form tag helpers and antiforgery tokens when forms are rendered server-side. After dynamic HTML injection, unobtrusive validation and AJAX parsers (`$.validator.unobtrusive.parse`, `$.fn.ajaxForm`) must be re-invoked on the new DOM subtree to attach client validation and event handlers.

---

## Q9. Why should AJAX partial endpoints check response status before injecting HTML?

**Concepts**
- Error responses from exception middleware returning full HTML page with layout
- `response.ok` or HTTP status check before assigning `response.text()` to `innerHTML`
- Full error page injected into widget container — nested navigation, broken styling
- Server-side AJAX detection for compact error shapes vs full error pages

**Answer**

Without checking `response.ok` or the HTTP status code, error responses get injected into the partial container via `innerHTML` — a 500 from `UseExceptionHandler` may return a full `/Home/Error` page complete with layout, navigation, and stylesheets, producing nested navigation inside a widget and hiding the failure from users. The status code may be 500 while the body appears as normal HTML, so naive status-code-ignoring code injects the error page silently. Client code should branch on status: `if (!response.ok) { showErrorToast(await response.text()); return; }`. Server-side, detect AJAX requests via the `Accept` header or a custom `X-Requested-With` header and return compact JSON `ProblemDetails` or a minimal error partial rather than the full HTML error page.

---

## Q10. What `Cache-Control` headers should dynamic partial views use?

**Concepts**
- `Cache-Control: private, no-store` for user-specific or frequently changing partials
- `[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]` on action
- `Vary: Cookie` when output differs by authentication state
- Short TTL with `VaryByQueryKeys` for anonymous cacheable fragments

**Answer**

User-specific or frequently changing partials — cart summaries, inventory badges, personalized widgets — should use `Cache-Control: private, no-store` to prevent browsers and CDNs from serving stale HTML. The equivalent MVC attribute is `[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]`. Without `no-store`, CDNs may cache the first user's badge and serve it to all subsequent users. Add `Vary: Cookie` when output differs by authentication state — without it, a shared cache may serve one user's partial to an unauthenticated visitor. Anonymous fragments that change rarely can use a short TTL with `VaryByQueryKeys` to scope the cache entry to specific query parameters.

---

## Q11. What XSS risks exist when injecting server-rendered HTML via `innerHTML`?

**Concepts**
- `innerHTML` causing the browser to parse and execute embedded scripts and event handlers
- `@Html.Raw` bypassing Razor's default encoding on user-influenced strings
- Server-rendered partials as an XSS surface when user content is not encoded
- Content-Security-Policy as defense in depth, not a substitute for encoding

**Answer**

Assigning server HTML to `innerHTML` causes the browser to parse and execute any script elements or event handlers embedded in that markup. If the partial used `@Html.Raw` on user-controlled content — search highlights, comments, product names — attacker-supplied `<script>` tags or `onerror` handlers run in the victim's session. Razor's default `@` encoding is safe; `Html.Raw` on user-influenced strings bypasses that protection. AJAX partials are not inherently safer than full pages — any HTML injected via `innerHTML` is an XSS surface identical to a full-page render. Use `@Model.Comment` for auto-encoded output or sanitize with a trusted HTML sanitizer library before passing content to `Html.Raw`. CSP is defense in depth but does not replace correct encoding.

---

## Q12. Why do duplicate HTML `id` attributes break AJAX-loaded partials?

**Concepts**
- HTML requiring unique `id` values document-wide
- `getElementById` returning only the first match — later panels' elements unreachable
- Event delegation on a stable parent container as the fix for per-panel event handlers
- Model-scoped id suffix — `id="edit-row-@Model.Id"` — for accessibility-required ids

**Answer**

HTML requires unique `id` values across the entire document. Loading the same partial into two placeholders duplicates ids such as `edit-form` — `getElementById` returns only the first match, label associations bind to the wrong element, and one-time event listeners wired at page load never fire on the second panel. The fix for interactive elements is to replace ids with class attributes inside partials and use event delegation on a stable parent container so events bubble up regardless of how many panels are loaded. When ids are required for accessibility (label/input association, ARIA), scope them with a model-derived suffix: `id="edit-row-@Model.Id"`. After each `innerHTML` swap, call an initialization function on the new container to rebind unobtrusive validation and event handlers.

---

## Q13. What is the difference between `Html.PartialAsync` returned from an action vs a full `View`?

**Concepts**
- `Html.PartialAsync` — inline embedding during server-side page composition, no separate request
- `PartialView()` as an action result — standalone HTTP response for AJAX clients
- Both rendering the same `.cshtml` file but in different HTTP contexts
- `PartialAsync` for initial render; `PartialView()` action for AJAX fragment endpoint

**Answer**

`Html.PartialAsync("_Comments", model)` in a Razor view renders the partial inline during server-side page composition — no separate HTTP request is issued, the partial's output is embedded in the parent page's response. `PartialView()` from a controller action returns a standalone HTTP response containing only the partial HTML, designed for AJAX clients to inject into a DOM container. Both render the same `.cshtml` file but differ in HTTP context: `PartialAsync` is a compile-time composition step during initial full-page render, while `PartialView()` is an AJAX endpoint contract — no layout, fragment only, callable by `fetch` or jQuery. The typical pattern is to use both: `PartialAsync` for initial load and the corresponding action for AJAX refresh of the same fragment.

---

## Q14. How do you handle validation errors in AJAX form submissions?

**Concepts**
- `ModelState.IsValid` check on the server — mandatory regardless of client validation
- Returning form partial with validation messages on failure for HTML-returning actions
- `ValidationProblemDetails` with 400 status for API-style clients
- Re-parsing unobtrusive validation after replacing the form container with the partial

**Answer**

On the server, check `ModelState.IsValid` and return the form partial with validation messages when invalid — `return PartialView("_Form", model)` preserves the bound model so `asp-validation-for` spans render errors. For API-style AJAX clients that expect JSON, return `ValidationProblem()` with 400. On the client, replace the form container with the returned partial and re-invoke `$.validator.unobtrusive.parse('#form-container')` to reattach client validation rules to the newly injected form elements. Server-side validation is mandatory — client validation only improves UX for legitimate users. Using consistent status codes (200 for valid, 400 for invalid, or always 200 and branching in JavaScript) matters less than consistency within the app so clients can reliably distinguish success from failure.

---

## Q15. What happens when `UseExceptionHandler` returns a full error page to a partial AJAX request?

**Concepts**
- `UseExceptionHandler` rendering the configured error view — full page with layout
- AJAX client injecting full page HTML into a widget container via `innerHTML`
- Nested navigation and broken styling from layout-in-widget injection
- `IExceptionHandler` or exception filter branching on AJAX request type for compact error shapes

**Answer**

The global exception handler renders the configured error view — often a full page with `_Layout.cshtml` including navigation, footer, and styles — and returns it as the response body. The AJAX client, without a status check, injects that entire page into a widget container via `innerHTML`, producing nested navigation bars, broken CSS, and no visible error message meaningful to the user. The HTTP status may be 500 while the body resembles a normal page. The fix has two parts: client-side, check `response.ok` before assigning response text to `innerHTML`; server-side, detect AJAX requests via the `Accept` or `X-Requested-With` header and return compact JSON `ProblemDetails` or a minimal error partial — either via an `IExceptionHandler` implementation or an exception filter that branches on request type.

---

## Q16. What is `[FromBody]` vs form-urlencoded binding for AJAX filter endpoints?

**Concepts**
- `[FromBody]` requiring JSON input formatter and `Content-Type: application/json`
- Form-urlencoded body binding through form value provider without `[FromBody]`
- `URLSearchParams` to a `[FromBody]` action — model receives null or defaults silently
- Align `Content-Type`, binding attribute, and action signature explicitly

**Answer**

`[FromBody]` on a complex parameter tells MVC to use the JSON input formatter — it expects `Content-Type: application/json` and a JSON body. Sending a `URLSearchParams` body (form-urlencoded) to an action with `[FromBody] ProductFilter filter` results in the JSON formatter finding no matching content, leaving the filter at default values — the grid shows wrong results silently without any error. The fix is either to remove `[FromBody]` and let the form value provider bind the fields, or to send JSON with `Content-Type: application/json` and `JSON.stringify`. MVC partial actions commonly use form encoding to mirror standard HTML form binding and avoid the `[FromBody]` requirement. The Content-Type, binding source attribute, and action parameter signature must be aligned explicitly.

---

## Q17. What is the difference between jQuery unobtrusive AJAX and `fetch` + `innerHTML`?

**Concepts**
- `jquery.unobtrusive-ajax` — declarative `data-ajax-*` attributes, automatic antiforgery and DOM swap
- `fetch` — explicit token header, status check, error handling, and unobtrusive re-parse after each swap
- HTMX as a declarative `fetch`-based alternative without jQuery coupling
- Both approaches receiving the same `PartialView` HTML from the server

**Answer**

jQuery unobtrusive AJAX declares behavior via `data-ajax-*` HTML attributes — the library intercepts form submission, sends the request with the antiforgery cookie, and swaps the response into `data-ajax-update` automatically. `fetch` + `innerHTML` requires explicitly reading the token, including it in headers, checking `response.ok`, handling errors, and re-parsing unobtrusive validation scripts after each DOM swap. jQuery unobtrusive couples the front-end to jQuery and manual re-parse hooks after dynamic updates. `fetch` is native, lighter, and pairs well with HTMX-style declarative swaps (`hx-post`, `hx-target`, `hx-swap`) which offer attribute-driven behavior without jQuery while keeping server-rendered partials. Both approaches typically receive the same `PartialView` HTML from the server — the difference is entirely in how the client manages the HTTP call, error handling, and DOM update.

---

## Q18. How do you design separate actions for full-page POST vs AJAX POST?

**Concepts**
- Full-page POST success requiring PRG redirect — browser last request must be GET
- AJAX success returning partial fragment, not a redirect response
- Branching on `Accept` header or `X-Requested-With` vs splitting into distinct action methods
- Full-page validation failure returning `View(model)` with layout; AJAX returning `PartialView`

**Answer**

The cleanest design is separate actions — `Create` for full-page form submission and `CreateAjax` (or `CreatePartial`) for AJAX — because the expected responses differ fundamentally: full-page success redirects (PRG), AJAX success returns a partial fragment or JSON. A single action branching on `Request.Headers["X-Requested-With"] == "XMLHttpRequest"` or `Accept` header works but mixes two contracts in one method and is harder to test and document. Full-page validation failure returns `View(model)` with layout intact; AJAX validation failure returns `PartialView("_Form", model)` as an HTML fragment. Full-page success uses `return RedirectToAction(...)` so browser refresh repeats a GET; AJAX success returns the new row or widget partial. Sharing one action without branching causes browsers to navigate to bare HTML fragments or re-submit on refresh.

---

## Gotchas — ASP.NET Core MVC (Interview Traps)

---

#### Gotcha 1. Business logic in Razor views

**Concepts**
- Business logic in Razor — untestable and duplicated from the service layer
- Separation of concerns — view as presentation only
- Authorization checks in templates bypassing security layers
- Divergent behavior when view and API/batch logic run the same rule separately

**Answer**

Placing pricing, discount, authorization, or business rules in `.cshtml` files bypasses unit tests, duplicates service-layer logic, and makes behavior hard to change consistently. Views should render only what the controller or ViewModel already prepared, since Razor calculations cannot be tested independently and often diverge from API or batch logic. Authorization belongs in filters, policies, or controller checks executed before the view — not in view conditionals that a developer can accidentally omit.

---

#### Gotcha 2. EF entities passed directly to views

**Concepts**
- Over-posting via direct entity binding on POST
- Lazy-loaded navigation properties triggering unexpected queries during rendering
- ViewModel as the narrow data contract between controller and view
- Entity-to-ViewModel mapping responsibility

**Answer**

Binding and displaying EF Core entities exposes navigation properties, enables over-posting on POST, and couples the UI to the database schema. Lazy-loaded navigations can trigger unexpected queries during Razor rendering — each navigation access issues a database round-trip. Mass assignment on POST can update properties the user should never control, such as `IsAdmin`. The correct pattern is a dedicated ViewModel with only the fields the view needs, mapped from the entity in the controller or a mapping service.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Concepts**
- Browser form encoding — `application/x-www-form-urlencoded` vs JSON
- `[FromBody]` routing to the JSON input formatter only
- Silent binding failure — model parameter receives default values
- `FormData` following the form value provider rules

**Answer**

Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`, not JSON. `[FromBody]` tells MVC to use the JSON input formatter — when a form POST arrives, the formatter finds no matching content and the model receives default values while the action runs silently. Remove `[FromBody]` for conventional form POSTs and let the form value provider bind fields. Use `[FromBody]` only when the client explicitly sends JSON with the correct Content-Type header.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Concepts**
- Client validation as a UX convenience, not a security boundary
- Server-side validation mandatory before any persist, redirect, or side effect
- Direct POST attacks bypassing browser scripts entirely

**Answer**

Client-side validation is bypassable — attackers can POST directly without running browser scripts. Server-side `ModelState.IsValid` is mandatory before any persist, redirect, or side effect. Always gate POST actions with `if (!ModelState.IsValid) return View(model);` or equivalent. Treating missing server validation as a security defect regardless of client script presence is the right standard.

---

#### Gotcha 5. `return View()` after successful POST

**Concepts**
- Duplicate form submission triggered by browser refresh after POST
- Post-Redirect-Get (PRG) pattern — mutation then safe redirect
- `RedirectToAction` separating command (POST) from display (GET)
- TempData carrying flash messages across the redirect

**Answer**

Returning the same view after a successful POST means the browser's last request was the POST. When the user refreshes, the browser re-submits the POST body, which can duplicate an order or registration. The fix is Post-Redirect-Get: return `RedirectToAction(nameof(Index))` after a successful mutation so the browser's last request is a safe GET. TempData carries flash success messages across the redirect.

---

#### Gotcha 6. `ModelState` after redirect

**Concepts**
- `ModelState` as request-scoped data lost on redirect
- Return `View(model)` on validation failure to preserve errors inline
- TempData serialization as a fallback for post-redirect error persistence
- AJAX partial forms avoiding the redirect problem entirely

**Answer**

`ModelState` lives in the controller's `ViewDataDictionary` for the current request only — a redirect ends that request with an empty `ModelState`. The standard pattern is redirect only on success and return `View(model)` on validation failure. If a redirect on failure is truly required, serialize errors to TempData or run a second validation pass on the GET action.

---

#### Gotcha 7. TempData read twice in layout and view

**Concepts**
- TempData consume-on-read default semantics
- Layout consuming flash key before the child view reads it
- `Peek()` — read without marking for deletion in the same request
- `Keep()` — preserve a consumed key for the next request

**Answer**

TempData marks entries for deletion the moment they are read via the indexer. If the layout reads a flash message first, the child view returns null. The fix is to use `TempData.Peek("Message")` in the layout, which reads without consuming. Centralizing flash display in a single partial avoids the double-read problem entirely.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Concepts**
- `[Area("AreaName")]` as required routing metadata on area controllers
- Controller in `Areas/` folder without attribute treated as a root controller
- `{area:exists}` constraint not matching unannotated controllers
- Compile-time success masking a runtime 404

**Answer**

A controller physically in `Areas/Admin/Controllers/` is not automatically registered with the area route — it needs `[Area("Admin")]` on the class. Without it, MVC treats it as a root controller and requests return 404. The project compiles without the attribute, giving false confidence until the first HTTP request hits the area URL.

---

#### Gotcha 9. Link generation without `asp-area`

**Concepts**
- Ambient area route values from the current request
- Absent area context in root views producing wrong URLs
- Explicit `asp-area` required for cross-area and root-to-area links
- `Url.Action` requiring area route values in the anonymous object

**Answer**

Tag Helpers inherit ambient route values from the current request. From a root view, `asp-controller="Users"` generates `/Users` with no area prefix. Cross-area links require explicit `asp-area="Admin"` on every anchor targeting an area controller. The same rule applies to `Url.Action` — pass `new { area = "Admin" }` in the route values object.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Concepts**
- Unchecked checkbox posting no value — binding sets non-nullable `bool` to `false`
- `[Required]` passing validation because `false` is a valid non-null value
- `bool?` with `[Required]` requiring explicit `true` for consent scenarios
- Hidden-field pattern for deliberate `false` submission

**Answer**

An unchecked checkbox posts nothing, so model binding sets a non-nullable `bool` to `false`. `[Required]` passes because `false` is non-null. For explicit consent, use `bool?` with `[Required]` — null (no field posted) fails `[Required]`. The hidden-field pattern ensures the form always posts a value.

---

#### Gotcha 11. Collection binding with gap indices

**Concepts**
- Contiguous-index requirement for MVC form collection binding
- Gap indices causing silent truncation or misalignment
- Client-side reindexing after row deletion
- Custom `IModelBinder` for non-contiguous index tolerance

**Answer**

MVC's collection binder expects contiguous indices starting at zero. Gap indices cause the binder to stop so subsequent items are silently dropped. The fix is to reindex client-side after every row deletion. A custom `IModelBinder` can tolerate non-contiguous indices for complex scenarios.

---

#### Gotcha 12. `@Html.Raw` with user content

**Concepts**
- Razor default `@` encoding preventing XSS
- `Html.Raw` bypassing encoding for attacker-supplied strings
- AJAX partial HTML injection via `innerHTML` as an XSS surface
- Content-Security-Policy as defense in depth, not a substitute

**Answer**

Razor's default `@` encoding prevents XSS. `@Html.Raw(Model.UserComment)` bypasses that protection, rendering `<script>` tags and event handlers. AJAX-loaded partials injected via `innerHTML` carry the same risk. Use `@Model.UserComment` for auto-encoded output, or sanitize with a trusted HTML sanitizer library.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Concepts**
- Antiforgery cookie-and-field/header pair preventing CSRF
- Form Tag Helpers emitting the hidden token field automatically
- Manual `RequestVerificationToken` header required for `fetch` and jQuery AJAX
- `[AutoValidateAntiforgeryToken]` covering all unsafe methods on a controller

**Answer**

Form Tag Helpers emit the token automatically, but `fetch` and jQuery AJAX must include it manually as the `RequestVerificationToken` header or form field. Without it, antiforgery validation returns 400 before the action executes. Disabling antiforgery on MVC cookie-auth endpoints to work around the 400 is not acceptable.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Concepts**
- Hub — per-connection transient lifecycle, not registered in DI for direct injection
- `IHubContext<THub>` — singleton proxy for server-side broadcasting
- Hub instance lacking connection context when activated outside SignalR
- Redis backplane or Azure SignalR for cross-instance message fan-out

**Answer**

Hubs are not registered in DI for direct injection — injecting a concrete `Hub` either fails activation or produces an instance without a connection context. The correct mechanism is `IHubContext<THub>`, a singleton proxy registered by `AddSignalR()`. For multi-instance deployments, pair it with a Redis backplane or Azure SignalR Service.

---

#### Gotcha 15. SignalR scale-out without backplane

**Concepts**
- In-memory connection registry local to each pod
- Sticky sessions routing connections but not cross-instance messages
- Redis backplane and Azure SignalR Service for full fan-out
- Group membership and connection IDs scoped per process instance

**Answer**

Each process maintains its own in-memory connection registry. Sticky sessions route a client to the same pod but do not fan-out cross-instance messages — a controller on instance A calling `IHubContext.Clients.User(id).SendAsync` misses users on instance B. The fix is a Redis backplane or Azure SignalR Service.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (D) A product page loads a comment list via AJAX. One teammate returns `PartialView("_Comments", comments)`; another returns `JsonResult(comments)` and builds HTML in JavaScript. The page also needs optimistic UI updates and SEO on the initial load. Which approach fits each concern, and what would you standardize on for this MVC app?

**Concepts**
- `PartialView` reusing server-side encoding and matching initial load partial for SEO consistency
- `JsonResult` enabling optimistic UI updates without re-rendering the full comment list
- SEO requiring server-side rendered initial content — both approaches compatible with initial full-page render
- Standardization on `PartialView` for list refresh, `Json` for lightweight state signals

**Answer**

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

**Concepts**
- Missing `RequestVerificationToken` header causing antiforgery 400 before action executes
- JSON body with no `[FromBody]` — form value provider binding not receiving JSON content
- Two separate problems — antiforgery failure masking the binding mismatch
- Fix requiring both token header inclusion and either `[FromBody]` on action or `URLSearchParams` body

**Answer**

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

**Concepts**
- `fetch` default `credentials: 'same-origin'` — cross-origin requests omit cookies
- Antiforgery cookie not sent cross-origin — token validation finds no matching cookie
- `credentials: 'include'` required for cross-origin cookie-based auth
- CORS policy with `AllowCredentials()` and specific origin required on the server

**Answer**

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

**Concepts**
- `@Html.Raw(hit.HighlightedHtml)` bypassing Razor encoding — user `term` injected verbatim
- Server-side highlight building `<mark>` around unencoded `term` — XSS vector in the HTML
- `HtmlEncoder.Encode(term)` required before embedding in HTML string concatenation
- `data-term="@hit.Term"` correctly encoded by Razor's default `@` encoding

**Answer**

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

**Concepts**
- `URLSearchParams` sending `application/x-www-form-urlencoded` body
- `[FromBody]` expecting JSON input formatter — form-encoded body not deserialized
- Silent empty model — action runs with all defaults, grid shows wrong results
- Fix: remove `[FromBody]` for form-encoded body, or send JSON with correct Content-Type

**Answer**

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

**Concepts**
- `UseExceptionHandler` returning full HTML error page — layout included
- No `response.ok` check before `innerHTML` assignment — error page injected into widget
- Server-side AJAX detection for compact error shape vs full HTML error page
- Client-side fallback — toast or inline error element on non-2xx response

**Answer**

_Answer not found._

---

#### Q7. (P) A `_StockBadge` partial is fetched via AJAX on every product hover. After deploy behind CloudFront, some users see stale "In stock" badges for sold-out items. The action returns `PartialView` with `Cache-Control` unset. What headers and action patterns fix this without disabling caching entirely for static assets?

**Concepts**
- Unset `Cache-Control` — CDN applies default caching rules, serving stale personalized content
- `Cache-Control: private, no-store` for user-specific or frequently changing partials
- `[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]` on the action
- Separate CDN behavior for static assets (long TTL) vs MVC partial routes (no-store)

**Answer**

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

**Concepts**
- Duplicate `id="edit-row"` and `id="quick-edit-form"` — `getElementById` returns only the first match
- Event listener wired at page load before partials are injected — no elements present yet
- Event delegation on stable parent container as the fix for dynamic content
- Model-scoped id suffixes for accessibility-required ids in reusable partials

**Answer**

_Answer not found._

---

#### Q9. (P) A team uses **unobtrusive AJAX** (`data-ajax="true"`) on a modal form with client-side validation (`jquery.validate.unobtrusive`). After a successful AJAX POST, the modal closes and reopens with a fresh empty form — but validation errors no longer appear until a full page reload. What causes this and how do you fix it in an MVC partial-update flow?

**Concepts**
- Unobtrusive validation rules parsed once on initial DOM ready — not re-parsed after dynamic injection
- `$.validator.unobtrusive.parse(container)` required after replacing form HTML via AJAX
- Stale validator state on the form element after `innerHTML` replacement
- Modal close and reopen preserving old validator instance on the replaced form

**Answer**

_Answer not found._

---

#### Q10. (D) Compare **ASP.NET MVC unobtrusive AJAX + jQuery** vs **HTMX** (or `fetch` + `innerHTML`) for a CRUD admin grid with inline edit, delete confirm, and server-rendered row partials. What breaks or gets simpler when you switch, and when would you not choose HTMX?

**Concepts**
- Unobtrusive AJAX — declarative `data-ajax-*`, automatic antiforgery, jQuery dependency
- HTMX — declarative `hx-*` attributes without jQuery, native `fetch` under the hood
- Re-parse requirement for unobtrusive validation after AJAX-updated fragments
- HTMX not integrating with `jquery.validate.unobtrusive` — separate validation approach needed

**Answer**

_Answer not found._

---

#### Q11. (M) Walk through what happens on an AJAX POST that returns `PartialView` when `[ValidateAntiForgeryToken]` is present: where the token is validated, what status code the client sees without a token, and how `[AutoValidateAntiforgeryToken]` on the controller differs from `[IgnoreAntiforgeryToken]` on one action.

**Concepts**
- `[ValidateAntiForgeryToken]` implemented as an authorization filter — runs before model binding
- Missing token returning 400 before action executes
- `[AutoValidateAntiforgeryToken]` applying to all unsafe methods on the controller
- `[IgnoreAntiforgeryToken]` adding opt-out metadata for specific actions

**Answer**

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

**Concepts**
- Full-page POST success returning partial fragment — browser navigates to bare HTML fragment
- No redirect on full-page success — browser refresh re-submits POST, creating duplicate rows
- Single action serving two incompatible response contracts — PRG vs AJAX partial
- Branching on `Accept` header or `X-Requested-With`, or splitting into separate actions

**Answer**

_Answer not found._

---
