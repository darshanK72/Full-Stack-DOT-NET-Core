/*
 * =============================================================================
 * 06. ELEMENT OPERATIONS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: LINQ element operators — methods that return a single item from a
 *        sequence (or a safe default). First/Last by position, Single when
 *        you expect exactly one match, ElementAt by zero-based index, and
 *        DefaultIfEmpty when downstream code needs at least one element.
 *
 * WHY IT MATTERS:
 *   Business rules often ask for "the first overdue invoice," "the only
 *   admin on the account," or "line item at index 3." Element operators
 *   express those rules in one readable call instead of manual index checks
 *   and loop flags. Choosing the wrong operator (First vs Single) or the
 *   strict vs OrDefault variant is a common source of runtime bugs — this
 *   chapter maps each method to its exception behavior so you pick safely.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Element-operator overview — immediate execution, strict vs OrDefault
 *   2.  First / FirstOrDefault — first element, optional predicate, empty cases
 *   3.  Last / LastOrDefault — last element (full scan), optional predicate
 *   4.  Single / SingleOrDefault — exactly-one semantics and uniqueness throws
 *   5.  ElementAt / ElementAtOrDefault — zero-based index and System.Index
 *   6.  DefaultIfEmpty — guarantee a non-empty sequence for aggregates / UI
 *   7.  Exception map — InvalidOperationException vs ArgumentOutOfRangeException
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace ElementOperations;

/*
 * =========================================================================
 * SECTION 1: SAMPLE DOMAIN — InvoiceStatus AND Invoice
 * =========================================================================
 *
 * A small clinic billing registry drives every demo below. Status values let
 * predicates match zero, one, or many invoices so First / Single / Last behave
 * differently on the same source:
 *
 *  Status     | Count in seed | Useful for
 *  -----------|---------------|----------------------------------------------
 *  Paid       | 2             | First/Last with multiple matches
 *  Overdue    | 3             | Single throws on "more than one"
 *  Pending    | 1             | Single succeeds (exactly one)
 *  Cancelled  | 0             | OrDefault / DefaultIfEmpty empty cases
 *
 * Invoice is a sealed record (reference type) — default(Invoice) is null, which
 * is what FirstOrDefault / ElementAtOrDefault return when nothing matches.
 * For value-type defaults (0), demos use int[] amounts later.
 * -------------------------------------------------------------------------
 */
public enum InvoiceStatus
{
    Pending,
    Paid,
    Overdue,
    Cancelled,
}

public sealed record Invoice(
    string Id,
    string PatientId,
    string Description,
    decimal Amount,
    InvoiceStatus Status,
    int DaysOverdue);

