/*
 * =============================================================================
 * 01. INTRODUCTION TO LINQ — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Language Integrated Query (LINQ) — what it is, why it exists, and the
 *        mental model for querying in-memory sequences with method syntax and
 *        query syntax over IEnumerable<T>.
 *
 * WHY IT MATTERS:
 *   Filtering, projecting, and summarizing collections with nested foreach
 *   loops becomes repetitive and hard to read as rules grow. LINQ expresses
 *   those operations as a composable pipeline (Where → Select → …) that reads
 *   top-to-bottom. The same mental model shows up across lists, arrays,
 *   EF Core, and XML — learn it once here, reuse it everywhere later.
 *
 * WHAT YOU WILL LEARN:
 *   1.  What LINQ is and how LINQ to Objects fits the bigger picture
 *   2.  Why LINQ beats hand-rolled loops for common data work
 *   3.  IEnumerable<T> as the sequence contract every pipeline uses
 *   4.  Extension methods — why Where/Select appear on arrays and List<T>
 *   5.  Method syntax — chaining Where and Select
 *   6.  Query syntax — from / where / select (and how it maps to methods)
 *   7.  Deferred execution — queries are recipes until you consume them
 *   8.  Immediate execution preview — ToList, Count, and other terminals
 *   9.  When to prefer LINQ vs explicit loops
 *  10. Preview — operator families owned by sibling chapters
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace IntroductionToLinq;

/*
 * =========================================================================
 * SECTION 1: WHAT IS LINQ?
 * =========================================================================
 *
 * LINQ = Language Integrated Query.
 *
 * C# embeds query capabilities in the language so you write declarative data
 * operations instead of hand-rolled loops for every filter and transform.
 *
 * This module focuses on LINQ to Objects — querying sequences already in
 * memory (arrays, List<T>, dictionaries, and other IEnumerable<T> sources).
 *
 * A typical LINQ query is a CHAIN of operators:
 *
 *   source  →  filter  →  transform  →  sort  →  take  →  result
 *
 * Each step is a method from System.Linq (Where, Select, OrderBy, …).
 * Under the hood those methods accept delegates (Func, Predicate) from
 * 04. Functional Style Programming.
 *
 *  Provider           | Source                        | Depth in this module
 *  -------------------|-------------------------------|----------------------
 *  LINQ to Objects    | IEnumerable<T> in memory      | FULL (this chapter+)
 *  LINQ to Entities   | EF Core → SQL translation     | preview only
 *  LINQ to XML        | XDocument / XElement trees    | ch.13
 *
 * Scenario: a warehouse tracks catalog SKUs. We filter active high-value
 * items, compare syntax styles, prove deferred execution, and contrast LINQ
 * with an equivalent foreach loop.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: SAMPLE DOMAIN — CATALOG ITEMS
 * =========================================================================
 *
 * Domain types for the demos. Section comments sit above the types so you
 * can read the model before Main wires the pipelines.
 *
 * --- 2a. StockStatus — lifecycle of a SKU ---
 *
 * Active / Discontinued / Backordered control which items enter reports.
 *
 * --- 2b. CatalogItem — one SKU row ---
 *
 * A readonly record struct keeps demo data immutable and printable via
 * ToString. Fields used by LINQ demos: Sku, Name, Category, UnitPrice, Status.
 * -------------------------------------------------------------------------
 */
public enum StockStatus
{
    Active,
    Backordered,
    Discontinued
}

public readonly record struct CatalogItem(
    string Sku,
    string Name,
    string Category,
    decimal UnitPrice,
    StockStatus Status)
{
    public override string ToString() =>
        $"{Sku,-8}  {Name,-18}  {Category,-10}  {UnitPrice,7:C}  {Status}";
}

