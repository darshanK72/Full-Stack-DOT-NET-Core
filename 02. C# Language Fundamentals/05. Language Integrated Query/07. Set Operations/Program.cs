/*
 * =============================================================================
 * 07. SET OPERATIONS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: LINQ set operators — Distinct, Union, Intersect, and Except — apply
 *        set membership to IEnumerable<T> sequences. Custom equality via
 *        IEqualityComparer<T>, plus .NET 6+ *By key-selector overloads
 *        (DistinctBy, UnionBy, IntersectBy, ExceptBy).
 *
 * WHY IT MATTERS:
 *   Real data arrives duplicated, split across sources, or partially overlapping.
 *   Set operators answer "unique values", "in A or B", "in both", and "in A
 *   but not B" without hand-rolled HashSet loops. Choosing the right equality
 *   rule (default, StringComparer, custom comparer, or *By key) is what makes
 *   the result match business identity.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Set-operation semantics, order guarantees, and deferred execution
 *   2.  Distinct — remove duplicates (default equality and comparer)
 *   3.  DistinctBy — uniqueness by a projected key (.NET 6+)
 *   4.  Union / UnionBy — merge two sequences with unique results
 *   5.  Intersect / IntersectBy — elements present in both sequences
 *   6.  Except / ExceptBy — elements in the first sequence but not the second
 *   7.  IEqualityComparer<T> — Equals/GetHashCode contract for custom types
 *   8.  Union vs Concat — set merge vs append (preview → ch.12)
 *   9.  SequenceEqual — brief equality check (preview → ch.09)
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace SetOperations;

/*
 * =============================================================================
 * SECTION 1: DOMAIN MODEL — CatalogItem (reference type for set demos)
 * =============================================================================
 *
 * Set operators need a clear notion of "same element." For int and string,
 * EqualityComparer<T>.Default already does the right thing. For classes,
 * default equality is REFERENCE equality unless the type implements
 * IEquatable<T> or you pass an IEqualityComparer<T>.
 *
 * This chapter uses a plain class (not a record) on purpose — two
 * `new CatalogItem("SKU-100", "Widget", …)` objects look the same in print
 * but are NOT equal by default, so Distinct/Union/Intersect/Except keep both
 * until you supply a comparer or a *By key selector.
 *
 * Scenario: a warehouse syncs product feeds from WebStore and Marketplace.
 * Feeds overlap and sometimes import the same SKU twice. Ops needs unique
 * catalogs, shared SKUs, channel-only listings, and a master unique list.
 * -------------------------------------------------------------------------
 */
public sealed class CatalogItem
{
    public string Sku { get; }          // stable business identity for set membership
    public string Name { get; }
    public string Channel { get; }      // "Web" or "Market" — informational only
    public decimal UnitPrice { get; }

    public CatalogItem(string sku, string name, string channel, decimal unitPrice)
    {
        Sku = sku;
        Name = name;
        Channel = channel;
        UnitPrice = unitPrice;
    }

    public override string ToString() => $"{Sku} | {Name} | {Channel} | {UnitPrice:C}";
}

/*
 * =============================================================================
 * SECTION 2: IEqualityComparer<T> — CUSTOM EQUALITY FOR SET OPERATORS
 * =============================================================================
 *
 * Implement System.Collections.Generic.IEqualityComparer<T>:
 *
 *   bool Equals(T? x, T? y);
 *   int GetHashCode(T obj);
 *
 * Contract (same rules as Object.Equals / GetHashCode):
 *
 *   • If Equals(a, b) is true, GetHashCode(a) MUST equal GetHashCode(b).
 *   • GetHashCode must be stable while the set operator runs.
 *   • Equals should be reflexive, symmetric, and transitive.
 *
 * If GetHashCode disagrees with Equals, Distinct/Union/Intersect/Except can
 * return wrong results — hash buckets will not align with equality.
 *
 * CatalogItemBySkuComparer treats two rows as the same product when Sku
 * matches (ignore duplicate import rows / name typos on the same SKU).
 *
 * Built-in alternatives when a BCL rule fits:
 *   StringComparer.OrdinalIgnoreCase
 *   EqualityComparer<T>.Default
 * -------------------------------------------------------------------------
 */
