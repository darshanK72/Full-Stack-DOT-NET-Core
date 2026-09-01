# 01. C# Basics - Done — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. Hello World](#01-hello-world)
  - [Q1. What is C# and what are its key features?](#q1-what-is-c-and-what-are-its-key-features)
  - [Q2. Explain namespaces in C#.](#q2-explain-namespaces-in-c)
  - [Q3. How do nested namespaces work in C#?](#q3-how-do-nested-namespaces-work-in-c)
  - [Q4. What is the purpose of the `using` directive (importing namespaces)?](#q4-what-is-the-purpose-of-the-using-directive-importing-namespaces)
  - [Q5. Explain preprocessor directives in C# (`#if`, `#define`, `#region`, `#pragma`, etc.).](#q5-explain-preprocessor-directives-in-c-if-define-region-pragma-etc)
  - [Q6. What is the role of the `Main` method, and how has entry-point syntax evolved (classic `Main`, top-level statements)?](#q6-what-is-the-role-of-the-main-method-and-how-has-entry-point-syntax-evolved-classic-main-top-level-statements)
  - [Q7. What is the difference between a project, a solution, and an assembly in a .NET workspace?](#q7-what-is-the-difference-between-a-project-a-solution-and-an-assembly-in-a-net-workspace)
  - [Q8. What does the `global using` directive do (C# 10+), and when is it useful?](#q8-what-does-the-global-using-directive-do-c-10-and-when-is-it-useful)
  - [Q9. Explain file-scoped namespaces (`namespace X;`) vs block-scoped namespace syntax.](#q9-explain-file-scoped-namespaces-namespace-x-vs-block-scoped-namespace-syntax)
  - [Q10. What is the purpose of `Program.cs` in a console application, and what other files typically accompany it (`.csproj`, `global usings`)?](#q10-what-is-the-purpose-of-programcs-in-a-console-application-and-what-other-files-typically-accompany-it-csproj-global-usings)
  - [Q11. What is the Common Language Runtime (CLR), and how does C# code become executable?](#q11-what-is-the-common-language-runtime-clr-and-how-does-c-code-become-executable)
  - [Q12. What is the difference between compiling to IL and JIT compilation at runtime?](#q12-what-is-the-difference-between-compiling-to-il-and-jit-compilation-at-runtime)
  - [Q13. What are SDK-style projects, and what does `<TargetFramework>` in the `.csproj` control?](#q13-what-are-sdk-style-projects-and-what-does-targetframework-in-the-csproj-control)
  - [Q14. When would you use `#nullable enable` at the project or file level?](#q14-when-would-you-use-nullable-enable-at-the-project-or-file-level)
  - [Q15. What is the difference between `Console.Out`, `Console.Error`, and writing directly with `Console.WriteLine`?](#q15-what-is-the-difference-between-consoleout-consoleerror-and-writing-directly-with-consolewriteline)

- [02. Data Types & Variables](#02-data-types-variables)
  - [Q1. What are the different data types in C#?](#q1-what-are-the-different-data-types-in-c)
  - [Q2. What are value types and reference types in C#?](#q2-what-are-value-types-and-reference-types-in-c)
  - [Q3. What is the difference between value types and reference types?](#q3-what-is-the-difference-between-value-types-and-reference-types)
  - [Q4. What is boxing and unboxing in C#?](#q4-what-is-boxing-and-unboxing-in-c)
  - [Q5. Explain the `var` keyword in C#.](#q5-explain-the-var-keyword-in-c)
  - [Q6. What are nullable types in C#? (including nullable reference types in C# 8+)](#q6-what-are-nullable-types-in-c-including-nullable-reference-types-in-c-8)
  - [Q7. Explain the `default` keyword and default values in C#.](#q7-explain-the-default-keyword-and-default-values-in-c)
  - [Q8. What are constants, literals, and readonly fields in C#?](#q8-what-are-constants-literals-and-readonly-fields-in-c)
  - [Q9. What is the difference between `const` and `readonly`?](#q9-what-is-the-difference-between-const-and-readonly)
  - [Q10. What is an enum in C#?](#q10-what-is-an-enum-in-c)
  - [Q11. What is a `struct` in C#? (basics — comparison with `class` is in OOP)](#q11-what-is-a-struct-in-c-basics-comparison-with-class-is-in-oop)
  - [Q12. What is a tuple in C#? (ValueTuple vs `Tuple<T>`)](#q12-what-is-a-tuple-in-c-valuetuple-vs-tuplet)
  - [Q13. Where do value types typically live (stack vs heap), and where do reference types live?](#q13-where-do-value-types-typically-live-stack-vs-heap-and-where-do-reference-types-live)
  - [Q14. When a value type is boxed, where does the data end up, and why does that matter for performance?](#q14-when-a-value-type-is-boxed-where-does-the-data-end-up-and-why-does-that-matter-for-performance)
  - [Q15. What is the difference between `int`, `long`, `decimal`, `float`, and `double` — when would you choose each?](#q15-what-is-the-difference-between-int-long-decimal-float-and-double-when-would-you-choose-each)
  - [Q16. What is the difference between signed and unsigned integer types (`int` vs `uint`, etc.)?](#q16-what-is-the-difference-between-signed-and-unsigned-integer-types-int-vs-uint-etc)

- [03. Input & Output](#03-input-output)
  - [Q1. What is the difference between `Console.WriteLine`, `Console.Write`, and string interpolation for output?](#q1-what-is-the-difference-between-consolewriteline-consolewrite-and-string-interpolation-for-output)
  - [Q2. What is the difference between `Console.ReadLine()` and `Console.ReadKey()`?](#q2-what-is-the-difference-between-consolereadline-and-consolereadkey)
  - [Q3. How do you safely parse user input (`int.TryParse`, `Parse`, `Convert`) and handle invalid input?](#q3-how-do-you-safely-parse-user-input-inttryparse-parse-convert-and-handle-invalid-input)
  - [Q4. How does formatted console output work (`Console.WriteLine("{0}", value)` vs interpolation)?](#q4-how-does-formatted-console-output-work-consolewriteline0-value-vs-interpolation)
  - [Q5. What is the difference between `CultureInfo.CurrentCulture`, `CurrentUICulture`, and `InvariantCulture`?](#q5-what-is-the-difference-between-cultureinfocurrentculture-currentuiculture-and-invariantculture)
  - [Q6. When should you use `InvariantCulture` for formatting numbers and dates instead of `CurrentCulture`?](#q6-when-should-you-use-invariantculture-for-formatting-numbers-and-dates-instead-of-currentculture)
  - [Q7. How do culture settings affect decimal separators, currency symbols, and date formats in console output?](#q7-how-do-culture-settings-affect-decimal-separators-currency-symbols-and-date-formats-in-console-output)
  - [Q8. What is composite formatting (`string.Format`, `{0:N2}`, alignment `{0,10}`, `{0,-10}`)?](#q8-what-is-composite-formatting-stringformat-0n2-alignment-010-0-10)
  - [Q9. What is the difference between `Console.InputEncoding` and `Console.OutputEncoding`, and why can mismatched encodings garble console text?](#q9-what-is-the-difference-between-consoleinputencoding-and-consoleoutputencoding-and-why-can-mismatched-encodings-garble-console-text)
  - [Q10. How do you capture console output programmatically (e.g., `StringWriter` redirected to `Console.SetOut`)?](#q10-how-do-you-capture-console-output-programmatically-eg-stringwriter-redirected-to-consolesetout)
  - [Q11. What is the difference between `Console.Read` and `Console.ReadLine`?](#q11-what-is-the-difference-between-consoleread-and-consolereadline)
  - [Q12. How do you format output with alignment and padding using interpolation (`$"{value,10}"`, `$"{value:N2}"`)?](#q12-how-do-you-format-output-with-alignment-and-padding-using-interpolation-value10-valuen2)
  - [Q13. What is `IFormattable`, and how does it relate to custom formatting in `ToString(format, provider)`?](#q13-what-is-iformattable-and-how-does-it-relate-to-custom-formatting-in-tostringformat-provider)
  - [Q14. What happens if you call `int.Parse` on invalid input vs `int.TryParse` — which pattern is preferred in production console apps?](#q14-what-happens-if-you-call-intparse-on-invalid-input-vs-inttryparse-which-pattern-is-preferred-in-production-console-apps)
  - [Q15. How does changing `CultureInfo.CurrentCulture` on the current thread affect subsequent formatting calls that omit an explicit provider?](#q15-how-does-changing-cultureinfocurrentculture-on-the-current-thread-affect-subsequent-formatting-calls-that-omit-an-explicit-provider)
  - [Q16. What is `NumberFormatInfo`, and how does it differ from `CultureInfo`?](#q16-what-is-numberformatinfo-and-how-does-it-differ-from-cultureinfo)

- [04. Operators & Expressions](#04-operators-expressions)
  - [Q1. What are the different types of operators in C#? (Arithmetic, Relational, Logical, Bitwise, Assignment, Ternary, Null-coalescing, etc.)](#q1-what-are-the-different-types-of-operators-in-c-arithmetic-relational-logical-bitwise-assignment-ternary-null-coalescing-etc)
  - [Q2. Explain the `checked` and `unchecked` keywords in C#.](#q2-explain-the-checked-and-unchecked-keywords-in-c)
  - [Q3. What is the difference between `==` and `.Equals()` for value types vs reference types?](#q3-what-is-the-difference-between-and-equals-for-value-types-vs-reference-types)
  - [Q4. What is integer division in C#, and how do you get a fractional result?](#q4-what-is-integer-division-in-c-and-how-do-you-get-a-fractional-result)
  - [Q5. Explain operator precedence and associativity — why does `a + b * c` evaluate differently than `(a + b) * c`?](#q5-explain-operator-precedence-and-associativity-why-does-a-b-c-evaluate-differently-than-a-b-c)
  - [Q6. What is the difference between prefix and postfix increment (`++i` vs `i++`)?](#q6-what-is-the-difference-between-prefix-and-postfix-increment-i-vs-i)
  - [Q7. What are short-circuit logical operators (`&&`, `||`), and why do they matter beyond boolean logic?](#q7-what-are-short-circuit-logical-operators-and-why-do-they-matter-beyond-boolean-logic)
  - [Q8. Explain the null-conditional operator (`?.`) and null-coalescing operators (`??`, `??=`).](#q8-explain-the-null-conditional-operator-and-null-coalescing-operators)
  - [Q9. What are bitwise operators (`&`, `|`, `^`, `~`, `<<`, `>>`), and when are they used in application code?](#q9-what-are-bitwise-operators-and-when-are-they-used-in-application-code)
  - [Q10. What is the difference between logical AND (`&&`) and bitwise AND (`&`) when applied to `bool` operands?](#q10-what-is-the-difference-between-logical-and-and-bitwise-and-when-applied-to-bool-operands)
  - [Q11. What is the ternary conditional operator (`?:`), and how does it differ from an `if/else` statement?](#q11-what-is-the-ternary-conditional-operator-and-how-does-it-differ-from-an-ifelse-statement)
  - [Q12. What is the difference between `is` pattern matching and a simple boolean expression in a condition?](#q12-what-is-the-difference-between-is-pattern-matching-and-a-simple-boolean-expression-in-a-condition)
  - [Q13. When does overflow occur for integer arithmetic, and how do `checked` blocks change behavior?](#q13-when-does-overflow-occur-for-integer-arithmetic-and-how-do-checked-blocks-change-behavior)
  - [Q14. What is the difference between `==` and `ReferenceEquals` for reference types?](#q14-what-is-the-difference-between-and-referenceequals-for-reference-types)
  - [Q15. Can you overload operators in C# — which operators can and cannot be overloaded?](#q15-can-you-overload-operators-in-c-which-operators-can-and-cannot-be-overloaded)
  - [Q16. What is the difference between compound assignment (`+=`, `-=`) and the expanded form (`x = x + y`) for value vs reference types?](#q16-what-is-the-difference-between-compound-assignment---and-the-expanded-form-x-x-y-for-value-vs-reference-types)

- [05. Type Conversion & Casting](#05-type-conversion-casting)
  - [Q1. What is the difference between the `is` and `as` operators?](#q1-what-is-the-difference-between-the-is-and-as-operators)
  - [Q2. What is the difference between implicit and explicit type conversion (casting)?](#q2-what-is-the-difference-between-implicit-and-explicit-type-conversion-casting)
  - [Q3. What is the difference between `Convert.ToInt32`, `(int)`, and `int.Parse`?](#q3-what-is-the-difference-between-converttoint32-int-and-intparse)
  - [Q4. When does a cast succeed at compile time but fail at runtime?](#q4-when-does-a-cast-succeed-at-compile-time-but-fail-at-runtime)
  - [Q5. What is widening vs narrowing conversion — which direction is implicit?](#q5-what-is-widening-vs-narrowing-conversion-which-direction-is-implicit)
  - [Q6. What is the difference between `Parse`, `TryParse`, and `Convert.ChangeType`?](#q6-what-is-the-difference-between-parse-tryparse-and-convertchangetype)
  - [Q7. When would you use the `is` pattern with a declaration (`if (obj is int n)`) vs a traditional cast?](#q7-when-would-you-use-the-is-pattern-with-a-declaration-if-obj-is-int-n-vs-a-traditional-cast)
  - [Q8. What exception types are commonly thrown by failed casts and parses (`FormatException`, `OverflowException`, `InvalidCastException`)?](#q8-what-exception-types-are-commonly-thrown-by-failed-casts-and-parses-formatexception-overflowexception-invalidcastexception)
  - [Q9. What is `TryFormat`, and how does writing into a `Span<char>` differ from calling `ToString()`?](#q9-what-is-tryformat-and-how-does-writing-into-a-spanchar-differ-from-calling-tostring)
  - [Q10. How does culture affect parsing and formatting during type conversion (e.g., `"1,234.56"` vs `"1.234,56"`)?](#q10-how-does-culture-affect-parsing-and-formatting-during-type-conversion-eg-123456-vs-123456)
  - [Q11. What is the difference between boxing during conversion to `object` and a direct numeric cast?](#q11-what-is-the-difference-between-boxing-during-conversion-to-object-and-a-direct-numeric-cast)
  - [Q12. When is the `as` operator preferred over a cast, and what does it return on failure?](#q12-when-is-the-as-operator-preferred-over-a-cast-and-what-does-it-return-on-failure)
  - [Q13. What is user-defined explicit/implicit conversion operator syntax (preview level)?](#q13-what-is-user-defined-explicitimplicit-conversion-operator-syntax-preview-level)
  - [Q14. What happens when you cast a `double` to `int` — is rounding or truncation applied?](#q14-what-happens-when-you-cast-a-double-to-int-is-rounding-or-truncation-applied)
  - [Q15. What is the difference between `default(T)` casting patterns and `Convert` methods for nullable value types?](#q15-what-is-the-difference-between-defaultt-casting-patterns-and-convert-methods-for-nullable-value-types)
  - [Q16. When converting between `string` and numeric types in APIs and logs, why is `InvariantCulture` often specified explicitly?](#q16-when-converting-between-string-and-numeric-types-in-apis-and-logs-why-is-invariantculture-often-specified-explicitly)

- [06. Control Flow & Loops](#06-control-flow-loops)
  - [Q1. What is the difference between `if/else` and the ternary operator?](#q1-what-is-the-difference-between-ifelse-and-the-ternary-operator)
  - [Q2. What is the difference between traditional `switch` and switch expressions (C# 8+)?](#q2-what-is-the-difference-between-traditional-switch-and-switch-expressions-c-8)
  - [Q3. When should you use `for`, `foreach`, `while`, and `do-while`?](#q3-when-should-you-use-for-foreach-while-and-do-while)
  - [Q4. What is the difference between `break`, `continue`, and `return` inside a loop?](#q4-what-is-the-difference-between-break-continue-and-return-inside-a-loop)
  - [Q5. What are common pitfalls with nested loops and loop variable scope?](#q5-what-are-common-pitfalls-with-nested-loops-and-loop-variable-scope)
  - [Q6. What is a switch expression, and how do relational and property patterns work in `switch`?](#q6-what-is-a-switch-expression-and-how-do-relational-and-property-patterns-work-in-switch)
  - [Q7. What is the difference between `break` in a `switch` vs `break` in a loop?](#q7-what-is-the-difference-between-break-in-a-switch-vs-break-in-a-loop)
  - [Q8. When is `goto` still used in C# (e.g., `goto case`, `goto default`), and why is it generally discouraged?](#q8-when-is-goto-still-used-in-c-eg-goto-case-goto-default-and-why-is-it-generally-discouraged)
  - [Q9. What is the scope of a variable declared in the initializer of a `for` loop (C# rules)?](#q9-what-is-the-scope-of-a-variable-declared-in-the-initializer-of-a-for-loop-c-rules)
  - [Q10. Why did C# 5 change loop variable capture semantics in lambdas, and how does that affect `foreach` vs `for`?](#q10-why-did-c-5-change-loop-variable-capture-semantics-in-lambdas-and-how-does-that-affect-foreach-vs-for)
  - [Q11. Can you modify the collection you are iterating in a `foreach` loop — what exception results?](#q11-can-you-modify-the-collection-you-are-iterating-in-a-foreach-loop-what-exception-results)
  - [Q12. What is the difference between `while` and `do-while` when the condition is false on the first check?](#q12-what-is-the-difference-between-while-and-do-while-when-the-condition-is-false-on-the-first-check)
  - [Q13. When would you prefer a `switch` over a chain of `if/else if` statements?](#q13-when-would-you-prefer-a-switch-over-a-chain-of-ifelse-if-statements)
  - [Q14. What is pattern matching with `is` in an `if` statement vs a `switch` on type?](#q14-what-is-pattern-matching-with-is-in-an-if-statement-vs-a-switch-on-type)
  - [Q15. What happens if you use `return` inside a `try` block that has a `finally` — which executes first?](#q15-what-happens-if-you-use-return-inside-a-try-block-that-has-a-finally-which-executes-first)

- [07. Methods](#07-methods)
  - [Q1. What are the `out`, `ref`, and `in` parameter modifiers? Explain their usage.](#q1-what-are-the-out-ref-and-in-parameter-modifiers-explain-their-usage)
  - [Q2. What is the `params` keyword in method definitions?](#q2-what-is-the-params-keyword-in-method-definitions)
  - [Q3. What are expression-bodied members in C#?](#q3-what-are-expression-bodied-members-in-c)
  - [Q4. Explain named arguments and optional parameters in C#.](#q4-explain-named-arguments-and-optional-parameters-in-c)
  - [Q5. What are local functions in C#?](#q5-what-are-local-functions-in-c)
  - [Q6. What is the `yield` keyword and iterators in C#? *(Cross-ref: Module 03 — IEnumerable)*](#q6-what-is-the-yield-keyword-and-iterators-in-c-cross-ref-module-03-ienumerable)
  - [Q7. Explain method overloading — what makes two methods overloads vs duplicate definitions?](#q7-explain-method-overloading-what-makes-two-methods-overloads-vs-duplicate-definitions)
  - [Q8. How does overload resolution work when multiple overloads could apply — what is the "better function member" rule?](#q8-how-does-overload-resolution-work-when-multiple-overloads-could-apply-what-is-the-better-function-member-rule)
  - [Q9. Why can't you overload methods by return type alone?](#q9-why-cant-you-overload-methods-by-return-type-alone)
  - [Q10. What is the difference between call-by-value for value types vs reference types at the parameter boundary?](#q10-what-is-the-difference-between-call-by-value-for-value-types-vs-reference-types-at-the-parameter-boundary)
  - [Q11. When should you use `ref` vs `out` vs `in` for parameters?](#q11-when-should-you-use-ref-vs-out-vs-in-for-parameters)
  - [Q12. What problem does the `in` modifier solve for large readonly structs?](#q12-what-problem-does-the-in-modifier-solve-for-large-readonly-structs)
  - [Q13. What is the Try-pattern (`bool TryX(..., out T result)`), and why is it preferred over exceptions for expected failures?](#q13-what-is-the-try-pattern-bool-tryx-out-t-result-and-why-is-it-preferred-over-exceptions-for-expected-failures)
  - [Q14. Can optional parameters precede required parameters — what are the ordering rules?](#q14-can-optional-parameters-precede-required-parameters-what-are-the-ordering-rules)
  - [Q15. What is the difference between `params int[]` and passing an explicit `int[]` at the call site?](#q15-what-is-the-difference-between-params-int-and-passing-an-explicit-int-at-the-call-site)
  - [Q16. When does overload resolution fail with ambiguity (CS0121), and how do casts or named arguments resolve it?](#q16-when-does-overload-resolution-fail-with-ambiguity-cs0121-and-how-do-casts-or-named-arguments-resolve-it)

- [08. Strings](#08-strings)
  - [Q1. Explain string handling in C# (`string` vs `StringBuilder`).](#q1-explain-string-handling-in-c-string-vs-stringbuilder)
  - [Q2. What are the different ways to format strings in C#? (`String.Format`, interpolation, composite formatting)](#q2-what-are-the-different-ways-to-format-strings-in-c-stringformat-interpolation-composite-formatting)
  - [Q3. Are strings mutable or immutable in C#? What are the implications?](#q3-are-strings-mutable-or-immutable-in-c-what-are-the-implications)
  - [Q4. What is string interning?](#q4-what-is-string-interning)
  - [Q5. What is the difference between `==`, `Equals`, `Compare`, and `CompareTo` for strings?](#q5-what-is-the-difference-between-equals-compare-and-compareto-for-strings)
  - [Q6. When should you use `StringComparison.Ordinal` vs `OrdinalIgnoreCase` vs culture-sensitive comparisons?](#q6-when-should-you-use-stringcomparisonordinal-vs-ordinalignorecase-vs-culture-sensitive-comparisons)
  - [Q7. What are verbatim string literals (`@"..."`), and when are they useful?](#q7-what-are-verbatim-string-literals-and-when-are-they-useful)
  - [Q8. What are raw string literals (`"""..."""`, C# 11+), and how do they handle quotes and newlines?](#q8-what-are-raw-string-literals-c-11-and-how-do-they-handle-quotes-and-newlines)
  - [Q9. What is the difference between `StringBuilder` and repeated string concatenation in a loop?](#q9-what-is-the-difference-between-stringbuilder-and-repeated-string-concatenation-in-a-loop)
  - [Q10. What is the difference between `string.Concat`, the `+` operator, and interpolation for combining text?](#q10-what-is-the-difference-between-stringconcat-the-operator-and-interpolation-for-combining-text)
  - [Q11. What is the difference between `IsNullOrEmpty`, `IsNullOrWhiteSpace`, and checking `Length == 0`?](#q11-what-is-the-difference-between-isnullorempty-isnullorwhitespace-and-checking-length-0)
  - [Q12. What is the difference between culture-sensitive (`ToUpper()`) and invariant (`ToUpperInvariant()`) case conversion?](#q12-what-is-the-difference-between-culture-sensitive-toupper-and-invariant-toupperinvariant-case-conversion)
  - [Q13. What methods would you use to split, trim, replace, pad, and search within strings?](#q13-what-methods-would-you-use-to-split-trim-replace-pad-and-search-within-strings)
  - [Q14. What is UTF-16 storage in .NET strings, and how does that relate to surrogate pairs and `char`?](#q14-what-is-utf-16-storage-in-net-strings-and-how-does-that-relate-to-surrogate-pairs-and-char)
  - [Q15. What is the string intern pool, and what does `string.Intern` do?](#q15-what-is-the-string-intern-pool-and-what-does-stringintern-do)
  - [Q16. Why can two strings with identical content fail `ReferenceEquals` while still passing `==`?](#q16-why-can-two-strings-with-identical-content-fail-referenceequals-while-still-passing)

- [09. Arrays](#09-arrays)
  - [Q1. What are arrays in C#? How is memory managed for single-dimensional, multi-dimensional, and jagged arrays?](#q1-what-are-arrays-in-c-how-is-memory-managed-for-single-dimensional-multi-dimensional-and-jagged-arrays)
  - [Q2. What is a jagged array?](#q2-what-is-a-jagged-array)
  - [Q3. What is the difference between `Array.Copy()`, `Clone()`, and assigning one array variable to another?](#q3-what-is-the-difference-between-arraycopy-clone-and-assigning-one-array-variable-to-another)
  - [Q4. What is the difference between a single-dimensional array, a rectangular multi-dimensional array (`[,]`), and a jagged array (`[][]`)?](#q4-what-is-the-difference-between-a-single-dimensional-array-a-rectangular-multi-dimensional-array-and-a-jagged-array)
  - [Q5. Are arrays value types or reference types in C#?](#q5-are-arrays-value-types-or-reference-types-in-c)
  - [Q6. What is array covariance for reference types, and why is `object[] arr = new string[3]; arr[0] = 42;` dangerous?](#q6-what-is-array-covariance-for-reference-types-and-why-is-object-arr-new-string3-arr0-42-dangerous)
  - [Q7. What do `Array.Resize`, `Array.Fill`, and `Array.Clear` do — which allocate new memory?](#q7-what-do-arrayresize-arrayfill-and-arrayclear-do-which-allocate-new-memory)
  - [Q8. What is the difference between `Length` on a single-dimensional array vs `GetLength(dimension)` on multi-dimensional arrays?](#q8-what-is-the-difference-between-length-on-a-single-dimensional-array-vs-getlengthdimension-on-multi-dimensional-arrays)
  - [Q9. How do you initialize arrays with collection initializer syntax and `new int[] { 1, 2, 3 }`?](#q9-how-do-you-initialize-arrays-with-collection-initializer-syntax-and-new-int-1-2-3)
  - [Q10. What is the relationship between arrays and `params` parameters in methods?](#q10-what-is-the-relationship-between-arrays-and-params-parameters-in-methods)
  - [Q11. What is the difference between shallow copy of an array reference and copying array elements?](#q11-what-is-the-difference-between-shallow-copy-of-an-array-reference-and-copying-array-elements)
  - [Q12. When would you use `Array.Sort` vs LINQ `OrderBy` on an array?](#q12-when-would-you-use-arraysort-vs-linq-orderby-on-an-array)
  - [Q13. What bounds-checking behavior does C# provide for array indexing?](#q13-what-bounds-checking-behavior-does-c-provide-for-array-indexing)
  - [Q14. What is `Span<T>`/`ReadOnlySpan<T>` in relation to arrays (preview — stack-friendly views)?](#q14-what-is-spantreadonlyspant-in-relation-to-arrays-preview-stack-friendly-views)
  - [Q15. How do jagged arrays differ in memory layout from rectangular 2D arrays?](#q15-how-do-jagged-arrays-differ-in-memory-layout-from-rectangular-2d-arrays)
  - [Q16. What happens when you pass an array to a method — can the callee change the caller's array contents?](#q16-what-happens-when-you-pass-an-array-to-a-method-can-the-callee-change-the-callers-array-contents)

- [10. Exception Handling](#10-exception-handling)
  - [Q1. Explain exception handling in C# (`try`, `catch`, `finally`, `throw`, and custom exceptions).](#q1-explain-exception-handling-in-c-try-catch-finally-throw-and-custom-exceptions)
  - [Q2. What is the difference between `throw` and `throw ex`?](#q2-what-is-the-difference-between-throw-and-throw-ex)
  - [Q3. Explain the `using` statement in the context of exception handling and resource management.](#q3-explain-the-using-statement-in-the-context-of-exception-handling-and-resource-management)
  - [Q4. What are exception filters in C#?](#q4-what-are-exception-filters-in-c)
  - [Q5. What is the difference between catching a specific exception type vs `catch (Exception)`?](#q5-what-is-the-difference-between-catching-a-specific-exception-type-vs-catch-exception)
  - [Q6. What happens if an exception is thrown inside a `finally` block?](#q6-what-happens-if-an-exception-is-thrown-inside-a-finally-block)
  - [Q7. What is the base class hierarchy for exceptions in .NET (`Exception`, `SystemException`, application-specific types)?](#q7-what-is-the-base-class-hierarchy-for-exceptions-in-net-exception-systemexception-application-specific-types)
  - [Q8. When should you create a custom exception type vs using an existing BCL exception?](#q8-when-should-you-create-a-custom-exception-type-vs-using-an-existing-bcl-exception)
  - [Q9. What is the difference between `using` statement and `using` declaration (`using var`) for disposal?](#q9-what-is-the-difference-between-using-statement-and-using-declaration-using-var-for-disposal)
  - [Q10. Can you have multiple `catch` blocks — what is the order rule for catching derived vs base exceptions?](#q10-can-you-have-multiple-catch-blocks-what-is-the-order-rule-for-catching-derived-vs-base-exceptions)
  - [Q11. What is `finally` guaranteed to do, and can it prevent an exception from propagating?](#q11-what-is-finally-guaranteed-to-do-and-can-it-prevent-an-exception-from-propagating)
  - [Q12. What is the difference between handled exceptions and unhandled exceptions in a console vs ASP.NET host?](#q12-what-is-the-difference-between-handled-exceptions-and-unhandled-exceptions-in-a-console-vs-aspnet-host)
  - [Q13. When is it appropriate to catch and swallow an exception vs rethrow?](#q13-when-is-it-appropriate-to-catch-and-swallow-an-exception-vs-rethrow)
  - [Q14. What is `ExceptionDispatchInfo`, and when is `throw;` insufficient?](#q14-what-is-exceptiondispatchinfo-and-when-is-throw-insufficient)
  - [Q15. What happens if both `try` and `finally` contain `return` statements?](#q15-what-happens-if-both-try-and-finally-contain-return-statements)
  - [Q16. What is the difference between `IDisposable.Dispose` and finalizers in exception-safe cleanup?](#q16-what-is-the-difference-between-idisposabledispose-and-finalizers-in-exception-safe-cleanup)
  - [Q30. **`checked` default is context-dependent** — Integer overflow wraps silently in unchecked default contexts; financial code may need explicit `checked` blocks.](#q30-checked-default-is-context-dependent-integer-overflow-wraps-silently-in-unchecked-default-contexts-financial-code-may-need-explicit-checked-blocks)

---

### 01. Hello World

#### Q1. What is C# and what are its key features?

**Concepts**
- Statically typed, object-oriented language targeting the .NET platform
- Compilation to Intermediate Language (IL) executed by the CLR
- Multi-paradigm: OOP, functional, and LINQ-based styles
- Annual language versioning with backward compatibility
- Garbage-collected memory management

**Answer**

C# is a modern, statically typed, object-oriented programming language designed for the .NET platform, where source code is compiled to Intermediate Language (IL) and executed by the Common Language Runtime (CLR). It combines C-style syntax with garbage-collected memory management, strong typing, and a rich standard library so you can build console apps, services, desktop clients, and web APIs from the same language.

- C# is multi-paradigm: you write classes and interfaces for object-oriented design, but also use delegates, lambdas, and Language Integrated Query (LINQ) for functional-style data processing.
- The language evolves with the runtime through annual releases, adding features such as nullable reference types, pattern matching, records, and top-level statements while keeping backward compatibility within a target framework.
- As a .NET language, C# interoperates with other CLI languages (F#, VB.NET) because all of them target the same assembly model defined by ECMA-335.
- Key practical features include exception handling, generics, async/await for non-blocking I/O, and compile-time safety that catches many errors before the program runs.

---

#### Q2. Explain namespaces in C#.

**Concepts**
- Hierarchical naming container preventing type-name collisions
- Fully qualified type names combining namespace path and type name
- Logical organization independent of physical folder layout
- Base Class Library (BCL) namespace hierarchy

**Answer**

A namespace is a hierarchical naming container that groups related types and prevents name collisions when two libraries define types with the same simple name. You declare a namespace in source code, and the fully qualified name of a type combines the namespace path with the type name (for example, `System.Console`).

- Namespaces do not dictate physical folder layout or assembly boundaries by themselves; they are a logical organization tool, though teams often mirror folder structure for readability.
- The root `System` namespace and its children (`System.Collections.Generic`, `System.IO`, and others) form the Base Class Library (BCL) surface you import with `using` directives.
- Two different assemblies can expose types in the same namespace, and the compiler resolves types by namespace plus assembly reference at build time.
- Without namespaces, every type name would have to be globally unique across all referenced libraries, which would be impractical in large ecosystems.

---

#### Q3. How do nested namespaces work in C#?

**Concepts**
- Parent-child hierarchy via nested blocks or dotted notation
- Dot notation as syntactic sugar for nested namespace declarations
- Lexical scoping independent of access modifier rules
- Domain partitioning without separate assembly boundaries

**Answer**

Nested namespaces express a parent-child hierarchy either by nesting `namespace` blocks inside one another or by using dotted names such as `Company.Product.Feature`, which the compiler treats as nested namespace declarations.

- Dot notation is syntactic sugar: `namespace A.B.C` is equivalent to nesting `namespace A { namespace B { namespace C { ... } } }`.
- Types inside a nested namespace are referenced with the full dotted path unless a `using` directive or alias shortens the name at the top of the file.
- Nested namespaces help large teams partition domains (for example, `MyApp.Services` vs `MyApp.Models`) without creating separate assemblies for every slice.
- The nesting is purely lexical; it does not automatically grant access to `internal` members of types in parent namespaces—access modifiers still follow class and assembly rules.

---

#### Q4. What is the purpose of the `using` directive (importing namespaces)?

**Concepts**
- Compile-time namespace import for unqualified type resolution
- Using aliases for disambiguation between conflicting simple names
- Assembly reference versus namespace import separation

**Answer**

The `using` directive tells the compiler to search specified namespaces when resolving unqualified type names, so you can write `Console.WriteLine` instead of the fully qualified `System.Console.WriteLine`. It affects compile-time name lookup only and does not copy code or change runtime behavior.

- A single file typically lists several `using` lines for namespaces whose types it references, keeping member access readable while the compiler still resolves to the same IL regardless of how many usings you add.
- You can also define `using` aliases (`using IO = System.IO;`) to disambiguate when two namespaces expose conflicting simple names.
- Removing a `using` does not remove the dependency on the underlying assembly; you must still reference the project or NuGet package that contains the type.
- Over-importing unused namespaces is harmless at runtime but may trigger analyzer warnings; add usings when the file actually needs types from that namespace.

---

#### Q5. Explain preprocessor directives in C# (`#if`, `#define`, `#region`, `#pragma`, etc.).

**Concepts**
- Compile-time symbol definitions and conditional compilation
- #region / #endregion for IDE code folding
- #pragma warning disable for analyzer and nullable tuning
- #line for source generator line remapping

**Answer**

Preprocessor directives are compile-time instructions processed before semantic analysis; they conditionally include or exclude code, define symbols, organize editor regions, or suppress warnings without changing runtime semantics of the code that remains.

- `#define` and `#undef` create or remove conditional compilation symbols (often combined with `#if`, `#elif`, `#else`, `#endif`) to build debug-only logging, platform-specific branches, or feature flags.
- `#region` / `#endregion` fold code blocks in the IDE for navigation; they do not affect generated IL and should not replace meaningful structure.
- `#pragma warning disable` / `restore` and `#nullable enable` / `disable` tune analyzer and nullable reference type behavior for a file or a span of code.
- `#line` can remap reported line numbers when generating source, which matters for source generators and tooling rather than everyday application code.

---

#### Q6. What is the role of the `Main` method, and how has entry-point syntax evolved (classic `Main`, top-level statements)?

**Concepts**
- CLR entry-point method invoked at process startup
- Single entry-point requirement per executable project
- Top-level statements as compiler-synthesized Main (C# 9+)
- Async entry-point support with Task and Task<int> return types

**Answer**

`Main` is the application entry point—the method the Common Language Runtime (CLR) invokes after loading the assembly to begin execution. Classic console apps declare `public static void Main(string[] args)` (or an equivalent return type/int signature), while C# 9+ allows top-level statements that the compiler synthesizes into a hidden `Main` method.

- The runtime requires exactly one entry point per executable project; duplicate `Main` methods produce compile error CS0017.
- `string[] args` receives command-line tokens split by the host; `args[0]` is the first argument after the program name (host-dependent).
- Top-level statements must appear in one file per project, before any type declarations, and are translated to a generated `Program` class with a `Main` method—ideal for scripts and small tools.
- `Main` can return `int` for process exit codes or be `async Task` / `async Task<int>` in modern templates so the entry point can await asynchronous work.

---

#### Q7. What is the difference between a project, a solution, and an assembly in a .NET workspace?

**Concepts**
- Solution as multi-project container and build orchestration unit
- Project as compilation unit producing one output assembly
- Assembly as deployable IL + metadata unit loaded by the CLR
- Project references for inter-assembly dependency resolution

**Answer**

A solution (`.sln`) is a container that groups related projects for IDE and build orchestration; a project (`.csproj`) is the build unit that compiles source into one primary output assembly (and copies dependencies); an assembly is the compiled `.dll` or `.exe` containing IL and metadata that the CLR loads at runtime.


- One solution commonly holds a web API project, a class library, and test projects, each producing its own assembly.
- Project references tell the compiler where to find dependent assemblies; at runtime the loader resolves them from the output folder or NuGet cache.
- The executable project's assembly contains the entry point; class library projects produce `.dll` files referenced by hosts.

---

#### Q8. What does the `global using` directive do (C# 10+), and when is it useful?

**Concepts**
- Project-wide namespace import eliminating per-file using lines
- ImplicitUsings SDK feature generating GlobalUsings.g.cs
- Global using aliases for project-wide short names

**Answer**

A `global using` directive imports a namespace for every source file in the project (or a defined subset), eliminating repeated `using System.Collections.Generic;` lines across dozens of files. The compiler treats it as if each file included that `using` at the top.

- SDK-style projects often generate `GlobalUsings.g.cs` when `<ImplicitUsings>enable</ImplicitUsings>` is set, adding common BCL namespaces automatically for console, web, and class library templates.
- You can add a `GlobalUsings.cs` file with `global using MyCompany.Shared;` so domain types resolve everywhere without per-file imports.
- `global using` aliases work too (`global using Json = System.Text.Json;`), giving a project-wide short name.
- Use global usings for truly common imports; keep file-specific or rare namespaces local to avoid hiding where a type originates.

---

#### Q9. Explain file-scoped namespaces (`namespace X;`) vs block-scoped namespace syntax.

**Concepts**
- File-scoped namespace syntax reducing indentation nesting
- Block-scoped namespace for multiple namespaces in one file
- Identical IL output from both syntax forms
- C# 10+ requirement for file-scoped syntax

**Answer**

File-scoped namespace syntax (`namespace MyApp;`) declares that all types in the file belong to `MyApp` without wrapping the entire file in an extra indentation level of braces, while block-scoped syntax (`namespace MyApp { ... }`) explicitly delimits the namespace with a code block.

- Both forms produce identical IL; the choice is readability and editor ergonomics, especially in files with a single namespace.
- File-scoped namespaces require C# 10 or later and must appear before other members; only one file-scoped namespace is allowed per file.
- Block-scoped namespaces still allow multiple namespaces in one file (unusual) and remain required when nesting multiple namespace levels with different members in the same file.
- Teams migrating legacy code often keep block syntax until they standardize on file-scoped style for new files.

---

#### Q10. What is the purpose of `Program.cs` in a console application, and what other files typically accompany it (`.csproj`, `global usings`)?

**Concepts**
- Entry-point file holding Main or top-level statements
- .csproj project file declaring target framework and references
- GlobalUsings.cs for project-wide namespace imports
- SDK-style build artifact directories (bin/ and obj/)

**Answer**

`Program.cs` holds the entry-point logic—either an explicit `Main` method or top-level statements—that starts the console application. It is the file developers open first to trace startup flow, though the runtime ultimately executes the compiled assembly described by the project file.

- The `.csproj` file defines the target framework (`<TargetFramework>net8.0</TargetFramework>`), output type (`Exe` vs `Library`), nullable settings, implicit usings, and package references.
- `GlobalUsings.cs` or generated global usings centralize namespace imports; some repos disable implicit usings while learning so every import is visible in source.
- Additional files include `appsettings.json` in larger apps, `AssemblyInfo` (often SDK-generated), and optional `Usings.cs` for project-wide aliases.
- In SDK-style projects, build artifacts land in `bin/` and `obj/` folders; source stays minimal with `Program.cs` plus types split into other `.cs` files as the app grows.

---

#### Q11. What is the Common Language Runtime (CLR), and how does C# code become executable?

**Concepts**
- CLR (CoreCLR) as managed execution engine
- Roslyn compiler producing IL and metadata assemblies
- JIT compilation of IL to native machine code on first invocation
- GC, threading, and exception services provided by the runtime
- Tiered compilation for startup and throughput optimization

**Answer**

The Common Language Runtime (CLR)—CoreCLR in modern .NET—is the managed execution engine that loads assemblies, verifies type safety where applicable, Just-In-Time (JIT) compiles IL to native machine code, and provides garbage collection, threading, and exception services. C# source is compiled by Roslyn into IL and metadata stored in a `.dll` or `.exe`, which the host (`dotnet` CLI or a native apphost) starts by loading the CLR.

1. **Compile** — The C# compiler translates `.cs` files into IL instructions and embeds type metadata in a portable executable assembly.
2. **Launch** — The host reads the target framework from the `.runtimeconfig.json` and loads CoreCLR into the process.
3. **Load** — The loader reads the assembly manifest, resolves references from the output directory, and constructs runtime types from metadata.
4. **JIT** — On first method invocation, the JIT compiler translates IL to CPU-specific native code and caches it for later calls.
5. **Execute** — The processor runs native instructions while the CLR manages memory, exceptions, and thread scheduling.

See Module questions on IL vs JIT (Q12) for the distinction between build-time IL emission and runtime native compilation.

---

#### Q12. What is the difference between compiling to IL and JIT compilation at runtime?

**Concepts**
- Build-time IL compilation to portable CPU-neutral bytecode
- Runtime JIT translation of IL to processor-specific native code
- Tiered compilation for hot-path optimization
- ReadyToRun and Native AOT as JIT alternatives

**Answer**

Compiling C# to IL happens at build time and produces portable, CPU-neutral bytecode plus metadata in an assembly, while JIT compilation happens at runtime when the CLR translates each method's IL into native machine instructions for the actual processor executing the process.

- IL compilation is done by Roslyn (or another C# compiler); the output is the same regardless of whether the app runs on x64, ARM64, or another supported architecture.
- JIT runs on first use of each method (with tiered compilation optimizing hot paths over time), so startup pays compilation cost lazily rather than ahead of time for every method.
- Alternatives such as ReadyToRun embed precompiled native images for faster startup, and Native Ahead-of-Time (AOT) compilation avoids JIT entirely for trimmed deployments.
- IL keeps assemblies compact and language-neutral; JIT enables processor-specific optimizations that would be impossible to bake in fully at build time on every target machine.

---

#### Q13. What are SDK-style projects, and what does `<TargetFramework>` in the `.csproj` control?

**Concepts**
- SDK-style .csproj with implicit file globbing and MSBuild SDK defaults
- Target Framework Moniker (TFM) selecting API surface and runtime
- Multi-targeting with <TargetFrameworks> for cross-platform libraries
- NuGet compatibility and language feature availability tied to TFM

**Answer**

SDK-style projects use a concise `.csproj` that begins with `<Project Sdk="Microsoft.NET.Sdk">` and relies on MSBuild SDK defaults to glob `.cs` files automatically, replacing the verbose legacy Framework project format. The `<TargetFramework>` element (or `<TargetFrameworks>` for multi-targeting) sets the Target Framework Moniker (TFM), which selects the API surface and runtime the compiler assumes.

- `net8.0` targets modern .NET 8 with its full BCL; `net48` targets .NET Framework 4.8; `netstandard2.0` targets the portable API contract.
- The TFM controls which NuGet packages are compatible, which language features are available, and which runtime must be installed to execute the built output.
- SDK-style projects integrate NuGet restore, implicit usings, and `dotnet build` / `dotnet run` without hand-maintaining file lists.
- Changing TFM can enable or disable APIs—migrating from `net472` to `net8.0` unlocks modern libraries but may require code changes for removed APIs.

---

#### Q14. When would you use `#nullable enable` at the project or file level?

**Concepts**
- Nullable reference type (NRT) compile-time analysis
- string vs string? as compile-time null-contract annotations
- Incremental opt-in migration via file-level pragma
- Compiler warning emission versus runtime enforcement distinction

**Answer**

`#nullable enable` turns on nullable reference type (NRT) analysis so the compiler warns when you assign `null` to a non-nullable reference type or dereference a value that might be null. You apply it project-wide via `<Nullable>enable</Nullable>` in the `.csproj` or per file with `#nullable enable` at the top when migrating legacy code incrementally.

- NRT annotations (`string` vs `string?`) are compile-time contracts; the runtime still allows null references unless you enforce checks in code.
- Enable nullable when writing new code or refactoring modules where null-related bugs (`NullReferenceException`) are costly, such as public APIs and service layers.
- File-level enable lets you modernize one class at a time in a large codebase without fixing every warning in a single change.
- Pair NRT with defensive patterns (`??`, null-conditional `?.`, `ArgumentNullException.ThrowIfNull`) so warnings reflect actual runtime guarantees.

---

#### Q15. What is the difference between `Console.Out`, `Console.Error`, and writing directly with `Console.WriteLine`?

**Concepts**
- Standard output (stdout) versus standard error (stderr) streams
- Console.Out and Console.Error as redirectable TextWriter properties
- Console.SetOut and Console.SetError for test output capture
- Separate pipe destinations for automation and CI systems

**Answer**

`Console.WriteLine` writes to standard output (`stdout`) through the `Console.Out` `TextWriter`, while error messages intended for diagnostic streams should go to `Console.Error`, which maps to `stderr`. Both are static properties you can redirect, but the convenience methods on `Console` target `Out` by default.

- Operating systems and hosts can pipe `stdout` and `stderr` separately; logging frameworks and CI systems often capture stderr for errors while treating stdout as normal program output.
- `Console.SetOut` and `Console.SetError` replace the writers—for example, redirecting output to a `StringWriter` in tests—without changing call sites that use `Console.WriteLine`.
- `Console.WriteLine` is equivalent to `Console.Out.WriteLine`; there is no separate `Console` buffer—it's a facade over the underlying writers.
- Use `Console.Error.WriteLine` for failure messages when stdout is consumed by another process (piping, automation) so errors remain visible on the error stream.

---

### 02. Data Types & Variables

#### Q1. What are the different data types in C#?

**Concepts**
- Value types (int, bool, struct, enum) storing data inline
- Reference types (class, string, array) holding heap references
- Nullable value types (int?) adding null to value types
- Built-in numeric type range from sbyte through ulong and decimal
- Special types: object, dynamic, nint/nuint

**Answer**

C# data types fall into value types (stored inline with their data, such as `int`, `bool`, `char`, and `struct`) and reference types (variables hold a reference to heap objects, such as `string`, arrays, and `class` instances). The type system also includes pointer-like `unsafe` types, generic type parameters, and special types like `dynamic` and `object`.

- Built-in numeric types span signed and unsigned integers (`sbyte` through `ulong`), floating-point (`float`, `double`), high-precision decimal (`decimal`), and platform-sized `nint`/`nuint`.
- Reference types include `string`, delegates, interfaces, arrays, and user-defined classes; all reference types inherit from `System.Object`.
- Nullable value types (`int?`, `bool?`) wrap value types so they can represent an absent value with `null`.
- Enumerations (`enum`) and tuples (`(int Id, string Name)` or `ValueTuple`) model named constants and lightweight multi-value groupings respectively.

---

#### Q2. What are value types and reference types in C#?

**Concepts**
- Value type copy semantics on assignment
- Reference type shared-object semantics on assignment
- System.ValueType inheritance for value types
- Nullable<T> wrapper enabling null for value types
- Heap allocation for reference type objects

**Answer**

Value types store their data directly in the variable's storage location, so assigning one variable to another copies the bits of the value. Reference types store a reference (address) to an object on the managed heap, so assignment copies the reference while both variables may point to the same object.

- Structs and enum underlying types are value types; classes, interfaces (as references to implementing objects), strings, and arrays are reference types.
- Value types cannot be `null` unless wrapped in `Nullable<T>` (`int?`); reference types default to `null`.
- Value types inherit from `System.ValueType` (which inherits `object`); reference type variables always refer to heap objects with object header and method table.
- Understanding the distinction drives correct equality semantics, parameter passing behavior, and performance (heap allocation vs stack/local storage).

---

#### Q3. What is the difference between value types and reference types?

**Concepts**
- Copy-on-assignment for value types versus pointer copy for reference types
- Zero-bit default for value types versus null default for reference types
- Mutation isolation for value type copies
- Stack versus heap storage as a teaching model

**Answer**

The practical difference is what gets copied on assignment and where mutable state lives: value types copy data, reference types copy pointers to shared heap objects. Method parameters and returns follow the same rules unless modified by `ref`, `out`, or `in`.


See Q2 for definitions; see Q13 for stack vs heap nuance and Q4 for boxing when value types meet reference contexts.

---

#### Q4. What is boxing and unboxing in C#?

**Concepts**
- Boxing as heap allocation wrapping a value type in an object
- Unboxing requiring exact type-match cast
- Performance cost of allocation and GC pressure from boxing
- Non-generic collections (ArrayList) as common boxing source
- Generic collections eliminating boxing for value types

**Answer**

Boxing converts a value type instance into a reference-type `object` (or interface) by copying the value onto the heap and wrapping it in a boxed object, while unboxing extracts the value type back from that object with an explicit cast. Both operations have allocation and type-check costs.

- Boxing occurs when you assign an `int` to `object`, call a non-generic collection's `Add(1)`, or invoke an interface method on a struct through the interface reference.
- Unboxing requires an explicit cast to the exact value type (`(int)obj`); wrong types throw `InvalidCastException`.
- Repeated boxing in hot loops (for example, storing many integers in `ArrayList`) causes garbage collection pressure—prefer generic collections like `List<int>`.
- Nullable value types box as either `null` or a boxed underlying value, not a boxed `Nullable<T>` wrapper.

---

#### Q5. Explain the `var` keyword in C#.

**Concepts**
- Compile-time type inference from initializer expression
- Strong static typing preserved after inference
- Mandatory initializer for var declarations
- Readability trade-offs for long generic type names

**Answer**

`var` instructs the compiler to infer the variable's type from the initializer expression at compile time, producing the same strong typing as an explicit declaration. Once inferred, the variable remains that fixed type—you cannot later assign an incompatible type.

- `var x = 10;` is compiled as `int x = 10;`; `var` is not `dynamic` and does not defer type checking to runtime.
- You must initialize `var` variables in the declaration because the compiler has no expression from which to infer a type.
- `var` improves readability for long generic types (`Dictionary<string, List<Order>>`) but can obscure intent when the initializer is unclear.

---

#### Q6. What are nullable types in C#? (including nullable reference types in C# 8+)

**Concepts**
- Nullable<T> (int?) extending value types with a null state
- HasValue / Value / GetValueOrDefault on nullable value types
- Nullable reference type annotations as compile-time contracts
- Compile-time-only enforcement of NRT analysis
- Null-forgiving operator (!) for external guarantees

**Answer**

Nullable value types (`int?`, `bool?`, etc.) extend value types with a `HasValue` flag so they can represent missing data, while nullable reference types (`string?` vs `string`) add compile-time annotations indicating whether a reference may be null under `#nullable enable`.

- `int?` is shorthand for `Nullable<int>` and supports `null`, `.Value`, and `.GetValueOrDefault()`.
- Nullable reference types do not change runtime behavior—the CLR still allows null on any reference; the compiler emits warnings when flow analysis cannot prove safety.
- Use nullable value types for optional numeric or date fields in databases and APIs; use NRT for documenting optional strings and navigation properties.
- The null-forgiving operator (`!`) suppresses warnings when you have external guarantees the compiler cannot see.

---

#### Q7. Explain the `default` keyword and default values in C#.

**Concepts**
- Zero-initialized default for value types (0, false, '\0')
- Null default for reference types and nullable types
- default(T) in generics applying uniform zero-initialization
- Context-inferred default without explicit type argument

**Answer**

The `default` keyword produces the type's zero-initialized value: numeric zero, `false` for `bool`, `\0` for `char`, and `null` for reference types and nullable types. `default(T)` in generics applies the same rule for any type parameter `T`.

- Local variables must be assigned before use; `default` is common when you need a placeholder before conditional assignment.
- Struct fields initialize to default before constructors run unless field initializers override them.
- `default` for `int?` is a null nullable with no value; for `string` it is `null`.

---

#### Q8. What are constants, literals, and readonly fields in C#?

**Concepts**
- Numeric literal suffixes (L, M, F, D) for type selection
- const as compile-time constant embedded in metadata
- readonly fields assigned at declaration or in constructors
- String literal interning in metadata

**Answer**

Literals are source-code representations of fixed values (`42`, `"hi"`, `3.14m`), constants (`const`) are compile-time constants embedded in metadata, and `readonly` fields are assigned at declaration or in the constructor but can differ per instance or per run.

- Integer literals default to `int`; suffixes (`L`, `M`, `F`, `D`) select `long`, `decimal`, `float`, or `double`.
- `const` values must be computable at compile time and are implicitly static.
- `readonly` instance fields can be set in the constructor, enabling per-object configuration that constants cannot express.
- String literals live in the metadata and may be interned; see Module 01 Strings chapter for immutability implications.

---

#### Q9. What is the difference between `const` and `readonly`?

**Concepts**
- const requiring compile-time constant expression
- readonly supporting runtime-computed values in constructors
- const as implicitly static; readonly as instance or static
- Type restrictions: const limited to primitives, string, null
- Per-instance readonly versus shared const value

**Answer**

`const` fields must be compile-time constants and are implicitly static, while `readonly` fields can be set at runtime in instance or static constructors and may hold values computed when the program runs.


Use `const` for true symbolic constants; use `readonly` for configuration loaded at startup (see Gotcha 3 on `DateTime.Now`).

---

#### Q10. What is an enum in C#?

**Concepts**
- Named integral constants with readable identifiers
- Underlying integral type (default int, configurable)
- [Flags] attribute for bitwise combinable enums
- Enum.Parse and Enum.TryParse for string conversion

**Answer**

An enumeration defines a named set of integral constants sharing one underlying type (default `int`), giving readable names to magic numbers such as days of the week or HTTP status categories. Enums are value types backed by their underlying integer.

- By default, the first member is 0 unless you assign explicit values; unspecified members increment by one.
- You can specify `: byte`, `: long`, or other integral underlying types to control size and interoperability.
- `[Flags]` enums combine bitwise values (`Read | Write`) and typically use powers of two for members.
- Enums convert implicitly to their underlying type and can be parsed from strings with `Enum.Parse` or `Enum.TryParse`.

---

#### Q11. What is a `struct` in C#? (basics — comparison with `class` is in OOP)

**Concepts**
- Value type encapsulating fields and methods inline
- System.ValueType inheritance without polymorphic inheritance
- Copy-on-assignment behavior for structs
- Parameterless constructor support from C# 10
- Performance trade-offs for large struct copying

**Answer**

A `struct` is a value type that encapsulates fields and methods inline, suitable for small, immutable or frequently copied data bundles where heap allocation should be avoided. Structs inherit from `System.ValueType` and do not support inheritance beyond interfaces.

- Structs are copied on assignment and passed by value unless you use `ref` or `in` modifiers.
- Parameterless constructors are supported from C# 10 onward with explicit field initialization rules; earlier versions relied on implicit zero initialization.
- Large structs can hurt performance when copied repeatedly; classes may be better for mutable or large state.
- Module 02 covers struct vs class trade-offs in depth (storage, identity, polymorphism).

---

#### Q12. What is a tuple in C#? (ValueTuple vs `Tuple<T>`)

**Concepts**
- ValueTuple as mutable struct with named element syntax
- Tuple<T1,T2> as legacy reference-type heap allocation
- Deconstruction and Item1/Item2 element access
- Value equality comparing element values for ValueTuple
- Named record or class preference for public APIs

**Answer**

Tuples group multiple values into one lightweight value without defining a named class. C# 7+ value tuples (`(int Id, string Name)`) are mutable structs with named fields, while `Tuple<T1,T2,...>` is a reference-type class from earlier APIs.

- Value tuples use `(1, "Ada")` syntax, deconstruction, and `Item1`/`Item2` or custom element names for readability.
- `Tuple.Create` allocates on the heap and is legacy; prefer value tuples for local multi-return scenarios.
- Value tuples are value types—assignment copies all elements; equality compares element values when types match.
- Use a named record or class when the grouping becomes part of a public API with behavior and invariants.

---

#### Q13. Where do value types typically live (stack vs heap), and where do reference types live?

**Concepts**
- Stack frame or register storage for local value type variables
- Managed heap for reference type objects
- Value types embedded in classes or arrays residing on heap
- Boxing moving value type copies onto the heap
- CLR optimization of stack model through registers and closures

**Answer**

Local value type variables and parameters usually live in stack frames or CPU registers, while reference type objects always live on the managed heap with the variable holding a reference. Value types embedded in classes or boxed into `object` also reside on the heap as part of the containing object or box.

- The CLR may optimize away the stack entirely through registers and may allocate value types on the heap when they escape the method (closures, async state machines).
- Arrays of value types store elements contiguously on the heap inside the array object; the array variable itself is a reference.
- "Stack vs heap" is a teaching model; the accurate rule is value types copy by value, reference types identify heap objects by reference.

---

#### Q14. When a value type is boxed, where does the data end up, and why does that matter for performance?

**Concepts**
- Heap allocation copying value type bits into a boxed object
- Object header and method table overhead per box
- GC pressure and cache misses from allocation churn
- Interface dispatch on structs triggering boxing
- Non-generic collection boxing versus generic collection avoidance

**Answer**

Boxing copies the value type's bits into a new heap object with an object header and method table pointer, so the original stack or inline value is separate from the boxed copy. Each box is a heap allocation that the garbage collector must eventually reclaim.

- Interface calls on structs often box because the dispatch requires a reference to an object implementing the interface.
- Non-generic collections (`ArrayList`, `Hashtable`) box every value type element on insertion.
- Hot-path boxing causes allocation churn, increased GC pauses, and cache misses compared to generic `List<T>` or span-based APIs.
- Mutating the struct after boxing does not change the boxed copy—another reason to avoid unintended boxing.

---

#### Q15. What is the difference between `int`, `long`, `decimal`, `float`, and `double` — when would you choose each?

**Concepts**
- Fixed-point integers (int, long) for counting and indexing
- Binary floating-point (float, double) for scientific math
- Base-10 decimal for financial calculations avoiding binary rounding
- Type size and range constraints per numeric type

**Answer**

`int` and `long` are fixed-point integers for counting and indexing; `float` and `double` are binary floating-point types for scientific and graphics math; `decimal` is a 128-bit base-10 floating type designed for financial calculations where binary rounding errors are unacceptable.


Choose `decimal` for currency; choose `double` for physics simulations; choose integers when fractional parts are impossible by domain rules.

---

#### Q16. What is the difference between signed and unsigned integer types (`int` vs `uint`, etc.)?

**Concepts**
- Two's complement signed representation for negative values
- Unsigned types doubling positive range at same bit width
- Signed/unsigned mixing rules in expressions
- char as unsigned 16-bit UTF-16 code unit

**Answer**

Signed integers (`sbyte`, `short`, `int`, `long`) represent negative and positive values using two's complement, while unsigned types (`byte`, `ushort`, `uint`, `ulong`) represent zero and positive values only, doubling the positive range at the same bit width.

- `int` ranges approximately ±2.1 billion; `uint` ranges 0 to ~4.3 billion at 32 bits.
- Mixing signed and unsigned in expressions follows C# promotion rules and can surprise you when comparing across signs.
- Use unsigned types for bit masks, hash codes, and protocols that define unsigned fields—not as a trick to get "more positive int" without domain justification.
- `char` is unsigned 16-bit for UTF-16 code units, not a signed numeric type in practice.

---

#### Q17. What is `char` in C# — is it a numeric type or a text type, and how does it relate to Unicode?

**Concepts**
- 16-bit UTF-16 code unit value type
- Surrogate pair encoding for supplementary Unicode characters
- Arithmetic promotion to int when used in expressions
- Rune type for full Unicode scalar values

**Answer**

`char` is a 16-bit Unicode code unit (UTF-16) value type, technically numeric under the hood but used primarily to represent a single text element or half of a surrogate pair. It is not a full Unicode grapheme or string.

- Literal `'A'` is a `char`; strings `"A"` are reference types of length one or more UTF-16 code units.
- Characters outside the Basic Multilingual Plane encode as surrogate pairs—two `char` values in one `string`.
- Arithmetic on `char` is legal (promotes to `int`) but rarely appropriate except for range checks or custom encodings.
- For full Unicode text, use `string`; for code-point-level work, consider `Rune` (.NET Core 3+) or UTF-8 APIs.

---

#### Q18. What is the difference between `bool` and nullable `bool?` in terms of default values and usage?

**Concepts**
- bool defaulting to false with definite true/false semantics
- bool? with three states: true, false, null (unknown)
- Lifted operators on nullable booleans
- InvalidOperationException on .Value when null

**Answer**

`bool` defaults to `false` and always holds `true` or `false`, while `bool?` defaults to `null` meaning "unknown or not specified" in addition to `true` and `false`.

- Use `bool?` for optional form fields (tri-state checkbox) or database columns that allow NULL.
- `bool?` has `.HasValue` and `.Value`; accessing `.Value` when null throws `InvalidOperationException`.
- Nullable booleans participate in lifted operators (`bool? a = true; bool? b = null; var c = a & b;` yields `null`).
- Prefer `bool` when the domain requires a definite yes/no with no missing state.

---

#### Q19. Explain the `??` (null-coalescing) and `??=` (null-coalescing assignment) operators with nullable types.

**Concepts**
- ?? returning left operand when non-null otherwise right operand
- ??= assigning right only when left is currently null
- Lazy initialization pattern with ??=
- Right-side short-circuit evaluation
- Left-to-right chaining for multiple fallback values

**Answer**

The `??` operator returns the left operand when it is not null; otherwise it evaluates and returns the right operand. The `??=` operator assigns the right side to the left variable only when the left is currently null.

- `name ?? "Guest"` yields `"Guest"` when `name` is null—common for defaults on nullable reference and value types.
- `cache ??= new Dictionary<string, string>();` lazily initializes `cache` once without repeating null checks.
- The right side of `??` is not evaluated unless the left is null, which matters when the fallback is expensive or has side effects.
- Chaining (`a ?? b ?? c`) walks left to right until a non-null value appears.

---

#### Q20. What is the difference between `var` and an explicit type declaration — when must you use explicit types?

**Concepts**
- Identical IL from var and explicit declarations when types match
- Mandatory explicit type when no initializer present
- string? requiring explicit type because var cannot infer null
- Interface or base-class variable typing beyond inferred concrete type

**Answer**

`var` and explicit declarations produce identical IL when the inferred type matches; the choice is readability. You must use an explicit type when there is no initializer, when the initializer is `null` without a target-typed context, or in public member signatures where `var` is not permitted.

- `var list = new List<int>();` is idiomatic; `List<int> list = new();` target-typed `new` also works from C# 9.
- `string? name = null;` must name the type because `var name = null;` does not compile.
- Interface or base-class typing often requires explicit types: `IEnumerable<int> ids = GetIds();` even if `var` could infer `List<int>`.

---

#### Q21. What are digit separators in numeric literals (e.g., `1_000_000`), and what problem do they solve?

**Concepts**
- Underscore character improving readability in numeric literals
- Compiler-ignored separator with no runtime effect
- Support across integer, floating-point, binary, and hex literals
- Pairing with explicit type suffixes for large constants

**Answer**

Digit separators are underscore characters placed inside numeric literals to improve human readability; the compiler ignores them, so `1_000_000` equals `1000000`. They work in integer, floating, binary (`0b1010_0001`), and hex literals.

- They reduce transcription errors when reading large constants such as file sizes, bit masks, or financial limits.
- You cannot place two underscores adjacent or lead/trail an underscore in ways the grammar forbids (for example, `_100` is invalid).
- Separators do not affect runtime type or value—only source readability.
- They pair well with explicit suffixes (`1_000_000L`, `3.14_15_92d`) in configuration and test data.

---

#### Q22. What is the difference between `default(int)` and `default` for a reference type?

**Concepts**
- default(T) yielding type's zero value uniformly
- 0 for int, null for reference types and nullable types
- Context-inferred default without explicit type argument
- Generic library usage of default(T) across value and reference types

**Answer**

Both forms yield the type's zero value: `default(int)` is `0`, and `default(string)` (or `default` for any reference type) is `null`. `default(T)` in generic code applies the same rule uniformly without knowing `T` at source-writing time.

- `default` without a type argument is inferred from context in declarations like `int x = default;`.
- For nullable value types, `default(int?)` is null with `HasValue == false`.
- There is no behavioral difference between `default(string)` and `null`—they are the same constant concept.
- Use `default` in generic libraries to initialize locals and return slots when `T` might be value or reference type.

---

#### Q23. What is a nullable reference type annotation (`string?` vs `string`), and is enforcement compile-time or runtime?

**Concepts**
- Nullable reference type annotations as compile-time contracts
- Compiler flow analysis tracking potential null dereferences
- [NotNullWhen] and related attributes for complex patterns
- Runtime behavior unchanged — CLR still allows null on any reference

**Answer**

Under nullable reference type analysis, `string` means the author intends a non-null reference and `string?` allows null, with the compiler emitting warnings when assignments and dereferences violate those intentions. Enforcement is compile-time only—the runtime does not distinguish `string` from `string?`.

- Flow analysis tracks whether a variable might be null after conditionals (`if (s is not null)`).
- Attributes like `[NotNullWhen(true)]` on `Try` methods improve analysis for patterns the compiler cannot infer alone.
- Without `#nullable enable`, annotations are ignored for warning purposes though they document intent.
- Runtime null checks still require explicit code; NRT prevents many bugs early but is not a runtime guard.

---

#### Q24. What happens when you assign `null` to a non-nullable reference type variable under `#nullable enable`?

**Concepts**
- CS8625/CS8600 compiler warning on null assignment to non-nullable
- Compile-only enforcement — runtime still allows null assignment
- Null-forgiving operator (!) suppressing warnings with documented justification
- Design feedback: fix annotation or fix assignment logic

**Answer**

The compiler emits a warning (typically CS8625 or CS8600 depending on context) because you promised the variable should not hold null, yet you assigned null anyway. The code still compiles unless warnings are treated as errors, and the assignment succeeds at runtime like any reference assignment.

- Suppress only with documented justification (`!`, `#pragma`, or `[AllowNull]` attributes on APIs).
- Fixing the warning means changing the type to nullable, ensuring initialization in all paths, or using a non-null default (`string.Empty`).
- Treat warnings as design feedback: either the annotation or the assignment logic is wrong.

---

#### Q25. What is the difference between `object` as a universal base type and using `dynamic`?

**Concepts**
- object as universal base type requiring cast for member access
- dynamic deferring member resolution to DLR at runtime
- RuntimeBinderException on dynamic member-not-found failures
- Boxing value types assigned to object
- COM interop and truly dynamic payload scenarios for dynamic

**Answer**

`object` is the root reference type for all managed types; assigning a value boxes value types and requires casts or pattern matching to use specific members after storage as `object`. `dynamic` defers member resolution to runtime via Dynamic Language Runtime (DLR) binding, skipping compile-time member checking.

- `object o = 42; int n = (int)o;` needs explicit unboxing.
- `dynamic d = GetSomething(); d.AnyMethod();` compiles even if `AnyMethod` might not exist—failures become runtime `RuntimeBinderException`.
- Use `object` for heterogeneous collections and reflection scenarios with explicit type tests.
- Use `dynamic` sparingly for COM interop and truly dynamic payloads; prefer strong typing or `JsonSerializer` elsewhere.

---

#### Q26. What are `nint` and `nuint`, and when might you encounter them?

**Concepts**
- Native-sized signed and unsigned integers matching pointer width
- System.IntPtr and System.UIntPtr aliases
- P/Invoke signature compatibility with C intptr_t / size_t
- Platform-width arithmetic avoiding explicit pointer-sized casts

**Answer**

`nint` and `nuint` are native-sized signed and unsigned integer types whose width matches a pointer on the platform (32-bit on 32-bit processes, 64-bit on 64-bit processes). They alias `System.IntPtr` and `System.UIntPtr` for interop and low-level pointer arithmetic.

- Common in P/Invoke signatures mirroring C `intptr_t`/`size_t` and in `Span<T>` length scenarios tied to platform limits.
- Arithmetic on `nint` avoids casting pointers to `long` on 64-bit while remaining correct on 32-bit.
- They are value types but not considered "built-in" in the same sense as `int`; use when size must track pointer width.
- Unsafe code and interop layers are the typical application domains—not general business logic.

---

#### Q27. What is the difference between declaring a variable with and without an initializer?

**Concepts**
- Initializer assigning value at declaration time
- Definite assignment analysis requiring initialization before read
- Fields receiving default values without explicit initializers
- CS0165 compiler error for uninitialized local variable use

**Answer**

A declaration with an initializer (`int count = 0;`) assigns a value at definition time, while a declaration without an initializer (`int count;`) leaves the variable in an uninitialized state until assigned—legal for locals only after definite assignment analysis proves use before assignment is impossible.

- Fields receive default values even without initializers (`0`, `null`, `false`).
- Locals must be assigned on every code path before read; the compiler error CS0165 catches violations.
- `const` and `readonly` require initializers (const at compile time, readonly by end of constructor).
- Initializers can call methods (`var now = DateTime.UtcNow;`) whereas `const` cannot use runtime values.

---

#### Q28. Can you use `const` with user-defined types like `DateTime` or `decimal` computed at runtime? Why or why not?

**Concepts**
- const requiring compile-time constant expression
- CS0133 compile error for runtime-computed const
- readonly field or static readonly for runtime-initialized values
- Decimal literal const (0.08m) versus runtime-computed decimal

**Answer**

You cannot mark such values `const` because `const` requires a compile-time constant expression, and `DateTime.Now` or runtime-computed `decimal` values are evaluated when the program runs, not when the compiler builds the assembly.

- `const decimal Tax = 0.08m;` is valid because the literal is known at compile time.
- `readonly DateTime Created = DateTime.UtcNow;` or a static readonly field set in the static constructor is the correct pattern for runtime values.
- Attempting `const DateTime d = DateTime.Now;` produces compile error CS0133.

---

### 03. Input & Output

#### Q1. What is the difference between `Console.WriteLine`, `Console.Write`, and string interpolation for output?

**Concepts**
- Console.WriteLine appending platform newline after output
- Console.Write for same-line prompts before ReadLine
- String interpolation as compile-time string composition technique
- Both Write and WriteLine targeting Console.Out stream

**Answer**

`Console.WriteLine` prints text followed by the platform newline, `Console.Write` prints without advancing to the next line, and string interpolation (`$"{name}"`) builds the string in memory before passing it to either write method. Interpolation is a string composition technique, not a separate I/O channel.

- Use `WriteLine` for line-oriented user prompts and log-style output where each message should appear on its own row.
- Use `Write` when prompting on the same line as user input (`"Enter name: "` followed by `ReadLine`).
- Interpolation embeds expressions and format specifiers (`$"{price:C2}"`) and is usually clearer than concatenation with `+`.
- Both `Write` and `WriteLine` target `Console.Out` unless you redirect output.

---

#### Q2. What is the difference between `Console.ReadLine()` and `Console.ReadKey()`?

**Concepts**
- ReadLine blocking until Enter and returning full line string
- ReadKey reading single keystroke and returning ConsoleKeyInfo
- ReadLine null return at end of stream (Ctrl+Z on Windows)
- ReadKey intercept mode suppressing keystroke echo

**Answer**

`Console.ReadLine()` blocks until the user presses Enter and returns the entire line as a `string` (without the newline), while `Console.ReadKey()` reads a single keystroke immediately (optionally intercepting it so it is not echoed) and returns a `ConsoleKeyInfo`.

- `ReadLine` suits free-text input such as names, addresses, or pasted tokens.
- `ReadKey(true)` suits "Press any key to continue" menus without requiring Enter.
- `ReadLine` can return `null` on stream end (Ctrl+Z on Windows console); handle null before parsing.
- `ReadKey` exposes whether Shift or Alt modifiers were held via `ConsoleKeyInfo`.

---

#### Q3. How do you safely parse user input (`int.TryParse`, `Parse`, `Convert`) and handle invalid input?

**Concepts**
- TryParse returning false on failure without throwing
- Parse throwing FormatException on invalid text
- NumberStyles and IFormatProvider for locale-aware parsing
- Reprompt loop pattern for interactive console input

**Answer**

Prefer `TryParse` for interactive input because it returns `false` on failure without throwing, letting you reprompt instead of crashing. `Parse` and `Convert` throw exceptions on invalid input, which is acceptable only when bad input represents a programmer error, not a user typo.

- Pattern: loop until `int.TryParse(Console.ReadLine(), out int n)` succeeds, showing an error message on failure.
- Supply `NumberStyles` and `IFormatProvider` when input may include currency symbols or locale-specific separators.
- `Convert.ToInt32` accepts `object` and null handling rules differ slightly from `int.Parse`.

---

#### Q4. How does formatted console output work (`Console.WriteLine("{0}", value)` vs interpolation)?

**Concepts**
- Composite formatting with indexed placeholder substitution
- String.Format semantics shared by Console.WriteLine and interpolation
- Alignment and numeric format specifiers in holes
- IFormattable participation in custom type formatting

**Answer**

Composite formatting passes a format string with indexed placeholders `{0}`, `{1}` and a parameter list; the runtime calls `String.Format` semantics to substitute each hole. String interpolation (`$"{value}"`) is translated by the compiler into a similar `Format` call with a generated format string.

- Composite formatting supports reuse of the same index (`{0}` twice) and explicit ordering when arguments are computed expressions.
- Interpolation inlines expressions directly, which is easier to read for simple messages.
- Both support alignment and numeric formats: `{0,10:N2}` or `$"{value,10:N2}"`.
- Custom types can participate via `IFormattable.ToString(format, provider)`.

---

#### Q5. What is the difference between `CultureInfo.CurrentCulture`, `CurrentUICulture`, and `InvariantCulture`?

**Concepts**
- CurrentCulture driving number/date/currency format for display
- CurrentUICulture selecting localized resource strings
- InvariantCulture for stable format across locales (logs, protocols)
- Thread-level culture affecting implicit formatting calls

**Answer**

`CurrentCulture` drives formatting and parsing of numbers, dates, and currency for user-facing display. `CurrentUICulture` selects localized resource strings (satellite assemblies). `InvariantCulture` is a fixed, culture-neutral English-like format used when data must round-trip regardless of user locale.


Changing `CurrentCulture` affects `ToString()` on numbers and dates when no explicit provider is passed.

---

#### Q6. When should you use `InvariantCulture` for formatting numbers and dates instead of `CurrentCulture`?

**Concepts**
- InvariantCulture for stored, transmitted, and parsed formats
- CurrentCulture for user-visible display strings only
- Explicit culture parameter documenting intent at call site
- Comma vs period decimal separator inconsistency across locales

**Answer**

Use `InvariantCulture` whenever the string is stored, transmitted, or parsed later in a different locale—logs, JSON, CSV, query strings, and file names—because `CurrentCulture` varies by machine and user settings.

- User-facing labels in a GUI may use `CurrentCulture` for friendly dates, but API payloads should not.
- Explicit `culture` parameters override the thread default and document intent at the call site.
- Mixing cultures between write and read causes subtle bugs (`"1,234.56"` vs `"1.234,56"`).

---

#### Q7. How do culture settings affect decimal separators, currency symbols, and date formats in console output?

**Concepts**
- NumberFormatInfo controlling decimal separator and group separators
- DateTimeFormatInfo controlling short and long date patterns
- Implicit culture use when no provider passed to formatting methods
- Explicit culture setting for deterministic test assertions

**Answer**

Formatting APIs consult the culture's `NumberFormatInfo` and `DateTimeFormatInfo`, so the same `double` or `DateTime` prints differently under `en-US`, `de-DE`, or `fr-FR`. Currency format adds symbol placement and group separators defined by that culture.

- `3.14.ToString()` in `de-DE` may display `3,14` while `en-US` shows `3.14`.
- Short dates might be `MM/dd/yyyy` vs `dd.MM.yyyy`, affecting user confusion in console apps.
- `Console` output does not auto-adapt unless the thread culture is set or you pass a format provider.
- Testing console apps should set culture explicitly or use invariant formatting for deterministic assertions.

---

#### Q8. What is composite formatting (`string.Format`, `{0:N2}`, alignment `{0,10}`, `{0,-10}`)?

**Concepts**
- Indexed placeholder substitution in format template strings
- N2 numeric format for two decimal places with group separators
- Alignment: positive for right-align, negative for left-align
- Standard and custom format strings per IFormattable type

**Answer**

Composite formatting replaces indexed placeholders in a template string with formatted argument values, optionally specifying width, alignment, and numeric or date format strings after a colon.

- `{0:N2}` formats argument zero as a number with two decimal places and group separators per the provider.
- `{0,10}` right-aligns in width 10; `{0,-10}` left-aligns with padding spaces by default.
- `string.Format(provider, format, args)` and `Console.WriteLine(format, args)` share the same rules.
- Custom formats (`"P"`, `"C"`, `"yyyy-MM-dd"`) map to standard or type-specific format strings documented for each type.

---

#### Q9. What is the difference between `Console.InputEncoding` and `Console.OutputEncoding`, and why can mismatched encodings garble console text?

**Concepts**
- InputEncoding controlling byte-to-char mapping on read
- OutputEncoding controlling char-to-byte mapping on write
- UTF-8 alignment requirement between console code page and .NET encoding
- Mojibake from multi-byte sequence misinterpretation

**Answer**

`Console.InputEncoding` and `Console.OutputEncoding` control how bytes from the console device map to .NET characters on read and write respectively. If the console code page or terminal encoding does not match the Unicode text you emit, characters outside that repertoire display as `?` or mojibake.

- Windows consoles historically defaulted to OEM or ANSI code pages; UTF-8 output requires matching console and `OutputEncoding`.
- Reading UTF-8 bytes with a Latin-1 interpretation corrupts multi-byte sequences.
- Modern .NET templates often set UTF-8 in project or host configuration; legacy environments may still need explicit setup.

---

#### Q10. How do you capture console output programmatically (e.g., `StringWriter` redirected to `Console.SetOut`)?

**Concepts**
- Console.SetOut replacing Console.Out with a TextWriter
- StringWriter accumulating output in memory for test assertions
- try/finally restoring original writer after capture
- Console.SetError for stderr capture in parallel

**Answer**

Replace `Console.Out` with a `TextWriter` such as `StringWriter` via `Console.SetOut`, run code under test that writes to `Console`, then read the accumulated text from the writer. Restore the original writer in a `finally` block so later tests are not affected.

- Save `var original = Console.Out;` before redirecting and call `Console.SetOut(original)` in `finally`.
- `StringWriter.ToString()` returns all captured output after the exercise under test completes.
- The same pattern works for `Console.SetError` with a separate `StringWriter` for stderr.
- Integration tests use this to assert CLI tools print expected messages without spawning a visible console.

---

#### Q11. What is the difference between `Console.Read` and `Console.ReadLine`?

**Concepts**
- Console.Read returning one int code unit or -1 at end of stream
- Console.ReadLine returning full line string until Enter
- ReadLine as standard choice for text input in console apps
- ReadKey providing richer keystroke control than Console.Read

**Answer**

`Console.Read()` returns the next character as an `int` code unit (or `-1` at end of stream), blocking until one character is available, while `ReadLine()` reads until Enter and returns a full `string` including all characters typed on that line.

- `Read` is low-level and rarely used in application code except single-key scenarios without `ReadKey` features.
- `ReadLine` is the standard choice for textual user input in tutorials and simple CLIs.
- Neither trims whitespace unless you call `Trim()` on the returned string.
- `Read` does not echo behavior differences beyond platform defaults; `ReadKey` offers more control over echo and intercept.

---

#### Q12. How do you format output with alignment and padding using interpolation (`$"{value,10}"`, `$"{value:N2}"`)?

**Concepts**
- Alignment in interpolation holes: comma and width
- Format specifiers after colon in interpolation holes
- Negative alignment for left-padding in columnar output
- Culture-sensitive formatting from current thread culture

**Answer**

Interpolation supports the same alignment and format syntax as composite formatting inside the braces: a comma and width for padding, a colon and format string for numeric or date patterns.

- `$"{name,-20}"` left-aligns `name` in 20 columns, useful for columnar console tables.
- `$"{amount:N2}"` prints two decimal places with culture-aware group separators unless you pass `CultureInfo.InvariantCulture`.
- Multiple holes can mix formats: `$"{id,5} {desc,-30} {price,10:C}"`.
- Constant alignment values compile efficiently; complex expressions inside `{...}` are evaluated before formatting.

---

#### Q13. What is `IFormattable`, and how does it relate to custom formatting in `ToString(format, provider)`?

**Concepts**
- IFormattable declaring ToString(string format, IFormatProvider provider)
- Runtime calling IFormattable when format specifier is present
- FormattableString and FormattableStringFactory for culture-invariant logging
- Rich standard format support on BCL numeric types

**Answer**

`IFormattable` declares `ToString(string? format, IFormatProvider? formatProvider)`, allowing types to interpret custom and standard format strings when used in composite formatting or interpolation. The runtime calls this interface when formatting an object with a non-null format specifier.

- Implement both `ToString()` and `ToString(format, provider)` consistently for public types.
- If a type lacks `IFormattable`, unknown format strings may be ignored in `ToString(format)` overrides on `object`.
- `FormattableString` and `FormattableStringFactory` expose interpolation holes for culture-invariant logging scenarios.
- Currency and numeric BCL types implement rich format support (`"C"`, `"X"`, `"E"`).

---

#### Q14. What happens if you call `int.Parse` on invalid input vs `int.TryParse` — which pattern is preferred in production console apps?

**Concepts**
- int.Parse throwing FormatException or OverflowException on failure
- TryParse returning false for expected bad input without throwing
- Exception for exceptional conditions; false return for normal failures
- Culture and NumberStyles parameters affecting parse behavior

**Answer**

`int.Parse` throws `FormatException` (or `OverflowException`) when the text is not a valid integer, terminating the flow unless caught. `int.TryParse` returns `false` and sets the out parameter to default, which fits expected user input mistakes in interactive loops.

- Production console apps should treat bad input as a normal branch, not an exceptional one—see Gotcha 10.
- `Parse` remains fine for trusted configuration files validated at startup with clear error handling.
- Always consider culture: parsing `"1.234"` depends on whether `.` is decimal or thousands separator.
- Wrap parse failures with user-visible messages and reprompt rather than stack traces.

---

#### Q15. How does changing `CultureInfo.CurrentCulture` on the current thread affect subsequent formatting calls that omit an explicit provider?

**Concepts**
- CurrentCulture as implicit provider for thread-level formatting
- ASP.NET Core per-request culture from Accept-Language headers
- Async continuation culture flow configuration
- Explicit IFormatProvider parameters avoiding global state dependency

**Answer**

Formatting methods without an explicit `IFormatProvider` use `CultureInfo.CurrentCulture` on the executing thread, so changing it mid-process changes how numbers, dates, and currency render from that point forward on that thread.

- ASP.NET and modern hosts often set culture per request from headers; console apps inherit OS user settings by default.
- Async continuations may flow culture depending on configuration; explicit providers avoid surprises.
- Libraries should not mutate global culture silently; accept `IFormatProvider` parameters instead.
- Tests set `CultureInfo.CurrentCulture` to invariant or a fixed culture in setup for deterministic output.

---

#### Q16. What is `NumberFormatInfo`, and how does it differ from `CultureInfo`?

**Concepts**
- NumberFormatInfo holding decimal separator, group size, negative pattern
- CultureInfo as broader container exposing NumberFormat and DateTimeFormat
- Clone and modify NumberFormatInfo for custom numeric display
- NumberStyles for controlling allowed parse input characters

**Answer**

`NumberFormatInfo` holds the specific rules for decimal separators, group sizes, negative patterns, and percent symbols, while `CultureInfo` is the broader culture object that exposes `NumberFormat`, `DateTimeFormat`, and other regional settings together.

- Access via `CultureInfo.CurrentCulture.NumberFormat` or `CultureInfo.InvariantCulture.NumberFormat`.
- Clone and modify `NumberFormatInfo` for custom numeric displays without creating a full custom culture.
- `CultureInfo` is the usual entry point; `NumberFormatInfo` is the detailed knob for number layout only.
- Parsing methods accept `NumberStyles` combined with a format provider derived from culture.

---

#### Q17. When reading numeric input from users in different locales, what pitfalls arise with comma vs period decimal separators?

**Concepts**
- Locale-dependent decimal separator interpretation in TryParse
- de-DE treating comma as decimal, en-US treating it as thousands separator
- Explicit culture matching the user's locale for interactive parsing
- InvariantCulture for machine-readable fixed-format input

**Answer**

Users type decimals according to local convention, but `TryParse` without a culture may interpret commas as thousands separators or decimal points inconsistently, producing wrong values or parse failures when the thread culture does not match the user's expectation.

- `"3,14"` is 3.14 in `de-DE` but might fail or misparse under `en-US` rules.
- Always document expected format in prompts or parse with an explicit culture matching the UI language.
- For machine-readable input, instruct users to use invariant format or accept both with custom parsing logic.

---

#### Q18. What is the purpose of `Console.ForegroundColor`, `BackgroundColor`, and resetting colors after use?

**Concepts**
- Console color properties for CLI highlighting and section emphasis
- Console.ResetColor() preventing color bleed to subsequent output
- try/finally restoring default colors on exception paths
- Terminal capability and accessibility considerations

**Answer**

These properties change the console's text and background colors for subsequent output, highlighting errors, success, or sections in CLI tools. Resetting to `Console.ResetColor()` afterward prevents later unrelated output from inheriting unintended colors.

- Color support depends on terminal capabilities; Windows Terminal and modern consoles support richer palettes than legacy hosts.
- Wrap color changes in `try/finally` to restore defaults even when exceptions occur.
- Overusing color reduces accessibility; pair with explicit labels, not color alone, for errors.
- Libraries logging to shared consoles should avoid setting colors unless configured, to respect host themes.

---

### 04. Operators & Expressions

#### Q1. What are the different types of operators in C#? (Arithmetic, Relational, Logical, Bitwise, Assignment, Ternary, Null-coalescing, etc.)

**Concepts**
- Arithmetic, relational, logical, bitwise, and assignment operator categories
- Operator precedence table from unary through assignment
- Null-conditional (?.) and null-coalescing (??, ??=) operators
- User-defined operator overloads via static methods

**Answer**

C# groups operators by purpose: arithmetic (`+`, `-`, `*`, `/`, `%`), relational (`==`, `!=`, `<`, `>`), logical (`&&`, `||`, `!`), bitwise (`&`, `|`, `^`, `~`, shifts), assignment (`=`, `+=`, compound forms), conditional (`?:`), null operators (`?.`, `??`, `??=`), and others such as `is`, `as`, `sizeof`, and `nameof`.

- Operator precedence determines evaluation order when parentheses are omitted; unary before multiplicative before additive before relational before logical AND before OR.
- Overloaded operators apply to user-defined types when declared with `public static` signatures matching language rules.
- Some operators behave differently on floating-point vs integer types (division, remainder).
- Null-conditional and null-coalescing operators integrate with nullable reference and value type analysis.

---

#### Q2. Explain the `checked` and `unchecked` keywords in C#.

**Concepts**
- checked context throwing OverflowException on integer overflow
- unchecked context (default) silently wrapping via two's complement
- Block and expression scope for checked/unchecked
- Financial and checksum code using checked for early failure

**Answer**

`checked` context causes arithmetic overflow on integral types to throw `OverflowException`, while `unchecked` (the default in most projects) silently wraps overflow using two's complement truncation. You can scope contexts with `checked { ... }` blocks or enable project-wide checked arithmetic.

- Financial and checksum code often enables `checked` for early failure instead of silent wraparound.
- `unchecked` is explicit when you rely on wrap semantics (hash mixing, low-level algorithms).
- Overflow does not apply to floating-point types—they produce infinity or NaN instead.

---

#### Q3. What is the difference between `==` and `.Equals()` for value types vs reference types?

**Concepts**
- == comparing values for primitives and overloaded reference types
- Equals() overridable for structural equality semantics
- Default reference equality for non-overloaded reference types
- IEquatable<T> and consistent GetHashCode for collection keys

**Answer**

For value types, `==` compares values when overloaded or compares bitwise equality for primitives; `.Equals` typically matches value semantics. For reference types, `==` may use reference equality unless overloaded (as `string` does), while `.Equals` may be overridden for logical equality.

- Default reference equality: `==` and `ReferenceEquals` align unless the type overloads `==`.
- Structs should implement `IEquatable<T>` and consistent `GetHashCode` when used in collections.
- `Equals(object?)` accepts null; `==` between reference types is false if either side is null (unless both null).

---

#### Q4. What is integer division in C#, and how do you get a fractional result?

**Concepts**
- Integer division truncating toward zero when both operands are integral
- Floating-point promotion for fractional results
- % remainder operator with dividend sign
- decimal division for financial fractional arithmetic

**Answer**

When both operands of `/` are integral types, C# performs integer division, truncating toward zero and discarding any fractional part. At least one operand must be floating-point (or cast) to obtain a fractional result.

- `10 / 3` yields `3`, not `3.333…`—see Gotcha 2.
- `10 / 3.0` or `10.0 / 3` promotes to `double` and yields approximately `3.333…`.
- `%` returns the remainder with the sign of the dividend: `-10 % 3` is `-1`.
- Use `decimal` division for money after promoting operands to `decimal`.

---

#### Q5. Explain operator precedence and associativity — why does `a + b * c` evaluate differently than `(a + b) * c`?

**Concepts**
- Multiplication higher precedence than addition
- Left-to-right associativity for most binary operators
- Right-to-left associativity for assignment and null-coalescing
- Parentheses explicitly overriding precedence

**Answer**

Operator precedence ranks multiplication above addition, so `a + b * c` computes `b * c` first then adds `a`. Associativity rules break ties at the same precedence level—most binary operators associate left-to-right, assignment and null-coalescing associate right-to-left.

- Parentheses override precedence explicitly and improve readability even when not strictly required.
- Misunderstanding precedence causes subtle bugs in compound conditions and arithmetic without parentheses.
- The language specification defines a complete table; IDE tooling parenthesizes subexpressions in tooltips when debugging.
- Ternary `?:` has lower precedence than most operators, which can surprise without parentheses around the condition.

---

#### Q6. What is the difference between prefix and postfix increment (`++i` vs `i++`)?

**Concepts**
- Prefix increment returning new value after increment
- Postfix increment returning original value before increment
- Identical effect for standalone increment statements
- Array indexer and expression context differences

**Answer**

Prefix increment (`++i`) adds one and returns the new value; postfix increment (`i++`) returns the original value then adds one. Both mutate the variable when applied to a mutable l-value.

- In standalone statements (`i++;` as its own line), the difference is invisible.
- In expressions like `array[i++]`, postfix uses the old index then advances; prefix advances first.
- Increment on value type properties that return copies does not compile unless the property returns by ref (rare).
- Overflow follows integral checked/unchecked context like other arithmetic.

---

#### Q7. What are short-circuit logical operators (`&&`, `||`), and why do they matter beyond boolean logic?

**Concepts**
- Short-circuit evaluation skipping right operand when outcome determined
- && skipping right when left is false; || skipping when left is true
- Null guard pattern: obj != null && obj.Count > 0
- Non-short-circuit & and | always evaluating both operands

**Answer**

`&&` and `||` evaluate the right operand only when necessary—`&&` skips the right side if the left is false, `||` skips if the left is true. This enables safe null checks and avoids side effects or expensive calls when the outcome is already determined.

- `if (obj != null && obj.Count > 0)` avoids `NullReferenceException` because `Count` is not evaluated when `obj` is null.
- Non-short-circuit `&` and `|` on `bool` always evaluate both sides—rarely needed except for bitwise booleans.
- Short-circuit behavior interacts with nullable booleans in lifted operators differently from strict evaluation.
- Side effects in the right operand must be understood as conditionally executed.

---

#### Q8. Explain the null-conditional operator (`?.`) and null-coalescing operators (`??`, `??=`).

**Concepts**
- Null-conditional ?. short-circuiting member access on null receiver
- Nullable<T> result for value-type member access through ?.
- ?? supplying default when expression is null
- ??= lazily initializing fields and locals
- Null-conditional delegate invocation pattern: handler?.Invoke()

**Answer**

The null-conditional operator `?.` accesses a member or indexer only when the receiver is not null, producing null for reference results or `Nullable<T>` for value results instead of throwing. `??` and `??=` supply defaults when an expression is null.

- `customer?.Address?.City` short-circuits at the first null in the chain.
- `?.` combined with invocation: `handler?.Invoke()` calls only if delegate is not null.
- `??=` lazily initializes fields and locals—see Q19 in Data Types.
- Null-conditional assignment extensions (`?.=`) appear in newer language versions for compound null-aware assignment.

---

#### Q9. What are bitwise operators (`&`, `|`, `^`, `~`, `<<`, `>>`), and when are they used in application code?

**Concepts**
- AND masking, OR setting, XOR toggling, NOT inverting bit patterns
- [Flags] enum combination with | and testing with &
- Unsigned right shift >>> (C# 11+) filling with zeros
- Low-level use: hashing, compression, hardware protocols

**Answer**

Bitwise operators manipulate individual bits of integral types: AND masks bits, OR sets bits, XOR toggles, NOT inverts, and shifts move bit patterns left or right. They appear in flags enums, permissions masks, hashing, compression, and low-level protocols.

- `[Flags]` enums combine values with `|` and test with `&`: `(mode & FileMode.Read) != 0`.
- Unsigned right shift `>>>` (C# 11+) fills with zeros regardless of sign for `int`/`uint`.
- Do not confuse bitwise `&` on integers with logical `&&` on booleans—they are different operators.
- Application business logic rarely needs bitwise ops unless modeling packed flags or interfacing with hardware formats.

---

#### Q10. What is the difference between logical AND (`&&`) and bitwise AND (`&`) when applied to `bool` operands?

**Concepts**
- Short-circuit && versus always-evaluating & on bool operands
- Bitwise & on integers unrelated to logical && on booleans
- Lifted nullable bool operators with & and |

**Answer**

Both combine boolean values, but `&&` short-circuits and `&` always evaluates both operands. For pure boolean logic with possible null checks or side effects on the right, `&&` is the default choice.

- `true & Foo()` always calls `Foo()`; `true && Foo()` still calls `Foo()` because the left is true—only false left skips right.
- Bitwise `&` on integers is unrelated to boolean `&&` despite similar symbols.
- Nullable bools use lifted operators with `&` and `|` producing null when either operand is null in some cases.
- Use `&` on bools only when both evaluations are required for correctness.

---

#### Q11. What is the ternary conditional operator (`?:`), and how does it differ from an `if/else` statement?

**Concepts**
- Ternary as expression yielding a value
- if/else as statement for multi-line arbitrary logic
- Type compatibility requirement for both ternary branches
- Nested ternary readability versus switch expression alternative

**Answer**

The ternary operator `condition ? whenTrue : whenFalse` is an expression that yields a value, while `if/else` is a statement controlling blocks of arbitrary size. Ternary fits simple choice-of-value scenarios; statements fit multi-line logic and void actions.

- Both branches must be type-compatible enough for the compiler to infer a common type.
- Nested ternaries reduce readability; prefer `if/else` or switch expressions for many branches.
- Ternary is evaluated eagerly for the chosen branch only—like `if/else`, not lazy like some functional forms.
- See Control Flow Q1 for stylistic guidance vs `if/else`.

---

#### Q12. What is the difference between `is` pattern matching and a simple boolean expression in a condition?

**Concepts**
- is operator combining type test, cast, and variable binding
- Declaration pattern introducing typed variable in true branch
- Constant, relational, property, and recursive pattern forms
- is not null integration with nullable flow analysis

**Answer**

The `is` operator tests runtime type and can introduce a typed variable in the same expression (`if (obj is Order o)`), combining type check, cast, and assignment. A simple boolean might call a method but does not bind a new strongly typed variable without a separate cast.

- Pattern forms include constant, relational, property, and recursive patterns in modern C#.
- `is not null` integrates with nullable flow analysis better than `!= null` in some analyzer versions.
- Type patterns fail closed when the object is null unless you use `is null` or null checks first.
- See Control Flow Q14 for `if` vs `switch` pattern usage.

---

#### Q13. When does overflow occur for integer arithmetic, and how do `checked` blocks change behavior?

**Concepts**
- Integer overflow when result exceeds type's representable range
- Silent wrap in unchecked context versus OverflowException in checked
- Compile-time overflow detection in checked constant expressions
- decimal always throwing OverflowException rather than wrapping

**Answer**

Overflow occurs when an integral operation's mathematical result lies outside the representable range of the type, such as `int.MaxValue + 1`. In unchecked context the value wraps; in checked context the runtime throws `OverflowException` at the overflowing operation.

- Literals and constant folding may detect overflow at compile time in checked context.
- Casting a too-large constant to a smaller type can overflow at compile time with error CS0221 in checked context.
- `decimal` throws `OverflowException` on overflow rather than wrapping silently.

---

#### Q14. What is the difference between `==` and `ReferenceEquals` for reference types?

**Concepts**
- ReferenceEquals always comparing object identity regardless of overloads
- == delegating to overloaded operator when defined (e.g., string)
- String interning as case where equal content may share identity
- Value types boxed to object compared by reference with ReferenceEquals

**Answer**

`ReferenceEquals(a, b)` always compares object identity—whether both refer to the exact same heap instance—ignoring any `==` overload on the type. `==` may delegate to an overloaded operator (strings compare by value) or default reference equality.

- Use `ReferenceEquals` when you intentionally need identity semantics despite value-based `==` overloads.
- Two distinct string objects with the same content may be `==` true but `ReferenceEquals` false unless interned.
- Value types boxed to object compare references when using `ReferenceEquals` on the boxed instances.
- See Strings chapter on interning and equality.

---

#### Q15. Can you overload operators in C# — which operators can and cannot be overloaded?

**Concepts**
- Overloadable unary and binary operators via static methods
- implicit and explicit conversion operators with clear semantics
- == and != requiring paired overloads
- Non-overloadable: =, +=, &&, ||, ternary ?:, and precedence rules
- Equals/GetHashCode/== consistency requirement for collection keys

**Answer**

C# allows overloading many unary and binary operators (`+`, `-`, `==`, `!=`, implicit/explicit conversions, etc.) as static methods on the declaring type, but cannot overload operator precedence, the ternary operator, `&&`, `||`, or assignment operators like `=` and `+=` directly.

- Comparison operators `==` and `!=` must be overloaded in pairs; `true`/`false` unary operators support custom boolean logic types.
- Conversion operators must be `implicit` or `explicit` and follow clear semantics to avoid surprising casts.
- Overloads should mirror intuitive meaning; abusing `+` for unrelated operations harms readability.
- `Equals`, `GetHashCode`, and `==` should stay consistent for types used as keys.

---

#### Q16. What is the difference between compound assignment (`+=`, `-=`) and the expanded form (`x = x + y`) for value vs reference types?

**Concepts**
- Compound assignment evaluating left side once for properties/indexers
- String immutability creating new instances with += or +
- Reference reassignment versus object mutation distinction
- ref parameters required for compound-assigned caller variable replacement

**Answer**

For value types, `x += y` mutates `x` in place when `x` is a mutable variable. For reference types, `+=` on references reassigns the variable when the operator returns a new reference (strings), while mutating methods on objects (`list.Add`) change the object without reassigning the variable.

- `string s = s + "a"` and `s += "a"` both create new string instances because strings are immutable.
- `list += item` is not valid unless overloaded; use `list.Add` for mutation.
- Compound assignment evaluates the left side once, which matters if the left is a property or indexer with side effects.
- See Methods chapter on ref parameters when reassignment must affect the caller's variable.

---

#### Q17. What is the `nameof` operator, and how is it used in validation messages and refactoring-safe code?

**Concepts**
- nameof returning unqualified identifier name as compile-time string
- Refactoring-safe argument names in ArgumentNullException
- Final token only: nameof(Foo.Bar) yields "Bar"
- Zero runtime overhead versus runtime reflection

**Answer**

`nameof` returns the unqualified identifier name of a variable, type, or member as a compile-time string without runtime reflection cost. Renaming the symbol updates `nameof` automatically, making it ideal for argument exception parameters and property change notifications.

- `throw new ArgumentNullException(nameof(customer));` stays correct if the parameter is renamed.
- `nameof` does not include namespace or full dotted paths—only the final token (`nameof(Foo.Bar)` is `"Bar"`).
- It avoids magic strings that drift from code during refactorings.
- Unlike string literals, `nameof` is resolved at compile time and has zero runtime overhead.

---

### 05. Type Conversion & Casting

#### Q1. What is the difference between the `is` and `as` operators?

**Concepts**
- is for type test with optional variable binding in true branch
- as for null-returning reference downcast on failure
- as incompatibility with non-nullable value types
- Declaration pattern preferred over as+null-check in modern C#

**Answer**

`is` tests whether an object is compatible with a type and, with pattern matching, binds a variable when the test succeeds. `as` attempts a cast and returns `null` on failure for reference types without throwing.

- `if (obj is Customer c)` combines check and assignment in one step.
- `var c = obj as Customer; if (c != null)` is the legacy pattern before pattern matching.
- `as` does not work on non-nullable value types directly; use `is` with patterns or explicit casts for value types.
- Failed `as` returns null, which can hide bugs if you forget the null check.

---

#### Q2. What is the difference between implicit and explicit type conversion (casting)?

**Concepts**
- Implicit conversion for guaranteed safe widening without cast syntax
- Explicit cast required for narrowing with potential data loss
- User-defined implicit and explicit conversion operators
- InvalidCastException on incompatible explicit reference casts

**Answer**

Implicit conversions compile without a cast when the conversion is guaranteed safe (widening numerics, derived to base reference). Explicit conversions require a cast operator when data might be lost or the relationship is not automatically safe (narrowing numerics, base to derived references).

- Widening (`int` to `long`) is implicit; narrowing (`long` to `int`) needs `(int)value` and may overflow at runtime.
- User-defined types can declare `implicit` or `explicit` conversion operators with clear semantics.
- Explicit reference casts throw `InvalidCastException` when the object is not actually of the target type.

---

#### Q3. What is the difference between `Convert.ToInt32`, `(int)`, and `int.Parse`?

**Concepts**
- Direct numeric cast (int) for compatible numeric types or unboxing
- int.Parse converting string representation with culture support
- Convert.ToInt32 accepting object and using IConvertible
- Convert.ToInt32 rounding versus cast truncation for floating-point

**Answer**

`(int)` is a direct cast requiring a compatible numeric type at compile time or an unboxing cast at runtime. `int.Parse` parses a string representation. `Convert.ToInt32` accepts broader inputs (`object`, strings, other numerics) with generalized conversion rules and uses `IConvertible`.

- `Parse` and `Convert` on strings throw on invalid text; prefer `TryParse` for user input.
- `Convert.ToInt32(3.9)` rounds to nearest integer (banker's rounding rules apply); `(int)3.9` truncates toward zero.
- `Convert` handles null for nullable types differently than `Parse`.
- Choose the narrowest API: cast for numeric narrowing, `TryParse` for strings, `Convert` for heterogeneous legacy APIs.

---

#### Q4. When does a cast succeed at compile time but fail at runtime?

**Concepts**
- Reference downcast compiling when static type is base but failing for wrong derived type
- Unboxing failing when boxed type does not match target exactly
- InvalidCastException as runtime failure signal
- Pattern matching with is avoiding runtime cast exceptions
- Array covariance enabling unsafe element writes

**Answer**

Reference downcasts compile when the static type is a base type but fail at runtime if the object is not actually an instance of the target derived type. Unboxing casts compile when the static type is `object` but fail if the boxed type does not match exactly.

- `(Derived)baseRef` compiles if `baseRef` is typed as `Base` but throws if it points to another derived type.
- `(int)obj` throws `InvalidCastException` if `obj` boxes a `double`, not an `int`.
- Pattern matching with `is` avoids exceptions by testing first.
- Array covariance also compiles unsafe assignments that fail at runtime—see Gotcha 8.

---

#### Q5. What is widening vs narrowing conversion — which direction is implicit?

**Concepts**
- Widening conversion to larger range allowed implicitly
- Narrowing conversion requiring explicit cast for possible data loss
- float/double to decimal requiring explicit cast
- Constant expression widening exception for fitting values

**Answer**

Widening conversions move to a type that can represent all values of the source without loss (for example `byte` to `int`), and C# allows them implicitly. Narrowing conversions move to a smaller range or lower precision and require explicit casts because values may be truncated or rounded.

- Floating to `decimal` is explicit because not all binary floats map exactly.
- `int` to `long` is implicit widening; `long` to `int` is explicit narrowing.
- Constant expressions may be implicitly converted when the value fits the target type even if narrowing in general.

---

#### Q6. What is the difference between `Parse`, `TryParse`, and `Convert.ChangeType`?

**Concepts**
- Parse converting strings and throwing on failure
- TryParse returning bool for expected parse failures without throwing
- Convert.ChangeType using IConvertible for reflection-based conversion
- Culture parameter requirement for locale-aware string parsing

**Answer**

`Parse` converts strings to target types and throws on failure. `TryParse` returns a boolean and an out result without throwing for expected bad input. `Convert.ChangeType` generalizes conversion between many types via `IConvertible`, often used in reflection-based scenarios.

- `TryParse` is preferred for interactive and protocol parsing where failure is normal.
- `ChangeType` returns `object` requiring a cast and throws `InvalidCastException` when no conversion exists.
- All string parsing should specify culture when format is locale-dependent.
- Nullable value types need `TryParse` overloads or parse the underlying type then assign.

---

#### Q7. When would you use the `is` pattern with a declaration (`if (obj is int n)`) vs a traditional cast?

**Concepts**
- Declaration pattern combining check, cast, and binding in one step
- Traditional cast acceptable when type already verified or exception desired
- Switch expression type patterns for multi-type dispatch
- Flow analysis benefits of is patterns over repeated explicit casts

**Answer**

Use the declaration pattern when you need both a type test and a typed variable in the true branch without a separate cast that the compiler cannot prove safe. Traditional casts `(int)obj` are acceptable when you already verified type or will catch exceptions.

- Declaration patterns integrate with nullable analysis and switch expressions cleanly.
- Repeated `(Target)obj` casts duplicate noise and skip flow analysis benefits.
- Traditional casts fail fast with exceptions—sometimes desired in trusted internal code paths.
- Switch expressions on type patterns replace long if-else chains—see Control Flow chapter.

---

#### Q8. What exception types are commonly thrown by failed casts and parses (`FormatException`, `OverflowException`, `InvalidCastException`)?

**Concepts**
- FormatException for string not matching expected numeric format
- OverflowException for value exceeding target type range
- InvalidCastException for incompatible cast or unboxing mismatch
- TryParse avoiding all three for expected string parse failures

**Answer**

`FormatException` indicates the string is not in the expected format for parse methods. `OverflowException` occurs when a numeric parse or checked arithmetic exceeds the target range. `InvalidCastException` signals an incompatible cast or unboxing operation at runtime.

- `int.Parse("abc")` throws `FormatException`; `int.Parse("999999999999999999999")` may throw `OverflowException`.
- Unboxing wrong types throws `InvalidCastException`.
- Catch specific types when recovering; let unexpected failures propagate in most application layers.
- `TryParse` avoids all three for expected failure paths on strings.

---

#### Q9. What is `TryFormat`, and how does writing into a `Span<char>` differ from calling `ToString()`?

**Concepts**
- TryFormat writing into caller-provided Span<char> without heap allocation
- ToString() always allocating a new string on the heap
- Span-based formatting for stackalloc buffers and UTF-8 pipelines
- Buffer-too-small false return with retry on larger span

**Answer**

`TryFormat` attempts to write a formatted representation into a caller-provided `Span<char>` buffer, returning whether the buffer was large enough. This avoids allocating a new `string` on the heap, which `ToString()` always does.

- Used internally by high-performance formatting and available on many primitive types.
- If the span is too small, `TryFormat` returns false and you can retry with a larger buffer.
- Span-based formatting fits stackalloc buffers and UTF-8 encoding pipelines in modern APIs.
- For simple logging, `ToString()` readability often outweighs micro-optimization unless profiling shows hot paths.

---

#### Q10. How does culture affect parsing and formatting during type conversion (e.g., `"1,234.56"` vs `"1.234,56"`)?

**Concepts**
- IFormatProvider controlling decimal separator and group separator
- InvariantCulture for wire formats and persisted data
- Locale-matching culture for user-input parsing
- Off-by-magnitude bugs from wrong culture in financial imports

**Answer**

Parse and format methods use the supplied `IFormatProvider` or `CurrentCulture` to decide whether comma or period is the decimal separator and how groups are laid out, so the same literal characters mean different numbers in different cultures.

- Always pass `CultureInfo.InvariantCulture` for wire formats and logs.
- User input should parse with the culture matching how you prompted the user to type data.
- Mis-specified culture causes off-by orders-of-magnitude bugs in financial imports.
- See Input & Output Q5–Q7 and Gotcha 9.

---

#### Q11. What is the difference between boxing during conversion to `object` and a direct numeric cast?

**Concepts**
- Boxing allocating heap wrapper when assigning value type to object
- Direct numeric cast converting value without heap allocation
- Unboxing requiring exact type match from object
- Generic collections avoiding boxing versus ArrayList boxing every element

**Answer**

Assigning a value type to `object` boxes it—allocating a heap wrapper—while casting between numeric types (`(int)d`) converts the value directly without creating an object wrapper when both sides are known numeric types.

- `(object)42` boxes; `(int)42.0` truncates a double without boxing.
- Unboxing from `object` back to a value type requires an exact type match.
- Generic collections avoid boxing for value types; `ArrayList` boxed every `int`.
- See Data Types Q4 and Q14 on performance impact.

---

#### Q12. When is the `as` operator preferred over a cast, and what does it return on failure?

**Concepts**
- as returning null on failure instead of throwing InvalidCastException
- Preferred when downcast failure is an expected normal branch
- as incompatibility with non-nullable value types
- is patterns preferred over as+null-check in modern C#

**Answer**

Prefer `as` when casting down a reference hierarchy where failure is an expected outcome and you will branch on null, avoiding exception cost. On failure, `as` returns `null` for reference types instead of throwing `InvalidCastException`.

- Do not use `as` with value types except nullable scenarios; use `is` patterns instead in modern code.
- After `as`, always null-check before dereferencing.
- When failure should abort processing, an explicit cast or pattern match documents intent more clearly.
- Legacy codebases mix `as` with null checks; new code favors `is` patterns.

---

#### Q13. What is user-defined explicit/implicit conversion operator syntax (preview level)?

**Concepts**
- implicit operator for obviously safe conversions
- explicit operator for conversions with possible information loss
- Overload resolution participation alongside built-in conversions
- Domain type use cases: units, identifier types with natural mappings

**Answer**

Types can declare `public static implicit operator TargetType(SourceType s)` or `explicit operator TargetType(SourceType s)` to allow the compiler to convert between types with defined semantics, subject to language rules requiring paired safety documentation in API design.

- Implicit conversions should be obviously safe; explicit conversions signal possible information loss.
- They participate in overload resolution like built-in conversions when applicable.
- Abuse creates hidden conversions that confuse readers—reserve for domain types with clear mappings (units, identifiers).
- They do not replace good constructor or factory methods when conversion is not natural.

---

#### Q14. What happens when you cast a `double` to `int` — is rounding or truncation applied?

**Concepts**
- Truncation toward zero dropping fractional part
- 3.9 and -3.9 casting to 3 and -3 respectively
- Convert.ToInt32 applying banker's rounding versus cast truncation
- Math.Floor/Ceiling/Round for explicit rounding before cast

**Answer**

Casting floating-point to integral types truncates toward zero, dropping the fractional part without rounding to nearest. `3.9` and `-3.9` cast to `3` and `-3` respectively.

- `Convert.ToInt32(3.9)` rounds to nearest even in default mode—different from cast truncation.
- `Math.Floor`, `Ceiling`, and `Round` express explicit rounding before casting when business rules require it.
- Overflow still possible if double magnitude exceeds int range—unchecked cast wraps in unchecked context.

---

#### Q15. What is the difference between `default(T)` casting patterns and `Convert` methods for nullable value types?

**Concepts**
- default(Nullable<int>) as null without value
- Convert methods potentially yielding zero versus null for null input
- TryParse for optional form fields representing absent numeric values
- Consistent null-versus-zero strategy per API layer

**Answer**

`default(Nullable<int>)` is null without value, while converting null with `Convert` may yield zero for value types depending on overload. Parsing empty strings differs between `TryParse` (false) and `Convert` behaviors.

- Use `TryParse` for optional form fields representing absent numbers with `int?`.
- `Convert.ChangeType` on boxed null often returns null for nullable target types when configured appropriately.
- Keep one consistent strategy per API layer to avoid null vs zero ambiguity.
- Document whether absent numeric input maps to null or zero for consumers.

---

#### Q16. When converting between `string` and numeric types in APIs and logs, why is `InvariantCulture` often specified explicitly?

**Concepts**
- InvariantCulture producing identical string representation on every machine
- Locale-independent parsing for downstream consumers
- Test determinism without thread-culture setup
- REST and JSON using invariant-like number formats

**Answer**

Explicit invariant culture makes serialized numbers and dates identical on every machine, so downstream parsers, diff tools, and aggregators do not misread separators when the server's locale differs from the developer's workstation.

- Logs consumed globally should not flip decimal commas based on OS language.
- REST and JSON often use invariant-like formats even though JSON numbers typically avoid locale separators.
- Unit tests assert expected strings without setting thread culture when invariant is specified at the call site.

---

### 06. Control Flow & Loops

#### Q1. What is the difference between `if/else` and the ternary operator?

**Concepts**
- if/else as statement for multi-line blocks and void actions
- Ternary as expression selecting between two values
- Type compatibility requirement for ternary branches
- Readability guideline: ternary for simple value choice only

**Answer**

`if/else` is a statement that executes one of two blocks of arbitrary size and may perform multiple actions, while the ternary operator is a single expression that selects between two values. Use ternary for simple assignments; use `if/else` for multi-statement branches and side effects.

- Ternary requires both branches to be expressions compatible enough for type inference.
- Nested ternaries harm readability compared to `if/else` chains or switch expressions.
- See Operators Q11 for evaluation semantics.
- Style guides often limit ternary to one line of choice between two values.

---

#### Q2. What is the difference between traditional `switch` and switch expressions (C# 8+)?

**Concepts**
- Switch expression producing a value with => arms
- Traditional switch statement with case labels and break
- Fall-through elimination in switch expressions
- Exhaustiveness requirement with _ discard pattern
- Pattern support in both: constant, type, relational, property

**Answer**

Traditional `switch` is a statement with `case` labels, optional `break`, and fall-through restrictions, while switch expressions (`var result = x switch { ... }`) produce a value with expression-bodied arms separated by commas and use exhaustive pattern matching rules.

- Switch expressions discourage fall-through bugs by requiring `=>` arms and no implicit fall-through.
- Both support type, constant, and relational patterns in modern C#.
- Switch expressions must cover all inputs or include a discard `_` pattern when exhaustive.
- Prefer switch expressions for mapping enums to labels; use statements when cases need multiple statements without local functions.

---

#### Q3. When should you use `for`, `foreach`, `while`, and `do-while`?

**Concepts**
- for for index-controlled loops with known bounds
- foreach for IEnumerable sequences without manual indexing
- while for condition-tested-before-first-iteration loops
- do-while for at-least-once body execution
- foreach compiler-generated enumerator disposal via try/finally

**Answer**

Use `for` when you need an index counter and known iteration bounds, `foreach` to enumerate `IEnumerable` sequences without manual indexing, `while` when the loop condition is tested before each iteration, and `do-while` when the body must run at least once before the condition is checked.

- `foreach` is idiomatic for collections and LINQ-friendly sequences; it hides enumerator disposal via compiler-generated try/finally.
- `for` suits arrays when you need the index for parallel arrays or reverse iteration.
- `while` fits polling and read-until-done loops with unknown iteration count.
- `do-while` fits menu loops that display once before validating exit condition.

---

#### Q4. What is the difference between `break`, `continue`, and `return` inside a loop?

**Concepts**
- break exiting innermost enclosing loop or switch
- continue skipping to next iteration of innermost loop
- return exiting the entire method after any enclosing finally
- finally execution on return from try block

**Answer**

`break` exits the innermost enclosing loop or switch immediately, `continue` skips to the next iteration of the innermost loop, and `return` exits the entire method (after running any enclosing `finally` blocks) regardless of loop nesting.

- `break` in nested loops does not exit outer loops unless you use labeled break (rare) or refactor.
- `continue` re-evaluates the loop condition before the next body execution.
- `return` inside `try` still executes `finally` before the method actually returns—see Q15 and Gotcha 7.
- Misusing `break` vs `return` in search loops changes whether cleanup after the loop runs.

---

#### Q5. What are common pitfalls with nested loops and loop variable scope?

**Concepts**
- O(n²) complexity from nested loops on large inputs
- Inner loop variable shadowing outer loop index names
- Lambda closure capture in foreach before C# 5 fix
- Off-by-one errors at inner loop boundaries

**Answer**

Nested loops multiply complexity and can hide O(n²) performance; reusing the same index variable name in inner loops shadows outer variables and confuses readers. Closure capture of loop variables in lambdas behaved differently before C# 5 for `foreach` vs `for`.

- Prefer extracting inner loops to methods when depth exceeds two levels with non-trivial logic.
- `for` loop variables are scoped to the loop; declaring the same name in nested `for` headers is legal but error-prone.
- Off-by-one errors at inner boundaries often cause skipped or duplicate processing in matrices.

---

#### Q6. What is a switch expression, and how do relational and property patterns work in `switch`?

**Concepts**
- Switch expression with => arms producing typed result
- Relational guards: >= 90 => "A" style range dispatch
- Property patterns deconstructing object shape: { Status: Shipped }
- Tuple patterns for multi-value discrimination
- Exhaustiveness compiler warning for unhandled enum values

**Answer**

Switch expressions map input patterns to result expressions using `=>` arms, supporting constant patterns, type patterns, relational guards (`when`), property patterns (`{ Length: > 0 }`), and positional patterns on tuples and records.

- Relational patterns combine with constants: `var label = score switch { >= 90 => "A", >= 80 => "B", _ => "C" };`.
- Property patterns deconstruct shape: `obj switch { { Status: OrderStatus.Shipped } => true, _ => false }`.
- The compiler warns when not all enum values are handled if exhaustive analysis applies.
- Switch expressions are expression-oriented—each arm must yield a compatible type.

---

#### Q7. What is the difference between `break` in a `switch` vs `break` in a loop?

**Concepts**
- break exiting only the innermost enclosing switch or loop
- Classic switch requiring break to prevent fall-through
- Switch expressions replacing break with arm separation
- break inside nested switch inside loop exiting only the switch

**Answer**

In both constructs `break` exits only the innermost enclosing switch or loop—it does not exit nested switches inside loops beyond one level. In modern switch expressions, `break` is replaced by arm separation without fall-through.

- Classic switch requires `break` (or `return`/`goto case`) at the end of each case unless the case ends with a jump.
- Accidentally omitting `break` in classic switch caused fall-through bugs before C# disallowed unreachable fall-through in many cases.
- `break` inside a case nested within a loop exits the switch, not the loop.
- Use `return` from a method or refactor when you need to exit both switch and loop.

---

#### Q8. When is `goto` still used in C# (e.g., `goto case`, `goto default`), and why is it generally discouraged?

**Concepts**
- goto case / goto default for intentional shared switch case logic
- Structured alternatives: switch expressions, local functions
- Unstructured jump making control flow hard to trace and refactor
- Acceptable in generated code and state machines

**Answer**

`goto case` and `goto default` jump to another switch label when sharing logic between cases without duplicating code. General `goto` labels are discouraged because unstructured jumps make control flow hard to follow and refactor compared to loops, methods, and structured switches.

- `goto case` appears when one case falls through intentionally to shared cleanup in legacy switch statements.
- Switch expressions and local functions largely eliminate the need for arbitrary labels.
- Acceptable in generated code or performance-critical state machines; rare in application business logic.
- Excessive `goto` correlates with maintenance defects in large methods.

---

#### Q9. What is the scope of a variable declared in the initializer of a `for` loop (C# rules)?

**Concepts**
- for initializer variable scoped to entire for statement
- Variable inaccessible after loop completes
- Outer declaration required to read final index after loop
- Separate scope for each nested for loop's own index variable

**Answer**

A variable declared in the `for` loop initializer is scoped to the entire `for` statement and is not visible after the loop ends. Each `for` statement creates its own scope for that variable.

- You cannot reference the loop variable after the loop if it was declared in the initializer: `for (int i = 0; ...)` then `i` is out of scope.
- Declare the variable outside the loop if you need the final index value after completion.
- C# disallows assigning to the foreach iteration variable—different rule from `for` index mutation.
- Nested loops can each declare `int i` in separate `for` headers in separate scopes.

---

#### Q10. Why did C# 5 change loop variable capture semantics in lambdas, and how does that affect `foreach` vs `for`?

**Concepts**
- Pre-C# 5 shared foreach variable captured by all lambdas
- C# 5 per-iteration variable capture fix for foreach
- for loop index still sharing one mutable variable without local copy
- Per-iteration local copy pattern inside for for async lambdas

**Answer**

Before C# 5, a lambda closing over a `foreach` iteration variable captured the single shared variable, so all delegates saw the final value after the loop. C# 5 changed `foreach` to capture each iteration's value separately; `for` loop index capture still closes over one mutable variable unless you copy to a local inside the loop.

- Bug pattern: tasks created in `foreach` all observing the last item before the fix.
- Inside `for`, copy `int copy = i;` before async lambdas when you need per-iteration capture with mutable index semantics.
- Understanding this prevents subtle parallel and async bugs in LINQ and Task loops.
- Modern code often uses `Select((item, index) => ...)` to avoid manual capture issues.

---

#### Q11. Can you modify the collection you are iterating in a `foreach` loop — what exception results?

**Concepts**
- InvalidOperationException on structural modification during enumeration
- Enumerator invalidation on add/remove in most collection types
- for loop or reverse iteration as safe alternatives for removal
- Concurrent modification from other threads causing same exception

**Answer**

You cannot add or remove elements from most collections during `foreach` enumeration; doing so throws `InvalidOperationException` with message that collection was modified. Mutating elements in place may be allowed for some collection types but is risky if it changes structure.

- Use `for` backward over indices or build a new collection when removing items during iteration.
- `List<T>` documents that structural changes during enumeration invalidate the enumerator.
- Concurrent modification from another thread causes the same exception on many collection types.
- LINQ deferred queries may re-enumerate underlying collections that changed between operations.

---

#### Q12. What is the difference between `while` and `do-while` when the condition is false on the first check?

**Concepts**
- while skipping body entirely when condition initially false
- do-while always executing body at least once before condition check
- Menu prompt pattern where first display must always occur
- Both support break and continue with innermost-loop rules

**Answer**

`while` evaluates the condition before the first iteration and may skip the body entirely if the condition is initially false. `do-while` always executes the body at least once before evaluating the condition at the bottom.

- Use `do-while` for input validation menus that must prompt at least once.
- Both support `break` and `continue` with the same innermost-loop rules.
- Infinite loops use `while (true)` with internal `break` when exit logic is complex.
- Condition side effects run zero times in `while` if initially false, once before check in `do-while` after first body run.

---

#### Q13. When would you prefer a `switch` over a chain of `if/else if` statements?

**Concepts**
- switch for single expression discriminated against many constant/pattern values
- Enum and string discrimination readability with switch
- Switch expression for mapping inputs to outputs with fewer temp variables
- if/else for disparate unrelated boolean conditions

**Answer**

Prefer `switch` when discriminating a single expression against many constant or pattern values, especially enums, because switch expressions and statements communicate intent and enable compiler exhaustiveness checks. Long unrelated boolean conditions remain clearer as `if/else`.

- Switch on strings and enums is efficient and readable with modern pattern support.
- Switch expressions reduce temporary variables when mapping inputs to outputs.
- `if/else` fits disparate conditions that are not variations of one expression's shape.
- Performance differences are usually negligible; readability and correctness matter more.

---

#### Q14. What is pattern matching with `is` in an `if` statement vs a `switch` on type?

**Concepts**
- if (obj is Type t) for one or two type test branches
- switch on type with multiple arms scaling to many type cases
- Nullable flow analysis integration with successful type patterns
- Virtual method dispatch preference for open hierarchies

**Answer**

`if (obj is Type t)` handles one or two type tests inline, while `switch (obj)` with multiple type patterns scales when many types map to different handling paths without nested if ladders.

- Switch on type with arms `case Customer c:` (classic) or expression patterns consolidates dispatch.
- Both integrate with nullable flow analysis when patterns succeed.
- Prefer switch when adding a new type means adding an arm—a table-driven dispatch shape.
- Virtual methods often replace type switches for open hierarchies—see Module 02 polymorphism.

---

#### Q15. What happens if you use `return` inside a `try` block that has a `finally` — which executes first?

**Concepts**
- finally executing before method actually returns to caller
- return in finally overriding try return value
- finally cleanup guarantee on normal completion, exception, and return
- Avoid return in finally in application code

**Answer**

When `return` executes in `try`, the runtime runs the associated `finally` block before the method actually returns to the caller. If `finally` also contains `return`, that return value overrides the `try` return—see Gotcha 7.

- Cleanup in `finally` runs even when `try` returns normally or via exception.
- Async methods compile similarly with respect to `finally` in many patterns but add state machine complexity.
- Do not put `return` in `finally` except in generated code—it hides control flow.
- See Exception Handling chapter for interaction with exceptions during return.

---

### 07. Methods

#### Q1. What are the `out`, `ref`, and `in` parameter modifiers? Explain their usage.

**Concepts**
- ref passing alias to existing variable for read and write
- out requiring callee to assign before return as additional output
- in passing readonly alias avoiding copy cost for large structs
- Call-site keyword requirement for ref and out arguments

**Answer**

`ref` passes an alias to an existing variable so the callee can read and write it; `out` requires the callee to assign before return and represents an extra output slot; `in` passes a readonly alias for large structs to avoid copy cost while preventing modification through the parameter.

- Callers must pass `ref` and `out` arguments with the keyword at the call site (`Method(ref x, out y)`).
- `out` variables can be declared inline at the call site in modern C# (`TryParse(s, out int n)`).
- `in` suits large readonly struct parameters in hot paths—see Q12.

---

#### Q2. What is the `params` keyword in method definitions?

**Concepts**
- params enabling variable-argument call syntax for array parameters
- Single params parameter required as last in signature
- Compiler packing individual arguments into array at call site
- params ReadOnlySpan<T> for performance-friendly variadic APIs

**Answer**

`params` allows a method to accept a variable number of arguments of a specified array element type, which the compiler packs into an array at the call site. Only one `params` parameter is allowed and it must be the last parameter—see Gotcha 12.

- `void Log(params string[] messages)` enables `Log("a", "b")` without explicit array syntax.
- Passing an existing array explicitly still works without extra copying in many cases.
- Overload resolution prefers non-params overloads when an exact match exists.
- `params` with `ReadOnlySpan<T>` expands options in newer language versions for performance.

---

#### Q3. What are expression-bodied members in C#?

**Concepts**
- => syntax for single-expression methods, properties, and constructors
- Read-only computed properties with expression body
- Noise reduction for trivial forwarding and projection members
- Block body preferred for complex multi-statement logic

**Answer**

Expression-bodied members use `=>` syntax to define methods, properties, accessors, or constructors with a single expression instead of a braced block, reducing noise for trivial forwarding and computed members.

- Example: `public int Area => Width * Height;` for a read-only property.
- Expression-bodied methods can return values or void (`void M() => Console.WriteLine("hi");`).
- Complex logic should remain block-bodied for debugging and multiple statements.
- Module 02 covers expression-bodied properties in depth.

---

#### Q4. Explain named arguments and optional parameters in C#.

**Concepts**
- Optional parameters with compile-time default values
- Named arguments supplying parameters by name in any order
- Default values baked into call sites at compile time
- Boolean flag clarity improvement via named arguments

**Answer**

Optional parameters declare default values in the method signature so callers may omit trailing arguments. Named arguments supply parameters by name (`Method(timeout: 30, retries: 3)`) regardless of order, improving readability for long parameter lists.

- Optional parameters must come after required parameters unless all trailing parameters are optional.
- Defaults are compile-time constants embedded at call sites—see Gotcha 13 on recompile requirement when defaults change.
- Named arguments help with boolean flag clarity at call sites.
- Overload resolution considers optional parameters and may introduce ambiguity—see Constructors chapter in Module 02.

---

#### Q5. What are local functions in C#?

**Concepts**
- Method declared inside another member's body with limited scope
- Closure over enclosing method's locals and parameters
- Static local functions preventing accidental capture
- Iterator and validation helper use cases

**Answer**

Local functions are methods declared inside another method's body, visible only within the enclosing member, useful for splitting algorithm steps without polluting the type's namespace or capturing class state unnecessarily.

- They can be static local functions to avoid accidental capture of instance members when not needed.
- Iterator methods and validation helpers commonly use local functions for clarity.
- Local functions can access outer local variables (closure) unless declared static.

---

#### Q6. What is the `yield` keyword and iterators in C#? *(Cross-ref: Module 03 — IEnumerable)*

**Concepts**
- yield return / yield break implementing lazy IEnumerable<T> sequences
- State machine compilation for iterator methods
- Lazy evaluation deferring element production until requested
- Memory savings for large or infinite filtered sequences

**Answer**

`yield return` and `yield break` implement iterator methods that compile into state machines implementing `IEnumerable<T>` or `IEnumerator<T>`, producing elements lazily one at a time without building a full in-memory collection upfront.

- Consumers foreach over the sequence; execution resumes after each `yield return` when the next element is requested.
- Lazy evaluation saves memory for large or infinite sequences filtered by callers.
- Iterator methods cannot mix unstructured `yield` with `try/finally` patterns easily in all cases—language rules apply.
- Module 03 expands IEnumerable, LINQ, and deferred execution interactions.

---

#### Q7. Explain method overloading — what makes two methods overloads vs duplicate definitions?

**Concepts**
- Overloads sharing name but differing in parameter count or types
- Return type alone insufficient to distinguish overloads
- ref / out / value modifiers participating in signature distinction
- Generic method overloading on type parameter count

**Answer**

Overloads share the same method name but differ in parameter count or types (and optionally generic arity); the return type alone cannot distinguish overloads. Duplicate signatures with only return type differing are compile errors.

- `void Print(int x)` and `void Print(string s)` are valid overloads.
- `ref` vs `out` vs value parameter modifiers participate in signature distinction.
- Generic methods overload on type parameter count and constraints.

---

#### Q8. How does overload resolution work when multiple overloads could apply — what is the "better function member" rule?

**Concepts**
- Best match preferring fewer conversions and better conversion kinds
- Non-params candidates preferred over expanded params forms
- CS0121 ambiguity error when no single best candidate exists
- Named arguments and casts for intentional disambiguation

**Answer**

The compiler picks the best match by preferring fewer conversions, better conversion kinds (implicit over explicit), and non-params candidates over expanded params forms. If no single best member exists, CS0121 ambiguity error results.

- Exact parameter type match beats conversion from `int` to `long`.
- More specific derived type beats base type when both are candidates.
- Named arguments and casts can disambiguate intentional choices.

---

#### Q9. Why can't you overload methods by return type alone?

**Concepts**
- Call sites often ignoring return values preventing return-type disambiguation
- Method signature identity comprising name and parameter types only
- Explicit interface implementation exposing conflicting return types

**Answer**

Call sites often ignore return values, so the compiler cannot infer which overload to invoke from return type context alone. Method signature identity for overload purposes includes name and parameter types, not the return type.

- `int GetValue()` and `string GetValue()` with identical parameters cannot coexist.
- Async overloads differ by return type (`Task` vs `Task<int>`) only when combined with `async` pattern and different parameter lists—or the async return type is part of a distinct signature in generic scenarios with constraints.
- Explicit interface implementation can expose conflicting return types on different interfaces implemented by one class.
- Return type participates in conversion targets after an overload is already chosen.

---

#### Q10. What is the difference between call-by-value for value types vs reference types at the parameter boundary?

**Concepts**
- Value type parameters receiving bit copy preventing caller mutation
- Reference type parameters copying pointer to shared object
- Object field mutation visible to caller without ref
- Parameter reassignment not affecting caller's variable without ref

**Answer**

Value type parameters receive a copy of the bits; mutating the parameter variable does not affect the caller's variable unless `ref` or `out` is used. Reference type parameters copy the reference; mutating the object's fields affects the shared object, but reassigning the parameter to a new object does not change the caller's variable without `ref`.

- `readonly` fields on structs passed by value cannot be mutated through the copy.
- Large structs should use `in` or `ref readonly` to avoid copy cost when read-only.
- Reference type null can be passed; callee can assign parameter to null without affecting caller's reference.

---

#### Q11. When should you use `ref` vs `out` vs `in` for parameters?

**Concepts**
- out for Try-pattern results definitely assigned in callee
- ref for read-and-write access to caller's existing variable
- in for read-only alias of large struct avoiding copy cost
- Canonical patterns: TryParse for out, swap for ref, transforms for in

**Answer**

Use `out` for Try-pattern results and multiple return values that are definitely assigned in the method. Use `ref` when the method must read and write an existing variable the caller already initialized. Use `in` for large structs the method reads but must not modify through the alias.

- `TryParse` is the canonical `out` pattern—see Q13.
- Swap methods and in-place sorting sometimes use `ref` on locals.
- Avoid `out` when the caller already has meaningful input in the variable that the callee should read (`ref` instead).
- `in` documents readonly intent to callers and analyzers.

---

#### Q12. What problem does the `in` modifier solve for large readonly structs?

**Concepts**
- Avoiding expensive copy for large value type parameters
- Readonly alias preventing callee mutation through parameter
- readonly struct combined with in for analyzer support
- Defensive copy occurrence in some edge callee storage scenarios

**Answer**

Passing a large struct by value copies every field, which is expensive for big value types like 3D transforms or matrix chunks. `in` passes a readonly reference alias, eliminating the copy while preventing silent mutation through the parameter.

- Call site uses `Method(in bigStruct)` for clarity at the API boundary.
- Defensive copies may still occur if the callee stores the parameter to a field in some scenarios (struct lifetime rules).
- Prefer `readonly struct` with `in` parameters together for clarity and analyzer support.
- Small structs (Point, int pairs) often remain pass-by-value for simplicity.

---

#### Q13. What is the Try-pattern (`bool TryX(..., out T result)`), and why is it preferred over exceptions for expected failures?

**Concepts**
- bool return signaling success/failure without exception overhead
- out parameter carrying result on success or default on failure
- TryGetValue, TryParse, TryParse as canonical BCL examples
- Exception use reserved for violated invariants and unexpected failures

**Answer**

Try-pattern methods return `false` when an operation cannot complete normally (parse failure, dictionary miss) and assign a default or meaningful `out` value, avoiding exception overhead for control flow that is common rather than exceptional.

- `Dictionary.TryGetValue`, `int.TryParse`, and `Enum.TryParse` follow this convention.
- Exceptions remain appropriate for violated invariants and unexpected environmental failures.
- Consistent naming (`Try` prefix, `out` last) makes APIs discoverable.

---

#### Q14. Can optional parameters precede required parameters — what are the ordering rules?

**Concepts**
- Required parameters before optional in parameter list
- Named arguments enabling optional before required in some call patterns
- API design placing rarely used flags last with defaults
- Compile error for required after optional without named argument workaround

**Answer**

Required parameters must appear before optional ones in the parameter list unless the trailing parameters are all optional and callers use named arguments to supply required values after skipped optionals—which is confusing and should be avoided.

- Valid: `void M(int a, int b = 0, int c = 0)`.
- Invalid: `void M(int a = 0, int b)` without special call patterns.
- API design places rarely used flags at the end with defaults.
- Changing default values does not update already compiled callers—Gotcha 13.

---

#### Q15. What is the difference between `params int[]` and passing an explicit `int[]` at the call site?

**Concepts**
- params enabling variadic individual-argument call syntax
- Explicit array passing bypassing variadic expansion
- Both arriving as array inside the method
- Hidden array allocation per params call site

**Answer**

Both end up as an array inside the method; `params` additionally allows variadic call syntax spreading individual arguments. Overload resolution treats explicit array argument as matching the array parameter directly without params expansion when an exact overload exists.

- `Method(new int[] { 1, 2 })` passes one array object; `Method(1, 2)` creates an array via params expansion.
- Null passed to params parameter can be ambiguous—prefer explicit array or overload without params.
- Performance-sensitive APIs may avoid params to reduce hidden array allocations.

---

#### Q16. When does overload resolution fail with ambiguity (CS0121), and how do casts or named arguments resolve it?

**Concepts**
- CS0121 ambiguity when two overloads are equally good matches
- Cast-to-target-type forcing specific overload selection
- Named argument reducing applicable candidate set
- Exact-type overload as long-term API fix

**Answer**

Ambiguity occurs when two overloads are equally good matches for the argument list, such as two implicit conversions of equal rank. Resolve by casting arguments to the intended parameter type, using named parameters to select an overload with fewer applicable candidates, or renaming methods to clarify intent.

- `(long)x` or `(int)x` disambiguates numeric overloads.
- Adding an overload with exact match types is the long-term API fix.
- Generic inference failures are a related but distinct compiler error family.
- Document overload sets carefully when adding optional and params overloads together.

---

#### Q17. What is the difference between a local function and a private instance method in the same class?

**Concepts**
- Local function scoped to single enclosing member
- Closure over enclosing method's locals in non-static local functions
- Private method accessible across all instance members of the class
- Static local function preventing accidental instance capture

**Answer**

Local functions are scoped inside a single member and can access local variables and parameters of the enclosing method via closure, while private methods are class-level members callable from any instance or static method in the class depending on modifiers.

- Static local functions cannot capture instance state unless passed explicitly—encourages clearer dependencies.
- Private methods appear in API surface of the type for testing and reuse across multiple members.
- Local functions are not virtual and cannot implement interfaces.
- Choose local functions for single-use algorithm steps tightly coupled to one method's locals.

---

#### Q18. What is recursion, what is a base case, and what risk does unbounded recursion pose?

**Concepts**
- Recursive method calling itself to solve smaller subproblems
- Base case returning without further recursive calls
- StackOverflowException from exhausting the call stack
- Absence of tail-call optimization guarantee in C#

**Answer**

Recursion is when a method calls itself to solve smaller subproblems; the base case is the condition where the method returns without further recursive calls. Unbounded recursion exhausts the call stack and throws `StackOverflowException`.

- Every recursive path needs a base case and progress toward it (smaller input, closer to terminal state).
- Tail recursion is not guaranteed to optimize to iteration in C#—do not rely on it for deep stacks.
- Tree and graph traversals use recursion with clear exit conditions or switch to explicit stacks for depth safety.
- Mutual recursion between two methods requires the same discipline on both sides.

---

#### Q19. Can `out` variables be declared inline at the call site (`TryParse(text, out int n)`)?

**Concepts**
- Inline out variable declaration at call site (C# 7+)
- Enclosing block scope for inline-declared out variable
- Definite assignment on true branch of TryParse
- Multiple inline out declarations for multi-output methods

**Answer**

Yes, C# 7+ allows declaring the type inline in the `out` argument position, scoping the new variable to the enclosing block and enabling concise Try-pattern usage without a separate declaration line.

- `if (int.TryParse(line, out int n))` uses `n` in the if block.
- Multiple inline `out` declarations in one call are supported when the method has multiple `out` parameters.
- The variable is definitely assigned when `TryParse` returns true per flow analysis.

---

#### Q20. What is the difference between mutating an object through a reference parameter vs reassigning the parameter variable itself?

**Concepts**
- Shared-object mutation visible to caller through reference parameter
- Parameter reassignment affecting local alias only without ref
- ref parameter enabling callee to replace caller's variable binding
- Value type parameter copying: mutation on copy only

**Answer**

Mutating fields on a reference-type object through a parameter (`customer.Name = "x"`) affects the caller's object because both refer to the same instance. Reassigning the parameter (`customer = new Customer()`) only changes the local alias inside the method unless the parameter is `ref Customer`.

- Gotcha 11 states this distinction explicitly for interviews.
- Value types always copy unless `ref`/`out`/`in`; mutation on struct parameter mutates the copy only.
- `ref` reassignment lets callee replace caller's variable binding: `void Reset(ref Customer c) { c = new Customer(); }`.
- Understanding this prevents bugs when trying to "replace" objects passed without `ref`.

---

### 08. Strings

#### Q1. Explain string handling in C# (`string` vs `StringBuilder`).

**Concepts**
- string as immutable reference type for stable text
- StringBuilder as mutable buffer for repeated append operations
- O(n²) allocation from loop concatenation with +
- string.Join and String.Concat for moderate multi-value joining

**Answer**

`string` is an immutable reference type optimized for relatively stable text, while `StringBuilder` provides a mutable buffer for repeated append operations that would otherwise create many intermediate string objects. Choose `string` for simple composition; choose `StringBuilder` for many updates in loops.

- `string` methods like `Replace` and `Substring` return new instances without modifying the original.
- `StringBuilder` exposes `Append`, `Insert`, and `Remove` mutating an internal buffer with amortized growth.
- Interpolation and `string.Join` often suffice without `StringBuilder` for moderate concatenation.

---

#### Q2. What are the different ways to format strings in C#? (`String.Format`, interpolation, composite formatting)

**Concepts**
- Composite formatting with indexed placeholder substitution
- String interpolation as compiler-translated format call
- IFormatProvider for culture-sensitive formatting
- StringBuilder.AppendFormat combining mutable buffer with composite

**Answer**

Composite formatting (`string.Format`, `Console.WriteLine` with placeholders) uses indexed holes and format providers. String interpolation (`$"..."`) embeds expressions directly. Both ultimately call formatting infrastructure with optional `IFormatProvider`.

- Interpolation is translated to `FormattableString` or `string.Format` calls at compile time.
- Culture-sensitive formatting passes `CultureInfo` explicitly or uses current culture by default.
- `StringBuilder.AppendFormat` combines mutable buffers with composite patterns.
- See Input & Output Q4 and Q8 for console-specific usage.

---

#### Q3. Are strings mutable or immutable in C#? What are the implications?

**Concepts**
- String immutability: every operation returning new string instance
- Thread-safety from content stability without locking
- O(n²) allocation from repeated concatenation in loops
- String interning of literals enabled by immutability

**Answer**

Strings are immutable: after construction, their character content cannot change. Any operation that appears to modify a string returns a new string instance, which simplifies threading, interning, and hash caching at the cost of allocations when building large text incrementally.

- Safe to share string references across threads without locks for content stability.
- Repeated concatenation in loops allocates O(n²) total characters without `StringBuilder`.
- Custom APIs should not expose mutable char buffers as `string`; use `StringBuilder` or `char[]` internally until finalized.
- Immutability enables the compiler and runtime to intern literal strings—see Q4.

---

#### Q4. What is string interning?

**Concepts**
- Intern pool storing one copy of each unique string content
- Automatic interning of string literals at compile/load time
- string.Intern for explicit lookup or insertion into pool
- Process-lifetime retention of interned strings

**Answer**

String interning stores one copy of each distinct literal or interned string content in a pool so multiple references can share the same object, saving memory and enabling reference equality for identical content when interned.

- Literal `"hello"` in source may be interned automatically at compile/load time.
- `string.Intern` forces lookup or insertion into the intern pool—see Q15.
- Two equal non-interned strings compare equal with `==` but may not be reference-equal—Gotcha 1.
- Interning trades memory deduplication for lifetime pinning of strings never collected while referenced from the pool.

---

#### Q5. What is the difference between `==`, `Equals`, `Compare`, and `CompareTo` for strings?

**Concepts**
- == and Equals comparing content with ordinal-default semantics
- string.Compare returning signed ordering with explicit StringComparison
- CompareTo implementing IComparable<string> for default sort order
- StringComparison enum for explicit ordinal or culture-sensitive choice

**Answer**

`==` and instance `Equals` compare string content with ordinal or overloaded semantics depending on overload; `string.Compare` returns signed ordering with explicit `StringComparison`; `CompareTo` implements `IComparable<string>` for default sort order.

- Use `StringComparison` overloads to avoid culture surprises—see Q6.
- `Compare` is static and accepts comparison type explicitly; good for sort keys.
- Reference equality differs from content equality unless interning aligns references—Q16.
- `Equals` overload without comparison uses ordinal default in modern .NET for `string.Equals(string)`.

---

#### Q6. When should you use `StringComparison.Ordinal` vs `OrdinalIgnoreCase` vs culture-sensitive comparisons?

**Concepts**
- Ordinal for identifiers, paths, protocol tokens, and dictionary keys
- OrdinalIgnoreCase for case-insensitive identifiers like HTTP headers
- Culture-sensitive for user-visible text sorting and natural language matching
- Turkish I problem illustrating culture-sensitive casing dangers

**Answer**

Use ordinal comparisons for identifiers, file paths, protocol tokens, and dictionary keys where byte-level Unicode order is stable. Use culture-sensitive comparison for user-visible sorting and matching words in natural language. Use `OrdinalIgnoreCase` for case-insensitive identifiers like HTTP headers or enum-like names when culture rules would be wrong.

- Culture-sensitive `string.Compare("i", "I", culture)` differs between Turkish and invariant for dotted/dotless I.
- LINQ and dictionary keys for internal IDs should use ordinal comparers.
- UI sort in user's language uses `StringComparison.CurrentCulture` or explicit culture.
- Security-sensitive comparisons (passwords, tokens) use fixed rules—often ordinal or fixed-time specialized APIs.

---

#### Q7. What are verbatim string literals (`@"..."`), and when are they useful?

**Concepts**
- Verbatim @ prefix making backslashes literal
- Doubled quote "" inside verbatim strings
- Windows path and regex pattern readability
- Literal newlines in source with verbatim strings

**Answer**

Verbatim strings prefix `@` so backslashes are literal and quotes are doubled (`""`) instead of escaped, which simplifies Windows paths, regular expression patterns, and multi-line text without doubling backslashes.

- `@"C:\Users\name"` avoids `"C:\\Users\\name"`.
- Newlines in verbatim strings are literal line breaks in source.
- Interpolation combines `$` and `@`: `$@"Hello {name}\there"`.
- Raw string literals (Q8) supersede some verbatim use cases for embedded quotes.

---

#### Q8. What are raw string literals (`"""..."""`, C# 11+), and how do they handle quotes and newlines?

**Answer**

Raw string literals delimit content with triple quotes (`"""`) and optional indentation stripping, allowing arbitrary quotes and multi-line JSON, SQL, or XML without escape proliferation.

- Opening quotes on their own line enable content that starts on the next line with dedented margins.
- More `"` characters in delimiter handle content containing triple quotes.
- Combine with `$` for interpolation inside raw strings with rules for brace placement on separate lines when needed.
- Prefer raw strings over heavily escaped verbatim strings for embedded code or markup templates.

---

#### Q9. What is the difference between `StringBuilder` and repeated string concatenation in a loop?

**Concepts**
- Loop concatenation creating new string per iteration with O(n²) copy
- StringBuilder amortized buffer growth reducing total copying
- string.Join and String.Concat for batch joining efficiency
- Profiling to confirm StringBuilder benefit before optimizing

**Answer**

Loop concatenation with `+` or `$"{s}{item}"` creates a new string each iteration, copying all prior content repeatedly. `StringBuilder` amortizes growth across a buffer, reducing total copying to roughly linear in final length.

- For small loops or few iterations, concatenation is readable and fast enough.
- Profile before optimizing; `StringBuilder` has overhead for tiny results.
- `string.Join` and `String.Concat` with array or span inputs batch concatenation efficiently.

---

#### Q10. What is the difference between `string.Concat`, the `+` operator, and interpolation for combining text?

**Concepts**
- All three producing new immutable string instances
- Compile-time constant folding for adjacent string literals
- String.Concat(ReadOnlySpan<string>) for modern allocation reduction
- Readability as primary differentiator for non-hot-path code

**Answer**

All produce new immutable strings; the compiler often optimizes simple chains of `+` on constants into one literal. Interpolation and `Concat` clarify intent for multiple parts; runtime behavior converges on allocation of a new string with combined content.

- Constant folding merges `"a" + "b"` at compile time.
- Interpolation evaluates expressions once into temporaries before formatting.
- `String.Concat(ReadOnlySpan<string>)` reduces allocations in modern APIs.
- Choose based on readability; micro-differences matter only in hot paths.

---

#### Q11. What is the difference between `IsNullOrEmpty`, `IsNullOrWhiteSpace`, and checking `Length == 0`?

**Concepts**
- IsNullOrEmpty returning true for null or zero-length strings
- IsNullOrWhiteSpace also treating Unicode whitespace-only as empty
- Length == 0 requiring non-null reference to avoid NullReferenceException
- Null-conditional before Length: s?.Length == 0

**Answer**

`IsNullOrEmpty` is true for null or zero-length strings. `IsNullOrWhiteSpace` also treats Unicode whitespace-only strings as empty. Checking `Length == 0` requires non-null reference or throws if null.

- Use null-conditional before length: `s?.Length == 0` distinguishes null from empty if needed.
- Whitespace includes spaces, tabs, and culture-specific space characters beyond `' '`.
- Validation of user names often needs `IsNullOrWhiteSpace`; protocol tokens may allow internal spaces but not empty.
- NRT flow analysis may still require null checks before dereferencing.

---

#### Q12. What is the difference between culture-sensitive (`ToUpper()`) and invariant (`ToUpperInvariant()`) case conversion?

**Concepts**
- Culture-sensitive casing using CurrentCulture Turkish-I rules
- InvariantCulture casing stable across machines for internal keys
- ToUpperInvariant for protocol identifiers and lookup normalization
- Security-sensitive normalization needing fixed rules

**Answer**

Culture-sensitive casing uses rules of `CurrentCulture` or Turkish etc., which can change dotted/dotless I behavior. Invariant casing uses fixed Unicode rules independent of user locale, preferred for identifiers and normalized keys.

- `ToUpper()` without culture uses current culture—dangerous for internal keys in global apps.
- `ToUpperInvariant()` is stable across machines for protocol identifiers.
- Security-sensitive normalization may need custom rules beyond simple casing.

---

#### Q13. What methods would you use to split, trim, replace, pad, and search within strings?

**Concepts**
- Split / TrimStart / TrimEnd / Replace / PadLeft / PadRight
- Contains / IndexOf / StartsWith / EndsWith with StringComparison
- Span-based overloads for allocation-free parsing pipelines
- StringSplitOptions.RemoveEmptyEntries for CSV-like cleaning

**Answer**

`Split` divides on separators; `Trim`/`TrimStart`/`TrimEnd` remove whitespace or specified chars; `Replace` substitutes substrings; `PadLeft`/`PadRight` align fixed-width fields; `Contains`, `IndexOf`, and `StartsWith`/`EndsWith` search with optional `StringComparison`.

- Span-based overloads on modern .NET reduce allocations for parsing pipelines.
- `Split` with `StringSplitOptions.RemoveEmptyEntries` cleans CSV-like input.
- Regular expressions handle complex patterns when literal methods are insufficient.
- `ReadOnlySpan<char>` slicing previews stack-friendly parsing without substring allocation in advanced scenarios.

---

#### Q14. What is UTF-16 storage in .NET strings, and how does that relate to surrogate pairs and `char`?

**Concepts**
- UTF-16 code units stored in contiguous .NET string buffer
- char as 16-bit code unit representing most Unicode characters
- Surrogate pairs for supplementary characters beyond BMP
- Length counting code units not perceived grapheme clusters
- StringInfo and Rune for grapheme-aware and code-point iteration

**Answer**

.NET `string` stores UTF-16 code units in a contiguous buffer; most characters are one `char`, but supplementary Unicode characters (emoji, rare scripts) occupy two `char` surrogate pairs. Length counts code units, not grapheme clusters users perceive as one character.

- Iterating `foreach (char c in s)` visits code units, not full Unicode scalars—use `StringInfo` or `Rune` for grapheme-aware logic.
- `char` is 16-bit UTF-16 code unit, not a full Unicode code point in all cases.
- Encoding to UTF-8 for wire formats uses `Encoding.UTF8.GetBytes` producing variable byte lengths.
- Surrogate pair corruption occurs if you manually splice strings at wrong indices.

---

#### Q15. What is the string intern pool, and what does `string.Intern` do?

**Concepts**
- Intern pool deduplicating equal string content via shared reference
- string.Intern returning pooled reference or inserting new entry
- Process-lifetime retention of interned strings
- Avoid interning unbounded user input to prevent pool growth

**Answer**

The intern pool is a runtime table of unique string contents; `string.Intern` returns the pooled reference for the argument's content, creating an entry if absent. Literals may already be interned without explicit calls.

- Useful rarely for deduplicating massive repeated dynamic strings with identical content.
- Interned strings live for process lifetime if referenced from pool—memory trade-off.
- Do not intern unbounded user input—it can grow the pool without bound in pathological cases.

---

#### Q16. Why can two strings with identical content fail `ReferenceEquals` while still passing `==`?

**Concepts**
- string == comparing character content not object identity
- ReferenceEquals checking heap object identity only
- Separately constructed strings creating different heap objects
- Interning aligning identity with content only when explicitly used

**Answer**

`==` for strings compares character content (ordinal by default in many overload paths), while `ReferenceEquals` checks object identity. Two separately constructed strings with the same text are equal by content but may be different heap objects unless interning aligns them.

- Literal `"hi"` reused in source may be the same reference; `new string("hi".ToCharArray())` is often not.
- Gotcha 1 is the canonical interview trap on interning.
- Do not use `ReferenceEquals` for string content comparison—use `==` or `Equals` with explicit comparison.
- Performance-sensitive deduplication sometimes interns known keys intentionally.

---

#### Q17. What is the difference between `Substring` and range/index syntax (`s[start..end]`) for slicing strings?

**Concepts**
- Substring(start, length) using start and length parameters
- Range syntax s[start..end] using inclusive start and exclusive end
- Hat (^) index from-end syntax for negative offsets
- Both allocating new strings; AsSpan avoiding allocation for downstream spans

**Answer**

Both extract contiguous portions; `Substring(start, length)` uses start and length, while range syntax `s[start..end]` uses start inclusive and end exclusive indices with clearer intent for "from here to there."

- Negative indices from end work with `^` in ranges: `s[^3..]` last three characters.
- Both allocate new strings because strings are immutable.
- Out-of-range indices throw `ArgumentOutOfRangeException` similarly.
- Span `s.AsSpan(start, length)` avoids allocation when downstream APIs accept span.

---

#### Q18. When is `StringBuilder` not the best choice despite many append operations?

**Concepts**
- Pre-sized char[] or string.Create for known final length
- Two to three appends being cleaner with interpolation or Concat
- string.Create(length, state, callback) for expert zero-copy construction
- Logging frameworks handling batching without application StringBuilder

**Answer**

When the final size is known upfront, a pre-sized `char[]` or single `string.Create` call may allocate once without `StringBuilder` overhead. Very few appends (two or three) are often clearer with interpolation or `Concat`.

- `string.Create(length, state, callback)` fills a buffer in one shot for expert scenarios.
- Logging frameworks batch efficiently without manual `StringBuilder` in application code.
- Pooling `StringBuilder` instances (`StringBuilderCache` internally in BCL) is framework concern, not typical app code.
- Measure: small `StringBuilder` growth copies dominate only at scale.

---

#### Q19. How does string interpolation handle format specifiers and alignment (`$"{price:C2}"`, `$"{name,-20}"`)?

**Concepts**
- Interpolation holes supporting alignment after comma and format after colon
- C2 currency format applying culture-aware symbol and decimals
- Negative alignment for left-padding in columnar console output
- FormattableString.Invariant for culture-invariant interpolation

**Answer**

Interpolation holes accept format after colon (`:C2` currency two decimals) and alignment after comma (`,-20` left-align width 20), mirroring composite formatting rules inside `{expression,alignment:format}`.

- Alignment pads with spaces by default; format uses current culture unless `FormattableString.Invariant` or custom culture is applied.
- Complex formats delegate to `IFormattable` on the expression's type.
- Constant format strings enable compile-time checking in some analyzers for correctness.
- See Input & Output Q12 for console column layout examples.

---

#### Q20. What is the performance implication of calling `Replace` or `Trim` on large strings repeatedly?

**Concepts**
- Each call scanning full string and allocating new instance on change
- Chained passes multiplying work for megabyte-scale text
- MemoryExtensions, compiled regex, or streaming readers for hot paths
- Profiling with realistic payload sizes before optimizing

**Answer**

Each call scans the full string and allocates a new string when changes occur, so chaining many passes over megabyte-scale text multiplies work and garbage. Prefer single-pass algorithms, spans, or `StringBuilder` pipelines for heavy text processing.

- `Replace` in a loop searching changing patterns can degrade badly without `StringBuilder`.
- Immutable returns mean no in-place win even when only one character changes.
- For hot paths, consider `MemoryExtensions`, regex compiled once, or streaming readers.
- Profile with realistic payload sizes before optimizing string pipelines.

---

### 09. Arrays

#### Q1. What are arrays in C#? How is memory managed for single-dimensional, multi-dimensional, and jagged arrays?

**Concepts**
- Fixed-rank contiguous heap reference types deriving from System.Array
- Single-dimensional array: one contiguous element buffer
- Rectangular [,] array: one object with row-major element layout
- Jagged [][] array: outer array of inner array references
- Array variable as reference to heap object not inline storage

**Answer**

Arrays are reference types holding a contiguous (per dimension rules) sequence of elements with fixed rank established at creation. Single-dimensional arrays store elements in one block; rectangular multi-dimensional arrays store one block with row-major layout; jagged arrays are arrays of arrays, each row potentially different length on the heap.

- Single-dim: `int[]` object header plus element buffer on heap.
- Rectangular `int[,]` stores all cells in one array object with two lengths.
- Jagged `int[][]` has outer array referencing separate inner arrays—non-uniform row lengths possible.
- All array objects are heap-allocated; the variable is a reference—see Q5.

---

#### Q2. What is a jagged array?

**Concepts**
- Array of arrays allowing variable-length rows
- int[][] syntax with outer array referencing separate inner arrays
- Two-step element access: jagged[i][j] with per-row null checks
- Memory trade-off: extra pointer per row versus rectangular locality

**Answer**

A jagged array is an array whose elements are themselves arrays (`int[][]`), allowing each row to have a different length, unlike rectangular `int[,]` where every row shares the same column count in one matrix object.

- Useful for sparse or ragged tables where rectangular storage would waste space.
- Access is two-step: `jagged[i][j]` with separate null checks for each row array.
- Memory layout differs from rectangular 2D—see Q15.
- Initialization often loops creating each inner array explicitly.

---

#### Q3. What is the difference between `Array.Copy()`, `Clone()`, and assigning one array variable to another?

**Concepts**
- Assignment copying reference only — both variables share same array
- Clone() shallow-copying elements into new array object
- Array.Copy copying element range to destination array
- Deep copy requiring per-element clone for reference-type elements

**Answer**

Assignment copies the reference only—both variables point to the same array object. `Clone()` on arrays performs shallow copy of elements into a new array object (new array, same element references for reference types). `Array.Copy` copies a range of elements from source to destination array, which may be existing or sized appropriately.

- Deep copy of elements requires looping or serialization, not `Clone()` alone for reference-type elements.
- `Copy` respects overlapping regions with defined behavior for same-array copies.
- Assignment does not duplicate elements—mutations visible through both references.

---

#### Q4. What is the difference between a single-dimensional array, a rectangular multi-dimensional array (`[,]`), and a jagged array (`[][]`)?

**Concepts**
- Single-dimensional []: one contiguous vector element block
- Rectangular [,]: one object with [row, col] dual-index access
- Jagged [][]: outer array referencing per-row inner arrays
- Rectangular for dense fixed-grid; jagged for variable row lengths

**Answer**

Single-dimensional arrays model vectors; rectangular arrays model fixed grid dimensions with one object; jagged arrays model rows as independent arrays allowing ragged shapes.


Choose rectangular for dense matrices with fixed columns; jagged for variable row lengths.

---

#### Q5. Are arrays value types or reference types in C#?

**Concepts**
- Arrays as reference types inheriting from System.Array
- Element storage inline in array object regardless of element type
- Default array variable value is null not an empty array
- Array.Empty<T>() singleton for zero-length arrays

**Answer**

Arrays are reference types inheriting from `System.Array`, regardless of whether their elements are value or reference types. The array variable holds a reference to the heap object containing lengths and element storage.

- `int[]` is a reference type; elements are value types stored inline in the array buffer.
- Default value of an array variable is `null`, not an empty array.
- `default(int[])` is null; use `Array.Empty<int>()` for zero-length singleton.
- Passing arrays to methods passes reference copy—see Q16.

---

#### Q6. What is array covariance for reference types, and why is `object[] arr = new string[3]; arr[0] = 42;` dangerous?

**Concepts**
- Covariance permitting string[] assignment to object[] at compile time
- ArrayTypeMismatchException on incompatible element write
- Read safety versus write danger of covariant array assignments
- IReadOnlyList<T> as safe alternative without write holes

**Answer**

Covariance allows assigning a derived array to a base array reference (`string[]` to `object[]`), but writing a non-compatible element through the base reference throws `ArrayTypeMismatchException` at runtime even though the assignment compiled.

- Covariance is safe for reading when element types match variance rules; writes can fail.
- Value type arrays are not covariant to `object[]` without boxing each element in a new array.
- Gotcha 8 is the classic interview example.
- Prefer `IReadOnlyList<T>` or generics for safe heterogeneous read scenarios without write holes.

---

#### Q7. What do `Array.Resize`, `Array.Fill`, and `Array.Clear` do — which allocate new memory?

**Concepts**
- Array.Resize creating new array and copying elements (allocates)
- Array.Fill setting all elements to value in-place (no allocation)
- Array.Clear zeroing or nulling element range in-place (no allocation)
- List<T> preference over Array.Resize for frequent growth

**Answer**

`Array.Resize` creates a new array of the specified size and copies elements from the old array, replacing the reference passed by ref—it allocates new memory. `Array.Fill` sets all elements to a value in an existing array without reallocating. `Array.Clear` zeroes or nulls a range in an existing array without reallocating.

- `Resize` is O(n) copy; use `List<T>` when frequent growth is needed.
- `Fill` and `Clear` mutate in place for existing buffers.
- Clearing sets value types to zero and references to null.
- Distinguish `Array.Clear` from `list.Clear()` which removes elements in dynamic lists.

---

#### Q8. What is the difference between `Length` on a single-dimensional array vs `GetLength(dimension)` on multi-dimensional arrays?

**Concepts**
- Length returning total element count across all dimensions
- GetLength(n) returning element count for specific dimension n
- Rank property for number of dimensions in rectangular arrays
- Loop bound API selection matching the array type

**Answer**

`Length` on single-dimensional arrays returns total element count. Multi-dimensional arrays expose `Rank` and per-dimension lengths via `GetLength(0)`, `GetLength(1)`, etc., because `Length` returns total elements across all dimensions.

- `int[,]` with 3 rows and 4 columns has `Length == 12` and `GetLength(0) == 3`, `GetLength(1) == 4`.
- Jagged outer array `Length` is row count; inner arrays have their own lengths.
- Bounds checks use these lengths on each access.
- Loop bounds should call the correct API for the array type to avoid logic errors.

---

#### Q9. How do you initialize arrays with collection initializer syntax and `new int[] { 1, 2, 3 }`?

**Concepts**
- Array initializer syntax: new Type[] { elements }
- Target-typed new[] { } with inferred element type
- Rectangular array nested brace syntax with uniform row lengths
- Collection expression syntax [1, 2, 3] targeting arrays in newer C#

**Answer**

Array initializer syntax lists elements in braces after `new Type[]` or with target-typed `new[] { 1, 2, 3 }` when the variable type is known. The compiler allocates an array of the correct size and assigns elements in order.

- Multi-dimensional rectangular arrays use nested brace syntax with uniform row lengths.
- Jagged arrays often combine outer initializer with per-row `new int[size]`.
- Collection expression syntax (`[1, 2, 3]`) in newer C# can target arrays and spans in some contexts.
- Initializers run before array reference is published to callers.

---

#### Q10. What is the relationship between arrays and `params` parameters in methods?

**Concepts**
- params as syntactic sugar for single-dimensional array parameter
- Compiler collecting individual arguments into array at call site
- params last parameter and single-instance rule
- ReadOnlySpan<T> overloads alongside params for performance APIs

**Answer**

A `params` parameter is syntactic sugar for a single-dimensional array parameter; callers may pass individual arguments that the compiler collects into an array, or pass an array directly.

- See Methods Q2 and Q15.
- Params arrays must be last in the signature—Gotcha 12.
- Only one params parameter per method.
- Prefer `ReadOnlySpan<T>` overloads in performance APIs alongside params for flexibility.

---

#### Q11. What is the difference between shallow copy of an array reference and copying array elements?

**Concepts**
- Reference copy: both variables pointing to same array object
- Shallow element copy: new array with same references to nested objects
- Deep copy requirement for mutable reference-type elements
- Immutable elements simplifying reasoning after shallow copy

**Answer**

Copying the reference aliases the same array object; copying elements into a new array duplicates the slot values—for reference-type elements, shallow copy duplicates references to the same nested objects, not deep clones of those objects.

- `Array.Copy` and `Clone` perform shallow element copy into new array storage.
- Deep copy requires per-element clone logic for mutable reference types.
- Assignment `b = a` shares identity; mutating `b[i]` affects `a[i]`.
- Immutability of elements simplifies reasoning after shallow array copy.

---

#### Q12. When would you use `Array.Sort` vs LINQ `OrderBy` on an array?

**Concepts**
- Array.Sort sorting in-place with low memory overhead
- LINQ OrderBy returning new deferred ordered sequence
- Array.BinarySearch requiring prior Array.Sort for sorted input
- Stable sort guarantee difference between Array.Sort and LINQ

**Answer**

`Array.Sort` sorts in place mutating the original array with efficient memory use and optional custom comparer. LINQ `OrderBy` returns a new ordered sequence (often deferred) without requiring in-place mutation, integrating with IEnumerable pipelines.

- Sorting large arrays in memory-critical code often prefers `Array.Sort`.
- Functional style chaining uses `OrderBy` then `ToArray()` if a new array is acceptable.
- `Array.Sort` throws if comparer violates contract; stable sort behavior differs from LINQ sort guarantees in some providers.
- Partial sorts and binary search (`Array.BinarySearch`) assume sorted in-place arrays.

---

#### Q13. What bounds-checking behavior does C# provide for array indexing?

**Concepts**
- Runtime bounds check on every array index access
- IndexOutOfRangeException for index < 0 or >= Length
- JIT eliminating redundant checks in provably safe loops
- Span<T> providing similar bounds checks with optimization opportunities

**Answer**

Every array index access is bounds-checked at runtime, throwing `IndexOutOfRangeException` when the index is less than zero or greater than or equal to the length. The Just-In-Time (JIT) compiler may optimize checks when it can prove safety in tight loops.

- Multi-dimensional indexing checks each dimension separately.
- `Span<T>` and `Memory<T>` provide similar checks with potentially better optimization paths.
- Unchecked unsafe pointer access bypasses checks—unsafe context required.
- Off-by-one errors at `Length` index are a common bug source despite checks.

---

#### Q14. What is `Span<T>`/`ReadOnlySpan<T>` in relation to arrays (preview — stack-friendly views)?

**Concepts**
- Span<T> as stack-only ref struct view over contiguous memory
- AsSpan() creating slice window without element copy
- ReadOnlySpan<char> for stack-friendly string parsing
- Ref struct restriction preventing heap storage of spans

**Answer**

`Span<T>` is a stack-only ref struct view over contiguous memory such as arrays, strings (via `ReadOnlySpan<char>`), or stackalloc buffers, providing slice and parse operations without allocating subarrays.

- `array.AsSpan(start, length)` creates a window without copying elements.
- Spans enable modern high-performance APIs (`TryParse` on spans, UTF-8 processing).
- Cannot be stored on heap fields directly due to ref struct restrictions (with exceptions for ref fields in newer versions in limited scenarios).
- Module 03 and performance chapters expand span usage patterns.

---

#### Q15. How do jagged arrays differ in memory layout from rectangular 2D arrays?

**Concepts**
- Rectangular: single contiguous object with stride-based element access
- Jagged: outer reference array plus separate inner array objects
- Rectangular locality advantage for dense fixed-size matrices
- Jagged flexibility for ragged row lengths at extra indirection cost

**Answer**

Rectangular arrays allocate one object with all cells in a single block indexed by `[row, col]`. Jagged arrays allocate an outer array of references, each pointing to a separate inner array object that may differ in length, causing more indirection and potential cache misses but saving space for ragged data.

- Rectangular: better locality for dense fixed-size matrices.
- Jagged: flexible row lengths, extra pointer per row.
- Serialization formats may prefer one shape over the other for compatibility.
- Choose based on data shape and access patterns, not syntax preference alone.

---

#### Q16. What happens when you pass an array to a method — can the callee change the caller's array contents?

**Concepts**
- Array parameter passing reference copy to same heap object
- Element mutation visible to caller through shared array reference
- Parameter reassignment not affecting caller's variable without ref
- Array.Resize requiring ref parameter to resize caller's array

**Answer**

The callee receives a copy of the reference pointing to the same array object, so mutating elements (`arr[0] = 99`) is visible to the caller. Reassigning the parameter to a new array object does not change which array the caller's variable references unless `ref` is used on the array parameter.

- Length is fixed after creation; callee cannot resize caller's array without `ref` and `Array.Resize` on caller's variable.
- Null assignment to parameter does not null caller's reference.
- Same semantics as reference types generally—see Methods Q10 and Q20.
- Returning a new array is common when transformation changes size.

---

### 10. Exception Handling

#### Q1. Explain exception handling in C# (`try`, `catch`, `finally`, `throw`, and custom exceptions).

**Concepts**
- try / catch / finally as structured exception handling blocks
- Catch ordering: most specific exception types before general
- finally running cleanup regardless of exception or normal return
- Custom exceptions deriving from Exception for domain-specific context
- IDisposable and using for deterministic resource cleanup

**Answer**

`try` wraps code that may fail; `catch` handles specific exception types; `finally` runs cleanup whether or not an exception occurred; `throw` signals failure up the stack; custom exceptions derive from `Exception` to express domain-specific errors with context.

- Catch most specific types first; general `Exception` last if used at all.
- `finally` releases unmanaged resources or resets state; pair with `using` for `IDisposable`.
- Custom exceptions should be `[Serializable]` when remoting legacy scenarios matter and include useful messages, not control flow.
- Unhandled exceptions terminate the process in console apps unless a host catches them.

---

#### Q2. What is the difference between `throw` and `throw ex`?

**Concepts**
- throw preserving original exception stack trace on rethrow
- throw ex resetting stack trace to current catch location
- wrap-with-inner-exception pattern for adding context
- Stack trace diagnostic value for production troubleshooting

**Answer**

`throw;` inside a catch block rethrows the same exception object and preserves the original stack trace, while `throw ex;` throws a new exception reference that resets the stack trace to the current catch location, hiding where the failure originally occurred.

- Use `throw;` after logging to keep diagnostics intact—Gotcha 6.
- Wrap with `throw new CustomException("...", ex)` when adding context and passing inner exception explicitly.
- Never use bare `throw ex;` unless you intentionally want a fresh stack (rare).

---

#### Q3. Explain the `using` statement in the context of exception handling and resource management.

**Concepts**
- using compiling to try/finally calling IDisposable.Dispose
- Deterministic resource release for files, connections, handles
- Dispose called even when exception occurs inside using block
- using var declaration scoping dispose to enclosing block

**Answer**

The `using` statement compiles to try/finally that calls `Dispose()` on `IDisposable` objects when leaving scope, even if an exception occurs, ensuring files, connections, and handles release promptly instead of waiting for garbage collection.

- `using var stream = File.OpenRead(path);` disposes at end of enclosing block (using declaration).
- Nested `using` statements dispose in reverse order of acquisition.
- `Dispose` should not throw; implementers swallow secondary errors when possible.
- `using` does not catch exceptions—it guarantees disposal on exceptional paths.

---

#### Q4. What are exception filters in C#?

**Concepts**
- when clause on catch statements for conditional handling
- Filter running before entering catch block on type match
- Avoiding catch-and-rethrow pattern for conditional handling
- Filter throwing converting to FailedExceptionFilterException

**Answer**

Exception filters are `when` clauses on `catch` statements that run a boolean expression after matching the exception type but before entering the catch block, allowing conditional handling without catching and rethrowing to inspect state.

- `catch (Exception ex) when (ex.HResult == specificCode)` handles only matching cases.
- Filters must not throw; throwing converts to `FailedExceptionFilterException` wrapping the filter failure.
- Useful for logging correlation without losing stack via catch-and-rethrow patterns.
- Overuse complicates control flow; prefer typed exceptions when possible.

---

#### Q5. What is the difference between catching a specific exception type vs `catch (Exception)`?

**Concepts**
- Specific exception types for targeted anticipated failure recovery
- Broad Exception catch masking unexpected bugs
- Catch-log-rethrow at process boundaries for observability
- when filter narrowing broad catch without rethrowing

**Answer**

Catching specific types (`FormatException`, `IOException`) handles anticipated failures with targeted recovery, while catching `Exception` intercepts all managed exceptions including unexpected bugs, which often hides defects unless you rethrow after logging.

- Broad catch is acceptable at process boundaries (top-level handler) or when translating to user-safe messages before rethrow.
- Catch derived before base; unreachable catch blocks are compile errors.
- Filtering with `when` narrows broad catches without separate rethrow gymnastics.
- ASP.NET Core middleware often maps exception types to HTTP status codes selectively.

---

#### Q6. What happens if an exception is thrown inside a `finally` block?

**Concepts**
- New finally exception replacing or chaining with original exception
- Loss of original failure information without explicit inner exception logging
- Dispose implementations swallowing secondary errors
- Defensive try/catch inside finally for non-critical cleanup steps

**Answer**

If `finally` throws while unwinding from an earlier exception, the new exception typically replaces the original active exception (or is chained depending on runtime/version rules), and the original failure information may be lost unless captured in an inner exception or logged first.

- Avoid throwing from `finally`; log and swallow secondary failures during cleanup when primary error matters more.
- `Dispose` implementations should not throw if avoidable for this reason.
- Return statements in `finally` similarly override try outcomes—Gotcha 7.
- Design cleanup code defensively with try/catch inside finally for non-critical steps.

---

#### Q7. What is the base class hierarchy for exceptions in .NET (`Exception`, `SystemException`, application-specific types)?

**Concepts**
- All exceptions deriving from System.Exception
- SystemException as historical system versus application distinction
- Common BCL bases: ArgumentException, InvalidOperationException
- AggregateException wrapping multiple Task failures

**Answer**

All exceptions derive from `Exception`. Many BCL runtime errors derive from `SystemException` (historical distinction for system vs application). Application code typically throws `ApplicationException` subclasses or domain-specific types directly inheriting `Exception` or intermediate bases like `InvalidOperationException`.

- `ArgumentException`, `InvalidOperationException`, and `NotSupportedException` are common BCL bases for APIs.
- Do not catch `StackOverflowException` or `OutOfMemoryException` for recovery in most cases.
- `AggregateException` wraps multiple failures from parallel tasks.
- Custom hierarchies should be shallow and meaningful to callers and middleware.

---

#### Q8. When should you create a custom exception type vs using an existing BCL exception?

**Concepts**
- Existing BCL exceptions matching documented semantics for standard failures
- Custom types for callers needing programmatic domain failure distinction
- Custom properties carrying structured data (ErrorCode, EntityId)
- XML documentation specifying which exceptions public methods throw

**Answer**

Use existing BCL exceptions when the failure mode matches their documented semantics (`ArgumentNullException` for null args, `InvalidOperationException` for wrong object state). Create custom types when callers need to distinguish domain failures programmatically or attach structured data not expressible in message alone.

- Custom exceptions need meaningful names ending in `Exception` and optional custom properties (ErrorCode, EntityId).
- Avoid deep exception inheritance trees rarely caught at different levels.
- Prefer standard types in public libraries to reduce consumer catch proliferation.
- Document which exceptions public methods throw in XML docs for API consumers.

---

#### Q9. What is the difference between `using` statement and `using` declaration (`using var`) for disposal?

**Concepts**
- Classic using statement creating explicit block scope ending at closing brace
- using var declaration disposing at end of enclosing method or block
- Reverse declaration order for disposal at end of scope
- Both compiling to equivalent try/finally dispose pattern

**Answer**

Classic `using (var r = ...) { }` creates an explicit block scope ending with dispose at the closing brace. `using var r = ...;` disposes at the end of the enclosing scope (method or block), reducing nesting while preserving dispose-on-exit semantics.

- Both compile to equivalent dispose patterns with try/finally.
- `using var` disposal order at method end is reverse declaration order.
- Choose block form when dispose boundary is narrower than the whole method.
- Neither replaces `IAsyncDisposable` async using patterns for async disposal (`await using`).

---

#### Q10. Can you have multiple `catch` blocks — what is the order rule for catching derived vs base exceptions?

**Concepts**
- Multiple catch blocks for disjoint exception type handling
- Derived types required before base types to avoid unreachable catch
- One catch executing per thrown exception
- Exception filters enabling conditional skip of type-matched catch

**Answer**

Multiple catch blocks are allowed for disjoint types; the compiler requires more specific types before less specific bases because the first matching catch handles the exception. A derived catch after a base catch for the same hierarchy is unreachable and errors at compile time.

- Only one catch executes per thrown exception.
- Exception filters can skip a catch block even when type matches, allowing fall-through to later catches in some designs—use carefully.
- Empty catch blocks swallow errors—avoid except at intentional boundaries with logging.
- Rethrow with `throw;` after partial handling to let upstream catch broader policy.

---

#### Q11. What is `finally` guaranteed to do, and can it prevent an exception from propagating?

**Concepts**
- finally running on normal completion, exception, or return
- return in finally overriding or replacing pending exception
- Idempotent cleanup preferred in finally blocks
- Thread abort and process kill as catastrophic finally-skipping scenarios

**Answer**

`finally` runs when control leaves the associated try/catch via normal completion, exception, or return, making it suitable for cleanup. A `return` or uncaught exception thrown inside `finally` can override or replace pending exceptions from try, effectively changing propagation—see Gotcha 7.

- `finally` does not suppress try exceptions unless it completes normally without throwing and without return override quirks.
- Cleanup in finally should be idempotent when possible.
- Thread abort and process kill can skip finally in catastrophic scenarios—design critical durability with `try/finally` plus persistent state.
- Async finally in async methods runs as part of async state machine completion.

---

#### Q12. What is the difference between handled exceptions and unhandled exceptions in a console vs ASP.NET host?

**Concepts**
- Handled exceptions caught by application logic with recovery or translation
- Unhandled exceptions crashing console apps or producing HTTP 500
- AppDomain.UnhandledException and TaskScheduler.UnobservedTaskException hooks
- ASP.NET Core exception handler middleware for production/development responses

**Answer**

Handled exceptions are caught by application catch blocks that recover or translate errors without terminating the process. Unhandled exceptions propagate until the runtime or host default handler runs— crashing console apps or returning HTTP 500 in ASP.NET Core developer/production exception middleware.

- Top-level `AppDomain.UnhandledException` and `TaskScheduler.UnobservedTaskException` are last-chance hooks.
- ASP.NET Core maps unhandled exceptions to responses via exception handler middleware and logging.
- Catching at boundary and returning exit codes is console best practice for CLI tools.
- Logging handled exceptions still matters for observability even when user sees friendly message.

---

#### Q13. When is it appropriate to catch and swallow an exception vs rethrow?

**Concepts**
- Swallow only when failure fully handled with documented logging
- Rethrow when callers must react or transaction must abort
- throw; preserving stack on rethrow at layer boundaries
- Try-pattern preference over catch for expected parse failures

**Answer**

Swallow only when the failure is fully handled and documented (retry succeeded, optional feature unavailable) and logging captures context for diagnostics. Rethrow when callers must react, transaction must abort, or you lack authority to decide recovery—use `throw;` to preserve stack.

- Empty catch is a code smell unless idempotent probe operations (try read optional config).
- Catch-log-rethrow at layer boundaries preserves observability without losing stack via `throw;`.
- Do not swallow `OutOfMemoryException` hoping to continue reliably.
- Try-pattern preferred over catch for expected parse failures—Gotcha 10.

---

#### Q14. What is `ExceptionDispatchInfo`, and when is `throw;` insufficient?

**Concepts**
- ExceptionDispatchInfo capturing exception stack for cross-thread rethrow
- throw; limited to same logical catch stack frame
- AggregateException flattening for Task failure reporting
- Advanced parallel and library code scenarios for ExceptionDispatchInfo

**Answer**

`ExceptionDispatchInfo.Capture(ex)` stores an exception's stack for rethrow on another thread with `Throw()`, preserving the original stack in scenarios where bare `throw;` cannot cross async or thread boundaries cleanly.

- Useful when marshaling failures from background threads to request threads in advanced patterns.
- `throw;` only preserves stack when rethrowing on the same logical catch stack frame.
- `AggregateException` on tasks may flatten inner exceptions for reporting.
- Most application code never needs `ExceptionDispatchInfo`; know it for library and parallel code reviews.

---

#### Q15. What happens if both `try` and `finally` contain `return` statements?

**Concepts**
- finally return overriding the try return value
- finally return swallowing pending exception from try
- Avoid return in finally in application code
- Local result variable set in try and returned after finally as clarity pattern

**Answer**

The `finally` block executes before the method actually returns, and if `finally` contains its own `return`, that return value typically overrides the `try` return value, producing surprising results—Gotcha 7.

- Avoid `return` in `finally` in handwritten code.
- Same interaction applies when `try` throws but `finally` returns, potentially swallowing the exception.
- Refactoring to local result variables set in try and returned after finally clarifies intent.
- Control Flow Q15 cross-references this behavior.

---

#### Q16. What is the difference between `IDisposable.Dispose` and finalizers in exception-safe cleanup?

**Concepts**
- Dispose running deterministically via using for prompt resource release
- Finalizer running non-deterministically on GC as last-chance safety net
- GC.SuppressFinalize called when Dispose has succeeded
- SafeHandle encapsulating critical finalization for native resources

**Answer**

`Dispose` runs deterministically when `using` or explicit calls release resources promptly and should not throw. Finalizers (`~ClassName`) run non-deterministically on garbage collection as a last-chance safety net for unmanaged handles, unsuitable for timely release during normal exception flows.

- Always implement `Dispose` for unmanaged resources; suppress finalizer when dispose succeeds (`GC.SuppressFinalize`).
- Finalizers delay object collection and add GC overhead—Module 02 covers IDisposable vs finalizer depth.
- Exception during dispose should not mask original exception from try without careful logging.
- `SafeHandle` encapsulates critical finalization patterns for native resources.

---

### Gotchas — Module 01

#### Gotcha 1. String interning

**Answer** Many candidates assume two strings with the same text always share reference identity, but only interned literals and explicit interning guarantee that; separately constructed equal strings compare equal with `==` yet may fail `ReferenceEquals`.

- Literal `"hello"` assignments often alias; `new string('h', 5)` built at runtime typically does not.
- Rely on `==` or `Equals` for content, not `ReferenceEquals`, unless testing interning explicitly.
- See Strings Q4, Q15, and Q16 for the full interning model.

---

#### Gotcha 2. Integer division

**Answer** Developers expect `10 / 3` to yield a fractional result, but integer division truncates toward zero when both operands are integral types.

- Promote at least one operand to `double`, `decimal`, or `float` for fractional math.
- See Operators Q4 and Type Conversion Q14 for related numeric rules.
- Financial code should use `decimal` explicitly, not integer division with accidental truncation.

---

#### Gotcha 3. `const` vs runtime values

**Answer** `const` requires compile-time constants, so values like `DateTime.Now` or computed decimals at runtime cannot be `const`; use `readonly` fields or properties set in constructors instead.

- Callers embedding optional parameter defaults capture const values at compile time—related to Gotcha 13.
- See Data Types Q8, Q9, and Q28.

---

#### Gotcha 4. Boxing silently hurts performance

**Answer** Assigning value types to `object` or non-generic collections boxes each value on the heap, causing allocations invisible in source that accumulate in hot loops.

- Prefer `List<int>` over `ArrayList` for integers.
- Interface dispatch on structs often boxes—see Module 02 Gotcha 5.
- See Data Types Q4 and Q14.

---

#### Gotcha 5. Modifying a struct inside `foreach`

**Answer** The foreach iteration variable is a read-only copy of each element, so mutating fields on that copy does not update the collection and fails to compile when you try to assign to the iteration variable itself.

- Use `for` with index, `Span<T>`, or refactor to mutable reference types when in-place updates are required.
- C# 5 fixed closure capture for foreach but not struct mutation rules.

---

#### Gotcha 6. `throw;` vs `throw ex;`

**Answer** Rethrowing with `throw ex;` resets the stack trace to the catch line, destroying diagnostic context; `throw;` preserves the original failure site.

- Wrap with new exception types using inner exceptions when adding context intentionally.
- See Exception Handling Q2.

---

#### Gotcha 7. `return` in `try` vs `finally`

**Answer** `finally` always runs before the method completes, and a `return` inside `finally` can override the value or exception pending from `try`, producing surprising control flow.

- Never `return` from `finally` in application code.
- See Control Flow Q15 and Exception Handling Q11 and Q15.

---

#### Gotcha 8. Array covariance trap

**Answer** Assigning `string[]` to `object[]` compiles due to covariance, but storing an incompatible element like `42` throws `ArrayTypeMismatchException` at runtime on write.

- Covariance is read-safe in many scenarios; writes are the trap.
- See Arrays Q6.

---

#### Gotcha 9. Culture-sensitive parse/format

**Answer** The same string `"3,14"` parses as 314 or 3.14 depending on whether comma is a decimal or thousands separator under the active culture, breaking logs and APIs shared across locales.

- Use `InvariantCulture` for stored and transmitted formats; use explicit culture for localized UI input.
- See Input & Output Q5–Q7 and Type Conversion Q10.

---

#### Gotcha 10. `Parse` vs `TryParse` in user input paths

**Answer** `int.Parse` on invalid console input throws and can crash the app; `TryParse` treats failure as a normal branch suitable for reprompt loops.

- Exceptions are for exceptional conditions, not expected typos.
- See Input & Output Q3 and Q14 and Methods Q13.

---

#### Gotcha 11. `ref` reassignment vs mutation

**Answer** Reassigning a reference parameter to a new object does not change the caller's variable unless the parameter is `ref`; mutating fields on the shared object does affect the caller.

- See Methods Q10 and Q20.

---

#### Gotcha 12. `params` must be last

**Answer** Only one `params` array parameter is permitted and it must be the final parameter in the method signature; violating this is a compile error.

- See Methods Q2 and Q14.

---

#### Gotcha 13. Optional parameter defaults are compile-time

**Answer** Default argument values are baked into call sites at compile time, so changing a default in the method definition does not affect callers until they recompile.

- Prefer overloads or mandatory parameters for breaking default changes in public APIs.
- See Methods Q4 and Q14.

---

#### Gotcha 14. `checked` default is context-dependent

**Answer** Integer arithmetic wraps silently in unchecked default contexts; financial or checksum code may need explicit `checked` blocks or project settings to throw on overflow instead.

- See Operators Q2 and Q13.

---

#### Gotcha 15. Console encoding mismatch

**Answer** Writing Unicode text when `Console.OutputEncoding` and the terminal code page disagree produces replacement characters or mojibake, especially on Windows consoles not configured for UTF-8.

- Align console, process, and font encoding for international output.
- See Input & Output Q9.

---


