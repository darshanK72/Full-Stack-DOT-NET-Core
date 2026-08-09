/*<bug-task1>
 * We are extending the gym membership problem with workout tracking.
 *
 * Classes:
 *   MembershipStatus      — enum: BRONZE (default), SILVER, GOLD (paid)
 *   Workout               — workout session (id, startTime, endTime in minutes)
 *   Member                — gym member (memberId, name, membershipStatus)
 *   Membership            — manages members and workouts
 *
 * TASK 1: Read the code. The test for Membership is not passing
 *         due to a bug. Find and fix the bug.
 </bug-task1>
 */

/*<task2>
 * Implement the following methods:
 *
 * 2.1) The addWorkout function should be used to add a workout session
 *      for a given member ID. If the workout is stored successfully,
 *      the function should return true. If the given member does not
 *      exist while calling this function, the workout should be ignored
 *      and the function should return false.
 *
 * 2.2) The getAverageWorkoutDurations function should generate a map
 *      of member IDs onto their average workout durations in minutes.
 *      The returned map should include all members. If a member has no
 *      workouts, their value should be null.
 </task2>
 */

/*<task3>
 * TASK 3: Implement getDuePayments() — tiered pricing:
 *   - BRONZE: 1st workout free, then $10/hour from 2nd onward
 *   - SILVER: first 3 free, then $8/hour from 4th onward
 *   - GOLD:   first 5 free, then $6/hour from 6th onward
 *   Workouts ordered by their ID.
 *   Duration rounded UP to nearest hour (80 min → 2 hours).
 *   Return Map<Integer, Integer>: memberId → total payment (ALL members).
 </task3>
 */

/*<task4>
 * TASK 4: Implement getGymBuddies() — concurrent gym users:
 *   Two members are buddies if any of their workout slots overlap.
 *   Return Map<Integer, List<Integer>>: memberId → buddy IDs,
 *   sorted by total shared duration descending. All members included.
 </task4>
 */

using System;
using System.Collections.Generic;

namespace DotNetQuestions
{
    public class GymMembershipNew
    {
        enum MembershipStatus { BRONZE, SILVER, GOLD }

        class Workout
        {
            private int id, startTime, endTime;
            public Workout(int id, int startTime, int endTime)
            {
                this.id = id; this.startTime = startTime; this.endTime = endTime;
            }
            public int GetId() { return id; }
            public int GetStartTime() { return startTime; }
            public int GetEndTime() { return endTime; }
            public int GetDuration() { return endTime - startTime; }
        }

        class Member
        {
            public int memberId; public string name; public MembershipStatus membershipStatus;
            public Member(int memberId, string name, MembershipStatus membershipStatus)
            {
                this.memberId = memberId; this.name = name; this.membershipStatus = membershipStatus;
            }
            public override string ToString() { return "Member ID: " + memberId + ", Name: " + name + ", Status: " + membershipStatus; }
        }

        class MembershipStatistics
        {
            public int totalMembers, totalPaidMembers; public double conversionRate;
            public MembershipStatistics(int totalMembers, int totalPaidMembers, double conversionRate)
            {
                this.totalMembers = totalMembers; this.totalPaidMembers = totalPaidMembers; this.conversionRate = conversionRate;
            }
        }

        class Membership
        {
            public List<Member> members = new List<Member>();
            public Dictionary<int, List<Workout>> workoutMap = new Dictionary<int, List<Workout>>();

            public void AddMember(Member member) { members.Add(member); }

            public void UpdateMembership(int memberId, MembershipStatus status)
            {
                foreach (Member m in members) if (m.memberId == memberId) { m.membershipStatus = status; break; }
            }

            public MembershipStatistics GetMembershipStatistics()
            {
                int totalMembers = members.Count;
                int totalPaidMembers = 0;

                foreach (Member m in members)
                {
                    // BUG 1: Only checking GOLD (SILVER check is missing)
                    if (m.membershipStatus == MembershipStatus.GOLD)
                    {
                        totalPaidMembers++;
                    }
                }

                // BUG 2: Integer division (e.g., 4 / 6 = 0)
                double conversionRate = totalMembers == 0 ? 0.0 : (totalPaidMembers / totalMembers) * 100.0;
                return new MembershipStatistics(totalMembers, totalPaidMembers, conversionRate);
            }

