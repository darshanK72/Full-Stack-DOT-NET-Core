using AjaxPartialUpdates.Models;
using System.Collections.Generic;
using System.Linq;

namespace AjaxPartialUpdates.Services;

/*
 * FILE ROLE: ProductService provides an in-memory product store used by every
 *            controller action in this chapter. It is registered as a Singleton
 *            so the list persists across requests for the lifetime of the app.
 *
 * SECTIONS IN THIS FILE:
 *   4. In-Memory ProductService — simple data layer for AJAX demos
 */

/*
 * SECTION 4: IN-MEMORY ProductService — DATA LAYER FOR AJAX DEMOS
 * ─────────────────────────────────────────────────────────────────────────────
 * A simple in-memory list simulates a database for the AJAX demos. The service
 * is registered as a Singleton in Program.cs so the same list instance is
 * shared across all HTTP requests — additions via AJAX persist during the
 * session without a real database.
 *
 * SINGLETON + INSTANCE FIELDS:
 *   _products and _nextId are instance fields. Because ProductService is
 *   registered as Singleton (one instance for the app lifetime), they behave
 *   like shared mutable state across all requests.
 *
 *   NOTE — Thread safety: for a real application, concurrent requests calling
 *   Add() simultaneously would race on _nextId and List<T>.Add(). In production,
 *   use a database with row-level locking or a thread-safe collection. Here a
 *   lock statement would be sufficient, but is omitted for readability.
 *
 * WHY A SERVICE CLASS (not inline in the controller):
 *   Extracting data access into a service class keeps the controller thin and
 *   testable. The controller only orchestrates the HTTP response — it does not
 *   hold business logic or data. This follows the Single Responsibility Principle
 *   and makes unit-testing the controller trivial (swap the service with a mock).
 *
 * REGISTRATION (in Program.cs SECTION 2):
 *   builder.Services.AddSingleton<ProductService>();
 *
 * INJECTION (in ProductsController constructor):
 *   public ProductsController(ProductService service) { _service = service; }
 */
public class ProductService
{
    // In-memory seed data — populated once when the Singleton is created
    private readonly List<ProductViewModel> _products = new List<ProductViewModel>
    {
        new ProductViewModel { Id = 1, Name = "Wireless Headphones",  Category = "Electronics", Price = 79.99m,  Description = "Over-ear noise-cancelling headphones", Stock = 15 },
        new ProductViewModel { Id = 2, Name = "Mechanical Keyboard",  Category = "Electronics", Price = 129.99m, Description = "TKL layout with tactile switches",     Stock = 8  },
        new ProductViewModel { Id = 3, Name = "Standing Desk Mat",    Category = "Office",       Price = 34.99m,  Description = "Anti-fatigue foam mat",                Stock = 22 },
        new ProductViewModel { Id = 4, Name = "Monitor Stand",        Category = "Office",       Price = 49.99m,  Description = "Adjustable aluminum stand",            Stock = 0  },
        new ProductViewModel { Id = 5, Name = "USB-C Hub",            Category = "Electronics", Price = 39.99m,  Description = "7-in-1 USB-C docking station",         Stock = 12 },
    };

    private int _nextId = 6; // auto-increment counter for new products

    // Returns a snapshot copy so callers cannot mutate the internal list
    public IEnumerable<ProductViewModel> GetAll() => _products.ToList();

    // Returns null when no product matches — callers must handle the null case
    public ProductViewModel? GetById(int id) =>
        _products.FirstOrDefault(p => p.Id == id);

    // Case-insensitive category filter; null or empty returns all products
    public IEnumerable<ProductViewModel> GetByCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category)) return GetAll();
        return _products
            .Where(p => p.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // Text search across Name, Category, and Description fields
    public IEnumerable<ProductViewModel> Search(string? term)
    {
        if (string.IsNullOrWhiteSpace(term)) return GetAll();
        return _products
            .Where(p => p.Name.Contains(term, System.StringComparison.OrdinalIgnoreCase)
                     || p.Category.Contains(term, System.StringComparison.OrdinalIgnoreCase)
                     || p.Description.Contains(term, System.StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // Mutates the in-memory list; assigns the server-side Id before returning
    public ProductViewModel Add(ProductViewModel vm)
    {
        vm.Id = _nextId++;   // assign next available Id
        _products.Add(vm);   // persist to the in-memory list
        return vm;           // return with assigned Id so the controller can include it in the response
    }
}
