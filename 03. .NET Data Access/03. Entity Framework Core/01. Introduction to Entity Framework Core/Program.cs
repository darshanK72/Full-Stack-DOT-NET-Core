using System;
using IntroductionToEntityFrameworkCore.Utils;

namespace IntroductionToEntityFrameworkCore;

/*
 * =============================================================================
 * TOPIC: Introduction to Entity Framework Core - Microsoft's ORM for modern .NET.
 *        EF Core maps C# entity classes to relational tables, translates LINQ to
 *        SQL, tracks changes, and supports migrations and reverse engineering.
 *
 * WHY IT MATTERS:
 *   After ADO.NET and Dapper you can run SQL and map rows manually or with
 *   Query<T>. EF Core adds a full object model: relationships, change tracking,
 *   and schema evolution - so CRUD apps spend less time on plumbing and more on
 *   domain logic. Teams pick EF Core when the database is modeled as entities,
 *   not when every query is a hand-tuned report.
 *
 * WHAT YOU WILL LEARN:
 *   1.  ORM concepts - object-relational mapping and impedance mismatch
 *   2.  What EF Core is - DbContext, LINQ, change tracking, migrations overview
 *   3.  NuGet setup - Microsoft.EntityFrameworkCore + SqlServer provider
 *   4.  EF Core database providers - SqlServer, PostgreSQL, SQLite, In-Memory, ...
 *   5.  Code-First vs Database-First - two workflows to build the model
 *   6.  EF Core vs ADO.NET vs Dapper - when to pick each stack
 *   7.  Entity classes - POCOs mapped to tables
 *   8.  DbContext preview - gateway to the database (detail -> ch02)
 *   9.  In-Memory provider preview - runnable demo without SQL Server
 *
 * CHAPTER MAP:
 *   1.  ORM concepts                         -> Utils/EfCoreConcepts.cs
 *   2.  What EF Core is                      -> Utils/EfCoreConcepts.cs
 *   3.  NuGet setup                           -> Utils/EfCoreConcepts.cs
 *   4.  EF Core database providers            -> Utils/EfCoreConcepts.cs
 *   5.  Code-First vs Database-First          -> Utils/EfCoreConcepts.cs
 *   6.  EF Core vs ADO.NET vs Dapper          -> Utils/EfCoreConcepts.cs
 *   7.  Entity classes                        -> Models/Product.cs
 *   8.  DbContext preview                     -> Data/AppDbContext.cs
 *   9.  In-Memory provider preview            -> Utils/InMemoryProviderPreview.cs
 *   10. Demonstration                         -> Program.cs Main (below)
 *
 * Later chapters use SQL Server against (localdb)\MSSQLLocalDB (same AdoNetTutorial
 * schema as ADO.NET and Dapper tracks). This intro chapter runs entirely in-memory
 * so no database setup is required here.
 * =============================================================================
 */
public class Program
{
    /*
     * SECTION 10: DEMONSTRATION - Main orchestrates the chapter demo
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== 01. Introduction to Entity Framework Core ===");
        Console.WriteLine();

        EfCoreConcepts.ExplainOrmConcepts();
        Console.WriteLine();
        EfCoreConcepts.ExplainWhatEfCoreIs();
        Console.WriteLine();
        EfCoreConcepts.ExplainNuGetSetup();
        Console.WriteLine();
        EfCoreConcepts.ExplainProviders();
        Console.WriteLine();
        EfCoreConcepts.ExplainCodeFirstVsDatabaseFirst();
        Console.WriteLine();
        EfCoreConcepts.ExplainStackComparison();
        Console.WriteLine();

        InMemoryProviderPreview.RunDemo();
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE - INTRODUCTION TO ENTITY FRAMEWORK CORE
 * =========================================================================
 *
 * --- Setup ---
 *
 *   NuGet: Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.SqlServer
 *   Preview/testing: Microsoft.EntityFrameworkCore.InMemory
 *   using Microsoft.EntityFrameworkCore;
 *
 * --- Core idea ---
 *
 *   ORM = map C# entities <-> relational tables; EF Core generates SQL from LINQ
 *   and tracks object changes until SaveChanges().
 *
 * --- DbContext sketch (preview) ---
 *
 *   public sealed class AppDbContext : DbContext
 *   {
 *       public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
 *       public DbSet<Product> Products => Set<Product>();
 *   }
 *
 * --- Provider selection ---
 *
 *   optionsBuilder.UseSqlServer(connectionString);     // production SQL Server
 *   optionsBuilder.UseInMemoryDatabase("TestDb");    // tests / intro demos only
 *
 * --- Code-First vs Database-First ---
 *
 *   Code-First       - entities + migrations drive schema -> ch03
 *   Database-First   - scaffold from existing DB -> ch04
 *
 * --- When to use ---
 *
 *   EF Core    - entity model, LINQ, migrations, relationships, change tracking
 *   Dapper     - hand-tuned SQL, Query<T> mapping, minimal overhead
 *   ADO.NET    - full manual control, provider-specific APIs
 *
 * --- Forward references ---
 *
 *   ch02 DbContext & DbSet                  options, DI, DbSet members, configuration
 *   ch03 Code-First Models & Migrations     Add-Migration, Update-Database
 *   ch04 Database-First & Reverse Engineering dotnet ef dbcontext scaffold
 *   ch05 CRUD Operations & SaveChanges      Add/Update/Remove, entity state
 *   ch07 Fluent API & Data Annotations      [Required], OnModelCreating
 *   ch08 LINQ to Entities & Query Patterns  IQueryable, deferred execution
 *   ADO.NET track                           SqlConnection, SqlDataReader
 *   Dapper track                            Query<T>, Execute, parameters
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Treat In-Memory like SQL Server      | Tests pass but production SQL fails
 *  Skip DbContextOptions injection      | Hard to swap providers or test
 *  Expect Dapper-style raw SQL default  | Use LINQ or ch10 raw SQL APIs
 *  Missing Design package at ch03       | dotnet ef / Add-Migration tools fail
 *  No primary key on entity             | EF Core cannot identify rows reliably
 *
 * =========================================================================
 */
