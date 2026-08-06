using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitectureApp.Application.DTOs
{
    public class EmployeeDTO
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public int? Age { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Guid? AddressId { get; set; }
        public Guid? ManagerId { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}
