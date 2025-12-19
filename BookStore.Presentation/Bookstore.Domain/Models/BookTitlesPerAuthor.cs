using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class BookTitlesPerAuthor
{
    public string FullName { get; set; } = null!;

    public string Age { get; set; } = null!;

    public string Titles { get; set; } = null!;

    public string InventoryValue { get; set; } = null!;
}
