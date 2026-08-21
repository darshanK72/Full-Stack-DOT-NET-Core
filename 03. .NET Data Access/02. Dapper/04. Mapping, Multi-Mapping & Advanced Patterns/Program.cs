using System;
using Dapper;
using DapperMappingAndAdvancedPatterns.Data;
using DapperMappingAndAdvancedPatterns.Models;
using DapperMappingAndAdvancedPatterns.Repositories;
using DapperMappingAndAdvancedPatterns.Utils;
using DapperMappingAndAdvancedPatterns.Utils.TypeHandlers;
using Microsoft.Data.SqlClient;

namespace DapperMappingAndAdvancedPatterns;

/*
 * TOPIC: Mapping, Multi-Mapping & Advanced Patterns - how Dapper maps SQL rows to objects,
 *        splits JOIN results across types, and extends mapping with type handlers and Contrib.
 *
 * WHY IT MATTERS:
 *   Most production queries return JOINs (order + customer, order + lines) or columns whose
 *   names do not match C# properties. Dapper's default name matching is fast but naive;
 *   multi-map, splitOn, and optional TypeHandlers are how you model real relational shapes
 *   without abandoning micro-ORM performance for EF Core change tracking.
 *
 * PREREQUISITE:
 *   Dapper ch.01-03 (Query, parameters, QueryMultiple) and ADO.NET connection basics.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Default column-to-property mapping (case-insensitive)
 *   2.  SQL column aliases (AS) and [Column] attribute with CustomPropertyTypeMap
 *   3.  Multi-mapping - Query<TFirst, TSecond, TReturn> with map delegate
 *   4.  splitOn - where Dapper splits columns between mapped types
 *   5.  One-to-many - multi-map + Dictionary lookup idiom
 *   6.  Custom type handlers - SqlMapper.AddTypeHandler (DateOnly preview)
 *   7.  Dapper.Contrib - Insert / Get preview
 *   8.  Dapper.SqlBuilder - preview pointer only
 *
 * CHAPTER MAP:
 *   1.  Default mapping row type           -> Models/ProductRow.cs
 *   2.  Aliases and [Column] types         -> Models/ProductMappingVariants.cs
 *   2b. [Column] TypeMap registration      -> Utils/ColumnAttributeTypeMap.cs
 *   3-5 Multi-map, splitOn, one-to-many    -> Models/OrderModels.cs
 *                                       -> Repositories/MultiMapRepository.cs
 *   1-2 Mapping queries                    -> Repositories/MappingRepository.cs
 *   6.  DateOnly type handler              -> Utils/TypeHandlers/DateOnlyTypeHandler.cs
 *   7.  Dapper.Contrib preview             -> Models/ContribProduct.cs
 *                                       -> Repositories/ContribPreviewRepository.cs
 *   8.  SqlBuilder preview pointer          -> Utils/SqlBuilderPreview.cs
 *   Bootstrap                              -> Data/DatabaseBootstrap.cs
 *   9.  Demonstration                      -> Program.cs Main (below)
 *
 * Connection string:
 *   Server=(localdb)\MSSQLLocalDB;Database=AdoNetTutorial;Integrated Security=true;TrustServerCertificate=true
 */

