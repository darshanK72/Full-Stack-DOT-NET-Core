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

**Concepts**
- Dynamic `ViewBag` property wrapping `ViewDataDictionary` on controller and Razor view
- Runtime property resolution — typos silent, no compile-time error
- Request-scoped — lost after any redirect
- Appropriate scope — incidental page metadata, not primary data or form models

**Answer**

`ViewBag` is a `dynamic` property on the controller and Razor view page that wraps `ViewDataDictionary` to provide a loosely typed bag for passing ad hoc values — `ViewBag.Title = "Home"` stores the entry in the shared view dictionary, and `@ViewBag.Title` reads it during rendering. Because property resolution is dynamic, typos like `@ViewBag.Tittle` compile without error and render blank at runtime, making `ViewBag` unsuitable for data that matters. It is request-scoped — any assigned value is gone after `RedirectToAction`. The right use is incidental page metadata such as page titles, active tab indicators, or layout flags, not form models, domain data, or anything requiring validation.

---

## Q2. What is `ViewData` and how does it differ from `ViewBag`?

**Concepts**
- `ViewDataDictionary` as the underlying shared store for both `ViewData` and `ViewBag`
- `ViewData` string-keyed dictionary with explicit cast on read
- `ViewBag` dynamic wrapper — same entries, dot-syntax access
- Both request-scoped, lost across redirects

**Answer**

`ViewData` and `ViewBag` share the same underlying `ViewDataDictionary` — writing `ViewData["Title"] = "Home"` and `ViewBag.Title = "Home"` store the same entry, so reads through either surface are interchangeable. `ViewData` uses string keys and requires an explicit cast when reading non-string values: `(int)ViewData["Count"]`. `ViewBag` offers dot-syntax convenience but loses compile-time key checking. Both are request-scoped — a redirect ends the request and the dictionary is discarded. `ViewData` also exposes `ViewData.Model`, which holds the strongly typed model passed to `View(model)` — this is the `@model` directive's backing store.

---

## Q3. What is `TempData` and when is it used?

**Concepts**
- `ITempDataDictionary` persisting values across one redirect via cookie or session provider
- Consume-on-read default — entries deleted after first access
- Default provider in ASP.NET Core — `CookieTempDataProvider` with Data Protection encryption
- Designed for short-lived flash payloads, not large view models or sensitive data

**Answer**

`TempData` is a dictionary backed by `ITempDataProvider` — by default `CookieTempDataProvider` — that serializes values across a redirect so they are available on the subsequent GET request. Values written in a POST action survive `RedirectToAction` and are accessible in the next action and its view. The defining behavior is consume-on-read: reading `TempData["Key"]` via the indexer marks it for deletion at the end of that request. The correct use case is flash messages in Post-Redirect-Get flows — `TempData["SuccessMessage"] = "Order created."` written before a redirect and displayed once on the confirmation page. It is backed by an encrypted cookie via Data Protection, so key rings must be synchronized across instances in multi-pod deployments.

---

## Q4. What is the difference between `ViewBag`, `ViewData`, and `TempData`?

**Concepts**
- `ViewBag` and `ViewData` — request-scoped, same underlying dictionary, different access syntax
- `TempData` — persists across one redirect via cookie or session provider
- Typing — `ViewData` string-keyed, `ViewBag` dynamic, `TempData` string-keyed with cross-request semantics
- All three supplementary — ViewModel preferred for primary page data and forms

**Answer**

`ViewBag` and `ViewData` share the same request-scoped dictionary — they differ only in access syntax (dynamic vs string key), and both are gone after any redirect. `TempData` persists across a single redirect via cookie or session — it is the only one of the three that survives `RedirectToAction`. The typing model differs too: `ViewData` requires explicit casts, `ViewBag` resolves dynamically at runtime, and `TempData` stores serializable objects that survive to the next request. All three are supplementary mechanisms — for primary page data, form models, and anything with validation attributes, a strongly typed ViewModel is the right choice.

---

## Q5. Why is `TempData` used after `RedirectToAction`?

**Concepts**
- 302 redirect ending the POST response — `ViewBag` and `ViewData` discarded
- Browser issuing a new GET request with no connection to prior POST state
- `TempData` serialized into cookie for the next request
- TempData as the built-in flash channel, not a substitute for reloading data from services

**Answer**

