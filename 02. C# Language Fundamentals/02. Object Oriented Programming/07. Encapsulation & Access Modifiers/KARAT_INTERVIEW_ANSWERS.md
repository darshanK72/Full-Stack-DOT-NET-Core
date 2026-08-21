# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/07. Encapsulation & Access Modifiers`

---

#### Q1. (R) A junior developer "simplifies" the chapter's `BankAccount` for a payments microservice. QA reports negative balances in production. Review the change — what broke the invariant, and how do you fix it?

**Answer:** Exposing `Balance` as a public setter lets any caller bypass `TryWithdraw` and `Deposit` rules — the type no longer owns its invariant, so external code and concurrent writers can corrupt state even though the methods still look correct.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Public `{ get; set; }` on domain state | Callers assign `Balance` directly — negative balances, skipped validation |
| Concurrency | Read-modify-write on public property | Two threads can interleave `Balance +=` / `-=` without synchronization |
| Encapsulation | Methods enforce rules; property ignores them | `TryWithdraw` checks become advisory — support scripts and mappers bypass them |
| API contract | Mutable balance contradicts audit expectations | Ledger reconciliation finds amounts that never passed `Deposit`/`TryWithdraw` |

**Fix (priority order):**

1. Restore a **private** backing field (`private decimal _balance`) — no public setter.
2. Expose balance read-only: `public decimal Balance => _balance;` or keep `GetBalance()` — callers observe, they do not mutate.
3. Route all changes through methods that enforce invariants (positive deposits, sufficient funds, audit logging).
4. If external systems must post adjustments, add an explicit `ApplyAdjustment(decimal amount, string reason, IAuthorizationContext ctx)` that validates authorization — never a bare setter.
5. For concurrent updates, guard mutations with a lock, database transaction, or optimistic concurrency token — encapsulation alone does not fix races.

```csharp
private decimal _balance;

public decimal Balance => _balance;

public bool TryWithdraw(decimal amount, out string message)
{
    // sole path to decrease _balance
}
```

**Production takeaway:** Auto-properties feel idiomatic in C#, but a public setter on invariant-bearing state is a field in disguise — Karat uses this to test whether you protect rules at the type boundary, not only inside "happy path" methods. See **Program.cs** Section 1 — `BankAccount` keeps `_balance` private.

---

#### Q2. (R) A shared library ships both a public façade and internal implementation types. A consuming team references the NuGet package and complains they cannot unit-test ledger entries. Review the library surface:

**Answer:** The library leaked an `internal` type through public method signatures — `CreateLedger` and `PostEntry` expose `InternalLedger` on the public API, which is a compile error for external consumers and breaks the intended assembly boundary even if it compiled.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `public` methods return/accept `internal` type | CS0051/CS0052 — inconsistent accessibility; package may not build |
| API surface | Internal implementation type is part of public contract | Callers depend on types you intended to hide — versioning nightmare |
| Encapsulation | Façade pattern inverted — gateway exposes guts | `List<string> Entries` on internal type becomes reachable if accessibility bug is fixed |
| Testing | Consumers cannot construct `InternalLedger` | Tests forced to go through static gateway — brittle, no seam for fakes |

**Fix (priority order):**

1. Keep `InternalLedger` **internal**; never appear in public signatures.
2. Introduce a **public** abstraction: `public interface ILedger { string PostEntry(string description); }` implemented internally, or return `string`/`LedgerEntryId` DTOs only.
3. `LedgerGateway.PostEntry(string description)` creates the internal ledger internally — matches **Program.cs** Section 3 pattern.
4. For testability inside the library, use `InternalsVisibleTo` for test assembly **or** expose `ILedger` with an internal default implementation registered via DI.
5. Return immutable snapshots (`IReadOnlyList<LedgerEntry>`) rather than live `List<T>` references.

```csharp
public static class LedgerGateway
{
    public static string PostEntry(string description)
    {
        var ledger = new InternalLedger();
        return ledger.Record(description);
    }
}
```

**Production takeaway:** `internal` types belong behind public façades — leaking them in signatures is worse than making everything public because it fails at compile time and signals unclear API design. See **Program.cs** Section 3 — `LedgerGateway` hides `InternalLedger`.

