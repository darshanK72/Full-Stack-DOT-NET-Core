using System;
using System.Collections.Generic;

/**
 *
We are building a system to manage physical access to a corporate campus. Each user has a unique ID, a name, and an access level. Access levels are ranked: VISITOR is the lowest, STAFF is higher, and ADMIN is the highest. A user is authorized for a required access level when their own level is the same or higher — so an ADMIN is authorized for STAFF and VISITOR areas, and a STAFF user is authorized for VISITOR areas.

Definitions:
* A "user" is an object representing someone with campus access. It has a user_id, a name,
  and an access_level.
* VISITOR is the default access level a new user receives.
* An "AccessManager" is the class used to manage all users on the campus.

To begin with, two tasks:
1-1) Read through and understand the code below.
1-2) The test for AccessManager is not passing due to a bug in the code. Make the
   necessary changes to AccessManager to fix the bug.
   Access levels are ranked: VISITOR (1), STAFF (2), ADMIN (3). A user is authorized
   for a required access level when their own level is the same or higher.
We are extending the system to record building access. The new AccessEvent class
represents one badge-in/badge-out session for a user. Each event has a unique ID, an entry_time, and an exit_time, all measured in minutes since the start of the day. All events happen on the same day.

The campus is open from minute 480 (08:00) to minute 1200 (20:00). An event is
"after-hours" if it starts before 480 or ends after 1200. Starting exactly at 480 or ending exactly at 1200 still counts as within hours.

Add two methods to AccessManager:

2.1) log_access(user_id, event): add the event to that user. Return True if the user
   exists. If the user does not exist, ignore the event and return False.

2.2) get_after_hours_users(): return the IDs of all users who have at least one
   after-hours event, sorted in ascending order. Do not include a user who has no
   after-hours events, including a user with no events at all.

The test_log_access and test_get_after_hours_users tests are provided.

We are adding a building utilization report. For capacity planning we want to know, for each hour of the day, how many distinct users were present in the building during that hour.
The day is divided into 24 one-hour buckets: hour h covers the minutes from h*60 up to (but not including) (h+1)*60.

For example, hour 0 is minutes 0–59, hour 1 is minutes 60–119, and so on. A user is "present during hour h" if any of their access events overlaps that hour. A session that ends exactly on an hour boundary does not count as present in the next hour (an event covering minutes 120–180 is present in hour 2 only,
not hour 3).

3.1) Implement a function get_hourly_occupancy(). It takes no arguments (other than self) and returns a dictionary mapping each hour to the number of distinct users present during that hour. Only include hours that have at least one user present; omit hours with no users.

Example:

Input:
  user 20 has one session: minute 130 to 260
  user 21 has one session: minute 200 to 230

Output (returned by get_hourly_occupancy()):
  {2: 1, 3: 2, 4: 1}

Explanation:
User 20's session covers hour 2 (minutes 120–180), hour 3 (180–240), and hour 4
(240–300). User 21's session falls entirely within hour 3. Hour 3 therefore has two users present, while hours 2 and 4 each have only user 20, giving {2: 1, 3: 2, 4: 1}.

The test_get_hourly_occupancy test is provided.

Task 4 generated manually not came in karat yet.
4.1) Implement get_total_minutes_on_campus(user_id). Return the sum of the durations
   of all events for a specific user. Duration is calculated as (exit_time - entry_time).
   If the user does not exist or has no events, return 0.
 */

namespace DotNetQuestions
{
    enum AccessLevel
    {
        VISITOR = 1,
        STAFF = 2,
        ADMIN = 3
    }

    static class AccessLevelExtensions
    {
        // Mirrors the Java enum's "rank" field (VISITOR=1, STAFF=2, ADMIN=3).
        public static int Rank(this AccessLevel level)
        {
            return (int)level;
        }
    }

    class AccessEvent
    {
        public int id;
        public int entryTime;
        public int exitTime;

        public AccessEvent(int id, int entryTime, int exitTime)
        {
            this.id = id;
            this.entryTime = entryTime;
            this.exitTime = exitTime;
        }
    }

    class User
    {
        public int userId;
        public string name;
        public AccessLevel accessLevel;

        public User(int userId, string name, AccessLevel accessLevel)
        {
            this.userId = userId;
            this.name = name;
            this.accessLevel = accessLevel;
        }
    }

    class AccessManager
    {
        public List<User> users = new List<User>();
        public Dictionary<int, List<AccessEvent>> userEvents = new Dictionary<int, List<AccessEvent>>();

