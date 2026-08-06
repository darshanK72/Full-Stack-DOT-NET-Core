/*
 * =============================================================================
 * 07. CONTROL FLOW & LOOPS IN C# — COMPLETE TUTORIAL
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
 *   1.  Control flow diagrams (decision and loop paths)
 *   2.  if / else if / else
 *   3.  Nested conditionals
 *   4.  Classic switch (break and fall-through rules)
 *   5.  while and do-while loops
 *   6.  for loops and nested loops
 *   7.  foreach over a sequence
 *   8.  break and continue
 *   9.  Infinite loops and safe exit
 *  10.  Previews: switch expressions, pattern matching, ternary, goto, recursion
 *
 * =============================================================================
 */

using System;
using System.Text;

namespace ControlFlowAndLoops;

public class Program
{
    public static void Main(string[] args)
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
         *   goto     — jump to a label (rare; see Section 15 preview)
         *
         * This chapter uses a small warehouse order scenario. Values are fixed so
         * the program runs without keyboard input; every variable is used in output.
         * -------------------------------------------------------------------------
         */

        string orderId = "ORD-1042";
        int lineItemCount = 4;
        decimal orderTotal = 127.50m;
        int unitsToPick = 12;
        string fulfillmentStatus = "Packed";


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
         * --- 2a. Even / odd on quantity ---
         *
         *   unitsToPick % 2 == 0   → even (divides evenly by 2)
         *   otherwise              → odd
         *
         * Compile note: the condition MUST be bool. Writing if (x = 5) assigns
         * instead of comparing and causes CS0029 or CS0266.
         * -------------------------------------------------------------------------
         */