---

#### Q3. (P) Two assemblies in the same solution — `Billing.Core` (library) and `Billing.Tests` — need test access to `internal` pricing helpers without exposing them on the public NuGet surface. A developer adds `InternalsVisibleTo` to the csproj. What does it grant, what risks does it introduce, and what guardrails apply?

**Answer:** `InternalsVisibleTo` lets named friend assemblies access `internal` types and members at compile time — it widens visibility from "same assembly" to "same assembly plus declared friends," without changing `public` NuGet consumers' view if friends are test or first-party tooling projects only.

- **What it grants:** Friend assemblies can reference `internal` classes, methods, and constructors — tests can call pricing helpers, factories, and validators directly without making them `public`.
- **Strong-name caveat:** Signed assemblies require `InternalsVisibleTo` to include the friend's public key (`Include="Tests, PublicKey=..."`) — mismatched keys silently fail to grant access.
- **Risks:** Every friend is a **maintenance coupling** — internal refactors break friend code; overuse turns `internal` into "public but inconvenient." Shipping `InternalsVisibleTo` to production friends (other product teams, plugins) expands your semver surface — `internal` changes become breaking for them.
- **Security:** Friends can invoke internal code paths — do not use IVT to bypass auth; it is a compile-time visibility tool, not a security boundary.
- **Guardrails:** Limit friends to `*.Tests` and build-time tooling; document in ARCHITECTURE.md; prefer `public` interfaces for legitimate extension points; never friend untrusted third-party assemblies; audit IVT entries in code review like public API changes.

**Production takeaway:** Friend assemblies are the idiomatic way to test `internal` implementation without polluting NuGet — Karat tests whether you distinguish visibility for **consumers** vs **collaborators**. See **Program.cs** Quick Reference — InternalsVisibleTo cross-ref to .NET Framework Architecture module.

---

#### Q4. (R) A domain hierarchy models employee compensation. A subclass "optimizes" payroll by writing directly to protected state. Review:

**Answer:** `protected` fields expose implementation details to every derived class — `CommissionEmployee` can mutate `_baseSalary` and `_auditTrail` without going through `ApplyRaise`, breaking payroll invariants and audit integrity that the base class thought it owned.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encapsulation | `protected` fields instead of controlled hooks | Subclasses bypass validation, logging, and business rules |
| Invariant | Direct `_baseSalary` assignment | Raises applied without approval workflow; minimum wage rules skipped |
| Audit | `_auditTrail.Clear()` | HR/compliance reports lose history — protected mutable collection |
| Design | Base class cannot enforce postconditions on derived behavior | Liskov violations — base assumes audit trail is append-only |

**Fix (priority order):**

1. Make fields **private**; stop exposing raw state to subclasses.
2. Expose controlled extension points: `protected void SetBaseSalary(decimal value, string reason)` that validates and appends audit entries — or make `ApplyRaise` `sealed`/`non-virtual` and use template method with protected abstract hooks that cannot touch salary directly.
3. Return **read-only** audit view: `public IReadOnlyList<string> AuditTrail => _auditTrail.AsReadOnly();` — never expose mutable `List<T>` as `protected`.
4. If derived types need guaranteed minimum, model it as separate state (`_guaranteedMinimum`) combined in a computed `EffectiveBaseSalary` — not by overwriting `_baseSalary`.
5. Consider `private protected` only for same-assembly inheritance helpers — not as a substitute for private fields.

```csharp
private decimal _baseSalary;
private readonly List<string> _auditTrail = new();

protected void AdjustBaseSalary(decimal newSalary, string reason)
{
    if (newSalary < 0) throw new ArgumentOutOfRangeException(nameof(newSalary));
    _baseSalary = newSalary;
    _auditTrail.Add(reason);
}
```

**Production takeaway:** `protected` is not "private but inheritance-friendly" for invariant-bearing data — it is a public API for every future subclass. See **Program.cs** Section 2 — `protected` visibility and Section 1 — behavior enforces invariants, not exposed fields.

---

