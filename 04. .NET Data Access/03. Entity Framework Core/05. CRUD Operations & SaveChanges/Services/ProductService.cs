using System;
using System.Collections.Generic;
using System.Linq;
using CrudOperationsAndSaveChanges.Data;
using CrudOperationsAndSaveChanges.Models;
using CrudOperationsAndSaveChanges.Utils;
using Microsoft.EntityFrameworkCore;

namespace CrudOperationsAndSaveChanges.Services;

/*
 * FILE ROLE: ProductService wraps DbContext CRUD calls — Add, query, update,
 *            Remove, and SaveChanges — with entity-state visibility for learning.
 *
 * SECTIONS IN THIS FILE:
 *   3. CREATE — Add and SaveChanges
 *   4. READ — load tracked entity (LINQ preview)
 *   5. UPDATE — tracked changes and Update()
 *   6. DELETE — Remove and SaveChanges
 *   7. SaveChanges — unit of work
 */

public sealed class ProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /*
     * =========================================================================
     * SECTION 3: CREATE — Add AND SaveChanges
     * =========================================================================
     *
     * DbSet.Add(entity) registers a new instance with the change tracker:
     *   entry.State == EntityState.Added
     *
     * The row does NOT exist in SQL until SaveChanges runs an INSERT.
     *
     * Add vs AddAsync: same tracking behavior; async variant -> ch11.
     * AddRange / AddRangeAsync batch many inserts before one SaveChanges.
     *
     * After SaveChanges succeeds, ProductId (store-generated identity) is
     * populated on the entity and state becomes Unchanged.
     * -------------------------------------------------------------------------
     */
    public Product AddProduct(string name, decimal unitPrice, int stockQuantity)
    {
        var product = new Product
        {
            ProductName = name,
            UnitPrice = unitPrice,
            StockQuantity = stockQuantity
        };

        _context.Products.Add(product); // state -> Added (no SQL yet)
        return product;
    }

    /*
     * =========================================================================
     * SECTION 4: READ — LOAD TRACKED ENTITY (LINQ PREVIEW)
     * =========================================================================
     *
     * Queries return entities tracked as Unchanged by default (change tracking on).
     *
     *   Find(key)           — PK lookup; checks context first, then database
     *   FirstOrDefault(...) — LINQ filter; full IQueryable patterns -> ch08
     *
     * Modifying a tracked entity's properties marks it Modified automatically;
     * no Update() call needed when the instance came from the same context.
     * -------------------------------------------------------------------------
     */
    public Product? GetById(int productId)
    {
        return _context.Products.Find(productId); // tracked, Unchanged if found
    }

    public IReadOnlyList<Product> GetActiveProducts()
    {
        return _context.Products
            .Where(p => !p.IsDiscontinued)
            .OrderBy(p => p.ProductId)
            .ToList(); // materialize; deferred execution -> ch08
    }

    /*
     * =========================================================================
     * SECTION 5: UPDATE — TRACKED CHANGES AND Update()
     * =========================================================================
     *
     * --- 5a. Tracked update (preferred when entity is already in context) ---
     *
     *   Product p = context.Products.Find(id);
     *   p.UnitPrice = 12.99m;   // change tracker marks entity Modified
     *
     * Only changed columns appear in the UPDATE statement (by default).
     *
     * --- 5b. Detached update via DbSet.Update(entity) ---
     *
     * When you build an object outside the context (e.g. from an API DTO),
     * attach it and mark the whole entity Modified:
     *
     *   var stub = new Product { ProductId = id, ProductName = "...", ... };
     *   context.Products.Update(stub);
     *
     * Update() is equivalent to Attach + setting State = Modified. Use when
     * you cannot load the entity first. If another instance with the same key
     * is already tracked, EF throws InvalidOperationException — detach or use
     * ChangeTracker.Clear() before Update (attach patterns -> ch11).
     * -------------------------------------------------------------------------
     */
    public bool TryUpdateUnitPriceTracked(int productId, decimal newUnitPrice)
    {
        Product? product = _context.Products.Find(productId);
        if (product is null)
        {
            return false;
        }

        product.UnitPrice = newUnitPrice; // state -> Modified
        return true;
    }

    public void UpdateProductDetached(Product product)
    {
        _context.Products.Update(product); // attach + Modified for all mapped props
    }

    /*
     * =========================================================================
     * SECTION 6: DELETE — Remove AND SaveChanges
     * =========================================================================
     *
     * Remove(entity) marks a tracked instance Deleted. If the entity is Detached,
     * EF attaches it first then marks Deleted.
     *
     * RemoveRange(entities) deletes many rows before one SaveChanges.
     *
     * The DELETE executes on SaveChanges. Cascade delete for related entities
     * -> ch06 Relationships & Navigation Properties.
     * -------------------------------------------------------------------------
     */
    public bool TryRemoveById(int productId)
    {
        Product? product = _context.Products.Find(productId);
        if (product is null)
        {
            return false;
        }

        RemoveProduct(product);
        return true;
    }

    public void RemoveProduct(Product product)
    {
        _context.Products.Remove(product); // state -> Deleted
    }

    /*
     * =========================================================================
     * SECTION 7: SaveChanges — UNIT OF WORK
     * =========================================================================
     *
     * SaveChanges() (and SaveChangesAsync -> ch11):
     *   1. Detects all Added / Modified / Deleted entries
     *   2. Orders INSERT, UPDATE, DELETE commands
     *   3. Executes them in one transaction (by default)
     *   4. Returns the number of state entries written (not always row count)
     *   5. Resets tracked entities to Unchanged (except Deleted — detached)
     *
     * Batch pattern — multiple operations, one round trip:
     *
     *   context.Products.Add(newProduct);
     *   existing.UnitPrice = 9.99m;
     *   context.Products.Remove(oldProduct);
     *   int affected = context.SaveChanges(); // single transaction
     *
     * Call SaveChanges only when you are ready to persist. Long-lived contexts
     * with many pending changes can grow memory and surprise you at flush time.
     * -------------------------------------------------------------------------
     */
    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public string DescribeState<TEntity>(TEntity entity)
        where TEntity : class
    {
        return EntityStateInspector.Describe(_context, entity);
    }

    public string DescribeAllTrackedStates()
    {
        return EntityStateInspector.DescribeAllTracked(_context);
    }
}
