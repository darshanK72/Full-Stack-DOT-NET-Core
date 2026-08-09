/*
 * PROBLEM: Mini Inventory Management System
 *
 * A regional warehouse uses this terminal tool so floor staff can manage
 * product stock during a single shift without needing a spreadsheet.
 * Staff add new SKUs, check current stock levels, record sales, receive
 * new stock, and quickly locate products by a partial name search.
 *
 * The system is entirely in-memory — all data resets when the application exits.
 *
 * This exercise covers:
 *   ch02 — decimal for monetary values, int for stock counts and Ids
 *   ch03 — reading user input with Console.ReadLine; displaying formatted output
 *   ch05 — converting raw string input into decimal and int safely with TryParse
 *   ch06 — a loop that keeps the menu running until the user exits; switch for dispatch
 *   ch07 — business logic isolated in InventoryService; I/O belongs only in Program
 *   ch08 — currency format specifier, case-insensitive substring match
 *   ch09 — List<Product> as the in-memory backing store
 */

using System;
using System.Collections.Generic;

namespace RetailInventory
{
    /*
     * Represents one stock-keeping unit (SKU) in the warehouse catalogue.
     *
     * Id is always assigned by InventoryService — the caller never sets it directly.
     * Price is decimal because it represents a monetary value (never double for money).
     */
    class Product
    {
        public int     Id    { get; set; }
        public string  Name  { get; set; }
        public decimal Price { get; set; }
        public int     Stock { get; set; }
    }

    /*
     * Holds all business rules for the warehouse catalogue.
     *
     * This class must never call Console.Read or Console.Write.
     * All screen output responsibility belongs exclusively to Program.
     */
    class InventoryService
    {
        private List<Product> _products = new List<Product>();
        private int _nextId = 1;

        /*
         * Adds a new product to the catalogue and returns the Id that was assigned to it.
         *
         * The service is responsible for assigning a unique, incrementing Id.
         * The caller provides the display name, the unit price, and the opening stock count.
         */
        public int AddProduct(string name, decimal price, int stock)
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Returns every product currently in the catalogue.
         * When no products have been added yet, returns an empty list.
         */
        public List<Product> GetAllProducts()
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Records a sale of the specified quantity for the product with the given Id.
         *
         * The sale is rejected — and a descriptive errorMessage is set — when any of
         * the following conditions are true:
         *   the product Id does not exist in the catalogue,
         *   the requested quantity is zero or negative,
         *   the product's current stock is less than the requested quantity.
         *
         * When the sale succeeds, stock is reduced by quantity and errorMessage
         * is set to an empty string.  The method returns true on success.
         */
        public bool Sell(int id, int quantity, out string errorMessage)
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Increases the stock of the product identified by Id by the given quantity.
         *
         * Rejected — with a descriptive errorMessage — when the Id does not exist
         * or the quantity is zero or negative.
         * Returns true on success; errorMessage is empty on success.
         */
        public bool Restock(int id, int quantity, out string errorMessage)
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Returns all products whose Name contains the given substring,
         * ignoring differences in letter case (e.g. "widget" matches "Widget" or "WIDGET").
         * Returns an empty list when nothing matches.
         */
        public List<Product> Search(string partial)
        {
            // TODO: implement
            throw new NotImplementedException();
        }
    }

    /*
     * Entry point for the application.
     *
     * Program owns all Console I/O — every prompt, every display line.
     * Each menu option is handled by a dedicated private static method so that
     * Main stays readable and each concern is clearly separated.
     */
    class Program
    {
        static InventoryService _service = new InventoryService();

        /*
         * Displays the main menu and processes choices in a loop until the user exits.
         *
         * Valid options: Add, List, Sell, Restock, Search, Exit.
         * Any unrecognised input should print a friendly message and show the menu again.
         */
        static void Main(string[] args)
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Prompts the user for a product name, a price, and an opening stock count.
         * Each field must be validated before forwarding to the service.
         * On success, confirms to the user which Id was assigned to the new product.
         */
        static void HandleAdd()
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Retrieves all products from the service and displays them as a table.
         * Each row shows the Id, Name, Price formatted as currency, and current Stock.
         * Prints a short message when the catalogue is empty.
         */
        static void HandleList()
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Prompts for a product Id and a sale quantity, then calls the service.
         * On success, shows the stock level that remains after the sale.
         * On failure, displays the error description returned by the service.
         */
        static void HandleSell()
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Prompts for a product Id and a restock quantity, then calls the service.
         * On success, shows the updated stock level.
         * On failure, displays the error description.
         */
        static void HandleRestock()
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        /*
         * Prompts for a partial name, retrieves matching products, and lists them.
         * Prints "No matches." when the search returns an empty result.
         */
        static void HandleSearch()
        {
            // TODO: implement
            throw new NotImplementedException();
        }
    }
}