#### Q5. (D) Your team designs an immutable `MemberProfile` DTO for cross-service messaging. Proposal A (init-only + `List<string>`) vs Proposal B (factory + `IReadOnlyList`). Which do you ship, and what breaks if callers treat Proposal A as immutable?

**Answer:** Ship Proposal B (or Proposal A only after deep-defensive copying and a read-only exposed collection) — init-only scalars are immutable after construction, but a mutable `List<string>` referenced by the DTO remains live, so "immutable" messages can change under downstream caches and serializers.

**Proposal A — hidden mutability:**

- `Roles` is init-settable once, but the **list contents** mutate forever — any holder can `dto.Roles.Add("Admin")` without reconstructing the DTO.
- Passing the DTO through a message bus, in-memory queue, or ORM session shares one list instance — Service B mutates roles; Service C sees the change — violates message immutability expectations.
- JSON deserializers often populate concrete `List<T>` — deserialization is fine, but post-deserialization mutation breaks contract assumptions.

**Proposal B — true read-only surface:**

- Factory validates inputs (non-empty email, trimmed strings — mirrors **Program.cs** `MemberProfile.Email` setter rules).
- Constructor copies roles into private `List<string>` or array; expose `IReadOnlyList<string>`.
- Callers cannot widen privileges after send — safe for retries, caching, parallel consumers.

**Pragmatic middle ground if staying with init:**

```csharp
public IReadOnlyList<string> Roles { get; init; }

// In factory/constructor:
Roles = roles.ToList().AsReadOnly();
```

**Production takeaway:** C# `init` immutability is shallow — reference-type properties still leak mutable innards. Karat tests whether you design DTOs like **Program.cs** Section 4 — `MemberId` get-only, `RegisteredOn` init-only, `LoginCount` private set — with attention to collection defense. See foundation **Properties** — init vs mutable backing stores.

---

#### Q6. (M) A plugin assembly references your core HR assembly and defines `PayrollProcessor : Employee`. Developers expect to read `InternalCounter` on a base instance from the plugin, but the build fails with CS0122. Explain visibility for `InternalCounter`, `ProtectedInternalCounter`, and `PrivateProtectedCounter` from a derived class in another assembly, and which modifier fits same-assembly first-party plugins.

**Answer:** From a derived class in **another assembly**, only `protected internal` members are accessible on `this` — plain `internal` is assembly-scoped (CS0122 from outside), and `private protected` requires both derivation **and** same assembly (narrowest intersection).

| Member | Modifier | Derived in other assembly |
|---|---|---|
| `InternalCounter` | `internal` | **No** — visible only inside HR assembly; unrelated same-assembly peers can access, but derived plugin code cannot |
| `ProtectedInternalCounter` | `protected internal` (union) | **Yes** — union: accessible if derived **or** same assembly; cross-assembly derivation satisfies the `protected` side |
| `PrivateProtectedCounter` | `private protected` (intersection) | **No** — must be derived **and** same assembly; plugin in `Plugins.Payroll.dll` fails CS0122 |

- **`internal` on base instance from derived code:** Even in the same assembly, `VisibilityDerived.SummarizeFromDerived` in **Program.cs** deliberately omits `InternalCounter` — `internal` is not inherited as a subclass privilege; it is assembly membership. Derived types do not get special access to `internal` members on arbitrary base instances unless they are in the same assembly (and even then, access is through the instance in same assembly — the chapter notes derived code in same asm still follows the visibility table).
- **First-party plugins compiled into the same assembly:** Use `private protected` when the hook must never leak to external extenders — same-assembly subclasses only. Use `protected internal` only when third-party plugins in other assemblies legitimately need the hook (widest combo — use sparingly).
- **Cross-assembly plugin extensibility:** Prefer `protected` methods with controlled behavior over exposing protected fields; keep counters private and expose `protected virtual OnPayrollProcessed()` template hooks.

**Production takeaway:** Combined modifiers are easy to misread — `protected internal` is a **union** (wider), `private protected` is an **intersection** (narrower). Karat embeds the chapter's **Program.cs** Section 2 visibility table in a cross-assembly plugin scenario — the fix is choosing the narrowest modifier that matches your trust boundary, not defaulting to `public`.

---