A `RedirectToAction` response sends a 302 to the browser, which issues a new independent GET request. That new request has no connection to the previous POST's `ViewBag`, `ViewData`, or `ModelState` — they are all request-scoped and discarded when the POST response completes. TempData survives because `ITempDataProvider` serializes the dictionary into an encrypted cookie on the POST response and deserializes it on the subsequent GET request. The only things that survive a redirect are route values in the URL and TempData — which makes TempData the correct channel for flash success messages, not a place to pass the entity. The GET action should reload data from services by route id.

---

## Q6. What is the Post-Redirect-Get (PRG) pattern?

**Concepts**
- PRG — POST mutates, response redirects to a GET, GET displays result
- Browser refresh repeating last request — GET is safe, POST is not
- `RedirectToAction` after success to make the last browser request a GET
- `return View(model)` on validation failure — no redirect, preserving `ModelState`

**Answer**

Post-Redirect-Get is the pattern of responding to a successful form POST with an HTTP redirect to a GET action rather than returning a view directly. The browser's last request then becomes the safe GET, so pressing refresh repeats a read rather than re-submitting the POST body — preventing duplicate orders, registrations, or charges. On success: `_products.Update(id, model)` followed by `return RedirectToAction(nameof(Details), new { id })`. TempData carries the flash message across the redirect. On validation failure, return `View(model)` in the same POST response to keep `ModelState` intact — redirect only on success.

---

## Q7. Why doesn't `ModelState` survive a redirect?

**Concepts**
- `ModelState` stored in `ViewDataDictionary` — request-scoped, not persisted
- Redirect ending the POST request — GET starts fresh with empty `ModelState`
- PRG intentionally separating command (POST) from query (GET) lifecycle
- Return `View(model)` on failure to preserve validation errors without redirect

**Answer**

`ModelState` lives in the controller's `ViewDataDictionary` for the current HTTP request only — it is populated by model binding from the POST body and is discarded when the response completes. A redirect sends a 302 ending the POST request; the subsequent GET starts a new request lifecycle with an empty `ModelState` and no knowledge of previous validation errors. This separation is by design in PRG — the GET is a clean read, not a replay of the POST. The correct pattern is to redirect only on success and return `View(model)` on validation failure to display errors in the same request where `ModelState` is still populated.

---

## Q8. How can validation errors survive a redirect?

**Concepts**
- Standard approach — `return View(model)` on failure, no redirect
- TempData serialization of `ModelState` entries as a workaround when redirect is required
- Second validation pass on the GET action as an alternative
- Cookie size constraint limiting TempData-based error serialization for large forms

**Answer**

The standard approach is not to redirect on validation failure — return `View(model)` in the POST response so `ModelState` errors display inline without any cross-request transfer. If redirect on failure is genuinely required, serialize `ModelState` errors to TempData before redirecting and rehydrate them in the GET action — third-party helpers like `ITempDataSerializer` exist for this purpose, but they are subject to cookie size limits for forms with many fields. A cleaner alternative is to redirect with the entity id and re-run server-side validation on the GET action before rendering the pre-populated form. Client-side validation state is always lost on redirect and is re-derived from server-side `ModelState` on the next render.

---

## Q9. What is the difference between cookie-based and session-based TempData?

**Concepts**
- `CookieTempDataProvider` — client-side storage in an encrypted cookie, no server state
- `SessionStateTempDataProvider` — server-side storage keyed by session id
- Cookie provider working across pods without sticky sessions
- Session provider requiring distributed session for multi-node deployments

**Answer**

Cookie-based TempData (`CookieTempDataProvider`) serializes all values into a single encrypted cookie via Data Protection and sends it with the response — the server holds no state, so it works across load-balanced pods without sticky sessions. Session-based TempData (`SessionStateTempDataProvider`) stores values server-side in ASP.NET session keyed by a session cookie id — supports larger arbitrary objects but requires the same session store to be reachable on every pod, meaning in-memory session fails behind round-robin load balancers. Cookie provider is the ASP.NET Core 8 default; session provider requires `AddSession()` and session middleware. Data Protection key ring synchronization is required for cookie provider in multi-instance deployments — without it, cookies encrypted on one node become unreadable on another.

---

## Q10. What happens to TempData when it is read?

**Concepts**
- Consume-on-read default — indexer access marks key for deletion at request end
- Read-once semantics distinct from session state persistence
- `Peek()` reading without consuming in the same request
- `Keep()` preserving a consumed key for the next request

**Answer**