public class Program
{
    /*
     * =========================================================================
     * SECTION 2: ELEMENT OPERATORS — OVERVIEW (Main orchestrates the demos)
     * =========================================================================
     *
     * Element operators answer "give me ONE item from this sequence."
     * They are extension methods on IEnumerable<T> in System.Linq.
     *
     *  Family          | Strict (throws)     | Safe (default)              | Typical question
     *  ----------------|---------------------|-----------------------------|---------------------------
     *  Position        | First, Last         | FirstOrDefault, LastOrDefault | First/last match?
     *  Uniqueness      | Single              | SingleOrDefault             | Exactly one match?
     *  Index           | ElementAt           | ElementAtOrDefault          | Item at index n?
     *  Empty fallback  | —                   | DefaultIfEmpty              | Never empty downstream
     *
     * Immediate execution — the operator runs when you call it (unlike
     * Where/Select chains that defer until enumeration).
     *
     * Query syntax has NO keywords for these operators — always method syntax.
     *
     * Seed data gaps are intentional:
     *   • no Cancelled rows → OrDefault / DefaultIfEmpty empty paths
     *   • exactly one Pending → Single succeeds
     *   • three Overdue → Single throws; First/Last pick ends of the match set
     *
     * COVERED IN DETAIL LATER → 09. Quantifier Operations
     *   Use Any / All when you only need yes/no, not the element itself.
     *
     * COVERED IN DETAIL LATER → 11. Partitioning Operations
     *   Use Take / Skip for slices; ElementAt picks a single index.
     *
     * COVERED IN DETAIL LATER → 05. Joins
     *   DefaultIfEmpty also powers left outer joins after GroupJoin.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Invoice[] invoices =
        [
            new Invoice("INV-1001", "P-001", "Annual Checkup", 125.00m, InvoiceStatus.Paid, DaysOverdue: 0),
            new Invoice("INV-1002", "P-002", "Lab Panel", 340.00m, InvoiceStatus.Overdue, DaysOverdue: 45),
            new Invoice("INV-1003", "P-003", "Specialist Referral", 890.00m, InvoiceStatus.Overdue, DaysOverdue: 12),
            new Invoice("INV-1004", "P-001", "Follow-up Visit", 95.00m, InvoiceStatus.Paid, DaysOverdue: 0),
            new Invoice("INV-1005", "P-004", "Imaging — MRI", 2100.00m, InvoiceStatus.Pending, DaysOverdue: 0),
            new Invoice("INV-1006", "P-002", "Physical Therapy (6 sessions)", 480.00m, InvoiceStatus.Overdue, DaysOverdue: 30),
        ];

        Invoice[] emptyInvoices = []; // empty source for strict-operator demos

        Console.WriteLine("=== Clinic billing registry (source) ===");
        PrintInvoices(invoices);


        /*
         * =========================================================================
         * SECTION 3: First AND FirstOrDefault
         * =========================================================================
         *
         * First()            — first element of the sequence
         * First(predicate)   — first element where predicate is true
         *
         * --- Exception behavior (strict First) ---
         *
         *  Condition                              | Exception
         *  ---------------------------------------|----------------------------------
         *  Sequence is empty                      | InvalidOperationException
         *  No element matches predicate           | InvalidOperationException
         *
         * Messages:
         *   empty     → "Sequence contains no elements"
         *   no match  → "Sequence contains no matching element"
         *
         * FirstOrDefault / FirstOrDefault(predicate) never throw for empty or
         * no-match — they return default(T):
         *
         *   int → 0     string → null     Invoice (class) → null
         *
         * Overloads with an explicit defaultValue (.NET Core 2.0+ / .NET 8):
         *   FirstOrDefault(predicate, defaultValue)
         * return your fallback object instead of null / 0.
         *
         * Use First when absence is a bug. Use FirstOrDefault when "maybe none"
         * is normal. First short-circuits — stops at the first match (O(k)).
         * -------------------------------------------------------------------------
         */

        Invoice firstInList = invoices.First(); // no predicate — first row in file order
        Invoice firstOverdue = invoices.First(inv => inv.Status == InvoiceStatus.Overdue); // short-circuits

        Console.WriteLine();
        Console.WriteLine("--- First (strict) ---");
        Console.WriteLine($"First invoice in file: {firstInList.Id} ({firstInList.Description})");
        Console.WriteLine($"First overdue invoice: {firstOverdue.Id} — {firstOverdue.DaysOverdue} days overdue");

        DemonstrateException(
            "First() on empty sequence",
            () => emptyInvoices.First());

        DemonstrateException(
            "First(predicate) when nothing matches",
            () => invoices.First(inv => inv.Amount > 5000m));

        Invoice? noHighValue = invoices.FirstOrDefault(inv => inv.Amount > 5000m); // null when none
        Invoice? emptyFirst = emptyInvoices.FirstOrDefault(); // empty source → null (reference type)
        Invoice fallbackHighValue = invoices.FirstOrDefault(
            inv => inv.Amount > 5000m,
            new Invoice("INV-NONE", "—", "(no high-value invoice)", 0m, InvoiceStatus.Pending, 0));

        Console.WriteLine();
        Console.WriteLine("--- FirstOrDefault (safe) ---");
        Console.WriteLine(
            noHighValue is null
                ? "No invoice over $5,000 — FirstOrDefault returned null (default for Invoice)."
                : noHighValue.Id);
        Console.WriteLine(
            emptyFirst is null
                ? "Empty sequence — FirstOrDefault() returned null."
                : emptyFirst.Id);
        Console.WriteLine($"FirstOrDefault with defaultValue: {fallbackHighValue.Id} — {fallbackHighValue.Description}");


