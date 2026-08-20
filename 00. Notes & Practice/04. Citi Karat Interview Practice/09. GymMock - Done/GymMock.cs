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
using System.Linq;

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
                    if (m.membershipStatus == MembershipStatus.GOLD || m.membershipStatus == MembershipStatus.SILVER)
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
                Member? member = members.FirstOrDefault(m => m.memberId == memberId);
                if (member != null)
                {
                    if (!workoutMap.ContainsKey(memberId))
                    {
                        workoutMap[memberId] = new List<Workout>();
                    }
                    workoutMap[memberId].Add(workout);
                    return true;
                }
                return false;
            }

            // 2.2) Returns empty map, causing lookup tests to fail
            public Dictionary<int, double?> GetAverageWorkoutDurations()
            {
                Dictionary<int, double?> output = new Dictionary<int, double?>();
                foreach (Member mem in members)
                {
                    output[mem.memberId] = workoutMap[mem.memberId].Average(w => (double)w.GetDuration());
                }
                return output;
            }

            // TASK 3: Returns empty map, causing payment tests to fail
            public Dictionary<int, int> GetDuePayments()
            {
                Dictionary<int, int> output = new Dictionary<int, int>();

                foreach (Member mem in members)
                {
                    int freeHours = GetFreeHours(mem.membershipStatus);
                    int rate = GetRate(mem.membershipStatus);
                    int duePayment = 0;
                    if (workoutMap.ContainsKey(mem.memberId))
                    {
                        List<Workout> memberWorkouts = workoutMap[mem.memberId].ToList();
                        memberWorkouts.Sort((a, b) => a.GetId().CompareTo(b.GetId()));
                        for (int i = 0; i < memberWorkouts.Count; i++)
                        {
                            if (i >= freeHours)
                            {
                                int hour = (memberWorkouts[i].GetDuration() + 59) / 60;
                                duePayment += hour * rate;
                            }
                        }
                    }
                    output[mem.memberId] = duePayment;
                }
                return output;
            }

            public int GetFreeHours(MembershipStatus status)
            {
                switch (status)
                {
                    case MembershipStatus.BRONZE: return 1;
                    case MembershipStatus.SILVER: return 3;
                    case MembershipStatus.GOLD: return 5;
                    default: return 0;
                }
            }

            public int GetRate(MembershipStatus status)
            {
                switch (status)
                {
                    case MembershipStatus.BRONZE: return 10;
                    case MembershipStatus.SILVER: return 8;
                    case MembershipStatus.GOLD: return 6;
                    default: return 0;
                }
            }

            // TASK 4: Returns empty map, causing buddy tests to fail
            public Dictionary<int, List<int>> GetGymBuddies()
            {
                Dictionary<int, List<int>> output = new Dictionary<int, List<int>>();


                foreach (Member mem1 in members)
                {
                    List<KeyValuePair<int, int>> buddyDuration = new List<KeyValuePair<int, int>>();
                    foreach (Member mem2 in members)
                    {
                        if (mem1.memberId == mem2.memberId)
                            continue;
                        int shared = SharedDuration(mem1.memberId, mem2.memberId);
                        if (shared > 0)
                        {
                            buddyDuration.Add(new KeyValuePair<int, int>(mem2.memberId, shared));
                        }
                    }

                    buddyDuration.Sort((a, b) =>
                    {
                        int byDuration = b.Value.CompareTo(a.Value);
                        if (byDuration != 0) return byDuration;
                        return a.Key.CompareTo(b.Key);
                    });

                    output[mem1.memberId] = buddyDuration.Select(kv => kv.Key).ToList();
                }

                return output;
            }

            public int SharedDuration(int mem1Id, int mem2Id)
            {
                if (!workoutMap.ContainsKey(mem1Id) || !workoutMap.ContainsKey(mem2Id)) return 0;
                int result = 0;
                foreach (Workout w1 in workoutMap[mem1Id])
                {
                    foreach (Workout w2 in workoutMap[mem2Id])
                    {
                        int maxStartTime = Math.Max(w1.GetStartTime(), w2.GetStartTime());
                        int minEndTime = Math.Min(w1.GetEndTime(), w2.GetEndTime());
                        if (maxStartTime < minEndTime)
                        {
                            result += minEndTime - maxStartTime;
                        }
                    }
                }
                return result;
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
