using System.Collections.Generic;

namespace CodeFirstModelsAndMigrations.Models;

/*
 * FILE ROLE: Code-First entity classes and the conventions EF Core applies by default.
 *
 * SECTIONS IN THIS FILE:
 *   1. Code-First entity classes
 *   2. EF Core conventions by default (table/column/key naming)
 */

/*
 * =========================================================================
 * SECTION 1: CODE-FIRST ENTITY CLASSES
 * =========================================================================
 *
 * Code-First means your C# types are the source of truth for the schema.
 * EF Core discovers entity types through:
 *   - public DbSet<T> properties on DbContext (see Data/StoreDbContext.cs)
 *   - types referenced by navigation properties on other entities
 *
 * A typical entity is a plain CLR class (POCO) with:
 *   - a primary key property (Id or {TypeName}Id)
 *   - scalar properties mapped to columns
 *   - optional navigation properties to related entities
 *
 * No base class required. No [Table] attribute required when conventions match.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: EF CORE CONVENTIONS BY DEFAULT
 * =========================================================================
 *
 * Without Fluent API or data annotations, EF Core applies these rules:
 *
 *   Concept              | Convention applied to Category
 *   ---------------------|-----------------------------------------------
 *   Table name           | DbSet property name -> dbo.Categories (pluralized)
 *   Column name          | Same as property name -> Name maps to Name
 *   Primary key          | Id or CategoryId -> CategoryId is PK here
 *   String column        | nvarchar(max), required unless nullable (string?)
 *   decimal              | decimal(18,2) by default for SQL Server
 *   bool                 | bit
 *   DateTime             | datetime2
 *
 * Pluralization uses English rules (Category -> Categories). Override table
 * names with Fluent API or [Table] - COVERED IN DETAIL LATER ->
 * 07. Fluent API & Data Annotations.
 *
 * Pitfall: a property named Id on multiple unrelated entities is fine - each
 * maps to its own table. A missing key property causes model validation errors
 * at startup or when generating migrations.
 * -------------------------------------------------------------------------
 */
public class Category
{
    public int CategoryId { get; set; }              // convention: primary key (ends with Id, type name prefix)
    public string Name { get; set; } = string.Empty; // maps to NVARCHAR - required (non-nullable reference)
    public ICollection<Product> Products { get; set; } = new List<Product>(); // navigation - one-to-many (preview ch06)
}
