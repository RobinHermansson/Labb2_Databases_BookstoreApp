using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class TotalSalesPerPublisher
{
    public string PublisherName { get; set; } = null!;

    public string? Country { get; set; }

    public int? TotalTitles { get; set; }

    public int? TotalOrderItems { get; set; }

    public int? TotalUnitsSold { get; set; }

    public decimal? TotalRevenue { get; set; }

    public decimal? AverageOrderValue { get; set; }

    public DateTime? MostRecentSale { get; set; }
}
