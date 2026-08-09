/*
 * =============================================================================
 * 07. METHODS IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Methods — named blocks of code you call by name, with parameters,
 *        return values, overloading and overload resolution, ref/out/in,
 *        params, optional and named arguments, and recursion.
 *
 * WHY IT MATTERS:
 *   Real programs repeat the same calculations and rules in many places.
 *   Methods package that logic once, name it clearly, and return results —
 *   so pricing, validation, and reports stay consistent and testable instead
 *   of copy-pasted blocks that drift apart.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Method syntax (access, static, return type, name, body)
 *   2.  Parameters and return values (void, early return, value vs reference)
 *   3.  Expression-bodied methods
 *   4.  Method overloading
 *   5.  Overload resolution — how the compiler picks the best match
 *   6.  Call by value vs ref
 *   7.  out parameters (Try-pattern, inline declaration)
 *   8.  in parameters — read-only reference (avoid struct copies)
 *   9.  params arrays
 *  10.  Optional parameters
 *  11.  Named arguments
 *  12.  Recursion (base case, stack overflow risk)
 *  13.  Static methods (intro preview)
 *  14.  Local functions (intro preview)
 *
 * =============================================================================
 */

using System;

namespace Methods;

/*
 * =========================================================================
 * SECTION 1: WHAT IS A METHOD?
 * =========================================================================
 *
 * A METHOD is a named block of statements that runs when you CALL it.
 * In C#, every executable statement lives inside a method (Main is one).
 *
 * Typical shape:
 *
 *   accessModifier  static?  returnType  Name(parameter list)
 *   {
 *       // body
 *       return value;   // if returnType is not void
 *   }
 *
 *  Part            | Role
 *  ----------------|------------------------------------------------------
 *  access          | Who can call it (public, private, …) — OOP depth later
 *  static          | Belongs to the type, not an instance (preview §13)
 *  return type     | Type of value produced, or void if none
 *  Name            | Identifier used at the call site
 *  parameters      | Inputs declared in parentheses
 *
 * Call site:  ReturnType result = MethodName(arg1, arg2);
 *
 * Main(string[] args) is the entry method. Command-line args were covered
 * in 03. Input & Output — this chapter focuses on writing your own methods.
 *
 * Scenario: warehouse order helpers — line totals, discounts, packing.
 * -------------------------------------------------------------------------
 */
public class Program
{
    /*
     * =========================================================================
     * SECTION 2: PARAMETERS AND RETURN VALUES
     * =========================================================================
     *
     * PARAMETERS are variables declared in the method signature. ARGUMENTS are
     * the values you pass at the call site.
     *
     *   CalculateLineTotal(quantity, unitPrice)
     *                      ────────  ─────────
     *                      arguments (call site)
     *
     *   static decimal CalculateLineTotal(int qty, decimal price)
     *                                     ───      ─────
     *                                     parameters (declaration)
     *
     * --- 2a. Returning a value ---
     *
     * Non-void methods MUST return a value of the declared type on every path
     * (or throw). Missing return → CS0161.
     *
     * --- 2b. void methods ---
     *
     * void means "no return value." Use for side effects (print, update state).
     * You cannot assign the call to a variable.
     *
     * --- 2c. Early return ---
     *
     * return;           // exit a void method early
     * return expression; // exit and produce a value
     *
     * --- 2d. Value types vs reference types at the call site ---
     *
     * Value types (int, decimal, struct): default call passes a COPY of bits.
     * Reference types (string, class): default call passes a COPY of the
     * reference — both caller and callee see the same object on the heap.
     * Mutating object state through that reference is visible to the caller;
     * reassigning the parameter to a new object is not (see §6).
     * -------------------------------------------------------------------------
     */
    public static decimal CalculateLineTotal(int qty, decimal price)
    {
        if (qty <= 0)
        {
            return 0m;   // early return — valid on every path
        }

        return qty * price; // non-void path must return decimal on every branch
    }

    public static void PrintSkuBanner(string skuCode)
    {
        if (string.IsNullOrWhiteSpace(skuCode))
        {
            return;      // early exit — void, no value
        }

        Console.WriteLine($"SKU: {skuCode}"); // side effect only — no return value
    }

