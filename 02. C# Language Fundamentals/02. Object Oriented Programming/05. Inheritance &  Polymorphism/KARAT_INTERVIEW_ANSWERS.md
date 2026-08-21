# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/05. Inheritance &  Polymorphism`

---

#### Q1. (R) Badge printing in production shows `"EMP"` for every staff member, including managers and contractors. Review this excerpt from the payroll service (pattern matches this chapter's `GetBadgeThroughEmployeeReference`). What is wrong, and how do you fix it?

**Answer:** `GetBadgeCode` is hidden with `new` in derived types but invoked through an `Employee` reference — binding is static (compile-time), so the base implementation always runs and every badge prints `"EMP"`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Polymorphism | `new` hides; no `virtual`/`override` | Base reference calls `Employee.GetBadgeCode()` — wrong badge for all derived instances |
| API contract | Polymorphic loop uses `Employee` (see chapter `ProcessPayroll` / badge helper) | HR export and access-control integrations show incorrect codes |
| Maintainability | Looks like overriding; behaves like hiding | Future devs add more `new` methods and repeat the bug |

**Fix (priority order):**

1. Make the base member polymorphic: `public virtual string GetBadgeCode() => "EMP";` and `public override string GetBadgeCode()` in derived types.
2. If badge text is not truly polymorphic, do not call it through `Employee` — accept `ContractEmployee`/`Manager` or introduce a strategy/interface (`IBadgeSource`) resolved at the call site.
3. Add a unit test that asserts badge text when the static type is `Employee` but the runtime type is `Manager` — catches hiding regressions.
4. Enable or heed compiler warning CS0114 ("hides inherited member") and treat `new` on instance methods as a code-review flag.

**Production takeaway:** Method hiding is the chapter's intentional trap — Karat tests whether you distinguish reference-type binding from virtual dispatch. See **Program.cs** Section 6b and `GetBadgeThroughEmployeeReference`.

---

#### Q2. (R) After adding `InternEmployee` to the payroll hierarchy, `ProcessPayroll` sometimes throws and totals are wrong. Review the new type and the unchanged payroll loop. What design rule did this violate, and what is the prioritized fix?

**Answer:** Substituting `InternEmployee` anywhere `Employee` is expected breaks callers that assume `CalculateNet()` always succeeds and returns a payroll amount — a Liskov Substitution Principle (LSP) violation, not a bug in the loop.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| LSP | Derived type throws where base contract implies a `Money` net pay | `ProcessPayroll` crashes when interns appear in `List<Employee>` |
| Design | Forced IS-A (`Intern : Employee`) for a role with incompatible pay semantics | Every consumer must special-case or try/catch |
| Extensibility | Polymorphic collection pattern (chapter Section 9a) assumes substitutability | Adding one subtype breaks aggregation without compile-time warning |

**Fix (priority order):**

1. Do not model interns as `Employee` if they cannot honor the payroll contract — use a separate type or composition (`PayrollParticipant` interface with `TryCalculateNet` / separate `StipendService`).
2. If they must share a collection, define an explicit contract on the base: document whether `CalculateNet()` may throw; prefer `Money?` or a result type over exceptions for expected branches.
3. Never fix this only with `if (employee is InternEmployee) continue` inside `ProcessPayroll` — that reintroduces switch-on-type and defeats the chapter's polymorphic design.
4. Add integration test: `ProcessPayroll` over mixed staff including the new role.

**Production takeaway:** LSP is not academic — any code that iterates `Employee[]` and calls `CalculateNet()` (as in **Program.cs** `ProcessPayroll`) trusts substitutability. Throwing overrides break that trust silently until runtime.

---

#### Q3. (R) A developer adds `Director : Manager` but the project fails to compile. Review the constructors. What is wrong with the chain, and what runs (in order) when `new Director(...)` succeeds?

**Answer:** The derived constructor must forward to an accessible base constructor with `: base(...)`; without it the compiler looks for a parameterless `Manager` constructor, finds none, and reports CS7036.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Missing `: base(id, name, dept, salary, perks, pf, teamBonus)` on `Director` | CS7036 — build blocked |
| Initialization | `BoardFee` assigned before base chain completes (if forced via workaround) | Illegal in C# — base constructors always run first |
| Hierarchy | Multilevel chain `Person → … → Manager → Director` requires each level to pass args upward | Easy to drop one salary/perk parameter when extending |

**Fix (priority order):**

1. Add explicit base forward:

```csharp
public Director(int id, string name, Department dept, Money salary,
    Money perks, Money pf, Money teamBonus, Money boardFee)
    : base(id, name, dept, salary, perks, pf, teamBonus)
{
    BoardFee = boardFee;
}
```

2. When `new Director(...)` runs successfully, constructors execute **base-first, outer-last**: `Person` → `Employee` → `PermanentEmployee` → `Manager` → `Director` body.
3. Reuse `: base(...)` in overrides like `CalculateNet()` when extending parent logic — same chaining idea as **Program.cs** `Manager.CalculateNet()` calling `base.CalculateNet()`.

**Production takeaway:** Constructor order is deterministic and non-negotiable — Karat uses multilevel payroll types to test CS7036 and whether you can narrate the chain. See **Program.cs** Sections 4 and 8.

