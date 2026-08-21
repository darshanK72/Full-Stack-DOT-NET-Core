# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/02. Controllers & Actions`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Review this action from a code review. The developer says "it compiles and works in Swagger." What is wrong with the return type and result handling?

```csharp
public class ProductsController : Controller
{
    private readonly IProductService _products;

    public ProductsController(IProductService products) => _products = products;

    [HttpGet("{id:int}")]
    public object Get(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid product id");
        var product = _products.GetById(id);
        if (product is null)
            return NotFound();
        return product;
    }
}
```

*(Route prefix: `[Route("api/[controller]")]` on the controller.)*

---

#### Q2. (R) A junior developer converts synchronous actions to async. After deploy, unhandled exceptions crash the worker process on slow queries. Review the action.

```csharp
public class OrdersController : Controller
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    [HttpGet("details/{id:int}")]
    public async void Details(int id)
    {
        var order = await _orders.GetAsync(id);
        View(order);
    }
}
```

---

#### Q3. (R) Review constructor and field usage on this MVC controller. Index works; other actions intermittently throw `NullReferenceException`.

```csharp
public class InvoicesController : Controller
{
    private IInvoiceService? _invoices;

    public InvoicesController() { }

    public IActionResult Index()
    {
        _invoices = HttpContext.RequestServices.GetRequiredService<IInvoiceService>();
        return View(_invoices.GetOpen());
    }

    [HttpPost]
    public IActionResult Approve(int id)
    {
        _invoices!.Approve(id);
        return RedirectToAction(nameof(Index));
    }
}
```

---

#### Q4. (R) A legacy module still resolves dependencies through a static locator. Review and explain what breaks for testing and lifetimes.

```csharp
public static class ServiceLocator
{
    public static IServiceProvider Provider { get; set; } = default!;
}

public class ReportsController : Controller
{
    public IActionResult Monthly()
    {
        var db = ServiceLocator.Provider.GetRequiredService<AppDbContext>();
        var rows = db.Reports.FromSqlRaw("EXEC usp_MonthlyReport").ToList();
        return View(rows);
    }
}
```

---

#### Q5. (R) The same controller serves a Razor admin page and a React widget on the dashboard. QA reports "HTML instead of JSON" for one endpoint. Review.

```csharp
[Route("admin/products")]
public class AdminProductsController : Controller
{
    private readonly IProductService _products;

    public AdminProductsController(IProductService products) => _products = products;

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var product = _products.GetById(id);
        if (product is null)
            return NotFound();
        return View("ProductDetail", product);
    }

    [HttpGet("lookup/{id:int}")]
    public IActionResult Lookup(int id)
    {
        var product = _products.GetById(id);
        return Json(product);
    }
}
```

*(React client calls `GET /admin/products/42` with `Accept: application/json`.)*

---

#### Q6. (R) Review this account controller. Security finds login CSRF exposure and duplicate route matches in integration tests.

```csharp
public class AccountController : Controller
{
    public IActionResult Login() => View();

    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        // sign-in logic
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        // sign-out logic
        return RedirectToAction("Login");
    }
}
```

---

#### Q7. (R) Review this create action. Invalid posts persist bad data; QA sees no validation messages on the form.

```csharp
public class CustomersController : Controller
{
    private readonly AppDbContext _db;

    public CustomersController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public IActionResult Create(CustomerViewModel model)
    {
        _db.Customers.Add(new Customer
        {
            Name = model.Name,
            Email = model.Email
        });
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
}
```

*(ViewModel has `[Required]` on `Name` and `[EmailAddress]` on `Email`.)*

---

#### Q8. (R) Attribute routing tests fail with `AmbiguousMatchException`. Review these action signatures.

```csharp
[Route("catalog")]
public class CatalogController : Controller
{
    [HttpGet("items")]
    public IActionResult Items() => View(_service.GetFeatured());

    [HttpGet("items")]
    public IActionResult ItemsByCategory(string category)
        => View("Items", _service.GetByCategory(category));
}
```

---

#### Q9. (R) Review this controller pulled from a tutorial. The team wants thin controllers and unit tests without a database.

```csharp
public class EmployeesController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        using var conn = new SqlConnection(_config.GetConnectionString("HrDb"));
        conn.Open();
        using var cmd = new SqlCommand("SELECT Id, Name, Dept FROM Employees", conn);
        var list = new List<Employee>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new Employee { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        return View(list);
    }
}
```

*(Constructor with `IConfiguration _config` omitted for brevity.)*

---

#### Q10. (P) An internal MVC app accepts POST forms for role changes. Pen test reports missing CSRF protection on one action. Review the pattern and what production setup is required beyond the attribute.

```csharp
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult EditUser(int id) => View(_users.Get(id));

    [HttpPost]
    public IActionResult EditUser(EditUserViewModel model)
    {
        _users.UpdateRole(model.UserId, model.Role);
        return RedirectToAction(nameof(Index));
    }
}
```

*(Razor form uses `<form asp-action="EditUser">` but the POST action has no antiforgery attribute.)*

---

#### Q11. (D) API consumers report inconsistent status codes: missing resources return 400, malformed ids return 404. Review this action and explain the correct HTTP semantics for MVC vs JSON API responses.

```csharp
[Route("api/inventory")]
public class InventoryController : Controller
{
    [HttpGet("{sku}")]
    public IActionResult Get(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return NotFound();

        if (sku.Length > 20)
            return BadRequest("SKU too long");

        var item = _inventory.Find(sku);
        if (item is null)
            return BadRequest($"Unknown SKU: {sku}");
        return Json(item);
    }
}
```

---

#### Q12. (M) A developer implements `IDisposable` on a controller to release an expensive native handle created in the constructor. Under load, handles leak and `_handle` is sometimes already disposed. Explain controller activation, lifetime, and the correct pattern.

```csharp
public class ScannerController : Controller, IDisposable
{
    private readonly NativeScanner _handle = new();

    public ScannerController() { }

    public IActionResult Scan()
    {
        var result = _handle.ReadBarcode();
        return Json(result);
    }

    public void Dispose()
    {
        _handle.Dispose();
    }
}
```

*(Assume default `ControllerActivator` and no custom `IControllerFactory`.)*
