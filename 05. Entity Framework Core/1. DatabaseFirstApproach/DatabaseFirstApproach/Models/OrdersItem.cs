using System;
using System.Collections.Generic;

namespace DatabaseFirstApproach.Models;

public partial class OrdersItem
{
    public int OrderId { get; set; }

    public decimal? TotalPrice { get; set; }
}
