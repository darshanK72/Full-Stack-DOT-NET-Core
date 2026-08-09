/*
 * =============================================================================
 * 05. FUNC, ACTION, AND PREDICATE — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Built-in generic delegate types — Func (returns a value), Action
 *        (returns void), and Predicate (returns bool). How they replace
 *        repetitive custom delegate declarations and wire into BCL APIs that
 *        accept callbacks, filters, and transformations.
 *
 * WHY IT MATTERS:
 *   Before generic delegates, every callback shape needed its own
 *   `delegate` keyword and type name. Func, Action, and Predicate are
 *   pre-declared templates in System — you supply type arguments and a
 *   method or lambda. List<T>.FindAll, Array.ForEach, and (later) LINQ
 *   operators take these types constantly.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Func<TResult> — parameterless function returning TResult
 *   2.  Func<T, TResult> and multi-parameter Func (up to 16 inputs)
 *   3.  Action and Action<T> — void-returning callbacks
 *   4.  Predicate<T> vs Func<T, bool> — legacy filter type and modern equivalent
 *   5.  Invoke patterns — shorthand call, .Invoke(), multicast preview
 *   6.  Null checks before invoke — NullReferenceException and ?.Invoke
 *   7.  Common BCL usage — List.FindAll, Array.ForEach, TrueForAll, RemoveAll
 *   8.  Built-in generic delegates vs custom delegate types (01. Delegates)
 *   9.  Preview — LINQ operators and Func/Predicate (module 05)
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;

namespace FuncActionAndPredicate;

/*
 * =========================================================================
 * SECTION 1: BUILT-IN GENERIC DELEGATES — OVERVIEW
 * =========================================================================
 *
 * In 01. Delegates you declared types like:
 *
 *   public delegate int MyDelegate(int x);
 *
 * The BCL ships ready-made generic delegate declarations so you rarely
 * need your own for common shapes:
 *
 *  Type            | Return   | Parameters        | Typical use
 *  ----------------|----------|-------------------|---------------------------
 *  Func<…>         | TResult  | 0–16 typed inputs | Compute / transform
 *  Action<…>       | void     | 0–16 typed inputs | Side effect / notify
 *  Predicate<T>    | bool     | exactly 1 (T)     | Filter / test condition
 *
 * All three live in System. They are reference types, support multicast
 * (+, -=), and accept compatible method groups or lambdas (02. Lambda
 * Expressions). Scenario below: warehouse SKU catalog — filter stock,
 * compute line totals, print pick-list lines.
 * -------------------------------------------------------------------------
 */
public readonly record struct Product(
    string Sku,
    string Name,
    decimal UnitPrice,
    int StockQty,
    bool IsActive);

/*
 * =========================================================================
 * SECTION 8: CUSTOM DELEGATE VS Func — NAMED DOMAIN TYPE
 * =========================================================================
 *
 * Custom delegate (from 01. Delegates):
 *
 *   public delegate decimal PriceCalculator(Product p, int quantity);
 *
 * Built-in equivalent:
 *
 *   Func<Product, int, decimal> priceCalculator = CalculateLineTotal;
 *
 * Both compile to the same IL pattern. Prefer a named delegate when the
 * NAME is part of a public contract; prefer Func/Action/Predicate for
 * locals and framework parameters.
 * -------------------------------------------------------------------------
 */
public delegate decimal LineTotalCalculator(Product product, int quantity);

public class Program
{
    /*
     * =========================================================================
     * SECTION 2: Func<TResult> — ZERO PARAMETERS, RETURNS A VALUE
     * =========================================================================
     *
     * Func<TResult> — method with NO arguments that returns TResult.
     *
     *   Func<decimal> getTaxRate = GetRegionalTaxRate;
     *   decimal rate = getTaxRate();              // invoke
     *
     * The LAST type parameter is ALWAYS the return type. With zero inputs,
     * Func has exactly one type argument.
     *
     * Common uses: lazy factories, configuration readers, testable time providers.
     * -------------------------------------------------------------------------
     */
    public static decimal GetRegionalTaxRate() => 0.0825m;

