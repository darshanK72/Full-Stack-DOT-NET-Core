/*
 * =============================================================================
 * 01. DELEGATES IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Delegates — type-safe references to methods with a matching signature.
 *        You declare a delegate type, store method references in variables, invoke
 *        them like functions, and combine multiple targets into one multicast chain.
 *
 * WHY IT MATTERS:
 *   Callbacks, plugin-style algorithms, and the .NET event system all depend on
 *   delegates. They let you pass behavior as data: choose a shipping rule at
 *   runtime, chain audit loggers, or wire a button click to a handler — without
 *   hard-coding every variation at compile time.
 *
 * WHAT YOU WILL LEARN:
 *   1.  What a delegate is (vs calling a method directly)
 *   2.  Declaring custom delegate types with the delegate keyword
 *   3.  Method group conversion, Invoke, and null-safe calls
 *   4.  Multicast delegates (+ =, -= =, invocation order)
 *   5.  Delegate.Combine and Delegate.Remove
 *   6.  Covariant return types on delegate assignments (brief)
 *   7.  Delegates vs interfaces — when each fits
 *   8.  Real-time order pipeline — pluggable rules and audit chain
 *   9.  Preview: lambda syntax, anonymous methods, Func/Action/Predicate
 *  10.  Preview: events defer to OOP Events chapter
 *
 * =============================================================================
 */

using System;
using System.Text;

namespace Delegates;

/*
 * =========================================================================
 * SECTION 1: WHAT IS A DELEGATE?
 * =========================================================================
 *
 * A DELEGATE is a reference type that points to one or more methods. The
 * compiler checks that each target's return type and parameter list match the
 * delegate declaration — unlike raw function pointers in other languages.
 *
 *   Concept          | Direct method call     | Delegate
 *   -----------------|------------------------|----------------------------------
 *   Target           | Fixed at compile time  | Chosen or swapped at runtime
 *   Type safety      | N/A                    | Signature enforced by compiler
 *   Multiple targets | One method             | Multicast chain (Sections 5–6)
 *
 * Every delegate type derives (indirectly) from System.MulticastDelegate.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: DECLARING CUSTOM DELEGATE TYPES
 * =========================================================================
 *
 * Use the delegate keyword to declare a new TYPE — like a class or interface:
 *
 *   public delegate int IntegerTransform(int value);
 *
 * Signature = return type + parameter list. Only methods that match exactly
 * can be assigned (wrong shape → CS0123 at compile time).
 *
 * Declare delegate types at namespace scope when several classes or helpers
 * share the same callback shape.
 * -------------------------------------------------------------------------
 */
public delegate int IntegerTransform(int value);
public delegate void BinaryOperationHandler(int left, int right);
public delegate void OrderAuditHandler(string message);
public delegate decimal ShippingRule(decimal weightKg, string zone);
public delegate decimal PriceAdjuster(decimal price);

/*
 * =========================================================================
 * SECTION 3: METHOD GROUP CONVERSION AND INVOKE
 * =========================================================================
 *
 * METHOD GROUP CONVERSION: writing Factorial without () passes the method
 * itself, not a call result. The compiler creates a delegate instance.
 *
 * INVOCATION — two equivalent forms:
 *
 *   int result = transform(7);            // shorthand — preferred
 *   int result = transform.Invoke(7);     // explicit — same IL
 *
 * The helper methods below are static int Factorial(int x) and static int
 * DoubleValue(int x) — return type and parameters match IntegerTransform.
 * -------------------------------------------------------------------------
 */
public static class TransformMethods
{
    public static int Factorial(int x)
    {
        if (x < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(x), "x must be >= 0");
        }

        if (x == 0)
        {
            return 1;
        }

        return x * Factorial(x - 1);
    }

    public static int DoubleValue(int value) => value * 2;
}

/*
 * =========================================================================
 * SECTION 4: NULL DELEGATE CHECKS
 * =========================================================================
 *
 * A delegate variable defaults to null. Calling Invoke on null throws
 * NullReferenceException. Use null-conditional invoke when a handler might
 * be absent:
 *
 *   handler?.Invoke("message");   // safe — skips if null
 *
 * REASSIGNMENT with = replaces the entire target list (unlike += which adds).
 *
 * Always check before Invoke when subscribers might unsubscribe down to null.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 5: MULTICAST DELEGATES (+ = AND -=)
 * =========================================================================
 *
 * Delegates are MULTICAST by default: += adds a method to the invocation
 * list; -= removes the first matching entry. When invoked, each handler runs
 * in registration order.
 *
 *   BinaryOperationHandler pipeline = Add;
 *   pipeline += Subtract;
 *   pipeline(42, 23);   // both run
 *
 * --- Return values on multicast ---
 *
 * For delegates that RETURN a value, only the LAST handler's return value
 * is kept; earlier returns are discarded. Multicast is designed for void
 * notification chains (logging, UI updates, events).
 *
 * --- GetInvocationList ---
 *
 * Returns each target separately — useful to invoke individually or count
 * subscribers without running the whole chain at once.
 * -------------------------------------------------------------------------
 */
