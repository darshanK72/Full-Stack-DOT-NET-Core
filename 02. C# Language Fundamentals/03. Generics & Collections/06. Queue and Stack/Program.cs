/*
 * =============================================================================
 * 06. QUEUE AND STACK — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Generic Queue<T> (FIFO) and Stack<T> (LIFO) — restricted-access
 *        collections where you add and remove only at defined ends.
 *
 * WHY IT MATTERS:
 *   Many real workflows are strictly ordered: print jobs, support tickets,
 *   breadth-first graph search, and undo/redo history. Queue and Stack encode
 *   those rules in the type itself so you cannot accidentally remove the wrong
 *   end — unlike List<T>, which allows Insert/Remove anywhere.
 *
 * WHAT YOU WILL LEARN:
 *   1.  FIFO vs LIFO — when each ordering model applies
 *   2.  Queue<T> — Enqueue, Dequeue, Peek, TryDequeue, TryPeek, Count,
 *       Clear, Contains, ToArray
 *   3.  Stack<T> — Push, Pop, Peek, TryPop, TryPeek, Count
 *   4.  Real-world wiring — ticket queue (FIFO) plus response undo (LIFO)
 *   5.  BFS — Queue<T> drives breadth-first graph traversal
 *   6.  Non-generic Queue / Stack (brief preview vs generic counterparts)
 *
 * =============================================================================
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace QueueAndStack;

/*
 * =========================================================================
 * SECTION 1: FIFO VS LIFO — TWO END-RESTRICTED COLLECTIONS
 * =========================================================================
 *
 * Both Queue<T> and Stack<T> live in System.Collections.Generic.
 * They implement IEnumerable<T> (iteration order covered later in
 * 08. IEnumerable and IEnumerator).
 *
 *  Collection   | Ordering acronym | Add at…      | Remove from…
 *  -------------|------------------|--------------|------------------
 *  Queue<T>     | FIFO             | back (tail)  | front (head)
 *               | First In,        | Enqueue      | Dequeue
 *               | First Out        |              |
 *  Stack<T>     | LIFO             | top          | top
 *               | Last In,         | Push         | Pop
 *               | First Out        |              |
 *
 * Mental model:
 *
 *   Queue (line at a counter):  [A] [B] [C]  ← Enqueue adds here
 *                               ↑
 *                               Dequeue removes here (oldest first)
 *
 *   Stack (plate pile):         Push/Pop both at top
 *                                     [C]  ← top
 *                                     [B]
 *                                     [A]
 *
 * Neither type supports random access by index — use List<T> when you
 * need indexing, sorting, or Insert-at-any-position.
 * -------------------------------------------------------------------------
 */
