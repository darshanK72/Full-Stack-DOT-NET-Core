/*
 * =============================================================================
 * 04. STATIC MEMBERS AND STATIC CLASSES — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Type-level members shared across all instances, static classes as
 *        utility holders, compile-time constants, and enums as named value sets.
 *
 * WHY IT MATTERS:
 *   Account-number sequences, tax rates, and validation helpers belong to the
 *   type — not to one object. Static members express shared state and behavior
 *   without creating instances. Static classes group related helpers; const and
 *   static readonly express values that should never change at runtime.
 *
 * WHAT YOU WILL LEARN:
 *   1.  What static means — type-level vs instance-level
 *   2.  static fields, methods, and properties
 *   3.  Static vs instance members — access rules and pitfalls
 *   4.  Preview: static constructor
 *   5.  Static class rules and utility patterns
 *   6.  const vs static readonly
 *   7.  Enums — declaration, parsing, switching, and [Flags]
 *   8.  Preview: basic singleton pattern
 *
 * =============================================================================
 */

using System;
using System.Linq;

namespace StaticMembersAndStaticClasses;

/*
 * =========================================================================
 * SECTION 1: WHAT "static" MEANS
 * =========================================================================
 *
 * Instance members belong to ONE object (this). Static members belong to the
 * TYPE itself — one copy shared by every instance and callable even when no
 * instance exists.
 *
 *   BankAccount alice = new BankAccount("Alice", 500m);  // instance
 *   BankAccount.BankName = "Contoso";                    // type-level
 *
 * Syntax: keyword static on the member declaration inside the class.
 *
 * Scenario: two customers open accounts at the same bank. Shared bank name and
 * account counters are static; each balance is instance data.
 *
 * --- 2a. static fields ---
 *
 * A static field has ONE storage location for the entire application domain
 * (per AppDomain). All instances read and write the same field.
 *
 * Common uses:
 *   • Counters (_totalAccountsCreated, _nextAccountNumber)
 *   • Shared caches or configuration loaded once
 *   • Default values applied before any instance exists
 *
 * Tip: prefix private static fields with _camelCase; expose read-only facts
 * through static properties when callers should not mutate state.
 *
 * Pitfall: mutable static fields are shared global state — multiple threads
 * can race unless you synchronize or use Interlocked. Prefer immutable
 * static readonly values when possible.
 *
 * --- 3a. static methods ---
 *
 * Called with TypeName.Method(...). Cannot use instance fields directly unless
 * you also receive an instance as a parameter.
 *
 * Good for:
 *   • Pure validation/formatting (no object state required)
 *   • Factory helpers that eventually return new instances
 *   • Shared algorithms that do not depend on this
 *
 * Compile note: calling an instance method without an object causes CS0120
 * ("An object reference is required for the non-static field, method, or
 * property ..."). A static method cannot read this.Balance without an instance.
 *
 * --- 4a. static properties ---
 *
 * Like static methods, static properties belong to the type. They can expose
 * get/set accessors over static fields.
 *
 *   public static string BankName { get; set; }
 *   public static int TotalAccountsCreated => _totalAccountsCreated;
 *
 * --- 5a. static vs instance on this type ---
 *
 * +----------------------+---------------------------+---------------------------+
 * | Aspect               | Instance member           | Static member             |
 * +----------------------+---------------------------+---------------------------+
 * | Belongs to           | One object (this)         | The type (one copy)       |
 * | Syntax to access     | obj.Field / obj.Method()  | TypeName.Field / Method() |
 * | Created when         | Object constructed        | First use / type load     |
 * | Typical data         | Name, balance, line items | Counters, shared config   |
 * | Can call instance?   | Yes (has this)            | Only with instance param  |
 * +----------------------+---------------------------+---------------------------+
 *
 * Rule of thumb: if the value or behavior is the same for every object of the
 * type, make it static. If each object needs its own copy, use instance.
 *
 * Always qualify static members with the type name (BankAccount.BankName).
 * Accessing a static member through an instance reference is a compile error:
 * CS0176 ("Member cannot be accessed with an instance reference").
 * -------------------------------------------------------------------------
 */
public class BankAccount
{
    private static int _nextAccountNumber = 1000;
    private static int _totalAccountsCreated;

    public int AccountNumber { get; }
    public string OwnerName { get; }
    public decimal Balance { get; private set; }

    public static string BankName { get; set; } = "Contoso Community Bank";

    public static int TotalAccountsCreated => _totalAccountsCreated;

    public BankAccount(string ownerName, decimal openingDeposit)
    {
        AccountNumber = _nextAccountNumber++;
        OwnerName = ownerName;
        Balance = openingDeposit;
        _totalAccountsCreated++;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Deposit must be positive.");
        }

