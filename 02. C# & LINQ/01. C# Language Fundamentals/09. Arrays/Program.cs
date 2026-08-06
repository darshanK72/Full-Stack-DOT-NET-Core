/*
 * =============================================================================
 * 09. ARRAYS IN C# — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Fixed-size collections stored in contiguous memory — single-dimensional,
 *        multidimensional, and jagged arrays; indexing, iteration, and built-in
 *        array helpers.
 *
 * WHY IT MATTERS:
 *   Exam scores, price lists, game boards, and spreadsheet-like grids all use
 *   arrays when the count is known at creation time. Index mistakes and fixed
 *   length cause IndexOutOfRangeException and wasted slots. Choosing arrays vs
 *   growable collections affects performance and design in every C# application.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Array declaration and initialization syntax
 *   2.  Access by index and the Length property
 *   3.  Iterating with for and foreach
 *   4.  Two-dimensional arrays (rows and columns)
 *   5.  Jagged arrays (array of arrays)
 *   6.  Array.Sort, Array.Reverse, and Array.IndexOf
 *   7.  Advantages and disadvantages vs collections
 *   8.  Previews: params arrays, arrays of objects, List<T>
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;

namespace Arrays;

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 1: ARRAYS OVERVIEW
         * =========================================================================
         *
         * An ARRAY is a fixed-size collection of elements of the SAME type,
         * stored in contiguous memory. Every element shares one type (int[],
         * string[], etc.) and is reached by a zero-based INDEX.
         *
         *   Index:     0      1      2      3      4
         *   int[]:   [ 88 ] [ 92 ] [ 76 ] [ 85 ] [ 90 ]
         *
         * Key facts:
         *
         *   • Size is set at creation — arrays do NOT grow or shrink.
         *   • First index is 0; last index is Length - 1.
         *   • Length is a property, not a method (no parentheses).
         *   • Arrays are reference types — the variable holds a reference to
         *     heap storage, not the elements themselves.
         *
         * This chapter uses a campus learning-center scenario: student exam
         * scores (1D), textbook stock on a shelf grid (2D), department course
         * catalogs (jagged), and SKU lookup for inventory. Values are fixed so
         * the program runs without keyboard input; every variable is used in output.
         * -------------------------------------------------------------------------
         */

        string studentName = "Priya Sharma";
        string termLabel = "Fall 2026";


        /*
         * =========================================================================
         * SECTION 2: ARRAY DECLARATION AND INITIALIZATION
         * =========================================================================
         *
         * DECLARATION tells the compiler the element type and array name.
         * INITIALIZATION allocates storage and optionally sets values.
         *
         * --- 2a. Declare then assign later ---
         *
         *   int[] scores;           // declaration only — not usable until new
         *   scores = new int[5];    // five ints, default value 0 for each slot
         *
         * --- 2b. Declare and allocate in one line ---
         *
         *   int[] scores = new int[5];
         *
         * --- 2c. Declare, allocate, and set values (collection initializer) ---
         *
         *   int[] scores = new int[] { 88, 92, 76, 85, 90 };
         *   int[] scores = { 88, 92, 76, 85, 90 };   // shorthand when type is clear
         *
         * --- 2d. string[] and other types ---
         *
         *   string[] subjects = { "Math", "Physics", "Chemistry", "English", "CS" };
         *
         * Default values when using new type[size] without initializers:
         *
         *   Type        | Default per slot
         *   ------------|------------------
         *   int, double | 0
         *   bool        | false
         *   char        | '\0'
         *   string, etc.| null (reference types)
         * -------------------------------------------------------------------------
         */

        int[] examScores = { 88, 92, 76, 85, 90 };
        string[] subjects = { "Math", "Physics", "Chemistry", "English", "CS" };
        int[] emptySlots = new int[3]; // three zeros — shows default initialization


        /*
         * =========================================================================
         * SECTION 3: ACCESS BY INDEX AND Length
         * =========================================================================
         *
         * Use square brackets with a zero-based index to read or write one slot:
         *
         *   examScores[0]   → first element (88)
         *   examScores[4]   → last element  (90)
         *   examScores[5]   → IndexOutOfRangeException at runtime
         *
         * Length returns the total number of slots (always ≥ 0):
         *
         *   examScores.Length   → 5
         *   last index          → examScores.Length - 1   → 4
         *
         * Compile notes:
         *
         *   examScores["0"]     → CS0022 — index must be int (or int-like), not string
         *   examScores.Length() → CS1955 — Length is a property, not a method
         *
         * Updating a slot overwrites the previous value in that position only.
         * -------------------------------------------------------------------------
         */

        int topScore = examScores[0];
        examScores[2] = 78; // Chemistry retake — was 76, now 78
        int scoreCount = examScores.Length;
        int lastIndex = scoreCount - 1;
        int lowestStoredScore = examScores[lastIndex];
        int unusedSlotSum = emptySlots[0] + emptySlots[1] + emptySlots[2]; // 0 + 0 + 0


        /*
         * =========================================================================
         * SECTION 4: ITERATING ARRAYS (for AND foreach)
         * =========================================================================
         *
         * --- 4a. for loop — index-based ---
         *
         * Use when you need the index (updating slots, pairing with another array):
         *
         *   for (int i = 0; i < examScores.Length; i++)
         *   {
         *       Console.WriteLine(subjects[i] + ": " + examScores[i]);
         *   }
         *
         * Loop bound: i < Length (not i <= Length) to avoid off-by-one errors.
         *
         * --- 4b. foreach — element-based ---
         *
         * Read-only style iteration over each element (no index variable):
         *
         *   foreach (int score in examScores)
         *   {
         *       total += score;
         *   }
         *
         * foreach cannot replace elements of a struct array in place through its
         * iteration variable, and modifying a collection during foreach throws
         * InvalidOperationException — use for when mutating during traversal.
         * -------------------------------------------------------------------------
         */

        int scoreTotal = 0;
        for (int i = 0; i < examScores.Length; i++)
        {
            scoreTotal += examScores[i];
        }

        double averageScore = (double)scoreTotal / examScores.Length;

        string scoreReport = string.Empty;
        for (int i = 0; i < subjects.Length; i++)
        {
            if (i > 0)
            {
                scoreReport += "; ";
            }

            scoreReport += subjects[i] + "=" + examScores[i];
        }

        int passCount = 0;
        foreach (int score in examScores)
        {
            if (score >= 40)
            {
                passCount++;
            }
        }


        /*
         * =========================================================================
         * SECTION 5: TWO-DIMENSIONAL ARRAYS (2D)
         * =========================================================================
         *
         * A 2D array is a grid with ROWS and COLUMNS — one rectangular block of
         * memory. Syntax uses a comma inside the brackets:
         *
         *   int[,] shelfStock = new int[3, 4];   // 3 rows, 4 columns
         *
         * Access: shelfStock[row, col]   — both indices zero-based.
         *
         *   shelfStock.GetLength(0)  → row count (3)
         *   shelfStock.GetLength(1)  → column count (4)
         *
         * Unlike a jagged array, every row has the SAME number of columns.
         *
         * Scenario: textbook copies on a shelf grid — rows = shelf levels,
         * columns = bin positions. Each cell holds copy count.
         * -------------------------------------------------------------------------
         */

        int[,] shelfStock = new int[,]
        {
            { 12, 8, 15, 6 },   // row 0 — top shelf
            { 5, 20, 10, 14 },  // row 1
            { 9, 11, 7, 18 }    // row 2 — bottom shelf
        };

        int shelfRows = shelfStock.GetLength(0);
        int shelfCols = shelfStock.GetLength(1);
        int gridCellCount = shelfRows * shelfCols;

        int totalCopiesOnShelf = 0;
        for (int row = 0; row < shelfRows; row++)
        {
            for (int col = 0; col < shelfCols; col++)
            {
                totalCopiesOnShelf += shelfStock[row, col];
            }
        }

        int centerBinCopies = shelfStock[1, 2]; // row 1, column 2 → 10


        /*
         * =========================================================================
         * SECTION 6: JAGGED ARRAYS
         * =========================================================================
         *
         * A JAGGED ARRAY is an array whose elements are OTHER arrays — each inner
         * array can have a DIFFERENT length. Syntax uses two sets of brackets:
         *
         *   string[][] departmentCourses = new string[3][];
         *   departmentCourses[0] = new string[] { "ENG-101", "ENG-201" };
         *   departmentCourses[1] = new string[] { "MAT-100", "MAT-200", "MAT-300" };
         *
         * Access: departmentCourses[deptIndex][courseIndex]
         *
         *   departmentCourses.Length           → number of departments (outer length)
         *   departmentCourses[1].Length        → courses in department 1 only
         *
         * 2D vs jagged:
         *
         *   int[,] grid     | rectangular — every row same width; single block
         *   int[][] ragged  | each row can differ; array-of-arrays on the heap
         *
         * Use jagged when rows naturally vary (departments with different course counts).
         * -------------------------------------------------------------------------
         */

        string[][] departmentCourses = new string[][]
        {
            new string[] { "ENG-101", "ENG-201" },
            new string[] { "MAT-100", "MAT-200", "MAT-300" },
            new string[] { "CS-50" }
        };

        int departmentCount = departmentCourses.Length;
        int scienceCourseCount = departmentCourses[1].Length;
        string firstScienceCourse = departmentCourses[1][0];

        int totalCourseOfferings = 0;
        for (int d = 0; d < departmentCourses.Length; d++)
        {
            totalCourseOfferings += departmentCourses[d].Length;
        }


        /*
         * =========================================================================
         * SECTION 7: ARRAY METHODS (Sort, Reverse, IndexOf)
         * =========================================================================
         *
         * System.Array static methods operate on any array instance:
         *
         *   Array.Sort(array)           — ascending order (in-place; mutates original)
         *   Array.Reverse(array)        — reverses element order (in-place)
         *   Array.IndexOf(array, value) — first index of value, or -1 if not found
         *
         * Sort and Reverse change the ORIGINAL array — keep a copy if you need the
         * original order later:
         *
         *   int[] copy = (int[])original.Clone();
         *
         * IndexOf uses default equality (== for ints, Equals for objects).
         * For sorted arrays, Array.BinarySearch is faster — covered with methods.
         * -------------------------------------------------------------------------
         */

        string[] skuCodes = { "TBK-104", "TBK-220", "TBK-087", "TBK-315", "TBK-150" };

        string[] skuLookupOrder = (string[])skuCodes.Clone();
        Array.Sort(skuLookupOrder);

        string[] skuDisplayOrder = (string[])skuCodes.Clone();
        Array.Reverse(skuDisplayOrder);

        int foundAtIndex = Array.IndexOf(skuCodes, "TBK-087");
        int notFoundIndex = Array.IndexOf(skuCodes, "TBK-999");

        string sortedFirstSku = skuLookupOrder[0];
        string reversedFirstSku = skuDisplayOrder[0];


        /*
         * =========================================================================
         * SECTION 8: ADVANTAGES AND DISADVANTAGES VS COLLECTIONS
         * =========================================================================
         *
         * --- Advantages of arrays ---
         *
         *   • Fast index access — O(1) by position
         *   • Contiguous memory — cache-friendly for numeric loops
         *   • Simple syntax — built into the language from the start
         *   • Fixed size can be a FEATURE when dimensions are truly constant
         *
         * --- Disadvantages of arrays ---
         *
         *   • Fixed length — cannot add/remove after creation without a new array
         *   • Resizing requires allocate-copy (manual or via List<T>.ToArray())
         *   • Insert/delete in the middle is expensive — must shift elements
         *   • Rectangular 2D only in int[,]; ragged rows need jagged arrays
         *
         * --- When to prefer something else ---
         *
         *   Need grow/shrink at runtime  → List<T> (preview below)
         *   Key/value lookup            → Dictionary<TKey,TValue> (later chapter)
         *   Unique sorted set             → HashSet<T> / SortedSet<T> (later)
         *
         * Arrays remain ideal for known-size buffers, matrices, and interop with
         * APIs that expect T[].
         * -------------------------------------------------------------------------
         */

        string arrayStrength = "O(1) index access, contiguous memory, simple syntax";
        string arrayLimitation = "fixed size — no add/remove without copying to a new array";
        string collectionHint = "use List<T> when count changes at runtime";


        /*
         * =========================================================================
         * SECTION 9: PARAMS ARRAYS (PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → Methods chapter
         *   (headline concepts only: variable argument count, params keyword,
         *    compiler wraps callers into a single array parameter)
         *
         * The params modifier on the last parameter lets callers pass any number
         * of arguments; the compiler builds an array automatically:
         *
         *   static int Sum(params int[] values) { ... }
         *   Sum(10, 20, 30);   // same as Sum(new int[] { 10, 20, 30 });
         * -------------------------------------------------------------------------
         */

        int quizTotal = SumScores(12, 18, 15);
        int labTotal = SumScores(25, 22, 28, 20);


        /*
         * =========================================================================
         * SECTION 10: ARRAYS OF OBJECTS (PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → OOP / Classes chapter
         *   (headline concepts only: custom types, Student[] arrays, polymorphism,
         *    storing heterogeneous data via a common base type or interface)
         *
         * Every array element shares one declared type. object[] can hold any
         * reference or boxed value, but you lose compile-time type safety until
         * you cast back. Prefer typed arrays (Student[], string[]) in real code.
         * -------------------------------------------------------------------------
         */

        object[] mixedPreview = new object[] { "ENG-101", 42, true };
        string mixedPreviewSummary = mixedPreview[0] + ", " + mixedPreview[1] + ", " + mixedPreview[2];


        /*
         * =========================================================================
         * SECTION 11: List<T> ALTERNATIVE (PREVIEW)
         * =========================================================================
         *
         * COVERED IN DETAIL LATER → 03. Generics / List
         *   (headline concepts only: dynamic size, Add/Remove, Count property,
         *    List<T> implements IList<T> — convert to array with ToArray())
         *
         * List<T> wraps a growable buffer. Use it when items are added or removed
         * during the program. Arrays and lists coexist — many APIs accept T[].
         * -------------------------------------------------------------------------
         */

        List<string> waitlistPreview = new List<string> { "Alex", "Jordan" };
        waitlistPreview.Add("Sam");
        int waitlistCount = waitlistPreview.Count;
        string[] waitlistSnapshot = waitlistPreview.ToArray();
        string waitlistFirst = waitlistSnapshot[0];


        /*
         * =========================================================================
         * SECTION 12: GRADE BOOK SUMMARY (OUTPUT)
         * =========================================================================
         * Prints results from every section above — no unused variables.
         * -------------------------------------------------------------------------
         */

        Console.WriteLine("=== Arrays — Campus Learning Center ===");
        Console.WriteLine();
        Console.WriteLine($"Student: {studentName} ({termLabel})");
        Console.WriteLine($"Scores ({scoreCount} subjects): {scoreReport}");
        Console.WriteLine($"Total={scoreTotal}, Average={averageScore:F1}, Pass count={passCount}/{scoreCount}");
        Console.WriteLine($"Top score (index 0)={topScore}, Last slot (index {lastIndex})={lowestStoredScore}");
        Console.WriteLine($"Chemistry updated score={examScores[2]}, Empty slot sum={unusedSlotSum}");
        Console.WriteLine();
        Console.WriteLine($"Shelf grid {shelfRows}x{shelfCols} ({gridCellCount} cells): total copies={totalCopiesOnShelf}");
        Console.WriteLine($"Center bin [1,2]={centerBinCopies}");
        Console.WriteLine();
        Console.WriteLine($"Departments={departmentCount}, Courses={totalCourseOfferings}");
        Console.WriteLine($"Science courses={scienceCourseCount}, First={firstScienceCourse}");
        Console.WriteLine();
        Console.WriteLine($"SKU original[0]={skuCodes[0]}, sorted first={sortedFirstSku}, reversed first={reversedFirstSku}");
        Console.WriteLine($"IndexOf TBK-087={foundAtIndex}, IndexOf TBK-999={notFoundIndex}");
        Console.WriteLine();
        Console.WriteLine($"Arrays: {arrayStrength}");
        Console.WriteLine($"Limit: {arrayLimitation} — {collectionHint}");
        Console.WriteLine($"Params preview: quiz={quizTotal}, lab={labTotal}");
        Console.WriteLine($"object[] preview: {mixedPreviewSummary}");
        Console.WriteLine($"List<string> preview: count={waitlistCount}, first={waitlistFirst}");
    }

    /*
     * Params-array helper — full params semantics in the Methods chapter.
     */
    static int SumScores(params int[] values)
    {
        int total = 0;
        foreach (int value in values)
        {
            total += value;
        }

        return total;
    }
}

