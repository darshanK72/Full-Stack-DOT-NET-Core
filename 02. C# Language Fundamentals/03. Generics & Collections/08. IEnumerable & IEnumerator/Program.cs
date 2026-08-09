/*
 * =============================================================================
 * 08. IEnumerable AND IEnumerator IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: IEnumerable<T> and IEnumerator<T> — the contracts that power foreach,
 *        manual iteration, yield return iterators, and custom enumerable types.
 *
 * WHY IT MATTERS:
 *   Almost every collection API in .NET — List<T>, Dictionary<K,V>, arrays, LINQ
 *   pipelines — exposes sequences through IEnumerable<T>. Understanding how foreach
 *   desugars to GetEnumerator / MoveNext / Current lets you write lazy generators,
 *   custom collections, and debug "collection modified" errors with confidence.
 *
 * WHAT YOU WILL LEARN:
 *   1.  IEnumerable<T> vs IEnumerator<T> — sequence vs cursor
 *   2.  foreach desugaring (GetEnumerator, MoveNext, Current, Dispose)
 *   3.  Manual iteration with using / IDisposable
 *   4.  MoveNext, Current, and Reset (legacy)
 *   5.  yield return — compiler-built iterator methods
 *   6.  Custom collection implementing IEnumerable<T>
 *   7.  Non-generic IEnumerable / IEnumerator (brief)
 *   8.  InvalidOperationException — modifying a collection during foreach
 *   9.  Preview: LINQ as IEnumerable<T> extension methods
 *
 * Scenario: warehouse pick ticket PB-2201 — lines, weights, and aisle batches.
 *
 * =============================================================================
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IEnumerableAndIEnumerator;

/*
 * =========================================================================
 * SECTION 1: PickLine — ONE LINE ON A PICK TICKET
 * =========================================================================
 *
 * Domain type for this chapter. PickLine holds SKU, quantity, and unit weight.
 * IEnumerable demos walk collections of PickLine — the element type T in
 * IEnumerable<PickLine> and IEnumerator<PickLine>.
 * -------------------------------------------------------------------------
 */
public class PickLine
{
    public PickLine(string sku, int quantity, decimal weightKg)
    {
        Sku = sku;
        Quantity = quantity;
        WeightKg = weightKg;
    }

    public string Sku { get; }

    public int Quantity { get; }

    public decimal WeightKg { get; }

    public decimal TotalWeightKg => Quantity * WeightKg; // computed property for weight checks

    public override string ToString()
    {
        return $"{Sku} × {Quantity} ({TotalWeightKg:0.##} kg)";
    }
}

/*
 * =========================================================================
 * SECTION 2: IEnumerable<T> VS IEnumerator<T>
 * =========================================================================
 *
 * Two interfaces cooperate — one describes the SEQUENCE, the other is the CURSOR
 * that walks it.
 *
 *   Interface          | Role                         | Key members
 *   -------------------|------------------------------|----------------------------
 *   IEnumerable<T>     | "You can foreach me"         | GetEnumerator()
 *   IEnumerator<T>     | Forward-only read cursor     | MoveNext(), Current, Dispose
 *
 * IEnumerable<T> is implemented by List<T>, arrays, Dictionary<K,V>, HashSet<T>,
 * custom types like PickBatch (below), and lazy sequences from yield return.
 *
 * IEnumerator<T> is what GetEnumerator() returns. foreach (and manual loops) call
 * MoveNext() to advance; when it returns true, Current holds the element at the
 * new position.
 *
 * --- 2a. foreach desugaring ---
 *
 *   foreach (PickLine line in pickList)
 *   {
 *       // use line
 *   }
 *
 * The compiler rewrites that into roughly:
 *
 *   IEnumerator<PickLine> enumerator = pickList.GetEnumerator();
 *   try
 *   {
 *       while (enumerator.MoveNext())
 *       {
 *           PickLine line = enumerator.Current;
 *           // use line
 *       }
 *   }
 *   finally
 *   {
 *       enumerator?.Dispose();
 *   }
 *
 * --- 2b. IEnumerable<T> extends IEnumerable ---
 *
 * Generic IEnumerable<T> also implements non-generic IEnumerable so legacy APIs
 * (ArrayList era, some COM interop) can still call GetEnumerator() returning
 * IEnumerator with object Current.
 *
 * --- 2c. Rules and pitfalls ---
 *
 *   • foreach iteration variable is read-only — you cannot assign to it.
 *   • IEnumerable<T> is forward-only traversal — not random access by index
 *     (IList<T> and arrays add indexing separately).
 *   • Do not add or remove items from a mutable collection while foreach runs
 *     on that same instance — InvalidOperationException (Section 8 demo).
 *
 * PickBatch (next section) implements both IEnumerable<PickLine> and the
 * explicit non-generic IEnumerable.GetEnumerator().
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 3: CUSTOM ENUMERABLE — PickBatch + HAND-WRITTEN IEnumerator<T>
 * =========================================================================
 *
 * When a type owns its own storage rules, implement IEnumerable<T> and return
 * a custom IEnumerator<T> from GetEnumerator().
 *
 * PickBatch wraps a PickLine[] and exposes lines in aisle order through
 * PickBatchEnumerator — you write MoveNext / Current yourself instead of using
 * yield return inside the collection class.
 *
 * Compare approaches:
 *
 *   Approach                 | Best for
 *   -------------------------|------------------------------------------------
 *   List<T> + foreach        | General-purpose mutable storage
 *   yield return method      | Lazy filters, pipelines, one-off sequences
 *   Custom IEnumerator class | Full control, special traversal order/rules
 *
 * --- 3a. IEnumerator<T> members ---
 *
 *   Member      | Purpose
 *   ------------|------------------------------------------------------------
 *   MoveNext()  | Advance cursor; returns false when no more elements
 *   Current     | Element at current position (throws if invalid — see pitfall)
 *   Reset()     | Legacy — rewinds to before first element; prefer fresh
 *                 GetEnumerator() in modern code
 *   Dispose()   | Release resources (files, DB readers); foreach always
 *                 calls Dispose in a finally block
 *
 * --- 3b. Explicit non-generic interface implementation ---
 *
 * IEnumerator.Current (non-generic) returns object. PickBatchEnumerator implements
 * it explicitly so legacy code using IEnumerator still works:
 *
 *   object IEnumerator.Current => Current!;
 * -------------------------------------------------------------------------
 */
