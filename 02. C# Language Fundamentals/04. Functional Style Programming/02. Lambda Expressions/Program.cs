/*
 * TOPIC: Lambda expressions — compact inline functions written with the => syntax,
 *        assigned to delegate types (custom, Func, Action) and passed as arguments.
 *
 * WHY IT MATTERS:
 *   Lambdas let you pass small pieces of behavior at the call site — filters,
 *   mappers, validators — without scattering one-off named methods. They are the
 *   default syntax for delegates in modern C# and the foundation of LINQ.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Lambda shape: parameters, =>, expression body vs block body
 *   2.  Parameter lists: zero, one, many, explicit types, discard (_)
 *   3.  Assigning lambdas to custom delegate types (from 01. Delegates)
 *   4.  Func and Action assignability (preview — full API in ch.05)
 *   5.  Method group conversion vs lambda at the call site
 *   6.  Target-typed lambdas (compiler infers from context)
 *   7.  Limitations, compile errors, and when to prefer a named method
 *   8.  Capturing outer variables (preview) and LINQ-style usage (preview)
 */

using System;
using System.Collections.Generic;

namespace LambdaExpressions;

/*
 * SECTION 5: CUSTOM DELEGATE TYPES — LAMBDA TARGETS
 *
 * A lambda has no standalone type name. It converts to a compatible delegate
 * (or, in advanced scenarios, an expression tree). These custom types mirror
 * 01. Delegates; built-in Func/Action are previewed in Section 6.
 */
public delegate decimal PriceTransform(decimal unitPrice);
public delegate decimal TaxRateProvider();
public delegate decimal LineTotalCalculator(int quantity, decimal unitPrice);
public delegate decimal LineTotalWithTaxCalculator(int quantity, decimal unitPrice);
public delegate bool PriceFilter(decimal unitPrice);
public delegate void PriceAlert(string message);

/*
 * SECTION 10: PIPELINE HELPERS — MANUAL WHERE / SELECT (LINQ PREVIEW)
 *
 * Methods that accept delegate parameters show how lambdas plug into APIs before
 * LINQ syntax. COVERED IN DETAIL LATER → 05. Language Integrated Query
 */
public static class PricePipeline
{
    public static decimal[] ApplyToAll(decimal[] prices, PriceTransform transform)
    {
        decimal[] result = new decimal[prices.Length];
        for (int i = 0; i < prices.Length; i++)
        {
            result[i] = transform(prices[i]); // invoke whatever lambda or method was passed
        }

        return result;
    }

    public static decimal[] ApplyToAll(IReadOnlyList<decimal> prices, PriceTransform transform)
    {
        decimal[] result = new decimal[prices.Count];
        for (int i = 0; i < prices.Count; i++)
        {
            result[i] = transform(prices[i]);
        }

        return result;
    }

    public static decimal[] FilterPrices(decimal[] prices, PriceFilter predicate)
    {
        int matchCount = 0;
        for (int i = 0; i < prices.Length; i++)
        {
            if (predicate(prices[i]))
            {
                matchCount++;
            }
        }

        decimal[] result = new decimal[matchCount];
        int index = 0;
        for (int i = 0; i < prices.Length; i++)
        {
            if (predicate(prices[i]))
            {
                result[index++] = prices[i];
            }
        }

        return result;
    }

    public static decimal[] FilterPrices(IReadOnlyList<decimal> prices, PriceFilter predicate)
    {
        int matchCount = 0;
        for (int i = 0; i < prices.Count; i++)
        {
            if (predicate(prices[i]))
            {
                matchCount++;
            }
        }

        decimal[] result = new decimal[matchCount];
        int index = 0;
        for (int i = 0; i < prices.Count; i++)
        {
            if (predicate(prices[i]))
            {
                result[index++] = prices[i];
            }
        }

        return result;
    }

    public static string[] FormatPrices(IReadOnlyList<decimal> prices)
    {
        string[] formatted = new string[prices.Count];
        for (int i = 0; i < prices.Count; i++)
        {
            formatted[i] = prices[i].ToString("C");
        }

        return formatted;
    }

