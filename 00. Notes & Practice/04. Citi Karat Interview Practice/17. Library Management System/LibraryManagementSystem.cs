// To clear the interview, you must complete at least one bug fix and two tasks.
// A single task may involve multiple functions; sub-parts like 2.1 and 2.2 are considered one task.
// Please provide a verbal walkthrough of your thought process while writing the code.

/*
CHALLENGE: Library Management System

DESCRIPTION:
Build an in-memory library management system that tracks library loans. A Member has a unique Id, a Name, and a Tier; the supported tiers are BASIC, STANDARD, and PREMIUM. A Loan records the MemberId, BookId, StartDay, and nullable EndDay, where all day values are numeric offsets from an arbitrary starting point and a null EndDay means the loan is still active. The LibraryManager owns the members and stored loans and provides statistics, data-management, tiered billing, and overlap analysis.

TASK 1 — BUG FIX: LOAN STATISTICS
Implement the manager method `GetLoanStatistics()` with return type `LoanStatistics`. It must examine every stored loan and return an object with exactly these integer fields: `TotalLoans`, `ActiveLoans`, and `CompletedLoans`. `TotalLoans` must equal the number of stored loans. A loan is active exactly when `EndDay` is null. A loan is completed exactly when `EndDay` has a value, including when that value is 0. `ActiveLoans` and `CompletedLoans` must partition the stored loans, so their sum must equal `TotalLoans`. The method must not modify the stored collection.
Example: with loans ending at null, 0, and 5, the expected result is `TotalLoans = 3`, `ActiveLoans = 1`, and `CompletedLoans = 2`. The loan ending at day 0 is completed.

TASK 2 — DATA MANAGEMENT
Implement the manager operation `RecordEventAndGetAverage(Loan loanToRecord)` with return type `double`. When `loanToRecord` is non-null, append that loan to the in-memory collection before calculating the result. When it is null, do not append anything. The returned value must be the arithmetic mean of `EndDay - StartDay` across all completed loans currently stored, where completed means `EndDay` is non-null. Active loans must be excluded. If there are no completed loans, return exactly `0.0`. Repeated calls must preserve every previously recorded loan.
Example: recording completed loans from day 2 to day 6 and from day 10 to day 19, then calling the operation with null, must return `(4 + 9) / 2 = 6.5`. If the collection contains only an active loan, the result must be `0.0`.

TASK 3 — CONDITIONAL LOGIC / TIERS: MEMBER AMOUNTS DUE
Implement `CalculateAmountsDue()` with return type `Dictionary<string, double>`, mapping every registered member's Id to that member's total amount due. Only completed loans are billable; active loans contribute nothing. For every completed loan, calculate duration as `EndDay - StartDay`, round it upward to the next whole unit using the mathematical ceiling operation, then subtract the member's tier allowance, never allowing billable units below zero. Multiply the billable units by the tier rate and sum all amounts for that member. BASIC has 2 free units and a rate of $3.00 per billable unit. STANDARD has 14 free units and a rate of $1.00 per billable unit. PREMIUM has 20 free units and a rate of $0.50 per billable unit. The result must contain every registered member, including members with no completed loans, with such members mapped to `0.0`. A member Id in a stored loan is guaranteed to identify a registered member.
Example: a STANDARD loan from day 0 to day 16.5 has rounded duration `ceil(16.5) = 17`, billable units `17 - 14 = 3`, and amount `$3.00`. A PREMIUM loan from day 0 to day 20 has billable units 0 and amount `$0.00`.

TASK 4 — ALGORITHM / OVERLAP: SHARED BOOK LOANS
Implement `FindOverlaps()` with return type `List<LoanOverlap>`. Consider only completed loans, and compare loans only when they have the same BookId and different MemberIds. Two time windows overlap only when their shared duration is strictly greater than zero. For intervals `[start, end]` and `[otherStart, otherEnd]`, shared duration is `min(end, otherEnd) - max(start, otherStart)`. A pair that merely touches at one boundary has shared duration zero and must be excluded. Produce at most one result for each unordered pair of member Ids, even if those members have multiple overlapping loans. The result's `Duration` must be the sum of all positive shared durations for that member pair across matching books and loans. Each result must contain the two member Ids and the BookId for the overlap; test data will not require combining different BookIds into one result. Sort the returned list by `Duration` descending. Pairs with no positive overlap must not appear.
Example: Member A has Book1 from day 0 to 5 and Member B has Book1 from day 2 to 8, so their overlap is 3 days and the result contains a duration of `3`. Loans from day 0 to 5 and day 5 to 9 only touch at day 5, so they produce no result.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

[TestFixture]
public class LibraryManagementSystemAssessment
{
    public enum MemberTier
    {
        BASIC,
        STANDARD,
        PREMIUM
    }

    public class Member
    {
        public string Id { get; }
        public string Name { get; }
        public MemberTier Tier { get; }

        public Member(string id, string name, MemberTier tier)
        {
            Id = id;
            Name = name;
            Tier = tier;
        }
    }

    public class Loan
    {
        public string MemberId { get; }
        public string BookId { get; }
        public double StartDay { get; }
        public double? EndDay { get; }

        public Loan(string memberId, string bookId, double startDay, double? endDay)
        {
            MemberId = memberId;
            BookId = bookId;
            StartDay = startDay;
            EndDay = endDay;
        }
    }

    public class LoanStatistics
    {
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int CompletedLoans { get; set; }
    }

    public class LoanOverlap
    {
        public string FirstMemberId { get; set; }
        public string SecondMemberId { get; set; }
        public string BookId { get; set; }
        public double Duration { get; set; }
    }

    public class LibraryManager
    {
        private readonly List<Member> members;
        private readonly List<Loan> loans;

        public LibraryManager(IEnumerable<Member> members)
        {
            this.members = new List<Member>(members);
            loans = new List<Loan>();
        }

        public LoanStatistics GetLoanStatistics()
        {
            int completed = loans.Where(loan => loan.EndDay != null).Count();
            int active = loans.Where(loan => loan.EndDay == null).Count();
            return new LoanStatistics
            {
                TotalLoans = loans.Count,
                ActiveLoans = active,
                CompletedLoans = completed
            };
        }

        public double RecordEventAndGetAverage(Loan loanToRecord)
        {
            if(loanToRecord != null){
                loans.Add(loanToRecord);
            }

            var completedLoans = loans.Where(loan => loan.EndDay != null).ToList();
            int completedLoansCount = completedLoans.Count;
            if(completedLoansCount == 0) return 0.0;
            double durationSum = loans.Sum(loan => (double)(loan.EndDay - loan.StartDay));
            return durationSum/completedLoansCount;
        }

        public Dictionary<string, double> CalculateAmountsDue()
        {
            return new Dictionary<string, double>();
        }

        public List<LoanOverlap> FindOverlaps()
        {
            return new List<LoanOverlap>();
        }
    }

    [Test]
    public void Task1_GetLoanStatistics_CoversNullAndZeroEndDays()
    {
        var members = new[] { new Member("m1", "Ada", MemberTier.BASIC) };
        var manager = new LibraryManager(members);
        manager.RecordEventAndGetAverage(new Loan("m1", "b1", 0, null));
        manager.RecordEventAndGetAverage(new Loan("m1", "b2", 2, 0));
        manager.RecordEventAndGetAverage(new Loan("m1", "b3", 4, 5));

        LoanStatistics statistics = manager.GetLoanStatistics();

        Assert.AreEqual(3, statistics.TotalLoans);
        Assert.AreEqual(1, statistics.ActiveLoans);
        Assert.AreEqual(2, statistics.CompletedLoans);
    }

    [Test]
    public void Task2_RecordAndAverage_CoversRelevantIrrelevantEmptyAndMixedData()
    {
        var member = new Member("m1", "Ada", MemberTier.STANDARD);
        var manager = new LibraryManager(new[] { member });

        Assert.AreEqual(0.0, manager.RecordEventAndGetAverage(null), 0.000001);

        Assert.AreEqual(4.0, manager.RecordEventAndGetAverage(new Loan("m1", "b1", 2, 6)), 0.000001);
        Assert.AreEqual(6.5, manager.RecordEventAndGetAverage(new Loan("m1", "b2", 10, 19)), 0.000001);
        Assert.AreEqual(6.5, manager.RecordEventAndGetAverage(new Loan("m1", "b3", 20, null)), 0.000001);

        var irrelevantOnlyManager = new LibraryManager(new[] { member });
        Assert.AreEqual(0.0, irrelevantOnlyManager.RecordEventAndGetAverage(new Loan("m1", "b4", 3, null)), 0.000001);

        var mixedManager = new LibraryManager(new[] { member });
        Assert.AreEqual(8.0, mixedManager.RecordEventAndGetAverage(new Loan("m1", "b5", 1, 9)), 0.000001);
        Assert.AreEqual(8.0, mixedManager.RecordEventAndGetAverage(new Loan("m1", "b6", 12, null)), 0.000001);
        Assert.AreEqual(8.0, mixedManager.RecordEventAndGetAverage(null), 0.000001);
    }

    [Test]
    public void Task3_CalculateAmountsDue_CoversAllTiersRoundingBoundariesAndMissingData()
    {
        var basic = new Member("basic", "B", MemberTier.BASIC);
        var standard = new Member("standard", "S", MemberTier.STANDARD);
        var premium = new Member("premium", "P", MemberTier.PREMIUM);
        var noLoans = new Member("none", "N", MemberTier.STANDARD);
        var manager = new LibraryManager(new[] { basic, standard, premium, noLoans });

        manager.RecordEventAndGetAverage(new Loan("basic", "b1", 0, 2));
        manager.RecordEventAndGetAverage(new Loan("standard", "b2", 0, 16.5));
        manager.RecordEventAndGetAverage(new Loan("premium", "b3", 0, 20.1));
        manager.RecordEventAndGetAverage(new Loan("basic", "b4", 0, null));

        Dictionary<string, double> amounts = manager.CalculateAmountsDue();

        Assert.AreEqual(4, amounts.Count);
        Assert.AreEqual(0.0, amounts["basic"], 0.000001);
        Assert.AreEqual(3.0, amounts["standard"], 0.000001);
        Assert.AreEqual(0.5, amounts["premium"], 0.000001);
        Assert.AreEqual(0.0, amounts["none"], 0.000001);
    }

    [Test]
    public void Task4_FindOverlaps_CoversSortingExclusionAndBoundaryTouching()
    {
        var a = new Member("a", "A", MemberTier.BASIC);
        var b = new Member("b", "B", MemberTier.BASIC);
        var c = new Member("c", "C", MemberTier.BASIC);
        var d = new Member("d", "D", MemberTier.BASIC);
        var manager = new LibraryManager(new[] { a, b, c, d });

        manager.RecordEventAndGetAverage(new Loan("a", "book1", 0, 5));
        manager.RecordEventAndGetAverage(new Loan("b", "book1", 2, 5));
        manager.RecordEventAndGetAverage(new Loan("a", "book2", 10, 14));
        manager.RecordEventAndGetAverage(new Loan("c", "book2", 12, 14));
        manager.RecordEventAndGetAverage(new Loan("a", "book3", 20, 25));
        manager.RecordEventAndGetAverage(new Loan("d", "book3", 25, 30));
        manager.RecordEventAndGetAverage(new Loan("b", "book4", 40, 45));
        manager.RecordEventAndGetAverage(new Loan("c", "book4", 46, 50));

        List<LoanOverlap> overlaps = manager.FindOverlaps();

        Assert.AreEqual(2, overlaps.Count);
        Assert.AreEqual("a", overlaps[0].FirstMemberId);
        Assert.AreEqual("b", overlaps[0].SecondMemberId);
        Assert.AreEqual("book1", overlaps[0].BookId);
        Assert.AreEqual(3.0, overlaps[0].Duration, 0.000001);
        Assert.AreEqual("a", overlaps[1].FirstMemberId);
        Assert.AreEqual("c", overlaps[1].SecondMemberId);
        Assert.AreEqual("book2", overlaps[1].BookId);
        Assert.AreEqual(2.0, overlaps[1].Duration, 0.000001);
        Assert.IsFalse(overlaps.Any(item => item.FirstMemberId == "a" && item.SecondMemberId == "d"));
        Assert.IsFalse(overlaps.Any(item => item.FirstMemberId == "b" && item.SecondMemberId == "c"));
    }
}
