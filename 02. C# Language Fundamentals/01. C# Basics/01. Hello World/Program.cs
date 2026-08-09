/*
 * TOPIC: Your first C# program — the smallest useful program that prints a
 *        greeting to the console and shows the skeleton every console app shares.
 *
 * WHY IT MATTERS:
 *   Every C# developer starts here. Hello World proves your toolchain works,
 *   introduces namespace → class → Main → output, and gives you a template for
 *   every console program that follows.
 *
 * WHAT YOU WILL LEARN:
 *   1. What Hello World is and what happens when a program runs
 *   2. Program structure: using, namespace, class, Main, Console output
 *   3. using directives and why ImplicitUsings is disabled in this repo
 *   4. File-scoped namespaces and the Program class
 *   5. Main as the entry point — static, void, and string[] args
 *   6. Console.WriteLine vs Console.Write and basic string literals
 *   7. Comment forms: single-line, multi-line, and XML (preview)
 *   8. The .csproj project file and dotnet build / run
 *   9. Previews: command-line arguments and top-level statements
 */

/*
 * SECTION 1: WHAT IS "HELLO WORLD"?
 *
 * Hello World is the traditional first program in any language. It does one
 * thing: display a short greeting (usually "Hello, World!") in the console.
 *
 * A CONSOLE is the text window where a command-line program shows output.
 * When you run the program, messages appear line by line in that window.
 *
 * A valid Hello World program must:
 *   - Use the smallest correct C# program structure
 *   - Print at least one greeting with Console.WriteLine
 *   - Start execution at the Main method
 */

/*
 * SECTION 2: PROGRAM STRUCTURE — THE BUILDING BLOCKS
 *
 * Every beginner console program in C# follows the same pattern:
 *
 *   Layer        | Role
 *   -------------|--------------------------------------------------------
 *   using        | Imports a namespace so type names resolve
 *   namespace    | Groups related code; avoids name clashes
 *   class        | Container that holds methods (and later, data)
 *   Main         | Entry point — execution starts here
 *   WriteLine    | Sends one line of text to the console
 *
 * Execution flow when this program runs:
 *
 *   1. The runtime locates Main inside Program
 *   2. Main runs top to bottom, calling helper methods in order
 *   3. Each Console call sends text to standard output
 *   4. Main returns → the process exits
 *
 * Main is never called from your application code. The .NET runtime invokes
 * it automatically at startup.
 */

using System;

/*
 * SECTION 3: USING DIRECTIVES — IMPORTING NAMESPACES
 *
 * The .NET class library organizes types into NAMESPACES — hierarchical name
 * groups. Console, String, and Exception live in the System namespace.
 *
 * SYNTAX (top of file, before namespace):
 *   using NamespaceName;
 *
 * WITHOUT using System:
 *   System.Console.WriteLine("Hello, World!");   // full name — always works
 *
 * WITH using System:
 *   Console.WriteLine("Hello, World!");          // short name — needs the using
 *
 * COMPILE ERROR without the using:
 *   Console.WriteLine("Hi");   → CS0103: The name 'Console' does not exist
 *                                in the current context
 *
 * --- ImplicitUsings (SDK feature) ---
 *
 * Modern .NET projects can set  <ImplicitUsings>enable</ImplicitUsings>  in
 * the .csproj. The SDK then generates obj/GlobalUsings.g.cs with common usings
 * you never see in source.
 *
 * Typical implicit usings for a console app (net8.0):
 *
 *   Namespace                  | Why it is included
 *   ---------------------------|------------------------------------------
 *   System                     | Console, String, Exception, Convert, …
 *   System.Collections.Generic | List<T>, Dictionary<TKey,TValue>, …
 *   System.IO                  | File, Stream, Path, …
 *   System.Linq                | LINQ extension methods (Where, Select, …)
 *
 * This HelloWorld project sets  <ImplicitUsings>disable</ImplicitUsings>  so
 * every import is visible while you learn. Add a using only when the file
 * actually needs types from that namespace — here, System is enough for Console.
 *
 * Fully qualified names (System.Console) work without a using, but usings keep
 * everyday code readable.
 */

namespace HelloWorld;

/*
 * SECTION 4: NAMESPACE
 *
 * A namespace organizes code into a logical group — like a folder for names.
 * Two libraries can both define a class called Program if they use different
 * namespaces.
 *
 * SYNTAX (file-scoped — C# 10+):
 *   namespace NamespaceName;
 *
 * The semicolon means everything below belongs to HelloWorld without extra
 * indentation braces. Older block syntax still works:
 *
 *   namespace HelloWorld
 *   {
 *       // types here
 *   }
 *
 * Matching the namespace to the project name (HelloWorld) is a common convention.
 * RootNamespace in the .csproj defaults to the same name unless you override it.
 */

public class Program
{
    /*
     * SECTION 5: THE PROGRAM CLASS
     *
     * A class is a blueprint — a container for methods and, in larger programs,
     * fields and properties. Even the smallest C# program needs at least one class.
     *
     * SYNTAX:
     *   public class ClassName
     *   {
     *       // members
     *   }
     *
     * MODIFIERS ON THIS LINE:
     *   public  → accessible from anywhere (runtime must see the type)
     *   class   → declares a reference type named Program
     *
     * WHY "Program"?
     *   Conventional name for the class that holds Main in small console projects.
     *   You may rename it, but Program is universally recognized in templates.
     */

