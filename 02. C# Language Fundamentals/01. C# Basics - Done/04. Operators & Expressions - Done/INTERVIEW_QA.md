# Foundation Questions

---

## Q1. What is the difference between integer division and floating-point division in C#, and when does each occur?

**Concepts**
- Integer division (both operands are integral types)
- Truncation toward zero
- Numeric promotion rules
- Floating-point division with double or decimal operand
- Decimal preference for monetary arithmetic

**Answer**

When both operands of the / operator are integer types (int, long, byte, etc.), C# performs integer division and discards the fractional part by truncating toward zero — so 7 / 2 evaluates to 3, not 3.5, with no error or warning. If either operand is a floating-point or decimal type, the compiler promotes the integer to that wider type and performs floating-point division instead: 7.0 / 2 yields 3.5 as a double, and 7.0m / 2 yields 3.5m as a decimal. The distinction is critical in production because a pricing formula written with integer operands silently loses cents on every calculation. For monetary arithmetic, always use decimal literals (49.99m) so that all intermediate results remain exact and free from binary floating-point rounding. For rates and ratios where you need the fractional part but are not dealing with money, cast at least one operand to double before dividing: (double)count / total. Understanding truncation also explains why modulo complements division: totalUnits / caseSize gives the whole cases while totalUnits % caseSize gives the leftover units, and the two results together reconstruct the original value exactly.

---

## Q2. How do the prefix (++x) and postfix (x++) forms of the increment operator differ in C#?

**Concepts**
- Prefix increment — modify first, return new value
- Postfix increment — return current value, then modify
- Return value in embedded expressions
- Standalone use (behavior is identical)
- Postfix in loop conditions

**Answer**

Both prefix and postfix increment add 1 to the variable, but they differ in the value they yield when embedded inside a larger expression. The prefix form (++x) increments the variable first and produces the updated value as its result. The postfix form (x++) captures the current value as its result and then increments the variable afterward. Starting with x = 5: int a = ++x sets both x and a to 6, while int b = x++ sets b to 6 but then advances x to 7. When the increment is a standalone statement — a line on its own — the two forms are identical and either can be used. The distinction matters only when the expression value is consumed by something else. The most subtle example appears in loop conditions: while (remaining-- > 0) uses the current value of remaining for the comparison and decrements it after, so the loop body executes once more than a naive reading of the initial value suggests, and remaining ends at -1 after the loop exits. To avoid confusion, place increment and decrement on their own lines in loop bodies and reserve embedded forms for established idioms where the behavior is documented and intentional.

---

## Q3. How does short-circuit evaluation work with && and ||, and how do they differ from & and | on bool operands?

**Concepts**
- Short-circuit AND (&&) — right operand skipped when left is false
- Short-circuit OR (||) — right operand skipped when left is true
- Non-short-circuit & and | on bool
- Null-guard pattern dependency on short-circuit
- Side-effecting methods in conditions

**Answer**

The && and || operators use short-circuit evaluation: && skips the right operand when the left side is false, because false AND anything is always false. Similarly, || skips the right operand when the left side is true, because true OR anything is always true. This behavior is not merely an optimization — it is essential for null guards. The pattern obj != null && obj.Name.Length > 0 is safe because the second sub-expression only executes after obj is confirmed non-null; replacing && with & would cause a NullReferenceException when obj is null because both sides would always evaluate. The single-character & and | operators applied to bool operands compute the correct logical result but always evaluate both operands regardless of the left side's value. The only reason to use & on bool is when you deliberately need both sides to execute for their side effects — a rare requirement that should be explicitly commented. The far more common production bug is the accidental use of & instead of && in a service guard like if (creditService.IsApproved(id) & inventoryService.TryReserve(sku, qty)), where the inventory reservation runs unconditionally even when credit is denied, silently mutating state on rejected orders.

---

## Q4. What does the null-coalescing operator (??) do, and how does ??= extend it in C# 8+?

**Concepts**
- Null-coalescing — return left operand if non-null, else right
- Left-to-right chain evaluation, stops at first non-null
- Lazy right-operand evaluation
- Conditional null-assignment (??=)
- Nullable value types (int?) and nullable reference types

**Answer**

The ?? operator evaluates to its left operand when that value is not null, and to its right operand when the left is null. It provides a compact fallback for nullable references and nullable value types: string label = nickname ?? legalName ?? "Guest" returns the first non-null value reading left to right, and the right-hand side is not evaluated when the left is already non-null — so expensive method calls on the right impose no cost in the common non-null path. The ??= operator, introduced in C# 8, combines a null check with conditional assignment: cache ??= LoadFromDatabase() is equivalent to if (cache == null) cache = LoadFromDatabase() and is the idiomatic lazy-initialization pattern. Both operators work with nullable value types (int?, decimal?) as well as reference types. The most important thing to understand about ?? is its precedence: it binds looser than all arithmetic operators, so skuCode?.Length ?? 0 + 1 parses as skuCode?.Length ?? (0 + 1) because + evaluates first. When the fallback value must participate in arithmetic, always parenthesize explicitly: (skuCode?.Length ?? 0) + 1. Overlooking this is one of the most common operator precedence bugs in C# codebases because the misparse produces a result that looks correct for the null path but is wrong for the non-null path.

