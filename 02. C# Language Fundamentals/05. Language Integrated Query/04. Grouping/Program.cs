/*
 * =============================================================================
 * 04. GROUPING — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: LINQ grouping — partition one sequence into keyed buckets (GroupBy),
 *        inspect each IGrouping, project members or summary rows with key /
 *        element / result selectors, nest groups, and build an immediate
 *        ILookup with ToLookup. Query syntax uses group … by … and into.
 *
 * WHY IT MATTERS:
 *   Reports rarely need every raw row. Support wants open tickets by priority,
 *   finance wants spend by vendor, HR wants headcount by department. GroupBy
 *   turns a flat list into keyed collections you can summarize, nest in UI
 *   trees, or index for repeated lookup. It sits between filtering/aggregation
 *   (ch.02) and joins that attach related sequences (ch.05 — GroupJoin is a
 *   different operator).
 *
 * WHAT YOU WILL LEARN:
 *   1.  GroupBy overloads — key, element, and result selectors
 *   2.  IGrouping<TKey, TElement> — .Key and nested iteration
 *   3.  Query syntax — group … by … and into
 *   4.  Composite keys, per-group aggregates, Select after GroupBy
 *   5.  Nested grouping basics
 *   6.  ToLookup — immediate ILookup vs deferred GroupBy
 *   7.  Custom key equality with IEqualityComparer<TKey>
 *   8.  GroupBy vs GroupJoin / Select depth — preview only (→ siblings)
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Grouping;

/*
 * =========================================================================
 * SECTION 1: SAMPLE MODEL — SUPPORT TICKETS
 * =========================================================================
 *
 * One flat row type powers every demo. Grouping reorganizes the sequence
 * into keyed buckets of these same Ticket instances (unless an element
 * selector projects a different TElement).
 *
 *  Field      | Role in this chapter
 *  -----------|----------------------------------------------------------
 *  TicketId   | Unique row id
 *  Title      | Display label inside a group
 *  Priority   | Group key (P1 / P2 / P3)
 *  Category   | Group key + nested-group outer key
 *  Assignee   | Element projection / lookup index
 *  HoursOpen  | Per-group aggregates (Sum, Average, Max)
 *
 * A readonly record struct keeps construction short; GroupBy works the same
 * on classes, structs, and anonymous types.
 * -------------------------------------------------------------------------
 */