/*
 * =========================================================================
 * SECTION 3: EXTENSION METHODS — WHY LINQ APPEARS ON SEQUENCES
 * =========================================================================
 *
 * LINQ operators are mostly EXTENSION METHODS on IEnumerable<T> (and
 * IQueryable<T> for providers). An extension method looks like an instance
 * method but is a static method whose first parameter is marked with this:
 *
 *   public static IEnumerable<T> Where<T>(
 *       this IEnumerable<T> source,
 *       Func<T, bool> predicate)
 *
 * Because arrays and List<T> implement IEnumerable<T>, you can write:
 *
 *   items.Where(i => i.UnitPrice > 20m)
 *
 * even though Where is not declared on CatalogItem[] — it lives in
 * System.Linq.Enumerable and becomes visible when you add:
 *
 *   using System.Linq;
 *
 * Without that using: CS1061 — 'CatalogItem[]' does not contain a definition
 * for 'Where'.
 *
 * --- 3a. Tiny custom extension (teaching pattern only) ---
 *
 * PrintLines below mirrors the pattern: this IEnumerable<T> + a foreach.
 * Real LINQ operators return new sequences; this helper only prints.
 *
 * COVERED IN DETAIL LATER → 04. Functional Style Programming / 04. Extension Methods
 * COVERED IN DETAIL LATER → sibling operator chapters for each LINQ API
 * -------------------------------------------------------------------------
 */
public static class IntroSequenceExtensions
{
    public static void PrintLines<T>(this IEnumerable<T> source, string heading)
    {
        Console.WriteLine(heading);
        foreach (T item in source) // enumerate the sequence one item at a time
        {
            Console.WriteLine($"  {item}");
        }
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 4: DEMONSTRATION — Main orchestrates the chapter
     * =========================================================================
     *
     * Main builds sample data, then walks each concept in order: IEnumerable,
     * method syntax, query syntax, deferred vs immediate execution, providers,
     * and LINQ vs loops. Helpers below keep each demo focused.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        CatalogItem[] catalog =
        [
            new CatalogItem("SKU-101", "USB-C Hub", "Electronics", 49.99m, StockStatus.Active),
            new CatalogItem("SKU-102", "Notebook A5", "Office", 6.50m, StockStatus.Active),
            new CatalogItem("SKU-103", "Desk Lamp", "Office", 32.00m, StockStatus.Backordered),
            new CatalogItem("SKU-104", "Wireless Mouse", "Electronics", 24.99m, StockStatus.Active),
            new CatalogItem("SKU-105", "Stapler", "Office", 8.25m, StockStatus.Discontinued),
            new CatalogItem("SKU-106", "HDMI Cable", "Electronics", 12.00m, StockStatus.Active),
        ];

        Console.WriteLine("=== 01. Introduction to LINQ ===");
        Console.WriteLine();
        Console.WriteLine("--- Warehouse catalog (source data) ---");
        catalog.PrintLines("All SKUs:"); // custom extension — same this-pattern as LINQ

        DemonstrateWhyLinq(catalog);
        DemonstrateIEnumerableContract(catalog);
        DemonstrateMethodSyntax(catalog);
        DemonstrateQuerySyntax(catalog);
        DemonstrateSyntaxEquivalence(catalog);
        DemonstrateDeferredExecution(catalog);
        DemonstrateImmediateExecutionPreview(catalog);
        PreviewOtherProviders();
        DemonstrateLinqVersusLoop(catalog);
        PreviewOperatorChapters();
    }