        /*
         * =========================================================================
         * SECTION 4: Last AND LastOrDefault
         * =========================================================================
         *
         * Last() / Last(predicate) mirror First but return the LAST matching
         * element. For plain IEnumerable<T> the implementation walks the entire
         * sequence (O(n)) — there is no short-circuit for "last."
         *
         * Exception rules match First: InvalidOperationException when empty or
         * no predicate match. LastOrDefault returns default(T) instead.
         *
         * LastOrDefault(predicate, defaultValue) supplies a custom fallback —
         * same idea as FirstOrDefault's defaultValue overload.
         *
         * Prefer Last only when order is meaningful (sorted exports, append-only
         * logs). If you truly need "most recent by date," OrderByDescending then
         * First is often clearer — see ch.03 Ordering.
         * -------------------------------------------------------------------------
         */

        Invoice lastInList = invoices.Last(); // full scan of the source
        Invoice lastOverdue = invoices.Last(inv => inv.Status == InvoiceStatus.Overdue); // last match in file order

        Console.WriteLine();
        Console.WriteLine("--- Last (strict) ---");
        Console.WriteLine($"Last invoice in file: {lastInList.Id} ({lastInList.Description})");
        Console.WriteLine($"Last overdue invoice: {lastOverdue.Id} — {lastOverdue.DaysOverdue} days overdue");

        DemonstrateException(
            "Last() on empty sequence",
            () => emptyInvoices.Last());

        DemonstrateException(
            "Last(predicate) when nothing matches",
            () => invoices.Last(inv => inv.Status == InvoiceStatus.Cancelled));

        Invoice? lastCancelled = invoices.LastOrDefault(inv => inv.Status == InvoiceStatus.Cancelled);
        Invoice lastCancelledFallback = invoices.LastOrDefault(
            inv => inv.Status == InvoiceStatus.Cancelled,
            new Invoice("INV-NONE", "—", "(no cancelled invoice)", 0m, InvoiceStatus.Cancelled, 0));

        Console.WriteLine();
        Console.WriteLine("--- LastOrDefault (safe) ---");
        Console.WriteLine(
            lastCancelled is null
                ? "No cancelled invoices — LastOrDefault returned null."
                : lastCancelled.Id);
        Console.WriteLine($"LastOrDefault with defaultValue: {lastCancelledFallback.Id} — {lastCancelledFallback.Description}");


        /*
         * =========================================================================
         * SECTION 5: Single AND SingleOrDefault
         * =========================================================================
         *
         * Single enforces UNIQUENESS — use when business rules say "there must
         * be exactly one."
         *
         * --- Exception behavior (strict Single) ---
         *
         *  Condition                                | Exception
         *  -----------------------------------------|----------------------------------
         *  Zero matches (empty or no predicate hit) | InvalidOperationException
         *  More than one match                      | InvalidOperationException
         *
         * Messages include "no elements" / "no matching element" and
         * "more than one element" / "more than one matching element".
         *
         * SingleOrDefault allows ZERO matches (returns default(T)) but STILL
         * throws InvalidOperationException when MORE THAN ONE element matches.
         *
         *  Matches | Single              | SingleOrDefault
         *  --------|---------------------|----------------------------------
         *  0       | throws              | returns default(T)
         *  1       | returns that item   | returns that item
         *  2+      | throws              | throws
         *
         * SingleOrDefault(predicate, defaultValue) returns your fallback when
         * there are zero matches — it still throws on two or more matches.
         *
         * Do NOT use Single when duplicates are possible — use First, or
         * Distinct / GroupBy first if uniqueness must be enforced.
         * -------------------------------------------------------------------------
         */

        Invoice onlyPending = invoices.Single(inv => inv.Status == InvoiceStatus.Pending); // exactly one Pending

        Console.WriteLine();
        Console.WriteLine("--- Single (strict — exactly one expected) ---");
        Console.WriteLine($"Only pending invoice: {onlyPending.Id} — {onlyPending.Description}");

        DemonstrateException(
            "Single() on empty sequence",
            () => emptyInvoices.Single());

        DemonstrateException(
            "Single() on sequence with more than one element",
            () => invoices.Single());

        DemonstrateException(
            "Single(predicate) when multiple match",
            () => invoices.Single(inv => inv.Status == InvoiceStatus.Overdue));