---

## Q5. What does the null-conditional operator (?.) return when applied to a null reference, and what type does it produce?

**Concepts**
- Null-conditional member access (?.)
- Short-circuit propagation to null
- Return type promotion to nullable
- Null-conditional indexer (?[])
- Composition with ?? for safe defaults

**Answer**

The ?. operator accesses a member or calls a method only when the left-hand operand is not null; when it is null the entire expression short-circuits and produces null rather than throwing a NullReferenceException. Because the result might be null even when the member itself is declared as a non-nullable type, the compiler promotes the expression type to nullable: customer?.Address?.City produces string? even if City is declared string. This nullable promotion flows through the entire chain, so every ?. application makes the downstream type nullable and the final result must be null-checked or coalesced before use in a non-nullable context. The ?[] variant applies the same behavior to indexers: orders?[0] returns null when orders is null rather than throwing. These operators compose naturally with ??: string city = customer?.Address?.City ?? "Unknown" safely reads a nested property and provides a fallback in one expression. When calling void-returning methods through ?., such as order?.Cancel(), the call is simply skipped when order is null with no exception raised. The null-conditional operator is read-only and cannot appear on the left side of an assignment. It is the idiomatic replacement for deeply nested null-guard if-chains and is the preferred approach for safe traversal of object graphs that may be partially null at any depth.

---

## Q6. How does operator precedence determine evaluation order in an expression that mixes *, +, and ??? 

**Concepts**
- Multiplicative group (*, /, %) — higher precedence than additive
- Additive group (+, -) — higher precedence than ??
- Null-coalescing lowest among common binary operators
- Parentheses for explicit grouping
- Left-to-right associativity for arithmetic operators

**Answer**

C# assigns each operator a precedence level that determines which operations bind first in the absence of parentheses. Multiplicative operators (*, /, %) have higher precedence than additive ones (+, -), so 10 + 20 * 3 evaluates as 10 + (20 * 3) = 70, not (10 + 20) * 3 = 90. The null-coalescing operator (??) sits below both groups in the precedence hierarchy, which means code?.Length ?? 0 + 1 is parsed as code?.Length ?? (0 + 1) — the + 1 is part of the fallback value and is not applied to the result of the coalescing when the left side is non-null. Developers who read the expression left-to-right often expect (code?.Length ?? 0) + 1, so the expression silently delivers the wrong result for non-null inputs. The full precedence ladder from high to low for common operators is: primary access, then unary, then multiplicative, then additive, then shift, then relational, then equality, then bitwise (&, ^, |), then conditional (&&, ||), then null-coalescing (??), then ternary (?:), then assignment. Parentheses override any level in this table and are the reliable remedy whenever expression intent could be misread, particularly when ?? mixes with arithmetic or comparison operators.

---

## Q7. What is the ternary conditional operator (?:) and what constraints apply to its two value branches?

**Concepts**
- Ternary operator syntax (condition ? trueExpr : falseExpr)
- Type compatibility requirement between branches
- Expression context (cannot be used as a statement)
- Nested ternary right-associativity
- Readability threshold for nesting depth

**Answer**

The ternary conditional operator takes the form condition ? valueIfTrue : valueIfFalse. The condition must evaluate to bool, and both branches must produce types that are mutually compatible — either identical, or one implicitly convertible to the other — because the compiler must resolve a single result type for the expression at compile time. Mixing incompatible types such as string and int in the two branches is a compile error unless an implicit conversion exists. The ternary operator is an expression rather than a statement, so it can appear anywhere a value is expected: in return statements, field initializers, method arguments, and LINQ projections where if/else cannot be used syntactically. When nested, the right-hand branch contains the inner ternary: a ? b : c ? d : e parses as a ? b : (c ? d : e) due to the right-to-left associativity of ?:. Nesting beyond two levels degrades readability sharply and should be replaced with if/else chains or a private helper method. The ternary operator shines when choosing between two simple values based on a boolean condition — string tier = totalUnits >= 100 ? "Bulk" : "Standard" is clear and concise. When the condition is only a nullability test, prefer the more readable null-coalescing operator (??) or null-conditional (?.) over an equivalent ternary expression.

---

## Q8. How do the bitwise operators &, |, ^, ~, <<, and >> work on integer types, and what is the idiomatic pattern for permission flags?

**Concepts**
- Bitwise AND (&) — both bits must be 1
- Bitwise OR (|) — either bit being 1 suffices
- Bitwise XOR (^) — exactly one bit is 1 (toggle)
- Bitwise NOT (~) — flips all bits
- Left shift (<<) and right shift (>>)

**Answer**

