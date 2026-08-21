using System;
using System.Data;
using Dapper;

namespace DapperMappingAndAdvancedPatterns.Utils.TypeHandlers;

/*
 * FILE ROLE:
 *   Custom SqlMapper.TypeHandler for DateOnly (SECTION 6 preview).
 *
 * SECTIONS IN THIS FILE:
 *   6. Custom type handlers — SqlMapper.AddTypeHandler preview
 */

/*
 * SECTION 6: CUSTOM TYPE HANDLERS — PREVIEW
 *
 * Dapper maps common CLR types to SQL types automatically. For types it does not know
 * (DateOnly, custom value objects, unusual bool encodings), register a TypeHandler:
 *
 *   SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
 *
 * | Method    | Role                                              |
 * |-----------|---------------------------------------------------|
 * | Parse     | Convert database value → CLR type on read         |
 * | SetValue  | Convert CLR type → parameter value on write       |
 *
 * Register once at startup (before any Query using DateOnly).
 * SQL Server DATE/DATETIME columns round-trip through DateTime internally.
 * -------------------------------------------------------------------------
 */
public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        if (value is DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }

        if (value is DateOnly dateOnly)
        {
            return dateOnly;
        }

        throw new DataException($"Cannot convert {value?.GetType().Name ?? "null"} to DateOnly.");
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToDateTime(TimeOnly.MinValue); // DATE column accepts DateTime at midnight
    }
}