        Invoice? singleCancelled = invoices.SingleOrDefault(inv => inv.Status == InvoiceStatus.Cancelled);
        Invoice singleCancelledFallback = invoices.SingleOrDefault(
            inv => inv.Status == InvoiceStatus.Cancelled,
            new Invoice("INV-NONE", "—", "(no cancelled invoice)", 0m, InvoiceStatus.Cancelled, 0));

        Console.WriteLine();
        Console.WriteLine("--- SingleOrDefault (zero OK, duplicates not OK) ---");
        Console.WriteLine(
            singleCancelled is null
                ? "Zero cancelled invoices — SingleOrDefault returned null."
                : singleCancelled.Id);
        Console.WriteLine($"SingleOrDefault with defaultValue: {singleCancelledFallback.Id}");

        DemonstrateException(
            "SingleOrDefault(predicate) when multiple match",
            () => invoices.SingleOrDefault(inv => inv.Status == InvoiceStatus.Overdue));


        /*
         * =========================================================================
         * SECTION 6: ElementAt AND ElementAtOrDefault
         * =========================================================================
         *
         * ElementAt(index) returns the element at a zero-based position by
         * advancing the enumerator index times — O(n) for plain IEnumerable<T>.
         * Arrays and IList<T> implementations use O(1) indexer access internally.
         *
         * Overloads also accept System.Index (from-end syntax with ^):
         *   ElementAt(^1)  → last element
         *   ElementAt(^2)  → second-to-last
         *
         * --- Exception behavior (strict ElementAt) ---
         *
         *  Condition                    | Exception
         *  -----------------------------|----------------------------------
         *  index < 0                    | ArgumentOutOfRangeException
         *  index >= sequence length     | ArgumentOutOfRangeException
         *  Index from end past length   | ArgumentOutOfRangeException
         *
         * ElementAtOrDefault(index) returns default(T) when the index is out of
         * range instead of throwing — useful for "maybe there is a third row."
         * Value types return 0 / false / etc.; reference types return null.
         *
         * Prefer list[index] / array[index] when you already have random access.
         * ElementAt still helps on deferred LINQ pipelines you have not
         * materialized yet.
         * -------------------------------------------------------------------------
         */

        Invoice thirdInvoice = invoices.ElementAt(2); // zero-based — third row
        Invoice lastByIndex = invoices.ElementAt(^1); // System.Index — last element
        int[] amounts = invoices.Select(inv => (int)inv.Amount).ToArray();
        int missingAmount = amounts.ElementAtOrDefault(30); // value type → 0 when out of range

        Console.WriteLine();
        Console.WriteLine("--- ElementAt (strict index) ---");
        Console.WriteLine($"Invoice at index 2 (third row): {thirdInvoice.Id} — {thirdInvoice.Description}");
        Console.WriteLine($"Invoice at ^1 (last via Index): {lastByIndex.Id} — {lastByIndex.Description}");

        DemonstrateException(
            "ElementAt(30) when only 6 invoices exist",
            () => invoices.ElementAt(30));

        DemonstrateException(
            "ElementAt(-1) with negative index",
            () => invoices.ElementAt(-1));

        Invoice? outOfRange = invoices.ElementAtOrDefault(30);
        Invoice? fromEndMiss = invoices.ElementAtOrDefault(^20); // Index past length → null

        Console.WriteLine();
        Console.WriteLine("--- ElementAtOrDefault (safe index) ---");
        Console.WriteLine(
            outOfRange is null
                ? "Index 30 out of range — ElementAtOrDefault returned null (reference type)."
                : outOfRange.Id);
        Console.WriteLine(
            fromEndMiss is null
                ? "Index ^20 out of range — ElementAtOrDefault returned null."
                : fromEndMiss.Id);
        Console.WriteLine($"ElementAtOrDefault(30) on int[] amounts → {missingAmount} (default(int) = 0)");


        /*
         * =========================================================================
         * SECTION 7: DefaultIfEmpty
         * =========================================================================
         *
         * DefaultIfEmpty() returns a NEW sequence:
         *   • source has ≥ 1 element → same elements (pass-through)
         *   • source is EMPTY        → a one-element sequence containing default(T)
         *
         * DefaultIfEmpty(defaultValue) supplies a custom fallback instead of
         * default(T) — critical for reference types where null would break
         * downstream code (foreach, property access, binding).
         *
         * Does NOT throw on empty source — that is its purpose.
         *
         * Common pattern: .DefaultIfEmpty(0m).Average() so empty filters do not
         * throw InvalidOperationException from Average on an empty sequence.
         *
         * In joins, DefaultIfEmpty after GroupJoin builds a left outer join —
         * depth lives in 05. Joins; here we focus on empty-sequence guarantees.
         * -------------------------------------------------------------------------
         */

