using Microsoft.AspNetCore.Mvc;
using ControllerWebApplication.Model;

namespace ControllerWebApplication.Controllers
{
    [Controller]
    public class EmployeeController : Controller
    {
        [Route("/Employee/{employeeId}/{employeeName}/{employeeAge}")]
        public IActionResult GetEmployeeDetails(Employee employee)
        {
            return Content($"Employee ID : {employee.EmployeeId}\nEmployee Name : {employee.EmployeeName}\nEmployee Age : {employee.EmployeeAge}");
        }
    }
}
