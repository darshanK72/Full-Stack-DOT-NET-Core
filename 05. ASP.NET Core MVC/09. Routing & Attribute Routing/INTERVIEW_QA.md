# Routing & Attribute Routing — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is conventional routing in ASP.NET Core MVC?](#q1-what-is-conventional-routing-in-aspnet-core-mvc)
2. [Q2. What is the default route pattern `{controller=Home}/{action=Index}/{id?}`?](#q2-what-is-the-default-route-pattern-controllerhomeactionindexid)
3. [Q3. What is attribute routing in MVC controllers?](#q3-what-is-attribute-routing-in-mvc-controllers)
4. [Q4. What is the difference between conventional routing and attribute routing?](#q4-what-is-the-difference-between-conventional-routing-and-attribute-routing)
5. [Q5. What are route constraints (e.g., `:int`, `:exists`)?](#q5-what-are-route-constraints-eg-int-exists)
6. [Q6. Why does route registration order matter in `MapControllerRoute`?](#q6-why-does-route-registration-order-matter-in-mapcontrollerroute)
7. [Q7. How does the `{area:exists}` constraint work?](#q7-how-does-the-areaexists-constraint-work)
8. [Q8. What is the difference between `[Route]` on a controller vs on an action?](#q8-what-is-the-difference-between-route-on-a-controller-vs-on-an-action)
9. [Q9. What does a leading slash in `[HttpGet("/export/{year}")]` mean?](#q9-what-does-a-leading-slash-in-httpgetexportyear-mean)
10. [Q10. What is a catch-all route parameter (`{*slug}`)?](#q10-what-is-a-catch-all-route-parameter-slug)
11. [Q11. How do HTTP verbs (`[HttpGet]`, `[HttpPost]`) affect action selection?](#q11-how-do-http-verbs-httpget-httppost-affect-action-selection)
12. [Q12. What happens when two actions match the same route?](#q12-what-happens-when-two-actions-match-the-same-route)
13. [Q13. How does Tag Helper link generation (`asp-controller`, `asp-action`) relate to routing?](#q13-how-does-tag-helper-link-generation-asp-controller-asp-action-relate-to-routing)
14. [Q14. Why must `area` be specified when generating links to area controllers from outside the area?](#q14-why-must-area-be-specified-when-generating-links-to-area-controllers-from-outside-the-area)
15. [Q15. What is `LowercaseUrls` and how does it affect link generation?](#q15-what-is-lowercaseurls-and-how-does-it-affect-link-generation)
16. [Q16. What is the difference between "no route matched" and "405 Method Not Allowed"?](#q16-what-is-the-difference-between-no-route-matched-and-405-method-not-allowed)
17. [Q17. How do optional route parameters (`{id?}`) and defaults interact?](#q17-how-do-optional-route-parameters-id-and-defaults-interact)
18. [Q18. What is the areas route pattern and how does it differ from the default route?](#q18-what-is-the-areas-route-pattern-and-how-does-it-differ-from-the-default-route)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is conventional routing in ASP.NET Core MVC?

**Concepts**
- Centralized route template registration in `Program.cs`
- `MapControllerRoute` and the endpoint routing middleware
- Segment-to-controller/action name mapping
- Route defaults for omitted URL segments
- Registration order and template specificity

**Answer**

Conventional routing maps URLs to controllers and actions using route templates registered centrally in `Program.cs` via `MapControllerRoute`. The endpoint routing middleware extracts segment values from the URL at runtime and invokes the matching action method — so `{controller=Home}/{action=Index}/{id?}` maps the first segment to a controller name (without the `Controller` suffix) and the second to an action name. Because defaults are baked into the template, a bare `/` resolves to `HomeController.Index()` with no additional configuration. Multiple named routes can coexist, which means registration order and specificity determine which route wins — more specific templates must come before general catch-all patterns.

---

## Q2. What is the default route pattern `{controller=Home}/{action=Index}/{id?}`?

**Concepts**
- Segment-to-controller/action/id mapping convention
- Inline route defaults for omitted URL segments
- Case-insensitive controller and action name matching
- Optional `{id?}` parameter token

**Answer**

The template maps the first URL segment to a controller name (minus the `Controller` suffix), the second to an action method, and an optional third to a parameter named `id`. The inline defaults — `controller=Home` and `action=Index` — mean a bare `/` resolves to `HomeController.Index()` with `id = null`. Matching is case-insensitive by default in ASP.NET Core 8, so `/products/details/42` and `/Products/Details/42` both reach `ProductsController.Details(42)`. The `?` on `{id?}` lets the same route template handle actions that accept or omit an id parameter.

---

## Q3. What is attribute routing in MVC controllers?

**Concepts**
- `[Route]`, `[HttpGet]`, `[HttpPost]` decorating controllers and actions directly
- `MapControllers()` for attribute route discovery at startup
- Controller-level prefix combined with action-level template
- Coexistence of attribute and conventional routes

**Answer**

Attribute routing colocates URL templates directly on the controller or action that handles them, rather than centralizing them in `Program.cs`. Calling `MapControllers()` at startup discovers all controllers and their route attributes and registers them into the endpoint route table. A controller-level `[Route("api/[controller]")]` prefixes every action in that class, while an action-level `[HttpGet("{id:int}")]` appends HTTP method constraints and parameter tokens. Attribute routes can coexist with conventional routes in the same application and are preferred for REST-style APIs and fixed URLs where you want the route shape to be explicit in code rather than inferred from class names.

---

## Q4. What is the difference between conventional routing and attribute routing?

**Concepts**
- Conventional routing — centralized patterns inferred from controller and action names
- Attribute routing — explicit colocated templates on controller classes
- Fine-grained URL control vs consistent URL shape trade-off
- Link generation with Tag Helpers across both styles

**Answer**

The key distinction is where the URL pattern lives. Conventional routing keeps everything in `Program.cs` and infers controller and action from URL segments, which suits sites with a consistent `{controller}/{action}` shape. Attribute routing puts the template on the class itself, making each action's public URL explicit — this is better for versioning prefixes, literal segments, or scenarios where the URL cannot follow the controller-naming convention. Tag Helpers work with both; attribute routes need the correct template tokens in `asp-route-*` values. When mixing both in the same application, I need to be careful because attribute routes and conventional routes compete in the same endpoint route table.

---

## Q5. What are route constraints (e.g., `:int`, `:exists`)?

**Concepts**
- Built-in constraints — `:int`, `:guid`, `:alpha`, `:minlength(n)`
- `IRouteConstraint` for custom validation logic
- Constraint failure yielding "no route matched" 404 rather than a bound parameter
- Constraint participation in URL generation

**Answer**

Route constraints restrict which values a route parameter will accept before an action is selected. `:int` rejects non-numeric segments so `/orders/abc` does not match `orders/{id:int}`, allowing another route to handle the request instead. The `{area:exists}` constraint verifies the captured segment matches a registered area name, preventing arbitrary first segments from being misinterpreted as areas. Constraints also participate in URL generation — if a supplied value violates the constraint, link generation fails or omits the segment. A failed constraint always yields a "no route matched" 404, not an action invocation with invalid data passed to the parameter.

---

## Q6. Why does route registration order matter in `MapControllerRoute`?

**Concepts**
- First-match evaluation order in endpoint routing
- Specific routes must precede catch-all patterns
- Registration sequence in `Program.cs` over named route priority
- Attribute route order metadata merged into the endpoint collection

**Answer**

Endpoint routing evaluates registered routes in the order they appear in `Program.cs` and takes the first template that matches the request. This means more specific routes must be registered before general ones like the default `{controller}/{action}/{id?}`. A slug route `products/{id}` registered before the default captures `/products/sale` as `id = "sale"` rather than routing to the `Sale` action via the conventional template. Named routes do not affect matching priority — only registration sequence matters. Attribute routes from `MapControllers()` are merged into the same endpoint collection with their own order metadata, so integration tests should cover ambiguous URLs whenever new conventional routes are added.

---

## Q7. How does the `{area:exists}` constraint work?

**Concepts**
- `exists` constraint verifying captured segment against registered area names
- `[Area("Name")]` attribute as the registration mechanism — not folder placement
- Standard areas route template using the constraint
- Link generation requiring a registered area name

**Answer**

The `exists` constraint on an `area` parameter checks that the captured first URL segment corresponds to a known area in the application — specifically, a controller decorated with `[Area("Name")]`. If no such area is registered, the route fails to match and the request falls through to the next route. This prevents arbitrary first segments from being mistakenly interpreted as area names. Physical folder placement under `Areas/` is not enough — the `[Area]` attribute on the controller class is what registers the area name. For link generation, `asp-area` must use a registered name or the URL will be generated incorrectly.

---

## Q8. What is the difference between `[Route]` on a controller vs on an action?

**Concepts**
- Controller-level `[Route]` establishing a shared URL prefix for all actions
- Action-level template appending to or overriding the controller prefix
- Root-absolute action template (leading `/`) ignoring the controller prefix
- HTTP method attributes as attribute routing with an implicit verb constraint

**Answer**

A controller-level `[Route("prefix")]` establishes a shared URL prefix that every action in the class inherits. An action-level template appends a segment relative to that prefix — so `[Route("[controller]")]` on the controller plus `[HttpGet("{id}")]` on an action yields `/Products/{id}` for `ProductsController`. The important exception is a leading slash: `[HttpGet("/absolute")]` is root-absolute and ignores the controller prefix entirely, mapping from the site root regardless of what the controller declares. HTTP method attributes like `[HttpGet]` and `[HttpPost]` are themselves a form of attribute routing that also impose a verb constraint on the template.

---

## Q9. What does a leading slash in `[HttpGet("/export/{year}")]` mean?

**Concepts**
- Root-absolute route template ignoring any controller-level prefix
- Contrast with relative template that appends to the controller prefix
- Fixed marketing or report URLs independent of controller naming

**Answer**

A route template starting with `/` is absolute from the application root and does not combine with any controller-level `[Route]` prefix. So `/export/{year}` maps directly to that path regardless of a `[Route("api")]` on the controller — the generated URL is `/export/2024` rather than `/api/export/2024`. Without the leading slash, the relative template `export/{year}` would append to the controller prefix. Root-absolute templates are useful when you need a fixed URL independent of the controller naming convention, such as a legacy report path or a marketing-friendly URL that must not change when the controller is renamed or reorganized.

---

## Q10. What is a catch-all route parameter (`{*slug}`)?

**Concepts**
- Greedy capture of remaining URL path including slashes as a single string
- Must be the last segment in the route template
- Use cases — nested virtual paths, documentation browsers, CMS content
- Limited constraint support compared to single-segment parameters

**Answer**

A catch-all parameter prefixed with `*` greedily captures the entire remainder of the URL path, including slash characters, as a single string value. `/docs/{*slug}` matches `/docs/a/b/c` with `slug = "a/b/c"`, which makes it ideal for documentation browsers or CMS pages where paths can be arbitrarily deep. The catch-all must be the last segment in the template because it consumes everything after the preceding literal. Route constraints on catch-all parameters are more limited compared to single-segment parameters, so complex validation needs to happen in the action method rather than in the route template.

---

## Q11. How do HTTP verbs (`[HttpGet]`, `[HttpPost]`) affect action selection?

**Concepts**
- HTTP method constraints narrowing action selection
- 405 Method Not Allowed when path matches but verb does not
- Multiple actions on the same route template differentiated by verb
- `[AcceptVerbs]` for listing multiple allowed methods on one action

**Answer**

HTTP method attributes constrain which requests can select an action during endpoint matching. `[HttpGet]` only accepts GET; `[HttpPost]` only accepts POST; when a route path matches but no action accepts the incoming verb, the framework returns 405 Method Not Allowed rather than 404. This means two actions can legitimately share the same route template as long as they are differentiated by verb — the standard REST resource pattern. An action without any verb attribute accepts any HTTP method, which is why annotating actions explicitly is good practice. `[AcceptVerbs("GET", "HEAD")]` lists multiple methods when a single attribute per action is not sufficient.

---

## Q12. What happens when two actions match the same route?

**Concepts**
- `AmbiguousActionException` thrown when multiple actions are equally eligible
- Distinct HTTP methods as the valid disambiguation mechanism
- Route template or constraint differences required for same-verb actions
- Action naming alone insufficient for disambiguation

**Answer**

When multiple actions are equally eligible for the same HTTP method and route template, ASP.NET Core throws `AmbiguousActionException` because the framework requires unambiguous endpoint selection. Two `[HttpGet]` actions on the same path is invalid, but `[HttpPost]` and `[HttpGet]` on the same path is valid because the verb differentiates them. Action naming alone does not disambiguate — if both actions match the incoming URL and method equally, the framework cannot choose. The fix is to add distinct route templates, HTTP method constraints, or route constraints that make one action more specific than the other.

---

## Q13. How does Tag Helper link generation (`asp-controller`, `asp-action`) relate to routing?

**Concepts**
- `IUrlHelper` consuming registered endpoint data for URL generation
- Ambient route values from the current request filling omitted parameters
- `asp-route-*` attributes supplying additional route values
- Missing or incorrect route values producing 404 links

**Answer**

Anchor and Form Tag Helpers call into `IUrlHelper` with the registered endpoint data, ambient route values from the current request, and any `asp-route-*` attributes you supply. Generated URLs automatically reflect the route table, so if a controller route changes, the Tag Helper picks it up without hardcoding paths. Ambient values from the current request fill in omitted `asp-controller` and `asp-action` parameters, which is convenient but can be surprising when the ambient context carries area or route values you did not intend. Incorrect or missing route values silently produce URLs that 404 when clicked because no endpoint in the route table matches.

---

## Q14. Why must `area` be specified when generating links to area controllers from outside the area?

**Concepts**
- Absent `area` ambient value in non-area requests
- Explicit `asp-area` required for root-to-area link generation
- Ambient area flowing within the same area context
- `asp-area=""` to escape an area and target root controllers

**Answer**

URL generation uses ambient route values from the executing request. A view outside any area has no ambient `area` value, so `asp-controller="Dashboard"` targets a root controller rather than `Areas/Admin/Controllers/DashboardController`. Explicit `asp-area="Admin"` is required to set the area route value for correct path generation. The reverse is also true: from within an area, the ambient area flows to links unless you override it with `asp-area=""` to clear it and target a root controller. Missing `asp-area` is one of the most common causes of 404 links from main site navigation to admin modules.

---

## Q15. What is `LowercaseUrls` and how does it affect link generation?

**Concepts**
- `RouteOptions.LowercaseUrls` normalizing generated URL output
- Applied to controller, action, and route parameter names in link output
- SEO consistency and Linux deployment path-casing sensitivity
- Tag Helper and `Url.Action` honoring the setting automatically

**Answer**

`LowercaseUrls` is a `RouteOptions` setting that normalizes generated URLs to lowercase, so `/Products/Details` becomes `/products/details` in all Tag Helper and `Url.Action` output. Incoming request matching remains case-insensitive by default, but on Linux-hosted deployments proxies or CDNs can be case-sensitive, meaning uppercase-generated links produce 404s behind them. The setting applies automatically to `asp-controller`, `asp-action`, and route parameter names without manual work. I would pair it with `LowercaseQueryStrings = true` for full consistency on SEO-important routes.

---

## Q16. What is the difference between "no route matched" and "405 Method Not Allowed"?

**Concepts**
- 404 "no route matched" — URL path does not fit any endpoint template
- 405 Method Not Allowed — path matched but HTTP verb rejected
- `Allow` header listing permitted methods on a 405 response
- Debugging distinction — 404 fixes templates, 405 fixes verb attributes

**Answer**

These outcomes differ in what matched. A 404 "no route matched" means no endpoint template fit the URL path at all — the URL shape is wrong, a required segment is missing, or a constraint failed. A 405 Method Not Allowed means the path matched a route but no action accepts the request's HTTP method, such as a GET request to an action decorated only with `[HttpPost]`. A 405 response should include an `Allow` header listing permitted methods. This distinction matters for debugging: a 404 means I should inspect route templates, while a 405 means I need to fix HTTP method attributes or the form method used by the client.

---

## Q17. How do optional route parameters (`{id?}`) and defaults interact?

**Concepts**
- `?` marking a segment as optional in the route template
- Inline defaults `{action=Index}` vs `defaults:` anonymous object in `MapControllerRoute`
- Route table defaults distinct from C# optional method parameters
- Link generation omitting optional segments when values equal defaults

**Answer**

The `?` suffix marks a parameter optional so the template matches URLs both with and without that segment. Inline defaults like `{action=List}` or a `defaults:` anonymous object in `MapControllerRoute` supply values when segments are omitted — so `/catalog` with pattern `catalog/{action=List}/{id?}` yields `action = List` and `id = null`. These route table defaults are distinct from C# optional parameters in the method signature, and confusing them produces subtle bugs. Link generation omits optional segments when supplied values equal the configured defaults, producing shorter canonical URLs automatically.

---

## Q18. What is the areas route pattern and how does it differ from the default route?

**Concepts**
- Standard areas pattern `{area:exists}/{controller=Home}/{action=Index}/{id?}`
- `[Area("AreaName")]` requirement on area controllers
- Area-specific view location under `Areas/{AreaName}/Views`
- Registration order — areas route before default route

**Answer**

The standard areas pattern is `{area:exists}/{controller=Home}/{action=Index}/{id?}`, registered with `MapControllerRoute` and requiring `[Area("AreaName")]` on area controllers. It inserts an area segment before controller and action, routing to `Areas/{AreaName}/Controllers` and resolving views from `Areas/{AreaName}/Views` rather than the root folders. The default route `{controller=Home}/{action=Index}/{id?}` has no area segment and serves root controllers. Because both patterns share a similar generic structure, the area route must be registered before the default so the `{area:exists}` constraint can consume the leading segment correctly before the default route claims it as the controller name.

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

Placing pricing, discount, authorization, or business rules in `.cshtml` files bypasses unit tests, duplicates service-layer logic, and makes behavior hard to change consistently. Views should render only what the controller or ViewModel already prepared, since Razor calculations cannot be tested independently and often diverge from API or batch logic running the same rule. Authorization belongs in filters, policies, or controller checks executed before the view — not in view conditionals that a developer can accidentally omit.

---

#### Gotcha 2. EF entities passed directly to views

**Concepts**
- Over-posting via direct entity binding on POST
- Lazy-loaded navigation properties triggering unexpected queries during rendering
- ViewModel as the narrow data contract between controller and view
- Entity-to-ViewModel mapping responsibility

**Answer**

Binding and displaying EF Core entities exposes navigation properties, enables over-posting on POST, and couples the UI to the database schema. Lazy-loaded navigations can trigger unexpected queries during Razor rendering — each navigation access issues a database round-trip the developer may not anticipate. Mass assignment on POST can update properties the user should never control, such as `IsAdmin`. The correct pattern is a dedicated ViewModel with only the fields the view needs, mapped from the entity in the controller or a mapping service.

---

#### Gotcha 3. `[FromBody]` on HTML form POST

**Concepts**
- Browser form encoding — `application/x-www-form-urlencoded` vs JSON
- `[FromBody]` routing to the JSON input formatter only
- Silent binding failure — model parameter receives default values
- `FormData` following the form value provider, not the JSON formatter

**Answer**

Standard browser forms send `application/x-www-form-urlencoded` or `multipart/form-data`, not JSON. `[FromBody]` tells MVC to use the JSON input formatter, which expects `Content-Type: application/json` — when a form POST arrives, the formatter finds no matching content and the model parameter receives default values while the action runs silently. Remove `[FromBody]` for conventional form POSTs and let the form value provider bind fields. Use `[FromBody]` only when the client explicitly sends JSON with the correct Content-Type header.

---

#### Gotcha 4. Skipping `ModelState.IsValid` because of client validation

**Concepts**
- Client validation as a UX convenience, not a security boundary
- Server-side validation mandatory before any persist, redirect, or side effect
- Direct POST attacks bypassing browser scripts entirely

**Answer**

Client-side validation is bypassable — attackers can POST directly using tools like Postman or curl without running browser validation scripts. Server-side `ModelState.IsValid` is mandatory before any persist, redirect, or side effect. Always gate POST actions with `if (!ModelState.IsValid) return View(model);` or equivalent. Client validation improves UX for legitimate users only, and treating missing server validation as a security defect regardless of client script presence is the right standard.

---

#### Gotcha 5. `return View()` after successful POST

**Concepts**
- Duplicate form submission triggered by browser refresh after POST
- Post-Redirect-Get (PRG) pattern — mutation then safe redirect
- `RedirectToAction` separating command (POST) from display (GET)
- TempData carrying flash messages across the redirect

**Answer**

Returning the same view after a successful POST means the browser's last request was the POST. When the user refreshes, the browser re-submits the POST body, which can duplicate an order, a payment, or a registration. The fix is Post-Redirect-Get: return `RedirectToAction(nameof(Index))` after a successful create or update so the browser's last request is a safe GET. TempData carries flash success messages across the redirect, and the GET action loads fresh data from services rather than relying on state passed from the POST.

---

#### Gotcha 6. `ModelState` after redirect

**Concepts**
- `ModelState` as request-scoped data lost on redirect
- Return `View(model)` on validation failure to preserve errors inline
- TempData serialization as a fallback for post-redirect error persistence
- AJAX partial forms avoiding the redirect problem entirely

**Answer**

`ModelState` lives in the controller's `ViewDataDictionary` for the current request only — a redirect ends that request and starts a new one with an empty `ModelState`, so validation errors disappear. The standard pattern is redirect only on success and return `View(model)` on validation failure in the same POST response so errors remain visible. If a redirect on failure is truly required, serialize errors to TempData (watching cookie size limits) or run a second validation pass on the GET action. AJAX partial forms avoid the problem by returning the form partial with inline errors without any redirect.

---

#### Gotcha 7. TempData read twice in layout and view

**Concepts**
- TempData consume-on-read default semantics
- Layout consuming flash key before the child view reads it
- `Peek()` — read without marking for deletion in the same request
- `Keep()` — preserve a consumed key for the next request

**Answer**

TempData marks entries for deletion the moment they are read via the indexer. If the layout reads a flash message first, the child view's subsequent read of the same key returns null — the message disappears intermittently depending on render order. The fix is to use `TempData.Peek("Message")` in the layout, which reads the value without consuming it and leaves it available for the child view. Alternatively, centralizing flash display in a single `_FlashMessages.cshtml` partial avoids the double-read problem by giving ownership of all TempData keys to one place.

---

#### Gotcha 8. Missing `[Area]` attribute on area controllers

**Concepts**
- `[Area("AreaName")]` as required routing metadata on area controllers
- Controller in `Areas/` folder without attribute treated as a root controller
- `{area:exists}` constraint not matching unannotated controllers
- Compile-time success masking a runtime 404

**Answer**

A controller physically located in `Areas/Admin/Controllers/` is not automatically registered with the area route — it needs `[Area("Admin")]` on the class. Without it, MVC treats the controller as a root controller so the `{area:exists}` route template does not match it and requests to `/Admin/Dashboard` return 404. The project compiles without the attribute because it is optional at compile time, giving false confidence until the first HTTP request hits the area URL. Every area controller must declare `[Area("AreaName")]` matching its folder, and the area route must be registered before the default route in `Program.cs`.

---

#### Gotcha 9. Link generation without `asp-area`

**Concepts**
- Ambient area route values from the current request
- Absent area context in root views producing wrong URLs
- Explicit `asp-area` required for cross-area and root-to-area links
- `Url.Action` requiring area route values in the anonymous object

**Answer**

Tag Helpers inherit ambient route values from the current request, so from within an Admin area view, `asp-controller="Users"` may generate `/Admin/Users` correctly because the ambient area is `Admin`. However, from a root view or a different area, the same Tag Helper generates `/Users` with no area prefix because there is no ambient area value. Cross-area links require explicit `asp-area="Admin"` on every anchor that targets an area controller. The same rule applies to `Url.Action` — pass `new { area = "Admin" }` in the route values object or the URL will be missing the area prefix.

---

#### Gotcha 10. Checkbox `[Required]` on non-nullable `bool`

**Concepts**
- Unchecked checkbox posting no value — binding sets non-nullable `bool` to `false`
- `[Required]` passing validation because `false` is a valid non-null value
- `bool?` with `[Required]` requiring an explicit `true` for consent scenarios
- Hidden-field pattern for deliberate `false` submission

**Answer**

An unchecked checkbox posts nothing — no form field at all — so model binding sets a non-nullable `bool` to its default `false`. `[Required]` then passes validation because `false` is a valid non-null value, meaning a user can submit a consent checkbox unchecked and the server accepts it. For explicit consent requirements, use `bool?` with `[Required]` since a null value (no field posted) fails `[Required]` while `true` (checked) passes. The hidden-field pattern — a hidden input with value `false` plus a checkbox with value `true` — ensures the form always posts a value so unchecked deliberately sends `false`.

---

#### Gotcha 11. Collection binding with gap indices

**Concepts**
- Contiguous-index requirement for MVC form collection binding
- Gap indices causing silent truncation or misalignment of bound items
- Client-side reindexing after row deletion
- Custom `IModelBinder` for non-contiguous index tolerance

**Answer**

MVC's collection binder expects form field names like `Lines[0].Name`, `Lines[1].Name` in contiguous order starting at zero. When a user deletes a middle row from a dynamic form, the remaining indices become `Lines[0]` and `Lines[2]`, with index 1 missing. The binder stops at the first gap so subsequent items are silently dropped or bound incorrectly. The fix is to reindex client-side after every row deletion so indices are always contiguous. A custom `IModelBinder` can tolerate non-contiguous indices for complex scenarios, but client-side reindexing is simpler and easier to test.

---

#### Gotcha 12. `@Html.Raw` with user content

**Concepts**
- Razor default `@` encoding preventing XSS
- `Html.Raw` bypassing encoding for attacker-supplied strings
- AJAX partial HTML injection via `innerHTML` as an XSS surface
- Content-Security-Policy as defense in depth, not a substitute for encoding

**Answer**

Razor's default `@` encoding prevents XSS by HTML-encoding output so attacker-supplied angle brackets and script tags render as visible text. `@Html.Raw(Model.UserComment)` bypasses that protection, rendering whatever string the user submitted directly into the HTML — including `<script>` tags and event handlers. AJAX-loaded partials injected via `innerHTML` carry the same risk: any script in the injected HTML executes in the victim's session. Use `@Model.UserComment` for auto-encoded output, or sanitize with a trusted HTML sanitizer library if preserving some HTML formatting is genuinely required.

---

#### Gotcha 13. AJAX POST without antiforgery token

**Concepts**
- Antiforgery cookie-and-field/header pair preventing CSRF
- Form Tag Helpers emitting the hidden token field automatically
- Manual `RequestVerificationToken` header required for `fetch` and jQuery AJAX
- `[AutoValidateAntiforgeryToken]` covering all unsafe methods on a controller

**Answer**

Form Tag Helpers emit the `__RequestVerificationToken` hidden field automatically, but `fetch` and jQuery AJAX calls must include the token manually — as the `RequestVerificationToken` header or as the form field in the POST body. Without it, `[ValidateAntiForgeryToken]` or `[AutoValidateAntiforgeryToken]` returns 400 Bad Request before the action executes. The fix is to read the hidden field value from the page and include it on every mutating AJAX request. Disabling antiforgery on MVC cookie-auth endpoints to work around the 400 is not acceptable — CSRF protection exists precisely because those endpoints are vulnerable.

---

#### Gotcha 14. Injecting Hub into MVC controller

**Concepts**
- Hub — per-connection transient lifecycle, not registered in DI for direct injection
- `IHubContext<THub>` — singleton proxy for server-side broadcasting
- Hub instance lacking connection context when activated outside SignalR
- Redis backplane or Azure SignalR for cross-instance message fan-out

**Answer**

Hubs are not registered in DI for direct injection into controllers — injecting a concrete `Hub` type either fails activation or produces an instance without a valid connection context, since hubs are per-connection objects managed by SignalR's infrastructure. The correct mechanism for broadcasting from a controller or service is `IHubContext<THub>`, a singleton proxy registered by `AddSignalR()` that routes messages through SignalR. For multi-instance deployments, pair it with a Redis backplane or Azure SignalR Service so messages reach clients on all pods, not just the current process.

---

#### Gotcha 15. SignalR scale-out without backplane

**Concepts**
- In-memory connection registry local to each pod
- Sticky sessions routing connections but not cross-instance messages
- Redis backplane and Azure SignalR Service for full fan-out
- Group membership and connection IDs scoped per process instance

**Answer**

Each ASP.NET Core process maintains its own in-memory registry of connections, groups, and user mappings. Sticky sessions route the same client to the same pod for their WebSocket lifetime, but when a controller on instance A calls `IHubContext.Clients.User(id).SendAsync`, that call only reaches users connected to instance A — users on instance B miss the message. The fix is a Redis backplane (`AddStackExchangeRedis`) or Azure SignalR Service, which routes events across all instances so any process can reach any connected client. Sticky sessions are still useful to avoid connection migration overhead but are not a substitute for a cross-instance backplane.

---

## Scenario-Based Questions (Karat Format)

---

#### Q1. (R) Review this `ProductsController` and `Program.cs`. `GET /products/sale` shows a product named "sale" instead of the sale landing page.

```csharp
// Program.cs
app.MapControllerRoute(
    name: "productSlug",
    pattern: "products/{id}",
    defaults: new { controller = "Products", action = "Details" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

public class ProductsController : Controller
{
    public IActionResult Index() => View(_repo.All());

    public IActionResult Details(string id) => View(_repo.BySlug(id));

    public IActionResult Sale() => View(_repo.SaleItems());
}
```

Marketing expects `/products/sale` to hit `Sale()`; QA reports it returns a product whose slug is `"sale"`.

**Concepts**
- Route registration order — first-match wins
- Slug route `products/{id}` consuming the literal segment `sale` as a parameter value
- Attribute route specificity — literal segments ranked over bare parameters
- Conventional routing URL collision between slug pattern and named action

**Answer**

The issue is route registration order: the dedicated `products/{id}` route is registered before the default route, so `/products/sale` matches the slug pattern with `id = "sale"` and calls `Details("sale")` rather than the `Sale()` action. The conventional `{controller}/{action}` template would reach `Sale()` via `/products/sale`, but the slug route intercepts it first since routes are evaluated in registration order.

The cleanest fix is to switch to attribute routing and declare `[HttpGet("sale")]` on `Sale` and `[HttpGet("products/{slug}")]` on `Details` — attribute routing ranks literal segments over bare parameters, so `/products/sale` binds to the literal `Sale` action rather than treating `sale` as a slug value. For conventional-only apps, register a dedicated named route that maps the literal path `products/sale` to the `Sale` action, placing it before the generic slug route. Either way, add integration tests for `/products/sale`, `/products/details/sale`, and related permutations to catch regressions when routes change.

---

#### Q2. (M) A team registers two conventional routes for an MVC storefront. Explain how optional `{id?}` and default values interact when a user visits `/catalog`, `/catalog/featured`, and `/catalog/list/42`.

```csharp
app.MapControllerRoute(
    name: "catalog",
    pattern: "catalog/{action=List}/{id?}",
    defaults: new { controller = "Catalog" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Which controller action handles each URL, and what happens if `List` is renamed but the route pattern is not updated?

**Concepts**
- Inline route defaults supplying action name when segment is omitted
- Optional `{id?}` matching URLs with or without the third segment
- Route pattern hard-coding action name — rename without update causes 404
- Catalog route registered before default route — first-match priority

**Answer**

_Answer not found._

---

#### Q3. (R) Review this route registration and action. `GET /orders/not-a-number` returns 500 with `FormatException` in logs instead of a client-friendly not-found.

```csharp
app.MapControllerRoute(
    name: "order",
    pattern: "orders/{id:int}",
    defaults: new { controller = "Orders", action = "Details" });

public class OrdersController : Controller
{
    public IActionResult Details(int id) => View(_orders.Get(id));
}
```

No attribute routes exist on `OrdersController`.

**Concepts**
- `:int` constraint — route should not match a non-integer segment
- Fallthrough to the next route when a constraint fails
- 404 expected from constraint failure, not 500 from `FormatException`
- Second unconstrained route likely catching the request after fallthrough

**Answer**

_Answer not found._

---

#### Q4. (R) Review `Program.cs` route order. After deploy, every unknown URL renders the home page with HTTP 200 instead of 404, and `/admin/users` never reaches `AdminController`.

```csharp
app.MapControllerRoute(
    name: "fallback",
    pattern: "{*path}",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Concepts**
- Catch-all `{*path}` registered first consuming every incoming request
- Route registration order — fallback must be the last entry
- Admin and default routes unreachable behind the catch-all
- 404 suppression — catch-all masks "no route matched" for all URLs

**Answer**

_Answer not found._

---

#### Q5. (R) Review this shared `_Layout.cshtml` link and area setup. Clicking **Manage Users** from the Shop area returns 404, but the same link works from the root site.

```html
<!-- _Layout.cshtml (shared by root and Shop area views) -->
<a asp-controller="Users" asp-action="Index">Manage Users</a>
```

```csharp
// Program.cs
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

[Area("Admin")]
public class UsersController : Controller
{
    public IActionResult Index() => View();
}
```

Current request when bug occurs: `/shop/products` (Shop area).

**Concepts**
- Ambient area route value carrying the current area into link generation
- `asp-area` absent — Tag Helper inheriting ambient `shop` area
- Generated URL `/shop/users` instead of intended `/admin/users`
- Explicit `asp-area="Admin"` required for cross-area navigation

**Answer**

_Answer not found._

---

#### Q6. (M) Compare attribute routing applied at the **controller** vs **action** level. Given the snippets below, what are the final URLs for `Monthly` and `Export`, and which tokens does each action inherit?

```csharp
[Route("reports/[controller]")]
public class ReportsController : Controller
{
    [HttpGet("monthly")]
    public IActionResult Monthly() => View();

    [HttpGet("/export/{year:int}")]
    public IActionResult Export(int year) => File(_svc.Export(year), "text/csv");
}
```

When would you put `[Route]` on the controller vs only on actions in a mixed MVC + API surface?

**Concepts**
- Controller-level `[Route]` prefix inherited by relative action templates
- Root-absolute action template ignoring the controller prefix
- `[controller]` token substituted with the controller name at startup
- Mixed MVC and API surface routing strategy

**Answer**

_Answer not found._

---

#### Q7. (R) Review this documentation site controller. `GET /docs/getting-started/install` returns 404, but `GET /docs/getting-started` works. Static files middleware is registered before routing.

```csharp
[Route("docs")]
public class DocsController : Controller
{
    [HttpGet("{*slug}")]
    public IActionResult Page(string slug)
    {
        var content = _store.Find(slug);
        return content is null ? NotFound() : View("Doc", content);
    }
}
```

**Concepts**
- Catch-all `{*slug}` capturing the full remainder of the path
- Static files middleware short-circuiting before routing for certain paths
- Path segments containing dots potentially matching static file patterns
- Missing leading slash on `{*slug}` vs correct `docs/{*slug}` template

**Answer**

_Answer not found._

---

#### Q8. (M) For the same conventional route `{controller=Home}/{action=Index}/{id?}`, contrast the HTTP status and routing outcome for:

1. `GET /home/index` when `Index` exists and allows GET  
2. `POST /home/index` when only GET is implemented (no `[HttpPost]` on `Index`)  
3. `GET /homme/index` (typo in controller name)

Explain how endpoint routing differs from "no route matched" vs "route matched, method not allowed."

**Concepts**
- 200 OK from exact route and method match
- 405 Method Not Allowed — route matched the path, verb rejected
- 404 Not Found — typo controller segment matches no registered controller
- Endpoint routing distinguishing path-match from method-match at selection time

**Answer**

_Answer not found._

---

#### Q9. (P) Production enables lowercase URLs for SEO. After enabling the option below, organic links work but several redirects and `Url.Action` calls still emit PascalCase paths that 404 on Linux containers.

```csharp
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Somewhere in AccountController:
return Redirect("/Account/Login?returnUrl=/Products/Details/5");
```

What must change in route registration, link generation, and hard-coded redirects for consistent behavior cross-platform?

**Concepts**
- `LowercaseUrls` applying only to Tag Helper and `Url.Action` generated output
- Hard-coded `Redirect("/Account/Login")` bypassing the routing option
- Linux case-sensitive path matching behind reverse proxies
- `Url.Action` respecting `LowercaseUrls` — hard-coded strings do not

**Answer**

_Answer not found._

---

#### Q10. (D) A legacy ASP.NET Core 2.2 MVC app is upgraded to .NET 8. The old `Startup.cs` still calls `UseMvcWithDefaultRoute()` while newer code uses `MapControllerRoute` inside `UseEndpoints`. Reviewers ask whether to keep both for compatibility.

```csharp
// Old (commented in places, still active in one branch)
app.UseMvcWithDefaultRoute();

// New
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
```

What breaks if both run, what is the modern replacement, and how does endpoint routing change action selection compared to legacy IRouter middleware?

**Concepts**
- `UseMvcWithDefaultRoute` — legacy IRouter-based middleware pipeline
- `MapControllerRoute` — endpoint routing in ASP.NET Core 3+
- Running both causing double route registration and 500 errors
- Endpoint routing vs IRouter — action selection timing and middleware access differences

**Answer**

_Answer not found._

---

#### Q11. (R) Review attribute + conventional mixing on one controller. Integration tests expect `GET /api/reports/summary` but receive 404; `GET /Reports/Summary` works. Tag Helper links in Razor views point at `/Reports/Summary`.

```csharp
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

[Route("api/[controller]")]
public class ReportsController : Controller
{
    [HttpGet("summary")]
    public IActionResult Summary() => Json(_svc.Summary());

    public IActionResult Index() => View();
}
```

```html
<a asp-controller="Reports" asp-action="Summary">Summary API</a>
```

**Concepts**
- Attribute route on `Summary` — `/api/reports/summary` is the endpoint
- Conventional route on `Index` — reachable via `/Reports/Index`
- Mixed routing on one controller — attribute-routed actions not reachable via conventional route
- Tag Helper generating conventional URL for an attribute-routed action

**Answer**

_Answer not found._
