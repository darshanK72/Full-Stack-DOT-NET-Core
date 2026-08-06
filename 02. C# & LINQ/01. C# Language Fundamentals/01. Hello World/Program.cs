/*
 * =============================================================================
 * 01. HELLO WORLD IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Your first C# program — the smallest useful program that prints a
 *        greeting to the console.
 *
 * WHY IT MATTERS:
 *   Every C# developer starts here. Hello World proves that your tools work,
 *   introduces the skeleton every console program shares, and shows how text
 *   reaches the screen. Once you understand this structure, you have a template
 *   for all future programs.
 *
 * WHAT YOU WILL LEARN:
 *   1. What Hello World is and what running a program does
 *   2. using directives — importing namespaces (and why ImplicitUsings is off)
 *   3. The program skeleton: namespace, class, Main, and output
 *   4. How Console.WriteLine and Console.Write print text
 *   5. Comments: single-line, multi-line, and XML (preview)
 *   6. How to build and run with dotnet CLI
 *   7. Previews: command-line args and top-level statements
 *
 * =============================================================================
 */

/*
 * =========================================================================
 * SECTION 1: WHAT IS "HELLO WORLD"?
 * =========================================================================
 *
 * Hello World is the traditional first program in any language. It does one
 * thing: display a short greeting (usually "Hello, World!") in the console.
 *
 * A CONSOLE is the text window where a command-line program shows output.
 * When the program runs, that window displays output and each message appears
 * on its own line.
 *
 * A Hello World program must:
 *   - Use the smallest valid C# program structure
 *   - Print at least one greeting using Console.WriteLine
 *   - Start execution at the Main method
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: PROGRAM STRUCTURE — THE BUILDING BLOCKS
 * =========================================================================
 *
 * Every beginner console program in C# follows the same pattern:
 *
 *   ┌─────────────────────────────────────────────────────────────┐
 *   │  using       →  imports a namespace so types resolve        │
 *   │  namespace   →  groups related code; avoids name clashes    │
 *   │  class       →  container that holds methods and data       │
 *   │  Main        →  entry point — execution starts here         │
 *   │  WriteLine   →  sends text to the console output            │
 *   └─────────────────────────────────────────────────────────────┘
 *
 * Execution flow when the program runs:
 *
 *   1. The runtime finds the Main method inside Program
 *   2. Main runs line by line, top to bottom
 *   3. Each Console.WriteLine sends one line of text to the console
 *   4. Main finishes → the program exits
 *
 * Main is never called from application code. The .NET runtime invokes it
 * automatically at startup.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 3: USING DIRECTIVES — IMPORTING NAMESPACES
 * =========================================================================
 *
 * The .NET class library organizes thousands of types into NAMESPACES —
 * hierarchical name groups. Console, String, and Exception all live inside
 * the System namespace.
 *
 * A using directive tells the compiler: "when you see a type name, also
 * look inside this namespace to resolve it."
 *
 * SYNTAX (placed at the top of the file, before namespace):
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
 * the .csproj file. When enabled, the SDK silently generates a hidden file
 * (obj/GlobalUsings.g.cs) that adds common using directives for you — you
 * never see them in Program.cs.
 *
 * Typical implicit usings for a console app (net8.0):
 *
 *   Namespace                  | Why it is included
 *   ---------------------------|------------------------------------------
 *   System                     | Console, String, Exception, Convert, …
 *   System.Collections.Generic | List<T>, Dictionary<TKey,TValue>, …
 *   System.IO                  | File, Stream, Path, …
 *   System.Linq                | LINQ extension methods (Where, Select, …)
 *   System.Net.Http            | HttpClient
 *   System.Threading           | Thread, Monitor, …
 *   System.Threading.Tasks     | Task, async/await support
 *
 * This HelloWorld project sets  <ImplicitUsings>disable</ImplicitUsings>  so
 * every namespace import appears explicitly in source code. That makes it
 * clear which library each type comes from — especially important while
 * learning. Add a using only when the program actually needs types from that
 * namespace; Hello World needs just System for Console.
 *
 * Fully qualified names (System.Console) always work without a using, but
 * using directives keep everyday code readable.
 * -------------------------------------------------------------------------
 */

using System;  // imports System — makes Console, String, etc. available

/*
 * =========================================================================
 * SECTION 4: NAMESPACE
 * =========================================================================
 *
 * A namespace organizes code into a logical group — like a folder for names.
 * Two different libraries can both define a class called "Program" as long
 * as they live in different namespaces.
 *
 * SYNTAX:
 *   namespace NamespaceName;
 *
 * The semicolon after the name is file-scoped namespace syntax (C# 10+).
 * All types declared below are grouped under the HelloWorld namespace.
 *
 * Matching the namespace to the project name (HelloWorld) is a common convention.
 * -------------------------------------------------------------------------
 */

