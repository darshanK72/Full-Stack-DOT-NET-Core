/*
 * =============================================================================
 * 12. GENERATION OPERATIONS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: LINQ operators that CREATE sequences without an existing source
 *        collection — Enumerable.Range, Repeat, and Empty<T> — plus how
 *        custom yield-return generators relate to the same idea. DefaultIfEmpty
 *        is introduced only as a generation-adjacent preview (full depth in
 *        Element Operations).
 *
 * WHY IT MATTERS:
 *   Before LINQ you wrote for-loops to build index lists, pad arrays with
 *   defaults, or return "no results" as null. Generation operators express
 *   those patterns as lazy IEnumerable<T> pipelines — composable with Where,
 *   Select, and the rest of LINQ without allocating intermediate lists unless
 *   you materialize.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Domain model for the training-portal demo
 *   2.  Custom yield generators — same lazy idea as BCL factories
 *   3.  Generation vs transformation — static Enumerable factories
 *   4.  Enumerable.Range — consecutive integers (count ≠ end index)
 *   5.  Enumerable.Repeat — N copies of one value (reference identity pitfall)
 *   6.  Enumerable.Empty<T> — typed zero-length sequence (prefer over null)
 *   7.  Deferred execution and chaining generated sequences with LINQ
 *   8.  Preview — DefaultIfEmpty → 06. Element Operations
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace GenerationOperations;

/*
 * =========================================================================
 * SECTION 1: DOMAIN TYPES — TRAINING PORTAL MODEL
 * =========================================================================
 *
 * Small types for the weekly training-portal demo. TrainingLevel labels skill
 * bands; Enrollment is an immutable row used with Empty, Where, and the
 * DefaultIfEmpty preview later in Main.
 * -------------------------------------------------------------------------
 */
public enum TrainingLevel
{
    Beginner,
    Intermediate,
    Advanced,
}

public readonly record struct Enrollment(
    string EmployeeId,
    string DisplayName,
    TrainingLevel Level,
    int AssessmentScore = 0); // optional score — Average + DefaultIfEmpty preview

/*
 * =========================================================================
 * SECTION 2: CUSTOM GENERATORS WITH yield return
 * =========================================================================
 *
 * Enumerable.Range / Repeat / Empty are BCL factories. You can build the same
 * kind of lazy sequence yourself with an iterator method:
 *
 *   public static IEnumerable<int> EvensUpTo(int max)
 *   {
 *       for (int n = 0; n <= max; n += 2)
 *           yield return n;   // pause; resume on next MoveNext
 *   }
 *
 * Rules that matter for generation:
 *   • The method returns IEnumerable<T> (or IEnumerator<T>)
 *   • Body runs only when enumerated — deferred, like Range
 *   • Each yield return hands one element to the consumer
 *   • count / bounds checks can throw at call time OR first MoveNext
 *     depending on where you put them (Range validates count immediately)
 *
 * Prefer BCL generators when they fit (Range/Repeat/Empty). Write yield
 * generators when the pattern is domain-specific (odd steps, calendar days,
 * synthetic IDs). Demo wiring is in Main (Section 8). Iterator mechanics:
 * COVERED IN DETAIL LATER → 03. Generics & Collections / 08. IEnumerable & IEnumerator
 * -------------------------------------------------------------------------
 */
