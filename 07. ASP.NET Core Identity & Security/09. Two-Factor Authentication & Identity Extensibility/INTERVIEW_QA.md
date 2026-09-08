# Two-Factor Authentication & Identity Extensibility — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is two-factor authentication (2FA) and why is it important for web applications?](#q1-what-is-two-factor-authentication-2fa-and-why-is-it-important-for-web-applications)
2. [Q2. What is TOTP, and which RFC defines it? Explain the role of the 30-second time window and HMAC-SHA1.](#q2-what-is-totp-and-which-rfc-defines-it-explain-the-role-of-the-30-second-time-window-and-hmac-sha1)
3. [Q3. How does ASP.NET Core Identity enable 2FA for a user? Which method sets the `TwoFactorEnabled` flag?](#q3-how-does-aspnet-core-identity-enable-2fa-for-a-user-which-method-sets-the-twofactorenabled-flag)
4. [Q4. What is `GenerateTwoFactorTokenAsync` and when is it called during the sign-in flow?](#q4-what-is-generatetwofactortokenasync-and-when-is-it-called-during-the-sign-in-flow)
5. [Q5. Describe the authenticator app setup flow in ASP.NET Core Identity: from generating a secret key to verifying the first code.](#q5-describe-the-authenticator-app-setup-flow-in-aspnet-core-identity-from-generating-a-secret-key-to-verifying-the-first-code)
6. [Q6. What is the format of the TOTP URI used to populate a QR code, and what does each component represent?](#q6-what-is-the-format-of-the-totp-uri-used-to-populate-a-qr-code-and-what-does-each-component-represent)
7. [Q7. What does `GenerateNewAuthenticatorKey` do, and how is the raw key made human-readable for users?](#q7-what-does-generatenewauthenticatorkey-do-and-how-is-the-raw-key-made-human-readable-for-users)
8. [Q8. How does `SignInManager.TwoFactorAuthenticatorSignInAsync` work? What does it return and when does it succeed?](#q8-how-does-signinmanagertwofactorauthenticatorsigninasync-work-what-does-it-return-and-when-does-it-succeed)
9. [Q9. What are recovery codes in 2FA? How does ASP.NET Core Identity store and validate them?](#q9-what-are-recovery-codes-in-2fa-how-does-aspnet-core-identity-store-and-validate-them)
10. [Q10. How does `GenerateNewTwoFactorRecoveryCodesAsync` work, and why should recovery codes be regenerated after use?](#q10-how-does-generatenewtwofactorrecoverycodesasync-work-and-why-should-recovery-codes-be-regenerated-after-use)
11. [Q11. How is SMS-based 2FA implemented in ASP.NET Core Identity? Why is there no built-in SMS sender?](#q11-how-is-sms-based-2fa-implemented-in-aspnet-core-identity-why-is-there-no-built-in-sms-sender)
12. [Q12. What is the "remember this machine" feature in 2FA, and how is it implemented using a cookie?](#q12-what-is-the-remember-this-machine-feature-in-2fa-and-how-is-it-implemented-using-a-cookie)
13. [Q13. How do you extend `IdentityUser` with custom properties such as a `Tenant` or `ProfilePicture` field?](#q13-how-do-you-extend-identityuser-with-custom-properties-such-as-a-tenant-or-profilepicture-field)
14. [Q14. What changes are required in `ApplicationDbContext` and migrations when you extend `IdentityUser`?](#q14-what-changes-are-required-in-applicationdbcontext-and-migrations-when-you-extend-identityuser)
15. [Q15. How do you implement a custom `IUserValidator<TUser>` in ASP.NET Core Identity, and when does it run?](#q15-how-do-you-implement-a-custom-iuservalidatortuser-in-aspnet-core-identity-and-when-does-it-run)
16. [Q16. How do you implement a custom `PasswordValidator<TUser>`, and how do you register it alongside the default validator?](#q16-how-do-you-implement-a-custom-passwordvalidatortuser-and-how-do-you-register-it-alongside-the-default-validator)
17. [Q17. What is `IUserStore<TUser>` and when would you implement a custom one instead of using `UserStore` from EF Core?](#q17-what-is-iuserstoretuser-and-when-would-you-implement-a-custom-one-instead-of-using-userstore-from-ef-core)
18. [Q18. What methods must a minimal `IUserStore<TUser>` implementation provide?](#q18-what-methods-must-a-minimal-iuserstoretuser-implementation-provide)
19. [Q19. What is `IRoleStore<TRole>` and how does it relate to `IUserStore`?](#q19-what-is-irolestoretrole-and-how-does-it-relate-to-iuserstore)
20. [Q20. What is an Identity token provider (`IUserTwoFactorTokenProvider<TUser>`)? What are its two key methods?](#q20-what-is-an-identity-token-provider-iusertwofactortokenprovidertuser-what-are-its-two-key-methods)
21. [Q21. How do you register a custom token provider with ASP.NET Core Identity using `RegisterTokenProvider`?](#q21-how-do-you-register-a-custom-token-provider-with-aspnet-core-identity-using-registertokenprovider)
22. [Q22. What is the security stamp in ASP.NET Core Identity? What is it, and what is its purpose?](#q22-what-is-the-security-stamp-in-aspnet-core-identity-what-is-it-and-what-is-its-purpose)
23. [Q23. What is `ISecurityStampValidator` and how does it interact with the cookie authentication middleware?](#q23-what-is-isecuritystampvalidator-and-how-does-it-interact-with-the-cookie-authentication-middleware)
24. [Q24. What triggers a security stamp refresh, and what happens to existing sessions when the stamp changes?](#q24-what-triggers-a-security-stamp-refresh-and-what-happens-to-existing-sessions-when-the-stamp-changes)
25. [Q25. How does the two-factor sign-in flow differ from a standard password sign-in in terms of `SignInResult` states?](#q25-how-does-the-two-factor-sign-in-flow-differ-from-a-standard-password-sign-in-in-terms-of-signinresult-states)
26. [Q26. What is `TwoFactorRecoveryCodeSignInAsync` and when should it be used instead of `TwoFactorAuthenticatorSignInAsync`?](#q26-what-is-twofactorrecoverycodesigninasync-and-when-should-it-be-used-instead-of-twofactorauthenticatorsigninasync)
27. [Q27. How does ASP.NET Core Identity protect against brute-force attacks during the 2FA code entry step?](#q27-how-does-aspnet-core-identity-protect-against-brute-force-attacks-during-the-2fa-code-entry-step)
28. [Q28. What are the key differences between data protection tokens (used for email confirmation and password reset) and TOTP tokens used for 2FA?](#q28-what-are-the-key-differences-between-data-protection-tokens-used-for-email-confirmation-and-password-reset-and-totp-tokens-used-for-2fa)

---

## Q1. What is two-factor authentication (2FA) and why is it important for web applications?

**Concepts**
- Two independent authentication factors — something known and something possessed
- Credential stuffing and phishing mitigation
- ASP.NET Core Identity built-in 2FA support
- Regulatory compliance (PCI-DSS, SOC 2)

**Answer**

Two-factor authentication requires a user to prove their identity using two independent factors: something they know (a password) and something they have (a one-time code from a device). This means a stolen password alone is not enough to access an account, significantly reducing the risk from phishing, credential stuffing, and data breaches where passwords are leaked and reused across sites. The "something you have" factor is typically a time-limited code generated by an authenticator app or delivered via SMS, which an attacker cannot reuse or easily intercept. ASP.NET Core Identity has built-in support for 2FA, enabling developers to add this protection without implementing the cryptographic mechanics from scratch. Regulatory standards such as PCI-DSS and SOC 2 either require or strongly recommend 2FA for privileged access, making it a compliance concern as well as a security one.

---

## Q2. What is TOTP, and which RFC defines it? Explain the role of the 30-second time window and HMAC-SHA1.

**Concepts**
- RFC 6238 TOTP built on RFC 4226 HOTP
- HMAC-SHA1 keyed hash over the time counter
- 30-second time step — server and app compute independently
- Dynamic truncation to a 6-digit code
- Replay prevention via last-used counter tracking

**Answer**

TOTP (Time-based One-Time Password), defined in RFC 6238, generates a short-lived numeric code by combining a shared secret key with the current Unix timestamp divided into fixed 30-second steps. The result is computed using HMAC-SHA1, where the timestamp is expressed as the number of 30-second intervals since the Unix epoch — this counter is called T. The 30-second window means both the server and the authenticator app independently compute the same code from the same secret and the same time step, with no network communication at code-generation time. Because server and device clocks are not perfectly synchronized, implementations typically accept codes from the previous and next time step as well, giving a practical acceptance window of up to 90 seconds. The 6-digit code is extracted from the HMAC output using the dynamic truncation algorithm specified in RFC 4226 (HOTP), which TOTP builds upon. Reuse within the same time step is blocked by the server tracking the last used counter value, preventing replay attacks.

---

## Q3. How does ASP.NET Core Identity enable 2FA for a user? Which method sets the `TwoFactorEnabled` flag?

**Concepts**
- UserManager.SetTwoFactorEnabledAsync to flip the flag
- Flag only meaningful after an authenticator secret is configured
- SignInManager returning TwoFactorRequired when flag is true
- ResetAuthenticatorKeyAsync on disable to invalidate old codes

**Answer**

ASP.NET Core Identity stores a `TwoFactorEnabled` boolean on the `IdentityUser` record that controls whether the 2FA challenge is presented at sign-in. The flag is set to `true` by calling `UserManager.SetTwoFactorEnabledAsync(user, true)`, typically after the user has verified their first authenticator code during setup. The flag is only meaningful when an authenticator secret or other second-factor credential has already been associated with the account — enabling the flag without a configured second factor would lock the user out. The standard scaffold pattern verifies the user's first TOTP code with `VerifyTwoFactorTokenAsync` and then calls `SetTwoFactorEnabledAsync` in the same request handler so the two steps are atomic from the user's perspective. `SignInManager` checks this flag at sign-in: when `TwoFactorEnabled` is `true`, a successful password check returns `SignInResult.TwoFactorRequired` instead of `SignInResult.Succeeded`, redirecting the flow to the 2FA challenge page. Disabling 2FA calls the same method with `false`, and the authenticator key is typically reset with `ResetAuthenticatorKeyAsync` to prevent old codes from being usable if the user re-enables 2FA later.

---

## Q4. What is `GenerateTwoFactorTokenAsync` and when is it called during the sign-in flow?

**Concepts**
- Server-side TOTP code computation for comparison, not delivery
- SMS 2FA using this method to produce the code to send
- VerifyTwoFactorTokenAsync calling it internally for TOTP
- Token provider name routing

**Answer**

`GenerateTwoFactorTokenAsync(user, tokenProvider)` is a `UserManager` method that asks a registered token provider to produce a one-time code for the given user. For the built-in `AuthenticatorTokenProvider`, the method computes the same HMAC-SHA1-derived code that the authenticator app would display at that moment — it is used during verification rather than to produce a code to send somewhere. For SMS-based 2FA, `GenerateTwoFactorTokenAsync` is called to produce a short code that the application then sends to the user's phone via a custom SMS service; the server stores no plaintext code, only the ability to regenerate and compare. For TOTP authenticator apps, the server does not need to call this method to send anything, since the code is generated offline by the app — the server calls `VerifyTwoFactorTokenAsync` instead, which internally calls `GenerateTwoFactorTokenAsync` to recompute the expected code for comparison. The token provider name (e.g., `"Authenticator"`, `"Phone"`, `"Email"`) must match a provider registered in `AddIdentity` or `AddTokenProvider`; passing an unregistered name throws at runtime.

---

## Q5. Describe the authenticator app setup flow in ASP.NET Core Identity: from generating a secret key to verifying the first code.

**Concepts**
- GenerateNewAuthenticatorKey + SetAuthenticatorKeyAsync before showing QR code
- TOTP URI construction and QR code rendering
- VerifyTwoFactorTokenAsync before SetTwoFactorEnabledAsync
- Recovery codes generated immediately after successful setup

**Answer**

The setup flow begins when a user navigates to the 2FA configuration page: the server generates a new secret key using `UserManager.GenerateNewAuthenticatorKey()`, stores it against the user with `SetAuthenticatorKeyAsync`, and constructs a TOTP URI that is rendered as a QR code for the user to scan. The user scans the QR code in their authenticator app, which stores the secret, then enters the first 6-digit code shown by the app. The server verifies it with `VerifyTwoFactorTokenAsync`; if valid, 2FA is activated by calling `SetTwoFactorEnabledAsync(user, true)`. The secret key must be persisted against the user before the QR code is shown, because the TOTP URI embeds it and the authenticator app needs it to generate codes — without persistence, each page reload would generate a different key and break the app's codes. The verification step before enabling 2FA is critical: it confirms that the user successfully scanned the key and that their device clock is sufficiently synchronized with the server. Recovery codes are typically generated and shown to the user immediately after this successful setup so they have a backup before leaving the page.

---

## Q6. What is the format of the TOTP URI used to populate a QR code, and what does each component represent?

**Concepts**
- `otpauth://totp/` scheme
- Label format: `issuer:accountName`
- Base32-encoded secret (not Base64)
- Issuer query parameter for app grouping display

**Answer**

The TOTP URI follows the `otpauth://` scheme defined by Google Authenticator and is the standard format that authenticator apps parse when scanning a QR code. Its full form is `otpauth://totp/{issuer}:{accountName}?secret={base32secret}&issuer={issuer}&digits=6&algorithm=SHA1&period=30`.

| Component | Meaning |
|---|---|
| `otpauth://totp/` | Scheme and type; `totp` distinguishes time-based from counter-based (hotp) |
| `{issuer}:{accountName}` | Label shown in the authenticator app; typically the app name and user's email |
| `secret` | The shared secret, Base32-encoded (not Base64); this is what the app stores |
| `issuer` | Repeated as a query parameter to associate the entry with the correct app |
| `digits` | Number of digits in the code (almost always 6) |
| `algorithm` | Hash algorithm (SHA1 per RFC 6238 default) |
| `period` | Time step in seconds (30 is the standard) |

ASP.NET Core Identity's scaffold builds this URI manually using `Uri.EscapeDataString` on the label parts and formats the Base32-encoded key from `GetAuthenticatorKeyAsync`; there is no built-in URI builder helper. The `secret` must be Base32-encoded (RFC 4648) because that is what the `otpauth` spec requires; the raw key returned by ASP.NET Core Identity is already in the correct format after stripping spaces.

---

## Q7. What does `GenerateNewAuthenticatorKey` do, and how is the raw key made human-readable for users?

**Concepts**
- Cryptographically random Base32 key from Data Protection infrastructure
- Grouped display (four-character groups) for manual entry
- Base32 alphabet avoiding visually ambiguous characters
- AspNetUserTokens storage via SetAuthenticatorKeyAsync

**Answer**

`UserManager.GenerateNewAuthenticatorKey()` generates a cryptographically random byte sequence using the Data Protection infrastructure and returns it as a Base32-encoded string, which serves as the TOTP shared secret for that user. The method does not persist the key — the caller must call `SetAuthenticatorKeyAsync(user, key)` separately to save it. The raw Base32 string is rendered to users in groups of four characters separated by spaces (e.g., `JBSA WQYL …`) to make manual entry easier for users who cannot scan a QR code. Base32 uses a 32-character alphabet (A-Z and 2-7) chosen because it avoids characters that are visually ambiguous — 0 vs O, 1 vs I, 8 vs B — reducing manual-entry errors. The generated key is stored in the `AspNetUserTokens` table via the token provider mechanism, under the name `AuthenticatorKey`, and `GetAuthenticatorKeyAsync` retrieves it. If a user requests a new key, calling `ResetAuthenticatorKeyAsync` deletes the old token row and forces a new `GenerateNewAuthenticatorKey` call on the next setup page load, ensuring the old key is never reused.

---

## Q8. How does `SignInManager.TwoFactorAuthenticatorSignInAsync` work? What does it return and when does it succeed?

**Concepts**
- In-progress sign-in retrieved from Identity.TwoFactorUserId temporary cookie
- SignInResult states: Succeeded, LockedOut, Failed
- rememberClient parameter writing the remember-machine cookie
- isPersistent parameter controlling main application cookie lifetime

**Answer**

`TwoFactorAuthenticatorSignInAsync(code, isPersistent, rememberClient)` is called on the 2FA challenge page after the user enters their TOTP code. It retrieves the in-progress sign-in state from a temporary cookie set during the password-check phase, locates the user, verifies the code with `VerifyTwoFactorTokenAsync` using the built-in `AuthenticatorTokenProvider`, and if valid, completes the sign-in by issuing the application cookie. The method returns `SignInResult.Succeeded` when the code is correct and the account is not locked out, `SignInResult.LockedOut` when too many failed attempts have triggered lockout, and `SignInResult.Failed` when the code is wrong. The `rememberClient` parameter controls whether a "remember this machine" cookie is written after a successful 2FA check, which suppresses the 2FA challenge for future sign-ins from the same browser. The `isPersistent` parameter controls the lifetime of the main application cookie — session vs. long-lived — the same as in `PasswordSignInAsync`. The temporary cookie bridging the two sign-in steps is named `Identity.TwoFactorUserId` by default; if the user closes the browser between the password step and the 2FA step, this cookie is lost and they must restart sign-in.

---

## Q9. What are recovery codes in 2FA? How does ASP.NET Core Identity store and validate them?

**Concepts**
- Single-use fallback codes for lost authenticator device
- Hashed storage in AspNetUserTokens — not plaintext
- Atomic removal on successful use
- CountRecoveryCodesAsync for depletion warnings

**Answer**

Recovery codes are single-use fallback codes given to users when they set up 2FA, intended for situations where they have lost access to their authenticator device. ASP.NET Core Identity generates them as random alphanumeric strings, stores them as hashed values in the `AspNetUserTokens` table, and removes each one from storage the moment it is successfully used. Storing hashes rather than plaintext means that even if the token table is compromised, the recovery codes themselves cannot be directly extracted and used. The default implementation generates codes in the format `XXXXX-XXXXX` (two groups of five alphanumeric characters), though the format is not part of the API contract. Validation in `TwoFactorRecoveryCodeSignInAsync` hashes the submitted code, searches the stored set for a match, and atomically removes the matched entry to enforce single-use semantics. `CountRecoveryCodesAsync` lets the application warn users when they are running low — typically when fewer than three remain — prompting them to generate a new set. Recovery codes are not time-limited, unlike TOTP codes; their security depends entirely on being kept secret by the user and consumed on first use.

---

## Q10. How does `GenerateNewTwoFactorRecoveryCodesAsync` work, and why should recovery codes be regenerated after use?

**Concepts**
- Plain-text codes returned only at generation time
- Previous stored set replaced entirely
- Display-once requirement — hashes only are stored
- Number parameter sizing trade-offs

**Answer**

`UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, number)` generates a specified number of new recovery codes, hashes each one, stores all hashes in `AspNetUserTokens` replacing any previously stored set, and returns the plain-text codes so the application can display them to the user exactly once. Because the hashes replace the previous set, calling this method invalidates all previously issued recovery codes. The plain-text codes are returned only at generation time — since only the hashes are stored, the application must display them in that response and there is no way to retrieve them again. Regenerating after use is important because a partially consumed set has fewer remaining fallbacks, and if the old codes were ever exposed in a screenshot or printout, regeneration revokes them all. The number parameter is typically 8 or 10; Microsoft's default scaffold uses 8. After generating new codes, the application should prompt the user to save or print them immediately, since there is no "resend" option once the response is sent.

---

## Q11. How is SMS-based 2FA implemented in ASP.NET Core Identity? Why is there no built-in SMS sender?

**Concepts**
- Custom ISmsSender for third-party provider integration
- PhoneNumberTokenProvider registration as "Phone"
- SIM-swapping vulnerability of SMS 2FA
- Adoption vs security trade-off

**Answer**

SMS-based 2FA uses the standard `GenerateTwoFactorTokenAsync` method to produce a short code and then delegates actual SMS delivery to a custom service that the developer registers — typically as an implementation of an `ISmsSender` interface that the developer defines. There is no built-in SMS sender because SMS delivery requires integration with a third-party provider (Twilio, Azure Communication Services, Amazon SNS, etc.), and bundling a specific provider would force an opinionated choice onto every user of the framework. The application calls `GenerateTwoFactorTokenAsync(user, "Phone")` to create the code using the phone token provider registered for that purpose, then passes the resulting code and the user's phone number to the custom `ISmsSender` service. The `"Phone"` token provider must be registered during Identity setup: `.AddIdentity(...).AddTokenProvider<PhoneNumberTokenProvider<ApplicationUser>>("Phone")`. SMS 2FA is generally considered less secure than TOTP authenticator apps because SMS messages can be intercepted via SIM-swapping attacks, where an attacker convinces a carrier to transfer the victim's number to an attacker-controlled SIM. Despite the security concerns, SMS 2FA is still widely used because it requires no app installation and works on any mobile phone, improving adoption rates.

---

## Q12. What is the "remember this machine" feature in 2FA, and how is it implemented using a cookie?

**Concepts**
- RememberTwoFactorClientAsync writing the remember-machine cookie
- Identity.TwoFactorRememberMe cookie name
- Security stamp embedded in cookie — invalidated on password change
- TwoFactorRememberMeCookieBuilder expiry configuration

**Answer**

The "remember this machine" feature allows a user to skip the 2FA challenge on future sign-ins from the same browser after they have successfully completed 2FA once. It is implemented by writing a separate cookie that contains a user-specific token; on subsequent sign-ins, `SignInManager` checks for this cookie before deciding whether to issue `SignInResult.TwoFactorRequired`. The cookie is written by `SignInManager.RememberTwoFactorClientAsync(user)` and read by `IsTwoFactorClientRememberedAsync(user)`, both called internally when the `rememberClient` parameter is `true` in `TwoFactorAuthenticatorSignInAsync`. The cookie name defaults to `Identity.TwoFactorRememberMe` and is tied to the browser's cookie storage, so clearing cookies or switching browsers requires completing 2FA again. The cookie content is a data-protection-encrypted token that includes the user's ID and security stamp — changing the security stamp (e.g., on password change) invalidates the remember-machine cookie because the stamp embedded in the cookie no longer matches. Applications can control the cookie's expiry through `TwoFactorRememberMeCookieBuilder` options in `IdentityOptions`; setting a short expiry reduces the window during which a stolen browser session bypasses 2FA.

---

## Q13. How do you extend `IdentityUser` with custom properties such as a `Tenant` or `ProfilePicture` field?

**Concepts**
- Inheritance from IdentityUser adding custom properties
- IdentityDbContext<ApplicationUser> generic argument
- EF Core migration for new columns
- Strongly typed UserManager<ApplicationUser> from DI

**Answer**

You extend `IdentityUser` by creating a class that inherits from it and adding the desired properties, then substituting that class everywhere `IdentityUser` was referenced — in `AddIdentity`, `ApplicationDbContext`, and `UserManager` usage throughout the application. Entity Framework Core includes the new properties as columns in the `AspNetUsers` table.

```csharp
public class ApplicationUser : IdentityUser
{
    public string? Tenant { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
```

The `ApplicationDbContext` must derive from `IdentityDbContext<ApplicationUser>` (not the default `IdentityDbContext<IdentityUser>`) so the EF Core model builder is aware of the derived type and its additional columns. After adding properties, a new migration (`dotnet ef migrations add AddUserTenantField`) is required to update the database schema. `UserManager<ApplicationUser>` is automatically registered by the DI system when you call `AddIdentity<ApplicationUser, IdentityRole>()`, giving you strongly typed access to the custom properties through the manager. Sensitive data placed in custom properties is not encrypted at rest by default — only the password hash uses a one-way hash — so additional protection should be considered for sensitive custom fields.

---

## Q14. What changes are required in `ApplicationDbContext` and migrations when you extend `IdentityUser`?

**Concepts**
- IdentityDbContext<ApplicationUser> generic argument change
- Table-per-hierarchy (TPH) — all properties in AspNetUsers table
- [MaxLength] avoiding nvarchar(max) for indexable columns
- Migration applied across all environments

**Answer**

The `ApplicationDbContext` must inherit from `IdentityDbContext<ApplicationUser>`, replacing the default generic argument, so that Entity Framework Core maps the derived user class and its extra columns to the `AspNetUsers` table. EF Core uses table-per-hierarchy (TPH) by default for `IdentityUser` inheritance, meaning all properties land in the same `AspNetUsers` table — no separate table is created for the subclass. After modifying the class, running `dotnet ef migrations add <MigrationName>` generates a migration file containing `AddColumn` statements for each new property; running `dotnet ef database update` applies it. Annotating nullable string properties with `[MaxLength]` avoids EF Core creating an `nvarchar(max)` column, which is important because `nvarchar(max)` columns cannot be included in indexes, degrading query performance. When multiple environments share the same database, the migration must be applied to each environment — the Identity table schema must stay in sync across all deployments.

---

## Q15. How do you implement a custom `IUserValidator<TUser>` in ASP.NET Core Identity, and when does it run?

**Concepts**
- IUserValidator<TUser>.ValidateAsync returning IdentityResult
- Runs during CreateAsync and UpdateAsync, after built-in validators
- DI registration collecting all IUserValidator<TUser> implementations
- Async validation allowing external API calls

**Answer**

A custom user validator implements `IUserValidator<TUser>`, which has a single method `ValidateAsync(UserManager<TUser> manager, TUser user)` that returns an `IdentityResult`. The result either succeeds or carries a list of `IdentityError` objects. It is called automatically by `UserManager` during `CreateAsync` and `UpdateAsync` operations, after the built-in validators have run.

```csharp
public class NoDisposableEmailValidator : IUserValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(
        UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        var blocked = new[] { "mailinator.com", "guerrillamail.com" };
        if (blocked.Any(d => user.Email!.EndsWith("@" + d)))
            return Task.FromResult(IdentityResult.Failed(
                new IdentityError { Description = "Disposable emails not allowed." }));
        return Task.FromResult(IdentityResult.Success);
    }
}
```

Register the validator by calling `builder.Services.AddScoped<IUserValidator<ApplicationUser>, NoDisposableEmailValidator>()` after `AddIdentity`; ASP.NET Core Identity collects all registered `IUserValidator<TUser>` implementations and runs them all. Custom validators run in addition to (not instead of) the default `UserValidator<TUser>`, which checks for duplicate usernames and emails. The validator can be async, allowing calls to a database or external service — for example, checking a blocklist API — as part of the validation logic.

---

## Q16. How do you implement a custom `PasswordValidator<TUser>`, and how do you register it alongside the default validator?

**Concepts**
- IPasswordValidator<TUser> implementation with ValidateAsync
- Chained alongside default PasswordValidator — not replacing it
- All validators must succeed for the operation to proceed
- Have I Been Pwned range endpoint for breached password checks

**Answer**

A custom password validator inherits from `PasswordValidator<TUser>` or directly implements `IPasswordValidator<TUser>`, overriding `ValidateAsync(UserManager<TUser> manager, TUser user, string password)`. Registration follows the same pattern as user validators: add it as a scoped service implementing `IPasswordValidator<ApplicationUser>`, and Identity will chain it with any others.

```csharp
public class NoUserNameInPasswordValidator : IPasswordValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(
        UserManager<ApplicationUser> manager, ApplicationUser user, string password)
    {
        if (password.Contains(user.UserName!, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(IdentityResult.Failed(
                new IdentityError { Description = "Password must not contain your username." }));
        return Task.FromResult(IdentityResult.Success);
    }
}
```

All registered `IPasswordValidator<TUser>` implementations are run every time `UserManager.CreateAsync`, `ChangePasswordAsync`, or `ResetPasswordAsync` is called; all must return `IdentityResult.Success` for the operation to proceed. The default `PasswordValidator<TUser>` enforces the `PasswordOptions` settings configured in `IdentityOptions`; adding a custom validator does not remove the default unless it is explicitly removed from `UserManager.PasswordValidators`. For complex rules such as breached password checks, the validator can be async and call an external API like Have I Been Pwned's range endpoint. Password validation errors are surfaced through `ModelState` in scaffolded pages and presented to the user via the standard validation summary.

---

## Q17. What is `IUserStore<TUser>` and when would you implement a custom one instead of using `UserStore` from EF Core?

**Concepts**
- Core persistence interface that UserManager delegates to
- Custom store for non-EF backends (MongoDB, Dapper, Azure Table Storage)
- Additional optional store interfaces detected via is-checks at runtime
- Custom data access patterns (read replica, primary writes)

**Answer**

`IUserStore<TUser>` is the core persistence interface that all of ASP.NET Core Identity's `UserManager` operations delegate to for reading and writing user data. The default `UserStore<TUser>` implementation uses Entity Framework Core; you implement a custom `IUserStore<TUser>` when your application stores user data in a backend that EF Core does not support — such as MongoDB, Dapper with a legacy SQL schema, Azure Table Storage, or an external identity system. Custom stores also make sense when you need to enforce a specific data access pattern — for example, all reads going through a read replica while all writes go to a primary — that cannot be expressed cleanly through the default `DbContext`. Additional store interfaces — `IUserPasswordStore<TUser>`, `IUserEmailStore<TUser>`, `IUserPhoneNumberStore<TUser>`, `IUserTwoFactorStore<TUser>` — are optional extensions that `UserManager` detects via `is` checks at runtime; a store that does not implement `IUserTwoFactorStore` cannot support 2FA. The store is registered with `builder.Services.AddScoped<IUserStore<ApplicationUser>, MyCustomUserStore>()` and is picked up automatically when `AddIdentity` is called.

---

## Q18. What methods must a minimal `IUserStore<TUser>` implementation provide?

**Concepts**
- 7 required CRUD and identity-lookup methods
- Normalized username for case-insensitive lookup without database collation dependency
- IDisposable as part of the interface contract
- Additional interfaces for every feature beyond basic CRUD

**Answer**

`IUserStore<TUser>` defines the minimum contract: `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `FindByIdAsync`, `FindByNameAsync`, `GetUserIdAsync`, `GetUserNameAsync`, `SetUserNameAsync`, `GetNormalizedUserNameAsync`, `SetNormalizedUserNameAsync`, and `Dispose`. These represent the fundamental create/read/update/delete and identity-lookup operations that `UserManager` requires to function.

| Method | Purpose |
|---|---|
| `CreateAsync` | Persists a new user record |
| `UpdateAsync` | Saves changes to an existing user |
| `DeleteAsync` | Removes a user from the store |
| `FindByIdAsync` | Looks up a user by their unique ID |
| `FindByNameAsync` | Looks up a user by normalized username |
| `GetUserIdAsync` / `GetUserNameAsync` | Reads the ID/username from a user object |
| `SetNormalizedUserNameAsync` | Stores the normalized (uppercase) username for case-insensitive lookup |

The normalized username is stored in all-uppercase so lookups are case-insensitive without relying on database collation settings. `IDisposable` is part of the interface; in practice the store is usually scoped and the DI container handles disposal, so `Dispose` can be a no-op. Any feature beyond basic user CRUD — passwords, emails, claims, roles, tokens, lockout, 2FA — requires implementing the corresponding additional interfaces.

---

## Q19. What is `IRoleStore<TRole>` and how does it relate to `IUserStore`?

**Concepts**
- IRoleStore as the role-entity persistence interface
- IUserRoleStore as the bridge on the user side
- IRoleClaimStore for roles that carry claims
- Optional role infrastructure — omit if using policy-only auth

**Answer**

`IRoleStore<TRole>` is the persistence interface for role entities, analogous to `IUserStore<TUser>` for users. It defines `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `FindByIdAsync`, `FindByNameAsync`, and normalized name accessors for roles. It is separate from `IUserStore` because roles and users have independent lifecycles and may even live in different data stores. When using EF Core, the default `RoleStore<TRole>` implements both `IRoleStore<TRole>` and `IQueryableRoleStore<TRole>`; for custom backends, you implement `IRoleStore<TRole>` and optionally `IRoleClaimStore<TRole>` if roles carry claims. `IUserRoleStore<TUser>` is the bridge interface on the user side that stores and retrieves which roles a user belongs to; it is separate from `IRoleStore` and is implemented as part of the user store. If the application uses only policy-based authorization with claims and never uses role-based `[Authorize(Roles = "Admin")]`, a custom `IRoleStore` may be entirely omitted by not calling `AddRoles()` during Identity setup. Role names are normalized for the same case-insensitive lookup reason as usernames; `SetNormalizedRoleNameAsync` must always be called when a role is created or renamed.

---

## Q20. What is an Identity token provider (`IUserTwoFactorTokenProvider<TUser>`)? What are its two key methods?

**Concepts**
- GenerateAsync for producing the token
- ValidateAsync for verifying the submitted token
- Purpose parameter namespacing — prevents cross-purpose token misuse
- CanGenerateTwoFactorTokenAsync for prerequisite checking

**Answer**

`IUserTwoFactorTokenProvider<TUser>` is the interface that encapsulates the logic for generating and verifying purpose-specific tokens, including 2FA codes, email confirmation links, and password reset tokens. The two key methods are `GenerateAsync`, which produces a token for a given purpose and user, and `ValidateAsync`, which checks whether a submitted token is correct and still valid. `GenerateAsync(purpose, manager, user)` is called when the application needs to create a token to send to the user — for example, an SMS 2FA code or an email verification link; for TOTP, this method computes the time-based code from the stored secret. `ValidateAsync(purpose, token, manager, user)` is called when the user submits a token back to the application; it regenerates the expected token and compares it, accepting adjacent time steps for TOTP. The `purpose` parameter (e.g., `"TwoFactor"`, `"EmailConfirmation"`, `"ResetPassword"`) namespaces the token so that a token generated for email confirmation cannot be submitted as a password reset token, preventing cross-purpose misuse. `CanGenerateTwoFactorTokenAsync` is an optional third method that `UserManager` calls to determine whether the provider has the necessary data to generate a 2FA code — for example, whether the user has a phone number for the phone token provider.

---

## Q21. How do you register a custom token provider with ASP.NET Core Identity using `RegisterTokenProvider`?

**Concepts**
- AddTokenProvider<TProvider>(name) on the IdentityBuilder
- Name as the routing key for Generate and Verify calls
- Multiple providers coexisting — routing by name
- Built-in providers registered via AddDefaultTokenProviders

**Answer**

A custom token provider is registered by chaining `AddTokenProvider<TProvider>(name)` onto the `IdentityBuilder` returned by `AddIdentity` or `AddDefaultIdentity`. The `name` string is the key used throughout the system to request tokens from that provider.

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddTokenProvider<MyCustomEmailTokenProvider>("MyEmailProvider");
```

The registered name is what you pass to `GenerateTwoFactorTokenAsync(user, "MyEmailProvider")` and `VerifyTwoFactorTokenAsync(user, "MyEmailProvider", code)`; mismatching the name returns a failed result rather than throwing. Multiple providers can be registered simultaneously — for example, both `AuthenticatorTokenProvider` and a custom SMS provider — and `UserManager` routes to the correct one based on the name parameter. The provider class must implement `IUserTwoFactorTokenProvider<TUser>` and is resolved from the DI container, so it can take constructor dependencies such as a data protection provider or a configuration object. ASP.NET Core Identity registers the built-in providers (`Default`, `Email`, `Phone`, `Authenticator`) via `AddDefaultTokenProviders()`; calling `AddTokenProvider` on top adds yours in addition to those.

---

## Q22. What is the security stamp in ASP.NET Core Identity? What is it, and what is its purpose?

**Concepts**
- Security stamp as account security state GUID
- Embedded in the authentication cookie at sign-in
- ISecurityStampValidator comparing stamp on each request interval
- UpdateSecurityStampAsync for explicit log-out-everywhere

**Answer**

The security stamp is a GUID-like string stored on the `IdentityUser` record that represents the current security state of the account. It changes whenever a security-sensitive action occurs — password change, external login added or removed, 2FA status change, or explicit revocation — and serves as a server-side mechanism to invalidate all existing authentication cookies. The security stamp is embedded in the authentication cookie when the user signs in; on each request, `ISecurityStampValidator` periodically re-reads the stamp from the database and compares it to the value in the cookie. When the stamps differ, the validator rejects the cookie and signs the user out, even if the cookie has not yet expired — this is what forces all active sessions to end after a password change. Without the security stamp, an authentication cookie issued before a password change would remain valid until its expiry date, leaving a compromised account accessible even after the owner changes the password. `UserManager.UpdateSecurityStampAsync(user)` can be called explicitly to revoke all sessions without changing any other user data, which is the mechanism for "log out everywhere" functionality.

---

## Q23. What is `ISecurityStampValidator` and how does it interact with the cookie authentication middleware?

**Concepts**
- Configurable ValidationInterval — default 30 minutes
- Database read on every request when ValidationInterval is zero
- SecurityStampValidator<TUser> default implementation
- Deleted-user signout as a side effect

**Answer**

`ISecurityStampValidator` is a service that the cookie authentication middleware calls on a configurable interval to verify that the security stamp in the current authentication cookie still matches the one stored in the database. If the stamps do not match, the middleware signs the user out by rejecting the cookie and redirecting to the login page. The interval is controlled by `SecurityStampValidatorOptions.ValidationInterval`, which defaults to 30 minutes — every request within that window passes without a database hit, but once the interval elapses the validator performs a database read. Setting `ValidationInterval = TimeSpan.Zero` causes the stamp to be validated on every request, which is maximally secure but generates a database read per request — appropriate only for high-security applications. `SecurityStampValidator<TUser>` is the default implementation; it retrieves the user by ID from the cookie, reads the current stamp from `UserManager`, and compares it to the stamp claim embedded in the cookie principal. If the user no longer exists in the database (deleted account), the validator also signs out the cookie, preventing ghost sessions from continuing after an account is removed.

---

## Q24. What triggers a security stamp refresh, and what happens to existing sessions when the stamp changes?

**Concepts**
- ChangePasswordAsync, AddLoginAsync, SetTwoFactorEnabledAsync all update the stamp
- Validation interval delay before sessions detect the change
- ValidationInterval = TimeSpan.Zero for immediate revocation
- Remember-machine 2FA cookie also invalidated by stamp change

**Answer**

ASP.NET Core Identity automatically refreshes the security stamp whenever `UserManager` performs a security-sensitive mutation: `ChangePasswordAsync`, `RemovePasswordAsync`, `AddLoginAsync`, `RemoveLoginAsync`, `SetTwoFactorEnabledAsync`, and `AddClaimsAsync` all call `UpdateSecurityStampAsync` internally. When the stamp changes, any authentication cookie that embeds the old stamp will fail the next validation check and trigger an automatic sign-out. "Existing sessions" means every browser or device that has an active cookie for that user — the revocation is global because all sessions share the same stamp value in the database. The sign-out is not instantaneous: it happens at the next validation interval boundary on each session, so a session that validated one second ago will continue for up to `ValidationInterval` (default 30 minutes) before detecting the stamp change. For immediate revocation with no delay, set `ValidationInterval = TimeSpan.Zero` or issue a new cookie to the current session immediately after the stamp-changing operation. The "remember this machine" 2FA cookie is also invalidated when the security stamp changes, because its content is encrypted with the old stamp value embedded — this means changing a password also forces re-completion of 2FA on all remembered machines.

---

## Q25. How does the two-factor sign-in flow differ from a standard password sign-in in terms of `SignInResult` states?

**Concepts**
- TwoFactorRequired returned instead of Succeeded when 2FA is enabled
- Identity.TwoFactorUserId temporary cookie bridging the two steps
- No application session cookie until both factors verified
- Remember-machine cookie bypassing TwoFactorRequired

**Answer**

In a standard password sign-in, `PasswordSignInAsync` returns `SignInResult.Succeeded` on a correct password with no lockout. In a two-factor flow, a correct password for a 2FA-enabled account instead returns `SignInResult.TwoFactorRequired`, telling the application to redirect to the 2FA challenge page rather than completing the sign-in.

| `SignInResult` State | Meaning |
|---|---|
| `Succeeded` | Full sign-in complete (no 2FA required, or 2FA step complete) |
| `TwoFactorRequired` | Password correct, 2FA code still needed |
| `LockedOut` | Account locked due to too many failures |
| `NotAllowed` | Account exists but sign-in not permitted (e.g., unconfirmed email) |
| `Failed` | Credentials incorrect |

The bridge between the two steps is a temporary cookie (`Identity.TwoFactorUserId`) that holds an encrypted reference to the user's ID, allowing the 2FA challenge page to know which user is being authenticated without having issued a full session cookie. If the remember-machine cookie is present and valid for the user, `PasswordSignInAsync` skips `TwoFactorRequired` and returns `Succeeded` directly. `SignInResult` has no `Succeeded` value until both factors are verified — partial completion is represented by `TwoFactorRequired` and the temporary cookie; there is no half-authenticated application cookie.

---

## Q26. What is `TwoFactorRecoveryCodeSignInAsync` and when should it be used instead of `TwoFactorAuthenticatorSignInAsync`?

**Concepts**
- RedeemTwoFactorRecoveryCodeAsync — atomic hash lookup and deletion
- Lockout still applies during recovery code entry
- Re-setup prompt after recovery sign-in
- Same two-step flow prerequisite as TOTP sign-in

**Answer**

`TwoFactorRecoveryCodeSignInAsync(code)` completes a 2FA challenge using a recovery code instead of a TOTP code from an authenticator app. It should be used when the user has lost access to their authenticator device and needs the fallback path to regain account access. Internally, it calls `RedeemTwoFactorRecoveryCodeAsync(user, code)`, which hashes the submitted code, searches the stored hashes for a match, and if found, deletes that hash atomically to enforce single-use semantics. Like `TwoFactorAuthenticatorSignInAsync`, it checks for account lockout before processing the code — too many failed recovery code attempts can lock the account the same way repeated TOTP failures do. After a successful recovery code sign-in, the application should prompt the user to disable and re-enable 2FA with a new authenticator setup, since their remaining recovery codes have decreased and their device access is uncertain. Recovery code sign-in still goes through the same two-step flow: `PasswordSignInAsync` must return `TwoFactorRequired` first, establishing the temporary cookie, before `TwoFactorRecoveryCodeSignInAsync` can complete the session.

---

## Q27. How does ASP.NET Core Identity protect against brute-force attacks during the 2FA code entry step?

**Concepts**
- AccessFailedCount increment per failure — same lockout mechanism as password
- MaxFailedAccessAttempts threshold (default 5) and DefaultLockoutTimeSpan (default 15 min)
- Per-user DB-persisted lockout surviving server restarts
- ResetAccessFailedCountAsync for admin account unlock

**Answer**

ASP.NET Core Identity applies the same lockout mechanism during 2FA code entry as during password entry: each failed attempt to verify a 2FA code increments the `AccessFailedCount` on the user record, and once `MaxFailedAccessAttempts` is reached (default 5), the account is locked out for `DefaultLockoutTimeSpan` (default 15 minutes). Lockout at the 2FA step is important because a 6-digit TOTP code has only one million possible values and is valid for 90 seconds — without lockout, an attacker who passes the password step could brute-force the second factor. `TwoFactorAuthenticatorSignInAsync` and `TwoFactorRecoveryCodeSignInAsync` both set `lockoutOnFailure: true` by default in their internal calls, ensuring the lockout counter is incremented on failure. Lockout state is per-user and persisted to the database, so it survives server restarts and applies across all instances in a load-balanced deployment. The lockout duration resets after a successful sign-in; `UserManager.ResetAccessFailedCountAsync` can also be called explicitly by administrators to unlock an account before the duration expires.

---

## Q28. What are the key differences between data protection tokens (used for email confirmation and password reset) and TOTP tokens used for 2FA?

**Concepts**
- Server-generated (data protection) vs client-computed (TOTP) tokens
- Configurable vs fixed 30-second validity
- Single-use semantics for both, enforced differently
- Security stamp invalidation of data protection tokens — not applicable to TOTP

**Answer**

Data protection tokens are server-generated, single-use, time-limited strings created by the server and sent to the user via email or SMS; they are validated once and then discarded. TOTP tokens are client-generated, short-lived codes that both the authenticator app and the server independently compute from a shared secret; no network transmission of the code is needed at generation time.

| Dimension | Data Protection Tokens | TOTP Tokens |
|---|---|---|
| Generated by | Server (`DataProtectionTokenProvider`) | Client device (authenticator app) |
| Transmission | Sent to user via email or SMS | User reads from app and types it |
| Validity duration | Configurable (default 1 day for email/password reset) | 30 seconds (±30 s tolerance) |
| Single-use | Yes — invalidated after first successful use | Yes — stamp tracks last used counter |
| Stored secret | The token itself is encrypted state | Shared secret key stored on both sides |
| Revokable | Yes — changing security stamp invalidates old tokens | Yes — `ResetAuthenticatorKeyAsync` revokes all codes |

Data protection tokens embed a timestamp and the user's security stamp in an encrypted blob; if the security stamp changes before the token is used, the token is rejected even within its validity window. TOTP tokens cannot be explicitly revoked without changing the shared secret, which is why `ResetAuthenticatorKeyAsync` is called when a user wants to remove or change their authenticator app. For password reset and email confirmation, data protection tokens are appropriate because the delivery channel proves the second factor; for interactive 2FA, TOTP is preferred because it does not depend on a potentially compromised email inbox.

---

## Gotchas — 2FA & Identity Extensibility (Interview Traps)

---

#### Gotcha 1. Enabling 2FA without verifying the first code locks users out

**Concepts**
- SetTwoFactorEnabledAsync must follow VerifyTwoFactorTokenAsync
- Clock synchronization confirmed only by first code verification
- Recovery codes generated in the same successful response

**Answer**

Many developers call `SetTwoFactorEnabledAsync(user, true)` as soon as the authenticator key is displayed, before the user has confirmed their app is correctly configured. If the user's clock is wrong or the QR code was not scanned correctly, they are immediately locked out of 2FA without a fallback. The correct flow always verifies the first TOTP code with `VerifyTwoFactorTokenAsync` and enables 2FA only on success — the verification step proves the device and server are synchronized. Recovery codes should be generated and displayed in the same successful response so the user always has a fallback before they close the setup page.

---

#### Gotcha 2. The security stamp invalidates remember-machine cookies after a password change

**Concepts**
- Remember-machine cookie embeds the security stamp
- ChangePasswordAsync updating the stamp, invalidating all remembered devices
- Expected security behaviour that surprises users

**Answer**

Developers often expect that changing a password only ends the user's current session, but because the remember-machine 2FA cookie also embeds the security stamp, it is invalidated simultaneously. The user will need to complete 2FA again on all previously remembered devices after a password change. The remember-machine cookie uses Data Protection encryption that includes the security stamp; when `ChangePasswordAsync` updates the stamp, the encrypted content of all existing remember-machine cookies becomes invalid. This is the correct security behaviour — a password change may indicate a compromise, and previously trusted machines should re-verify — but it surprises users who expect the remember-machine cookie to persist across password changes.

---

#### Gotcha 3. Recovery codes must be displayed immediately — they cannot be retrieved from the database later

**Concepts**
- GenerateNewTwoFactorRecoveryCodesAsync returning plain-text only once
- Hashes only stored — no retrieval path for plain-text
- TempData as safe bridge if a redirect is required

**Answer**

`GenerateNewTwoFactorRecoveryCodesAsync` returns the plain-text codes only once, in the return value of that call; only hashes are stored in the database. Attempting to show recovery codes again later returns nothing — the values are gone from server memory once the response is sent. The application must display recovery codes in the same HTTP response that generates them; any redirect before rendering them loses the codes permanently. The correct pattern is to store the codes in `TempData` if a redirect is required, or to render them inline in the success page without redirecting.

---

#### Gotcha 4. Custom `IUserStore` implementations must handle normalized names explicitly

**Concepts**
- SetNormalizedUserNameAsync and SetNormalizedEmailAsync called explicitly by Identity
- FindByNameAsync queries the normalized column — not the display column
- ILookupNormalizer uppercasing names by default

**Answer**

A common mistake when writing a custom user store is to store usernames and emails in their original casing and rely on database collation for case-insensitive lookups. ASP.NET Core Identity instead calls `SetNormalizedUserNameAsync` and `SetNormalizedEmailAsync` explicitly and uses those normalized values for all `FindByName` and `FindByEmail` lookups. If the custom store ignores normalized name setters, lookups will fail in case-sensitive ways — `FindByNameAsync("alice")` will not find a user whose normalized name was never set. The normalizer (`ILookupNormalizer`) uppercases names by default; the store's `FindByNameAsync` must query the normalized column, not the display column.

---

#### Gotcha 5. The `TwoFactorRequired` result does not mean the user is authenticated

**Concepts**
- TwoFactorRequired — password verified but no session cookie issued
- Identity.TwoFactorUserId temporary cookie is not an authentication cookie
- User.Identity.IsAuthenticated correctly false at this stage

**Answer**

When `PasswordSignInAsync` returns `SignInResult.TwoFactorRequired`, the user has proven their password but has not been granted a session cookie — the application must not treat this as a partially authenticated state that confers any privileges. The only thing set at this point is a temporary `Identity.TwoFactorUserId` cookie that identifies which sign-in attempt is in progress. Middleware or filters that check `User.Identity.IsAuthenticated` will correctly see the user as unauthenticated because no application cookie has been issued; the risk is in custom code that reads the temporary cookie and infers identity from it. The full session cookie is only written by `TwoFactorAuthenticatorSignInAsync` or `TwoFactorRecoveryCodeSignInAsync` after the second factor is successfully validated.

---

#### Gotcha 6. TOTP Codes Are Valid Across a Configurable Time Window — Not an Instant 30-Second Cutoff

**Concepts**
- RFC 6238 allowing a ±1 step tolerance (90-second effective window by default)
- `CompatibilityMode` in the ASP.NET Core TOTP provider controlling window width
- Clock skew between authenticator app and server causing valid codes to appear rejected

**Answer**

TOTP generates a new 6-digit code every 30 seconds, but ASP.NET Core Identity's implementation accepts codes from the current time step and the immediately preceding step by default — a 90-second effective validity window. This tolerance compensates for clock skew between the user's authenticator app and the server. Developers who expect a strict 30-second window will be surprised that a code displayed just before a step boundary is still accepted 40 seconds later. More importantly, for systems that require strict replay prevention, a used code can be accepted again within the same window if no `jti`-style single-use tracking is added — Identity itself does not record used codes and does not prevent replay within the window. Adding a per-code used cache keyed by `(userId, code, timeStep)` prevents in-window replay without breaking normal use.

---

#### Gotcha 7. SMS 2FA Has No Built-In Rate Limiting — Brute-Force of the Token Must Be Added Manually

**Concepts**
- SMS token typically 6 digits — 1-in-1,000,000 guessing probability
- No built-in failed-attempt counter for 2FA token submission
- `AccessFailedCountAsync` tracking only `PasswordSignInAsync` failures by default

**Answer**

The 6-digit numeric SMS token has a 1-in-1,000,000 probability of being guessed in a single attempt, but without rate limiting an attacker can make many automated guesses against `TwoFactorSignInAsync`. Identity's built-in lockout counter is incremented by `PasswordSignInAsync` failure — it is not automatically incremented by failed `TwoFactorSignInAsync` calls. A dedicated brute-force protection mechanism must be added to the 2FA submission endpoint: either call `UserManager.AccessFailedAsync` explicitly on failed token submission, implement IP-level rate limiting middleware, or add a time-delay after each failed attempt. SMS tokens are also vulnerable to SIM swapping and interception, so they should be treated as the weakest supported 2FA method.

---

#### Gotcha 8. `GetTwoFactorEnabledAsync` Returns `true` Even If No Authenticator App Is Configured

**Concepts**
- `TwoFactorEnabled` flag stored separately from authenticator key existence
- `GetAuthenticatorKeyAsync` returning null when no key has been generated
- Admin-reset flows that disable 2FA without clearing the authenticator key

**Answer**

`UserManager.GetTwoFactorEnabledAsync(user)` checks only the `TwoFactorEnabled` flag stored on the user record — it does not verify that an authenticator app key has been generated and confirmed. A user record can have `TwoFactorEnabled = true` but no authenticator key, no phone number, and no registered providers, if the flag was set directly (e.g., by a data migration or admin edit) without completing the setup flow. Attempting to trigger 2FA for such a user results in `PasswordSignInAsync` returning `TwoFactorRequired` but `TwoFactorSignInAsync` always failing because there is no valid token to verify against. The diagnosis check is `GetAuthenticatorKeyAsync(user)` — a non-null, non-empty result confirms an authenticator is registered; null means the key must be generated first.

---

#### Gotcha 9. `IUserTwoFactorTokenProvider` Is Registered by Name — Wrong Provider Name Means No Token Is Validated

**Concepts**
- `options.Tokens.AuthenticatorTokenProvider` specifying the provider name for TOTP
- Custom provider registered with a different name silently falling back to no provider
- `TokenOptions.DefaultAuthenticatorProvider` constant as the correct name key

**Answer**

ASP.NET Core Identity resolves token providers by name. Custom providers must be registered with `options.Tokens.AuthenticatorTokenProvider = "MyCustomTOTP"` and added via `services.AddSingleton<IUserTwoFactorTokenProvider<ApplicationUser>, MyCustomTOTP>()` using the same name as the key in the DI registration. If the name in `TokenOptions` does not match the registered name, the default TOTP provider is used instead — which means all custom validation logic is silently bypassed, and tokens generated by the custom provider are validated against the standard RFC 6238 algorithm instead. The built-in constant `TokenOptions.DefaultAuthenticatorProvider` should be used as the registration key unless a genuinely different name is required.

---

#### Gotcha 10. Re-Registering an Authenticator App Does Not Automatically Regenerate Recovery Codes

**Concepts**
- `ResetAuthenticatorKeyAsync` invalidating the old TOTP key — old codes fail
- Recovery codes remaining valid after authenticator reset unless explicitly regenerated
- Security policy requiring recovery code regeneration on authenticator re-enroll

**Answer**

When a user re-registers their authenticator app — typically because they got a new phone or their app was corrupted — `UserManager.ResetAuthenticatorKeyAsync` generates a new TOTP secret. This invalidates all codes from the old authenticator, which is correct. However, the existing recovery codes are stored independently and are **not** automatically regenerated; they remain valid for use with the newly registered authenticator. From a security standpoint, old recovery codes that may have been viewed or printed when the previous authenticator was set up should be invalidated when a new authenticator is enrolled. The application must explicitly call `GenerateNewTwoFactorRecoveryCodesAsync` during re-enrollment and prompt the user to save the new codes, rather than relying on the old ones silently remaining valid.
