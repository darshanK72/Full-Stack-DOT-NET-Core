using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiRepositryApplication.Models;
using WebApiRepositryApplication.Repository;

namespace WebApiRepositryApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private DepartmentRepository _departments;

        public DepartmentController(DepartmentRepository departments)
        {
            this._departments = departments;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            var result = await _departments.GetDepartmentList();

            if (result == null)
            {
                return NotFound("Department Not Found!!");
            }
            return Ok(result);
        }

        [HttpGet("{id}")]

        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            var result = await _departments.GetDepartmentById(id);

            if (result == null)
            {
                return NotFound($"Department With Id = {id} Not Found!!");
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department)
        {
            var result = await _departments.PostDepartment(department);

            if (result == null)
            {
                return BadRequest($"Department Cannot be Inserted!!");
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Department>> UpdateDepartment(int id, Department department)
        {
            var result = await _departments.PutDepartment(id,department);

            if (result == null)
            {
                return BadRequest($"Department Cannot be Updated!!");
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteDepartment(int id)
        {
            var result = await _departments.DeleteDepartment(id);

            if (result == null)
            {
                return NotFound("Department Cannot Be Found !!");
            }
            return Ok(result);
        }

    }
}