    public static decimal RoundToNearestDollar(decimal amount)
    {
        return Math.Round(amount, 0, MidpointRounding.AwayFromZero);
    }
}

public class Program
{
    /* Main orchestrates the chapter demo — calls each section helper in order. */
    public static void Main(string[] args)
    {
        decimal[] unitPrices = { 49.99m, 12.50m, 8.75m, 129.00m };

        Console.WriteLine("=== 02. Lambda Expressions — Pricing Pipeline ===");
        Console.WriteLine();

        DemonstrateBasicSyntax(unitPrices);
        DemonstrateParameterForms();
        DemonstrateExpressionVsStatement();
        DemonstrateCustomDelegates();
        DemonstrateFuncActionPreview();
        DemonstrateMethodGroupVsLambda();
        DemonstrateTargetTypedLambdas(unitPrices);
        DemonstrateLimitations();
        DemonstratePipeline(unitPrices);
        DemonstrateCapturingPreview();
        DemonstrateLinqPreview(unitPrices);
    }

    /*
     * SECTION 1: WHAT IS A LAMBDA?
     *
     * A lambda expression is an anonymous function — logic without a method name —
     * written inline where you need it.
     *
     *   (parameters) => expression-or-block
     *
     * The => token is read "goes to." The compiler generates a private method (or
     * closure) matching a delegate signature. Lambdas replaced anonymous methods
     * (delegate { }) in modern code; see 03. Anonymous Methods for the legacy form.
     *
     * You still need a delegate TYPE — variable, parameter, or field — to store
     * or pass the lambda unless the compiler can infer the target (Section 8).
     */
    private static void DemonstrateBasicSyntax(decimal[] unitPrices)
    {
        /*
         * SECTION 2: BASIC LAMBDA SYNTAX — x => x * 2
         *
         * Single-parameter expression lambda: input on the left, => , result expression on the right.
         *
         *   x => x * 2m
         *   (x) => x * 2m          // parentheses optional for one parameter
         *
         * The lambda must match the delegate. PriceTransform is decimal → decimal.
         * Use the m suffix on decimal literals — x * 2 (int) would not match.
         */
        PriceTransform doublePrice = x => x * 2m;
        decimal doubled = doublePrice(49.99m);

        decimal[] doubledPrices = PricePipeline.ApplyToAll(unitPrices, x => x * 2m);

        Console.WriteLine("--- Section 1–2: Basic syntax ---");
        Console.WriteLine($"doublePrice(49.99) = {doubled:F2}");
        Console.WriteLine($"All prices doubled: {string.Join(", ", PricePipeline.FormatPrices(doubledPrices))}");
        Console.WriteLine();
    }

    /*
     * SECTION 3: PARAMETER LIST FORMS
     *
     *  Form                      | Example                         | Notes
     *  --------------------------|---------------------------------|---------------------------
     *  Zero parameters           | () => 0.0825m                   | Parentheses required
     *  One parameter             | x => x * 1.08m                  | Parens optional
     *  Multiple parameters       | (qty, price) => qty * price     | Parens required
     *  Explicit parameter types  | (int qty, decimal price) => …   | When inference fails
     *  Discard (unused param)    | (_, price) => price * 2m        | _ ignores that slot
     *
     * Parameter names are local to the lambda; they can shadow outer names only
     * inside the lambda body.
     */
    private static void DemonstrateParameterForms()
    {
        TaxRateProvider salesTaxRate = () => 0.0825m;
        decimal rate = salesTaxRate();

        LineTotalCalculator lineTotal = (qty, price) => qty * price;
        decimal line = lineTotal(4, 12.50m);

        LineTotalCalculator explicitTypes = (int qty, decimal price) => qty * price;
        decimal lineExplicit = explicitTypes(4, 12.50m);

        PriceFilter alwaysPass = (_) => true;
        LineTotalCalculator flatPerUnit = (qty, _) => qty * 10m;

        Console.WriteLine("--- Section 3: Parameter forms ---");
        Console.WriteLine($"Sales tax rate (no params): {rate:P2}");
        Console.WriteLine($"Line total 4 × 12.50 = {line:F2}");
        Console.WriteLine($"Explicit types same result: {lineExplicit:F2}");
        Console.WriteLine($"Discard filter (always true): {alwaysPass(999m)}");
        Console.WriteLine($"Flat $10/unit (ignore price param): {flatPerUnit(3, 49.99m):F2}");
        Console.WriteLine();
    }