namespace HelloWorld;

/*
 * =========================================================================
 * SECTION 5: THE PROGRAM CLASS
 * =========================================================================
 *
 * A class is a blueprint — a container for methods (actions) and, in larger
 * programs, data (fields and properties). Even the simplest C# program needs
 * at least one class.
 *
 * SYNTAX:
 *   public class ClassName
 *   {
 *       // methods and members go inside the braces
 *   }
 *
 * MODIFIERS ON THIS LINE:
 *   public  →  accessible from anywhere (required for the runtime to find it)
 *   class   →  declares a reference type named Program
 *
 * WHY "Program"?
 *   It is the conventional name for the class that holds Main in small
 *   console projects. You may rename it, but Program is universally recognized.
 * -------------------------------------------------------------------------
 */

public class Program
{
    /*
     * =====================================================================
     * SECTION 6: THE MAIN METHOD — ENTRY POINT
     * =====================================================================
     *
     * Main is where execution begins. The runtime looks for a method with
     * this exact signature in a console application:
     *
     *   public static void Main(string[] args)
     *
     * PARTS OF THE SIGNATURE:
     *
     *   Modifier   | Keyword | Meaning
     *   -----------|---------|------------------------------------------
     *   public     | access  | callable by the runtime
     *   static     | keyword | belongs to the class, not an instance
     *   void       | return  | returns nothing when it finishes
     *   Main       | name    | conventional entry-point name (capital M)
     *   string[] args      | optional command-line text (not used here)
     *
     * static — why it matters:
     *   The runtime starts your program before any object exists. Main must
     *   be static so it can run without creating an instance of Program first.
     *
     * void — why it matters:
     *   Main does its work (print greetings) and simply ends. It does not
     *   need to send a value back to the caller.
     *
     * string[] args:
     *   An array of text tokens passed when launching from a terminal.
     *   A basic Hello World program does not use args yet.
     *
     *   COVERED IN DETAIL LATER → 07. Methods
     *     (reading args, passing values when you run the program)
     *
     * COMPILE ERROR NOTE:
     *   A console app must have exactly one entry point. Defining two Main
     *   methods in the same project causes CS0017 ("program has more than one
     *   entry point defined").
     * ---------------------------------------------------------------------
     */

    public static void Main(string[] args)
    {
        /*
         * =================================================================
         * SECTION 7: CONSOLE WRITELINE — PRINTING OUTPUT
         * =================================================================
         *
         * Console is a class in the System namespace that talks to the
         * standard output stream — the text window displayed when the program
         * runs. The using System; directive at the top of this file is what
         * allows the short name Console instead of System.Console.
         *
         * WriteLine vs Write:
         *
         *   Method              | Behavior
         *   --------------------|--------------------------------------------
         *   Console.WriteLine   | prints text, then moves to the next line
         *   Console.Write      | prints text, cursor stays on the same line
         *
         * For greetings and readable output, WriteLine is almost always what
         * you want. Each call produces one line in the console.
         *
         * WriteLine accepts any value that can become text. Strings are the
         * most common for literal messages like Hello World.
         *
         * COVERED IN DETAIL LATER → 03. Input & Output
         *   (Console.ReadLine, reading typed input, command-line args)
         * -----------------------------------------------------------------
         */

        Console.WriteLine("Hello, World!");                   // classic first greeting — double-quoted string literal
        Console.WriteLine("Welcome to the world of C#!");     // second line — each WriteLine prints on its own row

        Console.Write("Ready to learn C#? ");                 // Write — no newline; cursor stays on the same line
        Console.WriteLine("Let's go!");                       // WriteLine — completes the line started by Write

        /*
         * =================================================================
         * SECTION 8: STRING LITERALS — THE TEXT YOU PRINT
         * =================================================================
         *
         * A string literal is text written directly in source code, wrapped
         * in double quotes. The message between the quotes is sent to the
         * console unchanged (except for escape sequences — see below).
         *
         * RULES:
         *   - Opening and closing quote must match: "like this"
         *   - Single quotes 'like this' are for char, not string → compile error
         *   - A string can be empty: ""
         *
         * COMMON ESCAPE SEQUENCES (backslash + character inside a string):
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
         * -----------------------------------------------------------------
         */

        Console.WriteLine("Line 1\nLine 2");                  // \n — new line inside one string
        Console.WriteLine("She said, \"Hello!\"");            // \" — double quote inside the text
        Console.WriteLine("Path: C:\\Projects\\HelloWorld");  // \\ — literal backslash in output

        /*
         * =================================================================
         * SECTION 9: COMMENTS IN C#
         * =================================================================
         *
         * Comments are notes for humans — the compiler ignores them. This
         * entire tutorial uses comments to teach; your production code uses
         * them to explain non-obvious intent.
         *
         * THREE FORMS:
         *
         *   Form            | Syntax              | Best for
         *   ----------------|---------------------|-------------------------------
         *   Single-line     | // text to EOL       | Short notes on the next line(s)
         *   Multi-line      | / * ... * /           | Sections, tables, longer blocks
         *   XML doc (preview)| /// summary          | Public API documentation (tools)
         *
         * RULES:
         *   - // runs from the slashes to the end of the line
         *   - / * ... * / can span many lines; cannot nest another block inside
         *   - /// sits immediately above a type or member; builds documentation
         *
         * XML documentation is used heavily in libraries and IntelliSense.
         * You will write /// comments when you publish reusable methods.
         *
         * COVERED IN DETAIL LATER → 07. Methods
         *   (XML docs on parameters, return values, and public APIs)
         * -----------------------------------------------------------------
         */

        Console.WriteLine("Comments do not appear in console output.");  // this line runs; the comment does not print

        /*
         * =================================================================
         * SECTION 10: COMMAND-LINE ARGUMENTS (PREVIEW)
         * =================================================================
         *
         * The string[] args parameter receives tokens typed after the program
         * name when launched from a terminal:
         *
         *   dotnet run -- Alice 42
         *                    ^^^^^^ these become args[0] and args[1]
         *
         * Hello World only needs to know that args exists and how many values
         * were passed. Reading and acting on each arg comes later.
         *
         * COVERED IN DETAIL LATER → 07. Methods
         *   (parse args, validate input, pass values into helper methods)
         * -----------------------------------------------------------------
         */

        Console.WriteLine($"Arguments received: {args.Length}");  // 0 when run with no extra tokens
    }
}

