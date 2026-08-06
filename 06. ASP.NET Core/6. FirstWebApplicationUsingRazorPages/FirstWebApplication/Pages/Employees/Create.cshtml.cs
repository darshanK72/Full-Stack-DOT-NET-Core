using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ModelViews;

namespace FirstWebApplication.Pages.Employees
{
    public class CreateModel : PageModel
    {
        private readonly FirstWebApplication.Models.EmployeeDbContext _context;

        public CreateModel(FirstWebApplication.Models.EmployeeDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public EmployeeView EmployeeView { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid || _context.Employees == null || EmployeeView == null)
            {
                return Page();
            }

            Department? dept = _context.Departments.FirstOrDefault(dep => dep.DepartmentId == EmployeeView.DepartmentId);

            Employee emp = new Employee()
            {
                Name = EmployeeView.Name,
                DateOfBirth = EmployeeView.DateOfBirth,
                Department = dept,
                Email = EmployeeView.Email,
                Salary = EmployeeView.Salary
            };

            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