    /*
     * =========================================================================
     * SECTION 3: EXPRESSION-BODIED METHODS
     * =========================================================================
     *
     * When the body is a single expression, use => instead of a brace block:
     *
     *   static decimal ApplyPercent(decimal amount, decimal rate)
     *       => amount * rate;
     *
     * Equivalent to { return amount * rate; }.
     *
     * Works for void methods too when the body is one statement:
     *
     *   static void Ping() => Console.WriteLine("ok");
     *
     * Prefer expression-bodied form for short, obvious calculations — keep
     * multi-step logic in a normal block body for readability.
     * -------------------------------------------------------------------------
     */
    public static decimal ApplyPercent(decimal amount, decimal rate) => amount * rate; // expression-bodied — single return

    /*
     * =========================================================================
     * SECTION 4: METHOD OVERLOADING
     * =========================================================================
     *
     * OVERLOADING = same method name, different PARAMETER LISTS (signature).
     * The compiler picks the best match from the arguments you pass (see §5).
     *
     * What counts as a different signature:
     *   - Different number of parameters
     *   - Different parameter types (in order)
     *   - ref / out / in / params / optional change the signature
     *
     * What does NOT distinguish overloads:
     *   - Return type alone
     *   - Parameter names alone
     *
     *   // INVALID — same parameter types, only return type differs:
     *   // int Format(int x) { … }
     *   // string Format(int x) { … }   // CS0111
     *
     * Deep OOP context (constructors, inheritance) →
     * COVERED IN DETAIL LATER → 02. Object Oriented Programming /
     *   03. Constructors and Method Overloading
     * -------------------------------------------------------------------------
     */
    public static decimal EstimateShipping(decimal orderAmount)
    {
        return orderAmount >= 500m ? 0m : 12.50m; // one-parameter overload — free shipping threshold
    }

    public static decimal EstimateShipping(decimal orderAmount, double weightKg)
    {
        decimal baseFee = EstimateShipping(orderAmount); // reuses the single-arg overload
        decimal weightFee = (decimal)weightKg * 1.25m;   // implicit numeric conversion at call site
        return baseFee + weightFee;
    }

    /*
     * =========================================================================
     * SECTION 5: OVERLOAD RESOLUTION
     * =========================================================================
     *
     * When you write ApplyDiscount(100m, 10), the compiler must choose ONE
     * overload. Resolution happens in two phases:
     *
     *   1. APPLICABLE — argument list can be converted to the parameter list
     *   2. BETTER FUNCTION MEMBER — pick the single best among applicable
     *
     * Better-match rules (simplified):
     *
     *   Rule                         | Example
     *   -----------------------------|------------------------------------
     *   Exact type beats conversion  | (decimal,int) beats (decimal,decimal)
     *                                  when you pass literal 10 (int)
     *   Fewer conversions win        | one implicit cast beats two
     *   Non-params beats params      | fixed arity before params expansion
     *   Non-optional beats optional  | required params before defaulted
     *
     * --- 5a. Demonstration overloads ---
     *
     * Three ApplyDiscount overloads below differ by second parameter type or
     * count. Calls in Main show which one the compiler selects.
     *
     * --- 5b. Ambiguous calls (CS0121) ---
     *
     * If two overloads are equally good, compilation fails:
     *
     *   // ApplyDiscount(100m, 10.0)   // double fits decimal AND int paths
     *   //                              // equally — CS0121 ambiguous
     *
     * Fix: cast the argument (ApplyDiscount(100m, (int)10)) or rename overloads.
     *
     * --- 5c. Named arguments do not pick the overload ---
     *
     * Names apply AFTER overload resolution — they only reorder arguments for
     * the chosen signature. You cannot use a name to select a different overload.
     * -------------------------------------------------------------------------
     */
    public static decimal ApplyDiscount(decimal amount, decimal rate)
    {
        return amount * (1m - rate); // rate as fraction — e.g. 0.10m = 10% off
    }

    public static decimal ApplyDiscount(decimal amount, int percentOff)
    {
        return amount * (1m - percentOff / 100m); // int literal 5 picks this overload over decimal path
    }

    public static decimal ApplyDiscount(decimal amount, decimal rate, bool floorAtZero)
    {
        decimal result = amount * (1m - rate);
        return floorAtZero && result < 0m ? 0m : result; // optional cap when rate exceeds 100%
    }

    public static int CountTokens(params string[] parts)
    {
        return parts.Length; // params expands variable args into a string[]
    }

    public static int CountTokens(string first, string second)
    {
        return 2; // fixed two-arg overload — preferred over params when arity matches exactly
    }

