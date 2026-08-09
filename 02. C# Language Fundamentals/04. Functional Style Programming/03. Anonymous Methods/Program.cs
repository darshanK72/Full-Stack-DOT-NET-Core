/*
 * =============================================================================
 * 03. ANONYMOUS METHODS IN C#
 * =============================================================================
 *
 * TOPIC: Anonymous methods — inline delegate bodies written with
 *        delegate { … } (C# 2.0). They let you assign a block of statements
 *        to a delegate-typed variable without declaring a separate method name.
 *
 * WHY IT MATTERS:
 *   Before lambdas (C# 3.0), anonymous methods were the standard way to pass
 *   short behavior blocks to delegates. WinForms, WPF, older ASP.NET, and
 *   pre-2010 libraries still contain delegate { } handlers. You need to read
 *   them, maintain them, and recognize when a lambda is the modern equivalent.
 *
 * WHAT YOU WILL LEARN:
 *   1.  The evolution: named method → anonymous method → lambda
 *   2.  delegate { } syntax and assigning to a delegate variable
 *   3.  Explicit parameter lists vs omitting parameters
 *   4.  void-returning anonymous methods (delegate void D() { … })
 *   5.  Accessing outer local variables (closure preview)
 *   6.  Side-by-side comparison with lambdas — prefer lambdas in new code
 *   7.  Where anonymous methods still appear in legacy codebases
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace AnonymousMethods;

/*
 * =========================================================================
 * SECTION 1: DELEGATE TYPES — PREREQUISITE FROM 01. DELEGATES
 * =========================================================================
 *
 * Anonymous methods compile only when assigned to a compatible delegate type.
 * The delegate declares the return type and parameter list the anonymous body
 * must satisfy.
 *
 *   public delegate bool OrderRule(Order o);
 *   public delegate void OrderNotifier(string message);
 *
 * COVERED IN DETAIL → 01. Delegates
 *   (declaration, invocation, multicast, method group conversion)
 *
 * --- Three ways to supply behavior ---
 *
 *  Approach          | Shape                              | Era
 *  ------------------|------------------------------------|-------------
 *  Named method      | OrderRule rule = HasPositiveTotal; | C# 1.0+
 *  Anonymous method  | OrderRule rule = delegate (Order o)| C# 2.0
 *                    |   { return o.Total > 0; };
 *  Lambda expression | OrderRule rule = o => o.Total > 0; | C# 3.0+
 *
 * All three produce a callable delegate instance. Anonymous methods were the
 * stepping stone between named methods and lambdas.
 * -------------------------------------------------------------------------
 */
public delegate bool OrderRule(Order o);
public delegate void OrderNotifier(string message);

/*
 * =========================================================================
 * SECTION 2: SCENARIO TYPE — ORDER
 * =========================================================================
 *
 * Order records used by validation rules throughout the demo pipeline.
 * -------------------------------------------------------------------------
 */
public sealed class Order
{
    public Order(int id, string customer, decimal total, int lineItemCount)
    {
        Id = id;
        Customer = customer;
        Total = total;
        LineItemCount = lineItemCount;
    }

    public int Id { get; }
    public string Customer { get; }
    public decimal Total { get; }
    public int LineItemCount { get; }

