using CodeFirstModelsAndMigrations.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CodeFirstModelsAndMigrations.Data;

/*
 * FILE ROLE: Design-time factory so dotnet ef migrations add can construct StoreDbContext
 *            without a running application or DI container.
 *
 * SECTIONS IN THIS FILE:
 *   5. Design-time DbContext creation (dotnet ef tooling)
 */

/*
 * =========================================================================
 * SECTION 5: DESIGN-TIME DbContext CREATION
 * =========================================================================
 *
 * dotnet ef builds the project and needs a StoreDbContext instance to diff
 * the model. At design time there is no Main(), no DI, and no appsettings.json.
 *
 * IDesignTimeDbContextFactory<T> is the explicit hook EF Core looks for.
 * CreateDbContext builds the same options you use at runtime (LocalDB here).
 *
 * Alternative: parameterless StoreDbContext ctor + OnConfiguring fallback only.
 * This factory keeps OnConfiguring as a secondary fallback and documents the
 * tooling path clearly for readers.
 * -------------------------------------------------------------------------
 */
public class StoreDbContextFactory : IDesignTimeDbContextFactory<StoreDbContext>
{
    public StoreDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<StoreDbContext> options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseSqlServer(ConnectionHelper.LocalDbConnectionString)
            .Options;

        return new StoreDbContext(options);
    }
}
