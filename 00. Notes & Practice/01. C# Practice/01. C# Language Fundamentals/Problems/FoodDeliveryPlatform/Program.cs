/*
 * PROBLEM: Food Delivery Order Platform
 *
 * An operations team tracks food delivery orders through their full lifecycle.
 * Revenue and performance reports per restaurant help the business identify
 * top performers and spot operational issues.
 *
 * This is a manager-class style problem.  Main runs a scripted demo with known
 * data so you can verify each report method against expected output.
 *
 * This exercise covers:
 *   ch02 — enum for order lifecycle status; decimal for revenue; DateTime for order time
 *   ch04 — integer cast to double before computing an average; running-max comparison
 *   ch06 — loops over orders; per-restaurant accumulation
 *   ch07 — each report method returns a typed result with no Console calls
 *   ch09 — Dictionary for per-restaurant revenue and average prep time
 */

using System;
using System.Collections.Generic;

namespace FoodDelivery
{
    /*
     * Represents the lifecycle stage of a delivery order.
     * Revenue is counted only for orders that reach the DELIVERED state.
     */
    enum OrderStatus
    {
        PLACED, PREPARING, OUT_FOR_DELIVERY, DELIVERED, CANCELLED
    }

    /*
     * Represents one customer order placed through the platform.
     *
     * PrepMinutes records the estimated preparation time in minutes and is used
     * to compute per-restaurant performance averages for delivered orders.
     */
    class Order
    {
        public int         OrderId      { get; set; }
        public int         RestaurantId { get; set; }
        public decimal     Amount       { get; set; }
        public OrderStatus Status       { get; set; }
        public DateTime    PlacedAt     { get; set; }
        public int         PrepMinutes  { get; set; }
    }

    /*
     * Manages the platform's order log and produces operational reports.
     * No Console calls are permitted inside this class.
     */
    class OrderManager
    {
        private List<Order> _orders = new List<Order>();

        /*
         * Appends a new order to the log.
         * Orders are never deleted once added.
         */
        public void AddOrder(Order order)
        {
            _orders.Add(order);
        }

        /*
         * Returns a dictionary mapping each restaurant Id to the total revenue
         * earned from orders with status DELIVERED.
         *
         * Restaurants that have no delivered orders do not appear in the result.
         */
        public Dictionary<int, decimal> GetRevenueByRestaurant()
        {
            Dictionary<int,decimal> revenueByResturant = new Dictionary<int,decimal>();

            foreach(Order order in _orders){
                if(order.Status == OrderStatus.DELIVERED){
                    if(!revenueByResturant.ContainsKey(order.OrderId)){
                        revenueByResturant[order.OrderId] = 0;
                    }
                    revenueByResturant[order.OrderId] += order.Amount;
                }
            }
            return revenueByResturant;
        }

        /*
         * Returns a dictionary mapping each restaurant Id to its average PrepMinutes
         * across delivered orders only.
         *
         * Only restaurants with at least one delivered order appear in the result.
         */
        public Dictionary<int, double> GetAveragePrepMinutes()
        {
            Dictionary<int,double> 
        }

        /*
         * Returns the Id of the restaurant with the highest total delivered revenue.
         *
         * When two restaurants have identical revenue, the one with the lower Id is returned.
         * Returns -1 when no delivered orders exist.
         */
        public int GetTopRestaurantId()
        {
            // TODO: implement
            throw new NotImplementedException();
        }
    }

    /*
     * Entry point — scripted demo.
     *
     * Seeds the three orders shown in the sample data table and prints
     * each report alongside the expected output.
     */
    class Program
    {
        static void Main(string[] args)
        {
            OrderManager manager = new OrderManager();

            /*
             * Seed data (from PROBLEM.md sample table):
             *   Order 1 — Restaurant 10, Amount 50, DELIVERED,  PrepMinutes 20
             *   Order 2 — Restaurant 10, Amount 30, CANCELLED,  PrepMinutes 15
             *   Order 3 — Restaurant 20, Amount 40, DELIVERED,  PrepMinutes 25
             *
             * Order 2 is CANCELLED so Restaurant 10's delivered revenue is 50, not 80.
             */

            /*
             * Expected output when all methods are implemented:
             *
             *   Revenue by restaurant:
             *     Restaurant 10 : 50.00
             *     Restaurant 20 : 40.00
             *
             *   Average prep time (delivered orders only):
             *     Restaurant 10 : 20.0 min
             *     Restaurant 20 : 25.0 min
             *
             *   Top restaurant by revenue: 10
             */

            // TODO: call AddOrder with the three seed orders, then print each report
            throw new NotImplementedException();
        }
    }
}