        Balance += amount;
    }

    public static bool IsValidRoutingNumber(string routingNumber)
    {
        if (routingNumber.Length != 9)
        {
            return false;
        }

        foreach (char digit in routingNumber)
        {
            if (!char.IsDigit(digit))
            {
                return false;
            }
        }

        return true;
    }

    public override string ToString()
    {
        return $"#{AccountNumber} {OwnerName} — balance {Balance:C}";
    }
}

/*
 * =========================================================================
 * SECTION 6: static CONSTRUCTOR (PREVIEW)
 * =========================================================================
 *
 * A static constructor runs ONCE per type before any static or instance member
 * is first accessed. It initializes static fields that need runtime setup and
 * has no parameters.
 *
 *   static BankAccount()
 *   {
 *       // load config, wire static readonly fields
 *   }
 *
 * COVERED IN DETAIL LATER → 03. Constructors and Method Overloading
 *   (headline concepts only: runs once, no access modifier, no parameters,
 *    cannot be called directly, pairs with static readonly initialization)
 *
 * This chapter focuses on static members you call every day; constructor
 * ordering and chaining live in chapter 03.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 7: static CLASS RULES
 * =========================================================================
 *
 * Declared with static class TypeName { ... }. The compiler seals the type
 * automatically — it cannot be inherited or instantiated.
 *
 * Rules:
 *   1. Every member must be static (fields, methods, properties).
 *   2. Cannot use new TaxHelper() — CS0712.
 *   3. Cannot inherit from another class (except implicit object).
 *   4. Other classes cannot derive from a static class — CS0709.
 *   5. Instance constructors are not allowed; static constructor optional.
 *
 * Use static classes for stateless helper groups (Math, conversion, tax math).
 * When you need BOTH instance state and static helpers, use an ordinary class
 * with static members (like BankAccount), not a static class.
 *
 * Real-world parallel: System.Math is a static class — all members are static,
 * and you never construct Math.
 * -------------------------------------------------------------------------
 */
public static class TaxHelper
{
    public const decimal DefaultRate = 0.0825m;

    public static decimal CalculateSalesTax(decimal amount, decimal rate)
    {
        return Math.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
    }

    public static string FormatCurrency(decimal amount)
    {
        return amount.ToString("C");
    }
}

/*
 * =========================================================================
 * SECTION 8: const vs static readonly
 * =========================================================================
 *
 * Both express "do not reassign after initialization," but they differ in WHEN
 * the value is fixed and WHAT types are allowed.
 *
 * +------------------+---------------------------+--------------------------------+
 * |                  | const                      | static readonly                |
 * +------------------+---------------------------+--------------------------------+
 * | Fixed at         | Compile time               | Runtime (declaration or        |
 * |                  |                            | static constructor)            |
 * | Allowed types    | Primitives, string, null   | Any type                       |
 * | In metadata      | Inlined at call sites      | Loaded from static field       |
 * | Change impact    | Recompile all callers      | Recompile only this assembly   |
 * | Typical use      | MaxAttempts, app name      | Deploy date, config from env   |
 * +------------------+---------------------------+--------------------------------+
 *
 * Choose const for true compile-time literals. Choose static readonly when the
 * value comes from configuration, I/O, or complex construction.
 *
 * readonly (without static) applies to INSTANCE fields set in constructors —
 * covered in 02. Properties and Indexers and chapter 03 constructors.
 *
 * AppSettings is also a static class (Section 7) — every member here is static.
 * -------------------------------------------------------------------------
 */
public static class AppSettings
{
    public const int MaxLoginAttempts = 3;

    public const string ApplicationName = "Ledger Portal";

    public static readonly DateTime DeployedOnUtc =
        new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    public static readonly string EnvironmentName =
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
}

/*
 * =========================================================================
 * SECTION 9: ENUMS — NAMED CONSTANT SETS
 * =========================================================================
 *
 * enum declares a distinct value type with a fixed set of named constants.
 * Under the hood each name maps to an integer (default underlying type int,
 * starting at 0 unless you assign explicit values).
 *
 * Enums are "static-like" because members are type-level constants — you
 * reference TicketPriority.High without creating an enum instance via new.
 *
 * --- 9a. Declaring and comparing ---
 *
 * Use == and != like any value type. Avoid magic numbers in business logic.
 *
 * --- 9b. Underlying numeric value ---
 *
 * Cast to int (or the declared underlying type) when persisting to a database
 * or serializing to JSON numbers. You may choose a smaller underlying type:
 *
 *   enum OrderStatus : byte { Pending = 1, Shipped = 2 }
 *
 * --- 9c. Enum.Parse, TryParse, and GetName ---
 *
 * Parse converts strings from config/UI; TryParse avoids FormatException on
 * bad input. GetName maps a value back to text; returns null if undefined.
 *
 * --- 9d. Enum.GetValues and IsDefined ---
 *
 * GetValues returns every declared constant — useful for dropdowns and validation.
 * IsDefined checks whether a numeric value matches a named member.
 *
 * --- 9e. Switch on enum ---
 *
 * Switch expressions and statements give readable branching without magic
 * numbers scattered through the codebase. See GetSlaMessage on Program.
 * -------------------------------------------------------------------------
 */