    /*
     * =========================================================================
     * SECTION 3: Func<T, TResult> AND MULTI-PARAMETER Func
     * =========================================================================
     *
     * Func<T, TResult> — one input, one return:
     *
     *   Func<Product, decimal> priceWithTax = ApplyTax;
     *
     * Two or more inputs — add type parameters BEFORE the return type:
     *
     *   Func<T1, T2, …, T16, TResult>
     *                              ^^^^^^ always last = return type
     *
     * .NET defines overloads from Func<TResult> through
     * Func<T1,…,T16,TResult>. You cannot use Func for void — use Action.
     * -------------------------------------------------------------------------
     */
    public static decimal CalculateLineTotal(Product product, int quantity) =>
        product.UnitPrice * quantity;

    public static decimal ApplyVolumeDiscount(decimal unitPrice, int quantity, decimal discountRate) =>
        unitPrice * quantity * (1m - discountRate);

    public static bool IsActiveInStock(Product product) =>
        product.IsActive && product.StockQty > 0;

    public static int Square(int x) => x * x;

    /*
     * =========================================================================
     * SECTION 5: INVOKE AND CALL PATTERNS
     * =========================================================================
     *
     * Three equivalent ways to call a delegate (same IL for single-target):
     *
     *   result = func(arg);           // shorthand — most common
     *   result = func.Invoke(arg);      // explicit — same behavior
     *
     * Multicast (from 01. Delegates): += adds targets; one invoke runs all:
     *
     *   Action<string> chain = LogInfo;
     *   chain += LogAudit;
     *   chain("event");               // both methods run
     *
     * Inspect combined targets:
     *
     *   Delegate[] targets = chain.GetInvocationList();
     * -------------------------------------------------------------------------
     */
    public static void LogInfo(string message) =>
        Console.WriteLine($"  [INFO]  {message}");

    public static void LogAudit(string message) =>
        Console.WriteLine($"  [AUDIT] {message}");

    /*
     * =========================================================================
     * SECTION 6: NULL CHECKS BEFORE INVOKE
     * =========================================================================
     *
     * A delegate variable defaults to null. Calling it throws
     * NullReferenceException — same as any null reference call.
     *
     * Safe patterns:
     *
     *   handler?.Invoke(message);           // skip if null
     *   if (handler != null) handler(msg);  // explicit guard
     *   handler ??= DefaultHandler;           // assign fallback once
     *
     * Optional callbacks in APIs should always be invoked with ?.
     * -------------------------------------------------------------------------
     */
    public static void SafeNotify(Action<string>? handler, string message)
    {
        handler?.Invoke(message); // null-conditional — no throw when handler is absent
    }

    public static void DefaultPickLine(string line) =>
        Console.WriteLine($"  (default) {line}");

    /*
     * =========================================================================
     * SECTION 7: COMMON BCL APIs THAT TAKE Func / Action / Predicate
     * =========================================================================
     *
     * Collections predate LINQ but already used these delegate types:
     *
     *  API                         | Delegate type        | Returns
     *  ----------------------------|----------------------|------------------
     *  List<T>.FindAll             | Predicate<T>         | List<T>
     *  List<T>.TrueForAll          | Predicate<T>         | bool
     *  List<T>.RemoveAll           | Predicate<T>         | int (removed count)
     *  List<T>.ForEach             | Action<T>            | void
     *  Array.FindAll               | Predicate<T>         | T[]
     *  Array.ForEach               | Action<T>            | void
     *
     * Predicate<T> and Func<T, bool> are semantically the same shape
     * ((T) => bool) but are different delegate types — no implicit
     * variable conversion. Lambdas infer either at the call site; to
     * reuse a stored delegate, wrap: p => pred(p) or use .Invoke as
     * a method group: new Predicate<T>(func.Invoke).
     * -------------------------------------------------------------------------
     */
    public static void DemonstrateBclListApis(List<Product> items)
    {
        Predicate<Product> inStock = IsActiveInStock;
        Func<Product, bool> sameFilter = p => inStock(p); // wrap Predicate — no implicit delegate conversion

        List<Product> pickList = items.FindAll(inStock);
        bool allHaveSku = items.TrueForAll(p => !string.IsNullOrWhiteSpace(p.Sku));
        int removed = items.RemoveAll(p => !p.IsActive);

        Console.WriteLine($"  FindAll (in stock):     {pickList.Count} item(s)");
        Console.WriteLine($"  TrueForAll (valid SKU): {allHaveSku}");
        Console.WriteLine($"  RemoveAll (inactive):   {removed} removed, {items.Count} remain");

        Console.WriteLine("  ForEach pick lines:");
        pickList.ForEach(p =>
            Console.WriteLine($"    PICK  {p.Sku,-10}  stock {p.StockQty,4}"));

        Product[] snapshot = items.ToArray();
        Array.ForEach(snapshot, p =>
            Console.WriteLine($"    Array.ForEach saw {p.Sku}"));

        Product[] fromArray = Array.FindAll(snapshot, p => sameFilter(p)); // lambda satisfies Predicate<T> parameter
        Console.WriteLine($"  Array.FindAll returned {fromArray.Length} in-stock SKU(s)");
    }