        string parityLabel;
        if (unitsToPick % 2 == 0)
        {
            parityLabel = "even";
        }
        else
        {
            parityLabel = "odd";
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

        bool isBulkOrder = lineItemCount >= 3;
        string shippingTier;

        if (orderTotal >= 100m)
        {
            if (isBulkOrder)
            {
                shippingTier = "Express (bulk VIP)";
            }
            else
            {
                shippingTier = "Express";
            }
        }
        else if (orderTotal >= 50m)
        {
            shippingTier = "Standard free";
        }
        else
        {
            shippingTier = "Economy";
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
         * --- 4b. switch on string ---
         *
         * C# 7+ supports switch on string, int, char, enum, and (with patterns) more.
         * -------------------------------------------------------------------------
         */

        string statusMessage;
        switch (fulfillmentStatus)
        {
            case "Pending":
                statusMessage = "Awaiting pick list";
                break;
            case "Picking":
                statusMessage = "Items being collected from shelves";
                break;
            case "Packed":
                statusMessage = "Ready for carrier pickup";
                break;
            case "Shipped":
            case "Delivered":
                statusMessage = "Handed to carrier or customer";
                break;
            default:
                statusMessage = "Unknown status — escalate to supervisor";
                break;
        }

        int priorityCode = 2;
        string priorityLabel;
        switch (priorityCode)
        {
            case 1:
                priorityLabel = "Critical";
                break;
            case 2:
                priorityLabel = "Normal";
                break;
            case 3:
                priorityLabel = "Low";
                break;
            default:
                priorityLabel = "Unclassified";
                break;
        }


        /*
         * =========================================================================
         * SECTION 5: SWITCH EXPRESSIONS (PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → C# 8 Features
         *   (headline concepts only: expression-bodied switch that RETURNS a value)
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

        string priorityPreview = priorityCode switch
        {
            1 => "Critical",
            2 => "Normal",
            _ => "Other"
        };


        /*
         * =========================================================================
         * SECTION 6: PATTERN MATCHING IN SWITCH (PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → C# 7/8 Features
         *   (headline concepts only: type patterns, when guards, property patterns)
         *
         * Modern switch can match on type and shape:
         *
         *   switch (obj)
         *   {
         *       case int n when n > 0: ...
         *       case string s: ...
         *   }
         *
         * This chapter uses classic constant cases; pattern matching gets full treatment later.
         * -------------------------------------------------------------------------
         */


        /*
         * =========================================================================
         * SECTION 7: TERNARY IN CONTROL CONTEXT (PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → 04. Operators and Expressions
         *   (headline concepts only: condition ? trueValue : falseValue)
         *
         * The conditional operator picks one of two expressions. It is an operator,
         * not a statement — but it often replaces a simple if/else assignment:
         *
         *   string tier = orderTotal >= 50m ? "Free shipping" : "Paid shipping";
         *
         * See chapter 04 for nesting rules, type compatibility, and readability tips.
         * -------------------------------------------------------------------------
         */

        string shippingNote = orderTotal >= 50m ? "Free shipping applied" : "Shipping fee applies";


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

        int remaining = unitsToPick;
        var pickSteps = new StringBuilder();

        while (remaining > 0)
        {
            pickSteps.Append(remaining);
            pickSteps.Append(' ');
            remaining--;
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

        int scansLeft = 3;
        int scanLog = 0;

        do
        {
            scanLog++;
            scansLeft--;
        } while (scansLeft > 0);


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
         * Legacy demo pattern: print every integer from n1 through n2 inclusive.
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

        int rangeStart = 3;
        int rangeEnd = 7;
        var rangeOutput = new StringBuilder();

        for (int i = rangeStart; i <= rangeEnd; i++)
        {
            rangeOutput.Append(i);
            rangeOutput.Append(' ');
        }


        /*
         * =========================================================================
         * SECTION 11: FOR + IF (FILTER INSIDE LOOP)
         * =========================================================================
         *
         * Combine for with if to process only items matching a rule — e.g. even
         * bin slots in a range (legacy EvenNumbers demo pattern):
         *
         *   for (int i = n1; i <= n2; i++)
         *       if (i % 2 == 0)
         *           Console.WriteLine(i);
         * -------------------------------------------------------------------------
         */

        var evenBins = new StringBuilder();
        for (int i = rangeStart; i <= rangeEnd; i++)
        {
            if (i % 2 == 0)
            {
                evenBins.Append(i);
                evenBins.Append(' ');
            }
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

        int aisles = 2;
        int shelvesPerAisle = 3;
        int slotCount = 0;

        for (int aisle = 1; aisle <= aisles; aisle++)
        {
            for (int shelf = 1; shelf <= shelvesPerAisle; shelf++)
            {
                slotCount++;
            }
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

        string[] skuCodes = { "WIDGET-A", "WIDGET-B", "GADGET-C" };
        var skuSummary = new StringBuilder();

        foreach (string sku in skuCodes)
        {
            skuSummary.Append(sku);
            skuSummary.Append(", ");
        }

        if (skuSummary.Length >= 2)
        {
            skuSummary.Length -= 2; // trim trailing ", "
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

        string[] searchSkus = { "BOLT-1", "BOLT-2", "WIDGET-A", "BOLT-3" };
        string foundSku = "none";
        int scansBeforeFind = 0;

        foreach (string sku in searchSkus)
        {
            scansBeforeFind++;
            if (sku == "WIDGET-A")
            {
                foundSku = sku;
                break;
            }
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

        int[] lineQuantities = { 2, 0, 5, -1, 3 };
        int activeLineTotal = 0;
        int skippedLines = 0;

        foreach (int qty in lineQuantities)
        {
            if (qty <= 0)
            {
                skippedLines++;
                continue;
            }

            activeLineTotal += qty;
        }


        /*
         * =========================================================================
         * SECTION 16: INFINITE LOOPS AND SAFE EXIT
         * =========================================================================
         *
         * while (true) { ... } never tests false on its own — you MUST exit with
         * break, return, throw, or Environment.Exit. Used in server loops and menus;
         * this tutorial avoids unbounded loops in favor of a bounded retry pattern.
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

        int maxAttempts = 5;
        int attempt = 0;
        bool gateOpen = false;

        while (attempt < maxAttempts)
        {
            attempt++;
            if (attempt == 3)
            {
                gateOpen = true;
                break;
            }
        }


        /*
         * =========================================================================
         * SECTION 17: GOTO (PREVIEW)
         * =========================================================================
         *
         * goto labelName jumps to a marked statement. Labeled break targets exist
         * in nested loops in some languages; C# restricts goto but allows:
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
         * COVERED IN DETAIL LATER → 06. Methods
         *   (headline concepts only: a method calling itself vs iterative loops)
         *
         * Many problems have both loop and recursive solutions (factorial, tree walk).
         * Loops use fixed stack space; recursion uses the call stack — choose based
         * on clarity and depth. Methods chapter covers base cases and stack overflow.
         * -------------------------------------------------------------------------
         */


        /*
         * =========================================================================
         * SECTION 19: ORDER FULFILLMENT SUMMARY (OUTPUT)
         * =========================================================================
         * Prints results from every section above — no unused variables.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("=== Control Flow & Loops — Order Fulfillment ===");
        Console.WriteLine();
        Console.WriteLine($"Order {orderId}: {lineItemCount} lines, total {orderTotal:C}");
        Console.WriteLine($"Units to pick: {unitsToPick} ({parityLabel})");
        Console.WriteLine($"Shipping: {shippingTier} — {shippingNote}");
        Console.WriteLine($"Status [{fulfillmentStatus}]: {statusMessage}");
        Console.WriteLine($"Priority code {priorityCode} → {priorityLabel} (preview expr: {priorityPreview})");
        Console.WriteLine();
        Console.WriteLine($"Pick countdown: {pickSteps}");
        Console.WriteLine($"Barcode scans logged: {scanLog}");
        Console.WriteLine($"Bin range {rangeStart}-{rangeEnd}: {rangeOutput}");
        Console.WriteLine($"Even bins in range: {evenBins}");
        Console.WriteLine($"Warehouse slots ({aisles}×{shelvesPerAisle}): {slotCount}");
        Console.WriteLine($"SKU list: {skuSummary}");
        Console.WriteLine($"Search found '{foundSku}' after {scansBeforeFind} scan(s)");
        Console.WriteLine($"Active qty sum: {activeLineTotal} (skipped {skippedLines} lines)");
        Console.WriteLine($"Gate opened on attempt {attempt} (success={gateOpen})");
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