Bitwise operators work on individual bits of integral types (byte, int, long, etc.). AND (&) sets a result bit to 1 only when both corresponding input bits are 1 — this tests whether a flag is set: (role & CanWrite) != 0 returns true only when the CanWrite bit is present. OR (|) sets a result bit when either input has it — this combines flags: int role = Read | Write grants both bits at once. XOR (^) sets a result bit when exactly one of the two inputs has it — this toggles a bit on if it was off, or off if it was on, which is useful for cycling states but dangerous for unconditional flag removal because it adds the flag if it was already absent. NOT (~) inverts all bits; combining it with AND produces a reliable clear: role &= ~Write removes the Write bit unconditionally regardless of current state, which is the correct revoke pattern. Left shift (1 << 3 = 8) defines flag constants as expressive bit positions, and right shift reverses that for positive integers. The canonical permission flags pattern is: define constants with 1 << n for each capability, combine with |=, test with & != 0, and remove with &= ~Flag. In .NET 10, decorating an enum with [Flags] provides this pattern with type-safe naming, readable ToString output, and Enum.HasFlag support while preserving the int storage representation.

---

## Q9. What are compound assignment operators, and how do bitwise compound forms behave differently from arithmetic ones on bitmasks?

**Concepts**
- Arithmetic compound assignment (+=, -=, *=, /=, %=)
- Bitwise compound assignment (|=, &=, ^=, <<=, >>=)
- Left side evaluated once
- Integer division behavior of /= on bitmasks
- Right shift (>>=) vs division (/=) for flag normalization

**Answer**

A compound assignment operator fuses a binary operation with assignment, so x += y is equivalent to x = x + y, but with the guarantee that the left-hand side expression is evaluated only once — relevant when it is an indexer or property. C# provides arithmetic compound forms (+=, -=, *=, /=, %=) and bitwise compound forms (|=, &=, ^=, <<=, >>=). The bitwise variants are the idiomatic way to manipulate flags in place: role |= Admin adds the Admin flag, role &= ~Write removes Write, role ^= Feature toggles Feature, and mask <<= 1 shifts the mask left. A critical subtlety is that /= on integer types performs integer division — currentRole /= 2 on a bitmask does not produce the same result as currentRole >>= 1 for all signed values, and it can corrupt unrelated permission bits by treating the mask as an arbitrary number to divide. When a "normalize after a left shift" comment appears in code, the semantically correct operator is >>= 1, not /= 2. Similarly, %= on a bitmask is almost never meaningful. Because compound assignments are expressions, they can appear inside larger expressions, though doing so usually obscures intent. Favor explicit two-step operations (read, compute, assign) in flag-management code to make privilege changes auditable and understandable to reviewers.

---

## Q10. What is the checked context in C#, what overflow behavior does it change, and when should it be used?

**Concepts**
- Default unchecked context — silent wrap-around on overflow
- checked expression — throws OverflowException on overflow
- checked block — applies to all integer arithmetic within
- CheckForOverflowUnderflow project property
- Numeric promotion to long or decimal as alternative

**Answer**

By default, integer arithmetic in C# runs in an unchecked context: when the mathematical result exceeds the type's range, the extra bits are discarded and the value wraps around silently without any exception. Adding 1 to int.MaxValue (2,147,483,647) yields int.MinValue (-2,147,483,648) because the carry bit overflows mod 2³². This silent wrap is the source of an entire class of production bugs — inventory counts, financial totals, and security-sensitive computations that silently produce plausible-looking but wrong values that pass downstream validation checks. The checked keyword forces an OverflowException when integer arithmetic overflows: checked(shippedCases * unitsPerCase) throws instead of returning a garbage value. You can apply checked to a single expression or wrap multiple statements in a checked block. At the project level, adding <CheckForOverflowUnderflow>true</CheckForOverflowUnderflow> to the .csproj in .NET 10 enables checked behavior globally, and the unchecked keyword opts specific performance-critical paths back out. An alternative to checked is numeric promotion: (long)shippedCases * unitsPerCase widens before multiplying, eliminating overflow at the cost of a wider intermediate type. Use checked or long whenever operands come from external data, user input, or batch imports that might contain extreme values. Catching an OverflowException and failing fast is vastly safer than persisting a wrapped value into a ledger or inventory record.

---

## Q11. How do the is and as operators differ in C#, and when should each be used?

**Concepts**
- is — type compatibility test, returns bool
- as — safe reference cast, returns null on failure
- Type pattern declaration (is Type variable)
- No exception on is/as failure (vs direct cast)
- Prefer modern is pattern over as + null check

**Answer**