public static class ArithmeticHandlers
{
    public static void Add(int x, int y)
    {
        Console.WriteLine($"Addition of {x} + {y} = {x + y}");
    }

    public static void Subtract(int x, int y)
    {
        Console.WriteLine($"Subtraction of {x} - {y} = {x - y}");
    }

    public static void Multiply(int x, int y)
    {
        Console.WriteLine($"Multiplication of {x} * {y} = {x * y}");
    }

    public static void Divide(int x, int y)
    {
        Console.WriteLine($"Division of {x} / {y} = {x / y}");
    }
}

/*
 * =========================================================================
 * SECTION 6: DELEGATE.COMBINE AND DELEGATE.REMOVE
 * =========================================================================
 *
 * += and -= are syntactic sugar over static methods on System.Delegate:
 *
 *   handler = (OrderAuditHandler)Delegate.Combine(handler, extraHandler);
 *   handler = (OrderAuditHandler)Delegate.Remove(handler, extraHandler);
 *
 * Combine returns a new multicast delegate (or null if both operands are null).
 * Remove drops the first occurrence of the second operand's target; if none
 * match, the original list is returned unchanged.
 *
 * Cast the result back to your delegate type after Combine/Remove.
 * -------------------------------------------------------------------------
 */
public static class AuditHandlers
{
    public static void AppendAuditEntry(string message)
    {
        Console.WriteLine($"[AUDIT] {message}");
    }

    public static void AppendAuditEntryWithTimestamp(string message)
    {
        Console.WriteLine($"[AUDIT {DateTime.Now:HH:mm:ss}] {message}");
    }

    public static void LogOrderPlaced(string message)
    {
        Console.WriteLine($"[ORDER] {message}");
    }
}

/*
 * =========================================================================
 * SECTION 7: COVARIANT RETURN TYPES ON DELEGATES (BRIEF)
 * =========================================================================
 *
 * Delegate return types are COVARIANT: a method that returns a more-derived
 * type can satisfy a delegate that promises a base return type.
 *
 *   public delegate ReportSummary SummaryFactory();
 *   SummaryFactory factory = BuildDetailedReport;   // OK — DetailedReport IS-A ReportSummary
 *
 * Parameter types are CONTRAVARIANT on delegates (less common in day-to-day code).
 * Full variance rules live in advanced generics material; here we show the
 * return-type case because it appears when wiring factory callbacks.
 * -------------------------------------------------------------------------
 */
public class ReportSummary
{
    public string Title { get; init; } = string.Empty;
}

public sealed class DetailedReport : ReportSummary
{
    public int PageCount { get; init; }
}

public delegate ReportSummary SummaryFactory();

public static class ReportBuilders
{
    public static DetailedReport BuildDetailedReport()
    {
        return new DetailedReport { Title = "Q4 Sales", PageCount = 12 };
    }
}

/*
 * =========================================================================
 * SECTION 8: DELEGATES VS INTERFACES
 * =========================================================================
 *
 * Both describe callable behavior, but they solve different problems.
 *
 *   Aspect              | Delegate                    | Interface
 *   --------------------|-----------------------------|----------------------------
 *   Shape               | Single method signature     | One or many members
 *   Multicast           | Built-in (+= / -=)          | Not built-in
 *   Implementation      | Any matching method         | class/struct must implement
 *   Typical use         | Callbacks, events, hooks    | Capabilities, contracts, DI
 *   Swap at runtime     | Reassign variable           | Swap implementing object
 *
 * Use a DELEGATE when you pass one piece of behavior (sort key, validator,
 * notification handler). Use an INTERFACE when the consumer needs several
 * related operations on one object (IRepository, IPaymentGateway).
 * -------------------------------------------------------------------------
 */
public interface IPriceAdjuster
{
    decimal Adjust(decimal price);
}

public sealed class LoyaltyPriceAdjuster : IPriceAdjuster
{
    private readonly decimal _tierPercent;

    public LoyaltyPriceAdjuster(decimal tierPercent)
    {
        _tierPercent = tierPercent;
    }

    public decimal Adjust(decimal price) => price * (1m - _tierPercent);
}

