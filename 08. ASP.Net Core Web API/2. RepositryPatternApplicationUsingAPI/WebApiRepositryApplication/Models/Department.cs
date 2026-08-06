using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace WebApiRepositryApplication.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    //[JsonIgnore]
    //[IgnoreDataMember]
    public virtual ICollection<Employee?> Employees { get; set; } = new List<Employee?>();
}
