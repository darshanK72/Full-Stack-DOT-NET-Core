using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace IdentityWebApplication.Models;
public partial class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
