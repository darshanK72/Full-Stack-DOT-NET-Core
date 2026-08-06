
/*<bug-task1>
 * We are building a program to manage a food delivery platform. The platform has multiple restaurants,
 * customers place orders, and those orders move through statuses:
 * PLACED → PREPARING → OUT_FOR_DELIVERY → DELIVERED, or CANCELED.
 *
 * Definitions:
 * * An "order" has: orderId, restaurantId, customerId, orderValue, distanceKm, status.
 * * "OrderManager" manages orders and provides order statistics.
 *
 * 1-1) Read through and understand the code below. Feel free to run it.
 * 1-2) The test for OrderManager is not passing due to a bug in the code.
 *      Make the necessary changes to OrderManager to fix the bug.
 *</bug-task1>
 */
/*<task2>
 * 2.1) addDelivery(orderId, delivery): associate a delivery with an order. If the order does not exist, ignore.
 * 2.2) getAverageDeliveryTimeByRestaurant(): average delivery duration (minutes) per restaurantId.
 *      Count ALL deliveries for that restaurant (across orders). Return Map<Integer, Double>.
 *</task2>
 */
/*<task3>
 * 3) getTopRestaurantsByRevenue(int n): return the restaurantIds of the n restaurants with the highest
 *    total revenue, ordered from most to least revenue.
 *    - Revenue counts ONLY orders whose status is DELIVERED.
 *    - A restaurant with zero delivered revenue must NOT appear in the result.
 *    - Break ties by restaurantId in ascending order.
 *    - If n is greater than the number of qualifying restaurants, return all of them.
 *    - If n <= 0, return an empty list.
 *</task3>
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{
    enum OrderStatus
    {
        PLACED,
        PREPARING,
        OUT_FOR_DELIVERY,
        DELIVERED,
        CANCELED
    }

    class Delivery
    {
        public int deliveryId;
        public int startMinute;
        public int endMinute;

        public Delivery(int deliveryId, int startMinute, int endMinute)
        {
            this.deliveryId = deliveryId;
            this.startMinute = startMinute;
            this.endMinute = endMinute;
        }

        public int GetDurationMinutes()
        {
            return endMinute - startMinute;
        }
    }

    class Order
    {
        public int orderId;
        public int restaurantId;
        public int customerId;
        public double orderValue;
        public double distanceKm;
        public List<Delivery> deliverys = new List<Delivery>();
        public OrderStatus status;

        public Order(int orderId, int restaurantId, int customerId, double orderValue, double distanceKm, OrderStatus status)
        {
            this.orderId = orderId;
            this.restaurantId = restaurantId;
            this.customerId = customerId;
            this.orderValue = orderValue;
            this.distanceKm = distanceKm;
            this.status = status;
        }

        public List<Delivery> GetDelivery()
        {
            return this.deliverys;
        }
    }

    class OrderStats
    {
        public int totalOrders;
        public int activeOrders;
        public int closedOrders;

        public OrderStats(int totalOrders, int activeOrders, int closedOrders)
        {
            this.totalOrders = totalOrders;
            this.activeOrders = activeOrders;
            this.closedOrders = closedOrders;
        }
    }

    class OrderManager
    {
        public List<Order> orders;

        public OrderManager()
        {
            this.orders = new List<Order>();
        }

        public void AddOrder(Order order)
        {
            orders.Add(order);
        }

        public void UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            foreach (Order o in orders)
            {
                if (o.orderId == orderId)
                {
                    o.status = newStatus;
                    return;
                }
            }
        }

        // TASK 1: This method has a bug. Find and fix it.
        public OrderStats GetOrderStatistics()
        {
            int total = orders.Count;
            int active = 0;
            foreach (Order o in orders)
            {
                if (o.status == OrderStatus.PLACED || o.status == OrderStatus.OUT_FOR_DELIVERY || o.status == OrderStatus.PREPARING)
                {
                    active++;
                }
            }
            int closed = 0;
            foreach (Order o in orders)
            {
                if (o.status == OrderStatus.DELIVERED)
                {
                    closed++;
                }
            }
            return new OrderStats(total, active, closed);
        }

        public void AddDelivery(int orderId, Delivery delivery)
        {
            // TODO: implement — if order doesn't exist, ignore
        }

        public Dictionary<int, double> GetAverageDeliveryTimeByRestaurant()
        {
            // TODO: implement
            return new Dictionary<int, double>();
        }

        // TASK 3: implement
        public List<int> GetTopRestaurantsByRevenue(int n)
        {
            // TODO: implement
            return new List<int>();
        }
    }

    public class Main
    {
        private static int passed = 0, failed = 0;

        public static void Main(string[] args)
        {
            Console.WriteLine("\n=== FOOD DELIVERY V2 — TEST SUITE ===\n");
            Run("TASK 1 — Order statistics (FIX BUG)", TestOrderManager);
            Run("TASK 2.1 — addDelivery (unknown order ignored)", TestAddDeliveryIgnoresUnknown);
            Run("TASK 2.2 — Average delivery time by restaurant", TestGetAverageDeliveryTimeByRestaurant);
            Run("TASK 2.3 — Single restaurant, multiple orders", TestSingleRestaurantMultipleOrders);
            Run("TASK 2.4 — Empty manager returns empty map", TestEmptyManagerReturnsEmpty);
            Run("TASK 3 — Top restaurants by revenue", TestGetTopRestaurantsByRevenue);
            Console.WriteLine("\n--------------------------------------------------");
            Console.WriteLine("Results: " + passed + " passed, " + failed +
                    " failed out of " + (passed + failed));
        }

        // ── assertion helpers (no JUnit needed) ────────────────────────────

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }

        private static void AssertEquals(object expected, object actual)
        {
            if (!object.Equals(expected, actual))
            {
                throw new Exception("expected <" + expected + "> but was <" + actual + ">");
            }
        }

        private static void AssertEqualsList<T>(List<T> expected, List<T> actual)
        {
            if (!expected.SequenceEqual(actual))
            {
                throw new Exception("expected <" + string.Join(", ", expected) + "> but was <" + string.Join(", ", actual) + ">");
            }
        }

        private static void AssertAlmost(double expected, double actual, double eps)
        {
            if (Math.Abs(expected - actual) > eps)
            {
                throw new Exception("expected ~" + expected + " but was " + actual);
            }
        }

        // ── TASK 1 ─────────────────────────────────────────────────────────
        //<bug-task1>
        public static void TestOrderManager()
        {
            OrderManager om = new OrderManager();
            om.AddOrder(new Order(1, 10, 100, 25.0, 3.2, OrderStatus.PLACED));
            om.AddOrder(new Order(2, 10, 101, 55.0, 1.4, OrderStatus.PREPARING));
            om.AddOrder(new Order(3, 11, 102, 15.0, 6.0, OrderStatus.OUT_FOR_DELIVERY));
            om.AddOrder(new Order(4, 11, 103, 40.0, 2.0, OrderStatus.DELIVERED));
            om.AddOrder(new Order(5, 12, 104, 18.0, 4.5, OrderStatus.CANCELED));

            OrderStats stats = om.GetOrderStatistics();
            AssertEquals(5, stats.totalOrders);
            AssertEquals(3, stats.activeOrders);
            AssertEquals(2, stats.closedOrders);
        }
        //</bug-task1>

        // ── TASK 2.1 ───────────────────────────────────────────────────────
        //<task2>
        public static void TestAddDeliveryIgnoresUnknown()
        {
            OrderManager om = new OrderManager();
            om.AddOrder(new Order(1, 10, 100, 25.0, 3.2, OrderStatus.DELIVERED));
            // delivery for non-existent order 999 must be silently ignored
            om.AddDelivery(999, new Delivery(1, 0, 30));
            Dictionary<int, double> avg = om.GetAverageDeliveryTimeByRestaurant();
            // restaurant 10 has no deliveries — should not appear
            AssertTrue(!avg.ContainsKey(10), "restaurant 10 has no deliveries and must not appear");
        }

        // ── TASK 2.2 ───────────────────────────────────────────────────────

        public static void TestGetAverageDeliveryTimeByRestaurant()
        {
            OrderManager om = new OrderManager();
            om.AddOrder(new Order(1, 10, 100, 25.0, 3.2, OrderStatus.DELIVERED));
            om.AddOrder(new Order(2, 10, 101, 55.0, 1.4, OrderStatus.DELIVERED));
            om.AddOrder(new Order(3, 11, 102, 15.0, 6.0, OrderStatus.DELIVERED));

            om.AddDelivery(1, new Delivery(101, 10, 40));   // 30 min
            om.AddDelivery(2, new Delivery(102, 50, 80));   // 30 min
            om.AddDelivery(2, new Delivery(103, 90, 150));  // 60 min
            om.AddDelivery(3, new Delivery(104, 20, 50));   // 30 min

            om.AddDelivery(999, new Delivery(105, 0, 10));  // ignored

            Dictionary<int, double> avg = om.GetAverageDeliveryTimeByRestaurant();

            // restaurant 10: [30, 30, 60] → avg 40
            AssertAlmost(40.0, avg[10], 0.0001);
            // restaurant 11: [30] → avg 30
            AssertAlmost(30.0, avg[11], 0.0001);
            // no leakage from unknown order
            AssertTrue(!avg.ContainsKey(999), "unknown order must not leak into results");
        }

        // ── TASK 2.3 ───────────────────────────────────────────────────────

        public static void TestSingleRestaurantMultipleOrders()
        {
            OrderManager om = new OrderManager();
            om.AddOrder(new Order(1, 20, 200, 10.0, 1.0, OrderStatus.DELIVERED));
            om.AddOrder(new Order(2, 20, 201, 10.0, 1.0, OrderStatus.DELIVERED));
            om.AddDelivery(1, new Delivery(1, 0, 20));   // 20 min
            om.AddDelivery(2, new Delivery(2, 0, 40));   // 40 min

            Dictionary<int, double> avg = om.GetAverageDeliveryTimeByRestaurant();
            AssertAlmost(30.0, avg[20], 0.0001);
        }

        // ── TASK 2.4 ───────────────────────────────────────────────────────

        public static void TestEmptyManagerReturnsEmpty()
        {
            OrderManager om = new OrderManager();
            Dictionary<int, double> avg = om.GetAverageDeliveryTimeByRestaurant();
            AssertTrue(avg.Count == 0, "empty manager should return empty map");
        }
        //</task2>

        // ── TASK 3 ─────────────────────────────────────────────────────────
        //<task3>
        public static void TestGetTopRestaurantsByRevenue()
        {
            OrderManager om = new OrderManager();
            // restaurant 10: 25 + 55 = 80 delivered revenue
            om.AddOrder(new Order(1, 10, 100, 25.0, 3.2, OrderStatus.DELIVERED));
            om.AddOrder(new Order(2, 10, 101, 55.0, 1.4, OrderStatus.DELIVERED));
            // restaurant 11: 40 delivered (CANCELED 100 does NOT count)
            om.AddOrder(new Order(3, 11, 102, 40.0, 6.0, OrderStatus.DELIVERED));
            om.AddOrder(new Order(4, 11, 103, 100.0, 2.0, OrderStatus.CANCELED));
            // restaurant 12: 80 delivered (ties with restaurant 10 → id ascending wins)
            om.AddOrder(new Order(5, 12, 104, 80.0, 4.5, OrderStatus.DELIVERED));
            // restaurant 13: only a PLACED order → zero delivered revenue → must NOT appear
            om.AddOrder(new Order(6, 13, 105, 200.0, 1.0, OrderStatus.PLACED));

            // Ranking: 10(80) & 12(80) tie → 10 first, then 12; then 11(40). 13 excluded.
            AssertEqualsList(new List<int> { 10, 12, 11 }, om.GetTopRestaurantsByRevenue(5));
            AssertEqualsList(new List<int> { 10, 12 }, om.GetTopRestaurantsByRevenue(2));
            AssertEqualsList(new List<int> { 10 }, om.GetTopRestaurantsByRevenue(1));

            // restaurant 13 has no delivered revenue and must never appear
            AssertTrue(!om.GetTopRestaurantsByRevenue(10).Contains(13),
                    "restaurant 13 has no delivered revenue and must not appear");

            // Edge cases: n <= 0 → empty list.
            AssertTrue(om.GetTopRestaurantsByRevenue(0).Count == 0, "n=0 should be empty");
            AssertTrue(om.GetTopRestaurantsByRevenue(-3).Count == 0, "n<0 should be empty");

            // Empty manager → empty list.
            AssertTrue(new OrderManager().GetTopRestaurantsByRevenue(3).Count == 0,
                    "empty manager should return empty list");
        }
        //</task3>

        // ── runner ─────────────────────────────────────────────────────────

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