    /*
     * SECTION 6: MAIN — ENTRY POINT
     *
     * Main is where execution begins. A console app needs a method with this
     * signature (or an equivalent the compiler recognizes):
     *
     *   public static void Main(string[] args)
     *
     * PARTS OF THE SIGNATURE:
     *
     *   Part          | Meaning
     *   --------------|---------------------------------------------------
     *   public        | Callable by the runtime
     *   static        | Belongs to the class — no object instance required
     *   void          | Returns nothing when it finishes
     *   Main          | Conventional entry-point name (capital M)
     *   string[] args | Command-line tokens (optional to use)
     *
     * static — the runtime starts before any object exists, so Main must be
     * static. void — Main performs side effects (printing) and exits; it does
     * not need to return a value to a caller.
     *
     * COMPILE ERROR:
     *   Two Main methods in one project → CS0017 (more than one entry point).
     *
     * Main below orchestrates the chapter demo by calling private helpers —
     * each helper sits under its own section comment with the code it teaches.
     */
    public static void Main(string[] args)
    {
        PrintClassicGreeting();           // classic WriteLine greeting
        DemonstrateWriteLineVsWrite();    // Write vs WriteLine on same line
        DemonstrateStringLiterals();      // escape sequences in strings
        DemonstrateCommentsInOutput();    // comments are not printed
        PreviewCommandLineArgs(args);     // args[] from dotnet run --
    }

    /*
     * SECTION 7: CONSOLE WRITELINE — CLASSIC GREETING
     *
     * Console is a class in System that writes to standard output — the text
     * window shown when the program runs. The using System; directive at the
     * top of this file is what allows the short name Console.
     *
     * Console.WriteLine prints text and moves to the next line. For readable
     * output, WriteLine is the default choice for whole messages.
     */
    private static void PrintClassicGreeting()
    {
        Console.WriteLine("Hello, World!");              // each call prints one full line
        Console.WriteLine("Welcome to the world of C#!"); // cursor moves to next line after each
    }

    /*
     * SECTION 8: WRITELINE VS WRITE
     *
     *   Method              | Behavior
     *   --------------------|--------------------------------------------
     *   Console.WriteLine   | Prints text, then advances to the next line
     *   Console.Write       | Prints text; cursor stays on the same line
     *
     * Use Write when several calls should appear on one line; finish with
     * WriteLine so the next output starts on a fresh row.
     *
     * COVERED IN DETAIL LATER → 03. Input & Output
     *   (ReadLine, formatting, and combining read/write patterns)
     */
    private static void DemonstrateWriteLineVsWrite()
    {
        Console.Write("Ready to learn C#? "); // Write — no newline; stays on same row
        Console.WriteLine("Let's go!");       // WriteLine — finishes the line and advances
    }

    /*
     * SECTION 9: STRING LITERALS — TEXT IN SOURCE CODE
     *
     * A string literal is text written directly in source, wrapped in double
     * quotes. The characters between quotes are sent to the console unchanged
     * (except for escape sequences below).
     *
     * RULES:
     *   - Opening and closing quote must match: "like this"
     *   - Single quotes 'x' are for char, not string → CS1012 if used for text
     *   - A string can be empty: ""
     *
     * COMMON ESCAPE SEQUENCES (backslash + character):
     *
     *   Sequence | Prints        | Use case
     *   ---------|---------------|----------------------------------
     *   \n       | new line      | line break inside one WriteLine
     *   \t       | tab           | column alignment
     *   \"       | double quote  | quote character inside the text
     *   \\       | backslash     | literal backslash
     *
     * COVERED IN DETAIL LATER → 06. Strings
     *   (interpolation, verbatim strings, String methods, StringBuilder)
     */
    private static void DemonstrateStringLiterals()
    {
        Console.WriteLine("Line 1\nLine 2");              // \n — line break inside one string
        Console.WriteLine("She said, \"Hello!\"");        // \" — literal double quote in text
        Console.WriteLine("Path: C:\\Projects\\HelloWorld"); // \\ — literal backslash in path
        Console.WriteLine("Columns:\tName\tScore");       // \t — tab columns
    }

    /*
     * SECTION 10: COMMENTS IN C#
     *
     * Comments are notes for humans — the compiler ignores them.
     *
     * THREE FORMS:
     *
     *   Form             | Syntax              | Best for
     *   -----------------|---------------------|----------------------------
     *   Single-line      | // text to EOL       | Short notes on the next line
     *   Multi-line       | / * ... * /          | Sections, tables, blocks
     *   XML doc (preview)| /// summary          | Public API docs (tools)
     *
     * RULES:
     *   - // runs from the slashes to end of line
     *   - / * ... * / spans lines; cannot nest another block inside
     *   - /// sits immediately above a type or member for documentation tools
     *
     * COVERED IN DETAIL LATER → 07. Methods
     *   (XML docs on parameters, return values, and public APIs)
     */
    private static void DemonstrateCommentsInOutput()
    {
        // This comment does not print — only the string below reaches the console.
        Console.WriteLine("Comments do not appear in console output.");
    }

