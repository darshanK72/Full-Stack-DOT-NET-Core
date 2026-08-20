// Food delivery platform https://www.onlinegdb.com/online_java_compiler#
/*
DESCRIPTION:
We are building a program to manage a food delivery platform. The platform
tracks orders as they move through various stages:
PLACED → PREPARING → OUT_FOR_DELIVERY → DELIVERED (or CANCELED).

-------------------------------------------------------------------------------
TASK: BUG FIX (Statistics)
The 'get_order_statistics()' method should return:
- Total: All orders in the system.
- Active: Orders with status PLACED, PREPARING, or OUT_FOR_DELIVERY.
- Closed: Orders with status DELIVERED or CANCELED.
Currently, the logic is incomplete and failing tests. Fix the bug.
-------------------------------------------------------------------------------
TASK 1: REVENUE PER RESTAURANT
Implement 'get_revenue_per_restaurant()'.
- This should return a dictionary: {restaurant_id: total_revenue}.
- ONLY 'DELIVERED' orders should count toward revenue.
- Canceled or active orders must be ignored.
-------------------------------------------------------------------------------
TASK 2.1: CUSTOMER DISTANCE ANALYTICS
Implement 'get_average_delivery_distance_per_customer()'.
- Return a dictionary: {customer_id: average_distance}.
- ONLY 'DELIVERED' orders should be included in the average.
- If a customer has no delivered orders, they should not be in the result.
-------------------------------------------------------------------------------
TASK 2.2: HIGHLY ACTIVE CUSTOMERS
Implement 'get_highly_active_customers()'.
- Return a sorted list of customer IDs who have placed 3 or MORE orders
  in total (any status, including CANCELED).

 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetQuestions
{
    enum OrderStatus
    {
        PLACED, PREPARING, OUT_FOR_DELIVERY, DELIVERED, CANCELED
    }

    class Order
    {
        public int orderId, restaurantId, customerId;
        public double orderValue, distanceKm;
        public OrderStatus status;

        public Order(int orderId, int restaurantId, int customerId,
                     double orderValue, double distanceKm, OrderStatus status)
        {
            this.orderId = orderId; this.restaurantId = restaurantId;
            this.customerId = customerId; this.orderValue = orderValue;
            this.distanceKm = distanceKm; this.status = status;
        }
    }

    class OrderManager
    {
        private List<Order> orders = new List<Order>();

        public void AddOrder(Order order) { orders.Add(order); }

        /**
         * BUG 1: Order Statistics
         * Logic Error: The 'closed' count is calculated incorrectly.
         * It only counts DELIVERED orders, completely ignoring CANCELED orders,
         * which are also considered closed.
         */
        public Dictionary<string, int> GetOrderStatistics()
        {
            int total = orders.Count;
            int active = orders
                .Where(o => o.status == OrderStatus.PLACED ||
                            o.status == OrderStatus.PREPARING ||
                            o.status == OrderStatus.OUT_FOR_DELIVERY)
                .Count();
            // BUG: Logic below only looks for DELIVERED, missing CANCELED
            int closed = orders
                .Where(o => o.status == OrderStatus.DELIVERED || o.status == OrderStatus.CANCELED)
                .Count();

            Dictionary<string, int> stats = new Dictionary<string, int>();
            stats["total_orders"] = total;
            stats["active_orders"] = active;
            stats["closed_orders"] = closed;
            return stats;
        }

        /**
         * TASK 1: Revenue Per Restaurant
         * Currently unimplemented - returns empty map.
         */
        public Dictionary<int, double> GetRevenuePerRestaurant()
        {
            return orders
                .Where(o => o.status == OrderStatus.DELIVERED)
                .GroupBy(o => o.restaurantId)
                .ToDictionary(g => g.Key,g => g.Sum(o => o.orderValue));
        }

        /**
         * TASK 2: Average Delivery Distance Per Customer
         * Currently unimplemented - returns empty map.
         */
        public Dictionary<int, double> GetAverageDeliveryDistancePerCustomer()
        {
            return orders
                    .Where(o => o.status == OrderStatus.DELIVERED)
                    .GroupBy(o => o.customerId)
                    .ToDictionary(g => g.Key,g => g.Average(o => o.distanceKm));
        }

        /**
         * TASK 3: Highly Active Customers (3+ orders)
         * Currently unimplemented - returns empty list.
         */
        public List<int> GetHighlyActiveCustomers()
        {
            return orders
                .GroupBy(o => o.customerId)
                .Where(g => g.Count() >= 3)
                .Select(g => g.Key)
                .OrderBy(key => key)
                .ToList();
        }
    }

    public class FoodDeliveryPlatformStub
    {
        static int passed = 0, failed = 0;

        public static void Main(string[] args)
        {
            Console.WriteLine("=== FOOD DELIVERY SYSTEM TEST SUITE ===\n");

            RunTest("BUG 1: Closed Orders Stats (Inc. Canceled)", () =>
            {
                OrderManager om = new OrderManager();
                om.AddOrder(new Order(1, 1, 1, 10.0, 1.0, OrderStatus.DELIVERED));
                om.AddOrder(new Order(2, 1, 1, 10.0, 1.0, OrderStatus.CANCELED));
                Dictionary<string, int> stats = om.GetOrderStatistics();
                // This will FAIL because Bug 1 will return 1 instead of 2
                Check(stats["closed_orders"] == 2, "Expected 2 closed orders, but got: " + stats["closed_orders"]);
            });

            RunTest("TASK 1: Restaurant Revenue", () =>
            {
                OrderManager om = new OrderManager();
                om.AddOrder(new Order(1, 10, 1, 50.0, 1.0, OrderStatus.DELIVERED));
                Dictionary<int, double> rev = om.GetRevenuePerRestaurant();
                Check(rev.ContainsKey(10), "Task 1: Restaurant 10 missing from revenue map");
            });

            RunTest("TASK 2: Avg Customer Distance", () =>
            {
                OrderManager om = new OrderManager();
                om.AddOrder(new Order(1, 1, 100, 10.0, 5.0, OrderStatus.DELIVERED));
                Dictionary<int, double> dist = om.GetAverageDeliveryDistancePerCustomer();
                Check(dist.ContainsKey(100), "Task 2.1: Customer 100 missing from distance map");
            });

            RunTest("TASK 3: Highly Active (3+ Orders)", () =>
            {
                OrderManager om = new OrderManager();
                om.AddOrder(new Order(1, 1, 500, 10.0, 1.0, OrderStatus.PLACED));
                om.AddOrder(new Order(2, 1, 500, 10.0, 1.0, OrderStatus.DELIVERED));
                om.AddOrder(new Order(3, 1, 500, 10.0, 1.0, OrderStatus.CANCELED));
                List<int> active = om.GetHighlyActiveCustomers();
                Check(active.Contains(500), "Task 2.2: Customer 500 should be highly active");
            });

            Console.WriteLine("\nResults: " + passed + " passed, " + failed + " failed");
        }

        static void Check(bool condition, string msg)
        {
            if (!condition) throw new Exception(msg);
        }

        static void RunTest(string name, Action test)
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
