using System;
using System.Collections.Generic;

namespace LocalServerWebApiApplication.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public virtual List<Employee> Employees { get; set; }
}
