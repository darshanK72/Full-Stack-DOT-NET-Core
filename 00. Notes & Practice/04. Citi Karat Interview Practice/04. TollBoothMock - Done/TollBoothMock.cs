/*<bug-task1>
* We are writing software to analyze logs for toll booths on a highway.
* This highway is a divided highway with limited access; the only way on
* or off the highway is through a toll booth.
*
* There are three types of toll booths:
*   ENTRY  — where a car goes through a booth as it enters the highway
*   EXIT   — where a car goes through a booth as it exits the highway
*   MAINROAD (M) — sensors that record a license plate as a car drives
*                   through at full speed
*
*       Exit Booth           Entry Booth
*           |                    |
*   --------X--------------------X--------
*        /                          \
*       /            M               \
*   ---X-------------X---------------X---
*       \                            /
*        \                          /
*   ------X------------------------X------
*           |                    |
*       Entry Booth           Exit Booth
*
* The log entries look like:
*   ["ENTRY", "ABC123", "2023-06-25 10:00:00"]
*   ["M",     "ABC123", "2023-06-25 10:05:00"]
*   ["EXIT",  "ABC123", "2023-06-25 10:30:00"]
*
* TASK 1: Read through code, find and fix the bug, run the tests.
</bug-task1>
*/
/*<task2>
* TASK 2: Implement findMismatchedEntries() — find cars that entered
*         but never exited, or exited but never entered.
</task2>
*/
/*<task3>
* TASK 3: Implement getAverageTripTime() — average time on highway.
  </task3>
*/
/*<task4>
   * TASK 4  For each car that has both ENTRY and EXIT,
   * compute how many MAINROAD sensors it passed.
   * Returns Map: plate -> count of M logs between entry and exit.
   </task4>
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{
    class TollBoothLog
    {
        public string type;       // "ENTRY", "EXIT", or "M"
        public string plate;      // license plate number
        public string timestamp;  // e.g., "2023-06-25 10:00:00"

        public TollBoothLog(string type, string plate, string timestamp)
        {
            this.type = type;
            this.plate = plate;
            this.timestamp = timestamp;
        }

        public override string ToString()
        {
            return "[" + type + ", " + plate + ", " + timestamp + "]";
        }
    }

    class HighwayTracker
    {
        List<TollBoothLog> logs;

        public HighwayTracker()
        {
            this.logs = new List<TollBoothLog>();
        }

        public void AddLog(string type, string plate, string timestamp)
        {
            logs.Add(new TollBoothLog(type, plate, timestamp));
        }

        /**
         * TASK 1: This method has a bug. Find and fix it.
         * Should return a Set of unique license plates.
         */
        public HashSet<string> GetUniquePlates()
        {
            HashSet<string> plates = new HashSet<string>();
            foreach (TollBoothLog log in logs)
            {
                plates.Add(log.plate);
            }
            return plates;
        }

        /**
         * TASK 2: Find cars that entered but never exited, or exited but never entered.
         * Returns Map<plate, "NO_EXIT" | "NO_ENTRY">
         */
        public Dictionary<string, string> FindMismatchedEntries()
        {
            HashSet<string> entered = logs.Where(log => log.type == "ENTRY").Select(log => log.plate).ToHashSet();
            HashSet<string> exited = logs.Where(log => log.type == "EXIT").Select(log => log.plate).ToHashSet();

            var output = new Dictionary<string, string>();
            foreach (string plate in entered.Except(exited))
            {
                output[plate] = "NO_EXIT";
            }
            foreach (string plate in exited.Except(entered))
            {
                output[plate] = "NO_ENTRY";
            }

            return output;
        }

        /**
         * TASK 3: Count cars currently on the highway (entered but not exited).
         */
        public int GetCarsOnHighway()
        {
            HashSet<string> entered = this.logs.Where(log => log.type == "ENTRY").Select(log => log.plate).ToHashSet();
            HashSet<string> exited = this.logs.Where(log => log.type == "EXIT").Select(log => log.plate).ToHashSet();

            return entered.Except(exited).Count();
        }

        /**
         * TASK 4: Count MAINROAD sensor passes per car.
         * Returns Map: plate -> count of M logs.
         */
        public Dictionary<string, int> GetMainroadPassCount()
        {

            // Only those cars who entered and exited as well.
            // HashSet<string> entered = logs.Where(log => log.type == "ENTRY").Select(log => log.plate).ToHashSet();
            // HashSet<string> exited = logs.Where(log => log.type == "EXIT").Select(log => log.plate).ToHashSet();

            // var onHighwayCars = entered.Intersect(exited).ToHashSet();

            Dictionary<string,int> output = new Dictionary<string, int>();
            // return onHighwayCars.ToDictionary(plate => plate,plate => logs.Count(l => l.plate == plate && l.type == "M"));
        

            foreach(TollBoothLog log in logs){
                if(log.type == "M"){
                    if(!output.ContainsKey(log.plate)){
                        output[log.plate] = 0;
                    }
                    output[log.plate]++;
                }
            }
            return output;
        }
    }

    public class TollBoothMockStub
    {
        public static void Main(string[] args)
        {
            TestGetUniquePlates();
            TestFindMismatchedEntries();
            TestGetCarsOnHighway();
            TestGetMainroadPassCount();
            Console.WriteLine("All Toll Booth tests passed!");
        }

        static HighwayTracker CreateSampleTracker()
        {
            HighwayTracker tracker = new HighwayTracker();
            // Car ABC123: enters and exits normally
            tracker.AddLog("ENTRY", "ABC123", "2023-06-25 10:00:00");
            tracker.AddLog("M", "ABC123", "2023-06-25 10:05:00");
            tracker.AddLog("M", "ABC123", "2023-06-25 10:15:00");
            tracker.AddLog("EXIT", "ABC123", "2023-06-25 10:30:00");

            // Car DEF456: enters but never exits (still on highway)
            tracker.AddLog("ENTRY", "DEF456", "2023-06-25 10:10:00");
            tracker.AddLog("M", "DEF456", "2023-06-25 10:20:00");

            // Car GHI789: exits but never entered (data anomaly)
            tracker.AddLog("EXIT", "GHI789", "2023-06-25 10:45:00");

            // Car JKL012: enters and exits
            tracker.AddLog("ENTRY", "JKL012", "2023-06-25 11:00:00");
            tracker.AddLog("EXIT", "JKL012", "2023-06-25 11:45:00");

            return tracker;
        }

        // <bug-task1>
        static void TestGetUniquePlates()
        {
            Console.WriteLine("Running testGetUniquePlates");
            HighwayTracker tracker = CreateSampleTracker();
            HashSet<string> plates = tracker.GetUniquePlates();
            Assert(plates.Count == 4, "Should have 4 unique plates, got " + plates.Count);
            Assert(plates.Contains("ABC123"), "Should contain ABC123");
            Assert(plates.Contains("DEF456"), "Should contain DEF456");
            Assert(plates.Contains("GHI789"), "Should contain GHI789");
            Assert(plates.Contains("JKL012"), "Should contain JKL012");
        }
        // </bug-task1>

        // <task2>
        static void TestFindMismatchedEntries()
        {
            Console.WriteLine("Running testFindMismatchedEntries");
            HighwayTracker tracker = CreateSampleTracker();
            Dictionary<string, string> mismatches = tracker.FindMismatchedEntries();
            Assert(mismatches.Count == 2, "Should have 2 mismatches, got " + mismatches.Count);
            Assert("NO_EXIT".Equals(MapGet(mismatches, "DEF456")),
                    "DEF456 should be NO_EXIT, was " + MapGet(mismatches, "DEF456"));
            Assert("NO_ENTRY".Equals(MapGet(mismatches, "GHI789")),
                    "GHI789 should be NO_ENTRY, was " + MapGet(mismatches, "GHI789"));
        }
        // </task2>

        // <task3>
        static void TestGetCarsOnHighway()
        {
            Console.WriteLine("Running testGetCarsOnHighway");
            HighwayTracker tracker = CreateSampleTracker();
            int count = tracker.GetCarsOnHighway();
            // ABC123 entered & exited, DEF456 entered (still on), GHI789 exited only, JKL012 entered & exited
            Assert(count == 1, "Should have 1 car on highway (DEF456), got " + count);
        }
        // </task3>

        // <task4>
        static void TestGetMainroadPassCount()
        {
            Console.WriteLine("Running testGetMainroadPassCount");
            HighwayTracker tracker = CreateSampleTracker();
            Dictionary<string, int> passes = tracker.GetMainroadPassCount();
            Assert(passes["ABC123"] == 2, "ABC123 should have 2 M passes, got " + passes["ABC123"]);
            Assert(passes["DEF456"] == 1, "DEF456 should have 1 M pass, got " + passes["DEF456"]);
            Assert(passes.GetValueOrDefault("JKL012", 0) == 0,
                    "JKL012 should have 0 M passes");
        }
        // </task4>

        // Java's "assert cond : message;" translated to a throwing helper,
        // since C# has no direct language-level equivalent enabled by default.
        static void Assert(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        // Null-safe lookup mirroring Java's Map.get() (returns null instead of throwing).
        static string MapGet(Dictionary<string, string> map, string key)
        {
            return map.TryGetValue(key, out string value) ? value : null;
        }
    }
}
