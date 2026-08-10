/*
 * TOPIC: Fixed-size collections stored in contiguous memory — single-dimensional,
 *        multidimensional, and jagged arrays; indexing, bounds, iteration, and
 *        the static helpers on System.Array.
 *
 * WHY IT MATTERS:
 *   Exam scores, price lists, game boards, and spreadsheet-like grids all use
 *   arrays when the count is known at creation time. Index mistakes and fixed
 *   length cause IndexOutOfRangeException and wasted slots. Choosing arrays vs
 *   growable collections affects performance and design in every C# application.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Array declaration and initialization (1D)
 *   2.  Access by index, Length, and bounds checking
 *   3.  Iterating with for and foreach (including foreach rules)
 *   4.  Two-dimensional rectangular arrays
 *   5.  Jagged arrays (array of arrays)
 *   6.  Rank, GetLength, and Length on multidimensional arrays
 *   7.  System.Array — Sort, Reverse, IndexOf, BinarySearch, Copy, Clear, Fill, Resize, Clone
 *   8.  Advantages and disadvantages vs growable collections
 *   9.  Previews: params arrays, object[], List<T>
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Arrays;

public class Program
{
    /*
     * SECTION 1: ARRAYS OVERVIEW
     *
     * An ARRAY is a fixed-size collection of elements of the SAME type, stored in
     * contiguous memory. Every element shares one type (int[], string[], etc.) and
     * is reached by a zero-based INDEX.
     *
     *   Index:     0      1      2      3      4
     *   int[]:   [ 88 ] [ 92 ] [ 76 ] [ 85 ] [ 90 ]
     *
     * Key facts:
     *
     *   • Size is set at creation — arrays do NOT grow or shrink.
     *   • First index is 0; last index is Length - 1.
     *   • Length is a property, not a method (no parentheses).
     *   • Arrays are reference types — the variable holds a reference to heap
     *     storage, not the elements themselves.
     *   • All arrays inherit from System.Array (and implement IList, ICollection,
     *     IEnumerable — enumeration depth is in Generics & Collections).
     *
     * Scenario: campus learning center — exam scores (1D), textbook shelf grid (2D),
     * department course catalogs (jagged), and SKU lookup (Array helpers).
     */

    /*
     * SECTION 2: DECLARATION AND INITIALIZATION (1D)
     *
     * --- 2a. Declare then assign later ---
     *
     *   int[] scores;              // declaration only — not usable until new
     *   scores = new int[5];       // five ints, default 0 per slot
     *
     * --- 2b. Declare and allocate in one line ---
     *
     *   int[] scores = new int[5];
     *
     * --- 2c. Collection initializer ---
     *
     *   int[] scores = new int[] { 88, 92, 76, 85, 90 };
     *   int[] scores = { 88, 92, 76, 85, 90 };   // shorthand when type is clear
     *
     * Default values when using new type[size] without initializers:
     *
     *   Type         | Default per slot
     *   -------------|------------------
     *   int, double  | 0
     *   bool         | false
     *   char         | '\0'
     *   string, etc. | null (reference types)
     */
    static int[] CreateExamScores()
    {
        return new int[] { 88, 92, 76, 85, 90 }; // collection initializer — size inferred from values
    }

    static string[] CreateSubjects()
    {
        return new string[] { "Math", "Physics", "Chemistry", "English", "CS" }; // parallel array — same Length as scores
    }

    static int[] CreateDefaultInitializedSlots()
    {
        return new int[3]; // three ints, each 0
    }

    /*
     * SECTION 3: ACCESS BY INDEX AND Length
     *
     * Use square brackets with a zero-based index to read or write one slot:
     *
     *   scores[0]   → first element
     *   scores[4]   → last element when Length is 5
     *   scores[5]   → IndexOutOfRangeException at runtime
     *
     * Length returns the total number of slots (always ≥ 0):
     *
     *   scores.Length        → element count
     *   last index           → scores.Length - 1
     *
     * Compile notes:
     *
     *   scores["0"]          → CS0022 — index must be int (or int-like), not string
     *   scores.Length()      → CS1955 — Length is a property, not a method
     *
     * Updating a slot overwrites only that position; other elements are unchanged.
     */
    static int GetLastIndex(int[] values)
    {
        return values.Length - 1; // last valid index — Length is count, not an index
    }

    static void UpdateScoreAt(int[] scores, int index, int newScore)
    {
        scores[index] = newScore; // overwrites one slot; other elements unchanged
    }

    /*
     * SECTION 4: BOUNDS AND IndexOutOfRangeException
     *
     * Valid indices: 0 through Length - 1. Negative indices are NOT supported
     * (unlike Python). Reading or writing outside that range throws
     * IndexOutOfRangeException at RUNTIME — the compiler does not catch it.
     *
     * Safe pattern before access when the index comes from user input or search:
     *
     *   if (index >= 0 && index < array.Length)
     *       value = array[index];
     *
     * There is no automatic bounds-checked Get that returns a default; guard the
     * index yourself or catch the exception (prefer guarding in normal flow).
     */
    static bool IsValidIndex(int[] values, int index)
    {
        return index >= 0 && index < values.Length; // guard before access — avoids IndexOutOfRangeException
    }

    static int SumDefaultSlots(int[] defaultSlots)
    {
        return defaultSlots[0] + defaultSlots[1] + defaultSlots[2]; // new int[3] slots default to 0
    }

    /*
     * SECTION 5: ITERATING WITH for
     *
     * Use a for loop when you need the index — pairing parallel arrays, updating
     * slots, or computing position-dependent values.
     *
     *   for (int i = 0; i < scores.Length; i++)
     *   {
     *       total += scores[i];
     *   }
     *
     * Loop bound: i < Length (not i <= Length) to avoid off-by-one errors.
     *
     * Nested for loops traverse 2D grids row by row (see Section 7).
     */
    static int SumWithFor(int[] values)
    {
        int total = 0;
        for (int i = 0; i < values.Length; i++) // i < Length — not <= — avoids off-by-one
        {
            total += values[i]; // index access — use for when position matters
        }

        return total;
    }

    static string BuildScoreReport(string[] subjects, int[] scores)
    {
        StringBuilder report = new StringBuilder();
        for (int i = 0; i < subjects.Length; i++) // parallel arrays — same index pairs subject + score
        {
            if (i > 0)
            {
                report.Append("; "); // separator between pairs, not before the first
            }

            report.Append(subjects[i]);
            report.Append('=');
            report.Append(scores[i]);
        }

        return report.ToString();
    }

    /*
     * SECTION 6: ITERATING WITH foreach
     *
     * foreach walks each ELEMENT without exposing an index variable:
     *
     *   foreach (int score in scores)
     *   {
     *       passCount += score >= 40 ? 1 : 0;
     *   }
     *
     * Rules and pitfalls:
     *
     *   • Works on any array (arrays implement IEnumerable).
     *   • The iteration variable is read-only — you cannot assign to it to replace
     *     an element in the source array.
     *   • Do not add/remove elements (or change array length) during foreach.
     *   • Use for when you must mutate slots by index during traversal.
     *
     * On a 2D array, foreach visits every element in row-major order (Section 7).
     */
    static int CountPassingScores(int[] scores, int passingMark)
    {
        int passCount = 0;
        foreach (int score in scores) // foreach reads elements — cannot assign to score to replace slots
        {
            if (score >= passingMark)
            {
                passCount++;
            }
        }

        return passCount;
    }

    static int SumWithForeach(int[] values)
    {
        int total = 0;
        foreach (int value in values) // no index variable — fine when only reading elements
        {
            total += value;
        }

        return total;
    }

    /*
     * SECTION 7: TWO-DIMENSIONAL ARRAYS (RECTANGULAR)
     *
     * A 2D array is a grid with ROWS and COLUMNS — one rectangular block of memory.
     * Syntax uses a comma inside the brackets:
     *
     *   int[,] shelf = new int[3, 4];   // 3 rows, 4 columns
     *
     * Access: shelf[row, col] — both indices zero-based, comma separator.
     *
     *   shelf.GetLength(0)  → row count
     *   shelf.GetLength(1)  → column count
     *   shelf.Rank          → number of dimensions (2 here)
     *   shelf.Length        → total cells (rows × columns)
     *
     * Unlike jagged arrays, every row has the SAME number of columns.
     *
     * foreach (int cell in shelf) visits each cell row-by-row, left-to-right.
     */
    static int[,] CreateShelfStock()
    {
        return new int[,] // rectangular 2D — comma in type int[,]; comma between indices at access
        {
            { 12, 8, 15, 6 },
            { 5, 20, 10, 14 },
            { 9, 11, 7, 18 }
        };
    }

    static int SumGridWithNestedFor(int[,] grid)
    {
        int total = 0;
        int rows = grid.GetLength(0); // dimension 0 = row count
        int cols = grid.GetLength(1); // dimension 1 = column count
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                total += grid[row, col]; // comma separator — not grid[row][col]
            }
        }

        return total;
    }

    static int SumGridWithForeach(int[,] grid)
    {
        int total = 0;
        foreach (int cell in grid) // visits every cell in row-major order
        {
            total += cell;
        }

        return total;
    }

    /*
     * SECTION 8: JAGGED ARRAYS
     *
     * A JAGGED ARRAY is an array whose elements are OTHER arrays — each inner array
     * can have a DIFFERENT length. Syntax uses two sets of brackets:
     *
     *   string[][] departments = new string[3][];
     *   departments[0] = new string[] { "ENG-101", "ENG-201" };
     *   departments[1] = new string[] { "MAT-100", "MAT-200", "MAT-300" };
     *
     * Access: departments[deptIndex][courseIndex]
     *
     *   departments.Length           → number of rows (outer array length)
     *   departments[1].Length        → length of inner array at index 1 only
     *
     * 2D rectangular vs jagged:
     *
     *   int[,] grid     | one block; every row same width
     *   int[][] ragged  | array-of-arrays; rows can differ
     *
     * Use jagged when rows naturally vary (departments with different course counts).
     */
    static string[][] CreateDepartmentCourses()
    {
        return new string[][] // jagged — outer array of inner string arrays (rows can differ in length)
        {
            new string[] { "ENG-101", "ENG-201" },
            new string[] { "MAT-100", "MAT-200", "MAT-300" },
            new string[] { "CS-50" }
        };
    }

    static int CountAllCourses(string[][] departments)
    {
        int total = 0;
        for (int d = 0; d < departments.Length; d++) // outer Length = department count
        {
            total += departments[d].Length; // inner Length varies per row
        }

        return total;
    }

    /*
     * SECTION 9: System.Array — Sort, Reverse, IndexOf, BinarySearch
     *
     * Static methods on System.Array operate on any array instance:
     *
     *   Array.Sort(array)              — ascending, in-place
     *   Array.Reverse(array)           — reverses order, in-place
     *   Array.IndexOf(array, value)    — first index, or -1 if missing
     *   Array.LastIndexOf(array, val)  — last index, or -1
     *   Array.BinarySearch(array, v)   — index in a SORTED array, or negative
     *                                    insertion point if not found
     *
     * Sort and Reverse mutate the ORIGINAL array. Clone or Copy first when you
     * need to preserve the original order.
     *
     * BinarySearch requires the array to be sorted (same comparer as Sort uses).
     */
    static string[] CreateSkuCodes()
    {
        return new string[] { "TBK-104", "TBK-220", "TBK-087", "TBK-315", "TBK-150" };
    }

    static void DemonstrateSearchAndSort(string[] originalSkus, out string sortedFirst, out string reversedFirst,
        out int foundIndex, out int notFoundIndex, out int binarySearchIndex)
    {
        string[] sortedCopy = (string[])originalSkus.Clone(); // shallow copy — original order preserved
        Array.Sort(sortedCopy); // in-place ascending sort
        sortedFirst = sortedCopy[0];

        string[] reversedCopy = (string[])originalSkus.Clone();
        Array.Reverse(reversedCopy); // in-place reverse
        reversedFirst = reversedCopy[0];

        foundIndex = Array.IndexOf(originalSkus, "TBK-087"); // first match index, or -1 if missing
        notFoundIndex = Array.IndexOf(originalSkus, "TBK-999");
        binarySearchIndex = Array.BinarySearch(sortedCopy, "TBK-150"); // requires sorted array
    }

    /*
     * SECTION 10: System.Array — Copy, Clear, Fill, Resize
     *
     * --- Copy ---
     *
     *   Array.Copy(source, destination, length);
     *   Array.Copy(source, srcIndex, dest, destIndex, length);
     *
     * Copies elements between arrays (may overlap safely when using Array.Copy).
     *
     * --- Clear ---
     *
     *   Array.Clear(array, startIndex, count);
     *
     * Sets slots to the type default (0, false, null, etc.).
     *
     * --- Fill (.NET Core 2.1+) ---
     *
     *   Array.Fill(array, value);
     *   Array.Fill(array, value, startIndex, count);
     *
     * --- Resize ---
     *
     *   Array.Resize(ref int[] array, newSize);
     *
     * Allocates a new array, copies existing elements, updates the ref parameter.
     * Shrinking drops trailing elements; growing fills new slots with defaults.
     *
     * --- Clone ---
     *
     *   int[] copy = (int[])original.Clone();   // shallow copy of the array object
     */
    static int[] CopyAndResizeDemo(int[] source)
    {
        int[] destination = new int[source.Length + 2];
        Array.Copy(source, 0, destination, 1, source.Length); // copy into destination starting at index 1
        Array.Resize(ref destination, source.Length + 1); // ref parameter — rebinds destination to new array
        return destination;
    }

    static int[] ClearAndFillDemo()
    {
        int[] buffer = { 9, 9, 9, 9, 9 };
        Array.Clear(buffer, 1, 2); // slots 1–2 set to default (0 for int)
        Array.Fill(buffer, 7, 3, 2); // slots 3–4 set to 7
        return buffer;
    }

    /*
     * SECTION 11: ARRAYS VS COLLECTIONS
     *
     * --- Advantages of arrays ---
     *
     *   • O(1) index access
     *   • Contiguous memory — cache-friendly for numeric loops
     *   • Simple language syntax; interoperates with APIs expecting T[]
     *   • Fixed size is appropriate when dimensions are truly constant
     *
     * --- Disadvantages ---
     *
     *   • Fixed length — no Add/Remove without allocating a new array
     *   • Insert/delete in the middle requires shifting elements
     *   • Rectangular grids use int[,]; ragged rows need jagged arrays
     *
     * --- When to prefer collections (preview below) ---
     *
     *   Grow/shrink at runtime  → List<T>
     *   Key/value lookup        → Dictionary<TKey, TValue>
     *   Unique membership       → HashSet<T>
     */
    static string DescribeArrayTradeoffs()
    {
        return "O(1) index + contiguous memory vs fixed size — use List<T> when count changes";
    }

    /*
     * SECTION 12: PARAMS ARRAYS (PREVIEW)
     *
     * COVERED IN DETAIL LATER → 07. Methods
     *   (params keyword, variable argument count, compiler wraps callers into one array)
     *
     *   static int Sum(params int[] values) { ... }
     *   Sum(10, 20, 30);   // same as Sum(new int[] { 10, 20, 30 });
     */
    static int SumScores(params int[] values) // params — caller can pass comma-separated ints
    {
        return SumWithForeach(values);
    }

    /*
     * SECTION 13: ARRAYS OF OBJECTS (PREVIEW)
     *
     * COVERED IN DETAIL LATER → 02. Object Oriented Programming
     *   (custom types, Student[] arrays, polymorphism via base types and interfaces)
     *
     * object[] can hold any reference or boxed value but loses compile-time type
     * safety until you cast. Prefer typed arrays (string[], Student[]) in real code.
     */
    static string SummarizeObjectArrayPreview()
    {
        object[] mixed = new object[] { "ENG-101", 42, true }; // heterogeneous references — boxing for 42 and true
        return mixed[0] + ", " + mixed[1] + ", " + mixed[2]; // + concatenates after ToString on each element
    }

    /*
     * SECTION 14: List<T> AND COLLECTIONS (PREVIEW)
     *
     * COVERED IN DETAIL LATER → 03. Generics & Collections / 03. List
     *   (dynamic size, Add/Remove, Count, ToArray(), Dictionary, HashSet, IEnumerable)
     *
     * List<T> wraps a growable buffer backed by an array that resizes as needed.
     * Many APIs accept T[] — convert with list.ToArray() when an array is required.
     */
    static string FirstWaitlistNameAfterAdd()
    {
        List<string> waitlist = new List<string> { "Alex", "Jordan" };
        waitlist.Add("Sam"); // growable collection — no fixed Length like an array
        string[] snapshot = waitlist.ToArray(); // copy current items into a new T[]
        return snapshot[0];
    }

    /*
     * SECTION 15: DEMONSTRATION — Main orchestrates the chapter demo
     *
     * Wires every helper above into one runnable output. Values are fixed so the
     * program runs without keyboard input; every local is used in Console output.
     */
    public static void Main(string[] args)
    {
        string studentName = "Priya Sharma";
        string termLabel = "Fall 2026";

        int[] examScores = CreateExamScores();           // SECTION 2 — 1D initialization
        string[] subjects = CreateSubjects();
        int[] emptySlots = CreateDefaultInitializedSlots();

        int topScore = examScores[0];                    // SECTION 3 — zero-based index access
        UpdateScoreAt(examScores, 2, 78);
        int scoreCount = examScores.Length;              // Length property — not a method
        int lastIndex = GetLastIndex(examScores);
        int lowestStoredScore = examScores[lastIndex];
        int unusedSlotSum = SumDefaultSlots(emptySlots);
        bool chemistryIndexValid = IsValidIndex(examScores, 2); // SECTION 4 — bounds guard

        int scoreTotal = SumWithFor(examScores);         // SECTION 5 — for loop sum
        double averageScore = (double)scoreTotal / examScores.Length; // cast for floating-point division
        string scoreReport = BuildScoreReport(subjects, examScores);
        int passCount = CountPassingScores(examScores, 40); // SECTION 6 — foreach count

        int[,] shelfStock = CreateShelfStock();          // SECTION 7 — rectangular 2D grid
        int shelfRows = shelfStock.GetLength(0);
        int shelfCols = shelfStock.GetLength(1);
        int gridRank = shelfStock.Rank;                  // number of dimensions (2 here)
        int gridCellCount = shelfStock.Length;           // total cells = rows × columns
        int totalCopiesForLoop = SumGridWithNestedFor(shelfStock);
        int totalCopiesForeach = SumGridWithForeach(shelfStock);
        int centerBinCopies = shelfStock[1, 2];          // row 1, column 2

        string[][] departmentCourses = CreateDepartmentCourses(); // SECTION 8 — jagged rows
        int departmentCount = departmentCourses.Length;
        int scienceCourseCount = departmentCourses[1].Length; // inner array length at index 1
        string firstScienceCourse = departmentCourses[1][0];  // [dept][course] — two index steps
        int totalCourseOfferings = CountAllCourses(departmentCourses);

        string[] skuCodes = CreateSkuCodes();
        DemonstrateSearchAndSort(skuCodes, out string sortedFirstSku, out string reversedFirstSku,
            out int foundAtIndex, out int notFoundIndex, out int binarySearchIndex); // SECTION 9 — Array helpers

        int[] resizedCopy = CopyAndResizeDemo(examScores);   // SECTION 10 — Copy, Resize, Clear, Fill
        int resizedLength = resizedCopy.Length;
        int[] clearedFilled = ClearAndFillDemo();
        int clearedFilledSum = SumWithForeach(clearedFilled);

        string tradeoffSummary = DescribeArrayTradeoffs();   // SECTION 11 — arrays vs collections
        int quizTotal = SumScores(12, 18, 15);               // SECTION 12 — params preview
        int labTotal = SumScores(25, 22, 28, 20);
        string mixedPreviewSummary = SummarizeObjectArrayPreview(); // SECTION 13 — object[] preview
        string waitlistFirst = FirstWaitlistNameAfterAdd();  // SECTION 14 — List<T> preview

        Console.WriteLine("=== Arrays — Campus Learning Center ===");
        Console.WriteLine();
        Console.WriteLine($"Student: {studentName} ({termLabel})");
        Console.WriteLine($"Scores ({scoreCount} subjects): {scoreReport}");
        Console.WriteLine($"Total={scoreTotal}, Average={averageScore:F1}, Pass count={passCount}/{scoreCount}");
        Console.WriteLine($"Top score (index 0)={topScore}, Last slot (index {lastIndex})={lowestStoredScore}");
        Console.WriteLine($"Chemistry updated={examScores[2]}, index valid={chemistryIndexValid}, Empty slot sum={unusedSlotSum}");
        Console.WriteLine();
        Console.WriteLine($"Shelf grid {shelfRows}x{shelfCols}, Rank={gridRank}, Length={gridCellCount}");
        Console.WriteLine($"Copies (for)={totalCopiesForLoop}, (foreach)={totalCopiesForeach}, Center [1,2]={centerBinCopies}");
        Console.WriteLine();
        Console.WriteLine($"Departments={departmentCount}, Courses={totalCourseOfferings}");
        Console.WriteLine($"Science courses={scienceCourseCount}, First={firstScienceCourse}");
        Console.WriteLine();
        Console.WriteLine($"SKU original[0]={skuCodes[0]}, sorted first={sortedFirstSku}, reversed first={reversedFirstSku}");
        Console.WriteLine($"IndexOf TBK-087={foundAtIndex}, IndexOf TBK-999={notFoundIndex}, BinarySearch TBK-150={binarySearchIndex}");
        Console.WriteLine($"Copy/Resize length={resizedLength}, Clear/Fill sum={clearedFilledSum}");
        Console.WriteLine();
        Console.WriteLine($"Tradeoffs: {tradeoffSummary}");
        Console.WriteLine($"Params preview: quiz={quizTotal}, lab={labTotal}");
        Console.WriteLine($"object[] preview: {mixedPreviewSummary}");
        Console.WriteLine($"List<string> preview: first after Add={waitlistFirst}");
    }
}