The is operator tests whether an object is compatible with a given type and returns bool without throwing an exception on failure: obj is string returns true if obj can be treated as a string. Modern C# extends is with pattern matching: if (obj is string s) both tests the type and binds a correctly typed variable s within the branch, eliminating a separate cast and guaranteeing that the variable is only in scope when the test succeeded. The as operator attempts a safe cast to a reference type or nullable value type and returns null if the cast fails rather than throwing an InvalidCastException — the way a direct cast (string)obj would. Because as silently returns null, the result must always be null-checked before use; forgetting the check causes a NullReferenceException that is harder to trace than the original type mismatch. The is pattern form is safer precisely because it cannot be used without verifying the result: the variable is scoped to the true branch, so no null check can be forgotten at the call site. Neither is nor as works on non-nullable value types because value types cannot be null; int cannot be the target of as, and testing a boxed value requires unboxing through a direct cast or a nullable: obj is int i. For modern C# codebases, prefer the is pattern form for all runtime type checks and reserve as for interop code or APIs that predate pattern matching.

---

## Q12. How does string == comparison work in C# and how does it differ from reference equality for other types?

**Concepts**
- Operator overloading of == on System.String
- Content comparison (value semantics for strings)
- Reference equality for non-overloaded types
- String interning for literal constants
- Object.ReferenceEquals for identity comparison

**Answer**

For most reference types, the == operator compares object references — two variables are equal only when they point to the exact same object in memory. System.String overloads == to compare character content instead, giving strings value-like equality semantics. Two distinct string objects with identical text compare as equal: string a = "SKU-100"; string b = "SKU-" + "100"; a == b returns true even though b may be a newly allocated object produced by concatenation. This means business logic like sku1 == sku2 works correctly for content comparison without calling String.Equals explicitly. String interning further blurs the distinction for literal constants: the compiler interns string literals so that two occurrences of "SKU-100" in source code typically share the same object, making Object.ReferenceEquals also return true for them. However, strings computed at runtime — from user input, database reads, or string manipulation — are not interned and will have different references even when the text is identical. Best practice is to rely on == for content equality in business logic, reserve Object.ReferenceEquals for the rare case where reference identity is genuinely needed, and use StringComparison overloads of String.Equals for culture-aware or case-insensitive comparisons. For custom classes, overloading == requires also overriding GetHashCode and Equals to maintain consistency with the equality contract.

---

## Q13. What is operator associativity and how does it affect expressions like a = b = 5 and nested ternary chains?

**Concepts**
- Left-to-right associativity (most binary operators)
- Right-to-left associativity (assignment, ternary ?:, ??)
- Chained assignment evaluation order
- Nested ternary else-branch binding
- Readability implications of right-associativity

**Answer**

When two operators of the same precedence level appear in sequence, associativity determines which one groups first. Most binary operators in C# are left-associative: 10 - 3 - 2 is parsed as (10 - 3) - 2 = 5, not 10 - (3 - 2) = 9. Assignment and its compound forms are right-associative: a = b = 5 is parsed as a = (b = 5), which assigns 5 to b first and then assigns that result to a — making chained initialization of multiple variables to the same value both legal and idiomatic. The ternary operator ?: is also right-associative, so a ? b : c ? d : e parses as a ? b : (c ? d : e) — the inner ternary binds the else clause of the outer one, not its own separate expression. This right-to-left nesting is a common source of misreading during code review, because developers who scan left-to-right tend to group the operators differently than the compiler does. The null-coalescing operator ?? is likewise right-associative: a ?? b ?? c parses as a ?? (b ?? c), which still produces the first non-null value reading left to right because of how ?? short-circuits. In practice, associativity rarely produces surprising results on its own, but being explicit with parentheses in chained assignments and nested ternaries is the safest approach for code that will be reviewed and maintained over time.

---

## Q14. What is the modulo operator (%) in C# and how does it behave with negative integer operands?

**Concepts**
- Integer remainder after division
- Truncation-toward-zero rule
- Sign of result matches dividend sign
- Difference from mathematical modulo
- Wrapping-index pattern for negative inputs

**Answer**

The modulo operator % computes the remainder after integer division. For positive operands the result is intuitive: 25 % 12 = 1 because 25 = 2 × 12 + 1. With negative operands, C# applies the same truncation-toward-zero rule that governs the / operator: the sign of the remainder matches the sign of the dividend (the left operand), not the divisor. So -7 % 3 yields -1, not 2, because -7 = (−2) × 3 + (−1) — dividing toward zero produces a quotient of −2 and a remainder of −1. This differs from mathematical modulo, which always returns a non-negative result, and from Python's %, which follows floor division. The practical impact appears in circular-buffer and wrapping-index idioms: index % count works correctly only for non-negative index values. When the index can be negative — for example when stepping backward through a circular list — the correct expression is (index % count + count) % count, which guarantees a non-negative result regardless of the sign of index. In business scenarios, modulo is used for splitting quantities into whole packs and remainders, computing pagination offsets, and even/odd testing. All of these cases should validate inputs as non-negative before relying on the simple n % count form, to avoid surprising negative remainders entering downstream logic.

---

# Gotchas

---

## Q15. Why does 7 / 2 evaluate to 3 instead of 3.5 in C#, and what is the simplest fix when a fractional result is needed?

**Concepts**
- Integer division determined by operand types, not result variable
- Truncation before assignment to double or decimal
- Literal suffix (.0, m) to change operand type
- Explicit cast to change a variable's type at the division site
- Percentage and rate calculations as common victims