        IEnumerable<Invoice> paidOnly = invoices.Where(inv => inv.Status == InvoiceStatus.Paid);
        IEnumerable<Invoice> cancelledOnly = invoices.Where(inv => inv.Status == InvoiceStatus.Cancelled);

        Invoice placeholder = new Invoice(
            "INV-0000",
            "—",
            "(no cancelled invoices on file)",
            0m,
            InvoiceStatus.Cancelled,
            DaysOverdue: 0);

        IEnumerable<Invoice?> cancelledWithNullDefault = cancelledOnly.DefaultIfEmpty(); // yields { null }
        IEnumerable<Invoice> cancelledWithPlaceholder = cancelledOnly.DefaultIfEmpty(placeholder);

        Console.WriteLine();
        Console.WriteLine("--- DefaultIfEmpty ---");
        Console.WriteLine($"Paid invoices (non-empty — DefaultIfEmpty passes source through): {paidOnly.Count()}");
        foreach (Invoice inv in paidOnly.DefaultIfEmpty(placeholder))
        {
            Console.WriteLine($"  {inv.Id}  {inv.Amount:C}");
        }

        Console.WriteLine();
        Console.WriteLine("Cancelled filter empty — DefaultIfEmpty() yields one null element:");
        foreach (Invoice? inv in cancelledWithNullDefault)
        {
            Console.WriteLine(inv is null ? "  (null — default(Invoice))" : $"  {inv.Id}");
        }

        Console.WriteLine();
        Console.WriteLine("Cancelled filter empty — DefaultIfEmpty(placeholder) yields one synthetic row:");
        foreach (Invoice inv in cancelledWithPlaceholder)
        {
            Console.WriteLine($"  {inv.Id}  {inv.Description}");
        }

        decimal averageOverdueBalance = invoices
            .Where(inv => inv.Status == InvoiceStatus.Overdue)
            .Select(inv => inv.Amount)
            .DefaultIfEmpty(0m) // empty filter → Average would throw without this
            .Average();

        decimal averageCancelledBalance = invoices
            .Where(inv => inv.Status == InvoiceStatus.Cancelled)
            .Select(inv => inv.Amount)
            .DefaultIfEmpty(0m)
            .Average();

        Console.WriteLine();
        Console.WriteLine($"Average overdue balance (DefaultIfEmpty(0m) before Average): {averageOverdueBalance:C}");
        Console.WriteLine($"Average cancelled balance (empty → DefaultIfEmpty(0m)):     {averageCancelledBalance:C}");