Reading a TempData key via the indexer (`TempData["Key"]`) marks it for deletion at the end of the current request — after the response completes, the provider removes those marked entries so they are absent on the next round-trip. This is consume-on-read semantics: TempData behaves like a one-shot message queue, not persistent session state. If both the layout and the view indexer-access the same key in one request, the first access consumes it and the second returns null. `Peek("Key")` reads without marking for deletion; `Keep("Key")` reverses a prior mark to preserve the entry for the next request.

---

## Q11. What is `TempData.Keep()` used for?

**Concepts**
- `Keep(key)` — un-marking a consumed entry so it survives to the next request
- Multi-step redirect chains requiring the same flash message across two GET requests
- Overuse causing messages to reappear on unintended pages
- `Keep` vs `Peek` — `Keep` for next-request retention, `Peek` for same-request multi-read

**Answer**

`TempData.Keep("Key")` reverses the consume-on-read deletion mark, preserving the entry so it is still present on the next HTTP request after it was read in the current one. The use case is a redirect chain where a flash message must survive two consecutive GET requests — for example, an intermediate redirect to a payment gateway and back before the confirmation page renders. Without `Keep`, reading the entry on the first GET deletes it; the second GET sees nothing. Overusing `Keep` causes messages to reappear on unrelated pages if the user navigates further — flash messages should survive exactly one display and then disappear. `Keep` targets the next request; `Peek` targets multiple reads within the same request.

---

## Q12. What is `TempData.Peek()` used for?

**Concepts**
- `Peek(key)` — reading without marking for deletion
- Same-request multi-read — layout and view both accessing the same flash key
- `Peek` for same-request reads, `Keep` for next-request retention
- Centralizing flash display in a single partial as a cleaner alternative

**Answer**

`TempData.Peek("Key")` reads the value without marking it for deletion, so the entry is still available for subsequent reads in the same request. The canonical use is a layout that inspects a flash message first — `Peek` in the layout leaves the entry available for the child view to also read via the indexer in the same render pass. The key is still subject to normal consume-on-read deletion at request end unless also `Keep()`'d for the next request. In practice, the cleanest pattern is to centralize flash rendering in a single `_FlashMessages.cshtml` partial invoked from the layout and read each key exactly once there, avoiding the need for `Peek` entirely.

---

## Q13. What are the size limits of cookie-based TempData?

**Concepts**
- Browser per-cookie limit — approximately 4096 bytes
- Total request header size — browser and server limits of 8–16 KB
- All TempData entries serialized into a single cookie competing with antiforgery and auth cookies
- Store identifiers in TempData, reload full data from services on GET

**Answer**

Cookie-based TempData serializes all entries into a single `.AspNetCore.Mvc.CookieTempDataProvider` cookie — browsers impose a per-cookie limit of approximately 4096 bytes and a total header size limit around 8–16 KB depending on browser and server configuration. That budget is shared with antiforgery tokens, auth cookies, and other application cookies. Storing a large view model — a 40-field `OrderSummaryViewModel` or a `ModelStateDictionary` with many errors — exceeds cookie capacity and either throws `InvalidOperationException` or silently truncates. The correct pattern is to store only a small identifier in TempData, such as `TempData["OrderId"] = order.Id`, and load the full summary from services on the GET action using that id.

---

## Q14. Why does session-based TempData fail behind load balancers without sticky sessions?

**Concepts**
- In-memory session local to each pod — not shared across instances
- Redirect landing on a different pod after round-robin load balancing
- Sticky sessions masking the problem but not solving cross-instance state
- Distributed session (Redis, SQL) or cookie TempData as the correct multi-node fix

**Answer**

Session-based TempData stores entries server-side in ASP.NET session keyed by a session id cookie. After a redirect, the browser sends the session id cookie to the next request — but round-robin load balancing may route that request to a different pod whose in-memory session has no entry for that id, so `TempData["Key"]` returns null and the flash message disappears. Sticky sessions (session affinity) route the same client to the same pod and mask the problem, but they reduce failover flexibility and are brittle during rolling deploys. The correct fixes are either to switch to cookie-based TempData (the default), which carries the data in the cookie and requires no server-side session store, or to configure a distributed session provider such as Redis so all pods share the same session store.

---

## Q15. When should you use `ViewBag`/`ViewData` instead of a ViewModel?

