# C# Arrays — Interview Q&A


## Table of Contents

1. [Q1. What is an array in C# and how does its contiguous memory layout affect performance?](#q1-what-is-an-array-in-c-and-how-does-its-contiguous-memory-layout-affect-performance)
2. [Q2. What are the different ways to declare and initialize a single-dimensional array in C#?](#q2-what-are-the-different-ways-to-declare-and-initialize-a-single-dimensional-array-in-c)
3. [Q3. What is zero-based indexing, what is `IndexOutOfRangeException`, and how do you guard against it?](#q3-what-is-zero-based-indexing-what-is-indexoutofrangeexception-and-how-do-you-guard-against-it)
4. [Q4. When should you use a `for` loop versus `foreach` to iterate an array?](#q4-when-should-you-use-a-for-loop-versus-foreach-to-iterate-an-array)
5. [Q5. How do you declare and work with rectangular two-dimensional arrays in C#?](#q5-how-do-you-declare-and-work-with-rectangular-two-dimensional-arrays-in-c)
6. [Q6. What is a jagged array and when would you choose it over a rectangular array?](#q6-what-is-a-jagged-array-and-when-would-you-choose-it-over-a-rectangular-array)
7. [Q7. How do `Array.Sort` and `Array.BinarySearch` work, and what is the prerequisite for `BinarySearch`?](#q7-how-do-arraysort-and-arraybinarysearch-work-and-what-is-the-prerequisite-for-binarysearch)
8. [Q8. What does `Array.Copy` do, how does it differ from `Clone`, and when would you use each?](#q8-what-does-arraycopy-do-how-does-it-differ-from-clone-and-when-would-you-use-each)
9. [Q9. What does `Array.Resize` do and how does the `ref` parameter affect the calling code?](#q9-what-does-arrayresize-do-and-how-does-the-ref-parameter-affect-the-calling-code)
10. [Q10. How do `Array.Clear` and `Array.Fill` work, and what values do they write?](#q10-how-do-arrayclear-and-arrayfill-work-and-what-values-do-they-write)
11. [Q11. What is `Span<T>` and how does it provide a zero-allocation view over an array?](#q11-what-is-spant-and-how-does-it-provide-a-zero-allocation-view-over-an-array)
12. [Q12. What is array covariance in C# and why can it cause a runtime exception?](#q12-what-is-array-covariance-in-c-and-why-can-it-cause-a-runtime-exception)
13. [Q13. What is a fixed-size buffer in C# and when would you use one?](#q13-what-is-a-fixed-size-buffer-in-c-and-when-would-you-use-one)
14. [Q14. How does passing an array to a method work in C#, and what is the difference between modifying elements versus reassigning the parameter?](#q14-how-does-passing-an-array-to-a-method-work-in-c-and-what-is-the-difference-between-modifying-elements-versus-reassigning-the-parameter)
15. [Q15. What is the `params` keyword and how does the compiler handle a params array parameter?](#q15-what-is-the-params-keyword-and-how-does-the-compiler-handle-a-params-array-parameter)
16. [Q16. What is the off-by-one bug with `i <= array.Length` and why does the compiler not catch it?](#q16-what-is-the-off-by-one-bug-with-i-arraylength-and-why-does-the-compiler-not-catch-it)
17. [Q17. Why does assigning to a `foreach` iteration variable not update the array element?](#q17-why-does-assigning-to-a-foreach-iteration-variable-not-update-the-array-element)
18. [Q18. Why does `Array.BinarySearch` give wrong results on an unsorted array?](#q18-why-does-arraybinarysearch-give-wrong-results-on-an-unsorted-array)
19. [Q19. What is the `ArrayTypeMismatchException` covariance trap and how do you avoid it?](#q19-what-is-the-arraytypemismatchexception-covariance-trap-and-how-do-you-avoid-it)
20. [Q20. Why does `Clone` on an array of reference types not produce an independent copy?](#q20-why-does-clone-on-an-array-of-reference-types-not-produce-an-independent-copy)
21. [Q21. Code Review — Exam score averager throws intermittently in production](#q21-code-review-exam-score-averager-throws-intermittently-in-production)
22. [Q22. Code Review — Pricing service corrupts the audit log on every request](#q22-code-review-pricing-service-corrupts-the-audit-log-on-every-request)
23. [Q23. Code Review — Pass-rate normaliser fails silently on failing scores](#q23-code-review-pass-rate-normaliser-fails-silently-on-failing-scores)
24. [Q24. Code Review — Report builder fails to compile after partial migration from arrays to `List<T>`](#q24-code-review-report-builder-fails-to-compile-after-partial-migration-from-arrays-to-listt)
25. [Q25. Scenario — Processing a large binary protocol buffer without heap allocation](#q25-scenario-processing-a-large-binary-protocol-buffer-without-heap-allocation)
26. [Q26. Scenario — Sorting students by score while keeping names and scores aligned](#q26-scenario-sorting-students-by-score-while-keeping-names-and-scores-aligned)

---
> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/09. Arrays - Done`
> **Topics:** Single/multi-dimensional arrays, jagged arrays, Array class methods (Sort, BinarySearch, Copy, Resize), array covariance, fixed-size buffers, array initialization, bounds checking, Span\<T\> and Memory\<T\> as array views, passing arrays to methods.

---

## Foundation Questions

---

## Q1. What is an array in C# and how does its contiguous memory layout affect performance?

**Concepts**
- Fixed-size collection of same-type elements
- Contiguous heap allocation
- Zero-based index access
- Cache-friendly sequential reads
- Reference type semantics

**Answer**

An array is a fixed-size, ordered collection of elements of the same type allocated as a single contiguous block on the managed heap. Because all elements sit in adjacent memory addresses, the CPU cache line loads a full neighbourhood of elements on the first access, meaning sequential reads — a tight `for` loop summing integers, for example — rarely pay a cache miss penalty after the first fetch. This is called spatial locality, and it makes raw arrays faster than linked structures for bulk numeric processing.

The type of an array variable is reference: the variable holds a reference to the heap block, not the elements themselves. Two variables can point at the same array, and passing an array to a method shares that reference rather than copying the data. Arrays inherit from `System.Array` and implement `IList<T>`, `ICollection<T>`, and `IEnumerable<T>`, so they work with LINQ and any API that accepts these interfaces. The size is fixed at creation — there is no built-in mechanism to grow or shrink the array without allocating a fresh one.

```csharp
// net10.0 — contiguous int array, O(1) index, sequential iteration is cache-friendly
int[] primes = [2, 3, 5, 7, 11];       // collection expression (C# 12+)
Console.WriteLine(primes[2]);           // 5  — O(1) random access
Console.WriteLine(primes.Length);       // 5  — property, not a method
```

---

## Q2. What are the different ways to declare and initialize a single-dimensional array in C#?

**Concepts**
- Declaration without initialization
- `new T[n]` allocation with default values
- Collection initializer shorthand
- Collection expressions (C# 12)
- Default value per element type

**Answer**

There are four common patterns. Declaration-only (`int[] scores;`) creates a null reference — the array cannot be used until it is assigned. Allocation with size (`new int[5]`) creates five slots each initialized to the type default: `0` for numeric types, `false` for bool, `'\0'` for char, and `null` for reference types. The collection initializer (`new int[] { 88, 92, 76 }`) allocates and fills in one expression; the shorthand form (`int[] scores = { 88, 92, 76 }`) works when the type is clear from context. C# 12 collection expressions (`int[] scores = [88, 92, 76]`) are syntactically cleaner and work with any collection type, not just arrays.

Picking the right form matters in practice. Declaring then assigning allows late initialization. Allocating with a size is appropriate when the count is known but values are filled in a loop. The initializer form is best when values are constants known at compile time.

```csharp
// net10.0 — four initialization patterns
int[] a = new int[5];                   // five zeros
int[] b = new int[] { 88, 92, 76 };     // explicit type in initializer
int[] c = { 88, 92, 76 };              // shorthand — type inferred from declaration
int[] d = [88, 92, 76];                // C# 12 collection expression — preferred modern form

string[] names = new string[3];         // three nulls — reference type default
bool[] flags = new bool[4];             // four false values
```

---

## Q3. What is zero-based indexing, what is `IndexOutOfRangeException`, and how do you guard against it?

**Concepts**
- Zero-based index origin
- Valid index range: 0 to Length − 1
- `IndexOutOfRangeException` at runtime, not compile time
- Index guard pattern
- No negative index support in C# arrays

**Answer**

C# arrays are zero-based: the first element is at index 0, the last at `Length - 1`. A length-5 array has valid indices 0, 1, 2, 3, and 4. Accessing index 5 or any negative index throws `IndexOutOfRangeException` at runtime — the compiler does not catch index mistakes except in trivially obvious cases. This is one of the most common bugs in systems that receive an index from user input, a search result, or an external service.

The defensive pattern is to verify the index before use: `if (index >= 0 && index < array.Length)`. Alternatively, `^` (index from end) syntax provides a safe way to refer to the last element: `array[^1]` is equivalent to `array[array.Length - 1]` but the compiler desugars it safely. For scenarios where out-of-range access should return a default rather than throw, use a `Span<T>` slice or a helper method with an explicit guard.

```csharp
// net10.0
int[] scores = [88, 92, 76, 85, 90];

Console.WriteLine(scores[0]);           // 88 — first element
Console.WriteLine(scores[^1]);          // 90 — last element via index-from-end
Console.WriteLine(scores[4]);           // 90 — explicit last index (Length - 1)

// scores[5];                           // IndexOutOfRangeException at runtime

// Guard when index comes from outside
static int SafeGet(int[] arr, int index) =>
    index >= 0 && index < arr.Length ? arr[index] : -1;
```

---

## Q4. When should you use a `for` loop versus `foreach` to iterate an array?

**Concepts**
- `for` exposes index — required for mutation and parallel arrays
- `foreach` iteration variable is read-only
- `foreach` works on any `IEnumerable`
- Do not modify the array length during `foreach`
- `for` required for index-based write access

**Answer**

`foreach` is the idiomatic choice when you only need to read each element in order. It is concise, eliminates index variables, and reduces off-by-one errors. However, the iteration variable is a read-only copy of the element — assigning to it inside the loop does not write back to the array; that produces a compile error (CS1654). You also cannot resize or remove elements from the array during a `foreach` without undefined behavior.

Use `for` when you need the index itself — to pair two parallel arrays by position, to mutate slots in place, to traverse in reverse, or to skip elements. The loop bound must be `i < array.Length`, not `i <= array.Length`; the off-by-one with `<=` is the single most common array bug and always causes `IndexOutOfRangeException` on the final iteration.

```csharp
// net10.0
int[] scores = [88, 92, 76, 85, 90];

// foreach — read-only traversal
int total = 0;
foreach (int s in scores) total += s;

// for — mutate slots in place
for (int i = 0; i < scores.Length; i++)
    if (scores[i] < 50) scores[i] = 50;  // floor failing scores

// Parallel arrays — index required
string[] subjects = ["Math", "Physics", "CS", "English", "Chemistry"];
for (int i = 0; i < subjects.Length; i++)
    Console.WriteLine($"{subjects[i]}: {scores[i]}");
```

---

## Q5. How do you declare and work with rectangular two-dimensional arrays in C#?

**Concepts**
- `int[,]` syntax — comma inside brackets
- `GetLength(0)` for rows, `GetLength(1)` for columns
- `Rank` property — number of dimensions
- `Length` — total element count (rows × columns)
- `foreach` visits in row-major order

**Answer**

A rectangular (two-dimensional) array is declared with a comma inside the type brackets: `int[,] grid = new int[3, 4]`. This allocates a single contiguous block of 12 integers organized as 3 rows and 4 columns. Element access uses a comma between indices: `grid[1, 2]` — row 1, column 2. Using two separate bracket pairs (`grid[1][2]`) is a compile error because rectangular arrays are not arrays of arrays.

Key properties: `grid.GetLength(0)` returns the row count, `grid.GetLength(1)` the column count, `grid.Rank` returns 2, and `grid.Length` returns the total cell count (rows × columns). A `foreach` over a rectangular array visits every cell in row-major (left-to-right, top-to-bottom) order without exposing row or column indices — use a nested `for` when you need both coordinates.

```csharp
// net10.0
int[,] shelf = new int[3, 4]
{
    { 12,  8, 15,  6 },
    {  5, 20, 10, 14 },
    {  9, 11,  7, 18 }
};

Console.WriteLine(shelf.GetLength(0));  // 3  — rows
Console.WriteLine(shelf.GetLength(1));  // 4  — columns
Console.WriteLine(shelf.Rank);          // 2
Console.WriteLine(shelf.Length);        // 12 — total cells

int centerBin = shelf[1, 2];            // 10 — row 1, column 2 (comma, not double brackets)

// Nested for — both coordinates available
for (int r = 0; r < shelf.GetLength(0); r++)
    for (int c = 0; c < shelf.GetLength(1); c++)
        Console.Write($"{shelf[r, c],4}");
```

---

## Q6. What is a jagged array and when would you choose it over a rectangular array?

**Concepts**
- Array of arrays — each inner array is independent
- `T[][]` syntax — two separate bracket pairs
- Inner array length varies per row
- Outer `Length` vs per-row inner `Length`
- Memory layout: outer array holds references, not a single block

**Answer**

A jagged array (`string[][]`) is an array whose elements are themselves arrays. Each inner array is a separate heap object and can have a different length. This differs fundamentally from a rectangular array, which is a single contiguous block where every row must have the same number of columns.

Access uses two separate bracket pairs: `departments[1][0]` selects element 0 of the inner array at outer index 1. `departments.Length` gives the outer row count; `departments[i].Length` gives the length of the inner array at row `i`. If an inner array has not been assigned, that slot is null and `departments[i].Length` throws `NullReferenceException`.

Choose a jagged array when the row lengths vary naturally — a list of department course offerings, adjacency lists in a graph, or rows of a CSV file with optional trailing fields. Choose a rectangular array when every row must have exactly the same number of columns — an image pixel grid, a matrix, or a seating chart. Rectangular arrays are slightly more efficient (single allocation) but less flexible.

```csharp
// net10.0
string[][] departments = new string[3][];
departments[0] = ["ENG-101", "ENG-201"];
departments[1] = ["MAT-100", "MAT-200", "MAT-300"];
departments[2] = ["CS-50"];

Console.WriteLine(departments.Length);      // 3  — outer count
Console.WriteLine(departments[1].Length);   // 3  — inner count at row 1
Console.WriteLine(departments[1][0]);       // MAT-100

// Count all courses safely
int total = 0;
foreach (string[] dept in departments)
    if (dept is not null) total += dept.Length;
Console.WriteLine(total);  // 6
```

---

## Q7. How do `Array.Sort` and `Array.BinarySearch` work, and what is the prerequisite for `BinarySearch`?

**Concepts**
- `Array.Sort` — in-place, ascending by default, uses `IComparable<T>`
- `Array.Sort(keys, items)` — sorts parallel arrays together
- `Array.BinarySearch` — requires a sorted array, O(log n)
- Negative return value when not found — bitwise complement gives insertion point
- Custom comparer via `IComparer<T>` or `Comparison<T>`

**Answer**

`Array.Sort` sorts the array in place using an introspective sort algorithm (combination of quicksort, heapsort, and insertion sort) achieving O(n log n) average performance. Without a comparer it relies on the element type's `IComparable<T>` implementation. A two-argument overload `Array.Sort(keys, values)` sorts both arrays in tandem, keeping parallel elements aligned — essential for parallel arrays like a student name array paired with a score array.

`Array.BinarySearch` requires the array to be already sorted using the same comparer that will be passed to `BinarySearch`. It returns the zero-based index of the found element, or a negative value if not found. The negative value's bitwise complement (`~result`) gives the index where the searched value would be inserted to maintain sorted order. Calling `BinarySearch` on an unsorted array produces undefined — often incorrect but non-throwing — results, which is particularly insidious in production.

```csharp
// net10.0
string[] skus = ["TBK-104", "TBK-220", "TBK-087", "TBK-315", "TBK-150"];

string[] copy = (string[])skus.Clone();
Array.Sort(copy);                           // in-place — modifies copy, not skus
// copy is now: TBK-087, TBK-104, TBK-150, TBK-220, TBK-315

int idx = Array.BinarySearch(copy, "TBK-150");   // 2 — found
int miss = Array.BinarySearch(copy, "TBK-999");  // negative — not found
int insertAt = ~miss;                             // insertion point to keep sorted order
Console.WriteLine($"Found at {idx}, insert missing at {insertAt}");

// Sort parallel arrays together
string[] names = ["Priya", "Alex", "Sam"];
int[] scores   = [92, 88, 76];
Array.Sort(scores, names);   // sorts scores ascending; names moves with them
// scores: 76, 88, 92   names: Sam, Alex, Priya
```

---

## Q8. What does `Array.Copy` do, how does it differ from `Clone`, and when would you use each?

**Concepts**
- `Array.Copy` — bulk element transfer between two existing arrays
- Source/destination offset overloads
- `Clone` — creates a new array of the same type and length
- Shallow copy — reference types share inner objects
- `Array.ConstrainedCopy` for rollback safety

**Answer**

`Array.Copy(source, destination, length)` copies a specified number of elements from one array into another that already exists. An overload `Array.Copy(source, srcIndex, dest, destIndex, length)` allows copying a sub-range from an arbitrary offset in the source to an arbitrary offset in the destination — useful for building output buffers, moving a window of data, or merging segments. If source and destination are the same array, overlapping ranges are handled correctly.

`Clone` creates a new array of the same type and length and copies all elements into it, returning an `object` that you must cast back to the concrete array type: `(int[])original.Clone()`. Both `Copy` and `Clone` perform a shallow copy — for arrays of reference types, the elements inside the new array still point to the same objects as the original. Deep copying requires iterating and cloning each element individually.

Use `Clone` when you want a complete independent copy of a whole array. Use `Copy` when you need to copy a range, copy into a pre-existing destination buffer, or control offsets precisely. `Array.ConstrainedCopy` is a rarely needed variant that rolls back partial copies if an element conversion fails mid-way.

```csharp
// net10.0
int[] source = [1, 2, 3, 4, 5];

// Clone — whole-array shallow copy
int[] cloned = (int[])source.Clone();
cloned[0] = 99;
Console.WriteLine(source[0]);    // 1 — original unchanged

// Copy — range copy into existing destination
int[] dest = new int[7];
Array.Copy(source, 0, dest, 1, 5);   // dest = [0, 1, 2, 3, 4, 5, 0]

// Shallow copy caveat with reference types
string[] origNames = ["Alice", "Bob"];
string[] shallowNames = (string[])origNames.Clone();
// shallowNames[0] and origNames[0] point to the same string object
// (strings are immutable so this is safe; mutable objects would share state)
```

---

## Q9. What does `Array.Resize` do and how does the `ref` parameter affect the calling code?

**Concepts**
- `Array.Resize` allocates a new array and copies existing elements
- `ref` parameter rebinds the caller's variable to the new array
- Truncation when new size is smaller
- Default-filled new slots when new size is larger
- Variable rebinding — callers with other references do not see the change

**Answer**

`Array.Resize(ref T[] array, int newSize)` does not actually resize the existing array — arrays are fixed-size. Instead, it allocates a brand-new array of `newSize`, copies as many elements as fit, and then rebinds the `ref` parameter so the caller's variable now points to the new array. If `newSize` is smaller than the current length, trailing elements are discarded. If it is larger, new slots are filled with the type default.

The `ref` keyword is essential here. Without it, the method could only modify a local copy of the reference, leaving the caller's variable pointing at the original array. Because of this, `Array.Resize` only updates the one variable passed as `ref`. Any other variable that was already pointing at the old array is not affected — it still holds a reference to the original, unmodified array. This is a common source of confusion when multiple references to the same array exist in a codebase.

```csharp
// net10.0
int[] data = [10, 20, 30];
int[] aliasBeforeResize = data;          // second reference to same array

Array.Resize(ref data, 5);
// data now points at a NEW 5-element array: [10, 20, 30, 0, 0]
// aliasBeforeResize still points at the original: [10, 20, 30]

Console.WriteLine(data.Length);                 // 5
Console.WriteLine(aliasBeforeResize.Length);    // 3 — unchanged

Array.Resize(ref data, 2);
// data: [10, 20] — trailing elements dropped
```

---

## Q10. How do `Array.Clear` and `Array.Fill` work, and what values do they write?

**Concepts**
- `Array.Clear` — resets elements to type default
- `Array.Fill` — fills a range with a specified value
- Range overloads with start index and count
- Default values per type (0, false, null)
- In-place mutation

**Answer**

`Array.Clear(array, startIndex, count)` sets a range of elements to the type's default value: `0` for integers and floating-point types, `false` for booleans, `'\0'` for char, and `null` for reference types and nullable value types. It does not shrink the array or remove elements; it simply zeroes out the selected slots. It is the correct way to "reset" a reusable buffer between uses.

`Array.Fill(array, value)` sets every element of the array to the given value. An overload `Array.Fill(array, value, startIndex, count)` fills only a specified range. It is more expressive than a loop when initializing sentinel values, pre-filling a cache with a default, or writing test data.

```csharp
// net10.0
int[] buffer = [9, 9, 9, 9, 9];

Array.Clear(buffer, 1, 2);          // slots 1–2 → 0   result: [9, 0, 0, 9, 9]
Array.Fill(buffer, 7, 3, 2);        // slots 3–4 → 7   result: [9, 0, 0, 7, 7]

Console.WriteLine(string.Join(", ", buffer));  // 9, 0, 0, 7, 7

// Reference type clear
string[] labels = ["A", "B", "C"];
Array.Clear(labels, 0, labels.Length);  // all slots → null
Console.WriteLine(labels[0] is null);   // True
```

---

## Q11. What is `Span<T>` and how does it provide a zero-allocation view over an array?

**Concepts**
- `Span<T>` — ref struct over a contiguous memory region
- Slice syntax `span[start..end]` avoids allocation
- Stack-only — cannot be stored as a field or used in async methods
- Supports read and write through the view
- `Memory<T>` — heap-safe counterpart for async and class fields

**Answer**

`Span<T>` is a `ref struct` that holds a reference to a contiguous region of memory — an array, a stack-allocated block, or native memory — along with a length. It exposes indexed access and enumeration without copying the underlying data. This makes it ideal for processing sub-ranges of an array: calling `span.Slice(10, 20)` or using range syntax `span[10..30]` returns a new `Span<T>` pointing into the same memory at zero allocation cost.

Because `Span<T>` is a `ref struct`, the compiler enforces that it lives only on the stack — it cannot be boxed, stored as a class field, captured in a lambda, or used across `await` points. For scenarios that require persistence across asynchronous boundaries or storage in fields, `Memory<T>` serves the same purpose but without the stack restriction. `Memory<T>` exposes a `Span` property when synchronous work begins, deferring the pinning cost to that moment.

```csharp
// net10.0
int[] prices = [10, 20, 30, 40, 50, 60, 70, 80];

Span<int> view = prices.AsSpan();           // zero-allocation view
Span<int> middle = view[2..6];              // slice: [30, 40, 50, 60]

middle[0] = 99;                             // writes back into original prices array
Console.WriteLine(prices[2]);               // 99 — Span is a view, not a copy

// Summing a range without allocating a sub-array
static int SumSlice(ReadOnlySpan<int> data)
{
    int sum = 0;
    foreach (int v in data) sum += v;
    return sum;
}

Console.WriteLine(SumSlice(prices.AsSpan(3, 3)));  // 40 + 50 + 60 = 150 (original prices[3..6])
```

---

## Q12. What is array covariance in C# and why can it cause a runtime exception?

**Concepts**
- Array covariance — assigning `Derived[]` to `Base[]` reference
- Compile-time type vs runtime type mismatch
- `ArrayTypeMismatchException` on write
- Applies only to reference type arrays
- `IReadOnlyList<T>` covariance is safe (read-only)

**Answer**

C# allows assigning an array of a derived type to a variable of a base type array: `object[] arr = new string[3]` compiles without a cast. This is called array covariance. Reading elements through the `object[]` reference is safe because every `string` is an `object`. However, writing a non-`string` value — `arr[0] = 42` (a boxed integer) — throws `ArrayTypeMismatchException` at runtime, because the actual heap object is a `string[]` and only strings are type-safe to store there. The compiler cannot detect this at compile time because the violation happens through the less-specific reference type.

Array covariance exists because of a design decision made before generics were added to C#. It allows methods that accept `object[]` to receive `string[]` without copying, at the cost of introducing a runtime failure mode on writes. The modern safe alternative is `IReadOnlyList<string>` (covariant through the `out` type parameter on `IEnumerable<T>`), which prevents writes entirely. Prefer `IReadOnlyList<T>`, `ReadOnlySpan<T>`, or `IEnumerable<T>` in API signatures when only reading is required.

```csharp
// net10.0
string[] strings = ["hello", "world"];
object[] asObjects = strings;             // covariant assignment — compiles fine

Console.WriteLine(asObjects[0]);          // "hello" — reading is safe

// Writing a compatible type is fine
asObjects[0] = "updated";

// Writing an incompatible type throws at runtime
try
{
    asObjects[0] = 42;                    // 42 is an int, not a string
}
catch (ArrayTypeMismatchException ex)
{
    Console.WriteLine(ex.Message);        // runtime exception — not caught at compile time
}
```

---

## Q13. What is a fixed-size buffer in C# and when would you use one?

**Concepts**
- `unsafe` struct field with `fixed T name[count]`
- Inline storage — no separate heap allocation
- Used for P/Invoke interop and native data layouts
- Requires `unsafe` compilation flag
- Not a general-purpose replacement for arrays

**Answer**

A fixed-size buffer is an `unsafe` struct feature that embeds a contiguous block of a primitive type directly inside a struct's memory, bypassing the heap. The syntax is `fixed int Data[8]` inside an `unsafe struct`. This produces a struct whose value includes the eight integers inline, rather than holding a reference to a separately allocated array. The struct layout mirrors C/C++ structs, making it essential for P/Invoke scenarios where native code expects a specific memory layout — a network packet header with an embedded byte array, for example.

Fixed-size buffers are not general-purpose arrays. They are limited to unmanaged primitive types (int, byte, short, etc.), live only inside `unsafe` structs, and require the `unsafe` keyword and a compiler flag. Accessing elements requires a `fixed` statement to pin the struct in memory and obtain a pointer. In modern .NET, `Span<T>` and `Memory<T>` cover most zero-copy buffer needs without unsafe code; fixed-size buffers are reserved for interop scenarios where the exact binary layout must match a native or protocol definition.

```csharp
// net10.0 — unsafe feature, requires <AllowUnsafeBlocks>true</AllowUnsafeBlocks> in .csproj
unsafe struct NativePacketHeader
{
    public int MessageType;
    public fixed byte Payload[16];    // 16 bytes inline — no heap allocation
}

unsafe
{
    NativePacketHeader header = default;
    header.MessageType = 3;

    fixed (byte* ptr = header.Payload)
    {
        ptr[0] = 0xAB;
        ptr[1] = 0xCD;
    }

    Console.WriteLine(header.MessageType);  // 3
}
```

---

## Q14. How does passing an array to a method work in C#, and what is the difference between modifying elements versus reassigning the parameter?

**Concepts**
- Arrays are reference types — method receives a copy of the reference
- Modifying elements through the parameter affects the caller's array
- Reassigning the parameter rebinds only the local variable
- `ref` parameter to reassign the caller's variable
- `in` / `ReadOnlySpan<T>` to enforce read-only semantics

**Answer**

When you pass an array to a method, C# copies the reference, not the array data. Both the caller's variable and the method's parameter point at the same heap array. Assignments to elements through the parameter (`scores[0] = 99`) write into the shared heap object and are visible to the caller after the method returns. This is the standard way to implement in-place modification.

Reassigning the parameter itself (`scores = new int[5]`) only rebinds the local parameter variable to a new array; the caller's variable still points at the original. To make the caller's variable point at a new array, the parameter must be declared `ref`. For methods that should not modify any elements, accept `ReadOnlySpan<T>` or `IReadOnlyList<T>` — these types prevent writes and clearly communicate intent to callers.

```csharp
// net10.0
static void DoubleAll(int[] arr)
{
    for (int i = 0; i < arr.Length; i++)
        arr[i] *= 2;                // modifies shared heap array — caller sees change
}

static void ReplaceWithNew(int[] arr)
{
    arr = [99, 99];                 // rebinds local parameter only — caller unchanged
}

static void ReplaceViaRef(ref int[] arr)
{
    arr = [99, 99];                 // rebinds caller's variable — caller sees new array
}

int[] data = [1, 2, 3];
DoubleAll(data);
Console.WriteLine(data[0]);        // 2 — element mutation visible to caller

ReplaceWithNew(data);
Console.WriteLine(data[0]);        // 2 — caller still holds original

ReplaceViaRef(ref data);
Console.WriteLine(data[0]);        // 99 — caller's variable was rebound
```

---

## Q15. What is the `params` keyword and how does the compiler handle a params array parameter?

**Concepts**
- `params T[]` — variable-length argument list in method signature
- Compiler wraps comma-separated arguments into an array
- Must be the last parameter
- Callers can pass a pre-built array directly
- `params IEnumerable<T>` added in C# 13

**Answer**

The `params` modifier on the last parameter of a method allows callers to pass any number of comma-separated values of that type without explicitly constructing an array. The compiler rewrites the call site — `Sum(1, 2, 3)` becomes `Sum(new int[] { 1, 2, 3 })` — so the method body simply receives an `int[]`. If no arguments are provided, the compiler passes an empty array; calling the method with a pre-built array also works without modification.

`params` is useful for utility methods and logging helpers where the argument count varies at each call site. It is also the mechanism behind `Console.WriteLine`'s format overloads. C# 13 extended `params` to accept `IEnumerable<T>`, `ReadOnlySpan<T>`, and other collection types, which allows more efficient implementations without forcing an array allocation. The compiler selects the best overload based on the argument types passed.

```csharp
// net10.0
static int Sum(params int[] values)
{
    int total = 0;
    foreach (int v in values) total += v;
    return total;
}

Console.WriteLine(Sum(10, 20, 30));             // 60 — compiler wraps into int[]
Console.WriteLine(Sum(5));                      // 5
Console.WriteLine(Sum());                       // 0 — empty array passed
Console.WriteLine(Sum([1, 2, 3, 4]));           // 10 — pre-built array passed directly

// C# 13 — params ReadOnlySpan<T> avoids heap allocation at call site
static int SumSpan(params ReadOnlySpan<int> values)
{
    int total = 0;
    foreach (int v in values) total += v;
    return total;
}
Console.WriteLine(SumSpan(1, 2, 3));            // 6 — stack-allocated, no heap array
```

---

## Gotchas

---

## Q16. What is the off-by-one bug with `i <= array.Length` and why does the compiler not catch it?

**Concepts**
- Valid index range: 0 to `Length − 1`
- `i <= Length` reads one past the end
- `IndexOutOfRangeException` on the final iteration only
- Compiler treats this as valid C# — no diagnostic
- Pattern: always use `i < array.Length`

**Answer**

The off-by-one gotcha with `i <= array.Length` is perhaps the most frequent array bug. For a length-5 array, valid indices are 0 through 4. When the loop bound is `i <= scores.Length` instead of `i < scores.Length`, the loop body executes for `i = 0, 1, 2, 3, 4` correctly and then runs one final time with `i = 5` — which is out of range. The compiler sees a perfectly valid comparison of two integers and emits no warning; the crash only happens at runtime, typically on non-empty arrays in production rather than in unit tests that test with small samples.

The fix is always `i < array.Length`. If you use index-from-end or range syntax, the runtime calculates the actual index and applies its own bounds check safely. For a particularly subtle variant, the bug appears in sum calculations where the loop body adds and the out-of-bounds access is on an incidental read: it throws on the read rather than the accumulation, making the stack trace point somewhere unexpected.

```csharp
// net10.0 — WRONG: off-by-one
int[] scores = [88, 92, 76, 85, 90];
int total = 0;
for (int i = 0; i <= scores.Length; i++)    // BUG: i reaches 5 (= Length), out of range
    total += scores[i];                     // throws IndexOutOfRangeException when i == 5

// CORRECT
total = 0;
for (int i = 0; i < scores.Length; i++)     // i stops at 4 (= Length - 1)
    total += scores[i];
```

---

## Q17. Why does assigning to a `foreach` iteration variable not update the array element?

**Concepts**
- `foreach` iteration variable is a copy of the element value
- Assigning to it produces CS1654 compile error
- Value types copy; reference types copy the reference
- In-place mutation requires indexed `for`
- LINQ `.Select` for transforming to a new collection

**Answer**

The `foreach` iteration variable is read-only — assigning to it produces compile error CS1654: "Cannot assign to 'score' because it is a foreach iteration variable." The variable holds a copy of the element (for value types) or a copy of the reference (for reference types). Writing to the copy has no effect on the original array slot.

This surprises developers who come from languages where similar constructs do allow mutation, or who believe that a reference-type element variable "is" the element. For reference types, the gotcha is subtler: you can call methods on the object the variable refers to and those side effects will persist, but you cannot replace which object occupies that array slot. In-place slot replacement always requires indexed `for` access.

```csharp
// net10.0
int[] scores = [88, 40, 76, 35, 90];

// WRONG — CS1654 compile error
// foreach (int score in scores)
//     if (score < 50) score = 50;     // CS1654: cannot assign to foreach variable

// CORRECT — use for with index
for (int i = 0; i < scores.Length; i++)
    if (scores[i] < 50) scores[i] = 50;    // writes back to original slot

// Reference-type gotcha — can mutate object state but not replace the slot
var items = new System.Text.StringBuilder[] { new("hello"), new("world") };
foreach (var sb in items)
    sb.Append("!");                          // mutates object — changes ARE visible
// items[0].ToString() == "hello!" — the mutation propagated
```

---

## Q18. Why does `Array.BinarySearch` give wrong results on an unsorted array?

**Concepts**
- Binary search assumes sorted order — halves search space each step
- Unsorted array violates the invariant
- Returns wrong index or wrong negative value silently
- Does not throw — undefined behavior, not an exception
- Must sort with the same comparer before searching

**Answer**

`Array.BinarySearch` works by repeatedly halving the search range based on a comparison with the middle element. It assumes the array is sorted in the same order as the comparer being used. If the array is unsorted, the algorithm discards the wrong half at some step, potentially concluding that a present element does not exist (returning a negative value) or worse, returning a valid-looking index that points to the wrong element. Neither case throws an exception — the API contract places the responsibility for sorting on the caller.

This is dangerous in production because tests often run on data that happens to be sorted or nearly sorted, hiding the bug until a different data set arrives. The fix is always to call `Array.Sort` — with the same comparer — immediately before `Array.BinarySearch`, or to assert that the input is sorted. When you need to search an unsorted array, use `Array.IndexOf` (O(n) linear scan) instead.

```csharp
// net10.0
string[] skus = ["TBK-220", "TBK-087", "TBK-315", "TBK-104", "TBK-150"];
// Array is NOT sorted

// WRONG — silent incorrect result (not an exception)
int wrong = Array.BinarySearch(skus, "TBK-087");
Console.WriteLine(wrong);     // may be -1 or a wrong index — not -3 (the correct linear index)

// CORRECT — sort first, then search
Array.Sort(skus);
int correct = Array.BinarySearch(skus, "TBK-087");
Console.WriteLine(correct);   // 0 — now "TBK-087" is at index 0 after sorting
```

---

## Q19. What is the `ArrayTypeMismatchException` covariance trap and how do you avoid it?

**Concepts**
- Covariant assignment: `Derived[]` to `Base[]` variable
- Read through base reference is safe; write can fail
- `ArrayTypeMismatchException` at runtime, not compile time
- Affects only reference type arrays
- Prevention: read-only interface types, `IReadOnlyList<T>`

**Answer**

When a `string[]` is assigned to an `object[]` variable, the compiler allows it because every `string` is an `object`. Reading elements through the `object[]` reference is always safe. Writing a value that is not a `string` — such as `arr[0] = new Uri("http://example.com")` — compiles without error because the compiler sees an `object[]` and a `Uri`, both reference types, and assumes the write is valid. At runtime the CLR checks that the actual array type (`string[]`) accepts the value being stored and throws `ArrayTypeMismatchException` when it does not.

The trap is that neither the compiler nor static analysis tools catch this in the general case. The safe approach is to expose read-only views: `IReadOnlyList<string>` prevents writes entirely, and the compiler enforces this. `ReadOnlySpan<string>` serves the same purpose for stack-local processing. If a method genuinely only reads, declare the parameter as `IReadOnlyList<T>` or `IEnumerable<T>` and the covariance issue cannot arise.

```csharp
// net10.0
string[] strings = ["hello", "world"];
object[] asObjects = strings;           // covariant — compiles fine

asObjects[0] = "safe update";           // fine — string assigned to string[]

try
{
    asObjects[0] = new object();        // TRAP — object not a string: ArrayTypeMismatchException
}
catch (ArrayTypeMismatchException)
{
    Console.WriteLine("Covariance trap triggered");
}

// Safe alternative — accept IReadOnlyList<T>
static void PrintAll(IReadOnlyList<string> items)
{
    foreach (string s in items) Console.WriteLine(s);
    // no write possible — covariance trap eliminated
}
PrintAll(strings);
```

---

## Q20. Why does `Clone` on an array of reference types not produce an independent copy?

**Concepts**
- Shallow copy — element references are copied, not the objects
- Mutations through cloned array affect original objects
- Deep copy requires manual element cloning
- Value type arrays: `Clone` does produce independent copies
- `Array.Copy` has the same shallow behaviour

**Answer**

`Clone` copies the array structure — a new array object with the same length — and copies each element slot. For value-type arrays (`int[]`, `struct[]`), this is a true independent copy: modifying the cloned array's elements does not affect the original. For reference-type arrays (`string[]`, `object[]`, custom class arrays), each element slot holds a reference (a memory address), and `Clone` copies those references. Both arrays now hold references to the same underlying objects.

This means that if the element objects are mutable, calling a mutating method on an element through the cloned array will modify the same object that the original array points to. Replacing a slot in the clone (`cloned[0] = new Thing()`) does not affect the original array's slot — only the reference in that slot is replaced. A deep copy requires iterating every element and creating a new instance of each object.

```csharp
// net10.0
// Value type — Clone is a true independent copy
int[] original = [1, 2, 3];
int[] cloned = (int[])original.Clone();
cloned[0] = 99;
Console.WriteLine(original[0]);     // 1 — unaffected

// Reference type — shallow: both arrays share object references
var sb1 = new System.Text.StringBuilder("hello");
System.Text.StringBuilder[] refs = [sb1];
System.Text.StringBuilder[] shallowCopy = (System.Text.StringBuilder[])refs.Clone();

shallowCopy[0].Append(" world");    // mutates the SAME StringBuilder object
Console.WriteLine(refs[0]);         // "hello world" — original array sees the mutation

shallowCopy[0] = new System.Text.StringBuilder("replaced");
Console.WriteLine(refs[0]);         // "hello world" — slot replacement does NOT affect original
```

---

## Real-World Scenarios

---

## Q21. Code Review — Exam score averager throws intermittently in production

A batch reporting job crashes intermittently on empty student records. Review this helper:

```csharp
public static double AverageScores(int[] scores)
{
    double total = 0;
    for (int i = 0; i <= scores.Length; i++)
    {
        total += scores[i];
    }
    return total / scores.Length;
}
```

**Concepts**
- Off-by-one loop bound
- Division by zero on empty array
- Null parameter not guarded
- Contract definition for empty input
- Unit test coverage for edge cases

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `i <= scores.Length` — reads index `Length` on every non-empty call | `IndexOutOfRangeException` on last iteration |
| Runtime | `total / scores.Length` when `scores.Length == 0` | `DivideByZeroException` on empty arrays |
| Robustness | No null guard on `scores` parameter | `NullReferenceException` when caller passes null |
| Design | No documented contract for "no scores" case | Callers cannot distinguish valid zero-score from crash |

**Fix priority**

1. Change loop bound to `i < scores.Length` — matches the established safe pattern.
2. Add null and empty guard at entry; return `0` or `double.NaN` and document which is the contract.
3. Validate `scores` is not null before accessing any member.
4. Add unit tests for `null`, `[]`, single element, and multi-element arrays.

```csharp
// net10.0 — corrected version
public static double AverageScores(int[] scores)
{
    ArgumentNullException.ThrowIfNull(scores);
    if (scores.Length == 0)
        return 0.0;                              // defined contract for empty input

    double total = 0;
    for (int i = 0; i < scores.Length; i++)     // correct bound: < not <=
        total += scores[i];

    return total / scores.Length;
}
```

---

## Q22. Code Review — Pricing service corrupts the audit log on every request

A pricing audit log shows SKU codes reordered even though the developer intended to preserve the original. Review:

```csharp
public static string[] GetSortedSkusForDisplay(string[] skuCodes)
{
    string[] snapshot = skuCodes;
    Array.Sort(snapshot);
    return snapshot;
}

public static void UpdateSku(string[] skuCodes, int index, string newCode)
{
    skuCodes = new string[] { newCode };
}
```

**Concepts**
- Reference assignment copies reference, not data
- `Array.Sort` mutates in place
- Parameter reassignment rebinds local variable only
- `Clone` vs reference copy
- `ref` parameter for rebinding the caller's variable

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `snapshot = skuCodes` copies the reference — both point to same array | `Array.Sort` mutates the caller's original; audit log shows reordered input |
| Correctness | `skuCodes = new string[] { newCode }` rebinds local parameter only | Caller's array is unchanged; `UpdateSku` silently does nothing |
| API design | `Array.Sort` documented as in-place — code name "snapshot" implies copy | Misleading naming increases maintenance risk |
| Robustness | No bounds check in `UpdateSku` | Index could be out of range when eventually fixed |

**Fix priority**

1. Replace reference copy with `(string[])skuCodes.Clone()` before sorting to obtain an independent array.
2. Fix `UpdateSku` to mutate the slot: `skuCodes[index] = newCode` with a bounds guard.
3. Rename `snapshot` to `sorted` to reflect actual semantics.
4. Document in XML comments that `GetSortedSkusForDisplay` does not modify its input.

```csharp
// net10.0 — corrected
public static string[] GetSortedSkusForDisplay(string[] skuCodes)
{
    string[] sorted = (string[])skuCodes.Clone();    // independent copy — original untouched
    Array.Sort(sorted);
    return sorted;
}

public static void UpdateSku(string[] skuCodes, int index, string newCode)
{
    ArgumentOutOfRangeException.ThrowIfNegative(index);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, skuCodes.Length);
    skuCodes[index] = newCode;                       // mutate the slot, not the parameter
}
```

---

## Q23. Code Review — Pass-rate normaliser fails silently on failing scores

A student pass-rate calculator is supposed to floor any score below the passing mark up to the minimum pass value, but QA reports no scores are being changed. Review:

```csharp
public static void NormalizeScores(int[] scores, int passingMark)
{
    foreach (int score in scores)
    {
        if (score < passingMark)
        {
            score = passingMark;
        }
    }
}
```

**Concepts**
- `foreach` iteration variable is read-only (CS1654)
- Iteration variable holds a copy of the value
- In-place slot mutation requires indexed `for`
- Silent no-op vs compile error depending on type

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Compile | Assignment to `foreach` iteration variable `score` | CS1654 — does not compile |
| Logic | Even if compiled, `score = passingMark` writes a local copy | Source array unchanged; normalisation never applied |
| Design | Method signature implies in-place mutation but foreach cannot provide it | Caller expects normalised array, gets original values |
| Robustness | No guard against null or empty `scores` parameter | `NullReferenceException` if null passed |

**Fix priority**

1. Replace `foreach` with an indexed `for` loop to enable slot write-back.
2. Add null guard on `scores` at method entry.
3. Document that the method mutates the passed array in place.
4. Add a unit test asserting that elements below `passingMark` are replaced after the call.

```csharp
// net10.0 — corrected
public static void NormalizeScores(int[] scores, int passingMark)
{
    ArgumentNullException.ThrowIfNull(scores);
    for (int i = 0; i < scores.Length; i++)          // indexed for — enables slot mutation
    {
        if (scores[i] < passingMark)
            scores[i] = passingMark;                  // writes directly to the array slot
    }
}
```

---

## Q24. Code Review — Report builder fails to compile after partial migration from arrays to `List<T>`

A developer migrated the `subjects` parallel collection from `string[]` to `List<string>` but left `scores` as `int[]`. Review:

```csharp
public static string BuildScoreReport(List<string> subjects, int[] scores)
{
    if (subjects.Count != scores.Count)
        throw new ArgumentException("Length mismatch");

    var report = new System.Text.StringBuilder();
    for (int i = 0; i < subjects.Count; i++)
    {
        report.Append(subjects[i]).Append('=').Append(scores[i]);
        if (i < subjects.Count - 1) report.Append("; ");
    }

    int lastIndex = scores.Count - 1;
    return report.ToString();
}
```

**Concepts**
- Arrays expose `Length`; `List<T>` exposes `Count`
- Mixing collection types in parallel-index patterns
- CS1061 for calling undefined member
- API consistency in collection types

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Compile | `scores.Count` on `int[]` | CS1061 — `int[]` has no `Count` member; causes build failure |
| Compile | `lastIndex = scores.Count - 1` at method bottom | Same CS1061 — `Length` is the correct member for arrays |
| Design | Mixed `List<T>` and `T[]` parallel collections | Fragile pairing; one more migration step breaks it again |
| Robustness | Guard uses `scores.Count` which does not compile | Cannot evaluate guard before fixing — double bug |

**Fix priority**

1. Replace every `scores.Count` with `scores.Length` to fix the compile error immediately.
2. Update the guard to `subjects.Count != scores.Length` — one `Count`, one `Length`.
3. Decide on a consistent collection type for both parallel collections and complete the migration.
4. Consider encapsulating the pair in a record or tuple to remove the parallel-array risk entirely.

```csharp
// net10.0 — corrected
public static string BuildScoreReport(List<string> subjects, int[] scores)
{
    if (subjects.Count != scores.Length)                     // Count for List, Length for array
        throw new ArgumentException("subjects and scores must have equal length.");

    var report = new System.Text.StringBuilder();
    for (int i = 0; i < subjects.Count; i++)
    {
        report.Append(subjects[i]).Append('=').Append(scores[i]);
        if (i < subjects.Count - 1) report.Append("; ");
    }

    int lastIndex = scores.Length - 1;                       // Length — not Count
    return report.ToString();
}
```

---

## Q25. Scenario — Processing a large binary protocol buffer without heap allocation

A high-throughput network service receives fixed-length 1024-byte protocol frames and must parse the header (first 16 bytes) and route the body (remaining bytes) without allocating sub-arrays. How do you implement this with `Span<T>` and `Memory<T>`?

**Concepts**
- `Span<T>` slice — zero-allocation view over a range
- `MemoryMarshal` for reading structured data from byte spans
- `Memory<T>` for async boundaries
- Stack allocation with `stackalloc` for small temporary buffers
- `BinaryPrimitives` for endian-safe field reads

**Answer**

The key constraint is avoiding the heap allocations that would occur if you created a new `byte[]` for the header and another for the body. `Span<T>` solves this directly: `frame.AsSpan(0, 16)` returns a `Span<byte>` that points into the first 16 bytes of `frame` with no data copying. `frame.AsSpan(16)` gives a span over the remaining bytes. Both spans share the original `frame` array's memory.

For reading structured fields from the header span, `System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(headerSpan)` reads a four-byte big-endian integer without any intermediate allocation. For code that crosses an `await` boundary — async I/O handlers — you must use `Memory<byte>` instead of `Span<byte>` because `Span` is a `ref struct` and cannot survive a suspend point. `Memory<byte>` is a regular struct that can be stored and awaited; call `memory.Span` inside the synchronous section to get back a `Span` for the actual processing.

```csharp
// net10.0
using System.Buffers.Binary;

static (int MessageType, int SequenceNumber, ReadOnlySpan<byte> Body)
    ParseFrame(byte[] frame)
{
    if (frame.Length < 16)
        throw new ArgumentException("Frame too short");

    ReadOnlySpan<byte> header = frame.AsSpan(0, 16);    // zero-allocation header view
    ReadOnlySpan<byte> body   = frame.AsSpan(16);        // zero-allocation body view

    int msgType = BinaryPrimitives.ReadInt32BigEndian(header[0..4]);
    int seqNum  = BinaryPrimitives.ReadInt32BigEndian(header[4..8]);
    // remaining 8 bytes reserved — ignored

    return (msgType, seqNum, body);
}

// Async boundary — use Memory<T> across await
static async Task ProcessFrameAsync(Memory<byte> frameMemory, Stream output,
    CancellationToken ct)
{
    // Memory<T> survives the await
    await output.WriteAsync(frameMemory[16..], ct);      // write body asynchronously

    // Back in synchronous code — now safe to use Span<T>
    Span<byte> headerSpan = frameMemory.Span[0..16];
    headerSpan[0] = 0xFF;                                // mark frame as processed
}
```

---

## Q26. Scenario — Sorting students by score while keeping names and scores aligned

An exam management API returns students as two parallel arrays — names and scores — and must sort both in descending score order for a leaderboard. Walk through the safest approach using `Array.Sort`.

**Concepts**
- `Array.Sort(keys, items)` — parallel-array sort
- Descending order via `Comparison<T>` delegate
- `Array.Reverse` as an alternative
- Cloning before sort to preserve originals
- Choosing between parallel arrays and structured records

**Answer**

`Array.Sort` provides an overload that accepts two arrays: `Array.Sort(keys, values)`. It sorts the `keys` array in ascending order and applies every swap to the `values` array in tandem, maintaining the index alignment. This is exactly what a leaderboard needs — sort `scores` ascending, mirror those swaps into `names`, then reverse both arrays to achieve descending order. Alternatively, pass a `Comparison<T>` to the sort to request descending order directly, which avoids a second pass.

Before sorting, clone both arrays if the original order must be preserved elsewhere in the application — `Array.Sort` is in-place and will corrupt any other reference to the same heap objects. In a greenfield design, replacing the two parallel arrays with an array of records (`(string Name, int Score)[]`) eliminates the risk entirely: the fields travel together with no possibility of misalignment after a sort.

```csharp
// net10.0
string[] names  = ["Priya", "Alex", "Sam", "Jordan", "Taylor"];
int[]    scores = [92,       88,     76,    95,        83];

// Clone to preserve originals for audit
string[] sortedNames  = (string[])names.Clone();
int[]    sortedScores = (int[])scores.Clone();

// Sort ascending by score (keys), names mirror the swaps
Array.Sort(sortedScores, sortedNames);

// Reverse both for descending leaderboard
Array.Reverse(sortedScores);
Array.Reverse(sortedNames);

for (int i = 0; i < sortedNames.Length; i++)
    Console.WriteLine($"{i + 1}. {sortedNames[i]}: {sortedScores[i]}");
// 1. Jordan: 95   2. Priya: 92   3. Alex: 88   4. Taylor: 83   5. Sam: 76

// Modern alternative — record array eliminates alignment risk
var students = names.Zip(scores, (n, s) => (Name: n, Score: s)).ToArray();
Array.Sort(students, (a, b) => b.Score.CompareTo(a.Score));   // descending
```
