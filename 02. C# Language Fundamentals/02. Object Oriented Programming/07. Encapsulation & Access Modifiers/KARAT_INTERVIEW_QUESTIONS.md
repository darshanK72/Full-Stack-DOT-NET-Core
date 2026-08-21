# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/07. Encapsulation & Access Modifiers`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A junior developer "simplifies" the chapter's `BankAccount` for a payments microservice. QA reports negative balances in production. Review the change — what broke the invariant, and how do you fix it?

```csharp
public class BankAccount
{
    public decimal Balance { get; set; }
    public string AccountNumber { get; }

    public BankAccount(string accountNumber, decimal openingDeposit)
    {
        AccountNumber = accountNumber;
        Balance = openingDeposit;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Balance += amount;
    }

    public bool TryWithdraw(decimal amount, out string message)
    {
        if (amount <= 0) { message = "Amount must be positive."; return false; }
        if (amount > Balance) { message = "Insufficient funds."; return false; }
        Balance -= amount;
        message = "OK";
        return true;
    }
}

// Elsewhere in the API layer:
account.Balance = -10_000m;   // "adjustment" from a support script
account.Balance += 999m;      // race between two threads — no lock
```

---

#### Q2. (R) A shared library ships both a public façade and internal implementation types. A consuming team references the NuGet package and complains they cannot unit-test ledger entries. Review the library surface:

```csharp
// Payments.Core.dll
internal class InternalLedger
{
    public List<string> Entries { get; } = new();
    public void Record(string description) => Entries.Add(description);
}

public class LedgerGateway
{
    public static InternalLedger CreateLedger() => new InternalLedger();

    public static string PostEntry(InternalLedger ledger, string description)
    {
        ledger.Record(description);
        return ledger.Entries[^1];
    }
}
```

What is wrong with this public API shape, and how would you redesign the assembly boundary?

---

#### Q3. (P) Two assemblies in the same solution — `Billing.Core` (library) and `Billing.Tests` — need test access to `internal` pricing helpers without exposing them on the public NuGet surface. A developer adds this to `Billing.Core.csproj`:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="Billing.Tests" />
</ItemGroup>
```

What does `InternalsVisibleTo` actually grant, what risks does it introduce if misused, and what guardrails would you apply before adding friend assemblies in a production codebase?

---

#### Q4. (R) A domain hierarchy models employee compensation. A subclass "optimizes" payroll by writing directly to protected state. Review:

```csharp
public abstract class Employee
{
    protected decimal _baseSalary;
    protected List<string> _auditTrail = new();

    protected Employee(decimal baseSalary)
    {
        _baseSalary = baseSalary;
        _auditTrail.Add($"Hired at {_baseSalary:C}");
    }

    public decimal GetBaseSalary() => _baseSalary;

    public virtual void ApplyRaise(decimal percent)
    {
        if (percent <= 0) throw new ArgumentOutOfRangeException(nameof(percent));
        _baseSalary *= (1 + percent / 100m);
        _auditTrail.Add($"Raise {percent}% applied");
    }
}

public class CommissionEmployee : Employee
{
    public CommissionEmployee(decimal baseSalary) : base(baseSalary) { }

    public void SetGuaranteedMinimum(decimal minimum)
    {
        _baseSalary = minimum;           // bypasses ApplyRaise validation/audit
        _auditTrail.Clear();             // hides history from HR reports
    }
}
```

What encapsulation failure does `protected` enable here, and how would you protect invariants for derived types?

---

#### Q5. (D) Your team designs an immutable `MemberProfile` DTO for cross-service messaging (similar to this chapter's `MemberProfile`). Two proposals:

**Proposal A — all init-only, mutable collection inside:**

```csharp
public sealed class MemberProfileDto
{
    public string MemberId { get; init; }
    public string Email { get; init; }
    public List<string> Roles { get; init; } = new();
}
```

**Proposal B — private ctor + factory + read-only surface:**

```csharp
public sealed class MemberProfileDto
{
    public string MemberId { get; }
    public string Email { get; }
    public IReadOnlyList<string> Roles { get; }

    private MemberProfileDto(string memberId, string email, IReadOnlyList<string> roles) { ... }

    public static MemberProfileDto Create(string memberId, string email, IEnumerable<string> roles) { ... }
}
```

Which approach would you ship for a message contract shared between three services, and why? What breaks if callers treat Proposal A as immutable?

---

#### Q6. (M) A plugin assembly (`Plugins.Payroll`) references your core HR assembly and defines `PayrollProcessor : Employee`. Developers expect to read `InternalCounter` on a base instance from the plugin, but the build fails with CS0122. Given this base class from the chapter:

```csharp
public class VisibilityBase
{
    internal int InternalCounter = 3;
    protected internal int ProtectedInternalCounter = 4;
    private protected int PrivateProtectedCounter = 5;
}
```

Explain why each of the three counters behaves differently from a **derived class in another assembly**, and which modifier you would choose for a hook intended only for first-party plugins compiled into the same assembly as the base.

---
