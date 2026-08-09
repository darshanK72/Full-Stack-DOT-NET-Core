using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{

/* <bug-task1>
 * We are writing software to collect and manage data on how fast racers
 * complete obstacle courses. An obstacle course is a series of physical
 * challenges that a racer must go through.
 *
 * A "run" is one attempt at an obstacle course.
 * A "run collection" is a group of runs on a particular course.
 * Each obstacle has a time recorded for it.
 * A run may be incomplete (racer didn't finish all obstacles).
 *
 * Example data:
 *   Obstacles:    O1  O2  O3  O4
 *   Run 1:         3   4   5   6
 *   Run 2:         4   4   4   5
 *   Run 3:         4   5   4   6
 *   Run 4:         5   5   3        (incomplete)
 *
 * TASK 1: The tests are failing. Read the code, find the bug, fix it.
 </bug-task1>
 */
 /*<task2>
 * TASK 2: Implement bestOfBests().
 *   This is a measure of how fast a run could be if everything went
 *   perfectly. It is determined by taking the fastest time for each
 *   obstacle across all runs (even incomplete ones) and summing them.
 *   In the example above: 3, 4, 3, 5 → best of bests = 15 seconds.
 </task2>
 */
 /*<task3>
 * TASK 3: Implement chanceOfPersonalBest(Run inProgressRun).
 *   Given an in-progress run, simulate 10,000 trials. For each
 *   remaining obstacle, randomly pick a time from that obstacle's
 *   historical times across all existing runs. Return the fraction
 *   of trials where the simulated total <= personalBest().
 </task3>
 */

public class Course
{
    public string Title;
    public int ObstacleCount;

    public Course(string title, int obstacleCount)
    {
        this.Title = title;
        this.ObstacleCount = obstacleCount;
    }

    public override bool Equals(object o)
    {
        if (!(o is Course)) return false;
        Course c = (Course)o;
        return Title.Equals(c.Title) && ObstacleCount == c.ObstacleCount;
    }
}

public class Run
{
    public Course Course;
    public bool Complete;
    public List<int> ObstacleTimes;

    public Run(Course course)
    {
        this.Course = course;
        this.Complete = false;
        this.ObstacleTimes = new List<int>();
    }

    public void AddObstacleTime(int time)
    {
        if (Complete) throw new InvalidOperationException("Cannot add obstacle to complete run");
        ObstacleTimes.Add(time);
        if (ObstacleTimes.Count == Course.ObstacleCount)
        {
            Complete = true;
        }
    }

    public int GetRunTime()
    {
        int total = 0;
        foreach (int t in ObstacleTimes) total += t;
        return total;
    }
}

public class RunCollection
{
    public List<Run> Runs;
    public Course Course;

    public RunCollection(Course course)
    {
        this.Runs = new List<Run>();
        this.Course = course;
    }

    public int GetNumRuns()
    {
        return Runs.Count;
    }

    public void AddRun(Run run)
    {
        if (!run.Course.Equals(Course))
        {
            throw new ArgumentException("Run's course doesn't match collection's course");
        }
        Runs.Add(run);
    }

    // TASK 1: This method has a bug. Find and fix it.
    // Returns the fastest complete run time (personal best).
    public int PersonalBest()
    {
        return Runs.Count > 0 ? Runs.Min(r => r.GetRunTime()) : int.MaxValue;
    }

    /**
     * TASK 2: Best of bests.
     * Sum of the fastest time per obstacle across ALL runs (including incomplete).
     */
    public int BestOfBests()
    {
        // TODO: implement
        return 0;
    }

    /**
     * TASK 3: Chance of personal best.
     * Simulates 10,000 trials completing inProgressRun with random historical times.
     */
    public double ChanceOfPersonalBest(Run inProgressRun)
    {
        // TODO: implement
        return 0.0;
    }
}

public class ObstacleMockStub
{

    private static int passed = 0, failed = 0;

    public static void Main(string[] args)
    {
        Console.WriteLine("\n=== OBSTACLE COURSE — KARAT PRACTICE ===\n");

        RunTest("TASK 0 — Run basics", TestRun);
        RunTest("TASK 1 — Personal best", TestRunCollection);
        RunTest("TASK 2 — Best of bests", TestBestOfBests);
        RunTest("TASK 3 — Chance of personal best", TestChanceOfPersonalBest);

        Console.WriteLine("\n--------------------------------------------------");
        Console.WriteLine("Results: " + passed + " passed, " + failed +
                " failed out of " + (passed + failed));
    }

    private static RunCollection MakeRunCollection(Course course, int[][] data)
    {
        RunCollection rc = new RunCollection(course);
        foreach (int[] times in data)
        {
            global::DotNetQuestions.Run run = new global::DotNetQuestions.Run(course);
            foreach (int t in times) run.AddObstacleTime(t);
            rc.AddRun(run);
        }
        return rc;
    }

    public static void TestRun()
    {
        Course c = new Course("Test", 2);
        global::DotNetQuestions.Run r = new global::DotNetQuestions.Run(c);
        r.AddObstacleTime(3);
        if (r.Complete) throw new Exception("Should not be complete yet");
        r.AddObstacleTime(5);
        if (!r.Complete) throw new Exception("Should be complete now");
        if (r.GetRunTime() != 8) throw new Exception("Run time should be 8");
        try
        {
            r.AddObstacleTime(4);
            throw new Exception("Should have thrown exception");
        }
        catch (InvalidOperationException)
        {
            // expected
        }
    }

    // <bug-task1>
    public static void TestRunCollection()
    {
        Course c = new Course("Test", 4);
        int[][] data = { new int[] { 3, 4, 5, 6 }, new int[] { 4, 4, 4, 5 }, new int[] { 4, 5, 4, 6 }, new int[] { 5, 5, 3 } };
        RunCollection rc = MakeRunCollection(c, data);

        if (rc.GetNumRuns() != 4) throw new Exception("Should have 4 runs");
        if (rc.PersonalBest() != 17)
            throw new Exception("Personal best should be 17, was " + rc.PersonalBest());
    }
    // </bug-task1>

    // <task2>
    public static void TestBestOfBests()
    {
        Course c = new Course("Test", 4);
        int[][] data = { new int[] { 3, 4, 5, 6 }, new int[] { 4, 4, 4, 5 }, new int[] { 4, 5, 4, 6 }, new int[] { 5, 5, 3 } };
        RunCollection rc = MakeRunCollection(c, data);

        if (rc.BestOfBests() != 15)
            throw new Exception("Best of bests should be 15, was " + rc.BestOfBests());
    }
    // </task2>

    // <task3>
    public static void TestChanceOfPersonalBest()
    {
        Course c1 = new Course("Test", 3);
        int[][] data1 = { new int[] { 3, 3, 2 }, new int[] { 3, 3, 3 } };
        RunCollection rc1 = MakeRunCollection(c1, data1);
        global::DotNetQuestions.Run test1 = new global::DotNetQuestions.Run(c1);
        test1.AddObstacleTime(3);
        test1.AddObstacleTime(3);
        double chance1 = rc1.ChanceOfPersonalBest(test1);
        if (!(chance1 >= 0.48 && chance1 <= 0.52))
            throw new Exception("Chance should be ~0.50, was " + chance1);

        Course c2 = new Course("Test", 4);
        int[][] data2 = { new int[] { 3, 3, 2, 3 }, new int[] { 3, 3, 3, 2 }, new int[] { 5, 5, 2 } };
        RunCollection rc2 = MakeRunCollection(c2, data2);
        global::DotNetQuestions.Run test2 = new global::DotNetQuestions.Run(c2);
        test2.AddObstacleTime(3);
        test2.AddObstacleTime(3);
        double chance2 = rc2.ChanceOfPersonalBest(test2);
        if (!(chance2 >= 0.813 && chance2 <= 0.853))
            throw new Exception("Chance should be ~0.833, was " + chance2);
    }
    // </task3>

    // ================================================================
    //                      HELPERS
    // ================================================================

    private static void RunTest(string name, Action test)
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