    /*
     * SECTION 4: EXPRESSION LAMBDA vs STATEMENT LAMBDA (BLOCK BODY)
     *
     * --- 4a. Expression lambda ---
     * One expression only; no braces; return value is implicit. Works when the
     * delegate returns a value (non-void).
     *
     *   x => x * (1m + rate)
     *
     * --- 4b. Statement lambda (block body) ---
     * Use { } for multiple statements, locals, or control flow. Non-void delegates
     * must use return explicitly.
     *
     *   (qty, price) => { decimal sub = qty * price; return sub + tax; }
     *
     * --- 4c. void-returning block ---
     * Omit value return; use bare return; to exit early.
     *
     * Expression form for one-liners; block form when logic needs steps — same
     * guidance as expression-bodied methods elsewhere in the curriculum.
     */
    private static void DemonstrateExpressionVsStatement()
    {
        TaxRateProvider salesTaxRate = () => 0.0825m;

        PriceTransform withTaxExpression = x => x * (1m + salesTaxRate());

        LineTotalWithTaxCalculator withTaxBlock = (qty, price) =>
        {
            decimal subtotal = qty * price;
            decimal tax = subtotal * salesTaxRate();
            return subtotal + tax;
        };

        PriceAlert logAlert = msg => Console.WriteLine($"[ALERT] {msg}");

        PriceAlert logSkuOrSkip = sku =>
        {
            if (string.IsNullOrEmpty(sku))
            {
                return;
            }

            Console.WriteLine($"[SKU] Processing {sku}");
        };

        decimal taxedUnit = withTaxExpression(100m);
        decimal orderWithTax = withTaxBlock(2, 49.99m);

        Console.WriteLine("--- Section 4: Expression vs statement ---");
        Console.WriteLine($"Expression lambda $100 + tax: {taxedUnit:F2}");
        Console.WriteLine($"Statement lambda 2 × 49.99 + tax: {orderWithTax:F2}");
        logAlert("Bulk discount threshold reached");
        logSkuOrSkip("");
        logSkuOrSkip("WH-4412");
        Console.WriteLine();
    }

    /*
     * SECTION 5: LAMBDAS AND CUSTOM DELEGATE TYPES
     *
     * Any lambda whose signature matches a declared delegate can be assigned:
     *
     *   PriceTransform markup10 = price => price * 1.10m;
     *
     * The delegate types at namespace scope (above) are the assignment targets.
     */
    private static void DemonstrateCustomDelegates()
    {
        PriceTransform markup10 = price => price * 1.10m;
        PriceTransform markup25 = price => price * 1.25m;

        decimal basePrice = 19.99m;
        Console.WriteLine("--- Section 5: Custom delegates ---");
        Console.WriteLine($"Base {basePrice:C} + 10%: {markup10(basePrice):C}");
        Console.WriteLine($"Base {basePrice:C} + 25%: {markup25(basePrice):C}");
        Console.WriteLine();
    }

    /*
     * SECTION 6: FUNC AND ACTION — ASSIGNABILITY PREVIEW
     *
     * Lambdas assign equally to built-in generic delegates in System:
     *
     *   Func<decimal, decimal>   — last type param is return type
     *   Action<string>           — void return
     *   Predicate<decimal>       — bool filter (same as Func<T, bool>)
     *
     * COVERED IN DETAIL LATER → 05. Func Action and Predicate
     *   (headline: arity up to 16, multicast, List.FindAll, Array.ForEach)
     */
    private static void DemonstrateFuncActionPreview()
    {
        Func<decimal, decimal> doublePrice = x => x * 2m;
        Action<string> notify = msg => Console.WriteLine($"[NOTIFY] {msg}");
        Predicate<decimal> isPremium = p => p >= 100m;

        Console.WriteLine("--- Section 6: Func / Action preview ---");
        Console.WriteLine($"Func double 49.99 → {doublePrice(49.99m):F2}");
        notify("Func/Action accept lambdas the same way custom delegates do");
        Console.WriteLine($"Predicate isPremium(129.00): {isPremium(129.00m)}");
        Console.WriteLine();
    }