**Answer**

The / operator performs integer division when both operands are integer types, discarding the fractional part by truncating toward zero. No error or warning is emitted — the code compiles and runs, producing 3 from 7 / 2. Developers are most often surprised by this when they assign the result to a double or decimal, expecting widening to produce 3.5: int a = 7; int b = 2; double result = a / b evaluates the division as int / int first, yielding 3, and then widens 3 to 3.0. The assignment target type is irrelevant; the operand types at the division site are what matter. The simplest fixes are to use a literal suffix (7.0 / 2 or 7.0m / 2) or to cast one operand before dividing: (double)a / b. Percentage calculations are the most common victim: decimal rate = discountPercent / 100 produces 0.0m when discountPercent is 5 because 5 / 100 is 0 in integer arithmetic; the correct expression is discountPercent / 100m. Similarly, computing a fill percentage as int filled = 3; int capacity = 7; double pct = filled / capacity * 100 gives 0.0, not 42.857, because the division floors to 0 before the multiplication. The rule to remember is: look at the types of both operands at the / operator, not at the variable receiving the result.

---

## Q16. What is the danger of using & instead of && in a boolean guard expression that contains side-effecting methods?

**Concepts**
- & on bool — both sides always evaluate (no short-circuit)
- && on bool — right side skipped when left is false
- Side effects in guard conditions (mutations, DB calls, logging)
- Validate-then-mutate ordering
- Production state corruption from non-short-circuit evaluation

**Answer**

When & is used between two bool sub-expressions, C# evaluates both sides regardless of the left operand's value because the single-character & operator does not short-circuit. This means the right-hand side executes even when the left side is already false and the combined result is predetermined. In a guard like if (creditService.IsApproved(customerId) & inventoryService.TryReserve(sku, qty)), the inventory is reserved on every call — including those where credit is denied — because & forces both operands to evaluate unconditionally. With &&, the inventory reservation would only run when credit was approved. The real-world consequences are serious: stock decrements, audit log entries, and charge attempts happen on orders that should have been rejected at the first check. The bug is visually subtle — a single-character difference between & and && — and it compiles without any warning or diagnostic. The rule is to always use && (and ||) for boolean logic in application code unless you have an explicit, documented requirement for both sides to execute for their side effects, which is rare. Put the cheapest, most restrictive check on the left side of && so expensive or mutating operations on the right side are skipped as early as possible. If both operations genuinely must run (for audit completeness, for instance), separate them into explicit statements before the condition rather than combining them inside a single boolean expression.

---

## Q17. Why does code?.Length ?? 0 + 1 not produce "the string length, or 1 when null" in C#?

**Concepts**
- ?? precedence lower than additive (+, -)
- Actual parsing: code?.Length ?? (0 + 1)
- Non-null path misses the +1 increment
- Parentheses enforce intended grouping
- Null path vs non-null path asymmetry

**Answer**

The null-coalescing operator ?? has lower precedence than the additive operators + and -, so code?.Length ?? 0 + 1 is parsed by the compiler as code?.Length ?? (0 + 1), not as (code?.Length ?? 0) + 1. In the null path, code is null, so ?? returns its right operand: 0 + 1 = 1. This matches the developer's intent for the null case. But in the non-null path, code has a value, so ?? short-circuits and returns code.Length directly, without adding 1 at all. The two paths behave differently — the + 1 is silently applied to one path and silently dropped from the other. The fix is mechanical: add parentheses around the null-coalescing sub-expression before applying any arithmetic: (code?.Length ?? 0) + 1. If the intent is simply "length if present, else 1 as a total," then code?.Length ?? 1 is the cleaner expression. This precedence behavior is consistent with how the language specification defines all null-coalescing expressions, and the same trap appears any time ?? is mixed with +, -, *, or / without parentheses. The only reliable way to detect it through testing is to write test cases that cover the non-null path and assert the expected value after the arithmetic — tests that only cover the null path miss the bug entirely because the null path returns the correct answer by coincidence.

---

## Q18. How does integer overflow silently corrupt arithmetic in C# by default, and what tools prevent it?

**Concepts**
- Default unchecked context — wrap-around without exception
- int.MaxValue overflow to int.MinValue
- checked expression and checked block
- CheckForOverflowUnderflow project property
- Numeric promotion to long or decimal as alternative

**Answer**

C# integer arithmetic runs in an unchecked context by default, meaning that when a result exceeds the type's range, the extra bits are discarded and the value wraps around without any error. Multiplying 500_000 * 10_000 overflows int.MaxValue (approximately 2.1 billion) and wraps to a plausible-looking positive or negative number, depending on the bit pattern. The code compiles and runs normally, and the corrupted value may pass business validation — it is still a valid int — before reaching a database, a financial ledger, or an inventory record. The checked keyword adds overflow detection: checked(x * y) throws an OverflowException when the product overflows. A checked block wraps multiple integer arithmetic statements with the same behavior. The project-level property <CheckForOverflowUnderflow>true</CheckForOverflowUnderflow> in the .csproj enables checked behavior globally in .NET 10 projects, and the unchecked { } block opts specific hot paths back out. The alternative is numeric promotion: (long)shippedCases * unitsPerCase widens before multiplying, eliminating overflow entirely for values up to long.MaxValue. For production systems that process external data, the defensive practice is to validate numeric inputs for plausible range before arithmetic, use checked or long for any multiplication or addition that could plausibly approach the type's limits, and treat an OverflowException as a hard rejection signal rather than attempting to recover and continue with a corrupted value.