public enum TicketPriority
{
    Low,
    Normal,
    High,
    Critical
}

/*
 * --- 9f. [Flags] enum (combined values) ---
 *
 * [Flags] lets you combine values with bitwise OR for multi-select scenarios
 * (permissions, feature toggles). Assign powers of two (1, 2, 4, 8, ...).
 * Use HasFlag or (channels & SupportChannel.Chat) != 0 to test membership.
 * Not every enum needs [Flags] — use it only when combinations are meaningful.
 * -------------------------------------------------------------------------
 */
[Flags]
public enum SupportChannel
{
    None = 0,
    Email = 1,
    Chat = 2,
    Phone = 4
}

public enum OrderStatus : byte
{
    Pending = 1,
    Shipped = 2,
    Delivered = 3
}

/*
 * =========================================================================
 * SECTION 10: SINGLETON PATTERN (PREVIEW)
 * =========================================================================
 *
 * Ensures only ONE instance of a type exists app-wide. Typical ingredients:
 *   • private constructor (blocks new outside the type)
 *   • static property exposing the sole instance
 *   • static readonly field holding the eagerly created object
 *
 * COVERED IN DETAIL LATER → 03. Constructors and Method Overloading
 *   (private constructor, thread-safe lazy initialization, DI alternatives)
 *
 * Here the singleton wraps a simple in-memory counter. Production apps often
 * prefer dependency injection over hand-rolled singletons.
 * -------------------------------------------------------------------------
 */
public sealed class AuditLogger
{
    private static readonly AuditLogger InstanceField = new AuditLogger();
    private int _entryCount;

    private AuditLogger()
    {
    }

    public static AuditLogger Instance => InstanceField;

    public int EntryCount => _entryCount;

