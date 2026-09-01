# Authentication & Authorization Fundamentals — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [What is the difference between authentication and authorization, and which must …](#q1)
2. [What does the `UseAuthentication` middleware do, and why must it be registered b…](#q2)
3. [What is an authentication scheme in ASP.NET Core, and how is one registered?](#q3)
4. [What is the role of an authentication handler, and what interface must it implem…](#q4)
5. [What does `IAuthenticationService` do, and how does it relate to registered auth…](#q5)
6. [What is `AuthenticateResult`, and what are the possible outcomes it represents?](#q6)
7. [What is a `ClaimsPrincipal`, and how is it related to `ClaimsIdentity` and `Clai…](#q7)
8. [How is `HttpContext.User` populated during an HTTP request?](#q8)
9. [What is a default authentication scheme, and why does it matter when no scheme i…](#q9)
10. [How do you configure multiple authentication schemes, and how does ASP.NET Core …](#q10)
11. [What is the difference between a Challenge response and a Forbid response in ASP…](#q11)
12. [What do `SignIn` and `SignOut` operations do inside the authentication pipeline?](#q12)
13. [What does the `[Authorize]` attribute do at its simplest, and what happens when …](#q13)
14. [How does role-based authorization work in ASP.NET Core, and what are its limitat…](#q14)
15. [What is policy-based authorization, and why is it preferred over role-based auth…](#q15)
16. [What is `IAuthorizationRequirement`, and what is the relationship between a requ…](#q16)
17. [What is `IAuthorizationHandler`, and how does it signal success or failure when …](#q17)
18. [What is `IAuthorizationService`, and when would you use it programmatically inst…](#q18)
19. [What is resource-based authorization, and how does it differ from standard attri…](#q19)
20. [What does `[AllowAnonymous]` do, and can it override an `[Authorize]` attribute …](#q20)
21. [What is a claims-based identity, and how do claims drive authorization decisions…](#q21)
22. [How do you implement a custom authentication handler in ASP.NET Core?](#q22)
23. [What is the difference between `ClaimsIdentity.IsAuthenticated` returning `true`…](#q23)
24. [What is `AuthenticationProperties`, and what kind of data does it carry?](#q24)
25. [How does the authorization middleware interact with endpoint metadata in the end…](#q25)
26. [What is the difference between calling `RequireAuthorization()` on an endpoint a…](#q26)
27. [What are authentication scheme forwarding and passthrough, and when would you us…](#q27)
28. [What are the key tradeoffs between cookie-based authentication and JSON Web Toke…](#q28)

---

## Q1. What is the difference between authentication and authorization, and which must happen first?

What is the difference between authentication and authorization, and which must happen first?

**Answer:** Authentication is the process of establishing who the caller is — it produces an identity. Authorization is the process of deciding what that established identity is allowed to do. Authentication must always happen first because authorization decisions are meaningless without a known identity to evaluate.

- Authentication answers "Who are you?" and, on success, produces a `ClaimsPrincipal` that represents the caller's identity with one or more attached claims.
- Authorization answers "Are you allowed to do this?" and consumes the principal produced by authentication to evaluate policies, roles, or custom requirements.
- Reversing the order — or skipping authentication — means the authorization layer receives an unauthenticated or anonymous principal, which will cause all protected resources to be rejected or, worse, allowed by mistake.

---

## Q2. What does the `UseAuthentication` middleware do, and why must it be registered before `UseAuthorization`?

What does the `UseAuthentication` middleware do, and why must it be registered before `UseAuthorization`?

**Answer:** `UseAuthentication` runs the default authentication scheme's handler on every incoming request, populating `HttpContext.User` with a `ClaimsPrincipal` if the request carries valid credentials. `UseAuthorization` then reads that already-populated principal to enforce policies; if the order is reversed, `HttpContext.User` is still the default anonymous identity when authorization runs, so all protected endpoints appear to receive an unauthenticated user.

- When `UseAuthentication` executes, it calls `IAuthenticationService.AuthenticateAsync` using the configured default scheme, which in turn calls the corresponding handler (for example, the cookie or JWT Bearer handler).
- The handler inspects the request (reading a cookie or `Authorization` header), validates the credential, and either sets `HttpContext.User` to an authenticated principal or leaves it as the unauthenticated anonymous identity.
- `UseAuthorization` must appear after `UseAuthentication` but also after `UseRouting`, because it needs the matched endpoint's authorization metadata (such as `[Authorize]` attributes) to know what policies to enforce.
- A common mistake is placing `UseAuthorization` before `UseAuthentication`, resulting in every request appearing anonymous even when a valid token is presented.

---

## Q3. What is an authentication scheme in ASP.NET Core, and how is one registered?

What is an authentication scheme in ASP.NET Core, and how is one registered?

**Answer:** An authentication scheme is a named configuration that pairs a display name with a specific authentication handler and its options. Schemes give you a stable, referenceable name — such as `"Bearer"` or `"Cookies"` — that the rest of the framework uses to invoke the correct handler without hard-coding handler type names.

- You register a scheme by calling one of the `Add*` extension methods inside `builder.Services.AddAuthentication(...)`, for example `AddCookie("Cookies", options => { ... })` or `AddJwtBearer("Bearer", options => { ... })`.
- Each scheme registration stores a `AuthenticationSchemeBuilder` entry in `IAuthenticationSchemeProvider`, which is later resolved at runtime to find the right handler.
- The string name passed to these methods is what you reference in `[Authorize(AuthenticationSchemes = "Bearer")]` or in `HttpContext.AuthenticateAsync("Bearer")`.
- A single application can have several schemes registered simultaneously, each with independent options — for example, cookie authentication for browser clients and JWT Bearer for API clients.

---

## Q4. What is the role of an authentication handler, and what interface must it implement?

What is the role of an authentication handler, and what interface must it implement?

**Answer:** An authentication handler is a class responsible for executing the actual mechanics of one authentication scheme — parsing credentials from the request, validating them, and producing or rejecting an identity. Every handler must implement `IAuthenticationHandler`, and most concrete handlers extend the built-in base class `AuthenticationHandler<TOptions>`.

- The core method a handler must implement is `HandleAuthenticateAsync`, which returns an `AuthenticateResult` indicating success (with a ticket), no result (credential absent), or failure (credential present but invalid).
- Handlers that support issuing challenges implement `IAuthenticationHandler.ChallengeAsync`, which generates the appropriate "please authenticate" response — a redirect to a login page for cookies, or a `401 Unauthorized` with a `WWW-Authenticate` header for JWT Bearer.
- Handlers that support forbidding access implement `IAuthenticationHandler.ForbidAsync`, which generates a "you are authenticated but not allowed" response — typically a `403 Forbidden` or a redirect to an access-denied page.
- A handler is constructed fresh per-request (transient lifetime) by the dependency injection container, giving it access to the current `HttpContext` automatically through the base class.

---

## Q5. What does `IAuthenticationService` do, and how does it relate to registered authentication handlers?

What does `IAuthenticationService` do, and how does it relate to registered authentication handlers?

**Answer:** `IAuthenticationService` is the central facade for all authentication operations — authenticate, challenge, forbid, sign-in, and sign-out. It is injected into middleware and framework components so they do not need to resolve handlers directly; instead they call the service with a scheme name, and the service locates and invokes the correct handler through `IAuthenticationHandlerProvider`.

- When `UseAuthentication` middleware runs, it calls `context.AuthenticateAsync()` (which resolves to `IAuthenticationService.AuthenticateAsync`), passing the default scheme name to let the service find the right handler.
- `IAuthenticationService` is also called by the authorization middleware when a challenge or forbid response is needed — it delegates to the scheme's handler's `ChallengeAsync` or `ForbidAsync` methods.
- Developers can call `IAuthenticationService` directly from controllers or Razor Pages using the `HttpContext.AuthenticateAsync`, `HttpContext.SignInAsync`, and `HttpContext.SignOutAsync` extension methods.
- The default implementation, `AuthenticationService`, caches resolved handlers within a single request to avoid repeated construction.

---

## Q6. What is `AuthenticateResult`, and what are the possible outcomes it represents?

What is `AuthenticateResult`, and what are the possible outcomes it represents?

**Answer:** `AuthenticateResult` is the object returned by `HandleAuthenticateAsync` on any authentication handler, and it encapsulates whether authentication succeeded, was skipped, or failed explicitly. The three outcomes map to distinct actions the framework takes on the request.

- `AuthenticateResult.Success(ticket)` means the handler found and validated a credential; the `AuthenticationTicket` inside carries the `ClaimsPrincipal` that will be assigned to `HttpContext.User`.
- `AuthenticateResult.NoResult()` means the handler found no credential it could process — the request had no cookie or no `Authorization` header — so the handler opts out without producing an error; other schemes may still be tried.
- `AuthenticateResult.Fail(exception)` means the handler found a credential but it was invalid (expired token, tampered signature, etc.); this is a hard failure distinct from simply having no credential.
- The distinction between `NoResult` and `Fail` matters for forwarding logic and for producing meaningful error responses: a `Fail` result typically leads to a 401 with an error description, while a `NoResult` may allow the request to continue as anonymous.

---

## Q7. What is a `ClaimsPrincipal`, and how is it related to `ClaimsIdentity` and `Claim`?

What is a `ClaimsPrincipal`, and how is it related to `ClaimsIdentity` and `Claim`?

**Answer:** A `ClaimsPrincipal` is the top-level security object that represents a user; it can hold one or more `ClaimsIdentity` objects, each representing a separate identity the user has authenticated with. Each `ClaimsIdentity` contains a flat collection of `Claim` objects, where a claim is a typed key-value pair asserting a fact about the user, such as their name, email, or role.

- The three-level hierarchy — `ClaimsPrincipal` > `ClaimsIdentity` > `Claim` — exists because a single user can be authenticated by multiple schemes simultaneously; each scheme contributes one `ClaimsIdentity` to the same principal.
- `ClaimsPrincipal.Identity` returns the first (primary) `ClaimsIdentity` in the collection, and `IsAuthenticated` on that identity is `true` only when it has a non-empty `AuthenticationType` string.
- Claims carry values as strings, so a role stored as a claim looks like `new Claim(ClaimTypes.Role, "Admin")`; the authorization system reads these values when evaluating role checks or custom policies.
- You can add extra `ClaimsIdentity` objects to a principal — for example, after a second-factor check — without discarding the original identity.

---

## Q8. How is `HttpContext.User` populated during an HTTP request?

How is `HttpContext.User` populated during an HTTP request?

**Answer:** `HttpContext.User` starts as an unauthenticated anonymous `ClaimsPrincipal` at the beginning of every request and is replaced by the authenticated principal when `UseAuthentication` middleware runs and the default scheme's handler succeeds. If the handler returns `NoResult` or `Fail`, the property remains the anonymous principal.

- The `UseAuthentication` middleware calls `IAuthenticationService.AuthenticateAsync` with the configured default scheme name; if the result is `Success`, it assigns `result.Principal` to `HttpContext.User`.
- Some schemes — particularly cookie authentication — also refresh or re-issue their credential (sliding expiry) as part of populating the user, so the assignment step may also write a new cookie to the response.
- When multiple schemes are configured, only the default scheme runs automatically during `UseAuthentication`; other schemes are invoked only when explicitly requested — for example, through `[Authorize(AuthenticationSchemes = "Bearer")]`.
- After `UseAuthentication` runs, all subsequent middleware and endpoint handlers can read `HttpContext.User` and trust that it reflects the best available authentication result for the request.

---

## Q9. What is a default authentication scheme, and why does it matter when no scheme is explicitly named?

What is a default authentication scheme, and why does it matter when no scheme is explicitly named?

**Answer:** The default authentication scheme is the scheme that ASP.NET Core uses automatically when no explicit scheme name is provided — for example, when `UseAuthentication` runs, when `[Authorize]` has no `AuthenticationSchemes` property set, or when challenge/forbid is triggered without a named scheme. Setting a sensible default simplifies code and prevents the framework from throwing at runtime when it cannot determine which scheme to invoke.

- You set the default by passing its name to `AddAuthentication`: `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)`.
- ASP.NET Core actually has five distinct "default" slots: default authenticate, default challenge, default forbid, default sign-in, and default sign-out scheme; if you pass a single name to `AddAuthentication`, it fills all five slots with that name.
- You can override individual slots — for instance, using JWT Bearer as the default authenticate scheme but Cookie as the default sign-in scheme — by setting properties on `AuthenticationOptions` individually.
- If no default is configured and the code tries to authenticate without naming a scheme, an `InvalidOperationException` is thrown, which is a common misconfiguration error.

---

## Q10. How do you configure multiple authentication schemes, and how does ASP.NET Core decide which one to invoke?

How do you configure multiple authentication schemes, and how does ASP.NET Core decide which one to invoke?

**Answer:** Multiple schemes are registered by calling several `Add*` methods in sequence inside `AddAuthentication`, each with a unique name. The framework selects a scheme through a priority order: an explicit scheme name on the `[Authorize]` attribute wins first; otherwise the default scheme defined in `AuthenticationOptions` is used.

- Example: calling both `AddCookie("Cookies")` and `AddJwtBearer("Bearer")` registers two schemes; you can then apply `[Authorize(AuthenticationSchemes = "Cookies,Bearer")]` to accept either credential type for a given endpoint.
- Scheme forwarding allows one scheme to delegate its operation to another scheme under specific conditions, configured via `ForwardDefaultSelector` on `AuthenticationSchemeOptions`.
- When multiple scheme names are provided in `AuthenticationSchemes`, ASP.NET Core creates a combined principal that merges the identities from all successful schemes, giving the request access under any of them.
- For API-plus-browser applications, a common pattern is to set Cookie as the default scheme for HTML endpoints (so browsers get redirect-to-login behavior) and JWT Bearer for API endpoints (so they get 401 responses).

---

## Q11. What is the difference between a Challenge response and a Forbid response in ASP.NET Core?

What is the difference between a Challenge response and a Forbid response in ASP.NET Core?

**Answer:** A Challenge response tells the caller "you have not yet authenticated — please prove your identity," while a Forbid response tells the caller "you are already authenticated, but you do not have permission to do this." Both are generated by calling the active scheme's handler, but they carry different HTTP semantics and trigger different behavior per scheme.

| Situation | Method called | Typical HTTP result (Cookie) | Typical HTTP result (JWT Bearer) |
|---|---|---|---|
| No valid credential | `ChallengeAsync` | 302 redirect to login page | 401 Unauthorized |
| Valid credential, insufficient permission | `ForbidAsync` | 302 redirect to access-denied page | 403 Forbidden |

- The authorization middleware automatically chooses between Challenge and Forbid based on `HttpContext.User.Identity.IsAuthenticated`: if `false`, it challenges; if `true`, it forbids.
- Scheme handlers can customize both responses — for example, a cookie handler adds a `ReturnUrl` query parameter to the login redirect so the user lands back on the original page after sign-in.
- Developers can trigger either response manually: `HttpContext.ChallengeAsync()` or `HttpContext.ForbidAsync()`, optionally passing a specific scheme name.

---

## Q12. What do `SignIn` and `SignOut` operations do inside the authentication pipeline?

What do `SignIn` and `SignOut` operations do inside the authentication pipeline?

**Answer:** `SignIn` takes a `ClaimsPrincipal` and an `AuthenticationProperties` object and instructs the named scheme's handler to persist that identity — for example, by writing an encrypted authentication cookie or issuing a session token. `SignOut` instructs the handler to delete or revoke that persisted credential, ending the authenticated session.

- `SignIn` is only supported by schemes that implement `IAuthenticationSignInHandler`; JWT Bearer is a stateless scheme and does not support sign-in — only cookie and similar stateful schemes do.
- Calling `HttpContext.SignInAsync("Cookies", principal, properties)` writes an encrypted `FormsAuth`-style cookie to the response; on subsequent requests the cookie handler decrypts it and reconstructs the `ClaimsPrincipal`.
- `SignOut` clears the cookie (by setting it to expired) and may also revoke any server-side session token if the scheme maintains one.
- `AuthenticationProperties` passed to `SignIn` can carry metadata such as `IsPersistent` (whether the cookie survives browser close), `ExpiresUtc` (explicit expiry time), and `RedirectUri` (where to send the user after signing in).

---

## Q13. What does the `[Authorize]` attribute do at its simplest, and what happens when the authorization check fails?

What does the `[Authorize]` attribute do at its simplest, and what happens when the authorization check fails?

**Answer:** At its simplest, `[Authorize]` with no parameters requires that `HttpContext.User.Identity.IsAuthenticated` is `true` — that is, any successfully authenticated user is allowed, regardless of role or policy. When the check fails, the authorization middleware either challenges the caller (if unauthenticated) or forbids them (if authenticated but unauthorized).

- `[Authorize]` is an `IAuthorizationFilter` in MVC and an endpoint metadata marker in minimal APIs; in both cases the authorization middleware reads it and invokes `IAuthorizationService`.
- Applying `[Authorize]` to a controller class applies the requirement to every action method in that class; an action-level `[Authorize]` is additive — both the class-level and action-level requirements must be satisfied.
- When authorization fails, the middleware does not short-circuit to a hardcoded 401/403; it delegates to the authentication scheme's `ChallengeAsync` or `ForbidAsync`, allowing each scheme to produce its own appropriate response.
- `[Authorize(Policy = "AdminOnly")]` narrows the check to a named policy; `[Authorize(Roles = "Admin")]` restricts to users with the Admin role claim; these can be combined on the same action.

---

## Q14. How does role-based authorization work in ASP.NET Core, and what are its limitations?

How does role-based authorization work in ASP.NET Core, and what are its limitations?

**Answer:** Role-based authorization checks whether the authenticated user's `ClaimsPrincipal` contains a `ClaimTypes.Role` claim with a value matching the required role string. You declare the requirement with `[Authorize(Roles = "Admin")]` or `[Authorize(Roles = "Admin,Manager")]` (OR logic for multiple roles).

- Under the covers, `User.IsInRole("Admin")` is called, which searches all `ClaimsIdentity` objects in the principal for a claim of type `ClaimTypes.Role` with value `"Admin"`.
- Role names in JWT tokens often use a different claim type (`"roles"` or `"role"`) depending on the token issuer; the JWT Bearer handler must be configured with `TokenValidationParameters.RoleClaimType` to map the token's field to `ClaimTypes.Role`.
- The main limitation of role-based authorization is poor granularity: roles are coarse-grained strings with no built-in support for expressing conditions such as "allowed only on resources the user owns" or "allowed only during business hours."
- Policy-based authorization supersedes role-based authorization for complex scenarios because policies encapsulate arbitrarily rich logic inside `IAuthorizationRequirement` handlers.

---

## Q15. What is policy-based authorization, and why is it preferred over role-based authorization for complex rules?

What is policy-based authorization, and why is it preferred over role-based authorization for complex rules?

**Answer:** Policy-based authorization lets you define named rules — called policies — that combine one or more `IAuthorizationRequirement` objects; a request passes the policy only when all its requirements are satisfied. Policies are registered once in `AddAuthorization` and referenced by name in `[Authorize(Policy = "...")]`, separating the definition of a rule from its declaration at endpoints.

- A policy can express rich conditions that roles cannot: minimum age from a date-of-birth claim, department membership from a custom claim, or time-of-day restrictions implemented in handler logic.
- Requirements and their handlers are unit-testable in isolation because they have no dependency on HTTP infrastructure — they operate only on a `ClaimsPrincipal` and an optional resource object.
- Policy evaluation is short-circuited: if any required handler calls `context.Fail()`, the policy fails immediately; if all handlers call `context.Succeed(requirement)`, the policy passes.
- The `AuthorizationOptions.FallbackPolicy` property lets you set a policy that applies to every endpoint that has no explicit authorization metadata, making it easy to "secure by default."

---

## Q16. What is `IAuthorizationRequirement`, and what is the relationship between a requirement and its handler?

What is `IAuthorizationRequirement`, and what is the relationship between a requirement and its handler?

**Answer:** `IAuthorizationRequirement` is a marker interface — it carries no methods — that you implement on a plain data class to represent one specific authorization condition, such as "minimum age of 18" or "must be in department X." The actual evaluation logic lives in a separate `IAuthorizationHandler` class that is registered in the dependency injection container and matched to the requirement by type.

- This separation means a single requirement type can have multiple handlers — for example, one handler checks a database claim and a fallback handler checks a local cache — and the authorization system runs all matching handlers.
- A requirement class typically holds parameters: `public class MinimumAgeRequirement(int minimumAge) : IAuthorizationRequirement`.
- Handlers are registered as `services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>()` and the framework discovers all registered handlers when evaluating a policy.
- A handler signals success by calling `context.Succeed(requirement)` and signals failure by calling `context.Fail()` or simply not calling `Succeed` (a non-call is treated as no opinion, not failure, unless `RequireAuthenticatedUser` is also active).

---

## Q17. What is `IAuthorizationHandler`, and how does it signal success or failure when evaluating a requirement?

What is `IAuthorizationHandler`, and how does it signal success or failure when evaluating a requirement?

**Answer:** `IAuthorizationHandler` has a single method, `HandleAsync(AuthorizationHandlerContext)`, which receives the full context including the `ClaimsPrincipal`, all pending requirements, and an optional resource object. The handler inspects these, then signals its verdict by calling methods on the context rather than returning a Boolean.

- `context.Succeed(requirement)` marks one specific requirement as satisfied and removes it from the pending list; when all requirements are removed from pending, the policy passes.
- `context.Fail()` marks the entire authorization attempt as failed regardless of other handlers' results — it is a hard override.
- Not calling either method leaves the requirement in the pending list; if it remains pending after all handlers run, the policy fails (unless no requirement was pending to start, which is a pass).
- For convenience, the generic `AuthorizationHandler<TRequirement>` base class filters incoming calls to only those matching the requirement type `TRequirement`, so each concrete handler focuses on a single requirement.

---

## Q18. What is `IAuthorizationService`, and when would you use it programmatically instead of the `[Authorize]` attribute?

What is `IAuthorizationService`, and when would you use it programmatically instead of the `[Authorize]` attribute?

**Answer:** `IAuthorizationService` is the service that the authorization middleware ultimately calls to evaluate policies and requirements; it exposes `AuthorizeAsync(ClaimsPrincipal, object resource, string policyName)` for use anywhere in application code. You use it directly when you need to make authorization decisions inside a controller action, a service method, or a Razor Page handler rather than at the entry point.

- The primary use case is resource-based authorization: a controller loads a document from the database, then calls `await _authorizationService.AuthorizeAsync(User, document, "EditDocument")` to check whether the current user may edit that specific document instance.
- Another use case is conditional rendering: a Razor Page passes items through `IAuthorizationService` to decide which ones to show in a list, based on per-item policies.
- `IAuthorizationService` returns an `AuthorizationResult` with an `IsSucceeded` Boolean and a list of failed requirements; you decide what to do with a failure (return 403, hide a UI element, etc.) rather than having the framework handle it automatically.
- Injecting `IAuthorizationService` into a service class keeps authorization logic centralized and testable without coupling it to the HTTP layer.

---

## Q19. What is resource-based authorization, and how does it differ from standard attribute-driven policy authorization?

What is resource-based authorization, and how does it differ from standard attribute-driven policy authorization?

**Answer:** Resource-based authorization is a pattern where the authorization decision depends on a specific runtime object — the resource — in addition to the user's claims and the policy. Standard attribute-driven policies cannot access the resource because attributes are evaluated before the action method runs, before the resource is loaded from a database.

- In resource-based authorization, you call `IAuthorizationService.AuthorizeAsync(User, resource, policy)` inside an action method after loading the resource, passing the actual entity (for example, a `Document` record) as the resource parameter.
- The policy's `IAuthorizationHandler` receives the resource through `AuthorizationHandlerContext.Resource` (typed as `object`) and casts it to the expected type to inspect owner IDs, visibility flags, or other instance-specific data.
- A concrete example: `[Authorize]` at the controller level ensures the user is logged in; inside the action, after loading the document, `AuthorizeAsync(User, document, "DocumentOwner")` ensures the user owns that particular document.
- Resource-based authorization cannot be expressed as a simple attribute because attributes have no access to runtime values; the resource object is only available after the action or Razor Page handler has started executing.

---

## Q20. What does `[AllowAnonymous]` do, and can it override an `[Authorize]` attribute placed on a parent controller?

What does `[AllowAnonymous]` do, and can it override an `[Authorize]` attribute placed on a parent controller?

**Answer:** `[AllowAnonymous]` marks an action, controller, or endpoint as explicitly exempt from all authorization requirements; when the authorization middleware sees this metadata, it skips all policy evaluation and allows the request to proceed regardless of the user's identity. It always overrides any `[Authorize]` attributes, whether placed on the same element, a parent controller, or a global filter.

- This means you can apply `[Authorize]` globally (via `AuthorizationOptions.FallbackPolicy` or a global filter) and use `[AllowAnonymous]` on individual public endpoints — login, registration, public landing pages — without removing the global protection.
- `[AllowAnonymous]` works at the endpoint metadata level: the authorization middleware checks for its presence before evaluating any policy and exits immediately if found.
- Placing `[Authorize]` on a derived controller action and `[AllowAnonymous]` on the same action — both simultaneously — results in anonymous access; `[AllowAnonymous]` wins unconditionally.
- In minimal APIs the equivalent is `endpoints.MapGet("/login", ...).AllowAnonymous()`.

---

## Q21. What is a claims-based identity, and how do claims drive authorization decisions in ASP.NET Core?

What is a claims-based identity, and how do claims drive authorization decisions in ASP.NET Core?

**Answer:** A claims-based identity is an identity model where all facts about a user — name, email, roles, department, subscription tier, or anything else — are expressed as `Claim` objects (typed key-value pairs) attached to a `ClaimsIdentity`. Authorization rules then evaluate those claims rather than querying a separate user store at runtime.

- A claim is created as `new Claim(type, value)` where `type` is typically a `ClaimTypes` constant (a URI string) or a custom string, and `value` is always a string representation.
- Role checks, policy requirements, and custom authorization handlers all read claims off the `ClaimsPrincipal` — there is no framework-provided mechanism to call a database inside `[Authorize]` unless you implement it in a handler.
- Claims are issued at authentication time (by identity providers, login services, or token generators) and travel with the user's session; this stateless design is what makes JSON Web Tokens (JWTs) work across distributed services.
- Because claims are just strings, sensitive data such as database IDs or financial limits that travel in a JWT are visible to the client; avoid placing sensitive business data in claims unless the token is encrypted (JWE format).

---

## Q22. How do you implement a custom authentication handler in ASP.NET Core?

How do you implement a custom authentication handler in ASP.NET Core?

**Answer:** A custom authentication handler inherits from `AuthenticationHandler<TOptions>` and overrides `HandleAuthenticateAsync`; it may also override `HandleChallengeAsync` and `HandleForbiddenAsync` to customize the HTTP responses for unauthenticated and unauthorized requests. The handler is registered with `AddAuthentication().AddScheme<TOptions, THandler>("SchemeName", options => { })`.

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

- The `AuthenticationSchemeOptions` type is used as the options class when no custom options are needed; you can define a derived class with additional settings (API key value, allowed issuers, etc.).
- `HandleAuthenticateAsync` must never throw: all validation errors must be returned as `AuthenticateResult.Fail(...)` so the framework can route to the appropriate challenge or forbid response.
- The handler has access to `Request`, `Response`, `Context`, `Scheme`, and `Options` through base class properties, covering all typical use cases.

---

## Q23. What is the difference between `ClaimsIdentity.IsAuthenticated` returning `true` versus `false`?

What is the difference between `ClaimsIdentity.IsAuthenticated` returning `true` versus `false`?

**Answer:** `ClaimsIdentity.IsAuthenticated` returns `true` when the identity's `AuthenticationType` property is a non-null, non-empty string, indicating that the identity was constructed by a recognized authentication mechanism. When `AuthenticationType` is `null` or empty, `IsAuthenticated` returns `false`, representing an anonymous or unauthenticated placeholder identity.

- The `AuthenticationType` is set by passing it to the `ClaimsIdentity` constructor: `new ClaimsIdentity(claims, "Bearer")` produces an authenticated identity, while `new ClaimsIdentity()` produces an anonymous one.
- The authorization middleware uses `User.Identity.IsAuthenticated` to choose between calling `ChallengeAsync` (unauthenticated path) and `ForbidAsync` (authenticated but unauthorized path).
- This design means you can inspect whether a user is authenticated anywhere in the pipeline simply by checking `HttpContext.User.Identity?.IsAuthenticated == true` without needing to call an authentication service.
- A common mistake is creating a `ClaimsIdentity` with claims but without an `AuthenticationType`, producing an identity that carries claims yet reports itself as unauthenticated.

---

## Q24. What is `AuthenticationProperties`, and what kind of data does it carry?

What is `AuthenticationProperties`, and what kind of data does it carry?

**Answer:** `AuthenticationProperties` is a dictionary-backed object that travels alongside an authentication ticket, carrying session-level metadata that is not part of the user's identity claims. It is serialized into the authentication cookie (or other storage medium) and read back on subsequent requests.

- Typical properties include `IsPersistent` (whether the cookie persists across browser sessions), `ExpiresUtc` (absolute expiry time), `IssuedUtc` (issue time for sliding expiration calculations), and `AllowRefresh` (whether the cookie expiry can be extended).
- `RedirectUri` is set by the challenge mechanism to store where the user should be sent after a successful login, and the post-login callback reads it back to perform the redirect.
- You can store custom string values in `AuthenticationProperties.Items`, a `Dictionary<string, string?>`, for application-specific metadata such as a return URL or a selected tenant identifier.
- Because `AuthenticationProperties` is serialized into the cookie, keep it small to avoid large cookie payloads that may exceed browser or server limits.

---

## Q25. How does the authorization middleware interact with endpoint metadata in the endpoint routing model?

How does the authorization middleware interact with endpoint metadata in the endpoint routing model?

**Answer:** In the endpoint routing model, `UseRouting` matches the request to an endpoint and attaches the endpoint object — including all its metadata (attributes, fluent declarations, etc.) — to `HttpContext`. `UseAuthorization` then reads that endpoint's metadata to find `IAuthorizeData` entries (from `[Authorize]` attributes) and `IAllowAnonymous` markers, and uses them to determine which policies to enforce.

- Because the endpoint is resolved by `UseRouting` before `UseAuthorization` runs, the authorization middleware has access to the full policy list for the matched endpoint without executing the action.
- In the old MVC-only model, authorization ran as an `IAuthorizationFilter` inside the MVC pipeline; in the endpoint routing model it runs as middleware, so it protects not just MVC actions but also Razor Pages, gRPC endpoints, SignalR hubs, and minimal API endpoints uniformly.
- Endpoint metadata is immutable at runtime and is built during application startup when `MapControllers()`, `MapRazorPages()`, or `MapGet()` (and similar) are called.
- `RequireAuthorization()` adds `IAuthorizeData` metadata programmatically during startup, behaving identically to the `[Authorize]` attribute from the middleware's perspective.

---

## Q26. What is the difference between calling `RequireAuthorization()` on an endpoint and applying the `[Authorize]` attribute?

What is the difference between calling `RequireAuthorization()` on an endpoint and applying the `[Authorize]` attribute?

**Answer:** Both `RequireAuthorization()` and `[Authorize]` add `IAuthorizeData` metadata to an endpoint; to the authorization middleware they are functionally identical. The only practical difference is location: `[Authorize]` is declared in source code on the controller or action, while `RequireAuthorization()` is applied at endpoint registration time in the route definition.

- `RequireAuthorization()` is the idiomatic approach for minimal APIs, where there are no controller classes and attributes cannot be applied: `app.MapGet("/secret", handler).RequireAuthorization("PolicyName")`.
- For MVC controllers and Razor Pages, `[Authorize]` is the common choice because it keeps security metadata co-located with the code it protects, making it visible to reviewers without inspecting the route configuration.
- `RequireAuthorization()` with no arguments requires an authenticated user, equivalent to `[Authorize]` with no parameters.
- You can mix both approaches in the same application; the authorization middleware simply collects all `IAuthorizeData` entries from the endpoint's metadata regardless of how they were registered.

---

## Q27. What are authentication scheme forwarding and passthrough, and when would you use them?

What are authentication scheme forwarding and passthrough, and when would you use them?

**Answer:** Scheme forwarding lets one scheme delegate a specific operation (authenticate, challenge, forbid, sign-in, sign-out) to a different scheme rather than handling it itself. Passthrough is the same concept applied to the authenticate operation: a scheme can report no result (`NoResult`) and allow the next scheme to try, effectively passing through.

- Forwarding is configured via properties on `AuthenticationSchemeOptions`: `ForwardAuthenticate`, `ForwardChallenge`, `ForwardForbid`, `ForwardSignIn`, `ForwardSignOut`, and `ForwardDefault`. Setting `ForwardAuthenticate = "Bearer"` means that when the cookie scheme is asked to authenticate, it immediately delegates to the JWT Bearer scheme instead.
- `ForwardDefaultSelector` is a function-based variant: `context => context.Request.Path.StartsWithSegments("/api") ? "Bearer" : "Cookies"`, which selects the target scheme dynamically per-request.
- A common pattern for mixed API/browser applications is to set the default scheme to Cookie but use `ForwardDefaultSelector` to route API paths to JWT Bearer, so each client type gets the appropriate authentication behavior without separate endpoint groups.
- Passthrough (returning `NoResult`) is used in custom handlers that should only activate for certain requests — for example, a scheme that only handles requests with a specific custom header — and step aside for others.

---

## Q28. What are the key tradeoffs between cookie-based authentication and JSON Web Token (JWT) bearer authentication in ASP.NET Core?

What are the key tradeoffs between cookie-based authentication and JSON Web Token (JWT) bearer authentication in ASP.NET Core?

**Answer:** Cookie-based authentication stores the session on the server side (or encrypts it into the cookie) and is managed automatically by the browser, making it a natural fit for server-rendered web applications. JSON Web Token (JWT) bearer authentication is stateless — all identity data is encoded in the token itself — making it suitable for APIs, mobile clients, and distributed microservice architectures.

| Dimension | Cookie Authentication | JWT Bearer Authentication |
|---|---|---|
| State | Server-managed session or encrypted cookie payload | Stateless — all data in the token |
| Storage | Browser cookie (automatic) | Client-managed (header, local storage, etc.) |
| Revocation | Easy — delete session or set cookie to expired | Hard — token is valid until expiry unless a denylist is maintained |
| CSRF risk | Yes — browsers auto-send cookies | No — client must attach the token explicitly |
| Cross-origin use | Restricted by same-site policies | Works across origins with CORS |
| Scalability | Session data may require shared storage in a farm | Scales naturally — no server-side state |

- JWT tokens cannot be revoked before their `exp` claim expires without maintaining a server-side denylist, which reintroduces statefulness; short token lifetimes combined with refresh tokens are the standard mitigation.
- Cookies are automatically sent by browsers including on cross-site requests, creating Cross-Site Request Forgery (CSRF) risk; ASP.NET Core's anti-forgery middleware mitigates this for form submissions.
- For applications that serve both browser UIs and API clients, a common architecture uses cookies for the browser-facing pages and JWT Bearer for the API surface, with appropriate `ForwardDefaultSelector` forwarding configured.

---

## Gotchas — Authentication & Authorization (Interview Traps)

---

#### Gotcha 1. Registering `UseAuthorization` before `UseAuthentication`

**Answer:** Many developers assume the order of `UseAuthentication` and `UseAuthorization` does not matter because both are middleware; in fact, registering them in the wrong order means `HttpContext.User` is still anonymous when authorization runs, so all protected endpoints fail silently or allow anonymous access depending on the fallback policy.

- The correct order in `Program.cs` is always `UseAuthentication()` followed by `UseAuthorization()`, with both placed after `UseRouting()` but before `MapControllers()` or equivalent.
- If you observe that valid credentials are ignored and every request is treated as anonymous, the first thing to check is the middleware registration order.

---

#### Gotcha 2. Confusing Challenge (401) with Forbid (403)

**Answer:** Developers often think a failed `[Authorize]` always produces a 401 Unauthorized; in reality, a 401 (Challenge) is produced only when the user is not authenticated at all, while a 403 (Forbid) is produced when the user is authenticated but lacks the required permission.

- Getting a 403 when you expected to be authorized means you are authenticated (your token or cookie is valid) but your claims do not satisfy the policy — check role claims, policy requirements, and claim types.
- Getting a 401 when you expected to be authenticated usually means the authentication handler returned `NoResult` or `Fail` — check that the token is present, not expired, and that the scheme name matches.

---

#### Gotcha 3. Creating a `ClaimsIdentity` without an `AuthenticationType`

**Answer:** A `ClaimsIdentity` constructed without an `AuthenticationType` reports `IsAuthenticated = false` even if it carries many claims, because `IsAuthenticated` is derived solely from whether `AuthenticationType` is non-empty. This causes the authorization middleware to treat the identity as anonymous and issue a challenge.

- Always pass the scheme name or a meaningful string as the second argument: `new ClaimsIdentity(claims, Scheme.Name)` or `new ClaimsIdentity(claims, "ApiKey")`.
- A handler that correctly builds a ticket but omits `AuthenticationType` will produce a principal that has claims but fails `[Authorize]` checks — a subtle bug that appears to work until authorization is enabled.

---

#### Gotcha 4. Assuming `[AllowAnonymous]` only affects the attribute's target

**Answer:** Developers sometimes believe that placing `[AllowAnonymous]` on a base controller or a derived action only disables authorization on that specific element; in fact `[AllowAnonymous]` completely bypasses all `IAuthorizeData` metadata on the endpoint, including policies inherited from parent classes and globally registered filters.

- This means a security-critical policy applied at the controller level is silently bypassed for any action decorated with `[AllowAnonymous]`, which can be an unintentional security hole if the attribute is applied carelessly.
- Always audit every `[AllowAnonymous]` usage and confirm that the endpoint genuinely requires no authentication before applying it.

---

#### Gotcha 5. JWT Bearer scheme does not support `SignIn` or `SignOut`

**Answer:** JWT Bearer is a stateless scheme that only implements the authenticate (and challenge/forbid) operations; calling `HttpContext.SignInAsync` against it throws an `InvalidOperationException` at runtime because the handler does not implement `IAuthenticationSignInHandler`.

- Sign-in and sign-out are meaningful only for stateful schemes such as Cookie authentication, which can persist and later revoke an identity on behalf of the server.
- In applications that issue JWTs, the token generation is typically done in a dedicated login endpoint using plain code (not the authentication pipeline's `SignIn`), and the client is responsible for storing and attaching the token on subsequent requests.

---