    /*
     * =========================================================================
     * SECTION 9: PIPELINE — COMPOSE Func, Action, AND Predicate
     * =========================================================================
     *
     * Parameters typed as Func/Action/Predicate make helpers reusable —
     * swap the filter or formatter without changing loop structure.
     * -------------------------------------------------------------------------
     */
    public static void ProcessInventory(
        Product[] items,
        Predicate<Product> include,
        Func<Product, decimal> valueSelector,
        Action<string> reportLine)
    {
        reportLine("=== Inventory valuation (filtered) ===");
        decimal grandTotal = 0m;

        foreach (Product item in items)
        {
            if (!include(item))
            {
                continue;
            }

            decimal value = valueSelector(item); // Func invoke via shorthand
            grandTotal += value;
            reportLine($"  {item.Sku,-10}  stock value {value,10:C}");
        }

        reportLine($"  TOTAL ON-HAND VALUE: {grandTotal:C}");
    }

    public static void PrintCatalog(IEnumerable<Product> items)
    {
        foreach (Product p in items)
        {
            Console.WriteLine(
                $"  {p.Sku,-10}  {p.Name,-30}  {p.UnitPrice,8:C}  stock {p.StockQty,4}  active {p.IsActive}");
        }
    }

    /*
     * =========================================================================
     * SECTION 10: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     */
    public static void Main(string[] args)
    {
        Product[] catalog =
        [
            new Product("WH-4412", "Industrial Shelving Unit", 49.99m, 120, true),
            new Product("WH-8890", "Heavy-Duty Pallet Jack", 899.00m, 8, true),
            new Product("WH-2201", "Safety Vest (Bulk)", 12.50m, 0, false),
            new Product("WH-3305", "Barcode Scanner Kit", 245.00m, 34, true),
            new Product("WH-1100", "Forklift Battery Charger", 1250.00m, 3, true),
        ];

        Console.WriteLine("=== 05. Func, Action, and Predicate ===");
        Console.WriteLine();
        Console.WriteLine("--- Raw catalog ---");
        PrintCatalog(catalog);

        // --- Section 2: Func<TResult> ---
        Func<decimal> taxRateProvider = GetRegionalTaxRate; // method group → Func<decimal>
        decimal taxRate = taxRateProvider();                // shorthand invoke
        decimal taxRateExplicit = taxRateProvider.Invoke(); // .Invoke() — identical result

        Console.WriteLine();
        Console.WriteLine("--- Func<TResult> (no parameters) ---");
        Console.WriteLine($"Regional tax rate: {taxRate:P1} (Invoke: {taxRateExplicit:P1})");

        // --- Section 3: Func<T,TResult> and multi-param Func ---
        Func<Product, decimal> unitPriceWithTax = p => p.UnitPrice * (1m + taxRate);
        Func<Product, int, decimal> lineTotal = CalculateLineTotal;
        Func<decimal, int, decimal, decimal> discountedTotal = ApplyVolumeDiscount;

        Console.WriteLine();
        Console.WriteLine("--- Func<T,TResult> and Func<T1,T2,T3,TResult> ---");
        foreach (Product item in catalog)
        {
            decimal taxedUnit = unitPriceWithTax(item);
            decimal totalForTen = lineTotal(item, 10);
            decimal bulkLine = discountedTotal(item.UnitPrice, 10, 0.05m);
            Console.WriteLine(
                $"{item.Sku}: taxed unit {taxedUnit:C}, line×10 {totalForTen:C}, bulk-5% {bulkLine:C}");
        }

        /*
         * =========================================================================
         * SECTION 4: Action AND Action<T> — VOID-RETURNING CALLBACKS
         * =========================================================================
         *
         * Action mirrors Func but returns void — no return type parameter.
         *
         *  Declaration              | Meaning
         *  -------------------------|------------------------------------------
         *  Action                     | void Method()           — 0 parameters
         *  Action<T>                  | void Method(T arg)      — 1 parameter
         *  Action<T1, T2, …, T16>     | void Method(…)          — up to 16 params
         *
         * Use Action for SIDE EFFECTS (write, log, mutate) rather than a result.
         * List<T>.ForEach and Array.ForEach accept Action<T>.
         * -------------------------------------------------------------------------
         */
        Action printBanner = () =>
            Console.WriteLine("--- Pick list (in-stock, active SKUs) ---");

        Action<Product> printPickLine = p =>
            Console.WriteLine($"  PICK  {p.Sku,-10}  {p.Name,-30}  qty on hand: {p.StockQty,4}");

        Action<Product, int> printLineQty = (p, qty) =>
            Console.WriteLine($"  LINE  {p.Sku,-10}  order qty {qty,4}  extended {CalculateLineTotal(p, qty),8:C}");

        printBanner(); // Action with zero parameters
        foreach (Product item in catalog)
        {
            if (IsActiveInStock(item))
            {
                printPickLine(item);
                printLineQty(item, 5);
            }
        }

        /*
         * =========================================================================
         * SECTION 5: Predicate<T> VS Func<T, bool>
         * =========================================================================
         *
         * Predicate<T> is declared as:
         *
         *   public delegate bool Predicate<in T>(T obj);
         *
         * Exactly ONE parameter; returns bool — "does this item pass?"
         *
         * Func<T, bool> has the same signature shape but is a different delegate
         * type (no implicit variable conversion). List<T>.FindAll and
         * Array.FindAll name Predicate<T>; LINQ Where uses Func<T, bool>.
         *
         * Wrap stored delegates to cross types: p => pred(p) or pred.Invoke
         * as a method group when assigning Func → Predicate.
         * -------------------------------------------------------------------------
         */
        Predicate<Product> inStockAndActive = IsActiveInStock;
        Func<Product, bool> funcFilter = p => inStockAndActive(p);       // wrap stored Predicate as Func
        Predicate<Product> backToPredicate = funcFilter.Invoke;          // method group — Invoke matches Predicate shape

        Product[] pickCandidates = Array.FindAll(catalog, inStockAndActive);

        Console.WriteLine();
        Console.WriteLine("--- Predicate<T> vs Func<T, bool> ---");
        Console.WriteLine($"funcFilter( WH-4412 ): {funcFilter(catalog[0])}");
        Console.WriteLine($"backToPredicate via Invoke method group: {backToPredicate(catalog[0])}");
        Console.WriteLine($"Array.FindAll matched {pickCandidates.Length} of {catalog.Length} SKU(s)");

        // --- Section 5: Invoke patterns and multicast Action ---
        Action<string> notify = LogInfo;
        notify += LogAudit; // multicast — both run on one invoke
        Console.WriteLine();
        Console.WriteLine("--- Invoke / multicast Action<string> ---");
        notify.Invoke("pick-list generated");
        Console.WriteLine($"  Targets in chain: {notify.GetInvocationList().Length}");

        // --- Section 6: Null checks ---
        Action<string>? optionalHandler = null;
        Console.WriteLine();
        Console.WriteLine("--- Null-safe invoke ---");
        SafeNotify(optionalHandler, "this would throw without ?."); // skipped safely
        optionalHandler ??= DefaultPickLine;
        SafeNotify(optionalHandler, "WH-4412 × 2");

        // --- Section 7: BCL List/Array APIs ---
        List<Product> mutableCatalog = new List<Product>(catalog);
        Console.WriteLine();
        Console.WriteLine("--- BCL APIs (List / Array) ---");
        DemonstrateBclListApis(mutableCatalog);

        // --- Section 8: Custom delegate vs Func ---
        LineTotalCalculator customLineCalc = CalculateLineTotal;
        Func<Product, int, decimal> builtInLineCalc = CalculateLineTotal;
        Product sample = catalog[0];
        decimal fromCustom = customLineCalc(sample, 5);
        decimal fromBuiltIn = builtInLineCalc.Invoke(sample, 5); // Func invoke explicit

        Func<int, int> builtInSquare = Square;
        int squared = builtInSquare(6);

        Console.WriteLine();
        Console.WriteLine("--- Custom delegate vs Func (same method group) ---");
        Console.WriteLine($"LineTotalCalculator:              {fromCustom:C}");
        Console.WriteLine($"Func<Product,int,decimal>:        {fromBuiltIn:C}");
        Console.WriteLine($"Results equal:                    {fromCustom == fromBuiltIn}");
        Console.WriteLine($"Func<int,int> Square(6) = {squared} (01. Delegates int→int shape)");

        /*
         * =========================================================================
         * SECTION 11: PREVIEW — LINQ AND DELEGATES (MODULE 05)
         * =========================================================================
         *
         * LINQ extension methods on IEnumerable<T> take Func and Func<T,bool>
         * extensively — depth is in 05. Language Integrated Query:
         *
         *   items.Where(p => p.StockQty > 0)     // Func<T, bool> filter
         *   items.Select(p => p.UnitPrice)         // Func<T, TResult> projection
         *   items.OrderBy(p => p.Sku)              // Func<T, TKey> key selector
         *
         * COVERED IN DETAIL LATER → 05. Language Integrated Query
         * -------------------------------------------------------------------------
         */
        Console.WriteLine();
        Console.WriteLine("--- Preview: LINQ-shaped delegates (module 05) ---");
        Console.WriteLine("  Where/Select/OrderBy accept Func — full LINQ lesson in module 05.");

        // --- Section 9: Pipeline ---
        Predicate<Product> filter = IsActiveInStock;
        Func<Product, decimal> stockValue = p => p.UnitPrice * p.StockQty;
        Action<string> emit = line => Console.WriteLine(line);

        Console.WriteLine();
        ProcessInventory(catalog, filter, stockValue, emit);
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — FUNC, ACTION, PREDICATE
 * =========================================================================
 *
 * --- Func (returns TResult) ---
 *
 *   Func<TResult>                      // () => TResult
 *   Func<T, TResult>                   // (T) => TResult
 *   Func<T1, T2, TResult>              // (T1, T2) => TResult
 *   … up to Func<T1,…,T16, TResult>     // last type param is ALWAYS return type
 *
 * --- Action (returns void) ---
 *
 *   Action                               // () => void
 *   Action<T>                            // (T) => void
 *   Action<T1, T2, …>                    // up to 16 parameters, void return
 *
 * --- Predicate vs Func<T, bool> ---
 *
 *   Predicate<T>                         // (T) => bool  — legacy filter name
 *   Func<T, bool>                        // same shape; different delegate type (no implicit swap)
 *   Lambdas infer either at call site; wrap stored delegates to cross types
 *
 * --- Invoke ---
 *
 *   del(arg);              del.Invoke(arg);     // equivalent for single target
 *   del?.Invoke(arg);       // safe when delegate may be null
 *   del += Other;           // multicast (Action/common void pattern)
 *
 * --- vs custom delegate (01. Delegates) ---
 *
 *   public delegate TReturn MyOp(T arg);   // you declare + name
 *   Func<T, TReturn> op = Method;          // BCL generic, no new type
 *
 * --- Common BCL consumers ---
 *
 *   List<T>.FindAll / TrueForAll / RemoveAll / ForEach
 *   Array.FindAll / ForEach
 *   LINQ Where, Select, … → module 05
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Using Func for void method           | CS0123 — use Action instead
 *  Wrong type order in Func             | Return type must be last generic arg
 *  Null delegate invoke                 | NullReferenceException — use ?.
 *  Predicate vs Func<T,bool> confusion  | Same shape; wrap or use lambda at call site
 *
 * --- Related chapters ---
 *
 *   01. Delegates              — custom delegate, multicast, method groups
 *   02. Lambda Expressions     — inline targets for Func/Action/Predicate
 *   04. Extension Methods      — Func parameters on extension methods
 *   05. Language Integrated Query — IEnumerable operators and delegates
 *
 * =========================================================================
 */