public sealed class CatalogItemBySkuComparer : IEqualityComparer<CatalogItem>
{
    public bool Equals(CatalogItem? x, CatalogItem? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true; // same instance (or both null)
        }

        if (x is null || y is null)
        {
            return false;
        }

        return string.Equals(x.Sku, y.Sku, StringComparison.Ordinal); // identity = SKU only
    }

    public int GetHashCode(CatalogItem obj) =>
        StringComparer.Ordinal.GetHashCode(obj.Sku); // must use the same fields as Equals
}

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 3: SET OPERATIONS — OVERVIEW
         * =========================================================================
         *
         * LINQ set operators live on IEnumerable<T> (System.Linq). They return
         * IEnumerable<T> — execution is DEFERRED until you iterate or call a
         * terminal operator (ToList, Count, foreach, etc.).
         *
         *  Operator     | Set meaning                         | Order comes from
         *  -------------|-------------------------------------|------------------------
         *  Distinct     | Unique elements in ONE sequence     | First occurrence in source
         *  Union        | A ∪ B — unique from either          | First, then new from second
         *  Intersect    | A ∩ B — in both                     | First sequence
         *  Except       | A \ B — in first, not in second     | First sequence
         *
         * Each classic operator has a *By sibling (DistinctBy, UnionBy, …) that
         * compares by a key selector instead of whole-element equality.
         *
         * All classic operators also accept an optional IEqualityComparer<T>.
         *
         * COVERED IN DETAIL LATER → 12. Generation Operations
         *   Concat appends sequences and KEEPS duplicates; Union merges and
         *   REMOVES duplicates. Empty, Range, and Repeat are generation helpers.
         * -------------------------------------------------------------------------
         */

        IList<CatalogItem> webFeed = new List<CatalogItem>
        {
            new CatalogItem("SKU-100", "Widget A", "Web", 12.50m),
            new CatalogItem("SKU-200", "Gadget B", "Web", 29.00m),
            new CatalogItem("SKU-300", "Bracket C", "Web", 4.75m),
            new CatalogItem("SKU-300", "Bracket C", "Web", 4.75m), // duplicate import row
            new CatalogItem("SKU-300", "Bracket C", "Web", 4.75m),
            new CatalogItem("SKU-500", "Cable E", "Web", 8.25m),
        };

        IList<CatalogItem> marketFeed = new List<CatalogItem>
        {
            new CatalogItem("SKU-300", "Bracket C", "Market", 4.99m),
            new CatalogItem("SKU-500", "Cable E", "Market", 8.50m),
            new CatalogItem("SKU-600", "Hinge F", "Market", 15.00m),
            new CatalogItem("SKU-700", "Latch G", "Market", 6.40m),
        };

        string[] webTags = { "hardware", "FastShip", "linq", "hardware", "api", "linq" };
        string[] marketTags = { "linq", "api", "azure", "fastship", "docker" };

        IEqualityComparer<CatalogItem> bySku = new CatalogItemBySkuComparer();

        Console.WriteLine("=== Multi-channel catalog — set operations demo ===");
        Console.WriteLine($"Web feed rows: {webFeed.Count}, Marketplace feed rows: {marketFeed.Count}");
        Console.WriteLine();

        /*
         * =========================================================================
         * SECTION 4: Distinct — REMOVE DUPLICATES
         * =========================================================================
         *
         * Distinct() walks the sequence and yields each unique value once,
         * preserving the order of first occurrence.
         *
         *   IEnumerable<TSource> Distinct()
         *   IEnumerable<TSource> Distinct(IEqualityComparer<TSource>? comparer)
         *
         * For int, string, and other types with sensible default equality,
         * EqualityComparer<T>.Default decides sameness (strings: ordinal,
         * case-sensitive).
         *
         * --- 4a. Distinct on primitives ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<int> noisyBins = new[] { 12, 7, 12, 4, 7, 74, 5, 7, 84, 4, 5, 3, 4, 2, 3, 2, 34, 4, 24 };
        IEnumerable<int> uniqueBins = noisyBins.Distinct(); // first-occurrence order preserved

        Console.WriteLine("--- Distinct on int[] (method syntax) ---");
        PrintJoined(uniqueBins);

        /*
         * --- 4b. Distinct in a query expression ---
         *
         * C# query syntax has no `distinct` keyword (that is VB.NET). Call
         * .Distinct() on the source inside the from clause, or assign method
         * syntax to a variable used by a later query:
         *
         *   from bin in noisyBins.Distinct()
         *   where bin % 2 == 0
         *   select bin;
         * -------------------------------------------------------------------------
         */

        IEnumerable<int> uniqueEvenBins =
            from bin in noisyBins.Distinct()
            where bin % 2 == 0
            select bin;

        Console.WriteLine("Distinct even bin numbers:");
        PrintJoined(uniqueEvenBins);
        Console.WriteLine();

        /*
         * --- 4c. Distinct on strings (default = case-sensitive) ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<string> uniqueWebTags = webTags.Distinct(); // "hardware" once; case kept as first seen

        Console.WriteLine("--- Distinct on string tags (case-sensitive default) ---");
        PrintJoined(uniqueWebTags);
        Console.WriteLine();

        /*
         * --- 4d. Distinct on custom types without a comparer ---
         *
         * webFeed.Distinct() uses reference equality for CatalogItem → every
         * row counts as unique, including the three SKU-300 instances.
         * -------------------------------------------------------------------------
         */

        int distinctByReference = webFeed.Distinct().Count(); // three SKU-300 rows stay distinct

        Console.WriteLine("--- CatalogItem class — default equality ---");
        Console.WriteLine($"Distinct() without comparer: {distinctByReference} rows (reference equality)");
        Console.WriteLine();

        /*
         * --- 4e. Distinct with IEqualityComparer / StringComparer ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<CatalogItem> uniqueWebSkus = webFeed.Distinct(bySku); // collapse duplicate SKUs

        Console.WriteLine("--- Distinct with IEqualityComparer (by Sku) ---");
        foreach (CatalogItem item in uniqueWebSkus)
        {
            Console.WriteLine($"  {item}");
        }

        IEnumerable<string> uniqueTagsIgnoreCase =
            webTags.Distinct(StringComparer.OrdinalIgnoreCase); // BCL comparer — no custom class

        Console.WriteLine();
        Console.WriteLine("Distinct tags (OrdinalIgnoreCase):");
        PrintJoined(uniqueTagsIgnoreCase);
        Console.WriteLine();

        /*
         * =========================================================================
         * SECTION 5: DistinctBy — UNIQUENESS BY KEY (.NET 6+)
         * =========================================================================
         *
         * DistinctBy(keySelector) keeps the FIRST element for each distinct key.
         * Prefer this when "same" means "same SKU / same email / same Id" and you
         * do not want to write a full IEqualityComparer<T>.
         *
         *   source.DistinctBy(x => x.Sku)
         *   source.DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
         *
         * Same idea as Distinct(comparer) for key-based identity, with less
         * boilerplate when only one key field matters.
         * -------------------------------------------------------------------------
         */

        IEnumerable<CatalogItem> uniqueWebByKey = webFeed.DistinctBy(item => item.Sku);

        Console.WriteLine("--- DistinctBy(Sku) ---");
        Console.WriteLine($"Distinct(bySku) count:  {uniqueWebSkus.Count()}");
        Console.WriteLine($"DistinctBy(Sku) count:  {uniqueWebByKey.Count()}");
        Console.WriteLine();

        /*
         * =========================================================================
         * SECTION 6: Union — COMBINE TWO SEQUENCES (UNIQUE)
         * =========================================================================
         *
         * Union(first, second) yields unique elements from both inputs.
         *
         *   • Order: all unique items from FIRST (first-occurrence order),
         *     then unique items from SECOND that were not already yielded.
         *   • Overloads: Union(second) and Union(second, comparer)
         *   • UnionBy(second, keySelector) — uniqueness by projected key
         *
         * Method syntax:
         *   webFeed.Union(marketFeed, bySku)
         *
         * Query expression — call Union on the source in the from clause:
         *   from item in webFeed.Union(marketFeed, bySku) select item
         *
         * When the same SKU appears on both channels, Union keeps the FIRST
         * sequence's row (Web price wins over Market price here).
         * -------------------------------------------------------------------------
         */

        IEnumerable<CatalogItem> masterCatalog = webFeed.Union(marketFeed, bySku);

        IEnumerable<CatalogItem> masterCatalogQuery =
            from item in webFeed.Union(marketFeed, bySku)
            orderby item.Sku
            select item;

        IEnumerable<CatalogItem> masterByKey =
            webFeed.UnionBy(marketFeed, item => item.Sku); // same membership as Union(bySku)

        Console.WriteLine("--- Union (unique SKUs across both channels) ---");
        Console.WriteLine("Method syntax (insertion order; Web row wins on overlap):");
        foreach (CatalogItem item in masterCatalog)
        {
            Console.WriteLine($"  {item}");
        }

        Console.WriteLine("Query syntax (same set, ordered by Sku for display):");
        foreach (CatalogItem item in masterCatalogQuery)
        {
            Console.WriteLine($"  {item}");
        }

        Console.WriteLine($"UnionBy(Sku) count: {masterByKey.Count()} (matches Union with bySku)");

        IEnumerable<string> allTags = webTags.Union(marketTags, StringComparer.OrdinalIgnoreCase);

        Console.WriteLine();
        Console.WriteLine("Union of marketing tags (case-insensitive):");
        PrintJoined(allTags);
        Console.WriteLine();

        /*
         * =========================================================================
         * SECTION 7: Intersect — ELEMENTS IN BOTH SEQUENCES
         * =========================================================================
         *
         * Intersect(first, second) yields items in FIRST that also appear in
         * SECOND (by equality or comparer). Order follows the first sequence;
         * each matching element is yielded once (set intersection).
         *
         *   IEnumerable<TSource> Intersect(IEnumerable<TSource> second)
         *   IEnumerable<TSource> Intersect(IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
         *   IntersectBy(secondKeys, keySelector) — first elements whose key is in second
         *
         * Shared listings = SKU registered on BOTH channels.
         *
         * IntersectBy takes a sequence of KEYS as its second argument (not whole
         * elements) — project keys from the other feed with Select.
         * -------------------------------------------------------------------------
         */

        IEnumerable<CatalogItem> sharedListings = webFeed.Intersect(marketFeed, bySku);

        IEnumerable<CatalogItem> sharedListingsQuery =
            from item in webFeed.Intersect(marketFeed, bySku)
            select item;

        IEnumerable<CatalogItem> sharedByKey =
            webFeed.IntersectBy(marketFeed.Select(item => item.Sku), item => item.Sku);

        Console.WriteLine("--- Intersect (SKU listed on BOTH channels) ---");
        foreach (CatalogItem item in sharedListings)
        {
            Console.WriteLine($"  {item}"); // Web-side row (price from first sequence)
        }

        Console.WriteLine($"IntersectBy(Sku) count: {sharedByKey.Count()}");

        IEnumerable<string> sharedTags = webTags.Intersect(marketTags, StringComparer.OrdinalIgnoreCase);

        Console.WriteLine();
        Console.WriteLine("Shared tags:");
        PrintJoined(sharedTags);
        Console.WriteLine();

        /*
         * =========================================================================
         * SECTION 8: Except — IN FIRST, NOT IN SECOND
         * =========================================================================
         *
         * Except(first, second) is the set difference A \ B: unique elements from
         * FIRST that are not present in SECOND (by equality or comparer).
         *
         *   • Order preserved from the first sequence
         *   • Duplicates in first are collapsed — Except already has set semantics
         *   • ExceptBy(secondKeys, keySelector) — drop first elements whose key
         *     appears in the second key sequence
         *
         * Web-only SKUs = in Web feed, not in Marketplace.
         * Market-only SKUs = in Marketplace, not in Web (swap the arguments).
         * -------------------------------------------------------------------------
         */

        IEnumerable<CatalogItem> webOnly = webFeed.Except(marketFeed, bySku);
        IEnumerable<CatalogItem> marketOnly =
            from item in marketFeed.Except(webFeed, bySku)
            select item;

        IEnumerable<CatalogItem> webOnlyByKey =
            webFeed.ExceptBy(marketFeed.Select(item => item.Sku), item => item.Sku);

        Console.WriteLine("--- Except ---");
        Console.WriteLine("Web Except Market (Web-only SKUs):");
        foreach (CatalogItem item in webOnly)
        {
            Console.WriteLine($"  {item}");
        }

        Console.WriteLine("Market Except Web (new Marketplace listings):");
        foreach (CatalogItem item in marketOnly)
        {
            Console.WriteLine($"  {item}");
        }

        Console.WriteLine($"ExceptBy(Sku) Web-only count: {webOnlyByKey.Count()}");

        IEnumerable<int> aisleA = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 11, 52, 56, 23 };
        IEnumerable<int> aisleB = new List<int> { 4, 5, 6, 85, 63, 23, 55, 23, 87, 62, 872, 23, 52, 12 };
        IEnumerable<int> inANotB = aisleA.Except(aisleB); // primitive Except — default int equality

        Console.WriteLine();
        Console.WriteLine("int Except (in aisle A not in aisle B):");
        PrintJoined(inANotB);
        Console.WriteLine();

        /*
         * =========================================================================
         * SECTION 9: Union vs Concat — SET MERGE VS APPEND (PREVIEW)
         * =========================================================================
         *
         *  Operator | Duplicates              | Typical use
         *  ---------|-------------------------|-------------------------------
         *  Union    | Removed (set merge)     | Master unique membership list
         *  Concat   | Kept (append)           | Preserve every row from both
         *
         * Concat is taught fully in 12. Generation Operations — shown here only
         * so Union's "unique" contract is obvious against a familiar alternative.
         *
         * COVERED IN DETAIL LATER → 12. Generation Operations
         * -------------------------------------------------------------------------
         */

        IEnumerable<string> unionTags = webTags.Union(marketTags);
        IEnumerable<string> concatTags = webTags.Concat(marketTags); // keeps every tag, including duplicates

        Console.WriteLine("--- Union vs Concat (string tags) ---");
        Console.WriteLine($"Union count:  {unionTags.Count()}  → {string.Join(", ", unionTags)}");
        Console.WriteLine($"Concat count: {concatTags.Count()}  → {string.Join(", ", concatTags)}");
        Console.WriteLine();

        /*
         * =========================================================================
         * SECTION 10: SequenceEqual — BRIEF EQUALITY CHECK (PREVIEW)
         * =========================================================================
         *
         * SequenceEqual compares two sequences element-by-element (order and
         * count matter). Optional IEqualityComparer<T> uses the same contract
         * as Distinct/Union — useful here to verify two set pipelines agree.
         *
         *   left.SequenceEqual(right)
         *   left.SequenceEqual(right, comparer)
         *
         * Full depth with Any / All / Contains lives in Quantifier Operations.
         *
         * COVERED IN DETAIL LATER → 09. Quantifier Operations
         * -------------------------------------------------------------------------
         */

        bool distinctMatchesBy =
            uniqueWebSkus.SequenceEqual(uniqueWebByKey, bySku); // same SKUs, same order of first hit
        bool intersectPipelinesAgree =
            sharedListings.SequenceEqual(sharedListingsQuery, bySku);

        Console.WriteLine("--- Preview: SequenceEqual (verify set pipelines) ---");
        Console.WriteLine($"Distinct(bySku) vs DistinctBy(Sku): {distinctMatchesBy}");
        Console.WriteLine($"Intersect method vs query:          {intersectPipelinesAgree}");
        Console.WriteLine("(Full SequenceEqual depth → 09. Quantifier Operations)");
        Console.WriteLine();

        /*
         * =========================================================================
         * SECTION 11: METHOD SYNTAX VS QUERY SYNTAX — SUMMARY
         * =========================================================================
         *
         * C# has no distinct/union/intersect/except query keywords (those exist
         * in VB.NET). Invoke the extension methods directly or from a from clause:
         *
         *   from tag in webTags.Intersect(marketTags) select tag
         *
         * Side-by-side on primitive tags (default equality):
         * -------------------------------------------------------------------------
         */

        IEnumerable<string> methodUnion = webTags.Union(marketTags);
        IEnumerable<string> queryUnion =
            from tag in webTags.Union(marketTags)
            select tag;

        IEnumerable<string> methodIntersect = webTags.Intersect(marketTags);
        IEnumerable<string> queryIntersect =
            from tag in webTags.Intersect(marketTags)
            select tag;

        IEnumerable<string> methodExcept = webTags.Except(marketTags);
        IEnumerable<string> queryExcept =
            from tag in webTags.Except(marketTags)
            select tag;

        IEnumerable<string> queryDistinct =
            from tag in webTags.Distinct()
            select tag;

        Console.WriteLine("--- Method vs query syntax (string tags) ---");
        Console.WriteLine($"Union equal:        {methodUnion.SequenceEqual(queryUnion)}");
        Console.WriteLine($"Intersect equal:    {methodIntersect.SequenceEqual(queryIntersect)}");
        Console.WriteLine($"Except equal:       {methodExcept.SequenceEqual(queryExcept)}");
        Console.WriteLine("Query with Distinct in from clause:");
        PrintJoined(queryDistinct);

        Console.WriteLine();
        Console.WriteLine("=== Set ops summary ===");
        Console.WriteLine($"  Unique Web (by Sku):     {uniqueWebSkus.Count()}");
        Console.WriteLine($"  Master catalog (Union):  {masterCatalog.Count()}");
        Console.WriteLine($"  Shared (Intersect):      {sharedListings.Count()}");
        Console.WriteLine($"  Web-only (Except):       {webOnly.Count()}");
        Console.WriteLine($"  Market-only (Except):    {marketOnly.Count()}");
    }

    private static void PrintJoined(IEnumerable<int> values)
    {
        Console.WriteLine(string.Join(" ", values));
    }

    private static void PrintJoined(IEnumerable<string> values)
    {
        Console.WriteLine(string.Join(", ", values));
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — LINQ SET OPERATIONS
 * =============================================================================
 *
 * --- Classic operators (method syntax on IEnumerable<T>) ---
 *
 *   source.Distinct()
 *   source.Distinct(comparer)
 *
 *   first.Union(second)
 *   first.Union(second, comparer)
 *
 *   first.Intersect(second)
 *   first.Intersect(second, comparer)
 *
 *   first.Except(second)
 *   first.Except(second, comparer)
 *
 * --- *By operators (.NET 6+) — uniqueness / membership by key ---
 *
 *   source.DistinctBy(x => x.Key)
 *   first.UnionBy(second, x => x.Key)
 *   first.IntersectBy(secondKeys, x => x.Key)   // second arg = keys, not elements
 *   first.ExceptBy(secondKeys, x => x.Key)      // second arg = keys, not elements
 *
 * --- Query syntax (C#) ---
 *
 *   No distinct/union/intersect/except keywords — call methods in from:
 *
 *   from x in first.Union(second) select x
 *   from x in first.Intersect(second) select x
 *   from x in first.Except(second) select x
 *   from x in source.Distinct() select x
 *
 * --- Set meaning ---
 *
 *   Distinct / DistinctBy     unique items in one sequence
 *   Union / UnionBy           A ∪ B — every unique element from either
 *   Intersect / IntersectBy   A ∩ B — in both
 *   Except / ExceptBy         A \ B — in first, not in second
 *
 * --- IEqualityComparer<T> ---
 *
 *   bool Equals(T? x, T? y);
 *   int GetHashCode(T obj);
 *
 *   Equal objects MUST share hash codes. Prefer BCL comparers when possible:
 *   StringComparer.OrdinalIgnoreCase, EqualityComparer<T>.Default.
 *   Prefer DistinctBy / *By when identity is a single key field.
 *
 * --- Execution ---
 *
 *   All set operators are deferred — nothing runs until enumeration.
 *
 * --- Common mistakes ---
 *
 *  Mistake                                      | Result
 *  ---------------------------------------------|----------------------------------
 *  GetHashCode ignores fields used in Equals    | Wrong Distinct/Union/Intersect/Except
 *  Expect value equality on classes w/o comparer| Duplicate "equal" objects remain
 *  Confuse Union with Concat                    | Concat keeps duplicates (ch.12)
 *  Case-sensitive string sets on user input     | "API" and "api" treated as different
 *  Assume Except keeps duplicate rows from first| Except already collapses duplicates
 *  Pass whole elements to IntersectBy/ExceptBy  | Second arg must be a key sequence
 *
 * --- Related chapters ---
 *
 *   01. Introduction to LINQ     — deferred execution, IEnumerable<T>
 *   06. Element Operations       — First, Single, ElementAt
 *   09. Quantifier Operations    — Any, All, Contains; SequenceEqual depth
 *   12. Generation Operations    — Empty, Range, Repeat; Concat vs Union
 *
 * =============================================================================
 */