        public void AddUser(User user)
        {
            this.users.Add(user);
        }

        // <bug/task1>
        public bool IsAuthorized(int userId, AccessLevel requiredLevel)
        {
            foreach (User user in users)
            {
                if (user.userId == userId)
                {
                    // BUG: Still using exact match instead of rank
                    return user.accessLevel == requiredLevel;
                }
            }
            return false;
        }
        // </bug/task1>

        // <task2>
        public bool LogAccess(int userId, AccessEvent evt)
        {
            // Implementation needed
            return false;
        }

        public List<int> GetAfterHoursUsers()
        {
            // Implementation needed
            return new List<int>();
        }
        // </task2>

        // <task3>
        public Dictionary<int, int> GetHourlyOccupancy()
        {
            // Implementation needed
            return new Dictionary<int, int>();
        }
        // </task3>

        // <task4>
        public int GetTotalMinutesOnCampus(int userId)
        {
            // Implementation needed
            return 0;
        }
        // </task4>
    }

    public class Solution
    {
        public static void Main(string[] args)
        {
            TestSuite tests = new TestSuite();
            int passed = 0;
            int failed = 0;

            string[] testNames = { "testIsAuthorized", "testLogAccessAndAfterHours", "testGetHourlyOccupancy", "testTotalMinutes" };

            foreach (string testName in testNames)
            {
                try
                {
                    Console.Write("Running " + testName + "... ");
                    if (testName.Equals("testIsAuthorized")) tests.TestIsAuthorized();
                    else if (testName.Equals("testLogAccessAndAfterHours")) tests.TestLogAccessAndAfterHours();
                    else if (testName.Equals("testGetHourlyOccupancy")) tests.TestGetHourlyOccupancy();
                    else if (testName.Equals("testTotalMinutes")) tests.TestTotalMinutes();

                    Console.WriteLine("PASSED");
                    passed++;
                }
                catch (Exception e)
                {
                    Console.WriteLine("FAILED");
                    Console.WriteLine("   Error: " + e.Message);
                    failed++;
                }
            }

            Console.WriteLine("\n---------------------------");
            Console.WriteLine("Results: " + passed + " Passed, " + failed + " Failed");
            Console.WriteLine("---------------------------");
        }
    }

    class TestSuite
    {
        private void AssertCondition(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        private void AssertEquals(object expected, object actual, string message)
        {
            if (!Equals(expected, actual))
            {
                throw new Exception(message + " (Expected: " + expected + ", Got: " + actual + ")");
            }
        }

        // <test/task1>
        public void TestIsAuthorized()
        {
            AccessManager manager = new AccessManager();
            manager.AddUser(new User(1, "Ada Admin", AccessLevel.ADMIN));
            AssertCondition(manager.IsAuthorized(1, AccessLevel.VISITOR), "Admin should access VISITOR level");
            AssertCondition(manager.IsAuthorized(1, AccessLevel.STAFF), "Admin should access STAFF level");
        }
        // </test/task1>

        // <test/task2>
        public void TestLogAccessAndAfterHours()
        {
            AccessManager manager = new AccessManager();
            manager.AddUser(new User(10, "Early Bird", AccessLevel.STAFF));
            manager.LogAccess(10, new AccessEvent(1, 400, 500));

            List<int> result = manager.GetAfterHoursUsers();
            AssertCondition(result != null && result.Count == 1 && result[0] == 10, "User 10 should be in after-hours list");
        }
        // </test/task2>

        // <test/task3>
        public void TestGetHourlyOccupancy()
        {
            AccessManager manager = new AccessManager();
            manager.AddUser(new User(20, "User 20", AccessLevel.VISITOR));
            manager.LogAccess(20, new AccessEvent(1, 130, 260));

            Dictionary<int, int> occ = manager.GetHourlyOccupancy();
            AssertCondition(occ.ContainsKey(2), "Should contain Hour 2");
        }
        // </test/task3>

        // <test/task4>
        public void TestTotalMinutes()
        {
            AccessManager manager = new AccessManager();
            manager.AddUser(new User(1, "User 1", AccessLevel.STAFF));
            manager.LogAccess(1, new AccessEvent(1, 100, 150));
            AssertEquals(50, manager.GetTotalMinutesOnCampus(1), "Total minutes should be 50");
        }
        // </test/task4>
    }
}
