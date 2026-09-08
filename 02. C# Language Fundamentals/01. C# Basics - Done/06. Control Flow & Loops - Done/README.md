# Control Flow & Loops — Interview Q&A

---


## Table of Contents

1. [Q1. How does the if / else if / else chain evaluate its conditions, and what guarantee does it provide about which branch runs?](#q1-how-does-the-if-else-if-else-chain-evaluate-its-conditions-and-what-guarantee-does-it-provide-about-which-branch-runs)
2. [Q2. What are the rules for break in a classic switch statement, and how do shared case labels differ from C-style fall-through?](#q2-what-are-the-rules-for-break-in-a-classic-switch-statement-and-how-do-shared-case-labels-differ-from-c-style-fall-through)
3. [Q3. How does a switch expression differ from a switch statement, and what does the discard pattern _ represent?](#q3-how-does-a-switch-expression-differ-from-a-switch-statement-and-what-does-the-discard-pattern-represent)
4. [Q4. What is a when guard in a switch arm, and how does it interact with the two-phase pattern-matching evaluation?](#q4-what-is-a-when-guard-in-a-switch-arm-and-how-does-it-interact-with-the-two-phase-pattern-matching-evaluation)
5. [Q5. How is the for loop structured, and what is the off-by-one pitfall when choosing between < and <=?](#q5-how-is-the-for-loop-structured-and-what-is-the-off-by-one-pitfall-when-choosing-between-and)
6. [Q6. How does the while loop differ from the for loop, and when is while the correct choice?](#q6-how-does-the-while-loop-differ-from-the-for-loop-and-when-is-while-the-correct-choice)
7. [Q7. What distinguishes a do-while loop from a while loop, and when is do-while the right choice?](#q7-what-distinguishes-a-do-while-loop-from-a-while-loop-and-when-is-do-while-the-right-choice)
8. [Q8. How does foreach work under the hood, and what contract must a type satisfy to be foreach-compatible?](#q8-how-does-foreach-work-under-the-hood-and-what-contract-must-a-type-satisfy-to-be-foreach-compatible)
9. [Q9. What does break do in a loop context, and how does its behavior differ when break appears inside a switch nested within a loop?](#q9-what-does-break-do-in-a-loop-context-and-how-does-its-behavior-differ-when-break-appears-inside-a-switch-nested-within-a-loop)
10. [Q10. What does continue do, and how does it differ from break in the context of a filtered foreach loop?](#q10-what-does-continue-do-and-how-does-it-differ-from-break-in-the-context-of-a-filtered-foreach-loop)
11. [Q11. What is goto in C#, when is it permissible, and why should it be avoided in production code?](#q11-what-is-goto-in-c-when-is-it-permissible-and-why-should-it-be-avoided-in-production-code)
12. [Q12. How do nested loops behave, and what is the performance implication of each additional nesting level?](#q12-how-do-nested-loops-behave-and-what-is-the-performance-implication-of-each-additional-nesting-level)
13. [Q13. How does pattern matching work in a switch, and why does arm ordering determine correctness?](#q13-how-does-pattern-matching-work-in-a-switch-and-why-does-arm-ordering-determine-correctness)
14. [Q14. What is loop variable capture in closures, and what unexpected behavior does it produce in a for loop?](#q14-what-is-loop-variable-capture-in-closures-and-what-unexpected-behavior-does-it-produce-in-a-for-loop)
15. [Q15. What is an infinite loop, and what are the safe exit patterns for bounded-retry and server-loop scenarios?](#q15-what-is-an-infinite-loop-and-what-are-the-safe-exit-patterns-for-bounded-retry-and-server-loop-scenarios)
16. [Q16. What is the discard pattern _ in switch expressions, and how does it differ from default in a switch statement?](#q16-what-is-the-discard-pattern-in-switch-expressions-and-how-does-it-differ-from-default-in-a-switch-statement)
17. [Q17. What does the compiler do when a switch expression is non-exhaustive, and how does this differ for enum vs open types?](#q17-what-does-the-compiler-do-when-a-switch-expression-is-non-exhaustive-and-how-does-this-differ-for-enum-vs-open-types)
18. [Q18. When should you prefer a switch expression over a switch statement, and when does the statement remain preferable?](#q18-when-should-you-prefer-a-switch-expression-over-a-switch-statement-and-when-does-the-statement-remain-preferable)
19. [Q19. What happens when a C# condition uses assignment = instead of equality == inside an if statement?](#q19-what-happens-when-a-c-condition-uses-assignment-instead-of-equality-inside-an-if-statement)
20. [Q20. What exception is thrown when a collection is modified during foreach, and what are the safe alternatives?](#q20-what-exception-is-thrown-when-a-collection-is-modified-during-foreach-and-what-are-the-safe-alternatives)
21. [Q21. Why does break inside a switch nested within a loop exit only the switch, not the loop, and how do you exit both?](#q21-why-does-break-inside-a-switch-nested-within-a-loop-exit-only-the-switch-not-the-loop-and-how-do-you-exit-both)
22. [Q22. What bug does capturing a for-loop variable inside a lambda closure cause, and what is the idiomatic fix?](#q22-what-bug-does-capturing-a-for-loop-variable-inside-a-lambda-closure-cause-and-what-is-the-idiomatic-fix)
23. [Q23. What does an off-by-one error look like in a for loop, and how do you verify boundary conditions reliably?](#q23-what-does-an-off-by-one-error-look-like-in-a-for-loop-and-how-do-you-verify-boundary-conditions-reliably)
24. [Q24. How does case ordering with when guards lead to silently unreachable arms, and how do you prevent it?](#q24-how-does-case-ordering-with-when-guards-lead-to-silently-unreachable-arms-and-how-do-you-prevent-it)
25. [Q25. A developer ports a C-style fulfillment router to C#. The build fails with CS0163. Review the switch statement, identify all defects, and describe the corrected version.](#q25-a-developer-ports-a-c-style-fulfillment-router-to-c-the-build-fails-with-cs0163-review-the-switch-statement-identify-all-defects-and-describe-the-corrected-version)
26. [Q26. A nightly billing job under-counts warehouse slots. QA confirms the invoice is short by exactly shelvesPerAisle slots per run. Review the nested for loop, identify the defect, and describe the fix.](#q26-a-nightly-billing-job-under-counts-warehouse-slots-qa-confirms-the-invoice-is-short-by-exactly-shelvesperaisle-slots-per-run-review-the-nested-for-loop-identify-the-defect-and-describe-the-fix)
27. [Q27. A gate-controller service hangs in staging when the gate never opens. The retry loop is missing an increment. Review the while loop, describe both the original defect and the unsafe "fix" a colleague proposed, and define the safe bounded-retry pattern.](#q27-a-gate-controller-service-hangs-in-staging-when-the-gate-never-opens-the-retry-loop-is-missing-an-increment-review-the-while-loop-describe-both-the-original-defect-and-the-unsafe-fix-a-colleague-proposed-and-define-the-safe-bounded-retry-pattern)
28. [Q28. A barcode scan worker sums active line quantities but under-reports totals. A developer used break instead of continue to skip zero-quantity lines. Review the foreach loop, explain the difference, and describe the corrected version.](#q28-a-barcode-scan-worker-sums-active-line-quantities-but-under-reports-totals-a-developer-used-break-instead-of-continue-to-skip-zero-quantity-lines-review-the-foreach-loop-explain-the-difference-and-describe-the-corrected-version)
29. [Q29. You need to poll an external inventory API up to N times with exponential back-off, exiting early on success and respecting a CancellationToken. How do you structure the loop and what are the key design decisions?](#q29-you-need-to-poll-an-external-inventory-api-up-to-n-times-with-exponential-back-off-exiting-early-on-success-and-respecting-a-cancellationtoken-how-do-you-structure-the-loop-and-what-are-the-key-design-decisions)
30. [Q30. You have a five-arm classic switch statement that maps HTTP status codes to log severity levels. Refactor it into a switch expression, explain what you gain, and identify the one scenario where the statement form would still be preferable.](#q30-you-have-a-five-arm-classic-switch-statement-that-maps-http-status-codes-to-log-severity-levels-refactor-it-into-a-switch-expression-explain-what-you-gain-and-identify-the-one-scenario-where-the-statement-form-would-still-be-preferable)

---
## Foundation Questions

---

## Q1. How does the if / else if / else chain evaluate its conditions, and what guarantee does it provide about which branch runs?

**Concepts**
- Sequential top-to-bottom condition evaluation
- First-true-wins branch selection
- Mutually exclusive execution
- else clause as unconditional catch-all
- Strict bool requirement in C#

**Answer**

The if / else if / else chain evaluates each condition in source order, from top to bottom, and executes the body of the first condition that returns true. Once a branch body executes, all remaining else if and else arms are skipped entirely — this first-true-wins guarantee means that even if a later condition would also be true, it never runs. The else clause, when present, fires only when every preceding condition returned false, making it the safe catch-all for all remaining input. Because the ordering determines which branch runs for overlapping conditions, more specific checks must be placed before broader ones: a "bulk VIP" rule must appear before a plain "high-value" rule, or the broader rule captures the input first. Unlike C or JavaScript, C# requires every if condition to be a strict bool expression — writing if (count) instead of if (count > 0) is a compile error because int does not implicitly convert to bool. Nested ifs communicate that the inner condition only makes sense once the outer one passes, while flat else-if chains signal mutually exclusive alternatives; choosing the right shape makes the intent readable at a glance.

---

## Q2. What are the rules for break in a classic switch statement, and how do shared case labels differ from C-style fall-through?

**Concepts**
- Mandatory break / return / throw terminator
- CS0163 fall-through compile error
- Shared empty case labels for OR logic
- default case as catch-all label
- goto case for deliberate jump

**Answer**

Every case body in a classic C# switch statement that contains executable statements must end with break, return, throw, or goto — otherwise the compiler raises CS0163 ("control cannot fall through from one case label to the next"). This is a deliberate departure from C and C++, where a missing break silently executes the next case body. C# treats implicit fall-through as error-prone and bans it at compile time. The one legal exception is shared case labels: two or more consecutive case labels stacked above a single block with no statements between them form a valid group, because there is nothing to fall through between them. This is the correct idiom for OR-style logic such as `case "Shipped": case "Delivered": return "In transit";`. The default label runs when no other case matches and may be placed anywhere in the switch, though convention puts it last. goto case value and goto default allow deliberate jumps between cases and are the only legal way to transfer control to a sibling case that would otherwise require break. Switch on strings is evaluated using ordinal comparison; null input falls to default rather than throwing, so null handling should be explicit.

---

## Q3. How does a switch expression differ from a switch statement, and what does the discard pattern _ represent?

**Concepts**
- Expression vs statement distinction
- Value-returning arm syntax `=>`
- Discard pattern _ as default arm
- Compiler exhaustiveness analysis (CS8509)
- InvalidOperationException for unhandled input

**Answer**

A switch expression, introduced in C# 8, is a compact value-returning form that evaluates to a single result rather than executing a block of statements. Where a switch statement uses `case label:` with break, a switch expression uses comma-separated arms written as `pattern => value`, and the whole expression can be used directly in an assignment, return statement, or method argument. The discard pattern `_` serves as the unconditional catch-all arm, equivalent to default in the statement form, and participates in the compiler's exhaustiveness analysis. If the arms do not collectively cover all possible input values and no discard arm exists, the compiler emits warning CS8509 and, if that path is reached at runtime, throws InvalidOperationException. The discard arm must be placed last because any arm after it would be statically unreachable. Switch expressions integrate seamlessly with all C# pattern kinds — constant, type, relational, property, and positional — and are the preferred form whenever the goal is a pure mapping from input to output value. The statement form remains appropriate when each branch must execute multiple side-effecting statements rather than yield a single result.

---

## Q4. What is a when guard in a switch arm, and how does it interact with the two-phase pattern-matching evaluation?

**Concepts**
- when guard as secondary boolean condition
- Two-phase: pattern match then guard evaluation
- Guard can reference pattern-bound variable
- Arm ordering and guard shadowing
- CS8510 unreachable-arm warning

**Answer**

A when guard attaches a supplementary boolean condition to a switch arm: `case pattern when condition:` in a statement, or `pattern when condition =>` in an expression. Evaluation is two-phase: the runtime first tests whether the value matches the structural pattern (type, constant, or shape), and only if the pattern matches does it evaluate the when condition. The arm is selected only when both the pattern test and the when condition return true. Because C# uses first-match-wins ordering, a broader pattern without a guard placed before a narrower guarded arm will shadow it: `case int n:` before `case int n when n > 0:` means the guarded arm is never reached for any integer, because the unguarded arm matches all ints first. The compiler emits CS8510 for statically detectable unreachable arms, but overlapping type patterns with complex guards can escape static analysis and produce silent logic errors at runtime. The reliable ordering rule is most specific first — guarded arms before unguarded arms of the same type, narrower constant patterns before broader type patterns. The guard expression can freely reference the variable bound in the pattern, enabling expressive conditions such as `case string s when s.Length > 5`.

---

## Q5. How is the for loop structured, and what is the off-by-one pitfall when choosing between < and <=?

**Concepts**
- Initializer, condition, and iterator clauses
- Inclusive vs exclusive upper bound
- Zero-based index convention
- Off-by-one fencepost error
- Variable scoped to loop header

**Answer**

The for loop header contains three semicolon-separated clauses: the initializer runs once before the first iteration, the condition is evaluated before each pass and stops the loop when false, and the iterator runs after each body execution. A standard counted loop is `for (int i = 0; i < n; i++)`, which visits zero-based indices 0 through n-1. The off-by-one pitfall arises from choosing the wrong comparison operator at the boundary. Using `i < n` when iterating an array of length n is correct because the last evaluated index is n-1; writing `i <= n` adds one extra iteration where i equals n, and `array[n]` throws IndexOutOfRangeException. Conversely, when iterating a 1-based range such as bin numbers 1 through 10, `i <= 10` is correct because 10 is the inclusive last value. The mental verification technique is substitution: plug in the intended first value and last value of i, confirm the condition is true for both, and confirm it is false for one-past-the-last value. The loop variable declared in the initializer is scoped to the loop; it is not accessible after the closing brace. The iterator clause may be omitted when the body advances the variable, but the two semicolons in the header are still required.

---

## Q6. How does the while loop differ from the for loop, and when is while the correct choice?

**Concepts**
- Pre-test condition evaluation
- Zero-iteration possibility
- Unknown iteration count at loop start
- Condition-driven rather than counter-driven termination
- Progress guarantee requirement

**Answer**

A while loop tests its condition before every iteration, including the very first one, so if the condition starts false the body never runs. This makes it structurally identical to a for loop with no initializer or iterator — any while can be rewritten as a for and vice versa. The practical distinction is idiomatic intent. A for loop is the natural choice when you know the start, end, and step at the point you write the loop: iterating indices over an array, printing a countdown, processing a fixed-size batch. A while loop is the natural choice when the number of iterations is unknown at the start and the loop must continue until some external or derived condition changes: reading from a stream until end-of-file, retrying an operation until it succeeds, draining a queue until empty, or scanning input until a sentinel value appears. Writing a for loop for an unknown count forces an awkward dummy upper bound; writing a while loop for a counted sequence hides the termination logic inside the body rather than the header. The key discipline with while is that every iteration must make provable progress toward the exit condition, because a while loop with a condition that never becomes false produces an infinite loop. When maximum iterations matter, prefer a counter-bounded while or a for loop.

---

## Q7. What distinguishes a do-while loop from a while loop, and when is do-while the right choice?

**Concepts**
- Post-test condition evaluation
- Guaranteed at-least-once execution
- At-least-once semantics vs zero-or-more
- Semicolon required after closing condition
- Avoiding duplicate pre-loop call

**Answer**

A do-while loop places its condition after the loop body rather than before it. The body always executes at least once, and only then is the condition evaluated; if true, the loop repeats. A while loop, by contrast, checks the condition first and may run zero times if the condition starts false. The canonical justification for do-while is any operation that must be performed before it can be meaningfully tested. Reading user input is the classic example: you cannot test whether the input satisfies a rule before the read has happened. Without do-while, the alternative is to duplicate the read before the loop and again inside the loop body, creating a maintenance hazard where a change to the read logic must be made in two places. The do-while collapses both occurrences into the single body. The same pattern applies to first barcode scans, initial menu presentation, and first-attempt operations in retry logic where at least one attempt is guaranteed. A frequent syntactic mistake is omitting the required semicolon after the closing `while (condition);`, which compiles the code as an empty while loop body followed by a detached block, a silent logic error. Whenever the body must run at least once, prefer do-while; whenever the body may legitimately run zero times, prefer while or for.

---

## Q8. How does foreach work under the hood, and what contract must a type satisfy to be foreach-compatible?

**Concepts**
- IEnumerable<T> and GetEnumerator pattern
- Duck-typing enumerator contract
- MoveNext and Current iteration protocol
- Compiler desugaring into enumerator loop
- Dispose called in finally on early exit

**Answer**

The foreach statement is syntactic sugar that the compiler desugars into a while loop driven by an enumerator. For a type to be foreach-compatible it must either implement IEnumerable<T> (or the non-generic IEnumerable) or satisfy the duck-typing enumerator pattern: it must expose a public `GetEnumerator()` method returning an object with a `bool MoveNext()` method and a readable `Current` property. At the start of the loop the compiler calls `GetEnumerator()`, then repeatedly calls `MoveNext()`, which advances to the next element and returns true while elements remain, reading `Current` after each successful advance. When MoveNext returns false the loop exits. The compiler wraps the desugared loop in a try/finally block that calls `Dispose()` on the enumerator if the enumerator implements IDisposable, ensuring resources are released even when the loop exits early via break or an exception. This is why iterator methods using `yield return` can contain `using` blocks whose cleanup runs correctly on early loop exit. Arrays, `List<T>`, `IQueryable<T>`, and all standard collection types implement `IEnumerable<T>`. The foreach element variable is read-only — assignment to it is a compile error — and the underlying collection must not be structurally modified during iteration or `InvalidOperationException` is thrown.

---

## Q9. What does break do in a loop context, and how does its behavior differ when break appears inside a switch nested within a loop?

**Concepts**
- Exit of innermost enclosing construct
- Loop-break vs switch-break scope
- No labeled break in C# (unlike Java)
- Bool flag pattern for outer-loop exit
- return as clean alternative

**Answer**

Break always exits the innermost enclosing construct, whether that is a loop or a switch. In a loop context, break transfers control to the first statement after the loop's closing brace, abandoning all remaining iterations and the current condition check. This is the right tool when you have found what you need and further work is wasted. The scope rule creates a common trap: when a switch statement is nested inside a for or foreach loop, a break inside any case label exits the switch, not the enclosing loop, and the loop's next iteration begins normally. A developer who intended the loop to stop when a case matched will find the loop continues. C# has no labeled break statement (unlike Java), so there is no direct syntax to exit an outer loop from inside a nested switch. The two practical solutions are: a bool flag variable set inside the matched case and tested with an explicit `if (found) break;` immediately after the switch closes; or a return statement, which exits the method entirely and is typically the simplest and cleanest approach when early-exit-on-find is the goal. Extracting the loop-and-switch into a dedicated helper method that uses return is idiomatic C# and eliminates the need for a flag variable.

---

## Q10. What does continue do, and how does it differ from break in the context of a filtered foreach loop?

**Concepts**
- Skip current iteration remainder
- Advance to next iteration's condition check
- Guard-clause pattern with continue
- continue vs break distinction
- for-loop iterator still runs after continue

**Answer**

Continue skips the remaining statements in the current loop body and transfers control to the loop's next-iteration logic: the condition re-evaluation in a while or do-while, or the iterator step (i++) in a for loop before the condition re-evaluation. The loop itself continues — only the current pass is cut short. Break, by contrast, exits the loop entirely. The practical difference is significant: break on the first bad element abandons all subsequent elements; continue on the first bad element only skips that element and processes everything else. A common idiom is using continue as a guard clause at the top of a loop body to skip invalid or uninteresting elements early, which keeps the main processing logic at a flat indentation level rather than wrapping it in a deep if. For example, `if (qty <= 0) { continue; }` at the top of a foreach is cleaner than nesting the entire sum block inside `if (qty > 0) { ... }`. In a for loop, continue causes the iterator expression to run before re-testing the condition, so the loop counter advances correctly even when continue fires. Confusing break and continue — especially substituting break for continue when the intent is to skip a single element — is among the most common loop bugs and produces silent under-counting in financial and inventory calculations.

---

## Q11. What is goto in C#, when is it permissible, and why should it be avoided in production code?

**Concepts**
- Unconditional jump to a named label
- goto case and goto default in switch
- Scope restrictions on forward jumps
- Spaghetti-code readability cost
- Structured alternatives

**Answer**

goto transfers control unconditionally to a named label elsewhere in the same method. C# supports three forms: `goto labelName` jumps to a statement label defined as `labelName:`, `goto case value` jumps to a specific case within the immediately enclosing switch statement, and `goto default` jumps to the switch's default label. The compiler enforces scope rules — goto cannot jump forward into a nested block that introduces variables the jump would skip, preventing partially constructed variable lifetimes. The argument against goto in production code is readability and maintainability: arbitrary jumps break the top-down mental model that structured control flow provides. Loops, break, continue, return, and method calls cover every legitimate use case goto offers, with clear entry and exit points that make code paths traceable without following label chains. The one area where goto case has genuine utility is a switch statement where a case legitimately needs to share a different case's body without duplicating code, and a helper method extraction would be disproportionate. Code generators and state-machine compilers also emit goto freely because their output is not intended to be read by humans. In hand-written application code, encountering goto is a signal to refactor the control structure into an explicit loop or method call.

---

## Q12. How do nested loops behave, and what is the performance implication of each additional nesting level?

**Concepts**
- Multiplicative total iteration count
- O(m × n) complexity for two-level nesting
- break scope limited to innermost loop
- Grid and Cartesian-product traversal pattern
- Method extraction for multi-level early exit

**Answer**

Nested loops place one loop entirely inside the body of another. For each iteration of the outer loop, the inner loop runs its full cycle, so the total number of inner-body executions is the product of all loop counts. A two-level nest with outer count m and inner count n yields m × n total inner iterations; a third level multiplies again to m × n × p. This multiplicative growth is the reason nested loops over large data sets are a common performance bottleneck. A naive all-pairs search over two 1,000-element lists executes one million comparisons, while a sort-then-binary-search approach achieves the same result in roughly 1,000 × 20 operations. Each loop level has its own variable scope, so the inner loop's counter is not visible outside the inner block. A break inside the inner loop exits only the inner loop; the outer loop continues normally to its next iteration. To exit both levels simultaneously, the standard patterns are: a bool flag set inside the inner loop that the outer loop also checks with a subsequent break, or a return statement in a helper method that exits both loops and the method in one step. The latter is idiomatic C# for search routines and is strongly preferred over multi-level flag variables.

---

## Q13. How does pattern matching work in a switch, and why does arm ordering determine correctness?

**Concepts**
- Type pattern binding with named variable
- First-match-wins semantics
- Broad pattern shadowing narrow guard
- CS8510 unreachable-arm compiler warning
- Specific before general ordering rule

**Answer**

Pattern matching in a switch evaluates arms in source order and selects the first arm whose pattern matches the input value. A type pattern such as `case int n` matches any value of type int and binds it to the name n. A guarded arm such as `case int n when n > 0` matches any positive int. Because evaluation is first-match-wins, a bare `case int n` arm placed above `case int n when n > 0` in the same switch will always win for positive integers, making the guarded arm unreachable. The compiler performs pattern-subsumption analysis and emits CS8510 ("The pattern is unreachable") when it can statically determine that a prior arm's pattern fully covers the new arm's input space. However, subsumption analysis has limits — when guards involve arbitrary boolean expressions, the compiler cannot always determine unreachability and emits no warning, leaving silent logic errors in production code. The ordering rule is: always place the most specific arm first and the most general arm last. Concretely: guarded arms before unguarded arms of the same type, narrower constant cases before type patterns, and the discard arm `_` last of all. A code-review checklist for any switch on objects or polymorphic types should verify the from-specific-to-general order of every arm sequence.

---

## Q14. What is loop variable capture in closures, and what unexpected behavior does it produce in a for loop?

**Concepts**
- Closure by reference, not by value
- Single loop-variable reference shared across closures
- Deferred lambda invocation reads final value
- Local-copy workaround for for loops
- C# 5 foreach per-iteration variable fix

**Answer**

A closure captures a variable by reference, not by the value it held at capture time. When a lambda or delegate is created inside a loop body and references the loop counter, all lambdas created across every iteration share a single reference to the same variable. If those lambdas are invoked after the loop finishes — for example, stored in a list and executed later — every closure reads the variable's value at the moment of invocation, which is the final post-loop value, not the value at the iteration that created each closure. A for loop from 0 to 9 that appends `() => Console.WriteLine(i)` to an action list and then invokes all actions prints "10" ten times instead of 0 through 9, because i is 10 after the loop exits. The fix is to declare a local variable inside the loop body, assign the counter to it, and close over the local: `int copy = i; actions.Add(() => Console.WriteLine(copy));`. Because copy is declared inside the loop block, each iteration allocates a distinct variable, and each closure captures its own private copy. In C# 5, the language specification was changed so that the foreach iteration variable is logically scoped to each iteration — a new variable is created per pass — making foreach closures safe by default in C# 5 and later. For loops still use a single shared variable and require the explicit local copy.

---

## Q15. What is an infinite loop, and what are the safe exit patterns for bounded-retry and server-loop scenarios?

**Concepts**
- while(true) and for(;;) constructs
- Mandatory exit path via break / return / throw
- Bounded retry pattern with counter
- CancellationToken for cooperative shutdown
- Progress guarantee requirement

**Answer**

An infinite loop is any loop whose condition never becomes false on its own, most commonly written as `while (true)` or `for (;;)`. The loop body is entirely responsible for exiting via break, return, or throw. In server and daemon processes, an outer `while (true)` loop that dispatches work, then sleeps or awaits the next item, is a legitimate architecture — provided the loop responds to a shutdown signal. The idiomatic mechanism for cooperative cancellation in .NET is CancellationToken: calling `cancellationToken.ThrowIfCancellationRequested()` at the top of each iteration converts a cancellation request into an OperationCanceledException that propagates cleanly out of the loop. In application code where a fixed maximum number of attempts is meaningful, the bounded-retry pattern is safer than `while (true)`: a counter variable is incremented each iteration and the loop exits when the counter reaches the maximum, guaranteeing termination even when success is never achieved. The most common cause of unintentional infinite loops is forgetting to advance the variable that drives the exit condition — a while loop whose counter is never incremented spins forever, freezing the thread. Every loop should have an obvious answer to the question "what makes this stop?"; if the answer is not visible from the condition and the iterator, the loop needs a comment at minimum and a refactor at best.

---

## Q16. What is the discard pattern _ in switch expressions, and how does it differ from default in a switch statement?

**Concepts**
- Discard pattern as unconditional match
- Compiler exhaustiveness analysis for expressions
- CS8509 non-exhaustive switch warning
- default as label vs _ as pattern
- InvalidOperationException for unhandled input

**Answer**

In a switch expression, the discard pattern `_` is an arm that matches any value not already covered by a preceding arm, serving the same logical role as the default label in a switch statement. The key distinction is that `_` is a true pattern and participates in the compiler's exhaustiveness analysis. If no `_` arm is present and the compiler can determine that some input values are not handled — definitively provable for closed value sets like bool or enum — it emits warning CS8509 ("The switch expression does not handle all possible values of its input type"). At runtime, an unhandled value throws InvalidOperationException. The switch statement's default label does not trigger a corresponding exhaustiveness warning; if no case matches and no default exists, execution simply falls through to the next statement after the switch without error. The `_` arm must appear last in a switch expression because any arm following it would be statically unreachable. In a switch statement, default can legally appear anywhere in the case list, though convention places it at the end for readability. Both mechanisms cover all remaining inputs; the difference is that `_` participates in static correctness enforcement while default is a runtime fallback with no compile-time guarantee of completeness.

---

## Q17. What does the compiler do when a switch expression is non-exhaustive, and how does this differ for enum vs open types?

**Concepts**
- CS8509 non-exhaustive switch expression warning
- Closed vs open value-set exhaustiveness
- Enum coverage analysis
- InvalidOperationException for missed input
- Defensive discard arm for enum safety

**Answer**

When the compiler can statically determine that a switch expression does not handle every possible input value, it emits warning CS8509. If that uncovered path is reached at runtime, the switch expression throws InvalidOperationException rather than silently returning a default value. The strength of exhaustiveness analysis depends on how closed the input type is. For bool (two values) and enum (finite declared members), the compiler can enumerate all variants and verify every one is covered, so missing any produces a definite CS8509 warning. For open types such as int, string, or object, the value space is unbounded and the compiler cannot exhaustively check without a discard arm — it requires either `_` or an equivalent catch-all. A subtle enum gotcha: if a switch expression covers every declared member but has no discard arm, the compiler considers it exhaustive. However, an integer can be cast to any enum value regardless of whether that value has a declared name, and such an out-of-range cast at runtime reaches the unhandled path and throws. Adding `_ => throw new ArgumentOutOfRangeException(nameof(input))` or `_ => defaultValue` is the defensive practice for enum switches in production code. In library code that receives enums from external callers, never assume the enum value is a declared member.

---

## Q18. When should you prefer a switch expression over a switch statement, and when does the statement remain preferable?

**Concepts**
- Pure value mapping vs multi-statement action dispatch
- Expression assignable inline
- Exhaustiveness enforcement advantage
- Arm with side effects
- goto case unavailable in expressions

**Answer**

A switch expression is the right choice whenever the goal is a pure mapping from an input value to an output value — a transformation where each arm yields a single result. The concise arm syntax, the ability to embed the expression inline in an assignment or return statement, and the compiler's exhaustiveness enforcement all favor the expression form for scenarios such as translating a status code to a label, mapping an enum to a configuration value, or computing a discount rate from a tier. The gains are concrete: the result can flow directly into a method parameter or property initializer without declaring a mutable local first, the `_` arm forces you to decide what the default should be, and the visual alignment of arms makes the table-like intent of the mapping immediately clear. The switch statement remains preferable when each branch must execute multiple side-effecting statements — updating shared state, writing to a logger, throwing a custom exception with a multi-step message — rather than yielding a single value. If you find yourself wrapping switch expression arms in method calls purely to accommodate multiple statements, the branch complexity has exceeded the expression form's intent and a statement with well-named helper calls is cleaner. goto case, which has no switch-expression equivalent, is occasionally needed in statements and cannot be replaced. The pragmatic standard is: expression for mapping, statement for action dispatch.

---

## Gotchas — Control Flow & Loops (Interview Traps)

---

#### Gotcha 1. Captured loop variable in a for loop — all closures share the same variable

**Concepts**
- for loop creates one variable for all iterations
- lambda captures by reference not value
- all lambdas see the final value
- fix by copying to a local inside the loop

**Answer**

When you create a lambda inside a for(int i = 0; i < N; i++) loop and add it to a list, all lambdas share a reference to the single i variable, which by the time any lambda executes will equal N (the final value). The fix is to copy i to a separate local variable (int copy = i) inside the loop body before the lambda, so each lambda captures its own independent slot.

---

#### Gotcha 2. Modifying a collection inside foreach throws InvalidOperationException

**Concepts**
- foreach tracks version number
- Add/Remove/Clear increments version
- MoveNext detects version mismatch
- collect items to change, apply after loop

**Answer**

The foreach statement holds an enumerator that tracks a version counter on the collection. Any structural modification (Add, Remove, Insert, Clear) increments the version, and the enumerator's MoveNext detects this mismatch and throws InvalidOperationException. The safe pattern is to collect the items you want to remove in a separate list during the foreach, then loop through the removal list afterward.

---

#### Gotcha 3. switch fall-through does not exist in C# — omitting break is a compile error

**Concepts**
- CS8070 compile error for implicit fall-through
- explicit goto case allowed
- empty cases fall through
- default can appear anywhere

**Answer**

Unlike C and Java where fall-through in switch cases is implicit when break is omitted, C# requires every non-empty case to have an explicit exit (break, return, goto case, or throw). This prevents an entire class of accidental fall-through bugs; the compiler enforces it with CS8070. Empty cases (with no statements before the next case label) are the only exception that allows fall-through.

---

#### Gotcha 4. switch on string uses ordinal comparison — OrdinalIgnoreCase requires when or nested if

**Concepts**
- String switch is case-sensitive by default
- no StringComparison option on switch
- use when clause for case-insensitive
- or normalize input before switch

**Answer**

A switch statement on a string uses an exact ordinal comparison, so case "Apple": does not match "apple" or "APPLE". For case-insensitive matching, either normalize the input with .ToUpperInvariant() before the switch, or use when clauses: case var s when s.Equals("apple", StringComparison.OrdinalIgnoreCase):.

---

#### Gotcha 5. do-while executes at least once even when the condition is initially false

**Concepts**
- Body executes before condition check
- while checks condition first
- do-while is correct for input validation loops
- easy to confuse when migrating logic

**Answer**

A do-while loop guarantees exactly one execution of the body before evaluating the condition, regardless of the condition's initial value. Developers who replace a do-while with while (or vice versa) when refactoring produce off-by-one execution errors — the body either executes one extra time or skips execution entirely on the first iteration.

---

#### Gotcha 6. break inside nested loops exits only the innermost loop, not the outer one

**Concepts**
- break exits one loop level
- no labeled break in C# (unlike Java)
- flag variable or method return to exit outer loop
- goto label as labeled break alternative

**Answer**

C# does not support labeled break statements like Java does. Writing break inside an inner loop exits only that inner loop, leaving the outer loop continuing. To break out of multiple nesting levels, use a bool flag variable checked by the outer loop, or extract the nested logic into a method and use return to exit both.

---

#### Gotcha 7. continue in a for loop still executes the increment expression

**Concepts**
- continue jumps to the increment step in for
- increment runs before condition re-check
- does not skip the increment
- differs from while where continue jumps to condition

**Answer**

In a for(int i = 0; i < N; i++) loop, hitting continue does not skip to the next iteration from the current position — it jumps to the increment expression (i++) first, then re-evaluates the condition. This is the intended behavior for for loops, but developers migrating from languages with different continue semantics may be surprised that the counter still advances normally.

---

#### Gotcha 8. Pattern matching in switch with when guards — evaluation order matters

**Concepts**
- Case labels evaluated top-to-bottom
- when guard evaluated only if type pattern matches
- first matching case wins
- ordering determines which case handles ambiguous inputs

**Answer**

When multiple case patterns could match the same value, the runtime evaluates them in source order and stops at the first matching case whose when guard (if any) is also true. If a more general pattern appears before a more specific one, the specific case is dead code. The compiler warns about unreachable case sections, but complex when guards can hide the issue.

---

#### Gotcha 9. goto case in switch must name a constant label — variables are not allowed

**Concepts**
- goto case requires a compile-time constant
- goto case variableName is CS0159
- goto default is valid
- goto to a label outside switch is valid for special scenarios

**Answer**

The goto case statement requires a constant expression that matches one of the case labels. Writing goto case x where x is a local variable is a compile error CS0159 because the target must be determinable at compile time. Only goto default and goto case with a literal or const value are valid inside a switch.

---

#### Gotcha 10. Index from end (^1) is not the same as -1 — it's from-end Index type arithmetic

**Concepts**
- ^1 is new Index(1, fromEnd: true)
- index[^1] accesses last element
- ^0 is one-past-the-end (invalid)
- combining arithmetic needs care

**Answer**

The ^ operator creates an Index value that represents a position relative to the end of a sequence. ^1 refers to the last element (equivalent to array.Length - 1), and ^0 refers to length itself (one past the end, which would throw for indexing). Mixing ^ with regular integer arithmetic in the same expression requires understanding that they are Index and int types, not the same numeric type.

---

## Real-World Scenarios

---

## Q25. A developer ports a C-style fulfillment router to C#. The build fails with CS0163. Review the switch statement, identify all defects, and describe the corrected version.

**Concepts**
- CS0163 fall-through compile error
- Undeclared variable inside a case body
- Shared labels as the fix for OR-style routing
- return as an alternative to break

**Answer**

The defective switch assigns to an undeclared variable and leaves the "Packed" case without a terminating statement, which causes two compile errors.

```csharp
public static string RouteStatus(string status)
{
    switch (status)
    {
        case "Pending":
            return "Awaiting pick list";
        case "Picking":
            return "Items being collected";
        case "Packed":
            message = "Ready for carrier";     // CS0103: 'message' undeclared
        case "Shipped":                         // CS0163: fall-through from "Packed"
        case "Delivered":
            return "In transit pipeline";
        default:
            return "Unknown — escalate";
    }
}
```

| Category | Problem | Impact |
|---|---|---|
| Compilation | `message` is not declared; CS0103 | Build failure — method does not compile |
| Compilation | "Packed" case body has no break / return / throw; CS0163 | Build failure — fall-through rejected by compiler |
| Logic | Even if the errors were resolved by accident, intended "Ready for carrier" string is lost | Wrong output for "Packed" status at runtime |

**Fix priority**

1. Replace the `message = "Ready for carrier";` assignment with `return "Ready for carrier";` — this both declares the return value inline and terminates the case, eliminating CS0103 and CS0163 in one edit.
2. Verify that "Shipped" and "Delivered" remain as stacked empty labels sharing the `return "In transit pipeline";` body, which is the intended OR-routing behavior.
3. Add a unit test asserting `RouteStatus("Packed") == "Ready for carrier"` and `RouteStatus("Shipped") == "In transit pipeline"` to confirm the routing table after the fix.

---

## Q26. A nightly billing job under-counts warehouse slots. QA confirms the invoice is short by exactly shelvesPerAisle slots per run. Review the nested for loop, identify the defect, and describe the fix.

**Concepts**
- Off-by-one in outer loop condition
- < vs <= for 1-based inclusive range
- Multiplicative iteration count
- Substitution verification technique
- Unit test as regression guard

**Answer**

The outer loop uses a strict less-than `<` for a 1-based range that should be inclusive, skipping the last aisle entirely on every call.

```csharp
public static int CountBillableSlots(int aisles, int shelvesPerAisle)
{
    int count = 0;
    for (int aisle = 1; aisle < aisles; aisle++)     // defect: < should be <=
    {
        for (int shelf = 1; shelf <= shelvesPerAisle; shelf++)
        {
            count++;
        }
    }
    return count;
}
// Called as CountBillableSlots(5, 10). Expected: 50. Actual: 40.
```

| Category | Problem | Impact |
|---|---|---|
| Logic | Outer loop condition `aisle < aisles` excludes the last aisle in a 1-based range | Under-count by `shelvesPerAisle` per call |
| Business | Every invoice is systematically short regardless of the actual warehouse configuration | Revenue loss on every billing cycle |
| Verification | No unit test exists to catch this class of boundary error | Defect survived to production |

**Fix priority**

1. Change `aisle < aisles` to `aisle <= aisles`. Substitution check: aisles = 5, so aisle should iterate 1, 2, 3, 4, 5 — five passes, each contributing 10 inner iterations, total 50.
2. Apply the same boundary review to the inner loop — `shelf <= shelvesPerAisle` is already correct for a 1-based range.
3. Add parameterised unit tests: `CountBillableSlots(5, 10) == 50`, `CountBillableSlots(1, 1) == 1`, and `CountBillableSlots(0, 10) == 0` to catch regressions.

---

## Q27. A gate-controller service hangs in staging when the gate never opens. The retry loop is missing an increment. Review the while loop, describe both the original defect and the unsafe "fix" a colleague proposed, and define the safe bounded-retry pattern.

**Concepts**
- Missing loop-progress increment
- while(true) with unchecked maxAttempts
- Bounded retry pattern with counter
- CancellationToken for cooperative shutdown
- Progress guarantee requirement

**Answer**

The original loop never increments `attempt`, so when `TryOpenGate()` always returns false the condition `attempt < maxAttempts` is permanently true and the thread hangs indefinitely. A colleague's proposed fix of replacing the outer condition with `while (true)` and placing a `break` inside `TryOpenGate()` only on success means `maxAttempts` is never consulted, producing the same infinite hang under failure. The correct pattern bounds the loop at the call site and advances the counter unconditionally.

```csharp
public static bool WaitForGateOpen(int maxAttempts)
{
    int attempt = 0;
    while (attempt < maxAttempts)
    {
        if (TryOpenGate())
        {
            return true;
        }
        // attempt never incremented — spins forever when TryOpenGate always fails
    }
    return false;
}
```

| Category | Problem | Impact |
|---|---|---|
| Logic | `attempt++` is absent; loop condition never changes | Infinite spin when TryOpenGate never succeeds |
| Reliability | No delay between attempts | Hammers gate hardware under failure conditions |
| Design | No cancellation support | Thread cannot be shut down cooperatively |

**Fix priority**

1. Add `attempt++;` as the last statement inside the while body — place it unconditionally after the `if` block so it runs on every non-success pass.
2. Add `Thread.Sleep` or `await Task.Delay` between attempts if this calls real I/O, to avoid a spin-loop under failure.
3. Accept a `CancellationToken` parameter and call `cancellationToken.ThrowIfCancellationRequested()` at the top of the loop body to allow cooperative shutdown from a host service.

---

## Q28. A barcode scan worker sums active line quantities but under-reports totals. A developer used break instead of continue to skip zero-quantity lines. Review the foreach loop, explain the difference, and describe the corrected version.

**Concepts**
- break exits the loop entirely
- continue skips the current iteration only
- Guard-clause pattern with continue
- Silent under-count from premature loop exit
- Unit test with mixed positive and zero elements

**Answer**

The loop stops processing all elements as soon as it encounters the first non-positive quantity, because break exits the loop rather than skipping only the current element.

```csharp
public static int SumActiveLines(int[] quantities)
{
    int total = 0;
    foreach (int qty in quantities)
    {
        if (qty <= 0)
            break;       // stops entire loop on first bad element
        total += qty;
    }
    return total;
}
// Input: { 2, 0, 5, 3 }. Expected: 10. Actual: 2.
```

| Category | Problem | Impact |
|---|---|---|
| Logic | `break` exits the foreach on the first non-positive qty; should be `continue` | All elements after the first zero are silently dropped |
| Business | Active quantities after any zero or negative line are never accumulated | Pick lists and stock totals are systematically under-reported |
| Testability | No test covers a mixed-sign array with non-positive elements before positive ones | Defect undetected in unit test suites using only positive input |

**Fix priority**

1. Replace `break;` with `continue;` inside the `if (qty <= 0)` guard — this skips the current element and advances to the next iteration without exiting the loop.
2. Add a unit test with input `{ 2, 0, 5, -1, 3 }` and assert the result is 10 (2 + 5 + 3), covering positive values both before and after non-positive elements.
3. Consider renaming the guard condition to make the skip intent explicit: `if (qty <= 0) continue; // skip discontinued and zero-quantity lines`.

---

## Q29. You need to poll an external inventory API up to N times with exponential back-off, exiting early on success and respecting a CancellationToken. How do you structure the loop and what are the key design decisions?

**Concepts**
- Bounded for-loop over retry count
- ThrowIfCancellationRequested at each iteration
- Exponential back-off delay formula
- Task.Delay with CancellationToken for interruptible sleep
- Returning failed result after exhausting retries

**Answer**

The right structure is a bounded for loop, not `while (true)`, because the maximum attempt count is known and should be visible in the loop header for readability and correctness guarantees. The loop header `for (int attempt = 0; attempt < maxAttempts; attempt++)` makes the upper bound explicit and guarantees termination in O(maxAttempts) attempts. At the top of each iteration, call `cancellationToken.ThrowIfCancellationRequested()` before any I/O. This converts a cancellation request into an `OperationCanceledException` that propagates cleanly without requiring explicit return paths throughout the loop body. After the API call, if it succeeds, `return` the result — using return rather than break-plus-flag-plus-return keeps the success path in a single statement. If the call fails, calculate the delay using an exponential formula such as `TimeSpan delay = baseDelay * Math.Pow(2, attempt)` capped at a maximum to prevent multi-minute waits. Pass the `CancellationToken` to `Task.Delay(delay, cancellationToken)` so the delay itself is cancelled immediately on shutdown rather than sleeping through the full interval. After the loop exhausts all attempts without success, throw the last captured exception wrapped in a custom exception type or return a discriminated failure result — never silently return a default value, because callers cannot distinguish exhausted retries from a successful non-result. The method signature should be `async Task<T>` and accept `CancellationToken cancellationToken = default` following standard .NET async conventions.

---

## Q30. You have a five-arm classic switch statement that maps HTTP status codes to log severity levels. Refactor it into a switch expression, explain what you gain, and identify the one scenario where the statement form would still be preferable.

**Concepts**
- Switch expression as pure value-mapping form
- Inline assignment without mutable local variable
- CS8509 exhaustiveness enforcement
- Discard arm forcing explicit default decision
- Statement form for multi-statement action branches

**Answer**

The classic statement declares a mutable severity variable, assigns it in each case, and uses break, requiring five case bodies plus a default. The switch expression replaces this with a single assignment where the right-hand side is the switch expression, and each arm is a one-liner. A representative refactor produces:

```csharp
LogSeverity severity = statusCode switch
{
    200 or 201 or 204 => LogSeverity.Info,
    301 or 302        => LogSeverity.Debug,
    400               => LogSeverity.Warning,
    401 or 403        => LogSeverity.Warning,
    500 or 503        => LogSeverity.Error,
    _                 => LogSeverity.Warning
};
```

The gains are threefold. First, exhaustiveness: the `_` arm is required to silence CS8509, which forces you to explicitly decide what unlisted status codes should map to — the classic statement silently fell through to the next line with severity uninitialised. Second, the expression can be used inline as a method argument or property initialiser without declaring a prior mutable local. Third, the visual alignment of arms makes the mapping table immediately clear to reviewers. The discard arm `_ => LogSeverity.Warning` handles undocumented 4xx and 5xx codes from non-standard servers without throwing. The one scenario where the statement form remains preferable is when a branch must execute multiple side-effecting statements — for example, logging the status code with context, incrementing a counter, and then returning a severity. An expression arm for that case would require calling a helper method, which obscures the logic; a statement case block with explicit sequential steps is clearer and should be preferred over forcing multi-statement logic into a one-liner expression arm.

