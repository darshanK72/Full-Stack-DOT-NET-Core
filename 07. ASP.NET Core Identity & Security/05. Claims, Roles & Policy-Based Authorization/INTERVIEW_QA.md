# Claims, Roles & Policy-Based Authorization — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is a Claim in ASP.NET Core Identity, and what three pieces of data does every claim carry?](#q1-what-is-a-claim-in-aspnet-core-identity-and-what-three-pieces-of-data-does-every-claim-carry)
2. [Q2. How does a Claim differ from a Role conceptually, and what is the technical relationship between them?](#q2-how-does-a-claim-differ-from-a-role-conceptually-and-what-is-the-technical-relationship-between-them)
3. [Q3. What is a ClaimsIdentity, and what information does it encapsulate beyond the collection of claims?](#q3-what-is-a-claimsidentity-and-what-information-does-it-encapsulate-beyond-the-collection-of-claims)
4. [Q4. What is a ClaimsPrincipal, and how can a single user have multiple identities within one principal?](#q4-what-is-a-claimsprincipal-and-how-can-a-single-user-have-multiple-identities-within-one-principal)
5. [Q5. How does role-based authorization work with the [Authorize(Roles = "Admin")] attribute, and what does ASP.NET Core check under the hood?](#q5-how-does-role-based-authorization-work-with-the-authorizeroles-admin-attribute-and-what-does-aspnet-core-check-under-the-hood)
6. [Q6. How do you assign a role to a user using UserManager, and how is that role stored in the database?](#q6-how-do-you-assign-a-role-to-a-user-using-usermanager-and-how-is-that-role-stored-in-the-database)
7. [Q7. What is the difference between role-based authorization and claims-based authorization in terms of flexibility and design?](#q7-what-is-the-difference-between-role-based-authorization-and-claims-based-authorization-in-terms-of-flexibility-and-design)
8. [Q8. How do you implement claims-based authorization using the [Authorize] attribute and a custom policy?](#q8-how-do-you-implement-claims-based-authorization-using-the-authorize-attribute-and-a-custom-policy)
9. [Q9. What is AddAuthorization in ASP.NET Core, and what are the three main methods used inside AddPolicy to define a policy?](#q9-what-is-addauthorization-in-aspnet-core-and-what-are-the-three-main-methods-used-inside-addpolicy-to-define-a-policy)
10. [Q10. What is an IAuthorizationRequirement, and what is its role in the policy evaluation pipeline?](#q10-what-is-an-iauthorizationrequirement-and-what-is-its-role-in-the-policy-evaluation-pipeline)
11. [Q11. What is an IAuthorizationHandler, and how does it interact with IAuthorizationRequirement?](#q11-what-is-an-iauthorizationhandler-and-how-does-it-interact-with-iauthorizationrequirement)
12. [Q12. What is AuthorizationHandlerContext, and what three key properties does it expose to a handler?](#q12-what-is-authorizationhandlercontext-and-what-three-key-properties-does-it-expose-to-a-handler)
13. [Q13. How does ASP.NET Core evaluate a policy that contains multiple IAuthorizationRequirement objects — must all pass, or just one?](#q13-how-does-aspnet-core-evaluate-a-policy-that-contains-multiple-iauthorizationrequirement-objects-must-all-pass-or-just-one)
14. [Q14. What is resource-based authorization, and when should you use IAuthorizationService.AuthorizeAsync instead of [Authorize]?](#q14-what-is-resource-based-authorization-and-when-should-you-use-iauthorizationserviceauthorizeasync-instead-of-authorize)
15. [Q15. Show a minimal code example of a resource-based authorization check inside a controller action.](#q15-show-a-minimal-code-example-of-a-resource-based-authorization-check-inside-a-controller-action)
16. [Q16. What is IClaimsTransformation, and at what point in the request pipeline does it run?](#q16-what-is-iclaimstransformation-and-at-what-point-in-the-request-pipeline-does-it-run)
17. [Q17. How would you add a claim to a user's identity for every request, for example a "Subscription" claim fetched from a database?](#q17-how-would-you-add-a-claim-to-a-users-identity-for-every-request-for-example-a-subscription-claim-fetched-from-a-database)
18. [Q18. How do you combine a role check and a claims check in a single authorization policy?](#q18-how-do-you-combine-a-role-check-and-a-claims-check-in-a-single-authorization-policy)
19. [Q19. How does Minimal API authorization work, and how do you apply a named policy to a route?](#q19-how-does-minimal-api-authorization-work-and-how-do-you-apply-a-named-policy-to-a-route)
20. [Q20. What is the precedence rule when [Authorize] is applied at both the controller level and the action level?](#q20-what-is-the-precedence-rule-when-authorize-is-applied-at-both-the-controller-level-and-the-action-level)
21. [Q21. How does [AllowAnonymous] interact with [Authorize] at different scopes?](#q21-how-does-allowanonymous-interact-with-authorize-at-different-scopes)
22. [Q22. What is a fallback authorization policy, and how is it configured globally?](#q22-what-is-a-fallback-authorization-policy-and-how-is-it-configured-globally)
23. [Q23. What is RequireAssertion in AddPolicy, and when would you prefer it over a custom IAuthorizationHandler?](#q23-what-is-requireassertion-in-addpolicy-and-when-would-you-prefer-it-over-a-custom-iauthorizationhandler)
24. [Q24. Can an IAuthorizationHandler handle multiple requirement types? If so, how?](#q24-can-an-iauthorizationhandler-handle-multiple-requirement-types-if-so-how)
25. [Q25. What happens if no IAuthorizationHandler is registered for a given IAuthorizationRequirement?](#q25-what-happens-if-no-iauthorizationhandler-is-registered-for-a-given-iauthorizationrequirement)
26. [Q26. How does claims transformation affect performance, and what pattern should you use to avoid redundant work?](#q26-how-does-claims-transformation-affect-performance-and-what-pattern-should-you-use-to-avoid-redundant-work)
27. [Q27. What is the difference between authentication failure and authorization failure in terms of HTTP response codes?](#q27-what-is-the-difference-between-authentication-failure-and-authorization-failure-in-terms-of-http-response-codes)
28. [Q28. How would you unit-test a custom IAuthorizationHandler?](#q28-how-would-you-unit-test-a-custom-iauthorizationhandler)

---

## Q1. What is a Claim in ASP.NET Core Identity, and what three pieces of data does every claim carry?

**Concepts**
- Claim — a trusted assertion about a user
- Type — `ClaimTypes` constants or custom URI strings
- Value — always a string, no native numeric or date types
- Issuer — identifies the asserting authority; defaults to `"LOCAL AUTHORITY"`

**Answer**

A Claim is a statement about a user made by a trusted party — it asserts a fact such as the user's name, email address, department, or permission level. Every claim carries three pieces of data: a Type that identifies what kind of information it represents, a Value that holds the actual data, and an Issuer that names the party that made the assertion. The Type is typically a URI string; ASP.NET Core provides well-known constants in `System.Security.Claims.ClaimTypes`, such as `ClaimTypes.Email`, `ClaimTypes.Name`, and `ClaimTypes.Role`, but custom strings are fully supported. The Value is always a string, so numeric or date values must be serialized — an age claim stores `"30"` rather than the integer 30. The Issuer defaults to the string `"LOCAL AUTHORITY"` when not set explicitly; external identity providers such as Azure AD or Google supply their own issuer URIs, allowing the application to distinguish which system made the assertion. Claims are immutable once created, which keeps identity data trustworthy throughout a request's lifetime.

---

## Q2. How does a Claim differ from a Role conceptually, and what is the technical relationship between them?

**Concepts**
- Role — coarse-grained binary membership
- Claim — fine-grained key-value attribute
- Roles implemented as `ClaimTypes.Role` claims under the hood
- `RoleClaimType` on `ClaimsIdentity` — configurable role claim selector

**Answer**

Conceptually, a Role is a coarse-grained bucket — a user either belongs to "Admin" or they do not — while a Claim is a fine-grained key/value attribute that can express nuanced facts such as `SubscriptionTier=Gold` or `Department=Finance`. Despite this conceptual difference, roles are implemented as claims at the technical level: a role assignment is stored as a claim with `Type = ClaimTypes.Role` and `Value = "Admin"`. This means `User.IsInRole("Admin")` and checking for a role claim with value `"Admin"` are equivalent operations; the framework uses the `RoleClaimType` property of `ClaimsIdentity` to know which claim type to treat as a role. The practical implication is that there is no separate storage mechanism for role claims versus other claims — they all live in the `ClaimsPrincipal` and in the `AspNetUserClaims` / `AspNetUserRoles` tables, with roles ultimately surfacing as claims on the identity object.

| Dimension | Role | Claim |
|---|---|---|
| Granularity | Coarse (membership binary) | Fine (key/value attribute) |
| Typical use | Access category | Specific fact or permission |
| Storage | AspNetRoles + AspNetUserRoles | AspNetUserClaims |
| Technical form | Claim with type ClaimTypes.Role | Any claim type and value |

---

## Q3. What is a ClaimsIdentity, and what information does it encapsulate beyond the collection of claims?

**Concepts**
- `ClaimsIdentity` — single verified identity with authentication metadata
- `AuthenticationType` — non-null/non-empty string gates `IsAuthenticated`
- `NameClaimType` and `RoleClaimType` — configurable claim resolvers
- Multiple `ClaimsIdentity` instances on one principal

**Answer**

A `ClaimsIdentity` represents a single verified identity for a user — one way of knowing who the user is, such as through a cookie, a token, or an external provider. Beyond holding the collection of `Claim` objects, it carries the `AuthenticationType` string, the `IsAuthenticated` flag, and the `NameClaimType` and `RoleClaimType` configuration properties. `AuthenticationType` is a non-null, non-empty string that signals the identity was verified; when this property is null or empty, `IsAuthenticated` returns `false`, which is the pattern used to represent anonymous identities. `NameClaimType` tells the identity which claim type to use when resolving the `Name` property — by default `ClaimTypes.Name`, but it can be configured to match any claim type an external provider emits, such as `"sub"` in JWT scenarios. `RoleClaimType` plays the same role for `IsInRole` checks — it defaults to `ClaimTypes.Role` but can be overridden, which is why JWT Bearer authentication maps the `"roles"` claim using this property during token validation. A single user principal can hold multiple `ClaimsIdentity` objects, allowing layered or federated authentication scenarios.

---

## Q4. What is a ClaimsPrincipal, and how can a single user have multiple identities within one principal?

**Concepts**
- `ClaimsPrincipal` — top-level security context exposed as `HttpContext.User`
- `Claims` property — union across all contained identities
- Multiple schemes producing multiple identities in one session
- Anonymous principal — always non-null, carries one unauthenticated identity

**Answer**

A `ClaimsPrincipal` is the top-level security context object that represents the user for an HTTP request, exposed as `HttpContext.User`. It is a container that holds one or more `ClaimsIdentity` instances, and its `Claims` property is the union of all claims across all identities it holds. A single user can legitimately carry multiple identities when they authenticate through more than one mechanism in the same session — for example, a cookie identity from the application's own login flow and a separate identity populated from an external OAuth provider like Google. The principal's own `IsInRole` and `FindFirst` methods aggregate across all identities, so a role or claim present in any one identity is considered to belong to the principal as a whole. ASP.NET Core sets `HttpContext.User` to a `ClaimsPrincipal` with a single anonymous `ClaimsIdentity` (no `AuthenticationType`) for unauthenticated requests, so code can always call `HttpContext.User` safely without null checks. When multiple authentication schemes are applied, middleware can merge their results into one principal or keep them as separate identities depending on how the scheme is configured.

---

## Q5. How does role-based authorization work with the [Authorize(Roles = "Admin")] attribute, and what does ASP.NET Core check under the hood?

**Concepts**
- `[Authorize(Roles)]` — calls `User.IsInRole` under the hood
- Comma-separated roles — OR semantics within a single attribute
- Stacked `[Authorize]` attributes — AND semantics
- 401 vs 403 — unauthenticated vs authenticated but lacking role

**Answer**

The `[Authorize(Roles = "Admin")]` attribute instructs the authorization middleware to call `User.IsInRole("Admin")` before allowing the request to proceed. Under the hood, `IsInRole` searches the user's `ClaimsPrincipal` for a claim whose type matches the `RoleClaimType` of any contained `ClaimsIdentity` and whose value equals `"Admin"`. Multiple roles can be specified as a comma-separated string — `[Authorize(Roles = "Admin,Manager")]` — and in this case the check passes if the user belongs to any of the listed roles (OR semantics). To enforce AND semantics where the user must have both roles, stacking two separate `[Authorize]` attributes is required: `[Authorize(Roles = "Admin")] [Authorize(Roles = "Manager")]`. If the user is not authenticated at all, the framework returns a 401 Unauthorized response before the role check runs; if the user is authenticated but lacks the required role, it returns 403 Forbidden. Because roles are stored as claims, JWT Bearer authentication maps the token's `"roles"` claim array to individual `ClaimTypes.Role` claims during token validation, making `IsInRole` work identically regardless of whether the session is cookie- or token-based.

---

## Q6. How do you assign a role to a user using UserManager, and how is that role stored in the database?

**Concepts**
- `UserManager.AddToRoleAsync` — requires role pre-existing in `AspNetRoles`
- `AspNetUserRoles` join table — user ID to role ID mapping
- Sign-in process converts role rows into `ClaimTypes.Role` claims
- `AddLoginAsync` alternative for roles stored directly in `AspNetUserClaims`

**Answer**

Roles are assigned via `UserManager<TUser>.AddToRoleAsync(user, "Admin")`, which requires the role to exist in the `AspNetRoles` table first (created via `RoleManager.CreateAsync`), and the assignment is then stored as a row in the `AspNetUserRoles` join table, linking the user's ID to the role's ID. When the user authenticates, ASP.NET Core Identity's sign-in process reads the `AspNetUserRoles` table and converts each assigned role into a `Claim` with type `ClaimTypes.Role`, adding it to the user's `ClaimsIdentity` before issuing the authentication cookie or populating the token. `UserManager.GetRolesAsync(user)` retrieves role names from the database as a list of strings, while `UserManager.IsInRoleAsync(user, "Admin")` performs the same check against the database rather than the current `ClaimsPrincipal`. Using `UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, "Admin"))` bypasses the roles tables and adds the role directly to `AspNetUserClaims`; this approach works for scenarios where the formal role management infrastructure is not needed. Roles must be created before assignment; attempting to add a user to a non-existent role via `AddToRoleAsync` returns an error result rather than silently creating the role.

---

## Q7. What is the difference between role-based authorization and claims-based authorization in terms of flexibility and design?

**Concepts**
- Role-based — binary group membership, coarse-grained
- Claims-based — value-based rules, aligns with OIDC and JWT identity model
- `RequireClaim` vs `RequireRole` — design choice based on rule granularity
- Claims preferred in modern federated architectures

**Answer**

Role-based authorization assigns users to named groups and checks group membership, making it simple but coarse-grained; claims-based authorization evaluates specific attributes about a user, making it more expressive and better suited to fine-grained rules. The two are not mutually exclusive — claims-based policies can require a role as one of their criteria. Role checks work well when access control maps cleanly to job functions such as "only Admins can delete records," but they break down when the rule depends on a value rather than simple membership — for example, "users with SubscriptionTier=Premium can access this feature." Claims-based authorization handles value-based rules natively because a claim carries both a type and a value; a policy can require `RequireClaim("SubscriptionTier", "Premium")` without needing to create separate roles for every tier value. Role-based authorization is easier to manage through a UI since roles are named entities stored in the database and can be assigned by administrators, while claims often require custom provisioning logic. From a design perspective, claims-based authorization is preferred in modern architectures because it aligns with the identity model used by external providers and standards like OpenID Connect and JWT, where identity information arrives as a set of claims rather than a list of group memberships.

---

## Q8. How do you implement claims-based authorization using the [Authorize] attribute and a custom policy?

**Concepts**
- Named policy defined in `AddAuthorization` — referenced by `[Authorize(Policy = "...")]`
- `RequireClaim` with multiple acceptable values — OR semantics within a value list
- Policy reuse across controllers and actions
- `IAuthorizationRequirement` + `IAuthorizationHandler` for complex rules

**Answer**

Claims-based authorization is implemented by defining a named policy in `Program.cs` that specifies which claims the user must possess, then referencing that policy by name in the `[Authorize(Policy = "PolicyName")]` attribute on a controller or action. The policy engine evaluates the user's `ClaimsPrincipal` against the policy's requirements at request time. Defining the policy uses `builder.Services.AddAuthorization(options => options.AddPolicy("PremiumUser", p => p.RequireClaim("SubscriptionTier", "Premium")))`, where `RequireClaim` creates an implicit `ClaimsAuthorizationRequirement` behind the scenes. `RequireClaim("SubscriptionTier", "Premium", "Gold")` accepts multiple acceptable values and passes if the user has the claim with any of those values — OR semantics within the value list. Applying the policy to an action is as simple as `[Authorize(Policy = "PremiumUser")]`; the same policy name can be reused across any number of controllers and actions without duplicating the logic. For rules that cannot be expressed with `RequireClaim` alone, the policy can reference a custom `IAuthorizationRequirement` and a paired `IAuthorizationHandler`, giving full programmatic control over the evaluation logic.

---

## Q9. What is AddAuthorization in ASP.NET Core, and what are the three main methods used inside AddPolicy to define a policy?

**Concepts**
- `AddAuthorization` — registers policy infrastructure and named policies
- `RequireClaim` — `ClaimsAuthorizationRequirement`
- `RequireRole` — `RolesAuthorizationRequirement`
- `RequireAssertion` — `AssertionRequirement` with inline lambda

**Answer**

`AddAuthorization` is the service-registration method called in `Program.cs` that registers the authorization infrastructure and allows the developer to configure named policies via an `AuthorizationOptions` delegate. The three primary methods used inside `AddPolicy` are `RequireClaim`, `RequireRole`, and `RequireAssertion`. `RequireClaim(type)` or `RequireClaim(type, values)` adds a `ClaimsAuthorizationRequirement` that passes when the user's principal contains a claim of the specified type, optionally matching one of the specified values. `RequireRole(roles)` adds a `RolesAuthorizationRequirement` that passes when the user belongs to any of the specified roles; it is equivalent to calling `User.IsInRole` but expressed as a composable policy requirement. `RequireAssertion(context => bool)` adds an `AssertionRequirement` that evaluates an inline lambda giving access to the full `AuthorizationHandlerContext`, so arbitrary logic can be expressed without creating a separate handler class. Additional methods include `RequireAuthenticatedUser()` which ensures the user is authenticated, `RequireUserName(name)`, and `AddRequirements(requirement)` for attaching custom `IAuthorizationRequirement` objects with separate handlers.

---

## Q10. What is an IAuthorizationRequirement, and what is its role in the policy evaluation pipeline?

**Concepts**
- `IAuthorizationRequirement` — marker interface, intentionally empty
- Requirements as data — logic lives in paired `IAuthorizationHandler`
- Policy as a collection of requirements — ALL must succeed
- Built-in requirements that implement both interfaces for convenience

**Answer**

`IAuthorizationRequirement` is a marker interface with no members; it identifies a class as a discrete access rule that the authorization system must evaluate. A policy is a collection of `IAuthorizationRequirement` objects, and every requirement in the collection must be satisfied for the policy to pass. The interface itself carries no logic — it is intentionally empty so that requirements are plain data transfer objects that describe what must be true, while the actual evaluation logic lives in a paired `IAuthorizationHandler`. This separation of requirement from handler means the same requirement class can be evaluated by multiple handlers — for example, one handler for production and a stub handler in tests — and multiple requirements can be evaluated by the same handler. Examples of built-in requirements include `ClaimsAuthorizationRequirement`, `RolesAuthorizationRequirement`, and `AssertionRequirement`, all of which implement both `IAuthorizationRequirement` and `IAuthorizationHandler` themselves for convenience. For custom requirements, the pattern is to define a plain class implementing `IAuthorizationRequirement` with properties representing the access rule parameters, and register a separate handler that knows how to evaluate it.

---

## Q11. What is an IAuthorizationHandler, and how does it interact with IAuthorizationRequirement?

**Concepts**
- `AuthorizationHandler<TRequirement>` base class — typed routing to `HandleRequirementAsync`
- `context.Succeed(requirement)` vs `context.Fail()` — distinct signals
- Not calling either — requirement remains unsatisfied, policy fails
- DI registration required for the framework to discover handlers

**Answer**

`IAuthorizationHandler` defines a single method — `HandleAsync(AuthorizationHandlerContext context)` — that the authorization pipeline calls to evaluate one or more requirements. The handler inspects the user's claims, the resource, or any other data and signals success or failure by calling methods on the `AuthorizationHandlerContext`. The strongly-typed base class `AuthorizationHandler<TRequirement>` is the recommended starting point; it implements `IAuthorizationHandler` and routes the call to `HandleRequirementAsync(context, requirement)`, filtering so the method only runs for requirements of the matching type. Inside `HandleRequirementAsync`, calling `context.Succeed(requirement)` marks that requirement as satisfied; calling `context.Fail()` marks the entire authorization as failed regardless of other handlers; doing nothing leaves the requirement unsatisfied, which means the policy fails unless another handler succeeds it. Handlers must be registered in the DI container — typically as scoped or transient services — so the authorization service can discover and invoke them during policy evaluation.

```csharp
public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public int MinimumAge { get; }
    public MinimumAgeRequirement(int age) => MinimumAge = age;
}

public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
    {
        var dob = context.User.FindFirst("DateOfBirth")?.Value;
        if (dob is not null && DateTime.TryParse(dob, out var birth)
            && DateTime.Today.Year - birth.Year >= requirement.MinimumAge)
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
```

---

## Q12. What is AuthorizationHandlerContext, and what three key properties does it expose to a handler?

**Concepts**
- `User` — `ClaimsPrincipal` for the current request
- `Requirements` and `PendingRequirements` — full list vs unsatisfied subset
- `Resource` — optional entity passed by imperative `AuthorizeAsync` callers
- `HasFailed` — reflects an explicit `context.Fail()` call by any handler

**Answer**

`AuthorizationHandlerContext` is the object passed into every `IAuthorizationHandler.HandleAsync` call; it carries all the information the handler needs to make its decision. The three key properties it exposes are `User`, `Requirements`, and `Resource`. `User` is the `ClaimsPrincipal` for the current request, giving the handler access to all claims, roles, and identity information needed to evaluate the requirement. `Requirements` is the read-only collection of `IAuthorizationRequirement` objects that the current policy contains; the `PendingRequirements` sub-property filters this to requirements that have not yet been succeeded, which is useful when one handler processes multiple types. `Resource` is an optional `object` that the caller can pass when triggering authorization imperatively via `IAuthorizationService.AuthorizeAsync`; it typically holds the entity being accessed — such as a `Document` or `Order` object — so the handler can apply ownership or state rules that depend on the specific resource. `HasFailed` is a boolean property that reflects whether `context.Fail()` has already been called by any handler; a well-behaved handler can check this property and short-circuit its own logic.

---

## Q13. How does ASP.NET Core evaluate a policy that contains multiple IAuthorizationRequirement objects — must all pass, or just one?

**Concepts**
- AND semantics across requirements — all must succeed
- OR semantics within handler tier — multiple handlers for same requirement
- `context.Fail()` — hard deny overriding any subsequent `Succeed`
- `RequireAssertion` with inline OR for cross-requirement OR logic

**Answer**

When a policy contains multiple `IAuthorizationRequirement` objects, ALL of them must be satisfied for the policy to pass — the evaluation uses AND semantics across requirements. If any single requirement remains unsatisfied or is explicitly failed, the policy fails and the request is denied. This design means that building a policy with both `RequireRole("Admin")` and `RequireClaim("Department", "Finance")` demands that the user is both an Admin and in the Finance department; there is no built-in OR across requirements at the policy level. To achieve OR semantics across requirements, the correct approach is to define a single requirement whose handler internally evaluates the OR logic, or to use `RequireAssertion` with an inline OR expression. Multiple handlers can be registered for the same requirement type, and in that case the requirement is considered satisfied if any one of those handlers calls `context.Succeed` — so OR semantics exist within the handler tier for a single requirement, while AND semantics apply across different requirements in the same policy. Calling `context.Fail()` inside any handler is a hard failure that cannot be overridden by a successful handler for the same requirement; it signals an explicit deny rather than a mere failure to succeed.

---

## Q14. What is resource-based authorization, and when should you use IAuthorizationService.AuthorizeAsync instead of [Authorize]?

**Concepts**
- Resource-based authorization — decision depends on the specific loaded entity
- `[Authorize]` runs before the action — no access to the resource
- `IAuthorizationService.AuthorizeAsync` — imperative check with resource parameter
- `AuthorizationHandlerContext.Resource` — entity passed to the handler

**Answer**

Resource-based authorization is a pattern where the authorization decision depends on the specific resource being accessed — not just the user's identity in isolation. The `[Authorize]` attribute runs before the action method executes and has no access to the resource, so `IAuthorizationService.AuthorizeAsync` must be used imperatively inside the action when the resource must be loaded first. A typical example is an "Edit Document" action: the framework must first fetch the document from the database, then check whether the current user is the owner or has the required role for that document specifically, not just for documents in general. `IAuthorizationService.AuthorizeAsync(User, resource, policyName)` accepts the resource as a parameter, which is then available on `AuthorizationHandlerContext.Resource` inside the handler; this is the mechanism that enables ownership checks. The method returns an `AuthorizationResult` with a `Succeeded` property, and the action is responsible for returning a 403 Forbidden or 404 Not Found response if authorization fails. Because the logic runs inside the action, it can combine database lookups, business rules, and authorization in a single workflow, making it the preferred pattern for document-level, row-level, or state-dependent access control.

---

## Q15. Show a minimal code example of a resource-based authorization check inside a controller action.

**Concepts**
- `IAuthorizationService` injected into controller
- Load resource first, then authorize against it
- `Forbid()` vs `NotFound()` — intentional choice for information disclosure

**Answer**

The pattern requires injecting `IAuthorizationService`, loading the resource, then calling `AuthorizeAsync` with both the resource and the policy name before proceeding. If the result is not successful, the action returns an appropriate error response.

```csharp
public class DocumentController : Controller
{
    private readonly IAuthorizationService _authz;
    private readonly IDocumentRepository _repo;

    public DocumentController(IAuthorizationService authz, IDocumentRepository repo)
        => (_authz, _repo) = (authz, repo);

    public async Task<IActionResult> Edit(int id)
    {
        var doc = await _repo.GetAsync(id);
        if (doc is null) return NotFound();

        var result = await _authz.AuthorizeAsync(User, doc, "DocumentOwner");
        if (!result.Succeeded) return Forbid();

        return View(doc);
    }
}
```

The `"DocumentOwner"` policy has a corresponding `IAuthorizationHandler` whose `HandleRequirementAsync` receives the `doc` object via `context.Resource`, cast to the appropriate type. Returning `Forbid()` (403) rather than `NotFound()` (404) is a deliberate choice that reveals the resource exists but access is denied; returning 404 for unauthorized access is a common security hardening technique to avoid exposing the existence of records. The policy and its handler are registered in `Program.cs` just like any other policy; resource-based authorization requires no special registration beyond what standard policy-based authorization already needs.

---

## Q16. What is IClaimsTransformation, and at what point in the request pipeline does it run?

**Concepts**
- `IClaimsTransformation.TransformAsync` — post-authentication claim enrichment
- Runs inside authorization middleware, not authentication middleware
- One active implementation at a time — last-registered wins
- Per-request execution — must be efficient

**Answer**

`IClaimsTransformation` is an interface with a single method — `TransformAsync(ClaimsPrincipal principal)` — that allows the application to add, remove, or modify claims on the user's principal after authentication has completed and before the authorization middleware evaluates policies. It runs as part of the authorization middleware's pipeline, not the authentication middleware's pipeline, so the identity is already established by the time it executes. The method receives the already-authenticated `ClaimsPrincipal` and must return a `ClaimsPrincipal` — either the same instance with modifications or a new one wrapping the original. It is invoked on every request where the user is authenticated and the authorization middleware runs, which means the implementation must be efficient or use caching to avoid repeated expensive operations such as database round trips. A common use case is loading dynamic claims from a database — for example, fetching a user's current subscription tier or tenant ID — that are not stored in the authentication cookie or JWT but are needed for authorization decisions. Only one `IClaimsTransformation` implementation can be active at a time since the DI container resolves the last-registered implementation; to chain multiple transformers, they must be composed manually or coordinated within a single implementation class.

---

## Q17. How would you add a claim to a user's identity for every request, for example a "Subscription" claim fetched from a database?

**Concepts**
- `IClaimsTransformation` registered as scoped service
- Sentinel claim guard — prevent duplicate claims on re-entrant calls
- `principal.Clone()` before modification
- Scoped lifetime aligned with request-scoped database context

**Answer**

The correct mechanism is implementing `IClaimsTransformation`, registering it as a scoped service, and inside `TransformAsync` querying the database for the subscription data and adding the claim to a cloned identity. The implementation should guard against adding duplicate claims on re-entrant calls.

```csharp
public class SubscriptionClaimsTransformer : IClaimsTransformation
{
    private readonly ISubscriptionService _svc;
    public SubscriptionClaimsTransformer(ISubscriptionService svc) => _svc = svc;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.HasClaim(c => c.Type == "SubscriptionTier"))
            return principal; // already transformed this request

        var tier = await _svc.GetTierAsync(principal.Identity!.Name!);
        var clone = principal.Clone();
        ((ClaimsIdentity)clone.Identity!).AddClaim(new Claim("SubscriptionTier", tier));
        return clone;
    }
}
```

Registering the transformer uses `builder.Services.AddScoped<IClaimsTransformation, SubscriptionClaimsTransformer>()`; scoped lifetime means one instance per request, which matches the request-scoped database context used for the lookup. The guard clause `if (principal.HasClaim(...)) return principal` prevents adding the same claim twice if the transformer is invoked more than once in a single request, which can happen in some middleware configurations. Cloning the principal before modifying it is important when the original principal is immutable or shared; modifying it in place can cause unexpected side effects if the principal is reused or if the authorization middleware calls the transformer more than once in a request.

---

## Q18. How do you combine a role check and a claims check in a single authorization policy?

**Concepts**
- Chained requirements — AND semantics by default
- `RequireRole` + `RequireClaim` — independent built-in requirements both evaluated
- `RequireAssertion` for OR logic between role and claim

**Answer**

Requirements are chained on the `AuthorizationPolicyBuilder` inside `AddPolicy`; each method call adds another `IAuthorizationRequirement` to the policy's list, and since all requirements must pass, the result is an AND combination. Calling `.RequireRole("Admin").RequireClaim("Department", "Finance")` creates a policy that demands both role membership and a specific claim value simultaneously.

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("FinanceAdmin", policy => policy
        .RequireRole("Admin")
        .RequireClaim("Department", "Finance"));
});
```

This policy passes only when the user is in the `"Admin"` role and has a `"Department"` claim with value `"Finance"` — both conditions are enforced independently via their respective built-in requirements. The role check and the claim check each have their own underlying `IAuthorizationRequirement` and handler; the policy engine evaluates them all and requires each to succeed. For more complex combinations involving OR logic between a role and a claim, `RequireAssertion` provides the most direct path: `policy.RequireAssertion(ctx => ctx.User.IsInRole("Admin") || ctx.User.HasClaim("SpecialAccess", "true"))`. Policies defined this way can be applied using `[Authorize(Policy = "FinanceAdmin")]` and are reusable across any controller or action in the application.

---

## Q19. How does Minimal API authorization work, and how do you apply a named policy to a route?

**Concepts**
- `.RequireAuthorization()` on route builder — replaces `[Authorize]`
- `.AllowAnonymous()` on route — overrides fallback and global policy
- Interoperability with `AddAuthorization` named policies

**Answer**

In Minimal APIs, authorization is applied directly to route definitions using the `.RequireAuthorization()` extension method on the route builder; this replaces the attribute-based approach used in controller-based APIs. The method accepts no argument (requiring only that the user is authenticated), a policy name, or an `AuthorizeData` object for finer control.

```csharp
app.MapGet("/premium-content", () => "Hello, subscriber!")
   .RequireAuthorization("PremiumUser");

app.MapGet("/admin-only", () => "Admin area")
   .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
```

`.RequireAuthorization()` with no argument applies the default authorization policy, which by convention requires authenticated users; this is equivalent to placing `[Authorize]` on a controller action. Policies defined in `AddAuthorization` are referenced by their string name and evaluated by the same policy engine that powers attribute-based authorization, so custom requirements and handlers work identically. `.AllowAnonymous()` is the Minimal API counterpart of `[AllowAnonymous]` and overrides any authorization requirements on a route, including a fallback policy applied globally. Minimal APIs respect the `DefaultPolicy`, `FallbackPolicy`, and named policies from the authorization options object, making them fully interoperable with controller-based authorization in the same application.

---

## Q20. What is the precedence rule when [Authorize] is applied at both the controller level and the action level?

**Concepts**
- Additive accumulation — not replacement
- Both sets of requirements must be satisfied simultaneously
- `[AllowAnonymous]` — breaks the additive chain entirely

**Answer**

When `[Authorize]` appears on both the controller and an action method, both sets of requirements apply — they are combined, not overridden. The action-level attribute does not replace the controller-level one; instead, the user must satisfy all requirements from both levels for the request to be authorized. This means a controller decorated with `[Authorize(Roles = "Staff")]` and an action further decorated with `[Authorize(Policy = "FinanceAdmin")]` requires the user to be in the `Staff` role and to satisfy the `FinanceAdmin` policy simultaneously. The combination is additive in the direction of more restriction: each `[Authorize]` attribute independently passes its requirements to the authorization pipeline, and the pipeline requires every independently registered requirement set to succeed. `[AllowAnonymous]` on the action breaks this additive chain entirely — it bypasses all `[Authorize]` attributes on both the controller and the action, regardless of how many are stacked. This behavior differs from some other frameworks where action-level attributes override controller-level ones; in ASP.NET Core the mental model is accumulation, not replacement.

---

## Q21. How does [AllowAnonymous] interact with [Authorize] at different scopes?

**Concepts**
- `[AllowAnonymous]` — hard override checked before any policy evaluation
- `IAllowAnonymous` metadata — detected by authorization middleware
- Imperative `IAuthorizationService.AuthorizeAsync` — unaffected by `[AllowAnonymous]`

**Answer**

`[AllowAnonymous]` unconditionally bypasses all `[Authorize]` requirements, regardless of where in the attribute hierarchy they are defined. An action marked `[AllowAnonymous]` will be accessible to unauthenticated users even if the containing controller carries multiple `[Authorize]` attributes. The authorization middleware checks for the presence of `IAllowAnonymous` metadata on the endpoint before evaluating any policies; if it is present, the middleware skips authorization entirely for that endpoint. This makes `[AllowAnonymous]` a hard override — not a scoped one — which means it cannot be "cancelled" by a more specific `[Authorize]` further down the attribute stack or by a fallback policy set in `AuthorizationOptions`. A common pattern is to mark an entire controller `[Authorize]` for protection by default and then selectively decorate public actions with `[AllowAnonymous]` — for example, `Login` and `Register` endpoints — keeping the default secure. `[AllowAnonymous]` does not affect `IAuthorizationService.AuthorizeAsync` calls made imperatively inside action methods; those calls always evaluate their policy regardless of what attributes are present on the action.

---

## Q22. What is a fallback authorization policy, and how is it configured globally?

**Concepts**
- `FallbackPolicy` — applies to endpoints with no other authorization metadata
- Distinct from `DefaultPolicy` — `DefaultPolicy` applies to bare `[Authorize]`
- Static files not covered — served before authorization middleware

**Answer**

A fallback authorization policy is a policy that the authorization middleware applies to every endpoint that has no other authorization metadata — endpoints not decorated with `[Authorize]`, `[AllowAnonymous]`, or `.RequireAuthorization()`. It is configured via `AuthorizationOptions.FallbackPolicy` in the `AddAuthorization` call.

```csharp
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```

Setting the fallback policy to `RequireAuthenticatedUser()` effectively makes the entire application require authentication by default, with only endpoints that explicitly declare `[AllowAnonymous]` or `.AllowAnonymous()` remaining public. This is distinct from the `DefaultPolicy`, which is the policy applied by a bare `[Authorize]` attribute with no role or policy name specified; the default policy also defaults to requiring an authenticated user but only applies when an `[Authorize]` attribute is present. The fallback policy is particularly useful in security-conscious APIs where the developer wants to ensure no endpoint is accidentally left unprotected due to a missing attribute. Static files served by `UseStaticFiles` are not subject to the fallback policy because they are served before the authorization middleware runs; separate configuration is required to protect static files.

---

## Q23. What is RequireAssertion in AddPolicy, and when would you prefer it over a custom IAuthorizationHandler?

**Concepts**
- `RequireAssertion` — inline lambda as an `AssertionRequirement`
- Cannot inject DI services — lambda runs outside DI scope
- Custom `IAuthorizationHandler` preferred for complex, reusable, or service-dependent logic

**Answer**

`RequireAssertion` is a method on `AuthorizationPolicyBuilder` that accepts a predicate lambda with access to the `AuthorizationHandlerContext`, creating an inline requirement without requiring a separate class. It is preferred over a custom `IAuthorizationHandler` when the rule is simple, application-specific, and unlikely to be reused in other policies. The lambda receives the `AuthorizationHandlerContext` as its parameter and returns `bool` (or `Task<bool>` for the async overload); returning `true` signals success, `false` signals failure for that requirement. `RequireAssertion` is best for one-off rules that are clear when read inline — for example, `policy.RequireAssertion(ctx => ctx.User.HasClaim("AccountStatus", "Active") || ctx.User.IsInRole("Admin"))` is readable and does not warrant a dedicated class. A custom `IAuthorizationHandler` is preferred when the logic is complex, requires injected services such as a database repository, is reused across multiple policies, or needs to be independently unit-tested with mocked dependencies. `RequireAssertion` cannot easily inject services because the lambda runs within the framework's pipeline, not through the DI container; service-dependent authorization logic belongs in a handler registered as a DI service.

---

## Q24. Can an IAuthorizationHandler handle multiple requirement types? If so, how?

**Concepts**
- Implementing `IAuthorizationHandler` directly — bypasses generic type filter
- `context.PendingRequirements` iteration — process each recognized type
- Shared service dependencies — single lookup serving multiple requirements

**Answer**

Yes, a single `IAuthorizationHandler` can handle multiple requirement types by implementing `IAuthorizationHandler` directly rather than the generic `AuthorizationHandler<TRequirement>` base class, and iterating over `context.PendingRequirements` to identify and process each requirement type it knows about.

```csharp
public class MultiHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var req in context.PendingRequirements.ToList())
        {
            if (req is MinimumAgeRequirement age && MeetsAge(context.User, age))
                context.Succeed(req);
            else if (req is ActiveAccountRequirement && IsActive(context.User))
                context.Succeed(req);
        }
        return Task.CompletedTask;
    }
}
```

This pattern is useful when two or more requirements share state or service dependencies and combining them into one handler avoids redundant lookups — for example, fetching a user profile record once and using it to evaluate multiple requirements. The handler must be registered once in the DI container as `IAuthorizationHandler`; the framework's `DefaultAuthorizationService` discovers all registered `IAuthorizationHandler` implementations and invokes each one during policy evaluation. Requirements the handler does not recognize should be left untouched (neither succeeded nor failed) so that other handlers registered for those types still get a chance to evaluate them.

---

## Q25. What happens if no IAuthorizationHandler is registered for a given IAuthorizationRequirement?

**Concepts**
- Missing handler — requirement never succeeded, policy fails silently with 403
- No startup exception — bug is invisible at launch
- `AuthorizationResult.Failure.FailedRequirements` — diagnostic inspection

**Answer**

If no `IAuthorizationHandler` is registered that can handle a particular `IAuthorizationRequirement`, that requirement will never have `context.Succeed` called on it, so it remains unsatisfied and the policy fails, resulting in a 403 Forbidden response. There is no exception or error log; the requirement silently fails, which is a common source of bugs during development where a developer adds a new requirement class to a policy but forgets to register the corresponding handler. The authorization framework does not throw an exception for missing handlers by design, because handlers are optional participants — the absence of a success signal is treated as authorization not granted, which is the safe default. Diagnosing the issue requires inspecting `AuthorizationResult.Failure.FailedRequirements` in logging middleware or by enabling verbose authorization event logging via `IAuthorizationEventHandler` to surface which specific requirements were left unsatisfied. The fix is always to register the handler: `builder.Services.AddScoped<IAuthorizationHandler, MyRequirementHandler>()` ensures the framework can discover and invoke it during policy evaluation.

---

## Q26. How does claims transformation affect performance, and what pattern should you use to avoid redundant work?

**Concepts**
- `TransformAsync` called on every authenticated request
- Sentinel claim guard — short-circuit if already transformed
- `IMemoryCache` / `IDistributedCache` for database-backed claims
- `principal.Clone()` before mutation to avoid shared-reference side effects

**Answer**

`IClaimsTransformation.TransformAsync` is called on every authenticated request by the authorization middleware, which means any work inside it — especially database queries — is repeated per request and can become a significant performance bottleneck under load. The standard mitigation is to check whether the transformation has already been applied and short-circuit immediately if so. The simplest guard is a sentinel claim: before doing any expensive work, check `principal.HasClaim(c => c.Type == "TransformationApplied")` and return early if it exists; add this sentinel claim at the end of the transformation logic. For more elaborate scenarios, caching the transformed result using `IMemoryCache` or `IDistributedCache` keyed by the user's identifier and a short absolute expiration avoids database round trips on every request while keeping claims reasonably fresh. Cloning the principal before adding claims (`principal.Clone()`) rather than mutating the incoming object is important because modifying the original can cause unexpected behavior if the principal is reused or if the authorization middleware calls the transformer more than once in a request. `IClaimsTransformation` runs inside the authorization middleware, which runs after `UseAuthentication`, so it has access to the authenticated identity but runs on every request that passes through authorization.

---

## Q27. What is the difference between authentication failure and authorization failure in terms of HTTP response codes?

**Concepts**
- 401 Unauthorized — identity unknown, re-authenticate
- 403 Forbidden — identity known, permission denied
- 404 Not Found — intentional privacy-preserving substitution for 403
- `ChallengeAsync` vs `ForbidAsync` — different scheme response methods

**Answer**

Authentication failure occurs when the system cannot verify who the user is — the request carries no credentials or invalid credentials — and results in a 401 Unauthorized response. Authorization failure occurs when the user's identity is established but they lack permission to access the requested resource, resulting in a 403 Forbidden response.

| Scenario | Status Code | Meaning |
|---|---|---|
| No credentials / expired token | 401 Unauthorized | "Who are you?" — identity unknown |
| Valid identity, insufficient permission | 403 Forbidden | "I know who you are, but you cannot do this" |
| Resource not found (privacy-preserving) | 404 Not Found | Used intentionally to hide existence |

ASP.NET Core returns 401 when the authentication middleware cannot establish an identity; the `ChallengeAsync` method of the authentication scheme is responsible for generating this response, which for cookie schemes issues a redirect to the login page and for JWT Bearer schemes returns a plain 401. When an authenticated user fails an `[Authorize]` check, `ForbidAsync` is called, which for most schemes returns 403; some schemes redirect to an "access denied" page instead. The distinction matters for API clients: a 401 response signals that the client should re-authenticate (for example, refresh the token), while a 403 signals that re-authentication will not help — the user genuinely lacks the required permission. Returning 404 instead of 403 for resource-level authorization is a deliberate hardening technique that prevents callers from inferring that a resource exists but is restricted.

---

## Q28. How would you unit-test a custom IAuthorizationHandler?

**Concepts**
- `AuthorizationHandlerContext` as a concrete, directly instantiable test object
- `context.HasSucceeded` vs `context.HasFailed` — distinct assertion targets
- Mocking injected services for handlers with dependencies
- Resource parameter testing for resource-based handlers

**Answer**

Unit-testing a custom `IAuthorizationHandler` involves constructing an `AuthorizationHandlerContext` with a carefully crafted `ClaimsPrincipal`, the specific requirements under test, and an optional resource object, then calling `HandleAsync` on the handler and asserting whether the context shows success or failure. No ASP.NET Core hosting infrastructure is needed.

```csharp
[Fact]
public async Task Handler_Succeeds_When_User_Meets_MinimumAge()
{
    var requirement = new MinimumAgeRequirement(18);
    var claims = new[] { new Claim("DateOfBirth", "2000-01-01") };
    var identity = new ClaimsIdentity(claims, "Test");
    var user = new ClaimsPrincipal(identity);

    var context = new AuthorizationHandlerContext(
        new[] { requirement }, user, resource: null);

    var handler = new MinimumAgeHandler();
    await handler.HandleAsync(context);

    Assert.True(context.HasSucceeded);
}
```

`AuthorizationHandlerContext` is a concrete class with no dependencies, so it can be instantiated directly in tests without mocking; this makes handler unit tests lightweight and fast. Testing failure scenarios requires asserting `context.HasSucceeded == false` (requirement not succeeded) or `context.HasFailed == true` (explicit fail called); these are distinct states with different policy implications. When the handler under test depends on injected services, those services should be mocked using Moq or NSubstitute and passed through the handler's constructor. For resource-based handlers, pass the resource object as the third argument to `AuthorizationHandlerContext` and verify the handler correctly reads from `context.Resource`; casting to `null` or the wrong type should be tested as a failure case to confirm the handler handles it defensively.

---

## Gotchas — Claims & Authorization (Interview Traps)

---

#### Gotcha 1. Roles Are Just Claims Under the Hood

**Concepts**
- `ClaimTypes.Role` — the underlying claim type for role checks
- `RoleClaimType` misconfiguration — `IsInRole` always returns false despite role claim present
- JWT `"roles"` claim mapping requires `TokenValidationParameters.RoleClaimType`

**Answer**

Many developers treat roles as a fundamentally different concept from claims, but in ASP.NET Core roles are stored and transported as claims with type `ClaimTypes.Role`. The mental model of "roles are a separate system" breaks down when debugging why `User.IsInRole("Admin")` returns `false` even though the user appears to have the role; the real check is whether a claim with `ClaimTypes.Role` and value `"Admin"` is present on the principal. JWT tokens frequently emit roles under a `"roles"` claim key rather than the full `ClaimTypes.Role` URI; if `TokenValidationParameters.RoleClaimType` is not configured to match, `IsInRole` will always return `false` despite the claim being present in the token.

---

#### Gotcha 2. Multiple [Authorize] Attributes Combine, They Do Not Override

**Concepts**
- Attribute accumulation — each `[Authorize]` adds requirements independently
- `[AllowAnonymous]` — the correct mechanism to open a single action on a protected controller

**Answer**

Developers coming from frameworks where action-level attributes replace controller-level ones often assume that a more specific `[Authorize(Policy = "X")]` on an action overrides a broader `[Authorize(Roles = "Y")]` on the controller, but in ASP.NET Core all `[Authorize]` attributes accumulate — both must be satisfied. Adding `[Authorize(Policy = "PublicContent")]` to an action does not remove the controller's `[Authorize(Roles = "Staff")]` requirement; the user must satisfy both, which is almost certainly not what was intended. The correct way to open up a specific action on an otherwise-protected controller is `[AllowAnonymous]`, not a different `[Authorize]` attribute.

---

#### Gotcha 3. context.Fail() Is a Hard Deny That Cannot Be Overridden by Succeed

**Concepts**
- `context.Fail()` — explicit veto checked independently of `HasSucceeded`
- Permissive handler calling `Fail()` — accidentally blocks access entirely

**Answer**

Calling `context.Fail()` inside an `IAuthorizationHandler` signals an explicit deny that the policy engine treats as final; even if another handler calls `context.Succeed(requirement)` afterward, the policy still fails because `HasFailed` is checked independently of `HasSucceeded`. This behavior is intentional for security: a handler that detects a specific condition such as a suspended account can veto access with no risk of being overridden by another handler that does not know about the suspension. The practical gotcha is that calling `context.Fail()` in a permissive handler — one intended to only add successes, not denials — accidentally blocks access even when other handlers fully satisfy all requirements.

---

#### Gotcha 4. IClaimsTransformation Runs on Every Authenticated Request

**Concepts**
- `TransformAsync` per-request execution — not just at login
- Unconditional database call multiplies load by all authenticated endpoints
- Sentinel claim or caching as the required mitigation

**Answer**

Developers sometimes implement `IClaimsTransformation` with an unconditional database call and are surprised by the performance impact; the transformer runs on every request where authentication has succeeded and authorization middleware is active, not just on login. Without a guard clause checking whether the transformation has already been applied — for example, a sentinel claim — the database query runs on every single authenticated request, multiplying database load by the number of authorized endpoints called per user per session. The transformer is not cached by the framework; all caching and deduplication logic must be implemented by the developer inside `TransformAsync`.

---

#### Gotcha 5. A Missing Handler Silently Fails Authorization — No Exception Is Thrown

**Concepts**
- Missing handler — 403 Forbidden with no startup error, no log warning
- `AuthorizationResult.Failure.FailedRequirements` — diagnostic path
- `builder.Services.AddScoped<IAuthorizationHandler, ...>()` as the fix

**Answer**

When an `IAuthorizationRequirement` is added to a policy but no `IAuthorizationHandler` is registered for it, the requirement is never satisfied and the policy fails with 403 Forbidden — but no exception, warning, or log entry indicates the missing registration. This makes the bug invisible at startup; the application runs without error but every request hitting that policy is silently denied, which can look identical to a correctly configured access-denied scenario. The diagnostic approach is to inspect `AuthorizationResult.Failure.FailedRequirements` in a logging middleware or to temporarily add verbose authorization event logging to surface which requirements were left unsatisfied.

---
