using System.Collections.Generic;
using System.Linq;
using DbContextAndDbSet.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DbContextAndDbSet.Utils;

/*
 * FILE ROLE: Shows how AddDbContext registers AppDbContext with Microsoft.Extensions.DependencyInjection
 *            - the same registration shape ASP.NET Core uses, wired here in a console host.
 *
 * SECTIONS IN THIS FILE:
 *   8. AddDbContext and scoped lifetime preview
 *   8b. Resolving AppDbContext from IServiceProvider
 */

public static class DbContextServiceRegistration
{
    /*
     * SECTION 8: AddDbContext AND SCOPED LIFETIME PREVIEW
     *
     * AddDbContext<TContext> registers:
     *   - DbContextOptions<TContext> (singleton factory that builds options per registration)
     *   - TContext itself as SCOPED - one instance per scope (per HTTP request in web apps)
     *
     * Console equivalent of "one request":
     *   using IServiceScope scope = provider.CreateScope();
     *   AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
     *
     * Registration overload used here mirrors Program.cs in ASP.NET Core:
     *
     *   services.AddDbContext<AppDbContext>(options =>
     *       options.UseSqlServer(connectionString));
     *
     * Full ASP.NET Core host, appsettings.json, and middleware pipeline -> Web API modules.
     * DbContext pooling (AddDbContextPool) -> performance topic; default AddDbContext is enough here.
     *
     * Pitfall: Resolving AppDbContext from the root IServiceProvider (without a scope) throws
     * because scoped services cannot be created from a singleton root in strict DI validation.
     */
    public static ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString)); // same UseSqlServer as ConnectionOptions helper

        return services.BuildServiceProvider();
    }

    /*
     * SECTION 8b: RESOLVING AppDbContext FROM IServiceProvider
     *
     * Returns products loaded through a DI-resolved context to prove registration works.
     * Always create a scope before resolving a scoped DbContext.
     */
    public static IReadOnlyList<Models.Product> GetProductsViaDependencyInjection(ServiceProvider provider)
    {
        using IServiceScope scope = provider.CreateScope();
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return context.Products
            .OrderBy(product => product.Id)
            .ToList(); // executes SELECT against dbo.Products
    }
}
