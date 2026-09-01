/*
 * FILE ROLE: Defines IProductService and its in-memory ProductService implementation.
 *            Services are injected into endpoint handlers via implicit [FromServices]
 *            DI — no attribute needed when the parameter type is registered in the
 *            DI container.  Write operations return Task<T> to model real async I/O.
 * SECTIONS IN THIS FILE:
 *   6a. IProductService interface — contract used by endpoint handlers
 *   6b. ProductService implementation — in-memory store with async write operations
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApis.Models;

namespace MinimalApis.Services;

/*
 * SECTION 6a: IPRODUCTSERVICE INTERFACE
 *
 * Endpoints depend on this interface, not the concrete class.  The concrete
 * class is registered in Program.cs:
 *   builder.Services.AddSingleton<IProductService, ProductService>();
 *
 * The framework resolves IProductService from DI when it appears as a handler
 * parameter — NO [FromServices] attribute needed for this implicit resolution.
 * Implicit resolution kicks in when the parameter type is registered in DI
 * AND is not a primitive / string / IEnumerable<string> (those go to query).
 *
 * Read methods return synchronously (simulating a fast in-memory cache).
 * Write methods return Task<T> to simulate database async I/O.  In a real
 * app these would call await dbContext.SaveChangesAsync() or similar.
 */
public interface IProductService
{
    IEnumerable<Product> GetAll(string? category = null);   // sync — O(n) list scan
    Product? GetById(int id);                                // sync — returns null if missing
    Task<Product> CreateAsync(CreateProductRequest request);
    Task<Product?> UpdateAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteAsync(int id);                          // true = found and deleted
}

/*
 * SECTION 6b: IN-MEMORY PRODUCTSERVICE IMPLEMENTATION
 *
 * Registered as Singleton so the same list persists across requests.
 * Thread safety is NOT implemented here — acceptable for a tutorial; a real
 * concurrent store would use ConcurrentDictionary<int, Product> or a lock.
 *
 * Task.FromResult<T> wraps a synchronous value in a completed Task, letting
 * the handler use await without actual I/O.  This is the standard approach for
 * implementing async interfaces whose backing store is synchronous.
 *
 *   Real DB:           await _context.Products.FindAsync(id)
 *   In-memory tutorial: Task.FromResult(_products.FirstOrDefault(...))
 *
 * The pattern is identical from the handler's perspective — the handler always
 * writes `var p = await service.GetByIdAsync(id)` regardless of implementation.
 */
public sealed class ProductService : IProductService
{
    // Seed data — accessible from the first request
    private readonly List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Laptop Pro",  Price = 1299.99m, Category = "Electronics", StockQuantity = 50  },
        new Product { Id = 2, Name = "Mechanical Keyboard", Price = 79.99m, Category = "Electronics", StockQuantity = 200 },
        new Product { Id = 3, Name = "Ergonomic Chair", Price = 349.99m, Category = "Furniture", StockQuantity = 30  },
        new Product { Id = 4, Name = "Standing Desk", Price = 599.99m, Category = "Furniture", StockQuantity = 15  },
    };
    private int _nextId = 5; // auto-increment counter

    public IEnumerable<Product> GetAll(string? category = null)
    {
        // null category → return all; otherwise filter by exact match
        return category is null
            ? _products
            : _products.Where(p => p.Category == category);
    }

    public Product? GetById(int id) =>
        _products.FirstOrDefault(p => p.Id == id); // null if not found

    public Task<Product> CreateAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Id            = _nextId++,                // server-assigned
            Name          = request.Name,
            Price         = request.Price,
            Category      = request.Category,
            StockQuantity = request.StockQuantity,
        };
        _products.Add(product);
        return Task.FromResult(product); // wrap sync result in completed Task
    }

    public Task<Product?> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = GetById(id);
        if (product is null)
            return Task.FromResult<Product?>(null); // explicit generic arg for null literal

        product.Name          = request.Name;
        product.Price         = request.Price;
        product.Category      = request.Category;
        product.StockQuantity = request.StockQuantity;
        return Task.FromResult<Product?>(product);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var product = GetById(id);
        if (product is null)
            return Task.FromResult(false);

        _products.Remove(product);
        return Task.FromResult(true);
    }
}