public class PickBatch : IEnumerable<PickLine>
{
    private readonly PickLine[] _lines;

    public PickBatch(params PickLine[] lines)
    {
        _lines = lines;
    }

    public int LineCount => _lines.Length;

    public IEnumerator<PickLine> GetEnumerator()
    {
        return new PickBatchEnumerator(_lines); // fresh cursor per call
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator(); // non-generic IEnumerable contract
    }

    /*
     * Hand-written cursor for PickBatch — mirrors what foreach compiles to.
     */
    private sealed class PickBatchEnumerator : IEnumerator<PickLine>
    {
        private readonly PickLine[] _lines;
        private int _index = -1; // -1 = before first element (MoveNext advances to 0)

        public PickBatchEnumerator(PickLine[] lines)
        {
            _lines = lines;
        }

        public PickLine Current => _lines[_index]; // valid only after MoveNext returned true

        object IEnumerator.Current => Current!; // explicit non-generic Current → object

        public bool MoveNext()
        {
            _index++;
            return _index < _lines.Length;
        }

        public void Reset()
        {
            _index = -1; // legacy rewind — modern code prefers new GetEnumerator()
        }

        public void Dispose()
        {
            // Nothing to release for this in-memory array walk.
        }
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 4: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Sections 4a–4h map to the learning goals in the file intro. Output at the
     * end prints one summary block per section.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        // --- 4a. foreach on List<PickLine> (IEnumerable<T> in practice) ---
        List<PickLine> pickList = new List<PickLine>
        {
            new PickLine("BOLT-M8", 120, 0.02m),
            new PickLine("BRACKET-12", 24, 0.45m),
            new PickLine("PANEL-A", 6, 3.10m),
            new PickLine("GASKET-RING", 200, 0.01m)
        };

        decimal foreachTotalKg = 0m;
        StringBuilder foreachLines = new StringBuilder();

        foreach (PickLine line in pickList) // compiler desugars to GetEnumerator / MoveNext
        {
            foreachTotalKg += line.TotalWeightKg;
            foreachLines.AppendLine($"  {line}");
        }

        // --- 4b. Manual IEnumerator<T> with using (IDisposable) ---
        decimal manualTotalKg = 0m;
        int manualLineCount = 0;
        PickLine? heaviestLine = null;

        using (IEnumerator<PickLine> manualWalk = pickList.GetEnumerator()) // using → Dispose
        {
            while (manualWalk.MoveNext())
            {
                PickLine line = manualWalk.Current;
                manualLineCount++;
                manualTotalKg += line.TotalWeightKg;

                if (heaviestLine == null || line.TotalWeightKg > heaviestLine.TotalWeightKg)
                {
                    heaviestLine = line;
                }
            }
        } // Dispose called here even if loop breaks early

        // --- 4c. Reset() on custom enumerator (legacy pattern) ---
        PickBatch resetDemoBatch = new PickBatch(
            new PickLine("FUSE-5A", 10, 0.05m),
            new PickLine("RELAY-24V", 4, 0.12m));

