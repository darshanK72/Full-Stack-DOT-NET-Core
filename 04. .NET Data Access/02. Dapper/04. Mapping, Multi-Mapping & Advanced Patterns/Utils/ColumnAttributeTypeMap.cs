using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Dapper;

namespace DapperMappingAndAdvancedPatterns.Utils;

/*
 * FILE ROLE:
 *   Registers a CustomPropertyTypeMap so Dapper honors [Column] attributes (SECTION 2b).
 *
 * SECTIONS IN THIS FILE:
 *   2b. CustomPropertyTypeMap for [Column] attribute support
 */

/*
 * SECTION 2b: CUSTOMPROPERTYTYPEMAP FOR [COLUMN] ATTRIBUTE
 *
 * SqlMapper.SetTypeMap replaces default name matching for one CLR type.
 * CustomPropertyTypeMap resolves column names from [Column("Name")] when present,
 * otherwise falls back to the property name.
 *
 * Call RegisterOnce() before Query<ColumnMappedProduct> — Program.cs does this at startup.
 * -------------------------------------------------------------------------
 */
public static class ColumnAttributeTypeMap
{
    private static bool _registered;

    public static void RegisterOnce()
    {
        if (_registered)
        {
            return;
        }

        SqlMapper.SetTypeMap(
            typeof(Models.ColumnMappedProduct),
            new CustomPropertyTypeMap(
                typeof(Models.ColumnMappedProduct),
                (type, columnName) =>
                {
                    PropertyInfo? match = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(property =>
                        {
                            ColumnAttribute? column = property.GetCustomAttribute<ColumnAttribute>();
                            string mappedName = column?.Name ?? property.Name;
                            return string.Equals(mappedName, columnName, StringComparison.OrdinalIgnoreCase);
                        });

                    return match!; // null tells Dapper to skip unmapped columns
                }));

        _registered = true;
    }
}
