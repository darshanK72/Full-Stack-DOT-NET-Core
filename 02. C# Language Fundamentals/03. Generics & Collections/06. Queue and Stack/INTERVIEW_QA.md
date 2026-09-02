# Queue and Stack — Interview Q&A


## Table of Contents

1. [Q1. What is the difference between Queue\<T\> and Stack\<T\> in terms of ordering model?](#q1-what-is-the-difference-between-queuet-and-stackt-in-terms-of-ordering-model)
2. [Q2. What are the core operations of Queue\<T\> and what happens when the queue is empty?](#q2-what-are-the-core-operations-of-queuet-and-what-happens-when-the-queue-is-empty)
3. [Q3. What are the core operations of Stack\<T\> and what happens when the stack is empty?](#q3-what-are-the-core-operations-of-stackt-and-what-happens-when-the-stack-is-empty)
4. [Q4. How is Queue\<T\> implemented internally, and what is a circular buffer?](#q4-how-is-queuet-implemented-internally-and-what-is-a-circular-buffer)
5. [Q5. What is the difference between Peek and TryPeek on both Queue\<T\> and Stack\<T\>?](#q5-what-is-the-difference-between-peek-and-trypeek-on-both-queuet-and-stackt)
6. [Q6. What is PriorityQueue\<TElement, TPriority\> and how does it differ from Queue\<T\>?](#q6-what-is-priorityqueuetelement-tpriority-and-how-does-it-differ-from-queuet)
7. [Q7. What is the time complexity of the fundamental operations on Queue\<T\> and Stack\<T\>?](#q7-what-is-the-time-complexity-of-the-fundamental-operations-on-queuet-and-stackt)
8. [Q8. How does iteration order differ between Queue\<T\> and Stack\<T\>?](#q8-how-does-iteration-order-differ-between-queuet-and-stackt)
9. [Q9. When should you choose Queue\<T\> vs Stack\<T\> vs List\<T\>?](#q9-when-should-you-choose-queuet-vs-stackt-vs-listt)
10. [Q10. What is the difference between Queue\<T\> and the non-generic System.Collections.Queue?](#q10-what-is-the-difference-between-queuet-and-the-non-generic-systemcollectionsqueue)
11. [Q11. How do you implement breadth-first search (BFS) using Queue\<T\>?](#q11-how-do-you-implement-breadth-first-search-bfs-using-queuet)
12. [Q12. How do you implement iterative depth-first search (DFS) using Stack\<T\>?](#q12-how-do-you-implement-iterative-depth-first-search-dfs-using-stackt)
13. [Q13. What is ConcurrentQueue\<T\> and when should you use it instead of Queue\<T\>?](#q13-what-is-concurrentqueuet-and-when-should-you-use-it-instead-of-queuet)
14. [Q14. What is ConcurrentStack\<T\> and in which scenarios does it appear?](#q14-what-is-concurrentstackt-and-in-which-scenarios-does-it-appear)
15. [Q15. How do you implement an undo/redo system using two Stack\<T\> instances?](#q15-how-do-you-implement-an-undoredo-system-using-two-stackt-instances)
16. [Q16. What is a circular buffer pattern and how does Queue\<T\> relate to it?](#q16-what-is-a-circular-buffer-pattern-and-how-does-queuet-relate-to-it)
17. [Q17. How does Queue\<T\>.ToArray() differ from draining the queue via Dequeue?](#q17-how-does-queuettoarray-differ-from-draining-the-queue-via-dequeue)
18. [Q18. Queue\<T\>.Dequeue and Stack\<T\>.Pop throw on empty collections — how do you prevent unhandled exceptions in production?](#q18-queuetdequeue-and-stacktpop-throw-on-empty-collections-how-do-you-prevent-unhandled-exceptions-in-production)
19. [Q19. Stack\<T\>.foreach iterates top-to-bottom — why does this catch developers off guard?](#q19-stacktforeach-iterates-top-to-bottom-why-does-this-catch-developers-off-guard)
20. [Q20. Queue\<T\> and Stack\<T\> are not thread-safe — what can go wrong silently?](#q20-queuet-and-stackt-are-not-thread-safe-what-can-go-wrong-silently)
21. [Q21. PriorityQueue\<TElement, TPriority\> does not guarantee stable ordering for equal-priority elements — what are the consequences?](#q21-priorityqueuetelement-tpriority-does-not-guarantee-stable-ordering-for-equal-priority-elements-what-are-the-consequences)
22. [Q22. Calling ToArray() on a Stack\<T\> or Queue\<T\> inside a foreach loop invalidates the enumerator — why?](#q22-calling-toarray-on-a-stackt-or-queuet-inside-a-foreach-loop-invalidates-the-enumerator-why)
23. [Q23. A billing service processes customer invoices in strict arrival order using the following worker. Under moderate load the host logs unhandled exceptions and restarts. Review the code, identify all issues, and provide a prioritised fix list.](#q23-a-billing-service-processes-customer-invoices-in-strict-arrival-order-using-the-following-worker-under-moderate-load-the-host-logs-unhandled-exceptions-and-restarts-review-the-code-identify-all-issues-and-provide-a-prioritised-fix-list)
24. [Q24. A code-review task: this ASP.NET Core feature was written by a junior developer to track recent user activity for a "Recently Viewed" sidebar. Identify all issues and give a prioritised fix list.](#q24-a-code-review-task-this-aspnet-core-feature-was-written-by-a-junior-developer-to-track-recent-user-activity-for-a-recently-viewed-sidebar-identify-all-issues-and-give-a-prioritised-fix-list)
25. [Q25. Design a background task scheduler for a CI/CD pipeline that must run high-priority builds (hotfix branches) before normal builds, and within the same priority tier run builds in submission order. Describe your collection strategy and sketch the implementation.](#q25-design-a-background-task-scheduler-for-a-cicd-pipeline-that-must-run-high-priority-builds-hotfix-branches-before-normal-builds-and-within-the-same-priority-tier-run-builds-in-submission-order-describe-your-collection-strategy-and-sketch-the-implementation)
26. [Q26. A code-review task: a navigation history feature for a single-page application is implemented below. The team reports that the Back button sometimes skips pages and Forward never works. Identify every defect and provide a prioritised fix list.](#q26-a-code-review-task-a-navigation-history-feature-for-a-single-page-application-is-implemented-below-the-team-reports-that-the-back-button-sometimes-skips-pages-and-forward-never-works-identify-every-defect-and-provide-a-prioritised-fix-list)
27. [Q27. Design a thread-safe audit event pipeline for an ASP.NET Core application where multiple request handlers enqueue audit events and a single hosted background service batches them to a remote logging endpoint. Walk through your collection and lifetime choices.](#q27-design-a-thread-safe-audit-event-pipeline-for-an-aspnet-core-application-where-multiple-request-handlers-enqueue-audit-events-and-a-single-hosted-background-service-batches-them-to-a-remote-logging-endpoint-walk-through-your-collection-and-lifetime-choices)
28. [Q28. A game engine uses a Stack\<T\> to implement scene loading history for a "go back to previous scene" feature. A code review notices a StackOverflowException in load tests when the game has been running for hours and players have navigated through thousands of menus. Explain the cause and recommend a fix.](#q28-a-game-engine-uses-a-stackt-to-implement-scene-loading-history-for-a-go-back-to-previous-scene-feature-a-code-review-notices-a-stackoverflowexception-in-load-tests-when-the-game-has-been-running-for-hours-and-players-have-navigated-through-thousands-of-menus-explain-the-cause-and-recommend-a-fix)

---
> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/06. Queue and Stack`

---

## Foundation Questions

---

## Q1. What is the difference between Queue\<T\> and Stack\<T\> in terms of ordering model?

**Concepts**
- FIFO (First In, First Out) — Queue\<T\>
- LIFO (Last In, First Out) — Stack\<T\>
- End-restricted access — no random index
- Enqueue/Dequeue vs Push/Pop terminology
- Mental model: waiting line vs plate pile

**Answer**

`Queue<T>` enforces FIFO ordering: the element added earliest is the first to leave. Items enter at the tail via `Enqueue` and leave at the head via `Dequeue`, just like a printer job queue or a support ticket line where the customer who waited longest is served first. `Stack<T>` enforces LIFO ordering: the element added most recently is the first to leave. Both Push and Pop operate at the same end — the top — which models an undo history or the CLR call stack where the innermost frame returns before any outer frame. Neither collection supports access by numeric index; that is intentional — the restricted interface encodes the ordering invariant in the type, preventing code from accidentally removing the wrong element. Choosing the wrong one (Stack for tickets, Queue for undo) is a correctness bug that the compiler will not catch, making the conceptual difference the most important thing to commit to memory.

---

## Q2. What are the core operations of Queue\<T\> and what happens when the queue is empty?

**Concepts**
- `Enqueue` — add to back (tail)
- `Dequeue` — remove and return front (head)
- `Peek` — inspect front without removal
- `InvalidOperationException` on empty queue
- `TryDequeue` / `TryPeek` — exception-free safe variants
- `Count`, `Contains`, `Clear`, `ToArray`

**Answer**

`Enqueue(item)` adds an element to the back of the queue in O(1) amortized time. `Dequeue()` removes and returns the element at the front in O(1), but throws `InvalidOperationException` with the message "Queue empty." when the collection has no items. `Peek()` returns the front element without removing it, throwing the same exception on an empty queue. For production code where an empty queue is a normal operating condition — a background worker polling for new jobs, for instance — the safe variants `TryDequeue(out T result)` and `TryPeek(out T result)` return `false` instead of throwing, keeping control flow clean. `Count` gives the number of items, `Contains` performs a linear O(n) search, `Clear` drains all items, and `ToArray()` returns a snapshot copy in front-to-back order without modifying the queue.

```csharp
var jobs = new Queue<string>();
jobs.Enqueue("job-A");
jobs.Enqueue("job-B");

if (jobs.TryDequeue(out string? next))
    Console.WriteLine(next);           // job-A

string? front = jobs.TryPeek(out string? peeked) ? peeked : null; // job-B
```

---

## Q3. What are the core operations of Stack\<T\> and what happens when the stack is empty?

**Concepts**
- `Push` — add to top
- `Pop` — remove and return top
- `Peek` — inspect top without removal
- `InvalidOperationException` on empty stack
- `TryPop` / `TryPeek` — exception-free safe variants
- LIFO iteration order: top-to-bottom via `foreach`

**Answer**

`Push(item)` adds an element to the top of the stack in O(1) amortized time (Stack\<T\> uses a resizable array internally). `Pop()` removes and returns the top element in O(1), throwing `InvalidOperationException` with the message "Stack empty." on an empty stack. `Peek()` returns the top element without removing it, throwing the same exception when empty. `TryPop(out T result)` and `TryPeek(out T result)` are the safe counterparts that return `false` rather than throwing. `Count` returns the number of elements. One subtlety is that `foreach` over a Stack\<T\> iterates in LIFO order — top to bottom — which is the reverse of insertion order. This surprises developers who expect insertion-order iteration; if you need the original push order, call `ToArray()` and reverse it, or iterate the original array you pushed from.

```csharp
var history = new Stack<string>();
history.Push("v1");
history.Push("v2");
history.Push("v3");

Console.WriteLine(history.Pop());          // v3  — LIFO
foreach (string s in history)
    Console.Write(s + " ");               // v2 v1  — top to bottom
```

---

## Q4. How is Queue\<T\> implemented internally, and what is a circular buffer?

**Concepts**
- Internal circular buffer (ring buffer)
- Head and tail index tracking
- Array resizing when capacity is exceeded
- O(1) amortized Enqueue and Dequeue
- No element shifting on Dequeue — unlike List\<T\>.RemoveAt(0)

**Answer**

`Queue<T>` uses a fixed-size array together with two integer indices — a head and a tail — that wrap around the ends of the array. When you `Enqueue`, the runtime writes to `array[tail]` and advances tail modulo capacity; when you `Dequeue`, it reads from `array[head]` and advances head modulo capacity. This circular (ring buffer) design means neither enqueue nor dequeue copies or shifts any existing element, giving true O(1) per operation. When the array fills up — head catches up to tail — the runtime allocates a new, larger array (doubling strategy similar to `List<T>`), copies existing elements in FIFO order into the new array, and resets head to 0 and tail to Count. This resizing is rare relative to the total number of operations, giving O(1) amortized for both Enqueue and Dequeue. The contrast with `List<T>` is important: `list.RemoveAt(0)` shifts every remaining element one position left — an O(n) operation — making `List<T>` a poor FIFO queue for any non-trivial size.

---

## Q5. What is the difference between Peek and TryPeek on both Queue\<T\> and Stack\<T\>?

**Concepts**
- `Peek` — inspect without removal, throws on empty
- `TryPeek(out T result)` — inspect without removal, returns bool
- Non-destructive read — no state change to the collection
- Same semantic on both Queue\<T\> (front) and Stack\<T\> (top)
- Pattern for conditional inspection before commit

**Answer**

`Peek` on a `Queue<T>` returns the front element; on a `Stack<T>` it returns the top element. In both cases the collection is not modified — the element stays in place. The key difference between `Peek()` and `TryPeek(out T)` is error handling: `Peek()` throws `InvalidOperationException` when the collection is empty, while `TryPeek` returns `false` and assigns `default(T)` to the out parameter, leaving the calling code free to handle the empty case without a try/catch block. A common pattern is to TryPeek before committing to a conditional path — for example, checking whether the next ticket in a queue matches the current agent's specialty before dequeuing it. Because neither Peek variant removes the element, a subsequent Dequeue or Pop is still needed to actually consume it, which means two operations are required for inspect-then-consume flows and the collection could change between them in multi-threaded code.

---

## Q6. What is PriorityQueue\<TElement, TPriority\> and how does it differ from Queue\<T\>?

**Concepts**
- Introduced in .NET 6, available in .NET 10
- Min-heap backed — lowest priority value dequeues first
- `Enqueue(element, priority)` — two-argument enqueue
- `Dequeue()` / `TryDequeue(out TElement, out TPriority)` — removes minimum-priority element
- No stable ordering for equal-priority elements
- Use case: task scheduling, Dijkstra's algorithm, bandwidth throttling

**Answer**

`PriorityQueue<TElement, TPriority>` combines a collection element with a separate priority key. Internally it uses a min-heap, so `Dequeue()` always removes and returns the element whose priority value is numerically smallest (or least according to `IComparer<TPriority>`). This is fundamentally different from `Queue<T>`, which ignores any notion of importance and strictly serves elements in insertion order. You enqueue with `Enqueue(element, priority)` — for example, `pq.Enqueue(ticket, ticket.SeverityLevel)` — and dequeue without specifying a priority. An important caveat is that `PriorityQueue<TElement, TPriority>` gives **no stability guarantee** for equal-priority elements: two items enqueued with the same priority may come out in any order. If FIFO tie-breaking matters, you must embed a sequence number into the priority (e.g., use a `(int severity, int sequence)` tuple as TPriority with a comparer). There is no `Peek` that returns the highest-priority element without removing it in .NET 10 — `TryPeek(out TElement element, out TPriority priority)` covers that case.

```csharp
var pq = new PriorityQueue<string, int>();
pq.Enqueue("Low severity task",  3);
pq.Enqueue("Critical alert",     1);
pq.Enqueue("Medium task",        2);

while (pq.TryDequeue(out string? task, out int pri))
    Console.WriteLine($"[{pri}] {task}");
// [1] Critical alert
// [2] Medium task
// [3] Low severity task
```

---

## Q7. What is the time complexity of the fundamental operations on Queue\<T\> and Stack\<T\>?

**Concepts**
- O(1) amortized for Enqueue, Dequeue, Push, Pop
- O(n) for Contains — linear scan
- O(n) for ToArray — copy all elements
- O(n) for Clear — must null out references for GC
- Array resizing — O(n) worst case, amortized O(1)

**Answer**

`Enqueue`, `Dequeue`, `Push`, and `Pop` are all O(1) amortized. The "amortized" qualifier exists because the underlying array occasionally doubles its capacity, which requires an O(n) copy of existing elements, but that cost is spread across the many O(1) operations that preceded it. `Peek` and `TryPeek` are unconditionally O(1) — they only read an index. `Contains` is O(n) because neither collection maintains a hash table; it scans all elements. `ToArray()` is O(n) as it copies every element into a new array. `Clear()` is O(n) for reference types because the runtime must null out the array slots to allow garbage collection of the referenced objects; for value types it may be faster. These complexities match `List<T>` for most operations, with the critical difference that `Dequeue` on `Queue<T>` is O(1) while `List<T>.RemoveAt(0)` is O(n) due to element shifting.

---

## Q8. How does iteration order differ between Queue\<T\> and Stack\<T\>?

**Concepts**
- `Queue<T>` foreach — front to back (oldest to newest, FIFO order)
- `Stack<T>` foreach — top to bottom (newest to oldest, LIFO order)
- Both implement `IEnumerable<T>` — compatible with LINQ
- Iteration is non-destructive — elements remain in the collection
- Snapshot via `ToArray()` for index-based access

**Answer**

`Queue<T>` iterates in FIFO order: `foreach` visits the front (head) element first and the back (tail) element last, which is the same sequence as successive `Dequeue` calls would produce. `Stack<T>` iterates in LIFO order: `foreach` visits the top element first and the bottom element last, which is the same sequence as successive `Pop` calls. Both are non-destructive — the collection is unchanged after iteration. One practical consequence: if you serialize the contents of a Stack\<T\> with `foreach` and later push those items back in the iterated order, you get the original top element at the bottom — the stack is logically reversed. To restore the original order after iterating, you must reverse the sequence before re-pushing. Both types implement `IEnumerable<T>`, so LINQ operators like `Where`, `Select`, and `FirstOrDefault` work directly and respect the respective iteration order.

---

## Q9. When should you choose Queue\<T\> vs Stack\<T\> vs List\<T\>?

**Concepts**
- Queue\<T\> — arrival-order fairness, FIFO workflows
- Stack\<T\> — most-recent-first, undo, DFS, expression evaluation
- List\<T\> — index access, arbitrary insert/remove, sorting
- Semantic intent encoded in the type
- Preventing misuse via restricted API surface

**Answer**

The collection type itself communicates the ordering contract and prevents callers from violating it. Use `Queue<T>` whenever items must be processed in the order they arrive: print spoolers, support ticket queues, event buses, BFS graph traversal, or any workflow where first-come-first-served fairness is a business requirement. Use `Stack<T>` whenever the most recently added item is the one you need next: undo/redo history, backtracking search (DFS iterative), expression evaluation with operators and operands, parsing nested structures like HTML tags, and simulating the CLR call stack in recursive-to-iterative refactors. Use `List<T>` when you need to access, insert, or remove at an arbitrary position by index, or when you need sorting — operations that Queue\<T\> and Stack\<T\> deliberately omit. A common mistake is reaching for `List<T>` with `Add` and `RemoveAt(0)` as a queue, which produces O(n) removals; or using `List<T>` with `Add` and `RemoveAt(Count-1)` as a stack, which is functionally equivalent to Stack\<T\> but more verbose and easier to misuse.

---

## Q10. What is the difference between Queue\<T\> and the non-generic System.Collections.Queue?

**Concepts**
- `System.Collections.Queue` — stores `object`, requires cast
- Boxing for value types — heap allocation per enqueue of a value type
- `InvalidCastException` risk at runtime on bad cast
- `Queue<T>` — compile-time type safety, no boxing for value types
- Prefer `Queue<T>` in all new code

**Answer**

The non-generic `System.Collections.Queue` (namespace `System.Collections`) was introduced before C# generics and stores every element as `object`. Enqueueing a value type such as `int` or `struct` boxes it into a heap-allocated object wrapper, adding GC pressure; dequeuing requires an explicit cast back to the original type, and if the cast is wrong the runtime throws `InvalidCastException` rather than the compiler reporting a type error. The generic `Queue<T>` eliminates both problems: value types are stored directly in the typed array (no boxing), and the compiler enforces that only `T` elements enter and leave the queue. The APIs are nearly identical in shape — `Enqueue`, `Dequeue`, `Peek`, `Count`, `Contains`, `Clear` — but the generic version also adds `TryDequeue` and `TryPeek` safe variants not present on the legacy type. There is no migration cost beyond a type parameter, so new code should always use `Queue<T>`. The same argument applies to `System.Collections.Stack` vs `Stack<T>`.

---

## Q11. How do you implement breadth-first search (BFS) using Queue\<T\>?

**Concepts**
- Queue frontier — expands nodes layer by layer
- Visited set — prevents revisiting
- Shortest path in unweighted graph — guaranteed by FIFO ordering
- `TryDequeue` — safe drain loop termination
- Neighbor expansion — enqueue unvisited adjacent nodes

**Answer**

BFS uses a `Queue<T>` frontier because FIFO ordering guarantees that nodes closer to the start are explored before nodes further away. The algorithm starts by enqueueing the start node and marking it visited, then enters a loop: while the queue is not empty, dequeue the current node, process it, and enqueue each unvisited neighbor (marking each visited before enqueuing to prevent duplicate entries). Because each node is added exactly once and removed exactly once, the overall complexity is O(V + E) for a graph with V vertices and E edges. The first time the goal node is dequeued is guaranteed to be reached via the shortest path in an unweighted graph, which makes `Queue<T>` the canonical choice for BFS. Using `TryDequeue` in the loop condition keeps the code clean without a separate `Count > 0` check.

```csharp
var visited = new HashSet<int>();
var queue = new Queue<int>();
queue.Enqueue(startNode);
visited.Add(startNode);

while (queue.TryDequeue(out int current))
{
    if (current == goal) break;
    foreach (int neighbor in graph[current])
    {
        if (visited.Add(neighbor))        // Add returns false if already present
            queue.Enqueue(neighbor);
    }
}
```

---

## Q12. How do you implement iterative depth-first search (DFS) using Stack\<T\>?

**Concepts**
- Stack frontier — explores deepest path first
- LIFO ensures most recently discovered node is expanded next
- Iterative DFS avoids StackOverflowException on deep graphs
- Visited tracking — prevents cycles
- Back-edge detection — cycle check in directed graphs

**Answer**

Iterative DFS replaces the implicit CLR call stack used by recursive DFS with an explicit `Stack<T>`, which stores elements on the heap and avoids `StackOverflowException` on graphs deeper than the thread's stack limit (typically a few thousand frames). The algorithm mirrors the recursive form: push the start node, then loop — pop the top, skip it if already visited, otherwise mark it visited, process it, and push all its unvisited neighbors. Because Push and Pop both act on the top, the last-pushed neighbor is explored next, replicating LIFO depth-first order. This is the standard production pattern for graph search on large inputs. It is important to understand that switching from `Queue<T>` to `Stack<T>` does not merely change performance — it changes the traversal order from breadth-first (shortest-path guarantees) to depth-first (no shortest-path guarantee), which is a correctness consideration, not just an optimization.

```csharp
var visited = new HashSet<int>();
var stack = new Stack<int>();
stack.Push(startNode);

while (stack.TryPop(out int current))
{
    if (!visited.Add(current)) continue;  // already seen
    Process(current);
    foreach (int neighbor in graph[current])
        if (!visited.Contains(neighbor))
            stack.Push(neighbor);
}
```

---

## Q13. What is ConcurrentQueue\<T\> and when should you use it instead of Queue\<T\>?

**Concepts**
- `System.Collections.Concurrent.ConcurrentQueue<T>`
- Thread-safe without external locking
- Lock-free algorithm — uses `Interlocked` CAS operations
- `Enqueue`, `TryDequeue`, `TryPeek` — same logical API as Queue\<T\>
- Use case: producer-consumer, multi-threaded event pipelines
- `Channel<T>` as modern async alternative

**Answer**

`Queue<T>` is not thread-safe: concurrent `Enqueue` and `Dequeue` calls from multiple threads can corrupt the internal circular buffer, lose elements, or throw exceptions without any error message that reveals the root cause. `ConcurrentQueue<T>` (namespace `System.Collections.Concurrent`) solves this with a lock-free algorithm built on compare-and-swap (`Interlocked` operations), allowing multiple producers and multiple consumers to operate simultaneously without a coarse lock. The API is almost identical — `Enqueue`, `TryDequeue`, `TryPeek`, `Count`, `IsEmpty` — except there is no non-try `Dequeue` because the expected usage is always to check for emptiness. In .NET 10, `Channel<T>` (from `System.Threading.Channels`) is often the better choice for new producer-consumer pipelines because it supports async reading (`ReadAllAsync`), bounded capacity with back-pressure, and clean DI registration, making it more composable with `async/await` code than `ConcurrentQueue<T>`'s polling model.

---

## Q14. What is ConcurrentStack\<T\> and in which scenarios does it appear?

**Concepts**
- `System.Collections.Concurrent.ConcurrentStack<T>`
- Thread-safe LIFO — multiple producers and consumers
- `Push`, `PushRange`, `TryPop`, `TryPopRange` — batch operations
- Lock-free via CAS operations on the head pointer
- Use case: work-stealing queues, thread-local undo in concurrent editors

**Answer**

`ConcurrentStack<T>` is the thread-safe counterpart of `Stack<T>`. Like `ConcurrentQueue<T>`, it uses compare-and-swap rather than a lock, so multiple threads can push and pop simultaneously without contention on a mutex. A notable addition over `Stack<T>` is the batch API: `PushRange(T[] items)` and `TryPopRange(T[] output)` let you move multiple items in a single atomic-style operation, which reduces overhead in high-throughput scenarios. `ConcurrentStack<T>` appears less frequently than `ConcurrentQueue<T>` in practice; common scenarios include work-stealing thread-pool implementations (where each thread has a local LIFO stack and can steal from the bottom of other stacks), concurrent expression evaluators, and undo history in editors that allow simultaneous document edits from multiple users. For most ASP.NET Core services where LIFO ordering matters across threads, you would typically reach for `Channel<T>` with custom ordering logic before considering `ConcurrentStack<T>`.

---

## Q15. How do you implement an undo/redo system using two Stack\<T\> instances?

**Concepts**
- Undo stack — holds states before each edit
- Redo stack — holds states before each undo
- Push before apply — snapshot taken prior to mutation
- Redo stack cleared on new edit — history branches are discarded
- Bounded undo depth — optional capacity limit

**Answer**

The classic two-stack undo/redo pattern keeps two `Stack<T>` instances: an undo stack and a redo stack, where T is either the full document state (simple) or a reversible command object (more memory-efficient). Before applying any edit, push the current state onto the undo stack. When the user triggers undo, pop from the undo stack (restoring the previous state), and push the state that was just undone onto the redo stack so it can be reapplied. When the user triggers redo, pop from the redo stack and push the current state back onto the undo stack. Critically, whenever a new edit is applied (not undo or redo), the redo stack must be cleared — branching history is discarded in most editors. To implement a bounded undo history, a `LinkedList<T>` or a fixed-size circular buffer may replace the stack when you need to drop the oldest item rather than the newest, but `Stack<T>` is correct for unbounded or depth-limited histories where the oldest items are also the ones to discard first.

```csharp
Stack<string> _undo = new();
Stack<string> _redo = new();

void ApplyEdit(string newState)
{
    _undo.Push(currentState);
    _redo.Clear();                // discard forward history
    currentState = newState;
}

bool TryUndo()
{
    if (!_undo.TryPop(out string? prev)) return false;
    _redo.Push(currentState);
    currentState = prev;
    return true;
}
```

---

## Q16. What is a circular buffer pattern and how does Queue\<T\> relate to it?

**Concepts**
- Fixed-size ring buffer — head and tail wrap modulo capacity
- Overwrites oldest data when full (bounded mode)
- O(1) read and write at all times — no shifting
- Queue\<T\> uses unbounded circular buffer internally
- Explicit bounded circular buffer — data streaming, sensor logs

**Answer**

A circular buffer (ring buffer) allocates a fixed-size array and maintains head and tail pointers that advance modulo the array length. Reading advances head; writing advances tail. When the buffer is full, the oldest element is overwritten by the newest, which makes it ideal for scenarios where you want only the last N items — audio sample buffers, network packet queues, rolling log windows. `Queue<T>` uses an internal circular buffer but in an unbounded variant — when it fills, the array grows rather than overwriting old data. To implement a true fixed-capacity overwrite ring buffer in C#, you manage the array and index arithmetic yourself or use a third-party `MemoryPool<T>` backed approach. The relevant interview point is that understanding the circular buffer explains why `Queue<T>`.Dequeue() is O(1) (head index advances; no elements shift), unlike `List<T>.RemoveAt(0)` which shifts all remaining elements forward.

---

## Q17. How does Queue\<T\>.ToArray() differ from draining the queue via Dequeue?

**Concepts**
- `ToArray()` — non-destructive snapshot in FIFO order
- Draining with `Dequeue` — destructive, removes each element
- Use case for ToArray — logging, serialization, debugging
- Count unchanged after ToArray — queue intact
- Foreach alternative — also non-destructive, same FIFO order

**Answer**

`Queue<T>.ToArray()` returns a new array containing all elements in FIFO order — front to back — without removing anything from the queue. After the call, `Count` is unchanged and subsequent `Dequeue` calls still process elements in the original order. It is semantically equivalent to `foreach` over the queue but gives you an indexed array, which is useful for logging the current queue state, serializing it, or passing a snapshot to code that expects an array without giving it access to the live queue. Draining via `while (queue.TryDequeue(out var item))`, by contrast, removes every element and leaves `Count` at zero. A subtle consequence of `ToArray` is that changes to the returned array do not affect the queue — it is an independent copy, not a view. If elements are themselves mutable reference types, the array and the queue share the same object references, so mutating an element's fields will be visible through both.

---

## Gotchas

---

## Q18. Queue\<T\>.Dequeue and Stack\<T\>.Pop throw on empty collections — how do you prevent unhandled exceptions in production?

**Concepts**
- `InvalidOperationException` — "Queue empty." / "Stack empty."
- `TryDequeue` / `TryPop` — return `false` instead of throwing
- Exception-driven control flow — anti-pattern for expected empty state
- `Count > 0` check before Dequeue — race condition in concurrent code
- Prefer TryX variants as the default in background workers and polling loops

**Answer**

Calling `Dequeue()` or `Pop()` on an empty collection throws `InvalidOperationException`. In a background service that polls a queue for work, the queue being empty is not an error — it is the normal resting state — so using the throwing variants and wrapping them in `try/catch` is an anti-pattern that uses exceptions for control flow and imposes unnecessary allocation overhead per iteration. The correct approach is `TryDequeue(out T result)` and `TryPop(out T result)`, which return `false` when empty, allowing the caller to branch without exception handling. A common but subtly broken alternative is `if (queue.Count > 0) queue.Dequeue()`: in single-threaded code this works, but in multi-threaded code another thread could dequeue the last item between the `Count` check and the `Dequeue` call, causing the exception to appear anyway. Using `TryDequeue` atomically addresses this in `ConcurrentQueue<T>`, and matches the intended idiom for `Queue<T>` in single-threaded loops.

---

## Q19. Stack\<T\>.foreach iterates top-to-bottom — why does this catch developers off guard?

**Concepts**
- LIFO iteration — most recently pushed element visited first
- Contrast with Queue\<T\> foreach — front (oldest) to back (newest)
- `ToArray()` returns elements top-to-bottom as well
- Re-push from iteration produces reversed stack
- Workaround: `stack.ToArray().Reverse()` for bottom-to-top order

**Answer**

Developers expect `foreach` over a collection to iterate in insertion order by default, because `List<T>`, arrays, and `Queue<T>` all do. `Stack<T>` breaks that expectation: foreach visits the most recently pushed element first and the first pushed element last. This trips up code that iterates a Stack\<T\> to serialize its contents and then attempts to restore the stack by pushing elements in the iterated order — the reconstructed stack is inverted. The same trap exists with `ToArray()`, which also returns elements in top-to-bottom (LIFO) order. To iterate in push order (bottom to top), call `stack.ToArray().Reverse()` or maintain a parallel array and iterate that. The behaviour is intentional and documented, but its deviation from the List/Queue precedent makes it one of the more reliable gotcha questions in C# collection interviews.

---

## Q20. Queue\<T\> and Stack\<T\> are not thread-safe — what can go wrong silently?

**Concepts**
- No internal synchronization — designed for single-threaded use
- Circular buffer corruption under concurrent Enqueue/Dequeue
- Silent data loss — no exception guaranteed
- `InvalidOperationException` — can appear but is not reliable
- `ConcurrentQueue<T>` / `ConcurrentStack<T>` as thread-safe alternatives

**Answer**

`Queue<T>` and `Stack<T>` perform no internal locking. When two threads concurrently call `Enqueue` and `Dequeue` (or `Push` and `Pop`), both read and write the internal array and the head/tail index fields without coordination. The result is a data race: two threads may see the same index, write to the same slot, or advance the head/tail past valid bounds. The failure modes are unpredictable — the collection may silently drop elements, return stale values, or eventually throw an `IndexOutOfRangeException` or `InvalidOperationException`, but none of those outcomes is guaranteed. In a web application where the queue is registered as a `Singleton`, every incoming request thread can hit this concurrency bug, and because the corruption may produce wrong results rather than an exception, it can go undetected in testing and only surface as mysterious data loss under production load. The fix is to use `ConcurrentQueue<T>`, `ConcurrentStack<T>`, or a `Channel<T>`, depending on whether synchronous or asynchronous producer-consumer semantics are needed.

---

## Q21. PriorityQueue\<TElement, TPriority\> does not guarantee stable ordering for equal-priority elements — what are the consequences?

**Concepts**
- Min-heap internal structure — no stable position for ties
- Equal-priority elements may dequeue in any order
- FIFO tie-breaking requires composite priority key
- Sequence number — second component of priority tuple
- Contrast with `Queue<T>` — strictly stable FIFO

**Answer**

`PriorityQueue<TElement, TPriority>` uses a binary min-heap that does not preserve insertion order among elements that share the same priority value. If two support tickets both have severity 2, which one dequeues first depends on the internal heap structure, which can change with resizing or dequeue operations. In a real SLA system, two tickets of equal severity should ideally be resolved in arrival order (FIFO within tier), but the priority queue does not enforce this. The standard fix is to use a composite priority — a tuple of `(int severity, int sequenceNumber)` where `sequenceNumber` is a monotonically increasing counter assigned at enqueue time — and provide a comparer that first compares severity, then sequence number on a tie. This gives stable, deterministic ordering within each tier. Alternatively, maintaining one `Queue<T>` per priority tier and dequeuing from the highest non-empty tier achieves the same result with simpler code and explicit FIFO guarantees within each tier.

```csharp
int _seq = 0;
var pq = new PriorityQueue<string, (int Severity, int Seq)>(
    Comparer<(int Severity, int Seq)>.Create((a, b) =>
        a.Severity != b.Severity ? a.Severity.CompareTo(b.Severity)
                                 : a.Seq.CompareTo(b.Seq)));

pq.Enqueue("Ticket-A", (2, _seq++));
pq.Enqueue("Ticket-B", (2, _seq++));
// Ticket-A always dequeues before Ticket-B — stable within severity tier
```

---

## Q22. Calling ToArray() on a Stack\<T\> or Queue\<T\> inside a foreach loop invalidates the enumerator — why?

**Concepts**
- `InvalidOperationException` — "Collection was modified after the enumerator was created."
- Enumerator version stamp — incremented on any mutation
- Modifying collection during foreach — forbidden pattern
- ToArray snapshot — safe workaround to iterate and drain simultaneously
- Concurrent enumeration vs concurrent mutation

**Answer**

Both `Queue<T>` and `Stack<T>` use an internal version counter that is incremented on every mutation (`Enqueue`, `Dequeue`, `Push`, `Pop`, `Clear`). When you start a `foreach`, the enumerator captures the current version. On each `MoveNext()`, it checks that the version has not changed; if it has, it throws `InvalidOperationException: "Collection was modified after the enumerator was created."` This means you cannot call `Dequeue` inside a `foreach` over the same queue, nor `Pop` inside a `foreach` over the same stack. The canonical workaround for iterate-and-drain is to either use a `while (queue.TryDequeue(out var item))` loop instead of `foreach`, or take a `ToArray()` snapshot before the loop and iterate the array while draining the original collection separately. A less obvious trigger is LINQ: chained LINQ operators over a `Queue<T>` are lazy — if the query is enumerated after a `Dequeue` has already run, the version mismatch is still detected and throws.

---

## Real-World Scenarios

---

## Q23. A billing service processes customer invoices in strict arrival order using the following worker. Under moderate load the host logs unhandled exceptions and restarts. Review the code, identify all issues, and provide a prioritised fix list.

```csharp
public sealed class InvoiceProcessor
{
    private readonly Queue<Invoice> _queue = new();
    private bool _running;

    public void Submit(Invoice inv) => _queue.Enqueue(inv);

    public void Start()
    {
        _running = true;
        while (_running)
        {
            Invoice inv = _queue.Dequeue();
            Process(inv);
        }
    }

    public void Stop() => _running = false;
}
```

**Concepts**
- `Dequeue` throws on empty queue — expected idle state
- Busy-wait loop — 100% CPU during idle periods
- Non-atomic `_running` flag — visibility across threads
- Missing `CancellationToken` — no cooperative shutdown
- `TryDequeue` as the safe drain pattern

**Answer**

The worker crashes during idle periods because `Dequeue()` throws `InvalidOperationException` whenever the queue is empty, which happens whenever the upstream billing events pause between batches. The tight `while (_running)` loop also spins at 100% CPU even when there is nothing to process. The `_running` flag is a plain `bool` written from one thread and read from another with no memory barrier, making its visibility across threads undefined under the C# memory model. There is no `CancellationToken`, so graceful shutdown under ASP.NET Core's hosted-service lifecycle is not supported.

| Category | Problem | Impact |
|---|---|---|
| Runtime crash | `Dequeue()` on empty queue | `InvalidOperationException` → host restart, lost in-flight work |
| CPU waste | Busy-wait with no idle back-off | 100% CPU core consumed during every idle interval |
| Thread safety | `_running` bool read/written across threads without `volatile` or lock | Stale cached value — `Stop()` may not be observed promptly |
| Lifecycle | No `CancellationToken` integration | Cannot participate in ASP.NET Core graceful shutdown |

**Fix priority:**

1. Replace `_queue.Dequeue()` with `TryDequeue(out Invoice? inv)` and skip processing when it returns `false` — eliminates the crash immediately.
2. Add `await Task.Delay(pollInterval, ct)` (or use a `Channel<Invoice>` with `ReadAllAsync`) when `TryDequeue` returns `false` — eliminates the CPU spin.
3. Accept `CancellationToken ct` in `Start` (rename to `StartAsync`), use `ct.IsCancellationRequested` as the loop condition, and remove the `_running` field entirely.
4. If multiple threads call `Submit`, replace `Queue<Invoice>` with `Channel<Invoice>` for thread-safe producer-consumer with bounded back-pressure.

---

## Q24. A code-review task: this ASP.NET Core feature was written by a junior developer to track recent user activity for a "Recently Viewed" sidebar. Identify all issues and give a prioritised fix list.

```csharp
public sealed class RecentActivityService
{
    private readonly Stack<string> _activity = new();
    private const int MaxItems = 10;

    public void Record(string pageUrl)
    {
        _activity.Push(pageUrl);
        if (_activity.Count > MaxItems)
            _activity.Pop();    // remove newest to cap at 10
    }

    public IReadOnlyList<string> GetRecent()
        => _activity.ToArray().ToList().AsReadOnly();
}
```

**Concepts**
- `Pop` removes top (newest) — wrong element discarded
- Intended semantics — remove oldest, keep newest ten
- Thread safety — Singleton stack in web service
- `ToArray()` iterates top-to-bottom — display order may be inverted
- Bounded LIFO — `LinkedList<T>` or `Queue<T>` with dequeue-oldest pattern

**Answer**

`Pop` removes the element most recently pushed (the top), which is the item just recorded in `Record`. Capping the stack by popping the newest item means the history never grows beyond one entry — every push is immediately cancelled by the pop. The intent is to discard the oldest entry (the bottom of the stack), but `Stack<T>` has no `RemoveFromBottom` operation. Additionally the service is presumably registered as a Singleton, but `Stack<T>` is not thread-safe; concurrent `Record` calls from different request threads corrupt the internal array.

| Category | Problem | Impact |
|---|---|---|
| Logic error | `Pop()` removes newest item, not oldest | History capped at 1 item — sidebar always shows only the current page |
| Wrong collection | `Stack<T>` has no O(1) remove-oldest | Even if Pop is fixed, removing the bottom requires draining and rebuilding |
| Thread safety | `Stack<T>` in Singleton without locking | Concurrent request threads corrupt internal state |
| Display order | `ToArray()` returns newest-first | Sidebar shows reversed chronology if caller expects oldest-first |

**Fix priority:**

1. Replace `Stack<string>` with a `Queue<string>`: `Enqueue` the new URL, and when `Count > MaxItems`, call `Dequeue` to remove the oldest — correct O(1) remove-oldest semantics.
2. Protect the queue with a `lock` (or switch to `ConcurrentQueue<T>`) since multiple request threads can call `Record` simultaneously in an ASP.NET Core Singleton.
3. Document the iteration order in `GetRecent`: `Queue<T>.ToArray()` returns front-to-back (oldest first); reverse if the sidebar expects newest-first.
4. Consider a `Channel<string>` or a simple bounded `LinkedList<string>` if you need both thread safety and easy oldest-entry removal without a full concurrent queue.

---

## Q25. Design a background task scheduler for a CI/CD pipeline that must run high-priority builds (hotfix branches) before normal builds, and within the same priority tier run builds in submission order. Describe your collection strategy and sketch the implementation.

**Concepts**
- `PriorityQueue<TElement, TPriority>` for tier ordering
- Composite priority — (tier, sequence) for stable FIFO within tier
- Monotonic sequence counter — ensures deterministic tie-breaking
- `Channel<T>` or `ConcurrentQueue` for thread-safe submission
- `IHostedService` worker consuming the priority queue

**Answer**

A single `PriorityQueue<BuildJob, (int Tier, long Seq)>` gives both priority ordering and stable FIFO within a tier. Define tier values as constants (0 = hotfix, 1 = normal, 2 = scheduled), assign each submitted job a monotonically increasing sequence number from an `Interlocked.Increment` counter, and enqueue with that composite key. The comparer first sorts by tier ascending (lowest number = highest urgency), then by sequence ascending (earliest submission first within the tier), giving full determinism. Because `PriorityQueue<TElement, TPriority>` is not thread-safe, submissions from the web layer should go through a `Channel<BuildJob>` (bounded, async writes) whose background reader transfers items into the priority queue on the worker's thread — keeping the priority queue single-threaded while the channel handles the producer-consumer boundary. The `IHostedService` worker calls `TryDequeue` on the priority queue to pick the next job, or awaits the channel reader when the priority queue is empty.

```csharp
var pq = new PriorityQueue<BuildJob, (int Tier, long Seq)>();
long _seq = 0;

void Submit(BuildJob job)
{
    long seq = Interlocked.Increment(ref _seq);
    pq.Enqueue(job, (job.Tier, seq));   // single-threaded: fed by channel reader
}

while (!ct.IsCancellationRequested)
{
    if (pq.TryDequeue(out BuildJob? next, out _))
        await RunBuildAsync(next, ct);
    else
        await channelReader.WaitToReadAsync(ct);  // block until new submission
}
```

---

## Q26. A code-review task: a navigation history feature for a single-page application is implemented below. The team reports that the Back button sometimes skips pages and Forward never works. Identify every defect and provide a prioritised fix list.

```csharp
public sealed class NavHistory
{
    private readonly Stack<string> _back = new();
    private readonly Stack<string> _forward = new();

    public void Navigate(string url)
    {
        _back.Push(url);               // push destination, not current
        // forward not cleared
    }

    public string? GoBack()
    {
        if (_back.TryPop(out string? prev))
            return prev;
        return null;
    }

    public string? GoForward()
    {
        if (_forward.TryPop(out string? next))
            return next;
        return null;
    }

    public string? Current => _back.TryPeek(out string? c) ? c : null;
}
```

**Concepts**
- Current URL tracking — separate field or Peek on back stack
- Navigate must push current before moving to destination
- Forward stack cleared on new navigation
- GoBack pushes popped URL onto forward stack
- GoForward pushes popped URL back onto back stack

**Answer**

`Navigate` pushes the destination URL but never transfers the current page to the back stack first — the current page is lost on every navigation, so pressing Back returns the page visited two steps ago rather than one. `Navigate` also never clears the forward stack, so stale forward history accumulates across sessions. `GoBack` pops the destination without pushing it onto the forward stack, making `GoForward` permanently broken. `GoForward` pops from the forward stack but never pushes the current URL back onto the back stack, so the Back button stops working correctly after a forward navigation.

| Category | Problem | Impact |
|---|---|---|
| Logic — Navigate | Pushes destination, not current page | Back skips pages — current URL overwritten without recording it |
| Logic — Navigate | Does not clear forward stack | Stale forward history after new navigation |
| Logic — GoBack | Does not push returned URL onto `_forward` | Forward button never has anything to show |
| Logic — GoForward | Does not push popped URL onto `_back` | Back button breaks after any forward navigation |

**Fix priority:**

1. In `Navigate`: push the current URL (before the navigation) onto `_back`, set the new URL as current (a separate `_current` field), and `_forward.Clear()`.
2. In `GoBack`: push `_current` onto `_forward`, then pop from `_back` and assign it to `_current`; return the new current.
3. In `GoForward`: push `_current` onto `_back`, then pop from `_forward` and assign it to `_current`; return the new current.
4. Expose `Current` as the dedicated `_current` field rather than a Peek on the back stack — the back stack should hold only the history of pages to return to, not the live page.

---

## Q27. Design a thread-safe audit event pipeline for an ASP.NET Core application where multiple request handlers enqueue audit events and a single hosted background service batches them to a remote logging endpoint. Walk through your collection and lifetime choices.

**Concepts**
- `Channel<T>` — async producer-consumer, bounded back-pressure
- `BoundedChannelOptions.FullMode` — `Wait` vs `DropOldest`
- `ChannelWriter<T>` injected into handlers — producer interface
- `ChannelReader<T>` in `IHostedService` — consumer interface
- `ReadAllAsync(ct)` — async enumeration until channel closed

**Answer**

The producer-consumer split maps cleanly onto `Channel<AuditEvent>`. Register a `Channel<AuditEvent>` as a Singleton with bounded capacity — say 50,000 — and `FullMode = BoundedChannelFullMode.Wait` so that if the background uploader falls behind, request handlers back-pressure naturally rather than dropping events silently. Inject `ChannelWriter<AuditEvent>` into each request handler or domain service for writing; the writer is a lightweight interface with `TryWrite` (non-blocking, returns false when full) and `WriteAsync` (awaits space). The `IHostedService` receives `ChannelReader<AuditEvent>` and calls `ReadAllAsync(ct)` in `ExecuteAsync`, which yields events as they arrive and suspends the background thread when the channel is empty — no polling loop, no CPU spin. Batch them with a timer or size threshold before uploading. On shutdown, `StopAsync` calls `channel.Writer.Complete()`, which causes `ReadAllAsync` to drain remaining events and exit cleanly. This design ensures no external lock, no `ConcurrentQueue` polling, and no lost events between the complete signal and the final drain.

```csharp
// DI registration
builder.Services.AddSingleton(
    Channel.CreateBounded<AuditEvent>(new BoundedChannelOptions(50_000)
    { FullMode = BoundedChannelFullMode.Wait, SingleReader = true }));
builder.Services.AddSingleton(svc => svc.GetRequiredService<Channel<AuditEvent>>().Writer);
builder.Services.AddSingleton(svc => svc.GetRequiredService<Channel<AuditEvent>>().Reader);
builder.Services.AddHostedService<AuditBatchUploader>();

// Consumer (IHostedService.ExecuteAsync)
await foreach (AuditEvent evt in _reader.ReadAllAsync(ct))
    buffer.Add(evt);
```

---

## Q28. A game engine uses a Stack\<T\> to implement scene loading history for a "go back to previous scene" feature. A code review notices a StackOverflowException in load tests when the game has been running for hours and players have navigated through thousands of menus. Explain the cause and recommend a fix.

**Concepts**
- Unbounded `Stack<T>` — heap memory, not CLR call stack
- `StackOverflowException` from this code — not the expected cause
- Memory exhaustion — millions of string references on heap
- Bounded history — only N most recent scenes needed
- `Queue<T>` with `Dequeue`-oldest vs fixed-size array ring buffer

**Answer**

This is a deliberate misdirection: `Stack<T>` stores its elements on the managed heap, not on the CLR call stack, so it does not cause `StackOverflowException` no matter how many elements it holds. A `StackOverflowException` from this code would come from a recursive method called during scene loading — check whether the scene loading pipeline calls itself recursively without a proper base case. The memory issue after hours of play is real and separate: pushing thousands of scene names without ever popping or trimming means the stack grows without bound, eventually consuming enough memory to trigger an `OutOfMemoryException` or GC pressure that causes frame-rate hitches, but not a `StackOverflowException`. The fix for the memory concern is to cap history depth: keep only the last N scenes needed for the Back button. A `Queue<string>` with `Dequeue`-oldest when `Count > maxDepth` implements a sliding FIFO window. If only the previous scene is needed (single-level back), a single `string? _previousScene` field is all that is required.

```csharp
private readonly Queue<string> _history = new(capacity: 20);
private const int MaxHistory = 20;

public void PushScene(string sceneName)
{
    if (_history.Count >= MaxHistory)
        _history.Dequeue();        // drop oldest — O(1)
    _history.Enqueue(sceneName);
}

public string? PopScene()
    => _history.Count > 0 ? _history.Dequeue() : null;
```