        int firstPassCount = 0;
        int secondPassCount = 0;
        StringBuilder resetPassSummary = new StringBuilder();

        IEnumerator<PickLine> resetWalk = resetDemoBatch.GetEnumerator();
        while (resetWalk.MoveNext())
        {
            firstPassCount++;
        }

        resetWalk.Reset(); // rewind — prefer GetEnumerator() again in new code

        while (resetWalk.MoveNext())
        {
            secondPassCount++;
            resetPassSummary.Append(resetWalk.Current.Sku);
            resetPassSummary.Append(", ");
        }

        resetWalk.Dispose();

        if (resetPassSummary.Length >= 2)
        {
            resetPassSummary.Length -= 2;
        }

        // --- 4d. yield return iterators (Section 5 methods) ---
        decimal minHeavyKg = 1.0m;
        int heavyCount = 0;
        StringBuilder heavySummary = new StringBuilder();

        foreach (PickLine heavy in HeavyLines(pickList, minHeavyKg)) // lazy — runs on demand
        {
            heavyCount++;
            heavySummary.AppendLine($"  {heavy.Sku} ({heavy.TotalWeightKg:0.##} kg)");
        }

        string panelSku = "PANEL-A";
        StringBuilder segmentSummary = new StringBuilder();

        foreach (string segment in SkuSegments(panelSku))
        {
            segmentSummary.Append(segment);
            segmentSummary.Append(" | ");
        }

        if (segmentSummary.Length >= 3)
        {
            segmentSummary.Length -= 3;
        }

        // --- 4e. foreach on custom PickBatch ---
        PickBatch batch = new PickBatch(
            new PickLine("WIRE-18G", 3, 0.8m),
            new PickLine("CLAMP-4", 12, 0.15m),
            new PickLine("TAPE-3M", 2, 0.35m));

        decimal batchTotalKg = 0m;
        StringBuilder batchLines = new StringBuilder();

        foreach (PickLine line in batch) // calls PickBatch.GetEnumerator() → PickBatchEnumerator
        {
            batchTotalKg += line.TotalWeightKg;
            batchLines.AppendLine($"  {line}");
        }

        // --- 4f. Non-generic IEnumerable / IEnumerator (brief) ---
        IEnumerable nonGenericSource = pickList; // List<PickLine> implements IEnumerable
        int nonGenericBoxedCount = 0;
        StringBuilder nonGenericSkus = new StringBuilder();

        IEnumerator legacyWalk = nonGenericSource.GetEnumerator(); // IEnumerator, not <T>
        try
        {
            while (legacyWalk.MoveNext())
            {
                object boxed = legacyWalk.Current!; // Current is object — may box value types
                if (boxed is PickLine pickLine)
                {
                    nonGenericBoxedCount++;
                    nonGenericSkus.Append(pickLine.Sku);
                    nonGenericSkus.Append(", ");
                }
            }
        }
        finally
        {
            if (legacyWalk is IDisposable disposable)
            {
                disposable.Dispose(); // non-generic IEnumerator is not IDisposable — concrete types often are
            }
        }

        if (nonGenericSkus.Length >= 2)
        {
            nonGenericSkus.Length -= 2;
        }

        // --- 4g. Collection modified during enumeration ---
        List<PickLine> mutableCopy = new List<PickLine>(pickList);
        string modificationMessage = "not triggered";
        int linesBeforeModify = 0;

