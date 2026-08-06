using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalServerWebApiApplication.Models;
using LocalServerWebApiApplication.Helpers;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace LocalServerWebApiApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly Trainingdb46310114Context _context;
        private readonly IConfiguration _configuration;

        public DepartmentsController(Trainingdb46310114Context context,IConfiguration config)
        {
            _context = context;
            _configuration = config;
        }

        // GET: api/Departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
          if (_context.Departments == null)
          {
              return NotFound();
          }
            return await _context.Departments.ToListAsync();
        }

        // GET: api/Departments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
          if (_context.Departments == null)
          {
              return NotFound();
          }
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            return department;
        }

        // PUT: api/Departments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment(int id, Department department)
        {
            if (id != department.DepartmentId)
            {
                return BadRequest();
            }

            _context.Entry(department).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Departments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department)
        {
          //if (_context.Departments == null)
          //{
          //    return Problem("Entity set 'Trainingdb46310114Context.Departments'  is null.");
          //}
          //  _context.Departments.Add(department);
          //  try
          //  {
          //      await _context.SaveChangesAsync();
          //  }
          //  catch (DbUpdateException)
          //  {
          //      if (DepartmentExists(department.DepartmentId))
          //      {
          //          return Conflict();
          //      }
          //      else
          //      {
          //          throw;
          //      }
          //  }

            bool _isuploaded = await DepartmentHelper.UploadBlob(_configuration, department);

            if (_isuploaded)
            {
                return Ok(new
                {
                    Message = "Upload is in progress"
                });

            }

            return StatusCode(200);   
        }

        // DELETE: api/Departments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            if (_context.Departments == null)
            {
                return NotFound();
            }
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DepartmentExists(int id)
        {
            return (_context.Departments?.Any(e => e.DepartmentId == id)).GetValueOrDefault();
        }
    }
}
