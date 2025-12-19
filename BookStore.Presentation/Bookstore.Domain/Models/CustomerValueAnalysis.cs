using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class CustomerValueAnalysis
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public int? TotalOrders { get; set; }

    public int? TotalItemsPurchased { get; set; }

    public decimal? TotalSpent { get; set; }

    public decimal? AverageOrderValue { get; set; }

    public DateTime? LastPurchaseDate { get; set; }

    public int? DaysSinceLastPurchase { get; set; }
}