    /*
     * SECTION 7: METHOD GROUP vs LAMBDA
     *
     * A compatible named method converts to a delegate without => :
     *
     *   PriceTransform round = PricePipeline.RoundToNearestDollar;   // method group
     *
     * Use a lambda when logic is small and local to one call site. Use a named
     * method (or method group) when reused, tested separately, or complex enough
     * to deserve a name.
     */
    private static void DemonstrateMethodGroupVsLambda()
    {
        PriceTransform roundToDollar = PricePipeline.RoundToNearestDollar;
        PriceTransform roundViaLambda = amount => PricePipeline.RoundToNearestDollar(amount);

        Console.WriteLine("--- Section 7: Method group vs lambda ---");
        Console.WriteLine($"Method group: {roundToDollar(19.49m):C}");
        Console.WriteLine($"Equivalent lambda: {roundViaLambda(19.49m):C}");
        Console.WriteLine();
    }

    /*
     * SECTION 8: TARGET-TYPED LAMBDAS
     *
     * When the compiler knows the delegate type from context, it infers the lambda
     * target without an explicit cast:
     *
     *   PriceTransform t = x => x * 1.05m;              // variable type
     *   ApplyToAll(prices, p => p + 1m);                // parameter type
     *
     * Before C# 10, some APIs required (PriceTransform)(x => …) or a local variable
     * when inference failed. Prefer letting the parameter or variable type be the target.
     */
    private static void DemonstrateTargetTypedLambdas(decimal[] unitPrices)
    {
        PriceTransform bump = x => x + 1m;
        decimal[] bumped = PricePipeline.ApplyToAll(unitPrices, p => p + 1m);

        Console.WriteLine("--- Section 8: Target-typed lambdas ---");
        Console.WriteLine($"Variable target: bump(9.99m) = {bump(9.99m):F2}");
        Console.WriteLine($"Parameter target: {string.Join(", ", PricePipeline.FormatPrices(bumped))}");
        Console.WriteLine();
    }

    /*
     * SECTION 9: LIMITATIONS AND COMPILE ERRORS
     *
     *  Rule / mistake                         | Result
     *  ---------------------------------------|------------------------------------------
     *  Block body with one expression only    | Use expression form or add return
     *  Expression lambda uses { }             | CS0834 — not a single expression
     *  Non-void delegate, block without return| CS0161 — not all paths return
     *  Return type mismatch (2 vs 2m)         | CS1662 — lambda return doesn't match
     *  No target type for standalone lambda   | CS8917 — cannot infer delegate type
     *  Ambiguous overload                     | Add explicit parameter types or cast
     *
     * --- Design limits ---
     *   • Lambdas are anonymous — no direct recursion by name (use a local function instead).
     *   • static lambda (C# 9+): cannot capture outer locals — see 06. Closures for capture rules.
     *   • Expression lambdas cannot contain statements (no var + if chains in one expression).
     *   • Prefer named methods for large or reused logic; lambdas excel at short, local behavior.
     *
     * The demo below uses only valid patterns; the table documents what fails at compile time.
     */
    private static void DemonstrateLimitations()
    {
        PriceTransform validExpression = x => x * 1.08m;

        PriceTransform validStatement = x =>
        {
            if (x < 0m)
            {
                return 0m;
            }

            return x * 1.08m;
        };

        Console.WriteLine("--- Section 9: Limitations (valid patterns shown) ---");
        Console.WriteLine($"Expression: {validExpression(50m):F2}");
        Console.WriteLine($"Statement with guard: {validStatement(50m):F2}");
        Console.WriteLine("See section comment for CS0834, CS0161, CS1662, CS8917.");
        Console.WriteLine();
    }

