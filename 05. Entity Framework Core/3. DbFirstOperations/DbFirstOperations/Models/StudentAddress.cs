using System;
using System.Collections.Generic;

namespace DbFirstOperations.Models;

public partial class StudentAddress
{
    public int AddressId { get; set; }

    public string? City { get; set; }

    public string? Locality { get; set; }

    public string? Pincode { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