    /*
     * =========================================================================
     * SECTION 6: CALL BY VALUE VS ref
     * =========================================================================
     *
     * Default for value types (int, decimal, struct, …): CALL BY VALUE.
     * The method receives a COPY. Assigning to the parameter does not change
     * the caller's variable.
     *
     * ref passes an ALIAS to the caller's variable. Changes inside the method
     * are visible to the caller. Requirements:
     *   - Caller must initialize the variable before the call
     *   - Call site and declaration both use the ref keyword
     *
     *   AdjustQuantity(ref quantity, 12);   // quantity must already be set
     *
     * Uninitialized ref → CS0165 (use of unassigned local variable).
     *
     * Use ref when a method must update an existing variable in place.
     * Prefer returning a new value when a single result is enough — clearer.
     * -------------------------------------------------------------------------
     */
    public static void TryBumpByValue(int units, int extra)
    {
        units += extra;   // only the copy changes
    }

    public static void AdjustQuantity(ref int units, int extra)
    {
        units += extra; // ref alias — caller's variable changes here
    }

    /*
     * =========================================================================
     * SECTION 7: out PARAMETERS
     * =========================================================================
     *
     * out is like ref but for OUTPUT: the method MUST assign the parameter
     * before returning (every path). The caller does not need to initialize
     * beforehand. Unassigned out on return → CS0177.
     *
     * Classic pattern (TryParse style):
     *
     *   bool ok = TrySplitCases(packedUnits, 12, out int fullCases, out int loose);
     *
     * --- 7a. C# 7 inline out declaration ---
     *
     * You may declare the out variable at the call site (shown above). Older
     * style declared variables first, then passed them:
     *
     *   int fullCases; int loose;
     *   TrySplitCases(packedUnits, 12, out fullCases, out loose);
     *
     * --- 7b. out vs ref ---
     *
     *   Modifier | Caller must init? | Callee must assign? | Primary use
     *   ---------|-------------------|---------------------|------------------
     *   (none)   | yes (for use)     | no                  | input copy
     *   ref      | yes               | no                  | read/write alias
     *   out      | no                | yes                 | extra outputs
     *
     * COVERED IN DETAIL LATER → 08. Advanced C# Features / 05. C# 7 Features
     * -------------------------------------------------------------------------
     */
    public static bool TrySplitCases(int units, int unitsPerCase, out int cases, out int remainder)
    {
        cases = units / unitsPerCase;       // out must be assigned on every path before return
        remainder = units % unitsPerCase;
        return remainder == 0;              // bool result — Try-pattern style success flag
    }

    /*
     * =========================================================================
     * SECTION 8: in PARAMETERS — READ-ONLY REFERENCE
     * =========================================================================
     *
     * in passes an alias like ref, but the callee cannot assign to the
     * parameter (read-only). For large structs, in avoids copying bytes while
     * preventing accidental mutation inside the method.
     *
     *   ReadOnlyLineTotal(in slip)   // slip is PackingSlip struct below
     *
     * Caller does not write in at the call site for variables — only the
     * declaration uses in. Passing a property or expression may require
     * a temporary (compiler creates a copy) — prefer a local variable.
     *
     * ref readonly (C# 7.2+) on parameters is closely related; in is the
     * common choice for "read large struct efficiently."
     *
     * Attempting to assign inside the method → CS8331 (cannot assign to in).
     * -------------------------------------------------------------------------
     */
    public readonly struct PackingSlip
    {
        public readonly string Sku;         // readonly fields — struct passed by value unless in/ref
        public readonly int Units;
        public readonly decimal UnitPrice;

        public PackingSlip(string sku, int units, decimal unitPrice)
        {
            Sku = sku;
            Units = units;
            UnitPrice = unitPrice;
        }
    }

    public static decimal ReadOnlyLineTotal(in PackingSlip slip)
    {
        return slip.Units * slip.UnitPrice; // in avoids copying a large struct on the stack
    }

    public static bool IsHighValueOrder(in PackingSlip slip, decimal threshold)
    {
        return ReadOnlyLineTotal(in slip) >= threshold; // in at call site when passing to another in param
    }