    private static void DemonstratePipeline(decimal[] unitPrices)
    {
        decimal[] promoResult = PricePipeline.ApplyToAll(
            PricePipeline.FilterPrices(unitPrices, p => p >= 10m),
            p => p * 0.90m);

        Console.WriteLine("--- Section 10: Pipeline ---");
        Console.WriteLine($"Original:  {string.Join(", ", PricePipeline.FormatPrices(unitPrices))}");
        Console.WriteLine($"Promo (≥ $10, then 10% off): {string.Join(", ", PricePipeline.FormatPrices(promoResult))}");
        Console.WriteLine();
    }

    /*
     * SECTION 11: CAPTURING OUTER VARIABLES — PREVIEW
     *
     * A lambda can read locals, parameters, and fields from enclosing scopes. The
     * compiler captures them so the lambda sees current values when invoked later.
     *
     *   decimal discountRate = 0.15m;
     *   PriceTransform apply = price => price * (1m - discountRate);
     *
     * COVERED IN DETAIL LATER → 06. Closures
     *   (headline: lifetime, foreach pitfalls, mutating captures, loop sharing)
     */
    private static void DemonstrateCapturingPreview()
    {
        decimal discountRate = 0.15m;
        PriceTransform applyDiscount = price => price * (1m - discountRate);

        decimal previewPrice = 100m;
        Console.WriteLine("--- Section 11: Capturing (preview) ---");
        Console.WriteLine($"At {discountRate:P0}: {applyDiscount(previewPrice):C}");

        discountRate = 0.25m;
        Console.WriteLine($"After rate → {discountRate:P0}: {applyDiscount(previewPrice):C} (same lambda, updated capture)");
        Console.WriteLine();
    }

    /*
     * SECTION 11b: LINQ — PREVIEW
     *
     * LINQ extension methods take lambdas as arguments:
     *
     *   prices.Where(p => p > 20m)
     *   prices.Select(p => p * 1.08m)
     *
     * COVERED IN DETAIL LATER → 05. Language Integrated Query
     */
    private static void DemonstrateLinqPreview(decimal[] unitPrices)
    {
        List<decimal> catalog = new List<decimal>(unitPrices);

        decimal[] highValueAdjusted = PricePipeline.ApplyToAll(
            PricePipeline.FilterPrices(catalog, p => p > 20m),
            p => p - 5m);

        Console.WriteLine("--- Section 12: LINQ-style preview ---");
        Console.WriteLine($"Catalog: {string.Join(", ", PricePipeline.FormatPrices(catalog))}");
        Console.WriteLine($"Where price > 20, Select price - 5: {string.Join(", ", PricePipeline.FormatPrices(highValueAdjusted))}");
        Console.WriteLine();
        Console.WriteLine("Full LINQ → 05. Language Integrated Query / 01. Introduction to LINQ");
    }
}

/*
 * QUICK REFERENCE — LAMBDA EXPRESSIONS
 *
 * --- Syntax ---
 *   (params) => expression              expression lambda (implicit return)
 *   (params) => { statements; }         statement lambda (block body)
 *   x => x * 2m                         one param — parens optional
 *   () => value                         zero params — parens required
 *   (a, b) => a + b                     multiple params — parens required
 *   (int x) => x * 2                    explicit types when inference fails
 *   (_, price) => price * 2m            discard for unused parameters
 *
 * --- Expression vs block ---
 *   Expression   | Single expression; no return keyword; must match delegate return type
 *   Block        | { }; explicit return for non-void; void delegates use return; only
 *
 * --- Assignment targets ---
 *   Custom delegate   MyTransform t = x => x * 2m;
 *   Method group      MyTransform t = NamedMethod;
 *   Func / Action     Func<int,int> f = x => x * x;   (full API → ch.05)
 *   Target-typed      Pass to parameter or assign to typed variable — no cast needed
 *
 * --- Common compile errors ---
 *   CS0834  statement in expression lambda
 *   CS0161  not all code paths return (non-void block)
 *   CS1662  lambda return type mismatch
 *   CS8917  no target type for lambda
 *
 * --- Later chapters ---
 *   Closures & capture depth  → 06. Closures
 *   Func, Action, Predicate   → 05. Func Action and Predicate
 *   Anonymous methods legacy  → 03. Anonymous Methods
 *   LINQ operators            → 05. Language Integrated Query
 */
