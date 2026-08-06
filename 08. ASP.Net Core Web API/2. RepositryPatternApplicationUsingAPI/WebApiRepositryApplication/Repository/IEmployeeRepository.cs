using WebApiRepositryApplication.Models;

namespace WebApiRepositryApplication.Repository
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetEmployeeById(int  employeeId);
        Task<IEnumerable<Employee>?> GetEmployeeList();
        Task<Employee?> PostEmployee(Employee employee);
        Task<Employee?> PutEmployee(int id,Employee employee);
        Task<string?> DeleteEmployee(int employeeId);
    }
}