---

## Q19. Why is ^= (XOR-assign) the wrong operator for unconditionally removing a permission flag, and what should replace it?

**Concepts**
- XOR toggle — adds flag if absent, removes if present
- AND-NOT pattern (role &= ~Flag) — unconditional clear
- Idempotency requirement for revoke operations
- Bitwise NOT (~) to create clearing mask
- Security regression from toggling vs clearing

**Answer**

XOR sets a result bit to 1 when exactly one of the two input bits is 1, and to 0 when both are 1 or both are 0. This toggle behavior means role ^= Write removes the Write flag when it is currently set but adds it when it is currently absent. A "revoke write access" function that uses ^= is therefore unreliable: calling it once on a user who has Write removes it correctly, but calling it again silently re-grants Write, and calling it on a user who never had Write silently grants it for the first time. Neither outcome is the intent of a revoke operation. The correct pattern for unconditional flag removal is AND-NOT: role &= ~Write. The ~ operator inverts all bits of Write, producing a mask with every bit set except the Write bit. ANDing the current role against that mask clears the Write bit regardless of whether it was set before, and leaves all other bits unchanged. This operation is idempotent — calling role &= ~Write any number of times produces the same result as calling it once. The distinction matters for security: a permission revoke endpoint that toggles instead of clears is a security regression, allowing privilege re-escalation on repeated API calls. When writing bitwise flag manipulation, always document whether the intent is toggle (^=) or unconditional clear (&= ~Flag) and choose the operator family accordingly.

---

# Real-World Scenarios

---

## Q20. (Code Review) A developer submits this daily-reporting helper to compute the average order value. Review the code and identify all operator and type issues before it ships.

```csharp
// Returns average order value including tax for the daily summary
public static decimal ComputeAverageOrderValue(List<Order> orders, decimal taxRate)
{
    if (orders == null || orders.Count == 0)
        return 0;

    int totalRevenue = 0;
    foreach (var order in orders)
        totalRevenue += (int)order.LineTotal;

    decimal average  = totalRevenue / orders.Count;
    decimal withTax  = average + average * taxRate;
    return Math.Round(withTax, 2);
}
```

**Concepts**
- Decimal-to-int cast truncating cents
- Integer accumulator overflow risk
- Integer division of sum by count
- Precision loss before Math.Round
- Arithmetic precedence in tax expression

**Answer**

Three compounding operator issues corrupt the result before it reaches Math.Round.

| Category | Problem | Impact |
|---|---|---|
| Correctness | `(int)order.LineTotal` truncates every order's cents before summing | Accumulated revenue is understated by up to $0.99 per order — systematic underreporting |
| Correctness | `totalRevenue / orders.Count` is int / int — integer division floors the average | Average always truncates toward zero; never correct to cents |
| Overflow | `int totalRevenue` accumulates large decimal values cast to int; high-volume reports exceed int.MaxValue | Silent wrap produces negative or wildly wrong averages on large datasets |
| Precedence | `average + average * taxRate` — * binds before +, computing `average * (1 + taxRate)` in two multiplications | Precedence is correct; no bug, but single-multiplication form `average * (1m + taxRate)` is clearer |

**Fix (priority order):**

1. Replace `int totalRevenue = 0` with `decimal totalRevenue = 0m` to preserve cent precision throughout accumulation.
2. Remove the `(int)` cast — assign `totalRevenue += order.LineTotal` directly.
3. Decimal division now happens automatically: `decimal average = totalRevenue / orders.Count` promotes int to decimal.
4. Replace `average + average * taxRate` with `average * (1m + taxRate)` for a single multiplication and explicit grouping.

```csharp
public static decimal ComputeAverageOrderValue(List<Order> orders, decimal taxRate)
{
    if (orders == null || orders.Count == 0)
        return 0m;

    decimal totalRevenue = 0m;
    foreach (var order in orders)
        totalRevenue += order.LineTotal;

    decimal average = totalRevenue / orders.Count;
    return Math.Round(average * (1m + taxRate), 2);
}
```

---

## Q21. (Code Review) A team member submits this user-preference update method for a .NET 10 SaaS application. What operator issues would you flag before merging?

```csharp
public static int ApplyUserPreferences(
    int  currentFlags,
    bool enableNotifications,
    bool enableDarkMode)
{
    const int NotificationFlag = 1 << 3;
    const int DarkModeFlag     = 1 << 4;

    if (enableNotifications)
        currentFlags ^= NotificationFlag;

    if (enableDarkMode)
        currentFlags ^= DarkModeFlag;

    return currentFlags;
}
```

