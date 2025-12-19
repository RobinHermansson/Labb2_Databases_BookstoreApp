using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class BookSalesIntelligence
{
    public string Isbn13 { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string AuthorName { get; set; } = null!;

    public int? TotalUnitsSold { get; set; }

    public decimal? TotalRevenue { get; set; }

    public decimal? AverageSellingPrice { get; set; }

    public decimal CurrentPrice { get; set; }

    public int? UniqueCustomers { get; set; }

    public int? StoresWithSales { get; set; }

    public DateTime? LastSaleDate { get; set; }
}