        try
        {
            foreach (PickLine line in mutableCopy)
            {
                linesBeforeModify++;
                if (linesBeforeModify == 2)
                {
                    mutableCopy.Add(new PickLine("EXTRA-SKU", 1, 0.5m)); // invalidates enumerator
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            modificationMessage = ex.Message; // "Collection was modified; enumeration operation may not execute."
        }

        // --- 4h. LINQ preview on IEnumerable<T> ---
        decimal linqThresholdKg = 0.5m;
        int linqHeavyCount = pickList
            .Where(line => line.TotalWeightKg > linqThresholdKg) // extension on IEnumerable<T>
            .Count();

        // --- Output — pick ticket summary ---
        Console.WriteLine("=== Pick ticket PB-2201 — IEnumerable walkthrough ===");
        Console.WriteLine();

        Console.WriteLine("--- Section 4a: foreach on List<PickLine> ---");
        Console.WriteLine($"Lines: {pickList.Count}; total weight: {foreachTotalKg:0.##} kg");
        Console.WriteLine(foreachLines.ToString().TrimEnd());

        Console.WriteLine("--- Section 4b: manual IEnumerator<T> with using ---");
        Console.WriteLine($"Walked {manualLineCount} lines; total: {manualTotalKg:0.##} kg");
        Console.WriteLine($"Heaviest: {heaviestLine}");

        Console.WriteLine("--- Section 4c: Reset() on custom enumerator ---");
        Console.WriteLine($"First pass: {firstPassCount} line(s); after Reset: {secondPassCount} line(s)");
        Console.WriteLine($"Second pass SKUs: {resetPassSummary}");

        Console.WriteLine("--- Section 4d: yield return iterators ---");
        Console.WriteLine($"Heavy (>={minHeavyKg} kg): {heavyCount} line(s)");
        Console.WriteLine(heavySummary.ToString().TrimEnd());
        Console.WriteLine($"SkuSegments(\"{panelSku}\"): {segmentSummary}");

        Console.WriteLine("--- Section 4e: custom PickBatch ---");
        Console.WriteLine($"Batch lines: {batch.LineCount}; total: {batchTotalKg:0.##} kg");
        Console.WriteLine(batchLines.ToString().TrimEnd());

        Console.WriteLine("--- Section 4f: non-generic IEnumerable / IEnumerator ---");
        Console.WriteLine($"Unboxed {nonGenericBoxedCount} PickLine(s): {nonGenericSkus}");

        Console.WriteLine("--- Section 4g: modify during foreach ---");
        Console.WriteLine($"Lines read before exception: {linesBeforeModify}");
        Console.WriteLine($"InvalidOperationException: {modificationMessage}");

        Console.WriteLine("--- Section 4h: LINQ preview (Where + Count) ---");
        Console.WriteLine($"Lines over {linqThresholdKg} kg total: {linqHeavyCount}");
    }

    /*
     * =========================================================================
     * SECTION 5: yield return — ITERATOR METHODS
     * =========================================================================
     *
     * An ITERATOR METHOD returns IEnumerable<T> or IEnumerator<T> and uses
     * yield return to produce elements lazily — one at a time, on demand.
     *
     *   static IEnumerable<PickLine> HeavyLines(...)
     *   {
     *       foreach (PickLine line in source)
     *       {
     *           if (line.TotalWeightKg >= minKg)
     *               yield return line;   // pause; resume on next MoveNext
     *       }
     *   }
     *
     * The compiler synthesizes a state machine class implementing
     * IEnumerator<T> — you do not write MoveNext yourself for simple filters.
     *
     * --- 5a. Lazy evaluation ---
     *
     * No work runs until someone foreach-es or calls GetEnumerator(). Each
     * yield return hands one element; execution pauses until the consumer
     * asks for the next.
     *
     * --- 5b. yield break ---
     *
     * yield break; ends iteration early (like return in a normal method body).
     * SkuSegments uses a guard instead — empty segments are skipped.
     *
     * --- 5c. When to use ---
     *
     *   • Filter / transform sequences without building List<T> upfront
     *   • Infinite or unbounded sequences (e.g. date ranges — with a stop condition)
     *   • Readable named pipelines alongside LINQ (Section 4h preview)
     * -------------------------------------------------------------------------
     */
    private static IEnumerable<PickLine> HeavyLines(IEnumerable<PickLine> source, decimal minKg)
    {
        foreach (PickLine line in source)
        {
            if (line.TotalWeightKg >= minKg)
            {
                yield return line; // compiler-generated MoveNext resumes here
            }
        }
    }

    private static IEnumerable<string> SkuSegments(string sku)
    {
        string[] parts = sku.Split('-');

        foreach (string part in parts)
        {
            if (part.Length > 0)
            {
                yield return part;
            }
        }
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — IEnumerable / IEnumerator
 * =============================================================================
 *
 *  IEnumerable<T>              "sequence" — GetEnumerator() → IEnumerator<T>
 *  IEnumerator<T>              "cursor" — MoveNext(), Current, Dispose
 *
 *  foreach (T x in seq) { }     compiler: GetEnumerator → while MoveNext → Current
 *
 *  Manual (preferred modern):
 *    using IEnumerator<T> e = seq.GetEnumerator();
 *    while (e.MoveNext()) { T x = e.Current; }
 *
 *  Reset()                       legacy rewind — prefer new GetEnumerator()
 *  yield return item;            lazy iterator (compiler state machine)
 *  yield break;                  stop iterator early
 *
 *  Custom type:
 *    class Foo : IEnumerable<T>
 *    { public IEnumerator<T> GetEnumerator() { ... } }
 *
 *  Non-generic:                  IEnumerable / IEnumerator — Current is object
 *
 *  Pitfall:                      List.Add/Remove during foreach →
 *                                InvalidOperationException
 *
 *  LINQ (preview):               seq.Where(...).Select(...).Count()
 *    COVERED IN DETAIL LATER → 04. Functional Style Programming (Extension Methods)
 *    COVERED IN DETAIL LATER → 05. Language Integrated Query
 *
 * =============================================================================
 */
