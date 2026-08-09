using System;

namespace IntroductionToEntityFrameworkCore.Utils;

/*
 * FILE ROLE: Conceptual introduction - ORM ideas, EF Core packages, database
 *            providers, Code-First vs Database-First, and stack comparison.
 *
 * SECTIONS IN THIS FILE:
 *   1. ORM concepts - object-relational mapping and impedance mismatch
 *   2. What EF Core is - Microsoft's ORM for modern .NET
 *   3. NuGet setup - core + SQL Server provider packages
 *   4. EF Core database providers
 *   5. Code-First vs Database-First overview
 *   6. EF Core vs ADO.NET vs Dapper
 */

public static class EfCoreConcepts
{
    /*
     * =========================================================================
     * SECTION 1: ORM CONCEPTS - OBJECT-RELATIONAL MAPPING
     * =========================================================================
     *
     * Relational databases store rows and columns; .NET apps work with objects
     * and properties. An ORM (Object-Relational Mapper) bridges that gap:
     *
     *   C# object graph  <->  SQL tables / rows / foreign keys
     *
     * "Impedance mismatch" names the friction between the two models:
     *
     *   | Relational              | Object-oriented
     *   |-------------------------|------------------------------------------
     *   | Normalized tables       | Nested object graphs
     *   | JOINs for relationships | Navigation properties / collections
     *   | NULL in columns         | Nullable reference/value types
     *   | Stored procedures       | Methods on services or repositories
     *
     * ORMs translate LINQ or tracked object changes into SQL, map result rows
     * back to entities, and manage identity (which in-memory object is which row).
     *
     * Trade-off: convenience and consistency vs full control over every byte
     * of SQL. EF Core sits on the convenience side; ADO.NET on control.
     * -------------------------------------------------------------------------
     */
    public static void ExplainOrmConcepts()
    {
        Console.WriteLine("--- SECTION 1: ORM concepts ---");
        Console.WriteLine("  ORM maps C# classes <-> relational tables and rows.");
        Console.WriteLine("  Impedance mismatch: normalized SQL vs object graphs, JOINs vs navigation.");
        Console.WriteLine("  EF Core generates SQL, maps results, and tracks changes for SaveChanges.");
    }

    /*
     * =========================================================================
     * SECTION 2: WHAT EF CORE IS - MICROSOFT'S ORM FOR MODERN .NET
     * =========================================================================
     *
     * Entity Framework Core is the cross-platform ORM for .NET (Core / 5+).
     * It replaces EF6 (Windows / .NET Framework only) with a lighter,
     * provider-based architecture.
     *
     * Core responsibilities:
     *
     *   - Model mapping (entities, keys, relationships, indexes)
     *   - Query translation (LINQ -> SQL via provider)
     *   - Change tracking (detect inserts/updates/deletes)
     *   - Migrations (Code-First schema evolution) -> ch03
     *   - Reverse engineering (Database-First scaffold) -> ch04
     *
     * You do not call SqlConnection or write SqlDataReader loops for routine
     * CRUD - EF Core's provider does that under DbContext.
     * -------------------------------------------------------------------------
     */
    public static void ExplainWhatEfCoreIs()
    {
        Console.WriteLine("--- SECTION 2: What EF Core is ---");
        Console.WriteLine("  Cross-platform ORM for .NET - model, LINQ queries, change tracking, migrations.");
        Console.WriteLine("  DbContext is the main entry point (preview in Data/AppDbContext.cs).");
    }

    /*
     * =========================================================================
     * SECTION 3: NUGET SETUP - CORE + SQL SERVER PROVIDER
     * =========================================================================
     *
     * IntroductionToEntityFrameworkCore.csproj references:
     *
     *   <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
     *   <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
     *   <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.11" />
     *
     * Microsoft.EntityFrameworkCore - core abstractions (DbContext, LINQ provider).
     * Microsoft.EntityFrameworkCore.SqlServer - SQL Server / Azure SQL provider.
     * Microsoft.EntityFrameworkCore.InMemory - in-process store for tests (SECTION 9).
     *
     * Design-time package for migrations and scaffolding (added in ch03/ch04):
     *
     *   Microsoft.EntityFrameworkCore.Design
     *
     * Minimum usings for SQL Server apps:
     *
     *   using Microsoft.EntityFrameworkCore;
     *
     * Target framework: net8.0 (matches other tutorials in this repo).
     * -------------------------------------------------------------------------
     */
    public static void ExplainNuGetSetup()
    {
        Console.WriteLine("--- SECTION 3: NuGet setup ---");
        Console.WriteLine("  Packages: Microsoft.EntityFrameworkCore 8.0.11 + SqlServer + InMemory (see .csproj).");
        Console.WriteLine("  Design package (migrations/scaffold) -> ch03 and ch04.");
    }

