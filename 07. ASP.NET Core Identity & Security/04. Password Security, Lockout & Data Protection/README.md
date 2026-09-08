# Password Security, Lockout & Data Protection — Interview Q&A
> 27 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. How does ASP.NET Core Identity hash passwords by default, and what algorithm is used?](#q1-how-does-aspnet-core-identity-hash-passwords-by-default-and-what-algorithm-is-used)
2. [Q2. What is PBKDF2 and why does ASP.NET Core Identity use it instead of a faster hash like SHA-256?](#q2-what-is-pbkdf2-and-why-does-aspnet-core-identity-use-it-instead-of-a-faster-hash-like-sha-256)
3. [Q3. What does PasswordHasherOptions.IterationCount control, and what is its default value in ASP.NET Core?](#q3-what-does-passwordhasheroptionsiterationcount-control-and-what-is-its-default-value-in-aspnet-core)
4. [Q4. What are the two CompatibilityMode values in PasswordHasherOptions, and how do their hashed formats differ?](#q4-what-are-the-two-compatibilitymode-values-in-passwordhasheroptions-and-how-do-their-hashed-formats-differ)
5. [Q5. Describe the three members of the IPasswordHasher<TUser> interface and what each returns.](#q5-describe-the-three-members-of-the-ipasswordhashertuser-interface-and-what-each-returns)
6. [Q6. What are the three possible values of PasswordVerificationResult, and when is each one returned?](#q6-what-are-the-three-possible-values-of-passwordverificationresult-and-when-is-each-one-returned)
7. [Q7. How do you register a custom IPasswordHasher<TUser> implementation in the dependency injection container?](#q7-how-do-you-register-a-custom-ipasswordhashertuser-implementation-in-the-dependency-injection-container)
8. [Q8. What IdentityOptions.Password properties govern password complexity rules, and what are their defaults?](#q8-what-identityoptionspassword-properties-govern-password-complexity-rules-and-what-are-their-defaults)
9. [Q9. What does RequiredUniqueChars enforce, and what is its default value?](#q9-what-does-requireduniquechars-enforce-and-what-is-its-default-value)
10. [Q10. What IdentityOptions.Lockout properties control account lockout behaviour?](#q10-what-identityoptionslockout-properties-control-account-lockout-behaviour)
11. [Q11. How does MaxFailedAccessAttempts interact with LockoutEnabled to determine when a lockout is applied?](#q11-how-does-maxfailedaccessattempts-interact-with-lockoutenabled-to-determine-when-a-lockout-is-applied)
12. [Q12. How does SignInManager.PasswordSignInAsync integrate lockout checks into the sign-in pipeline?](#q12-how-does-signinmanagerpasswordsigninasync-integrate-lockout-checks-into-the-sign-in-pipeline)
13. [Q13. When is a user's failed access attempt count reset to zero, and what method performs that reset?](#q13-when-is-a-users-failed-access-attempt-count-reset-to-zero-and-what-method-performs-that-reset)
14. [Q14. How does LockoutEnd determine whether an account is currently locked out?](#q14-how-does-lockoutend-determine-whether-an-account-is-currently-locked-out)
15. [Q15. How do you programmatically unlock a locked-out account before its lockout period expires?](#q15-how-do-you-programmatically-unlock-a-locked-out-account-before-its-lockout-period-expires)
16. [Q16. Describe the end-to-end password reset flow in ASP.NET Core Identity.](#q16-describe-the-end-to-end-password-reset-flow-in-aspnet-core-identity)
17. [Q17. What is the purpose string for a password reset token, and why does it matter for token validation?](#q17-what-is-the-purpose-string-for-a-password-reset-token-and-why-does-it-matter-for-token-validation)
18. [Q18. How does ChangePasswordAsync differ from ResetPasswordAsync in terms of prerequisites and security guarantees?](#q18-how-does-changepasswordasync-differ-from-resetpasswordasync-in-terms-of-prerequisites-and-security-guarantees)
19. [Q19. What is the ASP.NET Core Data Protection API and what problem does it solve compared to raw cryptography?](#q19-what-is-the-aspnet-core-data-protection-api-and-what-problem-does-it-solve-compared-to-raw-cryptography)
20. [Q20. What is a purpose string in the Data Protection API, and how does it provide isolation between subsystems?](#q20-what-is-a-purpose-string-in-the-data-protection-api-and-how-does-it-provide-isolation-between-subsystems)
21. [Q21. How do you create an IDataProtector and use it to protect and unprotect arbitrary data?](#q21-how-do-you-create-an-idataprotector-and-use-it-to-protect-and-unprotect-arbitrary-data)
22. [Q22. Where are Data Protection keys stored by default in different hosting environments, and what are the implications?](#q22-where-are-data-protection-keys-stored-by-default-in-different-hosting-environments-and-what-are-the-implications)
23. [Q23. How does key rotation work in the Data Protection API, and what happens to data protected under an old key?](#q23-how-does-key-rotation-work-in-the-data-protection-api-and-what-happens-to-data-protected-under-an-old-key)
24. [Q24. How do you configure Data Protection to persist keys to a custom location such as Azure Blob Storage or the file system?](#q24-how-do-you-configure-data-protection-to-persist-keys-to-a-custom-location-such-as-azure-blob-storage-or-the-file-system)
25. [Q25. How does ASP.NET Core use the Data Protection API internally for authentication cookies and antiforgery tokens?](#q25-how-does-aspnet-core-use-the-data-protection-api-internally-for-authentication-cookies-and-antiforgery-tokens)
26. [Q26. What is the performance and security trade-off when increasing the PBKDF2 iteration count?](#q26-what-is-the-performance-and-security-trade-off-when-increasing-the-pbkdf2-iteration-count)
27. [Q27. How does the default PasswordHasher encode and embed the salt alongside the hashed password in the stored string?](#q27-how-does-the-default-passwordhasher-encode-and-embed-the-salt-alongside-the-hashed-password-in-the-stored-string)

---

## Q1. How does ASP.NET Core Identity hash passwords by default, and what algorithm is used?

**Concepts**
- `PasswordHasher<TUser>` — built-in default implementation
- PBKDF2 with HMACSHA256 in V3 mode
- Random salt per `HashPassword` call
- Self-describing stored format — version, algorithm, iterations, salt, subkey

**Answer**

ASP.NET Core Identity uses its built-in `PasswordHasher<TUser>` class, which derives a password hash using PBKDF2 (Password-Based Key Derivation Function 2) with HMACSHA256 as the pseudorandom function in the default V3 compatibility mode. PBKDF2 is a slow, iterated derivation function specifically designed for password hashing; its slowness is a security feature because it makes brute-force and dictionary attacks computationally expensive for an attacker. The default V3 format uses HMACSHA256, 310,000 iterations, a 16-byte random salt, and a 32-byte derived subkey — parameters chosen to take roughly 300 ms on typical server hardware. The salt is generated fresh for every `HashPassword` call, which means two users with the identical plaintext password will produce completely different stored hashes. The resulting hash is Base64-encoded and stored as a fully self-describing string that encodes the version byte, algorithm identifier, iteration count, salt, and derived key together, so the verifier can reconstruct the exact parameters used at hash time without any separate metadata lookup.

---

## Q2. What is PBKDF2 and why does ASP.NET Core Identity use it instead of a faster hash like SHA-256?

**Concepts**
- PBKDF2 (RFC 2898) — intentionally slow iterated key derivation
- Iteration count — operator-controlled cost, deters offline brute force
- Rainbow table defeat via unique random salt per password
- .NET BCL availability without additional NuGet dependencies

**Answer**

PBKDF2 (Password-Based Key Derivation Function 2), defined in RFC 2898, applies a pseudorandom function — such as HMACSHA256 — to a password and salt repeatedly for a configurable number of iterations, producing a fixed-length derived key. The intentional slowness is the critical design property: it makes each password check computationally expensive, which dramatically slows down an attacker who has obtained the database and is attempting offline brute-force cracking. A general-purpose hash like SHA-256 is engineered to be fast — modern GPU hardware can compute billions of SHA-256 hashes per second — making offline attacks against a leaked database trivially fast. PBKDF2's iteration count gives the operator direct control over cost: increasing the count keeps pace with hardware improvements by spending proportionally more CPU time on each hash, raising the attacker's cost in lockstep. Unlike a raw hash, PBKDF2 with a unique random salt per password naturally defeats precomputed rainbow-table attacks, because no generic table can cover arbitrary salts. Industry alternatives such as bcrypt and Argon2 share the slow-by-design philosophy; PBKDF2 is used by ASP.NET Core Identity because it is available in the .NET base class library (`System.Security.Cryptography`) without additional NuGet dependencies.

---

## Q3. What does PasswordHasherOptions.IterationCount control, and what is its default value in ASP.NET Core?

**Concepts**
- `IterationCount` — PBKDF2 work factor
- Self-describing hash — stored count governs future verification
- `SuccessRehashNeeded` — transparent incremental migration signal
- OWASP recommendation vs ASP.NET Core default

**Answer**

`PasswordHasherOptions.IterationCount` sets the number of PBKDF2 iterations performed when deriving a key from a password; a higher value means more CPU work per hash operation, making offline attacks slower at the cost of slower legitimate logins. In ASP.NET Core Identity V3 mode, the default is 310,000 iterations. The iteration count is encoded inside the stored hash string itself, so existing hashes are always verifiable with the correct parameters even after raising the option to a higher value in configuration. Raising `IterationCount` does not automatically re-hash stored passwords: existing passwords continue to use the count embedded in their stored bytes, and when a user next provides their plaintext password at login, `VerifyHashedPassword` returns `PasswordVerificationResult.SuccessRehashNeeded` if the stored count is below the current option, so `SignInManager` transparently re-hashes and saves the updated value. Setting the count too low makes offline attacks cheaper; setting it too high degrades login throughput under load — the right value is determined by benchmarking on production hardware and following current OWASP recommendations, which call for at least 600,000 iterations with HMACSHA256 as of 2023.

---

## Q4. What are the two CompatibilityMode values in PasswordHasherOptions, and how do their hashed formats differ?

**Concepts**
- `IdentityV2` — HMACSHA1, 1,000 iterations, format byte 0x00
- `IdentityV3` — HMACSHA256, 310,000 iterations, format byte 0x01
- `SuccessRehashNeeded` signal on V2 verification in a V3 application
- Backward compatibility — both formats recognised during verification

**Answer**

`PasswordHasherCompatibilityMode` has two values — `IdentityV2` and `IdentityV3` — that control which format is used when producing new hashes, and both formats are recognised during verification to support rolling upgrades without immediately invalidating every existing V2 hash.

| Property | IdentityV2 | IdentityV3 |
|---|---|---|
| PRF algorithm | HMACSHA1 | HMACSHA256 |
| Default iterations | 1,000 | 310,000 |
| Salt size | 16 bytes | 16 bytes |
| Subkey size | 32 bytes | 32 bytes |
| Format version byte | 0x00 | 0x01 |

`IdentityV2` exists for backward compatibility with ASP.NET Identity 2.x applications that have existing hashed passwords in the database; it uses HMACSHA1 and only 1,000 iterations, both of which are considered weak by current standards. `IdentityV3` is the default in all current ASP.NET Core Identity releases and should be used for every new application because HMACSHA256 is stronger and the iteration count is orders of magnitude higher. When the verifier encounters a V2-format hash — detected by the leading 0x00 byte — in a V3 application, it returns `PasswordVerificationResult.SuccessRehashNeeded`, signalling the caller to immediately hash the plaintext with V3 parameters and persist the new value.

---

## Q5. Describe the three members of the IPasswordHasher<TUser> interface and what each returns.

**Concepts**
- `HashPassword` — produces a self-describing Base64 string
- `VerifyHashedPassword` — constant-time comparison returning `PasswordVerificationResult`
- `TUser` parameter — enables per-user hashing strategy variation

**Answer**

`IPasswordHasher<TUser>` defines exactly three members: `HashPassword`, `VerifyHashedPassword`, and the generic type parameter `TUser` that represents the identity user class. The interface is the seam through which applications replace the built-in hashing strategy without modifying any other part of Identity's infrastructure. `HashPassword(TUser user, string password)` accepts the user object and the plaintext password and returns a Base64-encoded string containing the algorithm version, iteration count, salt, and derived key — everything needed to verify the same password later without any external metadata. `VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)` hashes `providedPassword` using the parameters extracted from `hashedPassword`, compares the two derived keys in a constant-time fashion, and returns a `PasswordVerificationResult` enum value indicating success, failure, or success with a re-hash signal. The `TUser` parameter is threaded through both methods so a custom implementation can vary its hashing strategy based on user properties — for example, applying a higher iteration count for accounts with administrative privileges — though the built-in implementation does not use it.

---

## Q6. What are the three possible values of PasswordVerificationResult, and when is each one returned?

**Concepts**
- `Failed` — hash mismatch, increment lockout counter
- `Success` — hash matches, format is current
- `SuccessRehashNeeded` — hash matches, outdated parameters signal transparent upgrade
- `CryptographicOperations.FixedTimeEquals` — timing side-channel prevention

**Answer**

`PasswordVerificationResult` is an enum with three values — `Failed`, `Success`, and `SuccessRehashNeeded` — that communicate both whether the password matched and whether the stored hash should be upgraded, and the caller is responsible for acting on `SuccessRehashNeeded`.

| Value | Meaning | Expected caller action |
|---|---|---|
| `Failed` | Password did not match | Reject the login, increment the lockout counter |
| `Success` | Password matched; hash format is current | Proceed with sign-in, no storage update needed |
| `SuccessRehashNeeded` | Password matched; hash uses outdated parameters | Re-hash with current settings, persist, then proceed |

`SuccessRehashNeeded` is returned when verification succeeds against a V2 hash in a V3 application, or when the stored iteration count is below the current `IterationCount` option, enabling a transparent incremental migration that requires no forced password reset. Treating `SuccessRehashNeeded` identically to `Success` without performing the re-hash is functionally safe but wastes the migration opportunity; users who never log in again will retain the weaker hash indefinitely. The comparison inside `VerifyHashedPassword` uses `CryptographicOperations.FixedTimeEquals` to prevent timing side-channel attacks that could reveal partial hash matches to an attacker monitoring network latency.

---

## Q7. How do you register a custom IPasswordHasher<TUser> implementation in the dependency injection container?

**Concepts**
- Last-registered implementation wins — override after `AddIdentity`
- `SuccessRehashNeeded` for legacy format detection
- Self-describing custom format embedding all verification parameters
- `AddSingleton` safe for stateless hashers

**Answer**

A custom `IPasswordHasher<TUser>` is registered by calling `AddScoped` or `AddSingleton` on the service collection after the `AddIdentity` or `AddDefaultIdentity` call, because ASP.NET Core's DI container resolves the last-registered implementation for a given interface, overriding the built-in default.

```csharp
builder.Services.AddDefaultIdentity<ApplicationUser>(options => { })
    .AddEntityFrameworkStores<AppDbContext>();

// The later registration wins and replaces the built-in hasher
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>,
    MyCustomPasswordHasher>();
```

The custom class must implement both `HashPassword` and `VerifyHashedPassword`, and it must return `PasswordVerificationResult.SuccessRehashNeeded` for any legacy hash format it can still verify but considers outdated, to trigger transparent upgrade at the next successful login. If the custom hasher wraps a third-party library such as Argon2, the stored format must embed all parameters — memory cost, time cost, parallelism factor, salt — so that verification is self-contained and survives future configuration changes. Registering as `AddSingleton` is safe only if the implementation is completely stateless; most hashers have no per-request state and can be singletons, avoiding repeated instantiation under high login load.

---

## Q8. What IdentityOptions.Password properties govern password complexity rules, and what are their defaults?

**Concepts**
- `PasswordOptions` — six configurable complexity properties
- `IPasswordValidator<TUser>` — pluggable validation pipeline
- `RequiredLength` as minimum, not maximum
- Default strictness by design — must consciously relax

**Answer**

`IdentityOptions.Password` is an instance of `PasswordOptions` that exposes six properties controlling what a valid password must contain. These rules are enforced by `UserManager.CreateAsync`, `UserManager.ChangePasswordAsync`, and `UserManager.ResetPasswordAsync` before the password is ever hashed.

| Property | Default | Meaning |
|---|---|---|
| `RequiredLength` | 6 | Minimum total character count |
| `RequireDigit` | `true` | Must contain at least one digit (0–9) |
| `RequireLowercase` | `true` | Must contain at least one lowercase letter |
| `RequireUppercase` | `true` | Must contain at least one uppercase letter |
| `RequireNonAlphanumeric` | `true` | Must contain at least one special character |
| `RequiredUniqueChars` | 1 | Minimum count of distinct characters |

These defaults are deliberately strict so that applications must consciously relax them rather than accidentally shipping with weak complexity requirements. Password validation is performed by all registered `IPasswordValidator<TUser>` instances; the built-in `PasswordValidator<TUser>` reads the `PasswordOptions` values, and additional validators can be added to the pipeline for custom rules such as checking against a list of common passwords. `RequiredLength` is a minimum, not a maximum; ASP.NET Core Identity imposes no upper length bound on passwords, though PBKDF2 with HMACSHA256 effectively truncates input beyond 512 bytes, which is far above any practical limit.

---

## Q9. What does RequiredUniqueChars enforce, and what is its default value?

**Concepts**
- `RequiredUniqueChars` — distinct character count, not character-class count
- Default of 1 — minimal restriction by design
- Orthogonality to character-class requirements
- Low-entropy repetition patterns it prevents

**Answer**

`PasswordOptions.RequiredUniqueChars` specifies the minimum number of distinct Unicode characters that a password must contain; the default is 1, meaning only one unique character is required, which is effectively no meaningful restriction on repetition. Setting it to a higher value — typically 4 to 8 — prevents low-entropy passwords like `aaaaaaaa` or `12121212` that satisfy length and character-class requirements but are trivially guessable. The check counts distinct characters as `password.Distinct().Count()` using ordinal character comparison, and the result must be greater than or equal to `RequiredUniqueChars`. The default of 1 is intentionally minimal to avoid breaking existing applications on upgrade; any security-conscious application should raise it to at least 4. `RequiredUniqueChars` is orthogonal to the character-class requirements: a password like `aAbBcCdD` satisfies both `RequireUppercase` and `RequireLowercase` and has 8 unique characters, while `aaAAAA11` satisfies the character-class checks but has only 3 unique characters, illustrating that character classes and uniqueness measure different entropy dimensions.

---

## Q10. What IdentityOptions.Lockout properties control account lockout behaviour?

**Concepts**
- `LockoutOptions` — three configurable properties
- `AllowedForNewUsers` — per-account lockout eligibility flag
- Per-user `LockoutEnabled` override via `SetLockoutEnabledAsync`
- `LockoutEnd` persisted to database — survives restarts

**Answer**

`IdentityOptions.Lockout` is a `LockoutOptions` instance with three key properties that together determine when and how long an account is locked after too many failed sign-in attempts.

| Property | Default | Meaning |
|---|---|---|
| `MaxFailedAccessAttempts` | 5 | Consecutive failures before lockout is triggered |
| `DefaultLockoutTimeSpan` | 5 minutes | Duration of each lockout period |
| `AllowedForNewUsers` | `true` | Whether lockout is enabled on newly created accounts |

`AllowedForNewUsers` controls whether a freshly created user starts with `LockoutEnabled = true` in the database; setting it to `false` means no new account can be locked, which is rarely appropriate in production but useful in testing environments where automated scripts repeatedly exercise login flows. These are application-wide defaults; per-user lockout can be overridden by calling `UserManager.SetLockoutEnabledAsync`, which is useful for service accounts that should never be locked. The lockout state is stored as `LockoutEnd` (a `DateTimeOffset?`) in the `AspNetUsers` table, so the lockout survives application restarts and is enforced consistently across multiple server instances in a load-balanced deployment without any distributed coordination.

---

## Q11. How does MaxFailedAccessAttempts interact with LockoutEnabled to determine when a lockout is applied?

**Concepts**
- Both conditions required — `LockoutEnabled` AND threshold reached
- `AccessFailedAsync` — increment and optional `SetLockoutEndDateAsync`
- Service account pattern — `LockoutEnabled = false` at creation

**Answer**

A lockout is triggered only when both conditions are satisfied simultaneously: the user's `LockoutEnabled` flag in the database is `true`, and the count of consecutive failed access attempts has reached `MaxFailedAccessAttempts`. If `LockoutEnabled` is `false` for a specific user, no lockout is ever applied to that account regardless of how many failures accumulate. `UserManager.AccessFailedAsync` increments the `AccessFailedCount` column and, only when `LockoutEnabled` is true and the threshold is reached, calls `SetLockoutEndDateAsync` to set `LockoutEnd` to `DateTimeOffset.UtcNow + DefaultLockoutTimeSpan`. `LockoutEnabled` is set to `true` by default for all new users when `LockoutOptions.AllowedForNewUsers` is `true`, so in the standard configuration every human account starts eligible for lockout from the first login attempt. A common production pattern is to create service accounts or API credentials with `LockoutEnabled = false` at the time of creation, while keeping lockout active for all human users, preventing automated integrations from triggering accidental self-lockouts.

---

## Q12. How does SignInManager.PasswordSignInAsync integrate lockout checks into the sign-in pipeline?

**Concepts**
- Lockout-first check — `IsLockedOutAsync` before password verification
- `AccessFailedAsync` on failure — may trigger fresh lockout
- `ResetAccessFailedCountAsync` on success
- Manual lockout sequence for custom API token endpoints

**Answer**

`SignInManager.PasswordSignInAsync` orchestrates the entire lockout-aware sign-in sequence internally: it first checks whether the account is currently locked out and, if so, returns `SignInResult.LockedOut` without ever attempting password verification. If the account is accessible, it verifies the password and on failure calls `UserManager.AccessFailedAsync` to increment the count and potentially trigger a fresh lockout. The internal sequence is:

1. `IsLockedOutAsync` — if `LockoutEnd > UtcNow`, return `SignInResult.LockedOut` immediately.
2. `CheckPasswordSignInAsync` — verify the provided password against the stored hash.
3. On success: call `UserManager.ResetAccessFailedCountAsync`, issue the authentication cookie, return `SignInResult.Success`.
4. On failure: call `UserManager.AccessFailedAsync` (increments count, may set `LockoutEnd`), return `SignInResult.Failed`.

When building custom sign-in flows for API token endpoints that do not use cookie-based authentication, the application must call `UserManager.IsLockedOutAsync`, `UserManager.CheckPasswordAsync`, `UserManager.AccessFailedAsync`, and `UserManager.ResetAccessFailedCountAsync` manually in the correct sequence. The lockout check is a simple timestamp comparison against `LockoutEnd` in the database; the lock expires naturally when UTC time passes the stored value, without any background job or scheduler.

---

## Q13. When is a user's failed access attempt count reset to zero, and what method performs that reset?

**Concepts**
- `ResetAccessFailedCountAsync` — sets `AccessFailedCount` to 0 in the database
- Reset only on successful password verification
- MFA flow consideration — first factor success resets count
- Custom `SignInAsync` bypass — must call reset manually

**Answer**

The failed access attempt count is reset to zero upon a successful password verification, and the method responsible is `UserManager.ResetAccessFailedCountAsync`. `SignInManager.PasswordSignInAsync` calls this automatically after a successful login, but custom authentication flows must invoke it explicitly. `ResetAccessFailedCountAsync` sets the `AccessFailedCount` column to 0 in the database; it does not clear `LockoutEnd` — that field expires naturally by time — but it prevents the count from immediately re-triggering a lockout on the next failure. The reset occurs only after the password is confirmed correct; navigating to the login page, submitting a username without a password, or completing the first factor of a multi-factor flow does not reset the count. In MFA flows, the first-factor password check resets the count when it passes; whether a failed second factor increments the count again is an application-level decision and must be handled by calling `AccessFailedAsync` explicitly if desired. An application that bypasses `PasswordSignInAsync` in favour of calling `SignInManager.SignInAsync` directly — for example, after a social login — must call `ResetAccessFailedCountAsync` separately, otherwise the count drifts upward from previous failed attempts.

---

## Q14. How does LockoutEnd determine whether an account is currently locked out?

**Concepts**
- `LockoutEnd` as `DateTimeOffset?` — null or past value means unlocked
- `DateTimeOffset.MaxValue` for indefinite deactivation
- Pure timestamp comparison — no background cleanup required
- `SetLockoutEndDateAsync(user, null)` for admin unlock

**Answer**

`LockoutEnd` is a nullable `DateTimeOffset` column in the `AspNetUsers` table; an account is considered locked out when `LockoutEnd` is non-null and its value is greater than the current UTC time. When the lockout period elapses naturally, the column value remains in the database but the comparison `LockoutEnd > DateTimeOffset.UtcNow` evaluates to false, so Identity treats the account as unlocked without any background cleanup. Setting `LockoutEnd` to `null` via `UserManager.SetLockoutEndDateAsync(user, null)` is the correct way to immediately unlock an account from code — for example, in an administrator "unlock user" action. Setting `LockoutEnd` to `DateTimeOffset.MaxValue` (or any date far in the future) effectively permanently disables the account, which is a common pattern for soft-deactivating an account without deleting its row from the database. Because the check is a pure timestamp comparison, the mechanism works correctly across application restarts, multiple server instances, and time zone differences, provided all servers use UTC consistently, which `DateTimeOffset.UtcNow` enforces.

---

## Q15. How do you programmatically unlock a locked-out account before its lockout period expires?

**Concepts**
- `SetLockoutEndDateAsync` with `DateTimeOffset.UtcNow` or `null`
- `ResetAccessFailedCountAsync` after unlock — prevent immediate re-lockout
- Admin endpoint authorization requirement

**Answer**

The correct approach is to call `UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow)` or pass `null` as the end date; either causes the `LockoutEnd > UtcNow` check to evaluate to false on the very next sign-in attempt.

```csharp
var user = await _userManager.FindByEmailAsync(email);
await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
await _userManager.ResetAccessFailedCountAsync(user);
```

Calling `ResetAccessFailedCountAsync` afterward is important because without it, `AccessFailedCount` remains at its maximum value; the very first post-unlock failed login will call `AccessFailedAsync` and push the count one step above the threshold, immediately triggering a new lockout and trapping a legitimate user in a loop. Setting `LockoutEnd` to `DateTimeOffset.UtcNow` is equivalent in practice to passing `null`, because the value is immediately in the past; `null` is slightly more expressive of the intent and is the value stored after a natural expiry. An admin unlock endpoint must be protected by an authorization policy that restricts access to privileged roles; without such a guard, any authenticated user who can reach the endpoint could unlock any account.

---

## Q16. Describe the end-to-end password reset flow in ASP.NET Core Identity.

**Concepts**
- Two-phase flow — token generation then token redemption
- `GeneratePasswordResetTokenAsync` — time-limited `DataProtectorTokenProvider` token
- URL-encoding the token before embedding in email link
- `ResetPasswordAsync` — validates token, updates `SecurityStamp`, invalidates sessions

**Answer**

The password reset flow has two distinct phases separated by an email delivery step: token generation followed by token redemption. In the first phase, `UserManager.GeneratePasswordResetTokenAsync(user)` produces a short-lived, cryptographically signed token that is embedded in a link and sent to the user's verified email address. In the second phase, the user submits that token along with their new password to `UserManager.ResetPasswordAsync(user, token, newPassword)`, which validates the token and, if valid, replaces the stored password hash. The end-to-end sequence is:

1. User requests a reset → `GeneratePasswordResetTokenAsync(user)` → URL-encode the token → embed in an email link.
2. User clicks the link → application receives `userId` and `token` from the query string.
3. `FindByIdAsync(userId)` or `FindByEmailAsync(email)` to load the user record.
4. `ResetPasswordAsync(user, token, newPassword)` → validates token, enforces `PasswordOptions`, hashes and persists the new password, updates `SecurityStamp`, invalidates the token.

The token is produced by `DataProtectorTokenProvider`, which uses the Data Protection API internally; it encodes the user's ID, security stamp, and an expiry timestamp, making it single-use and time-limited (default lifetime is one day, configurable via `TokenProviderOptions.TokenLifespan`). URL-encoding the token before embedding it in an email link is mandatory because the Base64 output may contain `+`, `/`, and `=` characters that browsers treat as URL delimiters, corrupting the token during round-trip. `ResetPasswordAsync` returns an `IdentityResult` whose `Errors` collection details any password complexity violations; the caller must surface these errors to the user rather than silently treating a failed result as a token error.

---

## Q17. What is the purpose string for a password reset token, and why does it matter for token validation?

**Concepts**
- Purpose string `"ResetPassword"` — cryptographic domain separator
- Purpose incorporated into key derivation — cross-purpose substitution prevented
- `GenerateUserTokenAsync` with custom purposes for domain-specific tokens

**Answer**

The purpose string for a password reset token is `"ResetPassword"`, set internally by `DataProtectorTokenProvider<TUser>` when it creates the token. The purpose string acts as a cryptographic domain separator: a token generated for `"ResetPassword"` cannot be accepted by a validator checking against `"EmailConfirmation"`, even if both tokens were signed by the same key ring. The Data Protection API incorporates the purpose string into the key derivation step that produces the working encryption key from the master key, so a `"ResetPassword"` ciphertext will throw `CryptographicException` when decrypted by a protector created with any other purpose string. This isolation is important because a password reset link and an email confirmation link are often sent to the same address; without purpose separation, a valid email confirmation token could be substituted for a reset token (or vice versa) and accepted by a careless implementation. Developers using `UserManager.GenerateUserTokenAsync(user, providerName, customPurpose)` directly can supply any custom purpose string to create tokens for domain-specific operations — such as magic-link login or account deletion confirmation — and those tokens will be completely isolated from all built-in token purposes.

---

## Q18. How does ChangePasswordAsync differ from ResetPasswordAsync in terms of prerequisites and security guarantees?

**Concepts**
- `ChangePasswordAsync` — requires current password as identity proof
- `ResetPasswordAsync` — accepts time-limited token in place of current password
- Both methods update `SecurityStamp` — invalidating all existing sessions
- NIST guidance favouring explicit re-authentication for in-session changes

**Answer**

`ChangePasswordAsync` requires the user to supply their current password as proof of identity before the new password is accepted, making it appropriate for authenticated sessions where the user already knows their password and wishes to update it. `ResetPasswordAsync` accepts a time-limited token in place of the current password, making it the correct path for users who have forgotten their password and must verify their identity through an out-of-band channel such as email.

| Aspect | ChangePasswordAsync | ResetPasswordAsync |
|---|---|---|
| Caller must be | Authenticated | Anonymous (token-verified) |
| Identity proof | Current password (verified against stored hash) | Signed, time-limited reset token |
| Enforces PasswordOptions | Yes | Yes |
| Updates SecurityStamp | Yes | Yes |
| Invalidates existing sessions | Yes (via stamp change) | Yes (via stamp change) |

Both methods update `SecurityStamp` after a successful change, which invalidates any existing authentication cookies or bearer tokens issued before the stamp changed, effectively signing the user out of all other devices and browser sessions. `ChangePasswordAsync` verifies the old password by calling `VerifyHashedPassword` internally, so it respects whatever `IPasswordHasher` is registered, including custom implementations. Calling `ResetPasswordAsync` when the user is already authenticated and knows their password is technically possible but bypasses the explicit re-authentication that `ChangePasswordAsync` provides; NIST guidance favours the explicit credential confirmation for in-session password changes.

---

## Q19. What is the ASP.NET Core Data Protection API and what problem does it solve compared to raw cryptography?

**Concepts**
- Data Protection API — `Protect`/`Unprotect` interface replacing `MachineKey`
- Automatic key management — generation, rotation, and retirement
- Authenticated encryption — both confidentiality and tamper detection
- Internal use for cookies, antiforgery, OAuth state, and Identity tokens

**Answer**

The ASP.NET Core Data Protection API is a built-in cryptographic system that provides authenticated encryption and decryption of arbitrary byte payloads through a simple `Protect`/`Unprotect` interface, without requiring developers to manage keys, algorithms, or initialisation vectors directly. It replaces the insecure `MachineKey` infrastructure from classic ASP.NET and treats key management as a first-class, solved problem. Raw cryptography requires the developer to choose an algorithm, generate and securely store a key, produce a fresh IV for every encryption, and authenticate the ciphertext to prevent tampering; the Data Protection API handles all of these steps correctly by default. The API is used internally by ASP.NET Core for protecting authentication cookies, antiforgery tokens, OAuth state parameters, and two-factor authentication tokens, ensuring that all built-in security features share a single, correctly configured cryptographic system. Key ring management — generating new keys before active keys expire, retaining retired keys for decryption of existing data, and optionally encrypting keys at rest — is handled automatically, requiring only configuration of the storage backend and, optionally, a key encryption provider.

---

## Q20. What is a purpose string in the Data Protection API, and how does it provide isolation between subsystems?

**Concepts**
- Purpose string — arbitrary domain separator passed to `CreateProtector`
- Per-purpose working key derived from master key — cross-purpose decryption fails
- Chained purposes — hierarchical namespace for sub-features
- Purposes are not secrets — domain separators, not cryptographic material

**Answer**

A purpose string is an arbitrary non-empty string passed to `IDataProtectionProvider.CreateProtector(purpose)` that binds the resulting `IDataProtector` to a specific use case; data protected with one purpose string cannot be decrypted by a protector created for a different purpose string, even if both share the same underlying key ring. The purpose string is incorporated into a key derivation step that produces a per-purpose working key from the master key; any `Unprotect` call using a different purpose-derived key will produce a different working key, causing authenticated decryption to fail and throw `CryptographicException`. Purposes can be chained: `provider.CreateProtector("Level1").CreateProtector("Level2")` is equivalent to `provider.CreateProtector("Level1", "Level2")`, creating a hierarchical namespace that further isolates sub-features within the same subsystem. Purpose strings do not need to be secret; they are domain separators, not secrets, and ASP.NET Core uses fully qualified type names as purpose strings internally — for example, `Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware` — to guarantee uniqueness across assemblies.

---

## Q21. How do you create an IDataProtector and use it to protect and unprotect arbitrary data?

**Concepts**
- `IDataProtectionProvider.CreateProtector` — obtains a purpose-bound protector
- `ITimeLimitedDataProtector` — adds expiry to protected payloads
- `CryptographicException` on tamper, expiry, or wrong purpose — must be caught

**Answer**

An `IDataProtector` is obtained by calling `CreateProtector` on an `IDataProtectionProvider`, which is registered automatically in the DI container by `AddDataProtection`. The `Protect` method accepts a plaintext byte array or string and returns an opaque, authenticated ciphertext; `Unprotect` reverses the operation and throws `CryptographicException` if the payload has been tampered with, expired, or was protected under a different purpose.

```csharp
public class TokenService
{
    private readonly IDataProtector _protector;

    public TokenService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("MyApp.SensitiveData.v1");
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string ciphertext) => _protector.Unprotect(ciphertext);
}
```

`IDataProtector` extends `IDataProtectionProvider`, so a protector can itself create child protectors by calling `CreateProtector` on it, enabling subsystem hierarchies. `ITimeLimitedDataProtector` (obtained via `protector.ToTimeLimitedDataProtector()`) adds an expiry timestamp to the payload so that `Unprotect` throws `CryptographicException` if the token has expired — which is exactly how password reset and email confirmation tokens enforce their lifetime without a database table of issued tokens. Callers should always catch `CryptographicException` from `Unprotect` and treat it as an invalid or tampered token rather than an unhandled server error, returning an appropriate user-facing message such as "the link is invalid or has expired."

---

## Q22. Where are Data Protection keys stored by default in different hosting environments, and what are the implications?

**Concepts**
- Platform-specific default storage — in-memory on Linux without explicit config
- In-memory keys lost on restart — authentication breaks for all users
- Per-instance key rings in containers — cross-instance decryption failures
- Shared persistent storage as the production requirement

**Answer**

The default key storage location depends on the operating system and hosting model: on Windows outside IIS, keys are persisted to the DPAPI-encrypted folder `%LOCALAPPDATA%\ASP.NET\DataProtection-Keys`; under IIS with `setProfileEnvironment` enabled, they go to the application pool identity's user profile; on Linux and macOS, they are stored in memory only and are lost when the process restarts. The in-memory default on Linux is the most consequential behaviour, because it means authentication cookies, antiforgery tokens, and any custom protected data become permanently unreadable after every restart. On any platform without explicit persistence configuration, a multi-instance deployment will generate separate, independent key rings per instance, causing tokens and cookies produced by one instance to fail decryption on another. In container environments, the process user typically has no persistent writable file system, so keys stored in the default location vanish on container replacement; a volume mount or external key store is required for persistence. The recommended production configuration is to persist keys to a shared location — Azure Blob Storage, Redis via `StackExchange.Redis`, a SQL database via the `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` package, or a network share — combined with key encryption at rest using a certificate or Azure Key Vault.

---

## Q23. How does key rotation work in the Data Protection API, and what happens to data protected under an old key?

**Concepts**
- Automatic rotation — new key generated before active key reaches expiry
- Retired keys retained for decryption — backward compatibility without re-encryption
- `IKeyManager.RevokeKey` — explicit early revocation on compromise
- Discarding the key ring — equivalent to revoking all keys at once

**Answer**

The Data Protection system automatically generates a new key before the active key's expiry date — by default, keys are created with a 90-day lifetime and a new key is generated when the active key has less than 14 days remaining — so there is always a fresh active key available. Old keys are retained in the key ring in a "retired" state and remain available for decrypting any data protected under them until they are explicitly revoked or the key ring is discarded. The system does not re-encrypt existing data when a new key becomes active; `Protect` calls use the current active key, while `Unprotect` tries the key identified in the ciphertext header. As long as the key ring persists and old keys are not revoked, previously protected data — authentication cookies, tokens, antiforgery values — continues to decrypt successfully even after multiple key rotations. Explicit key revocation is possible via `IKeyManager.RevokeKey(keyId, reason)`; a revoked key causes `Unprotect` to fail for any payload it protected even before the key would naturally expire, which is useful for responding to a key compromise. Replacing the entire key ring (for example, by deleting the key storage location) is equivalent to revoking all keys simultaneously, immediately invalidating all existing sessions and tokens in every deployed instance.

---

## Q24. How do you configure Data Protection to persist keys to a custom location such as Azure Blob Storage or the file system?

**Concepts**
- `PersistKeysToFileSystem` / `PersistKeysToAzureBlobStorage` — storage configuration
- Key persistence and key encryption at rest as separate concerns
- `SetApplicationName` — isolation when multiple applications share a key ring

**Answer**

Key persistence is configured by chaining extension methods on the `IDataProtectionBuilder` returned by `builder.Services.AddDataProtection()`. For the file system, `PersistKeysToFileSystem` accepts a `DirectoryInfo`; for Azure Blob Storage, the `Azure.Extensions.AspNetCore.DataProtection.Blobs` package provides `PersistKeysToAzureBlobStorage`; for Redis, `PersistKeysToStackExchangeRedis` is available from `Microsoft.AspNetCore.DataProtection.StackExchangeRedis`.

```csharp
builder.Services.AddDataProtection()
    .SetApplicationName("MyApp")
    .PersistKeysToFileSystem(new DirectoryInfo("/mnt/shared/dp-keys"))
    .ProtectKeysWithCertificate("thumbprint");
```

Key persistence and key encryption at rest are separate concerns: `PersistKeysToFileSystem` controls where the XML key ring files are written, while `ProtectKeysWithCertificate` or `ProtectKeysWithAzureKeyVault` controls whether the key material inside those files is encrypted before storage. Without key encryption at rest, the key ring files contain raw key material in plaintext XML protected only by file system permissions; encrypting them with a certificate means a compromised storage account or file share does not expose the actual cryptographic keys. `SetApplicationName` should be called when multiple applications share a key ring storage location, to ensure their purpose-derived working keys remain isolated even though they read the same physical key files; without it, two applications sharing a ring could theoretically decrypt each other's protected data.

---

## Q25. How does ASP.NET Core use the Data Protection API internally for authentication cookies and antiforgery tokens?

**Concepts**
- Cookie middleware — `IDataProtector.Protect` with scheme-specific purpose string
- Antiforgery — two-token pattern protecting against CSRF and replay
- Shared key ring — loss invalidates cookies, tokens, and password reset links simultaneously
- `DataProtectorTokenProvider<TUser>` — email confirmation and password reset

**Answer**

When a user authenticates, the cookie authentication middleware serialises the `ClaimsPrincipal` and authentication properties into a byte array, then calls `IDataProtector.Protect` with a scheme-specific purpose string before writing the encrypted, authenticated value into the `Set-Cookie` response header. On subsequent requests, the middleware reads the cookie, calls `Unprotect`, and rejects the cookie with a `CryptographicException` if the payload has been tampered with or the key that protected it is no longer in the ring. The antiforgery system uses the Data Protection API in the same way: the form token embedded in an HTML hidden input is protected with a purpose string like `Microsoft.AspNetCore.Antiforgery.AntiforgeryToken`, and the corresponding cookie value is protected with a paired purpose; the two-token pattern prevents simple replay and cross-site request forgery attacks. Because authentication cookies and antiforgery tokens share the same key ring, replacing or losing the ring — for example, redeploying a container without shared key persistence — immediately invalidates all active user sessions and all in-flight form submissions across the entire application. `DataProtectorTokenProvider<TUser>`, which backs `UserManager`'s email confirmation and password reset tokens, also uses `ITimeLimitedDataProtector` from the same ring, meaning that a key ring change also invalidates all outstanding password reset and email confirmation links.

---

## Q26. What is the performance and security trade-off when increasing the PBKDF2 iteration count?

**Concepts**
- Symmetric cost increase — attacker and server both pay proportionally
- Attacker asymmetry — billions of guesses vs one legitimate check
- Login throughput degradation under concurrent load
- Annual iteration count review as hardware improves

**Answer**

Increasing the PBKDF2 iteration count makes each hash operation proportionally slower for both the server and an offline attacker; the increase is symmetric — doubling the count doubles the cost for everyone. The asymmetry that favours the defender is that the attacker must run billions of guesses in parallel, so the iteration cost multiplies across every guess, while the server handles only one legitimate hash per login, though that single hash still consumes wall-clock time and CPU that accumulates under load. A server handling 1,000 concurrent login requests, each taking 300 ms of PBKDF2 CPU time at 310,000 iterations, needs dedicated thread pool capacity for hashing; doubling to 600,000 iterations approximately doubles each request's CPU time and thread-hold duration, increasing the risk of thread pool starvation under login bursts. The correct approach is to benchmark the target count on representative production hardware, measure p99 login latency under a realistic concurrent load, and choose the highest count that stays within the acceptable latency budget — then revisit annually as hardware improves. The self-describing hash format makes it safe to raise the count in configuration at any time; existing logins re-hash transparently at the next successful sign-in via the `SuccessRehashNeeded` signal, without any forced password reset.

---

## Q27. How does the default PasswordHasher encode and embed the salt alongside the hashed password in the stored string?

**Concepts**
- Self-describing byte layout — version, PRF, iterations, salt length, salt, subkey
- `RandomNumberGenerator.GetBytes(16)` — unique salt per hash
- Standard Base64 encoding — safe in `nvarchar` but must be URL-encoded in links

**Answer**

The default `PasswordHasher<TUser>` stores the hash as a Base64-encoded concatenation of fields packed into a byte array: a single format version byte (0x01 for V3), a 4-byte big-endian integer identifying the pseudorandom function, a 4-byte big-endian integer for the iteration count, a 4-byte big-endian integer for the salt length, a 16-byte random salt, and finally the 32-byte PBKDF2-derived subkey.

```
Byte layout (54 bytes total, before Base64):
[0x01][PRF id: 4 bytes][Iterations: 4 bytes][SaltLen: 4 bytes]
[Salt: 16 bytes][Subkey: 32 bytes]
```

The salt is generated by `RandomNumberGenerator.GetBytes(16)` at hash time, guaranteeing uniqueness per password hash regardless of password length or content. Because the format is self-describing, changing `IterationCount` in `PasswordHasherOptions` does not break verification of any existing hash: the verifier reads the iteration count from the stored bytes, not from configuration, and only returns `SuccessRehashNeeded` when the stored count is lower than the current option value. The entire byte array is encoded using standard Base64, producing a string that may contain `+`, `/`, and `=` characters; these are safe in a database `nvarchar` column but must be URL-encoded before embedding in a query string or a link, since they would otherwise be misinterpreted as URL delimiters.

---

## Gotchas — Password Security & Data Protection (Interview Traps)

---

#### Gotcha 1. Raising the iteration count is symmetric — it slows the server as much as the attacker

**Concepts**
- Symmetric cost increase — no asymmetric defensive win
- Attacker parallelism advantage — billions of guesses vs one legitimate login
- Thread pool starvation under concurrent login load

**Answer**

Developers sometimes assume that increasing PBKDF2's iteration count is a pure defensive win that hurts only attackers. In reality, the cost increase is completely symmetric: every hash takes proportionally longer for the server too, directly reducing the number of logins the application can process per second under concurrent load. The asymmetry that favours defence is that attackers run billions of guesses per second in parallel across GPU clusters, so iteration cost multiplies enormously on their side, while the server handles only one legitimate hash per login. However, that cost accumulates across all concurrent logins, so the iteration count must always be benchmarked on production hardware under realistic concurrent load before deployment, and the thread pool budget must be sized to absorb the added latency during peak login traffic.

---

#### Gotcha 2. Changing IterationCount does not automatically upgrade existing password hashes

**Concepts**
- `IterationCount` applies only to newly set passwords
- `SuccessRehashNeeded` — transparent migration on next login
- Long-inactive users retain weaker hash indefinitely

**Answer**

Raising `PasswordHasherOptions.IterationCount` in configuration takes effect only for newly set passwords; every existing hash in the database continues to use the iteration count embedded in its stored bytes until that user logs in successfully and the application re-hashes the plaintext. The migration is incremental and transparent: `VerifyHashedPassword` returns `SuccessRehashNeeded` when the stored iteration count is below the current option, and `SignInManager.PasswordSignInAsync` automatically calls `UserManager.UpdateAsync` with the upgraded hash, but only for users who actively log in. Users who never return retain the weaker hash indefinitely; if a complete migration is required, the correct approach is a background job that forces a password reset for long-inactive accounts rather than waiting for spontaneous logins.

---

#### Gotcha 3. Data Protection keys in memory are lost on restart, invalidating all protected data

**Concepts**
- In-memory default on Linux and containers — keys vanish on restart
- Per-instance key ring in load-balanced deployments — cross-server decryption failure
- `PersistKeysToFileSystem` / `PersistKeysToAzureBlobStorage` as the fix

**Answer**

On Linux and macOS, and in containerised environments without explicit persistence configuration, the Data Protection key ring lives only in process memory; when the process restarts, a brand-new ring is generated and all previously protected data — authentication cookies, antiforgery tokens, password reset links — becomes permanently unreadable. In a load-balanced or container deployment, each instance independently generates its own key ring, so a cookie minted by one instance will throw `CryptographicException` when decrypted by another, effectively logging every user out on every non-sticky request. The fix is to call `PersistKeysToFileSystem`, `PersistKeysToAzureBlobStorage`, or another persistence extension at startup, pointing all instances at a shared storage location; this single configuration change eliminates the entire class of cross-instance and restart-driven token invalidation problems.

---

#### Gotcha 4. A naturally expired lockout does not reset AccessFailedCount

**Concepts**
- Natural lockout expiry — `LockoutEnd` passes, count remains at threshold
- Immediate re-lockout on first post-expiry failure
- `ResetAccessFailedCountAsync` called only on successful login

**Answer**

When a lockout period expires naturally — the current UTC time passes the stored `LockoutEnd` value — the account becomes accessible again, but `AccessFailedCount` in the database remains at its maximum value. The count is not reset automatically by the passage of time. Because `AccessFailedCount` is still at the threshold, the very first failed login after the natural expiry will call `AccessFailedAsync`, push the count one step above the threshold, and immediately re-trigger a lockout — trapping a legitimate user in a loop of expiring-then-re-locking accounts. `SignInManager.PasswordSignInAsync` handles this correctly by calling `ResetAccessFailedCountAsync` on every successful login; custom sign-in flows that call `UserManager.CheckPasswordAsync` and `AccessFailedAsync` directly must also call the count reset on success to avoid this trap.

---

#### Gotcha 5. Purpose strings in the Data Protection API are domain separators, not secrets

**Concepts**
- Purpose strings — cryptographic domain separators, not keys
- Rotating purpose strings — unnecessary operational disruption
- Security comes from the key ring, not the purpose string

**Answer**

Developers sometimes treat purpose strings as sensitive configuration values and try to keep them private, believing that secrecy provides additional protection. Purpose strings provide cryptographic isolation between subsystems — a token for one purpose cannot satisfy validation for another — but they are not a source of cryptographic strength; that comes entirely from the key ring. Incorporating the purpose string into the key derivation step means knowing the purpose string gives an attacker no advantage without also having access to the master key, so there is nothing gained by treating purpose strings as secrets. Treating a purpose string as a secret and then rotating it as part of a secret-rotation policy would invalidate all existing tokens protected under that purpose — an unnecessary and disruptive operational event that provides no security improvement.

---

#### Gotcha 6. Lockout Requires `lockoutOnFailure: true` on Every `PasswordSignInAsync` Call

**Concepts**
- `lockoutOnFailure` parameter controlling per-call lockout tracking — not a global setting
- Calling with `false` never incrementing `AccessFailedCount` regardless of `LockoutOptions`
- Brute-force window opened when the parameter is omitted or set to `false`

**Answer**

Account lockout is triggered only when `PasswordSignInAsync` is called with `lockoutOnFailure: true`. Passing `false` — or building a custom authentication flow that calls `CheckPasswordAsync` instead — means failed attempts are never counted toward the lockout threshold, regardless of how `LockoutOptions.MaxFailedAccessAttempts` is configured. Many tutorials demonstrate `PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false)` as a shortcut to avoid accidental lockout during development, but this pattern carries into production code and silently disables lockout for every login attempt. The safe default is always `lockoutOnFailure: true`; if administrator bypass is required, use `CheckPasswordAsync` explicitly and document the intent.

---

#### Gotcha 7. `AllowedUserNameCharacters` Rejects, Not Strips — Invalid Characters Return an `IdentityResult` Failure

**Concepts**
- `AllowedUserNameCharacters` as a whitelist — characters outside it fail `CreateAsync`
- No automatic stripping or normalization of disallowed characters
- User-visible error message required — silent failure is not possible

**Answer**

`IdentityOptions.User.AllowedUserNameCharacters` is a whitelist: any character in the submitted username that is not in the allowed set causes `UserManager.CreateAsync` to return a failed `IdentityResult` with an error such as "Username 'user@example.com' is invalid, can only contain letters or digits." The validator does not trim or strip disallowed characters — the entire username is rejected. This surprises developers who want to allow email addresses as usernames but forget to add `@` and `.` to `AllowedUserNameCharacters`. The fix is to extend the whitelist: `options.User.AllowedUserNameCharacters += "@."`, or to store the email separately from the username and use a generated unique identifier as the username.

---

#### Gotcha 8. `IDataProtector.Unprotect()` Throws `CryptographicException` — It Never Returns `null`

**Concepts**
- `Unprotect` throwing on tampered, expired, or wrong-purpose data
- No null return path — every failure is an exception
- `try/catch CryptographicException` as mandatory pattern for any user-controlled input

**Answer**

`IDataProtector.Unprotect()` throws a `CryptographicException` when the payload has been tampered with, was protected under a different purpose string, was protected with a revoked or expired key, or is simply malformed. It does not return `null` or a sentinel value — any input that cannot be successfully unprotected results in an exception. Developers who pass user-controlled data (URL parameters, cookie values, request body fields) to `Unprotect` without wrapping the call in a `try/catch CryptographicException` block will expose unhandled exception behavior, which can reveal stack traces or cause cascading failures. The mandatory pattern for any Unprotect call that processes external input is `try { var value = protector.Unprotect(input); } catch (CryptographicException) { /* treat as invalid */ }`.

---

#### Gotcha 9. A Sub-Protector Cannot Decrypt Data Protected by Its Parent Protector

**Concepts**
- `CreateProtector("Purpose").CreateProtector("SubPurpose")` producing a distinct key derivation path
- Parent purpose + child purpose forming the full cryptographic domain
- Purpose hierarchy mixing causing permanent unprotect failures

**Answer**

`IDataProtector` instances created with `CreateProtector("A").CreateProtector("B")` are cryptographically distinct from `CreateProtector("A")` or `CreateProtector("B")` — the full purpose chain forms the key derivation input. Data protected by `CreateProtector("Session")` cannot be unprotected by `CreateProtector("Session").CreateProtector("Cookie")`, even though the child was derived from the parent. This distinction matters when refactoring code that introduces sub-purposes: any data already in transit (cookies, tokens, email links) that was protected under the old single-level purpose will fail to unprotect under the new hierarchical purpose, invalidating all existing sessions and tokens on deployment. Purpose changes must be coordinated with a key-rotation and migration plan, or rolled out with a transitional period that accepts both old and new purposes.

---

#### Gotcha 10. Password Validators Run on Both `CreateAsync` and `ChangePasswordAsync` — Custom Rules Must Handle Both Paths

**Concepts**
- `IPasswordValidator<TUser>` called by `UserManager` on creation and password change
- `ResetPasswordAsync` also running validators — not just `ChangePasswordAsync`
- Custom validator receiving the `TUser` — can implement user-specific rules

**Answer**

Custom `IPasswordValidator<TUser>` implementations are invoked by `UserManager` whenever a password is set — this includes `CreateAsync`, `ChangePasswordAsync`, `ResetPasswordAsync`, and `AddPasswordAsync`. A validator that checks the password against the user's display name or email (to prevent users from using their name as their password) must handle cases where the user object is partially populated: during `CreateAsync` the user may not yet have a normalized email if it was set after construction but before registration, while during `ResetPasswordAsync` the user is loaded from the database and is fully populated. Writing a validator that assumes a particular property is always present and not null will cause a `NullReferenceException` on one of these paths. Always guard against null user properties and test the validator across all password-setting code paths.

---