**Concepts**
- XOR-assign (^=) as toggle, not unconditional grant
- |= for idempotent flag grant
- &= ~Flag for idempotent flag revoke
- Idempotency requirement for preference application
- Method name "Apply" implies deterministic grant semantics

**Answer**

The method uses ^= for what the caller expects to be a deterministic enable operation, but ^= toggles. Calling ApplyUserPreferences(flags, enableNotifications: true, enableDarkMode: false) a second time disables notifications if they were enabled by the first call, because XOR flips the bit from 1 back to 0. A user who saves preferences twice on repeated logins ends up with the opposite of their last intent.

| Category | Problem | Impact |
|---|---|---|
| Correctness | `^= NotificationFlag` toggles — re-applying enableNotifications=true disables an already-enabled flag | Repeated preference saves alternate the feature between on and off silently |
| Correctness | Same toggle issue for `^= DarkModeFlag` | Dark mode cycles with each settings save |
| Design | Method name "Apply" implies idempotent grant semantics, not toggle | Callers cannot predict outcomes without knowing current flag state, defeating the point of a preference API |

**Fix (priority order):**

1. Use `|=` to unconditionally set a flag when the `enable*` parameter is true.
2. Use `&= ~Flag` to unconditionally clear a flag when the parameter is false.
3. Consider separate `EnableFeature` / `DisableFeature` overloads to make call-site intent explicit.

```csharp
if (enableNotifications) currentFlags |= NotificationFlag;
else                      currentFlags &= ~NotificationFlag;

if (enableDarkMode) currentFlags |= DarkModeFlag;
else                currentFlags &= ~DarkModeFlag;
```

---

## Q22. (Code Review) A shipping-cost helper returns wrong surcharges for zones with an explicit base rate. Identify the precedence issues before the method ships.

```csharp
public static decimal GetShippingCost(Order? order, ShippingZone? zone)
{
    decimal baseRate  = zone?.BaseRate ?? 0.0m + 5.0m;
    int     itemCount = order?.TotalItems ?? 0 + 1;
    decimal perItem   = 1.50m;
    return baseRate + itemCount * perItem;
}
```

**Concepts**
- ?? precedence lower than additive (+)
- Parsing: zone?.BaseRate ?? (0.0m + 5.0m)
- Non-null path receives no surcharge
- Parentheses for null-coalescing grouping
- Asymmetric behavior between null and non-null paths

**Answer**

Both null-coalescing expressions are parsed incorrectly because ?? binds looser than the additive + operator.

| Category | Problem | Impact |
|---|---|---|
| Precedence | `zone?.BaseRate ?? 0.0m + 5.0m` parses as `zone?.BaseRate ?? (0.0m + 5.0m)` | Non-null zones use only BaseRate with no +5.0m surcharge; null zones get 5.0m — the handling surcharge disappears for all real zones |
| Precedence | `order?.TotalItems ?? 0 + 1` parses as `order?.TotalItems ?? (0 + 1)` | Non-null orders return TotalItems without +1; null orders return 1 — every valid order is undercharged by one item slot |
| Design | Magic `+ 5.0m` and `+ 1` are undocumented offsets with no named constants | Intent is unclear; future developers cannot tell whether the offsets apply to both paths or only the null path |

**Fix (priority order):**

1. Parenthesize each null-coalescing sub-expression before applying the offset: `(zone?.BaseRate ?? 0.0m) + 5.0m` and `(order?.TotalItems ?? 0) + 1`.
2. Extract named constants: `const decimal HandlingSurcharge = 5.0m;` and ensure the surcharge is documented as a business rule that applies whether or not a zone has an explicit rate.
3. Add unit tests for all four combinations: null zone, non-null zone, null order, non-null order — asserting the surcharge and item adjustment appear in every applicable case.

```csharp
public static decimal GetShippingCost(Order? order, ShippingZone? zone)
{
    const decimal HandlingSurcharge = 5.0m;
    decimal baseRate  = (zone?.BaseRate ?? 0.0m) + HandlingSurcharge;
    int     itemCount = (order?.TotalItems ?? 0) + 1;
    return baseRate + itemCount * 1.50m;
}
```

---

## Q23. Your team needs a role-based permission system for a .NET 10 warehouse picker application. Roles must combine Read, Write, Delete, and Admin capabilities in a single int stored as a database column, and support grant, revoke, and test operations at runtime. How would you design and implement it using bitwise operators?

**Concepts**
- Powers-of-two flag constants (1 << n)
- [Flags] enum for type-safe naming and ToString
- Bitwise OR for granting (|=)
- Bitwise AND-NOT for revoking (&= ~Flag)
- Bitwise AND for testing ((role & Flag) != 0)

**Answer**

