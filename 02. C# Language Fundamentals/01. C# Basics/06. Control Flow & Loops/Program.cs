/*
 * =============================================================================
 * 06. CONTROL FLOW & LOOPS IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Control flow — choosing which code runs — and loops — repeating code
 *        until a condition is met. Together they shape every program's path.
 *
 * WHY IT MATTERS:
 *   Business rules ("free shipping over $50"), validation ("reject negative
 *   quantity"), and batch processing ("scan every line item") all depend on
 *   if/else, switch, and loops. Misplaced break statements or off-by-one loop
 *   bounds cause subtle production bugs in billing, inventory, and auth checks.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Control flow diagrams (sequential, decision, loop, jump)
 *   2.  if / else if / else
 *   3.  Nested conditionals
 *   4.  Classic switch (break rules, shared labels, string and int)
 *   5.  Switch expressions (preview)
 *   6.  Pattern matching in switch (preview)
 *   7.  Ternary in control context (preview)
 *   8.  while loops
 *   9.  do-while loops
 *  10.  for loops, filters, and nested loops
 *  11.  foreach over a sequence
 *  12.  break and continue
 *  13.  Infinite loops and safe exit patterns
 *  14.  Previews: goto, loop vs recursion
 *
 * =============================================================================
 */

using System;
using System.Text;

namespace ControlFlowAndLoops;

public class Program
{
    /*
     * =========================================================================
     * SECTION 1: CONTROL FLOW OVERVIEW
     * =========================================================================
     *
     * CONTROL FLOW is the order in which statements execute. By default, C#
     * runs top to bottom, one statement at a time. Branching and looping
     * statements change that path.
     *
     * --- 1a. Sequential flow ---
     *
     *   [Start] → Statement A → Statement B → Statement C → [End]
     *
     * --- 1b. Decision (branching) ---
     *
     *                    ┌── true  → Block A ──┐
     *   [Condition?] ────┤                     ├──→ [Continue]
     *                    └── false → Block B ──┘
     *
     *   Keywords: if, else, switch
     *
     * --- 1c. Loop (iteration) ---
     *
     *   [Start loop] → [Body] → [Update/Test] ──→ back to Body while true
     *                      │
     *                      └── false → [Exit loop]
     *
     *   Keywords: while, do, for, foreach
     *
     * --- 1d. Jump statements ---
     *
     *   break    — exit the innermost loop or switch immediately
     *   continue — skip rest of current loop iteration; go to next test
     *   goto     — jump to a label (rare; see Section 14 preview)
     *
     * The helpers below implement warehouse order rules. Main wires them into
     * one runnable demo with fixed values (no keyboard input required).
     * -------------------------------------------------------------------------
     */

    /*
     * =========================================================================
     * SECTION 2: IF / ELSE IF / ELSE
     * =========================================================================
     *
     * The if statement runs a block ONLY when a bool expression is true.
     *
     *   if (condition)
     *       statement;           // single statement — braces optional
     *
     *   if (condition)
     *   {                          // multiple statements — braces required
     *       ...
     *   }
     *
     * else if adds another test when the previous condition was false.
     * else runs when every preceding test was false.
     *
     * Compile note: the condition MUST be bool. Writing if (x = 5) assigns
     * instead of comparing and causes CS0029 or CS0266.
     * -------------------------------------------------------------------------
     */
    public static string GetParityLabel(int value)
    {
        if (value % 2 == 0) // modulus — remainder 0 means even
        {
            return "even";
        }

        return "odd";
    }

    /*
     * =========================================================================
     * SECTION 3: NESTED CONDITIONALS
     * =========================================================================
     *
     * An if inside another if handles combinations of rules.
     *
     *   if (outer)
     *       if (inner)
     *           ...
     *
     * Prefer flat else-if chains when tests are mutually exclusive; nest when
     * the inner check only makes sense after the outer one passes.
     *
     * Example: shipping tier depends on total AND whether the order is bulk.
     * -------------------------------------------------------------------------
     */
    public static string GetShippingTier(decimal orderTotal, int lineItemCount)
    {
        bool isBulkOrder = lineItemCount >= 3; // derived flag — used in nested check below

        if (orderTotal >= 100m) // outer rule — high-value orders first
        {
            if (isBulkOrder) // inner rule only evaluated when outer passes
            {
                return "Express (bulk VIP)";
            }

            return "Express";
        }

        if (orderTotal >= 50m) // flat else-if — mutually exclusive with tiers above
        {
            return "Standard free";
        }

        return "Economy"; // default path when no threshold matched
    }

    /*
     * =========================================================================
     * SECTION 4: CLASSIC SWITCH STATEMENT
     * =========================================================================
     *
     * switch routes execution by matching a value against case labels.
     *
     *   switch (expression)
     *   {
     *       case value1:
     *           // statements
     *           break;       // REQUIRED — exit switch
     *       case value2:
     *           ...
     *           break;
     *       default:
     *           // no match
     *           break;
     *   }
     *
     * --- 4a. break and fall-through ---
     *
     * Each case MUST end with break, return, throw, or goto — otherwise the
     * compiler reports CS0163 ("control cannot fall through").
     *
     * C# does NOT allow fall-through between cases (unlike C/C++), except
     * when multiple labels share one block:
     *
     *   case "Packed":
     *   case "Shipped":
     *       message = "In transit pipeline";
     *       break;
     *
     * --- 4b. switch on string and int ---
     *
     * C# supports switch on string, int, char, enum, and (with patterns) more.
     * -------------------------------------------------------------------------
     */
    public static string GetStatusMessage(string fulfillmentStatus)
    {
        switch (fulfillmentStatus)
        {
            case "Pending":
                return "Awaiting pick list";
            case "Picking":
                return "Items being collected from shelves";
            case "Packed":
                return "Ready for carrier pickup";
            case "Shipped":
            case "Delivered": // shared labels — one block, no fall-through between bodies
                return "Handed to carrier or customer";
            default:
                return "Unknown status — escalate to supervisor";
        }
    }

    public static string GetPriorityLabel(int priorityCode)
    {
        switch (priorityCode)
        {
            case 1:
                return "Critical";
            case 2:
                return "Normal";
            case 3:
                return "Low";
            default:
                return "Unclassified";
        }
    }

    /*
     * =========================================================================
     * SECTION 5: SWITCH EXPRESSIONS (PREVIEW)
     * =========================================================================
     *
     * COVERED IN DETAIL LATER → C# 8 Features
     *   (expression-bodied switch that RETURNS a value)
     *
     * C# 8 introduced switch expressions — compact value-returning form:
     *
     *   string label = priorityCode switch
     *   {
     *       1 => "Critical",
     *       2 => "Normal",
     *       _ => "Other"
     *   };
     *
     * The underscore _ is the discard pattern (default). Full syntax, relational
     * patterns, and when clauses belong in the C# 8 Features chapter.
     * -------------------------------------------------------------------------
     */
    public static string GetPriorityPreview(int priorityCode)
    {
        return priorityCode switch
        {
            1 => "Critical",
            2 => "Normal",
            _ => "Other" // discard pattern — default when no case matches
        };
    }

    /*
     * =========================================================================
     * SECTION 6: PATTERN MATCHING IN SWITCH (PREVIEW)
     * =========================================================================
     *
     * COVERED IN DETAIL LATER → C# 7/8 Features
     *   (type patterns, when guards, property patterns)
     *
     * Modern switch can match on type and shape:
     *
     *   switch (obj)
     *   {
     *       case int n when n > 0: ...
     *       case string s: ...
     *   }
     *
     * Classic constant cases (Section 4) remain the default for simple routing.
     * -------------------------------------------------------------------------
     */
    public static string DescribePayload(object payload)
    {
        switch (payload)
        {
            case int units when units > 0: // type pattern + when guard
                return $"Positive quantity: {units}";
            case int units: // same type, no guard — catches zero and negative
                return $"Non-positive quantity: {units}";
            case string sku when sku.Length > 0:
                return $"SKU code: {sku}";
            case string: // empty string falls here
                return "Empty SKU string";
            default:
                return "Unsupported payload type";
        }
    }

    /*
     * =========================================================================
     * SECTION 7: TERNARY IN CONTROL CONTEXT (PREVIEW)
     * =========================================================================
     *
     * COVERED IN DETAIL LATER → 04. Operators and Expressions
     *   (condition ? trueValue : falseValue)
     *
     * The conditional operator picks one of two expressions. It is an operator,
     * not a statement — but it often replaces a simple if/else assignment:
     *
     *   string tier = orderTotal >= 50m ? "Free shipping" : "Paid shipping";
     *
     * See chapter 04 for nesting rules, type compatibility, and readability tips.
     * -------------------------------------------------------------------------
     */
    public static string GetShippingNote(decimal orderTotal)
    {
        return orderTotal >= 50m ? "Free shipping applied" : "Shipping fee applies"; // ternary replaces simple if/else
    }

    /*
     * =========================================================================
     * SECTION 8: WHILE LOOP
     * =========================================================================
     *
     *   while (condition)
     *   {
     *       // body — runs zero or more times
     *   }
     *
     * The condition is checked BEFORE each iteration. If false initially, the
     * body never runs.
     *
     * Use when the number of iterations is unknown (e.g. retry until success).
     *
     * Example: count down remaining units to pick until zero.
     * -------------------------------------------------------------------------
     */
    public static string BuildPickCountdown(int unitsToPick)
    {
        int remaining = unitsToPick;
        var pickSteps = new StringBuilder();

        while (remaining > 0) // condition tested before each iteration — may run zero times
        {
            pickSteps.Append(remaining);
            pickSteps.Append(' ');
            remaining--; // countdown toward exit condition
        }

        return pickSteps.ToString().TrimEnd(); // remove trailing space from last append
    }

    /*
     * =========================================================================
     * SECTION 9: DO-WHILE LOOP
     * =========================================================================
     *
     *   do
     *   {
     *       // body — always runs at least once
     *   } while (condition);
     *
     * The condition is checked AFTER each iteration. The body runs at least once
     * even if the condition starts false.
     *
     * Example: process at least one scan, then continue while more items exist.
     * -------------------------------------------------------------------------
     */
    public static int RunBarcodeScanCycle(int scansToPerform)
    {
        int scansLeft = scansToPerform;
        int scanLog = 0;

        do
        {
            scanLog++;    // body runs at least once even if scansLeft starts at 0
            scansLeft--;
        } while (scansLeft > 0); // condition checked after each pass

        return scanLog;
    }

    /*
     * =========================================================================
     * SECTION 10: FOR LOOP
     * =========================================================================
     *
     *   for (initializer; condition; iterator)
     *   {
     *       // body
     *   }
     *
     * Best when you know start, end, and step — e.g. print a range of bin numbers.
     *
     * --- 10a. Print numbers in range ---
     *
     *   for (int i = n1; i <= n2; i++)
     *       Console.Write(i + " ");
     *
     * --- 10b. Off-by-one caution ---
     *
     *   i < n   → excludes n
     *   i <= n  → includes n
     *
     * Mixing these causes the most common loop bugs.
     * -------------------------------------------------------------------------
     */
    public static string BuildRangeOutput(int rangeStart, int rangeEnd)
    {
        var rangeOutput = new StringBuilder();

        for (int i = rangeStart; i <= rangeEnd; i++) // inclusive upper bound — both endpoints included
        {
            rangeOutput.Append(i);
            rangeOutput.Append(' ');
        }

        return rangeOutput.ToString().TrimEnd();
    }

    /*
     * =========================================================================
     * SECTION 11: FOR + IF (FILTER INSIDE LOOP)
     * =========================================================================
     *
     * Combine for with if to process only items matching a rule — e.g. even
     * bin slots in a range:
     *
     *   for (int i = n1; i <= n2; i++)
     *       if (i % 2 == 0)
     *           Console.WriteLine(i);
     * -------------------------------------------------------------------------
     */
    public static string BuildEvenBins(int rangeStart, int rangeEnd)
    {
        var evenBins = new StringBuilder();

        for (int i = rangeStart; i <= rangeEnd; i++)
        {
            if (i % 2 == 0) // filter inside loop — only even bin numbers collected
            {
                evenBins.Append(i);
                evenBins.Append(' ');
            }
        }

        return evenBins.ToString().TrimEnd();
    }

    /*
     * =========================================================================
     * SECTION 12: NESTED LOOPS
     * =========================================================================
     *
     * A loop inside another loop visits every combination — rows × columns,
     * aisles × shelves, multiplication tables.
     *
     *   for (int row = 0; row < rows; row++)
     *       for (int col = 0; col < cols; col++)
     *           ...
     *
     * Total iterations = outer count × inner count. Watch performance on large grids.
     * -------------------------------------------------------------------------
     */
    public static int CountWarehouseSlots(int aisles, int shelvesPerAisle)
    {
        int slotCount = 0;

        for (int aisle = 1; aisle <= aisles; aisle++) // outer loop — each aisle
        {
            for (int shelf = 1; shelf <= shelvesPerAisle; shelf++) // inner — every shelf in current aisle
            {
                slotCount++; // total iterations = aisles × shelvesPerAisle
            }
        }

        return slotCount;
    }

    /*
     * =========================================================================
     * SECTION 13: FOREACH LOOP
     * =========================================================================
     *
     *   foreach (type item in collection)
     *   {
     *       // use item
     *   }
     *
     * Iterates every element in any IEnumerable<T> — arrays, lists, etc.
     * You do not manage an index manually; you cannot change the collection
     * during foreach (InvalidOperationException).
     *
     * COVERED IN DETAIL LATER → 09. Arrays (and collections modules)
     *   Arrays implement IEnumerable; here we use a string[] of SKU codes.
     * -------------------------------------------------------------------------
     */
    public static string FormatSkuList(string[] skuCodes)
    {
        var skuSummary = new StringBuilder();

        foreach (string sku in skuCodes) // no index — walks IEnumerable<string>
        {
            skuSummary.Append(sku);
            skuSummary.Append(", ");
        }

        if (skuSummary.Length >= 2)
        {
            skuSummary.Length -= 2; // trim trailing ", " without extra string allocation
        }

        return skuSummary.ToString();
    }

    /*
     * =========================================================================
     * SECTION 14: BREAK
     * =========================================================================
     *
     * break exits the innermost enclosing loop or switch immediately.
     *
     * Use when you found what you need and further iterations are wasted work
     * — e.g. stop searching once a high-priority SKU is located.
     * -------------------------------------------------------------------------
     */
    public static bool TryFindSku(string[] searchSkus, string target, out string foundSku, out int scansBeforeFind)
    {
        foundSku = "none";
        scansBeforeFind = 0;

        foreach (string sku in searchSkus)
        {
            scansBeforeFind++;
            if (sku == target)
            {
                foundSku = sku;
                return true; // early exit — equivalent to break + flag in a loop
            }
        }

        return false;
    }

    /*
     * =========================================================================
     * SECTION 15: CONTINUE
     * =========================================================================
     *
     * continue skips the rest of the current iteration and jumps to the next
     * loop test — it does NOT exit the loop entirely.
     *
     * Example: sum quantities but skip discontinued items (qty <= 0).
     * -------------------------------------------------------------------------
     */
    public static void SumActiveQuantities(int[] lineQuantities, out int activeLineTotal, out int skippedLines)
    {
        activeLineTotal = 0;
        skippedLines = 0;

        foreach (int qty in lineQuantities)
        {
            if (qty <= 0)
            {
                skippedLines++;
                continue; // skip sum for discontinued/zero lines — next iteration
            }

            activeLineTotal += qty;
        }
    }

    /*
     * =========================================================================
     * SECTION 16: INFINITE LOOPS AND SAFE EXIT
     * =========================================================================
     *
     * while (true) { ... } never tests false on its own — you MUST exit with
     * break, return, throw, or Environment.Exit. Used in server loops and menus;
     * this tutorial uses a bounded retry pattern instead of an unbounded loop.
     *
     * --- 16a. Bounded retry pattern ---
     *
     *   int attempts = 0;
     *   while (attempts < maxAttempts)
     *   {
     *       if (TryOperation()) break;
     *       attempts++;
     *   }
     *
     * Always ensure a path out — hung infinite loops freeze console apps.
     * -------------------------------------------------------------------------
     */
    public static bool TryOpenGate(int maxAttempts, out int attemptUsed)
    {
        int attempt = 0;

        while (attempt < maxAttempts) // bounded retry — not while(true)
        {
            attempt++;
            if (attempt == 3) // simulated success on third try
            {
                attemptUsed = attempt;
                return true; // safe exit path
            }
        }

        attemptUsed = attempt;
        return false; // exhausted attempts without success
    }

    /*
     * =========================================================================
     * SECTION 17: GOTO (PREVIEW)
     * =========================================================================
     *
     * goto labelName jumps to a marked statement. C# allows:
     *
     *   start:
     *       if (done) goto finish;
     *       ...
     *       goto start;
     *   finish:
     *       ...
     *
     * Discouraged for everyday code — prefer break, continue, methods, or
     * structured loops. Mentioned here for syllabus coverage only.
     * -------------------------------------------------------------------------
     */

    /*
     * =========================================================================
     * SECTION 18: LOOP VS RECURSION (PREVIEW)
     * =========================================================================
     *
     * COVERED IN DETAIL LATER → 07. Methods
     *   (a method calling itself vs iterative loops)
     *
     * Many problems have both loop and recursive solutions (factorial, tree walk).
     * Loops use fixed stack space; recursion uses the call stack — choose based
     * on clarity and depth. Methods chapter covers base cases and stack overflow.
     * -------------------------------------------------------------------------
     */

    /*
     * =========================================================================
     * SECTION 19: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Fixed warehouse order values drive every helper above. Output confirms
     * branching, switching, and loop behavior in one cohesive run.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        string orderId = "ORD-1042";
        int lineItemCount = 4;
        decimal orderTotal = 127.50m;
        int unitsToPick = 12;
        string fulfillmentStatus = "Packed";
        int priorityCode = 2;

        string parityLabel = GetParityLabel(unitsToPick);                    // SECTION 2 — if/else
        string shippingTier = GetShippingTier(orderTotal, lineItemCount);    // SECTION 3 — nested if
        string statusMessage = GetStatusMessage(fulfillmentStatus);          // SECTION 4 — classic switch
        string priorityLabel = GetPriorityLabel(priorityCode);
        string priorityPreview = GetPriorityPreview(priorityCode);         // SECTION 5 — switch expression
        string payloadDescription = DescribePayload(unitsToPick);          // SECTION 6 — pattern switch
        string shippingNote = GetShippingNote(orderTotal);                 // SECTION 7 — ternary
        string pickCountdown = BuildPickCountdown(unitsToPick);              // SECTION 8 — while
        int scanLog = RunBarcodeScanCycle(3);                                // SECTION 9 — do-while
        int rangeStart = 3;
        int rangeEnd = 7;
        string rangeOutput = BuildRangeOutput(rangeStart, rangeEnd);         // SECTION 10 — for
        string evenBins = BuildEvenBins(rangeStart, rangeEnd);               // SECTION 11 — for + if
        int slotCount = CountWarehouseSlots(2, 3);                           // SECTION 12 — nested for
        string[] skuCodes = { "WIDGET-A", "WIDGET-B", "GADGET-C" };
        string skuSummary = FormatSkuList(skuCodes);                         // SECTION 13 — foreach
        string[] searchSkus = { "BOLT-1", "BOLT-2", "WIDGET-A", "BOLT-3" };
        bool skuFound = TryFindSku(searchSkus, "WIDGET-A", out string foundSku, out int scansBeforeFind); // SECTION 14
        int[] lineQuantities = { 2, 0, 5, -1, 3 };
        SumActiveQuantities(lineQuantities, out int activeLineTotal, out int skippedLines); // SECTION 15 — continue
        bool gateOpen = TryOpenGate(5, out int attemptUsed);                 // SECTION 16 — bounded loop exit

        Console.WriteLine("=== Control Flow & Loops — Order Fulfillment ===");
        Console.WriteLine();
        Console.WriteLine($"Order {orderId}: {lineItemCount} lines, total {orderTotal:C}");
        Console.WriteLine($"Units to pick: {unitsToPick} ({parityLabel})");
        Console.WriteLine($"Shipping: {shippingTier} — {shippingNote}");
        Console.WriteLine($"Status [{fulfillmentStatus}]: {statusMessage}");
        Console.WriteLine($"Priority code {priorityCode} → {priorityLabel} (preview expr: {priorityPreview})");
        Console.WriteLine($"Pattern preview on quantity: {payloadDescription}");
        Console.WriteLine();
        Console.WriteLine($"Pick countdown: {pickCountdown}");
        Console.WriteLine($"Barcode scans logged: {scanLog}");
        Console.WriteLine($"Bin range {rangeStart}-{rangeEnd}: {rangeOutput}");
        Console.WriteLine($"Even bins in range: {evenBins}");
        Console.WriteLine($"Warehouse slots (2×3): {slotCount}");
        Console.WriteLine($"SKU list: {skuSummary}");
        Console.WriteLine($"Search found '{foundSku}' after {scansBeforeFind} scan(s) (found={skuFound})");
        Console.WriteLine($"Active qty sum: {activeLineTotal} (skipped {skippedLines} lines)");
        Console.WriteLine($"Gate opened on attempt {attemptUsed} (success={gateOpen})");
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — CONTROL FLOW & LOOPS
 * =============================================================================
 *
 * --- Branching ---
 *
 *  if (cond) { } else if (cond) { } else { }     mutually exclusive paths
 *  switch (x) { case v: ... break; default: ... }  match constant values
 *
 * --- Loops ---
 *
 *  while (cond) { }           test before; may run zero times
 *  do { } while (cond);      test after; runs at least once
 *  for (init; test; step) { } counted iteration
 *  foreach (var x in seq) { }  each element in IEnumerable
 *
 * --- Jump ---
 *
 *  break      exit innermost loop or switch
 *  continue   skip to next loop iteration
 *  goto label rare; prefer structured control
 *
 * --- switch rules ---
 *
 *  Every case needs break/return/throw (or shared block)
 *  No fall-through between cases with code (CS0163 if missing break)
 *
 * --- Common mistakes ---
 *
 *  Mistake                          | Result
 *  ---------------------------------|----------------------------------
 *  if (x = 5) instead of x == 5     | compile error or wrong assignment
 *  for (i = 0; i < n; i++) off-by-one | skips last or runs one extra
 *  Missing break in switch case       | CS0163
 *  while (true) without break       | infinite hang
 *  Modifying collection in foreach    | InvalidOperationException
 *
 * =============================================================================
 */
