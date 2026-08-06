using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiRepositryApplication.Models;

namespace WebApiRepositryApplication.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private EmployeeDbContext _dbContext;
   
        public EmployeeRepository(EmployeeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Employee>?> GetEmployeeList()
        {
            return await _dbContext.Employees.ToListAsync();
        }
        public async Task<Employee?> GetEmployeeById(int employeeId)
        {
            return await _dbContext.Employees.FirstOrDefaultAsync(employee => employee.EmployeeId == employeeId);
        }
        public async Task<Employee?> PostEmployee(Employee employee)
        {
            var dept = _dbContext.Departments.Find(employee.DepartmentId);
            if(dept != null)
            {
                employee.Department = dept;
            }
            else
            {
                return null;
            }

            var result = await _dbContext.Employees.AddAsync(employee);

            await _dbContext.SaveChangesAsync();

            return result.Entity;
        }

        public async Task<Employee?> PutEmployee(int id,Employee employee)
        {
            var result = await _dbContext.Employees.FirstOrDefaultAsync(emp => emp.EmployeeId == id);

            if(result != null)
            {
                result.Name = employee.Name;
                result.Email = employee.Email;
                result.DateOfBirth = employee.DateOfBirth;
                result.Department = employee.Department;
                result.Salary = employee.Salary;
                result.DepartmentId = employee.DepartmentId;

                await _dbContext.SaveChangesAsync();

                return result;
            }

            return null;
           
        }
        public async Task<string?> DeleteEmployee(int employeeId)
        {
            var result = await _dbContext.Employees.FirstOrDefaultAsync(emp=>emp.EmployeeId == employeeId);
            if(result != null)
            {
                _dbContext.Employees.Remove(result);
                await _dbContext.SaveChangesAsync();
                return "Employee Deleted Successfull.";
            }
            return null;
           
        }

        public async Task<List<Employee>?> GetAllEmployeesOfDepartment(int departmentId)
        {
            var result = _dbContext.Departments.Find(departmentId);
            if (result != null)
            {
                return  _dbContext.Employees.Where(emp => emp.DepartmentId == departmentId).ToList();
            }
            return null;
            
        }

        

        

        
    }
}