/*
 * QUICK REFERENCE — ARRAYS
 *
 * --- 1D declaration ---
 *
 *  int[] a = new int[5];              allocate 5 ints (default 0)
 *  int[] b = { 1, 2, 3 };             allocate + initialize
 *  string[] c = new string[] { "x" }; explicit type in initializer
 *
 * --- Access and bounds ---
 *
 *  a[0]                 first element (index 0)
 *  a[a.Length - 1]      last element
 *  a.Length             element count (property, not a method)
 *  index >= 0 && index < a.Length   safe guard before access
 *
 * --- Iteration ---
 *
 *  for (int i = 0; i < a.Length; i++)   index needed / mutate by slot
 *  foreach (int x in a)                 read each element
 *
 * --- Rectangular 2D ---
 *
 *  int[,] grid = new int[3, 4];         rows × columns, one block
 *  grid[row, col]                       comma between indices
 *  grid.GetLength(0)                    row count; GetLength(1) = columns
 *  grid.Rank                            dimension count (2)
 *  grid.Length                          total cells (rows × columns)
 *
 * --- Jagged ---
 *
 *  int[][] jagged = new int[3][];       array of int arrays
 *  jagged[0] = new int[] { 1, 2 };     each row can differ in length
 *  jagged[0][1]                         row index, then column index
 *
 * --- System.Array static helpers ---
 *
 *  Array.Sort(a)                        sort in place (ascending)
 *  Array.Reverse(a)                     reverse in place
 *  Array.IndexOf(a, value)              first index, or -1
 *  Array.BinarySearch(a, value)         sorted array only
 *  Array.Copy(src, dest, len)           copy between arrays
 *  Array.Clear(a, start, count)         set range to default
 *  Array.Fill(a, value)                 set all elements
 *  Array.Resize(ref a, newSize)         new array + copy + rebind ref
 *  (int[])a.Clone()                     shallow copy of array object
 *
 * --- Common mistakes ---
 *
 *  Mistake                               | Result
 *  --------------------------------------|----------------------------------
 *  Index >= Length or index < 0          | IndexOutOfRangeException
 *  a.Length()                            | CS1955 — Length is not a method
 *  a["0"]                                | CS0022 — index must be int
 *  foreach iteration variable assignment | does not update source array
 *  Expect array to grow after creation   | allocate new array or use List<T>
 *  BinarySearch on unsorted array        | undefined / wrong index
 */
