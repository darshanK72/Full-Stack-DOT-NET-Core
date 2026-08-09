/*
 * TOPIC: Closures — when a lambda or anonymous method uses a variable from an
 *        enclosing scope, the compiler builds a hidden display class that holds
 *        that variable so the delegate can read and mutate it after the enclosing
 *        method returns.
 *
 * WHY IT MATTERS:
 *   Closures power LINQ, event handlers, timers, Task.Run callbacks, and factory
 *   functions. They are essential — and easy to misuse. Capturing a loop variable
 *   and running the delegate later is one of the most common C# interview bugs.
 *   Understanding capture rules prevents subtle defects in deferred queries, UI
 *   events, and background work.
 *
 * WHAT YOU WILL LEARN:
 *   1.  What capture and closure mean — display classes and shared storage
 *   2.  Lambdas and anonymous methods — both capture outer variables the same way
 *   3.  Reference capture vs value copy — modified outer locals vs loop fixes
 *   4.  Mutating captured variables and multiple delegates sharing one capture
 *   5.  Closure lifetime — locals that outlive the method that declared them
 *   6.  Memory and GC — what keeps captured objects alive on the heap
 *   7.  Classic for-loop + deferred delegate bug and safe copy patterns
 *   8.  foreach closure history — C# 5+ fix vs pre-C# 5 pitfall
 *   9.  Practical factory and counter patterns built on closures
 */

using System;
using System.Collections.Generic;