    /*
     * =========================================================================
     * SECTION 9: params ARRAYS
     * =========================================================================
     *
     * params lets the caller pass a variable number of arguments of one type.
     * The method receives them as an array. Rules:
     *   - Only one params parameter per method
     *   - It must be the LAST parameter
     *   - Violations → CS0231, CS0229
     *
     *   SumLineQuantities(10, 20, 6)     → int[] { 10, 20, 6 }
     *   SumLineQuantities()              → empty array (length 0)
     *   SumLineQuantities(new int[]{1})  → also valid (pass an array explicitly)
     *
     * Overload resolution prefers a fixed-parameter overload when it is a
     * better match (see CountTokens in §5 vs SumLineQuantities below).
     * -------------------------------------------------------------------------
     */
    public static int SumLineQuantities(params int[] quantities)
    {
        int sum = 0;
        foreach (int q in quantities) // zero iterations when caller passes no args
        {
            sum += q;
        }

        return sum;
    }

    /*
     * =========================================================================
     * SECTION 10: OPTIONAL PARAMETERS
     * =========================================================================
     *
     * A parameter with a default value may be omitted at the call site:
     *
     *   FormatMoney(amount)              // uses default currency
     *   FormatMoney(amount, "EUR")
     *
     * Rules:
     *   - Optional parameters must come AFTER all required ones → CS1737
     *   - Default must be a compile-time constant (literal, null, default, …)
     *   - Prefer overloads when defaults would confuse callers
     *
     * Optional parameters are baked into the CALL SITE at compile time — if you
     * change a default in a library, callers compiled against the old default
     * keep the old value until recompiled.
     * -------------------------------------------------------------------------
     */
    public static string FormatMoney(decimal amount, string currency = "USD")
    {
        return $"{currency} {amount:N2}"; // omitted second arg at call site uses compile-time default
    }

    /*
     * =========================================================================
     * SECTION 11: NAMED ARGUMENTS
     * =========================================================================
     *
     * Pass arguments by parameter name — order can change, intent is clearer:
     *
     *   BuildInvoiceLine(qty: 36, price: 49.99m, sku: "WH-4412")
     *
     * Mix positional and named: positional arguments must come first.
     *
     *   BuildInvoiceLine("WH-4412", qty: 36, price: 49.99m)  // OK
     *   BuildInvoiceLine(qty: 36, "WH-4412", price: 49.99m)  // ERROR — positional after named
     *
     * Named + optional together let you skip middle defaults without ambiguity:
     *
     *   BuildInvoiceLine(sku, qty, price, note: "rush")
     * -------------------------------------------------------------------------
     */
    public static string BuildInvoiceLine(string sku, int qty, decimal price, string note = "")
    {
        string core = $"{sku} × {qty} @ {price:C}";
        return string.IsNullOrEmpty(note) ? core : $"{core} [{note}]"; // optional note skipped when empty
    }

