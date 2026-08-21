# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/01. Introduction to MVC Pattern`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A team migrating from WebForms ports this "controller." It compiles and renders in dev. What architectural problems appear at scale, and how should responsibilities move in ASP.NET Core MVC?

```csharp
public class CustomerPageController : Controller
{
    public IActionResult Index()
    {
        if (!IsPostBack()) // extension from old WebForms helper
            return View();

        var id = int.Parse(Request.Form["CustomerId"]);
        using var conn = new SqlConnection(Configuration["DefaultConnection"]);
        conn.Open();
        var cmd = new SqlCommand("SELECT * FROM Customers WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        var reader = cmd.ExecuteReader();
        var model = new Customer();
        if (reader.Read())
        {
            model.Name = reader["Name"].ToString();
            model.Email = reader["Email"].ToString();
            if (model.Email.Contains("@") == false)
                ModelState.AddModelError("", "Invalid email");
            else
                SendWelcomeEmail(model.Email); // SMTP call inline
        }
        ViewBag.Message = "Loaded";
        return View(model);
    }
}
```

---

#### Q2. (D) A desktop WPF team and a web team both say they use "MV-something." Compare **MVC**, **MVP**, and **MVVM** for ASP.NET Core MVC — which pattern does the framework implement, where do the others still appear, and what confusion causes production bugs?

---

#### Q3. (R) Review this order checkout action promoted to production. Tests pass with an in-memory database. What breaks under load or when requirements change?

```csharp
public class OrdersController : Controller
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Checkout(int cartId)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstAsync(c => c.Id == cartId);
        if (cart.Items.Count == 0)
            return RedirectToAction("Index");

        decimal total = 0;
        foreach (var item in cart.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product.Stock < item.Qty)
                return View("Error", $"Out of stock: {product.Name}");
            product.Stock -= item.Qty;
            total += product.Price * item.Qty;
        }

        var order = new Order { UserId = User.FindFirstValue(ClaimTypes.NameIdentifier), Total = total };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var client = new HttpClient();
        await client.PostAsJsonAsync("https://payments.internal/charge", new { order.Id, total });

        return RedirectToAction("Confirm", new { order.Id });
    }
}
```

---

#### Q4. (P) Where should **input validation**, **business rules**, and **data access** live in a well-structured ASP.NET Core MVC app — and what symptoms appear when each is placed in the wrong layer (View, Controller, or Service)?

---

#### Q5. (R) This refactor was sold as "thin controllers + testability." Unit tests on `ProductsControllerTests` still require `TestServer` and a real database. Diagnose the layering mistake.

```csharp
public class ProductsController : Controller
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q));

        var products = await query.OrderBy(p => p.Name).ToListAsync();
        ViewBag.SearchTerm = q;
        ViewBag.ResultCount = products.Count;
        return View(products);
    }
}

// ProductsControllerTests — "unit" test
[Fact]
public async Task Index_filters_by_search_term()
{
    await using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    var response = await client.GetAsync("/Products?q=widget");
    response.EnsureSuccessStatusCode();
}
```

---

#### Q6. (D) Product asks for a **server-rendered admin CRUD** app (forms, validation messages, role-based pages). When do you choose **MVC**, **Razor Pages**, or **Minimal APIs + SPA**, and what would make you reverse the decision after six months?

---

#### Q7. (M) A browser submits `POST /Orders/Create` with form fields bound to `CreateOrderViewModel`. Trace responsibilities through **Model**, **View**, and **Controller** from first byte received through HTML response — where does model binding stop and where must business logic not run?

---

#### Q8. (R) A shared `_Layout.cshtml` and three feature views rely on `ViewBag` keys set inconsistently across controllers. QA reports intermittent blank sidebars and wrong page titles in production only on certain pods. Review the pattern — what is wrong?

```csharp
// HomeController
public IActionResult Index()
{
    ViewBag.Title = "Dashboard";
    ViewBag.ShowSidebar = true;
    ViewBag.CurrentUserDisplay = User.Identity?.Name;
    return View();
}

// ReportsController — different developer, six months later
public IActionResult Sales()
{
    ViewBag.PageTitle = "Sales"; // not ViewBag.Title
    ViewData["Sidebar"] = true;  // not ViewBag.ShowSidebar
    return View();
}
```

```html
<!-- _Layout.cshtml -->
<title>@ViewBag.Title</title>
@if (ViewBag.ShowSidebar == true) { <partial name="_Sidebar" model="ViewBag.CurrentUserDisplay" /> }
```

---

#### Q9. (R) A consultant puts domain rules inside the view model and presentation logic in the entity. Review before merge — what fails at compile time, runtime, and in API reuse?

```csharp
public class Product : BaseEntity
{
    public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }

    public string DisplayLabel => $"{Name} ({UnitPrice:C})";

    public bool IsEligibleForDiscount(HttpContext httpContext)
    {
        var role = httpContext.User.FindFirstValue(ClaimTypes.Role);
        return role == "Wholesale" && UnitPrice > 100;
    }
}

public class ProductEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }

    public void ApplyBusinessRules()
    {
        if (UnitPrice < 0) throw new InvalidOperationException("Price cannot be negative");
        if (Name.Length < 3) UnitPrice = 0; // "promo rule"
    }
}
```

---

#### Q10. (P) You inherit a "fat controller" codebase (~400 lines per action). Describe a **practical extraction sequence** to reach thin controllers without a big-bang rewrite — what moves first, what stays in the controller, and how you keep shipping?
