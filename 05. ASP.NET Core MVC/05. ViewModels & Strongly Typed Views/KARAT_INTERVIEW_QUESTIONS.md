# Karat — Interview Questions

> **Folder:** `06. ASP.NET Core MVC/05. ViewModels & Strongly Typed Views`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Review this MVC edit flow. The GET page renders, but POST throws `InvalidOperationException` about a tracked entity, and the Razor view shows columns that must never appear in HTML.

```csharp
// GET
public async Task<IActionResult> Edit(int id)
{
    var order = await _db.Orders.Include(o => o.Customer).FirstAsync(o => o.Id == id);
    return View(order); // @model Order
}

// POST
[HttpPost]
public async Task<IActionResult> Edit(int id, Order model)
{
    _db.Orders.Update(model);
    await _db.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
```

The `Order` entity has `Customer`, `LineItems`, `InternalMarginPercent`, and `RowVersion`.

---

#### Q2. (R) Review this POST action for privilege escalation via mass assignment. QA passes because testers only change `ShipDate`.

```csharp
public class OrderEditViewModel
{
    public int Id { get; set; }
    public DateTime ShipDate { get; set; }
}

// Hidden in DB entity but NOT on ViewModel:
// public bool IsPriority { get; set; }
// public decimal DiscountPercent { get; set; }

[HttpPost]
public async Task<IActionResult> Edit(OrderEditViewModel vm)
{
    var order = await _db.Orders.FindAsync(vm.Id);
    _mapper.Map(vm, order); // AutoMapper profile: CreateMap<OrderEditViewModel, Order>().ReverseMap();
    await _db.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
```

Attacker POSTs extra JSON/form fields: `IsPriority=true&DiscountPercent=100`.

---

#### Q3. (D) A team shares one `ProductDto` between the REST API (`ProductsController`) and MVC admin screens (`ProductsController` in Areas/Admin). API clients need `SupplierCost` and audit timestamps; the browser form must not expose them. What breaks if you keep one type, and how do you split responsibilities without duplicating every field?

---

#### Q4. (R) Review create vs edit ViewModel design. New products save with `Id = 0` overwriting an existing row; edit forms show validation errors on fields that should be read-only.

```csharp
public class ProductViewModel
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public string Sku { get; set; } = "";
    public bool IsDiscontinued { get; set; } // admin-only on edit
}

[HttpPost]
public async Task<IActionResult> Save(ProductViewModel vm)
{
    var entity = vm.ProductId == 0 ? new Product() : await _db.Products.FindAsync(vm.ProductId);
    entity.Name = vm.Name;
    entity.UnitPrice = vm.UnitPrice;
    entity.Sku = vm.Sku;
    entity.IsDiscontinued = vm.IsDiscontinued;
    if (vm.ProductId == 0) _db.Products.Add(entity);
    await _db.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}
```

Create and Edit both use `@model ProductViewModel` and the same `Save` action.

---

#### Q5. (R) Review mapping from entity to ViewModel. Support reports that after deploy, customer phone numbers and internal notes appear in "View Source" on the order details page.

```csharp
public class OrderDetailsViewModel
{
    public int OrderId { get; set; }
    public string CustomerDisplayName { get; set; } = "";
    public decimal Total { get; set; }
}

// AutoMapper profile
CreateMap<Order, OrderDetailsViewModel>()
    .ForMember(d => d.CustomerDisplayName, o => o.MapFrom(s => s.Customer.Name));

// Controller
var vm = _mapper.Map<OrderDetailsViewModel>(order);
return View(vm);
```

```html
@* Views/Orders/Details.cshtml *@
@model OrderDetailsViewModel
<input asp-for="CustomerDisplayName" />
@* Developer copied scaffold from Edit view *@
```

Entity `Customer` also has `Phone`, `Email`, `InternalNotes` — not on ViewModel.

---

#### Q6. (R) Review nullable reference types on a strongly typed create form. Compiler is clean; production logs show `NullReferenceException` in POST and optional middle name never round-trips.

```csharp
#nullable enable
public class RegisterViewModel
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    public string MiddleName { get; set; }  // optional
    [Required][EmailAddress] public string Email { get; set; }
}

[HttpPost]
public IActionResult Register(RegisterViewModel model)
{
    if (!ModelState.IsValid) return View(model);
    _users.Create(model.FirstName, model.MiddleName.Trim(), model.Email);
    return RedirectToAction(nameof(Index));
}
```

Form omits `MiddleName` when blank; model binding sets it to `null`.

---

#### Q7. (R) Review this AJAX partial refresh. The first load works; subsequent calls return 500 with JSON serialization cycle errors in logs.

```csharp
[HttpGet]
public IActionResult OrderSummary(int id)
{
    var order = _db.Orders
        .Include(o => o.Customer)
        .Include(o => o.LineItems)
        .First(o => o.Id == id);
    return PartialView("_OrderSummary", order); // @model Order
}

// _OrderSummary.cshtml — also used by SignalR hub pushing Order JSON to clients
```

`Customer` has `ICollection<Order> Orders`; `LineItem` has navigation back to `Order`.

---

#### Q8. (R) Review validation placement. Changing a business rule requires a DB migration; unit tests for the ViewModel pass but wrong totals still save.

```csharp
public class Order
{
    public int Id { get; set; }
    [Required][Range(0.01, 10000)] public decimal UnitPrice { get; set; }
    [Range(1, 100)] public int Quantity { get; set; }
}

public class OrderLineViewModel
{
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

[HttpPost]
public async Task<IActionResult> AddLine(OrderLineViewModel vm)
{
    var line = _mapper.Map<OrderLine>(vm);
    _db.OrderLines.Add(line);
    await _db.SaveChangesAsync();
    return Ok();
}
```

Rule: `LineTotal` must not exceed $50,000 per line.

---

#### Q9. (D) A dashboard action builds one ViewModel for a page with orders grid, user profile card, notification feed, and chart series. The type has 40+ properties and three nested lists. Refactors are painful and partial views reuse the whole model. What are the concrete risks, and how would you decompose without fragmenting the page into dozens of controller round-trips?

---

#### Q10. (R) Review list performance. The orders index page times out at ~2k rows after switching from manual projection to AutoMapper.

```csharp
public IActionResult Index()
{
    var orders = _db.Orders
        .Include(o => o.Customer)
        .Include(o => o.LineItems)
        .OrderByDescending(o => o.CreatedUtc)
        .ToList();

    var vms = _mapper.Map<List<OrderListItemViewModel>>(orders);
    return View(vms);
}

// Profile uses ReverseMap and maps Customer.Name -> CustomerName
CreateMap<Order, OrderListItemViewModel>().ReverseMap();
```

`OrderListItemViewModel` needs: `OrderId`, `CustomerName`, `Total`, `Status` — nothing else.

---