    /*
     * =========================================================================
     * SECTION 12: RECURSION
     * =========================================================================
     *
     * A recursive method CALLS ITSELF. Every recursive solution needs:
     *   1. BASE CASE — stops the chain (no further self-call)
     *   2. RECURSIVE CASE — moves toward the base case
     *
     * Factorial: n! = n × (n-1)! with 0! = 1 and 1! = 1.
     *
     * Each call pushes a FRAME on the call stack. Too deep → StackOverflowException.
     * Prefer loops for deep or unbounded depth; recursion shines for trees and
     * divide-and-conquer when depth is bounded.
     * -------------------------------------------------------------------------
     */
    public static long Factorial(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "n must be >= 0");
        }

        if (n <= 1)
        {
            return 1;                 // base case
        }

        return n * Factorial(n - 1);  // recursive case — each call pushes a stack frame
    }

    /*
     * =========================================================================
     * SECTION 13: STATIC METHODS (INTRO PREVIEW)
     * =========================================================================
     *
     * static methods belong to the TYPE, not an object instance. Call them on
     * the type name (or from other static methods in the same type, like Main):
     *
     *   Program.CalculateLineTotal(…)   // or just CalculateLineTotal inside Program
     *
     * They cannot use instance fields without an object reference.
     * All helpers in this chapter are static so Main can call them directly.
     *
     * COVERED IN DETAIL LATER → 02. Object Oriented Programming /
     *   04. Static Members and Static Classes
     * -------------------------------------------------------------------------
     */
    public static decimal SalesTaxPreview(decimal taxable, decimal rate) => taxable * rate;

    /*
     * =========================================================================
     * SECTION 14: LOCAL FUNCTIONS (INTRO PREVIEW)
     * =========================================================================
     *
     * A LOCAL FUNCTION is a method declared INSIDE another method. It is visible
     * only to that enclosing method and can capture its locals (closure).
     *
     * QuoteWithLocalHelper demonstrates a local function used for a one-off sum.
     *
     * COVERED IN DETAIL LATER → 08. Advanced C# Features / 05. C# 7 Features
     * -------------------------------------------------------------------------
     */
    public static decimal QuoteWithLocalHelper(decimal goods, decimal ship)
    {
        decimal Add(decimal a, decimal b) => a + b; // local function — visible only inside this method
        return Add(goods, ship);
    }

    /*
     * =========================================================================
     * SECTION 15: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Warehouse order scenario wires every method style above into one run.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        string sku = "WH-4412";
        int quantity = 36;
        decimal unitPrice = 49.99m;

        decimal lineTotal = CalculateLineTotal(quantity, unitPrice); // arguments map to parameters by position
        PrintSkuBanner(sku);                                         // void — no assignment at call site

        Console.WriteLine("--- Parameters & returns ---");
        Console.WriteLine($"Line total for {quantity} × {unitPrice:C}: {lineTotal:C}");

        decimal discountRate = 0.10m;
        decimal discountAmount = ApplyPercent(lineTotal, discountRate); // expression-bodied helper
        decimal afterDiscount = lineTotal - discountAmount;

        Console.WriteLine();
        Console.WriteLine("--- Expression-bodied ---");
        Console.WriteLine($"10% of {lineTotal:C} = {discountAmount:C}; after discount: {afterDiscount:C}");

        decimal shippingFlat = EstimateShipping(afterDiscount);           // one-arg overload
        decimal shippingWeight = EstimateShipping(afterDiscount, 4.5);    // two-arg overload — weightKg is double

        Console.WriteLine();
        Console.WriteLine("--- Overloading ---");
        Console.WriteLine($"Shipping (order amount only): {shippingFlat:C}");
        Console.WriteLine($"Shipping (amount + weight kg): {shippingWeight:C}");

        decimal rateDiscount = ApplyDiscount(afterDiscount, 0.05m);              // decimal second param
        decimal percentDiscount = ApplyDiscount(afterDiscount, 5);               // int literal → percent overload
        decimal flooredDiscount = ApplyDiscount(afterDiscount, 1.50m, floorAtZero: true); // named bool arg

        int twoTokenCount = CountTokens("A", "B");           // fixed two-parameter overload wins
        int manyTokenCount = CountTokens("X", "Y", "Z");     // params expansion — three args

        Console.WriteLine();
        Console.WriteLine("--- Overload resolution ---");
        Console.WriteLine($"ApplyDiscount(…, 0.05m) → rate path: {rateDiscount:C}");
        Console.WriteLine($"ApplyDiscount(…, 5) → int percent path: {percentDiscount:C}");
        Console.WriteLine($"ApplyDiscount(…, 1.50m, floor) → capped: {flooredDiscount:C}");
        Console.WriteLine($"CountTokens 2-arg fixed: {twoTokenCount}, params expanded: {manyTokenCount}");

        int packedUnits = quantity;
        TryBumpByValue(packedUnits, 5); // by value — copy inside method; caller unchanged
        int afterByValue = packedUnits;

        AdjustQuantity(ref packedUnits, 12); // ref at call site — must match ref in declaration

        Console.WriteLine();
        Console.WriteLine("--- By value vs ref ---");
        Console.WriteLine($"After by-value bump attempt: {afterByValue} (still original)");
        Console.WriteLine($"After ref +12: {packedUnits}");

        bool splitsCleanly = TrySplitCases(packedUnits, 12, out int fullCases, out int looseUnits); // inline out decl (C# 7)

        Console.WriteLine();
        Console.WriteLine("--- out parameters ---");
        Console.WriteLine(
            $"{packedUnits} units → {fullCases} cases + {looseUnits} loose (even split: {splitsCleanly})");

        PackingSlip slip = new PackingSlip(sku, packedUnits, unitPrice);
        decimal slipTotal = ReadOnlyLineTotal(in slip);       // in on struct — read-only alias
        bool highValue = IsHighValueOrder(in slip, 500m);

        Console.WriteLine();
        Console.WriteLine("--- in parameters ---");
        Console.WriteLine($"PackingSlip total (in, no struct copy): {slipTotal:C}; high value: {highValue}");

        int multiLineUnits = SumLineQuantities(10, 20, 6); // params — three int arguments
        int emptySum = SumLineQuantities();                // params with zero args → empty array

        Console.WriteLine();
        Console.WriteLine("--- params ---");
        Console.WriteLine($"Sum of line qtys 10+20+6: {multiLineUnits}; empty call: {emptySum}");

        string usdLabel = FormatMoney(afterDiscount);              // optional currency defaults to "USD"
        string eurLabel = FormatMoney(afterDiscount, "EUR");     // explicit second argument

        Console.WriteLine();
        Console.WriteLine("--- Optional parameters ---");
        Console.WriteLine($"Default: {usdLabel}; EUR: {eurLabel}");

        string invoiceLine = BuildInvoiceLine(qty: quantity, price: unitPrice, sku: sku); // named args — reorder OK
        string invoiceSkipNote = BuildInvoiceLine(sku, quantity, unitPrice, note: "rush"); // named optional only

        Console.WriteLine();
        Console.WriteLine("--- Named arguments ---");
        Console.WriteLine(invoiceLine);
        Console.WriteLine(invoiceSkipNote);

        int palletLevels = 5;
        long arrangements = Factorial(palletLevels); // recursion — base case stops at n <= 1

        Console.WriteLine();
        Console.WriteLine("--- Recursion ---");
        Console.WriteLine($"{palletLevels}! = {arrangements}");

        decimal previewTax = SalesTaxPreview(afterDiscount, 0.08m);              // static on Program — no instance
        decimal checkoutQuote = QuoteWithLocalHelper(afterDiscount, shippingWeight); // local function inside helper

        Console.WriteLine();
        Console.WriteLine("--- Static & local functions (preview) ---");
        Console.WriteLine($"Tax preview: {previewTax:C}; goods + shipping: {checkoutQuote:C}");

        Console.WriteLine();
        Console.WriteLine("=== Warehouse Method Pipeline ===");
        Console.WriteLine($"SKU {sku}: qty {quantity} → packed {packedUnits} ({fullCases}×12 + {looseUnits})");
        Console.WriteLine($"Line {lineTotal:C} − discount {discountAmount:C} = {afterDiscount:C}");
        Console.WriteLine($"Ship {shippingWeight:C}; tax preview {previewTax:C}; quote {checkoutQuote:C}");
        Console.WriteLine($"Multi-line units {multiLineUnits}; invoice: {invoiceLine}");
        Console.WriteLine($"Factorial demo {palletLevels}! = {arrangements}");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — METHODS
 * =========================================================================
 *
 * --- Syntax ---
 *
 *   access static ReturnType Name(Type param, …) { … return value; }
 *   void Name(…) { … }              // no return value
 *   ReturnType Name(…) => expr;     // expression-bodied
 *
 * --- Passing ---
 *
 *   default (value types)           copy — caller unchanged on reassignment
 *   ref Type x                      alias; caller must initialize; both use ref
 *   out Type x                      method must assign; caller need not initialize
 *   in Type x                       read-only alias; efficient for large structs
 *   params Type[] items             variable args; must be last parameter
 *
 * --- Call-site extras ---
 *
 *   optional: Type x = defaultValue   omit at call site (compile-time default)
 *   named:    Name(param: value)      reorder after positionals
 *   out inline (C# 7): Method(out int x)
 *
 * --- Overloading & resolution ---
 *
 *   Same name, different parameter list (count/types/modifiers). Not return type alone.
 *   Compiler: applicable candidates → pick better match (exact > conversion > params).
 *   Equally good overloads → CS0121 ambiguous call.
 *
 * --- Recursion ---
 *
 *   Base case + progress toward base. Deep recursion → StackOverflowException.
 *
 * --- Previews / later chapters ---
 *
 *   static members (deep)  → OOP / 04. Static Members and Static Classes
 *   local functions (deep) → Advanced C# / 05. C# 7 Features
 *   extension methods      → Functional Style / 04. Extension Methods
 *   async methods          → Multithreading & Async
 *   Main / args            → 03. Input & Output
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Missing return on non-void path      | CS0161
 *  ref without initializing variable    | CS0165
 *  out variable never assigned in method| CS0177
 *  Assign to in parameter               | CS8331
 *  params not last                      | CS0231
 *  Optional before required param       | CS1737
 *  Overload by return type only         | CS0111
 *  Ambiguous equally-good overloads     | CS0121
 *
 * =========================================================================
 */