public static class SlotGenerators
{
    /*
     * --- 2a. Stepped integers (Range cannot do step ≠ 1) ---
     *
     * Enumerable.Range always steps by +1. For every other session id, a
     * custom generator is clearer than Range(...).Where(n => n % 2 == 0).
     */
    public static IEnumerable<int> EvenSessionIds(int firstSession, int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count)); // fail fast — like Range

        int emitted = 0;
        int current = firstSession % 2 == 0 ? firstSession : firstSession + 1; // snap to even
        while (emitted < count)
        {
            yield return current; // one even id per MoveNext
            current += 2;
            emitted++;
        }
    }

    /*
     * --- 2b. Domain labels from a numeric range ---
     *
     * Projects generated numbers into display strings without allocating a
     * List first — same lazy shape as Enumerable.Range(...).Select(...).
     */
    public static IEnumerable<string> SessionLabels(int start, int count)
    {
        foreach (int n in Enumerable.Range(start, count)) // compose BCL Range inside yield
            yield return $"LAB-{n:D3}"; // zero-padded lab code
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 3: GENERATION OPERATORS — OVERVIEW
         * =========================================================================
         *
         * Most LINQ operators are *extension methods* on IEnumerable<T> — they take
         * an existing sequence and return a new one (Where, Select, OrderBy).
         *
         * Generation operators are different: they are *static methods* on
         * System.Linq.Enumerable and fabricate a sequence from parameters alone:
         *
         *  Method                    | Produces
         *  --------------------------|----------------------------------------------
         *  Enumerable.Range(s, c)    | s, s+1, … for c integers
         *  Enumerable.Repeat(v, c)   | v repeated c times
         *  Enumerable.Empty<T>()     | zero elements, typed as IEnumerable<T>
         *
         * All three return IEnumerable<T> and execute lazily — nothing is stored until
         * you foreach, Count, ToList, etc.
         *
         * Related (extension, not static generation): DefaultIfEmpty — if a filtered
         * sequence has no rows, yield one default so Average / reports still run
         * (Section 10 preview). Full element-operator catalog:
         * COVERED IN DETAIL LATER → 06. Element Operations
         *
         * Scenario: a corporate training portal assigns session numbers, open-seat
         * placeholders, and per-course enrollment lists for a weekly report.
         * -------------------------------------------------------------------------
         */

        const int sessionsThisWeek = 5;
        const int seatsPerSession = 4;

        Console.WriteLine("=== Training portal — weekly session report ===");
        Console.WriteLine();

        /*
         * =========================================================================
         * SECTION 4: Enumerable.Range — CONSECUTIVE INTEGERS
         * =========================================================================
         *
         *   IEnumerable<int> Range(int start, int count)
         *
         * Generates count consecutive integers beginning at start (inclusive).
         *
         *  Call                      | Elements emitted
         *  --------------------------|------------------------------------------
         *  Range(1, 5)               | 1, 2, 3, 4, 5
         *  Range(0, 3)               | 0, 1, 2          ← zero-based indexes
         *  Range(10, 1)              | 10               ← count = 1 → single item
         *  Range(5, 0)               | (empty)          ← count zero → nothing
         *
         * --- 4a. Count is NOT the end index ---
         *
         * Range(2, 4) means "start at 2, emit 4 numbers" → 2, 3, 4, 5.
         * It does NOT mean "from 2 to 4". Inclusive 2..4 needs
         * Range(2, 3) or Range(2, 4 - 2 + 1).
         *
         * --- 4b. Invalid arguments ---
         *
         * count < 0 throws ArgumentOutOfRangeException at call time.
         * Overflow of the inclusive end (start + count - 1 > int.MaxValue)
         * also throws ArgumentOutOfRangeException.
         *
         * --- 4c. Typical uses ---
         *
         *   • 1-based session / page numbers for UI labels
         *   • 0-based indexes paired with another collection via Zip/Select
         *   • Building small lookup tables without a literal array
         *   • Replacing for (int i = start; i < start + count; i++)
         * -------------------------------------------------------------------------
         */

        IEnumerable<int> sessionNumbers = Enumerable.Range(1, sessionsThisWeek); // 1..5

        Console.WriteLine("--- Enumerable.Range — session numbers ---");
        foreach (int session in sessionNumbers)
        {
            Console.WriteLine($"  Session {session} scheduled");
        }

        // Zero-based indexes for parallel arrays — Range + Select (Select depth in ch.08)
        string[] roomCodes = { "A-101", "B-204", "C-310", "D-115", "E-220" };
        IEnumerable<string> sessionRoomLines = Enumerable.Range(0, roomCodes.Length)
            .Select(i => $"  Session {i + 1} → room {roomCodes[i]}"); // i is 0-based index

        Console.WriteLine();
        Console.WriteLine("--- Range(0, n) for zero-based pairing ---");
        foreach (string line in sessionRoomLines)
        {
            Console.WriteLine(line);
        }

        // Inclusive end formula — "sessions 3 through 5" → start=3, count=3
        int inclusiveStart = 3;
        int inclusiveEnd = 5;
        IEnumerable<int> inclusiveWindow = Enumerable.Range(
            inclusiveStart,
            inclusiveEnd - inclusiveStart + 1); // count = end - start + 1

        Console.WriteLine();
        Console.WriteLine("--- Range with inclusive end (3..5) ---");
        Console.WriteLine($"  {string.Join(", ", inclusiveWindow)}");

        // Argument validation — negative count fails immediately (before foreach)
        Console.WriteLine();
        Console.WriteLine("--- Range invalid count ---");
        try
        {
            _ = Enumerable.Range(1, -1); // throws at call site
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"  ArgumentOutOfRangeException: {ex.ParamName}");
        }

        /*
         * =========================================================================
         * SECTION 5: Enumerable.Repeat — SAME VALUE, N TIMES
         * =========================================================================
         *
         *   IEnumerable<T> Repeat<T>(T element, int count)
         *
         * Emits element exactly count times.
         *
         *  Call                      | Elements emitted
         *  --------------------------|------------------------------------------
         *  Repeat("Open", 3)         | "Open", "Open", "Open"
         *  Repeat(0, 5)              | 0, 0, 0, 0, 0
         *  Repeat('x', 0)            | (empty)
         *
         * --- 5a. Reference types share one instance ---
         *
         * Repeat returns the SAME object reference each time — not clones.
         * Mutating one "slot" after ToList affects every yielded reference.
         *
         * --- 5b. Value types are independent copies ---
         *
         * Each int, decimal, struct, etc. is its own value when consumed.
         *
         * --- 5c. Negative count ---
         *
         * count < 0 throws ArgumentOutOfRangeException.
         *
         * --- 5d. Typical uses ---
         *
         *   • Placeholder labels ("Unassigned", "N/A") for unfilled slots
         *   • Initializing a sequence of zeros before aggregation
         *   • Padding length to match another collection (often with Zip/Concat)
         * -------------------------------------------------------------------------
         */

        const string openSeatLabel = "Open";

        IEnumerable<string> seatPlaceholders = Enumerable.Repeat(openSeatLabel, seatsPerSession);

        Console.WriteLine();
        Console.WriteLine("--- Enumerable.Repeat — open seat placeholders ---");
        int seatIndex = 1;
        foreach (string seat in seatPlaceholders)
        {
            Console.WriteLine($"  Seat {seatIndex++}: {seat}");
        }

        // Value-type repeats — each yield is an independent int
        int[] zeroPad = Enumerable.Repeat(0, 3).ToArray(); // materialize score pad
        zeroPad[0] = 99; // does not change the other slots

        Console.WriteLine();
        Console.WriteLine("--- Repeat value types (independent copies) ---");
        Console.WriteLine($"  Pad after mutate index 0: {string.Join(", ", zeroPad)}");

        // Reference-type identity — one shared List instance repeated twice
        List<string> sharedRoster = new List<string> { "Alice" };
        IEnumerable<List<string>> repeatedRefs = Enumerable.Repeat(sharedRoster, 2);
        List<List<string>> materializedRefs = repeatedRefs.ToList(); // materialize to mutate
        materializedRefs[0].Add("Bob"); // mutates sharedRoster — both slots see Bob

        Console.WriteLine();
        Console.WriteLine("--- Repeat reference identity (both slots share one List) ---");
        Console.WriteLine($"  Slot 0 count: {materializedRefs[0].Count}, Slot 1 count: {materializedRefs[1].Count}");
        Console.WriteLine($"  Same reference: {object.ReferenceEquals(materializedRefs[0], materializedRefs[1])}");

        // Pad seats when enrollments are short — Concat + Repeat
        string[] confirmedNames = { "Priya", "Marcus" };
        IEnumerable<string> fullSeatRow = confirmedNames
            .Concat(Enumerable.Repeat(openSeatLabel, seatsPerSession - confirmedNames.Length));

        Console.WriteLine();
        Console.WriteLine("--- Repeat as padding after Concat ---");
        Console.WriteLine($"  Seats: {string.Join(" | ", fullSeatRow)}");

        /*
         * =========================================================================
         * SECTION 6: Enumerable.Empty<T> — TYPED EMPTY SEQUENCE
         * =========================================================================
         *
         *   IEnumerable<T> Empty<T>()
         *
         * Returns a cached, read-only empty sequence for type T. Count is always 0;
         * foreach runs zero iterations.
         *
         * --- 6a. Prefer Empty over null ---
         *
         * Returning null from a method that promises IEnumerable<T> forces every
         * caller to null-check before foreach or LINQ. Empty<T>() is a valid
         * "no data" answer that composes safely:
         *
         *   var lines = GetEnrollments(courseId).Select(e => e.DisplayName);
         *   // works whether GetEnrollments returns items or Empty<Enrollment>()
         *
         * --- 6b. Same instance per T ---
         *
         * Empty<int>() and another Empty<int>() share one singleton empty
         * array internally — do not cast to mutable collections.
         *
         * --- 6c. Empty vs Array.Empty / new T[0] ---
         *
         *  Expression                 | Type              | Notes
         *  --------------------------|-------------------|--------------------------------
         *  Enumerable.Empty<T>()     | IEnumerable<T>    | LINQ-friendly factory
         *  Array.Empty<T>()          | T[]               | zero-length array singleton
         *  new List<T>()             | List<T>           | mutable; allocates a list shell
         *
         * Prefer Empty/Array.Empty for "no rows" returns. Prefer new List only when
         * the caller must Add later.
         *
         * --- 6d. Typing empty in LINQ pipelines ---
         *
         * Useful when a branch must return IEnumerable<T> with no rows — guard
         * clauses, optional join results, or Concat with a generated empty side.
         * -------------------------------------------------------------------------
         */

        IEnumerable<Enrollment> csharpEnrollments = GetEnrollmentsForCourse("CS-401");
        IEnumerable<Enrollment> retiredCourseEnrollments = GetEnrollmentsForCourse("RET-000");

        Console.WriteLine();
        Console.WriteLine("--- Enumerable.Empty<T> — safe empty enrollments ---");
        PrintEnrollmentBlock("CS-401 (active)", csharpEnrollments);
        PrintEnrollmentBlock("RET-000 (retired)", retiredCourseEnrollments);

        int csharpCount = csharpEnrollments.Count();
        int retiredCount = retiredCourseEnrollments.Count();
        Console.WriteLine($"  Counts — CS-401: {csharpCount}, RET-000: {retiredCount}");

        // Singleton identity for Empty<T> — same cached instance per T
        IEnumerable<Enrollment> emptyA = Enumerable.Empty<Enrollment>();
        IEnumerable<Enrollment> emptyB = Enumerable.Empty<Enrollment>();
        Console.WriteLine($"  Empty<Enrollment> same instance: {object.ReferenceEquals(emptyA, emptyB)}");

        // Empty as a typed left/right side of Concat — still one IEnumerable<Enrollment>
        IEnumerable<Enrollment> combined = csharpEnrollments.Concat(Enumerable.Empty<Enrollment>());
        Console.WriteLine($"  Concat with Empty: still {combined.Count()} enrollment(s)");

        /*
         * =========================================================================
         * SECTION 7: STATIC CALL SYNTAX + DEFERRED EXECUTION
         * =========================================================================
         *
         * Range, Repeat, and Empty are NOT called on an existing variable:
         *
         *   // Wrong — no such instance method:
         *   // someList.Range(1, 5);
         *
         *   // Correct — static on Enumerable:
         *   Enumerable.Range(1, 5);
         *
         * This chapter always uses the Enumerable. prefix for clarity (no
         * `using static System.Linq.Enumerable`).
         *
         * Generated sequences are ordinary IEnumerable<T> — chain Where, Select,
         * Take, etc. Execution stays deferred until enumeration / materialization
         * (same deferred model as ch.01 Introduction to LINQ).
         * -------------------------------------------------------------------------
         */

        IEnumerable<int> evenSessionQuery = Enumerable.Range(1, sessionsThisWeek)
            .Where(n => n % 2 == 0); // still a recipe — not run yet

        int[] evenSessionIds = evenSessionQuery.ToArray(); // materialize — query runs here

        Console.WriteLine();
        Console.WriteLine("--- Chaining generated sequence with Where ---");
        Console.WriteLine($"  Even session ids: {string.Join(", ", evenSessionIds)}");

        // Re-enumeration — Range regenerates; no cached buffer unless you ToList
        int firstPassSum = Enumerable.Range(1, 3).Sum();
        int secondPassSum = Enumerable.Range(1, 3).Sum(); // fresh generation
        Console.WriteLine($"  Range(1,3).Sum() twice: {firstPassSum}, {secondPassSum}");

        /*
         * =========================================================================
         * SECTION 8: DEMO — CUSTOM yield GENERATORS
         * =========================================================================
         *
         * SlotGenerators (Section 2) shows two custom factories. Use them when BCL
         * Range/Repeat cannot express the pattern cleanly (step ≠ 1, domain labels).
         * -------------------------------------------------------------------------
         */

        Console.WriteLine();
        Console.WriteLine("--- Custom yield — EvenSessionIds(1, 3) ---");
        foreach (int id in SlotGenerators.EvenSessionIds(1, 3))
        {
            Console.WriteLine($"  Even session id: {id}");
        }

        Console.WriteLine();
        Console.WriteLine("--- Custom yield — SessionLabels via Range inside ---");
        foreach (string label in SlotGenerators.SessionLabels(1, sessionsThisWeek))
        {
            Console.WriteLine($"  {label}");
        }

        /*
         * =========================================================================
         * SECTION 9: COMPOSING GENERATION WITH OTHER LINQ
         * =========================================================================
         *
         * Generation is usually the *start* of a pipeline:
         *
         *   Enumerable.Range(...)
         *     .Select(...)     → projection (ch.08)
         *     .Where(...)      → filtering (ch.02)
         *     .Take(...)       → partitioning (ch.11)
         *     .ToList()        → conversion (ch.10)
         *
         * Below: build session cards from Range, then keep only afternoon labs.
         * -------------------------------------------------------------------------
         */

        var afternoonLabs = Enumerable.Range(1, sessionsThisWeek)
            .Select(n => new
            {
                Session = n,
                Room = roomCodes[n - 1],
                StartsAt = TimeSpan.FromHours(12 + n), // synthetic start hour
            })
            .Where(s => s.StartsAt.Hours >= 14) // afternoon filter
            .Take(2); // partitioning preview — depth in ch.11

        Console.WriteLine();
        Console.WriteLine("--- Compose Range → Select → Where → Take ---");
        foreach (var lab in afternoonLabs)
        {
            Console.WriteLine($"  Session {lab.Session} @ {lab.StartsAt:hh\\:mm} in {lab.Room}");
        }

        /*
         * =========================================================================
         * SECTION 10: PREVIEW — DefaultIfEmpty (ELEMENT OPERATIONS)
         * =========================================================================
         *
         * DefaultIfEmpty is an *extension* on an existing sequence. MSDN groups it
         * with element operations, but it is generation-adjacent: when the source
         * is empty it *produces* one element so the result sequence is never empty.
         *
         *   source.DefaultIfEmpty()           → { default(T) } if empty
         *   source.DefaultIfEmpty(fallback)   → { fallback } if empty
         *   Non-empty source                  → pass-through (no extra element)
         *
         * Classic use: Average / Sum after a filter that might match nothing —
         * DefaultIfEmpty(0) before Average avoids InvalidOperationException.
         *
         * COVERED IN DETAIL LATER → 06. Element Operations
         * -------------------------------------------------------------------------
         */

        IEnumerable<Enrollment> advancedOnly = csharpEnrollments
            .Where(e => e.Level == TrainingLevel.Advanced); // none in sample data

        Enrollment fallbackSummary = new Enrollment(
            "—",
            "No advanced enrollments",
            TrainingLevel.Advanced); // synthetic row for empty reports

        IEnumerable<Enrollment> reportRows = advancedOnly.DefaultIfEmpty(fallbackSummary);

        Console.WriteLine();
        Console.WriteLine("--- Preview: DefaultIfEmpty (full depth in ch.06) ---");
        foreach (Enrollment row in reportRows)
        {
            Console.WriteLine($"  {row.EmployeeId,-8} {row.DisplayName,-28} {row.Level}");
        }

        double averageScore = advancedOnly
            .Select(e => e.AssessmentScore)
            .DefaultIfEmpty(0) // empty → 0 so Average does not throw
            .Average();

        Console.WriteLine($"  Average advanced score (0 when none): {averageScore:F1}");
    }

    /*
     * =========================================================================
     * SECTION 11: HELPERS — EMPTY-SAFE LOOKUPS
     * =========================================================================
     *
     * GetEnrollmentsForCourse returns real rows for known codes, otherwise
     * Enumerable.Empty<Enrollment>() — never null — so callers can LINQ
     * immediately. PrintEnrollmentBlock renders a block and shows when Empty
     * was the source.
     * -------------------------------------------------------------------------
     */
    public static IEnumerable<Enrollment> GetEnrollmentsForCourse(string courseCode) =>
        courseCode switch
        {
            "CS-401" =>
            [
                new Enrollment("E-1001", "Priya Sharma", TrainingLevel.Intermediate, 88),
                new Enrollment("E-1002", "Marcus Lee", TrainingLevel.Beginner, 74),
                new Enrollment("E-1003", "Jordan Kim", TrainingLevel.Intermediate, 91),
            ],
            _ => Enumerable.Empty<Enrollment>(), // typed empty — not null
        };

    public static void PrintEnrollmentBlock(string heading, IEnumerable<Enrollment> enrollments)
    {
        Console.WriteLine($"  {heading}:");
        foreach (Enrollment e in enrollments)
        {
            Console.WriteLine($"    {e.EmployeeId}  {e.DisplayName}  ({e.Level})");
        }

        if (!enrollments.Any()) // Empty enumerates zero times — Any is false
        {
            Console.WriteLine("    (no rows — source was Enumerable.Empty<Enrollment>)");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — GENERATION OPERATIONS
 * =========================================================================
 *
 * --- Static generation (System.Linq.Enumerable) ---
 *
 *   Enumerable.Range(start, count)   → start, start+1, … (count integers)
 *   Enumerable.Repeat(value, count)  → value repeated count times
 *   Enumerable.Empty<T>()            → empty IEnumerable<T> (singleton)
 *
 * --- Parameter rules ---
 *
 *   count must be ≥ 0 for Range and Repeat (else ArgumentOutOfRangeException)
 *   Range: second arg is COUNT, not end index
 *   Inclusive a..b → Range(a, b - a + 1)
 *   Range: start + count - 1 must fit in int (else ArgumentOutOfRangeException)
 *   Repeat: reference types → same instance each yield
 *
 * --- Empty vs null ---
 *
 *   return Enumerable.Empty<Order>();   // composable, no null checks
 *   return Array.Empty<Order>();        // T[] singleton — also fine
 *   return null;                        // forces callers to guard
 *
 * --- Custom generation ---
 *
 *   IEnumerable<T> Method(...) { yield return item; … }
 *   Prefer Range/Repeat/Empty when they fit; yield for domain-specific steps
 *   Iterator depth → 03. Generics & Collections / 08. IEnumerable & IEnumerator
 *
 * --- DefaultIfEmpty (preview; FULL in 06. Element Operations) ---
 *
 *   source.DefaultIfEmpty()              → yields default(T) if source empty
 *   source.DefaultIfEmpty(fallback)      → yields fallback if source empty
 *   Non-empty source → pass-through (no extra element)
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Range(1, 10) expecting end index 10  | Off-by-one if you meant 1..10
 *  Treating Repeat as cloning objects   | Shared reference for reference types
 *  Returning null IEnumerable           | NullReferenceException in foreach
 *  Calling Range on a list instance     | CS1061 — use Enumerable.Range
 *
 * --- Related chapters ---
 *
 *   06. Element Operations      — DefaultIfEmpty, FirstOrDefault, SingleOrDefault
 *   08. Projection Operations   — Select after Range for indexed projections
 *   11. Partitioning Operations — Take/Skip after generated sequences
 *   08. IEnumerable & IEnumerator (module 03) — yield return iterators
 *
 * =========================================================================
 */