    /*
     * =========================================================================
     * SECTION 5: WHY LINQ — DECLARATIVE PIPELINES VS HAND-ROLLED LOOPS
     * =========================================================================
     *
     * Without LINQ, a “active electronics names” report needs a list, a loop,
     * an if, and a projection — easy to get right once, painful to compose.
     *
     * With LINQ, the same intent is a short chain that reads as the business
     * rule. Later chapters add OrderBy, GroupBy, Join, and more without
     * rewriting the foreach skeleton each time.
     *
     * This section only motivates the style. Sections 6–8 show the syntax;
     * Section 12 compares totals side-by-side with a foreach.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateWhyLinq(CatalogItem[] catalog)
    {
        // Hand-rolled: allocate, loop, branch, project
        List<string> loopNames = new List<string>();
        foreach (CatalogItem item in catalog)
        {
            if (item.Status == StockStatus.Active && item.Category == "Electronics")
            {
                loopNames.Add(item.Name);
            }
        }

        // LINQ: same intent as a pipeline (deferred until PrintLines enumerates)
        IEnumerable<string> linqNames =
            catalog
                .Where(i => i.Status == StockStatus.Active && i.Category == "Electronics")
                .Select(i => i.Name);

        Console.WriteLine();
        Console.WriteLine("--- Why LINQ (same report, two styles) ---");
        loopNames.PrintLines("foreach + if + Add:");
        linqNames.PrintLines("Where + Select:");
    }

    /*
     * =========================================================================
     * SECTION 6: IEnumerable<T> — THE PIPELINE INPUT
     * =========================================================================
     *
     * LINQ to Objects extension methods extend IEnumerable<T> — anything you
     * can foreach over.
     *
     *   IEnumerable<CatalogItem> pipeline = catalog.Where(…);
     *
     * IEnumerable<T> means "a sequence of T items, consumed one at a time."
     * Arrays, List<T>, HashSet<T>, and most LINQ results implement it.
     *
     * Important: a LINQ Where/Select result is usually still IEnumerable<T>,
     * not List<T>. You only get a List when you call ToList() (or similar).
     *
     * COVERED IN DETAIL LATER → 03. Generics & Collections (IEnumerable chapter)
     *   (foreach, lazy iteration, covariance, custom enumerators)
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateIEnumerableContract(CatalogItem[] catalog)
    {
        IEnumerable<CatalogItem> asSequence = catalog; // array already is IEnumerable<T>
        int count = asSequence.Count();                // terminal — forces a full scan

        Console.WriteLine();
        Console.WriteLine("--- IEnumerable<T> ---");
        Console.WriteLine($"Catalog is IEnumerable<CatalogItem>; Count() = {count}");
        Console.WriteLine("LINQ operators accept and (usually) return IEnumerable<T>.");
    }

    /*
     * =========================================================================
     * SECTION 7: METHOD SYNTAX — Where AND Select INTRO
     * =========================================================================
     *
     * Method syntax is fluent chaining. Two operators you will use constantly:
     *
     *   Operator | Role                         | Lambda shape
     *   ---------|------------------------------|---------------------------
     *   Where    | Keep items that match        | Func<T, bool> predicate
     *   Select   | Project each item to a value | Func<T, TResult> selector
     *
     * Example pipeline:
     *
     *   catalog
     *     .Where(i => i.Status == StockStatus.Active)
     *     .Select(i => i.Name);
     *
     * Read it left-to-right: start from catalog, keep Active rows, then keep
     * only the Name of each remaining row.
     *
     * COVERED IN DETAIL LATER → 02. Filtering & Aggregation (Where, Count, Sum, …)
     * COVERED IN DETAIL LATER → 08. Projection Operations (Select / SelectMany depth)
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateMethodSyntax(CatalogItem[] catalog)
    {
        IEnumerable<string> activeNames =
            catalog
                .Where(i => i.Status == StockStatus.Active) // filter — keep Active only
                .Select(i => i.Name);                       // project — CatalogItem → string

        Console.WriteLine();
        Console.WriteLine("--- Method syntax (Where + Select) ---");
        activeNames.PrintLines("Active item names:");
    }

    /*
     * =========================================================================
     * SECTION 8: QUERY SYNTAX — from / where / select
     * =========================================================================
     *
     * Query syntax is a SQL-like language feature. The compiler translates it
     * into the same method calls you wrote in Section 7.
     *
     * Minimal shape:
     *
     *   from item in source
     *   where predicate
     *   select projection
     *
     * Rules that matter early:
     *   • from introduces the range variable (like foreach's loop variable)
     *   • where filters (compiles to Where)
     *   • select projects (compiles to Select) — query expressions end with
     *     select or group
     *   • You can stack multiple where clauses
     *
     * Query syntax shines when a query has several clauses. Method syntax is
     * more common for short chains and for operators with no query keyword.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateQuerySyntax(CatalogItem[] catalog)
    {
        IEnumerable<string> electronicsLabels =
            from item in catalog                          // range variable: each CatalogItem
            where item.Category == "Electronics"          // filter clause → Where
            where item.Status != StockStatus.Discontinued // second where → another Where
            select $"{item.Sku}: {item.Name}";            // projection → Select

        Console.WriteLine();
        Console.WriteLine("--- Query syntax (from / where / select) ---");
        electronicsLabels.PrintLines("Active/backordered electronics:");
    }

    /*
     * =========================================================================
     * SECTION 9: SAME LOGIC — TWO SURFACE FORMS
     * =========================================================================
     *
     * Method and query syntax are equivalent for standard operators. Pick the
     * form your team reads more easily; do not mix styles randomly inside one
     * expression without a reason.
     *
     *  Goal                         | Method syntax           | Query syntax
     *  -----------------------------|-------------------------|---------------------------
     *  Filter Active + price > 20   | .Where(…)               | where …
     *  Project to label string      | .Select(…)              | select …
     *
     * You can also mix: start with query syntax, then append method calls:
     *
     *   (from i in catalog where … select i).Take(2)
     *
     * COVERED IN DETAIL LATER → 11. Partitioning Operations (Take / Skip)
     * COVERED IN DETAIL LATER → 03. Ordering (orderby / OrderBy)
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateSyntaxEquivalence(CatalogItem[] catalog)
    {
        IEnumerable<CatalogItem> methodForm =
            catalog
                .Where(i => i.Status == StockStatus.Active)
                .Where(i => i.UnitPrice > 20m);

        IEnumerable<CatalogItem> queryForm =
            from i in catalog
            where i.Status == StockStatus.Active
            where i.UnitPrice > 20m
            select i;

        IEnumerable<string> mixed =
            (from i in catalog
             where i.Status == StockStatus.Active
             select $"{i.Sku} @ {i.UnitPrice:C}")
            .Take(2); // method call after a query expression — Take is ch.11

        Console.WriteLine();
        Console.WriteLine("--- Method vs query (Active and price > 20) ---");
        methodForm.PrintLines("Method syntax:");
        queryForm.PrintLines("Query syntax (same filter):");
        mixed.PrintLines("Mixed query + Take(2):");
    }

    /*
     * =========================================================================
     * SECTION 10: DEFERRED EXECUTION
     * =========================================================================
     *
     * Building a LINQ query does NOT iterate the source immediately.
     *
     *   var query = catalog.Where(…).Select(…);  // recipe only — no foreach yet
     *
     * Execution happens when you:
     *   • foreach over the query
     *   • call a terminal / aggregating operator (Count, ToList, First, Sum, …)
     *
     * Benefits:
     *   • Compose pipelines cheaply before any work runs
     *   • Re-enumerate to see current source data (mutable sources can change
     *     results between passes)
     *
     * Pitfall: enumerating a deferred query twice runs the work twice. Call
     * ToList() when you need a stable snapshot (preview in Section 11).
     *
     * The counter inside Select below proves laziness: it stays 0 until foreach.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateDeferredExecution(CatalogItem[] catalog)
    {
        int projectionRuns = 0;

        IEnumerable<string> deferredLabels =
            catalog
                .Where(i => i.Status == StockStatus.Backordered)
                .Select(i =>
                {
                    projectionRuns++; // runs only when the pipeline is consumed
                    return $"[{i.Sku}] backordered — {i.Name}";
                });

        Console.WriteLine();
        Console.WriteLine("--- Deferred execution ---");
        Console.WriteLine($"After building pipeline, projectionRuns = {projectionRuns}");

        Console.WriteLine("First foreach (execution pass 1):");
        foreach (string label in deferredLabels)
        {
            Console.WriteLine($"  {label}");
        }
        Console.WriteLine($"After first foreach, projectionRuns = {projectionRuns}");

        Console.WriteLine("Second foreach (execution pass 2 — pipeline re-runs):");
        foreach (string label in deferredLabels)
        {
            Console.WriteLine($"  {label}");
        }
        Console.WriteLine($"After second foreach, projectionRuns = {projectionRuns}");
    }

    /*
     * =========================================================================
     * SECTION 11: IMMEDIATE EXECUTION (PREVIEW)
     * =========================================================================
     *
     * Terminal operators force the pipeline (or enough of it) to run NOW and
     * return a concrete result:
     *
     *   Operator family     | Examples              | Result shape
     *   --------------------|-----------------------|------------------
     *   Materialize         | ToList, ToArray       | new collection
     *   Aggregate scalar    | Count, Sum, Average   | number / value
     *   Element pick        | First, Single, …      | one element
     *
     * Snapshot pattern — freeze Active items so later source changes do not
     * affect the cached list:
     *
     *   List<CatalogItem> snapshot = catalog.Where(…).ToList();
     *
     * COVERED IN DETAIL LATER → 10. Conversion Operations
     * COVERED IN DETAIL LATER → 06. Element Operations
     * COVERED IN DETAIL LATER → 02. Filtering & Aggregation (Count / Sum depth)
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateImmediateExecutionPreview(CatalogItem[] catalog)
    {
        List<CatalogItem> activeSnapshot =
            catalog
                .Where(i => i.Status == StockStatus.Active)
                .ToList(); // materialize now — later enumerations use the List

        int activeCount = catalog.Count(i => i.Status == StockStatus.Active); // terminal scalar

        Console.WriteLine();
        Console.WriteLine("--- Immediate execution preview ---");
        Console.WriteLine($"ToList snapshot: {activeSnapshot.Count} active SKU(s)");
        Console.WriteLine($"Count(predicate): {activeCount} active SKU(s)");
        Console.WriteLine("(Full conversion / element APIs → ch.10 and ch.06)");
    }

    /*
     * =========================================================================
     * SECTION 12: OTHER LINQ PROVIDERS (PREVIEW)
     * =========================================================================
     *
     * LINQ is a pattern, not only in-memory lists. The query SHAPE (where,
     * select, orderby) feels familiar; the provider decides how it runs.
     *
     *  Provider           | Execution model
     *  -------------------|------------------------------------------------
     *  LINQ to Objects    | In-process over IEnumerable<T> (this module)
     *  LINQ to XML        | Queries over XElement / XDocument trees
     *  LINQ to Entities   | IQueryable<T> translated to SQL (EF Core)
     *
     * COVERED IN DETAIL LATER → 13. LINQ to XML
     * EF Core / IQueryable lives outside this module; same mental model applies.
     * -------------------------------------------------------------------------
     */
    private static void PreviewOtherProviders()
    {
        Console.WriteLine();
        Console.WriteLine("--- LINQ providers preview ---");
        Console.WriteLine("This chapter: LINQ to Objects (in-memory IEnumerable<T>).");
        Console.WriteLine("Later: LINQ to XML (ch.13). EF Core uses IQueryable at runtime.");
    }

