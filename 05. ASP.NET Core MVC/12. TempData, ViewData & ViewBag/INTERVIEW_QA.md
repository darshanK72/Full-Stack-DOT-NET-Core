# TempData, ViewData & ViewBag — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is `ViewBag` in ASP.NET Core MVC?](#q1-what-is-viewbag-in-aspnet-core-mvc)
2. [Q2. What is `ViewData` and how does it differ from `ViewBag`?](#q2-what-is-viewdata-and-how-does-it-differ-from-viewbag)
3. [Q3. What is `TempData` and when is it used?](#q3-what-is-tempdata-and-when-is-it-used)
4. [Q4. What is the difference between `ViewBag`, `ViewData`, and `TempData`?](#q4-what-is-the-difference-between-viewbag-viewdata-and-tempdata)
5. [Q5. Why is `TempData` used after `RedirectToAction`?](#q5-why-is-tempdata-used-after-redirecttoaction)
6. [Q6. What is the Post-Redirect-Get (PRG) pattern?](#q6-what-is-the-post-redirect-get-prg-pattern)
7. [Q7. Why doesn't `ModelState` survive a redirect?](#q7-why-doesnt-modelstate-survive-a-redirect)
8. [Q8. How can validation errors survive a redirect?](#q8-how-can-validation-errors-survive-a-redirect)
9. [Q9. What is the difference between cookie-based and session-based TempData?](#q9-what-is-the-difference-between-cookie-based-and-session-based-tempdata)
10. [Q10. What happens to TempData when it is read?](#q10-what-happens-to-tempdata-when-it-is-read)
11. [Q11. What is `TempData.Keep()` used for?](#q11-what-is-tempdatakeep-used-for)
12. [Q12. What is `TempData.Peek()` used for?](#q12-what-is-tempdatapeek-used-for)
13. [Q13. What are the size limits of cookie-based TempData?](#q13-what-are-the-size-limits-of-cookie-based-tempdata)
14. [Q14. Why does session-based TempData fail behind load balancers without sticky sessions?](#q14-why-does-session-based-tempdata-fail-behind-load-balancers-without-sticky-sessions)
15. [Q15. When should you use `ViewBag`/`ViewData` instead of a ViewModel?](#q15-when-should-you-use-viewbagviewdata-instead-of-a-viewmodel)
16. [Q16. When should you not use `ViewBag` for layout data?](#q16-when-should-you-not-use-viewbag-for-layout-data)
17. [Q17. Does TempData work on AJAX partial responses the same as full page redirects?](#q17-does-tempdata-work-on-ajax-partial-responses-the-same-as-full-page-redirects)
18. [Q18. What data should never be stored in TempData?](#q18-what-data-should-never-be-stored-in-tempdata)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is `ViewBag` in ASP.NET Core MVC?

What is `ViewBag` in ASP.NET Core MVC?

**Answer:** `ViewBag` is a dynamic property on `Controller` and `ViewPage` that provides a loosely typed dictionary for passing ad hoc data from an action to a view without declaring a ViewModel property.

- Implemented as a wrapper around `ViewData` using `dynamic` — assignments like `ViewBag.Title = "Home"` store entries in the shared view dictionary.
- Property access in Razor (`@ViewBag.Title`) resolves at runtime — typos compile without error and render as blank output.
- Suitable for incidental page metadata (title, active tab, layout flags) rather than primary page data or form models.
- Does not survive redirects — it is scoped to the current request's view rendering only.

---

## Q2. What is `ViewData` and how does it differ from `ViewBag`?

What is `ViewData` and how does it differ from `ViewBag`?

**Answer:** `ViewData` is a strongly keyed `ViewDataDictionary` on the controller and view context; `ViewBag` is a dynamic wrapper over the same underlying dictionary, so writes through either are visible to both.

- `ViewData["Title"] = "Home"` and `ViewBag.Title = "Home"` store the same entry — they are not separate stores.
- `ViewData` supports typed access via `ViewData.Model` (the `@model` type) and requires string keys for arbitrary entries.
- `ViewBag` offers dot-syntax convenience but sacrifices compile-time checking on property names in views.
- Both are request-scoped and do not persist across `RedirectToAction` — unlike `TempData`.

---

## Q3. What is `TempData` and when is it used?

What is `TempData` and when is it used?

**Answer:** `TempData` is a dictionary backed by a temp-data provider (cookie or session) that persists values across a redirect to the next HTTP request, making it ideal for flash messages after Post-Redirect-Get flows.

- Values written in a POST action survive `RedirectToAction` and are available when the subsequent GET action and view render.
- Typical use: `TempData["SuccessMessage"] = "Order created."` after a successful form submission followed by redirect to a details page.
- Backed by `ITempDataProvider` — the default in ASP.NET Core 8 serializes data into an encrypted cookie via Data Protection.
- Designed for short-lived, small payloads (status messages, flags) — not for transporting large view models or domain entities.

---

## Q4. What is the difference between `ViewBag`, `ViewData`, and `TempData`?

What is the difference between `ViewBag`, `ViewData`, and `TempData`?

**Answer:** All three pass data from controllers to views, but they differ in typing, lifetime, and persistence across redirects.

- **ViewBag / ViewData:** request-scoped only — available during the current action's view rendering; lost after `RedirectToAction`.
- **TempData:** survives one redirect (by default) via cookie or session provider — designed for flash messaging across PRG.
- **ViewBag** is dynamic; **ViewData** uses string keys; **TempData** uses string keys with cross-request persistence semantics.
- For primary page data and forms, prefer a strongly typed ViewModel over any of the three — they are supplementary mechanisms.

---

## Q5. Why is `TempData` used after `RedirectToAction`?

Why is `TempData` used after `RedirectToAction`?

**Answer:** After a redirect the browser issues a new GET request with no connection to the previous POST's `ViewBag`, `ViewData`, or `ModelState` — TempData is the built-in mechanism to carry a small message or flag into that next request.

- `RedirectToAction` returns 302/303 — the POST response body (including any ViewBag values) is discarded by the browser.
- TempData serializes values into the temp-data cookie (or session) so the GET action's layout or view can display a success banner.
- Without TempData, the user completes an action but sees no confirmation on the redirected page.
- Only the route values (e.g., `{ id = orderId }`) and TempData survive — reload the entity from the database on GET rather than passing it through TempData.

---

## Q6. What is the Post-Redirect-Get (PRG) pattern?

What is the Post-Redirect-Get (PRG) pattern?

**Answer:** PRG is the pattern of responding to a successful POST with an HTTP redirect to a GET action, preventing the browser from re-submitting the form when the user refreshes the page.

- POST validates input and performs the mutation; on success, return `RedirectToAction(nameof(Details), new { id })` instead of `return View()`.
- The browser's refresh on the GET page repeats a safe read — not the original POST — avoiding duplicate creates or charges.
- On validation failure, return `View(model)` in the same POST response to preserve `ModelState` without redirecting.
- TempData carries ephemeral success messages across the redirect; the GET action loads fresh data from services by route id.

---

## Q7. Why doesn't `ModelState` survive a redirect?

Why doesn't `ModelState` survive a redirect?

**Answer:** `ModelState` is stored in the controller's `ViewDataDictionary` for the current HTTP request only — a redirect ends that request and starts a new one with an empty `ModelState`.

- After `RedirectToAction`, the GET action receives no knowledge of previous validation errors unless explicitly rehydrated.
- Model binding on the GET request populates a fresh model from route/query values, not from the failed POST fields.
- This is by design — PRG intentionally separates the command (POST) from the query (GET) request lifecycle.
- Re-displaying validation errors after redirect requires TempData serialization helpers, a second validation pass on GET, or avoiding redirect on validation failure.

---

## Q8. How can validation errors survive a redirect?

How can validation errors survive a redirect?

**Answer:** Prefer `return View(model)` on validation failure without redirect to keep `ModelState` intact; if redirect is required, serialize errors to TempData or re-validate on the GET action.

- Standard pattern: invalid POST → `return View(model)` (same request, ModelState preserved); valid POST → redirect with TempData success message.
- Third-party helpers (e.g., `TempData.Put("ModelState", ModelState)`) serialize errors to TempData — watch cookie size limits.
- Alternative: redirect to GET with the entity id and run server-side validation again in the GET action before displaying the form.
- Client-side validation state is also lost on redirect — the GET view re-renders from server-side ModelState or fresh validation.

---

## Q9. What is the difference between cookie-based and session-based TempData?

What is the difference between cookie-based and session-based TempData?

**Answer:** Cookie-based TempData (the default `CookieTempDataProvider`) serializes values into an encrypted cookie sent with the next request; session-based TempData stores values server-side in ASP.NET session keyed by session id.

- **Cookie provider:** no server-side session store required; works across load-balanced pods without sticky sessions; subject to cookie size limits (~4 KB per cookie).
- **Session provider (`SessionStateTempDataProvider`):** supports larger arbitrary objects; requires session middleware and either sticky sessions or distributed session (Redis/SQL) in multi-server deployments.
- Cookie TempData uses ASP.NET Core Data Protection for encryption — key rings must be synchronized across instances or cookies become unreadable after deploy to another node.
- Configure via `builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider()` or cookie provider in MVC options.

---

## Q10. What happens to TempData when it is read?

What happens to TempData when it is read?

**Answer:** Reading a TempData key with the indexer (`TempData["Key"]`) marks it for deletion at the end of the current request — it is a read-once-by-default flash semantics.

- After the request completes, marked keys are removed and will not appear on the subsequent request.
- If both the layout and the view read the same key in one request, the first read consumes it unless `Peek` is used for the first access.
- `TempData.Keep("Key")` explicitly preserves a key for the **next** request even after it was read.
- This consume-on-read behavior makes TempData behave like a one-shot message queue, not persistent session state.

---

## Q11. What is `TempData.Keep()` used for?

What is `TempData.Keep()` used for?

**Answer:** `TempData.Keep("Key")` marks a TempData entry to survive into the next HTTP request even after it has been read in the current request — extending flash message life across a redirect chain.

- Use when a message must display across two consecutive GET requests (e.g., a multi-step redirect flow).
- Without `Keep`, a read TempData key is deleted at end of request and absent on the next round-trip.
- Overusing `Keep` causes messages to reappear on unintended pages — flash messages should normally die after one display.
- Distinct from `Peek`: `Keep` affects the **next** request; `Peek` allows multiple reads within the **same** request.

---

## Q12. What is `TempData.Peek()` used for?

What is `TempData.Peek()` used for?

**Answer:** `TempData.Peek("Key")` reads a TempData value without marking it for deletion, allowing multiple components in the same request (layout and view) to read the same flash message.

- Use in the layout to inspect a message while leaving it available for the child view in the same render pass.
- The key is still subject to normal deletion at end of request unless also `Keep()`'d for the next request.
- Preferred over double indexer reads when both layout and view need the same TempData key in one round-trip.
- Best practice: centralize flash display in a single `_FlashMessages.cshtml` partial invoked from the layout to avoid double-read issues entirely.

---

## Q13. What are the size limits of cookie-based TempData?

What are the size limits of cookie-based TempData?

**Answer:** Cookie-based TempData serializes all entries into a single cookie (typically `.AspNetCore.Mvc.CookieTempDataProvider`), constrained by browser cookie limits of approximately 4096 bytes per cookie and total request header size limits of roughly 8–16 KB.

- Storing large objects (full view models, validation error collections for 40+ fields) exceeds cookie capacity and throws or silently fails on redirect.
- Total header size includes all cookies, antiforgery tokens, and auth cookies — TempData competes for the same budget.
- Store only small identifiers and messages in TempData (e.g., `orderId`, `"Saved successfully"`) and reload data from services on GET.
- Switch to session-based TempData with distributed session only when large flash payloads are genuinely required and session infrastructure already exists.

---

## Q14. Why does session-based TempData fail behind load balancers without sticky sessions?

Why does session-based TempData fail behind load balancers without sticky sessions?

**Answer:** Session-based TempData stores data server-side keyed by session id — after redirect, the browser may hit a different pod that does not hold that session entry, returning empty TempData.

- In-memory session on one node is invisible to other nodes in a round-robin load-balanced deployment.
- Sticky sessions (session affinity) route the same client to the same pod, masking the problem but reducing failover flexibility.
- Fix: use cookie-based TempData (default) for flash strings, or configure distributed session (Redis, SQL Server) shared by all pods.
- Data Protection key ring synchronization is also required for cookie TempData across nodes — unrelated to session but equally critical for multi-instance deployments.

---

## Q15. When should you use `ViewBag`/`ViewData` instead of a ViewModel?

When should you use `ViewBag`/`ViewData` instead of a ViewModel?

**Answer:** Use ViewBag or ViewData for incidental page metadata that is not part of the primary data contract — page title, active navigation tab, breadcrumb flags, or one-off layout toggles — not for form models or domain data.

- `ViewBag.Title = "Dashboard"` in an action and `@ViewBag.Title` in a layout is a common, acceptable pattern.
- Passing `"ActiveTab" => "Settings"` to highlight navigation avoids bloating a ViewModel with presentation-only properties.
- Action filters can set ViewData entries consumed by layouts without changing the action's return model.
- Anything with validation attributes, client-side validation, or more than a few simple properties belongs in a strongly typed ViewModel.

---

## Q16. When should you not use `ViewBag` for layout data?

When should you not use `ViewBag` for layout data?

**Answer:** Avoid ViewBag for data that multiple views depend on with compile-time safety, data shared between layout and child views with complex types, or any value where a typo causes silent runtime failures.

- Strongly typed layout models or view components provide compile-time checking that ViewBag's dynamic access cannot offer.
- When the layout and view both need the same value, ViewBag typos (`ViewBag.UsreName`) render blank without build errors.
- Authorization or role checks should not live in ViewBag flags set from actions — use policy-based authorization and view components instead.
- Large or structured data (user profile objects, cart summaries) should use ViewModels or view components, not ViewBag dynamic properties.

---

## Q17. Does TempData work on AJAX partial responses the same as full page redirects?

Does TempData work on AJAX partial responses the same as full page redirects?

**Answer:** No — TempData is designed for the next full HTTP request after a redirect; AJAX partial updates that return HTML or JSON in the same POST response cycle do not re-render the layout where TempData is typically consumed.

- The layout executes on the initial full-page load — TempData written during an AJAX POST is not injected into an already-rendered DOM.
- Partial view responses replace a page fragment — the layout's TempData block does not re-execute.
- For AJAX success messages, return the message in the JSON response body or embed a toast element in the returned partial HTML.
- Reserve TempData for full-page POST → redirect → GET flows; use response payloads, SignalR, or client-side state for in-place updates.

---

## Q18. What data should never be stored in TempData?

What data should never be stored in TempData?

**Answer:** Never store sensitive secrets, large domain objects, PII beyond minimal identifiers, or anything that should persist beyond a single flash display — TempData is serialized into cookies or session and consumed ephemerally.

- Passwords, API keys, credit card numbers, and auth tokens must not pass through TempData — cookies are client-visible (even if encrypted) and logs may capture values.
- Full entity graphs or large view models exceed cookie size limits and expose internal schema details unnecessarily.
- Instead of storing an `OrderSummaryViewModel` in TempData, store `TempData["OrderId"] = id` and load the summary from the service on the GET action.
- Treat TempData as a flash notification channel — short strings, status flags, and route-correlated identifiers only.

---

> **Target:** ASP.NET Core 8 MVC

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

#### Q1. (R) After a successful save, users sometimes see no success banner. Review the POST action, redirect, layout, and detail view.

```csharp
[HttpPost]
public IActionResult Edit(int id, ProductEditModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    _products.Update(id, model);
    TempData["SuccessMessage"] = "Product updated.";
    return RedirectToAction(nameof(Details), new { id });
}

// _Layout.cshtml
@if (TempData["SuccessMessage"] is string msg)
{
    <div class="alert alert-success">@msg</div>
}

// Views/Products/Details.cshtml
@if (TempData["SuccessMessage"] is string detailMsg)
{
    <p class="text-muted">@detailMsg</p>
}
```

---

**Answer:**

**Answer:** TempData is **read-once by default** — the layout consumes `SuccessMessage` on the first read, so the detail view's second read returns null and the banner disappears or appears inconsistently depending on render order.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| TempData semantics | Two reads of same key in one request | First consumer wins; second gets null |
| UX | Layout + view both display flash | Message missing in one location |
| Design | Implicit consumption without coordination | Intermittent "no banner" bug reports |

**Fix (priority order):**

1. Read once — only the layout **or** the view displays the message, not both.
2. Or use `TempData.Peek("SuccessMessage")` in the layout so the value stays available for the same request; use `Keep()` only if you need it on the **next** request too.
3. Prefer a single partial `_FlashMessages.cshtml` invoked from the layout that owns all TempData keys.

**Production takeaway:** TempData looks like session state but behaves like a one-shot queue — double-read in one round-trip is a classic Karat trap.

---

---

#### Q2. (P) A team deploys to three Kubernetes pods behind a round-robin load balancer. They use **session-based** TempData (`AddSession()` + default `SessionStateTempDataProvider`). Flash messages intermittently disappear after redirect. What is happening, and what are the two production-viable fixes?

---

**Answer:**

_Answer not found._

---

#### Q3. (M) Compare **cookie-based** TempData (`CookieTempDataProvider`) vs **session-based** TempData. For each, name one advantage and one failure mode in production (scale-out, size, security, or ops).

---

**Answer:**

_Answer not found._

---

#### Q4. (R) A Razor view renders blank where the user's display name should appear. The action compiles and runs without exceptions locally.

```csharp
public IActionResult Profile()
{
    var user = _users.GetCurrent();
    ViewBag.UserName = user.DisplayName;
    ViewBag.LastLogin = user.LastLoginUtc;
    return View();
}

// Profile.cshtml
<h2>Welcome, @ViewBag.UsreName</h2>
<p>Last login: @ViewBag.LastLogin</p>
```

---

**Answer:**

_Answer not found._

---

#### Q5. (P) Implement the correct **Post-Redirect-Get (PRG)** flow for a "Create Order" form. Validation errors must survive the redirect; success must not re-POST on browser refresh. Sketch controller actions and where TempData vs ModelState belong.

---

**Answer:**

_Answer not found._

---

#### Q6. (D) A junior developer passes a 40-field `CustomerEditViewModel` through `ViewData["Customer"]` instead of a strongly typed view. When should `ViewData`/`ViewBag` be acceptable, and when should you insist on a ViewModel — especially for PRG and validation?

---

**Answer:**

_Answer not found._

---

#### Q7. (M) After switching to cookie TempData, POST → redirect intermittently throws `InvalidOperationException` about cookie size or request headers. The team stores a full `OrderSummaryViewModel` (line items + audit trail) in TempData for the confirmation page. What is the limit, and what pattern fixes it?

---

**Answer:**

_Answer not found._

---

#### Q8. (R) A SPA-style partial update uses AJAX; the developer expects a TempData flash message on the same request cycle.

```csharp
[HttpPost]
public IActionResult Approve(int id)
{
    _workflow.Approve(id);
    TempData["Toast"] = "Approved.";
    return PartialView("_ApprovalBadge", _workflow.GetStatus(id));
}

// JavaScript: POST /orders/approve/42, replace #badge with HTML response
// Layout toast reads TempData on full page loads only
```

---

**Answer:**

_Answer not found._

---

#### Q9. (M) Explain **`TempData.Keep()`** vs **`TempData.Peek()`** with a scenario where the layout reads a flash key once and a child view must read the same key on the **same** request without losing it on the **next** request.

---

**Answer:**

_Answer not found._

---

#### Q10. (R) Review this controller after a security review flagged "duplicate form submission" and "lost validation messages."

```csharp
[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    _users.Create(model);
    ViewBag.Message = "Account created.";
    return View("Confirmation");
}

[HttpGet]
public IActionResult Register() => View();

// Confirmation.cshtml uses @ViewBag.Message
```

What breaks for PRG, refresh safety, and cross-request messaging — and what is the prioritized fix?

**Answer:**

_Answer not found._

---
