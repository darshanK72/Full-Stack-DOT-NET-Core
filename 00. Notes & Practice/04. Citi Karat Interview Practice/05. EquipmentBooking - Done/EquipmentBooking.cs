/*
 * <bug-task1>
 * We are building a program to manage equipment bookings at a sports
 * facility. The facility has multiple pieces of equipment, such as
 * treadmills and rowing machines, and members can reserve them for
 * specific time periods.
 *
 * Definitions:
 * - An "equipment" is an object representing a reservable item. It has
 *   properties for the ID and name.
 * - A "reservation" is an object representing a member booking for a
 *   specific piece of equipment over a time period. Times are represented
 *   as integers in minutes from the start of the day.
 * - A "FacilityManager" is the class used to manage all equipment and
 *   reservations.
 *
 * To begin with, we present you with two tasks:
 * 1-1) Read through and understand the code below. Please take as much
 *      time as necessary, and feel free to run the code.
 * 1-2) One of the tests is failing due to a bug in the code. Make the
 *      necessary changes to FacilityManager to fix the bug.
 * </bug-task1>
 */


/*
 * <task2>
 * We are updating our system to support richer reservation queries.
 * Facility managers need to look up all active reservations made by a
 * specific member, and equipment coordinators need a quick summary of
 * how each piece of equipment is being used.
 *
 * 2-1) Add getReservationsForMember() to FacilityManager.
 *      This function takes a member's name and returns a list of all
 *      their active reservations. If the member has no active
 *      reservations, return an empty list.
 *
 * 2-2) Add getEquipmentSummary() to FacilityManager.
 *      This function takes an equipment ID and returns a map with
 *      the following keys:
 *      - "total_reservations": the number of active reservations for
 *        that equipment.
 *      - "total_minutes": the total duration in minutes across all
 *        active reservations for that equipment.
 *      If the equipment has no active reservations, both values should
 *      be 0.
 *
 * To assist you in testing these new functions, we have provided the
 * testGetReservationsForMember and testGetEquipmentSummary tests.
 * </task2>
 */


/*
 * <task3>
 * The facility requires a minimum turnaround time of 30 minutes between
 * reservations for the same piece of equipment, to allow for cleaning
 * and sanitisation between members.
 *
 * A piece of equipment is unavailable if the requested period overlaps
 * an active reservation, or if there is less than 30 minutes between
 * the requested period and any active reservation before or after it.
 *
 * 3) Add getAvailableEquipment() to FacilityManager.
 *    This function takes a requested start and end time and returns a
 *    sorted list of equipment IDs (ascending) that are fully available
 *    for the requested period, including the required 30-minute turnaround
 *    on either side.
 *    If no equipment is available, return an empty list.
 *
 * To assist you in testing this new function, we have provided the
 * testGetAvailableEquipment test.
 * </task3>
 */