public static class PricingHelpers
{
    public static decimal ApplyTenPercentOff(decimal price) => price * 0.90m;
}

/*
 * =========================================================================
 * SECTION 9: REAL-TIME EXAMPLE — ORDER FULFILLMENT PIPELINE
 * =========================================================================
 *
 * Scenario: a warehouse service chooses a SHIPPING RULE at runtime (standard
 * vs express) and notifies multiple AUDIT handlers when an order is placed.
 *
 *   OrderFulfillmentService
 *     ├── ShippingRule (delegate) — one algorithm slot, swapped at runtime
 *     └── OrderAuditHandler chain — multicast logging
 *
 * Main wires the pipeline end-to-end — no menu switches.
 * -------------------------------------------------------------------------
 */
public sealed class OrderFulfillmentService
{
    public static ShippingRule SelectShippingRule(bool isExpress)
    {
        return isExpress ? ExpressShipping : StandardShipping;
    }

    public static decimal StandardShipping(decimal weightKg, string zone)
    {
        decimal baseRate = zone == "West" ? 6.50m : 8.00m;
        return baseRate + weightKg * 0.75m;
    }

    public static decimal ExpressShipping(decimal weightKg, string zone)
    {
        decimal baseRate = zone == "West" ? 14.00m : 16.50m;
        return baseRate + weightKg * 1.25m;
    }

    public static string BuildPipelineSummary(
        string orderId,
        string zone,
        decimal weightKg,
        decimal goodsTotal,
        decimal shippingFee,
        ShippingRule selectedRule,
        OrderAuditHandler? notifications)
    {
        decimal orderGrandTotal = goodsTotal + shippingFee;
        int handlerCount = notifications?.GetInvocationList().Length ?? 0;
        string ruleName = selectedRule == ExpressShipping ? "Express" : "Standard";

        StringBuilder summary = new StringBuilder();
        summary.AppendLine("=== Order Fulfillment Pipeline ===");
        summary.AppendLine($"Order: {orderId} | Zone: {zone} | Weight: {weightKg} kg");
        summary.AppendLine($"Goods: {goodsTotal:C} + Shipping: {shippingFee:C} = Total: {orderGrandTotal:C}");
        summary.AppendLine($"Shipping rule: {ruleName}");
        summary.AppendLine($"Audit handlers wired: {handlerCount}");
        return summary.ToString();
    }
}

/*
 * =========================================================================
 * SECTION 10: GENERIC DELEGATES — LIGHT RECAP
 * =========================================================================
 *
 * You saw generic delegate declarations in:
 *   03. Generics & Collections / 01. Generics (SECTION 6)
 *
 *   delegate TResult Mapper<TInput, TResult>(TInput input);
 *
 * The BCL ships ready-made generic delegate types (Func, Action, Predicate).
 * This chapter focuses on CUSTOM delegate types you declare yourself.
 *
 * COVERED IN DETAIL LATER → 05. Func Action and Predicate
 * -------------------------------------------------------------------------
 */
public delegate TResult Mapper<TInput, TResult>(TInput input);

public static class GenericDelegateHelpers
{
    public static string DescribeLength(string text) => $"{text.Length} chars";

    public static int ParseLength(string text) => text.Length;
}

/*
 * =========================================================================
 * SECTION 11: PREVIEW — LAMBDA SYNTAX, ANONYMOUS METHODS, BUILT-IN DELEGATES
 * =========================================================================
 *
 * Three modern ways to obtain a delegate instance without a separate named
 * method at namespace scope:
 *
 *   1. Lambda expression       x => x * 2
 *   2. Anonymous method        delegate (int x) { return x * 2; }
 *   3. Built-in generic types  Func<int, int>, Action<string>, Predicate<int>
 *
 * COVERED IN DETAIL LATER → 02. Lambda Expressions
 * COVERED IN DETAIL LATER → 03. Anonymous Methods
 * COVERED IN DETAIL LATER → 05. Func Action and Predicate
 *
 * Below: one-line previews only — each sibling chapter owns full depth.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 12: EVENTS — DEFERRED (NOT THIS CHAPTER)
 * =========================================================================
 *
 * C# EVENTS wrap multicast delegates so outsiders may only += / -= handlers;
 * they cannot Invoke the publisher's notification or wipe the list with = null.
 *
 * Events are NOT taught here — they belong in:
 *   02. Object Oriented Programming / 08. Events
 *
 * This chapter owns delegate mechanics: declaration, multicast, Combine/Remove,
 * null checks, covariance, and vs interfaces. Events add access control on top.
 * -------------------------------------------------------------------------
 */