    /*
     * =========================================================================
     * SECTION 4: EF CORE DATABASE PROVIDERS
     * =========================================================================
     *
     * EF Core is provider-based: the same DbContext and LINQ surface can target
     * different databases by swapping the provider extension method:
     *
     *   Provider package                              | UseSql* extension
     *   ----------------------------------------------|---------------------------
     *   Microsoft.EntityFrameworkCore.SqlServer       | UseSqlServer(connection)
     *   Npgsql.EntityFrameworkCore.PostgreSQL         | UseNpgsql(connection)
     *   Pomelo.EntityFrameworkCore.MySql              | UseMySql(...)
     *   Microsoft.EntityFrameworkCore.Sqlite            | UseSqlite(connection)
     *   Microsoft.EntityFrameworkCore.InMemory        | UseInMemoryDatabase(name)
     *
     * Each provider translates LINQ to dialect-specific SQL (or in-memory ops).
     * This module focuses on SQL Server; In-Memory is preview-only here for
     * runnable demos without a database install.
     *
     * Provider-specific features (hierarchyid, JSON columns, etc.) may require
     * raw SQL -> ch10 Raw SQL & Stored Procedures.
     * -------------------------------------------------------------------------
     */
    public static void ExplainProviders()
    {
        Console.WriteLine("--- SECTION 4: EF Core providers ---");
        Console.WriteLine("  Swap provider via UseSqlServer, UseNpgsql, UseSqlite, UseInMemoryDatabase, ...");
        Console.WriteLine("  Same DbContext/LINQ surface; SQL dialect changes per provider.");
        Console.WriteLine("  This track uses SqlServer; In-Memory preview in SECTION 9.");
    }

    /*
     * =========================================================================
     * SECTION 5: CODE-FIRST VS DATABASE-FIRST OVERVIEW
     * =========================================================================
     *
     * Two common workflows for building the EF model:
     *
     *   | Approach        | Source of truth | Typical steps
     *   |-----------------|-----------------|----------------------------------
     *   | Code-First      | C# entity classes | Write entities -> Add-Migration ->
     *   |                 |                 | Update-Database -> evolve with migrations
     *   | Database-First  | Existing SQL DB | Scaffold-DbContext / dotnet ef dbcontext
     *   |                 |                 | scaffold -> generated entities + DbContext
     *
     * Code-First suits greenfield apps where the team owns the schema through
     * migrations (ch03 Code-First Models & Migrations).
     *
     * Database-First suits legacy or DBA-owned schemas where C# mirrors an
     * existing database (ch04 Database-First & Reverse Engineering).
     *
     * After the model exists, both paths share the same DbContext, LINQ, CRUD,
     * and relationship chapters from ch05 onward (see module README).
     * -------------------------------------------------------------------------
     */
    public static void ExplainCodeFirstVsDatabaseFirst()
    {
        Console.WriteLine("--- SECTION 5: Code-First vs Database-First ---");
        Console.WriteLine("  Code-First: C# entities drive schema via migrations -> ch03.");
        Console.WriteLine("  Database-First: existing DB scaffolded to entities -> ch04.");
        Console.WriteLine("  Same DbContext/LINQ/CRUD from ch05 onward for both paths.");
    }

    /*
     * =========================================================================
     * SECTION 6: EF CORE VS ADO.NET VS DAPPER
     * =========================================================================
     *
     * | Approach   | You write           | Mapping                 | Best for
     * |------------|---------------------|-------------------------|----------------------------------
     * | ADO.NET    | SQL + reader loop   | Manual GetInt32/...     | Max control, streaming, provider APIs
     * | Dapper     | SQL                 | Automatic Query<T>      | Hand-tuned SQL, reports, low overhead
     * | EF Core    | LINQ + entities     | Change tracker + SQL    | Domain model, migrations, relationships
     *
     * Choose EF Core when:
     *   - CRUD and relationships are modeled as entities, not ad hoc SQL
     *   - Migrations or scaffolded models keep schema and code aligned
     *   - Change tracking and SaveChanges simplify unit-of-work patterns
     *
     * Stay on ADO.NET or Dapper when:
     *   - Every query is hand-optimized and EF's SQL is unacceptable
     *   - You need provider-specific reader behavior without EF overhead
     *   - Read-only reporting with no change tracking requirement
     *
     * Many production apps use EF Core for writes/domain and Dapper or raw SQL
     * for heavy read paths on the same database (ch10 covers EF raw SQL escape hatches).
     * -------------------------------------------------------------------------
     */
    public static void ExplainStackComparison()
    {
        Console.WriteLine("--- SECTION 6: EF Core vs ADO.NET vs Dapper ---");
        Console.WriteLine("  ADO.NET  - SqlConnection, SqlCommand, SqlDataReader loops (ADO.NET track).");
        Console.WriteLine("  Dapper   - same SQL, Query<T> maps rows (Dapper track).");
        Console.WriteLine("  EF Core  - LINQ on DbSet<T>, change tracker, migrations (this track).");
    }
}