        /*
         * =========================================================================
         * SECTION 8: CHOOSING THE RIGHT OPERATOR
         * =========================================================================
         *
         *  Need                                           | Operator
         *  -----------------------------------------------|----------------------------
         *  Earliest overdue bill (must exist)             | First(predicate)
         *  Maybe no bill over $5k                         | FirstOrDefault(predicate)
         *  Most recent overdue in file order (must exist) | Last(predicate)
         *  Exactly one pending bill on file               | Single(predicate)
         *  Lookup by row number in today's export         | ElementAt(index)
         *  Aggregate when filter might return nothing     | DefaultIfEmpty then Sum/Avg
         *  Only yes/no "does any exist?"                  | Any (ch.09) — not First
         *
         * Variables from earlier sections are reused below so every result is
         * observed — no unused locals.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine();
        Console.WriteLine("=== Operator choice summary ===");
        Console.WriteLine($"  First in file:        {firstInList.Id}");
        Console.WriteLine($"  First overdue:        {firstOverdue.Id}");
        Console.WriteLine($"  Last in file:         {lastInList.Id}");
        Console.WriteLine($"  Last overdue:         {lastOverdue.Id}");
        Console.WriteLine($"  Only pending:         {onlyPending.Id}");
        Console.WriteLine($"  At index 2:           {thirdInvoice.Id}");
        Console.WriteLine($"  At ^1 (last):         {lastByIndex.Id}");
        Console.WriteLine($"  High-value (OrDef):   {(noHighValue is null ? "(none)" : noHighValue.Id)}");
        Console.WriteLine($"  Cancelled (OrDef):    {(lastCancelled is null ? "(none)" : lastCancelled.Id)}");
        Console.WriteLine($"  Fallback high-value:  {fallbackHighValue.Id}");
        Console.WriteLine($"  Fallback cancelled:   {lastCancelledFallback.Id} / {singleCancelledFallback.Id}");
    }

    /*
     * =========================================================================
     * SECTION 9: HELPERS — EXCEPTION DEMO AND PRINT
     * =========================================================================
     *
     * DemonstrateException runs a strict operator inside try/catch so the
     * program can show InvalidOperationException / ArgumentOutOfRangeException
     * without terminating. PrintInvoices dumps the registry with zero-based
     * indices — useful when reading ElementAt demos.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateException(string scenario, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"  [{scenario}] — unexpected: no exception thrown.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [{scenario}] → {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void PrintInvoices(IEnumerable<Invoice> items)
    {
        int index = 0;
        foreach (Invoice inv in items)
        {
            Console.WriteLine(
                $"  [{index,2}] {inv.Id}  {inv.Status,-8}  {inv.Amount,9:C}  {inv.Description}");
            index++;
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — ELEMENT OPERATIONS
 * =========================================================================
 *
 * --- Strict vs safe pairs ---
 *
 *  Strict (throws)          | Safe (returns default(T))     | Throws when
 *  -------------------------|-------------------------------|------------------
 *  First()                  | FirstOrDefault()              | empty / no match
 *  Last()                   | LastOrDefault()               | empty / no match
 *  Single()                 | SingleOrDefault()             | 0 or 2+ matches*
 *  ElementAt(n) / ElementAt(^n) | ElementAtOrDefault(...)   | index out of range
 *
 *  * SingleOrDefault: throws only on 2+ matches; zero matches → default(T)
 *
 * --- Predicates ---
 *
 *   .First(x => …) / .Last(x => …) / .Single(x => …)
 *   Same OrDefault overloads accept a predicate. Empty source and "no match"
 *   share the same exception family for First/Last/Single.
 *
 * --- Custom defaultValue overloads ---
 *
 *   FirstOrDefault(pred, value) / LastOrDefault(pred, value) /
 *   SingleOrDefault(pred, value) — fallback when zero matches (Single* still
 *   throws on 2+).
 *
 * --- DefaultIfEmpty ---
 *
 *   source.DefaultIfEmpty()           // empty → sequence { default(T) }
 *   source.DefaultIfEmpty(fallback)   // empty → sequence { fallback }
 *
 * --- Exception types ---
 *
 *  Operator / condition                         | Exception
 *  ---------------------------------------------|----------------------------
 *  First, Last, Single — empty or no match      | InvalidOperationException
 *  Single, SingleOrDefault — more than one      | InvalidOperationException
 *  ElementAt — bad index                        | ArgumentOutOfRangeException
 *  FirstOrDefault, LastOrDefault,               | (none — returns default)
 *    SingleOrDefault (0 matches),              |
 *    ElementAtOrDefault, DefaultIfEmpty         |
 *
 * --- default(T) reminders ---
 *
 *  Value types (int, decimal) → 0 / 0m
 *  Reference types            → null
 *  nullable value types       → null
 *
 * --- Performance notes ---
 *
 *  First / FirstOrDefault(predicate) — stops at first match (short-circuit)
 *  Last / LastOrDefault(predicate)   — must enumerate entire sequence
 *  ElementAt(n) on IEnumerable       — O(n); prefer [index] on arrays/lists
 *  Single                            — may scan for a second match after finding one
 *
 * --- Related chapters ---
 *
 *   01. Introduction to LINQ     — deferred vs immediate execution
 *   02. Filtering & Aggregation  — Where before First; Count vs Any
 *   03. Ordering                 — OrderByDescending + First for "latest"
 *   05. Joins                    — DefaultIfEmpty in left outer joins
 *   09. Quantifier Operations    — Any / All for existence without picking
 *   11. Partitioning Operations  — Take / Skip for slices instead of one item
 *
 * =========================================================================
 */