The core insight is that each permission can be represented as a single bit position in an int, allowing any combination of permissions to be stored in one field with no additional columns or join tables. Define an enum decorated with [Flags] using powers of two as values, so each capability occupies a distinct non-overlapping bit:

```csharp
[Flags]
public enum PickerPermission
{
    None   = 0,
    Read   = 1 << 0,  // 1
    Write  = 1 << 1,  // 2
    Delete = 1 << 2,  // 4
    Admin  = 1 << 3   // 8
}
```

With this foundation, the three operations follow directly from the bitwise patterns: grant with bitwise OR (`role |= PickerPermission.Write` adds Write without touching other bits), test with bitwise AND (`(role & PickerPermission.Delete) != 0` returns true only when the Delete bit is set), and revoke with AND-NOT (`role &= ~PickerPermission.Write` clears Write unconditionally regardless of current state). The [Flags] attribute gives the enum a ToString that prints "Read, Write" instead of "3" and enables the Enum.HasFlag method as a readable alternative to the & != 0 pattern. For database storage, map the enum to an int or smallint column — EF Core handles this automatically. When adding new capabilities, always use the next unused power of two; never insert a new flag between existing values or renumber them, because that would corrupt all stored role masks. An int supports up to 31 non-overlapping flags, which is sufficient for typical warehouse role systems. Document the bit assignments as comments in the enum definition so that any developer reading the database schema can cross-reference them.

---

## Q24. Your team's LINQ projection crashes with NullReferenceException when processing orders from inactive customers whose Address or PostalCode property is null. How would you use null-conditional and null-coalescing operators to make the projection safe without adding if-chains or defensive helper methods?

**Concepts**
- Null-conditional chaining (?.) for nested property access
- Null-coalescing (??) for fallback values in projections
- LINQ Select projection context (expression, not statement)
- Short-circuit propagation through property chain
- Nullable return type from null-conditional chain

**Answer**

The crash occurs because direct property navigation like order.Customer.Address.PostalCode throws a NullReferenceException at the first null link in the chain — any of Customer, Address, or PostalCode being null causes the expression to fail. LINQ Select projections evaluate this path on every element, so the first inactive customer with a null Address terminates the entire query. The ?. operator provides a compile-safe traversal: order.Customer?.Address?.PostalCode evaluates to null rather than throwing when Customer or Address is null, because each ?. short-circuits the rest of the chain. The result type becomes string? because any link may be null, so combining with ?? provides a documented fallback for the projection output:

```csharp
var summaries = orders.Select(o => new OrderSummary
{
    OrderId      = o.Id,
    CustomerName = o.Customer?.Name ?? "Guest",
    PostalCode   = o.Customer?.Address?.PostalCode ?? "N/A",
    Region       = o.Customer?.Address?.Region?.Code ?? "DEFAULT",
    TotalItems   = o.TotalItems
});
```

The key discipline is to always end a ?. chain with ?? when the consuming code expects a non-nullable string — leaving the result as string? compiles but may propagate null further into rendering or serialization logic where a NullReferenceException is harder to trace. When the business logic genuinely needs to distinguish "no address on file" from "address present but field blank," use a conditional expression to separate those cases rather than collapsing both into a single ?? default. For deeply nested chains, extracting a well-named local variable (string? postalCode = o.Customer?.Address?.PostalCode;) before the projection makes the chain visible in isolation and easier to unit test.

---

## Q25. You are reviewing a pull request where a developer computed tiered discounts using only int literals and variables throughout. The unit tests pass with round-number inputs but fail with real order values. How do you identify and fix the operator and data-type issues?

**Concepts**
- Integer division determined by operand types at the division site
- Decimal suffix (m) on denominator to trigger promotion
- Explicit cast before division on variable operands
- Testing with fractional inputs to expose integer-division bugs
- Percentage computation operand order

**Answer**

When all literals and variables are typed as int, any division that should produce a fractional rate silently floors to zero. A discount rate of five percent written as int discountPercent = 5; decimal discount = discountPercent / 100 * lineTotal computes 5 / 100 = 0 in integer arithmetic before the result is ever widened, so the discount is always zero regardless of lineTotal. Unit tests that use multiples of 100 as inputs (for example, a 10% discount on an order of 200 units) never hit a division that floors to zero, which is why they pass. The fix is to ensure the denominator is decimal at the division site: discountPercent / 100m promotes discountPercent to decimal before dividing, yielding 0.05m. A second common variant is a fill-rate percentage: int filled = 3; int capacity = 7; double rate = filled / capacity * 100 gives 0.0 because filled / capacity floors to 0 before the multiplication. The correct form is (double)filled / capacity * 100, casting before the division. The review checklist for a PR with arithmetic that involves rates or percentages is: locate every / operator; inspect the declared types of both operands at that exact line (not the receiving variable); if both operands are integral types and the intent requires a fractional result, add a cast or a literal suffix on the denominator. Writing test cases with values that produce non-zero remainders under integer division — input pairs like 1 and 3, or 7 and 10 — alongside the round-number cases is the fastest way to expose this class of bug before it reaches production.