public static class OrderingConcepts
{
    public static void PrintSummary()
    {
        Console.WriteLine("=== Queue<T> and Stack<T> ===");
        Console.WriteLine("FIFO (Queue): first Enqueued is first Dequeued — fair waiting line.");
        Console.WriteLine("LIFO (Stack): last Pushed is first Popped — undo / call stack.");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 2: Queue<T> — FIFO OPERATIONS
 * =========================================================================
 *
 * Queue<T> wraps a circular buffer internally. Common members:
 *
 *  Member              | Role
 *  --------------------|--------------------------------------------------
 *  Enqueue(item)       | Add to the back (tail)
 *  Dequeue()           | Remove and return the front (head); throws if empty
 *  Peek()              | Inspect front without removing; throws if empty
 *  TryDequeue(out T)   | Safe remove — returns false when empty (no exception)
 *  TryPeek(out T)      | Safe inspect front — returns false when empty
 *  Count               | Number of items currently queued
 *  Contains(item)      | Linear search — O(n)
 *  Clear()             | Remove all items
 *  ToArray()           | Snapshot copy in FIFO order (front → back)
 *
 * --- 2a. Empty queue behavior ---
 *
 *   queue.Dequeue();   // InvalidOperationException — "Queue empty."
 *   queue.Peek();      // Same exception
 *
 * Prefer TryDequeue / TryPeek when empty is an expected state:
 *
 *   if (queue.TryDequeue(out string? next))
 *       Process(next);
 *
 * --- 2b. Iteration order ---
 *
 * foreach walks front-to-back (same order as Dequeue would drain).
 * -------------------------------------------------------------------------
 */
public static class QueueDemo
{
    public static void Run()
    {
        Queue<string> printJobs = new Queue<string>();
        printJobs.Enqueue("Invoice-1042.pdf");  // tail ← newest arrival at back
        printJobs.Enqueue("Report-Q3.pdf");
        printJobs.Enqueue("Label-A17.pdf");

        Console.WriteLine("--- Section 2: Queue<T> FIFO ---");
        Console.WriteLine($"Jobs waiting (Count): {printJobs.Count}");
        Console.WriteLine($"Next to print (Peek): {printJobs.Peek()}");

        if (printJobs.TryPeek(out string? peekedFront))
        {
            Console.WriteLine($"TryPeek front without removing: {peekedFront}");
        }

        Console.WriteLine($"Contains Report-Q3.pdf: {printJobs.Contains("Report-Q3.pdf")}");

        string firstJob = printJobs.Dequeue(); // head — oldest job leaves first
        Console.WriteLine($"Dequeued (oldest): {firstJob}");
        Console.WriteLine($"Remaining count: {printJobs.Count}");

        Console.Write("Snapshot (ToArray): ");
        foreach (string job in printJobs.ToArray()) // copy — queue unchanged
        {
            Console.Write(job + " ");
        }

        Console.WriteLine();

        printJobs.Clear(); // drain without reading each item
        Console.WriteLine($"After Clear — Count: {printJobs.Count}");

        bool removedWhenEmpty = printJobs.TryDequeue(out _);
        Console.WriteLine($"TryDequeue on empty queue: {removedWhenEmpty}"); // false — no exception
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 3: Stack<T> — LIFO OPERATIONS
 * =========================================================================
 *
 * Stack<T> uses an array that grows as needed (similar resize strategy
 * to List<T>).
 *
 *  Member              | Role
 *  --------------------|--------------------------------------------------
 *  Push(item)          | Add on top
 *  Pop()               | Remove and return top; throws if empty
 *  Peek()              | Inspect top without removing; throws if empty
 *  TryPop(out T)       | Safe remove top — returns false when empty
 *  TryPeek(out T)      | Safe inspect top — returns false when empty
 *  Count               | Items on the stack
 *
 * --- 3a. Empty stack behavior ---
 *
 *   stack.Pop();   // InvalidOperationException — "Stack empty."
 *   stack.Peek();  // Same exception
 *
 * --- 3b. Iteration order ---
 *
 * foreach walks top-to-bottom (most recently pushed first).
 * -------------------------------------------------------------------------
 */
public static class StackDemo
{
    public static void Run()
    {
        Stack<int> callStackPreview = new Stack<int>();
        callStackPreview.Push(100); // bottom of stack
        callStackPreview.Push(200);
        callStackPreview.Push(300); // top — most recent frame

        Console.WriteLine("--- Section 3: Stack<T> LIFO ---");
        Console.WriteLine($"Depth (Count): {callStackPreview.Count}");
        Console.WriteLine($"Top (Peek): {callStackPreview.Peek()}");

        if (callStackPreview.TryPeek(out int peekedTop))
        {
            Console.WriteLine($"TryPeek top without removing: {peekedTop}");
        }

        int popped = callStackPreview.Pop(); // removes 300 — last in, first out
        Console.WriteLine($"Pop removed most recent: {popped}");
        Console.WriteLine($"New top (Peek): {callStackPreview.Peek()}");

        Console.Write("foreach (top → bottom): ");
        foreach (int frame in callStackPreview)
        {
            Console.Write(frame + " ");
        }

        Console.WriteLine();

        callStackPreview.Clear();
        bool poppedWhenEmpty = callStackPreview.TryPop(out _);
        Console.WriteLine($"TryPop on empty stack: {poppedWhenEmpty}"); // false — no exception
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 4: REAL-WORLD — QUEUE + STACK TOGETHER
 * =========================================================================
 *
 * Scenario: a help-desk agent handles tickets in arrival order (Queue<T>)
 * while editing a draft response with undo support (Stack<T>).
 *
 *   Queue<Ticket>  — FIFO: oldest customer issue is resolved first
 *   Stack<string>  — LIFO: undo reverts the most recent keystroke batch
 *
 * Workflow:
 *   1. Enqueue tickets as they arrive
 *   2. Dequeue the next ticket; agent types a response
 *   3. Push a snapshot before each edit batch; Pop to undo
 * -------------------------------------------------------------------------
 */
public readonly record struct SupportTicket(int Id, string Subject);

public sealed class HelpDeskSession
{
    private readonly Queue<SupportTicket> _ticketQueue = new Queue<SupportTicket>();
    private readonly Stack<string> _responseUndoStack = new Stack<string>();
    private readonly StringBuilder _draftResponse = new StringBuilder();

    public void EnqueueTicket(SupportTicket ticket) => _ticketQueue.Enqueue(ticket);

    public int PendingTicketCount => _ticketQueue.Count;

    public bool TryPeekNextTicket(out SupportTicket ticket) => _ticketQueue.TryPeek(out ticket);

    public bool TryResolveNextTicket(out SupportTicket resolved)
    {
        return _ticketQueue.TryDequeue(out resolved);
    }

    public void BeginResponse(string openingLine)
    {
        _responseUndoStack.Clear();
        _draftResponse.Clear();
        _draftResponse.Append(openingLine);
    }

    public void ApplyEdit(Action<StringBuilder> edit, string label)
    {
        _responseUndoStack.Push(_draftResponse.ToString()); // snapshot before change
        edit(_draftResponse);
        Console.WriteLine($"    [{label}] applied — undo depth {_responseUndoStack.Count}");
    }

    public string CurrentDraft => _draftResponse.ToString();

    public bool TryUndo(out string restoredDraft)
    {
        if (!_responseUndoStack.TryPop(out string? previous))
        {
            restoredDraft = _draftResponse.ToString();
            return false;
        }

        _draftResponse.Clear();
        _draftResponse.Append(previous);
        restoredDraft = previous;
        return true;
    }
}

/*
 * =========================================================================
 * SECTION 5: BFS WITH Queue<T>
 * =========================================================================
 *
 * Breadth-First Search (BFS) explores a graph layer by layer:
 *   - Start at a node, Enqueue it
 *   - While queue not empty: TryDequeue current, Enqueue unvisited neighbors
 *
 * BFS finds shortest paths in unweighted graphs because nodes are
 * discovered in order of increasing distance from the start.
 *
 * Grid legend for this 4×4 maze (. = open, # = wall, S = start, G = goal):
 *
 *   S . # .
 *   . . . #
 *   # . . .
 *   . # . G
 *
 * BFS visits cells in expanding rings from S until G is reached.
 * -------------------------------------------------------------------------
 */
public static class BreadthFirstSearch
{
    public static (int Row, int Col)? Run(char[,] grid, int startRow, int startCol)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        bool[,] visited = new bool[rows, cols];

        Queue<(int Row, int Col)> frontier = new Queue<(int Row, int Col)>();
        frontier.Enqueue((startRow, startCol));
        visited[startRow, startCol] = true;

        int[] rowDelta = { -1, 1, 0, 0 };
        int[] colDelta = { 0, 0, -1, 1 };

        while (frontier.TryDequeue(out (int Row, int Col) current))
        {
            if (grid[current.Row, current.Col] == 'G')
            {
                return current;
            }

            for (int d = 0; d < 4; d++)
            {
                int nextRow = current.Row + rowDelta[d];
                int nextCol = current.Col + colDelta[d];

                if (nextRow < 0 || nextRow >= rows || nextCol < 0 || nextCol >= cols)
                {
                    continue;
                }

                if (visited[nextRow, nextCol])
                {
                    continue;
                }

                char cell = grid[nextRow, nextCol];
                if (cell == '#')
                {
                    continue;
                }

                visited[nextRow, nextCol] = true;
                frontier.Enqueue((nextRow, nextCol));
            }
        }

        return null;
    }

    public static void RunDemo()
    {
        char[,] maze =
        {
            { 'S', '.', '#', '.' },
            { '.', '.', '.', '#' },
            { '#', '.', '.', '.' },
            { '.', '#', '.', 'G' }
        };

        (int Row, int Col)? goal = Run(maze, startRow: 0, startCol: 0);

        Console.WriteLine("--- Section 5: BFS with Queue<(row, col)> ---");
        if (goal.HasValue)
        {
            Console.WriteLine($"Goal reached at row {goal.Value.Row}, col {goal.Value.Col}");
        }
        else
        {
            Console.WriteLine("Goal not reachable.");
        }

        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 6: NON-GENERIC Queue AND Stack (PREVIEW)
 * =========================================================================
 *
 * Before generics, System.Collections offered:
 *
 *   System.Collections.Queue   — non-generic FIFO (stores object)
 *   System.Collections.Stack   — non-generic LIFO (stores object)
 *
 * Every Enqueue/Push boxes value types into object; every Dequeue/Pop
 * requires a cast (or unbox) back to the original type — runtime errors
 * if you guess wrong.
 *
 *   Queue legacy = new Queue();
 *   legacy.Enqueue(42);           // boxes int → object
 *   int n = (int)legacy.Dequeue(); // unbox — InvalidCastException if wrong
 *
 * Generic Queue<T> / Stack<T> eliminate boxing for value types and give
 * compile-time type safety — always prefer them in new code.
 *
 * COVERED IN DETAIL LATER → 03. Generics & Collections / 02. ArrayList
 *   (headline concepts: non-generic Stack/Queue overview, boxing, Hashtable)
 *
 * Side-by-side API mapping:
 *
 *  Generic (preferred)     | Non-generic legacy
 *  ------------------------|----------------------------
 *  Queue<T>.Enqueue        | Queue.Enqueue(object)
 *  Queue<T>.Dequeue()      | Queue.Dequeue() → object
 *  Stack<T>.Push           | Stack.Push(object)
 *  Stack<T>.Pop()          | Stack.Pop() → object
 * -------------------------------------------------------------------------
 */
public static class LegacyCollectionPreview
{
    public static void Run()
    {
        Queue legacyQueue = new Queue();
        legacyQueue.Enqueue(42);              // boxes int
        legacyQueue.Enqueue("ticket");        // reference stored as object
        int first = (int)legacyQueue.Dequeue()!; // unbox — wrong cast throws at runtime

        Stack legacyStack = new Stack();
        legacyStack.Push("draft-v1");
        legacyStack.Push("draft-v2");
        string topDraft = (string)legacyStack.Pop()!; // LIFO — "draft-v2"

        Console.WriteLine("--- Section 6: Non-generic preview ---");
        Console.WriteLine($"Legacy Queue Dequeue (unboxed int): {first}");
        Console.WriteLine($"Legacy Stack Pop (cast string): {topDraft}");
        Console.WriteLine("Prefer Queue<T> and Stack<T> — see 02. ArrayList for full legacy coverage.");
        Console.WriteLine();
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 7: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Runs each section in reading order. Section 4 wires Queue and Stack
     * in one help-desk scenario; other sections isolate each API family.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        OrderingConcepts.PrintSummary();
        QueueDemo.Run();
        StackDemo.Run();
        RunHelpDeskScenario();
        BreadthFirstSearch.RunDemo();
        LegacyCollectionPreview.Run();
    }

    private static void RunHelpDeskScenario()
    {
        HelpDeskSession session = new HelpDeskSession();
        session.EnqueueTicket(new SupportTicket(1001, "Password reset"));
        session.EnqueueTicket(new SupportTicket(1002, "Billing discrepancy"));
        session.EnqueueTicket(new SupportTicket(1003, "Feature request"));

        Console.WriteLine("--- Section 4: Help desk — Queue + Stack ---");
        Console.WriteLine($"Tickets waiting (Queue Count): {session.PendingTicketCount}");

        if (session.TryPeekNextTicket(out SupportTicket peeked))
        {
            Console.WriteLine($"Next up (TryPeek): #{peeked.Id} — {peeked.Subject}");
        }

        while (session.TryResolveNextTicket(out SupportTicket ticket))
        {
            Console.WriteLine($"Resolved #{ticket.Id}: {ticket.Subject} (FIFO Dequeue)");

            if (ticket.Id != 1001)
            {
                continue;
            }

            session.BeginResponse($"Hi, regarding \"{ticket.Subject}\": ");
            Console.WriteLine($"  Draft start: \"{session.CurrentDraft}\"");

            session.ApplyEdit(d => d.Append(", we sent a reset link"), "add link");
            session.ApplyEdit(d => d.Append(" within 24 hours"), "add timeline");
            session.ApplyEdit(d => d.Append(" (draft)"), "mark draft");
            Console.WriteLine($"  Current draft: \"{session.CurrentDraft}\"");

            while (session.TryUndo(out string restored))
            {
                Console.WriteLine($"  Undo (TryPop) → \"{restored}\"");
            }

            Console.WriteLine($"  Final draft: \"{session.CurrentDraft}\"");
        }

        Console.WriteLine($"Queue drained. Pending count: {session.PendingTicketCount}");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — Queue<T> AND Stack<T>
 * =========================================================================
 *
 * --- When to use which ---
 *
 *   Need                              | Type
 *   ----------------------------------|----------------------------------
 *   Process in arrival order          | Queue<T>  (FIFO)
 *   Most-recent-first (undo, DFS)     | Stack<T>  (LIFO)
 *   Index access, sort, insert middle | List<T>   (not Queue/Stack)
 *
 * --- Queue<T> ---
 *
 *   Enqueue(item)           add to back
 *   Dequeue()               remove front (throws if empty)
 *   TryDequeue(out item)    safe remove front
 *   Peek() / TryPeek        inspect front
 *   Count, Contains, Clear, ToArray
 *   foreach                 front → back
 *
 * --- Stack<T> ---
 *
 *   Push(item)              add to top
 *   Pop()                   remove top (throws if empty)
 *   TryPop(out item)        safe remove top
 *   Peek() / TryPeek        inspect top
 *   Count
 *   foreach                 top → bottom
 *
 * --- Classic algorithms ---
 *
 *   BFS (shortest path unweighted)    Queue<T> frontier
 *   DFS                                 Stack<T> or recursion (call stack)
 *   Undo / redo                         Stack<T> (often two stacks)
 *
 * --- Exceptions ---
 *
 *   Dequeue/Pop/Peek on empty         InvalidOperationException
 *
 * --- Non-generic legacy (avoid in new code) ---
 *
 *   System.Collections.Queue / Stack — object storage, boxing, casts
 *   Full comparison → 02. ArrayList
 *
 * =========================================================================
 */
