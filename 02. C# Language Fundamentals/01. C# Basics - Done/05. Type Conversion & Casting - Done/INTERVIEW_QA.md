# Type Conversion & Casting — Interview Q&A


## Table of Contents

1. [Q1. What is an implicit conversion and when does the compiler allow one without explicit cast syntax?](#q1-what-is-an-implicit-conversion-and-when-does-the-compiler-allow-one-without-explicit-cast-syntax)
2. [Q2. What is a narrowing conversion and why does C# require an explicit cast for one?](#q2-what-is-a-narrowing-conversion-and-why-does-c-require-an-explicit-cast-for-one)
3. [Q3. How does the C-style cast `(T)expr` behave differently for value types versus reference types?](#q3-how-does-the-c-style-cast-texpr-behave-differently-for-value-types-versus-reference-types)
4. [Q4. What happens to the fractional part when you cast a floating-point or decimal value to an integer type?](#q4-what-happens-to-the-fractional-part-when-you-cast-a-floating-point-or-decimal-value-to-an-integer-type)
5. [Q5. How do `checked` and `unchecked` contexts affect narrowing casts?](#q5-how-do-checked-and-unchecked-contexts-affect-narrowing-casts)
6. [Q6. What is the difference between `int.Parse` and `int.TryParse`, and when should you prefer each?](#q6-what-is-the-difference-between-intparse-and-inttryparse-and-when-should-you-prefer-each)
7. [Q7. How does `Convert.ToInt32` handle a `null` argument differently from `int.Parse`?](#q7-how-does-converttoint32-handle-a-null-argument-differently-from-intparse)
8. [Q8. What is `Convert.ChangeType` and when would you use it over `Parse` or a direct cast?](#q8-what-is-convertchangetype-and-when-would-you-use-it-over-parse-or-a-direct-cast)
9. [Q9. What is boxing in C# and what happens at the memory level when a value type is boxed?](#q9-what-is-boxing-in-c-and-what-happens-at-the-memory-level-when-a-value-type-is-boxed)
10. [Q10. Why must you unbox to the exact stored type and not a compatible widening type?](#q10-why-must-you-unbox-to-the-exact-stored-type-and-not-a-compatible-widening-type)
11. [Q11. What is the difference between the `as` operator and a direct cast `(T)` for reference type downcasting?](#q11-what-is-the-difference-between-the-as-operator-and-a-direct-cast-t-for-reference-type-downcasting)
12. [Q12. How does `is` declaration pattern matching compare to using `as` followed by a null check?](#q12-how-does-is-declaration-pattern-matching-compare-to-using-as-followed-by-a-null-check)
13. [Q13. What is `IConvertible` and how does it relate to the `Convert` class?](#q13-what-is-iconvertible-and-how-does-it-relate-to-the-convert-class)
14. [Q14. How do you define custom implicit and explicit conversion operators in C#?](#q14-how-do-you-define-custom-implicit-and-explicit-conversion-operators-in-c)
15. [Q15. What is `TryFormat` and how does it differ from `ToString` in allocation behavior?](#q15-what-is-tryformat-and-how-does-it-differ-from-tostring-in-allocation-behavior)
16. [Q16. Why does `(long)(object)42` throw `InvalidCastException` even though `42` fits in a `long`?](#q16-why-does-longobject42-throw-invalidcastexception-even-though-42-fits-in-a-long)
17. [Q17. Why does `(int)3.9999` produce `3` and not `4`?](#q17-why-does-int39999-produce-3-and-not-4)
18. [Q18. Why does `Convert.ToString(null)` return an empty string instead of throwing?](#q18-why-does-converttostringnull-return-an-empty-string-instead-of-throwing)
19. [Q19. Why is `obj as int` a compile error when `obj as string` compiles fine?](#q19-why-is-obj-as-int-a-compile-error-when-obj-as-string-compiles-fine)
20. [Q20. Does placing a narrowing cast inside a `checked` block prevent all forms of data loss?](#q20-does-placing-a-narrowing-cast-inside-a-checked-block-prevent-all-forms-of-data-loss)
21. [Q21. (Code Review) A warehouse pricing service receives boxed quantities and tries to unbox them as `long`. What fails at runtime and how would you fix it?](#q21-code-review-a-warehouse-pricing-service-receives-boxed-quantities-and-tries-to-unbox-them-as-long-what-fails-at-runtime-and-how-would-you-fix-it)
22. [Q22. (Code Review) An ASP.NET Core controller calls `int.Parse` directly on a URL route segment. What is wrong and how do you fix it?](#q22-code-review-an-aspnet-core-controller-calls-intparse-directly-on-a-url-route-segment-what-is-wrong-and-how-do-you-fix-it)
23. [Q23. (Code Review) An inventory service casts `int` location IDs to `byte` for a shelf-slot database column. What corrupts data silently and how do you prevent it?](#q23-code-review-an-inventory-service-casts-int-location-ids-to-byte-for-a-shelf-slot-database-column-what-corrupts-data-silently-and-how-do-you-prevent-it)
24. [Q24. (Code Review) A shipping label builder uses `as` without null-checking the result. What throws and what is the cleaner fix?](#q24-code-review-a-shipping-label-builder-uses-as-without-null-checking-the-result-what-throws-and-what-is-the-cleaner-fix)
25. [Q25. A partner integration POSTs prices formatted as German decimal strings (`"1.234,56"`) to an en-US server. How do you parse them safely and what API contract prevents the problem entirely?](#q25-a-partner-integration-posts-prices-formatted-as-german-decimal-strings-123456-to-an-en-us-server-how-do-you-parse-them-safely-and-what-api-contract-prevents-the-problem-entirely)
26. [Q26. You are designing a `Money` value type. How would you use custom conversion operators to allow `Money price = 49.99m` but require `(decimal)price` when extracting the raw amount?](#q26-you-are-designing-a-money-value-type-how-would-you-use-custom-conversion-operators-to-allow-money-price-4999m-but-require-decimalprice-when-extracting-the-raw-amount)
27. [Q27. A legacy service stores thousands of integers per second in an `ArrayList`, then retrieves them for calculation. How does boxing affect correctness and performance, and what is the migration path?](#q27-a-legacy-service-stores-thousands-of-integers-per-second-in-an-arraylist-then-retrieves-them-for-calculation-how-does-boxing-affect-correctness-and-performance-and-what-is-the-migration-path)

---
## Foundation Questions

---

## Q1. What is an implicit conversion and when does the compiler allow one without explicit cast syntax?

**Concepts**
- widening conversion guarantee
- lossless value preservation
- compiler-inserted promotion
- implicit numeric chain
- user-defined implicit operator

**Answer**

An implicit conversion is one the compiler inserts automatically, with no cast syntax, because the destination type can represent every possible value of the source type without information loss. The core guarantee is lossless: the destination range must entirely contain every value the source type can hold. Common widening paths run from smaller to larger: `byte → short → int → long → float → double`; `int` also widens to `decimal` implicitly. Assigning an `int` variable to a `long` variable compiles without a cast because no `int` value can overflow a `long`. The rule extends to mixed-type arithmetic: in `int * decimal`, the compiler widens the `int` to `decimal` before multiplying. Not all widening is obvious — `char` converts implicitly to `int`, yielding the Unicode code point as a number. Conversely, `decimal` does not implicitly become `float` or `double`, because those representations use different encodings and neither direction is guaranteed lossless. User-defined types participate by declaring `public static implicit operator TargetType(SourceType s)`, letting the compiler treat that conversion as safe and automatic.

---

## Q2. What is a narrowing conversion and why does C# require an explicit cast for one?

**Concepts**
- narrowing and range-reducing conversion
- explicit cast syntax requirement
- developer acknowledgment of risk
- compile-time enforcement
- fraction truncation and overflow potential

**Answer**

A narrowing conversion moves a value into a type whose range or precision is smaller than the source, meaning some inputs will be silently truncated or wrapped. C# requires the explicit cast notation `(TargetType)expression` to force the developer to acknowledge the risk — the compiler refuses to insert a narrowing conversion silently. This requirement acts as a documentation marker in code review: every `(int)price` or `(byte)slotId` is a potential data-loss site that reviewers can inspect. Common narrowing paths include `long → int` (range loss), `double → int` (fraction truncation), `decimal → float` (precision loss), and `int → byte` (overflow wrap). For reference types, casting a base-class variable to a derived type is also narrowing: not every base-class instance is the derived type, so `(Derived)baseRef` can throw `InvalidCastException` at runtime. The combination of a compile-time requirement and possible runtime failure means narrowing casts must be paired with range validation or `checked` blocks whenever input values are not fully constrained at the call site.

---

## Q3. How does the C-style cast `(T)expr` behave differently for value types versus reference types?

**Concepts**
- value type numeric conversion
- reference type runtime type check
- `InvalidCastException` on type mismatch
- compile-time path validation only
- truncation versus exception asymmetry

**Answer**

For value types, `(T)expr` performs a numeric conversion: the compiler or runtime maps bits from one representation to another. Casting `decimal` to `int` discards the fractional part toward zero. Casting `int` to `byte` wraps modulo 256 in the default unchecked context. The cast always produces a result for value types — it never throws unless the code is inside a `checked` block that detects overflow. For reference types the behavior is fundamentally different: the runtime checks whether the heap object's actual type is compatible with the cast target. If the object was created as `DigitalLineItem` and you write `(PhysicalLineItem)obj`, the runtime throws `InvalidCastException` at that point regardless of the declared variable type. The compiler validates only that a conversion path exists between the declared types; it cannot know the runtime type of an `object` reference. This asymmetry — value casts always produce a result, reference casts may throw — explains why `as` and `is` were designed specifically for reference-type downcasts, where checking first and acting second is safer than catching exceptions.

---

## Q4. What happens to the fractional part when you cast a floating-point or decimal value to an integer type?

**Concepts**
- truncation toward zero
- no rounding in cast operator
- `Math.Round` before cast pattern
- signed negative truncation symmetry
- `checked` does not prevent fractional loss

**Answer**

Casting `double` or `decimal` to any integer type always truncates toward zero — the fractional digits are discarded without rounding. So `(int)49.99m` yields `49`, not `50`, and `(int)-49.99m` yields `-49`, not `-50`. The result is always the integer closest to zero, not the nearest integer. This surprises developers who expect rounding because `49.99` feels like it should round to `50`. The truncation rule applies consistently: `(int)3.9999`, `(long)3.9999`, and `(byte)3.9999` all yield `3`. When rounding is intended, the correct pattern is `(int)Math.Round(value, MidpointRounding.AwayFromZero)` before the cast. Using `checked` does not help here: `checked` detects overflow of the resulting integer — whether the integer portion exceeds the target type's range — but it accepts any in-range value, even one produced by silently discarding fractional digits. The distinction between overflow detection and truncation is a common source of confusion.

---

## Q5. How do `checked` and `unchecked` contexts affect narrowing casts?

**Concepts**
- `checked` overflow detection keyword
- `unchecked` default wrap-around behavior
- `OverflowException` at runtime
- `checked()` expression operator
- explicit range validation as alternative

**Answer**

In C#'s default unchecked context, a narrowing cast that produces a value outside the target type's range silently wraps. Casting `int` value `300` to `byte` yields `44` because `300 mod 256 = 44` — no exception, no warning, just wrong data. This silent wrap is the root cause of slot-ID corruption, counter wrap, and port-number bugs. The `checked` keyword changes this: `checked((byte)locationId)` makes the runtime verify the result fits in `byte` range and throw `OverflowException` if it does not. You can also surround a block with `checked { … }` to apply the check to all narrowing casts and arithmetic within that scope. The `unchecked` keyword is occasionally written explicitly to document that wrap-around is intentional — hash-code implementations, for instance, rely on wrap behavior and use `unchecked` to make that intent visible. For production business code, explicit range validation before the cast is usually preferred over `checked` because it produces a domain-meaningful error message and rejects invalid input at the boundary rather than inside the computation.

---

## Q6. What is the difference between `int.Parse` and `int.TryParse`, and when should you prefer each?

**Concepts**
- `Parse` exception-on-failure behavior
- `TryParse` bool-return pattern
- `FormatException` and `OverflowException`
- external input boundary rule
- exception-free control flow

**Answer**

`int.Parse(text)` converts a string to `int` and throws `FormatException` for non-numeric text or `OverflowException` for values outside `int` range. `int.TryParse(text, out int result)` performs the same conversion but returns `false` on failure, sets `result` to its default value, and raises no exception. The choice maps directly to the trust level of the input. Use `Parse` for strings that are known at compile time to be valid — configuration constants, test fixtures, hard-coded literals — where a failure genuinely indicates a programmer mistake and an exception is the right signal. Use `TryParse` at all external boundaries: HTTP query parameters, form fields, CSV rows, environment variables, and any input the user or a remote system controls. When `Parse` is used on untrusted HTTP input, a client sending `"abc"` crashes the request with an unhandled exception, producing a 500 status code instead of a 400 with a clear validation message. `TryParse` also avoids exception-based control flow, which carries higher overhead on the exception path. Every numeric type, `bool`, `DateTime`, `Guid`, and enums all provide the `TryParse` pattern, so the same discipline applies uniformly.

---

## Q7. How does `Convert.ToInt32` handle a `null` argument differently from `int.Parse`?

**Concepts**
- `Convert` null-to-default mapping
- `int.Parse` throws `ArgumentNullException`
- silent zero as potential data bug
- `DBNull` historical context
- optional parameter modeling with `int?`

**Answer**

`Convert.ToInt32(null)` returns `0` without throwing. The `Convert` class maps `null` to the default value for all value-type targets, a behavior designed for database interop where `DBNull` fields should fall back to zero rather than crash the application. `int.Parse(null)` throws `ArgumentNullException` immediately, treating `null` as an explicitly exceptional case. This difference looks like a convenience until it causes a data bug: an optional HTTP query parameter the caller omits produces a `null` string, and `Convert.ToInt32(null)` silently applies zero as though the caller explicitly passed `0`. The scenarios "parameter not supplied" and "parameter supplied as zero" become indistinguishable in the business logic. The correct tool for optional numeric parameters is `int.TryParse` combined with a nullable type: if the text is null or empty, leave the variable as `int?` with a null value and apply the absent-parameter business rule separately. Use `Convert.ToInt32` only in contexts where zero-on-null is explicitly the correct domain rule — reading from a legacy ADO.NET `DataReader` where `DBNull` should default to zero, for example — and document that rule at the call site.

---

## Q8. What is `Convert.ChangeType` and when would you use it over `Parse` or a direct cast?

**Concepts**
- runtime target type specification
- `IConvertible` requirement
- boxed `object` return value
- dynamic column mapper use case
- static alternative preference for known types

**Answer**

`Convert.ChangeType(object value, Type targetType)` performs a conversion when the target type is not known at compile time. It checks whether the source object implements `IConvertible`, delegates to the appropriate conversion method, and returns a boxed `object` that the caller must cast to the concrete type. A typical use is a generic CSV row mapper that reads column-to-type mappings from a configuration file: `(int)Convert.ChangeType(cellText, typeof(int))` converts a string cell to the column's declared type without a large `if/else` chain. The method works for all primitive types and `string` because they implement `IConvertible`; most custom classes do not and will throw `InvalidCastException`. When the conversion is impossible — for example converting `"hello"` to `int` — it throws `FormatException`. For code where the target type is known at compile time, `Convert.ChangeType` adds boxing and a runtime type check with no benefit over `TryParse` or a direct cast. Reserve it for dynamic pipelines: ORMs mapping loosely typed rows to typed properties, plugin systems translating schema versions, or reflection-driven serializers where the target type comes from metadata.

---

## Q9. What is boxing in C# and what happens at the memory level when a value type is boxed?

**Concepts**
- heap allocation for value type wrapper
- copy from stack into heap object
- independent stack and heap copies
- implicit boxing trigger scenarios
- GC pressure cost

**Answer**

Boxing occurs when a value type is assigned to a variable of type `object` or to an interface it implements. The runtime allocates a new object on the managed heap, copies the value from the stack into that heap object, and stores a reference. The original stack copy and the heap copy are independent — modifying one does not affect the other. The assignment `object boxed = 36` looks trivial but triggers a heap allocation, a GC-tracked reference, and a copy of four bytes. Boxing happens implicitly in common situations: passing a value type to a method accepting `object`, adding a value type to a non-generic `ArrayList`, using a value type in a `string.Format` argument without calling `ToString()` first, and assigning to an interface reference. Each box is a separate heap object, so tight loops boxing thousands of integers per second produce visible GC pressure. Modern C# eliminates most historic boxing scenarios through generics — `List<int>` stores values in-line without boxing — and span-based APIs that accept `ISpanFormattable`. Understanding boxing is foundational to understanding why unboxing requires an exact-type cast and why the `is T n` pattern is preferred over repeated type tests on boxed objects.

---

## Q10. Why must you unbox to the exact stored type and not a compatible widening type?

**Concepts**
- heap type descriptor exact-match rule
- `InvalidCastException` on widening unbox attempt
- widening applies only to variable assignment
- bit-layout copy mechanics
- `is T n` as safe alternative

**Answer**

When a value type is boxed, the heap object stores both the value and a type descriptor identifying the original type — `int`, `byte`, `decimal`, and so on. When you unbox with a cast, the runtime reads that descriptor and verifies it matches the cast target exactly. If the boxed type is `int` and you write `(long)boxedInt`, the runtime finds a mismatch and throws `InvalidCastException`, even though `int` widens to `long` freely in normal assignment. This happens because unboxing is a memory copy: the runtime copies the stored bit pattern into a stack variable of the declared type. An `int` occupies four bytes; a `long` occupies eight; copying four bytes of `int` data into an eight-byte `long` slot requires a numeric promotion step that unboxing does not perform. The widening rule that lets `int` implicitly become `long` in expressions applies only to compile-time variable assignments, not to unboxing. The safe pattern is `if (boxed is int n)` to extract only when the type matches exactly, or casting to `int` first and then widening in a second step: `long result = (int)boxed`. When the exact boxed type is unknown — JSON deserializers may box as `long` or `int` depending on value magnitude — use a multi-branch `is` check or normalize at the deserialization boundary.

---

## Q11. What is the difference between the `as` operator and a direct cast `(T)` for reference type downcasting?

**Concepts**
- `as` returns null on failure
- direct cast throws `InvalidCastException`
- null-check obligation after `as`
- reference and nullable-value types only
- use-case selection rule

**Answer**

A direct cast `(Derived)baseRef` asserts that the runtime object is the target type; if it is not, `InvalidCastException` is thrown immediately at the cast site. The `as` operator `baseRef as Derived` performs the same type check but returns `null` instead of throwing when the check fails — execution continues and the caller decides what null means. The critical obligation of `as` is that the caller must check the result for null before using it. Forgetting this check converts a potential `InvalidCastException` into a `NullReferenceException` at the usage site, which is harder to diagnose because the stack trace points to the usage site rather than the conversion. Use a direct cast when the code logically requires the type to match and a mismatch is a programmer error that should surface loudly — for instance, immediately after an `is` check has already confirmed the type. Use `as` when the type may or may not match and null is a meaningful sentinel for "not that kind of item." Both `as` and the direct cast work with reference types and nullable value types. Writing `obj as int` on a non-nullable value type is a compile error: `as` cannot produce null for a non-nullable type, so the operation is not valid for that form.

---

## Q12. How does `is` declaration pattern matching compare to using `as` followed by a null check?

**Concepts**
- `is T name` combined test-and-assign
- null-state compiler tracking
- scope of the pattern variable
- `switch` expression integration
- single-statement clarity

**Answer**

The `as` plus null-check idiom requires two statements: assign the cast result to a variable, then guard against null before using it. The `is` declaration pattern `if (obj is Derived d)` combines the type test and variable binding into a single expression — when the condition is true, `d` is already initialized to the typed value and is guaranteed non-null within the `if` block. The compiler's null-state analysis tracks this guarantee, so nullable-reference warnings are suppressed inside the block without explicit assertion operators. The pattern also composes naturally with `switch` expressions, `when` guards, and list patterns, making complex type-dispatch logic concise. The `as` idiom is still useful in one narrow case: when you want the cast result in both the true and false branches, since an `as` variable is in scope for the entire containing block. For everything else — single-branch dispatch, interface-type inspection, null checks — the `is` pattern is preferred in modern C# because it eliminates the separate null-guard step and signals to the reader that the variable inside the true block is already typed and non-null.

---

## Q13. What is `IConvertible` and how does it relate to the `Convert` class?

**Concepts**
- `IConvertible` interface contract
- `Convert` class dispatch mechanism
- thirteen declared conversion methods
- primitive types implement it by default
- `Convert.ChangeType` dependency

**Answer**

`IConvertible` is a `System` interface that a type implements to advertise its ability to convert itself to other base types. It declares methods including `ToInt32(IFormatProvider)`, `ToDecimal(IFormatProvider)`, `ToBoolean(IFormatProvider)`, and `ToType(Type, IFormatProvider)`. The `Convert` class is the public facade that consumes this interface: when you call `Convert.ToInt32(someObject)`, `Convert` checks whether `someObject` implements `IConvertible` and, if so, delegates to `someObject.ToInt32(null)`. All C# primitive types — `int`, `double`, `decimal`, `bool`, `char`, `string`, `DateTime` — implement `IConvertible`, which is why `Convert.ToInt32("42")` works on a `string`. Custom types that implement `IConvertible` can participate in `Convert.ChangeType` pipelines without the caller knowing the concrete type. In practice, implementing `IConvertible` on new types is rare because the interface requires thirteen method implementations and custom conversion operators are simpler for type-to-type conversion. The relationship matters most when diagnosing why `Convert.ChangeType` throws `InvalidCastException`: it does so when the source type does not implement `IConvertible`, meaning the conversion is not registered with the `Convert` infrastructure.

---

## Q14. How do you define custom implicit and explicit conversion operators in C#?

**Concepts**
- `static implicit operator` declaration
- `static explicit operator` declaration
- lossless versus lossy semantic signal
- operator defined on source or destination type
- .NET 10 static abstract interface members

**Answer**

Custom conversion operators are declared as `public static implicit operator TargetType(SourceType s)` for conversions that are lossless and should require no cast syntax, and as `public static explicit operator TargetType(SourceType s)` for conversions that may lose precision or require deliberate intent from the caller. An implicit operator lets the compiler insert the conversion automatically: if `Money` declares `implicit operator Money(decimal d)`, then `Money price = 49.99m` compiles without a cast. An explicit operator requires the cast syntax: `decimal raw = (decimal)price` signals that the caller accepts any semantic or precision consequences. The design principle mirrors the built-in numeric rules: implicit operators should be safe and lossless; explicit operators should convey that the caller is making a trade-off. Operators can be declared on either the source or the destination type — you need to own at least one. In .NET 10 with static abstract interface members, conversion operators can be declared in interfaces, enabling generic algorithms to use conversion operators over type parameters constrained to those interfaces. This pattern powers `System.Numerics.INumberBase<T>` and the broader generic math APIs introduced in .NET 7 and extended through .NET 10.

---

## Q15. What is `TryFormat` and how does it differ from `ToString` in allocation behavior?

**Concepts**
- `TryFormat` span-based output
- `stackalloc` zero-heap-allocation buffer
- `bool` return and `out int charsWritten`
- `ISpanFormattable` interface
- high-throughput formatting use case

**Answer**

`TryFormat` is a method on numeric, date, and GUID types that writes a formatted representation directly into a caller-supplied `Span<char>` buffer rather than allocating a new `string` on the heap. Its signature is `bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)`. When the caller provides a `stackalloc char[32]` buffer, the entire operation uses stack memory — no heap allocation occurs until the caller explicitly calls `.ToString()` on the written slice. The method returns `false` if the buffer is too small and writes nothing. By contrast, `value.ToString("F2")` always allocates a new `string` object. For hot paths that format thousands of values per second — log record builders, CSV exporters, network protocol frame writers — avoiding that per-value allocation reduces GC pause frequency measurably. For ordinary business logic and infrequent formatting, `ToString()` with a format specifier is simpler and more readable. The same allocation-free approach is available through `Utf8Formatter` for UTF-8 output and through `string.Create` for cases where a `string` is required but you want to fill it without intermediate allocations.

---

## Gotchas — Type Conversion & Casting (Interview Traps)

---

#### Gotcha 1. (int) cast on double truncates toward zero, not rounds

**Concepts**
- Truncation drops the fractional part
- Math.Round then cast for rounding
- Math.Floor/Ceiling are floor/ceiling
- Convert.ToInt32 rounds ties to even (banker's rounding)

**Answer**

(int)2.9 evaluates to 2, not 3, because the cast truncates by dropping the fractional part. For true rounding, use (int)Math.Round(value) — but be aware that Math.Round uses banker's rounding (round-half-to-even) by default, which means (int)Math.Round(2.5) returns 2, not 3.

---

#### Gotcha 2. Unboxing must exactly match the boxed type — widening conversions do not apply

**Concepts**
- Boxing records the exact CLR type
- (long)boxedInt throws InvalidCastException
- (int)boxedInt then (long) works
- Convert.ToInt64(obj) handles the conversion

**Answer**

If you box an int into an object, you must unbox it back to int before applying any numeric widening. Writing (long)(object)42 throws InvalidCastException at the unboxing step because the stored type is System.Int32, not System.Int64 — the widening conversion from int to long is a compile-time implicit conversion, not a runtime operation.

---

#### Gotcha 3. Convert.ToInt32(null) returns 0; (int)null throws NullReferenceException

**Concepts**
- Convert class handles null for numeric types
- Explicit cast on null throws
- as operator returns null for reference types
- Difference between Convert, cast, and as

**Answer**

Convert.ToInt32(null) is defined to return 0 by design, following the principle that null represents zero for numeric conversions. The explicit cast (int)null throws NullReferenceException because there is no unboxing operation that can produce an int from nothing, making Convert the safer choice when the input might be null.

---

#### Gotcha 4. The as operator returns null for incompatible types — it does not work with value types

**Concepts**
- as returns null instead of throwing for reference types
- as is invalid with non-nullable value types
- as string on non-string produces null
- is pattern preferred in modern C#

**Answer**

Writing obj as string returns null if obj is not a string (or null itself), rather than throwing InvalidCastException. However, as cannot be used with non-nullable value types like int because null is not a valid int — use the is pattern (if obj is int n) or (obj is int) followed by a cast for value type checks.

---

#### Gotcha 5. Implicit numeric widening can unexpectedly select a different overload

**Concepts**
- Compiler applies widening for overload resolution
- short selects int overload over long if only int overload exists
- Adding a long overload can break existing call sites
- Explicit cast forces the intended overload

**Answer**

When a method has overloads for int and long, passing a short selects the int overload because int is the nearest widening target. Adding a new long overload to a library later can silently change which overload is selected for short arguments in consuming code, subtly changing behavior without a compile error.

---

#### Gotcha 6. Checked arithmetic applies to both casts and operators — unchecked is the default context

**Concepts**
- checked block covers arithmetic and explicit casts
- checked((int)longValue) throws for out-of-range
- unchecked silently truncates
- Project-level checked context via /checked compiler flag

**Answer**

The checked keyword enables overflow detection for both arithmetic operations and explicit casts in the same expression. checked((int)longValue) throws OverflowException when longValue exceeds int's range, while the default unchecked context silently truncates the high bits, producing an incorrect negative value with no indication of the problem.

---

#### Gotcha 7. is and as pattern checks are not equivalent to successful cast availability

**Concepts**
- obj is string succeeds only for non-null string
- null is SomeType is always false
- as vs is keyword
- Pattern matching with is produces bound variable

**Answer**

if (obj is string) returns true only for a non-null reference of runtime type string or a subtype. A null reference fails the is check for any type, including string. When you need to both check and use the value, the pattern match form if (obj is string s) is the cleanest — it tests, casts, and declares s in one operation.

---

#### Gotcha 8. string to numeric conversion fails on whitespace, commas, and locale-specific separators

**Concepts**
- int.Parse(" 5 ") throws unless NumberStyles.AllowLeadingWhite is specified
- double.Parse("1,5") fails in en-US locale (comma is not decimal)
- NumberStyles and IFormatProvider overloads
- TryParse with NumberStyles

**Answer**

int.Parse("1 000") fails with FormatException because spaces are not allowed by default in the integer format. double.Parse("1,5") succeeds on a French system where comma is the decimal separator but throws on US machines — always pass CultureInfo.InvariantCulture when parsing numbers from machine-generated strings.

---

#### Gotcha 9. Dynamic dispatch bypasses compile-time widening conversions

**Concepts**
- object o = (int)42; (long)o throws InvalidCastException
- Widening is a compile-time rewrite not a runtime op
- Surprising exception on seemingly valid widening
- Convert.ToInt64(o) uses reflection-based numeric conversion

**Answer**

Writing (long)(object)42 throws InvalidCastException at runtime because widening (int to long) is purely a compile-time transformation that the compiler inserts before boxing. Once the int is boxed as object, the runtime only recognizes its exact boxed type (Int32), and (long) cannot unbox an Int32. Convert.ToInt64 handles this correctly by reading the boxed value and performing a numeric conversion.

---

#### Gotcha 10. Narrowing unsigned types wraps differently from signed — (byte)300 == 44, not -44

**Concepts**
- byte range 0-255; 300 mod 256 = 44 (truncation of high bits)
- Signed narrowing truncates high bits which can produce negative values
- checked context throws for both
- Unsigned behavior differs from signed at negative boundary

**Answer**

Narrowing an unsigned type like (byte)300 produces 44 (300 mod 256 = 44) because unsigned arithmetic truncates high bits and the result is always non-negative. Narrowing a signed type like (sbyte)200 produces -56 because the high bit in the truncated result is set, making the value negative in two's-complement representation — a subtle behavioral difference between signed and unsigned narrowing.

---

## Real-World Scenarios

---

## Q21. (Code Review) A warehouse pricing service receives boxed quantities and tries to unbox them as `long`. What fails at runtime and how would you fix it?

```csharp
// net10.0 — WarehousePricingService.cs
public decimal CalculateLineTotal(object quantityBoxed, decimal unitPrice)
{
    long units = (long)quantityBoxed;   // upstream boxes int.Parse result as object
    if (units <= 0)
        throw new ArgumentException("Quantity must be positive.");
    return units * unitPrice;
}

// Caller in OrderProcessor.cs
object qty = int.Parse(orderDto.QuantityText);  // implicit box — int stored on heap
decimal total = service.CalculateLineTotal(qty, 49.99m);
```

**Concepts**
- exact-type unbox rule
- `InvalidCastException` from wrong unbox target type
- widening not applied during unbox
- strongly typed service boundary
- `is int n` safe unbox pattern

**Answer**

The method throws `InvalidCastException` on every call. The caller boxes the result of `int.Parse` as `object`, which stores an `int` type descriptor on the heap. The service then attempts `(long)quantityBoxed` — unboxing to `long` — but the runtime finds the stored descriptor is `int`, not `long`, and throws. Widening from `int` to `long` applies only to compile-time variable assignment; it does not apply during unboxing, which copies a specific bit layout and validates the type descriptor exactly.

| Category | Problem | Impact |
|---|---|---|
| Runtime | `(long)` unbox on a heap object containing `int` | `InvalidCastException` on every call — pricing fails completely |
| Type model | `object` boundary bypasses the widening rule | Bug is invisible in static analysis; surfaces only at runtime |
| Design | `object` parameter for a scalar quantity | Loses type information at the API boundary, forcing a risky dynamic cast |

**Fix priority:**

1. Unbox to the exact stored type: replace `(long)quantityBoxed` with `(int)quantityBoxed`, or use the pattern `if (quantityBoxed is int n) { … }`.
2. Widen after unbox if `long` is required: `long units = (int)quantityBoxed`.
3. Prefer a strongly typed parameter `int quantityUnits` at the service boundary — eliminate `object` entirely.
4. If the source type varies (JSON deserializers may box as `long` vs `int` depending on value magnitude), use a multi-branch `is` check or normalize at the deserialization boundary before calling the service.

---

## Q22. (Code Review) An ASP.NET Core controller calls `int.Parse` directly on a URL route segment. What is wrong and how do you fix it?

```csharp
// net10.0 — OrdersController.cs
[HttpGet("orders/{quantity}/total")]
public IActionResult GetOrderTotal(string quantity)
{
    int units = int.Parse(quantity);
    if (units <= 0)
        return BadRequest("Quantity must be positive.");

    var total = _pricingService.ComputeTotal(units, _catalog.GetDefaultPrice());
    return Ok(new { units, total });
}
```

**Concepts**
- `Parse` exception on untrusted route input
- `TryParse` as HTTP boundary pattern
- 500 vs 400 status code impact
- route constraint as framework-level alternative
- exception-free validation path

**Answer**

`int.Parse(quantity)` throws `FormatException` for any non-numeric route segment and `OverflowException` for values outside `int` range. ASP.NET Core's exception middleware converts both into 500 responses. The positive-quantity range check on the next line is never reached for non-numeric input because the exception fires first. A client sending a malformed request receives no useful error information, and the server logs an exception for what is a normal client mistake rather than a server fault.

| Category | Problem | Impact |
|---|---|---|
| Runtime | `int.Parse` on unvalidated route segment | `FormatException` or `OverflowException` → 500 instead of 400 |
| API contract | Validation runs only after a potentially throwing parse | Range check is unreachable for non-numeric input |
| Operability | Exception-driven control flow for client errors | Noisy exception logs; hard to distinguish client mistakes from server bugs |

**Fix priority:**

1. Replace `int.Parse` with `int.TryParse` and return `BadRequest` when `false`: `if (!int.TryParse(quantity, out int units)) return BadRequest("Quantity must be a valid integer.");`
2. Alternatively, declare the route parameter as `int` directly with a route constraint: `[HttpGet("orders/{quantity:int}/total")] public IActionResult GetOrderTotal(int quantity)` — the constraint rejects non-integer segments before the action is invoked.
3. Keep the positive range check after successful parse; consider adding an upper bound.
4. Reserve `int.Parse` for configuration constants and trusted data sources, never for request input.

---

## Q23. (Code Review) An inventory service casts `int` location IDs to `byte` for a shelf-slot database column. What corrupts data silently and how do you prevent it?

```csharp
// net10.0 — InventorySlotService.cs
public byte AssignShelfSlot(int inventoryLocationId)
{
    return (byte)inventoryLocationId;
}

public async Task PersistSlotAsync(int locationId, CancellationToken ct)
{
    byte slot = AssignShelfSlot(locationId);
    await _db.ExecuteAsync(
        "UPDATE bins SET slot_id = @slot WHERE location_id = @id",
        new { slot, id = locationId }, cancellationToken: ct);
}
```

**Concepts**
- unchecked narrowing cast wrap-around
- silent data corruption with no exception
- `checked` keyword as fail-fast alternative
- range validation at entry point
- schema widening as permanent fix

**Answer**

The cast `(byte)inventoryLocationId` executes in the default unchecked context. Any location ID above 255 wraps silently: `300` becomes `44` (`300 mod 256`), `512` becomes `0`, `1000` becomes `232`. `PersistSlotAsync` then writes the corrupted slot value to the database, where it appears to be a valid `byte` — no exception is raised anywhere in the pipeline. Inventory picks based on those slot IDs route products to wrong bins. The bug is discovered only when physical counts do not match the system, often days after the bad data was written.

| Category | Problem | Impact |
|---|---|---|
| Correctness | Unchecked `(byte)` cast on values that may exceed 255 | Silent wrap corrupts shelf slot assignments in the database |
| Data integrity | No validation before the SQL write | Corruption propagates to picking, shipping, and audit trails |
| Observability | No exception or log entry on overflow | Root cause is invisible; only downstream physical discrepancy reveals it |

**Fix priority:**

1. Validate range before the cast: `if (inventoryLocationId is < 0 or > 255) throw new ArgumentOutOfRangeException(nameof(inventoryLocationId), "Slot IDs must be 0–255.");`
2. Alternatively, use `checked((byte)inventoryLocationId)` — `OverflowException` on values over 255, which at least fails fast rather than silently corrupting data.
3. If location IDs legitimately exceed 255 in the business domain, widen the database column to `smallint` or `int` and change the property type — the schema fix is more correct than repeated bounded casting.
4. Add unit tests for boundary values 0, 255, 256, and 300 — the 255/256 boundary test immediately reveals the wrap behavior.

---

## Q24. (Code Review) A shipping label builder uses `as` without null-checking the result. What throws and what is the cleaner fix?

```csharp
// net10.0 — LabelBuilder.cs
public string BuildLabel(object lineItem)
{
    if (lineItem is PhysicalLineItem)
        return ((PhysicalLineItem)lineItem).Sku;

    var digital = lineItem as DigitalLineItem;
    return digital.DownloadCode;
}
```

**Concepts**
- `as` returns null on type mismatch
- `NullReferenceException` on unchecked dereference of `as` result
- redundant `is` plus direct cast pattern
- `is T name` declaration pattern
- `switch` expression for exhaustive type dispatch

**Answer**

`lineItem as DigitalLineItem` returns `null` when `lineItem` is neither a `DigitalLineItem` nor `null`. The next line `digital.DownloadCode` then throws `NullReferenceException` because `digital` is `null`. For `null` input, `lineItem as DigitalLineItem` also returns `null`, so the method crashes before producing any output. The `PhysicalLineItem` branch also contains a redundant pattern: it tests with `is` and then immediately casts again with `(PhysicalLineItem)lineItem` — two runtime type checks instead of one, which the `is T name` pattern collapses into a single operation.

| Category | Problem | Impact |
|---|---|---|
| Runtime | `digital.DownloadCode` on a null `as` result | `NullReferenceException` for non-`DigitalLineItem` objects and for null input |
| Style | `is PhysicalLineItem` followed by `(PhysicalLineItem)` cast | Two type checks instead of one; pattern variable is the idiomatic replacement |
| Correctness | No fallback for unrecognized line item types | Method crashes or returns wrong data for any type not explicitly handled |

**Fix priority:**

1. Replace the physical branch with a declaration pattern: `if (lineItem is PhysicalLineItem physical) return physical.Sku;`
2. Guard the digital branch: `if (lineItem is DigitalLineItem digital) return digital.DownloadCode;`
3. Add an explicit fallback for null input and unrecognized types — never fall off the end of a method returning `string`.
4. Prefer a `switch` expression for exhaustive dispatch so the compiler warns when a new line-item type is added without updating the label logic:

```csharp
return lineItem switch
{
    PhysicalLineItem p => p.Sku,
    DigitalLineItem  d => d.DownloadCode,
    null               => throw new ArgumentNullException(nameof(lineItem)),
    _                  => throw new InvalidOperationException(
                              $"Unsupported line item type: {lineItem.GetType().Name}")
};
```

---

## Q25. A partner integration POSTs prices formatted as German decimal strings (`"1.234,56"`) to an en-US server. How do you parse them safely and what API contract prevents the problem entirely?

**Concepts**
- culture-sensitive `decimal.Parse` behavior
- `CultureInfo.InvariantCulture` for machine-to-machine parsing
- `NumberStyles.Number` flag
- JSON numeric type as superior contract
- `TryParse` at HTTP boundary

**Answer**

`decimal.Parse(text)` without an `IFormatProvider` uses the current thread culture, which on an en-US server interprets `.` as the decimal separator and `,` as the thousands separator. The German string `"1.234,56"` would either throw `FormatException` or parse incorrectly depending on `NumberStyles` in effect — neither outcome produces the correct `1234.56`. To parse a German-formatted string on any server regardless of deployment culture, pass the sending culture explicitly: `decimal.Parse(text, NumberStyles.Number, new CultureInfo("de-DE"))`. This tells the parser that `,` is the decimal separator and `.` is the group separator for this specific string.

The more robust fix is to change the API contract. JSON is a language-independent format, and JSON numbers use invariant formatting: `"amount": 1234.56` is unambiguous on any server. Accepting a JSON numeric field eliminates the parse step entirely — the deserializer reads the invariant number directly. If strings cannot be avoided, document and enforce invariant culture formatting in the API specification: `"amountText": "1234.56"`, and parse with `CultureInfo.InvariantCulture`. Combine this with `decimal.TryParse` rather than `decimal.Parse` to return `400 ProblemDetails` with field-level error detail rather than a 500 exception on malformed input. Culture-aware parsing belongs in UI layers where the user's locale is intentional — formatted invoices, address entry, local currency display. HTTP API bodies should use invariant culture or native JSON types to decouple the integration from any server's locale configuration and make the API testable in any environment.

---

## Q26. You are designing a `Money` value type. How would you use custom conversion operators to allow `Money price = 49.99m` but require `(decimal)price` when extracting the raw amount?

**Concepts**
- `static implicit operator` for `decimal → Money`
- `static explicit operator` for `Money → decimal`
- lossless versus lossy semantic signal
- `readonly struct` as value type foundation
- visible extraction as design intent

**Answer**

The design maps directly onto C#'s implicit/explicit conversion operator distinction. Declare `Money` as a `readonly struct` holding a `decimal Amount` and optionally a currency code. An implicit operator from `decimal` to `Money` is appropriate because assigning `49.99m` to `Money` is lossless — every `decimal` value maps to a unique `Money` amount and no precision is lost. The compiler then allows `Money price = 49.99m` without a cast, which reads naturally in domain code.

The explicit operator in the other direction carries deliberate friction. Extracting the raw decimal from a `Money` value discards the currency context — you are making a decision to work with a plain number — and that decision should be visible at the call site: `(decimal)price`. If `price` were accidentally passed where `decimal` was expected, the missing cast would be a compile error, catching the type mismatch before runtime.

```csharp
// net10.0
public readonly struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "USD")
        => (Amount, Currency) = (amount, currency);

    public static implicit operator Money(decimal d) => new(d);
    public static explicit operator decimal(Money m) => m.Amount;
}
```

The net effect is that arithmetic on `decimal` values flows naturally into `Money` through implicit promotion, while any extraction back to a raw number is a visible, intentional act. This mirrors the design of unit-carrying types like `TimeSpan`, which requires explicit construction from raw ticks, and strongly typed identifier types that reject accidental assignment across ID domains. In .NET 10 the same operators can be declared as static abstract members in an interface, enabling generic algorithms over `Money`-like types constrained to that interface.

---

## Q27. A legacy service stores thousands of integers per second in an `ArrayList`, then retrieves them for calculation. How does boxing affect correctness and performance, and what is the migration path?

**Concepts**
- per-element heap allocation in non-generic collection
- GC pressure from boxing in hot paths
- unboxing exact-type requirement
- `List<int>` in-line value storage
- migration from non-generic to generic collections

**Answer**

`ArrayList` stores its elements as `object` references. Every time an `int` is added — `list.Add(counter)` — the runtime boxes the value: it allocates a heap object, copies the four-byte integer into it, and stores a reference. Thousands of additions per second produce thousands of individual heap allocations. The garbage collector must track and collect each box independently, increasing Gen 0 collection frequency. Under sustained load this creates measurable GC pauses.

The correctness issue appears when retrieving: `int n = (int)list[i]` is correct, but `long n = (long)list[i]` throws `InvalidCastException` because the stored type descriptor is `int`, not `long`. Any code that tries to unbox to a different numeric type — even a wider one — fails at runtime, and because the error appears at the retrieval site rather than the insertion site, the root cause is difficult to trace.

The migration path is a one-for-one replacement with `List<int>`. `List<int>` is a generic collection that stores `int` values directly in a managed array without boxing — no heap object per element, no GC pressure per addition. Retrieval returns `int` directly, eliminating the unbox cast and the `InvalidCastException` risk. The substitution is mechanical: change the field type, update `Add` call sites if needed, and remove the explicit casts at retrieval. For callers that receive the list through a legacy `IEnumerable<object>` interface, introduce an adapter at the boundary that streams typed `int` values without converting back to `ArrayList`. Performance gains from eliminating per-element boxing are consistently significant in benchmarks involving millions of value-type elements, and the change simplifies the code by removing all cast syntax at retrieval sites — two wins from a single type change.