public readonly record struct Ticket(
    int TicketId,
    string Title,
    string Priority,
    string Category,
    string Assignee,
    int HoursOpen);

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 2: SAMPLE DATA AND GroupBy OVERVIEW
         * =========================================================================
         *
         * GroupBy partitions a source into groups that share the same key:
         *
         *  Overload shape (simplified)              | What each Func does
         *  -----------------------------------------|-------------------------------
         *  GroupBy(keySelector)                     | Key only; elements = source items
         *  GroupBy(keySelector, elementSelector)    | Project each member before bucketing
         *  GroupBy(keySelector, resultSelector)     | Emit one TResult per group (no IGrouping out)
         *  GroupBy(key, element, result)            | Project members AND fold each group
         *  … + IEqualityComparer<TKey>              | Custom key equality (case, culture, …)
         *
         * Return of the basic overloads:
         *   IEnumerable<IGrouping<TKey, TElement>>  — deferred until you enumerate
         *
         * Scenario: twelve open tickets across priorities, categories, and
         * assignees so buckets are multi-item and nestable.
         * -------------------------------------------------------------------------
         */

        List<Ticket> board =
        [
            new Ticket(1,  "VPN down",           "P1", "Network",  "Ada",  6),
            new Ticket(2,  "Slow Wi-Fi",         "P2", "Network",  "Bob",  14),
            new Ticket(3,  "Firewall rule",      "P1", "Network",  "Ada",  3),
            new Ticket(4,  "Laptop replace",     "P3", "Hardware", "Cara", 40),
            new Ticket(5,  "Broken keyboard",    "P2", "Hardware", "Bob",  18),
            new Ticket(6,  "Docking station",    "P3", "Hardware", "Cara", 28),
            new Ticket(7,  "License renew",      "P2", "Software", "Ada",  10),
            new Ticket(8,  "Install IDE",        "P3", "Software", "Dan",  8),
            new Ticket(9,  "SSO outage",         "P1", "Software", "Ada",  2),
            new Ticket(10, "Badge reader",       "P2", "Hardware", "Dan",  22),
            new Ticket(11, "DNS cache poison",   "P1", "Network",  "Bob",  5),
            new Ticket(12, "Patch Tuesday fail", "P2", "Software", "Cara", 16),
        ];

        Console.WriteLine("=== Support board (raw) ===");
        PrintBoard(board);

        /*
         * =========================================================================
         * SECTION 3: GroupBy — KEY SELECTOR (METHOD SYNTAX)
         * =========================================================================
         *
         * Simplest overload — only a key selector:
         *
         *   IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
         *       Func<TSource, TKey> keySelector)
         *
         *   board.GroupBy(ticket => ticket.Priority)
         *
         * TElement defaults to TSource (Ticket). GroupBy is DEFERRED — the
         * partition runs when you foreach the result (same model as
         * 01. Introduction to LINQ).
         *
         * --- 3a. Group by Priority ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<IGrouping<string, Ticket>> byPriority =
            board.GroupBy(ticket => ticket.Priority); // key = Priority string

        Console.WriteLine();
        Console.WriteLine("--- GroupBy(key): by Priority ---");
        PrintGroups(byPriority, group => $"Priority {group.Key}");

        /*
         * =========================================================================
         * SECTION 4: IGrouping — KEY AND NESTED ITERATION
         * =========================================================================
         *
         * Each element from GroupBy implements IGrouping<TKey, TElement>:
         *
         *  Member / shape              | Meaning
         *  ----------------------------|------------------------------------------
         *  .Key                        | Shared key for this bucket
         *  IEnumerable<TElement>       | The members — a group IS a sequence
         *  foreach (var item in group) | Walk members of this bucket only
         *
         * Because a group is IEnumerable<TElement>, you can OrderBy, Where,
         * Count, Average, … on that bucket alone — not the whole board.
         *
         * --- 4a. Group by Category — print key, then members ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<IGrouping<string, Ticket>> byCategory =
            board.GroupBy(ticket => ticket.Category); // key = Category

        Console.WriteLine();
        Console.WriteLine("--- IGrouping: by Category (key + nested members) ---");
        foreach (IGrouping<string, Ticket> categoryGroup in byCategory.OrderBy(g => g.Key))
        {
            Console.WriteLine($"  {categoryGroup.Key}: {categoryGroup.Count()} ticket(s)"); // Key + Count on bucket
            foreach (Ticket ticket in categoryGroup.OrderBy(t => t.TicketId))
            {
                Console.WriteLine($"    #{ticket.TicketId} {ticket.Title} ({ticket.Priority})");
            }
        }

        /*
         * =========================================================================
         * SECTION 5: ELEMENT SELECTOR — PROJECT MEMBERS INSIDE EACH GROUP
         * =========================================================================
         *
         * Second Func projects each source item into TElement before it lands
         * in its bucket:
         *
         *   GroupBy(keySelector, elementSelector)
         *     → IEnumerable<IGrouping<TKey, TElement>>
         *
         *   board.GroupBy(t => t.Assignee, t => t.Title)
         *     → IGrouping<string, string>   // Key=assignee, members=titles
         *
         * Use this when consumers only need a slice of each row (titles, ids)
         * and you want that shape baked into the grouping result.
         *
         * --- 5a. Assignee → ticket titles ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<IGrouping<string, string>> titlesByAssignee =
            board.GroupBy(
                ticket => ticket.Assignee, // keySelector
                ticket => ticket.Title);   // elementSelector → TElement = string

        Console.WriteLine();
        Console.WriteLine("--- GroupBy(key, element): titles per Assignee ---");
        foreach (IGrouping<string, string> titleGroup in titlesByAssignee.OrderBy(g => g.Key))
        {
            string titles = string.Join("; ", titleGroup); // members are strings
            Console.WriteLine($"  {titleGroup.Key}: {titles}");
        }

        /*
         * =========================================================================
         * SECTION 6: RESULT SELECTOR — ONE OUTPUT PER GROUP
         * =========================================================================
         *
         * resultSelector folds each group into a single TResult. You do not
         * receive IGrouping in the outer sequence — you receive whatever the
         * result selector returns.
         *
         *  Overload                                         | resultSelector args
         *  -------------------------------------------------|--------------------
         *  GroupBy(key, result)                             | (TKey key, IEnumerable<TSource> items)
         *  GroupBy(key, element, result)                    | (TKey key, IEnumerable<TElement> items)
         *
         * This is the idiomatic "GROUP BY … SELECT aggregates" shape in one call.
         * Chaining .Select after GroupBy (Section 10) does the same job in two steps.
         *
         * --- 6a. key + result — headcount and average hours per priority ---
         * --- 6b. key + element + result — join projected titles into one label ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<(string Priority, int Count, double AvgHours)> priorityStats =
            board.GroupBy(
                ticket => ticket.Priority,
                (priority, tickets) => (
                    Priority: priority,
                    Count: tickets.Count(),
                    AvgHours: tickets.Average(t => t.HoursOpen))); // resultSelector folds the group

        Console.WriteLine();
        Console.WriteLine("--- GroupBy(key, result): priority stats ---");
        Console.WriteLine("  Priority   Count   Avg Hours");
        foreach (var row in priorityStats.OrderBy(r => r.Priority))
        {
            Console.WriteLine($"  {row.Priority,-8}  {row.Count,5}  {row.AvgHours,9:F1}");
        }

        IEnumerable<string> categoryTitleLabels =
            board.GroupBy(
                ticket => ticket.Category,
                ticket => ticket.Title, // element = title only
                (category, titles) =>
                    $"{category}: {string.Join(", ", titles)}"); // fold titles

        Console.WriteLine();
        Console.WriteLine("--- GroupBy(key, element, result): label per category ---");
        foreach (string label in categoryTitleLabels.OrderBy(l => l))
        {
            Console.WriteLine($"  {label}");
        }

        /*
         * =========================================================================
         * SECTION 7: QUERY SYNTAX — group … by … AND into
         * =========================================================================
         *
         * Query syntax uses the `group` clause. The compiler emits GroupBy:
         *
         *   from t in board
         *   group t by t.Priority;
         *
         * Element projection in the group clause maps to the element selector:
         *
         *   group t.Title by t.Assignee   → IGrouping<string, string>
         *
         * `into` continues the query after grouping (orderby / select on groups):
         *
         *   group t by t.Priority into g
         *   orderby g.Key
         *   select g;
         *
         * --- 7a. Basic group by ---
         * --- 7b. Element projection in group clause ---
         * --- 7c. into + orderby on groups ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<IGrouping<string, Ticket>> byPriorityQuery =
            from ticket in board
            group ticket by ticket.Priority; // same as GroupBy(t => t.Priority)

        Console.WriteLine();
        Console.WriteLine("--- Query: group ticket by Priority ---");
        PrintGroupKeysOnly(byPriorityQuery);

        IEnumerable<IGrouping<string, string>> titlesByAssigneeQuery =
            from ticket in board
            group ticket.Title by ticket.Assignee; // element selector in query form

        Console.WriteLine();
        Console.WriteLine("--- Query: group Title by Assignee ---");
        foreach (IGrouping<string, string> titleGroup in titlesByAssigneeQuery.OrderBy(g => g.Key))
        {
            Console.WriteLine($"  {titleGroup.Key} ({titleGroup.Count()}): {string.Join("; ", titleGroup)}");
        }

        IEnumerable<IGrouping<string, Ticket>> prioritiesOrdered =
            from ticket in board
            group ticket by ticket.Priority into priorityGroup // continue after GroupBy
            orderby priorityGroup.Key
            select priorityGroup;

        Console.WriteLine();
        Console.WriteLine("--- Query: group … into … orderby Key ---");
        PrintGroupKeysOnly(prioritiesOrdered);

        /*
         * =========================================================================
         * SECTION 8: COMPOSITE AND ANONYMOUS GROUP KEYS
         * =========================================================================
         *
         * The key can be any type — int, string, tuple, anonymous object.
         * Use a composite key when one field alone does not define the bucket.
         *
         *  Form                              | Notes
         *  ----------------------------------|------------------------------------------
         *  new { t.Category, t.Priority }    | Anonymous type; equality by property values
         *  (t.Category, t.Priority)          | ValueTuple; value equality
         *
         * --- 8a. Anonymous key: category + priority ---
         * --- 8b. ValueTuple key: category + assignee ---
         * -------------------------------------------------------------------------
         */

        var byCategoryAndPriority =
            from ticket in board
            group ticket by new { ticket.Category, ticket.Priority } into cohort
            orderby cohort.Key.Category, cohort.Key.Priority
            select cohort;

        Console.WriteLine();
        Console.WriteLine("--- Composite key (anonymous): Category + Priority ---");
        foreach (var cohort in byCategoryAndPriority)
        {
            string titles = string.Join(", ", cohort.Select(t => t.Title));
            Console.WriteLine(
                $"  {cohort.Key.Category}/{cohort.Key.Priority}: [{titles}]");
        }

        IEnumerable<IGrouping<(string Category, string Assignee), Ticket>> byCategoryAndAssignee =
            board.GroupBy(ticket => (ticket.Category, ticket.Assignee)); // ValueTuple key

        Console.WriteLine();
        Console.WriteLine("--- Composite key (tuple): Category + Assignee ---");
        foreach (var group in byCategoryAndAssignee
                     .OrderBy(g => g.Key.Category)
                     .ThenBy(g => g.Key.Assignee))
        {
            string titles = string.Join(", ", group.Select(t => t.Title));
            Console.WriteLine($"  {group.Key.Category}/{group.Key.Assignee}: [{titles}]");
        }

        /*
         * =========================================================================
         * SECTION 9: AGGREGATING PER GROUP
         * =========================================================================
         *
         * Each IGrouping is IEnumerable<TElement>, so every aggregate from
         * 02. Filtering and Aggregation applies PER GROUP:
         *
         *   group.Count() / Sum / Average / Min / Max
         *
         * These run only over members of that bucket.
         *
         * --- 9a. Inline aggregates while iterating category groups ---
         * -------------------------------------------------------------------------
         */

        Console.WriteLine();
        Console.WriteLine("--- Per-group aggregates by Category ---");
        Console.WriteLine("  Category   Count   Total Hrs   Avg Hrs   Oldest");
        foreach (IGrouping<string, Ticket> categoryGroup in
                 board.GroupBy(t => t.Category).OrderBy(g => g.Key))
        {
            int count = categoryGroup.Count();
            int totalHours = categoryGroup.Sum(t => t.HoursOpen);
            double avgHours = categoryGroup.Average(t => t.HoursOpen);
            int oldest = categoryGroup.Max(t => t.HoursOpen);

            Console.WriteLine(
                $"  {categoryGroup.Key,-8}  {count,5}  {totalHours,10}  {avgHours,7:F1}  {oldest,6}");
        }

        /*
         * =========================================================================
         * SECTION 10: Select AFTER GroupBy — SUMMARY ROWS (PROJECTION PREVIEW)
         * =========================================================================
         *
         * Often you want one flat output row per group. Chain Select after
         * GroupBy (or use a result selector — Section 6):
         *
         *   board.GroupBy(t => t.Priority)
         *        .Select(g => new { g.Key, MaxHours = g.Max(t => t.HoursOpen) })
         *
         * Here Select is only the glue that flattens groups into summary rows.
         * Anonymous types, named records, and SelectMany depth live in
         * COVERED IN DETAIL LATER → 08. Projection Operations
         *
         * --- 10a. Max open hours per priority (method) ---
         * --- 10b. Headcount per assignee (query) ---
         * -------------------------------------------------------------------------
         */

        var maxHoursByPriority = board
            .GroupBy(ticket => ticket.Priority)
            .Select(group => new KeyValuePair<string, int>(
                group.Key,
                group.Max(ticket => ticket.HoursOpen))) // one summary row per group
            .OrderBy(pair => pair.Key);

        var headcountByAssignee =
            from assigneeGroup in board.GroupBy(t => t.Assignee)
            orderby assigneeGroup.Key
            select new { Assignee = assigneeGroup.Key, Count = assigneeGroup.Count() };

        Console.WriteLine();
        Console.WriteLine("--- Select after GroupBy: max hours per priority ---");
        foreach (KeyValuePair<string, int> row in maxHoursByPriority)
        {
            Console.WriteLine($"  {row.Key}: oldest ticket open = {row.Value}h");
        }

        Console.WriteLine();
        Console.WriteLine("--- Select after GroupBy: headcount per assignee ---");
        foreach (var row in headcountByAssignee)
        {
            Console.WriteLine($"  {row.Assignee}: {row.Count} ticket(s)");
        }

        /*
         * =========================================================================
         * SECTION 11: NESTED GROUPING BASICS
         * =========================================================================
         *
         * Nested grouping means: group once, then GroupBy again on each
         * group's members (or project inner groups from a result/Select).
         *
         * Pattern:
         *
         *   outer = board.GroupBy(t => t.Category)
         *   for each outer group:
         *       inner = outerGroup.GroupBy(t => t.Priority)
         *
         * Result shape is hierarchical: Category → Priority → Tickets.
         * Useful for tree views and indented reports. Keep nesting shallow —
         * deep nests are hard to read; prefer flat composite keys (Section 8)
         * when a single-level partition is enough.
         *
         * --- 11a. Category outer, Priority inner ---
         * -------------------------------------------------------------------------
         */

        Console.WriteLine();
        Console.WriteLine("--- Nested grouping: Category → Priority → titles ---");
        foreach (IGrouping<string, Ticket> categoryGroup in
                 board.GroupBy(t => t.Category).OrderBy(g => g.Key))
        {
            Console.WriteLine($"  {categoryGroup.Key}");
            foreach (IGrouping<string, Ticket> priorityGroup in
                     categoryGroup.GroupBy(t => t.Priority).OrderBy(g => g.Key)) // group members again
            {
                string titles = string.Join(", ", priorityGroup.Select(t => t.Title));
                Console.WriteLine($"    {priorityGroup.Key}: {titles}");
            }
        }

        /*
         * =========================================================================
         * SECTION 12: ToLookup — IMMEDIATE ILookup
         * =========================================================================
         *
         * ToLookup builds a keyed multi-map eagerly — fits the grouping story
         * when you need random access by key after one pass:
         *
         *  Aspect       | GroupBy                              | ToLookup
         *  -------------|--------------------------------------|---------------------------
         *  When it runs | Deferred (on foreach)                | Immediate (on the call)
         *  Return type  | IEnumerable<IGrouping<…>>            | ILookup<TKey, TElement>
         *  Access       | Walk groups sequentially             | map[key] → members
         *  Missing key  | N/A (no indexer)                     | Empty sequence (no throw)
         *  Contains     | (enumerate + Any)                    | map.Contains(key)
         *
         *   ILookup<string, Ticket> map = board.ToLookup(t => t.Assignee);
         *   IEnumerable<Ticket> ada = map["Ada"];
         *
         * ToLookup also has elementSelector and comparer overloads (same idea
         * as GroupBy). Other materialization (ToList, ToDictionary) and the
         * conversion-focused comparison of ToDictionary vs ToLookup live in
         * COVERED IN DETAIL LATER → 10. Conversion Operations
         *
         * --- 12a. Index by assignee; missing key is empty ---
         * --- 12b. Element selector + Contains ---
         * -------------------------------------------------------------------------
         */

        ILookup<string, Ticket> lookupByAssignee = board.ToLookup(ticket => ticket.Assignee); // immediate

        Console.WriteLine();
        Console.WriteLine("--- ToLookup: map[\"Ada\"] and missing key ---");
        Console.WriteLine(
            $"  Lookup contains {lookupByAssignee.Count} distinct assignee keys (built immediately).");
        Console.WriteLine($"  Contains(\"Ada\"): {lookupByAssignee.Contains("Ada")}");
        foreach (Ticket ticket in lookupByAssignee["Ada"].OrderBy(t => t.TicketId))
        {
            Console.WriteLine($"    Ada: #{ticket.TicketId} {ticket.Title}");
        }

        int missingKeyCount = lookupByAssignee["Zoe"].Count(); // missing key → empty, not exception
        Console.WriteLine(
            $"  lookupByAssignee[\"Zoe\"] (missing) → empty sequence, Count = {missingKeyCount}");
        Console.WriteLine($"  Contains(\"Zoe\"): {lookupByAssignee.Contains("Zoe")}");

        ILookup<string, string> titlesLookup =
            board.ToLookup(t => t.Category, t => t.Title); // key + element selectors

        Console.WriteLine(
            $"  ToLookup(key, element) Network titles: {string.Join(", ", titlesLookup["Network"])}");

        /*
         * =========================================================================
         * SECTION 13: CUSTOM KEY EQUALITY (IEqualityComparer)
         * =========================================================================
         *
         * GroupBy / ToLookup accept an optional IEqualityComparer<TKey> so keys
         * that differ only by case (or culture) land in the same bucket.
         *
         *   board.GroupBy(t => t.Category, StringComparer.OrdinalIgnoreCase)
         *
         * Without a comparer, string keys use default equality (case-sensitive).
         * The .Key value is the first-seen casing for that equality class.
         *
         * --- 13a. Case-insensitive category keys on mixed casing ---
         * -------------------------------------------------------------------------
         */

        List<Ticket> mixedCaseBoard =
        [
            new Ticket(101, "Cable swap",   "P3", "network",  "Eve", 4),
            new Ticket(102, "Switch reboot","P1", "Network",  "Eve", 1),
            new Ticket(103, "Mouse fail",   "P2", "hardware", "Eve", 9),
        ];

        IEnumerable<IGrouping<string, Ticket>> caseInsensitive =
            mixedCaseBoard.GroupBy(
                t => t.Category,
                StringComparer.OrdinalIgnoreCase); // "network" and "Network" share one bucket

        Console.WriteLine();
        Console.WriteLine("--- GroupBy with StringComparer.OrdinalIgnoreCase ---");
        foreach (IGrouping<string, Ticket> group in caseInsensitive.OrderBy(g => g.Key))
        {
            string titles = string.Join(", ", group.Select(t => t.Title));
            Console.WriteLine($"  Key \"{group.Key}\" → [{titles}]"); // first-seen casing
        }

        ILookup<string, string> caseInsensitiveLookup =
            mixedCaseBoard.ToLookup(
                t => t.Category,
                t => t.Title,
                StringComparer.OrdinalIgnoreCase); // ToLookup + comparer + element

        Console.WriteLine(
            $"  ToLookup ignore-case Network: {string.Join(", ", caseInsensitiveLookup["NETWORK"])}");

        /*
         * =========================================================================
         * SECTION 14: GroupBy vs GroupJoin — PREVIEW
         * =========================================================================
         *
         * Easy to confuse — different operators, different inputs:
         *
         *  Operator   | Input                         | Role
         *  -----------|-------------------------------|------------------------------
         *  GroupBy    | One sequence                  | Partition by a key on itself
         *  GroupJoin  | Outer + inner sequences       | Attach matching inners per outer
         *
         * GroupJoin is the LINQ shape behind SQL LEFT JOIN … GROUP / hierarchical
         * "customer with their orders" results.
         *
         * COVERED IN DETAIL LATER → 05. Joins
         *   (Join, GroupJoin, left outer join with DefaultIfEmpty)
         *
         * COVERED IN DETAIL LATER → 08. Projection Operations
         *   (Select / SelectMany depth beyond summary-row glue)
         * -------------------------------------------------------------------------
         */

        Console.WriteLine();
        Console.WriteLine("--- GroupBy vs GroupJoin (preview) ---");
        Console.WriteLine(
            $"  This chapter: GroupBy alone → {byPriority.Count()} priority buckets from one board.");
        Console.WriteLine(
            "  GroupJoin (two sequences, hierarchical left shape) → 05. Joins.");
        Console.WriteLine(
            "  Select / SelectMany depth after grouping → 08. Projection Operations.");
    }

    private static void PrintBoard(IEnumerable<Ticket> tickets)
    {
        foreach (Ticket ticket in tickets)
        {
            Console.WriteLine(
                $"  #{ticket.TicketId,2}  {ticket.Priority}  {ticket.Category,-8}  " +
                $"{ticket.Assignee,-4}  {ticket.HoursOpen,2}h  {ticket.Title}");
        }
    }

    private static void PrintGroups(
        IEnumerable<IGrouping<string, Ticket>> groups,
        Func<IGrouping<string, Ticket>, string> heading)
    {
        foreach (IGrouping<string, Ticket> group in groups.OrderBy(g => g.Key))
        {
            Console.WriteLine($"  {heading(group)} ({group.Count()})");
            foreach (Ticket ticket in group.OrderBy(t => t.TicketId))
            {
                Console.WriteLine($"    #{ticket.TicketId} {ticket.Title} → {ticket.Assignee}");
            }
        }
    }

    private static void PrintGroupKeysOnly(IEnumerable<IGrouping<string, Ticket>> groups)
    {
        string keys = string.Join(", ",
            groups.OrderBy(g => g.Key).Select(g => $"{g.Key}({g.Count()})"));
        Console.WriteLine($"  Group keys: {keys}");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — GROUPING
 * =========================================================================
 *
 * --- GroupBy overloads (method) ---
 *
 *   seq.GroupBy(x => x.Key)
 *   seq.GroupBy(x => x.Key, x => x.Name)                    // element selector
 *   seq.GroupBy(x => x.Key, (k, items) => …)                // result selector
 *   seq.GroupBy(x => x.Key, x => x.Name, (k, names) => …)   // element + result
 *   seq.GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
 *
 * --- Query syntax ---
 *
 *   from x in seq group x by x.Key
 *   from x in seq group x.Name by x.Key
 *   from x in seq group x by x.Key into g orderby g.Key select g
 *
 * --- IGrouping<TKey, TElement> ---
 *
 *   .Key                         shared key
 *   foreach (item in group)      members
 *   group.Count() / Average(…)   aggregates on this bucket only
 *
 * --- Nested grouping ---
 *
 *   foreach (var outer in seq.GroupBy(x => x.A))
 *       foreach (var inner in outer.GroupBy(x => x.B)) { … }
 *
 * --- ToLookup ---
 *
 *   ILookup<TKey,T> map = seq.ToLookup(x => x.Key);         // immediate
 *   map[key]              → IEnumerable<T> (empty if absent)
 *   map.Contains(key)     → bool
 *   seq.ToLookup(x => x.Key, x => x.Name)                   // element selector
 *   seq.ToLookup(x => x.Key, comparer)                      // custom equality
 *
 * --- GroupBy vs ToLookup ---
 *
 *   GroupBy     deferred; walk groups when needed
 *   ToLookup    immediate; random access by key
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result / fix
 *  -------------------------------------|----------------------------------------
 *  Aggregate without GroupBy first      | Runs on whole sequence
 *  Using .Key outside IGrouping         | CS0103 — Key exists on IGrouping only
 *  Expecting ILookup[missing] to throw  | Returns empty sequence
 *  Confusing GroupBy with GroupJoin     | GroupJoin needs two sequences → ch.05
 *  Second OrderBy instead of ThenBy     | Replaces sort (see 03. Ordering)
 *
 * --- Related chapters ---
 *
 *   02. Filtering and Aggregation   Count, Sum, Average, Min, Max
 *   03. Ordering                    Order groups or members inside groups
 *   05. Joins                       GroupJoin / left outer join shape
 *   08. Projection Operations       Select / SelectMany after grouping
 *   10. Conversion Operations       ToList, ToDictionary, ToLookup conversion focus
 *
 * =========================================================================
 */