/*
 * =========================================================================
 * SECTION 11: BUILD AND RUN WORKFLOW
 * =========================================================================
 *
 * A C# source file is not executed directly. The toolchain compiles it
 * into an assembly, then the runtime executes that assembly.
 *
 * FROM THE PROJECT FOLDER (where the .csproj lives):
 *
 *   Command              | What it does
 *   ---------------------|--------------------------------------------------
 *   dotnet build         | Compiles all .cs files; reports errors/warnings
 *   dotnet run           | Builds (if needed) and runs the program
 *   dotnet run -- arg1   | Runs and passes arg1 into Main's args array
 *
 * TYPICAL FLOW:
 *   1. Edit Program.cs in your editor
 *   2. dotnet build  →  fix any compile errors (red squiggles / CS#### messages)
 *   3. dotnet run    →  see console output
 *   4. Repeat
 *
 * A successful build prints "Build succeeded" with 0 warnings. Treat warnings
 * seriously — this repo targets 0 warnings on every tutorial project.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 12: TOP-LEVEL STATEMENTS (PREVIEW)
 * =========================================================================
 *
 * C# 9+ allows a minimal program without an explicit class or Main:
 *
 *   // TopLevelSample.cs (not used in this repo)
 *   Console.WriteLine("Hello!");
 *
 * The compiler generates a hidden Program class and Main method for you.
 *
 * This repository keeps explicit Main so you always see the full skeleton.
 * Every chapter uses namespace + class Program + Main until you are fluent
 * with the structure.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * QUICK REFERENCE — HELLO WORLD CHEAT SHEET
 * =========================================================================
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
 *   using           | Imports a namespace so type names resolve without full path
 *   Namespace       | Logical name group for your code and library types
 *   Class           | Container for methods and data
 *   Main            | Method where the runtime starts execution
 *   Console         | Built-in class (System namespace) for console I/O
 *   WriteLine       | Prints a line of text to the console
 *   String literal  | Text in double quotes, e.g. "Hello, World!"
 *   // comment       | Single-line note ignored by the compiler
 *   / * comment * /    | Multi-line note ignored by the compiler
 *
 * DOTNET CLI (run from project folder):
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
 *   Mismatched or missing braces { } | CS1513 / CS1022 compile error
 *   Using single quotes for text     | CS1012 — char vs string
 *   Two Main methods in one project  | CS0017 — multiple entry points
 *
 * WHAT COMES NEXT:
 *   02. Data Types and Variables — variables and built-in types
 *   03. Input & Output — Console.ReadLine and command-line arguments
 *   06. Strings — full string manipulation beyond literals
 *
 * =========================================================================
 */
