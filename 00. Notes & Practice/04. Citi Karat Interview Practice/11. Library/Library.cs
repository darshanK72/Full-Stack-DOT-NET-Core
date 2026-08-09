// library book management system https://www.onlinegdb.com/online_java_compiler#

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{

/*<bug-task1>
 * We are building a library book management system that tracks
 * members, books, and their loans.
 *
 * Classes:
 *   MemberTier    — enum: STANDARD, PREMIUM
 *   Genre         — enum: FICTION, HISTORY, NON_FICTION, SCIENCE
 *   LoanStatus    — enum: ACTIVE, OVERDUE, RETURNED
 *   Member        — library member (memberId, name, tier)
 *   Book          — library book (bookId, title, genre)
 *   Loan          — loan record (loanId, memberId, bookId, loanDays, status)
 *   LibraryManager — manages members, books, and loans
 * TASK 1: Read the code. The test is not passing due to a bug.
 *         Find and fix the bug in LibraryManager.
 </bug-task1>
 */

/*<task2>
 * TASK 2: Implement getOverdueBooksByMember():
 *   - Return Map<Integer, List<Integer>>: memberId → sorted list of bookIds
 *     that are currently OVERDUE for that member.
 *   - Only members with at least one OVERDUE loan appear in the result.
 *   - The list of bookIds for each member must be sorted in ascending order.
 </task2>
 */

/*<task3>
 * TASK 3: Implement calculateLateFees():
 *   - Only OVERDUE loans incur fees. ACTIVE and RETURNED loans have no fees.
 *   - STANDARD members are charged $0.50 per loanDay.
 *   - PREMIUM members are charged $0.25 per loanDay (discounted rate).
 *   - Return Map<Integer, Double>: memberId → total late fee.
 *   - Only members with at least one OVERDUE loan appear in the result.
 </task3>
 */

/*<task4>
 * TASK 4: Implement getLoanLimitViolators():
 *   - Members are allowed a maximum number of concurrent loans (ACTIVE + OVERDUE):
 *       STANDARD members : max 3 concurrent loans
 *       PREMIUM  members : max 5 concurrent loans
 *   - RETURNED loans do NOT count toward the concurrent limit.
 *   - A member is a violator if their current concurrent loan count EXCEEDS their limit.
 *   - Return a sorted List<Integer> of memberIds who are over their limit.
 *   - Members exactly at their limit are NOT violators.
 </task4>
 */

enum MemberTier
{
    STANDARD, PREMIUM
}

enum Genre
{
    FICTION, HISTORY, NON_FICTION, SCIENCE
}

enum LoanStatus
{
    ACTIVE, OVERDUE, RETURNED
}

class Member
{
    public int        memberId;
    public string     name;
    public MemberTier tier;

    public Member(int memberId, string name, MemberTier tier)
    {
        this.memberId = memberId;
        this.name     = name;
        this.tier     = tier;
    }
}

class Book
{
    public int    bookId;
    public string title;
    public Genre  genre;

    public Book(int bookId, string title, Genre genre)
    {
        this.bookId = bookId;
        this.title  = title;
        this.genre  = genre;
    }
}

class Loan
{
    public int        loanId;
    public int        memberId;
    public int        bookId;
    public int        loanDays;
    public LoanStatus status;

    public Loan(int loanId, int memberId, int bookId, int loanDays, LoanStatus status)
    {
        this.loanId   = loanId;
        this.memberId = memberId;
        this.bookId   = bookId;
        this.loanDays = loanDays;
        this.status   = status;
    }
}

class LibraryManager
{
    public Dictionary<int, Member> members = new Dictionary<int, Member>();
    public Dictionary<int, Book>   books   = new Dictionary<int, Book>();
    public List<Loan>              loans   = new List<Loan>();

    public void AddMember(Member member)
    {
        members[member.memberId] = member;
    }

    public void AddBook(Book book)
    {
        books[book.bookId] = book;
    }

    public void AddLoan(Loan loan)
    {
        if (!members.ContainsKey(loan.memberId)) return;
        if (!books.ContainsKey(loan.bookId))      return;
        loans.Add(loan);
    }

    // TASK 1 BUG: Returns the total number of books currently checked out (ACTIVE + OVERDUE).
    // Bug: RETURNED loans are NOT excluded from the count.
    public int GetCurrentCheckoutCount()
    {
        return loans.Count;
    }

    // TASK 2: Unimplemented - returns empty map.
    public Dictionary<int, List<int>> GetOverdueBooksByMember()
    {
        // TODO: implement
        return new Dictionary<int, List<int>>();
    }

    // TASK 3: Unimplemented - returns empty map.
    public Dictionary<int, double> CalculateLateFees()
    {
        // TODO: implement
        return new Dictionary<int, double>();
    }

    // TASK 4: Unimplemented - returns empty list.
    public List<int> GetLoanLimitViolators()
    {
        // TODO: implement
        return new List<int>();
    }
}

public class LibrarySystem
{

    private static int passed = 0, failed = 0;

    public static void Main(string[] args)
    {
        Console.WriteLine("\n=== LIBRARY BOOK MANAGEMENT — KARAT PRACTICE ===\n");

        Run("TASK 1 — Current checkout count (FIX BUG)", TestGetCurrentCheckoutCount);
        Run("TASK 2 — Overdue books by member",          TestGetOverdueBooksByMember);
        Run("TASK 3 — Calculate late fees",              TestCalculateLateFees);
        Run("BONUS — Loan limit violators",              TestGetLoanLimitViolators);

        Console.WriteLine("\n--------------------------------------------------");
        Console.WriteLine("Results: " + passed + " passed, " + failed +
                " failed out of " + (passed + failed));
    }

    // ================================================================
    //                            TESTS
    // ================================================================

    // <bug-task1>
    public static void TestGetCurrentCheckoutCount()
    {
        LibraryManager lm = new LibraryManager();
        lm.AddMember(new Member(1, "Alice", MemberTier.STANDARD));
        lm.AddMember(new Member(2, "Bob",   MemberTier.PREMIUM));
        lm.AddBook(new Book(1, "Dune",           Genre.FICTION));
        lm.AddBook(new Book(2, "A Brief History", Genre.SCIENCE));
        lm.AddBook(new Book(3, "Cosmos",          Genre.SCIENCE));

        lm.AddLoan(new Loan(1, 1, 1, 5,  LoanStatus.ACTIVE));    // counts
        lm.AddLoan(new Loan(2, 1, 2, 3,  LoanStatus.RETURNED));  // excluded
        lm.AddLoan(new Loan(3, 2, 1, 10, LoanStatus.OVERDUE));   // counts
        lm.AddLoan(new Loan(4, 2, 3, 7,  LoanStatus.RETURNED));  // excluded

        // 2 books currently checked out (ACTIVE + OVERDUE only)
        Check(lm.GetCurrentCheckoutCount() == 2,
                "Checkout count should be 2, was " + lm.GetCurrentCheckoutCount());
    }
    // </bug-task1>

    // <task2>
    public static void TestGetOverdueBooksByMember()
    {
        LibraryManager lm = new LibraryManager();
        foreach (int mid in new int[]{1, 2, 3, 4})
        {
            lm.AddMember(new Member(mid, "member" + mid, MemberTier.STANDARD));
        }
        lm.AddBook(new Book(1, "Book1", Genre.FICTION));
        lm.AddBook(new Book(2, "Book2", Genre.SCIENCE));
        lm.AddBook(new Book(3, "Book3", Genre.HISTORY));
        lm.AddBook(new Book(4, "Book4", Genre.NON_FICTION));

        // member 1: books 1 and 3 are OVERDUE, book 2 is RETURNED
        lm.AddLoan(new Loan(1, 1, 1, 5, LoanStatus.OVERDUE));
        lm.AddLoan(new Loan(2, 1, 3, 8, LoanStatus.OVERDUE));
        lm.AddLoan(new Loan(3, 1, 2, 3, LoanStatus.RETURNED));

        // member 2: book 2 is OVERDUE, book 4 is ACTIVE
        lm.AddLoan(new Loan(4, 2, 2, 6, LoanStatus.OVERDUE));
        lm.AddLoan(new Loan(5, 2, 4, 2, LoanStatus.ACTIVE));

        // member 3: only RETURNED — should not appear
        lm.AddLoan(new Loan(6, 3, 1, 4, LoanStatus.RETURNED));

        // member 4: no loans — should not appear

        Dictionary<int, List<int>> result = lm.GetOverdueBooksByMember();

        Check(result[1].SequenceEqual(new List<int> { 1, 3 }),
                "member1 overdue books should be [1,3], was " + FormatList(result.GetValueOrDefault(1)));
        Check(result[2].SequenceEqual(new List<int> { 2 }),
                "member2 overdue books should be [2], was " + FormatList(result.GetValueOrDefault(2)));
        Check(!result.ContainsKey(3),
                "member3 has no OVERDUE loans and should not appear");
        Check(!result.ContainsKey(4),
                "member4 has no loans and should not appear");
    }
    // </task2>

    // <task3>
    public static void TestCalculateLateFees()
    {
        LibraryManager lm = new LibraryManager();
        lm.AddMember(new Member(1, "Alice", MemberTier.STANDARD));
        lm.AddMember(new Member(2, "Bob",   MemberTier.PREMIUM));
        lm.AddMember(new Member(3, "Carol", MemberTier.STANDARD));
        lm.AddMember(new Member(4, "Dave",  MemberTier.PREMIUM));
        for (int bid = 1; bid <= 5; bid++)
        {
            lm.AddBook(new Book(bid, "Book" + bid, Genre.FICTION));
        }

        // member 1 (STANDARD, $0.50/day): 4 days + 6 days OVERDUE → 10 × $0.50 = $5.00
        lm.AddLoan(new Loan(1, 1, 1, 4, LoanStatus.OVERDUE));
        lm.AddLoan(new Loan(2, 1, 2, 6, LoanStatus.OVERDUE));
        lm.AddLoan(new Loan(3, 1, 3, 3, LoanStatus.RETURNED));  // no fee

        // member 2 (PREMIUM, $0.25/day): 8 days OVERDUE → 8 × $0.25 = $2.00
        lm.AddLoan(new Loan(4, 2, 1, 8, LoanStatus.OVERDUE));
        lm.AddLoan(new Loan(5, 2, 4, 2, LoanStatus.ACTIVE));    // no fee

        // member 3: only ACTIVE loan — no fees, should not appear
        lm.AddLoan(new Loan(6, 3, 2, 5, LoanStatus.ACTIVE));

        // member 4: no loans — should not appear

        Dictionary<int, double> fees = lm.CalculateLateFees();

        Check(Math.Abs(fees[1] - 5.0) < 0.0001,
                "member1 fee should be 5.0, was " + fees.GetValueOrDefault(1));
        Check(Math.Abs(fees[2] - 2.0) < 0.0001,
                "member2 fee should be 2.0, was " + fees.GetValueOrDefault(2));
        Check(!fees.ContainsKey(3),
                "member3 has no OVERDUE loans and should not appear in fees");
        Check(!fees.ContainsKey(4),
                "member4 has no loans and should not appear in fees");
    }
    // </task3>

    // <task4>
    public static void TestGetLoanLimitViolators()
    {
        LibraryManager lm = new LibraryManager();
        lm.AddMember(new Member(1, "Alice", MemberTier.STANDARD));  // limit = 3
        lm.AddMember(new Member(2, "Bob",   MemberTier.PREMIUM));   // limit = 5
        lm.AddMember(new Member(3, "Carol", MemberTier.STANDARD));  // limit = 3
        lm.AddMember(new Member(4, "Dave",  MemberTier.PREMIUM));   // limit = 5
        for (int bid = 1; bid <= 6; bid++)
        {
            lm.AddBook(new Book(bid, "Book" + bid, Genre.FICTION));
        }

        // member 1 (STANDARD, max 3): 4 ACTIVE/OVERDUE → EXCEEDS limit → VIOLATOR
        lm.AddLoan(new Loan(1,  1, 1, 2, LoanStatus.ACTIVE));
        lm.AddLoan(new Loan(2,  1, 2, 3, LoanStatus.ACTIVE));
        lm.AddLoan(new Loan(3,  1, 3, 1, LoanStatus.OVERDUE));
        lm.AddLoan(new Loan(4,  1, 4, 5, LoanStatus.ACTIVE));   // 4th → violator
        lm.AddLoan(new Loan(5,  1, 5, 2, LoanStatus.RETURNED)); // doesn't count

        // member 2 (PREMIUM, max 5): 3 ACTIVE/OVERDUE → within limit → OK
        lm.AddLoan(new Loan(6,  2, 1, 4, LoanStatus.ACTIVE));
        lm.AddLoan(new Loan(7,  2, 2, 6, LoanStatus.ACTIVE));
        lm.AddLoan(new Loan(8,  2, 3, 2, LoanStatus.OVERDUE));

        // member 3 (STANDARD, max 3): exactly 3 ACTIVE/OVERDUE → at limit, NOT a violator
        lm.AddLoan(new Loan(9,  3, 1, 3, LoanStatus.ACTIVE));
        lm.AddLoan(new Loan(10, 3, 2, 1, LoanStatus.OVERDUE));
        lm.AddLoan(new Loan(11, 3, 4, 4, LoanStatus.ACTIVE));

        // member 4 (PREMIUM, max 5): 6 ACTIVE/OVERDUE → EXCEEDS limit → VIOLATOR
        lm.AddLoan(new Loan(12, 4, 1, 1, LoanStatus.ACTIVE));
        lm.AddLoan(new Loan(13, 4, 2, 2, LoanStatus.ACTIVE));
        lm.AddLoan(new Loan(14, 4, 3, 3, LoanStatus.ACTIVE));
        lm.AddLoan(new Loan(15, 4, 4, 4, LoanStatus.OVERDUE));
        lm.AddLoan(new Loan(16, 4, 5, 5, LoanStatus.ACTIVE));
        lm.AddLoan(new Loan(17, 4, 6, 6, LoanStatus.OVERDUE));  // 6th → violator

        Check(lm.GetLoanLimitViolators().SequenceEqual(new List<int> { 1, 4 }),
                "Violators should be [1, 4], was " + FormatList(lm.GetLoanLimitViolators()));

        // edge case: exactly-at-limit is NOT a violator; RETURNED loans don't count
        LibraryManager lm2 = new LibraryManager();
        lm2.AddMember(new Member(10, "X", MemberTier.STANDARD));  // limit = 3
        lm2.AddMember(new Member(20, "Y", MemberTier.STANDARD));  // limit = 3
        lm2.AddMember(new Member(30, "Z", MemberTier.PREMIUM));   // limit = 5
        for (int bid = 1; bid <= 7; bid++)
        {
            lm2.AddBook(new Book(bid, "Book" + bid, Genre.FICTION));
        }

        // member 10: 2 ACTIVE + 3 RETURNED → current = 2 → OK
        lm2.AddLoan(new Loan(1, 10, 1, 2, LoanStatus.ACTIVE));
        lm2.AddLoan(new Loan(2, 10, 2, 1, LoanStatus.ACTIVE));
        lm2.AddLoan(new Loan(3, 10, 3, 3, LoanStatus.RETURNED));
        lm2.AddLoan(new Loan(4, 10, 4, 4, LoanStatus.RETURNED));
        lm2.AddLoan(new Loan(5, 10, 5, 2, LoanStatus.RETURNED));

        // member 20: exactly 3 ACTIVE → at STANDARD limit → NOT a violator
        lm2.AddLoan(new Loan(6,  20, 1, 1, LoanStatus.ACTIVE));
        lm2.AddLoan(new Loan(7,  20, 2, 2, LoanStatus.ACTIVE));
        lm2.AddLoan(new Loan(8,  20, 3, 3, LoanStatus.ACTIVE));

        // member 30: 6 ACTIVE → exceeds PREMIUM limit of 5 → VIOLATOR
        lm2.AddLoan(new Loan(9,  30, 1, 1, LoanStatus.ACTIVE));
        lm2.AddLoan(new Loan(10, 30, 2, 2, LoanStatus.ACTIVE));
        lm2.AddLoan(new Loan(11, 30, 3, 3, LoanStatus.ACTIVE));
        lm2.AddLoan(new Loan(12, 30, 4, 4, LoanStatus.ACTIVE));
        lm2.AddLoan(new Loan(13, 30, 5, 5, LoanStatus.ACTIVE));
        lm2.AddLoan(new Loan(14, 30, 6, 6, LoanStatus.ACTIVE));  // 6th → violator

        Check(lm2.GetLoanLimitViolators().SequenceEqual(new List<int> { 30 }),
                "Edge case violators should be [30], was " + FormatList(lm2.GetLoanLimitViolators()));
    }
    // </task4>

    // ================================================================
    //                           HELPERS
    // ================================================================

    private static string FormatList<T>(List<T> list)
    {
        if (list == null) return "null";
        return "[" + string.Join(", ", list) + "]";
    }

    private static void Check(bool condition, string msg)
    {
        if (!condition) throw new Exception(msg);
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            passed++;
            Console.WriteLine("  PASS: " + name);
        }
        catch (Exception e)
        {
            failed++;
            Console.WriteLine("  FAIL: " + name + " -> " + e.Message);
        }
    }
}

}
