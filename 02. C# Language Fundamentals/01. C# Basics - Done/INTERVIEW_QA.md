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
  - [Q17. What is `char` in C# — is it a numeric type or a text type, and how does it relate to Unicode?](#q17-what-is-char-in-c-is-it-a-numeric-type-or-a-text-type-and-how-does-it-relate-to-unicode)
  - [Q18. What is the difference between `bool` and nullable `bool?` in terms of default values and usage?](#q18-what-is-the-difference-between-bool-and-nullable-bool-in-terms-of-default-values-and-usage)
  - [Q19. Explain the `??` (null-coalescing) and `??=` (null-coalescing assignment) operators with nullable types.](#q19-explain-the-null-coalescing-and-null-coalescing-assignment-operators-with-nullable-types)
  - [Q20. What is the difference between `var` and an explicit type declaration — when must you use explicit types?](#q20-what-is-the-difference-between-var-and-an-explicit-type-declaration-when-must-you-use-explicit-types)
  - [Q21. What are digit separators in numeric literals (e.g., `1_000_000`), and what problem do they solve?](#q21-what-are-digit-separators-in-numeric-literals-eg-1_000_000-and-what-problem-do-they-solve)
  - [Q22. What is the difference between `default(int)` and `default` for a reference type?](#q22-what-is-the-difference-between-defaultint-and-default-for-a-reference-type)
  - [Q23. What is a nullable reference type annotation (`string?` vs `string`), and is enforcement compile-time or runtime?](#q23-what-is-a-nullable-reference-type-annotation-string-vs-string-and-is-enforcement-compile-time-or-runtime)
  - [Q24. What happens when you assign `null` to a non-nullable reference type variable under `#nullable enable`?](#q24-what-happens-when-you-assign-null-to-a-non-nullable-reference-type-variable-under-nullable-enable)
  - [Q25. What is the difference between `object` as a universal base type and using `dynamic`?](#q25-what-is-the-difference-between-object-as-a-universal-base-type-and-using-dynamic)
  - [Q26. What are `nint` and `nuint`, and when might you encounter them?](#q26-what-are-nint-and-nuint-and-when-might-you-encounter-them)
  - [Q27. What is the difference between declaring a variable with and without an initializer?](#q27-what-is-the-difference-between-declaring-a-variable-with-and-without-an-initializer)
  - [Q28. Can you use `const` with user-defined types like `DateTime` or `decimal` computed at runtime? Why or why not?](#q28-can-you-use-const-with-user-defined-types-like-datetime-or-decimal-computed-at-runtime-why-or-why-not)

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
  - [Q17. When reading numeric input from users in different locales, what pitfalls arise with comma vs period decimal separators?](#q17-when-reading-numeric-input-from-users-in-different-locales-what-pitfalls-arise-with-comma-vs-period-decimal-separators)
  - [Q18. What is the purpose of `Console.ForegroundColor`, `BackgroundColor`, and resetting colors after use?](#q18-what-is-the-purpose-of-consoleforegroundcolor-backgroundcolor-and-resetting-colors-after-use)

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
  - [Q17. What is the `nameof` operator, and how is it used in validation messages and refactoring-safe code?](#q17-what-is-the-nameof-operator-and-how-is-it-used-in-validation-messages-and-refactoring-safe-code)

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
  - [Q17. What is the difference between a local function and a private instance method in the same class?](#q17-what-is-the-difference-between-a-local-function-and-a-private-instance-method-in-the-same-class)
  - [Q18. What is recursion, what is a base case, and what risk does unbounded recursion pose?](#q18-what-is-recursion-what-is-a-base-case-and-what-risk-does-unbounded-recursion-pose)
  - [Q19. Can `out` variables be declared inline at the call site (`TryParse(text, out int n)`)?](#q19-can-out-variables-be-declared-inline-at-the-call-site-tryparsetext-out-int-n)
  - [Q20. What is the difference between mutating an object through a reference parameter vs reassigning the parameter variable itself?](#q20-what-is-the-difference-between-mutating-an-object-through-a-reference-parameter-vs-reassigning-the-parameter-variable-itself)

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
  - [Q17. What is the difference between `Substring` and range/index syntax (`s[start..end]`) for slicing strings?](#q17-what-is-the-difference-between-substring-and-rangeindex-syntax-sstartend-for-slicing-strings)
  - [Q18. When is `StringBuilder` not the best choice despite many append operations?](#q18-when-is-stringbuilder-not-the-best-choice-despite-many-append-operations)
  - [Q19. How does string interpolation handle format specifiers and alignment (`$"{price:C2}"`, `$"{name,-20}"`)?](#q19-how-does-string-interpolation-handle-format-specifiers-and-alignment-pricec2-name-20)
  - [Q20. What is the performance implication of calling `Replace` or `Trim` on large strings repeatedly?](#q20-what-is-the-performance-implication-of-calling-replace-or-trim-on-large-strings-repeatedly)

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
  - [Q17. **String interning** — `string a = "hello"; string b = "hello"; a == b` is `true`, but two separately constructed strings may not be reference-equal even when content matches.](#q17-string-interning-string-a-hello-string-b-hello-a-b-is-true-but-two-separately-constructed-strings-may-not-be-reference-equal-even-when-content-matches)
  - [Q18. **Integer division** — `10 / 3` is `3`, not `3.33`. At least one operand must be floating-point for fractional results.](#q18-integer-division-10-3-is-3-not-333-at-least-one-operand-must-be-floating-point-for-fractional-results)
  - [Q19. **`const` vs runtime values** — You cannot use `const` with a value that requires computation (e.g., `DateTime.Now`); use `readonly` or a property instead.](#q19-const-vs-runtime-values-you-cannot-use-const-with-a-value-that-requires-computation-eg-datetimenow-use-readonly-or-a-property-instead)
  - [Q20. **Boxing silently hurts performance** — Assigning value types to `object` or non-generic collections causes heap allocations; repeated boxing in hot paths is a common production issue.](#q20-boxing-silently-hurts-performance-assigning-value-types-to-object-or-non-generic-collections-causes-heap-allocations-repeated-boxing-in-hot-paths-is-a-common-production-issue)
  - [Q21. **Modifying a struct inside `foreach`** — Compile error: the iteration variable is a copy. Use a `for` loop with index or `ref`/`Span` patterns.](#q21-modifying-a-struct-inside-foreach-compile-error-the-iteration-variable-is-a-copy-use-a-for-loop-with-index-or-refspan-patterns)
  - [Q22. **`throw;` vs `throw ex;`** — `throw ex;` resets the stack trace; `throw;` preserves the original.](#q22-throw-vs-throw-ex-throw-ex-resets-the-stack-trace-throw-preserves-the-original)
  - [Q23. **`return` in `try` vs `finally`** — `finally` always runs before the method actually returns; a `return` in `finally` can override the `try` return value.](#q23-return-in-try-vs-finally-finally-always-runs-before-the-method-actually-returns-a-return-in-finally-can-override-the-try-return-value)
  - [Q24. **Array covariance trap** — `object[] arr = new string[3]; arr[0] = 42;` compiles but throws `ArrayTypeMismatchException` at runtime.](#q24-array-covariance-trap-object-arr-new-string3-arr0-42-compiles-but-throws-arraytypemismatchexception-at-runtime)
  - [Q25. **Culture-sensitive parse/format** — `"3,14"` parses as 314 in `en-US` but as 3.14 in `de-DE`; logs and APIs should use `InvariantCulture` when format must be fixed.](#q25-culture-sensitive-parseformat-314-parses-as-314-in-en-us-but-as-314-in-de-de-logs-and-apis-should-use-invariantculture-when-format-must-be-fixed)
  - [Q26. **`Parse` vs `TryParse` in user input paths** — `int.Parse` on bad console input crashes the app; Try-pattern avoids exceptions for expected failure.](#q26-parse-vs-tryparse-in-user-input-paths-intparse-on-bad-console-input-crashes-the-app-try-pattern-avoids-exceptions-for-expected-failure)
  - [Q27. **`ref` reassignment vs mutation** — Reassigning a reference parameter does not change the caller's variable; mutating the object it points to does.](#q27-ref-reassignment-vs-mutation-reassigning-a-reference-parameter-does-not-change-the-callers-variable-mutating-the-object-it-points-to-does)
  - [Q28. **`params` must be last** — Only one `params` array parameter is allowed, and it must be the final parameter in the signature.](#q28-params-must-be-last-only-one-params-array-parameter-is-allowed-and-it-must-be-the-final-parameter-in-the-signature)
  - [Q29. **Optional parameter defaults are compile-time** — Changing a default value in a method signature does not update callers compiled against the old default unless recompiled.](#q29-optional-parameter-defaults-are-compile-time-changing-a-default-value-in-a-method-signature-does-not-update-callers-compiled-against-the-old-default-unless-recompiled)
  - [Q30. **`checked` default is context-dependent** — Integer overflow wraps silently in unchecked default contexts; financial code may need explicit `checked` blocks.](#q30-checked-default-is-context-dependent-integer-overflow-wraps-silently-in-unchecked-default-contexts-financial-code-may-need-explicit-checked-blocks)
  - [Q31. **Console encoding mismatch** — Writing Unicode to a console whose output encoding is not UTF-8 can display replacement characters or mojibake on Windows.](#q31-console-encoding-mismatch-writing-unicode-to-a-console-whose-output-encoding-is-not-utf-8-can-display-replacement-characters-or-mojibake-on-windows)
  - [Q1. (R) After a merge, `dotnet build` fails with CS0017 ("Program has more than one entry point defined"). Review these two files in the same console project. What conflicted, and how do you fix it?](#q1-r-after-a-merge-dotnet-build-fails-with-cs0017-program-has-more-than-one-entry-point-defined-review-these-two-files-in-the-same-console-project-what-conflicted-and-how-do-you-fix-it)
  - [Q2. (R) A developer copies a startup snippet into this repo's HelloWorld project (`ImplicitUsings` disabled). Build fails. What is wrong, and what would you change?](#q2-r-a-developer-copies-a-startup-snippet-into-this-repos-helloworld-project-implicitusings-disabled-build-fails-what-is-wrong-and-what-would-you-change)
  - [Q3. (R) A deployment script runs the published console tool with no arguments:](#q3-r-a-deployment-script-runs-the-published-console-tool-with-no-arguments)
  - [Q4. (P) A containerized .NET 8 worker uses only `Console.Write` (no newline) for progress dots during a long loop. Locally you see live output; in Kubernetes logs appear only after the process exits or crashes. Explain why and what you would change.](#q4-p-a-containerized-net-8-worker-uses-only-consolewrite-no-newline-for-progress-dots-during-a-long-loop-locally-you-see-live-output-in-kubernetes-logs-appear-only-after-the-process-exits-or-crashes-explain-why-and-what-you-would-change)
  - [Q5. (D) Your team maintains internal CLI tools and tutorial projects. Some use **top-level statements**, others use explicit `namespace` + `class Program` + `Main` (as in this chapter). What convention would you recommend for production CLIs vs learning repos, and why?](#q5-d-your-team-maintains-internal-cli-tools-and-tutorial-projects-some-use-top-level-statements-others-use-explicit-namespace-class-program-main-as-in-this-chapter-what-convention-would-you-recommend-for-production-clis-vs-learning-repos-and-why)

- [02. Data Types & Variables - Done](#02-data-types-variables---done)

- [02. Data Types & Variables - Done](#02-data-types-variables---done-1)
  - [Q1. (R) Finance QA reports order totals off by one cent on some invoices. Review this pricing helper copied from a prototype:](#q1-r-finance-qa-reports-order-totals-off-by-one-cent-on-some-invoices-review-this-pricing-helper-copied-from-a-prototype)
  - [Q2. (R) A loyalty API returns `int?` for optional points. After deploy, `NullReferenceException` and `InvalidOperationException` appear in logs. Review:](#q2-r-a-loyalty-api-returns-int-for-optional-points-after-deploy-nullreferenceexception-and-invalidoperationexception-appear-in-logs-review)
  - [Q3. (R) A metrics exporter builds a snapshot list for a dashboard. Under load, Gen2 collections spike. Review:](#q3-r-a-metrics-exporter-builds-a-snapshot-list-for-a-dashboard-under-load-gen2-collections-spike-review)
  - [Q4. (P) A warehouse service increments a 32-bit `int transactionId` inside a tight loop processing bulk imports. In staging (small files) IDs look fine; in production one job reports duplicate IDs and negative values after a long run. The team says "C# integers don't overflow in normal use." Explain what happened and what you would use instead.](#q4-p-a-warehouse-service-increments-a-32-bit-int-transactionid-inside-a-tight-loop-processing-bulk-imports-in-staging-small-files-ids-look-fine-in-production-one-job-reports-duplicate-ids-and-negative-values-after-a-long-run-the-team-says-c-integers-dont-overflow-in-normal-use-explain-what-happened-and-what-you-would-use-instead)
  - [Q5. (M) A developer models store configuration like the chapter's `StoreConfig` but tries to share a tax rate across all instances from appsettings loaded at startup:](#q5-m-a-developer-models-store-configuration-like-the-chapters-storeconfig-but-tries-to-share-a-tax-rate-across-all-instances-from-appsettings-loaded-at-startup)
  - [Q6. (D) Your API team debates `var` vs explicit types in service-layer code. Two snippets assign the same JSON field:](#q6-d-your-api-team-debates-var-vs-explicit-types-in-service-layer-code-two-snippets-assign-the-same-json-field)

- [03. Input & Output - Done](#03-input-output---done)

- [03. Input & Output - Done](#03-input-output---done-1)
  - [Q1. (R) A batch pricing tool prompts for quantity over stdin in a CI pipeline (`dotnet run < empty.txt`). Review this handler — what fails at runtime, and how would you fix it?](#q1-r-a-batch-pricing-tool-prompts-for-quantity-over-stdin-in-a-ci-pipeline-dotnet-run-emptytxt-review-this-handler-what-fails-at-runtime-and-how-would-you-fix-it)
  - [Q2. (R) A containerized kiosk app runs with `CultureInfo.CurrentCulture` set to `de-DE`. Operators pipe order files from a US-based ERP. Review the parser:](#q2-r-a-containerized-kiosk-app-runs-with-cultureinfocurrentculture-set-to-de-de-operators-pipe-order-files-from-a-us-based-erp-review-the-parser)
  - [Q3. (R) A developer copies the receipt-capture pattern from this chapter's `CaptureFormattedReceipt` but omits cleanup. Review:](#q3-r-a-developer-copies-the-receipt-capture-pattern-from-this-chapters-captureformattedreceipt-but-omits-cleanup-review)
  - [Q4. (P) A .NET 8 worker deployed to Kubernetes reads config lines from stdin and writes a summary CSV to stdout. Ops runs:](#q4-p-a-net-8-worker-deployed-to-kubernetes-reads-config-lines-from-stdin-and-writes-a-summary-csv-to-stdout-ops-runs)
  - [Q5. (M) An internal CLI formats currency for operators in Mumbai (`en-IN`) but must emit a fixed wire-format total for downstream JSON consumers. Review:](#q5-m-an-internal-cli-formats-currency-for-operators-in-mumbai-en-in-but-must-emit-a-fixed-wire-format-total-for-downstream-json-consumers-review)
  - [Q6. (D) A team building Express-Mart-style kiosk CLIs debates input validation strategy for numeric prompts. Two approaches:](#q6-d-a-team-building-express-mart-style-kiosk-clis-debates-input-validation-strategy-for-numeric-prompts-two-approaches)

- [04. Operators & Expressions - Done](#04-operators-expressions---done)

- [04. Operators & Expressions - Done](#04-operators-expressions---done-1)
  - [Q1. (R) QA reports that bulk-order discounts are too high on large orders. Review this pricing helper used in checkout:](#q1-r-qa-reports-that-bulk-order-discounts-are-too-high-on-large-orders-review-this-pricing-helper-used-in-checkout)
  - [Q2. (R) A warehouse API returns `404` when a SKU code is missing from the request, but the team expected a default length of `1`. Review:](#q2-r-a-warehouse-api-returns-404-when-a-sku-code-is-missing-from-the-request-but-the-team-expected-a-default-length-of-1-review)
  - [Q3. (R) After a deploy, inventory audit logs show stock decrements even when orders are rejected for insufficient credit. Review the guard:](#q3-r-after-a-deploy-inventory-audit-logs-show-stock-decrements-even-when-orders-are-rejected-for-insufficient-credit-review-the-guard)
  - [Q4. (R) A nightly batch job silently wraps negative stock counts after a bad import. Review:](#q4-r-a-nightly-batch-job-silently-wraps-negative-stock-counts-after-a-bad-import-review)
  - [Q5. (R) A code review flags this permission-update endpoint copied from an internal admin tool. Identify the compound-assignment and operator issues:](#q5-r-a-code-review-flags-this-permission-update-endpoint-copied-from-an-internal-admin-tool-identify-the-compound-assignment-and-operator-issues)
  - [Q6. (P) Your team ships pricing, inventory, and permission rules that mix arithmetic, `??`, `&&`, and `|=` in single expressions. What review checklist would you use in PRs to catch operator bugs before they reach production?](#q6-p-your-team-ships-pricing-inventory-and-permission-rules-that-mix-arithmetic-and-in-single-expressions-what-review-checklist-would-you-use-in-prs-to-catch-operator-bugs-before-they-reach-production)

- [05. Type Conversion & Casting - Done](#05-type-conversion-casting---done)

- [05. Type Conversion & Casting - Done](#05-type-conversion-casting---done-1)
  - [Q1. (R) A warehouse pricing service receives quantities from an upstream JSON deserializer boxed as `object`. Review this method — what fails at runtime, and how would you fix it?](#q1-r-a-warehouse-pricing-service-receives-quantities-from-an-upstream-json-deserializer-boxed-as-object-review-this-method-what-fails-at-runtime-and-how-would-you-fix-it)
  - [Q2. (R) An ASP.NET Core order API accepts a quantity path segment. Review the action — what breaks for bad input, and what would you change?](#q2-r-an-aspnet-core-order-api-accepts-a-quantity-path-segment-review-the-action-what-breaks-for-bad-input-and-what-would-you-change)
  - [Q3. (R) A bonus-units endpoint maps optional query text to an integer. Review both methods — which hidden behavior causes incorrect totals in production?](#q3-r-a-bonus-units-endpoint-maps-optional-query-text-to-an-integer-review-both-methods-which-hidden-behavior-causes-incorrect-totals-in-production)
  - [Q4. (R) Inventory assigns shelf slot IDs stored in a `byte` column. Review this service — what corrupts data silently, and how do you prevent it?](#q4-r-inventory-assigns-shelf-slot-ids-stored-in-a-byte-column-review-this-service-what-corrupts-data-silently-and-how-do-you-prevent-it)
  - [Q5. (R) A shipping label builder walks a heterogeneous `List<object>` of line items. Review this code — what throws or returns wrong data?](#q5-r-a-shipping-label-builder-walks-a-heterogeneous-listobject-of-line-items-review-this-code-what-throws-or-returns-wrong-data)
  - [Q6. (P) A partner integration POSTs prices as formatted strings in JSON (`"amountText": "1.234,56"`). The API runs on en-US servers. Review the handler — what fails across environments, and what contract would you enforce?](#q6-p-a-partner-integration-posts-prices-as-formatted-strings-in-json-amounttext-123456-the-api-runs-on-en-us-servers-review-the-handler-what-fails-across-environments-and-what-contract-would-you-enforce)

- [06. Control Flow & Loops - Done](#06-control-flow-loops---done)

- [06. Control Flow & Loops - Done](#06-control-flow-loops---done-1)
  - [Q1. (R) A developer ports a C-style fulfillment router into C#. The build fails with CS0163. Review the switch — what is wrong, and how would you fix it while preserving the shared "in transit" behavior?](#q1-r-a-developer-ports-a-c-style-fulfillment-router-into-c-the-build-fails-with-cs0163-review-the-switch-what-is-wrong-and-how-would-you-fix-it-while-preserving-the-shared-in-transit-behavior)
  - [Q2. (R) A nightly batch job counts warehouse slots for billing. QA reports the invoice is one slot short for every aisle. Review the nested loop:](#q2-r-a-nightly-batch-job-counts-warehouse-slots-for-billing-qa-reports-the-invoice-is-one-slot-short-for-every-aisle-review-the-nested-loop)
  - [Q3. (R) A gate-controller service hangs in staging after a config change. Review the retry loop:](#q3-r-a-gate-controller-service-hangs-in-staging-after-a-config-change-review-the-retry-loop)
  - [Q4. (R) A pick-list optimizer searches a 2D bin grid for the first high-priority SKU. It finds the SKU but keeps scanning every remaining aisle. Review:](#q4-r-a-pick-list-optimizer-searches-a-2d-bin-grid-for-the-first-high-priority-sku-it-finds-the-sku-but-keeps-scanning-every-remaining-aisle-review)
  - [Q5. (R) A pricing API uses pattern matching on order payloads. Support tickets report zero-quantity lines labeled as "positive." Review:](#q5-r-a-pricing-api-uses-pattern-matching-on-order-payloads-support-tickets-report-zero-quantity-lines-labeled-as-positive-review)
  - [Q6. (R) A barcode scan worker sums active line quantities but under-reports totals. Review:](#q6-r-a-barcode-scan-worker-sums-active-line-quantities-but-under-reports-totals-review)

- [07. Methods - Done](#07-methods---done)

- [07. Methods - Done](#07-methods---done-1)
  - [Q1. (R) A warehouse API helper is supposed to bump packed quantity in place before case-splitting. QA reports the count never changes. Review the call site and method — what is wrong, and how do you fix it?](#q1-r-a-warehouse-api-helper-is-supposed-to-bump-packed-quantity-in-place-before-case-splitting-qa-reports-the-count-never-changes-review-the-call-site-and-method-what-is-wrong-and-how-do-you-fix-it)
  - [Q2. (R) A pricing service wraps a Try-pattern helper. Under some inputs the process throws instead of returning `false`. Review the method:](#q2-r-a-pricing-service-wraps-a-try-pattern-helper-under-some-inputs-the-process-throws-instead-of-returning-false-review-the-method)
  - [Q3. (R) A developer adds a flexible shipping-fee helper and the project fails to compile. Review the signatures and one call site:](#q3-r-a-developer-adds-a-flexible-shipping-fee-helper-and-the-project-fails-to-compile-review-the-signatures-and-one-call-site)
  - [Q4. (P) Your team ships `OrderFormatting.dll` v1.0 with this public API:](#q4-p-your-team-ships-orderformattingdll-v10-with-this-public-api)
  - [Q5. (R) A catalog service computes pallet arrangements recursively. In production, large orders crash the worker. Review:](#q5-r-a-catalog-service-computes-pallet-arrangements-recursively-in-production-large-orders-crash-the-worker-review)
  - [Q6. (R) A base reporting type and a derived export type disagree at runtime. The derived XML docs say it "overrides" discount logic, but callers through a base reference see the old behavior. Review:](#q6-r-a-base-reporting-type-and-a-derived-export-type-disagree-at-runtime-the-derived-xml-docs-say-it-overrides-discount-logic-but-callers-through-a-base-reference-see-the-old-behavior-review)
  - [Q7. (R) Static analysis flags a contract mismatch between XML documentation and implementation. Review:](#q7-r-static-analysis-flags-a-contract-mismatch-between-xml-documentation-and-implementation-review)

- [08. Strings - Done](#08-strings---done)

- [08. Strings - Done](#08-strings---done-1)
  - [Q1. (R) A nightly export job builds a CSV of 50,000 warehouse scan lines. After deployment, CPU and Gen0 GC spikes correlate with the export window. Review the row builder:](#q1-r-a-nightly-export-job-builds-a-csv-of-50000-warehouse-scan-lines-after-deployment-cpu-and-gen0-gc-spikes-correlate-with-the-export-window-review-the-row-builder)
  - [Q2. (R) A developer "normalizes" incoming scan text before lookup but duplicate orders still appear in the database. Review:](#q2-r-a-developer-normalizes-incoming-scan-text-before-lookup-but-duplicate-orders-still-appear-in-the-database-review)
  - [Q3. (R) An API endpoint accepts a scan line and compares the caller's API key to a configured secret. Review:](#q3-r-an-api-endpoint-accepts-a-scan-line-and-compares-the-callers-api-key-to-a-configured-secret-review)
  - [Q4. (R) A label-printing service crashes intermittently when optional notes are omitted from the request. Review:](#q4-r-a-label-printing-service-crashes-intermittently-when-optional-notes-are-omitted-from-the-request-review)
  - [Q5. (P) Your team formats shipping labels with `$"Total: {orderTotal:C}"` and writes JSON audit logs on servers in `en-US`, `de-DE`, and `ja-JP`. Finance reports totals that do not reconcile across regions. Explain what is happening and what formatting approach you would standardize for display vs wire/storage.](#q5-p-your-team-formats-shipping-labels-with-total-ordertotalc-and-writes-json-audit-logs-on-servers-in-en-us-de-de-and-ja-jp-finance-reports-totals-that-do-not-reconcile-across-regions-explain-what-is-happening-and-what-formatting-approach-you-would-standardize-for-display-vs-wirestorage)
  - [Q6. (D) A code review proposes replacing all `StringBuilder` usage with string interpolation because "strings are simpler." The PR touches both a 3-line title builder and `LabelAssembler.BuildFullLabel`-style code that loops over hundreds of SKUs. What guidance would you give — when is `StringBuilder` worth it, and when is plain string composition enough?](#q6-d-a-code-review-proposes-replacing-all-stringbuilder-usage-with-string-interpolation-because-strings-are-simpler-the-pr-touches-both-a-3-line-title-builder-and-labelassemblerbuildfulllabel-style-code-that-loops-over-hundreds-of-skus-what-guidance-would-you-give-when-is-stringbuilder-worth-it-and-when-is-plain-string-composition-enough)

- [09. Arrays - Done](#09-arrays---done)

- [09. Arrays - Done](#09-arrays---done-1)
  - [Q1. (R) A batch job averages exam scores for reporting. QA reports intermittent `IndexOutOfRangeException` in production when a student has no scores yet. Review the helper:](#q1-r-a-batch-job-averages-exam-scores-for-reporting-qa-reports-intermittent-indexoutofrangeexception-in-production-when-a-student-has-no-scores-yet-review-the-helper)
  - [Q2. (R) A developer models a textbook shelf grid with a 2D array, then copies a jagged-array traversal pattern from another service. Review:](#q2-r-a-developer-models-a-textbook-shelf-grid-with-a-2d-array-then-copies-a-jagged-array-traversal-pattern-from-another-service-review)
  - [Q3. (R) A pricing service must keep an immutable snapshot of SKU codes before sorting for audit, but the audit log shows the "original" list reordered. Review:](#q3-r-a-pricing-service-must-keep-an-immutable-snapshot-of-sku-codes-before-sorting-for-audit-but-the-audit-log-shows-the-original-list-reordered-review)
  - [Q4. (R) A pass-rate calculator tries to normalize scores in place during iteration:](#q4-r-a-pass-rate-calculator-tries-to-normalize-scores-in-place-during-iteration)
  - [Q5. (R) After migrating parallel arrays to `List<T>`, a report builder fails to compile. Review:](#q5-r-after-migrating-parallel-arrays-to-listt-a-report-builder-fails-to-compile-review)
  - [Q6. (M) A campus API returns course offerings per department. When every department is closed for the term, the outer array exists but inner arrays are empty. Review:](#q6-m-a-campus-api-returns-course-offerings-per-department-when-every-department-is-closed-for-the-term-the-outer-array-exists-but-inner-arrays-are-empty-review)

- [10. Exception Handling - Done](#10-exception-handling---done)

- [10. Exception Handling - Done](#10-exception-handling---done-1)
  - [Q1. (R) A teammate adds logging around order validation before rethrowing. Review this method — what would you change and why?](#q1-r-a-teammate-adds-logging-around-order-validation-before-rethrowing-review-this-method-what-would-you-change-and-why)
  - [Q2. (R) A nightly reconciliation job wraps payment-gateway calls like this. Support reports "job succeeded" but ledger rows are missing after gateway timeouts. What is wrong?](#q2-r-a-nightly-reconciliation-job-wraps-payment-gateway-calls-like-this-support-reports-job-succeeded-but-ledger-rows-are-missing-after-gateway-timeouts-what-is-wrong)
  - [Q3. (R) Audit entries must always be closed, even when `WriteEntry` throws. A junior developer refactors Section 15 without `using`. Review:](#q3-r-audit-entries-must-always-be-closed-even-when-writeentry-throws-a-junior-developer-refactors-section-15-without-using-review)
  - [Q4. (D) A team introduces `InvalidOrderException`, `InsufficientFundsException`, and `CustomerNotFoundException` for every validation failure — including null method parameters and missing optional query filters. When is a custom domain exception the right choice vs `ArgumentException`, a result type, or no throw at all?](#q4-d-a-team-introduces-invalidorderexception-insufficientfundsexception-and-customernotfoundexception-for-every-validation-failure-including-null-method-parameters-and-missing-optional-query-filters-when-is-a-custom-domain-exception-the-right-choice-vs-argumentexception-a-result-type-or-no-throw-at-all)
  - [Q5. (R) Two implementations look up a product by SKU. One is in code review. Which approach would you approve for a catalog service called millions of times per day, and why?](#q5-r-two-implementations-look-up-a-product-by-sku-one-is-in-code-review-which-approach-would-you-approve-for-a-catalog-service-called-millions-of-times-per-day-and-why)
  - [Q6. (P) This console chapter lets `InvalidOrderException` bubble out of `Main` when validation fails. In an ASP.NET Core API, the same unhandled domain exception currently returns a raw 500 HTML page. What centralized pattern replaces scattered try/catch in every controller, and what must differ between Development and Production responses?](#q6-p-this-console-chapter-lets-invalidorderexception-bubble-out-of-main-when-validation-fails-in-an-aspnet-core-api-the-same-unhandled-domain-exception-currently-returns-a-raw-500-html-page-what-centralized-pattern-replaces-scattered-trycatch-in-every-controller-and-what-must-differ-between-development-and-production-responses)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

### 01. Hello World

#### Q1. What is C# and what are its key features?

What is C# and what are its key features?

**Answer:** C# is a modern, statically typed, object-oriented programming language designed for the .NET platform, where source code is compiled to Intermediate Language (IL) and executed by the Common Language Runtime (CLR). It combines C-style syntax with garbage-collected memory management, strong typing, and a rich standard library so you can build console apps, services, desktop clients, and web APIs from the same language.

- C# is multi-paradigm: you write classes and interfaces for object-oriented design, but also use delegates, lambdas, and Language Integrated Query (LINQ) for functional-style data processing.
- The language evolves with the runtime through annual releases, adding features such as nullable reference types, pattern matching, records, and top-level statements while keeping backward compatibility within a target framework.
- As a .NET language, C# interoperates with other CLI languages (F#, VB.NET) because all of them target the same assembly model defined by ECMA-335.
- Key practical features include exception handling, generics, async/await for non-blocking I/O, and compile-time safety that catches many errors before the program runs.

---

#### Q2. Explain namespaces in C#.

Explain namespaces in C#.

**Answer:** A namespace is a hierarchical naming container that groups related types and prevents name collisions when two libraries define types with the same simple name. You declare a namespace in source code, and the fully qualified name of a type combines the namespace path with the type name (for example, `System.Console`).

- Namespaces do not dictate physical folder layout or assembly boundaries by themselves; they are a logical organization tool, though teams often mirror folder structure for readability.
- The root `System` namespace and its children (`System.Collections.Generic`, `System.IO`, and others) form the Base Class Library (BCL) surface you import with `using` directives.
- Two different assemblies can expose types in the same namespace, and the compiler resolves types by namespace plus assembly reference at build time.
- Without namespaces, every type name would have to be globally unique across all referenced libraries, which would be impractical in large ecosystems.

---

#### Q3. How do nested namespaces work in C#?

How do nested namespaces work in C#?

**Answer:** Nested namespaces express a parent-child hierarchy either by nesting `namespace` blocks inside one another or by using dotted names such as `Company.Product.Feature`, which the compiler treats as nested namespace declarations.

- Dot notation is syntactic sugar: `namespace A.B.C` is equivalent to nesting `namespace A { namespace B { namespace C { ... } } }`.
- Types inside a nested namespace are referenced with the full dotted path unless a `using` directive or alias shortens the name at the top of the file.
- Nested namespaces help large teams partition domains (for example, `MyApp.Services` vs `MyApp.Models`) without creating separate assemblies for every slice.
- The nesting is purely lexical; it does not automatically grant access to `internal` members of types in parent namespaces—access modifiers still follow class and assembly rules.

---

#### Q4. What is the purpose of the `using` directive (importing namespaces)?

What is the purpose of the `using` directive (importing namespaces)?

**Answer:** The `using` directive tells the compiler to search specified namespaces when resolving unqualified type names, so you can write `Console.WriteLine` instead of the fully qualified `System.Console.WriteLine`. It affects compile-time name lookup only and does not copy code or change runtime behavior.

- A single file typically lists several `using` lines for namespaces whose types it references, keeping member access readable while the compiler still resolves to the same IL regardless of how many usings you add.
- You can also define `using` aliases (`using IO = System.IO;`) to disambiguate when two namespaces expose conflicting simple names.
- Removing a `using` does not remove the dependency on the underlying assembly; you must still reference the project or NuGet package that contains the type.
- Over-importing unused namespaces is harmless at runtime but may trigger analyzer warnings; add usings when the file actually needs types from that namespace.

---

#### Q5. Explain preprocessor directives in C# (`#if`, `#define`, `#region`, `#pragma`, etc.).

Explain preprocessor directives in C# (`#if`, `#define`, `#region`, `#pragma`, etc.).

**Answer:** Preprocessor directives are compile-time instructions processed before semantic analysis; they conditionally include or exclude code, define symbols, organize editor regions, or suppress warnings without changing runtime semantics of the code that remains.

- `#define` and `#undef` create or remove conditional compilation symbols (often combined with `#if`, `#elif`, `#else`, `#endif`) to build debug-only logging, platform-specific branches, or feature flags.
- `#region` / `#endregion` fold code blocks in the IDE for navigation; they do not affect generated IL and should not replace meaningful structure.
- `#pragma warning disable` / `restore` and `#nullable enable` / `disable` tune analyzer and nullable reference type behavior for a file or a span of code.
- `#line` can remap reported line numbers when generating source, which matters for source generators and tooling rather than everyday application code.

---

#### Q6. What is the role of the `Main` method, and how has entry-point syntax evolved (classic `Main`, top-level statements)?

What is the role of the `Main` method, and how has entry-point syntax evolved (classic `Main`, top-level statements)?

**Answer:** `Main` is the application entry point—the method the Common Language Runtime (CLR) invokes after loading the assembly to begin execution. Classic console apps declare `public static void Main(string[] args)` (or an equivalent return type/int signature), while C# 9+ allows top-level statements that the compiler synthesizes into a hidden `Main` method.

- The runtime requires exactly one entry point per executable project; duplicate `Main` methods produce compile error CS0017.
- `string[] args` receives command-line tokens split by the host; `args[0]` is the first argument after the program name (host-dependent).
- Top-level statements must appear in one file per project, before any type declarations, and are translated to a generated `Program` class with a `Main` method—ideal for scripts and small tools.
- `Main` can return `int` for process exit codes or be `async Task` / `async Task<int>` in modern templates so the entry point can await asynchronous work.

---

#### Q7. What is the difference between a project, a solution, and an assembly in a .NET workspace?

What is the difference between a project, a solution, and an assembly in a .NET workspace?

**Answer:** A solution (`.sln`) is a container that groups related projects for IDE and build orchestration; a project (`.csproj`) is the build unit that compiles source into one primary output assembly (and copies dependencies); an assembly is the compiled `.dll` or `.exe` containing IL and metadata that the CLR loads at runtime.

| Concept | Role |
|---|---|
| Solution | Organizes multiple projects, build order, and shared configuration |
| Project | Declares target framework, references, and compiles to an assembly |
| Assembly | Deployable unit of IL + metadata + manifest (name, version, references) |

- One solution commonly holds a web API project, a class library, and test projects, each producing its own assembly.
- Project references tell the compiler where to find dependent assemblies; at runtime the loader resolves them from the output folder or NuGet cache.
- The executable project's assembly contains the entry point; class library projects produce `.dll` files referenced by hosts.

---

#### Q8. What does the `global using` directive do (C# 10+), and when is it useful?

What does the `global using` directive do (C# 10+), and when is it useful?

**Answer:** A `global using` directive imports a namespace for every source file in the project (or a defined subset), eliminating repeated `using System.Collections.Generic;` lines across dozens of files. The compiler treats it as if each file included that `using` at the top.

- SDK-style projects often generate `GlobalUsings.g.cs` when `<ImplicitUsings>enable</ImplicitUsings>` is set, adding common BCL namespaces automatically for console, web, and class library templates.
- You can add a `GlobalUsings.cs` file with `global using MyCompany.Shared;` so domain types resolve everywhere without per-file imports.
- `global using` aliases work too (`global using Json = System.Text.Json;`), giving a project-wide short name.
- Use global usings for truly common imports; keep file-specific or rare namespaces local to avoid hiding where a type originates.

---

#### Q9. Explain file-scoped namespaces (`namespace X;`) vs block-scoped namespace syntax.

Explain file-scoped namespaces (`namespace X;`) vs block-scoped namespace syntax.

**Answer:** File-scoped namespace syntax (`namespace MyApp;`) declares that all types in the file belong to `MyApp` without wrapping the entire file in an extra indentation level of braces, while block-scoped syntax (`namespace MyApp { ... }`) explicitly delimits the namespace with a code block.

- Both forms produce identical IL; the choice is readability and editor ergonomics, especially in files with a single namespace.
- File-scoped namespaces require C# 10 or later and must appear before other members; only one file-scoped namespace is allowed per file.
- Block-scoped namespaces still allow multiple namespaces in one file (unusual) and remain required when nesting multiple namespace levels with different members in the same file.
- Teams migrating legacy code often keep block syntax until they standardize on file-scoped style for new files.

---

#### Q10. What is the purpose of `Program.cs` in a console application, and what other files typically accompany it (`.csproj`, `global usings`)?

What is the purpose of `Program.cs` in a console application, and what other files typically accompany it (`.csproj`, `global usings`)?

**Answer:** `Program.cs` holds the entry-point logic—either an explicit `Main` method or top-level statements—that starts the console application. It is the file developers open first to trace startup flow, though the runtime ultimately executes the compiled assembly described by the project file.

- The `.csproj` file defines the target framework (`<TargetFramework>net8.0</TargetFramework>`), output type (`Exe` vs `Library`), nullable settings, implicit usings, and package references.
- `GlobalUsings.cs` or generated global usings centralize namespace imports; some repos disable implicit usings while learning so every import is visible in source.
- Additional files include `appsettings.json` in larger apps, `AssemblyInfo` (often SDK-generated), and optional `Usings.cs` for project-wide aliases.
- In SDK-style projects, build artifacts land in `bin/` and `obj/` folders; source stays minimal with `Program.cs` plus types split into other `.cs` files as the app grows.

---

#### Q11. What is the Common Language Runtime (CLR), and how does C# code become executable?

What is the Common Language Runtime (CLR), and how does C# code become executable?

**Answer:** The Common Language Runtime (CLR)—CoreCLR in modern .NET—is the managed execution engine that loads assemblies, verifies type safety where applicable, Just-In-Time (JIT) compiles IL to native machine code, and provides garbage collection, threading, and exception services. C# source is compiled by Roslyn into IL and metadata stored in a `.dll` or `.exe`, which the host (`dotnet` CLI or a native apphost) starts by loading the CLR.

1. **Compile** — The C# compiler translates `.cs` files into IL instructions and embeds type metadata in a portable executable assembly.
2. **Launch** — The host reads the target framework from the `.runtimeconfig.json` and loads CoreCLR into the process.
3. **Load** — The loader reads the assembly manifest, resolves references from the output directory, and constructs runtime types from metadata.
4. **JIT** — On first method invocation, the JIT compiler translates IL to CPU-specific native code and caches it for later calls.
5. **Execute** — The processor runs native instructions while the CLR manages memory, exceptions, and thread scheduling.

See Module questions on IL vs JIT (Q12) for the distinction between build-time IL emission and runtime native compilation.

---

#### Q12. What is the difference between compiling to IL and JIT compilation at runtime?

What is the difference between compiling to IL and JIT compilation at runtime?

**Answer:** Compiling C# to IL happens at build time and produces portable, CPU-neutral bytecode plus metadata in an assembly, while JIT compilation happens at runtime when the CLR translates each method's IL into native machine instructions for the actual processor executing the process.

- IL compilation is done by Roslyn (or another C# compiler); the output is the same regardless of whether the app runs on x64, ARM64, or another supported architecture.
- JIT runs on first use of each method (with tiered compilation optimizing hot paths over time), so startup pays compilation cost lazily rather than ahead of time for every method.
- Alternatives such as ReadyToRun embed precompiled native images for faster startup, and Native Ahead-of-Time (AOT) compilation avoids JIT entirely for trimmed deployments.
- IL keeps assemblies compact and language-neutral; JIT enables processor-specific optimizations that would be impossible to bake in fully at build time on every target machine.

---

#### Q13. What are SDK-style projects, and what does `<TargetFramework>` in the `.csproj` control?

What are SDK-style projects, and what does `<TargetFramework>` in the `.csproj` control?

**Answer:** SDK-style projects use a concise `.csproj` that begins with `<Project Sdk="Microsoft.NET.Sdk">` and relies on MSBuild SDK defaults to glob `.cs` files automatically, replacing the verbose legacy Framework project format. The `<TargetFramework>` element (or `<TargetFrameworks>` for multi-targeting) sets the Target Framework Moniker (TFM), which selects the API surface and runtime the compiler assumes.

- `net8.0` targets modern .NET 8 with its full BCL; `net48` targets .NET Framework 4.8; `netstandard2.0` targets the portable API contract.
- The TFM controls which NuGet packages are compatible, which language features are available, and which runtime must be installed to execute the built output.
- SDK-style projects integrate NuGet restore, implicit usings, and `dotnet build` / `dotnet run` without hand-maintaining file lists.
- Changing TFM can enable or disable APIs—migrating from `net472` to `net8.0` unlocks modern libraries but may require code changes for removed APIs.

---

#### Q14. When would you use `#nullable enable` at the project or file level?

When would you use `#nullable enable` at the project or file level?

**Answer:** `#nullable enable` turns on nullable reference type (NRT) analysis so the compiler warns when you assign `null` to a non-nullable reference type or dereference a value that might be null. You apply it project-wide via `<Nullable>enable</Nullable>` in the `.csproj` or per file with `#nullable enable` at the top when migrating legacy code incrementally.

- NRT annotations (`string` vs `string?`) are compile-time contracts; the runtime still allows null references unless you enforce checks in code.
- Enable nullable when writing new code or refactoring modules where null-related bugs (`NullReferenceException`) are costly, such as public APIs and service layers.
- File-level enable lets you modernize one class at a time in a large codebase without fixing every warning in a single change.
- Pair NRT with defensive patterns (`??`, null-conditional `?.`, `ArgumentNullException.ThrowIfNull`) so warnings reflect actual runtime guarantees.

---

#### Q15. What is the difference between `Console.Out`, `Console.Error`, and writing directly with `Console.WriteLine`?

What is the difference between `Console.Out`, `Console.Error`, and writing directly with `Console.WriteLine`?

**Answer:** `Console.WriteLine` writes to standard output (`stdout`) through the `Console.Out` `TextWriter`, while error messages intended for diagnostic streams should go to `Console.Error`, which maps to `stderr`. Both are static properties you can redirect, but the convenience methods on `Console` target `Out` by default.

- Operating systems and hosts can pipe `stdout` and `stderr` separately; logging frameworks and CI systems often capture stderr for errors while treating stdout as normal program output.
- `Console.SetOut` and `Console.SetError` replace the writers—for example, redirecting output to a `StringWriter` in tests—without changing call sites that use `Console.WriteLine`.
- `Console.WriteLine` is equivalent to `Console.Out.WriteLine`; there is no separate `Console` buffer—it's a facade over the underlying writers.
- Use `Console.Error.WriteLine` for failure messages when stdout is consumed by another process (piping, automation) so errors remain visible on the error stream.

---

### 02. Data Types & Variables

#### Q1. What are the different data types in C#?

What are the different data types in C#?

**Answer:** C# data types fall into value types (stored inline with their data, such as `int`, `bool`, `char`, and `struct`) and reference types (variables hold a reference to heap objects, such as `string`, arrays, and `class` instances). The type system also includes pointer-like `unsafe` types, generic type parameters, and special types like `dynamic` and `object`.

- Built-in numeric types span signed and unsigned integers (`sbyte` through `ulong`), floating-point (`float`, `double`), high-precision decimal (`decimal`), and platform-sized `nint`/`nuint`.
- Reference types include `string`, delegates, interfaces, arrays, and user-defined classes; all reference types inherit from `System.Object`.
- Nullable value types (`int?`, `bool?`) wrap value types so they can represent an absent value with `null`.
- Enumerations (`enum`) and tuples (`(int Id, string Name)` or `ValueTuple`) model named constants and lightweight multi-value groupings respectively.

---

#### Q2. What are value types and reference types in C#?

What are value types and reference types in C#?

**Answer:** Value types store their data directly in the variable's storage location, so assigning one variable to another copies the bits of the value. Reference types store a reference (address) to an object on the managed heap, so assignment copies the reference while both variables may point to the same object.

- Structs and enum underlying types are value types; classes, interfaces (as references to implementing objects), strings, and arrays are reference types.
- Value types cannot be `null` unless wrapped in `Nullable<T>` (`int?`); reference types default to `null`.
- Value types inherit from `System.ValueType` (which inherits `object`); reference type variables always refer to heap objects with object header and method table.
- Understanding the distinction drives correct equality semantics, parameter passing behavior, and performance (heap allocation vs stack/local storage).

---

#### Q3. What is the difference between value types and reference types?

What is the difference between value types and reference types?

**Answer:** The practical difference is what gets copied on assignment and where mutable state lives: value types copy data, reference types copy pointers to shared heap objects. Method parameters and returns follow the same rules unless modified by `ref`, `out`, or `in`.

| Aspect | Value type | Reference type |
|---|---|---|
| Assignment | Copies entire value | Copies reference; both may alias same object |
| Default | Zero-bit pattern (0, false, etc.) | `null` |
| Storage | Often stack or inline in object | Object on heap; variable holds reference |
| Mutability | Mutating copy does not affect original | Mutating object affects all references |

See Q2 for definitions; see Q13 for stack vs heap nuance and Q4 for boxing when value types meet reference contexts.

---

#### Q4. What is boxing and unboxing in C#?

What is boxing and unboxing in C#?

**Answer:** Boxing converts a value type instance into a reference-type `object` (or interface) by copying the value onto the heap and wrapping it in a boxed object, while unboxing extracts the value type back from that object with an explicit cast. Both operations have allocation and type-check costs.

- Boxing occurs when you assign an `int` to `object`, call a non-generic collection's `Add(1)`, or invoke an interface method on a struct through the interface reference.
- Unboxing requires an explicit cast to the exact value type (`(int)obj`); wrong types throw `InvalidCastException`.
- Repeated boxing in hot loops (for example, storing many integers in `ArrayList`) causes garbage collection pressure—prefer generic collections like `List<int>`.
- Nullable value types box as either `null` or a boxed underlying value, not a boxed `Nullable<T>` wrapper.

---

#### Q5. Explain the `var` keyword in C#.

Explain the `var` keyword in C#.

**Answer:** `var` instructs the compiler to infer the variable's type from the initializer expression at compile time, producing the same strong typing as an explicit declaration. Once inferred, the variable remains that fixed type—you cannot later assign an incompatible type.

- `var x = 10;` is compiled as `int x = 10;`; `var` is not `dynamic` and does not defer type checking to runtime.
- You must initialize `var` variables in the declaration because the compiler has no expression from which to infer a type.
- `var` improves readability for long generic types (`Dictionary<string, List<Order>>`) but can obscure intent when the initializer is unclear.
- See Q20 for when explicit types are required (no initializer, `null` without target type, or public API signatures).

---

#### Q6. What are nullable types in C#? (including nullable reference types in C# 8+)

What are nullable types in C#? (including nullable reference types in C# 8+)

**Answer:** Nullable value types (`int?`, `bool?`, etc.) extend value types with a `HasValue` flag so they can represent missing data, while nullable reference types (`string?` vs `string`) add compile-time annotations indicating whether a reference may be null under `#nullable enable`.

- `int?` is shorthand for `Nullable<int>` and supports `null`, `.Value`, and `.GetValueOrDefault()`.
- Nullable reference types do not change runtime behavior—the CLR still allows null on any reference; the compiler emits warnings when flow analysis cannot prove safety.
- Use nullable value types for optional numeric or date fields in databases and APIs; use NRT for documenting optional strings and navigation properties.
- The null-forgiving operator (`!`) suppresses warnings when you have external guarantees the compiler cannot see.

---

#### Q7. Explain the `default` keyword and default values in C#.

Explain the `default` keyword and default values in C#.

**Answer:** The `default` keyword produces the type's zero-initialized value: numeric zero, `false` for `bool`, `\0` for `char`, and `null` for reference types and nullable types. `default(T)` in generics applies the same rule for any type parameter `T`.

- Local variables must be assigned before use; `default` is common when you need a placeholder before conditional assignment.
- Struct fields initialize to default before constructors run unless field initializers override them.
- `default` for `int?` is a null nullable with no value; for `string` it is `null`.
- See Q22 for the difference between `default(int)` and `default` on reference types—they both yield zero/`null` but generic `default(T)` unifies the pattern.

---

#### Q8. What are constants, literals, and readonly fields in C#?

What are constants, literals, and readonly fields in C#?

**Answer:** Literals are source-code representations of fixed values (`42`, `"hi"`, `3.14m`), constants (`const`) are compile-time constants embedded in metadata, and `readonly` fields are assigned at declaration or in the constructor but can differ per instance or per run.

- Integer literals default to `int`; suffixes (`L`, `M`, `F`, `D`) select `long`, `decimal`, `float`, or `double`.
- `const` values must be computable at compile time and are implicitly static.
- `readonly` instance fields can be set in the constructor, enabling per-object configuration that constants cannot express.
- String literals live in the metadata and may be interned; see Module 01 Strings chapter for immutability implications.

---

#### Q9. What is the difference between `const` and `readonly`?

What is the difference between `const` and `readonly`?

**Answer:** `const` fields must be compile-time constants and are implicitly static, while `readonly` fields can be set at runtime in instance or static constructors and may hold values computed when the program runs.

| | `const` | `readonly` |
|---|---|---|
| When set | Compile time only | Declaration or constructor |
| Static | Always implicit static | Instance or explicit `static readonly` |
| Types allowed | Numeric, `bool`, `char`, `string`, null ref | Any type, including `DateTime` from runtime |
| Per instance | No—one shared value | Instance fields differ per object |

Use `const` for true symbolic constants; use `readonly` for configuration loaded at startup (see Gotcha 3 on `DateTime.Now`).

---

#### Q10. What is an enum in C#?

What is an enum in C#?

**Answer:** An enumeration defines a named set of integral constants sharing one underlying type (default `int`), giving readable names to magic numbers such as days of the week or HTTP status categories. Enums are value types backed by their underlying integer.

- By default, the first member is 0 unless you assign explicit values; unspecified members increment by one.
- You can specify `: byte`, `: long`, or other integral underlying types to control size and interoperability.
- `[Flags]` enums combine bitwise values (`Read | Write`) and typically use powers of two for members.
- Enums convert implicitly to their underlying type and can be parsed from strings with `Enum.Parse` or `Enum.TryParse`.

---

#### Q11. What is a `struct` in C#? (basics — comparison with `class` is in OOP)

What is a `struct` in C#? (basics — comparison with `class` is in OOP)

**Answer:** A `struct` is a value type that encapsulates fields and methods inline, suitable for small, immutable or frequently copied data bundles where heap allocation should be avoided. Structs inherit from `System.ValueType` and do not support inheritance beyond interfaces.

- Structs are copied on assignment and passed by value unless you use `ref` or `in` modifiers.
- Parameterless constructors are supported from C# 10 onward with explicit field initialization rules; earlier versions relied on implicit zero initialization.
- Large structs can hurt performance when copied repeatedly; classes may be better for mutable or large state.
- Module 02 covers struct vs class trade-offs in depth (storage, identity, polymorphism).

---

#### Q12. What is a tuple in C#? (ValueTuple vs `Tuple<T>`)

What is a tuple in C#? (ValueTuple vs `Tuple<T>`)

**Answer:** Tuples group multiple values into one lightweight value without defining a named class. C# 7+ value tuples (`(int Id, string Name)`) are mutable structs with named fields, while `Tuple<T1,T2,...>` is a reference-type class from earlier APIs.

- Value tuples use `(1, "Ada")` syntax, deconstruction, and `Item1`/`Item2` or custom element names for readability.
- `Tuple.Create` allocates on the heap and is legacy; prefer value tuples for local multi-return scenarios.
- Value tuples are value types—assignment copies all elements; equality compares element values when types match.
- Use a named record or class when the grouping becomes part of a public API with behavior and invariants.

---

#### Q13. Where do value types typically live (stack vs heap), and where do reference types live?

Where do value types typically live (stack vs heap), and where do reference types live?

**Answer:** Local value type variables and parameters usually live in stack frames or CPU registers, while reference type objects always live on the managed heap with the variable holding a reference. Value types embedded in classes or boxed into `object` also reside on the heap as part of the containing object or box.

- The CLR may optimize away the stack entirely through registers and may allocate value types on the heap when they escape the method (closures, async state machines).
- Arrays of value types store elements contiguously on the heap inside the array object; the array variable itself is a reference.
- "Stack vs heap" is a teaching model; the accurate rule is value types copy by value, reference types identify heap objects by reference.
- See Q14 for boxing, which moves a value type copy onto the heap wrapped in an object header.

---

#### Q14. When a value type is boxed, where does the data end up, and why does that matter for performance?

When a value type is boxed, where does the data end up, and why does that matter for performance?

**Answer:** Boxing copies the value type's bits into a new heap object with an object header and method table pointer, so the original stack or inline value is separate from the boxed copy. Each box is a heap allocation that the garbage collector must eventually reclaim.

- Interface calls on structs often box because the dispatch requires a reference to an object implementing the interface.
- Non-generic collections (`ArrayList`, `Hashtable`) box every value type element on insertion.
- Hot-path boxing causes allocation churn, increased GC pauses, and cache misses compared to generic `List<T>` or span-based APIs.
- Mutating the struct after boxing does not change the boxed copy—another reason to avoid unintended boxing.

---

#### Q15. What is the difference between `int`, `long`, `decimal`, `float`, and `double` — when would you choose each?

What is the difference between `int`, `long`, `decimal`, `float`, and `double` — when would you choose each?

**Answer:** `int` and `long` are fixed-point integers for counting and indexing; `float` and `double` are binary floating-point types for scientific and graphics math; `decimal` is a 128-bit base-10 floating type designed for financial calculations where binary rounding errors are unacceptable.

| Type | Size | Typical use |
|---|---|---|
| `int` | 32-bit signed integer | Loop counters, IDs, general arithmetic |
| `long` | 64-bit signed integer | Large counts, timestamps in ticks |
| `float` | 32-bit binary float | GPU/math when memory bandwidth matters |
| `double` | 64-bit binary float | Default floating literal type, scientific code |
| `decimal` | 128-bit base-10 | Money, tax, accounting (suffix `m`) |

Choose `decimal` for currency; choose `double` for physics simulations; choose integers when fractional parts are impossible by domain rules.

---

#### Q16. What is the difference between signed and unsigned integer types (`int` vs `uint`, etc.)?

What is the difference between signed and unsigned integer types (`int` vs `uint`, etc.)?

**Answer:** Signed integers (`sbyte`, `short`, `int`, `long`) represent negative and positive values using two's complement, while unsigned types (`byte`, `ushort`, `uint`, `ulong`) represent zero and positive values only, doubling the positive range at the same bit width.

- `int` ranges approximately ±2.1 billion; `uint` ranges 0 to ~4.3 billion at 32 bits.
- Mixing signed and unsigned in expressions follows C# promotion rules and can surprise you when comparing across signs.
- Use unsigned types for bit masks, hash codes, and protocols that define unsigned fields—not as a trick to get "more positive int" without domain justification.
- `char` is unsigned 16-bit for UTF-16 code units, not a signed numeric type in practice.

---

#### Q17. What is `char` in C# — is it a numeric type or a text type, and how does it relate to Unicode?

What is `char` in C# — is it a numeric type or a text type, and how does it relate to Unicode?

**Answer:** `char` is a 16-bit Unicode code unit (UTF-16) value type, technically numeric under the hood but used primarily to represent a single text element or half of a surrogate pair. It is not a full Unicode grapheme or string.

- Literal `'A'` is a `char`; strings `"A"` are reference types of length one or more UTF-16 code units.
- Characters outside the Basic Multilingual Plane encode as surrogate pairs—two `char` values in one `string`.
- Arithmetic on `char` is legal (promotes to `int`) but rarely appropriate except for range checks or custom encodings.
- For full Unicode text, use `string`; for code-point-level work, consider `Rune` (.NET Core 3+) or UTF-8 APIs.

---

#### Q18. What is the difference between `bool` and nullable `bool?` in terms of default values and usage?

What is the difference between `bool` and nullable `bool?` in terms of default values and usage?

**Answer:** `bool` defaults to `false` and always holds `true` or `false`, while `bool?` defaults to `null` meaning "unknown or not specified" in addition to `true` and `false`.

- Use `bool?` for optional form fields (tri-state checkbox) or database columns that allow NULL.
- `bool?` has `.HasValue` and `.Value`; accessing `.Value` when null throws `InvalidOperationException`.
- Nullable booleans participate in lifted operators (`bool? a = true; bool? b = null; var c = a & b;` yields `null`).
- Prefer `bool` when the domain requires a definite yes/no with no missing state.

---

#### Q19. Explain the `??` (null-coalescing) and `??=` (null-coalescing assignment) operators with nullable types.

Explain the `??` (null-coalescing) and `??=` (null-coalescing assignment) operators with nullable types.

**Answer:** The `??` operator returns the left operand when it is not null; otherwise it evaluates and returns the right operand. The `??=` operator assigns the right side to the left variable only when the left is currently null.

- `name ?? "Guest"` yields `"Guest"` when `name` is null—common for defaults on nullable reference and value types.
- `cache ??= new Dictionary<string, string>();` lazily initializes `cache` once without repeating null checks.
- The right side of `??` is not evaluated unless the left is null, which matters when the fallback is expensive or has side effects.
- Chaining (`a ?? b ?? c`) walks left to right until a non-null value appears.

---

#### Q20. What is the difference between `var` and an explicit type declaration — when must you use explicit types?

What is the difference between `var` and an explicit type declaration — when must you use explicit types?

**Answer:** `var` and explicit declarations produce identical IL when the inferred type matches; the choice is readability. You must use an explicit type when there is no initializer, when the initializer is `null` without a target-typed context, or in public member signatures where `var` is not permitted.

- `var list = new List<int>();` is idiomatic; `List<int> list = new();` target-typed `new` also works from C# 9.
- `string? name = null;` must name the type because `var name = null;` does not compile.
- Interface or base-class typing often requires explicit types: `IEnumerable<int> ids = GetIds();` even if `var` could infer `List<int>`.
- See Q5 for how inference works at compile time.

---

#### Q21. What are digit separators in numeric literals (e.g., `1_000_000`), and what problem do they solve?

What are digit separators in numeric literals (e.g., `1_000_000`), and what problem do they solve?

**Answer:** Digit separators are underscore characters placed inside numeric literals to improve human readability; the compiler ignores them, so `1_000_000` equals `1000000`. They work in integer, floating, binary (`0b1010_0001`), and hex literals.

- They reduce transcription errors when reading large constants such as file sizes, bit masks, or financial limits.
- You cannot place two underscores adjacent or lead/trail an underscore in ways the grammar forbids (for example, `_100` is invalid).
- Separators do not affect runtime type or value—only source readability.
- They pair well with explicit suffixes (`1_000_000L`, `3.14_15_92d`) in configuration and test data.

---

#### Q22. What is the difference between `default(int)` and `default` for a reference type?

What is the difference between `default(int)` and `default` for a reference type?

**Answer:** Both forms yield the type's zero value: `default(int)` is `0`, and `default(string)` (or `default` for any reference type) is `null`. `default(T)` in generic code applies the same rule uniformly without knowing `T` at source-writing time.

- `default` without a type argument is inferred from context in declarations like `int x = default;`.
- For nullable value types, `default(int?)` is null with `HasValue == false`.
- There is no behavioral difference between `default(string)` and `null`—they are the same constant concept.
- Use `default` in generic libraries to initialize locals and return slots when `T` might be value or reference type.

---

#### Q23. What is a nullable reference type annotation (`string?` vs `string`), and is enforcement compile-time or runtime?

What is a nullable reference type annotation (`string?` vs `string`), and is enforcement compile-time or runtime?

**Answer:** Under nullable reference type analysis, `string` means the author intends a non-null reference and `string?` allows null, with the compiler emitting warnings when assignments and dereferences violate those intentions. Enforcement is compile-time only—the runtime does not distinguish `string` from `string?`.

- Flow analysis tracks whether a variable might be null after conditionals (`if (s is not null)`).
- Attributes like `[NotNullWhen(true)]` on `Try` methods improve analysis for patterns the compiler cannot infer alone.
- Without `#nullable enable`, annotations are ignored for warning purposes though they document intent.
- Runtime null checks still require explicit code; NRT prevents many bugs early but is not a runtime guard.

---

#### Q24. What happens when you assign `null` to a non-nullable reference type variable under `#nullable enable`?

What happens when you assign `null` to a non-nullable reference type variable under `#nullable enable`?

**Answer:** The compiler emits a warning (typically CS8625 or CS8600 depending on context) because you promised the variable should not hold null, yet you assigned null anyway. The code still compiles unless warnings are treated as errors, and the assignment succeeds at runtime like any reference assignment.

- Suppress only with documented justification (`!`, `#pragma`, or `[AllowNull]` attributes on APIs).
- Fixing the warning means changing the type to nullable, ensuring initialization in all paths, or using a non-null default (`string.Empty`).
- Treat warnings as design feedback: either the annotation or the assignment logic is wrong.
- See Q23 for the compile-time-only nature of NRT enforcement.

---

#### Q25. What is the difference between `object` as a universal base type and using `dynamic`?

What is the difference between `object` as a universal base type and using `dynamic`?

**Answer:** `object` is the root reference type for all managed types; assigning a value boxes value types and requires casts or pattern matching to use specific members after storage as `object`. `dynamic` defers member resolution to runtime via Dynamic Language Runtime (DLR) binding, skipping compile-time member checking.

- `object o = 42; int n = (int)o;` needs explicit unboxing.
- `dynamic d = GetSomething(); d.AnyMethod();` compiles even if `AnyMethod` might not exist—failures become runtime `RuntimeBinderException`.
- Use `object` for heterogeneous collections and reflection scenarios with explicit type tests.
- Use `dynamic` sparingly for COM interop and truly dynamic payloads; prefer strong typing or `JsonSerializer` elsewhere.

---

#### Q26. What are `nint` and `nuint`, and when might you encounter them?

What are `nint` and `nuint`, and when might you encounter them?

**Answer:** `nint` and `nuint` are native-sized signed and unsigned integer types whose width matches a pointer on the platform (32-bit on 32-bit processes, 64-bit on 64-bit processes). They alias `System.IntPtr` and `System.UIntPtr` for interop and low-level pointer arithmetic.

- Common in P/Invoke signatures mirroring C `intptr_t`/`size_t` and in `Span<T>` length scenarios tied to platform limits.
- Arithmetic on `nint` avoids casting pointers to `long` on 64-bit while remaining correct on 32-bit.
- They are value types but not considered "built-in" in the same sense as `int`; use when size must track pointer width.
- Unsafe code and interop layers are the typical application domains—not general business logic.

---

#### Q27. What is the difference between declaring a variable with and without an initializer?

What is the difference between declaring a variable with and without an initializer?

**Answer:** A declaration with an initializer (`int count = 0;`) assigns a value at definition time, while a declaration without an initializer (`int count;`) leaves the variable in an uninitialized state until assigned—legal for locals only after definite assignment analysis proves use before assignment is impossible.

- Fields receive default values even without initializers (`0`, `null`, `false`).
- Locals must be assigned on every code path before read; the compiler error CS0165 catches violations.
- `const` and `readonly` require initializers (const at compile time, readonly by end of constructor).
- Initializers can call methods (`var now = DateTime.UtcNow;`) whereas `const` cannot use runtime values.

---

#### Q28. Can you use `const` with user-defined types like `DateTime` or `decimal` computed at runtime? Why or why not?

Can you use `const` with user-defined types like `DateTime` or `decimal` computed at runtime? Why or why not?

**Answer:** You cannot mark such values `const` because `const` requires a compile-time constant expression, and `DateTime.Now` or runtime-computed `decimal` values are evaluated when the program runs, not when the compiler builds the assembly.

- `const decimal Tax = 0.08m;` is valid because the literal is known at compile time.
- `readonly DateTime Created = DateTime.UtcNow;` or a static readonly field set in the static constructor is the correct pattern for runtime values.
- Attempting `const DateTime d = DateTime.Now;` produces compile error CS0133.
- See Gotcha 3 and Q9 for the const vs readonly decision table.

---

### 03. Input & Output

#### Q1. What is the difference between `Console.WriteLine`, `Console.Write`, and string interpolation for output?

What is the difference between `Console.WriteLine`, `Console.Write`, and string interpolation for output?

**Answer:** `Console.WriteLine` prints text followed by the platform newline, `Console.Write` prints without advancing to the next line, and string interpolation (`$"{name}"`) builds the string in memory before passing it to either write method. Interpolation is a string composition technique, not a separate I/O channel.

- Use `WriteLine` for line-oriented user prompts and log-style output where each message should appear on its own row.
- Use `Write` when prompting on the same line as user input (`"Enter name: "` followed by `ReadLine`).
- Interpolation embeds expressions and format specifiers (`$"{price:C2}"`) and is usually clearer than concatenation with `+`.
- Both `Write` and `WriteLine` target `Console.Out` unless you redirect output.

---

#### Q2. What is the difference between `Console.ReadLine()` and `Console.ReadKey()`?

What is the difference between `Console.ReadLine()` and `Console.ReadKey()`?

**Answer:** `Console.ReadLine()` blocks until the user presses Enter and returns the entire line as a `string` (without the newline), while `Console.ReadKey()` reads a single keystroke immediately (optionally intercepting it so it is not echoed) and returns a `ConsoleKeyInfo`.

- `ReadLine` suits free-text input such as names, addresses, or pasted tokens.
- `ReadKey(true)` suits "Press any key to continue" menus without requiring Enter.
- `ReadLine` can return `null` on stream end (Ctrl+Z on Windows console); handle null before parsing.
- `ReadKey` exposes whether Shift or Alt modifiers were held via `ConsoleKeyInfo`.

---

#### Q3. How do you safely parse user input (`int.TryParse`, `Parse`, `Convert`) and handle invalid input?

How do you safely parse user input (`int.TryParse`, `Parse`, `Convert`) and handle invalid input?

**Answer:** Prefer `TryParse` for interactive input because it returns `false` on failure without throwing, letting you reprompt instead of crashing. `Parse` and `Convert` throw exceptions on invalid input, which is acceptable only when bad input represents a programmer error, not a user typo.

- Pattern: loop until `int.TryParse(Console.ReadLine(), out int n)` succeeds, showing an error message on failure.
- Supply `NumberStyles` and `IFormatProvider` when input may include currency symbols or locale-specific separators.
- `Convert.ToInt32` accepts `object` and null handling rules differ slightly from `int.Parse`.
- See Gotcha 10 on production console apps vs `Parse` on unchecked user text.

---

#### Q4. How does formatted console output work (`Console.WriteLine("{0}", value)` vs interpolation)?

How does formatted console output work (`Console.WriteLine("{0}", value)` vs interpolation)?

**Answer:** Composite formatting passes a format string with indexed placeholders `{0}`, `{1}` and a parameter list; the runtime calls `String.Format` semantics to substitute each hole. String interpolation (`$"{value}"`) is translated by the compiler into a similar `Format` call with a generated format string.

- Composite formatting supports reuse of the same index (`{0}` twice) and explicit ordering when arguments are computed expressions.
- Interpolation inlines expressions directly, which is easier to read for simple messages.
- Both support alignment and numeric formats: `{0,10:N2}` or `$"{value,10:N2}"`.
- Custom types can participate via `IFormattable.ToString(format, provider)`.

---

#### Q5. What is the difference between `CultureInfo.CurrentCulture`, `CurrentUICulture`, and `InvariantCulture`?

What is the difference between `CultureInfo.CurrentCulture`, `CurrentUICulture`, and `InvariantCulture`?

**Answer:** `CurrentCulture` drives formatting and parsing of numbers, dates, and currency for user-facing display. `CurrentUICulture` selects localized resource strings (satellite assemblies). `InvariantCulture` is a fixed, culture-neutral English-like format used when data must round-trip regardless of user locale.

| Property | Purpose |
|---|---|
| `CurrentCulture` | Number/date/currency format and parse for display |
| `CurrentUICulture` | Localized UI strings from `.resx` files |
| `InvariantCulture` | Stable format for logs, protocols, and file formats |

Changing `CurrentCulture` affects `ToString()` on numbers and dates when no explicit provider is passed.

---

#### Q6. When should you use `InvariantCulture` for formatting numbers and dates instead of `CurrentCulture`?

When should you use `InvariantCulture` for formatting numbers and dates instead of `CurrentCulture`?

**Answer:** Use `InvariantCulture` whenever the string is stored, transmitted, or parsed later in a different locale—logs, JSON, CSV, query strings, and file names—because `CurrentCulture` varies by machine and user settings.

- User-facing labels in a GUI may use `CurrentCulture` for friendly dates, but API payloads should not.
- Explicit `culture` parameters override the thread default and document intent at the call site.
- Mixing cultures between write and read causes subtle bugs (`"1,234.56"` vs `"1.234,56"`).
- See Gotcha 9 for parse failures when comma is treated as thousands separator vs decimal point.

---

#### Q7. How do culture settings affect decimal separators, currency symbols, and date formats in console output?

How do culture settings affect decimal separators, currency symbols, and date formats in console output?

**Answer:** Formatting APIs consult the culture's `NumberFormatInfo` and `DateTimeFormatInfo`, so the same `double` or `DateTime` prints differently under `en-US`, `de-DE`, or `fr-FR`. Currency format adds symbol placement and group separators defined by that culture.

- `3.14.ToString()` in `de-DE` may display `3,14` while `en-US` shows `3.14`.
- Short dates might be `MM/dd/yyyy` vs `dd.MM.yyyy`, affecting user confusion in console apps.
- `Console` output does not auto-adapt unless the thread culture is set or you pass a format provider.
- Testing console apps should set culture explicitly or use invariant formatting for deterministic assertions.

---

#### Q8. What is composite formatting (`string.Format`, `{0:N2}`, alignment `{0,10}`, `{0,-10}`)?

What is composite formatting (`string.Format`, `{0:N2}`, alignment `{0,10}`, `{0,-10}`)?

**Answer:** Composite formatting replaces indexed placeholders in a template string with formatted argument values, optionally specifying width, alignment, and numeric or date format strings after a colon.

- `{0:N2}` formats argument zero as a number with two decimal places and group separators per the provider.
- `{0,10}` right-aligns in width 10; `{0,-10}` left-aligns with padding spaces by default.
- `string.Format(provider, format, args)` and `Console.WriteLine(format, args)` share the same rules.
- Custom formats (`"P"`, `"C"`, `"yyyy-MM-dd"`) map to standard or type-specific format strings documented for each type.

---

#### Q9. What is the difference between `Console.InputEncoding` and `Console.OutputEncoding`, and why can mismatched encodings garble console text?

What is the difference between `Console.InputEncoding` and `Console.OutputEncoding`, and why can mismatched encodings garble console text?

**Answer:** `Console.InputEncoding` and `Console.OutputEncoding` control how bytes from the console device map to .NET characters on read and write respectively. If the console code page or terminal encoding does not match the Unicode text you emit, characters outside that repertoire display as `?` or mojibake.

- Windows consoles historically defaulted to OEM or ANSI code pages; UTF-8 output requires matching console and `OutputEncoding`.
- Reading UTF-8 bytes with a Latin-1 interpretation corrupts multi-byte sequences.
- Modern .NET templates often set UTF-8 in project or host configuration; legacy environments may still need explicit setup.
- See Gotcha 15 on Unicode replacement characters when encodings disagree.

---

#### Q10. How do you capture console output programmatically (e.g., `StringWriter` redirected to `Console.SetOut`)?

How do you capture console output programmatically (e.g., `StringWriter` redirected to `Console.SetOut`)?

**Answer:** Replace `Console.Out` with a `TextWriter` such as `StringWriter` via `Console.SetOut`, run code under test that writes to `Console`, then read the accumulated text from the writer. Restore the original writer in a `finally` block so later tests are not affected.

- Save `var original = Console.Out;` before redirecting and call `Console.SetOut(original)` in `finally`.
- `StringWriter.ToString()` returns all captured output after the exercise under test completes.
- The same pattern works for `Console.SetError` with a separate `StringWriter` for stderr.
- Integration tests use this to assert CLI tools print expected messages without spawning a visible console.

---

#### Q11. What is the difference between `Console.Read` and `Console.ReadLine`?

What is the difference between `Console.Read` and `Console.ReadLine`?

**Answer:** `Console.Read()` returns the next character as an `int` code unit (or `-1` at end of stream), blocking until one character is available, while `ReadLine()` reads until Enter and returns a full `string` including all characters typed on that line.

- `Read` is low-level and rarely used in application code except single-key scenarios without `ReadKey` features.
- `ReadLine` is the standard choice for textual user input in tutorials and simple CLIs.
- Neither trims whitespace unless you call `Trim()` on the returned string.
- `Read` does not echo behavior differences beyond platform defaults; `ReadKey` offers more control over echo and intercept.

---

#### Q12. How do you format output with alignment and padding using interpolation (`$"{value,10}"`, `$"{value:N2}"`)?

How do you format output with alignment and padding using interpolation (`$"{value,10}"`, `$"{value:N2}"`)?

**Answer:** Interpolation supports the same alignment and format syntax as composite formatting inside the braces: a comma and width for padding, a colon and format string for numeric or date patterns.

- `$"{name,-20}"` left-aligns `name` in 20 columns, useful for columnar console tables.
- `$"{amount:N2}"` prints two decimal places with culture-aware group separators unless you pass `CultureInfo.InvariantCulture`.
- Multiple holes can mix formats: `$"{id,5} {desc,-30} {price,10:C}"`.
- Constant alignment values compile efficiently; complex expressions inside `{...}` are evaluated before formatting.

---

#### Q13. What is `IFormattable`, and how does it relate to custom formatting in `ToString(format, provider)`?

What is `IFormattable`, and how does it relate to custom formatting in `ToString(format, provider)`?

**Answer:** `IFormattable` declares `ToString(string? format, IFormatProvider? formatProvider)`, allowing types to interpret custom and standard format strings when used in composite formatting or interpolation. The runtime calls this interface when formatting an object with a non-null format specifier.

- Implement both `ToString()` and `ToString(format, provider)` consistently for public types.
- If a type lacks `IFormattable`, unknown format strings may be ignored in `ToString(format)` overrides on `object`.
- `FormattableString` and `FormattableStringFactory` expose interpolation holes for culture-invariant logging scenarios.
- Currency and numeric BCL types implement rich format support (`"C"`, `"X"`, `"E"`).

---

#### Q14. What happens if you call `int.Parse` on invalid input vs `int.TryParse` — which pattern is preferred in production console apps?

What happens if you call `int.Parse` on invalid input vs `int.TryParse` — which pattern is preferred in production console apps?

**Answer:** `int.Parse` throws `FormatException` (or `OverflowException`) when the text is not a valid integer, terminating the flow unless caught. `int.TryParse` returns `false` and sets the out parameter to default, which fits expected user input mistakes in interactive loops.

- Production console apps should treat bad input as a normal branch, not an exceptional one—see Gotcha 10.
- `Parse` remains fine for trusted configuration files validated at startup with clear error handling.
- Always consider culture: parsing `"1.234"` depends on whether `.` is decimal or thousands separator.
- Wrap parse failures with user-visible messages and reprompt rather than stack traces.

---

#### Q15. How does changing `CultureInfo.CurrentCulture` on the current thread affect subsequent formatting calls that omit an explicit provider?

How does changing `CultureInfo.CurrentCulture` on the current thread affect subsequent formatting calls that omit an explicit provider?

**Answer:** Formatting methods without an explicit `IFormatProvider` use `CultureInfo.CurrentCulture` on the executing thread, so changing it mid-process changes how numbers, dates, and currency render from that point forward on that thread.

- ASP.NET and modern hosts often set culture per request from headers; console apps inherit OS user settings by default.
- Async continuations may flow culture depending on configuration; explicit providers avoid surprises.
- Libraries should not mutate global culture silently; accept `IFormatProvider` parameters instead.
- Tests set `CultureInfo.CurrentCulture` to invariant or a fixed culture in setup for deterministic output.

---

#### Q16. What is `NumberFormatInfo`, and how does it differ from `CultureInfo`?

What is `NumberFormatInfo`, and how does it differ from `CultureInfo`?

**Answer:** `NumberFormatInfo` holds the specific rules for decimal separators, group sizes, negative patterns, and percent symbols, while `CultureInfo` is the broader culture object that exposes `NumberFormat`, `DateTimeFormat`, and other regional settings together.

- Access via `CultureInfo.CurrentCulture.NumberFormat` or `CultureInfo.InvariantCulture.NumberFormat`.
- Clone and modify `NumberFormatInfo` for custom numeric displays without creating a full custom culture.
- `CultureInfo` is the usual entry point; `NumberFormatInfo` is the detailed knob for number layout only.
- Parsing methods accept `NumberStyles` combined with a format provider derived from culture.

---

#### Q17. When reading numeric input from users in different locales, what pitfalls arise with comma vs period decimal separators?

When reading numeric input from users in different locales, what pitfalls arise with comma vs period decimal separators?

**Answer:** Users type decimals according to local convention, but `TryParse` without a culture may interpret commas as thousands separators or decimal points inconsistently, producing wrong values or parse failures when the thread culture does not match the user's expectation.

- `"3,14"` is 3.14 in `de-DE` but might fail or misparse under `en-US` rules.
- Always document expected format in prompts or parse with an explicit culture matching the UI language.
- For machine-readable input, instruct users to use invariant format or accept both with custom parsing logic.
- See Gotcha 9 and Q6 on invariant culture for stored data vs localized input.

---

#### Q18. What is the purpose of `Console.ForegroundColor`, `BackgroundColor`, and resetting colors after use?

What is the purpose of `Console.ForegroundColor`, `BackgroundColor`, and resetting colors after use?

**Answer:** These properties change the console's text and background colors for subsequent output, highlighting errors, success, or sections in CLI tools. Resetting to `Console.ResetColor()` afterward prevents later unrelated output from inheriting unintended colors.

- Color support depends on terminal capabilities; Windows Terminal and modern consoles support richer palettes than legacy hosts.
- Wrap color changes in `try/finally` to restore defaults even when exceptions occur.
- Overusing color reduces accessibility; pair with explicit labels, not color alone, for errors.
- Libraries logging to shared consoles should avoid setting colors unless configured, to respect host themes.

---

### 04. Operators & Expressions

#### Q1. What are the different types of operators in C#? (Arithmetic, Relational, Logical, Bitwise, Assignment, Ternary, Null-coalescing, etc.)

What are the different types of operators in C#? (Arithmetic, Relational, Logical, Bitwise, Assignment, Ternary, Null-coalescing, etc.)

**Answer:** C# groups operators by purpose: arithmetic (`+`, `-`, `*`, `/`, `%`), relational (`==`, `!=`, `<`, `>`), logical (`&&`, `||`, `!`), bitwise (`&`, `|`, `^`, `~`, shifts), assignment (`=`, `+=`, compound forms), conditional (`?:`), null operators (`?.`, `??`, `??=`), and others such as `is`, `as`, `sizeof`, and `nameof`.

- Operator precedence determines evaluation order when parentheses are omitted; unary before multiplicative before additive before relational before logical AND before OR.
- Overloaded operators apply to user-defined types when declared with `public static` signatures matching language rules.
- Some operators behave differently on floating-point vs integer types (division, remainder).
- Null-conditional and null-coalescing operators integrate with nullable reference and value type analysis.

---

#### Q2. Explain the `checked` and `unchecked` keywords in C#.

Explain the `checked` and `unchecked` keywords in C#.

**Answer:** `checked` context causes arithmetic overflow on integral types to throw `OverflowException`, while `unchecked` (the default in most projects) silently wraps overflow using two's complement truncation. You can scope contexts with `checked { ... }` blocks or enable project-wide checked arithmetic.

- Financial and checksum code often enables `checked` for early failure instead of silent wraparound.
- `unchecked` is explicit when you rely on wrap semantics (hash mixing, low-level algorithms).
- Overflow does not apply to floating-point types—they produce infinity or NaN instead.
- See Gotcha 14 on default unchecked integer behavior.

---

#### Q3. What is the difference between `==` and `.Equals()` for value types vs reference types?

What is the difference between `==` and `.Equals()` for value types vs reference types?

**Answer:** For value types, `==` compares values when overloaded or compares bitwise equality for primitives; `.Equals` typically matches value semantics. For reference types, `==` may use reference equality unless overloaded (as `string` does), while `.Equals` may be overridden for logical equality.

- Default reference equality: `==` and `ReferenceEquals` align unless the type overloads `==`.
- Structs should implement `IEquatable<T>` and consistent `GetHashCode` when used in collections.
- `Equals(object?)` accepts null; `==` between reference types is false if either side is null (unless both null).
- See Q14 for `ReferenceEquals` specifically.

---

#### Q4. What is integer division in C#, and how do you get a fractional result?

What is integer division in C#, and how do you get a fractional result?

**Answer:** When both operands of `/` are integral types, C# performs integer division, truncating toward zero and discarding any fractional part. At least one operand must be floating-point (or cast) to obtain a fractional result.

- `10 / 3` yields `3`, not `3.333…`—see Gotcha 2.
- `10 / 3.0` or `10.0 / 3` promotes to `double` and yields approximately `3.333…`.
- `%` returns the remainder with the sign of the dividend: `-10 % 3` is `-1`.
- Use `decimal` division for money after promoting operands to `decimal`.

---

#### Q5. Explain operator precedence and associativity — why does `a + b * c` evaluate differently than `(a + b) * c`?

Explain operator precedence and associativity — why does `a + b * c` evaluate differently than `(a + b) * c`?

**Answer:** Operator precedence ranks multiplication above addition, so `a + b * c` computes `b * c` first then adds `a`. Associativity rules break ties at the same precedence level—most binary operators associate left-to-right, assignment and null-coalescing associate right-to-left.

- Parentheses override precedence explicitly and improve readability even when not strictly required.
- Misunderstanding precedence causes subtle bugs in compound conditions and arithmetic without parentheses.
- The language specification defines a complete table; IDE tooling parenthesizes subexpressions in tooltips when debugging.
- Ternary `?:` has lower precedence than most operators, which can surprise without parentheses around the condition.

---

#### Q6. What is the difference between prefix and postfix increment (`++i` vs `i++`)?

What is the difference between prefix and postfix increment (`++i` vs `i++`)?

**Answer:** Prefix increment (`++i`) adds one and returns the new value; postfix increment (`i++`) returns the original value then adds one. Both mutate the variable when applied to a mutable l-value.

- In standalone statements (`i++;` as its own line), the difference is invisible.
- In expressions like `array[i++]`, postfix uses the old index then advances; prefix advances first.
- Increment on value type properties that return copies does not compile unless the property returns by ref (rare).
- Overflow follows integral checked/unchecked context like other arithmetic.

---

#### Q7. What are short-circuit logical operators (`&&`, `||`), and why do they matter beyond boolean logic?

What are short-circuit logical operators (`&&`, `||`), and why do they matter beyond boolean logic?

**Answer:** `&&` and `||` evaluate the right operand only when necessary—`&&` skips the right side if the left is false, `||` skips if the left is true. This enables safe null checks and avoids side effects or expensive calls when the outcome is already determined.

- `if (obj != null && obj.Count > 0)` avoids `NullReferenceException` because `Count` is not evaluated when `obj` is null.
- Non-short-circuit `&` and `|` on `bool` always evaluate both sides—rarely needed except for bitwise booleans.
- Short-circuit behavior interacts with nullable booleans in lifted operators differently from strict evaluation.
- Side effects in the right operand must be understood as conditionally executed.

---

#### Q8. Explain the null-conditional operator (`?.`) and null-coalescing operators (`??`, `??=`).

Explain the null-conditional operator (`?.`) and null-coalescing operators (`??`, `??=`).

**Answer:** The null-conditional operator `?.` accesses a member or indexer only when the receiver is not null, producing null for reference results or `Nullable<T>` for value results instead of throwing. `??` and `??=` supply defaults when an expression is null.

- `customer?.Address?.City` short-circuits at the first null in the chain.
- `?.` combined with invocation: `handler?.Invoke()` calls only if delegate is not null.
- `??=` lazily initializes fields and locals—see Q19 in Data Types.
- Null-conditional assignment extensions (`?.=`) appear in newer language versions for compound null-aware assignment.

---

#### Q9. What are bitwise operators (`&`, `|`, `^`, `~`, `<<`, `>>`), and when are they used in application code?

What are bitwise operators (`&`, `|`, `^`, `~`, `<<`, `>>`), and when are they used in application code?

**Answer:** Bitwise operators manipulate individual bits of integral types: AND masks bits, OR sets bits, XOR toggles, NOT inverts, and shifts move bit patterns left or right. They appear in flags enums, permissions masks, hashing, compression, and low-level protocols.

- `[Flags]` enums combine values with `|` and test with `&`: `(mode & FileMode.Read) != 0`.
- Unsigned right shift `>>>` (C# 11+) fills with zeros regardless of sign for `int`/`uint`.
- Do not confuse bitwise `&` on integers with logical `&&` on booleans—they are different operators.
- Application business logic rarely needs bitwise ops unless modeling packed flags or interfacing with hardware formats.

---

#### Q10. What is the difference between logical AND (`&&`) and bitwise AND (`&`) when applied to `bool` operands?

What is the difference between logical AND (`&&`) and bitwise AND (`&`) when applied to `bool` operands?

**Answer:** Both combine boolean values, but `&&` short-circuits and `&` always evaluates both operands. For pure boolean logic with possible null checks or side effects on the right, `&&` is the default choice.

- `true & Foo()` always calls `Foo()`; `true && Foo()` still calls `Foo()` because the left is true—only false left skips right.
- Bitwise `&` on integers is unrelated to boolean `&&` despite similar symbols.
- Nullable bools use lifted operators with `&` and `|` producing null when either operand is null in some cases.
- Use `&` on bools only when both evaluations are required for correctness.

---

#### Q11. What is the ternary conditional operator (`?:`), and how does it differ from an `if/else` statement?

What is the ternary conditional operator (`?:`), and how does it differ from an `if/else` statement?

**Answer:** The ternary operator `condition ? whenTrue : whenFalse` is an expression that yields a value, while `if/else` is a statement controlling blocks of arbitrary size. Ternary fits simple choice-of-value scenarios; statements fit multi-line logic and void actions.

- Both branches must be type-compatible enough for the compiler to infer a common type.
- Nested ternaries reduce readability; prefer `if/else` or switch expressions for many branches.
- Ternary is evaluated eagerly for the chosen branch only—like `if/else`, not lazy like some functional forms.
- See Control Flow Q1 for stylistic guidance vs `if/else`.

---

#### Q12. What is the difference between `is` pattern matching and a simple boolean expression in a condition?

What is the difference between `is` pattern matching and a simple boolean expression in a condition?

**Answer:** The `is` operator tests runtime type and can introduce a typed variable in the same expression (`if (obj is Order o)`), combining type check, cast, and assignment. A simple boolean might call a method but does not bind a new strongly typed variable without a separate cast.

- Pattern forms include constant, relational, property, and recursive patterns in modern C#.
- `is not null` integrates with nullable flow analysis better than `!= null` in some analyzer versions.
- Type patterns fail closed when the object is null unless you use `is null` or null checks first.
- See Control Flow Q14 for `if` vs `switch` pattern usage.

---

#### Q13. When does overflow occur for integer arithmetic, and how do `checked` blocks change behavior?

When does overflow occur for integer arithmetic, and how do `checked` blocks change behavior?

**Answer:** Overflow occurs when an integral operation's mathematical result lies outside the representable range of the type, such as `int.MaxValue + 1`. In unchecked context the value wraps; in checked context the runtime throws `OverflowException` at the overflowing operation.

- Literals and constant folding may detect overflow at compile time in checked context.
- Casting a too-large constant to a smaller type can overflow at compile time with error CS0221 in checked context.
- See Q2 and Gotcha 14 for project-level checked settings.
- `decimal` throws `OverflowException` on overflow rather than wrapping silently.

---

#### Q14. What is the difference between `==` and `ReferenceEquals` for reference types?

What is the difference between `==` and `ReferenceEquals` for reference types?

**Answer:** `ReferenceEquals(a, b)` always compares object identity—whether both refer to the exact same heap instance—ignoring any `==` overload on the type. `==` may delegate to an overloaded operator (strings compare by value) or default reference equality.

- Use `ReferenceEquals` when you intentionally need identity semantics despite value-based `==` overloads.
- Two distinct string objects with the same content may be `==` true but `ReferenceEquals` false unless interned.
- Value types boxed to object compare references when using `ReferenceEquals` on the boxed instances.
- See Strings chapter on interning and equality.

---

#### Q15. Can you overload operators in C# — which operators can and cannot be overloaded?

Can you overload operators in C# — which operators can and cannot be overloaded?

**Answer:** C# allows overloading many unary and binary operators (`+`, `-`, `==`, `!=`, implicit/explicit conversions, etc.) as static methods on the declaring type, but cannot overload operator precedence, the ternary operator, `&&`, `||`, or assignment operators like `=` and `+=` directly.

- Comparison operators `==` and `!=` must be overloaded in pairs; `true`/`false` unary operators support custom boolean logic types.
- Conversion operators must be `implicit` or `explicit` and follow clear semantics to avoid surprising casts.
- Overloads should mirror intuitive meaning; abusing `+` for unrelated operations harms readability.
- `Equals`, `GetHashCode`, and `==` should stay consistent for types used as keys.

---

#### Q16. What is the difference between compound assignment (`+=`, `-=`) and the expanded form (`x = x + y`) for value vs reference types?

What is the difference between compound assignment (`+=`, `-=`) and the expanded form (`x = x + y`) for value vs reference types?

**Answer:** For value types, `x += y` mutates `x` in place when `x` is a mutable variable. For reference types, `+=` on references reassigns the variable when the operator returns a new reference (strings), while mutating methods on objects (`list.Add`) change the object without reassigning the variable.

- `string s = s + "a"` and `s += "a"` both create new string instances because strings are immutable.
- `list += item` is not valid unless overloaded; use `list.Add` for mutation.
- Compound assignment evaluates the left side once, which matters if the left is a property or indexer with side effects.
- See Methods chapter on ref parameters when reassignment must affect the caller's variable.

---

#### Q17. What is the `nameof` operator, and how is it used in validation messages and refactoring-safe code?

What is the `nameof` operator, and how is it used in validation messages and refactoring-safe code?

**Answer:** `nameof` returns the unqualified identifier name of a variable, type, or member as a compile-time string without runtime reflection cost. Renaming the symbol updates `nameof` automatically, making it ideal for argument exception parameters and property change notifications.

- `throw new ArgumentNullException(nameof(customer));` stays correct if the parameter is renamed.
- `nameof` does not include namespace or full dotted paths—only the final token (`nameof(Foo.Bar)` is `"Bar"`).
- It avoids magic strings that drift from code during refactorings.
- Unlike string literals, `nameof` is resolved at compile time and has zero runtime overhead.

---

### 05. Type Conversion & Casting

#### Q1. What is the difference between the `is` and `as` operators?

What is the difference between the `is` and `as` operators?

**Answer:** `is` tests whether an object is compatible with a type and, with pattern matching, binds a variable when the test succeeds. `as` attempts a cast and returns `null` on failure for reference types without throwing.

- `if (obj is Customer c)` combines check and assignment in one step.
- `var c = obj as Customer; if (c != null)` is the legacy pattern before pattern matching.
- `as` does not work on non-nullable value types directly; use `is` with patterns or explicit casts for value types.
- Failed `as` returns null, which can hide bugs if you forget the null check.

---

#### Q2. What is the difference between implicit and explicit type conversion (casting)?

What is the difference between implicit and explicit type conversion (casting)?

**Answer:** Implicit conversions compile without a cast when the conversion is guaranteed safe (widening numerics, derived to base reference). Explicit conversions require a cast operator when data might be lost or the relationship is not automatically safe (narrowing numerics, base to derived references).

- Widening (`int` to `long`) is implicit; narrowing (`long` to `int`) needs `(int)value` and may overflow at runtime.
- User-defined types can declare `implicit` or `explicit` conversion operators with clear semantics.
- Explicit reference casts throw `InvalidCastException` when the object is not actually of the target type.
- See Q5 on widening vs narrowing direction.

---

#### Q3. What is the difference between `Convert.ToInt32`, `(int)`, and `int.Parse`?

What is the difference between `Convert.ToInt32`, `(int)`, and `int.Parse`?

**Answer:** `(int)` is a direct cast requiring a compatible numeric type at compile time or an unboxing cast at runtime. `int.Parse` parses a string representation. `Convert.ToInt32` accepts broader inputs (`object`, strings, other numerics) with generalized conversion rules and uses `IConvertible`.

- `Parse` and `Convert` on strings throw on invalid text; prefer `TryParse` for user input.
- `Convert.ToInt32(3.9)` rounds to nearest integer (banker's rounding rules apply); `(int)3.9` truncates toward zero.
- `Convert` handles null for nullable types differently than `Parse`.
- Choose the narrowest API: cast for numeric narrowing, `TryParse` for strings, `Convert` for heterogeneous legacy APIs.

---

#### Q4. When does a cast succeed at compile time but fail at runtime?

When does a cast succeed at compile time but fail at runtime?

**Answer:** Reference downcasts compile when the static type is a base type but fail at runtime if the object is not actually an instance of the target derived type. Unboxing casts compile when the static type is `object` but fail if the boxed type does not match exactly.

- `(Derived)baseRef` compiles if `baseRef` is typed as `Base` but throws if it points to another derived type.
- `(int)obj` throws `InvalidCastException` if `obj` boxes a `double`, not an `int`.
- Pattern matching with `is` avoids exceptions by testing first.
- Array covariance also compiles unsafe assignments that fail at runtime—see Gotcha 8.

---

#### Q5. What is widening vs narrowing conversion — which direction is implicit?

What is widening vs narrowing conversion — which direction is implicit?

**Answer:** Widening conversions move to a type that can represent all values of the source without loss (for example `byte` to `int`), and C# allows them implicitly. Narrowing conversions move to a smaller range or lower precision and require explicit casts because values may be truncated or rounded.

- Floating to `decimal` is explicit because not all binary floats map exactly.
- `int` to `long` is implicit widening; `long` to `int` is explicit narrowing.
- Constant expressions may be implicitly converted when the value fits the target type even if narrowing in general.
- See Q3 for truncation vs rounding on numeric conversions.

---

#### Q6. What is the difference between `Parse`, `TryParse`, and `Convert.ChangeType`?

What is the difference between `Parse`, `TryParse`, and `Convert.ChangeType`?

**Answer:** `Parse` converts strings to target types and throws on failure. `TryParse` returns a boolean and an out result without throwing for expected bad input. `Convert.ChangeType` generalizes conversion between many types via `IConvertible`, often used in reflection-based scenarios.

- `TryParse` is preferred for interactive and protocol parsing where failure is normal.
- `ChangeType` returns `object` requiring a cast and throws `InvalidCastException` when no conversion exists.
- All string parsing should specify culture when format is locale-dependent.
- Nullable value types need `TryParse` overloads or parse the underlying type then assign.

---

#### Q7. When would you use the `is` pattern with a declaration (`if (obj is int n)`) vs a traditional cast?

When would you use the `is` pattern with a declaration (`if (obj is int n)`) vs a traditional cast?

**Answer:** Use the declaration pattern when you need both a type test and a typed variable in the true branch without a separate cast that the compiler cannot prove safe. Traditional casts `(int)obj` are acceptable when you already verified type or will catch exceptions.

- Declaration patterns integrate with nullable analysis and switch expressions cleanly.
- Repeated `(Target)obj` casts duplicate noise and skip flow analysis benefits.
- Traditional casts fail fast with exceptions—sometimes desired in trusted internal code paths.
- Switch expressions on type patterns replace long if-else chains—see Control Flow chapter.

---

#### Q8. What exception types are commonly thrown by failed casts and parses (`FormatException`, `OverflowException`, `InvalidCastException`)?

What exception types are commonly thrown by failed casts and parses (`FormatException`, `OverflowException`, `InvalidCastException`)?

**Answer:** `FormatException` indicates the string is not in the expected format for parse methods. `OverflowException` occurs when a numeric parse or checked arithmetic exceeds the target range. `InvalidCastException` signals an incompatible cast or unboxing operation at runtime.

- `int.Parse("abc")` throws `FormatException`; `int.Parse("999999999999999999999")` may throw `OverflowException`.
- Unboxing wrong types throws `InvalidCastException`.
- Catch specific types when recovering; let unexpected failures propagate in most application layers.
- `TryParse` avoids all three for expected failure paths on strings.

---

#### Q9. What is `TryFormat`, and how does writing into a `Span<char>` differ from calling `ToString()`?

What is `TryFormat`, and how does writing into a `Span<char>` differ from calling `ToString()`?

**Answer:** `TryFormat` attempts to write a formatted representation into a caller-provided `Span<char>` buffer, returning whether the buffer was large enough. This avoids allocating a new `string` on the heap, which `ToString()` always does.

- Used internally by high-performance formatting and available on many primitive types.
- If the span is too small, `TryFormat` returns false and you can retry with a larger buffer.
- Span-based formatting fits stackalloc buffers and UTF-8 encoding pipelines in modern APIs.
- For simple logging, `ToString()` readability often outweighs micro-optimization unless profiling shows hot paths.

---

#### Q10. How does culture affect parsing and formatting during type conversion (e.g., `"1,234.56"` vs `"1.234,56"`)?

How does culture affect parsing and formatting during type conversion (e.g., `"1,234.56"` vs `"1.234,56"`)?

**Answer:** Parse and format methods use the supplied `IFormatProvider` or `CurrentCulture` to decide whether comma or period is the decimal separator and how groups are laid out, so the same literal characters mean different numbers in different cultures.

- Always pass `CultureInfo.InvariantCulture` for wire formats and logs.
- User input should parse with the culture matching how you prompted the user to type data.
- Mis-specified culture causes off-by orders-of-magnitude bugs in financial imports.
- See Input & Output Q5–Q7 and Gotcha 9.

---

#### Q11. What is the difference between boxing during conversion to `object` and a direct numeric cast?

What is the difference between boxing during conversion to `object` and a direct numeric cast?

**Answer:** Assigning a value type to `object` boxes it—allocating a heap wrapper—while casting between numeric types (`(int)d`) converts the value directly without creating an object wrapper when both sides are known numeric types.

- `(object)42` boxes; `(int)42.0` truncates a double without boxing.
- Unboxing from `object` back to a value type requires an exact type match.
- Generic collections avoid boxing for value types; `ArrayList` boxed every `int`.
- See Data Types Q4 and Q14 on performance impact.

---

#### Q12. When is the `as` operator preferred over a cast, and what does it return on failure?

When is the `as` operator preferred over a cast, and what does it return on failure?

**Answer:** Prefer `as` when casting down a reference hierarchy where failure is an expected outcome and you will branch on null, avoiding exception cost. On failure, `as` returns `null` for reference types instead of throwing `InvalidCastException`.

- Do not use `as` with value types except nullable scenarios; use `is` patterns instead in modern code.
- After `as`, always null-check before dereferencing.
- When failure should abort processing, an explicit cast or pattern match documents intent more clearly.
- Legacy codebases mix `as` with null checks; new code favors `is` patterns.

---

#### Q13. What is user-defined explicit/implicit conversion operator syntax (preview level)?

What is user-defined explicit/implicit conversion operator syntax (preview level)?

**Answer:** Types can declare `public static implicit operator TargetType(SourceType s)` or `explicit operator TargetType(SourceType s)` to allow the compiler to convert between types with defined semantics, subject to language rules requiring paired safety documentation in API design.

- Implicit conversions should be obviously safe; explicit conversions signal possible information loss.
- They participate in overload resolution like built-in conversions when applicable.
- Abuse creates hidden conversions that confuse readers—reserve for domain types with clear mappings (units, identifiers).
- They do not replace good constructor or factory methods when conversion is not natural.

---

#### Q14. What happens when you cast a `double` to `int` — is rounding or truncation applied?

What happens when you cast a `double` to `int` — is rounding or truncation applied?

**Answer:** Casting floating-point to integral types truncates toward zero, dropping the fractional part without rounding to nearest. `3.9` and `-3.9` cast to `3` and `-3` respectively.

- `Convert.ToInt32(3.9)` rounds to nearest even in default mode—different from cast truncation.
- `Math.Floor`, `Ceiling`, and `Round` express explicit rounding before casting when business rules require it.
- Overflow still possible if double magnitude exceeds int range—unchecked cast wraps in unchecked context.
- See Q3 for API differences between cast and `Convert`.

---

#### Q15. What is the difference between `default(T)` casting patterns and `Convert` methods for nullable value types?

What is the difference between `default(T)` casting patterns and `Convert` methods for nullable value types?

**Answer:** `default(Nullable<int>)` is null without value, while converting null with `Convert` may yield zero for value types depending on overload. Parsing empty strings differs between `TryParse` (false) and `Convert` behaviors.

- Use `TryParse` for optional form fields representing absent numbers with `int?`.
- `Convert.ChangeType` on boxed null often returns null for nullable target types when configured appropriately.
- Keep one consistent strategy per API layer to avoid null vs zero ambiguity.
- Document whether absent numeric input maps to null or zero for consumers.

---

#### Q16. When converting between `string` and numeric types in APIs and logs, why is `InvariantCulture` often specified explicitly?

When converting between `string` and numeric types in APIs and logs, why is `InvariantCulture` often specified explicitly?

**Answer:** Explicit invariant culture makes serialized numbers and dates identical on every machine, so downstream parsers, diff tools, and aggregators do not misread separators when the server's locale differs from the developer's workstation.

- Logs consumed globally should not flip decimal commas based on OS language.
- REST and JSON often use invariant-like formats even though JSON numbers typically avoid locale separators.
- Unit tests assert expected strings without setting thread culture when invariant is specified at the call site.
- See Q10 and Input & Output culture questions for the full picture.

---

### 06. Control Flow & Loops

#### Q1. What is the difference between `if/else` and the ternary operator?

What is the difference between `if/else` and the ternary operator?

**Answer:** `if/else` is a statement that executes one of two blocks of arbitrary size and may perform multiple actions, while the ternary operator is a single expression that selects between two values. Use ternary for simple assignments; use `if/else` for multi-statement branches and side effects.

- Ternary requires both branches to be expressions compatible enough for type inference.
- Nested ternaries harm readability compared to `if/else` chains or switch expressions.
- See Operators Q11 for evaluation semantics.
- Style guides often limit ternary to one line of choice between two values.

---

#### Q2. What is the difference between traditional `switch` and switch expressions (C# 8+)?

What is the difference between traditional `switch` and switch expressions (C# 8+)?

**Answer:** Traditional `switch` is a statement with `case` labels, optional `break`, and fall-through restrictions, while switch expressions (`var result = x switch { ... }`) produce a value with expression-bodied arms separated by commas and use exhaustive pattern matching rules.

- Switch expressions discourage fall-through bugs by requiring `=>` arms and no implicit fall-through.
- Both support type, constant, and relational patterns in modern C#.
- Switch expressions must cover all inputs or include a discard `_` pattern when exhaustive.
- Prefer switch expressions for mapping enums to labels; use statements when cases need multiple statements without local functions.

---

#### Q3. When should you use `for`, `foreach`, `while`, and `do-while`?

When should you use `for`, `foreach`, `while`, and `do-while`?

**Answer:** Use `for` when you need an index counter and known iteration bounds, `foreach` to enumerate `IEnumerable` sequences without manual indexing, `while` when the loop condition is tested before each iteration, and `do-while` when the body must run at least once before the condition is checked.

- `foreach` is idiomatic for collections and LINQ-friendly sequences; it hides enumerator disposal via compiler-generated try/finally.
- `for` suits arrays when you need the index for parallel arrays or reverse iteration.
- `while` fits polling and read-until-done loops with unknown iteration count.
- `do-while` fits menu loops that display once before validating exit condition.

---

#### Q4. What is the difference between `break`, `continue`, and `return` inside a loop?

What is the difference between `break`, `continue`, and `return` inside a loop?

**Answer:** `break` exits the innermost enclosing loop or switch immediately, `continue` skips to the next iteration of the innermost loop, and `return` exits the entire method (after running any enclosing `finally` blocks) regardless of loop nesting.

- `break` in nested loops does not exit outer loops unless you use labeled break (rare) or refactor.
- `continue` re-evaluates the loop condition before the next body execution.
- `return` inside `try` still executes `finally` before the method actually returns—see Q15 and Gotcha 7.
- Misusing `break` vs `return` in search loops changes whether cleanup after the loop runs.

---

#### Q5. What are common pitfalls with nested loops and loop variable scope?

What are common pitfalls with nested loops and loop variable scope?

**Answer:** Nested loops multiply complexity and can hide O(n²) performance; reusing the same index variable name in inner loops shadows outer variables and confuses readers. Closure capture of loop variables in lambdas behaved differently before C# 5 for `foreach` vs `for`.

- Prefer extracting inner loops to methods when depth exceeds two levels with non-trivial logic.
- `for` loop variables are scoped to the loop; declaring the same name in nested `for` headers is legal but error-prone.
- See Q10 on foreach closure semantics in older mental models vs fixed foreach iteration variable per iteration.
- Off-by-one errors at inner boundaries often cause skipped or duplicate processing in matrices.

---

#### Q6. What is a switch expression, and how do relational and property patterns work in `switch`?

What is a switch expression, and how do relational and property patterns work in `switch`?

**Answer:** Switch expressions map input patterns to result expressions using `=>` arms, supporting constant patterns, type patterns, relational guards (`when`), property patterns (`{ Length: > 0 }`), and positional patterns on tuples and records.

- Relational patterns combine with constants: `var label = score switch { >= 90 => "A", >= 80 => "B", _ => "C" };`.
- Property patterns deconstruct shape: `obj switch { { Status: OrderStatus.Shipped } => true, _ => false }`.
- The compiler warns when not all enum values are handled if exhaustive analysis applies.
- Switch expressions are expression-oriented—each arm must yield a compatible type.

---

#### Q7. What is the difference between `break` in a `switch` vs `break` in a loop?

What is the difference between `break` in a `switch` vs `break` in a loop?

**Answer:** In both constructs `break` exits only the innermost enclosing switch or loop—it does not exit nested switches inside loops beyond one level. In modern switch expressions, `break` is replaced by arm separation without fall-through.

- Classic switch requires `break` (or `return`/`goto case`) at the end of each case unless the case ends with a jump.
- Accidentally omitting `break` in classic switch caused fall-through bugs before C# disallowed unreachable fall-through in many cases.
- `break` inside a case nested within a loop exits the switch, not the loop.
- Use `return` from a method or refactor when you need to exit both switch and loop.

---

#### Q8. When is `goto` still used in C# (e.g., `goto case`, `goto default`), and why is it generally discouraged?

When is `goto` still used in C# (e.g., `goto case`, `goto default`), and why is it generally discouraged?

**Answer:** `goto case` and `goto default` jump to another switch label when sharing logic between cases without duplicating code. General `goto` labels are discouraged because unstructured jumps make control flow hard to follow and refactor compared to loops, methods, and structured switches.

- `goto case` appears when one case falls through intentionally to shared cleanup in legacy switch statements.
- Switch expressions and local functions largely eliminate the need for arbitrary labels.
- Acceptable in generated code or performance-critical state machines; rare in application business logic.
- Excessive `goto` correlates with maintenance defects in large methods.

---

#### Q9. What is the scope of a variable declared in the initializer of a `for` loop (C# rules)?

What is the scope of a variable declared in the initializer of a `for` loop (C# rules)?

**Answer:** A variable declared in the `for` loop initializer is scoped to the entire `for` statement and is not visible after the loop ends. Each `for` statement creates its own scope for that variable.

- You cannot reference the loop variable after the loop if it was declared in the initializer: `for (int i = 0; ...)` then `i` is out of scope.
- Declare the variable outside the loop if you need the final index value after completion.
- C# disallows assigning to the foreach iteration variable—different rule from `for` index mutation.
- Nested loops can each declare `int i` in separate `for` headers in separate scopes.

---

#### Q10. Why did C# 5 change loop variable capture semantics in lambdas, and how does that affect `foreach` vs `for`?

Why did C# 5 change loop variable capture semantics in lambdas, and how does that affect `foreach` vs `for`?

**Answer:** Before C# 5, a lambda closing over a `foreach` iteration variable captured the single shared variable, so all delegates saw the final value after the loop. C# 5 changed `foreach` to capture each iteration's value separately; `for` loop index capture still closes over one mutable variable unless you copy to a local inside the loop.

- Bug pattern: tasks created in `foreach` all observing the last item before the fix.
- Inside `for`, copy `int copy = i;` before async lambdas when you need per-iteration capture with mutable index semantics.
- Understanding this prevents subtle parallel and async bugs in LINQ and Task loops.
- Modern code often uses `Select((item, index) => ...)` to avoid manual capture issues.

---

#### Q11. Can you modify the collection you are iterating in a `foreach` loop — what exception results?

Can you modify the collection you are iterating in a `foreach` loop — what exception results?

**Answer:** You cannot add or remove elements from most collections during `foreach` enumeration; doing so throws `InvalidOperationException` with message that collection was modified. Mutating elements in place may be allowed for some collection types but is risky if it changes structure.

- Use `for` backward over indices or build a new collection when removing items during iteration.
- `List<T>` documents that structural changes during enumeration invalidate the enumerator.
- Concurrent modification from another thread causes the same exception on many collection types.
- LINQ deferred queries may re-enumerate underlying collections that changed between operations.

---

#### Q12. What is the difference between `while` and `do-while` when the condition is false on the first check?

What is the difference between `while` and `do-while` when the condition is false on the first check?

**Answer:** `while` evaluates the condition before the first iteration and may skip the body entirely if the condition is initially false. `do-while` always executes the body at least once before evaluating the condition at the bottom.

- Use `do-while` for input validation menus that must prompt at least once.
- Both support `break` and `continue` with the same innermost-loop rules.
- Infinite loops use `while (true)` with internal `break` when exit logic is complex.
- Condition side effects run zero times in `while` if initially false, once before check in `do-while` after first body run.

---

#### Q13. When would you prefer a `switch` over a chain of `if/else if` statements?

When would you prefer a `switch` over a chain of `if/else if` statements?

**Answer:** Prefer `switch` when discriminating a single expression against many constant or pattern values, especially enums, because switch expressions and statements communicate intent and enable compiler exhaustiveness checks. Long unrelated boolean conditions remain clearer as `if/else`.

- Switch on strings and enums is efficient and readable with modern pattern support.
- Switch expressions reduce temporary variables when mapping inputs to outputs.
- `if/else` fits disparate conditions that are not variations of one expression's shape.
- Performance differences are usually negligible; readability and correctness matter more.

---

#### Q14. What is pattern matching with `is` in an `if` statement vs a `switch` on type?

What is pattern matching with `is` in an `if` statement vs a `switch` on type?

**Answer:** `if (obj is Type t)` handles one or two type tests inline, while `switch (obj)` with multiple type patterns scales when many types map to different handling paths without nested if ladders.

- Switch on type with arms `case Customer c:` (classic) or expression patterns consolidates dispatch.
- Both integrate with nullable flow analysis when patterns succeed.
- Prefer switch when adding a new type means adding an arm—a table-driven dispatch shape.
- Virtual methods often replace type switches for open hierarchies—see Module 02 polymorphism.

---

#### Q15. What happens if you use `return` inside a `try` block that has a `finally` — which executes first?

What happens if you use `return` inside a `try` block that has a `finally` — which executes first?

**Answer:** When `return` executes in `try`, the runtime runs the associated `finally` block before the method actually returns to the caller. If `finally` also contains `return`, that return value overrides the `try` return—see Gotcha 7.

- Cleanup in `finally` runs even when `try` returns normally or via exception.
- Async methods compile similarly with respect to `finally` in many patterns but add state machine complexity.
- Do not put `return` in `finally` except in generated code—it hides control flow.
- See Exception Handling chapter for interaction with exceptions during return.

---

### 07. Methods

#### Q1. What are the `out`, `ref`, and `in` parameter modifiers? Explain their usage.

What are the `out`, `ref`, and `in` parameter modifiers? Explain their usage.

**Answer:** `ref` passes an alias to an existing variable so the callee can read and write it; `out` requires the callee to assign before return and represents an extra output slot; `in` passes a readonly alias for large structs to avoid copy cost while preventing modification through the parameter.

- Callers must pass `ref` and `out` arguments with the keyword at the call site (`Method(ref x, out y)`).
- `out` variables can be declared inline at the call site in modern C# (`TryParse(s, out int n)`).
- `in` suits large readonly struct parameters in hot paths—see Q12.
- See Q11 for choosing among them.

---

#### Q2. What is the `params` keyword in method definitions?

What is the `params` keyword in method definitions?

**Answer:** `params` allows a method to accept a variable number of arguments of a specified array element type, which the compiler packs into an array at the call site. Only one `params` parameter is allowed and it must be the last parameter—see Gotcha 12.

- `void Log(params string[] messages)` enables `Log("a", "b")` without explicit array syntax.
- Passing an existing array explicitly still works without extra copying in many cases.
- Overload resolution prefers non-params overloads when an exact match exists.
- `params` with `ReadOnlySpan<T>` expands options in newer language versions for performance.

---

#### Q3. What are expression-bodied members in C#?

What are expression-bodied members in C#?

**Answer:** Expression-bodied members use `=>` syntax to define methods, properties, accessors, or constructors with a single expression instead of a braced block, reducing noise for trivial forwarding and computed members.

- Example: `public int Area => Width * Height;` for a read-only property.
- Expression-bodied methods can return values or void (`void M() => Console.WriteLine("hi");`).
- Complex logic should remain block-bodied for debugging and multiple statements.
- Module 02 covers expression-bodied properties in depth.

---

#### Q4. Explain named arguments and optional parameters in C#.

Explain named arguments and optional parameters in C#.

**Answer:** Optional parameters declare default values in the method signature so callers may omit trailing arguments. Named arguments supply parameters by name (`Method(timeout: 30, retries: 3)`) regardless of order, improving readability for long parameter lists.

- Optional parameters must come after required parameters unless all trailing parameters are optional.
- Defaults are compile-time constants embedded at call sites—see Gotcha 13 on recompile requirement when defaults change.
- Named arguments help with boolean flag clarity at call sites.
- Overload resolution considers optional parameters and may introduce ambiguity—see Constructors chapter in Module 02.

---

#### Q5. What are local functions in C#?

What are local functions in C#?

**Answer:** Local functions are methods declared inside another method's body, visible only within the enclosing member, useful for splitting algorithm steps without polluting the type's namespace or capturing class state unnecessarily.

- They can be static local functions to avoid accidental capture of instance members when not needed.
- Iterator methods and validation helpers commonly use local functions for clarity.
- Local functions can access outer local variables (closure) unless declared static.
- See Q17 for comparison with private instance methods.

---

#### Q6. What is the `yield` keyword and iterators in C#? *(Cross-ref: Module 03 — IEnumerable)*

What is the `yield` keyword and iterators in C#? *(Cross-ref: Module 03 — IEnumerable)*

**Answer:** `yield return` and `yield break` implement iterator methods that compile into state machines implementing `IEnumerable<T>` or `IEnumerator<T>`, producing elements lazily one at a time without building a full in-memory collection upfront.

- Consumers foreach over the sequence; execution resumes after each `yield return` when the next element is requested.
- Lazy evaluation saves memory for large or infinite sequences filtered by callers.
- Iterator methods cannot mix unstructured `yield` with `try/finally` patterns easily in all cases—language rules apply.
- Module 03 expands IEnumerable, LINQ, and deferred execution interactions.

---

#### Q7. Explain method overloading — what makes two methods overloads vs duplicate definitions?

Explain method overloading — what makes two methods overloads vs duplicate definitions?

**Answer:** Overloads share the same method name but differ in parameter count or types (and optionally generic arity); the return type alone cannot distinguish overloads. Duplicate signatures with only return type differing are compile errors.

- `void Print(int x)` and `void Print(string s)` are valid overloads.
- `ref` vs `out` vs value parameter modifiers participate in signature distinction.
- Generic methods overload on type parameter count and constraints.
- See Q8 for resolution when multiple overloads apply.

---

#### Q8. How does overload resolution work when multiple overloads could apply — what is the "better function member" rule?

How does overload resolution work when multiple overloads could apply — what is the "better function member" rule?

**Answer:** The compiler picks the best match by preferring fewer conversions, better conversion kinds (implicit over explicit), and non-params candidates over expanded params forms. If no single best member exists, CS0121 ambiguity error results.

- Exact parameter type match beats conversion from `int` to `long`.
- More specific derived type beats base type when both are candidates.
- Named arguments and casts can disambiguate intentional choices.
- See Q16 for ambiguity resolution tactics.

---

#### Q9. Why can't you overload methods by return type alone?

Why can't you overload methods by return type alone?

**Answer:** Call sites often ignore return values, so the compiler cannot infer which overload to invoke from return type context alone. Method signature identity for overload purposes includes name and parameter types, not the return type.

- `int GetValue()` and `string GetValue()` with identical parameters cannot coexist.
- Async overloads differ by return type (`Task` vs `Task<int>`) only when combined with `async` pattern and different parameter lists—or the async return type is part of a distinct signature in generic scenarios with constraints.
- Explicit interface implementation can expose conflicting return types on different interfaces implemented by one class.
- Return type participates in conversion targets after an overload is already chosen.

---

#### Q10. What is the difference between call-by-value for value types vs reference types at the parameter boundary?

What is the difference between call-by-value for value types vs reference types at the parameter boundary?

**Answer:** Value type parameters receive a copy of the bits; mutating the parameter variable does not affect the caller's variable unless `ref` or `out` is used. Reference type parameters copy the reference; mutating the object's fields affects the shared object, but reassigning the parameter to a new object does not change the caller's variable without `ref`.

- See Gotcha 11 on ref reassignment vs mutation.
- `readonly` fields on structs passed by value cannot be mutated through the copy.
- Large structs should use `in` or `ref readonly` to avoid copy cost when read-only.
- Reference type null can be passed; callee can assign parameter to null without affecting caller's reference.

---

#### Q11. When should you use `ref` vs `out` vs `in` for parameters?

When should you use `ref` vs `out` vs `in` for parameters?

**Answer:** Use `out` for Try-pattern results and multiple return values that are definitely assigned in the method. Use `ref` when the method must read and write an existing variable the caller already initialized. Use `in` for large structs the method reads but must not modify through the alias.

- `TryParse` is the canonical `out` pattern—see Q13.
- Swap methods and in-place sorting sometimes use `ref` on locals.
- Avoid `out` when the caller already has meaningful input in the variable that the callee should read (`ref` instead).
- `in` documents readonly intent to callers and analyzers.

---

#### Q12. What problem does the `in` modifier solve for large readonly structs?

What problem does the `in` modifier solve for large readonly structs?

**Answer:** Passing a large struct by value copies every field, which is expensive for big value types like 3D transforms or matrix chunks. `in` passes a readonly reference alias, eliminating the copy while preventing silent mutation through the parameter.

- Call site uses `Method(in bigStruct)` for clarity at the API boundary.
- Defensive copies may still occur if the callee stores the parameter to a field in some scenarios (struct lifetime rules).
- Prefer `readonly struct` with `in` parameters together for clarity and analyzer support.
- Small structs (Point, int pairs) often remain pass-by-value for simplicity.

---

#### Q13. What is the Try-pattern (`bool TryX(..., out T result)`), and why is it preferred over exceptions for expected failures?

What is the Try-pattern (`bool TryX(..., out T result)`), and why is it preferred over exceptions for expected failures?

**Answer:** Try-pattern methods return `false` when an operation cannot complete normally (parse failure, dictionary miss) and assign a default or meaningful `out` value, avoiding exception overhead for control flow that is common rather than exceptional.

- `Dictionary.TryGetValue`, `int.TryParse`, and `Enum.TryParse` follow this convention.
- Exceptions remain appropriate for violated invariants and unexpected environmental failures.
- Consistent naming (`Try` prefix, `out` last) makes APIs discoverable.
- See Gotcha 10 and Input & Output Q3 on user input parsing.

---

#### Q14. Can optional parameters precede required parameters — what are the ordering rules?

Can optional parameters precede required parameters — what are the ordering rules?

**Answer:** Required parameters must appear before optional ones in the parameter list unless the trailing parameters are all optional and callers use named arguments to supply required values after skipped optionals—which is confusing and should be avoided.

- Valid: `void M(int a, int b = 0, int c = 0)`.
- Invalid: `void M(int a = 0, int b)` without special call patterns.
- API design places rarely used flags at the end with defaults.
- Changing default values does not update already compiled callers—Gotcha 13.

---

#### Q15. What is the difference between `params int[]` and passing an explicit `int[]` at the call site?

What is the difference between `params int[]` and passing an explicit `int[]` at the call site?

**Answer:** Both end up as an array inside the method; `params` additionally allows variadic call syntax spreading individual arguments. Overload resolution treats explicit array argument as matching the array parameter directly without params expansion when an exact overload exists.

- `Method(new int[] { 1, 2 })` passes one array object; `Method(1, 2)` creates an array via params expansion.
- Null passed to params parameter can be ambiguous—prefer explicit array or overload without params.
- Performance-sensitive APIs may avoid params to reduce hidden array allocations.
- See Q2 on params placement rules.

---

#### Q16. When does overload resolution fail with ambiguity (CS0121), and how do casts or named arguments resolve it?

When does overload resolution fail with ambiguity (CS0121), and how do casts or named arguments resolve it?

**Answer:** Ambiguity occurs when two overloads are equally good matches for the argument list, such as two implicit conversions of equal rank. Resolve by casting arguments to the intended parameter type, using named parameters to select an overload with fewer applicable candidates, or renaming methods to clarify intent.

- `(long)x` or `(int)x` disambiguates numeric overloads.
- Adding an overload with exact match types is the long-term API fix.
- Generic inference failures are a related but distinct compiler error family.
- Document overload sets carefully when adding optional and params overloads together.

---

#### Q17. What is the difference between a local function and a private instance method in the same class?

What is the difference between a local function and a private instance method in the same class?

**Answer:** Local functions are scoped inside a single member and can access local variables and parameters of the enclosing method via closure, while private methods are class-level members callable from any instance or static method in the class depending on modifiers.

- Static local functions cannot capture instance state unless passed explicitly—encourages clearer dependencies.
- Private methods appear in API surface of the type for testing and reuse across multiple members.
- Local functions are not virtual and cannot implement interfaces.
- Choose local functions for single-use algorithm steps tightly coupled to one method's locals.

---

#### Q18. What is recursion, what is a base case, and what risk does unbounded recursion pose?

What is recursion, what is a base case, and what risk does unbounded recursion pose?

**Answer:** Recursion is when a method calls itself to solve smaller subproblems; the base case is the condition where the method returns without further recursive calls. Unbounded recursion exhausts the call stack and throws `StackOverflowException`.

- Every recursive path needs a base case and progress toward it (smaller input, closer to terminal state).
- Tail recursion is not guaranteed to optimize to iteration in C#—do not rely on it for deep stacks.
- Tree and graph traversals use recursion with clear exit conditions or switch to explicit stacks for depth safety.
- Mutual recursion between two methods requires the same discipline on both sides.

---

#### Q19. Can `out` variables be declared inline at the call site (`TryParse(text, out int n)`)?

Can `out` variables be declared inline at the call site (`TryParse(text, out int n)`)?

**Answer:** Yes, C# 7+ allows declaring the type inline in the `out` argument position, scoping the new variable to the enclosing block and enabling concise Try-pattern usage without a separate declaration line.

- `if (int.TryParse(line, out int n))` uses `n` in the if block.
- Multiple inline `out` declarations in one call are supported when the method has multiple `out` parameters.
- The variable is definitely assigned when `TryParse` returns true per flow analysis.
- See Q1 and Q13 for Try-pattern context.

---

#### Q20. What is the difference between mutating an object through a reference parameter vs reassigning the parameter variable itself?

What is the difference between mutating an object through a reference parameter vs reassigning the parameter variable itself?

**Answer:** Mutating fields on a reference-type object through a parameter (`customer.Name = "x"`) affects the caller's object because both refer to the same instance. Reassigning the parameter (`customer = new Customer()`) only changes the local alias inside the method unless the parameter is `ref Customer`.

- Gotcha 11 states this distinction explicitly for interviews.
- Value types always copy unless `ref`/`out`/`in`; mutation on struct parameter mutates the copy only.
- `ref` reassignment lets callee replace caller's variable binding: `void Reset(ref Customer c) { c = new Customer(); }`.
- Understanding this prevents bugs when trying to "replace" objects passed without `ref`.

---

### 08. Strings

#### Q1. Explain string handling in C# (`string` vs `StringBuilder`).

Explain string handling in C# (`string` vs `StringBuilder`).

**Answer:** `string` is an immutable reference type optimized for relatively stable text, while `StringBuilder` provides a mutable buffer for repeated append operations that would otherwise create many intermediate string objects. Choose `string` for simple composition; choose `StringBuilder` for many updates in loops.

- `string` methods like `Replace` and `Substring` return new instances without modifying the original.
- `StringBuilder` exposes `Append`, `Insert`, and `Remove` mutating an internal buffer with amortized growth.
- Interpolation and `string.Join` often suffice without `StringBuilder` for moderate concatenation.
- See Q9 on loop concatenation performance.

---

#### Q2. What are the different ways to format strings in C#? (`String.Format`, interpolation, composite formatting)

What are the different ways to format strings in C#? (`String.Format`, interpolation, composite formatting)

**Answer:** Composite formatting (`string.Format`, `Console.WriteLine` with placeholders) uses indexed holes and format providers. String interpolation (`$"..."`) embeds expressions directly. Both ultimately call formatting infrastructure with optional `IFormatProvider`.

- Interpolation is translated to `FormattableString` or `string.Format` calls at compile time.
- Culture-sensitive formatting passes `CultureInfo` explicitly or uses current culture by default.
- `StringBuilder.AppendFormat` combines mutable buffers with composite patterns.
- See Input & Output Q4 and Q8 for console-specific usage.

---

#### Q3. Are strings mutable or immutable in C#? What are the implications?

Are strings mutable or immutable in C#? What are the implications?

**Answer:** Strings are immutable: after construction, their character content cannot change. Any operation that appears to modify a string returns a new string instance, which simplifies threading, interning, and hash caching at the cost of allocations when building large text incrementally.

- Safe to share string references across threads without locks for content stability.
- Repeated concatenation in loops allocates O(n²) total characters without `StringBuilder`.
- Custom APIs should not expose mutable char buffers as `string`; use `StringBuilder` or `char[]` internally until finalized.
- Immutability enables the compiler and runtime to intern literal strings—see Q4.

---

#### Q4. What is string interning?

What is string interning?

**Answer:** String interning stores one copy of each distinct literal or interned string content in a pool so multiple references can share the same object, saving memory and enabling reference equality for identical content when interned.

- Literal `"hello"` in source may be interned automatically at compile/load time.
- `string.Intern` forces lookup or insertion into the intern pool—see Q15.
- Two equal non-interned strings compare equal with `==` but may not be reference-equal—Gotcha 1.
- Interning trades memory deduplication for lifetime pinning of strings never collected while referenced from the pool.

---

#### Q5. What is the difference between `==`, `Equals`, `Compare`, and `CompareTo` for strings?

What is the difference between `==`, `Equals`, `Compare`, and `CompareTo` for strings?

**Answer:** `==` and instance `Equals` compare string content with ordinal or overloaded semantics depending on overload; `string.Compare` returns signed ordering with explicit `StringComparison`; `CompareTo` implements `IComparable<string>` for default sort order.

- Use `StringComparison` overloads to avoid culture surprises—see Q6.
- `Compare` is static and accepts comparison type explicitly; good for sort keys.
- Reference equality differs from content equality unless interning aligns references—Q16.
- `Equals` overload without comparison uses ordinal default in modern .NET for `string.Equals(string)`.

---

#### Q6. When should you use `StringComparison.Ordinal` vs `OrdinalIgnoreCase` vs culture-sensitive comparisons?

When should you use `StringComparison.Ordinal` vs `OrdinalIgnoreCase` vs culture-sensitive comparisons?

**Answer:** Use ordinal comparisons for identifiers, file paths, protocol tokens, and dictionary keys where byte-level Unicode order is stable. Use culture-sensitive comparison for user-visible sorting and matching words in natural language. Use `OrdinalIgnoreCase` for case-insensitive identifiers like HTTP headers or enum-like names when culture rules would be wrong.

- Culture-sensitive `string.Compare("i", "I", culture)` differs between Turkish and invariant for dotted/dotless I.
- LINQ and dictionary keys for internal IDs should use ordinal comparers.
- UI sort in user's language uses `StringComparison.CurrentCulture` or explicit culture.
- Security-sensitive comparisons (passwords, tokens) use fixed rules—often ordinal or fixed-time specialized APIs.

---

#### Q7. What are verbatim string literals (`@"..."`), and when are they useful?

What are verbatim string literals (`@"..."`), and when are they useful?

**Answer:** Verbatim strings prefix `@` so backslashes are literal and quotes are doubled (`""`) instead of escaped, which simplifies Windows paths, regular expression patterns, and multi-line text without doubling backslashes.

- `@"C:\Users\name"` avoids `"C:\\Users\\name"`.
- Newlines in verbatim strings are literal line breaks in source.
- Interpolation combines `$` and `@`: `$@"Hello {name}\there"`.
- Raw string literals (Q8) supersede some verbatim use cases for embedded quotes.

---

#### Q8. What are raw string literals (`"""..."""`, C# 11+), and how do they handle quotes and newlines?

What are raw string literals (`"""..."""`, C# 11+), and how do they handle quotes and newlines?

**Answer:** Raw string literals delimit content with triple quotes (`"""`) and optional indentation stripping, allowing arbitrary quotes and multi-line JSON, SQL, or XML without escape proliferation.

- Opening quotes on their own line enable content that starts on the next line with dedented margins.
- More `"` characters in delimiter handle content containing triple quotes.
- Combine with `$` for interpolation inside raw strings with rules for brace placement on separate lines when needed.
- Prefer raw strings over heavily escaped verbatim strings for embedded code or markup templates.

---

#### Q9. What is the difference between `StringBuilder` and repeated string concatenation in a loop?

What is the difference between `StringBuilder` and repeated string concatenation in a loop?

**Answer:** Loop concatenation with `+` or `$"{s}{item}"` creates a new string each iteration, copying all prior content repeatedly. `StringBuilder` amortizes growth across a buffer, reducing total copying to roughly linear in final length.

- For small loops or few iterations, concatenation is readable and fast enough.
- Profile before optimizing; `StringBuilder` has overhead for tiny results.
- `string.Join` and `String.Concat` with array or span inputs batch concatenation efficiently.
- See Q18 when `StringBuilder` is not ideal.

---

#### Q10. What is the difference between `string.Concat`, the `+` operator, and interpolation for combining text?

What is the difference between `string.Concat`, the `+` operator, and interpolation for combining text?

**Answer:** All produce new immutable strings; the compiler often optimizes simple chains of `+` on constants into one literal. Interpolation and `Concat` clarify intent for multiple parts; runtime behavior converges on allocation of a new string with combined content.

- Constant folding merges `"a" + "b"` at compile time.
- Interpolation evaluates expressions once into temporaries before formatting.
- `String.Concat(ReadOnlySpan<string>)` reduces allocations in modern APIs.
- Choose based on readability; micro-differences matter only in hot paths.

---

#### Q11. What is the difference between `IsNullOrEmpty`, `IsNullOrWhiteSpace`, and checking `Length == 0`?

What is the difference between `IsNullOrEmpty`, `IsNullOrWhiteSpace`, and checking `Length == 0`?

**Answer:** `IsNullOrEmpty` is true for null or zero-length strings. `IsNullOrWhiteSpace` also treats Unicode whitespace-only strings as empty. Checking `Length == 0` requires non-null reference or throws if null.

- Use null-conditional before length: `s?.Length == 0` distinguishes null from empty if needed.
- Whitespace includes spaces, tabs, and culture-specific space characters beyond `' '`.
- Validation of user names often needs `IsNullOrWhiteSpace`; protocol tokens may allow internal spaces but not empty.
- NRT flow analysis may still require null checks before dereferencing.

---

#### Q12. What is the difference between culture-sensitive (`ToUpper()`) and invariant (`ToUpperInvariant()`) case conversion?

What is the difference between culture-sensitive (`ToUpper()`) and invariant (`ToUpperInvariant()`) case conversion?

**Answer:** Culture-sensitive casing uses rules of `CurrentCulture` or Turkish etc., which can change dotted/dotless I behavior. Invariant casing uses fixed Unicode rules independent of user locale, preferred for identifiers and normalized keys.

- `ToUpper()` without culture uses current culture—dangerous for internal keys in global apps.
- `ToUpperInvariant()` is stable across machines for protocol identifiers.
- Security-sensitive normalization may need custom rules beyond simple casing.
- See Q6 for comparison vs conversion distinction.

---

#### Q13. What methods would you use to split, trim, replace, pad, and search within strings?

What methods would you use to split, trim, replace, pad, and search within strings?

**Answer:** `Split` divides on separators; `Trim`/`TrimStart`/`TrimEnd` remove whitespace or specified chars; `Replace` substitutes substrings; `PadLeft`/`PadRight` align fixed-width fields; `Contains`, `IndexOf`, and `StartsWith`/`EndsWith` search with optional `StringComparison`.

- Span-based overloads on modern .NET reduce allocations for parsing pipelines.
- `Split` with `StringSplitOptions.RemoveEmptyEntries` cleans CSV-like input.
- Regular expressions handle complex patterns when literal methods are insufficient.
- `ReadOnlySpan<char>` slicing previews stack-friendly parsing without substring allocation in advanced scenarios.

---

#### Q14. What is UTF-16 storage in .NET strings, and how does that relate to surrogate pairs and `char`?

What is UTF-16 storage in .NET strings, and how does that relate to surrogate pairs and `char`?

**Answer:** .NET `string` stores UTF-16 code units in a contiguous buffer; most characters are one `char`, but supplementary Unicode characters (emoji, rare scripts) occupy two `char` surrogate pairs. Length counts code units, not grapheme clusters users perceive as one character.

- Iterating `foreach (char c in s)` visits code units, not full Unicode scalars—use `StringInfo` or `Rune` for grapheme-aware logic.
- `char` is 16-bit UTF-16 code unit, not a full Unicode code point in all cases.
- Encoding to UTF-8 for wire formats uses `Encoding.UTF8.GetBytes` producing variable byte lengths.
- Surrogate pair corruption occurs if you manually splice strings at wrong indices.

---

#### Q15. What is the string intern pool, and what does `string.Intern` do?

What is the string intern pool, and what does `string.Intern` do?

**Answer:** The intern pool is a runtime table of unique string contents; `string.Intern` returns the pooled reference for the argument's content, creating an entry if absent. Literals may already be interned without explicit calls.

- Useful rarely for deduplicating massive repeated dynamic strings with identical content.
- Interned strings live for process lifetime if referenced from pool—memory trade-off.
- Do not intern unbounded user input—it can grow the pool without bound in pathological cases.
- See Gotcha 1 and Q4 for reference vs content equality implications.

---

#### Q16. Why can two strings with identical content fail `ReferenceEquals` while still passing `==`?

Why can two strings with identical content fail `ReferenceEquals` while still passing `==`?

**Answer:** `==` for strings compares character content (ordinal by default in many overload paths), while `ReferenceEquals` checks object identity. Two separately constructed strings with the same text are equal by content but may be different heap objects unless interning aligns them.

- Literal `"hi"` reused in source may be the same reference; `new string("hi".ToCharArray())` is often not.
- Gotcha 1 is the canonical interview trap on interning.
- Do not use `ReferenceEquals` for string content comparison—use `==` or `Equals` with explicit comparison.
- Performance-sensitive deduplication sometimes interns known keys intentionally.

---

#### Q17. What is the difference between `Substring` and range/index syntax (`s[start..end]`) for slicing strings?

What is the difference between `Substring` and range/index syntax (`s[start..end]`) for slicing strings?

**Answer:** Both extract contiguous portions; `Substring(start, length)` uses start and length, while range syntax `s[start..end]` uses start inclusive and end exclusive indices with clearer intent for "from here to there."

- Negative indices from end work with `^` in ranges: `s[^3..]` last three characters.
- Both allocate new strings because strings are immutable.
- Out-of-range indices throw `ArgumentOutOfRangeException` similarly.
- Span `s.AsSpan(start, length)` avoids allocation when downstream APIs accept span.

---

#### Q18. When is `StringBuilder` not the best choice despite many append operations?

When is `StringBuilder` not the best choice despite many append operations?

**Answer:** When the final size is known upfront, a pre-sized `char[]` or single `string.Create` call may allocate once without `StringBuilder` overhead. Very few appends (two or three) are often clearer with interpolation or `Concat`.

- `string.Create(length, state, callback)` fills a buffer in one shot for expert scenarios.
- Logging frameworks batch efficiently without manual `StringBuilder` in application code.
- Pooling `StringBuilder` instances (`StringBuilderCache` internally in BCL) is framework concern, not typical app code.
- Measure: small `StringBuilder` growth copies dominate only at scale.

---

#### Q19. How does string interpolation handle format specifiers and alignment (`$"{price:C2}"`, `$"{name,-20}"`)?

How does string interpolation handle format specifiers and alignment (`$"{price:C2}"`, `$"{name,-20}"`)?

**Answer:** Interpolation holes accept format after colon (`:C2` currency two decimals) and alignment after comma (`,-20` left-align width 20), mirroring composite formatting rules inside `{expression,alignment:format}`.

- Alignment pads with spaces by default; format uses current culture unless `FormattableString.Invariant` or custom culture is applied.
- Complex formats delegate to `IFormattable` on the expression's type.
- Constant format strings enable compile-time checking in some analyzers for correctness.
- See Input & Output Q12 for console column layout examples.

---

#### Q20. What is the performance implication of calling `Replace` or `Trim` on large strings repeatedly?

What is the performance implication of calling `Replace` or `Trim` on large strings repeatedly?

**Answer:** Each call scans the full string and allocates a new string when changes occur, so chaining many passes over megabyte-scale text multiplies work and garbage. Prefer single-pass algorithms, spans, or `StringBuilder` pipelines for heavy text processing.

- `Replace` in a loop searching changing patterns can degrade badly without `StringBuilder`.
- Immutable returns mean no in-place win even when only one character changes.
- For hot paths, consider `MemoryExtensions`, regex compiled once, or streaming readers.
- Profile with realistic payload sizes before optimizing string pipelines.

---

### 09. Arrays

#### Q1. What are arrays in C#? How is memory managed for single-dimensional, multi-dimensional, and jagged arrays?

What are arrays in C#? How is memory managed for single-dimensional, multi-dimensional, and jagged arrays?

**Answer:** Arrays are reference types holding a contiguous (per dimension rules) sequence of elements with fixed rank established at creation. Single-dimensional arrays store elements in one block; rectangular multi-dimensional arrays store one block with row-major layout; jagged arrays are arrays of arrays, each row potentially different length on the heap.

- Single-dim: `int[]` object header plus element buffer on heap.
- Rectangular `int[,]` stores all cells in one array object with two lengths.
- Jagged `int[][]` has outer array referencing separate inner arrays—non-uniform row lengths possible.
- All array objects are heap-allocated; the variable is a reference—see Q5.

---

#### Q2. What is a jagged array?

What is a jagged array?

**Answer:** A jagged array is an array whose elements are themselves arrays (`int[][]`), allowing each row to have a different length, unlike rectangular `int[,]` where every row shares the same column count in one matrix object.

- Useful for sparse or ragged tables where rectangular storage would waste space.
- Access is two-step: `jagged[i][j]` with separate null checks for each row array.
- Memory layout differs from rectangular 2D—see Q15.
- Initialization often loops creating each inner array explicitly.

---

#### Q3. What is the difference between `Array.Copy()`, `Clone()`, and assigning one array variable to another?

What is the difference between `Array.Copy()`, `Clone()`, and assigning one array variable to another?

**Answer:** Assignment copies the reference only—both variables point to the same array object. `Clone()` on arrays performs shallow copy of elements into a new array object (new array, same element references for reference types). `Array.Copy` copies a range of elements from source to destination array, which may be existing or sized appropriately.

- Deep copy of elements requires looping or serialization, not `Clone()` alone for reference-type elements.
- `Copy` respects overlapping regions with defined behavior for same-array copies.
- Assignment does not duplicate elements—mutations visible through both references.
- See Q11 on shallow vs element copy semantics.

---

#### Q4. What is the difference between a single-dimensional array, a rectangular multi-dimensional array (`[,]`), and a jagged array (`[][]`)?

What is the difference between a single-dimensional array, a rectangular multi-dimensional array (`[,]`), and a jagged array (`[][]`)?

**Answer:** Single-dimensional arrays model vectors; rectangular arrays model fixed grid dimensions with one object; jagged arrays model rows as independent arrays allowing ragged shapes.

| Type | Syntax | Layout |
|---|---|---|
| Single-dim | `int[]` | One contiguous element block |
| Rectangular | `int[,]` | One object, `[row, col]` indexing |
| Jagged | `int[][]` | Outer array + per-row inner arrays |

Choose rectangular for dense matrices with fixed columns; jagged for variable row lengths.

---

#### Q5. Are arrays value types or reference types in C#?

Are arrays value types or reference types in C#?

**Answer:** Arrays are reference types inheriting from `System.Array`, regardless of whether their elements are value or reference types. The array variable holds a reference to the heap object containing lengths and element storage.

- `int[]` is a reference type; elements are value types stored inline in the array buffer.
- Default value of an array variable is `null`, not an empty array.
- `default(int[])` is null; use `Array.Empty<int>()` for zero-length singleton.
- Passing arrays to methods passes reference copy—see Q16.

---

#### Q6. What is array covariance for reference types, and why is `object[] arr = new string[3]; arr[0] = 42;` dangerous?

What is array covariance for reference types, and why is `object[] arr = new string[3]; arr[0] = 42;` dangerous?

**Answer:** Covariance allows assigning a derived array to a base array reference (`string[]` to `object[]`), but writing a non-compatible element through the base reference throws `ArrayTypeMismatchException` at runtime even though the assignment compiled.

- Covariance is safe for reading when element types match variance rules; writes can fail.
- Value type arrays are not covariant to `object[]` without boxing each element in a new array.
- Gotcha 8 is the classic interview example.
- Prefer `IReadOnlyList<T>` or generics for safe heterogeneous read scenarios without write holes.

---

#### Q7. What do `Array.Resize`, `Array.Fill`, and `Array.Clear` do — which allocate new memory?

What do `Array.Resize`, `Array.Fill`, and `Array.Clear` do — which allocate new memory?

**Answer:** `Array.Resize` creates a new array of the specified size and copies elements from the old array, replacing the reference passed by ref—it allocates new memory. `Array.Fill` sets all elements to a value in an existing array without reallocating. `Array.Clear` zeroes or nulls a range in an existing array without reallocating.

- `Resize` is O(n) copy; use `List<T>` when frequent growth is needed.
- `Fill` and `Clear` mutate in place for existing buffers.
- Clearing sets value types to zero and references to null.
- Distinguish `Array.Clear` from `list.Clear()` which removes elements in dynamic lists.

---

#### Q8. What is the difference between `Length` on a single-dimensional array vs `GetLength(dimension)` on multi-dimensional arrays?

What is the difference between `Length` on a single-dimensional array vs `GetLength(dimension)` on multi-dimensional arrays?

**Answer:** `Length` on single-dimensional arrays returns total element count. Multi-dimensional arrays expose `Rank` and per-dimension lengths via `GetLength(0)`, `GetLength(1)`, etc., because `Length` returns total elements across all dimensions.

- `int[,]` with 3 rows and 4 columns has `Length == 12` and `GetLength(0) == 3`, `GetLength(1) == 4`.
- Jagged outer array `Length` is row count; inner arrays have their own lengths.
- Bounds checks use these lengths on each access.
- Loop bounds should call the correct API for the array type to avoid logic errors.

---

#### Q9. How do you initialize arrays with collection initializer syntax and `new int[] { 1, 2, 3 }`?

How do you initialize arrays with collection initializer syntax and `new int[] { 1, 2, 3 }`?

**Answer:** Array initializer syntax lists elements in braces after `new Type[]` or with target-typed `new[] { 1, 2, 3 }` when the variable type is known. The compiler allocates an array of the correct size and assigns elements in order.

- Multi-dimensional rectangular arrays use nested brace syntax with uniform row lengths.
- Jagged arrays often combine outer initializer with per-row `new int[size]`.
- Collection expression syntax (`[1, 2, 3]`) in newer C# can target arrays and spans in some contexts.
- Initializers run before array reference is published to callers.

---

#### Q10. What is the relationship between arrays and `params` parameters in methods?

What is the relationship between arrays and `params` parameters in methods?

**Answer:** A `params` parameter is syntactic sugar for a single-dimensional array parameter; callers may pass individual arguments that the compiler collects into an array, or pass an array directly.

- See Methods Q2 and Q15.
- Params arrays must be last in the signature—Gotcha 12.
- Only one params parameter per method.
- Prefer `ReadOnlySpan<T>` overloads in performance APIs alongside params for flexibility.

---

#### Q11. What is the difference between shallow copy of an array reference and copying array elements?

What is the difference between shallow copy of an array reference and copying array elements?

**Answer:** Copying the reference aliases the same array object; copying elements into a new array duplicates the slot values—for reference-type elements, shallow copy duplicates references to the same nested objects, not deep clones of those objects.

- `Array.Copy` and `Clone` perform shallow element copy into new array storage.
- Deep copy requires per-element clone logic for mutable reference types.
- Assignment `b = a` shares identity; mutating `b[i]` affects `a[i]`.
- Immutability of elements simplifies reasoning after shallow array copy.

---

#### Q12. When would you use `Array.Sort` vs LINQ `OrderBy` on an array?

When would you use `Array.Sort` vs LINQ `OrderBy` on an array?

**Answer:** `Array.Sort` sorts in place mutating the original array with efficient memory use and optional custom comparer. LINQ `OrderBy` returns a new ordered sequence (often deferred) without requiring in-place mutation, integrating with IEnumerable pipelines.

- Sorting large arrays in memory-critical code often prefers `Array.Sort`.
- Functional style chaining uses `OrderBy` then `ToArray()` if a new array is acceptable.
- `Array.Sort` throws if comparer violates contract; stable sort behavior differs from LINQ sort guarantees in some providers.
- Partial sorts and binary search (`Array.BinarySearch`) assume sorted in-place arrays.

---

#### Q13. What bounds-checking behavior does C# provide for array indexing?

What bounds-checking behavior does C# provide for array indexing?

**Answer:** Every array index access is bounds-checked at runtime, throwing `IndexOutOfRangeException` when the index is less than zero or greater than or equal to the length. The Just-In-Time (JIT) compiler may optimize checks when it can prove safety in tight loops.

- Multi-dimensional indexing checks each dimension separately.
- `Span<T>` and `Memory<T>` provide similar checks with potentially better optimization paths.
- Unchecked unsafe pointer access bypasses checks—unsafe context required.
- Off-by-one errors at `Length` index are a common bug source despite checks.

---

#### Q14. What is `Span<T>`/`ReadOnlySpan<T>` in relation to arrays (preview — stack-friendly views)?

What is `Span<T>`/`ReadOnlySpan<T>` in relation to arrays (preview — stack-friendly views)?

**Answer:** `Span<T>` is a stack-only ref struct view over contiguous memory such as arrays, strings (via `ReadOnlySpan<char>`), or stackalloc buffers, providing slice and parse operations without allocating subarrays.

- `array.AsSpan(start, length)` creates a window without copying elements.
- Spans enable modern high-performance APIs (`TryParse` on spans, UTF-8 processing).
- Cannot be stored on heap fields directly due to ref struct restrictions (with exceptions for ref fields in newer versions in limited scenarios).
- Module 03 and performance chapters expand span usage patterns.

---

#### Q15. How do jagged arrays differ in memory layout from rectangular 2D arrays?

How do jagged arrays differ in memory layout from rectangular 2D arrays?

**Answer:** Rectangular arrays allocate one object with all cells in a single block indexed by `[row, col]`. Jagged arrays allocate an outer array of references, each pointing to a separate inner array object that may differ in length, causing more indirection and potential cache misses but saving space for ragged data.

- Rectangular: better locality for dense fixed-size matrices.
- Jagged: flexible row lengths, extra pointer per row.
- Serialization formats may prefer one shape over the other for compatibility.
- Choose based on data shape and access patterns, not syntax preference alone.

---

#### Q16. What happens when you pass an array to a method — can the callee change the caller's array contents?

What happens when you pass an array to a method — can the callee change the caller's array contents?

**Answer:** The callee receives a copy of the reference pointing to the same array object, so mutating elements (`arr[0] = 99`) is visible to the caller. Reassigning the parameter to a new array object does not change which array the caller's variable references unless `ref` is used on the array parameter.

- Length is fixed after creation; callee cannot resize caller's array without `ref` and `Array.Resize` on caller's variable.
- Null assignment to parameter does not null caller's reference.
- Same semantics as reference types generally—see Methods Q10 and Q20.
- Returning a new array is common when transformation changes size.

---

### 10. Exception Handling

#### Q1. Explain exception handling in C# (`try`, `catch`, `finally`, `throw`, and custom exceptions).

Explain exception handling in C# (`try`, `catch`, `finally`, `throw`, and custom exceptions).

**Answer:** `try` wraps code that may fail; `catch` handles specific exception types; `finally` runs cleanup whether or not an exception occurred; `throw` signals failure up the stack; custom exceptions derive from `Exception` to express domain-specific errors with context.

- Catch most specific types first; general `Exception` last if used at all.
- `finally` releases unmanaged resources or resets state; pair with `using` for `IDisposable`.
- Custom exceptions should be `[Serializable]` when remoting legacy scenarios matter and include useful messages, not control flow.
- Unhandled exceptions terminate the process in console apps unless a host catches them.

---

#### Q2. What is the difference between `throw` and `throw ex`?

What is the difference between `throw` and `throw ex`?

**Answer:** `throw;` inside a catch block rethrows the same exception object and preserves the original stack trace, while `throw ex;` throws a new exception reference that resets the stack trace to the current catch location, hiding where the failure originally occurred.

- Use `throw;` after logging to keep diagnostics intact—Gotcha 6.
- Wrap with `throw new CustomException("...", ex)` when adding context and passing inner exception explicitly.
- Never use bare `throw ex;` unless you intentionally want a fresh stack (rare).
- See examples.md minimal code sample for the pattern.

---

#### Q3. Explain the `using` statement in the context of exception handling and resource management.

Explain the `using` statement in the context of exception handling and resource management.

**Answer:** The `using` statement compiles to try/finally that calls `Dispose()` on `IDisposable` objects when leaving scope, even if an exception occurs, ensuring files, connections, and handles release promptly instead of waiting for garbage collection.

- `using var stream = File.OpenRead(path);` disposes at end of enclosing block (using declaration).
- Nested `using` statements dispose in reverse order of acquisition.
- `Dispose` should not throw; implementers swallow secondary errors when possible.
- `using` does not catch exceptions—it guarantees disposal on exceptional paths.

---

#### Q4. What are exception filters in C#?

What are exception filters in C#?

**Answer:** Exception filters are `when` clauses on `catch` statements that run a boolean expression after matching the exception type but before entering the catch block, allowing conditional handling without catching and rethrowing to inspect state.

- `catch (Exception ex) when (ex.HResult == specificCode)` handles only matching cases.
- Filters must not throw; throwing converts to `FailedExceptionFilterException` wrapping the filter failure.
- Useful for logging correlation without losing stack via catch-and-rethrow patterns.
- Overuse complicates control flow; prefer typed exceptions when possible.

---

#### Q5. What is the difference between catching a specific exception type vs `catch (Exception)`?

What is the difference between catching a specific exception type vs `catch (Exception)`?

**Answer:** Catching specific types (`FormatException`, `IOException`) handles anticipated failures with targeted recovery, while catching `Exception` intercepts all managed exceptions including unexpected bugs, which often hides defects unless you rethrow after logging.

- Broad catch is acceptable at process boundaries (top-level handler) or when translating to user-safe messages before rethrow.
- Catch derived before base; unreachable catch blocks are compile errors.
- Filtering with `when` narrows broad catches without separate rethrow gymnastics.
- ASP.NET Core middleware often maps exception types to HTTP status codes selectively.

---

#### Q6. What happens if an exception is thrown inside a `finally` block?

What happens if an exception is thrown inside a `finally` block?

**Answer:** If `finally` throws while unwinding from an earlier exception, the new exception typically replaces the original active exception (or is chained depending on runtime/version rules), and the original failure information may be lost unless captured in an inner exception or logged first.

- Avoid throwing from `finally`; log and swallow secondary failures during cleanup when primary error matters more.
- `Dispose` implementations should not throw if avoidable for this reason.
- Return statements in `finally` similarly override try outcomes—Gotcha 7.
- Design cleanup code defensively with try/catch inside finally for non-critical steps.

---

#### Q7. What is the base class hierarchy for exceptions in .NET (`Exception`, `SystemException`, application-specific types)?

What is the base class hierarchy for exceptions in .NET (`Exception`, `SystemException`, application-specific types)?

**Answer:** All exceptions derive from `Exception`. Many BCL runtime errors derive from `SystemException` (historical distinction for system vs application). Application code typically throws `ApplicationException` subclasses or domain-specific types directly inheriting `Exception` or intermediate bases like `InvalidOperationException`.

- `ArgumentException`, `InvalidOperationException`, and `NotSupportedException` are common BCL bases for APIs.
- Do not catch `StackOverflowException` or `OutOfMemoryException` for recovery in most cases.
- `AggregateException` wraps multiple failures from parallel tasks.
- Custom hierarchies should be shallow and meaningful to callers and middleware.

---

#### Q8. When should you create a custom exception type vs using an existing BCL exception?

When should you create a custom exception type vs using an existing BCL exception?

**Answer:** Use existing BCL exceptions when the failure mode matches their documented semantics (`ArgumentNullException` for null args, `InvalidOperationException` for wrong object state). Create custom types when callers need to distinguish domain failures programmatically or attach structured data not expressible in message alone.

- Custom exceptions need meaningful names ending in `Exception` and optional custom properties (ErrorCode, EntityId).
- Avoid deep exception inheritance trees rarely caught at different levels.
- Prefer standard types in public libraries to reduce consumer catch proliferation.
- Document which exceptions public methods throw in XML docs for API consumers.

---

#### Q9. What is the difference between `using` statement and `using` declaration (`using var`) for disposal?

What is the difference between `using` statement and `using` declaration (`using var`) for disposal?

**Answer:** Classic `using (var r = ...) { }` creates an explicit block scope ending with dispose at the closing brace. `using var r = ...;` disposes at the end of the enclosing scope (method or block), reducing nesting while preserving dispose-on-exit semantics.

- Both compile to equivalent dispose patterns with try/finally.
- `using var` disposal order at method end is reverse declaration order.
- Choose block form when dispose boundary is narrower than the whole method.
- Neither replaces `IAsyncDisposable` async using patterns for async disposal (`await using`).

---

#### Q10. Can you have multiple `catch` blocks — what is the order rule for catching derived vs base exceptions?

Can you have multiple `catch` blocks — what is the order rule for catching derived vs base exceptions?

**Answer:** Multiple catch blocks are allowed for disjoint types; the compiler requires more specific types before less specific bases because the first matching catch handles the exception. A derived catch after a base catch for the same hierarchy is unreachable and errors at compile time.

- Only one catch executes per thrown exception.
- Exception filters can skip a catch block even when type matches, allowing fall-through to later catches in some designs—use carefully.
- Empty catch blocks swallow errors—avoid except at intentional boundaries with logging.
- Rethrow with `throw;` after partial handling to let upstream catch broader policy.

---

#### Q11. What is `finally` guaranteed to do, and can it prevent an exception from propagating?

What is `finally` guaranteed to do, and can it prevent an exception from propagating?

**Answer:** `finally` runs when control leaves the associated try/catch via normal completion, exception, or return, making it suitable for cleanup. A `return` or uncaught exception thrown inside `finally` can override or replace pending exceptions from try, effectively changing propagation—see Gotcha 7.

- `finally` does not suppress try exceptions unless it completes normally without throwing and without return override quirks.
- Cleanup in finally should be idempotent when possible.
- Thread abort and process kill can skip finally in catastrophic scenarios—design critical durability with `try/finally` plus persistent state.
- Async finally in async methods runs as part of async state machine completion.

---

#### Q12. What is the difference between handled exceptions and unhandled exceptions in a console vs ASP.NET host?

What is the difference between handled exceptions and unhandled exceptions in a console vs ASP.NET host?

**Answer:** Handled exceptions are caught by application catch blocks that recover or translate errors without terminating the process. Unhandled exceptions propagate until the runtime or host default handler runs— crashing console apps or returning HTTP 500 in ASP.NET Core developer/production exception middleware.

- Top-level `AppDomain.UnhandledException` and `TaskScheduler.UnobservedTaskException` are last-chance hooks.
- ASP.NET Core maps unhandled exceptions to responses via exception handler middleware and logging.
- Catching at boundary and returning exit codes is console best practice for CLI tools.
- Logging handled exceptions still matters for observability even when user sees friendly message.

---

#### Q13. When is it appropriate to catch and swallow an exception vs rethrow?

When is it appropriate to catch and swallow an exception vs rethrow?

**Answer:** Swallow only when the failure is fully handled and documented (retry succeeded, optional feature unavailable) and logging captures context for diagnostics. Rethrow when callers must react, transaction must abort, or you lack authority to decide recovery—use `throw;` to preserve stack.

- Empty catch is a code smell unless idempotent probe operations (try read optional config).
- Catch-log-rethrow at layer boundaries preserves observability without losing stack via `throw;`.
- Do not swallow `OutOfMemoryException` hoping to continue reliably.
- Try-pattern preferred over catch for expected parse failures—Gotcha 10.

---

#### Q14. What is `ExceptionDispatchInfo`, and when is `throw;` insufficient?

What is `ExceptionDispatchInfo`, and when is `throw;` insufficient?

**Answer:** `ExceptionDispatchInfo.Capture(ex)` stores an exception's stack for rethrow on another thread with `Throw()`, preserving the original stack in scenarios where bare `throw;` cannot cross async or thread boundaries cleanly.

- Useful when marshaling failures from background threads to request threads in advanced patterns.
- `throw;` only preserves stack when rethrowing on the same logical catch stack frame.
- `AggregateException` on tasks may flatten inner exceptions for reporting.
- Most application code never needs `ExceptionDispatchInfo`; know it for library and parallel code reviews.

---

#### Q15. What happens if both `try` and `finally` contain `return` statements?

What happens if both `try` and `finally` contain `return` statements?

**Answer:** The `finally` block executes before the method actually returns, and if `finally` contains its own `return`, that return value typically overrides the `try` return value, producing surprising results—Gotcha 7.

- Avoid `return` in `finally` in handwritten code.
- Same interaction applies when `try` throws but `finally` returns, potentially swallowing the exception.
- Refactoring to local result variables set in try and returned after finally clarifies intent.
- Control Flow Q15 cross-references this behavior.

---

#### Q16. What is the difference between `IDisposable.Dispose` and finalizers in exception-safe cleanup?

What is the difference between `IDisposable.Dispose` and finalizers in exception-safe cleanup?

**Answer:** `Dispose` runs deterministically when `using` or explicit calls release resources promptly and should not throw. Finalizers (`~ClassName`) run non-deterministically on garbage collection as a last-chance safety net for unmanaged handles, unsuitable for timely release during normal exception flows.

- Always implement `Dispose` for unmanaged resources; suppress finalizer when dispose succeeds (`GC.SuppressFinalize`).
- Finalizers delay object collection and add GC overhead—Module 02 covers IDisposable vs finalizer depth.
- Exception during dispose should not mask original exception from try without careful logging.
- `SafeHandle` encapsulates critical finalization patterns for native resources.

---

### Gotchas — Module 01

#### Gotcha 1. String interning

**Answer:** Many candidates assume two strings with the same text always share reference identity, but only interned literals and explicit interning guarantee that; separately constructed equal strings compare equal with `==` yet may fail `ReferenceEquals`.

- Literal `"hello"` assignments often alias; `new string('h', 5)` built at runtime typically does not.
- Rely on `==` or `Equals` for content, not `ReferenceEquals`, unless testing interning explicitly.
- See Strings Q4, Q15, and Q16 for the full interning model.

---

#### Gotcha 2. Integer division

**Answer:** Developers expect `10 / 3` to yield a fractional result, but integer division truncates toward zero when both operands are integral types.

- Promote at least one operand to `double`, `decimal`, or `float` for fractional math.
- See Operators Q4 and Type Conversion Q14 for related numeric rules.
- Financial code should use `decimal` explicitly, not integer division with accidental truncation.

---

#### Gotcha 3. `const` vs runtime values

**Answer:** `const` requires compile-time constants, so values like `DateTime.Now` or computed decimals at runtime cannot be `const`; use `readonly` fields or properties set in constructors instead.

- Callers embedding optional parameter defaults capture const values at compile time—related to Gotcha 13.
- See Data Types Q8, Q9, and Q28.

---

#### Gotcha 4. Boxing silently hurts performance

**Answer:** Assigning value types to `object` or non-generic collections boxes each value on the heap, causing allocations invisible in source that accumulate in hot loops.

- Prefer `List<int>` over `ArrayList` for integers.
- Interface dispatch on structs often boxes—see Module 02 Gotcha 5.
- See Data Types Q4 and Q14.

---

#### Gotcha 5. Modifying a struct inside `foreach`

**Answer:** The foreach iteration variable is a read-only copy of each element, so mutating fields on that copy does not update the collection and fails to compile when you try to assign to the iteration variable itself.

- Use `for` with index, `Span<T>`, or refactor to mutable reference types when in-place updates are required.
- C# 5 fixed closure capture for foreach but not struct mutation rules.

---

#### Gotcha 6. `throw;` vs `throw ex;`

**Answer:** Rethrowing with `throw ex;` resets the stack trace to the catch line, destroying diagnostic context; `throw;` preserves the original failure site.

- Wrap with new exception types using inner exceptions when adding context intentionally.
- See Exception Handling Q2.

---

#### Gotcha 7. `return` in `try` vs `finally`

**Answer:** `finally` always runs before the method completes, and a `return` inside `finally` can override the value or exception pending from `try`, producing surprising control flow.

- Never `return` from `finally` in application code.
- See Control Flow Q15 and Exception Handling Q11 and Q15.

---

#### Gotcha 8. Array covariance trap

**Answer:** Assigning `string[]` to `object[]` compiles due to covariance, but storing an incompatible element like `42` throws `ArrayTypeMismatchException` at runtime on write.

- Covariance is read-safe in many scenarios; writes are the trap.
- See Arrays Q6.

---

#### Gotcha 9. Culture-sensitive parse/format

**Answer:** The same string `"3,14"` parses as 314 or 3.14 depending on whether comma is a decimal or thousands separator under the active culture, breaking logs and APIs shared across locales.

- Use `InvariantCulture` for stored and transmitted formats; use explicit culture for localized UI input.
- See Input & Output Q5–Q7 and Type Conversion Q10.

---

#### Gotcha 10. `Parse` vs `TryParse` in user input paths

**Answer:** `int.Parse` on invalid console input throws and can crash the app; `TryParse` treats failure as a normal branch suitable for reprompt loops.

- Exceptions are for exceptional conditions, not expected typos.
- See Input & Output Q3 and Q14 and Methods Q13.

---

#### Gotcha 11. `ref` reassignment vs mutation

**Answer:** Reassigning a reference parameter to a new object does not change the caller's variable unless the parameter is `ref`; mutating fields on the shared object does affect the caller.

- See Methods Q10 and Q20.

---

#### Gotcha 12. `params` must be last

**Answer:** Only one `params` array parameter is permitted and it must be the final parameter in the method signature; violating this is a compile error.

- See Methods Q2 and Q14.

---

#### Gotcha 13. Optional parameter defaults are compile-time

**Answer:** Default argument values are baked into call sites at compile time, so changing a default in the method definition does not affect callers until they recompile.

- Prefer overloads or mandatory parameters for breaking default changes in public APIs.
- See Methods Q4 and Q14.

---

#### Gotcha 14. `checked` default is context-dependent

**Answer:** Integer arithmetic wraps silently in unchecked default contexts; financial or checksum code may need explicit `checked` blocks or project settings to throw on overflow instead.

- See Operators Q2 and Q13.

---

#### Gotcha 15. Console encoding mismatch

**Answer:** Writing Unicode text when `Console.OutputEncoding` and the terminal code page disagree produces replacement characters or mojibake, especially on Windows consoles not configured for UTF-8.

- Align console, process, and font encoding for international output.
- See Input & Output Q9.

---

---

## Scenario-Based Answers (Karat Format)

---

#### Q17. **String interning** — `string a = "hello"; string b = "hello"; a == b` is `true`, but two separately constructed strings may not be reference-equal even when content matches.

_Answer not found._

---

#### Q18. **Integer division** — `10 / 3` is `3`, not `3.33`. At least one operand must be floating-point for fractional results.

_Answer not found._

---

#### Q19. **`const` vs runtime values** — You cannot use `const` with a value that requires computation (e.g., `DateTime.Now`); use `readonly` or a property instead.

_Answer not found._

---

#### Q20. **Boxing silently hurts performance** — Assigning value types to `object` or non-generic collections causes heap allocations; repeated boxing in hot paths is a common production issue.

_Answer not found._

---

#### Q21. **Modifying a struct inside `foreach`** — Compile error: the iteration variable is a copy. Use a `for` loop with index or `ref`/`Span` patterns.

_Answer not found._

---

#### Q22. **`throw;` vs `throw ex;`** — `throw ex;` resets the stack trace; `throw;` preserves the original.

_Answer not found._

---

#### Q23. **`return` in `try` vs `finally`** — `finally` always runs before the method actually returns; a `return` in `finally` can override the `try` return value.

_Answer not found._

---

#### Q24. **Array covariance trap** — `object[] arr = new string[3]; arr[0] = 42;` compiles but throws `ArrayTypeMismatchException` at runtime.

_Answer not found._

---

#### Q25. **Culture-sensitive parse/format** — `"3,14"` parses as 314 in `en-US` but as 3.14 in `de-DE`; logs and APIs should use `InvariantCulture` when format must be fixed.

_Answer not found._

---

#### Q26. **`Parse` vs `TryParse` in user input paths** — `int.Parse` on bad console input crashes the app; Try-pattern avoids exceptions for expected failure.

_Answer not found._

---

#### Q27. **`ref` reassignment vs mutation** — Reassigning a reference parameter does not change the caller's variable; mutating the object it points to does.

_Answer not found._

---

#### Q28. **`params` must be last** — Only one `params` array parameter is allowed, and it must be the final parameter in the signature.

_Answer not found._

---

#### Q29. **Optional parameter defaults are compile-time** — Changing a default value in a method signature does not update callers compiled against the old default unless recompiled.

_Answer not found._

---

#### Q30. **`checked` default is context-dependent** — Integer overflow wraps silently in unchecked default contexts; financial code may need explicit `checked` blocks.

_Answer not found._

---

#### Q31. **Console encoding mismatch** — Writing Unicode to a console whose output encoding is not UTF-8 can display replacement characters or mojibake on Windows.

_Answer not found._

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) After a merge, `dotnet build` fails with CS0017 ("Program has more than one entry point defined"). Review these two files in the same console project. What conflicted, and how do you fix it?

```csharp
// Program.cs
Console.WriteLine("Starting batch export...");
RunExport();

static void RunExport() => Console.WriteLine("Export complete.");
```

```csharp
// LegacyMain.cs
namespace HelloWorld;

public class LegacyMain
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello from classic Main!");
    }
}
```

---

**Answer:**

**Answer:** The project defines two entry points — the compiler-generated `Main` from top-level statements in `Program.cs` and the explicit `LegacyMain.Main` — so the build cannot choose a single startup method.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Top-level statements **and** explicit `static Main` in one Exe project | CS0017 — build blocked |
| Structure | Two startup stories in one `OutputType` Exe project | Unclear which code runs even if one were removed manually |
| Merge hygiene | Legacy file left alongside modernized entry point | CI fails after merge; local dev blocked |

**Fix (priority order):**

1. Keep **one** entry style per executable project — either migrate fully to top-level statements **or** delete top-level code and keep `public static void Main(string[] args)`.
2. Remove or exclude `LegacyMain.cs` from the build if it was leftover from migration (`<Compile Remove="LegacyMain.cs" />` only if the file must stay in repo for history).
3. If both patterns are needed for teaching, split into two projects in the solution — each with its own `.csproj` and single entry point.
4. Run `dotnet build` in CI to catch CS0017 before deploy.

**Production takeaway:** Entry-point conflicts are compile-time, but they often appear only after merges — Karat uses this to test whether you know top-level statements still synthesize a hidden `Main`. See foundation **Hello World** — CS0017 gotcha.

---

---

#### Q2. (R) A developer copies a startup snippet into this repo's HelloWorld project (`ImplicitUsings` disabled). Build fails. What is wrong, and what would you change?

```csharp
namespace HelloWorld;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine($"Job started at {DateTime.UtcNow:O}");
        Console.Error.WriteLine("Config loaded.");
    }
}
```

---

**Answer:**

**Answer:** Without `using System;`, unqualified `Console`, `DateTime`, and related BCL types do not resolve — this project disables `ImplicitUsings`, so every namespace import must appear explicitly in source.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Missing `using System;` | CS0103 — `Console` / `DateTime` not in scope |
| Project assumptions | Snippet assumes SDK implicit usings (`enable`) | Copy-paste from ASP.NET/template projects breaks in explicit-usings repos |
| Output streams | Mix of `Console.WriteLine` and `Console.Error` | Correct pattern for stderr, but both require `System` |

**Fix (priority order):**

1. Add `using System;` at the top of the file (minimal fix matching this chapter's style).
2. Alternatively enable `<ImplicitUsings>enable</ImplicitUsings>` **only** if the team standard allows — understand what the SDK injects via `GlobalUsings.g.cs`.
3. Use fully qualified names (`System.Console.WriteLine`) only sparingly — usings are preferred for readability.
4. Document in README or `.editorconfig` that tutorial projects keep implicit usings disabled so learners see imports.

**Production takeaway:** Implicit usings hide `using` lines — production teams should know what's generated vs explicit, especially when onboarding devs from template-heavy IDEs. See **Program.cs** Section 3 — ImplicitUsings disabled in this repo.

---

---

#### Q3. (R) A deployment script runs the published console tool with no arguments:

```bash
dotnet HelloWorld.dll
```

The program crashes on startup. Review `Main`:

```csharp
public static void Main(string[] args)
{
    var environment = args[0];
    var connectionString = args[1];
    Console.WriteLine($"Running in {environment} using {connectionString[..8]}…");
}
```

What breaks, and how would you harden this for CI and production invocation?

---

**Answer:**

```bash
dotnet HelloWorld.dll
```

The program crashes on startup. Review `Main`:

```csharp
public static void Main(string[] args)
{
    var environment = args[0];
    var connectionString = args[1];
    Console.WriteLine($"Running in {environment} using {connectionString[..8]}…");
}
```

What breaks, and how would you harden this for CI and production invocation?

**Answer:** Indexing `args[0]` and `args[1]` without checking `args.Length` throws `IndexOutOfRangeException` when the script omits arguments — the process exits with an unhandled exception instead of a actionable usage message.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Unguarded `args[0]` / `args[1]` | Crash on `dotnet HelloWorld.dll` with zero args |
| Operability | No usage/help text on bad invocation | CI and on-call see stack trace instead of "expected: environment connectionString" |
| Security / logging | Substring of connection string to stdout | May leak secrets into log aggregators even when truncated |

**Fix (priority order):**

1. Validate `args.Length >= 2`; print usage to `Console.Error` and exit with non-zero code (e.g., `Environment.Exit(1)`) when invalid.
2. Prefer **options parsing** (`System.CommandLine`, environment variables, or `IConfiguration` for tools) over positional-only args for production CLIs.
3. Never log connection strings — log environment name only; load secrets from vault/env.
4. Add a CI smoke test that runs the tool with required args and one test that asserts graceful failure with `--help` or missing args.

```csharp
public static void Main(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: HelloWorld <environment> <connectionString>");
        Environment.Exit(1);
    }
    // ...
}
```

**Production takeaway:** Hello World introduces `string[] args` — Karat extends it to operational failure when schedulers invoke tools without the same args developers use locally. See **Program.cs** Section 11 — command-line arguments preview.

---

---

#### Q4. (P) A containerized .NET 8 worker uses only `Console.Write` (no newline) for progress dots during a long loop. Locally you see live output; in Kubernetes logs appear only after the process exits or crashes. Explain why and what you would change.

---

**Answer:**

**Answer:** Standard output in non-interactive containers is often fully buffered when not attached to a TTY, so `Console.Write` without newlines sits in a buffer until flush, process exit, or enough data accumulates — making the job look hung in `kubectl logs`.

- **Local vs prod:** Running in a terminal (interactive) typically line-buffers or flushes more aggressively; Kubernetes captures stdout as a pipe/file — block buffering applies.
- **`Write` vs `WriteLine`:** `WriteLine` emits a newline, which commonly triggers a flush; repeated `Write(".")` without `\n` keeps output in the buffer.
- **Fixes:** Use `Console.WriteLine` for progress milestones; call `Console.Out.Flush()` after periodic `Write` updates; prefer structured logging (`ILogger`, Serilog) over raw console in production workers.
- **Better pattern:** Emit JSON log lines or use OpenTelemetry — log aggregators expect line-delimited records, not interactive progress dots.
- **Docker/K8s:** Set `DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION` only for colors; buffering is about pipe vs TTY — run with `docker run -t` locally to reproduce, but design for non-TTY.

**Production takeaway:** Console I/O taught in Hello World behaves differently in containers — production CLIs and workers should use logging abstractions and line-delimited output, not assume an interactive console.

---

---

#### Q5. (D) Your team maintains internal CLI tools and tutorial projects. Some use **top-level statements**, others use explicit `namespace` + `class Program` + `Main` (as in this chapter). What convention would you recommend for production CLIs vs learning repos, and why?

---

---

### 02. Data Types & Variables - Done

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/02. Data Types & Variables - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Use explicit `Program` + `Main` (or a structured `Host`/`Main` that delegates immediately) for production CLIs where testability, clear entry-point discovery, and consistent review matter; allow top-level statements in small scripts and spikes, but keep learning repos explicit until the skeleton is familiar.

**Production CLIs:**

- Explicit entry point makes `args` handling, exit codes, and DI bootstrap (`Host.CreateApplicationBuilder`) visible in code review.
- Easier to attach XML docs, link to analyzer rules, and navigate in large solutions — no hidden compiler-generated `Program` class.
- Test hosts can invoke `Program.Main(args)` or extract logic into testable services called from a thin `Main`.

**Top-level statements — acceptable when:**

- Single-file utilities, prototypes, or `dotnet tool` templates where brevity wins and lifetime is short.
- Team documents that top-level files must stay under N lines and delegate to classes in other files.

**Learning repos (this chapter):**

- Keep `ImplicitUsings` disabled and explicit `namespace HelloWorld;` + `class Program` + `Main` so every layer (using → namespace → class → entry → output) is visible — matches **Program.cs** Sections 2–6.

**Production takeaway:** The choice is maintainability and team clarity, not correctness — both compile to the same IL entry point; Karat tests whether you can justify conventions for operators vs beginners.

---

---

### 02. Data Types & Variables - Done

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/02. Data Types & Variables - Done`

---

---

#### Q1. (R) Finance QA reports order totals off by one cent on some invoices. Review this pricing helper copied from a prototype:

```csharp
public decimal CalculateOrderTotal(double unitPrice, int quantity)
{
    double subtotal = unitPrice * quantity;
    double tax = subtotal * 0.18;
    return (decimal)(subtotal + tax);
}
```

A developer says casting the final result to `decimal` fixes binary rounding. What is wrong, and what would you change?

---

**Answer:**

```csharp
public decimal CalculateOrderTotal(double unitPrice, int quantity)
{
    double subtotal = unitPrice * quantity;
    double tax = subtotal * 0.18;
    return (decimal)(subtotal + tax);
}
```

A developer says casting the final result to `decimal` fixes binary rounding. What is wrong, and what would you change?

**Answer:** All arithmetic runs in `double` (binary floating-point), so rounding errors appear **before** the final cast — converting to `decimal` at the end only changes the display type, not the already-corrupted intermediate values. Money paths should use `decimal` (and `decimal` literals with `m`) from the first operand.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `double` used for unit price, subtotal, and tax | Cent-level drift on totals (e.g., `0.1 + 0.2` binary artifacts) |
| Design | Final `(decimal)` cast masks bad intermediate math | QA sees intermittent penny mismatches vs ledger |
| API contract | `double unitPrice` invites callers to pass JSON `number` doubles | Errors propagate from deserialization through billing |

**Fix (priority order):**

1. Change parameters and locals to `decimal` — e.g., `decimal unitPrice`, `decimal subtotal = unitPrice * quantity`, `const decimal TaxRate = 0.18m`.
2. Ensure literals use `m` suffix (`0.18m`, not `0.18`) so the compiler picks `decimal` arithmetic.
3. Parse inbound prices with `decimal.Parse` / `GetDecimal()` — never `GetDouble()` for currency fields.
4. Add regression tests for known edge cases (`0.1m + 0.2m`, quantities × repeating decimals).

```csharp
public decimal CalculateOrderTotal(decimal unitPrice, int quantity)
{
    decimal subtotal = unitPrice * quantity;
    decimal tax = subtotal * 0.18m;
    return subtotal + tax;
}
```

**Production takeaway:** This chapter's Section 11 shows `(double)0.1 + (double)0.2` vs `0.1m + 0.2m` — Karat tests whether you catch **where** the type matters, not just whether you can name `decimal`. See foundation **Data Types** — float/double vs decimal gotcha.

---

---

#### Q2. (R) A loyalty API returns `int?` for optional points. After deploy, `NullReferenceException` and `InvalidOperationException` appear in logs. Review:

```csharp
public int ComputeBonus(int? loyaltyPoints, string? tierCode)
{
    var bonus = loyaltyPoints.Value * 2;
    if (tierCode.Equals("Gold", StringComparison.OrdinalIgnoreCase))
        bonus += 50;
    return bonus;
}
```

What breaks in production, and how would you harden this method?

---

**Answer:**

```csharp
public int ComputeBonus(int? loyaltyPoints, string? tierCode)
{
    var bonus = loyaltyPoints.Value * 2;
    if (tierCode.Equals("Gold", StringComparison.OrdinalIgnoreCase))
        bonus += 50;
    return bonus;
}
```

What breaks in production, and how would you harden this method?

**Answer:** `loyaltyPoints.Value` throws `InvalidOperationException` when the nullable has no value, and `tierCode.Equals(...)` throws `NullReferenceException` when `tierCode` is null — both are realistic for optional API fields that clients omit or send as JSON `null`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Unguarded `.Value` on `int?` | `InvalidOperationException` — "Nullable object must have a value" |
| Runtime | Instance call on nullable `string?` | `NullReferenceException` when tier is absent |
| API / NRT | Ignores nullable annotations at boundaries | Crashes instead of treating missing data as zero bonus / non-Gold |

**Fix (priority order):**

1. Replace `.Value` with null-coalescing or `HasValue` check — e.g., `int points = loyaltyPoints ?? 0;`.
2. Use null-safe comparison — `string.Equals(tierCode, "Gold", StringComparison.OrdinalIgnoreCase)` (static overload handles null tier as non-match).
3. Enable `<Nullable>enable</Nullable>` and fix CS8602 warnings at compile time rather than suppressing with `!`.
4. Document contract: null points → 0 bonus; null/unknown tier → base bonus only.

```csharp
public int ComputeBonus(int? loyaltyPoints, string? tierCode)
{
    var bonus = (loyaltyPoints ?? 0) * 2;
    if (string.Equals(tierCode, "Gold", StringComparison.OrdinalIgnoreCase))
        bonus += 50;
    return bonus;
}
```

**Production takeaway:** Section 19–20 cover `int?`, `??`, and `string?` — production code must treat null as data, not as exceptional. See foundation **Nullable value types** and **NRT** gotchas.

---

---

#### Q3. (R) A metrics exporter builds a snapshot list for a dashboard. Under load, Gen2 collections spike. Review:

```csharp
public IReadOnlyList<object> BuildDailyCounts(IEnumerable<int> orderCounts)
{
    var snapshot = new List<object>();
    foreach (var count in orderCounts)
        snapshot.Add(count);
    return snapshot;
}
```

What is the performance issue, and what type change fixes it without changing call-site semantics?

---

**Answer:**

```csharp
public IReadOnlyList<object> BuildDailyCounts(IEnumerable<int> orderCounts)
{
    var snapshot = new List<object>();
    foreach (var count in orderCounts)
        snapshot.Add(count);
    return snapshot;
}
```

What is the performance issue, and what type change fixes it without changing call-site semantics?

**Answer:** Each `int` added to `List<object>` is **boxed** — a heap allocation and copy wrapper per value — which creates massive allocation pressure when `orderCounts` is large, driving frequent GC Gen2 collections under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Implicit boxing `int` → `object` on every `Add` | O(n) heap allocations; GC pauses in exporters |
| Design | `List<object>` where homogeneous ints suffice | Hides type intent; invites further boxing/unboxing downstream |
| Runtime | Unbox casts if consumers read values back | Extra CPU + `InvalidCastException` risk on bad casts |

**Fix (priority order):**

1. Change to `List<int>` (or `int[]` if size is known) — no boxing for value-type elements.
2. Return `IReadOnlyList<int>` so callers get a typed, allocation-efficient snapshot.
3. If polymorphism is truly required, document why and pool/reuse buffers; prefer generics over `object`.
4. Profile with `dotnet-counters` (GC heap size, allocation rate) before/after.

```csharp
public IReadOnlyList<int> BuildDailyCounts(IEnumerable<int> orderCounts)
{
    return orderCounts.ToList(); // List<int> — no boxing
}
```

**Production takeaway:** Section 18 previews boxing — Karat extends it to **hot-path allocation** in services. See foundation **Boxing/unboxing** — avoid `object`/`ArrayList` patterns for primitives.

---

---

#### Q4. (P) A warehouse service increments a 32-bit `int transactionId` inside a tight loop processing bulk imports. In staging (small files) IDs look fine; in production one job reports duplicate IDs and negative values after a long run. The team says "C# integers don't overflow in normal use." Explain what happened and what you would use instead.

---

**Answer:**

**Answer:** Integer arithmetic in C# is **unchecked by default**, so when `transactionId` passes `int.MaxValue` it silently wraps to `int.MinValue` (two's complement) — duplicates and negative IDs follow. Staging never reached the boundary; production volume did.

- **Mechanism:** `transactionId++` at `2,147,483,647` becomes `-2,147,483,648` without throwing — see Section 11a `unchecked` wrap behavior.
- **Why staging missed it:** Overflow is volume-dependent; small test files never cross `int.MaxValue`.
- **Fix — type:** Use `long` (or `ulong`) for monotonic counters and IDs expected to grow without bound; SQL `BIGINT` alignment.
- **Fix — guard:** Wrap critical increments in `checked` if overflow must abort the job (`OverflowException`) rather than wrap — appropriate for financial counters or sequence integrity.
- **Fix — architecture:** Prefer database sequences / `Guid` / snowflake IDs for distributed uniqueness instead of in-process `int` counters.
- **Detection:** Add metrics/alerts when IDs approach `int.MaxValue - margin`; integration tests that simulate boundary (not always feasible in CI, but document the limit).

**Production takeaway:** "Normal use" is not a type strategy — choose `long`/`decimal`/nullable types based on domain bounds. See Section 11a **checked/unchecked** and integer range table in this chapter's quick reference.

---

---

#### Q5. (M) A developer models store configuration like the chapter's `StoreConfig` but tries to share a tax rate across all instances from appsettings loaded at startup:

```csharp
public class PricingOptions
{
    public const decimal StandardTaxRate = LoadTaxRateFromConfiguration();

    private static decimal LoadTaxRateFromConfiguration() =>
        decimal.Parse(Environment.GetEnvironmentVariable("TAX_RATE") ?? "0.18");
}
```

Build fails with CS0133. They propose replacing `const` with `static readonly` assigned from a static constructor. Is that sufficient for production DI, and what pattern would you recommend?

---

**Answer:**

```csharp
public class PricingOptions
{
    public const decimal StandardTaxRate = LoadTaxRateFromConfiguration();

    private static decimal LoadTaxRateFromConfiguration() =>
        decimal.Parse(Environment.GetEnvironmentVariable("TAX_RATE") ?? "0.18");
}

```

Build fails with CS0133. They propose replacing `const` with `static readonly` assigned from a static constructor. Is that sufficient for production DI, and what pattern would you recommend?

**Answer:** `const` requires a compile-time constant — runtime config cannot qualify (CS0133). `static readonly` with a static constructor can work for a singleton-like value, but it hides testability, defers failures to type-load time, and bypasses the options/DI patterns ASP.NET Core expects.

- **`const` vs `readonly` (Section 2):** `const` is baked in at compile time; `readonly` fields are set at run time (declaration or ctor) — config values are run-time facts.
- **`static readonly` + static ctor:** Loads env var once when the type is first accessed; hard to mock in tests; parse errors crash type initialization (`TypeInitializationException`).
- **Production pattern:** `IOptions<PricingOptions>` / `IOptionsMonitor<PricingOptions>` bound from `IConfiguration` at startup — reloadable, injectable, validated with `DataAnnotations` or `IValidateOptions<T>`.
- **Instance `readonly`:** Matches `StoreConfig` — per-store values set once in ctor when each store entity is created from DB/config row.
- **Validation:** Use `decimal.Parse` with `CultureInfo.InvariantCulture`; prefer `TryParse` or config binder errors over unhandled `FormatException` on startup.

```csharp
public sealed class PricingOptions
{
    public decimal StandardTaxRate { get; init; }
}

// Program.cs — services.Configure<PricingOptions>(configuration.GetSection("Pricing"));
```

**Production takeaway:** Karat pairs language keywords (`const`/`readonly`) with **how teams actually load config** — static env reads are a step up from `const`, but DI options are the shippable pattern. See **StoreConfig** in this chapter's `Program.cs` for instance-level `readonly`.

---

---

#### Q6. (D) Your API team debates `var` vs explicit types in service-layer code. Two snippets assign the same JSON field:

```csharp
var total = json.GetProperty("orderTotal").GetDecimal();
decimal total = json.GetProperty("orderTotal").GetDecimal();

var orderId = json.GetProperty("orderId").GetInt64();
long orderId = json.GetProperty("orderId").GetInt64();
```

When would you require explicit `decimal` and `long` (as in this chapter's money and `orderId` examples), and when is `var` acceptable?

---

---

### 03. Input & Output - Done

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/03. Input & Output - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
var total = json.GetProperty("orderTotal").GetDecimal();
decimal total = json.GetProperty("orderTotal").GetDecimal();

var orderId = json.GetProperty("orderId").GetInt64();
long orderId = json.GetProperty("orderId").GetInt64();
```

When would you require explicit `decimal` and `long` (as in this chapter's money and `orderId` examples), and when is `var` acceptable?

**Answer:** Require explicit types when the type carries **domain meaning or correctness constraints** — money (`decimal`), large identifiers (`long`), flags, enums — so reviewers and maintainers see intent without inferring from the right-hand side. Use `var` when the type is obvious from a descriptive API (e.g., `var doc = JsonDocument.Parse(...)`) or when the right-hand side is verbose generic code.

**Require explicit types:**

- **Financial fields** — always `decimal total` (Section 11); never `var` that could drift if someone swaps in `GetDouble()`.
- **Large IDs and counters** — `long orderId` documents 64-bit range (Section 10 `orderId` literal with `L` suffix); prevents silent narrowing if API changes.
- **Public API surfaces** — method signatures and DTO properties must show types; no `var` in signatures.
- **Narrowing or ambiguous conversions** — when RHS involves casts, ternary mixes, or custom operators.

**`var` acceptable:**

- Right-hand side clearly names the type: `var store = new StoreConfig("PUN-01", 0.18m)` in local scope (though some teams still prefer explicit for `StoreConfig`).
- LINQ/comprehension where the static type is long (`IEnumerable<...>`) and explicit type clutters.
- `new()` target-typed scenarios where type appears on the left of assignment elsewhere in the statement.

**Team convention:** Allow `var` for locals when the type is **immediate and unambiguous**; disallow `var` for primitives that encode business rules (`decimal`, `long`, `bool` flags, enums like `ShipmentStatus`). Enforce via `.editorconfig` (IDE0008) with exceptions documented.

**Production takeaway:** Section 7 says `var` is still strongly typed — the judgment call is **readability and misuse prevention**, not compiler capability. Karat tests whether you tie type choice to money, ID range, and reviewability like this chapter's examples.

---

---

### 03. Input & Output - Done

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/03. Input & Output - Done`

---

---

#### Q1. (R) A batch pricing tool prompts for quantity over stdin in a CI pipeline (`dotnet run < empty.txt`). Review this handler — what fails at runtime, and how would you fix it?

```csharp
public static void PromptAndPrice(decimal unitPrice)
{
    Console.Write("Enter quantity: ");
    string input = Console.ReadLine();
    int qty = int.Parse(input.Trim());
    decimal total = unitPrice * qty;
    Console.WriteLine("Line total: {0:C2}", total);
}
```

---

**Answer:**

**Answer:** `Console.ReadLine()` returns `null` when stdin is closed or empty, and `int.Parse(null)` throws `ArgumentNullException` — worse, `Parse` on malformed text throws `FormatException`. Piped batch jobs need null-safe reads and non-throwing validation with errors on stderr.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | No null check after `ReadLine()` | `ArgumentNullException` when stdin closes (empty file, Ctrl+D, broken pipe) |
| Runtime | `int.Parse` on user/piped input | `FormatException` on `"3.5"`, `"twelve"`, or blank lines — unhandled crash |
| Operability | Errors go to stdout via `WriteLine` | Pipelines that redirect stdout lose actionable failure text |
| Design | Interactive prompt pattern in non-interactive CI | Job hangs or fails instead of reading quantity from args/env |

**Fix (priority order):**

1. Null-check `ReadLine()`; treat null/whitespace as invalid input, write to `Console.Error`, exit non-zero.
2. Replace `Parse` with `int.TryParse(..., NumberStyles.Integer, CultureInfo.InvariantCulture, out qty)` for piped/batch input.
3. For CI, accept quantity via command-line args or env var — skip `ReadLine` entirely in headless runs.
4. Loop or fail fast with usage text: `"Usage: PricingTool --qty 3"` printed to stderr.

```csharp
string? input = Console.ReadLine();
if (input is null || !int.TryParse(input.Trim(), NumberStyles.Integer,
        CultureInfo.InvariantCulture, out int qty))
{
    Console.Error.WriteLine("Invalid or missing quantity.");
    Environment.Exit(1);
}
```

**Production takeaway:** ReadLine returning `null` is easy to miss in demos that use simulated strings — Karat tests whether you treat stdin as unreliable. See **Program.cs** Section 3 — null when input is closed; Section 8 — prefer TryParse for user input.

---

---

#### Q2. (R) A containerized kiosk app runs with `CultureInfo.CurrentCulture` set to `de-DE`. Operators pipe order files from a US-based ERP. Review the parser:

```csharp
public static bool TryReadQuantity(string line, out int quantity)
{
    return int.TryParse(line.Trim(), out quantity);
}

public static bool TryReadUnitPrice(string line, out decimal price)
{
    return decimal.TryParse(line.Trim(), out price);
}
```

Sample piped line: `"Qty=3, UnitPrice=1.234,56"`. What breaks, and what would you change?

---

**Answer:**

```csharp
public static bool TryReadQuantity(string line, out int quantity)
{
    return int.TryParse(line.Trim(), out quantity);
}

public static bool TryReadUnitPrice(string line, out decimal price)
{
    return decimal.TryParse(line.Trim(), out price);
}
```

Sample piped line: `"Qty=3, UnitPrice=1.234,56"`. What breaks, and what would you change?

**Answer:** Both overloads use `CultureInfo.CurrentCulture` implicitly — under `de-DE`, `decimal.TryParse("1.234,56")` succeeds but `"1,234.56"` fails silently (`false`), so orders are skipped or misread without an exception. The sample line also mixes key-value text with locale-specific decimals, so naive `TryParse` on the whole line never succeeds.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `TryParse` without explicit `IFormatProvider` | Parsing depends on host/container culture — same string passes locally (en-US) and fails in de-DE pod |
| Correctness | No separation of `"Qty=3"` structure from numeric token | Returns `false` for entire line — silent data loss in batch import |
| Operability | `TryParse` failure looks like "bad data" | Operators cannot distinguish format mismatch vs genuinely invalid numbers |
| Design | Piped ERP exports use invariant or fixed locale | CurrentCulture in containers follows image/OS, not data source |

**Fix (priority order):**

1. Parse structured fields first (split on `,`, extract `UnitPrice=` value) — do not pass the whole line to `TryParse`.
2. Pass **`CultureInfo.InvariantCulture`** (or the ERP's known culture) explicitly: `decimal.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out price)`.
3. Log culture name and rejected token to `Console.Error` when parse fails — aids cross-region debugging.
4. Document expected wire format in the CLI `--help`; reject mixed-locale files early with a clear message.

```csharp
decimal.TryParse(priceToken, NumberStyles.Number,
    CultureInfo.InvariantCulture, out price);
```

**Production takeaway:** TryParse "never throws" makes culture bugs invisible — production importers must pin culture to the **data contract**, not the thread. See **Program.cs** Section 5 — InvariantCulture for wire/API; Section 8 — TryParse with `NumberStyles` and provider.

---

---

#### Q3. (R) A developer copies the receipt-capture pattern from this chapter's `CaptureFormattedReceipt` but omits cleanup. Review:

```csharp
public static string CaptureReceipt(string customer, decimal total)
{
    TextWriter original = Console.Out;
    using StringWriter buffer = new StringWriter(CultureInfo.InvariantCulture);
    Console.SetOut(buffer);

    Console.WriteLine("Receipt for {0}", customer);
    Console.WriteLine("Total: {0:C2}", total);
    return buffer.ToString();
}
```

After the first call, later `Console.WriteLine` calls in the same process produce no terminal output. Diagnose and fix.

---

**Answer:**

```csharp
public static string CaptureReceipt(string customer, decimal total)
{
    TextWriter original = Console.Out;
    using StringWriter buffer = new StringWriter(CultureInfo.InvariantCulture);
    Console.SetOut(buffer);

    Console.WriteLine("Receipt for {0}", customer);
    Console.WriteLine("Total: {0:C2}", total);
    return buffer.ToString();
}
```

After the first call, later `Console.WriteLine` calls in the same process produce no terminal output. Diagnose and fix.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / I/O | `Console.SetOut(buffer)` never restored | All subsequent stdout goes to discarded `StringWriter` after method returns — terminal appears "dead" |
| Correctness | `StringWriter` disposed while still set as `Console.Out` | Potential `ObjectDisposedException` on later writes depending on timing |
| Maintainability | Missing `try/finally` vs chapter pattern | First capture in a long-lived worker breaks all logging for process lifetime |
| Testing | Tests that capture output may pass once then flake | Order-dependent failures in test suites |

**Fix (priority order):**

1. Wrap body in `try/finally` and call `Console.SetOut(original)` in `finally` — match **Program.cs** Section 2a.
2. Restore **before** returning captured text (finally runs before return — safe).
3. Prefer injecting `TextWriter` or `ILogger` instead of mutating global `Console.Out` in production services.
4. Add a test that calls capture twice and asserts terminal output still works.

```csharp
try
{
    Console.SetOut(buffer);
    Console.WriteLine("Receipt for {0}", customer);
    Console.WriteLine("Total: {0:C2}", total);
    return buffer.ToString();
}
finally
{
    Console.SetOut(original);
}
```

**Production takeaway:** Global stream redirection is convenient for unit tests but dangerous in long-running processes — always restore in `finally`. See **Program.cs** `CaptureFormattedReceipt` — save, redirect, restore pattern.

---

---

#### Q4. (P) A .NET 8 worker deployed to Kubernetes reads config lines from stdin and writes a summary CSV to stdout. Ops runs:

```bash
kubectl run job --image=pricing-worker -- sh -c "cat orders.txt | dotnet PricingWorker.dll > /shared/export.csv 2> /shared/errors.log"
```

The job completes, but `export.csv` is empty while `errors.log` contains valid rows formatted as `"SKU,Qty,Total"`. The code mixes streams like this:

```csharp
foreach (var line in ReadLines())
{
    if (!decimal.TryParse(line, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
    {
        Console.WriteLine("Skipping bad line: {0}", line);
        continue;
    }
    Console.WriteLine("{0},{1},{2:F2}", sku, qty, amount);
}
```

Explain what went wrong and how you would structure stdout vs stderr for piped/container runs.

---

**Answer:**

```bash
kubectl run job --image=pricing-worker -- sh -c "cat orders.txt | dotnet PricingWorker.dll > /shared/export.csv 2> /shared/errors.log"
```

The job completes, but `export.csv` is empty while `errors.log` contains valid rows formatted as `"SKU,Qty,Total"`. The code mixes streams like this:

```csharp
foreach (var line in ReadLines())
{
    if (!decimal.TryParse(line, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
    {
        Console.WriteLine("Skipping bad line: {0}", line);
        continue;
    }
    Console.WriteLine("{0},{1},{2:F2}", sku, qty, amount);
}
```

Explain what went wrong and how you would structure stdout vs stderr for piped/container runs.

**Answer:** Shell redirection sends **stdout** (`Console.Out`) to `export.csv` and **stderr** (`Console.Error`) to `errors.log`. Skipped-line diagnostics correctly belong on stderr, but the successful CSV rows also went to stderr — meaning the developer likely used `Console.Error.WriteLine` for data rows (or redirected streams in code). The empty CSV proves machine-readable output must use stdout exclusively; human diagnostics use stderr.

- **Stream contract:** stdout = pipeable payload (CSV, JSON lines); stderr = logs, skip messages, progress — matches **Program.cs** Section 2 (Out vs Error).
- **Likely bug:** Data rows written with `Console.Error.WriteLine` "so errors stand out" during local dev — breaks shell redirection in K8s.
- **Fix:** `Console.WriteLine` (stdout) for `{sku},{qty},{amount:F2}`; `Console.Error.WriteLine` only for `"Skipping bad line"`.
- **Container buffering:** If using `Console.Write` without newlines for progress, logs may batch — use line-delimited records and `Flush()` if needed.
- **Validation:** Integration test that runs `dotnet run < sample.txt > out.csv 2> err.log` and asserts row count in `out.csv` only.

**Production takeaway:** Stdout/stderr separation is a deployment contract — mixing them breaks every `>` / `2>` pipeline and log agents that treat stdout as data. See **Program.cs** Section 2 — redirect examples (`>`, `2>`, `<`).

---

---

#### Q5. (M) An internal CLI formats currency for operators in Mumbai (`en-IN`) but must emit a fixed wire-format total for downstream JSON consumers. Review:

```csharp
decimal orderTotal = 1234567.89m;

Console.WriteLine("Display total: {0:C2}", orderTotal);
Console.WriteLine("Wire total: {0}", orderTotal.ToString("F2"));
File.WriteAllText("payload.json",
    $"{{\"total\":{orderTotal.ToString("F2")}}}");
```

The JSON consumer in `eu-west-1` intermittently rejects payloads. What is the culture bug, and how do you fix display vs wire formatting?

---

**Answer:**

```csharp
decimal orderTotal = 1234567.89m;

Console.WriteLine("Display total: {0:C2}", orderTotal);
Console.WriteLine("Wire total: {0}", orderTotal.ToString("F2"));
File.WriteAllText("payload.json",
    $"{{\"total\":{orderTotal.ToString("F2")}}}");
```

The JSON consumer in `eu-west-1` intermittently rejects payloads. What is the culture bug, and how do you fix display vs wire formatting?

**Answer:** `{0:C2}` correctly uses `CurrentCulture` for human display, but `ToString("F2")` without a provider uses **CurrentCulture** too — under `en-IN` or locales that use `,` as the decimal separator, the JSON file contains `"total":1234567,89`, which is invalid JSON and fails parsers expecting `.` as the decimal point.

- **Display path:** Keep `Console.WriteLine("{0:C2}", orderTotal)` or `ToString("C2", CultureInfo.GetCultureInfo("en-IN"))` for operators.
- **Wire path:** Always pass **`CultureInfo.InvariantCulture`** (or `CultureInfo.GetCultureInfo("en-US")` for numbers) on fixed-format exports: `orderTotal.ToString("F2", CultureInfo.InvariantCulture)`.
- **Safer JSON:** Use `System.Text.Json` serialization instead of manual string interpolation — avoids locale entirely for numeric fields.
- **Thread culture:** If the CLI temporarily sets `CultureInfo.CurrentCulture` for UI (see **Program.cs** Section 5e), wire writes must still pass invariant explicitly — thread culture leaks into `ToString("F2")`.

```csharp
Console.WriteLine("Display total: {0:C2}", orderTotal);
string wire = orderTotal.ToString("F2", CultureInfo.InvariantCulture);
File.WriteAllText("payload.json", $"{{\"total\":{wire}}}");
```

**Production takeaway:** "Invariant for logs and APIs, CurrentCulture for humans" — omitting the provider on `ToString` is one of the most common cross-region production bugs. See **Program.cs** Section 4 — `String.Format(InvariantCulture, …)` for export price; Section 5 — culture comparison table.

---

---

#### Q6. (D) A team building Express-Mart-style kiosk CLIs debates input validation strategy for numeric prompts. Two approaches:

**A — Parse (throws on bad input):**

```csharp
Console.Write("Quantity: ");
int qty = int.Parse(Console.ReadLine()!);
```

**B — TryParse loop (from this chapter's preview):**

```csharp
int qty;
while (!int.TryParse(Console.ReadLine(), NumberStyles.Integer,
    CultureInfo.InvariantCulture, out qty))
{
    Console.Error.WriteLine("Enter a whole number.");
}
```

When would you choose each in production CLIs vs interactive tutorials, and what traps remain in B?

---

---

### 04. Operators & Expressions - Done

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/04. Operators & Expressions - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**A — Parse (throws on bad input):**

```csharp
Console.Write("Quantity: ");
int qty = int.Parse(Console.ReadLine()!);
```

**B — TryParse loop (from this chapter's preview):**

```csharp
int qty;
while (!int.TryParse(Console.ReadLine(), NumberStyles.Integer,
    CultureInfo.InvariantCulture, out qty))
{
    Console.Error.WriteLine("Enter a whole number.");
}
```

When would you choose each in production CLIs vs interactive tutorials, and what traps remain in B?

**Answer:** Use **TryParse loops** (B) for interactive kiosk and operator CLIs where recovery without a stack trace is required; reserve **Parse** (A) only for trusted, pre-validated config (embedded defaults, known-good args) or after schema validation — never on raw `ReadLine()` in production.

**When A is acceptable:**

- Input is already validated (regex, config file, unit test fixture) and failure should crash fast during development.
- Prototype or tutorial code where the instructor wants learners to see exception types — not shipped CLIs.

**When B is required for production:**

- Any human-typed or piped stdin — Parse turns `"3.0"` or empty Enter into an unhandled exception and exit code unrelated to business rules.
- Errors should go to `Console.Error` so stdout stays clean for scripting.

**Traps that remain in B:**

1. **`ReadLine()` returns null** — loop must break or exit when stdin closes; otherwise infinite `"Enter a whole number"` on EOF.
2. **Culture** — B pins InvariantCulture (good for wire-style input); if operators type locale-specific decimals, pass their culture or document integer-only input.
3. **No retry limit** — unattended scripts with bad stdin spin forever; cap retries or fail after N attempts with exit code 1.
4. **Silent `0`** — distinguish parse failure from legitimate zero if business rules care.

**Production takeaway:** The chapter previews TryParse precisely because user input is unreliable — Karat tests prioritization (TryParse + stderr + null + culture) over memorizing Parse signatures. See **Program.cs** Section 8 — TryParse pattern; `DemonstrateParseFailure` — errors on `Console.Error`.

---

---

### 04. Operators & Expressions - Done

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/04. Operators & Expressions - Done`

---

---

#### Q1. (R) QA reports that bulk-order discounts are too high on large orders. Review this pricing helper used in checkout:

```csharp
public static decimal ComputeOrderTotal(decimal lineSubtotal, decimal discountRate)
{
    // discountRate is 0.05m for 5% off
    return lineSubtotal - lineSubtotal * discountRate;
}

public static decimal ComputeOrderTotalWithCap(decimal lineSubtotal, decimal discountRate, decimal maxDiscount)
{
    decimal discount = lineSubtotal * discountRate;
    return lineSubtotal - discount > maxDiscount
        ? lineSubtotal - maxDiscount
        : lineSubtotal - lineSubtotal * discountRate;
}
```

What precedence or grouping bugs do you see, and how would you fix them?

---

**Answer:**

```csharp
public static decimal ComputeOrderTotal(decimal lineSubtotal, decimal discountRate)
{
    // discountRate is 0.05m for 5% off
    return lineSubtotal - lineSubtotal * discountRate;
}

public static decimal ComputeOrderTotalWithCap(decimal lineSubtotal, decimal discountRate, decimal maxDiscount)
{
    decimal discount = lineSubtotal * discountRate;
    return lineSubtotal - discount > maxDiscount
        ? lineSubtotal - maxDiscount
        : lineSubtotal - lineSubtotal * discountRate;
}
```

What precedence or grouping bugs do you see, and how would you fix them?

**Answer:** `ComputeOrderTotal` is correct because `*` binds tighter than `-`, but `ComputeOrderTotalWithCap` compares `lineSubtotal - discount` before applying the cap logic incorrectly — when the uncapped discount exceeds `maxDiscount`, it returns `lineSubtotal - maxDiscount` as if `maxDiscount` were a final total, not a discount ceiling.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `lineSubtotal - discount > maxDiscount ? lineSubtotal - maxDiscount : …` treats `maxDiscount` as a **total cap**, not a max **discount amount** | Customers charged wrong totals when discount should be capped (over- or under-charging) |
| Maintainability | Duplicate `lineSubtotal * discountRate` in ternary branches | Drift risk — one branch updated, other not |
| Precedence | `-` vs `>` grouping is legal but obscures intent without parentheses | Reviewers misread intended math (related to **PrecedenceExamples.DiscountedSubtotal** in this chapter) |

**Fix (priority order):**

1. Compute discount once: `decimal discount = lineSubtotal * discountRate;`
2. Cap the discount, not the total: `discount = Math.Min(discount, maxDiscount);` then `return lineSubtotal - discount;`
3. Add parentheses when mixing `-`, `*`, and comparisons even if precedence is technically correct — `(lineSubtotal - discount)` makes audits faster.
4. Add unit tests for boundary cases: 0%, 5%, rate that exceeds cap, and zero subtotal.

```csharp
public static decimal ComputeOrderTotalWithCap(decimal lineSubtotal, decimal discountRate, decimal maxDiscount)
{
    decimal discount = Math.Min(lineSubtotal * discountRate, maxDiscount);
    return lineSubtotal - discount;
}
```

**Production takeaway:** Precedence makes `a - b * c` mean `a - (b * c)` — Karat tests whether you spot **business-logic grouping** errors, not just missing semicolons. See **PricingRules.ComputeDiscountedTotal** and **PrecedenceExamples** — multiplication before subtraction is correct; ternary + cap semantics often are not.

---

---

#### Q2. (R) A warehouse API returns `404` when a SKU code is missing from the request, but the team expected a default length of `1`. Review:

```csharp
public int ResolveMaxPickSlots(string? skuCode, int warehouseCapacity)
{
    int slots = skuCode?.Length ?? 0 + 1;
    return slots > warehouseCapacity ? warehouseCapacity : slots;
}
```

Callers pass `skuCode: null` and expect `1` slot. What is wrong, and what would you change?

---

**Answer:**

```csharp
public int ResolveMaxPickSlots(string? skuCode, int warehouseCapacity)
{
    int slots = skuCode?.Length ?? 0 + 1;
    return slots > warehouseCapacity ? warehouseCapacity : slots;
}
```

Callers pass `skuCode: null` and expect `1` slot. What is wrong, and what would you change?

**Answer:** `??` binds **looser** than `+`, so the expression parses as `skuCode?.Length ?? (0 + 1)` — when `skuCode` is null, `slots` becomes `1`. When `skuCode` is non-null, `slots` is the string length with **no** `+ 1`. The bug is inconsistent intent: developers often read `?? 0 + 1` as `(?? 0) + 1`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Null path yields `1`; non-null path yields `Length` without increment | Off-by-one pick slots for valid SKUs — wrong capacity errors or over-picking |
| Precedence | `??` lower than additive `+` (see **PrecedenceExamples.NullCoalesceBeforeAddition**) | Silent misread during code review — matches tutorial gotcha exactly |
| API contract | Method name implies "max slots" but magic `+ 1` is undocumented | Callers cannot predict behavior from signature alone |

**Fix (priority order):**

1. Parenthesize intent explicitly: `int slots = (skuCode?.Length ?? 0) + 1;`
2. If default should be `1` when null **and** length when present, document whether `+ 1` applies to both paths.
3. Consider `Math.Max(1, skuCode?.Length ?? 0)` if minimum one slot is a business rule.
4. Add tests: `null → 1`, `"AB" → 3` (or `2` if no increment), and capacity clamp edge cases.

**Production takeaway:** Null-coalescing chains (`preferred ?? backup ?? fallback`) are safe; mixing `??` with `+`, `-`, or comparisons without parentheses is a top Karat trap. See **LabelDefaults.ResolveDisplayLabel** for correct chaining vs **PrecedenceExamples.NullCoalesceBeforeAddition** for the precedence pitfall.

---

---

#### Q3. (R) After a deploy, inventory audit logs show stock decrements even when orders are rejected for insufficient credit. Review the guard:

```csharp
public bool TryReserveAndShip(Order order, InventoryService inventory, CreditService credit)
{
    if (credit.IsApproved(order.CustomerId) & inventory.TryReserve(order.Sku, order.Quantity))
    {
        order.MarkShipped();
        return true;
    }
    return false;
}
```

`CreditService.IsApproved` and `InventoryService.TryReserve` both write audit rows. What breaks compared to `&&`, and how do you fix it?

---

**Answer:**

```csharp
public bool TryReserveAndShip(Order order, InventoryService inventory, CreditService credit)
{
    if (credit.IsApproved(order.CustomerId) & inventory.TryReserve(order.Sku, order.Quantity))
    {
        order.MarkShipped();
        return true;
    }
    return false;
}
```

`CreditService.IsApproved` and `InventoryService.TryReserve` both write audit rows. What breaks compared to `&&`, and how do you fix it?

**Answer:** Bitwise/logical `&` on `bool` operands does **not** short-circuit — both `IsApproved` and `TryReserve` always run, so inventory is reserved even when credit fails. Production code must use `&&` unless both sides must execute.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `&` evaluates both operands always | Stock reserved on failed credit checks — oversell, reconciliation nightmares |
| Side effects | `TryReserve` mutates inventory and audit log unconditionally | Violates "fail fast" — credit gate is cosmetic |
| Operability | Audit trail shows reserve attempts for rejected orders | On-call cannot trust logs for fraud or inventory forensics |

**Fix (priority order):**

1. Replace `&` with `&&` so `TryReserve` runs only when credit is approved.
2. Order cheap, read-only checks first: `if (!credit.IsApproved(...)) return false;` then reserve — or keep single `&&` with approval on the left.
3. Split **validate → mutate → commit** phases; do not combine side-effecting calls inside a boolean expression.
4. Add integration test: credit denied → inventory unchanged, zero reserve audit entries.

**Production takeaway:** `&&`/`||` short-circuit; `&`/`|` on bool do not — **ShippingRules.NonShortCircuitDemo** in this chapter demonstrates the counter difference. Karat embeds the typo (`&` vs `&&`) in realistic service code; always ask whether the right-hand side has side effects.

---

---

#### Q4. (R) A nightly batch job silently wraps negative stock counts after a bad import. Review:

```csharp
public int ComputeRemainingUnits(int shippedCases, int unitsPerCase, int startingUnits)
{
    int totalShipped = shippedCases * unitsPerCase;
    return startingUnits - totalShipped;
}

// Called from batch:
int remaining = ComputeRemainingUnits(
    shippedCases: 500_000,
    unitsPerCase: 10_000,
    startingUnits: 1_000_000);
```

On a 32-bit host the value becomes positive when it should be deeply negative. What operator/context issue is this, and how would you harden it?

---

**Answer:**

```csharp
public int ComputeRemainingUnits(int shippedCases, int unitsPerCase, int startingUnits)
{
    int totalShipped = shippedCases * unitsPerCase;
    return startingUnits - totalShipped;
}

// Called from batch:
int remaining = ComputeRemainingUnits(
    shippedCases: 500_000,
    unitsPerCase: 10_000,
    startingUnits: 1_000_000);
```

On a 32-bit host the value becomes positive when it should be deeply negative. What operator/context issue is this, and how would you harden it?

**Answer:** Integer multiplication and subtraction run in the default **unchecked** context — when `shippedCases * unitsPerCase` exceeds `int.MaxValue`, the product wraps; subtracting a wrapped value from `startingUnits` yields a plausible-looking positive `remaining` instead of signaling overflow.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Unchecked `int` overflow on `*` | Wrong remaining stock — silent data corruption in batch reports |
| Context | No `checked` block or `checked` project setting | Overflow throws nowhere; negative economics look like surplus |
| Domain | `int` may be too narrow for warehouse-scale counts | Latent bug appears only under production volumes |

**Fix (priority order):**

1. Use `checked` for the multiply (or enable `<CheckForOverflowUnderflow>true</CheckForOverflowUnderflow>` in debug builds): `int totalShipped = checked(shippedCases * unitsPerCase);`
2. Promote to `long` or `decimal` for intermediate math: `(long)shippedCases * unitsPerCase` before compare/subtract.
3. Validate inputs up front — reject negative or absurd magnitudes before arithmetic.
4. Fail the batch on overflow (`OverflowException`) rather than persisting wrapped values; alert ops.

```csharp
public int ComputeRemainingUnits(int shippedCases, int unitsPerCase, int startingUnits)
{
    long totalShipped = (long)shippedCases * unitsPerCase;
    long remaining = startingUnits - totalShipped;
    if (remaining > int.MaxValue || remaining < int.MinValue)
        throw new OverflowException("Remaining units out of int range.");
    return (int)remaining;
}
```

**Production takeaway:** C# arithmetic overflows wrap by default — unlike SQL or decimal math. See foundation **Operators** gotcha on integer division; Karat extends to **checked** context for inventory and financial quantities. Prefer `decimal` for money (**OrderArithmetic.ComputeLineSubtotal**); use `checked` or wider types for counts that can exceed 2³¹.

---

---

#### Q5. (R) A code review flags this permission-update endpoint copied from an internal admin tool. Identify the compound-assignment and operator issues:

```csharp
public int UpdatePickerRole(int currentRole, bool grantAdmin, bool revokeWrite)
{
    if (grantAdmin)
        currentRole |= PermissionFlags.Admin;

    if (revokeWrite)
        currentRole ^= PermissionFlags.Write;  // "remove write" per ticket

    currentRole /= 2;  // "normalize" role mask after shift-left in legacy import

    return currentRole;
}
```

What would you change before merging?

---

**Answer:**

```csharp
public int UpdatePickerRole(int currentRole, bool grantAdmin, bool revokeWrite)
{
    if (grantAdmin)
        currentRole |= PermissionFlags.Admin;

    if (revokeWrite)
        currentRole ^= PermissionFlags.Write;  // "remove write" per ticket

    currentRole /= 2;  // "normalize" role mask after shift-left in legacy import

    return currentRole;
}
```

What would you change before merging?

**Answer:** Three operator mistakes stack: `^=` **toggles** Write (adds it if absent), it does not reliably remove; `|=` without validating existing flags can grant Admin on corrupted masks; and `/=` performs **integer division** on a bit mask, destroying unrelated permission bits instead of "normalizing" a shift.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `role ^= Write` toggles Write bit — wrong for "revoke write" | Users lose or gain Write unpredictably depending on current state |
| Correctness | `currentRole /= 2` is integer division, not "undo << 1" on flags | Arbitrary permission bits cleared — privilege escalation or lockout |
| Design | Compound bitwise ops (`|=`, `&= ~Flag`) require domain knowledge | Copy-paste from **PermissionFlags** tutorial without **RemoveWriteViaAndNot** pattern |
| Maintainability | Magic `/= 2` comment references legacy import | Future devs cannot reason about resulting mask |

**Fix (priority order):**

1. Revoke with AND-NOT: `currentRole &= ~PermissionFlags.Write;` (see **PermissionFlags.RemoveWriteViaAndNot**).
2. Grant with OR: keep `|=` for Admin only after validating `currentRole` is a known baseline mask.
3. Replace `/= 2` with explicit unshift if legacy data was shifted: `currentRole >>= 1` **only** if product owner confirms all roles were uniformly shifted — otherwise migrate data offline, do not "fix" in request path.
4. Return new mask from pure function; log before/after for audit; unit-test grant/revoke combinations.

```csharp
if (revokeWrite)
    currentRole &= ~PermissionFlags.Write;
// Remove currentRole /= 2 unless data migration explicitly requires >>= 1
```

**Production takeaway:** Compound assignment (`|=`, `&=`, `^=`, `/=`) is concise but encodes irreversible in-place mutations — Karat tests whether you know **XOR toggle vs AND-NOT clear** and that `/=` on flags is almost never what you want. See **PermissionFlags.ApplyCompoundAssignments** for intentional demo code vs production-safe **RemoveWriteViaAndNot**.

---

---

#### Q6. (P) Your team ships pricing, inventory, and permission rules that mix arithmetic, `??`, `&&`, and `|=` in single expressions. What review checklist would you use in PRs to catch operator bugs before they reach production?

---

---

### 05. Type Conversion & Casting - Done

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/05. Type Conversion & Casting - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Treat every multi-operator expression as a liability — require explicit parentheses, short-circuit logical operators for guards with side effects, and separate mutation from conditionals unless the idiom is a well-known flag pattern.

- **Precedence:** Flag any expression mixing `??` with `+`, `-`, `<`, or `?:` without parentheses; require `(a ?? b) + c` style or split into named locals (see **PrecedenceExamples**).
- **Short-circuit:** Side effects (DB calls, reserve/decrement, logging) must use `&&`/`||`, never `&`/`|` on bool; put failing checks on the left.
- **Arithmetic domain:** Money paths use `decimal` literals (`49.99m`); count paths use `checked` or `long` when products can overflow `int`; never rely on integer `/` for fractional business rules (**OrderArithmetic.CompareDivision**).
- **Compound assignment:** Bitwise `^=` is toggle, not remove; `/=` and `%=` on counts need explicit comment or rejection; prefer `role = (role & ~Flag)` for revocations.
- **Readability rule:** If an expression needs a comment to explain evaluation order, extract to two or three statements with descriptive names — reviewers and Karat both reward clarity over one-liners.

**Production takeaway:** Layer 1 teaches operator tables; Layer 2 expects a **PR nose** for precedence, short-circuit, overflow, and compound-assignment footguns — the same families demonstrated in this folder's **Program.cs** warehouse scenario.

---

---

### 05. Type Conversion & Casting - Done

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/05. Type Conversion & Casting - Done`

---

---

#### Q1. (R) A warehouse pricing service receives quantities from an upstream JSON deserializer boxed as `object`. Review this method — what fails at runtime, and how would you fix it?

```csharp
public decimal CalculateLineTotal(object quantityBoxed, decimal unitPrice)
{
    long units = (long)quantityBoxed;   // upstream stored int.Parse result as object
    return units * unitPrice;
}

// Caller:
object qty = int.Parse("36");           // implicit box — int on heap
var total = CalculateLineTotal(qty, 49.99m);
```

---

**Answer:**

**Answer:** Unboxing requires the cast target to match the exact boxed type — `(long)quantityBoxed` throws `InvalidCastException` because the heap object holds an `int`, not a `long`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `(long)` unbox on object boxed as `int` | `InvalidCastException` — order total calculation fails mid-request |
| Type model | Treating `object` as a generic numeric without inspection | Fragile when upstream changes serializer or numeric width |
| Design | No validation before arithmetic | Bad data propagates into pricing instead of failing fast at boundary |

**Fix (priority order):**

1. Unbox to the exact stored type: `int units = (int)quantityBoxed`, or use pattern matching: `if (quantityBoxed is int units)`.
2. Prefer strongly typed parameters (`int unitsOrdered`) at service boundaries — avoid `object` for scalars unless truly dynamic.
3. If the source type varies at runtime, branch with `is int`, `is long`, etc., or normalize to `decimal`/`long` at the API edge once.
4. Add a unit test that boxes `int` and asserts no exception — mirrors **Program.cs** Section 9 boxing demo.

**Production takeaway:** Boxing/unboxing looks harmless in tutorials; Karat uses it to test whether you know unbox casts are exact-type, not widening. See foundation **Type Conversion** — `(long)boxed` when boxed as `int` throws.

---

---

#### Q2. (R) An ASP.NET Core order API accepts a quantity path segment. Review the action — what breaks for bad input, and what would you change?

```csharp
[HttpGet("orders/{quantity}")]
public IActionResult GetOrderLine(string quantity)
{
    int units = int.Parse(quantity);
    if (units <= 0)
        return BadRequest("Quantity must be positive.");

    var line = _orderService.BuildLine(units);
    return Ok(line);
}
```

---

**Answer:**

**Answer:** `int.Parse` throws `FormatException` or `OverflowException` on non-numeric or out-of-range path values — ASP.NET turns that into a 500 instead of a client-facing 400 with a clear validation message.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Parse` on untrusted route segment `"abc"` or `"99999999999999999999"` | Unhandled exception → 500 Internal Server Error |
| API contract | Validation runs only after a throwing parse | Positive-quantity check never reached on bad text |
| Operability | Exception-based control flow on hot path | Noisy logs; harder to distinguish client mistakes from server bugs |

**Fix (priority order):**

1. Replace with `int.TryParse(quantity, out int units)` — return `BadRequest` when `false`.
2. Optionally bind with `[FromRoute] int quantity` and let model binding produce 400 for non-int routes (framework handles format).
3. Keep range checks (`units <= 0`, max order size) after successful parse.
4. Reserve `Parse` for trusted constants (config, test fixtures) — matches **Program.cs** Section 5 guidance.

```csharp
if (!int.TryParse(quantity, out int units) || units <= 0)
    return BadRequest("Quantity must be a positive integer.");
```

**Production takeaway:** Parse vs TryParse is not stylistic — on HTTP boundaries, throwing parse crashes the request pipeline. See **ParseDemo** — `TryParse` for external input, `Parse` for known-good text.

---

---

#### Q3. (R) A bonus-units endpoint maps optional query text to an integer. Review both methods — which hidden behavior causes incorrect totals in production?

```csharp
public int GetBonusUnitsFromQuery(string? bonusText)
{
    return Convert.ToInt32(bonusText);   // missing query → null
}

public int GetBonusUnitsStrict(string? bonusText)
{
    return int.Parse(bonusText!);        // developer added null-forgiving
}
```

---

**Answer:**

**Answer:** `Convert.ToInt32(null)` silently returns `0`, so a missing optional bonus query applies zero bonus without error — while `int.Parse(null!)` throws `ArgumentNullException` and fails the request loudly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Convert.ToInt32` maps null → default `0` | Missing `?bonus=` treated as "zero bonus" — under-credits or wrong pricing |
| Runtime | `int.Parse` on null (even with `!`) | `ArgumentNullException` — 500 for absent optional parameter |
| API design | No tri-state (missing vs zero vs invalid) | Cannot distinguish "no bonus specified" from "bonus is literally 0" |

**Fix (priority order):**

1. Model optional input explicitly: `int? bonus = string.IsNullOrWhiteSpace(bonusText) ? null : int.TryParse(...) ? n : throw/400`.
2. Do not use `Convert.ToInt32` for optional HTTP query parameters unless zero-is-correct is a documented business rule.
3. Remove null-forgiving on `Parse` — it hides nullability warnings without making null valid.
4. Return `400` for malformed text; omit bonus logic when parameter is absent.

**Production takeaway:** The Convert class is convenient for legacy `object`/`DBNull` pipelines (**Program.cs** Section 7), but its null→default behavior is a silent data bug on optional API fields. Prefer `TryParse` + nullable types at boundaries.

---

---

#### Q4. (R) Inventory assigns shelf slot IDs stored in a `byte` column. Review this service — what corrupts data silently, and how do you prevent it?

```csharp
public byte AssignShelfSlot(int inventoryLocationId)
{
    // Location IDs come from a legacy int column; values can exceed 255
    return (byte)inventoryLocationId;
}

public void PersistSlot(int locationId)
{
    byte slot = AssignShelfSlot(locationId);
    _db.Execute("UPDATE bins SET slot_id = @slot", new { slot });
}
```

---

**Answer:**

**Answer:** Narrowing `(byte)inventoryLocationId` in default unchecked context wraps values above 255 — location `300` persists as slot `44` (`300 % 256`) with no exception, corrupting bin assignments.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Unchecked narrowing cast int → byte | Silent wrap — wrong shelf slot in database |
| Data integrity | No validation before SQL write | Corruption spreads to picking, shipping, audits |
| Observability | No exception or log on overflow | Bug discovered only when physical inventory mismatches |

**Fix (priority order):**

1. Validate range before cast: `if (locationId is < 0 or > 255) throw/return error`.
2. Use `checked((byte)locationId)` to throw `OverflowException` if you prefer fail-fast over manual bounds check.
3. Consider widening the column to `smallint`/`int` if business IDs legitimately exceed 255 — schema fix beats repeated casts.
4. Add tests for boundary values 255, 256, 300 — mirrors **OverflowCastDemo** in **Program.cs** Section 4.

**Production takeaway:** Explicit casts truncate or wrap; they never round. Production inventory code needs validation or `checked` context — Karat tests whether you treat narrowing as a data-loss operation, not a free conversion.

---

---

#### Q5. (R) A shipping label builder walks a heterogeneous `List<object>` of line items. Review this code — what throws or returns wrong data?

```csharp
public string BuildLabel(object lineItem)
{
    if (lineItem is PhysicalLineItem)
        return ((PhysicalLineItem)lineItem).Sku;

    var digital = lineItem as DigitalLineItem;
    return digital.DownloadCode;   // digital is null for unknown types

    // Caller also tried: var n = lineItem as int;  // CS0039 — won't compile on object
}
```

---

**Answer:**

**Answer:** After `as DigitalLineItem` fails for unknown or null items, accessing `digital.DownloadCode` throws `NullReferenceException` — and the redundant cast after `is PhysicalLineItem` shows incomplete pattern-matching adoption.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `digital.DownloadCode` when `as` returned null | `NullReferenceException` for unsupported line types |
| Style / maintainability | `is` check then manual `(PhysicalLineItem)` cast | Duplicated type test; use pattern matching instead |
| Compile-time | `lineItem as int` on `object` | Invalid — `as` only works with reference and nullable value types, not unboxing boxed ints (**Program.cs** Section 10) |

**Fix (priority order):**

1. Replace physical branch with pattern: `if (lineItem is PhysicalLineItem physical) return physical.Sku;`.
2. Guard digital branch: `if (lineItem is DigitalLineItem digital) return digital.DownloadCode;`.
3. Return fallback or throw a domain exception for unrecognized types — never dereference unchecked `as` results.
4. For boxed value types, use `is int n` or `(int)obj`, not `as int`.

```csharp
return lineItem switch
{
    PhysicalLineItem physical => physical.Sku,
    DigitalLineItem digital => digital.DownloadCode,
    _ => throw new InvalidOperationException($"Unknown line item type {lineItem?.GetType().Name}")
};
```

**Production takeaway:** `as` returns null on failure — production code must null-check or prefer `is` patterns that assign in one step. See **IsAsDemo** — interface and derived-type inspection on `object` references.

---

---

#### Q6. (P) A partner integration POSTs prices as formatted strings in JSON (`"amountText": "1.234,56"`). The API runs on en-US servers. Review the handler — what fails across environments, and what contract would you enforce?

```csharp
public record PriceDto(string Sku, string AmountText);

[HttpPost("prices")]
public IActionResult ImportPrice([FromBody] PriceDto dto)
{
    var price = decimal.Parse(dto.AmountText);   // current thread culture
    _catalog.SavePrice(dto.Sku, price);
    return Ok(new { dto.Sku, price });
}
```

---

---

### 06. Control Flow & Loops - Done

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/06. Control Flow & Loops - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** `decimal.Parse` without `IFormatProvider` uses the current thread culture — en-US servers reject `"1.234,56"` (German grouping/decimal) or misread `"1,234.56"`, causing intermittent `FormatException` or wrong stored prices depending on deployment region.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Culture-dependent parse on API string field | Same JSON payload succeeds in one region, fails or mis-parses in another |
| API contract | Locale-formatted strings in machine-to-machine JSON | Partners must guess server culture; brittle integration |
| Runtime | `FormatException` on valid partner data | 500 or skipped catalog updates in production |

**Fix (priority order):**

1. **Best contract:** Accept JSON numbers (`"amount": 1234.56`) — deserialization handles invariant binary representation; no parse step.
2. If strings are required, parse with `CultureInfo.InvariantCulture` (or documented partner culture): `decimal.Parse(dto.AmountText, NumberStyles.Number, CultureInfo.InvariantCulture)`.
3. Prefer `decimal.TryParse` and return `400 ProblemDetails` with field-level validation errors.
4. Document and test both `"1234.56"` invariant and reject ambiguous formats — align with **ParseDemo** European `de-DE` example in **Program.cs** Section 5.

```csharp
if (!decimal.TryParse(dto.AmountText, NumberStyles.Number,
        CultureInfo.InvariantCulture, out var price))
    return ValidationProblem(/* ... */);
```

**Production takeaway:** Culture-aware parsing belongs where locale is intentional (UI, printed invoices). HTTP API bodies should use invariant culture or native JSON numeric types — Karat stacks globalization with API design in one question.

---

---

### 06. Control Flow & Loops - Done

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/06. Control Flow & Loops - Done`

---

---

#### Q1. (R) A developer ports a C-style fulfillment router into C#. The build fails with CS0163. Review the switch — what is wrong, and how would you fix it while preserving the shared "in transit" behavior?

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
            message = "Ready for carrier";
        case "Shipped":
        case "Delivered":
            return "In transit pipeline";
        default:
            return "Unknown — escalate";
    }
}
```

---

**Answer:**

**Answer:** C# does not allow fall-through between cases that contain executable statements — the `"Packed"` case assigns to `message` but never exits with `break`/`return`, triggering CS0163. Shared labels work only when multiple `case` labels precede a single block with one exit point.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `"Packed"` case has statements but no `break`/`return`/`throw` | CS0163 — build blocked |
| Logic | Undeclared `message` variable (if compile were forced) | Would not compile (CS0103) |
| Port mistake | Assumed C/C++ fall-through semantics | Classic switch migration trap |

**Fix (priority order):**

1. Use shared labels the C# way — multiple cases, one block, one exit:

```csharp
case "Packed":
case "Shipped":
case "Delivered":
    return "In transit pipeline";
```

2. Or give `"Packed"` its own `return` if the message must differ: `return "Ready for carrier";`
3. Prefer `return` per case (as in this chapter's `GetStatusMessage`) — eliminates missing-`break` bugs entirely.
4. For value-returning routing, consider a switch expression (`status switch { "Pending" => ..., _ => ... }`) — no fall-through surface area.

**Production takeaway:** Fulfillment status routing is a common Karat snippet — know that C# only allows fall-through via **empty** stacked labels, not between bodies. See **Program.cs** Section 4 — classic switch `break` rules.

---

---

#### Q2. (R) A nightly batch job counts warehouse slots for billing. QA reports the invoice is one slot short for every aisle. Review the nested loop:

```csharp
public static int CountBillableSlots(int aisles, int shelvesPerAisle)
{
    int count = 0;
    for (int aisle = 1; aisle < aisles; aisle++)
    {
        for (int shelf = 1; shelf <= shelvesPerAisle; shelf++)
        {
            count++;
        }
    }
    return count;
}
// Called with CountBillableSlots(5, 10) — ops expects 50 slots.
```

What is wrong, and what would you change?

---

**Answer:**

```csharp
public static int CountBillableSlots(int aisles, int shelvesPerAisle)
{
    int count = 0;
    for (int aisle = 1; aisle < aisles; aisle++)
    {
        for (int shelf = 1; shelf <= shelvesPerAisle; shelf++)
        {
            count++;
        }
    }
    return count;
}
// Called with CountBillableSlots(5, 10) — ops expects 50 slots.
```

What is wrong, and what would you change?

**Answer:** The outer loop uses `aisle < aisles` with a 1-based start, so it runs for aisles 1–4 instead of 1–5 — an off-by-one error that drops one entire aisle (10 slots) from the count.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Outer bound `aisle < aisles` with 1-based indexing | Misses last aisle — billing under-count |
| Consistency | Inner loop uses `<= shelvesPerAisle`, outer uses `< aisles` | Mixed inclusive/exclusive bounds — easy to miss in review |
| Testing | `(5, 10)` returns 40, not 50 | Silent revenue loss until reconciliation |

**Fix (priority order):**

1. Match the chapter's inclusive pattern: `for (int aisle = 1; aisle <= aisles; aisle++)` — mirrors `CountWarehouseSlots` in **Program.cs** Section 12.
2. Alternatively use 0-based indexing consistently: `for (int aisle = 0; aisle < aisles; aisle++)` — document which convention the API uses.
3. Add a unit test asserting `CountBillableSlots(5, 10) == 50` and edge cases (`0` aisles, `1` shelf).
4. Name parameters or add XML docs clarifying whether counts are 1-based inclusive ranges.

**Production takeaway:** Off-by-one is the most common loop bug — Karat pairs `<` vs `<=` with a business consequence (billing). Always trace first/last iteration against expected cardinality.

---

---

#### Q3. (R) A gate-controller service hangs in staging after a config change. Review the retry loop:

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
        // forgot to increment attempt
    }
    return false;
}
```

Another team member "fixes" it with `while (true)` and a `break` inside `TryOpenGate()` that only runs on success — but `maxAttempts` is never checked. What breaks in each version, and what is the safe bounded-retry pattern?

---

**Answer:**

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
        // forgot to increment attempt
    }
    return false;
}
```

Another team member "fixes" it with `while (true)` and a `break` inside `TryOpenGate()` that only runs on success — but `maxAttempts` is never checked. What breaks in each version, and what is the safe bounded-retry pattern?

**Answer:** Version one never increments `attempt`, so `attempt < maxAttempts` stays true forever when `TryOpenGate()` fails — an infinite loop that hangs the worker. Version two removes the upper bound entirely, so repeated failures spin forever unless an external timeout kills the process.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Missing `attempt++` after failed try | Infinite loop — thread/process hung, health checks fail |
| Runtime | `while (true)` without attempt cap | Unbounded retry — CPU burn, no graceful degradation |
| Operability | No logging between attempts | On-call cannot tell stuck vs slow vs failing gate |

**Fix (priority order):**

1. Use the bounded retry pattern from **Program.cs** Section 16:

```csharp
int attempt = 0;
while (attempt < maxAttempts)
{
    attempt++;
    if (TryOpenGate())
    {
        return true;
    }
}
return false;
```

2. Increment **before or after** the try, but always on every iteration — never only on success.
3. If using `while (true)`, enforce `if (++attempt >= maxAttempts) return false;` inside the body — still prefer `while (attempt < maxAttempts)` for readability.
4. Add delay/backoff between attempts for I/O gates; log attempt count and failure reason.

**Production takeaway:** `while (true)` is valid only when a guaranteed exit path exists (`break`, `return`, `throw`). Production services need bounded retries plus observability — see **TryOpenGate** in this chapter.

---

---

#### Q4. (R) A pick-list optimizer searches a 2D bin grid for the first high-priority SKU. It finds the SKU but keeps scanning every remaining aisle. Review:

```csharp
public static (int row, int col)? FindPrioritySku(string[,] grid, string target)
{
    for (int row = 0; row < grid.GetLength(0); row++)
    {
        for (int col = 0; col < grid.GetLength(1); col++)
        {
            if (grid[row, col] == target)
            {
                continue; // found — move to next cell
            }
        }
    }
    return null;
}
```

What is wrong with `continue` here, and how would you fix it for early exit?

---

**Answer:**

```csharp
public static (int row, int col)? FindPrioritySku(string[,] grid, string target)
{
    for (int row = 0; row < grid.GetLength(0); row++)
    {
        for (int col = 0; col < grid.GetLength(1); col++)
        {
            if (grid[row, col] == target)
            {
                continue; // found — move to next cell
            }
        }
    }
    return null;
}
```

What is wrong with `continue` here, and how would you fix it for early exit?

**Answer:** `continue` skips to the **next inner-loop iteration** — it does not exit either loop or return coordinates. Even when the SKU is found, scanning continues; the method always returns `null`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `continue` instead of `return`/`break` on match | Never returns found position — pick optimizer always fails |
| Performance | Full grid scan on every call | O(rows × cols) even after early match — wasted warehouse API time |
| API design | Nullable return never populated | Callers cannot route pickers to the bin |

**Fix (priority order):**

1. Return immediately on match: `return (row, col);` — clearest fix (matches `TryFindSku` using early `return true` in Section 14).
2. If only the inner loop should stop, use `break` plus a found flag — but `return` is simpler for a search helper.
3. To exit **both** nested loops without `return`, use a labeled `break` or extract search into a method that returns on first hit.
4. Add a test: 3×3 grid with target at `[0,0]` — assert scan stops and coordinates match.

**Production takeaway:** `break` exits the innermost loop/switch; `continue` advances to the next iteration of the **innermost** loop only. Karat uses nested loops to test whether you confuse the two with `return`.

---

---

#### Q5. (R) A pricing API uses pattern matching on order payloads. Support tickets report zero-quantity lines labeled as "positive." Review:

```csharp
public static string ClassifyLine(object line)
{
    switch (line)
    {
        case int qty:
            return qty > 0 ? $"Active: {qty} units" : $"Zero qty: {qty}";
        case int units when units > 0:
            return $"Positive quantity: {units}";
        case string sku when sku.Length > 0:
            return $"SKU: {sku}";
        default:
            return "Unsupported";
    }
}
// ClassifyLine(0) returns "Active: 0 units" — product owner expected "Zero qty: 0".
```

What is wrong, and how do case order and `when` guards interact?

---

**Answer:**

```csharp
public static string ClassifyLine(object line)
{
    switch (line)
    {
        case int qty:
            return qty > 0 ? $"Active: {qty} units" : $"Zero qty: {qty}";
        case int units when units > 0:
            return $"Positive quantity: {units}";
        case string sku when sku.Length > 0:
            return $"SKU: {sku}";
        default:
            return "Unsupported";
    }
}
// ClassifyLine(0) returns "Active: 0 units" — product owner expected "Zero qty: 0".
```

What is wrong, and how do case order and `when` guards interact?

**Answer:** Switch cases are evaluated **top to first match** — the unguarded `case int qty:` matches every `int`, including zero, before the `when units > 0` arm is ever considered. The guarded case is unreachable dead code for the intended positive-only path.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Broad `case int qty:` listed before guarded `case int when units > 0` | Zero/negative ints never reach the guarded arm |
| Maintainability | Duplicate `int` handling with conflicting messages | Product confusion — "Active" label on zero qty |
| Dead code | Second `int` case appears to handle positives | Misleading during review — looks correct but never runs for matched type |

**Fix (priority order):**

1. Order from **most specific to least** — guarded cases first:

```csharp
case int units when units > 0:
    return $"Positive quantity: {units}";
case int units:
    return $"Non-positive quantity: {units}";
```

2. Mirror **Program.cs** `DescribePayload` — `when units > 0` before bare `case int units`.
3. Remove redundant ternary in the first arm if cases are split cleanly.
4. Add tests for `0`, `-1`, positive int, empty string, and non-int payload.

**Production takeaway:** Pattern matching is first-match-wins, not best-match-wins — `when` guards do not override an earlier matching type pattern. See foundation **Control Flow** pattern switch and Section 6 in this chapter.

---

---

#### Q6. (R) A barcode scan worker sums active line quantities but under-reports totals. Review:

```csharp
public static int SumActiveLines(int[] quantities)
{
    int total = 0;
    foreach (int qty in quantities)
    {
        if (qty <= 0)
            break; // skip bad lines
        total += qty;
    }
    return total;
}
// Input: { 2, 0, 5, 3 } — expected 10, actual 2.
```

What is wrong, and what is the difference between `break` and `continue` in this loop?

---

### 07. Methods - Done

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/07. Methods - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public static int SumActiveLines(int[] quantities)
{
    int total = 0;
    foreach (int qty in quantities)
    {
        if (qty <= 0)
            break; // skip bad lines
        total += qty;
    }
    return total;
}
// Input: { 2, 0, 5, 3 } — expected 10, actual 2.
```

What is wrong, and what is the difference between `break` and `continue` in this loop?

**Answer:** `break` **exits the entire loop** on the first non-positive quantity — processing stops at `0` and never reaches `5` or `3`. The developer meant to skip bad lines and keep summing, which requires `continue`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `break` on `qty <= 0` instead of `continue` | Stops at first zero/negative — total 2 instead of 10 |
| Semantics | Comment says "skip bad lines" but code aborts | Comment/code mismatch — classic review trap |
| Data integrity | Inventory totals wrong when discontinued lines appear mid-batch | Shipping and stock reconciliation errors |

**Fix (priority order):**

1. Replace `break` with `continue` — matches `SumActiveQuantities` in **Program.cs** Section 15.
2. Optionally track `skippedLines` for audit logging when qty <= 0.
3. Clarify comment: `continue` skips **this iteration**; `break` ends the **whole** foreach.
4. Unit test mixed arrays: `{ 2, 0, 5, -1, 3 }` → total `10`, skipped `2`.

**Production takeaway:** `break` = stop looping entirely; `continue` = skip to next element. Karat embeds the bug in a foreach that looks like the chapter's correct `continue` example — read the jump statement, not the comment alone.

---

---

### 07. Methods - Done

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/07. Methods - Done`

---

---

#### Q1. (R) A warehouse API helper is supposed to bump packed quantity in place before case-splitting. QA reports the count never changes. Review the call site and method — what is wrong, and how do you fix it?

```csharp
public static void AdjustQuantity(ref int units, int extra) => units += extra;

// InventoryService.cs
int packedUnits = 36;
AdjustQuantity(packedUnits, 12);   // caller expects packedUnits == 48
bool ok = TrySplitCases(packedUnits, 12, out int cases, out int loose);
```

---

**Answer:**

**Answer:** `AdjustQuantity` is declared with `ref`, but the call site omits `ref` — that is a compile error (CS1615). If the team "fixed" it by removing `ref` from the signature instead, the method receives a copy of `packedUnits` and the caller stays at 36, matching the by-value trap in **Program.cs** Section 6.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile / API | Missing `ref` at call site when parameter is `ref int` | CS1615 — build blocked until corrected |
| Correctness | If `ref` was dropped from the signature to silence the error | Silent bug — quantity never updates; downstream `TrySplitCases` uses stale value |
| Design | Using `ref` for a single updated integer | Works, but returning `(int newUnits, …)` or assigning a return value is clearer for most callers |

**Fix (priority order):**

1. Add `ref` at the call site: `AdjustQuantity(ref packedUnits, 12);` — both declaration and call must use `ref`.
2. If the team avoids `ref` for readability, change the method to return the new value: `packedUnits = BumpQuantity(packedUnits, 12);`.
3. Add a unit test asserting `packedUnits` changes before `TrySplitCases` runs — catches by-value regressions in CI.

**Production takeaway:** `ref`/`out`/`in` are part of the method signature — callers must match exactly. Karat pairs this with the tutorial's `TryBumpByValue` vs `AdjustQuantity(ref …)` demo. See foundation **Methods** — ref requires initialization and keyword at both sites.

---

---

#### Q2. (R) A pricing service wraps a Try-pattern helper. Under some inputs the process throws instead of returning `false`. Review the method:

```csharp
public static bool TryApplyVolumeDiscount(
    decimal amount,
    int tier,
    out decimal discounted,
    out string reason)
{
    if (amount <= 0m)
    {
        reason = "Amount must be positive.";
        return false;   // early exit
    }

    discounted = amount * (1m - tier * 0.05m);
    return discounted >= amount * 0.5m;
}
```

What breaks at runtime, and what would you change?

---

**Answer:**

```csharp
public static bool TryApplyVolumeDiscount(
    decimal amount,
    int tier,
    out decimal discounted,
    out string reason)
{
    if (amount <= 0m)
    {
        reason = "Amount must be positive.";
        return false;   // early exit
    }

    discounted = amount * (1m - tier * 0.05m);
    return discounted >= amount * 0.5m;
}
```

What breaks at runtime, and what would you change?

**Answer:** On the failure path the method assigns `reason` but never assigns `out decimal discounted` before returning — the C# compiler enforces definite assignment for `out` parameters (CS0177 at compile time in strict builds; if `discounted` were partially fixed, any missed path still violates the contract). Callers using `discounted` after `false` would read an unassigned variable.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile / contract | `discounted` not assigned on the `amount <= 0` return path | CS0177 — build failure, or undefined output if analyzer rules differ |
| API semantics | Try-pattern implies all `out` values are defined on both success and failure | Callers may use `discounted` in logging even when `false` — wrong values or analyzer warnings |
| Maintainability | Future branch added without assigning every `out` | Easy regression — each new early return must assign all outs |

**Fix (priority order):**

1. Assign every `out` parameter on **every** return path: `discounted = 0m;` (or `amount`) before `return false` on the validation branch.
2. Consider reducing `out` count — return `(bool ok, decimal discounted, string? reason)` or a small result record for clarity.
3. Add tests for failure paths asserting `discounted` and `reason` have expected sentinel values.

```csharp
if (amount <= 0m)
{
    discounted = 0m;
    reason = "Amount must be positive.";
    return false;
}
```

**Production takeaway:** `out` means the callee **must** assign before any return — unlike `ref`, the caller does not initialize. Matches **Program.cs** Section 7 and the `TrySplitCases` pattern. See foundation **Methods** — CS0177 gotcha.

---

---

#### Q3. (R) A developer adds a flexible shipping-fee helper and the project fails to compile. Review the signatures and one call site:

```csharp
public static decimal AddFees(decimal subtotal, params decimal[] surcharges) =>
    subtotal + surcharges.Sum();

public static decimal AddFees(decimal subtotal, params decimal[] surcharges, decimal taxRate) =>
    (subtotal + surcharges.Sum()) * (1m + taxRate);

var total = AddFees(100m, 5m, 2.50m, taxRate: 0.08m);
```

What is wrong, and how would you redesign this API?

---

**Answer:**

```csharp
public static decimal AddFees(decimal subtotal, params decimal[] surcharges) =>
    subtotal + surcharges.Sum();

public static decimal AddFees(decimal subtotal, params decimal[] surcharges, decimal taxRate) =>
    (subtotal + surcharges.Sum()) * (1m + taxRate);

var total = AddFees(100m, 5m, 2.50m, taxRate: 0.08m);
```

What is wrong, and how would you redesign this API?

**Answer:** `params` must be the **last** parameter in the parameter list — placing `decimal taxRate` after `params decimal[] surcharges` is illegal (CS0231). Even if reordered, two `params` overloads would be invalid (only one `params` per method).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Parameter after `params` array | CS0231 — build blocked |
| API design | Two overloads both using `params` for the same position | CS0229 if both existed — only one params parameter allowed per method |
| Overload resolution | `AddFees(100m, 5m, 2.50m, taxRate: 0.08m)` — compiler must decide whether `0.08m` is another surcharge or tax | Ambiguous or surprising binding if signatures were "fixed" without care |

**Fix (priority order):**

1. Move non-params parameters **before** the `params` array: `AddFees(decimal subtotal, decimal taxRate, params decimal[] surcharges)`.
2. Prefer explicit overloads over `params` for production fee APIs — e.g., `AddFees(subtotal, IEnumerable<decimal> surcharges, decimal taxRate = 0m)` — clearer for JSON/API callers and unit tests.
3. Drop duplicate `params` overloads; use optional `taxRate` on a single method or named arguments: `AddFees(subtotal, taxRate: 0.08m, surcharges: new[] { 5m, 2.50m })`.

**Production takeaway:** `params` is syntactic sugar for callers, not a general "variadic tail" — it must be last and appears once. Fixed-arity overloads (like `CountTokens(string, string)` vs `params` in **Program.cs** Section 5) win overload resolution when they match exactly.

---

---

#### Q4. (P) Your team ships `OrderFormatting.dll` v1.0 with this public API:

```csharp
public static string FormatMoney(decimal amount, string currency = "USD")
    => $"{currency} {amount:N2}";
```

In v1.1 you change the default to `"USD "` (trailing space) for alignment. Existing microservices reference the new DLL but were **not recompiled**. What do callers observe, and how should you version optional-parameter defaults in shared libraries?

---

**Answer:**

```csharp
public static string FormatMoney(decimal amount, string currency = "USD")
    => $"{currency} {amount:N2}";
```

In v1.1 you change the default to `"USD "` (trailing space) for alignment. Existing microservices reference the new DLL but were **not recompiled**. What do callers observe, and how should you version optional-parameter defaults in shared libraries?

**Answer:** Optional parameter defaults are embedded at the **call site at compile time**, not resolved at runtime from the callee's metadata — services compiled against v1.0 keep passing `"USD"` implicitly even when v1.1 DLL is deployed, so behavior diverges from source-only callers who recompiled.

- **Observed split:** Recompiled callers get `"USD 123.45"` (with trailing space in default); non-recompiled callers still emit `"USD123.45"` — formatting inconsistencies across services, failed snapshot tests, and confused support tickets.
- **Why:** The compiler emits the default literal into each calling assembly's IL; swapping the DLL alone does not rewrite those call sites. See **Program.cs** Section 10 — "baked into the call site at compile time."
- **Safe patterns:** Treat default changes as **breaking** — bump major version, require rebuild, or avoid optional params on public library surfaces; use overloads (`FormatMoney(amount)` → calls `FormatMoney(amount, "USD")`) so defaults live in one method body inside the library.
- **Alternative:** Explicit arguments at all call sites in production code (`FormatMoney(x, "USD")`) — no implicit default dependency.
- **Documentation:** Release notes must say "recompile required" when any optional default changes; CI should rebuild all consumers on shared library updates.

**Production takeaway:** Optional parameters are convenient in tutorials but fragile for shared binaries — prefer overloads or mandatory parameters for stable public APIs. Karat tests whether you know DLL swap ≠ behavior change for optional defaults.

---

---

#### Q5. (R) A catalog service computes pallet arrangements recursively. In production, large orders crash the worker. Review:

```csharp
public static long CountArrangements(int levels)
{
    if (levels == 0)
        return 1;

    return levels * CountArrangements(levels - 1);   // factorial-style
}

// Called from batch job with user-supplied depth:
long ways = CountArrangements(requestedDepth);   // requestedDepth can be 50_000+
```

What fails, why does it surface only under load, and what fix do you prioritize?

---

**Answer:**

```csharp
public static long CountArrangements(int levels)
{
    if (levels == 0)
        return 1;

    return levels * CountArrangements(levels - 1);   // factorial-style
}

// Called from batch job with user-supplied depth:
long ways = CountArrangements(requestedDepth);   // requestedDepth can be 50_000+
```

What fails, why does it surface only under load, and what fix do you prioritize?

**Answer:** Deep recursion allocates one stack frame per call; `requestedDepth` in the tens of thousands exhausts the thread stack and throws `StackOverflowException` — an unhandled crash that kills the worker process. Small test values (e.g., 5 like **Program.cs** `Factorial` demo) pass QA.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Unbounded recursive depth on user input | `StackOverflowException` — process termination, no graceful error body |
| Correctness | Missing validation for negative `levels` | Infinite recursion toward negative depths until stack overflow |
| Operability | Crash only with large production inputs | Passes unit tests; fails under real catalog batch sizes |

**Fix (priority order):**

1. Replace recursion with an iterative loop for factorial-style math — O(1) stack, O(n) time — same result, no stack growth.
2. Validate input: reject `levels < 0` (`ArgumentOutOfRangeException`) and cap `levels` to a business maximum before computing — return 400/problem details at API boundary.
3. If recursion is required (tree structures), document max depth and use tail-recursion-friendly designs where applicable; monitor stack size in load tests.
4. Return typed errors to callers instead of allowing process crash — wrap computation in bounded worker with timeout for untrusted input.

```csharp
public static long CountArrangements(int levels)
{
    if (levels < 0) throw new ArgumentOutOfRangeException(nameof(levels));
    long result = 1;
    for (int i = 2; i <= levels; i++) result *= i;
    return result;
}
```

**Production takeaway:** Recursion needs a base case **and** a bounded depth — **Program.cs** Section 12 warns that deep chains cause `StackOverflowException`. Prefer loops for linear factorial-style work in services.

---

---

#### Q6. (R) A base reporting type and a derived export type disagree at runtime. The derived XML docs say it "overrides" discount logic, but callers through a base reference see the old behavior. Review:

```csharp
public class OrderReport
{
    public virtual decimal ApplyDiscount(decimal amount) => amount * 0.95m;
}

public class WholesaleReport : OrderReport
{
    /// <summary>Overrides ApplyDiscount to use wholesale rate.</summary>
    public decimal ApplyDiscount(decimal amount) => amount * 0.80m;   // note: no override keyword
}

OrderReport report = new WholesaleReport();
decimal result = report.ApplyDiscount(100m);   // team expects 80.00m
```

What is wrong, how does this differ from a true override, and what would you change?

---

**Answer:**

```csharp
public class OrderReport
{
    public virtual decimal ApplyDiscount(decimal amount) => amount * 0.95m;
}

public class WholesaleReport : OrderReport
{
    /// <summary>Overrides ApplyDiscount to use wholesale rate.</summary>
    public decimal ApplyDiscount(decimal amount) => amount * 0.80m;   // note: no override keyword
}

OrderReport report = new WholesaleReport();
decimal result = report.ApplyDiscount(100m);   // team expects 80.00m
```

What is wrong, how does this differ from a true override, and what would you change?

**Answer:** The derived method **hides** the base member (`new` is implied when signatures match without `override`) instead of overriding it — dispatch through `OrderReport report` binds to `OrderReport.ApplyDiscount`, so `result` is **95.00m**, not 80.00m. XML comments describe intent the runtime does not honor.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Missing `override` on `virtual` method — method hiding instead | Wholesale discount never applied when referenced as base type — revenue loss or wrong reports |
| Documentation | XML says "Overrides" but code uses hiding semantics | Misleading for IDE tooltips, Swagger generators, and code reviewers |
| Polymorphism | Factory returns `OrderReport` references | Entire inheritance chain silently uses retail rate — hard-to-spot production bug |

**Fix (priority order):**

1. Add `public override decimal ApplyDiscount(decimal amount) => amount * 0.80m;` — enables virtual dispatch through base references.
2. If hiding was intentional (rare), use explicit `public new decimal ApplyDiscount(...)` and fix XML to say "Hides" — never store as base type when hidden behavior is required.
3. Add polymorphic test: `OrderReport r = new WholesaleReport(); Assert.Equal(80m, r.ApplyDiscount(100m));` — fails with hiding, passes with override.

**Production takeaway:** Method **overloading** (same chapter, Section 4) is compile-time name + signature resolution; **override vs hide** is inheritance — different chapter but Karat stacks them because teams confuse "same method name" with polymorphic replacement. Deep coverage → OOP module; here the trap is `virtual`/`override` vs accidental hiding.

---

---

#### Q7. (R) Static analysis flags a contract mismatch between XML documentation and implementation. Review:

```csharp
/// <summary>
/// Splits <paramref name="units"/> into full cases and remainder.
/// Returns false when units is negative; <paramref name="cases"/> and
/// <paramref name="remainder"/> are zero on failure.
/// </summary>
public static bool TrySplitCases(int units, int unitsPerCase, out int cases, out int remainder)
{
    cases = units / unitsPerCase;
    remainder = units % unitsPerCase;
    return remainder == 0;
}
```

What behavior does the docs promise that the code does not deliver, and what breaks for callers that trust the XML contract?

---

### 08. Strings - Done

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/08. Strings - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
/// <summary>
/// Splits <paramref name="units"/> into full cases and remainder.
/// Returns false when units is negative; <paramref name="cases"/> and
/// <paramref name="remainder"/> are zero on failure.
/// </summary>
public static bool TrySplitCases(int units, int unitsPerCase, out int cases, out int remainder)
{
    cases = units / unitsPerCase;
    remainder = units % unitsPerCase;
    return remainder == 0;
}
```

What behavior does the docs promise that the code does not deliver, and what breaks for callers that trust the XML contract?

**Answer:** The XML claims the method returns `false` when `units` is negative and zeros the `out` values — the implementation never checks for negative `units` and defines success only as "even split" (`remainder == 0`). Callers handling `false` as a validation failure will mis-classify valid uneven splits, and negative inputs produce wrong `cases`/`remainder` without signaling failure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Contract | Docs: `false` on negative input with zeroed outs; code: `false` only when remainder ≠ 0 | Callers' error handling for invalid input never runs — bad data propagates |
| Correctness | Negative `units` with positive `unitsPerCase` yields negative `cases` in C# integer division | Downstream inventory counts wrong; no exception |
| Maintainability | IDE / DocFX / Swagger consumers surface the XML as truth | Onboarding devs implement against documentation, not behavior — persistent integration bugs |

**Fix (priority order):**

1. Align code with docs **or** fix docs to match code — pick one source of truth; prefer code + tests as authority, then update XML.
2. If Try-pattern for validation: `if (units < 0 \|\| unitsPerCase <= 0) { cases = 0; remainder = 0; return false; }` then compute split.
3. If success means "even split" only (as in **Program.cs** `TrySplitCases` demo), rewrite XML: "Returns true when units divides evenly into cases; otherwise false with computed cases and remainder."
4. Enable XML doc warnings (CS1591 / custom analyzers) in CI and add tests for negative input and uneven splits.

**Production takeaway:** XML documentation is a public contract when you ship libraries — drift is as harmful as a breaking signature change. The tutorial's `TrySplitCases` uses the bool for "even split," not negativity — Karat tests reading docs **and** the method body together.

---

### 08. Strings - Done

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/08. Strings - Done`

---

---

#### Q1. (R) A nightly export job builds a CSV of 50,000 warehouse scan lines. After deployment, CPU and Gen0 GC spikes correlate with the export window. Review the row builder:

```csharp
public static string BuildExportCsv(IEnumerable<ParsedScanLine> rows)
{
    string csv = "OrderId,ServiceLevel,SkuSummary\n";
    foreach (var row in rows)
    {
        csv += $"{row.OrderId},{row.ServiceLevel},{row.SkuSummary}\n";
    }
    return csv;
}
```

What is wrong, and how would you fix it for production?

---

**Answer:**

```csharp
public static string BuildExportCsv(IEnumerable<ParsedScanLine> rows)
{
    string csv = "OrderId,ServiceLevel,SkuSummary\n";
    foreach (var row in rows)
    {
        csv += $"{row.OrderId},{row.ServiceLevel},{row.SkuSummary}\n";
    }
    return csv;
}
```

What is wrong, and how would you fix it for production?

**Answer:** `+=` inside the loop creates a new immutable string on every iteration — for 50,000 rows that is tens of thousands of short-lived allocations and O(n²) total copying, which drives Gen0 GC pressure and CPU spikes during the export window.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Repeated `csv += …` in a tight loop | O(n²) char copying; massive intermediate string garbage |
| GC | One allocation per row (often two with interpolation) | Gen0 collections spike; export window slows other services on shared nodes |
| Scalability | Entire CSV held in one `string` at the end | Large LOH pressure if export grows beyond memory budget |
| Correctness (secondary) | Unescaped commas in `SkuSummary` | Broken CSV columns — separate fix, but often discovered during export rewrites |

**Fix (priority order):**

1. Use `StringBuilder` with a reasonable initial capacity (header size + estimated row width × count) and `AppendLine` / `AppendFormat` inside the loop — one final `ToString()` or stream write at the end.
2. For very large exports, write directly to a `StreamWriter` or `PipeWriter` instead of materializing the whole file in memory.
3. Escape or quote CSV fields that may contain commas (proper CSV encoding).
4. Add a benchmark or integration test with a representative row count so regressions show up in CI.

```csharp
public static string BuildExportCsv(IReadOnlyList<ParsedScanLine> rows)
{
    var sb = new StringBuilder(capacity: 64 + rows.Count * 48);
    sb.AppendLine("OrderId,ServiceLevel,SkuSummary");
    foreach (var row in rows)
    {
        sb.Append(row.OrderId).Append(',')
          .Append(row.ServiceLevel).Append(',')
          .Append(row.SkuSummary).AppendLine();
    }
    return sb.ToString();
}
```

**Production takeaway:** String concatenation in loops is a classic immutability trap — `string` is immutable, so every `+=` allocates. See **Program.cs** Section 11 (`LabelAssembler`) and the quick-reference "s1 + s2 in tight loop" row. Use `StringBuilder` or streaming for repeated building.

---

---

#### Q2. (R) A developer "normalizes" incoming scan text before lookup but duplicate orders still appear in the database. Review:

```csharp
public static string NormalizeOrderId(string rawScanLine)
{
    string cleaned = rawScanLine.Trim();
    cleaned.ToUpperInvariant(); // normalize case for lookup
    return cleaned;
}

public static bool OrderExists(string normalizedId, HashSet<string> knownOrders)
{
    return knownOrders.Contains(normalizedId);
}
```

What breaks, and what would you change?

---

**Answer:**

```csharp
public static string NormalizeOrderId(string rawScanLine)
{
    string cleaned = rawScanLine.Trim();
    cleaned.ToUpperInvariant(); // normalize case for lookup
    return cleaned;
}

public static bool OrderExists(string normalizedId, HashSet<string> knownOrders)
{
    return knownOrders.Contains(normalizedId);
}
```

What breaks, and what would you change?

**Answer:** `ToUpperInvariant()` returns a new string and does not mutate `cleaned` — the discarded result means lookup keys keep original casing, so `"ord-1042"` and `"ORD-1042"` are treated as different orders and duplicates slip through.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `ToUpperInvariant()` result not assigned | Normalization silently skipped — case variants create duplicate records |
| API design | Method name implies normalization but returns partially cleaned text | Misleading contract for callers and code reviewers |
| Comparison | `HashSet.Contains` uses default ordinal equality on unstabilized casing | Inconsistent with intended invariant ID rules from **ScanLineParser** |

**Fix (priority order):**

1. Reassign: `cleaned = cleaned.ToUpperInvariant();` or return in one expression: `return rawScanLine.Trim().ToUpperInvariant();`.
2. For machine identifiers prefer `ToUpperInvariant()` / `ToLowerInvariant()` over culture-sensitive `ToUpper()` — see **StringComparisonDemo.TurkishCultureCaseTrap** for the Turkish `"i"` pitfall.
3. Use `StringComparison.OrdinalIgnoreCase` at comparison boundaries if you must preserve display casing but match logically.
4. Add unit tests with mixed-case inputs asserting a single canonical key in the set.

**Production takeaway:** Immutable string methods never change the original — every transform must be reassigned or returned. This is the same class of bug as assuming `Trim()` mutates `rawScanLine` in **Program.cs** Section 2.

---

---

#### Q3. (R) An API endpoint accepts a scan line and compares the caller's API key to a configured secret. Review:

```csharp
public bool ValidateScanRequest(string apiKeyHeader, string configuredKey, string rawScanLine)
{
    if (apiKeyHeader == configuredKey)
    {
        string orderId = rawScanLine.Trim().Split('|')[0];
        return orderId.StartsWith("ORD");
    }
    return false;
}
```

What security and correctness issues do you see, and how would you fix them?

---

**Answer:**

```csharp
public bool ValidateScanRequest(string apiKeyHeader, string configuredKey, string rawScanLine)
{
    if (apiKeyHeader == configuredKey)
    {
        string orderId = rawScanLine.Trim().Split('|')[0];
        return orderId.StartsWith("ORD");
    }
    return false;
}
```

What security and correctness issues do you see, and how would you fix them?

**Answer:** The API key comparison uses default string equality without a fixed comparison mode or timing-safe compare, and order-ID validation uses culture-sensitive defaults — both are risky in production authentication and machine-identifier paths.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `apiKeyHeader == configuredKey` | Not timing-safe; default comparison mode may be culture-sensitive depending on overload resolution context |
| Security | Plain string compare for secrets | Theoretical timing side-channels; no explicit `Ordinal` / `FixedTimeEquals` |
| Correctness | `StartsWith("ORD")` without `StringComparison` | Culture-sensitive prefix rules — wrong for wire/machine IDs (see **ScanLineParser** — uses `StringComparison.Ordinal`) |
| Runtime | `Split('|')[0]` without length/null checks | `IndexOutOfRangeException` or `NullReferenceException` on malformed input |
| Input validation | No `IsNullOrWhiteSpace` on `rawScanLine` | Crashes or accepts garbage scan lines |

**Fix (priority order):**

1. Compare secrets with `CryptographicOperations.FixedTimeEquals` on UTF-8 bytes, or at minimum `string.Equals(apiKeyHeader, configuredKey, StringComparison.Ordinal)`.
2. Use `orderId.StartsWith("ORD", StringComparison.Ordinal)` for machine identifiers — never `CurrentCulture` for IDs, tokens, or paths.
3. Guard inputs: reject null/whitespace scan lines; validate segment count after `Split` (mirror **ScanLineParser.Parse** pipe-delimited structure).
4. Return `false` (or 401) without leaking which check failed — do not branch logic that reveals key vs payload validity.

**Production takeaway:** `StringComparison.Ordinal` / `OrdinalIgnoreCase` for IDs, headers, and secrets; `CurrentCulture` only for user-facing sort/display. See **Program.cs** Section 6 comparison table and Section 11 culture notes.

---

---

#### Q4. (R) A label-printing service crashes intermittently when optional notes are omitted from the request. Review:

```csharp
public static string BuildLabel(string orderId, string notes)
{
    if (notes == "")
    {
        notes = null; // treat empty as missing
    }

    string header = $"Ship: {orderId}";
    string footer = "Notes: " + notes.Trim(); // append customer notes
    return header + Environment.NewLine + footer;
}
```

What fails at runtime, and how would you harden null/empty handling?

---

**Answer:**

```csharp
public static string BuildLabel(string orderId, string notes)
{
    if (notes == "")
    {
        notes = null; // treat empty as missing
    }

    string header = $"Ship: {orderId}";
    string footer = "Notes: " + notes.Trim(); // append customer notes
    return header + Environment.NewLine + footer;
}
```

What fails at runtime, and how would you harden null/empty handling?

**Answer:** When the client omits `notes`, it arrives as `null`, not `""` — the empty-string check never runs, and `notes.Trim()` throws `NullReferenceException`. The method also conflates three states (null, empty, whitespace) without a consistent policy.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `notes.Trim()` when `notes` is `null` | Intermittent crash when JSON omits optional field or deserializer passes null |
| Logic | Only normalizes `""` to null, not whitespace-only strings | `"   "` still flows through — inconsistent footer |
| API contract | Optional string not coalesced before string operations | Callers must guess whether null or empty is expected |

**Fix (priority order):**

1. Use `string.IsNullOrWhiteSpace(notes)` and branch to a safe default footer (e.g., `"Notes: (none)"`) before any instance methods.
2. Prefer null-coalescing: `var safeNotes = string.IsNullOrWhiteSpace(notes) ? "(none)" : notes.Trim();`.
3. Do not assign `notes = null` to mean missing — that increases null dereference risk; normalize to display text or `string.Empty` early.
4. Align with **Program.cs** Section 4 — `string.IsNullOrEmpty` vs `string.IsNullOrWhiteSpace` depending on whether tabs/spaces-only input should count as blank.

```csharp
public static string BuildLabel(string orderId, string? notes)
{
    string header = $"Ship: {orderId}";
    string footer = string.IsNullOrWhiteSpace(notes)
        ? "Notes: (none)"
        : "Notes: " + notes.Trim();
    return header + Environment.NewLine + footer;
}
```

**Production takeaway:** Always gate `.Trim()`, `.Split()`, and concatenation with `IsNullOrEmpty` / `IsNullOrWhiteSpace` on external input — optional API fields are usually null, not empty string.

---

---

#### Q5. (P) Your team formats shipping labels with `$"Total: {orderTotal:C}"` and writes JSON audit logs on servers in `en-US`, `de-DE`, and `ja-JP`. Finance reports totals that do not reconcile across regions. Explain what is happening and what formatting approach you would standardize for display vs wire/storage.

---

**Answer:**

**Answer:** The `:C` format specifier uses `CultureInfo.CurrentCulture` (thread UI culture), so the same `decimal` renders as `$127.50`, `127,50 €`, or `￥127` depending on which server handled the request — audit logs and exported strings are not comparable across regions.

- **Root cause:** Interpolation `{orderTotal:C}` and `ToString("C")` without an explicit provider bind to the executing thread's culture — see **InterpolationDemo.BuildLines** and **CultureStringDemo.FormatTotals**.
- **Display (user-facing):** Format with the user's locale — `orderTotal.ToString("C", CultureInfo.CurrentCulture)` or explicit `CultureInfo.GetCultureInfo(userLocale)` in UI and printed labels.
- **Wire / storage / logs:** Use `CultureInfo.InvariantCulture` (or ISO formats like `"F2"` with invariant, or store raw `decimal` in JSON as a number) so `127.50` is identical on every machine.
- **Parsing round-trip:** If text is produced with `de-DE`, parse with the same culture — **CultureStringDemo.ParseLocalizedAmounts** shows `TryParse` failing when culture mismatches.
- **JSON specifically:** Prefer numeric JSON values for amounts; if stringified, document invariant format in the contract.

```csharp
// Label for German warehouse operator
string label = $"Total: {orderTotal.ToString("C", CultureInfo.GetCultureInfo("de-DE"))}";

// Audit log / export — stable everywhere
string audit = string.Format(CultureInfo.InvariantCulture, "Total:{0:F2}", orderTotal);
```

**Production takeaway:** Separate **display culture** from **invariant culture** — interpolation makes it easy to forget the provider. See **Program.cs** Sections 7, 10, and 11 and `FormatDemo.FormatOrderLine(IFormatProvider, …)`.

---

---

#### Q6. (D) A code review proposes replacing all `StringBuilder` usage with string interpolation because "strings are simpler." The PR touches both a 3-line title builder and `LabelAssembler.BuildFullLabel`-style code that loops over hundreds of SKUs. What guidance would you give — when is `StringBuilder` worth it, and when is plain string composition enough?

---

---

### 09. Arrays - Done

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/09. Arrays - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Keep interpolation and small `string.Concat` / `$"…"` for fixed, few-part text; retain `StringBuilder` (or streaming) when append count scales with data size or loop iterations — simplicity does not justify O(n²) allocations on hot paths.

**Plain strings / interpolation — sufficient when:**

- A handful of fixed parts (e.g., `$"Ship: {orderId}"` from **InterpolationDemo**).
- One-shot joins: `string.Join(" + ", skuList)` as in **ScanLineParser.Parse**.
- Static `string.Concat(prefix, "-", idPart)` for three known segments.

**StringBuilder — justified when:**

- Building text inside loops (CSV rows, log buffers, HTML tables, many SKU lines).
- Dozens of `Append` / `AppendLine` operations where chained `+` would allocate each step (**LabelAssembler.BuildFullLabel**).
- Reusing a buffer across requests (clear and re-append) to amortize capacity — `Clear()` retains capacity per Section 12 notes.

**Anti-patterns to reject in review:**

- Replacing a loop + `StringBuilder` with loop + `result += …`.
- Calling `builder.ToString()` mid-build only to search/replace unless necessary — **LabelAssembler** shows that materializing early defeats the mutable buffer; prefer `StringBuilder` search APIs or track indices.

**Production takeaway:** Choose based on allocation profile and readability, not ideology — Karat tests whether you know immutability cost, not whether you memorize `StringBuilder` API. Default to simple strings; escalate to `StringBuilder` when profiling or loop structure demands it.

---

---

### 09. Arrays - Done

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/09. Arrays - Done`

---

---

#### Q1. (R) A batch job averages exam scores for reporting. QA reports intermittent `IndexOutOfRangeException` in production when a student has no scores yet. Review the helper:

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

What is wrong, and how would you fix it for both empty and populated arrays?

---

**Answer:**

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

What is wrong, and how would you fix it for both empty and populated arrays?

**Answer:** The loop uses `i <= scores.Length`, which reads one past the last valid index (`Length - 1`), causing `IndexOutOfRangeException` on every non-empty array; dividing by `scores.Length` when the array is empty also throws `DivideByZeroException`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Off-by-one: `i <= Length` instead of `i < Length` | `IndexOutOfRangeException` on last iteration |
| Runtime | No guard when `scores.Length == 0` | `DivideByZeroException` on empty input |
| Design | No contract for "no scores" vs error | Callers cannot distinguish missing data from crash |

**Fix (priority order):**

1. Change loop bound to `i < scores.Length` — matches **Program.cs** Section 5 (`for` bound pattern).
2. Handle zero-length explicitly: return `0`, `double.NaN`, or throw `ArgumentException` — document the API contract.
3. Optionally validate `scores` is not null before accessing `.Length`.
4. Add unit tests for `[]`, single element, and multi-element arrays.

```csharp
public static double AverageScores(int[] scores)
{
    if (scores is null || scores.Length == 0)
        return 0; // or throw / return double.NaN — pick one contract

    double total = 0;
    for (int i = 0; i < scores.Length; i++)
        total += scores[i];

    return total / scores.Length;
}
```

**Production takeaway:** Bounds errors compile fine — Karat tests whether you spot `<= Length` vs `< Length` and empty-array division. See foundation **Arrays** — IndexOutOfRangeException gotcha; **Program.cs** Section 4 — bounds guard pattern.

---

---

#### Q2. (R) A developer models a textbook shelf grid with a 2D array, then copies a jagged-array traversal pattern from another service. Review:

```csharp
int[,] shelfStock = new int[,]
{
    { 12, 8, 15 },
    { 5, 20, 10 },
    { 9, 11, 7 }
};

int centerBin = shelfStock[1][2];  // row 1, column 2

string[][] departments = CreateDepartmentCourses();
int scienceCourses = departments[1, 0].Length;  // first science course
```

What breaks at compile time or runtime, and when would you choose `int[,]` vs `string[][]`?

---

**Answer:**

```csharp
int[,] shelfStock = new int[,]
{
    { 12, 8, 15 },
    { 5, 20, 10 },
    { 9, 11, 7 }
};

int centerBin = shelfStock[1][2];  // row 1, column 2

string[][] departments = CreateDepartmentCourses();
int scienceCourses = departments[1, 0].Length;  // first science course
```

What breaks at compile time or runtime, and when would you choose `int[,]` vs `string[][]`?

**Answer:** Rectangular arrays use comma indexing (`[row, col]`); jagged arrays use chained brackets (`[row][col]`). Swapping the syntax produces compile errors — `int[,]` does not support `[1][2]`, and `string[][]` does not support `[1, 0]`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `shelfStock[1][2]` on `int[,]` | CS0021 — wrong indexer arity for rectangular array |
| Compile | `departments[1, 0]` on `string[][]` | CS0022 — jagged rows are separate arrays, not one comma-indexed grid |
| Design | Mixing mental models between grid types | Wrong structure choice wastes memory or complicates iteration |

**Fix (priority order):**

1. Rectangular: `shelfStock[1, 2]` — one memory block, every row same width (**Program.cs** Section 7).
2. Jagged: `departments[1][0]` — outer index selects row array, inner index selects element (**Program.cs** Section 8).
3. Choose `int[,]` when the grid is truly rectangular (shelf bins, image pixels, matrices).
4. Choose `T[][]` when row lengths vary naturally (departments with different course counts); use `departments[d].Length` per row, not a single column count.

**Production takeaway:** Jagged vs multidimensional is a **data-shape** decision, not syntax preference — wrong choice forces awkward padding or nested loops. See **Program.cs** Sections 7–8 and Quick Reference — rectangular vs jagged.

---

---

#### Q3. (R) A pricing service must keep an immutable snapshot of SKU codes before sorting for audit, but the audit log shows the "original" list reordered. Review:

```csharp
public static string[] GetSortedSkusForDisplay(string[] skuCodes)
{
    string[] snapshot = skuCodes;           // preserve original order
    Array.Sort(snapshot);
    return snapshot;
}

public static void UpdateSku(string[] skuCodes, int index, string newCode)
{
    skuCodes = new string[] { "TBK-999" };  // replace caller's array
}
```

What went wrong with "snapshot" and `UpdateSku`, and how do you fix both?

---

**Answer:**

```csharp
public static string[] GetSortedSkusForDisplay(string[] skuCodes)
{
    string[] snapshot = skuCodes;           // preserve original order
    Array.Sort(snapshot);
    return snapshot;
}

public static void UpdateSku(string[] skuCodes, int index, string newCode)
{
    skuCodes = new string[] { "TBK-999" };  // replace caller's array
}
```

What went wrong with "snapshot" and `UpdateSku`, and how do you fix both?

**Answer:** Assigning `snapshot = skuCodes` copies only the **reference**, not the elements — `Array.Sort` mutates the same heap array the caller still holds. Reassigning `skuCodes` inside `UpdateSku` rebinds a **local** parameter; the caller's variable still points at the original array.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Reference assignment masquerading as copy | Audit "original" reordered after sort — data integrity bug |
| Correctness | `skuCodes = new string[] { ... }` in void method | Caller's array unchanged; silent no-op |
| Design | `Array.Sort` is in-place | Any shared reference sees mutation |

**Fix (priority order):**

1. Snapshot before sort: `(string[])skuCodes.Clone()` or `skuCodes.ToArray()` — shallow copy of the array object (**Program.cs** Section 9–10, `DemonstrateSearchAndSort`).
2. To mutate caller's slots: `skuCodes[index] = newCode` with bounds check — no reassignment of the parameter.
3. If the caller must receive a new array instance, return `string[]` instead of `void`.
4. Document whether APIs mutate in place (`Array.Sort`, `Array.Reverse`) vs return new buffers.

```csharp
public static string[] GetSortedSkusForDisplay(string[] skuCodes)
{
    string[] sorted = (string[])skuCodes.Clone();
    Array.Sort(sorted);
    return sorted;
}

public static void UpdateSku(string[] skuCodes, int index, string newCode)
{
    if (index < 0 || index >= skuCodes.Length)
        throw new ArgumentOutOfRangeException(nameof(index));
    skuCodes[index] = newCode;
}
```

**Production takeaway:** Arrays are reference types — assignment shares storage until you `Clone`, `Copy`, or `Resize`. Karat stacks this with in-place `Array` helpers. See foundation **Arrays** — arrays as reference types.

---

---

#### Q4. (R) A pass-rate calculator tries to normalize scores in place during iteration:

```csharp
public static void NormalizeScores(int[] scores, int passingMark)
{
    foreach (int score in scores)
    {
        if (score < passingMark)
        {
            score = passingMark;   // bump failing scores to minimum pass
        }
    }
}
```

What fails (compile and/or runtime behavior), and what pattern should replace it?

---

**Answer:**

```csharp
public static void NormalizeScores(int[] scores, int passingMark)
{
    foreach (int score in scores)
    {
        if (score < passingMark)
        {
            score = passingMark;   // bump failing scores to minimum pass
        }
    }
}
```

What fails (compile and/or runtime behavior), and what pattern should replace it?

**Answer:** The `foreach` iteration variable is read-only — assigning to `score` does not compile (CS1654). Even if it compiled, mutating the iteration variable would not write back to the source array; slot updates require index access.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Assignment to `foreach` iteration variable | CS1654 — cannot modify foreach variable |
| Logic | Expectation that `score = …` updates the array | Silent failure even in languages that allow it — wrong mental model |
| Design | In-place mutation needs index or `for` | Failing scores never normalized |

**Fix (priority order):**

1. Use `for` with index when mutating slots (**Program.cs** Section 6 — "use for when you must mutate slots by index").
2. Guard index if length may be zero.
3. Alternative: `for (int i = 0; i < scores.Length; i++)` with `scores[i] = passingMark` when below threshold.

```csharp
public static void NormalizeScores(int[] scores, int passingMark)
{
    for (int i = 0; i < scores.Length; i++)
    {
        if (scores[i] < passingMark)
            scores[i] = passingMark;
    }
}
```

**Production takeaway:** `foreach` on arrays is for **read-only** traversal — production code that "fixes up" collections in place should use indexed loops or LINQ projecting to a new array. See **Program.cs** Section 6 foreach rules; Quick Reference — foreach iteration variable assignment.

---

---

#### Q5. (R) After migrating parallel arrays to `List<T>`, a report builder fails to compile. Review:

```csharp
public static string BuildScoreReport(List<string> subjects, int[] scores)
{
    StringBuilder report = new StringBuilder();
    for (int i = 0; i < subjects.Count; i++)
    {
        report.Append(subjects[i]);
        report.Append('=');
        report.Append(scores[i]);
    }

    int totalSlots = scores.Count;
    int lastIndex = scores.Count - 1;
    return report.ToString();
}
```

What errors appear, and what guard would you add before pairing `subjects` and `scores` by index?

---

**Answer:**

```csharp
public static string BuildScoreReport(List<string> subjects, int[] scores)
{
    StringBuilder report = new StringBuilder();
    for (int i = 0; i < subjects.Count; i++)
    {
        report.Append(subjects[i]);
        report.Append('=');
        report.Append(scores[i]);
    }

    int totalSlots = scores.Count;
    int lastIndex = scores.Count - 1;
    return report.ToString();
}
```

What errors appear, and what guard would you add before pairing `subjects` and `scores` by index?

**Answer:** Arrays expose `.Length` (property); `List<T>` exposes `.Count`. Using `scores.Count` on `int[]` fails at compile time (CS1061). Even after fixing that, parallel arrays/lists must have matching lengths before indexed pairing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `scores.Count` on `int[]` | CS1061 — arrays have `Length`, not `Count` |
| Runtime | Mismatched `subjects.Count` vs `scores.Length` | `IndexOutOfRangeException` or wrong pairings in report |
| Design | Mixed abstractions (`List<T>` + `T[]`) without length contract | Fragile after partial migration |

**Fix (priority order):**

1. Use `scores.Length` for arrays; `subjects.Count` for lists — or normalize both to same type.
2. Guard: `if (subjects.Count != scores.Length) throw new ArgumentException(...)` before the loop.
3. Prefer a single model (both `List<T>` or both arrays) when rows are paired by index — matches **Program.cs** Section 5 `BuildScoreReport` parallel-array pattern.
4. For empty collections, loop runs zero times — safe; still validate pairing when one side is non-empty.

```csharp
if (subjects.Count != scores.Length)
    throw new ArgumentException("subjects and scores must have the same length.");

int lastIndex = scores.Length - 1;
```

**Production takeaway:** `Length` vs `Count` is a common migration footgun — Karat tests whether you know which type owns which member. See **Program.cs** Section 14 List preview vs Section 3 `Length` property.

---

---

#### Q6. (M) A campus API returns course offerings per department. When every department is closed for the term, the outer array exists but inner arrays are empty. Review:

```csharp
public static int GetFirstCourseCode(string[][] departments)
{
    return departments[0][0].GetHashCode();  // quick non-null check
}

public static double AverageDepartmentSize(string[][] departments)
{
    int totalCourses = 0;
    foreach (string[] dept in departments)
    {
        totalCourses += dept.Length;
    }
    return (double)totalCourses / departments.Length;
}

public static bool HasAnyCourses(string[][] departments)
{
    return departments.Length > 0;  // API contract: non-empty when courses exist
}
```

What breaks when `departments` is `new string[0][]`, when the outer array has rows but every inner array is empty, and how would you harden these helpers?

---

---

### 10. Exception Handling - Done

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/10. Exception Handling - Done`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public static int GetFirstCourseCode(string[][] departments)
{
    return departments[0][0].GetHashCode();  // quick non-null check
}

public static double AverageDepartmentSize(string[][] departments)
{
    int totalCourses = 0;
    foreach (string[] dept in departments)
    {
        totalCourses += dept.Length;
    }
    return (double)totalCourses / departments.Length;
}

public static bool HasAnyCourses(string[][] departments)
{
    return departments.Length > 0;  // API contract: non-empty when courses exist
}
```

What breaks when `departments` is `new string[0][]`, when the outer array has rows but every inner array is empty, and how would you harden these helpers?

**Answer:** Zero-length outer arrays make `departments[0]` throw immediately; `AverageDepartmentSize` divides by zero when the outer length is 0; `HasAnyCourses` returns true whenever any department row exists, even if every inner array is empty — violating the stated contract.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `departments[0][0]` when outer `Length == 0` | `IndexOutOfRangeException` on first access |
| Runtime | `totalCourses / departments.Length` when outer length 0 | `DivideByZeroException` |
| Correctness | `HasAnyCourses` checks outer length only | Returns `true` for `[ [], [], [] ]` — false positive |
| Null | Uninitialized jagged row (`departments[i]` null) | `NullReferenceException` in `dept.Length` |

**Fix (priority order):**

1. Null-check parameter; treat null/empty outer as "no courses."
2. `GetFirstCourseCode`: scan for first non-null inner array with `Length > 0`, or return nullable / throw meaningful `InvalidOperationException`.
3. `AverageDepartmentSize`: if `departments.Length == 0`, return `0` or `double.NaN` — document contract.
4. `HasAnyCourses`: `departments.Any(d => d is { Length: > 0 })` or explicit nested loop — outer length alone is insufficient.

```csharp
public static bool HasAnyCourses(string[][] departments)
{
    if (departments is null || departments.Length == 0)
        return false;

    foreach (string[] dept in departments)
    {
        if (dept is { Length: > 0 })
            return true;
    }
    return false;
}
```

**Production takeaway:** Jagged arrays have **two** length dimensions — outer and per-row inner — and zero-length is valid at either level. Production APIs must define behavior for empty outer, empty inners, and null rows. See **Program.cs** Section 8 — `departments.Length` vs `departments[d].Length`; Section 4 bounds guard.

---

---

### 10. Exception Handling - Done

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/01. C# Basics - Done/10. Exception Handling - Done`

---

---

#### Q1. (R) A teammate adds logging around order validation before rethrowing. Review this method — what would you change and why?

```csharp
public void ValidateAndCharge(Order order, decimal walletBalance, ILogger logger)
{
    try
    {
        order.Validate();
        ProcessPayment(walletBalance, order.Total);
    }
    catch (Exception ex)
    {
        logger.LogError(ex.Message);
        throw ex;
    }
}
```

---

**Answer:**

**Answer:** Logging only `ex.Message` strips stack trace and inner exceptions from structured logs, and `throw ex` resets the stack trace to this catch block — so on-call engineers see the handler as the fault site instead of `Order.Validate()` or `ProcessPayment`. Log the full exception and rethrow with `throw;`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Observability | `LogError(ex.Message)` — no exception parameter | Serilog / Application Insights cannot index stack trace, type, or `InnerException` chain |
| Diagnostics | `throw ex` resets stack trace | Original throw site in **Program.cs** Section 11 (`LogAndRethrow`) is lost — same anti-pattern as `throw ex` in the inner try |
| Design | Broad `catch (Exception)` without recovery | Catches business declines (`InsufficientFundsException`) the same as bugs — may over-log expected paths |

**Fix (priority order):**

1. Replace `logger.LogError(ex.Message)` with `logger.LogError(ex, "ValidateAndCharge failed for order {OrderId}", order.OrderId)` so the logging provider captures the full exception object.
2. Replace `throw ex` with `throw;` to preserve the original stack trace (See foundation **Exception Handling** — `throw;` vs `throw ex`).
3. Catch narrower types where you can recover (`InsufficientFundsException` → user message) and let unexpected failures propagate after logging, or wrap with an inner exception: `throw new InvalidOrderException(order.OrderId, "Charge failed.", ex)`.

```csharp
catch (Exception ex)
{
    logger.LogError(ex, "ValidateAndCharge failed for order {OrderId}", order.OrderId);
    throw;
}
```

**Production takeaway:** This debrief snippet passes visual review but breaks the first production incident — Karat tests whether you diagnose observability and stack-trace preservation together, not just "log and rethrow."

---

---

#### Q2. (R) A nightly reconciliation job wraps payment-gateway calls like this. Support reports "job succeeded" but ledger rows are missing after gateway timeouts. What is wrong?

```csharp
public int ReconcilePendingOrders(IEnumerable<Order> orders)
{
    int processed = 0;

    foreach (Order order in orders)
    {
        try
        {
            ChargeViaSimulatedGateway(order.Total);
            processed++;
        }
        catch (Exception)
        {
            // gateway blips are common — keep going
        }
    }

    return processed;
}
```

---

**Answer:**

**Answer:** The empty `catch (Exception)` swallows every failure — including `TimeoutException` from the gateway — while the method still returns a success count, so callers and monitors believe orders were charged when they were not.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Empty catch swallows all exceptions | Silent data loss — matches **Program.cs** Section 16a anti-pattern |
| Operability | `processed` increments only on success but job exits normally | Dashboards show green runs; finance finds missing ledger entries |
| Diagnostics | No log of SKU/order id, exception type, or inner cause | Cannot distinguish transient timeout from permanent misconfiguration |

**Fix (priority order):**

1. Never use empty catch — at minimum log with full exception and order context, then continue or fail the batch explicitly.
2. Decide policy: **fail fast** (stop batch, return non-zero exit) vs **partial success** (track failed order ids, emit summary metric) — document which.
3. Catch specific recoverable types (`TimeoutException`) separately from programming errors; rethrow or aggregate unexpected exceptions.
4. For expected gateway timeouts, use retry with backoff — not silent skip.

```csharp
catch (Exception ex)
{
    logger.LogError(ex, "Reconcile failed for order {OrderId}", order.OrderId);
    failedOrderIds.Add(order.OrderId);
}
// return processed + failed lists; exit non-zero if any failed
```

**Production takeaway:** Swallowing exceptions makes batch jobs the worst kind of green — the process completed, but business state is wrong. See **Program.cs** Section 16 — swallow demo vs explicit result strings.

---

---

#### Q3. (R) Audit entries must always be closed, even when `WriteEntry` throws. A junior developer refactors Section 15 without `using`. Review:

```csharp
public static string WriteAuditTrail(string orderId)
{
    AuditLogWriter audit = new AuditLogWriter();
    audit.WriteEntry($"Payment attempt for {orderId}");
    string trail = audit.LastEntry;
    audit.Dispose();
    return trail;
}
```

What can go wrong in production, and how would you fix it?

---

**Answer:**

```csharp
public static string WriteAuditTrail(string orderId)
{
    AuditLogWriter audit = new AuditLogWriter();
    audit.WriteEntry($"Payment attempt for {orderId}");
    string trail = audit.LastEntry;
    audit.Dispose();
    return trail;
}
```

What can go wrong in production, and how would you fix it?

**Answer:** If `WriteEntry` throws, `Dispose()` never runs — the audit writer stays open, `[closed]` is never appended, and native handles (file streams in real code) leak until GC finalization. Use `using`, a `try/finally`, or `try/finally` with explicit `Dispose()` so cleanup runs on every path.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource leak | `Dispose()` only on happy path | Unclosed streams, held locks, incomplete audit flush |
| Correctness | `_disposed` guard never set on failure path | Subsequent use may write to a half-closed resource |
| Maintainability | Manual dispose easy to break on new return/throw | **Program.cs** Section 4 + 15 teach compiler-generated `finally` via `using` for this reason |

**Fix (priority order):**

1. Restore `using (AuditLogWriter audit = new AuditLogWriter()) { ... }` — compiler emits `try/finally` calling `Dispose()` even when `WriteEntry` throws.
2. Alternatively explicit `try/finally` with null check if construction itself can fail.
3. Prefer C# 8 `using AuditLogWriter audit = new();` when style allows — same guarantee.

```csharp
using (AuditLogWriter audit = new AuditLogWriter())
{
    audit.WriteEntry($"Payment attempt for {orderId}");
    return audit.LastEntry;
} // Dispose always runs
```

**Production takeaway:** `finally` and `IDisposable` exist because success-only cleanup fails under real faults — payment and audit paths are exactly where this bites.

---

---

#### Q4. (D) A team introduces `InvalidOrderException`, `InsufficientFundsException`, and `CustomerNotFoundException` for every validation failure — including null method parameters and missing optional query filters. When is a custom domain exception the right choice vs `ArgumentException`, a result type, or no throw at all?

---

**Answer:**

**Answer:** Custom exceptions like `InvalidOrderException` and `InsufficientFundsException` fit **business-rule violations on domain entities** where callers catch by type and need structured properties (`OrderId`, `RequestedAmount`). They are the wrong tool for bad parameters, expected "not found" lookups, or control flow.

- **Use domain exceptions** when the rule is part of the entity's integrity (`Order.Validate()` throwing `InvalidOrderException` for quantity ≤ 0 — **Program.cs** Section 3) and upper layers map them to HTTP 400/402 or user-facing decline messages.
- **Use `ArgumentNullException` / `ArgumentException`** for invalid method inputs (null `order`, negative page size) — preconditions, not business outcomes.
- **Use return / `Try*` / result types** for expected outcomes callers handle often (optional filter absent, SKU not in catalog) — **Program.cs** Section 16c `ValidateWithResult` vs throw.
- **Do not inherit `ApplicationException`** — derive from `Exception` directly (**Program.cs** Section 1).
- **Avoid exception explosion** — three similar types with only message differences force catch clutter; one type with an error code enum may suffice.

**Production takeaway:** Custom exceptions are for exceptional **domain** state, not a replacement for validation attributes, result objects, or HTTP semantics — misuse slows hot paths and obscures which failures are operational vs programmer errors.

---

---

#### Q5. (R) Two implementations look up a product by SKU. One is in code review. Which approach would you approve for a catalog service called millions of times per day, and why?

```csharp
// A
public Product GetProduct(string sku)
{
    if (!_catalog.TryGetValue(sku, out Product? product))
        throw new KeyNotFoundException($"SKU {sku} not found.");
    return product;
}

// B
public bool TryGetProduct(string sku, out Product? product) =>
    _catalog.TryGetValue(sku, out product);
```

---

**Answer:**

**Answer:** Approve **B** (`TryGetProduct`) for the high-volume lookup path — missing SKU is an expected outcome, not an exceptional one. Reserve **A** only when absence truly indicates a programmer or configuration error that should fail fast.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Control flow | **A** uses exceptions for "not found" | Exception throw/catch is orders of magnitude slower on hot paths; clutters logs with stack traces for normal browsing |
| API clarity | **A** forces every caller into try/catch | **B** matches **Program.cs** Section 16c — return/Try* for expected flow |
| Semantics | `KeyNotFoundException` is a BCL type | Callers cannot distinguish catalog miss from dictionary bug without message parsing |

**Fix (priority order):**

1. Expose `TryGetProduct` (or `Product? GetProductOrDefault`) as the primary API for user-driven lookups.
2. If **A** remains for internal invariants ("SKU must exist after order commit"), document that contract and keep it off the request hot path.
3. At the API boundary, map `false` from TryGet to 404 — not an unhandled exception.

**Production takeaway:** Exceptions are for **exceptional** conditions — the chapter's payment decline (`InsufficientFundsException`) is appropriate; "customer typed wrong SKU" is not.

---

---

#### Q6. (P) This console chapter lets `InvalidOrderException` bubble out of `Main` when validation fails. In an ASP.NET Core API, the same unhandled domain exception currently returns a raw 500 HTML page. What centralized pattern replaces scattered try/catch in every controller, and what must differ between Development and Production responses?

---

**Answer:**

**Answer:** Register a **global exception handler** (`IExceptionHandler` + `AddExceptionHandler<T>()` and `UseExceptionHandler()` in .NET 8+, or exception-handling middleware) once in `Program.cs` so unhandled exceptions from any endpoint become a uniform JSON response — controllers stay thin and throw domain types like this chapter teaches.

- **Map domain types deliberately:** `InvalidOrderException` → 400 + `ProblemDetails` with safe message; `InsufficientFundsException` → 402 or 400 with decline detail; unexpected → 500.
- **Development:** richer body — exception detail, stack trace, or `DeveloperExceptionPage` for local debugging (never expose raw stacks to external clients).
- **Production:** log full exception with `LogError(ex, "...")` including `TraceIdentifier`; return RFC 7807 `ProblemDetails` without internal stack or connection strings — same rule as **Program.cs** Section 13 (log `StackTrace` internally, not in UI).
- **Preserve stacks upstream:** middleware sees the original trace only if lower layers used `throw;` or wrapped with `inner` — not `throw ex` (Q1).
- **Avoid duplicating catch blocks** in every action; handle expected failures locally only when the response shape differs (e.g., 201 vs 404).

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// ...
app.UseExceptionHandler();
```

**Production takeaway:** Console `Main` terminating on unhandled exceptions is the same failure mode as an unhandled API exception — centralized handling is the web equivalent of "one place decides log + user-safe message + status code." Full implementation lives in **05. ASP.NET Core/10. Exception Handling**.

---

---