    public void Record(string message)
    {
        _entryCount++;
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 11: DEMONSTRATION — static members ledger summary
     * =========================================================================
     *
     * Main wires the types defined above. Every local is consumed in output so
     * each declaration is used — no illustration-only variables.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        BankAccount.BankName = "Contoso Community Bank";
        BankAccount alice = new BankAccount("Alice Chen", 500.00m);
        BankAccount bob = new BankAccount("Bob Patel", 250.00m);

        int aliceNumber = alice.AccountNumber;
        int bobNumber = bob.AccountNumber;
        int totalCreated = BankAccount.TotalAccountsCreated;

        string validRouting = "021000021";
        string invalidRouting = "ABC";
        bool routingOk = BankAccount.IsValidRoutingNumber(validRouting);
        bool routingBad = BankAccount.IsValidRoutingNumber(invalidRouting);

        string bankLabel = BankAccount.BankName;
        decimal aliceBalanceBefore = alice.Balance;
        alice.Deposit(75.00m);
        decimal aliceBalanceAfter = alice.Balance;

        string instanceSummary = alice.ToString();
        string typeSummary = $"{BankAccount.BankName}: {BankAccount.TotalAccountsCreated} accounts";

        decimal subtotal = 120.00m;
        decimal tax = TaxHelper.CalculateSalesTax(subtotal, TaxHelper.DefaultRate);
        decimal totalWithTax = subtotal + tax;
        string formattedCurrency = TaxHelper.FormatCurrency(totalWithTax);

        int maxAttempts = AppSettings.MaxLoginAttempts;
        string appName = AppSettings.ApplicationName;
        DateTime deployedOn = AppSettings.DeployedOnUtc;
        string environmentName = AppSettings.EnvironmentName;

        TicketPriority aliceTicket = TicketPriority.High;
        TicketPriority bobTicket = TicketPriority.Normal;
        bool aliceIsUrgent = aliceTicket == TicketPriority.Critical
            || aliceTicket == TicketPriority.High;

        int highValue = (int)TicketPriority.High;
        TicketPriority parsedPriority = (TicketPriority)2;

        TicketPriority fromConfig = Enum.Parse<TicketPriority>("Critical");
        string priorityLabel = Enum.GetName(typeof(TicketPriority), aliceTicket) ?? "Unknown";
        bool tryParseOk = Enum.TryParse("Low", ignoreCase: true, out TicketPriority fromTryParse);

        TicketPriority[] allPriorities = Enum.GetValues<TicketPriority>().ToArray();
        int priorityCount = allPriorities.Length;
        bool criticalDefined = Enum.IsDefined(typeof(TicketPriority), TicketPriority.Critical);

        string slaMessage = GetSlaMessage(aliceTicket);
        string bobSlaMessage = GetSlaMessage(bobTicket);

        SupportChannel channels = SupportChannel.Email | SupportChannel.Chat;
        bool includesChat = channels.HasFlag(SupportChannel.Chat);
        bool includesPhone = (channels & SupportChannel.Phone) != 0;

        OrderStatus orderState = OrderStatus.Pending;
        byte pendingByte = (byte)orderState;

        AuditLogger logger = AuditLogger.Instance;
        logger.Record("Account opened for Alice");
        logger.Record("Deposit posted");
        int auditCount = logger.EntryCount;

        Console.WriteLine($"=== {appName} — static members demo ===");
        Console.WriteLine($"Environment: {environmentName}  Deployed (UTC): {deployedOn:yyyy-MM-dd}");
        Console.WriteLine($"Max login attempts (const): {maxAttempts}");
        Console.WriteLine();

        Console.WriteLine(typeSummary);
        Console.WriteLine($"Bank label (static property): {bankLabel}");
        Console.WriteLine($"Account numbers: Alice={aliceNumber}, Bob={bobNumber}  Total created={totalCreated}");
        Console.WriteLine(instanceSummary);
        Console.WriteLine($"Bob: {bob}");
        Console.WriteLine($"Alice balance: {aliceBalanceBefore:C} → {aliceBalanceAfter:C} after deposit");
        Console.WriteLine();

        Console.WriteLine($"Routing {validRouting} valid? {routingOk}   Routing '{invalidRouting}' valid? {routingBad}");
        Console.WriteLine();

        Console.WriteLine($"Subtotal {subtotal:C} + tax {tax:C} = {formattedCurrency}");
        Console.WriteLine();

        Console.WriteLine($"Ticket {priorityLabel} (value {highValue}) — SLA: {slaMessage}");
        Console.WriteLine($"Parsed enum value 2 → {parsedPriority}  TryParse Low OK? {tryParseOk} ({fromTryParse})");
        Console.WriteLine($"From config string 'Critical' → {fromConfig}");
        Console.WriteLine($"Bob ticket SLA: {bobSlaMessage}  Alice urgent? {aliceIsUrgent}");
        Console.WriteLine($"Enum values count: {priorityCount}  Critical defined? {criticalDefined}");
        Console.WriteLine($"Support channels: {channels}  includes chat? {includesChat}  includes phone? {includesPhone}");
        Console.WriteLine($"Order status Pending as byte: {pendingByte}");
        Console.WriteLine();

        Console.WriteLine($"Audit logger entries: {auditCount}  (singleton preview)");
    }

    /*
     * --- 9d. Switch on enum — SLA text by TicketPriority ---
     */
    private static string GetSlaMessage(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Low => "Respond within 5 business days",
            TicketPriority.Normal => "Respond within 2 business days",
            TicketPriority.High => "Respond within 1 business day",
            TicketPriority.Critical => "Respond within 4 hours",
            _ => "Unknown priority"
        };
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — STATIC MEMBERS AND STATIC CLASSES
 * =============================================================================
 *
 * --- static member syntax ---
 *
 *  public static int Count;
 *  public static void Reset() { }
 *  public static string Name { get; set; }
 *
 * --- access ---
 *
 *  MyType.StaticMember          // preferred — type name
 *  obj.InstanceMember           // object reference
 *  obj.StaticMember             // CS0176 — use type name instead
 *
 * --- static class ---
 *
 *  public static class Utils { public static int Add(int a, int b) => a + b; }
 *  // no new Utils(); no inheritance
 *
 * --- const vs static readonly ---
 *
 *  public const int Max = 10;                    // compile-time literal
 *  public static readonly DateTime Start = ...;  // runtime-once
 *
 * --- enum ---
 *
 *  enum Status { Pending, Shipped }
 *  enum Code : byte { A = 1, B = 2 }
 *  Status s = Status.Pending;
 *  int n = (int)s;
 *  Enum.Parse<Status>("Shipped");
 *  Enum.TryParse("Shipped", out Status parsed);
 *  Enum.GetValues<Status>();
 *  Enum.IsDefined(typeof(Status), Status.Pending);
 *
 * --- common errors ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  StaticMethod using instance field    | CS0120 object reference required
 *  new on static class                  | CS0712 cannot create instance
 *  inherit static class                 | CS0709 static types cannot be bases
 *  Enum.Parse bad string                | FormatException (use TryParse)
 *
 * --- previews (see other chapters) ---
 *
 *  static constructor       → 03. Constructors and Method Overloading
 *  singleton / private ctor → 03. Constructors and Method Overloading
 *
 * =============================================================================
 */
