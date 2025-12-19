using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int StoreId { get; set; }

    public DateTime? OrderDate { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Store Store { get; set; } = null!;
}
