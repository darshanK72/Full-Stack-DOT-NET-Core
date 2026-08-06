/*
 * =============================================================================
 * 06. METHODS IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Methods (functions) — named blocks of code you call by name, with
 *        parameters, return values, overloading, ref/out, params, optional
 *        and named arguments, and recursion.
 *
 * WHY IT MATTERS:
 *   Real programs repeat the same calculations and rules in many places.
 *   Methods package that logic once, name it clearly, and return results —
 *   so pricing, validation, and reports stay consistent and testable instead
 *   of copy-pasted blocks that drift apart.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Method syntax (access, static, return type, name, body)
 *   2.  Parameters and return values (including void)
 *   3.  Expression-bodied methods
 *   4.  Method overloading
 *   5.  Call by value vs ref
 *   6.  out parameters (and C# 7 inline declaration)
 *   7.  params arrays
 *   8.  Optional parameters
 *   9.  Named arguments
 *  10.  Recursion (base case and stack overflow risk)
 *  11.  Static methods (intro preview)
 *  12.  Local functions (intro preview)
 *
 * =============================================================================
 */

using System;

namespace Methods;

public class Program
{
    public static void Main(string[] args)
    {
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
         *   access  modifiers  returnType  Name(parameter list)
         *   {
         *       // body
         *       return value;   // if returnType is not void
         *   }
         *
         *  Part            | Role
         *  ----------------|------------------------------------------------------
         *  access          | Who can call it (public, private, …) — OOP depth later
         *  static          | Belongs to the type, not an instance (preview below)
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

        string sku = "WH-4412";
        int quantity = 36;
        decimal unitPrice = 49.99m;


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
         * (or throw). The caller stores or uses that result.
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
         * -------------------------------------------------------------------------
         */

        decimal lineTotal = CalculateLineTotal(quantity, unitPrice);
        PrintSkuBanner(sku);

        Console.WriteLine("--- Parameters & returns ---");
        Console.WriteLine($"Line total for {quantity} × {unitPrice:C}: {lineTotal:C}");


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

        decimal discountRate = 0.10m;
        decimal discountAmount = ApplyPercent(lineTotal, discountRate);
        decimal afterDiscount = lineTotal - discountAmount;

        Console.WriteLine();
        Console.WriteLine("--- Expression-bodied ---");
        Console.WriteLine($"10% of {lineTotal:C} = {discountAmount:C}; after discount: {afterDiscount:C}");


        /*
         * =========================================================================
         * SECTION 4: METHOD OVERLOADING
         * =========================================================================
         *
         * OVERLOADING = same method name, different PARAMETER LISTS (signature).
         * The compiler picks the best match from the arguments you pass.
         *
         * What counts as a different signature:
         *   - Different number of parameters
         *   - Different parameter types (in order)
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

        decimal shippingFlat = EstimateShipping(afterDiscount);           // overload 1
        decimal shippingWeight = EstimateShipping(afterDiscount, 4.5);    // overload 2

        Console.WriteLine();
        Console.WriteLine("--- Overloading ---");
        Console.WriteLine($"Shipping (order amount only): {shippingFlat:C}");
        Console.WriteLine($"Shipping (amount + weight kg): {shippingWeight:C}");


        /*
         * =========================================================================
         * SECTION 5: CALL BY VALUE VS ref
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
         * Use ref when a method must update an existing variable in place.
         * Prefer returning a new value when a single result is enough — clearer.
         * -------------------------------------------------------------------------
         */

        int packedUnits = quantity;
        TryBumpByValue(packedUnits, 5);          // copy — caller's packedUnits unchanged
        int afterByValue = packedUnits;

        AdjustQuantity(ref packedUnits, 12);     // alias — packedUnits becomes 48

        Console.WriteLine();
        Console.WriteLine("--- By value vs ref ---");
        Console.WriteLine($"After by-value bump attempt: {afterByValue} (still original)");
        Console.WriteLine($"After ref +12: {packedUnits}");


        /*
         * =========================================================================
         * SECTION 6: out PARAMETERS
         * =========================================================================
         *
         * out is like ref but for OUTPUT: the method MUST assign the parameter
         * before returning. The caller does not need to initialize beforehand.
         *
         * Classic pattern (TryParse style):
         *
         *   bool ok = TrySplitCases(packedUnits, 12, out int fullCases, out int loose);
         *
         * --- 6a. C# 7 inline out declaration ---
         *
         * You may declare the out variable at the call site (shown above). Older
         * style declared variables first, then passed them.
         *
         *   int fullCases; int loose;
         *   TrySplitCases(packedUnits, 12, out fullCases, out loose);
         *
         * COVERED IN DETAIL LATER → 08. Advanced C# Features / 05. C# 7 Features
         *   (out variables, tuples, and related C# 7 enhancements)
         * -------------------------------------------------------------------------
         */

        bool splitsCleanly = TrySplitCases(packedUnits, 12, out int fullCases, out int looseUnits);

        Console.WriteLine();
        Console.WriteLine("--- out parameters ---");
        Console.WriteLine(
            $"{packedUnits} units → {fullCases} cases + {looseUnits} loose (even split: {splitsCleanly})");


        /*
         * =========================================================================
         * SECTION 7: params ARRAYS
         * =========================================================================
         *
         * params lets the caller pass a variable number of arguments of one type.
         * The method receives them as an array. Rules:
         *   - Only one params parameter per method
         *   - It must be the LAST parameter
         *
         *   SumLineQuantities(10, 20, 6)     → int[] { 10, 20, 6 }
         *   SumLineQuantities()              → empty array (length 0)
         *   SumLineQuantities(new int[]{1})  → also valid (pass an array)
         * -------------------------------------------------------------------------
         */

        int multiLineUnits = SumLineQuantities(10, 20, 6);
        int emptySum = SumLineQuantities();

        Console.WriteLine();
        Console.WriteLine("--- params ---");
        Console.WriteLine($"Sum of line qtys 10+20+6: {multiLineUnits}; empty call: {emptySum}");


        /*
         * =========================================================================
         * SECTION 8: OPTIONAL PARAMETERS
         * =========================================================================
         *
         * A parameter with a default value may be omitted at the call site:
         *
         *   FormatMoney(amount)              // uses default currency symbol path
         *   FormatMoney(amount, "EUR")
         *
         * Rules:
         *   - Optional parameters must come AFTER all required ones
         *   - Default must be a compile-time constant (literal, null, default, …)
         *   - Prefer overloads when defaults would confuse callers
         * -------------------------------------------------------------------------
         */

        string usdLabel = FormatMoney(afterDiscount);
        string eurLabel = FormatMoney(afterDiscount, "EUR");

        Console.WriteLine();
        Console.WriteLine("--- Optional parameters ---");
        Console.WriteLine($"Default: {usdLabel}; EUR: {eurLabel}");


        /*
         * =========================================================================
         * SECTION 9: NAMED ARGUMENTS
         * =========================================================================
         *
         * Pass arguments by parameter name — order can change, intent is clearer:
         *
         *   BuildInvoiceLine(qty: 36, price: 49.99m, sku: "WH-4412")
         *
         * Mix positional and named: positional arguments must come first.
         *
         *   BuildInvoiceLine("WH-4412", qty: 36, price: 49.99m)  // OK
         *   BuildInvoiceLine(qty: 36, "WH-4412", price: 49.99m)  // ERROR
         *
         * Named + optional together let you skip middle defaults without ambiguity.
         * -------------------------------------------------------------------------
         */

        string invoiceLine = BuildInvoiceLine(sku: sku, qty: quantity, price: unitPrice);
        string invoiceSkipNote = BuildInvoiceLine(sku, quantity, unitPrice, note: "rush");

        Console.WriteLine();
        Console.WriteLine("--- Named arguments ---");
        Console.WriteLine(invoiceLine);
        Console.WriteLine(invoiceSkipNote);


        /*
         * =========================================================================
         * SECTION 10: RECURSION
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

        int palletLevels = 5;
        long arrangements = Factorial(palletLevels);

        Console.WriteLine();
        Console.WriteLine("--- Recursion ---");
        Console.WriteLine($"{palletLevels}! = {arrangements}");


        /*
         * =========================================================================
         * SECTION 11: STATIC METHODS (INTRO PREVIEW)
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
         *   (static fields, static constructors, static classes)
         * -------------------------------------------------------------------------
         */

        decimal previewTax = SalesTaxPreview(afterDiscount, 0.08m);

        Console.WriteLine();
        Console.WriteLine("--- Static (preview) ---");
        Console.WriteLine($"Tax preview on discounted total: {previewTax:C}");


        /*
         * =========================================================================
         * SECTION 12: LOCAL FUNCTIONS (INTRO PREVIEW)
         * =========================================================================
         *
         * A LOCAL FUNCTION is a method declared INSIDE another method. It is visible
         * only to that enclosing method and can use its locals (closures).
         *
         * Useful for small helpers that should not pollute the class API.
         *
         * COVERED IN DETAIL LATER → 08. Advanced C# Features / 05. C# 7 Features
         * -------------------------------------------------------------------------
         */

        decimal QuoteWithLocalHelper(decimal goods, decimal ship)
        {
            decimal Add(decimal a, decimal b) => a + b;   // local function
            return Add(goods, ship);
        }

        decimal checkoutQuote = QuoteWithLocalHelper(afterDiscount, shippingWeight);

        Console.WriteLine();
        Console.WriteLine("--- Local functions (preview) ---");
        Console.WriteLine($"Goods + shipping via local Add: {checkoutQuote:C}");


        /*
         * =========================================================================
         * SECTION 13: ORDER PIPELINE SUMMARY
         * =========================================================================
         *
         * One cohesive path that used every style of method from above.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine();
        Console.WriteLine("=== Warehouse Method Pipeline ===");
        Console.WriteLine($"SKU {sku}: qty {quantity} → packed {packedUnits} ({fullCases}×12 + {looseUnits})");
        Console.WriteLine($"Line {lineTotal:C} − discount {discountAmount:C} = {afterDiscount:C}");
        Console.WriteLine($"Ship {shippingWeight:C}; tax preview {previewTax:C}; quote {checkoutQuote:C}");
        Console.WriteLine($"Multi-line units {multiLineUnits}; invoice: {invoiceLine}");
        Console.WriteLine($"Factorial demo {palletLevels}! = {arrangements}");
    }

    /*
     * -------------------------------------------------------------------------
     * Helper methods — same class as Main (static Phase-1 style)
     * -------------------------------------------------------------------------
     */

    public static decimal CalculateLineTotal(int qty, decimal price)
    {
        return qty * price;
    }

    public static void PrintSkuBanner(string skuCode)
    {
        Console.WriteLine($"SKU: {skuCode}");
    }

    public static decimal ApplyPercent(decimal amount, decimal rate) => amount * rate;

    public static decimal EstimateShipping(decimal orderAmount)
    {
        // Flat rule: free over 500, else 12.50
        return orderAmount >= 500m ? 0m : 12.50m;
    }

    public static decimal EstimateShipping(decimal orderAmount, double weightKg)
    {
        decimal baseFee = EstimateShipping(orderAmount);
        decimal weightFee = (decimal)weightKg * 1.25m;
        return baseFee + weightFee;
    }

    public static void TryBumpByValue(int units, int extra)
    {
        units += extra;   // only the copy changes
    }

    public static void AdjustQuantity(ref int units, int extra)
    {
        units += extra;
    }

    public static bool TrySplitCases(int units, int unitsPerCase, out int cases, out int remainder)
    {
        cases = units / unitsPerCase;
        remainder = units % unitsPerCase;
        return remainder == 0;
    }

    public static int SumLineQuantities(params int[] quantities)
    {
        int sum = 0;
        foreach (int q in quantities)
        {
            sum += q;
        }

        return sum;
    }

    public static string FormatMoney(decimal amount, string currency = "USD")
    {
        return $"{currency} {amount:N2}";
    }

    public static string BuildInvoiceLine(string sku, int qty, decimal price, string note = "")
    {
        string core = $"{sku} × {qty} @ {price:C}";
        return string.IsNullOrEmpty(note) ? core : $"{core} [{note}]";
    }

    public static long Factorial(int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "n must be >= 0");
        }

        if (n <= 1)                 // base case
        {
            return 1;
        }

        return n * Factorial(n - 1); // recursive case
    }

    public static decimal SalesTaxPreview(decimal taxable, decimal rate) => taxable * rate;
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
 *   default (value types)           copy — caller unchanged
 *   ref Type x                      alias; caller must initialize; both use ref
 *   out Type x                      method must assign; caller need not initialize
 *   params Type[] items             variable args; must be last parameter
 *
 * --- Call-site extras ---
 *
 *   optional: Type x = defaultValue   omit at call site
 *   named:    Name(param: value)      any order after positionals
 *   out inline (C# 7): Method(out int x)
 *
 * --- Overloading ---
 *
 *   Same name, different parameter list (count/types). Not return type alone.
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
 *  params not last                      | CS0231
 *  Optional before required param       | CS1737
 *  Overload by return type only         | CS0111
 *
 * =========================================================================
 */