/*
 * =============================================================================
 * QUICK REFERENCE — ARRAYS
 * =============================================================================
 *
 * --- Declaration ---
 *
 *  int[] a = new int[5];              allocate 5 ints (default 0)
 *  int[] b = { 1, 2, 3 };             allocate + initialize
 *  string[] c = new string[] { "x" }; explicit type in initializer
 *
 * --- Access ---
 *
 *  a[0]              first element (index 0)
 *  a[a.Length - 1]   last element
 *  a.Length          element count (property)
 *
 * --- Iteration ---
 *
 *  for (int i = 0; i < a.Length; i++)   index needed / mutate slots
 *  foreach (int x in a)                 read each element
 *
 * --- Multidimensional ---
 *
 *  int[,] grid = new int[3, 4];         rectangular 2D
 *  grid[row, col]                       two indices, comma separator
 *  grid.GetLength(0)                    row count; GetLength(1) = columns
 *
 * --- Jagged ---
 *
 *  int[][] jagged = new int[3][];       array of int arrays
 *  jagged[0] = new int[] { 1, 2 };     each row can differ in length
 *  jagged[0][1]                         row index, then column index
 *
 * --- Static helpers (System.Array) ---
 *
 *  Array.Sort(a)                       sort in place (ascending)
 *  Array.Reverse(a)                    reverse in place
 *  Array.IndexOf(a, value)             first index, or -1
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Index >= Length                      | IndexOutOfRangeException
 *  a.Length()                           | CS1955 — Length is not a method
 *  a["0"]                               | CS0022 — index must be int
 *  foreach + modify same array structure| InvalidOperationException
 *  Expect array to grow after creation  | must allocate new array or use List<T>
 *
 * =============================================================================
 */
