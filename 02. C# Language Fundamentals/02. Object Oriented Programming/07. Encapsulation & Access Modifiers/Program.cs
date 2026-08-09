/*
 * =============================================================================
 * 07. ENCAPSULATION AND ACCESS MODIFIERS IN C#
 * =============================================================================
 *
 * TOPIC: Hiding implementation details and controlling who can read or change
 *        class members through access modifiers (public, private, protected,
 *        internal, and their combinations), with properties as the idiomatic
 *        encapsulation surface in C#.
 *
 * WHY IT MATTERS:
 *   A bank balance must not be set to any value from outside the account class.
 *   Framework types expose stable public APIs while keeping validation and storage
 *   private. Encapsulation protects invariants, reduces coupling, and lets you
 *   change internals without breaking callers.
 *
 * WHAT YOU WILL LEARN:
 *   1.  The encapsulation principle (data hiding + controlled access)
 *   2.  public and private — the default member access boundary
 *   3.  protected — extension hooks for derived types
 *   4.  internal — assembly-scoped visibility
 *   5.  protected internal and private protected — combined modifiers
 *   6.  Type-level visibility (public vs internal types)
 *   7.  Properties as encapsulation (auto, full, read-only, init, access on get/set)
 *   8.  Assembly boundaries and referencing other projects
 *
 * =============================================================================
 */

using System;
using System.Globalization;
using MyClassLibrary;

namespace EncapsulationAndAccessModifiers;

/*
 * =========================================================================
 * SECTION 1: ENCAPSULATION PRINCIPLE
 * =========================================================================
 *
 * ENCAPSULATION bundles data and behavior inside a type and exposes only what
 * callers need. Implementation details stay hidden; the public surface enforces
 * rules (validation, formatting, side effects).
 *
 * Without encapsulation:
 *   account._balance = -5000;   // anyone could corrupt state
 *
 * With encapsulation:
 *   account.TryWithdraw(100, out msg);  // rules enforced inside the type
 *
 * --- 1a. Private fields hold state ---
 *
 * Prefix private instance fields with _camelCase. Callers cannot read or assign
 * _balance directly — CS0122 if they try:
 *   account._balance = 0;
 *
 * --- 1b. Public methods enforce invariants ---
 *
 * Deposit rejects zero/negative amounts. TryWithdraw checks balance before
 * debiting. GetBalance exposes a read-only snapshot without a public setter.
 *
 * --- 1c. Minimal public property for low-risk data ---
 *
 * OwnerName uses an auto-property because renaming does not need complex rules.
 * Balance stays behind methods because withdrawal rules belong in behavior.
 * -------------------------------------------------------------------------
 */
public class BankAccount
{
    private decimal _balance;

    public string OwnerName { get; set; } = string.Empty;

    public string AccountNumber { get; }

    public BankAccount(string accountNumber, string ownerName, decimal openingDeposit)
    {
        AccountNumber = accountNumber;
        OwnerName = ownerName;
        Deposit(openingDeposit);
    }

    public decimal GetBalance()
    {
        return _balance;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Deposit must be positive.");
        }

        _balance += amount;
    }

    public bool TryWithdraw(decimal amount, out string message)
    {
        if (amount <= 0)
        {
            message = "Withdrawal amount must be positive.";
            return false;
        }

        if (amount > _balance)
        {
            message = $"Insufficient funds. Balance: {_balance:C}.";
            return false;
        }

        _balance -= amount;
        message = $"Withdrew {amount:C}. Remaining: {_balance:C}.";
        return true;
    }
}

