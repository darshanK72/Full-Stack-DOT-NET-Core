using System;
using System.Collections.Generic;

namespace DatabaseFirstApproach.Models;

public partial class ProductView
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;
}