/*
 * <task4>
 * TASK 4: Implement getMostBookedEquipment(n) in FacilityManager.
 *   - Only ACTIVE reservations count toward an equipment's total booked minutes.
 *   - A reservation's duration = endTime - startTime.
 *   - Returns a list of the top n equipment IDs sorted by total booked minutes
 *     descending.
 *   - On a tie in total minutes, sort by equipment ID ascending (lower ID first).
 *   - Equipment with no active reservations is excluded from the result.
 *   - If n exceeds the number of qualifying equipment, return all qualifying equipment.
 *
 * To assist you in testing this new function, we have provided the
 * testGetMostBookedEquipment test.
 * </task4>
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{
    enum ReservationStatus
    {
        ACTIVE,
        CANCELLED
    }

    class Equipment
    {
        public readonly int EquipmentId;
        public readonly string Name;

        public Equipment(int equipmentId, string name)
        {
            EquipmentId = equipmentId;
            Name = name;
        }

        public override string ToString()
        {
            return "Equipment ID: " + EquipmentId + ", Name: " + Name;
        }
    }

    class Reservation
    {
        public readonly int ReservationId;
        public readonly string MemberName;
        public readonly int EquipmentId;
        public readonly int StartTime;
        public readonly int EndTime;
        public ReservationStatus Status;

        public Reservation(int reservationId, string memberName,
                    int equipmentId, int startTime, int endTime)
        {
            ReservationId = reservationId;
            MemberName = memberName;
            EquipmentId = equipmentId;
            StartTime = startTime;
            EndTime = endTime;
            Status = ReservationStatus.ACTIVE;
        }

        public int GetDuration()
        {
            return EndTime - StartTime;
        }

        public override bool Equals(object other)
        {
            if (!(other is Reservation r)) return false;
            return ReservationId == r.ReservationId;
        }

        public override int GetHashCode()
        {
            return ReservationId.GetHashCode();
        }

        public override string ToString()
        {
            return "Reservation ID: " + ReservationId
                    + ", Member: " + MemberName
                    + ", Equipment ID: " + EquipmentId
                    + ", Start: " + StartTime
                    + ", End: " + EndTime
                    + ", Status: " + Status;
        }
    }

    class FacilityManager
    {
        public readonly List<Equipment> EquipmentList = new List<Equipment>();
        public readonly List<Reservation> Reservations = new List<Reservation>();

        /** Adds a piece of equipment to the facility inventory. */
        public void AddEquipment(Equipment equipment)
        {
            EquipmentList.Add(equipment);
        }

        /**
         * <bug 1>
         * Makes a reservation if the equipment is available for the requested period.
         * Returns true if successfully reserved, false if the equipment is unavailable.
         */
        public bool MakeReservation(Reservation reservation)
        {
            if (!IsAvailable(reservation.EquipmentId,
                             reservation.StartTime,
                             reservation.EndTime))
            {
                return false;
            }
            Reservations.Add(reservation);
            return true;
        }

        public bool IsAvailable(int equipmentId, int startTime, int endTime)
        {
            foreach (Reservation res in Reservations)
            {
                if (res.EquipmentId == equipmentId)
                {
                    if (startTime < res.EndTime && endTime > res.StartTime)
                    {
                        if (res.Status == ReservationStatus.CANCELLED)
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        public bool CancelReservation(int reservationId)
        {
            foreach (Reservation res in Reservations)
            {
                if (res.ReservationId == reservationId)
                {
                    res.Status = ReservationStatus.CANCELLED;
                    return true;
                }
            }
            return false;
        }

        public List<Reservation> GetReservationsForEquipment(int equipmentId)
        {
            List<Reservation> result = new List<Reservation>();
            foreach (Reservation res in Reservations)
            {
                if (res.EquipmentId == equipmentId
                        && res.Status == ReservationStatus.ACTIVE)
                {
                    result.Add(res);
                }
            }
            return result;
        }

        public List<Reservation> GetReservationsForMember(string memberName)
        {
            return Reservations
                .Where(res => res.MemberName == memberName && res.Status == ReservationStatus.ACTIVE)
                .ToList();
        }

        public Dictionary<string, int> GetEquipmentSummary(int equipmentId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            var equpReservations = Reservations
                .Where(res => res.EquipmentId == equipmentId && res.Status == ReservationStatus.ACTIVE)
                .ToList();
            result["total_reservations"] = equpReservations.Count;
            result["total_minutes"] = equpReservations.Sum(res => res.GetDuration());
            return result;
        }

        public List<int> GetAvailableEquipment(int startTime, int endTime)
        {
            List<int> output = new List<int>();
            foreach (Equipment eqp in EquipmentList)
            {
                var reservations = Reservations
                    .Where(res => res.EquipmentId == eqp.EquipmentId)
                    .ToList();
                if (reservations.Count == 0)
                {
                    output.Add(eqp.EquipmentId);
                    continue;
                }

                foreach (Reservation res in reservations)
                {
                    if (startTime < (res.EndTime + 30) && (endTime + 29) > res.StartTime)
                    {
                        if (res.Status == ReservationStatus.CANCELLED)
                        {
                            output.Add(eqp.EquipmentId);
                        }
                        
                    }else{
                        output.Add(eqp.EquipmentId);
                    }

                }

            }
            return output;
        }

        public List<int> GetMostBookedEquipment(int n)
        {
            return Reservations
                    .Where(res => res.Status == ReservationStatus.ACTIVE)
                    .GroupBy(res => res.EquipmentId)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => g.Key)
                    .Take(n)
                    .Select(g => g.Key)
                    .ToList();
        }
    }

    public class EquipmentBookingStub
    {
        private static int passed = 0, failed = 0;

        public static void Main(string[] args)
        {
            Console.WriteLine("\n=== EQUIPMENT BOOKING ===\n");

            Run("TASK 1   — Make reservation / conflict detection", TestMakeReservation);
            Run("TASK 1   — Different equipment, no conflict", TestDifferentEquipmentNoConflict);
            Run("TASK 1   — Cancelled reservation frees slot", TestCancelledReservationFreesSlot);
            Run("TASK 2-1 — Get reservations for member", TestGetReservationsForMember);
            Run("TASK 2-2 — Get equipment summary", TestGetEquipmentSummary);
            Run("TASK 3   — Get available equipment", TestGetAvailableEquipment);
            Run("TASK 4   — Get most booked equipment", TestGetMostBookedEquipment);

            Console.WriteLine("\n--------------------------------------------------");
            Console.WriteLine("Results: " + passed + " passed, " + failed +
                               " failed out of " + (passed + failed));
        }

        // ================================================================
        //                           TESTS
        // ================================================================

        // <bug-task1>
        public static void TestMakeReservation()
        {
            FacilityManager manager = new FacilityManager();
            Reservation res1 = new Reservation(101, "Alice Johnson", 1, 480, 600);
            Reservation res2 = new Reservation(102, "Bob Smith", 1, 540, 660);

            Check(manager.MakeReservation(res1),
                "makeReservation should return true for a valid reservation");
            Check(manager.Reservations.Count == 1,
                "reservations size should be 1, was " + manager.Reservations.Count);
            Check(!manager.MakeReservation(res2),
                "makeReservation should return false for a conflicting reservation");
        }

        public static void TestDifferentEquipmentNoConflict()
        {
            FacilityManager manager = new FacilityManager();
            Reservation res1 = new Reservation(101, "Alice Johnson", 1, 480, 600);
            Reservation res2 = new Reservation(102, "Bob Smith", 2, 480, 600);

            Check(manager.MakeReservation(res1),
                "makeReservation should return true for equipment 1");
            Check(manager.MakeReservation(res2),
                "makeReservation should return true for equipment 2 (different equipment)");
        }

        public static void TestCancelledReservationFreesSlot()
        {
            FacilityManager manager = new FacilityManager();
            Reservation res1 = new Reservation(101, "Alice Johnson", 1, 480, 600);
            manager.MakeReservation(res1);
            manager.CancelReservation(101);

            // After cancellation, the same slot should be bookable again
            Reservation res2 = new Reservation(102, "Bob Smith", 1, 480, 600);
            Check(manager.MakeReservation(res2),
                "makeReservation should return true after the conflicting reservation is cancelled");
        }
        // </bug-task1>

        // <task2>
        public static void TestGetReservationsForMember()
        {
            FacilityManager manager = new FacilityManager();
            Reservation res1 = new Reservation(101, "Alice Johnson", 1, 480, 600);
            Reservation res2 = new Reservation(102, "Bob Smith", 1, 660, 720);
            Reservation res3 = new Reservation(103, "Alice Johnson", 2, 720, 840);
            Reservation res4 = new Reservation(104, "Carol White", 2, 480, 540);
            manager.MakeReservation(res1);
            manager.MakeReservation(res2);
            manager.MakeReservation(res3);
            manager.MakeReservation(res4);

            List<Reservation> aliceRes = manager.GetReservationsForMember("Alice Johnson");
            Check(aliceRes.Count == 2,
                "Alice should have 2 reservations, was " + aliceRes.Count);
            Check(aliceRes.Contains(res1),
                "Alice's reservations should contain res1");
            Check(aliceRes.Contains(res3),
                "Alice's reservations should contain res3");

            // new string(...) ensures comparison uses Equals, not reference equality
            List<Reservation> aliceResByNewString =
                    manager.GetReservationsForMember(new string("Alice Johnson".ToCharArray()));
            Check(aliceResByNewString.Count == 2,
                "Alice (new string) should have 2 reservations, was " + aliceResByNewString.Count);
            Check(aliceResByNewString.Contains(res1),
                "Alice's (new string) reservations should contain res1");
            Check(aliceResByNewString.Contains(res3),
                "Alice's (new string) reservations should contain res3");

            List<Reservation> bobRes = manager.GetReservationsForMember("Bob Smith");
            Check(bobRes.Count == 1,
                "Bob should have 1 reservation, was " + bobRes.Count);
            Check(bobRes.Contains(res2),
                "Bob's reservations should contain res2");

            // Member with no reservations
            Check(manager.GetReservationsForMember("David Brown").Count == 0,
                "David Brown should have 0 reservations");

            // Cancelled reservations should not appear
            manager.CancelReservation(101);
            List<Reservation> aliceResAfter = manager.GetReservationsForMember("Alice Johnson");
            Check(aliceResAfter.Count == 1,
                "Alice should have 1 reservation after cancellation, was " + aliceResAfter.Count);
            Check(aliceResAfter.Contains(res3),
                "Alice's remaining reservation should be res3");
        }

        public static void TestGetEquipmentSummary()
        {
            FacilityManager manager = new FacilityManager();
            Reservation res1 = new Reservation(101, "Alice Johnson", 1, 480, 540);
            Reservation res2 = new Reservation(102, "Bob Smith", 1, 660, 720);
            Reservation res3 = new Reservation(103, "Carol White", 1, 720, 780);
            Reservation res4 = new Reservation(104, "David Brown", 2, 480, 600);
            manager.MakeReservation(res1);
            manager.MakeReservation(res2);
            manager.MakeReservation(res3);
            manager.MakeReservation(res4);

            Dictionary<string, int> summaryE1 = manager.GetEquipmentSummary(1);
            Check(summaryE1["total_reservations"] == 3,
                "equipment 1 total_reservations should be 3, was " + summaryE1["total_reservations"]);
            Check(summaryE1["total_minutes"] == 180,
                "equipment 1 total_minutes should be 180, was " + summaryE1["total_minutes"]);

            Dictionary<string, int> summaryE2 = manager.GetEquipmentSummary(2);
            Check(summaryE2["total_reservations"] == 1,
                "equipment 2 total_reservations should be 1, was " + summaryE2["total_reservations"]);
            Check(summaryE2["total_minutes"] == 120,
                "equipment 2 total_minutes should be 120, was " + summaryE2["total_minutes"]);

            // Equipment with no reservations
            Dictionary<string, int> summaryE3 = manager.GetEquipmentSummary(3);
            Check(summaryE3["total_reservations"] == 0,
                "equipment 3 total_reservations should be 0, was " + summaryE3["total_reservations"]);
            Check(summaryE3["total_minutes"] == 0,
                "equipment 3 total_minutes should be 0, was " + summaryE3["total_minutes"]);

            // Cancelled reservations should not be counted
            manager.CancelReservation(101);
            Dictionary<string, int> summaryE1After = manager.GetEquipmentSummary(1);
            Check(summaryE1After["total_reservations"] == 2,
                "equipment 1 total_reservations after cancel should be 2, was " + summaryE1After["total_reservations"]);
            Check(summaryE1After["total_minutes"] == 120,
                "equipment 1 total_minutes after cancel should be 120, was " + summaryE1After["total_minutes"]);
        }
        // </task2>

        // <task3>
        public static void TestGetAvailableEquipment()
        {
            FacilityManager manager = new FacilityManager();
            manager.AddEquipment(new Equipment(1, "Treadmill"));
            manager.AddEquipment(new Equipment(2, "Rowing Machine"));
            manager.AddEquipment(new Equipment(3, "Exercise Bike"));
            manager.AddEquipment(new Equipment(4, "Elliptical Trainer"));

            // equip1: reserved 480-600
            // equip2: reserved 300-420
            // equip3: reserved 700-780
            // equip4: no reservations
            manager.MakeReservation(new Reservation(101, "Alice Johnson", 1, 480, 600));
            manager.MakeReservation(new Reservation(102, "Bob Smith", 2, 300, 420));
            manager.MakeReservation(new Reservation(103, "Carol White", 3, 700, 780));

            // Request: 630-720
            // equip1: res ends 600, gap = 30 min exactly → available
            // equip2: res ends 420, gap = 210 min → available
            // equip3: request overlaps 700-780 from 700-720 → not available
            // equip4: no reservations → available
            Check(manager.GetAvailableEquipment(630, 720).SequenceEqual(new List<int> { 1, 2, 4 }),
                "630-720: expected [1, 2, 4], was [" + string.Join(", ", manager.GetAvailableEquipment(630, 720)) + "]");

            // Request: 620-720
            // equip1: res ends 600, gap = 20 min → NOT available (under 30-min turnaround)
            // equip2: res ends 420, gap = 200 min → available
            // equip3: res starts 700, within 30 min of request end 720 → not available
            // equip4: no reservations → available
            Check(manager.GetAvailableEquipment(620, 720).SequenceEqual(new List<int> { 2, 4 }),
                "620-720: expected [2, 4], was [" + string.Join(", ", manager.GetAvailableEquipment(620, 720)) + "]");

            // Cancelled reservations should not block availability
            manager.CancelReservation(101);
            Check(manager.GetAvailableEquipment(620, 720).Contains(1),
                "equipment 1 should be available after its reservation is cancelled");

            // No equipment available
            FacilityManager manager2 = new FacilityManager();
            manager2.AddEquipment(new Equipment(5, "Weight Bench"));
            manager2.MakeReservation(new Reservation(201, "Alice Johnson", 5, 480, 600));
            Check(manager2.GetAvailableEquipment(610, 700).Count == 0,
                "610-700: expected empty list, was [" + string.Join(", ", manager2.GetAvailableEquipment(610, 700)) + "]");

            // Turnaround applies before an existing reservation too
            // Existing res: 600-720. Request 500-580: gap = 20 min → not available
            // Request 500-570: gap = exactly 30 min → available
            FacilityManager manager3 = new FacilityManager();
            manager3.AddEquipment(new Equipment(6, "Spin Bike"));
            manager3.MakeReservation(new Reservation(301, "Nina Patel", 6, 600, 720));
            Check(manager3.GetAvailableEquipment(500, 580).Count == 0,
                "500-580: expected empty list (20-min gap), was [" + string.Join(", ", manager3.GetAvailableEquipment(500, 580)) + "]");
            Check(manager3.GetAvailableEquipment(500, 570).SequenceEqual(new List<int> { 6 }),
                "500-570: expected [6] (30-min gap exactly), was [" + string.Join(", ", manager3.GetAvailableEquipment(500, 570)) + "]");
        }
        // </task3>

        // <task4>
        public static void TestGetMostBookedEquipment()
        {
            FacilityManager manager = new FacilityManager();
            // equip 1: 60 + 60 = 120 active minutes
            // equip 2: 90 active minutes
            // equip 3: cancelled → 0 active minutes → excluded
            manager.MakeReservation(new Reservation(101, "Alice", 1, 480, 540));  // 60 min
            manager.MakeReservation(new Reservation(102, "Bob", 1, 600, 660));  // 60 min
            manager.MakeReservation(new Reservation(103, "Carol", 2, 480, 570));  // 90 min
            manager.MakeReservation(new Reservation(104, "Dave", 3, 480, 600));
            manager.CancelReservation(104);

            Check(manager.GetMostBookedEquipment(2).SequenceEqual(new List<int> { 1, 2 }),
                "top 2 should be [1, 2], was [" + string.Join(", ", manager.GetMostBookedEquipment(2)) + "]");
            Check(manager.GetMostBookedEquipment(5).SequenceEqual(new List<int> { 1, 2 }),
                "top 5 should be [1, 2] (only 2 qualifying), was [" + string.Join(", ", manager.GetMostBookedEquipment(5)) + "]");

            // tie-breaking: equal total minutes → lower equipment ID first
            FacilityManager manager2 = new FacilityManager();
            manager2.MakeReservation(new Reservation(201, "Alice", 10, 480, 540)); // equip 10: 60 min
            manager2.MakeReservation(new Reservation(202, "Bob", 20, 480, 540)); // equip 20: 60 min
            Check(manager2.GetMostBookedEquipment(2).SequenceEqual(new List<int> { 10, 20 }),
                "tie-break: expected [10, 20], was [" + string.Join(", ", manager2.GetMostBookedEquipment(2)) + "]");
        }
        // </task4>

        // ================================================================
        //                          HELPERS
        // ================================================================

        private static void Check(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
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
