# Authentication & Authorization Fundamentals — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is the difference between authentication and authorization, and which must happen first?](#q1-what-is-the-difference-between-authentication-and-authorization-and-which-must-happen-first)
2. [Q2. What does the `UseAuthentication` middleware do, and why must it be registered before `UseAuthorization`?](#q2-what-does-the-useauthentication-middleware-do-and-why-must-it-be-registered-before-useauthorization)
3. [Q3. What is an authentication scheme in ASP.NET Core, and how is one registered?](#q3-what-is-an-authentication-scheme-in-aspnet-core-and-how-is-one-registered)
4. [Q4. What is the role of an authentication handler, and what interface must it implement?](#q4-what-is-the-role-of-an-authentication-handler-and-what-interface-must-it-implement)
5. [Q5. What does `IAuthenticationService` do, and how does it relate to registered authentication handlers?](#q5-what-does-iauthenticationservice-do-and-how-does-it-relate-to-registered-authentication-handlers)
6. [Q6. What is `AuthenticateResult`, and what are the possible outcomes it represents?](#q6-what-is-authenticateresult-and-what-are-the-possible-outcomes-it-represents)
7. [Q7. What is a `ClaimsPrincipal`, and how is it related to `ClaimsIdentity` and `Claim`?](#q7-what-is-a-claimsprincipal-and-how-is-it-related-to-claimsidentity-and-claim)
8. [Q8. How is `HttpContext.User` populated during an HTTP request?](#q8-how-is-httpcontextuser-populated-during-an-http-request)
9. [Q9. What is a default authentication scheme, and why does it matter when no scheme is explicitly named?](#q9-what-is-a-default-authentication-scheme-and-why-does-it-matter-when-no-scheme-is-explicitly-named)
10. [Q10. How do you configure multiple authentication schemes, and how does ASP.NET Core decide which one to invoke?](#q10-how-do-you-configure-multiple-authentication-schemes-and-how-does-aspnet-core-decide-which-one-to-invoke)
11. [Q11. What is the difference between a Challenge response and a Forbid response in ASP.NET Core?](#q11-what-is-the-difference-between-a-challenge-response-and-a-forbid-response-in-aspnet-core)
12. [Q12. What do `SignIn` and `SignOut` operations do inside the authentication pipeline?](#q12-what-do-signin-and-signout-operations-do-inside-the-authentication-pipeline)
13. [Q13. What does the `[Authorize]` attribute do at its simplest, and what happens when the authorization check fails?](#q13-what-does-the-authorize-attribute-do-at-its-simplest-and-what-happens-when-the-authorization-check-fails)
14. [Q14. How does role-based authorization work in ASP.NET Core, and what are its limitations?](#q14-how-does-role-based-authorization-work-in-aspnet-core-and-what-are-its-limitations)
15. [Q15. What is policy-based authorization, and why is it preferred over role-based authorization for complex rules?](#q15-what-is-policy-based-authorization-and-why-is-it-preferred-over-role-based-authorization-for-complex-rules)
16. [Q16. What is `IAuthorizationRequirement`, and what is the relationship between a requirement and its handler?](#q16-what-is-iauthorizationrequirement-and-what-is-the-relationship-between-a-requirement-and-its-handler)
17. [Q17. What is `IAuthorizationHandler`, and how does it signal success or failure when evaluating a requirement?](#q17-what-is-iauthorizationhandler-and-how-does-it-signal-success-or-failure-when-evaluating-a-requirement)
18. [Q18. What is `IAuthorizationService`, and when would you use it programmatically instead of the `[Authorize]` attribute?](#q18-what-is-iauthorizationservice-and-when-would-you-use-it-programmatically-instead-of-the-authorize-attribute)
19. [Q19. What is resource-based authorization, and how does it differ from standard attribute-driven policy authorization?](#q19-what-is-resource-based-authorization-and-how-does-it-differ-from-standard-attribute-driven-policy-authorization)
20. [Q20. What does `[AllowAnonymous]` do, and can it override an `[Authorize]` attribute placed on a parent controller?](#q20-what-does-allowanonymous-do-and-can-it-override-an-authorize-attribute-placed-on-a-parent-controller)
21. [Q21. What is a claims-based identity, and how do claims drive authorization decisions in ASP.NET Core?](#q21-what-is-a-claims-based-identity-and-how-do-claims-drive-authorization-decisions-in-aspnet-core)
22. [Q22. How do you implement a custom authentication handler in ASP.NET Core?](#q22-how-do-you-implement-a-custom-authentication-handler-in-aspnet-core)
23. [Q23. What is the difference between `ClaimsIdentity.IsAuthenticated` returning `true` versus `false`?](#q23-what-is-the-difference-between-claimsidentityisauthenticated-returning-true-versus-false)
24. [Q24. What is `AuthenticationProperties`, and what kind of data does it carry?](#q24-what-is-authenticationproperties-and-what-kind-of-data-does-it-carry)
25. [Q25. How does the authorization middleware interact with endpoint metadata in the endpoint routing model?](#q25-how-does-the-authorization-middleware-interact-with-endpoint-metadata-in-the-endpoint-routing-model)
26. [Q26. What is the difference between calling `RequireAuthorization()` on an endpoint and applying the `[Authorize]` attribute?](#q26-what-is-the-difference-between-calling-requireauthorization-on-an-endpoint-and-applying-the-authorize-attribute)
27. [Q27. What are authentication scheme forwarding and passthrough, and when would you use them?](#q27-what-are-authentication-scheme-forwarding-and-passthrough-and-when-would-you-use-them)
28. [Q28. What are the key tradeoffs between cookie-based authentication and JSON Web Token (JWT) bearer authentication in ASP.NET Core?](#q28-what-are-the-key-tradeoffs-between-cookie-based-authentication-and-json-web-token-jwt-bearer-authentication-in-aspnet-core)

---

## Q1. What is the difference between authentication and authorization, and which must happen first?

**Concepts**
- Authentication establishing caller identity
- Authorization deciding what the identity may do
- ClaimsPrincipal produced as the output of authentication
- Ordering requirement — authentication must precede authorization

**Answer**

Authentication is the process of establishing who the caller is — it produces an identity. Authorization is the process of deciding what that established identity is allowed to do. Authentication must always happen first because authorization decisions are meaningless without a known identity to evaluate. Authentication answers "Who are you?" and on success produces a `ClaimsPrincipal` with one or more attached claims. Authorization answers "Are you allowed to do this?" and consumes that principal to evaluate policies, roles, or custom requirements. Reversing the order — or skipping authentication — means the authorization layer receives an unauthenticated principal, which causes protected resources to be rejected or, worse, accidentally allowed.

---

## Q2. What does the `UseAuthentication` middleware do, and why must it be registered before `UseAuthorization`?

**Concepts**
- UseAuthentication populating HttpContext.User via default scheme
- IAuthenticationService.AuthenticateAsync invocation order
- Middleware pipeline ordering requirement
- Anonymous principal as starting state before authentication runs

**Answer**

`UseAuthentication` runs the default authentication scheme's handler on every incoming request, populating `HttpContext.User` with a `ClaimsPrincipal` if the request carries valid credentials. `UseAuthorization` then reads that already-populated principal to enforce policies — if the order is reversed, `HttpContext.User` is still the default anonymous identity when authorization runs, so all protected endpoints appear to receive an unauthenticated user and every `[Authorize]` check fails. `UseAuthorization` must also appear after `UseRouting`, since it needs the matched endpoint's authorization metadata — `[Authorize]` attributes — to know which policies to enforce. A common symptom of reversed middleware order is that valid tokens or cookies are silently ignored and every request is treated as anonymous.

---

## Q3. What is an authentication scheme in ASP.NET Core, and how is one registered?

**Concepts**
- Named scheme pairing display name with handler and options
- IAuthenticationSchemeProvider as the scheme registry
- Add* extension methods inside AddAuthentication
- AuthenticationSchemes property on [Authorize] referencing scheme names

**Answer**

An authentication scheme is a named configuration that pairs a display name with a specific authentication handler and its options. Schemes give you a stable, referenceable name — such as `"Bearer"` or `"Cookies"` — that the rest of the framework uses to invoke the correct handler without hard-coding handler type names. I register a scheme by calling one of the `Add*` extension methods inside `builder.Services.AddAuthentication(...)`, for example `AddCookie("Cookies", options => { ... })` or `AddJwtBearer("Bearer", options => { ... })`. Each registration stores an entry in `IAuthenticationSchemeProvider`, which is resolved at runtime to find the right handler. The string name is what I reference in `[Authorize(AuthenticationSchemes = "Bearer")]` or in `HttpContext.AuthenticateAsync("Bearer")`. A single application can have several schemes registered simultaneously, each with independent options.

---

## Q4. What is the role of an authentication handler, and what interface must it implement?

**Concepts**
- IAuthenticationHandler.HandleAuthenticateAsync returning AuthenticateResult
- ChallengeAsync for unauthenticated responses
- ForbidAsync for authenticated-but-unauthorized responses
- Per-request transient handler lifetime via DI

**Answer**

An authentication handler is responsible for executing the actual mechanics of one authentication scheme — parsing credentials from the request, validating them, and producing or rejecting an identity. Every handler must implement `IAuthenticationHandler`, and most concrete handlers extend the built-in base class `AuthenticationHandler<TOptions>`. The core method is `HandleAuthenticateAsync`, which returns an `AuthenticateResult` indicating success (with a ticket), no result (credential absent), or failure (credential invalid). Handlers that support challenges implement `ChallengeAsync`, which generates the appropriate "please authenticate" response — a redirect for cookies, or a `401 Unauthorized` with a `WWW-Authenticate` header for JWT Bearer. A handler is constructed fresh per-request by the DI container, giving it access to the current `HttpContext` automatically through the base class.

---

## Q5. What does `IAuthenticationService` do, and how does it relate to registered authentication handlers?

**Concepts**
- IAuthenticationService as facade for authenticate/challenge/forbid/sign-in/sign-out
- Scheme name to handler dispatch via IAuthenticationHandlerProvider
- UseAuthentication middleware delegating to IAuthenticationService
- Handler caching per request by AuthenticationService

**Answer**

`IAuthenticationService` is the central facade for all authentication operations — authenticate, challenge, forbid, sign-in, and sign-out. It is injected into middleware and framework components so they do not need to resolve handlers directly; instead they call the service with a scheme name, and the service locates and invokes the correct handler through `IAuthenticationHandlerProvider`. When `UseAuthentication` middleware runs, it calls `context.AuthenticateAsync()`, which resolves to `IAuthenticationService.AuthenticateAsync` with the default scheme name. `IAuthenticationService` is also called by the authorization middleware when a challenge or forbid response is needed. Developers can call it directly from controllers using `HttpContext.AuthenticateAsync`, `HttpContext.SignInAsync`, and `HttpContext.SignOutAsync` extension methods. The default implementation caches resolved handlers within a single request to avoid repeated construction.

---

## Q6. What is `AuthenticateResult`, and what are the possible outcomes it represents?

**Concepts**
- AuthenticateResult.Success with AuthenticationTicket carrying ClaimsPrincipal
- AuthenticateResult.NoResult — credential absent, handler opts out
- AuthenticateResult.Fail — credential present but invalid
- NoResult vs Fail distinction for forwarding logic and error responses

**Answer**

`AuthenticateResult` is the object returned by `HandleAuthenticateAsync` on any authentication handler, encapsulating whether authentication succeeded, was skipped, or failed explicitly. `AuthenticateResult.Success(ticket)` means the handler found and validated a credential; the `AuthenticationTicket` inside carries the `ClaimsPrincipal` that will be assigned to `HttpContext.User`. `AuthenticateResult.NoResult()` means the handler found no credential it could process — no cookie or no `Authorization` header — so the handler opts out without producing an error, allowing other schemes to potentially handle the request. `AuthenticateResult.Fail(exception)` means the handler found a credential but it was invalid (expired token, tampered signature). The distinction between `NoResult` and `Fail` matters for forwarding logic: a `Fail` result typically leads to a 401 with an error description, while a `NoResult` may allow the request to continue as anonymous.

---

## Q7. What is a `ClaimsPrincipal`, and how is it related to `ClaimsIdentity` and `Claim`?

**Concepts**
- ClaimsPrincipal as top-level security object with multiple identities
- ClaimsIdentity as one authentication-produced identity with claims collection
- Claim as typed key-value string pair asserting a fact about the user
- AuthenticationType string determining IsAuthenticated

**Answer**

A `ClaimsPrincipal` is the top-level security object that represents a user — it can hold one or more `ClaimsIdentity` objects, each representing a separate identity the user has authenticated with. Each `ClaimsIdentity` contains a flat collection of `Claim` objects, where a claim is a typed key-value pair asserting a fact about the user, such as their name, email, or role. The three-level hierarchy exists because a single user can be authenticated by multiple schemes simultaneously — each scheme contributes one `ClaimsIdentity` to the same principal. `ClaimsPrincipal.Identity` returns the first primary `ClaimsIdentity`, and `IsAuthenticated` on that identity is `true` only when it has a non-empty `AuthenticationType` string. Claims carry values as strings, so a role stored as a claim looks like `new Claim(ClaimTypes.Role, "Admin")`.

---

## Q8. How is `HttpContext.User` populated during an HTTP request?

**Concepts**
- Anonymous ClaimsPrincipal as default at request start
- UseAuthentication replacing with authenticated principal on success
- Default scheme automatic invocation — non-default schemes on demand only
- Subsequent middleware trusting the populated User

**Answer**

`HttpContext.User` starts as an unauthenticated anonymous `ClaimsPrincipal` at the beginning of every request and is replaced by the authenticated principal when `UseAuthentication` middleware runs and the default scheme's handler succeeds. The `UseAuthentication` middleware calls `IAuthenticationService.AuthenticateAsync` with the configured default scheme name; if the result is `Success`, it assigns `result.Principal` to `HttpContext.User`. Some schemes — particularly cookie authentication — also refresh or re-issue their credential as part of this step, so the assignment may also write a new cookie to the response. When multiple schemes are configured, only the default scheme runs automatically during `UseAuthentication`; other schemes are invoked only when explicitly requested through `[Authorize(AuthenticationSchemes = "Bearer")]`. After `UseAuthentication` runs, all subsequent middleware and endpoint handlers can trust `HttpContext.User` as the best available authentication result for the request.

---

## Q9. What is a default authentication scheme, and why does it matter when no scheme is explicitly named?

**Concepts**
- Default scheme invoked when no explicit name is provided
- Five distinct default slots in AuthenticationOptions
- Single name filling all five slots when passed to AddAuthentication
- InvalidOperationException without default on unnamed operations

**Answer**

The default authentication scheme is the scheme ASP.NET Core uses automatically when no explicit scheme name is provided — when `UseAuthentication` runs, when `[Authorize]` has no `AuthenticationSchemes` property set, or when challenge/forbid is triggered without a named scheme. I set the default by passing its name to `AddAuthentication`: `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)`. ASP.NET Core actually has five distinct default slots — default authenticate, challenge, forbid, sign-in, and sign-out scheme. Passing a single name fills all five slots, but I can override individual slots by setting properties on `AuthenticationOptions` separately — for instance, using JWT Bearer as the default authenticate scheme but Cookie as the default sign-in scheme. If no default is configured and code tries to authenticate without naming a scheme, an `InvalidOperationException` is thrown, which is a common misconfiguration error in multi-scheme setups.

---

## Q10. How do you configure multiple authentication schemes, and how does ASP.NET Core decide which one to invoke?

**Concepts**
- Multiple Add* registrations with unique names
- [Authorize(AuthenticationSchemes)] explicit scheme override
- ForwardDefaultSelector for per-request dynamic scheme routing
- Combined principal from multiple successful schemes

**Answer**

Multiple schemes are registered by calling several `Add*` methods in sequence inside `AddAuthentication`, each with a unique name. The framework selects a scheme through a priority order: an explicit scheme name on the `[Authorize]` attribute wins first; otherwise the default scheme defined in `AuthenticationOptions` is used. For example, calling both `AddCookie("Cookies")` and `AddJwtBearer("Bearer")` registers two schemes, and `[Authorize(AuthenticationSchemes = "Cookies,Bearer")]` accepts either credential type for a given endpoint. Scheme forwarding via `ForwardDefaultSelector` allows per-request dynamic selection — `context => context.Request.Path.StartsWithSegments("/api") ? "Bearer" : "Cookies"` routes API paths to JWT Bearer and browser paths to cookie auth. When multiple scheme names are provided in `AuthenticationSchemes`, ASP.NET Core creates a combined principal that merges the identities from all successful schemes.

---

## Q11. What is the difference between a Challenge response and a Forbid response in ASP.NET Core?

**Concepts**
- Challenge — unauthenticated caller needs to prove identity
- Forbid — authenticated caller lacks required permission
- IsAuthenticated driving middleware choice between challenge and forbid
- Scheme-specific response customization per handler

**Answer**

A Challenge response tells the caller "you have not yet authenticated — please prove your identity," while a Forbid response tells the caller "you are already authenticated, but you do not have permission to do this." The authorization middleware automatically chooses between the two based on `HttpContext.User.Identity.IsAuthenticated`: if `false`, it challenges; if `true`, it forbids. Each scheme's handler produces a different HTTP response for each case:

| Situation | Method called | Cookie result | JWT Bearer result |
|---|---|---|---|
| No valid credential | `ChallengeAsync` | 302 redirect to login | 401 Unauthorized |
| Valid credential, insufficient permission | `ForbidAsync` | 302 redirect to access-denied | 403 Forbidden |

The cookie handler adds a `ReturnUrl` query parameter to the login redirect so the user lands back on the original page after sign-in. Developers can also trigger either response manually: `HttpContext.ChallengeAsync()` or `HttpContext.ForbidAsync()`, optionally passing a specific scheme name.

---

## Q12. What do `SignIn` and `SignOut` operations do inside the authentication pipeline?

**Concepts**
- SignIn persisting ClaimsPrincipal via IAuthenticationSignInHandler
- Cookie encryption and session persistence on SignIn
- SignOut deleting or revoking the persisted credential
- AuthenticationProperties metadata on SignIn — IsPersistent, ExpiresUtc

**Answer**

`SignIn` takes a `ClaimsPrincipal` and an `AuthenticationProperties` object and instructs the named scheme's handler to persist that identity — for example, by writing an encrypted authentication cookie. `SignOut` instructs the handler to delete or revoke that persisted credential, ending the authenticated session. `SignIn` is only supported by schemes that implement `IAuthenticationSignInHandler`; JWT Bearer is stateless and does not support sign-in — only cookie and similar stateful schemes do. Calling `HttpContext.SignInAsync("Cookies", principal, properties)` writes an encrypted cookie to the response; on subsequent requests the cookie handler decrypts it and reconstructs the `ClaimsPrincipal`. `AuthenticationProperties` passed to `SignIn` carries metadata such as `IsPersistent` (whether the cookie survives browser close), `ExpiresUtc` (explicit expiry time), and `RedirectUri` (where to send the user after signing in).

---

## Q13. What does the `[Authorize]` attribute do at its simplest, and what happens when the authorization check fails?

**Concepts**
- [Authorize] requiring IsAuthenticated = true as minimum check
- Class-level and action-level additive requirements
- Challenge vs Forbid delegation to scheme handler on failure
- Policy and Role narrowing on [Authorize]

**Answer**

At its simplest, `[Authorize]` with no parameters requires that `HttpContext.User.Identity.IsAuthenticated` is `true` — any successfully authenticated user is allowed, regardless of role or policy. When the check fails, the authorization middleware either challenges the caller if unauthenticated, or forbids them if authenticated but unauthorized. Applying `[Authorize]` to a controller class applies the requirement to every action method; an action-level `[Authorize]` is additive — both the class-level and action-level requirements must be satisfied. When authorization fails, the middleware does not short-circuit to a hardcoded 401/403; it delegates to the authentication scheme's `ChallengeAsync` or `ForbidAsync`, allowing each scheme to produce its own appropriate response. `[Authorize(Policy = "AdminOnly")]` narrows the check to a named policy; `[Authorize(Roles = "Admin")]` restricts to users with the Admin role claim.

---

## Q14. How does role-based authorization work in ASP.NET Core, and what are its limitations?

**Concepts**
- ClaimTypes.Role claim matching via User.IsInRole
- JWT role claim type mapping via RoleClaimType
- Role coarse-granularity limitation vs policy flexibility
- [Authorize(Roles = "Admin,Manager")] OR logic for multiple roles

**Answer**

Role-based authorization checks whether the authenticated user's `ClaimsPrincipal` contains a `ClaimTypes.Role` claim with a value matching the required role string. I declare it with `[Authorize(Roles = "Admin")]` or `[Authorize(Roles = "Admin,Manager")]` for OR logic across multiple roles. Under the covers, `User.IsInRole("Admin")` searches all `ClaimsIdentity` objects in the principal for a matching claim. Role names in JWT tokens often use a different claim type (`"roles"` or `"role"`) depending on the issuer, so I configure `TokenValidationParameters.RoleClaimType` to map the token's field to `ClaimTypes.Role`. The main limitation is poor granularity: roles are coarse-grained strings with no built-in support for conditions like "allowed only on resources the user owns" or "allowed only during business hours." Policy-based authorization supersedes roles for complex scenarios because policies encapsulate arbitrarily rich logic in `IAuthorizationRequirement` handlers.

---

## Q15. What is policy-based authorization, and why is it preferred over role-based authorization for complex rules?

**Concepts**
- Named policy combining one or more IAuthorizationRequirement objects
- AddAuthorization registration by name, [Authorize(Policy)] reference
- Unit-testable requirement handlers without HTTP coupling
- FallbackPolicy for secure-by-default endpoint protection

**Answer**

Policy-based authorization lets me define named rules — called policies — that combine one or more `IAuthorizationRequirement` objects; a request passes the policy only when all its requirements are satisfied. Policies are registered once in `AddAuthorization` and referenced by name in `[Authorize(Policy = "...")]`, which separates the definition of a rule from its declaration at endpoints. A policy can express rich conditions that roles cannot: minimum age from a date-of-birth claim, department membership from a custom claim, or time-of-day restrictions in handler logic. Requirements and their handlers are unit-testable in isolation because they have no dependency on HTTP infrastructure — they operate only on a `ClaimsPrincipal` and an optional resource object. Policy evaluation short-circuits: if any required handler calls `context.Fail()`, the policy fails immediately. The `AuthorizationOptions.FallbackPolicy` property lets me set a policy that applies to every endpoint without explicit authorization metadata, making it easy to secure by default.

---

## Q16. What is `IAuthorizationRequirement`, and what is the relationship between a requirement and its handler?

**Concepts**
- IAuthorizationRequirement as marker interface with no methods
- Separate handler class for evaluation logic
- Multiple handlers per requirement type
- context.Succeed vs context.Fail vs no-call semantics

**Answer**

`IAuthorizationRequirement` is a marker interface that I implement on a plain data class to represent one specific authorization condition, such as "minimum age of 18" or "must be in department X." The actual evaluation logic lives in a separate `IAuthorizationHandler` class that is registered in DI and matched to the requirement by type. This separation means a single requirement type can have multiple handlers — one handler checks a database claim, a fallback checks a local cache — and the authorization system runs all matching handlers. A requirement class typically holds parameters: `public class MinimumAgeRequirement(int minimumAge) : IAuthorizationRequirement`. Handlers are registered as `services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>()` and discovered automatically when evaluating a policy. A handler signals success by calling `context.Succeed(requirement)` and signals failure by calling `context.Fail()` or simply not calling either — a non-call is treated as no opinion rather than failure, unless `RequireAuthenticatedUser` is also active.

---

## Q17. What is `IAuthorizationHandler`, and how does it signal success or failure when evaluating a requirement?

**Concepts**
- HandleAsync receiving AuthorizationHandlerContext with principal and resource
- context.Succeed removing requirement from pending list
- context.Fail as hard override regardless of other handlers
- AuthorizationHandler<TRequirement> base class filtering by type

**Answer**

`IAuthorizationHandler` has a single method, `HandleAsync(AuthorizationHandlerContext)`, which receives the full context including the `ClaimsPrincipal`, all pending requirements, and an optional resource object. The handler inspects these and signals its verdict by calling methods on the context rather than returning a Boolean. `context.Succeed(requirement)` marks one specific requirement as satisfied and removes it from the pending list — when all requirements are removed from pending, the policy passes. `context.Fail()` marks the entire authorization attempt as failed regardless of other handlers' results — it is a hard override. Not calling either method leaves the requirement in the pending list; if it remains pending after all handlers run, the policy fails. The generic `AuthorizationHandler<TRequirement>` base class filters incoming calls to only those matching `TRequirement`, so each concrete handler focuses on a single requirement type.

---

## Q18. What is `IAuthorizationService`, and when would you use it programmatically instead of the `[Authorize]` attribute?

**Concepts**
- IAuthorizationService.AuthorizeAsync with resource and policy name
- Resource-based authorization inside action methods after loading entity
- Conditional rendering using per-item policy evaluation
- AuthorizationResult.IsSucceeded and failed requirements list

**Answer**

`IAuthorizationService` is the service that the authorization middleware ultimately calls to evaluate policies and requirements; it exposes `AuthorizeAsync(ClaimsPrincipal, object resource, string policyName)` for use anywhere in application code. I use it directly when I need to make authorization decisions inside a controller action, a service method, or a Razor Page handler rather than at the entry point. The primary use case is resource-based authorization: a controller loads a document from the database, then calls `await _authorizationService.AuthorizeAsync(User, document, "EditDocument")` to check whether the current user may edit that specific document. Another use case is conditional rendering: a Razor Page passes items through `IAuthorizationService` to decide which ones to show in a list based on per-item policies. `IAuthorizationService` returns an `AuthorizationResult` with an `IsSucceeded` Boolean and a list of failed requirements; I decide what to do with a failure rather than having the framework handle it automatically.

---

## Q19. What is resource-based authorization, and how does it differ from standard attribute-driven policy authorization?

**Concepts**
- Runtime resource object passed to handler alongside ClaimsPrincipal
- Attribute-driven policy pre-action execution limitation
- AuthorizationHandlerContext.Resource casting to entity type
- [Authorize] at entry + IAuthorizationService inside action pattern

**Answer**

Resource-based authorization is a pattern where the authorization decision depends on a specific runtime object — the resource — in addition to the user's claims and the policy. Standard attribute-driven policies cannot access the resource because attributes are evaluated before the action method runs, before the resource is loaded from a database. I call `IAuthorizationService.AuthorizeAsync(User, resource, policy)` inside an action method after loading the resource, passing the actual entity as the resource parameter. The policy's `IAuthorizationHandler` receives the resource through `AuthorizationHandlerContext.Resource` (typed as `object`) and casts it to the expected type to inspect owner IDs, visibility flags, or other instance-specific data. A concrete example: `[Authorize]` at the controller level ensures the user is logged in; inside the action, after loading the document, `AuthorizeAsync(User, document, "DocumentOwner")` ensures the user owns that particular document.

---

## Q20. What does `[AllowAnonymous]` do, and can it override an `[Authorize]` attribute placed on a parent controller?

**Concepts**
- [AllowAnonymous] bypassing all IAuthorizeData metadata on the endpoint
- Unconditional override of parent controller [Authorize]
- FallbackPolicy override by [AllowAnonymous]
- Minimal API AllowAnonymous() equivalent

**Answer**

`[AllowAnonymous]` marks an action, controller, or endpoint as explicitly exempt from all authorization requirements — when the authorization middleware sees this metadata, it skips all policy evaluation and allows the request to proceed regardless of the user's identity. It always overrides any `[Authorize]` attributes, whether placed on the same element, a parent controller, or a global filter. This means I can apply `[Authorize]` globally via `AuthorizationOptions.FallbackPolicy` and use `[AllowAnonymous]` on individual public endpoints — login, registration, public landing pages — without removing the global protection. `[AllowAnonymous]` wins unconditionally: placing both `[Authorize]` and `[AllowAnonymous]` on the same action results in anonymous access. In minimal APIs the equivalent is `endpoints.MapGet("/login", ...).AllowAnonymous()`.

---

## Q21. What is a claims-based identity, and how do claims drive authorization decisions in ASP.NET Core?

**Concepts**
- Claim as typed key-value string pair asserting a user fact
- Claims issued at authentication time and traveling with the session
- Stateless JWT distribution across distributed services
- Sensitive data risk in unencrypted JWT claims

**Answer**

A claims-based identity is an identity model where all facts about a user — name, email, roles, department, subscription tier — are expressed as `Claim` objects (typed key-value pairs) attached to a `ClaimsIdentity`. Authorization rules then evaluate those claims rather than querying a separate user store at runtime. A claim is created as `new Claim(type, value)` where `type` is typically a `ClaimTypes` constant or a custom string, and `value` is always a string. Role checks, policy requirements, and custom authorization handlers all read claims off the `ClaimsPrincipal` — there is no framework-provided mechanism to call a database inside `[Authorize]` unless I implement it in a handler. Claims are issued at authentication time and travel with the user's session; this stateless design is what makes JWTs work across distributed services without a shared session store. Because claims are just strings, sensitive data like database IDs or financial limits that travel in a JWT are visible to the client — I avoid placing sensitive business data in claims unless the token is encrypted (JWE format).

---

## Q22. How do you implement a custom authentication handler in ASP.NET Core?

**Concepts**
- AuthenticationHandler<TOptions> base class with HandleAuthenticateAsync
- AuthenticateResult.NoResult for absent credential, Fail for invalid
- AddScheme registration with scheme name and options type
- HandleChallengeAsync and HandleForbiddenAsync customization

**Answer**

A custom authentication handler inherits from `AuthenticationHandler<TOptions>` and overrides `HandleAuthenticateAsync`; it may also override `HandleChallengeAsync` and `HandleForbiddenAsync` to customize the HTTP responses for unauthenticated and unauthorized requests. The handler is registered with `AddAuthentication().AddScheme<TOptions, THandler>("SchemeName", options => { })`.

```csharp
public class ApiKeyHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var key))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (key != "secret")
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));

        var claims = new[] { new Claim(ClaimTypes.Name, "ApiClient") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

`HandleAuthenticateAsync` must never throw — all validation errors must be returned as `AuthenticateResult.Fail(...)` so the framework can route to the appropriate challenge or forbid response. The handler has access to `Request`, `Response`, `Context`, `Scheme`, and `Options` through base class properties. `AuthenticationSchemeOptions` is used as the options class when no custom options are needed; I define a derived class with additional settings when the scheme needs configuration.

---

## Q23. What is the difference between `ClaimsIdentity.IsAuthenticated` returning `true` versus `false`?

**Concepts**
- IsAuthenticated derived solely from non-empty AuthenticationType
- ClaimsIdentity constructor AuthenticationType parameter
- Challenge vs Forbid routing from IsAuthenticated
- Claims-with-no-AuthenticationType anti-pattern

**Answer**

`ClaimsIdentity.IsAuthenticated` returns `true` when the identity's `AuthenticationType` property is a non-null, non-empty string, indicating the identity was constructed by a recognized authentication mechanism. When `AuthenticationType` is `null` or empty, `IsAuthenticated` returns `false`, representing an anonymous placeholder. The `AuthenticationType` is set by passing it to the constructor: `new ClaimsIdentity(claims, "Bearer")` produces an authenticated identity, while `new ClaimsIdentity()` produces an anonymous one. The authorization middleware uses `User.Identity.IsAuthenticated` to choose between calling `ChallengeAsync` (unauthenticated path) and `ForbidAsync` (authenticated but unauthorized path). A common bug is creating a `ClaimsIdentity` with claims but without an `AuthenticationType`, producing an identity that carries claims yet reports itself as unauthenticated — the principal has data but `[Authorize]` still fails.

---

## Q24. What is `AuthenticationProperties`, and what kind of data does it carry?

**Concepts**
- Dictionary-backed session metadata beyond identity claims
- IsPersistent and ExpiresUtc controlling cookie session behavior
- RedirectUri for post-login return navigation
- Small payload requirement due to cookie serialization size limits

**Answer**

`AuthenticationProperties` is a dictionary-backed object that travels alongside an authentication ticket, carrying session-level metadata that is not part of the user's identity claims. It is serialized into the authentication cookie and read back on subsequent requests. Typical properties include `IsPersistent` (whether the cookie persists across browser sessions), `ExpiresUtc` (absolute expiry time), `IssuedUtc` (issue time for sliding expiration calculations), and `AllowRefresh`. `RedirectUri` is set by the challenge mechanism to store where the user should be sent after a successful login, and the post-login callback reads it back to perform the redirect. I can store custom string values in `AuthenticationProperties.Items` for application-specific metadata such as a return URL or a selected tenant identifier. Because `AuthenticationProperties` is serialized into the cookie, I keep it small to avoid large cookie payloads that may exceed browser or server limits.

---

## Q25. How does the authorization middleware interact with endpoint metadata in the endpoint routing model?

**Concepts**
- UseRouting attaching endpoint with metadata before UseAuthorization runs
- IAuthorizeData and IAllowAnonymous metadata reading
- Middleware-level authorization protecting non-MVC endpoints uniformly
- Endpoint metadata immutability at startup

**Answer**

In the endpoint routing model, `UseRouting` matches the request to an endpoint and attaches the endpoint object — including all its metadata — to `HttpContext`. `UseAuthorization` then reads that endpoint's metadata to find `IAuthorizeData` entries (from `[Authorize]` attributes) and `IAllowAnonymous` markers, and uses them to determine which policies to enforce. Because the endpoint is resolved by `UseRouting` before `UseAuthorization` runs, the authorization middleware has access to the full policy list for the matched endpoint without executing the action. In the old MVC-only model, authorization ran as an `IAuthorizationFilter` inside the MVC pipeline; in the endpoint routing model it runs as middleware, so it uniformly protects MVC actions, Razor Pages, gRPC endpoints, SignalR hubs, and minimal API endpoints. Endpoint metadata is immutable at runtime, built during application startup when `MapControllers()`, `MapRazorPages()`, or `MapGet()` are called.

---

## Q26. What is the difference between calling `RequireAuthorization()` on an endpoint and applying the `[Authorize]` attribute?

**Concepts**
- Functional equivalence — both add IAuthorizeData metadata
- RequireAuthorization idiomatic for minimal APIs without controller classes
- [Authorize] co-location advantage for code review
- Mixed usage supported in the same application

**Answer**

Both `RequireAuthorization()` and `[Authorize]` add `IAuthorizeData` metadata to an endpoint; to the authorization middleware they are functionally identical. The only practical difference is location: `[Authorize]` is declared in source code on the controller or action, while `RequireAuthorization()` is applied at endpoint registration time in the route definition. `RequireAuthorization()` is the idiomatic approach for minimal APIs, where there are no controller classes and attributes cannot be applied: `app.MapGet("/secret", handler).RequireAuthorization("PolicyName")`. For MVC controllers and Razor Pages, `[Authorize]` is the common choice because it keeps security metadata co-located with the code it protects. `RequireAuthorization()` with no arguments requires an authenticated user, equivalent to `[Authorize]` with no parameters. I mix both in the same application without any issues.

---

## Q27. What are authentication scheme forwarding and passthrough, and when would you use them?

**Concepts**
- ForwardAuthenticate/Challenge/Forbid delegating to a different scheme
- ForwardDefaultSelector for per-request dynamic scheme selection
- NoResult passthrough for conditional scheme activation
- Mixed API/browser ForwardDefaultSelector pattern

**Answer**

Scheme forwarding lets one scheme delegate a specific operation to a different scheme rather than handling it itself. I configure it via properties on `AuthenticationSchemeOptions`: `ForwardAuthenticate`, `ForwardChallenge`, `ForwardForbid`, `ForwardSignIn`, `ForwardSignOut`. Setting `ForwardAuthenticate = "Bearer"` means that when the cookie scheme is asked to authenticate, it immediately delegates to the JWT Bearer scheme instead. `ForwardDefaultSelector` is a function-based variant: `context => context.Request.Path.StartsWithSegments("/api") ? "Bearer" : "Cookies"` — it selects the target scheme dynamically per-request. The common use case for mixed API/browser applications is to set Cookie as the default scheme for HTML endpoints (redirect-to-login behavior) and use `ForwardDefaultSelector` to route API paths to JWT Bearer (401 responses). Passthrough — returning `NoResult` — is used in custom handlers that should only activate for requests with a specific custom header, stepping aside for others.

---

## Q28. What are the key tradeoffs between cookie-based authentication and JSON Web Token (JWT) bearer authentication in ASP.NET Core?

**Concepts**
- Cookie server-managed session vs JWT stateless token
- CSRF risk from automatic browser cookie sending
- JWT revocation difficulty before expiry — short lifetime + refresh token mitigation
- Scalability — JWT requires no server-side state in a farm

**Answer**

Cookie-based authentication stores the session on the server side (or encrypts it into the cookie) and is managed automatically by the browser, making it a natural fit for server-rendered web applications. JWT bearer authentication is stateless — all identity data is encoded in the token itself — making it suitable for APIs, mobile clients, and distributed microservice architectures.

| Dimension | Cookie Authentication | JWT Bearer Authentication |
|---|---|---|
| State | Server-managed session or encrypted cookie payload | Stateless — all data in the token |
| Storage | Browser cookie (automatic) | Client-managed (header, local storage, etc.) |
| Revocation | Easy — delete session or set cookie to expired | Hard — token is valid until expiry without a denylist |
| CSRF risk | Yes — browsers auto-send cookies | No — client must attach the token explicitly |
| Cross-origin use | Restricted by same-site policies | Works across origins with CORS |
| Scalability | Session data may require shared storage in a farm | Scales naturally — no server-side state |

JWT tokens cannot be revoked before their `exp` claim expires without maintaining a server-side denylist, which reintroduces statefulness; short token lifetimes combined with refresh tokens are the standard mitigation. Cookies are automatically sent by browsers including on cross-site requests, creating CSRF risk; ASP.NET Core's anti-forgery middleware mitigates this for form submissions. For applications that serve both browser UIs and API clients, I use cookies for the browser-facing pages and JWT Bearer for the API surface, with `ForwardDefaultSelector` routing configured appropriately.

---

## Gotchas — Authentication & Authorization (Interview Traps)

---

#### Gotcha 1. Registering `UseAuthorization` before `UseAuthentication`

**Concepts**
- Middleware registration order determining HttpContext.User state
- UseAuthentication before UseAuthorization requirement
- Anonymous principal visible to UseAuthorization when order is reversed

**Answer**

Many developers assume the order of `UseAuthentication` and `UseAuthorization` does not matter because both are middleware — in fact, registering them in the wrong order means `HttpContext.User` is still anonymous when authorization runs, so all protected endpoints fail silently or allow anonymous access depending on the fallback policy. The correct order in `Program.cs` is always `UseAuthentication()` followed by `UseAuthorization()`, with both placed after `UseRouting()` but before `MapControllers()` or equivalent. If valid credentials are ignored and every request is treated as anonymous, the first thing to check is the middleware registration order.

---

#### Gotcha 2. Confusing Challenge (401) with Forbid (403)

**Concepts**
- Challenge (401) for unauthenticated users
- Forbid (403) for authenticated but unauthorized users
- IsAuthenticated driving the middleware choice
- Role claims and policy requirements as 403 causes

**Answer**

A failed `[Authorize]` does not always produce a 401 — a 401 (Challenge) is produced only when the user is not authenticated at all, while a 403 (Forbid) is produced when the user is authenticated but lacks the required permission. Getting a 403 when I expected to be authorized means I am authenticated but my claims do not satisfy the policy — I check role claims, policy requirements, and claim types. Getting a 401 when I expected to be authenticated usually means the authentication handler returned `NoResult` or `Fail` — I check that the token is present, not expired, and that the scheme name matches the default or the explicit `[Authorize(AuthenticationSchemes)]` value.

---

#### Gotcha 3. Creating a `ClaimsIdentity` without an `AuthenticationType`

**Concepts**
- AuthenticationType non-empty string required for IsAuthenticated = true
- ClaimsIdentity(claims, Scheme.Name) correct constructor form
- [Authorize] failing despite claims being present

**Answer**

A `ClaimsIdentity` constructed without an `AuthenticationType` reports `IsAuthenticated = false` even if it carries many claims, because `IsAuthenticated` is derived solely from whether `AuthenticationType` is non-empty. This causes the authorization middleware to treat the identity as anonymous and issue a challenge. I always pass the scheme name or a meaningful string as the second argument: `new ClaimsIdentity(claims, Scheme.Name)` or `new ClaimsIdentity(claims, "ApiKey")`. A handler that correctly builds a ticket but omits `AuthenticationType` produces a principal that has claims but fails `[Authorize]` checks — a subtle bug that appears to work until authorization is enabled.

---

#### Gotcha 4. Assuming `[AllowAnonymous]` only affects the attribute's target

**Concepts**
- [AllowAnonymous] bypassing all IAuthorizeData on the endpoint
- Parent controller [Authorize] silently bypassed
- Security audit requirement for all [AllowAnonymous] usages

**Answer**

Developers sometimes believe `[AllowAnonymous]` only disables authorization on the specific element it is placed on — in fact it completely bypasses all `IAuthorizeData` metadata on the endpoint, including policies inherited from parent classes and globally registered filters. A security-critical policy applied at the controller level is silently bypassed for any action decorated with `[AllowAnonymous]`, which can be an unintentional security hole if the attribute is applied carelessly. I always audit every `[AllowAnonymous]` usage and confirm that the endpoint genuinely requires no authentication before applying it.

---

#### Gotcha 5. JWT Bearer scheme does not support `SignIn` or `SignOut`

**Concepts**
- JWT Bearer implementing authenticate only — not IAuthenticationSignInHandler
- InvalidOperationException on HttpContext.SignInAsync with JWT Bearer
- Token generation as application code, not authentication pipeline SignIn
- Cookie required for stateful session persistence

**Answer**

JWT Bearer is a stateless scheme that only implements the authenticate (and challenge/forbid) operations — calling `HttpContext.SignInAsync` against it throws an `InvalidOperationException` at runtime because the handler does not implement `IAuthenticationSignInHandler`. Sign-in and sign-out are meaningful only for stateful schemes such as Cookie authentication, which can persist and later revoke an identity on behalf of the server. In applications that issue JWTs, the token generation is done in a dedicated login endpoint using plain code outside the authentication pipeline, and the client is responsible for storing and attaching the token on subsequent requests. Confusing "issuing a JWT" with "calling SignInAsync" is a common mistake when migrating from cookie-based to JWT-based auth.

---

#### Gotcha 6. Setting a Fallback Policy When `RequireAuthenticatedUser` Was Already Applied

**Concepts**
- `options.FallbackPolicy` applying to endpoints with no `IAuthorizeData`
- `RequireAuthenticatedUser` as a shorthand for `options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()`
- Double-application causing confusion but no functional change
- `[AllowAnonymous]` still overriding the fallback policy on individual endpoints

**Answer**

A common confusion is between calling `options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()` directly and calling `options.AddDefaultPolicy(...)`. The fallback policy applies to all endpoints that have no explicit `IAuthorizeData` metadata — meaning endpoints with no `[Authorize]` attribute and no `RequireAuthorization()` call. Applying both `RequireAuthenticatedUser` via the fallback policy and an explicit `[Authorize]` on the controller results in the policies combining (AND), which has no visible effect if both require only authentication, but causes hard-to-debug denials if the second policy adds additional requirements. The `[AllowAnonymous]` attribute still overrides the fallback policy, which is intentional but surprises developers who expected a strict default-deny enforcement.

---

#### Gotcha 7. `IAuthorizationService.AuthorizeAsync` Does Not Throw on Failure

**Concepts**
- `AuthorizationResult.Succeeded` boolean — not an exception on denial
- Silently passing even when the authorization result is Failure
- Calling `AuthorizeAsync` without checking the result granting unauthorized access

**Answer**

`IAuthorizationService.AuthorizeAsync` always completes successfully as a Task — it returns an `AuthorizationResult` with a `Succeeded` flag rather than throwing an exception when authorization is denied. Developers who forget to check the result and proceed with the protected action will inadvertently grant access to every caller, regardless of their permissions. Unlike the `[Authorize]` attribute, which wires the result into the response pipeline, programmatic authorization calls are entirely the developer's responsibility: check `result.Succeeded`, and if it is `false`, return a `ForbidResult()` or `ChallengeResult()` explicitly — the framework does nothing automatically when the result is ignored.

---

#### Gotcha 8. `User.IsInRole()` Is Case-Sensitive and Depends on the Role Claim Type

**Concepts**
- `ClaimsPrincipal.IsInRole` comparing claim values with OrdinalIgnoreCase by default
- Role claim type varying between schemes — `ClaimTypes.Role` vs `"roles"` vs `"role"`
- JWT Bearer mapping `roles` claim to `ClaimTypes.Role` only when `RoleClaimType` is configured

**Answer**

`ClaimsPrincipal.IsInRole` searches for claims whose type matches the identity's `RoleClaimType`, comparing values with `OrdinalIgnoreCase` by default — but the `RoleClaimType` itself varies between authentication schemes. Cookie authentication sets it to `ClaimTypes.Role` (a long URI), while JWT Bearer may emit role information in a `roles` or `role` claim with a short name. If the JWT Bearer options are not configured with `TokenValidationParameters.RoleClaimType = "roles"`, `User.IsInRole("Admin")` will always return `false` even when the token carries an `"Admin"` value in the `roles` claim, because the runtime is looking for the claim type `ClaimTypes.Role`, not `"roles"`. The fix is to configure `RoleClaimType` explicitly or to use policy-based authorization with a `RequireClaim` check that references the correct claim type.

---

#### Gotcha 9. Schemes Registered with `AddAuthentication` Do Not Run Unless Middleware Is Also Added

**Concepts**
- `AddAuthentication` registering services only — no middleware execution
- `UseAuthentication` required for the handler to populate `HttpContext.User`
- Handlers run only when invoked — not on every request by default

**Answer**

Calling `AddAuthentication().AddJwtBearer(...)` in the DI container registers the scheme and its handler as services, but it does not cause the handler to run on any request — that requires the `UseAuthentication()` middleware call in the pipeline. Omitting `UseAuthentication()` means `HttpContext.User` is never populated from the token, authentication challenges are never triggered, and `[Authorize]` always sees an anonymous user, but no error appears at startup. A subtler version of this problem arises when the middleware is present but authentication handlers are invoked for the wrong scheme because the default was not configured explicitly, causing the wrong handler to run and the correct one to be silently skipped.

---

#### Gotcha 10. Authorization Handler Registered as `Singleton` Using Scoped Services Causes Runtime Errors

**Concepts**
- Singleton authorization handler capturing scoped dependencies on first resolution
- `IServiceProvider` injection pattern required to resolve scoped services safely
- Silent stale data from a scoped service captured at startup rather than per-request

**Answer**

Authorization handlers that need database access or other per-request state must be registered as `Scoped` in the DI container — not `Singleton`. Registering a handler as `Singleton` while its constructor takes a `Scoped` dependency (such as `DbContext`) causes ASP.NET Core's DI container to throw an `InvalidOperationException` at startup in development mode (when scope validation is enabled) or, worse, to capture a single `DbContext` instance at first resolution in production, leading to stale query results and thread-safety violations across all requests. The correct fix is to register the handler with `services.AddScoped<IAuthorizationHandler, MyHandler>()`, which ensures a new instance — and a new `DbContext` — is created for each request.
