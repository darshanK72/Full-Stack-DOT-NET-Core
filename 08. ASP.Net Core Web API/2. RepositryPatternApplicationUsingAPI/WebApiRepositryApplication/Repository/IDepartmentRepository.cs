using WebApiRepositryApplication.Models;

namespace WebApiRepositryApplication.Repository
{
    public interface IDepartmentRepository
    {
        Task<Department?> GetDepartmentById(int departmentId);
        Task<IEnumerable<Department>?> GetDepartmentList();
        Task<Department?> PostDepartment(Department department);
        Task<Department?> PutDepartment(int id,Department department);
        Task<string?> DeleteDepartment(int departmentId);
    }
}