/*
 * =========================================================================
 * SECTION 2: ACCESS MODIFIERS — OVERVIEW AND VISIBILITY LAB
 * =========================================================================
 *
 * Modifiers control VISIBILITY — who may refer to a member at compile time.
 *
 *   Modifier            | Same class | Same assembly | Derived (same asm) | Derived (other asm)
 *   --------------------|------------|---------------|--------------------|-----------------------
 *   (none / private)    | yes        | no            | no                 | no
 *   protected           | yes        | no            | yes                | yes
 *   internal            | yes        | yes           | yes*               | no
 *   protected internal  | yes        | yes           | yes                | yes
 *   private protected   | yes        | no            | yes                | no
 *   public              | yes        | yes           | yes                | yes
 *
 * * internal on a base class is visible to derived code only when the derived
 *   class lives in the same assembly.
 *
 * Defaults:
 *   • Class members without a modifier → private
 *   • Top-level types without a modifier → internal
 *
 * --- 2a. public and private ---
 *
 * public  — part of the type's outward contract; any code that can reference
 *           the type may use public members.
 * private — implementation detail of the declaring type only.
 *
 * --- 2b. protected ---
 *
 * Accessible in the declaring type and any derived type (even in another
 * assembly), but NOT from unrelated code in the same assembly.
 *
 * --- 2c. internal ---
 *
 * Visible anywhere inside the SAME ASSEMBLY (this project's compiled output).
 * Unrelated classes in the same assembly can read internal members on instances.
 *
 * --- 2d. protected internal (union) ---
 *
 * protected OR internal — reachable if the caller is in the same assembly OR
 * is a derived class (any assembly). Widest combination.
 *
 * --- 2e. private protected (intersection) ---
 *
 * protected AND internal — only derived classes in the SAME assembly. Narrowest
 * combination. A subclass compiled into another assembly gets CS0122.
 *
 * VisibilityBase / VisibilityDerived / VisibilityPeer below demonstrate legal
 * access from each context. Uncomment the blocked lines in Summarize* methods
 * to reproduce CS0122 (inaccessible due to protection level).
 * -------------------------------------------------------------------------
 */
public class VisibilityBase
{
    private int _privateCounter = 1;
    protected int ProtectedCounter = 2;
    internal int InternalCounter = 3;
    protected internal int ProtectedInternalCounter = 4;
    private protected int PrivateProtectedCounter = 5;
    public int PublicCounter = 6;

    public int SummarizeFromBase()
    {
        return _privateCounter
            + ProtectedCounter
            + InternalCounter
            + ProtectedInternalCounter
            + PrivateProtectedCounter
            + PublicCounter;
    }
}

public class VisibilityDerived : VisibilityBase
{
    public int SummarizeFromDerived()
    {
        // Legal: protected, protected internal, private protected, public.
        // NOT: private (_privateCounter), NOT internal on base instance from here.
        return ProtectedCounter
            + ProtectedInternalCounter
            + PrivateProtectedCounter
            + PublicCounter;
    }

    public int GetPrivateProtectedCounter()
    {
        return PrivateProtectedCounter;
    }
}

public class VisibilityPeer
{
    public int SummarizeFromPeer(VisibilityBase target)
    {
        // Legal: internal, protected internal, public on the target instance.
        // NOT: private, protected, private protected.
        return target.InternalCounter
            + target.ProtectedInternalCounter
            + target.PublicCounter;
    }
}

/*
 * =========================================================================
 * SECTION 3: TYPE-LEVEL VISIBILITY — internal TYPES
 * =========================================================================
 *
 * Access modifiers apply to TYPES as well as members:
 *
 *   public class Foo    — visible to any assembly that references this project
 *   internal class Bar  — visible only inside this assembly (default at namespace scope)
 *
 * InternalLedger below is an internal type. External assemblies cannot name
 * InternalLedger even with a ProjectReference — CS0122.
 *
 * LedgerGateway is public and hides the internal type behind a stable API —
 * a common library pattern: public façade, internal implementation types.
 * -------------------------------------------------------------------------
 */
internal class InternalLedger
{
    private int _entryCount;

    public string Record(string description)
    {
        _entryCount++;
        return $"Ledger entry #{_entryCount}: {description}";
    }
}