    /*
     * =========================================================================
     * SECTION 13: WHEN TO USE LINQ VS LOOPS
     * =========================================================================
     *
     * Prefer LINQ when:
     *   • The work is a standard filter / project / aggregate
     *   • You want a composable, readable pipeline
     *   • Clarity matters more than micro-optimizing every index
     *
     * Prefer explicit loops when:
     *   • Control flow is complex (many branches, early exits, multi-step state)
     *   • You must mutate the source while iterating (often a design smell)
     *   • Profiling shows a hot path where allocation or delegates hurt (rare)
     *
     * Below: same total of Active UnitPrice — foreach vs Where + Sum.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateLinqVersusLoop(CatalogItem[] catalog)
    {
        decimal totalLoop = 0m;
        foreach (CatalogItem item in catalog)
        {
            if (item.Status == StockStatus.Active)
            {
                totalLoop += item.UnitPrice;
            }
        }

        decimal totalLinq = catalog
            .Where(i => i.Status == StockStatus.Active)
            .Sum(i => i.UnitPrice); // terminal aggregate — runs immediately

        Console.WriteLine();
        Console.WriteLine("--- LINQ vs loop (active UnitPrice total) ---");
        Console.WriteLine($"Loop total:   {totalLoop:C}");
        Console.WriteLine($"LINQ Sum():   {totalLinq:C}");
    }

    /*
     * =========================================================================
     * SECTION 14: OPERATOR CHAPTERS AHEAD (PREVIEW)
     * =========================================================================
     *
     * This introduction stops at the mental model. Sibling folders teach each
     * operator family in depth — do not expect full API coverage here.
     *
     *  Chapter | Folder                         | Headline operators
     *  --------|--------------------------------|------------------------------
     *  02      | Filtering & Aggregation        | Where, Count, Sum, Average, …
     *  03      | Ordering                       | OrderBy, ThenBy, Reverse
     *  04      | Grouping                       | GroupBy, ToLookup
     *  05      | Joins                          | Join, GroupJoin
     *  06      | Element Operations             | First, Single, ElementAt, …
     *  07      | Set Operations                 | Distinct, Union, Except, …
     *  08      | Projection Operations          | Select, SelectMany
     *  09      | Quantifier Operations          | Any, All, Contains
     *  10      | Conversion Operations          | ToList, ToArray, ToDictionary
     *  11      | Partitioning Operations        | Take, Skip, TakeWhile, …
     *  12      | Generation Operations          | Range, Repeat, Empty
     *  13      | LINQ to XML                    | XElement queries
     * -------------------------------------------------------------------------
     */
    private static void PreviewOperatorChapters()
    {
        Console.WriteLine();
        Console.WriteLine("--- Next chapters ---");
        Console.WriteLine("Next: 02. Filtering & Aggregation — Where + aggregates in depth.");
        Console.WriteLine("Then: ordering, grouping, joins, and the remaining operator families.");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — INTRODUCTION TO LINQ
 * =========================================================================
 *
 * --- Core idea ---
 *
 *   LINQ = composable query pipelines over sequences (usually IEnumerable<T>).
 *   This module starts with LINQ to Objects (in-memory collections).
 *
 * --- Required using ---
 *
 *   using System.Linq;   // without it → CS1061 on Where / Select / …
 *
 * --- Extension methods ---
 *
 *   Where / Select / … are static methods with a this IEnumerable<T> parameter.
 *   They look like instance methods on arrays and List<T>.
 *
 * --- Syntax forms (equivalent for standard operators) ---
 *
 *   Method:  catalog.Where(i => i.Status == Active).Select(i => i.Name)
 *   Query:   from i in catalog where i.Status == Active select i.Name
 *
 * --- from / where / select ---
 *
 *   from x in source   → range variable (like foreach)
 *   where predicate    → Where
 *   select projection  → Select (query must end with select or group)
 *
 * --- Execution model ---
 *
 *   Deferred:  Where, Select, OrderBy, Take, … — run on foreach / each enumerate
 *   Immediate: ToList, ToArray, Count, Sum, First, Any, … — run now
 *
 * --- Typical pipeline ---
 *
 *   var result = source
 *       .Where(filter)
 *       .Select(projection);
 *
 *   foreach (var item in result) { … }   // execution happens here
 *
 * --- Prefer LINQ when ---
 *
 *   Standard filter / project / aggregate; readability and composition matter.
 *
 * --- Prefer loops when ---
 *
 *   Complex branching, in-place mutation, or a proven hot-path bottleneck.
 *
 * --- Forward chapters ---
 *
 *   Where / Sum / Count depth     → 02. Filtering & Aggregation
 *   OrderBy / orderby             → 03. Ordering
 *   Select / SelectMany depth     → 08. Projection Operations
 *   ToList / ToArray / …          → 10. Conversion Operations
 *   LINQ to XML                   → 13. LINQ to XML
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Expect query to run at declaration   | Nothing until foreach / terminal
 *  Enumerate deferred query twice       | Work runs twice — ToList if needed
 *  Treat IEnumerable as List            | May be lazy / single-pass
 *  Missing using System.Linq            | CS1061 — extension methods missing
 *
 * =========================================================================
 */
