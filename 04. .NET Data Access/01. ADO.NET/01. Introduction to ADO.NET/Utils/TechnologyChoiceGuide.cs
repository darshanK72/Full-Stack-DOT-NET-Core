using System.Collections.Generic;

namespace IntroductionToAdoNet.Utils;

/*
 * FILE ROLE: ADO.NET vs Dapper vs EF Core decision guide (Section 10).
 * SECTIONS IN THIS FILE:
 *   10. ADO.NET vs Dapper vs EF Core
 */

/*
 * =========================================================================
 * SECTION 10: ADO.NET VS DAPPER VS EF CORE
 * =========================================================================
 *
 * All three ultimately use ADO.NET connections. The difference is how much
 * boilerplate and how much SQL/ORM mapping you own.
 *
 *   Technology | What you write              | Strengths                    | Trade-offs
 *   -----------|-----------------------------|------------------------------|-----------------------------
 *   ADO.NET    | SqlConnection, SqlCommand,  | Full control, max perf,      | Most verbose; manual mapping,
 *              | manual mapping              | bulk APIs, any SQL shape     | you own SQL and connections
 *   Dapper     | SQL strings + parameters;   | Thin micro-ORM; fast;        | No change tracking; SQL still
 *              | maps to POCOs automatically | minimal magic                | yours; manual graph loading
 *   EF Core    | DbContext, LINQ, migrations | Model-first, relationships,  | Heavier; generated SQL may
 *              |                             | change tracking, migrations  | need tuning; learning curve
 *
 * When to choose:
 *   • ADO.NET — reporting, bulk copy, TVPs, hand-tuned SQL, legacy sprocs
 *   • Dapper  — services that mostly run explicit SQL with light mapping
 *   • EF Core — domain models, CRUD apps, migrations, relationship graphs
 *
 * Submodule pointers:
 *   Dapper    → 04. .NET Data Access / 02. Dapper
 *   EF Core   → 04. .NET Data Access / 03. Entity Framework Core
 * -------------------------------------------------------------------------
 */
public static class TechnologyChoiceGuide
{
    public static IReadOnlyList<(string Tech, string BestFor)> Recommendations()
    {
        return new[]
        {
            ("ADO.NET", "Bulk import, complex sprocs, performance-critical paths, full SQL control"),
            ("Dapper", "Microservices with hand-written SQL and simple POCO mapping"),
            ("EF Core", "Application-centric models, LINQ queries, schema migrations"),
        };
    }
}
