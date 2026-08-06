using System;
using System.Collections.Generic;

namespace DatabaseFirstApproach.Models;

public partial class StaffSale
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int? Year { get; set; }

    public decimal? Amount { get; set; }
}