            // 2.1) Returns false, causing addWorkout tests to fail
            public bool AddWorkout(int memberId, Workout workout)
            {
                return false;
            }

            // 2.2) Returns empty map, causing lookup tests to fail
            public Dictionary<int, double?> GetAverageWorkoutDurations()
            {
                return new Dictionary<int, double?>();
            }

            // TASK 3: Returns empty map, causing payment tests to fail
            public Dictionary<int, int> GetDuePayments()
            {
                return new Dictionary<int, int>();
            }

            // TASK 4: Returns empty map, causing buddy tests to fail
            public Dictionary<int, List<int>> GetGymBuddies()
            {
                return new Dictionary<int, List<int>>();
            }
        }

        static int passed = 0, failed = 0;

        // Helper to force failure if values don't match
        static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("\n=== GYM MEMBERSHIP TEST SUITE ===\n");
            Run("TASK 0 — Member basics", TestMember);
            Run("TASK 1 — Membership stats", TestMembership);
            Run("TASK 2 — Average workout durations", TestGetAverageWorkoutDurations);
            Run("TASK 3 — Due payments", TestGetDuePayments);
            Run("TASK 4 — Gym buddies", TestGetGymBuddies);
            Console.WriteLine("\nFinal Results: " + passed + " passed, " + failed + " failed");
        }


        static void TestMember()
        {
            Member m = new Member(1, "John Doe", MembershipStatus.BRONZE);
            Check(m.memberId == 1, "ID mismatch");
            Check(m.name.Equals("John Doe"), "Name mismatch");
        }


        // <bug-task1>
        static void TestMembership()
        {
            Membership gym = new Membership();
            gym.AddMember(new Member(1, "John Doe", MembershipStatus.SILVER));
            gym.AddMember(new Member(2, "Marie C", MembershipStatus.GOLD));
            gym.AddMember(new Member(3, "Joe D", MembershipStatus.SILVER));
            gym.AddMember(new Member(4, "Westley D", MembershipStatus.SILVER));

            MembershipStatistics stats = gym.GetMembershipStatistics();
            // This will fail because SILVER is not counted in the logic above
            Check(stats.totalPaidMembers == 4, "Expected 4 paid members, but got " + stats.totalPaidMembers);
            Check(Math.Abs(stats.conversionRate - 100.0) < 0.1, "Conversion rate logic failed");
        }
        // </bug-task1>

        // <task2>
        static void TestGetAverageWorkoutDurations()
        {
            Membership gym = new Membership();
            gym.AddMember(new Member(12, "John Doe", MembershipStatus.SILVER));
            // This will fail because addWorkout returns false
            Check(gym.AddWorkout(12, new Workout(1, 10, 20)), "addWorkout returned false for valid member");
        }
        // </task2>

        // <task3>
        static void TestGetDuePayments()
        {
            Membership gym = new Membership();
            gym.AddMember(new Member(1, "Bronze Ben", MembershipStatus.BRONZE));

            Dictionary<int, int> payments = gym.GetDuePayments();
            // This will fail because the map is empty
            Check(payments.ContainsKey(1), "Member 1 missing from payment map");
        }
        // </task3>

        // <task4>
        static void TestGetGymBuddies()
        {
            Membership gym = new Membership();
            gym.AddMember(new Member(1, "Alice", MembershipStatus.BRONZE));

            Dictionary<int, List<int>> buddies = gym.GetGymBuddies();
            // This will fail because the map is empty
            Check(buddies.ContainsKey(1), "Member 1 missing from buddies map");
        }
        // </task4>

        static void Run(string name, Action test)
        {
            try
            {
                test();
                passed++;
                Console.WriteLine("PASS: " + name);
            }
            catch (Exception e)
            {
                failed++;
                Console.WriteLine("FAIL: " + name);
                Console.WriteLine("      Error: " + e.Message);
                Console.WriteLine("-------------------------------------------");
            }
        }
    }
}