**Concepts**
- ViewBag/ViewData appropriate for incidental page metadata not part of primary data contract
- Action filters populating `ViewData` for layouts without changing action return types
- ViewModel preferred when validation attributes, multiple fields, or client-side binding required
- Layout data like page title and active tab as the canonical ViewBag use case

**Answer**

`ViewBag` and `ViewData` are appropriate for incidental page metadata that is not part of the primary data contract — `ViewBag.Title = "Dashboard"`, `ViewBag.ActiveTab = "Settings"`, or layout-level flags that no ViewModel property should own. Action filters can populate `ViewData` entries consumed by layouts without changing the action's return model, which is useful for breadcrumbs or per-action layout toggles. Anything with validation attributes, client-side binding, more than a few simple properties, or data the view must cast and process belongs in a strongly typed ViewModel. The rule of thumb: if it appears in `asp-validation-for`, `asp-for`, or requires a model directive, it is ViewModel territory.

---

## Q16. When should you not use `ViewBag` for layout data?

**Concepts**
- Compile-time checking absent — dot-property typos produce silent blank output
- Complex or structured data exposed via dynamic access without IntelliSense support
- Authorization and role checks belonging in policies and filters, not ViewBag flags
- View components and strongly typed layout models as the alternatives for shared structured data

**Answer**

Avoid `ViewBag` when multiple views depend on the same data and a compile-time contract is needed, when the data is structured or complex rather than a simple string or flag, and when authorization or role decisions flow from the value — dynamic access cannot be verified at build time and typos produce blank output without compiler errors. A strongly typed layout model or view component provides compile-time safety and IntelliSense. Authorization checks based on role or policy should be expressed in `[Authorize]` attributes, policies, or view components that query `HttpContext.User`, not in `ViewBag` flags set from actions that can be accidentally omitted on new actions.

---

## Q17. Does TempData work on AJAX partial responses the same as full page redirects?

**Concepts**
- TempData designed for the next full HTTP request after a redirect
- AJAX partial response returning HTML fragment — layout not re-executing
- TempData written during AJAX POST not visible in already-rendered DOM
- JSON response body or embedded partial element as the AJAX-appropriate flash channel

**Answer**

TempData does not work the same way for AJAX partial responses. It is designed for the POST → redirect → GET full-page cycle: the value serializes into the cookie on the POST response and is read during the subsequent GET's full-page layout and view render. In an AJAX partial update, the POST response returns an HTML fragment that replaces a DOM container — the layout has already rendered and is not re-executed, so any TempData written during the AJAX POST is never consumed during that response cycle. If the user then performs a full-page navigation, the TempData may unexpectedly appear. For AJAX success or error messages, embed the message in the JSON response body or include a toast element in the returned partial HTML rather than relying on TempData.

---

## Q18. What data should never be stored in TempData?

**Concepts**
- Sensitive credentials and auth tokens — encrypted cookie is still client-side
- Large domain objects and entity graphs — exceed cookie size limits
- PII beyond minimal identifiers — audit and compliance exposure
- TempData as a flash notification channel, not a data transport

**Answer**

Never store passwords, API keys, JWT tokens, session secrets, or credit card numbers in TempData — even though `CookieTempDataProvider` encrypts the cookie via Data Protection, the ciphertext is still client-visible, can be transmitted over insecure connections if HTTPS is misconfigured, and may appear in logs or browser developer tools. Full entity graphs or large view models exceed cookie size limits and expose internal schema details. PII beyond a minimal identifier such as an order id creates compliance exposure since TempData may be logged or transmitted beyond its intended lifespan. The correct use is a flash notification channel: short status strings, status flags, and route-correlated identifiers only — load full data from services on the GET action using those identifiers.

---

> **Target:** ASP.NET Core 8 MVC

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

**Concepts**
- TempData consume-on-read — first indexer access marks the key for deletion
- Layout executing before the child view — first consumer wins
- `TempData.Peek()` reading without consuming
- Single ownership pattern — one partial responsible for all flash display

**Answer**

TempData is consumed on the first indexer read — the layout reads `TempData["SuccessMessage"]` during render, which marks the key for deletion, so the child view's second read returns null. Depending on layout render order and caching, the banner appears in one location or neither. The fix is to read the key exactly once. The simplest approach is to use `TempData.Peek("SuccessMessage")` in the layout so the value is still present when the child view reads it. A cleaner long-term pattern is a single `_FlashMessages.cshtml` partial invoked once from the layout, which owns all TempData flash keys and reads each exactly once — eliminating the double-read problem for every view in the application.