namespace Closures;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Closures — capture, lifetime, and loop pitfalls ===");
        Console.WriteLine();

        DemonstrateBasicCapture();
        DemonstrateLambdaAndAnonymousCapture();
        DemonstrateModifiedVsCopySemantics();
        DemonstrateMutatingSharedCapture();
        DemonstrateMultipleDelegatesOneCapture();
        DemonstrateClosureLifetime();
        DemonstrateMemoryAndGcRoots();
        DemonstrateForLoopPitfall();
        DemonstrateForeachClosureHistory();
        DemonstratePracticalPatterns();
    }

    /*
     * SECTION 1: WHAT IS CAPTURE? WHAT IS A CLOSURE?
     *
     * A lambda or anonymous method can reference variables declared OUTSIDE its
     * body — method locals, parameters, or instance fields. That reference is
     * called CAPTURE.
     *
     *  Term              | Meaning
     *  ------------------|---------------------------------------------------
     *  Enclosing scope   | Block where the variable is declared
     *  Capture           | Inner function uses a variable from outer scope
     *  Closure           | Delegate + captured environment kept alive together
     *  Display class     | Compiler-generated type holding captured fields
     *
     * Capture is by REFERENCE to shared storage — not a snapshot of the value at
     * delegate creation time (Section 3 expands modified-vs-copy semantics).
     *
     * Builds on lambda syntax from 02. Lambda Expressions (previewed capture
     * there; this chapter owns full depth).
     */
    private static void DemonstrateBasicCapture()
    {
        decimal alertThreshold = 50.00m;

        Func<decimal, bool> isBargain = price => price <= alertThreshold; // captures alertThreshold

        Console.WriteLine("--- Section 1: Basic capture ---");
        Console.WriteLine($"Threshold at creation: {alertThreshold:C}");
        Console.WriteLine($"Price 45.00 is bargain: {isBargain(45.00m)}");
        Console.WriteLine($"Price 55.00 is bargain: {isBargain(55.00m)}");

        alertThreshold = 40.00m; // outer local changed after delegate was created

        Console.WriteLine($"Threshold changed to: {alertThreshold:C}");
        Console.WriteLine($"Price 45.00 is bargain now: {isBargain(45.00m)}");
        Console.WriteLine("(Delegate reads current value — capture shares one storage slot.)");
        Console.WriteLine();
    }

    /*
     * SECTION 2: LAMBDAS AND ANONYMOUS METHODS BOTH CAPTURE
     *
     * Capture is not lambda-specific. Anonymous methods (delegate { }) from
     * 03. Anonymous Methods use the same display-class machinery.
     *
     *  Syntax form                         | Captures outer locals?
     *  ------------------------------------|----------------------------
     *  x => x + offset                     | Yes — lambda
     *  delegate (int x) { return x + offset; } | Yes — anonymous method
     *  Named method on Program             | Only if it reads instance fields
     *
     * Prefer lambdas for new code; recognize anonymous-method capture when
     * reading legacy WinForms/WPF handlers.
     */
    private static void DemonstrateLambdaAndAnonymousCapture()
    {
        int offset = 10;

        Func<int, int> addOffsetLambda = x => x + offset;

        Func<int, int> addOffsetAnonymous = delegate (int x)
        {
            return x + offset; // same captured `offset` field as the lambda
        };

        Console.WriteLine("--- Section 2: Lambda vs anonymous method capture ---");
        Console.WriteLine($"Lambda:       5 + offset = {addOffsetLambda(5)}");
        Console.WriteLine($"Anonymous:    5 + offset = {addOffsetAnonymous(5)}");

        offset = 20; // both delegates see the mutation

        Console.WriteLine($"After offset → {offset}: lambda={addOffsetLambda(5)}, anonymous={addOffsetAnonymous(5)}");
        Console.WriteLine();
    }

    /*
     * SECTION 3: MODIFIED OUTER LOCAL vs VALUE COPY
     *
     * Two different ideas confuse beginners:
     *
     *  Situation                              | Semantics
     *  ---------------------------------------|----------------------------------
     *  Capture a method local, mutate later   | Delegate sees LIVE value (shared)
     *  Capture a method local inside lambda   | Assignment inside λ mutates outer too
     *  for-loop index without copy fix        | All lambdas share ONE `i` variable
     *  Inner local `int copy = i` per iter    | Each λ gets its own `copy` slot
     *  Helper parameter `void F(int value)`   | Each call freezes current value
     *
     * Capture always means "same storage." A copy fix creates NEW storage per
     * iteration and captures that instead of the shared loop variable.
     */
    private static void DemonstrateModifiedVsCopySemantics()
    {
        int liveSlot = 1;
        Func<int> readLive = () => liveSlot;

        int frozenCopy = liveSlot; // independent variable — not captured yet
        Func<int> readFrozen = () => frozenCopy;

        Console.WriteLine("--- Section 3: Modified vs copy semantics ---");
        Console.WriteLine($"readLive(): {readLive()}, readFrozen(): {readFrozen()}");

        liveSlot = 99; // mutates shared capture for readLive only

        Console.WriteLine($"After liveSlot → 99: readLive()={readLive()}, readFrozen()={readFrozen()}");
        Console.WriteLine("(readFrozen captured its own `frozenCopy` field — not wired to liveSlot.)");
        Console.WriteLine();
    }

    /*
     * SECTION 4: MUTATING CAPTURED VARIABLES
     *
     * Assignments inside a lambda write to the same field the outer scope uses.
     * This pattern implements counters, accumulators, and stateful callbacks.
     *
     * Func/Action types from 05. Func Action and Predicate carry these lambdas.
     */
    private static void DemonstrateMutatingSharedCapture()
    {
        int matchCount = 0;
        decimal[] prices = { 12.50m, 88.00m, 33.25m, 91.10m, 27.00m };
        decimal filterLevel = 30.00m;

        Action<decimal> countAboveFilter = price =>
        {
            if (price > filterLevel)
            {
                matchCount++; // mutates captured matchCount visible to outer code
            }
        };

        Console.WriteLine("--- Section 4: Mutating captured variables ---");
        Console.WriteLine($"Filter level: {filterLevel:C}");

        foreach (decimal price in prices)
        {
            countAboveFilter(price);
        }

        Console.WriteLine($"Prices above {filterLevel:C}: {matchCount}");

        filterLevel = 80.00m;
        matchCount = 0;

        foreach (decimal price in prices)
        {
            countAboveFilter(price);
        }

        Console.WriteLine($"After filter → {filterLevel:C}, matches: {matchCount}");
        Console.WriteLine();
    }

    /*
     * SECTION 5: MULTIPLE DELEGATES SHARING ONE CAPTURE
     *
     * Every lambda that captures the same local shares ONE display-class field.
     * Order of invocation matters when several delegates mutate that field.
     */
    private static void DemonstrateMultipleDelegatesOneCapture()
    {
        int runningTotal = 0;

        Action<int> addToTotal = value => runningTotal += value;
        Func<int> readTotal = () => runningTotal;
        Action resetTotal = () => runningTotal = 0;

        Console.WriteLine("--- Section 5: Shared captured state ---");

        addToTotal(100);
        addToTotal(25);
        Console.WriteLine($"Total after two adds: {readTotal()}");

        resetTotal();
        Console.WriteLine($"Total after reset: {readTotal()}");

        addToTotal(7);
        addToTotal(3);
        Console.WriteLine($"Total after 7 + 3: {readTotal()}");
        Console.WriteLine();
    }

    /*
     * SECTION 6 (demo): CLOSURE LIFETIME — LOCALS OUTLIVE THE METHOD
     *
     * MakeCounter (defined above in ClosureFactories) returns while `count` still
     * exists on the heap. Each returned Func<int> keeps its display class alive.
     */
    private static void DemonstrateClosureLifetime()
    {
        Func<int> counterA = MakeCounter(10);
        Func<int> counterB = MakeCounter(10); // separate display class

        Console.WriteLine("--- Section 6: Closure lifetime ---");
        Console.WriteLine($"counterA: {counterA()}, {counterA()}, {counterA()}");
        Console.WriteLine($"counterB: {counterB()}, {counterB()}");
        Console.WriteLine("(MakeCounter returned; captured `count` still increments.)");
        Console.WriteLine();
    }

    /*
     * SECTION 7: MEMORY AND GC CONSIDERATIONS
     *
     * The garbage collector cannot collect a display class while ANY delegate that
     * references it is reachable (a GC root exists through the delegate chain).
     *
     *  Root chain                         | Effect
     *  -----------------------------------|------------------------------------
     *  Static field holding handler       | Captured graph kept for app lifetime
     *  Event += handler capturing `this`  | Entire object graph may stay alive
     *  Handler captures large buffer      | Whole buffer pinned until released
     *  handler = null / -= unsubscribe    | Allows GC of display class + captures
     *
     * Prefer capturing small identifiers (id, threshold) instead of large graphs
     * in long-lived handlers. Release delegates when work completes.
     */
    private static void DemonstrateMemoryAndGcRoots()
    {
        byte[] largeScratchBuffer = new byte[1024];
        largeScratchBuffer[0] = 42;

        Action logFirstByte = () => Console.WriteLine($"Buffer[0] = {largeScratchBuffer[0]}");

        Console.WriteLine("--- Section 7: Memory / GC roots ---");
        logFirstByte();
        Console.WriteLine("logFirstByte keeps largeScratchBuffer reachable until the delegate is released.");

        logFirstByte = null!; // drop reference — buffer becomes collectable (no other roots)
        Console.WriteLine("After nulling delegate, captured buffer can be collected.");
        Console.WriteLine();
    }

    /*
     * SECTION 8: FOR-LOOP + DEFERRED LAMBDA — THE CLASSIC BUG
     *
     * `for (int i = 0; ...)` declares ONE loop variable reused each iteration.
     * Lambdas stored for later capture the VARIABLE, not the value at Add() time.
     * When they run, they all see the final value of `i`.
     *
     *  When Add() runs (i = 0,1,2) | Value seen WHEN delegate runs later
     *  -----------------------------|---------------------------------------
     *  Same storage slot for i      | 3 (loop finished) for EVERY delegate
     *
     * This still happens in modern C# — only foreach was fixed (Section 9).
     *
     * --- 8a. Fix: copy to inner local each iteration ---
     * --- 8b. Fix: pass value into LoopCaptureHelpers.AddPrinter ---
     */
    private static void DemonstrateForLoopPitfall()
    {
        List<Action> brokenForLoopActions = new List<Action>();

        for (int i = 0; i < 3; i++)
        {
            brokenForLoopActions.Add(() => Console.Write($" {i}")); // captures shared `i`
        }

        Console.WriteLine("--- Section 8: for-loop + lambda bug ---");
        Console.Write("Broken (expected 0 1 2):");
        foreach (Action action in brokenForLoopActions)
        {
            action();
        }

        Console.WriteLine();

        List<Action> fixedForLoopActions = new List<Action>();

        for (int i = 0; i < 3; i++)
        {
            int capturedIndex = i; // new local per iteration — lambda captures this slot
            fixedForLoopActions.Add(() => Console.Write($" {capturedIndex}"));
        }

        Console.Write("Fixed (copy to inner local):");
        foreach (Action action in fixedForLoopActions)
        {
            action();
        }

        Console.WriteLine();

        List<Action> parameterizedActions = new List<Action>();

        for (int i = 0; i < 3; i++)
        {
            AddPrinter(parameterizedActions, i);
        }

        Console.Write("Fixed (parameter snapshot):");
        foreach (Action action in parameterizedActions)
        {
            action();
        }

        Console.WriteLine();
        Console.WriteLine();
    }

    /*
     * SECTION 9: FOREACH LOOP CLOSURE — C# 5+ vs EARLIER
     *
     * Before C# 5, `foreach (var item in items)` reused ONE `item` variable.
     * Deferred lambdas inside the loop captured that single slot — same bug as
     * Section 8, printing the last element repeatedly.
     *
     * C# 5 and later: compiler generates a fresh iteration variable per loop, so
     * capturing inside foreach works as beginners expect.
     *
     *  Loop form   | Loop variable scope      | Safe to capture in deferred λ?
     *  ------------|--------------------------|-------------------------------
     *  for         | One shared `i`           | NO — copy to inner local first
     *  foreach     | New per iteration (5+)   | YES on modern C#
     *  while       | Manual — same pitfalls   | Copy if reusing one variable
     *
     * When maintaining pre-2012 code or translating old blog posts, apply the
     * Section 8 copy fix inside foreach loops written for older compilers.
     */
    private static void DemonstrateForeachClosureHistory()
    {
        string[] tickers = { "MSFT", "AAPL", "GOOG" };
        List<Func<string>> tickerReaders = new List<Func<string>>();

        foreach (string ticker in tickers)
        {
            tickerReaders.Add(() => ticker); // safe on C# 5+ — per-iteration `ticker`
        }

        Console.WriteLine("--- Section 9: foreach capture (C# 5+) ---");
        Console.Write("Tickers from deferred readers:");
        foreach (Func<string> reader in tickerReaders)
        {
            Console.Write($" {reader()}");
        }

        Console.WriteLine();
        Console.WriteLine("(Each lambda captured its iteration's ticker variable.)");
        Console.WriteLine();
    }

    /*
     * SECTION 10: PRACTICAL CLOSURE PATTERNS
     *
     * --- 10a. Configurable rule factories (MakeMarginRule) ---
     * --- 10b. Deferred batch checks referencing earlier captured state ---
     *
     * Deferred LINQ queries (05. Language Integrated Query) also capture outer
     * variables — the same lifetime and loop rules apply when queries run later.
     */
    private static void DemonstratePracticalPatterns()
    {
        Func<decimal, bool> strictRule = MakeMarginRule(0.05m);
        Func<decimal, bool> relaxedRule = MakeMarginRule(0.15m);

        decimal quote = 100.00m;

        Console.WriteLine("--- Section 10: Practical patterns ---");
        Console.WriteLine($"Quote {quote:C} passes strict 5% margin: {strictRule(quote)}");
        Console.WriteLine($"Quote {quote:C} passes relaxed 15% margin: {relaxedRule(quote)}");

        int batchRunningTotal = 250;
        List<Action> batchCompletionChecks = new List<Action>();
        decimal batchLimit = 1000.00m;

        batchCompletionChecks.Add(() =>
            Console.WriteLine($"Batch within limit {batchLimit:C}: {batchRunningTotal <= batchLimit}"));

        batchCompletionChecks.Add(() =>
            Console.WriteLine($"Audit total: {batchRunningTotal}"));

        Console.WriteLine("Deferred batch checks:");
        foreach (Action check in batchCompletionChecks)
        {
            check();
        }

        Console.WriteLine();
    }

    /*
     * SECTION 6: CLOSURE LIFETIME — FACTORY DELEGATES
     *
     * When a method returns a delegate that captured a local, the local is NOT stored
     * on the stack after the method returns. The compiler promotes it to a field on a
     * compiler-generated display class allocated on the heap. The returned delegate
     * holds a reference to that display class.
     *
     * Each call to MakeCounter creates a NEW display class instance — independent state.
     *
     * Prior chapters:
     *   02. Lambda Expressions  — () => ++count is the lambda form used here
     *   03. Anonymous Methods   — delegate { return ++count; } would capture identically
     *   05. Func Action and Predicate — Func<int> is the return type
     */
    private static Func<int> MakeCounter(int start)
    {
        int count = start; // captured field — lives on heap inside display class
        return () => ++count;
    }

    /*
     * --- 6a. Margin rule factory ---
     *
     * Each returned Func<decimal, bool> closes over its own `minimum` field.
     * Calling MakeMarginRule twice yields two independent closures.
     */
    private static Func<decimal, bool> MakeMarginRule(decimal margin)
    {
        decimal minimum = 100.00m * (1.0m - margin); // captured once per factory call
        return price => price >= minimum;
    }

    /*
     * SECTION 8: FOR-LOOP FIX — PARAMETER SNAPSHOT HELPER
     *
     * Passing the loop index as a parameter gives each created lambda its own
     * parameter slot with the value evaluated at the call site — not the shared
     * loop variable `i`.
     */
    private static void AddPrinter(List<Action> sink, int value)
    {
        sink.Add(() => Console.Write($" {value}")); // `value` is a distinct param per call
    }
}

