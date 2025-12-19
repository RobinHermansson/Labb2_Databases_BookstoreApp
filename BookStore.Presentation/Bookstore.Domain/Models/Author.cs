using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class Author
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }

    public DateOnly? DeathDate { get; set; }

    public virtual ICollection<Book> BookIsbn13s { get; set; } = new List<Book>();
}
