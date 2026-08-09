---
module: 08. Advanced C# Features
difficulty: Medium
chapters: 02 Reflection & Attributes
domain: RetailCatalog
---

# Product Export Scanner

Build a **.NET 8 console application from scratch** that uses reflection and custom attributes to build a CSV-style export manifest from decorated product types.

## Business context

A retail data team exports SKU catalogs to flat files for partners. Column headers come from `[DisplayLabel]` metadata; only properties marked `[Exportable]` appear. Class-level `[EntityTable]` supplies the logical table name in the manifest header.

## Definitions

**Attribute `[DisplayLabel(string label)]`**

- `AttributeTargets.Property`, `Inherited = true`
- Positional `Label` property (read-only)

**Attribute `[Exportable(bool include = true)]`**

- `AttributeTargets.Property`
- When `Include` is false, property is skipped even if present

**Attribute `[EntityTable(string tableName)]`**

- `AttributeTargets.Class`, `Inherited = false`

**Class `Product`**

- `[EntityTable("Products")]`
- `[DisplayLabel("SKU Code")]` + `[Exportable]` on `Sku`
- `[DisplayLabel("Product Name")]` + `[Exportable]` on `Name`
- `[DisplayLabel("Unit Price")]` + `[Exportable]` on `Price` (decimal)
- `[DisplayLabel("Stock Level")]` on `Stock` (int) — no Exportable, omitted from export

**Record `ExportColumn`**

- `Header` (string), `PropertyName` (string)

**Record `ExportManifest`**

- `TableName` (string), `Columns` (IReadOnlyList<ExportColumn>)

**Class `ProductExportScanner`**

- `ExportManifest BuildManifest<T>()` where `T : class` — read `[EntityTable]` on `typeof(T)`; scan public instance properties with `BindingFlags.Public | BindingFlags.Instance`; include property when `[Exportable]` exists and `Include != false`; header from `[DisplayLabel]` or fallback to property name
- `string[] ExportRow<T>(T instance)` where `T : class` — values in manifest column order via `PropertyInfo.GetValue`; format decimals with `"F2"`, others with `ToString()`
- `IReadOnlyList<string[]> ExportAll<T>(IEnumerable<T> items)` where `T : class`

## Demo Main

1. Create 2 `Product` instances with different prices/stock.
2. Build manifest; print table name and column headers.
3. Export both rows; print CSV lines (comma-separated, no quoting required for demo).

## Constraints

- net8, explicit usings, reflection only (no source generators)
- Reject null `instance` in `ExportRow` with `ArgumentNullException`
- Use `GetCustomAttribute<T>()` or equivalent

## Non-goals

File I/O, EF Core, performance caching of PropertyInfo

## Evaluation

[EVALUATION.md](EVALUATION.md)