/*
 * QUICK REFERENCE — CLOSURES
 *
 * --- Core terms ---
 *
 *   Capture       Inner lambda/anonymous method uses outer variable
 *   Closure       Delegate + captured environment (display class instance)
 *   Display class Compiler-generated heap type holding captured fields
 *
 * --- Capture semantics ---
 *
 *   Locals/parameters   Shared by reference — one field, live value
 *   Fields on `this`    Already shared — no extra display field for `this`
 *   Assignment in λ     Visible to outer scope and other capturing delegates
 *
 * --- Modified vs copy ---
 *
 *   Mutate captured local after creating λ     Delegate sees new value
 *   `int copy = i` inside for-loop             Each λ captures distinct copy slot
 *   Helper parameter                           Value frozen at call site
 *
 * --- Lifetime / GC ---
 *
 *   Captured locals live while ANY capturing delegate is reachable
 *   Returning λ from method promotes captures to the heap
 *   Long-lived handlers can pin large object graphs — capture small data only
 *   Release: unsubscribe, set delegate field to null
 *
 * --- Loop pitfalls ---
 *
 *   for (int i ...)      ONE shared i — deferred λ sees final i → inner copy fix
 *   foreach (var x ...)  Per-iteration x since C# 5 — safe to capture
 *   Fix                  int copy = i;  OR  pass i into helper parameter
 *
 * --- Syntax forms (same capture rules) ---
 *
 *   Lambda           x => x + offset
 *   Anonymous method delegate (int x) { return x + offset; }
 *
 * --- Related chapters ---
 *
 *   02. Lambda Expressions        syntax, expression vs statement lambdas
 *   03. Anonymous Methods         delegate { } capture preview
 *   01. Delegates                 delegate types, multicast
 *   05. Func Action and Predicate built-in delegate types
 *   05. Language Integrated Query deferred queries capture outers too
 *
 * --- Common mistakes ---
 *
 *  Mistake                               | Result
 *  --------------------------------------|----------------------------------
 *  Capture for-loop index, run later     | Every delegate prints last index
 *  Assume capture snapshots value        | Mutations visible both ways
 *  Capture `this` in long-lived event    | Keeps entire object alive
 *  Forget to release handler             | Memory held until delegate dies
 */