    /*
     * SECTION 11: COMMAND-LINE ARGUMENTS (PREVIEW)
     *
     * string[] args receives tokens typed after the program name in a terminal:
     *
     *   dotnet run -- Alice 42
     *                    ^^^^^^ become args[0] and args[1]
     *
     * Hello World only reports how many args were passed. Reading and acting on
     * each token comes in a later chapter.
     *
     * COVERED IN DETAIL LATER → 03. Input & Output and 07. Methods
     *   (parse args, validate input, pass values into helper methods)
     */
    private static void PreviewCommandLineArgs(string[] args)
    {
        Console.WriteLine("Arguments received: " + args.Length); // Length — count of tokens after program name
    }
}

/*
 * SECTION 12: THE .CSPROJ PROJECT FILE
 *
 * A C# program is not run as raw .cs text. The project file (HelloWorld.csproj)
 * tells the SDK how to compile and link your source into a runnable assembly.
 *
 * HelloWorld.csproj in this folder:
 *
 *   Element              | Value in this chapter | Purpose
 *   ---------------------|-----------------------|----------------------------------
 *   Sdk                  | Microsoft.NET.Sdk     | Standard .NET project SDK
 *   OutputType           | Exe                   | Console executable (not a library)
 *   TargetFramework      | net8.0                | .NET 8 runtime and API surface
 *   ImplicitUsings       | disable               | Usings must appear in source
 *   Nullable             | enable                | Compiler warns on null mistakes
 *   RootNamespace        | HelloWorld            | Default namespace for new files
 *
 * The SDK implicitly includes every .cs file in this folder — no manual list
 * of Program.cs is required. Adding another .cs file automatically joins the
 * build unless you exclude it explicitly.
 *
 * OutputType Exe tells the compiler to produce an executable with one entry
 * point (Main). A library project would use OutputType Library instead.
 */

/*
 * SECTION 13: BUILD AND RUN WITH DOTNET CLI
 *
 * FROM THE PROJECT FOLDER (where HelloWorld.csproj lives):
 *
 *   Command              | What it does
 *   ---------------------|--------------------------------------------------
 *   dotnet build         | Compiles all .cs files; reports errors/warnings
 *   dotnet run           | Builds (if needed) and runs the program
 *   dotnet run -- arg1   | Runs and passes arg1 into Main's args array
 *
 * TYPICAL FLOW:
 *   1. Edit Program.cs
 *   2. dotnet build  →  fix compile errors (CS#### messages)
 *   3. dotnet run    →  see console output
 *   4. Repeat
 *
 * A successful build prints "Build succeeded" with 0 warnings. This repo
 * targets zero warnings on every tutorial project.
 */

/*
 * SECTION 14: TOP-LEVEL STATEMENTS (PREVIEW)
 *
 * C# 9+ allows a minimal program without an explicit class or Main:
 *
 *   // TopLevelSample.cs (not used in this repo)
 *   Console.WriteLine("Hello!");
 *
 * The compiler generates a hidden Program class and Main method for you.
 *
 * This repository keeps explicit namespace + class Program + Main so you
 * always see the full skeleton until the structure is second nature.
 */

/*
 * QUICK REFERENCE — HELLO WORLD CHEAT SHEET
 *
 * SMALLEST CONSOLE PROGRAM SHAPE
 *
 *   using System;
 *
 *   namespace MyApp;
 *
 *   public class Program
 *   {
 *       public static void Main(string[] args)
 *       {
 *           Console.WriteLine("Hello, World!");
 *       }
 *   }
 *
 * KEY TERMS:
 *
 *   Term            | One-line meaning
 *   ----------------|--------------------------------------------------
 *   using           | Imports a namespace so short type names resolve
 *   namespace       | Logical name group for your code
 *   class           | Container for methods and data
 *   Main            | Method where the runtime starts execution
 *   Console         | System class for console input/output
 *   WriteLine       | Prints a line of text to the console
 *   String literal  | Text in double quotes, e.g. "Hello, World!"
 *   .csproj         | XML project file — framework, output type, settings
 *
 * DOTNET CLI (from project folder):
 *
 *   dotnet build   → compile
 *   dotnet run     → compile + execute
 *
 * COMMON BEGINNER MISTAKES:
 *
 *   Mistake                          | Result
 *   ---------------------------------|----------------------------------
 *   Missing using System;            | CS0103 — Console does not exist
 *   Missing semicolon after statement| CS1002 compile error
 *   Mismatched braces { }            | CS1513 / CS1022 compile error
 *   Single quotes for text           | CS1012 — char vs string
 *   Two Main methods in one project  | CS0017 — multiple entry points
 *
 * WHAT COMES NEXT:
 *   02. Data Types and Variables — variables and built-in types
 *   03. Input & Output — Console.ReadLine and formatted output
 *   06. Strings — full string manipulation beyond literals
 */
