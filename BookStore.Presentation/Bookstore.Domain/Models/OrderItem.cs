using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string Isbn13 { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public virtual Book Isbn13Navigation { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