---

#### Q4. (R) A refactor adds validation to the base payroll method. Contract net pay drops unexpectedly for some employees. Review the change. What broke, and how do you fix it without duplicating validation in every derived class?

**Answer:** Derived `CalculateNet` overrides replaced the base implementation entirely, so `ValidateNonNegative` in `Employee.CalculateNet()` no longer runs — a classic **fragile base class** problem when subclasses do not call `base.CalculateNet()`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Fragile base | Base gained behavior; derived overrides bypass it | Negative or inconsistent nets slip through for contract staff |
| Correctness | `ContractEmployee` omits validation on `BaseSalary` and `ContractBonus` | Payroll audit failures; possible overpayment |
| Maintainability | Every future override must remember hidden base rules | Each new employee type is a regression vector |

**Fix (priority order):**

1. Call base from override: `return base.CalculateNet() + ContractBonus;` (after ensuring bonus validation — either in base hook or local check).
2. Prefer **Template Method**: base defines `public Money CalculateNet() { Validate...; return CalculateNetCore(); }` with `protected abstract/virtual Money CalculateNetCore()` — derived types cannot skip validation.
3. Move cross-cutting rules to non-virtual helpers invoked from a sealed `CalculateNet()` on the base if the rule must never be skipped.
4. Add tests for each derived type asserting validation runs (negative salary throws).

**Production takeaway:** Adding logic to a base `virtual` method silently breaks subclasses that fully override — the fix is structural (template method / sealed orchestrator), not "remember to call base." Mirrors **Manager** reusing permanent math via `base.CalculateNet()` in **Program.cs**.

---

#### Q5. (P) A teammate replaces the polymorphic payroll loop with explicit type checks "for clarity." New `ContractEmployee` rows are added to the database but never appear in the exported total. Review the method. What failed at runtime, and what pattern from this chapter should drive payroll aggregation instead?

**Answer:** The `is PermanentEmployee` / `is Manager` ladder omits `ContractEmployee` (and any future sibling), so those instances contribute zero to `total` — silent underpayment, not a compile error.

- **Root cause:** Switching on concrete types duplicates dispatch the virtual table already provides; every new `Employee` subtype requires editing `ProcessPayroll`.
- **Manager branch is redundant noise:** `Manager` is a `PermanentEmployee` — if both were handled, order would matter; as written, neither contract nor many permanents may be counted correctly depending on edits.
- **Correct pattern:** Single polymorphic loop over `Employee` (or `IReadOnlyList<Employee>`) calling `employee.CalculateNet()` with `virtual`/`override` — exactly as **Program.cs** `ProcessPayroll` demonstrates in Section 9a.
- **Open/closed goal:** Add `ContractEmployee`, `PermanentEmployee`, `Manager` without changing the aggregator — new behavior lives in overrides.
- **If discrimination is truly required:** use visitor/double-dispatch or separate pipelines — not a partial `if/else` chain on siblings.

**Production takeaway:** Polymorphic collections only pay off when behavior stays on the type (`CalculateNet`, `RoleLabel`). Partial type switches fail open in finance code — totals look plausible but omit whole populations.

---

#### Q6. (D) Product wants `Employee` to inherit from a shared `AuditableEntity` base that already inherits `EntityBase`, while payroll still needs `Person → Employee → PermanentEmployee → Manager`. The team also proposes `Employee : Department` so every employee "is a department" for reporting. What breaks in C#, and where do LSP and fragile-base-class risks show up even if it compiles?

**Answer:** C# allows only one direct base class — you cannot chain `Person` and `AuditableEntity : EntityBase` on `Employee` without merging into one lineage or using interfaces; `Employee : Department` is a HAS-A relationship mis modeled as IS-A and invites LSP and fragile-hierarchy problems.

**Single inheritance (CS1721):**

- `class Employee : Person, AuditableEntity` does not compile — pick one base and move cross-cutting concerns to interfaces (`IAuditable`) or compose an `AuditableEntity` field.
- Deepening `Person → EntityBase → AuditableEntity → Employee → …` couples payroll to persistence auditing — changes to `AuditableEntity` (soft-delete flags, ORM hooks) ripple through every override (**fragile base class**).

**`Employee : Department` (HAS-A as IS-A):**

- Violates the chapter's composition rule: employees **have** departments (`HomeDepartment` field), they are not departments.
- LSP: code expecting a `Department` (code, name, org chart) receives an `Employee` — calling `Department` APIs on "employees" fails or returns nonsense.
- Reporting switches that pass `Department` into headcount APIs break when instances are actually people.

**Safer design:**

- Keep `Employee HAS-A Department`; share auditing via interface implementation on `Employee` or a small injected service.
- Cap hierarchy depth — prefer `Manager` bonuses over `Director : SeniorManager : …` solely for field reuse; extract shared payroll math into composable services when reuse is not IS-A.

**Production takeaway:** Inheritance is for substitutable IS-A contracts; auditing, persistence, and org structure are usually interfaces or composition. Karat ties **Program.cs** IS-A vs HAS-A (Section 2) to real hierarchy mistakes that compile only until you merge unrelated bases.

---