public class Program
{
    /*
     * SECTION 9: DEMONSTRATION - Main orchestrates the chapter demo
     *
     * Registers global Dapper extensions (TypeHandler, TypeMap) once, bootstraps schema,
     * then runs mapping, multi-map, Contrib, and preview sections in reading order.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 04. Mapping, Multi-Mapping & Advanced Patterns (Dapper) ===");
        Console.WriteLine();

        RegisterDapperExtensions();

        Console.WriteLine($"--- SECTION 8: {SqlBuilderPreview.PackageName} (preview) ---");
        Console.WriteLine("  Dynamic WHERE/ORDER BY templates - see Utils/SqlBuilderPreview.cs.");
        Console.WriteLine("  Install NuGet Dapper.SqlBuilder when you need optional filter clauses.");
        Console.WriteLine();

        bool databaseReady = DatabaseBootstrap.TryEnsureSchema(ConnectionHelper.AdoNetTutorial);
        if (!databaseReady)
        {
            Console.WriteLine("Database demos skipped - start LocalDB and re-run.");
            Console.WriteLine("  DDL documented in Data/DatabaseBootstrap.cs and chapter intro above.");
            return;
        }

        Console.WriteLine("AdoNetTutorial ready on LocalDB.");
        Console.WriteLine();

        DemonstrateDefaultMapping();
        DemonstrateAliasesAndColumnAttribute();
        DemonstrateMultiMappingAndSplitOn();
        DemonstrateOneToManyLookup();
        DemonstrateDateOnlyTypeHandler();
        DemonstrateContribPreview();
    }

    private static void RegisterDapperExtensions()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler()); // SECTION 6 - before Query<DateOnly>
        ColumnAttributeTypeMap.RegisterOnce();               // SECTION 2b - before Query<ColumnMappedProduct>
    }

    private static void DemonstrateDefaultMapping()
    {
        Console.WriteLine("--- SECTION 1: Default column-to-property mapping ---");

        MappingRepository repository = new MappingRepository(ConnectionHelper.AdoNetTutorial);
        foreach (ProductRow product in repository.GetProductsDefaultMapping())
        {
            Console.WriteLine($"  {product.ProductId}: {product.ProductName} @ ${product.UnitPrice:F2} (stock {product.StockQuantity})");
        }

        Console.WriteLine();
    }

    private static void DemonstrateAliasesAndColumnAttribute()
    {
        Console.WriteLine("--- SECTION 2a: SQL column aliases (AS) ---");

        MappingRepository repository = new MappingRepository(ConnectionHelper.AdoNetTutorial);
        foreach (ProductWithAlias product in repository.GetProductsWithSqlAliases())
        {
            Console.WriteLine($"  Id={product.Id}, Title={product.Title}, Price=${product.Price:F2}");
        }

        Console.WriteLine();
        Console.WriteLine("--- SECTION 2b: [Column] attribute via CustomPropertyTypeMap ---");

        ColumnMappedProduct? mapped = repository.GetProductWithColumnAttribute(productId: 1);
        if (mapped is not null)
        {
            Console.WriteLine($"  ProductId={mapped.ProductId}, Name={mapped.ProductName}, Price={mapped.Price} (from UnitPrice column)");
        }

        Console.WriteLine();
    }

    private static void DemonstrateMultiMappingAndSplitOn()
    {
        Console.WriteLine("--- SECTIONS 3-4: Multi-mapping and splitOn ---");

        MultiMapRepository repository = new MultiMapRepository(ConnectionHelper.AdoNetTutorial);
        foreach (Order order in repository.GetOrdersWithCustomers())
        {
            Customer? customer = order.Customer;
            Console.WriteLine(
                $"  Order {order.OrderId} ({order.OrderDate:d}): ${order.TotalAmount:F2} - customer: {customer?.Name} <{customer?.Email}>");
        }

        Console.WriteLine("  splitOn: \"Name\" - first column belonging only to Customer.");
        Console.WriteLine();
    }

    private static void DemonstrateOneToManyLookup()
    {
        Console.WriteLine("--- SECTION 5: One-to-many with Dictionary lookup ---");

        MultiMapRepository repository = new MultiMapRepository(ConnectionHelper.AdoNetTutorial);
        Order? order = repository.GetOrderWithLines(orderId: 1);

        if (order is null)
        {
            Console.WriteLine("  Order 1 not found.");
            return;
        }

        Console.WriteLine($"  Order {order.OrderId}: {order.Lines.Count} line(s), total ${order.TotalAmount:F2}");
        foreach (OrderLine line in order.Lines)
        {
            Console.WriteLine($"    Line {line.OrderLineId}: ProductId={line.ProductId}, Qty={line.Quantity}, ${line.LineTotal:F2}");
        }

        Console.WriteLine();
    }

    private static void DemonstrateDateOnlyTypeHandler()
    {
        Console.WriteLine("--- SECTION 6: DateOnly TypeHandler (preview) ---");

        const string sql = "SELECT TOP 1 OrderDate FROM dbo.Orders ORDER BY OrderId;";
        using SqlConnection connection = new SqlConnection(ConnectionHelper.AdoNetTutorial);
        DateOnly sampleDate = connection.QuerySingle<DateOnly>(sql); // TypeHandler converts DATE -> DateOnly
        Console.WriteLine($"  First order date as DateOnly: {sampleDate:yyyy-MM-dd}");
        Console.WriteLine();
    }

    private static void DemonstrateContribPreview()
    {
        Console.WriteLine("--- SECTION 7: Dapper.Contrib Insert / Get (preview) ---");

        ContribPreviewRepository repository = new ContribPreviewRepository(ConnectionHelper.AdoNetTutorial);
        ContribProduct inserted = repository.InsertAndGetSampleProduct();
        Console.WriteLine($"  Insert + Get: ProductId={inserted.ProductId}, {inserted.ProductName}, stock={inserted.StockQuantity}");
        Console.WriteLine();
    }
}

/*
 * QUICK REFERENCE - MAPPING, MULTI-MAP & ADVANCED PATTERNS
 *
 * --- Default mapping ---
 *
 *   connection.Query<ProductRow>("SELECT ProductId, Name FROM Products");
 *   // case-insensitive column name -> property name
 *
 * --- SQL aliases (preferred rename) ---
 *
 *   SELECT ProductId AS Id, Name AS Title FROM Products
 *   connection.Query<Dto>(sql);
 *
 * --- [Column] attribute ---
 *
 *   SqlMapper.SetTypeMap(typeof(T), new CustomPropertyTypeMap(...));
 *   // or alias in SQL - simpler for Dapper
 *
 * --- Multi-map (one-to-one JOIN) ---
 *
 *   connection.Query<Order, Customer, Order>(
 *       joinSql,
 *       (order, customer) => { order.Customer = customer; return order; },
 *       splitOn: "Name");
 *
 * --- splitOn ---
 *
 *   First column name where TSecond begins (comma-separated for 3+ types).
 *   Avoid duplicate names - alias or pick a TSecond-only column.
 *
 * --- One-to-many (JOIN duplicates parent rows) ---
 *
 *   var lookup = new Dictionary<int, Order>();
 *   connection.Query<Order, OrderLine, Order>(sql, (o, line) => {
 *       if (!lookup.TryGetValue(o.OrderId, out var entry)) lookup.Add(o.OrderId, entry = o);
 *       entry.Lines.Add(line);
 *       return entry;
 *   }, splitOn: "OrderLineId");
 *   return lookup.Values;
 *
 * --- Type handler ---
 *
 *   SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
 *   // Parse(object) on read; SetValue(IDbDataParameter, T) on write
 *
 * --- Dapper.Contrib ---
 *
 *   [Table("Products")] [ExplicitKey] or [Key] on entity
 *   [Key] = identity PK; [ExplicitKey] = manual PK included in INSERT
 *   connection.Insert(entity);
 *   connection.Get<T>(id);
 *
 * --- Dapper.SqlBuilder (preview) ---
 *
 *   NuGet: Dapper.SqlBuilder - template tokens for dynamic WHERE/ORDER BY
 *   See Utils/SqlBuilderPreview.cs
 */
