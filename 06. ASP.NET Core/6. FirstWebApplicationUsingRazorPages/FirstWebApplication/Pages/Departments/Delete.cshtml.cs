using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FirstWebApplication.Models;

namespace FirstWebApplication.Pages.Departments
{
    public class DeleteModel : PageModel
    {
        private readonly FirstWebApplication.Models.EmployeeDbContext _context;

        public DeleteModel(FirstWebApplication.Models.EmployeeDbContext context)
        {
            _context = context;
        }

        [BindProperty]
      public Department Department { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Departments == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FirstOrDefaultAsync(m => m.DepartmentId == id);

            if (department == null)
            {
                return NotFound();
            }
            else 
            {
                Department = department;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.Departments == null)
            {
                return NotFound();
            }
            var department = await _context.Departments.FindAsync(id);

            if (department != null)
            {
                Department = department;
                _context.Departments.Remove(Department);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