---

#### Q2. (P) A team deploys to three Kubernetes pods behind a round-robin load balancer. They use **session-based** TempData (`AddSession()` + default `SessionStateTempDataProvider`). Flash messages intermittently disappear after redirect. What is happening, and what are the two production-viable fixes?

**Concepts**
- In-memory session state local to each pod
- Round-robin routing landing redirected GET on a different pod than the POST
- Cookie TempData as a stateless alternative requiring no server-side session store
- Distributed session (Redis, SQL Server) sharing session across all pods

**Answer**

_Answer not found._

---

#### Q3. (M) Compare **cookie-based** TempData (`CookieTempDataProvider`) vs **session-based** TempData. For each, name one advantage and one failure mode in production (scale-out, size, security, or ops).

**Concepts**
- Cookie TempData — stateless, no server store, works across pods without sticky sessions
- Session TempData — server-side storage, larger payload capacity
- Cookie size limit (~4 KB) as cookie TempData failure mode
- Missing distributed session store as session TempData failure mode in multi-node deployments

**Answer**

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

**Concepts**
- Dynamic `ViewBag` resolving at runtime — typo `UsreName` silently returns null
- No compile-time error for misspelled `ViewBag` properties
- Strongly typed ViewModel preventing this class of runtime blank-output bug
- IntelliSense coverage as a practical advantage of ViewModel over ViewBag

**Answer**

_Answer not found._

---

#### Q5. (P) Implement the correct **Post-Redirect-Get (PRG)** flow for a "Create Order" form. Validation errors must survive the redirect; success must not re-POST on browser refresh. Sketch controller actions and where TempData vs ModelState belong.

**Concepts**
- PRG — POST mutates then redirects, GET displays
- `return View(model)` on validation failure — no redirect, `ModelState` intact
- TempData carrying success message across the redirect, not entity data
- Reload entity from service on GET using the route id from the redirect

**Answer**

_Answer not found._

---

#### Q6. (D) A junior developer passes a 40-field `CustomerEditViewModel` through `ViewData["Customer"]` instead of a strongly typed view. When should `ViewData`/`ViewBag` be acceptable, and when should you insist on a ViewModel — especially for PRG and validation?

**Concepts**
- `ViewData`/`ViewBag` appropriate for incidental metadata, not primary form models
- `asp-for` and `asp-validation-for` requiring strongly typed `@model` declaration
- PRG round-trip — ViewModel on both POST and GET actions for validation consistency
- Runtime cast errors and missing IntelliSense from untyped `ViewData["Customer"]`

**Answer**

_Answer not found._

---

#### Q7. (M) After switching to cookie TempData, POST → redirect intermittently throws `InvalidOperationException` about cookie size or request headers. The team stores a full `OrderSummaryViewModel` (line items + audit trail) in TempData for the confirmation page. What is the limit, and what pattern fixes it?

**Concepts**
- Cookie TempData serialized into a single cookie — ~4 KB per-cookie browser limit
- Total request header size budget shared with auth cookies and antiforgery tokens
- Store identifiers in TempData, reload full data from services on GET
- Compressed or chunked TempData as impractical alternatives vs the redirect id pattern

**Answer**

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

**Concepts**
- TempData designed for POST → redirect → GET cycle, not same-request AJAX responses
- Layout not re-executing during AJAX partial HTML injection
- TempData written during AJAX POST deferred to the next full-page GET
- JSON response body or toast element in returned partial as the AJAX-native flash channel

**Answer**

_Answer not found._

---

#### Q9. (M) Explain **`TempData.Keep()`** vs **`TempData.Peek()`** with a scenario where the layout reads a flash key once and a child view must read the same key on the **same** request without losing it on the **next** request.

**Concepts**
- `Peek()` — same-request read without consuming
- `Keep()` — reverse deletion mark so key survives to next request
- Both operating on different time horizons — same request vs next request
- Layout as appropriate `Peek` caller, child view as final consumer

**Answer**

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

**Concepts**
- `return View("Confirmation")` after POST — browser last request is POST, refresh re-submits
- `ViewBag.Message` request-scoped — lost on redirect, not suitable for cross-request flash
- PRG pattern — redirect to a GET confirmation action after success
- TempData replacing `ViewBag.Message` for the redirect flash channel

**Answer**

_Answer not found._

---
