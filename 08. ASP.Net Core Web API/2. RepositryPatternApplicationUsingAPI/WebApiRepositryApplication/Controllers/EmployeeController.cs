using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiRepositryApplication.Models;
using WebApiRepositryApplication.Repository;

namespace WebApiRepositryApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private EmployeeRepository _employees;
        private DepartmentRepository _departments;

        public EmployeeController(EmployeeRepository employees,DepartmentRepository departments)
        {
            this._employees = employees;
            this._departments = departments;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var result = await _employees.GetEmployeeList();

            if(result == null)
            {
                return NotFound("Employees Not Found!!");
            }
            return Ok(result);
        }

        [HttpGet("{id}")]

        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var result = await _employees.GetEmployeeById(id);

            if(result == null)
            {
                return NotFound($"Employee With Id = {id} Not Found!!");
            }
            return Ok(result);
        }

        [HttpGet("Department/{id}")]
        public async Task<ActionResult<List<Employee>>> GetEmployeeOfDepartment(int id)
        {
            var result = await _employees.GetAllEmployeesOfDepartment(id);

            if(result == null)
            {
                return NotFound($"Department with id = {id} not found!!");
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            var result = await _employees.PostEmployee(employee);

            if(result == null)
            {
                return BadRequest($"Employee Cannot be Inserted!!");
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Employee>> UpdateEmployee(int id,Employee employee)
        {
            var result = await _employees.PutEmployee(id,employee);

            if(result == null)
            {
                return BadRequest($"Employee Cannot be Updated!!");
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteEmployee(int id)
        {
            var result =  await _employees.DeleteEmployee(id);

            if(result == null)
            {
               return NotFound("Employee Cannot Be Found !!");
            }
            return Ok(result);
        }


    }
}