    public override string ToString()
    {
        return $"Order {Id} ({Customer}): {Total:C}, {LineItemCount} line item(s)";
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 3: NAMED METHOD AS DELEGATE TARGET (BASELINE)
     * =========================================================================
     *
     * The classic pattern from 01. Delegates: assign an existing method by name
     * when the logic is reused or deserves its own identifier.
     *
     *   OrderRule positiveTotalRule = HasPositiveTotal;
     *   bool ok = positiveTotalRule(someOrder);
     *
     * Method group conversion: the compiler wraps HasPositiveTotal in a delegate
     * instance whose Invoke calls that method.
     * -------------------------------------------------------------------------
     */
    public static bool HasPositiveTotal(Order order)
    {
        return order.Total > 0;
    }

    /*
     * =========================================================================
     * SECTION 4: ANONYMOUS METHOD SYNTAX — delegate { } WITH PARAMETERS
     * =========================================================================
     *
     * An ANONYMOUS METHOD is a block of statements assigned directly to a
     * delegate-typed variable. The keyword delegate introduces the body:
     *
     *   OrderRule maxLineItemsRule = delegate (Order order)
     *   {
     *       return order.LineItemCount <= maxAllowedItems;
     *   };
     *
     * Parts:
     *
     *  Part              | Role
     *  ------------------|----------------------------------------------------
     *  delegate keyword  | Starts the anonymous method (not a type declaration)
     *  (Order order)     | Parameter list — must match delegate signature
     *  { … }             | Statement body — multiple statements allowed
     *
     * The anonymous method has no name of its own; only the delegate variable
     * (maxLineItemsRule) holds a reference callers use.
     *
     * --- Compile errors ---
     *
     *   Parameter list doesn't match delegate     → CS1593 / CS0123
     *   Missing return on non-void code path      → CS0161
     *   return value in void delegate body        → CS0126 / CS1520
     * -------------------------------------------------------------------------
     */
    public static OrderRule BuildMaxLineItemsRule(int maxAllowedItems)
    {
        OrderRule maxLineItemsRule = delegate (Order o)
        {
            return o.LineItemCount <= maxAllowedItems; // maxAllowedItems captured from outer scope
        };

        return maxLineItemsRule;
    }

    /*
     * =========================================================================
     * SECTION 5: OMITTING THE PARAMETER LIST
     * =========================================================================
     *
     * You may OMIT the parameter list when the body does not reference any
     * delegate parameters — common in legacy WinForms/WPF event wiring:
     *
     *   buttonSave.Click += delegate { SaveOrder(); };
     *
     * When the body uses delegate parameters (Order o, sender, etc.), write an
     * explicit parameter list — required in modern C# / Roslyn:
     *
     *   OrderRule corporateMinimumRule = delegate (Order o)
     *   {
     *       return o.Total >= minimumCorporateTotal;
     *   };
     *
     * Older docs sometimes claimed omitted lists could use delegate parameter
     * names by scope; that pattern does not compile today — prefer explicit lists.
     *
     * --- 5a. Omitted list when body ignores all delegate parameters ---
     *
     *   OrderNotifier pipelineReady = delegate
     *   {
     *       Console.WriteLine("Pipeline configured.");
     *   };
     *
     * Invoke still passes the string argument; the body simply does not use it.
     * -------------------------------------------------------------------------
     */
    public static OrderRule BuildCorporateMinimumRule(decimal minimumCorporateTotal)
    {
        OrderRule corporateMinimumRule = delegate (Order o)
        {
            return o.Total >= minimumCorporateTotal; // explicit list — preferred in new code
        };

        return corporateMinimumRule;
    }

    public static OrderNotifier BuildPipelineReadyNotifier()
    {
        OrderNotifier pipelineReady = delegate
        {
            Console.WriteLine("  (Omitted parameter list — body ignores delegate parameters)");
        };

        return pipelineReady;
    }

    /*
     * =========================================================================
     * SECTION 6: void ANONYMOUS METHODS — delegate void D() { … }
     * =========================================================================
     *
     * Not every delegate returns a value. For void delegates, the anonymous body
     * runs statements without returning a value (or uses bare return; to exit early):
     *
     *   OrderNotifier logFailure = delegate (string message)
     *   {
     *       auditLog.AppendLine(message);
     *   };
     *
     * Conceptual shape (custom void delegate with no parameters):
     *
     *   delegate void D();
     *   D tick = delegate { Console.WriteLine("tick"); };
     *   tick();
     *
     * Same rules apply: explicit or omitted parameter list, statement body only.
     * -------------------------------------------------------------------------
     */
    public static OrderNotifier BuildAuditLogger(StringBuilder auditLog)
    {
        OrderNotifier logFailure = delegate (string message)
        {
            auditLog.AppendLine(message); // side effect — void delegate, no return value
        };

        return logFailure;
    }

    /*
     * =========================================================================
     * SECTION 7: ACCESSING OUTER LOCAL VARIABLES (PREVIEW)
     * =========================================================================
     *
     * Anonymous methods may read (and assign) local variables and parameters
     * from the enclosing scope — maxAllowedItems, minimumCorporateTotal, and
     * auditLog above are captured. The compiler generates a display class so
     * the delegate still works after the enclosing method returns.
     *
     * COVERED IN DETAIL LATER → 06. Closures
     *   (capture semantics, loop-variable pitfalls, lifetime, mutating captures)
     *
     * Capture behavior is equivalent between anonymous methods and lambdas.
     * -------------------------------------------------------------------------
     */
    public static int CountMatchingOrders(List<Order> orders, OrderRule rule)
    {
        int matchCount = 0;

        OrderRule countIfMatch = delegate (Order o)
        {
            bool passes = rule(o);
            if (passes)
            {
                matchCount++; // mutating captured local — preview only; see 06. Closures
            }

            return passes;
        };

        foreach (Order o in orders)
        {
            countIfMatch(o);
        }

        return matchCount;
    }

    /*
     * =========================================================================
     * SECTION 8: LAMBDA EQUIVALENTS — PREFER FOR NEW CODE
     * =========================================================================
     *
     * Lambdas (C# 3.0) replaced anonymous methods for new code. Every anonymous
     * method in this chapter has a lambda equivalent:
     *
     *  Anonymous method                              | Lambda equivalent
     *  ----------------------------------------------|---------------------------
     *  delegate (Order o) { return o.Total > 0; }    | o => o.Total > 0
     *  delegate (Order o) { return o.Total >= 100m; } | o => o.Total >= 100m
     *  delegate (string msg) { auditLog.AppendLine(  | msg => auditLog.AppendLine(
     *      msg); }                                     |     msg)
     *
     * Migration steps when modernizing legacy code:
     *
     *   1. Identify the delegate type (custom or Func/Action/Predicate).
     *   2. List parameters — restore omitted lists as lambda parameters.
     *   3. Replace delegate { … } with (params) => expression or { … }.
     *   4. Single-expression bodies can drop braces and return.
     *   5. Build and run tests — capture semantics are equivalent.
     *
     * COVERED IN DETAIL → 02. Lambda Expressions
     *   (expression vs statement lambdas, inference, LINQ usage)
     *
     * New code should use lambdas (or local functions) unless you are matching
     * surrounding legacy style.
     * -------------------------------------------------------------------------
     */
    public static OrderRule BuildMaxLineItemsLambda(int maxAllowedItems)
    {
        return o => o.LineItemCount <= maxAllowedItems;
    }

    public static OrderRule BuildCorporateMinimumLambda(decimal minimumCorporateTotal)
    {
        return o => o.Total >= minimumCorporateTotal;
    }

    /*
     * =========================================================================
     * SECTION 9: PIPELINE HELPER — RULES COMPOSED WITH DELEGATES
     * =========================================================================
     *
     * Applies each rule to every order and collects failure messages. Works with
     * named methods, anonymous methods, or lambdas — the delegate type is the
     * common contract (see 01. Delegates for multicast += preview).
     * -------------------------------------------------------------------------
     */
    public static List<string> RunAllRules(
        List<Order> orders,
        OrderRule[] rules,
        OrderNotifier notify)
    {
        List<string> failures = new List<string>();

        foreach (Order order in orders)
        {
            foreach (OrderRule rule in rules)
            {
                if (!rule(order))
                {
                    string message = $"Order {order.Id} failed rule {rule.Method?.Name ?? "anonymous"}";
                    failures.Add(message);
                    notify(message);
                }
            }
        }

        return failures;
    }

    /*
     * =========================================================================
     * SECTION 10: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     */
    public static void Main(string[] args)
    {
        List<Order> orders = new List<Order>
        {
            new Order(1001, "Acme Corp", 249.99m, 3),
            new Order(1002, "Beta LLC", -10.00m, 1),
            new Order(1003, "Gamma Inc", 89.50m, 0),
            new Order(1004, "Delta Co", 1200.00m, 12)
        };

        int maxAllowedItems = 10;
        decimal minimumCorporateTotal = 100.00m;

        Console.WriteLine("=== 03. Anonymous Methods — Order validation ===");
        Console.WriteLine();

        // Section 3 — named method delegate
        OrderRule positiveTotalRule = HasPositiveTotal;

        Console.WriteLine("--- Named method delegate ---");
        foreach (Order order in orders)
        {
            bool passes = positiveTotalRule(order);
            Console.WriteLine($"  Order {order.Id}: positive total? {passes}");
        }

        Console.WriteLine();

        // Section 4 — anonymous method with explicit parameters
        OrderRule maxLineItemsRule = BuildMaxLineItemsRule(maxAllowedItems);

        Console.WriteLine("--- Anonymous method (explicit parameters) ---");
        foreach (Order order in orders)
        {
            bool passes = maxLineItemsRule(order);
            Console.WriteLine($"  Order {order.Id}: line items {order.LineItemCount} — within limit? {passes}");
        }

        Console.WriteLine();

        // Section 5 — omitted parameter list (void delegate ignores parameters; OrderRule uses explicit list)
        OrderRule corporateMinimumRule = BuildCorporateMinimumRule(minimumCorporateTotal);
        OrderNotifier pipelineReady = BuildPipelineReadyNotifier();

        pipelineReady("unused-at-invoke"); // signature requires string; omitted-list body ignores it

        Console.WriteLine("--- Anonymous method (explicit params + omitted void list) ---");
        foreach (Order order in orders)
        {
            bool passes = corporateMinimumRule(order);
            Console.WriteLine($"  Order {order.Id}: total {order.Total:C} — meets minimum? {passes}");
        }

        Console.WriteLine();

        // Sections 6–7 — void anonymous method + captured StringBuilder
        StringBuilder auditLog = new StringBuilder();
        OrderNotifier logFailure = BuildAuditLogger(auditLog);

        Order sampleFailure = orders[1];
        if (!positiveTotalRule(sampleFailure))
        {
            logFailure($"REJECT order {sampleFailure.Id}: non-positive total {sampleFailure.Total:C}");
        }

        int passingPositiveRule = CountMatchingOrders(orders, positiveTotalRule);

        Console.WriteLine("--- void anonymous method + captured locals ---");
        Console.WriteLine(auditLog.ToString().TrimEnd());
        Console.WriteLine($"  Orders passing positive-total rule: {passingPositiveRule}");
        Console.WriteLine();

        // Section 8 — lambda equivalents
        OrderRule maxLineItemsLambda = BuildMaxLineItemsLambda(maxAllowedItems);
        OrderRule corporateMinimumLambda = BuildCorporateMinimumLambda(minimumCorporateTotal);

        Console.WriteLine("--- Lambda equivalents (modern style) ---");
        foreach (Order order in orders)
        {
            bool itemOk = maxLineItemsLambda(order);
            bool totalOk = corporateMinimumLambda(order);
            Console.WriteLine($"  Order {order.Id}: lambda checks — items={itemOk}, minimum={totalOk}");
        }

        Console.WriteLine();

        // Section 9 — full pipeline
        OrderRule[] pipeline =
        {
            HasPositiveTotal,
            maxLineItemsRule,
            corporateMinimumRule
        };

        List<string> failures = RunAllRules(orders, pipeline, logFailure);

        Console.WriteLine("--- Pipeline summary ---");
        Console.WriteLine($"  Orders processed: {orders.Count}");
        Console.WriteLine($"  Failure messages: {failures.Count}");
        foreach (string failure in failures)
        {
            Console.WriteLine($"    {failure}");
        }

        Console.WriteLine();
        PrintLegacyPatternsReference();
    }

    /*
     * =========================================================================
     * SECTION 11: LEGACY PATTERNS — READING OLDER CODEBASES
     * =========================================================================
     *
     * Anonymous methods still appear in maintained .NET Framework code:
     *
     *   • WinForms / WPF: button.Click += delegate { SaveOrder(); };
     *   • ThreadPool.QueueUserWorkItem(delegate { … }); (pre-Task API)
     *   • List<T>.FindAll(delegate (T x) { return …; }) before lambdas
     *   • Generated designer.cs in .NET Framework projects
     *
     * Modernization: replace with lambdas or local functions when touching the
     * file. Do not mass-convert unrelated legacy code without tests.
     *
     *   // WinForms-style (conceptual):
     *   // buttonSave.Click += delegate (object sender, EventArgs e) { SaveOrder(); };
     *   // Lambda replacement:
     *   // buttonSave.Click += (sender, e) => SaveOrder();
     * -------------------------------------------------------------------------
     */
    public static void PrintLegacyPatternsReference()
    {
        Console.WriteLine("--- Legacy hotspots ---");
        Console.WriteLine("  WinForms/WPF events, ThreadPool callbacks, old FindAll/ForEach, designer.cs");
        Console.WriteLine("  Prefer lambdas in new code; read delegate { } when maintaining older projects.");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — ANONYMOUS METHODS
 * =========================================================================
 *
 * --- Syntax ---
 *
 *   DelegateType name = delegate (Type param) { statements; return value; };
 *   DelegateType name = delegate { statements; };     // inherit delegate parameter names
 *   voidDelegate   name = delegate { DoWork(); };       // no return value
 *
 * --- vs named method ---
 *
 *   OrderRule r = HasPositiveTotal;                 // existing method by name
 *   OrderRule r = delegate (Order o) { … };           // inline, no method name
 *
 * --- vs lambda (prefer for new code) ---
 *
 *   delegate (Order o) { return o.Total > 0; }  →  o => o.Total > 0
 *   delegate (Order o) { return o.Total >= 100m; } →  o => o.Total >= 100m
 *   delegate (string m) { Log(m); }                 →  m => Log(m)
 *
 * --- Key facts ---
 *
 *   • Introduced in C# 2.0; still valid but superseded by lambdas (C# 3.0+)
 *   • Captures outer variables — same as lambdas (preview here; deep dive ch.06)
 *   • Cannot appear where a type name is required (only on delegate assignment)
 *   • No expression-bodied form — always delegate { … } block (or single return)
 *
 * --- Common mistakes ---
 *
 *  Mistake                                   | Result
 *  ------------------------------------------|----------------------------------
 *  Parameter list doesn't match delegate     | CS1593 / CS0123
 *  return value in void delegate body        | CS0126 / CS1520
 *  Missing return on non-void anonymous path | CS0161
 *  Using anonymous method as a type name     | CS0246 (invalid)
 *
 * --- Related chapters ---
 *
 *   Delegates (basics)  → 01. Delegates
 *   Lambdas (modern)    → 02. Lambda Expressions
 *   Closures (deep)     → 06. Closures
 *   Func/Action         → 05. Func Action and Predicate
 *
 * =========================================================================
 */
