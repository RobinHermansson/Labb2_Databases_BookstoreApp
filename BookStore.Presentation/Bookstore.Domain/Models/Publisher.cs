using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class Publisher
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Country { get; set; }

    public string Email { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
