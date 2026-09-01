# ASP.NET Core Identity — Setup & User Management — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [What is ASP.NET Core Identity and what does it provide out of the box?](#q1)
2. [What is the difference between `AddIdentity`, `AddDefaultIdentity`, and `AddIden…](#q2)
3. [What is `IdentityDbContext` and what tables does it create in the database?](#q3)
4. [What is the purpose of `UserManager<TUser>` and what are its most commonly used …](#q4)
5. [How do you create a new user with `UserManager<TUser>`?](#q5)
6. [What does `IdentityResult` represent and how do you handle failure cases?](#q6)
7. [What is `RoleManager<TRole>` and what operations does it support?](#q7)
8. [What is the role of `SignInManager<TUser>` and how does it differ from `UserMana…](#q8)
9. [What does `PasswordSignInAsync` return and what are its possible outcomes?](#q9)
10. [How do you extend `IdentityUser` with custom properties?](#q10)
11. [How does Identity store user data — what are `UserStore` and `RoleStore`?](#q11)
12. [What is `IdentityOptions` and what categories of settings can you configure thro…](#q12)
13. [How do you configure password complexity rules in ASP.NET Core Identity?](#q13)
14. [How does account lockout work in ASP.NET Core Identity and how do you configure …](#q14)
15. [What user validation rules does Identity enforce by default and how can you chan…](#q15)
16. [How do you seed users and roles at application startup?](#q16)
17. [How do you add a user to a role using `UserManager<TUser>`?](#q17)
18. [How do you use Identity without Entity Framework Core?](#q18)
19. [What happens to cookie authentication when you call `AddIdentity`?](#q19)
20. [How does `SignInAsync` differ from `PasswordSignInAsync`?](#q20)
21. [What is the `NormalizedEmail` and `NormalizedUserName` fields used for in Identi…](#q21)
22. [How do you find a user by email using `UserManager<TUser>`?](#q22)
23. [What is the `SecurityStamp` column in the `AspNetUsers` table and why does it ma…](#q23)
24. [How does Identity handle concurrent logins and token invalidation via `SecurityS…](#q24)
25. [What is the `AspNetUserClaims` table used for and how do you add claims to a use…](#q25)
26. [How do you change a user's password using `UserManager<TUser>`?](#q26)
27. [What is `IUserValidator<TUser>` and when would you implement a custom one?](#q27)
28. [How do you configure Identity so email confirmation is required before a user ca…](#q28)

---

## Q1. What is ASP.NET Core Identity and what does it provide out of the box?

What is ASP.NET Core Identity and what does it provide out of the box?

**Answer:** ASP.NET Core Identity is a membership system that adds login functionality to ASP.NET Core applications. It manages users, passwords, roles, and claims, and integrates with Entity Framework Core (EF Core) to persist that data automatically. It is designed to be extensible so that custom user properties, custom storage backends, and custom validation rules can all replace or augment the defaults.

- Identity ships with a default `IdentityUser` class that holds fields such as `UserName`, `Email`, `PasswordHash`, `PhoneNumber`, and `LockoutEnd`, saving developers from writing these by hand.
- It provides three high-level service classes — `UserManager<TUser>`, `RoleManager<TRole>`, and `SignInManager<TUser>` — that encapsulate all common membership operations at an abstraction layer above the database.
- Cookie-based authentication is pre-configured when Identity is registered with `AddIdentity`, so applications get session persistence without additional middleware setup.
- Token providers for email confirmation, password reset, two-factor authentication (2FA), and change-email flows are included and can be swapped for custom implementations.
- Identity enforces configurable password complexity, account lockout, and username/email uniqueness rules through its built-in validator pipeline.

---

## Q2. What is the difference between `AddIdentity`, `AddDefaultIdentity`, and `AddIdentityCore`?

What is the difference between `AddIdentity`, `AddDefaultIdentity`, and `AddIdentityCore`?

**Answer:** The three registration methods are layered from most complete to most minimal. `AddIdentity` registers the full Identity stack including `UserManager`, `RoleManager`, `SignInManager`, and cookie authentication. `AddDefaultIdentity` adds the same user-facing services but omits `RoleManager` and is the method used by Razor Pages scaffolding. `AddIdentityCore` registers only the core services with no authentication middleware, giving full control over what gets layered on top.

| Method | `UserManager` | `RoleManager` | `SignInManager` | Cookie Auth |
|---|---|---|---|---|
| `AddIdentity` | Yes | Yes | Yes | Yes (auto) |
| `AddDefaultIdentity` | Yes | No | Yes | Yes (auto) |
| `AddIdentityCore` | Yes | No | No | No |

- `AddIdentityCore` is the right choice for APIs that issue JSON Web Tokens (JWTs) instead of cookies, because it registers only the password hashing, user validation, and data access layer without forcing cookie middleware.
- `AddDefaultIdentity` is a convenience overload that calls `AddIdentityCore` internally, then manually chains `AddDefaultTokenProviders` and `AddSignInManager`, along with cookie configuration — but skips roles because many applications do not need them.
- When roles are needed with `AddDefaultIdentity`, you chain `.AddRoles<IdentityRole>()` after the call to restore `RoleManager` without switching to `AddIdentity`.
- Choosing the wrong method is a common setup mistake: using `AddIdentityCore` for a cookie-based application will leave `SignInManager` unregistered and throw a runtime exception when the service is injected.

---

## Q3. What is `IdentityDbContext` and what tables does it create in the database?

What is `IdentityDbContext` and what tables does it create in the database?

**Answer:** `IdentityDbContext<TUser>` is an Entity Framework Core `DbContext` subclass that pre-configures the entity model for all Identity-related tables. When you run `dotnet ef migrations add InitialCreate`, EF Core uses this context to generate the schema. All table names are prefixed with `AspNet` by convention, though they can be renamed by overriding `OnModelCreating`.

| Table | Purpose |
|---|---|
| `AspNetUsers` | One row per user; holds profile data, password hash, security stamp |
| `AspNetRoles` | One row per application role |
| `AspNetUserRoles` | Join table between users and roles (many-to-many) |
| `AspNetUserClaims` | Arbitrary claim key/value pairs owned by a user |
| `AspNetRoleClaims` | Claims attached to a role; inherited by all users in that role |
| `AspNetUserLogins` | External login provider associations (e.g., Google, Facebook) |
| `AspNetUserTokens` | Stored tokens such as refresh tokens or 2FA recovery codes |

- The generic `IdentityDbContext<TUser, TRole, TKey>` overload lets you use a custom user type, a custom role type, and a custom primary key type (e.g., `Guid` instead of `string`).
- You must call `base.OnModelCreating(builder)` when overriding `OnModelCreating` in a derived context, otherwise Identity's own entity configuration is skipped and migrations will be incorrect.
- Renaming tables (e.g., changing `AspNetUsers` to `Users`) is done inside `OnModelCreating` using `builder.Entity<IdentityUser>().ToTable("Users")`.

---

## Q4. What is the purpose of `UserManager<TUser>` and what are its most commonly used methods?

What is the purpose of `UserManager<TUser>` and what are its most commonly used methods?

**Answer:** `UserManager<TUser>` is the central service for all user-related operations in ASP.NET Core Identity. It abstracts the underlying data store so business logic does not couple directly to EF Core, and it runs validators and password hashers as part of each operation. It is registered as a scoped service and is the primary entry point for creating, finding, updating, and deleting users.

- `CreateAsync(user, password)` validates the user object, hashes the password, and persists the new user to the store in one call.
- `FindByEmailAsync(email)` looks up a user by normalised email, using the `NormalizedEmail` column rather than a case-sensitive comparison.
- `AddToRoleAsync(user, roleName)` inserts a row into `AspNetUserRoles`, making the user a member of that role.
- `CheckPasswordAsync(user, password)` verifies a plain-text password against the stored hash without triggering lockout logic — useful for re-authentication scenarios.
- `GeneratePasswordResetTokenAsync(user)` produces a time-limited, purpose-bound token that can be sent by email; `ResetPasswordAsync(user, token, newPassword)` then validates and applies the change.
- `UpdateAsync(user)` persists changes to a user entity and regenerates the `SecurityStamp` when sensitive fields change, ensuring active sessions are invalidated.

---

## Q5. How do you create a new user with `UserManager<TUser>`?

How do you create a new user with `UserManager<TUser>`?

**Answer:** You instantiate (or populate) a `TUser` object, then call `UserManager<TUser>.CreateAsync(user, password)`, which validates, hashes, and persists in one step. The return value is an `IdentityResult` that indicates success or carries a list of `IdentityError` objects describing what went wrong.

```csharp
var user = new ApplicationUser { UserName = "alice", Email = "alice@example.com" };
var result = await _userManager.CreateAsync(user, "P@ssw0rd!");
if (!result.Succeeded)
{
    foreach (var error in result.Errors)
        ModelState.AddModelError(string.Empty, error.Description);
}
```

- The `password` parameter is plain text; `UserManager` passes it through the configured `IPasswordHasher<TUser>` before storage, so the raw password is never persisted.
- All registered `IUserValidator<TUser>` implementations run before the record is written; if any validator fails, `CreateAsync` returns a failed `IdentityResult` and nothing is written to the database.
- When creating a user that should not have a password (e.g., a user who will only log in via an external provider), call the single-argument overload `CreateAsync(user)` to skip password hashing entirely.

---

## Q6. What does `IdentityResult` represent and how do you handle failure cases?

What does `IdentityResult` represent and how do you handle failure cases?

**Answer:** `IdentityResult` is a simple value object returned by most `UserManager` and `RoleManager` write operations. Its `Succeeded` property is `true` when the operation completed without errors, and its `Errors` property is a collection of `IdentityError` objects each containing a machine-readable `Code` and a human-readable `Description`. A static `IdentityResult.Success` singleton exists for the success case; `IdentityResult.Failed(errors)` constructs failure instances.

- Checking only `result.Succeeded` and ignoring `result.Errors` on failure is a common bug; the errors contain the reason the operation failed (e.g., `DuplicateEmail`, `PasswordTooShort`) and should be surfaced to the user or logged.
- `IdentityError.Code` values are stable, documented strings that can be used in unit tests to assert specific failure reasons rather than relying on localised description text.
- Custom `IUserValidator<TUser>` and `IPasswordValidator<TUser>` implementations return `IdentityResult` objects, so the error pipeline is consistent regardless of whether the failure came from a built-in or custom rule.
- In ASP.NET Core Minimal API or controller contexts, a common pattern is to map each error's `Description` into a `ProblemDetails` response so the client receives structured error information.

---

## Q7. What is `RoleManager<TRole>` and what operations does it support?

What is `RoleManager<TRole>` and what operations does it support?

**Answer:** `RoleManager<TRole>` is the counterpart to `UserManager` for role entities. It provides create, read, update, and delete (CRUD) operations over the `AspNetRoles` table and normalises role names in the same way `UserManager` normalises usernames and emails. It is only registered in the dependency injection (DI) container when `AddIdentity` or `AddRoles<TRole>()` is called; `AddDefaultIdentity` alone does not register it.

- `CreateAsync(role)` persists a new role and returns an `IdentityResult`, following the same validator pipeline used for users.
- `FindByNameAsync(roleName)` retrieves a role by its normalised name, making role lookups case-insensitive by default.
- `DeleteAsync(role)` removes the role record; existing `AspNetUserRoles` rows referencing that role are cascade-deleted if the foreign key is configured that way in EF Core.
- `AddClaimAsync(role, claim)` attaches a `Claim` to a role, which is then inherited by every user who is a member of that role when their `ClaimsPrincipal` is constructed by `SignInManager`.
- When seeding roles at startup, always call `RoleExistsAsync(name)` before `CreateAsync` to avoid duplicate-key exceptions on repeated application starts.

---

## Q8. What is the role of `SignInManager<TUser>` and how does it differ from `UserManager<TUser>`?

What is the role of `SignInManager<TUser>` and how does it differ from `UserManager<TUser>`?

**Answer:** `SignInManager<TUser>` sits above `UserManager` in the Identity stack and handles the authentication session layer — issuing and clearing cookies, evaluating lockout state, and orchestrating external logins. `UserManager` is concerned with persistence and validation of user data, while `SignInManager` is concerned with whether the current HTTP request should be treated as authenticated. They work together: `SignInManager` calls `UserManager` internally to look up user records and check passwords.

- `SignInManager` requires an `HttpContext` to write cookies, which means it cannot be used in background services or contexts that have no HTTP request — `UserManager` does not have this limitation.
- `SignInManager.PasswordSignInAsync` combines a password check with lockout tracking and cookie issuance in one call, making it the correct method for login form handlers.
- `UserManager.CheckPasswordAsync` verifies a password without side effects (no cookie, no lockout increment), making it suitable for re-authentication or API token scenarios.
- `SignInManager` exposes external authentication helpers such as `GetExternalLoginInfoAsync` and `ExternalLoginSignInAsync` that manage the handshake with OAuth providers; these have no equivalent on `UserManager`.

---

## Q9. What does `PasswordSignInAsync` return and what are its possible outcomes?

What does `PasswordSignInAsync` return and what are its possible outcomes?

**Answer:** `SignInManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure)` returns a `SignInResult` whose boolean properties communicate why the sign-in did or did not succeed. The four distinct outcomes map to four scenarios: success, lockout, two-factor required, and failure (wrong credentials or user not found).

| `SignInResult` property | Meaning |
|---|---|
| `Succeeded` | Credentials valid, not locked out, cookie issued |
| `IsLockedOut` | Valid username but account locked; no cookie issued |
| `RequiresTwoFactor` | First factor passed; 2FA challenge needed before cookie |
| `IsNotAllowed` | User exists but is not permitted to sign in (e.g., unconfirmed email) |
| None of the above (all false) | Wrong password or user not found |

- When `lockoutOnFailure` is `true`, each failed attempt increments the `AccessFailedCount` column and, once the threshold is reached, sets `LockoutEnd` to a future date-time.
- `IsNotAllowed` is only returned when `IdentityOptions.SignIn.RequireConfirmedEmail` (or phone/account) is `true` and the user has not yet confirmed; it does not occur with default settings.
- Never expose to the end user whether the username or the password was wrong — returning a generic "invalid credentials" message prevents username enumeration attacks.

---

## Q10. How do you extend `IdentityUser` with custom properties?

How do you extend `IdentityUser` with custom properties?

**Answer:** You create a class that inherits from `IdentityUser` (or `IdentityUser<TKey>` for a custom primary key type), add the desired properties, and then substitute that class everywhere `TUser` appears — in `IdentityDbContext`, service registration, and `UserManager` injection. EF Core will include the additional columns in its migration because the derived class is the mapped entity.

```csharp
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}
```

- The `AddIdentity<ApplicationUser, IdentityRole>()` call must reference the custom type so the DI container registers `UserManager<ApplicationUser>` rather than `UserManager<IdentityUser>`.
- The `DbContext` must inherit from `IdentityDbContext<ApplicationUser>` instead of `IdentityDbContext<IdentityUser>` so EF Core maps the extra columns to the `AspNetUsers` table.
- Injecting `UserManager<IdentityUser>` into a controller after registering `UserManager<ApplicationUser>` causes a `InvalidOperationException` at runtime; the generic type argument must match exactly.
- After adding properties, a new EF Core migration is required to add the corresponding columns; running the application without a migration leaves the database schema out of sync and causes runtime `SqlException` errors.

---

## Q11. How does Identity store user data — what are `UserStore` and `RoleStore`?

How does Identity store user data — what are `UserStore` and `RoleStore`?

**Answer:** `UserStore<TUser>` and `RoleStore<TRole>` are the data-access layer of ASP.NET Core Identity. They implement the `IUserStore<TUser>` and `IRoleStore<TRole>` interfaces, translating high-level calls from `UserManager` and `RoleManager` into actual database operations. When you call `AddEntityFrameworkStores<TContext>()`, EF Core-backed implementations of these interfaces are registered in the DI container.

- The store interface is broken into many smaller, optional interfaces — `IUserPasswordStore`, `IUserEmailStore`, `IUserLockoutStore`, `IUserRoleStore`, etc. — so a custom store need only implement the interfaces that match the features it supports.
- `UserManager` checks at runtime whether the injected store implements a given sub-interface before calling it; if the store does not implement `IUserEmailStore`, email-related `UserManager` methods throw `NotSupportedException`.
- The store receives the `TUser` entity but is responsible only for persistence; it does not validate passwords or enforce rules — that is done by the validator pipeline in `UserManager` before the store is called.
- `AddEntityFrameworkStores<TContext>()` must be chained immediately after `AddIdentity` or `AddIdentityCore`; omitting it leaves the store unregistered and causes a `InvalidOperationException` when `UserManager` is first resolved.

---

## Q12. What is `IdentityOptions` and what categories of settings can you configure through it?

What is `IdentityOptions` and what categories of settings can you configure through it?

**Answer:** `IdentityOptions` is the top-level configuration class for ASP.NET Core Identity, accessed through the standard `IOptions<IdentityOptions>` pattern. It groups settings into four nested objects: `PasswordOptions`, `LockoutOptions`, `UserOptions`, and `SignInOptions`. Values are set in `Program.cs` inside the lambda passed to `AddIdentity` (or `AddDefaultIdentity`).

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 10;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
});
```

- `PasswordOptions` controls minimum length, required character classes (uppercase, lowercase, digit, non-alphanumeric), and the number of unique characters required.
- `LockoutOptions` sets the maximum failed attempts before lockout, the lockout duration (`DefaultLockoutTimeSpan`), and whether new users start with lockout enabled.
- `UserOptions` defines the allowed characters in usernames (`AllowedUserNameCharacters`) and whether email addresses must be unique across all users.
- `SignInOptions` lets you require confirmed email, confirmed phone number, or confirmed account before `PasswordSignInAsync` will return `Succeeded`.

---

## Q13. How do you configure password complexity rules in ASP.NET Core Identity?

How do you configure password complexity rules in ASP.NET Core Identity?

**Answer:** Password complexity is controlled through `IdentityOptions.Password`, which is a `PasswordOptions` instance. You set it in the `AddIdentity` (or `AddDefaultIdentity`) configuration lambda. The default rules require at least one digit, one lowercase letter, one uppercase letter, one non-alphanumeric character, and a minimum length of six characters with at least one unique character.

| Option | Default | Description |
|---|---|---|
| `RequiredLength` | 6 | Minimum total character count |
| `RequiredUniqueChars` | 1 | Minimum number of distinct characters |
| `RequireDigit` | `true` | Must contain 0–9 |
| `RequireLowercase` | `true` | Must contain a–z |
| `RequireUppercase` | `true` | Must contain A–Z |
| `RequireNonAlphanumeric` | `true` | Must contain a symbol |

- Setting `RequireNonAlphanumeric = false` is a common production change when users complain about special-character requirements, but this weakens the password's entropy.
- Custom password rules that cannot be expressed through `PasswordOptions` (e.g., disallowing common passwords) are implemented as `IPasswordValidator<TUser>` and registered with `.AddPasswordValidator<MyPasswordValidator>()`.
- Password validation runs inside `UserManager.CreateAsync` and `UserManager.ChangePasswordAsync`; calling `UserManager.AddPasswordAsync` on a user who already has a password returns a failure result rather than overwriting the existing hash.

---

## Q14. How does account lockout work in ASP.NET Core Identity and how do you configure it?

How does account lockout work in ASP.NET Core Identity and how do you configure it?

**Answer:** Account lockout in ASP.NET Core Identity is implemented by tracking a failed-access counter and a lockout-end timestamp directly on the user record in `AspNetUsers`. When `PasswordSignInAsync` is called with `lockoutOnFailure: true` and the password is wrong, Identity increments `AccessFailedCount`. Once the count reaches `LockoutOptions.MaxFailedAccessAttempts`, it sets `LockoutEnd` to `DateTimeOffset.UtcNow + DefaultLockoutTimeSpan` and subsequent sign-in attempts return `SignInResult.IsLockedOut` without checking the password.

- A successful sign-in resets `AccessFailedCount` to zero and clears `LockoutEnd`, so the lockout counter is not cumulative across sessions.
- `LockoutOptions.AllowedForNewUsers` defaults to `true`, meaning new accounts are eligible for lockout immediately; setting it to `false` disables lockout for all new users until explicitly enabled on the user record.
- Administrators can manually unlock an account by calling `UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow)` or `UserManager.ResetAccessFailedCountAsync(user)`.
- Passing `lockoutOnFailure: false` to `PasswordSignInAsync` entirely skips the lockout mechanism for that call, which is appropriate when the caller (e.g., an admin impersonation flow) should not be subject to user-facing lockout rules.

---

## Q15. What user validation rules does Identity enforce by default and how can you change them?

What user validation rules does Identity enforce by default and how can you change them?

**Answer:** ASP.NET Core Identity validates user objects through the `IUserValidator<TUser>` pipeline before any `CreateAsync` or `UpdateAsync` call is committed to the store. By default, `UserValidator<TUser>` enforces that the `UserName` contains only characters listed in `UserOptions.AllowedUserNameCharacters` (letters, digits, and `-._@+`) and, when `RequireUniqueEmail` is `true` (default is `false`), that the email address is not already in use.

- `AllowedUserNameCharacters` defaults to `"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"` — any username character outside this set causes a `InvalidUserName` error.
- Changing the allowed character set requires setting `options.User.AllowedUserNameCharacters` in the Identity configuration lambda; setting it to an empty string or `null` disables character validation entirely.
- Custom validators are added with `.AddUserValidator<MyUserValidator>()` and run alongside the default validator; they do not replace it unless the default is explicitly removed from the `UserManager.UserValidators` collection.
- Email format validation is not performed by the default `UserValidator`; Identity only checks uniqueness. If you need RFC 5322 format enforcement, implement a custom `IUserValidator<TUser>`.

---

## Q16. How do you seed users and roles at application startup?

How do you seed users and roles at application startup?

**Answer:** The recommended pattern is to resolve `UserManager<TUser>` and `RoleManager<TRole>` from the application's root service provider after building the `WebApplication` and before calling `app.Run()`. This runs synchronously at startup, ensuring data exists before any request is handled. Because these are scoped services, they must be resolved inside an explicit service scope.

```csharp
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (await userManager.FindByEmailAsync("admin@example.com") == null)
    {
        var admin = new ApplicationUser { UserName = "admin", Email = "admin@example.com" };
        await userManager.CreateAsync(admin, "Admin@1234!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}
```

- Always guard each seed operation with an existence check (`RoleExistsAsync`, `FindByEmailAsync`) to make the seed idempotent across restarts and deployments.
- Seed data in production should be driven from environment variables or a secrets vault rather than hard-coded strings embedded in source code.
- An alternative approach is to place seed logic inside a custom `IHostedService` that runs as part of the `IHost` startup pipeline, which integrates better with testing frameworks.

---

## Q17. How do you add a user to a role using `UserManager<TUser>`?

How do you add a user to a role using `UserManager<TUser>`?

**Answer:** `UserManager<TUser>.AddToRoleAsync(user, roleName)` inserts a row into the `AspNetUserRoles` join table, associating the user with the named role. The role must already exist in `AspNetRoles`; if it does not, the method returns a failed `IdentityResult` with an `InvalidRoleName` error rather than creating the role automatically.

- `AddToRolesAsync(user, roles)` accepts an `IEnumerable<string>` and adds the user to multiple roles in a single call, reducing round-trips to the database.
- `IsInRoleAsync(user, roleName)` checks role membership without loading the full role list; it performs a case-insensitive comparison against the normalised role name.
- `RemoveFromRoleAsync(user, roleName)` removes the `AspNetUserRoles` row; it does not delete the role itself.
- After roles are assigned, the next time the user signs in through `SignInManager`, the role names are included as `ClaimTypes.Role` claims in the `ClaimsPrincipal`, which is what `[Authorize(Roles = "Admin")]` evaluates.

---

## Q18. How do you use Identity without Entity Framework Core?

How do you use Identity without Entity Framework Core?

**Answer:** You replace the EF Core store registration with a custom implementation of `IUserStore<TUser>` (and optionally `IRoleStore<TRole>`) that reads and writes to any backing system — a document database, a REST API, an in-memory store, or a relational database via raw ADO.NET. You then register the custom store with the DI container by calling `AddIdentityCore<TUser>()` (or `AddIdentity`) followed by `.AddUserStore<MyCustomUserStore>()` instead of `.AddEntityFrameworkStores<TContext>()`.

- The store interface is broken into capability sub-interfaces (`IUserPasswordStore`, `IUserEmailStore`, `IUserLockoutStore`, etc.) so you only implement what your backend supports; `UserManager` checks at runtime which interfaces the store implements.
- The store must be registered as at least a scoped service; if it maintains a connection or unit-of-work object, scoped lifetime ensures that object is created once per HTTP request.
- `IdentityDbContext` and the `AspNet*` tables are entirely absent in a custom-store setup — the schema is whatever the backing system defines.
- Custom stores are the correct path when integrating Identity with a legacy database that has an existing user table with a different schema or column names.

---

## Q19. What happens to cookie authentication when you call `AddIdentity`?

What happens to cookie authentication when you call `AddIdentity`?

**Answer:** `AddIdentity` automatically calls `AddAuthentication()` and configures two cookie schemes: the application cookie (named `Identity.Application`) and an external cookie (named `Identity.External`) for OAuth callback state. This means that if you call `AddAuthentication()` separately before `AddIdentity`, you risk conflicting registrations; conversely, calling `AddAuthentication()` with a different default scheme after `AddIdentity` will override the scheme Identity set.

- The application cookie's default settings include `SlidingExpiration = true`, a 14-day expiration when `isPersistent` is `true`, and a `LoginPath` of `/Account/Login` — all of which can be overridden through `CookieAuthenticationOptions`.
- `AddIdentityCore` does not register any authentication middleware, so you must call `AddAuthentication()` and configure a scheme manually — this is why it is preferred for JWT-based APIs.
- A common misconfiguration is calling `AddAuthentication().AddJwtBearer()` after `AddIdentity` in a project that needs both cookie and JWT authentication; the default scheme ends up pointing at whichever was registered last, breaking the other. The fix is to explicitly set `options.DefaultScheme` and `options.DefaultChallengeScheme` to the correct scheme names.
- The external cookie scheme is a short-lived (default 10 minutes) transient cookie used only during the OAuth callback round-trip; it is not used to persist the user's authenticated session.

---

## Q20. How does `SignInAsync` differ from `PasswordSignInAsync`?

How does `SignInAsync` differ from `PasswordSignInAsync`?

**Answer:** `SignInManager.SignInAsync(user, isPersistent)` issues the authentication cookie directly for a `TUser` object that you have already retrieved and validated yourself, without performing any credential check. `PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure)` does the credential lookup and password verification first, then calls `SignInAsync` internally only on success. Use `SignInAsync` when the authentication decision has already been made by some other mechanism (e.g., after an external OAuth callback resolves to a known user).

- `SignInAsync` does not increment `AccessFailedCount` or consult lockout state; it assumes the caller has already determined the user is allowed to sign in.
- After email confirmation, 2FA completion, or password reset flows where the user's identity is already established, `SignInAsync` is the correct choice to issue the session cookie without re-verifying a password.
- `PasswordSignInAsync` internally calls `UserManager.FindByNameAsync`, `SignInManager.CanSignInAsync` (which checks `IsNotAllowed` conditions), lockout checks, and `UserManager.CheckPasswordAsync` before calling `SignInAsync` — it is the full pipeline.
- Using `SignInAsync` to bypass password verification in a login flow is a security error; it should only be called when you have a trustworthy, pre-validated user object.

---

## Q21. What is the `NormalizedEmail` and `NormalizedUserName` fields used for in Identity?

What is the `NormalizedEmail` and `NormalizedUserName` fields used for in Identity?

**Answer:** `NormalizedEmail` and `NormalizedUserName` are uppercase (by default) versions of the corresponding user fields, stored alongside the original values and indexed for fast, case-insensitive lookups. The normalisation is performed by `ILookupNormalizer`, whose default implementation (`UpperInvariantLookupNormalizer`) converts strings to uppercase using the invariant culture. All `FindBy*` methods on `UserManager` query the normalised columns rather than the original ones.

- Storing a pre-normalised copy avoids using database functions like `UPPER()` or `LOWER()` in every query, which would prevent the database from using an index and would slow lookups on large user tables.
- The unique index on `NormalizedEmail` (when `RequireUniqueEmail = true`) and `NormalizedUserName` enforces uniqueness in a case-insensitive way at the database level, not just in application code.
- If you write raw SQL or LINQ queries that join on `Email` or `UserName` instead of their normalised counterparts, you risk missing users or getting duplicate rows in case-sensitive collations.
- You can replace `ILookupNormalizer` with a custom implementation (e.g., one that applies Unicode NFKD normalisation for international usernames) by registering it with the DI container before `AddIdentity` is called.

---

## Q22. How do you find a user by email using `UserManager<TUser>`?

How do you find a user by email using `UserManager<TUser>`?

**Answer:** `UserManager<TUser>.FindByEmailAsync(email)` is the correct method for email-based user lookups. It normalises the input string using the registered `ILookupNormalizer` and then queries the `NormalizedEmail` column, making the search case-insensitive and consistent with how the value was stored at creation time.

- Querying `dbContext.Users.FirstOrDefaultAsync(u => u.Email == email)` directly bypasses normalisation and will fail to find users whose stored `Email` has different casing from the search term.
- `FindByEmailAsync` returns `null` (not an exception) when no user matches; always null-check the result before accessing the returned user object.
- `FindByNameAsync(userName)` follows the same normalisation pattern for username lookups and is what `PasswordSignInAsync` uses internally when it receives a username string.
- `FindByIdAsync(userId)` takes the string representation of the primary key and is the most efficient lookup when the ID is already known (e.g., from a JWT sub claim).

---

## Q23. What is the `SecurityStamp` column in the `AspNetUsers` table and why does it matter?

What is the `SecurityStamp` column in the `AspNetUsers` table and why does it matter?

**Answer:** `SecurityStamp` is a `Guid`-valued string column on `AspNetUsers` that Identity regenerates whenever a user's security-sensitive data changes — password, email, phone number, external logins, or roles. The cookie authentication middleware periodically validates the current stamp in the cookie against the stamp in the database; if they differ, the cookie is rejected and the user is signed out. This mechanism ensures that changing a password or revoking a role takes effect for active sessions within a configurable grace period.

- The validation interval defaults to 30 minutes (`SecurityStampValidator.ValidationInterval`); within this window, a compromised cookie remains valid even after a password change. Lowering the interval to seconds or minutes reduces the attack window at the cost of additional database reads per request.
- `UserManager.UpdateSecurityStampAsync(user)` allows you to manually rotate the stamp, which is useful when an administrator forces a sign-out of all active sessions for a specific user.
- If `SecurityStamp` validation is disabled (by not adding the `SecurityStampValidator` middleware), changing a user's password will not invalidate their existing authenticated sessions, which is a significant security gap.
- `AddIdentity` and `AddDefaultIdentity` register `SecurityStampValidator` automatically; `AddIdentityCore` does not, so JWT-based APIs that use token introspection must implement their own stamp-checking logic.

---

## Q24. How does Identity handle concurrent logins and token invalidation via `SecurityStamp`?

How does Identity handle concurrent logins and token invalidation via `SecurityStamp`?

**Answer:** Identity does not block concurrent logins by default — multiple sessions can exist simultaneously for the same user. Token invalidation is handled through the `SecurityStamp` mechanism: when the stamp changes (e.g., after a password reset), the `SecurityStampValidator` middleware detects the mismatch on the next periodic check for each active session and forces those sessions to re-authenticate. This effectively invalidates all pre-existing cookies within the configured validation interval.

- The `SecurityStampValidatorOptions.ValidationInterval` controls how frequently the stamp is checked against the database; setting it to `TimeSpan.Zero` checks on every request, providing immediate invalidation at the cost of a database read per request.
- For JWTs, the security stamp must be embedded in the token as a claim at issuance time, and the token validation middleware must compare that claim to the current database value — this is not automatic and requires custom implementation.
- ASP.NET Core Identity has no built-in "sign out everywhere" feature; the standard pattern is to call `UserManager.UpdateSecurityStampAsync(user)`, which rotates the stamp and causes all existing cookies to be rejected on their next validation cycle.
- The `AspNetUserTokens` table can store per-session refresh tokens that are individually revocable, giving finer-grained control than the global stamp rotation approach.

---

## Q25. What is the `AspNetUserClaims` table used for and how do you add claims to a user?

What is the `AspNetUserClaims` table used for and how do you add claims to a user?

**Answer:** The `AspNetUserClaims` table stores arbitrary claim key/value pairs permanently associated with a specific user. These claims are loaded alongside the user's roles when `SignInManager.CreateUserPrincipalAsync` builds the `ClaimsPrincipal` for the session. They are distinct from role claims (which are stored in `AspNetRoleClaims`) in that they belong to one specific user rather than everyone in a role.

- `UserManager.AddClaimAsync(user, claim)` inserts a row into `AspNetUserClaims`; the `Claim` constructor takes a type string (e.g., `"department"`) and a value string (e.g., `"Engineering"`).
- `UserManager.GetClaimsAsync(user)` retrieves all persisted claims for a user; `UserManager.RemoveClaimAsync(user, claim)` deletes a specific row by matching both type and value.
- Persisted claims are appropriate for relatively stable user attributes (department, tenant ID) that must survive across sessions; transient or computed attributes are better added to the principal in a custom `IClaimsTransformation` so they are not stored in the database.
- The distinction between role-based and claim-based access control is practical: roles group users into named sets, while claims carry typed values that policies can evaluate with richer logic (e.g., `options.AddPolicy("SeniorEngineer", p => p.RequireClaim("level", "senior"))`).

---

## Q26. How do you change a user's password using `UserManager<TUser>`?

How do you change a user's password using `UserManager<TUser>`?

**Answer:** There are two paths depending on whether the current password is known. `UserManager.ChangePasswordAsync(user, currentPassword, newPassword)` verifies the current password before applying the change — use this in a user-initiated "change password" flow. `UserManager.ResetPasswordAsync(user, token, newPassword)` skips current-password verification but requires a valid reset token generated by `GeneratePasswordResetTokenAsync` — use this in a password-reset-via-email flow.

- Both methods run the full password validator pipeline against `newPassword`, so the new password must satisfy all configured `PasswordOptions` rules.
- Both methods automatically call `UpdateSecurityStampAsync` on success, invalidating all existing sessions for that user within the next validation interval.
- `UserManager.RemovePasswordAsync(user)` deletes the password hash without replacing it, leaving the account in a state where only external login providers can authenticate the user; `AddPasswordAsync(user, newPassword)` then sets a new password for an account that currently has none.
- Never update `PasswordHash` directly on the entity and call `UpdateAsync`; this bypasses the password validator and the security stamp rotation, leaving the account in an inconsistent state.

---

## Q27. What is `IUserValidator<TUser>` and when would you implement a custom one?

What is `IUserValidator<TUser>` and when would you implement a custom one?

**Answer:** `IUserValidator<TUser>` is an interface with a single method, `ValidateAsync(UserManager<TUser> manager, TUser user)`, that returns an `IdentityResult`. Implementations are called by `UserManager` as part of the validation pipeline before any `CreateAsync` or `UpdateAsync` is committed. The default implementation is `UserValidator<TUser>`, which checks username character rules and email uniqueness. Custom validators are added alongside the default by calling `.AddUserValidator<MyValidator>()`.

- A common use case for a custom validator is enforcing corporate email domains — for example, returning a failure result when the user's email does not end in `@company.com`.
- Another use case is blocking known disposable email providers by checking a deny-list; this logic does not fit neatly into `IdentityOptions` settings and is cleanly encapsulated in a custom validator.
- Multiple validators are all executed in sequence; if any returns a failed `IdentityResult`, the errors are aggregated and the operation is aborted before the store is called.
- To completely replace default validation rather than augment it, remove `UserValidator<TUser>` from `userManager.UserValidators` in a post-configure step or by registering the DI configuration in the correct order.

---

## Q28. How do you configure Identity so email confirmation is required before a user can sign in?

How do you configure Identity so email confirmation is required before a user can sign in?

**Answer:** Set `IdentityOptions.SignIn.RequireConfirmedEmail = true` in the Identity configuration lambda. When this is enabled, `SignInManager.PasswordSignInAsync` checks the `EmailConfirmed` column on the user record before issuing a cookie; if the column is `false`, the method returns a `SignInResult` with `IsNotAllowed = true` and no cookie is issued.

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
});
```

- After `CreateAsync` succeeds, call `UserManager.GenerateEmailConfirmationTokenAsync(user)` to get a time-limited, cryptographically signed token, then send a link containing that token to the user's email address.
- The confirmation endpoint calls `UserManager.ConfirmEmailAsync(user, token)`, which validates the token and sets `EmailConfirmed = true` in the database.
- The token is generated by the `DataProtectorTokenProvider` (by default), which uses ASP.NET Core Data Protection to sign and time-limit the token; the default expiry is one day.
- Enabling email confirmation without actually sending the email (e.g., during local development) locks out every newly registered user; a common workaround is to auto-confirm in development by calling `ConfirmEmailAsync` immediately after `CreateAsync`.

---

## Gotchas — ASP.NET Core Identity (Interview Traps)

---

#### Gotcha 1. `AddIdentity` silently replaces your `AddAuthentication` configuration

**Answer:** When `AddIdentity` is called, it internally calls `AddAuthentication()` and sets the default scheme to `Identity.Application`. Any `AddAuthentication()` call you placed before it is overwritten, and any scheme you configured after it may not be the default the middleware uses.

- This matters most in APIs that need both cookie and JWT authentication: the last registration wins for `DefaultAuthenticateScheme`, so the wrong scheme ends up handling requests.
- The fix is to explicitly set `DefaultAuthenticateScheme`, `DefaultChallengeScheme`, and other scheme properties in the `AuthenticationOptions` lambda after all identity services are registered, rather than relying on implicit defaults.

---

#### Gotcha 2. Injecting `UserManager<IdentityUser>` after registering a custom `ApplicationUser`

**Answer:** When Identity is registered with `AddIdentity<ApplicationUser, IdentityRole>()`, the DI container registers `UserManager<ApplicationUser>` — not `UserManager<IdentityUser>`. Injecting the base type causes a runtime `InvalidOperationException` because the base-typed service was never registered.

- This is particularly easy to get wrong when copy-pasting controller code from tutorials that use the default `IdentityUser` type.
- The fix is to consistently use `UserManager<ApplicationUser>` (or whatever the concrete type is) throughout the application; the base `IdentityUser` type should not appear in injection sites after a custom user class is introduced.

---

#### Gotcha 3. Forgetting `base.OnModelCreating(builder)` in a derived `IdentityDbContext`

**Answer:** Overriding `OnModelCreating` without calling `base.OnModelCreating(builder)` silently removes all of Identity's entity configuration — indexes, foreign keys, column types, and the `AspNet*` table mappings. Migrations generated from this misconfigured context will produce a schema that looks like a plain user table with no Identity infrastructure.

- The symptom is often a migration that drops all `AspNet*` tables and recreates them as plain tables without the correct indexes, or a runtime `SqlException` when Identity queries a column that was never created.
- The fix is a single line: always call `base.OnModelCreating(builder)` as the first statement inside the override.

---

#### Gotcha 4. Using `lockoutOnFailure: false` in `PasswordSignInAsync` for all login attempts

**Answer:** Passing `lockoutOnFailure: false` to `PasswordSignInAsync` disables lockout tracking for that call, meaning failed password attempts are never counted and accounts can never be locked out by brute force. This removes a critical defence against credential-stuffing attacks.

- The motivation for setting it to `false` is usually to avoid locking out legitimate users, but the correct mitigation is to tune `MaxFailedAccessAttempts` and `DefaultLockoutTimeSpan` rather than disable the mechanism.
- A better pattern for administrator-initiated authentication checks (where lockout should not apply) is `UserManager.CheckPasswordAsync`, which explicitly skips lockout, rather than silencing lockout in the user-facing sign-in path.

---

#### Gotcha 5. Seeding roles and users with blocking `.Result` or `.GetAwaiter().GetResult()` calls

**Answer:** Calling `.Result` or `.GetAwaiter().GetResult()` on async `UserManager` or `RoleManager` methods in a synchronous context can cause deadlocks when the underlying `SynchronizationContext` is present (e.g., in some hosting scenarios or tests that run on a single-threaded context).

- The deadlock occurs because the async continuation waits to be scheduled back onto the same thread that is already blocked waiting for the result, creating a circular dependency.
- The correct fix is to make the seeding method `async`, use `await` throughout, and call it from an `async` entry point such as the `Program.cs` top-level statements with `await` or inside an `async` `IHostedService.StartAsync` method.

---
