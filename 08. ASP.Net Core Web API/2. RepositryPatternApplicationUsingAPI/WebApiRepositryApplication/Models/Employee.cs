using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace WebApiRepositryApplication.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public double Salary { get; set; }

    public DateTime DateOfBirth { get; set; }

    public int DepartmentId { get; set; }
    [JsonIgnore]
    [IgnoreDataMember]
    public virtual Department? Department { get; set; } = null!;
}
