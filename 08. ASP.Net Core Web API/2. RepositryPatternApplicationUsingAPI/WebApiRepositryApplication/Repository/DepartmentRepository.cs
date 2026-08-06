using Microsoft.EntityFrameworkCore;
using WebApiRepositryApplication.Models;

namespace WebApiRepositryApplication.Repository
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private EmployeeDbContext _dbContext;

        public DepartmentRepository(EmployeeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Department?> GetDepartmentById(int departmentId)
        {
            return await _dbContext.Departments.FirstOrDefaultAsync(dept =>  dept.DepartmentId == departmentId);
        }

        public async Task<IEnumerable<Department>?> GetDepartmentList()
        {
            return await _dbContext.Departments.ToListAsync();
        }

        public async Task<Department?> PostDepartment(Department department)
        {
            await Console.Out.WriteLineAsync();
            var result = await _dbContext.Departments.AddAsync(department);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
         
        }

        public async Task<Department?> PutDepartment(int id, Department department)
        {
            var result = await _dbContext.Departments.FirstOrDefaultAsync(dept => dept.DepartmentId == id);

            if (result != null)
            {
                result.DepartmentName = department.DepartmentName;
                await _dbContext.SaveChangesAsync();
                return result;
            }
            return null;

        }
        public async Task<string?> DeleteDepartment(int departmentId)
        {
            var result = await _dbContext.Departments.FirstOrDefaultAsync(dept => dept.DepartmentId == departmentId);
            if (result != null)
            {
                _dbContext.Departments.Remove(result);
                await _dbContext.SaveChangesAsync();
                return "Department Deleted Successfull.";
            }
            return null;

        }
        
    }
}
