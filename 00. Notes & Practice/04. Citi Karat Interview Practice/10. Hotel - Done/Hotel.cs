// hotel booking system https://www.onlinegdb.com/online_java_compiler#

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{
    /*<bug-task1>
     * We are building a hotel room booking management system that tracks
     * guests, rooms, and their bookings.
     *
     * Classes:
     *   RoomType            — enum: DELUXE, STANDARD, SUITE
     *   BookingStatus       — enum: CONFIRMED, CHECKED_IN, CHECKED_OUT, CANCELLED
     *   Guest               — hotel guest (guestId, name)
     *   Room                — hotel room (roomId, roomType, pricePerNight)
     *   Booking             — reservation (bookingId, guestId, roomId, status, nights)
     *   GuestBookingSummary — summary result object
     *   HotelManager        — manages guests, rooms, and bookings

     * TASK 1: Read the code. The test is not passing due to a bug.
     *         Find and fix the bug in HotelManager.
     </bug-task1>
     */

    /*<task2>
     * TASK 2: Implement getAverageStayDurationByRoomType(int guestId):
     *   - Return Map<RoomType, Double>: room type → average nights for that
     *     guest's CHECKED_OUT bookings only.
     *   - Bookings with any other status are excluded.
     *   - Only room types with at least one CHECKED_OUT booking appear in the map.
     *   - Return an empty map if the guest has no CHECKED_OUT bookings or if
     *     the guestId is unknown.
     </task2>
     */

     // from guest id - all bookings for that guest
     // all bookings which are checkoud out
     // form bookings - group by room type
     // key - room type , value - Average(nights)

    /*<task3>
     * TASK 3: Implement getGuestBookingSummary():
     *   - Return Map<Integer, GuestBookingSummary> for every guest in the system.
     *   - totalBookings     : count of all bookings for that guest (all statuses).
     *   - totalNights       : sum of nights across all bookings (all statuses).
     *   - favoriteRoomType  : room type with the most bookings for that guest.
     *                         On a tie, pick the one that comes first alphabetically.
     *                         Set to null if the guest has no bookings.
     *   - Every guest must appear, including those with no bookings (0 / 0 / null).
     </task3>
     */

    /*<task4>
     * TASK 4: Implement getTopSpenders(int n):
     *   - Only CHECKED_OUT bookings count toward a guest's total spend.
     *   - A booking's spend = room.pricePerNight × booking.nights.
     *   - Return List<Integer>: top n guestIds sorted by total spend descending.
     *   - On a tie in spend, sort by guestId ascending (lower id first).
     *   - Guests with no CHECKED_OUT bookings have spend = 0 and are included.
     </task4>
     */

    enum RoomType {
        DELUXE, STANDARD, SUITE
    }

    enum BookingStatus {
        CONFIRMED, CHECKED_IN, CHECKED_OUT, CANCELLED
    }

    class GuestBookingSummary {
        public int       totalBookings;
        public int       totalNights;
        public RoomType? favoriteRoomType;

        public GuestBookingSummary(int totalBookings, int totalNights, RoomType? favoriteRoomType) {
            this.totalBookings    = totalBookings;
            this.totalNights      = totalNights;
            this.favoriteRoomType = favoriteRoomType;
        }
    }

    class Guest {
        public int    guestId;
        public string name;

        public Guest(int guestId, string name) {
            this.guestId = guestId;
            this.name    = name;
        }
    }

    class Room {
        public int      roomId;
        public RoomType roomType;
        public int      pricePerNight;

        public Room(int roomId, RoomType roomType, int pricePerNight) {
            this.roomId        = roomId;
            this.roomType      = roomType;
            this.pricePerNight = pricePerNight;
        }
    }

    class Booking {
        public int           bookingId;
        public int           guestId;
        public int           roomId;
        public BookingStatus status;
        public int           nights;

        public Booking(int bookingId, int guestId, int roomId, BookingStatus status, int nights) {
            this.bookingId = bookingId;
            this.guestId   = guestId;
            this.roomId    = roomId;
            this.status    = status;
            this.nights    = nights;
        }
    }

    class HotelManager {
        public Dictionary<int, Guest> guests   = new Dictionary<int, Guest>();
        public Dictionary<int, Room>  rooms    = new Dictionary<int, Room>();
        public List<Booking>          bookings = new List<Booking>();

        public void AddGuest(Guest guest) {
            guests[guest.guestId] = guest;
        }

        public void AddRoom(Room room) {
            rooms[room.roomId] = room;
        }

        public void AddBooking(Booking booking) {
            if (!guests.ContainsKey(booking.guestId)) return;
            if (!rooms.ContainsKey(booking.roomId))    return;
            bookings.Add(booking);
        }

        // TASK 1 BUG: Returns total revenue across all non-cancelled bookings.
        public int GetTotalRevenue() {
            int total = 0;
            foreach (Booking b in bookings) {
                if(b.status != BookingStatus.CANCELLED){
                    total += rooms[b.roomId].pricePerNight * b.nights;
                }
            }
            return total;
        }

        // TASK 2: Unimplemented - returns empty map.
        public Dictionary<RoomType, double> GetAverageStayDurationByRoomType(int guestId) {
            // TODO: implement
            // return new Dictionary<RoomType, double>();

            return bookings.Where(b => b.guestId ==guestId)
                .Where(b => b.status == BookingStatus.CHECKED_OUT)
                .Select(b => new 
                    {roomId = b.roomId,
                    nights = b.nights,
                    room = rooms[b.roomId]
                })
                .GroupBy(x => x.room.roomType)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(x => x.nights)
                );
        }

        // TASK 3: Unimplemented - returns empty map.
        public Dictionary<int, GuestBookingSummary> GetGuestBookingSummary() {
            // TODO: implement
            Dictionary<int,GuestBookingSummary> output = new Dictionary<int, GuestBookingSummary>();
            foreach(Guest guest in guests.Values){
                var guestBookings = bookings.Where(b => b.guestId == guest.guestId);
                if(guestBookings.Count() == 0){
                    output[guest.guestId] = new GuestBookingSummary(0,0,null);
                    continue;
                }
                RoomType? favRoomType = guestBookings
                    .GroupBy(b => rooms[b.roomId].roomType)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => g.Key)
                    .First()
                    .Key;

                var bookingSumary = new GuestBookingSummary(
                        guestBookings.Count(),
                        guestBookings.Sum(b => b.nights),
                        favRoomType
                    );
                output[guest.guestId] = bookingSumary;
            }
            return output;
        }

        // TASK 4: Unimplemented - returns empty list.
        public List<int> GetTopSpenders(int n) {
            
            List<KeyValuePair<int,int>> output = new List<KeyValuePair<int, int>>();
            foreach(Guest guest in guests.Values){
                var guestBookings = bookings.Where(b => b.guestId == guest.guestId);
                int total = 0;
                if(guestBookings.Count() != 0){
                    total = guestBookings
                        .Where(b => b.status == BookingStatus.CHECKED_OUT)
                        .Sum(b => rooms[b.roomId].pricePerNight * b.nights);
                }
                output.Add(new KeyValuePair<int, int>(guest.guestId,total));
            }

            return output
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .Take(n)
                .Select(kv => kv.Key)
                .ToList();
        }
    }

    public class HotelBooking {

        private static int passed = 0, failed = 0;

        public static void Main(string[] args) {
            Console.WriteLine("\n=== HOTEL BOOKING SYSTEM — KARAT PRACTICE ===\n");

            Run("TASK 1 — Total revenue (FIX BUG)",              TestGetTotalRevenue);
            Run("TASK 2 — Average stay duration by room type",   TestGetAverageStayDurationByRoomType);
            Run("TASK 3 — Guest booking summary",                TestGetGuestBookingSummary);
            Run("BONUS — Top spenders",                          TestGetTopSpenders);

            Console.WriteLine("\n--------------------------------------------------");
            Console.WriteLine("Results: " + passed + " passed, " + failed +
                    " failed out of " + (passed + failed));
        }

        // ================================================================
        //                            TESTS
        // ================================================================

        // <bug-task1>
        public static void TestGetTotalRevenue() {
            HotelManager hm = new HotelManager();
            hm.AddGuest(new Guest(1, "Alice"));
            hm.AddGuest(new Guest(2, "Bob"));
            hm.AddRoom(new Room(1, RoomType.STANDARD, 100));
            hm.AddRoom(new Room(2, RoomType.DELUXE,   200));

            hm.AddBooking(new Booking(1, 1, 1, BookingStatus.CONFIRMED,   3));  // $300
            hm.AddBooking(new Booking(2, 1, 2, BookingStatus.CHECKED_OUT, 2));  // $400
            hm.AddBooking(new Booking(3, 2, 1, BookingStatus.CANCELLED,   5));  // excluded
            hm.AddBooking(new Booking(4, 2, 2, BookingStatus.CHECKED_IN,  1));  // $200

            // 300 + 400 + 200 = 900  (cancelled booking must be excluded)
            Check(hm.GetTotalRevenue() == 900,
                    "Total revenue should be 900, was " + hm.GetTotalRevenue());
        }
        // </bug-task1>

        // <task2>
        public static void TestGetAverageStayDurationByRoomType() {
            HotelManager hm = new HotelManager();
            hm.AddGuest(new Guest(1, "Alice"));
            hm.AddGuest(new Guest(2, "Bob"));
            hm.AddGuest(new Guest(3, "Carol"));
            hm.AddRoom(new Room(1, RoomType.STANDARD, 100));
            hm.AddRoom(new Room(2, RoomType.DELUXE,   150));
            hm.AddRoom(new Room(3, RoomType.SUITE,    250));

            hm.AddBooking(new Booking(1, 1, 1, BookingStatus.CHECKED_OUT, 3));
            hm.AddBooking(new Booking(2, 1, 1, BookingStatus.CHECKED_OUT, 5));  // STANDARD avg = (3+5)/2 = 4.0
            hm.AddBooking(new Booking(3, 1, 2, BookingStatus.CHECKED_OUT, 2));  // DELUXE avg = 2.0
            hm.AddBooking(new Booking(4, 1, 3, BookingStatus.CONFIRMED,   7));  // not CHECKED_OUT → excluded
            hm.AddBooking(new Booking(5, 2, 2, BookingStatus.CHECKED_OUT, 6));  // DELUXE avg = 6.0

            Dictionary<RoomType, double> avg1 = hm.GetAverageStayDurationByRoomType(1);
            Check(Math.Abs(avg1[RoomType.STANDARD] - 4.0) < 0.0001,
                    "STANDARD avg should be 4.0, was " + avg1[RoomType.STANDARD]);
            Check(Math.Abs(avg1[RoomType.DELUXE] - 2.0) < 0.0001,
                    "DELUXE avg should be 2.0, was " + avg1[RoomType.DELUXE]);
            Check(!avg1.ContainsKey(RoomType.SUITE),
                    "SUITE should not appear — CONFIRMED booking is excluded");

            Dictionary<RoomType, double> avg2 = hm.GetAverageStayDurationByRoomType(2);
            Check(Math.Abs(avg2[RoomType.DELUXE] - 6.0) < 0.0001,
                    "DELUXE avg should be 6.0, was " + avg2[RoomType.DELUXE]);
            Check(!avg2.ContainsKey(RoomType.STANDARD),
                    "STANDARD should not appear in guest 2's result");

            // guest with no CHECKED_OUT bookings → empty map
            Check(hm.GetAverageStayDurationByRoomType(3).Count == 0,
                    "Guest 3 has no CHECKED_OUT bookings, should return empty map");

            // unknown guestId → empty map
            Check(hm.GetAverageStayDurationByRoomType(999).Count == 0,
                    "Unknown guest should return empty map");
        }
        // </task2>

        // <task3>
        public static void TestGetGuestBookingSummary() {
            HotelManager hm = new HotelManager();
            foreach (int gid in new int[] {1, 2, 3, 4}) {
                hm.AddGuest(new Guest(gid, "guest" + gid));
            }
            hm.AddRoom(new Room(1, RoomType.STANDARD, 100));
            hm.AddRoom(new Room(2, RoomType.DELUXE,   150));
            hm.AddRoom(new Room(3, RoomType.SUITE,    250));

            // guest 1: STANDARD x2, DELUXE x1, SUITE x1 → 4 bookings, 11 nights, STANDARD (most)
            hm.AddBooking(new Booking(1, 1, 1, BookingStatus.CHECKED_OUT, 3));
            hm.AddBooking(new Booking(2, 1, 1, BookingStatus.CONFIRMED,   5));
            hm.AddBooking(new Booking(3, 1, 2, BookingStatus.CHECKED_OUT, 2));
            hm.AddBooking(new Booking(4, 1, 3, BookingStatus.CANCELLED,   1));

            // guest 2: DELUXE x1, SUITE x1 — tied → DELUXE wins (D < S alphabetically)
            hm.AddBooking(new Booking(5, 2, 2, BookingStatus.CHECKED_OUT, 6));
            hm.AddBooking(new Booking(6, 2, 3, BookingStatus.CONFIRMED,   4));

            // guest 3: STANDARD x1
            hm.AddBooking(new Booking(7, 3, 1, BookingStatus.CHECKED_IN, 2));

            // guest 4: no bookings

            Dictionary<int, GuestBookingSummary> summary = hm.GetGuestBookingSummary();

            // guest 1
            Check(summary[1].totalBookings == 4,
                    "guest1 totalBookings should be 4, was " + summary[1].totalBookings);
            Check(summary[1].totalNights == 11,
                    "guest1 totalNights should be 11, was " + summary[1].totalNights);
            Check(summary[1].favoriteRoomType == RoomType.STANDARD,
                    "guest1 favorite should be STANDARD, was " + summary[1].favoriteRoomType);

            // guest 2 — tiebreak → DELUXE (D < S)
            Check(summary[2].totalBookings == 2,
                    "guest2 totalBookings should be 2, was " + summary[2].totalBookings);
            Check(summary[2].totalNights == 10,
                    "guest2 totalNights should be 10, was " + summary[2].totalNights);
            Check(summary[2].favoriteRoomType == RoomType.DELUXE,
                    "guest2 favorite should be DELUXE (tiebreak D < S), was " + summary[2].favoriteRoomType);

            // guest 3
            Check(summary[3].totalBookings == 1,
                    "guest3 totalBookings should be 1, was " + summary[3].totalBookings);
            Check(summary[3].totalNights == 2,
                    "guest3 totalNights should be 2, was " + summary[3].totalNights);
            Check(summary[3].favoriteRoomType == RoomType.STANDARD,
                    "guest3 favorite should be STANDARD, was " + summary[3].favoriteRoomType);

            // guest 4 — no bookings
            Check(summary[4].totalBookings == 0,
                    "guest4 totalBookings should be 0, was " + summary[4].totalBookings);
            Check(summary[4].totalNights == 0,
                    "guest4 totalNights should be 0, was " + summary[4].totalNights);
            Check(summary[4].favoriteRoomType == null,
                    "guest4 favoriteRoomType should be null, was " + summary[4].favoriteRoomType);
        }
        // </task3>

        // <task4>
        public static void TestGetTopSpenders() {
            HotelManager hm = new HotelManager();
            foreach (int gid in new int[] {1, 2, 3, 4}) {
                hm.AddGuest(new Guest(gid, "guest" + gid));
            }
            hm.AddRoom(new Room(1, RoomType.STANDARD, 100));
            hm.AddRoom(new Room(2, RoomType.DELUXE,   150));
            hm.AddRoom(new Room(3, RoomType.SUITE,    250));

            // guest 1 CHECKED_OUT: 3×$100=$300 + 2×$150=$300 → $600
            hm.AddBooking(new Booking(1, 1, 1, BookingStatus.CHECKED_OUT, 3));
            hm.AddBooking(new Booking(2, 1, 1, BookingStatus.CONFIRMED,   5));  // excluded
            hm.AddBooking(new Booking(3, 1, 2, BookingStatus.CHECKED_OUT, 2));
            hm.AddBooking(new Booking(4, 1, 3, BookingStatus.CANCELLED,   1));  // excluded

            // guest 2 CHECKED_OUT: 6×$150=$900 → $900
            hm.AddBooking(new Booking(5, 2, 2, BookingStatus.CHECKED_OUT, 6));
            hm.AddBooking(new Booking(6, 2, 3, BookingStatus.CONFIRMED,   4));  // excluded

            // guest 3: CHECKED_IN only → $0
            hm.AddBooking(new Booking(7, 3, 1, BookingStatus.CHECKED_IN, 2));

            // guest 4: no bookings → $0

            List<int> top2 = hm.GetTopSpenders(2);
            Check(top2.SequenceEqual(new List<int> { 2, 1 }),
                    "Top 2 should be [2, 1] ($900, $600), was " + string.Join(", ", top2));

            List<int> top3 = hm.GetTopSpenders(3);
            Check(top3.SequenceEqual(new List<int> { 2, 1, 3 }),
                    "Top 3 should be [2, 1, 3], was " + string.Join(", ", top3));

            List<int> top4 = hm.GetTopSpenders(4);
            Check(top4.SequenceEqual(new List<int> { 2, 1, 3, 4 }),
                    "Top 4 should be [2, 1, 3, 4], was " + string.Join(", ", top4));

            // tie-breaking: equal spend → lower guestId first
            HotelManager hm2 = new HotelManager();
            hm2.AddGuest(new Guest(10, "X"));
            hm2.AddGuest(new Guest(20, "Y"));
            hm2.AddRoom(new Room(1, RoomType.STANDARD, 100));
            hm2.AddBooking(new Booking(100, 10, 1, BookingStatus.CHECKED_OUT, 5));  // $500
            hm2.AddBooking(new Booking(101, 20, 1, BookingStatus.CHECKED_OUT, 5));  // $500

            List<int> tied = hm2.GetTopSpenders(2);
            Check(tied.SequenceEqual(new List<int> { 10, 20 }),
                    "Tied spenders should be ordered by guestId: [10, 20], was " + string.Join(", ", tied));
        }
        // </task4>

        // ================================================================
        //                           HELPERS
        // ================================================================

        private static void Check(bool condition, string msg) {
            if (!condition) throw new Exception(msg);
        }

        private static void Run(string name, Action test) {
            try {
                test();
                passed++;
                Console.WriteLine("  PASS: " + name);
            } catch (Exception e) {
                failed++;
                Console.WriteLine("  FAIL: " + name + " -> " + e.Message);
            }
        }
    }
}
