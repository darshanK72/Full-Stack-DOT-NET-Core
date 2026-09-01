/*
 * FILE ROLE: Demonstrates async action methods, RedirectToAction with route values,
 *            and the FileResult / File() helper family using a list of Product view models.
 *
 * SECTIONS IN THIS FILE:
 *   12. Async action methods — Task<IActionResult> pattern
 *   13. RedirectToAction with route values
 *   14. FileResult and the File() helper
 */

using ControllersActions.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ControllersActions.Controllers;

/*
 * SECTION 12: ASYNC ACTION METHODS
 * ─────────────────────────────────────────────────────────────────────────────
 * Action methods can be asynchronous by returning Task<IActionResult> (or
 * ValueTask<IActionResult>). The MVC framework awaits the task before writing
 * the response, freeing the thread pool thread to handle other requests while
 * I/O (database queries, HTTP calls, file reads) is in progress.
 *
 * Pattern:
 *   public async Task<IActionResult> Index()
 *   {
 *       var results = await _repo.GetAllAsync();   // await releases the thread
 *       return View(results);                      // resumes after I/O completes
 *   }
 *
 * Rules:
 *   • Mark the method async and return Task<IActionResult>.
 *   • Include at least one await expression (otherwise CS1998 warning:
 *     "async method lacks await operators and will run synchronously").
 *   • Model binding and filters still work identically to synchronous actions.
 *   • Avoid async void — exceptions are unobservable; use Task instead.
 *
 * Performance note:
 *   Async is most valuable when the action awaits I/O-bound work (database,
 *   external HTTP). For CPU-bound work, async does not improve throughput.
 *
 * Task.Delay(0) below simulates an async call. In a real application this
 * would be replaced by:
 *   await _context.Products.ToListAsync();         // EF Core
 *   await _repo.GetAllAsync();                     // repository pattern
 *   await _httpClient.GetFromJsonAsync<T>(url);    // HttpClient call
 */
