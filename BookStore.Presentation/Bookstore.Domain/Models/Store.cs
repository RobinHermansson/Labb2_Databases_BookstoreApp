using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class Store
{
    public int Id { get; set; }

    public string StoreName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string City { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string WebpageUrl { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual ICollection<InventoryBalance> InventoryBalances { get; set; } = new List<InventoryBalance>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