public static class LedgerGateway
{
    public static string PostEntry(string description)
    {
        InternalLedger ledger = new InternalLedger();
        return ledger.Record(description);
    }
}

/*
 * =========================================================================
 * SECTION 4: PROPERTIES AS ENCAPSULATION
 * =========================================================================
 *
 * Properties are the standard C# way to expose state without public fields.
 * Callers use dot syntax; the type controls get/set logic behind accessors.
 *
 * Prefer properties over public fields:
 *   • Validation in set
 *   • Computed read-only values
 *   • Change notification or lazy initialization (later chapters)
 *
 * --- 4a. Auto-implemented property ---
 *
 *   public string DisplayName { get; set; }
 *
 * Compiler generates a hidden backing field. Use when get/set need no extra logic.
 *
 * --- 4b. Full property with backing field ---
 *
 * Private _email field; public Email property validates on set (trim, reject empty).
 *
 * --- 4c. Read-only property (get-only) ---
 *
 *   public string MemberId { get; }   // set only in constructor or field initializer
 *
 * --- 4d. init-only property (C# 9+) ---
 *
 *   public DateTime RegisteredOn { get; init; }
 *
 * Settable during object initialization (object initializer or constructor) but
 * not afterward — useful for immutable identity data after construction.
 *
 * --- 4e. Restricted set access ---
 *
 *   public int LoginCount { get; private set; }
 *
 * public get lets callers read; private set limits writes to methods inside
 * MemberProfile (e.g. RecordLogin). Same pattern works with protected set for
 * inheritance scenarios.
 *
 * --- 4f. Expression-bodied read-only property ---
 *
 *   public string ProfileLabel => $"{DisplayName} ({MemberId})";
 *
 * Computed on each read; no backing field required for simple derivations.
 *
 * --- 4g. Different access on get vs set ---
 *
 * C# allows narrowing the setter independently:
 *   public string Status { get; private set; }
 *
 * The getter stays as accessible as the property declaration; the setter can be
 * more restrictive (private, protected, internal).
 *
 * Indexers, static properties, and overriding property behavior are covered in
 * COVERED IN DETAIL LATER → 02. Properties and Indexers
 * -------------------------------------------------------------------------
 */
public class MemberProfile
{
    private string _email = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string MemberId { get; }

    public DateTime RegisteredOn { get; init; }

    public int LoginCount { get; private set; }