public class Program
{
    /*
     * SECTION 13: DEMONSTRATION — Main orchestrates the chapter demo
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 01. Delegates ===");
        Console.WriteLine();

        // --- Section 3: method group conversion and Invoke ---
        IntegerTransform factorialTransform = TransformMethods.Factorial; // method group → delegate
        int factorialOfSeven = factorialTransform(7);                     // shorthand invoke

        Console.WriteLine("--- Declaration & invocation ---");
        Console.WriteLine($"Factorial of 7 via delegate: {factorialOfSeven}");
        Console.WriteLine($"Same via Invoke: {factorialTransform.Invoke(5)}");

        // --- Section 4: null checks and reassignment ---
        IntegerTransform? optionalTransform = null;
        optionalTransform?.Invoke(3); // null-safe — no output, no exception

        optionalTransform = TransformMethods.DoubleValue;
        Console.WriteLine($"Reassigned delegate: DoubleValue(21) = {optionalTransform(21)}");

        optionalTransform = TransformMethods.Factorial;
        Console.WriteLine($"Reassigned again: Factorial(6) = {optionalTransform(6)}");
        Console.WriteLine();

        // --- Section 5: multicast += / -= ---
        BinaryOperationHandler arithmeticChain = ArithmeticHandlers.Add;
        arithmeticChain += ArithmeticHandlers.Subtract;
        arithmeticChain += ArithmeticHandlers.Multiply;
        arithmeticChain += ArithmeticHandlers.Divide;

        Console.WriteLine("--- Multicast arithmetic chain ---");
        arithmeticChain.Invoke(42, 23);

        Delegate[] subscribers = arithmeticChain.GetInvocationList();
        Console.WriteLine($"Handlers in chain: {subscribers.Length}");
        Console.WriteLine();

        // --- Section 6: Delegate.Combine / Delegate.Remove ---
        OrderAuditHandler? combineChain = AuditHandlers.AppendAuditEntry;
        combineChain = (OrderAuditHandler)Delegate.Combine(
            combineChain,
            AuditHandlers.AppendAuditEntryWithTimestamp)!;

        Console.WriteLine("--- Combine / Remove ---");
        combineChain("Order ORD-9001 validated via Combine");

        combineChain = (OrderAuditHandler)Delegate.Remove(
            combineChain,
            AuditHandlers.AppendAuditEntry)!;

        combineChain?.Invoke("Order ORD-9001 shipped after Remove");

        OrderAuditHandler? auditChain = AuditHandlers.AppendAuditEntry;
        auditChain += AuditHandlers.AppendAuditEntryWithTimestamp;

        Console.WriteLine("--- Multicast audit (before -=) ---");
        auditChain("Order ORD-9001 validated");

        auditChain -= AuditHandlers.AppendAuditEntry; // remove first occurrence only
        Console.WriteLine("--- Multicast audit (after -=) ---");
        auditChain?.Invoke("Order ORD-9001 shipped");

        auditChain -= AuditHandlers.AppendAuditEntryWithTimestamp;
        bool chainIsEmpty = auditChain is null;
        Console.WriteLine($"Audit chain empty after full unsubscribe: {chainIsEmpty}");
        Console.WriteLine();

        // --- Section 7: covariant return type on delegate assignment ---
        SummaryFactory summaryFactory = ReportBuilders.BuildDetailedReport; // DetailedReport → ReportSummary
        ReportSummary summary = summaryFactory();
        DetailedReport? detailed = summary as DetailedReport;

        Console.WriteLine("--- Covariant return type ---");
        Console.WriteLine($"Summary title: {summary.Title}");
        Console.WriteLine($"Downcast page count: {detailed?.PageCount}");
        Console.WriteLine();

        // --- Section 8: delegates vs interfaces ---
        decimal listPrice = 120.00m;

        PriceAdjuster delegateAdjuster = PricingHelpers.ApplyTenPercentOff;
        decimal delegatePrice = delegateAdjuster(listPrice);

        IPriceAdjuster interfaceAdjuster = new LoyaltyPriceAdjuster(tierPercent: 0.15m);
        decimal interfacePrice = interfaceAdjuster.Adjust(listPrice);

        Console.WriteLine("--- Delegates vs interfaces ---");
        Console.WriteLine($"List {listPrice:C} → delegate adjuster: {delegatePrice:C}");
        Console.WriteLine($"List {listPrice:C} → interface adjuster: {interfacePrice:C}");
        Console.WriteLine();

        // --- Section 9: order fulfillment pipeline ---
        const string orderId = "ORD-7742";
        const decimal weightKg = 8.5m;
        const string zone = "West";
        const decimal goodsTotal = 349.99m;

        ShippingRule selectedRule = OrderFulfillmentService.SelectShippingRule(isExpress: true);
        decimal shippingFee = selectedRule(weightKg, zone);

        OrderAuditHandler orderNotifications = AuditHandlers.LogOrderPlaced;
        orderNotifications += AuditHandlers.AppendAuditEntryWithTimestamp;
        orderNotifications += AuditHandlers.LogOrderPlaced;

        orderNotifications?.Invoke($"Placed {orderId}: goods {goodsTotal:C}, ship {shippingFee:C}");

        string pipelineSummary = OrderFulfillmentService.BuildPipelineSummary(
            orderId, zone, weightKg, goodsTotal, shippingFee, selectedRule, orderNotifications);

        Console.WriteLine();
        Console.WriteLine(pipelineSummary);

        // --- Section 10: generic delegate recap (from Generics chapter) ---
        Mapper<string, string> describe = GenericDelegateHelpers.DescribeLength;
        Mapper<string, int> countChars = GenericDelegateHelpers.ParseLength;

        Console.WriteLine("--- Generic delegate recap ---");
        Console.WriteLine($"Mapper<string,string>: {describe("warehouse")}");
        Console.WriteLine($"Mapper<string,int>: {countChars("warehouse")}");
        Console.WriteLine();

        // --- Section 11: syntax previews (sibling chapters own full depth) ---
        IntegerTransform lambdaPreview = value => value + 10; // lambda — ch.02
        IntegerTransform anonymousPreview = delegate (int value) { return value + 20; }; // ch.03

        Func<int, int> funcPreview = TransformMethods.Factorial; // ch.05
        Action<string> actionPreview = AuditHandlers.AppendAuditEntry;
        Predicate<int> predicatePreview = static value => value % 2 == 0;

        Console.WriteLine("--- Syntax previews ---");
        Console.WriteLine($"Lambda preview IntegerTransform(5) = {lambdaPreview(5)}");
        Console.WriteLine($"Anonymous method preview(5) = {anonymousPreview(5)}");
        Console.WriteLine($"Func<int,int> Factorial(4) = {funcPreview(4)}");
        actionPreview("Action preview: audit via Action<string>");
        Console.WriteLine($"Predicate<int> isEven(10) = {predicatePreview(10)}");
        Console.WriteLine();

        // --- Section 12: events deferred — direct multicast without event keyword ---
        OrderAuditHandler standaloneNotification = AuditHandlers.LogOrderPlaced;
        standaloneNotification += AuditHandlers.AppendAuditEntryWithTimestamp;
        standaloneNotification("Preview: holders can Invoke directly — events add access control");

        Console.WriteLine();
        Console.WriteLine("=== Delegate tutorial complete ===");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — DELEGATES
 * =========================================================================
 *
 * --- Declare & invoke ---
 *
 *   public delegate TReturn Name(TParam p);
 *   Name handler = TargetMethod;     // method group conversion
 *   TReturn r = handler(arg);        // or handler.Invoke(arg)
 *   handler?.Invoke(arg);            // null-safe
 *
 * --- Multicast ---
 *
 *   handler += MethodB;              // add to chain
 *   handler -= MethodB;              // remove first match
 *   handler = MethodC;               // replace entire chain
 *   Delegate[] list = handler.GetInvocationList();
 *
 * --- Combine / Remove ---
 *
 *   handler = (MyDelegate)Delegate.Combine(handler, extra)!;
 *   handler = (MyDelegate)Delegate.Remove(handler, extra)!;
 *
 * --- void vs returning multicast ---
 *
 *   void delegates     → every handler runs
 *   returning delegate → only LAST handler's return value kept
 *
 * --- Covariant return (brief) ---
 *
 *   delegate Base Factory();
 *   Factory f = MethodReturningDerived;   // OK when Derived : Base
 *
 * --- vs interface ---
 *
 *   Delegate  → single callback, multicast, runtime swap
 *   Interface → multi-method contract, DI, test doubles
 *
 * --- Syntax alternatives (preview) ---
 *
 *   Lambda           → 02. Lambda Expressions
 *   Anonymous method → 03. Anonymous Methods
 *   Func/Action      → 05. Func Action and Predicate
 *   Events           → 02. OOP / 08. Events (not this folder)
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Invoke null delegate                 | NullReferenceException
 *  Signature mismatch on assignment     | CS0123
 *  Expect all return values from chain  | Only last return survives
 *  public delegate field for pub/sub    | Subscribers can be cleared — use event
 *
 * =========================================================================
 */
