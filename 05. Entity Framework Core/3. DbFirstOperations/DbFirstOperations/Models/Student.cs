using System;
using System.Collections.Generic;

namespace DbFirstOperations.Models;

public partial class Student
{
    public int RollNo { get; set; }

    public string? Name { get; set; }

    public string? Division { get; set; }

    public decimal? Marks { get; set; }

    public int? AddressId { get; set; }

    public virtual StudentAddress? Address { get; set; }
}
