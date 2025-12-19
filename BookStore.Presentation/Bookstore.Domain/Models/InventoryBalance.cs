using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class InventoryBalance
{
    public int StoreId { get; set; }

    public string Isbn13 { get; set; } = null!;

    public int Quantity { get; set; }

    public virtual Book Isbn13Navigation { get; set; } = null!;

    public virtual Store Store { get; set; } = null!;
}