public class ProductsController : Controller
{
    // In-memory data source — replaces a real DB or repository for this tutorial
    private static readonly List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Widget",    Price = 9.99m,  Category = "Hardware"     },
        new Product { Id = 2, Name = "Gadget",    Price = 24.99m, Category = "Electronics"  },
        new Product { Id = 3, Name = "Doohickey", Price = 4.99m,  Category = "Misc"         },
        new Product { Id = 4, Name = "Thingamajig", Price = 14.49m, Category = "Hardware"   },
    };

    // Async action — returns ViewResult after simulated async data retrieval
    public async Task<IActionResult> Index()
    {
        await Task.Delay(0); // illustrates the async signature; real apps await I/O here
        return View(_products); // passes List<Product> as the typed model to Views/Products/Index.cshtml
    }

    // Async action with a parameter — demonstrates async + nullable result handling
    public async Task<IActionResult> Details(int id)
    {
        await Task.Delay(0);                                        // simulate async lookup
        Product? product = _products.Find(p => p.Id == id);        // returns Product? (nullable)
        if (product is null)                                        // guard: id not in list
            return NotFound(new { message = $"Product {id} not found." }); // 404 + JSON detail
        return View(product); // renders Views/Products/Details.cshtml (file not in project; runtime only)
    }

    /* ── SECTION 13: RedirectToAction WITH ROUTE VALUES ──────────────────── */
    /*
     * RedirectToAction has several overloads; the most important:
     *
     *   RedirectToAction(actionName)
     *     302 → same controller, named action, no extra route data.
     *
     *   RedirectToAction(actionName, controllerName)
     *     302 → different controller.
     *
     *   RedirectToAction(actionName, routeValues)
     *     302 → same controller + extra route values appended to the URL.
     *     routeValues is an anonymous object; each property becomes a route
     *     segment or query string parameter depending on the route template.
     *
     *   RedirectToAction(actionName, controllerName, routeValues)
     *     Full overload: different controller + route data.
     *
     *   RedirectToActionPermanent(...)
     *     Same as above but returns 301 Moved Permanently.
     *
     * PITFALL: Never hard-code URL strings for internal redirects — if the route
     * template changes, the hard-coded URL breaks silently. Use nameof(MethodName)
     * to get the method name as a compile-safe string.
     *
     * POST-REDIRECT-GET PATTERN:
     *   POST action receives a form submission → validates and saves → redirects to
     *   the GET action. This prevents duplicate form submissions on browser refresh.
     *
     *   [HttpPost]
     *   public IActionResult Save(Product product)
     *   {
     *       // save product ...
     *       return RedirectToAction(nameof(Index));   // PRG: redirect to GET
     *   }
     */
    [HttpPost]
    public IActionResult Save(Product product)
    {
        // In a real app: validate ModelState, save to DB, then redirect.
        // Here we demonstrate the PRG redirect after a POST.
        _ = product; // suppress unused-parameter warning in this demo action
        return RedirectToAction(nameof(Index)); // 302 → GET /Products/Index
    }

    public IActionResult GoToDetails(int id)
    {
        // Route value {id} is inserted into the URL: /Products/Details/3
        return RedirectToAction(nameof(Details), new { id = id }); // 302 → /Products/Details/{id}
    }

    public IActionResult GoToHome()
    {
        // Cross-controller redirect: full overload specifying controller name
        return RedirectToAction(nameof(Index), "Home"); // 302 → /Home/Index
    }

    public IActionResult GoToExternalSearch(string query)
    {
        // RedirectToAction with multiple route values — extra values become query string params
        return RedirectToAction(nameof(Index), new { search = query, page = 1 });
        // → /Products/Index?search={query}&page=1
    }

    /* ── SECTION 14: FileResult AND THE File() HELPER ───────────────────── */
    /*
     * The File() helper produces a FileResult response — an HTTP response whose
     * body IS the file content. Three overloads cover the main scenarios:
     *
     *   File(byte[] fileContents, string contentType, string fileDownloadName)
     *     → FileContentResult. Entire file buffered in memory before sending.
     *       Use for small files (reports, CSVs, generated PDFs < a few MB).
     *       fileDownloadName sets Content-Disposition: attachment; filename="…"
     *       triggering a browser download dialog.
     *
     *   File(Stream fileStream, string contentType, string fileDownloadName)
     *     → FileStreamResult. File is streamed from the stream object.
     *       More efficient for large files — avoids loading all bytes into RAM.
     *
     *   PhysicalFile(string physicalPath, string contentType, string fileDownloadName)
     *     → PhysicalFileResult. Reads from an absolute path on the server's file system.
     *       Efficient: ASP.NET Core can use sendfile system call on Linux.
     *
     * VirtualFile(virtualPath, contentType)
     *     → VirtualFileResult. Reads from the app's virtual file system (IFileProvider),
     *       which includes wwwroot/ by default.
     *
     * Content-Type (MIME type) reference:
     *   "text/csv"                     CSV data
     *   "application/json"             JSON
     *   "application/pdf"              PDF
     *   "application/octet-stream"     unknown binary — forces download
     *   "image/png" / "image/jpeg"     images — usually inline display
     *
     * SECURITY NOTE: When the file path comes from user input (e.g. a query parameter),
     *   validate and sanitise it to prevent path traversal attacks:
     *   Path.GetFullPath(input).StartsWith(allowedRoot) before serving.
     */
    public IActionResult Export()
    {
        // Build a CSV string in memory and return as a downloadable file
        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,Price,Category");             // CSV header row
        foreach (Product p in _products)
            sb.AppendLine($"{p.Id},{p.Name},{p.Price},{p.Category}"); // one row per product

        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString()); // convert to byte array
        return File(bytes, "text/csv", "products.csv");        // 200 + Content-Disposition: attachment
    }

    public IActionResult ExportStream()
    {
        // MemoryStream example — demonstrates FileStreamResult path
        var ms = new System.IO.MemoryStream();
        using (var writer = new System.IO.StreamWriter(ms, Encoding.UTF8, leaveOpen: true))
        {
            writer.WriteLine("Id,Name,Price");
            foreach (Product p in _products)
                writer.WriteLine($"{p.Id},{p.Name},{p.Price}");
        }
        ms.Position = 0; // reset stream to beginning before sending
        return File(ms, "text/csv", "products-stream.csv"); // FileStreamResult — streamed to client
    }
}