    public string Email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Email cannot be empty.", nameof(value));
            }

            _email = value.Trim();
        }
    }

    public string ProfileLabel => $"{DisplayName} ({MemberId})";

    public MemberProfile(string memberId, string displayName, DateTime registeredOn)
    {
        MemberId = memberId;
        DisplayName = displayName;
        RegisteredOn = registeredOn;
    }

    public void RecordLogin()
    {
        LoginCount++;
    }

    public void UpdateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
        }

        DisplayName = displayName;
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 5: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Fixed demo values — no console input. Each block exercises types defined
     * above; output at the end ties encapsulation, modifiers, and properties
     * to one readable run.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        BankAccount account = new BankAccount("ACC-10042", "Jordan Lee", 250.00m);
        account.Deposit(75.50m);
        bool withdrawalSucceeded = account.TryWithdraw(120.00m, out string withdrawalMessage);
        decimal finalBalance = account.GetBalance();
        account.OwnerName = "Jordan L. Lee";
        string ownerAfterRename = account.OwnerName;

        VisibilityBase baseInstance = new VisibilityBase();
        VisibilityDerived derivedInstance = new VisibilityDerived();
        VisibilityPeer peer = new VisibilityPeer();
        int baseTotal = baseInstance.SummarizeFromBase();
        int derivedTotal = derivedInstance.SummarizeFromDerived();
        int peerTotal = peer.SummarizeFromPeer(baseInstance);
        int protectedInternalFromPeer = baseInstance.ProtectedInternalCounter;
        int protectedInternalFromDerived = derivedInstance.ProtectedInternalCounter;
        int privateProtectedFromDerived = derivedInstance.GetPrivateProtectedCounter();

        string ledgerLine = LedgerGateway.PostEntry("Opening balance recorded");

        DateTime registrationDate = new DateTime(2026, 1, 15);
        MemberProfile member = new MemberProfile("MBR-9001", "Alex Rivera", registrationDate)
        {
            Email = "alex.rivera@example.com"
        };
        member.RecordLogin();
        member.RecordLogin();
        member.UpdateDisplayName("Alex R. Rivera");
        string memberLabel = member.ProfileLabel;
        string memberEmail = member.Email;

        Class1 libraryType = new Class1();
        libraryType.Hello();

        Console.WriteLine("=== Encapsulation and Access Modifiers ===");
        Console.WriteLine();
        Console.WriteLine($"Account {account.AccountNumber} ({account.OwnerName})");
        Console.WriteLine($"Withdrawal OK: {withdrawalSucceeded} — {withdrawalMessage}");
        Console.WriteLine($"Final balance: {finalBalance:C}");
        Console.WriteLine($"Renamed owner: {ownerAfterRename}");
        Console.WriteLine();
        Console.WriteLine("Visibility totals (each Summarize* uses only legal members):");
        Console.WriteLine($"  From base type:              {baseTotal}");
        Console.WriteLine($"  From derived type:           {derivedTotal}");
        Console.WriteLine($"  From peer (same assembly):   {peerTotal}");
        Console.WriteLine($"  protected internal:          peer={protectedInternalFromPeer}, derived={protectedInternalFromDerived}");
        Console.WriteLine($"  private protected (derived): {privateProtectedFromDerived}");
        Console.WriteLine();
        Console.WriteLine(ledgerLine);
        Console.WriteLine();
        Console.WriteLine($"Member {memberLabel}");
        Console.WriteLine($"  Email: {memberEmail}");
        Console.WriteLine($"  Registered: {member.RegisteredOn:d}");
        Console.WriteLine($"  Logins: {member.LoginCount}");
        Console.WriteLine($"  Display name after update: {member.DisplayName}");
        Console.WriteLine();
        Console.WriteLine("Cross-assembly: public types in MyClassLibrary are callable;");
        Console.WriteLine("internal types in referenced assemblies are not (CS0122).");
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — ENCAPSULATION AND ACCESS MODIFIERS
 * =============================================================================
 *
 * --- Encapsulation ---
 *
 *  Hide fields; expose behavior (methods/properties) that enforce invariants.
 *  Private _field + public property/method is the standard C# pattern.
 *
 * --- Member visibility (class members) ---
 *
 *  Modifier            | Who can access
 *  --------------------|--------------------------------------------------
 *  private             | Declaring type only (default for members)
 *  protected           | Declaring type + any derived type
 *  internal            | Any code in the same assembly
 *  protected internal  | Same assembly OR derived (union — widest combo)
 *  private protected   | Derived types in the same assembly only (narrowest)
 *  public              | Everyone who can see the containing type
 *
 * --- Type visibility (top-level types) ---
 *
 *  internal (default)  | Same assembly only
 *  public              | Any referencing assembly
 *
 * --- Property encapsulation patterns ---
 *
 *  Auto:           public string Name { get; set; }
 *  Full:           private field + get/set with validation
 *  Read-only:      public string Id { get; }
 *  Init-only:      public DateTime Created { get; init; }
 *  Restricted set: public int Count { get; private set; }
 *  Computed:       public string Label => $"{Name} ({Id})";
 *
 * --- Common compile error ---
 *
 *  CS0122 — member is inaccessible due to its protection level
 *
 * --- Related chapters ---
 *
 *  Property/indexer depth, Equals/GetHashCode  → 02. Properties and Indexers
 *  protected in inheritance hierarchies          → 05. Inheritance and Polymorphism
 *  Assemblies, InternalsVisibleTo                → .NET Framework Architecture module
 *
 * =============================================================================
 */
